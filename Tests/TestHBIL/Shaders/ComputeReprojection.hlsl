////////////////////////////////////////////////////////////////////////////////
// Shaders to reproject radiance from last frame
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define THREADS_X	16
#define THREADS_Y	16

Texture2D< float4 >		_tex_sourceRadiance : register(t0);
Texture2D< float >		_tex_depth : register(t1);			// Depth or distance buffer (here we're given depth)
Texture2D< float3 >		_tex_motionVectors : register(t2);	// Motion vectors in camera space

RWTexture2D< float4 >	_tex_reprojectedRadiance : register(u0);


////////////////////////////////////////////////////////////////////////////////
// Reprojects last frame's radiance
[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_Reproject( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint2	pixelPosition = _dispatchThreadID.xy;
	float2	UV = float2( pixelPosition + 0.5 ) / _resolution;

	// Compute previous camera-space position
	float	previousZ = Z_FAR * _tex_depth[pixelPosition];
	float3	csView = BuildCameraRay( UV );
//	float	Z2Distance = length( csView );
	float3	csPreviousPosition = csView * previousZ;
			csPreviousPosition += _deltaTime * _tex_motionVectors[pixelPosition];	// Extrapolate new position using last frame's camera-space velocity

	// Re-project
	float3	csNewPosition = mul( float4( csPreviousPosition, 1.0 ), _PrevioucCamera2CurrentCamera ).xyz;
	float	newZ = csNewPosition.z;
			csNewPosition.xy /= csNewPosition.z;
	float2	newUV = 0.5 * (1.0 + float2( csNewPosition.x / (TAN_HALF_FOV * _resolution.x / _resolution.y), csNewPosition.y / -TAN_HALF_FOV ));
	int2	newPixelPosition = floor( newUV * _resolution );

//newPixelPosition = pixelPosition;

	if ( any( newPixelPosition < 0 ) || any( newPixelPosition >= _resolution ) )
		return;	// Off screen..

	float3	previousRadiance = _tex_sourceRadiance[pixelPosition].xyz;

//previousRadiance = float3( UV, 0 );
//previousRadiance = float3( newUV, 0 );
//previousRadiance = float3( 1, 0, 1 );

	_tex_reprojectedRadiance[newPixelPosition] = float4( previousRadiance, newZ );	// Store depth in alpha (will be used for bilateral filtering later)
}

////////////////////////////////////////////////////////////////////////////////
