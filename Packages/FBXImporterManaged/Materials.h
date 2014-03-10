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

		[DescriptionAttribute( "Gets emissive color" )]
		//
		property WMath::Vector^		EmissiveColor
		{
			WMath::Vector^		get()	{ return FindProperty( "EmissiveColor" )->AsVector3; }
		}

		[DescriptionAttribute( "Gets emissive factor" )]
		//
		property float				EmissiveFactor
		{
			float				get()	{ return FindProperty( "EmissiveFactor" )->AsFloat; }
		}

		[DescriptionAttribute( "Gets ambient color" )]
		//
		property WMath::Vector^		AmbientColor
		{
			WMath::Vector^		get()	{ return FindProperty( "AmbientColor" )->AsVector3; }
		}

		[DescriptionAttribute( "Gets ambient factor" )]
		//
		property float				AmbientFactor
		{
			float				get()	{ return FindProperty( "AmbientFactor" )->AsFloat; }
		}

		[DescriptionAttribute( "Gets diffuse color" )]
		//
		property WMath::Vector^		DiffuseColor
		{
			WMath::Vector^		get()	{ return FindProperty( "DiffuseColor" )->AsVector3; }
		}

		[DescriptionAttribute( "Gets diffuse factor" )]
		//
		property float				DiffuseFactor
		{
			float				get()	{ return FindProperty( "DiffuseFactor" )->AsFloat; }
		}


		[DescriptionAttribute( "Gets specular color" )]
		//
		property WMath::Vector^		SpecularColor
		{
			WMath::Vector^		get()	{ return FindProperty( "SpecularColor" )->AsVector3; }
		}

		[DescriptionAttribute( "Gets specular factor" )]
		//
		property float				SpecularFactor
		{
			float				get()	{ return FindProperty( "SpecularFactor" )->AsFloat; }
		}

		[DescriptionAttribute( "Gets reflection color" )]
		//
		property WMath::Vector^		ReflectionColor
		{
			WMath::Vector^		get()	{ return FindProperty( "ReflectionColor" )->AsVector3; }
		}

		[DescriptionAttribute( "Gets reflection factor" )]
		//
		property float				ReflectionFactor
		{
			float				get()	{ return FindProperty( "ReflectionFactor" )->AsFloat; }
		}

		[DescriptionAttribute( "Gets specular shininess (i.e. specular power)" )]
		//
		property float				Shininess
		{
			float				get()	{ return FindProperty( "Shininess" )->AsFloat; }
		}

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
	public:		// METHODS

		MaterialLambert( Scene^ _ParentScene, FbxSurfaceLambert* _pMaterial ) : Material( _ParentScene, _pMaterial )
		{
		}
	};

	// Standard Lambert material
	//
	public ref class		MaterialPhong : public MaterialLambert
	{
	public:		// METHODS

		MaterialPhong( Scene^ _ParentScene, FbxSurfacePhong* _pMaterial ) : MaterialLambert( _ParentScene, _pMaterial )
		{
		}
	};
}
