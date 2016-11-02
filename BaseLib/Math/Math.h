//////////////////////////////////////////////////////////////////////////
// Ultra simple math library
// We're trying to match the behavior of HLSL types float2/float3/float4/float4x4/etc.
//
#pragma once

#include <math.h>

static const float			PI = 3.1415926535897932384626433832795f;			// ??
static const float			TWOPI = 6.283185307179586476925286766559f;			// 2PI
static const float			FOURPI = 12.566370614359172953850573533118f;		// 4PI
static const float			HALFPI = 1.5707963267948966192313216916398f;		// PI/2
static const float			INVPI = 0.31830988618379067153776752674503f;		// 1/PI
static const float			INV2PI = 0.15915494309189533576888376337251f;		// 1/(2PI)
static const float			INV4PI = 0.07957747154594766788444188168626f;		// 1/(4PI)
static const float			FLOAT32_MAX = 3.402823466e+38f;
static const float			GOLDEN_RATIO = 1.6180339887498948482045868343656f;	// Phi = (1+sqrt(5)) / 2
static const float			ALMOST_EPSILON = 1e-6f;

#define MAX_FLOAT			3.40282e+038f
#define RAD2DEG( a )		(57.295779513082320876798154814105f * (a))
#define DEG2RAD( a )		(0.01745329251994329576923690768489f * (a))
#define NUAJBYTE2FLOAT( b )	((b) / 255.0f)
template<class T> inline T	MIN( const T& a, const T& b )					{ return a < b ? a : b;  }
template<class T> inline T	MAX( const T& a, const T& b )					{ return a > b ? a : b;  }
template<class T> inline T	CLAMP( const T& x, const T& min, const T& max )	{ return MIN( MAX( min, x ), max ); }
template<class T> inline T	LERP( const T& a, const T& b, float t )			{ return a * (1.0f - t) + b * t; }
template<class T> inline T	SATURATE( const T& x )							{ return x < 0.0f ? 0.0f : (x > 1.0f ? 1.0f : x); }
static U8					FLOAT2BYTE( float f )							{ return U8( CLAMP( 255.0f * f, 0.0f, 255.0f ) ); }

static bool					ALMOST( float a, float b, float _eps=ALMOST_EPSILON )		{ return abs( a - b ) < _eps; }
static bool					ALMOST( double a, double b, double _eps=ALMOST_EPSILON )	{ return abs( a - b ) < _eps; }


// Float2 used for point & vector operations
class   float2 {
public:

	float	x, y;

	float2() : x( 0 ), y( 0 )  {}
	float2( float _x, float _y ) : x( _x ), y( _y )  {}

	void		Set( float _x, float _y ) { x = _x; y = _y; }

	float		LengthSq() const						{ return x*x + y*y; }
	float		Length() const							{ return sqrtf( x*x + y*y ); }
	float2&		Normalize()								{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; return *this; }

	float2		Lerp( const float2& b, float t ) const	{ float r = 1.0f - t; return float2( x * r + b.x * t, y * r + b.y * t ); }
	float2		Min( const float2& b ) const			{ return float2( MIN( x, b.x ), MIN( y, b.y ) ); }
	float2		Max( const float2& b ) const			{ return float2( MAX( x, b.x ), MAX( y, b.y ) ); }
	float		Min() const								{ return MIN( x, y ); }
	float		Max() const								{ return MAX( x, y ); }
	bool		Almost( const float2& b ) const			{ return ALMOST( x, b.x ) && ALMOST( y, b.y ); }
	bool		Almost( const float2& b, float _eps ) const	{ return ALMOST( x, b.x, _eps ) && ALMOST( y, b.y, _eps ); }

	float		Dot( const float2& v ) const			{ return x*v.x + y*v.y; }
	float		Cross( const float2& v ) const			{ return x*v.y - y*v.x; }	// Returns the Z component of the orthogonal vector

	float2		operator-() const						{ return float2( -x, -y ); }
	float2		operator-( const float2& v ) const		{ return float2( x-v.x, y-v.y ); }
	float2		operator+( const float2& v ) const		{ return float2( x+v.x, y+v.y ); }
	float2		operator*( const float2& v ) const		{ return float2( x*v.x, y*v.y ); }
	float2		operator*( float v ) const				{ return float2( x * v, y * v ); }
	float2		operator/( float v ) const				{ return float2( x / v, y / v ); }
	float2		operator/( const float2& v ) const		{ return float2( x / v.x, y / v.y ); }

