//////////////////////////////////////////////////
//
///////////////////////////////////////////////////
//
#pragma once

#include "Types.h"
#include "FreeImage.h"

namespace ImageUtilityLib {

	class ImageFile;

	class	MetaData {
	private:
		bool			m_valid;

	public:

		float			m_ISOSpeed;
		float			m_shutterSpeed;
		float			m_aperture;
		float			m_focalLength;

	public:

		bool			IsValid() const { return m_valid; }

	public:
		MetaData();

		void	Reset();

	public:

		void	EnumerateMetaDataJPG( ImageFile& _image, bool& _profileFound, bool& _gammaWasSpecified );
		void	EnumerateMetaDataPNG( ImageFile& _image, bool& _profileFound, bool& _gammaWasSpecified );
		void	EnumerateMetaDataTIFF( ImageFile& _image, bool& _profileFound, bool& _gammaWasSpecified );

	public:	// HELPERS

		// Wraps FreeImage's metadata tags getters
		static bool	GetInteger( FREE_IMAGE_MDMODEL _model, const FIBITMAP& _bitmap, const char* _keyName, int& _value );
		static bool	GetFloat( FREE_IMAGE_MDMODEL _model, const FIBITMAP& _bitmap, const char* _keyName, float& _value );

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
	};
}