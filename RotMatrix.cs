using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CNC3
{
    internal class RotMatrix
    {
        double[,] data = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

        RotMatrix()
        {
            
        }

        RotMatrix(double angle, char axe)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            switch (axe)
            {
                default:
                case 'X':
                    data[0, 0] = 1;
                    data[0, 1] = 0;
                    data[0, 2] = 0;
                    data[1, 0] = 0;
                    data[1, 1] = cosAngle;
                    data[1, 2] = -sinAngle;
                    data[2, 0] = 0;
                    data[2, 1] = sinAngle;
                    data[2, 2] = cosAngle;
                    break;

                case 'Y':
                    data[0, 0] = cosAngle;
                    data[0, 1] = 0;
                    data[0, 2] = sinAngle;
                    data[1, 0] = 0;
                    data[1, 1] = 1;
                    data[1, 2] = 0;
                    data[2, 0] = -sinAngle;
                    data[2, 1] = 0;
                    data[2, 2] = cosAngle;
                    break;

                case 'Z':
                    data[0, 0] = cosAngle;
                    data[0, 1] = -sinAngle;
                    data[0, 2] = 0;
                    data[1, 0] = sinAngle;
                    data[1, 1] = cosAngle;
                    data[1, 2] = 0;
                    data[2, 0] = 0;
                    data[2, 1] = 0;
                    data[2, 2] = 1;
                    break;
            }



        }

        static public RotMatrix[] GetRotMatrix(Vector3D vector)
        {
            RotMatrix rotMatrix = new RotMatrix();

            

            RotMatrix rM;
            RotMatrix rMInv;

            if ((vector.X == 0) && (vector.Y == 0))
            {

                if (vector.Z < 0)
                {
                    /* simple inversion */
                    rotMatrix.data[1, 1] = -1;
                    rotMatrix.data[2, 2] = -1;
                }
                else
                {
                    /*nothing to do */

                }
                rM = rotMatrix;
                rMInv = rotMatrix;
            }
            else
            {
                double angleZ = Math.Atan2(vector.Y, vector.X);
                double angleY = Math.Acos(vector.Z / vector.Length);

                RotMatrix rotY = new RotMatrix(angleY,'Y');
                //RotMatrix rotY = new RotMatrix(angleY, 'X');
                RotMatrix rotZ = new RotMatrix(angleZ,'Z');

                RotMatrix rotYinv = new RotMatrix(-angleY,'Y');
                //RotMatrix rotYinv = new RotMatrix(-angleY, 'X');
                RotMatrix rotZinv = new RotMatrix(-angleZ,'Z');
                rM = rotZ * rotY;
                rMInv = rotYinv * rotZinv;
            }

            RotMatrix[] rmArray = { rM, rMInv };
            return rmArray;

        }





        public static RotMatrix operator *(RotMatrix a, RotMatrix m)
  => RotMatrix.multMM(a, m);

        static RotMatrix multMM(RotMatrix a, RotMatrix m)
        {
            RotMatrix res = new RotMatrix();

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    double val = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        val += a.data[r,i] * m.data[i,c];
                    }
                    res.data[r,c] = val;
                }
            }
            return res;
        }

        public static Vector3D operator *(RotMatrix m,Vector3D v)
  => RotMatrix.multMV(m, v);

        static Vector3D multMV(RotMatrix m, Vector3D v)
        {
            double[] vData = { v.X, v.Y, v.Z };
            double[] vOutData = new double[3];


            for (int r = 0; r < 3; r++)
            {

                double val = 0;

                for (int i = 0; i < 3; i++)
                {
                    val += m.data[r,i] * vData[i];
                }
                vOutData[r] = val;

            }

            Vector3D res = new Vector3D(vOutData[0], vOutData[1], vOutData[2]);
            return res;
        }

        public static Point3D operator *(RotMatrix m, Point3D v)
=> RotMatrix.multMP(m, v);

        static Point3D multMP(RotMatrix m, Point3D v)
        {
            double[] vData = { v.X, v.Y, v.Z };
            double[] vOutData = new double[3];


            for (int r = 0; r < 3; r++)
            {

                double val = 0;

                for (int i = 0; i < 3; i++)
                {
                    val += m.data[r, i] * vData[i];
                }
                vOutData[r] = val;

            }

            Point3D res = new Point3D(vOutData[0], vOutData[1], vOutData[2]);
            return res;
        }
    }
}
