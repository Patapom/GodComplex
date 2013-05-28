#pragma once
#include "stdafx.h"

class Node
{
	FbxNode&	m_Node;
	Node*		m_pParent;
	FbxMatrix	m_Local2Parent;
	FbxMatrix	m_Local2World;

public:
	Node( FbxNode& _Node, Node* _pParent );

	virtual void	Write( FILE* _pFile ) const;

protected:
	void	WriteInt( FILE* _pFile, int _Value );
	void	WriteFloat( FILE* _pFile, float _Value );
	void	WriteString( FILE* _pFile, const FbxString& _String );
};