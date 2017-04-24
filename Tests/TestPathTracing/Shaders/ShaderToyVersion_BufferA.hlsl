/////////////////////////////////////////////////////////////////////////////////////////////////////
// Ziks: https://soundcloud.com/theblizzard/the-blizzard-piercing-the-fog
// https://soundcloud.com/omnia/andy-moor-vs-m-i-k-e-spirits <= Error 403
// https://soundcloud.com/fake_mood/stephan-bodzin-singularity-fake-mood-mirida-edit?in=paulo-henrique-cordeiro/sets/stephan-bodzin-singularity
// https://soundcloud.com/lifeanddeath/lad022-stephan-bodzin-singularity?in=lifeanddeath/sets/lad022-stephan-bodzin
// 

vec2	Sequence( float _time ) {
    float	t = mod( _time, 1000.0 );
    return vec2( 0.0, t );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the camera & sphere
vec3	ComputeSphereCenter( vec2 _sequence ) {
	return vec3( 0.6 * sin( 0.5 * _sequence.x ), -0.8, 0.8 );
}

void	ComputeCamera( vec2 _sequence, out vec3 _wsPos, out vec3 _wsTarget ) {
//    _wsPos = vec3( sin( _sequence.x ), cos( 0.713 * _sequence.x ), -0.8 );
    _wsPos = vec3( 0.0, 0.0, -0.5 );
    _wsTarget = vec3( 0.0, -0.2, 0.0 );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes sequencing for the walls
//
vec3	ComputeWallColor( vec2 _sequence, vec2 _UV ) {
    float	t = _sequence.y;
    vec3	color = vec3( 0.0 );
    if ( _sequence.x == 0.0 ) {
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
            color = 0.5 * (1.0 + (fragCoord.x < 1.0 ? wsCameraPos : wsCameraTarget));
        } else if ( fragCoord.x < 3.0 ) {
            color = 0.5 * (1.0 + sphereCenter);
        }
    }
    
    fragColor = vec4( color, 1.0 );
}
