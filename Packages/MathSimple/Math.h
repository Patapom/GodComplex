//////////////////////////////////////////////////////////////////////////
// Contains:
//	• Single precision floating-point version of the System.Math class
//	• Float16 (half-precision float) implementation
//
#pragma once

using namespace System;

namespace SharpMath {

	//////////////////////////////////////////////////////////////////////////
	// Floating-point version of the System.Math class + additional constants + some helpers
	public ref class Mathf {
	public:

		// Standard constants
		literal float		E = 2.7182818284590452353602874713527f;				// ??
		literal float		PI = 3.1415926535897932384626433832795f;			// ??
		literal float		TWOPI = 6.283185307179586476925286766559f;			// 2PI
		literal float		FOURPI = 12.566370614359172953850573533118f;		// 4PI
		literal float		HALFPI = 1.5707963267948966192313216916398f;		// PI/2
		literal float		INVPI = 0.31830988618379067153776752674503f;		// 1/PI
		literal float		INV2PI = 0.15915494309189533576888376337251f;		// 1/(2PI)
		literal float		INV4PI = 0.07957747154594766788444188168626f;		// 1/(4PI)
		literal float		GOLDEN_RATIO = 1.6180339887498948482045868343656f;	// Phi = (1+sqrt(5)) / 2
		literal float		ALMOST_EPSILON = 1e-6f;

		// Regular functions
		static Decimal			Abs( Decimal v )									{ return Math::Abs( v ); }
		static double			Abs( double v )										{ return Math::Abs( v ); }
		static float			Abs( float v )										{ return Math::Abs( v ); }
		static long				Abs( long v )										{ return Math::Abs( v ); }
		static int				Abs( int v )										{ return Math::Abs( v ); }
		static short			Abs( short v )										{ return Math::Abs( v ); }
		static SByte			Abs( SByte v )										{ return Math::Abs( v ); }
		static float			Acos( float v )										{ return (float) Math::Acos( v ); }
		static float			Asin( float v )										{ return (float) Math::Asin( v ); }
		static float			Atan( float v )										{ return (float) Math::Atan( v ); }
		static float			Atan2( float y, float x )							{ return (float) Math::Atan2( y, x ); }
		static float			Ceiling( float v )									{ return (float) Math::Ceiling( v ); }
		static float			Cos( float v )										{ return (float) Math::Cos( v ); }
		static float			Cosh( float v )										{ return (float) Math::Cosh( v ); }
		static float			Exp( float v )										{ return (float) Math::Exp( v ); }
		static float			Exp2( float v )										{ return (float) Math::Pow( 2.0f, v ); }
		static float			Floor( float v )									{ return (float) Math::Floor( v ); }
		static float			Log( float v )										{ return (float) Math::Log( v ); }
		static float			Log( float v, float _newBase )						{ return (float) Math::Log( v, _newBase ); }
		static float			Log10( float v )									{ return (float) Math::Log10( v ); }
		static Decimal			Max( Decimal a, Decimal b )							{ return Math::Max( a, b ); }
		static double			Max( double a, double b )							{ return Math::Max( a, b ); }
		static float			Max( float a, float b )								{ return Math::Max( a, b ); }
		static long				Max( long a, long b )								{ return Math::Max( a, b ); }
		static int				Max( int a, int b )									{ return Math::Max( a, b ); }
		static short			Max( short a, short b )								{ return Math::Max( a, b ); }
		static SByte			Max( SByte a, SByte b )								{ return Math::Max( a, b ); }
		static Decimal			Min( Decimal a, Decimal b )							{ return Math::Min( a, b ); }
		static double			Min( double a, double b )							{ return Math::Min( a, b ); }
		static float			Min( float a, float b )								{ return Math::Min( a, b ); }
		static long				Min( long a, long b )								{ return Math::Min( a, b ); }
		static int				Min( int a, int b )									{ return Math::Min( a, b ); }
		static short			Min( short a, short b )								{ return Math::Min( a, b ); }
		static SByte			Min( SByte a, SByte b )								{ return Math::Min( a, b ); }
		static float			Pow( float a, float b )								{ return (float) Math::Pow( a, b ); }
		static float			Round( float a, int _digits, System::MidpointRounding _mode )	{ return (float) Math::Round( a, _digits, _mode ); }
		static float			Round( float a, System::MidpointRounding _mode )	{ return (float) Math::Round( a, _mode ); }
		static float			Round( float a, int _digits )						{ return (float) Math::Round( a, _digits ); }
		static float			Round( float a )									{ return (float) Math::Round( a ); }
		static int				Sign( Decimal v )									{ return Math::Sign( v ); }
		static int				Sign( double v )									{ return Math::Sign( v ); }
		static int				Sign( float v )										{ return Math::Sign( v ); }
		static int				Sign( long v )										{ return Math::Sign( v ); }
		static int				Sign( int v )										{ return Math::Sign( v ); }
		static int				Sign( short v )										{ return Math::Sign( v ); }
		static int				Sign( SByte v )										{ return Math::Sign( v ); }
		static float			Sin( float v )										{ return (float) Math::Sin( v ); }
		static float			Sinh( float v )										{ return (float) Math::Sinh( v ); }
		static float			Sqrt( float v )										{ return (float) Math::Sqrt( v ); }
		static float			Tan( float v )										{ return (float) Math::Tan( v ); }
		static float			Tanh( float v )										{ return (float) Math::Tanh( v ); }
		static float			Truncate( float v )									{ return (float) Math::Truncate( v ); }

		//////////////////////////////////////////////////////////////////////////
		// Additional helpers
		static float			Clamp( float x, float min, float max )				{ return x < min ? min : (x > max ? max : x); }
		static float			Lerp( float a, float b, float t )					{ return a * (1.0f - t) + b * t; }
		static float			Saturate( float x )									{ return x < 0.0f ? 0.0f : (x > 1.0f ? 1.0f : x); }
		static float			Sqr( float a )										{ return a * a;  }
		static bool				Almost( float a, float b )							{ return Almost( a, b, ALMOST_EPSILON ); }
		static bool				Almost( float a, float b, float _epsilon )			{ return Abs( a - b ) < _epsilon; }
		static bool				Almost( double a, double b )						{ return Almost( a, b, double(ALMOST_EPSILON) ); }
		static bool				Almost( double a, double b, double _epsilon )		{ return Abs( a - b ) < _epsilon; }
		static float			ToDeg( float _radians )								{ return 180.0f * _radians * INVPI; }
		static float			ToRad( float _degrees )								{ return PI * _degrees / 180.0f; }
	};