	float2&		operator-=( const float2& v )			{ *this = *this - v; return *this; }
	float2&		operator+=( const float2& v )			{ *this = *this + v; return *this; }
	float2&		operator*=( const float2& v )			{ *this = *this * v; return *this; }
	float2&		operator*=( float v )					{ *this = *this * v; return *this; }
	float2&		operator/=( float v )					{ *this = *this / v; return *this; }
	float2&		operator/=( const float2& v )			{ *this = *this / v; return *this; }

	static const float2	Zero;
	static const float2	One;
	static const float2	UnitX;
	static const float2	UnitY;
};

static float2   operator*( float a, const float2& b )	{ return float2( a*b.x, a*b.y ); }


// Float3 used for point & vector operations
class   float3 {
public:

	float	x, y, z;

	float3() : x( 0 ), y( 0 ), z( 0 )	{}
	float3( float _x, float _y, float _z ) : x( _x ), y( _y ), z( _z )	{}
	float3( const float2& _xy, float _z ) : x( _xy.x ), y( _xy.y ), z( _z )	{}

	void		Set( float _x, float _y, float _z )		{ x = _x; y = _y; z = _z; }
	void		Set( const float2& _xy, float _z )		{ x = _xy.x; y = _xy.y; z = _z; }

	float		LengthSq() const						{ return x*x + y*y + z*z; }
	float		Length() const							{ return sqrtf( x*x + y*y + z*z ); }
	float3&		Normalize()								{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; z *= InvL; return *this; }

	float3		Lerp( const float3& b, float t ) const	{ float r = 1.0f - t; return float3( x * r + b.x * t, y * r + b.y * t, z * r + b.z * t ); }
	float3		Min( const float3& b ) const			{ return float3( MIN( x, b.x ), MIN( y, b.y ), MIN( z, b.z ) ); }
	float3		Max( const float3& b ) const			{ return float3( MAX( x, b.x ), MAX( y, b.y ), MAX( z, b.z ) ); }
	float		Min() const								{ return MIN( MIN( x, y ), z ); }
	float		Max() const								{ return MAX( MAX( x, y ), z ); }
	bool		Almost( const float3& b ) const			{ return ALMOST( x, b.x ) && ALMOST( y, b.y ) && ALMOST( z, b.z ); }
	bool		Almost( const float3& b, float _eps ) const	{ return ALMOST( x, b.x, _eps ) && ALMOST( y, b.y, _eps ) && ALMOST( z, b.z, _eps ); }

	float		Dot( const float3& v ) const			{ return x*v.x + y*v.y + z*v.z; }
	float3		Cross( const float3& v ) const			{ return float3( y*v.z - z*v.y, v.x*z - v.z*x, x*v.y - y*v.x ); }

				operator float2() const					{ return float2( x, y ); }
	float3		operator-() const						{ return float3( -x, -y, -z ); }
	float3		operator-( const float3& v ) const		{ return float3( x-v.x, y-v.y, z-v.z ); }
	float3		operator+( const float3& v ) const		{ return float3( x+v.x, y+v.y, z+v.z ); }
	float3		operator*( const float3& v ) const		{ return float3( x*v.x, y*v.y, z*v.z ); }
	float3		operator*( float v ) const				{ return float3( x * v, y * v, z * v ); }
	float3		operator/( float v ) const				{ return float3( x / v, y / v, z / v ); }
	float3		operator/( const float3& v ) const		{ return float3( x / v.x, y / v.y, z / v.z ); }

	float3&		operator-=( const float3& v )			{ *this = *this - v; return *this; }
	float3&		operator+=( const float3& v )			{ *this = *this + v; return *this; }
	float3&		operator*=( const float3& v )			{ *this = *this * v; return *this; }
	float3&		operator*=( float v )					{ *this = *this * v; return *this; }
	float3&		operator/=( float v )					{ *this = *this / v; return *this; }
	float3&		operator/=( const float3& v )			{ *this = *this / v; return *this; }

	static const float3	Zero;
	static const float3	One;
	static const float3	MaxFlt;
	static const float3	UnitX;
	static const float3	UnitY;
	static const float3	UnitZ;
};

static float3   operator*( float a, const float3& b ) { return float3( a*b.x, a*b.y, a*b.z ); }


// Float4 used for point & vector operations
class   float4 {
public:

	float	x, y, z, w;

