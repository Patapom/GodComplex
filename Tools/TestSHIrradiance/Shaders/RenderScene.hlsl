#include "Global.hlsl"

Texture2D<float4>	_TexHDRBackBuffer_PreviousFrame : register( t1 );

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
//	float	k2 = 1.0 - h2*nl*nl;

	// above/below horizon: Quilez - http://iquilezles.org/www/articles/sphereao/sphereao.htm
	float res = max( 0.0, nl ) / h2;
	// intersecting horizon: Lagarde/de Rousiers - http://www.frostbite.com/wp-content/uploads/2014/11/course_notes_moving_frostbite_to_pbr.pdf
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
float4	ComputePlaneAO( float3 _wsPosition ) {
	float3	delta = SPHERE_CENTER - _wsPosition;
	float	dist2Sphere = length( delta );
			delta /= dist2Sphere;

	float	cosTheta = sqrt( 1.0 - SPHERE_RADIUS*SPHERE_RADIUS / (dist2Sphere*dist2Sphere) );
	float	cosThetaAO = cos( 0.5 * PI - acos( cosTheta ) );

	float3	ortho = cross( delta, PLANE_NORMAL );
	float3	direction = cross( ortho, delta );

if ( _flags & 0x80U ) {
	cosThetaAO = cos( 0.5 * PI * ComputeAOiQ( _wsPosition, PLANE_NORMAL, SPHERE_CENTER, SPHERE_RADIUS ) );
}

	return float4( direction, cosThetaAO );
}

float4	ComputeSphereAO( float3 _wsPosition ) {
	float3	wsNormal = normalize( _wsPosition - SPHERE_CENTER );
	float3	wsFlatDir = normalize( float3( wsNormal.xy, 0 ) );

	float3	ortho = normalize( cross( PLANE_NORMAL, wsNormal ) );
	float3	ortho2 = cross( wsNormal, ortho );

	float3	direction = normalize( 0.5 * (ortho2 + wsFlatDir ) );
	float	cosThetaAO = dot( direction, wsFlatDir );
	return float4( direction, cosThetaAO );
}

float2	hash2( float n ) { return frac(sin(float2(n,n+1.0))*float2(43758.5453123,22578.1459123)); }

