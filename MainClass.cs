using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CNC3
{
    public class Constants
    {
        public const int NO_OF_AXES = 4;
        public const int MOVE_BUFFOR_LENGTH = 60;
        public const int PIPELINE_LENGTH = 8;
        public const double ZeroLen = 0.021;
        public const int GRANULARITY = 10;
        public const int NO_OF_MACROS = 6;
        public const int QUEUE_SIZE = 10;
    };



    public class CommonCnc
    {

        public enum PlaneMode_et
        {
            PLANE_XY,
            PLANE_ZX,
            PLANE_YZ,
        };

        public enum PosMode_et
        {
            ABS,
            REL
        };

    };
    /*
#define STATUS_BIT_NOT_INIT       0x00001000
#define STATUS_BIT_NOT_BASED      0x00002000
#define STATUS_BIT_STOPPED        0x00004000
#define STATUS_BIT_X_LIM          0x00008000
#define STATUS_BIT_Y_LIM          0x00010000
#define STATUS_BIT_Z_LIM          0x00020000
#define STATUS_BIT_A_LIM          0x00040000
#define STATUS_BIT_B_LIM          0x00080000
#define STATUS_BIT_ESTOP          0x00100000
#define STATUS_BIT_PROBE_ERROR    0x00200000

#define STATUS_BIT_PROBE_END      0x01000000
#define STATUS_BIT_MMOVE_END      0x02000000
*/
    static class STATUS_BIT
    {
        public const int STATUS_BIT_NOT_INIT    = 0x00001000;
        public const int STATUS_BIT_NOT_BASED   = 0x00002000;
        public const int STATUS_BIT_STOPPED     = 0x00004000;
        public const int STATUS_BIT_X_LIM       = 0x00008000;
        public const int STATUS_BIT_Y_LIM       = 0x00010000;
        public const int STATUS_BIT_Z_LIM       = 0x00020000;
        public const int STATUS_BIT_A_LIM       = 0x00040000;
        public const int STATUS_BIT_B_LIM       = 0x00080000;
        public const int STATUS_BIT_ESTOP       = 0x00100000;
        public const int STATUS_BIT_PROBE_ERROR = 0x00200000;
        public const int STATUS_BIT_PROBE_END   = 0x01000000;
        public const int STATUS_BIT_MMOVE_END   = 0x02000000;
        public const int STATUS_BIT_PROBE_RESULT= 0x04000000;
        public const int STATUS_BIT_BASE_RESULT = 0x08000000;

        public const int STATUS_BIT_RESULTS = 0x0F000000;
        /* public const int STATUS_BITS_FAULTS = 0x000FFF00;*/
    }  

    internal class MainClass
    {

        Conn conn;
        internal MoveController moveController;
        public cGodeCompiller compiller;
        public Executor executor;




        public delegate void CallbackEventHandler(string msg);
        public event CallbackEventHandler TestCallback;

        
        public delegate void PrintPositionCallbackType(double x, double y, double z, double a);
        public event PrintPositionCallbackType PrintPositionCallback;
        public event PrintPositionCallbackType PrintPositionLocalCallback;
        public event PrintPositionCallbackType PrintPositionCallbackReal;

        public delegate void ConnectionStatusCallbackType(bool status, int stateBitMap);
        public event ConnectionStatusCallbackType ConnectionStatusCallback;

        public delegate void PipelineStatusCallbackType(string statusString);
        public event PipelineStatusCallbackType PipelineStatusCallback;

        public delegate void SeqNumbersStatusCallbackType(int execSeqNo, int actSeqNo);
        public event SeqNumbersStatusCallbackType SeqNumbersStatusCallback;

        public delegate void CallbackErrorCallback(string errorMsg);
        public static event CallbackErrorCallback ErrorCallback;
        
        public delegate string CallbackGetLine(int lineNo);
        public static event CallbackGetLine GetLineCallback;

        public delegate string[] GetLinesArrayCallbackType();
        public static event GetLinesArrayCallbackType GetLinesArrayCallback;
        
        public delegate int CallbackGetNoOfLines();
        public static event CallbackGetNoOfLines GetNoOfLinesCallback;

        public delegate void CallbackSetProgressType(int progress, string text);
        public static event CallbackSetProgressType CallbackSetProgress;


        static string progressText = "";
        public static void SetProgress(int progress)
        {
            CallbackSetProgress(progress, progressText);
        }
        public static void SetProgress(int progress,string text)
        {
            progressText = text;
            CallbackSetProgress(progress,text);
        }

        private void TimerEventProcessor(Object myObject,
                                        EventArgs myEventArgs)
        {
            /* Send status request */
            MsgData sendMsg = new MsgData(0);
            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_GETSTAT;
            MsgData result = conn.SendAndReceiveUdp(sendMsg);
            if(result == null)
            {
                ConnectionStatusCallback(false, 0xFFFF);
            }
            else
            {

                int status = 0xFFFF;
                if (result.words >= 8)
                {
                    double[] tmpPos = new double[4];
                    int[] intPos = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        intPos[i] = result.data[i];
                        tmpPos[i] = ((double)result.data[i]) / 1000;
                    }

                    status = result.data[6];

                    if (status != 0)
                    {

                        if ((status & STATUS_BIT.STATUS_BIT_NOT_INIT) != 0)
                        {
                            SendConfig();

                        }

                        


                    }

                    PrintPositionCallbackReal(tmpPos[0], tmpPos[1], tmpPos[2], tmpPos[3]);
                    Coord realPos = new Coord(intPos[0], intPos[1], intPos[2], intPos[3]);
                    moveController.SaveRealPosition(realPos);

                    moveController.UpdateStatus(status);
                    moveController.UpdateActSeqNo(result.data[5], result.data[7]);

                    SeqNumbersStatusCallback(result.data[5], result.data[7]);



                    if ((status & STATUS_BIT.STATUS_BIT_RESULTS) != 0)
                    {
                        MsgData ackMsg = new MsgData(1);
                        ackMsg.orderCode = MsgData.ConOrderCode_et.OC_RESULT_ACK;
                        ackMsg.data[0] = status & STATUS_BIT.STATUS_BIT_RESULTS;
                        conn.SendAndReceiveUdp(ackMsg);
                    }
 
                }
                else
                {
                    status = 0xFFF8;
                }
                
                ConnectionStatusCallback(true, status);

            }
            

            Coord actPos = executor.GetActPos();
            PrintPositionCallback(0.001 * actPos.x, 0.001 * actPos.y, 0.001 * actPos.z, 0.001 * actPos.a);
            Coord actPosLocal = executor.GetActPosLocal();
            PrintPositionLocalCallback(0.001 * actPosLocal.x, 0.001 * actPosLocal.y, 0.001 * actPosLocal.z, 0.001 * actPosLocal.a);
        }

        public void Init()
        {
            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(TimerEventProcessor);

            if (Config.configData.autoConnect) 
            { 
                conn.ConnectUdp(Config.configData.ip, Config.configData.port);
                timer.Start();
            }



        }

        

        void SendConfig()
        {
            int axeConfSize = 4;
            int offset2 = Constants.NO_OF_AXES * axeConfSize;

            MsgData sendMsg = new MsgData(offset2 + 5);
            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_SET_CONFIG;

            

            for(int axe = 0; axe < Constants.NO_OF_AXES;axe++)
            {
                int offset = axe * axeConfSize;
                sendMsg.data[offset]     = (int)Config.configData.scale[axe];

                int param = 0;
                if (Config.configData.ena[axe]) { param |= 0x01; }
                if (Config.configData.dir[axe]) { param |= 0x02; }

                param |= ((int)Config.configData.limMode[axe])<<8;
                param |= ((int)Config.configData.limType[axe]) << 12;
                param |= ((int)Config.configData.baseType[axe]) << 16;

                sendMsg.data[offset + 1] = param;

                sendMsg.data[offset + 2] = (int)(Config.configData.maxSpeed[axe] * 1000);
                sendMsg.data[offset + 3] = (int)(Config.configData.maxAcceleration[axe] * 1000);
            }

            

            sendMsg.data[offset2] = (int)Config.configData.eStopMode;
            sendMsg.data[offset2+1] = (int)Config.configData.probeMode;
            sendMsg.data[offset2+2] = (int)(Config.configData.minSpeed * 1000);
            sendMsg.data[offset2+3] = (int)(Config.configData.autoBaseSpeed * 1000);
            sendMsg.data[offset2+4] = (int)Config.configData.maxSpindleSpeed;

            MsgData result = conn.SendAndReceiveUdp(sendMsg);


        }

        public void ErrorHandler(string  ErrorMsg)
        {

        }

        void SetPipelineStaus(string statStr_)
        {
            PipelineStatusCallback(statStr_);
        }

        public MainClass(Form1 form_)
        {
            Config.ConfigChangedCallback += SendConfig;
            Config.LoadConfig();
            conn = new Conn(form_);
            conn.RecvCallback += new Conn.CallbackEventHandler(MsgReceivedCallback);
       

            compiller = new cGodeCompiller();
            executor = new Executor(compiller);
            moveController = new MoveController(conn, executor);

            moveController.PipelineStatusCallback += new MoveController.PipelineStatusCallbackType(SetPipelineStaus);



        }
        public enum Action_ET
        {
            Eth_Connect,
            Eth_Disconnect,
            Eth_Test,

            AxeRun_StartCont,
            AxeRun_StopCont,
            AxeRun_Run,

            CodeRun_Start,
            CodeRun_Stop,
            CodeRun_Resume,
            CodeRun_RunFromLine,
            CodeRun_Reset,

            CodeCompile,
            CodeSimulate,

            AxeBase_SetOffset,
            AxeBase_Autobase,
            AxeBase_SetZero,
            AxeBase_SetLocal,
            AxeBase_ClearOffsets,
            AxeBase_RunMacro,
            AxeBase_GoTo

        };

        string[] GetMainCode()
        {
            return GetLinesArrayCallback();
            /*
            int size = GetNoOfLinesCallback();
            string[] array = new string[size];
            MainClass.SetProgress(0,"Loading");
            for(int i=0;i<size;i++)
            {
                array[i] = GetLineCallback(i);
                MainClass.SetProgress((i*100)/size);
            }
            MainClass.SetProgress(100);
            return array;*/
        }






        public void Action(Action_ET action, Object parameters)
        {
            string[] codeFile;
            switch (action)
            { 
                case Action_ET.Eth_Connect:  conn.ConnectUdp(Config.configData.ip, Config.configData.port);  break;
                case Action_ET.Eth_Disconnect: conn.Disconnect();  break;
                case Action_ET.Eth_Test: TestComm(); break;

                case Action_ET.AxeRun_StartCont: moveController.ManualMoveStart((ManualMoveData)parameters); break;
                case Action_ET.AxeRun_StopCont: moveController.ManualMoveStop((ManualMoveData)parameters); break;
                case Action_ET.AxeRun_Run: moveController.ManualMove((ManualMoveData)parameters); break;

                case Action_ET.CodeRun_Start:
                    codeFile = GetMainCode();
                    moveController.StartCode(codeFile,null); 
                    break;
                case Action_ET.CodeRun_Stop: moveController.StopCode(); break;
                case Action_ET.CodeRun_Resume: moveController.ResumeCode(); break;
                case Action_ET.CodeRun_RunFromLine:
                    codeFile = GetMainCode();
                    moveController.StartCode(codeFile, null);
                    break;
                case Action_ET.CodeRun_Reset: moveController.Reset(); break;

                case Action_ET.CodeCompile:
                    compiller.ClearAll();
                    codeFile = GetMainCode();
                    compiller.RunCompilation(codeFile);
                    break;

                case Action_ET.CodeSimulate:
                    codeFile = GetMainCode();
                    moveController.RunSimulation(true,codeFile);
                    break;

                case Action_ET.AxeBase_SetOffset: moveController.SetBase((SetBaseData)parameters); break;
                case Action_ET.AxeBase_Autobase: moveController.SendAutoBase(); break;
                case Action_ET.AxeBase_SetZero: moveController.SendSetZero(); break;
                case Action_ET.AxeBase_SetLocal: moveController.SetBase(null); break;
                case Action_ET.AxeBase_ClearOffsets:
                    executor.ClearOffsets();
                    break;
                case Action_ET.AxeBase_RunMacro:
                    codeFile = moveController.GetMacroCode((string) parameters,false);
                    if (codeFile != null)
                    {
                        moveController.StartCode(codeFile, null);
                    }
                    break;

                case Action_ET.AxeBase_GoTo: moveController.GoTo((GoToData)parameters); break;

                default: break;
            }
        }

        public void TestComm()
        {
            



            MsgData sendMsg = new MsgData(0);
            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_GETSTAT;

            MsgData result = conn.SendAndReceiveUdp(sendMsg);

            if(result == null)
            {
                TestCallback("result = -1");
            }
 
            else
            {
                

            }
        }

        public void MsgReceivedCallback(int size, byte[] data)
        {
            string text = Encoding.ASCII.GetString(data);
            TestCallback(text);
        }
    }
}
