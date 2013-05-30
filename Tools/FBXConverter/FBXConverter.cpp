// FBXConverter.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#undef FBXSDK_printf
#define FBXSDK_printf	ConsolePrint

void	ConsolePrint( const char* _pText, ... )
{
	va_list	argptr;
	va_start( argptr,  _pText );

	char	pTemp[16384];
	vsprintf_s( pTemp, 16384, _pText, argptr );

	OutputDebugString( pTemp );
}

void	DisplayString( const char* _pText, const char* _pString=NULL )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %s\n", _pText, _pString );
	FBXSDK_printf( pTemp );
}

void	DisplayBool( const char* _pText, bool _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %s\n", _pText, _Value ? "true" : "false" );
	FBXSDK_printf( pTemp );
}

void	DisplayDouble( const char* _pText, double _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %f\n", _pText, _Value );
	FBXSDK_printf( pTemp );
}

void	DisplayInt( const char* _pText, int _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %d\n", _pText, _Value );
	FBXSDK_printf( pTemp );
}

void	Display2DVector( const char* _pText, const FbxVector2& _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %f, %f\n", _pText, _Value[0], _Value[1] );
	FBXSDK_printf( pTemp );
}

void	Display3DVector( const char* _pText, const FbxVector4& _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %f, %f, %f\n", _pText, _Value[0], _Value[1], _Value[2] );
	FBXSDK_printf( pTemp );
}

void	Display4DVector( const char* _pText, const FbxVector4& _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %f, %f, %f, %f\n", _pText, _Value[0], _Value[1], _Value[2], _Value[3] );
	FBXSDK_printf( pTemp );
}

void	DisplayColor( const char* _pText, const FbxColor& _Value )
{
	char	pTemp[16384];
	sprintf_s( pTemp, 16384, "%s %f, %f, %f, %f\n", _pText, _Value[0], _Value[1], _Value[2], _Value[3] );
	FBXSDK_printf( pTemp );
}

void	InitializeSdkObjects( FbxManager*& _pManager, FbxScene*& _pScene )
{
	//The first thing to do is to create the FBX Manager which is the object allocator for almost all the classes in the SDK
	_pManager = FbxManager::Create();
	if( !_pManager )
	{
		FBXSDK_printf("Error: Unable to create FBX Manager!\n");
		exit(1);
	}
	else
		FBXSDK_printf("Autodesk FBX SDK version %s\n", _pManager->GetVersion());

	// Create an IOSettings object. This object holds all import/export settings.
	FbxIOSettings* ios = FbxIOSettings::Create( _pManager, IOSROOT );
	_pManager->SetIOSettings( ios );

	//Load plugins from the executable directory (optional)
	FbxString lPath = FbxGetApplicationDirectory();
	_pManager->LoadPluginsDirectory(lPath.Buffer());

	//Create an FBX scene. This object holds most objects imported/exported from/to files.
	_pScene = FbxScene::Create( _pManager, "My Scene" );
	if( !_pScene )
	{
		FBXSDK_printf("Error: Unable to create FBX scene!\n");
		exit(1);
	}
}

