using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNC3
{
    internal class My3D
    {
        
    }

    public class Vector3D
    {
        public double X;
        public double Y;
        public double Z;
        public double Length;

        public Vector3D(double X_, double Y_, double Z_)
        {
            X = X_;
            Y = Y_;
            Z = Z_;
            Length = Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3D(Vector3D a)
        {
            X = a.X;
            Y = a.Y;
            Z = a.Z;
            Length = a.Length;
        }

        public void Normalize()
        {
            X /= Length; Y /= Length; Z /= Length; Length = 1;

        }

        public static  double AngleBetween(Vector3D v1, Vector3D v2)
        {
            if(v1.Length >0 && v2.Length >0)
            {
                double angle = (v1.X*v2.X + v1.Y*v2.Y +  v1.Z*v2.Z) / (v1.Length *  v2.Length);
                return Math.Acos(angle);
            }
            else
            {
                return 0;
            }
        }


        public static Point3D operator +(Point3D a, Vector3D b)
          => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Point3D operator -(Point3D a, Vector3D b)
  => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3D operator *(Vector3D a, double m)
          => new Vector3D(a.X*m, a.Y*m, a.Z*m);


    }
    public class Point3D
    {
        public double X;
        public double Y;
        public double Z;

        public Point3D(double X_, double Y_, double Z_)
        {
            X = X_;
            Y = Y_;
            Z = Z_;
        }

        public static Vector3D operator -(Point3D a, Point3D b)
          => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);


    }

    public class Vector
    {
        public double X;
        public double Y;

        public double Length;

        public Vector(double X_, double Y_)
        {
            X = X_;
            Y = Y_;
            Length = Math.Sqrt(X * X + Y * Y );
        }

        public Vector(Vector a)
        {
            X = a.X;
            Y = a.Y;
            Length = a.Length;
        }

        public void Normalize()
        {
            X /= Length; Y /= Length;  Length = 1;

        }

        public static double AngleBetween(Vector v1, Vector v2)
        {
            if (v1.Length > 0 && v2.Length > 0)
            {
                /*
                double angle = (v1.X * v2.X + v1.Y * v2.Y) / (v1.Length * v2.Length);
                return Math.Acos(angle);
                */
                double a1 = Math.Atan2(v1.Y, v1.X);
                double a2 = Math.Atan2(v2.Y, v2.X);
                double a = a2 - a1;

                if(a > Math.PI) { a -= 2*Math.PI; }
                else if (a < -Math.PI) { a += 2*Math.PI; }
                return a;
            }
            else
            {
                return 0;
            }
        }

        public static Vector operator *(Vector a, double m)
  => new Vector(a.X * m, a.Y * m);
        public static Vector operator *(double m, Vector a)
=> new Vector(a.X * m, a.Y * m);
        
        public static Vector operator +(Vector a, Vector b)
  => new Vector(a.X + b.X, a.Y + b.Y);

        public static Vector operator -(Vector a, Vector b)
=> new Vector(a.X - b.X, a.Y - b.Y);
    };





}



