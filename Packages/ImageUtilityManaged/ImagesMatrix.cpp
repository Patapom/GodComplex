// This is the main DLL file.

#include "stdafx.h"

#include "ImagesMatrix.h"

namespace ImageUtility {

	//////////////////////////////////////////////////////////////////////////
	// DDS-related methods
	//
	COMPONENT_FORMAT	ImagesMatrix::DDSLoadFile( System::IO::FileInfo^ _fileName ) {
		if ( !_fileName->Exists )
			throw gcnew System::IO::FileNotFoundException( "File not found!", _fileName->FullName );

		pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

		ImageUtilityLib::COMPONENT_FORMAT	loadedFormat;
		m_nativeObject->DDSLoadFile( nativeFileName, loadedFormat );

		return (ImageUtility::COMPONENT_FORMAT) loadedFormat;
	}
	COMPONENT_FORMAT	ImagesMatrix::DDSLoadMemory( NativeByteArray^ _imageContent ) {
		ImageUtilityLib::COMPONENT_FORMAT	loadedFormat;
		m_nativeObject->DDSLoadMemory( _imageContent->Length, _imageContent->AsBytePointer.ToPointer(), loadedFormat );

		return (ImageUtility::COMPONENT_FORMAT) loadedFormat;
	}
	void	ImagesMatrix::DDSSaveFile( System::IO::FileInfo^ _fileName, COMPONENT_FORMAT _componentFormat ) {
		pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );

		m_nativeObject->DDSSaveFile( nativeFileName, BaseLib::COMPONENT_FORMAT( _componentFormat ) );
	}
	NativeByteArray^	ImagesMatrix::DDSSaveMemory( COMPONENT_FORMAT _componentFormat ) {
		// Generate native byte array
		U64		fileSize = 0;
		void*	fileContent = NULL;
		m_nativeObject->DDSSaveMemory( fileSize, fileContent, BaseLib::COMPONENT_FORMAT( _componentFormat ) );

		NativeByteArray^	result = gcnew NativeByteArray( int(fileSize), fileContent );
		return result;
	}

}

