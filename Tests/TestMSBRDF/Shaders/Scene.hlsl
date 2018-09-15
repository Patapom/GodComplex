//////////////////////////////////////////////////////////////////////////////
// Scene tracing
static const float3	SPHERE_CENTER = float3( 0, 1, 0 );
static const float	SPHERE_RADIUS = 1;

float	RayTraceSphere( float3 _wsPos, float3 _wsDir, float3 _wsCenter, float _radius, out float3 _wsClosestPosition ) {
	float3	D = _wsPos - _wsCenter;
	_wsClosestPosition = _wsPos - dot( D, _wsDir ) * _wsDir;

	float	b = dot( D, _wsDir );
	float	c = dot( D, D ) - _radius*_radius;
	float	delta = b*b - c;
	if ( delta < 0.0 )
		return INFINITY;

	return -b - sqrt( delta );
}

float	RayTracePlane( float3 _wsPos, float3 _wsDir ) {
	float	t = -_wsPos.y / _wsDir.y;
	return t > 0.0 ? t : INFINITY;
}

float2	RayTraceScene( float3 _wsPos, float3 _wsDir, out float3 _wsNormal, out float3 _wsClosestPosition ) {
	_wsNormal = float3( 0, 1, 0 );

	float	t = RayTraceSphere( _wsPos, _wsDir, SPHERE_CENTER, SPHERE_RADIUS, _wsClosestPosition );
	if ( t > 0.0 && t < 1e4 ) {
		_wsNormal = normalize( _wsPos + t.x * _wsDir - SPHERE_CENTER );
		return float2( t, 0 );		// Sphere hit
	}

	#ifdef FULL_SCENE
		t = RayTracePlane( _wsPos, _wsDir );
		if ( t < 1e4 )
			return float2( t, 1 );		// Plane hit
	#endif

	return float2( INFINITY, -1 );	// No hit...
}

float	ComputeShadow( float3 _wsPos, float3 _wsLight ) {
	float3	wsClosestPosition;
	float	t = RayTraceSphere( _wsPos, _wsLight, SPHERE_CENTER, SPHERE_RADIUS, wsClosestPosition );
	if ( t < 1e4 )
		return 0;

	float	r = length( wsClosestPosition - SPHERE_CENTER ) / SPHERE_RADIUS;
	return smoothstep( 1.0, 2, r );
}

float	ComputeSphereAO( float3 _position, float3 _normal ) {
	return ComputeSphereAO( _position, _normal, SPHERE_CENTER, SPHERE_RADIUS );
}
