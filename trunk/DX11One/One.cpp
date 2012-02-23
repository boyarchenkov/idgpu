#include "stdafx.h"

long Device::ID = 1001001;
long Context::ID = 1001002;
long Shader::ID = 1001003;
long Buffer::ID = 1001004;
long Texture2D::ID = 1001005;

ID3D11Device* _device = NULL;
ID3D11DeviceContext* _context = NULL;
ID3D11ShaderResourceView** SRVs = NULL;
ID3D11UnorderedAccessView** UAVs = NULL;
ID3D11Buffer** CBs = NULL;
int SRV_count = -1, UAV_count = -1, CB_count = -1;

ID3D11ShaderResourceView** GetSRVs(int count, Buffer* buffers)
{
	if (SRV_count < count)
	{
		free(SRVs);
		SRVs = (ID3D11ShaderResourceView**)malloc(count * sizeof(ID3D11ShaderResourceView*));
		SRV_count = count;
	}
	if (buffers == NULL) { memset(SRVs, 0, count * sizeof(ID3D11ShaderResourceView*)); return SRVs; }
	for (int i = 0; i < count; i++)
	{
		if (buffers[i].id != buffers[i].ID || buffers[i].p_SRV == NULL) return NULL;
		SRVs[i] = buffers[i].p_SRV;
	}
	return SRVs;
}
ID3D11ShaderResourceView** GetSRVs(int r_count, Buffer* r_buffers, int t_count, Texture2D* textures)
{
	int count = r_count + t_count;
	if (SRV_count < count)
	{
		free(SRVs);
		SRVs = (ID3D11ShaderResourceView**)malloc(count * sizeof(ID3D11ShaderResourceView*));
		SRV_count = count;
	}
	if (r_buffers == NULL && textures == NULL) { memset(SRVs, 0, count * sizeof(ID3D11ShaderResourceView*)); return SRVs; }
	for (int i = 0; i < r_count; i++)
	{
		if (r_buffers[i].id != r_buffers[i].ID || r_buffers[i].p_SRV == NULL) return NULL;
		SRVs[i] = r_buffers[i].p_SRV;
	}
	for (int i = 0; i < t_count; i++)
	{
		if (textures[i].id != textures[i].ID || textures[i].p_SRV == NULL) return NULL;
		SRVs[r_count + i] = textures[i].p_SRV;
	}
	return SRVs;
}
ID3D11UnorderedAccessView** GetUAVs(int count, Buffer* buffers)
{
	if (UAV_count < count)
	{
		free(UAVs);
		UAVs = (ID3D11UnorderedAccessView**)malloc(count * sizeof(ID3D11UnorderedAccessView*));
		UAV_count = count;
	}
	if (buffers == NULL) { memset(UAVs, 0, count * sizeof(ID3D11UnorderedAccessView*)); return UAVs; }
	for (int i = 0; i < count; i++)
	{
		if (buffers[i].id != buffers[i].ID || buffers[i].p_UAV == NULL) return NULL;
		UAVs[i] = buffers[i].p_UAV;
	}
	return UAVs;
}
ID3D11Buffer** GetCBs(int count, Buffer* buffers)
{
	if (CB_count < count)
	{
		free(CBs);
		CBs = (ID3D11Buffer**)malloc(count * sizeof(ID3D11Buffer*));
		CB_count = count;
	}
	if (buffers == NULL) { memset(CBs, 0, count * sizeof(ID3D11Buffer*)); return CBs; }
	for (int i = 0; i < count; i++)
	{
		if (buffers[i].id != buffers[i].ID || buffers[i].p_buffer == NULL) return NULL;
		CBs[i] = buffers[i].p_buffer;
	}
	return CBs;
}

