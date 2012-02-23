using System;
using System.Windows.Forms;
using System.Threading;
using M.Tools;

namespace IDGPU
{
    public partial class MainForm : Form
    {
        public static int text_output_interval = 100;

        public bool Paused
        {
            get
            {
                return buttonPause.Text == ">";
            }
            set
            {
                buttonPause.Text = value ? ">" : "=";
            }
        }
        public MainForm()
        {
            InitializeComponent();

            t = new Thread(Run);
            t.Start();
        }

        private void Run()
        {
            System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);

            string[] ions = new[] {"O", "U"}; double[] mass = new double[] { 16, 238 }, charge = new double[] { -2, 4 }; // UO2
            var pp = PairPotentials.LoadPotentialsFromFile(ions, charge, "Data\\UO2.spp");
            var technique = new ForceDX11_IBC();
            //var technique = new ForceCPU_IBC();
            MDIBC md = new MDIBC(4, pp["MOX-07"], mass, charge, technique, AppendString);
            Clock clock = new Clock();

            Paused = false;
            float besttime = 1000, mean_time = 0;
            while (true)
            {
                while (Paused) Thread.Sleep(10);

                float time = clock.ElapsedTime;
                md.Update();
                time = clock.ElapsedTime - time;
                mean_time += time; besttime = Math.Min(time, besttime);

                if (md.Step % text_output_interval == 0)
                {
                    mean_time /= text_output_interval;
                    SetText(String.Format("{0} T={1} N={2} dt={3:F3} {4} {5}",
                        md.Step, MDIBC.T, md.Ions, MDIBC.dt, md.Technique.Name, besttime));
                    mean_time = 0;
                }
            }
        }
        private void SetText(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetText), s);
                return;
            }
            Text = s;
        }
        private void AppendString(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendString), s);
                return;
            }
            textBoxOut.AppendText(s);
        }
        private void MainFormClosed(object sender, FormClosedEventArgs e)
        {
            t.Abort();
        }
        private void buttonPause_Click(object sender, EventArgs e)
        {
            Paused = !Paused;
            if (Paused) Text = "= " + Text;
        }
        private void textBoxOut_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxOut.Text);
        }

        Thread t;
    }
}
