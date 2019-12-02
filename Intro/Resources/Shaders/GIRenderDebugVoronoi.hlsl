//////////////////////////////////////////////////////////////////////////
// This shader renders a probe's voronoï cell
//
#include "Inc/Global.hlsl"

cbuffer	cbObject	: register( b10 ) {
	float4	_Color;
}

struct	VS_IN {
	float3	Position	: POSITION;
};

struct	PS_IN {
	float4	__Position	: SV_POSITION;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;
	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	return _Color * float4( 1, 1, 0, 1 );
}
