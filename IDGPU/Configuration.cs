using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IDGPU
{
    public class Configuration
    {
        public static Configuration[] LoadConfigurationsFromFile(string filename)
        {
            var configs = new List<Configuration>();
            string[] lines =
                File.ReadAllLines(filename)
                    .SelectMany(line => line.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                    .Select(line => line.Trim())
                    .ToArray();
            var c = new Configuration();

            foreach (string l in lines)
            {
                if (l == "run")
                {
                    configs.Add(c.Clone());
                    continue;
                }
                if (l.StartsWith("#")) continue; // Whole line is comment
                var key_value = l.Split(new[] {' '}, 2, StringSplitOptions.RemoveEmptyEntries);
                var values = new string[0];
                if (key_value.Length > 1)
                {
                    // Quoted value with whitespaces
                    if (key_value[1].Length > 2 && key_value[1].StartsWith("\"") && key_value[1].EndsWith("\""))
                    {
                        values = new[] { key_value[1].Substring(1, key_value[1].Length - 2) };
                    }
                    else
                    {
                        values = key_value[1].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                c.parameters[key_value[0]] = values;
            }
            return configs.ToArray();
        }

        public string this[string key]
        {
            get
            {
                var s = Get(key);
                return s.Length > 0 ? s[0] : String.Empty;
            }
        }
        public string[] Get(string key)
        {
            return parameters.ContainsKey(key) ? parameters[key] : new string[0];
        }
        public double Get_dt_in_fs()
        {
            double dt = MDIBC.dt;
            if (!parameters.ContainsKey("dt")) return dt;
            var values = parameters["dt"];
            if (values.Length > 0)
            {
                double.TryParse(values[0], out dt);
                if (values.Length > 1) switch (values[1])
                    {
                        case "ps": dt *= 1000.0; break;
                        case "s":
                        case "sec": dt *= 1e+15; break;
                    }
            }
            return dt;
        }
        public double GetTimeInFractionalSteps(string key)
        {
            double dt = Get_dt_in_fs(), value = 0;
            var values = Get(key);
            if (values.Length > 0)
            {
                double.TryParse(values[0], out value);
                if (values.Length > 1) switch (values[1])
                    {
                        case "fs": value /= dt; break;
                        case "ps": value *= 1e+3 / dt; break;
                        case "ns": value *= 1e+6 / dt; break;
                        case "us": value *= 1e+9 / dt; break;
                        case "ms": value *= 1e+12 / dt; break;
                        case "s":
                        case "sec": value *= 1e+15 / dt; break;
                    }
            }
            return value;
        }
        public int GetTimeInSteps(string key)
        {
            return (int)Math.Round(GetTimeInFractionalSteps(key));
        }
        public double GetTemperatureInKelvins(string key)
        {
            double T = 0;
            var values = Get(key);
            if (values.Length > 0)
            {
                double.TryParse(values[0], out T);
                if (values.Length > 1) switch (values[1])
                    {
                        case "C": T += 273.15; break;
                        case "F": T = (T + 459.67) * 5.0 / 9.0; break;
                    }
            }
            return T;
        }
        public bool ContainsKey(string key)
        {
            return parameters.ContainsKey(key);
        }

        private Configuration()
        {
            parameters = new Dictionary<string, string[]>();
        }
        private Configuration Clone()
        {
            var c = new Configuration();
            foreach (var key in parameters.Keys) c.parameters.Add(key, parameters[key]);
            return c;
        }

        private Dictionary<string, string[]> parameters;
    }
}