HRESULT DX11W_API CreateDevice(D3D_DRIVER_TYPE driver_type, D3D_FEATURE_LEVEL level_wanted, Device* device, Context* context)
{
	if (device == NULL || context == NULL) return E_FAIL;
	if (context->id == context->ID && context->ptr != NULL) { context->ptr->Release(); _context = NULL; }
	if (device->id == device->ID && device->ptr != NULL) { device->ptr->Release(); _device = NULL; }
	*device = Device(); *context = Context();
	D3D_FEATURE_LEVEL levels_wanted[] = { level_wanted }; int num_levels_wanted = 1;
	D3D_FEATURE_LEVEL FeatureLevel;

	// default adapter: IDXGIAdapter *pAdapter = NULL 
	// hardware driver: D3D_DRIVER_TYPE DriverType = D3D_DRIVER_TYPE_HARDWARE,
	// no software rasterizer: HMODULE Software = NULL,
	// no layers (layers add functionality, but do not modify existing behavior): UINT Flags = 0,
	// DX10 feature level: CONST D3D_FEATURE_LEVEL *pFeatureLevels = &FeatureLevel,
	// UINT FeatureLevels = 1,
	// UINT SDKVersion = D3D11_SDK_VERSION,
	// (out, optional) ID3D11Device **ppDevice,
	// (out, optional) D3D_FEATURE_LEVEL *pFeatureLevel,
	// (out, optional) ID3D11DeviceContext **ppImmediateContext
	HRESULT hr = D3D11CreateDevice(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL, 0, levels_wanted, num_levels_wanted, D3D11_SDK_VERSION, &(device->ptr), &FeatureLevel, &(context->ptr));
	if (FAILED(hr))
	{
		device->id = -1;
		context->id = -1;
	}
	else
	{
		_device = device->ptr;
		_context = context->ptr;
	}
	return hr;
}

HRESULT DX11W_API CreateAndCompileShader(Device device, LPCSTR filename, LPCSTR entry_point, LPCSTR shader_profile, unsigned int flags, Shader* shader)
{
	if (shader == NULL) return E_FAIL;
	if (shader->id == shader->ID) ReleaseShader(*shader);
	*shader = Shader();
	HRESULT hr = S_OK;
	size_t required_size; mbstowcs_s(&required_size, NULL, 0, filename, 256);
	if (device.id != device.ID || required_size < 0 || required_size > 256) { shader->id = -1; return E_FAIL; }
	wchar_t* w_filename = (wchar_t*)malloc((required_size + 2) * 2);
	mbstowcs_s(&required_size, w_filename, required_size + 2, filename, 256);
    ID3D10Blob* pErrorBlob = NULL;
	hr = D3DX11CompileFromFile(w_filename, NULL, NULL, entry_point, shader_profile, flags, NULL, NULL, &(shader->blob), &pErrorBlob, NULL);
	free(w_filename);
	if (!FAILED(hr)) hr = device.ptr->CreateComputeShader(shader->blob->GetBufferPointer(), shader->blob->GetBufferSize(), NULL, &(shader->ptr));
	if (FAILED(hr)) shader->id = -1; // no "else", intentionally
	return hr;
}

HRESULT DX11W_API CompileShader(Device device, LPCSTR shader_source, int shader_length, LPCSTR entry_point, LPCSTR shader_profile, unsigned int flags, Shader* shader, const char **errors)
{
	if (shader == NULL) return E_FAIL;
	if (shader->id == shader->ID) ReleaseShader(*shader);
	*shader = Shader();
	HRESULT hr = S_OK;
	if (device.id != device.ID) { shader->id = -1; return E_FAIL; }
    ID3D10Blob* pErrorBlob = NULL;
	hr = D3DCompile(shader_source, shader_length, shader_source, NULL, NULL, entry_point, shader_profile, flags, NULL, &(shader->blob), &pErrorBlob);
	if (!FAILED(hr)) hr = device.ptr->CreateComputeShader(shader->blob->GetBufferPointer(), shader->blob->GetBufferSize(), NULL, &(shader->ptr));
	if (FAILED(hr)) { shader->id = -1; *errors = (char*)(pErrorBlob->GetBufferPointer()); } // no "else", intentionally
	return hr;
}


