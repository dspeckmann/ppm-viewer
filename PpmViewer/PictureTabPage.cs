using System.Drawing;
using System.Windows.Forms;

namespace PpmViewer
{
    class PictureTabPage : TabPage
    {
        private Label loadingLabel;
        private PictureBox pictureBox;

        public Image Image
        {
            get
            {
                return pictureBox.Image;
            }
            set
            {
                if (Controls.Contains(loadingLabel)) Controls.Remove(loadingLabel);
                pictureBox.Image = value;
            }
        }

        public PictureTabPage(string text)
            : base(text)
        {
            AutoScroll = true;

            loadingLabel = new Label();
            loadingLabel.Text = "Loading...";
            loadingLabel.Location = new Point(6, 6);
            Controls.Add(loadingLabel);

            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            Controls.Add(pictureBox);
        }
    }
}
