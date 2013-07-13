//////////////////////////////////////////////////////////////////////////
// This shader renders the Terrain lit by the Sun and sky
// (terrain code stolen from https://www.shadertoy.com/view/MdX3Rr)
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"
#include "Inc/Atmosphere.hlsl"

//#define	FUNKY_TERRAIN	// FONKIIII!


//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
	float4x4	_ObjectWorld2Proj;
	float3		_dUV;

	float		_TerrainHeight;
	float		_TerrainAlbedoMultiplier;
	float		_TerrainCloudShadowStrength;
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

	return (0.5*140.0) * a;
}

// Transforms the standard terrain height into terraces whose amplitude decreases with altitude
float	Map( in float3 p, const int _OctavesCount=14 )
{
#ifdef FUNKY_TERRAIN
//	p *= 1.2;

	float	h = GetTerrainHeight( p.xz, _OctavesCount );

	h = -50.0 * log( 4e-2 + saturate( 0.8 - 50.0 * pow( saturate( 0.2 + h / 140.0 ), 10.0 ) ) );

#else
	p *= 1.2;

	float	h = GetTerrainHeight( p.xz, _OctavesCount );

	float	ss = 0.03;
	float	hh = ss * h;
	float	fh = frac(hh);
	float	ih = floor(hh);
	fh = lerp( sqrt(fh), fh, smoothstep( 50.0, 140.0, h ) );	// Height in terraces varies with sqrt()
	h = (ih+fh) / ss;

#endif

	return h;
}

float3	CalcNormal( in float3 _Position, float _Distance )
{
#if 0
	float2	eps = float2( 0.01 * _Distance, 0.0 );	// Epsilon grows with distance to avoid normal aliasing...
	float3	Normal = float3(	Map( _Position + eps.xyy ) - Map( _Position - eps.xyy ),
								Map( _Position + eps.yxy ) - Map( _Position - eps.yxy ),
								Map( _Position + eps.yyx ) - Map( _Position - eps.yyx )
							);
#else
	float2	eps = float2( 0.002 * _Distance, 0.0 );	// Epsilon grows with distance to avoid normal aliasing...
	float3	Normal = -float3(	Map( _Position + eps.xyy ) - Map( _Position - eps.xyy ),
								-10.0 * eps.x,
								Map( _Position + eps.yyx ) - Map( _Position - eps.yyx )
							);
// 	float3	Dx = float3( 2.0 * eps.x, Map( _Position + eps.xyy ) - Map( _Position - eps.xyy ), 0.0 );
// 	float3	Dz = float3( 0.0, Map( _Position + eps.yyx ) - Map( _Position - eps.yyx ), 2.0 * eps.x );
// 	float3	Normal = cross( Dz, Dx );
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
		float	h = (_TerrainHeight/140.0) * Map( p, 5 );	// Use only 5 steps for shadow
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
	float3	TerrainPosition = _Position;
			TerrainPosition.y *= 140.0 / _TerrainHeight;	// Retrieve the terrain height expected by the original routine

	float3	Normal = CalcNormal( TerrainPosition, _Distance );
//return 4.0 * Normal;

	float	NdotL = saturate( dot( _LightDirection, Normal ) );		// Sun dot
//return NdotL;

	float3	Albedo = 0.0;

#if 1//def FUNKY_TERRAIN

//#else

	// Compute rock & grass albedo
	float	r = Noise( 200.0 * TerrainPosition.xz );

// 	const float3	RockColors[4] = {
// 		float3( 0.10, 0.05, 0.03 ),	// Striped rock - base color
// 		float3( 0.13, 0.10, 0.08 ),	// Striped rock - stripe color
// 		float3( 0.50, 0.23, 0.04 ),	// Sediment color
// 		float3( 0.20, 0.30, 0.00 )	// Grass color
// 	};
	const float3	RockColors[4] = {
		float3( 0.08, 0.10, 0.10 ),	// Striped rock - base color
		float3( 0.20, 0.20, 0.20 ),	// Striped rock - stripe color
		0.5*float3( 0.34, 0.30, 0.25 ),	// Sediment color
		float3( 0.20, 0.30, 0.00 )	// Grass color
	};

	Albedo = (r*0.25+0.75)*0.9 * lerp( RockColors[0], RockColors[1], saturate( GetTerrainHeight( float2(TerrainPosition.x, TerrainPosition.y*20.0 ) ) / 200.0 ) );
	Albedo = lerp( Albedo, 0.17 * RockColors[2] * (0.50+0.50*r), smoothstep( 0.70, 0.9, Normal.y ) );	// Add sediments on somewhat flat parts
	Albedo = lerp( Albedo, 0.10 * RockColors[3] * (0.25+0.75*r), smoothstep( 0.95, 1.0, Normal.y ) );	// Add grass on very flat parts
  	Albedo *= 0.75;
//return 50.0 * Albedo;

#if 1
	const float	SNOW_MIN_ALTITUDE = 50;	// Snowy in altitude

	// Add snow
	float	h = smoothstep( SNOW_MIN_ALTITUDE, 140.0, TerrainPosition.y + 25.0 * fbm( 0.01*TerrainPosition.xz ) );	// Very low frequency snow coverage depending on altitude
	float	e = smoothstep( 1.0-0.5*h, 1.0-0.1*h, Normal.y );		// Depends on flatness with tolerance varying with "snowiness" => very snowy is more tolerant to variations in normal
	float	o = 0.3 + 0.7 * smoothstep( 0.0, 0.1, Normal.x + h*h );	// Depends on wind orientation
	float	s = h * e * o;
			s = smoothstep( 0.1, 0.15, s );
	Albedo = lerp( Albedo, 0.5 * float3(0.6, 0.65, 0.7), s );
#endif

#endif

	Albedo *= _TerrainAlbedoMultiplier;

//return 50.0 * Albedo;

	// Compute shadowing by clouds
	float	CloudTransmittance = GetFastCloudTransmittance( _Position );
	CloudTransmittance = lerp( 1.0 - _TerrainCloudShadowStrength, 1.0, CloudTransmittance );
	_Shadow *= CloudTransmittance;

	// Build final color
	float3	Lighting  = NdotL * _Shadow * _SunColor;

	float3	Ambient = lerp( 0.8 * dot( _AmbientSkyColor, float3( 0.3, 0.5, 0.2 ) ), _AmbientSkyColor, CloudTransmittance );	// Make the ambient sky color become gray when in cloud shadow
			Lighting += lerp( 0.4, 1.0, Normal.y ) * Ambient;

	return INVPI * Albedo * Lighting;
}

