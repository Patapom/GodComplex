using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ShaderInterpreter.ShaderMath;
using ShaderInterpreter.Textures;

namespace ShaderInterpreter
{
	/// <summary>
	/// Base shader class with all the HLSL functions available
	/// </summary>
	public class	Shader
	{
		#region Math (Float)

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

		public float4	mul( float4x4 a, float4 b )
		{
			return new float4(
				b.x * a.m[0,0] + b.y * a.m[0,1] + b.z * a.m[0,2] + b.w * a.m[0,3],
				b.x * a.m[1,0] + b.y * a.m[1,1] + b.z * a.m[1,2] + b.w * a.m[1,3],
				b.x * a.m[2,0] + b.y * a.m[2,1] + b.z * a.m[2,2] + b.w * a.m[2,3],
				b.x * a.m[3,0] + b.y * a.m[3,1] + b.z * a.m[3,2] + b.w * a.m[3,3]
				);
		}

		public float	sin( float a )													{ return (float) Math.Sin( a ); }
		public float	cos( float a )													{ return (float) Math.Cos( a ); }
		public void		sincos( float a, out float _sin, out float _cos )				{ _sin = (float) Math.Sin( a ); _cos = (float) Math.Cos( a ); }

		public float3	cross( float3 a, float3 b )
		{
			return new float3( 
					a.y * b.z - a.z * b.y,
					a.z * b.x - a.x * b.z,
					a.x * b.y - a.y * b.x
				);
		}

		public static float2	_float2( double _x )									{ return new float2( _x ); }
		public static float2	_float2( double _x, double _y )							{ return new float2( _x, _y ); }
 		public static float3	_float3( double _x )									{ return new float3( _x ); }
		public static float3	_float3( double _x, double _y, double _z )				{ return new float3( _x, _y, _z ); }
		public static float3	_float3( float2 _xy, double _z )						{ return new float3( _xy, _z ); }
		public static float3	_float3( double _x, float2 _yz )						{ return new float3( _x, _yz ); }
		public static float4	_float4( double _x )									{ return new float4( _x ); }
		public static float4	_float4( double _x, double _y, double _z, double _w )	{ return new float4( _x, _y, _z, _w ); }
		public static float4	_float4( float2 _xy, double _z, double _w )				{ return new float4( _xy, _z, _w ); }
		public static float4	_float4( double _x, float2 _yz, double _w )				{ return new float4( _x, _yz, _w ); }
		public static float4	_float4( double _x, double _y, float2 _zw )				{ return new float4( _x, _y, _zw ); }
		public static float4	_float4( float2 _xy, float2 _zw )						{ return new float4( _xy, _zw ); }
		public static float4	_float4( float3 _xyz, double _w )						{ return new float4( _xyz, _w ); }

		#endregion

		#region Math (Integer)

		public static uint2		_uint2( uint _x )										{ return new uint2( _x ); }
		public static uint2		_uint2( uint _x, uint _y )								{ return new uint2( _x, _y ); }
 		public static uint3		_uint3( uint _x )										{ return new uint3( _x ); }
		public static uint3		_uint3( uint _x, uint _y, uint _z )						{ return new uint3( _x, _y, _z ); }
		public static uint3		_uint3( uint2 _xy, uint _z )							{ return new uint3( _xy, _z ); }
		public static uint3		_uint3( uint _x, uint2 _yz )							{ return new uint3( _x, _yz ); }
		public static uint4		_uint4( uint _x )										{ return new uint4( _x ); }
		public static uint4		_uint4( uint _x, uint _y, uint _z, uint _w )			{ return new uint4( _x, _y, _z, _w ); }
		public static uint4		_uint4( uint2 _xy, uint _z, uint _w )					{ return new uint4( _xy, _z, _w ); }
		public static uint4		_uint4( uint _x, uint2 _yz, uint _w )					{ return new uint4( _x, _yz, _w ); }
		public static uint4		_uint4( uint _x, uint _y, uint2 _zw )					{ return new uint4( _x, _y, _zw ); }
		public static uint4		_uint4( uint2 _xy, uint2 _zw )							{ return new uint4( _xy, _zw ); }
		public static uint4		_uint4( uint3 _xyz, uint _w )							{ return new uint4( _xyz, _w ); }

		#endregion
	}
}
