/////////////////////////////////////////////////////////////////////////////////////////////////////
// Ziks: https://soundcloud.com/theblizzard/the-blizzard-piercing-the-fog
// https://soundcloud.com/omnia/andy-moor-vs-m-i-k-e-spirits <= Error 403
// https://soundcloud.com/fake_mood/stephan-bodzin-singularity-fake-mood-mirida-edit?in=paulo-henrique-cordeiro/sets/stephan-bodzin-singularity
// https://soundcloud.com/lifeanddeath/lad022-stephan-bodzin-singularity?in=lifeanddeath/sets/lad022-stephan-bodzin
// 
#define saturate( a ) clamp( (a), 0.0, 1.0 )
#define lerp( a, b, t ) mix( a, b, t )

//#define USE_MIC

const float	TEX_SIZE = 128.0;

const float PI = 3.1415926535897932384626433832795;
const float INVPI = 0.31830988618379067153776752674503;

#define TEND (7.01 * 60.0)	// Total song length

#define T1	16.0
#define T2	31.5
#define T3	(T2 + 20.0)
//#define T4	(T3 + 40.0)
#define T4	TEND
#define T5	TEND

#define	DT0 (T1)
#define	DT1 (T2 - T1)
#define	DT2 (T3 - T2)
#define	DT3 (T4 - T3)
#define	DT4 (T5 - T4)
#define	DT5 (T6 - T5)

float	BeatBump() {
    #ifdef USE_MIC
		float	bass = texture( iChannel1, vec2( 4.5 / 512.0, 0.25 ) ).x;
    #else
		float	bass = texture( iChannel0, vec2( 0.5 / 512.0, 0.25 ) ).x;
    #endif
        	bass = pow( saturate( 4.0 * (bass - 0.6) ), 2.0 );
    return bass;
}

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

