#pragma once
#include "VertexFormats.h"

static const char*	POSITION = "POSITION";
static const char*	POSITION_TRANSFORMED = "SV_POSITION";
// static const char*	NORMAL = "NORMAL";
// static const char*	TANGENT = "TANGENT";
// static const char*	BITANGENT = "BITANGENT";
// static const char*	COLOR = "COLOR";
// static const char*	VIEW = "VIEW";
// static const char*	CURVATURE = "CURVATURE";
// static const char*	TEXCOORD = "TEXCOORD";	// In the shader, this semantic is written as TEXCOORD0, TEXCOORD1, etc.

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
