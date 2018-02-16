///////////////////////////////////////////////////////////////////////////////////
// Ray-tracing a heightfield
// I'm using the heightfield used in my ground truth tests to see if I'm getting the same result as the 2D "ground truth" version
//
///////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

#if 1
////////////////


Texture2D<float>	_tex_Height : register(t32);
Texture2D<float3>	_tex_NormalMap : register(t33);

static const float3	BOX_CENTER = float3( 0, 0, 0 );
static const float3	BOX_HALF_SIZE = float3( 2, 0.5, 2 );	// 2x2 m� and 50cm height

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

float	ComputeHeightFieldIntersection( float3 _lsPosIn, float3 _lsDir, float _length, uint _stepsCount, inout float3 _lsNormal ) {
	float4	lsPos = float4( _lsPosIn, 0.0 );
	float4	lsStep = float4( _lsDir, 1.0 ) * _length / _stepsCount;

	float	prevH = _lsPosIn.y;
	[loop]
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		float2	UV = lsPos.xz;
		float	H = _tex_Height.SampleLevel( LinearClamp, UV, 0.0 );
		if ( lsPos.y < H ) {
			// Got a hit! Compute precise intersection...
			float	Z0 = lsPos.y - lsStep.y;
			float	Dz = lsStep.y;
			float	H0 = prevH;
			float	Dh = H - H0;
			float	t = (Z0 - H0) / (Dh - Dz);
			lsPos += (t-1.0) * lsStep;
			UV = lsPos.xz;
			_lsNormal = _tex_NormalMap.SampleLevel( LinearClamp, UV, 0.0 );
			return lsPos.w;
		}
		lsPos += lsStep;
		prevH = H;
	}

	return 1e6;
}

