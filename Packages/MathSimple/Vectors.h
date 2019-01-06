//////////////////////////////////////////////////////////////////////////
// Super simple managed version of the BaseLib's math structures
//////////////////////////////////////////////////////////////////////////
//
#pragma once

using namespace System;

#include "Math.h"

namespace SharpMath {

	value struct	float3;

	[System::Diagnostics::DebuggerDisplayAttribute( "{x}, {y}" )]
	public value struct	float2 {
	public:
		float	x, y;
		float2( float _x, float _y )				{ Set( _x, _y ); }
		void	Set( float _x, float _y )			{ x = _x; y = _y; }

		String^	ToString() override {
			return "{ " + x + ", " + y + " }";
		}

		static bool		TryParse( String^ _stringValue, float2% _value );

		static float2	operator+( float2 a, float2 b )	{ return float2( a.x+b.x, a.y+b.y ); }
		static float2	operator-( float2 a, float2 b )	{ return float2( a.x-b.x, a.y-b.y ); }
		static float2	operator-( float2 a )			{ return float2( -a.x, -a.y ); }
		static float2	operator*( float a, float2 b )	{ return float2( a*b.x, a*b.y ); }
		static float2	operator*( float2 a, float b )	{ return float2( a.x*b, a.y*b ); }
		static float2	operator*( float2 a, float2 b )	{ return float2( a.x*b.x, a.y*b.y ); }
		static float2	operator/( float2 a, float b )	{ return float2( a.x/b, a.y/b ); }
		static float2	operator/( float2 a, float2 b )	{ return float2( a.x/b.x, a.y/b.y ); }

		property float	Length {
			float	get() { return (float) Math::Sqrt( x*x + y*y ); }
		}

		property float	LengthSquared {
			float	get() { return x*x + y*y; }
		}

		property float2	Normalized	{
			float2	get() {
				float	InvLength = 1.0f / Length;
				return float2( InvLength * x, InvLength * y );
			}
		}

		property float	default[int] {
			float	get( int _ComponentIndex ) {
				switch ( _ComponentIndex&1 ) {
					case 0: return x;
					case 1: return y;
				}
				return x;
			}
			void	set( int _ComponentIndex, float value ) {
				switch ( _ComponentIndex&1 ) {
					case 0: x = value; break;
					case 1: y = value; break;
				}
			}
		}

		float	Min()			{ return Math::Min( x, y ); }
		float	Max()			{ return Math::Max( x, y ); }
		void	Min( float2 p )	{ x = Math::Min( x, p.x ); y = Math::Min( y, p.y ); }
		void	Max( float2 p )	{ x = Math::Max( x, p.x ); y = Math::Max( y, p.y ); }
		float2	Clamp( float _min, float _max )	{ return float2( Mathf::Clamp( x, _min, _max ), Mathf::Clamp( y, _min, _max ) ); }
		float2	Saturate()						{ return float2( Mathf::Clamp( x, 0.0f, 1.0f ), Mathf::Clamp( y, 0.0f, 1.0f ) ); }

		float	Dot( float2 b )		{ return x*b.x + y*b.y; }
		void	Normalize()			{ float recLength = 1.0f / Length; x *= recLength; y *= recLength; }

		float3	Cross( float2 b );
		float	CrossZ( float2 b )	{ return x * b.y - y * b.x; }	// Returns the Z component of the orthogonal vector

		static bool			operator==( float2^ _Op0, float2^ _Op1 ) {
			if ( ((Object^) _Op0) == nullptr && ((Object^) _Op1) == nullptr )
				return true;
			if ( ((Object^) _Op0) == nullptr || ((Object^) _Op1) == nullptr )
				return false;
			return Math::Abs( _Op0->x - _Op1->x ) < float::Epsilon && Math::Abs( _Op0->y - _Op1->y ) < float::Epsilon;
		}
		static bool			operator!=( float2^ _Op0, float2^ _Op1 ) {
			if ( ((Object^) _Op0) == nullptr && ((Object^) _Op1) == nullptr )
				return false;
			if ( ((Object^) _Op0) == nullptr || ((Object^) _Op1) == nullptr )
				return true;
			return Math::Abs( _Op0->x - _Op1->x ) > float::Epsilon || Math::Abs( _Op0->y - _Op1->y ) > float::Epsilon;
		}

		bool	Almost( float2 b )					{ return Almost( b, Mathf::ALMOST_EPSILON ); }
		bool	Almost( float2 b, float _epsilon )	{ return Mathf::Almost( x, b.x, _epsilon ) && Mathf::Almost( y, b.y, _epsilon ); }

		static property float2	Zero	{ float2 get() { return float2( 0, 0 ); } }
		static property float2	UnitX	{ float2 get() { return float2( 1, 0 ); } }
		static property float2	UnitY	{ float2 get() { return float2( 0, 1 ); } }
		static property float2	One		{ float2 get() { return float2( 1, 1 ); } }
	};

