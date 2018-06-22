//////////////////////////////////////////////////////////////////////////
// Fitter class for Linearly-Transformed Cosines
// From "Real-Time Polygonal-Light Shading with Linearly Transformed Cosines" (https://eheitzresearch.wordpress.com/415-2/)
// This is a C# re-implementation of the code provided by Heitz et al.
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace TestMSBRDF.LTC
{
	public class LTCFitter {

		const int		MAX_ITERATIONS = 100;
		const float		FIT_EXPLORE_DELTA = 0.05f;
		const float		TOLERANCE = 1e-5f;
		const float		MIN_ALPHA = 0.0001f;		// minimal roughness (avoid singularities)

		[System.Diagnostics.DebuggerDisplay( "m11={m11}, m22={m22}, m13={m13}, m23={m23} - Amplitude = {amplitude}" )]
		public class LTC {

			// lobe amplitude
			public float		amplitude = 1;

			// parametric representation
			public float		m11 = 1, m22 = 1, m13 = 0, m23 = 0;
			public float3		X = float3.UnitX;
			public float3		Y = float3.UnitY;
			public float3		Z = float3.UnitZ;

			// Matrix representation
			public float3x3		M;
			public float3x3		invM;
			public float		detM;

			public LTC() {
				Update();
			}
			public LTC( System.IO.BinaryReader R ) {
				Read( R );
				Update();
			}

			public void	Set( float[] _parameters, bool _isotropic ) {
				float	tempM11 = Mathf.Max( _parameters[0], MIN_ALPHA );
				float	tempM22 = Mathf.Max( _parameters[1], MIN_ALPHA );
				float	tempM13 = _parameters[2];
				float	tempM23 = _parameters[3];

				if ( _isotropic ) {
					m11 = tempM11;
					m22 = tempM11;
					m13 = 0.0f;
					m23 = 0.0f;
				} else {
					m11 = tempM11;
					m22 = tempM22;
					m13 = tempM13;
					m23 = tempM23;
				}
				Update();
			}

			// Update matrix from parameters
			public void	Update() {
				M = new float3x3( X, Y, Z );
				float3x3	temp = new float3x3(	m11,	0,		0,
													0,		m22,	0,
													m13,	m23,	1 );
				M *= temp;
				Update2();
			}
			void Update2() {
				invM = M.Inverse;
				detM = M.Determinant;
			}

			public void	CleanMatrixAndNormalize() {
// 				// kill useless coefs in matrix and normalize
// 				tab[a+t*_tableSize][0][1] = 0;
// 				tab[a+t*_tableSize][1][0] = 0;
// 				tab[a+t*_tableSize][2][1] = 0;
// 				tab[a+t*_tableSize][1][2] = 0;
// 				tab[a+t*_tableSize] = 1.0f / tab[a+t*_tableSize][2][2] * tab[a+t*_tableSize];
			}

			public float Eval( ref float3 _tsLight ) {
				float3	Loriginal = (invM * _tsLight).Normalized;
				float3	L_ = M * Loriginal;

				float	l = L_.Length;
				float	jacobian = detM / (l*l*l);

				float	D = Mathf.INVPI * Math.Max( 0.0f, Loriginal.z ); 

				float	res = amplitude * D / jacobian;
				return res;
			}

			public void	GetSamplingDirection( float _U1, float _U2, ref float3 _direction ) {
				float	theta = Mathf.Acos(Mathf.Sqrt(_U1));
				float	phi = Mathf.TWOPI * _U2;
				_direction = M * new float3( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) );
				_direction.Normalize();
			}

			void testNormalization() {
				double	sum = 0;
				float	dtheta = 0.005f;
				float	dphi = 0.005f;
				float3	L = new float3();
				for( float theta = 0.0f; theta <= Mathf.PI; theta+=dtheta ) {
					for( float phi = 0.0f; phi <= Mathf.TWOPI; phi+=dphi ) {
						L.Set( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) );
						sum += Mathf.Sin(theta) * Eval( ref L );
					}
				}
				sum *= dtheta * dphi;
// 				cout << "LTC normalization test: " << sum << endl;
// 				cout << "LTC normalization expected: " << amplitude << endl;
			}

			public void	Read( System.IO.BinaryReader R ) {
				m11 = R.ReadSingle();
				m22 = R.ReadSingle();
				m13 = R.ReadSingle();
				m23 = R.ReadSingle();
				amplitude = R.ReadSingle();

				X.x = R.ReadSingle();
				X.y = R.ReadSingle();
				X.z = R.ReadSingle();
				Y.x = R.ReadSingle();
				Y.y = R.ReadSingle();
				Y.z = R.ReadSingle();
				Z.x = R.ReadSingle();
				Z.y = R.ReadSingle();
				Z.z = R.ReadSingle();
			}

			public void	Write( System.IO.BinaryWriter W ) {
				W.Write( m11 );
				W.Write( m22 );
				W.Write( m13 );
				W.Write( m23 );
				W.Write( amplitude );

				W.Write( X.x );
				W.Write( X.y );
				W.Write( X.z );
				W.Write( Y.x );
				W.Write( Y.y );
				W.Write( Y.z );
				W.Write( Z.x );
				W.Write( Z.y );
				W.Write( Z.z );
			}
		}

		FitterForm	m_debugForm = null;

		public LTCFitter( Form _ownerForm ) {
			if ( _ownerForm == null )
				return;	// No debug...

			m_debugForm = new FitterForm( this );
			m_debugForm.Show( _ownerForm );
		}

		public LTC[,]	Fit( IBRDF _brdf, int _tableSize, System.IO.FileInfo _tableFileName ) {

			NelderMead	NMFitter = new NelderMead( 4 );

			float[]		startFit = new float[4];
			float[]		resultFit = new float[4];

			LTC[,]		result = new LTC[_tableSize,_tableSize];

			if ( _tableFileName.Exists )
				result = Load( _tableFileName );

			// loop over theta and alpha
			int	count = 0;
			for ( int a = _tableSize-1; a >= 0; --a ) {
				for ( int t=0; t <= _tableSize-1; ++t ) {
					if ( result[a,t] != null )
						continue;	// Already computed!

					float	theta = t * Mathf.HALFPI / (_tableSize-1);
					float3	V = new float3( Mathf.Sin(theta), 0 , Mathf.Cos(theta) );

					// alpha = roughness^2
					float roughness = (float) a / (_tableSize-1);
					float alpha = Mathf.Max( MIN_ALPHA, roughness*roughness );

					LTC	ltc = new LTC();
					result[a,t] = ltc;

					ltc.amplitude = ComputeNorm( _brdf, ref V, alpha ); 
					float3	averageDir = ComputeAverageDir( _brdf, ref V, alpha );		
					bool	isotropic;

					// 1. first guess for the fit
					// init the hemisphere in which the distribution is fitted
					// if theta == 0 the lobe is rotationally symmetric and aligned with Z = (0 0 1)
					if ( t == 0 ) {
						ltc.X = float3.UnitX;
						ltc.Y = float3.UnitY;
						ltc.Z = float3.UnitZ;

						if ( a == _tableSize-1 ) {
							// roughness = 1
							ltc.m11 = 1.0f;
							ltc.m22 = 1.0f;
						} else {
							// init with roughness of previous fit
							ltc.m11 = Mathf.Max( result[a+1,t].m11, MIN_ALPHA );
							ltc.m22 = Mathf.Max( result[a+1,t].m22, MIN_ALPHA );
						}
			
						ltc.m13 = 0;
						ltc.m23 = 0;
						ltc.Update();

						isotropic = true;
					} else {
						// otherwise use previous configuration as first guess
						float3	L = averageDir.Normalized;
						float3	T1 = new float3( L.z, 0, -L.x );
						float3	T2 = new float3( 0, 1, 0);
						ltc.X = T1;
						ltc.Y = T2;
						ltc.Z = L;

						ltc.Update();

						isotropic = false;
					}

					// 2. fit (explore parameter space and refine first guess)
					startFit[0] = ltc.m11;
					startFit[1] = ltc.m22;
					startFit[2] = ltc.m13;
					startFit[3] = ltc.m23;

					// Find best-fit LTC lobe (scale, alphax, alphay)
					float	error = NMFitter.FindFit( resultFit, startFit, FIT_EXPLORE_DELTA, TOLERANCE, MAX_ITERATIONS, ( float[] _parameters ) => {
						ltc.Set( _parameters, isotropic );

						float	currentError = ComputeError( ltc, _brdf, ref V, alpha );
						return currentError;
					} );

					// Update LTC with best fitting values
					ltc.m11 = resultFit[0];
					ltc.m22 = resultFit[1];
					ltc.m13 = resultFit[2];
					ltc.m23 = resultFit[3];
					ltc.Update();
					ltc.CleanMatrixAndNormalize();

					// Show debug form
					++count;
//					if ( m_debugForm != null && (count & 0xF) == 1 ) {
					if ( m_debugForm != null ) {
						m_debugForm.ShowBRDF( (float) count / (_tableSize*_tableSize), error, theta, roughness, _brdf, ltc );
					}
				}

				Save( _tableFileName, result );
			}

			return result;
		}

		LTC[,]	Load( System.IO.FileInfo _tableFileName ) {
			LTC[,]	result = null;
			using ( System.IO.FileStream S = _tableFileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
					result = new LTC[R.ReadUInt32(), R.ReadUInt32()];
					for ( uint Y=0; Y < result.GetLength( 1 ); Y++ ) {
						for ( uint X=0; X < result.GetLength( 0 ); X++ ) {
							if ( R.ReadBoolean() ) {
								result[X,Y] = new LTC( R );
							}
						}
					}
				}

			return result;
		}

		void	Save( System.IO.FileInfo _tableFileName, LTC[,] _table ) {
			using ( System.IO.FileStream S = _tableFileName.Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
					W.Write( _table.GetLength( 0 ) );
					W.Write( _table.GetLength( 1 ) );
					for ( uint Y=0; Y < _table.GetLength( 1 ); Y++ )
						for ( uint X=0; X < _table.GetLength( 0 ); X++ ) {
							LTC	ltc = _table[X,Y];
							if ( ltc == null ) {
								W.Write( false );
								continue;
							}

							W.Write( true );
							ltc.Write( W );
						}
				}
		}

		#region Objective Function

		const int		SAMPLES_COUNT = 50;			// number of samples used to compute the error during fitting

		// compute the norm (albedo) of the BRDF
		float	ComputeNorm( IBRDF brdf, ref float3 V, float alpha ) {
			float3	L = float3.Zero;
			float	norm = 0.0f;
			for ( int j = 0 ; j < SAMPLES_COUNT ; ++j ) {
				for ( int i = 0 ; i < SAMPLES_COUNT ; ++i ) {
					float U1 = (i+0.5f) / SAMPLES_COUNT;
					float U2 = (j+0.5f) / SAMPLES_COUNT;

					// sample
					brdf.GetSamplingDirection( ref V, alpha, U1, U2, ref L );

					// eval
					float	pdf;
					float	eval = brdf.Eval( ref V, ref L, alpha, out pdf );

					// accumulate
					norm += pdf > 0 ? eval / pdf : 0.0f;
				}
			}

			norm /= SAMPLES_COUNT*SAMPLES_COUNT;
			return norm;
		}

		// compute the average direction of the BRDF
		float3	ComputeAverageDir( IBRDF brdf, ref float3 V, float alpha ) {
			float3	L = float3.Zero;
			float3	averageDir = float3.Zero;
			for ( int j = 0 ; j < SAMPLES_COUNT ; ++j ) {
				for ( int i = 0 ; i < SAMPLES_COUNT ; ++i ) {
					float U1 = (i+0.5f) / SAMPLES_COUNT;
					float U2 = (j+0.5f) / SAMPLES_COUNT;

					// sample
					brdf.GetSamplingDirection( ref V, alpha, U1, U2, ref L );

					// eval
					float	pdf;
					float	eval = brdf.Eval( ref V, ref L, alpha, out pdf );

					// accumulate
					averageDir += pdf > 0 ? eval / pdf * L : float3.Zero;
				}
			}

			// clear y component, which should be zero with isotropic BRDFs
			averageDir.y = 0.0f;

			averageDir.Normalize();
			return averageDir;
		}

		// compute the error between the BRDF and the LTC
		// using Multiple Importance Sampling
		float	ComputeError( LTC ltc, IBRDF brdf, ref float3 V, float alpha ) {
			float3	L = float3.Zero;
			double	error = 0.0;
			for ( int j = 0 ; j < SAMPLES_COUNT ; ++j ) {
				for ( int i = 0 ; i < SAMPLES_COUNT ; ++i ) {
					float U1 = (i+0.5f) / SAMPLES_COUNT;
					float U2 = (j+0.5f) / SAMPLES_COUNT;

					// importance sample LTC
					{
						// sample
						ltc.GetSamplingDirection( U1, U2, ref L );
				
						// error with MIS weight
						float	pdf_brdf;
						float	eval_brdf = brdf.Eval( ref V, ref L, alpha, out pdf_brdf );
						float	eval_ltc = ltc.Eval( ref L );
						float	pdf_ltc = eval_ltc / ltc.amplitude;
						double	error_ = Mathf.Abs( eval_brdf - eval_ltc );
								error_ = error_*error_*error_;
						error += error_ / (pdf_ltc + pdf_brdf);
					}

					// importance sample BRDF
					{
						// sample
						brdf.GetSamplingDirection( ref V, alpha, U1, U2, ref L );

						// error with MIS weight
						float	pdf_brdf;
						float	eval_brdf = brdf.Eval( ref V, ref L, alpha, out pdf_brdf );			
						float	eval_ltc = ltc.Eval( ref L );
						float	pdf_ltc = eval_ltc / ltc.amplitude;
						double	error_ = Mathf.Abs( eval_brdf - eval_ltc );
								error_ = error_*error_*error_;
						error += error_ / (pdf_ltc + pdf_brdf);
					}
				}
			}

			return (float) (error / (SAMPLES_COUNT*SAMPLES_COUNT));
		}

		#endregion
	}
}
