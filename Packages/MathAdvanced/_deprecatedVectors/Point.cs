using System;

namespace SharpMath
{
	/// <summary>
	/// Summary description for Point.
	/// </summary>
	[System.ComponentModel.TypeConverter(typeof(PointTypeConverter))]
    [System.Diagnostics.DebuggerDisplay("X = {x} Y = {y} Z = {z}")]
    public class float3
	{
		#region FIELDS

		public float			x, y, z;

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

		public static float3		Zero	{ get { return new float3( 0, 0, 0 ); } }
		public static float3		One		{ get { return new float3( 1, 1, 1 ); } }
		public static float3		UnitX	{ get { return new float3( 1, 0, 0 ); } }
		public static float3		UnitY	{ get { return new float3( 0, 1, 0 ); } }
		public static float3		UnitZ	{ get { return new float3( 0, 0, 1 ); } }

		#endregion

		#region METHODS

		// Constructors
		public						float3()											{}
		public						float3( Point2D _Source )						{ Set( _Source ); }
		public						float3( float3 _Source )							{ Set( _Source ); }
		public						float3( Point4D _Source )						{ Set( _Source ); }
		public						float3( float3 _Source )							{ Set( _Source ); }
		public						float3( float _x, float _y, float _z )			{ Set( _x, _y, _z ); }
		public						float3( float[] _f )								{ Set( _f ); }

		// Access methods
		public void					MakeZero()											{ x = y = z = 0.0f; }
		public void					Set( Point2D _Source )							{ x = _Source.x; y = _Source.y; z = 0.0f; }
		public void					Set( float3 _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; }
		public void					Set( Point4D _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; }
		public void					Set( float3 _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; }
		public void					Set( float _x, float _y, float _z )				{ x = _x; y = _y; z = _z; }
		public void					Set( float[] _f )								{ x = _f[0]; y = _f[1]; z = _f[2]; }
		public void					Add( float _x, float _y, float _z )				{ x += _x; y += _y; z += _z; }
		public float				Min()											{ return System.Math.Min( x, System.Math.Min( y, z ) ); }
		public void					Min( float3 _Op )								{ x = System.Math.Min( x, _Op.x ); y = System.Math.Min( y, _Op.y ); z = System.Math.Min( z, _Op.z ); }
		public float				Max()											{ return System.Math.Max( x, System.Math.Max( y, z ) ); }
		public void					Max( float3 _Op )								{ x = System.Math.Max( x, _Op.x ); y = System.Math.Max( y, _Op.y ); z = System.Math.Max( z, _Op.z ); }
		public float				Sum()											{ return x + y + z; }
		public float				Product()										{ return x * y * z; }
		public float				SquareDistance()								{ return x * x + y * y + z * z; }
		public float				Distance()										{ return (float) System.Math.Sqrt( x * x + y * y + z * z ); }

		public override string		ToString()
		{
			return	x.ToString() + "; " + y.ToString() + "; " + z.ToString();
		}

		public static float3			Parse( string _Source )
		{
			string[]	Members = _Source.Split( new System.Char[] { ';' } );
			if ( Members.Length != 3 )
				return	null;

			return	new float3(	float.Parse( Members[0] ),
								float.Parse( Members[1] ),
								float.Parse( Members[2] )
							  );
		}

		// Cast operators
		public static explicit		operator float3( float3 _Source )				{ return new float3( _Source ); }
		public static explicit		operator Point2D( float3 _Source )				{ return new Point2D( _Source ); }
		public static explicit		operator Point4D( float3 _Source )				{ return new Point4D( _Source ); }

		public float				this[int _Index]
		{
			get { return _Index == 0 ? x : (_Index == 1 ? y : (_Index == 2 ? z : 0.0f)); }
			set
			{
				if ( _Index == 0 )
					x = value;
				else if ( _Index == 1 )
					y = value;
				else if ( _Index == 2 )
					z = value;
			}
		}

