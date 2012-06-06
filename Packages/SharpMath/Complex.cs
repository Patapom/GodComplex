using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Complex.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("r = {r} i = {i}")]
    public class Complex
	{
		public double			r, i;

//		public double			R
//		{
//			get { return r; }
//			set { r = value; }
//		}
//
//		public double			I
//		{
//			get { return i; }
//			set { i = value; }
//		}
//
		// Constructors
		public						Complex()										{}
		public						Complex( Complex _Source )						{ Set( _Source ); }
		public						Complex( double _R, double _I )					{ Set( _R, _I ); }
		public						Complex( double _Z, double _Arg, bool _b )		{ SetPhasor( _Z, _Arg ); }
		public						Complex( double[] _f )							{ r = _f[0]; i = _f[1]; }

		// Access methods
		public void					Zero()											{ r = i = 0.0f; }
		public void					Set( Complex _Source )							{ r = _Source.r; i = _Source.i; }
		public void					Set( double _r, double _i )						{ r = _r; i = _i; }
		public void					SetPhasor( double _Modulus, double _Argument )	{ r = _Modulus * Math.Cos( _Argument ); i = _Modulus * Math.Sin( _Argument ); }
		public double				Min()											{ return System.Math.Min( r, i ); }
		public void					Min( Complex _Op )								{ r = System.Math.Min( r, _Op.r ); i = System.Math.Min( i, _Op.i ); }
		public double				Max()											{ return System.Math.Max( r, i ); }
		public void					Max( Complex _Op )								{ r = System.Math.Max( r, _Op.r ); i = System.Math.Max( i, _Op.i ); }
		public double				Sum()											{ return r + i; }
		public double				Product()										{ return r * i; }
		public static Complex		FromSqrt( double _r )
		{
			return new Complex( _r >= 0.0f ? Math.Sqrt( _r ) : 0.0f,
								_r < 0.0f ? Math.Sqrt( -_r ) : 0.0f );
		}
		public double				SquareMagnitude()								{ return r * r + i * i; }
		public double				Magnitude()										{ return System.Math.Sqrt( r * r + i * i ); }
		public double				Argument()										{ return Math.Atan2( i, r ); }
		public Complex				Conjugate()										{ return new Complex( r, -i ); }
		public Complex				Sqrt()											{ return new Complex( Math.Pow( SquareMagnitude(), .25 ), .5f * Argument(), true ); }

		public override string		ToString()
		{
			return	r.ToString() + " + i * " + i.ToString();
		}

//		public static Complex			Parse( string _Source )
//		{
//			string[]	Members = _Source.Split( new System.Char[] { ';' } );
//			if ( Members.Length != 3 )
//				return	null;
//
//			return	new Complex(	double.Parse( Members[0] ),
//								double.Parse( Members[1] ),
//								double.Parse( Members[2] )
//							  );
//		}

		// Cast operators
		public double				this[int _Index]
		{
			get { return _Index == 0 ? r : (_Index == 1 ? i : double.NaN); }
			set
			{
				if ( _Index == 0 )
					r = value;
				else if ( _Index == 1 )
					i = value;
			}
		}

		// Arithmetic operators
		public static Complex			operator-( Complex _Op0 )					{ return new Complex( -_Op0.r, -_Op0.i ); }
		public static Complex			operator+( Complex _Op0 )					{ return new Complex( +_Op0.r, +_Op0.i ); }
		public static Complex			operator+( Complex _Op0, Complex _Op1 )		{ return new Complex( _Op0.r + _Op1.r, _Op0.i + _Op1.i ); }
		public static Complex			operator-( Complex _Op0, Complex _Op1 )		{ return new Complex( _Op0.r - _Op1.r, _Op0.i - _Op1.i ); }
		public static Complex			operator*( Complex _Op0, Complex _Op1 )		{ return new Complex( _Op0.r * _Op1.r - _Op0.i * _Op1.i, _Op0.r * _Op1.i + _Op1.r * _Op0.i ); }
		public static Complex			operator*( Complex _Op0, double _s )		{ return new Complex( _Op0.r * _s, _Op0.i * _s ); }
		public static Complex			operator*( double _s, Complex _Op0 )		{ return new Complex( _Op0.r * _s, _Op0.i * _s ); }
		public static Complex			operator/( Complex _Op0, Complex _Op1 )		{ double fISMagnitude = 1.0f / _Op1.SquareMagnitude(); return new Complex( (_Op0.r * _Op1.r + _Op0.i * _Op1.i) * fISMagnitude, (_Op0.i * _Op1.r - _Op0.r * _Op1.i) * fISMagnitude ); }
		public static Complex			operator/( Complex _Op0, double _s )		{ double Is = 1.0f / _s; return new Complex( _Op0.r * Is, _Op0.i * Is ); }

		// Logic operators
//		public static bool			operator==( Complex _Op0, Complex _Op1 )			{ return (_Op0.r - _Op1.r)*(_Op0.r - _Op1.r) + (_Op0.i - _Op1.i)*(_Op0.i - _Op1.i) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) <= double.Epsilon*double.Epsilon; }
//		public static bool			operator!=( Complex _Op0, Complex _Op1 )			{ return (_Op0.r - _Op1.r)*(_Op0.r - _Op1.r) + (_Op0.i - _Op1.i)*(_Op0.i - _Op1.i) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) >= double.Epsilon*double.Epsilon; }
//		public static bool			operator<( Complex _Op0, Complex _Op1 )				{ return _Op0.r < _Op1.r && _Op0.i < _Op1.i && _Op0.z < _Op1.z; }
//		public static bool			operator<=( Complex _Op0, Complex _Op1 )			{ return _Op0.r < _Op1.r + double.Epsilon && _Op0.i < _Op1.i + double.Epsilon && _Op0.z < _Op1.z + double.Epsilon; }
//		public static bool			operator>( Complex _Op0, Complex _Op1 )				{ return _Op0.r > _Op1.r && _Op0.i > _Op1.i && _Op0.z > _Op1.z; }
//		public static bool			operator>=( Complex _Op0, Complex _Op1 )			{ return _Op0.r > _Op1.r - double.Epsilon && _Op0.i > _Op1.i - double.Epsilon && _Op0.z > _Op1.z - double.Epsilon; }
	}
}
