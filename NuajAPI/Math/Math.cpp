#include "../API/Types.h"

const float2	float2::Zero( 0, 0 );
const float2	float2::One( 1, 1 );
const float2	float2::UnitX( 1, 0 );
const float2	float2::UnitY( 0, 1 );

const float3	float3::Zero( 0, 0, 0 );
const float3	float3::One( 1, 1, 1 );
const float3	float3::MaxFlt( MAX_FLOAT, MAX_FLOAT, MAX_FLOAT );
const float3	float3::UnitX( 1, 0, 0 );
const float3	float3::UnitY( 0, 1, 0 );
const float3	float3::UnitZ( 0, 0, 1 );

const float4	float4::Zero( 0, 0, 0, 0 );
const float4	float4::One( 1, 1, 1, 1 );
const float4	float4::UnitX( 1, 0, 0, 0 );
const float4	float4::UnitY( 0, 1, 0, 0 );
const float4	float4::UnitZ( 0, 0, 1, 0 );
const float4	float4::UnitW( 0, 0, 0, 1 );

const float4x4	float4x4::Zero = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
const float4x4	float4x4::Identity = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

float4	float4::QuatFromAngleAxis( float _Angle, const float3& _Axis )
{
	float3	NormalizedAxis = _Axis;
				NormalizedAxis.Normalize();

	_Angle *= 0.5f;

	float	c = cosf(_Angle);
	float	s = sinf(_Angle);

	return float4( s * NormalizedAxis, c );
}

float4x4  float4x4::Inverse() const
{
	float	Det = Determinant();
	ASSERT( abs(Det) > 1e-6f, "Matrix is not inversible !" );

	Det = 1.0f / Det;

	float4x4  Temp;
	Temp.m[4*0+0] = CoFactor( 0, 0 ) * Det;
	Temp.m[4*1+0] = CoFactor( 0, 1 ) * Det;
	Temp.m[4*2+0] = CoFactor( 0, 2 ) * Det;
	Temp.m[4*3+0] = CoFactor( 0, 3 ) * Det;
	Temp.m[4*0+1] = CoFactor( 1, 0 ) * Det;
	Temp.m[4*1+1] = CoFactor( 1, 1 ) * Det;
	Temp.m[4*2+1] = CoFactor( 1, 2 ) * Det;
	Temp.m[4*3+1] = CoFactor( 1, 3 ) * Det;
	Temp.m[4*0+2] = CoFactor( 2, 0 ) * Det;
	Temp.m[4*1+2] = CoFactor( 2, 1 ) * Det;
	Temp.m[4*2+2] = CoFactor( 2, 2 ) * Det;
	Temp.m[4*3+2] = CoFactor( 2, 3 ) * Det;
	Temp.m[4*0+3] = CoFactor( 3, 0 ) * Det;
	Temp.m[4*1+3] = CoFactor( 3, 1 ) * Det;
	Temp.m[4*2+3] = CoFactor( 3, 2 ) * Det;
	Temp.m[4*3+3] = CoFactor( 3, 3 ) * Det;

	return	Temp;
}

float	   float4x4::Determinant() const
{
	return m[0] * CoFactor( 0, 0 ) + m[1] * CoFactor( 0, 1 ) + m[2] * CoFactor( 0, 2 ) + m[3] * CoFactor( 0, 3 ); 
}

float	   float4x4::CoFactor( int x, int y ) const
{
	static int  IndexLoop[7] = { 0, 1, 2, 3, 0, 1, 2 };

	return	((	m[4*IndexLoop[x+1]+IndexLoop[y+1]]*m[4*IndexLoop[x+2]+IndexLoop[y+2]]*m[4*IndexLoop[x+3]+IndexLoop[y+3]] +
				m[4*IndexLoop[x+1]+IndexLoop[y+2]]*m[4*IndexLoop[x+2]+IndexLoop[y+3]]*m[4*IndexLoop[x+3]+IndexLoop[y+1]] +
				m[4*IndexLoop[x+1]+IndexLoop[y+3]]*m[4*IndexLoop[x+2]+IndexLoop[y+1]]*m[4*IndexLoop[x+3]+IndexLoop[y+2]] )

			-(	m[4*IndexLoop[x+3]+IndexLoop[y+1]]*m[4*IndexLoop[x+2]+IndexLoop[y+2]]*m[4*IndexLoop[x+1]+IndexLoop[y+3]] +
				m[4*IndexLoop[x+3]+IndexLoop[y+2]]*m[4*IndexLoop[x+2]+IndexLoop[y+3]]*m[4*IndexLoop[x+1]+IndexLoop[y+1]] +
				m[4*IndexLoop[x+3]+IndexLoop[y+3]]*m[4*IndexLoop[x+2]+IndexLoop[y+1]]*m[4*IndexLoop[x+1]+IndexLoop[y+2]] ))
			* (((x + y) & 1) == 1 ? -1.0f : +1.0f);
}

