using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNC3
{
    public partial class globalsForm : Form
    {
        public globalsForm()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        char[] axesC = { '?','X', 'Y', 'Z', 'A' };

        private void globalsForm_Load(object sender, EventArgs e)
        {
            for (int i = 5000; i < gCodeCompMath.globalArray.Length; i++)
            {
                string name = "";

                int j = i % 10;

                bool print = false;


                if (i >= 5161 && i <= 5164) { name = "G28_" + axesC[j]; print = true; }
                if (i >= 5181 && i <= 5184) { name = "G30_" + axesC[j]; print = true; }
                if (i >= 5221 && i <= 5224) { name = "G54_" + axesC[j]; print = true; }
                if (i >= 5241 && i <= 5244) { name = "G55_" + axesC[j]; print = true; }
                if (i >= 5261 && i <= 5264) { name = "G56_" + axesC[j]; print = true; }
                if (i >= 5281 && i <= 5284) { name = "G57_" + axesC[j]; print = true; }
                if (i >= 5301 && i <= 5304) { name = "G58_" + axesC[j]; print = true; }
                if (i >= 5321 && i <= 5324) { name = "G59_" + axesC[j]; print = true; }
                if (i >= 5341 && i <= 5344) { name = "G59.1_" + axesC[j]; print = true; }
                if (i >= 5361 && i <= 5364) { name = "G59.2_" + axesC[j]; print = true; }
                if (i >= 5381 && i <= 5384) { name = "G59.3_" + axesC[j]; print = true; }
                if (i == 5220) { name = "Coordinate System number"; print = true; }

                if (i >= 5211 && i <= 5214) { name = "G92_offset_" + axesC[j]; print = true; }
                if (i == 5210) { name = "G92 offset status"; print = true; }
                if (i == 5430) { name = "Tool length offset"; print = true; }
                if (i == 5431) { name = "Tool length sensor offset"; print = true; }

                if (i >= 5061 && i <= 5064) { name = "Probe pos_" + axesC[j]; print = true; }
                if (i == 5070) { name = "Probe result"; print = true; }


                if (print)
                { 
                    dataGridView1.Rows.Add(i, gCodeCompMath.globalArray[i], name);
                }



            }
        }
    }
}
