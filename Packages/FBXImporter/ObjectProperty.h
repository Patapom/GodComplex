// Contains the Object Property class attached to each object
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "AnimationTrack.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	ref class	BaseObject;
	ref class	Texture;

	//////////////////////////////////////////////////////////////////////////
	// Represents a property attached to an object
	//
	[System::Diagnostics::DebuggerDisplayAttribute( "Name={Name} Value={Value}" )]
	public ref class	ObjectProperty
	{
	protected:	// FIELDS

		BaseObject^						m_Owner;

		String^							m_Name;
		String^							m_InternalName;
		String^							m_TypeName;
		Object^							m_Value;

		cli::array<Texture^>^			m_Textures;

		cli::array<AnimationTrack^>^	m_AnimTracks;


	public:		// PROPERTIES

		property String^	Name
		{
			String^		get()	{ return m_Name; }
		}

		property String^	InternalName
		{
			String^		get()	{ return m_InternalName; }
		}

		property String^	TypeName
		{
			String^		get()	{ return m_TypeName; }
		}

		property Object^	Value
		{
			Object^		get()	{ return m_Value; }
		}

		property cli::array<Texture^>^	Textures
		{
			cli::array<Texture^>^	get()	{ return m_Textures; }
			void	set( cli::array<Texture^>^ value )	{ m_Textures = value; }
		}

		property cli::array<AnimationTrack^>^	AnimTracks
		{
			cli::array<AnimationTrack^>^	get()	{ return m_AnimTracks; }
		}


		// Fast cast
		property bool				AsBool		{ bool				get()	{ return (bool) m_Value; } }
		property int				AsInt		{ int				get()	{ return (int) m_Value; } }
		property float				AsFloat		{ float				get()	{ return (float) m_Value; } }
		property SharpMath::float3	AsVector3	{ SharpMath::float3	get()	{ return (SharpMath::float3) m_Value; } }
		property SharpMath::float4	AsVector4	{ SharpMath::float4	get()	{ return (SharpMath::float4) m_Value; } }
		property System::String^	AsString	{ System::String^	get()	{ return (System::String^) m_Value; } }


	public:		// METHODS

		ObjectProperty( BaseObject^ _Owner, FbxProperty& _Property );
	};
}
