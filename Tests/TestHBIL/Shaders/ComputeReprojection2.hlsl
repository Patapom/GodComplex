////////////////////////////////////////////////////////////////////////////////
// Shaders to reproject radiance from last frame
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define THREADS_X	16
#define THREADS_Y	16

#define PIXEL_SHIFT			2
#define PIXEL_STRIDE		(1 << PIXEL_SHIFT)
#define USE_NOISE_JITTER	0

Texture2D< float4 >		_tex_sourceRadiance : register(t0);
Texture2D< float >		_tex_depth : register(t1);			// Depth or distance buffer (here we're given depth)
Texture2D< float3 >		_tex_motionVectors : register(t2);	// Motion vectors in camera space
Texture2D< float >		_tex_blueNoise : register(t3);

Texture2D< float4 >		_tex_sourceRadianceCurrentMip : register(t3);
RWTexture2D< float4 >	_tex_reprojectedRadiance : register(u0);

cbuffer CB_PushPull : register( b3 ) {
	uint2	_targetSize;
	float2	_bilateralDepths;
	float	_preferedDepth;
};

////////////////////////////////////////////////////////////////////////////////
// Reprojects last frame's radiance
[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_Reproject( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	#if PIXEL_SHIFT > 0
		#if USE_NOISE_JITTER
			uint	noise1D = PIXEL_STRIDE*PIXEL_STRIDE * _tex_blueNoise[_dispatchThreadID.xy & 0x3F];
			uint2	noise2D = uint2( noise1D & (PIXEL_STRIDE-1), noise1D >> PIXEL_SHIFT );
			uint2	pixelPosition = PIXEL_STRIDE * _dispatchThreadID.xy + noise2D;
		#else
			uint2	pixelPosition = PIXEL_STRIDE * _dispatchThreadID.xy;
		#endif
	#else
		uint2	pixelPosition = _dispatchThreadID.xy;
	#endif
	if ( any( pixelPosition >= uint2(_resolution) ) )
		return;

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

	_tex_reprojectedRadiance[newPixelPosition] = float4( previousRadiance, newZ );	// Store depth in alpha (will be used for bilateral filtering later)
}

////////////////////////////////////////////////////////////////////////////////
// Implements the PUSH phase of the push/pull algorithm described in "The Pull-Push Algorithm Revisited" M. Kraus (2009)
// https://pdfs.semanticscholar.org/9efe/2c33d8db1609276b989f569b66f1a90feaca.pdf
// Actually, this is called the "pull" phase in the original algorithm but I prefer thinking of it as pushing valid values down to the lower mips...
//
// The idea is basically to compute the next mip level using only valid pixels from current level
// A valid pixel is a non-empty pixel but we also account for depth difference to perform a depth-aware filter
//

// Returns a very weak weight if depths are deemed un-interesting (too close or too far away) but never 0 (0 is reserved for uninitialized values)
float	Depth2Weight( float _depth ) {
	return step( 1e-3, _depth );
//	return _depth < 1e-3 ? 0.0	// Keep uninitialized values as invalid
//						 : lerp( 0.001, 1.0, smoothstep( 0.0, 1.0, _depth ) * smoothstep( 100.0, 40.0, _depth ) );	// Otherwise, we maximize the weights of samples whose depth is between 1 and 40 meters
}

// Returns 0 if the depths are too far appart from each other...
float	BilateralWeight( float _depth0, float _depth1 ) {
	float	dZ = _depth0 - _depth1;
	return 1e-3 + smoothstep( _bilateralDepths.y, _bilateralDepths.x, dZ * dZ );
}

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_Push( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint2	targetPixelIndex = _dispatchThreadID.xy;
	if ( any( targetPixelIndex > _targetSize ) )
		return;

	// Fetch the 4 source pixels
	uint2	sourcePixelIndex = targetPixelIndex << 1;
	float4	V00 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.x++;
	float4	V10 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.y++;
	float4	V11 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.x--;
	float4	V01 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.y--;

	// Compute depth weight
	// Will return 0 for invalid pixels and low values for "non important depths"
	float	w00 = Depth2Weight( V00.w );
	float	w10 = Depth2Weight( V10.w );
	float	w11 = Depth2Weight( V11.w );
	float	w01 = Depth2Weight( V01.w );
	float	sumWeights = w00 + w10 + w01 + w11;

	// Compute average depth by harmonic mean with offset
	// averageZ = 1 / Sum( 1 / (offset+Z) ) - offset
	float	meanOffset = _preferedDepth;
	float	avgZ = w00 / (meanOffset + V00.w)
				 + w10 / (meanOffset + V10.w)
				 + w01 / (meanOffset + V01.w)
				 + w11 / (meanOffset + V11.w);
			avgZ = avgZ > 0.0 ? sumWeights / avgZ - meanOffset : 0.0;

	// Compute bilateral weights that yields 0 if the depth vary too much from average depth
	w00 *= BilateralWeight( V00.w, avgZ );
	w10 *= BilateralWeight( V10.w, avgZ );
	w01 *= BilateralWeight( V01.w, avgZ );
	w11 *= BilateralWeight( V11.w, avgZ );

	// Pre-multiply colors by weights
	float3	C = w00 * V00.xyz
			  + w10 * V10.xyz
			  + w01 * V01.xyz
			  + w11 * V11.xyz;

	sumWeights = w00 + w10 + w01 + w11;
	C *= sumWeights > 0.0 ? saturate( sumWeights ) / sumWeights : 0.0;	// Store un-premultiplied color

//if ( sumWeights < 1e-3 && avgZ > 1e-3 )
//	C = float3( 1, 0, 1 );	// Can it happen that we nullified all colors because of invalid weights although we have valid colors in the lot???

	_tex_reprojectedRadiance[targetPixelIndex] = float4( C, avgZ );
}

////////////////////////////////////////////////////////////////////////////////
// Implements the PULL phase of the push/pull algorithm described in "The Pull-Push Algorithm Revisited" M. Kraus (2009)
// Actually, this is called the "push" phase in the original algorithm but I prefer thinking of it as pulling valid values from to the lower mips up to the level 0...
//
[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS_Pull( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint2	targetPixelIndex = _dispatchThreadID.xy;
	if ( any( targetPixelIndex > _targetSize ) )
		return;

	// Read currently existing value (possibly already valid)
	float4	oldV = _tex_sourceRadianceCurrentMip[targetPixelIndex];
	float	oldW = Depth2Weight( oldV.w );
/*	if ( oldW > 0.999 )
		return;	// This pixel is already filled up with a valid value...

	// Read the lower mip's 4 values surrounding our invalid value
	// Now we KNOW that ALL of these values are guaranteed to be valid since we're reconstructing
	//	entire mip levels one by one and no pixel can be left empty anymore so any mip below our
	//	current level is completely filled up...
	uint2	sourcePixelIndex = targetPixelIndex >> 1;
	float4	V00 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.x++;
	float4	V10 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.y++;
	float4	V11 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.x--;
	float4	V01 = _tex_sourceRadiance[sourcePixelIndex];	sourcePixelIndex.y--;

	// Compute depth weight
	// Will return 0 for invalid pixels and low values for "non important depths"
	float	w00 = Depth2Weight( V00.w );
	float	w10 = Depth2Weight( V10.w );
	float	w11 = Depth2Weight( V11.w );
	float	w01 = Depth2Weight( V01.w );
	float	sumWeights = w00 + w10 + w01 + w11;

	// Perform a simple bilinear interpolation...
	_tex_reprojectedRadiance[targetPixelIndex] = _tex_sourceRadiance.SampleLevel( LinearClamp, float2( targetPixelIndex + 0.5 ) / _targetSize, 0.0 );
*/
	if ( oldW > 0.999 )
		_tex_reprojectedRadiance[targetPixelIndex] = oldV;
	else
		_tex_reprojectedRadiance[targetPixelIndex] = _tex_sourceRadiance.SampleLevel( LinearClamp, float2( targetPixelIndex + 0.5 ) / _targetSize, 0.0 );
}
