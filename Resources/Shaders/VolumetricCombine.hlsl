//////////////////////////////////////////////////////////////////////////
// This shader finally combines the volumetric rendering with the actual screen
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"
#include "Inc/Atmosphere.hlsl"

Texture2DArray	_TexDebug0	: register(t10);
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



float3	HDR( float3 L, float _Exposure=0.5 )
{
	L = L * _Exposure;
	L = 1.0 - exp( -L );
	return L;
}


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
		float	EndCosThetaView = dot( GroundNormal, _View );
		float	EndCosThetaSun = dot( GroundNormal, _Sun );

		float	CosThetaGround = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM / (EndRadiusKm*EndRadiusKm)) );
		float3	SunTransmittance = EndCosThetaSun > CosThetaGround ? GetTransmittance( EndAltitudeKm, EndCosThetaSun ) : 0.0;	// Here, we account for shadowing by the planet
		float3	DirectSunLight = saturate( EndCosThetaSun ) * SunTransmittance;													// Lighting by direct Sun light

		float3	GroundIrradiance = GetIrradiance( _TexIrradiance, EndAltitudeKm, EndCosThetaSun );								// Lighting by multiple-scattered light

		L0 = (_GroundReflectance * INVPI) * (DirectSunLight + GroundIrradiance);

		// Subtract end in-scattering if blocked by an obstacle other than ground (since ground has been accounted for in the pre-computation)
		if ( EndAltitudeKm > 0.01 )
		{
			float3	ViewTransmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );						
			float4	EndLin = Sample4DScatteringTable( _TexScattering, EndAltitudeKm, EndCosThetaView, EndCosThetaSun, CosGamma );
			Lin -= ViewTransmittance.xyzx * EndLin;
		}

		L0 *= GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );	// Attenuated through the atmosphere until end point
	}
	else
	{	// We're looking up. Check if we can see the Sun...
		L0 = smoothstep( 0.999, 0.9995, CosGamma );					// 1 if we're looking directly at the Sun (warning: bad for the eyes!)
		L0 *= GetTransmittance( StartAltitudeKm, CosThetaView );	// Attenuated through the atmosphere
	}

	// Compute final radiance
	Lin = max( 0.0, Lin );

	return PhaseFunctionRayleigh( CosGamma ) * Lin.xyz + PhaseFunctionMie( CosGamma ) * GetMieFromRayleighAndMieRed( Lin ) + L0;
}

// This function assumes we're standing below the cloud and thus get the full extinction
// float	GetFastCloudTransmittance( float3 _WorldPosition )
// {
// 	float3	ShadowPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow ).xyz;
// 	float2	UV = float2( 0.5 * (1.0 + ShadowPosition.x), 0.5 * (1.0 - ShadowPosition.y) );
// 
// 	float4	C0 = _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
// 	return C0.x - C0.y + C0.z - C0.w;	// Skip smaller coefficients... No need to tap further.
// 	float4	C1 = _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
// 	return C0.x - C0.y + C0.z - C0.w + C1.x - C1.y;
// }

float	ComputeCloudShadowing( float3 _PositionWorld, float3 _View, float _Distance, float _StepOffset=0.5, uniform uint _StepsCount=64 )
{
//	uint	StepsCount = ceil( lerp( 16.0, float(_StepsCount), saturate( _Distance / 150.0 ) ) );
	uint	StepsCount = ceil( 2.0 * _StepOffset + lerp( 16.0, float(_StepsCount), saturate( _Distance / 150.0 ) ) );	// Fantastic noise hides banding!

//return 0.01 * StepsCount;

	float3	Step = (_Distance / StepsCount) * _View;
//	_PositionWorld += _StepOffset * Step;

	float	SumIncomingLight = 0.0;
	for ( uint StepIndex=0; StepIndex < StepsCount; StepIndex++ )
	{
//		SumIncomingLight += GetCloudTransmittance( _PositionWorld );
		SumIncomingLight += GetFastCloudTransmittance( _PositionWorld );
		_PositionWorld += Step;
	}
	return saturate( SumIncomingLight / _StepsCount );
}

