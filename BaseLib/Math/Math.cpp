#include "../Types.h"

using namespace BaseLib;

const bfloat2	bfloat2::Zero( 0, 0 );
const bfloat2	bfloat2::One( 1, 1 );
const bfloat2	bfloat2::UnitX( 1, 0 );
const bfloat2	bfloat2::UnitY( 0, 1 );

const bfloat3	bfloat3::Zero( 0, 0, 0 );
const bfloat3	bfloat3::One( 1, 1, 1 );
const bfloat3	bfloat3::MaxFlt( MAX_FLOAT, MAX_FLOAT, MAX_FLOAT );
const bfloat3	bfloat3::UnitX( 1, 0, 0 );
const bfloat3	bfloat3::UnitY( 0, 1, 0 );
const bfloat3	bfloat3::UnitZ( 0, 0, 1 );

const bfloat4	bfloat4::Zero( 0, 0, 0, 0 );
const bfloat4	bfloat4::One( 1, 1, 1, 1 );
const bfloat4	bfloat4::UnitX( 1, 0, 0, 0 );
const bfloat4	bfloat4::UnitY( 0, 1, 0, 0 );
const bfloat4	bfloat4::UnitZ( 0, 0, 1, 0 );
const bfloat4	bfloat4::UnitW( 0, 0, 0, 1 );

const float4x4	float4x4::Zero( 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 );
const float4x4	float4x4::Identity( 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 );

bfloat4	bfloat4::QuatFromAngleAxis( float _angle, const bfloat3& _axis ) {
	bfloat3	NormalizedAxis = _axis;
			NormalizedAxis.Normalize();

	_angle *= 0.5f;

	float	c = cosf(_angle);
	float	s = sinf(_angle);

	return bfloat4( s * NormalizedAxis, c );
}

float4x4  float4x4::Inverse() const {
	float	det = Determinant();
	ASSERT( abs(det) > 1e-6f, "Matrix is not inversible!" );
	det = 1.0f / det;

	float4x4  Temp;
	Temp.r[0].x = CoFactor( 0, 0 ) * det;
	Temp.r[1].x = CoFactor( 0, 1 ) * det;
	Temp.r[2].x = CoFactor( 0, 2 ) * det;
	Temp.r[3].x = CoFactor( 0, 3 ) * det;
	Temp.r[0].y = CoFactor( 1, 0 ) * det;
	Temp.r[1].y = CoFactor( 1, 1 ) * det;
	Temp.r[2].y = CoFactor( 1, 2 ) * det;
	Temp.r[3].y = CoFactor( 1, 3 ) * det;
	Temp.r[0].z = CoFactor( 2, 0 ) * det;
	Temp.r[1].z = CoFactor( 2, 1 ) * det;
	Temp.r[2].z = CoFactor( 2, 2 ) * det;
	Temp.r[3].z = CoFactor( 2, 3 ) * det;
	Temp.r[0].w = CoFactor( 3, 0 ) * det;
	Temp.r[1].w = CoFactor( 3, 1 ) * det;
	Temp.r[2].w = CoFactor( 3, 2 ) * det;
	Temp.r[3].w = CoFactor( 3, 3 ) * det;

	return	Temp;
}

float	float4x4::Determinant() const {
	return r[0].x * CoFactor( 0, 0 ) + r[0].y * CoFactor( 0, 1 ) + r[0].z * CoFactor( 0, 2 ) + r[0].w * CoFactor( 0, 3 ); 
}

