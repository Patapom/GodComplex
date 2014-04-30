using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using SharpDX;
//using SharpDX.Direct3D10;
//using SharpDX.DXGI;
//using SharpDX.WIC;
using System.Windows.Media.Imaging;

using WMath;

// TODO !!! => Avoir la possibilité de créer une texture avec un seul channel du bitmap !? (filtrage)
//			=> Mettre une option pour "premultiplied alpha"

namespace StandardizedDiffuseAlbedoMaps
{
	#region ------------------- GAMMA CORRECTION -------------------
	// The Image class completely supports gamma-corrected images so your internal representation of images always is in linear space.
	// The _ImageGamma parameter present in numerous methods of the Image class is very important to understand in order to obtain
	//  linear space images so you can work peacefully with your pipeline.
	// 
	// The Problem :
	// -------------
	// To sum up, images digitized by cameras or scanners (and even images hand-painted by a software that does not support gamma correction)
	// are stored with gamma-correction "unapplied". That means your camera knows that your monitor has a display curve with a
	// gamma correction factor of about 2.2 so, in order to get back the image you took with the camera, it will store the image
	// with the inverse gamma factor of the monitor.
	// 
	// In short, here is what happens in a few steps :
	//   1) Photons and radiance in the real scene you shot with the camera are in "linear space"
	//       => This means that receiving twice more photons will double the radiance
	// 
	//   2) The camera sensor grabs the radiance and stores it internally in linear space (all is well until now)
	// 
	//   3) When you store the RAW camera image into JPEG or PNG for example, the camera will write the gamma-corrected radiance
	//       => This means the color written to the disk file is not RGB but pow( RGB, 1/Gamma ) instead
	//       => For JPEG or PNG, the usual Gamma value is 2.2 to compensate for the average 2.2 gamma of the CRT displays
	//       
	//   4) When you load the JPEG image as a texture, it's not in linear space but in *GAMMA SPACE*
	// 
	//   5) Finally, displaying the texture to the screen will apply the final gamma correction that will, ideally, convert back
	//      the gamma space image into linear space radiance for your eyes to see.
	//       => This means the monitor will not display the color RGB but pow( RGB, Gamma ) instead
	//       => The usual gamma of a CRT is 2.2, thus nullifying the effect of the JPEG 2.2 gamma correction
	// 
	// So, if you are assuming you are dealing with linear space images from point 4) then you are utterly **WRONG** and will lead to many problems !
	// (even creating mip-maps in gamma space is a problem)
	// 
	// 
	// The Solution :
	// --------------
	// The idea is simply to negate the effect of JPEG/PNG/Whatever gamma-uncorrection by applying pow( RGB, Gamma ) as soon as
	//  point 4) so you obtain nice linear-space textures you can work with peacefully.
	// You can either choose to apply the pow() providing the appropriate _ImageGamma parameter, or you can
	//	use the PF_RGBA8_sRGB pixel format with a _ImageGamma of 1.0 if you know your image is sRGB encoded.
	// 
	// If everything is in linear space then all is well in your rendering pipeline until the result is displayed back.
	// Indeed, right before displaying the final (linear space) color, you should apply gamma correction and write pow( RGB, 1/Gamma )
	//  so the monitor will then apply pow( RGB, Gamma ) and so your linear space color is correctly viewed by your eyes.
	// That may seem like a lot of stupid operations queued together to amount to nothing, but these are merely here to circumvent a physical
	//  property of the screens (which should have been handled by the screen constructors a long time ago IMHO).
	// 
	// 
	// The complete article you should read to make up your mind about gamma : http://http.developer.nvidia.com/GPUGems3/gpugems3_ch24.html
	#endregion

	/// <summary>
	/// The Bitmap class should be used to replace the standard System.Drawing.Bitmap
	/// The big advantage of the Bitmap class is to accurately read back the color profile and gamma correction data stored in the image's metadata
	/// so that, internally, the image is stored:
	///		* As device-independent CIE XYZ (http://en.wikipedia.org/wiki/CIE_1931_color_space) format, our Profile Connection Space
	///		* In linear space (i.e. no gamma curve is applied)
	///		* NOT pre-multiplied alpha (you can later re-pre-multiply if needed)
	///	
	/// This helps to ensure that whatever the source image format stored on disk, you always deal with a uniformized image internally.
	/// 
	/// Later, you can cast from the CIE XYZ device-independent format into any number of pre-defined texture profiles:
	///		* sRGB or Linear space textures (for 8bits per component images only)
	///		* Compressed (BC1-BC5) or uncompressed (for 8bits per component images only)
	///		* 8-, 16-, 16F- 32- or 32F-bits per component
	///		* Pre-multiplied alpha or not
	/// 
	/// The following image formats are currently supported:
	///		* JPG
	///		* PNG
	///		* TIFF
	///		* TGA
	///		* BMP
	///		* GIF
	///		* HDR
	/// </summary>
	/// <remarks>The Bitmap class has been tested with various formats, various bit depths and color profiles all created from Adobe Photoshop CS4 using
	/// the "Save As" dialog and the "Save for Web & Devices" dialog box.
	/// 
	/// In a general manner, you should NOT use the latter save option but rather select your working color profile from the "Edit > Color Settings" menu,
	///  then save your files and make sure you tick the "ICC Profile" checkbox using the DEFAULT save file dialog box to embed that profile in the image.
	/// </remarks>
	public class Bitmap2
	{
		#region CONSTANTS

		private static readonly System.Windows.Media.PixelFormat	GENERIC_PIXEL_FORMAT = System.Windows.Media.PixelFormats.Rgba128Float;
		protected const float	BYTE_TO_FLOAT = 1.0f / 255.0f;
		protected const float	WORD_TO_FLOAT = 1.0f / 65535.0f;

		#endregion

		#region NESTED TYPES

		/// <summary>
		/// Supported files types
		/// </summary>
		public enum	FILE_TYPE
		{
			JPEG,
			PNG,
			BMP,
			TGA,
			TIFF,
			GIF,
			HDR,

			UNKNOWN
		}

		/// <summary>
		/// A delegate used to process pixels (i.e. either build or modify the pixel)
		/// </summary>
		/// <param name="_X"></param>
		/// <param name="_Y"></param>
		/// <param name="_Color"></param>
		public delegate void	ImageProcessDelegate( int _X, int _Y, ref Vector4D _Color );

		/// <summary>
		/// Defines a color converter that can handle transforms between XYZ and RGB
		/// Usually implemented by a ColorProfile so the RGB color is fully characterized
		/// </summary>
		public interface IColorConverter
		{
			Vector4D	XYZ2RGB( Vector4D _XYZ );
			Vector4D	RGB2XYZ( Vector4D _RGB );
			void		XYZ2RGB( Vector4D[,] _XYZ );
			void		RGB2XYZ( Vector4D[,] _RGB );
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

			public static readonly Vector2D	ILLUMINANT_A	= new Vector2D( 0.44757f, 0.40745f );	// Incandescent, tungsten
			public static readonly Vector2D	ILLUMINANT_D50	= new Vector2D( 0.34567f, 0.35850f );	// Daylight, Horizon
			public static readonly Vector2D	ILLUMINANT_D55	= new Vector2D( 0.33242f, 0.34743f );	// Mid-Morning, Mid-Afternoon
			public static readonly Vector2D	ILLUMINANT_D65	= new Vector2D( 0.31271f, 0.32902f );	// Daylight, Noon, Overcast (sRGB reference illuminant)
			public static readonly Vector2D	ILLUMINANT_E	= new Vector2D( 1/3.0f, 1/3.0f );		// Reference

			#endregion

			#region NESTED TYPES

			public enum STANDARD_PROFILE
			{
				UNKNOWN,		// No recognizable profile
				INVALID,		// The profile is invalid (meaning one of the chromaticities was not initialized !)
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
			public struct	Chromaticities
			{
				public Vector2D		R, G, B, W;

				public static Chromaticities	sRGB			= new Chromaticities() { R = new Vector2D( 0.6400f, 0.3300f ), G = new Vector2D( 0.3000f, 0.6000f ), B = new Vector2D( 0.1500f, 0.0600f ), W = ILLUMINANT_D65 };
				public static Chromaticities	AdobeRGB_D50	= new Chromaticities() { R = new Vector2D( 0.6400f, 0.3300f ), G = new Vector2D( 0.2100f, 0.7100f ), B = new Vector2D( 0.1500f, 0.0600f ), W = ILLUMINANT_D50 };
				public static Chromaticities	AdobeRGB_D65	= new Chromaticities() { R = new Vector2D( 0.6400f, 0.3300f ), G = new Vector2D( 0.2100f, 0.7100f ), B = new Vector2D( 0.1500f, 0.0600f ), W = ILLUMINANT_D65 };
				public static Chromaticities	ProPhoto		= new Chromaticities() { R = new Vector2D( 0.7347f, 0.2653f ), G = new Vector2D( 0.1596f, 0.8404f ), B = new Vector2D( 0.0366f, 0.0001f ), W = ILLUMINANT_D50 };
				public static Chromaticities	Radiance		= new Chromaticities() { R = new Vector2D( 0.6400f, 0.3300f ), G = new Vector2D( 0.2900f, 0.6000f ), B = new Vector2D( 0.1500f, 0.0600f ), W = ILLUMINANT_E };

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
						return R.X != 0.0f && R.Y != 0.0f && G.X != 0.0f && G.Y != 0.0f && B.X != 0.0f && B.Y != 0.0f && W.X != 0.0f && W.Y != 0.0f ? STANDARD_PROFILE.UNKNOWN : STANDARD_PROFILE.INVALID;
					}
				}

				private bool	Equals( Chromaticities other )
				{
					return Equals( R, other.R ) && Equals( G, other.G ) && Equals( B, other.B ) && Equals( W, other.W );
				}
				private const float	EPSILON = 1e-3f;
				private bool	Equals( Vector2D a, Vector2D b )
				{
					return Math.Abs( a.X - b.X ) < EPSILON && Math.Abs( a.Y - b.Y ) < EPSILON;
				}
			}

			#region Internal XYZ<->RGB Converters

			protected class		InternalColorConverter_sRGB : IColorConverter
			{
				#region CONSTANTS

				public static readonly Matrix4x4	MAT_RGB2XYZ = new Matrix4x4( new float[] {
						0.4124f, 0.2126f, 0.0193f, 0.0f,
						0.3576f, 0.7152f, 0.1192f, 0.0f,
						0.1805f, 0.0722f, 0.9505f, 0.0f,
						0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
					} );

				public static readonly Matrix4x4	MAT_XYZ2RGB = new Matrix4x4( new float[] {
						 3.2406f, -0.9689f,  0.0557f, 0.0f,
						-1.5372f,  1.8758f, -0.2040f, 0.0f,
						-0.4986f,  0.0415f,  1.0570f, 0.0f,
						 0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
					} );

				#endregion

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * MAT_XYZ2RGB;

