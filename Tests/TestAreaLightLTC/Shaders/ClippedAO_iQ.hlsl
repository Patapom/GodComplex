// https://www.shadertoy.com/view/4sSXDV
// Created by inigo quilez - iq/2014
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.


// Analytical ambient occlusion of triangles. Left side of screen, stochastically 
// sampled occlusion. Right side of the screen, analytical solution (no rays casted).
//
// This shader computes proper clipping. With clipping, polygons can fall (completely 
// or partially) below the visibility horizon of the receiving point, while still 
// computing analytically correct occlusion.

//=====================================================

float sacos( float x ) { return acos( min(max(x,-1.0),1.0) ); }

vec3 clip( in vec3 a, in vec3 b, in vec4 p )
{
    return a - (b-a)*(p.w + dot(p.xyz,a))/dot(p.xyz,(b-a));
//    return ( a*dot(p.xyz,b) - b*dot(p.xyz,a)  - (b-a)*p.w ) / dot(p.xyz,(b-a));
}

//-----------------------------------------------------------------------------------------

// fully visible front acing Triangle occlusion
float ftriOcclusion( in vec3 pos, in vec3 nor, in vec3 v0, in vec3 v1, in vec3 v2 )
{
    vec3 a = normalize( v0 - pos );
    vec3 b = normalize( v1 - pos );
    vec3 c = normalize( v2 - pos );

    return (dot( nor, normalize( cross(a,b)) ) * sacos( dot(a,b) ) +
            dot( nor, normalize( cross(b,c)) ) * sacos( dot(b,c) ) +
            dot( nor, normalize( cross(c,a)) ) * sacos( dot(c,a) ) ) / 6.2831;
}


// fully visible front acing Quad occlusion
float fquadOcclusion( in vec3 pos, in vec3 nor, in vec3 v0, in vec3 v1, in vec3 v2, in vec3 v3 )
{
    vec3 a = normalize( v0 - pos );
    vec3 b = normalize( v1 - pos );
    vec3 c = normalize( v2 - pos );
    vec3 d = normalize( v3 - pos );
    
    return (dot( nor, normalize( cross(a,b)) ) * sacos( dot(a,b) ) +
            dot( nor, normalize( cross(b,c)) ) * sacos( dot(b,c) ) +
            dot( nor, normalize( cross(c,d)) ) * sacos( dot(c,d) ) +
            dot( nor, normalize( cross(d,a)) ) * sacos( dot(d,a) ) ) / 6.2831;
}

// partially or fully visible, front or back facing Triangle occlusion
float triOcclusion( in vec3 pos, in vec3 nor, in vec3 v0, in vec3 v1, in vec3 v2, in vec4 plane )
{
    if( dot( v0-pos, cross(v1-v0,v2-v0) ) < 0.0 ) return 0.0;  // back facing
    
    float s0 = dot( vec4(v0,1.0), plane );
    float s1 = dot( vec4(v1,1.0), plane );
    float s2 = dot( vec4(v2,1.0), plane );
    
    float sn = sign(s0) + sign(s1) + sign(s2);

    vec3 c0 = clip( v0, v1, plane );
    vec3 c1 = clip( v1, v2, plane );
    vec3 c2 = clip( v2, v0, plane );
    
    // 3 (all) vertices above horizon
    if( sn>2.0 )  
    {
        return ftriOcclusion(  pos, nor, v0, v1, v2 );
    }
    // 2 vertices above horizon
    else if( sn>0.0 ) 
    {
        vec3 pa, pb, pc, pd;
              if( s0<0.0 )  { pa = c0; pb = v1; pc = v2; pd = c2; }
        else  if( s1<0.0 )  { pa = c1; pb = v2; pc = v0; pd = c0; }
        else/*if( s2<0.0 )*/{ pa = c2; pb = v0; pc = v1; pd = c1; }
        return fquadOcclusion( pos, nor, pa, pb, pc, pd );
    }
    // 1 vertex aboce horizon
    else if( sn>-2.0 ) 
    {
        vec3 pa, pb, pc;
              if( s0>0.0 )   { pa = c2; pb = v0; pc = c0; }
        else  if( s1>0.0 )   { pa = c0; pb = v1; pc = c1; }
        else/*if( s2>0.0 )*/ { pa = c1; pb = v2; pc = c2; }
        return ftriOcclusion(  pos, nor, pa, pb, pc );
    }
    // zero (no) vertices above horizon
    
    return 0.0;
}


