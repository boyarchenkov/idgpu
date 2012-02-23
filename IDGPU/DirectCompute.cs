using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DriverType = DirectCompute.OneDLL.D3D_DRIVER_TYPE;
using FeatureLevel = DirectCompute.OneDLL.D3D_FEATURE_LEVEL;

namespace DirectCompute
{
    struct DC_Device
    {
        int class_id;
        IntPtr ptr;
        DC_Device(int something) { class_id = -1; ptr = IntPtr.Zero; }
    }
    struct DC_Context
    {
        int class_id;
        IntPtr ptr;
        DC_Context(int something) { class_id = -1; ptr = IntPtr.Zero; }
    }
    struct DC_Shader
    {
        int class_id;
        IntPtr blob, ptr;
        DC_Shader(int something) { class_id = -1; blob = ptr = IntPtr.Zero; }
        public void Release() { OneDLL.ReleaseShader(this); blob = ptr = IntPtr.Zero; }
    }
    public struct DC_Buffer
    {
        int id;
        IntPtr p_buffer, p_UAV, p_SRV;
        DC_Buffer(int something) { id = -1; p_buffer = p_UAV = p_SRV = IntPtr.Zero; }
        public void Release() { OneDLL.ReleaseBuffer(this); p_buffer = p_UAV = p_SRV = IntPtr.Zero; }
    }
    public struct DC_Texture2D
    {
        int id;
        IntPtr p_buffer, p_UAV, p_SRV;
        DC_Texture2D(int something) { id = -1; p_buffer = p_UAV = p_SRV = IntPtr.Zero; }
        public void Release() { OneDLL.ReleaseTexture(this); p_buffer = p_UAV = p_SRV = IntPtr.Zero; }
    }

    [Flags]
    public enum ShaderFlags
    {
        //   Insert debug file/line/type/symbol information.
        DEBUG = (1 << 0),

        //   Do not validate the generated code against known capabilities and
        //   constraints.  This option is only recommended when compiling shaders
        //   you KNOW will work.  (ie. have compiled before without this option.)
        //   Shaders are always validated by D3D before they are set to the device.
        SKIP_VALIDATION = (1 << 1),

        //   Instructs the compiler to skip optimization steps during code generation.
        //   Unless you are trying to isolate a problem in your code using this option 
        //   is not recommended.
        SKIP_OPTIMIZATION = (1 << 2),

        //   Unless explicitly specified, matrices will be packed in row-major order
        //   on input and output from the shader.
        PACK_MATRIX_ROW_MAJOR = (1 << 3),

        //   Unless explicitly specified, matrices will be packed in column-major 
        //   order on input and output from the shader.  This is generally more 
        //   efficient, since it allows vector-matrix multiplication to be performed
        //   using a series of dot-products.
        PACK_MATRIX_COLUMN_MAJOR = (1 << 4),

        //   Force all computations in resulting shader to occur at partial precision.
        //   This may result in faster evaluation of shaders on some hardware.
        PARTIAL_PRECISION = (1 << 5),

        //   Force compiler to compile against the next highest available software
        //   target for vertex shaders.  This flag also turns optimizations off, 
        //   and debugging on.  
        FORCE_VS_SOFTWARE_NO_OPT = (1 << 6),

        //   Force compiler to compile against the next highest available software
        //   target for pixel shaders.  This flag also turns optimizations off, 
        //   and debugging on.
        FORCE_PS_SOFTWARE_NO_OPT = (1 << 7),

        //   Disables Preshaders. Using this flag will cause the compiler to not 
        //   pull out static expression for evaluation on the host cpu
        NO_PRESHADER = (1 << 8),

        //   Hint compiler to avoid flow-control constructs where possible.
        AVOID_FLOW_CONTROL = (1 << 9),

        //   Hint compiler to prefer flow-control constructs where possible.
        PREFER_FLOW_CONTROL = (1 << 10),

