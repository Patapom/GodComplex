/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Structures, Variables & Buffers
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef AUTOEXPOSURE_COMMON_INCLUDED
#define AUTOEXPOSURE_COMMON_INCLUDED

// Auto-exposure structure
struct autoExposure_t {
	float	EngineLuminanceFactor;		// The actual factor to apply to values stored to the HDR render target (it's simply LuminanceFactor * WORLD_TO_BISOU_LUMINANCE so it's a division by about 100)

	float	TargetLuminance;			// The factor to apply to the HDR luminance to bring it to the LDR luminance (warning: still in world units, you must multiply by WORLD_TO_BISOU_LUMINANCE for a valid engine factor)
	float	MinLuminanceLDR;			// Minimum luminance (cd/m²) the screen will display as the value sRGB 1
	float	MaxLuminanceLDR;			// Maximum luminance (cd/m²) the screen will display as the value sRGB 255
	float	MiddleGreyLuminanceLDR;		// "Reference EV" luminance (cd/m²) the screen will display as the value sRGB 128 (55 linear)
	float	EV;							// Absolute Exposure Value of middle grey (sRGB 128) from a reference luminance of 0.15 cd/m² (see above for an explanation on that magic value)
	float	Fstop;						// The estimate F-stop number (overridden with env/autoexp/fstop_bias)

	uint	PeakHistogramValue;			// The maximum value found in the browsed histogram (values at start and end of histogram are not accounted for based on start & end bucket indices
};

StructuredBuffer<autoExposure_t>	_bufferAutoExposure : register(t0);	// The auto-exposure values for current frame
Texture2D<uint>						_texHistogram : register(t1);		// The image histogram (for debug purpose)


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// General Helpers & Constants
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

// The "arbitrary factor" that converts our lighting units to candelas so we can compute the histogram
// Actually, it's not _that_ arbitrary. It's computed assuming a 100W lightbulb emitting 1750 lumen (typical value)
//	will light a wall at 1 meter distance with an illuminance of 1750/(4PI) = 139.26 lm/m² (cd)
//
// For this we use the point light formula:
//	E = Phi / A = I . Omega / A = I . (A/r²) / A = I / r²
//
//	• Phi is the luminous flux in lumens (lm)
//	• I is the luminous intensity in lumens per steradian (lm/sr)
//	• A is the lit area in square meters (m²)
//	• Omega is the solid angle in steradians (sr) (which is also equal to the perceived area divided by the squared distance, hence the simplification using A/r²)
//
// Applying to a luminous intensity of 1750/(4PI) lm/sr and a distance of r=1 we get our magic number...
//
static const float	BISOU_TO_WORLD_LUMINANCE = 139.26;
static const float	WORLD_TO_BISOU_LUMINANCE = 1.0 / BISOU_TO_WORLD_LUMINANCE;

static const float	MIN_ADAPTABLE_SCENE_LUMINANCE = 1e-2;										// 0.01 cd/m² for star light but we limit to 0.01 cd/m² because we don't want to adapt that low!
static const float	MAX_ADAPTABLE_SCENE_LUMINANCE = 1e4;										// 100,000 cd/m² for the Sun
static const float	SCENE_LUMINANCE_RANGE_DB = 120.0;											// Scene dynamic range in decibels = 20.log10( MAX / MIN )
static const float	MIN_ADAPTABLE_SCENE_LUMINANCE_DB = -40;										// Minimum range in decibels
static const float	MAX_ADAPTABLE_SCENE_LUMINANCE_DB = 80;										// Maximum range in decibels

static const uint	TARGET_MONITOR_BITS_PRECISION = 8;											// Target monitors usually have 8 bits precision.
static const float	TARGET_MONITOR_LUMINANCE_RANGE = (1 << TARGET_MONITOR_BITS_PRECISION) - 1;	// So it has a range of 255 (the brightest pixel is 255 times brighter than the lowest)
static const float	TARGET_MONITOR_LUMINANCE_RANGE_DB = 48.164799306236991234198223155919;		// Target monitor's dynamic range in decibels = 20.log10( 1 << BITS )

static const uint	HISTOGRAM_BUCKETS_COUNT = 128;														// We choose to have 128 buckets in our histogram
static const float	HISTOGRAM_BUCKET_RANGE_DB = SCENE_LUMINANCE_RANGE_DB / HISTOGRAM_BUCKETS_COUNT;		// Range of a single histogram bucket (in dB)
//static const float	TARGET_MONITOR_BUCKETS_COUNT = TARGET_MONITOR_LUMINANCE_RANGE_DB / HISTOGRAM_BUCKET_RANGE_DB;	// Amount of buckets covered by the target monitor range


// After measuring a luminance of 240cd/m² on a typical display device (DELL U2412M) using the i1 Display Pro probe and taking pictures
//	of a white screen using my Canon EOS 300D, the EOS told me that what it considered to be "EV 0" was met for F=4 and t=1/100s.
//
// According to wikipedia (http://en.wikipedia.org/wiki/Exposure_value):
//	EV = log2( F² / t )
//
//		F is the F-stop number
//		t is the exposure time
// 
// This gives us 0 EV at 240 cd/m² = log2( 4*4 / (1/100) ) = log2( 1600 )
//
// This tells us that the absolute reference luminance for the EOS should be ~ 240 / 1600 = 0.15 cd/m²
// This reference luminance will map to sRGB = 0.5 (i.e. 128) and will be considered to be the "EV absolute 0"
//
static const float	ABSOLUTE_EV_REFERENCE_LUMINANCE = 0.15;

// Converts a luminance to a decibel value
//	dB = 20 * log10( Luminance )
float	Luminance2dB( float _Luminance ) {
	return 8.6858896380650365530225783783321 * log( _Luminance );
}

// Converts a luminance to a decibel value
//	Luminance = 10^(dB / 20.0)
float	dB2Luminance( float _dB ) {
	return pow( 10.0, 0.05 * _dB );
}

// Converts a luminance to an histogram bucket index
float	Luminance2HistogramBucketIndex( float _Luminance ) {
	float	dB = Luminance2dB( _Luminance );
	return (dB - MIN_ADAPTABLE_SCENE_LUMINANCE_DB) * (1.0 / HISTOGRAM_BUCKET_RANGE_DB);
}

// Converts an histogram bucket index to a luminance
float	HistogramBucketIndex2Luminance( float _BucketIndex ) {
	float	dB = MIN_ADAPTABLE_SCENE_LUMINANCE_DB + _BucketIndex * HISTOGRAM_BUCKET_RANGE_DB;
	return dB2Luminance( dB );
}

// Computes the exposure factor based on the user's provided EV
// float	ComputeExposureFactor() {
// 	return exp2( -($(PostFX/Luminance/customEVBias).x + $(env/autoexp/EV)) );
// }

// Reads back the current luminance exposure data for the current frame
autoExposure_t	ReadAutoExposureParameters() {
	return _bufferAutoExposure[0];
}

#endif // #ifndef AUTOEXPOSURE_COMMON_INCLUDED
