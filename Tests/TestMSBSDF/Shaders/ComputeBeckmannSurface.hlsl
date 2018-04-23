//////////////////////////////////////////////////////////////////////////
// This shader implements the Beckmann surface generator from Heitz
// https://drive.google.com/file/d/0BzvWIdpUpRx_U1NOUjlINmljQzg/view
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Beckmann : register(b10) {
	float2	_positionMin;
	float2	_size;
	uint	_HeightFieldResolution;
	uint	_SamplesCount;
};

struct SB_Beckmann {
	float		m_phase;
	float2		m_frequency;
};
StructuredBuffer<SB_Beckmann>	_SB_Beckmann : register( t0 );
RWTexture2D< float >			_Tex_HeightField_Height : register( u0 );
RWTexture2D< float4 >			_Tex_HeightField_Normal : register( u1 );

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;
	float2	position = _positionMin + _size * pixelPosition / _HeightFieldResolution;

	// Accumulate slope and height
	float2	slope = 0.0;
	float	height = 0.0;
	for ( uint i=0; i < _SamplesCount; i++ ) {
		SB_Beckmann	sample = _SB_Beckmann[i];

		float	offsetPhase = sample.m_phase + position.x * sample.m_frequency.x + position.y * sample.m_frequency.y;
		float2	scOffsetPhase;
		sincos( offsetPhase, scOffsetPhase.x, scOffsetPhase.y );

		slope -= scOffsetPhase.x * sample.m_frequency;	// Derivative of cos() is -sin()
		height += scOffsetPhase.y;
	}
	float	scale = sqrt( 2.0 / _SamplesCount );
	slope *= scale;
	height *= scale;

//	_Tex_HeightField[pixelPosition] = float4( normalize( float3( -slope, 1.0 ) ), height );

// Poor attempt at compensating for size factor...
	float	s = 1;//_HeightFieldResolution.x / _size.x;
	_Tex_HeightField_Height[pixelPosition] = s * height;
	_Tex_HeightField_Normal[pixelPosition] = float4( normalize( float3( -s * slope, 1.0 ) ), s * height );
}
