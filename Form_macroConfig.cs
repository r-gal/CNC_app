using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNC3
{
    internal partial class Form_macroConfig : Form
    {
        int idx;
        GUI_axeBasePanel parent;
        public Form_macroConfig(int idx_, GUI_axeBasePanel parent_)
        {
            InitializeComponent();

            idx = idx_;
            parent = parent_;

            nameTextBox.Text = Config.configData.macroConfig[idx].name;
            pathTextBox.Text = Config.configData.macroConfig[idx].path;
            needConfirm_checkBox1.Checked = Config.configData.macroConfig[idx].needConfirm;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            MacroData macroData = new MacroData();  
            macroData.name = nameTextBox.Text;
            macroData.path = pathTextBox.Text;  
            macroData.needConfirm = needConfirm_checkBox1.Checked;

            Config.SaveMacroConfig(macroData, idx);
            parent.UpdateMacroText( idx);
            this.Close();
        }

        private void openFileDialogOk(object sender, CancelEventArgs e)
        {
            string path = ((OpenFileDialog)sender).FileName;
            if (path != null)
            {
                pathTextBox.Text = path;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(openFileDialogOk);
            openFileDialog.ShowDialog(this);


        }
    }
}
