#include "Includes/global.hlsl"
#include "Includes/Photons.hlsl"
#include "Includes/Room.hlsl"
#include "Includes/Noise.hlsl"
#include "Includes/HeightMap.hlsl"

#define	NUM_THREADSX	16
#define	NUM_THREADSY	16
#define	NUM_THREADSZ	1

//cbuffer CB_UpdateHeightMap : register(b2) {
//};

RWTexture2D< float2 >	_Tex_HeightMap : register(u0);


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	float2	UV = float2( _DispatchThreadID.xy ) / HEIGHTMAP_SIZE;
//	float2	PhiTheta = HeightMapUV2PhiTheta( UV );
	float3	wsDirection = HeightMapUV2Direction( UV );

	float3	UVW = 0.2 * wsDirection + 1.0 * _Time * float3( 0.12, -0.1, 0.19 );
//	float	H = sin( 4.0 * PhiTheta.y ) * cos( 4.0 * PhiTheta.x );
//	float	H = HEIGHTMAP_AMPLITUDE * (2.0 * _TexNoise.SampleLevel( LinearWrap, UVW, 0.0 ) - 1.0);
	float	H = HEIGHTMAP_AMPLITUDE * (2.0 * fbm( UVW ) - 1.0);

	// Estimate curvature
	const float2	dS = float2( 0.01, 0.0 );
	float2	dHx = HEIGHTMAP_AMPLITUDE * (2.0 * float2( fbm( UVW - dS.xyy ), fbm( UVW + dS.xyy ) ) - 1.0);
	float2	dHy = HEIGHTMAP_AMPLITUDE * (2.0 * float2( fbm( UVW + dS.yxy ), fbm( UVW + dS.yxy ) ) - 1.0);



	_Tex_HeightMap[_DispatchThreadID.xy] = H;
}