	[System::Diagnostics::DebuggerDisplayAttribute( "{x}, {y}, {z}" )]
	public value struct	float3 {
	public:
		float	x, y, z;
		float3( float _x, float _y, float _z )		{ Set( _x, _y, _z ); }
		float3( float2 _xy, float _z )				{ Set( _xy.x, _xy.y, _z ); }
//		explicit float3( System::Drawing::Color^ _Color )	{ Set( _Color->R / 255.0f, _Color->G / 255.0f, _Color->B / 255.0f ); }
		void	Set( float _x, float _y, float _z )	{ x = _x; y = _y; z = _z; }

		String^	ToString() override {
			return "{ " + x + ", " + y + ", " + z + " }";
		}

		static bool		TryParse( String^ _stringValue, float3% _value );

		static float3	operator+( float3 a, float3 b )	{ return float3( a.x+b.x, a.y+b.y, a.z+b.z ); }
		static float3	operator-( float3 a, float3 b )	{ return float3( a.x-b.x, a.y-b.y, a.z-b.z ); }
		static float3	operator-( float3 a )			{ return float3( -a.x, -a.y, -a.z ); }
		static float3	operator*( float a, float3 b )	{ return float3( a*b.x, a*b.y, a*b.z ); }
		static float3	operator*( float3 a, float b )	{ return float3( a.x*b, a.y*b, a.z*b ); }
		static float3	operator*( float3 a, float3 b )	{ return float3( a.x*b.x, a.y*b.y, a.z*b.z ); }
		static float3	operator/( float3 a, float b )	{ return float3( a.x/b, a.y/b, a.z/b ); }
		static float3	operator/( float3 a, float3 b )	{ return float3( a.x/b.x, a.y/b.y, a.z/b.z ); }

		static explicit operator float2( float3 a )		{ return float2( a.x, a.y ); }

		property float	Length {
			float	get() { return (float) Math::Sqrt( x*x + y*y + z*z ); }
		}

		property float	LengthSquared {
			float	get() { return x*x + y*y + z*z; }
		}

		property float3	Normalized	{
			float3	get() {
				float	invLength = 1.0f / Length;
				return float3( invLength * x, invLength * y, invLength * z );
			}
		}

		property float3	NormalizedSafe	{
			float3	get() {
				float	L2 = LengthSquared;
				float	invLength = L2 > 1e-8 ? 1.0f / Mathf::Sqrt( L2 ) : 0.0f;
				return float3( invLength * x, invLength * y, invLength * z );
			}
		}

		property float2	xy	{
			float2	get() { return float2( x, y ); }
		}

		property float	default[int] {
			float	get( int _ComponentIndex ) {
				switch ( _ComponentIndex%3 ) {
					case 0: return x;
					case 1: return y;
					case 2: return z;
				}
				return x;
			}
			void	set( int _ComponentIndex, float value ) {
				switch ( _ComponentIndex%3 ) {
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
				}
			}
		}

		float	Min()			{ return Math::Min( Math::Min( x, y ), z ); }
		float	Max()			{ return Math::Max( Math::Max( x, y ), z ); }
		void	Min( float3 p )	{ x = Math::Min( x, p.x ); y = Math::Min( y, p.y ); z = Math::Min( z, p.z ); }
		void	Max( float3 p )	{ x = Math::Max( x, p.x ); y = Math::Max( y, p.y ); z = Math::Max( z, p.z ); }
		float3	Clamp( float _min, float _max )	{ return float3( Mathf::Clamp( x, _min, _max ), Mathf::Clamp( y, _min, _max ), Mathf::Clamp( z, _min, _max ) ); }
		float3	Saturate()						{ return float3( Mathf::Clamp( x, 0.0f, 1.0f ), Mathf::Clamp( y, 0.0f, 1.0f ), Mathf::Clamp( z, 0.0f, 1.0f ) ); }

		float	Dot( float3 b )	{ return x*b.x + y*b.y + z*b.z; }
		void	Normalize()		{ float recLength = 1.0f / Length; x *= recLength; y *= recLength; z *= recLength; }
		void	NormalizeSafe()	{
			float	L2 = LengthSquared;
			float	recLength = L2 > 1e-8 ? 1.0f / Mathf::Sqrt( L2 ) : 0.0f;
			x *= recLength; y *= recLength; z *= recLength;
		}

		float3	Cross( float3 b ) {
			return float3(	y * b.z - z * b.y,
							z * b.x - x * b.z,
							x * b.y - y * b.x );
		}

		// bmayaux (2016-01-04) Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
		// Builds up the 2 remaining vectors to form an orthonormal basis, assuming this vector is the "right" direction
		// Expected "this" to be normalized!
		// This code doesn't involve any square root!
		void	OrthogonalBasis( [System::Runtime::InteropServices::Out] float3% _left, [System::Runtime::InteropServices::Out] float3% _up ) {
			if ( z < -0.9999999f ) {
				// Handle the singularity
				_left.Set( 0.0f, -1.0f, 0.0f );
				_up.Set( -1.0f, 0.0f, 0.0f );
				return;
			}

			const float	a = 1.0f / (1.0f + z);
			const float	b = -x*y*a;
			_left.Set( 1.0f - x*x*a, b, -x );
			_up.Set( b, 1.0f - y*y*a, -y );
		}

		static bool			operator==( float3^ _Op0, float3^ _Op1 ) {
			if ( ((Object^) _Op0) == nullptr && ((Object^) _Op1) == nullptr )
				return true;
			if ( ((Object^) _Op0) == nullptr || ((Object^) _Op1) == nullptr )
				return false;
			return Math::Abs( _Op0->x - _Op1->x ) < float::Epsilon && Math::Abs( _Op0->y - _Op1->y ) < float::Epsilon && Math::Abs( _Op0->z - _Op1->z ) < float::Epsilon;
		}
		static bool			operator!=( float3^ _Op0, float3^ _Op1 ) {
			if ( ((Object^) _Op0) == nullptr && ((Object^) _Op1) == nullptr )
				return false;
			if ( ((Object^) _Op0) == nullptr || ((Object^) _Op1) == nullptr )
				return true;
			return Math::Abs( _Op0->x - _Op1->x ) > float::Epsilon || Math::Abs( _Op0->y - _Op1->y ) > float::Epsilon || Math::Abs( _Op0->z - _Op1->z ) > float::Epsilon;
		}

		bool	Almost( float3 b )					{ return Almost( b, Mathf::ALMOST_EPSILON ); }
		bool	Almost( float3 b, float _epsilon )	{ return Mathf::Almost( x, b.x, _epsilon ) && Mathf::Almost( y, b.y, _epsilon ) && Mathf::Almost( z, b.z, _epsilon ); }

		static property float3	Zero	{ float3 get() { return float3( 0, 0, 0 ); } }
		static property float3	UnitX	{ float3 get() { return float3( 1, 0, 0 ); } }
		static property float3	UnitY	{ float3 get() { return float3( 0, 1, 0 ); } }
		static property float3	UnitZ	{ float3 get() { return float3( 0, 0, 1 ); } }
		static property float3	One		{ float3 get() { return float3( 1, 1, 1 ); } }
	};

