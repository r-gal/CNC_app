using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CNC3
{
    public partial class Form_miscConfig : Form
    {
        public Form_miscConfig()
        {
            InitializeComponent();
        }

        Form1 mainForm;
        public Form_miscConfig(Form1 form_)
        {
            mainForm = form_;
            InitializeComponent();

            LoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();

            

            configData.eStopMode = (ConfigData.INPUT_MODE_et)comboBox_estop.SelectedIndex;
            configData.probeMode = (ConfigData.INPUT_MODE_et)comboBox_probe.SelectedIndex;

            try
            {
                configData.minSpeed = Double.Parse(textBox_minSpeed.Text);
            }
            catch
            {
                MessageBox.Show("Min speed error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.autoBaseSpeed = Double.Parse(textBox_autobase_speed.Text);
            }
            catch
            {
                MessageBox.Show("AutoBase speed error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.maxSpindleSpeed = int.Parse(textBox_spindleMaxSpeed.Text);
            }
            catch
            {
                MessageBox.Show("Spindle Max speed error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (configData.minSpeed < 0.1) { configData.minSpeed = 0.1; }

            try
            {
                configData.baseX = Double.Parse(textBox_baseX.Text);
            }
            catch
            {
                MessageBox.Show("Base X value error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.baseY = Double.Parse(textBox_baseY.Text);
            }
            catch
            {
                MessageBox.Show("Base Y value error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.baseZ = Double.Parse(textBox_baseZ.Text);
            }
            catch
            {
                MessageBox.Show("Base Z value error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.probeSpeed1 = Double.Parse(textBox_probeSpeed1.Text);
            }
            catch
            {
                MessageBox.Show("Probe speed 1 error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.probeSpeed2 = Double.Parse(textBox_probeSpeed2.Text);
            }
            catch
            {
                MessageBox.Show("Probe speed 2 error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                configData.probeLength = Double.Parse(textBox_probeLength.Text);
            }
            catch
            {
                MessageBox.Show("Probe length value error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            configData.autoTLO = autoTLO_checkbox.Checked;
            configData.zeroMachineAutobase = zeroHomeAfterBase_checkbox.Checked;

            Config.SaveMiscConfig(configData);
        }

        private void LoadData()
        {

            comboBox_estop.SelectedIndex = (int)Config.configData.eStopMode;
            comboBox_probe.SelectedIndex = (int)Config.configData.probeMode;
            textBox_minSpeed.Text = Config.configData.minSpeed.ToString();
            textBox_autobase_speed.Text = Config.configData.autoBaseSpeed.ToString();
            textBox_spindleMaxSpeed.Text = Config.configData.maxSpindleSpeed.ToString();
            textBox_baseX.Text = Config.configData.baseX.ToString();
            textBox_baseY.Text = Config.configData.baseY.ToString();
            textBox_baseZ.Text = Config.configData.baseZ.ToString();
            textBox_probeSpeed1.Text = Config.configData.probeSpeed1.ToString();
            textBox_probeSpeed2.Text = Config.configData.probeSpeed2.ToString();
            textBox_probeLength.Text = Config.configData.probeLength.ToString();
            autoTLO_checkbox.Checked = Config.configData.autoTLO;
            zeroHomeAfterBase_checkbox.Checked = Config.configData.zeroMachineAutobase;
        }


    }
}
