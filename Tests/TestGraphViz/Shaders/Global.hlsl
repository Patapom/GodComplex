
static const float	SPRING_CONSTANT = 1.0;
static const float	DAMPING_CONSTANT = 0.0;

#define PI		3.1415926535897932384626433832795
#define INVPI	0.31830988618379067153776752674503

cbuffer CB_Main : register(b0) {
	uint	_nodesCount;
	uint2	_resolution;
};

struct SB_NodeSim {
	float2		m_position;
	float2		m_velocity;
};

struct SB_NodeInfo {
	float		m_mass;			// Node mass
	uint		m_linkOffset;	// Start link index in the links array
	uint		m_linksCount;	// Amount of links in the array
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)


float	pow2( float a ) { return a*a; }
float2	pow2( float2 a ) { return a*a; }
float3	pow2( float3 a ) { return a*a; }
float4	pow2( float4 a ) { return a*a; }
float	pow3( float a ) { return a*a*a; }
float2	pow3( float2 a ) { return a*a*a; }
float3	pow3( float3 a ) { return a*a*a; }
float4	pow3( float4 a ) { return a*a*a; }


// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
float ReverseBits( uint bits ) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
