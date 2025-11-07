namespace CNC3
{
    partial class globalsForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.parIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.parIndex,
            this.parValue,
            this.parName});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.Size = new System.Drawing.Size(929, 687);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // parIndex
            // 
            this.parIndex.HeaderText = "Param number";
            this.parIndex.MinimumWidth = 8;
            this.parIndex.Name = "parIndex";
            this.parIndex.ReadOnly = true;
            this.parIndex.Width = 150;
            // 
            // parValue
            // 
            this.parValue.HeaderText = "Param Value";
            this.parValue.MinimumWidth = 8;
            this.parValue.Name = "parValue";
            this.parValue.Width = 150;
            // 
            // parName
            // 
            this.parName.HeaderText = "Param name";
            this.parName.MinimumWidth = 8;
            this.parName.Name = "parName";
            this.parName.ReadOnly = true;
            this.parName.Width = 250;
            // 
            // globalsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 701);
            this.Controls.Add(this.dataGridView1);
            this.Name = "globalsForm";
            this.Text = "w";
            this.Load += new System.EventHandler(this.globalsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn parIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn parValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn parName;
    }
}