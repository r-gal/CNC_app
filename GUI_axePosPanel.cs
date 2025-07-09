using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CNC3
{


    internal class GUI_axePosPanel
    {
        Form1 form;

        TextBox[] textBox_local = new TextBox[4];
        TextBox[] textBox_base =  new TextBox[4];
        TextBox[] textBox_baseFb = new TextBox[4];

        public void SetPosition(double x, double y, double z, double a)
        {
            textBox_base[0].Text = x.ToString() + " [mm]";
            textBox_base[1].Text = y.ToString() + " [mm]";
            textBox_base[2].Text = z.ToString() + " [mm]";
            textBox_base[3].Text = a.ToString() + " [mm]";

        }

        public void SetPositionLocal(double x, double y, double z, double a)
        {
            textBox_local[0].Text = x.ToString() + " [mm]";
            textBox_local[1].Text = y.ToString() + " [mm]";
            textBox_local[2].Text = z.ToString() + " [mm]";
            textBox_local[3].Text = a.ToString() + " [mm]";

        }

        public void SetPositionReal(double x, double y, double z, double a)
        {

            textBox_baseFb[0].Text = x.ToString() + " [mm]";
            textBox_baseFb[1].Text = y.ToString() + " [mm]";
            textBox_baseFb[2].Text = z.ToString() + " [mm]";
            textBox_baseFb[3].Text = a.ToString() + " [mm]";
        }

        public GUI_axePosPanel(Form1 form_, Point position)
        {
            form = form_;



            string Name = "AXE POSITION";


            GroupBox groupBox = new System.Windows.Forms.GroupBox();
            groupBox.Text = Name;
            groupBox.Location = position;
            groupBox.Size = new Size(300, 320);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

            form.Controls.Add(groupBox);

            Label label;
            TextBox textBox;

            string axeChars = "XYZA";


            label = new Label();
            label.Location = new System.Drawing.Point(30, 40 );
            label.Size = new Size(80, 30);
            label.Text = "LOCAL";
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            label.TabIndex = 8;
            groupBox.Controls.Add(label);

            label = new Label();
            label.Location = new System.Drawing.Point(120, 40 );
            label.Size = new Size(80, 30);
            label.Text = "ABS";
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            label.TabIndex = 8;
            groupBox.Controls.Add(label);

            label = new Label();
            label.Location = new System.Drawing.Point(210, 40 );
            label.Size = new Size(80, 30);
            label.Text = "REAL";
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            label.TabIndex = 8;
            groupBox.Controls.Add(label);

            for (int i=0;i<4; i++)
            {
                label = new Label();
                label.Location = new System.Drawing.Point(10, 80 + i*40);
                label.Size = new Size(20, 30);
                label.Text = axeChars[i].ToString();
                label.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                label.TabIndex = 11;
                groupBox.Controls.Add(label);

                textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(30, 80 + i * 40);
                textBox.Size = new Size(80, 30);
                textBox.TabStop = false;
                textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                textBox.TabIndex = 11;
                groupBox.Controls.Add(textBox);
                textBox_local[i] = textBox;

                textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(120, 80 + i * 40);
                textBox.Size = new Size(80, 30);
                textBox.TabStop = false;
                textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                textBox.TabIndex = 11;
                groupBox.Controls.Add(textBox);
                textBox_base[i] = textBox;

                textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(210, 80 + i * 40);
                textBox.Size = new Size(80, 30);
                textBox.TabStop = false;
                textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                textBox.TabIndex = 11;
                groupBox.Controls.Add(textBox);
                textBox_baseFb[i] = textBox;

            }

        }
    }
}
