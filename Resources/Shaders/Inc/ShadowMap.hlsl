////////////////////////////////////////////////////////////////////////////////////////
// Shadow Map Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _SHADOW_MAP_INC_
#define _SHADOW_MAP_INC_

//[
cbuffer	cbShadowMap : register( b2 )
{
	float4x4	_Shadow2World;
	float4x4	_World2Shadow;
	float3		_ShadowBoundMin;
	float3		_ShadowBoundMax;
};
//]

// struct ShadowMapInfos
// {
// 	float3		BoundMin;
// 	float3		BoundMax;
// };
// StructuredBuffer<ShadowMapInfos>	_ShadowMapInfos : register( t2 );

Texture2D<float>	_ShadowMap : register( t2 );

// Transforms the world position into a projected shadow map position
float4	World2ShadowMapProj( float3 _WorldPosition )
{
	return mul( float4( _WorldPosition, 1.0 ), _World2Shadow );
}

// Same but when I used manual bounds
float4	World2ShadowMapProj_ManualBounds( float3 _WorldPosition )
{
	float4	LightPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow );

	// Simply rescale XY coordinates in [-1,+1]
	LightPosition.xy = 2.0 * (LightPosition.xy - _ShadowBoundMin.xy) / (_ShadowBoundMax.xy - _ShadowBoundMin.xy) - 1.0;

	// And manually project Z,W coordinates
	LightPosition.w = LightPosition.z;																						// W' = Z no matter what
	LightPosition.z = _ShadowBoundMax.z * (LightPosition.z - _ShadowBoundMin.z) / (_ShadowBoundMax.z - _ShadowBoundMin.z);	// Z' = Zf * (Z-Zn) / (Zf - Zn) (as explained in http://msdn.microsoft.com/en-us/library/windows/desktop/bb147302(v=vs.85).aspx)

	return LightPosition;
}

// Computes the distance to the shadow blocker for that position
// Returns a negative distance if not blocked
float	ComputeBlockerDistance( float3 _WorldPosition )
{
	float4	ShadowPosition = World2ShadowMapProj( _WorldPosition );

	float2	UV = 0.5 * (1.0 + ShadowPosition.xy);
	float	ShadowZproj = _ShadowMap.Sample( LinearClamp, UV ).x;
	float	ShadowZ = ShadowZproj * (_ShadowBoundMax.z - _ShadowBoundMin.z) / _ShadowBoundMax.z + _ShadowBoundMin.z;

	return ShadowPosition.w - ShadowZ;
}

// Computes the shadow value for the given posiion, accounting for a radius around the position for soft shadowing
float	ComputeShadow( float3 _WorldPosition, float3 _WorldVertexNormal )
{
	float4	ShadowPosition = World2ShadowMapProj( _WorldPosition + 0.01 * _WorldVertexNormal );

	float2	UV = 0.5 * float2( 1.0 + ShadowPosition.x, 1.0 - ShadowPosition.y );
	float	Zproj = ShadowPosition.z / ShadowPosition.w;

//Zproj -= 0.002;	// Small bias to avoid noise

	return _ShadowMap.SampleCmp( ShadowSampler, UV, Zproj );
}

float	ComputeShadowPCF( float3 _WorldPosition, float3 _WorldVertexNormal, float3 _WorldVertexTangent, float _Radius )
{
	float3	X = _Radius * _WorldVertexTangent;
	float3	Y = cross( _WorldVertexNormal, _WorldVertexTangent );

	const uint		SHADOW_SAMPLES_COUNT = 32;
	const float2	SamplesOffset[SHADOW_SAMPLES_COUNT] = {
		float2( 0.6935199, 0.1379497 ),
		float2( 0.4619398, 0.1913417 ),
		float2( 0.7200738, 0.4811379 ),
		float2( 0.25, 0.25 ),
		float2( 0.4392168, 0.6573345 ),
		float2( 0.2343448, 0.5657583 ),
		float2( 0.1824902, 0.9174407 ),
		float2( -1.092785E-08, 0.25 ),
		float2( -0.1463178, 0.7355889 ),
		float2( -0.2139266, 0.5164644 ),
		float2( -0.5007843, 0.7494765 ),
		float2( -0.3061862, 0.3061862 ),
		float2( -0.6894183, 0.4606545 ),
		float2( -0.6110889, 0.2531212 ),
		float2( -0.9496413, 0.1888954 ),
		float2( -0.1767767, -1.545431E-08 ),
		float2( -0.714864, -0.1421954 ),
		float2( -0.4899611, -0.2029485 ),
		float2( -0.7349222, -0.4910594 ),
		float2( -0.2795084, -0.2795085 ),
		float2( -0.4500631, -0.6735675 ),
		float2( -0.2439136, -0.5888601 ),
		float2( -0.1857205, -0.9336798 ),
		float2( 3.651234E-09, -0.3061862 ),
		float2( 0.1503273, -0.7557458 ),
		float2( 0.2243682, -0.5416723 ),
		float2( 0.510324, -0.7637535 ),
		float2( 0.330719, -0.3307188 ),
		float2( 0.7049127, -0.4710076 ),
		float2( 0.6325371, -0.2620054 ),
		float2( 0.9653389, -0.1920177 ),
		float2( 0.125, 0 ),
	};

	float	Shadow = 0.0;
	[unroll]
	for ( int SampleIndex=0; SampleIndex < SHADOW_SAMPLES_COUNT; SampleIndex++ )
	{
		float3	WorldPosition = _WorldPosition + SamplesOffset[SampleIndex].x * X + SamplesOffset[SampleIndex].y * Y;
		float4	ShadowPosition = World2ShadowMapProj( WorldPosition + 0.01 * _WorldVertexNormal );

		float2	UV = 0.5 * float2( 1.0 + ShadowPosition.x, 1.0 - ShadowPosition.y );
		float	Zproj = ShadowPosition.z / ShadowPosition.w;

		Shadow += _ShadowMap.SampleCmp( ShadowSampler, UV, Zproj );
	}

	return Shadow / SHADOW_SAMPLES_COUNT;
}

#endif	// _SHADOW_MAP_INC_
