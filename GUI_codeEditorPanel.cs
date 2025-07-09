using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CNC3
{
    internal class GUI_codeEditorPanel
    {
        Form1 form;
        RichTextBox textBox;

        bool textChanged = false;



        public GUI_codeEditorPanel(Form1 form_, Point position)
        {
            form = form_;



            string Name = "CODE";


            GroupBox groupBox = new System.Windows.Forms.GroupBox();
            groupBox.Text = Name;
            groupBox.Location = position;
            groupBox.Size = new Size(610, 360);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

            form.Controls.Add(groupBox);

            textBox = new RichTextBox();

            textBox.Location = new System.Drawing.Point(10, 30);
            textBox.Size = new Size(590, 320);
            textBox.Text = "";
            textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            textBox.TabIndex = 11;
            textBox.AllowDrop = true;
            textBox.TextChanged += new EventHandler(this.TextChanged);
            textBox.SelectionChanged += new EventHandler(this.SelChanged);
            


            textBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.DragFile);
            textBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnter);
            groupBox.Controls.Add(textBox);
        }

        private void TextChanged(object sender, EventArgs e)
        {
            textChanged = true;
            form.TextChangedEvent(true);
        }

        private void SelChanged(object sender, EventArgs e)
        {
 
            int line = textBox.GetLineFromCharIndex(textBox.SelectionStart);
            form.PrintCursorPos(line+1);
        }

        private void DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;

            else
                e.Effect = DragDropEffects.None;
        }

        private void DragFile(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                bool cont = true;
                if (textChanged)
                {
                    DialogResult result = MessageBox.Show("Save changes?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                    switch (result)
                    {
                        case DialogResult.Yes:

                            cont = form.SaveFileReq();
                            break;

                        case DialogResult.No:
                            cont = true;
                            break;

                        default:
                            cont = false;
                            break;
                    }
                }


                if (cont)
                {
                    form.FileDragged(files[0]);

                    textChanged = false;
                }

            }


        }

        public int GetNoOfLines()
        {
            return textBox.Lines.Length;
        }

        public string GetLine(int idx)
        {
            if (idx >= 0 && idx < textBox.Lines.Length)
            {
                return textBox.Lines[idx];
            }
            else
            {
                return "";
            }
        }

        public void OpenFile(string fileName)
        {
            bool cont = true;
            if (textChanged)
            {
                DialogResult result = MessageBox.Show("Save changes?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                switch (result)
                {
                    case DialogResult.Yes:

                        cont = form.SaveFileReq();
                        break;

                    case DialogResult.No:
                        cont = true;
                        break;

                    default:
                        cont = false;
                        break;
                }
            }


            if (cont)
            {
                textBox.LoadFile(fileName, RichTextBoxStreamType.PlainText);
                textChanged = false;
            }


        }

        public void SaveFile(string fileName)
        {
            textBox.SaveFile(fileName, RichTextBoxStreamType.PlainText);
            textChanged = false;
        }

        public bool NewFile()
        {
            bool cont = true;
            if (textChanged)
            {
                DialogResult result = MessageBox.Show("Save changes?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                switch (result)
                {
                    case DialogResult.Yes:

                        cont = form.SaveFileReq();
                        break;

                    case DialogResult.No:
                        cont = true;
                        break;

                    default:
                        cont = false;
                        break;
                }
            }


            if (cont)
            {                
                textChanged = false;
                textBox.Clear();
            }
            return cont;
        }

        public bool Close()
        {
            bool cont = true;
            if (textChanged)
            {
                DialogResult result = MessageBox.Show("Save changes?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                switch (result)
                {
                    case DialogResult.Yes:

                        cont = form.SaveFileReq();
                        break;

                    case DialogResult.No:
                        cont = true;
                        break;

                    default:
                        cont = false;
                        break;
                }
            }


            return !cont;
        }
    }
}
