using CNC3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
//using System.Windows.Media;
//using System.Windows.Media.Media3D;
using static myCnc2._3dgraph;
using myCnc2;
namespace CNC3
{


    public class Drawer
    {


        myCnc2._3dgraph uc;

        int h;
        int w;




        public Drawer()
        {




        }

        public delegate void CallbackDrawClosed();
        public event CallbackDrawClosed DrawClosed;

        private void Form_Closed(object sender, System.EventArgs e)
        {
            DrawClosed();
        }

        public void init3D()
        {
            h = 1000;
            w = 1000;


            Form newForm = new Form();

            newForm.FormClosed += Form_Closed;

            newForm.Height = h;
            newForm.Width = w;




            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Dock = DockStyle.Fill;

            // Create the WPF UserControl.
            uc = new myCnc2._3dgraph();

            DrawAxes();

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = uc;

            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
/*
            Point3D p0 = new Point3D(0, 0, 0);
            Point3D p1 = new Point3D(10, 0, 0);
            Point3D p2 = new Point3D(10, 10, 0);
            Point3D p3 = new Point3D(0, 10, 0);
            Point3D p4 = new Point3D(0, 0, 10);
            Point3D p5 = new Point3D(10, 0, 10);
            Point3D p6 = new Point3D(10, 10, 10);
            Point3D p7 = new Point3D(0, 10, 10);

            uc.DrawLine(p0, p1);
            uc.DrawLine(p1, p2);
            uc.DrawLine(p2, p3);
            uc.DrawLine(p3, p0);

            uc.DrawLine(p4, p5);
            uc.DrawLine(p5, p6);
            uc.DrawLine(p6, p7);
            uc.DrawLine(p7, p4);

            uc.DrawLine(p0, p4);
            uc.DrawLine(p1, p5);
            uc.DrawLine(p2, p6);
            uc.DrawLine(p3, p7);

            uc.DrawArc(new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(20, 0, 0), false,_3dgraph.ARC_PLANE_et.PLANE_XY, Colors.Purple);
            uc.DrawArc(new Point3D(0, 0, 0), new Point3D(0, 0, 10), new Point3D(0, 0, 20), false, _3dgraph.ARC_PLANE_et.PLANE_ZX, Colors.Purple);
            uc.DrawArc(new Point3D(0, 0, 0), new Point3D(0, 10, 0), new Point3D(0, 20, 0), false, _3dgraph.ARC_PLANE_et.PLANE_YZ, Colors.Purple);
*/
            //uc.DrawArc(new Point3D(10, 0, 5), new Point3D(0, 0, 5), new Point3D(0, 10, 5), false, _3dgraph.ARC_PLANE_et.PLANE_XY, Colors.Purple);

            newForm.Show();

        }

        internal void Draw(CNC3.MoveData moveData)
        {
            switch (moveData.orderCode)
            {
                case CNC3.MoveData.orderCode_et.LINE:
                {
                    Point3D pS = moveData.startPoint.GetPoint();
                    Point3D pE = moveData.endPoint.GetPoint();
                    if(moveData.rapidMove)
                    {
                        DrawLineLocal(pS, pE, System.Windows.Media.Colors.Blue, false);
                    }
                    else if(moveData.compensated)
                    {
                        DrawLineLocal(pS, pE, System.Windows.Media.Colors.Violet, false);
                    }
                    else
                    {
                        DrawLineLocal(pS, pE, System.Windows.Media.Colors.Purple, false);
                    }

                    }
                break;
                case CNC3.MoveData.orderCode_et.ARC2:
                { 
                    Point3D pS = moveData.startPoint.GetPoint();
                    Point3D pE = moveData.endPoint.GetPoint();
                    Point3D pC = moveData.centrePoint.GetPoint();
                    Vector3D v= moveData.rotationAxeVector;
                    if (moveData.compensated)
                    {
                        DrawArcLocal(pS, pC, pE, v, moveData.turns, System.Windows.Media.Colors.Violet);
                    }
                    else
                    {
                        DrawArcLocal(pS, pC, pE, v, moveData.turns, System.Windows.Media.Colors.Purple);
                    }
                            
                }
                break;

                default:
                    break;


            }


        }

        private void DrawLineLocal(Point3D p0, Point3D p1, System.Windows.Media.Color color,bool axe)
        {

            LineDraw lineDraw = new LineDraw();
            lineDraw.Points = new System.Windows.Media.Media3D.Point3D[2];
            lineDraw.Points[0] = new System.Windows.Media.Media3D.Point3D( p0.X,p0.Y, p0.Z);
            lineDraw.Points[1] = new System.Windows.Media.Media3D.Point3D(p1.X, p1.Y, p1.Z);
            lineDraw.color = color;
            lineDraw.width = 0.3F;

            uc.DrawChain(lineDraw,axe);

        }

