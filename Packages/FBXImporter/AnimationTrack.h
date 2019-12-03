// Contains the Object Property class attached to each object
//
#pragma managed
#pragma once

#include "Helpers.h"

using namespace System;
using namespace System::Collections::Generic;


namespace FBXImporter
{
	ref class	BaseObject;
	ref class	Node;
	ref class	ObjectProperty;

	//////////////////////////////////////////////////////////////////////////
	// Represents a property attached to an object
	//
	[System::Diagnostics::DebuggerDisplayAttribute( "Name={Name} KeysCount={Keys.Length} ChildTracksCount={ChildTracks.Length} DefaultValue={m_Defaultvalue}" )]
	public ref class	AnimationTrack
	{
	public:		// NESTED TYPES

		[System::Diagnostics::DebuggerDisplayAttribute( "Time={Time} Value={Value} Type={Type}" )]
		ref class	AnimationKey
		{
		public:		// NESTED TYPES

			enum class	KEY_TYPE
			{
				CONSTANT,
				LINEAR,
				CUBIC
			};

			enum class	CUBIC_INTERPOLATION_TYPE
			{
				CARDINAL,				// Cardinal spline => slopes are built upon key values only
				TCB,					// Tension, Continuity, Bias
				CUSTOM					// Slopes are specified by the user
			};

		public:

			AnimationKey^	Previous;	// A link to the previous key
			AnimationKey^	Next;		// A link to the next key

			KEY_TYPE		Type;		// The type of used interpolation
			float			Time;		// The time of the key
			float			Value;		// The value of the key

			CUBIC_INTERPOLATION_TYPE	CubicType;	// The type of used cubic interpolation

			// These values are only relevant for CUSTOM cubic interpolation!
			float			RightSlope;
			float			NextLeftSlope;
			float			RightWeight;
			float			NextLeftWeight;
 			float			RightVelocity;
 			float			NextLeftVelocity;

			// These values are only relevant for TCB cubic interpolation!
			float			Tension;
			float			Continuity;
			float			Bias;
		};


	protected:	// FIELDS

		ObjectProperty^					m_Owner;
		FbxAnimCurve*					m_pAnimCurve;

		String^							m_Name;
		FTimeSpan^					m_TimeSpan;

		cli::array<AnimationKey^>^		m_Keys;


	public:		// PROPERTIES

		property String^		Name
		{
			String^		get()	{ return m_Name; }
		}

		property FTimeSpan^		TimeSpan
		{
			FTimeSpan^	get()	{ return m_TimeSpan; }
		}

		property cli::array<AnimationKey^>^	Keys
		{
			cli::array<AnimationKey^>^	get()	{ return m_Keys; }
		}


	public:		// METHODS

		AnimationTrack( ObjectProperty^ _Owner, FbxAnimCurve* _pAnimCurve );
		AnimationTrack( AnimationTrack^ _Source );

		float	Evaluate( float _Time );

		// Adds a value to all keys
		//
		void	AddValue( float _Value )
		{
			for ( int KeyIndex=0; KeyIndex < m_Keys->Length; KeyIndex++ )
			{
				AnimationKey^	Key = m_Keys[KeyIndex];

				Key->Value += _Value;
			}
		}

		// Applies the provided factor to every keys & their slopes
		//
		void	ApplyFactor( float _Factor )
		{
			for ( int KeyIndex=0; KeyIndex < m_Keys->Length; KeyIndex++ )
			{
				AnimationKey^	Key = m_Keys[KeyIndex];

				Key->Value *= _Factor;
				Key->RightSlope *= _Factor;
				Key->NextLeftSlope *= _Factor;
			}
		}
	};
}
