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

static const uint	OCTAVES_COUNT = 12;


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Very simple shader that computes a heavy fbm noise density and stores it as a [0,1] density into a 3D R8 texture
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
float	fbm( float3 _noisePosition ) {
	float	V0 = 
	float	V1 = _TexNoise.SampleLevel( LinearWrap, 0.10 * _noisePosition, 0.0 );
	float	V2 = _TexNoise.SampleLevel( LinearWrap, 0.20 * _noisePosition, 0.0 );
	return (V2 + 2.0 * V1 + 4.0 * V0) / 7.0;
}

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


[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	float3	wsPosition = RoomCellIndex2World( _DispatchThreadID );
			wsPosition += _wsOffset;	// Animation comes from outside the shader
	_Tex_VolumeDensity[_DispatchThreadID] = ComputeNoiseDensity( wsPosition );
}
