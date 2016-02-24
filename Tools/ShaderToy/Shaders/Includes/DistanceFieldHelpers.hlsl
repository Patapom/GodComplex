
// We assume a "map()" function exists later
// The map function must return the distance to the scene in the X component of
//	the float2 and the material index in the Y component
//
float2	map( float3 p );


float	subtract( float a, float b ) { return max( a, -b ); }
float	intersect( float a, float b ) { return max( a, b ); }
float	vmin( float2 v ) { return min( v.x, v.y ); }
float	vmin( float3 v ) { return min( min( v.x, v.y ), v.z ); }
float	vmin( float4 v ) { float2 temp = min( v.xy, v.zw ); return min( temp.x, temp.y ); }
float	vmax( float2 v ) { return max( v.x, v.y ); }
float	vmax( float3 v ) { return max( max( v.x, v.y ), v.z ); }
float	vmax( float4 v ) { float2 temp = max( v.xy, v.zw ); return max( temp.x, temp.y ); }
float	min3( float a, float b, float c ) { return min( min( a, b ), c ); }
float	min4( float a, float b, float c, float d ) { return min( min( min( a, b ), c ), d ); }
float	min5( float a, float b, float c, float d, float e ) { return min( min( min( min( a, b ), c ), d ), e ); }
float	min6( float a, float b, float c, float d, float e, float f ) { return min( min( min( min( min( a, b ), c ), d ), e ), f ); }
float	min7( float a, float b, float c, float d, float e, float f, float g ) { return min( min( min( min( min( min( a, b ), c ), d ), e ), f ), g ); }
float3	scale( float3 p, float3 s ) { return  p * (1.0 / s); }

float	sphere( float3 p, float r ) {
	return length( p ) - r;
}
float sbox( float3 p, float3 b ) {
	float3	d = abs(p) - b;
	return min( max(d.x,max(d.y,d.z)), length( max( d, 0.0 )) );
}
float box( float3 p, float3 b) {
	float3 d = abs(p) - b;
	return length( max( d, 0.0 ) ) + vmax( min( d, 0.0 ) );
}
float box2( float2 p, float2 b) {
	float2 d = abs(p) - b;
	return length( max(d, 0.0 ) ) + vmax( min( d, 0.0 ) );
}
float	plane( float3 p, float3 n, float d ) {
	return dot( p, n ) - d;
}
float	bowl( float3 p, float3 n, float r_out, float r_in, float h ) {
	float	hollowedSphere = subtract( sphere( p, r_out ), sphere( p, r_in ) );
	return subtract( hollowedSphere, plane( p, n, h ) );
}

float	sphereConvex( float3 p, float r, const float dAngle = PI / 10.0 ) {

	float3	dir = normalize( p );
	float	phi = atan2( dir.z, dir.x );
	float	theta = acos( clamp( dir.y, -1.0, 1.0 ) );

	phi = dAngle * (0.5 + floor( phi / dAngle ) );
	theta = dAngle * (0.5 + floor( theta / dAngle ) );

	float2	scPhi;
	sincos( phi, scPhi.x, scPhi.y );
	float2	scTheta;
	sincos( theta, scTheta.x, scTheta.y );

	float3	planeDir = float3( scTheta.x * scPhi.y, scTheta.y, scTheta.x * scPhi.x );
	float	planeDist = r * cos( 0.5 * dAngle );
	float3	planePos = planeDist * planeDir;
	float	planeHitDistance = dot( p - planePos, planeDir ) / dot( planeDir, dir );
	float3	planeHitPos = p - planeHitDistance * dir;

	return length( p - planeHitPos );
	return subtract( sphere( p, r ), length( p - planeHitPos ) );
}

