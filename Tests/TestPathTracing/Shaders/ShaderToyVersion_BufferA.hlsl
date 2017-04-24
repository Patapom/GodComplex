/////////////////////////////////////////////////////////////////////////////////////////////////////
// Ziks: https://soundcloud.com/theblizzard/the-blizzard-piercing-the-fog
// https://soundcloud.com/omnia/andy-moor-vs-m-i-k-e-spirits <= Error 403
// https://soundcloud.com/fake_mood/stephan-bodzin-singularity-fake-mood-mirida-edit?in=paulo-henrique-cordeiro/sets/stephan-bodzin-singularity
// https://soundcloud.com/lifeanddeath/lad022-stephan-bodzin-singularity?in=lifeanddeath/sets/lad022-stephan-bodzin
// 
#define saturate( a ) clamp( (a), 0.0, 1.0 )
#define lerp( a, b, t ) mix( a, b, t )

float rand( float n ) { return fract(sin(n) * 43758.5453123); }

float rand( vec2 _seed ) {
	return fract( sin( dot( _seed, vec2( 12.9898, 78.233 ) ) ) * 43758.5453 );
//= frac( sin(_pixelPosition.x*i)*sin(1767.0654+_pixelPosition.y*i)*43758.5453123 );
}

vec2 hash( vec2 x ) {
    const vec2 k = vec2( 0.3183099, 0.3678794 );
    x = x*k + k.yx;
    return -1.0 + 2.0*fract( 16.0 * k*fract( x.x*x.y*(x.x+x.y)) );
}

