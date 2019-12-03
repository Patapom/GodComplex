//////////////////////////////////////////////////////////////////////////
// Ultra simple math library
// We're trying to match the behavior of HLSL types float2/float3/float4/float4x4/etc.
//
#pragma once

#include <math.h>

//namespace BaseLib {

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
template<class T> inline T	SQR( const T& a )								{ return a * a;  }

static bool					ALMOST( float a, float b, float _eps=ALMOST_EPSILON )		{ return fabs( a - b ) < _eps; }
static bool					ALMOST( double a, double b, double _eps=ALMOST_EPSILON )	{ return fabs( a - b ) < _eps; }

//static inline bool		ISINFINITE( float a )							{ return _finitef( a ) != 0; }
//static inline bool		ISNAN( float a )								{ return _isnanf( a ) != 0; }
//static inline bool		ISVALID( float a )								{ return !ISNAN( a ) && !ISINFINITE( a ); }
static inline bool			ISINFINITE( float a )							{ return (((U32&) a) & 0x7FFFFFFFU) == 0x7F800000U; }
static inline bool			ISINFINITEPOSITIVE( float a )					{ return (((U32&) a) & 0xFFFFFFFFU) == 0x7F800000U; }
static inline bool			ISINFINITENEGATIVE( float a )					{ return (((U32&) a) & 0xFFFFFFFFU) == 0xFF800000U; }
static inline bool			ISNAN( float a )								{ return !ISINFINITE( a ) && (((U32&) a) & 0x7F800000U) == 0x7F800000U; }
static inline bool			ISVALID( float a )								{ return (((U32&) a) & 0x7F800000U) != 0x7F800000U; }

// Float2 used for point & vector operations
class   bfloat2 {
public:

	float	x, y;

	bfloat2() : x( 0 ), y( 0 )  {}
	bfloat2( float _x, float _y ) : x( _x ), y( _y )  {}

	void		Set( float _x, float _y ) { x = _x; y = _y; }

	float		LengthSq() const						{ return x*x + y*y; }
	float		Length() const							{ return sqrtf( x*x + y*y ); }
	bfloat2&	Normalize()								{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; return *this; }

	bfloat2		Lerp( const bfloat2& b, float t ) const	{ float r = 1.0f - t; return bfloat2( x * r + b.x * t, y * r + b.y * t ); }
	bfloat2		Min( const bfloat2& b ) const			{ return bfloat2( MIN( x, b.x ), MIN( y, b.y ) ); }
	bfloat2		Max( const bfloat2& b ) const			{ return bfloat2( MAX( x, b.x ), MAX( y, b.y ) ); }
	float		Min() const								{ return MIN( x, y ); }
	float		Max() const								{ return MAX( x, y ); }
	bool		Almost( const bfloat2& b ) const			{ return ALMOST( x, b.x ) && ALMOST( y, b.y ); }
	bool		Almost( const bfloat2& b, float _eps ) const	{ return ALMOST( x, b.x, _eps ) && ALMOST( y, b.y, _eps ); }

	float		Dot( const bfloat2& v ) const			{ return x*v.x + y*v.y; }
	float		Cross( const bfloat2& v ) const			{ return x*v.y - y*v.x; }	// Returns the Z component of the orthogonal vector

	float&		operator[]( int _index )				{ return (&x)[_index&1]; }
	const float&operator[]( int _index ) const			{ return (&x)[_index&1]; }

	bfloat2		operator-() const						{ return bfloat2( -x, -y ); }
	bfloat2		operator-( const bfloat2& v ) const		{ return bfloat2( x-v.x, y-v.y ); }
	bfloat2		operator+( const bfloat2& v ) const		{ return bfloat2( x+v.x, y+v.y ); }
	bfloat2		operator*( const bfloat2& v ) const		{ return bfloat2( x*v.x, y*v.y ); }
	bfloat2		operator*( float v ) const				{ return bfloat2( x * v, y * v ); }
	bfloat2		operator/( float v ) const				{ return bfloat2( x / v, y / v ); }
	bfloat2		operator/( const bfloat2& v ) const		{ return bfloat2( x / v.x, y / v.y ); }

	bfloat2&	operator-=( const bfloat2& v )			{ *this = *this - v; return *this; }
	bfloat2&	operator+=( const bfloat2& v )			{ *this = *this + v; return *this; }
	bfloat2&	operator*=( const bfloat2& v )			{ *this = *this * v; return *this; }
	bfloat2&	operator*=( float v )					{ *this = *this * v; return *this; }
	bfloat2&	operator/=( float v )					{ *this = *this / v; return *this; }
	bfloat2&	operator/=( const bfloat2& v )			{ *this = *this / v; return *this; }

