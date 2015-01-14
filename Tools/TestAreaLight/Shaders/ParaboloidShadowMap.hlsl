
static const float	SHADOW_ZFAR = 100.0				// Should be camera Z far
								* sqrt(2.0);


Texture2D< float >	_TexShadowMap : register(t2);
Texture2D< float2 >	_TexShadowSmoothie : register(t3);

float	ComputeShadow( float3 _wsPosition, float3 _wsNormal, out float4 _Debug ) {

	// Transform into area light space
	float3	lsDeltaPos = _wsPosition - _AreaLightT;
	float3	lsPosition = float3(	dot( lsDeltaPos, _AreaLightX ),
									dot( lsDeltaPos, _AreaLightY ),
									dot( lsDeltaPos, _AreaLightZ ) );

	// Apply paraboloid projection
	float	ReceiverDistance = length( lsPosition );
	float3	lsDirection = lsPosition / ReceiverDistance;

	float2	projPosition = lsDirection.xy / (1.0 + lsDirection.z);

	// Sample distance
	float2	UV = float2( 0.5 * (1.0 + projPosition.x), 0.5 * (1.0 - projPosition.y) );
	float	BlockerDistance = SHADOW_ZFAR * _TexShadowMap.SampleLevel( LinearClamp, UV, 0.0 );

// _Debug = float4( UV, 0, 0 );
// _Debug = SHADOW_ZFAR * _TexShadowMap.SampleLevel( LinearClamp, UV, 0.0 );
_Debug = saturate( dot( -lsDeltaPos / ReceiverDistance, _wsNormal ) ) *  step( ReceiverDistance, BlockerDistance );

	float	Shadow = step( ReceiverDistance, BlockerDistance );	// 1 if we're in front of the blocker
// 	if ( Shadow > 0.5 ) 
// 		return 1.0;

	// Smooth the shadow out
	float2	Smoothie = _TexShadowSmoothie.SampleLevel( LinearClamp, UV, 0.0 );

	float	ExpandedBlockerDistance = SHADOW_ZFAR * Smoothie.y;
//	float	Sharpness = 1.0 - saturate( ReceiverDistance / ExpandedBlockerDistance );
	float	Sharpness = saturate( ExpandedBlockerDistance / ReceiverDistance );
			Smoothie.x = saturate( Smoothie.x / (1.0 - Sharpness) );

_Debug = Smoothie.x;
//_Debug = Sharpness;

	return saturate( Smoothie.x * Shadow );//+ dot( -lsDeltaPos / ReceiverDistance, _wsNormal ) );
}