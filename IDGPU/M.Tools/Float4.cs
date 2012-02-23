using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using M.Tools;
using Float = System.Single;

namespace M.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Float4
    {
        public static Float4 Empty = new Float4();
        public static Float4 Cross(Float4 left, Float4 right)
        {
            return new Float4(
                left.y * right.z - left.z * right.y,
                left.z * right.x - left.x * right.z,
                left.x * right.y - left.y * right.x
                );
        }
        public static Float Dot(Float4 a, Float4 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
        public static Float Dot(Float3 a, Float4 b)
        {
            return (Float)(a.x * b.x + a.y * b.y + a.z * b.z);
        }
        public static Float4 TransformCoordinate(Float4 vector, Float4[] matrix) // in matrix: first Float4 = first column
        {
            return new Float4(Dot(vector, matrix[0]), Dot(vector, matrix[1]), Dot(vector, matrix[2]));
        }
        public static void Invert(Float4[] matrix) // 18 + 3 + 9 = 30 multiplications, 1 division
        {
            Float4[] inverse = new Float4[] { Float4.Empty, Float4.Empty, Float4.Empty };

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
        public static Float4 Parse(string s)
        {
            string[] components = s.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return components.Length < 3 ? Float4.Empty : new Float4(Float.Parse(components[0]), Float.Parse(components[1]), Float.Parse(components[2]));
        }

        public Float4(double d)
        {
            x = y = z = (Float)d;
            w = 0;
        }
        public Float4(Float x, Float y, Float z)
        {
            this.x = x; this.y = y; this.z = z; this.w = 0;
        }
        public Float4(Float x, Float y, Float z, Float w)
        {
            this.x = x; this.y = y; this.z = z; this.w = w;
        }
        public Float4(Float4 v)
        {
            x = v.x; y = v.y; z = v.z; w = v.w;
        }
        public Float4(Float3 v)
        {
            x = (Float)v.x; y = (Float)v.y; z = (Float)v.z; w = 0;
        }
        public Float4(Double3 v)
        {
            x = (Float)v.x; y = (Float)v.y; z = (Float)v.z; w = 0;
        }
        public Float4(Float3 v, Float w)
        {
            x = (Float)v.x; y = (Float)v.y; z = (Float)v.z; this.w = w;
        }

        public Float Distance(Float4 v)
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
        public Float4 Normalize()
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
            return String.Format("{0} {1} {2} {3}", x, y, z, w);
        }
        public string ToString(string format)
        {
            return String.Format("{0} {1} {2} {3}",
                ((Float)x).ToString(format).PadLeft(13),
                ((Float)y).ToString(format).PadLeft(13),
                ((Float)z).ToString(format).PadLeft(13),
                ((Float)w).ToString(format).PadLeft(13));
        }

        public static bool operator ==(Float4 a, Float4 b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public static bool operator !=(Float4 a, Float4 b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z);
        }
        public static Float4 operator +(Float4 a, Float4 b)
        {
            return new Float4(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Float4 operator +(Float4 a, Float3 b)
        {
            return new Float4(a.x + (Float)b.x, a.y + (Float)b.y, a.z + (Float)b.z);
        }
        public static Float4 operator +(Float4 a, Float b)
        {
            return new Float4(a.x + b, a.y + b, a.z + b);
        }
        public static Float4 operator -(Float4 a, Float4 b)
        {
            return new Float4(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Float4 operator -(Float4 a, Float b)
        {
            return new Float4(a.x - b, a.y - b, a.z - b);
        }
        public static Float4 operator *(Float4 a, Float c)
        {
            return new Float4(a.x * c, a.y * c, a.z * c);
        }
        public static Float4 operator *(Float c, Float4 a)
        {
            return new Float4(a.x * c, a.y * c, a.z * c);
        }
        //public static Float3 operator *(Float4 a, double c)
        //{
        //    return new Float3(a.x * c, a.y * c, a.z * c);
        //}
        public static Float4 operator /(Float4 a, Float c)
        {
            return new Float4(a.x / c, a.y / c, a.z / c);
        }
        public static Float4 operator /(Float4 a, Float4 b)
        {
            return new Float4(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public Float x, y, z, w;
    }
}
