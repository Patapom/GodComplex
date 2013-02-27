#pragma once
#include "DepthStencilFormats.h"

DepthStencilFormatD32F::Desc	DepthStencilFormatD32F::DESCRIPTOR;
DepthStencilFormatD24S8::Desc	DepthStencilFormatD24S8::DESCRIPTOR;

void	DepthStencilFormatD24S8::Desc::Write( DepthStencilFormat& _Pixel, float _Depth, int _Stencil )
{
	DepthStencilFormatD24S8& P = (DepthStencilFormatD24S8&)( _Pixel );
	
	U32	iDepth = U32( 16777215 * _Depth );
	P.Depth[0] = iDepth & 0xFF; iDepth >>= 8;
	P.Depth[1] = iDepth & 0xFF; iDepth >>= 8;
	P.Depth[2] = iDepth & 0xFF;
	P.Stencil = _Stencil & 0xFF;
}

void	DepthStencilFormatD24S8::Desc::Read( const DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) const
{
	const DepthStencilFormatD24S8& P = (const DepthStencilFormatD24S8&)( _Pixel );

	U32	iDepth = P.Depth[0] | (P.Depth[1] << 8) | (P.Depth[2] << 16);
	_Depth = iDepth / 16777215.0f;
	_Stencil = P.Stencil;
}
