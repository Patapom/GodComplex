// Contains Material classes
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "BaseObject.h"

using namespace System;
using namespace System::Collections::Generic;


namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// Represents a material
	//
	public ref class		Material : public BaseObject
	{
	protected:	// FIELDS

	public:		// PROPERTIES

	public:		// METHODS

		Material( Scene^ _ParentScene, FbxSurfaceMaterial* _pMaterial ) : BaseObject( _ParentScene, _pMaterial )
		{
		}
	};

	// Default material
	//
	public ref class		MaterialDefault : public Material
	{
	public:

		MaterialDefault( Scene^ _ParentScene, String^ _Name ) : Material( _ParentScene, NULL )
		{
			m_Name = _Name;
		}
	};

	// Standard Lambert material
	//
	public ref class		MaterialLambert : public Material
	{
	protected:	// FIELDS

		FbxSurfaceLambert*		m_pLambert;

	public:		// PROPERTIES

		[DescriptionAttribute( "Gets emissive color" )]
		//
		property WMath::Point^		EmissiveColor
		{
			WMath::Point^		get()	{ return Helpers::ToPoint3( m_pLambert->Emissive.Get() ); }
		}

		[DescriptionAttribute( "Gets emissive factor" )]
		//
		property float				EmissiveFactor
		{
			float				get()	{ return (float) m_pLambert->EmissiveFactor.Get(); }
		}

		[DescriptionAttribute( "Gets ambient color" )]
		//
		property WMath::Point^		AmbientColor
		{
			WMath::Point^		get()	{ return Helpers::ToPoint3( m_pLambert->Ambient.Get() ); }
		}

		[DescriptionAttribute( "Gets ambient factor" )]
		//
		property float				AmbientFactor
		{
			float				get()	{ return (float) m_pLambert->AmbientFactor.Get(); }
		}

		[DescriptionAttribute( "Gets diffuse color" )]
		//
		property WMath::Point^		DiffuseColor
		{
			WMath::Point^		get()	{ return Helpers::ToPoint3( m_pLambert->Diffuse.Get() ); }
		}

		[DescriptionAttribute( "Gets diffuse factor" )]
		//
		property float				DiffuseFactor
		{
			float				get()	{ return (float) m_pLambert->DiffuseFactor.Get(); }
		}


	public:		// METHODS

		MaterialLambert( Scene^ _ParentScene, FbxSurfaceLambert* _pMaterial ) : Material( _ParentScene, _pMaterial ), m_pLambert( _pMaterial )
		{
		}
	};

	// Standard Lambert material
	//
	public ref class		MaterialPhong : public MaterialLambert
	{
	protected:	// FIELDS

		FbxSurfacePhong*		m_pPhong;

	public:		// PROPERTIES

		[DescriptionAttribute( "Gets specular color" )]
		//
		property WMath::Point^		SpecularColor
		{
			WMath::Point^		get()	{ return Helpers::ToPoint3( m_pPhong->Specular.Get() ); }
		}

		[DescriptionAttribute( "Gets specular factor" )]
		//
		property float				SpecularFactor
		{
			float				get()	{ return (float) m_pPhong->SpecularFactor.Get(); }
		}

		[DescriptionAttribute( "Gets reflection color" )]
		//
		property WMath::Point^		ReflectionColor
		{
			WMath::Point^		get()	{ return Helpers::ToPoint3( m_pPhong->Reflection.Get() ); }
		}

		[DescriptionAttribute( "Gets reflection factor" )]
		//
		property float				ReflectionFactor
		{
			float				get()	{ return (float) m_pPhong->ReflectionFactor.Get(); }
		}

		[DescriptionAttribute( "Gets specular shininess (i.e. specular power)" )]
		//
		property float				Shininess
		{
			float				get()	{ return (float) m_pPhong->Shininess.Get(); }
		}


	public:		// METHODS

		MaterialPhong( Scene^ _ParentScene, FbxSurfacePhong* _pMaterial ) : MaterialLambert( _ParentScene, _pMaterial ), m_pPhong( _pMaterial )
		{
		}
	};
}
