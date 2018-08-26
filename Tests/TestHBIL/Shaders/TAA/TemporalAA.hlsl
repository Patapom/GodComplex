////////////////////////////////////////////////////////////////////////////////
// Performs temporal anti-aliasing
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define THREADS_X	16
#define THREADS_Y	16

Texture2D< float3 >		_tex_sourceHDR : register(t0);
Texture2D< float2 >		_tex_motionVectors : register(t1);
Texture2D< float3 >		_tex_historyIn : register(t2);
RWTexture2D< float3 >	_tex_TAAResult : register(u0);
RWTexture2D< float3 >	_tex_historyOut : register(u1);

cbuffer CB_TAA : register(b3) {
	float2	_TAADither;
	float	_TAAAmount;					// Default = 0.1
};

#define GROUP_X						16
#define GROUP_Y						16
#define BUFFER_X					(GROUP_X + 2)
#define BUFFER_Y					(GROUP_Y + 2)
#define RENAMED_GROUP_Y				((GROUP_X * GROUP_Y) / BUFFER_X)

#define TAA_DITHER					0.3	// 0-1
#define TAA_CLAMPING_FACTOR			1.0	// larger values produces less clamping, but introduces ghosting
#define TAA_MIN_LUMA				0.1
		
//#if defined( DURANGO )
//	#define __XBOX_CONTROL_NONIEEE 0
//	#define __XBOX_PRESERVE_MAD_LEGACY  1
//	#define __XBOX_DISABLE_NONIEEE_OPTIMIZATION_MASK0 7
//#endif

groupshared float4	gs_colorsAndLengths[BUFFER_Y][BUFFER_X];
groupshared float2	gs_motionVectors[BUFFER_Y][BUFFER_X];

float3 ApproximativeToneMap( float3 x, float _exposure ) {
	x *= _exposure;
	return ToneMap( x );
}

float3 ApproximativeInverseToneMap( float3 x, float _exposure ) {
	x = InverseToneMap( x );
	x /= _exposure;
	return x;
}

void Bicubic2DCatmullRom( in float2 _UV, in float2 _invSize, out float2 _pos[3], out float2 _weight[3] ) {
	float2 tc = floor( _UV - 0.5 ) + 0.5;
	float2 f = _UV - tc;
	float2 f2 = f * f;
	float2 f3 = f2 * f;

	float2 w0 = f2 - 0.5 * (f3 + f);
	float2 w1 = 1.5 * f3 - 2.5 * f2 + 1;
	float2 w3 = 0.5 * (f3 - f2);
	float2 w2 = 1 - w0 - w1 - w3;

	_weight[0] = w0;
	_weight[1] = w1 + w2;
	_weight[2] = w3;

	_pos[0] = tc - 1;
	_pos[1] = tc + w2 / _weight[1];
	_pos[2] = tc + 2;

	_pos[0] *= _invSize;
	_pos[1] *= _invSize;
	_pos[2] *= _invSize;
}

//#if !$taaR11G11B10
//	// IMPORTANT: YCoCg very rarely produces strange gray outlines for some luma-chroma combinations...
//	// it doesn't look incorrect, but you can feel that something goes wrong...
//	#define COLOR_TO(c)						RGB2YCoCg(c)
//	#define COLOR_FROM(c)					YCoCg2RGB(c)
//	#define COLOR_LUMA(c)					c.x
//	#define COLOR_SCALE_LUMA(c, scale)		c.x *= 1.0 + scale * (1.0 - c.x)
//#else
	#define COLOR_TO(c)						c
	#define COLOR_FROM(c)					c
	#define COLOR_LUMA(c)					dot( c, LUMINANCE )
	#define COLOR_SCALE_LUMA(c, scale)		c *= 1.0 + scale * (1.0 - COLOR_LUMA(c))
//#endif

void	Preload( int2 _sharedID, int2 _globalID, float _exposure ) {
	float3	colorHDR = _tex_sourceHDR[_globalID].xyz;
	float3	toneMappedColor = ApproximativeToneMap( colorHDR, _exposure );
			toneMappedColor = COLOR_TO( toneMappedColor );

	float2	motion = _tex_motionVectors[_globalID].xy;
			motion += _cameraSubPixelOffset.xy;
			motion *= 0.5;
			motion.y = -motion.y;

	float	motionLength = dot( motion, motion );

	gs_colorsAndLengths[_sharedID.y][_sharedID.x] = float4( toneMappedColor, motionLength );
	gs_motionVectors[_sharedID.y][_sharedID.x] = motion;
}

