using System;

namespace WMath
{
	/// <summary>
	/// Summary description for Matrix4x4.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("Trans = ({m[3,0]}, {m[3,1]}, {m[3,2]}) m = ({m[0,0]}, {m[0,1]}, {m[0,2]}) | ({m[1,0]}, {m[1,1]}, {m[1,2]}) | ({m[2,0]}, {m[2,1]}, {m[2,2]})")]
    public class Matrix4x4
	{
		#region CONSTANTS

/*		public static readonly Matrix4x4		ZERO = new Matrix4x4( new float[,] {	{ 0.0f, 0.0f, 0.0f, 0.0f },
																						{ 0.0f, 0.0f, 0.0f, 0.0f },
																						{ 0.0f, 0.0f, 0.0f, 0.0f },
																						{ 0.0f, 0.0f, 0.0f, 0.0f } } );

		public static readonly Matrix4x4		IDENTITY = new Matrix4x4( new float[,] {{ 1.0f, 0.0f, 0.0f, 0.0f },
																						{ 0.0f, 1.0f, 0.0f, 0.0f },
																						{ 0.0f, 0.0f, 1.0f, 0.0f },
																						{ 0.0f, 0.0f, 0.0f, 1.0f } } );
*/
		#endregion

		#region NESTED TYPES

		public enum COEFFS : int	{
										A = 0,	B = 1,	C = 2,	D = 3,
										E = 4,	F = 5,	G = 6,	H = 7,
										I = 8,	J = 9,	K = 10,	L = 11,
										M = 12,	N = 13,	O = 14,	P = 15,
									}

		public class	MatrixException : Exception
		{
			public MatrixException( string _Message ) : base( _Message )			{}
		}

		#endregion

		#region FIELDS

		public float[,]			m = new float[4,4];
		public static int[]		ms_Index	= { 0, 1, 2, 3, 0, 1, 2 };				// This array gives the index of the current component
		public static int[]		ms_RotIndex = { 0, 1, 2, 0, 1, 2, 0 };				// This array gives the index of the current component considering only the rotation part

		#endregion

		#region METHODS

		// Generators
		public static Matrix4x4		ROT_X( float _fAngle )
		{
			return (new Matrix4x4()).MakeRotX( _fAngle );
		}

		public static Matrix4x4		ROT_Y( float _fAngle )
		{
			return (new Matrix4x4()).MakeRotY( _fAngle );
		}

		public static Matrix4x4		ROT_Z( float _fAngle )
		{
			return (new Matrix4x4()).MakeRotZ( _fAngle );
		}

		public static Matrix4x4		PYR( float _fPitch, float _fYaw, float _fRoll )
		{
			return (new Matrix4x4()).MakePYR( _fPitch, _fYaw, _fRoll );
		}

		// Constructors
		public						Matrix4x4()
		{
		}

		public						Matrix4x4( float[,] _Source )
		{
			Set( _Source );
		}
		public						Matrix4x4( Matrix3x3 _Source )
		{
			Set( _Source );
		}
		public						Matrix4x4( Matrix4x4 _Source )
		{
			Set( _Source );
		}

