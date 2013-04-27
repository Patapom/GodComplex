//////////////////////////////////////////////////////////////////////////
// This shader renders the Terrain lit by the Sun and sky
// (terrain code stolen from https://www.shadertoy.com/view/MdX3Rr)
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"
#include "Inc/Atmosphere.hlsl"

static const float	TERRAIN_HEIGHT = 6.0;	// Original value is 140
static const float	TERRAIN_FACTOR = TERRAIN_HEIGHT / 140.0;

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
	float4x4	_Unused;
	float2		_dUV;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Shadow		: SHADOW;
	float3	SunColor	: SUN_COLOR;
	float3	SkyColor	: SKY_COLOR;
};


float hash( float n )
{
	return frac( sin(n) * 43758.5453123 );
}


float Noise( in float3 x )
{
x += float3( 2.370, 0.0, -1.89897 );

	float3 p = floor(x);
	float3 f = frac(x);

	f = f*f*(3.0-2.0*f);

	float n = p.x + p.y*57.0 + 113.0*p.z;

	float res = lerp(	lerp(	lerp( hash( n +   0.0 ), hash( n +   1.0 ), f.x ),
								lerp( hash( n +  57.0 ), hash( n +  58.0 ), f.x ), f.y ),
						lerp(	lerp( hash( n + 113.0 ), hash( n + 114.0 ), f.x ),
								lerp( hash( n + 170.0 ), hash( n + 171.0 ), f.x ), f.y ), f.z );
	return res;
}

// Computes the noise + derivatives (cf. http://www.iquilezles.org/www/articles/morenoise/morenoise.htm)
float3 NoiseDeriv( in float2 x )
{
x += float2( 2.370, -1.89897 );

	float2	p = floor(x);
	float2	f = frac(x);

	float2	u = f*f*(3.0-2.0*f);

	float	n = p.x + p.y*57.0;

	float	a = hash( n +  0.0 );
	float	b = hash( n +  1.0 );
	float	c = hash( n + 57.0 );
	float	d = hash( n + 58.0 );
	return float3(	a + (b-a)*u.x + (c-a)*u.y + (a-b-c+d)*u.x*u.y,
					30.0 * f*f*(f*(f-2.0)+1.0) * (float2(b-a,c-a)+(a-b-c+d)*u.yx)
				);
}

float Noise( in float2 x )
{
x += float2( 2.370, -1.89897 );

	float2	p = floor(x);
	float2	f = frac(x);

	f = f*f*(3.0-2.0*f);

	float	n = p.x + p.y*57.0;

	return lerp(	lerp( hash( n +  0.0 ), hash( n +  1.0 ), f.x ),
					lerp( hash( n + 57.0 ), hash( n + 58.0 ), f.x ), f.y );
}

static const float3x3	Rot = float3x3(  0.00,  0.80,  0.60,
										-0.80,  0.36, -0.48,
										-0.60, -0.48,  0.64 );


float fbm( float3 p )
{
	float f = 0.0;

	f += 0.5000 * Noise( p ); p = mul( Rot, p*2.02 );
	f += 0.2500 * Noise( p ); p = mul( Rot, p*2.03 );
	f += 0.1250 * Noise( p ); p = mul( Rot, p*2.01 );
	f += 0.0625 * Noise( p );

	return f / 0.9375;
}

static const float2x2	Rot2 = float2x2( 1.6, -1.2,
										 1.2, 1.6 );
	
float fbm( float2 p )
{
	float f = 0.0;

	f += 0.5000 * Noise( p ); p = mul( Rot2, p*2.02 );
	f += 0.2500 * Noise( p ); p = mul( Rot2, p*2.03 );
	f += 0.1250 * Noise( p ); p = mul( Rot2, p*2.01 );
	f += 0.0625 * Noise( p );

	return f / 0.9375;
}