bool	LoadScene( FbxManager* _pManager, FbxDocument* _pScene, const char* _pFilename )
{

	int i, lAnimStackCount;
	bool lStatus;

	// Get the file version number generate by the FBX SDK.
	int SDKMajor,  SDKMinor,  SDKRevision;
	FbxManager::GetFileFormatVersion( SDKMajor,  SDKMinor,  SDKRevision);

	// Create an importer.
	FbxImporter* pImporter = FbxImporter::Create(_pManager,"");

	// Initialize the importer by providing a filename.
	const bool lImportStatus = pImporter->Initialize(_pFilename, -1, _pManager->GetIOSettings());
	int	FileMajor, FileMinor, FileRevision;
	pImporter->GetFileVersion( FileMajor, FileMinor, FileRevision );

	if( !lImportStatus )
	{
		FBXSDK_printf("Call to FbxImporter::Initialize() failed.\n");
		FBXSDK_printf("Error returned: %s\n\n", pImporter->GetLastErrorString());

		if (pImporter->GetLastErrorID() == FbxIOBase::eFileVersionNotSupportedYet ||
			pImporter->GetLastErrorID() == FbxIOBase::eFileVersionNotSupportedAnymore)
		{
			FBXSDK_printf("FBX file format version for this FBX SDK is %d.%d.%d\n", SDKMajor, SDKMinor, SDKRevision);
			FBXSDK_printf("FBX file format version for file '%s' is %d.%d.%d\n\n", _pFilename, FileMajor, FileMinor, FileRevision);
		}

		return false;
	}

	FBXSDK_printf("FBX file format version for this FBX SDK is %d.%d.%d\n", SDKMajor, SDKMinor, SDKRevision);

	if (pImporter->IsFBX())
	{
		FBXSDK_printf("FBX file format version for file '%s' is %d.%d.%d\n\n", _pFilename, FileMajor, FileMinor, FileRevision);

		// From this point, it is possible to access animation stack information without
		// the expense of loading the entire file.

		FBXSDK_printf("Animation Stack Information\n");

		lAnimStackCount = pImporter->GetAnimStackCount();

		FBXSDK_printf("    Number of Animation Stacks: %d\n", lAnimStackCount);
		FBXSDK_printf("    Current Animation Stack: \"%s\"\n", pImporter->GetActiveAnimStackName().Buffer());
		FBXSDK_printf("\n");

		for(i = 0; i < lAnimStackCount; i++)
		{
			FbxTakeInfo* lTakeInfo = pImporter->GetTakeInfo(i);

			FBXSDK_printf("    Animation Stack %d\n", i);
			FBXSDK_printf("         Name: \"%s\"\n", lTakeInfo->mName.Buffer());
			FBXSDK_printf("         Description: \"%s\"\n", lTakeInfo->mDescription.Buffer());

			// Change the value of the import name if the animation stack should be imported 
			// under a different name.
			FBXSDK_printf("         Import Name: \"%s\"\n", lTakeInfo->mImportName.Buffer());

			// Set the value of the import state to false if the animation stack should be not
			// be imported. 
			FBXSDK_printf("         Import State: %s\n", lTakeInfo->mSelect ? "true" : "false");
			FBXSDK_printf("\n");
		}

		// Set the import states. By default, the import states are always set to 
		// true. The code below shows how to change these states.
// 		IOS_REF.SetBoolProp(IMP_FBX_MATERIAL,        true);
// 		IOS_REF.SetBoolProp(IMP_FBX_TEXTURE,         true);
// 		IOS_REF.SetBoolProp(IMP_FBX_LINK,            true);
// 		IOS_REF.SetBoolProp(IMP_FBX_SHAPE,           true);
// 		IOS_REF.SetBoolProp(IMP_FBX_GOBO,            true);
// 		IOS_REF.SetBoolProp(IMP_FBX_ANIMATION,       true);
// 		IOS_REF.SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
	}

	// Import the scene.
	lStatus = pImporter->Import(_pScene);

// 	if(lStatus == false && pImporter->GetLastErrorID() == FbxIOBase::ePasswordError)
// 	{
//		char lPassword[1024];
// 		FBXSDK_printf("Please enter password: ");
// 
// 		lPassword[0] = '\0';
// 
// 		FBXSDK_CRT_SECURE_NO_WARNING_BEGIN
// 		scanf("%s", lPassword);
// 		FBXSDK_CRT_SECURE_NO_WARNING_END
// 
// 		FbxString lString(lPassword);
// 
// 		IOS_REF.SetStringProp(IMP_FBX_PASSWORD,      lString);
// 		IOS_REF.SetBoolProp(IMP_FBX_PASSWORD_ENABLE, true);
// 
// 		lStatus = pImporter->Import(_pScene);
// 
// 		if(lStatus == false && pImporter->GetLastErrorID() == FbxIOBase::ePasswordError)
// 		{
// 			FBXSDK_printf("\nPassword is wrong, import aborted.\n");
// 		}
// 	}

	// Destroy the importer.
	pImporter->Destroy();

	return lStatus;
}

