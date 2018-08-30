//#define FIT_WITH_BFGS				// Use BFGS instead of Nelder-Mead
//#define COMPUTE_ERROR_NO_MIS		// Compute error from hemisphere sampling, don't use multiple importance sampling

#define SHOW_RELATIVE_ERROR


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
//	• The fitter works on matrix M: they initialize it with appropriate directions and amplitude fitting the BRDF's
//		Then the m11, m22, m13 parameters are the ones composing the matrix M and they're the ones that are fit
//		At each step, the inverse M matrix is computed and forced into its runtime form:
//			| m11'   0   m13' |
//	 M^-1 =	|  0    m22'  0   |
//			| m31'   0   m33' |
//
//		►►► WARNING: Notice the prime! They are NOT THE SAME as the m11, m22, m13 fitting parameters of the M matrix!
//
//	• The runtime matrix M^-1 is renormalized by m11', which is apparently more stable and easier to interpolate according to hill
//		We thus obtain the following runtime matrix with the 4 coefficients that need to be stored into a texture:
//			|  1     0   m13" |
//	 M^-1 =	|  0    m22"  0   |
//			| m31"   0   m33" |
//
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;

namespace LTCTableGenerator
{
	public partial class FitterForm : Form {

		#region CONSTANTS

		const int		MAX_ITERATIONS = 100;
		const float		FIT_EXPLORE_DELTA = 0.05f;
		const float		TOLERANCE = 1e-5f;
		const float		MIN_ALPHA = 0.00001f;		// minimal roughness (avoid singularities)

		#endregion

		#region NESTED TYPES

		#if FIT_WITH_BFGS

		class	FitModel : BFGS.IModel {

			public LTC		m_LTC;
			public IBRDF	m_BRDF;
			public float3	m_tsView;
			public float	m_alpha;
			public bool		m_isotropic = false;

			#region IModel Members

			public double[] Parameters {
				get { return m_LTC.GetFittingParms(); }
				set { m_LTC.SetFittingParms( value, m_isotropic ); }
			}