        private void DrawArcLocal(Point3D startPoint_, Point3D centrePoint_, Point3D endPoint_, Vector3D rotVector, int turns, System.Windows.Media.Color color)
        {
            double startAngle = 0;
            double endAngle = 0;
            /* rotate to plane */

            RotMatrix[] rotMatrixArray = CNC3.RotMatrix.GetRotMatrix(rotVector);


            Point3D startPoint = rotMatrixArray[1] * startPoint_;
            Point3D centrePoint = rotMatrixArray[1] * centrePoint_;
            Point3D endPoint = rotMatrixArray[1] * endPoint_;

            /* calc points */

            if (turns > 0)
            {
                startAngle = Math.Atan2(startPoint.Y - centrePoint.Y, startPoint.X - centrePoint.X);
                endAngle = startAngle - (2 * Math.PI * turns);
            }
            else
            {
                startAngle = Math.Atan2(startPoint.Y - centrePoint.Y, startPoint.X - centrePoint.X);
                endAngle = Math.Atan2(endPoint.Y - centrePoint.Y, endPoint.X - centrePoint.X);
            }


            double R = Math.Sqrt(Math.Pow(startPoint.X - centrePoint.X, 2) + Math.Pow(startPoint.Y - centrePoint.Y, 2));
            float segmentLength = 0.1F;
            double angleStep = segmentLength / R;
            float width = 0.3F;


            if (endAngle > startAngle) { startAngle += 2 * Math.PI; }
            int points = (int)((startAngle - endAngle) / angleStep);


            LineDraw lineDraw = new LineDraw();
            lineDraw.Points = new System.Windows.Media.Media3D.Point3D[points];
            lineDraw.color = color;
            lineDraw.width = 0.3F;


            double progress = 0;
            for (int i = 0; i < points; i++)
            {
                progress = (double)i / (double)points;

                double currentAngle = startAngle + ((endAngle - startAngle) * progress);

                double x = 0;
                double y = 0;
                double z = 0;
                x = Math.Cos(currentAngle) * R + centrePoint.X;
                y = Math.Sin(currentAngle) * R + centrePoint.Y;
                z = startPoint.Z + ((endPoint.Z - startPoint.Z) * progress);

                /* rotate from plane */

                Point3D pt = new Point3D(x, y, z);

                Point3D pt_ = rotMatrixArray[0] * pt;


                lineDraw.Points[i] = new System.Windows.Media.Media3D.Point3D(pt_.X, pt_.Y, pt_.Z);

            }

            uc.DrawChain(lineDraw, false);
        }

        private void DrawAxes()
        {
            DrawAxe(new Vector3D(1, 0, 0), new Vector3D(0, 0, 1),System.Windows.Media.Colors.Blue);
            DrawAxe(new Vector3D(0, 1, 0), new Vector3D(0, 0, 1),System.Windows.Media.Colors.Green);
            DrawAxe(new Vector3D(0, 0, 1), new Vector3D(1, 0, 0),System.Windows.Media.Colors.Violet);
        }

        private void DrawAxe(Vector3D axeVector, Vector3D ortVector, System.Windows.Media.Color color)
        {
            float axeLen = 100;
            float arrowSize = 5;
            float signSize = 1;
            /* axe X */
            Point3D p1 = new Point3D(0, 0, 0) + axeVector * axeLen;
            Point3D p2 = new Point3D(0, 0, 0) - axeVector * axeLen;

            ortVector.Normalize();
            Vector3D arr = ortVector * arrowSize;
            Point3D pArr = new Point3D(0, 0, 0) + axeVector * (axeLen - arrowSize);
            Point3D pArr_A = pArr + arr;
            Point3D pArr_B = pArr - arr;

            DrawLineLocal(p1, p2, color,true);
            DrawLineLocal(pArr_A, p1, color, true);
            DrawLineLocal(pArr_B, p1, color, true);

            for (float pos = -axeLen; pos < 0; pos += 10)
            {
                Point3D p1a = new Point3D(0, 0, 0) + axeVector * pos + ortVector * signSize;
                Point3D p2a = new Point3D(0, 0, 0) + axeVector * pos - ortVector * signSize;
                DrawLineLocal(p1a, p2a, color, true);
            }
            for (float pos = 10; pos < axeLen; pos += 10)
            {
                Point3D p1a = new Point3D(0, 0, 0) + axeVector * pos + ortVector * signSize;
                Point3D p2a = new Point3D(0, 0, 0) + axeVector * pos - ortVector * signSize;
                DrawLineLocal(p1a, p2a, color, true);
            }
        }










    }
}
