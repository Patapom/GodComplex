using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ShaderInterpreter.ShaderMath
{
	public class	ShaderSyntaxSupport
	{
		public class	float2
		{
			public float	x, y;

			#region Swizzles
			public float2	xx	{ get { return new float2( x, x ); } }
			public float2	xy	{ get { return new float2( x, y ); } }
			public float2	yy	{ get { return new float2( y, y ); } }
			#endregion

			public float2()							{}
			public float2( float _x )				{ x = y = _x; }
			public float2( float _x, float _y )		{ x = _x; y = _y; }

			public static float2	operator+( float a, float2 b )	{ return new float2( a + b.x, a + b.y ); }
			public static float2	operator+( float2 a, float b )	{ return new float2( a.x + b, a.y + b ); }
			public static float2	operator+( float2 a, float2 b )	{ return new float2( a.x + b.x, a.y + b.y ); }
			public static float2	operator-( float a, float2 b )	{ return new float2( a - b.x, a - b.y ); }
			public static float2	operator-( float2 a, float b )	{ return new float2( a.x - b, a.y - b ); }
			public static float2	operator-( float2 a, float2 b )	{ return new float2( a.x - b.x, a.y - b.y ); }
			public static float2	operator*( float a, float2 b )	{ return new float2( a * b.x, a * b.y ); }
			public static float2	operator*( float2 a, float b )	{ return new float2( a.x * b, a.y * b ); }
			public static float2	operator*( float2 a, float2 b )	{ return new float2( a.x * b.x, a.y * b.y ); }
			public static float2	operator/( float2 a, float b )	{ return new float2( a.x / b, a.y / b ); }
		}

		public class	float3
		{
			public float	x, y, z;

			#region Swizzles
			public float3	xxx	{ get { return new float3( x, x, x ); } }
			public float3	xxy	{ get { return new float3( x, x, y ); } }
			public float3	xyx	{ get { return new float3( x, y, x ); } }
			public float3	yxx	{ get { return new float3( y, x, x ); } }
			public float3	yxy	{ get { return new float3( y, x, y ); } }
			public float3	yyx	{ get { return new float3( y, y, x ); } }
			public float3	yyy	{ get { return new float3( y, y, y ); } }
			#endregion

			public float3()										{}
			public float3( float _x )							{ x = y = z = _x; }
			public float3( float _x, float _y, float _z )		{ x = _x; y = _y; z = _z; }
			public float3( float2 _xy, float _z )				{ x = _xy.x; y = _xy.y; z = _z; }
			public float3( float _x, float2 _yz )				{ x = _x; y = _yz.x; z = _yz.y; }

			public static implicit operator float2( float3 a )		{ return new float2( a.x, a.y ); }

			public static float3	operator+( float a, float3 b )	{ return new float3( a + b.x, a + b.y, a + b.z ); }
			public static float3	operator+( float3 a, float b )	{ return new float3( a.x + b, a.y + b, a.z + b ); }
			public static float3	operator+( float3 a, float3 b )	{ return new float3( a.x + b.x, a.y + b.y, a.z + b.z ); }
			public static float3	operator-( float a, float3 b )	{ return new float3( a - b.x, a - b.y, a - b.z ); }
			public static float3	operator-( float3 a, float b )	{ return new float3( a.x - b, a.y - b, a.z - b ); }
			public static float3	operator-( float3 a, float3 b )	{ return new float3( a.x - b.x, a.y - b.y, a.z - b.z ); }
			public static float3	operator*( float a, float3 b )	{ return new float3( a * b.x, a * b.y, a * b.z ); }
			public static float3	operator*( float3 a, float b )	{ return new float3( a.x * b, a.y * b, a.z * b ); }
			public static float3	operator*( float3 a, float3 b )	{ return new float3( a.x * b.x, a.y * b.y, a.z * b.z ); }
			public static float3	operator/( float3 a, float b )	{ return new float3( a.x / b, a.y / b, a.z / b ); }
		}

		public class	float4
		{
			public float	x, y, z, w;

			#region Swizzles
			public float3	xxx	{ get { return new float3( x, x, x ); } }
			public float3	xxy	{ get { return new float3( x, x, y ); } }
			public float3	xyx	{ get { return new float3( x, y, x ); } }
			public float3	xyz	{ get { return new float3( x, y, z ); } }
			public float3	yxx	{ get { return new float3( y, x, x ); } }
			public float3	yxy	{ get { return new float3( y, x, y ); } }
			public float3	yyx	{ get { return new float3( y, y, x ); } }
			public float3	yyy	{ get { return new float3( y, y, y ); } }
			#endregion

			public float4()												{}
			public float4( float _x )									{ x = y = z = w = _x; }
			public float4( float _x, float _y, float _z, float _w )		{ x = _x; y = _y; z = _z; w = _w; }
			public float4( float2 _xy, float _z, float _w )				{ x = _xy.x; y = _xy.y; z = _z; w = _w; }
			public float4( float _x, float2 _yz, float _w )				{ x = _x; y = _yz.x; z = _yz.y; w = _w; }
			public float4( float _x, float _y, float2 _zw )				{ x = _x; y = _y; z = _zw.x; w = _zw.y; }
			public float4( float2 _xy, float2 _zw )						{ x = _xy.x; y = _xy.y; z = _zw.x; w = _zw.y; }
			public float4( float3 _xyz, float _w )						{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = _w; }

			public static implicit operator float2( float4 a )		{ return new float2( a.x, a.y ); }
			public static implicit operator float3( float4 a )		{ return new float3( a.x, a.y, a.z ); }

			public static float4	operator+( float a, float4 b )	{ return new float4( a + b.x, a + b.y, a + b.z, a + b.w ); }
			public static float4	operator+( float4 a, float b )	{ return new float4( a.x + b, a.y + b, a.z + b, a.w + b ); }
			public static float4	operator+( float4 a, float4 b )	{ return new float4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w ); }
			public static float4	operator-( float a, float4 b )	{ return new float4( a - b.x, a - b.y, a - b.z, a - b.w ); }
			public static float4	operator-( float4 a, float b )	{ return new float4( a.x - b, a.y - b, a.z - b, a.w - b ); }
			public static float4	operator-( float4 a, float4 b )	{ return new float4( a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w ); }
			public static float4	operator*( float a, float4 b )	{ return new float4( a * b.x, a * b.y, a * b.z, a * b.w ); }
			public static float4	operator*( float4 a, float b )	{ return new float4( a.x * b, a.y * b, a.z * b, a.w * b ); }
			public static float4	operator*( float4 a, float4 b )	{ return new float4( a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w ); }
			public static float4	operator/( float4 a, float b )	{ return new float4( a.x / b, a.y / b, a.z / b, a.w / b ); }
		}

		public class	float4x4
		{
			public float[,]	m = new float[4,4];

			public float4	this[int _row]	{ get { return new float4( m[_row,0], m[_row,1], m[_row,2], m[_row,3] ); } }
		}

		public class	Sampler
		{

		}

		public class	Texture2D<T> where T:class,new()
		{
			public T	Sample( Sampler _sampler, float2 _uv )
			{
				return new T();
			}

			public Texture2D()
			{

			}
		}

		public float	dot( float2 a, float2 b )	{ return a.x*b.x + a.y*b.y; }
		public float	dot( float3 a, float3 b )	{ return a.x*b.x + a.y*b.y + a.z*b.z; }
		public float	dot( float4 a, float4 b )	{ return a.x*b.x + a.y*b.y + a.z*b.z + a.w*b.w; }
		public float	length( float2 _value )		{ return (float) Math.Sqrt( dot( _value, _value ) ); }
		public float	length( float3 _value )		{ return (float) Math.Sqrt( dot( _value, _value ) ); }
		public float	length( float4 _value )		{ return (float) Math.Sqrt( dot( _value, _value ) ); }
		public float2	normalize( float2 _value )	{ float	InvLength = 1.0f / length( _value ); return _value * InvLength; }
		public float3	normalize( float3 _value )	{ float	InvLength = 1.0f / length( _value ); return _value * InvLength; }
		public float4	normalize( float4 _value )	{ float	InvLength = 1.0f / length( _value ); return _value * InvLength; }

		public float4	mul( float4 a, float4x4 b )
		{
			return new float4(
				a.x * b.m[0,0] + a.y * b.m[1,0] + a.z * b.m[2,0] + a.w * b.m[3,0],
				a.x * b.m[0,1] + a.y * b.m[1,1] + a.z * b.m[2,1] + a.w * b.m[3,1],
				a.x * b.m[0,2] + a.y * b.m[1,2] + a.z * b.m[2,2] + a.w * b.m[3,2],
				a.x * b.m[0,3] + a.y * b.m[1,3] + a.z * b.m[2,3] + a.w * b.m[3,3]
				);
		}

		public float2	_float2( float _x )									{ return new float2( _x ); }
		public float2	_float2( float _x, float _y )						{ return new float2( _x, _y ); }
 		public float3	_float3( float _x )									{ return new float3( _x ); }
		public float3	_float3( float _x, float _y, float _z )				{ return new float3( _x, _y, _z ); }
		public float3	_float3( float2 _xy, float _z )						{ return new float3( _xy, _z ); }
		public float3	_float3( float _x, float2 _yz )						{ return new float3( _x, _yz ); }
		public float4	_float4( float _x )									{ return new float4( _x ); }
		public float4	_float4( float _x, float _y, float _z, float _w )	{ return new float4( _x, _y, _z, _w ); }
		public float4	_float4( float2 _xy, float _z, float _w )			{ return new float4( _xy, _z, _w ); }
		public float4	_float4( float _x, float2 _yz, float _w )			{ return new float4( _x, _yz, _w ); }
		public float4	_float4( float _x, float _y, float2 _zw )			{ return new float4( _x, _y, _zw ); }
		public float4	_float4( float2 _xy, float2 _zw )					{ return new float4( _xy, _zw ); }
		public float4	_float4( float3 _xyz, float _w )					{ return new float4( _xyz, _w ); }
	}
}
