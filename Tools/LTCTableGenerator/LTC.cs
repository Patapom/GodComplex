//#define FIT_TRANSPOSED				// Fit transposed matrix
//#define FIT_INV_M					// Fit M^-1 directly instead of M+inversion

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
//		public double		detInvM;
		public double		detM;

		/// <summary>
		/// Gets the runtime parameters to use for LTC estimate
		/// </summary>
		public double[]		RuntimeParameters {
			get { return new double[] {	invM[0,0], invM[1,1], invM[0,2], invM[2,0],		// invM matrix coefficients
										amplitude, fresnel };							// BRDF scale and Fresnel coefficients
			}
		}

// 		public double	MaxValue {
// 			get { return amplitude * detInvM / Mathf.PI; }
// 		}

		public LTC() {
			Update();
		}
		public LTC( System.IO.BinaryReader R ) {
			Read( R );
			Update();
		}

		/// <summary>
		/// Gets the parameters used for fitting
		/// </summary>
		/// <returns></returns>
		public double[]	GetFittingParms() {
			#if FIT_INV_M
				double[]	tempParams = new double[] {
					m11,
					m22,
					m31,
//					m13
				};
			#else
				double[]	tempParams = new double[] {
					m11,
					m22,
					m13,
//					m31,
				};
			#endif
			return tempParams;
		}

		/// <summary>
		/// Sets the coefficients for the M matrix (warning! NOT the inverse matrix used at runtime!)
		/// </summary>
		/// <param name="_parameters"></param>
		/// <param name="_isotropic"></param>
		public void	SetFittingParms( double[] _parameters, bool _isotropic ) {
 			double	tempM11 = Math.Max( _parameters[0], 0.001f );
 			double	tempM22 = Math.Max( _parameters[1], 0.001f );

			// When composing from the left (V' = V * invM), the important 3rd parameter is m31
// 			#if FIT_TRANSPOSED
//  				double	tempM31 = _parameters[2];
// 
// 				#if FIT_INV_M
//  					double	tempM13 = _parameters.Length > 3 ? _parameters[3] : 0;
// 					if ( _parameters.Length > 4 )
// 						amplitude = Math.Max( _parameters[4], 1e-4 );
// 				#else
// 					double	tempM13 = 0;
// //					if ( _parameters.Length > 3 )
// //						amplitude = Math.Max( _parameters[3], 1e-4 );
// 				#endif
// 			#else
 				double	tempM13 = _parameters[2];

				#if FIT_INV_M
 					double	tempM31 = _parameters.Length > 3 ? _parameters[3] : 0;
					if ( _parameters.Length > 4 )
						amplitude = Math.Max( _parameters[4], 1e-4 );
				#else
					double	tempM31 = _parameters.Length > 3 ? _parameters[3] : 0;
				#endif
// 			#endif

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

// 				#if FIT_TRANSPOSED
// // Code from Heitz et al., assuming they wrote it transposed
// // 					float3x3	temp = new float3x3(	(float) m11,	0,				(float) m13,
// // 														0,				(float) m22,	0,
// // 														(float) m31,	0,				1	);
// // 					M = temp * new float3x3( X, Y, Z );
// 
// 					M[0,0] = m11*X.x +			 + m13*Z.x;
// 					M[0,1] = m11*X.y +			 + m13*Z.y;
// 					M[0,2] = m11*X.z +			 + m13*Z.z;
// 
// 					M[1,0] =			m22*Y.x;
// 					M[1,1] =			m22*Y.y;
// 					M[1,2] =			m22*Y.z;
// 
// 					M[2,0] = m31*X.x			 +     Z.x;
// 					M[2,1] = m31*X.y			 +     Z.y;
// 					M[2,2] = m31*X.z			 +     Z.z;
// 				#else
// Code from Heitz et al., as given by the example code
// 					M = mat3(X, Y, Z) *
// 						mat3(m11, 0, 0,
// 							0, m22, 0,
// 							m13, 0, 1);
// 					invM = inverse(M);
// 					detM = abs(glm::determinant(M));

// Annoying GLM library details:
// 	struct mat3 {
// 		vec3	value[3];	// COLUMNS!!!!!
// 	}
// // 	mat3( T x0, T y0, T z0,
// 		  T x1, T y1, T z1,
// 		  T x2, T y2, T z2 ) {
// 			this->value[0] = col_type(x0, y0, z0);
// 			this->value[1] = col_type(x1, y1, z1);
// 			this->value[2] = col_type(x2, y2, z2);
// 	}
//
// 	mat3(col_type const& v0, col_type const& v1, col_type const& v2) {
// 			this->value[0] = col_type(v0);
// 			this->value[1] = col_type(v1);
// 			this->value[2] = col_type(v2);
// 	}
//
//	operator*( m1, m2 ) {
// 		mat<3, 3, T, Q> Result;
// 		Result[0][0] = m1[0][0] * m2[0][0] + m1[1][0] * m2[0][1] + m1[2][0] * m2[0][2];
// 		Result[0][1] = m1[0][1] * m2[0][0] + m1[1][1] * m2[0][1] + m1[2][1] * m2[0][2];
// 		Result[0][2] = m1[0][2] * m2[0][0] + m1[1][2] * m2[0][1] + m1[2][2] * m2[0][2];
// 		Result[1][0] = m1[0][0] * m2[1][0] + m1[1][0] * m2[1][1] + m1[2][0] * m2[1][2];
// 		Result[1][1] = m1[0][1] * m2[1][0] + m1[1][1] * m2[1][1] + m1[2][1] * m2[1][2];
// 		Result[1][2] = m1[0][2] * m2[1][0] + m1[1][2] * m2[1][1] + m1[2][2] * m2[1][2];
// 		Result[2][0] = m1[0][0] * m2[2][0] + m1[1][0] * m2[2][1] + m1[2][0] * m2[2][2];
// 		Result[2][1] = m1[0][1] * m2[2][0] + m1[1][1] * m2[2][1] + m1[2][1] * m2[2][2];
// 		Result[2][2] = m1[0][2] * m2[2][0] + m1[1][2] * m2[2][1] + m1[2][2] * m2[2][2];
// 		return Result;
// 	}
//
// So, knowing the convoluted constructors and operator* we see that:
//					| Xx Yx Zx |
//	mat3(X, Y, Z) = | Xy Yy Zy |
//					| Xz Yz Zz |
//
//	mat3( m11, 0, 0,	| m11  0  m13 |
// 		  0, m22, 0, =	|  0  m22  0  |
// 		  m13, 0, 1)	|  0   0   1  |
//
// And so:
//
//	| Xx Yx Zx |   | m11  0  m13 |   | Xx*m11  Yx*m22  Xx*m13+Zx |
//	| Xy Yy Zy | * |  0  m22  0  | = | Xy*m11  Yy*m22  Xy*m13+Zy |  (thank God, they didn't change the math!)
//	| Xz Yz Zz |   |  0   0   1  |	 | Xz*m11  Yz*m22  Xz*m13+Zz |
//
//	| Xx Yx Zx |   | m11  0  m13 |   | Xx*m11+m31*Zx  Yx*m22  Xx*m13+Zx |
//	| Xy Yy Zy | * |  0  m22  0  | = | Xy*m11+m31*Zy  Yy*m22  Xy*m13+Zy |  (thank God, they didn't change the math!)
//	| Xz Yz Zz |   | m31  0   1  |	 | Xz*m11+m31*Zz  Yz*m22  Xz*m13+Zz |
//
					M[0,0] = m11*X.x + m31*Z.x;
					M[0,1] = m22*Y.x;
					M[0,2] = m13*X.x + Z.x;

					M[1,0] = m11*X.y + m31*Z.y;
					M[1,1] = m22*Y.y;
					M[1,2] = m13*X.y + Z.y;

					M[2,0] = m11*X.z + m31*Z.z;
					M[2,1] = m22*Y.z;
					M[2,2] = m13*X.z + Z.z;

				#endif

				// Build the final matrix required at runtime for LTC evaluation
				detM = Invert( M, invM );
				if ( detM < 0.0 )
					throw new Exception( "Negative determinant!" );

// detInvM =	(invM[0,0]*invM[1,1]*invM[2,2] + invM[0,1]*invM[1,2]*invM[2,0] + invM[0,2]*invM[1,0]*invM[2,1])
// 			-   (invM[2,0]*invM[1,1]*invM[0,2] + invM[2,1]*invM[1,2]*invM[0,0] + invM[2,2]*invM[1,0]*invM[0,1]);
// if ( Math.Abs( detM - 1.0/detInvM ) > 1e-6 )
// 	throw new Exception( "Determinant discrepancy!" );


// 				// Clear it up so it's always in the required final form
// 				invM[0,1] = 0;
// 				invM[1,0] = 0;
// 				invM[1,2] = 0;
// 				invM[2,1] = 0;
// 				invM[2,2] = 1;

// 			#endif
		}

		public double	Eval( ref float3 _tsLight ) {
			// Transform into original distribution space
			float3	Loriginal = float3.Zero;
// 			#if FIT_TRANSPOSED
// 				Transform( _tsLight, invM, ref Loriginal );	// Compose from the left, as in the shader code!
// 			#else
				Transform( invM, _tsLight, ref Loriginal );	// Compose from the right, as in the paper!
// 			#endif
			float	l = Loriginal.Length;
					Loriginal /= l;

			// Estimate original distribution (a clamped cosine lobe)
			double	D = Math.Max( 0.0, Loriginal.z ) / Math.PI; 

// 			// Compute the Jacobian, roundDwo / roundDw
// 			double	jacobian = 1.0 / (detInvM * l*l*l);

// Ensure we get the same thing as Hill's code, without the 2nd transform
float3	L_ = float3.Zero;
Transform( M, Loriginal, ref L_ );	// Compose from the right, as in the paper!
double	l2 = L_.Length;
double	jacobian = detM / (l2*l2*l2);
//if ( Mathf.Abs( jacobian - jacobian2 ) > 1e-1 )
//	throw new Exception( "Different jacobians!" );
//double	jacobian = jacobian2;

			// Scale distribution
			double	res = amplitude * D / jacobian;
			return res;
		}

		public void	GetSamplingDirection( float _U1, float _U2, ref float3 _direction ) {
			float	theta = Mathf.Asin( Mathf.Sqrt( _U1 ) );
//			float	theta = Mathf.Acos( Mathf.Sqrt( _U1 ) );
			float	phi = Mathf.TWOPI * _U2;
// 			#if FIT_TRANSPOSED
// 				Transform( new float3( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) ), M, ref _direction );
// 			#else
				Transform( M, new float3( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) ), ref _direction );
// 			#endif
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

// 		void			Transform( float3 a, double[,] b, ref float3 c ) {
// 			c.x = (float) (a.x * b[0,0] + a.y * b[1,0] + a.z * b[2,0]);
// 			c.y = (float) (a.x * b[0,1] + a.y * b[1,1] + a.z * b[2,1]);
// 			c.z = (float) (a.x * b[0,2] + a.y * b[1,2] + a.z * b[2,2]);
// 		}
		void			Transform( double[,] a, float3 b, ref float3 c ) {

// Annoying GLM library details:
// return vec3(
// 	m[0][0] * v.x + m[1][0] * v.y + m[2][0] * v.z,
// 	m[0][1] * v.x + m[1][1] * v.y + m[2][1] * v.z,	  (thank God, they didn't change the math!)
// 	m[0][2] * v.x + m[1][2] * v.y + m[2][2] * v.z);


			c.x = (float) (b.x * a[0,0] + b.y * a[0,1] + b.z * a[0,2]);
			c.y = (float) (b.x * a[1,0] + b.y * a[1,1] + b.z * a[1,2]);
			c.z = (float) (b.x * a[2,0] + b.y * a[2,1] + b.z * a[2,2]);
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