HRESULT DX11W_API CreateRWBuffer(Device device, int element_size, int element_count, void *init_data, Buffer* buffer)
{
	if (buffer == NULL) return E_FAIL;
	if (buffer->id == buffer->ID) ReleaseBuffer(*buffer);
	*buffer = Buffer();
	HRESULT hr = S_OK;
	if (device.id != device.ID) { buffer->id = -1; return E_FAIL; }

	// Create Structured Buffer
	D3D11_BUFFER_DESC sbDesc;
	sbDesc.BindFlags			=	D3D11_BIND_UNORDERED_ACCESS | D3D11_BIND_SHADER_RESOURCE; // allow unordered access and access from the shader.
	sbDesc.Usage				=	D3D11_USAGE_DEFAULT; // it can be read/write by GPU, but need staging resource for CPU access. 
	sbDesc.CPUAccessFlags		=	0;
	sbDesc.MiscFlags			=	D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
	sbDesc.StructureByteStride	=	element_size;
	sbDesc.ByteWidth			=	element_size * element_count;
	if (init_data == NULL)
	{
		hr = device.ptr->CreateBuffer(&sbDesc, NULL, &(buffer->p_buffer));
	}
	else
	{
        D3D11_SUBRESOURCE_DATA InitData; InitData.pSysMem = init_data;
		hr = device.ptr->CreateBuffer(&sbDesc, &InitData, &(buffer->p_buffer));
	}

	// Create an Unordered Access View to the Structured Buffer
	D3D11_UNORDERED_ACCESS_VIEW_DESC sbUAVDesc;
	sbUAVDesc.Buffer.FirstElement		= 0;
	sbUAVDesc.Buffer.Flags				= 0;
	sbUAVDesc.Buffer.NumElements		= element_count;
	sbUAVDesc.Format					= DXGI_FORMAT_UNKNOWN;
	sbUAVDesc.ViewDimension				= D3D11_UAV_DIMENSION_BUFFER;
	if (!FAILED(hr)) hr = device.ptr->CreateUnorderedAccessView(buffer->p_buffer, &sbUAVDesc, &(buffer->p_UAV));

	// Create Shader Resource View for Structured Buffers
    D3D11_SHADER_RESOURCE_VIEW_DESC desc;
    ZeroMemory( &desc, sizeof(desc) );
    desc.ViewDimension = D3D11_SRV_DIMENSION_BUFFEREX;
    desc.BufferEx.FirstElement = 0;
	desc.Format = DXGI_FORMAT_UNKNOWN;
    desc.BufferEx.NumElements = element_count;
    if (!FAILED(hr)) hr = device.ptr->CreateShaderResourceView(buffer->p_buffer, &desc, &(buffer->p_SRV));

	if (FAILED(hr)) buffer->id = -1; // no "else", intentionally
	return hr;
}

HRESULT DX11W_API CreateRBuffer(Device device, int element_size, int element_count, void *init_data, Buffer* buffer)
{
	if (buffer == NULL) return E_FAIL;
	if (buffer->id == buffer->ID) ReleaseBuffer(*buffer);
	*buffer = Buffer();
	HRESULT hr = S_OK;
	if (device.id != device.ID) { buffer->id = -1; return E_FAIL; }

	// Create Structured Buffer
	D3D11_BUFFER_DESC sbDesc;
	sbDesc.BindFlags			=	D3D11_BIND_UNORDERED_ACCESS | D3D11_BIND_SHADER_RESOURCE; // allow unordered access and access from the shader.
	sbDesc.Usage				=	D3D11_USAGE_DEFAULT; // it can be read/write by GPU, but need staging resource for CPU access. 
	sbDesc.CPUAccessFlags		=	0;
	sbDesc.MiscFlags			=	D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
	sbDesc.StructureByteStride	=	element_size;
	sbDesc.ByteWidth			=	element_size * element_count;
	if (init_data == NULL)
	{
		hr = device.ptr->CreateBuffer(&sbDesc, NULL, &(buffer->p_buffer));
	}
	else
	{
        D3D11_SUBRESOURCE_DATA InitData; InitData.pSysMem = init_data;
		hr = device.ptr->CreateBuffer(&sbDesc, &InitData, &(buffer->p_buffer));
	}

	// Create Shader Resource View for Structured Buffers
    D3D11_SHADER_RESOURCE_VIEW_DESC desc;
    ZeroMemory( &desc, sizeof(desc) );
    desc.ViewDimension = D3D11_SRV_DIMENSION_BUFFEREX;
    desc.BufferEx.FirstElement = 0;
	desc.Format = DXGI_FORMAT_UNKNOWN;
    desc.BufferEx.NumElements = element_count;
    if (!FAILED(hr)) hr = device.ptr->CreateShaderResourceView(buffer->p_buffer, &desc, &(buffer->p_SRV));

	if (FAILED(hr)) buffer->id = -1; // no "else", intentionally
	return hr;
}

