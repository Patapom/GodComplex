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
			TRANSPARENT,
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
		String^					m_RelativeFileName;
		String^					m_AbsoluteFileName;


	public:		// PROPERTIES

		[DescriptionAttribute( "Gets texture sampler wrap mode on U" )]
		//
		property WRAP_MODE		WrapModeU
		{
			WRAP_MODE	get()	{ return static_cast<WRAP_MODE>( m_Properties->Count > 0 ? FindProperty( "WrapModeU" )->AsInt : 0 ); }
		}

		[DescriptionAttribute( "Gets texture sampler wrap mode on V" )]
		//
		property WRAP_MODE		WrapModeV
		{
			WRAP_MODE	get()	{ return static_cast<WRAP_MODE>( m_Properties->Count > 0 ? FindProperty( "WrapModeV" )->AsInt : 0 ); }
		}

		[DescriptionAttribute( "Gets texture blend mode" )]
		//
		property BLEND_MODE		BlendMode
		{
			BLEND_MODE	get()	{ return static_cast<BLEND_MODE>( m_Properties->Count > 0 ? FindProperty( "CurrentTextureBlendMode" )->AsInt : 0 ); }
		}

		[DescriptionAttribute( "Gets the type of mapping of this texture (e.g. Planar, Box, etc.)" )]
		//
		property MAPPING_TYPE	MappingType
		{
			MAPPING_TYPE	get()	{ return static_cast<MAPPING_TYPE>( m_Properties->Count > 0 ? FindProperty( "CurrentMappingType" )->AsInt : 0 ); }
		}

		[DescriptionAttribute( "Gets the texture usage (e.g. Shadow Map, Light Map, etc.)" )]
		//
		property TEXTURE_USAGE	TextureUsage
		{
			TEXTURE_USAGE	get()	{ return static_cast<TEXTURE_USAGE>( m_Properties->Count > 0 ? FindProperty( "TextureTypeUse" )->AsInt : 0 ); }
		}

		[DescriptionAttribute( "Gets the texture translation" )]
		//
		property WMath::Vector^	Translation
		{
			WMath::Vector^	get()	{ return m_Properties->Count > 0 ? FindProperty( "Translation" )->AsVector3 : WMath::Vector::Zero; }
		}

		[DescriptionAttribute( "Gets the texture rotation" )]
		//
		property WMath::Vector^	Rotation
		{
			WMath::Vector^	get()	{ return m_Properties->Count > 0 ? FindProperty( "Rotation" )->AsVector3 : WMath::Vector::Zero; }
		}

		[DescriptionAttribute( "Gets the texture scale" )]
		//
		property WMath::Vector^	Scale
		{
			WMath::Vector^	get()	{ return m_Properties->Count > 0 ? FindProperty( "Scaling" )->AsVector3 : WMath::Vector::One; }
		}

		[DescriptionAttribute( "Tells if the texture should be using mip-mapping" )]
		//
		property bool			UseMipMap
		{
			bool			get()	{ return m_Properties->Count > 0 ? FindProperty( "UseMipMap" )->AsBool : true; }
		}

		[DescriptionAttribute( "Gets the name of the UV set" )]
		//
		property String^		UVSet
		{
			String^			get()	{ return m_Properties->Count > 0 ? FindProperty( "UVSet" )->AsString : ""; }
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

		Texture( Scene^ _ParentScene, FbxFileTexture* _pTexture ) : BaseObject( _ParentScene, _pTexture )
		{
			m_Name = Helpers::GetString( _pTexture->GetName() );
			m_RelativeFileName = Helpers::GetString( _pTexture->GetRelativeFileName() );
			m_AbsoluteFileName = Helpers::GetString( _pTexture->GetFileName() );
		}

		Texture( Scene^ _ParentScene, System::String^ _TextureName, System::String^ _TextureRelativeFileName, System::String^ _AbsoluteFileName ) : BaseObject( _ParentScene, nullptr )
		{
			m_Name = _TextureName;
			m_RelativeFileName = _TextureRelativeFileName;
			m_AbsoluteFileName = _AbsoluteFileName;
		}
	};
}