float4x4&	float4x4::Normalize()
{
	float3&	X = *((float3*) &m[4*0]);
				X.Normalize();
	float3&	Y = *((float3*) &m[4*1]);
				Y.Normalize();
	float3&	Z = *((float3*) &m[4*2]);
				Z.Normalize();

	return *this;
}

float4x4&	float4x4::Scale( const float3& _Scale )
{
	m[4*0+0] *= _Scale.x;	m[4*0+1] *= _Scale.x;	m[4*0+2] *= _Scale.x; 	m[4*0+3] *= _Scale.x;
	m[4*1+0] *= _Scale.y;	m[4*1+1] *= _Scale.y;	m[4*1+2] *= _Scale.y; 	m[4*1+3] *= _Scale.y;
	m[4*2+0] *= _Scale.z;	m[4*2+1] *= _Scale.z;	m[4*2+2] *= _Scale.z; 	m[4*2+3] *= _Scale.z;

	return *this;
}

float4   operator*( const float4& a, const float4x4& b )
{
	float4	R;
	R.x = a.x * b.m[4*0+0] + a.y * b.m[4*1+0] + a.z * b.m[4*2+0] + a.w * b.m[4*3+0];
	R.y = a.x * b.m[4*0+1] + a.y * b.m[4*1+1] + a.z * b.m[4*2+1] + a.w * b.m[4*3+1];
	R.z = a.x * b.m[4*0+2] + a.y * b.m[4*1+2] + a.z * b.m[4*2+2] + a.w * b.m[4*3+2];
	R.w = a.x * b.m[4*0+3] + a.y * b.m[4*1+3] + a.z * b.m[4*2+3] + a.w * b.m[4*3+3];

	return R;
}


float4x4	float4x4::BuildFromQuat( const float4& _Quat )
{
	float4x4	Result;
	float4	q = _Quat;
	q.Normalize();

	float	xs = 2.0f * q.x;
	float	ys = 2.0f * q.y;
	float	zs = 2.0f * q.z;

	float	wx, wy, wz, xx, xy, xz, yy, yz, zz;
	wx = q.w * xs;	wy = q.w * ys;	wz = q.w * zs;
	xx = q.x * xs;	xy = q.x * ys;	xz = q.x * zs;
	yy = q.y * ys;	yz = q.y * zs;	zz = q.z * zs;

	Result.m[4*0+0] = 1.0f - yy - zz;
	Result.m[4*0+1] =        xy + wz;
	Result.m[4*0+2] =        xz - wy;
	Result.m[4*0+3] = 0.0f;

	Result.m[4*1+0] =        xy - wz;
	Result.m[4*1+1] = 1.0f - xx - zz;
	Result.m[4*1+2] =        yz + wx;
	Result.m[4*1+3] = 0.0f;

	Result.m[4*2+0] =        xz + wy;
	Result.m[4*2+1] =        yz - wx;
	Result.m[4*2+2] = 1.0f - xx - yy;
	Result.m[4*2+3] = 0.0f;

	Result.m[4*3+0] = 0.0f;
	Result.m[4*3+1] = 0.0f;
	Result.m[4*3+2] = 0.0f;
	Result.m[4*3+3] = 1.0f;

	return	Result;
}

float4x4&	float4x4::PRS( const float3& P, const float4& R, const float3& S )
{
	return *this = BuildFromPRS( P, R, S );
}

float4x4	float4x4::ProjectionPerspective( float _FOVY, float _AspectRatio, float _Near, float _Far )
{
	float	H = tanf( 0.5f * _FOVY );
	float	W = _AspectRatio * H;
	float	Q =  _Far / (_Far - _Near);

	float4x4	Result;
	Result.SetRow( 0, float4( 1.0f / W, 0.0f, 0.0f, 0.0f ) );
	Result.SetRow( 1, float4( 0.0f, 1.0f / H, 0.0f, 0.0f ) );
	Result.SetRow( 2, float4( 0.0f, 0.0f, Q, 1.0f ) );
	Result.SetRow( 3, float4( 0.0f, 0.0f, -_Near * Q, 0.0f ) );

	return Result;
}

