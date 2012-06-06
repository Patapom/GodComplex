using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Vector4D.
	/// </summary>
	[System.ComponentModel.TypeConverter(typeof(Vector4DTypeConverter))]
    [System.Diagnostics.DebuggerDisplay("X = {x} Y = {y} Z = {z} W = {w}")]
    public class Vector4D
	{
		#region FIELDS

		public float			x, y, z, w;

		#endregion

		#region PROPERTIES

		public float			X
		{
			get { return x; }
			set { x = value; }
		}

		public float			Y
		{
			get { return y; }
			set { y = value; }
		}

		public float			Z
		{
			get { return z; }
			set { z = value; }
		}

		public float			W
		{
			get { return w; }
			set { w = value; }
		}

		#endregion

		#region METHODS

		// Constructors
		public						Vector4D()											{}
		public						Vector4D( Vector2D _Source )						{ Set( _Source ); }
		public						Vector4D( Vector _Source )							{ Set( _Source ); }
		public						Vector4D( Vector4D _Source )						{ Set( _Source ); }
		public						Vector4D( Point4D _Source )							{ Set( _Source ); }
		public						Vector4D( float _x, float _y, float _z, float _w )	{ Set( _x, _y, _z, _w ); }
		public						Vector4D( float[] _f )								{ Set( _f ); }

		// Access methods
		public void					Zero()											{ x = y = z = w = 0.0f; }
		public void					Set( Vector2D _Source )							{ x = _Source.x; y = _Source.y; z = w = 0.0f; }
		public void					Set( Vector _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; w = 0.0f; }
		public void					Set( Vector4D _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; w = _Source.w; }
		public void					Set( Point4D _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; w = _Source.w; }
		public void					Set( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
		public void					Set( float[] _f )								{ x = _f[0]; y = _f[1]; z = _f[2]; w = _f[3]; }
		public void					Add( float _x, float _y, float _z, float _w )	{ x += _x; y += _y; z += _z; w += _w; }
		public float				Min()											{ return System.Math.Min( x, System.Math.Min( y, System.Math.Min( z, w ) ) ); }
		public void					Min( Vector4D _Op )								{ x = System.Math.Min( x, _Op.x ); y = System.Math.Min( y, _Op.y ); z = System.Math.Min( z, _Op.z ); w = System.Math.Min( w, _Op.w ); }
		public float				Max()											{ return System.Math.Max( x, System.Math.Max( y, System.Math.Max( z, w ) ) ); }
		public void					Max( Vector4D _Op )								{ x = System.Math.Max( x, _Op.x ); y = System.Math.Max( y, _Op.y ); z = System.Math.Max( z, _Op.z ); w = System.Math.Max( w, _Op.w ); }
		public float				Sum()											{ return x + y + z + w; }
		public float				Product()										{ return x * y * z * w; }
		public float				SquareMagnitude()								{ return x * x + y * y + z * z + w * w; }
		public float				Magnitude()										{ return (float) System.Math.Sqrt( x * x + y * y + z * z + w * w ); }
		public Vector4D				Normalize()										{ if ( Magnitude() < 1e-12 ) return this; float fINorm = 1.0f / Magnitude(); x *= fINorm; y *= fINorm; z *= fINorm; w *= fINorm; return this; }
		public bool					IsNormalized()									{ return System.Math.Abs( SquareMagnitude() - 1.0f ) < float.Epsilon*float.Epsilon; }
		public bool					IsTooSmall()									{ return SquareMagnitude() < float.Epsilon*float.Epsilon; }

		public void					Clamp( float _fMin, float _fMax )				{ x = System.Math.Max( _fMin, System.Math.Min( _fMax, x ) ); y = System.Math.Max( _fMin, System.Math.Min( _fMax, y ) ); z = System.Math.Max( _fMin, System.Math.Min( _fMax, z ) ); w = System.Math.Max( _fMin, System.Math.Min( _fMax, w ) ); }

		public override string		ToString()
		{
			return	x.ToString() + "; " + y.ToString() + "; " + z.ToString() + "; " + w.ToString();
		}

		public static Vector4D		Parse( string _Source )
		{
			string[]	Members = _Source.Split( new System.Char[] { ';' } );
			if ( Members.Length != 4 )
				return	null;

			return	new Vector4D(	float.Parse( Members[0] ),
									float.Parse( Members[1] ),
									float.Parse( Members[2] ),
									float.Parse( Members[3] )
								);
		}

		// Cast operators
		public static explicit		operator Vector2D( Vector4D _Source )			{ return new Vector2D( _Source ); }
		public static explicit		operator Vector( Vector4D _Source )				{ return new Vector( _Source ); }
		public static explicit		operator Point4D( Vector4D _Source )			{ return new Point4D( _Source ); }

		// Index operators
		public float				this[int _Index]
		{
			get { return _Index == 0 ? x : (_Index == 1 ? y : (_Index == 2 ? z : (_Index == 3 ? w : float.NaN))); }
			set
			{
				if ( _Index == 0 )
					x = value;
				else if ( _Index == 1 )
					y = value;
				else if ( _Index == 2 )
					z = value;
				else if ( _Index == 3 )
					w = value;
			}
		}

		// Arithmetic operators
		public static Vector4D		operator-( Vector4D _Op0 )						{ return new Vector4D( -_Op0.x, -_Op0.y, -_Op0.z, -_Op0.w ); }
		public static Vector4D		operator+( Vector4D _Op0 )						{ return new Vector4D( +_Op0.x, +_Op0.y, +_Op0.z, +_Op0.w ); }
		public static Vector4D		operator+( Vector4D _Op0, Vector4D _Op1 )		{ return new Vector4D( _Op0.x + _Op1.x, _Op0.y + _Op1.y, _Op0.z + _Op1.z, _Op0.w + _Op1.w ); }
		public static Vector4D		operator-( Vector4D _Op0, Vector4D _Op1 )		{ return new Vector4D( _Op0.x - _Op1.x, _Op0.y - _Op1.y, _Op0.z - _Op1.z, _Op0.w - _Op1.w ); }
		public static Vector4D		operator*( Vector4D _Op0, Vector4D _Op1 )		{ return new Vector4D( _Op0.x * _Op1.x, _Op0.y * _Op1.y, _Op0.z * _Op1.z, _Op0.w * _Op1.w ); }
		public static Vector4D		operator*( Vector4D _Op0, float _s )			{ return new Vector4D( _Op0.x * _s, _Op0.y * _s, _Op0.z * _s, _Op0.w * _s ); }
		public static Vector4D		operator*( float _s, Vector4D _Op0 )			{ return new Vector4D( _Op0.x * _s, _Op0.y * _s, _Op0.z * _s, _Op0.w * _s ); }
		public static Vector4D		operator*( Vector4D _Op0, Matrix4x4 _Op1 )
		{
			return new Vector4D(	_Op0.x * _Op1.m[0,0] + _Op0.y * _Op1.m[1,0] + _Op0.z * _Op1.m[2,0] + _Op0.w * _Op1.m[3,0],
									_Op0.x * _Op1.m[0,1] + _Op0.y * _Op1.m[1,1] + _Op0.z * _Op1.m[2,1] + _Op0.w * _Op1.m[3,1],
									_Op0.x * _Op1.m[0,2] + _Op0.y * _Op1.m[1,2] + _Op0.z * _Op1.m[2,2] + _Op0.w * _Op1.m[3,2],
									_Op0.x * _Op1.m[0,3] + _Op0.y * _Op1.m[1,3] + _Op0.z * _Op1.m[2,3] + _Op0.w * _Op1.m[3,3]
								);
		}
		public static Vector4D		operator*( Matrix4x4 _Op0, Vector4D _Op1 )
		{
			return new Vector4D(	_Op1.x * _Op0.m[0,0] + _Op1.y * _Op0.m[0,1] + _Op1.z * _Op0.m[0,2] + _Op1.w * _Op0.m[0,3],
									_Op1.x * _Op0.m[1,0] + _Op1.y * _Op0.m[1,1] + _Op1.z * _Op0.m[1,2] + _Op1.w * _Op0.m[1,3],
									_Op1.x * _Op0.m[2,0] + _Op1.y * _Op0.m[2,1] + _Op1.z * _Op0.m[2,2] + _Op1.w * _Op0.m[2,3],
									_Op1.x * _Op0.m[3,0] + _Op1.y * _Op0.m[3,1] + _Op1.z * _Op0.m[3,2] + _Op1.w * _Op0.m[3,3]
								);
			}
		public static Vector4D		operator/( Vector4D _Op0, float _s )				{ float Is = 1.0f / _s; return new Vector4D( _Op0.x * Is, _Op0.y * Is, _Op0.z * Is, _Op0.w * Is ); }
		public static Vector4D		operator/( Vector4D _Op0, Vector4D _Op1 )			{ return new Vector4D( _Op0.x / _Op1.x, _Op0.y / _Op1.y, _Op0.z / _Op1.z, _Op0.w / _Op1.w ); }
		public static float			operator|( Vector4D _Op0, Vector4D _Op1 )			{ return _Op0.x * _Op1.x + _Op0.y * _Op1.y + _Op0.z * _Op1.z + _Op0.w * _Op1.w; }
//		public static Vector4D		operator^( Vector4D _Op0, Vector4D _Op1 )			{ return new Vector4D( _Op0.y * _Op1.z - _Op0.z * _Op1.y, _Op0.z * _Op1.x - _Op0.x * _Op1.z, _Op0.x * _Op1.y - _Op0.y * _Op1.x ); }

		// Logic operators
		public static bool			operator==( Vector4D _Op0, Vector4D _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	true;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	false;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	false;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) + (_Op0.w - _Op1.w)*(_Op0.w - _Op1.w) <= float.Epsilon;
		}
		public static bool			operator!=( Vector4D _Op0, Vector4D _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	false;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	true;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	true;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) + (_Op0.w - _Op1.w)*(_Op0.w - _Op1.w) > float.Epsilon;
		}
		public static bool			operator<( Vector4D _Op0, Vector4D _Op1 )			{ return _Op0.x < _Op1.x && _Op0.y < _Op1.y && _Op0.z < _Op1.z && _Op0.w < _Op1.w; }
		public static bool			operator<=( Vector4D _Op0, Vector4D _Op1 )			{ return _Op0.x < _Op1.x + float.Epsilon && _Op0.y < _Op1.y + float.Epsilon && _Op0.z < _Op1.z + float.Epsilon && _Op0.w < _Op1.w + float.Epsilon; }
		public static bool			operator>( Vector4D _Op0, Vector4D _Op1 )			{ return _Op0.x > _Op1.x && _Op0.y > _Op1.y && _Op0.z > _Op1.z && _Op0.w > _Op1.w; }
		public static bool			operator>=( Vector4D _Op0, Vector4D _Op1 )			{ return _Op0.x > _Op1.x - float.Epsilon && _Op0.y > _Op1.y - float.Epsilon && _Op0.z > _Op1.z - float.Epsilon && _Op0.w > _Op1.w - float.Epsilon; }

		#endregion
	}

	// The type converter for the property grid
	public class Vector4DTypeConverter : System.ComponentModel.TypeConverter
	{
		public override bool	CanConvertFrom( System.ComponentModel.ITypeDescriptorContext _Context, System.Type _SourceType )
		{
			if ( _SourceType == typeof(string) ) 
				return	true;

			return	base.CanConvertFrom( _Context, _SourceType );
		}

		public override bool CanConvertTo( System.ComponentModel.ITypeDescriptorContext _Context, System.Type _DestinationType )
		{
			if ( _DestinationType == typeof(string) ) 
				return	true;

			return	base.CanConvertTo( _Context, _DestinationType );
		}

		public override object	ConvertFrom( System.ComponentModel.ITypeDescriptorContext _Context, System.Globalization.CultureInfo _Culture, object _Value )
		{
			if ( _Value is string )
				return	Vector4D.Parse( _Value as string );

			return	base.ConvertFrom( _Context, _Culture, _Value );
		}

		public override object	ConvertTo( System.ComponentModel.ITypeDescriptorContext _Context, System.Globalization.CultureInfo _Culture, object _Value, System.Type _DestinationType )
		{
			if ( _DestinationType == typeof(string) )
				return	(_Value as Vector4D).ToString();

			return	base.ConvertTo( _Context, _Culture, _Value, _DestinationType );
		}

		// Sub-properties
		public override bool GetPropertiesSupported( System.ComponentModel.ITypeDescriptorContext _Context )
		{
			return	true;
		}

		public override System.ComponentModel.PropertyDescriptorCollection	GetProperties( System.ComponentModel.ITypeDescriptorContext _Context, object _Value, System.Attribute[] _Attributes )
		{
			return	System.ComponentModel.TypeDescriptor.GetProperties( typeof(Vector4D), new System.Attribute[] { new System.ComponentModel.BrowsableAttribute( true ) } );
		}
	}
}
