using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Media.Media3D;

namespace CNC3
{
    internal class SpeedUnit
    {

        

        public SpeedUnit()
        {
             
        }

        public void CalcMaxSpeed(MoveData move)
        {
            ConfigData config = Config.configData;
            if(move.orderCode == MoveData.orderCode_et.LINE)
            {
                Coord moveCoord = move.endPoint - move.startPoint;

                double length = 0;
                double[] c = new double[Constants.NO_OF_AXES];
                c[0] = moveCoord.x;
                c[1] = moveCoord.y;
                c[2] = moveCoord.z;
                c[3] = moveCoord.a;

                for(int i=0;i< Constants.NO_OF_AXES;i++)
                {
                    length += c[i] * c[i];
                }
                length = Math.Sqrt(length);

                double maxSpeed = Double.MaxValue;
                double maxAcceleration = Double.MaxValue;

                for (int i = 0; i < Constants.NO_OF_AXES; i++)
                {
                    if (c[i] != 0)
                    {
                        double ratio = length / Math.Abs(c[i]);

                        double maxV = ratio * config.maxSpeed[i];
                        double maxA = ratio * config.maxAcceleration[i];

                        if (maxSpeed > maxV) { maxSpeed = maxV; }
                        if (maxAcceleration > maxA) { maxAcceleration = maxA; }
                    }
                }
                if (move.maxSpeed > 0)
                {
                    if (move.maxSpeed > maxSpeed)
                    {
                        move.maxSpeed = maxSpeed;
                    }
                }
                else
                {
                    move.maxSpeed = maxSpeed;
                }
                
                move.maxAcceleration = maxAcceleration;

            }
            else if((move.orderCode == MoveData.orderCode_et.ARC) || (move.orderCode == MoveData.orderCode_et.ARC2))
            {
                /*
                double  maxAccForArc;
                double maxSpeedForArc;
                double maxHAcc;
                double maxHSpeed;

                Vector3D calcVector;

                
                switch (move.planeMode)
                {
                    default:
                    case CommonCnc.PlaneMode_et.PLANE_XY:
                        maxAccForArc = Math.Min(config.maxAcceleration[0], config.maxAcceleration[1]);
                        maxSpeedForArc = Math.Min(config.maxSpeed[0], config.maxSpeed[1]);
                        maxHAcc = config.maxAcceleration[2];
                        maxHSpeed = config.maxSpeed[2];
                        calcVector = new Vector3D(move.startVector.X, move.startVector.Y, move.startVector.Z);
                        break;
                    case CommonCnc.PlaneMode_et.PLANE_ZX:
                        maxAccForArc = Math.Min(config.maxAcceleration[2], config.maxAcceleration[0]);
                        maxSpeedForArc = Math.Min(config.maxSpeed[2], config.maxSpeed[0]);
                        maxHAcc = config.maxAcceleration[1];
                        maxHSpeed = config.maxSpeed[1];
                        calcVector = new Vector3D(move.startVector.Z, move.startVector.X, move.startVector.Y);
                        break;
                    case CommonCnc.PlaneMode_et.PLANE_YZ:
                        maxAccForArc = Math.Min(config.maxAcceleration[1], config.maxAcceleration[2]);
                        maxSpeedForArc = Math.Min(config.maxSpeed[1], config.maxSpeed[2]);
                        maxHAcc = config.maxAcceleration[0];
                        maxHSpeed = config.maxSpeed[0];
                        calcVector = new Vector3D(move.startVector.Y, move.startVector.Z, move.startVector.X);
                        break;
                }
                calcVector.Normalize();

                calcVector.X = Math.Abs(calcVector.X);
                calcVector.Y = Math.Abs(calcVector.Y);
                calcVector.Z = Math.Abs(calcVector.Z);

                double maxArcSpeed = Math.Sqrt(maxAccForArc * move.radius);
                maxArcSpeed = Math.Min(maxArcSpeed, maxSpeedForArc);

                double maxSpeed = calcVector.X * maxArcSpeed + calcVector.Y * maxArcSpeed + calcVector.Z * maxHSpeed;
                double maxAcc = calcVector.X * maxAccForArc + calcVector.Y * maxAccForArc + calcVector.Z * maxHAcc;*/

                double maxSpeed = Math.Min(config.maxSpeed[0], Math.Min(config.maxSpeed[1], config.maxSpeed[2]));
                double maxAcc = Math.Min(config.maxAcceleration[0], Math.Min(config.maxAcceleration[1], config.maxAcceleration[2]));

                if (move.maxSpeed > 0)
                {
                    move.maxSpeed = Math.Min(maxSpeed, move.maxSpeed);
                }
                else
                {
                    move.maxSpeed = maxSpeed;
                }

                move.maxAcceleration = maxAcc;

            }
            else if (move.orderCode == MoveData.orderCode_et.PROBE)
            {
                double maxSpeed = config.maxSpeed[move.axe];
                if(move.maxSpeed > maxSpeed) { move.maxSpeed = maxSpeed; }
                move.maxAcceleration = config.maxAcceleration[move.axe];
            }
            else
            {
                move.maxSpeed = 0;
                move.maxAcceleration = 0;
            }
        }
        public double CalcTransferSpeed(MoveData move1, MoveData move2)
        {

            switch(move1.orderCode)
            {
                case MoveData.orderCode_et.LINE: break;

                case MoveData.orderCode_et.ARC: break;

                case MoveData.orderCode_et.ARC2: break;

                default: return 0;
            }

            switch (move2.orderCode)
            {
                case MoveData.orderCode_et.LINE: break;

                case MoveData.orderCode_et.ARC: break;

                case MoveData.orderCode_et.ARC2: break;

                default: return 0;
            }

            double angle = Vector3D.AngleBetween(move1.endVector, move2.startVector); 

            if((angle < 0.1)  && (angle > -0.1))
            {
                return Math.Min(move1.maxSpeed, move2.maxSpeed);
                
            }
            else
            {
                return 0;

            }
        }

