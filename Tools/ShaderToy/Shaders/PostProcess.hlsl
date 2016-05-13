#include "Includes/global.hlsl"
#include "Includes/ScreenSpaceRayTracing.hlsl"

Texture2DArray<float4>	_TexSource : register(t0);
Texture2D<float>		_TexLinearDepth : register(t1);
Texture2D<float4>		_TexDownsampledDepth : register(t2);

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;

	float4	Color = _TexSource[uint3( _In.__Position.xy, 0)];
	float	sceneDistance = Color.w;
	float4	Depth = _TexSource[uint3( _In.__Position.xy, 1)];
//Depth = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 1.0 );


	// Build reflection with a plane
	const float	TAN_HALF_FOV_Y = 0.57735026918962576450914878050196;	// Assuming a 60° FOV
	const float	ASPECT_RATIO = iResolution.x / iResolution.y;
	const float	TAN_HALF_FOV_X = ASPECT_RATIO * TAN_HALF_FOV_Y;

	float3	csDir = normalize( float3( TAN_HALF_FOV_X * (2.0 * UV.x - 1.0), TAN_HALF_FOV_Y * (1.0 - 2.0 * UV.y), 1.0 ) );
	float3	wsDir = mul( float4( csDir, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	const float	PLANE_HEIGHT = 0.1;
	float	hitDistance = -(wsPos.y - PLANE_HEIGHT) / wsDir.y;
	if ( hitDistance > 0.0 && hitDistance < sceneDistance ) {
		float3	wsHit = wsPos + hitDistance * wsDir;
		float3	wsNormal = float3( 0, 1, 0 );	// Let's make it wavy later...
		float3	wsReflect = reflect( wsDir, wsNormal );

		const uint	MAX_STEPS = 64;

		float3	csHit = mul( float4( wsHit, 1.0 ), _World2Camera ).xyz;
		float3	csReflect = mul( float4( wsReflect, 0.0 ), _World2Camera ).xyz;

		uint2	Dims;
		_TexDownsampledDepth.GetDimensions( Dims.x, Dims.y );

		// Compute screen space reflection and blend with background color
		// Actually, at the moment we simply replace the background color but we should use Fresnel equations to blend correctly...
		float2	hitUV;
		float	reflectionDistance;
		float	blend = ScreenSpaceRayTrace( csHit, csReflect, 100.0, _TexDownsampledDepth, Dims, _Camera2Proj, MAX_STEPS, reflectionDistance, hitUV );
		float3	SKY_COLOR = float3( 1, 0, 0 );
		Color.xyz = lerp( SKY_COLOR, _TexSource.SampleLevel( LinearClamp, float3( hitUV, 0.0 ), 0.0 ).xyz, blend );
	}


	Color.xyz = pow( max( 0.0, Color.xyz ), 1.0 / 2.2 );	// Gamma-correct

//return 0.1 * hitDistance;
//return 0.1 * sceneDistance;


//return 0.1 * _TexSource.SampleLevel( LinearClamp, float3( UV, 0.0 ), 0.0 ).w;	// Show distance
//return 0.1 * _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).w;	// Show projZ

//float	projZ = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).w;
//float	temp = _Proj2Camera[2].w * projZ + _Proj2Camera[3].w;
//return 0.1 * projZ / temp;	// Show linear Z

//return 0.1 * _TexLinearDepth.SampleLevel( LinearClamp, UV, 0.0 );
//return 0.1 * _TexDownsampledDepth.SampleLevel( LinearClamp, UV, 1.0 ).xyz;
//return 0.1 * _TexDownsampledDepth.SampleLevel( LinearClamp, UV, 1.0 ).xyz;



//uint	mipLevel = 4;
//Color = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 0)].xyz;
//Depth = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 1)];

//return 0.1 * Depth.x;


/*
uint	mipLevel = 2;
uint2	pixelPos = _In.__Position.xy;
		pixelPos >>= mipLevel;
float3	V = mipLevel > 0 ? _TexDownsampledDepth.mips[mipLevel-1][pixelPos].xyz : _TexLinearDepth.SampleLevel( LinearClamp, UV, 0.0 );
//return 1.0 * (V.z - V.y);
return 0.1 * V;
*/
	return Color.xyz;
}
