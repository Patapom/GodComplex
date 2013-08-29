// Contains the Mesh node class
//
#pragma managed
#pragma once

#include "Layers.h"
#include "Nodes.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// A Mesh node
	//
	public ref class		NodeSkeleton : public NodeWithAttribute
	{
	public:		// NESTED TYPES

	protected:	// FIELDS

		float						m_LimbLength;
		float						m_Size;


	public:		// PROPERTIES

		[DescriptionAttribute( "Gets the length of the skeleton limb (along the X axis)" )]
		// 
		property float		LimbLength
		{
			float		get()	{ return m_LimbLength; }
		}

		[DescriptionAttribute( "Gets the size of the skeleton node" )]
		// 
		property float		Size
		{
			float		get()	{ return m_Size; }
		}


	public:		// METHODS

		NodeSkeleton( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode );

	};
}
