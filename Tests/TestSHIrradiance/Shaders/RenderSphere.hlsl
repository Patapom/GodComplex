#include "Global.hlsl"

#if 1

// Test Sphere Rendering
float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _size;
	float	aspectRatio = float(_size.x) / _size.y;
	float3	csView = float3( aspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;
	// Make view Z-up
	wsView = float3( wsView.x, -wsView.z, wsView.y );

	float3	wsPos = float3( _camera2World[3].x, -_camera2World[3].z, _camera2World[3].y );

	float3	filteredEnvironmentSH[9];
	FilterHanning( EnvironmentSH, filteredEnvironmentSH, _filterWindowSize );

	float3	color = 0.0;
	float	dist = IntersectSphere( wsPos, wsView, 0.0, 1.0 );
	if ( dist < 0.0 || dist > NO_HIT ) {
//		return (_flags & 0x100U) ? EvaluateSHIrradiance( wsView, filteredEnvironmentSH )
		return (_flags & 0x100U) ? EvaluateSHRadiance( wsView, filteredEnvironmentSH )
								 : SampleHDREnvironment( wsView );
	}

	// Regular rendering
	float3	wsHitPos = wsPos + dist * wsView;
	float3	wsNormal = wsHitPos;

//	color = wsNormal;
//	color = 0.01 * dist * _cosAO;

	float3	irradianceOFF = 2.0 * INVPI * acos( _cosAO ) * EvaluateSHIrradiance( wsNormal, filteredEnvironmentSH );
//	float3	irradianceOFF = EvaluateSH( wsNormal, filteredEnvironmentSH );
	float3	irradianceON = EvaluateSHIrradiance( wsNormal, _cosAO, filteredEnvironmentSH );

	switch ( _flags & 1 ) {
	case 0:	color = (_flags & 0x8U) ? irradianceON : irradianceOFF; break;
	case 1:
		const float	dU = 3.0 / _size.x;
		float	left = smoothstep( 0.5, 0.5-dU, UV.x );
		float	right = smoothstep( 0.5, 0.5+dU, UV.x );
		color = left * irradianceOFF + right * irradianceON;
		break;
	}

	return color;
	return wsView;
	return float3( UV, 0 );
}

#else

static const float3	TEST_POSITION = float3( 1.0, 0.0, 2.5 );
static const float3	TEST_POSITION2 = float3( -1.0, 0.0, 2.5 );

// Enter SH function to visualize below:
float SH_TEST( in float3 n ) {
	// Build a rotating cosine lobe
	float	lobeSH0[9];
	ClampedCosineLobe( d, _cosAO, lobeSH0 );
	float3	d = normalize( float3( cos( _time ), sin( _time ), 0*1 ) );

	// Build a fixed clamped cone along +X
	float	lobeSH1[9];
	ClampedCone( float3( 1, 0, 0 ), _cosAO, lobeSH1 );

	// Compute convolution
	float	lobeSH[9];
	SHProduct( lobeSH0, lobeSH1, lobeSH );

	// Estimate lobe in requested direction
	float SH[9];
	Ylm( n, SH );

	return lobeSH[0] * SH[0]
		 + lobeSH[1] * SH[1]
		 + lobeSH[2] * SH[2]
		 + lobeSH[3] * SH[3]
		 + lobeSH[4] * SH[4]
		 + lobeSH[5] * SH[5]
		 + lobeSH[6] * SH[6]
		 + lobeSH[7] * SH[7]
		 + lobeSH[8] * SH[8];
}

float SH_TEST2( in float3 n ) {
return 1.0;
//return saturate( n.z );			// Regular cosine lobe
//return n.z > _cosAO ? n.z : 0.0;	// Clamped cosine lobe
//return n.z > _cosAO ? 1.0 : 0.0;	// Clamped hemisphere

	return 0.5 * sqrt( INVPI );	// Regular SH00
}

// Ray-marching stuff...
#if 1
// IQ's Raymarching SH Renderer (modified a bit)

// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.


// Four bands of Spherical Harmonics functions (or atomic orbitals if you want). For reference and fun.

// antialias level (try 1, 2, 3, ...)
#define AA 1

//#define SHOW_SPHERES

//---------------------------------------------------------------------------------


// Constants, see here: http://en.wikipedia.org/wiki/Table_of_spherical_harmonics
#define k01 0.2820947918 // sqrt(  1/PI)/2
#define k02 0.4886025119 // sqrt(  3/PI)/2
#define k03 1.0925484306 // sqrt( 15/PI)/2
#define k04 0.3153915652 // sqrt(  5/PI)/4
#define k05 0.5462742153 // sqrt( 15/PI)/4
#define k06 0.5900435860 // sqrt( 70/PI)/8
#define k07 2.8906114210 // sqrt(105/PI)/2
#define k08 0.4570214810 // sqrt( 42/PI)/8
#define k09 0.3731763300 // sqrt(  7/PI)/4
#define k10 1.4453057110 // sqrt(105/PI)/4

