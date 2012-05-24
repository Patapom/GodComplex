#pragma once
#include "VertexFormats.h"

const char*	VertexFormatDescriptor::POSITION = "POSITION";
const char*	VertexFormatDescriptor::POSITION_TRANSFORMED = "SV_POSITION";
// const char*	VertexFormatDescriptor::NORMAL = "NORMAL";
// const char*	VertexFormatDescriptor::TANGENT = "TANGENT";
// const char*	VertexFormatDescriptor::BITANGENT = "BITANGENT";
// const char*	VertexFormatDescriptor::COLOR = "COLOR";
// const char*	VertexFormatDescriptor::VIEW = "VIEW";
// const char*	VertexFormatDescriptor::CURVATURE = "CURVATURE";
// const char*	VertexFormatDescriptor::TEXCOORD = "TEXCOORD";	// In the shader, this semantic is written as TEXCOORD0, TEXCOORD1, etc.

VertexFormatPt4::Desc		VertexFormatPt4::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatPt4::Desc::ms_pInputElements[] =
{
	{ POSITION_TRANSFORMED, 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

// VertexFormatPt4::Desc::Desc()
// {
// 	m_pInputElements[0].SemanticName = POSITION_TRANSFORMED;
// 	m_pInputElements[0].SemanticIndex = 0;
// 	m_pInputElements[0].Format = DXGI_FORMAT_R32G32B32A32_FLOAT;
// 	m_pInputElements[0].InputSlot = 0;
// 	m_pInputElements[0].AlignedByteOffset = 0;
// 	m_pInputElements[0].InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA;
// 	m_pInputElements[0].InstanceDataStepRate = 0;
// }

VertexFormatP3::Desc		VertexFormatP3::DESCRIPTOR;
D3D11_INPUT_ELEMENT_DESC	VertexFormatP3::Desc::ms_pInputElements[] =
{
	{ POSITION, 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};
// VertexFormatP3::Desc::Desc()
// {
// 	m_pInputElements[0].SemanticName = POSITION;
// 	m_pInputElements[0].SemanticIndex = 0;
// 	m_pInputElements[0].Format = DXGI_FORMAT_R32G32B32_FLOAT;
// 	m_pInputElements[0].InputSlot = 0;
// 	m_pInputElements[0].AlignedByteOffset = 0;
// 	m_pInputElements[0].InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA;
// 	m_pInputElements[0].InstanceDataStepRate = 0;
// }
