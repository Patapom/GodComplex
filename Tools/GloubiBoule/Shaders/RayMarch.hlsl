#include "Includes/global.hlsl"
#include "Includes/Photons.hlsl"
#include "Includes/Room.hlsl"
#include "Includes/VolumeDensity.hlsl"

static const uint	STEPS_COUNT = 128;

cbuffer CB_RayMarch : register(b3) {
	float	_Sigma_t;
	float	_Sigma_s;
	float	_Phase_g;
};


struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float3	Scattering : SV_TARGET0;
	float3	Transmittance : SV_TARGET1;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

Texture3D< uint >		_Tex_PhotonAccumulator : register(t1);

PS_OUT	PS( VS_IN _In ) {
	float2	UV = _In.__Position.xy * _ScreenSize.zw;

	// Build camera ray
	float3	csView = float3( ASPECT_RATIO * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	float	stepOffset = rand( _In.__Position.x + 13249.7 * _In.__Position.y );

	PS_OUT	Out;
	Out.Scattering = 0.0;
	Out.Transmittance = 1.0;

	float4	wsPos = float4( _Camera2World[3].xyz, 0.0 );
	float4	wsDir = 0.1 * float4( wsView, 1.0 );

	wsPos += stepOffset * wsDir;
	float	previousDensity;
	float	density = 0.0;
	for ( uint stepIndex=0; stepIndex < STEPS_COUNT; stepIndex++ ) {
		previousDensity = density;
		float	density = SampleVolumeDensity( wsPos.xyz );

		// Compute local lighting
		float	opticalDepth = exp( -_Sigma_t * density * wsDir.w );

		float3	wsLight = ROOM_CENTER - wsPos.xyz;
		float	lightDistance = length( wsLight );
		float	invLightDistance = 1.0 / lightDistance;
				wsLight *= invLightDistance;
		float	Phase = PhaseFunctionMie( dot( wsLight, wsView ), _Phase_g );

		float3	Radiance = 10.0 * invLightDistance * invLightDistance * float3( 1, 0.8, 0.4 );

//float	PhotonPower = _Tex_PhotonAccumulator.SampleLevel( LinearClamp, World2RoomUVW( wsPos.xyz ), 0.0 ) / 65536.0;
float	PhotonPower = _Tex_PhotonAccumulator[World2RoomCellIndex( wsPos.xyz )] / 65536.0;
Radiance = 0.1 * PhotonPower;

		float3	Scat = Radiance * _Sigma_s * Phase * wsDir.w;
				Scat += 0.001 * float3( 0.03, 0.3, 0.9 );


		Out.Scattering += Out.Transmittance * Scat;
		Out.Transmittance *= opticalDepth;

		wsPos += wsDir;
	}


//Out.Scattering = wsView;//float3( UV, 1 );
//Out.Scattering = _TexNoise.Sample( LinearWrap, float3( UV, 0.25 ) ).x;
//Out.Transmittance = 0.0;

	return Out;
}
