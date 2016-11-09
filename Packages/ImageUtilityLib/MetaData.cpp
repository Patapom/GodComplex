#include "stdafx.h"

#include "MetaData.h"
#include "ImageFile.h"
#include "ColorProfile.h"

using namespace ImageUtilityLib;

MetaData::MetaData() : m_colorProfile( nullptr ) {
	Reset();
}
MetaData::MetaData( const MetaData& _other ) {
	*this = _other;
}
MetaData::~MetaData() {
	SAFE_DELETE( m_colorProfile );
}

void	MetaData::Reset() {
	m_gammaSpecifiedInFile = false;
	m_gammaExponent = ColorProfile::GAMMA_EXPONENT_STANDARD;

	m_valid = false;
	m_ISOSpeed = 100;
	m_exposureTime = 0.0f;
	m_Tv = 0.0f;
	m_Av = 0.0f;
//	m_exposureBias = 0.0f;
	m_focalLength = 0.0f;

	SAFE_DELETE( m_colorProfile );
}

MetaData&	MetaData::operator=( const MetaData& _other ) {
	// Copy in bulk
	memcpy_s( &m_colorProfile, sizeof(MetaData), &_other.m_colorProfile, sizeof(MetaData) );

	// But make a deep copy of the profile
	if ( _other.m_colorProfile != nullptr ) {
		m_colorProfile = new ColorProfile( *_other.m_colorProfile );
	}

	return *this;
}

void	MetaData::RetrieveFromImage( const ImageFile& _imageFile ) {
	Reset();

// 	// Try to use the file's associated profile if it exists
// 	FIICCPROFILE*	fileProfile = FreeImage_GetICCProfile( _bitmap );
// 	if ( fileProfile != nullptr ) {
//		@TODO => Use ICC profile lib to read embedded profile
// 	}

	// Attempt to grab generic data
	EnumerateDefaultTags( _imageFile );

	// Grab format-specific data and build color profile
	ImageFile::FILE_FORMAT	format = _imageFile.GetFileFormat();
	switch ( format ) {
	case ImageFile::FILE_FORMAT::PNG:
		EnumerateMetaDataPNG( _imageFile );
		break;
	case ImageFile::FILE_FORMAT::JPEG:
	case ImageFile::FILE_FORMAT::JP2:
		EnumerateMetaDataJPG( _imageFile );
		break;
	case ImageFile::FILE_FORMAT::TARGA:
		EnumerateMetaDataTGA( _imageFile );
		break;
	case ImageFile::FILE_FORMAT::TIFF:
		EnumerateMetaDataTIFF( _imageFile );
		break;
	case ImageFile::FILE_FORMAT::RAW:
		EnumerateMetaDataRAW( _imageFile );
		break;
	case ImageFile::FILE_FORMAT::BMP:
		EnumerateMetaDataBMP( _imageFile );
		break;
	case ImageFile::FILE_FORMAT::GIF:
		EnumerateMetaDataGIF( _imageFile );
		break;
	default:
		// Build the default sRGB profile
		m_colorProfile = new ColorProfile( ColorProfile::STANDARD_PROFILE::sRGB );
		m_colorProfile->SetProfileFoundInFile( false );
		break;
	}
}

#pragma region Format-specific metadata enumeration

void	MetaData::EnumerateMetaDataTGA( const ImageFile& _image ) {
	if ( !m_gammaSpecifiedInFile ) {
		// If not alerady found in EXIF data, try and readback our custom tags
		// NOTE: I had to modify the FreeImage library to add these custom metadata when loading a TGA file otherwise they are lost
		m_gammaExponent = ColorProfile::GAMMA_EXPONENT_STANDARD;

		const char*	numStr = nullptr;
		const char* denStr = nullptr;
		if (	MetaData::GetString( FIMD_COMMENTS, *_image.m_bitmap, "GammaNumerator", numStr )
			&&	MetaData::GetString( FIMD_COMMENTS, *_image.m_bitmap, "GammaDenominator", denStr ) ) {

			int	num, den;
			if (	sscanf_s( numStr, "%d", &num ) == 1
				&&	sscanf_s( denStr, "%d", &den ) == 1 ) {
				m_gammaExponent = float(num) / den;
				m_gammaSpecifiedInFile = true;
			}
		}
	}

	// Create the color profile
	m_colorProfile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Use default sRGB color profile
										ColorProfile::GAMMA_CURVE::STANDARD,	// But with a standard gamma curve...
										m_gammaExponent							// ...whose gamma is retrieved from extension data, if available
									);
	m_colorProfile->SetProfileFoundInFile( m_gammaSpecifiedInFile );
}

