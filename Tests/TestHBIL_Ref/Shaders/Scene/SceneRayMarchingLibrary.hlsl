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

#define MAT_WALL	1
#define MAT_FLOOR	2
#define MAT_STAIRS	3
#define MAT_PANEL	5
#define MAT_PAPERS	6
#define MAT_BOOKS	10

#define PAPER_VELOCITY float3( 0, 2.0, 0 );

#define repeat(v,c) (glmod(v,c)-0.5*c)
#define sDist(v,r) (length(v)-r)

float glmod(float x, float y) {
	return x - y * floor(x/y);
//	return x - y * trunc(x/y);
}

void	smin( inout float2 _d0, float _d1, float _mat ) {
	_d0 = _d0.x <= _d1 ? _d0 : float2( _d1, _mat );
}
void	smax( inout float2 _d0, float _d1, float _mat ) {
	_d0 = _d0.x >= _d1 ? _d0 : float2( _d1, _mat );
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
	float a = atan2(p.y,p.x)+0.5 * an;
	float c = floor(a/an);
	c = lerp(c,abs(c), step( count*.5, abs(c) ) );
	a = glmod(a,an)-an/2.;
	p.xy = float2(cos(a),sin(a))*length(p);
	return c;
}
//float aindex (float2 p, float count) { float an = 2.*PI/count; float a = atan(p.y,p.x)+an/2.; float c = floor(a/an); return mix(c,abs(c),step(count*.5,abs(c))); }
float2 map (float3 pos);
float3 getNormal (float3 p) { float2 e = float2(.001,0); return normalize(float3(map(p+e.xyy).x-map(p-e.xyy).x,map(p+e.yxy).x-map(p-e.yxy).x,map(p+e.yyx).x-map(p-e.yyx).x)); }

//float hardShadow (float3 pos, float3 light) {
//    float3 dir = normalize(light - pos);
//    float maxt = length(light - pos);
//    float t = .02;
//    for (float i = 0.; i <= 1.; i += 1./30.) {
//        float dist = map(pos + dir * t).x;
//        if (dist < 0.01) return 0.;
//        t += dist;
//        if (t >= maxt) break;
//    }
//    return 1.;
//}

float2 map(float3 pos) {
  float2 scene = float2( 1000., -1 );
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
  smin( scene, max(-p.x, abs(p.z)-wallThin), MAT_WALL );
  smax( scene, -sDist(pos.xz, wallRadius-wallOffset), MAT_WALL );

  // floors
  p = pos;
  p.y = repeat(p.y, cell.y);
  float disk = max(sDist(p.xz, 1000.), abs(p.y)-floorThin);
  disk = max(disk, -sDist(pos.xz, wallRadius));
  smin( scene, disk, MAT_FLOOR );

  // stairs
  p = pos;
  float stairIndex = amod(p.xz, stairCount);
  p.y -= stairIndex*stairHeight;
  p.y = repeat(p.y, stairCount*stairHeight);
  float stair = sdBox(p, float3(100,stairHeight,stairDepth));
  smin( scene, max(stair, max(holeWall, -holeStair)), MAT_STAIRS );
  p = pos;
  rot(p.xz,PI/stairCount);
  stairIndex = amod(p.xz, stairCount);
  p.y -= stairIndex*stairHeight;
  p.y = repeat(p.y, stairCount*stairHeight);
  stair = sdBox(p, float3(100,stairHeight,stairDepth));
  smin( scene, max(stair, max(holeWall, -holeStair)), MAT_STAIRS);
  p = pos;
  p.y += stairHeight*.5;
  p.y -= stairHeight*stairCount*atan2(p.z,p.x)/(2.*PI);
  p.y = repeat(p.y, stairCount*stairHeight);
  smin( scene, max(max(sDist(p.xz, wallRadius), abs(p.y)-stairHeight), -holeStair), MAT_STAIRS );
  
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
  smin( scene, max(sdBox(p, float3(bookSize.x,100.,bookSize.z)), p.y-bookSize.y), MAT_BOOKS + salt );


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
  smin( scene, panel, MAT_PANEL );

  // papers
  p = pos;
  p.y -= stairHeight;
  p += _time * PAPER_VELOCITY;
  rot(p.xz,PI/stairCount);
  float ry = 8.;
  float iy = floor(p.y/ry);
  salt = rng(iy);
  float a = iy;
  p.xz -= float2(cos(a),sin(a))*paperRadius;
  p.y = repeat(p.y, ry);
  rot(p.xy, p.z);
  rot(p.xz, PI/4.+salt+_time);
  smin( scene, sdBox(p, paperSize), MAT_PAPERS );

  return scene;
}

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
	result.roughness = 1.0;	// Totally rough
	result.F0 = 0.0;		// Dielectric
	result.emissive = 0.0;
	result.wsVelocity = 0.0;
	switch ( result.materialID ) {
		case MAT_WALL	: result.albedo = 0.10 * float3( 1.0, 0.6, 0.2 ); break;
		case MAT_FLOOR	: result.albedo = 0.15 * float3( 1.0, 0.6, 0.2 ); break;
		case MAT_STAIRS	: result.albedo = 0.4 * float3( 1.0, 0.9, 0.85 ); break;
		case MAT_PANEL	: result.albedo = 0.7 * float3( 1.0, 0.9, 0.05 ); break;
		case MAT_PAPERS	: result.albedo = 0.7 * float3( 1.0, 1.0, 1.0 ); result.wsVelocity = PAPER_VELOCITY; break;
	}
	if ( result.materialID >= MAT_BOOKS	) {
		float	salt = result.materialID - MAT_BOOKS;
//		result.albedo = 0.05 * float3( 1.0, 0.3, 0.1 );
		result.albedo = lerp( 0.05, 0.2, 1.0 - salt ) * float3( lerp( 0.8, 1.0, sin( -376.564 * salt ) ), lerp( 0.3, 0.6, salt ), lerp( 0.1, 0.2, sin( 12374.56 * salt ) ) );
	}

	return result;
//  float4 color = float4(shade);
//  color *= getLight(pos, _wsPos);
//  color = smoothstep(.0, .5, color);
//  color = sqrt(color);
//  return color;
}

////////////////////////////////////////////////////////////////////////////////
// Interface methods
////////////////////////////////////////////////////////////////////////////////
//
Texture2DArray<float>	_tex_ShadowMap : register( t6 );

Intersection	TraceScene( float3 _wsPos, float3 _wsDir ) {
	return RayMarchScene( _wsPos, _wsDir, 100, 0.8 );
}

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax ) {
	LightInfoPoint	lightInfo;
					lightInfo.flux = 1000.0;
					lightInfo.wsPosition = GetPointLightPosition( lightInfo.distanceAttenuation );

	// Sample shadow map
	lightInfo.flux *= GetShadow( _wsPosition, lightInfo.wsPosition, lightInfo.distanceAttenuation.y, _tex_ShadowMap );

	LightingResult	result = (LightingResult) 0;
	ComputeLightPoint( _wsPosition, _wsNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}

float3			GetPointLightPosition( out float2 _distanceNearFar ) {
	_distanceNearFar = float2( 10.0, 100.0 );	// Don't care, let natural 1/r² take care of attenuation
	return float3( 0, 10, 0 );
}
