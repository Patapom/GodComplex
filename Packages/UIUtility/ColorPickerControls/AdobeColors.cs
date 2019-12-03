/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          AdobeColors.cs                  *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:	                                          *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/

using System;
using System.Drawing;

using SharpMath;

namespace UIUtility
{
	/// <summary>
	/// Summary description for AdobeColors
	/// </summary>
	public static class AdobeColors {
		#region HSB Methods

		/// <summary> 
		/// Sets the absolute brightness of a colour 
		/// </summary> 
		/// <param name="c">Original colour</param> 
		/// <param name="brightness">The luminance level to impose</param> 
		/// <returns>an adjusted colour</returns> 
		public static float3	SetBrightness( float3 c, float brightness ) { 
			float3	hsl = RGB2HSB( c ); 
					hsl.z = brightness;

			return HSB2RGB( hsl ); 
		} 


		/// <summary> 
		/// Modifies an existing brightness level 
		/// </summary> 
		/// <remarks> 
		/// To reduce brightness use a number smaller than 1. To increase brightness use a number larger tnan 1 
		/// </remarks> 
		/// <param name="c">The original colour</param> 
		/// <param name="brightness">The luminance delta</param> 
		/// <returns>An adjusted colour</returns> 
		public static float3	ModifyBrightness( float3 c, float brightness ) { 
			float3	hsl = RGB2HSB( c );
					hsl.z *= brightness;
 
			return HSB2RGB( hsl );
		} 


		/// <summary> 
		/// Sets the absolute saturation level 
		/// </summary> 
		/// <remarks>Accepted values 0-1</remarks> 
		/// <param name="c">An original colour</param> 
		/// <param name="Saturation">The saturation value to impose</param> 
		/// <returns>An adjusted colour</returns> 
		public static float3	SetSaturation( float3 c, float Saturation ) { 
			float3	hsl = RGB2HSB( c ); 
					hsl.y = Saturation; 

			return HSB2RGB( hsl );
		} 


		/// <summary> 
		/// Modifies an existing Saturation level 
		/// </summary> 
		/// <remarks> 
		/// To reduce Saturation use a number smaller than 1. To increase Saturation use a number larger tnan 1 
		/// </remarks> 
		/// <param name="c">The original colour</param> 
		/// <param name="Saturation">The saturation delta</param> 
		/// <returns>An adjusted colour</returns> 
		public static float3	ModifySaturation( float3 c, float Saturation ) { 
			float3	hsl = RGB2HSB( c );
					hsl.y *= Saturation;

			return HSB2RGB( hsl );
		} 


		/// <summary> 
		/// Sets the absolute Hue level 
		/// </summary> 
		/// <remarks>Accepted values 0-1</remarks> 
		/// <param name="c">An original colour</param> 
		/// <param name="Hue">The Hue value to impose</param> 
		/// <returns>An adjusted colour</returns> 
		public static float3	SetHue( float3 c, float Hue ) { 
			float3	hsl = RGB2HSB( c );
					hsl.x = Hue;

			return HSB2RGB( hsl );
		} 


		/// <summary> 
		/// Modifies an existing Hue level 
		/// </summary> 
		/// <remarks> 
		/// To reduce Hue use a number smaller than 1. To increase Hue use a number larger tnan 1 
		/// </remarks> 
		/// <param name="c">The original colour</param> 
		/// <param name="Hue">The Hue delta</param> 
		/// <returns>An adjusted colour</returns> 
		public static float3	ModifyHue( float3 c, float Hue ) { 
			float3	hsl = RGB2HSB( c );
					hsl.x *= Hue;

			return HSB2RGB( hsl ); 
		} 


