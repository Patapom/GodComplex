#include "Global.hlsl"

Texture2D<float4>		_TexHDRBackBuffer_PreviousFrame : register( t1 );
Texture2D<float4>		_TexNoise : register( t2 );
Texture2DArray<float4>	_TexACoeffs : register( t3 );

static const float3	SPHERE_CENTER = float3( 0, 0, 1 );
static const float	SPHERE_RADIUS = 1.0;
static const float3	PLANE_NORMAL = float3( 0, 0, 1 );

float2	IntersectScene( float3 _pos, float3 _dir ) {
	float2	distSphere = float2( IntersectSphere( _pos, _dir, SPHERE_CENTER, SPHERE_RADIUS ), 0 );
	float2	distPlane = float2( IntersectPlane( _pos, _dir, 0.0, PLANE_NORMAL ), 1 );
	float2	distMin = float2( INFINITY, -1 );
	if ( distSphere.x > 0.0 && distSphere.x < distMin.x )
		distMin = distSphere;
	if ( distPlane.x > 0.0 && distPlane.x < distMin.x )
		distMin = distPlane;
	return distMin;
}

// iQ's analytical AO computation for a sphere
// Source: http://www.iquilezles.org/www/articles/sphereao/sphereao.htm
float	ComputeAOiQ( float3 _wsPosition, float3 _wsNormal, float3 _wsSphereCenter, float _sphereRadius ) {
	float3	dir = _wsSphereCenter - _wsPosition;
	float	l  = length(dir);
	float	nl = dot( _wsNormal, dir / l );
	float	h  = l / _sphereRadius;
	float	h2 = h*h;

	// above/below horizon: Quilez - http://iquilezles.org/www/articles/sphereao/sphereao.htm
	float res = max( 0.0, nl ) / h2;
	// intersecting horizon: Lagarde/de Rousiers - http://www.frostbite.com/wp-content/uploads/2014/11/course_notes_moving_frostbite_to_pbr.pdf
//	float	k2 = 1.0 - h2*nl*nl;
//	if( k2 > 0.0 )  {
//		#if 1
//			res = nl*acos(-nl*sqrt( (h2-1.0)/(1.0-nl*nl) )) - sqrt(k2*(h2-1.0));
//			res = res/h2 + atan( sqrt(k2/(h2-1.0)));
//			res /= 3.141593;
//		#else
//			// cheap approximation: Quilez
//			res = pow( clamp(0.5*(nl*h+1.0)/h2,0.0,1.0), 1.5 );
//		#endif
//	}
	return 1.0 - res;
}

// Hardcoded AO computations
#if 0
	// Compute cone's aperture as angle of non-occlusion
float4	ComputePlaneAO( float3 _wsPosition ) {
	float3	delta = SPHERE_CENTER - _wsPosition;
	float	dist2Sphere = length( delta );
			delta /= dist2Sphere;

	float	cosTheta = sqrt( 1.0 - SPHERE_RADIUS*SPHERE_RADIUS / (dist2Sphere*dist2Sphere) );
//	float	cosThetaAO = cos( 0.5 * PI - acos( cosTheta ) );
	float	AO = cosTheta;

	float3	ortho = cross( delta, PLANE_NORMAL );
	float3	direction = cross( ortho, delta );

if ( _flags & 0x80U ) {
	AO = ComputeAOiQ( _wsPosition, PLANE_NORMAL, SPHERE_CENTER, SPHERE_RADIUS );
}

	return float4( direction, AO );
}

float4	ComputeSphereAO( float3 _wsPosition ) {
	float3	wsNormal = normalize( _wsPosition - SPHERE_CENTER );
	float3	wsFlatDir = normalize( float3( wsNormal.xy, 0 ) );

	float3	ortho = normalize( cross( PLANE_NORMAL, wsNormal ) );
	float3	ortho2 = cross( wsNormal, ortho );

	float3	direction = normalize( 0.5 * (ortho2 + wsFlatDir ) );
	float	AO = acos( dot( direction, wsFlatDir ) );
	return float4( direction, AO );
}
#else
	// Compute cone angle as a ratio of open hemisphere
