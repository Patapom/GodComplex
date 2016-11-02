//////////////////////////////////////////////////////////////////////////
// This special Bitmap class handles many image formats (JPG, PNG, BMP, TGA, GIF, HDR and especially RAW camera formats)
// It also carefully handles color profiles to provide a faithful internal image representation that is always
//	stored as 32-bits floating point precision CIE XYZ device-independent format that you can later convert to
//	any other format.
//
//	@TODO: Avoir la possibilité de créer une texture avec un seul channel du bitmap !? (filtrage)
//	@TODO: Handle "premultiplied alpha"
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#include "Types.h"
#include "PixelFormats.h"
#include "ColorProfile.h"

namespace ImageUtilityLib {

	/// <summary>
	/// The Bitmap class should be used to replace the standard System.Drawing.Bitmap
	/// The big advantage of the Bitmap class is to accurately read back the color profile and gamma correction data stored in the image's metadata
	/// so that, internally, the image is stored:
	///		• As device-independent CIE XYZ (http://en.wikipedia.org/wiki/CIE_1931_color_space) format, our Profile Connection Space
	///		• In linear space (i.e. no gamma curve is applied)
	///		• NOT pre-multiplied alpha (you can later re-pre-multiply if needed)
	///	
	/// This helps to ensure that whatever the source image format stored on disk, you always deal with a uniformized image internally.
	/// 
	/// Later, you can cast from the CIE XYZ device-independent format into any number of pre-defined texture profiles:
	///		• sRGB or Linear space textures (for 8bits per component images only)
	///		• Compressed (BC1-BC5) or uncompressed (for 8bits per component images only)
	///		• 8-, 16-, 16F- 32- or 32F-bits per component
	///		• Pre-multiplied alpha or not
	/// 
	/// The following image formats are currently supported:
	///		• JPG
	///		• PNG
	///		• TIFF
	///		• TGA
	///		• BMP
	///		• GIF
	///		• HDR
	///		• Any RAW camera format supported by the LibRaw library
	/// </summary>
	/// <remarks>The Bitmap class has been tested with various formats, various bit depths and color profiles all created from Adobe Photoshop CS4 using
	/// the "Save As" dialog and the "Save for Web & Devices" dialog box.
	/// 
	/// In a general manner, you should NOT use the latter save option but rather select your working color profile from the "Edit > Color Settings" menu,
	///  then save your files and make sure you tick the "ICC Profile" checkbox using the DEFAULT save file dialog box to embed that profile in the image.
	/// </remarks>
	public class Bitmap : IDisposable
	{
		#pragma region CONSTANTS

		private static readonly System.Windows.Media.PixelFormat	GENERIC_PIXEL_FORMAT = System.Windows.Media.PixelFormats.Rgba128Float;
		protected const float	BYTE_TO_FLOAT = 1.0f / 255.0f;
		protected const float	WORD_TO_FLOAT = 1.0f / 65535.0f;

		#pragma endregion

		#pragma region NESTED TYPES

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
		/// Formatting flags for the Save() method
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
		/// This is an aggregate of the various options that can be fed to the Save() method
		/// </summary>
		public class FormatEncoderOptions
		{
			// FILE_TYPE == JPEG
			public int	JPEGQualityLevel = 80;	// 80%

			// FILE_TYPE == PNG
			public PngInterlaceOption	PNGInterlace = PngInterlaceOption.Default;

			// FILE_TYPE == TIFF
			public TiffCompressOption	TIFFCompression = TiffCompressOption.Rle;
		}

		#pragma endregion

		#pragma region FIELDS

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

		protected static bool		ms_ReadContent = true;
		protected static bool		ms_ConvertContent2XYZ = true;

		#pragma endregion

		#pragma region PROPERTIES

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
		public float4[,]	ContentXYZ				{ get { return m_Bitmap; } }

		/// <summary>
		/// Gets the image content as RGB using the color profile of the image as the reference transformation
		/// </summary>
		/// <remarks>Warning! This does a XYZ->RGB transform on the fly and is quite heavy</remarks>
		public float4[,]	ConvertedContentRGB {
			get {
				float4[,]	Result = new float4[m_Width,m_Height];
				m_ColorProfile.XYZ2RGB( m_Bitmap, Result );
				return Result;
			}
		}