        //   By default, the HLSL/Effect compilers are not strict on deprecated syntax.
        //   Specifying this flag enables the strict mode. Deprecated syntax may be
        //   removed in a future release, and enabling syntax is a good way to make sure
        //   your shaders comply to the latest spec.
        ENABLE_STRICTNESS = (1 << 11),

        //   This enables older shaders to compile to 4_0 targets.
        ENABLE_BACKWARDS_COMPATIBILITY = (1 << 12),

        IEEE_STRICTNESS = (1 << 13),
        WARNINGS_ARE_ERRORS = (1 << 18),

        // Optimization level flags
        OPTIMIZATION_LEVEL0 = (1 << 14),
        OPTIMIZATION_LEVEL1 = 0,
        OPTIMIZATION_LEVEL2 = ((1 << 14) | (1 << 15)),
        OPTIMIZATION_LEVEL3 = (1 << 15)
    }

    public enum ResourceFormat
    {
        UNKNOWN = 0,
        R32G32B32A32_TYPELESS = 1,
        R32G32B32A32_FLOAT = 2,
        R32G32B32A32_UINT = 3,
        R32G32B32A32_SINT = 4,
        R32G32B32_TYPELESS = 5,
        R32G32B32_FLOAT = 6,
        R32G32B32_UINT = 7,
        R32G32B32_SINT = 8,
        R32G32_TYPELESS = 15,
        R32G32_FLOAT = 16,
        R32G32_UINT = 17,
        R32G32_SINT = 18,
        R32_TYPELESS = 39,
        D32_FLOAT = 40,
        R32_FLOAT = 41,
        R32_UINT = 42,
        R32_SINT = 43
    }

    internal unsafe class OneDLL
    {
        public const string dll_filename = "DX11One.dll";

        internal enum D3D_DRIVER_TYPE
        {
            UNKNOWN = 0,
            HARDWARE = (UNKNOWN + 1),
            REFERENCE = (HARDWARE + 1),
            NULL = (REFERENCE + 1),
            SOFTWARE = (NULL + 1),
            WARP = (SOFTWARE + 1)
        }
        internal enum D3D_FEATURE_LEVEL
        {
            LEVEL_9_1 = 0x9100,
            LEVEL_9_2 = 0x9200,
            LEVEL_9_3 = 0x9300,
            LEVEL_10_0 = 0xa000,
            LEVEL_10_1 = 0xa100,
            LEVEL_11_0 = 0xb000
        }
        [DllImport(dll_filename, EntryPoint = "CreateDevice")]
        internal static extern int CreateDevice(D3D_DRIVER_TYPE driver_type, D3D_FEATURE_LEVEL level_wanted, DC_Device* device, DC_Context* context);
        [DllImport(dll_filename, EntryPoint = "CreateAndCompileShader")]
        internal static extern int CreateAndCompileShader(DC_Device device, string filename, string entry_point, string shader_profile, ShaderFlags flags, DC_Shader* shader);
        [DllImport(dll_filename, EntryPoint = "CompileShader")]
        internal static extern int CompileShader(DC_Device device, string filename, int shader_length, string entry_point, string shader_profile, ShaderFlags flags, DC_Shader* shader, out sbyte* errors);

        [DllImport(dll_filename, EntryPoint = "CreateRWBuffer")]
        internal static extern int CreateRWBuffer(DC_Device device, int element_size, int element_count, void* init_data, DC_Buffer* buffer);
        [DllImport(dll_filename, EntryPoint = "CreateRBuffer")]
        internal static extern int CreateRBuffer(DC_Device device, int element_size, int element_count, void* init_data, DC_Buffer* buffer);
        [DllImport(dll_filename, EntryPoint = "CreateInputBuffer")]
        internal static extern int CreateInputBuffer(DC_Device device, int element_size, int element_count, DC_Buffer* buffer);
        [DllImport(dll_filename, EntryPoint = "CreateStagingBuffer")]
        internal static extern int CreateStagingBuffer(DC_Device device, int element_size, int element_count, DC_Buffer* buffer);
        [DllImport(dll_filename, EntryPoint = "CreateConstantBuffer")]
        internal static extern int CreateConstantBuffer(DC_Device device, int length, DC_Buffer* buffer);
        [DllImport(dll_filename, EntryPoint = "CreateInputTexture2D")]
        internal static extern int CreateInputTexture2D(DC_Device device, int width, int height, ResourceFormat format, void* init_data, DC_Texture2D* buffer);