float4	ComputePlaneAO( float3 _wsPosition ) {
	float3	delta = SPHERE_CENTER - _wsPosition;
	float	dist2Sphere = length( delta );
			delta /= dist2Sphere;

	float	cosTheta = sqrt( 1.0 - SPHERE_RADIUS*SPHERE_RADIUS / (dist2Sphere*dist2Sphere) );
	float	AO = cosTheta;

	float3	ortho = cross( delta, PLANE_NORMAL );
	float3	direction = cross( ortho, delta );

if ( _flags & 0x80U ) {
	AO = ComputeAOiQ( _wsPosition, PLANE_NORMAL, SPHERE_CENTER, SPHERE_RADIUS );
}

	return float4( direction, AO );
}

float4	ComputeSphereAO( float3 _wsPosition ) {
	float3	wsNormal = normalize( _wsPosition - SPHERE_CENTER );
	float3	wsFlatDir = normalize( float3( wsNormal.xy, 0 ) );

	float3	ortho = normalize( cross( PLANE_NORMAL, wsNormal ) );
	float3	ortho2 = cross( wsNormal, ortho );

	float3	direction = normalize( 0.5 * (ortho2 + wsFlatDir ) );
	float	AO = acos( dot( direction, wsFlatDir ) );
	return float4( direction, AO );
}
#endif

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Computes the "ground truth" ambient lighting that samples 512 random directions each frame and accumulates the results
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
float2	hash2( float n ) { return frac(sin(float2(n,n+1.0))*float2(43758.5453123,22578.1459123)); }

float3	GroundTruth( float3 _wsHitPosition, float3 _wsNormal, float2 _dist, float2 _SVPosition ) {
	_wsHitPosition += 1e-3 * _wsNormal;	// Offset to avoid false hits

	// Build tangent frame
	float3	wsTangent, wsBiTangent;
	BuildOrthogonalVectors( _wsNormal, wsTangent, wsBiTangent );

	// Shoot a lot of rays
//	vec4	rrr = texture2D( iChannel0, (fragCoord.xy)/iChannelResolution[0].xy, -99.0  ).xzyw;	// Noise
//	float4	rrr = hash2( frac( 0.321519 * _SVPosition.x * _SVPosition.y ) + _time ).xyxy;
//	float4	rrr = hash2( _SVPosition.x * _SVPosition.y + _time ).xyxy;
	float2	rrr = frac( _TexNoise.SampleLevel( LinearWrap, _SVPosition.xy / 256 + hash2( _time ), 0.0 ).xy );

	float3	color = 0.0;
	const uint	SAMPLES_COUNT = 512;
	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float2	aa = hash2( rrr.x + 203.1 * i );
//		float2	aa = _TexNoise.SampleLevel( LinearWrap, 0.032191948 * (_SVPosition.xy + _time + i), 0.0 ).xy;
//		float2	aa = _TexNoise.SampleLevel( LinearWrap, float2( 0.5 + i, 0.5 + floor( i / 256.0 ) ) / 256, 0.0 ).xy;

		float	phi = 2.0 * PI * aa.x;

#if 0
		// Naïve sampling
//		float	cosTheta = sqrt( 1.0 - aa.y );
//		float	sinTheta = sqrt( aa.y );
//		float	cosTheta = cos( 0.5 * PI * aa.y );
//		float	cosTheta = aa.y;
//		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
		float	cosTheta = sqrt( aa.y );
		float	sinTheta = sqrt( 1.0 - aa.y );

		float	rx = sinTheta * cos( phi ); 
		float	ry = sinTheta * sin( phi );
		float	rz = cosTheta;
		float3	wsSampleDirection = rx * wsTangent + ry * wsBiTangent + rz * _wsNormal;

		float	hitDistance = IntersectScene( _wsHitPosition, wsSampleDirection ).x;
		float3	radiance = hitDistance < NO_HIT ? 0.0
												: 1;//SampleHDREnvironment( wsSampleDirection );

//		color += cosTheta * sinTheta;
//		color += cosTheta * sinTheta * radiance;
		color += radiance;
//		color += cosTheta * radiance;

#else
		// Importance-sampling
		float	cosTheta = sqrt( aa.y );		// Accounts for dot(N,L)
		float	sinTheta = sqrt( 1.0 - aa.y );

		float	rx = sinTheta * cos( phi ); 
		float	ry = sinTheta * sin( phi );
		float	rz = cosTheta;
		float3	wsSampleDirection = rx * wsTangent + ry * wsBiTangent + rz * _wsNormal;

		float	hitDistance = IntersectScene( _wsHitPosition, wsSampleDirection ).x;
		float3	radiance = hitDistance < NO_HIT ? 0.0
												: SampleHDREnvironment( wsSampleDirection );
		color += radiance;
//		color += radiance / (_luminanceFactor * 2.0 * PI);	// To normalize AO
#endif
	}
	color /= SAMPLES_COUNT;

