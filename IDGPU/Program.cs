using System;
using System.Windows.Forms;

namespace IDGPU
{
    static class Program
    {
        public static DateTime started = DateTime.Now;

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length >= 1) MainForm.config_filename = args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