void	DisplayTransformPropagation(FbxNode* pNode)
{
	FBXSDK_printf("    Transformation Propagation\n");

	// 
	// Rotation Space
	//
	EFbxRotationOrder lRotationOrder;
	pNode->GetRotationOrder(FbxNode::eSourcePivot, lRotationOrder);

	FBXSDK_printf("        Rotation Space: ");

	switch (lRotationOrder)
	{
	case eEulerXYZ: 
		FBXSDK_printf("Euler XYZ\n");
		break;
	case eEulerXZY:
		FBXSDK_printf("Euler XZY\n");
		break;
	case eEulerYZX:
		FBXSDK_printf("Euler YZX\n");
		break;
	case eEulerYXZ:
		FBXSDK_printf("Euler YXZ\n");
		break;
	case eEulerZXY:
		FBXSDK_printf("Euler ZXY\n");
		break;
	case eEulerZYX:
		FBXSDK_printf("Euler ZYX\n");
		break;
	case eSphericXYZ:
		FBXSDK_printf("Spheric XYZ\n");
		break;
	}

	//
	// Use the Rotation space only for the limits
	// (keep using eEulerXYZ for the rest)
	//
	FBXSDK_printf("        Use the Rotation Space for Limit specification only: %s\n",
		pNode->GetUseRotationSpaceForLimitOnly(FbxNode::eSourcePivot) ? "Yes" : "No");


	//
	// Inherit Type
	//
	FbxTransform::EInheritType lInheritType;
	pNode->GetTransformationInheritType(lInheritType);

	FBXSDK_printf("        Transformation Inheritance: ");

	switch (lInheritType)
	{
	case FbxTransform::eInheritRrSs:
		FBXSDK_printf("RrSs\n");
		break;
	case FbxTransform::eInheritRSrs:
		FBXSDK_printf("RSrs\n");
		break;
	case FbxTransform::eInheritRrs:
		FBXSDK_printf("Rrs\n");
		break;
	}
}

void	DisplayGeometricTransform(FbxNode* pNode)
{
	FbxVector4 lTmpVector;

	FBXSDK_printf("    Geometric Transformations\n");

	//
	// Translation
	//
	lTmpVector = pNode->GetGeometricTranslation(FbxNode::eSourcePivot);
	FBXSDK_printf("        Translation: %f %f %f\n", lTmpVector[0], lTmpVector[1], lTmpVector[2]);

	//
	// Rotation
	//
	lTmpVector = pNode->GetGeometricRotation(FbxNode::eSourcePivot);
	FBXSDK_printf("        Rotation:    %f %f %f\n", lTmpVector[0], lTmpVector[1], lTmpVector[2]);

	//
	// Scaling
	//
	lTmpVector = pNode->GetGeometricScaling(FbxNode::eSourcePivot);
	FBXSDK_printf("        Scaling:     %f %f %f\n", lTmpVector[0], lTmpVector[1], lTmpVector[2]);
}

