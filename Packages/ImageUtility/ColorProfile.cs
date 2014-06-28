//////////////////////////////////////////////////////////////////////////
// This special Bitmap class handles many image formats (JPG, PNG, BMP, TGA, GIF, HDR and especially RAW camera formats)
// It also carefully handles color profiles to provide a faithful internal image representation that is always
//	stored as 32-bits floating point precision CIE XYZ device-independent format that you can later convert to
//	any other format.
//
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

// TODO !!! => Avoir la possibilité de créer une texture avec un seul channel du bitmap !? (filtrage)
//			=> Mettre une option pour "premultiplied alpha"

namespace ImageUtility
{

	/// <summary>
	/// Defines a color converter that can handle transforms between XYZ and RGB
	/// Usually implemented by a ColorProfile so the RGB color is fully characterized
	/// </summary>
	public interface IColorConverter
	{
		float4		XYZ2RGB( float4 _XYZ );
		float4		RGB2XYZ( float4 _RGB );
		void		XYZ2RGB( float4[,] _XYZ, float4[,] _RGB );
		void		RGB2XYZ( float4[,] _RGB, float4[,] _XYZ );
	}

	/// <summary>
	/// The source color for the bitmap
	/// The color profile helps converting between the original color space and the internal CIEXYZ color space used in the Bitmap class
	/// 
	/// For now, only standard profiles like Linear, sRGB, Adobe RGB, ProPhoto RGB or any custom chromaticities are supported.
	/// I believe it would be overkill to include a library for parsing embedded ICC profiles...
	/// </summary>
	public class	ColorProfile : IColorConverter
	{
		#region CONSTANTS

		public static readonly float2	ILLUMINANT_A	= new float2( 0.44757f, 0.40745f );	// Incandescent, tungsten
		public static readonly float2	ILLUMINANT_D50	= new float2( 0.34567f, 0.35850f );	// Daylight, Horizon
		public static readonly float2	ILLUMINANT_D55	= new float2( 0.33242f, 0.34743f );	// Mid-Morning, Mid-Afternoon
		public static readonly float2	ILLUMINANT_D65	= new float2( 0.31271f, 0.32902f );	// Daylight, Noon, Overcast (sRGB reference illuminant)
		public static readonly float2	ILLUMINANT_E	= new float2( 1/3.0f, 1/3.0f );		// Reference

		public const float				GAMMA_EXPONENT_sRGB = 2.4f;
		public const float				GAMMA_EXPONENT_ADOBE = 2.19921875f;
		public const float				GAMMA_EXPONENT_PRO_PHOTO = 1.8f;

		#endregion

		#region NESTED TYPES

		public enum STANDARD_PROFILE
		{
			INVALID,		// The profile is invalid (meaning one of the chromaticities was not initialized!)
			CUSTOM,			// No recognizable standard profile (custom)
			sRGB,			// sRGB with D65 illuminant
			ADOBE_RGB_D50,	// Adobe RGB with D50 illuminant
			ADOBE_RGB_D65,	// Adobe RGB with D65 illuminant
			PRO_PHOTO,		// ProPhoto with D50 illuminant
			RADIANCE,		// Radiance HDR format with E illuminant
		}

		/// <summary>
		/// Enumerates the various supported gamma curves
		/// </summary>
		public enum GAMMA_CURVE
		{
			STANDARD,		// Standard gamma curve using a single exponent and no linear slope
			sRGB,			// sRGB gamma with linear slope
			PRO_PHOTO,		// ProPhoto gamma with linear slope
		}

