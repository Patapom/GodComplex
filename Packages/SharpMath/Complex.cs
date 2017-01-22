using System;

namespace SharpMath
{
	/// <summary>
	/// Summary description for Complex.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("r = {r} i = {i}")]
    public struct Complex {
		public double			r, i;

		// Properties
		public double				SquareMagnitude									{ get { return r * r + i * i; } }
		public double				Magnitude										{ get { return System.Math.Sqrt( r * r + i * i ); } }
		public double				Argument										{ get { return Math.Atan2( i, r ); } }

		public double				this[int _index] {
			get { return _index == 0 ? r : (_index == 1 ? i : double.NaN); }
			set {
				if ( _index == 0 )
					r = value;
				else if ( _index == 1 )
					i = value;
			}
		}

		// Constructors
//		public						Complex()										{}
		public						Complex( Complex _Source )						{ r = _Source.r; i = _Source.i; }
		public						Complex( double _R, double _I )					{ r = _R; i = _I; }
		public						Complex( float2 _f )							{ r = _f.x; i = _f.y; }
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
		public static Complex		FromSqrt( double _r ) {
			return new Complex( _r >= 0.0f ? Math.Sqrt( _r ) : 0.0f,
								_r < 0.0f ? Math.Sqrt( -_r ) : 0.0f );
		}
		public Complex				Conjugate()										{ return new Complex( r, -i ); }
		public Complex				Sqrt()											{ Complex R = new Complex(); R.SetPhasor( Math.Pow( SquareMagnitude, .25 ), .5 * Argument ); return R; }

		public override string		ToString() {
			return	r.ToString() + " + i * " + i.ToString();
		}

//		public static Complex			Parse( string _Source ) {
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
		public float2				float2()	{
			return new float2( (float) r, (float) i );
		}

		// Arithmetic operators
		public static Complex			operator-( Complex _Op0 )					{ return new Complex( -_Op0.r, -_Op0.i ); }
		public static Complex			operator+( Complex _Op0 )					{ return new Complex( +_Op0.r, +_Op0.i ); }
		public static Complex			operator+( Complex _Op0, Complex _Op1 )		{ return new Complex( _Op0.r + _Op1.r, _Op0.i + _Op1.i ); }
		public static Complex			operator-( Complex _Op0, Complex _Op1 )		{ return new Complex( _Op0.r - _Op1.r, _Op0.i - _Op1.i ); }
		public static Complex			operator*( Complex _Op0, Complex _Op1 )		{ return new Complex( _Op0.r * _Op1.r - _Op0.i * _Op1.i, _Op0.r * _Op1.i + _Op1.r * _Op0.i ); }
		public static Complex			operator*( Complex _Op0, double _s )		{ return new Complex( _Op0.r * _s, _Op0.i * _s ); }
		public static Complex			operator*( double _s, Complex _Op0 )		{ return new Complex( _Op0.r * _s, _Op0.i * _s ); }
		public static Complex			operator/( Complex _Op0, Complex _Op1 )		{ double fISMagnitude = 1.0f / _Op1.SquareMagnitude; return new Complex( (_Op0.r * _Op1.r + _Op0.i * _Op1.i) * fISMagnitude, (_Op0.i * _Op1.r - _Op0.r * _Op1.i) * fISMagnitude ); }
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
