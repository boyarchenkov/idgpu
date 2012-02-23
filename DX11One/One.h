#ifndef _ONE_H_
#define _ONE_H_

#include <stdio.h>
#include <d3dx11.h>
#include <d3dCompiler.h>
#include <D3DX10Math.h>

#define SAFE(msg, cmd) { printf(msg); hr = (cmd); if ( FAILED(hr) ) { printf("FAIL\n"); return; } else printf("OK\n"); }

struct Device { static long ID; long id; ID3D11Device* ptr; Device() { id = Device::ID; ptr = NULL; } };
struct Context { static long ID; long id; ID3D11DeviceContext* ptr; Context() { id = Context::ID; ptr = NULL; } };
struct Shader { static long ID; long id; ID3D10Blob* blob; ID3D11ComputeShader* ptr; Shader() { id = Shader::ID; blob = NULL; ptr = NULL; } };
struct Buffer { static long ID; long id; ID3D11Buffer *p_buffer; ID3D11UnorderedAccessView *p_UAV; ID3D11ShaderResourceView *p_SRV; Buffer() { id = Buffer::ID; p_buffer = NULL; p_UAV = NULL; p_SRV = NULL; } };
struct Texture2D { static long ID; long id; ID3D11Texture2D *p_texture; ID3D11UnorderedAccessView *p_UAV; ID3D11ShaderResourceView *p_SRV; Texture2D() { id = Texture2D::ID; p_texture = NULL; p_UAV = NULL; p_SRV = NULL; } };

#define DX11W_API __declspec(dllexport) _stdcall
// External functions:
extern "C" HRESULT DX11W_API CreateDevice(D3D_DRIVER_TYPE driver_type, D3D_FEATURE_LEVEL level_wanted, Device* device, Context* context);
extern "C" HRESULT DX11W_API CreateAndCompileShader(Device device, LPCSTR filename, LPCSTR entry_point, LPCSTR shader_profile, unsigned int flags, Shader* shader);
extern "C" HRESULT DX11W_API CompileShader(Device device, LPCSTR shader_source, int shader_length, LPCSTR entry_point, LPCSTR shader_profile, unsigned int flags, Shader* shader, const char **errors);

extern "C" HRESULT DX11W_API CreateRWBuffer(Device device, int element_size, int element_count, void *init_data, Buffer* buffer);
extern "C" HRESULT DX11W_API CreateRBuffer(Device device, int element_size, int element_count, void *init_data, Buffer* buffer);
extern "C" HRESULT DX11W_API CreateInputBuffer(Device device, int element_size, int element_count, Buffer* buffer);
extern "C" HRESULT DX11W_API CreateStagingBuffer(Device device, int element_size, int element_count, Buffer* buffer);
extern "C" HRESULT DX11W_API CreateConstantBuffer(Device device, int length, Buffer* buffer);
extern "C" HRESULT DX11W_API CreateInputTexture2D(Device device, int width, int height, DXGI_FORMAT format, void *init_data, Texture2D* t);

extern "C" HRESULT DX11W_API WriteToBuffer(Context context, Buffer destination, void* source, int length);
extern "C" HRESULT DX11W_API WriteToTexture2D(Context context, Texture2D texture, void* source, int width, int height, int element_size);
extern "C" HRESULT DX11W_API CopyBuffer(Context context, Buffer destination, Buffer source);
extern "C" HRESULT DX11W_API GetResults(Context context, Buffer staging_buffer, Buffer buffer, void *destination, int length);

extern "C" HRESULT DX11W_API SetRBuffers(Context context, Buffer *r_buffers, int count);
extern "C" HRESULT DX11W_API SetRBuffersAndTextures(Context context, Buffer *r_buffers, int r_count, Texture2D *textures, int t_count);
extern "C" HRESULT DX11W_API SetCBuffers(Context context, Buffer *c_buffers, int count);
extern "C" HRESULT DX11W_API SetRWBuffers(Context context, Buffer *rw_buffers, int count);
extern "C" HRESULT DX11W_API DispatchShader(Context context, Shader shader, int thread_group_x, int thread_group_y, int thread_group_z);
extern "C" HRESULT DX11W_API UnbindResources(Context context);

extern "C" HRESULT DX11W_API ReleaseBuffer(Buffer b);
extern "C" HRESULT DX11W_API ReleaseShader(Shader b);
extern "C" HRESULT DX11W_API ReleaseTexture(Texture2D t);
extern "C" HRESULT DX11W_API Dispose();

extern "C" void DX11W_API DecodeError(HRESULT hr, const char **output);

// CPU/GPU communication:

// gD3DContext->CopyResource() copies between two resources.

// To copy between the CPU and GPU:
//  1) Create a CPU side "staging" resource.
//  2) The staging resource can be mapped with a map() call which returns a CPU pointer that you can use to copy data into or read data from the staging resource.
//  3) unmap() the staging resource, and perform a CopyResource() to or from the GPU resource. 

// Performance:
//  D3D11_USAGE_STAGING - system memory so they can be read/written directly by the GPU: CopyResource(), CopySubresourceRegion(), but can’t be accessed directly by a shader.
//  + D3D11_CPU_ACCESS_WRITE - good CPU->GPU performance; 
//  + D3D11_CPU_ACCESS_READ - CPU-cached with lower performance (but allows for readback)
//    READ takes precedence over WRITE if you use both. 
//  D3D11_USAGE_DYNAMIC (for Buffer resources only, not Texture*) for fast CPU->GPU memory transfers.
//   They can be used as copy src/dest, and can also be read as textures (“ShaderResourceViews” in D3D-speak) from shaders.
//   They can’t be written from a shader.
//   They’re versioned by the driver -- each time you Map() with the DISCARD flag the driver will return a new chunk of memory if the previous version is still in use by the GPU, rather than stalling until the GPU finishes.
//   They’re meant for streaming data to the GPU.

// shader parameters:
// uint3 threadIDInGroup : SV_GroupThreadID (ID within the group, in each dimension
// uint3 groupID : SV_GroupID, (ID of the group, in each dimension of the dispatch)
// uint groupIndex : SV_GroupIndex (flattened ID of the group in one dimension if you counted like a raster)
// uint3 dispatchThreadID : SV_DispatchThreadID (ID of the thread within the entire dispatch in each dimension) 

#endif