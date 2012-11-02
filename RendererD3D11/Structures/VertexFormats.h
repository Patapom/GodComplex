#pragma once
#include "../Renderer.h"

class IVertexFormatDescriptor
{
public:	 // PROPERTIES

	virtual int			Size() const = 0;
	virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const = 0;
	virtual int			GetInputElementsCount() const = 0;
	virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const = 0;
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
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
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
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
	} DESCRIPTOR;

public:

	NjFloat3	P;

};

struct VertexFormatP3T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[2];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3T2); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 2; }
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
	} DESCRIPTOR;

public:

	NjFloat3	P;
	NjFloat2	UV;

};

struct VertexFormatP3N3G3T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[4];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T2); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 4; }
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
	} DESCRIPTOR;

public:

	NjFloat3	Position;
	NjFloat3	Normal;
	NjFloat3	Tangent;
	NjFloat2	UV;

};

struct VertexFormatP3N3G3T2T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[5];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T2T2); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 5; }
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
	} DESCRIPTOR;

public:

	NjFloat3	Position;
	NjFloat3	Normal;
	NjFloat3	Tangent;
	NjFloat2	UV;
	NjFloat2	UV2;

};

struct VertexFormatP3N3G3T2T3
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[5];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T2T3); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 5; }
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
	} DESCRIPTOR;

public:

	NjFloat3	Position;
	NjFloat3	Normal;
	NjFloat3	Tangent;
	NjFloat2	UV;
	NjFloat3	UV2;

};
