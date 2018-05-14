// ImagesMatrix.h

#pragma once

#include "ImageFile.h"
#include "ColorProfile.h"

using namespace System;

namespace ImageUtility {

	ref class ImageFile;

	// This class is used to build an array of images and their mips and is used as an argument for the static texture creation methods
	[System::Diagnostics::DebuggerDisplayAttribute( "{ArraySize} Slices {Type} {Format}" )]
	public ref class	ImagesMatrix {
	internal:
		bool							m_ownedObject;
		ImageUtilityLib::ImagesMatrix*	m_nativeObject;

// 		ImagesMatrix( ImageUtilityLib::ImagesMatrix& _nativeObject ) {
// 			m_ownedObject = false;
// 			m_nativeObject = &_nativeObject;
// 		}

	public:
		// Image type used for mips generation
		enum class IMAGE_TYPE {
			LINEAR,		// Default
			sRGB,		// sRGB-packed colors
			NORMAL_MAP,	// 0.5-offseted vectors
		};

		ref class	Mips {
		internal:
			ImageUtilityLib::ImagesMatrix::Mips*	m_nativeObject;
			Mips( ImageUtilityLib::ImagesMatrix::Mips& _nativeObject ) {
				m_nativeObject = &_nativeObject;
			}

		public:
			ref class	Mip {
			internal:
				ImageUtilityLib::ImagesMatrix::Mips::Mip*	m_nativeObject;
				Mip( ImageUtilityLib::ImagesMatrix::Mips::Mip& _nativeObject ) {
					m_nativeObject = &_nativeObject;
				}

			public:

				property UInt32		Width			{ UInt32 get() { return m_nativeObject->Width(); } }
				property UInt32		Height			{ UInt32 get() { return m_nativeObject->Height(); } }
				property UInt32		Depth			{ UInt32 get() { return m_nativeObject->Depth(); } }
				property ImageFile^	default[UInt32]	{
					ImageFile^ get( UInt32 _index ) {
						ImageUtilityLib::ImageFile* nativeImage = (*m_nativeObject)[_index];
						return nativeImage != NULL ? gcnew ImageFile( *nativeImage, false ) : nullptr;
					}
					void		set( UInt32 _index, ImageFile^ value ) {
						(*m_nativeObject)[_index] = value != nullptr ? value->m_nativeObject : NULL;
					}
				}
			};

		public:

			property UInt32		MipLevelsCount	{ UInt32 get() { return m_nativeObject->GetMipLevelsCount(); } }
			property Mip^		default[UInt32]	{ Mip^ get( UInt32 _index ) { return gcnew Mip( (*m_nativeObject)[_index] ); } }
		};

		enum class	TYPE : UInt32 {
			GENERIC,
			TEXTURE2D,
			TEXTURECUBE,
			TEXTURE3D,
		};

		delegate void	GetRawBufferSizeDelegate( UInt32 _arraySliceIndex, UInt32 _mipLevelIndex, UInt32% _rowPitch, UInt32% _slicePitch, UInt32% _slicesCount );

	public:

		property TYPE						Type				{ TYPE get() { return TYPE( m_nativeObject->GetType() ); } }
		property PIXEL_FORMAT				Format				{ PIXEL_FORMAT get() { return PIXEL_FORMAT( m_nativeObject->GetFormat() ); } }
		property UInt32						ArraySize			{ UInt32 get() { return m_nativeObject->GetArraySize(); } }
		property ImageUtility::ColorProfile^ColorProfile		{ ImageUtility::ColorProfile^ get() { return gcnew ImageUtility::ColorProfile( m_nativeObject->GetColorProfile() ); } }
		property Mips^						default[UInt32]		{ Mips^ get( UInt32 _index ) { return gcnew Mips( (*m_nativeObject)[_index] ); } }

		property IntPtr						NativeObject		{ IntPtr get() { return IntPtr( m_nativeObject ); } }

	public:
		ImagesMatrix() {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImagesMatrix();
		}
		~ImagesMatrix() {
			if ( m_ownedObject ) {
				delete m_nativeObject;
				m_nativeObject = NULL;
			}
		}

		// Builds a Texture2DArray with the convention that dimension 0 is the array slices and dimension 1 is the mips
		ImagesMatrix( array< ImageFile^, 2>^ _images ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImagesMatrix();

			ImageFile	referenceImage = _images[0,0];
			InitTexture2DArray( referenceImage.Width, referenceImage.Height, _images->GetLength(0), _images->GetLength(1) );

			for ( int arraySliceIndex=0; arraySliceIndex < _images->GetLength(0); arraySliceIndex++ ) {
				Mips^	sliceMips = this[arraySliceIndex];
				for ( int miplevelIndex=0; miplevelIndex < _images->GetLength(1); miplevelIndex++ ) {
					Mips::Mip^	mip = sliceMips[miplevelIndex];
					mip[0] = _images[arraySliceIndex,miplevelIndex];
				}
			}

			// "Allocate images"... Actually, all images should be already allocated, we just need to assign the format and color profile
			AllocateImageFiles( referenceImage.PixelFormat, referenceImage.ColorProfile );
		}

		// Builds a full mip-chain from a single image
		ImagesMatrix( ImageFile^ _image, IMAGE_TYPE _imageType ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImagesMatrix();

			InitTexture2DArray( _image->Width, _image->Height, 1, 0 );
			this[0][0][0] = _image;
			AllocateImageFiles( _image->PixelFormat, _image->ColorProfile );
			BuildMips( _imageType );
		}

		// Allocates a texture array
		void			InitTexture2DArray( U32 _width, U32 _height, U32 _arraySize, U32 _mipLevelsCount ) {
			m_nativeObject->InitTexture2DArray( _width, _height, _arraySize, _mipLevelsCount );
		}
		void			InitCubeTextureArray( U32 _cubeMapSize, U32 _cubeMapsCount, U32 _mipLevelsCount ) {
			m_nativeObject->InitCubeTextureArray( _cubeMapSize, _cubeMapsCount, _mipLevelsCount );
		}

		// Allocates a 3D texture
		void			InitTexture3D( U32 _width, U32 _height, U32 _depth, U32 _mipLevelsCount ) {
			m_nativeObject->InitTexture3D( _width, _height, _depth, _mipLevelsCount );
		}

		// Allocates/Releases actual ImageFiles or Raw Buffers
		void			AllocateImageFiles( PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile ) {
			m_nativeObject->AllocateImageFiles( BaseLib::PIXEL_FORMAT( _format ), *_colorProfile->m_nativeObject );
		}
		void			AllocateImageFiles( GetRawBufferSizeDelegate^ _getRawBufferSizeDelegate ) {
			// Get a function pointer to the delegate
			System::Runtime::InteropServices::GCHandle	gch = System::Runtime::InteropServices::GCHandle::Alloc( _getRawBufferSizeDelegate );
			IntPtr		ip = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate( _getRawBufferSizeDelegate );

//			m_nativeObject->AllocateRawBuffers( static_cast< ImageUtilityLib::ImagesMatrix::GetRawBufferSizeDelegate_t >( ip.ToPointer() ) );

			// release reference to delegate  
			gch.Free();  
		}
		void			ReleasePointers() {
			m_nativeObject->ReleasePointers();
		}
		void			ClearPointers() {
			m_nativeObject->ClearPointers();
		}

		// Converts from a source matrix into a target matrix
		void			ConvertFrom( ImagesMatrix^ _source, PIXEL_FORMAT _targetFormat, ImageUtility::ColorProfile^ _colorProfile ) {
			m_nativeObject->ConvertFrom( *_source->m_nativeObject, BaseLib::PIXEL_FORMAT( _targetFormat ), *_colorProfile->m_nativeObject );
		}

		// Makes the images signed/unsigned
		// WARNING: Works only for integer formats: throws if called on floating-point formats!
		void			MakeSigned()	{ m_nativeObject->MakeSigned(); }
		void			MakeUnSigned()	{ m_nativeObject->MakeUnSigned(); }

	public:
		//////////////////////////////////////////////////////////////////////////
		// DDS-related methods
		COMPONENT_FORMAT	DDSLoadFile( System::IO::FileInfo^ _fileName );
		COMPONENT_FORMAT	DDSLoadMemory( NativeByteArray^ _imageContent );
		void				DDSSaveFile( System::IO::FileInfo^ _fileName, COMPONENT_FORMAT _componentFormat );
		NativeByteArray^	DDSSaveMemory( COMPONENT_FORMAT _componentFormat );

		// DDS-Compression
 		// NOTE: Use the RendererLib::Device version to compress using the GPU
		enum class COMPRESSION_TYPE {
			BC4,
			BC5,
			BC6H,
			BC7,
		};
		// NOTE: If you want to use GPU compression, use Device.DDSCompress() instead!
		void	DDSCompress( ImagesMatrix^ _sourceImage, COMPRESSION_TYPE _compressionType, COMPONENT_FORMAT _componentFormat ) {
			m_nativeObject->DDSCompress( *reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( _sourceImage->NativeObject.ToPointer() ), ImageUtilityLib::ImagesMatrix::COMPRESSION_TYPE( _compressionType ), BaseLib::COMPONENT_FORMAT( _componentFormat ) );
		}

	public:
		//////////////////////////////////////////////////////////////////////////
		// Mips generation methods
		void			BuildMips( IMAGE_TYPE _imageType )	{ m_nativeObject->BuildMips( (ImageUtilityLib::ImagesMatrix::IMAGE_TYPE) _imageType ); }

		// Computes the next mip size
		static void		NextMipSize( UInt32% _size ) { U32 size; ImageUtilityLib::ImagesMatrix::NextMipSize( size ); _size = size; }
		static void		NextMipSize( UInt32% _width, UInt32% _height ) { U32 width, height; ImageUtilityLib::ImagesMatrix::NextMipSize( width, height ); _width = width; _height = height; }
		static void		NextMipSize( UInt32% _width, UInt32% _height, UInt32% _depth ) { U32 width, height, depth; ImageUtilityLib::ImagesMatrix::NextMipSize( width, height, depth ); _width = width; _height = height; _depth = depth; }
	};
}