// Y_l_m(s), where l is the band and m the range in [-l..l] 
float SH( in int l, in int m, in float3 s ) { 
	float3 n = s.zxy;
	
    //----------------------------------------------------------
    if( l==0 )          return  k01;
    //----------------------------------------------------------
	if( l==1 && m==-1 ) return -k02*n.y;
    if( l==1 && m== 0 ) return  k02*n.z;
    if( l==1 && m== 1 ) return -k02*n.x;
    //----------------------------------------------------------
	if( l==2 && m==-2 ) return  k03*n.x*n.y;
    if( l==2 && m==-1 ) return -k03*n.y*n.z;
    if( l==2 && m== 0 ) return  k04*(3.0*n.z*n.z-1.0);
    if( l==2 && m== 1 ) return -k03*n.x*n.z;
    if( l==2 && m== 2 ) return  k05*(n.x*n.x-n.y*n.y);
    //----------------------------------------------------------
    if( l==3 && m==-3 ) return -k06*n.y*(3.0*n.x*n.x-n.y*n.y);
    if( l==3 && m==-2 ) return  k07*n.z*n.y*n.x;
    if( l==3 && m==-1 ) return -k08*n.y*(5.0*n.z*n.z-1.0);
    if( l==3 && m== 0 ) return  k09*n.z*(5.0*n.z*n.z-3.0);
    if( l==3 && m== 1 ) return -k08*n.x*(5.0*n.z*n.z-1.0);
    if( l==3 && m== 2 ) return  k10*n.z*(n.x*n.x-n.y*n.y);
    if( l==3 && m== 3 ) return -k06*n.x*(n.x*n.x-3.0*n.y*n.y);
    //----------------------------------------------------------

	return 0.0;
}

// unrolled version of the above

float SH_0_0( in float3 n ) { return  k01; }

#if 0
float SH_1_0( in float3 n ) { return k02*n.y; }
float SH_1_1( in float3 n ) { return k02*n.z; }
float SH_1_2( in float3 n ) { return k02*n.x; }
float SH_2_0( in float3 n ) { return k03*n.x*n.y; }
float SH_2_1( in float3 n ) { return k03*n.y*n.z; }
float SH_2_2( in float3 n ) { return k04*(3.0*n.z*n.z-1.0); }
float SH_2_3( in float3 n ) { return k03*n.x*n.z; }
float SH_2_4( in float3 n ) { return k05*(n.x*n.x-n.y*n.y); }
#else
float SH_1_0( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[1]; }
float SH_1_1( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[2]; }
float SH_1_2( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[3]; }
float SH_2_0( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[4]; }
float SH_2_1( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[5]; }
float SH_2_2( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[6]; }
float SH_2_3( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[7]; }
float SH_2_4( in float3 n ) { float SH[9]; Ylm( n, SH ); return SH[8]; }
#endif

float SH_3_0( in float3 n ) { return k06*n.y*(3.0*n.x*n.x-n.y*n.y); }
float SH_3_1( in float3 n ) { return k07*n.z*n.y*n.x; }
float SH_3_2( in float3 n ) { return k08*n.y*(5.0*n.z*n.z-1.0); }
float SH_3_3( in float3 n ) { return k09*n.z*(5.0*n.z*n.z-3.0); }
float SH_3_4( in float3 n ) { return k08*n.x*(5.0*n.z*n.z-1.0); }
float SH_3_5( in float3 n ) { return k10*n.z*(n.x*n.x-n.y*n.y); }
float SH_3_6( in float3 n ) { return k06*n.x*(n.x*n.x-3.0*n.y*n.y); }

