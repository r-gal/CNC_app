

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Media3D;
using System.CodeDom;
using static CNC3.ManualMoveData;
using myCnc2;
using System.IO;

namespace CNC3
{



    public class ManualMoveData 
    {
        internal enum Axe_et
        {
            axe_x,
            axe_y,
            axe_z,
            axe_a,

        };
        internal Axe_et axe;
        internal int dist;/* [um]*/
        internal int speed; /* [um/s] */
        //internal int acceleration; /* [um/s2] */

        internal Coord startPoint;

        internal bool ignoreLimiters;
    };

    class MoveData 
    {
        public double startSpeed;
        public double endSpeed;

        public double maxSpeed;
        public double maxEndSpeed;
        public double maxAcceleration;

        public double length;
        public Vector3D startVector;
        public Vector3D endVector;

        public int seqNo;

        public int surfOff_xSize;
        public int surfOff_ySize;
        public int surfOff_xStep;
        public int surfOff_yStep;
        public int surfOff_xStart;
        public int surfOff_yStart;

        // public Coord moveVector;
        //public Coord centrePointVector;

        public Coord startPoint;
        public Coord centrePoint;
        public Coord endPoint;

        public Vector3D rotationAxeVector;

        public double radius;
        //public CommonCnc.PlaneMode_et planeMode;
        public int turns;
        //public double startAngle;
        //public double endAngle;
        public bool clockwise; /* used only in toolCompensation Calculations */

        public int delay;
        public int speed;
        public bool rapidMove;
        public bool compensated;

        public int axe;
        public int mode;
        public int probeLength;

        public enum orderCode_et
        {
            LINE,
            ARC,
            ARC2,
            DELAY,
            SPINDLE_SPEED,
            PAUSE,
            STOP,
            PROBE,
            ERROR,
            PAUSE_TOOL_CHANGE,
            SUFRACE_OFFSET_INIT,
            SUFRACE_OFFSET_SET,
            SUFRACE_OFFSET_ACTIVATE,
            SUFRACE_OFFSET_DEACTIVATE
        };

        public orderCode_et orderCode;

        public MoveData()
        {
            compensated = false;
        }

        public void Recalc()
        {
            if(orderCode == orderCode_et.LINE)
            {
                /* recalc length */

                Point3D sP = startPoint.GetPoint();                
                Point3D eP = endPoint.GetPoint();
                Vector3D v = sP - eP;
                length = v.Length;

            }
            else if(orderCode == orderCode_et.ARC2)
            {
                /* recalc diameter and length */

                Point3D sP = startPoint.GetPoint();
                Point3D cP = centrePoint.GetPoint() ;
                Point3D eP = endPoint.GetPoint() ;

                double startAngle = Math.Atan2(sP.Y - cP.Y  , sP.X - cP.X);
                double endAngle = Math.Atan2(eP.Y - cP.Y, eP.X - cP.X);

                double rx = sP.X - cP.X;
                double ry = sP.Y - cP.Y;

                radius = Math.Sqrt(rx * rx + ry * ry);

                double l = Math.Abs(radius * (endAngle - startAngle));

                double h = sP.Z - eP.Z;

                double tLength = Math.Sqrt(l * l + h * h);

                length = l;
            }

        }

    };

    internal class SurfaceProbes
    {
        int sizeX;
        int sizeY;
        int stepX;
        int stepY;
        int startX;
        int startY;
        bool active;

        int[,] probes;

        internal int GetSizeX() { return sizeX; }
        internal int GetSizeY() { return sizeY; }

        internal int GetStepX() { return stepX; }
        internal int GetStepY() { return stepY; }

        internal int GetStartX() { return startX; }
        internal int GetStartY() { return startY; }

        internal int GetProbe(int x, int y)
        {
            if (probes != null)
            {
                if ((probes.GetLength(0) > x) && (probes.GetLength(1) > y))
                {
                    return probes[x, y];
                }
            }
            return 0;
        }

        internal void Init(int sizeX, int sizeY, int stepX, int stepY, int posX, int posY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.stepX = stepX;
            this.stepY = stepY;
            this.startX = posX;
            this.startY = posY;
            active = false;
            probes = new int[sizeX, sizeY];
        }

        internal void SetProbe(int x, int y, int val)
        {
            if (probes != null)
            {
                if ((probes.GetLength(0) > x) && (probes.GetLength(1) > y))
                {
                    probes[x, y] = val;
                }
            }
        }

        private void CreateTestArray()
        {
            sizeX = 5;
            sizeY = 6;
            stepX = 10;
            stepY = 10;

            active = true;

            probes = new int[sizeX,sizeY];

            for(int x = 0; x < sizeX;x++)
            {
                for(int y = 0; y < sizeY;y++)
                {
                    probes[x, y] = x + 10 * y;
                }
            }
        }