					// Gamma correct
					_XYZ.X = _XYZ.X > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.X, 1.0f / 2.4f ) - 0.055f : 12.92f * _XYZ.X;
					_XYZ.Y = _XYZ.Y > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.Y, 1.0f / 2.4f ) - 0.055f : 12.92f * _XYZ.Y;
					_XYZ.Z = _XYZ.Z > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.Z, 1.0f / 2.4f ) - 0.055f : 12.92f * _XYZ.Z;

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = _RGB.X < 0.04045f ? _RGB.X / 12.92f : (float) Math.Pow( (_RGB.X + 0.055f) / 1.055f, 2.4f );
					_RGB.Y = _RGB.Y < 0.04045f ? _RGB.Y / 12.92f : (float) Math.Pow( (_RGB.Y + 0.055f) / 1.055f, 2.4f );
					_RGB.Z = _RGB.Z < 0.04045f ? _RGB.Z / 12.92f : (float) Math.Pow( (_RGB.Z + 0.055f) / 1.055f, 2.4f );

					// Transform into XYZ
					return _RGB * MAT_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ =  XYZ * MAT_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = XYZ.X > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.X, 1.0f / 2.4f ) - 0.055f : 12.92f * XYZ.X;
							_XYZ[X,Y].Y = XYZ.Y > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.Y, 1.0f / 2.4f ) - 0.055f : 12.92f * XYZ.Y;
							_XYZ[X,Y].Z = XYZ.Z > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.Z, 1.0f / 2.4f ) - 0.055f : 12.92f * XYZ.Z;
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = RGB.X < 0.04045f ? RGB.X / 12.92f : (float) Math.Pow( (RGB.X + 0.055f) / 1.055f, 2.4f );
							RGB.Y = RGB.Y < 0.04045f ? RGB.Y / 12.92f : (float) Math.Pow( (RGB.Y + 0.055f) / 1.055f, 2.4f );
							RGB.Z = RGB.Z < 0.04045f ? RGB.Z / 12.92f : (float) Math.Pow( (RGB.Z + 0.055f) / 1.055f, 2.4f );

							// Transform into XYZ
							_RGB[X,Y] =  RGB *  MAT_RGB2XYZ;
						}
				}

				#endregion
			}

			protected class		InternalColorConverter_AdobeRGB_D50 : IColorConverter
			{
				#region CONSTANTS

				public static readonly Matrix4x4	MAT_RGB2XYZ = new Matrix4x4( new float[] {
						0.60974f, 0.31111f, 0.01947f, 0.0f,
						0.20528f, 0.62567f, 0.06087f, 0.0f,
						0.14919f, 0.06322f, 0.74457f, 0.0f,
						0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
					} );

				public static readonly Matrix4x4	MAT_XYZ2RGB = new Matrix4x4( new float[] {
						 1.96253f, -0.97876f,  0.02869f, 0.0f,
						-0.61068f,  1.91615f, -0.14067f, 0.0f,
						-0.34137f,  0.03342f,  1.34926f, 0.0f,
						 0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
					} );

				#endregion

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * MAT_XYZ2RGB;

					// Gamma correct
					_XYZ.X = (float) Math.Pow( _XYZ.X, 1.0f / 2.19921875f );
					_XYZ.Y = (float) Math.Pow( _XYZ.Y, 1.0f / 2.19921875f );
					_XYZ.Z = (float) Math.Pow( _XYZ.Z, 1.0f / 2.19921875f );

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = (float) Math.Pow( _RGB.X, 2.19921875f );
					_RGB.Y = (float) Math.Pow( _RGB.Y, 2.19921875f );
					_RGB.Z = (float) Math.Pow( _RGB.Z, 2.19921875f );

					// Transform into XYZ
					return _RGB * MAT_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ = XYZ * MAT_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = (float) Math.Pow( XYZ.X, 1.0f / 2.19921875f );
							_XYZ[X,Y].Y = (float) Math.Pow( XYZ.Y, 1.0f / 2.19921875f );
							_XYZ[X,Y].Z = (float) Math.Pow( XYZ.Z, 1.0f / 2.19921875f );
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = (float) Math.Pow( RGB.X, 2.19921875f );
							RGB.Y = (float) Math.Pow( RGB.Y, 2.19921875f );
							RGB.Z = (float) Math.Pow( RGB.Z, 2.19921875f );

							// Transform into XYZ
							_RGB[X,Y] = RGB * MAT_RGB2XYZ;
						}
				}

				#endregion
			}

			protected class		InternalColorConverter_AdobeRGB_D65 : IColorConverter
			{
				#region CONSTANTS

				public static readonly Matrix4x4	MAT_RGB2XYZ = new Matrix4x4( new float[] {
						0.57667f, 0.29734f, 0.02703f, 0.0f,
						0.18556f, 0.62736f, 0.07069f, 0.0f,
						0.18823f, 0.07529f, 0.99134f, 0.0f,
						0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
					} );

				public static readonly Matrix4x4	MAT_XYZ2RGB = new Matrix4x4( new float[] {
						 2.04159f, -0.96924f,  0.01344f, 0.0f,
						-0.56501f,  1.87597f, -0.11836f, 0.0f,
						-0.34473f,  0.04156f,  1.01517f, 0.0f,
						 0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
					} );

				#endregion

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * MAT_XYZ2RGB;

					// Gamma correct
					_XYZ.X = (float) Math.Pow( _XYZ.X, 1.0f / 2.19921875f );
					_XYZ.Y = (float) Math.Pow( _XYZ.Y, 1.0f / 2.19921875f );
					_XYZ.Z = (float) Math.Pow( _XYZ.Z, 1.0f / 2.19921875f );

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = (float) Math.Pow( _RGB.X, 2.19921875f );
					_RGB.Y = (float) Math.Pow( _RGB.Y, 2.19921875f );
					_RGB.Z = (float) Math.Pow( _RGB.Z, 2.19921875f );

					// Transform into XYZ
					return _RGB * MAT_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ = XYZ * MAT_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = (float) Math.Pow( XYZ.X, 1.0f / 2.19921875f );
							_XYZ[X,Y].Y = (float) Math.Pow( XYZ.Y, 1.0f / 2.19921875f );
							_XYZ[X,Y].Z = (float) Math.Pow( XYZ.Z, 1.0f / 2.19921875f );
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = (float) Math.Pow( RGB.X, 2.19921875f );
							RGB.Y = (float) Math.Pow( RGB.Y, 2.19921875f );
							RGB.Z = (float) Math.Pow( RGB.Z, 2.19921875f );

							// Transform into XYZ
							_RGB[X,Y] = RGB * MAT_RGB2XYZ;
						}
				}

				#endregion
			}

			protected class		InternalColorConverter_ProPhoto : IColorConverter
			{
				#region CONSTANTS

				public static readonly Matrix4x4	MAT_RGB2XYZ = new Matrix4x4( new float[] {
						0.7977f, 0.2880f, 0.0000f, 0.0f,
						0.1352f, 0.7119f, 0.0000f, 0.0f,
						0.0313f, 0.0001f, 0.8249f, 0.0f,
						0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
					} );

				public static readonly Matrix4x4	MAT_XYZ2RGB = new Matrix4x4( new float[] {
						 1.3460f, -0.5446f,  0.0000f, 0.0f,
						-0.2556f,  1.5082f,  0.0000f, 0.0f,
						-0.0511f,  0.0205f,  1.2123f, 0.0f,
						 0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
					} );

				#endregion

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * MAT_XYZ2RGB;

					// Gamma correct
					_XYZ.X = _XYZ.X > 0.001953f ? (float) Math.Pow( _XYZ.X, 1.0f / 1.8f ) : 16.0f * _XYZ.X;
					_XYZ.Y = _XYZ.Y > 0.001953f ? (float) Math.Pow( _XYZ.Y, 1.0f / 1.8f ) : 16.0f * _XYZ.Y;
					_XYZ.Z = _XYZ.Z > 0.001953f ? (float) Math.Pow( _XYZ.Z, 1.0f / 1.8f ) : 16.0f * _XYZ.Z;

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = _RGB.X > 0.031248f ? (float) Math.Pow( _RGB.X, 1.8f ) : _RGB.X / 16.0f;
					_RGB.Y = _RGB.Y > 0.031248f ? (float) Math.Pow( _RGB.Y, 1.8f ) : _RGB.Y / 16.0f;
					_RGB.Z = _RGB.Z > 0.031248f ? (float) Math.Pow( _RGB.Z, 1.8f ) : _RGB.Z / 16.0f;

					// Transform into XYZ
					return _RGB * MAT_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ = XYZ * MAT_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = XYZ.X > 0.001953f ? (float) Math.Pow( XYZ.X, 1.0f / 1.8f ) : 16.0f * XYZ.X;
							_XYZ[X,Y].Y = XYZ.Y > 0.001953f ? (float) Math.Pow( XYZ.Y, 1.0f / 1.8f ) : 16.0f * XYZ.Y;
							_XYZ[X,Y].Z = XYZ.Z > 0.001953f ? (float) Math.Pow( XYZ.Z, 1.0f / 1.8f ) : 16.0f * XYZ.Z;
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = RGB.X > 0.031248f ? (float) Math.Pow( RGB.X, 1.8f ) : RGB.X / 16.0f;
							RGB.Y = RGB.Y > 0.031248f ? (float) Math.Pow( RGB.Y, 1.8f ) : RGB.Y / 16.0f;
							RGB.Z = RGB.Z > 0.031248f ? (float) Math.Pow( RGB.Z, 1.8f ) : RGB.Z / 16.0f;

							// Transform into XYZ
							_RGB[X,Y] = RGB * MAT_RGB2XYZ;
						}
				}

				#endregion
			}

			protected class		InternalColorConverter_Radiance : IColorConverter
			{
				#region CONSTANTS

				public static readonly Matrix4x4	MAT_RGB2XYZ = new Matrix4x4( new float[] {
						0.5141447f, 0.2651059f, 0.0241005f, 0.0f,
						0.3238845f, 0.6701059f, 0.1228527f, 0.0f,
						0.1619709f, 0.0647883f, 0.8530467f, 0.0f,
						0.0000000f, 0.0000000f, 0.0000000f, 1.0f		// Alpha stays the same
					} );

				public static readonly Matrix4x4	MAT_XYZ2RGB = new Matrix4x4( new float[] {
						 2.5653124f, -1.02210832f,  0.07472437f, 0.0f,
						-1.1668493f,  1.97828662f, -0.25193953f, 0.0f,
						-0.3984632f,  0.04382159f,  1.17721522f, 0.0f,
						 0.0000000f,  0.00000000f,  0.00000000f, 1.0f	// Alpha stays the same
					} );

				#endregion

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					return _XYZ * MAT_XYZ2RGB;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Transform into XYZ
					return _RGB * MAT_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							_XYZ[X,Y] = _XYZ[X,Y] * MAT_XYZ2RGB;
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							_RGB[X,Y] = _RGB[X,Y] * MAT_RGB2XYZ;
				}

				#endregion
			}

			protected class		InternalColorConverter_Generic_NoGamma : IColorConverter
			{
				protected Matrix4x4	m_RGB2XYZ;
				protected Matrix4x4	m_XYZ2RGB;

				public InternalColorConverter_Generic_NoGamma( Matrix4x4 _RGB2XYZ, Matrix4x4 _XYZ2RGB )
				{
					m_RGB2XYZ = _RGB2XYZ;
					m_XYZ2RGB = _XYZ2RGB;
				}

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					return _XYZ * m_XYZ2RGB;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Transform into XYZ
					return _RGB * m_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							_XYZ[X,Y] = _XYZ[X,Y] * m_XYZ2RGB;
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							_RGB[X,Y] = _RGB[X,Y] * m_RGB2XYZ;
				}

				#endregion
			}

			protected class		InternalColorConverter_Generic_StandardGamma : IColorConverter
			{
				protected Matrix4x4	m_RGB2XYZ;
				protected Matrix4x4	m_XYZ2RGB;
				protected float		m_Gamma = 1.0f;
				protected float		m_InvGamma = 1.0f;

				public InternalColorConverter_Generic_StandardGamma( Matrix4x4 _RGB2XYZ, Matrix4x4 _XYZ2RGB, float _Gamma )
				{
					m_RGB2XYZ = _RGB2XYZ;
					m_XYZ2RGB = _XYZ2RGB;
					m_Gamma = _Gamma;
					m_InvGamma = 1.0f / _Gamma;
				}

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * m_XYZ2RGB;

					// Gamma correct
					_XYZ.X = (float) Math.Pow( _XYZ.X, m_InvGamma );
					_XYZ.Y = (float) Math.Pow( _XYZ.Y, m_InvGamma );
					_XYZ.Z = (float) Math.Pow( _XYZ.Z, m_InvGamma );

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = (float) Math.Pow( _RGB.X, m_Gamma );
					_RGB.Y = (float) Math.Pow( _RGB.Y, m_Gamma );
					_RGB.Z = (float) Math.Pow( _RGB.Z, m_Gamma );

					// Transform into XYZ
					return _RGB * m_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ = XYZ * m_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = (float) Math.Pow( XYZ.X, m_InvGamma );
							_XYZ[X,Y].Y = (float) Math.Pow( XYZ.Y, m_InvGamma );
							_XYZ[X,Y].Z = (float) Math.Pow( XYZ.Z, m_InvGamma );
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = (float) Math.Pow( RGB.X, m_Gamma );
							RGB.Y = (float) Math.Pow( RGB.Y, m_Gamma );
							RGB.Z = (float) Math.Pow( RGB.Z, m_Gamma );

							// Transform into XYZ
							_RGB[X,Y] = RGB * m_RGB2XYZ;
						}
				}

				#endregion
			}

			protected class		InternalColorConverter_Generic_sRGBGamma : IColorConverter
			{
				protected Matrix4x4	m_RGB2XYZ;
				protected Matrix4x4	m_XYZ2RGB;

				public InternalColorConverter_Generic_sRGBGamma( Matrix4x4 _RGB2XYZ, Matrix4x4 _XYZ2RGB )
				{
					m_RGB2XYZ = _RGB2XYZ;
					m_XYZ2RGB = _XYZ2RGB;
				}

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * m_XYZ2RGB;

					// Gamma correct
					_XYZ.X = _XYZ.X > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.X, 1.0f / 2.4f ) - 0.055f : 12.92f * _XYZ.X;
					_XYZ.Y = _XYZ.Y > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.Y, 1.0f / 2.4f ) - 0.055f : 12.92f * _XYZ.Y;
					_XYZ.Z = _XYZ.Z > 0.0031308f ? 1.055f * (float) Math.Pow( _XYZ.Z, 1.0f / 2.4f ) - 0.055f : 12.92f * _XYZ.Z;

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = _RGB.X < 0.04045f ? _RGB.X / 12.92f : (float) Math.Pow( (_RGB.X + 0.055f) / 1.055f, 2.4f );
					_RGB.Y = _RGB.Y < 0.04045f ? _RGB.Y / 12.92f : (float) Math.Pow( (_RGB.Y + 0.055f) / 1.055f, 2.4f );
					_RGB.Z = _RGB.Z < 0.04045f ? _RGB.Z / 12.92f : (float) Math.Pow( (_RGB.Z + 0.055f) / 1.055f, 2.4f );

					// Transform into XYZ
					return _RGB * m_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ = XYZ * m_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = XYZ.X > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.X, 1.0f / 2.4f ) - 0.055f : 12.92f * XYZ.X;
							_XYZ[X,Y].Y = XYZ.Y > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.Y, 1.0f / 2.4f ) - 0.055f : 12.92f * XYZ.Y;
							_XYZ[X,Y].Z = XYZ.Z > 0.0031308f ? 1.055f * (float) Math.Pow( XYZ.Z, 1.0f / 2.4f ) - 0.055f : 12.92f * XYZ.Z;
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = RGB.X < 0.04045f ? RGB.X / 12.92f : (float) Math.Pow( (RGB.X + 0.055f) / 1.055f, 2.4f );
							RGB.Y = RGB.Y < 0.04045f ? RGB.Y / 12.92f : (float) Math.Pow( (RGB.Y + 0.055f) / 1.055f, 2.4f );
							RGB.Z = RGB.Z < 0.04045f ? RGB.Z / 12.92f : (float) Math.Pow( (RGB.Z + 0.055f) / 1.055f, 2.4f );

							// Transform into XYZ
							_RGB[X,Y] = RGB * m_RGB2XYZ;
						}
				}

				#endregion
			}

			protected class		InternalColorConverter_Generic_ProPhoto : IColorConverter
			{
				protected Matrix4x4	m_RGB2XYZ;
				protected Matrix4x4	m_XYZ2RGB;

				public InternalColorConverter_Generic_ProPhoto( Matrix4x4 _RGB2XYZ, Matrix4x4 _XYZ2RGB )
				{
					m_RGB2XYZ = _RGB2XYZ;
					m_XYZ2RGB = _XYZ2RGB;
				}

				#region IColorConverter Members

				public Vector4D XYZ2RGB( Vector4D _XYZ )
				{
					// Transform into RGB
					_XYZ = _XYZ * m_XYZ2RGB;

					// Gamma correct
					_XYZ.X = _XYZ.X > 0.001953f ? (float) Math.Pow( _XYZ.X, 1.0f / 1.8f ) : 16.0f * _XYZ.X;
					_XYZ.Y = _XYZ.Y > 0.001953f ? (float) Math.Pow( _XYZ.Y, 1.0f / 1.8f ) : 16.0f * _XYZ.Y;
					_XYZ.Z = _XYZ.Z > 0.001953f ? (float) Math.Pow( _XYZ.Z, 1.0f / 1.8f ) : 16.0f * _XYZ.Z;

					return _XYZ;
				}

				public Vector4D RGB2XYZ( Vector4D _RGB )
				{
					// Gamma un-correct
					_RGB.X = _RGB.X > 0.031248f ? (float) Math.Pow( _RGB.X, 1.8f ) : _RGB.X / 16.0f;
					_RGB.Y = _RGB.Y > 0.031248f ? (float) Math.Pow( _RGB.Y, 1.8f ) : _RGB.Y / 16.0f;
					_RGB.Z = _RGB.Z > 0.031248f ? (float) Math.Pow( _RGB.Z, 1.8f ) : _RGB.Z / 16.0f;

					// Transform into XYZ
					return _RGB * m_RGB2XYZ;
				}

				public void XYZ2RGB( Vector4D[,] _XYZ )
				{
					int		W = _XYZ.GetLength( 0 );
					int		H = _XYZ.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	XYZ = _XYZ[X,Y];

							// Transform into RGB
							XYZ = XYZ * m_XYZ2RGB;

							// Gamma correct
							_XYZ[X,Y].X = XYZ.X > 0.001953f ? (float) Math.Pow( XYZ.X, 1.0f / 1.8f ) : 16.0f * XYZ.X;
							_XYZ[X,Y].Y = XYZ.Y > 0.001953f ? (float) Math.Pow( XYZ.Y, 1.0f / 1.8f ) : 16.0f * XYZ.Y;
							_XYZ[X,Y].Z = XYZ.Z > 0.001953f ? (float) Math.Pow( XYZ.Z, 1.0f / 1.8f ) : 16.0f * XYZ.Z;
						}
				}

				public void RGB2XYZ( Vector4D[,] _RGB )
				{
					int		W = _RGB.GetLength( 0 );
					int		H = _RGB.GetLength( 1 );
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
						{
							Vector4D	RGB = _RGB[X,Y];

							// Gamma un-correct
							RGB.X = RGB.X > 0.031248f ? (float) Math.Pow( RGB.X, 1.8f ) : RGB.X / 16.0f;
							RGB.Y = RGB.Y > 0.031248f ? (float) Math.Pow( RGB.Y, 1.8f ) : RGB.Y / 16.0f;
							RGB.Z = RGB.Z > 0.031248f ? (float) Math.Pow( RGB.Z, 1.8f ) : RGB.Z / 16.0f;

							// Transform into XYZ
							_RGB[X,Y] = RGB * m_RGB2XYZ;
						}
				}

				#endregion
			}

			#endregion

			#endregion

			#region FIELDS

			protected bool				m_bProfileFoundInFile = false;
			protected Chromaticities	m_Chromaticities = new Chromaticities();
			protected GAMMA_CURVE		m_GammaCurve = GAMMA_CURVE.STANDARD;
			protected float				m_Gamma = 1.0f;
			protected float				m_Exposure = 0.0f;

			protected Matrix4x4			m_RGB2XYZ = Matrix4x4.Identity;
			protected Matrix4x4			m_XYZ2RGB = Matrix4x4.Identity;

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
			public Matrix4x4		MatrixRGB2XYZ			{ get { return m_RGB2XYZ; } }

			/// <summary>
			/// Gets the transform to convert CIEXYZ to RGB
			/// </summary>
			public Matrix4x4		MatrixXYZ2RGB			{ get { return m_XYZ2RGB; } }

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
			/// Creates the color profile from metadata embedded in the image file
			/// </summary>
			/// <param name="_MetaData"></param>
			/// <param name="_FileType"></param>
			public ColorProfile( BitmapMetadata _MetaData, FILE_TYPE _FileType )
			{
				string	MetaDump = _MetaData != null ? DumpMetaData( _MetaData ) : null;

				bool	bGammaFoundInFile = false;
				switch ( _FileType )
				{
					case FILE_TYPE.JPEG:
						m_GammaCurve = GAMMA_CURVE.STANDARD;
						m_Gamma = 2.2f;							// JPG uses a 2.2 gamma by default
						m_Chromaticities = Chromaticities.sRGB;	// Default for JPEGs is sRGB
						EnumerateMetaDataJPG( _MetaData, out m_bProfileFoundInFile, out bGammaFoundInFile );

						if ( !m_bProfileFoundInFile && !bGammaFoundInFile )
							bGammaFoundInFile = true;			// Unless specified otherwise, we override the gamma no matter what since JPEGs use a 2.2 gamma by default anyway
						break;

					case FILE_TYPE.PNG:
						m_GammaCurve = GAMMA_CURVE.sRGB;
						m_Gamma = 2.4f;
						m_Chromaticities = Chromaticities.sRGB;	// Default for PNGs is standard sRGB
						EnumerateMetaDataPNG( _MetaData, out m_bProfileFoundInFile, out bGammaFoundInFile );
						break;

					case FILE_TYPE.TIFF:
						m_GammaCurve = GAMMA_CURVE.STANDARD;
						m_Gamma = 1.0f;							// Linear gamma by default
						m_Chromaticities = Chromaticities.sRGB;	// Default for TIFFs is sRGB
						EnumerateMetaDataTIFF( _MetaData, out m_bProfileFoundInFile, out bGammaFoundInFile );
						break;

					case FILE_TYPE.GIF:
						m_GammaCurve = GAMMA_CURVE.STANDARD;
						m_Gamma = 1.0f;
						m_Chromaticities = Chromaticities.sRGB;	// Default for GIFs is standard sRGB with no gamma
						break;

					case FILE_TYPE.BMP:	// BMP Don't have metadata !
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
			public Vector4D	XYZ2RGB( Vector4D _XYZ )
			{
				return m_InternalConverter.XYZ2RGB( _XYZ );
			}

			/// <summary>
			/// Converts a RGB color to a CIEXYZ color
			/// </summary>
			/// <param name="_RGB"></param>
			/// <returns></returns>
			public Vector4D	RGB2XYZ( Vector4D _RGB )
			{
				return m_InternalConverter.RGB2XYZ( _RGB );
			}

			/// <summary>
			/// Converts a CIEXYZ color to a RGB color
			/// </summary>
			/// <param name="_XYZ"></param>
			public void		XYZ2RGB( Vector4D[,] _XYZ )
			{
				m_InternalConverter.XYZ2RGB( _XYZ );
			}

			/// <summary>
			/// Converts a RGB color to a CIEXYZ color
			/// </summary>
			/// <param name="_RGB"></param>
			public void		RGB2XYZ( Vector4D[,] _RGB )
			{
				m_InternalConverter.RGB2XYZ( _RGB );
			}

			#endregion

			#region Color Space Transforms

			/// <summary>
			/// Builds the RGB<->XYZ transforms from chromaticities
			/// (refer to http://wiki.patapom.com/index.php/Color_Transforms#XYZ_.E2.86.92_xyY for explanations)
			/// </summary>
			protected void	BuildTransformFromChroma( bool _bCheckGammaCurveOverride )
			{
				Vector	xyz_R = new Vector( m_Chromaticities.R.X, m_Chromaticities.R.Y, 1.0f - m_Chromaticities.R.X - m_Chromaticities.R.Y );
				Vector	xyz_G = new Vector( m_Chromaticities.G.X, m_Chromaticities.G.Y, 1.0f - m_Chromaticities.G.X - m_Chromaticities.G.Y );
				Vector	xyz_B = new Vector( m_Chromaticities.B.X, m_Chromaticities.B.Y, 1.0f - m_Chromaticities.B.X - m_Chromaticities.B.Y );
				Vector	XYZ_W = xyY2XYZ( new Vector( m_Chromaticities.W.X, m_Chromaticities.W.Y, 1.0f ) );

				Matrix	M_xyz = new Matrix(	xyz_R.X, xyz_R.Y, xyz_R.Z, 0.0f,
											xyz_G.X, xyz_G.Y, xyz_G.Z, 0.0f,
											xyz_B.X, xyz_B.Y, xyz_B.Z, 0.0f,
											0.0f, 0.0f, 0.0f, 1.0f );
				M_xyz.Invert();

				Vector	Sum_RGB = Vector.TransformCoordinate( XYZ_W, M_xyz );

				// Finally, we can retrieve the RGB->XYZ transform
				m_RGB2XYZ = Matrix.Identity;
				m_RGB2XYZ.Row1 = new Vector4D( Sum_RGB.X * xyz_R, 0.0f );
				m_RGB2XYZ.Row2 = new Vector4D( Sum_RGB.Y * xyz_G, 0.0f );
				m_RGB2XYZ.Row3 = new Vector4D( Sum_RGB.Z * xyz_B, 0.0f );

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
						case STANDARD_PROFILE.sRGB:				bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.sRGB, 2.4f ); break;
						case STANDARD_PROFILE.ADOBE_RGB_D50:	bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.STANDARD, 2.19921875f ); break;
						case STANDARD_PROFILE.ADOBE_RGB_D65:	bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.STANDARD, 2.19921875f ); break;
						case STANDARD_PROFILE.PRO_PHOTO:		bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.PRO_PHOTO, 1.8f ); break;
						case STANDARD_PROFILE.RADIANCE:			bIsGammaCorrect = EnsureGamma( GAMMA_CURVE.STANDARD, 1.0f ); break;
					}

					if ( !bIsGammaCorrect )
						RecognizedProfile = STANDARD_PROFILE.UNKNOWN;	// A non-standard gamma curves fails our pre-defined design...
				}


				// ============= Assign the internal converter depending on the profile =============
				switch ( RecognizedProfile )
				{
					case STANDARD_PROFILE.sRGB:
						m_GammaCurve = GAMMA_CURVE.sRGB;
						m_Gamma = 2.4f;
						m_InternalConverter = new InternalColorConverter_sRGB();
						break;

					case STANDARD_PROFILE.ADOBE_RGB_D50:
						m_GammaCurve = GAMMA_CURVE.STANDARD;
						m_Gamma = 2.19921875f;
						m_InternalConverter = new InternalColorConverter_AdobeRGB_D50();
						break;

					case STANDARD_PROFILE.ADOBE_RGB_D65:
						m_GammaCurve = GAMMA_CURVE.STANDARD;
						m_Gamma = 2.19921875f;
						m_InternalConverter = new InternalColorConverter_AdobeRGB_D65();
						break;

					case STANDARD_PROFILE.PRO_PHOTO:
						m_GammaCurve = GAMMA_CURVE.PRO_PHOTO;
						m_Gamma = 1.8f;
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
			public static Vector	XYZ2xyY( Vector _XYZ )
			{
				float	InvSum = 1.0f / (_XYZ.X + _XYZ.Y + _XYZ.Z);
				return new Vector( _XYZ.X * InvSum, _XYZ.Y * InvSum, _XYZ.Y );
			}

			/// <summary>
			/// Converts from xyY to XYZ
			/// </summary>
			/// <param name="_xyY"></param>
			/// <returns></returns>
			public static Vector	xyY2XYZ( Vector _xyY )
			{
				float	Y_y = _xyY.Z / _xyY.Y;
				return new Vector( _xyY.X * Y_y, _xyY.Z, (1.0f - _xyY.X - _xyY.Y) * Y_y );
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

				return 1.055f * (float) Math.Pow( c, 1.0f / 2.4f ) - 0.055f;
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

				return (float) Math.Pow( (c + 0.055f) / 1.055f, 2.4f );
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

						Chromaticities	TempChroma = m_Chromaticities;
						EnumerateMetaData( ChromaData,
							new MetaDataProcessor( "/RedX", ( object v2 ) => { TempChroma.R.X = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/RedY", ( object v2 ) => { TempChroma.R.Y = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/GreenX", ( object v2 ) => { TempChroma.G.X = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/GreenY", ( object v2 ) => { TempChroma.G.Y = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/BlueX", ( object v2 ) => { TempChroma.B.X = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/BlueY", ( object v2 ) => { TempChroma.B.Y = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/WhitePointX", ( object v2 ) => { TempChroma.W.X = 0.00001f * (uint) v2; } ),
							new MetaDataProcessor( "/WhitePointY", ( object v2 ) => { TempChroma.W.Y = 0.00001f * (uint) v2; } )
							);

						if ( TempChroma.RecognizedProfile != STANDARD_PROFILE.INVALID )
						{	// Assign new chroma values
							m_Chromaticities = TempChroma;
							bProfileFound = true;
						}
					} ),
					
					// Read gamma
					new MetaDataProcessor( "/gAMA/ImageGamma", ( object v ) => { m_GammaCurve = GAMMA_CURVE.STANDARD; m_Gamma = 1.0f / (0.00001f * (uint) v); bGammaWasSpecified = true; } ),

					// Read explicit sRGB
					new MetaDataProcessor( "/sRGB/RenderingIntent", ( object v ) => { m_Chromaticities = Chromaticities.sRGB; bProfileFound = true; bGammaWasSpecified = false; } ),

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

		#endregion

		#region FIELDS

		protected FILE_TYPE			m_Type = FILE_TYPE.UNKNOWN;
		protected int				m_Width = 0;
		protected int				m_Height = 0;
		protected bool				m_bHasAlpha = false;

		protected ColorProfile		m_ColorProfile = null;
		protected Vector4D[,]		m_Bitmap = null;		// CIEXYZ Bitmap content + Alpha

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets the source bitmap type
		/// </summary>
		public FILE_TYPE	Type					{ get { return m_Type; } }

		/// <summary>
		/// Gets the image width
		/// </summary>
		public int			Width					{ get { return m_Width; } }

		/// <summary>
		/// Gets the image height
		/// </summary>
		public int			Height					{ get { return m_Height; } }

		/// <summary>
		/// Tells if the image has an alpha channel
		/// </summary>
		public bool			HasAlpha				{ get { return m_bHasAlpha; } }

		/// <summary>
		/// Gets the image content stored as CIEXYZ + Alpha
		/// </summary>
		/// <remarks>You cannot use XYZ content directly : you must use the ColorProfile to perform color transformations</remarks>
		public Vector4D[,]	ContentXYZ				{ get { return m_Bitmap; } }

		/// <summary>
		/// Gets the image's color profile
		/// </summary>
		public ColorProfile	Profile					{ get { return m_ColorProfile; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Creates a bitmap from a file
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		public	Bitmap2( System.IO.FileInfo _ImageFileName )
		{
			Load( _ImageFileName, GetFileType( _ImageFileName ) );
		}

		/// <summary>
		/// Creates a bitmap from a stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageStream">The image stream to load the bitmap from</param>
		/// <param name="_ImageFileNameName">The name of the image file the stream is coming from originally (used to identify image file type)</param>
		public	Bitmap2( System.IO.Stream _ImageStream, System.IO.FileInfo _ImageFileNameName )
		{
			Load( _ImageStream, GetFileType( _ImageFileNameName ) );
		}

		/// <summary>
		/// Creates a bitmap from a stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageStream">The image stream to load the bitmap from</param>
		/// <param name="_FileType">The image type</param>
		public	Bitmap2( System.IO.Stream _ImageStream, FILE_TYPE _FileType )
		{
			Load( _ImageStream, _FileType );
		}

		/// <summary>
		/// Creates a bitmap from memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageFileContent">The memory buffer to load the bitmap from</param>
		/// <param name="_ImageFileNameName">The name of the image file the stream is coming from originally (used to identify image file type)</param>
		public	Bitmap2( byte[] _ImageFileContent, System.IO.FileInfo _ImageFileNameName )
		{
			Load( _ImageFileContent, GetFileType( _ImageFileNameName ) );
		}

		/// <summary>
		/// Creates a bitmap from memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageFileContent">The memory buffer to load the bitmap from</param>
		/// <param name="_FileType">The image type</param>
		public	Bitmap2( byte[] _ImageFileContent, FILE_TYPE _FileType )
		{
			Load( _ImageFileContent, _FileType );
		}

		/// <summary>
		/// Creates a bitmap from a System.Drawing.Bitmap and a color profile
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_Bitmap">The System.Drawing.Bitmap</param>
		/// <param name="_ColorProfile">The color profile to use transform the bitmap</param>
		public	Bitmap2( System.Drawing.Bitmap _Bitmap, ColorProfile _ColorProfile )
		{
			if ( _ColorProfile == null )
				throw new Exception( "Invalid profile : can't convert to CIE XYZ !" );
			m_ColorProfile = _ColorProfile;

			// Load the bitmap's content and copy it to a double entry array
			byte[]	BitmapContent = LoadBitmap( _Bitmap, out m_Width, out m_Height );

			m_Bitmap = new Vector4D[m_Width,m_Height];

			int	i=0;
			for ( int Y=0; Y < m_Height; Y++ )
				for ( int X=0; X < m_Width; X++ )
				{
					m_Bitmap[X,Y] = new Vector4D(
							BYTE_TO_FLOAT * BitmapContent[i++],	// R
							BYTE_TO_FLOAT * BitmapContent[i++],	// G
							BYTE_TO_FLOAT * BitmapContent[i++],	// B
							BYTE_TO_FLOAT * BitmapContent[i++]	// A
						);
				}

			// Convert to CIE XYZ
			m_ColorProfile.RGB2XYZ( m_Bitmap );
		}

		/// <summary>
		/// Loads from disk
		/// </summary>
		/// <param name="_ImageFileName"></param>
		/// <param name="_FileType"></param>
		public void	Load( System.IO.FileInfo _ImageFileName, FILE_TYPE _FileType )
		{
			using ( System.IO.FileStream ImageStream = _ImageFileName.OpenRead() )
				Load( ImageStream, _FileType );
		}

		/// <summary>
		/// Loads from stream
		/// </summary>
		/// <param name="_ImageStream"></param>
		/// <param name="_FileType"></param>
		public void	Load( System.IO.Stream _ImageStream, FILE_TYPE _FileType )
		{
			// Read the file's content
			byte[]	ImageContent = new byte[_ImageStream.Length];
			_ImageStream.Read( ImageContent, 0, (int) _ImageStream.Length );

			Load( ImageContent, _FileType );
		}

		/// <summary>
		/// Actual load from a byte[] in memory
		/// </summary>
		/// <param name="_ImageFileContent">The source image content as a byte[]</param>
		/// <param name="_FileType">The type of file to load</param>
		/// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
		/// <exception cref="NException">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
		public void	Load( byte[] _ImageFileContent, FILE_TYPE _FileType )
		{
			try
			{
				switch ( _FileType )
				{
					case FILE_TYPE.JPEG:
					case FILE_TYPE.PNG:
					case FILE_TYPE.TIFF:
					case FILE_TYPE.GIF:
					case FILE_TYPE.BMP:
						using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
						{
							// ===== 1] Load the bitmap source =====
							BitmapDecoder	Decoder = BitmapDecoder.Create( Stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnDemand );
							if ( Decoder.Frames.Count == 0 )
								throw new NException( this, "BitmapDecoder failed to read at least one bitmap frame !" );

							BitmapFrame	Frame = Decoder.Frames[0];
							if ( Frame == null )
								throw new NException( this, "Invalid decoded bitmap !" );

// DEBUG
int		StrideX = 4*Frame.PixelWidth;
byte[]	DebugImageSource = new byte[StrideX*Frame.PixelHeight];
Frame.CopyPixels( DebugImageSource, StrideX, 0 );
// DEBUG


// pas de gamma sur les JPEG si non spécifié !
// Il y a bien une magouille faite lors de la conversion par le FormatConvertedBitmap !


							// ===== 2] Build the color profile =====
							m_ColorProfile = new ColorProfile( Frame.Metadata as BitmapMetadata, _FileType );

							// ===== 3] Convert the frame to generic RGBA32F =====
							ConvertFrame( Frame );
// 
// 							m_Width = Frame.PixelWidth;
// 							m_Height = Frame.PixelHeight;
// 
// 							float[]	TempBitmap = new float[m_Width*m_Height*4];
// 							try
// 							{
// 								FormatConvertedBitmap	Converter = new FormatConvertedBitmap();
// 								Converter.BeginInit();
// 								Converter.Source = Frame;
// 								Converter.DestinationFormat = GENERIC_PIXEL_FORMAT;
// 								Converter.EndInit();
// 
// 								int		Stride = Frame.PixelWidth * Utilities.SizeOf<Vector4D>();
// 								Converter.CopyPixels( TempBitmap, Stride, 0 );
// 							}
// 							catch ( Exception _e )
// 							{
// 								throw new NException( this, "Failed to create the bitmap converter to convert from " + Frame.Format + " to " + GENERIC_PIXEL_FORMAT + " pixel format !", _e );
// 							}
// 
// 							// ===== 4] Build the target bitmap =====
// 							m_bHasAlpha = false;
// 							m_Bitmap = new Vector4D[m_Width,m_Height];
// 
// 							int		Position = 0;
// 							Vector4D	Temp;
// 							bool	bPreMultiplied = Frame.Format == System.Windows.Media.PixelFormats.Pbgra32 || Frame.Format == System.Windows.Media.PixelFormats.Prgba64 || Frame.Format == System.Windows.Media.PixelFormats.Prgba128Float;
// 							if ( bPreMultiplied )
// 							{	// Colors are pre-multiplied by alpha !
// 								for ( int Y = 0; Y < m_Height; Y++ )
// 									for ( int X = 0; X < m_Width; X++ )
// 									{
// 										Temp.X = TempBitmap[Position++];
// 										Temp.Y = TempBitmap[Position++];
// 										Temp.Z = TempBitmap[Position++];
// 										Temp.W = TempBitmap[Position++];
// 
// 										float	InvA = Temp.W != 0.0f ? 1.0f / Temp.W : 1.0f;
// 										Temp.X *= InvA;
// 										Temp.Y *= InvA;
// 										Temp.Z *= InvA;
// 
// 										m_Bitmap[X, Y] = Temp;
// 
// 										if ( Temp.W != 1.0f )
// 											m_bHasAlpha = true;
// 									}
// 							}
// 							else
// 							{
// 								for ( int Y = 0; Y < m_Height; Y++ )
// 									for ( int X = 0; X < m_Width; X++ )
// 									{
// 										Temp.X = TempBitmap[Position++];
// 										Temp.Y = TempBitmap[Position++];
// 										Temp.Z = TempBitmap[Position++];
// 										Temp.W = TempBitmap[Position++];
// 
// 										Temp.X = (float) Math.Pow( Temp.X, 1.0f / 2.2f );	// Correct the behind the scene gamma correction
// 										Temp.Y = (float) Math.Pow( Temp.Y, 1.0f / 2.2f );
// 										Temp.Z = (float) Math.Pow( Temp.Z, 1.0f / 2.2f );
// 
// 										m_Bitmap[X, Y] = Temp;
// 
// 										if ( Temp.W != 1.0f )
// 											m_bHasAlpha = true;
// 									}
// 							}

// DEBUG
byte[]	DebugImage = new byte[4*m_Width*m_Height];
for ( int Y=0; Y < m_Height; Y++ )
	for ( int X=0; X < m_Width; X++ )
	{
		DebugImage[4*(m_Width*Y+X)+0] = (byte) Math.Min( 255, Math.Max( 0, (255.0f * m_Bitmap[X,Y].X) ) );
		DebugImage[4*(m_Width*Y+X)+1] = (byte) Math.Min( 255, Math.Max( 0, (255.0f * m_Bitmap[X,Y].Y) ) );
		DebugImage[4*(m_Width*Y+X)+2] = (byte) Math.Min( 255, Math.Max( 0, (255.0f * m_Bitmap[X,Y].Z) ) );
		DebugImage[4*(m_Width*Y+X)+3] = (byte) Math.Min( 255, Math.Max( 0, (255.0f * m_Bitmap[X,Y].W) ) );
	}
// DEBUG



							// ===== 5] Convert to CIE XYZ (our device-independent profile connection space) =====
							m_ColorProfile.RGB2XYZ( m_Bitmap );
						}
						break;

					case FILE_TYPE.TGA:
						{
							// Load as a System.Drawing.Bitmap and convert to Vector4D
							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
								using ( Nuaj.Helpers.TargaImage TGA = new Nuaj.Helpers.TargaImage( Stream ) )
								{
									// Create a default sRGB linear color profile
									m_ColorProfile = new ColorProfile(
											ColorProfile.Chromaticities.sRGB,	// Use default sRGB color profile
											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
											TGA.ExtensionArea.GammaRatio		// ...whose gamma is retrieved from extension data
										);

									// Convert
									byte[]	ImageContent = LoadBitmap( TGA.Image, out m_Width, out m_Height );
									m_Bitmap = new Vector4D[m_Width,m_Height];
									byte	A;
									int		i = 0;
									for ( int Y=0; Y < m_Height; Y++ )
										for ( int X=0; X < m_Width; X++ )
										{
											m_Bitmap[X,Y].X = BYTE_TO_FLOAT * ImageContent[i++];
											m_Bitmap[X,Y].Y = BYTE_TO_FLOAT * ImageContent[i++];
											m_Bitmap[X,Y].Z = BYTE_TO_FLOAT * ImageContent[i++];

											A = ImageContent[i++];
											m_bHasAlpha |= A != 0xFF;

											m_Bitmap[X,Y].W = BYTE_TO_FLOAT * A;
										}

									// Convert to CIEXYZ
									m_ColorProfile.RGB2XYZ( m_Bitmap );
								}
							return;
						}

					case FILE_TYPE.HDR:
						{
							// Load as XYZ
							m_Bitmap = LoadAndDecodeHDRFormat( _ImageFileContent, true, out m_ColorProfile );
							m_Width = m_Bitmap.GetLength( 0 );
							m_Height = m_Bitmap.GetLength( 1 );
							return;
						}

					default:
						throw new NotSupportedException( "The image file type \"" + _FileType + "\" is not supported by the Bitmap class !" );
				}
			}
			catch ( Exception )
			{
				throw;	// Go on !
			}
			finally
			{
			}
		}

		/// <summary>
		/// Converts the source bitmap to a generic RGBA32F format
		/// </summary>
		/// <param name="_Frame">The source frame to convert</param>
		/// <remarks>I cannot use the FormatConvertedBitmap class because it applies some unwanted gamma correction depending on the source pixel format.
		/// For example, if the image is using the Bgr24 format (it uses a 1/2.2 gamma internally) so converting that to our generic format Rgba128Float
		/// (that uses a gamma of 1 internally) will automatically apply a pow( 2.2 ) to the RGB values, which is NOT what we're looking for since we're
		/// handling gamma correction ourselves here !
		/// </remarks>
		protected void	ConvertFrame( BitmapSource _Frame )
		{
			m_Width = _Frame.PixelWidth;
			m_Height = _Frame.PixelHeight;
			m_Bitmap = new Vector4D[m_Width,m_Height];

			int		W = m_Width;
			int		H = m_Height;
			Vector4D	Temp = new Vector4D();

			//////////////////////////////////////////////////////////////////////////
			// BGR24
			if ( _Frame.Format == System.Windows.Media.PixelFormats.Bgr24 )
			{	
				int		Stride = 3*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				Temp.W = 1.0f;
				m_bHasAlpha = false;
				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.Z = BYTE_TO_FLOAT * Content[Position++];
						Temp.Y = BYTE_TO_FLOAT * Content[Position++];
						Temp.X = BYTE_TO_FLOAT * Content[Position++];
						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGR32
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Bgr32 )
			{	
				int		Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				Temp.W = 1.0f;
				m_bHasAlpha = false;
				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.Z = BYTE_TO_FLOAT * Content[Position++];
						Temp.Y = BYTE_TO_FLOAT * Content[Position++];
						Temp.X = BYTE_TO_FLOAT * Content[Position++];
						Position++;
						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGRA32
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Bgra32 )
			{	
				int		Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				byte	A = 0;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.Z = BYTE_TO_FLOAT * Content[Position++];
						Temp.Y = BYTE_TO_FLOAT * Content[Position++];
						Temp.X = BYTE_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						Temp.W = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFF;

						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PBGRA32 (Pre-Multiplied)
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Pbgra32 )
			{	
				int		Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				byte	A = 0;
				float	InvA;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.Z = BYTE_TO_FLOAT * Content[Position++];
						Temp.Y = BYTE_TO_FLOAT * Content[Position++];
						Temp.X = BYTE_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						Temp.W = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFF;

						// De-premultiply
						InvA = A != 0 ? 1.0f / Temp.W : 1.0f;
						Temp.X *= InvA;
						Temp.Y *= InvA;
						Temp.Z *= InvA;

						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGB48
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Rgb48 )
			{	
				int			Stride = 6*W;
				ushort[]	Content = new ushort[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				m_bHasAlpha = false;
				Temp.W = 1.0f;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = WORD_TO_FLOAT * Content[Position++];
						Temp.Y = WORD_TO_FLOAT * Content[Position++];
						Temp.Z = WORD_TO_FLOAT * Content[Position++];
						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGBA64
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Rgba64 )
			{	
				int			Stride = 8*W;
				ushort[]	Content = new ushort[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				ushort	A = 0;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = WORD_TO_FLOAT * Content[Position++];
						Temp.Y = WORD_TO_FLOAT * Content[Position++];
						Temp.Z = WORD_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						Temp.W = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFFFF;

						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA64 (Pre-Multiplied)
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Rgba64 )
			{	
				int			Stride = 8*W;
				ushort[]	Content = new ushort[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				ushort	A = 0;
				float	InvA;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = WORD_TO_FLOAT * Content[Position++];
						Temp.Y = WORD_TO_FLOAT * Content[Position++];
						Temp.Z = WORD_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						Temp.W = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFFFF;

						// De-premultiply
						InvA = A != 0 ? 1.0f / Temp.W : 1.0f;
						Temp.X *= InvA;
						Temp.Y *= InvA;
						Temp.Z *= InvA;

						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGBA128F
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Rgba128Float )
			{	
				int		Stride = 16*W;
				float[]	Content = new float[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = Content[Position++];
						Temp.Y = Content[Position++];
						Temp.Z = Content[Position++];
						Temp.W = Content[Position++];

						m_bHasAlpha |= Temp.W != 1.0f;

						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA128F (Pre-Multiplied)
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Prgba128Float )
			{	
				int		Stride = 16*W;
				float[]	Content = new float[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				float	InvA;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = Content[Position++];
						Temp.Y = Content[Position++];
						Temp.Z = Content[Position++];
						Temp.W = Content[Position++];

						m_bHasAlpha |= Temp.W != 1.0f;

						// De-Premultiply
						InvA = Temp.W != 0.0f ? 1.0f / Temp.W : 1.0f;
						Temp.X *= InvA;
						Temp.Y *= InvA;
						Temp.Z *= InvA;

						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray16
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Gray16 )
			{	
				int			Stride = 2*W;
				ushort[]	Content = new ushort[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				m_bHasAlpha = false;
				Temp.W = 1.0f;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = Temp.Y = Temp.Z = WORD_TO_FLOAT * Content[Position++];
						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray32F
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Gray32Float )
			{	
				int		Stride = 4*W;
				float[]	Content = new float[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				m_bHasAlpha = false;
				Temp.W = 1.0f;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = Temp.Y = Temp.Z = Content[Position++];
						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray8
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Gray8 )
			{	
				int		Stride = 1*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				m_bHasAlpha = false;
				Temp.W = 1.0f;
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Temp.X = Temp.Y = Temp.Z = BYTE_TO_FLOAT * Content[Position++];
						m_Bitmap[X,Y] = Temp;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// 256 Colors Palette
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Indexed8 )
			{	
				int		Stride = 1*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				Vector4D[]	Palette = new Vector4D[_Frame.Palette.Colors.Count];
				for ( int i=0; i < Palette.Length; i++ )
				{
					System.Windows.Media.Color	C = _Frame.Palette.Colors[i];
					Palette[i] = new Vector4D(
						C.R * BYTE_TO_FLOAT,
						C.G * BYTE_TO_FLOAT,
						C.B * BYTE_TO_FLOAT,
						C.A * BYTE_TO_FLOAT );

					m_bHasAlpha |= C.A != 0xFF;
				}

				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
						m_Bitmap[X,Y] = Palette[Content[Position++]];
			}
			else
				throw new Exception( "Source format " + _Frame.Format + " not supported !" );
		}

// 		/// <summary>
// 		/// Builds a DataStream from a byte[]
// 		/// </summary>
// 		/// <param name="_ImageFileContent">The image file content as a byte[]</param>
// 		/// <param name="_Action">The action to perform with a DataStream created from the byte[]</param>
// 		protected void	BuildDataStream( byte[] _ImageFileContent, Action<DataStream> _Action )
// 		{
// 			Utilities.Pin( _ImageFileContent, ( IntPtr _FileConterPointer ) =>
// 				{
// 					using ( DataStream MemoryStream = new DataStream( _FileConterPointer, _ImageFileContent.Length, true, false ) )
// 						_Action( MemoryStream );
// 				} );
// 		}

		/// <summary>
		/// Loads a System.Drawing.Bitmap into a byte[] containing RGBARGBARG... pixels
		/// </summary>
		/// <param name="_Bitmap">The source System.Drawing.Bitmap to load</param>
		/// <param name="_Width">The bitmap's width</param>
		/// <param name="_Height">The bitmaps's height</param>
		/// <returns>The byte array containing a sequence of R,G,B,A,R,G,B,A pixels and of length Widht*Height*4</returns>
		public static unsafe byte[]	LoadBitmap( System.Drawing.Bitmap _Bitmap, out int _Width, out int _Height )
		{
			byte[]	Result = null;
			byte*	pScanline;
			byte	R, G, B, A;

			_Width = _Bitmap.Width;
			_Height = _Bitmap.Height;
			Result = new byte[4*_Width*_Height];

			System.Drawing.Imaging.BitmapData	LockedBitmap = _Bitmap.LockBits( new System.Drawing.Rectangle( 0, 0, _Width, _Height ), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			for ( int Y=0; Y < _Height; Y++ )
			{
				pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
				for ( int X=0; X < _Width; X++ )
				{
					// Read in shitty order
					B = *pScanline++;
					G = *pScanline++;
					R = *pScanline++;
					A = *pScanline++;

					// Write in correct order
					Result[((_Width*Y+X)<<2) + 0] = R;
					Result[((_Width*Y+X)<<2) + 1] = G;
					Result[((_Width*Y+X)<<2) + 2] = B;
					Result[((_Width*Y+X)<<2) + 3] = A;
				}
			}

			_Bitmap.UnlockBits( LockedBitmap );

			return Result;
		}

		/// <summary>
		/// Retrieves the image file type based on the image file name
		/// </summary>
		/// <param name="_ImageFileNameName">The image file name</param>
		/// <returns></returns>
		public static FILE_TYPE	GetFileType( System.IO.FileInfo _ImageFileNameName )
		{
			string	Extension = _ImageFileNameName.Extension.ToUpper();
			switch ( Extension )
			{
				case	".JPG":
				case	".JPEG":
				case	".JPE":
					return FILE_TYPE.JPEG;

				case	".PNG":
					return FILE_TYPE.PNG;

				case	".TGA":
					return FILE_TYPE.TGA;

				case	".HDR":
				case	".RGBE":
					return FILE_TYPE.HDR;

				case	".TIF":
				case	".TIFF":
					return FILE_TYPE.TIFF;

				case	".BMP":
					return FILE_TYPE.BMP;

				case	".GIF":
					return FILE_TYPE.GIF;
			}

			return FILE_TYPE.UNKNOWN;
		}

/*
		/// <summary>
		/// Creates an image from a bitmap
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess ) : this( _Device, _Name, _Image, false, _MipLevelsCount, _ImageGamma, _PreProcess )
		{
		}
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, int _MipLevelsCount, float _ImageGamma ) : this( _Device, _Name, _Image, false, _MipLevelsCount, _ImageGamma, null )	{}

		/// <summary>
		/// Creates an image from a bitmap
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, bool _MirrorY, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess ) : base( _Device, _Name )
		{
			m_Width = _Image.Width;
			m_Height = _Image.Height;
			m_MipLevelsCount = ComputeMipLevelsCount( _MipLevelsCount );
			m_DataStreams = new DataStream[m_MipLevelsCount];
			m_DataRectangles = new DataRectangle[m_MipLevelsCount];

			Load( _Image, _MirrorY, _ImageGamma, _PreProcess );
		}
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, bool _MirrorY, int _MipLevelsCount, float _ImageGamma ) : this( _Device, _Name, _Image, _MirrorY, _MipLevelsCount, _ImageGamma, null )	{}

		/// <summary>
		/// Creates an image from a bitmap and an alpha
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, System.Drawing.Bitmap _Alpha, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess ) : this( _Device, _Name, _Image, _Alpha, false, _MipLevelsCount, _ImageGamma, _PreProcess )
		{
		}
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, System.Drawing.Bitmap _Alpha, int _MipLevelsCount, float _ImageGamma ) : this( _Device, _Name, _Image, _Alpha, _MipLevelsCount, _ImageGamma, null )	{}

		/// <summary>
		/// Creates an image from a bitmap and an alpha
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, System.Drawing.Bitmap _Alpha, bool _MirrorY, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess ) : base( _Device, _Name )
		{
			m_Width = _Image.Width;
			m_Height = _Image.Height;
			m_MipLevelsCount = ComputeMipLevelsCount( _MipLevelsCount );
			m_DataStreams = new DataStream[m_MipLevelsCount];
			m_DataRectangles = new DataRectangle[m_MipLevelsCount];

			Load( _Image, _Alpha, _MirrorY, _ImageGamma, _PreProcess );
		}
		public	Image( Device _Device, string _Name, System.Drawing.Bitmap _Image, System.Drawing.Bitmap _Alpha, bool _MirrorY, int _MipLevelsCount, float _ImageGamma ) : this( _Device, _Name, _Image, _Alpha, _MirrorY, _MipLevelsCount, _ImageGamma, null )	{}

		/// <summary>
		/// Creates an image from a HDR array
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		public	Image( Device _Device, string _Name, Vector4D[,] _Image, float _Exposure, int _MipLevelsCount, ImageProcessDelegate _PreProcess ) : base( _Device, _Name )
		{
			m_Width = _Image.GetLength( 0 );
			m_Height = _Image.GetLength( 1 );
			m_MipLevelsCount = ComputeMipLevelsCount( _MipLevelsCount );
			m_DataStreams = new DataStream[m_MipLevelsCount];
			m_DataRectangles = new DataRectangle[m_MipLevelsCount];

			Load( _Image, _Exposure, _PreProcess );
		}
		public	Image( Device _Device, string _Name, Vector4D[,] _Image, float _Exposure, int _MipLevelsCount ) : this( _Device, _Name, _Image, _Exposure, _MipLevelsCount, null ) {}

		/// <summary>
		/// Creates a custom image using a pixel writer
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		public	Image( Device _Device, string _Name, int _Width, int _Height, ImageProcessDelegate _PixelWriter, int _MipLevelsCount ) : base( _Device, _Name )
		{
			if ( _PixelWriter == null )
				throw new NException( this, "Invalid pixel writer !" );

			m_Width = _Width;
			m_Height = _Height;
			m_MipLevelsCount = ComputeMipLevelsCount( _MipLevelsCount );
			m_DataStreams = new DataStream[m_MipLevelsCount];
			m_DataRectangles = new DataRectangle[m_MipLevelsCount];

			int	PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

			// Create the data rectangle
			try
			{
				PF[]	Scanline = new PF[m_Width];
				m_DataStreams[0] = ToDispose( new DataStream( m_Width * m_Height * PixelSize, true, true ) );

				// Convert all scanlines into the desired format and write them into the stream
				Vector4D	C = new Vector4D();
				for ( int Y=0; Y < m_Height; Y++ )
				{
					for ( int X=0; X < m_Width; X++ )
					{
						_PixelWriter( X, Y, ref C );
						Scanline[X].Write( C );

						m_bHasAlpha |= C.W != 1.0f;	// Check if it has alpha...
					}

					m_DataStreams[0].WriteRange<PF>( Scanline );
				}

				// Build the data rectangle from that stream
				m_DataRectangles[0] = new DataRectangle( m_DataStreams[0].DataPointer, m_Width*PixelSize );

				// Build mip levels
				BuildMissingMipLevels();
			}
			catch ( Exception _e )
			{
				throw new NException( this, "An error occurred while creating the custom image !", _e );
			}
		}

		/// <summary>
		/// Loads a LDR image from memory
		/// </summary>
		/// <param name="_Image">Source image to load</param>
		/// <param name="_MirrorY">True to mirror the image vertically</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		public void	Load( System.Drawing.Bitmap _Image, bool _MirrorY, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			if ( _Image.Width != m_Width )
				throw new NException( this, "Provided image width mismatch !" );
			if ( _Image.Height != m_Height )
				throw new NException( this, "Provided image height mismatch !" );

			int		PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

#if DEBUG
			// Ensure we're passing a unit image gamma
			bool	bUsesSRGB = new PF().sRGB;
			if ( bUsesSRGB && Math.Abs( _ImageGamma - 1.0f ) > 1e-3f )
				throw new NException( this, "You specified a sRGB pixel format but provided an image gamma different from 1 !" );
#endif

			byte[]	ImageContent = LoadBitmap( _Image, out m_Width, out m_Height );

			// Create the data rectangle
			try
			{
				PF[]	Scanline = new PF[m_Width];
				m_DataStreams[0] = ToDispose( new DataStream( m_Width * m_Height * PixelSize, true, true ) );

				// Convert all scanlines into the desired format and write them into the stream
				Vector4D	Temp = new Vector4D();
				int		Offset;
				for ( int Y=0; Y < m_Height; Y++ )
				{
					Offset = (m_Width * (_MirrorY ? m_Height-1-Y : Y)) << 2;
					for ( int X=0; X < m_Width; X++ )
					{
						Temp.X = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
						Temp.Y = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
						Temp.Z = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
						Temp.W = BYTE_TO_FLOAT * ImageContent[Offset++];

						if ( _PreProcess != null )
							_PreProcess( X, Y, ref Temp );

						Scanline[X].Write( Temp );

						m_bHasAlpha |= Scanline[X].Alpha != 1.0f;	// Check if it has alpha...
					}

					m_DataStreams[0].WriteRange<PF>( Scanline );
				}

				// Build the data rectangle from that stream
				m_DataRectangles[0] = new DataRectangle( m_DataStreams[0].DataPointer, m_Width*PixelSize );

				// Build mip levels
				BuildMissingMipLevels();
			}
			catch ( Exception _e )
			{
				throw new NException( this, "An error occurred while loading the image !", _e );
			}
		}

		/// <summary>
		/// Loads a LDR image and its alpha from memory
		/// </summary>
		/// <param name="_Image">Source image to load</param>
		/// <param name="_Alpha">Source alpha to load</param>
		/// <param name="_MirrorY">True to mirror the images vertically</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		public void	Load( System.Drawing.Bitmap _Image, System.Drawing.Bitmap _Alpha, bool _MirrorY, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			if ( _Image.Width != m_Width )
				throw new NException( this, "Provided image width mismatch !" );
			if ( _Image.Height != m_Height )
				throw new NException( this, "Provided image height mismatch !" );
			if ( _Alpha.Width != m_Width )
				throw new NException( this, "Provided alpha width mismatch !" );
			if ( _Alpha.Height != m_Height )
				throw new NException( this, "Provided alpha height mismatch !" );

			int		PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

#if DEBUG
			// Ensure we're passing a unit image gamma
			bool	bUsesSRGB = new PF().sRGB;
			if ( bUsesSRGB && Math.Abs( _ImageGamma - 1.0f ) > 1e-3f )
				throw new NException( this, "You specified a sRGB pixel format but provided an image gamma different from 1 !" );
#endif
			// Lock source image
			byte[]	ImageContent = LoadBitmap( _Image, out m_Width, out m_Height );
			byte[]	AlphaContent = LoadBitmap( _Alpha, out m_Width, out m_Height );

			m_bHasAlpha = true;	// We know it has alpha...

			// Create the data rectangle
			try
			{
				PF[]	Scanline = new PF[m_Width];
				m_DataStreams[0] = ToDispose( new DataStream( m_Width * m_Height * PixelSize, true, true ) );

				// Convert all scanlines into the desired format and write them into the stream
				int		Offset;
				Vector4D	Temp = new Vector4D();

				for ( int Y=0; Y < m_Height; Y++ )
				{
					Offset = (m_Width * (_MirrorY ? m_Height-1-Y : Y)) << 2;
					for ( int X=0; X < m_Width; X++, Offset+=4 )
					{
						Temp.X = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset+0], _ImageGamma );
						Temp.Y = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset+1], _ImageGamma );
						Temp.Z = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset+2], _ImageGamma );
						Temp.W = BYTE_TO_FLOAT * AlphaContent[Offset+0];

						if ( _PreProcess != null )
							_PreProcess( X, Y, ref Temp );

						Scanline[X].Write( Temp );
					}

					m_DataStreams[0].WriteRange<PF>( Scanline );
				}

				// Build the data rectangle from that stream
				m_DataRectangles[0] = new DataRectangle( m_DataStreams[0].DataPointer, m_Width*PixelSize );

				// Build mip levels
				BuildMissingMipLevels();
			}
			catch ( Exception _e )
			{
				throw new NException( this, "An error occurred while loading the image !", _e );
			}
		}

		/// <summary>
		/// Loads a HDR image from memory
		/// </summary>
		/// <param name="_Image">Source image to load</param>
		/// <param name="_Exposure">The exposure correction to apply (default should be 0)</param>
		public void	Load( Vector4D[,] _Image, float _Exposure, ImageProcessDelegate _PreProcess )
		{
			int	Width = _Image.GetLength( 0 );
			if ( Width != m_Width )
				throw new NException( this, "Provided image width mismatch !" );

			int	Height = _Image.GetLength( 1 );
			if ( Height != m_Height )
				throw new NException( this, "Provided image height mismatch !" );

			int	PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

			// Create the data rectangle
			PF[]	Scanline = new PF[m_Width];
			try
			{
				m_DataStreams[0] = ToDispose( new DataStream( m_Width * m_Height * PixelSize, true, true ) );

				// Convert all scanlines into the desired format and write them into the stream
				Vector4D	Temp = new Vector4D();
				for ( int Y=0; Y < m_Height; Y++ )
				{
					for ( int X=0; X < m_Width; X++ )
					{
						float	fLuminance = 0.3f * _Image[X,Y].X + 0.5f * _Image[X,Y].Y + 0.2f * _Image[X,Y].Z;
						float	fCorrectedLuminance = (float) Math.Pow( 2.0f, Math.Log( fLuminance ) / Math.Log( 2.0 ) + _Exposure );

						Temp = _Image[X,Y] * fCorrectedLuminance / fLuminance;

						if ( _PreProcess != null )
							_PreProcess( X, Y, ref Temp );

						Scanline[X].Write( Temp );

						m_bHasAlpha |= Scanline[X].Alpha != 1.0f;	// Check if it has alpha...
					}

					m_DataStreams[0].WriteRange<PF>( Scanline );
				}

				// Build the data rectangle from that stream
				m_DataRectangles[0] = new DataRectangle( m_DataStreams[0].DataPointer, m_Width*PixelSize );

				// Build mip levels
				BuildMissingMipLevels();
			}
			catch ( Exception _e )
			{
				throw new NException( this, "An error occurred while loading the image !", _e );
			}
		}

		/// <summary>
		/// Creates an image from a bitmap file
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_FileName"></param>
		/// <param name="_MipLevelsCount"></param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static Image<PF>	CreateFromBitmapFile( Device _Device, string _Name, System.IO.FileInfo _FileName, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			using ( System.Drawing.Bitmap B = System.Drawing.Bitmap.FromFile( _FileName.FullName ) as System.Drawing.Bitmap )
				return new Image<PF>( _Device, _Name, B, _MipLevelsCount, _ImageGamma, _PreProcess );
		}
		public static Image<PF>	CreateFromBitmapFile( Device _Device, string _Name, System.IO.FileInfo _FileName, int _MipLevelsCount, float _ImageGamma )
		{
			return CreateFromBitmapFile( _Device, _Name, _FileName, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an image array from several bitmap files
		/// (the images must all be disposed of properly by the caller)
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_FileNames"></param>
		/// <param name="_MipLevelsCount"></param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static Image<PF>[]	CreateFromBitmapFiles( Device _Device, string _Name, System.IO.FileInfo[] _FileNames, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			// Create the array of images
			Image<PF>[]	Images = new Image<PF>[_FileNames.Length];
			for ( int ImageIndex=0; ImageIndex < Images.Length; ImageIndex++ )
				using ( System.Drawing.Bitmap B = System.Drawing.Bitmap.FromFile( _FileNames[ImageIndex].FullName ) as System.Drawing.Bitmap )
					Images[ImageIndex] = new Image<PF>( _Device, _Name, B, _MipLevelsCount, _ImageGamma, _PreProcess );

			return Images;
		}
		public static Image<PF>[]	CreateFromBitmapFiles( Device _Device, string _Name, System.IO.FileInfo[] _FileNames, int _MipLevelsCount, float _ImageGamma )
		{
			return CreateFromBitmapFiles( _Device, _Name, _FileNames, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an image from a bitmap in memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_BitmapFileContent"></param>
		/// <param name="_MipLevelsCount"></param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static Image<PF>	CreateFromBitmapFileInMemory( Device _Device, string _Name, byte[] _BitmapFileContent, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _BitmapFileContent ) )
				using ( System.Drawing.Bitmap B = System.Drawing.Bitmap.FromStream( Stream ) as System.Drawing.Bitmap )
					return new Image<PF>( _Device, _Name, B, _MipLevelsCount, _ImageGamma, _PreProcess );
		}
		public static Image<PF>	CreateFromBitmapFileInMemory( Device _Device, string _Name, byte[] _BitmapFileContent, int _MipLevelsCount, float _ImageGamma )
		{
			return CreateFromBitmapFileInMemory( _Device, _Name, _BitmapFileContent, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an image array from several bitmaps in memory
		/// (the images must all be disposed of properly by the caller)
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_BitmapFileContents"></param>
		/// <param name="_MipLevelsCount"></param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static Image<PF>[]	CreateFromBitmapFilesInMemory( Device _Device, string _Name, byte[][] _BitmapFileContents, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			// Create the array of images
			Image<PF>[]	Images = new Image<PF>[_BitmapFileContents.GetLength( 1 )];
			for ( int ImageIndex=0; ImageIndex < Images.Length; ImageIndex++ )
				using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _BitmapFileContents[ImageIndex] ) )
					using ( System.Drawing.Bitmap B = System.Drawing.Bitmap.FromStream( Stream ) as System.Drawing.Bitmap )
						Images[ImageIndex] = new Image<PF>( _Device, _Name, B, _MipLevelsCount, _ImageGamma, _PreProcess );

			return Images;
		}
		public static Image<PF>[]	CreateFromBitmapFilesInMemory( Device _Device, string _Name, byte[][] _BitmapFileContents, int _MipLevelsCount, float _ImageGamma )
		{
			return CreateFromBitmapFilesInMemory( _Device, _Name, _BitmapFileContents, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an image from a bitmap stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_BitmapStream"></param>
		/// <param name="_MipLevelsCount"></param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static Image<PF>	CreateFromBitmapStream( Device _Device, string _Name, System.IO.Stream _BitmapStream, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			using ( System.Drawing.Bitmap B = System.Drawing.Bitmap.FromStream( _BitmapStream ) as System.Drawing.Bitmap )
				return new Image<PF>( _Device, _Name, B, _MipLevelsCount, _ImageGamma, _PreProcess );
		}
		public static Image<PF>	CreateFromBitmapStream( Device _Device, string _Name, System.IO.Stream _BitmapStream, int _MipLevelsCount, float _ImageGamma )
		{
			return CreateFromBitmapStream( _Device, _Name, _BitmapStream, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an image array from several bitmap streams
		/// (the images must all be disposed of properly by the caller)
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_BitmapStreams"></param>
		/// <param name="_MipLevelsCount"></param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		/// <remarks>Contrary to the 2 methods below that rely on "D3DX11CreateTextureFromMemory()", this method returns a texture of known dimension.</remarks>
		public static Image<PF>[]	CreateFromBitmapStreams( Device _Device, string _Name, System.IO.Stream[] _BitmapStreams, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			// Create the array of images
			Image<PF>[]	Images = new Image<PF>[_BitmapStreams.GetLength( 1 )];
			for ( int ImageIndex=0; ImageIndex < Images.Length; ImageIndex++ )
				using ( System.Drawing.Bitmap B = System.Drawing.Bitmap.FromStream( _BitmapStreams[ImageIndex] ) as System.Drawing.Bitmap )
					Images[ImageIndex] = new Image<PF>( _Device, _Name, B, _MipLevelsCount, _ImageGamma, _PreProcess );

			return Images;
		}
		public static Image<PF>[]	CreateFromBitmapStreams( Device _Device, string _Name, System.IO.Stream[] _BitmapStreams, int _MipLevelsCount, float _ImageGamma )
		{
			return CreateFromBitmapStreams( _Device, _Name, _BitmapStreams, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an array of sprite images from a single Texture Page
		/// </summary>
		/// <param name="_SpriteWidth">Width of the sprites</param>
		/// <param name="_SpriteHeight">Height of the sprites</param>
		/// <param name="_TPage">The source TPage</param>
		/// <param name="_MipLevelsCount">The amount of mip levels to create for each image</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns>An array of sprite images, all having the same width, height and mips count (ideal to create a texture array)</returns>
		public static Image<PF>[]	LoadFromTPage( Device _Device, string _Name, int _SpriteWidth, int _SpriteHeight, System.Drawing.Bitmap _TPage, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			// Create the crop rectangles
			int	RectanglesCountX = _TPage.Width / _SpriteWidth;
			int	RectanglesCountY = _TPage.Height / _SpriteHeight;

			System.Drawing.Rectangle[]	CropRectangles = new System.Drawing.Rectangle[RectanglesCountX*RectanglesCountY];
			for ( int RectangleIndexX=0; RectangleIndexX < RectanglesCountX; RectangleIndexX++ )
				for ( int RectangleIndexY=0; RectangleIndexY < RectanglesCountY; RectangleIndexY++ )
					CropRectangles[RectangleIndexY*RectanglesCountX+RectangleIndexX] = new System.Drawing.Rectangle(
						RectangleIndexX * _SpriteWidth,
						RectangleIndexY * _SpriteHeight,
						_SpriteWidth,
						_SpriteHeight
						);

			// Create the sprites
			return LoadFromTPage( _Device, _Name, CropRectangles, _TPage, _MipLevelsCount, _ImageGamma, _PreProcess );
		}
		public static Image<PF>[]	LoadFromTPage( Device _Device, string _Name, int _SpriteWidth, int _SpriteHeight, System.Drawing.Bitmap _TPage, int _MipLevelsCount, float _ImageGamma )
		{
			return LoadFromTPage( _Device, _Name, _SpriteWidth, _SpriteHeight, _TPage, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Creates an array of images from a single Texture Page
		/// </summary>
		/// <param name="_CropRectangles">The array of crop rectangles to isolate individual sprites</param>
		/// <param name="_TPage">The source TPage</param>
		/// <param name="_MipLevelsCount">The amount of mip levels to create for each image</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static Image<PF>[]	LoadFromTPage( Device _Device, string _Name, System.Drawing.Rectangle[] _CropRectangles, System.Drawing.Bitmap _TPage, int _MipLevelsCount, float _ImageGamma, ImageProcessDelegate _PreProcess )
		{
			int		Width, Height;
			byte[]	ImageContent = LoadBitmap( _TPage, out Width, out Height );
			int		Offset;

			Image<PF>[]	Result = new Image<PF>[_CropRectangles.Length];
			for ( int CropRectangleIndex=0; CropRectangleIndex < _CropRectangles.Length; CropRectangleIndex++ )
			{
				System.Drawing.Rectangle	Rect = _CropRectangles[CropRectangleIndex];
				Result[CropRectangleIndex] = new Image<PF>( _Device, _Name, Rect.Width, Rect.Height,
					( int _X, int _Y, ref Vector4D _Color ) =>
						{
							Offset = ((Rect.Y + _Y) + (Rect.X + _X)) << 2;

							_Color.X = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
							_Color.Y = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
							_Color.Z = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
							_Color.W = BYTE_TO_FLOAT * ImageContent[Offset++];

							if ( _PreProcess != null )
								_PreProcess( _X, _Y, ref _Color );
						},
						_MipLevelsCount );
			}

			return Result;
		}
		public static Image<PF>[]	LoadFromTPage( Device _Device, string _Name, System.Drawing.Rectangle[] _CropRectangles, System.Drawing.Bitmap _TPage, int _MipLevelsCount, float _ImageGamma )
		{
			return LoadFromTPage( _Device, _Name, _CropRectangles, _TPage, _MipLevelsCount, _ImageGamma, null );
		}

		/// <summary>
		/// Loads a bitmap from a stream into a byte[] of RGBA values
		/// Read the array like this :
		/// byte R = ReturnedArray[((Width*Y+X)<<2) + 0];
		/// byte G = ReturnedArray[((Width*Y+X)<<2) + 1];
		/// byte B = ReturnedArray[((Width*Y+X)<<2) + 2];
		/// byte A = ReturnedArray[((Width*Y+X)<<2) + 3];
		/// </summary>
		/// <param name="_BitmapStream"></param>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		/// <returns></returns>
		public static byte[]	LoadBitmap( System.IO.Stream _BitmapStream, out int _Width, out int _Height )
		{
			using ( System.Drawing.Bitmap Bitmap = System.Drawing.Bitmap.FromStream( _BitmapStream ) as System.Drawing.Bitmap )
			{
				return LoadBitmap( Bitmap, out _Width, out _Height );
			}
		}

		/// <summary>
		/// Loads a bitmap into a byte[] of RGBA values
		/// Read the array like this :
		/// byte R = ReturnedArray[((Width*Y+X)<<2) + 0];
		/// byte G = ReturnedArray[((Width*Y+X)<<2) + 1];
		/// byte B = ReturnedArray[((Width*Y+X)<<2) + 2];
		/// byte A = ReturnedArray[((Width*Y+X)<<2) + 3];
		/// </summary>
		/// <param name="_BitmapStream"></param>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		/// <returns></returns>
		public static unsafe byte[]	LoadBitmap( System.Drawing.Bitmap _Bitmap, out int _Width, out int _Height )
		{
			byte[]	Result = null;
			byte*	pScanline;
			byte	R, G, B, A;

			_Width = _Bitmap.Width;
			_Height = _Bitmap.Height;
			Result = new byte[4*_Width*_Height];

			System.Drawing.Imaging.BitmapData	LockedBitmap = _Bitmap.LockBits( new System.Drawing.Rectangle( 0, 0, _Width, _Height ), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			for ( int Y=0; Y < _Height; Y++ )
			{
				pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
				for ( int X=0; X < _Width; X++ )
				{
					// Read in shitty order
					B = *pScanline++;
					G = *pScanline++;
					R = *pScanline++;
					A = *pScanline++;

					// Write in correct order
					Result[((_Width*Y+X)<<2) + 0] = R;
					Result[((_Width*Y+X)<<2) + 1] = G;
					Result[((_Width*Y+X)<<2) + 2] = B;
					Result[((_Width*Y+X)<<2) + 3] = A;
				}
			}
			_Bitmap.UnlockBits( LockedBitmap );

			return Result;
		}

		/// <summary>
		/// Applies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-correct</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		public static float		GammaCorrect( float c, float _ImageGamma )
		{
			if ( _ImageGamma == GAMMA_SRGB )
				return Linear2sRGB( c );

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
			if ( _ImageGamma == GAMMA_SRGB )
				return sRGB2Linear( c );

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

			return 1.055f * (float) Math.Pow( c, 1.0f / 2.4f ) - 0.055f;
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

			return (float) Math.Pow( (c + 0.055f) / 1.055f, 2.4f );
		}
*/

		#region HDR Loaders

		/// <summary>
		/// Loads a bitmap in .HDR format into a Vector4D array directly useable by the image constructor
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static Vector4D[,]	LoadAndDecodeHDRFormat( byte[] _HDRFormatBinary, bool _bTargetNeedsXYZ, out ColorProfile _ColorProfile )
		{
			bool	bSourceIsXYZ;
			return DecodeRGBEImage( LoadHDRFormat( _HDRFormatBinary, out bSourceIsXYZ, out _ColorProfile ), bSourceIsXYZ, _bTargetNeedsXYZ, _ColorProfile );
		}

		/// <summary>
		/// Loads a bitmap in .HDR format into a RGBE array
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bIsXYZ">Tells if the image is encoded as XYZE rather than RGBE</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static unsafe PF_RGBE[,]	LoadHDRFormat( byte[] _HDRFormatBinary, out bool _bIsXYZ, out ColorProfile _ColorProfile )
		{
			try
			{
				// The header of a .HDR image file consists of lines terminated by '\n'
				// It ends when there are 2 successive '\n' characters, then follows a single line containing the resolution of the image and only then, real scanlines begin...
				//

				// 1] We must isolate the header and find where it ends.
				//		To do this, we seek and replace every '\n' characters by '\0' (easier to read) until we find a double '\n'
				List<string>	HeaderLines = new List<string>();
				int				CharacterIndex = 0;
				int				LineStartCharacterIndex = 0;

				while ( true )
				{
					if ( _HDRFormatBinary[CharacterIndex] == '\n' || _HDRFormatBinary[CharacterIndex] == '\0' )
					{	// Found a new line!
						_HDRFormatBinary[CharacterIndex] = 0;
						fixed ( byte* pLineStart = &_HDRFormatBinary[LineStartCharacterIndex] )
							HeaderLines.Add( new string( (sbyte*) pLineStart, 0, CharacterIndex-LineStartCharacterIndex, System.Text.Encoding.ASCII ) );

						LineStartCharacterIndex = CharacterIndex + 1;

						// Check for header end
						if ( _HDRFormatBinary[CharacterIndex + 2] == '\n' )
						{
							CharacterIndex += 3;
							break;
						}
						if ( _HDRFormatBinary[CharacterIndex + 1] == '\n' )
						{
							CharacterIndex += 2;
							break;
						}
					}

					// Next character
					CharacterIndex++;
				}

				// 2] Read the last line containing the resolution of the image
				byte*	pScanlines = null;
				string	Resolution = null;
				LineStartCharacterIndex = CharacterIndex;
				while ( true )
				{
					if ( _HDRFormatBinary[CharacterIndex] == '\n' || _HDRFormatBinary[CharacterIndex] == '\0' )
					{
						_HDRFormatBinary[CharacterIndex] = 0;
						fixed ( byte* pLineStart = &_HDRFormatBinary[LineStartCharacterIndex] )
							Resolution = new string( (sbyte*) pLineStart, 0, CharacterIndex-LineStartCharacterIndex, System.Text.Encoding.ASCII );

						fixed ( byte* pScanlinesStart = &_HDRFormatBinary[CharacterIndex + 1] )
							pScanlines = pScanlinesStart;

						break;
					}

					// Next character
					CharacterIndex++;
				}

				// 3] Check format and retrieve resolution
					// 3.1] Search lines for "#?RADIANCE" or "#?RGBE"
				if ( RadianceFileFindInHeader( HeaderLines, "#?RADIANCE" ) == null && RadianceFileFindInHeader( HeaderLines, "#?RGBE" ) == null )
					throw new NotSupportedException( "Unknown HDR format!" );		// Unknown HDR file format!

					// 3.2] Search lines for format
				string	FileFormat = RadianceFileFindInHeader( HeaderLines, "FORMAT=" );
				if ( FileFormat == null )
					throw new Exception( "No format description!" );			// Couldn't get FORMAT

				_bIsXYZ = false;
				if ( FileFormat.IndexOf( "32-bit_rle_rgbe" ) == -1 )
				{	// Check for XYZ encoding
					_bIsXYZ = true;
					if ( FileFormat.IndexOf( "32-bit_rle_xyze" ) == -1 )
						throw new Exception( "Can't read format \"" + FileFormat + "\". Only 32-bit-rle-rgbe or 32-bit_rle_xyze is currently supported!" );
				}

					// 3.3] Search lines for the exposure
				float	fExposure = 0.0f;
				string	ExposureText = RadianceFileFindInHeader( HeaderLines, "EXPOSURE=" );
				if ( ExposureText != null )
					float.TryParse( ExposureText, out fExposure );

					// 3.4] Read the color primaries
				ColorProfile.Chromaticities	Chromas = ColorProfile.Chromaticities.Radiance;	// Default chromaticities
				string	PrimariesText = RadianceFileFindInHeader( HeaderLines, "PRIMARIES=" );
				if ( PrimariesText != null )
				{
					string[]	Primaries = PrimariesText.Split( ' ' );
					if ( Primaries == null || Primaries.Length != 8 )
						throw new Exception( "Failed to parse color profile chromaticities !" );

					float.TryParse( Primaries[0], out Chromas.R.X );
					float.TryParse( Primaries[1], out Chromas.R.Y );
					float.TryParse( Primaries[2], out Chromas.G.X );
					float.TryParse( Primaries[3], out Chromas.G.Y );
					float.TryParse( Primaries[4], out Chromas.B.X );
					float.TryParse( Primaries[5], out Chromas.B.Y );
					float.TryParse( Primaries[6], out Chromas.W.X );
					float.TryParse( Primaries[7], out Chromas.W.Y );
				}

					// 3.5] Create the color profile
				_ColorProfile = new ColorProfile( Chromas, ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );
				_ColorProfile.Exposure = fExposure;

					// 3.6] Read the resolution out of the last line
				int		WayX = +1, WayY = +1;
				int		Width = 0, Height = 0;

				int	XIndex = Resolution.IndexOf( "+X" );
				if ( XIndex == -1 )
				{	// Wrong way!
					WayX = -1;
					XIndex = Resolution.IndexOf( "-X" );
				}
				if ( XIndex == -1 )
					throw new Exception( "Couldn't find image width in resolution string \"" + Resolution + "\"!" );
				int	WidthEndCharacterIndex = Resolution.IndexOf( ' ', XIndex + 3 );
				if ( WidthEndCharacterIndex == -1 )
					WidthEndCharacterIndex = Resolution.Length;
				Width = int.Parse( Resolution.Substring( XIndex + 2, WidthEndCharacterIndex - XIndex - 2 ) );

				int	YIndex = Resolution.IndexOf( "+Y" );
				if ( YIndex == -1 )
				{	// Flipped !
					WayY = -1;
					YIndex = Resolution.IndexOf( "-Y" );
				}
				if ( YIndex == -1 )
					throw new Exception( "Couldn't find image height in resolution string \"" + Resolution + "\"!" );
				int	HeightEndCharacterIndex = Resolution.IndexOf( ' ', YIndex + 3 );
				if ( HeightEndCharacterIndex == -1 )
					HeightEndCharacterIndex = Resolution.Length;
				Height = int.Parse( Resolution.Substring( YIndex + 2, HeightEndCharacterIndex - YIndex - 2 ) );

				// The encoding of the image data is quite simple:
				//
				//	_ Each floating-point component is first encoded in Greg Ward's packed-pixel format which encodes 3 floats into a single DWORD organized this way: RrrrrrrrGgggggggBbbbbbbbEeeeeeee (E being the common exponent)
				//	_ Each component of the packed-pixel is then encoded separately using a simple run-length encoding format
				//

				// 1] Allocate memory for the image and the temporary p_HDRFormatBinaryScanline
				PF_RGBE[,]	Dest = new PF_RGBE[Width, Height];
				byte[,]		TempScanline = new byte[Width,4];

				// 2] Read the scanlines
				int	ImageY = WayY == +1 ? 0 : Height - 1;
				for ( int y=0; y < Height; y++, ImageY += WayY )
				{
					if ( Width < 8 || Width > 0x7FFF || pScanlines[0] != 0x02 )
						throw new Exception( "Unsupported old encoding format!" );

					byte	Temp;
					byte	Green, Blue;

					// 2.1] Read an entire scanline
					pScanlines++;
					Green = *pScanlines++;
					Blue = *pScanlines++;
					Temp = *pScanlines++;

					if ( Green != 2 || (Blue & 0x80) != 0 )
						throw new Exception( "Unsupported old encoding format!" );

					if ( ((Blue << 8) | Temp) != Width )
						throw new Exception( "Line and image widths mismatch!" );

					for ( int ComponentIndex=0; ComponentIndex < 4; ComponentIndex++ )
					{
						for ( int x=0; x < Width; )
						{
							byte	Code = *pScanlines++;
							if ( Code > 128 )
							{	// Run-Length encoding
								Code &= 0x7F;
								byte	RLValue = *pScanlines++;
								while ( Code-- > 0 && x < Width )
									TempScanline[x++,ComponentIndex] = RLValue;
							}
							else
							{	// Normal encoding
								while ( Code-- > 0 && x < Width )
									TempScanline[x++, ComponentIndex] = *pScanlines++;
							}
						}	// For every pixels of the scanline
					}	// For every color components (including exponent)

					// 2.2] Post-process the scanline and re-order it correctly
					int	ImageX = WayX == +1 ? 0 : Width - 1;
					for ( int x=0; x < Width; x++, ImageX += WayX )
					{
						Dest[x,y].R = TempScanline[ImageX, 0];
						Dest[x,y].G = TempScanline[ImageX, 1];
						Dest[x,y].B = TempScanline[ImageX, 2];
						Dest[x,y].E = TempScanline[ImageX, 3];
					}
				}

				return	Dest;
			}
			catch ( Exception _e )
			{	// Ouch!
				throw new Exception( "An exception occured while attempting to load an HDR file!", _e );
			}
		}

		/// <summary>
		/// Decodes a RGBE formatted image into a plain floating-point image
		/// </summary>
		/// <param name="_Source">The source RGBE formatted image</param>
		/// <param name="_bSourceIsXYZ">Tells if the source image is encoded as XYZE rather than RGBE</param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns>A HDR image as floats</returns>
		public static Vector4D[,]	DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, bool _bTargetNeedsXYZ, ColorProfile _ColorProfile )
		{
			if ( _Source == null )
				return	null;

			Vector4D[,]	Result = new Vector4D[_Source.GetLength( 0 ), _Source.GetLength( 1 )];
			DecodeRGBEImage( _Source, _bSourceIsXYZ, Result, _bTargetNeedsXYZ, _ColorProfile );

			return Result;
		}

		/// <summary>
		/// Decodes a RGBE formatted image into a plain floating-point image
		/// </summary>
		/// <param name="_Source">The source RGBE formatted image</param>
		/// <param name="_bSourceIsXYZ">Tells if the source image is encoded as XYZE rather than RGBE</param>
		/// <param name="_Target">The target Vector4D image</param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		public static void			DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, Vector4D[,] _Target, bool _bTargetNeedsXYZ, ColorProfile _ColorProfile )
		{
			if ( _bSourceIsXYZ ^ _bTargetNeedsXYZ )
			{	// Requires conversion...
				if ( _bSourceIsXYZ )
				{	// Convert from XYZ to RGB
					for ( int Y=0; Y < _Source.GetLength( 1 ); Y++ )
						for ( int X=0; X < _Source.GetLength( 0 ); X++ )
							_Target[X,Y] = _ColorProfile.XYZ2RGB( _Source[X,Y].DecodedColorAsVector );
				}
				else
				{	// Convert from RGB to XYZ
					for ( int Y=0; Y < _Source.GetLength( 1 ); Y++ )
						for ( int X=0; X < _Source.GetLength( 0 ); X++ )
							_Target[X,Y] = _ColorProfile.RGB2XYZ( _Source[X,Y].DecodedColorAsVector );
				}
				return;
			}

			// Simply decode vector and leave as-is
			for ( int Y=0; Y < _Source.GetLength( 1 ); Y++ )
				for ( int X=0; X < _Source.GetLength( 0 ); X++ )
					_Target[X,Y] = _Source[X,Y].DecodedColorAsVector;
		}

		protected static string		RadianceFileFindInHeader( List<string> _HeaderLines, string _Search )
		{
			foreach ( string Line in _HeaderLines )
				if ( Line.IndexOf( _Search ) != -1 )
					return Line.Replace( _Search, "" );	// Return line and remove Search criterium

			return null;
		}

		#endregion

		#endregion
	}
}