float3	ComputeFinalColor( float3 _PositionWorld, float3 _View, float3 _Sun, float4 _CloudScatteringExtinction, float _StepOffset=0.5 )
{
//	return SUN_INTENSITY * TempComputeSkyColor( _PositionKm, _View, _Sun );	// Not accounting for shadowing

	float3	PositionKm = WORLD2KM * _PositionWorld;

	// Compute sky radiance arriving at camera, not accounting for cloud
	float3	StartPositionKm = PositionKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
	float	StartRadiusKm = length( StartPositionKm );
	float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;
	float3	StartNormal = StartPositionKm / StartRadiusKm;
	float	CosThetaView = dot( StartNormal, _View );
	float	CosThetaSun = dot( StartNormal, _Sun );

	float	CosGamma = dot( _View, _Sun );

	float4	Lin = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );
	float3	Lin_Rayleigh = Lin.xyz;
	float3	Lin_Mie = GetMieFromRayleighAndMieRed( Lin );
// return INVFOURPI * SUN_INTENSITY * Lin_Mie;
// return INVFOURPI * SUN_INTENSITY * Lin_Rayleigh;

	// Compute intersection with the bottom cloud plane
	float	Height2Plane = BOX_BASE + 0.2 * BOX_HEIGHT - _PositionWorld.y;
	float	HitDistance = Height2Plane / _View.y;
//return 0.1 * HitDistanceKm;
	if ( HitDistance < 0.0 )
		HitDistance = -_PositionWorld.y / _View.y;	// We're hitting the ground instead...

	float	HitDistanceKm = WORLD2KM * HitDistance;
	HitDistanceKm = min( 30.0, HitDistanceKm );	// Beyond that, we're outside the clouds...
	HitDistance = HitDistanceKm / WORLD2KM;

	// Account for cloud shadowing
	float3	CloudPositionKm = PositionKm + HitDistanceKm * _View - EARTH_CENTER_KM;
	float	CloudRadiusKm = length( CloudPositionKm );
	float	CloudAltitudeKm = CloudRadiusKm - GROUND_RADIUS_KM;
	float3	CloudNormal = CloudPositionKm / CloudRadiusKm;
	float	CloudCosThetaView = dot( CloudNormal, _View );
	float	CloudCosThetaSun = dot( CloudNormal, _Sun );

	// Compute sky radiance arriving at cloud (i.e. above and inside cloud)
	float4	Lin_cloud2atmosphere = Sample4DScatteringTable( _TexScattering, CloudAltitudeKm, CloudCosThetaView, CloudCosThetaSun, CosGamma );
	float3	Lin_cloud2atmosphere_Rayleigh = Lin_cloud2atmosphere.xyz;
	float3	Lin_cloud2atmosphere_Mie = GetMieFromRayleighAndMieRed( Lin_cloud2atmosphere );
// return 10 * INVFOURPI * SUN_INTENSITY * Lin_cloud2atmosphere_Mie;
// return INVFOURPI * SUN_INTENSITY * Lin_cloud2atmosphere_Rayleigh;

	// Compute sky radiance between camera and cloud
	float3	Transmittance = GetTransmittance( StartAltitudeKm, CosThetaView, HitDistanceKm );
	float3	Lin_camera2cloud_Rayleigh = max( 0.0, Lin_Rayleigh - Transmittance * Lin_cloud2atmosphere_Rayleigh );
	float3	Lin_camera2cloud_Mie = max( 0.0, Lin_Mie - Transmittance * Lin_cloud2atmosphere_Mie );

// return INVFOURPI * SUN_INTENSITY * Lin_camera2cloud_Mie;
// return INVFOURPI * SUN_INTENSITY * Lin_camera2cloud_Rayleigh;

	// Attenuate in-scattered light between camera and cloud due to shadowing
	float	Shadowing = ComputeCloudShadowing( _PositionWorld, _View, HitDistance, _StepOffset );
//return Shadowing;
	const float	ShadowingStrength = 1.0;
	Lin_camera2cloud_Rayleigh *= 1.0 - (ShadowingStrength * (1.0 - Shadowing));
	Lin_camera2cloud_Mie *= 1.0 - (ShadowingStrength * (1.0 - Shadowing));

	// Rebuild final camera2atmosphere scattering, accounting for cloud extinction
	Lin_Rayleigh = Lin_camera2cloud_Rayleigh + _CloudScatteringExtinction.w * Transmittance * Lin_cloud2atmosphere_Rayleigh;
	Lin_Mie = Lin_camera2cloud_Mie + _CloudScatteringExtinction.w * Transmittance * Lin_cloud2atmosphere_Mie;
