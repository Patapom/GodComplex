/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This shader computes the image's histogram
//	• Each thread reads multiple pixels along its scanline (default: 60 pixels for a 32*60=1920 image width), computes the log10 of the luminance
//		and accumulates into the appropriate histogram bucket.
//	• Several scanlines are processed at once (default: 4 scanlines) so a default total of 32*4=128 threads are used.
//	• The shader thus needs to be called with a dispatch of ( 1, ceil( ImageHeight / 4 ), 1 )
//	• The result will be stored into a render target of size 128 * ceil( ImageHeight / 4 )
//
#include "../Global.hlsl"
#include "Common.hlsl"

#define NUMTHREADX	32	// Tiles of 32x4 pixels
#define NUMTHREADY	4
#define NUMTHREADZ	1

static const uint	PASSES_COUNT = HISTOGRAM_SIZE / NUMTHREADX;	// To address the entire histogram, we need to make each thread in the group process several buckets
																// A typical histogram size of 128 and 32 threads per group will require 4 passes to address all 4*32=128 buckets...

Texture2D<float4>	_texSourceImageHDR : register(t0);			// Source image
RWTexture2D<uint>	_texTallHistogram : register(u0);			// The histogram used as a RW texture

groupshared uint	Histogram[NUMTHREADY][HISTOGRAM_SIZE];		// Here we have as many histograms as processed scanlines
																// The NUMTHREADY histograms are added together into a single one that we store at the end of the shader

[numthreads( NUMTHREADX, NUMTHREADY, NUMTHREADZ )]
void CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	Dimensions = uint2( ceil( 1.0 / _Resolution.xy ) );
	uint	Width = Dimensions.x;
	uint	Height = Dimensions.y;

	// Clear the histogram
	[unroll]
	for ( uint i=0; i < PASSES_COUNT; i++ ) {
		int	BucketIndex = NUMTHREADX * i + _GroupThreadID.x;
		Histogram[_GroupThreadID.y][BucketIndex] = 0;
	}
	GroupMemoryBarrierWithGroupSync();

	// Accumulate an entire scanline of the source image
	// NUMTHREADY scanlines of the source image are processed at the same time
	uint2	PixelIndex;
	PixelIndex.y = NUMTHREADY * _GroupID.y + _GroupThreadID.y;	// The source image's scanline we're reading from

	for ( uint X=0; X < Width; X+=NUMTHREADX ) {				// Jump in blocks of NUMTHREADX which is our granularity of horizontal blocks (so a 1920 image will require each thread to read 1920/32=60 pixels)
		PixelIndex.x = X + _GroupThreadID.x;
		if ( PixelIndex.x >= Width || PixelIndex.y >= Height )
			continue;	// Don't accumulate pixels outside the screen otherwise we have a bias toward blacks!

		float	Luminance = dot( LUMINANCE, _texSourceImageHDR[PixelIndex].xyz );
				Luminance *= BISOU_TO_WORLD_LUMINANCE;	// BISOU TO WORLD (?? units => cd/m²)

		// Reject black pixels that are clearly an error!
		if ( Luminance < MIN_ADAPTABLE_SCENE_LUMINANCE )
			continue;

		float	Luminance_dB = Luminance2dB( Luminance );	// 20 * log10( Luminance )

// Writing outside the target's range [0,127] crashes the driver!
//			uint	BucketIndex = uint( floor( HISTOGRAM_SIZE * saturate( (Luminance_dB - MIN_ADAPTABLE_SCENE_LUMINANCE_DB) / (MAX_ADAPTABLE_SCENE_LUMINANCE_DB - MIN_ADAPTABLE_SCENE_LUMINANCE_DB) ) ) );

		uint	BucketIndex = clamp( uint( floor( HISTOGRAM_SIZE * (Luminance_dB - MIN_ADAPTABLE_SCENE_LUMINANCE_DB) / (MAX_ADAPTABLE_SCENE_LUMINANCE_DB - MIN_ADAPTABLE_SCENE_LUMINANCE_DB) ) ), 0, HISTOGRAM_SIZE-1 );

		uint old;
		InterlockedAdd( Histogram[_GroupThreadID.y][BucketIndex], 1, old );
	}
	GroupMemoryBarrierWithGroupSync();

	#if NUMTHREADY > 8
		// Collapse second half of 8 lines into first half
		// (only working if threads count is at least 16)
		if ( _GroupThreadID.y < 8 ) {
			[unroll]
			for ( uint i=0; i < PASSES_COUNT; i++ ) {
				int	BucketIndex = NUMTHREADX * i + _GroupThreadID.x;
				Histogram[_GroupThreadID.y][BucketIndex] += Histogram[8+_GroupThreadID.y][BucketIndex];
			}
		}
		GroupMemoryBarrierWithGroupSync();
	#endif

	#if NUMTHREADY > 4
		// Collapse second half of 4 lines into first half
		// (only working if threads count is at least 8)
		if ( _GroupThreadID.y < 4 ) {
			[unroll]
			for ( uint i=0; i < PASSES_COUNT; i++ ) {
				int	BucketIndex = NUMTHREADX * i + _GroupThreadID.x;
				Histogram[_GroupThreadID.y][BucketIndex] += Histogram[4+_GroupThreadID.y][BucketIndex];
			}
		}
		GroupMemoryBarrierWithGroupSync();
	#endif

	#if NUMTHREADY > 2
		// Collapse second half of 2 lines into first half
		// (only working if threads count is at least 4)
		if ( _GroupThreadID.y < 2 ) {
			[unroll]
			for ( uint i=0; i < PASSES_COUNT; i++ ) {
				int	BucketIndex = NUMTHREADX * i + _GroupThreadID.x;
				Histogram[_GroupThreadID.y][BucketIndex] += Histogram[2+_GroupThreadID.y][BucketIndex];
			}
		}
		GroupMemoryBarrierWithGroupSync();
	#endif

	// From there, we only need a single line of threads to finalize the histogram
	if ( _GroupThreadID.y == 0 ) {
		[unroll]
		for ( uint i=0; i < PASSES_COUNT; i++ ) {
			int	BucketIndex = NUMTHREADX * i + _GroupThreadID.x;
			#if NUMTHREADY > 1
				_texTallHistogram[uint2( BucketIndex, _GroupID.y )] = Histogram[0][BucketIndex] + Histogram[1][BucketIndex];	// Add the two last lines together and store into destination
			#else // NUMTHREADY == 1
				_texTallHistogram[uint2( BucketIndex, _GroupID.y )] = Histogram[0][BucketIndex];								// Simply write result into destination
			#endif
		}
	}
}
