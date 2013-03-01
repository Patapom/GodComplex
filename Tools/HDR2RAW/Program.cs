//////////////////////////////////////////////////////////////////////////
// Converts a HDR image into a RAW RGB float32 format
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HDR2RAW
{
	class Program
	{
		static void Main( string[] args )
		{
// 			FileInfo	SourceFile = new FileInfo( "ennis_1024x512.hdr" );
 			FileInfo	SourceFile = new FileInfo( "doge2_1024x512.hdr" );
// 			FileInfo	SourceFile = new FileInfo( "grace-new_1024x512.hdr" );
// 			FileInfo	SourceFile = new FileInfo( "pisa_1024x512.hdr" );
//			FileInfo	SourceFile = new FileInfo( "uffizi-large_1024x512.hdr" );

			FileInfo	TargetFile = new FileInfo( Path.GetFileNameWithoutExtension( SourceFile.FullName ) + ".float" );

			byte[]	HDRContent = null;
			using ( Stream S = SourceFile.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) )
				{
					HDRContent = new byte[S.Length];
					R.Read( HDRContent, 0, (int) S.Length );
				}

			Vector4[,]	HDRValues = LoadAndDecodeHDRFormat( HDRContent, false );

			using ( Stream S = TargetFile.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
				{
					int	Width = HDRValues.GetLength( 0 );
					int	Height = HDRValues.GetLength( 1 );
					for ( int Y=0; Y < Height; Y++ )
						for ( int X=0; X < Width; X++ )
						{
							W.Write( HDRValues[X,Y].x );
							W.Write( HDRValues[X,Y].y );
							W.Write( HDRValues[X,Y].z );
						}
				}
		}

		#region HDR Loaders

		/// <summary>
		/// This format is a special encoding of 3 floating point values into 4 byte values, aka "Real Pixels"
		/// The RGB encode the mantissa of each RGB float component while A encodes the exponent by which multiply these 3 mantissae
		/// In fact, we only use a single common exponent that we factor out to 3 different mantissae.
		/// This format was first created by Gregory Ward for his Radiance software (http://www.graphics.cornell.edu/~bjw/rgbe.html)
		///  and allows to store HDR values using standard 8-bits formats.
		/// It's also quite useful to pack some data as we divide the size by 3, from 3 floats (12 bytes) down to only 4 bytes.
		/// </summary>
		/// <remarks>This format only allows storage of POSITIVE floats !</remarks>
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	PF_RGBE
		{
			public byte	B, G, R, E;

			#region IPixelFormat Members

			public bool	sRGB	{ get { return false; } }

// 			// NOTE: Alpha is ignored, RGB is encoded in RGBE
// 			public void Write( Vector4 _Color )
// 			{
// 				float	fMaxComponent = Math.Max( _Color.x, Math.Max( _Color.y, _Color.z ) );
// 				if ( fMaxComponent < 1e-16f )
// 				{	// Too low to encode...
// 					R = G = B = E = 0;
// 					return;
// 				}
// 
// 				double	CompleteExponent = Math.Log( fMaxComponent ) / Math.Log( 2.0 );
// 				int		Exponent = (int) Math.Ceiling( CompleteExponent );
// 				double	Mantissa = fMaxComponent / Math.Pow( 2.0f, Exponent );
// 				if ( Mantissa == 1.0 )
// 				{	// Step to next order
// 					Mantissa = 0.5;
// 					Exponent++;
// 				}
// 
// 				double	Debug0 = Mantissa * Math.Pow( 2.0, Exponent );
// 
// 				fMaxComponent = (float) Mantissa * 255.99999999f / fMaxComponent;
// 
// 				R = (byte) (_Color.x * fMaxComponent);
// 				G = (byte) (_Color.y * fMaxComponent);
// 				B = (byte) (_Color.z * fMaxComponent);
// 				E = (byte) (Exponent + 128 );
// 			}

			#endregion
	
			public Vector4	DecodedColorAsVector
			{
				get
				{
					double Exponent = Math.Pow( 2.0, E - (128 + 8) );
					return new Vector4(	(float) ((R + .5) * Exponent),
										(float) ((G + .5) * Exponent),
										(float) ((B + .5) * Exponent),
										1.0f
										);
				}
			} 
		}

		public class Vector4
		{
			public float	x, y, z, w;
			public Vector4( float _x, float _y, float _z, float _w )
			{
				x = _x; y = _y; z = _z; w = _w;
			}
		}

		/// <summary>
		/// Loads a bitmap in .HDR format into a Vector4 array directly useable by the image constructor
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static Vector4[,]	LoadAndDecodeHDRFormat( byte[] _HDRFormatBinary, bool _bTargetNeedsXYZ )
		{
			bool	bSourceIsXYZ;
			return DecodeRGBEImage( LoadHDRFormat( _HDRFormatBinary, out bSourceIsXYZ ), bSourceIsXYZ, _bTargetNeedsXYZ );
		}

		/// <summary>
		/// Loads a bitmap in .HDR format into a RGBE array
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bIsXYZ">Tells if the image is encoded as XYZE rather than RGBE</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static unsafe PF_RGBE[,]	LoadHDRFormat( byte[] _HDRFormatBinary, out bool _bIsXYZ )
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
// 				ColorProfile.Chromaticities	Chromas = ColorProfile.Chromaticities.Radiance;	// Default chromaticities
// 				string	PrimariesText = RadianceFileFindInHeader( HeaderLines, "PRIMARIES=" );
// 				if ( PrimariesText != null )
// 				{
// 					string[]	Primaries = PrimariesText.Split( ' ' );
// 					if ( Primaries == null || Primaries.Length != 8 )
// 						throw new Exception( "Failed to parse color profile chromaticities !" );
// 
// 					float.TryParse( Primaries[0], out Chromas.R.X );
// 					float.TryParse( Primaries[1], out Chromas.R.Y );
// 					float.TryParse( Primaries[2], out Chromas.G.X );
// 					float.TryParse( Primaries[3], out Chromas.G.Y );
// 					float.TryParse( Primaries[4], out Chromas.B.X );
// 					float.TryParse( Primaries[5], out Chromas.B.Y );
// 					float.TryParse( Primaries[6], out Chromas.W.X );
// 					float.TryParse( Primaries[7], out Chromas.W.Y );
// 				}
// 
// 					// 3.5] Create the color profile
// 				_ColorProfile = new ColorProfile( Chromas, ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );
// 				_ColorProfile.Exposure = fExposure;

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
		public static Vector4[,]	DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, bool _bTargetNeedsXYZ )
		{
			if ( _Source == null )
				return	null;

			Vector4[,]	Result = new Vector4[_Source.GetLength( 0 ), _Source.GetLength( 1 )];
			DecodeRGBEImage( _Source, _bSourceIsXYZ, Result, _bTargetNeedsXYZ );

			return Result;
		}

		/// <summary>
		/// Decodes a RGBE formatted image into a plain floating-point image
		/// </summary>
		/// <param name="_Source">The source RGBE formatted image</param>
		/// <param name="_bSourceIsXYZ">Tells if the source image is encoded as XYZE rather than RGBE</param>
		/// <param name="_Target">The target Vector4 image</param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		public static void			DecodeRGBEImage( PF_RGBE[,] _Source, bool _bSourceIsXYZ, Vector4[,] _Target, bool _bTargetNeedsXYZ )
		{
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
	}
}
