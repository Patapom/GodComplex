
// static const float	SHADOW_ZFAR = 100.0				// Should be camera Z far
// 								* sqrt(2.0);
// 
static const float	EXP_CONSTANT = 80.0;

cbuffer CB_ShadowMap : register(b3) {
	float2		_ShadowOffsetXY;			// XY Offset in [-1,+1] depending on where to place the shadow source
	float2		_ShadowZFar;				// X=Far clip distance for the shadow, Y=1/X
	float		_InvShadowMapSize;			// 1/Size of the shadow map
	float		_KernelSize;				// Size of the filtering kernel
	float2		_ShadowHardeningFactor;		// Hardening factor for the sigmoïd
};

Texture2D< float >	_TexShadowMap : register(t2);
Texture2D< float2 >	_TexShadowSmoothie : register(t3);

#if 0
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
	
//Shadow = step( ReceiverDistance, ExpandedBlockerDistance );	// 1 if we're in front of the blocker


//	float	Sharpness = 1.0 - saturate( ReceiverDistance / ExpandedBlockerDistance );
	float	Sharpness = pow( saturate( ExpandedBlockerDistance / ReceiverDistance ), 4.0 );
//			Smoothie.x = saturate( Smoothie.x / (1.0 - Sharpness) );
			Smoothie.x = lerp( Smoothie.x, 1.0, Sharpness );

//Smoothie.x = 1;
_Debug = Smoothie.x;
//_Debug = Sharpness;

	return saturate( Smoothie.x * Shadow );//+ dot( -lsDeltaPos / ReceiverDistance, _wsNormal ) );
}
#else
// Exponential shadow mapping
float	ComputeShadow( float3 _wsPosition, float3 _wsNormal, out float4 _Debug ) {

	// Transform into area light space
	float3	lsDeltaPos = _wsPosition - _AreaLightT;
	float3	lsPosition = float3(	(dot( lsDeltaPos, _AreaLightX ) / _AreaLightScaleX) + _ShadowOffsetXY.x,
									(dot( lsDeltaPos, _AreaLightY ) / _AreaLightScaleY) + _ShadowOffsetXY.y,
									dot( lsDeltaPos, _AreaLightZ ) );

	// Apply paraboloid projection
	float	ReceiverDistance = length( lsPosition );
	float3	lsDirection = lsPosition / ReceiverDistance;

	float2	projPosition = lsDirection.xy / (1.0 + lsDirection.z);
	float2	UV = float2( 0.5 * (1.0 + projPosition.x), 0.5 * (1.0 - projPosition.y) );

	// Sample exp( -c.z )
	float	Exp_BlockerDistance = _TexShadowMap.SampleLevel( LinearClamp, UV, 0.0 );		// exp( -EXP_CONSTANT * Distance/SHADOW_ZFAR )

_Debug = 0;

	// Compute sigmoïd
	float	Exp_ReceiverDistance = exp( _ShadowHardeningFactor.x * ReceiverDistance * _ShadowZFar.y );
//	float	Exp_ReceiverDistance = exp( _ShadowHardeningFactor.y * ReceiverDistance * _ShadowZFar.y );

//return 0.5 + 100.0 * ReceiverDistance * _ShadowZFar.y / log( Exp_BlockerDistance );

//	float	Exp_ReceiverDistance = exp( -_ShadowHardeningFactor * (ReceiverDistance * _ShadowZFar.y - 1.0) );
	float	Shadow = 1.0 / (1.0 + Exp_ReceiverDistance * Exp_BlockerDistance);		// Sigmoïd
//	float	Shadow = 1.0 - saturate( Exp_ReceiverDistance * Exp_BlockerDistance );	// Standard ESM
	return Shadow;
}
#endif