float4x4	float4x4::BuildFromPRS( const float3& P, const float4& R, const float3& S )
{
	float4x4	Result = BuildFromQuat( R );

	Result.m[4*0+0] *= S.x;
	Result.m[4*0+1] *= S.x;
	Result.m[4*0+2] *= S.x;
	Result.m[4*0+3] *= S.x;
	Result.m[4*1+0] *= S.y;
	Result.m[4*1+1] *= S.y;
	Result.m[4*1+2] *= S.y;
	Result.m[4*1+3] *= S.y;
	Result.m[4*2+0] *= S.z;
	Result.m[4*2+1] *= S.z;
	Result.m[4*2+2] *= S.z;
	Result.m[4*2+3] *= S.z;

	Result.m[4*3+0] = P.x;
	Result.m[4*3+1] = P.y;
	Result.m[4*3+2] = P.z;

	return	Result;
}

float4x4	float4x4::Rot( const float3& _Source, const float3& _Target )
{
	float3	Ortho = _Source ^ _Target;
	float		Length = Ortho.Length();
	if ( Length > 1e-6f )
		Ortho = Ortho / Length;
	else
		Ortho.Set( 1, 0, 0 );

	float	Angle = asinf( Length );
	return BuildFromAngleAxis( Angle, Ortho );
}

float4x4	float4x4::RotX( float _Angle )
{
	float4x4	Result = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	Result.m[4*1+1] = C;	Result.m[4*1+2] = S;
	Result.m[4*2+1] = -S;	Result.m[4*2+2] = C;

	return Result;
}
float4x4	float4x4::RotY( float _Angle )
{
	float4x4	Result = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	Result.m[4*0+0] = C;	Result.m[4*0+2] = -S;
	Result.m[4*2+0] = S;	Result.m[4*2+2] = C;

	return Result;
}
float4x4	float4x4::RotZ( float _Angle )
{
	float4x4	Result = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	Result.m[4*0+0] = C;	Result.m[4*0+1] = S;
	Result.m[4*1+0] = -S;	Result.m[4*1+1] = C;

	return Result;
}
float4x4	float4x4::PYR( float _Pitch, float _Yaw, float _Roll )
{
	float4x4	Pitch = RotX( _Pitch );
	float4x4	Yaw = RotY( _Yaw );
	float4x4	Roll = RotZ( _Roll );

	float4x4	Result = Pitch * Yaw * Roll;

	return Result;
}


//////////////////////////////////////////////////////////////////////////
// Half floats encoding
const float	half::SMALLEST = 6.1035156e-005f;	// The smallest encodable float


#define F16_EXPONENT_BITS 0x1F
#define F16_EXPONENT_SHIFT 10
#define F16_EXPONENT_BIAS 15
#define F16_MANTISSA_BITS 0x03ff
#define F16_MANTISSA_SHIFT (23 - F16_EXPONENT_SHIFT)
#define F16_MAX_EXPONENT (F16_EXPONENT_BITS << F16_EXPONENT_SHIFT)

half::half( float value )
{
	U32 f32 = *((U32*) &value);
	raw = 0;

	// Decode IEEE 754 little-endian 32-bit floating-point value
	int sign = (f32 >> 16) & 0x8000;
	// Map exponent to the range [-127,128]
	int exponent = ((f32 >> 23) & 0xff) - 127;
	int mantissa = f32 & 0x007fffff;
	if ( exponent == 128 )
	{   // Infinity or NaN
		raw = U16( sign | F16_MAX_EXPONENT );
		if ( mantissa != 0 ) raw |= (mantissa & F16_MANTISSA_BITS);
	}
	else if ( exponent > 15 )
	{   // Overflow - flush to Infinity
		raw = U16( sign | F16_MAX_EXPONENT );
	}
	else if ( exponent > -15 )
	{   // Representable value
		exponent += F16_EXPONENT_BIAS;
		mantissa >>= F16_MANTISSA_SHIFT;
		raw = U16( sign | exponent << F16_EXPONENT_SHIFT | mantissa );
	}
	else
	{
		raw = U16(sign);
	}
}

half::operator float() const
{
	union 
	{
		float   f;
		U32	 ui;
	} f32;

	int sign = (raw & 0x8000) << 15;
	int exponent = (raw & 0x7c00) >> 10;
	int mantissa = (raw & 0x03ff);

	f32.f = 0.0f;
	if ( exponent == 0 )
	{
		if ( mantissa != 0 ) 
			f32.f = mantissa / float(1 << 24);
	}
	else if ( exponent == 31 )
	{
		f32.ui = sign | 0x7f800000 | mantissa;
	}
	else 
	{
		float scale, decimal;
		exponent -= 15;
		if ( exponent < 0 )
		{
			scale = float( 1.0 / (1 << -exponent) );
		}
		else 
		{
			scale = float( 1 << exponent );
		}
		decimal = 1.0f + (float) mantissa / (1 << 10);
		f32.f = scale * decimal;
	}
	
	if ( sign != 0 )
		f32.f = -f32.f;

	return f32.f;
}

