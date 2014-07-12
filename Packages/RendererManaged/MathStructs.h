// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace RendererManaged
{
	[System::Diagnostics::DebuggerDisplayAttribute( "{x}, {y}" )]
	public value struct	float2
	{
	public:
		float	x, y;
		float2( float _x, float _y )				{ Set( _x, _y ); }
		void	Set( float _x, float _y )			{ x = _x; y = _y; }
		void	FromVector2( WMath::Vector2D^ a )	{ Set( a->x, a->y ); }
	};

	[System::Diagnostics::DebuggerDisplayAttribute( "{x}, {y}, {z}" )]
	public value struct	float3
	{
	public:
		float	x, y, z;
		float3( float _x, float _y, float _z )		{ Set( _x, _y, _z ); }
		float3( System::Drawing::Color^ _Color )	{ Set( _Color->R / 255.0f, _Color->G / 255.0f, _Color->B / 255.0f ); }
		void	Set( float _x, float _y, float _z )	{ x = _x; y = _y; z = _z; }
		void	FromVector3( WMath::Vector^ a )		{ Set( a->x, a->y, a->z ); }

		static float3	operator+( float3 a, float3 b )	{ return float3( a.x+b.x, a.y+b.y, a.z+b.z ); }
		static float3	operator-( float3 a, float3 b )	{ return float3( a.x-b.x, a.y-b.y, a.z-b.z ); }
		static float3	operator*( float a, float3 b )	{ return float3( a*b.x, a*b.y, a*b.z ); }
		static float3	operator/( float3 a, float b )	{ return float3( a.x/b, a.y/b, a.z/b ); }

		property float	Length
		{
			float	get() { return (float) Math::Sqrt( x*x + y*y + z*z ); }
		}

		property float3	Normalized	{
			float3	get()
			{
				float	InvLength = 1.0f / Length;
				return float3( InvLength * x, InvLength * y, InvLength * z );
			}
		}

		float	Dot( float3 b )	{ return x*b.x + y*b.y + z*b.z; }

		float3	Cross( float3 b )
		{
			return float3(	y * b.z - z * b.y,
							z * b.x - x * b.z,
							x * b.y - y * b.x );
		}
	};

	[System::Diagnostics::DebuggerDisplayAttribute( "{x}, {y}, {z}, {w}" )]
	public value struct	float4
	{
	public:
		float	x, y, z, w;
		float4( float _x, float _y, float _z, float _w )		{ Set( _x, _y, _z, _w ); }
		float4( float3 _xyz, float _w )							{ Set( _xyz.x, _xyz.y, _xyz.z, _w ); }
		float4( System::Drawing::Color^ _Color, float _Alpha )	{ Set( _Color->R / 255.0f, _Color->G / 255.0f, _Color->B / 255.0f, _Alpha ); }
		void	Set( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
		void	FromVector4( WMath::Vector4D^ a )				{ Set( a->x, a->y, a->z, a->w ); }

		static float4	operator+( float4 a, float4 b )	{ return float4( a.x+b.x, a.y+b.y, a.z+b.z, a.w+b.w ); }
		static float4	operator-( float4 a, float4 b )	{ return float4( a.x-b.x, a.y-b.y, a.z-b.z, a.w-b.w ); }
		static float4	operator*( float a, float4 b )	{ return float4( a*b.x, a*b.y, a*b.z, a*b.w ); }

// 		float	operator[]( int _ComponentIndex )
// 		{
// 			switch ( _ComponentIndex&3 )
// 			{
// 			case 0: return x;
// 			case 1: return y;
// 			case 2: return z;
// 			case 3: return w;
// 			}
// 			return x;
// 		}

		property float	default[int]
		{
			float	get( int _ComponentIndex )
			{
				switch ( _ComponentIndex&3 )
				{
				case 0: return x;
				case 1: return y;
				case 2: return z;
				case 3: return w;
				}
				return x;
			}
			void	set( int _ComponentIndex, float value )
			{
				switch ( _ComponentIndex&3 )
				{
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				case 3: w = value; break;
				}
			}
		}

		float	Dot( float4 b )	{ return x*b.x + y*b.y + z*b.z + w*b.w; }

		property float	Length
		{
			float	get() { return (float) Math::Sqrt( x*x + y*y + z*z ); }
		}

		property float4	Normalized	{
			float4	get()
			{
				float	InvLength = 1.0f / Length;
				return float4( InvLength * x, InvLength * y, InvLength * z, InvLength * w );
			}
		}
	};

	public value struct	float4x4
	{
	public:
		float4	r0;
		float4	r1;
		float4	r2;
		float4	r3;

		float4x4( cli::array<float>^ _values )
		{
			r0.Set( _values[4*0+0], _values[4*0+1], _values[4*0+2], _values[4*0+3] );
			r1.Set( _values[4*1+0], _values[4*1+1], _values[4*1+2], _values[4*1+3] );
			r2.Set( _values[4*2+0], _values[4*2+1], _values[4*2+2], _values[4*2+3] );
			r3.Set( _values[4*3+0], _values[4*3+1], _values[4*3+2], _values[4*3+3] );
		}
		float4x4( float4^ _r0, float4^ _r1, float4^ _r2, float4^ _r3 )	{ r0 = *_r0; r1 = *_r1; r2 = *_r2; r3 = *_r3; }
		void	FromMatrix4( WMath::Matrix4x4^ a )	{ r0.FromVector4( a->GetRow0() ); r1.FromVector4( a->GetRow1() ); r2.FromVector4( a->GetRow2() ); r3.FromVector4( a->GetRow3() ); }

		float4x4	MakeLookAt( float3 _Position, float3 _Target, float3 _Up )
		{
			float3	At = (_Target - _Position).Normalized;	// We want Z to point toward target
			float3	Right = At.Cross( _Up ).Normalized;		// We want X to point to the right
			float3	Up = Right.Cross( At );					// We want Y to point upward

			r0.Set( Right.x, Right.y, Right.z, 0.0f );
			r1.Set( Up.x, Up.y, Up.z, 0.0f );
			r2.Set( At.x, At.y, At.z, 0.0f );
			r3.Set( _Position.x, _Position.y, _Position.z, 1.0f );

			return *this;
		}
	
		float4x4	MakeProjectionPerspective( float _FOVY, float _AspectRatio, float _Near, float _Far )
		{
			float	H = (float) Math::Tan( 0.5f * _FOVY );
			float	W = _AspectRatio * H;
			float	Q =  _Far / (_Far - _Near);

			r0.Set( 1.0f / W, 0.0f, 0.0f, 0.0f );
			r1.Set( 0.0f, 1.0f / H, 0.0f, 0.0f );
			r2.Set( 0.0f, 0.0f, Q, 1.0f );
			r3.Set( 0.0f, 0.0f, -_Near * Q, 0.0f );

			return *this;
		}

// 		float4x4	operator*( float4x4 b )
// 		{
// 			float4x4	R;
// 			R.r0.Set( r0.x*b.r0.x + r0.y*b.r1.x + r0.z*b.r2.x + r0.w*b.r3.x, /**/ r0.x*b.r0.y + r0.y*b.r1.y + r0.z*b.r2.y + r0.w*b.r3.y, /**/ r0.x*b.r0.z + r0.y*b.r1.z + r0.z*b.r2.z + r0.w*b.r3.z, /**/ r0.x*b.r0.w + r0.y*b.r1.w + r0.z*b.r2.w + r0.w*b.r3.w );
// 			R.r1.Set( r1.x*b.r0.x + r1.y*b.r1.x + r1.z*b.r2.x + r1.w*b.r3.x, /**/ r1.x*b.r0.y + r1.y*b.r1.y + r1.z*b.r2.y + r1.w*b.r3.y, /**/ r1.x*b.r0.z + r1.y*b.r1.z + r1.z*b.r2.z + r1.w*b.r3.z, /**/ r1.x*b.r0.w + r1.y*b.r1.w + r1.z*b.r2.w + r1.w*b.r3.w );
//			R.r2.Set( r2.x*b.r0.x + r2.y*b.r1.x + r2.z*b.r2.x + r2.w*b.r3.x, /**/ r2.x*b.r0.y + r2.y*b.r1.y + r2.z*b.r2.y + r2.w*b.r3.y, /**/ r2.x*b.r0.z + r2.y*b.r1.z + r2.z*b.r2.z + r2.w*b.r3.z, /**/ r2.x*b.r0.w + r2.y*b.r1.w + r2.z*b.r2.w + r2.w*b.r3.w );
// 			R.r3.Set( r3.x*b.r0.x + r3.y*b.r1.x + r3.z*b.r2.x + r3.w*b.r3.x, /**/ r3.x*b.r0.y + r3.y*b.r1.y + r3.z*b.r2.y + r3.w*b.r3.y, /**/ r3.x*b.r0.z + r3.y*b.r1.z + r3.z*b.r2.z + r3.w*b.r3.z, /**/ r3.x*b.r0.w + r3.y*b.r1.w + r3.z*b.r2.w + r3.w*b.r3.w );
// 
// 			return R;
// 		}
 
		static float4x4	operator*( float4x4 a, float4x4 b )
		{
			float4x4	R;
			R.r0.Set( a.r0.x*b.r0.x + a.r0.y*b.r1.x + a.r0.z*b.r2.x + a.r0.w*b.r3.x, /**/ a.r0.x*b.r0.y + a.r0.y*b.r1.y + a.r0.z*b.r2.y + a.r0.w*b.r3.y, /**/ a.r0.x*b.r0.z + a.r0.y*b.r1.z + a.r0.z*b.r2.z + a.r0.w*b.r3.z, /**/ a.r0.x*b.r0.w + a.r0.y*b.r1.w + a.r0.z*b.r2.w + a.r0.w*b.r3.w );
			R.r1.Set( a.r1.x*b.r0.x + a.r1.y*b.r1.x + a.r1.z*b.r2.x + a.r1.w*b.r3.x, /**/ a.r1.x*b.r0.y + a.r1.y*b.r1.y + a.r1.z*b.r2.y + a.r1.w*b.r3.y, /**/ a.r1.x*b.r0.z + a.r1.y*b.r1.z + a.r1.z*b.r2.z + a.r1.w*b.r3.z, /**/ a.r1.x*b.r0.w + a.r1.y*b.r1.w + a.r1.z*b.r2.w + a.r1.w*b.r3.w );
			R.r2.Set( a.r2.x*b.r0.x + a.r2.y*b.r1.x + a.r2.z*b.r2.x + a.r2.w*b.r3.x, /**/ a.r2.x*b.r0.y + a.r2.y*b.r1.y + a.r2.z*b.r2.y + a.r2.w*b.r3.y, /**/ a.r2.x*b.r0.z + a.r2.y*b.r1.z + a.r2.z*b.r2.z + a.r2.w*b.r3.z, /**/ a.r2.x*b.r0.w + a.r2.y*b.r1.w + a.r2.z*b.r2.w + a.r2.w*b.r3.w );
			R.r3.Set( a.r3.x*b.r0.x + a.r3.y*b.r1.x + a.r3.z*b.r2.x + a.r3.w*b.r3.x, /**/ a.r3.x*b.r0.y + a.r3.y*b.r1.y + a.r3.z*b.r2.y + a.r3.w*b.r3.y, /**/ a.r3.x*b.r0.z + a.r3.y*b.r1.z + a.r3.z*b.r2.z + a.r3.w*b.r3.z, /**/ a.r3.x*b.r0.w + a.r3.y*b.r1.w + a.r3.z*b.r2.w + a.r3.w*b.r3.w );

			return R;
		}

		static float4x4	operator*( float a, float4x4 b )
		{
			float4x4	R;
			R.r0.Set( a*b.r0.x + a*b.r1.x + a*b.r2.x + a*b.r3.x, /**/ a*b.r0.y + a*b.r1.y + a*b.r2.y + a*b.r3.y, /**/ a*b.r0.z + a*b.r1.z + a*b.r2.z + a*b.r3.z, /**/ a*b.r0.w + a*b.r1.w + a*b.r2.w + a*b.r3.w );
			R.r1.Set( a*b.r0.x + a*b.r1.x + a*b.r2.x + a*b.r3.x, /**/ a*b.r0.y + a*b.r1.y + a*b.r2.y + a*b.r3.y, /**/ a*b.r0.z + a*b.r1.z + a*b.r2.z + a*b.r3.z, /**/ a*b.r0.w + a*b.r1.w + a*b.r2.w + a*b.r3.w );
			R.r2.Set( a*b.r0.x + a*b.r1.x + a*b.r2.x + a*b.r3.x, /**/ a*b.r0.y + a*b.r1.y + a*b.r2.y + a*b.r3.y, /**/ a*b.r0.z + a*b.r1.z + a*b.r2.z + a*b.r3.z, /**/ a*b.r0.w + a*b.r1.w + a*b.r2.w + a*b.r3.w );
			R.r3.Set( a*b.r0.x + a*b.r1.x + a*b.r2.x + a*b.r3.x, /**/ a*b.r0.y + a*b.r1.y + a*b.r2.y + a*b.r3.y, /**/ a*b.r0.z + a*b.r1.z + a*b.r2.z + a*b.r3.z, /**/ a*b.r0.w + a*b.r1.w + a*b.r2.w + a*b.r3.w );

			return R;
		}

// 		float4		operator[]( int _RowIndex )
// 		{
// 			return GetRow( _RowIndex );
// 		}

		property float4	default[int]
		{
			float4	get( int _RowIndex )				{ return GetRow( _RowIndex ); }
			void	set( int _RowIndex, float4 value )	{ SetRow( _RowIndex, value ); }
		}

		float4	GetRow( int _RowIndex )
		{
			switch ( _RowIndex & 3 )
			{
			case 0: return r0;
			case 1: return r1;
			case 2: return r2;
			case 3: return r3;
			}
			return r0;
		}

		void	SetRow( int _RowIndex, float4 _Value )
		{
			switch ( _RowIndex & 3 )
			{
			case 0: r0 = _Value;
			case 1: r1 = _Value;
			case 2: r2 = _Value;
			case 3: r3 = _Value;
			}
		}

		float	CoFactor( int _dwRow, int _dwCol )
		{
			return	((	GetRow(_dwRow+1)[_dwCol+1]*GetRow(_dwRow+2)[_dwCol+2]*GetRow(_dwRow+3)[_dwCol+3] +
						GetRow(_dwRow+1)[_dwCol+2]*GetRow(_dwRow+2)[_dwCol+3]*GetRow(_dwRow+3)[_dwCol+1] +
						GetRow(_dwRow+1)[_dwCol+3]*GetRow(_dwRow+2)[_dwCol+1]*GetRow(_dwRow+3)[_dwCol+2] )

					-(	GetRow(_dwRow+3)[_dwCol+1]*GetRow(_dwRow+2)[_dwCol+2]*GetRow(_dwRow+1)[_dwCol+3] +
						GetRow(_dwRow+3)[_dwCol+2]*GetRow(_dwRow+2)[_dwCol+3]*GetRow(_dwRow+1)[_dwCol+1] +
						GetRow(_dwRow+3)[_dwCol+3]*GetRow(_dwRow+2)[_dwCol+1]*GetRow(_dwRow+1)[_dwCol+2] ))
					* (((_dwRow + _dwCol) & 1) == 1 ? -1.0f : +1.0f);
		}

		float	Determinant()
		{
			return GetRow(0)[0] * CoFactor( 0, 0 ) + GetRow(0)[1] * CoFactor( 0, 1 ) + GetRow(0)[2] * CoFactor( 0, 2 ) + GetRow(0)[3] * CoFactor( 0, 3 );
		}

		property float4x4		Inverse
		{
			float4x4	get()
			{
				float	fDet = Determinant();
				if ( (float) Math::Abs(fDet) < Single::Epsilon )
					throw gcnew Exception( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

				float	fIDet = 1.0f / fDet;

				float4x4	R;
				R.SetRow( 0, float4( CoFactor( 0, 0 ) * fIDet, CoFactor( 1, 0 ) * fIDet, CoFactor( 2, 0 ) * fIDet, CoFactor( 3, 0 ) * fIDet ) );
				R.SetRow( 1, float4( CoFactor( 0, 1 ) * fIDet, CoFactor( 1, 1 ) * fIDet, CoFactor( 2, 1 ) * fIDet, CoFactor( 3, 1 ) * fIDet ) );
				R.SetRow( 2, float4( CoFactor( 0, 2 ) * fIDet, CoFactor( 1, 2 ) * fIDet, CoFactor( 2, 2 ) * fIDet, CoFactor( 3, 2 ) * fIDet ) );
				R.SetRow( 3, float4( CoFactor( 0, 3 ) * fIDet, CoFactor( 1, 3 ) * fIDet, CoFactor( 2, 3 ) * fIDet, CoFactor( 3, 3 ) * fIDet ) );

				return	R;
			}
		}

		static property float4x4	Identity
		{
			float4x4	get() {
				return float4x4( gcnew cli::array<float>( 16 ) { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 } );
			}
		}


		static float4x4	RotationX( float _Angle )
		{
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			float4x4	R = Identity;
			R[1][1] = C;	R[1][2] = S;
			R[2][1] = -S;	R[2][2] = C;

			return R;
		}
		static float4x4	RotationY( float _Angle )
		{
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			float4x4	R = Identity;
			R[0][0] = C;	R[0][2] = -S;
			R[2][0] = S;	R[2][2] = C;

			return R;
		}
		static float4x4	RotationZ( float _Angle )
		{
			float C = (float) Math::Cos( _Angle );
			float S = (float) Math::Sin( _Angle );

			float4x4	R = Identity;
			R[0][0] = C;	R[0][1] = S;
			R[1][0] = -S;	R[1][1] = C;

			return R;
		}

		/// <summary>
		/// Converts an angle+axis into a plain rotation matrix
		/// </summary>
		/// <param name="_Angle"></param>
		/// <param name="_Axis"></param>
		/// <returns></returns>
		static float4x4	FromAngleAxis( float _Angle, float3 _Axis )
		{
			// Convert into a quaternion
			float3	qv = (float) Math::Sin( 0.5f * _Angle ) * _Axis;
			float	qs = (float) Math::Cos( 0.5f * _Angle );

			// Then into a matrix
			float	xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;

			xs = 2.0f * qv.x;	ys = 2.0f * qv.y;	zs = 2.0f * qv.z;

			wx = qs * xs;		wy = qs * ys;		wz = qs * zs;
			xx = qv.x * xs;	xy = qv.x * ys;	xz = qv.x * zs;
			yy = qv.y * ys;	yz = qv.y * zs;	zz = qv.z * zs;

			float4x4	R;
			R.r0 = float4( 1.0f -	yy - zz,		xy + wz,		xz - wy, 0.0f );
			R.r1 = float4(			xy - wz, 1.0f -	xx - zz,		yz + wx, 0.0f );
			R.r2 = float4(			xz + wy,		yz - wx, 1.0f -	xx - yy, 0.0f );
			R.r3 = float4( 0, 0, 0, 1 );

			return	R;
		}
	};
}