	float4() : x( 0 ), y( 0 ), z( 0 ), w( 0 )  {}
	float4( float _x, float _y, float _z, float _w ) : x( _x ), y( _y ), z( _z ), w( _w )   {}
	float4( const float2& _xy, float _z, float _w ) : x( _xy.x ), y( _xy.y ), z( _z ), w( _w ) {}
	float4( const float3& _xyz, float _w ) : x( _xyz.x ), y( _xyz.y ), z( _xyz.z ), w( _w ) {}

	void		Set( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
	void		Set( const float2& _xy, float _z, float _w )	{ x = _xy.x; y = _xy.y; z = _z; w = _w; }
	void		Set( const float3& _xyz, float _w )				{ x = _xyz.x; y = _xyz.y; z = _xyz.x; w = _w; }

	float		LengthSq() const						{ return x*x + y*y + z*z + w*w; }
	float		Length() const							{ return sqrtf( x*x + y*y + z*z + w*w ); }
	float4&		Normalize()								{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; z *= InvL; w *= InvL; return *this; }

	float4		Lerp( const float4& b, float t ) const	{ float r = 1.0f - t; return float4( x * r + b.x * t, y * r + b.y * t, z * r + b.z * t, w * r + b.w * t ); }
	float4		Min( const float4& b ) const			{ return float4( MIN( x, b.x ), MIN( y, b.y ), MIN( z, b.z ), MIN( w, b.w ) ); }
	float4		Max( const float4& b ) const			{ return float4( MAX( x, b.x ), MAX( y, b.y ), MAX( z, b.z ), MAX( w, b.w ) ); }
	float		Min() const								{ return MIN( MIN( MIN( x, y ), z), w ); }
	float		Max() const								{ return MAX( MAX( MAX( x, y ), z), w ); }
	bool		Almost( const float4& b ) const			{ return ALMOST( x, b.x ) && ALMOST( y, b.y ) && ALMOST( z, b.z ) && ALMOST( w, b.w ); }
	bool		Almost( const float4& b, float _eps ) const	{ return ALMOST( x, b.x, _eps ) && ALMOST( y, b.y, _eps ) && ALMOST( z, b.z, _eps ) && ALMOST( w, b.w, _eps ); }

	float		Dot( const float4& b ) const			{ return x*b.x + y*b.y + z*b.z + w*b.w; }

				operator float2() const					{ return float2( x, y ); }
				operator float3() const					{ return float3( x, y, z ); }
	float4		operator-()								{ return float4( -x, -y, -z, -w ); }
	float4		operator-( const float4& v ) const		{ return float4( x-v.x, y-v.y, z-v.z, w-v.w ); }
	float4		operator+( const float4& v ) const		{ return float4( x+v.x, y+v.y, z+v.z, w+v.w ); }
	float4		operator*( const float4& v ) const		{ return float4( x*v.x, y*v.y, z*v.z, w*v.w ); }
	float4		operator*( float v ) const				{ return float4( x * v, y * v, z * v, w * v ); }
	float4		operator/( float v ) const				{ return float4( x / v, y / v, z / v, w / v ); }
	float4		operator/( const float4& v ) const		{ return float4( x / v.x, y / v.y, z / v.z, w / v.w ); }

	float4&		operator-=( const float4& v )			{ *this = *this - v; return *this; }
	float4&		operator+=( const float4& v )			{ *this = *this + v; return *this; }
	float4&		operator*=( const float4& v )			{ *this = *this * v; return *this; }
	float4&		operator*=( float v )					{ *this = *this * v; return *this; }
	float4&		operator/=( float v )					{ *this = *this / v; return *this; }
	float4&		operator/=( const float4& v )			{ *this = *this / v; return *this; }

	static float4	QuatFromAngleAxis( float _Angle, const float3& _Axis );

	static const float4	Zero;
	static const float4	One;
	static const float4	UnitX;
	static const float4	UnitY;
	static const float4	UnitZ;
	static const float4	UnitW;
};

static float4   operator*( float a, const float4& b )	{ return float4( a*b.x, a*b.y, a*b.z, a*b.w ); }


// Float4x4 used for matrix operations
class   float4x4 {
public:

	float4	r[4];

