using System;
using M.Tools;

namespace IDGPU
{
    public class ForceCPU_IBC : IForce, IDisposable
    {
        public static readonly double cutoff = 10;
        public string Name { get { return "CPU C# IBC"; } }

        public void Dispose() { ions = types = 0; type = null; coefs2D = null; }
        public void SetPositions(Double3[] pos, Double3[] acc) { this.pos = pos; this.acc = acc; }
        public int Init(int[] type, PairPotentials pp, int types, int ions)
        {
            this.type = type;
            this.coefs2D = new double[types * types][];
            this.types = types;
            this.ions = ions;
            double[] coefs = pp.CoefsDouble8;
            for (int i = 0; i < types * types; i++)
            {
                int length = coefs.Length / (types * types);
                this.coefs2D[i] = new double[length];
                for (int j = 0; j < length; j++)
                    this.coefs2D[i][j] = coefs[i * length + j];
            }
            switch (pp.Form)
            {
                case "Buckingham": force = new Buckingham_Force(); force_energy = new Buckingham_ForceEnergy(); break;
                case "BuckinghamMorse": force = new BuckinghamMorse_Force(); force_energy = new BuckinghamMorse_ForceEnergy(); break;
                default: throw new NotImplementedException("Unknown potential form: " + pp.Form);
            }
            return 0;
        }
        private double TriangleCycle(Potential p)
        {
            double U = 0;
            for (int i = 0; i < ions; i++)
                for (int j = i + 1; j < ions; j++)
                {
                    double dx = pos[i].x - pos[j].x, dy = pos[i].y - pos[j].y, dz = pos[i].z - pos[j].z;
                    int type_ij = type[i] * types + type[j];
                    double dU = p.PairInteraction(ref U, type_ij, coefs2D[type_ij], Math.Sqrt(dx * dx + dy * dy + dz * dz));

                    acc[i].x += dx * dU; acc[i].y += dy * dU; acc[i].z += dz * dU;
                    acc[j].x -= dx * dU; acc[j].y -= dy * dU; acc[j].z -= dz * dU;
                }
            return U;
        }
        public void Force()
        {
            TriangleCycle(force);
        }
        public double Energy()
        {
            return TriangleCycle(force_energy);
        }

        private int ions, types;
        private int[] type;
        private double[][] coefs2D;
        private Double3[] pos, acc;
        private Potential force, force_energy;

        protected abstract class Potential
        {
            public abstract double PairInteraction(ref double U, int type_ij, double[] coefs, double R);
        }
        protected class Buckingham_Force : Potential
        {
            public override double PairInteraction(ref double U, int type_ij, double[] c, double R)
            {
                double r = 1 / R, r4 = (r * r) * (r * r), dU = r4 * (c[0] * R - (c[3] * r4) * 6);
                if (cutoff == 0 || R < cutoff) dU -= (c[1] * Math.Exp(c[2] * R)) * c[2] * r;
                return dU;
            }
        }
        protected class Buckingham_ForceEnergy : Potential
        {
            public override double PairInteraction(ref double U, int type_ij, double[] c, double R)
            {
                double r = 1 / R, r4 = (r * r) * (r * r), dU = r4 * (c[0] * R - (c[3] * r4) * 6);
                if (cutoff == 0 || R < cutoff)
                {
                    double e = c[1] * Math.Exp(c[2] * R);
                    dU -= e * c[2] * r; U += e;
                }
                U += r * (c[0] - (c[3] * r4) * r);
                return dU;
            }
        }
        protected class BuckinghamMorse_Force : Potential
        {
            public override double PairInteraction(ref double U, int type_ij, double[] c, double R)
            {
                double r = 1 / R, r4 = (r * r) * (r * r), dU = r4 * (c[0] * R - (c[3] * r4) * 6);
                if (cutoff == 0 || R < cutoff)
                {
                    double e = c[1] * Math.Exp(c[2] * R), ee = Math.Exp(c[5] * (R - c[6]));
                    dU -= (e * c[2] + c[4] * c[5] * ee * (2 * ee - 2)) * r;
                }
                return dU;
            }
        }
        protected class BuckinghamMorse_ForceEnergy : Potential
        {
            public override double PairInteraction(ref double U, int type_ij, double[] c, double R)
            {
                double r = 1 / R, r4 = (r * r) * (r * r), dU = r4 * (c[0] * R - (c[3] * r4) * 6);
                if (cutoff == 0 || R < cutoff)
                {
                    double e = c[1] * Math.Exp(c[2] * R), ee = Math.Exp(c[5] * (R - c[6]));
                    dU -= (e * c[2] + c[4] * c[5] * ee * (2 * ee - 2)) * r;
                    U += e + c[4] * ee * (ee - 2);
                }
                U += r * (c[0] - (c[3] * r4) * r);
                return dU;
            }
        }
        protected class Buckingham4_Force : Potential
        {
            public override double PairInteraction(ref double U, int type_ij, double[] c, double R)
            {
                double r = 1 / R, r2 = r * r, dU = c[0] * r2 * r;
                switch (type_ij)
                {
                    case 0:
                        if (R < 2.1) dU -= ((((-27.244726 * 5 * R + 246.43471 * 4) * R - 881.96861 * 3) * R + 1562.2235 * 2) * R - 1372.5306) * r;
                        else if (R < 2.6) dU -= ((-3.1313949 * 3 * R + 23.077354 * 2) * R - 55.496531) * r;
                        else if (cutoff == 0 || R < cutoff) dU -= 6 * c[3] * r2 * r2 * r2 * r2;
                        break;
                    case 1:
                    case 2:
                        if (cutoff == 0 || R < cutoff) dU -= c[2] * c[1] * Math.Exp(c[2] * R) * r;
                        break;
                }
                return dU;
            }
        }
        protected class Buckingham4_ForceEnergy : Potential
        {
            public override double PairInteraction(ref double U, int type_ij, double[] c, double R)
            {
                double r = 1 / R, r2 = r * r, dU = c[0] * r2 * r;
                U += c[0] * r;
                switch (type_ij)
                {
                    case 0:
                        if (R < 2.1)
                        {
                            dU -= ((((-27.244726 * 5 * R + 246.43471 * 4) * R - 881.96861 * 3) * R + 1562.2235 * 2) * R - 1372.5306) * r;
                            U += ((((-27.244726 * R + 246.43471) * R - 881.96861) * R + 1562.2235) * R - 1372.5306) * R + 479.95538;
                        }
                        else if (R < 2.6)
                        {
                            dU -= ((-3.1313949 * 3 * R + 23.077354 * 2) * R - 55.496531) * r;
                            U += ((-3.1313949 * R + 23.077354) * R - 55.496531) * R + 42.891691;
                        }
                        else if (cutoff == 0 || R < cutoff)
                        {
                            dU -= 6 * c[3] * r2 * r2 * r2 * r2;
                            U -= c[3] * r2 * r2 * r2;
                        }
                        break;
                    case 1:
                    case 2:
                        if (cutoff == 0 || R < cutoff)
                        {
                            dU -= c[2] * c[1] * Math.Exp(c[2] * R) * r;
                            U += c[1] * Math.Exp(c[2] * R);
                        }
                        break;
                }
                return dU;
            }
        }
    }
}