// return INVFOURPI * SUN_INTENSITY * Lin_Mie;
// return INVFOURPI * SUN_INTENSITY * Lin_Rayleigh;

	// Compute Sun radiance at the end of the ray
	float3	L0 = smoothstep( 0.999, 0.9995, CosGamma );					// 1 if we're looking directly at the Sun (warning: bad for the eyes!)
			L0 *= _CloudScatteringExtinction.w * GetTransmittance( StartAltitudeKm, CosThetaView );	// Attenuated through the atmosphere

	return SUN_INTENSITY * (PhaseFunctionRayleigh( CosGamma ) * Lin_Rayleigh + PhaseFunctionMie( CosGamma ) * Lin_Mie + L0) + _CloudScatteringExtinction.xyz;
}

float3	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;

// DEBUG
#if 0
if ( UV.x < 0.2 && UV.y > 0.8 )
{	// Show the transmittance map
	UV.x /= 0.2;
	UV.y = (UV.y - 0.8) / 0.2;
	return _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).xyz;
}
#endif
// DEBUG



//return _TexTransmittance.SampleLevel( LinearClamp, UV, 0.0 );




	float3	View = normalize( float3( _CameraData.x * (2.0 * UV.x - 1.0), -_CameraData.y * (2.0 * UV.y - 1.0), 1.0 ) );
			View = mul( float4( View, 0.0 ), _Camera2World ).xyz;

//	float	Sun = saturate( dot( _LightDirection, View ) );
//
// float4	Pipo = Sample4DScatteringTable( _TexScattering, 0.0, View.y, _LightDirection.y, dot( View, _LightDirection ) );
// float3	Mie = GetMieFromRayleighAndMieRed( Pipo );
// return 10.0 * Mie;

// Debug transmittance function map
// 	float4	C0 = _TexDebug2.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
// 	float4	C1 = _TexDebug2.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
// return 1.0 * C0.xyz;
// return 1.0 * abs( C0.x );
// return 4.0 * abs(C0.xyz);
// 
// 	float3	ShadowPos = float3( 2.0 * fmod( 0.5 * _Time.x, 1.0 ) - 1.0, 1.0 - 2.0 * UV.y, _ShadowZMinMax.x * UV.x );
// 	return 0.999 * GetCloudTransmittance( ShadowPos );

	// Load terrain background
	float4	TerrainAlpha = _TexDebug1.SampleLevel( LinearClamp, UV, 0.0 );
	float3	Terrain = TerrainAlpha.xyz;
//return TerrainAlpha.w;
//return HDR( Terrain );

	// Load scattering & extinction from sky and clouds
	float3	Scattering = _TexDebug0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).xyz;
	float3	Extinction = _TexDebug0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;
return Scattering;
//return HDR( Scattering );
//return HDR( Extinction );

	// Compute Sun's color
	float	CameraAltitudeKm = WORLD2KM * _Camera2World[3].y;
	float	CosThetaView = View.y;
	float	CosGamma = dot( View, _LightDirection );

	float3	SunColor = SUN_INTENSITY * GetTransmittance( CameraAltitudeKm, CosThetaView );	// Attenuated through the atmosphere
	float3	DirectSunLight = smoothstep( 0.999, 0.9995, CosGamma );							// 1 if we're looking directly at the Sun (warning: bad for the eyes!)
			DirectSunLight *= (1.0-TerrainAlpha.w) * SunColor;
//return HDR( DirectSunLight );

	// Compose color
	float3	FinalColor = (Terrain + DirectSunLight) * Extinction + Scattering;

	// Add a nice bloom for the Sun
	FinalColor += 0.02 * smoothstep( 0.9, 1.0, sqrt(CosGamma) ) * SunColor * smoothstep( 0.1, 0.3, _LightDirection.y );
	FinalColor += 0.002 * smoothstep( 0.1, 1.0, sqrt(CosGamma) ) * SunColor * smoothstep( 0.1, 0.3, _LightDirection.y );

	return HDR( FinalColor );

// 	// From sky's multiple scattering
// 	float4	ScatteringExtinction = _TexDebug0.SampleLevel( LinearClamp, UV, 0.0 );
// //return HDR( ScatteringExtinction.xyz );
// 
// 	float	StepOffset = FastScreenNoise( _In.__Position.xy );
// 	float3	PositionWorld = _Camera2World[3].xyz;
// 	float3	FinalColor = ComputeFinalColor( PositionWorld, View, _LightDirection, ScatteringExtinction, StepOffset );
// //return FinalColor;
// 	return HDR( FinalColor );
}
