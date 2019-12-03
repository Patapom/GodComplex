//////////////////////////////////////////////////////////////////////////
// This special Bitmap class carefully handles color profiles to provide a faithful internal image representation that
//	is always stored as 32-bits floating point precision CIE XYZ device-independent format that you can later convert to any other format.
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#include "ImageFile.h"

using namespace System;
using namespace SharpMath;

namespace ImageUtility {

	/// <summary>
	/// The Bitmap class should be used to replace the standard System.Drawing.Bitmap
	/// The big advantage of the Bitmap class is to accurately read back the color profile and gamma correction data stored in the image's metadata
	/// so that, internally, the image is stored:
	///		� As device-independent CIE XYZ (http://en.wikipedia.org/wiki/CIE_1931_color_space) format, our Profile Connection Space
	///		� In linear space (i.e. no gamma curve is applied)
	///		� NOT pre-multiplied alpha (you can later re-pre-multiply if needed)
	///	
	/// This helps to ensure that whatever the source image format stored on disk, you always deal with a uniformized image internally.
	/// 
	/// Later, you can cast from the CIE XYZ device-independent format into any number of pre-defined texture profiles:
	///		� sRGB or Linear space textures (for 8bits per component images only)
	///		� Compressed (BC1-BC5) or uncompressed (for 8bits per component images only)
	///		� 8-, 16-, 16F- 32- or 32F-bits per component
	///		� Pre-multiplied alpha or not
	/// 
	/// </summary>
	/// <remarks>The Bitmap class has been tested with various formats, various bit depths and color profiles all created from Adobe Photoshop CS4 using
	/// the "Save As" dialog and the "Save for Web & Devices" dialog box.
	/// 
	/// In a general manner, you should NOT use the latter save option but rather select your working color profile from the "Edit > Color Settings" menu,
	///  then save your files and make sure you tick the "ICC Profile" checkbox using the DEFAULT save file dialog box to embed that profile in the image.
	/// </remarks>
	public ref class Bitmap {
	internal:
		ImageUtilityLib::Bitmap*	m_nativeObject;

	public:
		#pragma region PROPERTIES

		/// <summary>
		/// Gets the image width
		/// </summary>
		property UInt32		Width {
			UInt32		get() { return m_nativeObject->Width(); }
		}

		/// <summary>
		/// Gets the image height
		/// </summary>
		property UInt32		Height {
			UInt32		get() { return m_nativeObject->Height(); }
		}

		/// <summary>
		/// Gets the image content stored as CIEXYZ + Alpha
		/// </summary>
		property cli::array< float4, 2 >^	ContentXYZ {
			cli::array< float4, 2 >^	get() {
				const bfloat4*	source = m_nativeObject->GetContentXYZ();
				int				W = m_nativeObject->Width();
				int				H = m_nativeObject->Height();

				cli::array< float4, 2 >^	result = gcnew cli::array< float4, 2 >( W, H );
				for ( int Y=0; Y < H; Y++ )
					for ( int X=0; X < W; X++, source++ ) {
						result[X,Y].Set( source->x, source->y, source->z, source->w );
					}
				return result;
			}
		}

		// Accesses the individual XYZ-Alpha pixels
		property float4	default[UInt32, UInt32] {
			float4		get( UInt32 _X, UInt32 _Y ) {
				const bfloat4&	native = m_nativeObject->Access( _X, _Y );
				return float4( native.x, native.y, native.z, native.w );
			}
			void		set( UInt32 _X, UInt32 _Y, float4 value ) {
				bfloat4&	native = m_nativeObject->Access( _X, _Y );
				native.Set( value.x, value.y, value.z, value.w );
			}
		}

static property ImageFile^	DEBUG {
	ImageFile^	get() { return gcnew ImageFile( *ImageUtilityLib::Bitmap::ms_DEBUG, false ); }
}

		#pragma endregion

	public:

		#pragma region METHODS

		Bitmap() {
			m_nativeObject = new ImageUtilityLib::Bitmap();
		}

		~Bitmap() {
			SAFE_DELETE( m_nativeObject );
		}

		// Manual creation
		Bitmap( UInt32 _width, UInt32 _height ) {
			m_nativeObject = new ImageUtilityLib::Bitmap( _width, _height );
		}

		// Creates a bitmap from a file
		Bitmap( ImageFile^ _file ) {
			FromImageFile( _file );
		}

		void			Init( UInt32 _width, UInt32 _height ) {
			m_nativeObject->Init( _width, _height );
		}

		// Initializes the bitmap from an image file
		void			FromImageFile( ImageFile^ _sourceFile ) {
			FromImageFile( _sourceFile, nullptr );
		}
		void			FromImageFile( ImageFile^ _sourceFile, ColorProfile^ _profileOverride ) {
			FromImageFile( _sourceFile, _profileOverride, false );
		}
		void			FromImageFile( ImageFile^ _sourceFile, ColorProfile^ _profileOverride, bool _unPremultiplyAlpha ) {
			if ( _sourceFile == nullptr )
				throw gcnew Exception( "Invalid source file!" );
			m_nativeObject->FromImageFile( *_sourceFile->m_nativeObject, _profileOverride != nullptr ? _profileOverride->m_nativeObject : nullptr, _unPremultiplyAlpha );
		}

		// Builds an RGBA32F image file from the bitmap that you can later tone map
		void			ToImageFile( ImageFile^ _targetFile, ColorProfile^ _colorProfile ) {
			ToImageFile( _targetFile, _colorProfile, false );
		}
		void			ToImageFile( ImageFile^ _targetFile, ColorProfile^ _colorProfile, bool _premultiplyAlpha ) {
			if ( _targetFile == nullptr )
				throw gcnew Exception( "Invalid target file!" );
			if ( _colorProfile == nullptr )
				throw gcnew Exception( "Invalid color profile!" );
			m_nativeObject->ToImageFile( *_targetFile->m_nativeObject, *_colorProfile->m_nativeObject, _premultiplyAlpha );
		}

		/// <summary>
		/// Performs bilinear sampling of the XYZ content using CLAMP addressing
		/// </summary>
		/// <param name="X">A column index in [0,Width[ (will be clamped if out of range)</param>
		/// <param name="Y">A row index in [0,Height[ (will be clamped if out of range)</param>
		/// <returns>The XYZ at the requested location</returns>
		float4^			BilinearSample( float X, float Y ) {
			bfloat4	XYZ;
			m_nativeObject->BilinearSample( X, Y, XYZ );
			return float4( XYZ.x, XYZ.y, XYZ.z, XYZ.w );
		}

	public:
		//////////////////////////////////////////////////////////////////////////
		// HDR Helpers
		//
		// This section follows the algorithm provided by "Recovering HDR Radiance Maps from Photographs", Debevec (1997)
		// As advised by the author, you should:
		//	� Always capture LDR images using apertures of f/8 or higher to avoid a possible lense variance between radiance and irradiance (vignetting)
		//	� Always use a quite large amount of LDR images to compute the camera response curve (a minimum of 5 is quite okay)
		//	� Use more samples (i.e. the quality settings) in the parameters structure below if you don't have enough images
		//
		enum class FILTER_TYPE {
			NONE,							// No filter
			SMOOTHING_GAUSSIAN,				// Curve smoothing using gaussian filtering
			SMOOTHING_GAUSSIAN_2_PASSES,	// Curve smoothing using 2 passes of gaussian filtering
			SMOOTHING_TENT,					// Curve smoothing using tent filtering
			CURVE_FITTING,					// Curve fitting (warning: extremums are less fit than the center of the curve because the tent filtering of extremums is accounted for during curve fitting)
			GAUSSIAN_PLUS_CURVE_FITTING,	// Gaussian filtering followed by curve fitting (warning: extremums are less fit than the center of the curve because the tent filtering of extremums is accounted for during curve fitting)
		};

		ref class HDRParms {
		public:
			// The amount of bits per (R,G,B) component the camera is able to output
			// Usually, for RAW input images are either 12- or 16-bits depending on model while non-RAW outputs (e.g. JPG or PNG) are simply 8-bits
			UInt32	_inputBitsPerComponent;

			// The default luminance factor to apply to all the images
			//	(allows you to scale the base luminance if you know the absolute value)
			float	_luminanceFactor;

			// The curve smoothness constraint used to enforce the smoothness of the response curve
			// A value of 0 doesn't constrain at all while a value of 1 makes sure the response curve is smooth
			float	_curveSmoothnessConstraint;

			// The "subjective quality" parameter used for the algorithm that guides how many pixels are going to be used to compute the response curve
			// The default value is 1 so an average number of pixels is used
			// Using a quality of 2 will use twice as many pixels, increasing response curve quality and computation time
			// Using a quality of 0.5 will use half as many pixels, decreasing response curve quality and computation time
			// WARNING: the computation time grows quadratically with quality!
			float	_quality;

			// If true then the luminance of the pixels is used and only a single response curve is computed instead of 3 individual curves for R,G and B
			bool	_luminanceOnly;

			// The type of filtering to apply to the raw response curve to smooth it out
			FILTER_TYPE	_responseCurveFilterType;

			HDRParms()
				: _inputBitsPerComponent( 8 )		// default = 8 for JPEG, 12 for RAW;
				, _luminanceFactor( 1.0f )
				, _curveSmoothnessConstraint( 1.0f )
				, _quality( 3.0f )
				, _luminanceOnly( true )
				, _responseCurveFilterType( FILTER_TYPE::SMOOTHING_GAUSSIAN ) {
			}
		};

		// Builds a HDR image from a set of LDR images
		//	_images, the array of LDR bitmaps
		//	_imageShutterSpeeds, the array of shutter speeds (in seconds) used for each image
		void		LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, HDRParms^ _parms );

