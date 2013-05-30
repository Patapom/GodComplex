#include "stdafx.h"


Node::Node( FbxNode& _Node, Node* _pParent ) : m_Node( _Node ), m_pParent( _pParent )
{
	FBXSDK_printf( "Node Name: %s\n", _Node.GetName() );

// 	DisplayUserProperties(&_Node);
// 	DisplayTarget(&_Node);
// 	DisplayPivotsAndLimits(&_Node);
// 	DisplayTransformPropagation(&_Node);
// 	DisplayGeometricTransform(&_Node);

	FbxVector4	T = _Node.GetGeometricTranslation( FbxNode::eSourcePivot );
	FbxVector4	R = _Node.GetGeometricRotation( FbxNode::eSourcePivot );
	FbxVector4	S = _Node.GetGeometricScaling( FbxNode::eSourcePivot );

	m_Local2Parent.SetTRS( T, R, S );

	if ( m_pParent != NULL )
	{
		m_Local2World = m_Local2Parent * m_pParent->m_Local2World;
	}
	else
		m_Local2World = m_Local2Parent;
}

void	Node::Write( FILE* _pFile ) const
{

}

void	Node::WriteInt( FILE* _pFile, int _Value )
{
	fwrite( &_Value, sizeof(int), 1, _pFile );
}

void	Node::WriteFloat( FILE* _pFile, float _Value )
{
	fwrite( &_Value, sizeof(float), 1, _pFile );
}

void	Node::WriteString( FILE* _pFile, const FbxString& _String )
{
	int	TextSize = (int) _String.GetLen();
	WriteInt( _pFile, TextSize );

	const char*	pText = _String.Buffer();
	fwrite( pText, sizeof(char), TextSize, _pFile );
}
