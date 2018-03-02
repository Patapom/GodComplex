//////////////////////////////////////////////////////////////////////////
// This class is used to build an array of images and their mips and is used as an argument for the DDS-related methods of the ImageFile class
// It's also used as an entry point for texture-related creation methods in the rendering library
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#include "ImageFile.h"

namespace ImageUtilityLib {

	// The images matrix class contains a collection of "Mips"
	// • For Texture2DArrays and TextureCubes, the collection of mips is of the size of the array (or 6 times the amount of cube maps)
	// • For Texture3D, the collection contains only a single "Mips" element but its Mip contain an array of slices
	//
	class	ImagesMatrix {
	public:
		// Image type used for mips generation
		enum IMAGE_TYPE {
			LINEAR,		// Default
			sRGB,		// sRGB-packed colors
			NORMAL_MAP,	// 0.5-offseted vectors
		};

		// The Mips class contains a collection of "Mip" elements, as many mips as necessary to represent a texture
		//
		class	Mips {
		public:

			// The Mip class is the final container of ImageFiles
			// • For Texture2DArrays and TextureCubes, the class contains only a single image
			// • For Texture3D, a single Mip can contain multiple images to represent the appropriate depth at the current mip level
			//
			class	Mip {

				U32					m_width;			// Mip width
				U32					m_height;			// Mip height
				List< ImageFile* >	m_images;			// The list of images in the mip

				// Raw buffer data
				U32					m_rowPitch;			// Pitch to reach next row in the buffer
				U32					m_slicePitch;		// Pitch to reach next slice in the buffer
				U8*					m_rawBuffer;		// The list of raw buffers in the mip

			public:

				U32				Width() const	{ return m_width; }
				U32				Height() const	{ return m_height; }
				U32				Depth() const	{ return m_images.Count(); }

				// Indexers
				ImageFile*&			operator[]( U32 _index )					{ return m_images[_index]; }
				ImageFile* const&	operator[]( U32 _index ) const				{ return m_images[_index]; }

				// Raw buffer access
				U32				RowPitch() const	{ return m_rowPitch; }
				U32				SlicePitch() const	{ return m_slicePitch; }
				U8*				GetRawBuffer()		{ return m_rawBuffer; }
				const U8*		GetRawBuffer() const{ return m_rawBuffer; }

			public:
								Mip() : m_width( 0 ), m_height( 0 ), m_rowPitch( 0 ), m_slicePitch( 0 ), m_rawBuffer( NULL ) {}
				void			Init( U32 _width, U32 _height, U32 _depth );

				// Allocates/Releases actual ImageFiles and Raw buffer
				void			AllocateImageFiles( PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
				void			AllocateRawBuffer( U32 _rowPitch, U32 _slicePitch, const U8* _sourceBuffer=NULL );
				void			ReleasePointers();	// Release image and raw buffer pointers
				void			ClearPointers();	// Clears pointers but don't release

				void			MakeSigned();
				void			MakeUnSigned();

				// Build the mips from mip 0
				void			BuildMip3D( const Mip& _sourceMip, IMAGE_TYPE _imageType );
			};

		private:

			List< Mip >		m_mips;

		public:

 			U32				GetMipLevelsCount() const		{ return m_mips.Count(); }

			// Indexers
			Mip&			operator[]( U32 _index )		{ return m_mips[_index]; }
			const Mip&		operator[]( U32 _index ) const	{ return m_mips[_index]; }

		public:
							Mips() {}
			void			Init( U32 _mipLevelsCount );

			// Allocates/Releases actual ImageFiles
			void			AllocateImageFiles( PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
			void			ReleasePointers();	// Release image and raw buffer pointers
			void			ClearPointers();	// Clears pointers but don't release

			void			MakeSigned();
			void			MakeUnSigned();

			// Build the mips from mip 0
			void			BuildMips2D( IMAGE_TYPE _imageType );
			void			BuildMips3D( IMAGE_TYPE _imageType );
			static void		BuildMip2D( const ImageFile& _sourceMip, ImageFile& _targetMip, IMAGE_TYPE _imageType );
		};

		// The type of texture the matrix is a container for
		enum class	TYPE {
			GENERIC,
			TEXTURE2D,
			TEXTURECUBE,
			TEXTURE3D,
		};

		// Functor class used by the AllocateRawBuffers() method to allocate a raw buffer
		class GetRawBufferSizeFunctor {
		public:
			// _arraySliceIndex, _mipLevelIndex, the position nin the array we need raw buffer information for
			// _rowPitch, the pitch to reach the next row in the buffer
			// _slicePitch, the pitch to reach the next slice in the buffer
			// Returns the raw buffer to copy from, or NULL if you want to copy manually later
			virtual const U8*	operator()( U32 _arraySliceIndex, U32 _mipLevelIndex, U32& _rowPitch, U32& _slicePitch ) const abstract;
		};

