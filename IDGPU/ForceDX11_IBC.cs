using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using M.Tools;
using DirectCompute;

namespace IDGPU
{
    public class ForceDX11_IBC : IForce, IDisposable
    {
        public static string parameters_filename;
        public static string ParametersFilename
        {
            get { return parameters_filename; }
            set
            {
                parameters_filename = value;
                if (File.Exists(value))
                {
                    var lines = File.ReadAllLines(value);
                    var numbers =
                        lines.Select(
                            line =>
                            line.Split(new[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries).Take(3).Select(word => int.Parse(word)));
                    texture_size = new SortedDictionary<int, int[]>(numbers.ToDictionary(ii => ii.First(), ii => ii.Skip(1).ToArray()));
                }
            }
        }
        public static void OptimizeTiling(double timeout, Crystal c, Action<string> append_text)
        {
            float min_time = float.MaxValue;
            int min_iterations = 3, w = -1, h = -1, min_w = -1, min_h = -1, ions = c.Ions;
            Double3[] pos = c.Scale(5.5), acc = new Double3[ions];
            var wh = new int[2];
            var technique = new ForceDX11_IBC();

            string filename = "tiling " + ions + ".wht", s;
            if (File.Exists(filename)) File.Delete(filename);
            File.AppendAllText(filename, "width\theight\ttime\titerations\tFictitious particles\r\n");

            for (w = 4; w <= 256; w += 4)
            {
                h = (int)Math.Ceiling((double)ions / w);
                if (h > 4096) continue;
                wh[0] = w;
                wh[1] = h;
                texture_size[ions] = wh;
                int total_ions = technique.Init(c.Type, MainForm.potentials.First().Value, 2, ions);
                technique.SetPositions(pos, acc);

                // Test the next set of parameters
                int i;
                var times = new List<float>();
                var clock = new Clock();
                float started = clock.ElapsedTime, min_time_w = float.MaxValue;
                for (i = 0; i < min_iterations || clock.ElapsedTime - started < timeout; i++)
                {
                    float time = clock.ElapsedTime;
                    technique.Force();
                    times.Add(clock.ElapsedTime - time);
                    min_time_w = Math.Min(min_time_w, clock.ElapsedTime - time);
                }
                // Compare results
                if (min_time_w < min_time)
                {
                    min_w = w;
                    min_h = h;
                    min_time = min_time_w;
                }

                // Write results
                times.Sort();
                float median = times[times.Count / 2];
                s = String.Format("{0}\t{1}\t{2:F6}\t{3:F6}\t{4}\t{5}\r\n", w, h, min_time_w, median, i, total_ions - ions);
                append_text(s);
                File.AppendAllText(filename, s);

                technique.Dispose();
            }
            // Save optimal parameters
            wh[0] = w = min_w;
            wh[1] = h = min_h;
            texture_size[ions] = wh;
            s = String.Format("{0} {1} {2} // {3:F6}\r\n", ions, w, h, min_time);
            append_text(s);
            filename = Path.ChangeExtension(filename, "nwh");
            if (File.Exists(filename)) File.Delete(filename);
            File.WriteAllText(filename, s);
        }

        static ForceDX11_IBC()
        {
            ParametersFilename = "Data\\Radeon6970.nwh";
        }
        private static SortedDictionary<int, int[]> texture_size;
        private static void SetTiling(int ions, out int width, out int height, out int threads, out int bj)
        {
            threads = 64; bj = 10;
            width = 384;
            if (texture_size != null)
            {
                if (texture_size.ContainsKey(ions)) width = texture_size[ions][0];
                else width = texture_size.FirstOrDefault(pair => pair.Key < ions).Value[0]; // select parameters for the nearest number of ions
            }
            height = (int)Math.Ceiling((double)ions / width);
            while (width * height < ions) height++;
        }

        public string Name { get { return "GPU DX11 IBC"; } }

        public void Dispose()
        {
            ions = texels = bi = bj = cycles = threads = -1;
            if (!initialized) return; initialized = false;
            pos_gpu.Release();
            type_gpu.Release();
            coefs_gpu.Release();
            force_gpu.Release();
            staging_buffer.Release();
            constants_gpu.Release();
        }

        public void SetPositions(Double3[] pos, Double3[] acc) { this.pos = pos; this.acc = acc; }
        public unsafe int Init(int[] type, PairPotentials pp, int types, int ions_ext)
        {
            Dispose();
            device = KernelRepository.Device;

            this.ions = ions_ext;
            SetTiling(ions, out ww, out hh, out threads, out bj);
            this.texels = ww * hh;
            bi = (int)Math.Ceiling((double)texels / (threads * 4));
            cycles = (int)Math.Ceiling((double)hh / bj);

            string definitions = String.Format("#define n {0}{3}#define threads {1}{3}#define types {2}{3}", texels, threads, types, Environment.NewLine);
            string shader_filename = null;
            switch (pp.Form)
            {
                case "Buckingham": shader_filename = "Kernels\\IBC-B.hlsl"; break;
                case "BuckinghamMorse": shader_filename = "Kernels\\IBC-BM.hlsl"; break;
                case "Buckingham4": shader_filename = "Kernels\\IBC-B4.hlsl"; break;
                default: throw new NotImplementedException("Unknown potential form: " + pp.Form);
            }
            kernel_force_NxN = KernelRepository.Get(shader_filename, "ForceNxN", definitions);
            kernel_energy_NxN = KernelRepository.Get(shader_filename, "EnergyNxN", definitions);
            kernel_sum = KernelRepository.Get(shader_filename, "Sum", definitions);

            // Input textures
            buffer_in_pos = new Float4[texels]; buffer_in_type = new int[texels]; buffer_out = new float[4 * texels];
            pos_gpu = device.CreateInputTexture2D(ww, hh, ResourceFormat.R32G32B32A32_FLOAT, (void*)0);
            type_gpu = device.CreateInputTexture2D(ww, hh, ResourceFormat.R32_UINT, (void*)0);
            coefs_gpu = device.CreateInputTexture2D(2, 4, ResourceFormat.R32G32B32A32_FLOAT, (void*)0); // float4 * 8 = float8 * 4
            float[] coefs_float = Array.ConvertAll(pp.CoefsDouble8, a => (float)a);
            fixed (float* ptr = coefs_float) device.WriteToTexture(coefs_gpu, (void*)ptr, 2, 4, sizeof(float) * 4);

            // Output buffers
            force_gpu = device.CreateRWBuffer(16, texels * bj, (void*)0);
            staging_buffer = device.CreateStagingBuffer(16, texels * bj);
            uint[] tiling = new uint[] { (uint)cycles, (uint)bj, (uint)ww, (uint)hh };
            constants_gpu = device.CreateConstantBuffer(tiling.Length * 4);
            fixed (uint* ptr = tiling) device.WriteToBuffer(constants_gpu, (void*)ptr, tiling.Length * 4);

            initialized = true;

            // Fictitious particles - at the end of array
            for (int i = 0; i < texels - ions; i++)
            {
                buffer_in_pos[texels - i - 1] = new Float4(1e+10f);
                buffer_in_type[texels - i - 1] = 1;
            }
            for (int i = 0; i < ions; i++) buffer_in_type[i] = type[i];

            fixed (int* ptr = buffer_in_type) device.WriteToTexture(type_gpu, ptr, ww, hh, sizeof(int));
            return texels;
        }
        private unsafe void RunKernel(Kernel kernel_NxN)
        {
            for (int i = 0; i < ions; i++) buffer_in_pos[i] = new Float4(pos[i]);

            fixed (Float4* ptr = buffer_in_pos) device.WriteToTexture(pos_gpu, ptr, ww, hh, sizeof(Float4));

            kernel_NxN.SetRBuffers(null, pos_gpu, type_gpu, coefs_gpu);
            kernel_NxN.SetCBuffers(constants_gpu);
            kernel_NxN.SetRWBuffers(force_gpu);
            kernel_NxN.Run(bi, bj, 1);

            // Reduction of array force_gpu
            if (bj > 1) kernel_sum.Run(null, null, new DC_Buffer[] { force_gpu }, bi, 1, 1);

            device.GetResults(staging_buffer, force_gpu, buffer_out, sizeof(Float4) * texels);
            device.UnbindResources();
        }
        public void Force()
        {
            RunKernel(kernel_force_NxN);

            for (int i = 0; i < ions; i++)
            {
                acc[i].x = buffer_out[i * 4 + 0];
                acc[i].y = buffer_out[i * 4 + 1];
                acc[i].z = buffer_out[i * 4 + 2];
            }
        }
        public double Energy()
        {
            RunKernel(kernel_energy_NxN);

            double energy = 0;
            for (int i = 0; i < ions; i++)
            {
                acc[i].x = buffer_out[i * 4 + 0];
                acc[i].y = buffer_out[i * 4 + 1];
                acc[i].z = buffer_out[i * 4 + 2];
                energy += buffer_out[i * 4 + 3];
            }
            return energy * 0.5;
        }

        private bool initialized = false;
        private Device device;

        private Kernel kernel_force_NxN, kernel_energy_NxN, kernel_sum;
        private DC_Texture2D pos_gpu, type_gpu, coefs_gpu;
        private DC_Buffer force_gpu, staging_buffer, constants_gpu;

        private int ions, texels, ww, hh, bi, bj, cycles, threads;
        private int[] buffer_in_type;
        private Float4[] buffer_in_pos;
        private Double3[] pos, acc;
        private float[] buffer_out;
    }
}
