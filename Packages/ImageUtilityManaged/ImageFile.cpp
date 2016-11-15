#include "stdafx.h"

#include "ImageFile.h"

using namespace ImageUtility;

// Helper to wrap a bunch of images into a managed array
cli::array< ImageFile^ >^	WrapNativeImages( U32 _imagesCount, ImageUtilityLib::ImageFile*& _images, bool _deleteNativeImages ) {
	// Wrap our managed version of ImageFiles around returned images
	cli::array< ImageFile^ >^	result = gcnew cli::array< ImageFile^ >( _imagesCount );
	for ( U32 imageIndex=0; imageIndex < _imagesCount; imageIndex++ ) {
		result[imageIndex] = gcnew ImageFile( _images[imageIndex], true );
	}

	if ( _deleteNativeImages )
		delete[] _images;

	return result;
}

// Creates a bitmap from a System::Drawing.Bitmap and a color profile
ImageFile::ImageFile( System::Drawing::Bitmap^ _bitmap, ImageUtility::ColorProfile^ _colorProfile ) {
	m_ownedObject = true;
	m_nativeObject = new ImageUtilityLib::ImageFile();

	// Load the bitmap's content
	int	width, height;
	cli::array< Byte >^	bitmapContent = LoadBitmap( _bitmap, width, height );
	if ( bitmapContent == nullptr )
		throw gcnew Exception( "Failed to load bitmap content into an RGBA[]!" );

	// Initialize an empty native object
	m_nativeObject->Init( width, height, ImageUtilityLib::ImageFile::PIXEL_FORMAT::RGBA8, *_colorProfile->m_nativeObject );

	// Copy bitmap content
	U8*		target = (U8*) m_nativeObject->GetBits();
	int		sourceIndex = 0;
	for ( int i=width*height; i > 0; i-- ) {
		*target++ = bitmapContent[sourceIndex++];
		*target++ = bitmapContent[sourceIndex++];
		*target++ = bitmapContent[sourceIndex++];
		*target++ = bitmapContent[sourceIndex++];
	}
}
ImageFile::~ImageFile() {
	Exit();
	if ( m_ownedObject ) {
		SAFE_DELETE( m_nativeObject );
	}
}

void	ImageFile::Init( U32 _width, U32 _height, PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile ) {
	m_nativeObject->Init( _width, _height, ImageUtilityLib::ImageFile::PIXEL_FORMAT( _format ), *_colorProfile->m_nativeObject );
}

void	ImageFile::Exit() {
	m_nativeObject->Exit();
}

// Load from a file or memory
void	ImageFile::Load( System::IO::FileInfo^ _fileName ) {
	if ( !_fileName->Exists )
		throw gcnew System::IO::FileNotFoundException( "File \"" + _fileName + "\" not found!" );
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_nativeObject->Load( nativeFileName );
}
void	ImageFile::Load( System::IO::FileInfo^ _fileName, FILE_FORMAT _format ) {
	if ( !_fileName->Exists )
		throw gcnew System::IO::FileNotFoundException( "File \"" + _fileName + "\" not found!" );
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_nativeObject->Load( nativeFileName, ImageUtilityLib::ImageFile::FILE_FORMAT( _format ) );

// Or alternatively, we could load the stream
// 	using ( System::IO::FileStream ImageStream = _fileName.Open( System::IO::FileMode.Open, System::IO::FileAccess.Read, System::IO::FileShare.Read ) )
// 		Load( ImageStream, _format );
}
void	ImageFile::Load( System::IO::Stream^ _imageStream, FILE_FORMAT _format ) {
	// Read the file's content
	cli::array< Byte >^	imageContent = gcnew cli::array< Byte >( int( _imageStream->Length ) );
	_imageStream->Read( imageContent, 0, (int) _imageStream->Length );

	// Read from memory
	Load( gcnew NativeByteArray( imageContent ), _format );
}
void	ImageFile::Load( NativeByteArray^ _fileContent, FILE_FORMAT _format ) {
	// Call native method
	m_nativeObject->Load( _fileContent->AsBytePointer.ToPointer(), _fileContent->Length, ImageUtilityLib::ImageFile::FILE_FORMAT( _format ) );
}