//-----------------------------------------------------------------------------------------


// Box occlusion (if fully visible)
float boxOcclusion( in vec3 pos, in vec3 nor, in mat4 txx, in mat4 txi, in vec3 rad ) 
{
	vec3 p = (txx*vec4(pos,1.0)).xyz;
	vec3 n = (txx*vec4(nor,0.0)).xyz;
    vec4 w = vec4( n, -dot(n,p) ); // clipping plane
    
    // 8 verts
    vec3 v0 = vec3(-1.0,-1.0,-1.0)*rad;
    vec3 v1 = vec3( 1.0,-1.0,-1.0)*rad;
    vec3 v2 = vec3(-1.0, 1.0,-1.0)*rad;
    vec3 v3 = vec3( 1.0, 1.0,-1.0)*rad;
    vec3 v4 = vec3(-1.0,-1.0, 1.0)*rad;
    vec3 v5 = vec3( 1.0,-1.0, 1.0)*rad;
    vec3 v6 = vec3(-1.0, 1.0, 1.0)*rad;
    vec3 v7 = vec3( 1.0, 1.0, 1.0)*rad;
    

    // 6 faces    
    float occ = 0.0;
    occ += triOcclusion( p, n, v0, v2, v3, w );
    occ += triOcclusion( p, n, v0, v3, v1, w );

    occ += triOcclusion( p, n, v4, v5, v7, w );
    occ += triOcclusion( p, n, v4, v7, v6, w );
    
    occ += triOcclusion( p, n, v5, v1, v3, w );
    occ += triOcclusion( p, n, v5, v3, v7, w );
    
    occ += triOcclusion( p, n, v0, v4, v6, w );
    occ += triOcclusion( p, n, v0, v6, v2, w );
    
    occ += triOcclusion( p, n, v6, v7, v3, w );
    occ += triOcclusion( p, n, v6, v3, v2, w );
    
    occ += triOcclusion( p, n, v0, v1, v5, w );
    occ += triOcclusion( p, n, v0, v5, v4, w );

    return occ;
}

//-----------------------------------------------------------------------------------------

// returns t and normal
vec4 boxIntersect( in vec3 ro, in vec3 rd, in mat4 txx, in mat4 txi, in vec3 rad ) 
{
    // convert from ray to box space
	vec3 rdd = (txx*vec4(rd,0.0)).xyz;
	vec3 roo = (txx*vec4(ro,1.0)).xyz;

	// ray-box intersection in box space
    vec3 m = 1.0/rdd;
    vec3 n = m*roo;
    vec3 k = abs(m)*rad;
	
    vec3 t1 = -n - k;
    vec3 t2 = -n + k;

	float tN = max( max( t1.x, t1.y ), t1.z );
	float tF = min( min( t2.x, t2.y ), t2.z );
	
	if( tN > tF || tF < 0.0) return vec4(-1.0);

	vec3 nor = -sign(rdd)*step(t1.yzx,t1.xyz)*step(t1.zxy,t1.xyz);

    // convert to ray space
	
	nor = (txi * vec4(nor,0.0)).xyz;

	return vec4( tN, nor );
}

mat4 rotationAxisAngle( vec3 v, float angle )
{
    float s = sin( angle );
    float c = cos( angle );
    float ic = 1.0 - c;

    return mat4( v.x*v.x*ic + c,     v.y*v.x*ic - s*v.z, v.z*v.x*ic + s*v.y, 0.0,
                 v.x*v.y*ic + s*v.z, v.y*v.y*ic + c,     v.z*v.y*ic - s*v.x, 0.0,
                 v.x*v.z*ic - s*v.y, v.y*v.z*ic + s*v.x, v.z*v.z*ic + c,     0.0,
			     0.0,                0.0,                0.0,                1.0 );
}

