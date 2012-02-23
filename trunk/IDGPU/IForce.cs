using System;
using M.Tools;

namespace IDGPU
{
    public unsafe interface IForce
    {
        string Name { get; }
        int Init(int[] type, PairPotentials pp, int types, int ions);
        void SetPositions(Double3[] pos, Double3[] acc);
        void Force();
        double Energy();
        void Dispose();
    }
}