void	MetaData::EnumerateMetaDataJPG( const ImageFile& _image ) {
// 	EnumerateMetaData( _MetaData,
// 		new MetaDataProcessor( "/xmp", ( object _SubData ) =>
// 		{
// 			BitmapMetadata	SubData = _SubData as BitmapMetadata;
// 
// 			// Retrieve gamma ramp
// 			gammaWasSpecified = FindPhotometricInterpretation( SubData, "/tiff:PhotometricInterpretation" );
// 
//  			// Let's look for the ICCProfile line that Photoshop puts out...
// 			if ( profileFound = FindICCProfileString( SubData, ref gammaWasSpecified ) )
// 				return;
// 
// 			// Ok ! So we got nothing so far... Try and read a recognized color space
// 			profileFound = FindEXIFColorProfile( SubData );
// 		} )
// 
// // These are huffman tables (cf. http://www.impulseadventure.com/photo/optimized-jpeg.html)
// // Nothing to do with color profiles
// // 					new MetaDataProcessor( "/luminance/TableEntry", ( object _SubData ) =>
// // 					{
// // 					} ),
// // 
// // 					new MetaDataProcessor( "/chrominance/TableEntry", ( object _SubData ) =>
// // 					{
// // 					} )
// 		);

	m_colorProfile = new ColorProfile(	ColorProfile::Chromaticities::sRGB,		// Default for JPEGs is sRGB
										ColorProfile::GAMMA_CURVE::STANDARD,
										m_gammaSpecifiedInFile ? m_gammaExponent : ColorProfile::GAMMA_EXPONENT_STANDARD	// Unless specified, JPG uses a 2.2 gamma by default
									);
	m_colorProfile->SetProfileFoundInFile( false );
}

void	MetaData::EnumerateMetaDataPNG( const ImageFile& _image ) {
// 	bool	bGammaWasSpecified = false;
// 	bool	bProfileFound = false;
// 
// 	EnumerateMetaData( _MetaData,
// 		// Read chromaticities
// 		new MetaDataProcessor( "/cHRM", ( object v ) =>
// 		{
// 			BitmapMetadata	ChromaData = v as BitmapMetadata;
// 
// 			Chromaticities	TempChroma = Chromaticities.Empty;
// 			EnumerateMetaData( ChromaData,
// 				new MetaDataProcessor( "/RedX",			( object v2 ) => { TempChroma.R.x = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/RedY",			( object v2 ) => { TempChroma.R.y = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/GreenX",		( object v2 ) => { TempChroma.G.x = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/GreenY",		( object v2 ) => { TempChroma.G.y = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/BlueX",		( object v2 ) => { TempChroma.B.x = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/BlueY",		( object v2 ) => { TempChroma.B.y = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/WhitePointX",	( object v2 ) => { TempChroma.W.x = 0.00001f * (uint) v2; } ),
// 				new MetaDataProcessor( "/WhitePointY",	( object v2 ) => { TempChroma.W.y = 0.00001f * (uint) v2; } )
// 				);
// 
// 			if ( TempChroma.FindRecognizedChromaticity != STANDARD_PROFILE.INVALID )
// 			{	// Assign new chroma values
// 				m_chromaticities = TempChroma;
// 				bProfileFound = true;
// 			}
// 		} ),
// 					
// 		// Read gamma
// 		new MetaDataProcessor( "/gAMA/ImageGamma", ( object v ) => {
// 			m_gammaCurve = GAMMA_CURVE.STANDARD; m_gamma = 1.0f / (0.00001f * (uint) v); bGammaWasSpecified = true;
// 		} ),
// 
// 		// Read explicit sRGB
// 		new MetaDataProcessor( "/sRGB/RenderingIntent", ( object v ) => {
// 			m_chromaticities = Chromaticities.sRGB; bProfileFound = true; bGammaWasSpecified = false;
// 		} ),
// 
// 		// Read string profile from iTXT
// 		new MetaDataProcessor( "/iTXt/TextEntry", ( object v ) =>
// 		{
// 			if ( bProfileFound )
// 				return;	// No need...
// 
// 			// Hack content !
// 			string	XMLContent = v as string;
// 						
// 			string	ICCProfile = FindAttribute( XMLContent, "photoshop:ICCProfile" );
// 			if ( ICCProfile != null && (bProfileFound = HandleICCProfileString( ICCProfile )) )
// 				return;
// 
// 			string	ColorSpace = FindAttribute( XMLContent, "exif:ColorSpace" );
// 			if ( ColorSpace != null )
// 				bProfileFound = HandleEXIFColorSpace( ColorSpace );
// 		} )
// 		);
// 
// 	_bGammaWasSpecified = bGammaWasSpecified;
// 	_bProfileFound = bProfileFound;

	m_colorProfile = new ColorProfile(	ColorProfile::Chromaticities::sRGB,		// Default for PNGs is standard sRGB	
										ColorProfile::GAMMA_CURVE::sRGB,
										ColorProfile::GAMMA_EXPONENT_sRGB
									);
	m_colorProfile->SetProfileFoundInFile( false );
}

