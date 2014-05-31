using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using SharpDX;
//using SharpDX.Direct3D10;
//using SharpDX.DXGI;
//using SharpDX.WIC;
using System.Windows.Media.Imaging;

//using WMath;

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

	#region Small math helpers

	[System.Diagnostics.DebuggerDisplay( "x={x} y={y}" )]
	public struct	float2
	{
		public float	x, y;
		public float2( float _x, float _y )		{ x = _x; y = _y; }
		public static float2	operator+( float2 a, float2 b )		{ return new float2( a.x + b.x, a.y + b.y ); }
		public static float2	operator-( float2 a, float2 b )		{ return new float2( a.x - b.x, a.y - b.y ); }
		public static float2	operator*( float a, float2 b )		{ return new float2( a * b.x, a * b.y ); }
		public float			Dot( float2 a )						{ return x*a.x + y*a.y; }

		public override string ToString()
		{
			return x.ToString() + "; " + y.ToString();
		}
		public static float2	Parse( string v )
		{
			string[]	Components = v.Split( ';' );
			if ( Components.Length < 2 )
				throw new Exception( "Not enough vector components!" );
			float2		Result = new float2();
			if ( !float.TryParse( Components[0].Trim(), out Result.x ) )
				throw new Exception( "Can't parse X field!" );
			if ( !float.TryParse( Components[1].Trim(), out Result.y ) )
				throw new Exception( "Can't parse Y field!" );
			return Result;
		}
	}
	[System.Diagnostics.DebuggerDisplay( "x={x} y={y} z={z}" )]
	public struct	float3
	{
		public float	x, y, z;
		public float3( float _x, float _y, float _z )		{ x = _x; y = _y; z = _z; }
		public static float3	operator+( float3 a, float3 b )		{ return new float3( a.x + b.x, a.y + b.y, a.z + b.z ); }
		public static float3	operator-( float3 a, float3 b )		{ return new float3( a.x - b.x, a.y - b.y, a.z - b.z ); }
		public static float3	operator*( float a, float3 b )		{ return new float3( a * b.x, a * b.y, a * b.z ); }

		public override string ToString()
		{
			return x.ToString() + "; " + y.ToString() + "; " + z.ToString();
		}
		public static float3	Parse( string v )
		{
			string[]	Components = v.Split( ';' );
			if ( Components.Length < 3 )
				throw new Exception( "Not enough vector components!" );
			float3		Result = new float3();
			if ( !float.TryParse( Components[0].Trim(), out Result.x ) )
				throw new Exception( "Can't parse X field!" );
			if ( !float.TryParse( Components[1].Trim(), out Result.y ) )
				throw new Exception( "Can't parse Y field!" );
			if ( !float.TryParse( Components[2].Trim(), out Result.z ) )
				throw new Exception( "Can't parse Z field!" );
			return Result;
		}
	}
	[System.Diagnostics.DebuggerDisplay( "x={x} y={y} z={z} w={w}" )]
	public struct	float4
	{
		public float	x, y, z, w;

		public float	this[int _coeff]
		{
			get
			{
				switch ( _coeff )
				{
					case 0: return x;
					case 1: return y;
					case 2: return z;
					case 3: return w;
				}
				return float.NaN;
			}
			set
			{
				switch ( _coeff )
				{
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					case 3: w = value; break;
				}
			}
		}

		public float4( float _x, float _y, float _z, float _w )		{ x = _x; y = _y; z = _z; w = _w; }
		public float4( float3 _xyz, float _w )						{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = _w; }

		public static explicit	operator float3( float4 a )
		{
			return new float3( a.x, a.y, a.z );
		}

		public static float4	operator+( float4 a, float4 b )		{ return new float4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w ); }
		public static float4	operator*( float a, float4 b )		{ return new float4( a * b.x, a * b.y, a * b.z, a *b.w ); }
		public static float4	operator*( float4 a, float4x4 b )
		{
			return new float4(
				a.x * b.row0.x + a.y * b.row1.x + a.z * b.row2.x + a.w * b.row3.x,
				a.x * b.row0.y + a.y * b.row1.y + a.z * b.row2.y + a.w * b.row3.y,
				a.x * b.row0.z + a.y * b.row1.z + a.z * b.row2.z + a.w * b.row3.z,
				a.x * b.row0.w + a.y * b.row1.w + a.z * b.row2.w + a.w * b.row3.w
				);
		}
		public float			dot( float4 b ) { return x*b.x + y*b.y + z*b.z + w*b.w; }

		public override string ToString()
		{
			return x.ToString() + "; " + y.ToString() + "; " + z.ToString() + "; " + w.ToString();
		}
		public static float4	Parse( string v )
		{
			string[]	Components = v.Split( ';' );
			if ( Components.Length < 4 )
				throw new Exception( "Not enough vector components!" );
			float4		Result = new float4();
			if ( !float.TryParse( Components[0].Trim(), out Result.x ) )
				throw new Exception( "Can't parse X field!" );
			if ( !float.TryParse( Components[1].Trim(), out Result.y ) )
				throw new Exception( "Can't parse Y field!" );
			if ( !float.TryParse( Components[2].Trim(), out Result.z ) )
				throw new Exception( "Can't parse Z field!" );
			if ( !float.TryParse( Components[3].Trim(), out Result.w ) )
				throw new Exception( "Can't parse W field!" );
			return Result;
		}
	}
	public struct	float4x4
	{
		public float4	row0;
		public float4	row1;
		public float4	row2;
		public float4	row3;

		public float	this[int row, int column]
		{
			get
			{
				switch ( row )
				{
					case 0: return row0[column];
					case 1: return row1[column];
					case 2: return row2[column];
					case 3: return row3[column];
				}
				return float.NaN;
			}
			set
			{
				switch ( row )
				{
					case 0: row0[column] = value; break;
					case 1: row1[column] = value; break;
					case 2: row2[column] = value; break;
					case 3: row3[column] = value; break;
				}
			}
		}

		public float4x4( float[] _a )
		{
			row0 = new float4( _a[0], _a[1], _a[2], _a[3] );
			row1 = new float4( _a[4], _a[5], _a[6], _a[7] );
			row2 = new float4( _a[8], _a[9], _a[10], _a[11] );
			row3 = new float4( _a[12], _a[13], _a[14], _a[15] );
		}

		public float4	column0	{ get { return new float4( row0.x, row1.x, row2.x, row3.x ); } }
		public float4	column1	{ get { return new float4( row0.y, row1.y, row2.y, row3.y ); } }
		public float4	column2	{ get { return new float4( row0.z, row1.z, row2.z, row3.z ); } }
		public float4	column3	{ get { return new float4( row0.w, row1.w, row2.w, row3.w ); } }

		private static int[]		ms_Index	= { 0, 1, 2, 3, 0, 1, 2 };				// This array gives the index of the current component
		public float				CoFactor( int _dwRow, int _dwCol )
		{
			return	((	this[ms_Index[_dwRow+1], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+3], ms_Index[_dwCol+3]] +
						this[ms_Index[_dwRow+1], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+3], ms_Index[_dwCol+1]] +
						this[ms_Index[_dwRow+1], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+3], ms_Index[_dwCol+2]] )

					-(	this[ms_Index[_dwRow+3], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+1], ms_Index[_dwCol+3]] +
						this[ms_Index[_dwRow+3], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+1], ms_Index[_dwCol+1]] +
						this[ms_Index[_dwRow+3], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+1], ms_Index[_dwCol+2]] ))
					* (((_dwRow + _dwCol) & 1) == 1 ? -1.0f : +1.0f);
		}
		public float				Determinant()					{ return this[0, 0] * CoFactor( 0, 0 ) + this[0, 1] * CoFactor( 0, 1 ) + this[0, 2] * CoFactor( 0, 2 ) + this[0, 3] * CoFactor( 0, 3 ); }
		public void	Invert()
		{
			float	fDet = Determinant();
			if ( (float) System.Math.Abs(fDet) < float.Epsilon )
				throw new Exception( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

			float	fIDet = 1.0f / fDet;

			float4x4	Temp = new float4x4();
			Temp[0, 0] = CoFactor( 0, 0 ) * fIDet;
			Temp[1, 0] = CoFactor( 0, 1 ) * fIDet;
			Temp[2, 0] = CoFactor( 0, 2 ) * fIDet;
			Temp[3, 0] = CoFactor( 0, 3 ) * fIDet;
			Temp[0, 1] = CoFactor( 1, 0 ) * fIDet;
			Temp[1, 1] = CoFactor( 1, 1 ) * fIDet;
			Temp[2, 1] = CoFactor( 1, 2 ) * fIDet;
			Temp[3, 1] = CoFactor( 1, 3 ) * fIDet;
			Temp[0, 2] = CoFactor( 2, 0 ) * fIDet;
			Temp[1, 2] = CoFactor( 2, 1 ) * fIDet;
			Temp[2, 2] = CoFactor( 2, 2 ) * fIDet;
			Temp[3, 2] = CoFactor( 2, 3 ) * fIDet;
			Temp[0, 3] = CoFactor( 3, 0 ) * fIDet;
			Temp[1, 3] = CoFactor( 3, 1 ) * fIDet;
			Temp[2, 3] = CoFactor( 3, 2 ) * fIDet;
			Temp[3, 3] = CoFactor( 3, 3 ) * fIDet;

			row0 = Temp.row0;
			row1 = Temp.row1;
			row2 = Temp.row2;
			row3 = Temp.row3;
		}

		public static float4x4	Identity	{ get { return new float4x4( new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 } ); } }

		public static float4x4	operator*( float4x4 a, float4x4 b )
		{
			return new float4x4() {
				row0 = new float4( a.row0.dot( b.column0 ), a.row0.dot( b.column1 ), a.row0.dot( b.column2 ), a.row0.dot( b.column3 ) ),
				row1 = new float4( a.row1.dot( b.column0 ), a.row1.dot( b.column1 ), a.row1.dot( b.column2 ), a.row1.dot( b.column3 ) ),
				row2 = new float4( a.row2.dot( b.column0 ), a.row2.dot( b.column1 ), a.row2.dot( b.column2 ), a.row2.dot( b.column3 ) ),
				row3 = new float4( a.row3.dot( b.column0 ), a.row3.dot( b.column1 ), a.row3.dot( b.column2 ), a.row3.dot( b.column3 ) )
			};
		}
	}

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
	public class Bitmap2 : IDisposable
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
			CRW,
			CR2,
			DNG,

			UNKNOWN
		}

		/// <summary>
		/// Formatting flags for Save() method
		/// </summary>
		[Flags]
		public enum FORMAT_FLAGS
		{
			NONE = 0,

			// Bits per pixel component
			SAVE_8BITS_UNORM = 0,	// Save as byte
			SAVE_16BITS_UNORM = 1,	// Save as UInt16 if possible (valid for PNG, TIFF)
			SAVE_32BITS_FLOAT = 2,	// Save as float if possible (valid for TIFF)

			// Gray
			GRAY = 4,				// Save as gray levels

			SKIP_ALPHA = 8,			// Don't save alpha
			PREMULTIPLY_ALPHA = 16,	// RGB should be multiplied by alpha
		}

		/// <summary>
		/// A delegate used to process pixels (i.e. either generate a new pixel or alter the existing pixel)
		/// </summary>
		/// <param name="_X"></param>
		/// <param name="_Y"></param>
		/// <param name="_Color"></param>
		public delegate void	ImageProcessDelegate( int _X, int _Y, ref float4 _Color );

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
						m_Gamma = GAMMA_EXPONENT_sRGB;
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

					case FILE_TYPE.BMP:	// BMP Don't have metadata!
						m_GammaCurve = GAMMA_CURVE.STANDARD;
						m_Gamma = 1.0f;
						m_Chromaticities = Chromaticities.sRGB;	// Default for BMPs is standard sRGB with no gamma
						break;

					case FILE_TYPE.CRW:	// Raw files have no correction
					case FILE_TYPE.CR2:
					case FILE_TYPE.DNG:
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
				float	Y_y = _xyY.z / Math.Max( 1e-8f, _xyY.y );
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

		#endregion

		#region FIELDS

		protected FILE_TYPE			m_Type = FILE_TYPE.UNKNOWN;
		protected int				m_Width = 0;
		protected int				m_Height = 0;
		protected bool				m_bHasAlpha = false;

		protected ColorProfile		m_ColorProfile = null;
		protected float4[,]			m_Bitmap = null;		// CIEXYZ Bitmap content + Alpha

		protected bool				m_bHasValidShotInfo;	// True if available
		protected float				m_ISOSpeed = -1.0f;
		protected float				m_ShutterSpeed = -1.0f;
		protected float				m_Aperture = -1.0f;
		protected float				m_FocalLength = -1.0f;

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
		public bool			HasAlpha				{ get { return m_bHasAlpha; } set { m_bHasAlpha = value; } }

		/// <summary>
		/// Gets the image content stored as CIEXYZ + Alpha
		/// </summary>
		/// <remarks>You cannot use XYZ content directly : you must use the ColorProfile to perform color transformations</remarks>
		public float4[,]	ContentXYZ				{ get { return m_Bitmap; } }

		/// <summary>
		/// Gets the image's color profile
		/// </summary>
		public ColorProfile	Profile					{ get { return m_ColorProfile; } set { m_ColorProfile = value; } }

		/// <summary>
		/// Tells if the image contains valid shot info (i.e. ISO, Tv, Av, focal length, etc.)
		/// </summary>
		public bool			HasValidShotInfo		{ get { return m_bHasValidShotInfo; } set { m_bHasValidShotInfo = value; } }

		/// <summary>
		/// Gets or sets the ISO speed associated to the image
		/// </summary>
		public float		ISOSpeed				{ get { return m_ISOSpeed; } set { m_ISOSpeed = value; } }

		/// <summary>
		/// Gets or sets the shutter speed associated to the image
		/// </summary>
		public float		ShutterSpeed			{ get { return m_ShutterSpeed; } set { m_ShutterSpeed = value; } }

		/// <summary>
		/// Gets or sets the aperture associated to the image
		/// </summary>
		public float		Aperture				{ get { return m_Aperture; } set { m_Aperture = value; } }

		/// <summary>
		/// Gets or sets the focal length associated to the image
		/// </summary>
		public float		FocalLength				{ get { return m_FocalLength; } set { m_FocalLength = value; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Manual creation
		/// </summary>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		public Bitmap2( int _Width, int _Height )
		{
			m_Width = _Width;
			m_Height = _Height;
			m_Bitmap = new float4[m_Width,m_Height];
			for ( int Y=0; Y < m_Height; Y++ )
				for ( int X=0; X < m_Width; X++ )
					m_Bitmap[X,Y] = new float4( 0, 0, 0, 0 );
		}

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
		/// <param name="_ColorProfile">The color profile to use to transform the bitmap</param>
		public	Bitmap2( System.Drawing.Bitmap _Bitmap, ColorProfile _ColorProfile )
		{
			if ( _ColorProfile == null )
				throw new Exception( "Invalid profile: can't convert to CIE XYZ !" );
			m_ColorProfile = _ColorProfile;

			// Load the bitmap's content and copy it to a double entry array
			byte[]	BitmapContent = LoadBitmap( _Bitmap, out m_Width, out m_Height );

			m_Bitmap = new float4[m_Width,m_Height];

			int	i=0;
			for ( int Y=0; Y < m_Height; Y++ )
				for ( int X=0; X < m_Width; X++ )
				{
					m_Bitmap[X,Y] = new float4(
							BYTE_TO_FLOAT * BitmapContent[i++],	// R
							BYTE_TO_FLOAT * BitmapContent[i++],	// G
							BYTE_TO_FLOAT * BitmapContent[i++],	// B
							BYTE_TO_FLOAT * BitmapContent[i++]	// A
						);
				}

			// Convert to CIE XYZ
			m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
		}

		/// <summary>
		/// Performs bilinear sampling of the XYZ content
		/// </summary>
		/// <param name="X">A column index in [0,Width[ (will be clamped if out of range)</param>
		/// <param name="Y">A row index in [0,Height[ (will be clamped if out of range)</param>
		/// <returns>The XYZ at the requested location</returns>
		public float4	BilinearSample( float X, float Y )
		{
			int		X0 = (int) Math.Floor( X );
			int		Y0 = (int) Math.Floor( Y );
			float	x = X - X0;
			float	y = Y - Y0;
			float	rx = 1.0f - x;
			float	ry = 1.0f - y;
			X0 = Math.Max( 0, Math.Min( Width-1, X0 ) );
			Y0 = Math.Max( 0, Math.Min( Height-1, Y0 ) );
			int		X1 = Math.Min( Width-1, X0+1 );
			int		Y1 = Math.Min( Height-1, Y0+1 );

			float4	V00 = m_Bitmap[X0,Y0];
			float4	V01 = m_Bitmap[X1,Y0];
			float4	V10 = m_Bitmap[X0,Y1];
			float4	V11 = m_Bitmap[X1,Y1];

			float4	V0 = rx * V00 + x * V01;
			float4	V1 = rx * V10 + x * V11;

			float4	V = ry * V0 + y * V1;
			return V;
		}

		/// <summary>
		/// Loads from disk
		/// </summary>
		/// <param name="_ImageFileName"></param>
		/// <param name="_FileType"></param>
		public void	Load( System.IO.FileInfo _ImageFileName, FILE_TYPE _FileType )
		{
			using ( System.IO.FileStream ImageStream = _ImageFileName.Open( System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read ) )
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
							BitmapDecoder	Decoder = BitmapDecoder.Create( Stream, BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand );
							if ( Decoder.Frames.Count == 0 )
								throw new Exception( "BitmapDecoder failed to read at least one bitmap frame !" );

							BitmapFrame	Frame = Decoder.Frames[0];
							if ( Frame == null )
								throw new Exception( "Invalid decoded bitmap !" );

// DEBUG
// int		StrideX = (Frame.Format.BitsPerPixel>>3)*Frame.PixelWidth;
// byte[]	DebugImageSource = new byte[StrideX*Frame.PixelHeight];
// Frame.CopyPixels( DebugImageSource, StrideX, 0 );
// DEBUG

// pas de gamma sur les JPEG si non spécifié !
// Il y a bien une magouille faite lors de la conversion par le FormatConvertedBitmap!


							// ===== 2] Build the color profile =====
							m_ColorProfile = new ColorProfile( Frame.Metadata as BitmapMetadata, _FileType );

							// ===== 3] Convert the frame to generic RGBA32F =====
							ConvertFrame( Frame );

							// ===== 4] Convert to CIE XYZ (our device-independent profile connection space) =====
							m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
						}
						break;

					case FILE_TYPE.TGA:
						{
							// Load as a System.Drawing.Bitmap and convert to float4
							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
								using ( TargaImage TGA = new TargaImage( Stream ) )
								{
									// Create a default sRGB linear color profile
									m_ColorProfile = new ColorProfile(
											ColorProfile.Chromaticities.sRGB,	// Use default sRGB color profile
											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
											TGA.ExtensionArea.GammaRatio		// ...whose gamma is retrieved from extension data
										);

									// Convert
									byte[]	ImageContent = LoadBitmap( TGA.Image, out m_Width, out m_Height );
									m_Bitmap = new float4[m_Width,m_Height];
									byte	A;
									int		i = 0;
									for ( int Y=0; Y < m_Height; Y++ )
										for ( int X=0; X < m_Width; X++ )
										{
											m_Bitmap[X,Y].x = BYTE_TO_FLOAT * ImageContent[i++];
											m_Bitmap[X,Y].y = BYTE_TO_FLOAT * ImageContent[i++];
											m_Bitmap[X,Y].z = BYTE_TO_FLOAT * ImageContent[i++];

											A = ImageContent[i++];
											m_bHasAlpha |= A != 0xFF;

											m_Bitmap[X,Y].w = BYTE_TO_FLOAT * A;
										}

									// Convert to CIEXYZ
									m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
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

					case FILE_TYPE.CRW:
					case FILE_TYPE.CR2:
					case FILE_TYPE.DNG:
						{
							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
								using ( LibRawManaged.RawFile Raw = new LibRawManaged.RawFile() )
								{
									Raw.UnpackRAW( Stream );

									ColorProfile.Chromaticities	Chroma = Raw.ColorProfile == LibRawManaged.RawFile.COLOR_PROFILE.ADOBE_RGB
																		? ColorProfile.Chromaticities.AdobeRGB_D65	// Use Adobe RGB
																		: ColorProfile.Chromaticities.sRGB;			// Use default sRGB color profile

									// Create a default sRGB linear color profile
									m_ColorProfile = new ColorProfile(
											Chroma,
											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
											1.0f								// Linear
										);

									// Also get back valid camera shot info
									m_bHasValidShotInfo = true;
									m_ISOSpeed = Raw.ISOSpeed;
									m_ShutterSpeed = Raw.ShutterSpeed;
									m_Aperture = Raw.Aperture;
									m_FocalLength = Raw.FocalLength;

 									// Convert
									m_Width = Raw.Width;
									m_Height = Raw.Height;
//									float	ColorNormalizer = 1.0f / Raw.Maximum;
									float	ColorNormalizer = 1.0f / 65535.0f;

									m_Bitmap = new float4[m_Width,m_Height];
									UInt16[,][]	ImageContent = Raw.Image;
									for ( int Y=0; Y < m_Height; Y++ )
										for ( int X=0; X < m_Width; X++ )
										{
 											m_Bitmap[X,Y].x = ImageContent[X,Y][0] * ColorNormalizer;
 											m_Bitmap[X,Y].y = ImageContent[X,Y][1] * ColorNormalizer;
 											m_Bitmap[X,Y].z = ImageContent[X,Y][2] * ColorNormalizer;
 											m_Bitmap[X,Y].w = ImageContent[X,Y][3] * ColorNormalizer;
 										}

									// Convert to CIEXYZ
									m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
								}

#region My poor attempt at reading CRW files
// 							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
// 								using ( CanonRawLoader CRWLoader = new CanonRawLoader( Stream ) )
// 								{
// 									ColorProfile.Chromaticities	Chroma = CRWLoader.m_ColorProfile == CanonRawLoader.DataColorProfile.COLOR_PROFILE.ADOBE_RGB
// 																		? ColorProfile.Chromaticities.AdobeRGB_D65	// Use Adobe RGB
// 																		: ColorProfile.Chromaticities.sRGB;			// Use default sRGB color profile
// 
// 									// Create a default sRGB linear color profile
// 									m_ColorProfile = new ColorProfile(
// 											Chroma,
// 											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
// 											1.0f								// Linear
// 										);
// 
//  									// Convert
// 									m_Width = CRWLoader.m_RAWImage.m_Width;
// 									m_Height = CRWLoader.m_RAWImage.m_Height;
// 
// 									m_Bitmap = new float4[m_Width,m_Height];
// 									UInt16[]	ImageContent = CRWLoader.m_RAWImage.m_DecodedImage;
// 									int			i = 0;
// // 									for ( int Y=0; Y < m_Height; Y++ )
// // 										for ( int X=0; X < m_Width; X++ )
// // 										{
// //  											m_Bitmap[X,Y].x = ImageContent[i++] / 4096.0f;
// //  											m_Bitmap[X,Y].y = ImageContent[i++] / 4096.0f;
// //  											m_Bitmap[X,Y].z = ImageContent[i++] / 4096.0f;
// // 											i++;
// //  										}
// 
// 									i=0;
// 									for ( int Y=0; Y < m_Height; Y++ )
// 										for ( int X=0; X < m_Width; X++ )
//  											m_Bitmap[X,Y].x = ImageContent[i++] / 4096.0f;
// 									i=0;
// 									for ( int Y=0; Y < m_Height; Y++ )
// 										for ( int X=0; X < m_Width; X++ )
//  											m_Bitmap[X,Y].y = ImageContent[i++] / 4096.0f;
// 									i=0;
// 									for ( int Y=0; Y < m_Height; Y++ )
// 										for ( int X=0; X < m_Width; X++ )
//  											m_Bitmap[X,Y].z = ImageContent[i++] / 4096.0f;
// 
// 									// Convert to CIEXYZ
// 									m_ColorProfile.RGB2XYZ( m_Bitmap );
// 								}
#endregion
							return;
 						}

					default:
						throw new NotSupportedException( "The image file type \"" + _FileType + "\" is not supported by the Bitmap class!" );
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
		/// For example, if the image is using the Bgr24 format that uses a 1/2.2 gamma internally, converting that to our generic format Rgba128Float
		/// (that uses a gamma of 1 internally) will automatically apply a pow( 2.2 ) to the RGB values, which is NOT what we're looking for since we're
		/// handling gamma correction ourselves here !
		/// </remarks>
		protected void	ConvertFrame( BitmapSource _Frame )
		{
			m_Width = _Frame.PixelWidth;
			m_Height = _Frame.PixelHeight;
			m_Bitmap = new float4[m_Width,m_Height];

			int		W = m_Width;
			int		H = m_Height;

			float4	V = new float4();

			//////////////////////////////////////////////////////////////////////////
			// BGR24
			if ( _Frame.Format == System.Windows.Media.PixelFormats.Bgr24 )
			{	
				int		Stride = 3*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				m_bHasAlpha = false;
				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						V.z = BYTE_TO_FLOAT * Content[Position++];
						V.y = BYTE_TO_FLOAT * Content[Position++];
						V.x = BYTE_TO_FLOAT * Content[Position++];
						V.w = 1.0f;
						m_Bitmap[X,Y] = V;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGR32
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Bgr32 )
			{	
				int		Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				m_bHasAlpha = false;
				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						V.z = BYTE_TO_FLOAT * Content[Position++];
						V.y = BYTE_TO_FLOAT * Content[Position++];
						V.x = BYTE_TO_FLOAT * Content[Position++];
						V.w = 1.0f;
						Position++;
						m_Bitmap[X,Y] = V;
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
						V.z = BYTE_TO_FLOAT * Content[Position++];
						V.y = BYTE_TO_FLOAT * Content[Position++];
						V.x = BYTE_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						V.w = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFF;

						m_Bitmap[X,Y] = V;
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
						V.z = BYTE_TO_FLOAT * Content[Position++];
						V.y = BYTE_TO_FLOAT * Content[Position++];
						V.x = BYTE_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						V.w = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFF;

						// Un-premultiply
						InvA = A != 0 ? 1.0f / V.w : 1.0f;
						V.x *= InvA;
						V.y *= InvA;
						V.z *= InvA;

						m_Bitmap[X,Y] = V;
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
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						V.x = WORD_TO_FLOAT * Content[Position++];
						V.y = WORD_TO_FLOAT * Content[Position++];
						V.z = WORD_TO_FLOAT * Content[Position++];
						V.w = 1.0f;
						m_Bitmap[X,Y] = V;
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
						V.x = WORD_TO_FLOAT * Content[Position++];
						V.y = WORD_TO_FLOAT * Content[Position++];
						V.z = WORD_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						V.w = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFFFF;

						m_Bitmap[X,Y] = V;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA64 (Pre-Multiplied)
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Prgba64 )
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
						V.x = WORD_TO_FLOAT * Content[Position++];
						V.y = WORD_TO_FLOAT * Content[Position++];
						V.z = WORD_TO_FLOAT * Content[Position++];

						A = Content[Position++];
						V.w = BYTE_TO_FLOAT * A;
						m_bHasAlpha |= A != 0xFFFF;

						// Un-premultiply
						InvA = A != 0 ? 1.0f / V.w : 1.0f;
						V.x *= InvA;
						V.y *= InvA;
						V.z *= InvA;

						m_Bitmap[X,Y] = V;
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
						V.x = Content[Position++];
						V.y = Content[Position++];
						V.z = Content[Position++];
						V.w = Content[Position++];

						m_bHasAlpha |= V.w != 1.0f;

						m_Bitmap[X,Y] = V;
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
						V.x = Content[Position++];
						V.y = Content[Position++];
						V.z = Content[Position++];
						V.w = Content[Position++];

						m_bHasAlpha |= V.w != 1.0f;

						// Un-premultiply
						InvA = V.w != 0.0f ? 1.0f / V.w : 1.0f;
						V.x *= InvA;
						V.y *= InvA;
						V.z *= InvA;

						m_Bitmap[X,Y] = V;
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
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						V.x = V.y = V.z = WORD_TO_FLOAT * Content[Position++];
						V.w = 1.0f;
						m_Bitmap[X,Y] = V;
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
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						V.x = V.y = V.z = Content[Position++];
						V.w = 1.0f;
						m_Bitmap[X,Y] = V;
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
				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						V.x = V.y = V.z = BYTE_TO_FLOAT * Content[Position++];
						V.w = 1.0f;
						m_Bitmap[X,Y] = V;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// 256 Colors Palette
			else if ( _Frame.Format == System.Windows.Media.PixelFormats.Indexed8 )
			{	
				int		Stride = 1*W;
				byte[]	Content = new byte[Stride*H];
				_Frame.CopyPixels( Content, Stride, 0 );

				float4[]	Palette = new float4[_Frame.Palette.Colors.Count];
				for ( int i=0; i < Palette.Length; i++ )
				{
					System.Windows.Media.Color	C = _Frame.Palette.Colors[i];
					Palette[i] = new float4(
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

		/// <summary>
		/// Save to a strea
		/// </summary>
		/// <param name="_Stream">The stream to write the image to</param>
		/// <param name="_FileType">The file type to save as</param>
		/// <param name="_Parms">Additional formatting flags</param>
		/// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
		/// <exception cref="Exception">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
		public void	Save( System.IO.Stream _Stream, FILE_TYPE _FileType, FORMAT_FLAGS _Parms )
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
						{
							BitmapEncoder	Encoder = null;
							switch ( _FileType )
							{
								case FILE_TYPE.JPEG:	Encoder = new JpegBitmapEncoder(); break;
								case FILE_TYPE.PNG:		Encoder = new PngBitmapEncoder(); break;
								case FILE_TYPE.TIFF:	Encoder = new TiffBitmapEncoder(); break;
								case FILE_TYPE.GIF:		Encoder = new GifBitmapEncoder(); break;
								case FILE_TYPE.BMP:		Encoder = new BmpBitmapEncoder(); break;
							}

							// Find the appropriate pixel format
							int		BitsPerComponent = 8;
							bool	IsFloat = false;
							if ( (_Parms & FORMAT_FLAGS.SAVE_16BITS_UNORM) != 0 )
								BitsPerComponent = 16;
							if ( (_Parms & FORMAT_FLAGS.SAVE_32BITS_FLOAT) != 0 )
							{	// Floating-point format
								BitsPerComponent = 32;
								IsFloat = true;
							}

							int		ComponentsCount = (_Parms & FORMAT_FLAGS.GRAY) == 0 ? 3 : 1;
							if ( m_bHasAlpha && (_Parms & FORMAT_FLAGS.SKIP_ALPHA) == 0 )
								ComponentsCount++;

							bool	PreMultiplyAlpha = (_Parms & FORMAT_FLAGS.PREMULTIPLY_ALPHA) != 0;

							System.Windows.Media.PixelFormat	Format;
							if ( ComponentsCount == 1 )
							{	// Gray
								switch ( BitsPerComponent )
								{
									case 8:		Format = System.Windows.Media.PixelFormats.Gray8; break;
									case 16:	Format = System.Windows.Media.PixelFormats.Gray16; break;
									case 32:	Format = System.Windows.Media.PixelFormats.Gray32Float; break;
									default:	throw new Exception( "Unsupported format!" );
								}
							}
							else if ( ComponentsCount == 3 )
							{	// RGB
								switch ( BitsPerComponent )
								{
									case 8:		Format = System.Windows.Media.PixelFormats.Rgb24; break;
									case 16:	Format = System.Windows.Media.PixelFormats.Rgb48; break;
									case 32:	throw new Exception( "32BITS formats aren't supported without ALPHA!" );
									default:	throw new Exception( "Unsupported format!" );
								}
							}
							else
							{	// RGBA
								switch ( BitsPerComponent )
								{
									case 8:		Format = PreMultiplyAlpha ? System.Windows.Media.PixelFormats.Pbgra32 : System.Windows.Media.PixelFormats.Bgra32; break;
									case 16:	Format = PreMultiplyAlpha ? System.Windows.Media.PixelFormats.Prgba64 : System.Windows.Media.PixelFormats.Rgba64; break;
									case 32:	Format = PreMultiplyAlpha ? System.Windows.Media.PixelFormats.Prgba128Float : System.Windows.Media.PixelFormats.Rgba128Float;
										if ( !IsFloat ) throw new Exception( "32BITS_UNORM format isn't supported if not floating-point!" );
										break;
									default:	throw new Exception( "Unsupported format!" );
								}
							}

							// Convert into appropriate frame
							BitmapFrame	Frame = ConvertFrame( Format );
							Encoder.Frames.Add( Frame );

							// Save
							Encoder.Save( _Stream );
						}
						break;

					case FILE_TYPE.TGA:
//TODO!
// 						{
// 							// Load as a System.Drawing.Bitmap and convert to float4
// 							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
// 								using ( TargaImage TGA = new TargaImage( Stream ) )
// 								{
// 									// Create a default sRGB linear color profile
// 									m_ColorProfile = new ColorProfile(
// 											ColorProfile.Chromaticities.sRGB,	// Use default sRGB color profile
// 											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
// 											TGA.ExtensionArea.GammaRatio		// ...whose gamma is retrieved from extension data
// 										);
// 
// 									// Convert
// 									byte[]	ImageContent = LoadBitmap( TGA.Image, out m_Width, out m_Height );
// 									m_Bitmap = new float4[m_Width,m_Height];
// 									byte	A;
// 									int		i = 0;
// 									for ( int Y=0; Y < m_Height; Y++ )
// 										for ( int X=0; X < m_Width; X++ )
// 										{
// 											m_Bitmap[X,Y].x = BYTE_TO_FLOAT * ImageContent[i++];
// 											m_Bitmap[X,Y].y = BYTE_TO_FLOAT * ImageContent[i++];
// 											m_Bitmap[X,Y].z = BYTE_TO_FLOAT * ImageContent[i++];
// 
// 											A = ImageContent[i++];
// 											m_bHasAlpha |= A != 0xFF;
// 
// 											m_Bitmap[X,Y].w = BYTE_TO_FLOAT * A;
// 										}
// 
// 									// Convert to CIEXYZ
// 									m_ColorProfile.RGB2XYZ( m_Bitmap );
// 								}
// 							return;
// 						}

					case FILE_TYPE.HDR:
//TODO!
// 						{
// 							// Load as XYZ
// 							m_Bitmap = LoadAndDecodeHDRFormat( _ImageFileContent, true, out m_ColorProfile );
// 							m_Width = m_Bitmap.GetLength( 0 );
// 							m_Height = m_Bitmap.GetLength( 1 );
// 							return;
// 						}

					case FILE_TYPE.CRW:
					case FILE_TYPE.CR2:
					case FILE_TYPE.DNG:
					default:
						throw new NotSupportedException( "The image file type \"" + _FileType + "\" is not supported by the Bitmap class!" );
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
		/// Converts the generic XYZ+A bitmap to the specified format frame
		/// </summary>
		/// <param name="_Format">The format to convert into</param>
		protected BitmapFrame	ConvertFrame( System.Windows.Media.PixelFormat _Format )
		{
			// Convert to RGB first
			float4[,]	RGB = new float4[m_Width,m_Height];
			m_ColorProfile.XYZ2RGB( m_Bitmap, RGB );

			Array	Pixels = null;
			int		Stride = 0;

			int		W = m_Width;
			int		H = m_Height;

			//////////////////////////////////////////////////////////////////////////
			// BGR24
			if ( _Format == System.Windows.Media.PixelFormats.Bgr24 )
			{	
				Stride = 3*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGR32
			else if ( _Format == System.Windows.Media.PixelFormats.Bgr32 )
			{	
				Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Position++;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGRA32
			else if ( _Format == System.Windows.Media.PixelFormats.Bgra32 )
			{	
				Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PBGRA32 (Pre-Multiplied)
			else if ( _Format == System.Windows.Media.PixelFormats.Pbgra32 )
			{	
				Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						RGB[X,Y].x *= RGB[X,Y].w;
						RGB[X,Y].y *= RGB[X,Y].w;
						RGB[X,Y].z *= RGB[X,Y].w;
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].w );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGB48
			else if ( _Format == System.Windows.Media.PixelFormats.Rgb48 )
			{	
				Stride = 6*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].z );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGBA64
			else if ( _Format == System.Windows.Media.PixelFormats.Rgba64 )
			{	
				Stride = 8*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].w );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA64 (Pre-Multiplied)
			else if ( _Format == System.Windows.Media.PixelFormats.Prgba64 )
			{	
				Stride = 8*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						RGB[X,Y].x *= RGB[X,Y].w;
						RGB[X,Y].y *= RGB[X,Y].w;
						RGB[X,Y].z *= RGB[X,Y].w;
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].w );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGBA128F
			else if ( _Format == System.Windows.Media.PixelFormats.Rgba128Float )
			{	
				Stride = 16*W;
				float[]	Content = new float[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = RGB[X,Y].x;
						Content[Position++] = RGB[X,Y].y;
						Content[Position++] = RGB[X,Y].z;
						Content[Position++] = RGB[X,Y].w;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA128F (Pre-Multiplied)
			else if ( _Format == System.Windows.Media.PixelFormats.Prgba128Float )
			{	
				Stride = 16*W;
				float[]	Content = new float[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						RGB[X,Y].x *= RGB[X,Y].w;
						RGB[X,Y].y *= RGB[X,Y].w;
						RGB[X,Y].z *= RGB[X,Y].w;
						Content[Position++] = RGB[X,Y].x;
						Content[Position++] = RGB[X,Y].y;
						Content[Position++] = RGB[X,Y].z;
						Content[Position++] = RGB[X,Y].w;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray16
			else if ( _Format == System.Windows.Media.PixelFormats.Gray16 )
			{	
				Stride = 2*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_WORD( m_Bitmap[X,Y].y );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray32F
			else if ( _Format == System.Windows.Media.PixelFormats.Gray32Float )
			{	
				Stride = 4*W;
				float[]	Content = new float[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = m_Bitmap[X,Y].y;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray8
			else if ( _Format == System.Windows.Media.PixelFormats.Gray8 )
			{	
				Stride = 1*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ )
					{
						Content[Position++] = FLOAT_TO_BYTE( m_Bitmap[X,Y].y );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// 256 Colors Palette
			else if ( _Format == System.Windows.Media.PixelFormats.Indexed8 )
			{
				throw new Exception( "Palette format are not supported!" );
			}
			else
				throw new Exception( "Source format " + _Format + " not supported !" );

			// Create the bitmap source & only frame
			BitmapSource	Source = BitmapSource.Create( m_Width, m_Height, 100, 100, _Format, null, Pixels, Stride );
			BitmapFrame		Frame = BitmapFrame.Create( Source );
			return Frame;
		}

		protected byte		FLOAT_TO_BYTE( float v )	{ return (byte) Math.Max( 0, Math.Min( 255, 255.0f * v ) ); }
		protected UInt16	FLOAT_TO_WORD( float v )	{ return (UInt16) Math.Max( 0, Math.Min( 65535, 65535.0f * v ) ); }

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

				case	".CRW":
					return FILE_TYPE.CRW;
				case	".CR2":
					return FILE_TYPE.CR2;
				case	".DNG":
					return FILE_TYPE.DNG;
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
		public	Image( Device _Device, string _Name, float4[,] _Image, float _Exposure, int _MipLevelsCount, ImageProcessDelegate _PreProcess ) : base( _Device, _Name )
		{
			m_Width = _Image.GetLength( 0 );
			m_Height = _Image.GetLength( 1 );
			m_MipLevelsCount = ComputeMipLevelsCount( _MipLevelsCount );
			m_DataStreams = new DataStream[m_MipLevelsCount];
			m_DataRectangles = new DataRectangle[m_MipLevelsCount];

			Load( _Image, _Exposure, _PreProcess );
		}
		public	Image( Device _Device, string _Name, float4[,] _Image, float _Exposure, int _MipLevelsCount ) : this( _Device, _Name, _Image, _Exposure, _MipLevelsCount, null ) {}

		/// <summary>
		/// Creates a custom image using a pixel writer
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_MipLevelsCount">Amount of mip levels to generate (0 is for entire mip maps pyramid)</param>
		public	Image( Device _Device, string _Name, int _Width, int _Height, ImageProcessDelegate _PixelWriter, int _MipLevelsCount ) : base( _Device, _Name )
		{
			if ( _PixelWriter == null )
				throw new Exception( "Invalid pixel writer !" );

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
				float4	C = new float4();
				for ( int Y=0; Y < m_Height; Y++ )
				{
					for ( int X=0; X < m_Width; X++ )
					{
						_PixelWriter( X, Y, ref C );
						Scanline[X].Write( C );

						m_bHasAlpha |= C.w != 1.0f;	// Check if it has alpha...
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
				throw new Exception( "An error occurred while creating the custom image !", _e );
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
				throw new Exception( "Provided image width mismatch !" );
			if ( _Image.Height != m_Height )
				throw new Exception( "Provided image height mismatch !" );

			int		PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

#if DEBUG
			// Ensure we're passing a unit image gamma
			bool	bUsesSRGB = new PF().sRGB;
			if ( bUsesSRGB && Math.Abs( _ImageGamma - 1.0f ) > 1e-3f )
				throw new Exception( "You specified a sRGB pixel format but provided an image gamma different from 1 !" );
#endif

			byte[]	ImageContent = LoadBitmap( _Image, out m_Width, out m_Height );

			// Create the data rectangle
			try
			{
				PF[]	Scanline = new PF[m_Width];
				m_DataStreams[0] = ToDispose( new DataStream( m_Width * m_Height * PixelSize, true, true ) );

				// Convert all scanlines into the desired format and write them into the stream
				float4	Temp = new float4();
				int		Offset;
				for ( int Y=0; Y < m_Height; Y++ )
				{
					Offset = (m_Width * (_MirrorY ? m_Height-1-Y : Y)) << 2;
					for ( int X=0; X < m_Width; X++ )
					{
						Temp.x = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
						Temp.y = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
						Temp.z = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
						Temp.w = BYTE_TO_FLOAT * ImageContent[Offset++];

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
				throw new Exception( "An error occurred while loading the image !", _e );
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
				throw new Exception( "Provided image width mismatch !" );
			if ( _Image.Height != m_Height )
				throw new Exception( "Provided image height mismatch !" );
			if ( _Alpha.Width != m_Width )
				throw new Exception( "Provided alpha width mismatch !" );
			if ( _Alpha.Height != m_Height )
				throw new Exception( "Provided alpha height mismatch !" );

			int		PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

#if DEBUG
			// Ensure we're passing a unit image gamma
			bool	bUsesSRGB = new PF().sRGB;
			if ( bUsesSRGB && Math.Abs( _ImageGamma - 1.0f ) > 1e-3f )
				throw new Exception( "You specified a sRGB pixel format but provided an image gamma different from 1 !" );
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
				float4	Temp = new float4();

				for ( int Y=0; Y < m_Height; Y++ )
				{
					Offset = (m_Width * (_MirrorY ? m_Height-1-Y : Y)) << 2;
					for ( int X=0; X < m_Width; X++, Offset+=4 )
					{
						Temp.x = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset+0], _ImageGamma );
						Temp.y = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset+1], _ImageGamma );
						Temp.z = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset+2], _ImageGamma );
						Temp.w = BYTE_TO_FLOAT * AlphaContent[Offset+0];

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
				throw new Exception( "An error occurred while loading the image !", _e );
			}
		}

		/// <summary>
		/// Loads a HDR image from memory
		/// </summary>
		/// <param name="_Image">Source image to load</param>
		/// <param name="_Exposure">The exposure correction to apply (default should be 0)</param>
		public void	Load( float4[,] _Image, float _Exposure, ImageProcessDelegate _PreProcess )
		{
			int	Width = _Image.GetLength( 0 );
			if ( Width != m_Width )
				throw new Exception( "Provided image width mismatch !" );

			int	Height = _Image.GetLength( 1 );
			if ( Height != m_Height )
				throw new Exception( "Provided image height mismatch !" );

			int	PixelSize = System.Runtime.InteropServices.Marshal.SizeOf( typeof(PF) );

			// Create the data rectangle
			PF[]	Scanline = new PF[m_Width];
			try
			{
				m_DataStreams[0] = ToDispose( new DataStream( m_Width * m_Height * PixelSize, true, true ) );

				// Convert all scanlines into the desired format and write them into the stream
				float4	Temp = new float4();
				for ( int Y=0; Y < m_Height; Y++ )
				{
					for ( int X=0; X < m_Width; X++ )
					{
						float	fLuminance = 0.3f * _Image[X,Y].x + 0.5f * _Image[X,Y].y + 0.2f * _Image[X,Y].z;
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
				throw new Exception( "An error occurred while loading the image !", _e );
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
					( int _X, int _Y, ref float4 _Color ) =>
						{
							Offset = ((Rect.y + _Y) + (Rect.x + _X)) << 2;

							_Color.x = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
							_Color.y = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
							_Color.z = GammaUnCorrect( BYTE_TO_FLOAT * ImageContent[Offset++], _ImageGamma );
							_Color.w = BYTE_TO_FLOAT * ImageContent[Offset++];

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
*/

		#region HDR Loaders

		/// <summary>
		/// Loads a bitmap in .HDR format into a float4 array directly useable by the image constructor
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static float4[,]	LoadAndDecodeHDRFormat( byte[] _HDRFormatBinary, bool _bTargetNeedsXYZ, out ColorProfile _ColorProfile )
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

					float.TryParse( Primaries[0], out Chromas.R.x );
					float.TryParse( Primaries[1], out Chromas.R.y );
					float.TryParse( Primaries[2], out Chromas.G.x );
					float.TryParse( Primaries[3], out Chromas.G.y );
					float.TryParse( Primaries[4], out Chromas.B.x );
					float.TryParse( Primaries[5], out Chromas.B.y );
					float.TryParse( Primaries[6], out Chromas.W.x );
					float.TryParse( Primaries[7], out Chromas.W.y );
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
		public static float4[,]	DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, bool _bTargetNeedsXYZ, ColorProfile _ColorProfile )
		{
			if ( _Source == null )
				return	null;

			float4[,]	Result = new float4[_Source.GetLength( 0 ), _Source.GetLength( 1 )];
			DecodeRGBEImage( _Source, _bSourceIsXYZ, Result, _bTargetNeedsXYZ, _ColorProfile );

			return Result;
		}

		/// <summary>
		/// Decodes a RGBE formatted image into a plain floating-point image
		/// </summary>
		/// <param name="_Source">The source RGBE formatted image</param>
		/// <param name="_bSourceIsXYZ">Tells if the source image is encoded as XYZE rather than RGBE</param>
		/// <param name="_Target">The target float4 image</param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		public static void			DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, float4[,] _Target, bool _bTargetNeedsXYZ, ColorProfile _ColorProfile )
		{
			if ( _bSourceIsXYZ ^ _bTargetNeedsXYZ )
			{	// Requires conversion...
				if ( _bSourceIsXYZ )
				{	// Convert from XYZ to RGB
					for ( int Y=0; Y < _Source.GetLength( 1 ); Y++ )
						for ( int X=0; X < _Source.GetLength( 0 ); X++ )
							_Target[X,Y] = _ColorProfile.XYZ2RGB( new float4( _Source[X,Y].DecodedColor.x, _Source[X,Y].DecodedColor.y, _Source[X,Y].DecodedColor.z, 1.0f ) );
				}
				else
				{	// Convert from RGB to XYZ
					for ( int Y=0; Y < _Source.GetLength( 1 ); Y++ )
						for ( int X=0; X < _Source.GetLength( 0 ); X++ )
							_Target[X,Y] = _ColorProfile.RGB2XYZ( new float4( _Source[X,Y].DecodedColor.x, _Source[X,Y].DecodedColor.y, _Source[X,Y].DecodedColor.z, 1.0f ) );
				}
				return;
			}

			// Simply decode vector and leave as-is
			for ( int Y=0; Y < _Source.GetLength( 1 ); Y++ )
				for ( int X=0; X < _Source.GetLength( 0 ); X++ )
					_Target[X,Y] = new float4( _Source[X,Y].DecodedColor.x, _Source[X,Y].DecodedColor.y, _Source[X,Y].DecodedColor.z, 1.0f );
		}

		protected static string		RadianceFileFindInHeader( List<string> _HeaderLines, string _Search )
		{
			foreach ( string Line in _HeaderLines )
				if ( Line.IndexOf( _Search ) != -1 )
					return Line.Replace( _Search, "" );	// Return line and remove Search criterium

			return null;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			// Nothing special to do, we only have clean managed types here...
		}

		#endregion

		#endregion
	}
}
