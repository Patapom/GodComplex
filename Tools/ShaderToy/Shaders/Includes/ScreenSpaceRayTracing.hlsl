/////////////////////////////////////////////////////////////////////////////////////////////////////
// This routine performs screen-space ray-tracing into a downsampled depth buffer
// It's adaptative as it will change the mip into which it's fetching depth interval as long as no potential intersection is found...
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
static const uint	SSR_MIP_MAX = 4U;	// Max mip level is 1/16th resolution


// bmayaux (2016-05-07) Thickness is now a [1,2] factor varying with scene depth
//	_ZThicknessFactors, contains 2 float2 couples with each couple indicating (Z, Thickness Factor) where Z is the Z at which the factor applies
//	_ZThicknessPow, the exponent for the interpolation of Z thickness factors
// Returns the Z max value corresponding to the provided Z min value
//
// The idea is that near objects have a very small thickness and far objects have a larger thickness
// The thickness factor is interpolated between the 2 provided [ZStart,ZEnd] Z values, the factors at interval ends being [Thickness Factor Min, Thickness Factor Max]
//
float ComputeSceneZThickness( float _sceneZMin, float4 _ZThicknessFactors, float _ZThicknessPow ) {
	float	t = saturate( (_sceneZMin - _ZThicknessFactors.y) / (_ZThicknessFactors.w - _ZThicknessFactors.y) );
			t = pow( t, _ZThicknessPow );
	return lerp( _ZThicknessFactors.x, _ZThicknessFactors.z, t ) * _sceneZMin;
}

// Samples the scene's depth buffer at appropriate mip level and returns the [ZMin,ZMax] interval
float2	SampleSceneZ( uint2 _pixelPosition, uint _mipLevel, Texture2D<float> _TexFullResDepth, Texture2D<float4> _TexDownsampledDepth, float4 _ZThicknessFactors, float _ZThicknessPow ) {
// 	float2	ZMinMax = _mipLevel == 0 ?	_TexFullResDepth[_pixelPosition] :							// Read full resolution Z
// 										_TexDownsampledDepth.mips[_mipLevel-1U][_pixelPosition].yz;	// Assuming we're receiving Y=Min Z, Z=Max Z

	float2	ZMinMax;
	if ( _mipLevel == 0 )
		ZMinMax = _TexFullResDepth[_pixelPosition];								// Read full resolution Z
	else
		ZMinMax = _TexDownsampledDepth.mips[_mipLevel-1U][_pixelPosition].yz;	// Assuming we're receiving Y=Min Z, Z=Max Z

	ZMinMax.y = ComputeSceneZThickness( ZMinMax.y, _ZThicknessFactors, _ZThicknessPow );	// Grow the interval with artificial scene thickness

	return ZMinMax;
}