//color *= PI*PI;		// if cosTheta = cos( PI/2 * rnd )
//color *= 2.0 * PI;	// if cosTheta = rnd
color *= PI;			// if cosTheta = sqrt( rnd )

	return color;
}

float3	GroundTruth_AO( float3 _wsHitPosition, float3 _wsNormal, float2 _dist, float2 _SVPosition ) {
	_wsHitPosition += 1e-3 * _wsNormal;	// Offset to avoid false hits

	// Build tangent frame
	float3	wsTangent, wsBiTangent;
	BuildOrthogonalVectors( _wsNormal, wsTangent, wsBiTangent );

	// Shoot a lot of rays
	float2	rrr = frac( _TexNoise.SampleLevel( LinearWrap, _SVPosition.xy / 256 + hash2( _time ), 0.0 ).xy );

	float	AO = 0.0;
	const uint	SAMPLES_COUNT = 512;
	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float2	aa = hash2( rrr.x + 203.1 * i );
		float	phi = 2.0 * PI * aa.x;

		// Importance-sampling
		float	cosTheta = aa.y;
		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

		float	rx = sinTheta * cos( phi ); 
		float	ry = sinTheta * sin( phi );
		float	rz = cosTheta;
		float3	wsSampleDirection = rx * wsTangent + ry * wsBiTangent + rz * _wsNormal;

		float	hitDistance = IntersectScene( _wsHitPosition, wsSampleDirection ).x;
		AO += hitDistance < NO_HIT ? 0.0 : 1.0;
	}
	AO /= (SAMPLES_COUNT * _luminanceFactor);

	return AO;
}


