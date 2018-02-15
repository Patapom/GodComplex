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
Texture2D< float3 >			_tex_sourceNormal : register(t3);

RWTexture2D< float4 >		_tex_targetRadiance : register(u0);
RWTexture2D< float4 >		_tex_targetBentCone : register(u1);

cbuffer CB_DownSample : register( b3 ) {
	uint2	_targetSize;
};

float	BilateralWeight( float4 _normalDepth0, float4 _normalDepth1 ) {
	float	deltaZ = abs( _normalDepth0.w - _normalDepth1.w );
	float	filterZ = smoothstep( 0.05, 0.01, deltaZ );
	float	dotN = dot( _normalDepth0.xyz, _normalDepth1.xyz );
	float	filterN = smoothstep( 0.95, 1.0, dotN );
//	return lerp( 0.01, 1.0, filterZ );
//	return lerp( 0.01, 1.0, filterN );
	return lerp( 0.01, 1.0, filterN * filterZ );
	return 1.0;	// Average all
}

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint3	sourcePixelIndex = uint3( _dispatchThreadID.xy, 0 );
	uint2	targetPixelIndex = _dispatchThreadID.xy << 2;

	//////////////////////////////////////////////////////////////////////////////
	// Read source 4x4 values from each little buffer
	float4	sourceRadiance[4*4];
	float4	sourceBentCone[4*4];
	float4	normalDepth[4*4];
	{
		for ( uint Y=0; Y < 4; Y++ ) {
			for ( uint X=0; X < 4; X++ ) {
				sourcePixelIndex.z = (Y << 2) + X;
				sourceRadiance[sourcePixelIndex.z] = _tex_splitIrradiance[sourcePixelIndex];
				sourceBentCone[sourcePixelIndex.z] = _tex_splitBentCone[sourcePixelIndex];
				normalDepth[sourcePixelIndex.z].xyz = _tex_sourceNormal[targetPixelIndex + uint2( X, Y )];
				normalDepth[sourcePixelIndex.z].w = _tex_sourceDepth[targetPixelIndex + uint2( X, Y )];
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////
	// Mix & Recompose
	float4	targetRadiance[4*4];
	float4	targetBentCone[4*4];
	#if 0

Ca coûte trop cher!
Adapter le filtre Z au Z moyen!!! => Relative Z!

		for ( uint Y0=0; Y0 < 4; Y0++ ) {
			for ( uint X0=0; X0 < 4; X0++ ) {
				uint	index0 = (Y0 << 2) + X0;
				float4	radiance0 = sourceRadiance[index0];
				float4	bentCone0 = sourceBentCone[index0];
				float4	normalDepth0 = normalDepth[index0];

				float4	sumRadiance = 0.0;
				float4	sumBentCone = 0.0;
				for ( uint Y1=0; Y1 < 4; Y1++ ) {
					for ( uint X1=0; X1 < 4; X1++ ) {
						uint	index1 = (Y1 << 2) + X1;
						float4	radiance1 = float4( sourceRadiance[index1].xyz, 1.0 );
						float4	bentCone1 = sourceBentCone[index1];
						float4	normalDepth1 = normalDepth[index1];

						float	weight = BilateralWeight( normalDepth0, normalDepth1 );
						sumRadiance += weight * radiance1;
						sumBentCone += weight * bentCone1;
					}
				}

				float	invWeight = 1.0 / sumRadiance.w;
				sumRadiance.xyz *= invWeight;
				sumBentCone.w *= invWeight;

				// Use AO to compute cone angle
				float	sumAO = sumBentCone.w;
				float	cosAverageConeAngle = 1.0 - sumAO;
				float3	csAverageBentNormal = normalize( sumBentCone.xyz );
				float	stdDeviation = 0.0;

				const float	MIN_ENCODABLE_VALUE = 1.0 / 128.0;
				csAverageBentNormal *= sqrt( max( MIN_ENCODABLE_VALUE, cosAverageConeAngle ) );
				float4	csBentCone = float4( csAverageBentNormal, stdDeviation );

				targetRadiance[index0] = sumRadiance;
				targetBentCone[index0] = csBentCone;
			}
		}
	#else
		//////////////////////////////////////////////////////////////////////////////
		// Naive average stored for all pixels
		float3	sumRadiance = 0.0;
		float4	sumBentNormalAO = 0.0;
		{
			for ( uint Y=0; Y < 4; Y++ ) {
				for ( uint X=0; X < 4; X++ ) {
					sumRadiance += sourceRadiance[(Y << 2) + X].xyz;
					sumBentNormalAO += sourceBentCone[(Y << 2) + X];
				}
			}
		}

		sumRadiance /= MAX_ANGLES;
		sumBentNormalAO.w /= MAX_ANGLES;

		// Use AO to compute cone angle
		float	cosAverageConeAngle = 1.0 - sumBentNormalAO.w;
		float3	csAverageBentNormal = normalize( sumBentNormalAO.xyz );
		float	stdDeviation = 0.0;

		const float	MIN_ENCODABLE_VALUE = 1.0 / 128.0;
		csAverageBentNormal *= sqrt( max( MIN_ENCODABLE_VALUE, cosAverageConeAngle ) );
		float4	csBentCone = float4( csAverageBentNormal, stdDeviation );

		{
			for ( uint Y=0; Y < 4; Y++ ) {
				for ( uint X=0; X < 4; X++ ) {
					targetRadiance[(Y << 2) + X] = float4( sumRadiance, 0.0 );
					targetBentCone[(Y << 2) + X] = csBentCone;
				}
			}
		}
	#endif


	//////////////////////////////////////////////////////////////////////////////
	// Write result
	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*0+0];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*0+0];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*0+1];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*0+1];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*0+2];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*0+2];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*0+3];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*0+3];
	targetPixelIndex.y++;

	// 2nd line
	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*1+3];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*1+3];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*1+2];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*1+2];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*1+1];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*1+1];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*1+0];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*1+0];
	targetPixelIndex.y++;

	// 3rd line
	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*2+0];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*2+0];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*2+1];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*2+1];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*2+2];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*2+2];
	targetPixelIndex.x++;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*2+3];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*2+3];
	targetPixelIndex.y++;

	// 4th line
	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*3+3];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*3+3];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*3+2];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*3+2];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*3+1];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*3+1];
	targetPixelIndex.x--;

	_tex_targetRadiance[targetPixelIndex] = targetRadiance[4*3+0];
	_tex_targetBentCone[targetPixelIndex] = targetBentCone[4*3+0];
}
