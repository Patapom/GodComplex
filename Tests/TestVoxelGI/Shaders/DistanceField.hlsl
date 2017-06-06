// Distance Field Scene Tracer
//
static const float3	CORNELL_SIZE = float3( 5.528f, 5.488f, 5.592f );
static const float3	CORNELL_POS = 0.0;
static const float	CORNELL_THICKNESS = 0.1f;

static const float3	CORNELL_SMALL_BOX_SIZE = float3( 1.65, 1.65, 1.65 );	// It's a cube
static const float3	CORNELL_SMALL_BOX_POS = float3( 1.855, 0.5 * CORNELL_SMALL_BOX_SIZE.y, 1.69 ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float	CORNELL_SMALL_BOX_ANGLE = 0.29145679447786709199560462143289;	// ~16°

static const float3	CORNELL_LARGE_BOX_SIZE = float3( 1.65, 3.3, 1.65 );
static const float3	CORNELL_LARGE_BOX_POS = float3( 3.685, 0.5 * CORNELL_LARGE_BOX_SIZE.y, 3.6125 ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float	CORNELL_LARGE_BOX_ANGLE = -0.30072115015043337195437489062082;	// ~17°

static const float3	CORNELL_LIGHT_SIZE = float3( 1.3, 0.0, 1.05 );
static const float3	CORNELL_LIGHT_POS = float3( 2.78, 5.2, 2.795 ) - 0.0 * float3( CORNELL_LIGHT_SIZE.x, 0.0, CORNELL_LIGHT_SIZE.z ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );

float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize ) {
	_wsPosition -= _wsBoxCenter;
	_wsBoxSize *= 0.5;

	float x = max( _wsPosition.x - _wsBoxSize.x, -_wsPosition.x - _wsBoxSize.x );
	float y = max( _wsPosition.y - _wsBoxSize.y, -_wsPosition.y - _wsBoxSize.y );
	float z = max( _wsPosition.z - _wsBoxSize.z, -_wsPosition.z - _wsBoxSize.z );
	return max( max( x, y ), z );
}
float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize, float _rotationAngle ) {
	float	s = sin( _rotationAngle );
	float	c = cos( _rotationAngle );
	float3	rotatedX = float3( c, 0.0, s );
	float3	rotatedZ = float3( -s, 0.0, c );

	_wsPosition -= _wsBoxCenter;
	_wsPosition = float3( dot( _wsPosition, rotatedX ), _wsPosition.y, dot( _wsPosition, rotatedZ ) );
	return DistBox( _wsPosition, 0.0, _wsBoxSize );
}
float2	DistMin( float2 a, float2 b ) {
	return a.x < b.x ? a : b;
}

float2	Map( float3 _wsPosition );

float3	Normal( float3 _wsPosition ) {
	const float	eps = 0.001;

	_wsPosition.x += eps;
	float	Dx = Map( _wsPosition ).x;
	_wsPosition.x -= 2.0 * eps;
	Dx -= Map( _wsPosition ).x;
	_wsPosition.x += eps;

	_wsPosition.y += eps;
	float	Dy = Map( _wsPosition ).x;
	_wsPosition.y -= 2.0 * eps;
	Dy -= Map( _wsPosition ).x;
	_wsPosition.y += eps;

	_wsPosition.z += eps;
	float	Dz = Map( _wsPosition ).x;
	_wsPosition.z -= 2.0 * eps;
	Dz -= Map( _wsPosition ).x;
//	_wsPosition.x += eps;

// 	Dx *= 2.0 / eps;
// 	Dy *= 2.0 / eps;
// 	Dz *= 2.0 / eps;

	return normalize( float3( Dx, Dy, Dz ) );
}

/// <summary>
/// Maps a world-space position into a distance to the nearest object in the scene
/// </summary>
/// <param name="_wsPosition"></param>
/// <returns></returns>
float2	Map( float3 _wsPosition ) {
	// Walls
	float2	distance = float2( DistBox( _wsPosition, float3( 0, 0, 0 ), float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 1.0 );	// Floor
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0, CORNELL_SIZE.y, 0 ), float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 2.0 ) );	// Ceiling
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( -0.5f * CORNELL_SIZE.x, 0.5f * CORNELL_SIZE.y, 0 ), float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 3.0 ) );	// Left wall
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0.5f * CORNELL_SIZE.x, 0.5f * CORNELL_SIZE.y, 0 ), float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 4.0 ) );	// Right wall
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0, 0.5f * CORNELL_SIZE.y, 0.5f * CORNELL_SIZE.z ), float3( CORNELL_SIZE.x, CORNELL_SIZE.y, CORNELL_THICKNESS ) ), 5.0 ) );	// Back wall

	// Small box
	distance = DistMin( distance, float2( DistBox( _wsPosition, CORNELL_SMALL_BOX_POS, CORNELL_SMALL_BOX_SIZE, CORNELL_SMALL_BOX_ANGLE ), 6.0 ) );

	// Large box
	distance = DistMin( distance, float2( DistBox( _wsPosition, CORNELL_LARGE_BOX_POS, CORNELL_LARGE_BOX_SIZE, CORNELL_LARGE_BOX_ANGLE ), 7.0 ) );

	return distance;
}

float3	Albedo( float3 _wsPosition, float _materialID ) {
	switch ( (int) _materialID ) {
		case 3:		return 0.6 * float3( 0.2, 0.5, 1.0 ); break;
		case 4:		return 0.6 * float3( 1.0, 0.1, 0.01 ); break;
		default:	return 0.6; break;
	}
}

// Traces a ray into the scene and returns hit distance and material ID
float2	Trace( float3 _wsPos, float3 _wsDir, float _initialDistance, const uint _stepsCount ) {
	float4	wsPos = float4( _wsPos, 0.0 );
	float4	wsView = float4( _wsDir, 1.0 );
	float2	distance = float2( _initialDistance, -1 );
	for ( uint i=0; i < _stepsCount; i++ ) {
		wsPos += distance.x * wsView;
		distance = Map( wsPos.xyz );
		if ( distance.x < 0.001 )
			break;
	}
	distance.x = wsPos.w;
	return distance;
}

// Traces a ray from source to target and returns the visibility
float	ShadowTrace( float3 _wsSource, float3 _wsTarget, const uint _stepsCount ) {
#if 1
	float3	wsDir = _wsTarget - _wsSource;
	float	dist = length( wsDir );
			wsDir /= dist;
	float2	hitDistance = Trace( _wsSource, wsDir, 0.01, _stepsCount );
	return smoothstep( 0.95, 1.0, hitDistance.x / dist );
#else
	float4	wsPos = float4( _wsSource, 0.0 );
	_wsTarget -= _wsSource;
	float4	wsView = float4( _wsTarget, length( _wsTarget ) );
	float	remainingDistance = wsView.w;
			wsView /= remainingDistance;

	for ( uint i=0; i < _stepsCount; i++ ) {
		float	distance = Map( wsPos.xyz ).x;
		if ( distance.x > remainingDistance )
			return 1.0;	// Unoccluded!
		wsPos += distance * wsView;
		remainingDistance -= distance;
	}

	return 0.0;
#endif
}
