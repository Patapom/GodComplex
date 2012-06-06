using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Plane.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("d = {d} N = ({n.x}, {n.y}, {n.z})")]
    public class Plane
	{
		#region FIELDS

		public Vector		n;
		public float		d;

		#endregion

		#region METHODS

		// Constructors
		public						Plane()														{}
		public						Plane( float _nx, float _ny, float _nz, float _d )			{ n.Set( _nx, _ny, _nz ); }
		public						Plane( Point _p, Vector _n )								{ Set( _p, _n ); }
		public						Plane( Point _p0, Point _p1, Point _p2 )					{ Set( _p0, _p1, _p2 ); }
		public						Plane( Vector _n, float _d )								{ n = _n; d = _d; }

		// Access methods
		public Plane				Zero()														{ n.Zero(); d = 0.0f; return this; }
		public Plane				Set( float _nx, float _ny, float _nz, float _d )			{ n.Set( _nx, _ny, _nz ); d = _d; return this; }
		public Plane				Set( Point _p, Vector _n )									{ n = _n; d = -((Vector) _p ) | n; return this; }
		public Plane				Set( Point _p0, Point _p1, Point _p2 )		
		{
			Vector	v0 = _p1 - _p0, v1 = _p2 - _p0;									// Compute two vectors from the three points

			n = v0 ^ v1;															// Compute the normal to the plane
			n.Normalize();
			d = -(((Vector) _p0) | n);

			return	this;
		}


		public float				Distance( Point _p )										{ return (((Vector) _p ) | n) + d; }
		public bool					Belongs( Point _p )											{ return (float) System.Math.Abs( Distance( _p ) ) < float.Epsilon; }

		// Helpers
			// Intersection between a plane and a ray
		public Point				Intersect( Ray _Ray )
		{
			float	fGradient = n | _Ray.Aim;
			if ( (float) System.Math.Abs( fGradient ) < float.Epsilon )
				return	null;					// It cannot hit! (or very far away at least!)

			_Ray.Length = (-d - (((Vector) _Ray.Pos) | n)) / fGradient;

			return	_Ray.GetHitPos();
		}
			// Intersection between 2 planes
		public bool					Intersect( Plane _p, ref Ray _Ray )
		{
			// Check if both planes are coplanar
			if ( (float) System.Math.Abs( 1.0f - (_p.n | n) ) < float.Epsilon )
				return	false;

			// Let's have fun!
			Point	I;

			_Ray.Pos.Zero();
			_Ray.Aim = n;
			if ( (I = _p.Intersect( _Ray )) == null )
				return	false;

			_Ray.Aim = _p.n;
			if ( (_Ray.Pos = _p.Intersect( _Ray )) == null )
				return	false;

			_Ray.Aim = I - _Ray.Pos;
			if ( (_Ray.Pos = Intersect( _Ray )) == null )
				return	false;

			// We have at least one point belonging to both planes!
			_Ray.Aim = (n ^ _p.n).Normalize();

			return	true;
		}
			// Intersection between 3 planes
		public Point				Intersect( Plane _p0, Plane _p1 )
		{
			// Compute the intersection of 2 planes first, yielding a ray
			Ray		Hit = new Ray();
			if ( !_p0.Intersect( _p1, ref Hit ) )
				return	null;

			// Then compute the intersection of this ray with our plane
			return	Intersect( Hit );
		}

		// Arithmetic operators
		public static Plane			operator*( Plane _Op0, Matrix4x4 _Op1 )
		{
			Plane	Ret = new Plane();

			Vector n2 = _Op0.n * _Op1;
			Ret.d = -(((Vector) (new Point( -_Op0.d * _Op0.n ) * _Op1)) | n2);
			Ret.n = n2;

			return	Ret;
		}

		#endregion
	}
}