		/// <summary> 
		/// Converts a colour from HSL to RGB 
		/// </summary> 
		/// <remarks>Adapted from the algoritm in Foley and Van-Dam</remarks> 
		/// <param name="hsl">The HSL value</param> 
		/// <returns>A float3 structure containing the equivalent RGB values</returns> 
		public static float3	HSB2RGB( float3 hsl ) {
			float	Max = hsl.z;
			float	Min = ((1.0f - hsl.y) * hsl.z);
			float	q = Max - Min;

			float	Mid;
			if ( hsl.x >= 0 && hsl.x <= 1.0 / 6.0 ) {
				Mid = (float) (((hsl.x - 0) * q) * 6 + Min);
				return new float3( Max,Mid,Min );
			} else if ( hsl.x <= 1.0/3.0 ) {
				Mid = (float) (-((hsl.x - 1.0/6.0) * q) * 6 + Max);
				return new float3( Mid,Max,Min);
			} else if ( hsl.x <= 0.5f ) {
				Mid = (float) (((hsl.x - 1.0/3.0) * q) * 6 + Min);
				return new float3( Min,Max,Mid);
			} else if ( hsl.x <= 2.0f/3.0f ) {
				Mid = (float) (-((hsl.x - 0.5) * q) * 6 + Max);
				return new float3( Min,Mid,Max);
			} else if ( hsl.x <= 5.0f/6.0f ) {
				Mid = (float) (((hsl.x - 2.0f / 3.0f) * q) * 6 + Min);
				return new float3( Mid,Min,Max);
			} else if ( hsl.x <= 1.0f ) {
				Mid = (float) (-((hsl.x - 5.0f / 6.0f) * q) * 6 + Max);
				return new float3( Max,Min,Mid);
			} else
				return float3.Zero;
		} 

		public static Color		HSL_to_RGB_LDR( float3 _HSL ) {
			float3	RGB = AdobeColors.HSB2RGB( new float3( _HSL.x, _HSL.y, Math.Min( 1.0f, _HSL.z ) ) );
 			return Color.FromArgb( (int) Math.Max( 0.0f, Math.Min( 255.0f, RGB.x * 255.0f ) ), (int) Math.Max( 0.0f, Math.Min( 255.0f, RGB.y * 255.0f ) ), (int) Math.Max( 0.0f, Math.Min( 255.0f, RGB.z * 255.0f ) ) );
		}

		public static Color		ConvertHDR2LDR( float3 _RGBHDR ) {
			return	HSL_to_RGB_LDR( RGB2HSB( _RGBHDR ) );
		}

		public static float4	RGB_LDR_to_RGB_HDR( Color _RGBLDR ) {
			return RGB_LDR_to_RGB_HDR( _RGBLDR.R, _RGBLDR.G, _RGBLDR.B, _RGBLDR.A );
		}

		public static float4	RGB_LDR_to_RGB_HDR( int _R, int _G, int _B, int _A ) {
			return new float4( _R / 255.0f, _G / 255.0f, _B / 255.0f, _A / 255.0f );
		}

		/// <summary> 
		/// Converts RGB to HSL 
		/// </summary> 
		/// <param name="c">The RGB float3 to convert</param> 
		/// <returns>An HSL value</returns> 
		public static float3 RGB2HSB( float3 c ) { 
			float3 HSB = new float3(); 
          
			float Max, Min, Diff, Sum;

			//	Of our RGB values, assign the highest value to Max, and the Smallest to Min
			if ( c.x > c.y )	{ Max = c.x; Min = c.y; }
			else				{ Max = c.y; Min = c.x; }
			if ( c.z > Max )	  Max = c.z;
			else if ( c.z < Min ) Min = c.z;

			Diff = Max - Min;
			Sum = Max + Min;

			//	Luminance - a.k.a. Brightness - Adobe photoshop uses the logic that the
			//	site VBspeed regards (regarded) as too primitive = superior decides the 
			//	level of brightness.
			HSB.z = Max;
//			hsl.z = 0.5f * (Min + Max);

			//	Saturation
			HSB.y = Max != 0 ? Diff / Max : 0;	// The logic of Adobe Photoshops is this simple.

			//	Hue		R is situated at the angel of 360 eller noll degrees; 
			//			G vid 120 degrees
			//			B vid 240 degrees
			float q = Diff != 0 ? 60.0f / Diff : 0;
			
			if ( Max == c.x ) {
				if ( c.y < c.z )	HSB.x = (360 + q * (c.y - c.z)) / 360.0f;
				else				HSB.x = (q * (c.y - c.z)) / 360.0f;
			}
			else if ( Max == c.y )	HSB.x = (120 + q * (c.z - c.x)) / 360.0f;
			else if ( Max == c.z )	HSB.x = (240 + q * (c.x - c.y)) / 360.0f;
			else					HSB.x = 0.0f;

			return HSB; 
		} 

		#endregion

		#region L*a*b* and XYZ

		static readonly float3	whitePointXYZ = new float3( 0.95047f, 1.0f, 1.08883f );	// Observer= 2°, Illuminant = D65