mat4 translate( float x, float y, float z )
{
    return mat4( 1.0, 0.0, 0.0, 0.0,
				 0.0, 1.0, 0.0, 0.0,
				 0.0, 0.0, 1.0, 0.0,
				 x,   y,   z,   1.0 );
}

mat4 inverse( in mat4 m )
{
	return mat4(
        m[0][0], m[1][0], m[2][0], 0.0,
        m[0][1], m[1][1], m[2][1], 0.0,
        m[0][2], m[1][2], m[2][2], 0.0,
        -dot(m[0].xyz,m[3].xyz),
        -dot(m[1].xyz,m[3].xyz),
        -dot(m[2].xyz,m[3].xyz),
        1.0 );
}


vec2 hash2( float n ) { return fract(sin(vec2(n,n+1.0))*vec2(43758.5453123,22578.1459123)); }

//-----------------------------------------------------------------------------------------

float iPlane( in vec3 ro, in vec3 rd )
{
    return (-1.0 - ro.y)/rd.y;
}

void main( void )
{
	vec2 p = (2.0*gl_FragCoord.xy-iResolution.xy) / iResolution.y;
    float s = (2.0*iMouse.x-iResolution.x) / iResolution.y;
    if( iMouse.z<0.001 ) s=0.0;

	vec3 ro = vec3(0.0, 0.0, 4.0 );
	vec3 rd = normalize( vec3(p,-2.0) );
	
    // box animation
	mat4 rot = rotationAxisAngle( normalize(vec3(1.0,0.9,0.5)), 0.5*iGlobalTime );
	mat4 tra = translate( 0.0, 0.0, 0.0 );
	mat4 txi = tra * rot; 
	mat4 txx = inverse( txi );
	vec3 box = vec3(0.2,0.7,2.0) ;

    vec4 rrr = texture2D( iChannel0, (gl_FragCoord.xy)/iChannelResolution[0].xy, -99.0  ).xzyw;

    vec3 col = vec3(0.0);

    float tmin = 1e10;
    
    float t1 = iPlane( ro, rd );
    if( t1>0.0 )
    {
        tmin = t1;
        vec3 pos = ro + tmin*rd;
        vec3 nor = vec3(0.0,1.0,0.0);
        float occ = 0.0;
        
        if( p.x > s )
        {
            occ = boxOcclusion( pos, nor, txx, txi, box );
        }
        else
        {
   		    vec3  ru  = normalize( cross( nor, vec3(0.0,1.0,1.0) ) );
		    vec3  rv  = normalize( cross( ru, nor ) );

            occ = 0.0;
            for( int i=0; i<256; i++ )
            {
                vec2  aa = hash2( rrr.x + float(i)*203.1 );
                float ra = sqrt(aa.y);
                float rx = ra*cos(6.2831*aa.x); 
                float ry = ra*sin(6.2831*aa.x);
                float rz = sqrt( 1.0-aa.y );
                vec3  dir = vec3( rx*ru + ry*rv + rz*nor );
                vec4 res = boxIntersect( pos, dir, txx, txi, box );
                occ += step(0.0,res.x);
            }
            occ /= 256.0;
        }

        col = vec3(1.2);
        col *= 1.0 - occ;
    }

    vec4 res = boxIntersect( ro, rd, txx, txi, box );
    float t2 = res.x;
    if( t2>0.0 && t2<tmin )
    {
        tmin = t2;
        float t = t2;
        vec3 pos = ro + t*rd;
        vec3 nor = res.yzw;
		col = vec3(1.4);//vec3(1.0,0.85,0.6);
        col *= 0.6 + 0.4*nor.y;
	}

	col *= exp( -0.05*tmin );

    float e = 2.0/iResolution.y;
    col *= smoothstep( 0.0, 2.0*e, abs(p.x-s) );
    
    gl_FragColor = vec4( col, 1.0 );
}