float3	GroundTruth( float3 _wsHitPosition, float3 _wsNormal, float2 _dist, float2 _SVPosition ) {
	_wsHitPosition += 1e-3 * _wsNormal;	// Offset to avoid false hits

	float3	color = 0.0;
//	if ( dist.y < 0.5 ) {
//		// Hit sphere, bounce accounting for plane only
//		[loop]
//		for ( uint i=0; i < 256; i++ ) {
//			float2	aa = hash2( rrr.x + float(i)*203.1 );
//			float	ra = sqrt(aa.y);
//			float	rx = ra*cos(6.2831*aa.x); 
//			float	ry = ra*sin(6.2831*aa.x);
//			float	rz = sqrt( 1.0-aa.y );
//			vec3	dir = vec3( rx*ru + ry*rv + rz*nor );
//			float	res = sphIntersect( pos, dir, sph );
//			occ += step(0.0,res);
//
//		}
//	} else {
//		// Hit plane, bounce accounteing for sphere only
//
//	}

	// Build tangent frame
	float3	wsTangent, wsBiTangent;
	BuildOrthogonalVectors( _wsNormal, wsTangent, wsBiTangent );

	// Shoot a lot of rays
//    vec4 rrr = texture2D( iChannel0, (fragCoord.xy)/iChannelResolution[0].xy, -99.0  ).xzyw;	// Noise
//	float4	rrr = hash2( frac( 0.321519 * _SVPosition.x * _SVPosition.y ) + _time ).xyxy;
	float4	rrr = hash2( _SVPosition.x * _SVPosition.y + _time ).xyxy;

	const uint	SAMPLES_COUNT = 512;
	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float2	aa = hash2( rrr.x + 203.1 * i );

		float	phi = 2.0 * PI * aa.x;

#if 1
		// Naïve sampling
//		float	cosTheta = sqrt( 1.0 - aa.y );
//		float	sinTheta = sqrt( aa.y );
		float	cosTheta = cos( 0.5 * PI * aa.y );
		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

		float	rx = sinTheta * cos( phi ); 
		float	ry = sinTheta * sin( phi );
		float	rz = cosTheta;
		float3	wsSampleDirection = rx * wsTangent + ry * wsBiTangent + rz * _wsNormal;

		float	hitDistance = IntersectScene( _wsHitPosition, wsSampleDirection ).x;
		float3	radiance = hitDistance < NO_HIT ? 0.0
												: 1;//SampleHDREnvironment( wsSampleDirection );
//		color += cosTheta * sinTheta * PI * radiance;
		color += cosTheta * sinTheta * PI * radiance / _luminanceFactor;
#else
		// Importance-sampling
//		float	theta = 2.0 * acos( sqrt( 1.0 - 0.5 * aa.y ) );		// Uniform sampling on theta
		float	cosTheta = aa.y;
		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

		float	rx = sinTheta * cos( phi ); 
		float	ry = sinTheta * sin( phi );
		float	rz = cosTheta;
		float3	wsSampleDirection = rx * wsTangent + ry * wsBiTangent + rz * _wsNormal;

		float	hitDistance = IntersectScene( _wsHitPosition, wsSampleDirection ).x;
		float3	radiance = hitDistance < NO_HIT ? 0.0
												: 1;//SampleHDREnvironment( wsSampleDirection );
//		color += radiance;
		color += radiance / _luminanceFactor;
#endif
	}
	color /= SAMPLES_COUNT;

	return color;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _size;
	float	aspectRatio = float(_size.x) / _size.y;
	float3	csView = float3( aspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;
			wsView = float3( wsView.x, -wsView.z, wsView.y );	// Make view Z-up
	float3	wsPos = float3( _camera2World[3].x, -_camera2World[3].z, _camera2World[3].y );	// Make camera pos Z-up


//float	v = EstimateLambertReflectanceFactors( UV.x, UV.y ).z;
//return v < 0.0 || v > 1.0 ? float3( v-1, 0, 0 ) : v;

	float3	filteredEnvironmentSH[9];
	FilterHanning( EnvironmentSH, filteredEnvironmentSH, _filterWindowSize );

	float2	dist = IntersectScene( wsPos, wsView );
	if ( dist.x > NO_HIT ) {
		return (_flags & 0x100U) ? EvaluateSHRadiance( wsView, filteredEnvironmentSH )
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

	if ( _flags & 0x1000U ) {
		float3	previousFrame = _TexHDRBackBuffer_PreviousFrame[_In.__Position.xy].xyz;
		float3	currentFrame = GroundTruth( wsHitPos, wsNormal, dist, _In.__Position.xy );
		return lerp( previousFrame, currentFrame, 0.4 );
	}

	AO.xyz = normalize( lerp( wsNormal, AO.xyz, _influenceBentNormal ) );
	AO.w = lerp( 0.0, AO.w, _influenceAO );

	float3	color = 0.0;
//	color = 0.01 * dist.x;

	if ( (_flags & 0x20U) ) {
		// Bent normal debug
		color = wsNormal;
		color = AO.xyz;
		color = saturate( dot( AO.xyz, wsNormal ) );
		color /= _luminanceFactor;
	} else if ( (_flags & 0x10U) ) {
		// AO debug
//		color = AO.w;
		color = 2.0 * INVPI * acos( AO.w );
		color /= _luminanceFactor;
	} else {
		// Regular scene display
		float	bentConeAngle = acos( saturate( dot( AO.xyz, wsNormal ) ) );

		float3	correctIrradiance = EvaluateSHIrradiance( wsNormal, AO.w, bentConeAngle, filteredEnvironmentSH );
		float3	incorrectIrradiance = (_flags & 0x40U)  ?	2.0 * INVPI * acos(AO.w) * EvaluateSHIrradiance( wsNormal, filteredEnvironmentSH )
														:	EvaluateSHIrradiance( wsNormal, AO.w, filteredEnvironmentSH );
//														:	EvaluateSHIrradiance( wsNormal, AO.w, 0.0, filteredEnvironmentSH );

		color = (_flags & 0x8U) ? correctIrradiance : incorrectIrradiance;
	}

	return color;
}