void	DisplayPolygons( FbxMesh* _pMesh )
{
	int	PolygonsCount = _pMesh->GetPolygonCount();
	FbxVector4* lControlPoints = _pMesh->GetControlPoints(); 

	DisplayString("    Polygons", "" );

	int vertexId = 0;
	for ( int i=0; i < PolygonsCount; i++ )
	{
		DisplayInt("        Polygon ", i);
		for ( int l=0; l < _pMesh->GetElementPolygonGroupCount(); l++ )
		{
			FbxGeometryElementPolygonGroup*	PolygonGroup = _pMesh->GetElementPolygonGroup( l );
			switch ( PolygonGroup->GetMappingMode() )
			{
			case FbxGeometryElement::eByPolygon:
				if (PolygonGroup->GetReferenceMode() == FbxGeometryElement::eIndex)
				{
//					FBXSDK_printf( "        Assigned to group: " );
					int	polyGroupId = PolygonGroup->GetIndexArray().GetAt(i);
					DisplayInt( "group", polyGroupId);
					break;
				}
			default:
				// any other mapping modes don't make sense
				DisplayString("        \"unsupported group assignment\"", "");
				break;
			}
		}

		int lPolygonSize = _pMesh->GetPolygonSize(i);

		for ( int j=0; j < lPolygonSize; j++ )
		{
			int	lControlPointIndex = _pMesh->GetPolygonVertex( i, j );

			Display3DVector("            Coordinates: ", lControlPoints[lControlPointIndex]);

			for ( int l=0; l < _pMesh->GetElementVertexColorCount(); l++ )
			{
				FbxGeometryElementVertexColor* leVtxc = _pMesh->GetElementVertexColor( l);
				FBXSDK_printf( "            Color vertex: "); 

				switch (leVtxc->GetMappingMode())
				{
				case FbxGeometryElement::eByControlPoint:
					switch (leVtxc->GetReferenceMode())
					{
					case FbxGeometryElement::eDirect:
						DisplayColor( "", leVtxc->GetDirectArray().GetAt(lControlPointIndex));
						break;
					case FbxGeometryElement::eIndexToDirect:
						{
							int id = leVtxc->GetIndexArray().GetAt(lControlPointIndex);
							DisplayColor( "", leVtxc->GetDirectArray().GetAt(id));
						}
						break;
					default:
						break; // other reference modes not shown here!
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						switch (leVtxc->GetReferenceMode())
						{
						case FbxGeometryElement::eDirect:
							DisplayColor( "", leVtxc->GetDirectArray().GetAt(vertexId));
							break;
						case FbxGeometryElement::eIndexToDirect:
							{
								int id = leVtxc->GetIndexArray().GetAt(vertexId);
								DisplayColor( "", leVtxc->GetDirectArray().GetAt(id));
							}
							break;
						default:
							break; // other reference modes not shown here!
						}
					}
					break;

				case FbxGeometryElement::eByPolygon: // doesn't make much sense for UVs
				case FbxGeometryElement::eAllSame:   // doesn't make much sense for UVs
				case FbxGeometryElement::eNone:       // doesn't make much sense for UVs
					break;
				}
			}
			for ( int l=0; l < _pMesh->GetElementUVCount(); ++l )
			{
				FbxGeometryElementUV* leUV = _pMesh->GetElementUV( l);
				FBXSDK_printf( "UV stream #%d: ", l ); 

				switch ( leUV->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					switch (leUV->GetReferenceMode())
					{
					case FbxGeometryElement::eDirect:
						Display2DVector( "", leUV->GetDirectArray().GetAt(lControlPointIndex));
						break;
					case FbxGeometryElement::eIndexToDirect:
						{
							int id = leUV->GetIndexArray().GetAt(lControlPointIndex);
							Display2DVector( "", leUV->GetDirectArray().GetAt(id));
 						}
						break;
					default:
						break; // other reference modes not shown here!
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int lTextureUVIndex = _pMesh->GetTextureUVIndex(i, j);
						switch (leUV->GetReferenceMode())
						{
						case FbxGeometryElement::eDirect:
						case FbxGeometryElement::eIndexToDirect:
							Display2DVector( "", leUV->GetDirectArray().GetAt(lTextureUVIndex) );
							break;
						default:
							break; // other reference modes not shown here!
						}
					}
					break;

				case FbxGeometryElement::eByPolygon:	// doesn't make much sense for UVs
				case FbxGeometryElement::eAllSame:		// doesn't make much sense for UVs
				case FbxGeometryElement::eNone:			// doesn't make much sense for UVs
					break;
				}
			}

			for( int l=0; l < _pMesh->GetElementNormalCount(); ++l )
			{
				FbxGeometryElementNormal*	leNormal = _pMesh->GetElementNormal( l);
				FBXSDK_printf( "Normal stream #%d: ", l ); 

				if ( leNormal->GetMappingMode() == FbxGeometryElement::eByPolygonVertex )
				{
					switch ( leNormal->GetReferenceMode() )
					{
					case FbxGeometryElement::eDirect:
						Display3DVector( "", leNormal->GetDirectArray().GetAt(vertexId));
						break;
					case FbxGeometryElement::eIndexToDirect:
						{
							int id = leNormal->GetIndexArray().GetAt(vertexId);
							Display3DVector( "", leNormal->GetDirectArray().GetAt(id));
						}
						break;
					default:
						break; // other reference modes not shown here!
					}
				}

			}
			for( int l=0; l < _pMesh->GetElementTangentCount(); ++l )
			{
				FbxGeometryElementTangent* leTangent = _pMesh->GetElementTangent( l);
				FBXSDK_printf( "Tangent stream #%d: ", l ); 

				if ( leTangent->GetMappingMode() == FbxGeometryElement::eByPolygonVertex )
				{
					switch (leTangent->GetReferenceMode())
					{
					case FbxGeometryElement::eDirect:
						Display3DVector( "", leTangent->GetDirectArray().GetAt(vertexId));
						break;
					case FbxGeometryElement::eIndexToDirect:
						{
							int id = leTangent->GetIndexArray().GetAt(vertexId);
							Display3DVector( "", leTangent->GetDirectArray().GetAt(id));
						}
						break;
					default:
						break; // other reference modes not shown here!
					}
				}

			}

			for ( int l=0; l < _pMesh->GetElementBinormalCount(); ++l )
			{

				FbxGeometryElementBinormal* leBinormal = _pMesh->GetElementBinormal( l);

				FBXSDK_printf( "Binormal stream #%d: ", l ); 
				if ( leBinormal->GetMappingMode() == FbxGeometryElement::eByPolygonVertex )
				{
					switch ( leBinormal->GetReferenceMode() )
					{
					case FbxGeometryElement::eDirect:
						Display3DVector( "", leBinormal->GetDirectArray().GetAt(vertexId));
						break;
					case FbxGeometryElement::eIndexToDirect:
						{
							int id = leBinormal->GetIndexArray().GetAt(vertexId);
							Display3DVector( "", leBinormal->GetDirectArray().GetAt(id));
						}
						break;
					default:
						break; // other reference modes not shown here!
					}
				}
			}
			vertexId++;
		}	// for polygonSize
	}	// for polygonCount


//	//check visibility for the edges of the mesh
//	for(int l = 0; l < _pMesh->GetElementVisibilityCount(); ++l)
//	{
//		FbxGeometryElementVisibility* leVisibility=_pMesh->GetElementVisibility(l);
//		FBXSDK_printf(header, 100, "    Edge Visibility : ");
//		DisplayString(header);
//		switch(leVisibility->GetMappingMode())
//		{
//			//should be eByEdge
//		case FbxGeometryElement::eByEdge:
//			//should be eDirect
//			for(int j=0; j!=pMesh->GetMeshEdgeCount();++j)
//			{
//				DisplayInt("        Edge ", j);
//				DisplayBool("              Edge visibility: ", leVisibility->GetDirectArray().GetAt(j));
//			}
//	
//			break;
//		}
//	}
//	   DisplayString("");
}

