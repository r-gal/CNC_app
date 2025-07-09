using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace CNC3
{
    internal class GoToData
    {
        public bool globalPos;
        public Coord coord;
        public int safeZ;
    }


    internal class GUI_GoToPanel
    {

        Form1 form;

        TextBox[,] tBox= new TextBox[2,4];

        TextBox[] safeZtBox = new TextBox[2];





        CheckBox[] globalPos = new CheckBox[2];

        private void GoToButtonClick(object sender, EventArgs e)
        {
            

            int idx = (int)((Button)sender).Tag;


            double[] pos = { 0, 0, 0, 0 };
            int[] intPos = new int[4];
            int safeInt = 0;

            for(int i=0;i<4;i++)
            {
                try
                {
                    pos[i] = double.Parse(tBox[idx,i].Text);

                    intPos[i] = (int)(pos[i] * 1000);
                }
                catch
                {
                    return;
                }
            }

            try
            {
                double safe = double.Parse(safeZtBox[idx].Text);
                safeInt = (int)(safe * 1000);
            }
            catch
            {
                return;
            }

            GoToData data = new GoToData();

            data.coord = new Coord(intPos[0], intPos[1], intPos[2], intPos[3]);
            data.safeZ = safeInt;
            data.globalPos = globalPos[idx].Checked;

            form.mainClass.Action(MainClass.Action_ET.AxeBase_GoTo, data);


        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            //form.mainClass.Action(MainClass.Action_ET.AxeBase_GoLocal, null);

            int idx = (int)((Button)sender).Tag;

            Coord pos;

            if (globalPos[idx].Checked)
            {
                pos = form.mainClass.executor.GetActPos();
                safeZtBox[idx].Text = "0";
            }
            else
            {
                pos = form.mainClass.executor.GetActPosLocal();
                safeZtBox[idx].Text = "20";
            }

            tBox[idx, 0].Text = (((double)pos.x) / 1000).ToString();
            tBox[idx, 1].Text = (((double)pos.y) / 1000).ToString();
            tBox[idx, 2].Text = (((double)pos.z) / 1000).ToString();
            tBox[idx, 3].Text = (((double)pos.a) / 1000).ToString();


        }

        public GUI_GoToPanel(Form1 form_, Point position)
        {
            form = form_;

            string Name = "GO TO";

            string axeChars = "XYZA";

            GroupBox groupBox = new System.Windows.Forms.GroupBox();
            groupBox.Text = Name;
            groupBox.Location = position;
            groupBox.Size = new Size(300, 320);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

            form.Controls.Add(groupBox);

            Label label;

            int yPos = 40;
            int xPos = 10;


            for (int i = 0; i < 4; i++)
            {
                label = new Label();
                label.Location = new System.Drawing.Point(xPos, yPos);
                label.Size = new Size(50, 30);
                label.Text = axeChars[i].ToString();
                label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                label.TabIndex = 8;
                groupBox.Controls.Add(label);

                yPos += 30;
            }
            label = new Label();
            label.Location = new System.Drawing.Point(xPos, yPos);
            label.Size = new Size(60, 30);
            label.Text = "SAFE Z";
            label.Font = new System.Drawing.Font("Microsoft Sans Serif",12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            label.TabIndex = 8;
            groupBox.Controls.Add(label);


            xPos = 70;

            for (int i=0;i<2;i++)
            {
                yPos = 40;

                for(int j=0;j<4;j++)
                {
                    tBox[i,j] = new TextBox();
                    tBox[i,j].Location = new System.Drawing.Point(xPos, yPos);
                    tBox[i,j].Size = new System.Drawing.Size(80, 25);
                    tBox[i,j].Text = "0";
                    tBox[i,j].TabIndex = 8;
                    tBox[i,j].Tag = 5;
                    groupBox.Controls.Add(tBox[i,j]);

                    yPos += 30;
                }

                safeZtBox[i] = new TextBox();
                safeZtBox[i].Location = new System.Drawing.Point(xPos, yPos);
                safeZtBox[i].Size = new System.Drawing.Size(80, 25);
                safeZtBox[i].Text = "0";
                safeZtBox[i].TabIndex = 8;
                safeZtBox[i].Tag = 5;
                groupBox.Controls.Add(safeZtBox[i]);

                yPos += 30;

                Button button;

                button = new System.Windows.Forms.Button();
                button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                button.Location = new System.Drawing.Point(xPos, yPos);
                button.Size = new System.Drawing.Size(80, 30);
                button.TabIndex = 10;
                button.UseVisualStyleBackColor = true;
                button.Text = "SAVE";
                button.Click += new System.EventHandler(this.SaveButtonClick);
                button.Tag = i;
                groupBox.Controls.Add(button);

                yPos += 40;

                button = new System.Windows.Forms.Button();
                button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                button.Location = new System.Drawing.Point(xPos, yPos);
                button.Size = new System.Drawing.Size(80, 40);
                button.TabIndex = 10;
                button.UseVisualStyleBackColor = true;
                button.Text = "GO TO";
                button.Click += new System.EventHandler(this.GoToButtonClick );
                button.Tag = i;
                groupBox.Controls.Add(button);

                yPos += 50;

                globalPos[i] = new CheckBox();
                globalPos[i].Location = new System.Drawing.Point(xPos, yPos);
                globalPos[i].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                globalPos[i].Text = "GLOBAL POS";
                globalPos[i].Checked = false;
                globalPos[i].TabIndex = 10;
                groupBox.Controls.Add(globalPos[i]);

                

                xPos += 120;
            }


        }
    }
}