	static const bfloat2	Zero;
	static const bfloat2	One;
	static const bfloat2	UnitX;
	static const bfloat2	UnitY;
};

static bfloat2   operator*( float a, const bfloat2& b )	{ return bfloat2( a*b.x, a*b.y ); }


// Float3 used for point & vector operations
class   bfloat3 {
public:

	float	x, y, z;

	bfloat3() : x( 0 ), y( 0 ), z( 0 )	{}
	bfloat3( float _x, float _y, float _z ) : x( _x ), y( _y ), z( _z )	{}
	bfloat3( const bfloat2& _xy, float _z ) : x( _xy.x ), y( _xy.y ), z( _z )	{}

	void		Set( float _x, float _y, float _z )		{ x = _x; y = _y; z = _z; }
	void		Set( const bfloat2& _xy, float _z )		{ x = _xy.x; y = _xy.y; z = _z; }

	float		LengthSq() const						{ return x*x + y*y + z*z; }
	float		Length() const							{ return sqrtf( x*x + y*y + z*z ); }
	bfloat3&	Normalize()								{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; z *= InvL; return *this; }

	bfloat3		Lerp( const bfloat3& b, float t ) const	{ float r = 1.0f - t; return bfloat3( x * r + b.x * t, y * r + b.y * t, z * r + b.z * t ); }
	bfloat3		Min( const bfloat3& b ) const			{ return bfloat3( MIN( x, b.x ), MIN( y, b.y ), MIN( z, b.z ) ); }
	bfloat3		Max( const bfloat3& b ) const			{ return bfloat3( MAX( x, b.x ), MAX( y, b.y ), MAX( z, b.z ) ); }
	float		Min() const								{ return MIN( MIN( x, y ), z ); }
	float		Max() const								{ return MAX( MAX( x, y ), z ); }
	bool		Almost( const bfloat3& b ) const			{ return ALMOST( x, b.x ) && ALMOST( y, b.y ) && ALMOST( z, b.z ); }
	bool		Almost( const bfloat3& b, float _eps ) const	{ return ALMOST( x, b.x, _eps ) && ALMOST( y, b.y, _eps ) && ALMOST( z, b.z, _eps ); }

	float		Dot( const bfloat3& v ) const			{ return x*v.x + y*v.y + z*v.z; }
	bfloat3		Cross( const bfloat3& v ) const			{ return bfloat3( y*v.z - z*v.y, v.x*z - v.z*x, x*v.y - y*v.x ); }

	// bmayaux (2016-01-04) Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
	// Builds up the 2 remaining vectors to form an orthonormal basis, assuming this vector is the "right" direction
	// Expected "this" to be normalized!
	// This code doesn't involve any square root!
	void		OrthogonalBasis( bfloat3& _left, bfloat3& _up ) const;

	float&		operator[]( int _index )				{ return (&x)[_index%3]; }
	const float&operator[]( int _index ) const			{ return (&x)[_index%3]; }

				operator bfloat2() const				{ return bfloat2( x, y ); }
	bfloat3		operator-() const						{ return bfloat3( -x, -y, -z ); }
	bfloat3		operator-( const bfloat3& v ) const		{ return bfloat3( x-v.x, y-v.y, z-v.z ); }
	bfloat3		operator+( const bfloat3& v ) const		{ return bfloat3( x+v.x, y+v.y, z+v.z ); }
	bfloat3		operator*( const bfloat3& v ) const		{ return bfloat3( x*v.x, y*v.y, z*v.z ); }
	bfloat3		operator*( float v ) const				{ return bfloat3( x * v, y * v, z * v ); }
	bfloat3		operator/( float v ) const				{ return bfloat3( x / v, y / v, z / v ); }
	bfloat3		operator/( const bfloat3& v ) const		{ return bfloat3( x / v.x, y / v.y, z / v.z ); }

	bfloat3&	operator-=( const bfloat3& v )			{ *this = *this - v; return *this; }
	bfloat3&	operator+=( const bfloat3& v )			{ *this = *this + v; return *this; }
	bfloat3&	operator*=( const bfloat3& v )			{ *this = *this * v; return *this; }
	bfloat3&	operator*=( float v )					{ *this = *this * v; return *this; }
	bfloat3&	operator/=( float v )					{ *this = *this / v; return *this; }
	bfloat3&	operator/=( const bfloat3& v )			{ *this = *this / v; return *this; }

