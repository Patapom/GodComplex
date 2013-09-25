// That code is mainly inspired by the ImportScene example in the FBX SDK
//
//
#pragma managed
#pragma once

using namespace System;
using namespace System::Collections::Generic;

namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// Start/Stop time span => Contains 2 time spans to be read from absolute time 0
	//
	public ref class		FTimeSpan
	{
	protected:	// FIELDS

		TimeSpan			m_Start;
		TimeSpan			m_Stop;

	public:		// PROPERTIES

		property TimeSpan		Start
		{
			TimeSpan	get() { return m_Start; }
		}

		property TimeSpan		Stop
		{
			TimeSpan	get() { return m_Stop; }
		}


	public:		// METHODS

		FTimeSpan( TimeSpan _Start, TimeSpan _Stop )
		{
			m_Start = _Start;
			m_Stop = _Stop;
		}
	};


	//////////////////////////////////////////////////////////////////////////
	// General helpers for types conversion
	//
	public ref class	Helpers
	{
	public:

		//////////////////////////////////////////////////////////////////////////
		// HELPER METHODS

		static const char*		FromString( String^ _String )
		{
			return	(const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _String ).ToPointer();
		}

		static System::String^	GetString( FbxString* _pString )
		{
			return	GetString( _pString->Buffer() );
		}

		static System::String^	GetString( const char* _pString )
		{
			return	System::Runtime::InteropServices::Marshal::PtrToStringAnsi( System::IntPtr( (void*) _pString ) );
		}

		static WMath::Point2D^	ToPoint2( FbxDouble2& _Value )
		{
			return gcnew WMath::Point2D( (float) _Value[0], (float) _Value[1] );
		}

		static WMath::Vector2D^	ToVector2( FbxDouble2& _Value )
		{
			return gcnew WMath::Vector2D( (float) _Value[0], (float) _Value[1] );
		}

		static WMath::Point^	ToPoint3( FbxDouble3& _Value )
		{
			return gcnew WMath::Point( (float) _Value[0], (float) _Value[1], (float) _Value[2] );
		}

		static WMath::Vector^	ToVector3( FbxDouble3& _Value )
		{
			return gcnew WMath::Vector( (float) _Value[0], (float) _Value[1], (float) _Value[2] );
		}

		static WMath::Point4D^	ToPoint4( FbxDouble4& _Value )
		{
			return gcnew WMath::Point4D( (float) _Value[0], (float) _Value[1], (float) _Value[2], (float) _Value[3] );
		}

		static WMath::Vector4D^	ToVector4( FbxDouble4& _Value )
		{
			return gcnew WMath::Vector4D( (float) _Value[0], (float) _Value[1], (float) _Value[2], (float) _Value[3] );
		}

		static WMath::Vector4D^	ToVector4( FbxColor& _Value )
		{
			return gcnew WMath::Vector4D( (float) _Value.mRed, (float) _Value.mGreen, (float) _Value.mBlue, (float) _Value.mAlpha );
		}

		static WMath::Matrix4x4^	ToMatrix( FbxVector4& _P, FbxVector4& _R, FbxVector4& _S )
		{
			return	ToMatrix( FbxMatrix( _P, _R, _S ) );
		}

		static WMath::Matrix4x4^	ToMatrix( FbxMatrix& _Value )
		{
			WMath::Matrix4x4^	Result = gcnew WMath::Matrix4x4();
								Result->SetRow0( ToVector4( _Value.GetRow( 0 ) ) );
								Result->SetRow1( ToVector4( _Value.GetRow( 1 ) ) );
								Result->SetRow2( ToVector4( _Value.GetRow( 2 ) ) );
								Result->SetTrans( ToPoint4( _Value.GetRow( 3 ) ) );

			return	Result;
		}

		static FTimeSpan^		GetTimeSpan( FbxTimeSpan& _TimeSpan )
		{
			return gcnew FTimeSpan(	System::TimeSpan::FromMilliseconds( (double) _TimeSpan.GetStart().GetMilliSeconds() ),
									System::TimeSpan::FromMilliseconds( (double) _TimeSpan.GetStop().GetMilliSeconds() ) );
		}
	};
}
