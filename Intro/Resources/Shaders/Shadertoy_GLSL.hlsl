
// If you want to puke, just uncomment these lines... ^^
//#define ANIMATE_TUNNEL
//#define ROTATE_BARS
//#define TRANSLATE_BARS

// Define this to feel the power of the turd
//#define TURD_MODE

const float PI = 3.14159265358979;

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

float noise( in vec3 x )
{
   vec3 p = floor(x);
   vec3 f = fract(x);
	f = f*f*(3.0-2.0*f);

	vec2 uv = (p.xy+vec2(37.0,17.0)*p.z) + f.xy;
	vec2 rg = texture2D( iChannel0, (uv+0.5)/256.0, -100.0 ).yx;
	return mix( rg.x, rg.y, f.z );
}

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
vec3	Fresnel_IORFromF0( vec3 _F0 )
{
	vec3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
vec3	Fresnel( vec3 _IOR, float _CosTheta, float _FresnelStrength )
{
	float	c = mix( 1.0, _CosTheta, _FresnelStrength );
	vec3	g_squared = max( vec3(0.0), _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	vec3	g = sqrt( g_squared );

	vec3	a = (g - c) / (g + c);
			a *= a;
	vec3	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

float smoothinterp( float x0, float x1, float t )
{
	return mix( x0, x1, smoothstep( 0.0, 1.0, t ) );
}

vec2 tunnelCenter( float z )
{
#ifdef ANIMATE_TUNNEL
	return vec2(	0.1 * (sin( 0.5919 * z ) + sin( 1.2591 * z )*cos( 0.915 * z )),
					0.1 * (sin( 1.8 * z ) + sin( 0.1378 * z )) );
#else
	return vec2( 0.0, 0.0 );
#endif
}

const float barGap = 1.0;

void barCenterAxis( float z, out vec3 barCenter, out vec3 barAxis, out float barOffset )
{	
	vec2	c = tunnelCenter( z ); 	// Tunnel center
	
	// Retrieve bar index & compute rotation/offset
	float	barIndex = floor( z / barGap );
	float	zcenter = (0.5+barIndex) * barGap;
	float	angle = 2.0 * PI * 0.156 * (0.2+barIndex);// + iGlobalTime;
	
	angle = 0.798 * barIndex;	// Fixed bars
#ifdef ROTATE_BARS
	angle += iGlobalTime * (0.5 + 1.0 * sin( 1234.567 * barIndex ));
#endif

	barAxis = vec3( cos( angle ), sin( angle ), 0.2 * sin( zcenter ) );

#ifdef TRANSLATE_BARS
	barOffset = 0.0 + 0.5 * sin( 17.21191 * barIndex + iGlobalTime * (0.5 + 1.0 * sin( 8976.5431 * barIndex )) );
#else
	barOffset = 0.0 + 0.5 * sin( 17.21191 * barIndex );
#endif

	barCenter = vec3( c + barOffset * vec2( -barAxis.y, barAxis.x ), zcenter );
}

vec3 safePosition( float z, float rand )
{
	vec3	barCenter0, barAxis0; float barOffset0;
	barCenterAxis( z - 0.5 * barGap, barCenter0, barAxis0, barOffset0 );
	vec2	tunnelCenter0 = tunnelCenter( barCenter0.z );

	vec3	barCenter1, barAxis1; float barOffset1;
	barCenterAxis( z + 0.5 * barGap, barCenter1, barAxis1, barOffset1 );
	vec2	tunnelCenter1 = tunnelCenter( barCenter1.z );
	
	float	z0 = barCenter0.z;
	float	z1 = barCenter1.z;
	float	t = (z - z0) / (z1 - z0); // Interpolant
	
	// Compute the 2 valid positions for each bar
	float	off0 = mix( 0.5*(1.0+barOffset0), 0.5*(-1.0+barOffset0),
						step( 0.0, sign( sin( 37.85961 * (z0 + rand) ) ) ) );	// Choose between left or right of the bar...
	vec2	safePos0 = tunnelCenter0 + off0 * vec2( -barAxis0.y, barAxis0.x );

	float	off1 = mix( 0.5*(1.0+barOffset1), 0.5*(-1.0+barOffset1),
						step( 0.0, sign( sin( 37.85961 * (z1 + rand) ) ) ) );
	vec2	safePos1 = tunnelCenter1 + off1 * vec2( -barAxis1.y, barAxis1.x );
	
	return vec3(	smoothinterp( safePos0.x, safePos1.x, t ),
					smoothinterp( safePos0.y, safePos1.y, t ),
					z );
}

float bisou( vec3 p )
{
	vec3	c, axis; float offset;
	barCenterAxis( p.z, c, axis, offset );
	
	vec3	toP = p-c;
	vec3	planeP = toP - dot( toP, axis ) * axis;
	
	return length( planeP ) - 0.1
//		- 0.05 * noise(37.0569 * p )
		;
}

float map( vec3 p )
{
	vec2	c = tunnelCenter( p.z );
	float	d_tunnel = 1.0 - length( p.xy - c );
	float	d_bar = bisou( p );

	return smin2( d_tunnel, d_bar, 0.7 )
#ifdef TURD_MODE
		- 0.025 * noise( 67.0569 * p )
#endif
	;
	
	return min( d_tunnel, d_bar );
	return smin( d_tunnel, d_bar, -4.0 );
}

vec3 normal( vec3 p, const float eps, out float cheapAO )
{
	vec2 e = vec2( eps, 0.0 );
	float c = map( p );
	vec3	n = vec3(
		map( p + e.xyy ) - map( p - e.xyy ),
		map( p + e.yxy ) - map( p - e.yxy ),
		map( p + e.yyx ) - map( p - e.yyx )
		);
	cheapAO = length( n );
	return n / cheapAO;
}

vec3 reflection( vec3 p, vec3 v, vec3 n )
{
	v = reflect( v, n );
//	p += (0.01 / dot( v, n )) * v;
	p += 0.01 * n;

	float	t = 0.0;
	for ( int i=0; i < 64; i++ )
	{
		float	d = map( p );
		if ( d < 0.005 ) break;

		t += d;
		p += d * v;
	}

	return p;
}

float AO( vec3 p, vec3 n )
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

float	Shadow( vec3 p, vec3 l, float distance2Light )
{
	const float	k = 10.0;

	float S = 1.0;
	float t = 0.05;
	for ( int i=0; i < 64; i++ )
	{
		float	h = map( p + t * l );
		if ( h < 0.0001 )
			return 0.0;

		S = min( S, k*h / t );
		t += h;
		if ( t > distance2Light )
			break;
	}
	return S;	
}

void	main()
{
	vec2	uv = gl_FragCoord.xy / iResolution.xy;
	
	float	z = 1.0 * iGlobalTime;

	vec3	p = safePosition( z, 0.0 );

	vec3	target = p + vec3(
		sin( 1.2 * iGlobalTime ),
		sin( 1.0 + 0.7891 * iGlobalTime ),
		5.0 + 2.0 * sin( 1.2 * iGlobalTime )
		);
	
	vec3	at = normalize( target - p );
	vec3	right = normalize( cross( at, vec3( 0, 1, 0 ) ) );
	vec3	up = cross( right, at );

	float	Tan = 0.6;
	vec3	v = normalize( vec3( iResolution.x / iResolution.y * Tan * (2.0 * uv.x - 1.0), Tan * (2.0 * uv.y - 1.0), 1.0 ) );
			v = v.x * right + v.y * up + v.z * at;

	// Compute light position
	float	lightTime = 0.25 * iGlobalTime;
	float	z_light = z + 4.0 + 3.0 * sin( 3.156 * lightTime ) * sin( 0.15891 * lightTime );
	vec3	l = safePosition( z_light, 187.65 );

	// March!
	vec3	prevPos2Light = l - p;
	float	prevDist2Light = length( prevPos2Light );
			prevPos2Light /= max( 1e-4, prevDist2Light );

	float	scatt = 0.0;
	vec3	p_orig = p;
	for ( int i=0; i < 64; i++ )
	{
		float	d = map( p );
		if ( d < 0.005 ) break;
		p += d * v;

		vec3	pos2Light = l - p;
		float	dist2Light = length( pos2Light );
				pos2Light /= max( 1e-4, dist2Light );
		scatt += d / pow( 0.5 * (prevDist2Light + dist2Light), 2.0 );
		prevPos2Light = pos2Light;
		prevDist2Light = dist2Light;
	}

	const float		LightIntensity = 0.5;
	const vec3	C0 = 1.0 * vec3( 0.2, 0.2, 0.2 );
	const vec3	C1 = vec3( 1.0, 1.0, 1.0 );

	scatt = pow( max( 0.0, 0.2 * (scatt - 0.25) ), 2.0 );

	// Compute normal and Fresnel
	float	AO;
	vec3	n = normal( p, 0.0001, AO );
	vec3	F0 = 0.05 * vec3( 0.5, 0.8, 1.0 );
	vec3	Fr = clamp( vec3( 0.0 ), vec3( 1.0 ), Fresnel( Fresnel_IORFromF0( F0 ), dot( -v, n ), 1.0 ) );

	// Compute direct lighting
	vec3	Light = l - p;
	float	dLight = length( Light );
			Light *= 1.0 / max( 0.001, dLight );
	float	shadow = Shadow( p, Light, dLight );
	vec3	colorT = (LightIntensity / (dLight*dLight)) * mix( C0, shadow * C1, 0.5 + 0.5 * dot( n, Light ));

	// Compute reflection
	vec3	p_refl = reflection( p, v, n );
	vec3	n_refl = normal( p_refl, 0.0001, AO );

			Light = l - p_refl;
			dLight = max( 0.05, length( Light ) );
			Light *= 1.0 / dLight;
	vec3	colorR = (LightIntensity / (dLight*dLight)) * mix( AO*C0, C1, 0.5 + 0.5 * dot( n_refl, Light ));
	
	vec3	color = mix( colorT, colorR, Fr );

	color += scatt * vec3( 1.0, 0.5, 0.3 );

	// Apply fog
	float	t = length( p - p_orig );
	float	fog = exp( -0.05 * t );
	color = mix( vec3( 1.0, 0.9, 0.8 ), color, fog );

	color = pow( color, vec3(1.0/2.2) );
	
	gl_FragColor = vec4( color, 1.0 );
}