		/// <summary>
		/// Converts XYZ to L*a*b* (from http://wiki.nuaj.net/index.php?title=Color_Transforms)
		/// </summary>
		/// <param name="_XYZ"></param>
		public static float3	XYZ2Lab( float3 _XYZ ) {
			_XYZ.x /= whitePointXYZ.x;
			_XYZ.y /= whitePointXYZ.y;
			_XYZ.z /= whitePointXYZ.z;

			_XYZ.x = _XYZ.x > 0.008856f ? (float) Math.Pow( _XYZ.x, 1.0 / 3.0 ) : ( 7.787f * _XYZ.x ) + ( 16.0f / 116 );
			_XYZ.y = _XYZ.y > 0.008856f ? (float) Math.Pow( _XYZ.y, 1.0 / 3.0 ) : ( 7.787f * _XYZ.y ) + ( 16.0f / 116 );
			_XYZ.z = _XYZ.z > 0.008856f ? (float) Math.Pow( _XYZ.z, 1.0 / 3.0 ) : ( 7.787f * _XYZ.z ) + ( 16.0f / 116 );

			float3	Lab = new float3();
			Lab.x = (116 * _XYZ.y) - 16;
			Lab.y = 500 * ( _XYZ.x - _XYZ.y );
			Lab.z = 200 * ( _XYZ.y - _XYZ.z );
			return Lab;
		}

		/// <summary>
		/// Converts L*a*b* to XYZ (from http://wiki.nuaj.net/index.php?title=Color_Transforms)
		/// </summary>
		/// <param name="_XYZ"></param>
		public static float3	Lab2XYZ( float3 _Lab ) {
			float3	XYZ = new float3();

			XYZ.y = (_Lab.x + 16) / 116.0f;
			XYZ.x = _Lab.y / 500.0f + XYZ.y;
			XYZ.z = XYZ.y - _Lab.z / 200.0f;

			float3	XYZ3 = XYZ * XYZ * XYZ;
			XYZ.x = XYZ3.x > 0.008856f ? XYZ3.x : (XYZ.x - 16.0f / 116) / 7.787f;
			XYZ.y = XYZ3.y > 0.008856f ? XYZ3.y : (XYZ.y - 16.0f / 116) / 7.787f;
			XYZ.z = XYZ3.z > 0.008856f ? XYZ3.z : (XYZ.z - 16.0f / 116) / 7.787f;

			XYZ *= whitePointXYZ;

			return XYZ;
		}

		/// <summary>
		/// Converts XYZ to *linear* RGB using a D65 illuminant (from http://wiki.nuaj.net/index.php?title=Color_Transforms)
		/// NOTE: Use Linear2sRGB() to further convert to sRGB RGB!
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		public static float3	XYZ2RGB_D65( float3 _XYZ ) {
			float3	RGB = new float3(	_XYZ.x *  3.2406f + _XYZ.y * -1.5372f + _XYZ.z * -0.4986f,
										_XYZ.x * -0.9689f + _XYZ.y *  1.8758f + _XYZ.z *  0.0415f,
										_XYZ.x *  0.0557f + _XYZ.y * -0.2040f + _XYZ.z *  1.0570f );

			return RGB;
		}

		/// <summary>
		/// Converts *linear* RGB to XYZ using a D65 illuminant (from http://wiki.nuaj.net/index.php?title=Color_Transforms)
		/// NOTE: Use sRGB2Linear() before calling this function if you're starting from a sRGB RGB value!
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		public static float3	RGB2XYZ_D65( float3 _RGB ) {
			float3	XYZ = new float3(	_RGB.x * 0.4124f + _RGB.y * 0.3576f + _RGB.z * 0.1805f,
										_RGB.x * 0.2126f + _RGB.y * 0.7152f + _RGB.z * 0.0722f,
										_RGB.x * 0.0193f + _RGB.y * 0.1192f + _RGB.z * 0.9505f );

			return XYZ;
		}

		/// <summary>
		/// Directly converts a sRGB RGB value into a L*a*b* value
		/// </summary>
		/// <param name="_RGB"></param>
		/// <returns></returns>
		public static float3	RGB2Lab_D65( float3 _RGB ) {
			_RGB = sRGB2Linear( _RGB );
			float3	XYZ = RGB2XYZ_D65( _RGB );
			float3	Lab = XYZ2Lab( XYZ );
			return Lab;
		}

