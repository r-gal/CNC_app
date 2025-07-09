using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static CNC3.ManualMoveData;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using System.Windows.Media;
//using System.Windows.Media.Media3D;

namespace CNC3
{
    public class Coord
    {
        public int x, y, z, a;

        int globIdx;

        public Coord(int x, int y, int z, int a)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.a = a;
            globIdx = -1;
        }

        public Coord(Coord org)
        {
            this.x = org.x;
            this.y = org.y;
            this.z = org.z;
            this.a = org.a;
            globIdx = -1;
        }

     /*   public static Coord operator +(Coord a) => a;
        public static Coord operator -(Coord a) => new Coord(-a.num, a.den);*/

        public static Coord operator +(Coord a, Coord b)
            => new Coord( a.x + b.x, a.y + b.y, a.z + b.z, a.a + b.a);

        public static Coord operator -(Coord a, Coord b)
            => new Coord(a.x - b.x, a.y - b.y, a.z - b.z, a.a - b.a);

        public void Clear()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.a = 0;
        }

        internal int RoundUpToGranularity(int val, int granularity)
        {
            int mod = val % granularity;
            if(mod != 0)
            {
                val += (granularity - mod);
                return val;
            }
            else
            {
                return val;
            }
        }

        public void Round()
        {
            this.x = RoundUpToGranularity(this.x, Constants.GRANULARITY);
            this.y = RoundUpToGranularity(this.y, Constants.GRANULARITY);
            this.z = RoundUpToGranularity(this.z, Constants.GRANULARITY);
            this.a = RoundUpToGranularity(this.a, Constants.GRANULARITY);
        }

        public void Scale(double scale)
        {
            this.x = (int)((double)this.x * scale);
            this.y = (int)((double)this.y * scale);
            this.z = (int)((double)this.z * scale);
            this.a = (int)((double)this.a * scale);

            this.x =  RoundUpToGranularity(this.x, Constants.GRANULARITY);
            this.y = RoundUpToGranularity(this.y, Constants.GRANULARITY);
            this.z = RoundUpToGranularity(this.z, Constants.GRANULARITY);
            this.a = RoundUpToGranularity(this.a, Constants.GRANULARITY);
        }

        public Point3D GetPoint()
        {
            Point3D pt = new Point3D(((float)x) / 1000, ((float)y) / 1000, ((float)z) / 1000);
            return pt;
        }

        public void LoadPoint(Point3D point)
        {
            x = (int)(point.X * 1000);
            y = (int)(point.Y * 1000);
            z = (int)(point.Z * 1000);
        }
    }


    public class CoordInGlobal
    {
        public CoordInGlobal(int idx)
        {
            idxGlobal = idx;
        }


        public int x() { return (int)(gCodeCompMath.globalArray[idxGlobal]*1000); }
        public void x(int x_) { gCodeCompMath.globalArray[idxGlobal] = 0.001 * (double)(x_);   }
        public int y() { return (int)(gCodeCompMath.globalArray[idxGlobal+1] * 1000); }
        public void y(int y_) { gCodeCompMath.globalArray[idxGlobal+1] = 0.001 * (double)(y_); }
        public int z() { return (int)(gCodeCompMath.globalArray[idxGlobal+2] * 1000); }
        public void z(int z_) { gCodeCompMath.globalArray[idxGlobal+2] = 0.001 * (double)(z_); }
        public int a() { return (int)(gCodeCompMath.globalArray[idxGlobal+3] * 1000); }
        public void a(int a_) { gCodeCompMath.globalArray[idxGlobal+3] = 0.001 * (double)(a_); }


        int idxGlobal;

        public void Clear()
        {
            x(0);
            y(0);
            z(0);
            a(0);
        }

        public void Set(Coord coord)
        {
            x(coord.x); y(coord.y); z(coord.z); a(coord.a);
        }

        public Coord Get()
        {
            Coord coord = new Coord(x(),y(),z(),a());
            return coord;
        }
    }

    internal class ExecutorData : CommonCnc
    {
        public Coord actPos = new Coord(0, 0, 0, 0); /* absolute position */
        public Coord actLocalPos = new Coord(0, 0, 0, 0); /* local position */

        public PosMode_et posMode = PosMode_et.ABS;
        public PosMode_et polarPosMode = PosMode_et.REL;


        public PlaneMode_et planeMode = PlaneMode_et.PLANE_XY;

        public void CopyFrom(ExecutorData org)
        {
            posMode = org.posMode;
            polarPosMode = org.polarPosMode;

            planeMode = org.planeMode;
            actPos = new Coord(org.actPos);
            actLocalPos = new Coord(org.actLocalPos);
        }
    };

    abstract internal class MuliLineInstruction
    {
        internal Executor executor;
        public  abstract MoveData GetMove();


    }

    internal class ToolChangeInstruction : MuliLineInstruction
    {
        int phase = 0;
        int toolIdx;
        bool calib;
        public override MoveData GetMove()
        {
            MoveData move = null;
            Coord actPos = executor.GetActPos();
            //executor.PrintError(0,"Phase = " +  phase.ToString() + " act pos = " + actPos.x.ToString()  + "," + actPos.y.ToString() + "," + actPos.z.ToString() + "," + "\n");
            if (phase ==  0)
            {
                /* G53 G0 Z[baseZ] */
                phase = 1;
                
                int zPos = (int)(Config.configData.baseZ * 1000);
                if (actPos.z != zPos)
                {
                    Coord startPoint = new Coord(actPos);
                    Coord endPoint = new Coord(startPoint);
                    endPoint.z = zPos;
                    move = executor.ExecLine(startPoint, endPoint, true, true);
                    return move;
                }                
            }

            if (phase == 1)
            {
                /* G53 G0 X[baseX] Y[baseY] */
                if(calib)
                {
                    phase = 3;
                }
                else
                {
                    phase = 2;
                }
                
                int xPos = (int)(Config.configData.baseX * 1000);
                int yPos = (int)(Config.configData.baseY * 1000);
                int zPos = (int)(Config.configData.baseZ * 1000);
                if((actPos.x != xPos) | (actPos.y != yPos) | (actPos.z != zPos))
                {
                    Coord startPoint = new Coord(actPos);
                    Coord endPoint = new Coord(startPoint);
                    endPoint.x = xPos;
                    endPoint.y = yPos;
                    endPoint.z = zPos;
                    move = executor.ExecLine(startPoint, endPoint, true, true);
                    return move;
                }
            }

            if(phase == 2)
            {
                /* pause, wait for tool change */
                if(Config.configData.autoTLO == true)
                {
                    phase = 3;
                }
                else
                {
                    phase = 100;
                }                
                move = new MoveData();
                move.orderCode = MoveData.orderCode_et.PAUSE_TOOL_CHANGE;
                move.mode = toolIdx;
                return move;
            }

            if(phase == 3)
            {
                /* G38.2 Z[-200] F[probeSpeed1] */
                phase = 4;

                double speed = Config.configData.probeSpeed1;
                if(speed > 0)
                {
                    MoveData moveData = new MoveData();

                    moveData.probeLength = -(int)(Config.configData.probeLength * 1000);
                    moveData.axe = 2; /* axe Z */
                    moveData.mode = 2;
                    moveData.maxSpeed = speed;

                    moveData.orderCode = MoveData.orderCode_et.PROBE;
                    return moveData;
                }
                else
                {
                    return null;
                }
            }

            if(phase == 4)
            {
                double speed = Config.configData.probeSpeed2;
                if (speed > 0)
                {
                    /*G53 G91 G0 Z1 */
                    phase = 5;
                    Coord startPoint = new Coord(actPos);
                    Coord endPoint = new Coord(startPoint);
                    endPoint.z += 3000;
                    move = executor.ExecLine(startPoint, endPoint, true, true);
                    return move;
                }
                else
                {
                    phase = 6;
                }
            }

            if (phase == 5)
            {
                /* G38.2 Z[-200] F[probeSpeed2] */
                phase = 6;
                double speed = Config.configData.probeSpeed2;
                MoveData moveData = new MoveData();

                moveData.probeLength = -6000;
                moveData.axe = 2; /* axe Z */
                moveData.mode = 2;
                moveData.maxSpeed = speed;

                moveData.orderCode = MoveData.orderCode_et.PROBE;
                return moveData;
            }

            if (phase == 6)
            {
                phase = 100;
                /* save offset */
                if(calib)
                {
                    executor.CalcProbeOffset();
                }
                else
                {
                    executor.CalcToolLengthOffset();
                }     

                /* G53 G0 Z[baseZ] */
                int xPos = (int)(Config.configData.baseX * 1000);
                int yPos = (int)(Config.configData.baseY * 1000);
                int zPos = (int)(Config.configData.baseZ * 1000);
                if ((actPos.x != xPos) | (actPos.y != yPos) | (actPos.z != zPos))
                {
                    Coord startPoint = new Coord(actPos);
                    Coord endPoint = new Coord(startPoint);
                    endPoint.x = xPos;
                    endPoint.y = yPos;
                    endPoint.z = zPos;
                    move = executor.ExecLine(startPoint, endPoint, true, true);
                    return move;
                }
            }
            return null;
        }

        public ToolChangeInstruction(Executor executor_, int toolIdx_, bool calib_)
        {
            executor = executor_;
            toolIdx = toolIdx_;
            calib = calib_;
        }
    }


    internal class DrillingCycleInstruction : MuliLineInstruction
    {
        Coord absInitPos;
        int absClearZ;
        int absEndZ;


        int stepX;
        int stepY;
        int repeats;

        int phase = 0;
        /*
         * phase 0 - go to absClearZ if act.z < absClearZ 
         * phase 1 - go to X,Y if needed 
         * phase 2 - rapid move to absClearZ if needed
         * phase 3 - normal move to oldZ if needed
         * phase 4 - dwell if needed
         * phase 5 - rapid move to old Z or absClearZ
         * if repeat then goto phase 1 
         */

        public DrillingCycleInstruction(Executor executor_, Coord initPos, int clearZ, int endZ, int repeats_, int stepX_, int stepY_)
        {
            executor = executor_;

            stepX = stepX_;
            stepY = stepY_;
            repeats = repeats_;

            /* convert position data to absolute */
        Coord tmpPos = new Coord(0, 0, clearZ, 0);
            tmpPos = executor.CalcMachinePos(tmpPos);
            absClearZ = tmpPos.z;
            tmpPos = new Coord(0, 0, endZ, 0);
            tmpPos = executor.CalcMachinePos(tmpPos);
            absEndZ = tmpPos.z;
            absInitPos = executor.CalcMachinePos(initPos);



        }

        public override MoveData GetMove()
        {
            MoveData move = null;
            Coord actPos = executor.GetActPos();
            bool cont = true;


            while(cont)
            {
                switch(phase)
                {
                    case 0:
                        if (actPos.z < absClearZ)
                        {
                            absInitPos.z = absClearZ;

                            Coord startPoint = new Coord(actPos);
                            Coord endPoint = new Coord(startPoint);
                            endPoint.z = absClearZ;
                            move = executor.ExecLine(startPoint, endPoint, true, true);
                            phase = 1;
                            return move;
                        }
                        else
                        {
                            /* skip phase 0 */
                            absInitPos.z = actPos.z;
                            phase = 1;
                        }
                        break;

                    case 1:
                        if(actPos.x != absInitPos.x | actPos.y != absInitPos.y)
                        {
                            Coord startPoint = new Coord(actPos);
                            Coord endPoint = new Coord(absInitPos);
                            move = executor.ExecLine(startPoint, endPoint, true, true);
                            phase = 2;
                            return move;
                        }
                        else
                        {
                            /*skip phase 1 */
                            phase = 2;
                        }
                        break;
                    case 2:
                        if(actPos.z != absClearZ)
                        {
                            Coord startPoint = new Coord(actPos);
                            Coord endPoint = new Coord(absInitPos);
                            endPoint.z = absClearZ;
                            move = executor.ExecLine(startPoint, endPoint, true, true);
                            phase = 3;
                            return move;
                        }
                        else
                        {
                            /* skip phase 2*/
                            phase = 3;
                        }
                        break;
                    case 3:
                        if (actPos.z != absEndZ)
                        {
                            Coord startPoint = new Coord(actPos);
                            Coord endPoint = new Coord(absInitPos);
                            endPoint.z = absEndZ;
                            move = executor.ExecLine(startPoint, endPoint, false, true);
                            phase = 4;
                            return move;
                        }
                        else
                        {
                            /* skip phase 3*/
                            phase = 4;
                        }
                        break;
                    case 4:
                        /*dwel, skip */
                        phase = 5;
                        break;
                    case 5:
                        if (actPos.z != absInitPos.z)
                        {
                            Coord startPoint = new Coord(actPos);
                            Coord endPoint = new Coord(absInitPos);
                            move = executor.ExecLine(startPoint, endPoint, true, true);
                            phase = 6;
                            return move;
                        }
                        else
                        {
                            /* skip phase 5*/
                            phase = 6;
                        }
                        break;
                    case 6:
                        repeats--;
                        if(repeats > 0)
                        {
                            absInitPos.x += stepX;
                            absInitPos.y += stepY;
                            phase = 1;
                        }
                        else
                        {
                            phase = 100;
                            return null;                            
                        }
                        break;
                    default:
                        cont = false;
                        break;
                }
            }
            return null;
        }
    }

    internal class Executor : CommonCnc
    {
        cGodeCompiller compiller;

        ExecutorData data = null;
        ExecutorData runData = new ExecutorData();
        ExecutorData simData = new ExecutorData();

        CoordInGlobal[] coordSystem = new CoordInGlobal[10];
        

        CoordInGlobal G28_pos = new CoordInGlobal(5161);
        CoordInGlobal G30_pos = new CoordInGlobal(5181);

        CoordInGlobal G92_offset = new CoordInGlobal(5211);

        CoordInGlobal probePos = new CoordInGlobal(5061);

        void SetProbeResult(bool result)
        {
            gCodeCompMath.globalArray[5070] = result ? 1 : 0;
        }

        ToolDiameterCompensation toolDiameterCompensation = new ToolDiameterCompensation();

        MuliLineInstruction multiLineinstruction = null;
        bool Get_G92_offsetStatus()
        {
            return (gCodeCompMath.globalArray[5210] != 0);
        }
        void Set_G92_offsetStatus(bool status)
        {
            gCodeCompMath.globalArray[5210] = status ? 1 : 0;
        }

        int Get_coordSystemIdx()
        {
            return (int)(gCodeCompMath.globalArray[5220]);
        }
        void Set_coordSystemIdx(int idx)
        {
            gCodeCompMath.globalArray[5220] = idx;
        }

        double Get_ToolLengthOffset()
        {
            return (gCodeCompMath.globalArray[5430]);
        }
        int Get_ToolLengthOffsetInt()
        {
            return (int)((gCodeCompMath.globalArray[5430]) * 1000);
        }

        void Set_ToolLengthOffset(double offset)
        {
            gCodeCompMath.globalArray[5430] = offset;
        }
        double Get_ToolLengthSensorOffset()
        {
            return (gCodeCompMath.globalArray[5431]);
        }
        void Set_ToolLengthSensorOffset(double offset)
        {
            gCodeCompMath.globalArray[5431] = offset;
        }

        bool Get_CannedCycleReturnLevel()
        {
            return (gCodeCompMath.globalArray[5602] != 0);
        }
        void Set_CannedCycleReturnLevel(bool status)
        {
            gCodeCompMath.globalArray[5602] = status ? 1 : 0;
        }

        double millSpeed = 0;

        int seqNo = 0;

        public delegate void CallbackErrorCallback(string errorMsg);
        public static event CallbackErrorCallback ErrorCallback;
        internal  void PrintError(int line, string err)
        {
            ErrorCallback("Sim: Line " + line.ToString() + ":" + err);
        }

        internal void CalcToolLengthOffset()
        {
            Coord actPos = GetActPos();
            double newOffset = (((double)(actPos.z))* 0.001) - Get_ToolLengthSensorOffset();
            Set_ToolLengthOffset(newOffset);
        }

        internal void CalcProbeOffset()
        {
            Coord actPos = GetActPos();
            double newOffset = (((double)(actPos.z)) * 0.001);
            Set_ToolLengthOffset(0);
            Set_ToolLengthSensorOffset(newOffset);
        }

        public Executor(cGodeCompiller compiller_) 
        {
            compiller = compiller_;

            data = runData ;

            int globIdx = 5221;

            for (int i=1; i<10; i++)
            {
                coordSystem[i] = new CoordInGlobal(globIdx);
                globIdx += 10;
            }

            Set_coordSystemIdx(0);
            Set_G92_offsetStatus(false);
        }

        public Coord GetActPos()
        { 
            return new Coord(data.actPos);
        }

        public Coord GetActPosLocal() 
        {          
            return data.actLocalPos;
        }

        public void SetActPos(Coord actPos_) 
        {
            data.actPos = actPos_;
            data.actLocalPos = CalcLocalPos(actPos_);
        }

        public void SetProbeResult(Coord probePos_, bool probeResult)
        {
            probePos.Set(probePos_);
            SetProbeResult(probeResult);
        }

        public void ClearOffsets()
        {
            G28_pos.Clear();
            G30_pos.Clear();
            for (int i = 1; i < 10; i++)
            {
                coordSystem[i].Clear();
            }
            G92_offset.Clear();
            Set_G92_offsetStatus(false);

            Set_ToolLengthOffset(0);
            Set_ToolLengthSensorOffset(0);


        }

        public bool Start(int startLine, bool simulation, string[] codeFile)
        {
            compiller.ClearAll();
            seqNo = 0;
            moveTmp = null;
            toolDiameterCompensation.Reset();
            multiLineinstruction = null;
            if (simulation)
            {
                data = simData;

                simData.CopyFrom(runData);
            }
            else
            {
                data = runData;
            }

            return compiller.RunCompilation(codeFile);

            
        }

        public void StopSimulation()
        {
            data = runData;
        }

        MoveData moveTmp;

        public MoveData[] GetMove()
        {
            Word_st[] parArray;

            MoveData move = null;
            MoveData[] moves = null;
            int lineIdx = 0;

            if (multiLineinstruction != null)
            {
                move = multiLineinstruction.GetMove();

                if(move == null)
                {
                    multiLineinstruction = null;
                }
                else
                {

                    //PrintError(lineIdx, "Move from multiline " + move.orderCode.ToString() + "\n");
                }

            }

            if (move == null)
            {
                
                bool ready = false;
                do
                {
                    parArray = compiller.ExecuteLine();
                    if (parArray != null)
                    {
                        if (parArray.Length < 2) { return null; }

                        lineIdx = (int)parArray[1].value;


                        if (parArray[0].c == 'G')
                        {
                            switch (parArray[0].value)
                            {
                                case 900: data.posMode = PosMode_et.ABS; break; /* G90*/
                                case 910: data.posMode = PosMode_et.REL; break;   /* G91 */
                                case 901: data.polarPosMode = PosMode_et.ABS; break; /* G90.1*/
                                case 911: data.polarPosMode = PosMode_et.REL; break;   /* G91.1 */

                                case 170: data.planeMode = PlaneMode_et.PLANE_XY; break;  /* G17 */
                                case 180: data.planeMode = PlaneMode_et.PLANE_YZ; break;  /* G18 */
                                case 190: data.planeMode = PlaneMode_et.PLANE_ZX; break;  /* G19 */

                                case 400:
                                    SetCutterCompensation(parArray, 0);
                                    move = moveTmp;
                                    moveTmp = null;
                                    break;
                                case 410: SetCutterCompensation(parArray, -1); break;
                                case 420: SetCutterCompensation(parArray, 1); break;

                                case 0: /* G00*/
                                    move = ExecG0G1(parArray, true, false);
                                    break;


                                case 10:  /* G01*/
                                    move = ExecG0G1(parArray, false, false);
                                    break;

                                case 530: /* G53*/
                                    move = ExecG0G1(parArray, true, true);
                                    break;

                                case 20: /* G02*/
                                    move = ExecG2G3(parArray, true, false);
                                    break;
                                case 21: /* G02.1*/
                                    move = ExecG2G3(parArray, true, true);
                                    break;
                                case 30: /* G03*/
                                    move = ExecG2G3(parArray, false, false);
                                    break;
                                case 31: /* G03.1*/
                                    move = ExecG2G3(parArray, false, true);
                                    break;
                                case 40: /* G04*/
                                    move = ExecDwel(parArray);
                                    break;

                                case 382: /* G38.2 */
                                    move = ExecProbe(parArray, 2);
                                    break;
                                case 383: /* G38.3 */
                                    move = ExecProbe(parArray, 3);
                                    break;
                                case 384: /* G38.4 */
                                    move = ExecProbe(parArray, 4);
                                    break;
                                case 385: /* G38.5 */
                                    move = ExecProbe(parArray, 5);
                                    break;

                                case 280: /* G28 */
                                    {
                                        Coord startPoint = new Coord(data.actPos);
                                        Coord endPoint = new Coord(G28_pos.x(), G28_pos.y(), G28_pos.z(), G28_pos.a());
                                        move = ExecLine(startPoint, endPoint, true, true);
                                    }
                                    break;
                                case 281: /* G28.1 */
                                    G28_pos.Set(data.actPos);
                                    break;
                                case 300: /* G30 */
                                    {
                                        Coord startPoint = new Coord(data.actPos);
                                        Coord endPoint = new Coord(G30_pos.x(), G30_pos.y(), G30_pos.z(), G30_pos.a());
                                        move = ExecLine(startPoint, endPoint, true, true);
                                    }
                                    break;
                                case 301: /* G30.1 */
                                    G30_pos.Set(data.actPos);
                                    break;

                                case 100: /*G100 */
                                    ExecG100(parArray);
                                    break;
                                case 433: /* G43.3 */
                                    move = ExecG433(parArray);
                                    break;
                                case 434: /* G43.4 */
                                    move = ExecG434(parArray);
                                    break;
                                case 435: /* G43.5 */
                                    move = ExecG435();
                                    break;
                                case 436: /* G43.6 */
                                    move = ExecG436();
                                    break;
                                case 540: /* G54 */
                                    Set_coordSystemIdx(1);
                                    break;
                                case 550: /* G55 */
                                    Set_coordSystemIdx(2);
                                    break;
                                case 560: /* G56 */
                                    Set_coordSystemIdx(3);
                                    break;
                                case 570: /* G57 */
                                    Set_coordSystemIdx(4);
                                    break;
                                case 580: /* G58 */
                                    Set_coordSystemIdx(5);
                                    break;
                                case 590: /* G59 */
                                    Set_coordSystemIdx(6);
                                    break;
                                case 591: /* G59.1 */
                                    Set_coordSystemIdx(7);
                                    break;
                                case 592: /* G59.2 */
                                    Set_coordSystemIdx(8);
                                    break;
                                case 593: /* G59.3 */
                                    Set_coordSystemIdx(9);
                                    break;

                                case 920: /* G92 */
                                    ExecG92(parArray);
                                    break;
                                case 921: /* G92.1 */
                                    G92_offset.Clear();
                                    Set_G92_offsetStatus(false);
                                    break;
                                case 922: /* G92.2 */
                                    Set_G92_offsetStatus(false);
                                    break;
                                case 923: /* G92.3 */
                                    Set_G92_offsetStatus(true);
                                    break;
                                case 980: /* G98 */
                                    Set_CannedCycleReturnLevel(false);
                                    break;
                                case 990: /* G99 */
                                    Set_CannedCycleReturnLevel(true);
                                    break;
                                case 800: break;          /* G80 */
                                    move = ExecG800();
                                    break;
                                case 810: break;          /* G81 */
                                    move = ExecG810(parArray);
                                    break;

                                default: break;
                            }
                        }
                        else if (parArray[0].c == 'M')
                        {
                            switch (parArray[0].value)
                            {
                                case 30:
                                    move = ExecM3M4(parArray, false); /* M3 */
                                    break;
                                case 40:
                                    move = ExecM3M4(parArray, true); /* M3 */
                                    break;
                                case 50:
                                    move = ExecSpinde(0); /* M3 */
                                    break;
                                case 20:
                                case 300:
                                    move = new MoveData();
                                    move.orderCode = MoveData.orderCode_et.STOP;
                                    break;
                                case 0:
                                case 10: /* pause */
                                    move = new MoveData();
                                    move.orderCode = MoveData.orderCode_et.PAUSE;
                                    break;
                                case 60: /* tool change */
                                    move = HandleToolChange(parArray);
                                    break;
                                case 61: /* probe calibration */
                                    move = HandleProbeCalibration();

                                    break;
                                default: break;
                            }
                        }

                        if ((move != null) || (moves != null))
                        {
                            ready = true;
                            //PrintError(lineIdx, "Move from regular " + move.orderCode.ToString() + "\n");
                        }
                    }
                } while ((parArray != null) && (ready == false));
            }

            if (toolDiameterCompensation.IsActive())
            {
                if (moves != null)
                {
                    PrintError(lineIdx, "Multiple moves are not supported for cutter compensation");
                    MoveData moveError = new MoveData();
                    moveError.orderCode = MoveData.orderCode_et.ERROR;
                    MoveData[] moveArray = new MoveData[1];
                    moveArray[0] = moveError;
                    moveArray[0].seqNo = seqNo;
                    seqNo++;
                    return moveArray;
                }
                if (move != null)
                {
                    if (moveTmp == null)
                    {
                        moveTmp = move;
                    }
                    else
                    {
                        MoveData[] moveArray;

                        moveArray = toolDiameterCompensation.Compensate(moveTmp, move);
                        moveTmp = move;

                        for (int i = 0; i < moveArray.Length; i++)
                        {
                            moveArray[i].seqNo = seqNo;
                            seqNo++;
                        }

                        return moveArray;

                    }
                }
            }
            else
            {
                if (move != null)
                {
                    MoveData[] moveArray = new MoveData[1];
                    moveArray[0] = move;
                    moveArray[0].seqNo = seqNo;
                    seqNo++;
                    return moveArray;
                }
                else if (moves != null)
                {
                    for (int i = 0; i < moves.Length; i++)
                    {
                        moves[i].seqNo = seqNo;
                        seqNo++;
                    }
                    return moves;
                }


            }

            if (moveTmp != null)
            {
                MoveData[] moveArray = new MoveData[1];
                moveArray[0] = moveTmp;
                moveTmp = null;
                moveArray[0].seqNo = seqNo;
                seqNo++;
                return moveArray;
            }



            return null;
        }

        MoveData HandleToolChange(Word_st[] parArray)
        {
            int toolIdx = -1;
            foreach (var param in parArray)
            {

                switch (param.c)
                {
                    case 'T': toolIdx = (int)(param.value * 1000); ; break;
                }


            }

            if (toolIdx < 0)
            {
                int lineidx = (int)parArray[1].value;
                PrintError(lineidx, "Invalid tool index");
            }

            multiLineinstruction = new ToolChangeInstruction(this, toolIdx, false);

            return multiLineinstruction.GetMove();
        }

        MoveData HandleProbeCalibration()
        {
            multiLineinstruction = new ToolChangeInstruction(this, 0, true);
            return multiLineinstruction.GetMove();
        }

            void SetCutterCompensation(Word_st[] parArray,int dir)
        {
            if( dir == 0)
            {
                /*G40 */
                toolDiameterCompensation.SetDiameter(0);
            }
            else
            {
                bool diamValid = false;
                double diam = 0;
                foreach (var param in parArray)
                {
                    switch (param.c)
                    {
                        case 'D': diamValid = true; diam = (int)(param.value ); break;
                    }
                }
                if(diamValid)
                {
                    toolDiameterCompensation.SetDiameter(diam * dir);
                }
            }
        }

        MoveData ExecProbe(Word_st[] parArray, int mode)
        {

            int axe = -1;
            int length = 0;
            bool validF = false;
            double f = 0 ;

            foreach (var param in parArray)
            {
                
                switch (param.c)
                {
                    case 'X': if (axe != -1) { return null; }  axe = 0; length = (int)(param.value * 1000); break;
                    case 'Y': if (axe != -1) { return null; }  axe = 1; length = (int)(param.value * 1000); break;
                    case 'Z': if (axe != -1) { return null; }  axe = 2; length = (int)(param.value * 1000); break;
                    case 'A': if (axe != -1) { return null; }  axe = 3; length = (int)(param.value * 1000); break;
                    case 'F': validF = true; f = param.value; break;
                }
            }

            if (validF)
            {
                millSpeed = f / 60;
            }

            if (axe != -1)
            {
                MoveData moveData = new MoveData();

                moveData.probeLength = length;
                moveData.axe = axe;
                moveData.mode = mode;
                moveData.maxSpeed = millSpeed;

                moveData.orderCode = MoveData.orderCode_et.PROBE;
                return moveData;
            }
            else
            {
                return null;
            }




        }



        void ExecG92(Word_st[] parArray)
        {
            bool validX = false;
            bool validY = false;
            bool validZ = false;
            bool validA = false;

            int x = 0;
            int y = 0;
            int z = 0;
            int a = 0;

            foreach (var param in parArray)
            {

                switch (param.c)
                {
                    case 'X': validX = true; x = (int)(param.value * 1000); break;
                    case 'Y': validY = true; y = (int)(param.value * 1000); break;
                    case 'Z': validZ = true; z = (int)(param.value * 1000); break;
                    case 'A': validA = true; a = (int)(param.value * 1000); break;
                    
                }
            }

            int coordSystemIdx = Get_coordSystemIdx();
            Coord coordSystemOffset;
            if (coordSystemIdx > 0)
            {
                coordSystemOffset = coordSystem[coordSystemIdx].Get();
            }
            else
            {
                coordSystemOffset = new Coord(0, 0, 0, 0);
            }

            if (validX) { G92_offset.x(data.actPos.x - (x + coordSystemOffset.x)); }
            if (validY) { G92_offset.y(data.actPos.y - (y + coordSystemOffset.y)); }
            if (validZ) { G92_offset.z(data.actPos.z - (z + coordSystemOffset.z + Get_ToolLengthOffsetInt())); }
            if (validA) { G92_offset.a(data.actPos.a - (a + coordSystemOffset.a)); }

            Set_G92_offsetStatus( true);

            SetActPos(data.actPos); /* recalc local */

        }               

        public void SetOffset(SetBaseData baseData)
        {
            if (baseData == null)
            {
                Coord newOffset = new Coord(data.actPos);


                int coordSystemIdx = Get_coordSystemIdx();
                if (coordSystemIdx > 0) { newOffset -= coordSystem[coordSystemIdx].Get(); }
                newOffset.z -= Get_ToolLengthOffsetInt();

                G92_offset.Set(newOffset);
                
            }
            else
            { 
                int offset = (int)(baseData.offset * 1000);

                int coordSystemIdx = Get_coordSystemIdx();
                Coord coordSystemOffset;
                if (coordSystemIdx > 0) 
                { 
                    coordSystemOffset = coordSystem[coordSystemIdx].Get(); 
                }
                else
                {
                    coordSystemOffset = new Coord(0, 0, 0,0);
                }

                switch (baseData.axe)
                {
                    case Axe_et.axe_x:
                        G92_offset.x(data.actPos.x - (offset + coordSystemOffset.x));
                        break;
                    case Axe_et.axe_y:
                        G92_offset.y(data.actPos.y - (offset + coordSystemOffset.y));
                        break;
                    case Axe_et.axe_z:
                        G92_offset.z(data.actPos.z - (offset + coordSystemOffset.z + Get_ToolLengthOffsetInt()));
                        break;
                    case Axe_et.axe_a:
                        G92_offset.a(data.actPos.a - (offset + coordSystemOffset.a));
                        break;
                }
            }
            Set_G92_offsetStatus( true);

            SetActPos(data.actPos); /* recalc local */

        }

        MoveData ExecG433(Word_st[] parArray)
        {
            bool validI = false;
            bool validJ = false;
            bool validX = false;
            bool validY = false;

            int i = 0;
            int j = 0;
            int x = 0;
            int y = 0;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'I': validI = true; i = (int)(param.value ); break;
                    case 'J': validJ = true; j = (int)(param.value ); break;
                    case 'X': validX = true; x = (int)(param.value * 1000); break;
                    case 'Y': validY = true; y = (int)(param.value * 1000); break;
                }
            }

            if(validI && validJ && validX && validY)
            {
                Coord gPos = GetActPos();
                Coord lPos = GetActPosLocal();

                int xStart = 0;
                int yStart = 0;

                xStart = gPos.x - lPos.x;
                yStart = gPos.y - lPos.y;

                MoveData moveData = new MoveData();
                moveData.orderCode = MoveData.orderCode_et.SUFRACE_OFFSET_INIT;
                moveData.surfOff_xSize = i;
                moveData.surfOff_ySize = j;
                moveData.surfOff_xStep = x;
                moveData.surfOff_yStep = y;
                moveData.surfOff_xStart = xStart;
                moveData.surfOff_yStart = yStart;
                return moveData;
            }
            return null;

        }

        MoveData ExecG434(Word_st[] parArray)
        {
            bool validI = false;
            bool validJ = false;
            bool validZ = false;

            int i = 0;
            int j = 0;
            int z = 0;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'I': validI = true; i = (int)(param.value); break;
                    case 'J': validJ = true; j = (int)(param.value); break;
                    case 'Z': validZ = true; z = (int)(param.value * 1000); break;
                }
            }

            if (validI && validJ)
            {
                if (validZ == false)
                {
                    Coord actPos = GetActPosLocal();
                    z = actPos.z;
                }
                MoveData moveData = new MoveData();
                moveData.orderCode = MoveData.orderCode_et.SUFRACE_OFFSET_SET;
                moveData.endPoint = new Coord(i, j, z, 0);
                return moveData;



            }
            return null;

        }

        MoveData ExecG435()
        {
            MoveData moveData = new MoveData();
            moveData.orderCode = MoveData.orderCode_et.SUFRACE_OFFSET_ACTIVATE;
            return moveData;
        }

        MoveData ExecG436()
        {
            MoveData moveData = new MoveData();
            moveData.orderCode = MoveData.orderCode_et.SUFRACE_OFFSET_DEACTIVATE;
            return moveData;
        }

        MoveData ExecG800()
        {
            MoveData moveData = new MoveData();
            moveData.orderCode = MoveData.orderCode_et.SUFRACE_OFFSET_DEACTIVATE;
            return moveData;
        }

        MoveData ExecG810(Word_st[] parArray)
        {
            bool validX = false;
            bool validY = false;
            bool validZ = false;
            bool validR = false;
            bool validL = false;

            int x = 0;
            int y = 0;
            int z = 0;
            int r = 0;
            int l = 1;


            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'X': validX = true; y = (int)(param.value * 1000); break;
                    case 'Y': validY = true; y = (int)(param.value * 1000); break;
                    case 'Z': validZ = true; z = (int)(param.value * 1000); break;
                    case 'R': validR = true; r = (int)(param.value * 1000); break;
                    case 'L': validL = true; l = (int)(param.value); break;
                }
            }

            Coord initPoint = new Coord(data.actLocalPos);
            int clearZ = initPoint.z ;
            int endZ = initPoint.z; 
            int stepX = 0; 
            int stepY = 0;

            if (data.posMode == PosMode_et.ABS)
            {

                if (validX)
                {
                    initPoint.x = x;
                }
                if (validY)
                {
                    initPoint.y = y;
                }
                if (validZ)
                {
                    endZ = z;
                }
                if(validR)
                {
                    clearZ = r;
                }
            }
            else
            {
                if (validX) { initPoint.x += x; }
                if (validY) { initPoint.y += y; }

                if (validR) { clearZ += r; }
                endZ = clearZ;
                if (validZ) { endZ += z; }

                if(clearZ > initPoint.z)
                {
                    initPoint.z = clearZ;
                }
            }

            if(validL && l > 1 )                
            {
                if(data.posMode == PosMode_et.ABS)
                {
                    l = 1;
                    stepX = 0;
                    stepY = 0;
                    int lineIdx = (int)parArray[1].value;
                    PrintError(lineIdx, "Multiple drill cycles need relative positionig mode");
                }
                else
                {
                    stepX = x;
                    stepY = y;
                }

            }

            multiLineinstruction = new DrillingCycleInstruction(this, initPoint, clearZ, endZ, l, stepX, stepY);
            return multiLineinstruction.GetMove();

        }

        void ExecG100(Word_st[] parArray)
        {
            bool validX = false;
            bool validY = false;
            bool validZ = false;
            bool validA = false;
            bool validP = false;
            bool validL = false;


            int x = 0;
            int y = 0;
            int z = 0;
            int a = 0;
            int p = 0;
            int l = 0;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'X': validX = true; x = (int)(param.value * 1000); break;
                    case 'Y': validY = true; y = (int)(param.value * 1000); break;
                    case 'Z': validZ = true; z = (int)(param.value * 1000); break;
                    case 'A': validA = true; a = (int)(param.value * 1000); break;
                    case 'P': validP = true; p = (int)(param.value); break;
                    case 'L': validL = true; l = (int)(param.value); break;
                }
            }

            if (validL && (l == 20))
            {

                if (validP && (p < 10))
                {
                    if (p == 0)
                    {
                        p = Get_coordSystemIdx();
                    }
                    if (p > 0)
                    {
                        if (validX) { coordSystem[p].x(x); }
                        if (validY) { coordSystem[p].y(y); }
                        if (validZ) { coordSystem[p].z(z); }
                        if (validA) { coordSystem[p].a(a); }
                    }
                }
            }
        }

        MoveData ExecM3M4(Word_st[] parArray, bool ccw)
        {
            int speed = -1;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'S': speed = (int)(param.value ); break;
                }
            }
            if (speed >= 0)
            {
                if (ccw == true)
                {
                    speed = -speed;
                }
                return ExecSpinde(speed);

            }
            return null;
        }

        MoveData ExecSpinde(int speed)
        {
            MoveData moveData = new MoveData(); 
            moveData.orderCode = MoveData.orderCode_et.SPINDLE_SPEED;
            moveData.speed = speed;
            return moveData;
        }

        MoveData ExecG0G1(Word_st[] parArray, bool fast, bool machineCoord)
        {
            bool validX = false;
            bool validY = false;
            bool validZ = false;
            bool validA = false;
            bool validF = false;

            int x = 0;
            int y = 0;
            int z = 0;
            int a = 0;
            double f = 0;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'X': validX = true; x = (int)(param.value * 1000); break;
                    case 'Y': validY = true; y = (int)(param.value * 1000); break;
                    case 'Z': validZ = true; z = (int)(param.value * 1000); break;
                    case 'A': validA = true; a = (int)(param.value * 1000); break;
                    case 'F': validF = true; f = param.value; break;
                }
            }

            if (validF)
            {
                millSpeed = f/60;
            }

            Coord startPoint = new Coord(data.actPos);
            Coord endPoint;

            if (machineCoord)
            {
                endPoint = new Coord(data.actPos);
            }
            else
            {
                endPoint = new Coord(data.actLocalPos);
            }

            if (data.posMode == PosMode_et.ABS)
            {
                if (validX)
                {
                    endPoint.x = x;
                }
                if (validY)
                {
                    endPoint.y = y;
                }
                if (validZ)
                {
                    endPoint.z = z;
                }
                if (validA)
                {
                    endPoint.a = a;
                }    
                
            }
            else
            {
                if (validX) { endPoint.x += x; }
                if (validY) { endPoint.y += y; }
                if (validZ) { endPoint.z += z; }
                if (validA) { endPoint.a += a; }
            }

            return ExecLine(startPoint, endPoint, fast, machineCoord);

        }

        internal Coord CalcMachinePos(Coord localPos)
        {
            Coord machinePos = new Coord(localPos);

            int coordSystemIdx = Get_coordSystemIdx();
            if (coordSystemIdx > 0) { machinePos += coordSystem[coordSystemIdx].Get(); }
            if (Get_G92_offsetStatus()) { machinePos += G92_offset.Get(); }
            machinePos.z += Get_ToolLengthOffsetInt();

            return machinePos;
        }

        internal Coord CalcLocalPos(Coord machinePos)
        {
            Coord localPos = new Coord(machinePos);

            int coordSystemIdx = Get_coordSystemIdx();
            if (coordSystemIdx > 0) { localPos -= coordSystem[coordSystemIdx].Get(); }
            if (Get_G92_offsetStatus()) { localPos -= G92_offset.Get(); }
            localPos.z -= Get_ToolLengthOffsetInt();

            return localPos;
        }

        internal MoveData ExecLine(Coord startPoint, Coord endPoint_, bool fast, bool endPointIsAbsolute)
        {
            Coord endPoint;            
            
            if (endPointIsAbsolute == false)
            {
                /* apply offsets */
                endPoint = CalcMachinePos(endPoint_);
            }
            else
            {
                endPoint = new Coord(endPoint_);
            }

            if ((startPoint.x == endPoint.x) && (startPoint.y == endPoint.y) && (startPoint.z == endPoint.z) && (startPoint.a == endPoint.a))
            {
                return null;
            }

            MoveData moveData = new MoveData();
 
            Point3D sPt, ePt;
            sPt = startPoint.GetPoint();
            ePt = endPoint.GetPoint();
            Vector3D moveVector = ePt - sPt;

            //moveData.moveVector = endPoint - startPoint;
            moveData.endPoint = endPoint;
            moveData.startPoint = startPoint;

            moveData.length = moveVector.Length;

            moveVector.Normalize();
            moveData.startVector = moveVector;
            moveData.endVector = moveVector;

            moveData.orderCode = MoveData.orderCode_et.LINE;

            moveData.maxSpeed = fast ? -1 : millSpeed;
            moveData.rapidMove = fast;

            SetActPos(endPoint);

            return moveData;


        }

        MoveData ExecDwel(Word_st[] parArray)
        {

            int p = 0;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'P': p = (int)(param.value*1000); break;
                }
            }
            if(p > 0)
            {
                MoveData moveData = new MoveData();
                moveData.orderCode = MoveData.orderCode_et.DELAY;
                moveData.delay = p;
                return moveData;

            }

            return null;
        }
        


        MoveData ExecG2G3 (Word_st[] parArray, bool clockwise, bool direct3d)
        {
            int lineidx = (int)parArray[1].value;
            if (direct3d == true)
            {
                if(toolDiameterCompensation.IsActive())
                {
                    PrintError(lineidx, "3D arc is not supported for cutter compensation");
                    MoveData moveError = new MoveData();
                    moveError.orderCode = MoveData.orderCode_et.ERROR;
                    return moveError;


                }
            }
            else if(data.planeMode != PlaneMode_et.PLANE_XY)
            {
                if (toolDiameterCompensation.IsActive())
                {
                    PrintError(lineidx, "Only XY plane is supported for cutter compensation");
                    MoveData moveError = new MoveData();
                    moveError.orderCode = MoveData.orderCode_et.ERROR;
                    return moveError;
                }
            }

            bool validX = false;
            bool validY = false;
            bool validZ = false;

            bool validI = false;
            bool validJ = false;
            bool validK = false;

            bool validR = false;
            //bool validP = false;

            bool validA = false;
            bool validF = false;

            int x = 0;
            int y = 0;
            int z = 0;

            int i = 0;
            int j = 0;
            int k = 0;

            double r = 0;
            int p = 1;

            int a = 0;
            double f = 0;

            foreach (var param in parArray)
            {
                switch (param.c)
                {
                    case 'X': validX = true; x = (int)(param.value * 1000); break;
                    case 'Y': validY = true; y = (int)(param.value * 1000); break;
                    case 'Z': validZ = true; z = (int)(param.value * 1000); break;
                    case 'A': validA = true; a = (int)(param.value * 1000); break;
                    case 'I': validI = true; i = (int)(param.value * 1000); break;
                    case 'J': validJ = true; j = (int)(param.value * 1000); break;
                    case 'K': validK = true; k = (int)(param.value * 1000); break;
                    case 'P': /*validP = true;*/ p = (int)(param.value); break;
                    case 'R': validR = true; r = param.value; break;
                    case 'F': validF = true; f = param.value; break;
                }
            }

            if (validF)
            {
                millSpeed = f/60;
            }

            Coord startPoint = new Coord(data.actLocalPos);
            Coord endPoint = new Coord(data.actLocalPos);
            Coord centrePoint = new Coord(data.actLocalPos);

            if (data.posMode == PosMode_et.ABS)
            {
                
                if (validX)
                {
                    endPoint.x = x;
                }
                if (validY)
                {
                    endPoint.y = y;
                }
                if (validZ)
                {
                    endPoint.z = z;
                }
                if (validA)
                {
                    endPoint.a = a;
                }
            }
            else
            {
                if (validX) { endPoint.x += x; }
                if (validY) { endPoint.y += y; }
                if (validZ) { endPoint.z += z; }
                if (validA) { endPoint.a += a; }
            }

            int cwFactor = clockwise ? 1 : -1;

            Vector3D rotVector;

            if (direct3d)
            { 
                rotVector = new Vector3D(cwFactor * (startPoint.y - endPoint.y), -cwFactor * (startPoint.x - endPoint.x), 0);
                rotVector.Normalize();
            }
            else
            {
                switch (data.planeMode)
                {
                    default:
                    case PlaneMode_et.PLANE_XY:
                        rotVector = new Vector3D(0, 0, cwFactor);
                        break;
                    case PlaneMode_et.PLANE_ZX:
                        rotVector = new Vector3D(0, cwFactor, 0);
                        break;
                    case PlaneMode_et.PLANE_YZ:
                        rotVector = new Vector3D(cwFactor, 0, 0);
                        break;
                }
            }
            RotMatrix[] rotMatrixArray = RotMatrix.GetRotMatrix(rotVector);

            Point3D pCentre;

            if (validR == true)
            {
                Point3D ptStart = startPoint.GetPoint();
                Point3D ptEnd = endPoint.GetPoint();
                pCentre = CalcCentrePoint(ptStart, ptEnd, r, data.planeMode, rotMatrixArray);
                
                centrePoint.LoadPoint(pCentre);
            }
            else
            {
                if (data.polarPosMode == PosMode_et.ABS)
                {
                    if (validI) 
                    { 
                        centrePoint.x = i ;
                    }
                    if (validJ)
                    { 
                        centrePoint.y = j ;
                    }
                    if (validK)
                    { 
                        centrePoint.z = k ;
                    }
                }
                else
                {
                    if (validI) { centrePoint.x += i; }
                    if (validJ) { centrePoint.y += j; }
                    if (validK) { centrePoint.z += k; }
                }
                pCentre = centrePoint.GetPoint();
            }

            bool fullCircles = false;
            if (direct3d == false)
            {
                switch (data.planeMode)
                {
                    default:
                    case PlaneMode_et.PLANE_XY:
                        centrePoint.z = startPoint.z;
                        break;
                    case PlaneMode_et.PLANE_ZX:
                        centrePoint.y = startPoint.y;
                        break;

                    case PlaneMode_et.PLANE_YZ:
                        centrePoint.x = startPoint.x;
                        break;
                }


                switch (data.planeMode)
                {
                    default:
                    case PlaneMode_et.PLANE_XY:
                        if ((validX | validY) == false)
                        {
                            fullCircles = true;
                        }
                        break;
                    case PlaneMode_et.PLANE_ZX:
                        if ((validX | validZ) == false)
                        {
                            fullCircles = true;
                        }
                        break;

                    case PlaneMode_et.PLANE_YZ:
                        if ((validY | validZ) == false)
                        {
                            fullCircles = true;
                        }
                        break;
                }
            }
            if(fullCircles == false)
            {
                p = 0;
            }


            Point3D pStart = startPoint.GetPoint();
            Point3D pEnd = endPoint.GetPoint();

            Point3D pStart_plane = rotMatrixArray[1] * pStart;
            Point3D pCentre_plane = rotMatrixArray[1] * pCentre;
            Point3D pEnd_plane = rotMatrixArray[1] * pEnd;




            double startAngle = Math.Atan2(pStart_plane.Y - pCentre_plane.Y, pStart_plane.X - pCentre_plane.X);
            double endAngle;
            if (p == 0)
            {
                p = 0;
                endAngle = Math.Atan2(pEnd_plane.Y - pCentre_plane.Y, pEnd_plane.X - pCentre_plane.X);

                if (startAngle < endAngle) { endAngle -= (Math.PI * 2); }

            }
            else
            {
                endAngle = startAngle + (2 * Math.PI * p);
            }

            double rx = pStart_plane.X - pCentre_plane.X;
            double ry = pStart_plane.Y - pCentre_plane.Y;

            double radius = Math.Sqrt(rx * rx + ry * ry);

            double length = Math.Abs(radius * (endAngle - startAngle));

            if (length == 0)
            {
                return null;
            }

            double h = pEnd_plane.Z - pStart_plane.Z;

            double tLength = Math.Sqrt(length * length + h * h);

            Vector3D vectorR1 = pStart_plane - pCentre_plane;
            Vector3D vectorR2 = pEnd_plane - pCentre_plane;

            Vector3D vectorStart;
            Vector3D vectorEnd;

            vectorStart = new Vector3D(vectorR1.Y, -vectorR1.X, 0);
            vectorEnd = new Vector3D(vectorR2.Y, -vectorR2.X, 0);

            vectorStart.Normalize();
            vectorEnd.Normalize();
            vectorStart *= length;
            vectorEnd *= length;
            vectorStart.Z = h;
            vectorEnd.Z = h;

            MoveData move = new MoveData();

            move.radius = radius;
            move.startVector = rotMatrixArray[0] * vectorStart;
            move.endVector = rotMatrixArray[0] * vectorEnd;
            move.length = tLength;

            centrePoint.LoadPoint(pCentre);


            Coord centrePointAbs = CalcMachinePos(centrePoint);
            Coord endPointAbs = CalcMachinePos(endPoint);

            move.endPoint = endPointAbs;
            move.centrePoint = centrePointAbs;
            move.startPoint = new Coord(data.actPos); ;
            /*move.startAngle = startAngle;*/
            /*move.endAngle = endAngle;*/

            move.turns = p;

            move.orderCode = MoveData.orderCode_et.ARC2;

            move.rotationAxeVector = rotVector;

            SetActPos(endPointAbs);

            move.maxSpeed = millSpeed;

            return move;
        }


        

        Point3D CalcCentrePoint(Point3D startPoint, Point3D endPoint, double r,PlaneMode_et pMode, RotMatrix[] rotMatrixArray)
        {
            Point3D centrePoint;
            Point3D pStart;
            Point3D pEnd;

            Vector3D v1;
            Vector3D v2;

            Point3D pStart_plane = rotMatrixArray[1] * startPoint;
            Point3D pEnd_plane = rotMatrixArray[1] * endPoint;

            v1 = pEnd_plane - pStart_plane;
            v2 = new Vector3D(v1.Y , -v1.X , 0);

            v1 *= 0.5;
            v2.Normalize();

            double s = v1.Length;

            double h = Math.Sqrt(r * r - s * s);
            v2 *= h;

            Point3D pCentre_plane = pStart_plane + v1;
            pCentre_plane = pCentre_plane + v2;

            Point3D pCentre = rotMatrixArray[0] * pCentre_plane;

            return pCentre;

        }

        double CalcRadius(Point3D startPoint, Point3D centrePoint, PlaneMode_et pMode)
        {
            Point3D pStart;
            Point3D pCentre;

            switch (pMode)
            {
                default:
                case PlaneMode_et.PLANE_XY:
                    pStart = new Point3D(startPoint.X, startPoint.Y, 0);
                    pCentre = new Point3D(centrePoint.X, centrePoint.Y, 0);
                    break;
                case PlaneMode_et.PLANE_ZX:
                    pStart = new Point3D(startPoint.Z, startPoint.X, 0);
                    pCentre = new Point3D(centrePoint.Z, centrePoint.X, 0);
                    break;

                case PlaneMode_et.PLANE_YZ:
                    pStart = new Point3D(startPoint.Y, startPoint.Z, 0);
                    pCentre = new Point3D(centrePoint.Y, centrePoint.Z, 0);
                    break;
            }
            Vector3D v1 = pCentre - pStart;
            return v1.Length;

        }

        double CalcAngle(Point3D point, Point3D centrePoint, PlaneMode_et pMode)
        {
            double angle = 0;

            switch (pMode)
            {
                case PlaneMode_et.PLANE_XY:
                    angle = Math.Atan2(point.Y - centrePoint.Y, point.X - centrePoint.X);
                    break;
                case PlaneMode_et.PLANE_ZX:
                    angle = Math.Atan2(point.X - centrePoint.X, point.Z - centrePoint.Z);
                    break;
                case PlaneMode_et.PLANE_YZ:
                    angle = Math.Atan2(point.Z - centrePoint.Z, point.Y - centrePoint.Y);
                    break;
            }

            return angle;
        }


        double CalcHLength(Point3D startPoint, Point3D endPoint,  PlaneMode_et pMode)
        {
            double h = 0;

            switch (pMode)
            {
                case PlaneMode_et.PLANE_XY:
                    h = endPoint.Z - startPoint.Z;
                    break;
                case PlaneMode_et.PLANE_ZX:
                    h = endPoint.Y - startPoint.Y;
                    break;
                case PlaneMode_et.PLANE_YZ:
                    h = endPoint.X - startPoint.X;
                    break;
            }

            return h;
        }
    }
}
