using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;
using System.Xml;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.LinkLabel;

namespace CNC3
{



    public partial class Form1 : Form
    {
        internal MainClass mainClass;

        GUI_axeRunPanel  guiAxeRunPanel;
        GUI_axePosPanel  guiAxePosPanel;
        GUI_codeRunPanel guiCodeRunPanel;
        GUI_codeEditorPanel guiCodeEditorPanel;
        GUI_axeBasePanel guiAxeBasePanel;
        GUI_GoToPanel guiGoToPanel;

        //public ConfigData configData = new ConfigData();

        string actFileName = "New File";
        bool actFileNameValid = false;

        List<string> recentFiles = new List<string>() ;

        private void InitRecentFilesList()
        {
            if (File.Exists("recentFiles.dat"))
            {
                System.IO.StreamReader file = File.OpenText("recentFiles.dat");

                string fileNameTmp = file.ReadLine();
                while (fileNameTmp != null)
                {
                    recentFiles.Add(fileNameTmp);
                    fileNameTmp = file.ReadLine();
                }
                file.Close();
            }
            
            DrawRecentFilesList();
        }

        private void DrawRecentFilesList()
        {
            recentToolStripMenuItem.DropDownItems.Clear();
            for (int i = 0; i < recentFiles.Count; i++)
            {
                

                System.Windows.Forms.ToolStripMenuItem fileItem;


                fileItem = new System.Windows.Forms.ToolStripMenuItem();
                fileItem.Name = "recentFileToolStripMenuItem_" + i.ToString();
                fileItem.Size = new System.Drawing.Size(270, 34);
                fileItem.Text = recentFiles[i] ;
                fileItem.Tag = i;
                fileItem.Click += new System.EventHandler(OpenRecentFile);

                recentToolStripMenuItem.DropDownItems.Insert(0,fileItem);
            }
        }

        private void OpenRecentFile(object sender, EventArgs e)
        {
            int idx = (int)((System.Windows.Forms.ToolStripMenuItem)sender).Tag;

            OpenFile(recentFiles[idx]);
        }

        private static bool FileNameMatch(String s)
        {
            return s.ToLower().EndsWith("saurus");
        }

        private void AddFileToRecentFiles(string fileName)
        {
            /* scan if file is already on list */


            recentFiles.RemoveAll(x => x == fileName);

            recentFiles.Add(fileName);

            if(recentFiles.Count > 5)
            {
                recentFiles.RemoveAt(0);
            }

             File.WriteAllLines("recentFiles.dat",recentFiles, Encoding.UTF8);


             DrawRecentFilesList();
        }

        public void TestMsgCallback(string msg)
        {

            
        }

        internal void LoadLanguage()
        {
            XmlDocument ling = new XmlDocument();
            bool ok = false;

            if (File.Exists("Ling.xml"))
            {
                try
                {
                    ling.Load("Ling.xml");
                    ok = true;
                }
                catch
                {

                }
            }

            if(ok)
            {
                XmlNode node;

                try
                {
                    node = ling.SelectSingleNode("ROOT/Main/File/Name");
                    fileToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/File/New");
                    newToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/File/Open");
                    openToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/File/Save");
                    saveToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/File/SaveAs");
                    saveAsToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/File/Close");
                    closeToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/File/Recent");
                    recentToolStripMenuItem.Text = node.InnerText;

                    node = ling.SelectSingleNode("ROOT/Main/Connection/Name");
                    connectionToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/Connection/Connect");
                    connectToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/Connection/Disconnect");
                    disconnectToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/Connection/Settings");
                    conSettingsToolStripMenuItem.Text = node.InnerText;

                    node = ling.SelectSingleNode("ROOT/Main/Settings/Name");
                    settingsToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/Settings/Axe");
                    axesConfigToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/Settings/Misc");
                    miscConfigToolStripMenuItem.Text = node.InnerText;

                    node = ling.SelectSingleNode("ROOT/Main/View/Name");
                    viewToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/View/Globals");
                    globalsToolStripMenuItem.Text= node.InnerText;

                    node = ling.SelectSingleNode("ROOT/Main/Tools/Name");
                    toolsToolStripMenuItem.Text = node.InnerText;
                    node = ling.SelectSingleNode("ROOT/Main/Tools/SurfaceProbe");
                    surfaceProbeToolStripMenuItem.Text = node.InnerText;
                }
                catch { ok = false; }
            }
        }



