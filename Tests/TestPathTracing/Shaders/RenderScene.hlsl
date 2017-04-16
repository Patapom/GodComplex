//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

Texture2DArray< float4 >	_Tex_GBuffer : register(t0);
Texture2D< float4 >			_Tex_Wall : register(t1);
Texture2D< float4 >			_Tex_BlueNoise : register(t2);

float3	MapColor( float3 _wsPosition, float3 _wsNormal, float _materialID ) {
	float3	color;
	if ( _materialID < 0.5 ) {
		// Map wall color depending on normal
		float2	UV = 0.5 * (1.0 + _wsPosition.xy);
//		color = float3( UV, 0 );
		color = 1.0 * _Tex_Wall[uint2( 64.0 * UV )].xyz;

// TODO: Tweak color depending on normal (invert blue and green apparently)

	} else {
		// Map sphere color depending on height
		float	V = 0.5 * (1.0 + (_wsPosition.y - SPHERE_CENTER.y) * (1.0 / SPHERE_RADIUS));
//		color = V;
		const float	BANDS_COUNT = 20;
		V *= BANDS_COUNT;
		float	bandIndex = floor( V );
		float	Vband = 2.0 * frac( V ) - 1.0;
				Vband = sqrt( 1.0 - Vband*Vband );
		float	intensity = abs( sin( 4.0 * _Time + bandIndex * sin( _Time ) ) ) * Vband;
		float3	bandColor = frac( float3( 13.289 * bandIndex, 0.9 - 3.18949 * bandIndex, 17.0 * bandIndex ) );
				bandColor = 0.25 + 0.75 * bandColor;
		color = intensity * bandColor;
	}
	return 2.0 * color;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Resolution;
	float	aspectRatio = float(_Resolution.x) / _Resolution.y;

	float	noise = _NoiseInfluence * _Tex_BlueNoise[uint2( _In.__Position.xy ) & 0x3F].x;
//return _Tex_BlueNoise.SampleLevel( LinearWrap, 10.0 * float2( aspectRatio * UV.x, UV.y ), 0.0 ).x;

	float3	csView = float3( aspectRatio * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
	float	viewLength = length( csView );
			csView /= viewLength;

	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	float4	wsNormal_Distance = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ); 
	float4	wsMaterialID_Roughness = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ); 

	// Compute hit position
	float3	wsPos = _Camera2World[3].xyz + wsNormal_Distance.w * wsView; 

	// Build tangent space
	float3	wsNormal = wsNormal_Distance.xyz;
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );
//return cross( wsTangent, wsBiTangent );
//return wsPos;

	///////////////////////////////////////////////////////////////////
	// Compute emissive color
	float3	emissive = MapColor( wsPos, wsNormal, wsMaterialID_Roughness.x );

	///////////////////////////////////////////////////////////////////
	// Importance sample specular distribution
	const uint	SAMPLES_COUNT = 512;

	float	alpha = wsMaterialID_Roughness.y;

	float3	specular = 0.0;

	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		// Generate random half vector
		float	X0 = float(i) / SAMPLES_COUNT;
		float	X1 = ReverseBits( i );
		float	phi = 2.0 * PI * (X0 + noise);
//		float	phi = 2.0 * PI * (X0);
		float2	sinCosPhi;
		sincos( phi, sinCosPhi.x, sinCosPhi.y );

		float	sqrCosTheta = (1.0 - X1) / ((alpha*alpha - 1.0) * X1 + 1.0);
		float	cosTheta = sqrt( sqrCosTheta );
		float	sinTheta = sqrt( 1.0 - sqrCosTheta );

		float3	lsHalf = float3( sinTheta * sinCosPhi.y, sinTheta * sinCosPhi.x, cosTheta );

		// Generate world-space light ray
		float3	wsHalf = lsHalf.x * wsTangent + lsHalf.y * wsBiTangent + lsHalf.z * wsNormal;
		float3	wsLight = wsView - 2.0 * dot( wsHalf, wsView ) * wsHalf;

		// Intersect scene in light direction
		float2	d = Map( wsPos, wsLight );
		float3	wsSceneHitPos = wsPos + d.x * wsLight;
		float3	wsSceneNormal = 0.0;	// !!!!!TODO!!!!!
		float3	sceneColor = 10.0 * MapColor( wsSceneHitPos, wsSceneNormal, d.y );
//		color += ComputeLighting( wsPos, -wsView, wsLight, wsNormal, wsTangent, wsBiTangent );

		// Compute Fresnel
		const float	F0 = 0.04;	// Dielectric
		float	F = FresnelSchlick( F0, cosTheta );

//		float	k = alpha * sqrt( X1 ) / sqrt( 1.0 - X1 );
//		float	Xm = k * sinCosPhi.x;
//		float	Ym = k * sinCosPhi.y;

		specular += sceneColor * F0;
	}
	specular /= SAMPLES_COUNT;

	return emissive + specular;
	return MapColor( wsPos, wsNormal_Distance.xyz, wsMaterialID_Roughness.x );