		// Access methods
		public Vector4D				GetRow( int _dwRowIndex )						{ return new Vector4D( m[_dwRowIndex,0], m[_dwRowIndex, 1], m[_dwRowIndex, 2], m[_dwRowIndex, 3] ); }
		public Vector4D				GetRow0()										{ return new Vector4D( m[0, 0], m[0, 1], m[0, 2], m[0, 3] ); }
		public Vector4D				GetRow1()										{ return new Vector4D( m[1, 0], m[1, 1], m[1, 2], m[1, 3] ); }
		public Vector4D				GetRow2()										{ return new Vector4D( m[2, 0], m[2, 1], m[2, 2], m[2, 3] ); }
		public Point4D				GetTrans()										{ return new Point4D ( m[3, 0], m[3, 1], m[3, 2], m[3, 3] ); }
		public void					SetRow( int _dwRowIndex, Vector4D _Row )		{ m[_dwRowIndex, 0] = _Row.x; m[_dwRowIndex, 1] = _Row.y; m[_dwRowIndex, 2] = _Row.z; m[_dwRowIndex, 3] = _Row.w; }
		public void					SetRow0( Vector4D _Row )						{ m[0, 0] = _Row.x; m[0, 1] = _Row.y; m[0, 2] = _Row.z; m[0, 3] = _Row.w; }
		public void					SetRow1( Vector4D _Row )						{ m[1, 0] = _Row.x; m[1, 1] = _Row.y; m[1, 2] = _Row.z; m[1, 3] = _Row.w; }
		public void					SetRow2( Vector4D _Row )						{ m[2, 0] = _Row.x; m[2, 1] = _Row.y; m[2, 2] = _Row.z; m[2, 3] = _Row.w; }
		public void					SetTrans( Point4D _Trans )						{ m[3, 0] = _Trans.x; m[3, 1] = _Trans.y; m[3, 2] = _Trans.z; m[3, 3] = _Trans.w; }
		public void					SetRow( int _dwRowIndex, Vector _Row )			{ m[_dwRowIndex, 0] = _Row.x; m[_dwRowIndex, 1] = _Row.y; m[_dwRowIndex, 2] = _Row.z; }
		public void					SetRow0( Vector _Row )							{ m[0, 0] = _Row.x; m[0, 1] = _Row.y; m[0, 2] = _Row.z; }
		public void					SetRow1( Vector _Row )							{ m[1, 0] = _Row.x; m[1, 1] = _Row.y; m[1, 2] = _Row.z; }
		public void					SetRow2( Vector _Row )							{ m[2, 0] = _Row.x; m[2, 1] = _Row.y; m[2, 2] = _Row.z; }
		public void					SetTrans( Point _Trans )						{ m[3, 0] = _Trans.x; m[3, 1] = _Trans.y; m[3, 2] = _Trans.z; }
		public Matrix3x3			GetRotation()									{ return new Matrix3x3( new float[3,3] {	{ m[0, 0], m[0, 1], m[0, 2] },
																																{ m[1, 0], m[1, 1], m[1, 2] },
																																{ m[2, 0], m[2, 1], m[2, 2] } } );
																					}
		public void					SetRotation( Matrix3x3 _Rot )					{	m[0, 0] = _Rot[0, 0]; m[0, 1] = _Rot[0, 1]; m[0, 2] = _Rot[0, 2];
																						m[1, 0] = _Rot[1, 0]; m[1, 1] = _Rot[1, 1]; m[1, 2] = _Rot[1, 2];
																						m[2, 0] = _Rot[2, 0]; m[2, 1] = _Rot[2, 1]; m[2, 2] = _Rot[2, 2];
																					}
		public Vector				GetScale()										{ return new Vector( new Vector( m[0, 0], m[0, 1], m[0, 2] ).Magnitude(), new Vector( m[1, 0], m[1, 1], m[1, 2] ).Magnitude(), new Vector( m[2, 0], m[2, 1], m[2, 2] ).Magnitude() ); }
		public void					SetScale( Vector _Scale )						{ m[0, 0] *= _Scale.x; m[1, 1] *= _Scale.y; m[2, 2] *= _Scale.z; }
		public Matrix4x4			Scale( Vector _Scale )							{ m[0, 0] *= _Scale.x; m[0, 1] *= _Scale.x; m[0, 2] *= _Scale.x; m[1, 0] *= _Scale.y; m[1, 1] *= _Scale.y; m[1, 2] *= _Scale.y; m[2, 0] *= _Scale.z; m[2, 1] *= _Scale.z; m[2, 2] *= _Scale.z; return this; }
		public void					Set( float[,] _Source )
		{
			if ( _Source == null )
				return;
			m[0,0] = _Source[0,0];	m[0,1] = _Source[0,1];	m[0,2] = _Source[0,2];	m[0,3] = _Source[0,3];
			m[1,0] = _Source[1,0];	m[1,1] = _Source[1,1];	m[1,2] = _Source[1,2];	m[1,3] = _Source[1,3];
			m[2,0] = _Source[2,0];	m[2,1] = _Source[2,1];	m[2,2] = _Source[2,2];	m[2,3] = _Source[2,3];
			m[3,0] = _Source[3,0];	m[3,1] = _Source[3,1];	m[3,2] = _Source[3,2];	m[3,3] = _Source[3,3];
		}
		public void					Set( Matrix4x4 _Source )
		{
			if ( _Source == null )
				return;
			m[0, 0] = _Source.m[0, 0]; m[0, 1] = _Source.m[0, 1]; m[0, 2] = _Source.m[0, 2]; m[0, 3] = _Source.m[0, 3];
			m[1, 0] = _Source.m[1, 0]; m[1, 1] = _Source.m[1, 1]; m[1, 2] = _Source.m[1, 2]; m[1, 3] = _Source.m[1, 3];
			m[2, 0] = _Source.m[2, 0]; m[2, 1] = _Source.m[2, 1]; m[2, 2] = _Source.m[2, 2]; m[2, 3] = _Source.m[2, 3];
			m[3, 0] = _Source.m[3, 0]; m[3, 1] = _Source.m[3, 1]; m[3, 2] = _Source.m[3, 2]; m[3, 3] = _Source.m[3, 3];
		}
		public void					Set( Matrix3x3 _Source )
		{
			if ( _Source == null )
				return;
			MakeIdentity();
			m[0, 0] = _Source.m[0, 0]; m[0, 1] = _Source.m[0, 1]; m[0, 2] = _Source.m[0, 2];
			m[1, 0] = _Source.m[1, 0]; m[1, 1] = _Source.m[1, 1]; m[1, 2] = _Source.m[1, 2];
			m[2, 0] = _Source.m[2, 0]; m[2, 1] = _Source.m[2, 1]; m[2, 2] = _Source.m[2, 2];
		}

