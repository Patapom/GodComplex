using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Matrix3x3.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("m = ({m[0,0]}, {m[0,1]}, {m[0,2]}) | ({m[1,0]}, {m[1,1]}, {m[1,2]}) | ({m[2,0]}, {m[2,1]}, {m[2,2]})")]
    public class Matrix3x3
	{
		#region NESTED TYPES

		public enum INIT_TYPES		
		{
			ZERO,
			IDENTITY,
			ROT_X,
			ROT_Y,
			ROT_Z,
		};

		public enum COEFFS : int	
		{
			A = 0,	B = 1,	C = 2,
			D = 3,	E = 4,	F = 5,
			G = 6,	H = 7,	I = 8
		}

		public class	MatrixException : Exception
		{
			public MatrixException( string _Message ) : base( _Message )			{}
		}

		#endregion

		#region FIELDS

		public float[,]			m = new float[3,3];
		public static int[]		ms_Index = { 0, 1, 2, 0, 1 };				// This array gives the index of the current component

		#endregion

		#region METHODS

		// Constructors
		public						Matrix3x3()										{}
		public						Matrix3x3( float[,] _Source )
		{
			Set( _Source );
		}
		public						Matrix3x3( Matrix3x3 _Source )
		{
			Set( _Source );
		}
		public						Matrix3x3( Matrix4x4 _Source )
		{
			Set( _Source );
		}
		public						Matrix3x3( INIT_TYPES _Init, float _fAngle )
		{
			switch ( _Init )
			{
				case	INIT_TYPES.ZERO:
					MakeZero();
					break;
				case	INIT_TYPES.IDENTITY:
					MakeIdentity();
					break;
				case	INIT_TYPES.ROT_X:
					MakeRotX( _fAngle );
					break;
				case	INIT_TYPES.ROT_Y:
					MakeRotY( _fAngle );
					break;
				case	INIT_TYPES.ROT_Z:
					MakeRotZ( _fAngle );
					break;
			}
		}

		// Access methods
		public Vector				GetRow( int _dwRowIndex )						{ return new Vector( m[_dwRowIndex,0], m[_dwRowIndex, 1], m[_dwRowIndex, 2] ); }
		public Vector				GetRow0()										{ return new Vector( m[0, 0], m[0, 1], m[0, 2] ); }
		public Vector				GetRow1()										{ return new Vector( m[1, 0], m[1, 1], m[1, 2] ); }
		public Vector				GetRow2()										{ return new Vector( m[2, 0], m[2, 1], m[2, 2] ); }
		public Vector				GetScale()										{ return new Vector( new Vector( m[0, 0], m[0, 1], m[0, 2] ).Magnitude(), new Vector( m[1, 0], m[1, 1], m[1, 2] ).Magnitude(), new Vector( m[2, 0], m[2, 1], m[2, 2] ).Magnitude() ); }
		public void					SetRow( int _dwRowIndex, Vector _Row )			{ m[_dwRowIndex, 0] = _Row.x; m[_dwRowIndex, 1] = _Row.y; m[_dwRowIndex, 2] = _Row.z; }
		public void					SetRow0( Vector _Row )							{ m[0, 0] = _Row.x; m[0, 1] = _Row.y; m[0, 2] = _Row.z; }
		public void					SetRow1( Vector _Row )							{ m[1, 0] = _Row.x; m[1, 1] = _Row.y; m[1, 2] = _Row.z; }
		public void					SetRow2( Vector _Row )							{ m[2, 0] = _Row.x; m[2, 1] = _Row.y; m[2, 2] = _Row.z; }
		public void					SetScale( Vector _Scale )						{ m[0, 0] *= _Scale.x; m[1, 1] *= _Scale.y; m[2, 2] *= _Scale.z; }
		public void					Scale( Vector _Scale )							{ m[0, 0] *= _Scale.x; m[0, 1] *= _Scale.x; m[0, 2] *= _Scale.x; m[1, 0] *= _Scale.y; m[1, 1] *= _Scale.y; m[1, 2] *= _Scale.y; m[2, 0] *= _Scale.z; m[2, 1] *= _Scale.z; m[2, 2] *= _Scale.z; }
		public void					Set( float[,] _Source )
		{
			if ( _Source == null )
				return;
			m[0, 0] = _Source[0, 0]; m[0, 1] = _Source[0, 1]; m[0, 2] = _Source[0, 2];
			m[1, 0] = _Source[1, 0]; m[1, 1] = _Source[1, 1]; m[1, 2] = _Source[1, 2];
			m[2, 0] = _Source[2, 0]; m[2, 1] = _Source[2, 1]; m[2, 2] = _Source[2, 2];
		}
		public void					Set( Matrix3x3 _Source )
		{
			if ( _Source == null )
				return;
			m[0, 0] = _Source.m[0, 0]; m[0, 1] = _Source.m[0, 1]; m[0, 2] = _Source.m[0, 2];
			m[1, 0] = _Source.m[1, 0]; m[1, 1] = _Source.m[1, 1]; m[1, 2] = _Source.m[1, 2];
			m[2, 0] = _Source.m[2, 0]; m[2, 1] = _Source.m[2, 1]; m[2, 2] = _Source.m[2, 2];
		}
		public void					Set( Matrix4x4 _Source )
		{
			if ( _Source == null )
				return;
			m[0, 0] = _Source.m[0, 0]; m[0, 1] = _Source.m[0, 1]; m[0, 2] = _Source.m[0, 2];
			m[1, 0] = _Source.m[1, 0]; m[1, 1] = _Source.m[1, 1]; m[1, 2] = _Source.m[1, 2];
			m[2, 0] = _Source.m[2, 0]; m[2, 1] = _Source.m[2, 1]; m[2, 2] = _Source.m[2, 2];
		}

		// Helpers
		public float				Trace()											{ return m[0, 0] + m[1, 1] + m[2, 2]; }
		public Matrix3x3			MakeZero()										{ m[0, 0] = m[0, 1] = m[0, 2] = m[1, 0] = m[1, 1] = m[1, 2] = m[2, 0] = m[2, 1] = m[2, 2] = 0.0f; return this; }
		public Matrix3x3			MakeIdentity()									{ m[0, 1] = m[0, 2] = m[1, 0] = m[1, 2] = m[2, 0] = m[2, 1] = 0.0f; m[0, 0] = m[1, 1] = m[2, 2] = 1.0f; return this; }
		public bool					IsIdentity()									{ if ( m[0, 0] != 1.0f || m[1, 1] != 1.0f || m[2, 2] != 1.0f ) return false; if ( m[0, 1] != 0.0f || m[0, 2] != 0.0f || m[1, 0] != 0.0f || m[1, 2] != 0.0f || m[2, 0] != 0.0f || m[2, 1] != 0.0f ) return false; return true; }
		public Matrix3x3			MakeRotX( float _fAngle )						{ MakeIdentity(); float fCosine = (float) System.Math.Cos( _fAngle ); float fSine = (float) System.Math.Sin( _fAngle ); m[1, 1] = +fCosine; m[1, 2] = +fSine; m[2, 1] = -fSine; m[2, 2] = +fCosine; return this; }
		public Matrix3x3			MakeRotY( float _fAngle )						{ MakeIdentity(); float fCosine = (float) System.Math.Cos( _fAngle ); float fSine = (float) System.Math.Sin( _fAngle ); m[0, 0] = +fCosine; m[0, 2] = -fSine; m[2, 0] = +fSine; m[2, 2] = +fCosine; return this; }
		public Matrix3x3			MakeRotZ( float _fAngle )						{ MakeIdentity(); float fCosine = (float) System.Math.Cos( _fAngle ); float fSine = (float) System.Math.Sin( _fAngle ); m[0, 0] = +fCosine; m[0, 1] = +fSine; m[1, 0] = -fSine; m[1, 1] = +fCosine; return this; }
		public Matrix3x3			MakePYR( float _fPitch, float _fYaw, float _fRoll )	{ Matrix3x3 Pitch = new Matrix3x3( INIT_TYPES.ROT_X, _fPitch ); Matrix3x3 Yaw = new Matrix3x3( INIT_TYPES.ROT_Y, _fYaw ); Matrix3x3 Roll = new Matrix3x3( INIT_TYPES.ROT_Z, _fRoll ); Set( Roll * Yaw * Pitch ); return this; }
		public Vector				GetEuler()
		{
			Vector		Ret = new Vector();
			float		fSinY = System.Math.Min( +1.0f, System.Math.Max( -1.0f, m[0, 2] ) ), fCosY = (float) System.Math.Sqrt( 1.0f - fSinY*fSinY );

			if ( m[0, 0] < 0.0 && m[2, 2] < 0.0 )
				fCosY = -fCosY;

			if ( (float) System.Math.Abs( fCosY ) > float.Epsilon )
			{
				Ret.x = (float)  System.Math.Atan2( m[1, 2] / fCosY, m[2, 2] / fCosY );
				Ret.y = (float) -System.Math.Atan2( fSinY, fCosY );
				Ret.z = (float)  System.Math.Atan2( m[0, 1] / fCosY, m[0, 0] / fCosY );
			}
			else
			{
				Ret.x = (float)  System.Math.Atan2( -m[2, 1], m[1, 1] );
				Ret.y = (float) -System.Math.Asin( fSinY );
				Ret.z = 0.0f;
			}

			return	Ret;
		}
		public void					FromEuler( Vector _EulerAngles )				{ Matrix3x3 MatX = new Matrix3x3( INIT_TYPES.ROT_X, _EulerAngles.x ); Matrix3x3 MatY = new Matrix3x3( INIT_TYPES.ROT_Y, _EulerAngles.y ); Matrix3x3 MatZ = new Matrix3x3( INIT_TYPES.ROT_Z, _EulerAngles.z ); Set( MatX * MatY * MatZ ); }
		public Matrix3x3			Transpose()										{ float fTemp; fTemp = m[1, 0]; m[1, 0] = m[0, 1]; m[0, 1] = fTemp; fTemp = m[2, 0]; m[2, 0] = m[0, 2]; m[0, 2] = fTemp; fTemp = m[2, 1]; m[2, 1] = m[1, 2]; m[1, 2] = fTemp; return this; }
		public float				CoFactor( int _dwRow, int _dwCol )				{ return (m[ms_Index[_dwRow+1], ms_Index[_dwCol+1]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+2]] - m[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*m[ms_Index[_dwRow+1], ms_Index[_dwCol+2]]); }
		public float				Determinant()									{ return (m[0, 0]*m[1, 1]*m[2, 2] + m[0, 1]*m[1, 2]*m[2, 0] + m[0, 2]*m[1, 0]*m[2, 1]) - (m[2, 0]*m[1, 1]*m[0, 2] + m[2, 1]*m[1, 2]*m[0, 0] + m[2, 2]*m[1, 0]*m[0, 1]); }
		public Matrix3x3			Invert()
		{
			float	fDet = Determinant();
			if ( (float) System.Math.Abs(fDet) < float.Epsilon )
				throw new MatrixException( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

			float	fIDet = 1.0f / fDet;

			Matrix3x3	Temp = new Matrix3x3();
			Temp.m[0, 0] = +(m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) * fIDet;
			Temp.m[1, 0] = -(m[1, 0] * m[2, 2] - m[2, 0] * m[1, 2]) * fIDet;
			Temp.m[2, 0] = +(m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]) * fIDet;
			Temp.m[0, 1] = -(m[0, 1] * m[2, 2] - m[2, 1] * m[0, 2]) * fIDet;
			Temp.m[1, 1] = +(m[0, 0] * m[2, 2] - m[2, 0] * m[0, 2]) * fIDet;
			Temp.m[2, 1] = -(m[0, 0] * m[2, 1] - m[2, 0] * m[0, 1]) * fIDet;
			Temp.m[0, 2] = +(m[0, 1] * m[1, 2] - m[1, 1] * m[0, 2]) * fIDet;
			Temp.m[1, 2] = -(m[0, 0] * m[1, 2] - m[1, 0] * m[0, 2]) * fIDet;
			Temp.m[2, 2] = +(m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1]) * fIDet;

			Set( Temp );

			return	this;
		}
		public void					Normalize()
		{
			SetRow0( GetRow0().Normalize() );
			SetRow1( GetRow1().Normalize() );
			SetRow2( GetRow2().Normalize() );
		}

		/// <summary>
		/// Computes the rotation matrix to transform a source vector into a target vector
		/// (routine from Thomas Moller)
		/// </summary>
		/// <param name="_Source">The source vector</param>
		/// <param name="_Target">The target vector</param>
		/// <returns>The rotation matrix to apply that will transform</returns>
		public static Matrix3x3		ComputeRotationMatrix( Vector _Source, Vector _Target )
		{
			WMath.Matrix3x3	Result = new Matrix3x3();
							Result.MakeIdentity();

			float	e = _Source | _Target;
			bool	bReverse = e < 0.0f;
			if ( bReverse )
			{	// Revert target
				_Target = -_Target;
				e = -e;
			}

			if ( e > 1.0f - 0.000001f )
			{
				if ( bReverse )
				{	// Reverse final matrix
 					Result.SetRow0( -Result.GetRow0() );
 					Result.SetRow1( -Result.GetRow1() );
 					Result.SetRow2( -Result.GetRow2() );
				}

				return	Result;					// No rotation needed...
			}

			Vector	Ortho = _Source ^ _Target;

			float	h = 1.0f / (1.0f + e);      // Optimization by Gottfried Chen
			
			Result.SetRow0(	new Vector( e + h * Ortho.x * Ortho.x,
										h * Ortho.x * Ortho.y + Ortho.z,
										h * Ortho.x * Ortho.z - Ortho.y ) );

			Result.SetRow1( new Vector( h * Ortho.x * Ortho.y - Ortho.z,
										e + h * Ortho.y * Ortho.y,
										h * Ortho.y * Ortho.z + Ortho.x ) );

			Result.SetRow2(	new Vector( h * Ortho.x * Ortho.z + Ortho.y,
										h * Ortho.y * Ortho.z - Ortho.x,
										e + h * Ortho.z * Ortho.z ) );

			if ( bReverse )
			{	// Reverse final matrix
 				Result.SetRow0( -Result.GetRow0() );
 				Result.SetRow1( -Result.GetRow1() );
 				Result.SetRow2( -Result.GetRow2() );
			}

			return	Result;
		}

		public override string		ToString()
		{
			return	GetRow0().ToString() + ";" + GetRow1().ToString() + ";" + GetRow2().ToString();
		}

		public static Matrix3x3		Parse( string _Source )
		{
			string[]	Terms = _Source.Split( new char[] { ';' } );

			Matrix3x3	Result = new Matrix3x3();
						Result.m[0,0] = float.Parse( Terms[3 * 0 + 0] );
						Result.m[0,1] = float.Parse( Terms[3 * 0 + 1] );
						Result.m[0,2] = float.Parse( Terms[3 * 0 + 2] );
						Result.m[1,0] = float.Parse( Terms[3 * 1 + 0] );
						Result.m[1,1] = float.Parse( Terms[3 * 1 + 1] );
						Result.m[1,2] = float.Parse( Terms[3 * 1 + 2] );
						Result.m[2,0] = float.Parse( Terms[3 * 2 + 0] );
						Result.m[2,1] = float.Parse( Terms[3 * 2 + 1] );
						Result.m[2,2] = float.Parse( Terms[3 * 2 + 2] );

			return	Result;
		}

		// Cast operators
		public static explicit		operator Quat( Matrix3x3 _Mat )
		{
			float	fTrace = _Mat.Trace(), s;
			Quat	q = new Quat();

			if ( fTrace > 0.0f )
			{
				s = (float) System.Math.Sqrt( fTrace + 1.0f );
				q.qs = s * 0.5f;
				s = 0.5f / s;
				q.qv.x = (_Mat.m[2, 1] - _Mat.m[1, 2]) * s;
				q.qv.y = (_Mat.m[0, 2] - _Mat.m[2, 0]) * s;
				q.qv.z = (_Mat.m[1, 0] - _Mat.m[0, 1]) * s;
			}
			else
			{
				int		i,  j,  k;
				int		mi, mj, mk;

				i = (int) Quat.COMPONENTS.I;
				mi = 0;
				if ( _Mat.m[1, 1] > _Mat.m[0, 0] )
				{
					i = (int) Quat.COMPONENTS.J;
					mi = 1;
				}
				if ( _Mat.m[2, 2] > _Mat.m[i, i] )
				{
					i = (int) Quat.COMPONENTS.K;
					mi = 2;
				}
				j = (int) Quat.ms_Next[i];
				mj = (mi+1) % 3;
				k = (int) Quat.ms_Next[j];
				mk = (mj+1) % 3;

				s = (float) System.Math.Sqrt( (_Mat.m[mi, mi] - (_Mat.m[mj, mj] + _Mat.m[mk, mk])) + 1.0f );
				q[i] = s * 0.5f;

				if ( System.Math.Abs( s ) > float.Epsilon )
					s = 0.5f /s;

				q.qs = (_Mat.m[mj, mk] - _Mat.m[mk, mj]) * s;
				q[j] = (_Mat.m[mj, mi] + _Mat.m[mi, mj]) * s;
				q[k] = (_Mat.m[mk, mi] + _Mat.m[mi, mk]) * s;
			}

			return	q;
		}

		// Indexers
		public float				this[int _i, int _j]
		{
			get { return m[_i, _j]; }
			set { m[_i, _j] = value; }
		}

		public float				this[int _CoeffIndex]
		{
			get
			{
				switch ( _CoeffIndex )
				{
					case	(int) COEFFS.A:
						return	m[0, 0];
					case	(int) COEFFS.B:
						return	m[0, 1];
					case	(int) COEFFS.C:
						return	m[0, 2];
					case	(int) COEFFS.D:
						return	m[1, 0];
					case	(int) COEFFS.E:
						return	m[1, 1];
					case	(int) COEFFS.F:
						return	m[1, 2];
					case	(int) COEFFS.G:
						return	m[2, 0];
					case	(int) COEFFS.H:
						return	m[2, 1];
					case	(int) COEFFS.I:
						return	m[2, 2];
					default:
						throw new MatrixException( "Coefficient index out of range!" );
				}
			}

			set
			{
				switch ( _CoeffIndex )
				{
					case	(int) COEFFS.A:
						m[0, 0] = value;
						break;
					case	(int) COEFFS.B:
						m[0, 1] = value;
						break;
					case	(int) COEFFS.C:
						m[0, 2] = value;
						break;
					case	(int) COEFFS.D:
						m[1, 0] = value;
						break;
					case	(int) COEFFS.E:
						m[1, 1] = value;
						break;
					case	(int) COEFFS.F:
						m[1, 2] = value;
						break;
					case	(int) COEFFS.G:
						m[2, 0] = value;
						break;
					case	(int) COEFFS.H:
						m[2, 1] = value;
						break;
					case	(int) COEFFS.I:
						m[2, 2] = value;
						break;
					default:
						throw new MatrixException( "Coefficient index out of range!" );
				}
			}
		}

		// Arithmetic operators
		public static Matrix3x3		operator+( Matrix3x3 _Op0, Matrix3x3 _Op1 )
		{
			Matrix3x3	Ret = new Matrix3x3();
			Ret.m[0, 0] = _Op0.m[0, 0] + _Op1.m[0, 0];
			Ret.m[0, 1] = _Op0.m[0, 1] + _Op1.m[0, 1];
			Ret.m[0, 2] = _Op0.m[0, 2] + _Op1.m[0, 2];
			Ret.m[1, 0] = _Op0.m[1, 0] + _Op1.m[1, 0];
			Ret.m[1, 1] = _Op0.m[1, 1] + _Op1.m[1, 1];
			Ret.m[1, 2] = _Op0.m[1, 2] + _Op1.m[1, 2];
			Ret.m[2, 0] = _Op0.m[2, 0] + _Op1.m[2, 0];
			Ret.m[2, 1] = _Op0.m[2, 1] + _Op1.m[2, 1];
			Ret.m[2, 2] = _Op0.m[2, 2] + _Op1.m[2, 2];
			return	Ret;
		}
		public static Matrix3x3		operator-( Matrix3x3 _Op0, Matrix3x3 _Op1 )
		{
			Matrix3x3	Ret = new Matrix3x3();
			Ret.m[0, 0] = _Op0.m[0, 0] - _Op1.m[0, 0];
			Ret.m[0, 1] = _Op0.m[0, 1] - _Op1.m[0, 1];
			Ret.m[0, 2] = _Op0.m[0, 2] - _Op1.m[0, 2];
			Ret.m[1, 0] = _Op0.m[1, 0] - _Op1.m[1, 0];
			Ret.m[1, 1] = _Op0.m[1, 1] - _Op1.m[1, 1];
			Ret.m[1, 2] = _Op0.m[1, 2] - _Op1.m[1, 2];
			Ret.m[2, 0] = _Op0.m[2, 0] - _Op1.m[2, 0];
			Ret.m[2, 1] = _Op0.m[2, 1] - _Op1.m[2, 1];
			Ret.m[2, 2] = _Op0.m[2, 2] - _Op1.m[2, 2];
			return	Ret;
		}
		public static Matrix3x3		operator*( Matrix3x3 _Op0, Matrix3x3 _Op1 )
		{
			Matrix3x3	Ret = new Matrix3x3();
						Ret.m[0, 0] = _Op0.m[0, 0]*_Op1.m[0, 0] + _Op0.m[0, 1]*_Op1.m[1, 0] + _Op0.m[0, 2]*_Op1.m[2, 0];
						Ret.m[0, 1] = _Op0.m[0, 0]*_Op1.m[0, 1] + _Op0.m[0, 1]*_Op1.m[1, 1] + _Op0.m[0, 2]*_Op1.m[2, 1];
						Ret.m[0, 2] = _Op0.m[0, 0]*_Op1.m[0, 2] + _Op0.m[0, 1]*_Op1.m[1, 2] + _Op0.m[0, 2]*_Op1.m[2, 2];

						Ret.m[1, 0] = _Op0.m[1, 0]*_Op1.m[0, 0] + _Op0.m[1, 1]*_Op1.m[1, 0] + _Op0.m[1, 2]*_Op1.m[2, 0];
						Ret.m[1, 1] = _Op0.m[1, 0]*_Op1.m[0, 1] + _Op0.m[1, 1]*_Op1.m[1, 1] + _Op0.m[1, 2]*_Op1.m[2, 1];
						Ret.m[1, 2] = _Op0.m[1, 0]*_Op1.m[0, 2] + _Op0.m[1, 1]*_Op1.m[1, 2] + _Op0.m[1, 2]*_Op1.m[2, 2];

						Ret.m[2, 0] = _Op0.m[2, 0]*_Op1.m[0, 0] + _Op0.m[2, 1]*_Op1.m[1, 0] + _Op0.m[2, 2]*_Op1.m[2, 0];
						Ret.m[2, 1] = _Op0.m[2, 0]*_Op1.m[0, 1] + _Op0.m[2, 1]*_Op1.m[1, 1] + _Op0.m[2, 2]*_Op1.m[2, 1];
						Ret.m[2, 2] = _Op0.m[2, 0]*_Op1.m[0, 2] + _Op0.m[2, 1]*_Op1.m[1, 2] + _Op0.m[2, 2]*_Op1.m[2, 2];

			return	Ret;
		}
		public static Vector		operator*( Matrix3x3 _Op0, Vector _Op1 )
		{
			return	new Vector( _Op0.m[0, 0]*_Op1.x + _Op0.m[0, 1]*_Op1.y + _Op0.m[0, 2]*_Op1.z,
								_Op0.m[1, 0]*_Op1.x + _Op0.m[1, 1]*_Op1.y + _Op0.m[1, 2]*_Op1.z,
								_Op0.m[2, 0]*_Op1.x + _Op0.m[2, 1]*_Op1.y + _Op0.m[2, 2]*_Op1.z );
		}
		public static Matrix3x3		operator*( Matrix3x3 _Op0, float _s )
		{
			Matrix3x3	Ret = new Matrix3x3();
			Ret.m[0, 0] = _Op0.m[0, 0] * _s;
			Ret.m[0, 1] = _Op0.m[0, 1] * _s;
			Ret.m[0, 2] = _Op0.m[0, 2] * _s;
			Ret.m[1, 0] = _Op0.m[1, 0] * _s;
			Ret.m[1, 1] = _Op0.m[1, 1] * _s;
			Ret.m[1, 2] = _Op0.m[1, 2] * _s;
			Ret.m[2, 0] = _Op0.m[2, 0] * _s;
			Ret.m[2, 1] = _Op0.m[2, 1] * _s;
			Ret.m[2, 2] = _Op0.m[2, 2] * _s;
			return	Ret;
		}
		public static Matrix3x3		operator*( float _s, Matrix3x3 _Op0 )
		{
			Matrix3x3	Ret = new Matrix3x3();
			Ret.m[0, 0] = _Op0.m[0, 0] * _s;
			Ret.m[0, 1] = _Op0.m[0, 1] * _s;
			Ret.m[0, 2] = _Op0.m[0, 2] * _s;
			Ret.m[1, 0] = _Op0.m[1, 0] * _s;
			Ret.m[1, 1] = _Op0.m[1, 1] * _s;
			Ret.m[1, 2] = _Op0.m[1, 2] * _s;
			Ret.m[2, 0] = _Op0.m[2, 0] * _s;
			Ret.m[2, 1] = _Op0.m[2, 1] * _s;
			Ret.m[2, 2] = _Op0.m[2, 2] * _s;
			return	Ret;
		}
		public static Matrix3x3		operator/( Matrix3x3 _Op0, float _s )
		{
			float	Is = 1.0f / _s;
			Matrix3x3	Ret = new Matrix3x3();
			Ret.m[0, 0] = _Op0.m[0, 0] * Is;
			Ret.m[0, 1] = _Op0.m[0, 1] * Is;
			Ret.m[0, 2] = _Op0.m[0, 2] * Is;
			Ret.m[1, 0] = _Op0.m[1, 0] * Is;
			Ret.m[1, 1] = _Op0.m[1, 1] * Is;
			Ret.m[1, 2] = _Op0.m[1, 2] * Is;
			Ret.m[2, 0] = _Op0.m[2, 0] * Is;
			Ret.m[2, 1] = _Op0.m[2, 1] * Is;
			Ret.m[2, 2] = _Op0.m[2, 2] * Is;
			return	Ret;
		}


		#endregion
	}
}
