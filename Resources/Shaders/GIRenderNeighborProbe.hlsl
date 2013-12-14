//////////////////////////////////////////////////////////////////////////
// This shader renders a plane centered on a probe whose normal points from the probe to our current probe
// The plane's pixel will be visible if not culled by the ZBuffer and they will write the probe's ID so we
//	identify it when we read back the cube map
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbCubeMapCamera	: register( b9 )
{
	float4x4	_CubeMapWorld2Proj;
};
//]

//[
cbuffer	cbProbe	: register( b10 )
{
	float3		_CurrentProbePosition;
	uint		_NeighborProbeID;
	float3		_NeighborProbePosition;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	float3	PlaneNormal = _CurrentProbePosition - _NeighborProbePosition;
	float	Distance2Neighbor = length( PlaneNormal );
	PlaneNormal /= Distance2Neighbor;

	float3	PlaneTangent = cross( float3( 0, 1, 0 ), PlaneNormal );
	float3	PlaneBiTangent;
	float	L = length( PlaneTangent );
	if ( L > 1e-6 )
	{
		PlaneTangent /= L;
		PlaneBiTangent = cross( PlaneNormal, PlaneTangent );
	}
	else
	{	// Arbitrary basis
		PlaneTangent = float3( 1, 0, 0 );
		PlaneBiTangent = float3( 0, 0, 1 );
	}

	float4	WorldPosition = float4( _NeighborProbePosition + 5.0 * Distance2Neighbor * (_In.__Position.x * PlaneTangent + _In.__Position.y * PlaneBiTangent), 1.0 );

	VS_IN	Out;
	Out.__Position = mul( WorldPosition, _CubeMapWorld2Proj );

	return Out;
}

//VS_IN	VS( VS_IN _In )	{ return _In; }

uint	PS( VS_IN _In ) : SV_TARGET0
{
	return _NeighborProbeID;
}
