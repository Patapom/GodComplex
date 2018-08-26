///////////////////////////////////////////////////////////////////////////////////
// Shamelessly stolen from https://www.shadertoy.com/view/XlfBR7
//
// Raymarching sketch inspired by the work of Marc-Antoine Mathieu
// Leon 2017-11-21
// using code from IQ, Mercury, LJ, Duke, Koltes
///////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

static const float	SHADOW_BIAS = 1e-3;


#define MAT_GROUND				1
#define MAT_WALL				2
#define MAT_WINDOW_HORIZONTAL	3
#define MAT_WINDOW_VERTICAL		4
#define MAT_BOX					5

// tweak it
#define donut 30.
#define cell 4.
#define height 2.
#define thin .04
#define radius 15.
#define speed 1.

#define VOLUME 0.001

float glmod(float x, float y) {
	return x - y * floor(x/y);
//	return x - y * trunc(x/y);
}
float2 glmod(float2 x, float2 y) {
	return x - y * floor(x/y);
}
float3 glmod(float3 x, float3 y) {
	return x - y * floor(x/y);
}

void	smin( inout float2 _d0, float _d1, float _mat ) {
	_d0 = _d0.x <= _d1 ? _d0 : float2( _d1, _mat );
}
void	smax( inout float2 _d0, float _d1, float _mat ) {
	_d0 = _d0.x >= _d1 ? _d0 : float2( _d1, _mat );
}

// raymarching toolbox
float rng (float2 seed) { return frac(sin(dot(seed*.1684,float2(54.649,321.547)))*450315.); }
float2x2 rot (float a) { float c=cos(a),s=sin(a); return float2x2(c,-s,s,c); }
float sdSphere (float3 p, float r) { return length(p)-r; }
float sdCylinder (float2 p, float r) { return length(p)-r; }
float sdDisk (float3 p, float3 s) { return max(max(length(p.xz)-s.x, s.y), abs(p.y)-s.z); }
float sdIso(float3 p, float r) { return max(0.,dot(p,normalize(sign(p))))-r; }
float sdBox( float3 p, float3 b ) { float3 d = abs(p) - b; return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0)); }
float sdTorus( float3 p, float2 t ) { float2 q = float2(length(p.xz)-t.x,p.y); return length(q)-t.y; }
float amod (inout float2 p, float count) { float an = 2.*PI/count; float a = atan2(p.y,p.x)+an/2.; float c = floor(a/an); c = lerp(c,abs(c),step(count*.5,abs(c))); a = glmod(a,an)-an/2.; p.xy = float2(cos(a),sin(a))*length(p); return c; }
float amodIndex (float2 p, float count) { float an = 2.*PI/count; float a = atan2(p.y,p.x)+an/2.; float c = floor(a/an); c = lerp(c,abs(c),step(count*.5,abs(c))); return c; }
float repeat (float v, float c) { return glmod(v,c)-c/2.; }
float2 repeat (float2 v, float2 c) { return glmod(v,c)-c/2.; }
float3 repeat (float3 v, float c) { return glmod(v,c)-c/2.; }
float smoo (float a, float b, float r) { return clamp(.5+.5*(b-a)/r, 0., 1.); }
//float smin(float a, float b, float r) { float h = smoo(a,b,r); return lerp(b,a,h)-r*h*(1.-h); }
//float smax(float a, float b, float r) { float h = smoo(a,b,r); return lerp(a,b,h)+r*h*(1.-h); }
float2 displaceLoop (float2 p, float r) { return float2(length(p.xy)-r, atan2(p.y,p.x)); }
float2 map( float3 p );
float getShadow (float3 pos, float3 at, float k) {
    float3 dir = normalize(at - pos);
    float maxt = length(at - pos);
    float f = 01.;
    float t = VOLUME*50.;
    for (float i = 0.; i <= 1.; i += 1./15.) {
        float dist = map(pos + dir * t).x;
        if (dist < VOLUME) return 0.;
        f = min(f, k * dist / t);
        t += dist;
        if (t >= maxt) break;
    }
    return f;
}
float3 getNormal (float3 p) { float2 e = float2(.01,0); return normalize(float3(map(p+e.xyy).x-map(p-e.xyy).x,map(p+e.yxy).x-map(p-e.yxy).x,map(p+e.yyx).x-map(p-e.yyx).x)); }

void camera (inout float3 p) {
    p.xz = mul( p.xz, rot(PI/8.) );
    p.yz = mul( p.yz, rot(PI/6.) );
}

