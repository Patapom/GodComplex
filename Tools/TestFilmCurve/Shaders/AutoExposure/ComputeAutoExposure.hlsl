/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This shader computes the dynamic range scaling factor by fitting the limited display device's dynamic range (usually 48dB)
//	to the most appropriate High Dynamic Range by maximizing the luminance integral from the previously computed histogram.
// This is an implementation of "2007 Schulz - Using Brightness Histogram to perform Optimum Auto Exposure" (http://www.ti1.tu-harburg.de/Mitarbeiter/ehemalige/simon/pub/ISPRA2007_schulz_OptimumAE.pdf)
//
// As a second step, it performs temporal adaptation to mimic the human eye response to changes in luminance.
//
// Finally, based on the currently adapted luminance range, a F-stop number is computed that can be used to determine
//	the aperture of the camera diaphragm so post-processes like bloom, glares and DOF are adequately portrayed
//
// NOTE: This reads from & renders to a 1x1 pixel and uses only a single thread
//
#include "../Global.hlsl"
#include "Common.hlsl"

#define NUMTHREADX	1
#define NUMTHREADY	1
#define NUMTHREADZ	1


cbuffer CB_AutoExposure : register( b10 ) {
	float	_delta_time;				// Clamped delta-time
	float	_white_level;				// (1.0) White level for tone mapping
	float	_clip_shadows;				// (0.0) Shadow cropping in histogram (first buckets will be ignored, leading to brighter image)
	float	_clip_highlights;			// (1.0) Highlights cropping in histogram (last buckets will be ignored, leading to darker image)
	float	_EV;						// (0.0) Your typical EV setting
	float	_fstop_bias;				// (0.0) F-stop number bias to override automatic computation (NOTE: This will NOT change exposure, only the F number)
	float	_reference_camera_fps;		// (30.0) Default camera at 30 FPS
	float	_adapt_min_luminance;		// (0.03) Prevents the auto-exposure to adapt to luminances lower than this
	float	_adapt_max_luminance;		// (2000.0) Prevents the auto-exposure to adapt to luminances higher than this
	float	_adapt_speed_up;			// (0.99) Adaptation speed from low to high luminances
	float	_adapt_speed_down;			// (0.99) Adaptation speed from high to low luminances
};

RWStructuredBuffer<autoExposure_t>	_targetBufferAutoExposure : register(u0);	// The auto-exposure values we'll need for current frame

[numthreads( NUMTHREADX, NUMTHREADY, NUMTHREADZ )]
void CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	autoExposure_t	Result;

	autoExposure_t	LastFrameResult = _bufferAutoExposure[0];

	// Compute luminance range in dB and the amount of buckets covered by the monitor
	float	MonitorLuminanceRange_dB = Luminance2dB( 255.0 * _white_level );
	float	MonitorBucketsCount = MonitorLuminanceRange_dB / HISTOGRAM_BUCKET_RANGE_DB;

	// Compute the start & end bucket indices based on shadows/highlights clipping boundaries (it's basically the same as a level in photoshop)
	// They're computed by specifying the bottom and top % of the 48dB histogram range we want to consider (shadow / highlights)
	// Once renormalized using MonitorBucketsCount, we get the min/max bucket index...
	//
	uint2	ClippedBucketIndex = uint2( _clip_shadows * MonitorBucketsCount, _clip_highlights * MonitorBucketsCount );