		/// <summary>
		/// Directly converts a L*a*b* value into sRGB RGB value
		/// </summary>
		/// <param name="_Lab"></param>
		/// <returns></returns>
		public static float3	Lab2RGB_D65( float3 _Lab ) {
			float3	XYZ = Lab2XYZ( _Lab );
			float3	RGB = XYZ2RGB_D65( XYZ );
			RGB = Linear2sRGB( RGB );
			return RGB;
		}

		/// <summary>
		/// Converts a linear value into an sRGB-corrected value
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float	Linear2sRGB( float x ) {
			return x > 0.0031308f ? 1.055f * (float) Math.Pow( x, 1.0 / 2.4 ) - 0.055f : 12.92f * x;
		}
		public static float3	Linear2sRGB( float3 _RGB ) {
			_RGB.x = Linear2sRGB( _RGB.x );
			_RGB.y = Linear2sRGB( _RGB.y );
			_RGB.z = Linear2sRGB( _RGB.z );
			return _RGB;
		}

		/// <summary>
		/// Converts an sRGB-corrected value into a linear value
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float	sRGB2Linear( float x ) {
			return x > 0.04045f ? (float) Math.Pow( (x + 0.055) / 1.055, 2.4 ) : x / 12.92f;
		}
		public static float3	sRGB2Linear( float3 _RGB ) {
			_RGB.x = sRGB2Linear( _RGB.x );
			_RGB.y = sRGB2Linear( _RGB.y );
			_RGB.z = sRGB2Linear( _RGB.z );
			return _RGB;
		}

		#endregion

		#region Public Classes

// 		[System.Diagnostics.DebuggerDisplay( "HSL=[{_h}, {_s}, {_l}]" )]
// 		public class HSL { 
// 			#region Class Variables
// 
// 			public HSL() 
// 			{ 
// 				_h=0; 
// 				_s=0; 
// 				_l=0; 
// 			} 
// 
// 			public HSL( HSL _Source )
// 			{ 
// 				_h=_Source._h;
// 				_s=_Source._s;
// 				_l=_Source._l;
// 			} 
// 
// 			public HSL( double _H, double _S, double _L )
// 			{ 
// 				_h=_H; 
// 				_s=_S; 
// 				_l=_L; 
// 			} 
// 
// 			double _h; 
// 			double _s; 
// 			double _l; 
// 
// 			#endregion
// 
// 			#region Public Methods
// 
// 			public double H 
// 			{ 
// 				get{return _h;} 
// 				set 
// 				{ 
// 					_h=value; 
// 					_h=_h>1 ? 1 : _h<0 ? 0 : _h; 
// 				} 
// 			} 
// 
// 
// 			public double S 
// 			{ 
// 				get{return _s;} 
// 				set 
// 				{ 
// 					_s=value; 
// 					_s=_s>1 ? 1 : _s<0 ? 0 : _s; 
// 				} 
// 			} 
// 
// 
// 			public double L 
// 			{ 
// 				get{return _l;} 
// 				set 
// 				{ 
// 					_l=value; 
// 					_l = Math.Max( 0.0, _l ); 
// 				} 
// 			} 
// 
// 
// 			#endregion
// 		} 


		[System.Diagnostics.DebuggerDisplay( "CMYK=[{ch}, {_m}, {_y}, {_k}]" )]
		public class CMYK { 
			#region Class Variables

			public CMYK() 
			{ 
				_c=0; 
				_m=0; 
				_y=0; 
				_k=0; 
			} 


			double _c; 
			double _m; 
			double _y; 
			double _k;

			#endregion

			#region Public Methods

			public double C 
			{ 
				get{return _c;} 
				set 
				{ 
					_c=value; 
					_c=_c>1 ? 1 : _c<0 ? 0 : _c; 
				} 
			} 


			public double M 
			{ 
				get{return _m;} 
				set 
				{ 
					_m=value; 
					_m=_m>1 ? 1 : _m<0 ? 0 : _m; 
				} 
			} 


			public double Y 
			{ 
				get{return _y;} 
				set 
				{ 
					_y=value; 
					_y=_y>1 ? 1 : _y<0 ? 0 : _y; 
				} 
			} 


			public double K 
			{ 
				get{return _k;} 
				set 
				{ 
					_k=value; 
					_k=_k>1 ? 1 : _k<0 ? 0 : _k; 
				} 
			} 


			#endregion
		} 

		#endregion
	}
}
