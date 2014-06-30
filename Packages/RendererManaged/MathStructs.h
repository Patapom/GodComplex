// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace RendererManaged
{
	public value struct	float2
	{
	public:
		float	x, y;
		float2( float _x, float _y )				{ Set( _x, _y ); }
		void	Set( float _x, float _y )			{ x = _x; y = _y; }
		void	FromVector2( WMath::Vector2D^ a )	{ Set( a->x, a->y ); }
	};

	public value struct	float3
	{
	public:
		float	x, y, z;
		float3( float _x, float _y, float _z )		{ Set( _x, _y, _z ); }
		void	Set( float _x, float _y, float _z )	{ x = _x; y = _y; z = _z; }
		void	FromVector3( WMath::Vector^ a )		{ Set( a->x, a->y, a->z ); }
	};

	public value struct	float4
	{
	public:
		float	x, y, z, w;
		float4( float _x, float _y, float _z, float _w )		{ Set( _x, _y, _z, _w ); }
		void	Set( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
		void	FromVector4( WMath::Vector4D^ a )				{ Set( a->x, a->y, a->z, a->w ); }
	};

	public value struct	float4x4
	{
	public:
		float4	r0;
		float4	r1;
		float4	r2;
		float4	r3;
		float4x4( float4^ _r0, float4^ _r1, float4^ _r2, float4^ _r3 )	{ r0 = *_r0; r1 = *_r1; r2 = *_r2; r3 = *_r3; }
		void	FromMatrix4( WMath::Matrix4x4^ a )	{ r0.FromVector4( a->GetRow0() ); r1.FromVector4( a->GetRow1() ); r2.FromVector4( a->GetRow2() ); r3.FromVector4( a->GetRow3() ); }
	};
}