void	MetaData::EnumerateMetaDataTIFF( const ImageFile& _image ) {
// 	bool	bGammaWasSpecified = false;
// 	bool	bProfileFound = false;
// 
// 	EnumerateMetaData( _MetaData,
// 			// Read Photometric Interpretation
// 			new MetaDataProcessor( "/ifd", ( object v ) =>
// 				{
// 					bGammaWasSpecified = FindPhotometricInterpretation( v as BitmapMetadata, "/{ushort=262}" );
// 				} ),
// 
// 			// Read WhitePoint
// 			new MetaDataProcessor( "/ifd/{ushort=318}", ( object v ) =>
// 			{
// 				bProfileFound = true;
// 				throw new Exception( "TODO: Handle TIFF tag 0x13E !" );
// 				// White point ! Encoded as 2 "RATIONALS"
// 			} ),
// 
// 			// Read Chromaticities
// 			new MetaDataProcessor( "/ifd/{ushort=319}", ( object v ) =>
// 			{
// 				bProfileFound = true;
// 				throw new Exception( "TODO: Handle TIFF tag 0x13F !" );
// 				// Chromaticities ! Encoded as 6 "RATIONALS"
// 			} ),
// 
// 			// Read generic data
// 			new MetaDataProcessor( "/ifd/{ushort=700}", ( object _SubData ) =>
// 			{
// 				if ( bProfileFound )
// 					return;	// We already have a valid profile...
// 
// 				BitmapMetadata	SubData = _SubData as BitmapMetadata;
// 
// 				// Try and read a recognized color space
// 				if ( (bProfileFound = FindEXIFColorProfile( SubData )) )
// 					return;	// No need to go hacker-style !
// 
//  				// Ok ! So we got nothing so far... Let's look for the ICCProfile line that Photoshop puts out...
// 				bProfileFound = FindICCProfileString( SubData, ref bGammaWasSpecified );
// 			} )
// 		);
// 
// 	_bGammaWasSpecified = bGammaWasSpecified;
// 	_bProfileFound = bProfileFound;

	m_colorProfile = new ColorProfile(	ColorProfile::Chromaticities::sRGB,		// Default for TIFFs is sRGB
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f									// Linear gamma by default
									);
	m_colorProfile->SetProfileFoundInFile( false );
}