        [DllImport(dll_filename, EntryPoint = "WriteToBuffer")]
        internal static extern int WriteToBuffer(DC_Context context, DC_Buffer destination, void* source, int length);
        [DllImport(dll_filename, EntryPoint = "WriteToTexture2D")]
        internal static extern int WriteToTexture2D(DC_Context context, DC_Texture2D texture, void* source, int width, int height, int element_size);
        [DllImport(dll_filename, EntryPoint = "CopyBuffer")]
        internal static extern int CopyBuffer(DC_Context context, DC_Buffer destination, DC_Buffer source);
        [DllImport(dll_filename, EntryPoint = "GetResults")]
        internal static extern int GetResults(DC_Context context, DC_Buffer staging_buffer, DC_Buffer buffer, void* destination, int length);

        [DllImport(dll_filename, EntryPoint = "SetRBuffers")]
        internal static extern int SetRBuffers(DC_Context context, DC_Buffer[] r_buffers, int count);
        [DllImport(dll_filename, EntryPoint = "SetRBuffersAndTextures")]
        internal static extern int SetRBuffersAndTextures(DC_Context context, DC_Buffer[] r_buffers, int r_count, DC_Texture2D[] textures, int t_count);
        [DllImport(dll_filename, EntryPoint = "SetCBuffers")]
        internal static extern int SetCBuffers(DC_Context context, DC_Buffer[] c_buffers, int count);
        [DllImport(dll_filename, EntryPoint = "SetRWBuffers")]
        internal static extern int SetRWBuffers(DC_Context context, DC_Buffer[] rw_buffers, int count);
        [DllImport(dll_filename, EntryPoint = "DispatchShader")]
        internal static extern int DispatchShader(DC_Context context, DC_Shader shader, int thread_group_x, int thread_group_y, int thread_group_z);
        [DllImport(dll_filename, EntryPoint = "UnbindResources")]
        internal static extern int UnbindResources(DC_Context context);

        [DllImport(dll_filename, EntryPoint = "ReleaseBuffer")]
        internal static extern int ReleaseBuffer(DC_Buffer b);
        [DllImport(dll_filename, EntryPoint = "ReleaseTexture")]
        internal static extern int ReleaseTexture(DC_Texture2D b);
        [DllImport(dll_filename, EntryPoint = "ReleaseShader")]
        internal static extern int ReleaseShader(DC_Shader s);
        [DllImport(dll_filename, EntryPoint = "Dispose")]
        internal static extern int Dispose();

        [DllImport(dll_filename, EntryPoint = "DecodeError")]
        internal static extern int DecodeError(int hresult, out sbyte* output);

        internal static void Check(int hresult)
        {
            if (hresult < 0)
            {
                StackTrace trace = new StackTrace(true);
                StackFrame frame1 = trace.GetFrame(1);
                StackFrame frame2 = trace.GetFrame(2);
                sbyte* bytes;
                DecodeError(hresult, out bytes);
                string error_message = new string(bytes);
                throw new InvalidOperationException(String.Format("Error 0x{0:X8} in '{1}() -> {2}()' in {3}:line {4}\r\nmessage: {5}",
                    hresult, frame2.GetMethod().Name, frame1.GetMethod().Name, frame2.GetFileName(), frame2.GetFileLineNumber(), error_message));
            }
        }
    }
    public unsafe class Device
    {
        public Device(bool hardware)
        {
            DriverType h = DriverType.HARDWARE, s = DriverType.SOFTWARE;
            fixed (DC_Device* pd = &device)
            fixed (DC_Context* pc = &context)
                OneDLL.Check(OneDLL.CreateDevice(hardware ? h : s, FeatureLevel.LEVEL_11_0, pd, pc));
        }
        public void Dispose() { OneDLL.Dispose(); }

