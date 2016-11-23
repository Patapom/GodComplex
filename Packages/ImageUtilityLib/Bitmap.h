//////////////////////////////////////////////////////////////////////////
// This special Bitmap class carefully handles color profiles to provide a faithful internal image representation that
//	is always stored as 32-bits floating point precision CIE XYZ device-independent format that you can later convert
//	to any other format.
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

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
		#pragma region FIELDS

		U32				m_width;
		U32				m_height;

		bfloat4*		m_XYZ;				// CIEXYZ Bitmap content + Alpha

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
		bfloat4*		GetContentXYZ()			{ return m_XYZ; }
		const bfloat4*	GetContentXYZ() const	{ return m_XYZ; }

		#pragma endregion


static ImageFile*	ms_DEBUG;


	public:

		#pragma region METHODS

		Bitmap()
			: m_width( 0 )
			, m_height( 0 )
			, m_XYZ( nullptr ) {
		}

		~Bitmap() {
			Exit();
		}

		// Manual creation
		//	_profile, an optional color profile (NOTE: you will need a valid profile if you wish to save the bitmap)
		Bitmap( int _width, int _height ) : m_XYZ( nullptr ) {
			Init( _width, _height );
		}

		// Creates a bitmap from a file
		Bitmap( const ImageFile& _file ) : m_XYZ( nullptr ) {
			FromImageFile( _file );
		}

		// Initializes with appropriate dimensions
		void			Init( int _width, int _height );

		// Initializes the bitmap from an image file
		void			FromImageFile( const ImageFile& _sourceFile, ColorProfile* _profileOverride=nullptr, bool _unPremultiplyAlpha=false );

		// Builds an RGBA32F image file from the bitmap that you can later tone map
		void			ToImageFile( ImageFile& _targetFile, ColorProfile* _profileOverride=nullptr, bool _premultiplyAlpha=false ) const;

		void			Exit();

		// Accesses the individual XYZ-Alpha pixels
		bfloat4&		Access( U32 _X, U32 _Y ) {
			return m_XYZ[m_width*_Y+_X];
		}
		const bfloat4&	Access( U32 _X, U32 _Y ) const {
			return m_XYZ[m_width*_Y+_X];
		}

		/// <summary>
		/// Performs bilinear sampling of the XYZ content using CLAMP addressing
		/// </summary>
		/// <param name="X">A column index in [0,Width[ (will be clamped if out of range)</param>
		/// <param name="Y">A row index in [0,Height[ (will be clamped if out of range)</param>
		/// <returns>The XYZ at the requested location</returns>
		void			BilinearSample( float X, float Y, bfloat4& _XYZ ) const;


	public:
		//////////////////////////////////////////////////////////////////////////
		// HDR Helpers
		//
		// This section follows the algorithm provided by "Recovering HDR Radiance Maps from Photographs", Debevec (1997)
		// As advised by the author, you should:
		//	• Always capture LDR images using apertures of f/8 or higher to avoid a possible lense variance between radiance and irradiance (vignetting)
		//
		class HDRParms {
		public:
			// The amount of bits per (R,G,B) component the camera is able to output
			// Usually, for RAW input images are either 12- or 16-bits depending on model while non-RAW outputs (e.g. JPG or PNG) are simply 8-bits
			U32		_inputBitsPerComponent;

			//	The default luminance factor to apply to all the images
			//	(allows you to scale the base luminance if you know the absolute value)
			float	_luminanceFactor;

			// The curve smoothness constraint used to enforce the smoothness of the response curve
			// A value of 0 doesn't constrain at all while a value of 1 makes sure the response curve is smooth
			float	_curveSmoothnessConstraint;

			// The "subjective quality" parameter used for the algorithm that guides how many pixels are going to be used to compute the response curve
			// The default value is 1 so an average number of pixels is used
			// Using a quality of 2 will use twice as many pixels, increasing response curve quality and computation time
			// Using a quality of 0.5 will use half as many pixels, decreasing response curve quality and computation time
			float	_quality;

			// If true, the Camera Response Curve is fit against a polynomial curve and replaced by its smooth version
			// If false, the raw response curve is returned (with noise and such)
			bool	_performResponseCurveFitting;

			HDRParms()
				: _inputBitsPerComponent( 8 )		// default = 8 for JPEG, 12 for RAW;
				, _luminanceFactor( 1.0f )
				, _curveSmoothnessConstraint( 1.0f )
				, _quality( 1.0f )
				, _performResponseCurveFitting( true ) {
			}
		};

		// Builds a HDR image from a set of LDR images
		//	_images, the array of LDR bitmaps
		//	_imageShutterSpeeds, the array of shutter speeds (in seconds) used for each image
		void		LDR2HDR( U32 _imagesCount, const ImageFile** _images, const float* _imageShutterSpeeds, const HDRParms& _parms );

		// Builds a HDR image from a set of LDR images and a response curve (usually computed using ComputeHDRResponseCurve)
		// You can use this method to build the HDR image from a larger set of LDR images than used to resolve the response curve
		//	_images, the array of LDR bitmaps
		//	_imageShutterSpeeds, the array of shutter speeds (in seconds) used for each image
		//	_responseCurve, the list of values corresponding to the response curve
		//	The default luminance factor to apply to all the images (allows you to scale the base luminance if you know the absolute value)
		void		LDR2HDR( U32 _imagesCount, const ImageFile** _images, const float* _imageShutterSpeeds, const BaseLib::List< bfloat3 >& _responseCurve, float _luminanceFactor );

		// Computes the response curve of the sensor that captured the provided LDR images
		//	_images, the array of LDR bitmaps
		//	_imageShutterSpeeds, the array of shutter speeds (in seconds) used for each image
		//	_responseCurve, the list to fill with values corresponding to the response curve
		//	_luminanceOnly, if true then the luminance of the pixels is used and only a single response curve is computed instead of 3 individual curves for R,G and B
		static void	ComputeCameraResponseCurve( U32 _imagesCount, const ImageFile** _images, const float* _imageShutterSpeeds, const HDRParms& _parms, BaseLib::List< bfloat3 >& _responseCurve, bool _luminanceOnly=true );

	private:

		static void	PerformCurveFitting( BaseLib::List< bfloat3 >& _responseCurve, U32 _componentsCount );

		#pragma endregion
	};
}
