#pragma once
#include "../Renderer.h"
#include "FormatDescriptor.h"

struct DepthStencilFormat
{
};

class DepthStencilFormatDescriptor : public IFormatDescriptor
{
public: // PROPERTIES

	virtual void		Write( DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) = 0;
	virtual void		Read( const DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) const = 0;
};

struct DepthStencilFormatD32F : public DepthStencilFormat
{
public:

	static class Desc : public DepthStencilFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_D32_FLOAT; }
		virtual int			Size() const					{ return sizeof(DepthStencilFormatD32F); }
		virtual void		Write( DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil )				{ DepthStencilFormatD32F& P = (DepthStencilFormatD32F&)( _Pixel ); P.Depth = _Depth; }
		virtual void		Read( const DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) const	{ const DepthStencilFormatD32F& P = (const DepthStencilFormatD32F&)( _Pixel ); _Depth = P.Depth; }
	} DESCRIPTOR;

public:

	float	Depth;

};
