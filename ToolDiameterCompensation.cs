using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNC3
{
    internal class ToolDiameterCompensation
    {
        double compensation;

        Vector prevVector;


        public ToolDiameterCompensation()
        {
            compensation = 0;
            prevVector = null;
        }



        public MoveData[] Compensate(MoveData actMove, MoveData nextMove)
        {
            if (compensation == 0)
            {
                MoveData[] moveArray = new MoveData[1];
                moveArray[0] = actMove;
                return moveArray;
            }
            else
            {
                actMove.compensated = true;
                return UpdateCommonPoint( actMove,  nextMove);
            }

        }

        public void Reset()
        {
            compensation = 0;
            prevVector = null;
        }

        public bool IsActive()
        {
            return compensation != 0;
        }

        public void SetDiameter(double diam)
        {
            compensation = diam;
        }

        private MoveData[] UpdateCommonPoint(MoveData actCom, MoveData nextCom)
        {

            //DrawItem next2Com = drawProgList[idx + 2];

            Vector vectorActEnd;
            Vector vectorNextStart;


            Vector vectorActShift;
            Vector vectorNextShift;


            double r1;
            double r2;
            double R1 = 0;
            double R2 = 0;

            double trend1 = 0;
            double trend2 = 0;

            bool clockwise;


            int caseSel = 0;

            bool cont = true;
            /* data preparation */
            MoveData[] moveArray;

            switch (actCom.orderCode)
            {
                case MoveData.orderCode_et.LINE:
                    vectorActEnd = new Vector(actCom.endVector.X, actCom.endVector.Y);
                    if (vectorActEnd.X == 0 && vectorActEnd.Y == 0)
                    { vectorActEnd = prevVector; }
                    vectorActEnd.Normalize();
                    vectorActShift = new Vector(-vectorActEnd.Y, vectorActEnd.X);
                    trend1 = 0;
                    r1 = compensation;
                    break;
                case MoveData.orderCode_et.ARC2:
                    vectorActEnd = new Vector(actCom.endVector.X, actCom.endVector.Y);
                    vectorActEnd.Normalize();
                    vectorActShift = new Vector(-vectorActEnd.Y, vectorActEnd.X);
                    R1 = actCom.radius;

                    actCom.clockwise = actCom.rotationAxeVector.Z > 0;

                    trend1 = actCom.clockwise ? R1 : -R1;

                    //r1 = compensation;
                     if (actCom.clockwise)
                     {

                         r1 = compensation;
                     }
                     else
                     {
                         r1 = -compensation;
                     }
                    caseSel |= 1;
                    break;
                default:
                    /* nothing to do */
                    moveArray = new MoveData[1];
                    moveArray[0] = actCom;
                    return moveArray;
            }

            switch (nextCom.orderCode)
            {
                case MoveData.orderCode_et.LINE:
                    vectorNextStart = new Vector(nextCom.startVector.X, nextCom.startVector.Y);
                    if (vectorNextStart.X == 0 && vectorNextStart.Y == 0)
                    { vectorNextStart = vectorActEnd; }
                    vectorNextStart.Normalize();
                    vectorNextShift = new Vector(-vectorNextStart.Y, vectorNextStart.X);
                    trend2 = 0;
                    r2 = compensation;
                    break;
                case MoveData.orderCode_et.ARC2:
                    vectorNextStart = new Vector(nextCom.startVector.X, nextCom.startVector.Y);
                    vectorNextStart.Normalize();
                    vectorNextShift = new Vector(-vectorNextStart.Y, vectorNextStart.X);
                    R2 = nextCom.radius;

                    nextCom.clockwise = nextCom.rotationAxeVector.Z > 0;
                    trend2 = nextCom.clockwise ? -R2 : R2;
                    //r2 = compensation;
                    if (nextCom.clockwise == true)
                    {
                        
                        r2 = compensation;
                    }
                    else
                    {
                        r2 = -compensation;
                    }
                    caseSel |= 2;
                    break;
                default:
                    /* nothing to do */
                    moveArray = new MoveData[1];
                    moveArray[0] = actCom;
                    return moveArray;
            }



            if (compensation < 0) /* G42 */
            {
                clockwise = false;
            }
            else
            {
                clockwise = true;
            }

            prevVector = vectorActEnd;
            /* preparation finished*/

            /* case selection */
            double angle = Vector.AngleBetween(vectorActEnd, vectorNextStart);


            bool addArc = false;
            bool calcPoint = false;

            double f = 0.01;


            if (angle == 0)
            {
                /*easy point */
            }
            else if (Math.Abs(angle - Math.PI) < f || Math.Abs(angle + Math.PI) < f)
            {
                if (compensation < 0) /* G42 */
                {
                    if (trend1 < trend2)
                    {
                        calcPoint = true;
                    }
                    else
                    {
                        addArc = true;
                    }
                }
                else
                {
                    if (trend1 > trend2)
                    {
                        calcPoint = true;
                    }
                    else
                    {
                        addArc = true;
                    }
                }

            }
            else if (compensation < 0) /* G42 */
            {
                if (angle < 0)
                {
                    calcPoint = true;
                }
                else
                {
                    addArc = true;
                }

            }
            else /* G41 */
            {
                if (angle > 0)
                {
                    calcPoint = true;
                }
                else
                {
                    addArc = true;
                }
            }

            /* execute selected case */

            MoveData addedMove = null;

            if (addArc)
            {
                vectorActShift *= compensation;
                vectorNextShift *= compensation;

                addedMove = new MoveData();

                addedMove.orderCode = MoveData.orderCode_et.ARC2;
                addedMove.turns = 0;
                Point3D sP = actCom.endPoint.GetPoint();
                Vector3D sV = new Vector3D(vectorActShift.X, vectorActShift.Y, 0);
                Point3D eP = actCom.endPoint.GetPoint();
                Vector3D eV = new Vector3D(vectorNextShift.X, vectorNextShift.Y, 0);
                addedMove.startPoint = new Coord(0, 0, 0, 0);
                addedMove.endPoint = new Coord(0, 0, 0, 0);
                addedMove.startPoint.LoadPoint(sP + sV);
                addedMove.endPoint.LoadPoint(eP + eV);
                addedMove.centrePoint = actCom.endPoint;
                if (clockwise)
                {
                    addedMove.rotationAxeVector = new Vector3D(0, 0, 1);
                }
                else
                {
                    addedMove.rotationAxeVector = new Vector3D(0, 0, -1);
                }
                addedMove.startVector = actCom.endVector;
                addedMove.endVector = nextCom.startVector;
                addedMove.compensated = true;

                actCom.endPoint = addedMove.startPoint;
                nextCom.startPoint = addedMove.endPoint;

                addedMove.Recalc();
            }
            else if (calcPoint)
            {
                Point3D newPoint;
                switch (caseSel)
                {
                    case 0: /* line-line */
                        newPoint = CalcNewPointLineLine(angle, actCom, r1, vectorActEnd, vectorActShift);
                        break;
                    case 1: /* arc - line */
                        newPoint = CalcNewPointLineArc(angle, actCom.endPoint.GetPoint(), r1, R1, -1*vectorNextStart, !actCom.clockwise);
                        break;
                    case 2: /* line - arc */
                        newPoint = CalcNewPointLineArc(angle, actCom.endPoint.GetPoint(), r2, R2, vectorActEnd, nextCom.clockwise);
                        break;
                    case 3: /* arc - arc */
                        newPoint = CalcNewPointArcArc(angle, actCom, nextCom, R1, R2, r1, r2);
                        break;
                    default: /* error case */
                        newPoint = new Point3D(0, 0, 0);
                        break;

                }

                actCom.endPoint.LoadPoint(newPoint);
                nextCom.startPoint.LoadPoint(newPoint);

            }
            else
            {
                /* easy point */

                vectorActShift *= compensation;
                vectorNextShift *= compensation;


                actCom.endPoint.LoadPoint(actCom.endPoint.GetPoint() + new Vector3D(vectorActShift.X, vectorActShift.Y, 0));
                nextCom.startPoint.LoadPoint(nextCom.startPoint.GetPoint() + new Vector3D(vectorNextShift.X, vectorNextShift.Y, 0));

            }

            actCom.Recalc();

            if (addArc)
            {
                moveArray = new MoveData[2];
                moveArray[1] = addedMove;
                moveArray[0] = actCom;
            }
            else
            {
                moveArray = new MoveData[1];
                moveArray[0] = actCom;
            }
        

            return moveArray;

        }

        private Point3D CalcNewPointLineLine(double angle, MoveData actCom, double r1, Vector vectorActEnd, Vector vectorActShift)
        {
            double h = r1 * Math.Tan( angle / 2);
            h = Math.Abs(h);

            Vector vectorH = vectorActEnd;
            vectorH.Normalize();

            vectorH *= -h;
            vectorH += vectorActShift * r1;


            Vector3D pointShift = new Vector3D(vectorH.X, vectorH.Y, 0);

            Point3D newPoint = actCom.endPoint.GetPoint() + pointShift;

            return newPoint;
        }

        private Point3D CalcNewPointLineArc(double angle, Point3D actPoint, double r, double R, Vector vectorLine, bool rPol)
        {

            double x = R * Math.Cos( angle);
            double y = R * Math.Sin( angle);

            double h = Math.Sqrt(Math.Pow(R + r, 2) - Math.Pow(x + r, 2)) - Math.Abs(y);

            Vector vectorH = vectorLine;
            vectorH.Normalize();
            Vector vertorR = new Vector(vectorH.Y, -vectorH.X);

            if (rPol)
            {
                vertorR *= -r;
            }
            else
            {
                vertorR *= r;
            }

            vectorH *= -h;


            vectorH += vertorR;
            /*

            if (clockwise)
            {
                vectorH *= -h;
                vectorH += vectorActSchift;
            }
            else
            {
                vectorH *= h;
                vectorH += vectorActSchift;
            }*/

                    Vector3D pointShift = new Vector3D(vectorH.X, vectorH.Y, 0);

            Point3D newPoint = actPoint + pointShift;
            return newPoint;
        }

        private Point3D CalcNewPointArcArc(double angle, MoveData actCom, MoveData nextCom, double R1, double R2, double r1, double r2)
        {
            bool hPolNeg = false;
            if (angle == -180 || angle == 180)
            {
                if (actCom.clockwise)
                {
                    hPolNeg = true;
                }
                else
                {
                    hPolNeg = false;
                }
            }
            else if (compensation < 0) /* G42 */
            {
                if (actCom.clockwise == nextCom.clockwise)
                {
                    hPolNeg = false;
                }
                else
                {
                    hPolNeg = true;
                }
            }
            else /* G41 */
            {
                if (actCom.clockwise == nextCom.clockwise)
                {
                    hPolNeg = true;
                }
                else
                {
                    hPolNeg = false;
                }
            }

            Point3D cPNext = nextCom.centrePoint.GetPoint();
            Point3D cpAct = actCom.centrePoint.GetPoint();

            Vector vectorCentres = new Vector(cPNext.X - cpAct.X, cPNext.Y - cpAct.Y);
            double L = vectorCentres.Length;

            double x = (Math.Pow(R1 + r1, 2) - Math.Pow(R2 + r2, 2) + Math.Pow(L, 2)) / (2 * L);
            double h = Math.Sqrt(Math.Pow(R1 + r1, 2) - Math.Pow(x, 2));
            if (hPolNeg) { h = -h; }

            vectorCentres.Normalize();
            Vector vectorH = new Vector(vectorCentres.Y, -vectorCentres.X);

            Vector newPointVector = x * vectorCentres + h * vectorH;

            Vector3D pointShift = new Vector3D(newPointVector.X, newPointVector.Y, 0);

            Point3D newPoint = actCom.centrePoint.GetPoint() + pointShift;

            return newPoint;


        }

    }

    
}
