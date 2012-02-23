using System;
using System.IO;
using M.Tools;

namespace IDGPU
{
    // Molecular Dynamics under Isolated Boundary Conditions.
    public class MDIBC
    {
        // http://physics.nist.gov/cuu/Constants/index.html (2006)
        // c = 299792458 m/sec, e_charge = 1.602176487e-19 C, Kb = 1.3806504e-23 J/K
        // Ke = (e-charge * 1e+10 / (4 * pi * epsilon-zero)) = 1e-7 * c^2 * 1e+10 * e_charge = 14.39964414942 in eV*A
        // Kb = Kb / e_charge = 8.617342791e-5 in eV/K
        public const double Ke = 14.39964415; // eV * A
        public const double Kb = 8.617342791E-5; // eV / K
        private static Random rand = new Random(0);

        public static double dt = 0.5; // in 1e-14 sec
        public static double T = 2400; // in K

        public int Ions { get { return type.Length; } }
        public int Step { get { return step; } }
        public int Types { get { return mass.Length; } }
        public double Period // in A
        {
            get
            {
                return (periods == null || periods.Count < 10) ? pp.SolidPeriod(T) : period;
            }
        }
        public IForce Technique { get { return technique; } }

        public MDIBC(Crystal c, PairPotentials pp, IForce technique, Action<string> output)
        {
            Utility.SetDecimalSeparator();
            this.c = c;
            this.pp = pp;
            this.technique = technique;
            this.outputs = output;
            this.mass = pp.Material.IonMass;
            Init();
            technique.Init(type, pp, Types, Ions);
        }

        private double Maxvel(double mass) { return Math.Sign(rand.NextDouble() - 0.5) * Math.Sqrt(-2 * Math.Log(rand.NextDouble()) * Kb * T / mass); }

        private void Init()
        {
            type = c.Type; pos = c.Scale(Period); vel = new Double3[c.Ions]; acc = new Double3[c.Ions];

            int i, j, k;
            T_system = 0;
            for (i = 0; i < Ions; i++)
            {
                vel[i] = new Double3(Maxvel(mass[type[i]]), Maxvel(mass[type[i]]), Maxvel(mass[type[i]]));
                T_system += mass[type[i]] * vel[i].LengthSq();
            }
            k3N = Kb * 3 * Ions;
            T_system /= k3N;
            rfr_radius = Period * c.EdgeCells * 0.5; skip_intervals = (int)(rfr_intervals / (c.EdgeCells * 0.5));
            if (c.EdgeCells == 4) skip_intervals = (9 * skip_intervals) / 10;

            // Sort ions by type (for unrolls)
            int[] indices = new int[Ions];
            for (k = i = 0; i < Types; i++)
                for (j = 0; j < Ions; j++)
                    if (type[j] == i) indices[k++] = j;
            for (i = 0; i < Ions; i++)
            {
                j = indices[i]; if (i >= j) continue;
                Utility.Swap(ref type[i], ref type[j]);
                Utility.Swap(ref pos[i], ref pos[j]);
                Utility.Swap(ref vel[i], ref vel[j]);
            }

            periods = new FixedQueue<double>(); mean_periods = new FixedQueue<double>();
            i_periods = new FixedQueue<double>(); i_mean_periods = new FixedQueue<double>();
            temperatures = new FixedQueue<double>(); mean_temperatures = new FixedQueue<double>();
            energies = new FixedQueue<double>();
        }