// Small macro that accesses the component of a float4 given its index
#define COMP( f4, index ) ((float*) &f4.x)[index]
float	float4x4::CoFactor( int x, int y ) const {
	static int  IndexLoop[7] = { 0, 1, 2, 3, 0, 1, 2 };

	return	((	COMP( r[IndexLoop[x+1]], IndexLoop[y+1] ) * COMP( r[IndexLoop[x+2]], IndexLoop[y+2] ) * COMP( r[IndexLoop[x+3]], IndexLoop[y+3] ) +
				COMP( r[IndexLoop[x+1]], IndexLoop[y+2] ) * COMP( r[IndexLoop[x+2]], IndexLoop[y+3] ) * COMP( r[IndexLoop[x+3]], IndexLoop[y+1] ) +
				COMP( r[IndexLoop[x+1]], IndexLoop[y+3] ) * COMP( r[IndexLoop[x+2]], IndexLoop[y+1] ) * COMP( r[IndexLoop[x+3]], IndexLoop[y+2] ) )

			-(	COMP( r[IndexLoop[x+3]], IndexLoop[y+1] ) * COMP( r[IndexLoop[x+2]], IndexLoop[y+2] ) * COMP( r[IndexLoop[x+1]], IndexLoop[y+3] ) +
				COMP( r[IndexLoop[x+3]], IndexLoop[y+2] ) * COMP( r[IndexLoop[x+2]], IndexLoop[y+3] ) * COMP( r[IndexLoop[x+1]], IndexLoop[y+1] ) +
				COMP( r[IndexLoop[x+3]], IndexLoop[y+3] ) * COMP( r[IndexLoop[x+2]], IndexLoop[y+1] ) * COMP( r[IndexLoop[x+1]], IndexLoop[y+2] ) ))
			* (((x + y) & 1) == 1 ? -1.0f : +1.0f);
}

float4x4&	float4x4::Normalize() {
	((bfloat3&) r[0]).Normalize();
	((bfloat3&) r[1]).Normalize();
	((bfloat3&) r[2]).Normalize();
	return *this;
}

float4x4&	float4x4::Scale( const bfloat3& _scale ) {
	r[0] *= _scale.x;
	r[1] *= _scale.y;
	r[2] *= _scale.z;
	return *this;
}

bfloat4   operator*( const bfloat4& a, const float4x4& b ) {
	bfloat4	R;
	R.x = a.x * b.r[0].x + a.y * b.r[1].x + a.z * b.r[2].x + a.w * b.r[3].x;
	R.y = a.x * b.r[0].y + a.y * b.r[1].y + a.z * b.r[2].y + a.w * b.r[3].y;
	R.z = a.x * b.r[0].z + a.y * b.r[1].z + a.z * b.r[2].z + a.w * b.r[3].z;
	R.w = a.x * b.r[0].w + a.y * b.r[1].w + a.z * b.r[2].w + a.w * b.r[3].w;

	return R;
}

bfloat4   operator*( const float4x4& b, const bfloat4& a ) {
	bfloat4	R;
	R.x = a.Dot( b.r[0] );
	R.y = a.Dot( b.r[1] );
	R.z = a.Dot( b.r[2] );
	R.w = a.Dot( b.r[3] );
	return R;
}

float4x4	float4x4::BuildFromQuat( const bfloat4& _Quat ) {
	float4x4	result;
	bfloat4		q = _Quat;
	q.Normalize();

	float	xs = 2.0f * q.x;
	float	ys = 2.0f * q.y;
	float	zs = 2.0f * q.z;

	float	wx, wy, wz, xx, xy, xz, yy, yz, zz;
	wx = q.w * xs;	wy = q.w * ys;	wz = q.w * zs;
	xx = q.x * xs;	xy = q.x * ys;	xz = q.x * zs;
	yy = q.y * ys;	yz = q.y * zs;	zz = q.z * zs;

	result.r[0].x = 1.0f - yy - zz;
	result.r[0].y =        xy + wz;
	result.r[0].z =        xz - wy;
	result.r[0].w = 0.0f;

	result.r[1].x =        xy - wz;
	result.r[1].y = 1.0f - xx - zz;
	result.r[1].z =        yz + wx;
	result.r[1].w = 0.0f;

	result.r[2].x =        xz + wy;
	result.r[2].y =        yz - wx;
	result.r[2].z = 1.0f - xx - yy;
	result.r[2].w = 0.0f;

	result.r[3].x = 0.0f;
	result.r[3].y = 0.0f;
	result.r[3].z = 0.0f;
	result.r[3].w = 1.0f;

	return	result;
}

float4x4&	float4x4::PRS( const bfloat3& P, const bfloat4& R, const bfloat3& S ) {
	return *this = BuildFromPRS( P, R, S );
}

