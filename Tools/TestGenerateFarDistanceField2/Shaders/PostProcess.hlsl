#include "Global.hlsl"
#include "CommonDistanceField.hlsl"

Texture2DArray< float3 >	_TexSource : register(t0);
Texture2D< float >			_TexDepthStencil : register(t1);
Texture3D< float >			_TexDistanceField : register(t2);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	ComputeCameraSpacePosition( float2 _UV ) {
	float2	PixelPosition = _UV * iResolution.xy;
	float	Zproj = _TexDepthStencil[floor(PixelPosition)];
	float4	projPosition = float4( 2.0 * (PixelPosition.x + 0.5) / iResolution.x - 1.0, 1.0 - 2.0 * (PixelPosition.y + 0.5) / iResolution.y, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
	return csPosition.xyz / csPosition.w;
}

float3	ComputeIntersection( float3 _csPosition ) {

	float	maxDistance = length( _csPosition );
	float4	csView = float4( _csPosition / maxDistance, 1.0 );
	float4	csPos = 0.1 * csView;	// Walk away a little

maxDistance = min( 20.0, maxDistance );

	[fastopt]
	[loop]
	while ( csPos.w < maxDistance ) {
		float3	vxPos = CameraSpace2Voxel( csPos.xyz );
		float	csDistance = max( 0.01, VOXEL_SIZE * SampleDistanceLevel( _TexDistanceField, vxPos, 0.0 ) );
		csPos += csDistance * csView;
		if ( csDistance < 0.01 )
			break;
	}

	return csPos.xyz;
}

float3	ComputeAO( float3 _csPosition, float3 _csNormal ) {

//return 1.0 * _TexDistanceField.Sample( LinearClamp, INV_VOXELS_COUNT * CameraSpace2Voxel( _csPosition ) );
//return 0.01 * csPosition.z;

	// Try computing normal from the distance field (TODO!)
//_csNormal = normalize( ComputeNormal( _TexDistanceField, CameraSpace2Voxel( _csPosition ), 1.0 ) );
//return _csNormal;
//float3	wsNormal = mul( float4( _csNormal, 0.0 ), _Camera2World ).xyz;
//return normalize( wsNormal );

#if 0
	// Try cone tracing
#else
	// Sample a few times
	float	stepSize = 0.4;
	uint	stepsCount = 8;
	float4	csUnitStep = float4( _csNormal, 1.0 );
	float4	csStep = stepSize * csUnitStep;
	float4	csPos = float4( _csPosition, 0.0 ) + 0.0 * csUnitStep;
	float	sumDistances = 0.0;
	for ( uint i=0; i < stepsCount; i++ ) {
		float	distance = SampleDistanceLevel( _TexDistanceField, CameraSpace2Voxel( csPos.xyz ), 0.0 );
		sumDistances += distance;
		csPos += csStep;
	}

	return saturate( VOXEL_SIZE * sumDistances / csPos.w );
#endif
}


float3	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	PixelPos = uint2(_In.__Position.xy);
	float2	UV = _In.__Position.xy / iResolution.xy;

	float3	Color = _TexSource[uint3(PixelPos,0)];
	float3	wsNormal = _TexSource[uint3(PixelPos,1)];
	float3	csNormal = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;
//Color = csNormal;

	float3	csPosition = ComputeCameraSpacePosition( UV );

	if ( all( UV < 0.4 ) ) {
		UV /= 0.4;

wsNormal = _TexSource.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;
csNormal = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;

		float3	csPosition = ComputeCameraSpacePosition( UV );
		float3	csRayMarchedPosition = ComputeIntersection( csPosition );
//return 0.05 * length( csRayMarchedPosition );
return ComputeAO( csPosition, csNormal );
return ComputeAO( csRayMarchedPosition, csNormal );

		float	time = 0.25 * iGlobalTime;
//		float	time = 4.0 * iGlobalTime;
//		float3	UVW = float3( UV, abs( 2.0 * frac( time ) - 1.0 ) );
		float3	UVW = float3( UV, (0.5 + floor( 64.0 * abs( 2.0 * frac( time ) - 1.0 ) )) / 64.0 );
		Color = 1/32.0 * _TexDistanceField.SampleLevel( LinearClamp, UVW, 0.0 );
//		Color = Color.z >= 1.0 ? float3( 0, 0, 0 ) : Color;
return Color;
	}

	float	AO = ComputeAO( csPosition, csNormal ).x;

	return AO * Color;
}
