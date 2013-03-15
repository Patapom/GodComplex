//////////////////////////////////////////////////////////////////////////
// This shader computes the transmittance function map
//
#include "Inc/Global.fx"
#include "Inc/Volumetric.fx"


//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2Proj;
	float3		_dUV;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_OUT
{
	float4	C0 : SV_TARGET0;
	float4	C1 : SV_TARGET1;
};

PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
			Out.__Position = mul( float4( _In.Position, 1.0 ), _Local2Proj );
	return Out;
}

PS_OUT	PS( PS_IN _In )
{
	PS_OUT	Out;



	return Out;
}
