using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Point2D.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("X = {x} Y = {y}")]
    public class Point2D
	{
		#region FIELDS

		public float			x, y;

		#endregion

		#region PROPERTIES

		public float				X
		{
			get	{ return x; }
			set { x = value; }
		}

		public float				Y
		{
			get	{ return y; }
			set { y = value; }
		}

		#endregion

		#region METHODS

		// Constructors
		public						Point2D()										{}
		public						Point2D( Point2D _Source )						{ Set( _Source ); }
		public						Point2D( Point _Source )						{ Set( _Source ); }
		public						Point2D( Point4D _Source )						{ Set( _Source ); }
		public						Point2D( Vector2D _Source )						{ Set( _Source ); }
		public						Point2D( float _x, float _y )					{ Set( _x, _y ); }
		public						Point2D( float[] _f )							{ Set( _f ); }

		// Access methods
		public void					Zero()											{ x = y = 0.0f; }
		public void					Set( Point2D _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( Point _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( Point4D _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( Vector2D _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( float _x, float _y )						{ x = _x; y = _y; }
		public void					Set( float[] _f )								{ x = _f[0]; y = _f[1]; }
		public float				Min()											{ return System.Math.Min( x, y ); }
		public void					Min( Point2D _Op )								{ x = System.Math.Min( x, _Op.x ); y = System.Math.Min( y, _Op.y ); }
		public float				Max()											{ return System.Math.Max( x, y ); }
		public void					Max( Point2D _Op )								{ x = System.Math.Max( x, _Op.x ); y = System.Math.Max( y, _Op.y ); }
		public float				Sum()											{ return x + y; }
		public float				Product()										{ return x * y; }
		public float				SquareDistance()								{ return x * x + y * y; }
		public float				Distance()										{ return (float) System.Math.Sqrt( x * x + y * y ); }

		public override string		ToString()
		{
			return	x.ToString() + "; " + y.ToString();
		}

		public static Point2D		Parse( string _Source )
		{
			string[]	Members = _Source.Split( new System.Char[] { ';' } );
			if ( Members.Length != 2 )
				return	null;

			return	new Point2D( float.Parse( Members[0] ), float.Parse( Members[1] ) );
		}

		// Arithmetic operators
		public static Point2D		operator-( Point2D _Op0 )						{ return new Point2D( -_Op0.x, -_Op0.y ); }
		public static Point2D		operator+( Point2D _Op0 )						{ return new Point2D( +_Op0.x, +_Op0.y ); }
		public static Point2D		operator+( Point2D _Op0, Vector2D _Op1 )		{ return new Point2D( _Op0.x + _Op1.x, _Op0.y + _Op1.y ); }
		public static Point2D		operator-( Point2D _Op0, Vector2D _Op1 )		{ return new Point2D( _Op0.x - _Op1.x, _Op0.y - _Op1.y ); }
		public static Vector2D		operator-( Point2D _Op0, Point2D _Op1 )			{ return new Vector2D( _Op0.x - _Op1.x, _Op0.y - _Op1.y ); }
		public static Point2D		operator*( Point2D _Op0, float _s )				{ return new Point2D( _Op0.x * _s, _Op0.y * _s ); }
		public static Point2D		operator*( float _s, Point2D _Op0 )				{ return new Point2D( _Op0.x * _s, _Op0.y * _s ); }
		public static Point2D		operator/( Point2D _Op0, float _s )				{ float Is = 1.0f / _s; return new Point2D( _Op0.x * Is, _Op0.y * Is ); }

		// Logic operators
		public static bool			operator==( Point2D _Op0, Point2D _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	true;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	false;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	false;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) <= float.Epsilon;
		}
		public static bool			operator!=( Point2D _Op0, Point2D _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	false;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	true;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	true;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) > float.Epsilon;
		}
		public static bool			operator<( Point2D _Op0, Point2D _Op1 )			{ return _Op0.x < _Op1.x && _Op0.y < _Op1.y; }
		public static bool			operator<=( Point2D _Op0, Point2D _Op1 )		{ return _Op0.x < _Op1.x + float.Epsilon && _Op0.y < _Op1.y + float.Epsilon; }
		public static bool			operator>( Point2D _Op0, Point2D _Op1 )			{ return _Op0.x > _Op1.x && _Op0.y > _Op1.y; }
		public static bool			operator>=( Point2D _Op0, Point2D _Op1 )		{ return _Op0.x > _Op1.x - float.Epsilon && _Op0.y > _Op1.y - float.Epsilon; }

		#endregion
	}
}