        public void PrintCursorPos(int lineNumber)
        {
            statusStrip1.Items[3].Text = lineNumber.ToString();
        }


        public void ConnectionStatusCallback(bool status, int stateBitMap)
        {
            if(status)
            {
                statusStrip1.Items[0].Text = "Connected: ";
            }
            else
            {
                statusStrip1.Items[0].Text = "Disconnected: ";
            }
            statusStrip1.Items[1].Text = "STAT=" + stateBitMap.ToString("X");

        }

        public void PipelineStatusCallback(string statusString)
        {
            statusStrip1.Items[2].Text = statusString;
        }

        public void PrintPositionCallback(double x, double y, double z, double a)
        {
            guiAxePosPanel.SetPosition(x, y, z, a);
        }

        public void PrintPositionLocalCallback(double x, double y, double z, double a)
        {
            guiAxePosPanel.SetPositionLocal(x, y, z, a);
        }

        public void PrintPositionCallbackReal(double x, double y, double z, double a)
        {
            guiAxePosPanel.SetPositionReal(x, y, z, a);
        }

        public void SetProgresValueCallback(int progress, string text)
        {
            toolStripProgressBar1.Text = text;
            toolStripProgressBar1.Value = progress;
        }

        private void ReadConfig()
        {
            bool cont = false;
            StreamReader sr = null;
            try
            {
                sr = new StreamReader("config.txt");
                cont = true;
            }
            catch
            {
                MessageBox.Show("Nie znaleziono pliku konfguracyjnego, ustawienia domyślne", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (cont)
            {


                bool fileOk = true;
                for (int i = 0; i < 6 && fileOk == true; i++)
                {
                    string line = sr.ReadLine();
                    if (line == null) { fileOk = false; break; }

                }

                if (fileOk == false)
                {
                    MessageBox.Show("Uszkodzony plik konfiguracjny, ustawienia domyślne", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                sr.Close();
            }
        }

        public Form1()
        {



            InitializeComponent();
            

            mainClass = new MainClass(this);

            guiCodeRunPanel = new GUI_codeRunPanel(this, new Point(10, 40));  /* 300x360 */
            guiAxeBasePanel = new GUI_axeBasePanel(this, new Point(10, 400)); /* 300x320 */

            guiCodeEditorPanel = new GUI_codeEditorPanel(this, new Point(320, 40)); /* 610 x 360 */


            guiAxeRunPanel = new GUI_axeRunPanel(this, new Point(940, 40));
            guiAxePosPanel = new GUI_axePosPanel(this, new Point(940, 400));
            
            
            
            guiGoToPanel = new GUI_GoToPanel(this, new Point(320, 400));

            richTextBox1.Location = new Point(10, 720);

            mainClass.TestCallback += new MainClass.CallbackEventHandler(TestMsgCallback);
            mainClass.PrintPositionCallback += new MainClass.PrintPositionCallbackType(PrintPositionCallback);
            mainClass.PrintPositionLocalCallback += new MainClass.PrintPositionCallbackType(PrintPositionLocalCallback);
            mainClass.PrintPositionCallbackReal += new MainClass.PrintPositionCallbackType(PrintPositionCallbackReal);
            mainClass.ConnectionStatusCallback += new MainClass.ConnectionStatusCallbackType(ConnectionStatusCallback);
            mainClass.PipelineStatusCallback += new MainClass.PipelineStatusCallbackType(PipelineStatusCallback);

            MainClass.CallbackSetProgress += new MainClass.CallbackSetProgressType(SetProgresValueCallback);
            MainClass.GetNoOfLinesCallback += new MainClass.CallbackGetNoOfLines(GetNoOfLines);
            MainClass.GetLineCallback += new MainClass.CallbackGetLine(GetLine);
            MainClass.GetLinesArrayCallback += new MainClass.GetLinesArrayCallbackType(GetLinesArray);
            MainClass.ErrorCallback += new MainClass.CallbackErrorCallback(ErrorHandler);
            gCodeCompMath.ErrorCallback += new gCodeCompMath.CallbackErrorCallback(ErrorHandler);
            cGodeCompiller.ErrorCallback += new cGodeCompiller.CallbackErrorCallback(ErrorHandler);
            MoveController.ErrorCallback += new MoveController.CallbackErrorCallback(ErrorHandler);
            Executor.ErrorCallback += new Executor.CallbackErrorCallback(ErrorHandler);

            mainClass.Init();

            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add(new MenuItem("Clear", new EventHandler(textBox_eventClear)));
            richTextBox1.ContextMenu = cm;

            InitRecentFilesList();

            LoadLanguage();
           

        }

        public string[] GetLinesArray()
        {
            return guiCodeEditorPanel.GetLinesArray();
        }


        public int GetNoOfLines()
        {            
            return guiCodeEditorPanel.GetNoOfLines();
        }

        public string GetLine(int idx)
        {
            return guiCodeEditorPanel.GetLine(idx);            
        }

        public void ErrorHandler(string errMsg)
        {
            richTextBox1.AppendText(errMsg);
            if(richTextBox1.Lines.Length > 1000)
            {
                int start_index = richTextBox1.GetFirstCharIndexFromLine(0);
                int count = richTextBox1.Lines[0].Length;
                richTextBox1.Text = richTextBox1.Text.Remove(start_index, count + 1);
            }
        }

        public void FileDragged(string fileName)
        {
            OpenFile(fileName);
        }

        public void TextChangedEvent(bool status)
        {
            if(status)
            {
                this.Text = "MyCNC3 - " + actFileName + "*";
            }
            else
            {
                this.Text = "MyCNC3 - " + actFileName;
            }
        }


        private void OpenFile(string  fileName)
        {
            guiCodeEditorPanel.OpenFile(fileName);

            actFileName = fileName;
            actFileNameValid = true;
            saveToolStripMenuItem.Enabled = true;
            this.Text = "MyCNC3 - " + actFileName;

            AddFileToRecentFiles(fileName);
        }

        public bool SaveFileReq()
        { 
            if(actFileNameValid == true)
            {
                guiCodeEditorPanel.SaveFile(actFileName);
                return true;
            }
            else
            {
                sfd.ShowDialog();
                if (actFileNameValid == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            
        }



        private void ustawieniaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2(this);
            newForm.Size = new System.Drawing.Size(500, 300);
            newForm.ShowDialog();
        }

        private void połączToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainClass.Action(MainClass.Action_ET.Eth_Connect, null);
        }

        private void rozłączToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainClass.Action(MainClass.Action_ET.Eth_Disconnect, null);
        }

        private void otwórzToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ofd.ShowDialog();


        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (ofd.FileName != null)
            {
                OpenFile(ofd.FileName);
            }
        }



        private void axesConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_axeConfig newForm = new Form_axeConfig(this);
            newForm.Size = new System.Drawing.Size(300, 300);
            newForm.ShowDialog();
        }

        private void miscConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_miscConfig newForm = new Form_miscConfig(this);
            newForm.Size = new System.Drawing.Size(700, 400);
            newForm.ShowDialog();
        }

