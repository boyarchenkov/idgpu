using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using M.Tools;

namespace IDGPU
{
    public class UnitCell
    {
        public static Dictionary<string, UnitCell> LoadUnitCellsFromFile(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            var doc = XDocument.Load(filename).Root;
            return doc.Elements("Cell").Select(cell => new UnitCell(cell)).ToDictionary(c => c.Name, c => c);
        }

        private UnitCell(XElement cell)
        {
            name = cell.AttributeOrEmpty("name");
            var ions = cell.Elements("Ion").ToArray();
            pos = new Double3[ions.Length];
            type = new int[ions.Length];
            types = 0;
            for (int i = 0; i < ions.Length; i++)
            {
                type[i] = ions[i].Int("type");
                types = Math.Max(types, type[i] + 1);
                var position = ions[i].AttributeOrEmpty("position").ToDoubleArray();
                pos[i] = new Double3(position[0], position[1], position[2]);
            }
        }

        public string Name
        {
            get { return name; }
        }
        public int Ions
        {
            get { return pos.Length; }
        }
        public int Types
        {
            get { return types; }
        }
        public Double3[] Pos
        {
            get { return pos; }
        }
        public int[] Type
        {
            get { return type; }
        }

        private string name;
        private int types;
        private Double3[] pos;
        private int[] type;
    }
}
