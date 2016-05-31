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
        // TODO: Allow saving as PBM, maybe Plain PGM, PBM, PPM?
        // TODO: LoadPbm, LoadPgm and LoadPpm share too much code
        // TODO: Customize error message for different file types
        // TODO: How to distinguish between PPM and PlainPPM when saving?
        // TODO: Implement zoom?
        // TODO: Notify user when image is loading, now that WaitCursor is gone
        // TODO: try-catch does not seem to work with async-await

        public MainForm(string path)
            : this()
        {
            // TODO: Autosize window when given argument
            LoadImage(path);
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string path in openFileDialog.FileNames)
                {
                    LoadImage(path);
                }
            }
        }
        
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.TabCount < 1) return;

            PictureTabPage tab = (PictureTabPage)tabControl.SelectedTab;
            Image image = tab.Image;
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                string extension = Path.GetExtension(saveFileDialog.FileName);

                try
                /
                    if (extension == ".pbm" || extension == ".pgm" || extension == ".ppm")
                    {
                        Anymap.Save(image, saveFileDialog.FileName);
                    }
                    else
                    {
                        image.Save(saveFileDialog.FileName);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not save file as " + saveFileDialog.FileName + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Cursor = Cursors.Default;
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

        private async void LoadImage(string path)
        {
            string filename = Path.GetFileName(path);
            string extension = Path.GetExtension(path);

            try
            {
                Image image;
                if (extension == ".pbm" || extension == ".pgm" || extension == ".ppm")
                {
                    image = await Anymap.Load(path);
                }
                else
                {
                    image = Image.FromFile(path);
                }

                PictureTabPage tab = new PictureTabPage(filename, image);
                tabControl.TabPages.Add(tab);
                tabControl.SelectedTab = tab;
                
                saveAsToolStripMenuItem.Enabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading " + filename + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
