#pragma once
#include "../Renderer.h"

class IVertexFormatDescriptor
{
public:	 // PROPERTIES

	virtual int			Size() const = 0;
	virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const = 0;
	virtual int			GetInputElementsCount() const = 0;
	virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const = 0;

	bool		operator==( const IVertexFormatDescriptor& _Other ) const
	{
#ifdef _DEBUG
		if ( Size() != _Other.Size() )
			return false;
		if ( GetInputElementsCount() != _Other.GetInputElementsCount() )
			return false;

		const D3D11_INPUT_ELEMENT_DESC*	pDescs0 = GetInputElements();
		const D3D11_INPUT_ELEMENT_DESC*	pDescs1 = _Other.GetInputElements();
		for ( int i=0; i < GetInputElementsCount(); i++ )
		{
			const D3D11_INPUT_ELEMENT_DESC&	Desc0 = pDescs0[i];
			const D3D11_INPUT_ELEMENT_DESC&	Desc1 = pDescs1[i];
			if ( Desc0.SemanticIndex != Desc1.SemanticIndex )
				return false;
			if ( Desc0.Format != Desc1.Format )
				return false;
			if ( Desc0.InputSlot != Desc1.InputSlot )
				return false;
			if ( Desc0.AlignedByteOffset != Desc1.AlignedByteOffset )
				return false;
		}
#endif
		return true;
	}
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

struct VertexFormatP3N3G3T3T3
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[5];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T3T3); }
		virtual D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 5; }
		virtual void		Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV ) const;
	} DESCRIPTOR;

public:

	NjFloat3	Position;
	NjFloat3	Normal;
	NjFloat3	Tangent;
	NjFloat3	UV;
	NjFloat3	UV2;

};
