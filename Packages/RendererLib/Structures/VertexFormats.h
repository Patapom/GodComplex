#pragma once
#include "../Renderer.h"

class IVertexFormatDescriptor {
public:	 // PROPERTIES

	virtual int			Size() const = 0;
	virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const = 0;
	virtual int			GetInputElementsCount() const = 0;
	virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const = 0;

	// Exact format comparison
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

	// Checks if this format is a compatible subset to the other provided format
	bool	IsSubset( const IVertexFormatDescriptor& _super ) const {
#ifdef _DEBUG
		if ( &_super == this )
			return true;	// Obvious case!
		if ( Size() > _super.Size() )
			return false;
		if ( GetInputElementsCount() > _super.GetInputElementsCount() )
			return false;

		const D3D11_INPUT_ELEMENT_DESC*	pDescs0 = GetInputElements();
		const D3D11_INPUT_ELEMENT_DESC*	pDescs1 = _super.GetInputElements();
		for ( int i=0; i < GetInputElementsCount(); i++ ) {
			bool	found = false;
			const D3D11_INPUT_ELEMENT_DESC&	Desc0 = pDescs0[i];
			for ( int j=0; j < _super.GetInputElementsCount(); j++ ) {
				const D3D11_INPUT_ELEMENT_DESC&	Desc1 = pDescs1[j];
				if ( strcmp( Desc0.SemanticName, Desc1.SemanticName ) )
					continue;
				if ( Desc0.SemanticIndex != Desc1.SemanticIndex )
					continue;
				if ( Desc0.Format != Desc1.Format )
					continue;
				if ( Desc0.InputSlot != Desc1.InputSlot )
					continue;
				if ( Desc0.AlignedByteOffset != Desc1.AlignedByteOffset )
					continue;

				// Found the exact match in super's format
				found = true;
				break;
			}
			if ( !found )
				return false;	// That input was not found
		}
#endif
		return true;
	}
};

// SV_POSITION
struct VertexFormatPt4
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[1];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatPt4); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 1; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat4	Pt;

};

// Position
struct VertexFormatP3
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[1];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 1; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	P;

};

// Position
// Normal
struct VertexFormatP3N3
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[2];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 2; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	P;
	bfloat3	N;

};

// Position
// UV
struct VertexFormatP3T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[2];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3T2); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 2; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	P;
	bfloat2	UV;

};

// UV
struct VertexFormatT2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[1];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatT2); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 1; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat2	UV;

};

// Position
// Normal
// UV
struct VertexFormatP3N3T2
{
public:

	static class Desc : public IVertexFormatDescriptor {
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[3];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3T2); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 3; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	P;
	bfloat3	N;
	bfloat2	UV;

};

// Position
// Normal
// Tangent
// UV
struct VertexFormatP3N3G3T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[4];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T2); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 4; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	Position;
	bfloat3	Normal;
	bfloat3	Tangent;
	bfloat2	UV;

};

// Position
// Normal
// Tangent
// UV0
// UV1
struct VertexFormatP3N3G3T2T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[5];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T2T2); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 5; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	Position;
	bfloat3	Normal;
	bfloat3	Tangent;
	bfloat2	UV;
	bfloat2	UV2;

};

// Position
// Normal
// Tangent
// UV0
// UV1
struct VertexFormatP3N3G3T3T3
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[5];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3T3T3); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 5; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	Position;
	bfloat3	Normal;
	bfloat3	Tangent;
	bfloat3	UV;
	bfloat3	UV2;

};

// Position
// Normal
// Tangent
// BiTangent
// UV
struct VertexFormatP3N3G3B3T2
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[5];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3B3T2); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 5; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	Position;
	bfloat3	Normal;
	bfloat3	Tangent;
	bfloat3	BiTangent;
	bfloat2	UV;

};

// Position
// Normal
// Tangent
// BiTangent
// UV0
// Color0
// Color1
struct VertexFormatP3N3G3B3T3C4C4
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[7];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatP3N3G3B3T3C4C4); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 7; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	bfloat3	Position;
	bfloat3	Normal;
	bfloat3	Tangent;
	bfloat3	BiTangent;
	bfloat3	UV0;
	U32		Color0;
	U32		Color1;

};

// Simple U32
struct VertexFormatU32
{
public:

	static class Desc : public IVertexFormatDescriptor
	{
		static D3D11_INPUT_ELEMENT_DESC	ms_pInputElements[1];

	public:

		virtual int			Size() const							{ return sizeof(VertexFormatU32); }
		virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return ms_pInputElements; }
		virtual int			GetInputElementsCount() const			{ return 1; }
		virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
	} DESCRIPTOR;

public:

	U32		Value;

};

//////////////////////////////////////////////////////////////////////////
// Composite descriptor
// This is a helper to create a composite vertex format by aggregating existing types
//
class CompositeVertexFormatDescriptor : public IVertexFormatDescriptor
{
	static const int	MAX_INPUT_ELEMENTS = 16;

	int							m_ElementsCount;
	D3D11_INPUT_ELEMENT_DESC	m_pInputElements[MAX_INPUT_ELEMENTS];
	int							m_Size;

	int								m_AggregatedVertexFormatsCount;
	const IVertexFormatDescriptor*	m_ppAggregatedVertexFormats[MAX_INPUT_ELEMENTS];

public:

	CompositeVertexFormatDescriptor();

	void				AggregateVertexFormat( const IVertexFormatDescriptor& _VertexFormat );

	virtual int			Size() const							{ return m_Size; }
	virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const	{ return m_pInputElements; }
	virtual int			GetInputElementsCount() const			{ return m_ElementsCount; }
	virtual void		Write( void* _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) const;
};
