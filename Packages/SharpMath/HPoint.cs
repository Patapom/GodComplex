using System;

namespace WMath
{
	/// <summary>
	/// Summary description for HPoint.
	/// </summary>
	[System.ComponentModel.TypeConverter(typeof(HPointTypeConverter))]
	public class HPoint
	{
		public float			x, y, z, w;

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

		// Constructors
		public						HPoint()											{}
		public						HPoint( HPoint _Source )							{ Set( _Source ); }
		public						HPoint( Point _Source )								{ x = _Source.x; y = _Source.y; z = _Source.z; w = 1.0f; }
		public						HPoint( Vector _Source )							{ x = _Source.x; y = _Source.y; z = _Source.z; w = 0.0f; }
		public						HPoint( float _x, float _y, float _z, float _w )	{ x = _x; y = _y; z = _z; w = _w; }
		public						HPoint( float[] _f )								{ x = _f[0]; y = _f[1]; z = _f[2]; w = _f[3]; }

		// Access methods
		public void					Zero()												{ x = y = z = 0.0f; }
		public void					Set( HPoint _Source )								{ x = _Source.x; y = _Source.y; z = _Source.z; w = _Source.w; }
		public void					Set( float _x, float _y, float _z, float _w )		{ x = _x; y = _y; z = _z; w = _w; }
		public void					Add( float _x, float _y, float _z, float _w )		{ x += _x; y += _y; z += _z; w = _w; }
		public float				Min()												{ return System.Math.Min( x, System.Math.Min( y, z ) ); }
		public void					Min( HPoint _Op )									{ x = System.Math.Min( x, _Op.x ); y = System.Math.Min( y, _Op.y ); z = System.Math.Min( z, _Op.z ); }
		public float				Max()												{ return System.Math.Max( x, System.Math.Max( y, z ) ); }
		public void					Max( HPoint _Op )									{ x = System.Math.Max( x, _Op.x ); y = System.Math.Max( y, _Op.y ); z = System.Math.Max( z, _Op.z ); }
		public float				InnerSum()											{ return x + y + z + w; }
		public float				InnerProduct()										{ return x * y * z * w; }
		public float				SquareDistance()									{ return x * x + y * y + z * z + w * w; }
		public float				Distance()											{ return (float) System.Math.Sqrt( x * x + y * y + z * z + w * w ); }

		public override string		ToString()
		{
			return	x.ToString() + "; " + y.ToString() + "; " + z.ToString() + "; " + w.ToString();
		}

		public static HPoint			Parse( string _Source )
		{
			string[]	Members = _Source.Split( new System.Char[] { ';' } );
			if ( Members.Length != 4 )
				return	null;

			return	new HPoint(	float.Parse( Members[0] ),
								float.Parse( Members[1] ),
								float.Parse( Members[2] ),
								float.Parse( Members[3] )
							  );
		}

		// Cast operators
		public static explicit		operator Vector( HPoint _Source )				{ float fIw = 1.0f / _Source.w; return new Vector( _Source.x * fIw, _Source.y * fIw, _Source.z * fIw ); }
		public static explicit		operator Point( HPoint _Source )				{ float fIw = 1.0f / _Source.w; return new Point( _Source.x * fIw, _Source.y * fIw, _Source.z * fIw ); }

