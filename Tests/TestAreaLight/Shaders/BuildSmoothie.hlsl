#include "Global.hlsl"
#include "AreaLight.hlsl"
#include "ParaboloidShadowMap.hlsl"

Texture2D< float2 >	_TexShadowDistance : register(t0);

static const float	MAX_DISTANCE = 32.0;

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float2	PS_Edge( VS_IN _In ) : SV_TARGET0 {

	float3	dUV = float3( 1.0.xx / 512.0, 0.0 );
	float2	UV = _In.__Position.xy * dUV.xy;

	UV -= dUV.xy;
	float	Z00 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV += dUV.xz;
	float	Z01 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV += dUV.xz;
	float	Z02 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV += dUV.zy;
	float	Z12 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV -= dUV.xz;
	float	Z11 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV -= dUV.xz;
	float	Z10 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV += dUV.zy;
	float	Z20 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV += dUV.xz;
	float	Z21 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );	UV += dUV.xz;
	float	Z22 = _TexShadowMap.SampleLevel( PointClamp, UV, 0.0 );

	float3	dX = float3( 2.0 * dUV.x, 0.0, Z12 - Z10 );
	float3	dY = float3( 0.0, 2.0 * dUV.y, Z21 - Z01 );
	float3	Normal = normalize( cross( dX, dY ) );

	float	Z0 = min( min( Z00, Z01 ), Z02 );
	float	Z1 = min( min( Z10, Z11 ), Z12 );
	float	Z2 = min( min( Z20, Z21 ), Z22 );

	return float2( saturate( step( Normal.z, 0.01 ) + step( Z11, 0.999 ) ), min( min( Z0, Z1 ), Z2 ) );
}

float2	PS_DistanceFieldH( VS_IN _In ) : SV_TARGET0 {

	float3	dUV = float3( 1.0.xx / 512.0, 0.0 );
	float2	UV = _In.__Position.xy * dUV.xy;

	float2	EdgeDepth = _TexShadowSmoothie.SampleLevel( PointClamp, UV, 0.0 );
	float2	Distance = float2( MAX_DISTANCE * step( EdgeDepth.x, 0.5 ), EdgeDepth.y );

	// Check left and right for an edge
	float2	UVl = UV;
	float2	UVr = UV;
	for ( float i=0; i < Distance.x; i++ ) {
		UVl -= dUV.xz;
		UVr += dUV.xz;

		float2	SmoothieL = _TexShadowSmoothie.SampleLevel( PointClamp, UVl, 0.0 );
		if ( SmoothieL.x > 0.5 && SmoothieL.y <= Distance.y ) {
			Distance = float2( i, SmoothieL.y );
		}
		float2	SmoothieR = _TexShadowSmoothie.SampleLevel( PointClamp, UVr, 0.0 );
		if ( SmoothieR.x > 0.5 && SmoothieR.y <= Distance.y ) {
			Distance = float2( i, SmoothieR.y );
		}
	}

	return Distance;
}

float2	PS_DistanceFieldV( VS_IN _In ) : SV_TARGET0 {

	float3	dUV = float3( 1.0.xx / 512.0, 0.0 );
	float2	UV = _In.__Position.xy * dUV.xy;

// return _TexShadowDistance.SampleLevel( PointClamp, UV, 0.0 );

	const float	eps = 1e-3;

	// Check top and bottom for an edge
	float2	sqDistance = float2( 32.0*32.0, 1.0 );
	float2	UVt = UV;
	float2	UVb = UV;
	for ( uint i=0; i < 32; i++ ) {
		UVt -= dUV.zy;
		UVb += dUV.zy;
		
		float2	Dt = _TexShadowDistance.SampleLevel( PointClamp, UVt, 0.0 );
		float2	Db = _TexShadowDistance.SampleLevel( PointClamp, UVb, 0.0 );

// 		float	sqDt = i*i + Dt*Dt;
// 		float	sqDB = i*i + Db*Db;
// 		sqDistance = min( sqDistance, sqDt );
// 		sqDistance = min( sqDistance, sqDB );

		float	sqDt = i*i + Dt.x*Dt.x;
		float	sqDB = i*i + Db.x*Db.x;
		if ( sqDt < sqDistance.x && Dt.y <= sqDistance.y+eps ) {
			sqDistance = float2( sqDt, Dt.y );
		}
		if ( sqDB < sqDistance.x && Db.y <= sqDistance.y+eps ) {
			sqDistance = float2( sqDB, Db.y );
		}
	}

	return float2( lerp( sqrt( sqDistance.x ) / MAX_DISTANCE, 1.0, step( sqDistance.x, -10.0 ) ), sqDistance.y );
}