float noise( in vec2 p ) {
    vec2 i = floor( p );
    vec2 f = fract( p );
	
	vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( hash( i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ), 
                     dot( hash( i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( hash( i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ), 
                     dot( hash( i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}


float	BeatBump() {
	float	bass = texture( iChannel0, vec2( 0.5 / 512.0, 0.25 ) ).x;
        	bass = pow( saturate( 4.0 * (bass - 0.6) ), 2.0 );
    return bass;
}

vec2	Sequence( float _time ) {
    float	t = mod( _time, 10000.0 );
    // 0s -> 30s = build up
    return vec2( 0.0, t );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the camera & sphere
void	ComputeSequencedElements( float _sequenceID, float t, vec2 _UV,
                                 out vec3 _wsCameraPosition, out vec3 _wsCameraTarget,
                                 out vec3 _wsSphereCenter,
                                 out vec3 _colorWall,
                                 out vec3 _colorSphere ) {
_wsCameraPosition = _wsCameraTarget = _wsSphereCenter = _colorWall = _colorSphere = vec3( 0.0 );
    
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	if ( _sequenceID < 1.0 ) {
        // Fixed viewpoint with slow traveling of target...
        float	t2 = saturate( 0.1 * t );
	    _wsCameraPosition = vec3( 0.0, -0.8, -0.5 );
        _wsCameraTarget = vec3( 0.0, -0.8, lerp( -0.8, 0.8, t2 ) );

		// Fixed, near the corner
        _wsSphereCenter = vec3( -0.6, -0.8, 0.8 );
        
        // Walls are dark to start with
		_colorWall = vec3( 0.0 );
        
        // Random flashes with a vertical gradient
//        color = vec3( BeatBump() );
        float	buildUp = t2;//saturate( 0.1*t );
        float	r = buildUp * noise( vec2( 3.897 * t + 17.651923, 0.0 ) );
        		r = pow( saturate( r + 1.0 * BeatBump() ), 2.0 );
        _colorSphere = vec3( 20.0 * r * (1.0 - _UV.y) );
	//
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	//
    } else if ( _sequenceID < 2.0 ) {
        // Tests
        _wsSphereCenter = vec3( 0.6 * sin( 0.5 * t ), -0.8, 0.8 );

		_colorWall = vec3( texture( iChannel0, vec2( 1.0 - _UV.x, 0.25 + 0.5 * abs( sin( t ) ) ) ).x );
//		_colorWall = texture( iChannel1, UV ).xxx;
		_colorWall = pow( _colorWall, vec3( 4.0 ) );
        
		// Map sphere color depending on height
//		float	V = 0.5 * (1.0 + (_wsPosition.y - _wsSphereCenter.y) * (1.0 / SPHERE_RADIUS));
		float	V = _UV.y;
		const float	BANDS_COUNT = 20.0;
		V *= BANDS_COUNT;
		float	bandIndex = floor( V );
		float	Vband = 2.0 * fract( V ) - 1.0;
				Vband = sqrt( 1.0 - Vband*Vband );
		float	intensity = abs( sin( 4.0 * t + bandIndex * sin( t ) ) ) * Vband;
		vec3	bandColor = fract( vec3( 13.289 * bandIndex, 0.9 - 3.18949 * bandIndex, 17.0 * bandIndex ) );
				bandColor = 0.25 + 0.75 * bandColor;
		_colorSphere = intensity * bandColor;
    }
}

vec3	ComputeSphereCenter( vec2 _sequence ) {
    float	t = _sequence.y;
    vec3	pos = vec3( 0.0 );
    if ( _sequence.x == 0.0 ) {
        // Fixed, near the corner
        pos = vec3( -0.6, -0.8, 0.8 );
    } else {
        pos = vec3( 0.6 * sin( 0.5 * t ), -0.8, 0.8 );
    }
	return pos;
}

void	ComputeCamera( vec2 _sequence, out vec3 _wsPos, out vec3 _wsTarget ) {
    float	t = _sequence.y;
    _wsPos = vec3( 0.0, -0.8, -0.5 );
//    _wsPos = vec3( sin( t ), cos( 0.713 * t ), -0.8 );
    _wsTarget = vec3( 0.0, -0.8, 0.0 );

    if ( _sequence.x < 1.0 ) {
        // Fixed viewpoint with slow traveling of target...
        float	t2 = saturate( 0.1 * t );
	    _wsPos = vec3( 0.0, -0.8, -0.5 );
        _wsTarget = vec3( 0.0, -0.8, lerp( -0.8, 0.8, t2 ) );
    }

//_wsTarget = vec3( sin( 2.0 * t ), -0.2, 0.8 );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the walls
//
vec3	ComputeWallColor( vec2 _sequence, vec2 _UV ) {
    float	t = _sequence.y;
    vec3	color = vec3( 0.0 );
    if ( _sequence.x == 0.0 ) {
        // Walls are dark to start with
		color = vec3( 0.0 );
    } else if ( _sequence.x == 1.0 ) {
		color = vec3( texture( iChannel0, vec2( 1.0 - _UV.x, 0.25 + 0.5 * abs( sin( t ) ) ) ).x );
//		color = texture( iChannel1, UV ).xxx;
		color = pow( color, vec3( 4.0 ) );
    }
    return color;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the sphere
//
vec3	ComputeSphereColor( vec2 _sequence, vec2 _UV, vec3 _wsSphereCenter ) {
    float	t = _sequence.y;
    vec3	color = vec3( 0.0 );
    if ( _sequence.x == 0.0 ) {
        // Random flashes
//        color = vec3( BeatBump() );
        float	buildUp = saturate( 0.1*t );
        float	r = buildUp * noise( vec2( 3.897 * t + 17.651923, 0.0 ) );
        		r = pow( saturate( r + 1.0 * BeatBump() ), 2.0 );
        color = vec3( 20.0 * r * (1.0 - _UV.y) );
        
    } else if ( _sequence.x == 1.0 ) {
		// Map sphere color depending on height
//		float	V = 0.5 * (1.0 + (_wsPosition.y - _wsSphereCenter.y) * (1.0 / SPHERE_RADIUS));
		float	V = _UV.y;
		const float	BANDS_COUNT = 20.0;
		V *= BANDS_COUNT;
		float	bandIndex = floor( V );
		float	Vband = 2.0 * fract( V ) - 1.0;
				Vband = sqrt( 1.0 - Vband*Vband );
		float	intensity = abs( sin( 4.0 * t + bandIndex * sin( t ) ) ) * Vband;
		vec3	bandColor = fract( vec3( 13.289 * bandIndex, 0.9 - 3.18949 * bandIndex, 17.0 * bandIndex ) );
				bandColor = 0.25 + 0.75 * bandColor;
		color = intensity * bandColor;
	}
    
color = vec3( 10.0 );
    
    return color;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
//
void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
//	vec2 UV = fragCoord.xy / iResolution.xy;
//	fragColor = vec4(UV,0.5+0.5*sin(iGlobalTime),1.0);
//    float	FFT = texture( iChannel0, vec2( UV.x, 0.25 ) ).x;
//    float	wave = texture( iChannel0, vec2( UV.x, 0.75 ) ).x;
////    fragColor = vec4( mix( FFT, wave, abs( sin( iGlobalTime ) ) ) );
//    fragColor = vec4( texture( iChannel0, vec2( UV.x, 0.25 + 0.5 * abs( sin( iGlobalTime ) ) ) ).x );
//	fragColor = pow( fragColor, vec4( 4.0 ) );
    
	vec2	sequence = Sequence( iChannelTime[0] );
#if 1
	vec2	UV = fract( fragCoord.xy / 64.0 );
	vec3	wsCameraPosition, wsCameraTarget, wsSphereCenter, colorWall, colorSphere;
	ComputeSequencedElements( sequence.x, sequence.y, UV, wsCameraPosition, wsCameraTarget, wsSphereCenter, colorWall, colorSphere );

    vec3	color = vec3( 0.0 );
    if ( fragCoord.y < 64.0 && fragCoord.x < 128.0 ) {
		color = fragCoord.x < 64.0 ? colorWall : colorSphere;
    } else if ( fragCoord.y > iResolution.y-1.0 ) {
        color = fragCoord.x < 1.0 ? wsCameraPosition : (fragCoord.x < 2.0 ? wsCameraTarget : wsSphereCenter);
    } 
#else    
    vec3	sphereCenter = ComputeSphereCenter( sequence );
    
    vec3	color = vec3( 0.0 );
    if ( fragCoord.y < 64.0 && fragCoord.x < 128.0 ) {
		vec2	UV = fragCoord.xy / 64.0;
        if ( fragCoord.x < 64.0 ) {
            // Material 0 = Walls
            color = ComputeWallColor( sequence, UV );
        } else {
            // Material 1 = Sphere
			UV.x -= 1.0;
            color = ComputeSphereColor( sequence, UV, sphereCenter );
        }
    } else if ( fragCoord.y > iResolution.y-1.0 ) {
        // Compute camera & sphere vectors
        if ( fragCoord.x < 2.0 ) {
            vec3	wsCameraPos, wsCameraTarget;
            ComputeCamera( sequence, wsCameraPos, wsCameraTarget );
            color = fragCoord.x < 1.0 ? wsCameraPos : wsCameraTarget;
        } else if ( fragCoord.x < 3.0 ) {
            color = sphereCenter;
        }
    }
#endif    
    fragColor = vec4( color, 1.0 );
}
