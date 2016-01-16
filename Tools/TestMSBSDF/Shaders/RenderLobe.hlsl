#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
	float	_Intensity;
	uint	_ScatteringOrder;
}

Texture2DArray< float >		_Tex_DirectionsHistogram : register( t2 );

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	float3	lsDirection = _In.Position;												// Direction in our local object's Y-up space (which is also our world space BTW)
	float3	mfDirection = float3( lsDirection.x, -lsDirection.z, lsDirection.y );	// Direction in µ-facet Z-up space

	float	theta = acos( clamp( mfDirection.z, -1.0, 1.0 ) );
	float	phi = fmod( 2.0 * PI + atan2( mfDirection.y, mfDirection.x ), 2.0 * PI );

	float	thetaBinIndex = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) );		// Inverse of 2*asin( sqrt( i / (2 * N) ) )
	float2	UV = float2( phi / (2.0 * PI), thetaBinIndex / LOBES_COUNT_THETA );
	float	lobeIntensity = _Tex_DirectionsHistogram.SampleLevel( PointClamp, float3( UV, _ScatteringOrder ), 0.0 );
			lobeIntensity *= LOBES_COUNT_THETA * LOBES_COUNT_PHI;
			lobeIntensity *= _Intensity;
			lobeIntensity = max( 0.01, lobeIntensity );

	float3	lsPosition = lobeIntensity * lsDirection;

	PS_IN	Out;
	Out.__Position = mul( float4( lsPosition, 1.0 ), _World2Proj );
	Out.Color = 0.1 * lobeIntensity;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
//	return float3( 1, 0, 0 );
	return _In.Color;
}
