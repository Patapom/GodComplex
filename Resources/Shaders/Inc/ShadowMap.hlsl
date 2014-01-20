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
float	ComputeShadow( float3 _WorldPosition, float3 _WorldVertexNormal, float _Radius )
{
	float4	ShadowPosition = World2ShadowMapProj( _WorldPosition + 0.01 * _WorldVertexNormal );

	float2	UV = 0.5 * float2( 1.0 + ShadowPosition.x, 1.0 - ShadowPosition.y );
	float	Zproj = ShadowPosition.z / ShadowPosition.w;

//Zproj -= 0.002;	// Small bias to avoid noise

	return _ShadowMap.SampleCmp( ShadowSampler, UV, Zproj );
}

#endif	// _SHADOW_MAP_INC_