HRESULT DX11W_API CreateInputBuffer(Device device, int element_size, int element_count, Buffer* buffer)
{
	if (buffer == NULL) return E_FAIL;
	if (buffer->id == buffer->ID) ReleaseBuffer(*buffer);
	*buffer = Buffer();
	HRESULT hr = S_OK;
	if (device.id != device.ID) { buffer->id = -1; return E_FAIL; }

	// Create Structured Buffer
	D3D11_BUFFER_DESC sbDesc;
	sbDesc.BindFlags			=	D3D11_BIND_SHADER_RESOURCE; // allow unordered access and access from the shader.
	sbDesc.Usage				=	D3D11_USAGE_DYNAMIC; // it can be read/write by GPU, but need staging resource for CPU access. 
	sbDesc.CPUAccessFlags		=	D3D11_CPU_ACCESS_WRITE;
	sbDesc.MiscFlags			=	D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
	sbDesc.StructureByteStride	=	element_size;
	sbDesc.ByteWidth			=	element_size * element_count;

	hr = device.ptr->CreateBuffer(&sbDesc, NULL, &(buffer->p_buffer));
	if (FAILED(hr)) buffer->id = -1;
	buffer->p_UAV = NULL; buffer->p_SRV = NULL;
	return hr;
}

HRESULT DX11W_API CreateStagingBuffer(Device device, int element_size, int element_count, Buffer* buffer)
{
	if (buffer == NULL) return E_FAIL;
	if (buffer->id == buffer->ID) ReleaseBuffer(*buffer);
	*buffer = Buffer();
	if (device.id != device.ID) { buffer->id = -1; return E_FAIL; }

	// Create a "Staging" Resource to actually copy data to-from the GPU buffer. 
	D3D11_BUFFER_DESC stagingBufferDesc;
	stagingBufferDesc.BindFlags				= 0;
	stagingBufferDesc.Usage					= D3D11_USAGE_STAGING;
	stagingBufferDesc.CPUAccessFlags		= D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
	stagingBufferDesc.MiscFlags				= D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
	stagingBufferDesc.StructureByteStride	= element_size;
	stagingBufferDesc.ByteWidth				= element_size * element_count;

	HRESULT hr = device.ptr->CreateBuffer(&stagingBufferDesc, NULL, &(buffer->p_buffer));
	if (FAILED(hr)) buffer->id = -1;
	buffer->p_UAV = NULL; buffer->p_SRV = NULL;
	return hr;
}

HRESULT DX11W_API CreateConstantBuffer(Device device, int length, Buffer* buffer)
{
	if (buffer == NULL) return E_FAIL;
	if (buffer->id == buffer->ID) ReleaseBuffer(*buffer);
	*buffer = Buffer();
	if (device.id != device.ID) { buffer->id = -1; return E_FAIL; }
	D3D11_BUFFER_DESC cbDesc;
	cbDesc.BindFlags		=	D3D11_BIND_CONSTANT_BUFFER;
	cbDesc.Usage			=	D3D11_USAGE_DYNAMIC;
	cbDesc.CPUAccessFlags	=	D3D11_CPU_ACCESS_WRITE; // CPU writable, should be updated per frame
	cbDesc.MiscFlags		=	0;
	cbDesc.ByteWidth		=	length;

	HRESULT hr = device.ptr->CreateBuffer(&cbDesc, NULL, &(buffer->p_buffer));
	if (FAILED(hr)) buffer->id = -1;
	buffer->p_UAV = NULL; buffer->p_SRV = NULL;
	return hr;
}

