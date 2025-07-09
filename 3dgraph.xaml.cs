using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace myCnc2
{
    /// <summary>
    /// Logika interakcji dla klasy _3dgraph.xaml
    /// </summary>
    /// 
    public  class LineDraw
    {
        internal Point3D[] Points;

        internal float width;
        internal System.Windows.Media.Color color;


    };
    public partial class _3dgraph : UserControl
    {
        Vector3D lookVector;
        Point3D lookPoint;
        double zoom;
        Point3D cameraPos;
        OrthographicCamera camera;

        List<LineDraw> arraysLines = new List<LineDraw>();
        List<LineDraw> modelLines = new List<LineDraw>();



        public void DrawChain(LineDraw chain,bool axe)
        {
            if(axe)
            {
                arraysLines.Add(chain);
            }
            else
            {
                modelLines.Add(chain);
            }
            
            DrawLineChain(chain);
        }


        private void DrawLineChain(LineDraw chain)
        {
            if(chain.Points.Length < 2) return;
            Material material = new DiffuseMaterial(new SolidColorBrush(chain.color));
            MeshGeometry3D mesh = new MeshGeometry3D();


            AddPointWithWidth(mesh, chain.Points[0], chain.Points[1] - chain.Points[0], chain.width);
            for (int i = 1;i<chain.Points.Length;i++)
            {
                AddPointWithWidth(mesh, chain.Points[i], chain.Points[i] - chain.Points[i-1], chain.width);

                int offset = 2 * (i-1);
                mesh.TriangleIndices.Add(0 + offset);
                mesh.TriangleIndices.Add(2 + offset);
                mesh.TriangleIndices.Add(3 + offset);
                mesh.TriangleIndices.Add(3 + offset);
                mesh.TriangleIndices.Add(1 + offset);
                mesh.TriangleIndices.Add(0 + offset);
            }
            var modelsGroup = new Model3DGroup();

            modelsGroup.Children.Add(new GeometryModel3D(mesh, material));
            modelsGroup.Children.Add(new AmbientLight(Colors.White));

            ModelVisual3D Model = new ModelVisual3D();
            Model.Content = modelsGroup;
            this.myViewport.Children.Add(Model);

        }

        



        private void AddPointWithWidth(MeshGeometry3D mesh, Point3D point, Vector3D lineVector, float width)
        {
            Vector3D widthVector = Vector3D.CrossProduct(lineVector, lookVector);
            widthVector.Normalize();
            widthVector *= (width / 2);
            mesh.Positions.Add(point - widthVector);
            mesh.Positions.Add(point + widthVector);
        }



        void RedrawAll()
        {
            this.myViewport.Children.Clear();

            foreach(var item in arraysLines)
            {
                DrawLineChain(item);
            }

            foreach (var item in modelLines)
            {
                DrawLineChain(item);
            }
        }

        private void UpdateCamera()
        {
            Vector3D posVector = lookVector*1000;
            Point3D cameraPos = lookPoint - posVector;

            ((OrthographicCamera)(myViewport.Camera)).Position = cameraPos;
            ((OrthographicCamera)(myViewport.Camera)).LookDirection = lookVector;
            ((OrthographicCamera)(myViewport.Camera)).Width = 100000/zoom;
        }

        private void ZoomMinus(object sender, RoutedEventArgs e)
        {
            zoom /= 2;
            UpdateCamera();
            RedrawAll();
        }
        private void ZoomPlus(object sender, RoutedEventArgs e)
        {
            zoom *= 2;
            UpdateCamera();
            RedrawAll();
        }
        private void MoveLeft(object sender, RoutedEventArgs e)
        {
            Vector3D moveVector = new Vector3D(lookVector.Y,-lookVector.X,0);
            lookPoint += moveVector;
            UpdateCamera();
        }
        private void MoveRight(object sender, RoutedEventArgs e)
        {
            Vector3D moveVector = new Vector3D(-lookVector.Y, lookVector.X, 0);
            lookPoint += moveVector;
            UpdateCamera();
        }
        private void MoveForward(object sender, RoutedEventArgs e)
        {
            Vector3D moveVector = new Vector3D(lookVector.X, lookVector.Y, 0);
            lookPoint += moveVector;
            UpdateCamera();
        }
        private void MoveBackward(object sender, RoutedEventArgs e)
        {
            Vector3D moveVector = new Vector3D(-lookVector.X, -lookVector.Y, 0);
            lookPoint += moveVector;
            UpdateCamera();
        }

        private void RotateLeft(object sender, RoutedEventArgs e)
        {
            Vector3D newLookVector = lookVector;
            newLookVector.Z = 0;
            newLookVector = Vector3D.CrossProduct(newLookVector, new Vector3D(0,0,1));
            newLookVector.Z = lookVector.Z;
            lookVector = newLookVector;
            UpdateCamera();
            RedrawAll();
        }
        private void RotateRight(object sender, RoutedEventArgs e)
        {
            Vector3D newLookVector = lookVector;
            newLookVector.Z = 0;
            newLookVector = Vector3D.CrossProduct(newLookVector, new Vector3D(0, 0, -1));
            newLookVector.Z = lookVector.Z;
            lookVector = newLookVector;
            UpdateCamera();
            RedrawAll();
        }

        public _3dgraph()
        {
            InitializeComponent();

            arraysLines.Clear();
            modelLines.Clear();

            lookVector = new Vector3D(-4, 3, -2);
            lookPoint = new Point3D(0, 0, 0);
            zoom = 400;      

            camera = new OrthographicCamera();
            camera.Position = cameraPos;
            camera.LookDirection = lookVector;
            camera.NearPlaneDistance = 1;
            camera.FarPlaneDistance = 100000;
            camera.UpDirection = new Vector3D(0, 0, 1);
            myViewport.Camera = camera;

            UpdateCamera();



 
            /*       Point3D p0 = new Point3D(0, 0, 1);
                   Point3D p1 = new Point3D(1, -1, 0);
                   Point3D p2 = new Point3D(-1, 1, 0);

                   var modelsGroup = new Model3DGroup();

                   modelsGroup.Children.Add(AddTriangle(p0, p1, p2));
                   modelsGroup.Children.Add(new AmbientLight(Colors.White));

                   ModelVisual3D Model = new ModelVisual3D();
                   Model.Content = modelsGroup;
                   this.myViewport.Children.Add(Model);*/
        }
    }
}
