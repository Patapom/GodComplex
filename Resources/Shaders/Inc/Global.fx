////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
// WARNING! These must correspond to the resolution set in GodComplex.h !
static const float	RESX = 1280.0;
static const float	RESY = 720.0;
static const float	ASPECT_RATIO = RESX / RESY;
static const float2	SCREEN_SIZE = float2( RESX, RESY );
static const float2	INV_SCREEN_SIZE = float2( 1.0/RESX, 1.0/RESY );

static const float	PI = 3.1415926535897932384626433832795;			// ...
static const float	TWOPI = 6.283185307179586476925286766559;		// 2PI
static const float	HALFPI = 1.5707963267948966192313216916398;		// PI/2
static const float	RECIPI = 0.31830988618379067153776752674503;	// 1/PI

static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.patapom.com/index.php/Colorimetry)


#define TEX2D( Texture, Sampler, UV )					Texture.Sample( Sampler, UV.xy )
// On old ATIs, the SampleLevel() function doesn't work so you should use the other implementation (although I'm pretty sure it will fuck everything up if you start sampling textures within conditional branches)
#define TEX2DLOD( Texture, Sampler, UV, MipLevel )		Texture.SampleLevel( Sampler, UV.xy, MipLevel )
#define TEX3DLOD( Texture, Sampler, UVW, MipLevel )		Texture.SampleLevel( Sampler, UVW.xyz, MipLevel )
// #define TEX2DLOD( Texture, Sampler, UV, MipLevel )	Texture.Sample( Sampler, UV.xy )
// #define TEX3DLOD( Texture, Sampler, UVW, MipLevel )	Texture.Sample( Sampler, UVW.xyz )


////////////////////////////////////////////////////////////////////////////////////////
// Samplers
SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );


////////////////////////////////////////////////////////////////////////////////////////
// Constants

//[ // Minifier doesn't'support cbuffers !
cbuffer	cbCamera	: register( b0 )
{
	float4		_CameraData;
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Camera2Proj;
	float4x4	_ProjCamera;
	float4x4	_World2Proj;
	float4x4	_Proj2World;
};
//]