	static const bfloat3	Zero;
	static const bfloat3	One;
	static const bfloat3	MaxFlt;
	static const bfloat3	UnitX;
	static const bfloat3	UnitY;
	static const bfloat3	UnitZ;
};

static bfloat3   operator*( float a, const bfloat3& b ) { return bfloat3( a*b.x, a*b.y, a*b.z ); }


// Float4 used for point & vector operations
class   bfloat4 {
public:

	float	x, y, z, w;

	bfloat4() : x( 0 ), y( 0 ), z( 0 ), w( 0 )  {}
	bfloat4( float _x, float _y, float _z, float _w ) : x( _x ), y( _y ), z( _z ), w( _w )   {}
	bfloat4( const bfloat2& _xy, float _z, float _w ) : x( _xy.x ), y( _xy.y ), z( _z ), w( _w ) {}
	bfloat4( const bfloat3& _xyz, float _w ) : x( _xyz.x ), y( _xyz.y ), z( _xyz.z ), w( _w ) {}

	void		Set( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
	void		Set( const bfloat2& _xy, float _z, float _w )	{ x = _xy.x; y = _xy.y; z = _z; w = _w; }
	void		Set( const bfloat3& _xyz, float _w )			{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = _w; }

	float		LengthSq() const						{ return x*x + y*y + z*z + w*w; }
	float		Length() const							{ return sqrtf( x*x + y*y + z*z + w*w ); }
	bfloat4&	Normalize()								{ float InvL = 1.0f / Length(); x *= InvL; y *= InvL; z *= InvL; w *= InvL; return *this; }

	bfloat4		Lerp( const bfloat4& b, float t ) const	{ float r = 1.0f - t; return bfloat4( x * r + b.x * t, y * r + b.y * t, z * r + b.z * t, w * r + b.w * t ); }
	bfloat4		Min( const bfloat4& b ) const			{ return bfloat4( MIN( x, b.x ), MIN( y, b.y ), MIN( z, b.z ), MIN( w, b.w ) ); }
	bfloat4		Max( const bfloat4& b ) const			{ return bfloat4( MAX( x, b.x ), MAX( y, b.y ), MAX( z, b.z ), MAX( w, b.w ) ); }
	float		Min() const								{ return MIN( MIN( MIN( x, y ), z), w ); }
	float		Max() const								{ return MAX( MAX( MAX( x, y ), z), w ); }
	bool		Almost( const bfloat4& b ) const		{ return ALMOST( x, b.x ) && ALMOST( y, b.y ) && ALMOST( z, b.z ) && ALMOST( w, b.w ); }
	bool		Almost( const bfloat4& b, float _eps ) const	{ return ALMOST( x, b.x, _eps ) && ALMOST( y, b.y, _eps ) && ALMOST( z, b.z, _eps ) && ALMOST( w, b.w, _eps ); }

	float		Dot( const bfloat4& b ) const			{ return x*b.x + y*b.y + z*b.z + w*b.w; }

	float&		operator[]( int _index )				{ return (&x)[_index&3]; }
	const float&operator[]( int _index ) const			{ return (&x)[_index&3]; }

				operator bfloat2() const				{ return bfloat2( x, y ); }
				operator bfloat3() const				{ return bfloat3( x, y, z ); }
	bfloat4		operator-()								{ return bfloat4( -x, -y, -z, -w ); }
	bfloat4		operator-( const bfloat4& v ) const		{ return bfloat4( x-v.x, y-v.y, z-v.z, w-v.w ); }
	bfloat4		operator+( const bfloat4& v ) const		{ return bfloat4( x+v.x, y+v.y, z+v.z, w+v.w ); }
	bfloat4		operator*( const bfloat4& v ) const		{ return bfloat4( x*v.x, y*v.y, z*v.z, w*v.w ); }
	bfloat4		operator*( float v ) const				{ return bfloat4( x * v, y * v, z * v, w * v ); }
	bfloat4		operator/( float v ) const				{ return bfloat4( x / v, y / v, z / v, w / v ); }
	bfloat4		operator/( const bfloat4& v ) const		{ return bfloat4( x / v.x, y / v.y, z / v.z, w / v.w ); }

	bfloat4&	operator-=( const bfloat4& v )			{ *this = *this - v; return *this; }
	bfloat4&	operator+=( const bfloat4& v )			{ *this = *this + v; return *this; }
	bfloat4&	operator*=( const bfloat4& v )			{ *this = *this * v; return *this; }
	bfloat4&	operator*=( float v )					{ *this = *this * v; return *this; }
	bfloat4&	operator/=( float v )					{ *this = *this / v; return *this; }
	bfloat4&	operator/=( const bfloat4& v )			{ *this = *this / v; return *this; }

	static bfloat4	QuatFromAngleAxis( float _Angle, const bfloat3& _Axis );

	static const bfloat4	Zero;
	static const bfloat4	One;
	static const bfloat4	UnitX;
	static const bfloat4	UnitY;
	static const bfloat4	UnitZ;
	static const bfloat4	UnitW;
};

static bfloat4   operator*( float a, const bfloat4& b )	{ return bfloat4( a*b.x, a*b.y, a*b.z, a*b.w ); }


//////////////////////////////////////////////////////////////////////////
// Float3x3 used for matrix operations
class	float3x3 {
public:
	bfloat3	r[3];