		// Helpers
		public float				Trace()											{ return m[0, 0] + m[1, 1] + m[2, 2]; }
		public Matrix4x4			MakeZero()										{ m[0, 0] = m[0, 1] = m[0, 2] = m[0, 3] = m[1, 0] = m[1, 1] = m[1, 2] = m[1, 3] = m[2, 0] = m[2, 1] = m[2, 2] = m[2, 3] = m[3, 0] = m[3, 1] = m[3, 2] = m[3, 3] = 0.0f; return this; }
		public Matrix4x4			MakeIdentity()									{ m[0, 1] = m[0, 2] = m[0, 3] = m[1, 0] = m[1, 2] = m[1, 3] = m[2, 0] = m[2, 1] = m[2, 3] = m[3, 0] = m[3, 1] = m[3, 2] = 0.0f; m[0, 0] = m[1, 1] = m[2, 2] = m[3, 3] = 1.0f; return this; }
		public bool					IsIdentity()									{ if ( m[0, 0] != 1.0f || m[1, 1] != 1.0f || m[2, 2] != 1.0f || m[3, 3] != 1.0f ) return false; if ( m[0, 1] != 0.0f || m[0, 2] != 0.0f || m[0, 3] != 0.0f || m[1, 0] != 0.0f || m[1, 2] != 0.0f || m[1, 3] != 0.0f || m[2, 0] != 0.0f || m[2, 1] != 0.0f || m[2, 3] != 0.0f || m[3, 0] != 0.0f || m[3, 1] != 0.0f || m[3, 2] != 0.0f ) return false; return true; }
		public Matrix4x4			MakeRotX( float _fAngle )						{ MakeIdentity(); float fCosine = (float) System.Math.Cos( _fAngle ); float fSine = (float) System.Math.Sin( _fAngle ); m[1, 1] = +fCosine; m[1, 2] = +fSine; m[2, 1] = -fSine; m[2, 2] = +fCosine; return this; }
		public Matrix4x4			MakeRotY( float _fAngle )						{ MakeIdentity(); float fCosine = (float) System.Math.Cos( _fAngle ); float fSine = (float) System.Math.Sin( _fAngle ); m[0, 0] = +fCosine; m[0, 2] = -fSine; m[2, 0] = +fSine; m[2, 2] = +fCosine; return this; }
		public Matrix4x4			MakeRotZ( float _fAngle )						{ MakeIdentity(); float fCosine = (float) System.Math.Cos( _fAngle ); float fSine = (float) System.Math.Sin( _fAngle ); m[0, 0] = +fCosine; m[0, 1] = +fSine; m[1, 0] = -fSine; m[1, 1] = +fCosine; return this; }
//		public Matrix4x4			MakePYR( float _fPitch, float _fYaw, float _fRoll )	{ Matrix4x4 Pitch = ROT_X( _fPitch ); Matrix4x4 Yaw = ROT_Y( _fYaw ); Matrix4x4 Roll = ROT_Z( _fRoll ); Set( Roll * Yaw * Pitch ); return this; }
		public Matrix4x4			MakePYR( float _fPitch, float _fYaw, float _fRoll )	{ Matrix4x4 Pitch = ROT_X( _fPitch ); Matrix4x4 Yaw = ROT_Y( _fYaw ); Matrix4x4 Roll = ROT_Z( _fRoll ); Set( Pitch * Yaw * Roll ); return this; }

