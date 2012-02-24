using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using M.Tools;

namespace IDGPU
{
    public partial class MainForm : Form
    {
        public static int text_output_interval = 100;
        public static string materials_filename = "Data\\Materials.mat";
        public static string cells_filename = "Data\\UnitCells.uc";
        public static string potentials_filename = "Data\\UO2.spp";
        public static Dictionary<string, UnitCell> unit_cells = UnitCell.LoadUnitCellsFromFile(cells_filename);
        public static Dictionary<string, Material> materials = Material.LoadMaterialsFromFile(materials_filename);

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
            Utility.SetDecimalSeparator();

            var m = materials["UO2"];
            var cell = unit_cells[m.UnitCell];
            var potentials = PairPotentials.LoadPotentialsFromFile(m, potentials_filename);

            var technique = new ForceDX11_IBC();
            //var technique = new ForceCPU_IBC();
            MDIBC md = new MDIBC(Crystal.CreateCube(cell, 4), potentials["MOX-07"], technique, AppendText);
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
                    SetTitle(String.Format("{0} T={1} N={2} dt={3:F3} {4} {5}",
                        md.Step, MDIBC.T, md.Ions, MDIBC.dt, md.Technique.Name, besttime));
                    mean_time = 0;
                }
            }
        }
        private void SetTitle(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetTitle), s);
                return;
            }
            Text = s;
        }
        private void AppendText(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendText), s);
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
