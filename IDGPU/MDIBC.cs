﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
        public static double kJ_mol; // Conversion factor of energy to kJ/mol

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

        public MDIBC(Configuration cfg, Crystal c, PairPotentials pp, IForce technique, Action<string> append_text)
        {
            edge_cells = c.EdgeCells;
            dt = cfg.Get_dt_in_fs() / 10.0;
            T = cfg.GetTemperatureInKelvins("T");
            rfr_intervals = cfg["rfr-intervals"].ToInt();
            output = cfg.GetTimeInSteps("output");
            energy_interval = cfg.GetTimeInSteps("energy-interval");
            relaxation = cfg.GetTimeInSteps("relaxation");
            autosave = cfg.GetTimeInSteps("autosave");
            tau_t = cfg.GetTimeInFractionalSteps("tau-t");
            tau_t_relaxation = cfg.GetTimeInFractionalSteps("tau-t-relaxation");
            MSD_reset_interval = cfg.GetTimeInSteps("MSD-reset-interval");

            kJ_mol = 96.485 / c.Ions * c.Cell.IonsInMolecule;
            this.pp = pp;
            this.technique = technique;
            this.append_text = append_text;
            this.mass = pp.Material.IonMass;

            type = c.Type; origin = c.Scale(Period); pos = new Double3[c.Ions]; vel = new Double3[c.Ions]; acc = new Double3[c.Ions];
            origin.CopyTo(pos, 0);
            for (int i = 0; i < Ions; i++)
                vel[i] = new Double3(Maxvel(mass[type[i]]), Maxvel(mass[type[i]]), Maxvel(mass[type[i]]));

            Init();
        }

        private double Maxvel(double mass) { return Math.Sign(rand.NextDouble() - 0.5) * Math.Sqrt(-2 * Math.Log(rand.NextDouble()) * Kb * T / mass); }

        private void Init()
        {
            k3N = Kb * 3 * Ions;
            T_system = Temperature();

            rfr_radius = Period * edge_cells * 0.5; skip_intervals = (int)(rfr_intervals / (edge_cells * 0.5));
            if (edge_cells == 4) skip_intervals = (9 * skip_intervals) / 10;

            // Sort ions by type (for unrolls)
            int i, j, k;
            int[] indices = new int[Ions];
            for (k = i = 0; i < Types; i++)
                for (j = 0; j < Ions; j++)
                    if (type[j] == i) indices[k++] = j;
            for (i = 0; i < Ions; i++)
            {
                j = indices[i]; if (i >= j) continue;
                Utility.Swap(ref type[i], ref type[j]);
                Utility.Swap(ref pos[i], ref pos[j]);
                Utility.Swap(ref origin[i], ref origin[j]);
                Utility.Swap(ref vel[i], ref vel[j]);
            }

            periods = new IndexableQueue<double>(); mean_periods = new IndexableQueue<double>();
            central_periods = new IndexableQueue<double>(); central_mean_periods = new IndexableQueue<double>();
            temperatures = new IndexableQueue<double>(); mean_temperatures = new IndexableQueue<double>();
            energies = new IndexableQueue<double>();

            technique.Init(type, pp, Types, Ions);

            layer_count = new IndexableQueue<int>[edge_cells / 2][][];
            layer_dist = new IndexableQueue<double>[edge_cells / 2][][];
            last_layer_count = new int[edge_cells / 2][][];
            last_layer_dist = new double[edge_cells / 2][][];
            for (i = 0; i < edge_cells / 2; i++)
            {
                layer_count[i] = new IndexableQueue<int>[edge_cells / 2][];
                layer_dist[i] = new IndexableQueue<double>[edge_cells / 2][];
                last_layer_count[i] = new int[edge_cells / 2][];
                last_layer_dist[i] = new double[edge_cells / 2][];
                for (j = 0; j < edge_cells / 2; j++)
                {
                    layer_count[i][j] = new IndexableQueue<int>[Types];
                    layer_dist[i][j] = new IndexableQueue<double>[Types];
                    last_layer_count[i][j] = new int[Types];
                    last_layer_dist[i][j] = new double[Types];
                    for (k = 0; k < Types; k++)
                    {
                        layer_count[i][j][k] = new IndexableQueue<int>();
                        layer_dist[i][j][k] = new IndexableQueue<double>();
                    }
                }
            }

            bilayer_count = new IndexableQueue<int>[2][];
            bilayer_dist = new IndexableQueue<double>[2][];
            last_bilayer_count = new int[2][];
            last_bilayer_dist = new double[2][];
            for (i = 0; i < 2; i++)
            {
                bilayer_count[i] = new IndexableQueue<int>[Types];
                bilayer_dist[i] = new IndexableQueue<double>[Types];
                last_bilayer_count[i] = new int[Types];
                last_bilayer_dist[i] = new double[Types];
                for (k = 0; k < Types; k++)
                {
                    bilayer_count[i][k] = new IndexableQueue<int>();
                    bilayer_dist[i][k] = new IndexableQueue<double>();
                }
            }
        }

        public void Load(string filename)
        {
            var root = XDocument.Load(filename).Root;
            var m = MainForm.materials[root.Attribute("material").Value];
            pp = PairPotentials.LoadPotentialsFromFile(m, MainForm.potentials_filename)[root.Attribute("potentials").Value];
            edge_cells = root.Int("edge-cells");
            T = root.Double("T");
            dt = root.Double("timestep");
            autosave = root.Int("autosave-step");
            step = root.Element("Ions").Int("step");
            var ions = root.Element("Ions").Elements().ToArray();
            type = new int[ions.Length];
            pos = new Double3[ions.Length];
            origin = new Double3[ions.Length];
            vel = new Double3[ions.Length];
            acc = new Double3[ions.Length];
            for (int i = 0; i < ions.Length; i++)
            {
                type[i] = ions[i].Int("type");
                var d = ions[i].Attribute("pos").Value.ToDoubleArray();
                pos[i] = new Double3(d[0], d[1], d[2]);
                d = ions[i].Attribute("origin").Value.ToDoubleArray();
                origin[i] = new Double3(d[0], d[1], d[2]);
                d = ions[i].Attribute("vel").Value.ToDoubleArray();
                vel[i] = new Double3(d[0], d[1], d[2]);
            }
            Init();
        }
        public void Save(string filename)
        {
            var ions = new XElement("Ions", new XAttribute("step", step));
            for (int i = 0; i < Ions; i++)
                ions.Add(new XElement("Ion",
                            new XAttribute("type", type[i]),
                            new XAttribute("pos", pos[i].ToString("F9")),
                            new XAttribute("origin", origin[i].ToString("F9")),
                            new XAttribute("vel", vel[i].ToString("F9"))));

            new XDocument(
                    new XElement("Simulation",
                        new XAttribute("material", pp.Material.Formula),
                        new XAttribute("potentials", pp.Name),
                        new XAttribute("edge-cells", edge_cells),
                        new XAttribute("T", T),
                        new XAttribute("timestep", dt),
                        new XAttribute("autosave-step", autosave),
                        ions
                    )
                ).Save(filename);
        }
        public void SaveResults(string path)
        {
            int i, j, k;
            string filename = path + ".txt";
            if (File.Exists(filename)) File.Copy(filename, filename + ".bak", true);
            using (var w = new StreamWriter(filename, true))
                for (i = output; i <= mean_periods.Count; i += output)
                {
                    double time = (step - mean_periods.Count + i) * dt / 100.0; // Save time in picoseconds instead of just steps
                    j = Math.Max(1, i / energy_interval);
                    w.Write("{0:F1}\t{1:F1}\t{2:F4}\t{3:F4}\t{4:F2}\t",
                        time, mean_temperatures[i - 1], mean_periods[i - 1], central_mean_periods[i - 1], energies[j - 1]);
                    for (j = 0; j < Types; j++)
                        for (k = 0; k < 2; k++)
                            w.Write("{0:F3}\t", bilayer_dist[k][j].Dequeue());
                    //for (j = 0; j < Types; j++)
                    //    for (k = 0; k < edge_cells / 2; k++)
                    //    {
                    //        w.Write("{0:F3}\t", layer_dist[k][k][j][i / output - 1]);
                    //        layer_count[k][k][j].Dequeue(); layer_dist[k][k][j].Dequeue();
                    //    }
                    w.WriteLine();
                }
        }
        private void Force()
        {
            technique.SetPositions(pos, acc);
            if (step % energy_interval == 0)
            {
                energy = (technique.Energy() + k3N * T_system / 2) * kJ_mol;
                energies.Enqueue(energy); if (energies.Count * energy_interval > autosave) energies.Dequeue();
            }
            else
            {
                technique.Force();
            }
        }
        private double Temperature()
        {
            double mvv = 0;
            for (int i = 0; i < Ions; i++) mvv += mass[type[i]] * vel[i].LengthSq();
            return mvv / k3N;
        }
        private void Correct()
        {
            int i;
            Double3 impulse = Double3.Empty, moment = Double3.Empty, wel;
            Double3[] inertia = new[] { Double3.Empty, Double3.Empty, Double3.Empty };
            for (T_system = 0, i = 0; i < Ions; i++)
            {
                double mi = mass[type[i]];
                impulse += mi * vel[i];
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
            T_system = Temperature(); temperatures.Enqueue(T_system); if (temperatures.Count > output) temperatures.Dequeue();
            T_mean = 0; foreach (double t in temperatures) T_mean += t; T_mean /= temperatures.Count;
            mean_temperatures.Enqueue(T_mean); if (mean_temperatures.Count > autosave) mean_temperatures.Dequeue();

            double tau = step < relaxation ? tau_t_relaxation : tau_t;
            for (i = 0; i < Ions; i++) vel[i] *= Math.Sqrt(1 + (T / T_system - 1) / tau); // Berendsen, if tau = 1 step then DumbVelScaling
        }
        private void RevertEvaporatedParticles()
        {
            double r2 = 2 * edge_cells * edge_cells * Period * Period; // Some empirical radius of bounding sphere
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
            central_periods.Enqueue((double)Math.Pow(4 / density[1], 1 / 3.0)); if (central_periods.Count > output) central_periods.Dequeue();
            central_period = 0; foreach (double p in central_periods) central_period += p; central_period /= central_periods.Count;
            central_mean_periods.Enqueue(periods.Count < 10 ? Period : central_period); if (central_mean_periods.Count > autosave) central_mean_periods.Dequeue();

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
            rfr_radius = (double)(Period * edge_cells * 0.5);
        }
        private void ComputeCKC()
        {
            int i, j, k;
            if (step % output == 0)
            {
                for (i = 0; i < edge_cells / 2; i++)
                    for (j = 0; j < edge_cells / 2; j++)
                        for (k = 0; k < Types; k++)
                        {
                            last_layer_count[i][j][k] = 0;
                            last_layer_dist[i][j][k] = 0;
                        }
                for (i = 0; i < Ions; i++)
                {
                    double o = origin[i].Length(), r = pos[i].Length(); // SphereLayers
                    int layer_o = 0, layer_r = 0; // joined core, if "= 1" and every "for" [from 0 to cells / 2] starts from 1
                    for (j = 0; j < edge_cells / 2; j++)
                    {
                        if (o > j * Period) layer_o = j;
                        if (r > j * Period) layer_r = j;
                    }
                    last_layer_count[layer_o][layer_r][type[i]]++;
                    if (layer_o == layer_r) last_layer_dist[layer_o][layer_r][type[i]] += (pos[i] - origin[i]).LengthSq();
                    else if (layer_o > layer_r) last_layer_dist[layer_o][layer_r][type[i]] += (layer_r + 1) * Period - r;
                    else if (layer_o < layer_r) last_layer_dist[layer_o][layer_r][type[i]] += r - layer_r * Period;
                }
                for (i = 0; i < edge_cells / 2; i++)
                    for (j = 0; j < edge_cells / 2; j++)
                        for (k = 0; k < Types; k++)
                        {
                            if (last_layer_count[i][j][k] > 0) last_layer_dist[i][j][k] /= last_layer_count[i][j][k];
                            layer_count[i][j][k].Enqueue(last_layer_count[i][j][k]);
                            layer_dist[i][j][k].Enqueue(last_layer_dist[i][j][k]);
                        }
            }
            if (step % output == 0)
            {
                for (i = 0; i < 2; i++)
                    for (k = 0; k < Types; k++)
                    {
                        last_bilayer_count[i][k] = 0;
                        last_bilayer_dist[i][k] = 0;
                    }
                for (i = 0; i < Ions; i++)
                {
                    //Float o = origin[i].Length(), r = pos[i].Length(), border = (Float)((cells * 0.5 - 1.125) * Period); // SphereLayers
                    double o = origin[i].Length(), r = pos[i].Length(), border = (edge_cells * 0.5 - 1.0) * Period; // SphereLayers
                    j = -1; if (o < border && r < border) j = 0; if (o > border && r > border) j = 1;
                    if (j >= 0)
                    {
                        last_bilayer_count[j][type[i]]++;
                        last_bilayer_dist[j][type[i]] += (pos[i] - origin[i]).LengthSq();
                    }
                }
                for (i = 0; i < 2; i++)
                    for (k = 0; k < Types; k++)
                    {
                        if (last_bilayer_count[i][k] > 0) last_bilayer_dist[i][k] /= last_bilayer_count[i][k];
                        bilayer_count[i][k].Enqueue(last_bilayer_count[i][k]);
                        bilayer_dist[i][k].Enqueue(last_bilayer_dist[i][k]);
                    }
            }
        }
        public void Update()
        {
            // One MD step: compute forces, then update velocities and correct them, then update positions and compute density
            step++;

            Force();

            // Integration and corrections
            int i;
            for (i = 0; i < Ions; i++)
            {
                vel[i] += acc[i] * (dt / mass[type[i]]);
                acc[i] = Double3.Empty;
            }
            Correct();
            for (i = 0; i < Ions; i++) pos[i] += vel[i] * dt;

            // Analysis
            RevertEvaporatedParticles();
            ComputeDensity();
            ComputeCKC();

            // Reset origin of each ion after relaxation and at the given intervals
            if (step == relaxation || (MSD_reset_interval > 0 && step % MSD_reset_interval == 0)) pos.CopyTo(origin, 0);

            // Output to screen and files
            if (autosave > 0 && step % autosave == 0)
            {
                string path = "results " + Program.started.ToString("yyyy-MM-dd");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path += String.Format("\\{0}-{1}-{2:F0}", pp.Name, Ions, T);
                SaveResults(path);
                Save(String.Format("{0} {1}.sim", path, step));
            }
            if (step % MainForm.text_output_interval == 0)
            {
                double CKC_U_bulk = last_bilayer_dist[0][1];
                append_text(String.Format("\r\n{0}\t{1:F0}\t{2:F0}\t{3:F4}\t{4:F4}\t{5:F3}\t{6:F1}",
                    step, T_system, T_mean, period, central_period, energy, CKC_U_bulk));
            }
        }

        private Action<string> append_text;
        private IForce technique;
        private PairPotentials pp;

        // Current state
        private int step, edge_cells;
        private double T_mean, T_system, period, central_period, energy, rfr_radius, k3N;
        private IndexableQueue<double> periods, mean_periods, temperatures, mean_temperatures, energies;
        private IndexableQueue<double> central_periods, central_mean_periods; // Bulk (internal) lattice period
        public int[] type;
        public double[] mass;
        public Double3[] pos, vel, acc, origin;

        // Parameters of simulation
        private double tau_t = 200, tau_t_relaxation = 20; // in steps
        private double MSD_reset_interval = 1000000; // in steps
        private int relaxation = 1000; // in steps
        private int rfr_intervals = 1000, skip_intervals; // For density computation

        // Parameters of output
        private int output = 200; // in steps
        private int energy_interval = 200; // in steps
        public static int autosave = 5000; // in steps

        // Multiple layer diffusion
        IndexableQueue<int>[][][] layer_count;
        IndexableQueue<double>[][][] layer_dist;
        int[][][] last_layer_count;
        double[][][] last_layer_dist;

        // Two layer diffusion (bulk + surface)
        IndexableQueue<int>[][] bilayer_count;
        IndexableQueue<double>[][] bilayer_dist;
        int[][] last_bilayer_count;
        public double[][] last_bilayer_dist;
    }
}
