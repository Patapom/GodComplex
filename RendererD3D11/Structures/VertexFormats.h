#pragma once
#include "../Renderer.h"

class IVertexFormatDescriptor
{
public:	 // PROPERTIES

	virtual int			Size() const = 0;
	virtual const D3D11_INPUT_ELEMENT_DESC*  GetInputElements() const = 0;
	virtual int			GetInputElementsCount() const = 0;
	virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const = 0;

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
	bool	IsSubset( const IVertexFormatDescriptor& _Super ) const
	{
#ifdef _DEBUG
		if ( Size() > _Super.Size() )
			return false;
		if ( GetInputElementsCount() > _Super.GetInputElementsCount() )
			return false;

		const D3D11_INPUT_ELEMENT_DESC*	pDescs0 = GetInputElements();
		const D3D11_INPUT_ELEMENT_DESC*	pDescs1 = _Super.GetInputElements();
		for ( int i=0; i < GetInputElementsCount(); i++ )
		{
			bool	Found = false;
			const D3D11_INPUT_ELEMENT_DESC&	Desc0 = pDescs0[i];
			for ( int j=0; j < _Super.GetInputElementsCount(); j++ )
			{
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
				Found = true;
				break;
			}
			if ( !Found )
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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float4	Pt;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	P;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	P;
	float3	N;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	P;
	float2	UV;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	Position;
	float3	Normal;
	float3	Tangent;
	float2	UV;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	Position;
	float3	Normal;
	float3	Tangent;
	float2	UV;
	float2	UV2;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	Position;
	float3	Normal;
	float3	Tangent;
	float3	UV;
	float3	UV2;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
	} DESCRIPTOR;

public:

	float3	Position;
	float3	Normal;
	float3	Tangent;
	float3	BiTangent;
	float2	UV;

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
		virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
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
	virtual void		Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const;
};
