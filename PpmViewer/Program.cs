using System;
using System.Windows.Forms;

namespace PpmViewer
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                Application.Run(new MainForm(args[0]));
            }
            else
            {
                Application.Run(new MainForm());
            }
        }
    }
}
