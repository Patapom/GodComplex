////////////////////////////////////////////////////////////////////////////////
// Recomposes 4x4 quarter res textures into a full-res texture
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define THREADS_X	8
#define THREADS_Y	8

#define MAX_ANGLES	16

Texture2DArray< float4 >	_tex_splitIrradiance : register(t0);
Texture2DArray< float4 >	_tex_splitBentCone : register(t1);
Texture2D< float >			_tex_sourceDepth : register(t2);

RWTexture2D< float4 >		_tex_targetRadiance : register(u0);
RWTexture2D< float4 >		_tex_targetBentCone : register(u1);

cbuffer CB_DownSample : register( b3 ) {
	uint2	_targetSize;
};

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	sourcePixelIndex = uint3( _dispatchThreadID.xy, 0 );
	uint2	targetPixelIndex = _dispatchThreadID.xy << 2;

	// Read source 4x4 values
	float4	radiance[4*4];
	float4	bentCone[4*4];
	{
		for ( uint Y=0; Y < 4; Y++ ) {
			for ( uint X=0; X < 4; X++ ) {
				sourcePixelIndex.z = (Y << 2) + X;
				radiance[sourcePixelIndex.z] = _tex_splitIrradiance[sourcePixelIndex];
				bentCone[sourcePixelIndex.z] = _tex_splitBentCone[sourcePixelIndex];
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////
	//
// #TODO Mix & Recompose
	float3	sumRadiance = 0.0;
	float3	sumBentNormal = 0.0;
	float	sumAO = 0.0;
	{
		for ( uint Y=0; Y < 4; Y++ ) {
			for ( uint X=0; X < 4; X++ ) {
				sumRadiance += radiance[(Y << 2) + X].xyz;
				sumBentNormal += bentCone[(Y << 2) + X].xyz;
				sumAO += bentCone[(Y << 2) + X].w;
			}
		}
	}

	sumRadiance /= MAX_ANGLES;
	sumAO /= MAX_ANGLES;

	// Use AO to compute cone angle
	float	cosAverageConeAngle = 1.0 - sumAO;
	float3	csAverageBentNormal = normalize( sumBentNormal );
	float	stdDeviation = 0.0;

	const float	MIN_ENCODABLE_VALUE = 1.0 / 128.0;
	csAverageBentNormal *= sqrt( max( MIN_ENCODABLE_VALUE, cosAverageConeAngle ) );
	float4	csBentCone = float4( csAverageBentNormal, stdDeviation );

	{
		for ( uint Y=0; Y < 4; Y++ ) {
			for ( uint X=0; X < 4; X++ ) {
				radiance[(Y << 2) + X] = float4( sumRadiance, 0.0 );
				bentCone[(Y << 2) + X] = csBentCone;
			}
		}
	}
	//
	//////////////////////////////////////////////////////////////////////////////


	// Write result
	_tex_targetRadiance[targetPixelIndex] = radiance[4*0+0];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*0+0];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*0+1];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*0+1];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*0+2];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*0+2];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*0+3];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*0+3];
	targetPixelIndex.y++;

	// 2nd line
	_tex_targetRadiance[targetPixelIndex] = radiance[4*1+3];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*1+3];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*1+2];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*1+2];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*1+1];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*1+1];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*1+0];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*1+0];
	targetPixelIndex.y++;

	// 3rd line
	_tex_targetRadiance[targetPixelIndex] = radiance[4*2+0];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*2+0];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*2+1];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*2+1];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*2+2];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*2+2];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*2+3];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*2+3];
	targetPixelIndex.y++;

	// 4th line
	_tex_targetRadiance[targetPixelIndex] = radiance[4*3+3];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*3+3];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*3+2];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*3+2];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*3+1];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*3+1];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = radiance[4*3+0];
	_tex_targetBentCone[targetPixelIndex] = bentCone[4*3+0];
}
