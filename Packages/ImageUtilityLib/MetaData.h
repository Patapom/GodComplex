//////////////////////////////////////////////////
// Contains relevant metadata (e.g. ISO, Tv, Av, focal length, etc.)
///////////////////////////////////////////////////
//
#pragma once

#include "ColorProfile.h"

namespace ImageUtilityLib {

	class ImageFile;
	class ColorProfile;

	class	MetaData {
	public:

		ColorProfile*		m_colorProfile;				// The color profile found in the input file if the bitmap was loaded from a file, or the default sRGB profile otherwise

		bool				m_gammaSpecifiedInFile;		// True if the gamma exponent was found in the file

		bool				m_valid;					// True if the following information was found in the file (usually not available except from RAW files)
		float				m_ISOSpeed;
		float				m_shutterSpeed;
		float				m_aperture;
		float				m_exposureBias;
		float				m_focalLength;

	public:
		MetaData();
		MetaData( const MetaData& _other );
		~MetaData();

		void	Reset();
		void	RetrieveFromImage( const ImageFile& _imageFile );

		MetaData&	operator=( const MetaData& _other );

	public:

		void	EnumerateMetaDataPNG( const ImageFile& _image );
		void	EnumerateMetaDataJPG( const ImageFile& _image );
		void	EnumerateMetaDataTGA( const ImageFile& _image );
		void	EnumerateMetaDataTIFF( const ImageFile& _image );
		void	EnumerateMetaDataBMP( const ImageFile& _image );
		void	EnumerateMetaDataGIF( const ImageFile& _image );
		void	EnumerateMetaDataRAW( const ImageFile& _image );

	public:	// HELPERS

		void	EnumerateDefaultTags( const ImageFile& _image );

		// Wraps FreeImage's metadata tags getters
		static bool	GetInteger( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, int& _value );
		static bool	GetFloat( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, float& _value );
	};
}