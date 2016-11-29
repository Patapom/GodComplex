//////////////////////////////////////////////////
// Contains relevant metadata (e.g. ISO, Tv, Av, focal length, etc.)
///////////////////////////////////////////////////
//
#pragma once

#include "ColorProfile.h"

namespace ImageUtilityLib {

	class ImageFile;
	class ColorProfile;

	// Holds the image's color profile as well as important shot information pulled from EXIF data
	// You may want to have a look at APEX format to understand Tv and Av settings (https://en.wikipedia.org/wiki/APEX_system)
	// NOTE: The color profile is never NULL and is assigned to the default sRGB profile if left unspecified
	//
	class	MetaData {
	public:
		template< typename T > class Field {
		public:
			bool		m_isValid;
			T			m_value;

			Field() : m_isValid( false ) {}
			void		Clear( T _defaultValue ) {
				m_isValid = false;
				m_value = _defaultValue;
			}
			Field<T>&	operator=( const T& _value ) {
				m_isValid = true;
				m_value = _value;
				return *this;
			}
						operator T() const { return m_value; }
		};

	private:

		ColorProfile*		m_colorProfile;				// The color profile found in the input file if the bitmap was loaded from a file, or the default profile corresponding to the image type otherwise

	public:

 		bool				m_gammaSpecifiedInFile;		// True if the gamma exponent was found in the file

		Field<U32>			m_ISOSpeed;					// ISO speed (min = 50)
		Field<float>		m_exposureTime;				// Exposure time (in seconds)
		Field<float>		m_Tv;						// Shutter Speed Value, in EV (Tv = log2( 1/ShutterSpeed))
		Field<float>		m_Av;						// Aperture Value, in EV (Av = log2( Aperture² ))
		Field<float>		m_FNumber;					// In F-stops
		Field<float>		m_focalLength;				// In mm

	public:

		// Gets or sets the color profile
		ColorProfile&			GetColorProfile()		{ return *m_colorProfile; }
		const ColorProfile&		GetColorProfile() const	{ return *m_colorProfile; }
		void					SetColorProfile( const ColorProfile& value ) {
			SAFE_DELETE( m_colorProfile );				// We always have the responsibility of our profile
			m_colorProfile = new ColorProfile( value );	// ...because we always make a deep copy out of what we're given!
		}

	public:
		MetaData();
		MetaData( const MetaData& _other );
		~MetaData();

		void	Reset();
		void	RetrieveFromImage( const ImageFile& _imageFile );

		MetaData&	operator=( const MetaData& _other );

	public:	// HELPERS

		// _gammaExponent, the gamma exponent found in the file or the default 2.2 exponent otherwise
		void	EnumerateDefaultTags( const ImageFile& _image, float& _gammaExponent );

		// Wraps FreeImage's metadata tags getters
 		static bool	GetString( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, const char*& _value );
 		static bool	GetInteger( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, S32& _value );
		static bool	GetRational64( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, float& _value );
		static bool	GetRational64( FREE_IMAGE_MDMODEL _model, FIBITMAP& _bitmap, const char* _keyName, S32& _numerator, S32& _denominator );

	private:

		void	EnumerateMetaDataPNG( const ImageFile& _image, float _gammaExponent );
		void	EnumerateMetaDataJPG( const ImageFile& _image, float _gammaExponent );
		void	EnumerateMetaDataTGA( const ImageFile& _image, float _gammaExponent );
		void	EnumerateMetaDataTIFF( const ImageFile& _image, float _gammaExponent );
		void	EnumerateMetaDataBMP( const ImageFile& _image, float _gammaExponent );
		void	EnumerateMetaDataGIF( const ImageFile& _image, float _gammaExponent );
		void	EnumerateMetaDataRAW( const ImageFile& _image, float _gammaExponent );
	};
}