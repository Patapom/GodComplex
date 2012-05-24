#pragma once
#include "../Renderer.h"
#include "FormatDescriptor.h"

struct PixelFormat
{
};

class PixelFormatDescriptor : public IFormatDescriptor
{
public: // PROPERTIES

	virtual void		Write( PixelFormat& _Pixel, const NjFloat4& _Color ) = 0;
	virtual NjFloat4	Read( const PixelFormat& _Pixel ) const = 0;
};

struct PixelFormatRGBA8 : public PixelFormat
{
public:

	static class Desc : public PixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R8G8B8A8_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA8); }
		virtual void		Write( PixelFormat& _Pixel, const NjFloat4& _Color )	{ PixelFormatRGBA8& P = (PixelFormatRGBA8&)( _Pixel ); P.R = FLOAT2BYTE( _Color.x ); P.G = FLOAT2BYTE( _Color.y ); P.B = FLOAT2BYTE( _Color.z ); P.A = FLOAT2BYTE( _Color.w ); }
		virtual NjFloat4	Read( const PixelFormat& _Pixel ) const					{ const PixelFormatRGBA8& P = (const PixelFormatRGBA8&)( _Pixel ); return NjFloat4( BYTE2FLOAT( P.R ), BYTE2FLOAT( P.G ), BYTE2FLOAT( P.B ), BYTE2FLOAT( P.A ) ); }
	} DESCRIPTOR;

public:

	U8  R, G, B, A;

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
		virtual NjFloat4	Read( const PixelFormat& _Pixel ) const					{ const PixelFormatRGBA16F& P = (const PixelFormatRGBA16F&)( _Pixel ); return NjFloat4( P.R, P.G, P.B, P.A ); }
	} DESCRIPTOR;

public:

	NjHalf  R, G, B, A;

};
