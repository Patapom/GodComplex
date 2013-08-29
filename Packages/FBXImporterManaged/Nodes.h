// Contains base Node class as well as Light & Camera nodes
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "BaseObject.h"
#include "Materials.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	ref class	Take;
	ref class	AnimationTrack;

	//////////////////////////////////////////////////////////////////////////
	// Represents a node in the hierarchy of scene nodes
	// A node can be a light, a camera, a mesh or virtually anything
	//
	public ref class		Node abstract : public BaseObject
	{

	protected:	// FIELDS

//		FbxNode*			m_pNode;			// The FBX node we're wrapping

		bool				m_bVisible;

		Node^				m_Parent;			// Parent
		List<Node^>^		m_Children;			// The list of child nodes

		WMath::Matrix4x4^	m_PreRotation;
		WMath::Matrix4x4^	m_PostRotation;

		WMath::Matrix4x4^	m_LocalTransform;

		WMath::Matrix4x4^				m_AnimationSourceMatrix;	// The matrix to use as source for the animation
		cli::array<AnimationTrack^>^	m_AnimP;					// Position animation track
		cli::array<AnimationTrack^>^	m_AnimR;					// Rotation animation track
		cli::array<AnimationTrack^>^	m_AnimS;					// Scale animation track


	public:		// PROPERTIES

		[DescriptionAttribute( "Gets the node's visible state" )]
		//
		property bool		Visible
		{
			bool				get()	{ return m_bVisible; }
		}

		[DescriptionAttribute( "Gets the node's parent" )]
		//
		property Node^		Parent
		{
			Node^				get()	{ return m_Parent; }
		}

		[DescriptionAttribute( "Gets the node's children" )]
		//
		property cli::array<Node^>^	Children
		{
			cli::array<Node^>^	get()	{ return m_Children->ToArray(); }
		}

		[DescriptionAttribute( "Gets the pre-rotation matrix to apply to transforms (static or animated)" )]
		//
		property WMath::Matrix4x4^	PreRotation
		{
			WMath::Matrix4x4^	get()	{ return m_PreRotation; }
		}

		[DescriptionAttribute( "Gets the post-rotation matrix to apply to transforms (static or animated)" )]
		//
		property WMath::Matrix4x4^	PostRotation
		{
			WMath::Matrix4x4^	get()	{ return m_PostRotation; }
		}

		[DescriptionAttribute( "Gets the local transform matrix that transforms the node from LOCAL to WORLD space" )]
		//
		property WMath::Matrix4x4^	LocalTransform
		{
			WMath::Matrix4x4^	get()	{ return m_LocalTransform; }
		}

		[DescriptionAttribute( "Tells if the node has PRS animation" )]
		//
		property bool				IsPRSAnimated
		{
			bool				get()	{ return m_AnimP != nullptr || m_AnimR != nullptr || m_AnimS != nullptr; }
		}

		[DescriptionAttribute( "Gives the duration of the animation (in seconds)" )]
		//
		property float				AnimationDuration
		{
			float				get();
		}

		[DescriptionAttribute( "Gets the matrix to use as source for animation" )]
		//
		property WMath::Matrix4x4^	AnimationSourceMatrix
		{
			WMath::Matrix4x4^	get()	{ return m_AnimationSourceMatrix; }
		}

		[DescriptionAttribute( "Gets the 3 animation tracks for position" )]
		//
		property cli::array<AnimationTrack^>^	AnimationTracksPosition
		{
			cli::array<AnimationTrack^>^	get()	{ return m_AnimP; }
		}

		[DescriptionAttribute( "Gets the 3 animation tracks for rotation" )]
		//
		property cli::array<AnimationTrack^>^	AnimationTracksRotation
		{
			cli::array<AnimationTrack^>^	get()	{ return m_AnimR; }
		}

		[DescriptionAttribute( "Gets the 3 animation tracks for scale" )]
		//
		property cli::array<AnimationTrack^>^	AnimationTracksScale
		{
			cli::array<AnimationTrack^>^	get()	{ return m_AnimS; }
		}


	public:		// METHODS

		Node( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode );

		[BrowsableAttribute( false )]
		void	AddChild( Node^ _Child )
		{
			m_Children->Add( _Child );
		}

		// Resolves the FBX material reference into one of our materials
		[BrowsableAttribute( false )]
		Material^	ResolveMaterial( FbxSurfaceMaterial* _pMaterial );

		// Gets the default scene's take name
		[BrowsableAttribute( false )]
		Take^		GetCurrentTake();
	};

	//////////////////////////////////////////////////////////////////////////
	// The root node class, with a reference to the scene
	//
	public ref class		NodeRoot : public Node
	{
	public:		// METHODS

		NodeRoot( Scene^ _ParentScene, FbxNode* _pNode ) : Node( _ParentScene, nullptr, _pNode )
		{
		}
	};


	//////////////////////////////////////////////////////////////////////////
	// The generic node class
	//
	public ref class		NodeGeneric : public Node
	{
	public:		// METHODS

		NodeGeneric( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : Node( _ParentScene, _Parent, _pNode )
		{
		}
	};


	//////////////////////////////////////////////////////////////////////////
	// A node with a valid content attribute
	//
	public ref class		NodeWithAttribute : public Node
	{
	protected:	// FIELDS

		FbxNodeAttribute*	m_pAttribute;

		List<Material^>^	m_Materials;

	public:		// PROPERTIES

		[DescriptionAttribute( "Gets the color of the node (issued from the modelling package, as seen in the viewport)" )]
		//
		property WMath::Vector^	Color
		{
			WMath::Vector^			get()	{ return Helpers::ToVector3( m_pAttribute->Color.Get() ); }
		}

		[DescriptionAttribute( "Gets the list of materials associated to that node" )]
		//
		property cli::array<Material^>^	Materials
		{
			cli::array<Material^>^	get()	{ return m_Materials->ToArray(); }
		}


	public:		// METHODS

		NodeWithAttribute( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : Node( _ParentScene, _Parent, _pNode )
		{
			m_pAttribute = _pNode->GetNodeAttribute();

			//////////////////////////////////////////////////////////////////////////
			// Resolve materials
			m_Materials = gcnew List<Material^>();
			for ( int MaterialIndex=0; MaterialIndex < _pNode->GetMaterialCount(); MaterialIndex++ )
				m_Materials->Add( ResolveMaterial( _pNode->GetMaterial( MaterialIndex ) ) );
		}

		// Resolves a material by index
		[BrowsableAttribute( false )]
		Material^	ResolveMaterial( int _MaterialIndex )
		{
			return	m_Materials[_MaterialIndex];
		}
	};


	//////////////////////////////////////////////////////////////////////////
	// A Light node
	//
	public ref class		NodeLight : public NodeWithAttribute
	{
	public:		// NESTED TYPES

		enum class	LIGHT_TYPE
		{
			POINT,
			DIRECTIONAL,
			SPOT,
			AREA_RECTANGLE,
			AREA_SPHERE,
		};

		enum class	DECAY_TYPE
		{
			LINEAR,
			QUADRATIC,
			CUBIC,
		};

	protected:	// FIELDS

		FbxLight*	m_pLight;	// The node attribute cast to a light

	public:		// PROPERTIES

		property LIGHT_TYPE	LightType
		{
			LIGHT_TYPE	get()
			{
				switch ( m_pLight->LightType.Get() )
				{
				case FbxLight::ePoint:
					return	LIGHT_TYPE::POINT;

				case FbxLight::eDirectional:
					return	LIGHT_TYPE::DIRECTIONAL;

				case FbxLight::eSpot:
					return	LIGHT_TYPE::SPOT;

				case FbxLight::eArea:
					return	m_pLight->AreaLightShape.Get() == FbxLight::eRectangle ? LIGHT_TYPE::AREA_RECTANGLE : LIGHT_TYPE::AREA_SPHERE;
				}

				return	LIGHT_TYPE::POINT;
			}
		}

		[DescriptionAttribute( "Gets the light color" )]
		//
		property WMath::Vector^	Color
		{
			WMath::Vector^	get()	{ return Helpers::ToVector3( m_pLight->Color.Get() ); }
		}

		[DescriptionAttribute( "Gets the light intensity" )]
		//
		property float			Intensity
		{
			float			get()	{ return (float) m_pLight->Intensity.Get() * 0.01f; }
		}

		[DescriptionAttribute( "Gets the Hotspot angle in radians" )]
		// 
		property float			HotSpot
		{
			float			get()	{ return (float) (Math::PI * m_pLight->InnerAngle.Get() / 180.0f); }
		}

		[DescriptionAttribute( "Gets the Cone angle in radians" )]
		// 
		property float			ConeAngle
		{
			float			get()	{ return (float) (Math::PI * m_pLight->OuterAngle.Get() / 180.0f); }
		}

		[DescriptionAttribute( "Gets the decay type (e.g. linear, quadratic, cubic)" )]
		// 
		property DECAY_TYPE		DecayType
		{
			DECAY_TYPE	get()
			{
				switch ( m_pLight->DecayType.Get() )
				{
				case	FbxLight::eLinear:
					return	DECAY_TYPE::LINEAR;

				case	FbxLight::eQuadratic:
					return	DECAY_TYPE::QUADRATIC;

				case	FbxLight::eCubic:
					return	DECAY_TYPE::CUBIC;
				}

				return	DECAY_TYPE::LINEAR;
			}
		}

		[DescriptionAttribute( "Gets the start distance for the decay" )]
		// 
		property float			DecayStart
		{
			float	get()	{ return (float) m_pLight->DecayStart.Get(); }
		}

		[DescriptionAttribute( "Tells if the light casts shadows" )]
		// 
		property bool			CastShadows
		{
			bool	get()	{ return m_pLight->CastShadows.Get(); }
		}

		[DescriptionAttribute( "Gets the fog value" )]
		// 
		property float			Fog
		{
			float			get()	{ return (float) m_pLight->Fog.Get(); }
		}

		[DescriptionAttribute( "Tells if the light has near attenuation" )]
		// 
		property bool			EnableNearAttenuation
		{
			bool	get()	{ return m_pLight->EnableNearAttenuation.Get(); }
		}

		[DescriptionAttribute( "Gets the near attenuation start" )]
		// 
		property float			NearAttenuationStart
		{
			float			get()	{ return (float) m_pLight->NearAttenuationStart.Get(); }
		}

		[DescriptionAttribute( "Gets the near attenuation end" )]
		// 
		property float			NearAttenuationEnd
		{
			float			get()	{ return (float) m_pLight->NearAttenuationEnd.Get(); }
		}

		[DescriptionAttribute( "Tells if the light has far attenuation" )]
		// 
		property bool			EnableFarAttenuation
		{
			bool	get()	{ return m_pLight->EnableFarAttenuation.Get(); }
		}

		[DescriptionAttribute( "Gets the far attenuation start" )]
		// 
		property float			FarAttenuationStart
		{
			float			get()	{ return (float) m_pLight->FarAttenuationStart.Get(); }
		}

		[DescriptionAttribute( "Gets the far attenuation end" )]
		// 
		property float			FarAttenuationEnd
		{
			float			get()	{ return (float) m_pLight->FarAttenuationEnd.Get(); }
		}


	public:		// METHODS

		NodeLight( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : NodeWithAttribute( _ParentScene, _Parent, _pNode )
		{
			m_pLight = _pNode->GetLight();
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// A Camera node
	//
	public ref class		NodeCamera : public NodeWithAttribute
	{
	public:		// NESTED TYPES

		enum class	PROJECTION_TYPE
		{
			PERSPECTIVE,
			ORTHOGRAPHIC,
		};

	protected:	// FIELDS

		FbxCamera*	m_pCamera;	// The node attribute cast to a camera

	public:		// PROPERTIES

		[DescriptionAttribute( "Gets the camera projection type (i.e. perspective or orthographic)" )]
		// 
		property PROJECTION_TYPE	ProjectionType
		{
			PROJECTION_TYPE	get()
			{
				FbxCamera::EProjectionType	ProjType = m_pCamera->ProjectionType.Get();
				return	ProjType == FbxCamera::ePerspective ? PROJECTION_TYPE::PERSPECTIVE : PROJECTION_TYPE::ORTHOGRAPHIC;
			}
		}

		[DescriptionAttribute( "Gets the camera up vector" )]
		// 
		property WMath::Vector^		UpVector
		{
			WMath::Vector^	get()	{ return Helpers::ToVector3( m_pCamera->UpVector.Get() ); }
		}

		[DescriptionAttribute( "Gets the target position" )]
		// 
		property WMath::Point^		Target
		{
			WMath::Point^	get()	{ return Helpers::ToPoint3( m_pCamera->InterestPosition.Get() ); }
		}

		[DescriptionAttribute( "Gets the horizontal field of view in radians" )]
		// 
		property float				FOVX
		{
			float			get()	{ return (float) (Math::PI * m_pCamera->FieldOfViewX.Get() / 180.0f); }
		}

		[DescriptionAttribute( "Gets the vertical field of view in radians" )]
		// 
		property float				FOVY
		{
			float			get()	{ return (float) (Math::PI * m_pCamera->FieldOfViewX.Get() / 180.0f); }
		}

		[DescriptionAttribute( "Gets the focal length" )]
		// 
		property float				FocalLength
		{
			float			get()	{ return (float) m_pCamera->FocalLength.Get(); }
		}

		[DescriptionAttribute( "Gets the camera roll in radians" )]
		// 
		property float				Roll
		{
			float			get()	{ return (float) (Math::PI * m_pCamera->Roll.Get() / 180.0f); }
		}

		[DescriptionAttribute( "Gets the near clip distance" )]
		// 
		property float				NearClipPlane
		{
			float			get()	{ return (float) m_pCamera->NearPlane.Get(); }
		}

		[DescriptionAttribute( "Gets the far clip distance" )]
		// 
		property float				FarClipPlane
		{
			float			get()	{ return (float) m_pCamera->FarPlane.Get(); }
		}


	public:		// METHODS

		NodeCamera( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : NodeWithAttribute( _ParentScene, _Parent, _pNode )
		{
			m_pCamera = _pNode->GetCamera();
		}
	};
}
