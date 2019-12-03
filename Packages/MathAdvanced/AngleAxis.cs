using System;

namespace SharpMath
{
	/// <summary>
	/// Summary description for AngleAxis.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("Angle = {Angle} Axis = ({Axis.x}, {Axis.y}, {Axis.z})")]
    public class AngleAxis
	{
		#region FIELDS

		public float			Angle = 0.0f;
		public float3			Axis = new float3( 0.0f, 0.0f, 0.0f );

		#endregion

		#region METHODS

		// Constructors
		public						AngleAxis()												{}
		public						AngleAxis( AngleAxis _aa )								{ Angle = _aa.Angle; Axis = _aa.Axis; }
		public						AngleAxis( float _angle, float _x, float _y, float _z )	{ Set( _angle, _x, _y, _z ); }
		public						AngleAxis( float[] _f )									{ Set( _f[0], _f[1], _f[2], _f[3] ); }
		public						AngleAxis( float _angle, float3 _axis )					{ Set( _angle, _axis ); }
		public						AngleAxis( Quat _q )
		{
			Angle = (float) System.Math.Acos( _q.qs );
			float	fSine = (float) System.Math.Sin( Angle );
			Angle *= 2.0f;

			Quat	Temp = new Quat( _q );
			Temp.Normalize();

			if ( System.Math.Abs( fSine ) > float.Epsilon )
				Axis = Temp.qv / fSine;
			else 
				Axis.Set( 0, 0, 0 );
		}


		// Access methods
		public void					Zero()													{ Angle = 0.0f; Axis.Set( 0, 0, 0 ); }
		public void					Set( float _fAngle, float _x, float _y, float _z )		{ Angle = _fAngle; Axis.Set( _x, _y, _z ); }
		public void					Set( float _fAngle, float3 _Axis )						{ Angle = _fAngle; Axis.Set( _Axis.x, _Axis.y, _Axis.z ); }
		public int					GetRevNum()												{ return (int) System.Math.Floor( 0.5f * Angle / System.Math.PI ); }
		public void					SetRevNum( int _RevCount )								{ Angle += 2.0f * (_RevCount - GetRevNum()) * (float) System.Math.PI; }
		public float				SquareMagnitude()										{ return Axis.LengthSquared; }
		public float				Magnitude()												{ return Axis.Length; }

		// Helpers
		public void					Normalize()												{ Axis.Normalize(); }
		public bool					IsNormalized()											{ return Math.Abs( Axis.LengthSquared - 1) < 1e-6f; }
		public bool					IsTooSmall()											{ return Axis.LengthSquared < 1e-12f; }

		// Cast operators
		public static explicit		operator Quat( AngleAxis _aa )							{ return new Quat( (float) System.Math.Cos( .5f * _aa.Angle ), (float) System.Math.Sin( .5f * _aa.Angle ) * _aa.Axis ); }
		public static explicit		operator float4x4( AngleAxis _aa )						{ return (float4x4) (Quat) _aa; }

		// Arithmetic operators
		public static AngleAxis		operator-( AngleAxis _aa )								{ return new AngleAxis( -_aa.Angle, -_aa.Axis ); }
		public static AngleAxis		operator+( AngleAxis _aa )								{ return new AngleAxis(  _aa.Angle,  _aa.Axis ); }

		public static AngleAxis		operator+( AngleAxis _Op0, AngleAxis _Op1 )				{ return new AngleAxis( _Op0.Angle + _Op1.Angle, _Op0.Axis + _Op1.Axis ); }
		public static AngleAxis		operator-( AngleAxis _Op0, AngleAxis _Op1 )				{ return new AngleAxis( _Op0.Angle - _Op1.Angle, _Op0.Axis - _Op1.Axis ); }

		// Logic operators
		public static bool			operator==( AngleAxis _Op0, AngleAxis _Op1 )			{ return _Op0.Axis == _Op1.Axis && (float) System.Math.Abs( _Op0.Angle - _Op1.Angle ) <= float.Epsilon; }
		public static bool			operator!=( AngleAxis _Op0, AngleAxis _Op1 )			{ return _Op0.Axis != _Op1.Axis || (float) System.Math.Abs( _Op0.Angle - _Op1.Angle ) > float.Epsilon; }

		#endregion
	}
}
