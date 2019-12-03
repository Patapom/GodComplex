// This is the main DLL file.

#include "stdafx.h"

#include "NodeSkeleton.h"
#include "Scene.h"

using namespace	FBXImporter;

NodeSkeleton::NodeSkeleton( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : NodeWithAttribute( _ParentScene, _Parent, _pNode )
{
	FbxSkeleton*	pSkeleton = _pNode->GetSkeleton();

	m_LimbLength = (float) pSkeleton->LimbLength.Get();
	m_Size = (float) pSkeleton->Size.Get();
}
