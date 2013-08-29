// This is the main DLL file.

#include "stdafx.h"
#include <assert.h>

#include "ObjectProperty.h"
#include "BaseObject.h"
#include "Textures.h"
#include "Scene.h"

#include "Nodes.h"

using namespace	FBXImporter;

ObjectProperty::ObjectProperty( BaseObject^ _Owner, FbxProperty& _Property ) : m_Owner( _Owner )
{
	//////////////////////////////////////////////////////////////////////////
	// Store static data
	m_Name = Helpers::GetString( _Property.GetLabel() );
	m_InternalName = Helpers::GetString( _Property.GetName() );
	m_TypeName = Helpers::GetString( _Property.GetPropertyDataType().GetName() );

	//////////////////////////////////////////////////////////////////////////
	// Retrieve the property value
	bool	bAnimatable = false;
	m_Value = nullptr;
	EFbxType	PropertyType = _Property.GetPropertyDataType().GetType();
	switch ( PropertyType )
	{
	case	eFbxBool:
		m_Value = _Property.Get<bool>();
		break;

	case	eFbxDouble:
	case	eFbxFloat:
		m_Value = _Property.Get<float>();
		bAnimatable = true;
		break;

	case	eFbxInt:
		m_Value = _Property.Get<int>();
		break;

	case	eFbxDouble3:
		m_Value = Helpers::ToVector3( _Property.Get<FbxDouble3>() );
		bAnimatable = true;
		break;

	case	eFbxDouble4:
		m_Value = Helpers::ToVector4( _Property.Get<FbxDouble4>() );
		bAnimatable = true;
		break;

	case	eFbxString:
		m_Value = Helpers::GetString( _Property.Get<FbxString>() );
		break;

	case	eFbxEnum:
		m_Value = _Property.Get<int>();
		break;

	case	eFbxReference:
		{
			FbxReference*	pReference = NULL;
//			_Property.Get( &pReference, eREFERENCE );
			break;
		}

//	default:
//		throw gcnew Exception( "Property type \"" + TypeName + "\" is unsupported! Can't get a value..." );
	}


	//////////////////////////////////////////////////////////////////////////
	// Retrieve textures
	if ( _Property.GetSrcObjectCount<FbxLayeredTexture>() > 0 )
		throw gcnew Exception( "Found unsupported layer textures on property \"" + _Owner->Name + "." + Name + "\" !\r\nOnly single textures are supported in this version." );

	List<Texture^>^	Textures = gcnew List<Texture^>();
	for ( int TextureIndex=0; TextureIndex < _Property.GetSrcObjectCount<FbxTexture>(); TextureIndex++ )
	{
		FbxTexture*	pTexture = FbxCast<FbxTexture>( _Property.GetSrcObject<FbxTexture>( TextureIndex ) );
		Textures->Add( gcnew Texture( _Owner->ParentScene, pTexture ) );
	}

	m_Textures = Textures->ToArray();


	//////////////////////////////////////////////////////////////////////////
	// Retrieve animations
	m_AnimTracks = gcnew cli::array<AnimationTrack^>( 0 );

	if ( !bAnimatable )
		return;

	Node^	OwnerNode = dynamic_cast<Node^>( m_Owner );
	if ( OwnerNode == nullptr || OwnerNode->GetCurrentTake() == nullptr )
		return;	// Anim tracks are only supported on node objects

	FbxAnimLayer*	pAnimLayer = OwnerNode->GetCurrentTake()->AnimLayer;
	if ( pAnimLayer == NULL )
		return;

	FbxAnimCurveNode*	pCurveNode = _Property.GetCurveNode( pAnimLayer );
	if ( pCurveNode == NULL )
		return;

	int		AnimTracksCount = pCurveNode->GetChannelsCount();
	m_AnimTracks = gcnew cli::array<AnimationTrack^>( AnimTracksCount );

	for ( int AnimTrackIndex=0; AnimTrackIndex < AnimTracksCount; AnimTrackIndex++ )
	{
		assert( pCurveNode->GetCurveCount( AnimTrackIndex ) == 1 );	// Fucking composite nodes! Don't give a damn about that!
		FbxAnimCurve*	pAnimCurve = pCurveNode->GetCurve( AnimTrackIndex );
		m_AnimTracks[AnimTrackIndex] = gcnew AnimationTrack( this, pAnimCurve );
	}

	// TODO !
// 	const char*		pCurrentTakeName = Helpers::FromString( OwnerNode->GetCurrentTake()->Name );
// 	KFCurveNode*	pCurveNode = _Property.GetKFCurveNode( false, pCurrentTakeName );
// 	if ( pCurveNode != NULL )
// 		m_AnimTrack = gcnew AnimationTrack( nullptr, this, OwnerNode, pCurveNode, _Property.GetPropertyDataType().GetType() );	// Create the single anim track
}
