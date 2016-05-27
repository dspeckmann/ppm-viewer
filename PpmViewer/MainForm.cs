using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PpmViewer
{
    public partial class MainForm : Form
    {
        // TODO: Do not ignore maximum color value
        // TODO: Add support for other file types (first check for 'P', then switch case for the next char and abort if number doesn't match)

        public MainForm(string path)
            : this()
        {
            LoadImage(path);
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadImage(openFileDialog.FileName);
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

        /// <summary>
        /// Tries to load a PPM image from the given file path.
        /// </summary>
        /// <param name="path">The file to open.</param>
        private void LoadImage(string path)
        {
            if(!File.Exists(path))
            {
                Cursor = Cursors.Default;
                MessageBox.Show(this, "File " + path + " cannot be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Cursor = Cursors.WaitCursor;
            toolStripProgressBar.Value = 0;
            
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open), new ASCIIEncoding()))
            {
                // Check magic number
                if (reader.ReadChar() != 'P' || reader.ReadChar() != '6')
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, "File " + path + " is not a valid PPM file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
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
                catch(Exception)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, "File " + path + " is not a valid PPM file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Cursor = Cursors.Default;
        }
        

        /// <summary>
        /// Reads from the BinaryReader until it encounters whitespace and tries to convert the read bytes to an integer.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <returns>The read integer.</returns>
        private int ReadValue(BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char c = reader.ReadChar();

            while (c != ' ' && c != '\t' && c != '\n')
            {
                builder.Append(c);
                c = reader.ReadChar();
            }
            
            return int.Parse(builder.ToString());
        }
        
        /// <summary>
        /// Reads from the BinaryReader until it encounters non-whitespace characters.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        private void ReadWhitespace(BinaryReader reader)
        {
            char c = reader.ReadChar();

            while (c == ' ' || c == '\t' || c == '\n')
            {
                c = reader.ReadChar();

                // When we encounter a comment, read until the end of it (has to be a newline)
                if (c == '#')
                {
                    reader.ReadChar();
                    while (c != '\n')
                    {
                        reader.ReadChar();
                        c = reader.ReadChar();
                    }
                }
            }

            // When we encounter a non-whitespace character again we have to go back one byte, so it can be read by other functions
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
        }
    }
}