Intersection	TraceScene( float3 _wsPos, float3 _wsDir ) {
	Intersection	result = (Intersection) 0;
	result.shade = 0;
	result.wsHitPosition = float4( _wsPos, 0.0 );

	// Test the bounding-box
	float2	hitDistance = float2( 1e6, 0 );
	uint2	hitSides;
	float2	testDistances = ComputeBoxIntersections( _wsPos, _wsDir, BOX_CENTER, 1.0 / BOX_HALF_SIZE, hitSides );

	float3	wsNormal = float3( 0, 1, 0 );
	if ( testDistances.x < testDistances.y ) {
		float3	wsPosIn = _wsPos + testDistances.x * _wsDir;
		float3	wsPosOut = _wsPos + testDistances.y * _wsDir;

		const float3	BBOX_MIN = BOX_CENTER - BOX_HALF_SIZE;
		float3	lsPosIn = (wsPosIn - BBOX_MIN) * 0.5 / BOX_HALF_SIZE;
		float3	lsPosOut = (wsPosOut - BBOX_MIN) * 0.5 / BOX_HALF_SIZE;
		float3	lsDir = lsPosOut - lsPosIn;
		float	L = length( lsDir );
				lsDir /= L;

		float3	lsNormal = 0;
		float	lsHitDistance = ComputeHeightFieldIntersection( lsPosIn, lsDir, L, 100, lsNormal );
		float3	lsHitPos = lsPosIn + lsHitDistance * lsDir;
		float3	wsHitPos = BBOX_MIN + lsHitPos * 2.0 * BOX_HALF_SIZE;

		hitDistance.x = length( wsHitPos - _wsPos );
//hitDistance.x = testDistances.x;
		wsNormal = float3( lsNormal.x, lsNormal.z, -lsNormal.y );
	}

	// Update result
	result.shade = step( 1e-5, hitDistance.x );
	result.wsHitPosition += hitDistance.x * float4( _wsDir, result.shade );	// W kept at 0 (invalid) if no hit
	result.wsNormal = wsNormal;
	result.albedo = 0.5 * float3( 1.0, 0.9, 0.7 );
	result.wsVelocity = 0;
	result.roughness = 0;
	result.F0 = 0;
	result.materialID = 0;

	return result;
}

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax ) {
//	LightInfoPoint	lightInfo;
//					lightInfo.flux = LIGHT_ILLUMINANCE;
//					lightInfo.wsPosition = GetPointLightPosition( lightInfo.distanceAttenuation );
//
	LightingResult	result = (LightingResult) 0;
//	ComputeLightPoint( _wsPosition, _wsNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}

float3			GetPointLightPosition( out float2 _distanceNearFar ) {
	_distanceNearFar = float2( 100.0, 1000.0 );	// Don't care, let natural 1/r� take care of attenuation
	return BOX_CENTER + float3( 0, 2, 0 );
}


////////////////
#else

#define SAMPLE_HEIGHT_MAP	1

Texture2D<float>	_tex_Height : register(t32);
Texture2D<float3>	_tex_NormalMap : register(t33);

static const float3	BOX_CENTER = float3( 0, 0, 0 );
static const float3	BOX_HALF_SIZE = float3( 2, 0.5, 2 );	// 2x2 m� and 50cm height

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

#if SAMPLE_HEIGHT_MAP
float	SampleHeight( float2 _UV ) {
	return _tex_Height.SampleLevel( LinearClamp, _UV, 0.0 );
}
float3	SampleNormal( float2 _UV ) {
	return _tex_NormalMap.SampleLevel( LinearClamp, _UV, 0.0 );
}
#else

static const float	SC = 1.0;
//#define SC (250.0)
static const float2x2 m2 = float2x2(0.8,-0.6,0.6,0.8);

//float hash( const in float n ) {
//    return fract(sin(n)*43758.5453123);
//}
float hash( const in float2 p ) {
	float h = dot(p,float2(127.1,311.7));	
    return frac(sin(h)*43758.5453123);
}

// value noise, and its analytical derivatives
float3 noised( in float2 x ) {
    float2 f = frac(x);
    float2 u = f*f*(3.0-2.0*f);

#if 1
	int2 p = int2(floor(x));
    float a = hash( (p+int2(0,0)) );
    float b = hash( (p+int2(1,0)) );
    float c = hash( (p+int2(0,1)) );
    float d = hash( (p+int2(1,1)) );
#elif 1
    // texel fetch version
    int2 p = int2(floor(x));
    float a = texelFetch( iChannel0, (p+int2(0,0))&255, 0 ).x;
	float b = texelFetch( iChannel0, (p+int2(1,0))&255, 0 ).x;
	float c = texelFetch( iChannel0, (p+int2(0,1))&255, 0 ).x;
	float d = texelFetch( iChannel0, (p+int2(1,1))&255, 0 ).x;
#else    
    // texture version    
    float2 p = floor(x);
	float a = textureLod( iChannel0, (p+float2(0.5,0.5))/256.0, 0.0 ).x;
	float b = textureLod( iChannel0, (p+float2(1.5,0.5))/256.0, 0.0 ).x;
	float c = textureLod( iChannel0, (p+float2(0.5,1.5))/256.0, 0.0 ).x;
	float d = textureLod( iChannel0, (p+float2(1.5,1.5))/256.0, 0.0 ).x;
#endif
    
	return float3(a+(b-a)*u.x+(c-a)*u.y+(a-b-c+d)*u.x*u.y,
				6.0*f*(1.0-f)*(float2(b-a,c-a)+(a-b-c+d)*u.yx));
}

float terrainM( in float2 x ) {
	float2  p = x / SC;
    float a = 0.0;
    float b = 1.0;
	float2  d = 0.0;
    for( int i=0; i<9; i++ ) {
        float3 n = noised(p);
        d += n.yz;
        a += b*n.x/(1.0+dot(d,d));
		b *= 0.5;
        p = mul( m2, p*2.0 );
    }
	return 0.5 * SC*a;
}

float	SampleHeight( float2 _UV ) {
	return terrainM( 2.0 * 1.537989 * _UV );
}

float3	SampleNormal( float2 _UV ) {
	const float t = 1.0;
    float2	eps = float2( 0.002*t, 0.0 );
    return normalize( float3(	SampleHeight(_UV-eps.xy) - SampleHeight(_UV+eps.xy),
								SampleHeight(_UV-eps.yx) - SampleHeight(_UV+eps.yx),
								2.0*eps.x )
					);
}

#endif

float	ComputeHeightFieldIntersection( float3 _lsPosIn, float3 _lsDir, float _length, uint _stepsCount, inout float3 _lsNormal ) {
	float4	lsPos = float4( _lsPosIn, 0.0 );
	float4	lsStep = float4( _lsDir, 1.0 ) * _length / _stepsCount;

	float	prevH = _lsPosIn.y;
	[loop]
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		float2	UV = lsPos.xz;
		float	H = SampleHeight( UV );
		if ( lsPos.y < H ) {
			// Got a hit! Compute precise intersection...
			float	Z0 = lsPos.y - lsStep.y;
			float	Dz = lsStep.y;
			float	H0 = prevH;
			float	Dh = H - H0;
			float	t = (Z0 - H0) / (Dh - Dz);
			lsPos += (t-1.0) * lsStep;
			UV = lsPos.xz;
			_lsNormal = SampleNormal( UV );
			return lsPos.w;
		}
		lsPos += lsStep;
		prevH = H;
	}

	return 1e6;
}