float windowCross (float3 pos, float4 size, float salt) {
	float3 p = pos;
	float sx = size.x * (.6+salt*.4);
	float sy = size.y * (.3+salt*.7);
	float2 sxy = float2(sx,sy);
	p.xy = repeat(p.xy+sxy/2., sxy);
	float scene = sdBox(p, size.zyw*2.);
	scene = min(scene, sdBox(p, size.xzw*2.));
	scene = max(scene, sdBox(pos, size.xyw));
	return scene;
}

float window (float3 pos, float2 dimension, float salt) {
	float thinn = .008;
	float depth = .04;
	float depthCadre = .06;
	float padding = .08;
	float scene = windowCross(pos, float4(dimension,thinn,depth), salt);
	float cadre = sdBox(pos, float3(dimension, depthCadre));
	cadre = max(cadre, -sdBox(pos, float3(dimension - padding, depthCadre*2.)));
	return min( scene, cadre );
}

float boxes (float3 pos, float salt) {
	float3 p = pos;
	float ry = cell * .43*(.3+salt);
	float rz = cell * .2*(.5+salt);
	float salty = rng(float2(floor(pos.y/ry), floor(pos.z/rz)));
	pos.y = repeat(pos.y, ry);
	pos.z = repeat(pos.z, rz);
	float scene = sdBox(pos, float3(.1+.8*salt+salty,.1+.2*salt,.1+.2*salty));
	return max(scene, sdBox(p, cell*.2));
}

float2	map (float3 pos) {
	float3 camOffset = float3(-4,0,0.);
	
	float2	scene = float2( 1000., -1 );
	float3	p = pos + camOffset;
	float	segments = PI*radius;
	float	indexX, indexY, salt;
	float2	seed;
	
	// donut distortion
	float3 pDonut = p;
	pDonut.x += donut;
	pDonut.y += radius;
	pDonut.xz = displaceLoop(pDonut.xz, donut);
	pDonut.z *= donut;
	pDonut.xzy = pDonut.xyz;
	pDonut.xz = mul( pDonut.xz, rot(_time*.05*speed) );
	
	// ground
	p = pDonut;
	smin( scene, sdCylinder(p.xz, radius-height), MAT_GROUND );
	
	// walls
	p = pDonut;
	float py = p.y + _time * speed;
	indexY = floor(py / (cell+thin));
	p.y = repeat(py, cell+thin);
	smin( scene, max(abs(p.y)-thin, sdCylinder(p.xz, radius)), MAT_WALL );
	amod(p.xz, segments);
	p.x -= radius;
	smin( scene, max(abs(p.z)-thin, p.x), MAT_WALL );
	
	// horizontal window
	p = pDonut;
	p.xz = mul( p.xz , rot(PI/segments) );
	py = p.y + _time * speed;
	indexY = floor(py / (cell+thin));
	p.y = repeat(py, cell+thin);
	indexX = amodIndex(p.xz, segments);
	amod(p.xz, segments);
	seed = float2(indexX, indexY);
	salt = rng(seed);
	p.x -= radius;
	float2 dimension = float2(.75,.5);
	p.x +=  dimension.x * 1.5;
	smax( scene, -sdBox(p, float3(dimension.x, .1, dimension.y)), MAT_WINDOW_HORIZONTAL );
	smin( scene, window( p.xzy, dimension, salt ), MAT_WINDOW_HORIZONTAL );
	
	// vertical window
	p = pDonut;
	py = p.y + cell/2. + _time * speed;
	indexY = floor(py / (cell+thin));
	p.y = repeat(py, cell+thin);
	indexX = amodIndex(p.xz, segments);
	amod(p.xz, segments);
	seed = float2(indexX, indexY);
	salt = rng(seed);
	p.x -= radius;
	dimension.y = 1.5;
	p.x +=  dimension.x * 1.25;
	smax( scene, -sdBox(p, float3(dimension, .1)), MAT_WINDOW_VERTICAL );
	smin( scene, window(p, dimension, salt), MAT_WINDOW_VERTICAL );
	
	// elements
	p = pDonut;
	p.xz = mul( p.xz, rot(PI/segments) );
	py = p.y + cell/2. + _time * speed;
	indexY = floor(py / (cell+thin));
	p.y = repeat(py, cell+thin);
	indexX = amodIndex(p.xz, segments);
	amod(p.xz, segments);
	seed = float2(indexX, indexY);
	salt = rng(seed);
	p.x -= radius - height;
	smin( scene, boxes(p, salt), MAT_BOX+salt );
	
	return scene;
}
/*
void mainImage( out float4 color, in float2 coord ) {
    float2 uv = (coord.xy-.5*iResolution.xy)/iResolution.y;
    float3 eye = float3(0,0,-20);
    float3 ray = normalize(float3(uv, 1.3));
    camera(eye);
    camera(ray);
    float dither = rng(uv+frac(_time));
    float3 pos = eye;
    float shade = 0.;

const float STEPS = 100.;

    for (float i = 0.; i <= 1.; i += 1./STEPS) {
        float dist = map(pos);
        if (dist < VOLUME) {
            shade = 1.-i;
            break;
        }
        dist *= .5 + .1 * dither;
        pos += ray * dist;
    }
    float3 light = float3(40.,100.,-10.);
    float shadow = getShadow(pos, light, 4.);
    color = float4(1);
    color *= shade;
    color *= shadow;
    color = smoothstep(.0, .5, color);
    color.rgb = sqrt(color.rgb);
}
*/
Intersection RayMarchScene( float3 _wsPos, float3 _wsDir, uint _stepsCount, float _distanceFactor=0.8 ) {

	const float STEP = 1.0 / _stepsCount;

	Intersection	result = (Intersection) 0;
	result.shade = 0.0;
	result.wsHitPosition = float4( _wsPos, 0.0 );

	float4	unitStep = float4( _wsDir, 1.0 );
	for ( float i=0; i <= 1.0; i+=STEP ) {
		float2 d = map( result.wsHitPosition.xyz );
		if ( d.x < 0.01 ) {
			result.shade = 1.0-i;
			result.materialID = d.y;
			break;
		}
		d.x *= _distanceFactor;	// Tends to miss features if larger than 0.5 (???)
		result.wsHitPosition += d.x * unitStep;
	}

	result.wsNormal = getNormal( result.wsHitPosition.xyz );

	result.albedo = 0.5 * float3( 1, 1, 1 );
	result.roughness = 0.5;	// Totally rough
	result.F0 = 0.04;		// Dielectric
	result.emissive = 0.0;
	result.wsVelocity = 0.0;
//	switch ( result.materialID ) {
//		case MAT_WALL	: result.albedo = 0.10 * float3( 1.0, 0.6, 0.2 ); break;
//		case MAT_FLOOR	: result.albedo = 0.15 * float3( 1.0, 0.6, 0.2 ); break;
//		case MAT_STAIRS	: result.albedo = 0.4 * float3( 1.0, 0.9, 0.85 ); break;
//		case MAT_PANEL	: result.albedo = 0.7 * float3( 1.0, 0.9, 0.05 ); break;
//		case MAT_PAPERS	: result.albedo = 0.7 * float3( 1.0, 1.0, 1.0 ); result.wsVelocity = PAPER_VELOCITY; break;
//	}
//	if ( result.materialID >= MAT_BOOKS	) {
//		float	salt = result.materialID - MAT_BOOKS;
////		result.albedo = 0.05 * float3( 1.0, 0.3, 0.1 );
//		result.albedo = lerp( 0.05, 0.2, 1.0 - salt ) * float3( lerp( 0.8, 1.0, sin( -376.564 * salt ) ), lerp( 0.3, 0.6, salt ), lerp( 0.1, 0.2, sin( 12374.56 * salt ) ) );
//	}

	return result;
}

