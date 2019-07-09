#include "Global.hlsl"

Texture2D<float3>	_texRef : register(t0);
Texture2D<float3>	_texLTC : register(t1);
Texture2D<float3>	_texGradient : register(t2);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _in ) { return _in; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	pixel = uint2( floor( _In.__Position.xy ) );
	float3	C0 = _texRef[pixel];
	float3	C1 = _texLTC[pixel];
	float3	diff = C0 - C1;
	float	t = length( diff );
	float	logT = log10( clamp( t, 1e-10, 1 ) );
	float	U = 1 + logT / 10.0;
//return 0.5 * -logT / 10.0;
//U = _In.__Position.x / 1024;
	return _texGradient.SampleLevel( LinearClamp, float2( U, 0.5 ), 0.0 );
}
