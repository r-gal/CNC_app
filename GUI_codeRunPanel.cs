using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace CNC3
{
    internal class GUI_codeRunPanel
    {
        Form1 form;

        TextBox tbox;
        private void CodeRunButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.CodeRun_Start, null);
        }

        private void CodeRunFromButtonClick(object sender, EventArgs e)
        {
            int idx = 0;
            try
            {
                idx = int.Parse(tbox.Text);
                form.mainClass.Action(MainClass.Action_ET.CodeRun_RunFromLine, idx);

            }
            catch
            {

            }
        }

        private void CodeResetButtonClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                form.mainClass.Action(MainClass.Action_ET.CodeRun_Reset, null);
            }
        }

        private void CodeStopButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.CodeRun_Stop, null);
        }

        private void CodeResumeButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.CodeRun_Resume, null);
        }        

        private void CodeCompileButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.CodeCompile, null);
        }
        private void CodeSimulateButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.CodeSimulate, null);
        }

        public GUI_codeRunPanel(Form1 form_, Point position)
        {
            form = form_;

            string Name = "CODE RUN";


            GroupBox groupBox = new System.Windows.Forms.GroupBox();
            groupBox.Text = Name;
            groupBox.Location = position;
            groupBox.Size = new Size(300, 360);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

            form.Controls.Add(groupBox);

            Button btn;

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(50, 30);
            btn.Size = new System.Drawing.Size(100, 30);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "COMPILE";
            btn.Click += new System.EventHandler(this.CodeCompileButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(160, 30);
            btn.Size = new System.Drawing.Size(100, 30);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "SIMULATE";
            btn.Click += new System.EventHandler(this.CodeSimulateButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(50, 70);
            btn.Size = new System.Drawing.Size(200, 40);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "RUN";
            btn.Click += new System.EventHandler(this.CodeRunButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(50, 120);
            btn.Size = new System.Drawing.Size(90, 30);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "STOP";
            btn.Click += new System.EventHandler(this.CodeStopButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(160, 120);
            btn.Size = new System.Drawing.Size(90, 30);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "RESUME";
            btn.Click += new System.EventHandler(this.CodeResumeButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(50, 160);
            btn.Size = new System.Drawing.Size(90, 30);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "RESET";
            btn.Click += new System.EventHandler(this.CodeResetButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            btn = new System.Windows.Forms.Button();
            btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            btn.Location = new System.Drawing.Point(50, 220);
            btn.Size = new System.Drawing.Size(90, 30);
            btn.TabIndex = 11;
            btn.UseVisualStyleBackColor = true;
            btn.Text = "RUN:";
            btn.Click += new System.EventHandler(this.CodeRunFromButtonClick);
            btn.Tag = 5;
            groupBox.Controls.Add(btn);

            tbox = new TextBox();
            tbox.Location = new System.Drawing.Point(160, 220);
            tbox.Size = new System.Drawing.Size(90, 30);
            tbox.Text = "0";
            tbox.TabIndex = 11;
            tbox.Tag = 5;
            groupBox.Controls.Add(tbox);
        }


    }
}