void	DisplayMaterialMapping(FbxMesh* pMesh)
{
	const char* lMappingTypes[] = { "None", "By Control Point", "By Polygon Vertex", "By Polygon", "By Edge", "All Same" };
	const char* lReferenceMode[] = { "Direct", "Index", "Index to Direct"};

	FbxNode*	pNode = pMesh->GetNode();
	int			MaterialsCount = pNode != NULL ? pNode->GetMaterialCount() : 0;

	for ( int l=0; l < pMesh->GetElementMaterialCount(); l++ )
	{
		FbxGeometryElementMaterial*	pMat = pMesh->GetElementMaterial( l );
		if ( pMat == NULL )
			continue;

		FBXSDK_printf( "    Material Element %d: ", l ); 
 
 		DisplayString("           Mapping: ", lMappingTypes[pMat->GetMappingMode()]);
 		DisplayString("           ReferenceMode: ", lReferenceMode[pMat->GetReferenceMode()]);

		if (pMat->GetReferenceMode() == FbxGeometryElement::eIndex ||
			pMat->GetReferenceMode() == FbxGeometryElement::eIndexToDirect)
		{
			FbxString	lString = "           Indices: ";

			int lIndexArrayCount = pMat->GetIndexArray().GetCount(); 
			for ( int i=0; i < lIndexArrayCount; i++)
			{
				lString += pMat->GetIndexArray().GetAt(i);

				if (i < lIndexArrayCount - 1)
				{
					lString += ", ";
				}
			}

			lString += "\n";

			FBXSDK_printf(lString);
		}
	}

//	DisplayString("");
}