float rand24b( float2 _seed ) {
	uint n = asuint(_seed.y * 214013.153 + _seed.x * 2531011.731);
	n = n * (n * n * 15731u + 789221u);
	n = (n >> 9u) | 0x3F800000u;
	return 2.0 - asfloat(n);
}

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS( uint _groupIndex : SV_groupIndex, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	int2	localID = int2(_groupThreadID.xy);
	int2	globalID = int2(_dispatchThreadID.xy);
//	uint	threadID = _groupIndex;
		
	// Rename the 16x16 group into a 18x14 group + 4 idle threads in the end
	float	linearID = localID.y * GROUP_X + localID.x;
			linearID = (linearID + 0.5) / float(BUFFER_X);
		
	int2	newID;
			newID.y = int( floor(linearID) );
			newID.x = int( floor(frac(linearID) * BUFFER_X) );
		
	int2	groupBase = globalID - localID - 1;
		
	// Compute exposure from one thread and store in shared memory
	// NOTE: Actually hardcoded in this example
	float	exposure = _exposure;

	// Preload the colors and motion vectors into shared memory
	if ( newID.y < RENAMED_GROUP_Y )
		Preload( newID, groupBase + newID, exposure );

	newID.y += RENAMED_GROUP_Y;
	if ( newID.y < BUFFER_Y )
		Preload( newID, groupBase + newID, exposure );

	GroupMemoryBarrierWithGroupSync();

	// Calculate the color distribution and find the longest MV in the neighbourhood
	float3	colorMoment1 = 0;
	float3	colorMoment2 = 0;
	float	longestMVLength = -1;
	int2	longestMVPos = 0;
	float3	thisPixelColor;
	float2	pixelCoord_f = float2(globalID) + 0.5;

	// IMPORTANT: I like jitter! Why not to use it? It hides banding and scattering upsampling artifacts...
	float	jitter = rand24b( pixelCoord_f + _TAADither );
			jitter = lerp(-TAA_DITHER, TAA_DITHER, jitter );

	[unroll]
	for ( int dy = 0; dy <= 2; dy++ ) {
		[unroll]
		for ( int dx = 0; dx <= 2; dx++ ) {
			int2	pos = localID.xy + int2(dx, dy);

			float4	colorAndLength = gs_colorsAndLengths[pos.y][pos.x];
			float3	color = colorAndLength.rgb;
			float	motionLength = colorAndLength.a;

			[flatten]
			if ( dx == 1 && dy == 1 ) {
				thisPixelColor = color;
//				COLOR_SCALE_LUMA( thisPixelColor, jitter );	// Dither
			}

			colorMoment1 += color;
			colorMoment2 += color * color;

			[flatten]
			if ( motionLength > longestMVLength ) {
				longestMVPos = pos;
				longestMVLength = motionLength;
			}
		}
	}

	colorMoment1 *= 1.0 / 9.0;
	colorMoment2 *= 1.0 / 9.0;

	float3	colorVariance = colorMoment2 - colorMoment1 * colorMoment1;
	float3	colorSigma = sqrt( max(0, colorVariance) ) * TAA_CLAMPING_FACTOR;
	float3	colorMin = colorMoment1 - colorSigma;
			colorMin = min( thisPixelColor, colorMin );
	float3	colorMax = colorMoment1 + colorSigma;
			colorMax = max( thisPixelColor, colorMax );
		
	// Sample the previous frame using the longest MV
	float2	longestMV = gs_motionVectors[longestMVPos.y][longestMVPos.x];
	float2	sourcePos = pixelCoord_f - longestMV * _resolution;
		
	float2 sampleLoc[3], sampleWeight[3];
	Bicubic2DCatmullRom( sourcePos, 1.0 / _resolution, sampleLoc, sampleWeight );

	float3	history = 0;
	float	hcolorMoment1 = 0;
	float	hcolorMin = 1e32;
	float	hcolorMax = -1e32;
	float4	screenLimits = float4( 0.5 / _resolution, 1.0 - 0.5 / _resolution );
		
	[unroll]
	for ( int i = 0; i <= 2; i++ ) {
		[unroll]
		for( int j = 0; j <= 2; j++ ) {
			float2	UV = clamp( float2( sampleLoc[j].x, sampleLoc[i].y ), screenLimits.xy, screenLimits.zw );
			float3	temp = _tex_historyIn.SampleLevel( LinearClamp, UV, 0 ).rgb;

			history += temp * (sampleWeight[j].x * sampleWeight[i].y);

			float luma = COLOR_LUMA(temp);
				
			hcolorMoment1 += luma;

			hcolorMin = min( hcolorMin, luma );
			hcolorMax = max( hcolorMax, luma );
		}
	}

	hcolorMoment1 *= 1.0 / 9.0;
	history = min( colorMax, max( colorMin, history ) );
		
	// Anti-flicker
	float	a = max( COLOR_LUMA(colorMax) - COLOR_LUMA(colorMin), TAA_MIN_LUMA ) * max( hcolorMoment1, TAA_MIN_LUMA );
	float	b = max( hcolorMax - hcolorMin, TAA_MIN_LUMA ) * max( COLOR_LUMA(colorMoment1), TAA_MIN_LUMA );
	float	c = smoothstep( 1.0, 2.0, a / b );
	float	frameWeight = _TAAAmount * (1.0 - c) + 0.01;

	// Blend the old color with the new color and store output
	float3	result = lerp( history, thisPixelColor, frameWeight );

	// NOTE: store history (LDR)
	_tex_historyOut[globalID] = result;

	result = COLOR_FROM( result );
//	result = saturate( result );
	result = ApproximativeInverseToneMap( result, exposure );

	// NOTE: store antialiased output (HDR)
	_tex_TAAResult[globalID] = result;
}
