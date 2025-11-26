using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CNC3.MoveController;

namespace CNC3
{
    internal class TXQueue
    {
        Conn conn;
        SurfaceProbes surfaceProbes;


        public delegate void CallbackErrorCallback(string errorMsg);
        public static event CallbackErrorCallback ErrorCallback;

        MsgData txMsg = null;
        int txMsgCnt;

        public TXQueue(Conn conn_, SurfaceProbes surfaceProbes_)
        {
            this.conn = conn_;
            this.surfaceProbes = surfaceProbes_;
        }
        internal void FlushSendQueue()
        {
            if(txMsg != null && txMsgCnt > 0)
            {

                //ErrorCallback("Send " + txMsgCnt.ToString() + "messages\n");
                txMsg.words = 1 + 20 * txMsgCnt;
                txMsg.data[0] = txMsgCnt;
                conn.SendAndReceiveUdp(txMsg);

                txMsg = null;
                txMsgCnt = 0;
            }

        }

        internal void Clear()
        {
            txMsgCnt = 0;
        }

        internal void QueueMessage(MoveData moveData)
        {
            if(txMsg == null)
            {
                txMsgCnt = 0;
                txMsg = new MsgData(1 + 20 * Constants.QUEUE_SIZE);
                txMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_BULK;
            }

            int offset = 1 + txMsgCnt * 20;
            txMsg.data[offset + 0] = (int)moveData.orderCode;


            switch (moveData.orderCode)
            {
                case MoveData.orderCode_et.LINE:
                    QueueLine(moveData, false);
                    break;
                case MoveData.orderCode_et.ARC2:
                    QueueArc2(moveData);
                    break;
                case MoveData.orderCode_et.DELAY:
                    QueueDelay(moveData);
                    break;
                case MoveData.orderCode_et.SPINDLE_SPEED:
                    QueueSpindleSpeed(moveData);
                    break;
                case MoveData.orderCode_et.STOP:
                    QueueStop(moveData);
                    break;
                case MoveData.orderCode_et.PAUSE:
                    QueuePause(moveData);
                    break;
                case MoveData.orderCode_et.PROBE:
                    QueueProbe(moveData);
                    break;
                case MoveData.orderCode_et.PAUSE_TOOL_CHANGE:
                    QueuePause(moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_INIT:
                    QueueSurfaceOffset(SurfaceOffsetOrderCode_et.INIT, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_SET:
                    QueueSurfaceOffset(SurfaceOffsetOrderCode_et.SET_POINT, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_ACTIVATE:
                    QueueSurfaceOffset(SurfaceOffsetOrderCode_et.ACTIVATE, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_DEACTIVATE:
                    QueueSurfaceOffset(SurfaceOffsetOrderCode_et.DEACTIVATE, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_CLEAR:
                    QueueSurfaceOffset(SurfaceOffsetOrderCode_et.CLEAR, moveData);
                    break;
                default:
                    
                    break;

            }



            txMsgCnt++;

            if (txMsgCnt >= Constants.QUEUE_SIZE)
            {
                FlushSendQueue();
            }

        }

        private void QueueLine(MoveData moveData, bool ignoreLimiters)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;

            txMsg.data[offset + 2] = (int)(moveData.startSpeed * 1000);
            txMsg.data[offset + 3] = (int)(moveData.endSpeed * 1000);
            txMsg.data[offset + 4] = (int)(moveData.maxSpeed * 1000);
            txMsg.data[offset + 5] = (int)(moveData.maxAcceleration * 1000);
            txMsg.data[offset + 6] = moveData.endPoint.x;
            txMsg.data[offset + 7] = moveData.endPoint.y;
            txMsg.data[offset + 8] = moveData.endPoint.z;
            txMsg.data[offset + 9] = moveData.endPoint.a;
            txMsg.data[offset + 10] = ignoreLimiters ? 1 : 0;

            //ErrorCallback("SEND Line: (" + txMsg.data[offset + 6].ToString() + "," + txMsg.data[offset + 7].ToString() + "," + txMsg.data[offset + 8].ToString() + "," + txMsg.data[offset + 9].ToString() + "\n");
            //ErrorCallback("vs=" + txMsg.data[offset + 2].ToString() + " ve=" + txMsg.data[offset + 3].ToString() + " vMax=" + txMsg.data[offset + 4].ToString() + " aMax=" + txMsg.data[offset + 5].ToString() + " SEQ= " + txMsg.data[offset + 1].ToString() + "\n");
        }

        private void QueueArc2(MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;

            txMsg.data[offset + 2] = (int)(moveData.startSpeed * 1000);
            txMsg.data[offset + 3] = (int)(moveData.endSpeed * 1000);
            txMsg.data[offset + 4] = (int)(moveData.maxSpeed * 1000);
            txMsg.data[offset + 5] = (int)(moveData.maxAcceleration * 1000);

            txMsg.data[offset + 6] = moveData.endPoint.x;
            txMsg.data[offset + 7] = moveData.endPoint.y;
            txMsg.data[offset + 8] = moveData.endPoint.z;
            txMsg.data[offset + 9] = moveData.endPoint.a;

            txMsg.data[offset + 10] = moveData.centrePoint.x;
            txMsg.data[offset + 11] = moveData.centrePoint.y;
            txMsg.data[offset + 12] = moveData.centrePoint.z;

            txMsg.data[offset + 13] = (int)(moveData.rotationAxeVector.X * 1000);
            txMsg.data[offset + 14] = (int)(moveData.rotationAxeVector.Y * 1000);
            txMsg.data[offset + 15] = (int)(moveData.rotationAxeVector.Z * 1000);

            txMsg.data[offset + 16] = moveData.turns;

            //ErrorCallback("SEND Arc2: (" + txMsg.data[offset + 6].ToString() + "," + txMsg.data[offset + 7].ToString() + "," + txMsg.data[offset + 8].ToString() + "," + txMsg.data[offset + 9].ToString() + " SEQ= " + txMsg.data[offset + 0].ToString() + "\n");
            //ErrorCallback("centre: (" + sendMsg.data[10].ToString() + "," + sendMsg.data[11].ToString() + "," + sendMsg.data[12].ToString() + "\n");
            //ErrorCallback("rotVecotr: (" + sendMsg.data[13].ToString() + "," + sendMsg.data[14].ToString() + "," + sendMsg.data[15].ToString() + "\n");
            //ErrorCallback("vs=" + sendMsg.data[2].ToString() + " ve=" + sendMsg.data[3].ToString() + " vMax=" + sendMsg.data[4].ToString() + " aMax=" + sendMsg.data[5].ToString() + "\n");

        }

        private void QueueDelay(MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;

            txMsg.data[offset + 2] = moveData.delay;

            ErrorCallback("SEND Dwel " + txMsg.data[offset + 2].ToString() + "ms SEQ=" + moveData.seqNo + "\n");
        }

        private void QueueSpindleSpeed(MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;

            txMsg.data[offset + 2] = moveData.speed;

            ErrorCallback("SEND M3 " + txMsg.data[offset + 2].ToString() + "prom  SEQ=" + moveData.seqNo + "\n");

        }

        private void QueueStop(MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;
            ErrorCallback("SEND STOP CODE  \n");
        }

        private void QueuePause(MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;

            ErrorCallback("SEND PAUSE CODE  SEQ=" + moveData.seqNo + "\n");
        }

        private void QueueProbe(MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            txMsg.data[offset + 1] = moveData.seqNo;

            txMsg.data[offset + 2] = moveData.axe;
            txMsg.data[offset + 3] = moveData.probeLength;
            txMsg.data[offset + 4] = moveData.mode;
            txMsg.data[offset + 5] = (int)(moveData.maxSpeed * 1000);
            txMsg.data[offset + 6] = (int)(moveData.maxAcceleration * 1000);

            ErrorCallback("SEND PROBE CODE: vMax = " + txMsg.data[offset + 5].ToString() + " aMax = " + txMsg.data[offset + 6].ToString() + "\n");
        }

        private void QueueSurfaceOffset(SurfaceOffsetOrderCode_et orderCode, MoveData moveData)
        {
            int offset = 1 + txMsgCnt * 20;

            if(moveData ==null)
            {
                if(orderCode == SurfaceOffsetOrderCode_et.INIT || orderCode == SurfaceOffsetOrderCode_et.SET_POINT)
                {
                    return;
                }
                else
                {
                    txMsg.data[offset + 1] = -1;
                }


            }
            else
            {
                txMsg.data[offset + 1] = moveData.seqNo;
            }

            



            switch (orderCode)
            {
                case SurfaceOffsetOrderCode_et.INIT:
                    txMsg.data[offset + 2] = moveData.surfOff_xSize;
                    txMsg.data[offset + 3] = moveData.surfOff_ySize;
                    txMsg.data[offset + 4] = moveData.surfOff_xStep;
                    txMsg.data[offset + 5] = moveData.surfOff_yStep;
                    txMsg.data[offset + 6] = moveData.surfOff_xStart;
                    txMsg.data[offset + 7] = moveData.surfOff_yStart;

                    surfaceProbes.Clear();
                    surfaceProbes.Init(moveData.surfOff_xSize, moveData.surfOff_ySize, moveData.surfOff_xStep, moveData.surfOff_yStep, moveData.surfOff_xStart, moveData.surfOff_yStart);
                    break;
                case SurfaceOffsetOrderCode_et.SET_POINT:
                    txMsg.data[offset + 2] = moveData.endPoint.x;
                    txMsg.data[offset + 3] = moveData.endPoint.y;
                    txMsg.data[offset + 8] = moveData.endPoint.z;

                    ErrorCallback("set point, x=" + txMsg.data[offset + 2].ToString() + " y= " + txMsg.data[offset + 3].ToString() + " val= " + txMsg.data[offset + 8].ToString() + " \n");

                    surfaceProbes.SetProbe(txMsg.data[offset + 2], txMsg.data[offset + 3], txMsg.data[offset + 8]);
                    break;
                default:
                    break;

            }

            ErrorCallback("SEND Surface offset " + orderCode.ToString() + " \n");
        }




        internal void SendLine(MoveData moveData, bool ignoreLimiters)
        {
            if (txMsg == null)
            {
                txMsgCnt = 0;
                txMsg = new MsgData(1 + 20);
                txMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_BULK;
            }

            QueueLine(moveData, ignoreLimiters);
            txMsg.data[1] = (int)MoveData.orderCode_et.LINE;
            txMsgCnt++;
            FlushSendQueue();


        }


        internal void SendSurfaceOffset(SurfaceOffsetOrderCode_et orderCode, MoveData moveData)
        {
            if (txMsg == null)
            {
                txMsgCnt = 0;
                txMsg = new MsgData(1 + 20);
                txMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_BULK;
            }

            QueueSurfaceOffset(orderCode, moveData);
            txMsg.data[1] = (int)orderCode;
            txMsgCnt++;
            FlushSendQueue();

            
        }




    }
}