////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Computes the "improved irradiance estimate" using up to 20 orders of SH
// To be used against ground truth
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
float3	EvaluateSHIrradiance( float3 _wsDirection, float _AO, float _cosBentConeAngle, int _SHOrdersCount, float _filterWindowSize ) {

//return _AO / _luminanceFactor;
//_cosBentConeAngle = 1.0;

	// Build A terms
	float	A[20];
	for ( uint sliceIndex=0; sliceIndex < 5; sliceIndex++ ) {
		float2	UV = float2( _AO, _cosBentConeAngle );
		float4	V = _TexACoeffs.SampleLevel( LinearClamp, float3( UV, sliceIndex ), 0.0 );

//V = 1.0;

		A[4*sliceIndex+0] = V.x;
		A[4*sliceIndex+1] = V.y;
		A[4*sliceIndex+2] = V.z;
		A[4*sliceIndex+3] = V.w;
	}

//_wsDirection = normalize( float3( 0.01, 0, 0 ) + _wsDirection );

////float	v = General_Ylm( _SHOrdersCount, int( _SHOrdersCount * sin( 3.0 * _time ) ), _wsDirection.z, atan2( _wsDirection.y, _wsDirection.x ) );
//float	v = General_Ylm( _SHOrdersCount, _customM, _wsDirection.z, atan2( _wsDirection.y, _wsDirection.x ) );
//return float3( v, -v, 0 );
//
//float	c[9];
//Ylm( _wsDirection, c );
////v = c[_SHOrdersCount*(_SHOrdersCount+1) + int( _SHOrdersCount * sin( _time ) )];
//float	v2 = c[_SHOrdersCount*(_SHOrdersCount+1) + _customM];
//return float3( v - v2, v2 - v, 0 );
//return any( isnan( General_P( 0, 0, _wsDirection.z ) ) ) ? float3( 1, 0, 1 ) : 0.0;

#if 0	// Debug order-2 analytical SH
float	c[9];
Ylm( _wsDirection, c );

float	cosTheta = _wsDirection.z;
float	phi = abs(_wsDirection.x) > 1e-6 ? atan2( _wsDirection.y, _wsDirection.x ) : 0.0;

float3	anus = 0.0;
for ( int i=0; i < 9; i++ ) {
	int	l = int( floor( sqrt( i ) ) );

	float	filter = 0.5 * (1.0 + cos( l * PI / _filterWindowSize ));

int	m = i - l*(l+1);
//c[i] = General_Ylm( l, m, cosTheta, phi );
//c[i] = General_P( l, abs(m), cosTheta );
//float	factor = General_K( l, m );
//factor *= (m & 1 ? -1 : 1) * SQRT2;	// (2015-04-13) According to wikipedia (http://en.wikipedia.org/wiki/Spherical_harmonics#Real_form) we should multiply coeffs by (-1)^m ...
//c[i] *= factor;
//c[i] *= cos( (m < 0 ? -0.5 * PI : 0.0) + m * phi );
////c[i] *= cos( m * phi );

if ( isnan( phi ) )
	return float3( 10, 10, 0 );

if ( isnan(c[i]) )
	return float3( 10, 0, 10 );
if ( isinf(c[i]) )
	return float3( 0, 10, 0 );

	anus += filter * c[i] * _environmentSH[i].xyz;
}

return anus;
#endif

	// Estimate radiance in reduced AO cone
	return EvaluateSHRadiance( _wsDirection, _SHOrdersCount, _filterWindowSize, A );
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _size;
	float	aspectRatio = float(_size.x) / _size.y;
	float3	csView = float3( aspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;
			wsView = float3( wsView.x, -wsView.z, wsView.y );	// Make view Z-up
	float3	wsPos = float3( _camera2World[3].x, -_camera2World[3].z, _camera2World[3].y );	// Make camera pos Z-up

// Debug general Ylm estimates
#if 0
float	cosTheta = wsView.z;
float	phi = atan2( wsView.y, wsView.x );
int		l = _SHOrdersCount;
//float	v = General_Ylm( l, l * sin( _time ), cosTheta, phi );
//float	v = General_Ylm( l, 18, cosTheta, phi );
//float	v = General_P( l, 10, cosTheta );
return v == 0.0 ? float3( 1, 0, 1 ) : float3( v, -v, 0 );
#endif

//float	v = EstimateLambertReflectanceFactors( UV.x, UV.y ).z;
//return v < 0.0 || v > 1.0 ? float3( v-1, 0, 0 ) : v;

// Debug A coefficient tables
#if 0

	#if 0	// cos(thetaAO) = U						=> thetaAO = acos( U )
			// (thetaCone) = cos( PI/2 * V )		=> thetaCone = PI/2 * V
	float	angleAO = acos( UV.x );
	float	angleCone = 0.5 * PI * UV.y;
	#else	// cos(thetaAO) = cos( PI/2 * U )		=> thetaAO = PI/2 * U
			// cos(thetaCone) = V					=> thetaCone = acos( V )
	float	angleAO = 0.5 * PI * UV.x;
	float	angleCone = acos( UV.y );
	#endif

//return angleCone * 2.0 * INVPI / _luminanceFactor;

//float	belowHorizon_StartAngle = angleCone + angleAO;
//float	belowHorizon_EndAngle = angleCone - angleAO;
float	horizonRatio = saturate( ((angleCone + angleAO) - 0.5 * PI) / (2.0 * angleCone) )
					/ _luminanceFactor;

//UV.y = angleAO * 2 * INVPI;	// <= AO
//UV.x = cos( 0.5 * PI * UV.x );
uint	testOrder = min( 19U, _SHOrdersCount );
return 2.0 * (1.0 + testOrder) * abs( _TexACoeffs.SampleLevel( LinearClamp, float3( UV, testOrder >> 2 ), 0.0 )[testOrder&0x3U] )
	+ (angleCone + angleAO > 0.5 * PI ? float3( 0.2 + horizonRatio, 0, 0 ) : 0.0);
#endif


	float3	filteredEnvironmentSH[9];
	FilterHanning( EnvironmentSH, filteredEnvironmentSH, _filterWindowSize );

	float2	dist = IntersectScene( wsPos, wsView );
	if ( dist.x > NO_HIT ) {
		// Initialize default A's
		float	A[20];
		for ( uint l=0; l < 20; l++ )
			A[l] = 1.0;

		return (_flags & 0x100U) ? EvaluateSHRadiance( wsView, _SHOrdersCount, _filterWindowSize, A )
//		return (_flags & 0x100U) ? EvaluateSHRadiance( wsView, filteredEnvironmentSH )
								 : SampleHDREnvironment( wsView );
	}

	// Compute scene AO
	float3	wsHitPos = wsPos + dist.x * wsView;
	float3	wsNormal = PLANE_NORMAL;
	float4	AO;
	if ( dist.y < 0.5 ) {
		// Sphere
		AO = ComputeSphereAO( wsHitPos );
		wsNormal = normalize( wsHitPos - SPHERE_CENTER );
	} else if ( dist.y < 1.5 ) {
		// Plane
		AO = ComputePlaneAO( wsHitPos );
	}

	AO.xyz = normalize( lerp( wsNormal, AO.xyz, _influenceBentNormal ) );
	AO.w = lerp( 1.0, AO.w, _influenceAO );

	// Compute ground truth
	if ( _flags & 0x1000U ) {
		float3	previousFrame = _TexHDRBackBuffer_PreviousFrame[_In.__Position.xy].xyz;
		float3	currentFrame = 0.0;

		if ( (_flags & 0x10U) )
			 currentFrame = GroundTruth_AO( wsHitPos, wsNormal, dist, _In.__Position.xy );
		else
			 currentFrame = GroundTruth( wsHitPos, wsNormal, dist, _In.__Position.xy );

		return lerp( previousFrame, currentFrame, 0.1 );
	}

	float3	color = 0.0;
//	color = 0.01 * dist.x;

	if ( (_flags & 0x20U) ) {
		// Bent normal debug
		color = wsNormal;
		color = saturate( dot( AO.xyz, wsNormal ) );
		color = AO.xyz;
		color /= _luminanceFactor;
	} else if ( (_flags & 0x10U) ) {
		// AO debug
		color = AO.w;
		color /= _luminanceFactor;
	} else {
		// Regular scene display
		float	cosBentConeAngle = saturate( dot( AO.xyz, wsNormal ) );
		float	bentConeAngle = acos( cosBentConeAngle );

		// Use hardcoded order 2 estimate
//		float3	correctIrradiance = EvaluateSHIrradiance( wsNormal, cos( 0.5 * PI * AO.w ), bentConeAngle, filteredEnvironmentSH );

		// Use generic order in normal's direction
//		float3	correctIrradiance = EvaluateSHIrradiance( wsNormal, AO.w, cosBentConeAngle, _SHOrdersCount, _filterWindowSize );

		// Use generic order in BENT NORMAL's direction
		float3	correctIrradiance = EvaluateSHIrradiance( AO.xyz, AO.w, cosBentConeAngle, _SHOrdersCount, _filterWindowSize );

		float3	incorrectIrradiance = (_flags & 0x40U)  ?	AO.w * EvaluateSHIrradiance( wsNormal, filteredEnvironmentSH )						// "Traditional method" AO * irradiance
														:	EvaluateSHIrradiance( wsNormal, cos( 0.5 * PI * AO.w ), filteredEnvironmentSH );	// Improved method, without bent normal
//														:	EvaluateSHIrradiance( wsNormal, cos( 0.5 * PI * AO.w ), 0.0, filteredEnvironmentSH );

		color = (_flags & 0x8U) ? correctIrradiance : incorrectIrradiance;
	}

	return color;
}