		/// <summary>
		/// Describes the Red, Green, Blue and White Point chromaticities of a simple/standard color profile
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "R=({R.x},{R.y}) G=({G.x},{G.y}) B=({B.x},{B.y}) W=({W.x},{W.y}) Prof={RecognizedProfile}" )]
		public struct	Chromaticities
		{
			public float2		R, G, B, W;

			public Chromaticities( float xr, float yr, float xg, float yg, float xb, float yb, float xw, float yw )
			{
				R = new float2( xr, yr );
				G = new float2( xg, yg );
				B = new float2( xb, yb );
				W = new float2( xw, yw );
			}

			public static Chromaticities	Empty			{ get { return new Chromaticities() { R = new float2(), G = new float2(), B = new float2(), W = new float2() }; } }
			public static Chromaticities	sRGB			{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.3000f, 0.6000f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_D65 }; } }
			public static Chromaticities	AdobeRGB_D50	{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.2100f, 0.7100f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_D50 }; } }
			public static Chromaticities	AdobeRGB_D65	{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.2100f, 0.7100f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_D65 }; } }
			public static Chromaticities	ProPhoto		{ get { return new Chromaticities() { R = new float2( 0.7347f, 0.2653f ), G = new float2( 0.1596f, 0.8404f ), B = new float2( 0.0366f, 0.0001f ), W = ILLUMINANT_D50 }; } }
			public static Chromaticities	Radiance		{ get { return new Chromaticities() { R = new float2( 0.6400f, 0.3300f ), G = new float2( 0.2900f, 0.6000f ), B = new float2( 0.1500f, 0.0600f ), W = ILLUMINANT_E }; } }

			/// <summary>
			/// Attempts to recognize the current chromaticities as a standard profile
			/// </summary>
			/// <returns></returns>
			public STANDARD_PROFILE	RecognizedProfile
			{
				get
				{
					if ( Equals( sRGB ) )
						return STANDARD_PROFILE.sRGB;
					if ( Equals( AdobeRGB_D65 ) )
						return STANDARD_PROFILE.ADOBE_RGB_D65;
					if ( Equals( AdobeRGB_D50 ) )
						return STANDARD_PROFILE.ADOBE_RGB_D50;
					if ( Equals( ProPhoto ) )
						return STANDARD_PROFILE.PRO_PHOTO;
					if ( Equals( Radiance ) )
						return STANDARD_PROFILE.RADIANCE;

					// Ensure the profile is valid
					return R.x != 0.0f && R.y != 0.0f && G.x != 0.0f && G.y != 0.0f && B.x != 0.0f && B.y != 0.0f && W.x != 0.0f && W.y != 0.0f ? STANDARD_PROFILE.CUSTOM : STANDARD_PROFILE.INVALID;
				}
			}

			private bool	Equals( Chromaticities other )
			{
				return Equals( R, other.R ) && Equals( G, other.G ) && Equals( B, other.B ) && Equals( W, other.W );
			}
			private const float	EPSILON = 1e-3f;
			private bool	Equals( float2 a, float2 b )
			{
				return Math.Abs( a.x - b.x ) < EPSILON && Math.Abs( a.y - b.y ) < EPSILON;
			}
		}

		#region Internal XYZ<->RGB Converters

		protected class		InternalColorConverter_sRGB : IColorConverter
		{
			#region CONSTANTS

			public static readonly float4x4	MAT_RGB2XYZ = new float4x4( new float[] {
					0.4124f, 0.2126f, 0.0193f, 0.0f,
					0.3576f, 0.7152f, 0.1192f, 0.0f,
					0.1805f, 0.0722f, 0.9505f, 0.0f,
					0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
				} );

			public static readonly float4x4	MAT_XYZ2RGB = new float4x4( new float[] {
						3.2406f, -0.9689f,  0.0557f, 0.0f,
					-1.5372f,  1.8758f, -0.2040f, 0.0f,
					-0.4986f,  0.0415f,  1.0570f, 0.0f,
						0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
				} );

			#endregion

			#region IColorConverter Members

			public float4 XYZ2RGB( float4 _XYZ )
			{
				// Transform into RGB
				_XYZ = _XYZ * MAT_XYZ2RGB;

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

			#endregion
		}

		protected class		InternalColorConverter_AdobeRGB_D50 : IColorConverter
		{
			#region CONSTANTS

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

			#endregion

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_AdobeRGB_D65 : IColorConverter
		{
			#region CONSTANTS

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

			#endregion

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_ProPhoto : IColorConverter
		{
			#region CONSTANTS

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

			#endregion

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_Radiance : IColorConverter
		{
			#region CONSTANTS

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

			#endregion

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_Generic_NoGamma : IColorConverter
		{
			protected float4x4	m_RGB2XYZ;
			protected float4x4	m_XYZ2RGB;

			public InternalColorConverter_Generic_NoGamma( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB )
			{
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
			}

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_Generic_StandardGamma : IColorConverter
		{
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

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_Generic_sRGBGamma : IColorConverter
		{
			protected float4x4	m_RGB2XYZ;
			protected float4x4	m_XYZ2RGB;

			public InternalColorConverter_Generic_sRGBGamma( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB )
			{
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
			}

			#region IColorConverter Members

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

			#endregion
		}

		protected class		InternalColorConverter_Generic_ProPhoto : IColorConverter
		{
			protected float4x4	m_RGB2XYZ;
			protected float4x4	m_XYZ2RGB;

			public InternalColorConverter_Generic_ProPhoto( float4x4 _RGB2XYZ, float4x4 _XYZ2RGB )
			{
				m_RGB2XYZ = _RGB2XYZ;
				m_XYZ2RGB = _XYZ2RGB;
			}

			#region IColorConverter Members

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

			#endregion
		}

		#endregion

		#endregion

		#region FIELDS

		protected bool				m_bProfileFoundInFile = false;
		protected Chromaticities	m_Chromaticities = Chromaticities.Empty;
		protected GAMMA_CURVE		m_GammaCurve = GAMMA_CURVE.STANDARD;
		protected float				m_Gamma = 1.0f;
		protected float				m_Exposure = 0.0f;

		protected float4x4			m_RGB2XYZ = float4x4.Identity;
		protected float4x4			m_XYZ2RGB = float4x4.Identity;

		protected IColorConverter	m_InternalConverter = null;
 
		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets the chromaticities attached to the profile
		/// </summary>
		public Chromaticities	Chromas					{ get { return m_Chromaticities; } }

		/// <summary>
		/// Gets the transform to convert RGB to CIEXYZ
		/// </summary>
		public float4x4		MatrixRGB2XYZ			{ get { return m_RGB2XYZ; } }

		/// <summary>
		/// Gets the transform to convert CIEXYZ to RGB
		/// </summary>
		public float4x4		MatrixXYZ2RGB			{ get { return m_XYZ2RGB; } }

		/// <summary>
		/// Gets or sets the image gamma curve
		/// </summary>
		public GAMMA_CURVE		GammaCurve				{ get { return m_GammaCurve; } set { m_GammaCurve = value; BuildTransformFromChroma( true ); } }

		/// <summary>
		/// Gets or sets the image gamma
		/// </summary>
		public float			Gamma					{ get { return m_Gamma; } set { m_Gamma = value; BuildTransformFromChroma( true ); } }

		/// <summary>
		/// Gets or sets the image exposure (usually for HDR images)
		/// </summary>
		public float			Exposure				{ get { return m_Exposure; } set { m_Exposure = value; } }

		/// <summary>
		/// True if the profile was found in the file's metadata and can be considered accurate.
		/// False if it's the default assumed profile and may NOT be the actual image's profile.
		/// </summary>
		public bool				ProfileFoundInFile		{ get { return m_bProfileFoundInFile; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Build from a standard profile
		/// </summary>
		/// <param name="_Profile"></param>
		public ColorProfile( STANDARD_PROFILE _Profile )
		{
			switch ( _Profile )
			{
				case STANDARD_PROFILE.sRGB:
					m_Chromaticities = Chromaticities.sRGB;
					m_GammaCurve = GAMMA_CURVE.sRGB;
					m_Gamma = GAMMA_EXPONENT_sRGB;
					break;
				case STANDARD_PROFILE.ADOBE_RGB_D50:
					m_Chromaticities = Chromaticities.AdobeRGB_D50;
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = GAMMA_EXPONENT_ADOBE;
					break;
				case STANDARD_PROFILE.ADOBE_RGB_D65:
					m_Chromaticities = Chromaticities.AdobeRGB_D65;
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = GAMMA_EXPONENT_ADOBE;
					break;
				case STANDARD_PROFILE.PRO_PHOTO:
					m_Chromaticities = Chromaticities.ProPhoto;
					m_GammaCurve = GAMMA_CURVE.PRO_PHOTO;
					m_Gamma = GAMMA_EXPONENT_PRO_PHOTO;
					break;
				case STANDARD_PROFILE.RADIANCE:
					m_Chromaticities = Chromaticities.Radiance;
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 1.0f;
					break;
				default:
					throw new Exception( "Unsupported standard profile!" );
			}

			BuildTransformFromChroma( true );
		}

		/// <summary>
		/// Creates the color profile from metadata embedded in the image file
		/// </summary>
		/// <param name="_MetaData"></param>
		/// <param name="_FileType"></param>
		public ColorProfile( BitmapMetadata _MetaData, Bitmap.FILE_TYPE _FileType )
		{
			string	MetaDump = _MetaData != null ? DumpMetaData( _MetaData ) : null;

			bool	bGammaFoundInFile = false;
			switch ( _FileType )
			{
				case Bitmap.FILE_TYPE.JPEG:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 2.2f;							// JPG uses a 2.2 gamma by default
					m_Chromaticities = Chromaticities.sRGB;	// Default for JPEGs is sRGB
					EnumerateMetaDataJPG( _MetaData, out m_bProfileFoundInFile, out bGammaFoundInFile );

					if ( !m_bProfileFoundInFile && !bGammaFoundInFile )
						bGammaFoundInFile = true;			// Unless specified otherwise, we override the gamma no matter what since JPEGs use a 2.2 gamma by default anyway
					break;

				case Bitmap.FILE_TYPE.PNG:
					m_GammaCurve = GAMMA_CURVE.sRGB;
					m_Gamma = GAMMA_EXPONENT_sRGB;
					m_Chromaticities = Chromaticities.sRGB;	// Default for PNGs is standard sRGB
					EnumerateMetaDataPNG( _MetaData, out m_bProfileFoundInFile, out bGammaFoundInFile );
					break;

				case Bitmap.FILE_TYPE.TIFF:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 1.0f;							// Linear gamma by default
					m_Chromaticities = Chromaticities.sRGB;	// Default for TIFFs is sRGB
					EnumerateMetaDataTIFF( _MetaData, out m_bProfileFoundInFile, out bGammaFoundInFile );
					break;

				case Bitmap.FILE_TYPE.GIF:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 1.0f;
					m_Chromaticities = Chromaticities.sRGB;	// Default for GIFs is standard sRGB with no gamma
					break;

				case Bitmap.FILE_TYPE.BMP:	// BMP Don't have metadata!
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 1.0f;
					m_Chromaticities = Chromaticities.sRGB;	// Default for BMPs is standard sRGB with no gamma
					break;

				case Bitmap.FILE_TYPE.CRW:	// Raw files have no correction
				case Bitmap.FILE_TYPE.CR2:
				case Bitmap.FILE_TYPE.DNG:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 1.0f;
					m_Chromaticities = Chromaticities.sRGB;	// Default for BMPs is standard sRGB with no gamma
					break;
			}

			BuildTransformFromChroma( bGammaFoundInFile );
		}

		/// <summary>
		/// Creates a color profile from chromaticities
		/// </summary>
		/// <param name="_Chromaticities">The chromaticities for this profile</param>
		/// <param name="_GammaCurve">The type of gamma curve to use</param>
		/// <param name="_Gamma">The gamma power</param>
		public ColorProfile( Chromaticities _Chromaticities, GAMMA_CURVE _GammaCurve, float _Gamma )
		{
			m_Chromaticities = _Chromaticities;
			m_GammaCurve = _GammaCurve;
			m_Gamma = _Gamma;

			BuildTransformFromChroma( true );
		}

		#region IColorConverter Members

		/// <summary>
		/// Converts a CIEXYZ color to a RGB color
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		public float4	XYZ2RGB( float4 _XYZ )
		{
			return m_InternalConverter.XYZ2RGB( _XYZ );
		}

		/// <summary>
		/// Converts a RGB color to a CIEXYZ color
		/// </summary>
		/// <param name="_RGB"></param>
		/// <returns></returns>
		public float4	RGB2XYZ( float4 _RGB )
		{
			return m_InternalConverter.RGB2XYZ( _RGB );
		}

		/// <summary>
		/// Converts a CIEXYZ color to a RGB color
		/// </summary>
		/// <param name="_XYZ"></param>
		public void		XYZ2RGB( float4[,] _XYZ, float4[,] _RGB )
		{
			m_InternalConverter.XYZ2RGB( _XYZ, _RGB );
		}

		/// <summary>
		/// Converts a RGB color to a CIEXYZ color
		/// </summary>
		/// <param name="_RGB"></param>
		public void		RGB2XYZ( float4[,] _RGB, float4[,] _XYZ )
		{
			m_InternalConverter.RGB2XYZ( _RGB, _XYZ );
		}

		#endregion

		#region Color Space Transforms

		/// <summary>
		/// Builds the RGB<->XYZ transforms from chromaticities
		/// (refer to http://wiki.nuaj.net/index.php/Color_Transforms#XYZ_Matrices for explanations)
		/// </summary>
		protected void	BuildTransformFromChroma( bool _bCheckGammaCurveOverride )
		{
			float3	xyz_R = new float3( m_Chromaticities.R.x, m_Chromaticities.R.y, 1.0f - m_Chromaticities.R.x - m_Chromaticities.R.y );
			float3	xyz_G = new float3( m_Chromaticities.G.x, m_Chromaticities.G.y, 1.0f - m_Chromaticities.G.x - m_Chromaticities.G.y );
			float3	xyz_B = new float3( m_Chromaticities.B.x, m_Chromaticities.B.y, 1.0f - m_Chromaticities.B.x - m_Chromaticities.B.y );
			float3	XYZ_W = xyY2XYZ( new float3( m_Chromaticities.W.x, m_Chromaticities.W.y, 1.0f ) );

			float4x4	M_xyz = new float4x4() {
				row0 = new float4( xyz_R, 0.0f ),
				row1 = new float4( xyz_G, 0.0f ),
				row2 = new float4( xyz_B, 0.0f ),
				row3 = new float4( 0.0f, 0.0f, 0.0f, 1.0f )
			};

			M_xyz.Invert();

			float4	Sum_RGB = new float4( XYZ_W, 1.0f ) * M_xyz;

			// Finally, we can retrieve the RGB->XYZ transform
			m_RGB2XYZ.row0 = new float4( Sum_RGB.x * xyz_R, 0.0f );
			m_RGB2XYZ.row1 = new float4( Sum_RGB.y * xyz_G, 0.0f );
			m_RGB2XYZ.row2 = new float4( Sum_RGB.z * xyz_B, 0.0f );

			// And the XYZ->RGB transform
			m_XYZ2RGB = m_RGB2XYZ;
			m_XYZ2RGB.Invert();

			// ============= Attempt to recognize a standard profile ============= 
			STANDARD_PROFILE	RecognizedProfile = m_Chromaticities.RecognizedProfile;

			if ( _bCheckGammaCurveOverride )
			{	// Also ensure the gamma ramp is correct before assigning a standard profile
				bool	bIsGammaCorrect = true;
				switch ( RecognizedProfile )
				{
					case STANDARD_PROFILE.sRGB:				bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.sRGB, GAMMA_EXPONENT_sRGB ); break;
					case STANDARD_PROFILE.ADOBE_RGB_D50:	bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.STANDARD, GAMMA_EXPONENT_ADOBE ); break;
					case STANDARD_PROFILE.ADOBE_RGB_D65:	bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.STANDARD, GAMMA_EXPONENT_ADOBE ); break;
					case STANDARD_PROFILE.PRO_PHOTO:		bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.PRO_PHOTO, GAMMA_EXPONENT_PRO_PHOTO ); break;
					case STANDARD_PROFILE.RADIANCE:			bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.STANDARD, 1.0f ); break;
				}

				if ( !bIsGammaCorrect )
					RecognizedProfile = STANDARD_PROFILE.CUSTOM;	// A non-standard gamma curves fails our pre-defined design...
			}


			// ============= Assign the internal converter depending on the profile =============
			switch ( RecognizedProfile )
			{
				case STANDARD_PROFILE.sRGB:
					m_GammaCurve = GAMMA_CURVE.sRGB;
					m_Gamma = GAMMA_EXPONENT_sRGB;
					m_InternalConverter = new InternalColorConverter_sRGB();
					break;

				case STANDARD_PROFILE.ADOBE_RGB_D50:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = GAMMA_EXPONENT_ADOBE;
					m_InternalConverter = new InternalColorConverter_AdobeRGB_D50();
					break;

				case STANDARD_PROFILE.ADOBE_RGB_D65:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = GAMMA_EXPONENT_ADOBE;
					m_InternalConverter = new InternalColorConverter_AdobeRGB_D65();
					break;

				case STANDARD_PROFILE.PRO_PHOTO:
					m_GammaCurve = GAMMA_CURVE.PRO_PHOTO;
					m_Gamma = GAMMA_EXPONENT_PRO_PHOTO;
					m_InternalConverter = new InternalColorConverter_ProPhoto();
					break;

				case STANDARD_PROFILE.RADIANCE:
					m_GammaCurve = GAMMA_CURVE.STANDARD;
					m_Gamma = 1.0f;
					m_InternalConverter = new InternalColorConverter_Radiance();
					break;

				default:	// Switch to one of our generic converters
					switch ( m_GammaCurve )
					{
						case GAMMA_CURVE.sRGB:
							m_InternalConverter = new InternalColorConverter_Generic_sRGBGamma( m_RGB2XYZ, m_XYZ2RGB );
							break;
						case GAMMA_CURVE.PRO_PHOTO:
							m_InternalConverter = new InternalColorConverter_Generic_ProPhoto( m_RGB2XYZ, m_XYZ2RGB );
							break;
						case GAMMA_CURVE.STANDARD:
							if ( Math.Abs( m_Gamma - 1.0f ) < 1e-3f )
								m_InternalConverter = new InternalColorConverter_Generic_NoGamma( m_RGB2XYZ, m_XYZ2RGB );
							else
								m_InternalConverter = new InternalColorConverter_Generic_StandardGamma( m_RGB2XYZ, m_XYZ2RGB, m_Gamma );
							break;
					}
					break;
			}
		}

		/// <summary>
		/// Ensures the current gamma curve type and value are the ones we want
		/// </summary>
		/// <param name="_Curve"></param>
		/// <param name="_Gamma"></param>
		/// <returns></returns>
		protected bool	EnsureGamma( GAMMA_CURVE _Curve, float _Gamma )
		{
			return m_GammaCurve == _Curve && Math.Abs( _Gamma - m_Gamma ) < 1e-3f;
		}

		/// <summary>
		/// Converts from XYZ to xyY
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		public static float3	XYZ2xyY( float3 _XYZ )
		{
			float	InvSum = 1.0f / Math.Max( 1e-8f, _XYZ.x + _XYZ.y + _XYZ.z);
			return new float3( _XYZ.x * InvSum, _XYZ.y * InvSum, _XYZ.y );
		}

		/// <summary>
		/// Converts from xyY to XYZ
		/// </summary>
		/// <param name="_xyY"></param>
		/// <returns></returns>
		public static float3	xyY2XYZ( float3 _xyY )
		{
			float	Y_y = _xyY.y > 1e-8f ? _xyY.z / _xyY.y : 0.0f;
			return new float3( _xyY.x * Y_y, _xyY.z, (1.0f - _xyY.x - _xyY.y) * Y_y );
		}

		/// <summary>
		/// Applies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-correct</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static float		GammaCorrect( float c, float _ImageGamma )
		{
			return (float) Math.Pow( c, 1.0f / _ImageGamma );
		}

		/// <summary>
		/// Un-aplies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-uncorrect</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static float		GammaUnCorrect( float c, float _ImageGamma )
		{
			return (float) Math.Pow( c, _ImageGamma );
		}

		/// <summary>
		/// Converts from linear space to sRGB
		/// Code borrowed from D3DX_DXGIFormatConvert.inl from the DX10 SDK
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static float		Linear2sRGB( float c )
		{
			if ( c < 0.0031308f )
				return c * 12.92f;

			return 1.055f * (float) Math.Pow( c, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f;
		}

		/// <summary>
		/// Converts from sRGB to linear space
		/// Code borrowed from D3DX_DXGIFormatConvert.inl from the DX10 SDK
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static float		sRGB2Linear( float c )
		{
			if ( c < 0.04045f )
				return c / 12.92f;

			return (float) Math.Pow( (c + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		}

		#endregion

		#region Metadata Parsing

		protected void	EnumerateMetaDataJPG( BitmapMetadata _MetaData, out bool _bProfileFound, out bool _bGammaWasSpecified )
		{
			bool	bGammaWasSpecified = false;
			bool	bProfileFound = false;

			EnumerateMetaData( _MetaData,
				new MetaDataProcessor( "/xmp", ( object _SubData ) =>
				{
					BitmapMetadata	SubData = _SubData as BitmapMetadata;

					// Retrieve gamma ramp
					bGammaWasSpecified = FindPhotometricInterpretation( SubData, "/tiff:PhotometricInterpretation" );

 					// Let's look for the ICCProfile line that Photoshop puts out...
					if ( bProfileFound = FindICCProfileString( SubData, ref bGammaWasSpecified ) )
						return;

					// Ok ! So we got nothing so far... Try and read a recognized color space
					bProfileFound = FindEXIFColorProfile( SubData );
				} )

// These are huffman tables (cf. http://www.impulseadventure.com/photo/optimized-jpeg.html)
// Nothing to do with color profiles
// 					new MetaDataProcessor( "/luminance/TableEntry", ( object _SubData ) =>
// 					{
// 					} ),
// 
// 					new MetaDataProcessor( "/chrominance/TableEntry", ( object _SubData ) =>
// 					{
// 					} )
				);

			_bGammaWasSpecified = bGammaWasSpecified;
			_bProfileFound = bProfileFound;
		}

		protected void	EnumerateMetaDataPNG( BitmapMetadata _MetaData, out bool _bProfileFound, out bool _bGammaWasSpecified )
		{
			bool	bGammaWasSpecified = false;
			bool	bProfileFound = false;

			EnumerateMetaData( _MetaData,
				// Read chromaticities
				new MetaDataProcessor( "/cHRM", ( object v ) =>
				{
					BitmapMetadata	ChromaData = v as BitmapMetadata;

					Chromaticities	TempChroma = Chromaticities.Empty;
					EnumerateMetaData( ChromaData,
						new MetaDataProcessor( "/RedX",			( object v2 ) => { TempChroma.R.x = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/RedY",			( object v2 ) => { TempChroma.R.y = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/GreenX",		( object v2 ) => { TempChroma.G.x = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/GreenY",		( object v2 ) => { TempChroma.G.y = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/BlueX",		( object v2 ) => { TempChroma.B.x = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/BlueY",		( object v2 ) => { TempChroma.B.y = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/WhitePointX",	( object v2 ) => { TempChroma.W.x = 0.00001f * (uint) v2; } ),
						new MetaDataProcessor( "/WhitePointY",	( object v2 ) => { TempChroma.W.y = 0.00001f * (uint) v2; } )
						);

					if ( TempChroma.RecognizedProfile != STANDARD_PROFILE.INVALID )
					{	// Assign new chroma values
						m_Chromaticities = TempChroma;
						bProfileFound = true;
					}
				} ),
					
				// Read gamma
				new MetaDataProcessor( "/gAMA/ImageGamma", ( object v ) => {
					m_GammaCurve = GAMMA_CURVE.STANDARD; m_Gamma = 1.0f / (0.00001f * (uint) v); bGammaWasSpecified = true;
				} ),

				// Read explicit sRGB
				new MetaDataProcessor( "/sRGB/RenderingIntent", ( object v ) => {
					m_Chromaticities = Chromaticities.sRGB; bProfileFound = true; bGammaWasSpecified = false;
				} ),

				// Read string profile from iTXT
				new MetaDataProcessor( "/iTXt/TextEntry", ( object v ) =>
				{
					if ( bProfileFound )
						return;	// No need...

					// Hack content !
					string	XMLContent = v as string;
						
					string	ICCProfile = FindAttribute( XMLContent, "photoshop:ICCProfile" );
					if ( ICCProfile != null && (bProfileFound = HandleICCProfileString( ICCProfile )) )
						return;

					string	ColorSpace = FindAttribute( XMLContent, "exif:ColorSpace" );
					if ( ColorSpace != null )
						bProfileFound = HandleEXIFColorSpace( ColorSpace );
				} )
				);

			_bGammaWasSpecified = bGammaWasSpecified;
			_bProfileFound = bProfileFound;
		}

		protected void	EnumerateMetaDataTIFF( BitmapMetadata _MetaData, out bool _bProfileFound, out bool _bGammaWasSpecified )
		{
			bool	bGammaWasSpecified = false;
			bool	bProfileFound = false;

			EnumerateMetaData( _MetaData,
					// Read Photometric Interpretation
					new MetaDataProcessor( "/ifd", ( object v ) =>
						{
							bGammaWasSpecified = FindPhotometricInterpretation( v as BitmapMetadata, "/{ushort=262}" );
						} ),

					// Read WhitePoint
					new MetaDataProcessor( "/ifd/{ushort=318}", ( object v ) =>
					{
						bProfileFound = true;
						throw new Exception( "TODO: Handle TIFF tag 0x13E !" );
						// White point ! Encoded as 2 "RATIONALS"
					} ),

					// Read Chromaticities
					new MetaDataProcessor( "/ifd/{ushort=319}", ( object v ) =>
					{
						bProfileFound = true;
						throw new Exception( "TODO: Handle TIFF tag 0x13F !" );
						// Chromaticities ! Encoded as 6 "RATIONALS"
					} ),

					// Read generic data
					new MetaDataProcessor( "/ifd/{ushort=700}", ( object _SubData ) =>
					{
						if ( bProfileFound )
							return;	// We already have a valid profile...

						BitmapMetadata	SubData = _SubData as BitmapMetadata;

						// Try and read a recognized color space
						if ( (bProfileFound = FindEXIFColorProfile( SubData )) )
							return;	// No need to go hacker-style !

 						// Ok ! So we got nothing so far... Let's look for the ICCProfile line that Photoshop puts out...
						bProfileFound = FindICCProfileString( SubData, ref bGammaWasSpecified );
					} )
				);

			_bGammaWasSpecified = bGammaWasSpecified;
			_bProfileFound = bProfileFound;
		}

		/// <summary>
		/// Attempts to find the TIFF "PhotometricInterpretation" metadata
		/// </summary>
		/// <param name="_Meta"></param>
		/// <param name="_MetaPath"></param>
		/// <returns>True if gamma was specified</returns>
		protected bool	FindPhotometricInterpretation( BitmapMetadata _Meta, string _MetaPath )
		{
			bool	bGammaWasSpecified = false;
			EnumerateMetaData( _Meta,
				new MetaDataProcessor( _MetaPath, ( object v ) =>
				{
					int	PhotometricInterpretation = -1;
					if ( v is string )
					{
						if ( !int.TryParse( v as string, out PhotometricInterpretation ) )
							throw new Exception( "Invalid string for TIFF Photometric Interpretation !" );
					}
					else if ( v is ushort )
						PhotometricInterpretation = (ushort) v;

					switch ( PhotometricInterpretation )
					{
						case 0:	// Grayscale
						case 1:
							m_GammaCurve = GAMMA_CURVE.STANDARD;
							m_Gamma = 1.0f;
							bGammaWasSpecified = true;
							break;

						case 2:	// NTSC RGB
							m_GammaCurve = GAMMA_CURVE.STANDARD;
							m_Gamma = 2.2f;
							bGammaWasSpecified = true;
							break;

						default:
							// According to the spec (page 117), a value of 6 is a YCrCb image while a value of 8 is a L*a*b* image
							// SHould we handle this in case of ???
							throw new Exception( "TODO: Handle TIFF special photometric interpretation !" );
					}
				} ) );

			return bGammaWasSpecified;
		}

		/// <summary>
		/// Attempts to find the color profile in the EXIF metadata
		/// </summary>
		/// <param name="_Meta"></param>
		/// <returns>True if the profile was successfully found</returns>
		protected bool	FindEXIFColorProfile( BitmapMetadata _Meta )
		{
			bool	bProfileFound = false;
			EnumerateMetaData( _Meta,
				new MetaDataProcessor( "/exif:ColorSpace", ( object v ) =>
					{
						bProfileFound = HandleEXIFColorSpace( v as string );
					} )
				);

			return bProfileFound;
		}
			
		/// <summary>
		/// Attempts to find the "photoshop:ICCProfile" string in the metadata dump and retrieve a known profile from it
		/// </summary>
		/// <param name="_Meta"></param>
		/// <param name="_bGammaWasSpecified"></param>
		/// <returns>True if the profile was successfully found</returns>
		protected bool	FindICCProfileString( BitmapMetadata _Meta, ref bool _bGammaWasSpecified )
		{
			bool	bProfileFound = false;
			bool	bGammaWasSpecified = _bGammaWasSpecified;
			EnumerateMetaData( _Meta,
				new MetaDataProcessor( "/photoshop:ICCProfile", ( object v ) =>
					{
						if ( HandleICCProfileString( v as string ) )
							bGammaWasSpecified = false;	// Assume profile, complete with gamma
					} ) );

			_bGammaWasSpecified = bGammaWasSpecified;
			return bProfileFound;
		}

		protected bool	HandleEXIFColorSpace( string _ColorSpace )
		{
			int	Value = -1;
			return int.TryParse( _ColorSpace, out Value ) ? HandleEXIFColorSpace( Value ) : false;
		}

		/// <summary>
		/// Attempts to handle a color profile from the EXIF ColorSpace tag
		/// </summary>
		/// <param name="_ColorSpace"></param>
		/// <returns>True if the profile was recognized</returns>
		protected bool	HandleEXIFColorSpace( int _ColorSpace )
		{
			switch ( _ColorSpace )
			{
				case 1:
					m_Chromaticities = Chromaticities.sRGB;			// This is definitely sRGB
					return true;									// We now know the profile !

				case 2:
					m_Chromaticities = Chromaticities.AdobeRGB_D65;	// This is not official but sometimes it's AdobeRGB
					return true;									// We now know the profile !
			}

			return false;
		}

		/// <summary>
		/// Attempts to handle an ICC profile by name
		/// </summary>
		/// <param name="_ProfilName"></param>
		/// <returns>True if the profile was recognized</returns>
		protected bool	HandleICCProfileString( string _ProfilName )
		{
			if ( _ProfilName.IndexOf( "sRGB IEC61966-2.1" ) != -1 )
			{
				m_Chromaticities = Chromaticities.sRGB;
				return true;
			}
			else if ( _ProfilName.IndexOf( "Adobe RGB (1998)" ) != -1 )
			{
				m_Chromaticities = Chromaticities.AdobeRGB_D65;
				return true;
			}
			else if ( _ProfilName.IndexOf( "ProPhoto" ) != -1 )
			{
				m_Chromaticities = Chromaticities.ProPhoto;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to find an XML attribute by name
		/// </summary>
		/// <param name="_XMLContent"></param>
		/// <param name="_AttributeName"></param>
		/// <returns></returns>
		protected string	FindAttribute( string _XMLContent, string _AttributeName )
		{
			int	AttributeStartIndex = _XMLContent.IndexOf( _AttributeName );
			if ( AttributeStartIndex == -1 )
				return null;

			int	ValueStartIndex = _XMLContent.IndexOf( "\"", AttributeStartIndex );
			if ( ValueStartIndex == -1 || ValueStartIndex > AttributeStartIndex+_AttributeName.Length+2+2 )
				return null;	// Not found or too far from attribute... (we're expecting Name="Value" or Name = "Value")

			int	ValueEndIndex = _XMLContent.IndexOf( "\"", ValueStartIndex+1 );
			if ( ValueEndIndex == -1 )
				return null;

			return _XMLContent.Substring( ValueStartIndex+1, ValueEndIndex-ValueStartIndex-1 );
		}

		#region Enumeration Tools

		[System.Diagnostics.DebuggerDisplay( "Path={Path}" )]
		protected class		MetaDataProcessor
		{
			public string			Path;
			public Action<object>	Process;
			public MetaDataProcessor( string _Path, Action<object> _Process )	{ Path = _Path; Process = _Process; }
		}

		protected void	EnumerateMetaData( BitmapMetadata _Root, params MetaDataProcessor[] _Processors )
		{
			foreach ( MetaDataProcessor Processor in _Processors )
			{
				if ( !_Root.ContainsQuery( Processor.Path ) )
					continue;

				object	Value = _Root.GetQuery( Processor.Path );
				if ( Value == null )
					throw new Exception( "Failed to find the metadata path \"" + Processor.Path + "\" !" );

				Processor.Process( Value );
			}
		}

		protected string	DumpMetaData( BitmapMetadata _Root )
		{
			return DumpMetaData( _Root, "" );
		}
		protected string	DumpMetaData( BitmapMetadata _Root, string _Tab )
		{
			string	Result = "";
			foreach ( string Meta in _Root.AsEnumerable<string>() )
			{
				Result += _Tab + Meta;

				object	Value = _Root.GetQuery( Meta );
				if ( Value is BitmapMetadata )
				{	// Recurse
					_Tab += "\t";
					Result += "\r\n" + DumpMetaData( Value as BitmapMetadata, _Tab );
					_Tab = _Tab.Remove( _Tab.Length-1 );
				}
				else
				{	// Leaf
					Result += " = " + Value + "\r\n";
				}
			}

			return Result;
		}

		#endregion

		#endregion

		#endregion
	}
}