// Save to a file or memory
void	ImageFile::Save( System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_nativeObject->Save( nativeFileName );
}
void	ImageFile::Save( System::IO::FileInfo^ _fileName, FILE_FORMAT _format ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_nativeObject->Save( nativeFileName, ImageUtilityLib::ImageFile::FILE_FORMAT( _format ) );
}
void	ImageFile::Save( System::IO::FileInfo^ _fileName, FILE_FORMAT _format, SAVE_FLAGS _options ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_nativeObject->Save( nativeFileName, ImageUtilityLib::ImageFile::FILE_FORMAT( _format ), ImageUtilityLib::ImageFile::SAVE_FLAGS( _options ) );
}
void	ImageFile::Save( System::IO::Stream^ _imageStream, FILE_FORMAT _format, SAVE_FLAGS _options ) {
	// Save into a temporary array
	NativeByteArray^	temp = Save( _format, _options );

	// Dump to stream
	_imageStream->Write( temp->AsByteArray, 0, temp->Length );

	delete temp;
}
NativeByteArray^	ImageFile::Save( FILE_FORMAT _format, SAVE_FLAGS _options ) {
	// Save to native memory
	void*	nativeBuffer = nullptr;
	U64		nativeBufferSize = 0;
	m_nativeObject->Save( ImageUtilityLib::ImageFile::FILE_FORMAT( _format ), ImageUtilityLib::ImageFile::SAVE_FLAGS( _options ), nativeBufferSize, nativeBuffer );

	// Copy to managed array
	return gcnew NativeByteArray( nativeBufferSize, nativeBuffer );
}

 System::Drawing::Bitmap^	ImageFile::AsBitmap::get() {
	int	W = Width;
	int	H = Height;

	// Convert source bitmap to a compatible format
	ImageFile^	source = this;
	if ( PixelFormat != PIXEL_FORMAT::RGB8 && PixelFormat != PIXEL_FORMAT::RGBA8 ) {
		source = gcnew ImageFile();
		source->ConvertFrom( this, PIXEL_FORMAT::RGBA8 );
	}

	System::Drawing::Bitmap^	result = gcnew System::Drawing::Bitmap( W, H, System::Drawing::Imaging::PixelFormat::Format32bppArgb );

	const U8*		sourcePtr = (U8*) source->Bits.ToPointer();

	System::Drawing::Imaging::BitmapData^	lockedBitmap = result->LockBits( System::Drawing::Rectangle( 0, 0, W, H ), System::Drawing::Imaging::ImageLockMode::ReadOnly, System::Drawing::Imaging::PixelFormat::Format32bppArgb );

	if ( source->PixelFormat == PIXEL_FORMAT::RGBA8 ) {
		// 32 bpp
		for ( int Y=0; Y < H; Y++ ) {
			pin_ptr<Byte>	targetPtr = (Byte*) lockedBitmap->Scan0.ToPointer() + Y * lockedBitmap->Stride;
			for ( int X=0; X < W; X++ ) {
				*targetPtr++ = *sourcePtr++;	// B
				*targetPtr++ = *sourcePtr++;	// G
				*targetPtr++ = *sourcePtr++;	// R
				*targetPtr++ = *sourcePtr++;	// A
			}
		}
	} else {
		// 24 bpp
		for ( int Y=0; Y < H; Y++ ) {
			pin_ptr<Byte>	targetPtr = (Byte*) lockedBitmap->Scan0.ToPointer() + Y * lockedBitmap->Stride;
			for ( int X=0; X < W; X++ ) {
				*targetPtr++ = *sourcePtr++;	// B
				*targetPtr++ = *sourcePtr++;	// G
				*targetPtr++ = *sourcePtr++;	// R
				*targetPtr++ = 0xFFU;			// A
			}
		}
	}

	result->UnlockBits( lockedBitmap );

	if ( source != this )
		delete source;	// We had to make a temporary conversion so now we must delete it

	return result;
}


// Converts the source image to a target format
void	ImageFile::ConvertFrom( ImageFile^ _source, PIXEL_FORMAT _targetFormat ) {
	m_nativeObject->ConvertFrom( *_source->m_nativeObject, ImageUtilityLib::ImageFile::PIXEL_FORMAT( _targetFormat ) );
}

