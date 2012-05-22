#include "../../GodComplex.h"

const NjFloat2	NjFloat2::Zero( 0, 0 );
const NjFloat2	NjFloat2::One( 1, 1 );
const NjFloat2	NjFloat2::UnitX( 1, 0 );
const NjFloat2	NjFloat2::UnitY( 0, 1 );

const NjFloat3	NjFloat3::Zero( 0, 0, 0 );
const NjFloat3	NjFloat3::One( 1, 1, 1 );
const NjFloat3	NjFloat3::UnitX( 1, 0, 0 );
const NjFloat3	NjFloat3::UnitY( 0, 1, 0 );
const NjFloat3	NjFloat3::UnitZ( 0, 0, 1 );

const NjFloat4	NjFloat4::Zero( 0, 0, 0, 0 );
const NjFloat4	NjFloat4::One( 1, 1, 1, 1 );
const NjFloat4	NjFloat4::UnitX( 1, 0, 0, 0 );
const NjFloat4	NjFloat4::UnitY( 0, 1, 0, 0 );
const NjFloat4	NjFloat4::UnitZ( 0, 0, 1, 0 );
const NjFloat4	NjFloat4::UnitW( 0, 0, 1, 1 );

NjFloat4x4  NjFloat4x4::Inverse() const
{
	float	Det = Determinant();
	ASSERT( abs(Det) > 1e-6f );

	Det = 1.0f / Det;

	NjFloat4x4  Temp;
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

float	   NjFloat4x4::Determinant() const
{
	return m[0] * CoFactor( 0, 0 ) + m[1] * CoFactor( 0, 1 ) + m[2] * CoFactor( 0, 2 ) + m[3] * CoFactor( 0, 3 ); 
}

float	   NjFloat4x4::CoFactor( int x, int y ) const
{
	static int  NextIndex[7] = { 1, 2, 3, 0, 1, 2, 3 };

	return	((	m[4*NextIndex[x+1]+NextIndex[y+1]]*m[4*NextIndex[x+2]+NextIndex[y+2]]*m[4*NextIndex[x+3]+NextIndex[y+3]] +
				m[4*NextIndex[x+1]+NextIndex[y+2]]*m[4*NextIndex[x+2]+NextIndex[y+3]]*m[4*NextIndex[x+3]+NextIndex[y+1]] +
				m[4*NextIndex[x+1]+NextIndex[y+3]]*m[4*NextIndex[x+2]+NextIndex[y+1]]*m[4*NextIndex[x+3]+NextIndex[y+2]] )

			-(	m[4*NextIndex[x+3]+NextIndex[y+1]]*m[4*NextIndex[x+2]+NextIndex[y+2]]*m[4*NextIndex[x+1]+NextIndex[y+3]] +
				m[4*NextIndex[x+3]+NextIndex[y+2]]*m[4*NextIndex[x+2]+NextIndex[y+3]]*m[4*NextIndex[x+1]+NextIndex[y+1]] +
				m[4*NextIndex[x+3]+NextIndex[y+3]]*m[4*NextIndex[x+2]+NextIndex[y+1]]*m[4*NextIndex[x+1]+NextIndex[y+2]] ))
			* (((x + y) & 1) == 1 ? -1.0f : +1.0f);
}

NjFloat4   operator*( const NjFloat4& a, const NjFloat4x4& b )
{
	NjFloat4	R;
	R.x = a.x * b.m[4*0+0] + a.y * b.m[4*1+0] + a.z * b.m[4*2+0] + a.w * b.m[4*3+0];
	R.y = a.x * b.m[4*0+1] + a.y * b.m[4*1+1] + a.z * b.m[4*2+1] + a.w * b.m[4*3+1];
	R.z = a.x * b.m[4*0+2] + a.y * b.m[4*1+2] + a.z * b.m[4*2+2] + a.w * b.m[4*3+2];
	R.w = a.x * b.m[4*0+3] + a.y * b.m[4*1+3] + a.z * b.m[4*2+3] + a.w * b.m[4*3+3];

	return R;
}


#define F16_EXPONENT_BITS 0x1F
#define F16_EXPONENT_SHIFT 10
#define F16_EXPONENT_BIAS 15
#define F16_MANTISSA_BITS 0x3ff
#define F16_MANTISSA_SHIFT (23 - F16_EXPONENT_SHIFT)
#define F16_MAX_EXPONENT (F16_EXPONENT_BITS << F16_EXPONENT_SHIFT)

NjHalf::NjHalf( float value )
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

NjHalf::operator float() const
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

NjFloat4x4  NjFloat4x4::operator*( const NjFloat4x4& b ) const
{
	NjFloat4x4  R;

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