	float4x4() {}
	float4x4( const float4x4& _other ) {
		Set( _other );
	}
	float4x4( float _coeffs[16] ) {
		Set( _coeffs );
	}
	float4x4( const float4& _r0, const float4& _r1, const float4& _r2, const float4& _r3 ) {
		Set( _r0, _r1, _r2, _r3 );
	}
	float4x4( const float4 _rows[4] ) {
		Set( _rows[0], _rows[1], _rows[2], _rows[3] );
	}
	float4x4( float r00, float r01, float r02, float r03,
			  float r10, float r11, float r12, float r13,
			  float r20, float r21, float r22, float r23,
			  float r30, float r31, float r32, float r33 ) {
		Set( r00, r01, r02, r03, r10, r11, r12, r13, r20, r21, r22, r23, r30,  r31,  r32, r33 );
	}

	void	Set( const float4x4& _other ) {
		r[0] = _other.r[0];
		r[1] = _other.r[1];
		r[2] = _other.r[2];
		r[3] = _other.r[3];
	}
	void	Set( float _coeffs[16] ) {
		r[0].Set( _coeffs[4*0+0], _coeffs[4*0+1], _coeffs[4*0+2], _coeffs[4*0+3] );
		r[1].Set( _coeffs[4*1+0], _coeffs[4*1+1], _coeffs[4*1+2], _coeffs[4*1+3] );
		r[2].Set( _coeffs[4*2+0], _coeffs[4*2+1], _coeffs[4*2+2], _coeffs[4*2+3] );
		r[3].Set( _coeffs[4*3+0], _coeffs[4*3+1], _coeffs[4*3+2], _coeffs[4*3+3] );
	}
	void	Set( const float4& _r0, const float4& _r1, const float4& _r2, const float4& _r3 ) {
		r[0] = _r0;
		r[1] = _r1;
		r[2] = _r2;
		r[3] = _r3;
	}
	void	Set( float r00, float r01, float r02, float r03,
				 float r10, float r11, float r12, float r13,
				 float r20, float r21, float r22, float r23,
				 float r30, float r31, float r32, float r33 ) {
		r[0].Set( r00, r01, r02, r03 );
		r[1].Set( r10, r11, r12, r13 );
		r[2].Set( r20, r21, r22, r23 );
		r[3].Set( r30, r31, r32, r33 );
	}

	float4x4			Inverse() const;
	float4x4&			Invert() { *this = Inverse(); }
	float				Determinant() const;
	float				CoFactor( int x, int y ) const;
	float4x4&			Normalize();

	float4x4&			Scale( const float3& _Scale );

//	float4				operator*( const float4& b ) const;
	float4x4			operator*( const float4x4& b ) const;
	float&				operator()( int _Row, int _Column );

	float4x4&			PRS( const float3& P, const float4& R, const float3& S=float3::One );		// Builds a transform matrix from Position, Rotation (a quat) and Scale

	static float4x4		BuildFromPRS( const float3& P, const float4& R, const float3& S=float3::One );
	static float4x4		BuildFromAngleAxis( float _Angle, const float3& _Axis )	{ return BuildFromQuat( float4::QuatFromAngleAxis( _Angle, _Axis ) ); }
	static float4x4		BuildFromQuat( const float4& _Quat );
	static float4x4		ProjectionPerspective( float _FOVY, float _AspectRatio, float _Near, float _Far );	// Builds a perspective projection matrix

	static float4x4		Rot( const float3& _Source, const float3& _Target );	// Generate the rotation matrix that rotates the _Source vector into the _Target vector
	static float4x4		RotX( float _Angle );
	static float4x4		RotY( float _Angle );
	static float4x4		RotZ( float _Angle );
	static float4x4		PYR( float _Pitch, float _Yaw, float _Roll );

	static const float4x4	Zero;
	static const float4x4	Identity;
};

float4   operator*( const float4& a, const float4x4& b );
float4   operator*( const float4x4& b, const float4& a );


// Float16
class   half {
public:
	static const U16	SMALLEST_UINT = 0x0400;
	static const float	SMALLEST;// = 6.1035156e-005f;	// The smallest encodable float

	U16 raw;

	half()	{ raw=0; }
	half( float value );
	operator float() const;
};

class   half4 {
public:
	half  x, y, z, w;

	half4() : x( 0.0f ), y( 0.0f ), z( 0.0f ), w( 0.0f )	{}
	half4( float _x, float _y, float _z, float _w ) : x( _x ), y( _y ), z( _z ), w( _w )	{}
	half4( const float4& v ) : x( v.x ), y( v.y ), z( v.z ), w( v.w )	{}

	operator float4()	{ return float4( x, y, z, w ); }
};
