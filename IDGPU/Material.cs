using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IDGPU
{
    public class Material
    {
        public static Dictionary<string, Material> LoadMaterialsFromFile(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            var doc = XDocument.Load(filename).Root;
            return doc.Elements("Material").Select(material => new Material(material)).ToDictionary(m => m.Formula, m => m);
        }

        private Material(XElement m)
        {
            name = m.AttributeOrEmpty("name");
            formula = m.AttributeOrEmpty("formula");
            unit_cell = m.AttributeOrEmpty("unit-cell");
            var ions = m.Elements("Ion").ToArray();
            ion_name = new string[ions.Length];
            ion_mass = new double[ions.Length];
            ion_charge = new double[ions.Length];
            for (int i = 0; i < ions.Length; i++)
            {
                ion_name[i] = ions[i].AttributeOrEmpty("name");
                ion_mass[i] = ions[i].Double("mass");
                ion_charge[i] = ions[i].Double("charge");
            }
            T_melting = m.ElementOrDefault("MeltingTemperature").Value.ToDouble();
            T_superionic = m.ElementOrDefault("SuperionicTemperature").Value.ToDouble();
            solid_period = new Polynom(m.ElementOrDefault("SolidPeriod").Value);
        }

        public string Name
        {
            get { return name; }
        }
        public string Formula
        {
            get { return formula; }
        }
        public string UnitCell
        {
            get { return unit_cell; }
        }
        public string[] IonName
        {
            get { return ion_name; }
        }
        public double[] IonMass
        {
            get { return ion_mass; }
        }
        public double[] IonCharge
        {
            get { return ion_charge; }
        }
        public double Tmelt
        {
            get { return T_melting; }
        }
        public double Tsuperionic
        {
            get { return T_superionic; }
        }

        public double SolidPeriod(double T)
        {
            return solid_period.Eval(T);
        }

        private string name, formula, unit_cell;
        private string[] ion_name;
        private double[] ion_mass, ion_charge;
        private double T_melting, T_superionic;
        private Polynom solid_period;
    }
}
