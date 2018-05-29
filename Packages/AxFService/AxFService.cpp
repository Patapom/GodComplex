// This is the main DLL file.

#include "stdafx.h"

#include "AxFService.h"

using namespace AxFService;
using namespace axf::decoding;

AxFFile::AxFFile( System::IO::FileInfo^ _fileName ) {
	if ( !_fileName->Exists )
		throw gcnew System::IO::FileNotFoundException( "File \"" + _fileName + "\" not found!" );
	pin_ptr< const wchar_t >	nativeFileName = PtrToStringChars( _fileName->FullName );
	m_hFile = axfOpenFileW( nativeFileName, true, true );
}
AxFFile::~AxFFile() {
	AXF_FILE_HANDLE	temp = m_hFile;
	axfCloseFile( &temp );
}

UInt32	AxFFile::MaterialsCount::get() {
	return axfGetNumberOfMaterials( m_hFile );
}

AxFFile::Material^	AxFFile::default::get( UInt32 _materialIndex ) {
	Material^	result = gcnew Material( this, _materialIndex );
	return result;
}

String^	GetTypeKey( AXF_REPRESENTATION_HANDLE _handle ) {
	char	charPtr[AXF_MAX_KEY_SIZE];
	axfGetRepresentationTypeKey( _handle, charPtr, AXF_MAX_KEY_SIZE );
	String^	key = gcnew String( charPtr );
	return key;
}

AxFFile::Material::Material( AxFFile^ _owner, UInt32 _materialIndex ) : m_owner( _owner ) {
	m_hMaterial = axfGetMaterial( m_owner->m_hFile, _materialIndex );

	wchar_t	nativeMatName[1024];
	axfGetMaterialDisplayName( m_hMaterial, nativeMatName, 1024 );
	m_name = gcnew String( nativeMatName );

	// Get material representation
	m_hMaterialRepresentation = axfGetPreferredRepresentation( m_hMaterial );

	char	repClassCharPtr[AXF_MAX_KEY_SIZE];
	axfGetRepresentationClass( m_hMaterialRepresentation, repClassCharPtr, AXF_MAX_KEY_SIZE );
	String^	repClass = gcnew String( repClassCharPtr );
	if ( repClass == "SVBRDF" ) {
		// Read back the diffuse & specular types
		m_type = TYPE::SVBRDF;
		AXF_REPRESENTATION_HANDLE	hDiff = axfGetSvbrdfDiffuseModelRepresentation( m_hMaterialRepresentation );
		String^	diffuseKey = GetTypeKey( hDiff );
		if ( diffuseKey == "com.xrite.LambertDiffuseModel" ) {
			m_diffuseType = SVBRDF_DIFFUSE_TYPE::LAMBERT;
		} else if (diffuseKey == "com.xrite.OrenNayarDiffuseModel" ) {
			m_diffuseType = SVBRDF_DIFFUSE_TYPE::OREN_NAYAR;
		} else {
			throw gcnew Exception( "Unsupported diffuse type!" );
		}

		AXF_REPRESENTATION_HANDLE	hSpec = axfGetSvbrdfSpecularModelRepresentation( m_hMaterialRepresentation );
		String^	specularKey = GetTypeKey( hSpec );
		if ( specularKey == "com.xrite.WardSpecularModel" ) {
			m_specularType = SVBRDF_SPECULAR_TYPE::WARD;
		} else if ( specularKey == "com.xrite.BlinnPhongSpecularModel" ) {
			m_specularType = SVBRDF_SPECULAR_TYPE::BLINN_PHONG;
		} else if ( specularKey == "com.xrite.CookTorranceSpecularModel" ) {
			m_specularType = SVBRDF_SPECULAR_TYPE::COOK_TORRANCE;
		} else if ( specularKey == "com.xrite.GGXSpecularModel" ) {
			m_specularType = SVBRDF_SPECULAR_TYPE::GGX;
		} else if ( specularKey == "com.xrite.PhongSpecularModel" ) {
			m_specularType = SVBRDF_SPECULAR_TYPE::PHONG;
		} else {
			throw gcnew Exception( "Unsupported specular type!" );
		}
	} else if ( repClass == "CarPaint" || repClass == "CarPaint2" ) {
		m_type = TYPE::CARPAINT;
		throw gcnew Exception( "HANDLE THIS!" );
	} else if ( repClass == "FactorizedBTF" ) {
		m_type = TYPE::BTF;
		throw gcnew Exception( "HANDLE THIS!" );
	} else if ( repClass == "Layered" ) {
		throw gcnew Exception( "HANDLE THIS!" );
	} else {
		throw gcnew Exception( "Unsupported material class type!" );
	}
}
