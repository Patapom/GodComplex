// This is the main DLL file.

#include "stdafx.h"

#include "Nodes.h"
#include "Scene.h"
#include "AnimationTrack.h"

using namespace	FBXImporter;


Node::Node( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : BaseObject( _ParentScene, _pNode ), m_Parent( _Parent )
{
//	m_bVisible = _pNode->GetVisibility();
	m_bVisible = _pNode->Show.Get();

	m_Children = gcnew List<Node^>();

	//////////////////////////////////////////////////////////////////////////
	// Retrieve the pre & post rotations
 	ObjectProperty^	PreRotProp = FindProperty( "PreRotation" );
 	WMath::Vector^	PreRotEuler = (float) Math::PI / 180.0f * (PreRotProp != nullptr ? PreRotProp->AsVector3 : gcnew WMath::Vector( 0, 0, 0 ));
//	WMath::Vector^	PreRotEuler = Helpers::ToVector3( _pNode->PreRotation.Get() );
//	WMath::Vector^	PreRotEuler = Helpers::ToVector3( _pNode->PreRotation.Get() );

	m_PreRotation = gcnew WMath::Matrix4x4();
	m_PreRotation->MakePYR( PreRotEuler->x, PreRotEuler->y, PreRotEuler->z );

 	ObjectProperty^	PostRotProp = FindProperty( "PreRotation" );
 	WMath::Vector^	PostRotEuler = (float) Math::PI / 180.0f * (PostRotProp != nullptr ? PostRotProp->AsVector3 : gcnew WMath::Vector( 0, 0, 0 ));
//	WMath::Vector^	PostRotEuler = Helpers::ToVector3( _pNode->PostRotation.Get() );

	m_PostRotation = gcnew WMath::Matrix4x4();
	m_PostRotation->MakePYR( PostRotEuler->x, PostRotEuler->y, PostRotEuler->z );


	//////////////////////////////////////////////////////////////////////////
	// Retrieve the local transform
	m_LocalTransform = gcnew WMath::Matrix4x4();
	m_LocalTransform->MakeIdentity();

	WMath::Point^		Position = FindProperty( "Lcl Translation" )->AsPoint;
	WMath::Vector^		Rotation = (float) Math::PI / 180.0f * FindProperty( "Lcl Rotation" )->AsVector3;
	WMath::Vector^		Scale    = FindProperty( "Lcl Scaling" )->AsVector3;

	WMath::Matrix4x4^	Pitch = WMath::Matrix4x4::ROT_X( Rotation->x );
	WMath::Matrix4x4^	Yaw = WMath::Matrix4x4::ROT_Y( Rotation->y );
	WMath::Matrix4x4^	Roll = WMath::Matrix4x4::ROT_Z( Rotation->z );

	switch ( FindProperty( "RotationOrder" )->AsInt )
	{
	case 0:	// eEulerXYZ
		m_LocalTransform = Pitch * Yaw * Roll;
		break;

	case 1:	// eEulerXZY
		m_LocalTransform = Pitch * Roll * Yaw;
		break;

	case 2:	// eEulerYZX
		m_LocalTransform = Yaw * Roll * Pitch;
		break;

	case 3:	// eEulerYXZ
		m_LocalTransform = Yaw * Pitch * Roll;
		break;

	case 4:	// eEulerZXY
		m_LocalTransform = Roll * Pitch * Yaw;
		break;

	case 5:	// eEulerZYX
		m_LocalTransform = Roll * Yaw * Pitch;
		break;

	default:
		throw gcnew Exception( "Unsupported rotation order!" );
	}

	// WORKING CODE
//	m_LocalTransform->MakePYR( Rotation->x, Rotation->y, Rotation->z );
	m_LocalTransform->Scale( Scale );
	m_LocalTransform->SetTrans( Position );
	// WORKING CODE

	// ADDITIONAL CODE
	// Apply pre & post rotations
	m_LocalTransform = m_PreRotation * m_LocalTransform * m_PostRotation;
	// ADDITIONAL CODE


	// ADDITIONAL CODE
	Scene::UP_AXIS	UpAxis = Scene::UP_AXIS::Y;	// Default is Y-Up (no transform)
	if ( m_Parent == nullptr || dynamic_cast<NodeRoot^>( m_Parent ) != nullptr )
		UpAxis = m_ParentScene->UpAxis;			// If root, use scene's Up axis to transform into Y-up

	switch ( UpAxis )
	{
	case Scene::UP_AXIS::X:
		throw gcnew Exception( "X as Up Axis is not supported!" );
		break;

	case Scene::UP_AXIS::Y:
//		throw gcnew Exception( "TODO!" );
		break;

	case Scene::UP_AXIS::Z:
		{
// 			WMath::Matrix4x4^	RotX = gcnew WMath::Matrix4x4();
// 								RotX->MakeRotX( -0.5f * (float) Math::PI );
// 
// 			m_LocalTransform *= RotX;
			break;
		}
	}
	// ADDITIONAL CODE


// 	m_AnimP = m_AnimR = m_AnimS = nullptr;
// 	return;

/*	//////////////////////////////////////////////////////////////////////////
	// Attempt to retrieve the PRS animation tracks
	ObjectProperty^	Prop = FindProperty( "Lcl Translation" );
	m_AnimP = nullptr;
	if ( Prop != nullptr && Prop->AnimTrack != nullptr )
	{
		cli::array<AnimationTrack^>^	ChildTracks = Prop->AnimTrack->ChildTracks;
		if ( ChildTracks->Length < 3 )
			throw gcnew Exception( "Object \"" + Name + "\" has a POSITION animation track that contains only " + ChildTracks->Length + " child tracks (expected 3)!" );

		m_AnimP = gcnew cli::array<AnimationTrack^>( 3 );
		m_AnimP[0] = ChildTracks[0];
		m_AnimP[1] = ChildTracks[1];
		m_AnimP[2] = ChildTracks[2];
	}

	Prop = FindProperty( "Lcl Rotation" );
	m_AnimR = nullptr;
	if ( Prop != nullptr && Prop->AnimTrack != nullptr )
	{
		cli::array<AnimationTrack^>^	ChildTracks = Prop->AnimTrack->ChildTracks;
		if ( ChildTracks->Length != 3 )
			throw gcnew Exception( "Object \"" + Name + "\" has a ROTATION animation track that contains " + ChildTracks->Length + " child tracks (expected 3)!" );

		m_AnimR = gcnew cli::array<AnimationTrack^>( 3 );
		m_AnimR[0] = ChildTracks[0];
		m_AnimR[1] = ChildTracks[1];
		m_AnimR[2] = ChildTracks[2];

		m_AnimR[0]->ApplyFactor( (float) Math::PI / 180.0f );
		m_AnimR[1]->ApplyFactor( (float) Math::PI / 180.0f );
		m_AnimR[2]->ApplyFactor( (float) Math::PI / 180.0f );
	}

	Prop = FindProperty( "Lcl Scaling" );
	m_AnimS = nullptr;
	if ( Prop != nullptr && Prop->AnimTrack != nullptr )
	{
		cli::array<AnimationTrack^>^	ChildTracks = Prop->AnimTrack->ChildTracks;
		if ( ChildTracks->Length < 3 )
			throw gcnew Exception( "Object \"" + Name + "\" has a SCALE animation track that contains only " + ChildTracks->Length + " child tracks (expected 3)!" );

		m_AnimS = gcnew cli::array<AnimationTrack^>( 3 );
		m_AnimS[0] = ChildTracks[0];
		m_AnimS[1] = ChildTracks[1];
		m_AnimS[2] = ChildTracks[2];
	}
*/

 	m_AnimP = m_AnimR = m_AnimS = nullptr;
	FbxAnimLayer*	pAnimLayer = _ParentScene->CurrentTake != nullptr ? _ParentScene->CurrentTake->AnimLayer : NULL;
	if ( pAnimLayer != NULL )
	{
		ObjectProperty^	Prop = FindProperty( "Lcl Translation" );
		cli::array<AnimationTrack^>^	AnimTracks = Prop != nullptr ? Prop->AnimTracks : nullptr;
		if ( Prop != nullptr && AnimTracks->Length > 0 )
		{
			if ( AnimTracks->Length < 3 )
				throw gcnew Exception( "Object \"" + Name + "\" has a POSITION animation track that contains only " + AnimTracks->Length + " child tracks (expected 3)!" );

			m_AnimP = gcnew cli::array<AnimationTrack^>( 3 );
			m_AnimP[0] = AnimTracks[0];
			m_AnimP[1] = AnimTracks[1];
			m_AnimP[2] = AnimTracks[2];
		}

		Prop = FindProperty( "Lcl Rotation" );
		AnimTracks = Prop != nullptr ? Prop->AnimTracks : nullptr;
		if ( Prop != nullptr && AnimTracks->Length > 0 )
		{
			if ( AnimTracks->Length != 3 )
				throw gcnew Exception( "Object \"" + Name + "\" has a ROTATION animation track that contains " + AnimTracks->Length + " child tracks (expected 3)!" );

			m_AnimR = gcnew cli::array<AnimationTrack^>( 3 );
			m_AnimR[0] = AnimTracks[0];
			m_AnimR[1] = AnimTracks[1];
			m_AnimR[2] = AnimTracks[2];

			m_AnimR[0]->ApplyFactor( (float) Math::PI / 180.0f );
			m_AnimR[1]->ApplyFactor( (float) Math::PI / 180.0f );
			m_AnimR[2]->ApplyFactor( (float) Math::PI / 180.0f );
		}

		Prop = FindProperty( "Lcl Scaling" );
		AnimTracks = Prop != nullptr ? Prop->AnimTracks : nullptr;
		if ( Prop != nullptr && AnimTracks->Length > 0 )
		{
			if ( AnimTracks->Length < 3 )
				throw gcnew Exception( "Object \"" + Name + "\" has a SCALE animation track that contains only " + AnimTracks->Length + " child tracks (expected 3)!" );

			m_AnimS = gcnew cli::array<AnimationTrack^>( 3 );
			m_AnimS[0] = AnimTracks[0];
			m_AnimS[1] = AnimTracks[1];
			m_AnimS[2] = AnimTracks[2];
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Build the source animation matrix
	m_AnimationSourceMatrix = gcnew WMath::Matrix4x4();
	m_AnimationSourceMatrix->MakeIdentity();

	switch ( UpAxis )
	{
	case Scene::UP_AXIS::X:
		throw gcnew Exception( "X as Up Axis is not supported!" );
		break;

	case Scene::UP_AXIS::Y:
		// Don't need to do anything !
		break;

	case Scene::UP_AXIS::Z:
		m_AnimationSourceMatrix->MakeRotX( -0.5f * (float) Math::PI );
		break;
	}

	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	// OLD CODE where I vainly attempted to find the correct orientation

// 	//////////////////////////////////////////////////////////////////////////
// 	// Attempt to retrieve the PRS animation tracks
// 	ObjectProperty^	Prop = FindProperty( "Lcl Translation" );
// 	m_AnimP = nullptr;
// 	if ( Prop != nullptr && Prop->AnimTrack != nullptr )
// 	{
// 		cli::array<AnimationTrack^>^	ChildTracks = Prop->AnimTrack->ChildTracks;
// 		if ( ChildTracks->Length < 3 )
// 			throw gcnew Exception( "Object \"" + Name + "\" has a POSITION animation track that contains only " + ChildTracks->Length + " child tracks (expected 3)!" );
// 
// 		m_AnimP = gcnew cli::array<AnimationTrack^>( 3 );
// //		switch ( UpAxis )
// 		switch ( Scene::UP_AXIS::Y )
// 		{
// 		case Scene::UP_AXIS::X:
// 			throw gcnew Exception( "X as Up Axis is not supported!" );
// 			break;
// 
// 		case Scene::UP_AXIS::Y:
// 			m_AnimP[0] = ChildTracks[0];
// 			m_AnimP[1] = ChildTracks[1];
// 			m_AnimP[2] = ChildTracks[2];
// 			break;
// 
// 		case Scene::UP_AXIS::Z:
// 			m_AnimP[0] = ChildTracks[0];
// 			m_AnimP[1] = ChildTracks[2];	// Moving forward on Z means moving forward on Y
// 			m_AnimP[2] = ChildTracks[1];
// 			m_AnimP[2]->ApplyFactor( -1 );	// Moving forward on Y means moving backward on Z
// 			break;
// 		}
// 	}
// 
// 	Prop = FindProperty( "Lcl Rotation" );
// 	m_AnimR = nullptr;
// 	if ( Prop != nullptr && Prop->AnimTrack != nullptr )
// 	{
// 		cli::array<AnimationTrack^>^	ChildTracks = Prop->AnimTrack->ChildTracks;
// 		if ( ChildTracks->Length != 3 )
// 			throw gcnew Exception( "Object \"" + Name + "\" has a ROTATION animation track that contains " + ChildTracks->Length + " child tracks (expected 3)!" );
// 
// 		m_AnimR = gcnew cli::array<AnimationTrack^>( 3 );
// //		switch ( UpAxis )
// 		switch ( Scene::UP_AXIS::Y )
// 		{
// 		case Scene::UP_AXIS::X:
// 			throw gcnew Exception( "X as Up Axis is not supported!" );
// 			break;
// 
// 		case Scene::UP_AXIS::Y:
// 			m_AnimR[0] = ChildTracks[0];
// 			m_AnimR[1] = ChildTracks[1];
// 			m_AnimR[2] = ChildTracks[2];
// 			break;
// 
// 		case Scene::UP_AXIS::Z:
// // 			m_AnimR[0] = ChildTracks[0];
// // 			m_AnimR[1] = ChildTracks[1];
// // 			m_AnimR[2] = ChildTracks[2];
// //			m_AnimR[0]->ConvertRotationToYUp( m_AnimR[0], m_AnimR[1], m_AnimR[2] );
// 
// 
// 			m_AnimR[0] = ChildTracks[0];
// // 			m_AnimR[0]->AddValue( -90 );	// Same as rotating on X by -90 for the local transform...
// 			m_AnimR[1] = ChildTracks[2];	// Rotating CCW on Z means rotating CCW on Y
// 			m_AnimR[2] = ChildTracks[1];	// Rotating CCW on Y means rotating CW on Z (the negative sign is passed through the negative axis below)
// 			break;
// 		}
// 
// 		m_AnimR[0]->ApplyFactor( (float) Math::PI / 180.0f );
// 		m_AnimR[1]->ApplyFactor( (float) Math::PI / 180.0f );
// 		m_AnimR[2]->ApplyFactor( (float) Math::PI / 180.0f );
// 	}
// 
// 	Prop = FindProperty( "Lcl Scaling" );
// 	m_AnimS = nullptr;
// 	if ( Prop != nullptr && Prop->AnimTrack != nullptr )
// 	{
// 		cli::array<AnimationTrack^>^	ChildTracks = Prop->AnimTrack->ChildTracks;
// 		if ( ChildTracks->Length < 3 )
// 			throw gcnew Exception( "Object \"" + Name + "\" has a SCALE animation track that contains only " + ChildTracks->Length + " child tracks (expected 3)!" );
// 
// 		m_AnimS = gcnew cli::array<AnimationTrack^>( 3 );
// //		switch ( UpAxis )
// 		switch ( Scene::UP_AXIS::Y )
// 		{
// 		case Scene::UP_AXIS::X:
// 			throw gcnew Exception( "X as Up Axis is not supported!" );
// 			break;
// 
// 		case Scene::UP_AXIS::Y:
// 			m_AnimS[0] = ChildTracks[0];
// 			m_AnimS[1] = ChildTracks[1];
// 			m_AnimS[2] = ChildTracks[2];
// 			break;
// 
// 		case Scene::UP_AXIS::Z:
// 			m_AnimS[0] = ChildTracks[0];
// 			m_AnimS[1] = ChildTracks[2];	// Scaling on Z means scaling on Y
// 			m_AnimS[2] = ChildTracks[1];	// Scaling on Y means scaling on Z
// 			break;
// 		}
// 	}
// 
// 	// Build rotation axes based on scene's Up axis
// 	m_RotationAxes = gcnew cli::array<WMath::Vector^>( 3 );
// //	switch ( UpAxis )
// 	switch ( Scene::UP_AXIS::Y )
// 	{
// 	case Scene::UP_AXIS::X:
// 		throw gcnew Exception( "X as Up Axis is not supported!" );
// 		break;
// 
// 	case Scene::UP_AXIS::Y:
// 		m_RotationAxes[0] = gcnew WMath::Vector( 1, 0, 0 );
// 		m_RotationAxes[1] = gcnew WMath::Vector( 0, 1, 0 );
// 		m_RotationAxes[2] = gcnew WMath::Vector( 0, 0, 1 );
// 		break;
// 
// 	case Scene::UP_AXIS::Z:
// 		m_RotationAxes[0] = gcnew WMath::Vector( 1, 0, 0 );
// 		m_RotationAxes[1] = gcnew WMath::Vector( 0, 0, -1 );	// Rotating CCW on Y means rotating CW on Z (the negative sign is passed through the negative axis below)
// 		m_RotationAxes[2] = gcnew WMath::Vector( 0, 1, 0 );		// Rotating CCW on Z means rotating CCW on Y
// 		break;
// 	}
}

float		Node::AnimationDuration::get()
{
	return m_ParentScene->CurrentTake != nullptr ? m_ParentScene->CurrentTake->Duration : 0.0f;
}

Material^	Node::ResolveMaterial( FbxSurfaceMaterial* _pMaterial )
{
	return	m_ParentScene->ResolveMaterial( _pMaterial );
}

Take^		Node::GetCurrentTake()
{
	return m_ParentScene->CurrentTake;
}