			public double Eval( double[] _newParameters ) {
				m_LTC.SetFittingParms( _newParameters, m_isotropic );
				double	error = ComputeError( m_LTC, m_BRDF, ref m_tsView, m_alpha );
				if ( error < 0.0 )
					throw new Exception( "Negative error!" );
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

		#region FIELDS

		// Fiiting data
		IBRDF				m_BRDF = null;
		int					m_tableSize;
		System.IO.FileInfo	m_tableFileName;

		#if FIT_WITH_BFGS
			BFGS			m_fitter = new BFGS();
			FitModel		m_fitModel = new FitModel();
		#else
			NelderMead		m_fitter = new NelderMead( new LTC().GetFittingParms().Length );
		#endif

		// Results
		LTC[,]				m_results;
		int					m_validResultsCount = 0;

		int					m_errorsCount = 0;
		string				m_errors = null;

		// Rendering
		uint				m_width;
		float4[]			m_falseSpectrum;

		ImageFile			m_imageSource;
		ImageFile			m_imageTarget;
		ImageFile			m_imageDifference;

		// Fitting stats
		int					m_statsCounter;
		int					m_statsNormalizationCounter;
		double				m_lastError;
		int					m_lastIterationsCount;
		double				m_lastNormalization;

		double				m_statsSumError;
		double				m_statsSumErrorWithoutHighValues;
		double				m_statsSumNormalization;
		int					m_statsSumIterations;

		#endregion

		#region PROPERTIES

		public IBRDF	BRDF {
			get { return m_BRDF; }
			set { m_BRDF = value; }
		}

		public bool		Paused {
			get { return checkBoxPause.Checked; }
			set {
				checkBoxPause.Checked = value;
				checkBoxPause.Text = Paused ? "PLAY" : "PAUSE";

				integerTrackbarControlRoughnessIndex.Enabled = Paused;
				integerTrackbarControlThetaIndex.Enabled = Paused;
				panelMatrixCoefficients.Enabled = Paused;
			}
		}

		// Options
		public bool		AutoRun { get { return checkBoxAutoRun.Checked; } set { checkBoxAutoRun.Checked = value; } }
		public bool		DoFitting { get { return checkBoxDoFitting.Checked; } set { checkBoxDoFitting.Checked = value; } }
		public bool		UsePreviousRoughness { get { return checkBoxUsePreviousRoughness.Checked; } set { checkBoxUsePreviousRoughness.Checked = value; } }

		bool			m_readOnly = false;
		public bool		ReadOnly { get { return m_readOnly; } set { m_readOnly = value; } }

		bool			m_renderBRDF = false;
		public bool		RenderBRDF { get { return m_renderBRDF; } set { m_renderBRDF = value; } }

		bool			m_useAdaptiveFit = false;
		public bool		UseAdaptiveFit { get { return m_useAdaptiveFit; } set { m_useAdaptiveFit = value; } }

		bool			m_retryFitOnLargeError = true;
		public bool		RetryFitOnLargeError { get { return m_retryFitOnLargeError; } set { m_retryFitOnLargeError = value; } }


		// Info
		public int		RoughnessIndex { get { return integerTrackbarControlRoughnessIndex.Value; } set { integerTrackbarControlRoughnessIndex.Value = value; } }
		public int		ThetaIndex { get { return integerTrackbarControlThetaIndex.Value; } set { integerTrackbarControlThetaIndex.Value = value; } }
		public int		StepX { get { return integerTrackbarControlStepX.Value; } }
		public int		StepY { get { return integerTrackbarControlStepY.Value; } }

		#endregion

		#region METHODS

		public FitterForm() {
			InitializeComponent();

			m_width = (uint) panelOutputSourceBRDF.Width;

			ColorProfile	profile = new ColorProfile( ColorProfile.STANDARD_PROFILE.LINEAR );

			m_imageSource = new ImageFile( (uint) panelOutputSourceBRDF.Width, (uint) panelOutputSourceBRDF.Height, PIXEL_FORMAT.BGRA8, profile );
			m_imageTarget = new ImageFile( (uint) panelOutputTargetBRDF.Width, (uint) panelOutputTargetBRDF.Height, PIXEL_FORMAT.BGRA8, profile );
			m_imageDifference = new ImageFile( (uint) panelOutputDifference.Width, (uint) panelOutputDifference.Height, PIXEL_FORMAT.BGRA8, profile );

			// Read false spectrum colors
			ImageFile	I = new ImageFile( Properties.Resources.FalseColorsSpectrum2, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_falseSpectrum = new float4[I.Width];
			I.ReadScanline( 0, m_falseSpectrum );

			#if SHOW_RELATIVE_ERROR
				label7.Text = "1e4";
				label3.Text = "Relative Error";
			#else
				label7.Text = "1e0";
				label3.Text = "Absolute Error";
			#endif

			// Prevent high gradients
// 			#if FIT_WITH_BFGS
// 				m_fitter.GRADIENT_EPS = 0.005;
// 			#endif

			Application.Idle += Application_Idle;
		}

		public double[,]	SetupBRDF( IBRDF _BRDF, int _tableSize, System.IO.FileInfo _tableFileName ) {
			m_BRDF = _BRDF;
			m_tableSize = _tableSize;

			// Reload or initialize results
			if ( _tableFileName == null )
				_tableFileName = new System.IO.FileInfo( "DefaultTable_" + DateTime.Now.ToLongTimeString().Replace( ":", "-" ) + ".ltc" );
			m_tableFileName = _tableFileName;
			if ( m_tableFileName.Exists )
				m_results = LoadTable( m_tableFileName, out m_validResultsCount );
			else
				m_results = new LTC[m_tableSize,m_tableSize];

			// Reset trackbars
			m_internalChange = true;
			integerTrackbarControlRoughnessIndex.RangeMax = m_tableSize-1;
			integerTrackbarControlRoughnessIndex.VisibleRangeMax = m_tableSize-1;
//			integerTrackbarControlRoughnessIndex.Value = m_tableSize-1;

			integerTrackbarControlThetaIndex.RangeMax = m_tableSize-1;
			integerTrackbarControlThetaIndex.VisibleRangeMax = m_tableSize-1;
//			integerTrackbarControlThetaIndex.Value = 0;
			m_internalChange = false;

			if ( RenderBRDF )
				UpdateView();

			// Check the BRDF is correctly normalized
			double[,]	sums = CheckBRDFNormalization( m_BRDF );
			return sums;
		}

		#region Main Computation Loop

		bool	m_internalChange = false;
		void Application_Idle( object sender, EventArgs e ) {
			if ( m_internalChange || Paused )
				return;

			m_internalChange = true;

			float	alpha, cosTheta;
			GetRoughnessAndAngle( RoughnessIndex, ThetaIndex, m_tableSize, m_tableSize, out alpha, out cosTheta );

			float3	tsView = new float3( Mathf.Sqrt( 1 - cosTheta*cosTheta ), 0, cosTheta );

			LTC		ltc = m_results[RoughnessIndex,ThetaIndex];
			if ( ltc == null && DoFitting ) {
				// ============= Do a new fitting! ============= 
				ltc = new LTC();
				m_results[RoughnessIndex,ThetaIndex] = ltc;

				ltc.ComputeAverageTerms( m_BRDF, ref tsView, alpha );

				// 1. first guess for the fit
				// init the hemisphere in which the distribution is fitted
				// if theta == 0 the lobe is rotationally symmetric and aligned with Z = (0 0 1)
				LTC		previousLTC = null;
				bool	isotropic;
				if ( ThetaIndex == 0 ) {
					ltc.X = float3.UnitX;
					ltc.Y = float3.UnitY;
					ltc.Z = float3.UnitZ;

					if ( RoughnessIndex == m_tableSize-1 || m_results[RoughnessIndex+1,ThetaIndex] == null ) {
						// roughness = 1 or no available result
						ltc.m11 = 1.0f;
						ltc.m22 = 1.0f;
					} else {
						// init with roughness of previous fit
						previousLTC = m_results[RoughnessIndex+1,ThetaIndex];
						ltc.m11 = previousLTC.m11;
						ltc.m22 = previousLTC.m22;
					}

					ltc.m13 = 0;

					isotropic = true;
				} else {
					// Otherwise use average direction as Z vector
					// And use previous configuration as first guess
// 					if ( m_useAdaptiveFit ) {
// 						const int	CRITICAL_THETA_INDEX = 56;	// Above this index, start using previous angle
// 						if ( RoughnessIndex < m_tableSize-1 && ThetaIndex < CRITICAL_THETA_INDEX )
// 							previousLTC = m_results[RoughnessIndex+1,ThetaIndex];	// Always use previous roughness if above critical angle
// 						else
// 							previousLTC = m_results[RoughnessIndex,ThetaIndex-1];	// Above critical angle, just use previous angle
// 
// 					} else {
// 						if ( RoughnessIndex < m_tableSize-1 && checkBoxUsePreviousRoughness.Checked )
// 							previousLTC = m_results[RoughnessIndex+1,ThetaIndex];	// At low roughness, prefer using same angle, but previous roughness!
// 						else
// 							previousLTC = m_results[RoughnessIndex,ThetaIndex-1];	// At high roughness, prefer using same roughness but previous angle!
// 					}

					previousLTC = m_results[RoughnessIndex,ThetaIndex-1];
					if ( previousLTC != null ) {
						ltc.m11 = previousLTC.m11;
						ltc.m22 = previousLTC.m22;
						ltc.m13 = previousLTC.m13;
					}

					isotropic = false;
				}
				ltc.Update();

				// Find best-fit LTC lobe (scale, alphax, alphay)
				try {
					if ( ltc.magnitude > 1e-6 ) {
						#if FIT_WITH_BFGS
							m_fitModel.m_LTC = ltc;
							m_fitModel.m_BRDF = m_BRDF;
							m_fitModel.m_alpha = alpha;
							m_fitModel.m_tsView = tsView;
							m_fitModel.m_isotropic = isotropic;

							m_fitter.Minimize( m_fitModel );
							ltc.error = m_fitter.FunctionMinimum;
							ltc.iterationsCount = m_fitter.IterationsCount;

							// Now check if the error is too large compared to previous result
							if ( m_retryFitOnLargeError && previousLTC != null ) {
								const double	CRITICAL_ERROR_RATIO = 3.0;

								LTC	ltcPrevRoughness = RoughnessIndex < m_tableSize-1 ? m_results[RoughnessIndex+1,ThetaIndex] : null;
								LTC	ltcPrevTheta = ThetaIndex > 0 ? m_results[RoughnessIndex,ThetaIndex-1] : null;

								LTC	retryLTC = null;
								if ( previousLTC == ltcPrevRoughness ) {
									// Check error ratio
									double	errorRatio = ltc.error / ltcPrevRoughness.error;
									if ( errorRatio > CRITICAL_ERROR_RATIO )
										retryLTC = ltcPrevTheta;		// Retry with previous theta instead
								} else if ( previousLTC == ltcPrevTheta ) {
									// Check error ratio
									double	errorRatio = ltc.error / ltcPrevTheta.error;
									if ( errorRatio > CRITICAL_ERROR_RATIO )
										retryLTC = ltcPrevRoughness;	// Retry with previous roughness instead
								}

								if ( retryLTC != null ) {
									// Retry with new primer!
									double	errorM11 = ltc.m11;
									double	errorM22 = ltc.m22;
									double	errorM13 = ltc.m13;
									double	errorM31 = ltc.m31;

									ltc.m11 = retryLTC.m11;
									ltc.m22 = retryLTC.m22;
									ltc.m13 = retryLTC.m13;
									ltc.m31 = retryLTC.m31;
									ltc.Update();

									m_fitter.Minimize( m_fitModel );

									if ( m_fitter.FunctionMinimum < ltc.error ) {
										// New error is lower! => Accept new result
										ltc.error = m_fitter.FunctionMinimum;
										ltc.iterationsCount = m_fitter.IterationsCount;
									} else {
										// New error is higher! => Restore previous results
										ltc.m11 = errorM11;
										ltc.m22 = errorM22;
										ltc.m13 = errorM13;
										ltc.m31 = errorM31;
										ltc.Update();
									}
								}
							}

							// Check final params
							double[]	resultParms = ltc.RuntimeParameters;
							if ( double.IsNaN(resultParms[0]) || double.IsNaN(resultParms[1]) || double.IsNaN(resultParms[2]) || double.IsNaN(resultParms[3]) )
								throw new Exception( "NaN in solution" );

						#else
// ltc.m11 = 1;
// ltc.m22 = 1;
// ltc.m13 = 0;
// ltc.Update();

//m_fitter.log.WriteLine( "LTC m11 = {0}, m22 = {1}, m13 = {2}", ltc.m11, ltc.m22, ltc.m13 );
//m_fitter.log.WriteLine( "LTC Z = {{ {0}, {1}, {2} }}", ltc.Z.x, ltc.Z.y, ltc.Z.z );
//m_fitter.log.WriteLine( "LTC mag = {0}, fresnel = {1}", ltc.magnitude, ltc.fresnel );
//m_fitter.log.WriteLine();


							float[]	startFit = ltc.GetFittingParms();
							float[]	resultFit = new float[startFit.Length];

							ltc.error = m_fitter.FindFit( resultFit, startFit, FIT_EXPLORE_DELTA, TOLERANCE, MAX_ITERATIONS, ( float[] _parameters ) => {
								ltc.SetFittingParms( _parameters, isotropic );

								double	currentError = ComputeError( ltc, m_BRDF, ref tsView, alpha );
								return (float) currentError;
							} );
							ltc.iterationsCount = m_fitter.m_lastIterationsCount;

							// Update LTC with final best fitting values
							ltc.SetFittingParms( resultFit, isotropic );
						#endif
					}
				} catch ( Exception _e ) {
					// Clear LTC!
					ltc = null;
					m_results[RoughnessIndex,ThetaIndex] = ltc;

					m_errorsCount++;
					m_errors += "An error occurred at [" + RoughnessIndex + ", " + ThetaIndex + "]: " + _e.Message + "\r\n";
				}
			}

			// Show debug form
			m_validResultsCount++;
			if ( RenderBRDF )
				ShowBRDF( (float) m_validResultsCount / (m_tableSize*m_tableSize), Mathf.Acos( cosTheta ), alpha, m_BRDF, ltc );

			// Iterate...
			if ( ThetaIndex < m_tableSize-1 ) {
				ThetaIndex++;
			} else {
				// Next line!
				if ( DoFitting && !ReadOnly ) {
					SaveTable( m_tableFileName, m_results, m_errors );
				}

				ThetaIndex = 0;
				if ( RoughnessIndex > 0 ) {
					RoughnessIndex--;
				} else {
					// Terminate...
					Paused = true;
					if ( DoFitting && !ReadOnly ) {
//						Application.Exit();
						Application.Idle -= new EventHandler( Application_Idle );
						Close();
					}
				}
			}
			if ( !AutoRun )
				Paused = true;

			m_internalChange = false;
		}

		public static void	GetRoughnessAndAngle( int _roughnessIndex, int _thetaIndex, int _tableSizeRoughness, int _tableSizeTheta, out float _alpha, out float _cosTheta ) {

			// alpha = perceptualRoughness^2  (perceptualRoughness = "sRGB" representation of roughness, as painted by artists)
			float perceptualRoughness = (float) _roughnessIndex / (_tableSizeRoughness-1);
			_alpha = Mathf.Max( MIN_ALPHA, perceptualRoughness * perceptualRoughness );

			// parameterised by sqrt(1 - cos(theta))
			float	x = (float) _thetaIndex / (_tableSizeTheta - 1);
			_cosTheta = 1.0f - x*x;
			_cosTheta = Mathf.Max( 3.7540224885647058065387021283285e-4f, _cosTheta );	// Clamp to cos(1.57)
		}

		/// <summary>
		/// Ensures the provided BRDF is normalized for various values of view and roughness
		/// </summary>
		/// <param name="_BRDF"></param>
		public double[,]	CheckBRDFNormalization( IBRDF _BRDF ) {

			const int	THETA_VIEW_VALUES_COUNT = 8;
			const int	ROUGHNESS_VALUES_COUNT = 32;

			double	pdf;
			float3	tsView = new float3();
			float3	tsLight = new float3();

			double[,]	sums = new double[ROUGHNESS_VALUES_COUNT,THETA_VIEW_VALUES_COUNT];
			for ( int roughnessIndex=0; roughnessIndex < ROUGHNESS_VALUES_COUNT; roughnessIndex++ ) {
				float	perceptualRoughness = (float) roughnessIndex / (ROUGHNESS_VALUES_COUNT-1);
				float	alpha = Mathf.Max( MIN_ALPHA, perceptualRoughness * perceptualRoughness );

				for ( int thetaIndex=0; thetaIndex < THETA_VIEW_VALUES_COUNT; thetaIndex++ ) {
					float	x = (float) thetaIndex * Mathf.HALFPI / Math.Max( 1, THETA_VIEW_VALUES_COUNT-1 );
					float	cosTheta = 1.0f - x*x;
							cosTheta = Mathf.Max( 3.7540224885647058065387021283285e-4f, cosTheta );	// Clamp to cos(1.57)
					tsView.Set( Mathf.Sqrt( 1 - cosTheta*cosTheta ), 0, cosTheta );

					double	sum = 0;

					// Uniform sampling
// 					const float	dtheta = 0.005f;
// 					const float	dphi = 0.025f;
// 					for( float theta = 0.0f; theta < Mathf.HALFPI; theta+=dtheta ) {
// 						for( float phi = 0.0f; phi <= Mathf.PI; phi+=dphi ) {
// 							tsLight.Set( Mathf.Sin(theta)*Mathf.Cos(phi), Mathf.Sin(theta)*Mathf.Sin(phi), Mathf.Cos(theta) );
// 							sum += Mathf.Sin(theta) * _BRDF.Eval( ref tsView, ref tsLight, alpha, out pdf );
// 						}
// 					}
// 					sum *= dtheta * 2*dphi;

					// Importance sampling
					int	samplesCount = 0;
					for( float u=0; u <= 1; u+=0.02f ) {
						for( float v=0; v < 1; v+=0.02f ) {
							_BRDF.GetSamplingDirection( ref tsView, alpha, u, v, ref tsLight );
							double	V = _BRDF.Eval( ref tsView, ref tsLight, alpha, out pdf );
							if ( pdf > 0.0 )
								sum += V / pdf;
							samplesCount++;
						}
					}
					sum /= samplesCount;

					sums[roughnessIndex,thetaIndex] = sum;
				}
			}

			return sums;
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
			const int	SAMPLES_COUNT = 32;			// number of samples used to compute the error during fitting

			// Compute the error between the BRDF and the LTC using Multiple Importance Sampling
			static double	ComputeError( LTC _LTC, IBRDF _BRDF, ref float3 _tsView, float _alpha ) {
				float3	tsLight = float3.Zero;

				double	pdf_BRDF, eval_BRDF;
				double	pdf_LTC, eval_LTC;

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
							pdf_LTC = eval_LTC / _LTC.magnitude;
							double	error = Math.Abs( eval_BRDF - eval_LTC );
							#if !FIT_WITH_BFGS
								error = error*error*error;		// Use L3 norm to favor large values over smaller ones
							#endif

							#if DEBUG
								if ( pdf_LTC + pdf_BRDF < 0.0 )
									throw new Exception( "Negative PDF!" );
							#endif

							if ( error != 0.0 )
								error /= pdf_LTC + pdf_BRDF;

//							#if FIT_WITH_BFGS
//								error = error*error*error;		// Use L3 norm to favor large values over smaller ones
//							#endif

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
							pdf_LTC = eval_LTC / _LTC.magnitude;
							double	error = Math.Abs( eval_BRDF - eval_LTC );
							#if !FIT_WITH_BFGS
								error = error*error*error;		// Use L3 norm to favor large values over smaller ones
							#endif

							#if DEBUG
								if ( pdf_LTC + pdf_BRDF < 0.0 )
									throw new Exception( "Negative PDF!" );
							#endif

							if ( error != 0.0 )
								error /= pdf_LTC + pdf_BRDF;

//							#if FIT_WITH_BFGS
//								error = error*error*error;		// Use L3 norm to favor large values over smaller ones
//							#endif

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

		public static LTC[,]	LoadTable( System.IO.FileInfo _tableFileName, out int _validResultsCount ) {
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

		public static void	SaveTable( System.IO.FileInfo _tableFileName, LTC[,] _table, string _errors ) {
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

			if ( _errors == null )
				return;	// Nothing to report!

			try {
				System.IO.FileInfo	logFileName = new System.IO.FileInfo( _tableFileName.FullName + ".errorLog" );
				using ( System.IO.TextWriter W = logFileName.CreateText() )
					W.Write( _errors );
			} catch ( Exception ) {
				// Silently fail logging errors... :/
			}
		}

		#endregion

		#endregion

		#region BRDF Rendering

		float	m_theta;
		float	m_roughness;
		LTC		m_LTC;
		float3	m_tsView;
		float3	m_tsLight;

		bool	m_renderingBRDF = false;
		public void	ShowBRDF( float _progress, float _theta, float _roughness, IBRDF _BRDF, LTC _LTC ) {
			if ( m_renderingBRDF )
				return;

			m_renderingBRDF = true;

			m_theta = _theta;
			m_roughness = _roughness;
			m_BRDF = _BRDF;
			m_LTC = _LTC;

			this.Text = "Fitter Debugger " + m_BRDF.GetType().Name + " - Theta = " + Mathf.ToDeg(_theta).ToString( "G3" ) + "° - Roughness = " + _roughness.ToString( "G3" ) + " - Error = " + (_LTC != null ? _LTC.error.ToString( "G4" ) : "not computed") + " - Progress = " + (100.0f * _progress).ToString( "G3" ) + "%";

			// Build up stats
			if ( _LTC != null )
				AccumulateStatistics( _LTC, true );

			// Build fixed view vector
			m_tsView.x = Mathf.Sin( m_theta );
			m_tsView.y = 0;
			m_tsView.z = Mathf.Cos( m_theta );

			// Recompute images
			if ( true ) {

				#if DEBUG
					// Render BRDF and compute integral as well...
					float	sum = 0;
					m_imageSource.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { sum += RenderSphereCalc( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateBRDF( ref _tsView, ref _tsLight ); } ); } );
				#else
					m_imageSource.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateBRDF( ref _tsView, ref _tsLight ); } ); } );
				#endif

				if ( m_LTC != null ) {
					m_imageTarget.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateLTC( ref _tsView, ref _tsLight ); } ); } );
				} else {
					m_imageTarget.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { _color = (((_X >> 2) ^ (_Y >> 2)) & 1) == 0 ? new float4( 1, 0, 0, 1 ) : float4.UnitW; } );
				}

				if ( m_LTC != null ) {
					#if SHOW_RELATIVE_ERROR
						m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, 4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => {
							float	V0 = EstimateBRDF( ref _tsView, ref _tsLight );
							float	V1 = EstimateLTC( ref _tsView, ref _tsLight );
							float	relativeError = (V0 > V1 ? V0 / Math.Max( 1e-6f, V1 ) : V1 / Math.Max( 1e-6f, V0 )) - 1.0f;
									relativeError *= Math.Min( Math.Abs( V0 ), Math.Abs( V1 ) );	// Weigh by the value itself to give very low importance to small values after all
							return relativeError;
						} ); } );
					#else
						m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, 0, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => {
//							utiliser error!
							return Mathf.Abs( EstimateBRDF( ref _tsView, ref _tsLight ) - EstimateLTC( ref _tsView, ref _tsLight ) );
						} ); } );
					#endif
				} else {
					m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { _color = (((_X >> 2) ^ (_Y >> 2)) & 1) == 0 ? new float4( 1, 0, 0, 1 ) : float4.UnitW; } );
				}

				// Assign bitmaps
				panelOutputSourceBRDF.PanelBitmap = m_imageSource.AsBitmap;
				panelOutputTargetBRDF.PanelBitmap = m_imageTarget.AsBitmap;
				panelOutputDifference.PanelBitmap = m_imageDifference.AsBitmap;

				panelOutputSourceBRDF.Refresh();
				panelOutputTargetBRDF.Refresh();
				panelOutputDifference.Refresh();
			}

			// Update text
			if ( m_LTC == null ) {
				textBoxFitting.Text = "<NOT COMPUTED>";
				labelError.Text = "<NOT COMPUTED>";

				m_renderingBRDF = false;
				return;
			}

			textBoxFitting.Text = "m11 = " + _LTC.m11 + "\r\n"
								+ "m22 = " + _LTC.m22 + "\r\n"
								+ "m13 = " + _LTC.m13 + "\r\n"
								+ "\r\n"
								+ "Magnitude = " + _LTC.magnitude + "\r\n"
								+ "Fresnel = " + _LTC.fresnel + "\r\n"
								+ "\r\n"
								+ "invM = \r\n"
								+ " r0 = " + WriteRow( 0, _LTC.invM ) + "\r\n"
								+ " r1 = " + WriteRow( 1, _LTC.invM ) + "\r\n"
								+ " r2 = " + WriteRow( 2, _LTC.invM ) + "\r\n"
								+ "\r\n"
								+ "► Error:\r\n"
								+ "Avg. = " + (m_statsSumError / m_statsCounter).ToString( "G4" ) + "\r\n"
								+ "Avg. (Clipped) = " + (m_statsSumErrorWithoutHighValues / m_statsCounter).ToString( "G4" ) + "\r\n"
								+ "\r\n"
								+ "► Normalization:\r\n"
								+ "Value = " + m_lastNormalization.ToString( "G4" ) + "\r\n"
								+ "Avg. = " + (m_statsSumNormalization / m_statsNormalizationCounter).ToString( "G4" ) + "\r\n"
								+ "\r\n"
								+ "► Iterations:\r\n"
								+ "Value = " + m_lastIterationsCount + "\r\n"
								+ "Avg. = " + (m_statsSumIterations / m_statsCounter).ToString( "G4" ) + "\r\n";

			// Update coefficients
			floatTrackbarControl_m11.Value = (float) _LTC.m11;
			floatTrackbarControl_m22.Value = (float) _LTC.m22;
			floatTrackbarControl_m13.Value = (float) _LTC.m13;
			labelError.Text = "Error: " + _LTC.error;

			m_renderingBRDF = false;
		}

		string		WriteRow( int _rowIndex, double[,] _M ) {
			return _M[_rowIndex,0].ToString( "G4" ) + ", " + _M[_rowIndex,1].ToString( "G4" ) + ", " + _M[_rowIndex,2].ToString( "G4" );
		}

		delegate float	RadialAmplitudeDelegate( ref float3 _tsView, ref float3 _tsLight );

		void	RenderSphere( uint _X, uint _Y, float _log10Min, float _log10Max, ref float4 _color, RadialAmplitudeDelegate _radialAmplitude ) {
			m_tsLight.x = 2.0f * _X / m_width - 1.0f;
			m_tsLight.y = 1.0f - 2.0f * _Y / m_width;
			m_tsLight.z = 1 - m_tsLight.x*m_tsLight.x - m_tsLight.y*m_tsLight.y;
			if ( m_tsLight.z <= 0.0f ) {
				_color.Set( 0, 0, 0, 1 );
				return;
			}

			m_tsLight.z = Mathf.Sqrt( m_tsLight.z );

			// Estimate radial amplitude
			float	V = _radialAmplitude( ref m_tsView, ref m_tsLight );

			// Transform into false spectrum color
			float	logV = Mathf.Clamp( Mathf.Log10( Mathf.Max( 1e-8f, V ) ), _log10Min, _log10Max );
			float	t = (logV - _log10Min) / (_log10Max - _log10Min);
			int		it = Mathf.Clamp( (int) (t * m_falseSpectrum.Length), 0, m_falseSpectrum.Length-1 );
			_color = m_falseSpectrum[it];
		}

		// Same as RenderSphere() but integrates values on the sphere as well
		float	RenderSphereCalc( uint _X, uint _Y, float _log10Min, float _log10Max, ref float4 _color, RadialAmplitudeDelegate _radialAmplitude ) {
			m_tsLight.x = 2.0f * _X / m_width - 1.0f;
			m_tsLight.y = 1.0f - 2.0f * _Y / m_width;
			m_tsLight.z = 1 - m_tsLight.x*m_tsLight.x - m_tsLight.y*m_tsLight.y;
			if ( m_tsLight.z <= 0.0f ) {
				_color.Set( 0, 0, 0, 1 );
				return 0;
			}

			m_tsLight.z = Mathf.Sqrt( m_tsLight.z );

			// Estimate radial amplitude
			float	V = _radialAmplitude( ref m_tsView, ref m_tsLight );

			// Transform into false spectrum color
			float	logV = Mathf.Clamp( Mathf.Log10( Mathf.Max( 1e-8f, V ) ), _log10Min, _log10Max );
			float	t = (logV - _log10Min) / (_log10Max - _log10Min);
			int		it = Mathf.Clamp( (int) (t * m_falseSpectrum.Length), 0, m_falseSpectrum.Length-1 );
			_color = m_falseSpectrum[it];

			// Compute solid angle (from http://patapom.com/blog/Math/OrthoSolidAngle/)
			float	solidAngle = (float) ComputeSolidAngle( _X, _Y, m_width, m_width );
			V *= solidAngle;

			return V;
		}

		double  ComputeSolidAngle( uint _X, uint _Y, uint _width, uint _height ) {
			double  x0 = 2.0 * _X / _width - 1.0;
			double  y0 = 2.0 * _Y / _height - 1.0;
			double  x1 = 2.0 * (_X+1) / _width - 1.0;
			double  y1 = 2.0 * (_Y+1) / _height - 1.0;

			double  A0, A1, A2, A3;
			if ( !ComputeArea( x0, y0, out A0 ) )
				return 0.0;
			if ( !ComputeArea( x1, y0, out A1 ) )
				return 0.0;
			if ( !ComputeArea( x0, y1, out A2 ) )
				return 0.0;
			if ( !ComputeArea( x1, y1, out A3 ) )
				return 0.0;

			double  dA = A3 - A1 - A2 + A0;
			return Math.Max( 0.0, dA );
		}

		// y ArcTan[x/Sqrt[1 - x^2 - y^2]] + x ArcTan[y/Sqrt[1 - x^2 - y^2]] + 1/2 (ArcTan[(1 - x - y^2)/(y Sqrt[1 - x^2 - y^2])] - ArcTan[(1 + x - y^2)/(y Sqrt[1 - x^2 - y^2])])
		bool    ComputeArea( double x, double y, out double _area ) {
			double  sqRadius = x*x  + y*y;
			if ( sqRadius > 1.0 ) {
				// Outside unit circle
				_area = 0.0;
				return false;
			}

			double  rcpCosTheta = 1.0 / Math.Sqrt( 1.0 - sqRadius );
			if ( double.IsInfinity( rcpCosTheta ) ) {
				if ( x == 0.0 ) x = 1e-12;
				if ( y == 0.0 ) y = 1e-12;
			}
			_area = y * Math.Atan( x * rcpCosTheta ) + x * Math.Atan( y * rcpCosTheta )
				+ 0.5 * (Math.Atan( (1 - x - y*y) * rcpCosTheta / y ) - Math.Atan( (1 + x - y*y) * rcpCosTheta / y ));

			return true;
		}
		float	EstimateBRDF( ref float3 _tsView, ref float3 _tsLight ) {
			double	pdf;
			float	V = (float) m_BRDF.Eval( ref _tsView, ref _tsLight, m_roughness, out pdf );
			return V;
		}