        private void zapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (actFileNameValid)
            {
                guiCodeEditorPanel.SaveFile(actFileName);
                this.Text = "MyCNC3 - " + actFileName;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sfd.ShowDialog();
        }

        private void sfd_FileOk(object sender, CancelEventArgs e)
        {
            if (sfd.FileName != null)
            {
                guiCodeEditorPanel.SaveFile(sfd.FileName);

                actFileName = sfd.FileName;
                actFileNameValid = true;
                saveToolStripMenuItem.Enabled = true;
                this.Text = "MyCNC3 - " + actFileName;
                AddFileToRecentFiles(actFileName);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = guiCodeEditorPanel.Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool cont = guiCodeEditorPanel.NewFile();

            if(cont)
            {
                actFileName = "";
                actFileNameValid = false;
                this.Text = "MyCNC3";
            }
        }

        private void textBox_eventClear(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }


        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            /*
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                cm.MenuItems.Add(new MenuItem("Clear", new EventHandler(textBox_eventClear)));
            }*/
        }

        private void globalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            globalsForm newForm = new globalsForm();            
            newForm.Show();

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void surfaceProbeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_surfaceProbe newForm = new Form_surfaceProbe(this);
            newForm.Size = new System.Drawing.Size(500, 600);
            newForm.Show();
        }

    }
}
