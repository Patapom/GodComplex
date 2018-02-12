///////////////////////////////////////////////////////////////////////////////////
// Adapted for true Cornell Box dimensions (as given by https://www.graphics.cornell.edu/online/box/data.html)
//
///////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

// Cornell Box Dimensions
//
static const float3	CORNELL_SIZE = float3( 5.528f, 5.488f, 5.592f );
static const float3	CORNELL_POS = 0.0;
static const float	CORNELL_THICKNESS = 0.1f;
static const float3	CORNELL_ROOM_REFLECTANCE = 0.5 * float3( 1.0, 1.0, 1.0 );
static const float3	CORNELL_LEFT_WALL_REFLECTANCE = 0.5 * float3( 1.0, 0.2, 0.02 );
static const float3	CORNELL_RIGHT_WALL_REFLECTANCE = 0.5 * float3( 0.2, 1.0, 0.05 );

// Small box setup
static const float3	CORNELL_SMALL_BOX_SIZE = float3( 1.65, 1.65, 1.65 );	// It's a cube
static const float3	CORNELL_SMALL_BOX_POS = float3( 1.855, 0.5 * CORNELL_SMALL_BOX_SIZE.y, 1.69 ) - 0.5 * CORNELL_SIZE;
static const float	CORNELL_SMALL_BOX_ANGLE = 0.29145679447786709199560462143289;	// ~16°
static const float3	CORNELL_SMALL_BOX_REFLECTANCE = 0.5 * float3( 1.0, 1.0, 1.0 );

// Large box setup
static const float3	CORNELL_LARGE_BOX_SIZE = float3( 1.65, 3.3, 1.65 );
static const float3	CORNELL_LARGE_BOX_POS = float3( 3.685, 0.5 * CORNELL_LARGE_BOX_SIZE.y, 3.6125 ) - 0.5 * CORNELL_SIZE;
static const float	CORNELL_LARGE_BOX_ANGLE = -0.30072115015043337195437489062082;	// ~17°
static const float3	CORNELL_LARGE_BOX_REFLECTANCE = 0.5 * float3( 1.0, 1.0, 1.0 );

