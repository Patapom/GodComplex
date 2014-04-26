//////////////////////////////////////////////////////////////////////////
// This shader post-processes the Global Illumination test room
//
#include "Inc/Global.hlsl"

static const float3	dUV = float3( 1.0 / RESX, 1.0 / RESY, 0.0 );

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

#define HLSL

#define mix	lerp
#define mod	fmod
#define fract frac
#define atan atan2
#define iGlobalTime _Time.x
static const float2	iResolution = float2( RESX, RESY );

//const float PI = 3.14159265358979;


float repeat( float a, float s )
{
	return mod( a - 0.5*s, s ) + 0.5*s;
}

float smin( float a, float b, float k )
{
	return log( max( 0.001, exp( a*k ) + exp( b*k ) ) ) / k;
	return log( max( 0.001, exp( (-10.0+a)*k ) + exp( (-10.0+b)*k ) ) ) / k + 10.0;
}

float smin2( float a, float b, float k )
{
    float h = clamp( 0.5+0.5*(b-a)/k, 0.0, 1.0 );
    return mix( b, a, h ) - k*h*(1.0-h);
}

float hash( float n ) { return fract(sin(n)*43758.5453123); }

// float noise( in float3 x )
// {
//     float3 p = floor(x);
//     float3 f = fract(x);
// 	f = f*f*(3.0-2.0*f);
// 	
// 	float2 uv = (p.xy+float2(37.0,17.0)*p.z) + f.xy;
// 	float2 rg = texture2D( iChannel0, (uv+0.5)/256.0, -100.0 ).yx;
// 	return mix( rg.x, rg.y, f.z );
// }

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float3	Fresnel_IORFromF0( float3 _F0 )
{
	float3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float3	Fresnel( float3 _IOR, float _CosTheta, float _FresnelStrength )
{
	float	c = mix( 1.0, _CosTheta, _FresnelStrength );
	float3	g_squared = max( 0.0, _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	float3	g = sqrt( g_squared );

	float3	a = (g - c) / (g + c);
			a *= a;
	float3	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

float smoothinterp( float x0, float x1, float t )
{
	return mix( x0, x1, smoothstep( 0, 1, t ) );
}

float2 tunnelCenter( float z )
{
	return float2( 0.0, 0.0 );
	return float2(	0.1 * (sin( 0.5919 * z ) + sin( 1.2591 * z )*cos( 0.915 * z )),
					0.1 * (sin( 1.8 * z ) + sin( 0.1378 * z )) );
}

static const float barGap = 1.0;

void barCenterAxis( float z, out float3 barCenter, out float3 barAxis, out float barOffset )
{	
	float2	c = tunnelCenter( z ); 	// Tunnel center
	
	// Retrieve bar index & compute rotation/offset
	float	barIndex = floor( z / barGap );
	float	zcenter = (0.5+barIndex) * barGap;
	float	angle = 2.0 * PI * 0.156 * (0.2+barIndex);// + iGlobalTime;
	
angle = 0.798 * barIndex;
//angle = 0.0;

	barAxis = float3( cos( angle ), sin( angle ), 0.2 * sin( zcenter ) );

//	float2	barOffset = float2( 0.2-0.1 * sin( zcenter ), 0.5 + 0.3 * sin( -1.5 * zcenter)  );

	barOffset = 0.0 + 0.5 * sin( 17.21191 * barIndex );
//	float3	barCenter = float3( center( zcenter ) + barOffset, zcenter );
	barCenter = float3( c + barOffset * float2( -barAxis.y, barAxis.x ), zcenter );
}

float3 safePosition( float z, float rand )
{
	float3	barCenter0, barAxis0; float barOffset0;
	barCenterAxis( z - 0.5 * barGap, barCenter0, barAxis0, barOffset0 );
	float2	tunnelCenter0 = tunnelCenter( barCenter0.z );

	float3	barCenter1, barAxis1; float barOffset1;
	barCenterAxis( z + 0.5 * barGap, barCenter1, barAxis1, barOffset1 );
	float2	tunnelCenter1 = tunnelCenter( barCenter1.z );

//	float3	barCenter2, barAxis2; float barOffset2;
//	barCenterAxis( z + barGap, barCenter2, barAxis2, barOffset2 );
	
//	avoidPos = float3( c - barOffset * float2( -barAxis.y, barAxis.x ), zcenter );
	
	float	z0 = barCenter0.z;
	float	z1 = barCenter1.z;
	float	t = (z - z0) / (z1 - z0); // Interpolant
	
//t = 0.0;
	
	// Compute the 2 valid positions for each bar
	float	off0 = mix( 0.5*(1.0+barOffset0), 0.5*(-1.0+barOffset0),
						step( 0.0, sign( sin( 37.85961 * (z0 + rand) ) ) ) );	// Choose between left or right of the bar...
	float2	safePos0 = tunnelCenter0 + off0 * float2( -barAxis0.y, barAxis0.x );

	float	off1 = mix( 0.5*(1.0+barOffset1), 0.5*(-1.0+barOffset1),
						step( 0.0, sign( sin( 37.85961 * (z1 + rand) ) ) ) );
	float2	safePos1 = tunnelCenter1 + off1 * float2( -barAxis1.y, barAxis1.x );
	
//return float3( safePos1, z );
	
//	float3	safePos = float3( smoothstep( safePos0, safePos1, t ), z );
//	float3	safePos = float3( mix( safePos0, safePos1, t ), z );
	float3	safePos = float3( smoothinterp( safePos0.x, safePos1.x, t ),
							  smoothinterp( safePos0.y, safePos1.y, t ),
							  z );
	return safePos;
}

float bisou2( float3 p ) 
{
	float	r = length( p.xy );
	float 	a = atan( p.y, p.x );
	//		a = repeat( a, PI/6.0 );
	a = (0.5 + floor( a * 6.0 / PI )) * PI / 6.0;
	
	float3	c = float3( 0.9 * float2( cos( a ), sin( a ) ), p.z );
	
	return length( p - c ) - 0.01;
}

float bisou( float3 p )
{
	float3	c, axis; float offset;
	barCenterAxis( p.z, c, axis, offset );
	
	float3	toP = p-c;
	float3	planeP = toP - dot( toP, axis ) * axis;
	
	return length( planeP ) - 0.1;// - 0.05 * noise(37.0569 * p );
}

float map( float3 p )
{
	float2	c = tunnelCenter( p.z );
	float	d_tunnel = 1.0 - length( p.xy - c );
	
	float	d_gloub = bisou( p );
//return d_gloub;

	return smin2( d_tunnel, d_gloub, 0.7 );
	return min( d_tunnel, d_gloub );
	return smin( d_tunnel, d_gloub, -4.0 );
}

float3 normal( float3 p, float eps, out float cheapAO )
{
	const float2 e = float2( eps, 0.0 );
	float c = map( p );
	float3	n = float3(
		map( p + e.xyy ) - map( p - e.xyy ),
		map( p + e.yxy ) - map( p - e.yxy ),
		map( p + e.yyx ) - map( p - e.yyx )
		);
	cheapAO = length( n );
	return n / cheapAO;
}

float3 reflection( float3 p, float3 v, float3 n )
{
	v = reflect( v, n );
	p += (0.01 / dot( v, n )) * v;
//	p += 0.01 * n;

	float	t = 0.0;
	for ( int i=0; i < 64; i++ )
	{
		float	d = map( p );
//		if ( abs( d ) < 0.001 ) break;
		if ( d < 0.005 ) break;

		t += d;
		p += d * v;
	}

//return 0.1 * t;

	return p;
}

float AO( float3 p, float3 n )
{
	const float step = 0.01;
	p += 0.1 * n;
	float AO = 1.0;
	for ( int i=0; i < 16; i++ )
	{
		float	d = max( 0.0, map( p ) );
		p += step * n;
		
		AO *= 1.0 - exp( -20.0 * d * (2.0+float(i)) );
	}
	return AO;
}

float	Shadow( float3 p, float3 l, float distance2Light )
{
// 	const float step = 0.05;
// //	p += (0.001 / dot( l, n )) * l;
// 	p += 0.1 * n;
// 	float S = 1.0;
// 	for ( int i=0; i < 32; i++ )
// 	{
// 		float	d = max( 0.0, map( p ) );
// 		p += step * l;
// 		
// 		S *= 1.0 - exp( -20.0 * d * (50.0+float(i)) );
// 	}
// 	return S;

	const float	k = 10.0;

	float S = 1.0;
	for( float t=0.05; t < distance2Light; )
	{
		float	h = map( p + t * l );
		if( h < 0.0001 )
			return 0.0;

		S = min( S, k*h / t );
		t += h;
	}
	return S;	
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	uv = _In.__Position.xy * dUV.xy;
	
	float	z = 1.0 * iGlobalTime;

	float3	p = safePosition( z, 0.0 );

	float3	target = p + float3(
		sin( 1.2 * iGlobalTime ),
		sin( 1.0 + 0.7891 * iGlobalTime ),
		5.0 + 2.0 * sin( 1.2 * iGlobalTime )
		);
//target = p + float3( 0, 0, 1 );
	
	float3	at = normalize( target - p );
	float3	right = normalize( cross( at, float3( 0, 1, 0 ) ) );
	float3	up = cross( right, at );

	float	Tan = 0.6;
#ifdef HLSL
	float3	v = normalize( float3( iResolution.x / iResolution.y * Tan * (2.0 * uv.x - 1.0), Tan * (1.0 - 2.0 * uv.y), 1.0 ) );
//	float3	v = float3( iResolution.x / iResolution.y * Tan * (2.0 * uv.x - 1.0), Tan * (1.0 - 2.0 * uv.y), 1.0 );
#else
	float3	v = normalize( float3( iResolution.x / iResolution.y * Tan * (2.0 * uv.x - 1.0), Tan * (2.0 * uv.y - 1.0), 1.0 ) );
#endif
			v = v.x * right + v.y * up + v.z * at;

// return float4( v, 1 );

	// Compute light position
	float	lightTime = 0.25 * iGlobalTime;
	float	z_light = z + 4.0 + 3.0 * sin( 3.156 * lightTime ) * sin( 0.15891 * lightTime );
	float3	l = safePosition( z_light, 187.65 );

	// March!
	float3	prevPos2Light = l - p;
	float	prevDist2Light = length( prevPos2Light );
			prevPos2Light /= max( 1e-4, prevDist2Light );

	float	scatt = 0.0;
	float3	p_orig = p;
	int	i=0;
	for ( ; i < 64; i++ )
	{
		float	d = map( p );
		if ( d < 0.005 ) break;
		p += d * v;

		float3	pos2Light = l - p;
		float	dist2Light = length( pos2Light );
				pos2Light /= max( 1e-4, dist2Light );
		scatt += d / pow( 0.5 * (prevDist2Light + dist2Light), 2.0 );
		prevPos2Light = pos2Light;
		prevDist2Light = dist2Light;
	}

// return i / 64.0;

	const float		LightIntensity = 0.5;
	const float3	C0 = 1.0 * float3( 0.2, 0.2, 0.2 );
	const float3	C1 = float3( 1.0, 1.0, 1.0 );

	scatt = pow( max( 0.0, 0.2 * (scatt - 0.25) ), 2.0 );

	// Compute normal and Fresnel
	float	AO;
	float3	n = normal( p, 0.0001, AO );
	float3	F0 = 0.05 * float3( 0.5, 0.8, 1.0 );
	float3	Fr = clamp( 0.0, 1.0, Fresnel( Fresnel_IORFromF0( F0 ), dot( -v, n ), 1.0 ) );


// const float2 e = float2( 0.0001, 0.0 );
// float c = map( p );
// n = float3(
// 	map( p + e.xyy ) - c, //map( p - e.xyy ),
// 	map( p + e.yxy ) - c, //map( p - e.yxy ),
// 	map( p + e.yyx ) - c //map( p - e.yyx )
// 	);
// //return float4( 100.0 * n, 0 );
// float	length_n = length(n);
// //return 10.0 * length_n;
// return float4( n / length_n, 0 );



//return float4( ddx(p), 1 );
//return float4( 1.0*p- float3(0,0,8), 1 );
//return AO;
//return float4( n, 1 );
//Fr = 1.0;

	// Compute direct lighting
	float3	Light = l - p;
	float	dLight = length( Light );
			Light *= 1.0 / max( 0.001, dLight );
	float	shadow = Shadow( p, Light, dLight );
	float3	colorT = (LightIntensity / (dLight*dLight)) * mix( C0, shadow * C1, 0.5 + 0.5 * dot( n, Light ));

//return shadow;
//return float4( colorT, 1 );

	// Compute reflection
	float3	p_refl = reflection( p, v, n );

//return float4( p_refl, 1 );

	float3	n_refl = normal( p_refl, 1.0, AO );

			Light = l - p_refl;
			dLight = max( 0.05, length( Light ) );
			Light *= 1.0 / dLight;
	float3	colorR = (LightIntensity / (dLight*dLight)) * mix( AO*C0, C1, 0.5 + 0.5 * dot( n_refl, Light ));
	
	float3	color = mix( colorT, colorR, Fr );

	color += scatt * float3( 1.0, 0.5, 0.3 );

	// Apply fog
	float	t = length( p - p_orig );
	float	fog = exp( -0.05 * t );
	color = mix( float3( 1.0, 0.9, 0.8 ), color, fog );

	return float4( color, 1.0 );
}