		public float				this[int _Index]
		{
			get { return _Index == 0 ? x : (_Index == 1 ? y : (_Index == 2 ? z : (_Index == 3 ? w : 0.0f))); }
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
		public static HPoint		operator-( HPoint _Op0 )						{ return new HPoint( -_Op0.x, -_Op0.y, -_Op0.z, -_Op0.w ); }
		public static HPoint		operator+( HPoint _Op0 )						{ return new HPoint( +_Op0.x, +_Op0.y, +_Op0.z, +_Op0.w ); }
		public static HPoint		operator+( HPoint _Op0, Vector _Op1 )			{ return new HPoint( _Op0.x + _Op1.x, _Op0.y + _Op1.y, _Op0.z + _Op1.z, _Op0.w ); }
		public static HPoint		operator-( HPoint _Op0, Vector _Op1 )			{ return new HPoint( _Op0.x - _Op1.x, _Op0.y - _Op1.y, _Op0.z - _Op1.z, _Op0.w ); }
		public static Vector		operator-( HPoint _Op0, HPoint _Op1 )			{ return new Vector( _Op0.x - _Op1.x, _Op0.y - _Op1.y, _Op0.z - _Op1.z ); }
		public static HPoint		operator*( Vector _Op0, HPoint _Op1 )			{ return new HPoint( _Op0.x * _Op1.x, _Op0.y * _Op1.y, _Op0.z * _Op1.z, 0.0f ); }
		public static HPoint		operator*( HPoint _Op0, float _s )				{ return new HPoint( _Op0.x * _s, _Op0.y * _s, _Op0.z * _s, _Op0.w * _s ); }
		public static HPoint		operator*( float _s, HPoint _Op0 )				{ return new HPoint( _Op0.x * _s, _Op0.y * _s, _Op0.z * _s, _Op0.w * _s ); }
		public static HPoint		operator*( HPoint _Op0, Matrix4x4 _Op1 )
		{
			return new HPoint(	_Op0.x * _Op1.m[0,0] + _Op0.y * _Op1.m[1,0] + _Op0.z * _Op1.m[2,0] + _Op0.w * _Op1.m[3,0],
								_Op0.x * _Op1.m[0,1] + _Op0.y * _Op1.m[1,1] + _Op0.z * _Op1.m[2,1] + _Op0.w * _Op1.m[3,1],
								_Op0.x * _Op1.m[0,2] + _Op0.y * _Op1.m[1,2] + _Op0.z * _Op1.m[2,2] + _Op0.w * _Op1.m[3,2],
								_Op0.x * _Op1.m[0,3] + _Op0.y * _Op1.m[1,3] + _Op0.z * _Op1.m[2,3] + _Op0.w * _Op1.m[3,3]
							);
		}
		public static HPoint			operator*( Matrix4x4 _Op1, HPoint _Op0 )
		{
			return new HPoint(	_Op0.x * _Op1.m[0,0] + _Op0.y * _Op1.m[0,1] + _Op0.z * _Op1.m[0,2] + _Op0.w * _Op1.m[0,3],
								_Op0.x * _Op1.m[1,0] + _Op0.y * _Op1.m[1,1] + _Op0.z * _Op1.m[1,2] + _Op0.w * _Op1.m[1,3],
								_Op0.x * _Op1.m[2,0] + _Op0.y * _Op1.m[2,1] + _Op0.z * _Op1.m[2,2] + _Op0.w * _Op1.m[2,3],
								_Op0.x * _Op1.m[3,0] + _Op0.y * _Op1.m[3,1] + _Op0.z * _Op1.m[3,2] + _Op0.w * _Op1.m[3,3]
							);
		}

		// Special "optimized" operator for in-place multiplication
		public static HPoint			operator^( HPoint _Op0, Matrix4x4 _Op1 )
		{
			float	TempX = _Op0.x, TempY = _Op0.y, TempZ = _Op0.z, TempW = _Op0.w;
			_Op0.x = TempX * _Op1.m[0,0] + TempY * _Op1.m[1,0] + TempZ * _Op1.m[2,0] + TempW * _Op1.m[3,0];
			_Op0.y = TempX * _Op1.m[0,1] + TempY * _Op1.m[1,1] + TempZ * _Op1.m[2,1] + TempW * _Op1.m[3,1];
			_Op0.z = TempX * _Op1.m[0,2] + TempY * _Op1.m[1,2] + TempZ * _Op1.m[2,2] + TempW * _Op1.m[3,2];
			_Op0.w = TempX * _Op1.m[0,3] + TempY * _Op1.m[1,3] + TempZ * _Op1.m[2,3] + TempW * _Op1.m[3,3];

			return	_Op0;
		}
		public static HPoint		operator/( HPoint _Op0, float _s )				{ float Is = 1.0f / _s; return new HPoint( _Op0.x * Is, _Op0.y * Is, _Op0.z * Is, _Op0.w * Is ); }

		// Logic operators
		public static bool			operator==( HPoint _Op0, HPoint _Op1 )			{ return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) + (_Op0.w - _Op1.w)*(_Op0.w - _Op1.w) <= float.Epsilon*float.Epsilon; }
		public static bool			operator!=( HPoint _Op0, HPoint _Op1 )			{ return (_Op0.x - _Op1.x)*(_Op0.x - _Op1.x) + (_Op0.y - _Op1.y)*(_Op0.y - _Op1.y) + (_Op0.z - _Op1.z)*(_Op0.z - _Op1.z) + (_Op0.w - _Op1.w)*(_Op0.w - _Op1.w) >= float.Epsilon*float.Epsilon; }
		public static bool			operator<( HPoint _Op0, HPoint _Op1 )			{ return _Op0.x < _Op1.x && _Op0.y < _Op1.y && _Op0.z < _Op1.z && _Op0.w < _Op1.w; }
		public static bool			operator<=( HPoint _Op0, HPoint _Op1 )			{ return _Op0.x < _Op1.x + float.Epsilon && _Op0.y < _Op1.y + float.Epsilon && _Op0.z < _Op1.z + float.Epsilon && _Op0.w < _Op1.w + float.Epsilon; }
		public static bool			operator>( HPoint _Op0, HPoint _Op1 )			{ return _Op0.x > _Op1.x && _Op0.y > _Op1.y && _Op0.z > _Op1.z && _Op0.w > _Op1.w; }
		public static bool			operator>=( HPoint _Op0, HPoint _Op1 )			{ return _Op0.x > _Op1.x - float.Epsilon && _Op0.y > _Op1.y - float.Epsilon && _Op0.z > _Op1.z - float.Epsilon && _Op0.w > _Op1.w + float.Epsilon; }
	}

	// The type converter for the property grid
	public class HPointTypeConverter : System.ComponentModel.TypeConverter
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
				return	HPoint.Parse( _Value as string );

			return	base.ConvertFrom( _Context, _Culture, _Value );
		}

		public override object	ConvertTo( System.ComponentModel.ITypeDescriptorContext _Context, System.Globalization.CultureInfo _Culture, object _Value, System.Type _DestinationType )
		{
			if ( _DestinationType == typeof(string) )
				return	(_Value as HPoint).ToString();

			return	base.ConvertTo( _Context, _Culture, _Value, _DestinationType );
		}

		// Sub-properties
		public override bool GetPropertiesSupported( System.ComponentModel.ITypeDescriptorContext _Context )
		{
			return	true;
		}

		public override System.ComponentModel.PropertyDescriptorCollection	GetProperties( System.ComponentModel.ITypeDescriptorContext _Context, object _Value, System.Attribute[] _Attributes )
		{
			return	System.ComponentModel.TypeDescriptor.GetProperties( typeof(HPoint), new System.Attribute[] { new System.ComponentModel.BrowsableAttribute( true ) } );
		}
	}
}
