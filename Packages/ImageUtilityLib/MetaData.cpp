#include "MetaData.h"
#include <string>

using namespace ImageUtilityLib;

MetaData::MetaData()
	: m_valid( false )
	, m_ISOSpeed( -1.0f )
	, m_shutterSpeed( -1.0f )
	, m_aperture( -1.0f )
	, m_focalLength( -1.0f ) {

}

void	MetaData::Reset() {
	m_valid = false;
}

bool	MetaData::GetInteger( FREE_IMAGE_MDMODEL _model, const FIBITMAP& _bitmap, const char* _keyName, int& _value ) {
	FITAG*	tag;
	FreeImage_GetMetadata( FIMD_COMMENTS, const_cast< FIBITMAP* >( &_bitmap ), _keyName, &tag );	// const_casting, hoping it really doesn't touch the bitmap!
	if ( tag == NULL )
		return false;	// Not found...

	const char*	value = (const char*) FreeImage_GetTagValue( tag );
	int	foundFieldsCount = sscanf_s( value, "%d", &_value );
	return foundFieldsCount == 1;
}
bool	MetaData::GetFloat( FREE_IMAGE_MDMODEL _model, const FIBITMAP& _bitmap, const char* _keyName, float& _value ) {
	FITAG*	tag;
	FreeImage_GetMetadata( FIMD_COMMENTS, const_cast< FIBITMAP* >( &_bitmap ), _keyName, &tag );	// const_casting, hoping it really doesn't touch the bitmap!
	if ( tag == NULL )
		return false;	// Not found...

	const char*	value = (const char*) FreeImage_GetTagValue( tag );
	int	foundFieldsCount = sscanf_s( value, "%f", &_value );
	return foundFieldsCount == 1;
}

/*
void	MetaData::EnumerateMetaDataJPG( ImageFile& _image, bool& _profileFound, bool& _gammaWasSpecified ) {
	bool	gammaWasSpecified = false;
	bool	profileFound = false;

	EnumerateMetaData( _MetaData,
		new MetaDataProcessor( "/xmp", ( object _SubData ) =>
		{
			BitmapMetadata	SubData = _SubData as BitmapMetadata;

			// Retrieve gamma ramp
			gammaWasSpecified = FindPhotometricInterpretation( SubData, "/tiff:PhotometricInterpretation" );

 			// Let's look for the ICCProfile line that Photoshop puts out...
			if ( profileFound = FindICCProfileString( SubData, ref gammaWasSpecified ) )
				return;

			// Ok ! So we got nothing so far... Try and read a recognized color space
			profileFound = FindEXIFColorProfile( SubData );
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

	_gammaWasSpecified = gammaWasSpecified;
	_profileFound = profileFound;
}

void	MetaData::EnumerateMetaDataPNG( BitmapMetadata _MetaData, bool& _profileFound, bool& _gammaWasSpecified ) {
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

			if ( TempChroma.FindRecognizedChromaticity != STANDARD_PROFILE.INVALID )
			{	// Assign new chroma values
				m_chromaticities = TempChroma;
				bProfileFound = true;
			}
		} ),
					
		// Read gamma
		new MetaDataProcessor( "/gAMA/ImageGamma", ( object v ) => {
			m_gammaCurve = GAMMA_CURVE.STANDARD; m_gamma = 1.0f / (0.00001f * (uint) v); bGammaWasSpecified = true;
		} ),

		// Read explicit sRGB
		new MetaDataProcessor( "/sRGB/RenderingIntent", ( object v ) => {
			m_chromaticities = Chromaticities.sRGB; bProfileFound = true; bGammaWasSpecified = false;
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

void	MetaData::EnumerateMetaDataTIFF( BitmapMetadata _MetaData, bool& _profileFound, bool& _gammaWasSpecified ) {
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

	
/// <summary>
/// Attempts to find the "photoshop:ICCProfile" string in the metadata dump and retrieve a known profile from it
/// </summary>
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
	if ( _ProfilName.IndexOf( "sRGB IEC61966-2.1" ) != -1 )
	{
		m_chromaticities = Chromaticities.sRGB;
		return true;
	}
	else if ( _ProfilName.IndexOf( "Adobe RGB (1998)" ) != -1 )
	{
		m_chromaticities = Chromaticities.AdobeRGB_D65;
		return true;
	}
	else if ( _ProfilName.IndexOf( "ProPhoto" ) != -1 )
	{
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