// Apply many octaves of fbm
float	GetTerrainHeight( in float2 x, const int _OctavesCount=14 )
{
	float2	p = x * 0.03;
	float	a = 0.0;
	float	b = 1.0;
	float2	d = 0.0;
	for ( int i=0; i < _OctavesCount; i++ )
	{
		float3 n = NoiseDeriv( p );
		d += n.yz;
		a += b * n.x / (1.0 + dot(d,d));	// Variation on noise with accumulation of derivatives (from http://www.iquilezles.org/www/articles/morenoise/morenoise.htm)
//		a += b * n.x;						// Use this for standard fbm
		b *= 0.5;
		p = mul( Rot2, p );
	}

	return 140.0 * a;
}

// Transforms the standard terrain height into terraces whose amplitude decreases with altitude
float	Map( in float3 p, const int _OctavesCount=14 )
{
	float	h = GetTerrainHeight( p.xz, _OctavesCount );	// Map to height it was originaly written for

	float	ss = 0.03;
	float	hh = ss * h;
	float	fh = frac(hh);
	float	ih = floor(hh);
	fh = lerp( sqrt(fh), fh, smoothstep( 50.0, 140.0, h ) );	// Height in terraces varies with sqrt()
	h = (ih+fh) / ss;

	return h;
}

float3	CalcNormal( in float3 _Position, float _Distance )
{
	float2	eps = float2( 0.0005 * _Distance, 0.0 );	// Epsilon grows with distance to avoid normal aliasing...

#if 0
	float3	Normal = float3(	Map( _Position + eps.xyy ) - Map( _Position - eps.xyy ),
								Map( _Position + eps.yxy ) - Map( _Position - eps.yxy ),
								Map( _Position + eps.yyx ) - Map( _Position - eps.yyx )
							);
#else
	float3	Normal = float3(	Map( _Position + eps.xyy ) - Map( _Position - eps.xyy ),
								20.0 * eps.x,
								Map( _Position + eps.yyx ) - Map( _Position - eps.yyx )
							);
#endif

	return normalize( Normal );
}

// Computes the shadow intersection
float	ShadowIntersect( float3 _Position, float3 _Light, uniform int _StepsCount=50 )
{
	float	res = 1.0;	// Current shadow value is fully lit
	float	t = 0.1;

	for ( int j=0; j < _StepsCount; j++ )
	{
		float3	p = _Position + t * _Light;
		float	h = TERRAIN_FACTOR * Map( p, 5 );	// Use only 5 steps for shadow
		float	Diff = p.y - h;

		res -= max( 0.0, -0.1 * Diff / t );	// Soft shadows brought by division with distance and difference of heights...
		if ( res < 0.01 )
			break;

		t += 0.25 * Diff;	// Advance by terrain height
	}

	return saturate( res );
}