float	sphereConvex2( float3 p, float r, const float dAngle = PI / 10.0 ) {

p.y *= -1.0;

	float	interval = 0.001 + _DebugParm;

	float	dist = length( p );
	float3	dir = 0.5 * p / dist;
	float2	stereoP = float2( dir.xz ) / (1.0 + dir.y);

#if 1
	// Cartersian repeat
	stereoP = interval * (frac( 0.5 + stereoP / interval ) - 0.5);
#endif

#if 0
	// Polar repeat
	interval = 0.125 * PI;
	float	radius = length( stereoP );
	float	angle = atan2( stereoP.y, stereoP.x );
			angle = fmod( PI + 0.5 * interval + angle, interval ) - 0.5*interval;
	sincos( angle, stereoP.x, stereoP.y );
	stereoP *= radius;
#endif

#if 0
	// Croute repeat
	float	radius = length( stereoP );
	float	newRadius = mod( radius, interval );
	stereoP *= newRadius / radius;
#endif

	// Unproject back to 3D cartesian
	float3	newDir = float3( 2.0 * stereoP.x, 1.0 - dot(stereoP,stereoP), 2.0 * stereoP.y ) / (1.0 + dot(stereoP,stereoP) );
	float3	newP = dist * newDir;

//	return length(newP) - r;
	return newP.y - r;
}

// Repeat around the origin by a fixed angle.
// For easier use, num of repetitions is used to specify the angle.
float	pModPolar( inout float2 p, float repetitions ) {
	float	angle = 2.0*PI / repetitions;
	float	halfAngle = 0.5 * angle;
	float	a = halfAngle + atan2( p.y, p.x );
	float	r = length( p );
	float	cell = floor( a / angle );
	a = fmod( 100.0 * angle + a, angle ) - halfAngle;
	p = float2( cos(a), sin(a) ) * r;

	// For an odd number of repetitions, fix cell index of the cell in -x direction
	// (cell index would be e.g. -5 and 5 in the two halves of the cell):
//	if ( abs(cell) >= (repetitions/2) ) cell = abs(cell);

	return cell;
}

float3	pModSpherical( in float3 p, float repetitions ) {
	float	angle = 2.0*PI / repetitions;
	float	halfAngle = 0.5 * angle;

	float	r = length( p );
	float	phi = atan2( p.z, p.x );
			phi = fmod( 100.5 * angle + phi, angle ) - halfAngle;

	float	theta = asin( clamp( p.y / r, -1.0, 1.0 ) );
			angle = PI / repetitions;;
			halfAngle = 0.5 * angle;
			theta = fmod( 100.5 * angle + theta, angle ) - halfAngle;

	float2	scPhi, scTheta;
	sincos( phi, scPhi.x, scPhi.y );
	sincos( theta, scTheta.x, scTheta.y );
	return r * float3( scTheta.y * scPhi.y, scTheta.x, scTheta.y * scPhi.x );
}

float	sphereConvex3( float3 p, float r, const float dAngle = PI / 10.0 ) {
	pModPolar( p.xz, 18.0 );
	pModPolar( p.xy, 18.0 );
//	pModPolar( p.xz, lerp( 36.0, 36.0, abs( p.y ) ) );	// Funky
//	pModPolar( p.xy, lerp( 18.0, 36.0, abs( p.z + p.y ) ) );
	return p.x - r;
//	return dot( float2( p.x, -p.z ), SQRT2 ) - r;
}

float	sphereConvex4( float3 p, float r, const float dAngle = PI / 10.0 ) {
	p = pModSpherical( p, 18.0 );
//	return dot( p, normalize(
//	return length( p ) - r;
	return p.x - r;
//	return dot( float2( p.x, -p.z ), SQRT2 ) - r;
}


/////////////////////////////////////////////////////////////////////////////////
// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
//
// A list of useful distance function to simple primitives, and an example on how to 
// do some interesting boolean operations, repetition and displacement.
//
// More info here: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
//
float sdPlane( float3 p )
{
	return p.y;
}

float sdSphere( float3 p, float s )
{
    return length(p)-s;
}

float sdBox( float3 p, float3 b )
{
  float3 d = abs(p) - b;
  return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}

float sdEllipsoid( in float3 p, in float3 r )
{
    return (length( p/r ) - 1.0) * min(min(r.x,r.y),r.z);
}

float udRoundBox( float3 p, float3 b, float r )
{
  return length(max(abs(p)-b,0.0))-r;
}

float sdTorus( float3 p, float2 t )
{
  return length( float2(length(p.xz)-t.x,p.y) )-t.y;
}

