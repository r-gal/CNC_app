using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CNC3
{
    internal class GUI_axeRunPanel
    {
        Form1 form;
        Button[] modeButtons;

        CheckBox limIgnoreCheckBox;

        enum Mode_et
        {
            mode_cont,
            mode_1mm,
            mode_01mm,
            mode_001mm
        };

        Mode_et mode;

        private Button CreateAxeCtrlPanelButton(int posX, int posY, int len)
        {
            Button button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(posX, posY);
            button.Size = new System.Drawing.Size(len, 40);
            button.TabIndex = 11;

            button.UseVisualStyleBackColor = true;

            return button;
        }

    



        private void SetMode(Mode_et mode_)
        {
            modeButtons[0].BackColor = SystemColors.Control;
            modeButtons[1].BackColor = SystemColors.Control;
            modeButtons[2].BackColor = SystemColors.Control;
            modeButtons[3].BackColor = SystemColors.Control;

            modeButtons[(int)mode_].BackColor = SystemColors.ControlDark;

            mode = mode_;
        }

        private void modeButtonClick(object sender, EventArgs e)
        {
            int idx = (int)((Button)sender).Tag;

            SetMode((Mode_et)idx);

        }

        private void axeMoveButtonClick(object sender, EventArgs e)
        {
            if (mode != Mode_et.mode_cont)
            {
                ManualMoveData runData = new ManualMoveData();
                int idx = (int)((Button)sender).Tag;
                switch (mode)
                {
                    case Mode_et.mode_1mm: runData.dist = 1000; break;
                    case Mode_et.mode_01mm: runData.dist = 100; break;
                    case Mode_et.mode_001mm: runData.dist = 10; break;
                    default: return;
                }

                switch(idx)
                {
                    case 0: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_x; break;
                    case 1:                     runData.axe = ManualMoveData.Axe_et.axe_x; break;
                    case 2: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_y; break;
                    case 3:                     runData.axe = ManualMoveData.Axe_et.axe_y; break;
                    case 4: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_z; break;
                    case 5:                     runData.axe = ManualMoveData.Axe_et.axe_z; break;
                    case 6: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_a; break;
                    case 7:                     runData.axe = ManualMoveData.Axe_et.axe_a; break;
                    default: return;           
                }

                runData.ignoreLimiters = limIgnoreCheckBox.Checked;

                form.mainClass.Action(MainClass.Action_ET.AxeRun_Run, runData);
            }
        }
        private void axeMoveButtonDown(object sender, EventArgs e)
        {
            if (mode == Mode_et.mode_cont)
            {
                ManualMoveData runData = new ManualMoveData();
                runData.dist = 1;
                int idx = (int)((Button)sender).Tag;
                switch (idx)
                {
                    case 0: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_x; break;
                    case 1: runData.axe = ManualMoveData.Axe_et.axe_x; break;
                    case 2: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_y; break;
                    case 3: runData.axe = ManualMoveData.Axe_et.axe_y; break;
                    case 4: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_z; break;
                    case 5: runData.axe = ManualMoveData.Axe_et.axe_z; break;
                    case 6: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_a; break;
                    case 7: runData.axe = ManualMoveData.Axe_et.axe_a; break;
                    default: return;
                }

                form.mainClass.Action(MainClass.Action_ET.AxeRun_StartCont, runData);                
            }
        }
        private void axeMoveButtonUp(object sender, EventArgs e)
        {
            if (mode == Mode_et.mode_cont)
            {
                ManualMoveData runData = new ManualMoveData();
                runData.dist = 1;
                int idx = (int)((Button)sender).Tag;
                switch (idx)
                {
                    case 0: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_x; break;
                    case 1: runData.axe = ManualMoveData.Axe_et.axe_x; break;
                    case 2: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_y; break;
                    case 3: runData.axe = ManualMoveData.Axe_et.axe_y; break;
                    case 4: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_z; break;
                    case 5: runData.axe = ManualMoveData.Axe_et.axe_z; break;
                    case 6: runData.dist *= -1; runData.axe = ManualMoveData.Axe_et.axe_a; break;
                    case 7: runData.axe = ManualMoveData.Axe_et.axe_a; break;
                    default: return;
                }
                form.mainClass.Action(MainClass.Action_ET.AxeRun_StopCont, runData);
            }
        }

        public GUI_axeRunPanel(Form1 form_,Point position)
        {
            form = form_;

            string Name = "AXE RUN";
            

            GroupBox groupBox = new System.Windows.Forms.GroupBox();
            groupBox.Text = Name;
            groupBox.Location = position;
            groupBox.Size = new Size(300, 360);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            form.Controls.Add(groupBox);

            int col0_pos = 10;
            int row0_pos = 50;
            int row0_step = 40;

            modeButtons = new Button[4];

            modeButtons[0] = CreateAxeCtrlPanelButton(col0_pos, row0_pos, 80);
            modeButtons[0].Text = "Cont";
            modeButtons[0].Click += new System.EventHandler(this.modeButtonClick);

            modeButtons[0].Tag = 0;
            groupBox.Controls.Add(modeButtons[0]);

            row0_pos += row0_step;

            modeButtons[1] = CreateAxeCtrlPanelButton(col0_pos, row0_pos, 80);
            modeButtons[1].Text = "1.0mm";
            modeButtons[1].Click += new System.EventHandler(this.modeButtonClick);
            modeButtons[1].Tag = 1;
            groupBox.Controls.Add(modeButtons[1]);

            row0_pos += row0_step;

            modeButtons[2] = CreateAxeCtrlPanelButton(col0_pos, row0_pos, 80);
            modeButtons[2].Text = "0.1mm";
            modeButtons[2].Click += new System.EventHandler(this.modeButtonClick);
            modeButtons[2].Tag = 2;
            groupBox.Controls.Add(modeButtons[2]);

            row0_pos += row0_step;

            modeButtons[3] = CreateAxeCtrlPanelButton(col0_pos, row0_pos, 80);
            modeButtons[3].Text = "0.01mm";
            modeButtons[3].Click += new System.EventHandler(this.modeButtonClick);
            modeButtons[3].Tag = 3;
            groupBox.Controls.Add(modeButtons[3]);

            //int col1_pos = 100;
            int col1a_pos = 110;
            int col2_pos = 160;
            int col3_pos = 220;
            int col3a_pos = 210;

            int row1_pos = 60;
            int row2_pos = 120;
            int row3_pos = 180;
            int row4_pos = 230;
            int row5_pos = 280;



            Button btn = CreateAxeCtrlPanelButton(col2_pos, row1_pos, 40);
            btn.Text = "Z+";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col2_pos, row2_pos, 40);
            btn.Text = "Z-";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 4;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col3_pos, row1_pos, 40);
            btn.Text = "A+";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 7;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col3_pos, row2_pos, 40);
            btn.Text = "A-";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 6;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col3a_pos, row4_pos, 40);
            btn.Text = "X+";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 1;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col1a_pos, row4_pos, 40);
            btn.Text = "X-";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 0;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col2_pos, row3_pos, 40);
            btn.Text = "Y+";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 3;
            groupBox.Controls.Add(btn);

            btn = CreateAxeCtrlPanelButton(col2_pos, row5_pos, 40);
            btn.Text = "Y-";
            btn.Click += new System.EventHandler(this.axeMoveButtonClick);
            btn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonDown);
            btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.axeMoveButtonUp);
            btn.Tag = 2;
            groupBox.Controls.Add(btn);

            limIgnoreCheckBox = new CheckBox();
            limIgnoreCheckBox.Checked = false;
            limIgnoreCheckBox.Text = "Ign lim";
            limIgnoreCheckBox.Location = new Point(20, 310);
            limIgnoreCheckBox.Tag = 2;
            groupBox.Controls.Add(limIgnoreCheckBox);


            SetMode(Mode_et.mode_cont);
        }


    }
}
