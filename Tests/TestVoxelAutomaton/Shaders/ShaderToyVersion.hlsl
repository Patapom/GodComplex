/////////////////////////////////////////////////////////////////////////////////////////////////////
// A (bad) ripoff of the amazing "Absolute Territory" 4K intro by Nnnnoby.
// https://www.youtube.com/watch?v=q7BMD2ZPt00
//
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// TODO: https://www.shadertoy.com/view/4sf3zX

#define saturate( a ) clamp( (a), 0.0, 1.0 )
#define lerp( a, b, t ) mix( a, b, t )

const float	TEX_SIZE = 128.0;

const float INFINITY = 1e6;
const float PI = 3.1415926535897932384626433832795;
const float INVPI = 0.31830988618379067153776752674503;

const float SPHERE_RADIUS = 0.2;

const uint	SAMPLES_COUNT = 64U;
const uint	AA_SAMPLES = 8U;


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Importance Sampling + RNG

// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
float ReverseBits( uint bits ) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float rand( float n ) { return fract(sin(n) * 43758.5453123); }

float rand( vec2 _seed ) {
	return fract( sin( dot( _seed, vec2( 12.9898, 78.233 ) ) ) * 43758.5453 );
//= frac( sin(_pixelPosition.x*i)*sin(1767.0654+_pixelPosition.y*i)*43758.5453123 );
}

// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
void BuildOrthonormalBasis( vec3 _normal, out vec3 _tangent, out vec3 _bitangent ) {
	float a = _normal.z > -0.9999999 ? 1.0 / (1.0 + _normal.z) : 0.0;
	float b = -_normal.x * _normal.y * a;

	_tangent = vec3( 1.0 - _normal.x*_normal.x*a, b, -_normal.x );
	_bitangent = vec3( b, 1.0 - _normal.y*_normal.y*a, -_normal.y );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// FRESNEL

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float	Fresnel_IORFromF0( float _F0 ) {
	float	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}
vec3	Fresnel_IORFromF0( vec3 _F0 ) {
	vec3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}

// Assuming n1=1 (air) we get:
//	IOR = (1 + sqrt(F0)) / (1 - sqrt(F0))
//	=> F0 = ((n2 - 1) / (n2 + 1))²
//
float	Fresnel_F0FromIOR( float _IOR ) {
	float	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}
vec3	Fresnel_F0FromIOR( vec3 _IOR ) {
	vec3	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}

// Schlick's approximation to Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float	FresnelSchlick( float _F0, float _CosTheta ) {
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, t4 * t );
}

