#pragma once
#include "../Renderer.h"

struct PixelFormat
{
};

class PixelFormatDescriptor
{
public: // PROPERTIES

	virtual DXGI_FORMAT	DirectXFormat() const = 0;
	virtual int			Size() const = 0;
	virtual void		Write( PixelFormat& _Pixel, const NjFloat4& _Color ) = 0;
	virtual NjFloat4	Read( PixelFormat& _Pixel ) const = 0;
};

struct PixelFormatRGBA16F : public PixelFormat
{
public:

	static class Desc : public PixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16B16A16_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA16F); }
		virtual void		Write( PixelFormat& _Pixel, const NjFloat4& _Color )	{ PixelFormatRGBA16F& P = (PixelFormatRGBA16F&)( _Pixel ); P.R = _Color.x; P.G = _Color.y; P.B = _Color.z; P.A = _Color.w; }
		virtual NjFloat4	Read( PixelFormat& _Pixel ) const						{ PixelFormatRGBA16F& P = (PixelFormatRGBA16F&)( _Pixel ); return NjFloat4( P.R, P.G, P.B, P.A ); }
	} DESCRIPTOR;

public:

	NjHalf  R, G, B, A;

};
