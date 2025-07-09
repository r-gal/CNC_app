using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;
using System.Xml;
using System.IO;

namespace CNC3
{
    public partial class Form_surfaceProbe : Form
    {

        Form1 mainForm;
        public Form_surfaceProbe(Form1 mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;

            LoadLanguage();
        }

        private void LoadLanguage()
        {
            XmlDocument ling = new XmlDocument();
            bool ok = false;

            if (File.Exists("Ling.xml"))
            {
                try
                {
                    ling.Load("Ling.xml");
                    ok = true;
                }
                catch
                {

                }
            }

            if (ok)
            {
                XmlNode node;

                try
                {
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Title");
                    this.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Info");
                    info_Label.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Step");
                    step_Label.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Size");
                    size_Label.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/StartPos");
                    StartPos_Label.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Run");
                    run_Button.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Cancel");
                    cancel_Button.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Clear");
                    clear_Button.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/SurfaceProbe/Refresh");
                    refresh_Button.Text = node.InnerText;
                }
                catch { ok = false; }
            }
        }

        private void RefreshDataGrid()
        {
            int xSize = mainForm.mainClass.moveController.surfaceProbes.GetSizeX();
            int ySize = mainForm.mainClass.moveController.surfaceProbes.GetSizeY();
            int xStep = mainForm.mainClass.moveController.surfaceProbes.GetStepX();
            int yStep = mainForm.mainClass.moveController.surfaceProbes.GetStepY();
            int xStart = mainForm.mainClass.moveController.surfaceProbes.GetStartX();
            int yStart = mainForm.mainClass.moveController.surfaceProbes.GetStartY();

            stepX_textBox.Text = (xStep/1000).ToString();
            stepY_textBox.Text = (yStep/1000).ToString();
            startPosX_textBox.Text = ((double)xStart/1000).ToString();
            startPosY_textBox.Text = ((double)yStart/1000).ToString();

            double xSizeF = 0;
            double ySizeF = 0;
            if (xSize>0)
            {
                xSizeF = ( xSize  -1) * xStep * 0.001;
            }
            sizeX_textBox.Text = xSizeF.ToString();
            if (ySize > 0)
            {
                ySizeF = (ySize  -1) * yStep * 0.001;
            }
            sizeY_textBox.Text = ySizeF.ToString();



            dataGridView1.Columns.Clear();


            dataGridView1.Columns.Add("posX", "X");
            dataGridView1.Columns[0].Width = 40;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

            for (int x=0;x<xSize;x++)
            {
                dataGridView1.Columns.Add("colX" + x.ToString(), x.ToString() );
                dataGridView1.Columns[x+1].Width = 40;
                dataGridView1.Columns[x+1].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dataGridView1.Rows.Add();

            dataGridView1.Rows[0].HeaderCell.Value = "Y";

            dataGridView1.Rows[0].Cells[0].Value = "POS";

            for (int x = 0; x < xSize; x++)
            {
                dataGridView1.Rows[0].Cells[1 + x].Value = ((double)(x * xStep) * 0.001);
                dataGridView1.Rows[0].Cells[1 + x].Style.BackColor = System.Drawing.Color.Gray;
            }

            for(int y = 0; y < ySize; y++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[1 + y].HeaderCell.Value = y.ToString();
                dataGridView1.Rows[1+y].Cells[0].Value = ((double)(y * yStep)*0.001);
                dataGridView1.Rows[1 + y].Cells[0].Style.BackColor = System.Drawing.Color.Gray;

                for (int x = 0; x < xSize; x++)
                {
                    int val = mainForm.mainClass.moveController.surfaceProbes.GetProbe(x, y);
                    dataGridView1.Rows[1 + y].Cells[1+x].Value = ((double)(val) * 0.001);
                }

            }

            dataGridView1.Columns[0].Frozen = true;
            
            dataGridView1.Rows[0].Cells[0].Style.BackColor = SystemColors.ControlLightLight;
            dataGridView1.Rows[0].Frozen = true;



        }


        private void cancel_Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void refresh_Button_Click(object sender, EventArgs e)
        {
            RefreshDataGrid();
        }

        private void clear_Button_Click(object sender, EventArgs e)
        {
            mainForm.mainClass.moveController.ClearSurfaceProbe();
        }

        private void run_Button_Click(object sender, EventArgs e)
        {
            int xSize = 0;
            int ySize = 0;
            int xStep = 0;
            int yStep = 0;

            int endPosX = 0; 
            int endPosY = 0;

            bool ok = true;
            try
            {
                endPosX = (int)(Double.Parse(sizeX_textBox.Text) * 1000);
                endPosY = (int)(Double.Parse(sizeY_textBox.Text) * 1000);
                xStep = (int)(Double.Parse(stepX_textBox.Text) * 1000);
                yStep = (int)(Double.Parse(stepY_textBox.Text) * 1000);
            }
            catch 
            {
                ok = false;
            }

            if (ok)
            {
                if(endPosX <= 0) { ok = false; }
                if (endPosY <= 0) { ok = false; }
                if (xStep <= 0) { ok = false; }
                if (yStep <= 0) { ok = false; }
            }

            if(ok)
            {
                xSize = (endPosX / xStep)+1;
                ySize = (endPosY / yStep)+1;
                if (xSize < 2) { ok = false; }
                if (ySize < 2) { ok = false; }
            }

            if (ok)
            {
                mainForm.mainClass.moveController.RunSurfaceProbe(xSize, ySize, xStep, yStep);
            }           
        }
    }
}
