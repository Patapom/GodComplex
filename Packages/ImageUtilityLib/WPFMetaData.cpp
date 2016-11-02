#include "ColorProfile.h"

using namespace ImageUtilityLib;

const float2	ColorProfile::ILLUMINANT_A( 0.44757f, 0.40745f );	// Incandescent, tungsten
const float2	ColorProfile::ILLUMINANT_D50( 0.34567f, 0.35850f );	// Daylight, Horizon
const float2	ColorProfile::ILLUMINANT_D55( 0.33242f, 0.34743f );	// Mid-Morning, Mid-Afternoon
const float2	ColorProfile::ILLUMINANT_D65( 0.31271f, 0.32902f );	// Daylight, Noon, Overcast (sRGB reference illuminant)
const float2	ColorProfile::ILLUMINANT_E( 1/3.0f, 1/3.0f );		// Reference

const float		ColorProfile::GAMMA_EXPONENT_sRGB = 2.4f;
const float		ColorProfile::GAMMA_EXPONENT_ADOBE = 2.19921875f;
const float		ColorProfile::GAMMA_EXPONENT_PRO_PHOTO = 1.8f;


const ColorProfile::Chromaticities::Empty		( float2(), float2(), float2(), float2() );
const ColorProfile::Chromaticities::sRGB		( float2( 0.6400f, 0.3300f ), float2( 0.3000f, 0.6000f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_D65 );
const ColorProfile::Chromaticities::AdobeRGB_D50( float2( 0.6400f, 0.3300f ), float2( 0.2100f, 0.7100f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_D50 );
const ColorProfile::Chromaticities::AdobeRGB_D65( float2( 0.6400f, 0.3300f ), float2( 0.2100f, 0.7100f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_D65 );
const ColorProfile::Chromaticities::ProPhoto	( float2( 0.7347f, 0.2653f ), float2( 0.1596f, 0.8404f ), float2( 0.0366f, 0.0001f ), ILLUMINANT_D50 );
const ColorProfile::Chromaticities::Radiance	( float2( 0.6400f, 0.3300f ), float2( 0.2900f, 0.6000f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_E );


float4x4	ColorProfile::InternalColorConverter_sRGB::MAT_RGB2XYZ = new float4x4( new float[] {
			0.4124f, 0.2126f, 0.0193f, 0.0f,
			0.3576f, 0.7152f, 0.1192f, 0.0f,
			0.1805f, 0.0722f, 0.9505f, 0.0f,
			0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
		} );

float4x4	ColorProfile::InternalColorConverter_sRGB::MAT_XYZ2RGB = new float4x4( new float[] {
			3.2406f, -0.9689f,  0.0557f, 0.0f,
			-1.5372f,  1.8758f, -0.2040f, 0.0f,
			-0.4986f,  0.0415f,  1.0570f, 0.0f,
			0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
		} );

float4 ColorProfile::InternalColorConverter_sRGB::XYZ2RGB( float4 _XYZ ) {
	// Transform into RGB
	_XYZ = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_XYZ.x = _XYZ.x > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _XYZ.x;
	_XYZ.y = _XYZ.y > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _XYZ.y;
	_XYZ.z = _XYZ.z > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _XYZ.z;

	return _XYZ;
}

float4 ColorProfile::InternalColorConverter_sRGB::RGB2XYZ( float4 _RGB ) {
		// Gamma un-correct
		_RGB.x = _RGB.x < 0.04045f ? _RGB.x / 12.92f : (float) Math.Pow( (_RGB.x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_RGB.y = _RGB.y < 0.04045f ? _RGB.y / 12.92f : (float) Math.Pow( (_RGB.y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_RGB.z = _RGB.z < 0.04045f ? _RGB.z / 12.92f : (float) Math.Pow( (_RGB.z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );

		// Transform into XYZ
		return _RGB * MAT_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ =  XYZ * MAT_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = XYZ.x > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * XYZ.x;
				_RGB[X,Y].y = XYZ.y > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * XYZ.y;
				_RGB[X,Y].z = XYZ.z > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * XYZ.z;
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = RGB.x < 0.04045f ? RGB.x / 12.92f : (float) Math.Pow( (RGB.x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
				RGB.y = RGB.y < 0.04045f ? RGB.y / 12.92f : (float) Math.Pow( (RGB.y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
				RGB.z = RGB.z < 0.04045f ? RGB.z / 12.92f : (float) Math.Pow( (RGB.z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );

				// Transform into XYZ
				_XYZ[X,Y] =  RGB *  MAT_RGB2XYZ;
			}
	}

	#pragma endregion
}

class		InternalColorConverter_AdobeRGB_D50 : IColorConverter {
	#pragma region CONSTANTS

	public static readonly float4x4	MAT_RGB2XYZ = new float4x4( new float[] {
			0.60974f, 0.31111f, 0.01947f, 0.0f,
			0.20528f, 0.62567f, 0.06087f, 0.0f,
			0.14919f, 0.06322f, 0.74457f, 0.0f,
			0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
		} );

	public static readonly float4x4	MAT_XYZ2RGB = new float4x4( new float[] {
				1.96253f, -0.97876f,  0.02869f, 0.0f,
			-0.61068f,  1.91615f, -0.14067f, 0.0f,
			-0.34137f,  0.03342f,  1.34926f, 0.0f,
				0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
		} );

	#pragma endregion

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		_XYZ = _XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_XYZ.x = (float) Math.Pow( _XYZ.x, 1.0f / GAMMA_EXPONENT_ADOBE );
		_XYZ.y = (float) Math.Pow( _XYZ.y, 1.0f / GAMMA_EXPONENT_ADOBE );
		_XYZ.z = (float) Math.Pow( _XYZ.z, 1.0f / GAMMA_EXPONENT_ADOBE );

		return _XYZ;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Gamma un-correct
		_RGB.x = (float) Math.Pow( _RGB.x, GAMMA_EXPONENT_ADOBE );
		_RGB.y = (float) Math.Pow( _RGB.y, GAMMA_EXPONENT_ADOBE );
		_RGB.z = (float) Math.Pow( _RGB.z, GAMMA_EXPONENT_ADOBE );

		// Transform into XYZ
		return _RGB * MAT_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ = XYZ * MAT_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = (float) Math.Pow( XYZ.x, 1.0f / GAMMA_EXPONENT_ADOBE );
				_RGB[X,Y].y = (float) Math.Pow( XYZ.y, 1.0f / GAMMA_EXPONENT_ADOBE );
				_RGB[X,Y].z = (float) Math.Pow( XYZ.z, 1.0f / GAMMA_EXPONENT_ADOBE );
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = (float) Math.Pow( RGB.x, GAMMA_EXPONENT_ADOBE );
				RGB.y = (float) Math.Pow( RGB.y, GAMMA_EXPONENT_ADOBE );
				RGB.z = (float) Math.Pow( RGB.z, GAMMA_EXPONENT_ADOBE );

				// Transform into XYZ
				_XYZ[X,Y] = RGB * MAT_RGB2XYZ;
			}
	}

	#pragma endregion
}

class		InternalColorConverter_AdobeRGB_D65 : IColorConverter {
	#pragma region CONSTANTS

	public static readonly float4x4	MAT_RGB2XYZ = new float4x4( new float[] {
			0.57667f, 0.29734f, 0.02703f, 0.0f,
			0.18556f, 0.62736f, 0.07069f, 0.0f,
			0.18823f, 0.07529f, 0.99134f, 0.0f,
			0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
		} );

	public static readonly float4x4	MAT_XYZ2RGB = new float4x4( new float[] {
				2.04159f, -0.96924f,  0.01344f, 0.0f,
			-0.56501f,  1.87597f, -0.11836f, 0.0f,
			-0.34473f,  0.04156f,  1.01517f, 0.0f,
				0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
		} );

	#pragma endregion

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		_XYZ = _XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_XYZ.x = (float) Math.Pow( _XYZ.x, 1.0f / GAMMA_EXPONENT_ADOBE );
		_XYZ.y = (float) Math.Pow( _XYZ.y, 1.0f / GAMMA_EXPONENT_ADOBE );
		_XYZ.z = (float) Math.Pow( _XYZ.z, 1.0f / GAMMA_EXPONENT_ADOBE );

		return _XYZ;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Gamma un-correct
		_RGB.x = (float) Math.Pow( _RGB.x, GAMMA_EXPONENT_ADOBE );
		_RGB.y = (float) Math.Pow( _RGB.y, GAMMA_EXPONENT_ADOBE );
		_RGB.z = (float) Math.Pow( _RGB.z, GAMMA_EXPONENT_ADOBE );

		// Transform into XYZ
		return _RGB * MAT_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ = XYZ * MAT_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = (float) Math.Pow( XYZ.x, 1.0f / GAMMA_EXPONENT_ADOBE );
				_RGB[X,Y].y = (float) Math.Pow( XYZ.y, 1.0f / GAMMA_EXPONENT_ADOBE );
				_RGB[X,Y].z = (float) Math.Pow( XYZ.z, 1.0f / GAMMA_EXPONENT_ADOBE );
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = (float) Math.Pow( RGB.x, GAMMA_EXPONENT_ADOBE );
				RGB.y = (float) Math.Pow( RGB.y, GAMMA_EXPONENT_ADOBE );
				RGB.z = (float) Math.Pow( RGB.z, GAMMA_EXPONENT_ADOBE );

				// Transform into XYZ
				_XYZ[X,Y] = RGB * MAT_RGB2XYZ;
			}
	}

	#pragma endregion
}

class		InternalColorConverter_ProPhoto : IColorConverter {
	#pragma region CONSTANTS

	public static readonly float4x4	MAT_RGB2XYZ = new float4x4( new float[] {
			0.7977f, 0.2880f, 0.0000f, 0.0f,
			0.1352f, 0.7119f, 0.0000f, 0.0f,
			0.0313f, 0.0001f, 0.8249f, 0.0f,
			0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
		} );

	public static readonly float4x4	MAT_XYZ2RGB = new float4x4( new float[] {
				1.3460f, -0.5446f,  0.0000f, 0.0f,
			-0.2556f,  1.5082f,  0.0000f, 0.0f,
			-0.0511f,  0.0205f,  1.2123f, 0.0f,
				0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
		} );

	#pragma endregion

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		_XYZ = _XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_XYZ.x = _XYZ.x > 0.001953f ? (float) Math.Pow( _XYZ.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _XYZ.x;
		_XYZ.y = _XYZ.y > 0.001953f ? (float) Math.Pow( _XYZ.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _XYZ.y;
		_XYZ.z = _XYZ.z > 0.001953f ? (float) Math.Pow( _XYZ.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _XYZ.z;

		return _XYZ;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Gamma un-correct
		_RGB.x = _RGB.x > 0.031248f ? (float) Math.Pow( _RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.x / 16.0f;
		_RGB.y = _RGB.y > 0.031248f ? (float) Math.Pow( _RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.y / 16.0f;
		_RGB.z = _RGB.z > 0.031248f ? (float) Math.Pow( _RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.z / 16.0f;

		// Transform into XYZ
		return _RGB * MAT_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ = XYZ * MAT_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = XYZ.x > 0.001953f ? (float) Math.Pow( XYZ.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * XYZ.x;
				_RGB[X,Y].y = XYZ.y > 0.001953f ? (float) Math.Pow( XYZ.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * XYZ.y;
				_RGB[X,Y].z = XYZ.z > 0.001953f ? (float) Math.Pow( XYZ.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * XYZ.z;
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = RGB.x > 0.031248f ? (float) Math.Pow( RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : RGB.x / 16.0f;
				RGB.y = RGB.y > 0.031248f ? (float) Math.Pow( RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : RGB.y / 16.0f;
				RGB.z = RGB.z > 0.031248f ? (float) Math.Pow( RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : RGB.z / 16.0f;

				// Transform into XYZ
				_XYZ[X,Y] = RGB * MAT_RGB2XYZ;
			}
	}

	#pragma endregion
}

class		InternalColorConverter_Radiance : IColorConverter {
	#pragma region CONSTANTS

	public static readonly float4x4	MAT_RGB2XYZ = new float4x4( new float[] {
			0.5141447f, 0.2651059f, 0.0241005f, 0.0f,
			0.3238845f, 0.6701059f, 0.1228527f, 0.0f,
			0.1619709f, 0.0647883f, 0.8530467f, 0.0f,
			0.0000000f, 0.0000000f, 0.0000000f, 1.0f		// Alpha stays the same
		} );

	public static readonly float4x4	MAT_XYZ2RGB = new float4x4( new float[] {
				2.5653124f, -1.02210832f,  0.07472437f, 0.0f,
			-1.1668493f,  1.97828662f, -0.25193953f, 0.0f,
			-0.3984632f,  0.04382159f,  1.17721522f, 0.0f,
				0.0000000f,  0.00000000f,  0.00000000f, 1.0f	// Alpha stays the same
		} );

	#pragma endregion

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		return _XYZ * MAT_XYZ2RGB;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Transform into XYZ
		return _RGB * MAT_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
				_RGB[X,Y] = _XYZ[X,Y] * MAT_XYZ2RGB;
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
				_XYZ[X,Y] = _RGB[X,Y] * MAT_RGB2XYZ;
	}

	#pragma endregion
}

class		InternalColorConverter_Generic_NoGamma : IColorConverter {
	protected float4x4	m_RGB2XYZ;
	protected float4x4	m_XYZ2RGB;

	public InternalColorConverter_Generic_NoGamma( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB )
	{
		m_RGB2XYZ = _RGB2XYZ;
		m_XYZ2RGB = _XYZ2RGB;
	}

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		return _XYZ * m_XYZ2RGB;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Transform into XYZ
		return _RGB * m_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
				_RGB[X,Y] = _XYZ[X,Y] * m_XYZ2RGB;
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
				_XYZ[X,Y] = _RGB[X,Y] * m_RGB2XYZ;
	}

	#pragma endregion
}

class		InternalColorConverter_Generic_StandardGamma : IColorConverter {
	protected float4x4	m_RGB2XYZ;
	protected float4x4	m_XYZ2RGB;
	protected float		m_Gamma = 1.0f;
	protected float		m_InvGamma = 1.0f;

	public InternalColorConverter_Generic_StandardGamma( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB, float _Gamma )
	{
		m_RGB2XYZ = _RGB2XYZ;
		m_XYZ2RGB = _XYZ2RGB;
		m_Gamma = _Gamma;
		m_InvGamma = 1.0f / _Gamma;
	}

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		_XYZ = _XYZ * m_XYZ2RGB;

		// Gamma correct
		_XYZ.x = (float) Math.Pow( _XYZ.x, m_InvGamma );
		_XYZ.y = (float) Math.Pow( _XYZ.y, m_InvGamma );
		_XYZ.z = (float) Math.Pow( _XYZ.z, m_InvGamma );

		return _XYZ;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Gamma un-correct
		_RGB.x = (float) Math.Pow( _RGB.x, m_Gamma );
		_RGB.y = (float) Math.Pow( _RGB.y, m_Gamma );
		_RGB.z = (float) Math.Pow( _RGB.z, m_Gamma );

		// Transform into XYZ
		return _RGB * m_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ = XYZ * m_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = (float) Math.Pow( XYZ.x, m_InvGamma );
				_RGB[X,Y].y = (float) Math.Pow( XYZ.y, m_InvGamma );
				_RGB[X,Y].z = (float) Math.Pow( XYZ.z, m_InvGamma );
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = (float) Math.Pow( RGB.x, m_Gamma );
				RGB.y = (float) Math.Pow( RGB.y, m_Gamma );
				RGB.z = (float) Math.Pow( RGB.z, m_Gamma );

				// Transform into XYZ
				_XYZ[X,Y] = RGB * m_RGB2XYZ;
			}
	}

	#pragma endregion
}

class		InternalColorConverter_Generic_sRGBGamma : IColorConverter {
	protected float4x4	m_RGB2XYZ;
	protected float4x4	m_XYZ2RGB;

	public InternalColorConverter_Generic_sRGBGamma( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB )
	{
		m_RGB2XYZ = _RGB2XYZ;
		m_XYZ2RGB = _XYZ2RGB;
	}

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		_XYZ = _XYZ * m_XYZ2RGB;

		// Gamma correct
		_XYZ.x = _XYZ.x > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _XYZ.x;
		_XYZ.y = _XYZ.y > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _XYZ.y;
		_XYZ.z = _XYZ.z > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _XYZ.z;

		return _XYZ;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Gamma un-correct
		_RGB.x = _RGB.x < 0.04045f ? _RGB.x / 12.92f : (float) Math.Pow( (_RGB.x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_RGB.y = _RGB.y < 0.04045f ? _RGB.y / 12.92f : (float) Math.Pow( (_RGB.y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_RGB.z = _RGB.z < 0.04045f ? _RGB.z / 12.92f : (float) Math.Pow( (_RGB.z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );

		// Transform into XYZ
		return _RGB * m_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ = XYZ * m_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = XYZ.x > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * XYZ.x;
				_RGB[X,Y].y = XYZ.y > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * XYZ.y;
				_RGB[X,Y].z = XYZ.z > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * XYZ.z;
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = RGB.x < 0.04045f ? RGB.x / 12.92f : (float) Math.Pow( (RGB.x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
				RGB.y = RGB.y < 0.04045f ? RGB.y / 12.92f : (float) Math.Pow( (RGB.y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
				RGB.z = RGB.z < 0.04045f ? RGB.z / 12.92f : (float) Math.Pow( (RGB.z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );

				// Transform into XYZ
				_XYZ[X,Y] = RGB * m_RGB2XYZ;
			}
	}

	#pragma endregion
}

class		InternalColorConverter_Generic_ProPhoto : IColorConverter {
	protected float4x4	m_RGB2XYZ;
	protected float4x4	m_XYZ2RGB;

	public InternalColorConverter_Generic_ProPhoto( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB )
	{
		m_RGB2XYZ = _RGB2XYZ;
		m_XYZ2RGB = _XYZ2RGB;
	}

	#pragma region IColorConverter Members

	public float4 XYZ2RGB( float4 _XYZ )
	{
		// Transform into RGB
		_XYZ = _XYZ * m_XYZ2RGB;

		// Gamma correct
		_XYZ.x = _XYZ.x > 0.001953f ? (float) Math.Pow( _XYZ.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _XYZ.x;
		_XYZ.y = _XYZ.y > 0.001953f ? (float) Math.Pow( _XYZ.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _XYZ.y;
		_XYZ.z = _XYZ.z > 0.001953f ? (float) Math.Pow( _XYZ.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _XYZ.z;

		return _XYZ;
	}

	public float4 RGB2XYZ( float4 _RGB )
	{
		// Gamma un-correct
		_RGB.x = _RGB.x > 0.031248f ? (float) Math.Pow( _RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.x / 16.0f;
		_RGB.y = _RGB.y > 0.031248f ? (float) Math.Pow( _RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.y / 16.0f;
		_RGB.z = _RGB.z > 0.031248f ? (float) Math.Pow( _RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.z / 16.0f;

		// Transform into XYZ
		return _RGB * m_RGB2XYZ;
	}

	public void XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
	{
		int		W = _XYZ.GetLength( 0 );
		int		H = _XYZ.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	XYZ = _XYZ[X,Y];

				// Transform into RGB
				XYZ = XYZ * m_XYZ2RGB;

				// Gamma correct
				_RGB[X,Y].x = XYZ.x > 0.001953f ? (float) Math.Pow( XYZ.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * XYZ.x;
				_RGB[X,Y].y = XYZ.y > 0.001953f ? (float) Math.Pow( XYZ.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * XYZ.y;
				_RGB[X,Y].z = XYZ.z > 0.001953f ? (float) Math.Pow( XYZ.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * XYZ.z;
				_RGB[X,Y].w = XYZ.w;
			}
	}

	public void RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
	{
		int		W = _RGB.GetLength( 0 );
		int		H = _RGB.GetLength( 1 );
		for ( int Y=0; Y < H; Y++ )
			for ( int X=0; X < W; X++ )
			{
				float4	RGB = _RGB[X,Y];

				// Gamma un-correct
				RGB.x = RGB.x > 0.031248f ? (float) Math.Pow( RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : RGB.x / 16.0f;
				RGB.y = RGB.y > 0.031248f ? (float) Math.Pow( RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : RGB.y / 16.0f;
				RGB.z = RGB.z > 0.031248f ? (float) Math.Pow( RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : RGB.z / 16.0f;

				// Transform into XYZ
				_XYZ[X,Y] = RGB * m_RGB2XYZ;
			}
	}

	#pragma endregion
}