// Tone maps a HDR image into a LDR RGBA8 format
void	ImageFile::ToneMapFrom( ImageFile^ _source, ToneMapper^ _toneMapper ) {

	// Get a function pointer to the delegate
	System::Runtime::InteropServices::GCHandle	gch = System::Runtime::InteropServices::GCHandle::Alloc( _toneMapper );
	IntPtr		ip = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate( _toneMapper );

	m_nativeObject->ToneMapFrom( *_source->m_nativeObject, static_cast< ImageUtilityLib::ImageFile::toneMapper_t >( ip.ToPointer() ) );

	// release reference to delegate  
	gch.Free();  
}

// Retrieves the image file type based on the image file name
ImageFile::FILE_FORMAT	ImageFile::GetFileType( System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	return FILE_FORMAT( ImageUtilityLib::ImageFile::GetFileType( nativeFileName ) );
}

void	ImageFile::ReadScanline( UInt32 _Y, cli::array< float4 >^ _color, UInt32 _startX ) {
	pin_ptr<float4>	color = &_color[0];
	m_nativeObject->ReadScanline( _Y, (bfloat4*) color, _startX, _color->Length );
}
void	ImageFile::WriteScanline( UInt32 _Y, cli::array< float4 >^ _color, UInt32 _startX ) {
	pin_ptr<float4>	color = &_color[0];
	m_nativeObject->WriteScanline( _Y, (bfloat4*) color, _startX, _color->Length );
}


//////////////////////////////////////////////////////////////////////////
// DDS-related methods

// Compresses a single image
NativeByteArray^	ImageFile::DDSCompress( COMPRESSION_TYPE _compressionType ) {

	// Call native method
	void*	compressedImage;
	U32		compressedImageLength;
	m_nativeObject->DDSCompress( ImageUtilityLib::ImageFile::COMPRESSION_TYPE( _compressionType ), compressedImageLength, compressedImage );

	return gcnew NativeByteArray( compressedImageLength, compressedImage );
}

// Saves a DDS image in memory to disk (usually used after a compression)
void ImageFile::DDSSaveFromMemory( NativeByteArray^ _DDSImage, System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	ImageUtilityLib::ImageFile::DDSSaveFromMemory( _DDSImage->Length, _DDSImage->AsBytePointer.ToPointer(), nativeFileName );
}
void ImageFile::DDSSaveFromMemory( NativeByteArray^ _DDSImage, System::IO::Stream^ _imageStream ) {
	_imageStream->Write( _DDSImage->AsByteArray, 0, _DDSImage->Length );
}

// Cube map handling
cli::array< ImageFile^ >^	ImageFile::DDSLoadCubeMap( System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

	// Call native method
	U32							imagesCount;
	ImageUtilityLib::ImageFile*	images;
	ImageUtilityLib::ImageFile::DDSLoadCubeMapFile( nativeFileName, imagesCount, images );

	return WrapNativeImages( imagesCount, images, true );

// Or call streamed version
// 	System::IO::FileStream^	S = _fileName->OpenRead();
// 	cli::array< ImageFile^ >^	result = DDSLoadCubeMap( S );
// 	delete S;
// 	return result;
}
cli::array< ImageFile^ >^	ImageFile::DDSLoadCubeMap( System::IO::Stream^ _imageStream ) {
	// Load stream into memory
	cli::array< Byte >^	fileContent = gcnew cli::array< Byte >( int( _imageStream->Length ) );
	_imageStream->Read( fileContent, 0, int( _imageStream->Length ) );

	NativeByteArray^	temp = gcnew NativeByteArray( fileContent );

	// Call native method
	U32							imagesCount;
	ImageUtilityLib::ImageFile*	images;
	ImageUtilityLib::ImageFile::DDSLoadCubeMapMemory( temp->Length, temp->AsBytePointer.ToPointer(), imagesCount, images );

	// Release native memory
	delete temp;

	return WrapNativeImages( imagesCount, images, true );
}
void	ImageFile::DDSSaveCubeMap( cli::array< ImageFile^ >^ _cubeMapFaces, bool _compressBC6H, System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

	const ImageUtilityLib::ImageFile**	nativeCubeMapFaces = new const ImageUtilityLib::ImageFile*[_cubeMapFaces->Length];
	for ( int i=0; i < _cubeMapFaces->Length; i++ )
		nativeCubeMapFaces[i] = _cubeMapFaces[i]->m_nativeObject;

	ImageUtilityLib::ImageFile::DDSSaveCubeMapFile( _cubeMapFaces->Length, nativeCubeMapFaces, _compressBC6H, nativeFileName );

	delete[] nativeCubeMapFaces;
}
void	ImageFile::DDSSaveCubeMap( cli::array< ImageFile^ >^ _cubeMapFaces, bool _compressBC6H, System::IO::Stream^ _imageStream ) {
//	System::Runtime::InteropServices::Marshal::AllocHGlobal( _cubeMapFaces->Length * Sizeof* ); ??
	const ImageUtilityLib::ImageFile**	nativeCubeMapFaces = new const ImageUtilityLib::ImageFile*[_cubeMapFaces->Length];
	for ( int i=0; i < _cubeMapFaces->Length; i++ )
		nativeCubeMapFaces[i] = _cubeMapFaces[i]->m_nativeObject;

	// Call native method
	void*	fileContent = nullptr;
	U32		fileLength = 0;
	ImageUtilityLib::ImageFile::DDSSaveCubeMapMemory( _cubeMapFaces->Length, nativeCubeMapFaces, _compressBC6H, fileLength, fileContent );

	delete[] nativeCubeMapFaces;

	// Write to stream
	NativeByteArray^	temp = gcnew NativeByteArray( fileLength, fileContent );
	_imageStream->Write( temp->AsByteArray, 0, temp->Length );
	delete temp;
}

