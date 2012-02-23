using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace IDGPU
{
    public static class Extensions
    {
        public static string AttributeOrEmpty(this XElement e, string name)
        {
            var a = e.Attribute(name);
            return a == null ? String.Empty : a.Value;
        }
        public static XElement ElementOrDefault(this XElement e, string name)
        {
            var ns = e.Document.Root.GetDefaultNamespace();
            return e.Element(ns + name) ?? new XElement("default");
        }
        public static int ToInt(this string s)
        {
            int i = 0;
            int.TryParse(s, out i);
            return i;
        }
        public static int Int(this XElement e)
        {
            return e.Value.ToInt();
        }
        public static int Int(this XElement e, string attribute)
        {
            var a = e.Attribute(attribute);
            return a == null ? 0 : a.Value.ToInt();
        }
        public static double ToDouble(this string s)
        {
            double d = 0;
            double.TryParse(s, out d);
            return d;
        }
        public static double[] ToDoubleArray(this string s)
        {
            return s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToDouble()).ToArray();
        }
        public static double Double(this XElement e)
        {
            return e.Value.ToDouble();
        }
        public static double Double(this XElement e, string attribute)
        {
            var a = e.Attribute(attribute);
            return a == null ? 0 : a.Value.ToDouble();
        }
    }
}
