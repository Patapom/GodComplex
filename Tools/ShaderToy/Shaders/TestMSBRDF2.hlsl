// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

// A list of usefull distance function to simple primitives, and an example on how to 
// do some interesting boolean operations, repetition and displacement.
//
// More info here: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

cbuffer CB_Main : register(b0) {
	float3	iResolution;	// viewport resolution (in pixels)
	float	iGlobalTime;	// shader playback time (in seconds)

	float2	_OnSenFout0;
	float2	_OnSenFout1;
	float2	_OnSenFout2;
	float2	_OnSenFout3;
	float2	_OnSenFout4;

	uint	_OnSenFout5;
	float	_DebugPlaneHeight;
	uint	_DebugFlags;
	float	_DebugParm;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Proj2World;
	float4x4	_World2Proj;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;
};

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}



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

//----------------------------------------------------------------------

float2 map( in float3 pos )
{
    float2 res = opU( float2( sdPlane(     pos), 1.0 ),
	                float2( sdSphere(    pos-float3( 0.0,0.25, 0.0), 0.25 ), 46.9 ) );
    res = opU( res, float2( sdBox(       pos-float3( 1.0,0.25, 0.0), 0.25 ), 3.0 ) );
    res = opU( res, float2( udRoundBox(  pos-float3( 1.0,0.25, 1.0), 0.15, 0.1 ), 41.0 ) );
	res = opU( res, float2( sdTorus(     pos-float3( 0.0,0.25, 1.0), float2(0.20,0.05) ), 25.0 ) );
    res = opU( res, float2( sdCapsule(   pos,float3(-1.3,0.10,-0.1), float3(-0.8,0.50,0.2), 0.1  ), 31.9 ) );
	res = opU( res, float2( sdTriPrism(  pos-float3(-1.0,0.25,-1.0), float2(0.25,0.05) ),43.5 ) );
	res = opU( res, float2( sdCylinder(  pos-float3( 1.0,0.30,-1.0), float2(0.1,0.2) ), 8.0 ) );
	res = opU( res, float2( sdCone(      pos-float3( 0.0,0.50,-1.0), float3(0.8,0.6,0.3) ), 55.0 ) );
	res = opU( res, float2( sdTorus82(   pos-float3( 0.0,0.25, 2.0), float2(0.20,0.05) ),50.0 ) );
	res = opU( res, float2( sdTorus88(   pos-float3(-1.0,0.25, 2.0), float2(0.20,0.05) ),43.0 ) );
	res = opU( res, float2( sdCylinder6( pos-float3( 1.0,0.30, 2.0), float2(0.1,0.2) ), 12.0 ) );
	res = opU( res, float2( sdHexPrism(  pos-float3(-1.0,0.20, 1.0), float2(0.25,0.05) ),17.0 ) );

    res = opU( res, float2( opS(
		             udRoundBox(  pos-float3(-2.0,0.2, 1.0), 0.15,0.05),
	                 sdSphere(    pos-float3(-2.0,0.2, 1.0), 0.25)), 13.0 ) );

    res = opU( res, float2( opS(
		             sdTorus82(	pos-float3(-2.0,0.2, 0.0), float2(0.20,0.1)),
	                 sdCylinder(  opRep( float3(
											(3.14 + atan2(pos.x+2.0,pos.z))/6.2831,
											pos.y,
											0.02+0.5*length(pos-float3(-2.0,0.2, 0.0))
											),
								  float3(0.05,1.0,0.05)), float2(0.02,0.6))), 51.0 ) );

	res = opU( res, float2( 0.7*sdSphere(    pos-float3(-2.0,0.25,-1.0), 0.2 ) + 
										     0.03*sin(50.0*pos.x)*sin(50.0*pos.y)*sin(50.0*pos.z), 
											65.0 ) );

	res = opU( res, float2( 0.5*sdTorus( opTwist(pos-float3(-2.0,0.25, 2.0)),float2(0.20,0.05)), 46.7 ) );

    res = opU( res, float2( sdConeSection( pos-float3( 0.0,0.35,-2.0), 0.15, 0.2, 0.1 ), 13.67 ) );

    res = opU( res, float2( sdEllipsoid( pos-float3( 1.0,0.35,-2.0), float3(0.15, 0.2, 0.05) ), 43.17 ) );
        
    return res;
}

