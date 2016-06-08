using System;
using System.Drawing;
using System.Windows.Forms;

namespace PpmViewer
{
    class ClosableTabControl : TabControl
    {
        public event EventHandler TabClosed;

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            TabPage tab = (TabPage)e.Control;
            tab.Text += "   x";
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle bounds = GetTabRect(i);
                Rectangle closeButtonBounds = new Rectangle(bounds.Right - 15, bounds.Top + 5, 15, bounds.Height - 10);
                if(closeButtonBounds.Contains(e.Location))
                {
                    TabPages.RemoveAt(i);
                    OnTabClosed(new EventArgs());
                }
            }
        }

        protected virtual void OnTabClosed(EventArgs e)
        {
            if (TabClosed != null) TabClosed(this, e);
        }
    }
}
