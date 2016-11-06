//////////////////////////////////////////////////////////////////////////
// This special Bitmap class carefully handles color profiles to provide a faithful internal image representation that
//	is always stored as 32-bits floating point precision CIE XYZ device-independent format that you can later convert
//	to any other format.
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

		/// <summary>
		/// Gets or sets the image's color profile
		/// </summary>
		property ColorProfile^	Profile {
			ColorProfile^	get() {
				ImageUtilityLib::ColorProfile&	nativeProfile = m_nativeObject->GetProfile();
				return gcnew ColorProfile( nativeProfile );
			}
			void			set( ColorProfile^ value ) {
				if ( value == nullptr )
					throw gcnew Exception( "Invalid profile for bitmap! A bitmap must always have a valid, non-null color profile!" );
				m_nativeObject->SetProfile( *value->m_nativeObject );
			}
		}

		// Accesses the individual XYZ-Alpha pixels
		property float4^	Item[UInt32, UInt32] {
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
		//	_profile, an optional color profile (NOTE: you will need a valid profile if you wish to save the bitmap)
		Bitmap( int _width, int _height, ColorProfile^ _profile ) {
			if ( _profile == nullptr )
				throw gcnew Exception( "Invalid profile for bitmap! A bitmap must always have a valid, non-null color profile!" );
			m_nativeObject = new ImageUtilityLib::Bitmap( _width, _height, *_profile->m_nativeObject );
		}

		// Creates a bitmap from a file
		Bitmap( ImageFile^ _file ) {
			FromImageFile( _file );
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
			ToImageFile( _targetFile, _targetFormat, false );
		}
		void			ToImageFile( ImageFile^ _targetFile, ImageFile::PIXEL_FORMAT _targetFormat, bool _premultiplyAlpha ) {
			m_nativeObject->ToImageFile( *_targetFile->m_nativeObject, ImageUtilityLib::ImageFile::PIXEL_FORMAT( _targetFormat ), _premultiplyAlpha );
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

		#pragma endregion
	};
}