	float3x3() {}
	float3x3( const float3x3& _other ) {
		Set( _other );
	}
	float3x3( float _coeffs[9] ) {
		Set( _coeffs );
	}
	float3x3( const bfloat3& _r0, const bfloat3& _r1, const bfloat3& _r2 ) {
		Set( _r0, _r1, _r2 );
	}
	float3x3( const bfloat3 _rows[3] ) {
		Set( _rows[0], _rows[1], _rows[2] );
	}
	float3x3( float r00, float r01, float r02,
			  float r10, float r11, float r12,
			  float r20, float r21, float r22 ) {
		Set( r00, r01, r02, r10, r11, r12, r20, r21, r22 );
	}

	void	Set( const float3x3& _other ) {
		r[0] = _other.r[0];
		r[1] = _other.r[1];
		r[2] = _other.r[2];
	}
	void	Set( float _coeffs[9] ) {
		r[0].Set( _coeffs[3*0+0], _coeffs[3*0+1], _coeffs[3*0+2] );
		r[1].Set( _coeffs[3*1+0], _coeffs[3*1+1], _coeffs[3*1+2] );
		r[2].Set( _coeffs[3*2+0], _coeffs[3*2+1], _coeffs[3*2+2] );
	}
	void	Set( const bfloat3& _r0, const bfloat3& _r1, const bfloat3& _r2 ) {
		r[0] = _r0;
		r[1] = _r1;
		r[2] = _r2;
	}
	void	Set( float r00, float r01, float r02,
				 float r10, float r11, float r12,
				 float r20, float r21, float r22 ) {
		r[0].Set( r00, r01, r02 );
		r[1].Set( r10, r11, r12 );
		r[2].Set( r20, r21, r22 );
	}
 
	float3x3&			Invert() { *this = Inverse(); return *this; }
	float3x3			Inverse() const;
	float				Determinant() const;

	float3x3&			Scale( const bfloat3& _Scale ) {
		r[0] *= _Scale.x;
		r[1] *= _Scale.y;
		r[2] *= _Scale.z;
		return *this;
	}

	float3x3			operator*( float a ) const;
	float3x3			operator*( const float3x3& b ) const;
	float&				operator()( int _Row, int _Column );


	float3x3&			BuildFromAngleAxis( float _Angle, const bfloat3& _Axis )	{ return BuildFromQuat( bfloat4::QuatFromAngleAxis( _Angle, _Axis ) ); }
	float3x3&			BuildFromQuat( const bfloat4& _Quat );

	float3x3&			BuildRot( const bfloat3& _Source, const bfloat3& _Target );	// Generate the rotation matrix that rotates the _Source vector into the _Target vector
	float3x3&			BuildRot( const bfloat3& _Vector );							// Builds the remaining 2 orthogonal vectors from a given X vector (very fast! no normalization or square root involved!)
	float3x3&			BuildRotX( float _Angle );
	float3x3&			BuildRotY( float _Angle );
	float3x3&			BuildRotZ( float _Angle );
	float3x3&			BuildPYR( float _Pitch, float _Yaw, float _Roll );

	static const float3x3	Zero;
	static const float3x3	Identity;
};

float3x3	operator*( float a, const float3x3& b );
bfloat3		operator*( const bfloat3& a, const float3x3& b );
bfloat3		operator*( const float3x3& b, const bfloat3& a );


//////////////////////////////////////////////////////////////////////////
// Float4x4 used for matrix operations
class   float4x4 {
public:

