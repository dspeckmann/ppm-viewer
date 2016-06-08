using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PpmViewer
{
    public partial class ZoomForm : Form
    {
        public float Zoom
        {
            get;
            private set;
        }

        public ZoomForm(float initialZoom)
        {
            InitializeComponent();

            zoomNumericUpDown.Value = (int)(initialZoom * 100.0f);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Zoom = ((float)zoomNumericUpDown.Value / 100.0f);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
