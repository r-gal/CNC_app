using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static CNC3.MsgData;

namespace CNC3
{
    public class MsgData
    {
        public enum ConOrderCode_et
        {
            OC_RESET,
            OC_GETSTAT,
            OC_RUN_LINE,
            OC_RUN_ARC,
            OC_RUN_ARC2,
            OC_RUN_DELAY,
            OC_RUN_AUTOBASE,
            OC_RUN_STOP,
            OC_RUN_PAUSE,
            OC_RUN_PROBE,
            OC_STOP,
            OC_SET_BASE,
            OC_SET_CONFIG,
            OC_SET_SPINDLE_SPEED,
            OC_SET_ZERO,
            OC_PROBE_ACK, /* unused */
            OC_MANUAL_MOVE_START,
            OC_MANUAL_MOVE_STOP,
            OC_RESULT_ACK,
            OC_SUFRACE_OFFSET,
            OC_RUN_BULK


        };
        public ConOrderCode_et orderCode;
        public int words;

        public Int32[] data;

        public MsgData(int words_)
        {
            words = words_;
            if (words_ > 0)
            {
                data = new Int32[words];
            }
        }
    };

    internal class Conn
    {
        Socket client;
        Form1 form;


        public delegate void CallbackEventHandler(int size, byte[] data);
        public event CallbackEventHandler RecvCallback;




        public Conn(Form1 form_)
        {
            form = form_;
        }
        public class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();

            public Form1 formHandle;
            public Conn connHandle;
        }

        public bool ConnectUdp(IPAddress ipAddress, int port)
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

                client = new Socket(
                                            ipEndPoint.AddressFamily,
                                            SocketType.Dgram,
                                            ProtocolType.Udp);

                client.Connect(ipEndPoint);
                client.ReceiveTimeout = 100;

                return true;
            }
            catch
            {
                return false;
            }
        }


        public MsgData SendAndReceiveUdp(MsgData sendMsg )
        {
            int bytes = 4 + 4 * sendMsg.words;

            byte[] msgBuffer = new byte[bytes];

            msgBuffer[0] = (byte)sendMsg.orderCode;
            msgBuffer[1] = (byte)sendMsg.words;
            msgBuffer[2] = 0; /* spare  */
            msgBuffer[3] = 0; /* spare  */

            for(int i = 0; i < sendMsg.words; i++)
            {
                int offset = 4 + i * 4;

                byte[] bytesTmp = BitConverter.GetBytes(sendMsg.data[i]);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytesTmp);

                bytesTmp.CopyTo(msgBuffer, offset);
            }


            if (msgBuffer.Length > 0)
            {
                byte[] msgBufferRx = new byte[256];
                try
                {
                    

                    client.Send(msgBuffer,bytes,SocketFlags.None);

                    int recBytes;

                    recBytes = client.Receive(msgBufferRx);

                    if (recBytes >= 4)
                    {
                        int words = (recBytes - 4) / 4;

                        MsgData recMsg = new MsgData(words);

                        recMsg.orderCode = (ConOrderCode_et)msgBufferRx[0];

                        
                        for (int i = 0; i < words; i++)
                        {
                            int offset = 4 + i * 4;


                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(msgBufferRx, offset,4);

                            recMsg.data[i] = BitConverter.ToInt32(msgBufferRx, offset);

                        }
                        return recMsg;
                    }
                    else
                    {
                        return null;
                    }


                }
                catch
                {
                    return null;
                }

            }
            return null;
        }






        public bool Connect(IPAddress ipAddress, int port)
        {

            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

            client = new Socket(
                                        ipEndPoint.AddressFamily,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);

            client.Connect(ipEndPoint);




            return client.Connected;
                        

            //client.Receive(recMsgByffer);



            //textBox1.Text += Encoding.ASCII.GetString(recMsgByffer);


        }

        public void Send(string msg)
        {
            byte[] msgBuffer = Encoding.ASCII.GetBytes(msg);

            byte[] recMsgByffer = new byte[1000];

            if (msgBuffer.Length > 0)
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                //state.textBoxRec = textBox2;
                state.connHandle = this;
                state.formHandle = form;

                client.BeginReceive(state.buffer, 0, 1000, SocketFlags.None, ReceiveCallback, state);



                client.Send(msgBuffer);

                //textBox2.Text = "";
                //client.Receive(recMsgByffer);





                //var received = await client.ReceiveAsync(recMsgByffer, SocketFlags.None);
            }


            //textBox2.Text += Encoding.ASCII.GetString(recMsgByffer);
        }

        public void Disconnect()
        {
            client.Close();


        }

        public void ReadData(byte[] buffer, int bytesRead)
        {
            RecvCallback(bytesRead, buffer);
            //string msg = Encoding.ASCII.GetString(buffer);
            //textBox2.Text = msg;

        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            string msg = Encoding.ASCII.GetString(state.buffer);
            if (bytesRead > 0)
            {


                IAsyncResult result = state.formHandle.BeginInvoke(new EventHandler(delegate
                {
                    state.connHandle.ReadData(state.buffer, bytesRead);
                    //state.textBoxRec.Text += msg;
                }));

                state.formHandle.EndInvoke(result);
            }
        }
    }
}
