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
//	float	Zproj = _TexDepthStencil[floor(PixelPosition)];
	float	Zproj = _TexDepthStencil.SampleLevel( LinearClamp, _UV, 0.0 );
	float4	projPosition = float4( 2.0 * (PixelPosition.x + 0.5) / iResolution.x - 1.0, 1.0 - 2.0 * (PixelPosition.y + 0.5) / iResolution.y, Zproj, 1.0 );
	float4	csPosition = mul( projPosition, _Proj2Camera );
	return csPosition.xyz / csPosition.w;
}

float4	ComputeIntersection( float3 _csPosition, float3 _csDirection, float _maxDistance, float _initialStepSize=0.0, float _eps=0.2 ) {
	float4	csDir = float4( _csDirection, 1.0 );
	float4	csPos = float4( _csPosition, 0.0 ) + _initialStepSize * csDir;

	[fastopt]
	[loop]
	while ( csPos.w < _maxDistance ) {
		float3	vxPos = CameraSpace2Voxel( csPos.xyz );
		float	csDistance = VOXEL_SIZE * SampleDistanceLevel( _TexDistanceField, vxPos, 0.0 );
		csPos += csDistance * csDir;
		if ( csDistance < _eps )
			break;
		csPos += saturate( _eps - csDistance ) * csDir;	// Make sure we take a minimal step each time
	}

//float	eps = 0.1;
//float	value = (csPos.w-_maxDistance) / eps;
//return lerp( float3( 0,0,1 ), lerp( float3( 0,1,0 ), float3( 1,0,0 ), saturate( value ) ), saturate( 1.0+value ) );
//return 0.5 * csPos.w / _maxDistance;

	return csPos;
}

float3	ComputeAO( float3 _csPosition, float3 _csNormal, float2 _pixelPosition ) {

//return 1.0 * _TexDistanceField.Sample( LinearClamp, INV_VOXELS_COUNT * CameraSpace2Voxel( _csPosition ) );
//return 0.01 * csPosition.z;

	// Try computing normal from the distance field (TODO!)
//_csNormal = normalize( ComputeNormal( _TexDistanceField, CameraSpace2Voxel( _csPosition ), 1.0 ) );
//return _csNormal;
//float3	wsNormal = mul( float4( _csNormal, 0.0 ), _Camera2World ).xyz;
//return normalize( wsNormal );

#if 0
	// Try multiple traces
	const uint	SAMPLES_COUNT = 16U;
	const float	INITIAL_STEP = 0.1;
	const float	MAX_AO_DISTANCE = 8.0;

	float3	csTangent = normalize( cross( float3( 0, 0, -1 ), _csNormal ) );
	float3	csBiTangent = cross( csTangent, _csNormal );
	float	sumHitDistances = 0.0;
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float	rand = frac( sin(_pixelPosition.x*i)*sin(1767.0654+_pixelPosition.y*i)*43758.5453123 );
		float	theta = 0.8 * 2.0 * asin( sqrt( 0.5 * rand ) );	// in [0,PI/2]
		float2	scTheta;
		sincos( theta, scTheta.x, scTheta.y );
		float2	scPhi;
		sincos( 2.0*PI*i/SAMPLES_COUNT, scPhi.x, scPhi.y );

		float3	tsDir = float3( scTheta.x * scPhi.y, scTheta.x * scPhi.x, scTheta.y );
		float3	csDir = tsDir.x * csTangent + tsDir.y * csBiTangent + tsDir.z * _csNormal;

		float4	csHit = ComputeIntersection( _csPosition, csDir, MAX_AO_DISTANCE, INITIAL_STEP, 0.04 );
//return 0.5 * csHit.w / MAX_AO_DISTANCE;
		sumHitDistances += min( csHit.w, MAX_AO_DISTANCE );
	}

	return sumHitDistances / (SAMPLES_COUNT * MAX_AO_DISTANCE);

#elif 0
	// "Cone tracing"
	float4	csDir = float4( _csNormal, 1.0 );
	float4	csPos = float4( _csPosition, 0.0 ) + 0.05 * _csPosition.z * csDir;	// Walk away a little

	float	maxDistance = 8.0;		// Don't go further than 1m
	uint	stepsCount = 32;			// Don't use more than 8 steps!
	float	csDistance = 1e6;
	float2	sumConeAngles = 0.0;
	[fastopt]
	[loop]
	while ( csPos.w < maxDistance && csDistance > 0.01 && stepsCount-- ) {
		csDistance = VOXEL_SIZE * SampleDistanceLevel( _TexDistanceField, CameraSpace2Voxel( csPos.xyz ), 0.0 );
		csPos += csDistance * csDir;

		// Accumulate cone angles...
		float	sinConeAngle = csDistance / csPos.w;
		sumConeAngles += float2( sinConeAngle, 1.0 );
	}

//return 0.1 * csPos.w;
return csPos.w / maxDistance;
return 1.0 - stepsCount / 32.0;

	if ( csDistance <= 0.01 )
		return 0.0;

