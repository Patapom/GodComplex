//////////////////////////////////////////////////////////////////////////
// LTC Class containing both M and M^-1 matrice + fitting values (e.g. coefficients, error, stats, etc.)
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace LTCTableGenerator
{

	[System.Diagnostics.DebuggerDisplay( "m11={m11}, m22={m22}, m13={m13}, m31={m31} - Amplitude = {amplitude}" )]
	public class LTC {

		// lobe amplitude
		public double		amplitude = 1;

		// Average Schlick Fresnel term
		public double		fresnel = 1;

		// Parametric representation (used by the fitter only!)
		public float3		X = float3.UnitX;
		public float3		Y = float3.UnitY;
		public float3		Z = float3.UnitZ;
		public double		m11 = 1, m22 = 1, m13 = 0, m31 = 0;	// WARNING: These are NOT final parameters to use at runtime. Use the "RuntimeParameters" properties instead
		public double[,]	M = new double[3,3];

		public double		error;				// Last fitting error
		public int			iterationsCount;	// Last amount of iterations

		// Runtime matrix representation
		public double[,]	invM = new double[3,3];
		public double		detInvM;

		/// <summary>
		/// Gets the runtime parameters to use for LTC estimate
		/// </summary>
		public double[]		RuntimeParameters {
			get { return new double[] {	invM[0,0], invM[1,1], invM[0,2], invM[2,0],		// invM matrix coefficients
										amplitude, fresnel };							// BRDF scale and Fresnel coefficients
			}
		}

		public LTC() {
			Update();
		}
		public LTC( System.IO.BinaryReader R ) {
			Read( R );
			Update();
		}

		/// <summary>
		/// Sets the coefficients for the M matrix (warning! NOT the inverse matrix used at runtime!)
		/// </summary>
		/// <param name="_parameters"></param>
		/// <param name="_isotropic"></param>
		public void	Set( double[] _parameters, bool _isotropic ) {
 			double	tempM11 = Math.Max( _parameters[0], 0.002f );
 			double	tempM22 = Math.Max( _parameters[1], 0.002f );

			// When composing from the left (V' = V * invM), the important 3rd parameter is m31
 			double	tempM31 = _parameters[2];

			#if FIT_INV_M
 				double	tempM13 = _parameters.Length > 3 ? _parameters[3] : 0;
				if ( _parameters.Length > 4 )
					amplitude = Math.Max( _parameters[4], 1e-4 );
			#else
				double	tempM13 = 0;
// 					if ( _parameters.Length > 3 )
// 						amplitude = Math.Max( _parameters[3], 1e-4 );
			#endif

			if ( _isotropic ) {
				m11 = tempM11;
				m22 = tempM11;
				m13 = 0.0;
				m31 = 0.0;
			} else {
				m11 = tempM11;
				m22 = tempM22;
				m13 = tempM13;
				m31 = tempM31;
			}

			Update();		// Update the matrix
		}

		public void		Update() {
			#if FIT_INV_M
				// My method => Directly fit target inverse matrix + amplitude
				invM[0,0] = m11;
				invM[0,1] = 0;
				invM[0,2] = m13;
				invM[1,0] = 0;
				invM[1,1] = m22;
				invM[1,2] = 0;
				invM[2,0] = m31;
				invM[2,1] = 0;
				invM[2,2] = 1;

				detInvM = Invert( invM, M );
			#else
				// Heitz & Hill Method => Fit M, inverse to obtain target matrix
				// Build the source matrix M for which we're exploring the parameter space
// 					float3x3	temp = new float3x3(	(float) m11,	0,				(float) m13,
// 														0,				(float) m22,	0,
// 														(float) m31,	0,				1	);
// 					M = temp * new float3x3( X, Y, Z );

				M[0,0] = m11*X.x +			 + m13*Z.x;
				M[0,1] = m11*X.y +			 + m13*Z.y;
				M[0,2] = m11*X.z +			 + m13*Z.z;

				M[1,0] =			m22*Y.x;
				M[1,1] =			m22*Y.y;
				M[1,2] =			m22*Y.z;

				M[2,0] = m31*X.x			 +     Z.x;
				M[2,1] = m31*X.y			 +     Z.y;
				M[2,2] = m31*X.z			 +     Z.z;

				// Build the final matrix required at runtime for LTC evaluation
				detInvM = 1.0 / Invert( M, invM );

				// Clear it up so it's always in the required final form
				invM[0,1] = 0;
				invM[1,0] = 0;
				invM[1,2] = 0;
				invM[2,1] = 0;
				invM[2,2] = 1;
			#endif
		}

		public double	MaxValue {
			get { return amplitude * detInvM / Mathf.PI; }
		}

		public double	Eval( ref float3 _tsLight ) {
			// Transform into original distribution space
			float3	Loriginal = float3.Zero;
			Transform( _tsLight, invM, ref Loriginal );	// Compose from the left, as in the shader code!
			float	l = Loriginal.Length;
					Loriginal /= l;

			// Estimate original distribution (a clamped cosine lobe)
			float	D = Mathf.INVPI * Math.Max( 0.0f, Loriginal.z ); 

			// Compute the Jacobian, roundDwo / roundDw
			double	jacobian = detInvM / (l*l*l);

			// Scale distribution
			double	res = amplitude * D * jacobian;
			return res;
		}

		public void	GetSamplingDirection( float _U1, float _U2, ref float3 _direction ) {
			float	theta = Mathf.Asin( Mathf.Sqrt( _U1 ) );
			float	phi = Mathf.TWOPI * _U2;
			Transform( new float3( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) ), M, ref _direction );
			_direction.Normalize();
		}

		/// <summary>
		/// Should always return something close to 1
		/// </summary>
		/// <returns></returns>
		public double	TestNormalization() {
			double	sum = 0;
			float	dtheta = 0.005f;
			float	dphi = 0.025f;
			float3	L = new float3();
			for( float theta = 0.0f; theta <= Mathf.PI; theta+=dtheta ) {
				for( float phi = 0.0f; phi <= Mathf.PI; phi+=dphi ) {
					L.Set( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) );
					sum += Mathf.Sin(theta) * Eval( ref L );
				}
			}
			sum *= dtheta * 2*dphi;
			return sum;
		}

		#region Initialization Function

		const int	SAMPLES_COUNT = 50;			// number of samples used to compute the error during fitting

		// compute the average direction of the BRDF
		public void	ComputeAverageTerms( IBRDF _BRDF, ref float3 _tsView, float _alpha ) {
			amplitude = 0.0;
			fresnel = 0.0;
			Z = float3.Zero;
			error = 0.0;

			double	weight, pdf, eval;
			float3	tsLight = float3.Zero;
			float3	H = float3.Zero;
			for ( int j = 0 ; j < SAMPLES_COUNT ; ++j ) {
				for ( int i = 0 ; i < SAMPLES_COUNT ; ++i ) {
					float U1 = (i+0.5f) / SAMPLES_COUNT;
					float U2 = (j+0.5f) / SAMPLES_COUNT;

					// sample
					_BRDF.GetSamplingDirection( ref _tsView, _alpha, U1, U2, ref tsLight );

					// eval
					eval = _BRDF.Eval( ref _tsView, ref tsLight, _alpha, out pdf );
					if ( pdf == 0.0f )
						continue;

					H = (_tsView + tsLight).Normalized;

					// accumulate
					weight = eval / pdf;
					if ( double.IsNaN( weight ) )
						throw new Exception( "NaN!" );

					amplitude += weight;
					fresnel += weight * Math.Pow( 1 - _tsView.Dot( H ), 5.0 );
					Z += (float) weight * tsLight;
				}
			}
			amplitude /= SAMPLES_COUNT*SAMPLES_COUNT;
			fresnel /= SAMPLES_COUNT*SAMPLES_COUNT;

			// Finish building the average TBN orthogonal basis
			Z.y = 0.0f;		// clear y component, which should be zero with isotropic BRDFs
			float	length = Z.Length;
			if ( length > 0.0f )
				Z /= length;
			else
				Z = float3.UnitZ;
			X.Set( Z.z, 0, -Z.x );
			Y = float3.UnitY;

// 			#if FIT_INV_M
// 				// Compute intial inverse M coefficients to match average direction best
// 				M = new float3x3( X, Y, Z );
// 				invM = M.Inverse;
// 
// 				m11 = invM.r0.x;
// 				m22 = invM.r1.y;
// 				m13 = invM.r0.z;
// 				m31 = invM.r2.x;
// 			#endif
		}

		#endregion

		#region Math Functions

		/// <summary>
		/// Computes B = A^-1, returns determinant of _A
		/// </summary>
		/// <param name="_A"></param>
		/// <param name="_B"></param>
		/// <returns></returns>
		double			Invert( double[,] _A, double[,] _B ) {
			double	det =	(_A[0,0]*_A[1,1]*_A[2,2] + _A[0,1]*_A[1,2]*_A[2,0] + _A[0,2]*_A[1,0]*_A[2,1])
						-   (_A[2,0]*_A[1,1]*_A[0,2] + _A[2,1]*_A[1,2]*_A[0,0] + _A[2,2]*_A[1,0]*_A[0,1]);
			if ( Math.Abs(det) < float.Epsilon )
				throw new Exception( "Matrix is not invertible!" );		// The matrix is not invertible! Singular case!

			double	invDet = 1.0 / det;

			_B[0,0] = +(_A[1,1] * _A[2,2] - _A[2,1] * _A[1,2]) * invDet;
			_B[1,0] = -(_A[1,0] * _A[2,2] - _A[2,0] * _A[1,2]) * invDet;
			_B[2,0] = +(_A[1,0] * _A[2,1] - _A[2,0] * _A[1,1]) * invDet;
			_B[0,1] = -(_A[0,1] * _A[2,2] - _A[2,1] * _A[0,2]) * invDet;
			_B[1,1] = +(_A[0,0] * _A[2,2] - _A[2,0] * _A[0,2]) * invDet;
			_B[2,1] = -(_A[0,0] * _A[2,1] - _A[2,0] * _A[0,1]) * invDet;
			_B[0,2] = +(_A[0,1] * _A[1,2] - _A[1,1] * _A[0,2]) * invDet;
			_B[1,2] = -(_A[0,0] * _A[1,2] - _A[1,0] * _A[0,2]) * invDet;
			_B[2,2] = +(_A[0,0] * _A[1,1] - _A[1,0] * _A[0,1]) * invDet;

			return det;
		}

		void			Transform( float3 a, double[,] b, ref float3 c ) {
			c.x = (float) (a.x * b[0,0] + a.y * b[1,0] + a.z * b[2,0]);
			c.y = (float) (a.x * b[0,1] + a.y * b[1,1] + a.z * b[2,1]);
			c.z = (float) (a.x * b[0,2] + a.y * b[1,2] + a.z * b[2,2]);
		}

		#endregion

		#region I/O

		public void	Read( System.IO.BinaryReader R ) {
			m11 = R.ReadDouble();
			m22 = R.ReadDouble();
			m13 = R.ReadDouble();
			m31 = R.ReadDouble();
			amplitude = R.ReadDouble();
			fresnel = R.ReadDouble();

			X.x = R.ReadSingle();
			X.y = R.ReadSingle();
			X.z = R.ReadSingle();
			Y.x = R.ReadSingle();
			Y.y = R.ReadSingle();
			Y.z = R.ReadSingle();
			Z.x = R.ReadSingle();
			Z.y = R.ReadSingle();
			Z.z = R.ReadSingle();

			error = R.ReadDouble();

			Update();
		}

		public void	Write( System.IO.BinaryWriter W ) {
			W.Write( m11 );
			W.Write( m22 );
			W.Write( m13 );
			W.Write( m31 );
			W.Write( amplitude );
			W.Write( fresnel );

			W.Write( X.x );
			W.Write( X.y );
			W.Write( X.z );
			W.Write( Y.x );
			W.Write( Y.y );
			W.Write( Y.z );
			W.Write( Z.x );
			W.Write( Z.y );
			W.Write( Z.z );

			W.Write( error );
		}

		#endregion
	}
}
