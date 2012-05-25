#pragma once
#include "Component.h"

class	RasterizerState : public Component
{
public:	// FIELDS

	ID3D11RasterizerState*	m_pState;

public:	// METHODS

	RasterizerState( Device& _Device, D3D11_RASTERIZER_DESC& _Description );
	virtual ~RasterizerState();
};

class	DepthStencilState : public Component
{
public:	// FIELDS

	ID3D11DepthStencilState*	m_pState;

public: // METHODS

	DepthStencilState( Device& _Device, D3D11_DEPTH_STENCIL_DESC& _Description );
	virtual ~DepthStencilState();
};

class	BlendState : public Component
{
public:	// FIELDS

	ID3D11BlendState*	m_pState;

public: // METHODS

	BlendState( Device& _Device, D3D11_BLEND_DESC& _Description );
	virtual ~BlendState();
};
