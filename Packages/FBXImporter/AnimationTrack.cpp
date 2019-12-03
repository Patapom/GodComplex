// This is the main DLL file.

#include "stdafx.h"

#include "AnimationTrack.h"
#include "Nodes.h"
#include "ObjectProperty.h"

using namespace	FBXImporter;

AnimationTrack::AnimationTrack( ObjectProperty^ _Owner, FbxAnimCurve* _pAnimCurve )
	: m_Owner( _Owner )
	, m_pAnimCurve( _pAnimCurve )
{
	// Get the track's name
 	m_Name = Helpers::GetString( _pAnimCurve->GetName() );

	// and its time span
	FbxTimeSpan	StartStop;
	_pAnimCurve->GetTimeInterval( StartStop );
	m_TimeSpan = Helpers::GetTimeSpan( StartStop );


	//////////////////////////////////////////////////////////////////////////
	// Build the set of animation keys
	List<AnimationKey^>^	Keys = gcnew List<AnimationKey^>();

	AnimationKey^	Previous = nullptr;
	int	KeysCount = _pAnimCurve->KeyGetCount();
	for ( int KeyIndex=0; KeyIndex < KeysCount; KeyIndex++ )
	{
		FbxAnimCurveKey&	SourceKey = _pAnimCurve->KeyGet( KeyIndex );

		AnimationKey^	K = gcnew AnimationKey();
						K->Previous = Previous;
		if ( Previous != nullptr )
			Previous->Next = K;

		Previous = K;

		Keys->Add( K );

		// Build the key
		K->Time = (float) SourceKey.GetTime().GetSecondDouble();
		K->Value = float( SourceKey.GetValue() );

		// Retrieve interpolation type
		switch ( SourceKey.GetInterpolation() )
		{
		case  FbxAnimCurveDef::eInterpolationConstant:
			K->Type = AnimationKey::KEY_TYPE::CONSTANT;
			break;

		case FbxAnimCurveDef::eInterpolationLinear:
			K->Type = AnimationKey::KEY_TYPE::LINEAR;
			break;

		case FbxAnimCurveDef::eInterpolationCubic:
			K->Type = AnimationKey::KEY_TYPE::CUBIC;
			break;

		default:
			throw gcnew Exception( "Unsupported Interpolation Mode!" );
		}

		if ( K->Type != AnimationKey::KEY_TYPE::CUBIC )
			continue;	// No other data needed

			// Retrieve cubic interpolation data
		FbxAnimCurveDef::ETangentMode	TangentMode = SourceKey.GetTangentMode();
		FbxAnimCurveDef::EWeightedMode	TangentWeightMode = SourceKey.GetTangentWeightMode();
		FbxAnimCurveDef::EVelocityMode	TangentVelocityMode = SourceKey.GetTangentVelocityMode();

// 			KFCurveTangentInfo			LeftInfo = _pAnimCurve->KeyGetLeftDerivativeInfo( KeyIndex );
// 			KFCurveTangentInfo			RightInfo = _pAnimCurve->KeyGetRightDerivativeInfo( KeyIndex );
 
		switch ( TangentMode )
		{
		case FbxAnimCurveDef::eTangentAuto:	// Cardinal spline
			K->CubicType = AnimationKey::CUBIC_INTERPOLATION_TYPE::CARDINAL;
			break;

		case FbxAnimCurveDef::eTangentTCB:	// TCB spline
			K->CubicType = AnimationKey::CUBIC_INTERPOLATION_TYPE::TCB;
			K->Tension = SourceKey.GetDataFloat( FbxAnimCurveDef::eTCBTension );
			K->Continuity = SourceKey.GetDataFloat( FbxAnimCurveDef::eTCBContinuity );
			K->Bias = SourceKey.GetDataFloat( FbxAnimCurveDef::eTCBBias );
			break;

		case FbxAnimCurveDef::eTangentUser:			// Left slope = Right slope
		case FbxAnimCurveDef::eTangentGenericBreak:	// Independent left & right slopes
			K->CubicType = AnimationKey::CUBIC_INTERPOLATION_TYPE::CUSTOM;
			K->RightSlope = SourceKey.GetDataFloat( FbxAnimCurveDef::eRightSlope );
			K->NextLeftSlope = SourceKey.GetDataFloat( FbxAnimCurveDef::eNextLeftSlope );
			K->RightWeight = SourceKey.GetDataFloat( FbxAnimCurveDef::eRightWeight );
			K->NextLeftWeight = SourceKey.GetDataFloat( FbxAnimCurveDef::eNextLeftWeight );
			K->RightWeight = SourceKey.GetDataFloat( FbxAnimCurveDef::eRightVelocity );
			K->NextLeftWeight = SourceKey.GetDataFloat( FbxAnimCurveDef::eNextLeftVelocity );
			break;

		default:
			throw gcnew Exception( "Unsupported Tangent Mode!" );
		}
	}

	m_Keys = Keys->ToArray();
}

AnimationTrack::AnimationTrack( AnimationTrack^ _Source )
{
	m_Owner = _Source->m_Owner;
	m_pAnimCurve = _Source->m_pAnimCurve;

	m_Name = _Source->m_Name;
	m_TimeSpan = _Source->m_TimeSpan;
	m_pAnimCurve = _Source->m_pAnimCurve;

	m_Keys = gcnew cli::array<AnimationKey^>( _Source->m_Keys->Length );
	for ( int KeyIndex=0; KeyIndex < m_Keys->Length; KeyIndex++ )
	{
		AnimationKey^	SK = _Source->m_Keys[KeyIndex];
		AnimationKey^	K = m_Keys[KeyIndex] = gcnew AnimationKey();

		K->Previous = KeyIndex > 0 ? m_Keys[KeyIndex-1] : nullptr;
		K->Next = nullptr;
		if ( KeyIndex > 0 )
			m_Keys[KeyIndex-1]->Next = K;
		K->Type = SK->Type;
		K->Time = SK->Time;
		K->Value = SK->Value;
		K->CubicType = SK->CubicType;
		K->RightSlope = SK->RightSlope;
		K->NextLeftSlope = SK->NextLeftSlope;
		K->RightWeight = SK->RightWeight;
		K->NextLeftWeight = SK->NextLeftWeight;
		K->RightVelocity = SK->RightVelocity;
		K->NextLeftVelocity = SK->NextLeftVelocity;
		K->Tension = SK->Tension;
		K->Continuity = SK->Continuity;
		K->Bias = SK->Bias;
	}
}

float	AnimationTrack::Evaluate( float _Time )
{
	FbxTime	T;
			T.SetSecondDouble( _Time );

	float	Value = m_pAnimCurve->Evaluate( T );
	return	Value;
}
