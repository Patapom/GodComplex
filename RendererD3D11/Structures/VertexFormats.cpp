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

VertexFormatU16::Desc	VertexFormatU16::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatU16::Desc::ms_pInputElements[] =
{
	{ INFO, 0, DXGI_FORMAT_R16_UINT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

VertexFormatU32::Desc	VertexFormatU32::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatU32::Desc::ms_pInputElements[] =
{
	{ INFO, 0, DXGI_FORMAT_R32_UINT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

void	VertexFormatPt4::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatPt4&	V = *((VertexFormatPt4*) _pVertex);
	V.Pt = NjFloat4( _Position, 1.0f );
}

void	VertexFormatP3::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3&	V = *((VertexFormatP3*) _pVertex);
	V.P = _Position;
}

void	VertexFormatP3N3::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3N3&	V = *((VertexFormatP3N3*) _pVertex);
	V.P = _Position;
	V.N = _Normal;
}

void	VertexFormatP3T2::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3T2&	V = *((VertexFormatP3T2*) _pVertex);
	V.P = _Position;
	V.UV = _UV;
}

void	VertexFormatP3N3G3T2::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3N3G3T2&	V = *((VertexFormatP3N3G3T2*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.UV = _UV;
}

void	VertexFormatP3N3G3T2T2::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3N3G3T2T2&	V = *((VertexFormatP3N3G3T2T2*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.UV = _UV;
	V.UV2 = _UV;
}

void	VertexFormatP3N3G3T3T3::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3N3G3T3T3&	V = *((VertexFormatP3N3G3T3T3*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.UV.Set( _UV.x, _UV.y, 0.0f );
	V.UV2.Set( _UV.x, _UV.y, 0.0f );
}

void	VertexFormatP3N3G3B3T2::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatP3N3G3B3T2&	V = *((VertexFormatP3N3G3B3T2*) _pVertex);
	V.Position = _Position;
	V.Normal = _Normal;
	V.Tangent = _Tangent;
	V.BiTangent = _BiTangent;
	V.UV = _UV;
}

void	VertexFormatU16::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatU16&	V = *((VertexFormatU16*) _pVertex);
	V.Value = U16( _Position.x );
}

void	VertexFormatU32::Desc::Write( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV ) const
{
	VertexFormatU16&	V = *((VertexFormatU16*) _pVertex);
	V.Value = U32( _Position.x );
}
