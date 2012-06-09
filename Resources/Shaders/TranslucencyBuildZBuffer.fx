//////////////////////////////////////////////////////////////////////////
// This shader renders 2 internal & external objects as a RGBA Z-Buffer where each channel is masked
//	to render the external object's front (Red) and back (Green) + the internal object's front (Blue) and back (Alpha)
//
#include "Inc/Global.fx"

//[
cbuffer	cbObject	: register( b1 )
{
	float4x4	_Local2World;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float	Z			: Z;
};

PS_IN	VS( VS_IN _In )
{
	float4	Position = mul( float4( _In.Position, 1.0 ), _Local2World );	// We assume the object is already in clip space, we only rotate it a bit
	float	Z = 1.0 + Position.z;					// Keep the linear Z in [0,2]

	Position.z = 0.5 * Z;							// Finally, clip space Z is in [0,1]

	PS_IN	Out;
	Out.__Position = Position;
	Out.Z = Z;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return _In.Z;	// Simply write Z in all channels. The blending state will isolate the channel we'll write to
}
