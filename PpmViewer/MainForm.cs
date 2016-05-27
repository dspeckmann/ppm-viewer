using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace PpmViewer
{
    public partial class MainForm : Form
    {
        // TODO: Add licence file
        // TODO: Change folder structure (subfolder for project)?
        // TODO: Add support for other file types (first check for 'P', then switch case for the next char and abort if number doesn't match)

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenImage(openFileDialog.FileName);
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void OpenImage(string path)
        {
            Cursor = Cursors.WaitCursor;
            toolStripProgressBar.Value = 0;

            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open), new ASCIIEncoding()))
            {
                // Check magic number
                if (reader.ReadChar() != 'P') MessageBox.Show("No P!");
                if (reader.ReadChar() != '6') MessageBox.Show("No 6!");
                
                // Read width, height and maximum color value while ignoring whitespace and comments
                ReadWhitespace(reader);
                int width = ReadValue(reader);
                ReadWhitespace(reader);
                int height = ReadValue(reader);
                ReadWhitespace(reader);
                int maximumColorValue = ReadValue(reader);
                ReadWhitespace(reader);

                // Read the actual pixels
                Bitmap bitmap = new Bitmap(width, height);
                int i = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    byte[] rgb = reader.ReadBytes(3);
                    Color color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                    bitmap.SetPixel(i % width, (int)Math.Floor((double)i / width), color);
                    i++;

                    // Explicitely cast width to float so it gets applied to height and i, too
                    float progress = (i / ((float)width * height)) * 100;
                    toolStripProgressBar.Value = (int)progress;
                }

                pictureBox.Image = bitmap;
                toolStripStatusLabel.Text = string.Format("File name: {0} / Width: {1} / Height: {2} / Maximum color value: {3}",
                    new string[] { Path.GetFileName(path), width.ToString(), height.ToString(), maximumColorValue.ToString() });
            }

            Cursor = Cursors.Default;
        }

        // TODO: Refactor ReadValue
        private int ReadValue(BinaryReader reader)
        {
            List<char> line = new List<char>();
            char c = reader.ReadChar();

            while (c != ' ' && c != '\t' && c != '\n')
            {
                line.Add(c);
                c = reader.ReadChar();
            }

            return int.Parse(new string(line.ToArray(), 0, line.Count));
        }

        // TODO: Refactor ReadWhitespace
        private void ReadWhitespace(BinaryReader reader)
        {
            char c = (char)reader.PeekChar();

            while (c == ' ' || c == '\t' || c == '\n')
            {
                reader.ReadChar();
                c = (char)reader.PeekChar();

                if (c == '#')
                {
                    reader.ReadChar();
                    while (c != '\n')
                    {
                        reader.ReadChar();
                        c = (char)reader.PeekChar();
                    } 
                }
            } 
        }
    }
}
