//////////////////////////////////////////////////////////////////////////
// Ultra simple math library
// Self-addressing operators (like +=, -=, etc.) don't exist for brevity so write a = a + b instead of a += b thank you
//
#ifndef _NUAJ_MATH_H_
#define _NUAJ_MATH_H_

#include <math.h>

// Override some functions with our own implementations
//#ifdef GODCOMPLEX
#if 1

#define log2f( a )			ASM_log2f( a )
#define expf( a )			ASM_expf( a )
#define powf( a, b )		ASM_powf( a, b )
#define fmodf( a, b )		ASM_fmodf( a, b )
#define floorf( a )			ASM_floorf( a )
#define ceilf( a )			ASM_ceilf( a )
#define acosf( a )			ASM_acosf( a )
#define asinf( a )			ASM_asinf( a )

#else

// #define log2f( a )			(float(logf( a ) * 1.4426950408889634073599246810019))
// #define floorf( a )			int(floorf( a ))
// #define ceilf( a )			int(ceilf( a ))

#endif

static const float			PI = 3.1415926535897932384626433832795f;			// ??
static const float			TWOPI = 6.283185307179586476925286766559f;			// 2PI
static const float			HALFPI = 1.5707963267948966192313216916398f;		// PI/2
static const float			INVPI = 0.31830988618379067153776752674503f;		// 1/PI
static const float			INV2PI = 0.15915494309189533576888376337251f;		// 1/(2PI)
static const float			INV4PI = 0.07957747154594766788444188168626f;		// 1/(4PI)
static const float			FLOAT32_MAX = 3.402823466e+38f;
static const float			GOLDEN_RATIO = 1.6180339887498948482045868343656f;	// Phi = (1+sqrt(5)) / 2

#define MAX_FLOAT			3.40282e+038f
#define NUAJRAD2DEG( a )	(57.295779513082320876798154814105f * (a))
#define NUAJDEG2RAD( a )	(0.01745329251994329576923690768489f * (a))
#define NUAJBYTE2FLOAT( b )	((b) / 255.0f)
template<class T> inline T	MIN( const T& a, const T& b )					{ return a < b ? a : b;  }
template<class T> inline T	MAX( const T& a, const T& b )					{ return a > b ? a : b;  }
template<class T> inline T	CLAMP( const T& x, const T& min, const T& max )	{ return MIN( MAX( min, x ), max ); }
template<class T> inline T	LERP( const T& a, const T& b, float t )			{ return a * (1.0f - t) + b * t; }
template<class T> inline T	SATURATE( const T& x )							{ return x < 0.0f ? 0.0f : (x > 1.0f ? 1.0f : x); }
static U8					FLOAT2BYTE( float f )							{ return U8( CLAMP( 255.0f * f, 0.0f, 255.0f ) ); }


// Float2 used for point & vector operations
class   NjFloat2
{
public:

	float	x, y;

	NjFloat2() : x( 0 ), y( 0 )  {}
	NjFloat2( float _x, float _y ) : x( _x ), y( _y )  {}

	void		Set( float _x, float _y ) { x = _x; y = _y; }

	float		LengthSq() const	{ return x*x + y*y; }
	float		Length() const		{ return sqrtf( x*x + y*y ); }
	NjFloat2&	Normalize()			{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; return *this; }

	NjFloat2	Lerp( const NjFloat2& b, float t ) const	{ float r = 1.0f - t; return NjFloat2( x * r + b.x * t, y * r + b.y * t ); }
	NjFloat2	Min( const NjFloat2& b ) const	{ return NjFloat2( MIN( x, b.x ), MIN( y, b.y ) ); }
	NjFloat2	Max( const NjFloat2& b ) const	{ return NjFloat2( MAX( x, b.x ), MAX( y, b.y ) ); }
	float		Min() const						{ return MIN( x, y ); }
	float		Max() const						{ return MAX( x, y ); }

	NjFloat2	operator-( const NjFloat2& v ) const	{ return NjFloat2( x-v.x, y-v.y ); }
	NjFloat2	operator+( const NjFloat2& v ) const	{ return NjFloat2( x+v.x, y+v.y ); }
	NjFloat2	operator*( const NjFloat2& v ) const	{ return NjFloat2( x*v.x, y*v.y ); }
	NjFloat2	operator*( float v ) const				{ return NjFloat2( x * v, y * v ); }
	NjFloat2	operator/( float v ) const				{ return NjFloat2( x / v, y / v ); }
	NjFloat2	operator/( const NjFloat2& v ) const	{ return NjFloat2( x / v.x, y / v.y ); }
	float		operator|( const NjFloat2& v ) const	{ return x*v.x + y*v.y; }
	float		operator^( const NjFloat2& v ) const	{ return x*v.y - y*v.x; }	// Returns the Z component of the orthogonal vector

	static const NjFloat2	Zero;
	static const NjFloat2	One;
	static const NjFloat2	UnitX;
	static const NjFloat2	UnitY;
};

