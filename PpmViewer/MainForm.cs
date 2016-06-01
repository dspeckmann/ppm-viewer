using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace PpmViewer
{
    // TODO: Do not ignore maximum color/gray value
    // TODO: try-catch does not seem to work with async-await
    // TODO: Detect anymap formats by magic number, not extension?
    // TODO: Add support for Plain PBM, PGM, PPM?
    // TODO: How to distinguish between binary and plain formats when saving?
    // TODO: Implement zoom?
    // TODO: AutoScaleMode Font vs DPI?

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
                string path = saveFileDialog.FileName;
                string extension = Path.GetExtension(saveFileDialog.FileName);

                Cursor = Cursors.WaitCursor;

                try
                {
                    if (extension == ".pbm" || extension == ".pgm" || extension == ".ppm")
                    {
                        Anymap.Save(image, path);
                    }
                    else
                    {
                        image.Save(path);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(string.Format("Could not save file as {0}!", path), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            PictureTabPage tab = new PictureTabPage(filename);
            tabControl.TabPages.Add(tab);
            tabControl.SelectedTab = tab;
            tab.Cursor = Cursors.WaitCursor;

            try
            {
                Image image;
                if (extension == ".pbm" || extension == ".pgm" || extension == ".ppm")
                {
                    image = await Anymap.LoadAsync(path);
                }
                else
                {
                    image = Image.FromFile(path);
                }

                tab.Image = image;
                saveAsToolStripMenuItem.Enabled = true;
            }
            catch (Exception)
            {
                tabControl.TabPages.Remove(tab);
                MessageBox.Show(string.Format("Error loading {0}!", path), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            tab.Cursor = Cursors.Default;
        }

        private void tabControl_TabClosed(object sender, EventArgs e)
        {
            if(tabControl.TabCount < 1)
            {
                saveAsToolStripMenuItem.Enabled = false;
            }
        }
    }
}
