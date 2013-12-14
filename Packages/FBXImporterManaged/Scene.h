// FBXImporter.h
#pragma managed
#pragma once

using namespace System;
using namespace System::Collections::Generic;

#include "Helpers.h"
#include "Nodes.h"
#include "NodeMesh.h"
#include "NodeSkeleton.h"
#include "Layers.h"
#include "Materials.h"
#include "HardwareMaterials.h"

namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// Contains informations about the available FBX "takes"
	// In FBX, a scene can have one or more "takes". A take is a container for animation data.
	// You can access a file's take information without the overhead of loading the entire file into the scene.
	//
	public ref class		Take
	{
	protected:	// FIELDS

		int					m_Index;
		String^				m_Name;
		String^				m_Description;
		String^				m_ImportName;
		FTimeSpan^			m_LocalTimeSpan;
		FTimeSpan^			m_ReferenceTimeSpan;

		FbxAnimStack*		m_pAnimStack;


	public:		// PROPERTIES

		property String^			Name
		{
			String^		get()		{ return m_Name; }
		}

		// Gets the take's duration in seconds
		property float				Duration
		{
			float		get()
			{
				float	D0 = (float) (m_ReferenceTimeSpan->Stop.TotalSeconds - m_ReferenceTimeSpan->Start.TotalSeconds);
				float	D1 = (float) (m_LocalTimeSpan->Stop.TotalSeconds - m_LocalTimeSpan->Start.TotalSeconds);
				return	D0;
			}
		}

		property FbxAnimLayer*		AnimLayer
		{
			FbxAnimLayer*	get()
			{
				int	AnimLayersCount = m_pAnimStack != NULL ? m_pAnimStack->GetMemberCount<FbxAnimLayer>() : 0;
				return AnimLayersCount > 0 ? m_pAnimStack->GetMember<FbxAnimLayer>( 0 ) : NULL;
			}
		}

	public:		// METHODS

		Take( int _TakeIndex, FbxTakeInfo* _pTakeInfo )
		{
			m_Index = _TakeIndex;
			m_Name = Helpers::GetString( _pTakeInfo->mName );
			m_Description = Helpers::GetString( _pTakeInfo->mDescription );
			m_ImportName = Helpers::GetString( _pTakeInfo->mImportName );
			m_LocalTimeSpan = Helpers::GetTimeSpan( _pTakeInfo->mLocalTimeSpan );
			m_ReferenceTimeSpan = Helpers::GetTimeSpan( _pTakeInfo->mReferenceTimeSpan );
		}

		void	BuildAnimStack( FbxScene* _pScene )
		{
			m_pAnimStack = _pScene->GetSrcObject<FbxAnimStack>( m_Index );
		}
	};

	public ref class	Scene
	{
	public:		// NESTED TYPES

		enum class	UP_AXIS
		{
			X,
			Y,
			Z
		};

	protected:	// FIELDS

		// Scene infos
		Take^				m_CurrentTake;
		List<Take^>^		m_Takes;

		UP_AXIS				m_UpAxis;

		// Materials list
		List<Material^>^	m_Materials;
		Dictionary<String^,Material^>^	m_Name2Material;

		// Nodes hierarchy
		List<Node^>^		m_Nodes;
		Node^				m_RootNode;


	internal:
		// Temporary, valid during "Load()"
		FbxManager*			m_pTempSDKManager;


	public:		// PROPERTIES

		property cli::array<Take^>^			Takes
		{
			cli::array<Take^>^		get()	{ return m_Takes->ToArray(); }
		}

		property Take^						CurrentTake
		{
			Take^					get()	{ return m_CurrentTake; }
		}

		property cli::array<Material^>^		Materials
		{
			cli::array<Material^>^	get()	{ return m_Materials->ToArray(); }
		}

		property cli::array<Node^>^			Nodes
		{
			cli::array<Node^>^		get()	{ return m_Nodes->ToArray(); }
		}

		property Node^						RootNode
		{
			Node^					get()	{ return m_RootNode; }
		}

		property UP_AXIS					UpAxis
		{
			UP_AXIS					get()	{ return m_UpAxis; }
		}


	public:		// METHODS

		Scene()
			: m_pTempSDKManager( NULL )
		{
			// Initialize lists
			m_Takes = gcnew List<Take^>();
			m_Materials = gcnew List<Material^>();
			m_Nodes = gcnew List<Node^>();
			m_Name2Material = gcnew Dictionary<String^,Material^>();
		}

		~Scene()
		{
		}

		//////////////////////////////////////////////////////////////////////////
		// Loads a scene from disk
		//
		void		Load( System::String^ _FileName )
		{
			// The first thing to do is to create the FBX SDK manager which is the object allocator for almost all the classes in the SDK.
			m_pTempSDKManager = FbxManager::Create();
			if ( !m_pTempSDKManager )
				throw gcnew Exception( "Unable to create the FBX SDK manager!" );

			// Create an IOSettings object
			FbxIOSettings*	pIOSettings = FbxIOSettings::Create( m_pTempSDKManager, IOSROOT );
			m_pTempSDKManager->SetIOSettings( pIOSettings );

			// Load plugins from the executable directory
			FbxString lPath = FbxGetApplicationDirectory();
			FbxString lExtension = "dll";
			m_pTempSDKManager->LoadPluginsDirectory( lPath.Buffer(), lExtension.Buffer() );


			// Clear lists & pointers
			m_CurrentTake = nullptr;
			m_Takes->Clear();

			m_RootNode = nullptr;
			m_Nodes->Clear();

			m_Materials->Clear();
			m_Name2Material->Clear();

			// Get the file version number generate by the FBX SDK.
			int lSDKMajor,  lSDKMinor,  lSDKRevision;
			FbxManager::GetFileFormatVersion( lSDKMajor, lSDKMinor, lSDKRevision );

			// Create the entity that will hold the scene.
			FbxScene*	pScene = FbxScene::Create( m_pTempSDKManager, "" );

			// Create an importer
			FbxImporter*	pImporter = FbxImporter::Create( m_pTempSDKManager,"" );
			try
			{
				// Initialize the importer by providing a filename.
				const char*		pFileName = Helpers::FromString( _FileName );
				const bool		bImportStatus = pImporter->Initialize( pFileName, -1, pIOSettings );

				int lFileMajor, lFileMinor, lFileRevision;
				pImporter->GetFileVersion( lFileMajor, lFileMinor, lFileRevision );

				if ( !bImportStatus )
				{
					System::String^	Report = "Call to FbxImporter::Initialize() failed.\n" +
											 "Error returned: " + Helpers::GetString( pImporter->GetLastErrorString() ) + "\n\n";

					if ( pImporter->GetLastErrorID() == FbxIO::eFileVersionNotSupportedYet ||
						 pImporter->GetLastErrorID() == FbxIO::eFileVersionNotSupportedAnymore )
					{
						Report += "FBX version number for this FBX SDK is " + lSDKMajor + "." + lSDKMinor + "." + lSDKRevision + "\n";
						Report += "FBX version number for file \"" + _FileName + "\" is " + lFileMajor + "." + lFileMinor + "." + lFileRevision + "\n\n";
					}

					throw gcnew Exception( Report );
				}

				if ( pImporter->IsFBX() )
				{	// Build animation takes
					int				AnimStacksCount = pImporter->GetAnimStackCount();
					System::String^	pCurrentTakeName = Helpers::GetString( pImporter->GetActiveAnimStackName().Buffer() );
					for ( int AnimStackIndex=0; AnimStackIndex < AnimStacksCount; AnimStackIndex++ )
					{
						FbxTakeInfo*	pTakeInfo = pImporter->GetTakeInfo( AnimStackIndex );
						Take^			NewTake = gcnew Take( AnimStackIndex, pTakeInfo );
						m_Takes->Add( NewTake );

						// Is this current take ??
						if ( NewTake->Name == pCurrentTakeName )
							m_CurrentTake = NewTake;	// This is our current take...
					}

					// Set the import states. By default, the import states are always set to true. The code below shows how to change these states.
					pIOSettings->SetBoolProp(IMP_FBX_MATERIAL,        true);
					pIOSettings->SetBoolProp(IMP_FBX_TEXTURE,         true);
					pIOSettings->SetBoolProp(IMP_FBX_LINK,            true);
					pIOSettings->SetBoolProp(IMP_FBX_SHAPE,           true);
					pIOSettings->SetBoolProp(IMP_FBX_GOBO,            true);
					pIOSettings->SetBoolProp(IMP_FBX_ANIMATION,       true);
					pIOSettings->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
				}

				// Import the scene.
				bool	bStatus = pImporter->Import( pScene );
				if ( !bStatus )
					throw gcnew Exception( "Failed to import \"" + _FileName + "\" ! Last Error : " + Helpers::GetString( pImporter->GetLastErrorString() ) );
			}
			catch ( Exception^ )
			{
				pScene->Destroy( true );
				throw;
			}
			finally
			{
				// Destroy the importer.
				pImporter->Destroy();
			}

			try
			{
				ProcessSceneData( pScene );
			}
			catch ( Exception^ _e )
			{
				throw gcnew Exception( "An error occurred while importing scene data!", _e );
			}
			finally
			{
				pScene->Destroy( true );
			}

			// Delete the FBX SDK manager. All the objects that have been allocated 
			// using the FBX SDK manager and that haven't been explicitly destroyed 
			// are automatically destroyed at the same time.
			if ( m_pTempSDKManager )
				m_pTempSDKManager->Destroy();
			m_pTempSDKManager = NULL;
		}

		// Finds a node by name
		//	_bThrowOnMultipleNodes, will throw an exception if multiple nodes are found with the same name
		//
		Node^			FindNode( String^ _NodeName )
		{
			return	FindNode( _NodeName, true );
		}
		Node^			FindNode( String^ _NodeName, bool _bThrowOnMultipleNodes )
		{
			if ( _NodeName == nullptr )
				return	nullptr;

			Node^	Result = nullptr;
			for ( int NodeIndex=0; NodeIndex < m_Nodes->Count; NodeIndex++ )
				if ( m_Nodes[NodeIndex]->Name == _NodeName )
				{
					if ( !_bThrowOnMultipleNodes )
						return	m_Nodes[NodeIndex];	// No use to look any further if we don't check duplicate names !

					if ( Result != nullptr )
						throw gcnew Exception( "There are more than one object with the name \"" + _NodeName + "\"!" );

					Result = m_Nodes[NodeIndex];
				}

			return	Result;
		}

	protected:

		void			ProcessSceneData( FbxScene* _pScene );
		Node^			CreateNodesHierarchy( Node^ _Parent, FbxNode* _pNode );

	internal:

		// Resolves a FBX material into one of our materials
		//
		Material^		ResolveMaterial( FbxSurfaceMaterial* _pMaterial )
		{
			if ( _pMaterial == NULL )
				return	nullptr;

			String^	MaterialName = Helpers::GetString( _pMaterial->GetName() );
			if ( m_Name2Material->ContainsKey( MaterialName ) )
				return	m_Name2Material[MaterialName];

			//////////////////////////////////////////////////////////////////////////
			// Build a brand new material
			Material^	NewMaterial = nullptr;

			// Check for hardward shader materials
            const FbxImplementation*	pImplementation = GetImplementation( _pMaterial, FBXSDK_IMPLEMENTATION_HLSL );
            if ( pImplementation != NULL )
				NewMaterial = gcnew MaterialHLSL( this, _pMaterial, pImplementation );
			else
			{
				pImplementation = GetImplementation( _pMaterial, FBXSDK_IMPLEMENTATION_CGFX );
				if ( pImplementation != NULL )
					NewMaterial = gcnew MaterialCGFX( this, _pMaterial, pImplementation );
				else
				{	// Standard materials
					FbxClassId	ClassID = _pMaterial->GetClassId();
					if ( ClassID.Is( FbxSurfaceLambert::ClassId ) )
					{
						FbxSurfaceLambert*	pMat = (FbxSurfaceLambert*) _pMaterial;
						NewMaterial = gcnew MaterialLambert( this, pMat );
					}
					else if ( ClassID.Is( FbxSurfacePhong::ClassId ) )
					{
						FbxSurfacePhong*	pMat = (FbxSurfacePhong*) _pMaterial;
						NewMaterial = gcnew MaterialPhong( this, pMat );
					}
					else
					{
						FbxSurfaceMaterial*	pMat = (FbxSurfaceMaterial*) _pMaterial;
						NewMaterial = gcnew Material( this, pMat );
					}
// 					else
// 						throw gcnew Exception( "Unsupported material class ID: " + Helpers::GetString( _pMaterial->GetClassId().GetName() ) + " for material \"" + Helpers::GetString( _pMaterial->GetName() ) + "\"!" );
				}
            }

			// Register it for later
			m_Materials->Add( NewMaterial );
			m_Name2Material->Add( MaterialName, NewMaterial );

			return	NewMaterial;
		}
	};
}
