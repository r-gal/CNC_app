namespace CNC3
{
    partial class Form_miscConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_estop = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox_probe = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_minSpeed = new System.Windows.Forms.TextBox();
            this.textBox_autobase_speed = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_spindleMaxSpeed = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox_probeLength = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_baseZ = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_probeSpeed2 = new System.Windows.Forms.TextBox();
            this.textBox_probeSpeed1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_baseY = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_baseX = new System.Windows.Forms.TextBox();
            this.autoTLO_checkbox = new System.Windows.Forms.CheckBox();
            this.zeroHomeAfterBase_checkbox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(41, 40);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(179, 20);
            this.label7.TabIndex = 23;
            this.label7.Text = "Emergency STOP mode";
            // 
            // comboBox_estop
            // 
            this.comboBox_estop.FormattingEnabled = true;
            this.comboBox_estop.Items.AddRange(new object[] {
            "NO",
            "NC"});
            this.comboBox_estop.Location = new System.Drawing.Point(223, 37);
            this.comboBox_estop.Name = "comboBox_estop";
            this.comboBox_estop.Size = new System.Drawing.Size(121, 28);
            this.comboBox_estop.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 74);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 20);
            this.label6.TabIndex = 21;
            this.label6.Text = "Probe mode";
            // 
            // comboBox_probe
            // 
            this.comboBox_probe.FormattingEnabled = true;
            this.comboBox_probe.Items.AddRange(new object[] {
            "NO",
            "NC"});
            this.comboBox_probe.Location = new System.Drawing.Point(223, 71);
            this.comboBox_probe.Name = "comboBox_probe";
            this.comboBox_probe.Size = new System.Drawing.Size(121, 28);
            this.comboBox_probe.TabIndex = 20;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(45, 282);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 50);
            this.button1.TabIndex = 19;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 20);
            this.label1.TabIndex = 24;
            this.label1.Text = "Min speed";
            // 
            // textBox_minSpeed
            // 
            this.textBox_minSpeed.Location = new System.Drawing.Point(223, 105);
            this.textBox_minSpeed.Name = "textBox_minSpeed";
            this.textBox_minSpeed.Size = new System.Drawing.Size(121, 26);
            this.textBox_minSpeed.TabIndex = 25;
            // 
            // textBox_autobase_speed
            // 
            this.textBox_autobase_speed.Location = new System.Drawing.Point(223, 137);
            this.textBox_autobase_speed.Name = "textBox_autobase_speed";
            this.textBox_autobase_speed.Size = new System.Drawing.Size(121, 26);
            this.textBox_autobase_speed.TabIndex = 27;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 20);
            this.label2.TabIndex = 26;
            this.label2.Text = "AutoBase speed";
            // 
            // textBox_spindleMaxSpeed
            // 
            this.textBox_spindleMaxSpeed.Location = new System.Drawing.Point(223, 169);
            this.textBox_spindleMaxSpeed.Name = "textBox_spindleMaxSpeed";
            this.textBox_spindleMaxSpeed.Size = new System.Drawing.Size(121, 26);
            this.textBox_spindleMaxSpeed.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(41, 172);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 20);
            this.label3.TabIndex = 28;
            this.label3.Text = "Spindle max speed";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.textBox_probeLength);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.textBox_baseZ);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.textBox_probeSpeed2);
            this.groupBox1.Controls.Add(this.textBox_probeSpeed1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBox_baseY);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_baseX);
            this.groupBox1.Location = new System.Drawing.Point(367, 37);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(289, 243);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tool change settings";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(26, 196);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(99, 20);
            this.label11.TabIndex = 41;
            this.label11.Text = "Probe length";
            // 
            // textBox_probeLength
            // 
            this.textBox_probeLength.Location = new System.Drawing.Point(144, 193);
            this.textBox_probeLength.Name = "textBox_probeLength";
            this.textBox_probeLength.Size = new System.Drawing.Size(121, 26);
            this.textBox_probeLength.TabIndex = 40;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 100);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(61, 20);
            this.label10.TabIndex = 39;
            this.label10.Text = "Base Y";
            // 
            // textBox_baseZ
            // 
            this.textBox_baseZ.Location = new System.Drawing.Point(144, 97);
            this.textBox_baseZ.Name = "textBox_baseZ";
            this.textBox_baseZ.Size = new System.Drawing.Size(121, 26);
            this.textBox_baseZ.TabIndex = 38;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(26, 164);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(112, 20);
            this.label9.TabIndex = 37;
            this.label9.Text = "Probe speed 2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 132);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(112, 20);
            this.label8.TabIndex = 36;
            this.label8.Text = "Probe speed 1";
            // 
            // textBox_probeSpeed2
            // 
            this.textBox_probeSpeed2.Location = new System.Drawing.Point(144, 161);
            this.textBox_probeSpeed2.Name = "textBox_probeSpeed2";
            this.textBox_probeSpeed2.Size = new System.Drawing.Size(121, 26);
            this.textBox_probeSpeed2.TabIndex = 35;
            // 
            // textBox_probeSpeed1
            // 
            this.textBox_probeSpeed1.Location = new System.Drawing.Point(144, 129);
            this.textBox_probeSpeed1.Name = "textBox_probeSpeed1";
            this.textBox_probeSpeed1.Size = new System.Drawing.Size(121, 26);
            this.textBox_probeSpeed1.TabIndex = 34;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 20);
            this.label5.TabIndex = 33;
            this.label5.Text = "Base Y";
            // 
            // textBox_baseY
            // 
            this.textBox_baseY.Location = new System.Drawing.Point(144, 65);
            this.textBox_baseY.Name = "textBox_baseY";
            this.textBox_baseY.Size = new System.Drawing.Size(121, 26);
            this.textBox_baseY.TabIndex = 32;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 20);
            this.label4.TabIndex = 31;
            this.label4.Text = "Base X";
            // 
            // textBox_baseX
            // 
            this.textBox_baseX.Location = new System.Drawing.Point(144, 33);
            this.textBox_baseX.Name = "textBox_baseX";
            this.textBox_baseX.Size = new System.Drawing.Size(121, 26);
            this.textBox_baseX.TabIndex = 31;
            // 
            // autoTLO_checkbox
            // 
            this.autoTLO_checkbox.AutoSize = true;
            this.autoTLO_checkbox.Location = new System.Drawing.Point(45, 201);
            this.autoTLO_checkbox.Name = "autoTLO_checkbox";
            this.autoTLO_checkbox.Size = new System.Drawing.Size(223, 24);
            this.autoTLO_checkbox.TabIndex = 31;
            this.autoTLO_checkbox.Text = "Run TLO after tool change";
            this.autoTLO_checkbox.UseVisualStyleBackColor = true;
            // 
            // zeroHomeAfterBase_checkbox
            // 
            this.zeroHomeAfterBase_checkbox.AutoSize = true;
            this.zeroHomeAfterBase_checkbox.Location = new System.Drawing.Point(45, 233);
            this.zeroHomeAfterBase_checkbox.Name = "zeroHomeAfterBase_checkbox";
            this.zeroHomeAfterBase_checkbox.Size = new System.Drawing.Size(294, 24);
            this.zeroHomeAfterBase_checkbox.TabIndex = 32;
            this.zeroHomeAfterBase_checkbox.Text = "Reset machine coord after autobase";
            this.zeroHomeAfterBase_checkbox.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.zeroHomeAfterBase_checkbox.UseVisualStyleBackColor = true;
            // 
            // Form_miscConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 344);
            this.Controls.Add(this.zeroHomeAfterBase_checkbox);
            this.Controls.Add(this.autoTLO_checkbox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox_spindleMaxSpeed);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_autobase_speed);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_minSpeed);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.comboBox_estop);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBox_probe);
            this.Controls.Add(this.button1);
            this.Name = "Form_miscConfig";
            this.Text = "Form_miscConfig";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_estop;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_probe;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_minSpeed;
        private System.Windows.Forms.TextBox textBox_autobase_speed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_spindleMaxSpeed;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_baseZ;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_probeSpeed2;
        private System.Windows.Forms.TextBox textBox_probeSpeed1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_baseY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_baseX;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox_probeLength;
        private System.Windows.Forms.CheckBox autoTLO_checkbox;
        private System.Windows.Forms.CheckBox zeroHomeAfterBase_checkbox;
    }
}