        internal void Clear()
        {
            sizeX = 0;
            sizeY = 0;
            stepX = 0;
            stepY = 0;
            startX = 0;
            startY = 0;
            active = false;
            probes = null;
        }


        internal SurfaceProbes()
        {
            Clear();
            //CreateTestArray();
        }




    }



    internal class MoveController : CommonCnc
    {
        Conn conn;
        PipelineUnit pipelineUnit;
        Executor executor;
        int sentSeqNo; /* seqNo  sent */
        int actSeqNo; /* act executed seqNo */

        Timer manualMoveTimer = new Timer();
        ManualMoveData manualMoveData;
        Coord realPos;

        Drawer drawer = null;

        internal SurfaceProbes surfaceProbes = new SurfaceProbes();

        public delegate void CallbackErrorCallback(string errorMsg);
        public static event CallbackErrorCallback ErrorCallback;

        public delegate void PipelineStatusCallbackType(string statusString);
        public event PipelineStatusCallbackType PipelineStatusCallback;

        enum State_et
        {
            Idle,
            ManualMove,
            Starting,
            Running,
            Finishing,
            Paused,
            Probing,
            Fault,
            Simulating,
            ManualMoving,
            AutoBase
        };

        State_et state;

        bool isBased = false;

        void SetState(State_et state_)
        {
            state = state_;
            PipelineStatusCallback(state.ToString());
            //ErrorCallback("Set state = " + state.ToString() + "\n");

        }

        public MoveController(Conn conn_,  Executor executor_)
        { 
            conn = conn_;
            pipelineUnit = new PipelineUnit(executor_);
            
            executor = executor_;
            sentSeqNo = 0;
            state = State_et.Idle;  

            manualMoveTimer = new Timer();
            manualMoveTimer.Interval = 250;
            manualMoveTimer.Tick += new EventHandler(ManualMoveTick);


        }
        public void SaveRealPosition(Coord realPos_)
        {
            realPos = realPos_;
        }


