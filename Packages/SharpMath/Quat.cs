using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Quat.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("W = {qs} I = {qv.x} J = {qv.y} K = {qv.z}")]
    public class Quat
	{
		#region NESTED TYPES

		public enum	COMPONENTS : int	{
										S = 0,
										I = 1,
										J = 2,
										K = 3,
										}

		public class	QuatException : Exception
		{
			public					QuatException( string _Message ) : base( _Message )		{}
		}

		#endregion

		#region FIELDS

		public float			qs = 0.0f;
		public Vector			qv = new Vector( 0.0f, 0.0f, 0.0f );

		public static COMPONENTS[]	ms_Next = {
												COMPONENTS.S,
												COMPONENTS.J,
												COMPONENTS.K,
												COMPONENTS.I
											  };							// This array gives the index of the next component of the complex part of the quaternion (borrowed from 3DS-MAX sources)

		#endregion

		#region METHODS

		// Constructors
		public						Quat()													{}
		public						Quat( Quat _q )											{ qs = _q.qs; qv.Set( _q.qv ); }
		public						Quat( float _s, float _i, float _j, float _k )			{ qs = _s; qv.Set( _i, _j, _k ); }
		public						Quat( float[] _f )										{ qs = _f[0]; qv.x = _f[1]; qv.y = _f[2]; qv.z = _f[3]; }
		public						Quat( float _s, Vector _v )								{ qs = _s; qv.Set( _v ); }
		public						Quat( AngleAxis _aa )
		{
			qs = (float) System.Math.Cos( 0.5f * _aa.Angle );
			qv = (float) System.Math.Sin( 0.5f * _aa.Angle ) * _aa.Axis;
		}

		// Access methods
		public void					Zero()													{ qs = 0.0f; qv.Zero(); }
		public void					Set( float _s, float _i, float _j, float _k )			{ qs = _s; qv.x = _i; qv.y = _j; qv.z = _k; }
		public void					Set( float _s, Vector _v )								{ qs = _s; qv.Set( _v ); }
		public void					Set( Quat _q )											{ qs = _q.qs; qv.Set( _q.qv ); }
		public void					Log()
		{
			// If q = cos(A)+sin(A)*(x*i+y*j+z*k) where (x,y,z) is unit length, then
			// log(q) = A*(x*i+y*j+z*k).  If sin(A) is near zero, use log(q) =
			// sin(A)*(x*i+y*j+z*k) since sin(A)/A has limit 1.

			if ( (float) System.Math.Abs( qs ) < 1.0f )
			{
				float	fAngle = (float) System.Math.Acos( qs ), fSine = (float) System.Math.Sin( fAngle );
				if ( System.Math.Abs( fSine ) >= float.Epsilon )
				{
					float	fCoeff = fAngle / fSine;
					qv.x *= fCoeff;
					qv.y *= fCoeff;
					qv.z *= fCoeff;
				}
			}
			qs = 0.0f;
		}
		public void					LogN()
		{
			float	fTheta, fScale;

			fScale = (float) System.Math.Sqrt( qv.x*qv.x + qv.y*qv.y + qv.z*qv.z );
			fTheta = (float) System.Math.Atan2( fScale, qs );
			if ( fScale > 0.0f )
				fScale = fTheta / fScale;
			qv.x *= fScale;
			qv.y *= fScale;
			qv.z *= fScale;
			qs = 0.0f;
		}
		public void					Exp()
		{
			float	fTheta, fScale;

			fTheta = (float) System.Math.Sqrt( qv.x*qv.x + qv.y*qv.y + qv.z*qv.z );
			fScale = 1.0f;
			if ( fTheta > float.Epsilon )
				fScale = (float) System.Math.Sin( fTheta ) / fTheta;
			qv.x *= fScale;
			qv.y *= fScale;
			qv.z *= fScale;
			qs = (float) System.Math.Cos( fTheta );
		}
		public void					LnDiff( Quat _q )										{ Set( _q / this ); LogN(); }
		public float				SquareMagnitude()										{ return qv.SquareMagnitude() + qs*qs; }
		public float				Magnitude()												{ return (float) System.Math.Sqrt( SquareMagnitude() ); }

		// Helpers
		public void					Normalize()
		{
			float	fNorm = Magnitude();
			if ( fNorm < float.Epsilon || System.Math.Abs( fNorm - 1.0f ) < float.Epsilon )
				return;

			float	fINorm = 1.0f / fNorm;
			qs *= fINorm;
			qv *= fINorm;
		}
		public void					Invert()
		{
			float	fNorm = SquareMagnitude();
			if ( fNorm < float.Epsilon )
				return;

			float	fINorm = -1.0f / fNorm;
			qv.x *=  fINorm;
			qv.y *=  fINorm;
			qv.z *=  fINorm;
			qs   *= -fINorm;
		}
		public void					MakeIdentity()											{ qs = 0.0f; qv.Zero(); }
		public void					MakeOrtho( Vector _Axis )								{ Set( this * new Quat( 0.0f, _Axis ) ); }
		public void					MakeClosest( Quat _q )									{ if ( (this | _q) < 0.0f ) Set( -this ); }
		public void					MakeSLERP( Quat _q0, Quat _q1, float _t )
		{
			float	fCosine = _q0 | _q1, fSine = (float) System.Math.Sqrt( 1.0f - fCosine*fCosine );
			if ( fSine < float.Epsilon )
			{	// Clamp to lowest bound
				Set( _q0 );
				return;
			}

			float	fAngle = (float) System.Math.Atan2( fSine, fCosine ), fInvSine = 1.0f / fSine;

			float	c0 = (float) System.Math.Sin( (1-_t) * fAngle ) * fInvSine;
			float	c1 = (float) System.Math.Sin(   _t   * fAngle ) * fInvSine;

			Set( c0 * _q0 + c1 * _q1 );
		}
		public Quat					SLERP( Quat _q, float _t )
		{
			float	fCosine = this | _q, fSine = (float) System.Math.Sqrt( System.Math.Abs( 1.0f - fCosine*fCosine ) );
			if ( System.Math.Abs( fSine ) < float.Epsilon )
				return	this;

			float	fAngle = (float) System.Math.Atan2( fSine, fCosine ), fInvSine = 1.0f / fSine;

			float	c0 = (float) System.Math.Sin( (1-_t) * fAngle) * fInvSine;
			float	c1 = (float) System.Math.Sin(   _t   * fAngle) * fInvSine;

			return	new Quat( c0 * (this) + c1 * _q );
		}
		public void					MakeShortestSLERP( Quat _q0, Quat _q1, float _t )		{ Quat ShortQ0 = new Quat( _q0 ); ShortQ0.MakeClosest( _q1 ); MakeSLERP( ShortQ0, _q1, _t ); }		// Same as "MakeSlerp" except it makes a SLERP between 2 quaternions on the same hemisphere.
		public void					MakeSQUAD( Quat _q0, Quat _q1, Quat _t0, Quat _t1, float _t )
		{
			Quat	Slerp0 = new Quat();
			Slerp0.MakeSLERP( _q0, _q1, _t );
			Quat	Slerp1 = new Quat();
			Slerp1.MakeSLERP( _t0, _t1, _t );
			MakeSLERP( Slerp0, Slerp1, 2.0f * _t * (1.0f - _t) );
		}
		public void					MakeSQUADRev( AngleAxis _aa, Quat _q0, Quat _q1, Quat _t0, Quat _t1, float _t )
		{
			float	s, v;
			float	fOmega = _aa.Angle * 0.5f;
			float	fNbRevs = 0.0f;
			Quat	pp, qq;

			if ( fOmega < System.Math.PI - float.Epsilon )
			{
				MakeSQUAD( _q0, _q1, _t0, _t1, _t );
				return;
			}

			while ( fOmega > System.Math.PI - float.Epsilon )
			{
				fOmega -= (float) System.Math.PI;
				fNbRevs += 1.0f;
			}

			if ( fOmega < 0.0f )
				fOmega = 0.0f;

			s = _t * _aa.Angle / (float) System.Math.PI;					// 2t(omega + N.PI) / PI
	
			if ( s < 1.0f )
			{
				pp = _q0;
				pp.MakeOrtho( _aa.Axis );
				MakeSQUAD( _q0, pp, _t0, pp, s );	// In first 90 degrees
			}
			else if ( ( v = s + 1.0f - 2.0f * (fNbRevs + ( fOmega / (float) System.Math.PI )) ) <= 0.0f )
			{	// middle part, on great circle(p,q)
				while ( s >= 2.0f )
					s -= 2.0f;
				pp = _q0;
				pp.MakeOrtho( _aa.Axis );
				MakeSLERP( _q0, pp, s );
			}
			else
			{	// in last 90 degrees
				qq = _q0;
				qq.MakeOrtho( _aa.Axis );
				qq = -qq;
				MakeSQUAD( qq, _q1, qq, _t1, v );
			}
		}

		// Cast operators
		public static explicit		operator AngleAxis( Quat _Source )						{ return new AngleAxis( _Source ); }
		public static explicit		operator Matrix4x4( Quat _Source )
		{
			Matrix4x4	Ret = (new Matrix4x4()).MakeIdentity();

			float	xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;

			Quat	q = new Quat( _Source );
			q.Normalize();		// A cast to a matrix only works with normalized quaternions!

			xs = 2.0f * q.qv.x;	ys = 2.0f * q.qv.y;	zs = 2.0f * q.qv.z;

			wx = q.qs * xs;		wy = q.qs * ys;		wz = q.qs * zs;
			xx = q.qv.x * xs;	xy = q.qv.x * ys;	xz = q.qv.x * zs;
			yy = q.qv.y * ys;	yz = q.qv.y * zs;	zz = q.qv.z * zs;

			Ret[(int) Matrix4x4.COEFFS.A] = 1.0f -	yy - zz;
			Ret[(int) Matrix4x4.COEFFS.E] =			xy - wz;
			Ret[(int) Matrix4x4.COEFFS.I] =			xz + wy;
			Ret[(int) Matrix4x4.COEFFS.M] = 0.0f;

			Ret[(int) Matrix4x4.COEFFS.B] =			xy + wz;
			Ret[(int) Matrix4x4.COEFFS.F] = 1.0f -	xx - zz;
			Ret[(int) Matrix4x4.COEFFS.J] =			yz - wx;
			Ret[(int) Matrix4x4.COEFFS.N] = 0.0f;

			Ret[(int) Matrix4x4.COEFFS.C] =			xz - wy;
			Ret[(int) Matrix4x4.COEFFS.G] =			yz + wx;
			Ret[(int) Matrix4x4.COEFFS.K] = 1.0f -	xx - yy;
			Ret[(int) Matrix4x4.COEFFS.O] = 0.0f;

			return	Ret;
		}

		// Indexers
		public float				this[int _Index]
		{
			get
			{
				switch ( _Index )
				{
					case	(int) COMPONENTS.S:
						return	qs;
					case	(int) COMPONENTS.I:
						return	qv.x;
					case	(int) COMPONENTS.J:
						return	qv.y;
					case	(int) COMPONENTS.K:
						return	qv.z;
					default:
						throw new QuatException( "Index out of range!" );
				}
			}

			set
			{
				switch ( _Index )
				{
					case	(int) COMPONENTS.S:
						qs = value;
						break;
					case	(int) COMPONENTS.I:
						qv.x = value;
						break;
					case	(int) COMPONENTS.J:
						qv.y = value;
						break;
					case	(int) COMPONENTS.K:
						qv.z = value;
						break;
					default:
						throw new QuatException( "Index out of range!" );
				}
			}
		}

		// Arithmetic operators
		public static Quat			operator-( Quat _Op )									{ return new Quat( -_Op.qs, -_Op.qv ); }
		public static Quat			operator+( Quat _Op0, Quat _Op1 )						{ return new Quat( _Op0.qs + _Op1.qs, _Op0.qv + _Op1.qv ); }
		public static Quat			operator*( Quat _Op0, Quat _Op1 )						{ return new Quat( (_Op0.qs * _Op1.qs) - (_Op0.qv | _Op1.qv), (_Op0.qs * _Op1.qv) + (_Op0.qv * _Op1.qs) + (_Op0.qv ^ _Op1.qv) ); }
		public static Quat			operator*( Quat _Op0, float _s )						{ return new Quat( _Op0.qs * _s, _Op0.qv * _s ); }
		public static Quat			operator*( float _s, Quat _Op0 )						{ return new Quat( _Op0.qs * _s, _Op0.qv * _s ); }
		public static Quat			operator/( Quat _Op0, Quat _Op1 )						{ Quat InvQuat = new Quat( _Op1 ); InvQuat.Invert(); return InvQuat * _Op0; }
		public static Quat			operator/( Quat _Op0, float _s )						{ float Is = 1.0f / _s; return new Quat( _Op0.qs * Is, _Op0.qv * Is ); }
		public static float			operator|( Quat _Op0, Quat _Op1 )						{ return (_Op0.qs * _Op1.qs) + (_Op0.qv | _Op1.qv); }

		// Logic operators
		public static bool			operator==( Quat _Op0, Quat _Op1 )						{ return _Op0.qv == _Op1.qv && (float) System.Math.Abs( _Op0.qs - _Op1.qs ) <= float.Epsilon; }
		public static bool			operator!=( Quat _Op0, Quat _Op1 )						{ return _Op0.qv != _Op1.qv || (float) System.Math.Abs( _Op0.qs - _Op1.qs ) > float.Epsilon; }

		#endregion
	}
}