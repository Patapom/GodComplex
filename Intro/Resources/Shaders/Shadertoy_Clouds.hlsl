// Firstly for the original and best visuals see the original shader by iq: https://www.shadertoy.com/view/XslGRr
// I had to mess with a few things to make this tech demo work :(, due to the limitations
// of working within a stateless shader, so the quality is compromised here.

// In this shader I make use of the tree-based sampling. See my SIGGRAPH talk for
// a proper introduction: http://advances.realtimerendering.com/s2013/OceanShoestring_SIGGRAPH2013_Online.pptx
// And this shader for a live demo: https://www.shadertoy.com/view/XsfSRn

// To summarise, sample distances from the camera are chosen by intersecting a line
// with a binary tree. The line is chosen to produce a dense sampling near the camera
// and a sparse sampling futher away. This means we can render an expansive volume
// with just 32 ray steps. Furthermore, this line is moved with the camera, so that the
// samples tend to stay relatively stationary in world space. This reduces the aliasing
// issues associated with a moving camera. You can set the camera speed below using
// the SPEED constant.

// To see the difference all this makes, click the mouse to see typical view space sampling.
// The samples are placed in at similar locations but no longer move with the camera.

// It's crazy - and sloooow - to generate the sample locations in the shader - in practice this
// would be done on the CPU and passed in as an array or in a texture. The variable
// dens_x moves the line forward. This would be integrated on the CPU as follows:
//   dens_x += dot( camForward, camVelocity*dt );

// Future work:
// * Remove noise octaves in the distance. Due to branching costs this is probably
//   best done by writing a few different noise functions for different LODs and
//   unrolling the raymarch loop.
// * Fade out the last sample can pop in or out of existence. I don't know if there's a great
//   solution for this, besides simply not using it.

//#define FULL_PROCEDURAL

#define SPEED 2.
float dens_x = 0.;
float dens_m = .25;

float dens( float x )
{
	return dens_m * (x - dens_x) ;//+ y0;
}
float dens_inv( float y )
{
	return (y /*- y0*/) / dens_m + dens_x;
}

bool canonicalInt( float i_x0, float i_y0, float segIndex, out float i0, out float i1 )
{
	float segWidth = 2. * i_y0 ;
	
	float x0_offset = segWidth * segIndex;
	
	float x0_seg = i_x0 - x0_offset;
	float x0_prime = i_y0 / 2.;
	
	i0 = (dens_m*x0_seg - 2.*x0_prime) / (dens_m-2.) + x0_offset;
	i1 = (dens_m*x0_seg + 6.*x0_prime) / (dens_m+2.) + x0_offset;
	
	return true;
}

// this is a recurrence relation that iteratively computes intersections
// between the density line and the tree.
float nextDist( float prevDist )
{
	float y0_ = dens( prevDist );
	y0_ = pow( 2.0, floor(log2(y0_)) );
	#define SEGWIDTH (2.0*y0_)
	float segIndex = floor( prevDist / SEGWIDTH );
	
	for( int i = 0; i < 3; i++ )
	{
		float i0, i1;
		canonicalInt( dens_inv(y0_), y0_, segIndex, i0, i1 );
		
		float di = dens(i0);
		if( i0 > prevDist && di >= y0_ && di < 2.*y0_ )
		{
			return i0;
		}
		di = dens(i1);
		if( i1 > prevDist && di >= y0_ && di < 2.*y0_ )
		{
			return i1;
		}
		
		if( dens((segIndex+1.0)*SEGWIDTH) > 2.0 * y0_ )
		{
			// move up a level
			y0_ *= 2.;
			segIndex = floor(segIndex/2.);
		}
		else
		{
			// move to next segment
			segIndex += 1.0;
		}
	}
	
	// shouldnt get here
	return 0.;
}

// all this really should be on the CPU. this computes the sample distances which do not
// vary across pixels
#define SAMPLE_COUNT 32
float dists[SAMPLE_COUNT+1]; //+1 because we'll do forward differences to calc dt
#define MIN_DENS 0.125
#define DIST_ORIGIN dens_inv( MIN_DENS )
void populateDists()
{
	// generate a naiive view space sampling if the mouse button is down
	bool useNewApproach = true;
	if( iMouse.z > 0. )
		useNewApproach = false;
	
	dens_x = SPEED*iGlobalTime * (useNewApproach?1.:0.);
	
	float x_origin = DIST_ORIGIN;
	dists[0] = x_origin;
	
	for( int i = 1; i < SAMPLE_COUNT; i++ )
	{
		dists[i] = nextDist( dists[i-1] );
		// now that we are done with it, make it camera relative
		dists[i-1] -= x_origin;
	}
	
	// last elements
	dists[SAMPLE_COUNT-1] -= x_origin;
	dists[SAMPLE_COUNT] = dists[SAMPLE_COUNT-1];
}


