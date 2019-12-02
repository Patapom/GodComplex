//////////////////////////////////////////////////////////////////////////
// This shader renders a plane centered on a probe whose normal points from the probe to our current probe
// The plane's pixel will be visible if not culled by the ZBuffer and they will write the probe's ID so we
//	identify it when we read back the cube map
//
#include "Inc/Global.hlsl"

static const float	PLANE_SIZE_RATIO = 0.5;	// The neighbor probe's plane will be RATIO * DistanceBetweenProbes long

cbuffer	cbCubeMapCamera	: register( b8 )
{
	float4x4	_CubeMap2World;
	float4x4	_CubeMapWorld2Proj;
};

cbuffer	cbProbe	: register( b10 )
{
	float3		_CurrentProbePosition;
	uint		_NeighborProbeID;
	float3		_NeighborProbePosition;
	float		_QuadHalfSize;
};

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	UVW			: TEXCOORD0;
};

PS_IN	VS( VS_IN _In )
{
	float3	PlaneNormal = _CurrentProbePosition - _NeighborProbePosition;
	float	Distance2Neighbor = length( PlaneNormal );
	PlaneNormal /= Distance2Neighbor;

	float3	PlaneTangent = cross( float3( 0, 1, 0 ), PlaneNormal );
	float3	PlaneBiTangent;
	float	L = length( PlaneTangent );
	if ( L > 1e-6 ) {
		PlaneTangent /= L;
		PlaneBiTangent = cross( PlaneNormal, PlaneTangent );
	} else {
		// Arbitrary basis
		PlaneTangent = float3( 1, 0, 0 );
		PlaneBiTangent = float3( 0, 0, 1 );
	}

//	float4	WorldPosition = float4( _NeighborProbePosition + 0.5 * PLANE_SIZE_RATIO * Distance2Neighbor * (_In.__Position.x * PlaneTangent + _In.__Position.y * PlaneBiTangent), 1.0 );
	float4	WorldPosition = float4( _NeighborProbePosition + _QuadHalfSize * (_In.__Position.x * PlaneTangent + _In.__Position.y * PlaneBiTangent), 1.0 );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _CubeMapWorld2Proj );
//	Out.UVW = float3( 0.5 * float2( 1.0 + _In.__Position.x, 1.0 - _In.__Position.y ), 0 );
//	Out.UVW = PlaneNormal;
	Out.UVW = WorldPosition;

	return Out;
}

//VS_IN	VS( VS_IN _In )	{ return _In; }

float4	PS( PS_IN _In ) : SV_TARGET0
{
	float	Distance2Plane = length( _In.UVW - _CurrentProbePosition );
	return float4( asfloat( _NeighborProbeID ), Distance2Plane, 0, 0 );

//return float4( _In.UVW, 0 );
// return uint( 255.0 * length( _In.UVW - _CurrentProbePosition ) );
//return uint( 255.0 * 0.5 * (1.0 + _In.UVW.x) ) | (uint( 255.0 * 0.5 * (1.0 + _In.UVW.y) ) << 8) | (uint( 255.0 * 0.5 * (1.0 + _In.UVW.z) ) << 16);
//return uint( 255.0 * _In.UVW.x ) | (uint( 255.0 * _In.UVW.y ) << 8) | (uint( 255.0 * _In.UVW.z ) << 16);
}