#if 0
	// =====================================================================================
	// 1] Compute the initial integral using an "optimized trapezoidal integration"
	// Typically, we decide that each bucket's width is 1 (dx=1) and we integrate by doing:
	//	I += 0.5 * (Bucket[0] + Bucket[1]) * dx
	//	I += 0.5 * (Bucket[1] + Bucket[2]) * dx
	//	(...)
	//	I += 0.5 * (Bucket[N-1] + Bucket[N]) * dx
	//
	// Which is finally equivalent to:
	//	I = 0.5 * Bucket[0]
	//	I += Bucket[1];
	//	I += Bucket[2];
	//	(...)
	//	I += Bucket[N-1]
	//	I += 0.5 * Bucket[N]
	//
	Result.PeakHistogramValue = 0;
	uint2	TailBucketPosition = uint2( ClippedBucketIndex.x, 0 );								// Start from the specified minimum bucket
	uint2	HeadBucketPosition = TailBucketPosition;											// We'll make head index grow
	uint	LastBucketIndex = ClippedBucketIndex.y - 1;											// Stop before the last bucket
	float	Integral = 0.5 * _texHistogram[HeadBucketPosition].x; HeadBucketPosition.x++;	// Add half of first bucket
	for ( ; HeadBucketPosition.x < LastBucketIndex; HeadBucketPosition.x++ ) {
		uint	HistoValue = _texHistogram[HeadBucketPosition].x;
		Integral += HistoValue;																	// Add each interior bucket once
		Result.PeakHistogramValue = max( Result.PeakHistogramValue, HistoValue );
	}
	Integral += 0.5 * _texHistogram[HeadBucketPosition].x;		 HeadBucketPosition.x++;	// Add half of last bucket


	// =====================================================================================
	// 2] Perform the scrolling integral on the histogram
	// The idea here is to make the integral "evolve" by subtracting the first bucket on the left
	//	and add the next bucket on the right. By iterating this way until the right end of the histogram
	//	we will account for all possible integrals and keep the one that gives the highest result.
	// This is where we'll place our low dynamic range, thus ensuring we're covering most of the significant pixels...
	//
	LastBucketIndex = HISTOGRAM_BUCKETS_COUNT - uint( MonitorBucketsCount - ClippedBucketIndex.y );	// If less than 100% of the monitor's range is accounted for (i.e. bright luminances cut)
																							//	then we don't need to integrate all the way to the end of the histogram...

	uint	MaxIntegralTailBucketPosition = TailBucketPosition.x;
	float	MaxIntegral = Integral;

	while ( HeadBucketPosition.x < LastBucketIndex ) {

		// Subtract tail bucket integral
		uint	HistoValueTail0 = _texHistogram[TailBucketPosition].x; TailBucketPosition.x++;
		uint	HistoValueTail1 = _texHistogram[TailBucketPosition].x;
		Integral -= 0.5 * (HistoValueTail0 + HistoValueTail1);

		// Add head bucket integral
		uint	HistoValueHead0 = _texHistogram[HeadBucketPosition].x; HeadBucketPosition.x++;
		uint	HistoValueHead1 = _texHistogram[HeadBucketPosition].x;
		Integral += 0.5 * (HistoValueHead0 + HistoValueHead1);

		// Update max integral & peak value
		if ( Integral > MaxIntegral ) {
			MaxIntegral = Integral;
			MaxIntegralTailBucketPosition = TailBucketPosition.x;
		}

		Result.PeakHistogramValue = max( Result.PeakHistogramValue, HistoValueHead1 );
	}
#else

	// =====================================================================================
	// 1] Compute the initial integral using a stupid sum
	// Use a "sticky integral" that attributes a lesser weight to samples that are far away from current exposure
	Result.PeakHistogramValue = 0;
	uint2	TailBucketPosition = uint2( ClippedBucketIndex.x, 0 );					// Start from the specified minimum bucket
	uint2	HeadBucketPosition = TailBucketPosition;								// We'll make head index grow
	uint	Integral = 0U;
	for ( ; HeadBucketPosition.x < ClippedBucketIndex.y; HeadBucketPosition.x++ ) {
		uint	HistoValue = _texHistogram[HeadBucketPosition].x;
		Integral += HistoValue;														// Add each interior bucket once
		Result.PeakHistogramValue = max( Result.PeakHistogramValue, HistoValue );
	}

	// =====================================================================================
	// 2] Perform the scrolling integral on the histogram
	// The idea here is to make the integral "evolve" by subtracting the first bucket to the left
	//	and add the next bucket to the right. By iterating this way until the right end of the histogram
	//	we will account for all possible integrals and keep the one that gives the highest result.
	// This is where we'll place our low dynamic range, thus ensuring we're covering most of the significant pixels...
	//
	uint	LastBucketIndex = HISTOGRAM_BUCKETS_COUNT - uint( MonitorBucketsCount - ClippedBucketIndex.y );	// If less than 100% of the monitor's range is accounted for (i.e. bright luminances cut)
																											//	then we don't need to integrate all the way to the end of the histogram...

	uint	MaxIntegralTailBucketPosition = TailBucketPosition.x;
	uint	MaxIntegral = Integral;

	while ( HeadBucketPosition.x < LastBucketIndex ) {

		// Subtract tail bucket integral
		uint	HistoValueTail = _texHistogram[TailBucketPosition].x; TailBucketPosition.x++;
		Integral -= HistoValueTail;

		// Add head bucket integral
		uint	HistoValueHead = _texHistogram[HeadBucketPosition].x; HeadBucketPosition.x++;
		Integral += HistoValueHead;

		// Update max integral & peak value
		if ( Integral >= MaxIntegral ) {	// Here, using > or >= makes a huge difference for histograms we can completely encompass in our LDR range!
			MaxIntegral = Integral;
			MaxIntegralTailBucketPosition = TailBucketPosition.x;
		}

		Result.PeakHistogramValue = max( Result.PeakHistogramValue, HistoValueHead );
	}
#endif

	// Make a completely black image have a centered window
	if ( MaxIntegral == 0U ) {
		MaxIntegralTailBucketPosition = (HISTOGRAM_BUCKETS_COUNT - MonitorBucketsCount) / 2;
	}

