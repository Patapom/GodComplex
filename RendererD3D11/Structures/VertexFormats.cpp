#pragma once
#include "VertexFormats.h"

static const char*	POSITION = "POSITION";
static const char*	POSITION_TRANSFORMED = "SV_POSITION";
static const char*	NORMAL = "NORMAL";
static const char*	TANGENT = "TANGENT";
static const char*	BITANGENT = "BITANGENT";
// static const char*	COLOR = "COLOR";
// static const char*	VIEW = "VIEW";
// static const char*	CURVATURE = "CURVATURE";
static const char*	TEXCOORD = "TEXCOORD";	// In the shader, this semantic is written as TEXCOORD0, TEXCOORD1, etc.
static const char*	INFO = "INFO";


VertexFormatPt4::Desc		VertexFormatPt4::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatPt4::Desc::ms_pInputElements[] =
{
	{ POSITION_TRANSFORMED, 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3::Desc		VertexFormatP3::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3N3::Desc		VertexFormatP3N3::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3N3::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ NORMAL, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3T2::Desc		VertexFormatP3T2::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3T2::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 0, DXGI_FORMAT_R32G32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3N3G3T2::Desc	VertexFormatP3N3G3T2::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3N3G3T2::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ NORMAL, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TANGENT, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 0, DXGI_FORMAT_R32G32_FLOAT, 0, 36, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3N3G3T2T2::Desc	VertexFormatP3N3G3T2T2::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3N3G3T2T2::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ NORMAL, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TANGENT, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 0, DXGI_FORMAT_R32G32_FLOAT, 0, 36, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 1, DXGI_FORMAT_R32G32_FLOAT, 0, 44, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3N3G3T3T3::Desc	VertexFormatP3N3G3T3T3::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3N3G3T3T3::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ NORMAL, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TANGENT, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 36, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 1, DXGI_FORMAT_R32G32B32_FLOAT, 0, 48, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatP3N3G3B3T2::Desc	VertexFormatP3N3G3B3T2::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3N3G3B3T2::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ NORMAL, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TANGENT, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 24, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ BITANGENT, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 36, D3D11_INPUT_PER_VERTEX_DATA, 0 },
	{ TEXCOORD, 0, DXGI_FORMAT_R32G32_FLOAT, 0, 48, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatU32::Desc	VertexFormatU32::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatU32::Desc::ms_pInputElements[] =
{
	{ INFO, 0, DXGI_FORMAT_R32_UINT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

void	VertexFormatPt4::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatPt4&	V = *((VertexFormatPt4*) _pVertex);
	V.Pt = float4( _Position, 1.0f );
}

void	VertexFormatP3::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3&	V = *((VertexFormatP3*) _pVertex);
	V.P = _Position;
}

void	VertexFormatP3N3::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3N3&	V = *((VertexFormatP3N3*) _pVertex);
	V.P = _Position;
	V.N = _Normal;
}

void	VertexFormatP3T2::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3T2&	V = *((VertexFormatP3T2*) _pVertex);
	V.P = _Position;
	V.UV = _UV;
}

void	VertexFormatP3N3G3T2::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3N3G3T2&	V = *((VertexFormatP3N3G3T2*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.UV = _UV;
}

void	VertexFormatP3N3G3T2T2::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3N3G3T2T2&	V = *((VertexFormatP3N3G3T2T2*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.UV = _UV;
	V.UV2 = _UV;
}

void	VertexFormatP3N3G3T3T3::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3N3G3T3T3&	V = *((VertexFormatP3N3G3T3T3*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.UV.Set( _UV.x, _UV.y, 0.0f );
	V.UV2.Set( _UV.x, _UV.y, 0.0f );
}

void	VertexFormatP3N3G3B3T2::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatP3N3G3B3T2&	V = *((VertexFormatP3N3G3B3T2*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.BiTangent = _BiTangent;
	V.UV = _UV;
}

void	VertexFormatU32::Desc::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	VertexFormatU32&	V = *((VertexFormatU32*) _pVertex);
	V.Value = U32( _Position.x );
}


//////////////////////////////////////////////////////////////////////////
// CompositeVertexFormatDescriptor
//
CompositeVertexFormatDescriptor::CompositeVertexFormatDescriptor()
	: m_ElementsCount( 0 )
	, m_Size( 0 )
	, m_AggregatedVertexFormatsCount( 0 )
{
	memset( m_pInputElements, 0, MAX_INPUT_ELEMENTS*sizeof(D3D11_INPUT_ELEMENT_DESC) );
	memset( m_ppAggregatedVertexFormats, 0, MAX_INPUT_ELEMENTS*sizeof(IVertexFormatDescriptor*) );
}

void	CompositeVertexFormatDescriptor::AggregateVertexFormat( const IVertexFormatDescriptor& _VertexFormat )
{
	// Compute byte offset from existing vertex formats
	U32	AlignedByteOffset = 0;
	for ( int ExistingVertexFormatIndex=0; ExistingVertexFormatIndex < m_AggregatedVertexFormatsCount; ExistingVertexFormatIndex++ )
		AlignedByteOffset += m_ppAggregatedVertexFormats[ExistingVertexFormatIndex]->Size();

	// Aggregate this vertex format's input elements
	const D3D11_INPUT_ELEMENT_DESC*	pElements = (const D3D11_INPUT_ELEMENT_DESC*) _VertexFormat.GetInputElements();
	for ( int ElementsCount=0; ElementsCount < _VertexFormat.GetInputElementsCount(); ElementsCount++, m_ElementsCount++ )
	{
		memcpy_s( &m_pInputElements[m_ElementsCount], sizeof(D3D11_INPUT_ELEMENT_DESC), &pElements[ElementsCount], sizeof(D3D11_INPUT_ELEMENT_DESC) );

		// Actually, DON'T! Additional vertex streams must start at their respective offset...
//		m_pInputElements[m_ElementsCount].AlignedByteOffset += AlignedByteOffset;	// Patch offset
	}

	m_Size += _VertexFormat.Size();

	// Store vertex format source
	m_ppAggregatedVertexFormats[m_AggregatedVertexFormatsCount++] = &_VertexFormat;
}

void	CompositeVertexFormatDescriptor::Write( void* _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV ) const
{
	int	Offset = 0;
	for ( int VertexFormatIndex=0; VertexFormatIndex < m_AggregatedVertexFormatsCount; VertexFormatIndex++ )
	{
		m_ppAggregatedVertexFormats[VertexFormatIndex]->Write( (void*) ((U8*) _pVertex + Offset), _Position, _Normal, _Tangent, _BiTangent, _UV );
		Offset += m_ppAggregatedVertexFormats[VertexFormatIndex]->Size();
	}
}
