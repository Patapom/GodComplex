///////////////////////////////////////////////////////////////////////////////////
// Shamelessly stolen from https://www.shadertoy.com/view/MllBR7
//
// M.A.M. Stairs by Leon Denise aka ponk
// another raymarching sketch inspired by Marc-Antoine Mathieu.
// using code from IQ, Mercury, LJ, Duke, Koltes
// made with Atom Editor GLSL viewer (that's why there is 2 space tabulations)
// 2017-11-24
///////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#define repeat(v,c) (glmod(v,c)-0.5*c)
#define sDist(v,r) (length(v)-r)

float glmod(float x, float y) {
	return x - y * floor(x/y);
//	return x - y * trunc(x/y);
}

void rot( inout float2 p, float a) {
	float2	scA;
	sincos( a, scA.x, scA.y );
	float2x2	M = float2x2(scA.y,-scA.x,scA.x,scA.y);
	p = mul( p, M );
}
float rng(float2 seed) { return frac(sin(dot(seed*.1684,float2(32.649,321.547)))*43415.); }
float sdBox( float3 p, float3 b ) { float3 d = abs(p) - b; return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0)); }
float amod( inout float2 p, float count ) {
	float an = 2.*PI/count;
	float a = atan2(p.y,p.x)+an/2.;
	float c = floor(a/an);
	c = lerp(c,abs(c), step( count*.5, abs(c) ) );
	a = glmod(a,an)-an/2.;
	p.xy = float2(cos(a),sin(a))*length(p);
	return c;
}
//float aindex (float2 p, float count) { float an = 2.*PI/count; float a = atan(p.y,p.x)+an/2.; float c = floor(a/an); return mix(c,abs(c),step(count*.5,abs(c))); }
float map (float3 pos);
float3 getNormal (float3 p) { float2 e = float2(.001,0); return normalize(float3(map(p+e.xyy)-map(p-e.xyy),map(p+e.yxy)-map(p-e.yxy),map(p+e.yyx)-map(p-e.yyx))); }

float hardShadow (float3 pos, float3 light) {
    float3 dir = normalize(light - pos);
    float maxt = length(light - pos);
    float t = .02;
    for (float i = 0.; i <= 1.; i += 1./30.) {
        float dist = map(pos + dir * t);
        if (dist < 0.01) return 0.;
        t += dist;
        if (t >= maxt) break;
    }
    return 1.;
}