float sdHexPrism( float3 p, float2 h )
{
    float3 q = abs(p);
#if 0
    return max(q.z-h.y,max((q.x*0.866025+q.y*0.5),q.y)-h.x);
#else
    float d1 = q.z-h.y;
    float d2 = max((q.x*0.866025+q.y*0.5),q.y)-h.x;
    return length(max(float2(d1,d2),0.0)) + min(max(d1,d2), 0.);
#endif
}

float sdCapsule( float3 p, float3 a, float3 b, float r )
{
	float3 pa = p-a, ba = b-a;
	float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
	return length( pa - ba*h ) - r;
}

float sdTriPrism( float3 p, float2 h )
{
    float3 q = abs(p);
#if 0
    return max(q.z-h.y,max(q.x*0.866025+p.y*0.5,-p.y)-h.x*0.5);
#else
    float d1 = q.z-h.y;
    float d2 = max(q.x*0.866025+p.y*0.5,-p.y)-h.x*0.5;
    return length(max(float2(d1,d2),0.0)) + min(max(d1,d2), 0.);
#endif
}

float sdCylinder( float3 p, float2 h )
{
  float2 d = abs(float2(length(p.xz),p.y)) - h;
  return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float sdCone( in float3 p, in float3 c )
{
    float2 q = float2( length(p.xz), p.y );
    float d1 = -q.y-c.z;
    float d2 = max( dot(q,c.xy), q.y);
    return length(max(float2(d1,d2),0.0)) + min(max(d1,d2), 0.);
}

float sdConeSection( in float3 p, in float h, in float r1, in float r2 )
{
    float d1 = -p.y - h;
    float q = p.y - h;
    float si = 0.5*(r1-r2)/h;
    float d2 = max( sqrt( dot(p.xz,p.xz)*(1.0-si*si)) + q*si - r2, q );
    return length(max(float2(d1,d2),0.0)) + min(max(d1,d2), 0.);
}


float length2( float2 p )
{
	return sqrt( p.x*p.x + p.y*p.y );
}

float length6( float2 p )
{
	p = p*p*p; p = p*p;
	return pow( p.x + p.y, 1.0/6.0 );
}

float length8( float2 p )
{
	p = p*p; p = p*p; p = p*p;
	return pow( p.x + p.y, 1.0/8.0 );
}

float sdTorus82( float3 p, float2 t )
{
  float2 q = float2(length2(p.xz)-t.x,p.y);
  return length8(q)-t.y;
}

float sdTorus88( float3 p, float2 t )
{
  float2 q = float2(length8(p.xz)-t.x,p.y);
  return length8(q)-t.y;
}

float sdCylinder6( float3 p, float2 h )
{
  return max( length6(p.xz)-h.x, abs(p.y)-h.y );
}

//----------------------------------------------------------------------

float opS( float d1, float d2 ) {
	return max(-d2,d1);
}

float2 opU( float2 d1, float2 d2 ) {
	return (d1.x<d2.x) ? d1 : d2;
}

float3 opRep( float3 p, float3 c ) {
	return fmod(p,c)-0.5*c;
}

float3 opTwist( float3 p ) {
	float		c = cos(10.0*p.y+10.0);
	float		s = sin(10.0*p.y+10.0);
	float2x2	m = float2x2(c,-s,s,c);
	return float3( mul( p.xz, m ),p.y );
}


/////////////////////////////////////////////////////////////////////////////////
// map-dependent Functions
//
float3	calcNormal( float3 p, const float _eps=0.01 ) {
	const float2	e = float2( _eps, 0.0 );
	return normalize( float3(
			map( p + e.xyy ).x - map( p - e.xyy ).x,
			map( p + e.yxy ).x - map( p - e.yxy ).x,
			map( p + e.yyx ).x - map( p - e.yyx ).x
		) );
}

float softshadow( in float3 ro, in float3 rd, in float mint, in float tmax ) {
	float res = 1.0;
    float t = mint;
    for( int i=0; i<16; i++ )
    {
		float h = map( ro + rd*t ).x;
        res = min( res, 8.0*h/t );
        t += clamp( h, 0.02, 0.10 );
        if( h<0.001 || t>tmax ) break;
    }
    return clamp( res, 0.0, 1.0 );

}


float calcAO( in float3 pos, in float3 nor )
{
	float occ = 0.0;
    float sca = 1.0;
    for( int i=0; i<5; i++ )
    {
        float hr = 0.01 + 0.12*float(i)/4.0;
        float3 aopos =  nor * hr + pos;
        float dd = map( aopos ).x;
        occ += -(dd-hr)*sca;
        sca *= 0.95;
    }
    return saturate( 1.0 - 3.0*occ );
}

float	AO( float3 p, float3 n, float _strength, const float _stepSize=0.1, const uint _stepsCount=5 ) {
	float4	dir = _stepSize * float4( n, 1.0 );
	float4	pos = float4( p, 0.0 );
//			pos += 1.0 * dir;

#if 1	// iQ's method
	float	sumAO = 0.0;
	float	den = 1.0;
	for ( uint i=0; i < _stepsCount; i++ ) {
		float	distance = max( 0.0, map( pos.xyz ) ).x;
		sumAO += (pos.w - distance) * den;
		den *= 0.5;
		pos += dir;
	}

	return 1.0 - saturate( _strength * sumAO / _stepsCount );
#else
	float	sumAO = 0.0;
	float	den = 1.0;
	for ( uint i=0; i < _stepsCount; i++ ) {
		pos += dir;
		float	distance = max( 0.0, map( pos.xyz ) );
		float	ratio = distance / pos.w;
//		sumAO += ratio;
		sumAO += ratio * ratio;
	}
	return sumAO / _stepsCount;
#endif
}

float3	DebugPlane( float3 _color, float3 pos, float3 dir, float _rayHitDistance ) {
	float	planeHitDistance = -(pos - float3( 0, _DebugPlaneHeight, 0 )).y / dir.y;
	if ( planeHitDistance < 0.0 || planeHitDistance > _rayHitDistance )
		return _color;

	float	d = map( pos + planeHitDistance * dir ).x;
	float	i0 = saturate( d );
	float	i1 = saturate( d - 1.0 );
	float	i2 = saturate( d - 2.0 );
	float	i3 = saturate( d - 3.0 );
	float	i4 = saturate( d - 4.0 );
	float3	gradient[] = {
		float3( 0, 0, 0.25 ),
		float3( 0.25, 0, 0.25 ),
		float3( 0.25, 0, 0 ),
		float3( 0.5, 0.5, 0 ),
		float3( 0.5, 0.5, 0.5 ),
		float3( 1, 1, 1 ),
	};
	_color = lerp( lerp( lerp( lerp( lerp( gradient[0], gradient[1], i0 ), gradient[2], i1 ), gradient[3], i2 ), gradient[4], i3 ), gradient[5], i4 );

	// Add graduations
	float	isGraduation = smoothstep( 0.1, 0.0, abs( frac( 0.5 + 10.0 * d ) - 0.5 ) );
	float	isLargeGraduation = smoothstep( 0.05, 0.0, abs( frac( 0.5 + d ) - 0.5 ) );
	_color = lerp( _color, float3( 0, 0, 0 ), saturate( isGraduation + isLargeGraduation ) );

	return _color;
}

// iQ's standard ray-marching routing
float2 castRay( in float3 ro, in float3 rd, const float tmin=0.01, const float tmax=20.0 ) {
   
#if 0
    float tp1 = (0.0-ro.y)/rd.y; if( tp1>0.0 ) tmax = min( tmax, tp1 );
    float tp2 = (1.6-ro.y)/rd.y; if( tp2>0.0 ) {
		if( ro.y>1.6 ) tmin = max( tmin, tp2 );
		else           tmax = min( tmax, tp2 );
	}
#endif
    
	float precis = 0.002;
    float t = tmin;
    float m = -1.0;
    for( int i=0; i<50; i++ ) {
	    float2 res = map( ro+rd*t );
        if( res.x<precis || t>tmax )
			break;
        t += res.x;
	    m = res.y;
    }

    if( t>tmax )
		m = -1.0;	// Sky

    return float2( t, m );
}
