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

	[System.Diagnostics.DebuggerDisplay( "m11={m11}, m22={m22}, m13={m13} - Magnitude = {magnitude} - Fresnel = {fresnel}" )]
	public class LTC {

		// Lobe amplitude
		public double		magnitude = 1;

		// Average Schlick Fresnel term
		public double		fresnel = 1;

		// Parametric representation (used by the fitter only!)
		public float3		X = float3.UnitX;
		public float3		Y = float3.UnitY;
		public float3		Z = float3.UnitZ;
		public double		m11 = 1, m22 = 1, m13 = 0;
		public double[,]	M = new double[3,3];

		public double		error;				// Last fitting error
		public int			iterationsCount;	// Last amount of iterations

		// Runtime matrix representation
		public double[,]	invM = new double[3,3];
		public double		detM;

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
		public float[]	GetFittingParms() {
			float[]	tempParams = new float[] {
				(float) m11,
				(float) m22,
				(float) m13,
			};
			return tempParams;
		}

		/// <summary>
		/// Sets the coefficients for the M matrix (warning! NOT the inverse matrix used at runtime!)
		/// </summary>
		/// <param name="_parameters"></param>
		/// <param name="_isotropic"></param>
		public void	SetFittingParms( float[] _parameters, bool _isotropic ) {
 			double	tempM11 = Math.Max( _parameters[0], 1e-7 );
 			double	tempM22 = Math.Max( _parameters[1], 1e-7 );
 			double	tempM13 = _parameters[2];

			if ( _isotropic ) {
				m11 = tempM11;
				m22 = tempM11;
				m13 = 0.0;
			} else {
				m11 = tempM11;
				m22 = tempM22;
				m13 = tempM13;
			}

			Update();	// Update the matrices
		}

		// Heitz & Hill Method => Fit M, inverse to obtain target matrix
		public void		Update() {

// Code from Heitz et al., as given by the example code
// 					M = mat3(X, Y, Z) *
// 						mat3(m11, 0, 0,	// column 0
// 							0, m22, 0,	// column 1
// 							m13, 0, 1);	// column 2
// 					invM = inverse(M);
// 					detM = abs(glm::determinant(M));

// Annoying GLM library details:
// 	struct mat3 {
// 		vec3	value[3];	// COLUMNS!!!!!
// 	}
// 
// 	mat3( T x0, T y0, T z0,
// 		  T x1, T y1, T z1,
// 		  T x2, T y2, T z2 ) {
// 			this->value[0] = vec3(x0, y0, z0);
// 			this->value[1] = vec3(x1, y1, z1);
// 			this->value[2] = vec3(x2, y2, z2);
// 	}
//
// 	mat3(vec3 const& v0, vec3 const& v1, vec3 const& v2) {
// 			this->value[0] = v0;
// 			this->value[1] = v1;
// 			this->value[2] = v2;
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
			// Build the source matrix M for which we're exploring the parameter space
			M[0,0] = m11*X.x;
			M[0,1] = m22*Y.x;
			M[0,2] = m13*X.x + Z.x;

			M[1,0] = m11*X.y;
			M[1,1] = m22*Y.y;
			M[1,2] = m13*X.y + Z.y;

			M[2,0] = m11*X.z;
			M[2,1] = m22*Y.z;
			M[2,2] = m13*X.z + Z.z;

			// Build the final matrix required at runtime for LTC evaluation
			detM = Invert( M, invM );
			if ( detM < 0.0 )
				throw new Exception( "Negative determinant!" );

// DEBUG: Check determinant of M^-1 = reciprocal of determine of M
// detInvM =	(invM[0,0]*invM[1,1]*invM[2,2] + invM[0,1]*invM[1,2]*invM[2,0] + invM[0,2]*invM[1,0]*invM[2,1])
// 			-   (invM[2,0]*invM[1,1]*invM[0,2] + invM[2,1]*invM[1,2]*invM[0,0] + invM[2,2]*invM[1,0]*invM[0,1]);
// if ( Math.Abs( detM - 1.0/detInvM ) > 1e-6 )
// 	throw new Exception( "Determinant discrepancy!" );

			// Kill useless coefs in matrix
			invM[0,1] = 0;	// Row 0 - Col 1
			invM[1,0] = 0;	// Row 1 - Col 0
			invM[1,2] = 0;	// Row 1 - Col 2
			invM[2,1] = 0;	// Row 2 - Col 1
		}

		public double	Eval( ref float3 _tsLight ) {
			// Transform into original distribution space
			float3	Loriginal = float3.Zero;
			Transform( invM, _tsLight, ref Loriginal );
			float	l = Loriginal.Length;
					Loriginal /= l;

			// Estimate original distribution (a clamped cosine lobe)
			double	D = Math.Max( 0.0, Loriginal.z ) / Math.PI; 

 			// Compute the Jacobian, roundDwo / roundDw
 			double	jacobian = 1.0 / (detM * l*l*l);

// DEBUG: Ensure we get the same thing as Hill's code, without the 2nd transform
// float3	L_ = float3.Zero;
// Transform( M, Loriginal, ref L_ );
// double	l2 = L_.Length;
// double	jacobian2 = detM / (l2*l2*l2);
// if ( Mathf.Abs( jacobian - 1.0 / jacobian2 ) > 1e-4 )
// 	throw new Exception( "Different jacobians!" );

			// Scale distribution
			double	res = magnitude * D * jacobian;
			return res;
		}

		public void	GetSamplingDirection( float _U1, float _U2, ref float3 _direction ) {
			float	theta = Mathf.Asin( Mathf.Sqrt( _U1 ) );
//			float	theta = Mathf.Acos( Mathf.Sqrt( _U1 ) );
			float	phi = Mathf.TWOPI * _U2;
			float3	D = new float3( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) );

			Transform( M, D, ref _direction );

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
			magnitude = 0.0;
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

					magnitude += weight;
					fresnel += weight * Math.Pow( 1 - Math.Max( 0.0f, _tsView.Dot( H ) ), 5.0 );
					Z += (float) weight * tsLight;
				}
			}
			magnitude /= SAMPLES_COUNT*SAMPLES_COUNT;
			fresnel /= SAMPLES_COUNT*SAMPLES_COUNT;

			// Finish building the average TBN orthogonal basis
// 			Z.y = 0.0f;		// clear y component, which should be zero with isotropic BRDFs
// 			float	length = Z.Length;
// 			if ( length > 0.0f )
// 				Z /= length;
// 			else
// 				Z = float3.UnitZ;
			Z.Normalize();
			X.Set( Z.z, 0, -Z.x );
			Y = float3.UnitY;
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

// Not used: we always multiply by the right
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
// 			m31 = R.ReadDouble();
			magnitude = R.ReadDouble();
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
// 			W.Write( m31 );
			W.Write( magnitude );
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