	//////////////////////////////////////////////////////////////////////////
	// Float16
	#define F16_EXPONENT_BITS	0x1F
	#define F16_EXPONENT_SHIFT	10
	#define F16_EXPONENT_BIAS	15
	#define F16_MANTISSA_BITS	0x03ff
	#define F16_MANTISSA_SHIFT	(23 - F16_EXPONENT_SHIFT)
	#define F16_MAX_EXPONENT	(F16_EXPONENT_BITS << F16_EXPONENT_SHIFT)

	[System::Diagnostics::DebuggerDisplayAttribute( "{value}" )]
	public value class   half {
	public:
		static const UInt16	SMALLEST_UINT = 0x0400;
		static const float	SMALLEST = 6.1035156e-005f;	// The smallest encodable float

		UInt16			raw;
		property float	value	{ float get() { return ((float) *this); } }

		half( float value ) {
			UInt32 f32 = *((UInt32*) &value);
			raw = 0;

			// Decode IEEE 754 little-endian 32-bit floating-point value
			int sign = (f32 >> 16) & 0x8000;
			// Map exponent to the range [-127,128]
			int exponent = ((f32 >> 23) & 0xff) - 127;
			int mantissa = f32 & 0x007fffff;
			if ( exponent == 128 ) {
			   // Infinity or NaN
				raw = UInt16( sign | F16_MAX_EXPONENT );
				if ( mantissa != 0 ) raw |= (mantissa & F16_MANTISSA_BITS);
			} else if ( exponent > 15 ) {
			   // Overflow - flush to Infinity
				raw = UInt16( sign | F16_MAX_EXPONENT );
			} else if ( exponent > -15 ) {
			   // Representable value
				exponent += F16_EXPONENT_BIAS;
				mantissa >>= F16_MANTISSA_SHIFT;
				raw = UInt16( sign | exponent << F16_EXPONENT_SHIFT | mantissa );
			} else {
				raw = UInt16(sign);
			}
		}

		static operator float( half _value ) {
			union 
			{
				float	f;
				UInt32	ui;
			} f32;

			int sign = (_value.raw & 0x8000) << 15;
			int exponent = (_value.raw & 0x7c00) >> 10;
			int mantissa = (_value.raw & 0x03ff);

			f32.f = 0.0f;
			if ( exponent == 0 ) {
				if ( mantissa != 0 ) 
					f32.f = mantissa / float(1 << 24);
			} else if ( exponent == 31 ) {
				f32.ui = sign | 0x7f800000 | mantissa;
			} else {
				float scale, decimal;
				exponent -= 15;
				if ( exponent < 0 )
					scale = float( 1.0 / (1 << -exponent) );
				else 
					scale = float( 1 << exponent );
				decimal = 1.0f + (float) mantissa / (1 << 10);
				f32.f = scale * decimal;
			}
	
			if ( sign != 0 )
				f32.f = -f32.f;

			return f32.f;
		}
	};
}
