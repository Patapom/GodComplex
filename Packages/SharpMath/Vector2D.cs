using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Vector2D.
	/// </summary>
	[System.ComponentModel.TypeConverter(typeof(Vector2DTypeConverter))]
    [System.Diagnostics.DebuggerDisplay("X = {x} Y = {y}")]
    public class Vector2D
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
		public						Vector2D()										{}
		public						Vector2D( Vector2D _Source )					{ Set( _Source ); }
		public						Vector2D( Vector _Source )						{ Set( _Source ); }
		public						Vector2D( Vector4D _Source )					{ Set( _Source ); }
		public						Vector2D( Point2D _Source )						{ Set( _Source ); }
		public						Vector2D( float _x, float _y )					{ Set( _x, _y ); }
		public						Vector2D( float[] _f )							{ x = _f[0]; y = _f[1]; }

		// Access methods
		public void					Zero()											{ x = y = 0.0f; }
		public void					Set( Vector2D _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( Vector _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( Vector4D _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( Point2D _Source )							{ x = _Source.x; y = _Source.y; }
		public void					Set( float _x, float _y )						{ x = _x; y = _y; }
		public float				Min()											{ return System.Math.Min( x, y ); }
		public void					Min( Vector2D _Op )								{ x = System.Math.Min( x, _Op.x ); y = System.Math.Min( y, _Op.y ); }
		public float				Max()											{ return System.Math.Max( x, y ); }
		public void					Max( Vector2D _Op )								{ x = System.Math.Max( x, _Op.x ); y = System.Math.Max( y, _Op.y ); }
		public float				Sum()											{ return x + y; }
		public float				Product()										{ return x * y; }
		public float				SquareMagnitude()								{ return x * x + y * y; }
		public float				Magnitude()										{ return (float) System.Math.Sqrt( x * x + y * y ); }
		public Vector2D				Normalize()										{ float fINorm = 1.0f / Magnitude(); x *= fINorm; y *= fINorm; return this; }
		public bool					IsNormalized()									{ return System.Math.Abs( SquareMagnitude() - 1.0f ) < float.Epsilon*float.Epsilon; }
		public bool					IsTooSmall()									{ return SquareMagnitude() < float.Epsilon*float.Epsilon; }

		public override string		ToString()
		{
			return	x.ToString() + "; " + y.ToString();
		}

		public static Vector2D		Parse( string _Source )
		{
			string[]	Members = _Source.Split( new System.Char[] { ';' } );
			if ( Members.Length != 2 )
				return	null;

			return	new Vector2D( float.Parse( Members[0] ), float.Parse( Members[1] ) );
		}

		// Cast operators
		public static explicit		operator Vector( Vector2D _Source )				{ return new Vector( _Source ); }
		public static explicit		operator Vector4D( Vector2D _Source )			{ return new Vector4D( _Source ); }
		public static explicit		operator Point2D( Vector2D _Source )			{ return new Point2D( _Source ); }

		// Arithmetic operators
		public static Vector2D		operator-( Vector2D _Op0 )						{ return new Vector2D( -_Op0.x, -_Op0.y ); }
		public static Vector2D		operator+( Vector2D _Op0 )						{ return new Vector2D( +_Op0.x, +_Op0.y ); }
		public static Vector2D		operator+( Vector2D _Op0, Vector2D _Op1 )		{ return new Vector2D( _Op0.x + _Op1.x, _Op0.y + _Op1.y ); }
		public static Vector2D		operator-( Vector2D _Op0, Vector2D _Op1 )		{ return new Vector2D( _Op0.x - _Op1.x, _Op0.y - _Op1.y ); }
		public static Vector2D		operator*( Vector2D _Op0, Vector2D _Op1 )		{ return new Vector2D( _Op0.x * _Op1.x, _Op0.y * _Op1.y ); }
		public static Vector2D		operator*( Vector2D _Op0, float _s )			{ return new Vector2D( _Op0.x * _s, _Op0.y * _s ); }
		public static Vector2D		operator*( float _s, Vector2D _Op0 )			{ return new Vector2D( _Op0.x * _s, _Op0.y * _s ); }
		public static Vector2D		operator/( Vector2D _Op0, float _s )			{ float Is = 1.0f / _s; return new Vector2D( _Op0.x * Is, _Op0.y * Is ); }
		public static Vector2D		operator/( Vector2D _Op0, Vector2D _Op1 )		{ return new Vector2D( _Op0.x / _Op1.x, _Op0.y / _Op1.y ); }
		public static float			operator|( Vector2D _Op0, Vector2D _Op1 )		{ return _Op0.x * _Op1.x + _Op0.y * _Op1.y; }
		public static Vector		operator^( Vector2D _Op0, Vector2D _Op1 )		{ return new Vector( 0.0f, 0.0f, _Op0.x * _Op1.y - _Op0.y * _Op1.x ); }

		// Logic operators
		public static bool			operator==( Vector2D _Op0, Vector2D _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	true;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	false;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	false;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) <= float.Epsilon;
		}
		public static bool			operator!=( Vector2D _Op0, Vector2D _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	false;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	true;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	true;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) > float.Epsilon;
		}
		public static bool			operator<( Vector2D _Op0, Vector2D _Op1 )		{ return _Op0.x < _Op1.x && _Op0.y < _Op1.y; }
		public static bool			operator<=( Vector2D _Op0, Vector2D _Op1 )		{ return _Op0.x < _Op1.x + float.Epsilon && _Op0.y < _Op1.y + float.Epsilon; }
		public static bool			operator>( Vector2D _Op0, Vector2D _Op1 )		{ return _Op0.x > _Op1.x && _Op0.y > _Op1.y; }
		public static bool			operator>=( Vector2D _Op0, Vector2D _Op1 )		{ return _Op0.x > _Op1.x - float.Epsilon && _Op0.y > _Op1.y - float.Epsilon; }

		#endregion
	}

	// The type converter for the property grid
	public class Vector2DTypeConverter : System.ComponentModel.TypeConverter
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
				return	Vector2D.Parse( _Value as string );

			return	base.ConvertFrom( _Context, _Culture, _Value );
		}

		public override object	ConvertTo( System.ComponentModel.ITypeDescriptorContext _Context, System.Globalization.CultureInfo _Culture, object _Value, System.Type _DestinationType )
		{
			if ( _DestinationType == typeof(string) )
				return	(_Value as Vector2D).ToString();

			return	base.ConvertTo( _Context, _Culture, _Value, _DestinationType );
		}

		// Sub-properties
		public override bool GetPropertiesSupported( System.ComponentModel.ITypeDescriptorContext _Context )
		{
			return	true;
		}

		public override System.ComponentModel.PropertyDescriptorCollection	GetProperties( System.ComponentModel.ITypeDescriptorContext _Context, object _Value, System.Attribute[] _Attributes )
		{
			return	System.ComponentModel.TypeDescriptor.GetProperties( typeof(Vector2D), new System.Attribute[] { new System.ComponentModel.BrowsableAttribute( true ) } );
		}
	}
}
