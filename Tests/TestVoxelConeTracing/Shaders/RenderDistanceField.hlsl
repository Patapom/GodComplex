//////////////////////////////////////////////////////////////////////////
// This shader performs ray-marching in a distance field
//////////////////////////////////////////////////////////////////////////
//
//
#include "Global.hlsl"

static const float3	CORNELL_SIZE = float3( 5.528, 5.488, 5.592 );
static const float3	CORNELL_POS = 0;//float3( -0.5 * CORNELL_SIZE.x, 0.0, -0.5 * CORNELL_SIZE.z );
static const float	CORNELL_THICKNESS = 0.1;

static const float3	CORNELL_SMALL_BOX_SIZE = 1.65;	// It's a cube
static const float	CORNELL_SMALL_BOX_ANGLE = 0.29145679447786709199560462143289;	// ~16°
static const float3	CORNELL_SMALL_BOX_POS = float3( 1.855, 0.5 * CORNELL_SMALL_BOX_SIZE.y, 1.69 ) - 0.5 * float3( CORNELL_SIZE.x, 0, CORNELL_SIZE.z );

static const float3	CORNELL_LARGE_BOX_SIZE = float3( 1.65, 3.3, 1.65 );
static const float	CORNELL_LARGE_BOX_ANGLE = -0.30072115015043337195437489062082;	// ~17°
static const float3	CORNELL_LARGE_BOX_POS = float3( 3.685, 0.5 * CORNELL_LARGE_BOX_SIZE.y, 3.5125 ) - 0.5 * float3( CORNELL_SIZE.x, 0, CORNELL_SIZE.z );

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION {
	return __Position;
}

float	DistPlane( float3 _wsPosition, float4 _wsPlane ) {
	return dot( _wsPosition, _wsPlane.xyz ) + _wsPlane.w;
}
// iQ's code doesn't seem to work
//float	DistBox( float3 _wsPosition ) {
////return length( _wsPosition ) - 1.0;
////	return length( max( abs(_wsPosition), 0.0 ) );
//	float3	d = abs(_wsPosition);
//	return min( max( d.x, max( d.y, d.z ) ), 0.0 ) + length( max( d, 0.0 ) );
//}

float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize ) {
//	return DistBox( (_wsPosition - _wsBoxCenter) * 2.0 / _wsBoxSize );
	_wsPosition -= _wsBoxCenter;
	_wsBoxSize *= 0.5;

	float x = max( _wsPosition.x - _wsBoxSize.x, -_wsPosition.x - _wsBoxSize.x );
	float y = max( _wsPosition.y - _wsBoxSize.y, -_wsPosition.y - _wsBoxSize.y );
	float z = max( _wsPosition.z - _wsBoxSize.z, -_wsPosition.z - _wsBoxSize.z );
	return max( max( x, y ), z );
}
float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize, float _rotationAngle ) {
	float2	sc;
	sincos( _rotationAngle, sc.x, sc.y );
	float3	rotatedX = float3( sc.y, 0.0, sc.x );
	float3	rotatedZ = float3( -sc.x, 0.0, sc.y );

	_wsPosition -= _wsBoxCenter;
	_wsPosition = float3( dot( _wsPosition, rotatedX ), _wsPosition.y, dot( _wsPosition, rotatedZ ) );
	return DistBox( _wsPosition, 0, _wsBoxSize );
}
float2	DistMin( float2 a, float2 b ) {
	return a.x < b.x ? a : b;
}
float2	Map( float3 _wsPosition ) {
	// Walls
	float2	distance = float2( DistBox( _wsPosition, float3( 0, 0, 0 ), float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 1.0 );	// Floor
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0, CORNELL_SIZE.y, 0 ), float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 2.0 ) );	// Ceiling
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( -0.5 * CORNELL_SIZE.x, 0.5 * CORNELL_SIZE.y, 0 ), float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 3.0 ) );	// Left wall
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0.5 * CORNELL_SIZE.xy, 0 ), float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 4.0 ) );	// Right wall
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0, 0.5 * CORNELL_SIZE.y, 0.5 * CORNELL_SIZE.z ), float3( CORNELL_SIZE.xy, CORNELL_THICKNESS ) ), 5.0 ) );	// Back wall

	// Small box
	distance = DistMin( distance, float2( DistBox( _wsPosition, CORNELL_SMALL_BOX_POS, CORNELL_SMALL_BOX_SIZE, CORNELL_SMALL_BOX_ANGLE ), 6.0 ) );

	// Large box
	distance = DistMin( distance, float2( DistBox( _wsPosition, CORNELL_LARGE_BOX_POS, CORNELL_LARGE_BOX_SIZE, CORNELL_LARGE_BOX_ANGLE ), 7.0 ) );

	return distance;
}

float3	ComputeNormal( float3 _wsPosition ) {
	const float2	eps = float2( 0.001, 0 );
	return float3(	Map( _wsPosition + eps.xyy ).x - Map( _wsPosition - eps.xyy ).x,
					Map( _wsPosition + eps.yxy ).x - Map( _wsPosition - eps.yxy ).x,
					Map( _wsPosition + eps.yyx ).x - Map( _wsPosition - eps.yyx ).x ) * (0.5 / eps.x);
}

float3	ComputeSceneColor( float3 _wsPos, float2 _intersection ) {
//return float3( 1, 0, 0 );
//return 0.1 * _wsPos;
//return 10.0 * _intersection.x;
//return float3( 1, 0, 0 );
	float3	wsNormal = ComputeNormal( _wsPos );
	return wsNormal;
}

float3	PS( float4 __Position : SV_POSITION ) : SV_TARGET0 {
	float2	UV = __Position.xy / float2( _resolution );
	float3	csView = float3( _tanHalfFOV.x * (2.0 * UV.x - 1.0), _tanHalfFOV.y * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
//return csView;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;
//return wsView;

	float2	intersection = float2( INFINITY, -1.0 );
	for ( uint stepIndex=0; stepIndex < 128; stepIndex++ ) {
		intersection = Map( wsPos );
		wsPos += intersection.x * wsView;
		if ( intersection.x < 1e-3 ) {
			return ComputeSceneColor( wsPos, intersection );	// We got an intersection!
		}
	}

//return intersection.x;
//return float3( 1, 0, 1 );
	return 0.0;
}