float4x4  float4x4::operator*( const float4x4& b ) const
{
	float4x4  R;

	R.m[4*0+0] = m[4*0+0] * b.m[4*0+0] + m[4*0+1] * b.m[4*1+0] + m[4*0+2] * b.m[4*2+0] + m[4*0+3] * b.m[4*3+0];
	R.m[4*0+1] = m[4*0+0] * b.m[4*0+1] + m[4*0+1] * b.m[4*1+1] + m[4*0+2] * b.m[4*2+1] + m[4*0+3] * b.m[4*3+1];
	R.m[4*0+2] = m[4*0+0] * b.m[4*0+2] + m[4*0+1] * b.m[4*1+2] + m[4*0+2] * b.m[4*2+2] + m[4*0+3] * b.m[4*3+2];
	R.m[4*0+3] = m[4*0+0] * b.m[4*0+3] + m[4*0+1] * b.m[4*1+3] + m[4*0+2] * b.m[4*2+3] + m[4*0+3] * b.m[4*3+3];

	R.m[4*1+0] = m[4*1+0] * b.m[4*0+0] + m[4*1+1] * b.m[4*1+0] + m[4*1+2] * b.m[4*2+0] + m[4*1+3] * b.m[4*3+0];
	R.m[4*1+1] = m[4*1+0] * b.m[4*0+1] + m[4*1+1] * b.m[4*1+1] + m[4*1+2] * b.m[4*2+1] + m[4*1+3] * b.m[4*3+1];
	R.m[4*1+2] = m[4*1+0] * b.m[4*0+2] + m[4*1+1] * b.m[4*1+2] + m[4*1+2] * b.m[4*2+2] + m[4*1+3] * b.m[4*3+2];
	R.m[4*1+3] = m[4*1+0] * b.m[4*0+3] + m[4*1+1] * b.m[4*1+3] + m[4*1+2] * b.m[4*2+3] + m[4*1+3] * b.m[4*3+3];

	R.m[4*2+0] = m[4*2+0] * b.m[4*0+0] + m[4*2+1] * b.m[4*1+0] + m[4*2+2] * b.m[4*2+0] + m[4*2+3] * b.m[4*3+0];
	R.m[4*2+1] = m[4*2+0] * b.m[4*0+1] + m[4*2+1] * b.m[4*1+1] + m[4*2+2] * b.m[4*2+1] + m[4*2+3] * b.m[4*3+1];
	R.m[4*2+2] = m[4*2+0] * b.m[4*0+2] + m[4*2+1] * b.m[4*1+2] + m[4*2+2] * b.m[4*2+2] + m[4*2+3] * b.m[4*3+2];
	R.m[4*2+3] = m[4*2+0] * b.m[4*0+3] + m[4*2+1] * b.m[4*1+3] + m[4*2+2] * b.m[4*2+3] + m[4*2+3] * b.m[4*3+3];

	R.m[4*3+0] = m[4*3+0] * b.m[4*0+0] + m[4*3+1] * b.m[4*1+0] + m[4*3+2] * b.m[4*2+0] + m[4*3+3] * b.m[4*3+0];
	R.m[4*3+1] = m[4*3+0] * b.m[4*0+1] + m[4*3+1] * b.m[4*1+1] + m[4*3+2] * b.m[4*2+1] + m[4*3+3] * b.m[4*3+1];
	R.m[4*3+2] = m[4*3+0] * b.m[4*0+2] + m[4*3+1] * b.m[4*1+2] + m[4*3+2] * b.m[4*2+2] + m[4*3+3] * b.m[4*3+2];
	R.m[4*3+3] = m[4*3+0] * b.m[4*0+3] + m[4*3+1] * b.m[4*1+3] + m[4*3+2] * b.m[4*2+3] + m[4*3+3] * b.m[4*3+3];

	return R;
}

float&	float4x4::operator()( int _Row, int _Column )
{
	float4*	pRow = 0;
	switch ( _Row&3 )
	{
	case 0: pRow = (float4*) &m[4*0]; break;
	case 1: pRow = (float4*) &m[4*1]; break;
	case 2: pRow = (float4*) &m[4*2]; break;
	case 3: pRow = (float4*) &m[4*3]; break;
	}

	switch ( _Column&3 )
	{
	case 0: return pRow->x;
	case 1: return pRow->y;
	case 2: return pRow->z;
	case 3: return pRow->w;
	}
	return *((float*) 0);
}
