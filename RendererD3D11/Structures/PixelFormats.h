#pragma once
#include "../Renderer.h"
#include "FormatDescriptor.h"

/*
struct PixelFormat
{
};

class IPixelFormatDescriptor : public IFormatDescriptor {
public: // PROPERTIES

	virtual int			Size() const = 0;
	virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const = 0;
	virtual bfloat4		Read( const U8* _pPixel ) const = 0;
};

struct PixelFormatR8 : public PixelFormat {
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R8_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatR8); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatR8& P = (PixelFormatR8&)( *_pPixel ); P.R = FLOAT2BYTE( _Color.x ); }
		virtual bfloat4	Read( const U8* _pPixel ) const						{ const PixelFormatR8& P = (const PixelFormatR8&)( *_pPixel ); return bfloat4( NUAJBYTE2FLOAT( P.R ), 0, 0, 1 ); }
	} DESCRIPTOR;

public:

	U8  R;

};

struct PixelFormatRGBA8 : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R8G8B8A8_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA8); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA8& P = (PixelFormatRGBA8&)( *_pPixel ); P.R = FLOAT2BYTE( _Color.x ); P.G = FLOAT2BYTE( _Color.y ); P.B = FLOAT2BYTE( _Color.z ); P.A = FLOAT2BYTE( _Color.w ); }
		virtual bfloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA8& P = (const PixelFormatRGBA8&)( *_pPixel ); return bfloat4( NUAJBYTE2FLOAT( P.R ), NUAJBYTE2FLOAT( P.G ), NUAJBYTE2FLOAT( P.B ), NUAJBYTE2FLOAT( P.A ) ); }
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
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA8_sRGB& P = (PixelFormatRGBA8_sRGB&)( *_pPixel ); P.R = FLOAT2BYTE( _Color.x ); P.G = FLOAT2BYTE( _Color.y ); P.B = FLOAT2BYTE( _Color.z ); P.A = FLOAT2BYTE( _Color.w ); }
		virtual bfloat4	Read( const U8* _pPixel ) const						{ const PixelFormatRGBA8_sRGB& P = (const PixelFormatRGBA8_sRGB&)( *_pPixel ); return bfloat4( NUAJBYTE2FLOAT( P.R ), NUAJBYTE2FLOAT( P.G ), NUAJBYTE2FLOAT( P.B ), NUAJBYTE2FLOAT( P.A ) ); }
	} DESCRIPTOR;

public:

	U8  R, G, B, A;

};

struct PixelFormatRGBA16F : public PixelFormat {
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16B16A16_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA16F); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA16F& P = (PixelFormatRGBA16F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; P.B = _Color.z; P.A = _Color.w; }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRGBA16F& P = (const PixelFormatRGBA16F&)( *_pPixel ); return bfloat4( P.R, P.G, P.B, P.A ); }
	} DESCRIPTOR;

public:

	half  R, G, B, A;

};

struct PixelFormatR16F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatR16F); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatR16F& P = (PixelFormatR16F&)( *_pPixel ); P.R = _Color.x; }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatR16F& P = (const PixelFormatR16F&)( *_pPixel ); return bfloat4( P.R, 0, 0, 0 ); }
	} DESCRIPTOR;

public:

	half  R;

};

struct PixelFormatR16_UNORM : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatR16_UNORM); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatR16_UNORM& P = (PixelFormatR16_UNORM&)( *_pPixel ); P.R = U16( 65535.0f * _Color.x ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatR16_UNORM& P = (const PixelFormatR16_UNORM&)( *_pPixel ); return bfloat4( P.R / 65535.0f, 0, 0, 0 ); }
	} DESCRIPTOR;

public:

	U16	R;

};

struct PixelFormatRG16F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRG16F); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRG16F& P = (PixelFormatRG16F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRG16F& P = (const PixelFormatRG16F&)( *_pPixel ); return bfloat4( P.R, P.G, 0, 0 ); }
	} DESCRIPTOR;

public:

	half  R, G;

};

struct PixelFormatRG16_UNORM : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatRG16_UNORM); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRG16_UNORM& P = (PixelFormatRG16_UNORM&)( *_pPixel ); P.R = U16( 65535.0f * _Color.x ); P.G = U16( 65535.0f * _Color.y ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRG16_UNORM& P = (const PixelFormatRG16_UNORM&)( *_pPixel ); return bfloat4( P.R / 65535.0f, P.G / 65535.0f, 0, 0 ); }
	} DESCRIPTOR;

public:

	U16	R, G;

};

// Warning: Truly stores INTs!! Use UNORM if you intend to store floats in [0,1]
struct PixelFormatRGBA16_UINT : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16B16A16_UINT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA16_UINT); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA16_UINT& P = (PixelFormatRGBA16_UINT&)( *_pPixel ); P.R = U16( 65535.0f * _Color.x ); P.G = U16( 65535.0f * _Color.y ); P.B = U16( 65535.0f * _Color.z ); P.A = U16( 65535.0f * _Color.w ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRGBA16_UINT& P = (const PixelFormatRGBA16_UINT&)( *_pPixel ); return bfloat4( P.R / 65535.0f, P.G / 65535.0f, P.B / 65535.0f, P.A / 65535.0f ); }
	} DESCRIPTOR;

public:

	U16	R, G, B, A;

};

struct PixelFormatRGBA16_UNORM : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R16G16B16A16_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA16_UNORM); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA16_UNORM& P = (PixelFormatRGBA16_UNORM&)( *_pPixel ); P.R = U16( 65535.0f * _Color.x ); P.G = U16( 65535.0f * _Color.y ); P.B = U16( 65535.0f * _Color.z ); P.A = U16( 65535.0f * _Color.w ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRGBA16_UNORM& P = (const PixelFormatRGBA16_UNORM&)( *_pPixel ); return bfloat4( P.R / 65535.0f, P.G / 65535.0f, P.B / 65535.0f, P.A / 65535.0f ); }
	} DESCRIPTOR;

public:

	U16	R, G, B, A;

};

struct PixelFormatR32F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatR32F); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatR32F& P = (PixelFormatR32F&)( *_pPixel ); P.R = _Color.x; }
		virtual bfloat4	Read( const U8* _pPixel ) const						{ const PixelFormatR32F& P = (const PixelFormatR32F&)( *_pPixel ); return bfloat4( P.R, 0, 0, 0 ); }
	} DESCRIPTOR;

public:

	float	R;

};

struct PixelFormatR32_UINT : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32_UINT; }
		virtual int			Size() const					{ return sizeof(PixelFormatR32_UINT); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatR32_UINT& P = (PixelFormatR32_UINT&)( *_pPixel ); P.R = U32( _Color.x ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatR32_UINT& P = (const PixelFormatR32_UINT&)( *_pPixel ); return bfloat4( float( P.R ), 0, 0, 0 ); }
	} DESCRIPTOR;

public:

	U32		R;

};

struct PixelFormatRG32F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32G32_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRG32F); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRG32F& P = (PixelFormatRG32F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRG32F& P = (const PixelFormatRG32F&)( *_pPixel ); return bfloat4( P.R, P.G, 0, 0 ); }
	} DESCRIPTOR;

public:

	float	R, G;

};

struct PixelFormatRGBA32F : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32G32B32A32_FLOAT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA32F); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA32F& P = (PixelFormatRGBA32F&)( *_pPixel ); P.R = _Color.x; P.G = _Color.y; P.B = _Color.z; P.A = _Color.w; }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRGBA32F& P = (const PixelFormatRGBA32F&)( *_pPixel ); return bfloat4( P.R, P.G, P.B, P.A ); }
	} DESCRIPTOR;

public:

	float  R, G, B, A;

};

struct PixelFormatRGBA32_UINT : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_R32G32B32A32_UINT; }
		virtual int			Size() const					{ return sizeof(PixelFormatRGBA32_UINT); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ PixelFormatRGBA32_UINT& P = (PixelFormatRGBA32_UINT&)( *_pPixel ); P.R = U32( _Color.x ); P.G = U32( _Color.y ); P.B = U32( _Color.z ); P.A = U32( _Color.w ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ const PixelFormatRGBA32_UINT& P = (const PixelFormatRGBA32_UINT&)( *_pPixel ); return bfloat4( float(P.R), float(P.G), float(P.B), float(P.A) ); }
	} DESCRIPTOR;

public:

	U32		R, G, B, A;

};

//////////////////////////////////////////////////////////////////////////
// COMPRESSED FORMATS
//////////////////////////////////////////////////////////////////////////
//
struct PixelFormatBC3_UNORM : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_BC3_UNORM; }
		virtual int			Size() const					{ return sizeof(PixelFormatBC3_UNORM); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ ASSERT( false, "Can't write individual BC3 pixels!" ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ ASSERT( false, "Can't read individual BC3 pixels!" ); return bfloat4::Zero; }
	} DESCRIPTOR;

public:

	U8		value;
};

struct PixelFormatBC3_UNORM_sRGB : public PixelFormat
{
public:

	static class Desc : public IPixelFormatDescriptor
	{
	public:

		virtual DXGI_FORMAT	DirectXFormat() const			{ return DXGI_FORMAT_BC3_UNORM_SRGB; }
		virtual int			Size() const					{ return sizeof(PixelFormatBC3_UNORM_sRGB); }
		virtual void		Write( U8* _pPixel, const bfloat4& _Color ) const	{ ASSERT( false, "Can't write individual BC3 pixels!" ); }
		virtual bfloat4		Read( const U8* _pPixel ) const						{ ASSERT( false, "Can't read individual BC3 pixels!" ); return bfloat4::Zero; }
	} DESCRIPTOR;

public:

	U8		value;
};
//*/