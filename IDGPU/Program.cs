using System;
using System.Windows.Forms;

namespace IDGPU
{
    static class Program
    {
        public static DateTime started = DateTime.Now;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
