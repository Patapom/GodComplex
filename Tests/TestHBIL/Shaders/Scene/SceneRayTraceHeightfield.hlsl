///////////////////////////////////////////////////////////////////////////////////
// Ray-tracing a heightfield
// I'm using the heightfield used in my ground truth tests to see if I'm getting the same result as the 2D "ground truth" version
//
///////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

Texture2D<float>	_tex_Height : register(t32);
Texture2D<float3>	_tex_NormalMap : register(t33);

static const float3	BOX_CENTER = float3( 0, 0, 0 );
static const float3	BOX_HALF_SIZE = float3( 2, 0.5, 2 );	// 2x2 m² and 50cm height

float2	ComputeBoxIntersections( float3 _position, float3 _view, float3 _boxCenter, float3 _boxInvHalfSize, out uint2 _hitSides ) {
	float3	normPos = (_position - _boxCenter) * _boxInvHalfSize;
	float3	normView = _view * _boxInvHalfSize;

	float2	tempX = float2( 1.0 - normPos.x, -1.0 - normPos.x ) / normView.x;
	float2	tempY = float2( 1.0 - normPos.y, -1.0 - normPos.y ) / normView.y;
	float2	tempZ = float2( 1.0 - normPos.z, -1.0 - normPos.z ) / normView.z;

	tempX = _view.x >= 0.0 ? tempX.yx : tempX;
	tempY = _view.y >= 0.0 ? tempY.yx : tempY;
	tempZ = _view.z >= 0.0 ? tempZ.yx : tempZ;

#if 1
	// Verbose code but returns hit sides
	_hitSides = uint2( _view.x >= 0.0 ? 0 : 1, _view.x >= 0.0 ? 1 : 0 );
	float2	hitDistances = tempX;
	if ( tempY.x > hitDistances.x ) {
		hitDistances.x = tempY.x;
		_hitSides.x = _view.y >= 0 ? 2 : 3;
	}
	if ( tempZ.x > hitDistances.x ) {
		hitDistances.x = tempZ.x;
		_hitSides.x = _view.z >= 0 ? 4 : 5;
	}
	if ( tempY.y < hitDistances.y ) {
		hitDistances.y = tempY.y;
		_hitSides.y = _view.y >= 0 ? 3 : 2;
	}
	if ( tempZ.y < hitDistances.y ) {
		hitDistances.y = tempZ.y;
		_hitSides.y = _view.z >= 0 ? 5 : 4;
	}

	return hitDistances;
#else
	// Fast result without hit side info
	_hitSides = 0;
	return float2(	max( max( tempX.x, tempY.x ), tempZ.x ),
					min( min( tempX.y, tempY.y ), tempZ.y ) );
#endif
}

float	ComputeHeightFieldIntersection( float3 _lsPosIn, float3 _lsDir, float _length, uint _stepsCount, inout float3 _lsNormal ) {
	float4	lsPos = float4( _lsPosIn, 0.0 );
	float4	lsStep = float4( _lsDir, 1.0 ) * _length / _stepsCount;

	float	prevH = _lsPosIn.y;
	[loop]
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		float2	UV = lsPos.xz;
		float	H = _tex_Height.SampleLevel( LinearClamp, UV, 0.0 );
		if ( lsPos.y < H ) {
			// Got a hit! Compute precise intersection...
			float	Z0 = lsPos.y - lsStep.y;
			float	Dz = lsStep.y;
			float	H0 = prevH;
			float	Dh = H - H0;
			float	t = (Z0 - H0) / (Dh - Dz);
			lsPos += (t-1.0) * lsStep;
			UV = lsPos.xz;
			_lsNormal = _tex_NormalMap.SampleLevel( LinearClamp, UV, 0.0 );
			return lsPos.w;
		}
		lsPos += lsStep;
		prevH = H;
	}

	return 1e6;
}

Intersection	TraceScene( float3 _wsPos, float3 _wsDir ) {
	Intersection	result = (Intersection) 0;
	result.shade = 0;
	result.wsHitPosition = float4( _wsPos, 0.0 );

	// Test the bounding-box
	float2	hitDistance = float2( 1e6, 0 );
	uint2	hitSides;
	float2	testDistances = ComputeBoxIntersections( _wsPos, _wsDir, BOX_CENTER, 1.0 / BOX_HALF_SIZE, hitSides );

	float3	wsNormal = float3( 0, 1, 0 );
	if ( testDistances.x < testDistances.y ) {
		float3	wsPosIn = _wsPos + testDistances.x * _wsDir;
		float3	wsPosOut = _wsPos + testDistances.y * _wsDir;

		const float3	BBOX_MIN = BOX_CENTER - BOX_HALF_SIZE;
		float3	lsPosIn = (wsPosIn - BBOX_MIN) * 0.5 / BOX_HALF_SIZE;
		float3	lsPosOut = (wsPosOut - BBOX_MIN) * 0.5 / BOX_HALF_SIZE;
		float3	lsDir = lsPosOut - lsPosIn;
		float	L = length( lsDir );
				lsDir /= L;

		float3	lsNormal = 0;
		float	lsHitDistance = ComputeHeightFieldIntersection( lsPosIn, lsDir, L, 100, lsNormal );
		float3	lsHitPos = lsPosIn + lsHitDistance * lsDir;
		float3	wsHitPos = BBOX_MIN + lsHitPos * 2.0 * BOX_HALF_SIZE;

		hitDistance.x = length( wsHitPos - _wsPos );
//hitDistance.x = testDistances.x;
		wsNormal = float3( lsNormal.x, lsNormal.z, -lsNormal.y );
	}

	// Update result
	result.shade = step( 1e-5, hitDistance.x );
	result.wsHitPosition += hitDistance.x * float4( _wsDir, result.shade );	// W kept at 0 (invalid) if no hit
	result.wsNormal = wsNormal;
	result.albedo = 0.5 * float3( 1.0, 0.9, 0.7 );
	result.wsVelocity = 0;
	result.roughness = 0;
	result.F0 = 0;
	result.materialID = 0;

	return result;
}

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax ) {
//	LightInfoPoint	lightInfo;
//					lightInfo.flux = LIGHT_ILLUMINANCE;
//					lightInfo.wsPosition = CORNELL_LIGHT_POS;
//					lightInfo.distanceAttenuation = float2( 100.0, 1000.0 );	// Don't care, let natural 1/r² take care of attenuation
//
	LightingResult	result = (LightingResult) 0;
//	ComputeLightPoint( _wsPosition, _wsNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}