vec3	FresnelSchlick( vec3 _F0, float _CosTheta ) {
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, vec3( 1.0 ), vec3( t4 * t ) );
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float	FresnelAccurate( float _IOR, float _CosTheta ) {
	float	c = _CosTheta;
	float	g_squared = max( 0.0, _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	float	g = sqrt( g_squared );

	float	a = (g - c) / (g + c);
			a *= a;
	float	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Ray-tracing

vec3	ComputeSphereCenter() {
//	return SPHERE_CENTER;
	return vec3( 0.6 * sin( 0.5 * iGlobalTime ), -0.8, 0.8 );
}

float	IntersectBox( vec3 _wsPos, vec3 _wsView ) {
	vec3	dir = vec3( _wsView.x < 0.0 ? -1.0 : 1.0,
                        _wsView.y < 0.0 ? -1.0 : 1.0,
                        _wsView.z < 0.0 ? -1.0 : 1.0 );
	vec3	wallDistance = dir - _wsPos;
	vec3	t3 = wallDistance / _wsView;
	return min( min( t3.x, t3.y ), t3.z );
}

float	IntersectSphere( vec3 _wsPos, vec3 _wsView, vec3 _wsCenter, float _radius ) {
	vec3	D = _wsPos - _wsCenter;
	float	c = dot( D, D ) - _radius*_radius;
	float	b = dot( D, _wsView );
	float	delta = b*b - c;
	return delta >= 0.0 && b < 0.0 ? -b - sqrt( delta ) : INFINITY;
}

float	IntersectBoxNormal( vec3 _wsPos, vec3 _wsView, out vec3 _wsNormal ) {
	vec3	dir = vec3( _wsView.x < 0.0 ? -1.0 : 1.0,
                        _wsView.y < 0.0 ? -1.0 : 1.0,
                        _wsView.z < 0.0 ? -1.0 : 1.0 );
	vec3	wallDistance = dir - _wsPos;
	vec3	t3 = wallDistance / _wsView;
	float	t = t3.x;
	_wsNormal = vec3( -dir.x, 0, 0 );
	if ( t3.y < t ) {
		t = t3.y;
		_wsNormal = vec3( 0, -dir.y, 0 );
	}
	if ( t3.z < t ) {
		t = t3.z;
		_wsNormal = vec3( 0, 0, -dir.z );
	}
	return t;
}

float	IntersectSphereNormal( vec3 _wsPos, vec3 _wsView, vec3 _wsCenter, float _radius, out vec3 _wsNormal ) {
	vec3	D = _wsPos - _wsCenter;
	float	c = dot( D, D ) - _radius*_radius;
	float	b = dot( D, _wsView );
	float	delta = b*b - c;
	float	t = -b - sqrt( delta );
	_wsNormal = normalize( _wsPos + t * _wsView - _wsCenter );
	return delta >= 0.0 && b < 0.0 ? t : INFINITY;
}

// Computes scene intersection distance only
vec2	Map( vec3 _wsPos, vec3 _wsView, vec3 _wsSpherePosition ) {
	vec2	d = vec2( IntersectBox( _wsPos, _wsView ), 0.0 );
	vec2	ds = vec2( IntersectSphere( _wsPos, _wsView, _wsSpherePosition, SPHERE_RADIUS ), 1.0 );
	return d.x < ds.x ? d : ds;
}

// Computes scene intersection distance + normal
vec2	MapNormal( vec3 _wsPos, vec3 _wsView, out vec3 _wsNormal, vec3 _wsSpherePosition ) {
	vec2	d = vec2( IntersectBoxNormal( _wsPos, _wsView, _wsNormal ), 0.0 );

	vec3	wsNormal2;
	vec2	ds = vec2( IntersectSphereNormal( _wsPos, _wsView, _wsSpherePosition, SPHERE_RADIUS, wsNormal2 ), 1.0 );
	if ( ds.x < d.x ) {
		_wsNormal = wsNormal2;
	}
	return d.x < ds.x ? d : ds;
}

// Maps a material to a color
vec3	MapColor( vec3 _wsPosition, vec3 _wsNormal, vec3 _wsSpherePosition, float _materialID ) {
    vec2	matUV;
	vec3	color;
	if ( _materialID < 0.5 ) {
		// Map wall color depending on position
		matUV = 0.5 * (1.0 + _wsPosition.xy);
// TODO: Tweak color depending on normal (invert blue and green apparently)
    } else {
        // Map sphere depending on height only, performing an atan would be too expensive...
        matUV = vec2( 0.5, 0.5 * (1.0 + (_wsPosition.y - _wsSpherePosition.y) / SPHERE_RADIUS) );
        matUV = saturate( matUV );
    }
    
    matUV = clamp( vec2( 0.0 ), vec2( (TEX_SIZE-1.0) / TEX_SIZE ), matUV );
//    matUV.x = clamp( 0.0, (TEX_SIZE-1.0) / TEX_SIZE, matUV.x );
    matUV.x += _materialID;
    
    vec2	UV = vec2( matUV * TEX_SIZE / iChannelResolution[0].xy );
    return texture( iChannel0, UV, 0.0 ).xyz;
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// 
void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    vec3	color = vec3( 0.0 );
    float	rcpAA = 1.0 / float(AA_SAMPLES);
    for ( uint AA=0U; AA < AA_SAMPLES; AA++ ) {
        float a = float(AA) * 2.0 * PI * rcpAA;
//    float a = 0.0;
//    vec2	pixel = fragCoord.xy + 0.5*vec2(cos(a),sin(a));
    vec2	pixel = fragCoord.xy + vec2( (float(AA)+1.0) * rcpAA, ReverseBits( AA ) );
	vec2	UV = pixel / iResolution.xy;

    float	noise = rand( pixel );

    // Sample sequencing vectors
    vec2	rcpChannel0PixelSize = vec2( 1.0 ) / iChannelResolution[0].xy;
    vec2	sequenceUV = vec2( 0.5 * rcpChannel0PixelSize.x, (iChannelResolution[0].y - 0.5) * rcpChannel0PixelSize.y );
    vec3	wsCameraPos = texture( iChannel0, sequenceUV, 0.0 ).xyz;	sequenceUV.x += rcpChannel0PixelSize.x;
    vec3	wsCameraTarget = texture( iChannel0, sequenceUV, 0.0 ).xyz;	sequenceUV.x += rcpChannel0PixelSize.x;
    vec3	wsCameraUpRef = texture( iChannel0, sequenceUV, 0.0 ).xyz;	sequenceUV.x += rcpChannel0PixelSize.x;
    vec3	wsSpherePosition = texture( iChannel0, sequenceUV, 0.0 ).xyz; // sequenceUV.x += rcpChannel0PixelSize.x;

    
    // Build camera ray
    vec3	wsPos = wsCameraPos;
    vec3	wsTarget = wsCameraTarget;
    vec3	wsUpRef = wsCameraUpRef;

    vec3	wsAt = normalize( wsTarget - wsPos );
    vec3	wsRight = normalize( cross( wsAt, wsUpRef ) );
    vec3	wsUp = cross( wsRight, wsAt );
    
    const float	tanHalfFOV = 0.57735026918962576450914878050196;	// tan( 60° / 2 )

    vec3	csView = vec3( tanHalfFOV * (2.0 * UV.xy - 1.0), 1.0 );
    		csView.x *= float(iResolution.x) / iResolution.y;
	float	viewLength = length( csView );
			csView /= viewLength;
    vec3	wsView = csView.x * wsRight + csView.y * wsUp + csView.z * wsAt;

    // Compute scene hit
    vec3	wsNormal;
    vec2	distanceMatID = MapNormal( wsPos, wsView, wsNormal, wsSpherePosition );
    float	distance = distanceMatID.x;
    float	matID = distanceMatID.y;

	wsPos += distance * wsView;	// March to hit position

	// Build tangent space
	vec3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

	///////////////////////////////////////////////////////////////////
	// Compute emissive color
	vec3	emissive = MapColor( wsPos, wsNormal, wsSpherePosition, matID );

	///////////////////////////////////////////////////////////////////
	// Importance sample specular distribution
	float	alpha = lerp( 0.05, 0.025, matID );
	float	sqrAlpha = alpha * alpha;

    float	F0 = lerp( 0.04,			// Walls = Dielectric
                       0.9,				// Sphere = Metal
                       matID );
 
//	float	Gv = GSmith( wsNormal, wsView, sqrAlpha );

	vec3	specular = vec3( 0.0 );
	for ( uint i=0U; i < SAMPLES_COUNT; i++ ) {        
		// Generate random half vector
		float	X0 = float(i) / float(SAMPLES_COUNT);
		float	X1 = ReverseBits( i );
		float	phi = 2.0 * PI * (X0 + noise);
		vec2	sinCosPhi = vec2( sin( phi ), cos( phi ) );

		float	sqrCosTheta = (1.0 - X1) / ((alpha*alpha - 1.0) * X1 + 1.0);
		float	cosTheta = sqrt( sqrCosTheta );
		float	sinTheta = sqrt( 1.0 - sqrCosTheta );

		vec3	lsHalf = vec3( sinTheta * sinCosPhi.y, sinTheta * sinCosPhi.x, cosTheta );

		// Generate world-space light ray
		vec3	wsHalf = lsHalf.x * wsTangent + lsHalf.y * wsBiTangent + lsHalf.z * wsNormal;
		vec3	wsLight = wsView - 2.0 * dot( wsHalf, wsView ) * wsHalf;

		// Intersect scene in light direction
		vec2	d = Map( wsPos, wsLight, wsSpherePosition );
		vec3	wsSceneHitPos = wsPos + d.x * wsLight;
		vec3	wsSceneNormal = vec3( 0.0 );	// !!!!!TODO!!!!!
		vec3	sceneColor = 10.0 * MapColor( wsSceneHitPos, wsSceneNormal, wsSpherePosition, d.y );

		// Compute Fresnel
		float	F = FresnelSchlick( F0, cosTheta );

		// Compute shadowing/masking
//		float	Gl = GSmith( wsNormal, wsLight, sqrAlpha );
		float	Gl = 1.0;

		specular += sceneColor * F * Gl;
	}
//	specular *= Gv / SAMPLES_COUNT;
	specular *= 1.0 / float(SAMPLES_COUNT);
	color += emissive + specular;
	}
	color *= rcpAA;

	fragColor = vec4( color, 1.0 );
    fragColor.xyz = pow( fragColor.xyz, vec3( 1.0 / 2.2 ) );
//fragColor = vec4( noise );
//fragColor = vec4( 0.2 * distance );
//fragColor = vec4( matID );
//fragColor = texture( iChannel0, UV, 0.0 );
}
