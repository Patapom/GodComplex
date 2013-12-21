using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ShaderInterpreter.ShaderMath
{
	public class	uint2
	{
		public uint	x, y;

		#region Swizzles
		public uint2	xx	{ get { return new uint2( x, x ); } }
		public uint2	xy	{ get { return new uint2( x, y ); } }
		public uint2	yy	{ get { return new uint2( y, y ); } }
		#endregion

		public uint2()							{}
		public uint2( uint _x )				{ x = y =_x; }
		public uint2( uint _x, uint _y )	{ x = _x; y = _y; }

		public static uint2	operator+( uint a, uint2 b )	{ return new uint2( a + b.x, a + b.y ); }
		public static uint2	operator+( uint2 a, uint b )	{ return new uint2( a.x + b, a.y + b ); }
		public static uint2	operator+( uint2 a, uint2 b )	{ return new uint2( a.x + b.x, a.y + b.y ); }
		public static uint2	operator-( uint a, uint2 b )	{ return new uint2( a - b.x, a - b.y ); }
		public static uint2	operator-( uint2 a, uint b )	{ return new uint2( a.x - b, a.y - b ); }
		public static uint2	operator-( uint2 a, uint2 b )	{ return new uint2( a.x - b.x, a.y - b.y ); }
		public static uint2	operator*( uint a, uint2 b )	{ return new uint2( a * b.x, a * b.y ); }
		public static uint2	operator*( uint2 a, uint b )	{ return new uint2( a.x * b, a.y * b ); }
		public static uint2	operator*( uint2 a, uint2 b )	{ return new uint2( a.x * b.x, a.y * b.y ); }
		public static uint2	operator/( uint2 a, uint b )	{ return new uint2( a.x / b, a.y / b ); }
	}

	public class	uint3
	{
		public uint	x, y, z;

		#region Swizzles
		public uint3	xxx	{ get { return new uint3( x, x, x ); } }
		public uint3	xxy	{ get { return new uint3( x, x, y ); } }
		public uint3	xyx	{ get { return new uint3( x, y, x ); } }
		public uint3	yxx	{ get { return new uint3( y, x, x ); } }
		public uint3	yxy	{ get { return new uint3( y, x, y ); } }
		public uint3	yyx	{ get { return new uint3( y, y, x ); } }
		public uint3	yyy	{ get { return new uint3( y, y, y ); } }
		#endregion

		public uint3()										{}
		public uint3( uint _x )								{ x = y = z = _x; }
		public uint3( uint _x, uint _y, uint _z )			{ x = _x; y = _y; z = _z; }
		public uint3( uint2 _xy, uint _z )					{ x = _xy.x; y = _xy.y; z = _z; }
		public uint3( uint _x, uint2 _yz )					{ x = _x; y = _yz.x; z = _yz.y; }

		public static implicit operator uint2( uint3 a )		{ return new uint2( a.x, a.y ); }

		public static uint3	operator+( uint a, uint3 b )	{ return new uint3( a + b.x, a + b.y, a + b.z ); }
		public static uint3	operator+( uint3 a, uint b )	{ return new uint3( a.x + b, a.y + b, a.z + b ); }
		public static uint3	operator+( uint3 a, uint3 b )	{ return new uint3( a.x + b.x, a.y + b.y, a.z + b.z ); }
		public static uint3	operator-( uint a, uint3 b )	{ return new uint3( a - b.x, a - b.y, a - b.z ); }
		public static uint3	operator-( uint3 a, uint b )	{ return new uint3( a.x - b, a.y - b, a.z - b ); }
		public static uint3	operator-( uint3 a, uint3 b )	{ return new uint3( a.x - b.x, a.y - b.y, a.z - b.z ); }
		public static uint3	operator*( uint a, uint3 b )	{ return new uint3( a * b.x, a * b.y, a * b.z ); }
		public static uint3	operator*( uint3 a, uint b )	{ return new uint3( a.x * b, a.y * b, a.z * b ); }
		public static uint3	operator*( uint3 a, uint3 b )	{ return new uint3( a.x * b.x, a.y * b.y, a.z * b.z ); }
		public static uint3	operator/( uint3 a, uint b )	{ return new uint3( a.x / b, a.y / b, a.z / b ); }
	}

	public class	uint4
	{
		public uint	x, y, z, w;

		#region Swizzles
		public uint3	xxx	{ get { return new uint3( x, x, x ); } }
		public uint3	xxy	{ get { return new uint3( x, x, y ); } }
		public uint3	xyx	{ get { return new uint3( x, y, x ); } }
		public uint3	xyz	{ get { return new uint3( x, y, z ); } }
		public uint3	yxx	{ get { return new uint3( y, x, x ); } }
		public uint3	yxy	{ get { return new uint3( y, x, y ); } }
		public uint3	yyx	{ get { return new uint3( y, y, x ); } }
		public uint3	yyy	{ get { return new uint3( y, y, y ); } }
		#endregion

		public uint4()										{}
		public uint4( uint _x )								{ x = y = z = w = _x; }
		public uint4( uint _x, uint _y, uint _z, uint _w )	{ x = _x; y = _y; z = _z; w = _w; }
		public uint4( uint2 _xy, uint _z, uint _w )			{ x = _xy.x; y = _xy.y; z = _z; w = _w; }
		public uint4( uint _x, uint2 _yz, uint _w )			{ x = _x; y = _yz.x; z = _yz.y; w = _w; }
		public uint4( uint _x, uint _y, uint2 _zw )			{ x = _x; y = _y; z = _zw.x; w = _zw.y; }
		public uint4( uint2 _xy, uint2 _zw )						{ x = _xy.x; y = _xy.y; z = _zw.x; w = _zw.y; }
		public uint4( uint3 _xyz, uint _w )					{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = _w; }

		public static implicit operator uint2( uint4 a )	{ return new uint2( a.x, a.y ); }
		public static implicit operator uint3( uint4 a )	{ return new uint3( a.x, a.y, a.z ); }

		public static uint4	operator+( uint a, uint4 b )	{ return new uint4( a + b.x, a + b.y, a + b.z, a + b.w ); }
		public static uint4	operator+( uint4 a, uint b )	{ return new uint4( a.x + b, a.y + b, a.z + b, a.w + b ); }
		public static uint4	operator+( uint4 a, uint4 b )	{ return new uint4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w ); }
		public static uint4	operator-( uint a, uint4 b )	{ return new uint4( a - b.x, a - b.y, a - b.z, a - b.w ); }
		public static uint4	operator-( uint4 a, uint b )	{ return new uint4( a.x - b, a.y - b, a.z - b, a.w - b ); }
		public static uint4	operator-( uint4 a, uint4 b )	{ return new uint4( a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w ); }
		public static uint4	operator*( uint a, uint4 b )	{ return new uint4( a * b.x, a * b.y, a * b.z, a * b.w ); }
		public static uint4	operator*( uint4 a, uint b )	{ return new uint4( a.x * b, a.y * b, a.z * b, a.w * b ); }
		public static uint4	operator*( uint4 a, uint4 b )	{ return new uint4( a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w ); }
		public static uint4	operator/( uint4 a, uint b )	{ return new uint4( a.x / b, a.y / b, a.z / b, a.w / b ); }
	}
}
