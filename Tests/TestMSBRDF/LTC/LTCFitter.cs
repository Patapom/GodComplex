#define FIT_WITH_BFGS
//#define FIT_INV_M
//#define COMPUTE_ERROR_NO_MIS

//////////////////////////////////////////////////////////////////////////
// Fitter class for Linearly-Transformed Cosines
// From "Real-Time Polygonal-Light Shading with Linearly Transformed Cosines" (https://eheitzresearch.wordpress.com/415-2/)
// This is a C# re-implementation of the code provided by Heitz et al.
// UPDATE: Using code from Stephen Hill's github repo instead (https://github.com/selfshadow/ltc_code/tree/master/fit)
//////////////////////////////////////////////////////////////////////////
// Some notes:
//	• The fitter probably uses L3 norm error because it's more important to have strong fitting on large values (i.e. the BRDF peak values)
//		than on the mostly 0 values at low roughness
//
//	• I implemented 2 kinds of fitting:
//		Method 1) is the one used by Heitz & Hill
//			They work on M: they initialize it with appropriate directions and amplitude fitting the BRDF's
//			Then the m11, m22, etc. parameters are the ones composing the matrix M and they're the ones that are fit
//			At each step, the inverse M matrix is computed and forced into its runtime form:
//				| m11'   0   m13' |
//				|  0    m22'  0   |
//				| m31'   0    1   |
//			►►► WARNING: Notice the prime! They are NOT THE SAME as the m11, m22, etc. fitting parameters of the M matrix!
//			If you want to be sure, just use the "RuntimeParameters" property that contains the 4 runtime matrix parameters in order + scale & fresnel!
//
//		Method 2) is mine and starts from the target runtime inverse M matrix:
//				| m11   0  m13 |
//				|  0   m22  0  |
//				| m31   0   1  |
//			This time, only the required parameters are fitted
//
//		Overall, method 2) somewhat has a smaller error but sometimes (especially at grazing angles) we end up with strange lobes
//			that I'm afraid may look strange (like a secondary lobe rising at the antipodes of the first lobe near 90° values)
//
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

		#region CONSTANTS

		const int		MAX_ITERATIONS = 200;
		const float		FIT_EXPLORE_DELTA = 0.05f;
		const float		TOLERANCE = 1e-5f;
		const float		MIN_ALPHA = 0.0001f;		// minimal roughness (avoid singularities)

		#endregion

		#region NESTED TYPES

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

						amplitude += weight;
						fresnel += weight * Math.Pow( 1 - _tsView.Dot( H ), 5.0 );
						Z += (float) weight * tsLight;
					}
				}
				amplitude /= SAMPLES_COUNT*SAMPLES_COUNT;
				fresnel /= SAMPLES_COUNT*SAMPLES_COUNT;

				// Finish building the average TBN orthogonal basis
				Z.y = 0.0f;		// clear y component, which should be zero with isotropic BRDFs
				Z.Normalize();
				X.Set( Z.z, 0, -Z.x );
				Y = float3.UnitY;