void	MetaData::EnumerateMetaDataRAW( const ImageFile& _image ) {


// 			case FILE_TYPE.CRW:
// 			case FILE_TYPE.CR2:
// 			case FILE_TYPE.DNG:
// 				{
// 					using ( System::IO::MemoryStream Stream = new System::IO::MemoryStream( _ImageFileContent ) )
// 						using ( LibRawManaged.RawFile Raw = new LibRawManaged.RawFile() ) {
// 							Raw.UnpackRAW( Stream );
// 
// 							ColorProfile.Chromaticities	Chroma = Raw.ColorProfile == LibRawManaged.RawFile.COLOR_PROFILE.ADOBE_RGB
// 																? ColorProfile.Chromaticities.AdobeRGB_D65	// Use Adobe RGB
// 																: ColorProfile.Chromaticities.sRGB;			// Use default sRGB color profile
// 
// 							// Create a default sRGB linear color profile
// 							m_colorProfile = _ProfileOverride != nullptr ? _ProfileOverride
// 								: new ColorProfile(
// 									Chroma,
// 									ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
// 									1.0f								// Linear
// 								);
// 
// 							// Also get back valid camera shot info
// 							m_hasValidShotInfo = true;
// 							m_ISOSpeed = Raw.ISOSpeed;
// 							m_Tv = Raw.ShutterSpeed;
// 							m_aperture = Raw.Aperture;
// 							m_focalLength = Raw.FocalLength;



	m_colorProfile = new ColorProfile(	ColorProfile::Chromaticities::sRGB,
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f
									);
	m_colorProfile->SetProfileFoundInFile( false );
}

void	MetaData::EnumerateMetaDataBMP( const ImageFile& _image ) {
	m_colorProfile = new ColorProfile(	ColorProfile::Chromaticities::sRGB,		// Default for BMPs is standard sRGB with no gamma
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f
									);
	m_colorProfile->SetProfileFoundInFile( false );
}

void	MetaData::EnumerateMetaDataGIF( const ImageFile& _image ) {
	m_colorProfile = new ColorProfile(	ColorProfile::Chromaticities::sRGB,		// Default for GIFs is standard sRGB with no gamma
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f
									);
	m_colorProfile->SetProfileFoundInFile( false );
}

#pragma endregion 

//////////////////////////////////////////////////////////////////////////
// Tag reading
void	MetaData::EnumerateDefaultTags( const ImageFile& _image ) {

	m_gammaSpecifiedInFile = GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "Gamma", m_gammaExponent );

	//////////////////////////////////////////////////////////////////////////
	// Attempt to read these standard EXIF tags
	//	{  0x8827, (char *) "ISOSpeedRatings", (char *) "ISO speed rating"},
	//	{  0x9201, (char *) "ShutterSpeedValue", (char *) "Shutter speed"},
	//	{  0x9202, (char *) "ApertureValue", (char *) "Aperture"},
	//	{  0x9203, (char *) "BrightnessValue", (char *) "Brightness"},
	//	{  0x9204, (char *) "ExposureBiasValue", (char *) "Exposure bias"},
	//	{  0x920A, (char *) "FocalLength", (char *) "Lens focal length"},
	int	validTagsCount = 0;
	S32	temp;
	if ( GetInteger( FIMD_EXIF_EXIF, *_image.m_bitmap, "ISOSpeedRatings", temp ) ) {
		temp = temp & 0xFFFF;
		m_ISOSpeed = temp < 50 ? temp * 200 : temp;
		validTagsCount++;
	}
	if ( GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "ExposureTime", m_exposureTime ) ) {
		validTagsCount++;
	}
	if ( GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "ShutterSpeedValue", m_Tv ) ) {
		validTagsCount++;
	}
	if ( GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "ApertureValue", m_Av ) ) {
		validTagsCount++;
	}
// 	if ( GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "ExposureBiasValue", m_exposureBias ) ) {
// 		validTagsCount++;
// 	}
	if ( GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "FNumber", m_FNumber ) ) {
		validTagsCount++;
	}
	if ( GetRational64( FIMD_EXIF_EXIF, *_image.m_bitmap, "FocalLength", m_focalLength ) ) {
		validTagsCount++;
	}

	m_valid = validTagsCount == 6;
}

bool	MetaData::GetString( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, const char*& _value ) {
	FITAG*	tag = nullptr;
	if ( !FreeImage_GetMetadata( _model, &_bitmap, _keyName, &tag ) )
		return false;
	if ( tag == NULL )
		return false;	// Not found...

	_value = (const char*) FreeImage_GetTagValue( tag );

	return true;
}