        void FillPipeline()
        {
            while (actSeqNo + Constants.MOVE_BUFFOR_LENGTH > sentSeqNo)
            {

                MoveData moveData = pipelineUnit.GetMove(false);
                if (moveData != null)
                {
                    MsgData result = SendMove(moveData);
                    sentSeqNo = moveData.seqNo;
                    if (moveData.orderCode == MoveData.orderCode_et.STOP)
                    {
                        SetState(State_et.Finishing);
                    }
                    if (moveData.orderCode == MoveData.orderCode_et.PAUSE)
                    {
                        SetState(State_et.Paused);
                        break;
                    }
                    if (moveData.orderCode == MoveData.orderCode_et.PROBE)
                    {
                        SetState(State_et.Probing);
                        break;
                    }

                }
                else
                {

                    break;
                }

            }

            int i = 0;
            do
            {
                MoveData moveData = pipelineUnit.GetMove(false);

                if (moveData == null)
                {
                    if (i == 0)
                    {
                        SetState(State_et.Idle);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    i++;
                    MsgData result = SendMove(moveData);
                    sentSeqNo = moveData.seqNo;

                    if (moveData.orderCode == MoveData.orderCode_et.STOP)
                    {
                        SetState(State_et.Finishing);
                        break;
                    }
                    else if (moveData.orderCode == MoveData.orderCode_et.PAUSE)
                    {
                        State_et stateTmp = state;
                        SetState(State_et.Paused);
                        if (WaitForContinue() == false)
                        {
                            SetState(State_et.Idle);
                            return;
                        }
                        else
                        {
                            SetState(stateTmp);
                        }
                        break;
                    }
                    if (moveData.orderCode == MoveData.orderCode_et.PROBE)
                    {
                        SetState(State_et.Probing);
                        break;
                    }

                }
            }
            while (i < Constants.MOVE_BUFFOR_LENGTH);
        }

        public void UpdateActSeqNo(int seqNo_, int lastSentSeqNo_)
        {
            actSeqNo = seqNo_;
            if (state == State_et.Starting)
            {
                if (lastSentSeqNo_ != -1)
                {
                    SetState(State_et.Running);
                }
            }
            else if (state == State_et.Running)
            {
                if (actSeqNo == -1) /* means that buffer in ARM is empty, for example after probing */
                {
                    actSeqNo = sentSeqNo;
                }


                while (actSeqNo + Constants.MOVE_BUFFOR_LENGTH > sentSeqNo)
                {

                    MoveData moveData = pipelineUnit.GetMove(false);
                    if (moveData != null)
                    {
                        MsgData result = SendMove(moveData);
                        sentSeqNo = moveData.seqNo;
                        if (moveData.orderCode == MoveData.orderCode_et.STOP)
                        {
                            SetState(State_et.Finishing);
                        }
                        if (moveData.orderCode == MoveData.orderCode_et.PAUSE)
                        {
                            SetState(State_et.Paused);
                            if (WaitForContinue() == false)
                            {
                                SetState(State_et.Idle);
                                return;
                            }
                            else
                            {
                                SetState(State_et.Running);
                            }
                            break;
                        }
                        else if (moveData.orderCode == MoveData.orderCode_et.PAUSE_TOOL_CHANGE)
                        {
                            SetState(State_et.Paused);
                            if (WaitForToolChange(moveData.mode) == false)
                            {
                                SetState(State_et.Idle);
                                return;
                            }
                            else
                            {
                                SetState(State_et.Running);
                            }
                            break;
                        }
                        if (moveData.orderCode == MoveData.orderCode_et.PROBE)
                        {
                            SetState(State_et.Probing);
                            //ErrorCallback("Probing breake 1\n");
                            break;
                        }

                    }
                    else
                    {

                        break;
                    }

                }
            }
            else if (state == State_et.Finishing)
            {
                if (seqNo_ == -1)
                {
                    SetState(State_et.Idle);

                    MoveData moveData = pipelineUnit.GetMove(false);
                    if (moveData != null)
                    {
                        ErrorCallback("PIPELINE ERROR 3 \n");
                    }
                }
                else
                {
                    /* nothing to do */
                }
            }
            else if (state == State_et.ManualMove)
            {
                if (seqNo_ == -1)
                {
                    SetState(State_et.Idle);
                }
            }
        }


        public void UpdateStatus(int status)
        {

            if ((state != State_et.Idle) && (state != State_et.Fault))
            {
                if ((status & STATUS_BIT.STATUS_BIT_STOPPED) != 0)
                {
                    SetState(State_et.Fault);
                    if ((status & STATUS_BIT.STATUS_BIT_X_LIM) != 0)
                    {                        
                        System.Windows.Forms.MessageBox.Show("Limiter X Reached", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if ((status & STATUS_BIT.STATUS_BIT_Y_LIM) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Limiter Y Reached", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if ((status & STATUS_BIT.STATUS_BIT_Z_LIM) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Limiter Z Reached", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if ((status & STATUS_BIT.STATUS_BIT_A_LIM ) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Limiter A Reached", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if ((status & STATUS_BIT.STATUS_BIT_B_LIM) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Limiter B Reached", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if ((status & STATUS_BIT.STATUS_BIT_ESTOP) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Emergency stop", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                }
                
            }
            if((state == State_et.Probing) )
            {
                if((status & STATUS_BIT.STATUS_BIT_PROBE_END) > 0)
                {
                    executor.SetActPos(realPos);
                    SetState(State_et.Running);

                    executor.SetProbeResult(realPos, ((status & STATUS_BIT.STATUS_BIT_PROBE_RESULT) > 0));
                }
            }
            if ( (state == State_et.ManualMoving))
            {
                if ((status & STATUS_BIT.STATUS_BIT_MMOVE_END) > 0)
                {
                    executor.SetActPos(realPos);
                    SetState(State_et.Idle);
                }
            }
            
            if ( (state == State_et.AutoBase))
            {
                if ((status & STATUS_BIT.STATUS_BIT_BASE_RESULT) > 0)
                {
                    SetState(State_et.Idle);
                    if (Config.configData.zeroMachineAutobase == true)
                    {
                        
                        SendSetZero();
                    }
                    

                }
            }

        }

        public void StartCode(string[] codeFile, Object parameters)
        {
            if (state == State_et.Idle)
            {
                
                bool checkResult = RunSimulation(false, codeFile);
                
                if(checkResult == false)
                {
                    ErrorCallback("Simulation failed, ABORT \n");
                    return;
                }


                int startLine;
                if (parameters == null)
                { 
                    startLine = 0;
                }
                else
                {
                    startLine = (int)parameters;
                }
                SetState(State_et.Starting);

                bool ok = pipelineUnit.PrepareRun(startLine,false, codeFile);

                if(ok == false)
                {
                    return;
                }


                int i = 0;
                do
                {
                    MoveData moveData = pipelineUnit.GetMove(false);

                    if(moveData == null)
                    {
                        if(i == 0)
                        {
                            SetState(State_et.Idle);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        i++; 
                        MsgData result = SendMove(moveData);
                        sentSeqNo = moveData.seqNo;

                        if (moveData.orderCode == MoveData.orderCode_et.STOP)
                        {
                            SetState(State_et.Finishing);
                            break;
                        }
                        else if (moveData.orderCode == MoveData.orderCode_et.PAUSE)
                        {
                            State_et stateTmp = state;
                            SetState(State_et.Paused);
                            if (WaitForContinue() == false)
                            {
                                SetState(State_et.Idle);
                                return;
                            }
                            else
                            {
                                SetState(stateTmp);
                            }
                            break;
                        }
                        else if (moveData.orderCode == MoveData.orderCode_et.PAUSE_TOOL_CHANGE)
                        {
                            State_et stateTmp = state;
                            SetState(State_et.Paused);
                            if (WaitForToolChange(moveData.mode) == false)
                            {
                                SetState(State_et.Idle);
                                return;
                            }
                            else
                            {
                                SetState(stateTmp);
                            }
                            break;
                        }
                        if (moveData.orderCode == MoveData.orderCode_et.PROBE)
                        {
                            SetState(State_et.Probing);
                            //ErrorCallback("Probing breake 2\n");
                            break;
                        }

                    }

                }
                while ( i < Constants.MOVE_BUFFOR_LENGTH);
                
            }
        }

        bool WaitForContinue()
        {
            DialogResult result = MessageBox.Show("Continue?", "Program Paused", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool WaitForToolChange(int toolIdx)
        {
            DialogResult result = MessageBox.Show("Insert tool nr " + (toolIdx/1000).ToString() + " and press OK to continue.", "Program Paused", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StopCode()
        {
            SetState(State_et.Idle);

            int msgLength = 0;
            MsgData sendMsg = new MsgData(msgLength);

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_STOP;


            ErrorCallback("SEND STOP \n");
            conn.SendAndReceiveUdp(sendMsg);

        }

        public void ResumeCode()
        {
            if (state == State_et.Paused)
            {
                SetState(State_et.Starting);

                MoveData moveData = pipelineUnit.GetMove(false);

                if (moveData == null)
                {
                    SetState(State_et.Idle);
                }
                else
                {

                    MsgData result = SendMove(moveData);
                    sentSeqNo = moveData.seqNo;


                    for (int i = 0; i < Constants.MOVE_BUFFOR_LENGTH; i++)
                    {
                        moveData = pipelineUnit.GetMove(false);

                        if (moveData == null)
                        {
                            break;
                        }
                        else
                        {
                            result = SendMove(moveData);
                            sentSeqNo = moveData.seqNo;

                            if (moveData.orderCode == MoveData.orderCode_et.STOP)
                            {
                                SetState(State_et.Finishing);
                            }
                            else if (moveData.orderCode == MoveData.orderCode_et.PAUSE)
                            {
                                SetState(State_et.Paused);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            int msgLength = 0;
            MsgData sendMsg = new MsgData(msgLength);

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RESET;

            ErrorCallback("RESET \n");
            conn.SendAndReceiveUdp(sendMsg);

            SetState(State_et.Idle);
            
            //executor.ClearOffsets();
            executor.SetActPos(realPos);
        }

        public void SetBase(SetBaseData baseData)
        {
            if (state == State_et.Idle)
            {
                executor.SetOffset(baseData);
            }
        }

        public void  SendSetZero()
        {
            if (state == State_et.Idle)
            {
                int msgLength = 4;
                MsgData sendMsg = new MsgData(msgLength);

                sendMsg.orderCode = MsgData.ConOrderCode_et.OC_SET_ZERO;
                for(int i=0;i<Constants.NO_OF_AXES;i++)
                {
                    sendMsg.data[i] = Config.configData.zeroOffset[i];
                }                

                ErrorCallback("SEND SET_ZERO \n");
                conn.SendAndReceiveUdp(sendMsg);

                executor.SetActPos(new Coord(Config.configData.zeroOffset[0], Config.configData.zeroOffset[1], Config.configData.zeroOffset[2], Config.configData.zeroOffset[3]));
            }
        }

        public void GoTo(GoToData data)
        {
            if (state == State_et.Idle)
            {
                string[] codeFile = null;

                if (data.globalPos)
                {
                    codeFile = GetMacroCode("GoMachine_Macro.txt", true);
                }
                else
                {
                    codeFile = GetMacroCode("GoLocal_Macro.txt", true);
                }                                
                
                if (codeFile != null && codeFile.Length > 5)
                {

                    codeFile[1] = "#<xPos> = " + (((double)data.coord.x) * 0.001).ToString();
                    codeFile[2] = "#<yPos> = " + (((double)data.coord.y) * 0.001).ToString();
                    codeFile[3] = "#<zPos> = " + (((double)data.coord.z) * 0.001).ToString();
                    codeFile[4] = "#<zSafe> = " + (((double)data.safeZ) * 0.001).ToString();
                    if(data.globalPos == false)
                    {
                        codeFile[5] = "#<zSafeIsLocal> = 1";
                    }    

                    StartCode(codeFile, null);
                }

            }
        }


        private MsgData SendMove(MoveData moveData)
        {
            MsgData result = null;
            switch (moveData.orderCode)
            {
                case MoveData.orderCode_et.LINE:
                    result = SendLine(moveData,false); 
                    break;
              /*  case MoveData.orderCode_et.ARC:
                    result = SendArc(moveData);
                    break;*/
                case MoveData.orderCode_et.ARC2:
                    result = SendArc2(moveData);
                    break;
                case MoveData.orderCode_et.DELAY:
                    result = SendDelay(moveData);
                    break;
                case MoveData.orderCode_et.SPINDLE_SPEED:
                    result = SendSpindleSpeed(moveData);
                    break;
                case MoveData.orderCode_et.STOP:
                    result = SendStop(moveData); 
                    break;
                case MoveData.orderCode_et.PAUSE:
                    result = SendPause(moveData);
                    break;
                case MoveData.orderCode_et.PROBE:
                    result = SendProbe(moveData);
                    break;
                case MoveData.orderCode_et.PAUSE_TOOL_CHANGE:
                    result = SendPause(moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_INIT:
                    result = SendSurfaceOffset(SurfaceOffsetOrderCode_et.INIT, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_SET:
                    result = SendSurfaceOffset(SurfaceOffsetOrderCode_et.SET_POINT, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_ACTIVATE:
                    result = SendSurfaceOffset(SurfaceOffsetOrderCode_et.ACTIVATE, moveData);
                    break;
                case MoveData.orderCode_et.SUFRACE_OFFSET_DEACTIVATE:
                    result = SendSurfaceOffset(SurfaceOffsetOrderCode_et.DEACTIVATE, moveData);
                    break;
                default:
                    result = null; 
                    break;

            }
            if(result == null)
            {
                ErrorCallback("SENT FAILED \n");
            }
            return result;

        }
        private MsgData SendDelay(MoveData moveData)
        {
            int msgLength = 3;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_DELAY;

            sendMsg.data[0] = moveData.seqNo;
            sendMsg.data[1] = 0; /* no of segments in msg */

            sendMsg.data[2] = moveData.delay;

            ErrorCallback("SEND Dwel " + sendMsg.data[2].ToString() + "ms \n");

            return conn.SendAndReceiveUdp(sendMsg);
        }

        private MsgData SendStop(MoveData moveData)
        {
            int msgLength = 2;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_STOP;

            sendMsg.data[0] = moveData.seqNo;
            sendMsg.data[1] = 0; /* no of segments in msg */


            ErrorCallback("SEND STOP CODE  \n");

            return conn.SendAndReceiveUdp(sendMsg);
        }

        private MsgData SendPause(MoveData moveData)
        {
            int msgLength = 2;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_PAUSE;

            sendMsg.data[0] = moveData.seqNo;
            sendMsg.data[1] = 0; /* no of segments in msg */


            ErrorCallback("SEND PAUSE CODE  \n");

            return conn.SendAndReceiveUdp(sendMsg);

        }

        private MsgData SendProbe(MoveData moveData)
        {
            int msgLength = 7;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_PROBE;

            sendMsg.data[0] = moveData.seqNo;
            sendMsg.data[1] = 0; /* no of segments in msg */

            sendMsg.data[2] = moveData.axe;
            sendMsg.data[3] = moveData.probeLength;
            sendMsg.data[4] = moveData.mode;
            sendMsg.data[5] = (int)(moveData.maxSpeed * 1000);
            sendMsg.data[6] = (int)(moveData.maxAcceleration * 1000);



            ErrorCallback("SEND PROBE CODE: vMax = " + sendMsg.data[5].ToString() + " aMax = " + sendMsg.data[6].ToString() + "\n");

            return conn.SendAndReceiveUdp(sendMsg);

        }
        private MsgData SendSpindleSpeed(MoveData moveData)
        {
            int msgLength = 3;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_SET_SPINDLE_SPEED;

            sendMsg.data[0] = moveData.seqNo;
            sendMsg.data[1] = 0; /* no of segments in msg */

            sendMsg.data[2] = moveData.speed;

            ErrorCallback("SEND M3 " + sendMsg.data[2].ToString() + "prom \n");

            return conn.SendAndReceiveUdp(sendMsg);
        }

        private void SendManualMoveStart(MoveData moveData)
        {
            int msgLength = 6;

            MsgData sendMsg = new MsgData(msgLength);
            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_MANUAL_MOVE_START;

            sendMsg.data[0] = -1;/* sentSeqNo;*/
            sendMsg.data[1] = 0; /* no of segments in msg */
            sendMsg.data[2] = moveData.axe;
            sendMsg.data[3] = moveData.probeLength; /* direction */
            sendMsg.data[4] = (int)(moveData.maxSpeed * 1000);
            sendMsg.data[5] = (int)(moveData.maxAcceleration * 1000);

            ErrorCallback("SEND Manual move start: vMax = " + sendMsg.data[4].ToString() + " aMax = " + sendMsg.data[5].ToString()  +"\n");

            conn.SendAndReceiveUdp(sendMsg);
        }
        private void SendManualMoveStop()
        {

            int msgLength = 2;

            MsgData sendMsg = new MsgData(msgLength);
            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_MANUAL_MOVE_STOP;

            sendMsg.data[0] = -1; /* sentSeqNo;*/
            sendMsg.data[1] = 0; /* no of segments in msg */


            ErrorCallback("SEND Manual move stop \n");

            conn.SendAndReceiveUdp(sendMsg);

        }

        private MsgData SendLine(MoveData moveData, bool ignoreLimiters)
        {
            int msgLength = 11;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_LINE;

            sendMsg.data[0] = moveData.seqNo ;
            sendMsg.data[1] = 0; /* no of segments in msg */

            sendMsg.data[2] = (int)(moveData.startSpeed * 1000);
            sendMsg.data[3] = (int)(moveData.endSpeed * 1000);
            sendMsg.data[4] = (int)(moveData.maxSpeed * 1000);
            sendMsg.data[5] = (int)(moveData.maxAcceleration * 1000);
            /*
            sendMsg.data[6] = moveData.moveVector.x;
            sendMsg.data[7] = moveData.moveVector.y;
            sendMsg.data[8] = moveData.moveVector.z;
            sendMsg.data[9] = moveData.moveVector.a;*/
            sendMsg.data[6] = moveData.endPoint.x;
            sendMsg.data[7] = moveData.endPoint.y;
            sendMsg.data[8] = moveData.endPoint.z;
            sendMsg.data[9] = moveData.endPoint.a;
            sendMsg.data[10] = ignoreLimiters ? 1 : 0;

            ErrorCallback("SEND Line: (" + sendMsg.data[6].ToString() + "," + sendMsg.data[7].ToString() + "," + sendMsg.data[8].ToString() + "," + sendMsg.data[9].ToString() + "\n");
            ErrorCallback("vs=" + sendMsg.data[2].ToString() + " ve=" + sendMsg.data[3].ToString() + " vMax=" + sendMsg.data[4].ToString() + " aMax=" + sendMsg.data[5].ToString() + " SEQ= " +  sendMsg.data[0].ToString() + "\n");

            return conn.SendAndReceiveUdp(sendMsg);

        }

        private MsgData SendArc2(MoveData moveData)
        {
            int msgLength = 17;

            MsgData sendMsg = new MsgData(msgLength);

            for (int i = 0; i < msgLength; i++)
            {
                sendMsg.data[i] = 0;
            }

            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_ARC2;

            sendMsg.data[0] = moveData.seqNo;
            sendMsg.data[1] = 0; /* no of segments in msg */

            sendMsg.data[2] = (int)(moveData.startSpeed * 1000);
            sendMsg.data[3] = (int)(moveData.endSpeed * 1000);
            sendMsg.data[4] = (int)(moveData.maxSpeed * 1000);
            sendMsg.data[5] = (int)(moveData.maxAcceleration * 1000);
            /*
            sendMsg.data[6] = moveData.moveVector.x;
            sendMsg.data[7] = moveData.moveVector.y;
            sendMsg.data[8] = moveData.moveVector.z;
            sendMsg.data[9] = moveData.moveVector.a;

            sendMsg.data[10] = moveData.centrePointVector.x;
            sendMsg.data[11] = moveData.centrePointVector.y;
            sendMsg.data[12] = moveData.centrePointVector.z;
            */
            sendMsg.data[6] = moveData.endPoint.x;
            sendMsg.data[7] = moveData.endPoint.y;
            sendMsg.data[8] = moveData.endPoint.z;
            sendMsg.data[9] = moveData.endPoint.a;

            sendMsg.data[10] = moveData.centrePoint.x;
            sendMsg.data[11] = moveData.centrePoint.y;
            sendMsg.data[12] = moveData.centrePoint.z;

            sendMsg.data[13] = (int)(moveData.rotationAxeVector.X * 1000);
            sendMsg.data[14] = (int)(moveData.rotationAxeVector.Y * 1000);
            sendMsg.data[15] = (int)(moveData.rotationAxeVector.Z * 1000);

            sendMsg.data[16] = moveData.turns;

            ErrorCallback("SEND Arc2: (" + sendMsg.data[6].ToString() + "," + sendMsg.data[7].ToString() + "," + sendMsg.data[8].ToString() + "," + sendMsg.data[9].ToString() + " SEQ= " + sendMsg.data[0].ToString() + "\n");
            ErrorCallback("centre: (" + sendMsg.data[10].ToString() + "," + sendMsg.data[11].ToString() + "," + sendMsg.data[12].ToString() + "\n");
            ErrorCallback("rotVecotr: (" + sendMsg.data[13].ToString() + "," + sendMsg.data[14].ToString() + "," + sendMsg.data[15].ToString() + "\n");
            ErrorCallback("vs=" + sendMsg.data[2].ToString() + " ve=" + sendMsg.data[3].ToString() + " vMax=" + sendMsg.data[4].ToString() + " aMax=" + sendMsg.data[5].ToString() + "\n");

            return conn.SendAndReceiveUdp(sendMsg);


        }

        public void SendAutoBase()
        {
            if (state == State_et.Idle)
            {
                int msgLength = 6;

                SetState(State_et.AutoBase);

                MsgData sendMsg = new MsgData(msgLength);
                sendMsg.orderCode = MsgData.ConOrderCode_et.OC_RUN_AUTOBASE;

                sendMsg.data[0] = sentSeqNo;
                sendMsg.data[1] = 0; /* no of segments in msg */
                sendMsg.data[2] = -1; /* axe x base */
                sendMsg.data[3] = -1; /* axe y base */
                sendMsg.data[4] = 1; /* axe z base */
                sendMsg.data[5] = 0; /* axe a base */

                ErrorCallback("SEND AutoBase \n");

                conn.SendAndReceiveUdp(sendMsg);
            }
            

        }
        internal enum SurfaceOffsetOrderCode_et
        {
            INIT,
            SET_POINT,
            ACTIVATE,
            DEACTIVATE,
            CLEAR
        };

        internal MsgData SendSurfaceOffset(SurfaceOffsetOrderCode_et orderCode,MoveData moveData)
        {


            int msgLength = 8;

            MsgData sendMsg = new MsgData(msgLength);
            sendMsg.orderCode = MsgData.ConOrderCode_et.OC_SUFRACE_OFFSET;

            sendMsg.data[0] = (int)orderCode;

            switch(orderCode)
            {
                case SurfaceOffsetOrderCode_et.INIT:
                    sendMsg.data[1] = moveData.surfOff_xSize;
                    sendMsg.data[2] = moveData.surfOff_ySize;
                    sendMsg.data[3] = moveData.surfOff_xStep;
                    sendMsg.data[4] = moveData.surfOff_yStep;
                    sendMsg.data[5] = moveData.surfOff_xStart;
                    sendMsg.data[6] = moveData.surfOff_yStart;

                    surfaceProbes.Clear();
                    surfaceProbes.Init(moveData.surfOff_xSize, moveData.surfOff_ySize, moveData.surfOff_xStep, moveData.surfOff_yStep, moveData.surfOff_xStart, moveData.surfOff_yStart);
                    break;
                case SurfaceOffsetOrderCode_et.SET_POINT:
                    sendMsg.data[1] = moveData.endPoint.x;
                    sendMsg.data[2] = moveData.endPoint.y;
                    sendMsg.data[7] = moveData.endPoint.z;

                    ErrorCallback("set point, x=" +  sendMsg.data[1].ToString() + " y= " +  sendMsg.data[2].ToString() + " val= " +  sendMsg.data[7].ToString() + " \n");

                    surfaceProbes.SetProbe(sendMsg.data[1], sendMsg.data[2], sendMsg.data[7]);
                    break;
                default:
                    break;

            }

            ErrorCallback("SEND Surface offset " + orderCode.ToString() + " \n");
            return conn.SendAndReceiveUdp(sendMsg);
        }


        internal void RunSurfaceProbe(int xSize, int ySize, int  xStep, int yStep)
        {
            surfaceProbes.Clear();

            string[] codeFile;

            Coord gPos = executor.GetActPos();
            Coord lPos = executor.GetActPosLocal();

            int xStart = 0;
            int yStart = 0;

            xStart = gPos.x - lPos.x;
            yStart = gPos.y - lPos.y;

            surfaceProbes.Init(xSize, ySize, xStep, yStep,xStart,yStart);

            codeFile = GetMacroCode("SurfaceProbe_Macro.txt", true);
            if (codeFile != null && codeFile.Length > 5)
            {
                codeFile[1] = "#<xSize> = " + xSize.ToString();
                codeFile[2] = "#<ySize> = " + ySize.ToString();
                codeFile[3] = "#<xStep> = " + (((double)xStep) * 0.001).ToString();
                codeFile[4] = "#<yStep> = " + (((double)yStep) * 0.001).ToString();



                StartCode(codeFile, null);
            }

        }

        internal void ClearSurfaceProbe()
        {
            SendSurfaceOffset(SurfaceOffsetOrderCode_et.CLEAR, null );
            surfaceProbes.Clear();

        }

        internal string[] GetMacroCode(string filePath, bool relative)
        {
            string path;
            if (relative == true)
            {
                path = "Macros/" + filePath;
            }
            else
            {
                path = filePath;
            }
            
            if (!File.Exists(path))
            {
                string curDir = Directory.GetCurrentDirectory();
                DialogResult result = MessageBox.Show("Macro " + path + " not found in " + curDir, "Macro not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            else
            {
                return File.ReadAllLines(path);
            }
        }


        /************************* manual move section ************************/
        private void SendManualMove(ManualMoveData runData, int startSpeed, int stopSpeed)
        {
            MoveData moveData = new MoveData();

            moveData.startSpeed = startSpeed * runData.speed; ;
            moveData.endSpeed = stopSpeed * runData.speed; 

            moveData.seqNo = -2;

            moveData.endPoint = runData.startPoint;



            switch (runData.axe)
            {
                case ManualMoveData.Axe_et.axe_x: 
                    moveData.endPoint.x += runData.dist;
                    moveData.maxSpeed = runData.speed;
                    moveData.maxAcceleration = Config.configData.maxAcceleration[0]; 
                    break;
                case ManualMoveData.Axe_et.axe_y:
                    moveData.endPoint.y += runData.dist;
                    moveData.maxSpeed = runData.speed;
                    moveData.maxAcceleration = Config.configData.maxAcceleration[1];
                    break;
                case ManualMoveData.Axe_et.axe_z:
                    moveData.endPoint.z += runData.dist;
                    moveData.maxSpeed = runData.speed;
                    moveData.maxAcceleration = Config.configData.maxAcceleration[2];
                    break;
                case ManualMoveData.Axe_et.axe_a: 
                    moveData.endPoint.a += runData.dist;
                    moveData.maxSpeed = runData.speed;
                    moveData.maxAcceleration = Config.configData.maxAcceleration[3]; 
                    break;
            }

            runData.startPoint = moveData.endPoint;

            moveData.length = Math.Abs(((double)(runData.dist))/1000);

            executor.SetActPos(moveData.endPoint);
            SetState(State_et.ManualMove);

            SendLine(moveData, runData.ignoreLimiters);


        }

        public void ManualMove(ManualMoveData runData)
        {
            if (state == State_et.Idle)
            {
                runData.speed = 10;
                runData.startPoint = executor.GetActPos();
                SendManualMove(runData, 0, 0);
            }
        }

        public void ManualMoveStart(ManualMoveData runData)
        {
            /*
            int speed = 10;    
            int dist = 1000;

            int interval = dist / speed;

            runData.dist *= dist;
            runData.speed = speed;
            manualMoveTimer.Interval = interval;            
            manualMoveData = runData;
            runData.startPoint = executor.GetActPos();
            if (state == State_et.Idle)
            {
                manualMoveTimer.Start();
                SendManualMove(runData, 0, 1);
                state = State_et.ManualMove;
            }
            */


            if (state == State_et.Idle)
            {
                MoveData moveData = new MoveData();

                moveData.seqNo = -1;
                moveData.endPoint = runData.startPoint;

                int axeIdx = 0;
                switch (runData.axe)
                {
                    case ManualMoveData.Axe_et.axe_x:
                        axeIdx = 0;
                        break;
                    case ManualMoveData.Axe_et.axe_y:
                        axeIdx = 1; ;
                        break;
                    case ManualMoveData.Axe_et.axe_z:
                        axeIdx = 2;
                        break;
                    case ManualMoveData.Axe_et.axe_a:
                        axeIdx = 3;
                        break;
                }

                moveData.axe = axeIdx;
                moveData.maxSpeed = Config.configData.maxSpeed[axeIdx];
                moveData.maxAcceleration = Config.configData.maxAcceleration[axeIdx];
                moveData.probeLength = runData.dist;

                SetState(State_et.ManualMoving);

                SendManualMoveStart(moveData);               

            }
        }


        private void ManualMoveTick(Object myObject,
                                        EventArgs myEventArgs)
        {
            if (state == State_et.ManualMove)
            {
                SendManualMove(manualMoveData, 1, 1);
            }
        }


        public void ManualMoveStop(ManualMoveData runData)
        {
            /*
            if (state == State_et.ManualMove)
            {
                manualMoveTimer.Stop();
                SendManualMove(manualMoveData, 1,0);
                state = State_et.Idle;
            }*/


                SendManualMoveStop();
          
        }

        public void DrawerClosed()
        {
            drawer = null;
        }


        public bool RunSimulation(bool draw, string[] codeFile)
        {
            if (state == State_et.Idle)
            {
                SetState(State_et.Simulating);

                bool ok = pipelineUnit.PrepareRun(0,true, codeFile);

                if (ok == false)
                {
                    SetState(State_et.Idle);
                    return false;
                }
                
                if(draw)
                {
                    if (drawer == null)
                    {
                        //drawer = new Drawer();
                        //drawer.DrawClosed += DrawerClosed;
                        //drawer.init3D();
                    }
                }

                MoveData moveData;
                bool cont = true;
                bool result = true;

                while(cont )
                {
                    moveData = pipelineUnit.GetMove(false);

                    if(moveData == null)
                    {
                        cont = false;
                    }
                    else if(moveData.orderCode == MoveData.orderCode_et.ERROR)
                    {
                        cont = false;
                        result = false;
                    }
                    else if (draw)
                    {
                        //drawer.Draw(moveData);

                    }    


                }
                pipelineUnit.StopSimulation();



                SetState(State_et.Idle);
                return result;
            }
            return false;
        }

    }
}
