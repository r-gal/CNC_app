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
    public partial class Form2 : Form
    {
        Form1 mainForm;
        public Form2(Form1 form_)
        {
            InitializeComponent();
            mainForm = form_;

            textBox1.Text = Config.configData.ip.ToString();
            textBox2.Text = Config.configData.port.ToString();
            checkBox1.Checked = Config.configData.autoConnect;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();

            try
            {
                configData.ip = IPAddress.Parse(textBox1.Text);
            }
            catch
            {     
                MessageBox.Show("Niewłaściwy adres", "Error", MessageBoxButtons.OK  , MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.port = int.Parse(textBox2.Text);
            }
            catch
            {
                MessageBox.Show("Niewłaściwy port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(configData.port < 1000 || configData.port > 65535)
            {
                MessageBox.Show("Niewłaściwy port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            configData.autoConnect = checkBox1.Checked;

            Config.SaveConnectionConfig(configData);
        }
    }
}
