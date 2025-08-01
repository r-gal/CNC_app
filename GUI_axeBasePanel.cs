using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Media;
using System.Xml.Linq;
using static CNC3.ManualMoveData;

namespace CNC3


{

    public class SetBaseData
    {
        internal double offset;
        internal Axe_et axe;
    };

    internal class GUI_axeBasePanel
    {
        Form1 form;
        TextBox tbox;

        Button[] macroButtons = new Button[Constants.NO_OF_MACROS];
        private void SetOffsetButtonClick(object sender, EventArgs e)
        {
            SetBaseData baseData = new SetBaseData();
            int idx = (int)((Button)sender).Tag;

            switch(idx)
            {
                default:
                case 0: baseData.axe = Axe_et.axe_x; break;
                case 1: baseData.axe = Axe_et.axe_y; break;
                case 2: baseData.axe = Axe_et.axe_z; break;
                case 3: baseData.axe = Axe_et.axe_a; break;
            }

            try
            {
                baseData.offset = double.Parse(tbox.Text);
                form.mainClass.Action(MainClass.Action_ET.AxeBase_SetOffset, baseData);
            }
            catch
            {

            }           

        }

        private void AutobaseButtonClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Run Autobase?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                form.mainClass.Action(MainClass.Action_ET.AxeBase_Autobase, null);
            }
        }
        
        private void SetHomeButtonClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Set Home?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                form.mainClass.Action(MainClass.Action_ET.AxeBase_SetZero, null);
            }
        }


        private void SetLocalButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.AxeBase_SetLocal, null);
        }

        private void ClearOffsetsButtonClick(object sender, EventArgs e)
        {
            form.mainClass.Action(MainClass.Action_ET.AxeBase_ClearOffsets, null);
        }

        private void MacroButtonClick(object sender, MouseEventArgs e)
        {
            int idx = (int)((Button)sender).Tag;
            if (e.Button == MouseButtons.Left)
            {
                if (Config.configData.macroConfig[idx].needConfirm == true)
                {
                    DialogResult result = MessageBox.Show("Execute "+ Config.configData.macroConfig[idx].name + " ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                    if (result == DialogResult.OK)
                    {
                        form.mainClass.Action(MainClass.Action_ET.AxeBase_RunMacro, Config.configData.macroConfig[idx].path);
                    }
                }
                else
                {
                    form.mainClass.Action(MainClass.Action_ET.AxeBase_RunMacro, Config.configData.macroConfig[idx].path);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                OpenMacroConfig(idx);
            }
        }


        private void OpenMacroConfig(int idx)
        {
            Form_macroConfig form = new Form_macroConfig(idx,this);
            form.ShowDialog();
        }

        internal void UpdateMacroText(int idx)
        {
            macroButtons[idx].Text = Config.configData.macroConfig[idx].name;
        }

        public GUI_axeBasePanel(Form1 form_, Point position)
        {

            form = form_;

            string Name = "AXE BASE";


            GroupBox groupBox = new System.Windows.Forms.GroupBox();
            groupBox.Text = Name;
            groupBox.Location = position;
            groupBox.Size = new Size(300, 320);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            form.Controls.Add(groupBox);

            Button button;

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(40, 50);
            button.Size = new System.Drawing.Size(40, 40);
            button.TabIndex = 11;
            button.UseVisualStyleBackColor = true;
            button.Text = "X";
            button.Click += new System.EventHandler(this.SetOffsetButtonClick);
            button.Tag = 0;
            groupBox.Controls.Add(button);

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(90, 50);
            button.Size = new System.Drawing.Size(40, 40);
            button.TabIndex = 11;
            button.UseVisualStyleBackColor = true;
            button.Text = "Y";
            button.Click += new System.EventHandler(this.SetOffsetButtonClick);
            button.Tag = 1;
            groupBox.Controls.Add(button);

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(140, 50);
            button.Size = new System.Drawing.Size(40, 40);
            button.TabIndex = 11;
            button.UseVisualStyleBackColor = true;
            button.Text = "Z";
            button.Click += new System.EventHandler(this.SetOffsetButtonClick);
            button.Tag = 2;
            groupBox.Controls.Add(button);

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(190, 50);
            button.Size = new System.Drawing.Size(40, 40);
            button.TabIndex = 11;
            button.UseVisualStyleBackColor = true;
            button.Text = "A";
            button.Click += new System.EventHandler(this.SetOffsetButtonClick);
            button.Tag = 3;
            groupBox.Controls.Add(button);

            tbox = new TextBox();
            tbox.Location = new System.Drawing.Point(50, 100);
            tbox.Size = new System.Drawing.Size(100, 30);
            tbox.Text = "0";
            tbox.TabIndex = 11;
            tbox.Tag = 5;
            groupBox.Controls.Add(tbox);

            int x_base = 20;
            int y_base = 140;
            int dx = 150;
            int dy = 32;

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(x_base, y_base);
            button.Size = new System.Drawing.Size(120, 30);
            button.TabIndex = 10;
            button.UseVisualStyleBackColor = true;
            button.Text = "AUTOBASE";
            button.Click += new System.EventHandler(this.AutobaseButtonClick);
            button.Tag = 3;
            groupBox.Controls.Add(button);

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(x_base + dx, y_base );
            button.Size = new System.Drawing.Size(120, 30);
            button.TabIndex = 10;
            button.UseVisualStyleBackColor = true;
            button.Text = "CLEAR OFFSETS";
            button.Click += new System.EventHandler(this.ClearOffsetsButtonClick);
            button.Tag = 3;
            groupBox.Controls.Add(button);

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(x_base , y_base + dy);
            button.Size = new System.Drawing.Size(120, 30);
            button.TabIndex = 10;
            button.UseVisualStyleBackColor = true;
            button.Text = "SET MACHINE 0";
            button.Click += new System.EventHandler(this.SetHomeButtonClick);
            button.Tag = 3;
            groupBox.Controls.Add(button);

            button = new System.Windows.Forms.Button();
            button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            button.Location = new System.Drawing.Point(x_base + dx, y_base + dy);
            button.Size = new System.Drawing.Size(120, 30);
            button.TabIndex = 10;
            button.UseVisualStyleBackColor = true;
            button.Text = "SET LOCAL 0";
            button.Click += new System.EventHandler(this.SetLocalButtonClick);
            button.Tag = 3;
            groupBox.Controls.Add(button);

            for (int idx=0;idx<Constants.NO_OF_MACROS;idx++)
            {
                int x = idx % 2;
                int y = (idx / 2)+2;


                button = new System.Windows.Forms.Button();
                button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                button.Location = new System.Drawing.Point(x_base +  x*dx, y_base + y* dy);
                button.Size = new System.Drawing.Size(120, 30);
                button.TabIndex = 10;
                button.UseVisualStyleBackColor = true;
                button.Text = Config.configData.macroConfig[idx].name;
                button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MacroButtonClick);                
                button.Tag = idx;
                macroButtons[idx] = button;
                groupBox.Controls.Add(button);
            }
            

        }

    }
}