return _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;
return 0.2 * _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).w;
return _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).xyz;

//	float3	csView = float3( float(_Resolution.x) / _Resolution.y * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
////return float3( csView.xy, 0 );
//	float	viewLength = length( csView );
//			csView /= viewLength;
//
//	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
//	float3	wsPos = _Camera2World[3].xyz;
//
////	float2	bisou = IntersectBox( wsPos, wsView );
//	float2	bisou = Map( wsPos, wsView ).x;
//return 0.2 * bisou.x;
//return wsView;
//
	return float3( UV, 0 );
//
//	float2	UV = _In.__Position.xy / _Resolution.xy;
//	float3	OriginalColor = _texHDR.SampleLevel( LinearClamp, UV, 0.0 ).xyz;
//
//
//// Debug tall histogram
//// 	uint2	PixPos = uint2( 0.4 * _In.__Position.xy );
//// 	if ( PixPos.x < 128 && PixPos.y < 202 )
//// 		return 0.01 * _texTallHistogram[PixPos];
//
//
//	// Apply auto-exposure
//	autoExposure_t	currentExposure = ReadAutoExposureParameters();
//	float3	Color = OriginalColor * currentExposure.EngineLuminanceFactor;
//
//	// Apply tone mapping
//	if ( _Flags & 1 ) {
//
//		if ( _Flags & 8 )
//			Color = max( 0.0, ToneMappingFilmic_Hable( Color ) / max( 1e-3, ToneMappingFilmic_Hable( _WhitePoint ) ) );
//		else if ( _Flags & 16 ) {
////			float	Lum = dot( Color, LUMINANCE );
//			float	Lum = max( max( Color.x, Color.y ), Color.z );
//			Color /= Lum;
//			Lum = ToneMappingFilmic_Insomniac( Lum );
//			Color = saturate( Lum * Color );
//		} else {
//			Color = saturate( ToneMappingFilmic_Insomniac( Color ) );
////			Color = max( 0.0, ToneMappingFilmic_Insomniac( Color ) / ToneMappingFilmic_Insomniac( _WhitePoint ) );
//		}
//
////		Color = Sigmoid( 1.0 * Color );
//		
//// 		// Try darkening saturated colors
//// //		Color = saturate( Color );
//// 		float	MinRGB = min( min( Color.x, Color.y ), Color.z );
//// 		float	MaxRGB = max( max( Color.x, Color.y ), Color.z );
//// 		float	L = 0.5 * (MinRGB + MaxRGB);
//// //		float	S = (MaxRGB - MinRGB) / (1.00001 - abs(2*L-1));
//// 		float	S = (MaxRGB - MinRGB) / MaxRGB;
//// 
//// 		Color *= 1.0 - _A * pow( abs( S ), _B );
//// //		Color = S;
// 	}
//
//	if ( _Flags & 2 ) {
//		// Show debug luminance value
//		float	Color_WorldLuma = BISOU_TO_WORLD_LUMINANCE * dot( OriginalColor, LUMINANCE );
//		float	Color_dB = Luminance2dB( Color_WorldLuma );
//		float	Debug_dB = Luminance2dB( _DebugLuminanceLevel );
//		if ( abs( Color_dB - Debug_dB ) < 0.4 ) {
//			uint2	pixelIndex = uint2( floor( 0.25 * _In.__Position.xy + 4.0 * _GlobalTime ) );
//			bool	checker = (pixelIndex.x & 1) ^ (pixelIndex.y & 1);
//			Color = checker ? float3( 1, 0, 0 ) : float3( 0, 0, 1 );
//		}
//	}
//	
//	// Show debug histogram
//	if ( _Flags & 4 ) {
//		DEBUG_DisplayLuminanceHistogram( _WhitePoint, UV, float2( _MouseU, _MouseV ), (_Flags & 2) ? _DebugLuminanceLevel : 0.0001, _Resolution.xy, _GlobalTime, Color, OriginalColor );
//
//		// Debug curves
//		// if ( UV.x < 0.4 && UV.y > 0.75 ) {
//		// 	UV.x /= 0.4;
//		// 	UV.y = (UV.y - 0.75) / 0.25;
//		// 	float	LumaLDR = 1.0 - UV.y;
//		// 	float	LumaHDR = 10.0 * UV.x;
//		// 	float3	ToneMappedColor = (_Flags & 8) ? (ToneMappingFilmic_Hable( LumaHDR ) / max( 1e-3, ToneMappingFilmic_Hable( _WhitePoint ) )) : ToneMappingFilmic_Insomniac( LumaHDR );
//		// 
//		// 	Color.x = ToneMappedColor.x < LumaLDR ? 1 : 0;
//		// 	Color.y = ToneMappedColor.y < LumaLDR ? 1 : 0;
//		// 	Color.z = ToneMappedColor.z < LumaLDR ? 1 : 0;
//		// }
//	}
//
//	return Color;
}
