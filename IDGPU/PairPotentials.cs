using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace IDGPU
{
    public class PairPotentials
    {
        public static Dictionary<string, PairPotentials> LoadPotentialsFromFile(string[] ion_types, double[] charge, string filename)
        {
            var doc = XDocument.Load(filename).Root ?? new XElement("Sets");
            return doc.Elements("Set").Select(set => new PairPotentials(ion_types, charge, set)).ToDictionary(p => p.Name, p => p);
        }

        public string Name
        {
            get { return name; }
        }
        public string Form
        {
            get { return form; }
        }
        public double Ionicity
        {
            get { return coefs[0]; }
        }
        public double Tmelt
        {
            get { return T_melting; }
        }
        public double Tsuperionic
        {
            get { return T_superionic; }
        }
        public double[] Coefs
        {
            get { return coefs; }
        }
        public double[] CoefsDouble8
        {
            get
            {
                return new[] {
                    MDIBC.Ke * charge[0] * charge[0], Coefs[1], Coefs[2], Coefs[3], 0, 0, 0, 0, 
                    MDIBC.Ke * charge[0] * charge[1], Coefs[4], Coefs[5], 0, Coefs[6], Coefs[7], Coefs[8], 0,
                    MDIBC.Ke * charge[0] * charge[1], Coefs[4], Coefs[5], 0, Coefs[6], Coefs[7], Coefs[8], 0,
                    MDIBC.Ke * charge[1] * charge[1], Coefs[9], Coefs[10], 0, 0, 0, 0, 0,
                };
            }
        }

        public PairPotentials(string[] ion_types, double[] charge, XElement spp)
        {
            this.ion_types = new string[ion_types.Length]; ion_types.CopyTo(this.ion_types, 0);
            this.charge = new double[charge.Length]; charge.CopyTo(this.charge, 0);

            name = spp.AttributeOrEmpty("name");
            form = spp.AttributeOrEmpty("form");
            coefs = new double[12];
            coefs[0] = spp.ElementOrDefault("Ionicity").Double();
            T_melting = spp.ElementOrDefault("MeltingTemperature").Double();
            T_superionic = spp.ElementOrDefault("SuperionicTemperature").Double();
            var pairs = spp.Elements("Pair").ToDictionary(e => e.AttributeOrEmpty("ions"), e => e);
            string P00 = ion_types[0] + " " + ion_types[0];
            string P01 = ion_types[0] + " " + ion_types[1];
            string P10 = ion_types[1] + " " + ion_types[0];
            string P11 = ion_types[1] + " " + ion_types[1];
            XElement x = pairs.ContainsKey(P00) ? pairs[P00] : null;
            if (x != null)
            {
                var c = x.AttributeOrEmpty("BornMayer").ToDoubleArray();
                if (c.Length >= 2)
                {
                    coefs[1] = c[0];
                    coefs[2] = -c[1];
                }
                coefs[3] = x.Double("Dispersion");
            }
            x = pairs.ContainsKey(P01) ? pairs[P01] : (pairs.ContainsKey(P10) ? pairs[P10] : null);
            if (x != null)
            {
                var c = x.AttributeOrEmpty("BornMayer").ToDoubleArray();
                if (c.Length >= 2)
                {
                    coefs[4] = c[0];
                    coefs[5] = -c[1];
                }
                c = x.AttributeOrEmpty("Morse").ToDoubleArray();
                if (c.Length >= 3)
                {
                    coefs[6] = c[0];
                    coefs[7] = -c[1];
                    coefs[8] = c[2];
                }
            }
            x = pairs.ContainsKey(P11) ? pairs[P11] : null;
            if (x != null)
            {
                var c = x.AttributeOrEmpty("BornMayer").ToDoubleArray();
                if (c.Length >= 2)
                {
                    coefs[9] = c[0];
                    coefs[10] = -c[1];
                }
            }
            solid_period = new Polynom(spp.ElementOrDefault("SolidPeriod").Value);
            for (int i = 0; i < this.charge.Length; i++) this.charge[i] *= Ionicity;
        }

        public double SolidPeriod(double T)
        {
            return solid_period.Eval(T);
        }

        private string name, form;
        private string[] ion_types;
        private double[] coefs, charge;
        private double T_melting, T_superionic;
        private Polynom solid_period;
    }
}