		/// <summary>
		/// Gets or sets the image's color profile
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

		/// <summary>
		/// Gets or sets the DontReadContent state flag that allows to skip reading the content of a file, only its header containing file information (e.g. width, height) is available
		/// </summary>
		public static bool	ReadContent				{ get { return ms_ReadContent; } set { ms_ReadContent = value; } }

		/// <summary>
		/// Gets or sets the DontReadContent state flag that allows to skip reading the content of a file, only its header containing file information (e.g. width, height) is available
		/// </summary>
		public static bool	ConvertContent2XYZ		{ get { return ms_ConvertContent2XYZ; } set { ms_ConvertContent2XYZ = value; } }

		#pragma endregion

		#pragma region METHODS

		/// <summary>
		/// Manual creation
		/// </summary>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		/// <param name="_Profile">An optional color profile, you will need a valid profile if you wish to save the bitmap!</param>
		public Bitmap( int _Width, int _Height, ColorProfile _Profile )
		{
			m_Width = _Width;
			m_Height = _Height;
			m_Bitmap = new float4[m_Width,m_Height];
			for ( int Y=0; Y < m_Height; Y++ )
				for ( int X=0; X < m_Width; X++ )
					m_Bitmap[X,Y] = new float4( 0, 0, 0, 0 );
			m_ColorProfile = _Profile;
		}

		/// <summary>
		/// Creates a bitmap from a file
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		public	Bitmap( System.IO.FileInfo _ImageFileName )
		{
			Load( _ImageFileName, GetFileType( _ImageFileName ) );
		}

		/// <summary>
		/// Creates a bitmap from a file
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		public	Bitmap( System.IO.FileInfo _ImageFileName, ColorProfile _ProfileOverride )
		{
			Load( _ImageFileName, GetFileType( _ImageFileName ), _ProfileOverride );
		}

		/// <summary>
		/// Creates a bitmap from a stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageStream">The image stream to load the bitmap from</param>
		/// <param name="_ImageFileNameName">The name of the image file the stream is coming from originally (used to identify image file type)</param>
		public	Bitmap( System.IO.Stream _ImageStream, System.IO.FileInfo _ImageFileNameName )
		{
			Load( _ImageStream, GetFileType( _ImageFileNameName ) );
		}

		/// <summary>
		/// Creates a bitmap from a stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageStream">The image stream to load the bitmap from</param>
		/// <param name="_ImageFileNameName">The name of the image file the stream is coming from originally (used to identify image file type)</param>
		public	Bitmap( System.IO.Stream _ImageStream, System.IO.FileInfo _ImageFileNameName, ColorProfile _ProfileOverride )
		{
			Load( _ImageStream, GetFileType( _ImageFileNameName ), _ProfileOverride );
		}

		/// <summary>
		/// Creates a bitmap from a stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageStream">The image stream to load the bitmap from</param>
		/// <param name="_FileType">The image type</param>
		public	Bitmap( System.IO.Stream _ImageStream, FILE_TYPE _FileType )
		{
			Load( _ImageStream, _FileType );
		}

		/// <summary>
		/// Creates a bitmap from a stream
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageStream">The image stream to load the bitmap from</param>
		/// <param name="_FileType">The image type</param>
		public	Bitmap( System.IO.Stream _ImageStream, FILE_TYPE _FileType, ColorProfile _ProfileOverride )
		{
			Load( _ImageStream, _FileType, _ProfileOverride );
		}

		/// <summary>
		/// Creates a bitmap from memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageFileContent">The memory buffer to load the bitmap from</param>
		/// <param name="_ImageFileNameName">The name of the image file the stream is coming from originally (used to identify image file type)</param>
		public	Bitmap( byte[] _ImageFileContent, System.IO.FileInfo _ImageFileNameName )
		{
			Load( _ImageFileContent, GetFileType( _ImageFileNameName ) );
		}