bool	MetaData::GetInteger( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, S32& _value ) {
	FITAG*	tag = nullptr;
	if ( !FreeImage_GetMetadata( _model, &_bitmap, _keyName, &tag ) )
		return false;
	if ( tag == NULL )
		return false;	// Not found...

	S32*	pvalue = (S32*) FreeImage_GetTagValue( tag );
	_value = *pvalue;

	return true;
}

bool	MetaData::GetRational64( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, S32& _numerator, S32& _denominator ) {
	FITAG*	tag = nullptr;
	if ( !FreeImage_GetMetadata( _model, &_bitmap, _keyName, &tag ) )
		return false;
	if ( tag == NULL )
		return false;	// Not found...

	switch ( FreeImage_GetTagType( tag ) ) {
		case FIDT_RATIONAL: {
			// 64-bit unsigned fraction 
			U32*	pvalue = (U32*) FreeImage_GetTagValue( tag );
			_numerator = S32( pvalue[0] );
			_denominator = S32( pvalue[1] );
			break;
		}

		case FIDT_SRATIONAL: {
			// 64-bit signed fraction 
			S32*	pvalue = (S32*) FreeImage_GetTagValue( tag );
			_numerator = pvalue[0];
			_denominator = pvalue[1];
			break;
		}

		default:
			throw "Unexpected tag data type!";
	}

	return true;
}
bool	MetaData::GetRational64( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, float& _value ) {
	S32 num, den;
	if ( !GetRational64( _model, _bitmap, _keyName, num, den ) )
		return false;

	_value = float(num) / den;

	return true;
}
/*

	protected:

// 		// Attempts to find the TIFF "PhotometricInterpretation" metadata
// 		// <param name="_Meta"></param>
// 		// <param name="_MetaPath"></param>
// 		// <returns>True if gamma was specified</returns>
// 		bool	FindPhotometricInterpretation( BitmapMetadata _Meta, string _MetaPath );
// 
// 
// 		// Attempts to find the color profile in the EXIF metadata
// 		// <param name="_Meta"></param>
// 		// <returns>True if the profile was successfully found</returns>
// 		bool	FindEXIFColorProfile( BitmapMetadata _Meta );
// 		
// 
// 		// Attempts to find the "photoshop:ICCProfile" string in the metadata dump and retrieve a known profile from it
// 		// <param name="_Meta"></param>
// 		// <param name="_bGammaWasSpecified"></param>
// 		// <returns>True if the profile was successfully found</returns>
// 		bool	FindICCProfileString( BitmapMetadata _Meta, bool& _gammaWasSpecified );
// 
// 		bool	HandleEXIFColorSpace( string _ColorSpace );
// 
// 
// 		// Attempts to handle a color profile from the EXIF ColorSpace tag
// 		// <param name="_ColorSpace"></param>
// 		// <returns>True if the profile was recognized</returns>
// 		bool	HandleEXIFColorSpace( int _ColorSpace );
// 
// 
// 		// Attempts to handle an ICC profile by name
// 		// <param name="_ProfilName"></param>
// 		// <returns>True if the profile was recognized</returns>
// 		bool	HandleICCProfileString( string _ProfilName );
// 
// 
// 		// Attempts to find an XML attribute by name
// 		// <param name="_XMLContent"></param>
// 		// <param name="_AttributeName"></param>
// 		// <returns></returns>
// 		string	FindAttribute( string _XMLContent, string _AttributeName );

	protected:

// 		#pragma region Enumeration Tools
// 
// //		[System.Diagnostics.DebuggerDisplay( "Path={Path}" )]
// 		class		MetaDataProcessor {
// 			public string			Path;
// 			public Action<object>	Process;
// 			public MetaDataProcessor( string _Path, Action<object> _Process )	{ Path = _Path; Process = _Process; }
// 		};
// 
// 		void	EnumerateMetaData( BitmapMetadata _Root, params MetaDataProcessor[] _Processors );
// 		string	DumpMetaData( BitmapMetadata _Root );
// 		string	DumpMetaData( BitmapMetadata _Root, string _Tab );
// 
// 		#pragma endregion

		#pragma endregion

bool	MetaData::FindPhotometricInterpretation( BitmapMetadata _Meta, string _MetaPath ) {
	bool	gammaWasSpecified = false;
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
					m_gammaCurve = GAMMA_CURVE.STANDARD;
					m_gamma = 1.0f;
					gammaWasSpecified = true;
					break;

				case 2:	// NTSC RGB
					m_gammaCurve = GAMMA_CURVE.STANDARD;
					m_gamma = 2.2f;
					gammaWasSpecified = true;
					break;

				default:
					// According to the spec (page 117), a value of 6 is a YCrCb image while a value of 8 is a L*a*b* image
					// SHould we handle this in case of ???
					throw new Exception( "TODO: Handle TIFF special photometric interpretation !" );
			}
		} ) );

	return gammaWasSpecified;
}

bool	MetaData::FindEXIFColorProfile( BitmapMetadata _Meta ) {
	bool	profileFound = false;
	EnumerateMetaData( _Meta,
		new MetaDataProcessor( "/exif:ColorSpace", ( object v ) =>
			{
				profileFound = HandleEXIFColorSpace( v as string );
			} )
		);

	return profileFound;
}

	
/// Attempts to find the "photoshop:ICCProfile" string in the metadata dump and retrieve a known profile from it
/// <param name="_Meta"></param>
/// <param name="_bGammaWasSpecified"></param>
/// <returns>True if the profile was successfully found</returns>
bool	FindICCProfileString( BitmapMetadata _Meta, bool& _gammaWasSpecified ) {
	bool	profileFound = false;
	bool	bGammaWasSpecified = _gammaWasSpecified;
	EnumerateMetaData( _Meta,
		new MetaDataProcessor( "/photoshop:ICCProfile", ( object v ) =>
			{
				if ( HandleICCProfileString( v as string ) )
					bGammaWasSpecified = false;	// Assume profile, complete with gamma
			} ) );

	_gammaWasSpecified = bGammaWasSpecified;
	return profileFound;
}

bool	HandleEXIFColorSpace( string _ColorSpace ) {
	int	Value = -1;
	return int.TryParse( _ColorSpace, out Value ) ? HandleEXIFColorSpace( Value ) : false;
}

/// <summary>
/// Attempts to handle a color profile from the EXIF ColorSpace tag
/// </summary>
/// <param name="_ColorSpace"></param>
/// <returns>True if the profile was recognized</returns>
bool	HandleEXIFColorSpace( int _ColorSpace ) {
	switch ( _ColorSpace ) {
		case 1:
			m_chromaticities = Chromaticities.sRGB;			// This is definitely sRGB
			return true;									// We now know the profile !

		case 2:
			m_chromaticities = Chromaticities.AdobeRGB_D65;	// This is not official but sometimes it's AdobeRGB
			return true;									// We now know the profile !
	}

	return false;
}

/// <summary>
/// Attempts to handle an ICC profile by name
/// </summary>
/// <param name="_ProfilName"></param>
/// <returns>True if the profile was recognized</returns>
bool	HandleICCProfileString( string _ProfilName ) {
	if ( _ProfilName.IndexOf( "sRGB IEC61966-2.1" ) != -1 ) {
		m_chromaticities = Chromaticities.sRGB;
		return true;
	} else if ( _ProfilName.IndexOf( "Adobe RGB (1998)" ) != -1 ) {
		m_chromaticities = Chromaticities.AdobeRGB_D65;
		return true;
	} else if ( _ProfilName.IndexOf( "ProPhoto" ) != -1 ) {
		m_chromaticities = Chromaticities.ProPhoto;
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
string	FindAttribute( string _XMLContent, string _AttributeName ) {
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

protected:

#pragma region Enumeration Tools

//		[System.Diagnostics.DebuggerDisplay( "Path={Path}" )]
class		MetaDataProcessor {
	public string			Path;
	public Action<object>	Process;
	public MetaDataProcessor( string _Path, Action<object> _Process )	{ Path = _Path; Process = _Process; }
};

void	EnumerateMetaData( BitmapMetadata _Root, params MetaDataProcessor[] _Processors ) {
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

string	DumpMetaData( BitmapMetadata _Root ) {
	return DumpMetaData( _Root, "" );
}
string	DumpMetaData( BitmapMetadata _Root, string _Tab ) {
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

#pragma endregion
*/