// Performs screen-space ray-tracing using a downsampled depth buffer
//	_csPosition, the initial camera-space ray position
//	_csDirection, the camera-space ray direction (normalized)
//	_MaxStepsCount, the maximum allowed amount of steps
//	_TexFullResDepth, the full resolution depth buffer containing linear Z values
//	_TexDownsampledDepth, the downsampled depth buffer containing (X=Average Z, Y=Min Z, Z=Max Z, W=Unused)
//	_TexSize, the size (in pixels) of _TexFullResDepth
//	_ZThicknessFactors, contains 2 float2 couples with each couple indicating (Z, Thickness Factor) where Z is the Z at which the factor applies
//	_ZThicknessPow, the exponent for the interpolation of Z thickness factors
//	_VerticalFade, specifies the minimum Z component of the view direction from which we start fading out the reflection (i.e. fade out of vertical rays) (default = 0.05)
//	_Camera2Proj, the CAMERA=>PROJECTION transform
//	_HitZ, the camera space Z of the hit
//	_UV, the normalized UV coordinates where the hit occurred
// Returns a blend factor in [0,1] to blend the reflection with default sky reflection
//
float	ScreenSpaceRayTrace( float3 _csPosition, float3 _csDirection, uint _MaxStepsCount, Texture2D<float> _TexFullResDepth, Texture2D<float4> _TexDownsampledDepth, uint2 _TexSize, float4 _ZThicknessFactors, float _ZThicknessPow, float _VerticalFade, float4x4 _Camera2Proj, out float _HitZ, out float2 _UV, out float3 _DEBUG ) {

	_HitZ = 1e6;
	_UV = 0.0;
	_DEBUG = 0.0;

	// Fade based on view direction
//	const float	START_FADE_DIR_Z = 0.05;			// Start fading out when direction's Z component is less than this value (i.e. moving toward negative Z directions)
	const float	START_FADE_DIR_Z = _VerticalFade;	// Start fading out when direction's Z component is less than this value (i.e. moving toward negative Z directions)
	float	Fade = smoothstep( 0.0, START_FADE_DIR_Z, -_csDirection.z );
	if ( Fade == 0.0 )
		return 0.0;	// Don't trace rays that come our way

	float3	csEndPos = _csPosition + _csDirection;	// We don't really care about the end position here, we just want the slope

	float4	H0 = mul( float4( _csPosition, 1.0 ), _Camera2Proj );
	float4	H1 = mul( float4( csEndPos, 1.0 ), _Camera2Proj );

	float	k0 = 1.0 / H0.w;	// k0 = 1/Zstart
	float	k1 = 1.0 / H1.w;	// k1 = 1/Zend

	H0.xy *= k0;	// Project in [-1,+1]
	H1.xy *= k1;
	H0.y *= -1.0;	// Because +Y goes downward in screen space (with DirectX anyway)
	H1.y *= -1.0;
	H0.z = k0;		// We're linearly interpolating 1/Z values
	H1.z = k1;

	// Compute the slope bend factor for when the ray origin gets close to a screen border
//	float	RayBendFactor = saturate( 4.0 * (1.0 - abs(H0.x) ) );
	float	RayBendFactor = 1.0 - smoothstep( 0.5, 1.0, abs(H0.x) );	

	// Express screen position in full-resolution pixels
	H0.xy = _TexSize * 0.5 * (1.0 + H0.xy);
	H1.xy = _TexSize * 0.5 * (1.0 + H1.xy);

	// Compute slope
	float2	Delta = H1.xy - H0.xy;
	float3	Slope = (H1.xyz - H0.xyz) / max( abs( Delta.x ), abs( Delta.y ) );	// Slope, with at least one of the 2 XY components equal to +1 or -1 (so adding the slope makes us advance an entire pixel)

	// This ugly code is here to prevent invalid 0 slopes when camera is perfectly horizontal
	Slope.x = abs(Slope.x) > 1e-3 ? Slope.x : 1e-3;
	Slope.y = abs(Slope.y) > 1e-3 ? Slope.y : 1e-3;

	// Bend horizontal slope based on closeness to screen border
	Slope.x *= RayBendFactor;

	// Pre-Compute the never changing pixel border intersect (i.e. the rate at which we will intersect the next X or Y pixel border)
	float2	InvSlope = 1.0 / Slope.xy;

	// Pre-Compute the never changing pixel border offset to add to a floor( pixel position ) to obtain the next pixel border to cross
	const float	BORDER_EPSILON = 1e-3;														// Always add a little epsilon so we always fully cross the border
	float2	BorderOffset = float2(	Slope.x > 0.0 ? 1.0+BORDER_EPSILON : -BORDER_EPSILON,	// If we're tracing to the right, then the next pixel border is on the right, otherwise it's on the left
									Slope.y > 0.0 ? 1.0+BORDER_EPSILON : -BORDER_EPSILON );	// If we're tracing to the bottom, then the next pixel border is below, otherwise it's above

	// Initial scale pixel position and slope depending on mip
	uint	MipLevel = SSR_MIP_MAX;		// Start at coarsest mip
	float	PixelSize = 1U << MipLevel;	// That's the size of a pixel at this mip (in full-resolution pixels)

	H0.xy /= PixelSize;
	Slope.z *= PixelSize;

	float	MaxY = _TexSize.y >> MipLevel;

	// Main loop
	bool	InInterval = false;
	float	Z = H0.w;	// Initial Z
	uint	StepIndex = 0U;
	[loop]
	while ( StepIndex < _MaxStepsCount && H0.y >= 0.0 && H0.y < MaxY && H0.z > 0.01 ) {

		// Compute next position
		uint2	PixelPos = uint2( floor( H0.xy ) );
		float2	PixelBorder = PixelPos + BorderOffset;
		float2	Distance2Border = PixelBorder - H0.xy;
		float2	T = Distance2Border * InvSlope;		// "Time" to intersect, on both X and Y, to reach either next left/right pixel, or top/bottom pixel
		float	t = min( T.x, T.y );				// Choose the closest intersection (i.e. horizontal or vertical border)

		float3	NextH = H0.xyz + t * Slope;
		float	NextZ = 1.0 / max( 1e-6, NextH.z );	// Here we need to make sure we don't exceed the "ray horizon" and the interpolated Z is always positive
													// Indeed, for very grazing rays, we quickly reach very far every time we march a single pixel and we can 
													//	actually exceed the ray's "infinite distance" which occurs when the slope takes us into negative Z values
													//	that's also one of the loop's exit condition (H0.z > 0) to avoid tracing "further than infinity"...

		// Sample Zs at new mip and position
		float2	ZMinMax = SampleSceneZ( PixelPos, MipLevel, _TexFullResDepth, _TexDownsampledDepth, _ZThicknessFactors, _ZThicknessPow );

		StepIndex++;	// One more texture tap

//		const float	INTERVAL_EPSILON = 1e-3;		// Positive values grow interval, negative values shrink it
//		ZMinMax.x -= INTERVAL_EPSILON;
//		ZMinMax.y += INTERVAL_EPSILON;

		// Check if we're in the thickness interval
		// The condition might look a little odd but it's clearer with a drawing.
		// Let's assume we're tracing from left to right, going up.
		// We start at a given Z at the bottom of the current pixel.
		// Knowing the slope and size of the pixel, we can find NextZ at the top of the current pixel as well:
		//
		//	 --> +Z
		//	                        Next Z
		//	Top	-----------------------X-------
		//		                    +
		//		                 +
		//		              +
		//		           +
		//		        +
		//	Bottom ---X------------------------
		//	      Current Z
		//
		// We actually have 4 intersection cases to distinguish (C is Current Z, N is Next Z):
		//
		//	          Zmin    Zmax
		//	           |       |
		//	   1)   C--+-----N |
		//	           |       |
		//	   2)   C--+-------+--N
		//	           |       |
		//	   3)      | C---N |
		//	           |       |
		//	   4)      | C-----+--N
		//	           |       |
		//
		// 1) We either enter the Z interval			=> Z < ZMin && NextZ >= Zmax
		// 2) Or we entirely cross the Z interval		=> Z < ZMin && NextZ > Zmax
		// 3) Or we're entirely inside the Z interval	=> Z >= ZMin && NextZ <= Zmax
		// 4) Or we exit the Z interval					=> Z <= ZMax && NextZ > Zmax
		//
		// We can immediately notice that:
		//	• All C's are standing BEFORE Zmax
		//	• All N's are standing AFTER Zmin
		// 
		// QED?
		//
		InInterval = Z <= ZMinMax.y && NextZ >= ZMinMax.x;

		[branch]
		if ( InInterval ) {
			[branch]
			if ( MipLevel == 0U ) {
				// We have an intersection at finest mip!
				// Compute "exact" hit Z
				// We know that in the distance "t" we marched from Current Z to Next Z and crossed the ZMin boundary
				//	ZMin = Current Z + (Next Z - Current Z) * s / t
				// We then retrieve s = (ZMin - CurrentZ) * t / (Next Z - Current Z)
				//
				float	s = saturate( (ZMinMax.x - Z) * t / (NextZ - Z) );
				_HitZ = 1.0 / (H0.z + s * Slope.z);
//_HitZ = 100.0 * abs( Z - _HitZ );			// Show absolute error
//_HitZ = 100.0 * abs( (Z - _HitZ) / Z );	// Show relative error
				break;
			}

//*			// Refine mip to check if we're still intersecting at finer resolution (smaller pixels, slower steps)
			MipLevel--;
			H0.xy *= 2.0;		// Smaller pixels
			Slope.z *= 0.5;		// Smaller steps
			MaxY = _TexSize.y >> MipLevel;

			InInterval = false;	// Assume we're not in the interval anymore (in case we exit because of excess of steps, we don't want false hits)

			// We can't march yet as we don't know if we're intersecting at finer mip!
			continue;
//*/
		}

		// No intersection!
		// It means we can safely march forward a full pixel!
		H0.xyz = NextH;
		Z = NextZ;

//*		// Attempt to trace at coarser resolution (larger pixels, faster steps)
		[branch]
		if ( MipLevel < SSR_MIP_MAX ) {
			MipLevel++;
			H0.xy *= 0.5;		// Larger pixels
			Slope.z *= 2.0;		// Larger steps
			MaxY = _TexSize.y >> MipLevel;
		}
//*/
	}

	// Recompute and UVs
	_UV = H0.xy / _TexSize;

//_DEBUG = float(MipLevel) / SSR_MIP_MAX;
_DEBUG = float3( _UV, 0 );
_DEBUG = 0.1 * _HitZ;
_DEBUG = H0.z <= 0.0 ? float3( 1, 0, 0 ) : float3( 0, 1, 0 );
_DEBUG = float(StepIndex) / _MaxStepsCount;
//InInterval = 1.0;	// Force show debug value

	return InInterval ? Fade : 0.0;
}