		/// <summary>
		/// Creates a bitmap from memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageFileContent">The memory buffer to load the bitmap from</param>
		/// <param name="_ImageFileNameName">The name of the image file the stream is coming from originally (used to identify image file type)</param>
		public	Bitmap( byte[] _ImageFileContent, System.IO.FileInfo _ImageFileNameName, ColorProfile _ProfileOverride )
		{
			Load( _ImageFileContent, GetFileType( _ImageFileNameName ), _ProfileOverride );
		}

		/// <summary>
		/// Creates a bitmap from memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageFileContent">The memory buffer to load the bitmap from</param>
		/// <param name="_FileType">The image type</param>
		public	Bitmap( byte[] _ImageFileContent, FILE_TYPE _FileType )
		{
			Load( _ImageFileContent, _FileType );
		}

		/// <summary>
		/// Creates a bitmap from memory
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_ImageFileContent">The memory buffer to load the bitmap from</param>
		/// <param name="_FileType">The image type</param>
		public	Bitmap( byte[] _ImageFileContent, FILE_TYPE _FileType, ColorProfile _ProfileOverride )
		{
			Load( _ImageFileContent, _FileType, _ProfileOverride );
		}

		/// <summary>
		/// Creates a bitmap from a System.Drawing.Bitmap and a color profile
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_Bitmap">The System.Drawing.Bitmap</param>
		/// <param name="_ColorProfile">The color profile to use to transform the bitmap</param>
		public	Bitmap( System.Drawing.Bitmap _Bitmap, ColorProfile _ColorProfile )
		{
			if ( _ColorProfile == null )
				throw new Exception( "Invalid profile: can't convert to CIE XYZ!" );
			m_ColorProfile = _ColorProfile;

			// Load the bitmap's content and copy it to a double entry array
			byte[]	BitmapContent = LoadBitmap( _Bitmap, out m_Width, out m_Height );
			if ( BitmapContent == null )
				return;

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

			if ( ms_ConvertContent2XYZ ) {
				// Convert to CIE XYZ
				m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
			}
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
			Load( _ImageFileName, _FileType, null );
		}
		public void	Load( System.IO.FileInfo _ImageFileName, FILE_TYPE _FileType, ColorProfile _ProfileOverride )
		{
			using ( System.IO.FileStream ImageStream = _ImageFileName.Open( System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read ) )
				Load( ImageStream, _FileType, _ProfileOverride );
		}

		/// <summary>
		/// Loads from stream
		/// </summary>
		/// <param name="_ImageStream"></param>
		/// <param name="_FileType"></param>
		public void	Load( System.IO.Stream _ImageStream, FILE_TYPE _FileType )
		{
			Load( _ImageStream, _FileType, null );
		}
		public void	Load( System.IO.Stream _ImageStream, FILE_TYPE _FileType, ColorProfile _ProfileOverride )
		{
			// Read the file's content
			byte[]	ImageContent = new byte[_ImageStream.Length];
			_ImageStream.Read( ImageContent, 0, (int) _ImageStream.Length );

			Load( ImageContent, _FileType, _ProfileOverride );
		}

		/// <summary>
		/// Actual load from a byte[] in memory
		/// </summary>
		/// <param name="_ImageFileContent">The source image content as a byte[]</param>
		/// <param name="_FileType">The type of file to load</param>
		/// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
		/// <exception cref="Exception">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
		public void	Load( byte[] _ImageFileContent, FILE_TYPE _FileType )
		{
			Load( _ImageFileContent, _FileType );
		}
		public void	Load( byte[] _ImageFileContent, FILE_TYPE _FileType, ColorProfile _ProfileOverride )
		{
			m_Type = _FileType;
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
								throw new Exception( "BitmapDecoder failed to read at least one bitmap frame!" );

							BitmapFrame	Frame = Decoder.Frames[0];
							if ( Frame == null )
								throw new Exception( "Invalid decoded bitmap!" );

// DEBUG
// int		StrideX = (Frame.Format.BitsPerPixel>>3)*Frame.PixelWidth;
// byte[]	DebugImageSource = new byte[StrideX*Frame.PixelHeight];
// Frame.CopyPixels( DebugImageSource, StrideX, 0 );
// DEBUG

// pas de gamma sur les JPEG si non spécifié !
// Il y a bien une magouille faite lors de la conversion par le FormatConvertedBitmap!


							// ===== 2] Build the color profile =====
							m_ColorProfile = _ProfileOverride != null ? _ProfileOverride : new ColorProfile( Frame.Metadata as BitmapMetadata, _FileType );

							// ===== 3] Convert the frame to generic RGBA32F =====
							ConvertFrame( Frame );

							// ===== 4] Convert to CIE XYZ (our device-independent profile connection space) =====
							if ( ms_ReadContent && ms_ConvertContent2XYZ )
								m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
						}
						break;

