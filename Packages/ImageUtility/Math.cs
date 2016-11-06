using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//////////////////////////////////////////////////////////////////////////
// Very simple and basic math library oriented to support basic color vectors manipulation and transformation
//
namespace ImageUtility
{
	[System.Diagnostics.DebuggerDisplay( "x={x} y={y}" )]
	public struct	float2 {
		public float	x, y;
		public float2( float _x, float _y )		{ x = _x; y = _y; }
		public static float2	operator+( float2 a, float2 b )		{ return new float2( a.x + b.x, a.y + b.y ); }
		public static float2	operator-( float2 a, float2 b )		{ return new float2( a.x - b.x, a.y - b.y ); }
		public static float2	operator*( float a, float2 b )		{ return new float2( a * b.x, a * b.y ); }
		public float			Dot( float2 a )						{ return x*a.x + y*a.y; }

		public override string ToString()
		{
			return x.ToString() + "; " + y.ToString();
		}
		public string ToString( string _Format )
		{
			return x.ToString( _Format ) + "; " + y.ToString( _Format );
		}
		public static float2	Parse( string v )
		{
			string[]	Components = v.Split( ';' );
			if ( Components.Length < 2 )
				throw new Exception( "Not enough vector components!" );
			float2		Result = new float2();
			if ( !float.TryParse( Components[0].Trim(), out Result.x ) )
				throw new Exception( "Can't parse X field!" );
			if ( !float.TryParse( Components[1].Trim(), out Result.y ) )
				throw new Exception( "Can't parse Y field!" );
			return Result;
		}
	}
	[System.Diagnostics.DebuggerDisplay( "x={x} y={y} z={z}" )]
	public struct	float3 {
		public float	x, y, z;
		public float3( float _x, float _y, float _z )		{ x = _x; y = _y; z = _z; }
		public static float3	operator+( float3 a, float3 b )		{ return new float3( a.x + b.x, a.y + b.y, a.z + b.z ); }
		public static float3	operator-( float3 a, float3 b )		{ return new float3( a.x - b.x, a.y - b.y, a.z - b.z ); }
		public static float3	operator*( float a, float3 b )		{ return new float3( a * b.x, a * b.y, a * b.z ); }
		public static float3	operator*( float3 a, float3 b )		{ return new float3( a.x * b.x, a.y * b.y, a.z * b.z ); }

		public override string ToString()
		{
			return x.ToString() + "; " + y.ToString() + "; " + z.ToString();
		}
		public string ToString( string _Format )
		{
			return x.ToString( _Format ) + "; " + y.ToString( _Format ) + "; " + z.ToString( _Format );
		}
		public static float3	Parse( string v )
		{
			string[]	Components = v.Split( ';' );
			if ( Components.Length < 3 )
				throw new Exception( "Not enough vector components!" );
			float3		Result = new float3();
			if ( !float.TryParse( Components[0].Trim(), out Result.x ) )
				throw new Exception( "Can't parse X field!" );
			if ( !float.TryParse( Components[1].Trim(), out Result.y ) )
				throw new Exception( "Can't parse Y field!" );
			if ( !float.TryParse( Components[2].Trim(), out Result.z ) )
				throw new Exception( "Can't parse Z field!" );
			return Result;
		}
	}
	[System.Diagnostics.DebuggerDisplay( "x={x} y={y} z={z} w={w}" )]
	public struct	float4
	{
		public float	x, y, z, w;

		public float	this[int _coeff]
		{
			get
			{
				switch ( _coeff )
				{
					case 0: return x;
					case 1: return y;
					case 2: return z;
					case 3: return w;
				}
				return float.NaN;
			}
			set
			{
				switch ( _coeff )
				{
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					case 3: w = value; break;
				}
			}
		}

		public float4( float _x, float _y, float _z, float _w )		{ x = _x; y = _y; z = _z; w = _w; }
		public float4( float3 _xyz, float _w )						{ x = _xyz.x; y = _xyz.y; z = _xyz.z; w = _w; }

		public static explicit	operator float3( float4 a )
		{
			return new float3( a.x, a.y, a.z );
		}

		public static float4	operator+( float4 a, float4 b )		{ return new float4( a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w ); }
		public static float4	operator*( float a, float4 b )		{ return new float4( a * b.x, a * b.y, a * b.z, a *b.w ); }
		public static float4	operator*( float4 a, float4x4 b )
		{
			return new float4(
				a.x * b.row0.x + a.y * b.row1.x + a.z * b.row2.x + a.w * b.row3.x,
				a.x * b.row0.y + a.y * b.row1.y + a.z * b.row2.y + a.w * b.row3.y,
				a.x * b.row0.z + a.y * b.row1.z + a.z * b.row2.z + a.w * b.row3.z,
				a.x * b.row0.w + a.y * b.row1.w + a.z * b.row2.w + a.w * b.row3.w
				);
		}
		public float			dot( float4 b ) { return x*b.x + y*b.y + z*b.z + w*b.w; }

		public override string ToString()
		{
			return x.ToString() + "; " + y.ToString() + "; " + z.ToString() + "; " + w.ToString();
		}
		public string ToString( string _Format )
		{
			return x.ToString( _Format ) + "; " + y.ToString( _Format ) + "; " + z.ToString( _Format ) + "; " + w.ToString( _Format );
		}
		public static float4	Parse( string v )
		{
			string[]	Components = v.Split( ';' );
			if ( Components.Length < 4 )
				throw new Exception( "Not enough vector components!" );
			float4		Result = new float4();
			if ( !float.TryParse( Components[0].Trim(), out Result.x ) )
				throw new Exception( "Can't parse X field!" );
			if ( !float.TryParse( Components[1].Trim(), out Result.y ) )
				throw new Exception( "Can't parse Y field!" );
			if ( !float.TryParse( Components[2].Trim(), out Result.z ) )
				throw new Exception( "Can't parse Z field!" );
			if ( !float.TryParse( Components[3].Trim(), out Result.w ) )
				throw new Exception( "Can't parse W field!" );
			return Result;
		}
	}
	public struct	float4x4
	{
		public float4	row0;
		public float4	row1;
		public float4	row2;
		public float4	row3;