HRESULT DX11W_API CreateInputTexture2D(Device device, int width, int height, DXGI_FORMAT format, void *init_data, Texture2D* t)
{
	if (t == NULL) return E_FAIL;
	if (t->id == t->ID) ReleaseTexture(*t);
	*t = Texture2D();
	if (device.id != device.ID || device.ptr == NULL) { t->id = -1; return E_FAIL; }
    D3D11_TEXTURE2D_DESC desc2D;
    //ZeroMemory( &desc2D, sizeof( D3D11_TEXTURE2D_DESC ) );
    desc2D.ArraySize = 1;
    desc2D.BindFlags = D3D11_BIND_SHADER_RESOURCE; // | D3D11_BIND_UNORDERED_ACCESS;
	desc2D.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    desc2D.Usage = D3D11_USAGE_DYNAMIC;
    desc2D.Format = format;
    desc2D.Width = width;
    desc2D.Height = height;
    desc2D.MipLevels = 1;  
	desc2D.MiscFlags = 0;
    desc2D.SampleDesc.Count = 1;
    desc2D.SampleDesc.Quality = 0;

	HRESULT hr = S_OK;
	if (init_data == NULL)
	{
		hr = device.ptr->CreateTexture2D( &desc2D, NULL, &(t->p_texture));
	}
	else
	{
        D3D11_SUBRESOURCE_DATA InitData; InitData.pSysMem = init_data;
		hr = device.ptr->CreateTexture2D(&desc2D, &InitData, &(t->p_texture));
	}
    // Create Fragment Count Resource View
    D3D11_SHADER_RESOURCE_VIEW_DESC descRV;
    ZeroMemory( &descRV, sizeof( D3D11_SHADER_RESOURCE_VIEW_DESC ) );
    descRV.Format = format;
    descRV.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    descRV.Texture2D.MipLevels = 1;
    descRV.Texture2D.MostDetailedMip = 0;
    if (!FAILED(hr)) hr = device.ptr->CreateShaderResourceView(t->p_texture, &descRV, &(t->p_SRV));

    //// Create Unordered Access Views
    //D3D11_UNORDERED_ACCESS_VIEW_DESC descUAV;
    //descUAV.Format = desc2D.Format;
    //descUAV.ViewDimension = D3D11_UAV_DIMENSION_TEXTURE2D;
    //descUAV.Texture2D.MipSlice = 0;
    //if (!FAILED(hr)) device.ptr->CreateUnorderedAccessView(t->p_texture, &descUAV, &(t->p_UAV));

    //// Clear buffer
    //static const float clearValue[1] = { 0.0f };
    //pD3DContext->ClearUnorderedAccessViewFloat(m_pFragmentCountUAV, clearValue);

	if (FAILED(hr)) t->id = -1;
	return hr;
}