					case FILE_TYPE.TGA:
						{
							// Load as a System.Drawing.Bitmap and convert to float4
							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
								using ( TargaImage TGA = new TargaImage( Stream, !ms_ReadContent ) ) {
									// Create a default sRGB linear color profile
									m_ColorProfile = _ProfileOverride != null ? _ProfileOverride
										: new ColorProfile(
											ColorProfile.Chromaticities.sRGB,	// Use default sRGB color profile
											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
											TGA.ExtensionArea.GammaRatio		// ...whose gamma is retrieved from extension data
										);

									if ( ms_ReadContent ) {
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

										if ( ms_ConvertContent2XYZ ) {
											// Convert to CIEXYZ
											m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
										}
									} else {
										// Only read dimensions
										m_Width = TGA.Header.Width;
										m_Height = TGA.Header.Height;
									}
								}
							return;
						}

					case FILE_TYPE.HDR:
						{
							// Load as XYZ
							m_Bitmap = LoadAndDecodeHDRFormat( _ImageFileContent, true, _ProfileOverride, out m_ColorProfile );
							m_Width = m_Bitmap.GetLength( 0 );
							m_Height = m_Bitmap.GetLength( 1 );
							return;
						}

				#if USE_LIB_RAW
					case FILE_TYPE.CRW:
					case FILE_TYPE.CR2:
					case FILE_TYPE.DNG:
						{
							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
								using ( LibRawManaged.RawFile Raw = new LibRawManaged.RawFile() ) {
									Raw.UnpackRAW( Stream );

									ColorProfile.Chromaticities	Chroma = Raw.ColorProfile == LibRawManaged.RawFile.COLOR_PROFILE.ADOBE_RGB
																		? ColorProfile.Chromaticities.AdobeRGB_D65	// Use Adobe RGB
																		: ColorProfile.Chromaticities.sRGB;			// Use default sRGB color profile

									// Create a default sRGB linear color profile
									m_ColorProfile = _ProfileOverride != null ? _ProfileOverride
										: new ColorProfile(
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

									if ( ms_ReadContent ) {
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

										if ( ms_ConvertContent2XYZ ) {
											// Convert to CIEXYZ
											m_ColorProfile.RGB2XYZ( m_Bitmap, m_Bitmap );
										}
									}
								}

#pragma region My poor attempt at reading CRW files
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
#pragma endregion
							return;
 						}
					#endif

					default:
						throw new NotSupportedException( "The image file type \"" + _FileType + "\" is not supported by the Bitmap class!" );
				}
			}
			catch ( Exception )
			{
				throw;	// Go on !
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
		protected void	ConvertFrame( BitmapSource _Frame ) {
			m_Width = _Frame.PixelWidth;
			m_Height = _Frame.PixelHeight;
			if ( !ms_ReadContent )
				return;

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
		/// Save to a file
		/// </summary>
		/// <param name="_Stream"></param>
		/// <param name="_FileType"></param>
		/// <param name="_Parms"></param>
		public void	Save( System.IO.FileInfo _FileName )
		{
			Save( _FileName, FORMAT_FLAGS.NONE );
		}
		public void	Save( System.IO.FileInfo _FileName, FORMAT_FLAGS _Parms )
		{
			Save( _FileName, _Parms, null );
		}
		public void	Save( System.IO.FileInfo _FileName, FORMAT_FLAGS _Parms, FormatEncoderOptions _Options )
		{
			FILE_TYPE	FileType = GetFileType( _FileName );
			using ( System.IO.FileStream S = _FileName.Create() )
				Save( S, FileType, _Parms, _Options );
		}

		/// <summary>
		/// Save to a stream
		/// </summary>
		/// <param name="_Stream">The stream to write the image to</param>
		/// <param name="_FileType">The file type to save as</param>
		/// <param name="_Parms">Additional formatting flags</param>
		/// <param name="_Options">An optional block of options for encoding</param>
		/// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
		/// <exception cref="Exception">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
		public void	Save( System.IO.Stream _Stream, FILE_TYPE _FileType, FORMAT_FLAGS _Parms, FormatEncoderOptions _Options )
		{
			if ( m_ColorProfile == null )
				throw new Exception( "You can't save the bitmap if you don't provide a valid color profile!" );

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

							if ( _Options != null )
							{
								switch ( _FileType )
								{
									case FILE_TYPE.JPEG:
										(Encoder as JpegBitmapEncoder).QualityLevel = _Options.JPEGQualityLevel;
										break;

									case FILE_TYPE.PNG:
										(Encoder as PngBitmapEncoder).Interlace = _Options.PNGInterlace;
										break;

									case FILE_TYPE.TIFF:
										(Encoder as TiffBitmapEncoder).Compression = _Options.TIFFCompression;
										break;

									case FILE_TYPE.GIF:
										break;

									case FILE_TYPE.BMP:
										break;
								}
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
									case 8:		Format = System.Windows.Media.PixelFormats.Bgr24; break;
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

//					case FILE_TYPE.TGA:
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

//					case FILE_TYPE.HDR:
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
			int		W = m_Width;
			int		H = m_Height;

			// Convert to RGB first
			float4[,]	SourceXYZ = m_Bitmap;
			if ( _Format == System.Windows.Media.PixelFormats.Gray8 ||
				 _Format == System.Windows.Media.PixelFormats.Gray16 ||
				 _Format == System.Windows.Media.PixelFormats.Gray32Float ) {
				// Convert to grayscale
				float4[,]	XYZ = new float4[W,H];
				Array.Copy( m_Bitmap, XYZ, m_Bitmap.LongLength );
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						float3	xyY = ColorProfile.XYZ2xyY( (float3) XYZ[X,Y] );
						xyY.x = m_ColorProfile.Chromas.W.x;
						xyY.y = m_ColorProfile.Chromas.W.y;
						XYZ[X,Y] = new float4( ColorProfile.xyY2XYZ( xyY ), XYZ[X,Y].w );
					}
				SourceXYZ = XYZ;
			}
			
			float4[,]	RGB;
			if ( ConvertContent2XYZ ) {
				RGB = new float4[W,H];
				m_ColorProfile.XYZ2RGB( SourceXYZ, RGB );	// Standard conversion
			} else
				RGB = SourceXYZ;

			Array	Pixels = null;
			int		Stride = 0;

			//////////////////////////////////////////////////////////////////////////
			// BGR24
			if ( _Format == System.Windows.Media.PixelFormats.Bgr24 ) {	
				Stride = 3*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGR32
			else if ( _Format == System.Windows.Media.PixelFormats.Bgr32 ) {	
				Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int	Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Position++;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// BGRA32
			else if ( _Format == System.Windows.Media.PixelFormats.Bgra32 ) {	
				Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].w );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PBGRA32 (Pre-Multiplied)
			else if ( _Format == System.Windows.Media.PixelFormats.Pbgra32 ) {	
				Stride = 4*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						RGB[X,Y].x *= RGB[X,Y].w;
						RGB[X,Y].y *= RGB[X,Y].w;
						RGB[X,Y].z *= RGB[X,Y].w;
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].w );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGB48
			else if ( _Format == System.Windows.Media.PixelFormats.Rgb48 ) {	
				Stride = 6*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].z );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// RGBA64
			else if ( _Format == System.Windows.Media.PixelFormats.Rgba64 ) {	
				Stride = 8*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].x );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].y );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].z );
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].w );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA64 (Pre-Multiplied)
			else if ( _Format == System.Windows.Media.PixelFormats.Prgba64 ) {	
				Stride = 8*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
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
			else if ( _Format == System.Windows.Media.PixelFormats.Rgba128Float ) {	
				Stride = 16*W;
				float[]	Content = new float[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = RGB[X,Y].x;
						Content[Position++] = RGB[X,Y].y;
						Content[Position++] = RGB[X,Y].z;
						Content[Position++] = RGB[X,Y].w;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// PRGBA128F (Pre-Multiplied)
			else if ( _Format == System.Windows.Media.PixelFormats.Prgba128Float ) {	
				Stride = 16*W;
				float[]	Content = new float[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
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
			else if ( _Format == System.Windows.Media.PixelFormats.Gray16 ) {	
				Stride = 2*W;
				ushort[]	Content = new ushort[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_WORD( RGB[X,Y].x );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray32F
			else if ( _Format == System.Windows.Media.PixelFormats.Gray32Float ) {	
				Stride = 4*W;
				float[]	Content = new float[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = RGB[X,Y].x;
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// Gray8
			else if ( _Format == System.Windows.Media.PixelFormats.Gray8 ) {	
				Stride = 1*W;
				byte[]	Content = new byte[Stride*H];
				Pixels = Content;

				int		Position = 0;
				for ( int Y = 0; Y < H; Y++ )
					for ( int X = 0; X < W; X++ ) {
						Content[Position++] = FLOAT_TO_BYTE( RGB[X,Y].x );
					}
			}
			//////////////////////////////////////////////////////////////////////////
			// 256 Colors Palette
			else if ( _Format == System.Windows.Media.PixelFormats.Indexed8 ) {
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
		public static unsafe byte[]	LoadBitmap( System.Drawing.Bitmap _Bitmap, out int _Width, out int _Height ) {
			_Width = _Bitmap.Width;
			_Height = _Bitmap.Height;
			if ( !ms_ReadContent )
				return null;

			byte[]	Result = null;
			byte*	pScanline;
			byte	R, G, B, A;

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

		#pragma region HDR Loaders

		/// <summary>
		/// Loads a bitmap in .HDR format into a float4 array directly useable by the image constructor
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bTargetNeedsXYZ">Tells if the target needs to be in CIE XYZ space (true) or RGB (false)</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static float4[,]	LoadAndDecodeHDRFormat( byte[] _HDRFormatBinary, bool _bTargetNeedsXYZ, ColorProfile _ProfileOverride, out ColorProfile _ColorProfile )
		{
			bool	bSourceIsXYZ;
			return DecodeRGBEImage( LoadHDRFormat( _HDRFormatBinary, _ProfileOverride, out bSourceIsXYZ, out _ColorProfile ), bSourceIsXYZ, _bTargetNeedsXYZ, _ColorProfile );
		}

		/// <summary>
		/// Loads a bitmap in .HDR format into a RGBE array
		/// </summary>
		/// <param name="_HDRFormatBinary"></param>
		/// <param name="_bIsXYZ">Tells if the image is encoded as XYZE rather than RGBE</param>
		/// <param name="_ColorProfile">The color profile for the image</param>
		/// <returns></returns>
		public static unsafe PF_RGBE[,]	LoadHDRFormat( byte[] _HDRFormatBinary, ColorProfile _ProfileOverride, out bool _bIsXYZ, out ColorProfile _ColorProfile )
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
				if ( _ProfileOverride == null )
				{
					_ColorProfile = new ColorProfile( Chromas, ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );
					_ColorProfile.Exposure = fExposure;
				}
				else
					_ColorProfile = _ProfileOverride;

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
				PF_RGBE[,]	Dest = null;
				if ( ms_ReadContent ) {
					// 1] Allocate memory for the image and the temporary p_HDRFormatBinaryScanline
					Dest = new PF_RGBE[Width, Height];

					// 2] Read the scanlines
					byte[,]		TempScanline = new byte[Width,4];
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

		#pragma endregion

		#pragma region IDisposable Members

		public void Dispose()
		{
			// Nothing special to do, we only have clean managed types here...
		}

		#pragma endregion

		#pragma endregion
	}
}