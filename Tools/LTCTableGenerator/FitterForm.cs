#define FIT_WITH_BFGS				// Use BFGS instead of Nelder-Mead
//#define FIT_INV_M					// Fit M^-1 directly instead of M+inversion
//#define COMPUTE_ERROR_NO_MIS		// Compute error from hemisphere sampling, don't use multiple importance sampling

//#define SHOW_RELATIVE_ERROR


//////////////////////////////////////////////////////////////////////////
// Fitter class for Linearly-Transformed Cosines
// From "Real-Time Polygonal-Light Shading with Linearly Transformed Cosines" (https://eheitzresearch.wordpress.com/415-2/)
// This is a C# re-implementation of the code provided by Heitz et al.
// UPDATE: Using code from Stephen Hill's github repo instead (https://github.com/selfshadow/ltc_code/tree/master/fit)
//////////////////////////////////////////////////////////////////////////
// Some notes:
//	• The fitter probably uses L3 norm error because it's more important to have strong fitting on large values (i.e. the BRDF peak values)
//		than on the mostly 0 values at low roughness
//	  Anyway, this L3 metric is detrimental when fitting with BFGS instead of Nelder-Mead so I simply removed it
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

		const int		MAX_ITERATIONS = 200;
		const float		FIT_EXPLORE_DELTA = 0.05f;
		const float		TOLERANCE = 1e-5f;
		const float		MIN_ALPHA = 0.0001f;		// minimal roughness (avoid singularities)

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

		#region FIELDS

		// Fiiting data
		IBRDF				m_BRDF = null;
		int					m_tableSize;
		System.IO.FileInfo	m_tableFileName;

		#if FIT_WITH_BFGS
			BFGS			m_fitter = new BFGS();
			FitModel		m_fitModel = new FitModel();
		#else
			#if FIT_INV_M	// FIT_4_PARAMETERS
				NelderMead	m_fitter = new NelderMead( 5 );
			#else
				NelderMead	m_fitter = new NelderMead( 3 );
			#endif
		#endif

 		double[]			m_startFit = new double[5];
 		double[]			m_resultFit = new double[5];


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

				floatTrackbarControl_m11.Enabled = Paused;
				floatTrackbarControl_m22.Enabled = Paused;
				floatTrackbarControl_m13.Enabled = Paused;
				floatTrackbarControl_m31.Enabled = Paused;
			}
		}

		public bool		AutoRun { get { return checkBoxAutoRun.Checked; } set { checkBoxAutoRun.Checked = value; } }
		public bool		DoFitting { get { return checkBoxDoFitting.Checked; } set { checkBoxDoFitting.Checked = value; } }

		bool			m_readOnly = false;
		public bool		ReadOnly { get { return m_readOnly; } set { m_readOnly = value; } }

		bool			m_renderBRDF = false;
		public bool		RenderBRDF { get { return m_renderBRDF; } set { m_renderBRDF = value; } }

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
			#if FIT_WITH_BFGS
				m_fitter.GRADIENT_EPS = 0.005;
			#endif

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
			integerTrackbarControlRoughnessIndex.Value = m_tableSize-1;

			integerTrackbarControlThetaIndex.RangeMax = m_tableSize-1;
			integerTrackbarControlThetaIndex.VisibleRangeMax = m_tableSize-1;
			integerTrackbarControlThetaIndex.Value = 0;
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
			GetRoughnessAndAngle( RoughnessIndex, ThetaIndex, out alpha, out cosTheta );

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
				bool	isotropic;
				if ( ThetaIndex == 0 ) {
					if ( RoughnessIndex == m_tableSize-1 || m_results[RoughnessIndex+1,ThetaIndex] == null ) {
						// roughness = 1 or no available result
						ltc.m11 = 1.0f;
						ltc.m22 = 1.0f;
					} else {
						// init with roughness of previous fit
						ltc.m11 = Mathf.Max( m_results[RoughnessIndex+1,ThetaIndex].m11, MIN_ALPHA );
						ltc.m22 = Mathf.Max( m_results[RoughnessIndex+1,ThetaIndex].m22, MIN_ALPHA );
					}

					ltc.m13 = 0;
					ltc.m31 = 0;
					ltc.Update();

					isotropic = true;
				} else {
					// Otherwise use average direction as Z vector
					// And use previous configuration as first guess
					LTC	previousLTC = null;
					if ( RoughnessIndex < m_tableSize-1 && checkBoxUsePreviousRoughness.Checked )
						previousLTC = m_results[RoughnessIndex+1,ThetaIndex];	// At low roughness, prefer using same angle, but previous roughness!
					else
						previousLTC = m_results[RoughnessIndex,ThetaIndex-1];	// At high roughness, prefer using same roughness but previous angle!

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
				m_startFit[0] = ltc.m11;
				m_startFit[1] = ltc.m22;
				m_startFit[2] = ltc.m31;
				#if FIT_INV_M
					m_startFit[3] = ltc.m13;
					m_startFit[4] = ltc.amplitude;
				#else
					m_startFit[3] = ltc.amplitude;
				#endif

				// Find best-fit LTC lobe (scale, alphax, alphay)
				try {
					if ( ltc.amplitude > 1e-6 ) {
						#if FIT_WITH_BFGS
							m_fitModel.m_LTC = ltc;
							m_fitModel.m_BRDF = m_BRDF;
							m_fitModel.m_alpha = alpha;
							m_fitModel.m_tsView = tsView;
							m_fitModel.m_isotropic = isotropic;

							m_fitter.Minimize( m_fitModel );
							ltc.error = m_fitter.FunctionMinimum;
							ltc.iterationsCount = m_fitter.IterationsCount;

							double[]	resultParms = ltc.RuntimeParameters;
							if ( double.IsNaN(resultParms[0]) || double.IsNaN(resultParms[1]) || double.IsNaN(resultParms[2]) || double.IsNaN(resultParms[3]) )
								throw new Exception( "NaN in solution" );

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
					if ( DoFitting && !ReadOnly )
						Close();
				}
			}
			if ( !AutoRun )
				Paused = true;

			m_internalChange = false;
		}

		void	GetRoughnessAndAngle( int _roughnessIndex, int _thetaIndex, out float _alpha, out float _cosTheta ) {

			// alpha = perceptualRoughness^2  (perceptualRoughness = "sRGB" representation of roughness, as painted by artists)
			float perceptualRoughness = (float) _roughnessIndex / (m_tableSize-1);
			_alpha = Mathf.Max( MIN_ALPHA, perceptualRoughness * perceptualRoughness );

			// parameterised by sqrt(1 - cos(theta))
			float	x = (float) _thetaIndex / (m_tableSize - 1);
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

		float			m_theta;
		float			m_roughness;
		LTC				m_LTC;
		float3			m_tsView;
		float3			m_tsLight;

		bool	m_renderingBRDF = false;
		public void	ShowBRDF( float _progress, float _theta, float _roughness, IBRDF _BRDF, LTC _LTC ) {
			if ( m_renderingBRDF )
				return;

			m_renderingBRDF = true;

			m_theta = _theta;
			m_roughness = _roughness;
			m_BRDF = _BRDF;
			m_LTC = _LTC;

			this.Text = "Fitter Debugger - Theta = " + Mathf.ToDeg(_theta).ToString( "G3" ) + "° - Roughness = " + _roughness.ToString( "G3" ) + " - Error = " + (_LTC != null ? _LTC.error.ToString( "G4" ) : "not computed") + " - Progress = " + (100.0f * _progress).ToString( "G3" ) + "%";

			// Build up stats
			if ( _LTC != null )
				AccumulateStatistics( _LTC, true );

			// Build fixed view vector
			m_tsView.x = Mathf.Sin( m_theta );
			m_tsView.y = 0;
			m_tsView.z = Mathf.Cos( m_theta );

			// Recompute images
			if ( true ) {
				m_imageSource.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateBRDF( ref _tsView, ref _tsLight ); } ); } );

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
							return (V0 > V1 ? V0 / Math.Max( 1e-6f, V1 ) : V1 / Math.Max( 1e-6f, V0 )) - 1.0f;
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

			double[]	runtimeParms = _LTC.RuntimeParameters;

			textBoxFitting.Text = "m11 = " + _LTC.m11 + "\r\n"
								+ "m22 = " + _LTC.m22 + "\r\n"
								+ "m13 = " + _LTC.m13 + "\r\n"
								+ "m31 = " + _LTC.m31 + "\r\n"
								+ "\r\n"
								+ "Amplitude = " + runtimeParms[4] + "\r\n"
								+ "Fresnel = " + runtimeParms[5] + "\r\n"
								+ "\r\n"
								+ "invM = \r\n"
								+ "r0 = " + WriteRow( 0, _LTC.invM ) + "\r\n"
								+ "r1 = " + WriteRow( 1, _LTC.invM ) + "\r\n"
								+ "r2 = " + WriteRow( 2, _LTC.invM ) + "\r\n"
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
			floatTrackbarControl_m31.Value = (float) _LTC.m31;
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
//t = (float) _X / m_width;
			int		it = Mathf.Clamp( (int) (t * m_falseSpectrum.Length), 0, m_falseSpectrum.Length-1 );
			_color = m_falseSpectrum[it];