float3	ComputeTerrainColor( float3 _Position, float _Distance, float3 _Shadow, float3 _SunColor, float3 _AmbientSkyColor )
{
//return _Shadow;
//return 0.2 * GetTerrainHeight( TerrainPosition );
//return 1.0 * Map( TerrainPosition );
// TerrainPosition.y *= 140.0 / TERRAIN_HEIGHT;
// return ShadowIntersect( TerrainPosition + _LightDirection * 20.0, _LightDirection );
//return _SunColor;

	float3	TerrainPosition = _Position;
			TerrainPosition.y /= TERRAIN_FACTOR;	// Retrieve the terrain height expected by the original routine

	float3	Normal = CalcNormal( TerrainPosition, _Distance );
//return 4.0 * Normal;

	float	NdotL = saturate( dot( _LightDirection, Normal ) );		// Sun dot
//	float	dif2 = saturate( 0.2 + 0.8 * dot( light2, Normal ) );	// Ambient dot
//return NdotL;

	float3	Albedo;

	// Compute rock & grass albedo
	float	r = Noise( 80.0 * TerrainPosition.xz );
	Albedo = (r*0.25+0.75)*0.9 * lerp( float3(0.10,0.05,0.03), float3(0.13,0.10,0.08), saturate( GetTerrainHeight( float2(TerrainPosition.x, TerrainPosition.y*20.0 ) ) / 200.0 ) );
	Albedo = lerp( Albedo, 0.17 * float3( 0.5, 0.23, 0.04)*(0.50+0.50*r), smoothstep( 0.70, 0.9, Normal.y ) );	// Add sediments on somewhat flat parts
	Albedo = lerp( Albedo, 0.10 * float3( 0.2, 0.30, 0.00)*(0.25+0.75*r), smoothstep( 0.95, 1.0, Normal.y ) );	// Add grass on very flat parts
  	Albedo *= 0.75;
//return 50.0 * Albedo;

#if 1
	// Add snow
//return fbm( 0.01*TerrainPosition.xz );
	float	h = smoothstep( 55.0, 140.0, TerrainPosition.y + 25.0 * fbm( 0.01*TerrainPosition.xz ) );	// Very low frequency snow coverage depending on altitude
	float	e = smoothstep( 1.0-0.5*h, 1.0-0.1*h, Normal.y );								// Depends on flatness with tolerance varying with "snowiness" => very snowy is more tolerant to variations in normal
	float	o = 0.3 + 0.7 * smoothstep( 0.0, 0.1, Normal.x + h*h );							// Depends on wind orientation
	float	s = h * e * o;
			s = smoothstep( 0.1, 0.15, s );
	Albedo = lerp( Albedo, 0.4 * float3(0.6, 0.65, 0.7), s );
#endif

//return 50.0 * Albedo;

// 	float3	brdf  = 2.0 * float3( 0.17, 0.19, 0.20 ) * saturate( Normal.y ) * _AmbientSkyColor;
// 			brdf += 6.0 * float3( 1.00, 0.95, 0.80 ) * _Shadow * NdotL * _SunColor;
// //			brdf += 2.0 * float3( 0.20, 0.20, 0.20 ) * dif2;

	// Compute shadowing by clouds
	_SunColor *= GetFastCloudTransmittance( _Position );

	// Build final color
	float3	brdf  = _Shadow * NdotL * _SunColor;
			brdf += lerp( 0.8, 1.0, Normal.y ) * _AmbientSkyColor;

	return Albedo * brdf;
}


PS_IN VS ( VS_IN _In ) 
{ 
	PS_IN	Out;

	// Apply Terrain deformation
	//
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
			WorldPosition.y = TERRAIN_FACTOR * Map( WorldPosition.xyz, 5 );

	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;

	// Compute colored shadow
	float	Shadow = ShadowIntersect( WorldPosition, _LightDirection );	// Compute intersection with terrain for shadowing
//	Out.Shadow = float3( Shadow, Shadow*Shadow*0.5 + 0.5*Shadow, Shadow*Shadow );
	Out.Shadow = Shadow;

	// Compute Sun & Sky colors
	float3	EarthPositionKm = WORLD2KM * WorldPosition.xyz - EARTH_CENTER_KM;
	float	RadiusKm = length( EarthPositionKm );
	float	AltitudeKm = RadiusKm - GROUND_RADIUS_KM;
	float3	Normal = EarthPositionKm / RadiusKm;
	float	CosThetaSun = dot( Normal, _LightDirection );

	Out.SunColor = SUN_INTENSITY * GetTransmittance( AltitudeKm, CosThetaSun );					// Sun light attenuated by the atmosphere
	Out.SkyColor = SUN_INTENSITY * GetIrradiance( _TexIrradiance, AltitudeKm, CosThetaSun );	// Lighting by multiple-scattered light

	return Out;
} 

float4	PS( PS_IN _In ) : SV_TARGET0
{
	float	Distance2Camera = length( _In.Position - _Camera2World[3].xyz );
	return float4( ComputeTerrainColor( _In.Position, Distance2Camera, _In.Shadow, _In.SunColor, _In.SkyColor ), 1.0 );
}