		// Arithmetic operators
		public static float3			operator-( float3 _Op0 )							{ return new float3( -_Op0.x, -_Op0.y, -_Op0.z ); }
		public static float3			operator+( float3 _Op0 )							{ return new float3( +_Op0.x, +_Op0.y, +_Op0.z ); }
		public static float3			operator+( float3 _Op0, float3 _Op1 )			{ return new float3( _Op0.x + _Op1.x, _Op0.y + _Op1.y, _Op0.z + _Op1.z ); }
		public static float3			operator-( float3 _Op0, float3 _Op1 )			{ return new float3( _Op0.x - _Op1.x, _Op0.y - _Op1.y, _Op0.z - _Op1.z ); }
		public static float3		operator-( float3 _Op0, float3 _Op1 )				{ return new float3( _Op0.x - _Op1.x, _Op0.y - _Op1.y, _Op0.z - _Op1.z ); }
		public static float3			operator*( float3 _Op0, float3 _Op1 )			{ return new float3( _Op0.x * _Op1.x, _Op0.y * _Op1.y, _Op0.z * _Op1.z ); }
		public static float3			operator*( float3 _Op0, float _s )				{ return new float3( _Op0.x * _s, _Op0.y * _s, _Op0.z * _s ); }
		public static float3			operator*( float _s, float3 _Op0 )				{ return new float3( _Op0.x * _s, _Op0.y * _s, _Op0.z * _s ); }
		public static float3			operator*( float3 _Op0, float4x4 _Op1 )
		{
			return new float3(	_Op0.x * _Op1.m[0,0] + _Op0.y * _Op1.m[1,0] + _Op0.z * _Op1.m[2,0] + _Op1.m[3,0],
								_Op0.x * _Op1.m[0,1] + _Op0.y * _Op1.m[1,1] + _Op0.z * _Op1.m[2,1] + _Op1.m[3,1],
								_Op0.x * _Op1.m[0,2] + _Op0.y * _Op1.m[1,2] + _Op0.z * _Op1.m[2,2] + _Op1.m[3,2]
							);
		}
		public static float3			operator*( float4x4 _Op0, float3 _Op1 )
		{
			return new float3(	_Op1.x * _Op0.m[0,0] + _Op1.y * _Op0.m[0,1] + _Op1.z * _Op0.m[0,2] + _Op0.m[0,3],
								_Op1.x * _Op0.m[1,0] + _Op1.y * _Op0.m[1,1] + _Op1.z * _Op0.m[1,2] + _Op0.m[1,3],
								_Op1.x * _Op0.m[2,0] + _Op1.y * _Op0.m[2,1] + _Op1.z * _Op0.m[2,2] + _Op0.m[2,3]
							);
		}
		public static float3			operator*( float3 _Op0, Matrix3x3 _Op1 )
		{
			return new float3(	_Op0.x * _Op1.m[0,0] + _Op0.y * _Op1.m[1,0] + _Op0.z * _Op1.m[2,0],
								_Op0.x * _Op1.m[0,1] + _Op0.y * _Op1.m[1,1] + _Op0.z * _Op1.m[2,1],
								_Op0.x * _Op1.m[0,2] + _Op0.y * _Op1.m[1,2] + _Op0.z * _Op1.m[2,2]
							);
		}
		public static float3			operator*( Matrix3x3 _Op0, float3 _Op1 )
		{
			return new float3(	_Op1.x * _Op0.m[0,0] + _Op1.y * _Op0.m[0,1] + _Op1.z * _Op0.m[0,2],
								_Op1.x * _Op0.m[1,0] + _Op1.y * _Op0.m[1,1] + _Op1.z * _Op0.m[1,2],
								_Op1.x * _Op0.m[2,0] + _Op1.y * _Op0.m[2,1] + _Op1.z * _Op0.m[2,2]
							);
		}

		// Special "optimized" operator for in-place multiplication
		public static float3			operator^( float3 _Op0, float4x4 _Op1 )
		{
			float	TempX = _Op0.x, TempY = _Op0.y, TempZ = _Op0.z;
			_Op0.x = TempX * _Op1.m[0,0] + TempY * _Op1.m[1,0] + TempZ * _Op1.m[2,0] + _Op1.m[3,0];
			_Op0.y = TempX * _Op1.m[0,1] + TempY * _Op1.m[1,1] + TempZ * _Op1.m[2,1] + _Op1.m[3,1];
			_Op0.z = TempX * _Op1.m[0,2] + TempY * _Op1.m[1,2] + TempZ * _Op1.m[2,2] + _Op1.m[3,2];

			return	_Op0;
		}
		public static float3			operator/( float3 _Op0, float _s )				{ float Is = 1.0f / _s; return new float3( _Op0.x * Is, _Op0.y * Is, _Op0.z * Is ); }

		// Logic operators
		public static bool			operator==( float3 _Op0, float3 _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	true;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	false;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	false;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) <= float.Epsilon;
		}
		public static bool			operator!=( float3 _Op0, float3 _Op1 )
		{
			if ( (_Op0 as object) == null && (_Op1 as object) == null )
				return	false;
			if ( (_Op0 as object) == null && (_Op1 as object) != null )
				return	true;
			if ( (_Op0 as object) != null && (_Op1 as object) == null )
				return	true;

			return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) > float.Epsilon;
		}
		public static bool			operator<( float3 _Op0, float3 _Op1 )				{ return _Op0.x < _Op1.x && _Op0.y < _Op1.y && _Op0.z < _Op1.z; }
		public static bool			operator<=( float3 _Op0, float3 _Op1 )			{ return _Op0.x < _Op1.x + float.Epsilon && _Op0.y < _Op1.y + float.Epsilon && _Op0.z < _Op1.z + float.Epsilon; }
		public static bool			operator>( float3 _Op0, float3 _Op1 )				{ return _Op0.x > _Op1.x && _Op0.y > _Op1.y && _Op0.z > _Op1.z; }
		public static bool			operator>=( float3 _Op0, float3 _Op1 )			{ return _Op0.x > _Op1.x - float.Epsilon && _Op0.y > _Op1.y - float.Epsilon && _Op0.z > _Op1.z - float.Epsilon; }

		#endregion
	}

	// The type converter for the property grid
	public class PointTypeConverter : System.ComponentModel.TypeConverter
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
				return	float3.Parse( _Value as string );

			return	base.ConvertFrom( _Context, _Culture, _Value );
		}

		public override object	ConvertTo( System.ComponentModel.ITypeDescriptorContext _Context, System.Globalization.CultureInfo _Culture, object _Value, System.Type _DestinationType )
		{
			if ( _DestinationType == typeof(string) )
				return	(_Value as float3).ToString();

			return	base.ConvertTo( _Context, _Culture, _Value, _DestinationType );
		}

		// Sub-properties
		public override bool GetPropertiesSupported( System.ComponentModel.ITypeDescriptorContext _Context )
		{
			return	true;
		}

		public override System.ComponentModel.PropertyDescriptorCollection	GetProperties( System.ComponentModel.ITypeDescriptorContext _Context, object _Value, System.Attribute[] _Attributes )
		{
			return	System.ComponentModel.TypeDescriptor.GetProperties( typeof(float3), new System.Attribute[] { new System.ComponentModel.BrowsableAttribute( true ) } );
		}
	}
}
