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

		static SharpMath::float2	ToPoint2( FbxDouble2& _Value )
		{
			return SharpMath::float2( (float) _Value[0], (float) _Value[1] );
		}

		static SharpMath::float2	ToVector2( FbxDouble2& _Value )
		{
			return SharpMath::float2( (float) _Value[0], (float) _Value[1] );
		}

		static SharpMath::float3	ToPoint3( FbxDouble3& _Value )
		{
			return SharpMath::float3( (float) _Value[0], (float) _Value[1], (float) _Value[2] );
		}

		static SharpMath::float3	ToVector3( FbxDouble3& _Value )
		{
			return SharpMath::float3( (float) _Value[0], (float) _Value[1], (float) _Value[2] );
		}

		static SharpMath::float4	ToPoint4( FbxDouble4& _Value )
		{
			return SharpMath::float4( (float) _Value[0], (float) _Value[1], (float) _Value[2], (float) _Value[3] );
		}

		static SharpMath::float4	ToVector4( FbxDouble4& _Value )
		{
			return SharpMath::float4( (float) _Value[0], (float) _Value[1], (float) _Value[2], (float) _Value[3] );
		}

		static SharpMath::float4	ToVector4( FbxColor& _Value )
		{
			return SharpMath::float4( (float) _Value.mRed, (float) _Value.mGreen, (float) _Value.mBlue, (float) _Value.mAlpha );
		}

		static SharpMath::float4x4	ToMatrix( FbxVector4& _P, FbxVector4& _R, FbxVector4& _S )
		{
			return	ToMatrix( FbxMatrix( _P, _R, _S ) );
		}

		static SharpMath::float4x4	ToMatrix( FbxMatrix& _Value )
		{
			SharpMath::float4x4	result;
								result.r0 = ToVector4( _Value.GetRow( 0 ) );
								result.r1 = ToVector4( _Value.GetRow( 1 ) );
								result.r2 = ToVector4( _Value.GetRow( 2 ) );
								result.r3 = ToPoint4( _Value.GetRow( 3 ) );

			return	result;
		}

		static FTimeSpan^		GetTimeSpan( FbxTimeSpan& _TimeSpan )
		{
			return gcnew FTimeSpan(	System::TimeSpan::FromMilliseconds( (double) _TimeSpan.GetStart().GetMilliSeconds() ),
									System::TimeSpan::FromMilliseconds( (double) _TimeSpan.GetStop().GetMilliSeconds() ) );
		}
	};
}