//	float	averageConAngle = sumConeAngles.y > 0.0 ? 2.0 * INVPI * asin( sumConeAngles.x / sumConeAngles.y ) : 1.0;
	float	averageConAngle = 2.0 * INVPI * asin( sumConeAngles.x / sumConeAngles.y );
	return averageConAngle;

#elif 0
	// Sample a few times
	float	stepSize = 0.2;
	uint	stepsCount = 8;
	float4	csUnitStep = float4( _csNormal, 1.0 );
	float4	csStep = stepSize * csUnitStep;
	float4	csPos = float4( _csPosition, 0.0 ) + 0.01 * csUnitStep;
	float	sumDistances = 0.0;
	for ( uint i=0; i < stepsCount; i++ ) {
		float	distance = SampleDistanceLevel( _TexDistanceField, CameraSpace2Voxel( csPos.xyz ), 0.0 );
		sumDistances += distance;
		csPos += csStep;
	}

	return saturate( VOXEL_SIZE * sumDistances / csPos.w );
#else
	// Sample a few times
	float	stepSize = 0.1;
	uint	stepsCount = 10;
	float4	csUnitStep = float4( _csNormal, 1.0 );
	float4	csStep = stepSize * csUnitStep;
	float4	csPos = float4( _csPosition, 0.0 ) + 0.4 * csUnitStep;
	float	sumAO = 0.0;
	for ( uint i=0; i < stepsCount; i++ ) {
		float	distance = VOXEL_SIZE * SampleDistanceLevel( _TexDistanceField, CameraSpace2Voxel( csPos.xyz ), 0.0 );
//		sumAO += (csPos.w - distance) * pow( 2.0, 1.0 / (1+i) );
		sumAO += (csPos.w - distance) * pow( 2.0, -2.0 * csPos.w );
		csPos += csStep;
	}

	return 1.0 - saturate( 4.0 * sumAO / stepsCount );
#endif
}


float3	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	PixelPos = uint2(_In.__Position.xy);
	float2	UV = _In.__Position.xy / iResolution.xy;

	float3	Color = _TexSource[uint3(PixelPos,0)];
	float3	wsNormal = _TexSource[uint3(PixelPos,1)];
	float3	csNormal = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;

	if ( all( UV < 0.4 ) ) {
		UV /= 0.4;

wsNormal = _TexSource.Sample( LinearClamp, float3( UV, 1.0 ) );
csNormal = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;
//wsNormal = mul( float4( csNormal, 0 ), _Camera2World ).xyz;

//csNormal = float3(	dot( wsNormal, _Camera2World[0].xyz ),
//					dot( wsNormal, _Camera2World[1].xyz ),
//					dot( wsNormal, _Camera2World[2].xyz ) );

//return wsNormal;
//return csNormal;
//return _Camera2World[0].xyz;

		#if 0	// Visualize distance field slices as orthographic projection
			uint3	voxelIndex = uint3( VOXELS_COUNT * UV.x, VOXELS_COUNT * (1.0-UV.y), 0 );
			for ( ; voxelIndex.z < VOXELS_COUNT; voxelIndex.z++ ) {
				if ( _TexDistanceField[voxelIndex] < 1.0 )
					break;
			}
			return float(voxelIndex.z) / VOXELS_COUNT;
		#endif

		#if 0	// Visualize distance field slices one by one using time for cycling
			float	time = 0.25 * iGlobalTime;
//			float	time = 4.0 * iGlobalTime;
//			float3	UVW = float3( UV, abs( 2.0 * frac( time ) - 1.0 ) );
			float3	UVW = float3( UV, (0.5 + floor( 64.0 * abs( 2.0 * frac( time ) - 1.0 ) )) / 64.0 );
			Color = 1/32.0 * _TexDistanceField.SampleLevel( LinearClamp, UVW, 0.0 );
//			Color = Color.z >= 1.0 ? float3( 0, 0, 0 ) : Color;
			return Color;
		#endif

		#if 1	// Visualize AO
			float3	csPosition = ComputeCameraSpacePosition( UV );
			float	maxDistance = length( csPosition );
			float3	csView = csPosition / maxDistance;

			wsNormal = _TexSource.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;
			csNormal = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;
			return ComputeAO( csPosition, csNormal, PixelPos );
		#endif

//maxDistance = 40.0;
//maxDistance = min( VOXEL_SIZE * VOXELS_COUNT, maxDistance );
//maxDistance = min( 40.0, maxDistance );

//		float3	csRayMarchedPosition = ComputeIntersection( 0.0, csView, maxDistance, 0.1 ).xyz;
		float3	csRayMarchedPosition = ComputeIntersection( csPosition, csNormal, 8.0, 0.1 ).xyz;
//return 0.1 * csRayMarchedPosition;
//return 1.0 * length( csRayMarchedPosition - csPosition );
//return ComputeAO( csRayMarchedPosition, csNormal );
	}

//return -_Camera2World[0].xyz;
//return wsNormal;
//return csNormal;

	float3	csPosition = ComputeCameraSpacePosition( UV );
	float	AO = ComputeAO( csPosition, csNormal, PixelPos ).x;

	return AO * Color;
}
