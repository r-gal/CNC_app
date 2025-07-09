namespace CNC3
{
    partial class Form_surfaceProbe
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
            this.run_Button = new System.Windows.Forms.Button();
            this.info_Label = new System.Windows.Forms.Label();
            this.stepX_textBox = new System.Windows.Forms.TextBox();
            this.step_Label = new System.Windows.Forms.Label();
            this.stepY_textBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.sizeY_textBox = new System.Windows.Forms.TextBox();
            this.size_Label = new System.Windows.Forms.Label();
            this.sizeX_textBox = new System.Windows.Forms.TextBox();
            this.cancel_Button = new System.Windows.Forms.Button();
            this.clear_Button = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.refresh_Button = new System.Windows.Forms.Button();
            this.startPosY_textBox = new System.Windows.Forms.TextBox();
            this.StartPos_Label = new System.Windows.Forms.Label();
            this.startPosX_textBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // run_Button
            // 
            this.run_Button.Location = new System.Drawing.Point(69, 266);
            this.run_Button.Name = "run_Button";
            this.run_Button.Size = new System.Drawing.Size(120, 40);
            this.run_Button.TabIndex = 0;
            this.run_Button.Text = "Run";
            this.run_Button.UseVisualStyleBackColor = true;
            this.run_Button.Click += new System.EventHandler(this.run_Button_Click);
            // 
            // info_Label
            // 
            this.info_Label.AutoSize = true;
            this.info_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.info_Label.Location = new System.Drawing.Point(38, 31);
            this.info_Label.Name = "info_Label";
            this.info_Label.Size = new System.Drawing.Size(326, 22);
            this.info_Label.TabIndex = 1;
            this.info_Label.Text = "Probe will start from local zero position. ";
            // 
            // stepX_textBox
            // 
            this.stepX_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.stepX_textBox.Location = new System.Drawing.Point(239, 117);
            this.stepX_textBox.Name = "stepX_textBox";
            this.stepX_textBox.Size = new System.Drawing.Size(160, 30);
            this.stepX_textBox.TabIndex = 2;
            this.stepX_textBox.Text = "0";
            // 
            // step_Label
            // 
            this.step_Label.AutoSize = true;
            this.step_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.step_Label.Location = new System.Drawing.Point(37, 117);
            this.step_Label.Name = "step_Label";
            this.step_Label.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.step_Label.Size = new System.Drawing.Size(123, 29);
            this.step_Label.TabIndex = 3;
            this.step_Label.Text = "Step [mm]";
            // 
            // stepY_textBox
            // 
            this.stepY_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.stepY_textBox.Location = new System.Drawing.Point(405, 117);
            this.stepY_textBox.Name = "stepY_textBox";
            this.stepY_textBox.Size = new System.Drawing.Size(160, 30);
            this.stepY_textBox.TabIndex = 4;
            this.stepY_textBox.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(305, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 29);
            this.label3.TabIndex = 5;
            this.label3.Text = "X";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(463, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 29);
            this.label4.TabIndex = 6;
            this.label4.Text = "Y";
            // 
            // sizeY_textBox
            // 
            this.sizeY_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.sizeY_textBox.Location = new System.Drawing.Point(405, 153);
            this.sizeY_textBox.Name = "sizeY_textBox";
            this.sizeY_textBox.Size = new System.Drawing.Size(160, 30);
            this.sizeY_textBox.TabIndex = 9;
            this.sizeY_textBox.Text = "0";
            // 
            // size_Label
            // 
            this.size_Label.AutoSize = true;
            this.size_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.size_Label.Location = new System.Drawing.Point(37, 153);
            this.size_Label.Name = "size_Label";
            this.size_Label.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.size_Label.Size = new System.Drawing.Size(120, 29);
            this.size_Label.TabIndex = 8;
            this.size_Label.Text = "Size [mm]";
            // 
            // sizeX_textBox
            // 
            this.sizeX_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.sizeX_textBox.Location = new System.Drawing.Point(239, 153);
            this.sizeX_textBox.Name = "sizeX_textBox";
            this.sizeX_textBox.Size = new System.Drawing.Size(160, 30);
            this.sizeX_textBox.TabIndex = 7;
            this.sizeX_textBox.Text = "0";
            // 
            // cancel_Button
            // 
            this.cancel_Button.Location = new System.Drawing.Point(195, 266);
            this.cancel_Button.Name = "cancel_Button";
            this.cancel_Button.Size = new System.Drawing.Size(120, 40);
            this.cancel_Button.TabIndex = 10;
            this.cancel_Button.Text = "Cancel";
            this.cancel_Button.UseVisualStyleBackColor = true;
            this.cancel_Button.Click += new System.EventHandler(this.cancel_Button_Click);
            // 
            // clear_Button
            // 
            this.clear_Button.Location = new System.Drawing.Point(321, 266);
            this.clear_Button.Name = "clear_Button";
            this.clear_Button.Size = new System.Drawing.Size(120, 40);
            this.clear_Button.TabIndex = 11;
            this.clear_Button.Text = "Clear";
            this.clear_Button.UseVisualStyleBackColor = true;
            this.clear_Button.Click += new System.EventHandler(this.clear_Button_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(42, 322);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(546, 282);
            this.dataGridView1.TabIndex = 12;
            // 
            // refresh_Button
            // 
            this.refresh_Button.Location = new System.Drawing.Point(445, 266);
            this.refresh_Button.Name = "refresh_Button";
            this.refresh_Button.Size = new System.Drawing.Size(120, 40);
            this.refresh_Button.TabIndex = 13;
            this.refresh_Button.Text = "Refresh";
            this.refresh_Button.UseVisualStyleBackColor = true;
            this.refresh_Button.Click += new System.EventHandler(this.refresh_Button_Click);
            // 
            // startPosY_textBox
            // 
            this.startPosY_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.startPosY_textBox.Location = new System.Drawing.Point(405, 189);
            this.startPosY_textBox.Name = "startPosY_textBox";
            this.startPosY_textBox.ReadOnly = true;
            this.startPosY_textBox.Size = new System.Drawing.Size(160, 30);
            this.startPosY_textBox.TabIndex = 16;
            this.startPosY_textBox.Text = "0";
            // 
            // StartPos_Label
            // 
            this.StartPos_Label.AutoSize = true;
            this.StartPos_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.StartPos_Label.Location = new System.Drawing.Point(37, 189);
            this.StartPos_Label.Name = "StartPos_Label";
            this.StartPos_Label.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPos_Label.Size = new System.Drawing.Size(168, 29);
            this.StartPos_Label.TabIndex = 15;
            this.StartPos_Label.Text = "Start pos [mm]";
            // 
            // startPosX_textBox
            // 
            this.startPosX_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.startPosX_textBox.Location = new System.Drawing.Point(239, 189);
            this.startPosX_textBox.Name = "startPosX_textBox";
            this.startPosX_textBox.ReadOnly = true;
            this.startPosX_textBox.Size = new System.Drawing.Size(160, 30);
            this.startPosX_textBox.TabIndex = 14;
            this.startPosX_textBox.Text = "0";
            // 
            // Form_surfaceProbe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 640);
            this.Controls.Add(this.startPosY_textBox);
            this.Controls.Add(this.StartPos_Label);
            this.Controls.Add(this.startPosX_textBox);
            this.Controls.Add(this.refresh_Button);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.clear_Button);
            this.Controls.Add(this.cancel_Button);
            this.Controls.Add(this.sizeY_textBox);
            this.Controls.Add(this.size_Label);
            this.Controls.Add(this.sizeX_textBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.stepY_textBox);
            this.Controls.Add(this.step_Label);
            this.Controls.Add(this.stepX_textBox);
            this.Controls.Add(this.info_Label);
            this.Controls.Add(this.run_Button);
            this.Name = "Form_surfaceProbe";
            this.Text = "Surface Probe";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button run_Button;
        private System.Windows.Forms.Label info_Label;
        private System.Windows.Forms.TextBox stepX_textBox;
        private System.Windows.Forms.Label step_Label;
        private System.Windows.Forms.TextBox stepY_textBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox sizeY_textBox;
        private System.Windows.Forms.Label size_Label;
        private System.Windows.Forms.TextBox sizeX_textBox;
        private System.Windows.Forms.Button cancel_Button;
        private System.Windows.Forms.Button clear_Button;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button refresh_Button;
        private System.Windows.Forms.TextBox startPosY_textBox;
        private System.Windows.Forms.Label StartPos_Label;
        private System.Windows.Forms.TextBox startPosX_textBox;
    }
}