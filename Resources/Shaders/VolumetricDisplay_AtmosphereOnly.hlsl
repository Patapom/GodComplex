//////////////////////////////////////////////////////////////////////////
// This shader renders the atmosphere without clouds, need to debug these tables!!!
//
#include "Inc/Global.hlsl"
#include "Inc/Atmosphere.hlsl"

cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
	bool		_bSampleTerrainShadow;
};

Texture2D		_TexSceneDepth				: register(t11);
Texture2D		_TexDownsampledSceneDepth	: register(t12);

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_OUT
{
	float3	Scattering	: SV_TARGET0;
	float3	Extinction	: SV_TARGET1;
};

// Read Z from the ZBuffer
float	ReadDepth( float2 _UV )
{
	float	Zproj = _TexSceneDepth.SampleLevel( LinearClamp, _UV, 0.0 ).x;

	float	Q = _CameraData.w / (_CameraData.w - _CameraData.z);	// Zf / (Zf-Zn)
	return (Q * _CameraData.z) / (Q - Zproj);
}

void	ComputeSkyColor( float3 _PositionWorldKm, float3 _View, float _DistanceKm, float3 _Sun, float4 _CloudScatteringExtinction, float _GroundBlocking, float _StepOffset, out float3 _Scattering, out float3 _Extinction )
{
//_PositionWorldKm.y -= _AltitudeOffsetKm;

//_DistanceKm = 1000.0;

	////////////////////////////////////////////////////////////
	// Compute sky radiance arriving at camera, not accounting for clouds
	float3	StartPositionKm = _PositionWorldKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
	float	StartRadiusKm = length( StartPositionKm );
	float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;
	float3	StartNormal = StartPositionKm / StartRadiusKm;
	float	CosThetaView = dot( StartNormal, _View );
	float	CosThetaSun = dot( StartNormal, _Sun );
	float	CosGamma = dot( _View, _Sun );

	float4	Lin_camera2atmosphere = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );
	float3	Lin_camera2atmosphere_Rayleigh = Lin_camera2atmosphere.xyz;
	float3	Lin_camera2atmosphere_Mie = GetMieFromRayleighAndMieRed( Lin_camera2atmosphere );

	////////////////////////////////////////////////////////////
	// Account for obstacle occlusion
	float3	HitPositionKm = _PositionWorldKm + _DistanceKm * _View - EARTH_CENTER_KM;
	float	HitRadiusKm = length( HitPositionKm );
	float	HitAltitudeKm = HitRadiusKm - GROUND_RADIUS_KM;
	float3	HitNormal = HitPositionKm / HitRadiusKm;
	float	HitCosThetaView = dot( HitNormal, _View );
	float	HitCosThetaSun = dot( HitNormal, _Sun );

	// Compute sky radiance arriving at cloud/ground (i.e. above and inside cloud)
	float4	Lin_hit2atmosphere = Sample4DScatteringTable( _TexScattering, HitAltitudeKm, HitCosThetaView, HitCosThetaSun, CosGamma );
	float3	Lin_hit2atmosphere_Rayleigh = Lin_hit2atmosphere.xyz;
	float3	Lin_hit2atmosphere_Mie = GetMieFromRayleighAndMieRed( Lin_hit2atmosphere );

	// Compute sky radiance between camera and hit
	float3	Transmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );

	float3	Lin_camera2hit_Rayleigh = max( 0.0, Lin_camera2atmosphere_Rayleigh - Transmittance * Lin_hit2atmosphere_Rayleigh );
	float3	Lin_camera2hit_Mie = max( 0.0, Lin_camera2atmosphere_Mie - Transmittance * Lin_hit2atmosphere_Mie );

	////////////////////////////////////////////////////////////
	// Rebuild final scattering, accounting for extinction
	float	BlockerExtinction = _GroundBlocking * _CloudScatteringExtinction.w;	// Partialy mask remaining segment

	float3	Lin_Rayleigh = Lin_camera2hit_Rayleigh + BlockerExtinction * Transmittance * Lin_hit2atmosphere_Rayleigh;
	float3	Lin_Mie = Lin_camera2hit_Mie + BlockerExtinction * Transmittance * Lin_hit2atmosphere_Mie;
//	float3	Lin_Rayleigh = Lin_camera2atmosphere_Rayleigh;
//	float3	Lin_Mie = Lin_camera2atmosphere_Mie;
//	float3	Lin_Rayleigh = Lin_camera2hit_Rayleigh;
//	float3	Lin_Mie = Lin_camera2hit_Mie;
//	float3	Lin_Rayleigh = Transmittance * Lin_hit2atmosphere_Rayleigh;
//	float3	Lin_Mie = Transmittance * Lin_hit2atmosphere_Mie;

	// Finalize extinction & scattering
	_Extinction = Transmittance * _CloudScatteringExtinction.w;	// Combine with cloud
	_Scattering = SUN_INTENSITY * (PhaseFunctionRayleigh( CosGamma ) * Lin_Rayleigh + PhaseFunctionMie( CosGamma ) * Lin_Mie) + _CloudScatteringExtinction.xyz;

// _Extinction = 0;
// _Scattering = 4.0 * Transmittance;

}


VS_IN	VS( VS_IN _In )	{ return _In; }

PS_OUT	ReturnTestValue( float3 _Value )
{
	PS_OUT	Out;
	Out.Scattering = Out.Extinction = _Value;
	return Out;
}

PS_OUT	PS( VS_IN _In )
{
	float2	UV = _In.__Position.xy * _dUV.xy;

	// Sample ZBuffer
//	float	SceneZ = ReadDepth( UV );
	float	SceneZ = _TexDownsampledSceneDepth.mips[1][_In.__Position.xy].x;	// Use average Z
	float	GroundBlocking = step( 0.9*_CameraData.w, SceneZ );					// 0 if we hit anything

	float3	ViewCamera = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	View = mul( float4( ViewCamera, 0.0 ), _Camera2World ).xyz;
	float	ViewLength = length( View );
			View /= ViewLength;

	float3	PositionWorld = _Camera2World[3].xyz;
	float3	PositionWorldKm = WORLD2KM * PositionWorld;
	float	GroundHitDistanceKm = WORLD2KM * ViewLength * SceneZ;

//	float	HitDistanceKm = min( GroundHitDistanceKm, SphereIntersectionExit( PositionWorldKm, View, ATMOSPHERE_THICKNESS_KM ) );
//	float	HitDistanceKm = GroundHitDistanceKm;
	float	HitDistanceKm = lerp( GroundHitDistanceKm, SphereIntersectionExit( PositionWorldKm, View, ATMOSPHERE_THICKNESS_KM ), GroundBlocking );

	// Compute sky color & compose with cloud
	PS_OUT	Out;
	ComputeSkyColor( PositionWorldKm, View, HitDistanceKm, _LightDirection, float4( 0, 0, 0, 1 ), GroundBlocking, 0, Out.Scattering, Out.Extinction );


//Out.Scattering = 0;

	return Out;
}
