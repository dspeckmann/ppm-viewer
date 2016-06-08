using System.Drawing;
using System.Windows.Forms;

namespace PpmViewer
{
    class PictureTabPage : TabPage
    {
        private Label loadingLabel;

        private float _zoom = 1.0f;
        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                // TODO: Restrict zoom differently? Maybe depending on image size?
                if (value < 0.1f)
                {
                    _zoom = 0.1f;
                }
                else if (value > 2.0f)
                {
                    _zoom = 2.0f;
                }
                else
                {
                    _zoom = value;
                }
                Invalidate();
            }
        }

        private Image _image;
        public Image Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (Controls.Contains(loadingLabel)) Controls.Remove(loadingLabel);
                _image = value;
                Invalidate();
            }
        }

        public PictureTabPage(string text)
            : base(text)
        {
            AutoScroll = true;
            DoubleBuffered = true;

            loadingLabel = new Label();
            loadingLabel.Text = "Loading...";
            loadingLabel.Location = new Point(6, 6);
            Controls.Add(loadingLabel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Image != null)
            {
                int newWidth = (int)(Zoom * Image.Width);
                int newHeight = (int)(Zoom * Image.Height);
                e.Graphics.DrawImage(Image, new Rectangle(AutoScrollPosition.X, AutoScrollPosition.Y, newWidth, newHeight));
                AutoScrollMinSize = new Size(newWidth, newHeight);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                Zoom += (e.Delta / 1000.0f);
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }
    }
}