static NjFloat2   operator*( float a, const NjFloat2& b )	{ return NjFloat2( a*b.x, a*b.y ); }


// Float3 used for point & vector operations
class   NjFloat3
{
public:

	float	x, y, z;

	NjFloat3() : x( 0 ), y( 0 ), z( 0 )	{}
	NjFloat3( float _x, float _y, float _z ) : x( _x ), y( _y ), z( _z )	{}
	NjFloat3( const NjFloat2& _xy, float _z ) : x( _xy.x ), y( _xy.y ), z( _z )	{}

	void		Set( float _x, float _y, float _z ) { x = _x; y = _y; z = _z; }

	float		LengthSq() const	{ return x*x + y*y + z*z; }
	float		Length() const		{ return sqrtf( x*x + y*y + z*z ); }
	NjFloat3&   Normalize()			{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; z *= InvL; return *this; }

	NjFloat3	Lerp( const NjFloat3& b, float t ) const	{ float r = 1.0f - t; return NjFloat3( x * r + b.x * t, y * r + b.y * t, z * r + b.z * t ); }
	NjFloat3	Min( const NjFloat3& b ) const	{ return NjFloat3( MIN( x, b.x ), MIN( y, b.y ), MIN( z, b.z ) ); }
	NjFloat3	Max( const NjFloat3& b ) const	{ return NjFloat3( MAX( x, b.x ), MAX( y, b.y ), MAX( z, b.z ) ); }
	float		Min() const						{ return MIN( MIN( x, y ), z ); }
	float		Max() const						{ return MAX( MAX( x, y ), z ); }

	NjFloat3	operator-( const NjFloat3& v ) const	{ return NjFloat3( x-v.x, y-v.y, z-v.z ); }
	NjFloat3	operator+( const NjFloat3& v ) const	{ return NjFloat3( x+v.x, y+v.y, z+v.z ); }
	NjFloat3	operator*( const NjFloat3& v ) const	{ return NjFloat3( x*v.x, y*v.y, z*v.z ); }
	NjFloat3	operator*( float v ) const				{ return NjFloat3( x * v, y * v, z * v ); }
	NjFloat3	operator/( float v ) const				{ return NjFloat3( x / v, y / v, z / v ); }
	NjFloat3	operator/( const NjFloat3& v ) const	{ return NjFloat3( x / v.x, y / v.y, z / v.z ); }
	float		operator|( const NjFloat3& v ) const	{ return x*v.x + y*v.y + z*v.z; }
	NjFloat3	operator-() const						{ return NjFloat3( -x, -y, -z ); }
				operator NjFloat2() const				{ return NjFloat2( x, y ); }
	NjFloat3	operator^( const NjFloat3& v ) const	{ return NjFloat3( y * v.z - v.y * z, z * v.x - v.z * x, x * v.y - v.x * y ); }

	static const NjFloat3	Zero;
	static const NjFloat3	One;
	static const NjFloat3	UnitX;
	static const NjFloat3	UnitY;
	static const NjFloat3	UnitZ;
};

static NjFloat3   operator*( float a, const NjFloat3& b ) { return NjFloat3( a*b.x, a*b.y, a*b.z ); }


// Float4 used for point & vector operations
class   NjFloat4
{
public:

	float	x, y, z, w;

	NjFloat4() : x( 0 ), y( 0 ), z( 0 ), w( 0 )  {}
	NjFloat4( float _x, float _y, float _z, float _w ) : x( _x ), y( _y ), z( _z ), w( _w )   {}
	NjFloat4( const NjFloat2& _xy, float _z, float _w ) : x( _xy.x ), y( _xy.y ), z( _z ), w( _w ) {}
	NjFloat4( const NjFloat3& _xyz, float _w ) : x( _xyz.x ), y( _xyz.y ), z( _xyz.z ), w( _w ) {}

	void		Set( float _x, float _y, float _z, float _w ) { x = _x; y = _y; z = _z; w = _w; }

	float		LengthSq() const	{ return x*x + y*y + z*z + w*w; }
	float		Length() const		{ return sqrtf( x*x + y*y + z*z + w*w ); }
	NjFloat4&   Normalize()			{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; z *= InvL; w *= InvL; return *this; }

	NjFloat4	Lerp( const NjFloat4& b, float t ) const	{ float r = 1.0f - t; return NjFloat4( x * r + b.x * t, y * r + b.y * t, z * r + b.z * t, w * r + b.w * t ); }
	NjFloat4	Min( const NjFloat4& b ) const	{ return NjFloat4( MIN( x, b.x ), MIN( y, b.y ), MIN( z, b.z ), MIN( w, b.w ) ); }
	NjFloat4	Max( const NjFloat4& b ) const	{ return NjFloat4( MAX( x, b.x ), MAX( y, b.y ), MAX( z, b.z ), MAX( w, b.w ) ); }
	float		Min() const						{ return MIN( MIN( MIN( x, y ), z), w ); }
	float		Max() const						{ return MAX( MAX( MAX( x, y ), z), w ); }