//		BRDF_GGX_NoView	COMPARE = new BRDF_GGX_NoView();
		float	EstimateLTC( ref float3 _tsView, ref float3 _tsLight ) {
// float	pdf;
// return COMPARE.Eval( ref _tsView, ref _tsLight, m_roughness, out pdf ) * 1 / (1 + COMPARE.Lambda( _tsView.z, m_roughness )) / (4 * _tsView.z);	// Apply view-dependent part later

			float	V = (float) m_LTC.Eval( ref _tsLight );
			return V;
		}

		void	AccumulateStatistics( LTC _LTC, bool _fullRefresh ) {
			m_lastError = _LTC.error;
			m_lastIterationsCount = _LTC.iterationsCount;
			if ( _fullRefresh ) {
				m_lastNormalization = _LTC.TestNormalization();
				m_statsSumNormalization += m_lastNormalization;
				m_statsNormalizationCounter++;
			}
			m_statsSumError += m_lastError;
			m_statsSumErrorWithoutHighValues += Math.Min( 1, m_lastError );
			m_statsSumIterations += m_lastIterationsCount;
			m_statsCounter++;
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void checkBoxPause_CheckedChanged( object sender, EventArgs e ) {
			Paused = checkBoxPause.Checked;
		}

		private void integerTrackbarControlRoughnessIndex_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			UpdateView();
		}

		private void integerTrackbarControlThetaIndex_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			UpdateView();
		}

		void	UpdateView() {
			buttonClear.Enabled = false;
			buttonClearRowsFromHere.Enabled = false;
			buttonClearColumnsFromHere.Enabled = false;
			if ( m_results == null )
				return;	// Not loaded yet
			if ( m_internalChange || !Paused )
				return;	// Currently fitting...

			float	alpha, cosTheta;
			GetRoughnessAndAngle( RoughnessIndex, ThetaIndex, m_tableSize, m_tableSize, out alpha, out cosTheta );

			LTC		ltc = m_results[RoughnessIndex,ThetaIndex];

			ShowBRDF( (float) m_validResultsCount / (m_tableSize*m_tableSize), Mathf.Acos( cosTheta ), alpha, m_BRDF, ltc );

			buttonClear.Enabled = ltc != null;
			buttonClearRowsFromHere.Enabled = ltc != null;
			buttonClearColumnsFromHere.Enabled = ltc != null;
		}

		private void buttonClear_Click( object sender, EventArgs e ) {
			m_results[RoughnessIndex,ThetaIndex] = null;
			m_validResultsCount--;
			UpdateView();
		}

		private void buttonClearRowsFromHere_Click( object sender, EventArgs e ) {
			if ( MessageBox.Show( this, "Are you sure you want to clear the table ROWS from this position, down to roughness=0?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;	// Cancel, malheureux!

			int	startRoughnessIndex = checkBoxClearPrev.Checked ? m_tableSize-1 : RoughnessIndex;
			int	endRoughnessIndex = checkBoxClearNext.Checked ? 0 : RoughnessIndex;
			int	thetaIndex = checkBoxClearPrev.Checked ? 0 : ThetaIndex;
			for ( int roughnessIndex=startRoughnessIndex; roughnessIndex >= endRoughnessIndex; roughnessIndex-- ) {
				for ( ; thetaIndex < m_tableSize; thetaIndex++ ) {
					if ( m_results[roughnessIndex,thetaIndex] != null ) {
						m_results[roughnessIndex,thetaIndex] = null;
						m_validResultsCount--;
					}
				}
				thetaIndex = 0;
			}
			UpdateView();
		}

		private void buttonClearColumnsFromHere_Click( object sender, EventArgs e ) {
			if ( MessageBox.Show( this, "Are you sure you want to clear the table COLUMNS from this position, up to cosTheta=PI/2?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;	// Cancel, malheureux!

			int	roughnessIndex = checkBoxClearPrev.Checked ? m_tableSize-1 : RoughnessIndex;
			int	startThetaIndex = checkBoxClearPrev.Checked ? 0 : ThetaIndex;
			int	endThetaIndex = checkBoxClearNext.Checked ? m_tableSize : ThetaIndex+1;
			for ( int thetaIndex=startThetaIndex; thetaIndex < endThetaIndex; thetaIndex++ ) {
				for ( ; roughnessIndex >= 0; roughnessIndex-- ) {
					if ( m_results[roughnessIndex,thetaIndex] != null ) {
						m_results[roughnessIndex,thetaIndex] = null;
						m_validResultsCount--;
					}
				}
				roughnessIndex = m_tableSize-1;
			}
			UpdateView();
		}

		private void floatTrackbarControl_matrix_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			if ( m_internalChange || m_renderingBRDF )
				return;

			LTC		ltc = m_results[RoughnessIndex,ThetaIndex];
			if ( ltc != null ) {
				ltc.m11 = floatTrackbarControl_m11.Value;
				ltc.m22 = floatTrackbarControl_m22.Value;
				ltc.m13 = floatTrackbarControl_m13.Value;
				ltc.Update();

				// Show error
				float	alpha, cosTheta;
				GetRoughnessAndAngle( RoughnessIndex, ThetaIndex, m_tableSize, m_tableSize, out alpha, out cosTheta );

				float3	tsView = new float3( Mathf.Sqrt( 1 - cosTheta*cosTheta ), 0, cosTheta );

				double	error = ComputeError( ltc, m_BRDF, ref tsView, alpha );
				labelError.Text = "Error: " + error;
			}

			UpdateView();
		}

		private void buttonDebugLine_Click( object sender, EventArgs e )
		{
			DebugForm	F = new DebugForm();
			F.Results = m_results;
			float	alpha, cosTheta;
			GetRoughnessAndAngle( RoughnessIndex, 0, m_tableSize, m_tableSize, out alpha, out cosTheta );
			F.DebugRoughness( RoughnessIndex, " - Roughness = " + alpha );
			F.ShowDialog( this );
		}

		#endregion
	}
}