void	DisplayMaterial( FbxGeometry* pGeometry )
{
	if ( pGeometry == NULL )
		return;

	FbxNode*	pNode = pGeometry->GetNode();
	int MaterialsCount = pNode != NULL ? pNode->GetMaterialCount() : NULL;
	if ( MaterialsCount <= 0 )
		return;

	FbxPropertyT<FbxDouble3> lKFbxDouble3;
	FbxPropertyT<FbxDouble> lKFbxDouble1;
	FbxColor theColor;

	for ( int MatIndex=0; MatIndex < MaterialsCount; MatIndex++ )
	{
		DisplayInt("        Material ", MatIndex);

		FbxSurfaceMaterial*	pMat = pNode->GetMaterial( MatIndex );

		DisplayString( "            Name: \"", (char *) pMat->GetName() ); 

		//Get the implementation to see if it's a hardware shader.
		const FbxImplementation*	Implementation = GetImplementation( pMat, FBXSDK_IMPLEMENTATION_HLSL );
		FbxString lImplemenationType = "HLSL";
		if ( Implementation == NULL )
		{
			Implementation = GetImplementation( pMat, FBXSDK_IMPLEMENTATION_CGFX );
			lImplemenationType = "CGFX";
		}
		if ( Implementation != NULL )
		{
			//Now we have a hardware shader, let's read it
			DisplayString("            Hardware Shader Type:", lImplemenationType.Buffer());
			FbxBindingTable const*	RootTable = Implementation->GetRootTable();
			FbxString				FileName = RootTable->DescAbsoluteURL.Get();
			FbxString				TechniqueName = RootTable->DescTAG.Get(); 


			FbxBindingTable const*	Table = Implementation->GetRootTable();
			size_t	EntriesCount = Table->GetEntryCount();

			for( int i=0; i < (int) EntriesCount; ++i )
			{
				const FbxBindingTableEntry&	Entry = Table->GetEntry(i);
				const char* lEntrySrcType = Entry.GetEntryType(true); 
				FbxProperty Property;


				FbxString lTest = Entry.GetSource();
				FBXSDK_printf("            Entry: %s\n", lTest.Buffer());


				if ( strcmp( FbxPropertyEntryView::sEntryType, lEntrySrcType ) == 0 )
				{   
					Property = pMat->FindPropertyHierarchical(Entry.GetSource()); 
					if ( !Property.IsValid() )
					{
						Property = pMat->RootProperty.FindHierarchical(Entry.GetSource());
					}
				}
				else if( strcmp( FbxConstantEntryView::sEntryType, lEntrySrcType ) == 0 )
				{
					Property = Implementation->GetConstants().FindHierarchical(Entry.GetSource());
				}
				if ( Property.IsValid() )
				{
					if ( Property.GetSrcObjectCount<FbxTexture>() > 0 )
					{
						//do what you want with the textures
						for ( int j=0; j<Property.GetSrcObjectCount<FbxFileTexture>(); ++j )
						{
							FbxFileTexture *lTex = Property.GetSrcObject<FbxFileTexture>( j );
							FBXSDK_printf( "           File Texture: %s\n", lTex->GetFileName() );
						}
						for ( int j=0; j<Property.GetSrcObjectCount<FbxLayeredTexture>(); ++j )
						{
							FbxLayeredTexture *lTex = Property.GetSrcObject<FbxLayeredTexture>( j );
							FBXSDK_printf( "        Layered Texture: %s\n", lTex->GetName() );
						}
						for ( int j=0; j<Property.GetSrcObjectCount<FbxProceduralTexture>(); ++j )
						{
							FbxProceduralTexture *lTex = Property.GetSrcObject<FbxProceduralTexture>(j);
							FBXSDK_printf( "     Procedural Texture: %s\n", lTex->GetName() );
						}
					}
					else
					{
						FbxDataType	PropDataType = Property.GetPropertyDataType();
//						FbxString blah = PropDataType.GetName();
						if ( FbxBoolDT == PropDataType )
						{
							DisplayBool("                Bool: ", Property.Get<FbxBool>() );
						}
						else if ( FbxIntDT == PropDataType || FbxEnumDT == PropDataType )
						{
							DisplayInt("                Int: ", Property.Get<FbxInt>());
						}
						else if ( FbxFloatDT == PropDataType)
						{
							DisplayDouble("                Float: ", Property.Get<FbxFloat>());

						}
						else if ( FbxDoubleDT == PropDataType)
						{
							DisplayDouble("                Double: ", Property.Get<FbxDouble>());
						}
						else if ( FbxStringDT == PropDataType
							||  FbxUrlDT  == PropDataType
							||  FbxXRefUrlDT  == PropDataType )
						{
							DisplayString("                String: ", Property.Get<FbxString>().Buffer());
						}
						else if ( FbxDouble2DT == PropDataType)
						{
							FbxDouble2 lDouble2 = Property.Get<FbxDouble2>();
							FbxVector2 lVect;
							lVect[0] = lDouble2[0];
							lVect[1] = lDouble2[1];

							Display2DVector("                2D vector: ", lVect);
						}
						else if ( FbxDouble3DT == PropDataType || FbxColor3DT == PropDataType)
						{
							FbxDouble3 lDouble3 = Property.Get<FbxDouble3>();


							FbxVector4 lVect;
							lVect[0] = lDouble3[0];
							lVect[1] = lDouble3[1];
							lVect[2] = lDouble3[2];
							Display3DVector("                3D vector: ", lVect);
						}

						else if ( FbxDouble4DT == PropDataType || FbxColor4DT == PropDataType)
						{
							FbxDouble4 lDouble4 = Property.Get<FbxDouble4>();
							FbxVector4 lVect;
							lVect[0] = lDouble4[0];
							lVect[1] = lDouble4[1];
							lVect[2] = lDouble4[2];
							lVect[3] = lDouble4[3];
							Display4DVector("                4D vector: ", lVect);
						}
						else if ( FbxDouble4x4DT == PropDataType)
						{
							FbxDouble4x4 lDouble44 = Property.Get<FbxDouble4x4>();
							for(int j=0; j<4; ++j)
							{

								FbxVector4 lVect;
								lVect[0] = lDouble44[j][0];
								lVect[1] = lDouble44[j][1];
								lVect[2] = lDouble44[j][2];
								lVect[3] = lDouble44[j][3];
								Display4DVector("                4x4D vector: ", lVect);
							}

						}
					}

				}   
			}
		}
		else if (pMat->GetClassId().Is(FbxSurfacePhong::ClassId))
		{
			// We found a Phong material.  Display its properties.

			// Display the Ambient Color
			lKFbxDouble3 =((FbxSurfacePhong *) pMat)->Ambient;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Ambient: ", theColor);

			// Display the Diffuse Color
			lKFbxDouble3 =((FbxSurfacePhong *) pMat)->Diffuse;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Diffuse: ", theColor);

			// Display the Specular Color (unique to Phong materials)
			lKFbxDouble3 =((FbxSurfacePhong *) pMat)->Specular;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Specular: ", theColor);

			// Display the Emissive Color
			lKFbxDouble3 =((FbxSurfacePhong *) pMat)->Emissive;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Emissive: ", theColor);

			//Opacity is Transparency factor now
			lKFbxDouble1 =((FbxSurfacePhong *) pMat)->TransparencyFactor;
			DisplayDouble("            Opacity: ", 1.0-lKFbxDouble1.Get());

			// Display the Shininess
			lKFbxDouble1 =((FbxSurfacePhong *) pMat)->Shininess;
			DisplayDouble("            Shininess: ", lKFbxDouble1.Get());

			// Display the Reflectivity
			lKFbxDouble1 =((FbxSurfacePhong *) pMat)->ReflectionFactor;
			DisplayDouble("            Reflectivity: ", lKFbxDouble1.Get());
		}
		else if(pMat->GetClassId().Is(FbxSurfaceLambert::ClassId) )
		{
			// We found a Lambert material. Display its properties.
			// Display the Ambient Color
			lKFbxDouble3=((FbxSurfaceLambert *)pMat)->Ambient;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Ambient: ", theColor);

			// Display the Diffuse Color
			lKFbxDouble3 =((FbxSurfaceLambert *)pMat)->Diffuse;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Diffuse: ", theColor);

			// Display the Emissive
			lKFbxDouble3 =((FbxSurfaceLambert *)pMat)->Emissive;
			theColor.Set(lKFbxDouble3.Get()[0], lKFbxDouble3.Get()[1], lKFbxDouble3.Get()[2]);
			DisplayColor("            Emissive: ", theColor);

			// Display the Opacity
			lKFbxDouble1 =((FbxSurfaceLambert *)pMat)->TransparencyFactor;
			DisplayDouble("            Opacity: ", 1.0-lKFbxDouble1.Get());
		}
 		else
 			DisplayString("Unknown type of Material", "");

		FbxPropertyT<FbxString> lString;
		lString = pMat->ShadingModel;
		DisplayString("            Shading Model: ", lString.Get().Buffer());
//		DisplayString("");
	}
}

