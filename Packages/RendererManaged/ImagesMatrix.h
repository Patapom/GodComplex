// RendererManaged.h

#pragma once

#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace Renderer {

	// This class is used to build an array of images and their mips and is used as an argument for the static texture creation methods
	ref class	ImagesMatrix {
	public:
		ref class	Mips {
			array< ImageUtility::ImageFile^ >^	m_images;

		public:

			property UInt32						MipLevelsCount { UInt32 get() { return m_images->Length; } }
			property ImageUtility::ImageFile^	default[UInt32] { ImageUtility::ImageFile^ get( UInt32 _index ) { return m_images[_index]; } void set( UInt32, ImageUtility::ImageFile^ ); }

		public:
			// Allocates a texture arrray for each mip
			Mips( UInt32 _mipLevelsCount );
			// Automatically builds mips from level 0 mip
			Mips( ImageUtility::ImageFile^ _mip0, UInt32 _mipLevelsCount );
			// Mips are already built
			Mips( array< ImageUtility::ImageFile^ >^ _mipLevels );

// 			// Sets the image for the mip level
// 			void		SetMipLevel( UInt32 _mipLevelIndex, ImageUtility::ImageFile^ _image );

			// Automatically builds the necessary mips from the level 0 mip
			void			BuildMips( ImageUtility::ImageFile^ _mip0 );
		};

	private:
		UInt32			m_width;			// Base images width
		UInt32			m_height;			// Base images height
		UInt32			m_mipLevelsCount;	// Amount of mip levels for each array entry
		bool			m_isCubeMap;		// True if the image matrix represents a cube maps array
		array< Mips^ >^	m_mipsArray;		// An array of mip-mapped images

	public:

		property UInt32		Width { UInt32 get() { return m_width; } }
		property UInt32		Height { UInt32 get() { return m_height; } }
		property bool		IsCubeMap { bool get() { return m_isCubeMap; } void set( bool value ) { m_isCubeMap = value; } }
		property Mips^		default[UInt32] { Mips^ get( UInt32 _index ) { return m_mipsArray[_index]; } void set( UInt32 _index, Mips^ ); }

	public:
		// Allocates a texture array
		ImagesMatrix( UInt32 _width, UInt32 _height, UInt32 _arraySize, UInt32 _mipLevelsCount );
		// Automatically build mips from an array of level 0 mips
		// NOTE: All mip 0 images must have the same dimension and pixel format!
		ImagesMatrix( array< ImageUtility::ImageFile^ >^ _mips0, UInt32 _mipLevelsCount );

// 		// Sets the image for the given array index and mip level
// 		void		SetImage( UInt32 _arrayIndex, UInt32 _mipLevelIndex, ImageUtility::ImageFile^ _image );

		// Computes the next mip size
		static void		NextMipSize( UInt32% _width, UInt32% _height );
	};
}
