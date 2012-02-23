using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Globalization;

namespace M.Tools
{
	public class Utility
	{
		/// <summary>
		/// Set some System.Globalization parameters (number decimal separator -> point)
		/// </summary>
		static Utility()
		{
			SetDecimalSeparator();
		}

		public static void SetDecimalSeparator()
		{
			int i = Thread.CurrentThread.CurrentCulture.LCID;
			CultureInfo ci = new CultureInfo(i);
			ci.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = ci;
		}

		/// <summary>
		/// Get the resource stream containing the embedded resource
		/// </summary>
		/// <param name="t">Type from the assembly, from which to take resources</param>
		/// <param name="resource">The name of the requested resource</param>
		/// <returns></returns>
		public static Stream GetResource(Type t, string name)
		{
			return Assembly.GetAssembly(t).GetManifestResourceStream(name);
		}
		/// <summary>
		/// Get the resource stream containing the embedded resource from the calling assembly
		/// </summary>
		/// <param name="name">The name of the requested resource</param>
		/// <returns></returns>
		public static Stream GetResource(string name)
		{
			return Assembly.GetCallingAssembly().GetManifestResourceStream(name);
		}

        /// <remarks>
		/// If cannot find file with name 'name', tries to get resource from the calling assembly
        /// </remarks>
		public static Stream GetFileOrResource(string name, string nameSpace)
		{
			try
			{
                string s = nameSpace == null ? name : nameSpace + "." + name;
				if (!File.Exists(name))
					return Assembly.GetCallingAssembly().GetManifestResourceStream(s);
				return new StreamReader(name).BaseStream;
			}
			catch
			{
				return null;
			}
		}
        /// <remarks>
		/// If cannot find file with name 'name', tries to get resource
		/// from the assembly of the given type.
        /// </remarks>
        public static Stream GetFileOrResource(string name, Type t)
		{
			try
			{
                string[] res = Assembly.GetCallingAssembly().GetManifestResourceNames();
                if (!File.Exists(name))
					return Assembly.GetCallingAssembly().GetManifestResourceStream(t, name);
				return new StreamReader(name).BaseStream;
			}
			catch
			{
				return null;
			}
		}

		public static List<string> Split(string s, params char[] separator)
		{
            List<string> components = new List<string>(s.Split(separator));
            for (int i = 0; i < components.Count; i++)
                if (components[i].Length == 0) components.RemoveAt(i--);
            return components;
		}

        public static string Join(ICollection objects, char separator)
        {
            if (objects.Count == 0) return String.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (object o in objects)
            {
                sb.Append(o.ToString());
                sb.Append(separator);
            }
            if (sb.Length > 1) sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static string Join(ICollection objects, string separator)
        {
            if (objects.Count == 0) return String.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (object o in objects)
            {
                sb.Append(o.ToString());
                sb.Append(separator);
            }
            if (sb.Length > separator.Length) sb.Remove(sb.Length - separator.Length, 1);
            return sb.ToString();
        }

        public static string LastModified(string path, string pattern)
        {
            string file = String.Empty;
            DateTime date = DateTime.MinValue, cdate;
            foreach (string f in Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly))
            {
                cdate = File.GetCreationTime(f);
                if (cdate.CompareTo(date) > 0)
                {
                    file = f;
                    date = cdate;
                }
            }
            return file;
        }

        public static float[] ToFloat(double[] a)
        {
            float[] result = new float[a.Length];
            for (int i = 0; i < a.Length; i++) result[i] = (float)a[i];
            return result;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a; a = b; b = temp;
        }

        public static T[] Subset<T>(T[] set, params int[] indices)
        {
            T[] result = new T[indices.Length];
            for (int i = 0; i < indices.Length; i++) result[i] = set[indices[i]];
            return result;
        }
    }

}