void	FindAndDisplayTextureInfoByProperty(FbxProperty pProperty, int pMaterialIndex)
{
	if ( !pProperty.IsValid() )
		return;

	bool	bDisplayHeader = true;

	int lTextureCount = pProperty.GetSrcObjectCount<FbxTexture>();

	for (int j = 0; j < lTextureCount; ++j)
	{
		//Here we have to check if it's layeredtextures, or just textures:
		FbxLayeredTexture *lLayeredTexture = pProperty.GetSrcObject<FbxLayeredTexture>(j);
		if (lLayeredTexture)
		{
			DisplayInt("    Layered Texture: ", j);
			FbxLayeredTexture *lLayeredTexture = pProperty.GetSrcObject<FbxLayeredTexture>(j);
			int lNbTextures = lLayeredTexture->GetSrcObjectCount<FbxTexture>();
			for(int k =0; k<lNbTextures; ++k)
			{
				FbxTexture* lTexture = lLayeredTexture->GetSrcObject<FbxTexture>(k);
				if(lTexture)
				{

					if(bDisplayHeader){                    
						DisplayInt("    Textures connected to Material ", pMaterialIndex);
						bDisplayHeader = false;
					}

					//NOTE the blend mode is ALWAYS on the LayeredTexture and NOT the one on the texture.
					//Why is that?  because one texture can be shared on different layered textures and might
					//have different blend modes.

					FbxLayeredTexture::EBlendMode lBlendMode;
					lLayeredTexture->GetTextureBlendMode(k, lBlendMode);
					DisplayString("    Textures for ", pProperty.GetName());
					DisplayInt("        Texture ", k);  
//					DisplayTextureInfo(lTexture, (int) lBlendMode);   
				}

			}
		}
		else
		{
			//no layered texture simply get on the property
			FbxTexture* lTexture = pProperty.GetSrcObject<FbxTexture>(j);
			if(lTexture)
			{
				//display connected Material header only at the first time
				if(bDisplayHeader){                    
					DisplayInt("    Textures connected to Material ", pMaterialIndex);
					bDisplayHeader = false;
				}             

				DisplayString("    Textures for ", pProperty.GetName());
				DisplayInt("        Texture ", j);  
//				DisplayTextureInfo(lTexture, -1);
			}
		}
	}
}

