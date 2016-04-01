
static const float	SPHERE_RADIUS = 0.5;
static const uint	HEIGHTMAP_SIZE = 128;
static const float	HEIGHTMAP_AMPLITUDE = 0.1;

Texture2D< float2 >	_TexHeight : register(t11);

float2	HeightMapUV2PhiTheta( float2 _UV ) {
	float	phi = TWOPI * _UV.x;
//	float	theta = 2.0 * acos( sqrt( _UV.y ) );
	float	theta = acos( 2.0 * _UV.y - 1.0 );
	return float2( phi, theta );
}

float3	HeightMapUV2Direction( float2 _UV ) {
	float2	PhiTheta = HeightMapUV2PhiTheta( _UV );
	float2	scPhi, scTheta;
	sincos( PhiTheta.x, scPhi.x, scPhi.y );
	sincos( PhiTheta.y, scTheta.x, scTheta.y );
	return float3( scTheta.x * scPhi.x, scTheta.y, scTheta.x * scPhi.y );
}

float2	Direction2HeightMapUV( float3 _wsDirection ) {
	float	phi = fmod( TWOPI + atan2( _wsDirection.x, _wsDirection.z ), TWOPI );
	return float2( 0.5 * (1.0 + _wsDirection.y), 0.5*INVPI * phi );
}

float3	HeightMapUV2WorldPosition( float2 _UV ) {
	float	HeightValue = _TexHeight.SampleLevel( LinearClamp, _UV, 0.0 ).x;
	float3	wsDirection = HeightMapUV2Direction( _UV );
	return (SPHERE_RADIUS + HEIGHTMAP_AMPLITUDE * HeightValue) * wsDirection;
}
