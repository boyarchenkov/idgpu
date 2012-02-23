using System;
using M.Tools;
using System.Linq;

namespace IDGPU
{
    public class Crystal
    {
        public static Crystal CreateCube(UnitCell cell, int edge_cells)
        {
            int i, x, y, z, n = 0, ions = cell.Ions * edge_cells * edge_cells * edge_cells;

            var c = new Crystal { pos = new Double3[ions], type = new int[ions], cells = edge_cells };

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

        private Crystal() { }

        public Double3[] Scale(double lattice_period)
        {
            return pos.Select(p => p * lattice_period).ToArray();
        }

        private UnitCell c;
        private Double3[] pos;
        private int[] type;
        private int cells;
    }
}