		public float	this[int row, int column]
		{
			get
			{
				switch ( row )
				{
					case 0: return row0[column];
					case 1: return row1[column];
					case 2: return row2[column];
					case 3: return row3[column];
				}
				return float.NaN;
			}
			set
			{
				switch ( row )
				{
					case 0: row0[column] = value; break;
					case 1: row1[column] = value; break;
					case 2: row2[column] = value; break;
					case 3: row3[column] = value; break;
				}
			}
		}

		public float4x4( float[] _a )
		{
			row0 = new float4( _a[0], _a[1], _a[2], _a[3] );
			row1 = new float4( _a[4], _a[5], _a[6], _a[7] );
			row2 = new float4( _a[8], _a[9], _a[10], _a[11] );
			row3 = new float4( _a[12], _a[13], _a[14], _a[15] );
		}

		public float4	column0	{ get { return new float4( row0.x, row1.x, row2.x, row3.x ); } }
		public float4	column1	{ get { return new float4( row0.y, row1.y, row2.y, row3.y ); } }
		public float4	column2	{ get { return new float4( row0.z, row1.z, row2.z, row3.z ); } }
		public float4	column3	{ get { return new float4( row0.w, row1.w, row2.w, row3.w ); } }

		private static int[]		ms_Index	= { 0, 1, 2, 3, 0, 1, 2 };				// This array gives the index of the current component
		public float				CoFactor( int _dwRow, int _dwCol )
		{
			return	((	this[ms_Index[_dwRow+1], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+3], ms_Index[_dwCol+3]] +
						this[ms_Index[_dwRow+1], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+3], ms_Index[_dwCol+1]] +
						this[ms_Index[_dwRow+1], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+3], ms_Index[_dwCol+2]] )

					-(	this[ms_Index[_dwRow+3], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+1], ms_Index[_dwCol+3]] +
						this[ms_Index[_dwRow+3], ms_Index[_dwCol+2]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+1], ms_Index[_dwCol+1]] +
						this[ms_Index[_dwRow+3], ms_Index[_dwCol+3]]*this[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*this[ms_Index[_dwRow+1], ms_Index[_dwCol+2]] ))
					* (((_dwRow + _dwCol) & 1) == 1 ? -1.0f : +1.0f);
		}
		public float				Determinant()					{ return this[0, 0] * CoFactor( 0, 0 ) + this[0, 1] * CoFactor( 0, 1 ) + this[0, 2] * CoFactor( 0, 2 ) + this[0, 3] * CoFactor( 0, 3 ); }
		public void	Invert()
		{
			float	fDet = Determinant();
			if ( (float) System.Math.Abs(fDet) < float.Epsilon )
				throw new Exception( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

			float	fIDet = 1.0f / fDet;

			float4x4	Temp = new float4x4();
			Temp[0, 0] = CoFactor( 0, 0 ) * fIDet;
			Temp[1, 0] = CoFactor( 0, 1 ) * fIDet;
			Temp[2, 0] = CoFactor( 0, 2 ) * fIDet;
			Temp[3, 0] = CoFactor( 0, 3 ) * fIDet;
			Temp[0, 1] = CoFactor( 1, 0 ) * fIDet;
			Temp[1, 1] = CoFactor( 1, 1 ) * fIDet;
			Temp[2, 1] = CoFactor( 1, 2 ) * fIDet;
			Temp[3, 1] = CoFactor( 1, 3 ) * fIDet;
			Temp[0, 2] = CoFactor( 2, 0 ) * fIDet;
			Temp[1, 2] = CoFactor( 2, 1 ) * fIDet;
			Temp[2, 2] = CoFactor( 2, 2 ) * fIDet;
			Temp[3, 2] = CoFactor( 2, 3 ) * fIDet;
			Temp[0, 3] = CoFactor( 3, 0 ) * fIDet;
			Temp[1, 3] = CoFactor( 3, 1 ) * fIDet;
			Temp[2, 3] = CoFactor( 3, 2 ) * fIDet;
			Temp[3, 3] = CoFactor( 3, 3 ) * fIDet;

			row0 = Temp.row0;
			row1 = Temp.row1;
			row2 = Temp.row2;
			row3 = Temp.row3;
		}

		public static float4x4	Identity	{ get { return new float4x4( new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 } ); } }

		public static float4x4	operator*( float4x4 a, float4x4 b )
		{
			return new float4x4() {
				row0 = new float4( a.row0.dot( b.column0 ), a.row0.dot( b.column1 ), a.row0.dot( b.column2 ), a.row0.dot( b.column3 ) ),
				row1 = new float4( a.row1.dot( b.column0 ), a.row1.dot( b.column1 ), a.row1.dot( b.column2 ), a.row1.dot( b.column3 ) ),
				row2 = new float4( a.row2.dot( b.column0 ), a.row2.dot( b.column1 ), a.row2.dot( b.column2 ), a.row2.dot( b.column3 ) ),
				row3 = new float4( a.row3.dot( b.column0 ), a.row3.dot( b.column1 ), a.row3.dot( b.column2 ), a.row3.dot( b.column3 ) )
			};
		}
	}
}