	bfloat4	r[4];

	float4x4() {}
	float4x4( const float4x4& _other ) {
		Set( _other );
	}
	float4x4( float _coeffs[16] ) {
		Set( _coeffs );
	}
	float4x4( const bfloat4& _r0, const bfloat4& _r1, const bfloat4& _r2, const bfloat4& _r3 ) {
		Set( _r0, _r1, _r2, _r3 );
	}
	float4x4( const bfloat4 _rows[4] ) {
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
	void	Set( const bfloat4& _r0, const bfloat4& _r1, const bfloat4& _r2, const bfloat4& _r3 ) {
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

	float4x4&			Invert() { *this = Inverse(); return *this; }
	float4x4			Inverse() const;
	float				Determinant() const;
	float				CoFactor( int x, int y ) const;
	float4x4&			Normalize();

	float4x4&			Scale( const bfloat3& _Scale );

	float4x4			operator*( float a ) const;
	float4x4			operator*( const float4x4& b ) const;
	float&				operator()( int _Row, int _Column );
						operator float3x3() const;

	float4x4&			BuildPRS( const bfloat3& P, const bfloat4& R, const bfloat3& S=bfloat3::One );
	float4x4&			BuildFromAngleAxis( float _Angle, const bfloat3& _Axis )	{ return BuildFromQuat( bfloat4::QuatFromAngleAxis( _Angle, _Axis ) ); }
	float4x4&			BuildFromQuat( const bfloat4& _Quat );
	float4x4&			BuildProjectionPerspective( float _FOVY, float _AspectRatio, float _Near, float _Far );	// Builds a perspective projection matrix

	float4x4&			BuildRot( const bfloat3& _Source, const bfloat3& _Target );	// Generate the rotation matrix that rotates the _Source vector into the _Target vector
	float4x4&			BuildRot( const bfloat3& _Vector );							// Generate the rotation matrix that rotates the X vector into the provided vector
	float4x4&			BuildRotX( float _Angle );
	float4x4&			BuildRotY( float _Angle );
	float4x4&			BuildRotZ( float _Angle );
	float4x4&			BuildPYR( float _Pitch, float _Yaw, float _Roll );

	static const float4x4	Zero;
	static const float4x4	Identity;
};

float4x4	operator*( float a, const float4x4& b );
bfloat4		operator*( const bfloat4& a, const float4x4& b );
bfloat4		operator*( const float4x4& b, const bfloat4& a );


//////////////////////////////////////////////////////////////////////////
// Float16
class   half {
public:
	static const U16	SMALLEST_UINT = 0x0400;
	static const float	SMALLEST;// = 6.1035156e-005f;	// The smallest encodable float

	U16 raw;

	half()	{ raw=0; }
	half( float value );
	operator float() const;

	inline bool isDenormalized() const {
		U16 e = (raw >> 10) & 0x001f;
		U16 m = raw & 0x3ff;
		return e == 0 && m != 0;
	}

	inline bool isZero() const {
		return (raw & 0x7fff) == 0;
	}

	inline bool isNan() const {
		U16 e = (raw >> 10) & 0x001f;
		U16 m =  raw & 0x3ff;
		return e == 31 && m != 0;
	}

	inline bool isInfinity() const {
		U16 e = (raw >> 10) & 0x001f;
		U16 m =  raw & 0x3ff;
		return e == 31 && m == 0;
	}

	inline bool isNegative() const {
		return (raw & 0x8000) != 0;
	}

	static half positiveInfinity() {
		half h;
		h.raw = 0x7c00;
		return h;
	}

	static half negativeInfinity() {
		half h;
		h.raw = 0xfc00;
		return h;
	}

	// Quiet NaN
	static half qNaN() {
		half h;
		h.raw = 0x7fff;
		return h;
	}
	// Signaling NaN
	static half sNaN() {
		half h;
		h.raw = 0x7dff;
		return h;
	}
};

class   half4 {
public:
	half  x, y, z, w;

	half4() : x( 0.0f ), y( 0.0f ), z( 0.0f ), w( 0.0f )	{}
	half4( float _x, float _y, float _z, float _w ) : x( _x ), y( _y ), z( _z ), w( _w )	{}
	half4( const bfloat4& v ) : x( v.x ), y( v.y ), z( v.z ), w( v.w )	{}

	operator bfloat4()	{ return bfloat4( x, y, z, w ); }
};

//}	// namespace BaseLib