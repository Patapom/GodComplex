// AxFService.h
#pragma once

using namespace System;

namespace AxFService {

	public ref class AxFFile {
	public:

		ref class	Material {
		public:

			enum class	TYPE {
				SVBRDF,
				BTF,		// Factorized Bidirectional Texture Function
				CARPAINT,
			};

			enum class	SVBRDF_DIFFUSE_TYPE {
				LAMBERT,
				OREN_NAYAR,
//				DISNEY,
			};

			enum class	SVBRDF_SPECULAR_TYPE {
				WARD,
				BLINN_PHONG,
				COOK_TORRANCE,
				GGX,
				PHONG,
			};

			enum class	SVBRDF_SPECULAR_VARIANT {
				// Ward variants
				GEISLERMORODER,		// 2010 (albedo-conservative, should always be preferred!)
				DUER,				// 2006
				WARD,				// 1992 (original paper)

				// Blinn-Phong variants
				ASHIKHMIN_SHIRLEY,	// 2000
				BLINN,				// 1977 (original paper)
				VRAY,
				LEWIS,				// 1993
			};

			enum class	SVBRDF_FRESNEL_VARIANT {
				NO_FRESNEL,			// No fresnel
				FRESNEL,			// Full fresnel (1818)
				SCHLICK,			// Schlick's Approximation (1994)
			};

			ref class Property {
			public:
				String^							m_name;
				Object^							m_value;
			};

			ref class Texture {
			private:

				String^							m_name;
				ImageUtility::COMPONENT_FORMAT	m_componentFormat;
				ImageUtility::ImagesMatrix^		m_images;

			public:

				property String^		Name {
					String^	get() { return m_name; }
				}
				property ImageUtility::COMPONENT_FORMAT	ComponentFormat {
					ImageUtility::COMPONENT_FORMAT	get() { return m_componentFormat; }
				}
				property ImageUtility::ImagesMatrix^	Images {
					ImageUtility::ImagesMatrix^	get() { return m_images; }
				}

			public:
				Texture( ::axf::decoding::TextureDecoder* _decoder, UInt32 _textureIndex );
				~Texture();
			};

		private:

			AxFFile^					m_owner;
			::axf::decoding::AXF_MATERIAL_HANDLE		m_hMaterial;
			::axf::decoding::AXF_REPRESENTATION_HANDLE	m_hMaterialRepresentation;
			String^						m_name;

			TYPE						m_type;
			SVBRDF_DIFFUSE_TYPE			m_diffuseType;
			SVBRDF_SPECULAR_TYPE		m_specularType;
			SVBRDF_SPECULAR_VARIANT		m_specularVariant;
			SVBRDF_FRESNEL_VARIANT		m_fresnelVariant;
			bool						m_isAnisotropic;

			cli::array< Texture^ >^		m_textures;
			cli::array< Property^ >^	m_properties;

		public:

			property String^					Name { String^ get() { return m_name; } }
			property TYPE						Type { TYPE get() { return m_type; } }
			property SVBRDF_DIFFUSE_TYPE		DiffuseType { SVBRDF_DIFFUSE_TYPE get() { return m_diffuseType; } }
			property SVBRDF_SPECULAR_TYPE		SpecularType { SVBRDF_SPECULAR_TYPE get() { return m_specularType; } }
			property SVBRDF_SPECULAR_VARIANT	SpecularVariant { SVBRDF_SPECULAR_VARIANT get() { return m_specularVariant; } }
			property SVBRDF_FRESNEL_VARIANT		FresnelVariant { SVBRDF_FRESNEL_VARIANT get() { return m_fresnelVariant; } }
			property bool						IsAnisotropic { bool get() { return m_isAnisotropic; } }

			property cli::array< Property^ >^	Properties { cli::array< Property^ >^ get(); }
			property cli::array< Texture^ >^	Textures { cli::array< Texture^ >^ get(); }

		public:
			Material( AxFFile^ _owner, UInt32 _materialIndex );

			System::String^	ToString() override { return Name; }

		private:
			void	ReadProperties();
			void	ReadTextures();
		};
		
	private:

		::axf::decoding::AXF_FILE_HANDLE	m_hFile;	// Opened file handle

	public:

		property UInt32		MaterialsCount { UInt32	get(); }
		property cli::array<Material^>^	Materials { cli::array<Material^>^	get(); }
		property Material^	default[UInt32] { Material^ get( UInt32 ); }

	public:

		AxFFile( System::IO::FileInfo^ _fileName );
		~AxFFile();
	};
}
