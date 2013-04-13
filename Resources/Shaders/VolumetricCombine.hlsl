//////////////////////////////////////////////////////////////////////////
// This shader finally combines the volumetric rendering with the actual screen
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"
#include "Inc/Atmosphere.hlsl"

Texture2D		_TexDebug0	: register(t10);
Texture2D		_TexDebug1	: register(t11);
Texture2DArray	_TexDebug2	: register(t12);

//[
cbuffer	cbObject	: register( b10 )
{
	float3		_dUV;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }






float3	TempComputeSkyColor( float3 _PositionKm, float3 _View, float3 _Sun, float _DistanceKm=-1, float3 _GroundReflectance=0.0 )
{
	float3	StartPositionKm = _PositionKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
	float	StartRadiusKm = length( StartPositionKm );
	float3	StartNormal = StartPositionKm / StartRadiusKm;
	float	CosThetaView = dot( StartNormal, _View );

// 	// Check if we're outside the atmosphere
//	float	d = -StartRadiusKm * CosThetaView - sqrt( StartRadiusKm * StartRadiusKm * (CosThetaView * CosThetaView - 1.0) + ATMOSPHERE_RADIUS_KM*ATMOSPHERE_RADIUS_KM );
// 	if ( d > 0.0 )
// 	{	// if we're in space and ray intersects atmosphere, move to nearest intersection of ray with top atmosphere boundary
//		StartPositionKm += d * _View;
//		StartRadiusKm = ATMOSPHERE_RADIUS_KM-0.01;
//		StartNormal = StartPositionKm / StartRadiusKm;
//		_DistanceKm -= d;
//		CosThetaView = (StartRadiusKm * CosThetaView + d) / ATMOSPHERE_RADIUS_KM;
//	}
//	if ( StartRadiusKm > ATMOSPHERE_RADIUS_KM )
//		return 0.0;	// Lost in space...

	float	CosThetaSun = dot( StartNormal, _Sun );
    float	CosGamma = dot( _View, _Sun );
	float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;

	// Compute sky radiance
    float4	Lin = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Compute end point's radiance
	float3	L0 = 0.0;
	if ( _DistanceKm > 0.0 )
	{	// We're looking at the ground. Compute perceived reflected radiance...
		float3	EndPositionKm = _PositionKm + _DistanceKm * _View - EARTH_CENTER_KM;	// Ground position from origin (i.e. center of the Earth)
		float	EndRadiusKm = length( EndPositionKm );
		float	EndAltitudeKm = EndRadiusKm - GROUND_RADIUS_KM;
		float3	GroundNormal = EndPositionKm / EndRadiusKm;
		float	EndCosThetaSun = dot( _Sun, GroundNormal );

		float	CosThetaGround = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM / (EndRadiusKm*EndRadiusKm)) );
		float3	SunTransmittance = EndCosThetaSun > CosThetaGround ? GetTransmittance( EndAltitudeKm, EndCosThetaSun ) : 0.0;	// Here, we account for shadowing by the planet
		float3	DirectSunLight = saturate( EndCosThetaSun ) * SunTransmittance;													// Lighting by direct Sun light

		float3	GroundIrradiance = GetIrradiance( _TexIrradiance, EndAltitudeKm, EndCosThetaSun );								// Lighting by multiple-scattered light

		L0 = (_GroundReflectance * INVPI) * (DirectSunLight + GroundIrradiance);

		// Subtract end in-scattering if blocked by an obstacle other than ground (since ground has been accounted for in the pre-computation)
		if ( EndAltitudeKm > 0.01 )
		{
			float3	ViewTransmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );						
			float4	EndLin = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );
			Lin -= ViewTransmittance.xyzx * EndLin;
		}
	}
	else
	{	// We're looking up. Check if we can see the Sun...
		L0 = smoothstep( 0.999, 0.9995, CosGamma );	// 1 if we're looking directly at the Sun (warning: bad for the eyes!)
	}

	// Compute final radiance
	Lin = max( 0.0, Lin );

	return PhaseFunctionRayleigh( CosGamma ) * Lin.xyz + PhaseFunctionMie( CosGamma ) * GetMieFromRayleighAndMieRed( Lin ) + L0 * GetTransmittance( StartAltitudeKm, CosThetaView );
}

float3	HDR( float3 L, float _Exposure=0.5 )
{
    L = L * _Exposure;
//     L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
//     L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
//     L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);

	L = 1.0 - exp( -L );

    return L;
}

float3	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;
//return float4( UV, 0, 1 );
	
// 	float2	Depth = _TexDebug1.SampleLevel( LinearClamp, UV, 0.0 ).xy;
// return 0.5 * Depth.y;
// return 0.5 * (Depth.y - Depth.x);

//	float3	BackgroundColor = 0.9 * float3( 135, 206, 235 ) / 255.0;
//	float3	BackgroundColor = 0.3;

//return 4.0 * _TexScattering.SampleLevel( LinearClamp, float3( UV, 0.0 / RESOLUTION_ALTITUDE + fmod( 0.1 * _Time.x, 1.0 ) ), 0.0 );
//return _TexCloudTransmittance.SampleLevel( LinearClamp, UV, 0.0 );
//return 50.0 * _TexIrradiance.SampleLevel( LinearClamp, UV, 0.0 );

	float3	View = normalize( float3( _CameraData.x * (2.0 * UV.x - 1.0), -_CameraData.y * (2.0 * UV.y - 1.0), 1.0 ) );
			View = mul( float4( View, 0.0 ), _Camera2World ).xyz;
	float	Sun = saturate( dot( _LightDirection, View ) );

// float4	Pipo = Sample4DScatteringTable( _TexScattering, 0.0, View.y, _LightDirection.y, dot( View, _LightDirection ) );
// float3	Mie = GetMieFromRayleighAndMieRed( Pipo );
// return 10.0 * Mie;

	// From iQ's clouds
// 	float3	BackgroundColor = float3( 0.6, 0.71, 0.75 ) - View.y * 0.2 * float3( 1.0, 0.9, 1.0 ) + 0.15*0.5;	// Sky gradient
// //			BackgroundColor += 0.8 * float3(1.0,0.8,0.6) * pow( Sun, 8.0 );	// Sun glow
// 			BackgroundColor *= 0.95;
// //		 	BackgroundColor += 0.2 * float3(1.0,0.95,0.8) * pow( Sun, 3.0 );


	// From sky's multiple scattering
	float3	PositionKm = 0.01 * _Camera2World[3].xyz;
	float3	BackgroundColor = TempComputeSkyColor( PositionKm, View, _LightDirection );
			BackgroundColor *= SUN_INTENSITY;
//return BackgroundColor;
//return HDR( BackgroundColor );

	float4	ScatteringExtinction = _TexDebug0.SampleLevel( LinearClamp, UV, 0.0 );
return HDR( BackgroundColor * ScatteringExtinction.w + ScatteringExtinction.xyz );

	float4	C0 = _TexDebug2.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	C1 = _TexDebug2.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
return 1.0 * C0.xyz;
return 1.0 * abs( C0.x );
return 4.0 * abs(C0.xyz);

//return fmod( 0.5 * _Time.x, 1.0 );

	float3	ShadowPos = float3( 2.0 * fmod( 0.5 * _Time.x, 1.0 ) - 1.0, 1.0 - 2.0 * UV.y, _ShadowZMax.x * UV.x );
	return 0.999 * GetTransmittance( ShadowPos );
}