////////////////////////////////////////////////////////////////////////////////
// Interface methods
////////////////////////////////////////////////////////////////////////////////
//
Intersection	TraceScene( float3 _wsPos, float3 _wsDir ) {
	return RayMarchScene( _wsPos, _wsDir, 200, 0.8 );
}

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _roughness, float3 _IOR, float _pixelSize_m, float3 _wsBentNormal, float2 _cosConeAnglesMinMax, float _noise ) {

	LightingResult	result = (LightingResult) 0;

//	LightInfoPoint	lightInfo;
//					lightInfo.wsPosition = GetPointLightPosition( lightInfo.distanceAttenuation );
//					lightInfo.flux = 1000.0 * GetShadowPoint( _wsPosition, _wsNormal, pixelSize_m, lightInfo.wsPosition, lightInfo.distanceAttenuation.y, _tex_ShadowMap, _noise );
//	ComputeLightPoint( _wsPosition, _wsNormal, _wsView, _roughness, _IOR, _wsBentNormal, _cosConeAnglesMinMax, lightInfo, result );

	LightInfoDirectional	lightInfo;
							lightInfo.illuminance = _sunIntensity * GetShadowDirectional( _wsPosition, _wsNormal, _pixelSize_m, _tex_ShadowMapDirectional, _noise, SHADOW_BIAS );
							lightInfo.wsDirection = -_directionalShadowMap2World[2].xyz;
	ComputeLightDirectional( _wsPosition, _wsNormal, _wsView, _roughness, _IOR, _wsBentNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}

float3			GetPointLightPosition( out float2 _distanceNearFar ) {
	_distanceNearFar = float2( 10.0, 20.0 );	// Don't care, let natural 1/r² take care of attenuation
	return float3( 0, 10, 0 );
}
