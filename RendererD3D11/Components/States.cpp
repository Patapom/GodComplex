#include "States.h"

RasterizerState::RasterizerState( Device& _Device, D3D11_RASTERIZER_DESC& _Description ) : Component( _Device )
{
	m_Device.DXDevice().CreateRasterizerState( &_Description, &m_pState );
}
RasterizerState::~RasterizerState()
{
	m_pState->Release(); delete m_pState; m_pState = NULL;
}

DepthStencilState::DepthStencilState( Device& _Device, D3D11_DEPTH_STENCIL_DESC& _Description ) : Component( _Device )
{
	m_Device.DXDevice().CreateDepthStencilState( &_Description, &m_pState );
}
DepthStencilState::~DepthStencilState()
{
	m_pState->Release(); delete m_pState; m_pState = NULL;
}

BlendState::BlendState( Device& _Device, D3D11_BLEND_DESC& _Description ) : Component( _Device )
{
	m_Device.DXDevice().CreateBlendState( &_Description, &m_pState );
}
BlendState::~BlendState()
{
	m_pState->Release(); delete m_pState; m_pState = NULL;
}