// 3D Texture handling
cli::array< ImageFile^ >^	ImageFile::DDSLoad3DTexture( System::IO::FileInfo^ _fileName, U32& _slicesCount ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

	// Call native method
	U32							imagesCount;
	ImageUtilityLib::ImageFile*	images;
	ImageUtilityLib::ImageFile::DDSLoad3DTextureFile( nativeFileName, imagesCount, images );

	return WrapNativeImages( imagesCount, images, true );

// Or call streamed version
// 	System::IO::FileStream^	S = _fileName->OpenRead();
// 	cli::array< ImageFile^ >^	result = DDSLoadCubeMap( S );
// 	delete S;
// 	return result;
}
cli::array< ImageFile^ >^	ImageFile::DDSLoad3DTexture( System::IO::Stream^ _imageStream ) {
	// Load stream into memory
	cli::array< Byte >^	fileContent = gcnew cli::array< Byte >( int( _imageStream->Length ) );
	_imageStream->Read( fileContent, 0, int( _imageStream->Length ) );

	NativeByteArray^	temp = gcnew NativeByteArray( fileContent );

	// Call native method
	U32							imagesCount;
	ImageUtilityLib::ImageFile*	images;
	ImageUtilityLib::ImageFile::DDSLoad3DTextureMemory( temp->Length, temp->AsBytePointer.ToPointer(), imagesCount, images );

	delete temp;

	return WrapNativeImages( imagesCount, images, true );
}
void	ImageFile::DDSSave3DTexture( cli::array< ImageFile^ >^ _slices, bool _compressBC6H, System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

	const ImageUtilityLib::ImageFile**	nativeSlices = new const ImageUtilityLib::ImageFile*[_slices->Length];
	for ( int i=0; i < _slices->Length; i++ )
		nativeSlices[i] = _slices[i]->m_nativeObject;

	ImageUtilityLib::ImageFile::DDSSave3DTextureFile( _slices->Length, nativeSlices, _compressBC6H, nativeFileName );

	delete[] nativeSlices;
}
void	ImageFile::DDSSave3DTexture( cli::array< ImageFile^ >^ _slices, bool _compressBC6H, System::IO::Stream^ _imageStream ) {
	const ImageUtilityLib::ImageFile**	nativeSlices = new const ImageUtilityLib::ImageFile*[_slices->Length];
	for ( int i=0; i < _slices->Length; i++ )
		nativeSlices[i] = _slices[i]->m_nativeObject;

	void*	fileContent = nullptr;
	U32		fileLength = 0;
	ImageUtilityLib::ImageFile::DDSSaveCubeMapMemory( _slices->Length, nativeSlices, _compressBC6H, fileLength, fileContent );

	delete[] nativeSlices;

	// Copy to Byte[]
	cli::array< Byte >^	managedBuffer = gcnew cli::array< Byte >( fileLength );
	System::Runtime::InteropServices::Marshal::Copy( IntPtr(fileContent), managedBuffer, 0, fileLength );

	// Write to stream
	_imageStream->Write( managedBuffer, 0, fileLength );
}