        public void SaveResults()
        {
            string directory = "results " + Program.started.ToString("yyyy-MM-dd");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            string filename = String.Format(directory + "\\{0}-{1}-{2:F0}", pp.Name, Ions, T);
            if (File.Exists(filename)) File.Copy(filename, filename + ".bak", true);
            using (var w = new StreamWriter(filename, true))
                for (int i = output; i <= mean_periods.Count; i += output)
                {
                    double time = (step - mean_periods.Count + i) * dt / 100.0; // Save time in picoseconds instead of just step
                    w.Write("{0:F1}\t{1:F1}\t{2:F4}\t{3:F4}\t{4:F2}\t", time, mean_temperatures[i - 1], mean_periods[i - 1], i_mean_periods[i - 1], energies[i / output - 1]);
                    w.WriteLine();
                }
        }
        private void Force()
        {
            technique.SetPositions(pos, acc);
            if (step % output == 0)
            {
                energy = technique.Energy() + k3N * T_system / 2;
                energies.Enqueue(energy); if (energies.Count > autosave / output) energies.Dequeue();
            }
            else
            {
                technique.Force();
            }
        }
        private void Correct()
        {
            int i;
            Double3 impulse = Double3.Empty, moment = Double3.Empty, wel;
            Double3[] inertia = new[] { Double3.Empty, Double3.Empty, Double3.Empty };
            for (T_system = 0, i = 0; i < Ions; i++)
            {
                double mi = mass[type[i]];
                impulse += mi * vel[i]; T_system += mi * vel[i].LengthSq();
                inertia[0].x += mi * (pos[i].y * pos[i].y + pos[i].z * pos[i].z); inertia[0].y -= mi * pos[i].x * pos[i].y;
                inertia[1].y += mi * (pos[i].x * pos[i].x + pos[i].z * pos[i].z); inertia[1].z -= mi * pos[i].y * pos[i].z;
                inertia[2].z += mi * (pos[i].x * pos[i].x + pos[i].y * pos[i].y); inertia[2].x -= mi * pos[i].x * pos[i].z;
            }
            impulse /= Ions; inertia[0].z = inertia[2].x; inertia[1].x = inertia[0].y; inertia[2].y = inertia[1].z; Double3.Invert(inertia);
            for (i = 0; i < Ions; i++)
            {
                vel[i] -= impulse / mass[type[i]]; // Correct impulse
                moment += Double3.Cross(pos[i], vel[i]) * mass[type[i]];
            }
            wel = Double3.TransformCoordinate(moment, inertia); // Compute angular velocity
            for (i = 0; i < Ions; i++) vel[i] -= Double3.Cross(wel, pos[i]); // Correct moment

            // Stack temperatures for averaging and saving
            T_system /= k3N; temperatures.Enqueue(T_system); if (temperatures.Count > output) temperatures.Dequeue();
            T_mean = 0; foreach (double t in temperatures) T_mean += t; T_mean /= temperatures.Count;
            mean_temperatures.Enqueue(T_mean); if (mean_temperatures.Count > autosave) mean_temperatures.Dequeue();

            double tau_in_ps = step < relaxation ? 0.1 : 1, tau_in_steps = tau_in_ps / (0.01 * dt);
            for (i = 0; i < Ions; i++) vel[i] *= Math.Sqrt(1 + (T / T_system - 1) / tau_in_steps); // Berendsen, if tau = 1 step then DumbVelScaling
        }
        private void RevertEvaporatedParticles()
        {
            double r2 = 2 * c.EdgeCells * c.EdgeCells * Period * Period; // Some empirical radius of sphere
            for (int i = 0; i < Ions; i++)
            {
                if (pos[i].LengthSq() > r2)
                {
                    vel[i].x = -Math.Sign(pos[i].x) * Math.Abs(vel[i].x);
                    vel[i].y = -Math.Sign(pos[i].y) * Math.Abs(vel[i].y);
                    vel[i].z = -Math.Sign(pos[i].z) * Math.Abs(vel[i].z);
                }
            }
        }
        private void ComputeDensity()
        {
            int i, j;

            // Compute RFR
            int[][] rfr = new int[Types][];
            for (i = 0; i < Types; i++) rfr[i] = new int[rfr_intervals];
            for (i = 0; i < Ions; i++)
            {
                j = (int)(pos[i].Length() * rfr_intervals / rfr_radius);
                if (j >= 0 && j < rfr_intervals) rfr[type[i]][j]++;
            }

            // Compute internal density and period
            double[] density = new double[Types], N = new double[Types];
            for (j = 0; j < rfr_intervals - skip_intervals; j++)
            {
                double r = (j + 1) * rfr_radius / rfr_intervals;
                for (i = 0; i < Types; i++)
                {
                    N[i] += rfr[i][j];
                    if (j >= skip_intervals) density[i] += N[i] / (r * r * r); // D += N(r)/V(r), but join several first intervals to avoid small denominators
                }
            }
            for (i = 0; i < Types; i++) density[i] /= (double)(4 * Math.PI / 3) * (rfr_intervals - 2 * skip_intervals);
            i_periods.Enqueue((double)Math.Pow(4 / density[1], 1 / 3.0)); if (i_periods.Count > output) i_periods.Dequeue();
            i_period = 0; foreach (double p in i_periods) i_period += p; i_period /= i_periods.Count;
            i_mean_periods.Enqueue(periods.Count < 10 ? Period : i_period); if (i_mean_periods.Count > autosave) i_mean_periods.Dequeue();

            // Compute density and period
            density = new double[Types]; N = new double[Types];
            for (j = 0; j < rfr_intervals; j++)
            {
                double r = (j + 1) * rfr_radius / rfr_intervals;
                for (i = 0; i < Types; i++)
                {
                    N[i] += rfr[i][j];
                    if (j >= skip_intervals) density[i] += N[i] / (r * r * r); // D += N(r)/V(r), but join several first intervals to avoid small denominators
                }
            }
            for (i = 0; i < Types; i++) density[i] /= (double)(4 * Math.PI / 3) * (rfr_intervals - skip_intervals);
            periods.Enqueue((double)Math.Pow(4 / density[1], 1 / 3.0)); if (periods.Count > output) periods.Dequeue();
            period = 0; foreach (double p in periods) period += p; period /= periods.Count;
            mean_periods.Enqueue(Period); if (mean_periods.Count > autosave) mean_periods.Dequeue();
            rfr_radius = (double)(Period * c.EdgeCells * 0.5);
        }
        public void Update()
        {
            // One MD step: compute forces, then update velocities and correct them, then update positions and compute density
            step++;

            Force();

            // Integrate velocities
            for (int i = 0; i < Ions; i++)
            {
                vel[i] += acc[i] * (dt / mass[type[i]]);
                acc[i] = Double3.Empty;
            }

            Correct();

            // Integrate positions
            for (int i = 0; i < Ions; i++) pos[i] += vel[i] * dt;

            RevertEvaporatedParticles();
            ComputeDensity();

            if (autosave > 0 && step % autosave == 0) SaveResults();

            // Make output
            if (step % MainForm.text_output_interval == 0)
                outputs(String.Format("\r\n{0}\t{1:F0}\t{2:F0}\t{3:F4}\t{4:F4}\t{5:F3}", step, T_system, T_mean, period, i_period, energy));
        }

        private Action<string> outputs;
        private IForce technique;
        private PairPotentials pp;
        private Crystal c;

        // Current state
        private int step;
        private double T_mean, T_system, period, i_period, energy, rfr_radius, k3N;
        private FixedQueue<double> periods, mean_periods, temperatures, mean_temperatures, energies;
        private FixedQueue<double> i_periods, i_mean_periods; // Bulk (internal) lattice period
        public int[] type;
        public double[] mass;
        public Double3[] pos, vel, acc;

        // Parameters of simulation
        private int relaxation = 1000; // 4000; 20 ps
        private int rfr_intervals = 1000, skip_intervals; // for density computation

        // Parameters of output
        private int output = 200; // 1 ps
        public static int autosave = 5000; // 500 ps
    }
}
