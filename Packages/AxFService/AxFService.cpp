// This is the main DLL file.

#include "stdafx.h"

#include "AxFService.h"

using namespace AxFService;
using namespace axf::decoding;
using namespace ImageUtility;

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

AxFFile::Material::Material( AxFFile^ _owner, UInt32 _materialIndex ) : m_owner( _owner ) {
	m_hMaterial = axfGetMaterial( m_owner->m_hFile, _materialIndex );

	wchar_t	nativeMatName[1024];
	axfGetMaterialDisplayName( m_hMaterial, nativeMatName, 1024 );
	m_name = gcnew String( nativeMatName );

	// Get material representation
	m_hMaterialRepresentation = axfGetPreferredRepresentation( m_hMaterial );

	char	tempCharPtr[AXF_MAX_KEY_SIZE];
	axfGetRepresentationClass( m_hMaterialRepresentation, tempCharPtr, AXF_MAX_KEY_SIZE );
	String^	repClass = gcnew String( tempCharPtr );
	if ( repClass == "SVBRDF" ) {
		// Read back the diffuse & specular types
		m_type = TYPE::SVBRDF;
		AXF_REPRESENTATION_HANDLE	hDiff = axfGetSvbrdfDiffuseModelRepresentation( m_hMaterialRepresentation );
		axfGetRepresentationTypeKey( hDiff, tempCharPtr, AXF_MAX_KEY_SIZE );
		String^	diffuseKey = gcnew String( tempCharPtr );
		if ( diffuseKey == "com.xrite.LambertDiffuseModel" ) {
			m_diffuseType = SVBRDF_DIFFUSE_TYPE::LAMBERT;
		} else if (diffuseKey == "com.xrite.OrenNayarDiffuseModel" ) {
			m_diffuseType = SVBRDF_DIFFUSE_TYPE::OREN_NAYAR;
		} else {
			throw gcnew Exception( "Unsupported diffuse type!" );
		}

		AXF_REPRESENTATION_HANDLE	hSpec = axfGetSvbrdfSpecularModelRepresentation( m_hMaterialRepresentation );
		axfGetRepresentationTypeKey( hSpec, tempCharPtr, AXF_MAX_KEY_SIZE );
		String^	specularKey = gcnew String( tempCharPtr );
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

		// Read back precise specular variants
		bool	hasFresnel = false;
		bool	isAnisotropic = false;
		axfGetSvbrdfSpecularModelVariant( hSpec, tempCharPtr, AXF_MAX_KEY_SIZE, isAnisotropic, hasFresnel );
		m_isAnisotropic = isAnisotropic;
		String^	specularVariant = gcnew String( tempCharPtr );
		if ( specularVariant == "GeislerMoroder2010" ) {
			m_specularVariant = SVBRDF_SPECULAR_VARIANT::GEISLERMORODER;
		} else {
			throw gcnew Exception( "Unsupported specular variant!" );
		}

		m_fresnelVariant = SVBRDF_FRESNEL_VARIANT::NO_FRESNEL;
		if ( hasFresnel ) {
			axfGetSvbrdfSpecularFresnelVariant( hSpec, tempCharPtr, AXF_MAX_KEY_SIZE );
			String^	fresnelVariant = gcnew String( tempCharPtr );
			if ( fresnelVariant == "Schlick1994" ) {
				m_fresnelVariant = SVBRDF_FRESNEL_VARIANT::SCHLICK;
			} else if ( fresnelVariant == "Schlick1994" ) {
				m_fresnelVariant = SVBRDF_FRESNEL_VARIANT::FRESNEL;
			} else {
				throw gcnew Exception( "Unsupported fresnel type!" );
			}
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

cli::array< AxFFile::Material::Texture^ >^	AxFFile::Material::Textures::get() {
	if ( m_textures == nullptr ) {
		ReadTextures();
	}
	return m_textures;
}

void	AxFFile::Material::ReadTextures() {

	CPUDecoder*		pcl_decoder = CPUDecoder::create( m_hMaterialRepresentation, "sRGB,E", ORIGIN_TOPLEFT );
	TextureDecoder* pcl_tex_decoder = TextureDecoder::create( m_hMaterialRepresentation, pcl_decoder, ID_DEFAULT );

	m_textures = gcnew cli::array<Texture ^>( pcl_tex_decoder->getNumTextures() );
	if ( m_textures->Length > 0 ) {
		for ( UInt32 textureIndex=0; textureIndex < UInt32(m_textures->Length); textureIndex++ ) {
			Texture^	tex = gcnew Texture( pcl_tex_decoder, textureIndex );
			m_textures[textureIndex] = tex;
		}
	}

	pcl_tex_decoder->destroy();
	pcl_decoder->destroy();
}

AxFFile::Material::Texture::Texture( TextureDecoder* _decoder, UInt32 _textureIndex ) {

	char	tempCharPtr[255];
	_decoder->getTextureName( _textureIndex, tempCharPtr, 255 );
	m_name = gcnew String( tempCharPtr );

	// Retrieve mip 0 information
	int			width, height, depth, channelsCount, datatype_src;
	_decoder->getTextureSize( _textureIndex, 0, width, height, depth, channelsCount, datatype_src );
	UInt32		mipsCount = _decoder->getTextureNumMipLevels( _textureIndex );

	if ( depth > 1 )
		throw gcnew Exception( "Handle 3D textures or texture arrays!" );
	if ( channelsCount > 4 )
		throw gcnew Exception( "Handle textures with more than 4 channels! (or not)" );

	TextureType			textureDataFormat = TextureType(datatype_src);
	PIXEL_FORMAT		targetFormat;
	UInt32				componentSize;
	switch ( textureDataFormat ) {
		case TEXTURE_TYPE_BYTE:
			componentSize = 1;
			switch ( channelsCount ) {
				case 1:
					targetFormat = PIXEL_FORMAT::R8;
					m_componentFormat = COMPONENT_FORMAT::UNORM;
					break;
				case 2:
					targetFormat = PIXEL_FORMAT::RG8;
					m_componentFormat = COMPONENT_FORMAT::UNORM;
					break;
				case 3:
				case 4:
					targetFormat = PIXEL_FORMAT::RGBA8;
					m_componentFormat = COMPONENT_FORMAT::UNORM_sRGB;
					break;
			}
			break;
		case TEXTURE_TYPE_HALF:
			componentSize = 2;
			switch ( channelsCount ) {
				case 1:
					targetFormat = PIXEL_FORMAT::R16F;
					m_componentFormat = COMPONENT_FORMAT::AUTO;
					break;
				case 2:
					targetFormat = PIXEL_FORMAT::RG16F;
					m_componentFormat = COMPONENT_FORMAT::AUTO;
					break;
				case 3:
				case 4:
					targetFormat = PIXEL_FORMAT::RGBA16F;
					m_componentFormat = COMPONENT_FORMAT::AUTO;
					break;
			}
			break;
		case TEXTURE_TYPE_FLOAT:
			componentSize = 4;
			switch ( channelsCount ) {
				case 1:
					targetFormat = PIXEL_FORMAT::R32F;
					m_componentFormat = COMPONENT_FORMAT::AUTO;
					break;
				case 2:
					targetFormat = PIXEL_FORMAT::RG32F;
					m_componentFormat = COMPONENT_FORMAT::AUTO;
					break;
				case 3:
				case 4:
					targetFormat = PIXEL_FORMAT::RGBA32F;
					m_componentFormat = COMPONENT_FORMAT::AUTO;
					break;
			}
			break;

// 		default:
// 			throw gcnew Exception( "Unsupported pixel format!" );
	}

	// Allocate images
	m_images = gcnew ImagesMatrix();
	m_images->InitTexture2DArray( UInt32(width), UInt32(height), 1, mipsCount );
	m_images->AllocateImageFiles( targetFormat, gcnew ImageUtility::ColorProfile( ColorProfile::STANDARD_PROFILE::sRGB ) );

	UInt32	sourcePixelSize = channelsCount * componentSize;
	UInt32	targetPixelSize = channelsCount != 3 ? sourcePixelSize : 4 * componentSize;	// Force 4 channels

	Byte*	tempPixelsBuffer = new Byte[width*height*sourcePixelSize];

	// Readback all mip levels
	for ( UInt32 mipIndex=0; mipIndex < mipsCount; mipIndex++ ) {
		_decoder->getTextureSize( _textureIndex, mipIndex, width, height, depth, channelsCount, datatype_src );
		_decoder->getTextureData( _textureIndex, mipIndex, textureDataFormat, tempPixelsBuffer );

		UInt32	sourcePitch = width * sourcePixelSize;

		// Write to our target image
		ImageFile^	targetMip = m_images[0][mipIndex][0];
		if ( targetMip->Height != height )
			throw gcnew Exception( "Target mip level isn't the same resolution as source mip level!" );

		UInt32	targetPitch = targetMip->Pitch;
		Byte*	sourcePtr = tempPixelsBuffer;
		Byte*	targetPtr = (Byte*) (void*) targetMip->Bits;
		for ( UInt32 Y=0; Y < UInt32(height); Y++, sourcePtr+=sourcePitch, targetPtr+=targetPitch ) {
			if ( channelsCount != 3 ) {
				memcpy_s( targetPtr, targetPitch, sourcePtr, sourcePitch );
				continue;
			}

			// We need to copy RGB and create our own alpha
			for ( UInt32 X=0; X < UInt32(width); X++ ) {
				memcpy_s( targetPtr + targetPixelSize*X, targetPixelSize, sourcePtr + sourcePixelSize*X, sourcePixelSize );

//((float*) (targetPtr + targetPixelSize*X))[0] = 1;
//((float*) (targetPtr + targetPixelSize*X))[1] = 0;
//((float*) (targetPtr + targetPixelSize*X))[2] = 0;

				// Fill opaque alpha
				switch ( targetFormat ) {
					case PIXEL_FORMAT::RGBA8: ((Byte*)(targetPtr + targetPixelSize*X))[3] = 0xFF; break;
					case PIXEL_FORMAT::RGBA16F: ((UInt16*)(targetPtr + targetPixelSize*X))[3] = 0x3C00; break;	// Representation for 1
					case PIXEL_FORMAT::RGBA32F: ((float*)(targetPtr + targetPixelSize*X))[3] = 1.0f; break;
				}
			}
		}
	}

	delete[] tempPixelsBuffer;
}
AxFFile::Material::Texture::~Texture() {
	delete m_images;
}
