// #define threads 256

#define FF { force_i0 += force_ij(pos_i0, pos[uint2(x, y)], CC1, CC2, k); force_i1 += force_ij(pos_i1, pos[uint2(x, y)], CC1, CC2, k); force_i2 += force_ij(pos_i2, pos[uint2(x, y)], CC1, CC2, k); force_i3 += force_ij(pos_i3, pos[uint2(x, y)], CC1, CC2, k); x++; }
#define EE { force_i0 += energy_ij(pos_i0, pos[uint2(x, y)], CC1, CC2, k); force_i1 += energy_ij(pos_i1, pos[uint2(x, y)], CC1, CC2, k); force_i2 += energy_ij(pos_i2, pos[uint2(x, y)], CC1, CC2, k); force_i3 += energy_ij(pos_i3, pos[uint2(x, y)], CC1, CC2, k); x++; }

cbuffer cTiling : register( b0 )
{
	uint cycles, bj, ww, hh;
}

Texture2D<float4> pos : register ( t0 );
Texture2D<int> type : register ( t1 );
Texture2D<float4> coefs : register ( t2 );

RWStructuredBuffer<float4> force;

float4 force_ij(float4 pos_i, float4 pos_j, float4 c1, float4 c2, int type_ij)
{
	float3 R = pos_i.xyz - pos_j.xyz;
	float r = rsqrt(max(dot(R, R), 1e-4f)), _r = 1 / r; float r2 = r * r, dU = c1.x * r2 * r;

	switch (type_ij)
	{
        case 0:
            //if (_r < 1.2f) dU += c1.z * c1.y * exp(c1.z * _r) * r; else
            if (_r < 2.1f) dU -= ((((-27.244726f * 5 * _r + 246.43471f * 4) * _r - 881.96861f * 3) * _r + 1562.2235f * 2) * _r - 1372.5306f) * r;
            else if (_r < 2.6f) dU -= ((-3.1313949f * 3 * _r + 23.077354f * 2) * _r - 55.496531f) * r;
            else dU -= 6 * c1.w * r2 * r2 * r2 * r2;
            break;
        case 1:
        case 2:
            dU -= c1.z * c1.y * exp(c1.z * _r) * r;
            break;
	}
	return float4(R.xyz * dU, 0);
}
float4 energy_ij(float4 pos_i, float4 pos_j, float4 c1, float4 c2, int type_ij)
{
	float4 R = float4(pos_i.xyz - pos_j.xyz, 0); R.w = dot(R.xyz, R.xyz);
	if (R.w == 0) return 0;
	else
	{
		float r = rsqrt(R.w), r2 = r * r, _r = 1 / r, dU = 0;
		float energy = c1.x * r;
		dU = c1.x * r2 * r;
		switch (type_ij) // type_ij = (type_i * types=3 * 2) + (type_j[j] * 2) * 10
		{
			case 0:
				if (_r < 2.1f)
				{
					dU -= ((((-27.244726f * 5 * _r + 246.43471f * 4) * _r - 881.96861f * 3) * _r + 1562.2235f * 2) * _r - 1372.5306f) * r;
					energy += ((((-27.244726f * _r + 246.43471f) * _r - 881.96861f) * _r + 1562.2235f) * _r - 1372.5306f) * _r + 479.95538f;
				}
				else if (_r < 2.6f)
				{
					dU -= ((-3.1313949f * 3 * _r + 23.077354f * 2) * _r - 55.496531f) * r;
					energy += ((-3.1313949f * _r + 23.077354f) * _r - 55.496531f) * _r + 42.891691f;
				}
				else
				{
					dU -= 6 * c1.w * r2 * r2 * r2 * r2;
                    			energy -= c1.w * r2 * r2 * r2;
				}
				break;
			case 1:
			case 2:
				energy += c1.y * exp(c1.z * _r);
				dU -= c1.z * c1.y * exp(c1.z * _r) * r;
				break;
		}
		return float4(R.xyz * dU, energy);
	}
}

[numthreads(threads, 1, 1)]
void ForceNxN(uint3 t : SV_GroupThreadID, uint3 g : SV_GroupID, uint3 tg : SV_DispatchThreadID)
{
	uint i = (t.x + g.x * threads) * 4, x = i % ww, y = i / ww;
	if (i < n)
	{
		float4 pos_i0 = pos[uint2(x + 0, y)], force_i0 = 0;
		float4 pos_i1 = pos[uint2(x + 1, y)], force_i1 = 0;
		float4 pos_i2 = pos[uint2(x + 2, y)], force_i2 = 0;
		float4 pos_i3 = pos[uint2(x + 3, y)], force_i3 = 0;
		uint type_i = type[uint2(x, y)] * 2;

		for (y = g.y; y < hh; y += bj)
		{
			for (x = 0; x < ww; )
			{
				uint k = type_i + type[uint2(x, y)];
				float4 CC1 = coefs[uint2(0, k)], CC2 = coefs[uint2(1, k)];

				FF FF
			}
		}

		force[i + g.y * n + 0] = force_i0;
		force[i + g.y * n + 1] = force_i1;
		force[i + g.y * n + 2] = force_i2;
		force[i + g.y * n + 3] = force_i3;
	}
}

[numthreads(threads, 1, 1)]
void EnergyNxN(uint3 t : SV_GroupThreadID, uint3 g : SV_GroupID, uint3 tg : SV_DispatchThreadID)
{
	uint i = (t.x + g.x * threads) * 4, x = i % ww, y = i / ww;
	if (i < n)
	{
		float4 pos_i0 = pos[uint2(x + 0, y)], force_i0 = 0;
		float4 pos_i1 = pos[uint2(x + 1, y)], force_i1 = 0;
		float4 pos_i2 = pos[uint2(x + 2, y)], force_i2 = 0;
		float4 pos_i3 = pos[uint2(x + 3, y)], force_i3 = 0;
		uint type_i = type[uint2(x, y)] * 2;

		for (y = g.y; y < hh; y += bj)
		{
			for (x = 0; x < ww; )
			{
				uint k = type_i + type[uint2(x, y)];
				float4 CC1 = coefs[uint2(0, k)], CC2 = coefs[uint2(1, k)];

				EE EE
			}
		}

		force[i + g.y * n + 0] = force_i0;
		force[i + g.y * n + 1] = force_i1;
		force[i + g.y * n + 2] = force_i2;
		force[i + g.y * n + 3] = force_i3;
	}
}

[numthreads(threads, 1, 1)]
void Sum(uint3 tg : SV_DispatchThreadID)
{
	float4 sum0 = 0, sum1 = 0, sum2 = 0, sum3 = 0; uint i = tg.x * 4;
	if (i < n)
	{
		for (uint j = 0; j < bj; j++) {
			sum0 += force[i + j * n + 0];
			sum1 += force[i + j * n + 1];
			sum2 += force[i + j * n + 2];
			sum3 += force[i + j * n + 3];
		}
		force[i + 0] = sum0;
		force[i + 1] = sum1;
		force[i + 2] = sum2;
		force[i + 3] = sum3;
	}
}
