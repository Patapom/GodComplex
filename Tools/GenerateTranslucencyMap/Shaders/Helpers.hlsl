////////////////////////////////////////////////////////////////////////////////
// Small helpers to finalize accumulation buffer and mix maps into a single map using a dominant hue
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2 degree observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

cbuffer	CBInput : register( b0 ) {
	uint	_Width;
	uint	_Height;
	float3	_Parm;
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture2D<float3>			_Source0 : register( t0 );
Texture2D<float3>			_Source1 : register( t1 );
Texture2D<float3>			_Source2 : register( t2 );
RWTexture2D<float4>			_Target : register( u0 );

// First shader simply multiplies accumulated values by the normalization factor (i.e. 1 / Rays#)
[numthreads( 16, 16, 1 )]
void	CS_Finalize( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelPosition = _DispatchThreadID.xy;
	if ( PixelPosition.x >= _Width || PixelPosition.y >= _Height )
		return;

	float3	V = _Source0.Load( uint3( PixelPosition, 0 ) );
	_Target[PixelPosition] = float4( _Parm * V, 1 );
}

// Second shader will mix the 3 RGB sources into a single RGB source using a dominant wavelength
[numthreads( 16, 16, 1 )]
void	CS_Mix( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelPosition = _DispatchThreadID.xy;
	if ( PixelPosition.x >= _Width || PixelPosition.y >= _Height )
		return;

	float3	V0 = _Source0.Load( uint3( PixelPosition, 0 ) );
	float3	V1 = _Source1.Load( uint3( PixelPosition, 0 ) );
	float3	V2 = _Source2.Load( uint3( PixelPosition, 0 ) );
	_Target[PixelPosition] = float4( dot( _Parm, V0 ), dot( _Parm, V1 ), dot( _Parm, V2 ), 1 );
}
