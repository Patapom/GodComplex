#pragma once
#include "../Renderer.h"
#include "FormatDescriptor.h"

struct DepthStencilFormat
{
};

class IDepthStencilFormatDescriptor : public IFormatDescriptor
{
public: // PROPERTIES

	virtual DXGI_FORMAT	WritableDirectXFormat() const = 0;	// The format to create the DepthStencilView with (i.e. the GPU will write through this view)
	virtual DXGI_FORMAT	ReadableDirectXFormat() const = 0;	// The format to create the ShaderResourceView with (i.e. the shaders will read through this view)

	virtual void		Write( DepthStencilFormat& _Pixel, float _Depth, int _Stencil ) = 0;
	virtual void		Read( const DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) const = 0;
};

struct DepthStencilFormatD32F : public DepthStencilFormat
{
public:

	static class Desc : public IDepthStencilFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32_TYPELESS; }
		virtual DXGI_FORMAT	WritableDirectXFormat() const	{ return DXGI_FORMAT_D32_FLOAT; }
		virtual DXGI_FORMAT	ReadableDirectXFormat() const	{ return DXGI_FORMAT_R32_FLOAT; }

		virtual int			Size() const					{ return sizeof(DepthStencilFormatD32F); }
		virtual void		Write( DepthStencilFormat& _Pixel, float _Depth, int _Stencil )					{ DepthStencilFormatD32F& P = (DepthStencilFormatD32F&)( _Pixel ); P.Depth = _Depth; }
		virtual void		Read( const DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) const	{ const DepthStencilFormatD32F& P = (const DepthStencilFormatD32F&)( _Pixel ); _Depth = P.Depth; }
	} DESCRIPTOR;

public:

	float	Depth;

};

struct DepthStencilFormatD24S8 : public DepthStencilFormat
{
public:

	static class Desc : public IDepthStencilFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R24G8_TYPELESS; }
		virtual DXGI_FORMAT	WritableDirectXFormat() const	{ return DXGI_FORMAT_D24_UNORM_S8_UINT; }
		virtual DXGI_FORMAT	ReadableDirectXFormat() const	{ return DXGI_FORMAT_R24_UNORM_X8_TYPELESS; }

		virtual int			Size() const					{ return sizeof(DepthStencilFormatD24S8); }
		virtual void		Write( DepthStencilFormat& _Pixel, float _Depth, int _Stencil );
		virtual void		Read( const DepthStencilFormat& _Pixel, float& _Depth, int& _Stencil ) const;
	} DESCRIPTOR;

public:

	U8		Depth[3];
	U8		Stencil;

};