        public Kernel CreateAndCompileShader(string filename, string entry_point, string shader_profile, ShaderFlags flags)
        {
            DC_Shader shader;
            OneDLL.Check(OneDLL.CreateAndCompileShader(device, filename, entry_point, shader_profile, flags, &shader));
            return new Kernel(context, shader);
        }
        public Kernel CompileShader(string source, string entry_point, string shader_profile, ShaderFlags flags)
        {
            DC_Shader shader;
            sbyte* errors_bytes;
            int hresult = OneDLL.CompileShader(device, source, source.Length, entry_point, shader_profile, flags, &shader, out errors_bytes);
            string error_message = new string(errors_bytes);
            if (hresult < 0) System.Windows.Forms.MessageBox.Show(error_message);
            OneDLL.Check(hresult);
            return new Kernel(context, shader);
        }
        public DC_Buffer CreateRWBuffer(int element_size, int element_count, void* init_data)
        {
            DC_Buffer buffer;
            OneDLL.Check(OneDLL.CreateRWBuffer(device, element_size, element_count, init_data, &buffer));
            return buffer;
        }
        public DC_Buffer CreateRBuffer(int element_size, int element_count, void* init_data)
        {
            DC_Buffer buffer;
            OneDLL.Check(OneDLL.CreateRBuffer(device, element_size, element_count, init_data, &buffer));
            return buffer;
        }
        public DC_Buffer CreateInputBuffer(int element_size, int element_count)
        {
            DC_Buffer buffer;
            OneDLL.Check(OneDLL.CreateInputBuffer(device, element_size, element_count, &buffer));
            return buffer;
        }
        public DC_Buffer CreateStagingBuffer(int element_size, int element_count)
        {
            DC_Buffer buffer;
            OneDLL.Check(OneDLL.CreateStagingBuffer(device, element_size, element_count, &buffer));
            return buffer;
        }
        public DC_Buffer CreateConstantBuffer(int length_in_bytes)
        {
            DC_Buffer buffer;
            OneDLL.Check(OneDLL.CreateConstantBuffer(device, length_in_bytes, &buffer));
            return buffer;
        }
        public DC_Texture2D CreateInputTexture2D(int w, int h, ResourceFormat format, void* init_data)
        {
            DC_Texture2D texture;
            if (init_data != null) throw new NotImplementedException();
            OneDLL.Check(OneDLL.CreateInputTexture2D(device, w, h, format, init_data, &texture));
            return texture;
        }


        public void WriteToBuffer(DC_Buffer destination, void* source, int length_in_bytes)
        {
            OneDLL.Check(OneDLL.WriteToBuffer(context, destination, source, length_in_bytes));
        }
        public void WriteToTexture(DC_Texture2D destination, void* source, int w, int h, int element_size)
        {
            OneDLL.Check(OneDLL.WriteToTexture2D(context, destination, source, w, h, element_size));
        }
        public void CopyBuffer(DC_Buffer destination, DC_Buffer source)
        {
            OneDLL.Check(OneDLL.CopyBuffer(context, destination, source));
        }
        public void GetResults(DC_Buffer staging_buffer, DC_Buffer buffer, float[] destination, int length)
        {
            fixed (float* ptr = destination)
                OneDLL.Check(OneDLL.GetResults(context, staging_buffer, buffer, (void *)ptr, length));
        }
        public void GetResults(DC_Buffer staging_buffer, DC_Buffer buffer, void* destination, int length)
        {
            OneDLL.Check(OneDLL.GetResults(context, staging_buffer, buffer, destination, length));
        }
        public void ReleaseBuffer(DC_Buffer b) { b.Release(); }
        public void UnbindResources()
        {
            OneDLL.UnbindResources(context);
        }