void	DisplayTexture(FbxGeometry* pGeometry)
{
	if ( pGeometry->GetNode() == NULL )
		return;

	int MatsCount = pGeometry->GetNode()->GetSrcObjectCount<FbxSurfaceMaterial>();
	for ( int MatIndex=0; MatIndex < MatsCount; MatIndex++ )
	{
		FbxSurfaceMaterial *pMat = pGeometry->GetNode()->GetSrcObject<FbxSurfaceMaterial>( MatIndex );
		if ( pMat == NULL )
			continue;

		//go through all the possible textures

		int TextureIndex;
		FBXSDK_FOR_EACH_TEXTURE(TextureIndex)
		{
			FbxProperty Property = pMat->FindProperty( FbxLayerElement::sTextureChannelNames[TextureIndex] );
			FindAndDisplayTextureInfoByProperty( Property, MatIndex );
		}

	}	// end for MatIndex     
}

// void DisplayContent( FbxScene* pScene )
// {
// 	FbxNode* pNode = pScene->GetRootNode();
// 	if ( pNode == NULL )
// 		return;
// 
// 	for ( int i=0; i < pNode->GetChildCount(); i++ )
// 	{
// 		DisplayContent( pNode->GetChild(i) );
// 	}
// }

void DisplayContent( FbxNode* _pNode, Node* _pParent )
{
	Node*	pMyNode = NULL;

	if( _pNode->GetNodeAttribute() == NULL )
	{
		FBXSDK_printf("NULL Node Attribute\n\n");
	}
	else
	{
		FbxNodeAttribute::EType	lAttributeType = _pNode->GetNodeAttribute()->GetAttributeType();
		switch ( lAttributeType )
		{
// 		case FbxNodeAttribute::eMarker:  
// 			DisplayMarker(pNode);
// 			break;
// 
// 		case FbxNodeAttribute::eSkeleton:  
// 			DisplaySkeleton(pNode);
// 			break;

		case FbxNodeAttribute::eMesh:
			{
				FbxMesh*	pMesh = (FbxMesh*) _pNode->GetNodeAttribute();
				pMyNode = new Mesh( *pMesh, _pParent );
			}
			break;

// 		case FbxNodeAttribute::eNurbs:      
// 			DisplayNurb(pNode);
// 			break;
// 
// 		case FbxNodeAttribute::ePatch:     
// 			DisplayPatch(pNode);
// 			break;
// 
// 		case FbxNodeAttribute::eCamera:    
// 			DisplayCamera(pNode);
// 			break;
// 
// 		case FbxNodeAttribute::eLight:     
// 			DisplayLight(pNode);
// 			break;
// 
// 		case FbxNodeAttribute::eLODGroup:
// 			DisplayLodGroup(pNode);
// 			break;
		}   
	}

	if ( pMyNode == NULL )
		pMyNode = new Node( *_pNode, _pParent );

	for ( int i=0; i < _pNode->GetChildCount(); i++ )
		DisplayContent( _pNode->GetChild( i ), pMyNode );
}

int _tmain( int argc, char* argv[] )
{
	// The example can take a FBX file as an argument.
	if ( argc != 3 )
	{
		FBXSDK_printf("\n\nUsage: FBXConverter <FBX file name> <Target file name.scene>\n\n");
		return -1;
	}

	FbxString	SourceFilePath = argv[1];
	FbxString	TargetFilePath = argv[2];

	// Prepare the FBX SDK.
	FbxManager*	Manager = NULL;
    FbxScene*	Scene = NULL;
    InitializeSdkObjects( Manager, Scene );

	if ( !LoadScene( Manager, Scene, SourceFilePath.Buffer() ) )
	{
        FBXSDK_printf("\n\nAn error occurred while loading the scene...");
		return -1;
	}

	DisplayContent( Scene->GetRootNode(), NULL );

	if ( Manager )
		Manager->Destroy();

	return 0;
}
