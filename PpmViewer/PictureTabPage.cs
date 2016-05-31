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
            set
            {
                pictureBox.Image = value;
            }
        }

        public PictureTabPage(string text)
            : base(text)
        {
            AutoScroll = true;

            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            Controls.Add(pictureBox);
        }
    }
}