float4x4	float4x4::ProjectionPerspective( float _FOVY, float _aspectRatio, float _near, float _far ) {
	float	H = tanf( 0.5f * _FOVY );
	float	W = _aspectRatio * H;
	float	Q =  _far / (_far - _near);

	float4x4	result;
	result.r[0].Set( 1.0f / W, 0.0f, 0.0f, 0.0f );
	result.r[1].Set( 0.0f, 1.0f / H, 0.0f, 0.0f );
	result.r[2].Set( 0.0f, 0.0f, Q, 1.0f );
	result.r[3].Set( 0.0f, 0.0f, -_near * Q, 0.0f );

	return result;
}

float4x4	float4x4::BuildFromPRS( const bfloat3& P, const bfloat4& R, const bfloat3& S ) {
	float4x4	result = BuildFromQuat( R );

	result.r[0].x *= S.x;
	result.r[0].y *= S.x;
	result.r[0].z *= S.x;
	result.r[0].w *= S.x;
	result.r[1].x *= S.y;
	result.r[1].y *= S.y;
	result.r[1].z *= S.y;
	result.r[1].w *= S.y;
	result.r[2].x *= S.z;
	result.r[2].y *= S.z;
	result.r[2].z *= S.z;
	result.r[2].w *= S.z;
	result.r[3].x = P.x;
	result.r[3].y = P.y;
	result.r[3].z = P.z;

	return	result;
}

float4x4	float4x4::Rot( const bfloat3& _Source, const bfloat3& _Target ) {
	bfloat3	Ortho = _Source.Cross( _Target );
	float		Length = Ortho.Length();
	if ( Length > 1e-6f )
		Ortho = Ortho / Length;
	else
		Ortho.Set( 1, 0, 0 );

	float	Angle = asinf( Length );
	return BuildFromAngleAxis( Angle, Ortho );
}

float4x4	float4x4::RotX( float _Angle ) {
	float4x4	Result = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	Result.r[1].y = C;	Result.r[1].z = S;
	Result.r[2].y = -S;	Result.r[2].z = C;

	return Result;
}
float4x4	float4x4::RotY( float _Angle ) {
	float4x4	Result = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	Result.r[0].x = C;	Result.r[0].z = -S;
	Result.r[2].x = S;	Result.r[2].z = C;

	return Result;
}
float4x4	float4x4::RotZ( float _Angle ) {
	float4x4	Result = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	Result.r[0].x = C;	Result.r[0].y = S;
	Result.r[1].x = -S;	Result.r[1].y = C;

	return Result;
}
float4x4	float4x4::PYR( float _Pitch, float _Yaw, float _Roll ) {
	float4x4	Pitch = RotX( _Pitch );
	float4x4	Yaw = RotY( _Yaw );
	float4x4	Roll = RotZ( _Roll );

	float4x4	Result = Pitch * Yaw * Roll;

	return Result;
}

float4x4  float4x4::operator*( const float4x4& b ) const {
	float4x4  R;

	R.r[0].x = r[0].x * b.r[0].x + r[0].y * b.r[1].x + r[0].z * b.r[2].x + r[0].w * b.r[3].x;
	R.r[0].y = r[0].x * b.r[0].y + r[0].y * b.r[1].y + r[0].z * b.r[2].y + r[0].w * b.r[3].y;
	R.r[0].z = r[0].x * b.r[0].z + r[0].y * b.r[1].z + r[0].z * b.r[2].z + r[0].w * b.r[3].z;
	R.r[0].w = r[0].x * b.r[0].w + r[0].y * b.r[1].w + r[0].z * b.r[2].w + r[0].w * b.r[3].w;

	R.r[1].x = r[1].x * b.r[0].x + r[1].y * b.r[1].x + r[1].z * b.r[2].x + r[1].w * b.r[3].x;
	R.r[1].y = r[1].x * b.r[0].y + r[1].y * b.r[1].y + r[1].z * b.r[2].y + r[1].w * b.r[3].y;
	R.r[1].z = r[1].x * b.r[0].z + r[1].y * b.r[1].z + r[1].z * b.r[2].z + r[1].w * b.r[3].z;
	R.r[1].w = r[1].x * b.r[0].w + r[1].y * b.r[1].w + r[1].z * b.r[2].w + r[1].w * b.r[3].w;

	R.r[2].x = r[2].x * b.r[0].x + r[2].y * b.r[1].x + r[2].z * b.r[2].x + r[2].w * b.r[3].x;
	R.r[2].y = r[2].x * b.r[0].y + r[2].y * b.r[1].y + r[2].z * b.r[2].y + r[2].w * b.r[3].y;
	R.r[2].z = r[2].x * b.r[0].z + r[2].y * b.r[1].z + r[2].z * b.r[2].z + r[2].w * b.r[3].z;
	R.r[2].w = r[2].x * b.r[0].w + r[2].y * b.r[1].w + r[2].z * b.r[2].w + r[2].w * b.r[3].w;

	R.r[3].x = r[3].x * b.r[0].x + r[3].y * b.r[1].x + r[3].z * b.r[2].x + r[3].w * b.r[3].x;
	R.r[3].y = r[3].x * b.r[0].y + r[3].y * b.r[1].y + r[3].z * b.r[2].y + r[3].w * b.r[3].y;
	R.r[3].z = r[3].x * b.r[0].z + r[3].y * b.r[1].z + r[3].z * b.r[2].z + r[3].w * b.r[3].z;
	R.r[3].w = r[3].x * b.r[0].w + r[3].y * b.r[1].w + r[3].z * b.r[2].w + r[3].w * b.r[3].w;

	return R;
}

