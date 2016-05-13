/////////////////////////////////////////////////////////////////////////////////////////////////////
// This routine performs a screen-space ray-tracing into a downsampled depth buffer
// It's adaptative as it will change the mip into which it's tracing the ray as long as no potential
//	intersection is found...
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
//static const uint	SSR_MIP_MIN = 1;	// Min mip level is half-resolution
static const uint	SSR_MIP_MAX = 3;	// Max mip level is 1/8th resolution


// bmayaux (2016-05-07) Thickness is now a [1,2] factor varying with scene depth
// The idea is that near objects have a very small thickness and far objects have a larger thickness
float ComputeSceneZThickness( float _sceneZMin, float4 _zThicknessFactors, float _zThicknessPow ) {
	float	t = saturate( (_sceneZMin - _zThicknessFactors.y) / (_zThicknessFactors.w - _zThicknessFactors.y) );
			t = pow( t, _zThicknessPow );
	return lerp( _zThicknessFactors.x, _zThicknessFactors.z, t ) * _sceneZMin;
}

// Performs screen-space ray-tracing using a downsampled depth buffer
//	_csPosition, the initial camera-space ray position
//	_csDirection, the camera-space ray direction
//	_RayLength, the maximum ray length to trace
//	_TexDownsampledDepth, the downsampled depth buffer
//	_TexSize, the size of lowest mip of the _TexDownsampledDepth
//	_Camera2Proj, the CAMERA=>PROJECTION transform
//	_MaxStepsCount, the maximum allowed amount of steps
//	_HitDistance, the distance of the hit, in camera space units
//	_UV, the normalized UV coordinates where the hit occurred
// Returns a blend factor in [0,1] to blend the reflection with default sky reflection
//
float	ScreenSpaceRayTrace( float3 _csPosition, float3 _csDirection, float _RayLength, Texture2D<float4> _TexDownsampledDepth, uint2 _TexSize, float4x4 _Camera2Proj, uint _MaxStepsCount, out float _HitDistance, out float2 _UV ) {

	_HitDistance = 0.0;
	_UV = 0.0;


const float4	ZThicknessFactors = float4( 1.02, 10.0, 1.3, 100.0 );	// Apply factor 1.02 at 10 meters, and 1.3 at 100 meters
const float		ZThicknessPow = 1.0;


	float3	csEndPos = _csPosition + _RayLength * _csDirection;

	float4	H0 = mul( float4( _csPosition, 1.0 ), _Camera2Proj );
	float4	H1 = mul( float4( csEndPos, 1.0 ), _Camera2Proj );

	float	k0 = 1.0 / H0.w;
	float	k1 = 1.0 / H1.w;

	H0.xy *= k0;
	H1.xy *= k1;

	H0.xy = float2( 0.5 * (1.0 + H0.x), 0.5 * (1.0 - H0.y) );
	H1.xy = float2( 0.5 * (1.0 + H1.x), 0.5 * (1.0 - H1.y) );

	H0.xy *= _TexSize;	// In pixels
	H1.xy *= _TexSize;

	float2	Delta = H1.xy - H0.xy;
	float	PixelsCount = max( abs( Delta.x ), abs( Delta.y ) );

	float4	P0 = float4( H0, _csPosition.z * k0, k0 );
	float4	P1 = float4( H1, csEndPos.z * k1, k1 );

	float4	Slope = (P1 - P0) / PixelsCount;						// Slope, with at least one of the 2 components equal to +1 or -1
	float2	BorderOffset = float2(	Slope.x > 0.0 ? 1.0 : 0.0,		// If we're tracing to the right, then the next pixel border is +1, otherwise it's 0
									Slope.y > 0.0 ? 1.0 : 0.0 );	// If we're tracing to the bottom, then the next pixel border is +1, otherwise it's 0

	float2	InvSlope = abs( Slope );
			InvSlope = sign( Slope ) / max( 1e-6, InvSlope );

	uint	MipLevel = SSR_MIP_MAX;			// Start at coaresest mip
	float	PixelSize = 1U << MipLevel;		// That's the size of a pixel at this mip
	float	ScaleFactor = 1.0 / PixelSize;	// That's the scale factor to apply to a mip 0 pixel to obtain its position in the current mip's pixel

	float2	PixelPos = ScaleFactor * P0;
	float2	PixelBorder = floor( PixelPos ) + BorderOffset;	// This is our target boundary
	float2	Distance2Border = PixelBorder - PixelPos;

	uint	StepsCount = min( PixelsCount, _MaxStepsCount );
	uint	StepIndex = 0;
	bool	InInterval = false;

	while ( stepIndex < StepsCount ) {

		// Sample at current position & current mip
		float3	ZAvgMinMax = _TexDownsampledDepth.mips[MipLevel][uint2(PixelPos)].xyz;
				ZAvgMinMax.z = ComputeSceneZThickness( ZAvgMinMax.z, ZThicknessFactors, ZThicknessPow );	// Grow the interval with artificial scene thickness

		// Check if we're in the thickness interval
		float	Z = P0.z / P0.w;
		bool	InInterval = Z >= ZAvgMinMax.y && Z <= ZAvgMinMax.z;
		if ( InInterval ) {
			if ( MipLevel == 0U ) {
				// We have an intersection at finest mip!
				// @TODO: Compute better intersection
				break;
			}

			// Refine mip to check if we're still intersecting
			MipLevel--;
			PixelSize *= 0.5;	// Smaller pixels
			ScaleFactor *= 2.0;	// Larger scale

			// Recompute pixel position and next intersection border
			PixelPos = ScaleFactor * P0.xy;
			PixelBorder = floor( PixelPos ) + BorderOffset;
			Distance2Border = PixelBorder - PixelPos;

			// We can't march yet as we don't know if we intersect something in finer mip!
			continue;
		}

		// No intersection!
		// It means we can safely march forward a full pixel!
		float2	T = Distance2Border * InvSlope;	// "Time" to intersect, on both X and Y, to reach either next left/right pixel, or top/bottom pixel
		float	t = min( T.x, T.y );			// Choose the closest intersection

		P0 += t * PixelSize * Slope;
		PixelPos += t * Slope.xy;
		PixelBorder = floor( PixelPos ) + BorderOffset;
		Distance2Border = PixelBorder - PixelPos;
//		StepIndex++;			// Count actual steps
		StepIndex += PixelSize;	// Or count actual pixels on screen


		// Attempt to trace at coarser resolution (faster steps)
		if ( MipLevel < SSR_MIP_MAX ) {
			MipLevel++;
			PixelSize *= 2.0;	// Larger pixels
			ScaleFactor *= 0.5;	// Smaller scale

			// Recompute pixel position and next intersection border
			PixelPos = ScaleFactor * P0.xy;
			PixelBorder = floor( PixelPos ) + BorderOffset;
			Distance2Border = PixelBorder - PixelPos;
		}
	}

	return InInterval ? 1.0 : 0.0;
}
