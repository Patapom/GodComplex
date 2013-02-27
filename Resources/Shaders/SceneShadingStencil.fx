//////////////////////////////////////////////////////////////////////////
// This shader performs the stencil pass for the different light types
//
#include "Inc/Global.fx"

cbuffer	cbLight	: register( b11 )
{
	float3		_LightPosition;
	float3		_LightDirection;
	float3		_LightIrradiance;
	float4		_LightData;			// For directionals: X=Hotspot Radius Y=Falloff Radius Z=Length
									// For points: X=Radius
									// For spots: X=Hotspot Angle Y=Falloff Angle Z=Length W=tan(Falloff Angle/2)
};

struct	VS_IN
{
	float3	Position	: POSITION;
	float2	UV			: TEXCOORD0;
};


#if LIGHT_TYPE == 0
// ======================= DIRECTIONAL LIGHT =======================
// We're drawing a cylinder that we must orient to fit the light's direction
//
float4	VS( VS_IN _In ) : SV_POSITION
{
	float3	Z = normalize( cross( _LightDirection, float3( 1, 0, 0 ) ) );	// Won't work if light is aligned with X!
	float3	X = cross( Z, _LightDirection );

	float	Length = _LightData.z * _In.UV.y;
	float	Radius = _LightData.y;
			Radius *= 1.0048385723763114110233402385602;	// 1 / cos(0.5 * 2PI/32 )  32 = amount of subdivisions of the cylinder

	float3	LocalPosition = float3( Radius * _In.Position.x, Length, Radius * _In.Position.z );
	float3	WorldPosition = _LightPosition + LocalPosition.x * X + LocalPosition.y * _LightDirection + LocalPosition.z * Z;

	// Project
	return mul( float4( WorldPosition, 1.0 ), _World2Proj );
}

#elif LIGHT_TYPE == 1
// ======================= POINT LIGHT =======================
// We're drawing a quad that must bound the point light's sphere
//
float4	VS( VS_IN _In ) : SV_POSITION
{
// 	float3	ToCenter = _LightPosition - _Camera2World[3].xyz;
// 	float	Front = dot( ToCenter, _Camera2World[2].xyz );
// 	if ( Front < 0.0 )
// 	{	// Don't display anything if behind the camera
// 		Out.__Position = float4( -2.0, -2.0, 0.0, 1.0 );
// 		return Out;
// 	}
// 
// 	float	SqDistance = dot( ToCenter, ToCenter );
// 	if ( SqDistance < _LightData.x*_LightData.x )
// 	{	// We're inside the sphere => the quad must cover the entire screen unfortunately...
// 		Out.__Position = float4( _In.UV, 0.0, 1.0 );
// 		return Out;
// 	}
// 
// 	// The camera is outside and in front of the sphere
// 	// We must compute the size of the quand in view space, which is larger than the radius of the sphere because of perspective projection
// 	float3	X = normalize( cross( ToCenter, float3( 0, 1, 0 ) ) );	// Won't work if camera is aligned with Y but that's quite improbable!
// 	float3	Y = normalize( cross( X, ToCenter ) );
// 
// 	float	r = _LightData.x;
// 	float	Distance = sqrt( SqDistance );
// 	ToCenter /= Distance;	// Normalize
// 	Distance -=  r;
// 	float	Radius = Distance * r / sqrt( Distance*Distance - r*r );
// 
// 
// // Out.Color = Radius;
// // 	Radius = 3.0;
// 
// 
// 	float3	WorldPosition = _LightPosition + Radius * (_In.UV.x * X + _In.UV.y * Y) - r * ToCenter;

	float	Radius = _LightData.x;
			Radius *= 1.0048385723763114110233402385602;	// 1 / cos(0.5 * 2PI/32 )  32 = amount of subdivisions of the sphere
			Radius *= 1.0048385723763114110233402385602;	// Squared because of subdivisions on theta
 	float3	WorldPosition = _LightPosition + Radius * _In.Position;

	// Project
	return mul( float4( WorldPosition, 1.0 ), _World2Proj );
}

#else
// ======================= SPOT LIGHT =======================
// We're drawing a cone that we must orient to fit the light's direction
//
float4	VS( VS_IN _In ) : SV_POSITION
{
	float3	Z = normalize( cross( _LightDirection, float3( 1, 0, 0 ) ) );	// Won't work if Light is aligned with X!
	float3	X = cross( Z, _LightDirection );

	float	Length = _LightData.z * _In.UV.y;
	float	Radius = Length * _LightData.w;	// Falloff radius is the largest at the bottom. We should increase it to compensate for radial discretization of faces that will cut through a perfect cylinder... TODO!
			Radius *= 1.0048385723763114110233402385602;	// 1 / cos(0.5 * 2PI/32 )  32 = amount of subdivisions of the cylinder

	float3	LocalPosition = float3( Radius * _In.Position.x, Length, Radius * _In.Position.z );
	float3	WorldPosition = _LightPosition + LocalPosition.x * X + LocalPosition.y * _LightDirection + LocalPosition.z * Z;

	// Project
	return mul( float4( WorldPosition, 1.0 ), _World2Proj );
}

#endif
