using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNC3
{
    public partial class Form_axeConfig : Form
    {
        Form1 mainForm;
        int axeIdx = 0;
        public Form_axeConfig(Form1 form_)
        {
            mainForm = form_;
            InitializeComponent();

            LoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();

            configData.ena[axeIdx] = checkBox1.Checked;
            configData.dir[axeIdx] = checkBox2.Checked;

            

            try
            {
                configData.scale[axeIdx] = Double.Parse(textBox1.Text);
            }
            catch
            {
                MessageBox.Show("Scale error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.maxSpeed[axeIdx] = Double.Parse(textBox2.Text);
            }
            catch
            {
                MessageBox.Show("Max speed error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.maxAcceleration[axeIdx] = Double.Parse(textBox3.Text);
            }
            catch
            {
                MessageBox.Show("Max acceleration error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.length[axeIdx] = int.Parse(textBox4.Text);
            }
            catch
            {
                MessageBox.Show("Length error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.zeroOffset[axeIdx] = (int)(double.Parse(zeroOffset_textBox.Text) * 1000);
            }
            catch
            {
                MessageBox.Show("Zero offset error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

            configData.limMode[axeIdx] = (ConfigData.LIM_MODE_et)comboBox2.SelectedIndex;
            configData.limType[axeIdx] = (ConfigData.LIM_TYPE_et)comboBox4.SelectedIndex;

            configData.baseType[axeIdx] = (ConfigData.BASE_TYPE_et)comboBox3.SelectedIndex;

            Config.SaveAxeConfig(configData, axeIdx) ;
        }

        private void LoadData()
        {
            checkBox1.Checked = Config.configData.ena[axeIdx];
            checkBox2.Checked = Config.configData.dir[axeIdx];
            textBox1.Text = Config.configData.scale[axeIdx].ToString();
            textBox2.Text = Config.configData.maxSpeed[axeIdx].ToString();
            textBox3.Text = Config.configData.maxAcceleration[axeIdx].ToString();
            textBox4.Text = Config.configData.length[axeIdx].ToString();
            comboBox2.SelectedIndex = (int)Config.configData.limMode[axeIdx];
            comboBox3.SelectedIndex = (int)Config.configData.baseType[axeIdx];
            comboBox4.SelectedIndex = (int)Config.configData.limType[axeIdx];
            zeroOffset_textBox.Text = (0.001 * Config.configData.zeroOffset[axeIdx]).ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            axeIdx = comboBox1.SelectedIndex;
            LoadData();
        }

    }
}