#ifdef FULL_PROCEDURAL

// hash based 3d value noise
float hash( float n )
{
    return fract(sin(n)*43758.5453);
}
float noise( in vec3 x )
{
    vec3 p = floor(x);
    vec3 f = fract(x);

    f = f*f*(3.0-2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;
    return mix(mix(mix( hash(n+  0.0), hash(n+  1.0),f.x),
                   mix( hash(n+ 57.0), hash(n+ 58.0),f.x),f.y),
               mix(mix( hash(n+113.0), hash(n+114.0),f.x),
                   mix( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
}
#else

// LUT based 3d value noise
float noise( in vec3 x )
{
    vec3 p = floor(x);
    vec3 f = fract(x);
	f = f*f*(3.0-2.0*f);
	
	vec2 uv = (p.xy+vec2(37.0,17.0)*p.z) + f.xy;
	vec2 rg = texture2D( iChannel0, (uv+ 0.5)/256.0, -100.0 ).yx;
	return mix( rg.x, rg.y, f.z );
}
#endif

vec4 map( in vec3 p )
{
	float d = 0.2 - p.y;

	// NEW - im moving the camera instead of moving the volume function. note that
	// we want samples to stay stationary relative to the function - if we have  moving
	// volume we want to make the samples move with it if at all possible. motion
	// orthogonal into the camera direction is usually not a problem as the function is
	// densely sampled in this direction by the pixels. motion along the camera direction
	// is the problem as this is sparsely sampled.
	vec3 q = p;// - vec3(1.0,0.1,0.0)*iGlobalTime;
	float f;
    f  = 0.5000*noise( q ); q = q*2.02;
    f += 0.2500*noise( q ); q = q*2.03;
    f += 0.1250*noise( q ); q = q*2.01;
    f += 0.0625*noise( q );

	d += 3.0 * f;

	d = clamp( d, 0.0, 1.0 );
	
	vec4 res = vec4( d );

	res.xyz = mix( 1.15*vec3(1.0,0.95,0.8), vec3(0.7,0.7,0.7), res.x );
	
	return res;
}


vec3 sundir = normalize(vec3(-1.0,0.0,-1.));


vec4 raymarch( in vec3 ro, in vec3 rd )
{
	vec4 sum = vec4(0, 0, 0, 0);

	float x_origin = DIST_ORIGIN;
	float t = 0.0;
	for(int i=0; i<SAMPLE_COUNT; i++)
	{
		// NEW - read the ray march step distances from the generated sample locations
		t = dists[i];
		
		if( sum.a > 0.99 ) continue;

		vec3 pos = ro + t*rd;
		vec4 col = map( pos );
		
		#if 1
		float dif =  clamp((col.w - map(pos+0.3*sundir).w)/0.6, 0.0, 1.0 );

        vec3 lin = vec3(0.65,0.68,0.7)*1.35 + 0.45*vec3(0.7, 0.5, 0.3)*dif;
		col.xyz *= lin;
		#endif
		
		col.a *= 0.35;
		col.rgb *= col.a;

		// NEW - integrate samples along ray
		float dt = dists[i+1]-dists[i];
		// hack - redistribute cloud densities to make more opaque in foreground and
		// softer in distance. TODO - am i using the right formula for integration?
		dt = sqrt( dt/5. )*5.;
		
		sum = sum + dt * col*(1.0 - sum.a);	
	}

	sum.xyz /= (0.001+sum.w);

	return clamp( sum, 0.0, 1.0 );
}

void main(void)
{
	// this would be done on the CPU and passed in!!
	populateDists();
	
	vec2 q = gl_FragCoord.xy / iResolution.xy;
    vec2 p = -1.0 + 2.0*q;
    p.x *= iResolution.x/ iResolution.y;
    vec2 mo = -1.0 + 2.0*iMouse.xy / iResolution.xy;
   
	// NEW - since i couldnt integrate the camera forward motion (see initial comments),
	// i had to simplify the motion to make it move along one axis in a simple and
	// predictable way, and i lost iqs nice camera motion and framing along the way.
	
    // camera
    vec3 ro = vec3(0.-SPEED*iGlobalTime,1.9,0.);//4.0*normalize(vec3(cos(2.75-3.0*mo.x), 0.7+(mo.y+1.0), sin(2.75-3.0*mo.x)));
	vec3 ta = vec3(ro.x-1., ro.y, ro.z);
    vec3 ww = normalize( ta - ro);
    vec3 uu = normalize(cross( vec3(0.0,1.0,0.0), ww ));
    vec3 vv = normalize(cross(ww,uu));
    vec3 rd = normalize( p.x*uu + 1.2*p.y*vv + 1.5*ww );

	
    vec4 res = raymarch( ro, rd );

	float sun = clamp( dot(sundir,rd), 0.0, 1.0 );
	vec3 col = vec3(0.6,0.71,0.75) - rd.y*0.2*vec3(1.0,0.5,1.0) + 0.15*0.5;
	col += 0.2*vec3(1.0,.6,0.1)*pow( sun, 8.0 );
	col *= 0.95;
	col = mix( col, res.xyz, res.w );
	col += 0.1*vec3(1.0,0.4,0.2)*pow( sun, 3.0 );
	    
    gl_FragColor = vec4( col, 1.0 );
}

/* Binary tree
// 
// // This is an approach to generate sample locations at an adaptive sampling rate.
// // The desired sample density is given by the red line, which is then intersected
// // with the binary tree to obtain the samples.
// 
// // For a full description see the "Oceans on a shoestring" talk from the advances
// // course at SIGGRAPH 2013: http://advances.realtimerendering.com/s2013/
// // Direct link: http://advances.realtimerendering.com/s2013/OceanShoestring_SIGGRAPH2013_Online.pptx
// 
// // Its doubtful that one would do this in a shader in practice, although their might
// // be applications. This is more a demonstration of the concept;
// 
// // This shader is based on TinyGrapher: https://www.shadertoy.com/view/Msl3Ws
// 
// // simple macros to pull mouse position in [0,1]
// #define MOUSEX	iMouse.x/iResolution.x
// 
// #define SAMPLE_COUNT 32
// #define PARAMETRIC_STEPS 32
// 
// // the gradient of the desired density line
// float dens_m = .2;
// 
// // the density line
// float dens( float x )
// {
// 	float x0 = 18.*(MOUSEX-.2);
// 	return dens_m * (x - x0) ;//+ y0;
// }
// float dens_inv( float y )
// {
// 	float x0 = 18.*(MOUSEX-.2);
// 	return (y /*- y0*/) / dens_m + x0;
// }
// bool fn1( float x, out float y, out vec4 col )
// {
// 	col = vec4(1.,0.,0.,1.);
// 	
// 	y = dens( x );
// 	
// 	return true;
// }
// 
// // visualise two tree levels
// float inten = .4;
// float visy0 = .5;
// bool fn2( float x, out float y, out vec4 col )
// {
// 	x = mod( x, 2.*visy0 );
// 	float x0_prime = visy0 / 2.;
// 	y = 2. * ( x - x0_prime ) + visy0;
// 	
// 	col = vec4(inten);
// 
// 	if( y < visy0 || y > visy0*2. )
// 		col.a = 0.;
// 	
// 	return true;
// }
// bool fn3( float x, out float y, out vec4 col )
// {
// 	x = mod( x, 2.*visy0 );
// 	float x0_prime = visy0 / 2.;
// 	y = -2. * ( x - 3.*x0_prime ) + visy0;
// 	
// 	col = vec4(inten);
// 
// 	if( y < visy0 || y > visy0*2. )
// 		col.a = 0.;
// 	
// 	return true;
// }
// float visy02 = 1.;
// bool fn4( float x, out float y, out vec4 col )
// {
// 	x = mod( x, 2.*visy02 );
// 	float x0_prime = visy02 / 2.;
// 	y = 2. * ( x - x0_prime ) + visy02;
// 	
// 	col = vec4(inten);
// 
// 	if( y < visy02 || y > visy02*2. )
// 		col.a = 0.;
// 	
// 	return true;
// }
// bool fn5( float x, out float y, out vec4 col )
// {
// 	x = mod( x, 2.*visy02 );
// 	float x0_prime = visy02 / 2.;
// 	y = -2. * ( x - 3.*x0_prime ) + visy02;
// 	
// 	col = vec4(inten);
// 
// 	if( y < visy02 || y > visy02*2. )
// 		col.a = 0.;
// 	
// 	return true;
// }
// 
// // intersect the line with two branches between a parent and its children
// bool canonicalInt( float i_x0, float i_y0, float segIndex, out float i0, out float i1 )
// {
// 	float segWidth = 2. * i_y0 ;
// 	
// 	float x0_offset = segWidth * segIndex;
// 	
// 	float x0_seg = i_x0 - x0_offset;
// 	float x0_prime = i_y0 / 2.;
// 	
// 	i0 = (dens_m*x0_seg - 2.*x0_prime) / (dens_m-2.) + x0_offset;
// 	i1 = (dens_m*x0_seg + 6.*x0_prime) / (dens_m+2.) + x0_offset;
// 	
// 	return true;
// }
// 
// // given a distance, generates the next one. i had to use this recurrence approach
// // because I couldnt easily just loop through the tree due to webgl constraints
// float nextDist( float prevDist )
// {
// 	float y0_ = dens( prevDist );
// 	y0_ = pow( 2.0, floor(log2(y0_)) );
// 	#define SEGWIDTH (2.0*y0_)
// 	float segIndex = floor( prevDist / SEGWIDTH );
// 	
// 	// this is the max number of steps required to get to the next intersection
// 	for( int i = 0; i < 3; i++ )
// 	{
// 		float i0, i1;
// 		canonicalInt( dens_inv(y0_), y0_, segIndex, i0, i1 );
// 		
// 		float di = dens(i0);
// 		if( i0 > prevDist && di >= y0_ && di < 2.*y0_ )
// 			return i0;
// 		di = dens(i1);
// 		if( i1 > prevDist && di >= y0_ && di < 2.*y0_ )
// 			return i1;
// 		
// 		if( dens((segIndex+1.0)*SEGWIDTH) > 2.0 * y0_ )
// 		{
// 			// move up a level
// 			y0_ *= 2.;
// 			segIndex = floor(segIndex/2.);
// 		}
// 		else
// 		{
// 			// move to next segment (right)
// 			segIndex += 1.0;
// 		}
// 	}
// 	return 4.;
// }
// 
// // these are the sample dists that will be generated. it makes much more sense
// // to generate this on the CPU and pass it in.
// float dists[SAMPLE_COUNT];
// void populateDists()
// {
// 	float minDens = .125;
// 	dists[0] = dens_inv( minDens );
// 	
// 	for( int i = 1; i < SAMPLE_COUNT; i++ )
// 	{
// 		dists[i] = nextDist( dists[i-1] );
// 	}
// }
// 
// // this parametric function places a white dot at each intersection
// bool pfn1( float t, out float x, out float y, out vec4 col, out float mint, out float maxt )
// {
// 	col = vec4(1.);
// 	mint = 0.;
// 	maxt = 1.;
// 	
// 	float mindens = 0.125;
// 	x = dens_inv( mindens );
// 	
// 	int thisPt = int(floor(t*float(SAMPLE_COUNT)));
// 	for( int i = 0; i < SAMPLE_COUNT; i++ )
// 	{
// 		if( i == thisPt )
// 		{
// 			x = dists[i];
// 			break;
// 		}
// 	}
// 	
// 	vec4 col_dummy;
// 	fn1(x,y,col_dummy);
// 	
// 	return true;
// }
// 
// vec4 graph( vec2 p, float xmin, float xmax, float ymin, float ymax, float width );
// 
// void main(void)
// {
// 	populateDists();
// 	
//     vec2 uv = gl_FragCoord.xy / iResolution.xy;
//  
// 	gl_FragColor = graph( uv, 0., 6./*displaySegs*y0*2.*/, 0./*y0*/, 4./*y0*2.*/, .0225 );
// 
// 	return;
// }
// 
// 
// float drawNumber( float num, vec2 pos, vec2 pixel_coords );
// 
// // p is in [0,1]. 
// vec4 graph( vec2 p, float xmin, float xmax, float ymin, float ymax, float width )
// {
// 	vec4 result = vec4(0.1);
// 	
// 	float thisx = xmin + (xmax-xmin)*p.x;
// 	float thisy = ymin + (ymax-ymin)*p.y;
// 	
// 	// compute gradient between this pixel and next (seems reasonable)
// 	float eps = dFdx(thisx);
// 
// 	float alpha;
// 	
// 	vec4 axisCol = vec4(vec3(.3),1.);
// 	
// 	// axes
// 	// x
// 	alpha = abs( thisy - 0. ); alpha = smoothstep( width, width/4., alpha );
// 	result = (1.-alpha)*result + alpha*axisCol;
// 	// y
// 	alpha = abs( thisx - 0. ); alpha = smoothstep( width, width/4., alpha );
// 	result = (1.-alpha)*result + alpha*axisCol;
// 	
// 	// uses iq's awesome distance to implicit http://www.iquilezles.org/www/articles/distance/distance.htm
// 	float f;
// 	vec4 fcol;
// 	if( fn1( thisx, f, fcol ) )
// 	{
// 		float f_1; fn1( thisx + eps, f_1, fcol ); float f_prime = (f_1 - f) / eps;
// 		alpha = abs(thisy - f)/sqrt(1.+f_prime*f_prime); alpha = smoothstep( width, width/4., alpha ); alpha *= fcol.a;
// 		result = (1.-alpha)*result + alpha*fcol;
// 	}
// 	if( fn2( thisx, f, fcol ) )
// 	{
// 		float f_1; fn2( thisx + eps, f_1, fcol ); float f_prime = (f_1 - f) / eps;
// 		alpha = abs(thisy - f)/sqrt(1.+f_prime*f_prime); alpha = smoothstep( width, width/4., alpha ); alpha *= fcol.a;
// 		result = (1.-alpha)*result + alpha*fcol;
// 	}
// 	if( fn3( thisx, f, fcol ) )
// 	{
// 		float f_1; fn3( thisx + eps, f_1, fcol ); float f_prime = (f_1 - f) / eps;
// 		alpha = abs(thisy - f)/sqrt(1.+f_prime*f_prime); alpha = smoothstep( width, width/4., alpha ); alpha *= fcol.a;
// 		result = (1.-alpha)*result + alpha*fcol;
// 	}
// 	if( fn4( thisx, f, fcol ) )
// 	{
// 		float f_1; fn4( thisx + eps, f_1, fcol ); float f_prime = (f_1 - f) / eps;
// 		alpha = abs(thisy - f)/sqrt(1.+f_prime*f_prime); alpha = smoothstep( width, width/4., alpha ); alpha *= fcol.a;
// 		result = (1.-alpha)*result + alpha*fcol;
// 	}
// 	if( fn5( thisx, f, fcol ) )
// 	{
// 		float f_1; fn5( thisx + eps, f_1, fcol ); float f_prime = (f_1 - f) / eps;
// 		alpha = abs(thisy - f)/sqrt(1.+f_prime*f_prime); alpha = smoothstep( width, width/4., alpha ); alpha *= fcol.a;
// 		result = (1.-alpha)*result + alpha*fcol;
// 	}
// 	
// 	// parametric curves. todo - join the dots!
// 	float x, mint, maxt;
// 	if( pfn1( 0., x, f, fcol, mint, maxt ) )
// 	{
// 		float dt = (maxt-mint)/float(PARAMETRIC_STEPS);
// 		float t = mint;
// 		for( int i = 0; i <= PARAMETRIC_STEPS; i++ )
// 		{
// 			pfn1( t, x, f, fcol, mint, maxt );
// 			alpha = length(vec2(x,f)-vec2(thisx,thisy));
// 			alpha = smoothstep( width, width/4., alpha ); alpha *= fcol.a;
// 			result = (1.-alpha)*result + alpha*fcol;
// 			t += dt;
// 		}
// 	}
// 	
// 	result += vec4(drawNumber(xmin, vec2(0.,0.)+vec2(1.)/iResolution.xy, p ));
// 	result += vec4(drawNumber(xmax, vec2(1.,0.)+vec2(-26.,1.)/iResolution.xy, p ));
// 	result += vec4(drawNumber(ymax, vec2(0.,1.)+vec2(1.,-7.)/iResolution.xy, p ));
// 	result += vec4(drawNumber(ymin, vec2(0.,0.)+vec2(1.,10.)/iResolution.xy, p ));
// 	
// 	return result;
// }
// 
// // digits based on the nice ascii shader by movAX13h
// 
// float drawDig( vec2 pos, vec2 pixel_coords, float bitfield )
// {
// 	// offset relative to 
// 	vec2 ic = pixel_coords - pos ;
// 	ic = floor(ic*iResolution.xy);
// 	// test if overlap letter
// 	if( clamp(ic.x, 0., 2.) == ic.x && clamp(ic.y, 0., 4.) == ic.y )
// 	{
// 		// compute 1d bitindex from 2d pos
// 		float bitIndex = ic.y*3.+ic.x;
// 		// isolate the bit
// 		return floor( mod( bitfield / exp2( floor(bitIndex) ), 2. ) );
// 	}
// 	return 0.;
// }
// // decimal point
// float drawDecPt( vec2 center, vec2 pixel_coords )
// {
// 	return drawDig( center, pixel_coords, 1. );
// }
// // minus sign
// float drawMinus( vec2 center, vec2 pixel_coords )
// {
// 	return drawDig( center, pixel_coords, 448. );
// }
// // digits 0 to 9
// float drawDigit( float dig, vec2 pos, vec2 pixel_coords )
// {
// 	if( dig == 1. )
// 		return drawDig( pos, pixel_coords, 18724. );
// 	if( dig == 2. )
// 		return drawDig( pos, pixel_coords, 31183. );
// 	if( dig == 3. )
// 		return drawDig( pos, pixel_coords, 31207. );
// 	if( dig == 4. )
// 		return drawDig( pos, pixel_coords, 23524. );
// 	if( dig == 5. )
// 		return drawDig( pos, pixel_coords, 29671. );
// 	if( dig == 6. )
// 		return drawDig( pos, pixel_coords, 29679. );
// 	if( dig == 7. )
// 		return drawDig( pos, pixel_coords, 31012. );
// 	if( dig == 8. )
// 		return drawDig( pos, pixel_coords, 31727. );
// 	if( dig == 9. )
// 		return drawDig( pos, pixel_coords, 31719. );
// 	// 0
// 	return drawDig( pos, pixel_coords, 31599. );
// }
// 
// // max num width is 26px (minus, 3 nums, dec pt, 2 nums)
// // max height is 6px
// float drawNumber( float num, vec2 pos, vec2 pixel_coords )
// {
// 	float result = 0.;
// 	bool on = false;
// 	float d;
// 	
// 	// minus sign
// 	if( num < 0. )
// 	{
// 		result += drawMinus( pos, pixel_coords );
// 		pos.x += 4. / iResolution.x;
// 		num = -num;
// 	}
// 	// hundreds
// 	d = floor(mod(num/100.,10.));
// 	if( on || d > 0. )
// 	{
// 		result += drawDigit( d, pos, pixel_coords );
// 		pos.x += 4. / iResolution.x;
// 		on = true;
// 	}
// 	// tens
// 	d = floor(mod(num/10.,10.));
// 	if( on || d > 0. )
// 	{
// 		result += drawDigit( d, pos, pixel_coords );
// 		pos.x += 4. / iResolution.x;
// 		on = true;
// 	}
// 	// ones
// 	d = floor(mod(num,10.));
// 	result += drawDigit( d, pos, pixel_coords );
// 	pos.x += 4. / iResolution.x;
// 	// dec pt
// 	result += drawDecPt( pos, pixel_coords );
// 	pos.x += 2. / iResolution.x;
// 	// tenths
// 	d = floor(mod(num/.1,10.));
// 	if( true )
// 	{
// 		result += drawDigit( d, pos, pixel_coords );
// 		pos.x += 4. / iResolution.x;
// 	}
// 	// hundredths
// 	d = floor(.5+mod(num/.01,10.));
// 	if( d > 0. )
// 	{
// 		result += drawDigit( d, pos, pixel_coords );
// 		pos.x += 4. / iResolution.x;
// 	}
// 	
// 	return clamp(result,0.,1.);
// }
// 
// vec3 hsv2rgb(vec3 c);
// vec3 rgb2hsv(vec3 c);
// 
// vec3 errorColour( float err, float maxerror )
// {
// 	err = 1. - err / maxerror;
// 	err *= 2. / 3.;
// 	return hsv2rgb( vec3(err, 1., 1.) );
// }
// 
// //http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
// 
// vec3 rgb2hsv(vec3 c)
// {
//     vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
//     vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
//     vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
// 
//     float d = q.x - min(q.w, q.y);
//     float e = 1.0e-10;
//     return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
// }
// 
// vec3 hsv2rgb(vec3 c)
// {
//     vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
//     vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
//     return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
// }

*/