// Light setup
static const float3	CORNELL_LIGHT_SIZE = float3( 1.3, 0.0, 1.05 );
static const float3	CORNELL_LIGHT_POS = float3( 2.78, 5.2, 2.795 ) - 0.0 * float3( CORNELL_LIGHT_SIZE.x, 0.0, CORNELL_LIGHT_SIZE.z ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float3	LIGHT_ILLUMINANCE = 2500.0;
static const float3	LIGHT_REFLECTANCE = 0.78;
static const float3	LIGHT_SIZE = float3( 1.0, 0.0, 1.0 );

static const float	MAT_ROOM = 1;
static const float	MAT_ROOM_WALL_LEFT = 2;
static const float	MAT_ROOM_WALL_RIGHT = 3;
static const float	MAT_SMALL_BOX = 4;
static const float	MAT_LARGE_BOX = 5;
static const float	MAT_LIGHT = 6;
static const float	MAT_EMISSIVE = 7;



float2	ComputeBoxIntersections( float3 _position, float3 _view, float3 _boxCenter, float3 _boxInvHalfSize, out uint2 _hitSides ) {
	float3	normPos = (_position - _boxCenter) * _boxInvHalfSize;
	float3	normView = _view * _boxInvHalfSize;

	float2	tempX = float2( 1.0 - normPos.x, -1.0 - normPos.x ) / normView.x;
	float2	tempY = float2( 1.0 - normPos.y, -1.0 - normPos.y ) / normView.y;
	float2	tempZ = float2( 1.0 - normPos.z, -1.0 - normPos.z ) / normView.z;

	tempX = _view.x >= 0.0 ? tempX.yx : tempX;
	tempY = _view.y >= 0.0 ? tempY.yx : tempY;
	tempZ = _view.z >= 0.0 ? tempZ.yx : tempZ;

#if 1
	// Verbose code but returns hit sides
	_hitSides = uint2( _view.x >= 0.0 ? 0 : 1, _view.x >= 0.0 ? 1 : 0 );
	float2	hitDistances = tempX;
	if ( tempY.x > hitDistances.x ) {
		hitDistances.x = tempY.x;
		_hitSides.x = _view.y >= 0 ? 2 : 3;
	}
	if ( tempZ.x > hitDistances.x ) {
		hitDistances.x = tempZ.x;
		_hitSides.x = _view.z >= 0 ? 4 : 5;
	}
	if ( tempY.y < hitDistances.y ) {
		hitDistances.y = tempY.y;
		_hitSides.y = _view.y >= 0 ? 3 : 2;
	}
	if ( tempZ.y < hitDistances.y ) {
		hitDistances.y = tempZ.y;
		_hitSides.y = _view.z >= 0 ? 5 : 4;
	}

	return hitDistances;
#else
	// Fast result without hit side info
	_hitSides = 0;
	return float2(	max( max( tempX.x, tempY.x ), tempZ.x ),
					min( min( tempX.y, tempY.y ), tempZ.y ) );
#endif
}
float3x3	BuildBoxRotation( float _angle ) {
	float2	scAngle;
	sincos( _angle, scAngle.x, scAngle.y );
	return float3x3( scAngle.y, 0, scAngle.x, 0, 1, 0, -scAngle.x, 0, scAngle.y );
}
float2	ComputeRotatedBoxIntersections( float3 _position, float3 _view, float3 _boxCenter, float3 _boxInvHalfSize, float _angle, out uint2 _hitSides ) {
	_position -= _boxCenter;

	float3x3	rot = BuildBoxRotation( _angle );
	_position = mul( _position, rot );
	_view = mul( _view, rot );

	return ComputeBoxIntersections( _position, _view, 0, _boxInvHalfSize, _hitSides );
}
float3	Side2Normal( uint _side ) {
	return _side == 0 ? float3( -1, 0, 0 ) : (_side == 1 ? float3( 1, 0, 0 ) : (_side == 2 ? float3( 0, -1, 0 ) : (_side == 3 ? float3( 0, 1, 0 ) : (_side == 4 ? float3( 0, 0, -1 ) : float3( 0, 0, 1 )))));
}

Intersection	TraceScene( float3 _wsPos, float3 _wsDir ) {
	Intersection	result = (Intersection) 0;
	result.shade = 0;
	result.wsHitPosition = float4( _wsPos, 0.0 );

	// Test the 3 boxes
	float2	hitDistance = float2( 1e6, 0 );
	uint2	hitSides;
	float2	testDistances = ComputeBoxIntersections( _wsPos, _wsDir, CORNELL_POS, 2.0 / CORNELL_SIZE, hitSides );
	if ( testDistances.x < testDistances.y ) {
		hitDistance = float2( testDistances.y, hitSides.y == 0 ? MAT_ROOM_WALL_RIGHT : (hitSides.y == 1 ? MAT_ROOM_WALL_LEFT : MAT_ROOM) );
		result.wsNormal = -Side2Normal( hitSides.y );	// Turn normals inside out
	}

	testDistances = ComputeRotatedBoxIntersections( _wsPos, _wsDir, CORNELL_SMALL_BOX_POS, 2.0 / CORNELL_SMALL_BOX_SIZE, CORNELL_SMALL_BOX_ANGLE, hitSides );
	if ( testDistances.x < testDistances.y && testDistances.x > 0 && testDistances.x < hitDistance.x ) {
		hitDistance = float2( testDistances.x, MAT_SMALL_BOX );
		float3x3	rot = BuildBoxRotation( CORNELL_SMALL_BOX_ANGLE );
		result.wsNormal = mul( rot, Side2Normal( hitSides.x ) );
//		result.wsNormal = Side2Normal( hitSides.x );
	}

//	float	testAngle = CORNELL_LARGE_BOX_ANGLE;
	float	testAngle = _time;
	testDistances = ComputeRotatedBoxIntersections( _wsPos, _wsDir, CORNELL_LARGE_BOX_POS, 2.0 / CORNELL_LARGE_BOX_SIZE, testAngle, hitSides );
	if ( testDistances.x < testDistances.y && testDistances.x > 0 && testDistances.x < hitDistance.x ) {
		hitDistance = float2( testDistances.x, MAT_LARGE_BOX );
		float3x3	rot = BuildBoxRotation( testAngle );
		result.wsNormal = mul( rot, Side2Normal( hitSides.x ) );
//		result.wsNormal = Side2Normal( hitSides.x );
	}

	// Test some emissive area light...
const float3	CORNELL_EMISSIVE_RECT_POS = float3( 4.6, 0.6, 1.6 ) - 0.5 * CORNELL_SIZE;
const float3	CORNELL_EMISSIVE_RECT_SIZE = float3( 1.0, 1.0, 0.1 );	// It's a thin rectangle
const float		CORNELL_EMISSIVE_RECT_ANGLE = 0.29145679447786709199560462143289;	// ~16°
	testDistances = ComputeRotatedBoxIntersections( _wsPos, _wsDir, CORNELL_EMISSIVE_RECT_POS, 2.0 / CORNELL_EMISSIVE_RECT_SIZE, CORNELL_EMISSIVE_RECT_ANGLE, hitSides );
	if ( testDistances.x < testDistances.y && testDistances.x > 0 && testDistances.x < hitDistance.x ) {
		hitDistance = float2( testDistances.x, MAT_EMISSIVE );
		float3x3	rot = BuildBoxRotation( CORNELL_EMISSIVE_RECT_ANGLE );
		result.wsNormal = mul( rot, Side2Normal( hitSides.x ) );
	}

	// Update result
	result.shade = step( 1e-5, hitDistance.x );
	result.wsHitPosition += hitDistance.x * float4( _wsDir, result.shade );	// W kept at 0 (invalid) if no hit
	result.roughness = 0;
	result.F0 = 0;
	result.materialID = hitDistance.y;
	result.emissive = 0.0;
	switch ( uint(result.materialID) ) {
		case MAT_ROOM :				result.albedo = CORNELL_ROOM_REFLECTANCE; break;
		case MAT_ROOM_WALL_LEFT :	result.albedo = CORNELL_LEFT_WALL_REFLECTANCE; break;
		case MAT_ROOM_WALL_RIGHT :	result.albedo = CORNELL_RIGHT_WALL_REFLECTANCE; break;
		case MAT_SMALL_BOX :		result.albedo = CORNELL_SMALL_BOX_REFLECTANCE; break;
		case MAT_LARGE_BOX :		result.albedo = CORNELL_LARGE_BOX_REFLECTANCE; break;
		case MAT_EMISSIVE :			result.emissive = 4.0 * float3( 0.2, 0.8, 1.0 ); break;
	}

	return result;
}

Texture2DArray<float>	_tex_ShadowMap : register( t6 );

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax ) {
	LightInfoPoint	lightInfo;
					lightInfo.flux = LIGHT_ILLUMINANCE;
					lightInfo.wsPosition = GetPointLightPosition( lightInfo.distanceAttenuation );

	// Sample shadow map
	lightInfo.flux *= GetShadow( _wsPosition, lightInfo.wsPosition, lightInfo.distanceAttenuation.y, _tex_ShadowMap );

	LightingResult	result = (LightingResult) 0;
	ComputeLightPoint( _wsPosition, _wsNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}

float3			GetPointLightPosition( out float2 _distanceNearFar ) {
	_distanceNearFar = float2( 100.0, 1000.0 );	// Don't care, let natural 1/r² take care of attenuation
	return CORNELL_LIGHT_POS;
}


/*
// Shamelessly stolen from https://www.shadertoy.com/view/Xt2fWK
// NOT USED AFTER ALL
//
struct Plane 
{
    vec3 center;
    vec3 s;
    vec3 t;
    vec2 dim;
    int material;
};
    
struct Ray
{
    vec3 origin;
	vec3 direction;
};
    
struct Sphere
{
    vec3 center;
    float radius;
};
    
struct Material
{
    vec3 color;
    vec3 emissive;
};
    
struct Light
{
    int plane;
};
    
const float PI = 3.1415926;
const vec2 lightSize = vec2(0.468, 0.378);

bool raySphereIntersection(in Ray r, in Sphere s, out float t, out vec3 i, out vec3 n, out int m)
{
    vec3 dv = s.center - r.origin;
	float b = dot(r.direction, dv);
	float d = b * b - dot(dv, dv) + s.radius * s.radius;
    bool intersects = (d >= 0.0);
    if (intersects)
    {
		t = b - sqrt(d);
        i = r.origin + t * r.direction;
        n = normalize(i - s.center);
        m = 0;
    }
	return intersects;
}

bool rayPlaneIntersection(in Ray r, in Plane p, out float t, out vec3 i, out vec3 n, out int m)
{
    vec3 planeNormal = normalize(cross(p.t, p.s));
	float d = dot(r.direction, planeNormal);
    bool intersects = (d <= 0.0f);
    if (intersects)
    {
        m = p.material;
        n = planeNormal;
    	t = dot(planeNormal, p.center - r.origin) / d;
        i = r.origin + r.direction * t;
        float ds = dot(p.center - i, p.s);
        float dt = dot(p.center - i, p.t);
        intersects = (abs(ds) <= p.dim.x) && (abs(dt) <= p.dim.y);
    }
    return intersects;
}

const vec3 cameraPostiion = vec3(0.0, 1.0, 3.5);
const vec3 cameraTarget = vec3(0.0, 1.0, 0.0);
const vec3 cameraUp = vec3(0.0, 1.0, 0.0);

#define materialsCount 5
Material materials[materialsCount];

#define planesCount 6
Plane planes[planesCount];

#define lightsCount 1
Light lights[lightsCount];

void populateMaterials();
void populatePlanes();

vec3 shade(in vec3 position, in vec3 normal, in vec3 view, in int materialId);
vec3 toLinear(in vec3 srgb);
vec3 toSRGB(in vec3 linear);

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	fragColor = vec4(0.025, 0.05, 0.075, 1.0);
    
    populateMaterials();
    populatePlanes();
    
	vec2 p = (2.0 * fragCoord.xy - iResolution.xy) / iResolution.y;
    
    vec3 w = normalize(cameraTarget - cameraPostiion);
    vec3 u = normalize(cross(w, cameraUp));
    vec3 v = normalize(cross(u, w));
    
    Ray primaryRay;
    primaryRay.origin = cameraPostiion;
    primaryRay.direction = normalize(p.x * u + p.y * v + 2.5 * w);
    
    vec3 intersectionPoint;
    vec3 intersectionNormal;
    float minIntersectionDistance = 1000000.0;
    int materialIndex = 0;
    int objectIndex = 0;
    
    bool intersectionOccured = false;
    for (int i = 0; i < planesCount; ++i)
    {
	    vec3 p;
    	vec3 n;
        int m;
    	float t = 0.0;
        if (rayPlaneIntersection(primaryRay, planes[i], t, p, n, m))
        {
            if (t < minIntersectionDistance)
            {
                objectIndex = i;
                materialIndex = m;
                minIntersectionDistance = t;
                intersectionNormal = n;
                intersectionPoint = p;
                intersectionOccured = true;
            }
        }
    }
    
    if (intersectionOccured == false)
        return;
    
    bool hitLight = false;
    for (int i = 0; i < lightsCount; ++i)
    {
        if (lights[i].plane == objectIndex)
        {
            hitLight = true;
            break;
        }
            
    }
    
    if (hitLight)
    {
        fragColor.xyz = materials[materialIndex].emissive;
        return;
    }
    
	fragColor.xyz = toSRGB(shade(intersectionPoint, intersectionNormal, primaryRay.direction, materialIndex));
}

Material makeMaterial(in vec3 d, in vec3 e)
{
    Material m;
    m.color = d;
    m.emissive = e;
    return m;
}

void populateMaterials()
{
	float lightArea = lightSize.x * lightSize.y;
    
	materials[0] = makeMaterial(vec3(1.0, 1.0, 1.0), vec3(0.0, 0.0, 0.0));   
	materials[1] = makeMaterial(vec3(0.05, 1.0, 0.05), vec3(0.0, 0.0, 0.0));   
	materials[2] = makeMaterial(vec3(1.0, 0.05, 0.05), vec3(0.0, 0.0, 0.0));   
	materials[3] = makeMaterial(vec3(0.0, 0.0, 0.0), vec3(15.0, 15.0, 15.0));
}

Plane makePlane(in vec3 c, in vec3 s, in vec3 t, in vec2 d, in int m)
{
    Plane p;
    p.center = c;
    p.s = normalize(s);
    p.t = normalize(t);
    p.dim = d / 2.0;
    p.material = m;
    return p;
}

void populatePlanes()
{
    planes[0] = makePlane(vec3( 0.0,  0.0,  0.0), vec3( 1.0,  0.0, 0.0), vec3(0.0, 0.0, 1.0), vec2(2.0, 2.0), 0); // floor
    planes[1] = makePlane(vec3( 0.0, +2.0,  0.0), vec3(-1.0,  0.0, 0.0), vec3(0.0, 0.0, 1.0), vec2(2.0, 2.0), 0); // ceil
    planes[2] = makePlane(vec3( 0.0,  1.0, -1.0), vec3(-1.0,  0.0, 0.0), vec3(0.0, 1.0, 0.0), vec2(2.0, 2.0), 0); // back
    planes[3] = makePlane(vec3( 1.0,  1.0,  0.0), vec3( 0.0,  1.0, 0.0), vec3(0.0, 0.0, 1.0), vec2(2.0, 2.0), 1); // right
    planes[4] = makePlane(vec3(-1.0,  1.0,  0.0), vec3( 0.0, -1.0, 0.0), vec3(0.0, 0.0, 1.0), vec2(2.0, 2.0), 2); // left
    planes[5] = makePlane(vec3( 0.0, +1.998, 0.0), vec3(-1.0,  0.0, 0.0), vec3(0.0, 0.0, 1.0), lightSize, 3); // light
    
    lights[0].plane = 5;
}

//
//
// shading happens here
//
//
vec3 toLinear(in vec3 srgb)
{
    return pow(srgb, vec3(2.2));
}
    
vec3 toSRGB(in vec3 linear)
{
    return pow(linear, vec3(1.0/2.2));
}

float integrateLTC(in vec3 v1, in vec3 v2)
{
    float cosTheta = dot(v1, v2);
    float theta = acos(cosTheta);    
    return cross(v1, v2).z * ((theta > 0.001) ? theta / sin(theta) : 1.0);
}

vec3 mul(in mat3 m, in vec3 p)
{
    return m * p;
}

float evaluateLTC(in vec3 position, in vec3 normal, in vec3 view, in vec3 points[4])
{
    vec3 t1 = normalize(view - normal * dot(view, normal));
    vec3 t2 = cross(normal, t1);

    mat3 Minv = transpose(mat3(t1, t2, normal));

    vec3 L[4];
    L[0] = normalize(mul(Minv, points[0] - position));
    L[1] = normalize(mul(Minv, points[1] - position));
    L[2] = normalize(mul(Minv, points[2] - position));
    L[3] = normalize(mul(Minv, points[3] - position));

    float sum = 0.0;
    sum += integrateLTC(L[0], L[1]);
    sum += integrateLTC(L[1], L[2]);
    sum += integrateLTC(L[2], L[3]);
	sum += integrateLTC(L[3], L[0]);
    return max(0.0, sum); 
}

vec3 sampleLight(in vec3 p[4], in vec4 rnd)
{
    vec3 pt = mix(p[0], p[1], rnd.x);
    vec3 pb = mix(p[3], p[2], rnd.x);
    return mix(pt, pb, rnd.y);
}

vec3 shade(in vec3 position, in vec3 normal, in vec3 view, in int materialId)
{
    vec4 rnd = texture(iChannel0, 100.0 * (normal.xy + position.y * view.yx - position.xz) + iTime);
    
	float lightArea = lightSize.x * lightSize.y;
    Plane lightPlane = planes[lights[0].plane];
    Material lightMaterial = materials[lightPlane.material];
    vec3 materialColor = materials[materialId].color;
    vec3 lightColor = lightMaterial.emissive;
    vec3 lightNormal = normalize(cross(lightPlane.s, lightPlane.t));
    vec3 lightPoints[4];
    lightPoints[0] = lightPlane.center + lightPlane.s * lightPlane.dim.x - lightPlane.t * lightPlane.dim.y;
    lightPoints[1] = lightPlane.center + lightPlane.s * lightPlane.dim.x + lightPlane.t * lightPlane.dim.y;
    lightPoints[2] = lightPlane.center - lightPlane.s * lightPlane.dim.x + lightPlane.t * lightPlane.dim.y;
    lightPoints[3] = lightPlane.center - lightPlane.s * lightPlane.dim.x - lightPlane.t * lightPlane.dim.y;
    
    vec3 l = normalize(lightPlane.center - position);
    float lambert = dot(normal, l) / PI;
    
    float ltc = evaluateLTC(position, normal, view, lightPoints) / (2.0 * PI);
    
    float bruteforced = 0.0;
    const int samples = 500;
    for (int i = 0; i < samples; ++i)
    {
        vec3 pl = sampleLight(lightPoints, rnd) - position;
		float DdotL = dot(pl, lightNormal);
        float LdotN = dot(pl, normal);
        if ((LdotN > 0.0) && (DdotL > 0.0))
        {
    	    float distanceSquared = dot(pl, pl);
            float distanceToPoint = sqrt(distanceSquared);
            float pdf = distanceSquared / (DdotL / distanceToPoint * lightArea);
            float bsdf = 1.0 / PI;
        	bruteforced += bsdf / pdf * (LdotN / distanceToPoint);
        }
    	rnd = texture(iChannel0, rnd.xz + 23.0 * rnd.yx);
    }
    bruteforced /= float(samples);
    
    return lightColor * materialColor * ltc; // abs(ltc - bruteforced);
}
*/