        DC_Device device;
        DC_Context context;
    }
    public class Kernel
    {
        internal Kernel(DC_Context context, DC_Shader shader)
        {
            this.context = context;
            this.shader = shader;
        }
        public void Dispose() { shader.Release(); }

        public void SetRBuffers(params DC_Buffer[] buffers)
        {
            OneDLL.Check(OneDLL.SetRBuffers(context, buffers, buffers == null ? 0 : buffers.Length));
        }
        public void SetRBuffers(DC_Buffer[] buffers, params DC_Texture2D[] textures)
        {
            OneDLL.Check(OneDLL.SetRBuffersAndTextures(context, buffers, buffers == null ? 0 : buffers.Length, textures, textures == null ? 0 : textures.Length));
        }
        public void SetCBuffers(params DC_Buffer[] buffers)
        {
            OneDLL.Check(OneDLL.SetCBuffers(context, buffers, buffers == null ? 0 : buffers.Length));
        }
        public void SetRWBuffers(params DC_Buffer[] buffers)
        {
            OneDLL.Check(OneDLL.SetRWBuffers(context, buffers, buffers == null ? 0 : buffers.Length));
        }

        public void Run(int thread_group_x, int thread_group_y, int thread_group_z)
        {
            OneDLL.Check(OneDLL.DispatchShader(context, shader, thread_group_x, thread_group_y, thread_group_z));
        }
        public void Run(DC_Buffer[] r_buffers, DC_Buffer[] c_buffers, DC_Buffer[] rw_buffers, int thread_group_x, int thread_group_y, int thread_group_z)
        {
            OneDLL.Check(OneDLL.SetRBuffers(context, r_buffers, r_buffers == null ? 0 : r_buffers.Length));
            OneDLL.Check(OneDLL.SetCBuffers(context, c_buffers, c_buffers == null ? 0 : c_buffers.Length));
            OneDLL.Check(OneDLL.SetRWBuffers(context, rw_buffers, rw_buffers == null ? 0 : rw_buffers.Length));
            OneDLL.Check(OneDLL.DispatchShader(context, shader,
                thread_group_x, thread_group_y, thread_group_z));
        }

        DC_Context context;
        DC_Shader shader;
    }
    public static class KernelRepository
    {
        public class RecompilableKernel : IDisposable
        {
            public RecompilableKernel(string filename)
            {
                source = System.IO.File.ReadAllText(filename);
                kernels = new Dictionary<string, Kernel>();
            }
            public void Dispose()
            {
                foreach (Kernel s in kernels.Values) s.Dispose();
                kernels.Clear();
            }
            public Kernel Get(string entry_point, string parameters)
            {
                Kernel s = kernels.ContainsKey(entry_point) ? kernels[entry_point] : null;
                if (s != null && parameters == this.parameters) return s;
                if (s != null) s.Dispose();
                kernels[entry_point] = s = device.CompileShader(parameters + source, entry_point, "cs_5_0", flags);
                this.parameters = parameters;
                return s;
            }

            public string source, parameters;
            private Dictionary<string, Kernel> kernels;
        }

        public static ShaderFlags flags = ShaderFlags.OPTIMIZATION_LEVEL1; // | ShaderFlags.ENABLE_STRICTNESS;
        public static Device Device
        {
            get { return device ?? (device = new Device(true)); }
        }
        public static void Dispose()
        {
            foreach (RecompilableKernel s in kernels.Values) s.Dispose();
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }
        public static Kernel Get(string filename, string entry_point, string parameters)
        {
            if (kernels.ContainsKey(filename)) return kernels[filename].Get(entry_point, parameters);
            RecompilableKernel s = new RecompilableKernel(filename);
            kernels.Add(filename, s);
            return s.Get(entry_point, parameters);
        }

        static Device device = null;
        static Dictionary<string, RecompilableKernel> kernels = new Dictionary<string, RecompilableKernel>();
    }
}
