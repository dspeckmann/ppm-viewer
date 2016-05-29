using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using System.Linq;

namespace PpmViewer
{
    public partial class MainForm : Form
    {
        // TODO: Fix PBM loading
        // TODO: Do not ignore maximum color/gray value
        // TODO: Add support for PlainPBM, PlainPGM and PlainPPM
        // TODO: Allow saving as (plain) PPM, PGM or PBM
        // TODO: LoadPbm, LoadPgm and LoadPpm share too much code
        // TODO: Customize error message for different file types

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
                string[] customExtensions = { ".pbm", ".pgm", ".ppm" };

                if (customExtensions.Any(extension => extension == Path.GetExtension(openFileDialog.FileName)))
                {
                    LoadImage(openFileDialog.FileName);
                }
                else
                {
                    try
                    {
                        pictureBox.Image = Image.FromFile(openFileDialog.FileName);
                    }
                    catch(Exception)
                    {
                        MessageBox.Show("Error loading " + openFileDialog.FileName + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
                return;

            string oldStatusText = toolStripStatusLabel.Text;
            toolStripStatusLabel.Text = "Saving image...";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel.Text = "Saving image as " + saveFileDialog.FileName + "...";
                try
                {
                    pictureBox.Image.Save(saveFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not save file as " + saveFileDialog.FileName + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            toolStripStatusLabel.Text = oldStatusText;
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
            if (!File.Exists(path))
            {
                Cursor = Cursors.Default;
                MessageBox.Show(this, "File " + path + " cannot be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Cursor = Cursors.WaitCursor;
            toolStripProgressBar.Value = 0;
            toolStripStatusLabel.Text = "Loading...";


            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open), new ASCIIEncoding()))
            {
                // Check magic number
                if (reader.ReadChar() != 'P')
                {
                    Cursor = Cursors.Default;
                    toolStripStatusLabel.Text = "Could not load " + path;
                    MessageBox.Show(this, "File " + path + " is not a valid PPM file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Bitmap bitmap;

                    char c = reader.ReadChar();

                    switch (c)
                    {
                        case '1':
                            bitmap = LoadPlainPbm(reader);
                            break;
                        case '2':
                            bitmap = LoadPlainPgm(reader);
                            break;
                        case '3':
                            bitmap = LoadPlainPpm(reader);
                            break;
                        case '4':
                            bitmap = LoadPbm(reader);
                            break;
                        case '5':
                            bitmap = LoadPgm(reader);
                            break;
                        case '6':
                            bitmap = LoadPpm(reader);
                            break;
                        default:
                            Cursor = Cursors.Default;
                            toolStripStatusLabel.Text = "Could not load " + path;
                            MessageBox.Show(this, "File " + path + " is not a valid PPM file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }

                    pictureBox.Image = bitmap;
                    toolStripStatusLabel.Text = string.Format("File name: {0} / Width: {1} / Height: {2}",
                        Path.GetFileName(path), bitmap.Width.ToString(), bitmap.Height.ToString());
                }
                catch (Exception)
                {
                    Cursor = Cursors.Default;
                    toolStripStatusLabel.Text = "Could not load " + path;
                    MessageBox.Show(this, "File " + path + " is not a valid PPM file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            saveAsToolStripMenuItem.Enabled = true;
            Cursor = Cursors.Default;
        }

        private Bitmap LoadPlainPbm(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private Bitmap LoadPlainPgm(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private Bitmap LoadPlainPpm(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private Bitmap LoadPbm(BinaryReader reader)
        {
            // Read width, height and maximum color value while ignoring whitespace and comments
            ReadWhitespace(reader);
            int width = ReadValue(reader);
            ReadWhitespace(reader);
            int height = ReadValue(reader);
            ReadWhitespace(reader);

            // Read the actual pixels
            Bitmap bitmap = new Bitmap(width, height);
            int x = 0, y = 0;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                BitArray bits = new BitArray(reader.ReadByte());

                foreach (bool bit in bits)
                {
                    Color color;
                    if(bit)
                    {
                        color = Color.Black;
                    }
                    else
                    {
                        color = Color.White;
                    }
                    
                    bitmap.SetPixel(x, y, color);

                    // Explicitely cast width to float so it gets applied to height and i, too
                    float progress = ((x + 1) * (y + 1) / ((float)width * height)) * 100;
                    toolStripProgressBar.Value = (int)progress;

                    if (++x == width)
                    {
                        x = 0;
                        y++;
                        break;
                    }
                }
            }

            return bitmap;
        }

        private Bitmap LoadPgm(BinaryReader reader)
        {
            // Read width, height and maximum color value while ignoring whitespace and comments
            ReadWhitespace(reader);
            int width = ReadValue(reader);
            ReadWhitespace(reader);
            int height = ReadValue(reader);
            ReadWhitespace(reader);
            int maximumGrayValue = ReadValue(reader);
            ReadWhitespace(reader);

            // Read the actual pixels
            Bitmap bitmap = new Bitmap(width, height);
            int i = 0;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                byte grayValue = reader.ReadByte();
                Color color = Color.FromArgb(grayValue, grayValue, grayValue);
                bitmap.SetPixel(i % width, (int)Math.Floor((double)i / width), color);
                i++;

                // Explicitely cast width to float so it gets applied to height and i, too
                float progress = ((i + 1) / ((float)width * height)) * 100;
                toolStripProgressBar.Value = (int)progress;
            }

            return bitmap;
        }

        private Bitmap LoadPpm(BinaryReader reader)
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

            return bitmap;
        }

        private void SavePlainPbm(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private void SavePlainPgm(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private void SavePlainPpm(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private void SavePbm(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private void SavePgm(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        private void SavePpm(BinaryWriter writer)
        {
            throw new NotImplementedException();
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
                        c = reader.ReadChar();
                    }
                }
            }

            // When we encounter a non-whitespace character again we have to go back one byte, so it can be read by other functions
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
        }
    }
}
