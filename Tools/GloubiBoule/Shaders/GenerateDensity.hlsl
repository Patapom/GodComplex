#include "Includes/global.hlsl"
#include "Includes/Photons.hlsl"
#include "Includes/Room.hlsl"
#include "Includes/Noise.hlsl"
#include "Includes/HeightMap.hlsl"

#define	NUM_THREADSX	8
#define	NUM_THREADSY	8
#define	NUM_THREADSZ	8

cbuffer CB_GenerateDensity : register(b2) {
	float3	_wsOffset;
};

RWTexture3D< float2 >	_Tex_VolumeDensity : register(u0);


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Very simple shader that computes a heavy fbm noise density and stores it as a [0,1] density into a 3D R8 texture
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#if 0

static const float3x3	s_rotationMatrices[8] = {
	float3x3( 0.8874482, 0.4474611, -0.1105177,-0.4120835, 0.8776987, 0.2446061,0.206453, -0.1715327, 0.9633035 ),
	float3x3( 0.7185354, 0.3319585, -0.6111549,0.08623777, 0.8294328, 0.5519095,0.690123, -0.4492712, 0.5673496 ),
	float3x3( 0.5836591, 0.05930796, -0.80983,0.1845327, 0.9615456, 0.2034149,0.7907526, -0.268165, 0.5502706 ),
	float3x3( 0.9690007, 0.2453932, -0.0286329,-0.1799182, 0.7803379, 0.5989175,0.1693137, -0.5752, 0.8002986 ),
	float3x3( 0.4260639, 0.3460401, -0.8358982,-0.3942408, 0.9026309, 0.1727181,0.8142749, 0.2559562, 0.5210016 ),
	float3x3( 0.7841536, 0.5543236, -0.2789776,-0.3190698, 0.7457208, 0.5848888,0.532257, -0.3696293, 0.7616276 ),
	float3x3( 0.4443594, 0.6210936, -0.6455908,-0.3765859, 0.7833869, 0.4944573,0.8128516, 0.02340363, 0.5820004 ),
	float3x3( 0.9159451, 0.3679352, -0.1602137,-0.3572623, 0.9294573, 0.09204819,0.1827796, -0.02707275, 0.9827811 ),
};