float map (float3 pos) {
  float scene = 1000.;
  float wallThin = .2;
  float wallRadius = 8.;
  float wallOffset = .2;
  float wallCount = 10.;
  float floorThin = .1;
  float stairRadius = 5.;
  float stairHeight = .4;
  float stairCount = 40.;
  float stairDepth = .31;
  float bookCount = 100.;
  float bookRadius = 9.5;
  float bookSpace = 1.75;
  float3 bookSize = float3(1.,.2,.2);
  float3 panelSize = float3(.03,.2,.7);
  float2 cell = float2(1.4,3.);
  float paperRadius = 4.;
  float3 paperSize = float3(.3,.01,.4);
  float3 p;

  // move it
  //pos.y += _time;

  // twist it
  //rot(pos.xz,pos.y*.05+_time*.1);
  //pos.xz += normalize(pos.xz) * sin(pos.y*.5+_time);
  
  // holes
  float holeWall = sDist(pos.xz, wallRadius);
  float holeStair = sDist(pos.xz, stairRadius);

  // walls
  p = pos;
  amod(p.xz, wallCount);
  p.x -= wallRadius;
  scene = min(scene, max(-p.x, abs(p.z)-wallThin));
  scene = max(scene, -sDist(pos.xz, wallRadius-wallOffset));

  // floors
  p = pos;
  p.y = repeat(p.y, cell.y);
  float disk = max(sDist(p.xz, 1000.), abs(p.y)-floorThin);
  disk = max(disk, -sDist(pos.xz, wallRadius));
  scene = min(scene, disk);

  // stairs
  p = pos;
  float stairIndex = amod(p.xz, stairCount);
  p.y -= stairIndex*stairHeight;
  p.y = repeat(p.y, stairCount*stairHeight);
  float stair = sdBox(p, float3(100,stairHeight,stairDepth));
  scene = min(scene, max(stair, max(holeWall, -holeStair)));
  p = pos;
  rot(p.xz,PI/stairCount);
  stairIndex = amod(p.xz, stairCount);
  p.y -= stairIndex*stairHeight;
  p.y = repeat(p.y, stairCount*stairHeight);
  stair = sdBox(p, float3(100,stairHeight,stairDepth));
  scene = min(scene, max(stair, max(holeWall, -holeStair)));
  p = pos;
  p.y += stairHeight*.5;
  p.y -= stairHeight*stairCount*atan2(p.z,p.x)/(2.*PI);
  p.y = repeat(p.y, stairCount*stairHeight);
  scene = min(scene, max(max(sDist(p.xz, wallRadius), abs(p.y)-stairHeight), -holeStair));

  // books
  p = pos;
  p.y -= cell.y*.5;
  float2 seed = float2(floor(p.y/cell.y), 0);
  p.y = repeat(p.y, cell.y);
  rot(p.xz,PI/wallCount);
  seed.y += amod(p.xz, wallCount)/10.;
  seed.y += floor(p.z/(bookSize.z*bookSpace));
  p.z = repeat(p.z, bookSize.z*bookSpace);
  float salt = rng(seed);
  bookSize.x *= .5+.5*salt;
  bookSize.y += salt;
  bookSize.z *= .5+.5*salt;
  p.x -= bookRadius + wallOffset;
  p.x += cos(p.z*2.) - bookSize.x - salt * .25;
  p.x += .01*smoothstep(.99,1.,sin(p.y*(1.+10.*salt)));
  scene = min(scene, max(sdBox(p, float3(bookSize.x,100.,bookSize.z)), p.y-bookSize.y));

  // panel
  p = pos;
  p.y = repeat(p.y, cell.y);
  rot(p.xz,PI/wallCount);
  amod(p.xz, wallCount);
  p.x -= wallRadius;
  float panel = sdBox(p, panelSize);
  float pz = p.z;
  p.z = repeat(p.z, .2+.3*salt);
  panel = min(panel, max(sdBox(p, float3(.1,.1,.04)), abs(pz)-panelSize.z*.8));
  scene = min(scene, panel);

  // papers
  p = pos;
  p.y -= stairHeight;
  p.y += _time*2.;
  rot(p.xz,PI/stairCount);
  float ry = 8.;
  float iy = floor(p.y/ry);
  salt = rng(iy);
  float a = iy;
  p.xz -= float2(cos(a),sin(a))*paperRadius;
  p.y = repeat(p.y, ry);
  rot(p.xy, p.z);
  rot(p.xz, PI/4.+salt+_time);
  scene = min(scene, sdBox(p, paperSize));

  return scene;
}
//*
float3 getCamera (float3 eye, float2 uv) {
  float3 lookAt = 0;
//  float click = 0;//clamp(iMouse.w,0.,1.);
//  lookAt.x += mix(0.,((iMouse.x/iResolution.x)*2.-1.) * 10., click);
//  lookAt.y += mix(0.,iMouse.y/iResolution.y * 10., click);
  float fov = .65;
  float3 forward = normalize(lookAt - eye);
  float3 right = normalize(cross(float3(0,1,0), forward));
  float3 up = normalize(cross(forward, right));
  return normalize(fov * forward + uv.x * right + uv.y * up);
}

float getLight (float3 pos, float3 eye) {
  float3 light = float3(-.5,7.,1.);
  float3 normal = getNormal(pos);
  float3 view = normalize(eye-pos);
  float shade = dot(normal, view);
  shade *= hardShadow(pos, light);
  return shade;
}
//*/

struct Intersection {
	float4	hitPosition;
	float	shade;
	float3	wsNormal;
	float3	albedo;
};

Intersection RayMarchScene( float3 _wsPos, float3 _wsDir, float2 _UV ) {
//  float2 uv = (gl_FragCoord.xy-.5*iResolution.xy)/iResolution.y;
//  float dither = rng(uv+frac(time));

//	_UV = 2.0 * _UV - 1.0;
//	_UV.x *= _resolution.x / _resolution.y;
//	_UV.y *= -1.0;
//	_wsPos = float3(0,5,-4.5);
//	_wsDir = getCamera(_wsPos, _UV);

	const float STEP = 1.0 / 50;

	Intersection	result;
	result.shade = 0;
	result.hitPosition = float4( _wsPos, 0.0 );

	float4	unitStep = float4( _wsDir, 1.0 );
	for ( float i=0; i <= 1.0; i+=STEP ) {
		float d = map( result.hitPosition.xyz );
		if ( d < 0.01 ) {
			result.shade = 1.0-i;
			break;
		}
		d *= .5;// + .1 * dither;
		result.hitPosition += d * unitStep;
	}

	result.wsNormal = 0.0;
	if ( result.shade > 0 ) {
		result.wsNormal = getNormal( result.hitPosition.xyz );
	}

	result.albedo = 0.5 * float3( 1, 1, 1 );

	return result;
//  float4 color = float4(shade);
//  color *= getLight(pos, _wsPos);
//  color = smoothstep(.0, .5, color);
//  color = sqrt(color);
//  return color;
}