//### Use as debug value
//Result.PeakHistogramValue = MaxIntegralTailBucketPosition;
//Result.PeakHistogramValue = HeadBucketPosition.x - TailBucketPosition.x;
//Result.PeakHistogramValue = LastBucketIndex;


	// =====================================================================================
	// 3] Clamp target position to authorized range
	float	MinAdaptableLuminance_Bucket = Luminance2HistogramBucketIndex( _adapt_min_luminance );
	float	MaxAdaptableLuminance_Bucket = Luminance2HistogramBucketIndex( _adapt_max_luminance );

	float	TargetTailBucketPosition = clamp( MaxIntegralTailBucketPosition, MinAdaptableLuminance_Bucket, MaxAdaptableLuminance_Bucket - MonitorBucketsCount );


	// =====================================================================================
	// 4] Compute immediate target exposure value
	// We now know the index of the "tail bucket" (the left bucket of the LDR histogram) so we can easily retrieve
	//	the luminance factor as decibels and as a global factor afterward.
	//
	float	TargetLuminance_dB = MIN_ADAPTABLE_SCENE_LUMINANCE_DB + TargetTailBucketPosition * HISTOGRAM_BUCKET_RANGE_DB;	// = 20.log10( LuminanceFactor )
	float	TargetLuminance = dB2Luminance( TargetLuminance_dB );
			TargetLuminance *= exp2( -_EV );	// Override the default EV using our traditional camera EV bias


	// =====================================================================================
	// 5] Perform temporal adaptation based on last frame's parameters

	// The user's adapted luminance level is simulated by closing the gap between adapted luminance and current luminance by some % every frame
	// This is not an accurate model of human adaptation, which can sometimes take longer than half an hour.
	if ( LastFrameResult.TargetLuminance < TargetLuminance ) {
		Result.TargetLuminance = lerp( TargetLuminance, LastFrameResult.TargetLuminance, pow( abs(1.0 - _adapt_speed_up), _delta_time ) );
	} else {
		float	InvTargetLuminanceFactor = 1.0 / max( 1e-4, TargetLuminance );
		float	InvLastFrameLuminanceFactor = 1.0 / max( 1e-4, LastFrameResult.TargetLuminance );
		Result.TargetLuminance = 1.0 / lerp( InvTargetLuminanceFactor, InvLastFrameLuminanceFactor, pow( abs(1.0 - _adapt_speed_down), _delta_time ) );
	}


	// =====================================================================================
	// 6] Compute current frame's EV (Exposure Value) and F-stops number based on provided exposure time
	//
	// _ The luminance factor multiplied by the minimum adaptable luminance (default 0.01 cd/m²) will map to the luminance L0 of the least luminous pixel (i.e. pixel value = 1)
	// _ L0 * 255 will yield the maximum luminance displayed by the monitor (i.e. pixel value = 255)
	// _ The sRGB value used as "reference for the 0 EV" is sRGB 128, which is mapped to (128/256)^2.2 = 0.21 (i.e. pixel value = 55)
	//		so L_ref = L0 * 55 will be the luminance we'll use as a reference to compute the "absolute EV"
	// 
	Result.MinLuminanceLDR = Result.TargetLuminance;												// Minimum luminance the screen will display as the value 1
	Result.MaxLuminanceLDR = Result.MinLuminanceLDR * _white_level * TARGET_MONITOR_LUMINANCE_RANGE;// Maximum luminance the screen will display as the value 255
	Result.MiddleGreyLuminanceLDR = Result.MinLuminanceLDR * 55.497598410127913869937213614334;		// Reference EV luminance the screen will display as the value 255*(0.5^2.2) ~= 55

	float	PowEV = Result.MiddleGreyLuminanceLDR / ABSOLUTE_EV_REFERENCE_LUMINANCE;				// Absolute EV exponent based on our measured reference of 0.15 cd/m²
	Result.EV = log2( PowEV );																		// Absolute Exposure Value

	// Using EV = log2( F² / t ) and knowing both EV and t (given), we retrieve the F-stop number as
	//	F = sqrt( t * 2^EV )
	Result.Fstop = sqrt( PowEV / _reference_camera_fps );
	Result.Fstop += _fstop_bias;																	// Although it's not physically correct, nevertheless we let the user bias that value...

	// =====================================================================================
	// 7] Build the final engine factor
	// 
	float	WhiteLevelLuminance = Result.MinLuminanceLDR * 1.0 * TARGET_MONITOR_LUMINANCE_RANGE;	// Even though we can adapt to larger luminances thanks to tone mapping, keep a _UNIT_ white level so the user is not confused!
	float	WhiteLevelEngineLuminance = WhiteLevelLuminance * WORLD_TO_BISOU_LUMINANCE;				// This is the highest adaptable luminance, in bisou units
	Result.EngineLuminanceFactor = 1.0 / WhiteLevelEngineLuminance;									// So it must map to 1... Simple!

	// Write the result
	_targetBufferAutoExposure[0] = Result;
}
