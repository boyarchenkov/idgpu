using System;
using M.Tools;
using System.Linq;
using System.Collections.Generic;

namespace IDGPU
{
    public class Crystal
    {
        private class Ion
        {
            public Ion(Double3 pos, int type) { this.pos = pos; this.type = type; r = (Math.Abs(pos.x) + Math.Abs(pos.y) + Math.Abs(pos.z)) / 3; } // Octahedron

            public Double3 pos;
            public double r;
            public int type;
        }

        public static Crystal Create(UnitCell cell, string[] parameters)
        {
            if (parameters.Length < 2) throw new ArgumentException("Undefined crystal size");
            return Create(cell, parameters[0], parameters[1].ToInt());
        }
        public static Crystal Create(UnitCell cell, string type, int edge_cells)
        {
            switch (type)
            {
                case "cube":
                    return CreateCube(cell, edge_cells);
                case "octa":
                    return CreateOctahedron(cell, edge_cells);
                default:
                    throw new ArgumentOutOfRangeException("Unknown crystal type: " + type);
            }
        }
        public static Crystal CreateCube(UnitCell cell, int edge_cells)
        {
            int i, x, y, z, n = 0, N = cell.Ions * edge_cells * edge_cells * edge_cells;

            var c = new Crystal { cell = cell, pos = new Double3[N], type = new int[N], cells = edge_cells };

            for (x = 0; x < edge_cells; x++)
                for (y = 0; y < edge_cells; y++)
                    for (z = 0; z < edge_cells; z++)
                        for (i = 0; i < cell.Pos.Length; i++)
                        {
                            c.pos[n] = new Double3(x, y, z) - 0.5 * edge_cells + cell.Pos[i];
                            c.type[n] = cell.Type[i];
                            n++;
                        }
            return c;
        }
        public static Crystal CreateOctahedron(UnitCell cell, int edge_cells)
        {
            int i, x, y, z, n = 0, N = (int)(12 * (((2 * edge_cells - 3) * edge_cells + 4) * 2.0 / 3 * edge_cells - 1));
            var c = new Crystal { cell = cell, pos = new Double3[N], type = new int[N], cells = edge_cells };

            for (x = -edge_cells; x <= edge_cells; x++)
                for (y = -edge_cells; y <= edge_cells; y++)
                    for (z = -edge_cells; z <= edge_cells; z++)
                        if (Math.Abs(x) + Math.Abs(y) + Math.Abs(z) < edge_cells)
                        {
                            for (i = 0; i < cell.Pos.Length; i++)
                            {
                                c.pos[n] = new Double3(x, y, z) - 0.5 + cell.Pos[i];
                                c.type[n] = cell.Type[i];
                                n++;
                            }
                        }
            return c;
        }
        public static Crystal CreateOctahedronFluorite(UnitCell cell, int edge_cells) // Only for diatomic compounds, where count(anions) = 2 * count(cations)
        {
            int i, N = cell.Ions * edge_cells * edge_cells * edge_cells;
            var ions = new List<Ion>();
            var c = CreateCube(cell, (int) Math.Ceiling(edge_cells * Math.Sqrt(3)));
            var random = new Random(0);

            for (i = 0; i < c.Ions; i++) ions.Add(new Ion(c.Pos[i], c.Type[i]));

            for (i = 0; i < ions.Count; i++) // Random reorder
            {
                int n = random.Next(ions.Count);
                var temp = ions[i]; ions[i] = ions[n]; ions[n] = temp;
            }
            ions.Sort((a, b) => a.r.CompareTo(b.r));

            int o, u; o = u = ions.Count - 1;
            do
            {
                // Remove UO2 molecule
                while (ions[u].type != 1) u--; ions.RemoveAt(u); if (o > --u) o--;
                while (ions[o].type != 0) o--; ions.RemoveAt(o); if (u > --o) u--;
                while (ions[o].type != 0) o--; ions.RemoveAt(o); if (u > --o) u--;
            } while (ions.Count > N);

            c = new Crystal { cell = cell, pos = new Double3[N], type = new int[N], cells = edge_cells };
            for (i = 0; i < N; i++)
            {
                c.pos[i] = ions[i].pos;
                c.type[i] = ions[i].type;
            }
            return c;
        }

        public int Ions
        {
            get { return pos.Length; }
        }
        public int EdgeCells
        {
            get { return cells; }
        }
        public int[] Type
        {
            get { return type; }
        }
        public Double3[] Pos
        {
            get { return pos; }
        }
        public UnitCell Cell
        {
            get { return cell; }
        }

        private Crystal() { }

        public Double3[] Scale(double lattice_period)
        {
            return pos.Select(p => p * lattice_period).ToArray();
        }

        private UnitCell cell;
        private Double3[] pos;
        private int[] type;
        private int cells;
    }
}
