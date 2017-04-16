//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

Texture2DArray< float4 >	_Tex_GBuffer : register(t0);


float3	MapColor( float3 _wsPosition, float3 _wsNormal, float _materialID ) {

	return _materialID;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Resolution;
	float3	csView = float3( float(_Resolution.x) / _Resolution.y * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
	float	viewLength = length( csView );
			csView /= viewLength;

	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	float4	wsNormal_Distance = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ); 
	float4	wsMaterialID = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ); 

	float3	wsPos = _Camera2World[3].xyz + wsNormal_Distance.w * wsView; 

	return MapColor( wsPos, wsNormal_Distance.xyz, wsMaterialID.x );

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
