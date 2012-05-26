#pragma once
#include "../Renderer.h"

class IVertexFormatDescriptor
{
public:	 // PROPERTIES

	virtual int			Size() const = 0;
	virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const = 0;
	virtual int			GetInputElementsCount() const = 0;
};

struct VertexFormatPt4
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[1];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatPt4); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 1; }
	} DESCRIPTOR;

public:

	NjFloat4	Pt;

};

struct VertexFormatP3
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[1];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 1; }
	} DESCRIPTOR;

public:

	NjFloat3	P;

};