	NjFloat4	operator-()								{ return NjFloat4( -x, -y, -z, -w ); }
				operator NjFloat3()						{ return NjFloat3( x, y, z ); }
	NjFloat4	operator-( const NjFloat4& v ) const	{ return NjFloat4( x-v.x, y-v.y, z-v.z, w-v.w ); }
	NjFloat4	operator+( const NjFloat4& v ) const	{ return NjFloat4( x+v.x, y+v.y, z+v.z, w+v.w ); }
	NjFloat4	operator*( const NjFloat4& v ) const	{ return NjFloat4( x*v.x, y*v.y, z*v.z, w*v.w ); }
	NjFloat4	operator*( float v ) const				{ return NjFloat4( x * v, y * v, z * v, w * v ); }
	NjFloat4	operator/( float v ) const				{ return NjFloat4( x / v, y / v, z / v, w / v ); }
	NjFloat4	operator/( const NjFloat4& v ) const	{ return NjFloat4( x / v.x, y / v.y, z / v.z, w / v.w ); }
	float		operator|( const NjFloat4& v ) const	{ return x*v.x + y*v.y + z*v.z + w*v.w; }

	static NjFloat4	QuatFromAngleAxis( float _Angle, const NjFloat3& _Axis );

	static const NjFloat4	Zero;
	static const NjFloat4	One;
	static const NjFloat4	UnitX;
	static const NjFloat4	UnitY;
	static const NjFloat4	UnitZ;
	static const NjFloat4	UnitW;
};

static NjFloat4   operator*( float a, const NjFloat4& b ) { return NjFloat4( a*b.x, a*b.y, a*b.z, a*b.w ); }


// Float4x4 used for matrix operations
class   NjFloat4x4
{
public:

	float	m[16];

	NjFloat4			GetRow( int _RowIndex ) const								{ return NjFloat4( m[4*_RowIndex+0], m[4*_RowIndex+1], m[4*_RowIndex+2], m[4*_RowIndex+3] ); }
	NjFloat4x4&			SetRow( int _RowIndex, const NjFloat4& _Row )				{ m[4*_RowIndex+0] = _Row.x; m[4*_RowIndex+1] = _Row.y; m[4*_RowIndex+2] = _Row.z; m[4*_RowIndex+3] = _Row.w; return *this; }
	NjFloat4x4&			SetRow( int _RowIndex, const NjFloat3& _Row, float _w=0 )	{ m[4*_RowIndex+0] = _Row.x; m[4*_RowIndex+1] = _Row.y; m[4*_RowIndex+2] = _Row.z; m[4*_RowIndex+3] = _w; return *this; }
	NjFloat4x4			Inverse() const;
	float				Determinant() const;
	float				CoFactor( int x, int y ) const;

//	NjFloat4			operator*( const NjFloat4& b ) const;
	NjFloat4x4			operator*( const NjFloat4x4& b ) const;
	float&				operator()( int _Row, int _Column );

	NjFloat4x4&			FromAngleAxis( float _Angle, const NjFloat3& _Axis )	{ return FromQuat( NjFloat4::QuatFromAngleAxis( _Angle, _Axis ) ); }
	NjFloat4x4&			FromQuat( const NjFloat4& _Quat );
	NjFloat4x4&			PRS( const NjFloat3& P, const NjFloat4& R, const NjFloat3& S=NjFloat3::One );
	static NjFloat4x4	BuildFromPRS( const NjFloat3& P, const NjFloat4& R, const NjFloat3& S=NjFloat3::One );

	NjFloat4x4&			Rot( const NjFloat3& _Source, const NjFloat3& _Target );	// Generate the rotation matrix that rotates the _Source vector into the _Target vector
	NjFloat4x4&			RotX( float _Angle );
	NjFloat4x4&			RotY( float _Angle );
	NjFloat4x4&			RotZ( float _Angle );
	NjFloat4x4&			PYR( float _Pitch, float _Yaw, float _Roll );

	static const NjFloat4x4	Zero;
	static const NjFloat4x4	Identity;
};

NjFloat4   operator*( const NjFloat4& a, const NjFloat4x4& b );


// Float16
class   NjHalf
{
public:
	U16 raw;

	NjHalf()	{ raw=0; }
	NjHalf( float value );
	operator float() const;
};

class   NjHalf4
{
public:
	NjHalf  x, y, z, w;

	NjHalf4() : x( 0.0f ), y( 0.0f ), z( 0.0f ), w( 0.0f )	{}
	NjHalf4( float _x, float _y, float _z, float _w ) : x( _x ), y( _y ), z( _z ), w( _w )	{}
	NjHalf4( const NjFloat4& v ) : x( v.x ), y( v.y ), z( v.z ), w( v.w )	{}

	operator NjFloat4()	{ return NjFloat4( x, y, z, w ); }
};

#endif  // _NUAJ_MATH_H_