		// Builds a HDR image from a set of LDR images and a response curve (usually computed using ComputeHDRResponseCurve)
		// You can use this method to build the HDR image from a larger set of LDR images than used to resolve the response curve
		//	_images, the array of LDR bitmaps
		//	_imageShutterSpeeds, the array of shutter speeds (in seconds) used for each image
		//	_responseCurve, the list of values corresponding to the response curve
		//	The default luminance factor to apply to all the images (allows you to scale the base luminance if you know the absolute value)
		void		LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, System::Collections::Generic::List< float3 >^ _responseCurve, float _luminanceFactor );
		void		LDR2HDR( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, System::Collections::Generic::List< float >^ _responseCurveLuminance, float _luminanceFactor );

		// Computes the response curve of the sensor that captured the provided LDR images
		// (NOTE: it's important to understand that the response curve shouldn't be used as-is but weighted by a "tent filter" centered on the middle of the response curve
		//	so the extremum values of the curve shouldn't be used directly because of large noisy variations in these ranges)
		//	_images, the array of LDR bitmaps
		//	_imageEVs, the array of Exposure Values (EV) used for each image
		//	_responseCurve, the list to fill with values corresponding to the response curve
		// (NOTE: if you're using the 2nd prototype then only the response curve for luminance is returned, which is okay for most sensors that won't differ much between R,G and B)
		//
		static void	ComputeCameraResponseCurve( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, UInt32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, System::Collections::Generic::List< float3 >^ _responseCurve );
		static void	ComputeCameraResponseCurve( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, UInt32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, System::Collections::Generic::List< float >^ _responseCurveLuminance );


		static void	FilterCameraResponseCurve( System::Collections::Generic::List< float3 >^ _rawResponseCurve, System::Collections::Generic::List< float3 >^ _filteredResponseCurve, FILTER_TYPE _filterType );
		static void	FilterCameraResponseCurve( System::Collections::Generic::List< float >^ _rawResponseCurve, System::Collections::Generic::List< float >^ _filteredResponseCurve, FILTER_TYPE _filterType );

	private:
		void		LDR2HDR_internal( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, const BaseLib::List< bfloat3 >& _responseCurve, bool _luminanceOnly, float _luminanceFactor );
		static void	ComputeCameraResponseCurve_internal( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, UInt32 _inputBitsPerComponent, float _curveSmoothnessConstraint, float _quality, bool _luminanceOnly, BaseLib::List< bfloat3 >& _responseCurve );

		#pragma endregion
	};
}