        public void RecalcSpeeds(List<MoveData> movesList, bool stepMode)
        {

            if(stepMode == true)
            {
                if (movesList.Count > 0)
                {
                    movesList[0].startSpeed = 0;
                    movesList[0].endSpeed = 0;
                    return;
                }
            }

            int idx = 0;

            double speed = 0;
            if (movesList.Count > 0)
            {
                speed = movesList[0].startSpeed;
            }

            while (idx < movesList.Count)
            {
                double endSpeed = movesList[idx].maxEndSpeed; /* maxEndSpeed is calculated according to transfer speed and max speed in both segments */

                if(endSpeed > 0)
                {
                    double maxSpeedFromLength = Math.Sqrt(speed * speed + 2 * movesList[idx].maxAcceleration * movesList[idx].length);
                    endSpeed = Math.Min(endSpeed, maxSpeedFromLength);  
                }
                movesList[idx].endSpeed = endSpeed;
                speed = endSpeed;
                idx++;
                if(idx < movesList.Count)
                {
                    movesList[idx].startSpeed = speed;
                }

            }

            idx = movesList.Count - 1;

            speed = 0; /* speed at end */

            while (idx >= 0)
            {
                double maxSpeedFromLength = Math.Sqrt(speed * speed + 2 * movesList[idx].maxAcceleration * movesList[idx].length);

                if(movesList[idx].startSpeed > maxSpeedFromLength)
                {


                    movesList[idx].startSpeed = maxSpeedFromLength;
                    speed = movesList[idx].startSpeed;
                    idx--;
                    if(idx >= 0)
                    {
                        movesList[idx].endSpeed = speed;
                    }
                }
                else
                {
                    speed = movesList[idx].startSpeed;
                    idx--;
                }
                

            }

        }


    }
}
