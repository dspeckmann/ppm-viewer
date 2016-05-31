using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace PpmViewer
{
    // TODO: Do not ignore maximum color/gray value
    // TODO: Notify user when image is loading, now that WaitCursor is gone
    // TODO: try-catch does not seem to work with async-await
    // TODO: Check license of Anymap file formats and names
    // TODO: Improve about screen with custom graphics and link to GitHub
    // TODO: Detect anymap formats by magic number, not extension?
    // TODO: Implement zoom?
    // TODO: Add support for Plain PBM, PGM, PPM?
    // TODO: How to distinguish between binary and plain formats when saving?

    public partial class MainForm : Form
    {
        public MainForm(string path)
            : this()
        {
            // TODO: Autosize window when given argument (maximize, when image is bigger than screen)
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
                {
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
