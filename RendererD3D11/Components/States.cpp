#include "States.h"

RasterizerState::RasterizerState( Device& _Device, D3D11_RASTERIZER_DESC& _Description ) : Component( _Device )
{
	m_Device.DXDevice().CreateRasterizerState( &_Description, &m_pState );
	ASSERT( m_pState, "Failed state creation!" );
}
RasterizerState::~RasterizerState()
{
	m_pState->Release();
}

DepthStencilState::DepthStencilState( Device& _Device, D3D11_DEPTH_STENCIL_DESC& _Description ) : Component( _Device )
{
	m_Device.DXDevice().CreateDepthStencilState( &_Description, &m_pState );
	ASSERT( m_pState, "Failed state creation!" );
}
DepthStencilState::~DepthStencilState()
{
	m_pState->Release();
}

BlendState::BlendState( Device& _Device, D3D11_BLEND_DESC& _Description ) : Component( _Device )
{
	m_Device.DXDevice().CreateBlendState( &_Description, &m_pState );
	ASSERT( m_pState, "Failed state creation!" );
}
BlendState::~BlendState()
{
	m_pState->Release();
}
