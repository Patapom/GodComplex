#include "stdafx.h"

#include "ImageFile.h"

using namespace ImageUtility;

// Creates a bitmap from a System::Drawing.Bitmap and a color profile
ImageFile::ImageFile( System::Drawing::Bitmap^ _bitmap, ImageUtility::ColorProfile^ _colorProfile ) {
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
	SAFE_DELETE( m_nativeObject );
}

void	ImageFile::Init( U32 _width, U32 _height, PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile ) {
	m_nativeObject->Init( _width, _height, ImageUtilityLib::ImageFile::PIXEL_FORMAT( _format ), *_colorProfile->m_nativeObject );
}

void	ImageFile::Exit() {
	m_nativeObject->Exit();
}

// Load from a file or memory
void	ImageFile::Load( System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_nativeObject->Load( nativeFileName );
}
void	ImageFile::Load( System::IO::FileInfo^ _fileName, FILE_FORMAT _format ) {
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
	Load( imageContent, _format );
}
void	ImageFile::Load( cli::array< Byte >^ _fileContent, FILE_FORMAT _format ) {
	// Copy to Byte*
	IntPtr	nativeBuffer = System::Runtime::InteropServices::Marshal::AllocHGlobal( _fileContent->Length );
	System::Runtime::InteropServices::Marshal::Copy( _fileContent, 0, nativeBuffer, _fileContent->Length );

	// Call native method
	m_nativeObject->Load( nativeBuffer.ToPointer(), _fileContent->Length, ImageUtilityLib::ImageFile::FILE_FORMAT( _format ) );

	System::Runtime::InteropServices::Marshal::FreeHGlobal( nativeBuffer );
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
	cli::array< Byte >^	temp = Save( _format, _options );

	// Dump to stream
	_imageStream->Write( temp, 0, temp->Length );
}
cli::array< Byte >^	ImageFile::Save( FILE_FORMAT _format, SAVE_FLAGS _options ) {
	// Save to native memory
	void*	nativeBuffer = nullptr;
	U64		nativeBufferSize = 0;
	m_nativeObject->Save( ImageUtilityLib::ImageFile::FILE_FORMAT( _format ), ImageUtilityLib::ImageFile::SAVE_FLAGS( _options ), nativeBuffer, nativeBufferSize );

	// Copy to managed array
	cli::array< Byte >^ fileContent = gcnew cli::array< Byte >( nativeBufferSize );
	System::Runtime::InteropServices::Marshal::Copy( IntPtr( nativeBuffer ), fileContent, 0, nativeBufferSize );

	SAFE_DELETE( nativeBuffer );

	return fileContent;
}

// Converts the source image to a target format
void	ImageFile::ConvertFrom( ImageFile^ _source, PIXEL_FORMAT _targetFormat ) {

}

// Retrieves the image file type based on the image file name
ImageFile::FILE_FORMAT	ImageFile::GetFileType( System::IO::FileInfo^ _fileName ) {
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	return FILE_FORMAT( ImageUtilityLib::ImageFile::GetFileType( nativeFileName ) );
}
