//////////////////////////////////////////////////////////////////////////
// This special Bitmap class carefully handles color profiles to provide a faithful internal image representation that
//	is always stored as 32-bits floating point precision CIE XYZ device-independent format that you can later convert
//	to any other format.
//
//	@TODO: Avoir la possibilité de créer une texture avec un seul channel du bitmap !? (filtrage)
//	@TODO: Handle "premultiplied alpha"
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#include "Types.h"
#include "ColorProfile.h"
#include "ImageFile.h"

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
	/// </summary>
	/// <remarks>The Bitmap class has been tested with various formats, various bit depths and color profiles all created from Adobe Photoshop CS4 using
	/// the "Save As" dialog and the "Save for Web & Devices" dialog box.
	/// 
	/// In a general manner, you should NOT use the latter save option but rather select your working color profile from the "Edit > Color Settings" menu,
	///  then save your files and make sure you tick the "ICC Profile" checkbox using the DEFAULT save file dialog box to embed that profile in the image.
	/// </remarks>
	class Bitmap {
	private:
		#pragma region CONSTANTS

//		private static readonly System.Windows.Media.PixelFormat	GENERIC_PIXEL_FORMAT = System.Windows.Media.PixelFormats.Rgba128Float;
// 		static const float	BYTE_TO_FLOAT = 1.0f / 255.0f;
// 		static const float	WORD_TO_FLOAT = 1.0f / 65535.0f;

		#pragma endregion

	private:
		#pragma region FIELDS

		U32					m_width;
		U32					m_height;
//		bool				m_hasAlpha;

		const ColorProfile*	m_colorProfile;		// The color profile to use with this bitmap (NOTE: Not owned by the class, it's the responsibility of the provider to delete the profile)

		float4*				m_XYZ;				// CIEXYZ Bitmap content + Alpha

		#pragma endregion

	public:
		#pragma region PROPERTIES

		/// <summary>
		/// Gets the image width
		/// </summary>
		U32				Width() const	{ return m_width; }

		/// <summary>
		/// Gets the image height
		/// </summary>
		U32				Height() const	{ return m_height; }

		/// <summary>
		/// Gets the image content stored as CIEXYZ + Alpha
		/// </summary>
		float4*			GetContentXYZ()			{ return m_XYZ; }
		const float4*	GetContentXYZ() const	{ return m_XYZ; }

		/// <summary>
		/// Gets or sets the image's color profile
		/// </summary>
		const ColorProfile&	GetProfile() const						{ return *m_colorProfile; }
		void				SetProfile( const ColorProfile& value )	{ m_colorProfile = &value; }

		#pragma endregion

	public:

		#pragma region METHODS

		Bitmap()
			: m_width( 0 )
			, m_height( 0 )
//			, m_hasAlpha( false )
			, m_colorProfile( nullptr )
			, m_XYZ( nullptr ) {
		}

		~Bitmap() {
			SAFE_DELETE( m_XYZ );
		}

		/// <summary>
		/// Manual creation
		/// </summary>
		/// <param name="_Width"></param>
		/// <param name="_Height"></param>
		/// <param name="_Profile">An optional color profile, you will need a valid profile if you wish to save the bitmap!</param>
		Bitmap( int _width, int _height, const ColorProfile& _profile ) {
			m_width = _width;
			m_height = _height;
			m_XYZ = new float4[m_width * m_height];
			memset( m_XYZ, 0, m_width*m_height*sizeof(float4) );
			m_colorProfile = &_profile;
		}

		/// <summary>
		/// Creates a bitmap from a file
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		Bitmap( const ImageFile& _file ) {
			FromImageFile( _file );
		}

		// Initializes the bitmap from an image file
		void	FromImageFile( const ImageFile& _sourceFile ) {
			FromImageFile( _sourceFile, nullptr );
		}
		void	FromImageFile( const ImageFile& _sourceFile, const ColorProfile* _profileOverride );

		// Builds an image file from the bitmap
		void	ToImageFile( ImageFile& _targetFile, ImageFile::PIXEL_FORMAT _targetFormat ) const;

		// Accesses the 
		float4&	Access( U32 _X, U32 _Y ) {
			return m_XYZ[m_width*_Y+_X];
		}
		const float4&	Access( U32 _X, U32 _Y ) const {
			return m_XYZ[m_width*_Y+_X];
		}

		/// <summary>
		/// Performs bilinear sampling of the XYZ content using CLAMP addressing
		/// </summary>
		/// <param name="X">A column index in [0,Width[ (will be clamped if out of range)</param>
		/// <param name="Y">A row index in [0,Height[ (will be clamped if out of range)</param>
		/// <returns>The XYZ at the requested location</returns>
		void	BilinearSample( float X, float Y, float4& _XYZ ) const {
			int		X0 = (int) floorf( X );
			int		Y0 = (int) floorf( Y );
			float	x = X - X0;
			float	y = Y - Y0;
			float	rx = 1.0f - x;
			float	ry = 1.0f - y;
					X0 = CLAMP( X0, 0, S32(m_width-1) );
					Y0 = CLAMP( Y0, 0, S32(m_height-1) );
			int		X1 = MIN( X0+1, S32(m_width-1) );
			int		Y1 = MIN( Y0+1, S32(m_height-1) );

			const float4&	V00 = Access( X0, Y0 );
			const float4&	V01 = Access( X1, Y0 );
			const float4&	V10 = Access( X0, Y1 );
			const float4&	V11 = Access( X1, Y1 );

			float4	V0 = rx * V00 + x * V01;
			float4	V1 = rx * V10 + x * V11;

			_XYZ = ry * V0 + y * V1;
		}

		#pragma endregion
	};
}