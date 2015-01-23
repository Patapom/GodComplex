#include "Global.hlsl"
#include "AreaLight2.hlsl"
#include "ParaboloidShadowMap.hlsl"

cbuffer CB_Object : register(b4) {
	float4x4	_Local2World;
};

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.UV = _In.UV;

	return Out;
}

float4	SampleSATSinglePixel( float2 _UV ) {
	
	float2	PixelIndex = _UV * _AreaLightTexDimensions.xy;
	float2	NextPixelIndex = PixelIndex + 1;
	float2	UV2 = NextPixelIndex * _AreaLightTexDimensions.zw;

	float3	dUV = float3( _AreaLightTexDimensions.zw, 0.0 );
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV );
	float4	C01	= _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xz );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.zy );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xy );

	return C11 - C10 - C01 + C00;
}


float3	ClipSegmentBLI( float3 _P0, float3 _P1, float3 _PlanePosition, float3 _PlaneNormal, out float4 _Debug ) {

	float3	V = _P1 - _P0;
	float3	D = _PlanePosition - _P0;
	float	d = dot( D, _PlaneNormal );
	float	t = d / dot( V, _PlaneNormal );

_Debug = t;

	float	IsPositive_d = step( 0.0, d );			// d > 0
	float	IsPositive_t = 1.0 - step( t, 0.0 );	// t >= 0
	float	d_or_t = saturate( IsPositive_d + IsPositive_t );
	return lerp( _P1, _P0 + saturate( t - 1e-3 ) * V, d_or_t );

	if ( d > 0.0 || t >= 0.0 )
		return _P0 + saturate( t - 1e-3 ) * V;	// There's an intersection
	else
		return _P1;						// Both points are above the plane, go straight to end point

// 	if ( t < 0.0 ) {
// 		if ( d > 0.0 )
// 			return _P0;					// Both points are below the plane, don't move from start point
// 		else
// 			return _P1;					// Both points are above the plane, go straight to end point
// 	} else
// 		return _P0 + saturate( t ) * V;	// There's an intersection
}

// Computes the potential UV clipping by the surface's normal
// We simplify *a lot* by assuming either a vertical or horizontal normal that clearly cuts the square along one of its main axes
//
float4	ComputeClippingBLI( float3 _lsPosition, float3 _lsNormal, out float4 _Debug ) {

_Debug = 0;

float4	Debug;

// 	float3	P0 = float3( -1.0, -1.0, 0.0 );
// 	float3	P1 = float3( -1.0, +1.0, 0.0 );
// 	float3	P2 = float3( +1.0, +1.0, 0.0 );
// 	float3	P3 = float3( +1.0, -1.0, 0.0 );

	_lsPosition.y = -_lsPosition.y;
	_lsNormal.y = -_lsNormal.y;

//	float	PrincipalAxis = step( abs( _lsNormal.y ), abs( _lsNormal.x ) );	// 1=X, 0=Y
	float	X0 = 1.0 - 2.0 * step( _lsNormal.x, 0.0 );
	float	Y0 = 1.0 - 2.0 * step( _lsNormal.y, 0.0 );

	float3	P0 = float3( X0, Y0, 0.0 );
	float3	P1 = float3( -X0, Y0, 0.0 );
	float3	P2 = float3( -X0, -Y0, 0.0 );
	float3	P3 = float3( X0, -Y0, 0.0 );

// _Debug = float4( 0.5*(1.0+P3.xy), 0, 0 );
// return 0.0;

	// Compute clipping of the square by browsing the contour in both CCW and CW
	// We keep the min/max of UVs each time
	float4	MinMax = float4( 1.0, 1.0, -1.0, -1.0 );

	// CCW
	float3	P = ClipSegmentBLI( P0, P1, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

//_Debug = float4( 0.5 * (1.0 + float2( P.x, -P.y )), 0, 0 );
_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );
// _Debug = Debug;

	P = ClipSegmentBLI( P, P2, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );
//_Debug = float4( _lsNormal.xy, 0, 0 );
//_Debug = Debug;

	P = ClipSegmentBLI( P, P3, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegmentBLI( P, P0, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	// CW
	P = ClipSegmentBLI( P0, P3, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegmentBLI( P, P2, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegmentBLI( P, P1, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegmentBLI( P, P0, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	// Finalize UVs
//	MinMax.yw = -MinMax.yw;
	return 0.5 * (1.0 + MinMax);
}

float4	PS( PS_IN _In ) : SV_TARGET0 {

return float4( _TexBRDFIntegral.Sample( LinearClamp, _In.UV ), 0, 1 );

// 	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
	float4	StainedGlass = SampleSATSinglePixel( _In.UV );

// 	if ( all( abs( 2.0 * _In.UV - 1.0 ) > 0.9 ) )
// 		return float4( _In.UV, 0, 0 );


// 	float4	StainedGlass = _TexAreaLight.SampleLevel( LinearClamp, _In.UV, 10.0 * (0.5 * (1.0 + sin( iGlobalTime ))) );

	StainedGlass *= _AreaLightIntensity;

// Debug shadow map
//StainedGlass = 1.0 * _TexShadowMap.Sample( LinearClamp, _In.UV );
// StainedGlass = float4( _TexShadowMap.SampleLevel( LinearClamp, _In.UV, 0.0 ).x - _TexShadowSmoothie.Sample( LinearClamp, _In.UV ), 0, 0 ).y;
//StainedGlass = float4( _TexShadowSmoothie.Sample( LinearClamp, _In.UV ), 0, 0 ).x;
//StainedGlass = 20.0 * float4( _TexShadowSmoothie.Sample( LinearClamp, _In.UV ), 0, 0 ).y;

// 	// Debug UV clipping
// 	float3	wsPosition = float3( 0, 0, 0 );
// 	float3	wsNormal = float3( 0, 1, 0 );
// 
// 	float3	wsCenter2Position = wsPosition - _AreaLightT;
// 	float3	lsPosition = float3(	dot( wsCenter2Position, _AreaLightX ),	// Transform world position in local area light space
// 									dot( wsCenter2Position, _AreaLightY ),
// 									dot( wsCenter2Position, _AreaLightZ ) );
// 	lsPosition.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );			// Account for scale
// 	float3	lsNormal = float3(	dot( wsNormal, _AreaLightX ),				// Transform world normal in local area light space
// 								dot( wsNormal, _AreaLightY ),
// 								dot( wsNormal, _AreaLightZ ) );
// 
// 	float4	Debug;
// 	float4	ClippedUVs = ComputeClippingBLI( lsPosition, lsNormal, Debug );
// 	StainedGlass.xyz = (_In.UV.x < ClippedUVs.x || _In.UV.y < ClippedUVs.y || _In.UV.x > ClippedUVs.z || _In.UV.y > ClippedUVs.w) ? float3( 0.2, 0, 0.2 ) : float3( _In.UV, 0 );

//return Debug;
//return float4( ClippedUVs.zw, 0, 0 );


	return float4( StainedGlass.xyz, 1 );
}
