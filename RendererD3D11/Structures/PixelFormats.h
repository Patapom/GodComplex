#pragma once
#include "../Renderer.h"
#include "FormatDescriptor.h"

struct PixelFormat
{
};

class IPixelFormatDescriptor : public IFormatDescriptor
{
public: // PROPERTIES

	virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const = 0;
	virtual NjFloat4	Read( const U8* _pPixel ) const = 0;
};

struct PixelFormatRGBA8 : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R8G8B8A8_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA8); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatRGBA8& P = (PixelFormatRGBA8&)( *_pPixel ); P.R = FLOAT2BYTE( _Color.x ); P.G = FLOAT2BYTE( _Color.y ); P.B = FLOAT2BYTE( _Color.z ); P.A = FLOAT2BYTE( _Color.w ); }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA8& P = (const PixelFormatRGBA8&)( *_pPixel ); return NjFloat4( NUAJBYTE2FLOAT( P.R ), NUAJBYTE2FLOAT( P.G ), NUAJBYTE2FLOAT( P.B ), NUAJBYTE2FLOAT( P.A ) ); }
	} DESCRIPTOR;

public:

	U8  R, G, B, A;

};

struct PixelFormatRGBA8_sRGB : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R8G8B8A8_UNORM_SRGB; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA8_sRGB); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatRGBA8_sRGB& P = (PixelFormatRGBA8_sRGB&)( *_pPixel ); P.R = FLOAT2BYTE( _Color.x ); P.G = FLOAT2BYTE( _Color.y ); P.B = FLOAT2BYTE( _Color.z ); P.A = FLOAT2BYTE( _Color.w ); }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA8_sRGB& P = (const PixelFormatRGBA8_sRGB&)( *_pPixel ); return NjFloat4( NUAJBYTE2FLOAT( P.R ), NUAJBYTE2FLOAT( P.G ), NUAJBYTE2FLOAT( P.B ), NUAJBYTE2FLOAT( P.A ) ); }
	} DESCRIPTOR;

public:

	U8  R, G, B, A;

};

struct PixelFormatRGBA16F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16B16A16_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA16F); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatRGBA16F& P = (PixelFormatRGBA16F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; P.B = _Color.z; P.A = _Color.w; }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA16F& P = (const PixelFormatRGBA16F&)( *_pPixel ); return NjFloat4( P.R, P.G, P.B, P.A ); }
	} DESCRIPTOR;

public:

	NjHalf  R, G, B, A;

};

struct PixelFormatRG16F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRG16F); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatRG16F& P = (PixelFormatRG16F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRG16F& P = (const PixelFormatRG16F&)( *_pPixel ); return NjFloat4( P.R, P.G, 0, 0 ); }
	} DESCRIPTOR;

public:

	NjHalf  R, G;

};

struct PixelFormatRGBA16_UINT : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16B16A16_UINT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA16_UINT); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatRGBA16_UINT& P = (PixelFormatRGBA16_UINT&)( *_pPixel ); P.R = U16( 65535.0f * _Color.x ); P.G = U16( 65535.0f * _Color.y ); P.B = U16( 65535.0f * _Color.z ); P.A = U16( 65535.0f * _Color.w ); }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA16_UINT& P = (const PixelFormatRGBA16_UINT&)( *_pPixel ); return NjFloat4( P.R / 65535.0f, P.G / 65535.0f, P.B / 65535.0f, P.A / 65535.0f ); }
	} DESCRIPTOR;

public:

	U16	R, G, B, A;

};

struct PixelFormatRGBA32F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32G32B32A32_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA32F); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatRGBA32F& P = (PixelFormatRGBA32F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; P.B = _Color.z; P.A = _Color.w; }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA32F& P = (const PixelFormatRGBA32F&)( *_pPixel ); return NjFloat4( P.R, P.G, P.B, P.A ); }
	} DESCRIPTOR;

public:

	float  R, G, B, A;

};

struct PixelFormatR32F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatR32F); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatR32F& P = (PixelFormatR32F&)( *_pPixel ); P.R = _Color.x; }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatR32F& P = (const PixelFormatR32F&)( *_pPixel ); return NjFloat4( P.R, 0, 0, 0 ); }
	} DESCRIPTOR;

public:

	float	R;

};

struct PixelFormatR16_UINT : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16_UINT; }
		virtual int			Size() const					{ return sizeof(PixelFormatR16_UINT); }
		virtual void		Write( U8* _pPixel, const NjFloat4& _Color ) const	{ PixelFormatR16_UINT& P = (PixelFormatR16_UINT&)( *_pPixel ); P.R = U16( 65535.0f * _Color.x ); }
		virtual NjFloat4	Read( const U8* _pPixel ) const						{ const PixelFormatR16_UINT& P = (const PixelFormatR16_UINT&)( *_pPixel ); return NjFloat4( P.R / 65535.0f, 0, 0, 0 ); }
	} DESCRIPTOR;

public:

	U16	R;

};