float&	float4x4::operator()( int _row, int _column ) {
	bfloat4&	row = r[_row&3];
	switch ( _column&3 ) {
		case 0: return row.x;
		case 1: return row.y;
		case 2: return row.z;
		case 3: return row.w;
	}
	return *((float*) 0);
}

//////////////////////////////////////////////////////////////////////////
// Half floats encoding
const float	half::SMALLEST = 6.1035156e-005f;	// The smallest encodable half float

#define F16_EXPONENT_BITS 0x1F
#define F16_EXPONENT_SHIFT 10
#define F16_EXPONENT_BIAS 15
#define F16_MANTISSA_BITS 0x03ff
#define F16_MANTISSA_SHIFT (23 - F16_EXPONENT_SHIFT)
#define F16_MAX_EXPONENT (F16_EXPONENT_BITS << F16_EXPONENT_SHIFT)

half::half( float value ) {
	U32 f32 = *((U32*) &value);
	raw = 0;

	// Decode IEEE 754 little-endian 32-bit floating-point value
	int sign = (f32 >> 16) & 0x8000;
	// Map exponent to the range [-127,128]
	int exponent = ((f32 >> 23) & 0xff) - 127;
	int mantissa = f32 & 0x007fffff;
	if ( exponent == 128 ) {
		// Infinity or NaN
		raw = U16( sign | F16_MAX_EXPONENT );
		if ( mantissa != 0 ) raw |= (mantissa & F16_MANTISSA_BITS);
	} else if ( exponent > 15 ) {
		// Overflow - flush to Infinity
		raw = U16( sign | F16_MAX_EXPONENT );
	} else if ( exponent > -15 ) {
		// Representable value
		exponent += F16_EXPONENT_BIAS;
		mantissa >>= F16_MANTISSA_SHIFT;
		raw = U16( sign | exponent << F16_EXPONENT_SHIFT | mantissa );
	} else {
		raw = U16(sign);
	}
}

half::operator float() const {
	union {
		float   f;
		U32	 ui;
	} f32;

	int sign = (raw & 0x8000) << 15;
	int exponent = (raw & 0x7c00) >> 10;
	int mantissa = (raw & 0x03ff);

	f32.f = 0.0f;
	if ( exponent == 0 ) {
		if ( mantissa != 0 ) 
			f32.f = mantissa / float(1 << 24);
	} else if ( exponent == 31 ) {
		f32.ui = sign | 0x7f800000 | mantissa;
	} else {
		float scale, decimal;
		exponent -= 15;
		if ( exponent < 0 ) {
			scale = float( 1.0 / (1 << -exponent) );
		} else {
			scale = float( 1 << exponent );
		}
		decimal = 1.0f + (float) mantissa / (1 << 10);
		f32.f = scale * decimal;
	}
	
	if ( sign != 0 )
		f32.f = -f32.f;

	return f32.f;
}