	[System::Diagnostics::DebuggerDisplayAttribute( "{x}, {y}, {z}, {w}" )]
	public value struct	float4 {
	public:
		float	x, y, z, w;

		float4( float _x, float _y, float _z, float _w )		{ Set( _x, _y, _z, _w ); }
		float4( float2 _xy, float _z, float _w )				{ Set( _xy.x, _xy.y, _z, _w ); }
		float4( float2 _xy, float2 _zw )						{ Set( _xy.x, _xy.y, _zw.x, _zw.y ); }
		float4( float3 _xyz, float _w )							{ Set( _xyz.x, _xyz.y, _xyz.z, _w ); }
//		explicit float4( System::Drawing::Color^ _Color, float _Alpha )	{ Set( _Color->R / 255.0f, _Color->G / 255.0f, _Color->B / 255.0f, _Alpha ); }
		void	Set( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
		void	Set( float3 _xyz, float _w )					{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = _w; }

		String^	ToString() override {
			return "{ " + x + ", " + y + ", " + z + ", " + w + " }";
		}

		static bool		TryParse( String^ _stringValue, float4% _value );

		static float4	operator+( float4 a, float4 b )	{ return float4( a.x+b.x, a.y+b.y, a.z+b.z, a.w+b.w ); }
		static float4	operator-( float4 a, float4 b )	{ return float4( a.x-b.x, a.y-b.y, a.z-b.z, a.w-b.w ); }
		static float4	operator-( float4 a )			{ return float4( -a.x, -a.y, -a.z, -a.w ); }
		static float4	operator*( float a, float4 b )	{ return float4( a*b.x, a*b.y, a*b.z, a*b.w ); }
		static float4	operator*( float4 a, float b )	{ return float4( a.x*b, a.y*b, a.z*b, a.w*b ); }
		static float4	operator*( float4 a, float4 b )	{ return float4( a.x*b.x, a.y*b.y, a.z*b.z, a.w*b.w ); }
		static float4	operator/( float4 a, float b )	{ return float4( a.x/b, a.y/b, a.z/b, a.w/b ); }
		static float4	operator/( float4 a, float4 b )	{ return float4( a.x/b.x, a.y/b.y, a.z/b.z, a.w/b.w ); }

		static explicit operator float2( float4 a )		{ return float2( a.x, a.y ); }
		static explicit operator float3( float4 a )		{ return float3( a.x, a.y, a.z ); }

		property float	Length {
			float	get() { return (float) Math::Sqrt( x*x + y*y + z*z + w*w ); }
		}

		property float	LengthSquared {
			float	get() { return x*x + y*y + z*z + w*w; }
		}

		property float4	Normalized {
			float4	get() {
				float	InvLength = 1.0f / Length;
				return float4( InvLength * x, InvLength * y, InvLength * z, InvLength * w );
			}
		}

		property float2	xy	{
			float2	get() { return float2( x, y ); }
		}
		property float2	zw	{
			float2	get() { return float2( z, w ); }
		}
		property float3	xyz	{
			float3	get() { return float3( x, y, z ); }
		}

		property float	default[int] {
			float	get( int _ComponentIndex ) {
				switch ( _ComponentIndex&3 ) {
					case 0: return x;
					case 1: return y;
					case 2: return z;
					case 3: return w;
				}
				return x;
			}
			void	set( int _ComponentIndex, float value ) {
				switch ( _ComponentIndex&3 ) {
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					case 3: w = value; break;
				}
			}
		}

		float	Min()			{ return Math::Min( Math::Min( Math::Min( x, y ), z ), w ); }
		float	Max()			{ return Math::Max( Math::Max( Math::Max( x, y ), z ), w ); }
		void	Min( float4 p )	{ x = Math::Min( x, p.x ); y = Math::Min( y, p.y ); z = Math::Min( z, p.z ); w = Math::Min( w, p.w ); }
		void	Max( float4 p )	{ x = Math::Max( x, p.x ); y = Math::Max( y, p.y ); z = Math::Max( z, p.z ); w = Math::Max( w, p.w ); }
		float4	Clamp( float _min, float _max )	{ return float4( Mathf::Clamp( x, _min, _max ), Mathf::Clamp( y, _min, _max ), Mathf::Clamp( z, _min, _max ), Mathf::Clamp( w, _min, _max ) ); }
		float4	Saturate()						{ return float4( Mathf::Clamp( x, 0.0f, 1.0f ), Mathf::Clamp( y, 0.0f, 1.0f ), Mathf::Clamp( z, 0.0f, 1.0f ), Mathf::Clamp( w, 0.0f, 1.0f ) ); }

		float	Dot( float4 b )	{ return x*b.x + y*b.y + z*b.z + w*b.w; }
		void	Normalize()		{ float recLength = 1.0f / Length; x *= recLength; y *= recLength; z *= recLength; w*= recLength; }

		static bool			operator==( float4^ _Op0, float4^ _Op1 ) {
			if ( ((Object^) _Op0) == nullptr && ((Object^) _Op1) == nullptr )
				return true;
			if ( ((Object^) _Op0) == nullptr || ((Object^) _Op1) == nullptr )
				return false;
			return Math::Abs( _Op0->x - _Op1->x ) < float::Epsilon && Math::Abs( _Op0->y - _Op1->y ) < float::Epsilon && Math::Abs( _Op0->z - _Op1->z ) < float::Epsilon && Math::Abs( _Op0->w - _Op1->w ) < float::Epsilon;
		}
		static bool			operator!=( float4^ _Op0, float4^ _Op1 ) {
			if ( ((Object^) _Op0) == nullptr && ((Object^) _Op1) == nullptr )
				return false;
			if ( ((Object^) _Op0) == nullptr || ((Object^) _Op1) == nullptr )
				return true;
			return Math::Abs( _Op0->x - _Op1->x ) > float::Epsilon || Math::Abs( _Op0->y - _Op1->y ) > float::Epsilon || Math::Abs( _Op0->z - _Op1->z ) > float::Epsilon || Math::Abs( _Op0->w - _Op1->w ) > float::Epsilon;
		}

		bool	Almost( float4 b )					{ return Almost( b, Mathf::ALMOST_EPSILON ); }
		bool	Almost( float4 b, float _epsilon )	{ return Mathf::Almost( x, b.x, _epsilon ) && Mathf::Almost( y, b.y, _epsilon ) && Mathf::Almost( z, b.z, _epsilon ) && Mathf::Almost( w, b.w, _epsilon ); }

		static property float4	Zero	{ float4 get() { return float4( 0, 0, 0, 0 ); } }
		static property float4	UnitX	{ float4 get() { return float4( 1, 0, 0, 0 ); } }
		static property float4	UnitY	{ float4 get() { return float4( 0, 1, 0, 0 ); } }
		static property float4	UnitZ	{ float4 get() { return float4( 0, 0, 1, 0 ); } }
		static property float4	UnitW	{ float4 get() { return float4( 0, 0, 0, 1 ); } }
		static property float4	One		{ float4 get() { return float4( 1, 1, 1, 1 ); } }
	};

	//////////////////////////////////////////////////////////////////////////
	[System::Diagnostics::DebuggerDisplayAttribute( "{r0}, {r1}, {r2}" )]
	public value struct	float3x3 {
	public:
		float3	r0;
		float3	r1;
		float3	r2;

		float3x3( cli::array<float>^ _values ) {
			r0.Set( _values[3*0+0], _values[3*0+1], _values[3*0+2] );
			r1.Set( _values[3*1+0], _values[3*1+1], _values[3*1+2] );
			r2.Set( _values[3*2+0], _values[3*2+1], _values[3*2+2] );
		}
		float3x3( float3^ _r0, float3^ _r1, float3^ _r2 ) {
			r0 = *_r0;
			r1 = *_r1;
			r2 = *_r2;
		}
		float3x3(	float r00, float r01, float r02, 
					float r10, float r11, float r12, 
					float r20, float r21, float r22 ) {
			r0.Set( r00, r01, r02 );
			r1.Set( r10, r11, r12 );
			r2.Set( r20, r21, r22 );
		}

		String^	ToString() override {
			return "{ " + r0.ToString() + ", " + r1.ToString() + ", " + r2.ToString() + " }";
		}

		static bool		TryParse( String^ _stringValue, float3x3% _value );

		void	Scale( float3 _Scale ) {
			r0 *= _Scale.x;
			r1 *= _Scale.y;
			r2 *= _Scale.z;
		}

		static float3x3	operator*( float3x3^ a, float3x3^ b ) {
			float3x3	R;
			R.r0.Set( a->r0.x*b->r0.x + a->r0.y*b->r1.x + a->r0.z*b->r2.x, /**/ a->r0.x*b->r0.y + a->r0.y*b->r1.y + a->r0.z*b->r2.y, /**/ a->r0.x*b->r0.z + a->r0.y*b->r1.z + a->r0.z*b->r2.z );
			R.r1.Set( a->r1.x*b->r0.x + a->r1.y*b->r1.x + a->r1.z*b->r2.x, /**/ a->r1.x*b->r0.y + a->r1.y*b->r1.y + a->r1.z*b->r2.y, /**/ a->r1.x*b->r0.z + a->r1.y*b->r1.z + a->r1.z*b->r2.z );
			R.r2.Set( a->r2.x*b->r0.x + a->r2.y*b->r1.x + a->r2.z*b->r2.x, /**/ a->r2.x*b->r0.y + a->r2.y*b->r1.y + a->r2.z*b->r2.y, /**/ a->r2.x*b->r0.z + a->r2.y*b->r1.z + a->r2.z*b->r2.z );
			return R;
		}

		static float3x3	operator*( float a, float3x3^ b ) {
			float3x3	R;
			R.r0.Set( a*b->r0.x, a*b->r0.y, a*b->r0.z );
			R.r1.Set( a*b->r1.x, a*b->r1.y, a*b->r1.z );
			R.r2.Set( a*b->r2.x, a*b->r2.y, a*b->r2.z );
			return R;
		}

		static float3	operator*( float3 a, float3x3^ b ) {
			float3	R;
			R.x = a.x*b->r0.x + a.y*b->r1.x + a.z*b->r2.x;
			R.y = a.x*b->r0.y + a.y*b->r1.y + a.z*b->r2.y;
			R.z = a.x*b->r0.z + a.y*b->r1.z + a.z*b->r2.z;
			return R;
		}

		static float3	operator*( float3x3^ a, float3 b ) {
			float3	R;
			R.x = a->r0.x * b.x + a->r0.y * b.y + a->r0.z * b.z;
			R.y = a->r1.x * b.x + a->r1.y * b.y + a->r1.z * b.z;
			R.z = a->r2.x * b.x + a->r2.y * b.y + a->r2.z * b.z;
			return R;
		}

		property float3	default[int] {
			float3		get( int _rowIndex ) {
				switch ( _rowIndex % 3 ) {
				case 0: return r0;
				case 1: return r1;
				case 2: return r2;
				}
				return r0;
			}
			void		set( int _rowIndex, float3 value ) {
				switch ( _rowIndex % 3 ) {
				case 0: r0 = value;	break;
				case 1: r1 = value;	break;
				case 2: r2 = value;	break;
				}
			}
		}

		property float	default[int,int] {
			float	get( int _rowIndex, int _columnIndex )				{ return (*this)[_rowIndex%3][_columnIndex%3]; }
			void	set( int _rowIndex, int _columnIndex, float value )	{
				switch ( _rowIndex%3 ) {
					case 0: r0[_columnIndex%3] = value; break;
					case 1: r1[_columnIndex%3] = value; break;
					case 2: r2[_columnIndex%3] = value; break;
				}
			}
		}

		property float	Determinant {
			float	get() {
				return (r0[0]*r1[1]*r2[2] + r0[1]*r1[2]*r2[0] + r0[2]*r1[0]*r2[1])
					 - (r2[0]*r1[1]*r0[2] + r2[1]*r1[2]*r0[0] + r2[2]*r1[0]*r0[1]);
			}
		}

		property float3x3	Inverse {
			float3x3	get() {
				float	det = Determinant;
				if ( Math::Abs(det) < float::Epsilon )
					throw gcnew Exception( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

				float	invDet = 1.0f / det;

				float3x3	R;
				R.r0[0] = +(r1[1] * r2[2] - r2[1] * r1[2]) * invDet;
				R.r1[0] = -(r1[0] * r2[2] - r2[0] * r1[2]) * invDet;
				R.r2[0] = +(r1[0] * r2[1] - r2[0] * r1[1]) * invDet;
				R.r0[1] = -(r0[1] * r2[2] - r2[1] * r0[2]) * invDet;
				R.r1[1] = +(r0[0] * r2[2] - r2[0] * r0[2]) * invDet;
				R.r2[1] = -(r0[0] * r2[1] - r2[0] * r0[1]) * invDet;
				R.r0[2] = +(r0[1] * r1[2] - r1[1] * r0[2]) * invDet;
				R.r1[2] = -(r0[0] * r1[2] - r1[0] * r0[2]) * invDet;
				R.r2[2] = +(r0[0] * r1[1] - r1[0] * r0[1]) * invDet;

				return	R;
			}
		}

		static property float3x3	Identity {
			float3x3	get() {
				return float3x3( 1, 0, 0, 0, 1, 0, 0, 0, 1 );
			}
		}

		// Generates the rotation matrix that rotates the _Source vector into the _Target vector
		float3x3	BuildRot( float3 _source, float3 _target ) {
			float3	ortho = _source.Cross( _target );
			float	length = ortho.Length;
			ortho = length > 1e-6f ? ortho / length : float3::UnitX;
			float	angle = (float) Math::Asin( length );
			return BuildFromAngleAxis( angle, ortho );
		}
		// (2016-01-06) Builds the remaining 2 orthogonal vectors from a given vector (very fast! no normalization or square root involved!)
		// Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
		float3x3	BuildRot( float3 _vector ) {
			r0 = _vector;
			_vector.OrthogonalBasis( r1, r2 );
			return *this;
		}

		float3x3	BuildRotationX( float _Angle ) {
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			r0.Set( 1,  0, 0 );
			r1.Set( 0,  C, S );
			r2.Set( 0, -S, C );

			return *this;
		}
		float3x3	BuildRotationY( float _Angle ) {
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			r0.Set( C, 0, -S );
			r1.Set( 0, 1,  0 );
			r2.Set( S, 0,  C );

			return *this;
		}
		float3x3	BuildRotationZ( float _Angle ) {
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			r0.Set(  C, S, 0 );
			r1.Set( -S, C, 0 );
			r2.Set(  0, 0, 1 );

			return *this;
		}

		/// <summary>
		/// Converts an angle+axis into a plain rotation matrix
		/// </summary>
		/// <param name="_Angle"></param>
		/// <param name="_Axis"></param>
		/// <returns></returns>
		float3x3	BuildFromAngleAxis( float _Angle, float3 _Axis ) {
			// Convert into a quaternion
			float3	qv = (float) Math::Sin( 0.5f * _Angle ) * _Axis;
			float	qs = (float) Math::Cos( 0.5f * _Angle );

			// Then into a matrix
			float	xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;

			xs = 2.0f * qv.x;	ys = 2.0f * qv.y;	zs = 2.0f * qv.z;

			wx = qs * xs;	wy = qs * ys;	wz = qs * zs;
			xx = qv.x * xs;	xy = qv.x * ys;	xz = qv.x * zs;
			yy = qv.y * ys;	yz = qv.y * zs;	zz = qv.z * zs;

			r0.Set( 1.0f -	yy - zz,		xy + wz,		xz - wy );
			r1.Set(			xy - wz, 1.0f -	xx - zz,		yz + wx );
			r2.Set(			xz + wy,		yz - wx, 1.0f -	xx - yy );

			return *this;
		}
	};

	//////////////////////////////////////////////////////////////////////////
	[System::Diagnostics::DebuggerDisplayAttribute( "{r0}, {r1}, {r2}, {r3}" )]
	public value struct	float4x4 {
	public:
		float4	r0;
		float4	r1;
		float4	r2;
		float4	r3;

		float4x4( cli::array<float>^ _values ) {
			r0.Set( _values[4*0+0], _values[4*0+1], _values[4*0+2], _values[4*0+3] );
			r1.Set( _values[4*1+0], _values[4*1+1], _values[4*1+2], _values[4*1+3] );
			r2.Set( _values[4*2+0], _values[4*2+1], _values[4*2+2], _values[4*2+3] );
			r3.Set( _values[4*3+0], _values[4*3+1], _values[4*3+2], _values[4*3+3] );
		}
		float4x4( float4^ _r0, float4^ _r1, float4^ _r2, float4^ _r3 ) {
			r0 = *_r0;
			r1 = *_r1;
			r2 = *_r2;
			r3 = *_r3;
		}
		float4x4(	float r00, float r01, float r02, float r03,
					float r10, float r11, float r12, float r13,
					float r20, float r21, float r22, float r23,
					float r30, float r31, float r32, float r33 ) {
			r0.Set( r00, r01, r02, r03 );
			r1.Set( r10, r11, r12, r13 );
			r2.Set( r20, r21, r22, r23 );
			r3.Set( r30, r31, r32, r33 );
		}

		String^	ToString() override {
			return "{ " + r0.ToString() + ", " + r1.ToString() + ", " + r2.ToString() + ", " + r3.ToString() + " }";
		}

		static bool		TryParse( String^ _stringValue, float4x4% _value );

		void	Scale( float3 _Scale ) {
			r0 *= _Scale.x;
			r1 *= _Scale.y;
			r2 *= _Scale.z;
		}

		static 	operator float3x3( float4x4 a ) {
			float3x3	R;
			R.r0.Set( a.r0.x, a.r0.y, a.r0.z );
			R.r1.Set( a.r1.x, a.r1.y, a.r1.z );
			R.r2.Set( a.r2.x, a.r2.y, a.r2.z );
			return R;
		}
		static float4x4	operator*( float4x4^ a, float4x4^ b ) {
			float4x4	R;
			R.r0.Set( a->r0.x*b->r0.x + a->r0.y*b->r1.x + a->r0.z*b->r2.x + a->r0.w*b->r3.x, /**/ a->r0.x*b->r0.y + a->r0.y*b->r1.y + a->r0.z*b->r2.y + a->r0.w*b->r3.y, /**/ a->r0.x*b->r0.z + a->r0.y*b->r1.z + a->r0.z*b->r2.z + a->r0.w*b->r3.z, /**/ a->r0.x*b->r0.w + a->r0.y*b->r1.w + a->r0.z*b->r2.w + a->r0.w*b->r3.w );
			R.r1.Set( a->r1.x*b->r0.x + a->r1.y*b->r1.x + a->r1.z*b->r2.x + a->r1.w*b->r3.x, /**/ a->r1.x*b->r0.y + a->r1.y*b->r1.y + a->r1.z*b->r2.y + a->r1.w*b->r3.y, /**/ a->r1.x*b->r0.z + a->r1.y*b->r1.z + a->r1.z*b->r2.z + a->r1.w*b->r3.z, /**/ a->r1.x*b->r0.w + a->r1.y*b->r1.w + a->r1.z*b->r2.w + a->r1.w*b->r3.w );
			R.r2.Set( a->r2.x*b->r0.x + a->r2.y*b->r1.x + a->r2.z*b->r2.x + a->r2.w*b->r3.x, /**/ a->r2.x*b->r0.y + a->r2.y*b->r1.y + a->r2.z*b->r2.y + a->r2.w*b->r3.y, /**/ a->r2.x*b->r0.z + a->r2.y*b->r1.z + a->r2.z*b->r2.z + a->r2.w*b->r3.z, /**/ a->r2.x*b->r0.w + a->r2.y*b->r1.w + a->r2.z*b->r2.w + a->r2.w*b->r3.w );
			R.r3.Set( a->r3.x*b->r0.x + a->r3.y*b->r1.x + a->r3.z*b->r2.x + a->r3.w*b->r3.x, /**/ a->r3.x*b->r0.y + a->r3.y*b->r1.y + a->r3.z*b->r2.y + a->r3.w*b->r3.y, /**/ a->r3.x*b->r0.z + a->r3.y*b->r1.z + a->r3.z*b->r2.z + a->r3.w*b->r3.z, /**/ a->r3.x*b->r0.w + a->r3.y*b->r1.w + a->r3.z*b->r2.w + a->r3.w*b->r3.w );

			return R;
		}

		static float4x4	operator*( float a, float4x4^ b ) {
			float4x4	R;
			R.r0.Set( a*b->r0.x, a*b->r0.y, a*b->r0.z, a*b->r0.w );
			R.r1.Set( a*b->r1.x, a*b->r1.y, a*b->r1.z, a*b->r1.w );
			R.r2.Set( a*b->r2.x, a*b->r2.y, a*b->r2.z, a*b->r2.w );
			R.r3.Set( a*b->r3.x, a*b->r3.y, a*b->r3.z, a*b->r3.w );
			return R;
		}

		static float4	operator*( float4 a, float4x4^ b ) {
			float4	R;
			R.x = a.x*b->r0.x + a.y*b->r1.x + a.z*b->r2.x + a.w*b->r3.x;
			R.y = a.x*b->r0.y + a.y*b->r1.y + a.z*b->r2.y + a.w*b->r3.y;
			R.z = a.x*b->r0.z + a.y*b->r1.z + a.z*b->r2.z + a.w*b->r3.z;
			R.w = a.x*b->r0.w + a.y*b->r1.w + a.z*b->r2.w + a.w*b->r3.w;

			return R;
		}

		property float4	default[int] {
			float4		get( int _rowIndex ) {
				switch ( _rowIndex & 3 ) {
				case 0: return r0;
				case 1: return r1;
				case 2: return r2;
				case 3: return r3;
				}
				return r0;
			}
			void		set( int _rowIndex, float4 value ) {
				switch ( _rowIndex & 3 ) {
				case 0: r0 = value; break;
				case 1: r1 = value; break;
				case 2: r2 = value; break;
				case 3: r3 = value; break;
				}
			}
		}

		property float	default[int,int] {
			float	get( int _rowIndex, int _columnIndex )				{ return (*this)[_rowIndex&3][_columnIndex&3]; }
			void	set( int _rowIndex, int _columnIndex, float value )	{
				switch ( _rowIndex & 3 ) {
				case 0: r0[_columnIndex&3] = value;	break;
				case 1: r1[_columnIndex&3] = value;	break;
				case 2: r2[_columnIndex&3] = value;	break;
				case 3: r3[_columnIndex&3] = value;	break;
				}
			}
		}

		float	CoFactor( int _dwRow, int _dwCol ) {
			return	((	(*this)[_dwRow+1, _dwCol+1]*(*this)[_dwRow+2, _dwCol+2]*(*this)[_dwRow+3, _dwCol+3] +
						(*this)[_dwRow+1, _dwCol+2]*(*this)[_dwRow+2, _dwCol+3]*(*this)[_dwRow+3, _dwCol+1] +
						(*this)[_dwRow+1, _dwCol+3]*(*this)[_dwRow+2, _dwCol+1]*(*this)[_dwRow+3, _dwCol+2] )

					-(	(*this)[_dwRow+3, _dwCol+1]*(*this)[_dwRow+2, _dwCol+2]*(*this)[_dwRow+1, _dwCol+3] +
						(*this)[_dwRow+3, _dwCol+2]*(*this)[_dwRow+2, _dwCol+3]*(*this)[_dwRow+1, _dwCol+1] +
						(*this)[_dwRow+3, _dwCol+3]*(*this)[_dwRow+2, _dwCol+1]*(*this)[_dwRow+1, _dwCol+2] ))
					* (((_dwRow + _dwCol) & 1) == 1 ? -1.0f : +1.0f);
		}

		property float	Determinant {
			float	get() {
				return r0.x * CoFactor( 0, 0 ) + r0.y * CoFactor( 0, 1 ) + r0.z * CoFactor( 0, 2 ) + r0.w * CoFactor( 0, 3 );
			}
		}

		property float4x4	Inverse {
			float4x4	get() {
				float	det = Determinant;
				if ( Math::Abs(det) < float::Epsilon )
					throw gcnew Exception( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

				float	recDet = 1.0f / det;

				float4x4	R;
				R.r0.Set( CoFactor( 0, 0 ) * recDet, CoFactor( 1, 0 ) * recDet, CoFactor( 2, 0 ) * recDet, CoFactor( 3, 0 ) * recDet );
				R.r1.Set( CoFactor( 0, 1 ) * recDet, CoFactor( 1, 1 ) * recDet, CoFactor( 2, 1 ) * recDet, CoFactor( 3, 1 ) * recDet );
				R.r2.Set( CoFactor( 0, 2 ) * recDet, CoFactor( 1, 2 ) * recDet, CoFactor( 2, 2 ) * recDet, CoFactor( 3, 2 ) * recDet );
				R.r3.Set( CoFactor( 0, 3 ) * recDet, CoFactor( 1, 3 ) * recDet, CoFactor( 2, 3 ) * recDet, CoFactor( 3, 3 ) * recDet );

				return	R;
			}
		}

		static property float4x4	Identity {
			float4x4	get() {
				return float4x4( 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 );
			}
		}

		// Makes a "look at" camera matrix (left-handed)
		void	BuildRotLeftHanded( float3 _position, float3 _target, float3 _up ) {
			float3	at = (_target - _position).Normalized;	// We want Z to point toward target
			float3	right = at.Cross( _up ).Normalized;		// We want X to point to the right
			float3	up = right.Cross( at );					// We want Y to point upward

			r0.Set( right.x, right.y, right.z, 0.0f );
			r1.Set( up.x, up.y, up.z, 0.0f );
			r2.Set( at.x, at.y, at.z, 0.0f );
			r3.Set( _position.x, _position.y, _position.z, 1.0f );
		}

		// Makes a regular "look at" matrix for objects (right-handed)
		void	BuildRotRightHanded( float3 _position, float3 _target, float3 _up ) {
			float3	at = (_target - _position).Normalized;	// We want Z to point toward target
			float3	right = _up.Cross( at ).Normalized;		// We want X to point to the right
			float3	up = at.Cross( right );					// We want Y to point upward

			r0.Set( right.x, right.y, right.z, 0.0f );
			r1.Set( up.x, up.y, up.z, 0.0f );
			r2.Set( at.x, at.y, at.z, 0.0f );
			r3.Set( _position.x, _position.y, _position.z, 1.0f );
		}
	
		void	BuildProjectionPerspective( float _FOVY, float _aspectRatio, float _Near, float _Far ) {
			float	H = (float) Math::Tan( 0.5f * _FOVY );
			float	W = _aspectRatio * H;
			float	Q =  _Far / (_Far - _Near);

			r0.Set( 1.0f / W, 0.0f, 0.0f, 0.0f );
			r1.Set( 0.0f, 1.0f / H, 0.0f, 0.0f );
			r2.Set( 0.0f, 0.0f, Q, 1.0f );
			r3.Set( 0.0f, 0.0f, -_Near * Q, 0.0f );
		}
 
		float4x4	BuildRotationX( float _Angle ) {
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			r0.Set( 1,  0, 0, 0 );
			r1.Set( 0,  C, S, 0 );
			r2.Set( 0, -S, C, 0 );
			r3.Set( 0,  0, 0, 1 );

			return *this;
		}
		float4x4	BuildRotationY( float _Angle ) {
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			r0.Set( C, 0, -S, 0 );
			r1.Set( 0, 1,  0, 0 );
			r2.Set( S, 0,  C, 0 );
			r3.Set( 0, 0,  0, 1 );

			return *this;
		}
		float4x4	BuildRotationZ( float _Angle ) {
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			r0.Set(  C, S, 0, 0 );
			r1.Set( -S, C, 0, 0 );
			r2.Set(  0, 0, 1, 0 );
			r3.Set(  0, 0, 0, 1 );

			return *this;
		}

		/// <summary>
		/// Converts an angle+axis into a plain rotation matrix
		/// </summary>
		/// <param name="_Angle"></param>
		/// <param name="_Axis"></param>
		/// <returns></returns>
		float4x4	BuildFromAngleAxis( float _Angle, float3 _Axis ) {
			// Convert into a quaternion
			float3	qv = (float) Math::Sin( 0.5f * _Angle ) * _Axis;
			float	qs = (float) Math::Cos( 0.5f * _Angle );

			// Then into a matrix
			float	xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;

			xs = 2.0f * qv.x;	ys = 2.0f * qv.y;	zs = 2.0f * qv.z;

			wx = qs * xs;	wy = qs * ys;	wz = qs * zs;
			xx = qv.x * xs;	xy = qv.x * ys;	xz = qv.x * zs;
			yy = qv.y * ys;	yz = qv.y * zs;	zz = qv.z * zs;

			r0.Set( 1.0f -	yy - zz,		xy + wz,		xz - wy, 0.0f );
			r1.Set(			xy - wz, 1.0f -	xx - zz,		yz + wx, 0.0f );
			r2.Set(			xz + wy,		yz - wx, 1.0f -	xx - yy, 0.0f );
			r3.Set( 0, 0, 0, 1 );

			return *this;
		}
	};
}
