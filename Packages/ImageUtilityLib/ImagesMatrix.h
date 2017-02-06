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

			public:

				U32				Width() const	{ return m_width; }
				U32				Height() const	{ return m_height; }
				U32				Depth() const	{ return m_images.Count(); }

				// Indexers
				ImageFile*&			operator[]( U32 _index )					{ return m_images[_index]; }
				ImageFile* const&	operator[]( U32 _index ) const				{ return m_images[_index]; }

			public:
								Mip() : m_width( 0 ), m_height( 0 ) {}
				void			Init( U32 _width, U32 _height, U32 _depth );

				// Allocates/Releases actual ImageFiles
				void			AllocateImageFiles( ImageFile::PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
				void			ReleaseImageFiles();
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
			void			AllocateImageFiles( ImageFile::PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
			void			ReleaseImageFiles();
		};

		// The type of texture the matrix is a container for
		enum class	TYPE {
			GENERIC,
			TEXTURE2D,
			TEXTURECUBE,
			TEXTURE3D,
		};

	private:

		TYPE					m_type;				// Optional field describing the type of images stored in the matrix
		ImageFile::PIXEL_FORMAT	m_format;			// Pixel format of the images in the matrix
		ColorProfile			m_colorProfile;		// Color profile of the images in the matrix
		List< Mips >			m_mipsArray;		// An array of mip-mapped images

	public:

		TYPE					GetType() const					{ return m_type; }
		void					SetType( TYPE value )			{ m_type = value; }
		ImageFile::PIXEL_FORMAT	GetFormat() const				{ return m_format; }
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
		void			AllocateImageFiles( ImageFile::PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
		void			ReleaseImageFiles();

		// Computes the next mip size
		static void		NextMipSize( U32& _size );
		static void		NextMipSize( U32& _width, U32& _height );
		static void		NextMipSize( U32& _width, U32& _height, U32& _depth );
		static U32		ComputeMipsCount( U32 _size );
	};
}
