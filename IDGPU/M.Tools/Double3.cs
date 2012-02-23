using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using M.Tools;
using Float = System.Double;

namespace M.Tools
{
    [StructLayout(LayoutKind.Sequential)] //, Size = 32)] // 16 = 4xfloat, 32 = 4xdouble. must be equal to definition in c++ code!!!
    public struct Double3
    {
        public static Double3 Empty = new Double3();
        public static Double3 Cross(Double3 left, Double3 right)
        {
            return new Double3(
                left.y * right.z - left.z * right.y,
                left.z * right.x - left.x * right.z,
                left.x * right.y - left.y * right.x
                );
        }
        public static Float Dot(Double3 a, Double3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
        //public static Float3 TransformCoordinate(Float3 I, Microsoft.Xna.Framework.Matrix A)
        //{
        //    return new Float3(
        //        I.x * A.M11 + I.y * A.M21 + I.z * A.M31,
        //        I.x * A.M12 + I.y * A.M22 + I.z * A.M32,
        //        I.x * A.M13 + I.y * A.M23 + I.z * A.M33
        //        );
        //}
        public static Double3 TransformCoordinate(Double3 vector, Double3[] matrix) // in matrix: first Float3 = first column
        {
            return new Double3(Dot(vector, matrix[0]), Dot(vector, matrix[1]), Dot(vector, matrix[2]));
        }
        public static void Invert(Double3[] matrix) // 18 + 3 + 9 = 30 multiplications, 1 division
        {
            Double3[] inverse = new Double3[] { Double3.Empty, Double3.Empty, Double3.Empty };

            inverse[0].x = matrix[1].y * matrix[2].z - matrix[1].z * matrix[2].y;
            inverse[1].x = matrix[1].z * matrix[2].x - matrix[1].x * matrix[2].z;
            inverse[2].x = matrix[1].x * matrix[2].y - matrix[1].y * matrix[2].x;

            Float _det = 1 / (matrix[0].x * inverse[0].x + matrix[0].y * inverse[1].x + matrix[0].z * inverse[2].x);

            inverse[0].y = matrix[0].z * matrix[2].y - matrix[0].y * matrix[2].z;
            inverse[0].z = matrix[0].y * matrix[1].z - matrix[0].z * matrix[1].y;
            inverse[1].y = matrix[0].x * matrix[2].z - matrix[0].z * matrix[2].x;
            inverse[1].z = matrix[0].z * matrix[1].x - matrix[0].x * matrix[1].z;
            inverse[2].y = matrix[0].y * matrix[2].x - matrix[0].x * matrix[2].y;
            inverse[2].z = matrix[0].x * matrix[1].y - matrix[0].y * matrix[1].x;

            matrix[0] = inverse[0] * _det;
            matrix[1] = inverse[1] * _det;
            matrix[2] = inverse[2] * _det;
        }
        public static Double3 Parse(string s)
        {
            string[] components = s.Split(new char[] {' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return components.Length < 3 ? Double3.Empty : new Double3(Float.Parse(components[0]), Float.Parse(components[1]), Float.Parse(components[2]));
        }

        public Double3(double x, double y, double z)
        {
            this.x = (Float)x; this.y = (Float)y; this.z = (Float)z;
        }
        public Double3(float x, float y, float z)
        {
            this.x = (Float)x; this.y = (Float)y; this.z = (Float)z;
        }
        public Double3(Double3 v)
        {
            x = v.x; y = v.y; z = v.z;
        }
        //public Float3(Float4 v)
        //{
        //    x = v.x; y = v.y; z = v.z;
        //}

        public Float Distance(Double3 v)
        {
            return (Float)Math.Sqrt((x - v.x) * (x - v.x) + (y - v.y) * (y - v.y) + (z - v.z) * (z - v.z));
        }
        public Float Length()
        {
            return (Float)Math.Sqrt(x * x + y * y + z * z);
        }
        public Float LengthSq()
        {
            return x * x + y * y + z * z;
        }
        public Double3 Normalize()
        {
            Float r = (Float)Math.Sqrt(x * x + y * y + z * z);
            if (r > 0) { x /= r; y /= r; z /= r; }
            return this;
        }
        public Float NormInf()
        {
            Float xx = Math.Abs(x), yy = Math.Abs(y), zz = Math.Abs(z);
            return Math.Max(xx, Math.Max(yy, zz));
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return String.Format("{0} {1} {2}", x, y, z);
        }
        public string ToString(string format)
        {
            return String.Format("{0} {1} {2}",
                ((double)x).ToString(format).PadLeft(13),
                ((double)y).ToString(format).PadLeft(13),
                ((double)z).ToString(format).PadLeft(13));
        }

        public static bool operator ==(Double3 a, Double3 b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public static bool operator !=(Double3 a, Double3 b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z);
        }
        public static Double3 operator +(Double3 a, Double3 b)
        {
            return new Double3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Double3 operator +(Double3 a, Float b)
        {
            return new Double3(a.x + b, a.y + b, a.z + b);
        }
        public static Double3 operator -(Double3 a, Double3 b)
        {
            return new Double3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Double3 operator -(Double3 a, Float b)
        {
            return new Double3(a.x - b, a.y - b, a.z - b);
        }
        public static Double3 operator *(Double3 a, Float c)
        {
            return new Double3(a.x * c, a.y * c, a.z * c);
        }
        public static Double3 operator *(Float c, Double3 a)
        {
            return new Double3(a.x * c, a.y * c, a.z * c);
        }
        public static Double3 operator /(Double3 a, Float c)
        {
            return new Double3(a.x / c, a.y / c, a.z / c);
        }
        public static Double3 operator /(Double3 a, Double3 b)
        {
            return new Double3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public Float x, y, z;
    }
}