float3 map( in float3 p ) {

	p += TEST_POSITION;

	float3 testP = p - TEST_POSITION;
    float3 p00 = p - TEST_POSITION2;

	float3 p01 = p - float3(-1.25, 0.0, 1.0);
	float3 p02 = p - float3( 0.00, 0.0, 1.0);
	float3 p03 = p - float3( 1.25, 0.0, 1.0);
	float3 p04 = p - float3(-2.50, 0.0,-0.5);
	float3 p05 = p - float3(-1.25, 0.0,-0.5);
	float3 p06 = p - float3( 0.00, 0.0,-0.5);
	float3 p07 = p - float3( 1.25, 0.0,-0.5);
	float3 p08 = p - float3( 2.50, 0.0,-0.5);
	float3 p09 = p - float3(-3.75, 0.0,-2.0);
	float3 p10 = p - float3(-2.50, 0.0,-2.0);
	float3 p11 = p - float3(-1.25, 0.0,-2.0);
	float3 p12 = p - float3( 0.00, 0.0,-2.0);
	float3 p13 = p - float3( 1.25, 0.0,-2.0);
	float3 p14 = p - float3( 2.50, 0.0,-2.0);
	float3 p15 = p - float3( 3.75, 0.0,-2.0);
	
	float r, d; float3 n, s, res;
	
    #ifdef SHOW_SPHERES
	#define SHAPE (float3(d-0.35, -1.0+2.0*clamp(0.5 + 16.0*r,0.0,1.0),d))
	#else
	#define SHAPE (float3(d-abs(r), sign(r),d))
	#endif
	d=length(p00); n=p00/d; r = SH_TEST2( n ); s = SHAPE; res = s;
	d=length(p01); n=p01/d; r = SH_1_0( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p02); n=p02/d; r = SH_1_1( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p03); n=p03/d; r = SH_1_2( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p04); n=p04/d; r = SH_2_0( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p05); n=p05/d; r = SH_2_1( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p06); n=p06/d; r = SH_2_2( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p07); n=p07/d; r = SH_2_3( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p08); n=p08/d; r = SH_2_4( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p09); n=p09/d; r = SH_3_0( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p10); n=p10/d; r = SH_3_1( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p11); n=p11/d; r = SH_3_2( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p12); n=p12/d; r = SH_3_3( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p13); n=p13/d; r = SH_3_4( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p14); n=p14/d; r = SH_3_5( n ); s = SHAPE; if( s.x<res.x ) res=s;
	d=length(p15); n=p15/d; r = SH_3_6( n ); s = SHAPE; if( s.x<res.x ) res=s;
	
	d=length(testP); n=testP/d; r = SH_TEST( n ); s = SHAPE; if( s.x<res.x ) res=s;

	return float3( res.x, 0.5+0.5*res.y, res.z );
}

float3 intersect( in float3 ro, in float3 rd ) {
	float3 res = float3(1e10,-1.0, 1.0);

	float maxd = 10.0;
    float h = 1.0;
    float t = 0.0;
    float2  m = -1.0;
	[loop]
    for( int i=0; i<200; i++ ) {
        if( h<0.001||t>maxd ) break;
	    float3 res = map( ro+rd*t );
        h = res.x;
		m = res.yz;
        t += h*0.3;
    }
	if( t<maxd && t<res.x ) res=float3(t,m);

	return res;
}

float3 calcNormal( in float3 pos ) {
    float2 eps = float2( 0.001, 0.0 );

	return normalize( float3(
           map(pos+eps.xyy).x - map(pos-eps.xyy).x,
           map(pos+eps.yxy).x - map(pos-eps.yxy).x,
           map(pos+eps.yyx).x - map(pos-eps.yyx).x ) );
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _size;
	float	aspectRatio = float(_size.x) / _size.y;

    float3 tot = 0.0;
    for( int m=0; m<AA; m++ )
    for( int n=0; n<AA; n++ ) {        
//        float2 p = (-_size.xy + 2.0*(fragCoord.xy+float2(float(m),float(n))/float(AA))) / _size.y;

        float2 p = (-float2(_size.xy) + 2.0*(float2( _In.__Position.x, _size.y-_In.__Position.y )+float2( m, n ) / AA)) / _size.y;
//return float3( p, 0 );
        // camera
//        float	an = 0.314*_time - 10.0*_mouse.x/_size.x;
        float	an = 4.0*_mouse.x/_size.x;
        float3  ta = float3(1.0,2.5,0.0);
        float3  ro = ta + float3(6.0*sin(an),0.0,6.0*cos(an));

        // camera matrix
        float3 ww = normalize( ta - ro );
        float3 uu = normalize( cross(ww,float3(0.0,1.0,0.0) ) );
        float3 vv = normalize( cross(uu,ww));

ro = _camera2World[3].xyz;
uu = _camera2World[0].xyz;
vv = _camera2World[1].xyz;
ww = _camera2World[2].xyz;

        // create view ray
        float3 rd = normalize( p.x*uu + p.y*vv + 2.0*ww );


rd = float3( rd.x, -rd.z, rd.y );	// Make view Z-up
ro = float3( ro.x, -ro.z, ro.y );	// Make camera pos Z-up


        // background 
        float3 col = 0.3 * saturate( 1.0-length(p)*0.5 );

        // raymarch
        float3 tmat = intersect(ro,rd);
        if( tmat.y > -0.5 ) {
            // geometry
            float3 pos = ro + tmat.x*rd;
            float3 nor = calcNormal(pos);
            float3 ref = reflect( rd, nor );

            // material		
            float3 mate = 0.5*lerp( float3(1.0,0.6,0.15), float3(0.2,0.4,0.5), tmat.y );

            float occ = clamp( 2.0*tmat.z, 0.0, 1.0 );
            float sss = pow( clamp( 1.0 + dot(nor,rd), 0.0, 1.0 ), 1.0 );

            // lights
            float3 lin  = 2.5*occ*float3(1.0,1.00,1.00)*(0.6+0.4*nor.y);
                 lin += 1.0*sss*float3(1.0,0.95,0.70)*occ;		

            // surface-light interacion
            col = mate.xyz * lin;
        }
        tot += col;
    }
    tot /= float(AA*AA) * _luminanceFactor;;
    return tot;
}
#endif

#endif
