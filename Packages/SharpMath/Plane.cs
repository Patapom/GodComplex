using System;

namespace SharpMath
{
	/// <summary>
	/// Summary description for Plane.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("d = {d} N = ({n.x}, {n.y}, {n.z})")]
    public class Plane
	{
		#region FIELDS

		public float3		n;
		public float		d;

		#endregion

		#region METHODS

		// Constructors
		public						Plane()														{}
		public						Plane( float _nx, float _ny, float _nz, float _d )			{ n.Set( _nx, _ny, _nz ); }
		public						Plane( float3 _p, float3 _n )								{ Set( _p, _n ); }
		public						Plane( float3 _p0, float3 _p1, float3 _p2 )					{ Set( _p0, _p1, _p2 ); }
		public						Plane( float3 _n, float _d )								{ n = _n; d = _d; }

		// Access methods
		public Plane				Zero()														{ n.Set( 0, 0, 0 ); d = 0.0f; return this; }
		public Plane				Set( float _nx, float _ny, float _nz, float _d )			{ n.Set( _nx, _ny, _nz ); d = _d; return this; }
		public Plane				Set( float3 _p, float3 _n )									{ n = _n; d = -_p.Dot( n ); return this; }
		public Plane				Set( float3 _p0, float3 _p1, float3 _p2 ) {
			// Compute two vectors from the three points
			float3	v0 = _p1 - _p0, v1 = _p2 - _p0;

			// Compute the normal to the plane
			n = v0.Cross( v1 );
			n.Normalize();
			d = -_p0.Dot(n);

			return	this;
		}


		public float				Distance( float3 _p )										{ return _p.Dot(n) + d; }
		public bool					Belongs( float3 _p )										{ return (float) System.Math.Abs( Distance( _p ) ) < float.Epsilon; }

		// Helpers
			// Intersection between a plane and a ray
		public bool					Intersect( Ray _Ray, ref float3 _intersection ) {
			float	fGradient = n.Dot( _Ray.Aim );
			if ( (float) System.Math.Abs( fGradient ) < float.Epsilon )
				return false;	// It cannot hit! (or very far away at least!)

			_Ray.Length = (-d - _Ray.Pos.Dot(n)) / fGradient;

			_intersection = _Ray.GetHitPos();
			return true;
		}
			// Intersection between 2 planes
		public bool					Intersect( Plane _p, Ray _ray ) {
			// Check if both planes are coplanar
			if ( (float) System.Math.Abs( 1.0f - _p.n.Dot(n) ) < float.Epsilon )
				return	false;

			// Let's have fun!
			float3	I = new float3();
			_ray.Pos.Set( 0, 0, 0 );
			_ray.Aim = n;
			if ( !_p.Intersect( _ray, ref I ) )
				return	false;

			_ray.Aim = _p.n;
			if ( !_p.Intersect( _ray, ref I ) )
				return	false;
			_ray.Pos = I;

			_ray.Aim = I - _ray.Pos;
			if ( !Intersect( _ray, ref I ) )
				return	false;
			_ray.Pos = I;

			// We have at least one point belonging to both planes!
			_ray.Aim = n.Cross(_p.n).Normalized;

			return	true;
		}
			// Intersection between 3 planes
		public bool				Intersect( Plane _p0, Plane _p1, ref float3 _intersection ) {
			// Compute the intersection of 2 planes first, yielding a ray
			Ray		Hit = new Ray();
			if ( !_p0.Intersect( _p1, Hit ) )
				return	false;

			// Then compute the intersection of this ray with our plane
			return Intersect( Hit, ref _intersection );
		}

		// Arithmetic operators
		public static Plane			operator*( Plane _Op0, float4x4 _Op1 ) {
			Plane	Ret = new Plane();

			float3 n2 = (float3) (new float4( _Op0.n, 0 ) * _Op1);
			Ret.d = -((float3) (new float4( -_Op0.d * _Op0.n, 0.0f ) * _Op1)).Dot( n2 );
			Ret.n = n2;

			return	Ret;
		}

		#endregion
	}
}
