// RendererManaged.h

#pragma once

#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace Renderer {

	ref class View2D;

	// Wraps a 2D texture (2D, 2DArray, CubeMap, CubeMapArray, RenderTarget, DepthStencilBuffer)
	[System::Diagnostics::DebuggerDisplayAttribute( "{Width,d}x{Height,d}x{ArraySize,d}x{MipLevelsCount,d} {Tag}" )]
	public ref class Texture2D {
	internal:

		::Texture2D*	m_texture;
		Object^			m_tag;

		ImageUtility::PIXEL_FORMAT		m_pixelFormat;
		ImageUtility::COMPONENT_FORMAT	m_componentFormat;

	public:

		property UInt32	Width			{ UInt32 get() { return m_texture->GetWidth(); } }
		property UInt32	Height			{ UInt32 get() { return m_texture->GetHeight(); } }
		property UInt32	ArraySize		{ UInt32 get() { return m_texture->GetArraySize(); } }
		property UInt32	MipLevelsCount	{ UInt32 get() { return m_texture->GetMipLevelsCount(); } }
		property ImageUtility::PIXEL_FORMAT	PixelFormat { ImageUtility::PIXEL_FORMAT get() { return m_pixelFormat; } }
		property ImageUtility::COMPONENT_FORMAT	ComponentFormat { ImageUtility::COMPONENT_FORMAT get() { return m_componentFormat; } }

		property UInt32	WidthAtMip[UInt32] {
			UInt32	get( UInt32 _mipLevelIndex ) { return GetSizeAtMip( Width, _mipLevelIndex ); }
		}

		property UInt32	HeightAtMip[UInt32] {
			UInt32	get( UInt32 _mipLevelIndex ) { return GetSizeAtMip( Height, _mipLevelIndex ); }
		}

		property Object^	Tag { Object^ get() { return m_tag; } void set( Object^ _value ) { m_tag = _value; } }

		void*	GetWrappedtexture()	{ return m_texture; }

	public:

		// _Content must be of size _arraySize * _mipLevelsCount and must contain all consecutive mips for each slice (e.g. 3 mips and array size 2 : [ Mip0_slice0, Mip1_slice0, Mip2_slice0, Mip0_slice1, Mip1_slice1, Mip2_slice1])
		Texture2D( Device^ _device, UInt32 _width, UInt32 _height, int _arraySize, UInt32 _mipLevelsCount, ImageUtility::PIXEL_FORMAT _pixelFormat, ImageUtility::COMPONENT_FORMAT _componentFormat, bool _staging, bool _UAV, cli::array<PixelsBuffer^>^ _Content );
		Texture2D( Device^ _device, ImageUtility::ImagesMatrix^ _images, ImageUtility::COMPONENT_FORMAT _componentFormat );
		Texture2D( Device^ _device, UInt32 _width, UInt32 _height, UInt32 _arraySize, UInt32 _mipLevelsCount, DEPTH_STENCIL_FORMAT _DepthStencilFormat );
		~Texture2D() {
 			delete m_texture;
		}


		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture2D^ _source ) {
			m_texture->CopyFrom( *_source->m_texture );
		}

		// These are simple helpers to ease the reading and writing of textures
		delegate void	PixelReaderDelegate( UInt32 _X, UInt32 _Y, System::IO::BinaryReader^ _reader );
		void			ReadPixels( UInt32 _mipLevelIndex, UInt32 _arrayIndex, PixelReaderDelegate^ _reader ) {
			PixelsBuffer^				pixels = MapRead( _mipLevelIndex, _arrayIndex, true );
			System::IO::BinaryReader^	R = pixels->OpenStreamRead();
			UInt32	W = WidthAtMip[_mipLevelIndex];
			UInt32	H = HeightAtMip[_mipLevelIndex];
			for ( UInt32 Y=0; Y < H; Y++ ) {
				pixels->JumpToScanline( R, Y );
				for ( UInt32 X=0; X < W; X++ ) {
					_reader( X, Y, R );
				}
			}
			delete R;
			UnMap( pixels );
		}

		delegate void	PixelWriterDelegate( UInt32 _X, UInt32 _Y, System::IO::BinaryWriter^ _writer );
		void			WritePixels( UInt32 _mipLevelIndex, UInt32 _arrayIndex, PixelWriterDelegate^ _writer ) {
			PixelsBuffer^				pixels = MapWrite( _mipLevelIndex, _arrayIndex, true );
			System::IO::BinaryWriter^	Wr = pixels->OpenStreamWrite();
			UInt32	W = WidthAtMip[_mipLevelIndex];
			UInt32	H = HeightAtMip[_mipLevelIndex];
			for ( UInt32 Y=0; Y < H; Y++ ) {
				pixels->JumpToScanline( Wr, Y );
				for ( UInt32 X=0; X < W; X++ ) {
					_writer( X, Y, Wr );
				}
			}
			delete Wr;
			UnMap( pixels );
		}

		// Converts a CPU-readable texture (i.e. staging) into an ImagesMatrix
		property ImageUtility::ImagesMatrix^	AsImagesMatrix	{
			ImageUtility::ImagesMatrix^	get() {
				ImageUtility::ImagesMatrix^	result = gcnew ImageUtility::ImagesMatrix();
				m_texture->ReadAsImagesMatrix( *reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( result->NativeObject.ToPointer() ) );
				return result;
			}
		}

		// NOTE: The strange "_ImAwareOfStrideAlignmentTo128Bytes" argument in the Map() functions is here so you have to explicitely declare that you're aware of a certain fact:
		//	• Most video cards have a low-bound alignment of mapped memory of 128 bytes so you have to be careful about the row pitch length of each scanline
		//	• For example, if your texture is a R32F and your scanline has less than 128/4 = 32 texels the pitch/stride will stay at 128 bytes nonetheless
		//		and so to access the Nth scanline you'll have to use scanlineStart[N] = N * rowPitch
		//	• Just set _ImAwareOfStrideAlignmentTo128Bytes = true if you correctly handled this specificity so no exception is thrown
		//
		PixelsBuffer^	MapRead( UInt32 _mipLevelIndex, UInt32 _arrayIndex ) {
			return MapRead( _mipLevelIndex, _arrayIndex, false );	// Unaware of alignment by default
		}
		PixelsBuffer^	MapRead( UInt32 _mipLevelIndex, UInt32 _arrayIndex, bool _ImAwareOfStrideAlignmentTo128Bytes ) {
			const D3D11_MAPPED_SUBRESOURCE&	mappedResource = m_texture->MapRead( _mipLevelIndex, _arrayIndex );
			#ifdef _DEBUG
				if ( !_ImAwareOfStrideAlignmentTo128Bytes ) {
					BaseLib::COMPONENT_FORMAT	componentFormat;
					U32							pixelSize;
					DXGIFormat2PixelFormat( m_texture->GetFormat(), componentFormat, pixelSize );
					if ( m_texture->GetWidth() * pixelSize != mappedResource.RowPitch )
					throw gcnew Exception( "Be careful about 128 bytes alignment: each scanline should account for proper row stride!" );
				}
			#endif
			return gcnew PixelsBuffer( mappedResource, _mipLevelIndex, _arrayIndex, true );
		}

		PixelsBuffer^	MapWrite( UInt32 _mipLevelIndex, UInt32 _arrayIndex ) {
			return MapWrite( _mipLevelIndex, _arrayIndex, false );	// Unaware of alignment by default
		}
		PixelsBuffer^	MapWrite( UInt32 _mipLevelIndex, UInt32 _arrayIndex, bool _ImAwareOfStrideAlignmentTo128Bytes ) {
			const D3D11_MAPPED_SUBRESOURCE&	mappedResource = m_texture->MapWrite( _mipLevelIndex, _arrayIndex );
			#ifdef _DEBUG
				if ( !_ImAwareOfStrideAlignmentTo128Bytes ) {
					BaseLib::COMPONENT_FORMAT	componentFormat;
					U32							pixelSize;
					DXGIFormat2PixelFormat( m_texture->GetFormat(), componentFormat, pixelSize );
					if ( m_texture->GetWidth() * pixelSize != mappedResource.RowPitch )
						throw gcnew Exception( "Be careful about 128 bytes alignment: each scanline should account for proper row stride!" );
				}
			#endif
			return gcnew PixelsBuffer( mappedResource, _mipLevelIndex, _arrayIndex, false );
		}

		void			UnMap( PixelsBuffer^ _mappedSubResource ) {
			if ( !_mappedSubResource->m_readOnly ) {
				// Write back buffer to mapped sub-resource for upload
				_mappedSubResource->WriteToMappedSubResource();
			}
			m_texture->UnMap( _mappedSubResource->m_mappedMipLevelIndex, _mappedSubResource->m_mappedArrayIndex );
			delete _mappedSubResource;
		}

		// Views
		View2D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View2D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _arrayStart, UInt32 _arraySize );
		View2D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _arrayStart, UInt32 _arraySize, bool _asArray );

		// Uploads the texture to the shader
		void		Set( UInt32 _slotIndex );
		void		SetVS( UInt32 _slotIndex );
		void		SetHS( UInt32 _slotIndex );
		void		SetDS( UInt32 _slotIndex );
		void		SetGS( UInt32 _slotIndex );
		void		SetPS( UInt32 _slotIndex );
		void		SetCS( UInt32 _slotIndex );
		void		RemoveFromLastAssignedSlots()	{ m_texture->RemoveFromLastAssignedSlots(); }

		// Uploads the texture as a UAV for a compute shader
		void		SetCSUAV( UInt32 _slotIndex );
		void		RemoveFromLastAssignedSlotUAV()	{ m_texture->RemoveFromLastAssignedSlotUAV(); }

		// Helper to compute a size (width or height) at a specific mip level
		static UInt32		GetSizeAtMip( UInt32 _sizeAtMip0, UInt32 _mipLevelIndex ) {
			return Math::Max( 1U, _sizeAtMip0 >> _mipLevelIndex );
		}

	internal:

		Texture2D( const ::Texture2D& _existingTexture ) {
			m_texture = const_cast< ::Texture2D* >( &_existingTexture );
		}
	};

	// Wraps a 2D texture view (SRV, RTV, DSV or UAV)
	public ref class	View2D : public IView {
	internal:
		Texture2D^	m_owner;
		UInt32		m_mipLevelStart;
		UInt32		m_mipLevelsCount;
		UInt32		m_arrayStart;
		UInt32		m_arraySize;
		bool		m_asArray;

		View2D( Texture2D^ _Owner, UInt32 _MipLevelStart, UInt32 _mipLevelsCount, UInt32 _ArrayStart, UInt32 _arraySize ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _mipLevelsCount ), m_arrayStart( _ArrayStart ), m_arraySize( _arraySize ), m_asArray( false ) {}
		View2D( Texture2D^ _Owner, UInt32 _MipLevelStart, UInt32 _mipLevelsCount, UInt32 _ArrayStart, UInt32 _arraySize, bool _AsArray ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _mipLevelsCount ), m_arrayStart( _ArrayStart ), m_arraySize( _arraySize ), m_asArray( _AsArray ) {}

	public:

		virtual property UInt32	Width				{ UInt32 get(); }
		virtual property UInt32	Height				{ UInt32 get(); }
		virtual property UInt32	ArraySizeOrDepth	{ UInt32 get(); }

		virtual property ::ID3D11ShaderResourceView*	SRV { ::ID3D11ShaderResourceView*	get(); }
		virtual property ::ID3D11RenderTargetView*		RTV { ::ID3D11RenderTargetView*		get(); }
		virtual property ::ID3D11UnorderedAccessView*	UAV { ::ID3D11UnorderedAccessView*	get(); }
		virtual property ::ID3D11DepthStencilView*		DSV { ::ID3D11DepthStencilView*		get(); }

	public:	// Setters to shader inputs
		void		Set( UInt32 _slotIndex );
		void		SetVS( UInt32 _slotIndex );
		void		SetHS( UInt32 _slotIndex );
		void		SetDS( UInt32 _slotIndex );
		void		SetGS( UInt32 _slotIndex );
		void		SetPS( UInt32 _slotIndex );
		void		SetCS( UInt32 _slotIndex );
		void		SetCSUAV( UInt32 _slotIndex );
	};
}