HRESULT DX11W_API WriteToBuffer(Context context, Buffer destination, void* source, int length)
{
	if (context.id != context.ID || destination.id != destination.ID || source == NULL) return E_FAIL;

	D3D11_MAPPED_SUBRESOURCE mappedResource;
	HRESULT hr = context.ptr->Map(destination.p_buffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
	if (FAILED(hr)) return hr;
	memcpy(mappedResource.pData, source, length);
	context.ptr->Unmap(destination.p_buffer, 0);
	return S_OK;
}

HRESULT DX11W_API WriteToTexture2D(Context context, Texture2D texture, void* source, int width, int height, int element_size)
{
	if (context.id != context.ID || texture.id != texture.ID || source == NULL) return E_FAIL;

	D3D11_MAPPED_SUBRESOURCE mappedResource;
	HRESULT hr = context.ptr->Map(texture.p_texture, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
	if (FAILED(hr)) return hr;
	int pitch = width * element_size;
	if (height == 1 || pitch == mappedResource.RowPitch) memcpy(mappedResource.pData, source, pitch * height);
	else
	{
		void *ptr = mappedResource.pData;
		for (int i = 0; i < height; ++i)
		{
			memcpy(ptr, source, pitch);
			((char*&)ptr) += mappedResource.RowPitch;
			((char*&)source) += pitch;
		}
	}
	context.ptr->Unmap(texture.p_texture, 0);
	return S_OK;
}

HRESULT DX11W_API CopyBuffer(Context context, Buffer destination, Buffer source)
{
	if (context.id != context.ID || destination.id != destination.ID || source.id != source.ID) return E_FAIL;
	context.ptr->CopyResource(destination.p_buffer, source.p_buffer);
	return S_OK;
}

HRESULT DX11W_API SetRBuffers(Context context, Buffer *r_buffers, int count)
{
	if (context.id == context.ID)
	{
		if (count > 0) context.ptr->CSSetShaderResources(0, count, GetSRVs(count, r_buffers));
		return S_OK;
	}
	return E_FAIL;
}
HRESULT DX11W_API SetRBuffersAndTextures(Context context, Buffer *r_buffers, int r_count, Texture2D *textures, int t_count)
{
	if (context.id == context.ID)
	{
		int count = r_count + t_count;
		if (count > 0) context.ptr->CSSetShaderResources(0, count, GetSRVs(r_count, r_buffers, t_count, textures));
		return S_OK;
	}
	return E_FAIL;
}
HRESULT DX11W_API SetCBuffers(Context context, Buffer *c_buffers, int count)
{
	if (context.id == context.ID)
	{
		if (count > 0) context.ptr->CSSetConstantBuffers(0, count, GetCBs(count, c_buffers));
		return S_OK;
	}
	return E_FAIL;
}
HRESULT DX11W_API SetRWBuffers(Context context, Buffer *rw_buffers, int count)
{
	if (context.id == context.ID)
	{
		UINT init_counts = 0;
		if (count > 0) context.ptr->CSSetUnorderedAccessViews(0, count, GetUAVs(count, rw_buffers), &init_counts);
		return S_OK;
	}
	return E_FAIL;
}

HRESULT DX11W_API DispatchShader(Context context, Shader shader, int thread_group_x, int thread_group_y, int thread_group_z)
{
	if (context.id == context.ID && shader.id == shader.ID)
	{
		context.ptr->CSSetShader(shader.ptr, NULL, 0);
		context.ptr->Dispatch(thread_group_x, thread_group_y, thread_group_z);
		return S_OK;
	}
	return E_FAIL;
}

HRESULT DX11W_API GetResults(Context context, Buffer staging_buffer, Buffer buffer, void *destination, int length)
{
	if (context.id == context.ID && staging_buffer.id == staging_buffer.ID && buffer.id == buffer.ID && destination != NULL && length > 0)
	{
		context.ptr->CopyResource(staging_buffer.p_buffer, buffer.p_buffer);
		D3D11_MAPPED_SUBRESOURCE mappedResource;
		context.ptr->Map(staging_buffer.p_buffer, 0, D3D11_MAP_READ, 0, &mappedResource);
		memcpy(destination, mappedResource.pData, length);
		context.ptr->Unmap(staging_buffer.p_buffer, 0);
		return S_OK;
	}
	return E_FAIL;
}

HRESULT DX11W_API UnbindResources(Context context)
{
	if (context.id == context.ID)
	{
		UINT initCounts = 0;
		if (SRV_count > 0) context.ptr->CSSetShaderResources(0, SRV_count, GetSRVs(SRV_count, NULL));
		// After a dispatch, if using CS 4.x hardware, be sure to unbind it, since only a single UAV can be bound to a pipeline. (Set to NULL to unbind)
		if (UAV_count > 0) context.ptr->CSSetUnorderedAccessViews(0, UAV_count, GetUAVs(UAV_count, NULL), &initCounts);
		if (CB_count > 0) context.ptr->CSSetConstantBuffers(0, CB_count, GetCBs(CB_count, NULL));
		return S_OK;
	}
	return E_FAIL;
}

HRESULT DX11W_API ReleaseBuffer(Buffer b)
{
	if (b.p_SRV != NULL) b.p_SRV->Release();
	if (b.p_UAV != NULL) b.p_UAV->Release();
	if (b.p_buffer != NULL) b.p_buffer->Release();
	b.p_SRV = NULL; b.p_UAV = NULL; b.p_buffer = NULL; b.id = -1;
	return S_OK;
}

HRESULT DX11W_API ReleaseShader(Shader s)
{
	if (s.blob != NULL) s.blob->Release();
	if (s.ptr != NULL) s.ptr->Release();
	s.blob = NULL; s.ptr = NULL; s.id = -1;
	return S_OK;
}

HRESULT DX11W_API ReleaseTexture(Texture2D t)
{
	if (t.p_texture != NULL) t.p_texture->Release();
	if (t.p_UAV != NULL) t.p_UAV->Release();
	if (t.p_SRV != NULL) t.p_SRV->Release();
	t.p_texture = NULL; t.p_UAV = NULL; t.p_SRV = NULL; t.id = -1;
	return S_OK;
}

HRESULT DX11W_API Dispose()
{
	if (SRV_count > 0) free(SRVs); SRVs = NULL; SRV_count = -1;
	if (UAV_count > 0) free(UAVs); UAVs = NULL; UAV_count = -1;
	if (CB_count > 0) free(CBs); CBs = NULL; CB_count = -1;
	if (_context != NULL) _context->Release();
	if (_device != NULL) _device->Release();
	return S_OK;
}

const char* error_text[12] = {
	"Unknown error code.",
	"D3D11_ERROR_FILE_NOT_FOUND - The file was not found.",
	"D3D11_ERROR_TOO_MANY_UNIQUE_STATE_OBJECTS - There are too many unique instances of a particular type of state object.",
	"D3D11_ERROR_TOO_MANY_UNIQUE_VIEW_OBJECTS - There are too many unique instance of a particular type of view object.",
	"D3D11_ERROR_DEFERRED_CONTEXT_MAP_WITHOUT_INITIAL_DISCARD - The first call to ID3D11DeviceContext::Map after either ID3D11Device::CreateDeferredContext or ID3D11DeviceContext::FinishCommandList per Resource was not D3D11_MAP_WRITE_DISCARD.",
	"D3DERR_INVALIDCALL - The method call is invalid. For example, a method's parameter may not be a valid pointer.",
	"D3DERR_WASSTILLDRAWING - The previous blit operation that is transferring information to or from this surface is incomplete.",
	"E_FAIL - An undetermined error occurred.",
	"E_INVALIDARG - An invalid parameter was passed to the returning function.",
	"E_OUTOFMEMORY - Could not allocate sufficient memory to complete the call.",
	"S_FALSE - Alternate success value, indicating a successful but nonstandard completion (the precise meaning depends on context).",
	"S_OK - No error occurred."
};
void DX11W_API DecodeError(HRESULT hr, const char **output)
{
	switch (hr)
	{
		case D3D11_ERROR_FILE_NOT_FOUND:
			*output = error_text[1]; break;
		case D3D11_ERROR_TOO_MANY_UNIQUE_STATE_OBJECTS:
			*output = error_text[2]; break;
		case D3D11_ERROR_TOO_MANY_UNIQUE_VIEW_OBJECTS:
			*output = error_text[3]; break;
		case D3D11_ERROR_DEFERRED_CONTEXT_MAP_WITHOUT_INITIAL_DISCARD:
			*output = error_text[4]; break;
		case D3DERR_INVALIDCALL:
			*output = error_text[5]; break;
		case D3DERR_WASSTILLDRAWING:
			*output = error_text[6]; break;
		case E_FAIL:
			*output = error_text[7]; break;
		case E_INVALIDARG:
			*output = error_text[8]; break;
		case E_OUTOFMEMORY:
			*output = error_text[9]; break;
		case S_FALSE:
			*output = error_text[10]; break;
		case S_OK:
			*output = error_text[11]; break;
		default: *output = error_text[0]; break;
	}
}
