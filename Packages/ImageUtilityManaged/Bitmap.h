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
	public ref class Bitmap {
	internal:
		ImageUtilityLib::Bitmap*	m_nativeObject;

	public:
		#pragma region PROPERTIES

		/// <summary>
		/// Gets the image width
		/// </summary>
		property int		Width {
			int		get() { return m_nativeObject->Width(); }
		}

		/// <summary>
		/// Gets the image height
		/// </summary>
		property int		Height {
			int		get() { return m_nativeObject->Height(); }
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
		property float4^	default[UInt32, UInt32] {
			float4^		get( UInt32 _X, UInt32 _Y ) {
				const bfloat4&	native = m_nativeObject->Access( _X, _Y );
				return float4( native.x, native.y, native.z, native.w );
			}
			void		set( UInt32 _X, UInt32 _Y, float4^ value ) {
				bfloat4&	native = m_nativeObject->Access( _X, _Y );
				native.Set( value->x, value->y, value->z, value->w );
			}
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
		Bitmap( int _width, int _height ) {
			m_nativeObject = new ImageUtilityLib::Bitmap( _width, _height );
		}

		// Creates a bitmap from a file
		Bitmap( ImageFile^ _file ) {
			FromImageFile( _file );
		}

		void			Init( int _width, int _height ) {
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
			m_nativeObject->FromImageFile( *_sourceFile->m_nativeObject, _profileOverride != nullptr ? _profileOverride->m_nativeObject : nullptr, _unPremultiplyAlpha );
		}

		// Builds an image file from the bitmap
		void			ToImageFile( ImageFile^ _targetFile, ImageFile::PIXEL_FORMAT _targetFormat ) {
			ToImageFile( _targetFile, _targetFormat, nullptr, false );
		}
		void			ToImageFile( ImageFile^ _targetFile, ImageFile::PIXEL_FORMAT _targetFormat, ColorProfile^ _profileOverride ) {
			ToImageFile( _targetFile, _targetFormat, nullptr, false );
		}
		void			ToImageFile( ImageFile^ _targetFile, ImageFile::PIXEL_FORMAT _targetFormat, ColorProfile^ _profileOverride, bool _premultiplyAlpha ) {
			m_nativeObject->ToImageFile( *_targetFile->m_nativeObject, ImageUtilityLib::ImageFile::PIXEL_FORMAT( _targetFormat ), _profileOverride != nullptr ? _profileOverride->m_nativeObject : nullptr, _premultiplyAlpha );
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
		//	• Always capture LDR images using apertures of f/8 or higher to avoid a possible lense variance between radiance and irradiance (vignetting)
		//
		ref struct HDRParms {
			// The amount of bits per (R,G,B) component the camera is able to output
			// Usually, for RAW input images are either 12- or 16-bits depending on model while non-RAW outputs (e.g. JPG or PNG) are simply 8-bits
			UInt32	_inputBitsPerComponent;		// default = 8 for JPEG, 12 for RAW;

			//	The default luminance factor to apply to all the images
			//	(allows you to scale the base luminance if you know the absolute value)
			float	_luminanceFactor;			// default = 1.0f

			// The curve smoothness constraint used to enforce the smoothness of the response curve
			// A value of 0 doesn't constrain at all while a value of 1 makes sure the response curve is smooth
			float	_curveSmoothnessConstraint;	// default = 1.0f

			// The "subjective quality" parameter used for the algorithm that guides how many pixels are going to be used to compute the response curve
			// The default value is 1 so an average number of pixels is used
			// Using a quality of 2 will use twice as many pixels, increasing response curve quality and computation time
			// Using a quality of 0.5 will use half as many pixels, decreasing response curve quality and computation time
			float	_quality;					// default = 1.0;f
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

		// Computes the response curve of the sensor that captured the provided LDR images
		//	_images, the array of LDR bitmaps
		//	_imageEVs, the array of Exposure Values (EV) used for each image
		//	_responseCurve, the list to fill with values corresponding to the response curve
		static void	ComputeCameraResponseCurve( cli::array< ImageFile^ >^ _images, cli::array< float >^ _imageShutterSpeeds, HDRParms^ _parms, System::Collections::Generic::List< float3 >^ _responseCurve );

		#pragma endregion
	};
}