//_color = new float4( 1, 1, 0, 1 );
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
			if ( m_internalChange || !Paused )
				return;	// Currently fitting...

			float	alpha, cosTheta;
			GetRoughnessAndAngle( RoughnessIndex, ThetaIndex, out alpha, out cosTheta );

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

			int	roughnessIndex = RoughnessIndex;
			int	thetaIndex = ThetaIndex;
			for ( ; roughnessIndex >= 0; roughnessIndex-- ) {
				for ( ; thetaIndex < m_tableSize; thetaIndex++ ) {
					m_results[roughnessIndex,thetaIndex] = null;
					m_validResultsCount--;
				}
				thetaIndex = 0;
			}
			UpdateView();
		}

		private void buttonClearColumnsFromHere_Click( object sender, EventArgs e ) {
			if ( MessageBox.Show( this, "Are you sure you want to clear the table COLUMNS from this position, up to cosTheta=PI/2?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes )
				return;	// Cancel, malheureux!

			int	roughnessIndex = RoughnessIndex;
			int	thetaIndex = ThetaIndex;
			for ( ; thetaIndex < m_tableSize; thetaIndex++ ) {
				for ( ; roughnessIndex >= 0; roughnessIndex-- ) {
					m_results[roughnessIndex,thetaIndex] = null;
					m_validResultsCount--;
				}
				roughnessIndex = m_tableSize-1;
			}
			UpdateView();
		}

		private void floatTrackbarControl_m31_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			if ( m_internalChange || m_renderingBRDF )
				return;

			LTC		ltc = m_results[RoughnessIndex,ThetaIndex];
			if ( ltc != null ) {
				ltc.m11 = floatTrackbarControl_m11.Value;
				ltc.m22 = floatTrackbarControl_m22.Value;
				ltc.m13 = floatTrackbarControl_m13.Value;
				ltc.m31 = floatTrackbarControl_m31.Value;
				ltc.Update();

				// Show error
				float	alpha, cosTheta;
				GetRoughnessAndAngle( RoughnessIndex, ThetaIndex, out alpha, out cosTheta );

				float3	tsView = new float3( Mathf.Sqrt( 1 - cosTheta*cosTheta ), 0, cosTheta );

				double	error = ComputeError( ltc, m_BRDF, ref tsView, alpha );
				labelError.Text = "Error: " + error;
			}

			UpdateView();
		}

		#endregion
	}
}