		public Matrix4x4			MakeLookAt( Point _Position, Point _Target, Vector _Up )
		{
			Vector	At = (_Target - _Position).Normalize();
			Vector	Right = (_Up ^ At).Normalize();
			Vector	Up = At ^ Right;

			MakeIdentity();
			SetRow0( Right );
			SetRow1( Up );
			SetRow2( At );
			SetTrans( _Position );

			return	this;
		}

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
		public void					FromEuler( Vector _EulerAngles )				{ Matrix4x4 MatX = ROT_X( _EulerAngles.x ); Matrix4x4 MatY = ROT_Y( _EulerAngles.y ); Matrix4x4 MatZ = ROT_Z( _EulerAngles.z ); Set( MatX * MatY * MatZ ); }
		public Matrix4x4			Transpose()										{ float fTemp; fTemp = m[1, 0]; m[1, 0] = m[0, 1]; m[0, 1] = fTemp; fTemp = m[2, 0]; m[2, 0] = m[0, 2]; m[0, 2] = fTemp; fTemp = m[3, 0]; m[3, 0] = m[0, 3]; m[0, 3] = fTemp; fTemp = m[3, 1]; m[3, 1] = m[1, 3]; m[1, 3] = fTemp; fTemp = m[3, 2]; m[3, 2] = m[2, 3]; m[2, 3] = fTemp; fTemp = m[2, 1]; m[2, 1] = m[1, 2]; m[1, 2] = fTemp; return this; }
		public float				CoFactor( int _dwRow, int _dwCol )
		{
			return	((	m[ms_Index[_dwRow+1], ms_Index[_dwCol+1]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+2]]*m[ms_Index[_dwRow+3], ms_Index[_dwCol+3]] +
						m[ms_Index[_dwRow+1], ms_Index[_dwCol+2]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+3]]*m[ms_Index[_dwRow+3], ms_Index[_dwCol+1]] +
						m[ms_Index[_dwRow+1], ms_Index[_dwCol+3]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*m[ms_Index[_dwRow+3], ms_Index[_dwCol+2]] )

					-(	m[ms_Index[_dwRow+3], ms_Index[_dwCol+1]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+2]]*m[ms_Index[_dwRow+1], ms_Index[_dwCol+3]] +
						m[ms_Index[_dwRow+3], ms_Index[_dwCol+2]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+3]]*m[ms_Index[_dwRow+1], ms_Index[_dwCol+1]] +
						m[ms_Index[_dwRow+3], ms_Index[_dwCol+3]]*m[ms_Index[_dwRow+2], ms_Index[_dwCol+1]]*m[ms_Index[_dwRow+1], ms_Index[_dwCol+2]] ))
					* (((_dwRow + _dwCol) & 1) == 1 ? -1.0f : +1.0f);
		}
		public float				Determinant()									{ return m[0, 0] * CoFactor( 0, 0 ) + m[0, 1] * CoFactor( 0, 1 ) + m[0, 2] * CoFactor( 0, 2 ) + m[0, 3] * CoFactor( 0, 3 ); }
		public Matrix4x4			Invert()
		{
			float	fDet = Determinant();
			if ( (float) System.Math.Abs(fDet) < float.Epsilon )
				throw new MatrixException( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

			float	fIDet = 1.0f / fDet;

			Matrix4x4	Temp = new Matrix4x4();
			Temp.m[0, 0] = CoFactor( 0, 0 ) * fIDet;
			Temp.m[1, 0] = CoFactor( 0, 1 ) * fIDet;
			Temp.m[2, 0] = CoFactor( 0, 2 ) * fIDet;
			Temp.m[3, 0] = CoFactor( 0, 3 ) * fIDet;
			Temp.m[0, 1] = CoFactor( 1, 0 ) * fIDet;
			Temp.m[1, 1] = CoFactor( 1, 1 ) * fIDet;
			Temp.m[2, 1] = CoFactor( 1, 2 ) * fIDet;
			Temp.m[3, 1] = CoFactor( 1, 3 ) * fIDet;
			Temp.m[0, 2] = CoFactor( 2, 0 ) * fIDet;
			Temp.m[1, 2] = CoFactor( 2, 1 ) * fIDet;
			Temp.m[2, 2] = CoFactor( 2, 2 ) * fIDet;
			Temp.m[3, 2] = CoFactor( 2, 3 ) * fIDet;
			Temp.m[0, 3] = CoFactor( 3, 0 ) * fIDet;
			Temp.m[1, 3] = CoFactor( 3, 1 ) * fIDet;
			Temp.m[2, 3] = CoFactor( 3, 2 ) * fIDet;
			Temp.m[3, 3] = CoFactor( 3, 3 ) * fIDet;

			Set( Temp );

			return	this;
		}

		public Matrix4x4			Inverse
		{
			get
			{
				Matrix4x4	Result = new Matrix4x4( this );
							Result.Invert();

				return	Result;
			}
		}

		public Matrix4x4			Normalize()
		{
			SetRow0( GetRow0().Normalize() );
			SetRow1( GetRow1().Normalize() );
			SetRow2( GetRow2().Normalize() );

			return	this;
		}
		public void					OrthoNormalize()
		{
			// Normalize first
			Normalize();

			// Find the most minimal divergence in the existing 3 axes
			float	fDivergenceXY = System.Math.Abs( GetRow0() | GetRow1() );
			float	fDivergenceYZ = System.Math.Abs( GetRow1() | GetRow2() );
			float	fDivergenceZX = System.Math.Abs( GetRow2() | GetRow0() );

			int		MinDivergenceRowIndex = 0;
			if ( fDivergenceXY < fDivergenceYZ )
			{
				if ( fDivergenceXY < fDivergenceZX )
					MinDivergenceRowIndex = 2;
				else
					MinDivergenceRowIndex = 1;
			}
			else
			{
				if ( fDivergenceYZ < fDivergenceZX )
					MinDivergenceRowIndex = 0;
				else
					MinDivergenceRowIndex = 1;
			}

				// The complementary axis is the safest to be recomputed
			SetRow( MinDivergenceRowIndex, ((Vector) GetRow( ms_RotIndex[MinDivergenceRowIndex+1] ) ^ (Vector) GetRow( ms_RotIndex[MinDivergenceRowIndex+2] )).Normalize() );

			// Find the minimal divergence in the remaining 2 axes
			float	fDivergence0 = System.Math.Abs( GetRow( ms_RotIndex[MinDivergenceRowIndex+0] ) | GetRow( ms_RotIndex[MinDivergenceRowIndex+1] ) );
			float	fDivergence1 = System.Math.Abs( GetRow( ms_RotIndex[MinDivergenceRowIndex+0] ) | GetRow( ms_RotIndex[MinDivergenceRowIndex+2] ) );

			int		MinSecondDivergenceRowIndex = 0;
			if ( fDivergence0 < fDivergence1 )
				MinSecondDivergenceRowIndex = ms_RotIndex[MinDivergenceRowIndex + 2];
			else
				MinSecondDivergenceRowIndex = ms_RotIndex[MinDivergenceRowIndex + 1];

				// The complementary axis is the safest to be recomputed
			SetRow( MinSecondDivergenceRowIndex, ((Vector) GetRow( ms_RotIndex[MinSecondDivergenceRowIndex+1] ) ^ (Vector) GetRow( ms_RotIndex[MinSecondDivergenceRowIndex+2] )).Normalize() );

			// Compute the final, remaining axis
			int	MinDivergenceIndex = System.Math.Min( MinDivergenceRowIndex, MinSecondDivergenceRowIndex );
			int	MaxDivergenceIndex = System.Math.Max( MinDivergenceRowIndex, MinSecondDivergenceRowIndex );
			int	RemainingAxisIndex = MinDivergenceIndex == 1 ? 0 : (MaxDivergenceIndex == 1 ? 2 : 1);

			SetRow( RemainingAxisIndex, (Vector) GetRow( MinDivergenceRowIndex ) ^ (Vector) GetRow( MinSecondDivergenceRowIndex ) );
		}

		/// <summary>
		/// Computes the rotation matrix to transform a source vector into a target vector
		/// (routine from Thomas Moller)
		/// </summary>
		/// <param name="_Source">The source vector</param>
		/// <param name="_Target">The target vector</param>
		/// <returns>The rotation matrix to apply that will transform</returns>
		public static Matrix4x4		ComputeRotationMatrix( Vector _Source, Vector _Target )
		{
			WMath.Matrix4x4	Result = new Matrix4x4();
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
			return	GetRow0().ToString() + ";" + GetRow1().ToString() + ";" + GetRow2().ToString() + ";" + GetTrans().ToString();
		}

		public static Matrix4x4		Parse( string _Source )
		{
			string[]	Terms = _Source.Split( new char[] { ';' } );
			if ( Terms.Length != 16 )
				throw new Exception( "Source string must have 16 terms!" );

			Matrix4x4	Result = new Matrix4x4();
						Result.m[0,0] = float.Parse( Terms[4 * 0 + 0] );
						Result.m[0,1] = float.Parse( Terms[4 * 0 + 1] );
						Result.m[0,2] = float.Parse( Terms[4 * 0 + 2] );
						Result.m[0,3] = float.Parse( Terms[4 * 0 + 3] );
						Result.m[1,0] = float.Parse( Terms[4 * 1 + 0] );
						Result.m[1,1] = float.Parse( Terms[4 * 1 + 1] );
						Result.m[1,2] = float.Parse( Terms[4 * 1 + 2] );
						Result.m[1,3] = float.Parse( Terms[4 * 1 + 3] );
						Result.m[2,0] = float.Parse( Terms[4 * 2 + 0] );
						Result.m[2,1] = float.Parse( Terms[4 * 2 + 1] );
						Result.m[2,2] = float.Parse( Terms[4 * 2 + 2] );
						Result.m[2,3] = float.Parse( Terms[4 * 2 + 3] );
						Result.m[3,0] = float.Parse( Terms[4 * 3 + 0] );
						Result.m[3,1] = float.Parse( Terms[4 * 3 + 1] );
						Result.m[3,2] = float.Parse( Terms[4 * 3 + 2] );
						Result.m[3,3] = float.Parse( Terms[4 * 3 + 3] );

			return	Result;
		}

		// ToString/Parse methods for 4x3 matrices
		public static Matrix4x4		Parse4x3( string _Source )
		{
			string[]	Terms = _Source.Split( ';' );
			if ( Terms.Length != 12 )
				throw new Exception( "Source string must have 12 terms!" );

			Matrix4x4	Result = new Matrix4x4();
						Result.m[0,0] = float.Parse( Terms[3 * 0 + 0] );
						Result.m[0,1] = float.Parse( Terms[3 * 0 + 1] );
						Result.m[0,2] = float.Parse( Terms[3 * 0 + 2] );
						Result.m[0,3] = 0.0f;
						Result.m[1,0] = float.Parse( Terms[3 * 1 + 0] );
						Result.m[1,1] = float.Parse( Terms[3 * 1 + 1] );
						Result.m[1,2] = float.Parse( Terms[3 * 1 + 2] );
						Result.m[1,3] = 0.0f;
						Result.m[2,0] = float.Parse( Terms[3 * 2 + 0] );
						Result.m[2,1] = float.Parse( Terms[3 * 2 + 1] );
						Result.m[2,2] = float.Parse( Terms[3 * 2 + 2] );
						Result.m[2,3] = 0.0f;
						Result.m[3,0] = float.Parse( Terms[3 * 3 + 0] );
						Result.m[3,1] = float.Parse( Terms[3 * 3 + 1] );
						Result.m[3,2] = float.Parse( Terms[3 * 3 + 2] );
						Result.m[3,3] = 1.0f;

			return	Result;
		}

		public string				ToString4x3()
		{
			return	m[0,0].ToString() + ";" + m[0,1].ToString() + ";" + m[0,2].ToString() + ";" +
					m[1,0].ToString() + ";" + m[1,1].ToString() + ";" + m[1,2].ToString() + ";" +
					m[2,0].ToString() + ";" + m[2,1].ToString() + ";" + m[2,2].ToString() + ";" +
					m[3,0].ToString() + ";" + m[3,1].ToString() + ";" + m[3,2].ToString();
		}

		// Cast operators
		public static explicit		operator Quat( Matrix4x4 _Mat )
		{
			float	fTrace = _Mat.Trace() - 1.0f, s;
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
						return	m[0, 3];
					case	(int) COEFFS.E:
						return	m[1, 0];
					case	(int) COEFFS.F:
						return	m[1, 1];
					case	(int) COEFFS.G:
						return	m[1, 2];
					case	(int) COEFFS.H:
						return	m[1, 3];
					case	(int) COEFFS.I:
						return	m[2, 0];
					case	(int) COEFFS.J:
						return	m[2, 1];
					case	(int) COEFFS.K:
						return	m[2, 2];
					case	(int) COEFFS.L:
						return	m[2, 3];
					case	(int) COEFFS.M:
						return	m[3, 0];
					case	(int) COEFFS.N:
						return	m[3, 1];
					case	(int) COEFFS.O:
						return	m[3, 2];
					case	(int) COEFFS.P:
						return	m[3, 3];
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
						m[0, 3] = value;
						break;
					case	(int) COEFFS.E:
						m[1, 0] = value;
						break;
					case	(int) COEFFS.F:
						m[1, 1] = value;
						break;
					case	(int) COEFFS.G:
						m[1, 2] = value;
						break;
					case	(int) COEFFS.H:
						m[1, 3] = value;
						break;
					case	(int) COEFFS.I:
						m[2, 0] = value;
						break;
					case	(int) COEFFS.J:
						m[2, 1] = value;
						break;
					case	(int) COEFFS.K:
						m[2, 2] = value;
						break;
					case	(int) COEFFS.L:
						m[2, 3] = value;
						break;
					case	(int) COEFFS.M:
						m[3, 0] = value;
						break;
					case	(int) COEFFS.N:
						m[3, 1] = value;
						break;
					case	(int) COEFFS.O:
						m[3, 2] = value;
						break;
					case	(int) COEFFS.P:
						m[3, 3] = value;
						break;
					default:
						throw new MatrixException( "Coefficient index out of range!" );
				}
			}
		}

		// Arithmetic operators
		public static Matrix4x4		operator+( Matrix4x4 _Op0, Matrix4x4 _Op1 )
		{
			Matrix4x4	Ret = new Matrix4x4();
			Ret.m[0, 0] = _Op0.m[0, 0] + _Op1.m[0, 0];
			Ret.m[0, 1] = _Op0.m[0, 1] + _Op1.m[0, 1];
			Ret.m[0, 2] = _Op0.m[0, 2] + _Op1.m[0, 2];
			Ret.m[0, 3] = _Op0.m[0, 3] + _Op1.m[0, 3];
			Ret.m[1, 0] = _Op0.m[1, 0] + _Op1.m[1, 0];
			Ret.m[1, 1] = _Op0.m[1, 1] + _Op1.m[1, 1];
			Ret.m[1, 2] = _Op0.m[1, 2] + _Op1.m[1, 2];
			Ret.m[1, 3] = _Op0.m[1, 3] + _Op1.m[1, 3];
			Ret.m[2, 0] = _Op0.m[2, 0] + _Op1.m[2, 0];
			Ret.m[2, 1] = _Op0.m[2, 1] + _Op1.m[2, 1];
			Ret.m[2, 2] = _Op0.m[2, 2] + _Op1.m[2, 2];
			Ret.m[2, 3] = _Op0.m[2, 3] + _Op1.m[2, 3];
			Ret.m[3, 0] = _Op0.m[3, 0] + _Op1.m[3, 0];
			Ret.m[3, 1] = _Op0.m[3, 1] + _Op1.m[3, 1];
			Ret.m[3, 2] = _Op0.m[3, 2] + _Op1.m[3, 2];
			Ret.m[3, 3] = _Op0.m[3, 3] + _Op1.m[3, 3];
			return	Ret;
		}
		public static Matrix4x4		operator-( Matrix4x4 _Op0, Matrix4x4 _Op1 )
		{
			Matrix4x4	Ret = new Matrix4x4();
			Ret.m[0, 0] = _Op0.m[0, 0] - _Op1.m[0, 0];
			Ret.m[0, 1] = _Op0.m[0, 1] - _Op1.m[0, 1];
			Ret.m[0, 2] = _Op0.m[0, 2] - _Op1.m[0, 2];
			Ret.m[0, 3] = _Op0.m[0, 3] - _Op1.m[0, 3];
			Ret.m[1, 0] = _Op0.m[1, 0] - _Op1.m[1, 0];
			Ret.m[1, 1] = _Op0.m[1, 1] - _Op1.m[1, 1];
			Ret.m[1, 2] = _Op0.m[1, 2] - _Op1.m[1, 2];
			Ret.m[1, 3] = _Op0.m[1, 3] - _Op1.m[1, 3];
			Ret.m[2, 0] = _Op0.m[2, 0] - _Op1.m[2, 0];
			Ret.m[2, 1] = _Op0.m[2, 1] - _Op1.m[2, 1];
			Ret.m[2, 2] = _Op0.m[2, 2] - _Op1.m[2, 2];
			Ret.m[2, 3] = _Op0.m[2, 3] - _Op1.m[2, 3];
			Ret.m[3, 0] = _Op0.m[3, 0] - _Op1.m[3, 0];
			Ret.m[3, 1] = _Op0.m[3, 1] - _Op1.m[3, 1];
			Ret.m[3, 2] = _Op0.m[3, 2] - _Op1.m[3, 2];
			Ret.m[3, 3] = _Op0.m[3, 3] - _Op1.m[3, 3];
			return	Ret;
		}
		public static Matrix4x4		operator*( Matrix4x4 _Op0, Matrix4x4 _Op1 )
		{
			Matrix4x4	Ret = new Matrix4x4();
						Ret.m[0, 0] = _Op0.m[0, 0]*_Op1.m[0, 0] + _Op0.m[0, 1]*_Op1.m[1, 0] + _Op0.m[0, 2]*_Op1.m[2, 0] + _Op0.m[0, 3]*_Op1.m[3, 0];
						Ret.m[0, 1] = _Op0.m[0, 0]*_Op1.m[0, 1] + _Op0.m[0, 1]*_Op1.m[1, 1] + _Op0.m[0, 2]*_Op1.m[2, 1] + _Op0.m[0, 3]*_Op1.m[3, 1];
						Ret.m[0, 2] = _Op0.m[0, 0]*_Op1.m[0, 2] + _Op0.m[0, 1]*_Op1.m[1, 2] + _Op0.m[0, 2]*_Op1.m[2, 2] + _Op0.m[0, 3]*_Op1.m[3, 2];
						Ret.m[0, 3] = _Op0.m[0, 0]*_Op1.m[0, 3] + _Op0.m[0, 1]*_Op1.m[1, 3] + _Op0.m[0, 2]*_Op1.m[2, 3] + _Op0.m[0, 3]*_Op1.m[3, 3];

						Ret.m[1, 0] = _Op0.m[1, 0]*_Op1.m[0, 0] + _Op0.m[1, 1]*_Op1.m[1, 0] + _Op0.m[1, 2]*_Op1.m[2, 0] + _Op0.m[1, 3]*_Op1.m[3, 0];
						Ret.m[1, 1] = _Op0.m[1, 0]*_Op1.m[0, 1] + _Op0.m[1, 1]*_Op1.m[1, 1] + _Op0.m[1, 2]*_Op1.m[2, 1] + _Op0.m[1, 3]*_Op1.m[3, 1];
						Ret.m[1, 2] = _Op0.m[1, 0]*_Op1.m[0, 2] + _Op0.m[1, 1]*_Op1.m[1, 2] + _Op0.m[1, 2]*_Op1.m[2, 2] + _Op0.m[1, 3]*_Op1.m[3, 2];
						Ret.m[1, 3] = _Op0.m[1, 0]*_Op1.m[0, 3] + _Op0.m[1, 1]*_Op1.m[1, 3] + _Op0.m[1, 2]*_Op1.m[2, 3] + _Op0.m[1, 3]*_Op1.m[3, 3];

						Ret.m[2, 0] = _Op0.m[2, 0]*_Op1.m[0, 0] + _Op0.m[2, 1]*_Op1.m[1, 0] + _Op0.m[2, 2]*_Op1.m[2, 0] + _Op0.m[2, 3]*_Op1.m[3, 0];
						Ret.m[2, 1] = _Op0.m[2, 0]*_Op1.m[0, 1] + _Op0.m[2, 1]*_Op1.m[1, 1] + _Op0.m[2, 2]*_Op1.m[2, 1] + _Op0.m[2, 3]*_Op1.m[3, 1];
						Ret.m[2, 2] = _Op0.m[2, 0]*_Op1.m[0, 2] + _Op0.m[2, 1]*_Op1.m[1, 2] + _Op0.m[2, 2]*_Op1.m[2, 2] + _Op0.m[2, 3]*_Op1.m[3, 2];
						Ret.m[2, 3] = _Op0.m[2, 0]*_Op1.m[0, 3] + _Op0.m[2, 1]*_Op1.m[1, 3] + _Op0.m[2, 2]*_Op1.m[2, 3] + _Op0.m[2, 3]*_Op1.m[3, 3];

						Ret.m[3, 0] = _Op0.m[3, 0]*_Op1.m[0, 0] + _Op0.m[3, 1]*_Op1.m[1, 0] + _Op0.m[3, 2]*_Op1.m[2, 0] + _Op0.m[3, 3]*_Op1.m[3, 0];
						Ret.m[3, 1] = _Op0.m[3, 0]*_Op1.m[0, 1] + _Op0.m[3, 1]*_Op1.m[1, 1] + _Op0.m[3, 2]*_Op1.m[2, 1] + _Op0.m[3, 3]*_Op1.m[3, 1];
						Ret.m[3, 2] = _Op0.m[3, 0]*_Op1.m[0, 2] + _Op0.m[3, 1]*_Op1.m[1, 2] + _Op0.m[3, 2]*_Op1.m[2, 2] + _Op0.m[3, 3]*_Op1.m[3, 2];
						Ret.m[3, 3] = _Op0.m[3, 0]*_Op1.m[0, 3] + _Op0.m[3, 1]*_Op1.m[1, 3] + _Op0.m[3, 2]*_Op1.m[2, 3] + _Op0.m[3, 3]*_Op1.m[3, 3];

			return	Ret;
		}
		public static Matrix4x4		operator*( Matrix4x4 _Op0, float _s )
		{
			Matrix4x4	Ret = new Matrix4x4();
			Ret.m[0, 0] = _Op0.m[0, 0] * _s;
			Ret.m[0, 1] = _Op0.m[0, 1] * _s;
			Ret.m[0, 2] = _Op0.m[0, 2] * _s;
			Ret.m[0, 3] = _Op0.m[0, 3] * _s;
			Ret.m[1, 0] = _Op0.m[1, 0] * _s;
			Ret.m[1, 1] = _Op0.m[1, 1] * _s;
			Ret.m[1, 2] = _Op0.m[1, 2] * _s;
			Ret.m[1, 3] = _Op0.m[1, 3] * _s;
			Ret.m[2, 0] = _Op0.m[2, 0] * _s;
			Ret.m[2, 1] = _Op0.m[2, 1] * _s;
			Ret.m[2, 2] = _Op0.m[2, 2] * _s;
			Ret.m[2, 3] = _Op0.m[2, 3] * _s;
			Ret.m[3, 0] = _Op0.m[3, 0] * _s;
			Ret.m[3, 1] = _Op0.m[3, 1] * _s;
			Ret.m[3, 2] = _Op0.m[3, 2] * _s;
			Ret.m[3, 3] = _Op0.m[3, 3] * _s;
			return	Ret;
		}
		public static Matrix4x4		operator*( float _s, Matrix4x4 _Op0 )
		{
			Matrix4x4	Ret = new Matrix4x4();
			Ret.m[0, 0] = _Op0.m[0, 0] * _s;
			Ret.m[0, 1] = _Op0.m[0, 1] * _s;
			Ret.m[0, 2] = _Op0.m[0, 2] * _s;
			Ret.m[0, 3] = _Op0.m[0, 3] * _s;
			Ret.m[1, 0] = _Op0.m[1, 0] * _s;
			Ret.m[1, 1] = _Op0.m[1, 1] * _s;
			Ret.m[1, 2] = _Op0.m[1, 2] * _s;
			Ret.m[1, 3] = _Op0.m[1, 3] * _s;
			Ret.m[2, 0] = _Op0.m[2, 0] * _s;
			Ret.m[2, 1] = _Op0.m[2, 1] * _s;
			Ret.m[2, 2] = _Op0.m[2, 2] * _s;
			Ret.m[2, 3] = _Op0.m[2, 3] * _s;
			Ret.m[3, 0] = _Op0.m[3, 0] * _s;
			Ret.m[3, 1] = _Op0.m[3, 1] * _s;
			Ret.m[3, 2] = _Op0.m[3, 2] * _s;
			Ret.m[3, 3] = _Op0.m[3, 3] * _s;
			return	Ret;
		}
		public static Matrix4x4		operator/( Matrix4x4 _Op0, float _s )
		{
			float	Is = 1.0f / _s;
			Matrix4x4	Ret = new Matrix4x4();
			Ret.m[0, 0] = _Op0.m[0, 0] * Is;
			Ret.m[0, 1] = _Op0.m[0, 1] * Is;
			Ret.m[0, 2] = _Op0.m[0, 2] * Is;
			Ret.m[0, 3] = _Op0.m[0, 3] * Is;
			Ret.m[1, 0] = _Op0.m[1, 0] * Is;
			Ret.m[1, 1] = _Op0.m[1, 1] * Is;
			Ret.m[1, 2] = _Op0.m[1, 2] * Is;
			Ret.m[1, 3] = _Op0.m[1, 3] * Is;
			Ret.m[2, 0] = _Op0.m[2, 0] * Is;
			Ret.m[2, 1] = _Op0.m[2, 1] * Is;
			Ret.m[2, 2] = _Op0.m[2, 2] * Is;
			Ret.m[2, 3] = _Op0.m[2, 3] * Is;
			Ret.m[3, 0] = _Op0.m[3, 0] * Is;
			Ret.m[3, 1] = _Op0.m[3, 1] * Is;
			Ret.m[3, 2] = _Op0.m[3, 2] * Is;
			Ret.m[3, 3] = _Op0.m[3, 3] * Is;
			return	Ret;
		}

		#endregion
	}
}
