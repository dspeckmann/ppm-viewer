using System.Drawing;
using System.Windows.Forms;

namespace PpmViewer
{
    class PictureTabPage : TabPage
    {
        private PictureBox pictureBox;

        public Image Image
        {
            get
            {
                return pictureBox.Image;
            }
        }

        public PictureTabPage(string text, Image image)
            : base(text)
        {
            AutoScroll = true;

            pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            Controls.Add(pictureBox);
        }
    }
}