float	ComputeNoiseDensity( float3 _wsPosition ) {
/*
	// Sample an ultra-ultra-low-frequency (slowly-varying) float4 
	// noise value we can use to vary high-level terrain features 
	// over space.
//	float4 uulf_rand  = saturate( NMQu( _wsPosition*0.000718, NoiseTexture0 ) * 2.0 - 0.5 );
	float4 uulf_rand2 =           NMQu( _wsPosition*0.000632, NoiseTexture1 );
	float4 uulf_rand3 =           NMQu( _wsPosition*0.000695, NoiseTexture2 );


	//-----------------------------------------------
	// PRE-WARP the world-space coordinate.
	const float prewarp_str = 25;   // recommended range: 5..25

	float3	ulf_rand;

	ulf_rand.x = NHQs( _wsPosition*0.0041*0.971, float3(0,0,1,0) ) * 0.64
			   + NHQs( _wsPosition*0.0041*0.461, float3(0,0,0,1) ) * 0.32;
	ulf_rand.y = NHQs( _wsPosition*0.0041*0.997, float3(0,1,0,0) ) * 0.64
			   + NHQs( _wsPosition*0.0041*0.453, float3(1,0,0,0) ) * 0.32;
	ulf_rand.z = NHQs( _wsPosition*0.0041*1.032, float3(0,0,0,1) ) * 0.64
			   + NHQs( _wsPosition*0.0041*0.511, float3(0,0,1,0) ) * 0.32;

	_wsPosition += ulf_rand.xyz * prewarp_str * saturate( uulf_rand3.x*1.4 - 0.3 );
*/

	//-----------------------------------------------
	// compute 8 randomly-rotated versions of '_wsPosition'.  
	// we probably won't use them all, but they're here for experimentation.
	// (and if they're not used, the shader compiler will optimize them out.)
	float3	c0 = mul( _wsPosition, s_rotationMatrices[0] );
	float3	c1 = mul( _wsPosition, s_rotationMatrices[1] );
	float3	c2 = mul( _wsPosition, s_rotationMatrices[2] );
	float3	c3 = mul( _wsPosition, s_rotationMatrices[3] );
	float3	c4 = mul( _wsPosition, s_rotationMatrices[4] );
	float3	c5 = mul( _wsPosition, s_rotationMatrices[5] );
	float3	c6 = mul( _wsPosition, s_rotationMatrices[6] );
	float3	c7 = mul( _wsPosition, s_rotationMatrices[7] );

	// sample 9 octaves of noise, w/rotated ws coord for the last few.
	// NOTE: sometimes you'll want to use NHQs (high-quality noise) instead of NMQs for the lowest 3 frequencies or so; otherwise they can introduce UNWANTED high-frequency noise (jitter).
	//   BE SURE TO PASS IN 'PackedNoiseVolX' instead of 'NoiseVolX' WHEN USING NHQs()!!!
	// NOTE: if you want to randomly rotate various octaves, feed c0..c7 (instead of ws) into the noise functions.
	//   This is especially good to do with the lowest frequency, so that it doesn't repeat (across the ground plane) as often... and so that you can actually randomize the terrain!
	//   Note that the shader compiler will skip generating any rotated coords (c0..c7) that are never used.
	//
	const float	frequencies[] = {	0.3200*0.934,
									0.1600*1.021,
									0.0800*0.985,
									0.0400*1.051,
									0.0200*1.020,
									0.0100*0.968,
									0.0050*0.994,
									0.0025*1.045,
									0.0012*0.972,
								};
	const float	weights[] =		{	0.16*1.20,
									0.32*1.16,
									0.64*1.12,
									1.28*1.08,
									2.56*1.04,
									5.0,
									10*1.0,
									20*0.9,
									40*0.8,
								};
	const float	sumWeights = 70.3248;

	float	density = weights[0] * NLQ( frequencies[0] * _wsPosition, float4(0,0,0,1) ) 
					+ weights[1] * NLQ( frequencies[1] * _wsPosition, float4(0,1,0,0) )
 					+ weights[2] * NLQ( frequencies[2] * _wsPosition, float4(0,0,1,0) )
 					+ weights[3] * NLQ( frequencies[3] * _wsPosition, float4(1,0,0,0) )
 					+ weights[4] * NLQ( frequencies[4] * _wsPosition, float4(0,1,0,0) )
 					+ weights[5] * NLQ( frequencies[5] * _wsPosition, float4(0,0,0,1) ) 
 					+ weights[6] * NMQ( frequencies[6] * _wsPosition, float4(1,0,0,0) )	// MQ
 					+ weights[7] * NMQ( frequencies[7] * c6, float4(0,0,1,0) )				// MQ
 					+ weights[8] * NHQ( frequencies[8] * c7, float4(0,0,0,1) );			// HQ and *rotated*!

	return density / sumWeights;

}
#else
static const uint	OCTAVES_COUNT = 12;

float	ComputeNoiseDensity( float3 _wsPosition ) {

	float3	noisePosition = _wsPosition;

	float	sum = 0.0, sumWeights = 0.0;
	float	A = 1.0, F = 1.0;
	for ( uint octaveIndex=0; octaveIndex < OCTAVES_COUNT; octaveIndex++ ) {
		sum += A * _TexNoise.SampleLevel( LinearWrap, F * noisePosition, 0.0 );
		sumWeights += A;

		A *= 0.5;
		F *= 2.0;
	}

	return sum / sumWeights;
}
#endif

[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	float3	wsPosition = RoomCellIndex2World( _DispatchThreadID );
			wsPosition += _wsOffset;	// Animation comes from outside the shader
	_Tex_VolumeDensity[_DispatchThreadID] = ComputeNoiseDensity( wsPosition );
}
