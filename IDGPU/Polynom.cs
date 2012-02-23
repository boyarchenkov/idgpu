using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDGPU
{
    public class Polynom
    {
        public Polynom(double[] coefs)
        {
            c = new double[coefs.Length];
            coefs.CopyTo(c, 0);
        }
        public Polynom(string coefs)
        {
            c = coefs.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(word => double.Parse(word)).ToArray();
        }

        public double Eval(double x)
        {
            double y = c[0];
            for (int i = 1; i < c.Length; i++) y = y * x + c[i];
            return y;
        }

        private double[] c;
    }
}