Intersection	TraceScene( float3 _wsPos, float3 _wsDir ) {
	Intersection	result = (Intersection) 0;
	result.shade = 0;
	result.wsHitPosition = float4( _wsPos, 0.0 );

	// Test the bounding-box
	float2	hitDistance = float2( 1e6, 0 );
	uint2	hitSides;
	float2	testDistances = ComputeBoxIntersections( _wsPos, _wsDir, BOX_CENTER, 1.0 / BOX_HALF_SIZE, hitSides );

	float3	wsNormal = float3( 0, 1, 0 );
	if ( testDistances.x < testDistances.y ) {
		float3	wsPosIn = _wsPos + testDistances.x * _wsDir;
		float3	wsPosOut = _wsPos + testDistances.y * _wsDir;

		const float3	BBOX_MIN = BOX_CENTER - BOX_HALF_SIZE;
		float3	lsPosIn = (wsPosIn - BBOX_MIN) * 0.5 / BOX_HALF_SIZE;
		float3	lsPosOut = (wsPosOut - BBOX_MIN) * 0.5 / BOX_HALF_SIZE;
		float3	lsDir = lsPosOut - lsPosIn;
		float	L = length( lsDir );
				lsDir /= L;

		float3	lsNormal = 0;
		float	lsHitDistance = ComputeHeightFieldIntersection( lsPosIn, lsDir, L, 200, lsNormal );
		float3	lsHitPos = lsPosIn + lsHitDistance * lsDir;
		float3	wsHitPos = BBOX_MIN + lsHitPos * 2.0 * BOX_HALF_SIZE;

		hitDistance.x = length( wsHitPos - _wsPos );
//hitDistance.x = testDistances.x;
		wsNormal = float3( lsNormal.x, lsNormal.z, -lsNormal.y );
	}

	// Update result
	result.shade = step( 1e-5, hitDistance.x );
	result.wsHitPosition += hitDistance.x * float4( _wsDir, result.shade );	// W kept at 0 (invalid) if no hit
	result.wsNormal = wsNormal;
	result.albedo = 0.5 * float3( 1.0, 0.9, 0.7 );
	result.wsVelocity = 0;
	result.roughness = 0;
	result.F0 = 0;
	result.materialID = 0;

	return result;
}

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax ) {
//	LightInfoPoint	lightInfo;
//					lightInfo.flux = LIGHT_ILLUMINANCE;
//					lightInfo.wsPosition = GetPointLightPosition( lightInfo.distanceAttenuation );
//
	LightingResult	result = (LightingResult) 0;
//	ComputeLightPoint( _wsPosition, _wsNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}

float3			GetPointLightPosition( out float2 _distanceNearFar ) {
	_distanceNearFar = float2( 100.0, 1000.0 );	// Don't care, let natural 1/r� take care of attenuation
	return BOX_CENTER + float3( 0, 2, 0 );
}

#endif