float2 castRay( in float3 ro, in float3 rd )
{
    float tmin = 1.0;
    float tmax = 20.0;
    
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
    for( int i=0; i<50; i++ )
    {
	    float2 res = map( ro+rd*t );
        if( res.x<precis || t>tmax ) break;
        t += res.x;
	    m = res.y;
    }

    if( t>tmax ) m=-1.0;
    return float2( t, m );
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

float3 calcNormal( in float3 pos )
{
	float3 eps = float3( 0.001, 0.0, 0.0 );
	float3 nor = float3(
	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
	return normalize(nor);
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
    return clamp( 1.0 - 3.0*occ, 0.0, 1.0 );    
}




float3 render( in float3 ro, in float3 rd ) { 
	float3 col = float3(0.7, 0.9, 1.0) +rd.y*0.8;
	float2 res = castRay(ro,rd);
	float t = res.x;
	float m = res.y;
	if( m>-0.5 )
	{
		float3 pos = ro + t*rd;
		float3 nor = calcNormal( pos );
		float3 ref = reflect( rd, nor );
        
		// material        
		col = 0.45 + 0.3*sin( float3(0.05,0.08,0.10)*(m-1.0) );
		
		if( m<1.5 ) {
			float f = fmod( floor(5.0*pos.z) + floor(5.0*pos.x), 2.0 );
			col = 0.4 + 0.1*f*float3(1.0,1,1);
		}

		// lighitng        
		float occ = calcAO( pos, nor );
		float3  lig = normalize( float3(-0.6, 0.7, -0.5) );
		float amb = clamp( 0.5+0.5*nor.y, 0.0, 1.0 );
		float dif = clamp( dot( nor, lig ), 0.0, 1.0 );
		float bac = clamp( dot( nor, normalize(float3(-lig.x,0.0,-lig.z))), 0.0, 1.0 )*clamp( 1.0-pos.y,0.0,1.0);
		float dom = smoothstep( -0.1, 0.1, ref.y );
		float fre = pow( clamp(1.0+dot(nor,rd),0.0,1.0), 2.0 );
		float spe = pow(clamp( dot( ref, lig ), 0.0, 1.0 ),16.0);
        
		dif *= softshadow( pos, lig, 0.02, 2.5 );
		dom *= softshadow( pos, ref, 0.02, 2.5 );

		float3 lin = float3(0,0,0);
		lin += 1.20*dif*float3(1.00,0.85,0.55);
		lin += 1.20*spe*float3(1.00,0.85,0.55)*dif;
		lin += 0.20*amb*float3(0.50,0.70,1.00)*occ;
		lin += 0.30*dom*float3(0.50,0.70,1.00)*occ;
		lin += 0.30*bac*float3(0.25,0.25,0.25)*occ;
		lin += 0.40*fre*float3(1.00,1.00,1.00)*occ;
		col = col*lin;

		col = lerp( col, float3(0.8,0.9,1.0), 1.0-exp( -0.002*t*t ) );
	}

	return saturate( col );
}

float3x3 setCamera( in float3 ro, in float3 ta, float cr )
{
	float3 cw = normalize(ta-ro);
	float3 cp = float3(sin(cr), cos(cr),0.0);
	float3 cu = normalize( cross(cw,cp) );
	float3 cv = normalize( cross(cu,cw) );
    return float3x3( cu, cv, cw );
}

float3 PS( VS_IN _In ) : SV_TARGET0 {

	float2	fragCoord = _In.__Position.xy;
	float2	q = fragCoord.xy / iResolution.xy;
	float2	p = -1.0+2.0*q;
			p.x *= iResolution.x / iResolution.y;

	float2 mo = 0;//iMouse.xy / iResolution.xy;
		 
	float time = 15.0 + iGlobalTime;

	// camera	
	float3 ro = float3( -0.5+3.5*cos(0.1*time + 6.0*mo.x), 1.0 + 2.0*mo.y, 0.5 + 3.5*sin(0.1*time + 6.0*mo.x) );
	float3 ta = float3( -0.5, -0.4, 0.5 );
	
	// camera-to-world transformation
	float3x3 ca = setCamera( ro, ta, 0.0 );
    
	// ray direction
	float3	rd = mul( normalize( float3(p.x, -p.y, 2.0) ), ca );

	// render	
	float3 col = render( ro, rd );
	return pow( col, 0.4545 );
}