vec3	Sequence( float _time ) {
    float	ID = 0.0;
    float	t = mod( _time, TEND );
    float	dt = 1.0;

//t += T1; // Force sequence 1
//t += T2; // Force sequence 2
//t += T3; // Force sequence 3
    
t = mod( t, T3 );
    
    // 0s -> 30s = build up
    if ( t < T1 ) {
        // 0s -> 12s => Vertical traveling
        dt = DT0;
    } else if ( t < T2 ) {
        // 12s -> 30s => Rolled traveling
        t -= T1;
        ID = 1.0;
        dt = DT1;
    } else if ( t < T3 ) {
        // 30s -> 50s => Flickering neons on walls + traveling
        t -= T2;
        ID = 2.0;
        dt = DT2;
/*    } else if ( t < T4 ) {
        // 50s -> XX => Flickering walls and colored bands on sphere
        t -= T3;
        dt = DT3 / 6.0;	// Split sequence into 6 parts
        float	timeSlice = floor( t / dt );
        t -= timeSlice * dt;
        ID = 3.0 + timeSlice;
*/    }
    
    return vec3( ID, t, dt );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the camera & sphere positions
void	ComputeSequencedCameras( float _sequenceID, float t, float dt,
                                 out vec3 _wsCameraPosition, out vec3 _wsCameraTarget, out vec3 _wsCameraUp,
                                 out vec3 _wsSphereCenter ) {
	_wsCameraPosition = _wsCameraTarget = vec3( 0.0 );
    _wsCameraUp = vec3( 0.0, 1.0, 0.0 );

	_wsSphereCenter = vec3( -0.6, -0.8, 0.8 );		// Fixed, near the corner

    float	rcpDt = 1.0 / dt;
    float	t2 = saturate( rcpDt * t );	// Time normalized to interval
    
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Intro with slow travelings
	if ( _sequenceID < 1.0 ) {
        // Vertical traveling
        _wsCameraPosition = vec3( 0.0, lerp( -0.9, -0.2, t2 ), -0.6 );
        _wsCameraTarget = vec3( 0.0, lerp( -2.0, -1.2, t2*t2 ), 0.8 );
	} else if ( _sequenceID < 2.0 ) {
        // Horizontal traveling with rolled camera
        _wsCameraUp = vec3( 1.0, -0.6, 0.0 );
        _wsCameraPosition = _wsSphereCenter + vec3( lerp( 0.2, 0.0, t2 ), -0.15, -0.5 );
        _wsCameraTarget = _wsSphereCenter + vec3( lerp( 0.2, 0.0, t2 ), -0.15, 0.0 );
	//
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Many small zooms and travelings 
    } else if ( _sequenceID < 3.0 ) {
       	_wsCameraTarget = _wsSphereCenter + vec3( 0.0, 0.0, 0.0 );
    	_wsCameraPosition = _wsSphereCenter + lerp( vec3( 1.0, -0.15, -0.02 ), vec3( -0.35, -0.18, -1.5 ), t2 );
    	_wsCameraUp = lerp( vec3( 0, 1, 0 ), vec3( 0.5, 1, 0 ), t2 );
	//
/*	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// 6 parts of alternating dark/light with different travelings
    } else if ( _sequenceID < 10.0 ) {
       	_wsCameraTarget = _wsSphereCenter + vec3( 1.0, 0.0, 0.0 );
    	_wsCameraPosition = _wsSphereCenter + vec3( -0.2, 0.0, -1.0 );
	//
	/////////////////////////////////////////////////////////////////////////////////////////////////////
*/  //
    } else if ( _sequenceID < 4.0 ) {
        // Tests
        _wsSphereCenter = vec3( 0.6 * sin( 0.5 * t ), -0.8, 0.8 );
    }
    
    _wsCameraUp = normalize( _wsCameraUp );
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the wall & sphere colors
void	ComputeSequencedColors( float _sequenceID, float t, float dt, vec2 _UV,
                                 out vec3 _colorWall,
                                 out vec3 _colorSphere ) {
	_colorWall = _colorSphere = vec3( 0.0 );

    vec2	pixel = _UV * TEX_SIZE;
    float	rcpDt = 1.0 / dt;
    float	t2 = saturate( rcpDt * t );	// Time normalized to interval
    
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Intro with flickering sphere and dark walls
	if ( _sequenceID < 2.0 ) {
        // Walls are dark to start with
		_colorWall = vec3( 0.0 );
        
        // Random flashes with a vertical gradient
        float	buildUp = saturate( 0.5*t );
        float	r = buildUp * noise( vec2( 3.897 * t + 17.651923, 0.0 ) );
        		r = pow( saturate( r + 1.0 * BeatBump() ), 2.0 );
        _colorSphere = vec3( 1000.0 * max( 0.001, r ) * pow( 1.2 - _UV.y, 2.0 ) );
	//
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Dark sphere + flickering lines on walls
    } else if ( _sequenceID < 3.0 ) {
        // Walls are flickering
        float	bump = BeatBump();
        float	part = floor( _UV.x * 4.0 ) + 1.0 * sin( 0.1 * PI * bump );
        float	rem = fract( _UV.x * 4.0 );
        float	lit = bump * saturate( 10.0 * fract( 0.1 * (pixel.y + 7.165 * part) + t ) - 9.0 );
        		lit = floor( _UV.x * 4.0 ) == 2.0 ? 0.1 * float( uint( rem * 16.0 ) & (uint( _UV.y * TEX_SIZE + 4.0 * t ) & 7U)): lit;
        
//lit = saturate( 10.0 * fract( 0.1 * pixel.y ) - 9.0 );
        
        _colorWall = vec3( 10.0 * lit * lerp( 0.1, 1.0, _UV.y ) );
	//
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Flickering walls and colored bands on sphere
/*    } else if ( _sequenceID < 10.0 ) {
        float	lit = 1.0;//step( 0.5, t2 );
        _colorWall = vec3( 0.1 * _UV.x * (0.5 + 0.5 * sin( _UV.y )) );//(0.5 + 0.5 * BeatBump()) );
        _colorWall *= lit;
//_colorWall = vec3( t2 );
	//
	/////////////////////////////////////////////////////////////////////////////////////////////////////
*/	//
    } else if ( _sequenceID < 3.0 ) {
        // Tests
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
    
//_colorSphere = vec3( BeatBump() );
//_colorSphere = vec3( 10.0 );
//    float	FFT = texture( iChannel0, vec2( UV.x, 0.25 ) ).x;
//    float	wave = texture( iChannel0, vec2( UV.x, 0.75 ) ).x;
////    fragColor = vec4( mix( FFT, wave, abs( sin( iGlobalTime ) ) ) );
//    fragColor = vec4( texture( iChannel0, vec2( UV.x, 0.25 + 0.5 * abs( sin( iGlobalTime ) ) ) ).x );
//	fragColor = pow( fragColor, vec4( 4.0 ) );
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
//
void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
	vec3	sequence = Sequence( iChannelTime[0] );

    vec3	wsCameraPosition, wsCameraTarget, wsCameraUp, wsSphereCenter;
	ComputeSequencedCameras( sequence.x, sequence.y, sequence.z, wsCameraPosition, wsCameraTarget, wsCameraUp, wsSphereCenter );

	vec2	UV = fract( fragCoord.xy / TEX_SIZE );
	vec3	colorWall, colorSphere;
   	ComputeSequencedColors( sequence.x, sequence.y, sequence.z, UV, colorWall, colorSphere );
 
    vec3	color = vec3( 0.0 );
    if ( fragCoord.y < TEX_SIZE && fragCoord.x < (2.0*TEX_SIZE) ) {
        // Bottom of the texture will contain the sequenced wall & sphere textures
		color = fragCoord.x < TEX_SIZE ? colorWall : colorSphere;
    } else if ( fragCoord.y > iResolution.y-1.0 ) {
        // Top line will contain sequenced positions
        color = fragCoord.x < 1.0 ? wsCameraPosition : (fragCoord.x < 2.0 ? wsCameraTarget : (fragCoord.x < 3.0 ? wsCameraUp : wsSphereCenter));
    } 
    fragColor = vec4( color, 1.0 );
}
