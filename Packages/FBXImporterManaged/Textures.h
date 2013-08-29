// Contains Material classes
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "BaseObject.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// Represents a texture
	//
	public ref class		Texture : public BaseObject
	{
	public:		// NESTED TYPES

		enum class	WRAP_MODE
		{
			REPEAT,
			CLAMP
		};

		enum class	BLEND_MODE
		{
			TRANSLUCENT,
			ADDITIVE,
			MODULATE,
			MODULATE_2X
		};

		enum class	MAPPING_TYPE
		{
			NULL_,
			PLANAR,
			SPHERICAL,
			CYLINDRICAL,
			BOX,
			FACE,
			UV,
			ENVIRONMENT
		};

		enum class	TEXTURE_USAGE
		{
			STANDARD,
			SHADOW_MAP,
			LIGHT_MAP,
			SPHERICAL_REFLEXION_MAP,
			SPHERE_REFLEXION_MAP,
			BUMP_NORMAL_MAP
		};


	protected:	// FIELDS

		String^					m_Name;				// Texture name

		WRAP_MODE				m_WrapModeU;
		WRAP_MODE				m_WrapModeV;
		BLEND_MODE				m_BlendMode;
		MAPPING_TYPE			m_MappingType;
		TEXTURE_USAGE			m_TextureUsage;
		WMath::Vector^			m_Translation;
		WMath::Vector^			m_Rotation;
		WMath::Vector^			m_Scale;
		bool					m_bUseMipMap;
		String^					m_UVSet;
		String^					m_RelativeFileName;
		String^					m_AbsoluteFileName;


	public:		// PROPERTIES

		[DescriptionAttribute( "Gets texture sampler wrap mode on U" )]
		//
		property WRAP_MODE		WrapModeU
		{
			WRAP_MODE	get()	{ return m_WrapModeU; }
		}

		[DescriptionAttribute( "Gets texture sampler wrap mode on V" )]
		//
		property WRAP_MODE		WrapModeV
		{
			WRAP_MODE	get()	{ return m_WrapModeV; }
		}

		[DescriptionAttribute( "Gets texture blend mode" )]
		//
		property BLEND_MODE		BlendMode
		{
			BLEND_MODE	get()	{ return m_BlendMode; }
		}

		[DescriptionAttribute( "Gets the type of mapping of this texture (e.g. Planar, Box, etc.)" )]
		//
		property MAPPING_TYPE	MappingType
		{
			MAPPING_TYPE	get()	{ return m_MappingType; }
		}

		[DescriptionAttribute( "Gets the texture usage (e.g. Shadow Map, Light Map, etc.)" )]
		//
		property TEXTURE_USAGE	TextureUsage
		{
			TEXTURE_USAGE	get()	{ return m_TextureUsage; }
		}

		[DescriptionAttribute( "Gets the texture translation" )]
		//
		property WMath::Vector^	Translation
		{
			WMath::Vector^	get()	{ return m_Translation; }
		}

		[DescriptionAttribute( "Gets the texture rotation" )]
		//
		property WMath::Vector^	Rotation
		{
			WMath::Vector^	get()	{ return m_Rotation; }
		}

		[DescriptionAttribute( "Gets the texture scale" )]
		//
		property WMath::Vector^	Scale
		{
			WMath::Vector^	get()	{ return m_Scale; }
		}

		[DescriptionAttribute( "Tells if the texture should be using mip-mapping" )]
		//
		property bool			UseMipMap
		{
			bool			get()	{ return m_bUseMipMap; }
		}

		[DescriptionAttribute( "Gets the name of the UV set" )]
		//
		property String^		UVSet
		{
			String^			get()	{ return m_UVSet; }
		}

		[DescriptionAttribute( "Gets the texture's relative filename (i.e. relative to the exported FBX file's location)" )]
		//
		property String^		RelativeFileName
		{
			String^			get()	{ return m_RelativeFileName; }
		}

		[DescriptionAttribute( "Gets the texture's absolute filename" )]
		//
		property String^		AbsoluteFileName
		{
			String^			get()	{ return m_AbsoluteFileName; }
		}


	public:		// METHODS

		Texture( Scene^ _ParentScene, FbxTexture* _pTexture ) : BaseObject( _ParentScene, _pTexture )
		{
			m_Name = Helpers::GetString( _pTexture->GetName() );

			m_WrapModeU = static_cast<WRAP_MODE>( _pTexture->WrapModeU.Get() );
			m_WrapModeV = static_cast<WRAP_MODE>( _pTexture->WrapModeV.Get() );
			m_BlendMode = static_cast<BLEND_MODE>( _pTexture->CurrentTextureBlendMode.Get() );
			m_MappingType = static_cast<MAPPING_TYPE>( _pTexture->GetMappingType() );
			m_TextureUsage = static_cast<TEXTURE_USAGE>( _pTexture->GetTextureUse() );
			m_Translation = Helpers::ToVector3( _pTexture->Translation.Get() );
			m_Rotation = Helpers::ToVector3( _pTexture->Rotation.Get() );
			m_Scale = Helpers::ToVector3( _pTexture->Scaling.Get() );
//			m_bUseMipMap = _pTexture->UseMipMap.Get();
			m_UVSet = Helpers::GetString( _pTexture->UVSet.Get() );
//			m_RelativeFileName = Helpers::GetString( _pTexture->GetRelativeFileName() );
//			m_AbsoluteFileName = Helpers::GetString( _pTexture->GetFileName() );
		}
	};
}
