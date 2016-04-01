#include "Room.hlsl"

Texture3D< float >		_TexNoise : register(t8);
Texture3D< float4 >		_TexNoise4D : register(t9);
Texture3D< float >		_TexDensity : register(t10);

float	fbm( float3 _noisePosition ) {
	float	V0 = _TexNoise.SampleLevel( LinearWrap, 0.05 * _noisePosition, 0.0 );
	float	V1 = _TexNoise.SampleLevel( LinearWrap, 0.10 * _noisePosition, 0.0 );
	float	V2 = _TexNoise.SampleLevel( LinearWrap, 0.20 * _noisePosition, 0.0 );
	return (V2 + 2.0 * V1 + 4.0 * V0) / 7.0;
}

float3	fbm3D( float3 _noisePosition ) {
	float3	V0 = _TexNoise4D.SampleLevel( LinearWrap, 0.05 * _noisePosition, 0.0 ).xyz;
	float3	V1 = _TexNoise4D.SampleLevel( LinearWrap, 0.10 * _noisePosition, 0.0 ).xyz;
	float3	V2 = _TexNoise4D.SampleLevel( LinearWrap, 0.20 * _noisePosition, 0.0 ).xyz;
	return (V2 + 2.0 * V1 + 4.0 * V0) / 7.0;
}

float3	NoiseVector( float3 _noisePosition ) {
	return 2.0 * fbm3D( _noisePosition ) - 1.0;
}

float	SampleNoiseDensity( float3 _wsPosition ) {
	float3	noiseUVW = World2RoomUVW( _wsPosition );
	return _TexDensity.SampleLevel( LinearClamp, noiseUVW, 0.0 );
}