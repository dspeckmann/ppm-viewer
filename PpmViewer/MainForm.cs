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
        // TODO: Allow saving as PGM or PBM, maybe Plain PGM, PBM, PPM?
        // TODO: Fix PPM saving
        // TODO: LoadPbm, LoadPgm and LoadPpm share too much code
        // TODO: Customize error message for different file types
        // TODO: How to distinguish between PPM and PlainPPM when saving?

        public MainForm(string path)
            : this()
        {
            // TODO: Autosize window when given argument
            Anymap.Load(path); // TODO: Error handling? Combine with openToolStripMenuItem_Click to one method
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                toolStripProgressBar.Value = 0;
                toolStripStatusLabel.Text = "Loading...";
                string extension = Path.GetExtension(openFileDialog.FileName);

                try
                {
                    if (extension == ".pbm" || extension == ".pgm" || extension == ".ppm")
                    {
                        pictureBox.Image = Anymap.Load(openFileDialog.FileName);
                    }
                    else
                    {
                        pictureBox.Image = Image.FromFile(openFileDialog.FileName);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error loading " + openFileDialog.FileName + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                toolStripProgressBar.Value = 100;
                saveAsToolStripMenuItem.Enabled = true;
                Cursor = Cursors.Default;
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
                string extension = Path.GetExtension(saveFileDialog.FileName);

                try
                {
                    if (extension == ".pbm" || extension == ".pgm" || extension == ".ppm")
                    {
                        Anymap.Save(pictureBox.Image, saveFileDialog.FileName);
                    }
                    else
                    {

                        pictureBox.Image.Save(saveFileDialog.FileName);
                    }
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
    }
}
