using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ShaderInterpreter.ShaderMath
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
		public float2( double _x )				{ x = y = (float) _x; }
		public float2( double _x, double _y )	{ x = (float) _x; y = (float) _y; }

		public static float2	operator+( double a, float2 b )	{ return new float2( a + b.x, a + b.y ); }
		public static float2	operator+( float2 a, double b )	{ return new float2( a.x + b, a.y + b ); }
		public static float2	operator+( float2 a, float2 b )	{ return new float2( a.x + b.x, a.y + b.y ); }
		public static float2	operator-( double a, float2 b )	{ return new float2( a - b.x, a - b.y ); }
		public static float2	operator-( float2 a, double b )	{ return new float2( a.x - b, a.y - b ); }
		public static float2	operator-( float2 a, float2 b )	{ return new float2( a.x - b.x, a.y - b.y ); }
		public static float2	operator*( double a, float2 b )	{ return new float2( a * b.x, a * b.y ); }
		public static float2	operator*( float2 a, double b )	{ return new float2( a.x * b, a.y * b ); }
		public static float2	operator*( float2 a, float2 b )	{ return new float2( a.x * b.x, a.y * b.y ); }
		public static float2	operator/( float2 a, double b )	{ return new float2( a.x / b, a.y / b ); }
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
		public float3( double _x )							{ x = y = z = (float) _x; }
		public float3( double _x, double _y, double _z )	{ x = (float) _x; y = (float) _y; z = (float) _z; }
		public float3( float2 _xy, double _z )				{ x = _xy.x; y = _xy.y; z = (float) _z; }
		public float3( double _x, float2 _yz )				{ x = (float) _x; y = _yz.x; z = _yz.y; }

		public static implicit operator float2( float3 a )		{ return new float2( a.x, a.y ); }

		public static float3	operator+( double a, float3 b )	{ return new float3( a + b.x, a + b.y, a + b.z ); }
		public static float3	operator+( float3 a, double b )	{ return new float3( a.x + b, a.y + b, a.z + b ); }
		public static float3	operator+( float3 a, float3 b )	{ return new float3( a.x + b.x, a.y + b.y, a.z + b.z ); }
		public static float3	operator-( double a, float3 b )	{ return new float3( a - b.x, a - b.y, a - b.z ); }
		public static float3	operator-( float3 a, double b )	{ return new float3( a.x - b, a.y - b, a.z - b ); }
		public static float3	operator-( float3 a, float3 b )	{ return new float3( a.x - b.x, a.y - b.y, a.z - b.z ); }
		public static float3	operator*( double a, float3 b )	{ return new float3( a * b.x, a * b.y, a * b.z ); }
		public static float3	operator*( float3 a, double b )	{ return new float3( a.x * b, a.y * b, a.z * b ); }
		public static float3	operator*( float3 a, float3 b )	{ return new float3( a.x * b.x, a.y * b.y, a.z * b.z ); }
		public static float3	operator/( float3 a, double b )	{ return new float3( a.x / b, a.y / b, a.z / b ); }
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
		public float4( double _x )									{ x = y = z = w = (float) _x; }
		public float4( double _x, double _y, double _z, double _w )	{ x = (float) _x; y = (float) _y; z = (float) _z; w = (float) _w; }
		public float4( float2 _xy, double _z, double _w )			{ x = _xy.x; y = _xy.y; z = (float) _z; w = (float) _w; }
		public float4( double _x, float2 _yz, double _w )			{ x = (float) _x; y = _yz.x; z = _yz.y; w = (float) _w; }
		public float4( double _x, double _y, float2 _zw )			{ x = (float) _x; y = (float) _y; z = _zw.x; w = _zw.y; }
		public float4( float2 _xy, float2 _zw )						{ x = _xy.x; y = _xy.y; z = _zw.x; w = _zw.y; }
		public float4( float3 _xyz, double _w )						{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = (float) _w; }

		public static implicit operator float2( float4 a )		{ return new float2( a.x, a.y ); }
		public static implicit operator float3( float4 a )		{ return new float3( a.x, a.y, a.z ); }

		public static float4	operator+( double a, float4 b )	{ return new float4( a + b.x, a + b.y, a + b.z, a + b.w ); }
		public static float4	operator+( float4 a, double b )	{ return new float4( a.x + b, a.y + b, a.z + b, a.w + b ); }
		public static float4	operator+( float4 a, float4 b )	{ return new float4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w ); }
		public static float4	operator-( double a, float4 b )	{ return new float4( a - b.x, a - b.y, a - b.z, a - b.w ); }
		public static float4	operator-( float4 a, double b )	{ return new float4( a.x - b, a.y - b, a.z - b, a.w - b ); }
		public static float4	operator-( float4 a, float4 b )	{ return new float4( a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w ); }
		public static float4	operator*( double a, float4 b )	{ return new float4( a * b.x, a * b.y, a * b.z, a * b.w ); }
		public static float4	operator*( float4 a, double b )	{ return new float4( a.x * b, a.y * b, a.z * b, a.w * b ); }
		public static float4	operator*( float4 a, float4 b )	{ return new float4( a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w ); }
		public static float4	operator/( float4 a, double b )	{ return new float4( a.x / b, a.y / b, a.z / b, a.w / b ); }
	}

	public class	float4x4
	{
		public float[,]	m = new float[4,4];

		public float4	this[int _row]	{ get { return new float4( m[_row,0], m[_row,1], m[_row,2], m[_row,3] ); } }
	}
}