	private:

		TYPE				m_type;				// Optional field describing the type of images stored in the matrix
		PIXEL_FORMAT		m_format;			// Pixel format of the images in the matrix
		ColorProfile		m_colorProfile;		// Color profile of the images in the matrix
		List< Mips >		m_mipsArray;		// An array of mip-mapped images

	public:

		TYPE					GetType() const					{ return m_type; }
		void					SetType( TYPE value )			{ m_type = value; }
		PIXEL_FORMAT			GetFormat() const				{ return m_format; }
		const ColorProfile&		GetColorProfile() const			{ return m_colorProfile; }
		ColorProfile&			GetColorProfile()				{ return m_colorProfile; }
		U32						GetArraySize() const			{ return m_mipsArray.Count(); }

		// Indexers
		Mips&					operator[]( U32 _index )		{ return m_mipsArray[_index]; }
		const Mips&				operator[]( U32 _index ) const	{ return m_mipsArray[_index]; }

	public:
						ImagesMatrix();
						~ImagesMatrix();

		// Allocates a texture array
		void			InitTexture2DArray( U32 _width, U32 _height, U32 _arraySize, U32 _mipLevelsCount );
		void			InitCubeTextureArray( U32 _cubeMapSize, U32 _cubeMapsCount, U32 _mipLevelsCount );

		// Allocates a 3D texture
		void			InitTexture3D( U32 _width, U32 _height, U32 _depth, U32 _mipLevelsCount );

		// Allocates a generic texture
		void			InitTextureGeneric( U32 _width, U32 _height, U32 _depth, U32 _arraySize, U32 _mipLevelsCount );

		// Allocates/Releases actual ImageFiles
		void			AllocateImageFiles( PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
		void			AllocateRawBuffers( PIXEL_FORMAT _format, const GetRawBufferSizeFunctor& _getRawBufferSizeDelegate );
		void			ReleasePointers();	// Release image and raw buffer pointers
		void			ClearPointers();	// Clears pointers but don't release

		// Converts from a source matrix into a target matrix
		void			ConvertFrom( const ImagesMatrix& _source, PIXEL_FORMAT _targetFormat, const ColorProfile& _colorProfile );

		// Makes the images signed/unsigned
		// WARNING: Works only for integer formats: throws if called on floating-point formats!
		void			MakeSigned();
		void			MakeUnSigned();

		//////////////////////////////////////////////////////////////////////////
		// DDS Loading / Saving / Compression
		void			DDSLoadFile( const wchar_t* _fileName, COMPONENT_FORMAT& _componentFormat );
		void			DDSLoadMemory( U64 _fileSize, void* _fileContent, COMPONENT_FORMAT& _componentFormat );
		void			DDSSaveFile( const wchar_t* _fileName, COMPONENT_FORMAT _componentFormat=COMPONENT_FORMAT::AUTO ) const;
		void			DDSSaveMemory( U64& _fileSize, void*& _fileContent, COMPONENT_FORMAT _componentFormat=COMPONENT_FORMAT::AUTO ) const;	// NOTE: The caller MUST delete[] the returned buffer!

		// DDS-Compression
		enum class COMPRESSION_TYPE {
			BC4,
			BC5,
			BC6H,
			BC7,
		};
		void			DDSCompress( const ImagesMatrix& _source, COMPRESSION_TYPE _compressionType, COMPONENT_FORMAT _componentFormat=COMPONENT_FORMAT::AUTO, void* _blindPointerDevice=NULL );	// NOTE: Pass a valid D3D device to enable GPU compression

		static DXGI_FORMAT	CompressionType2DXGIFormat( COMPRESSION_TYPE _compressionType, COMPONENT_FORMAT _componentFormat );

		//////////////////////////////////////////////////////////////////////////
		// Mips Building methods
		void			BuildMips( IMAGE_TYPE _imageType );

		// Computes the next mip size
		static void		NextMipSize( U32& _size );
		static void		NextMipSize( U32& _width, U32& _height );
		static void		NextMipSize( U32& _width, U32& _height, U32& _depth );
		static U32		ComputeMipsCount( U32 _size );

	private:

		void			DDSLoad( const void* _blindPointerImage, const void* _blindPointerMetaData, COMPONENT_FORMAT& _componentFormat );
		void			DDSSave( void** _blindPointerImage, COMPONENT_FORMAT _componentFormat ) const;
	};
}