PS_IN VS( VS_IN _In ) 
{ 
	PS_IN	Out;

	// Apply Terrain deformation
	//
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
			WorldPosition.y = (_TerrainHeight/140.0) * Map( WorldPosition.xyz, 5 );

	Out.__Position = mul( WorldPosition, _ObjectWorld2Proj );	// Use provided projection instead (because we also use this VS for the shadow map)
	Out.Position = WorldPosition.xyz;

	// Compute colored shadow
	float	Shadow = ShadowIntersect( WorldPosition.xyz, _LightDirection );	// Compute intersection with terrain for shadowing
//	Out.Shadow = float3( Shadow, Shadow*Shadow*0.5 + 0.5*Shadow, Shadow*Shadow );
	Out.Shadow = Shadow;
//	Out.Shadow = 1.0;

	// Compute Sun & Sky colors
	float3	EarthPositionKm = WORLD2KM * WorldPosition.xyz - EARTH_CENTER_KM;
	float	RadiusKm = length( EarthPositionKm );
	float	AltitudeKm = RadiusKm - GROUND_RADIUS_KM;
	float3	Normal = EarthPositionKm / RadiusKm;
	float	CosThetaSun = dot( Normal, _LightDirection );

	Out.SunColor = _SunIntensity * GetTransmittance( AltitudeKm, CosThetaSun );					// Sun light attenuated by the atmosphere
	Out.SkyColor = _SunIntensity * GetIrradiance( _TexIrradiance, AltitudeKm, CosThetaSun );	// Lighting by multiple-scattered light

	return Out;
} 

float	TempGetTerrainShadow( float3 _Position )
{
	float4	PositionProj = mul( float4( _Position, 1.0 ), _World2TerrainShadow );
//			PositionProj /= PositionProj.w;
	float2	UV = float2( 0.5 * (1.0 + PositionProj.x), 0.5 * (1.0 - PositionProj.y) );

	float	Zproj = _TexTerrainShadow.SampleLevel( LinearClamp, UV, 0.0 ).x;

return 0.001+Zproj > PositionProj.z ? 1.0 : 0.0;

	return saturate( 1.0 * (0.001 + PositionProj.z - Zproj) );
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	float	Distance2Camera = length( _In.Position - _Camera2World[3].xyz );

	float	Shadow = _In.Shadow;
//	float	Shadow = TempGetTerrainShadow( _In.Position );
	return float4( ComputeTerrainColor( _In.Position, Distance2Camera, Shadow, _In.SunColor, _In.SkyColor ), 1.0 );
}
