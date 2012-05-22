#pragma once
#include "../Renderer.h"

struct VertexFormat
{
};

class VertexFormatDescriptor
{
protected:  // CONSTANTS

	static const char*	POSITION;
	static const char*	POSITION_TRANSFORMED;
// 	static const char*	NORMAL;
// 	static const char*	TANGENT;
// 	static const char*	BITANGENT;
// 	static const char*	COLOR;
// 	static const char*	VIEW;
// 	static const char*	CURVATURE;
// 	static const char*	TEXCOORD;	// In the shader, this semantic is written as TEXCOORD0, TEXCOORD1, etc.

public:	 // PROPERTIES

	virtual int			 Size() const = 0;
	virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const = 0;
	virtual int			 GetInputElementsCount() const = 0;
};

struct VertexFormatPt4 : public VertexFormat
{
public:

	static class Desc : public VertexFormatDescriptor
	{
		D3D11_INPUT_ELEMENT_DESC	m_pInputElements[1];

	public:

		Desc();
		virtual int			 Size() const							{ return sizeof(VertexFormatPt4); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return (D3D11_INPUT_ELEMENT_DESC*) m_pInputElements; }
		virtual int			 GetInputElementsCount() const			{ return 1; }
	} DESCRIPTOR;

public:

	NjFloat4	Pt;

};

struct VertexFormatP3 : public VertexFormat
{
public:

	static class Desc : public VertexFormatDescriptor
	{
		D3D11_INPUT_ELEMENT_DESC	m_pInputElements[1];

	public:

		Desc();
		virtual int			 Size() const							{ return sizeof(VertexFormatP3); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return (D3D11_INPUT_ELEMENT_DESC*) m_pInputElements; }
		virtual int			 GetInputElementsCount() const			{ return 1; }
	} DESCRIPTOR;

public:

	NjFloat3	P;

};