// 				#if FIT_INV_M
// 					// Compute intial inverse M coefficients to match average direction best
// 					M = new float3x3( X, Y, Z );
// 					invM = M.Inverse;
// 
// 					m11 = invM.r0.x;
// 					m22 = invM.r1.y;
// 					m13 = invM.r0.z;
// 					m31 = invM.r2.x;
// 				#endif
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

		#if FIT_WITH_BFGS

		class	FitModel : BFGS.IModel {

			public LTC		m_LTC;
			public IBRDF	m_BRDF;
			public float3	m_tsView;
			public float	m_alpha;
			public bool		m_isotropic = false;

			#region IModel Members

			public double[] Parameters {
				get {
					#if FIT_INV_M
						double[]	tempParams = new double[4] {
							m_LTC.m11,
							m_LTC.m22,
							m_LTC.m31,
							m_LTC.m13
						};
					#else
						double[]	tempParams = new double[3] {
							m_LTC.m11,
							m_LTC.m22,
							m_LTC.m31
						};
					#endif
					return tempParams;
				}
				set {
					m_LTC.Set( value, m_isotropic );
				}
			}

			public double Eval( double[] _newParameters ) {
				m_LTC.Set( _newParameters, m_isotropic );
				double	error = ComputeError( m_LTC, m_BRDF, ref m_tsView, m_alpha );
				return error;
			}

			public void Constrain( double[] _parameters ) {
//				_parameters[0] = Math.Max( 0.002, _parameters[0] );
//				_parameters[1] = Math.Max( 0.002, _parameters[1] );
			}

			#endregion
		}

		#endif

		#endregion

		FitterForm	m_debugForm = null;

		public LTCFitter( Form _ownerForm ) {
			if ( _ownerForm == null )
				return;	// No debug...

			m_debugForm = new FitterForm( this );
			m_debugForm.Show( _ownerForm );
		}

		public LTC[,]	Fit( IBRDF _BRDF, int _tableSize, System.IO.FileInfo _tableFileName ) {

			#if FIT_WITH_BFGS
				BFGS		fitter = new BFGS();
							fitter.GRADIENT_EPS = 0.005;
				FitModel	fitModel = new FitModel();
			#else
				#if FIT_INV_M	// FIT_4_PARAMETERS
					NelderMead	fitter = new NelderMead( 5 );
				#else
					NelderMead	fitter = new NelderMead( 3 );
				#endif
			#endif

 			double[]	startFit = new double[5];
 			double[]	resultFit = new double[5];

			LTC[,]		result = new LTC[_tableSize,_tableSize];
			int			validResultsCount = 0;

			if ( _tableFileName == null )
				_tableFileName = new System.IO.FileInfo( "DefaultTable_" + DateTime.Now.ToLongTimeString().Replace( ":", "-" ) + ".ltc" );
			if ( _tableFileName.Exists )
				result = Load( _tableFileName, out validResultsCount );

			int	roughnessIndex = _tableSize-1;
			int	thetaIndex = 0;
			if ( m_debugForm != null ) {
				roughnessIndex = m_debugForm.RoughnessIndex;
				thetaIndex = m_debugForm.ThetaIndex;
			}

			// Handle manual scrolling in results
			m_debugForm.TrackbarValueChanged += () => {
				if ( !m_debugForm.Paused )
					return;	// Let auto

				thetaIndex = m_debugForm.ThetaIndex;
				float	x = (float) thetaIndex / (_tableSize - 1);
				float	cosTheta = 1.0f - x*x;
						cosTheta = Mathf.Max( 3.7540224885647058065387021283285e-4f, cosTheta );	// Clamp to cos(1.57)

				// alpha = perceptualRoughness^2  (perceptualRoughness = "sRGB" representation of roughness, as painted by artists)
				roughnessIndex = m_debugForm.RoughnessIndex;
				float perceptualRoughness = (float) roughnessIndex / (_tableSize-1);
				float alpha = Mathf.Max( MIN_ALPHA, perceptualRoughness * perceptualRoughness );

 				m_debugForm.ShowBRDF( (float) validResultsCount / (_tableSize*_tableSize), Mathf.Acos( cosTheta ), alpha, _BRDF, result[m_debugForm.RoughnessIndex,m_debugForm.ThetaIndex], false );

				// This to ensure the next thetaIndex++ gets canceled!
				thetaIndex--;
			};

			// loop over theta and alpha
			for ( ; roughnessIndex >= 0; roughnessIndex -= (m_debugForm!=null ? m_debugForm.StepY : 1) ) {
				if ( m_debugForm != null )
					m_debugForm.RoughnessIndex = roughnessIndex;

				for ( ; thetaIndex <= _tableSize-1; thetaIndex += (m_debugForm != null ? m_debugForm.StepX : 1) ) {
//thetaIndex = _tableSize - 3;
//m_debugForm.Paused = true;

					if ( m_debugForm != null )
						m_debugForm.ThetaIndex = thetaIndex;

					if ( result[roughnessIndex,thetaIndex] != null ) {
						validResultsCount++;
						if ( m_debugForm != null )
							m_debugForm.AccumulateStatistics( result[roughnessIndex,thetaIndex], false );
						continue;	// Already computed!
					}

					// parameterised by sqrt(1 - cos(theta))
					float	x = (float) thetaIndex / (_tableSize - 1);
					float	cosTheta = 1.0f - x*x;
							cosTheta = Mathf.Max( 3.7540224885647058065387021283285e-4f, cosTheta );	// Clamp to cos(1.57)
					float3	tsView = new float3( Mathf.Sqrt( 1 - cosTheta*cosTheta ), 0, cosTheta );

					// alpha = perceptualRoughness^2  (perceptualRoughness = "sRGB" representation of roughness, as painted by artists)
					float perceptualRoughness = (float) roughnessIndex / (_tableSize-1);
					float alpha = Mathf.Max( MIN_ALPHA, perceptualRoughness * perceptualRoughness );

					LTC	ltc = new LTC();
					result[roughnessIndex,thetaIndex] = ltc;

					ltc.ComputeAverageTerms( _BRDF, ref tsView, alpha );

					// 1. first guess for the fit
					// init the hemisphere in which the distribution is fitted
					// if theta == 0 the lobe is rotationally symmetric and aligned with Z = (0 0 1)
					bool	isotropic;
					if ( thetaIndex == 0 ) {
						if ( roughnessIndex == _tableSize-1 || result[roughnessIndex+1,thetaIndex] == null ) {
							// roughness = 1 or no available result
							ltc.m11 = 1.0f;
							ltc.m22 = 1.0f;
						} else {
							// init with roughness of previous fit
							ltc.m11 = Mathf.Max( result[roughnessIndex+1,thetaIndex].m11, MIN_ALPHA );
							ltc.m22 = Mathf.Max( result[roughnessIndex+1,thetaIndex].m22, MIN_ALPHA );
						}

						ltc.m13 = 0;
						ltc.m31 = 0;
						ltc.Update();

						isotropic = true;
					} else {
						// Otherwise use average direction as Z vector

						// And use previous configuration as first guess
						LTC	previousLTC = result[roughnessIndex,thetaIndex-1];
						if ( previousLTC != null ) {
							ltc.m11 = previousLTC.m11;
							ltc.m22 = previousLTC.m22;
							ltc.m13 = previousLTC.m13;
							ltc.m31 = previousLTC.m31;
							ltc.Update();
						}

						isotropic = false;
					}

					// 2. fit (explore parameter space and refine first guess)
					startFit[0] = ltc.m11;
					startFit[1] = ltc.m22;
					startFit[2] = ltc.m31;
					#if FIT_INV_M
						startFit[3] = ltc.m13;
						startFit[4] = ltc.amplitude;
					#else
						startFit[3] = ltc.amplitude;
					#endif

					// Find best-fit LTC lobe (scale, alphax, alphay)
					if ( m_debugForm == null || m_debugForm.DoFitting ) {
						#if FIT_WITH_BFGS
							fitModel.m_LTC = ltc;
							fitModel.m_BRDF = _BRDF;
							fitModel.m_alpha = alpha;
							fitModel.m_tsView = tsView;
							fitModel.m_isotropic = isotropic;

							fitter.Minimize( fitModel );
							ltc.error = fitter.FunctionMinimum;
							ltc.iterationsCount = fitter.IterationsCount;
						#else
							ltc.error = fitter.FindFit( resultFit, startFit, FIT_EXPLORE_DELTA, TOLERANCE, MAX_ITERATIONS, ( double[] _parameters ) => {
								ltc.Set( _parameters, isotropic );

								double	currentError = ComputeError( ltc, _BRDF, ref tsView, alpha );
								return currentError;
							} );
							ltc.iterationsCount = fitter.m_lastIterationsCount;

							// Update LTC with final best fitting values
							ltc.Set( resultFit, isotropic );
						#endif
					}

					// Show debug form
					validResultsCount++;
					if ( m_debugForm != null ) {
						m_debugForm.AccumulateStatistics( ltc, true );
						m_debugForm.ShowBRDF( (float) validResultsCount / (_tableSize*_tableSize), Mathf.Acos( cosTheta ), alpha, _BRDF, ltc, true );

						// Check if we should continue computing
						if ( !m_debugForm.AutoRun )
							m_debugForm.Paused = true;	// Pause after computation
					}
				}

				thetaIndex = 0;	// Clear after first line

				Save( _tableFileName, result );
			}

			return result;
		}

		#region Objective Function

		#if COMPUTE_ERROR_NO_MIS
			const int	HEMISPHERE_RADIUS = 32;
			const int	HEMISPHERE_DIAMETER = 1+2*HEMISPHERE_RADIUS;
			static double	ComputeError( LTC _LTC, IBRDF _BRDF, ref float3 _tsView, float _alpha ) {
				float3	L = float3.Zero;
				float3	tsLight = float3.Zero;

				double	pdf_BRDF, eval_BRDF, eval_LTC;

				float3	tsReflection = 2.0f * _tsView.z * float3.UnitZ - _tsView;	// Expected reflection direction will be used as new hemisphere pole
tsReflection = _LTC.Z;	// Use preferred direction
				float3	T = new float3( tsReflection.z, 0, -tsReflection.x );
				float3	B = tsReflection.Cross( T );

				float	scalePower = Mathf.Pow( 10.0f, Mathf.Lerp( 2, 0, _alpha ) );	// Group samples closer to reflection direction based on roughness

				double	sumError = 0.0;
				for ( int Y=0; Y < HEMISPHERE_DIAMETER; Y++ ) {
					L.y = (float) (HEMISPHERE_RADIUS - Y) / (HEMISPHERE_RADIUS+1);
					L.y = Mathf.Sign(L.y) * Mathf.Pow( Math.Abs(L.y), scalePower );
					for ( int X=0; X < HEMISPHERE_DIAMETER; X++ ) {
						L.x = (float) (X - HEMISPHERE_RADIUS) / (HEMISPHERE_RADIUS+1);
						L.x = Mathf.Sign(L.x) * Mathf.Pow( Math.Abs(L.x), scalePower );
						L.z = 1.0f - L.x*L.x - L.y*L.y;
						if ( L.z <= 0.0f )
							continue;	// Outside hemisphere area

						L.z = Mathf.Sqrt( L.z );

						// Transform into tangent space
						tsLight = L.x * T + L.y * B + L.z * tsReflection;

						// Estimate BRDF
						eval_BRDF = _BRDF.Eval( ref _tsView, ref tsLight, _alpha, out pdf_BRDF );
						eval_LTC = _LTC.Eval( ref tsLight );

						double	error = Math.Abs( eval_BRDF - eval_LTC );
//								error = error*error*error;		// Use L3 norm to favor large values over smaller ones

						sumError += error;
					}
				}
				return sumError;
			}
		#else
			const int	SAMPLES_COUNT = 50;			// number of samples used to compute the error during fitting

			// Compute the error between the BRDF and the LTC using Multiple Importance Sampling
			static double	ComputeError( LTC _LTC, IBRDF _BRDF, ref float3 _tsView, float _alpha ) {
				float3	tsLight = float3.Zero;

				double	pdf_BRDF, eval_BRDF;
				double	pdf_LTC, eval_LTC;

// 				double	maxBRDF = _BRDF.MaxValue( ref _tsView, _alpha );
// 				double	maxLTC = _LTC.MaxValue;
//				double	recMaxValue = 1.0 / Math.Max( maxBRDF, maxLTC );

				double	sumError = 0.0;
				for ( int j = 0 ; j < SAMPLES_COUNT ; ++j ) {
					for ( int i = 0 ; i < SAMPLES_COUNT ; ++i ) {
						float	U1 = (i+0.5f) / SAMPLES_COUNT;
						float	U2 = (j+0.5f) / SAMPLES_COUNT;

						// importance sample LTC
						{
							// sample
							_LTC.GetSamplingDirection( U1, U2, ref tsLight );
				
							// error with MIS weight
							eval_BRDF = _BRDF.Eval( ref _tsView, ref tsLight, _alpha, out pdf_BRDF );
							eval_LTC = _LTC.Eval( ref tsLight );

// eval_BRDF *= recMaxValue;
// eval_LTC *= recMaxValue;

							pdf_LTC = eval_LTC / _LTC.amplitude;
							double	error = Math.Abs( eval_BRDF - eval_LTC );
//									error = error*error*error;		// Use L3 norm to favor large values over smaller ones

	// 						if ( pdf_LTC + pdf_BRDF < 1e-12 )
	// 							throw new Exception( "NaN!" );
	// 						sumError += error / (pdf_LTC + pdf_BRDF);

							if ( error != 0.0 )
								error /= pdf_LTC + pdf_BRDF;
							if ( double.IsNaN( error ) )
								throw new Exception( "NaN!" );
							sumError += error;
						}

						// importance sample BRDF
						{
							// sample
							_BRDF.GetSamplingDirection( ref _tsView, _alpha, U1, U2, ref tsLight );

							// error with MIS weight
							eval_BRDF = _BRDF.Eval( ref _tsView, ref tsLight, _alpha, out pdf_BRDF );			
							eval_LTC = _LTC.Eval( ref tsLight );

// eval_BRDF *= recMaxValue;
// eval_LTC *= recMaxValue;

							pdf_LTC = eval_LTC / _LTC.amplitude;
							double	error = Math.Abs( eval_BRDF - eval_LTC );
//									error = error*error*error;		// Use L3 norm to favor large values over smaller ones

	// 						if ( pdf_LTC + pdf_BRDF < 1e-12 )
	// 							throw new Exception( "NaN!" );
	// 						sumError += error / (pdf_LTC + pdf_BRDF);

							if ( error != 0.0 )
								error /= pdf_LTC + pdf_BRDF;
							if ( double.IsNaN( error ) )
								throw new Exception( "NaN!" );
							sumError += error;
						}
					}
				}

				sumError /= SAMPLES_COUNT * SAMPLES_COUNT;
				return sumError;
			}
		#endif

		#endregion

		#region I/O

		LTC[,]	Load( System.IO.FileInfo _tableFileName, out int _validResultsCount ) {
			LTC[,]	result = null;
			_validResultsCount = 0;
			using ( System.IO.FileStream S = _tableFileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
					result = new LTC[R.ReadUInt32(), R.ReadUInt32()];
					for ( uint Y=0; Y < result.GetLength( 1 ); Y++ ) {
						for ( uint X=0; X < result.GetLength( 0 ); X++ ) {
							if ( R.ReadBoolean() ) {
								result[X,Y] = new LTC( R );
								_validResultsCount++;
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

		#endregion
	}
}
