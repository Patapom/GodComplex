//////////////////////////////////////////////////////////////////////////
// This is a program that will attempt to fit a BRDF from the MERL database (http://www.merl.com/brdf/) into
//	several generalized cosine lobes as described in the Lafortune model (http://www.graphics.cornell.edu/pubs/1997/LFTG97.pdf).
// 
// The model "simply" assumes a generalized cosine lobe as in the classic Phong model, except each component in the dot product
//	is weighted by a factor that will alter the lobe.
//
// Take the simple example of a local patch of 2D surface:
//
//		. Wi   ^ N
//		 .     |          . Wr
//		  .    |        .
//		   .   |      .
//		    .  |    .
//		     . |  .
//		      .|.
//	 --------------------> X
//
//	Wi (omega i) is the incoming light direction (pointing TOWARD the light)
//	Wr (omega r) is the view direction (pointing TOWARD the viewer)
//	N is the normal to the surface
//	X is the local tangent to the surface
//
// Wi is first transformed into Wi' through coefficients Cx, Cy, Cz:
//
//	Wi' = [Wi.x * Cx, Wi.y * Cy, Wi.z * Cz]
//
// Then Wi' is dotted with Wr and raised to the power n:
//
//	f = (Wi'.x*Wr.x + Wi'.y*Wr.y + Wi'.z*Wr.z)^n
//
// This is what Lafortune calls the "generalized cosine lobe model".
// It's generalized in the sense that if you use Cx=Cy=-1 and Cz=1 then Wi' = reflect( Wi, N ) is the standard Phong model
//	but other values for Cx,Cy,Cz will change the aspect of the lobe.
//
//---------------------------------------------------------------------------
// So, using Cx,Cy,Cz (each different on R,G and B) and n we can describe a single cosine lobe.
// Using several of these lobes as a sum, we can describe more complex models.
//
// This is the goal of this little application that will attempt to fit complex 4D BRDFs from the MERL database (each file is 33MB!)
//	into several cosine lobes.
//
// Lafortune advises to use the Levenberg-Marquardt algorithm which will perform a least-square fit of a function but I will rather
//	re-use my BFGS minimization scheme I used in my SH library to match several rotated ZH to compose a single SH...
//
// The algorithm then becomes:
//
//	Read and store BRDF into an array
//	For each R,G,B component
//	{
//		For each lobe in LOBES_COUNT (provided as a parameter)
//		{
//			Use BFGS to determine best fit (Cx,Cy,Cz,n) to the BRDF
//			Subtract the resulting cosine lobe from the BRDF (hence removing the effect of the lobe from the actual BRDF)
//		}
//		Final BFGS to determine best fit of all lobes to the original BRDF
//	}
//
// Perhaps it's actually a bad idea to separate the BRDF as the sum of several lobes to compute an approximation lobe by lobe rather than
//	using all the lobes at once, but since the Lafortune approximation works with an independent sum of lobes, why not also rendering the
//	computation of those lobes' coefficients independent from one another?
// There is a small sentence in the Lafortune paper that says all the lobes should be computed together though, I wonder which solution
//	is the best?
//
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using WMath;

namespace BRDFLafortuneFitting
{
	class Program
	{
		#region CONSTANTS



		#endregion

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "x={x} y={y} z={z}" )]
		struct	Vector3
		{
			public double	x, y, z;

			public void		Set( double _x, double _y, double _z )	{ x = _x; y = _y; z = _z; }
			public double	LengthSq()	{ return x*x + y*y + z*z; }
			public double	Length()	{ return Math.Sqrt( LengthSq() ); }
			public void		Normalize()
			{
				double	InvLength = 1.0 / Length();
				x *= InvLength;
				y *= InvLength;
				z *= InvLength;
			}

			public double	Dot( ref Vector3 a )
			{
				return x*a.x + y*a.y + z*a.z;
			}

			public void		Cross( ref Vector3 a, out Vector3 _Out )
			{
				_Out.x = y*a.z - z*a.y;
				_Out.y = z*a.x - x*a.z;
				_Out.z = x*a.y - y*a.x;
			}

			// Rotate vector along one axis
			private static Vector3	TempCross = new Vector3();
			public void		Rotate( ref Vector3 _Axis, double _Angle, out Vector3 _Out )
			{
				double	cos_ang = Math.Cos( _Angle );
				double	sin_ang = Math.Sin( _Angle );

				_Out.x = x * cos_ang;
				_Out.y = y * cos_ang;
				_Out.z = z * cos_ang;

				double	temp = Dot( ref _Axis );
						temp *= 1.0-cos_ang;

				_Out.x += _Axis.x * temp;
				_Out.y += _Axis.y * temp;
				_Out.z += _Axis.z * temp;

				_Axis.Cross( ref this, out TempCross );
	
				_Out.x += TempCross.x * sin_ang;
				_Out.y += TempCross.y * sin_ang;
				_Out.z += TempCross.z * sin_ang;
			}
		}

		struct	CosineLobe
		{
			public Vector3	C;		// Cx, Cy, Cz coefficients of the generalized cosine lobe model
			public double	N;		// Exponent

			public	CosineLobe( Vector3 _C, double _N )
			{
				C = _C;
				N = _N;
			}
		}

		class	LocalFitEvaluationContext
		{
			public double[]		m_BRDF = null;				// The temporary BRDF coefficients
			public CosineLobe	m_Lobe = new CosineLobe();	// The current cosine lobe coefficients
		}

		/// <summary>
		/// The delegate used to evaluate the function to minimize using BFGS
		/// </summary>
		/// <param name="_Coefficients">The array of coefficients where to evaluate the function</param>
		/// <param name="_Params">User params</param>
		/// <returns>The function value for the given coefficients</returns>
		protected delegate double	BFGSFunctionEval( double[] _Coefficients, object _Params );

		/// <summary>
		/// The delegate used to evaluate the function to minimize using BFGS
		/// </summary>
		/// <param name="_Coefficients">The array of coefficients where to evaluate the function</param>
		/// <param name="_Gradient">The evaluated gradient</param>
		/// <param name="_Params">User params</param>
		protected delegate void		BFGSFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params );

		#endregion

		static void Main( string[] args )
		{
			try
			{
				// Analyze arguments
				if ( args.Length != 3 )
					throw new Exception( "Usage: BRDFLafortuneFitting \"Path to MERL BRDF\" \"Path to Lafortune Coeffs file\" AmountOfCosineLobes" );

				FileInfo	SourceBRDF = new FileInfo( args[0] );
				if ( !SourceBRDF.Exists )
					throw new Exception( "Source BRDF file \"" + SourceBRDF.FullName + "\" does not exist!" );
				FileInfo	TargetFile = new FileInfo( args[1] );
				if ( !TargetFile.Directory.Exists )
					throw new Exception( "Target coefficient file's \"" + TargetFile.FullName + "\" directory does not exist!" );
				int	LobesCount = 0;
				if ( !int.TryParse( args[2], out LobesCount ) )
					throw new Exception( "3rd argument must be an integer number!" );
				if ( LobesCount <= 0 || LobesCount > 100 )
					throw new Exception( "Number of lobes must be in [1,100]!" );

				// Load the BRDF
				double[][]	BRDF = LoadBRDF( SourceBRDF );

				// DEBUG CHECK
				{	// Generate a bunch of incoming/outgoing directions, get their Half/Diff angles then regenerate back incoming/outgoing directions from these angles and check the relative incoming/outgoing directions are conserved
					// This is important to ensure we sample only the relevant (i.e. changing) parts of the BRDF in our minimization scheme
					// (I want to actually sample the BRDF using the half/diff angles and generate incoming/outgoing vectors from these, rather than sample all the possible 4D space)
					//
					Random	RNG = new Random( 1 );
					for ( int i=0; i < 10000; i++ )
					{
						double	Phi_i = 2.0 * Math.PI * RNG.NextDouble();
						double	Theta_i = 0.5 * Math.PI * RNG.NextDouble();
						double	Phi_r = 2.0 * Math.PI * RNG.NextDouble();
						double	Theta_r = 0.5 * Math.PI * RNG.NextDouble();

						double	Theta_half, Phi_half, Theta_diff, Phi_diff;
						std_coords_to_half_diff_coords( Theta_i, Phi_i, Theta_r, Phi_r, out Theta_half, out Phi_half, out Theta_diff, out Phi_diff );

						// Convert back...
						double	NewTheta_i, NewPhi_i, NewTheta_r, NewPhi_r;
						half_diff_coords_to_std_coords( Theta_half, Phi_half, Theta_diff, Phi_diff, out NewTheta_i, out NewPhi_i, out NewTheta_r, out NewPhi_r );

						// Check
						const double Tol = 1e-4;
						if ( Math.Abs( NewTheta_i - Theta_i ) > Tol
							|| Math.Abs( NewTheta_r - Theta_r ) > Tol )
							throw new Exception( "ARGH!" );
						if ( Math.Abs( NewPhi_i - Phi_i ) > Tol
							|| Math.Abs( NewPhi_r - Phi_r ) > Tol )
							throw new Exception( "ARGH!" );
					}
				}
				// DEBUG CHECK

				// 

			}
			catch ( Exception _e )
			{
				MessageBox.Show( "An error occurred!\r\n\r\n" + _e.Message + "\r\n\r\n" + _e.StackTrace, "BRDF Fitting", MessageBoxButtons.OK, MessageBoxIcon.Warning );
			}
		}

		/// <summary>
		/// Performs mapping of a BRDF into N sets of cosine lobes coefficients
		/// WARNING: Takes hell of a time to compute !
		/// </summary>
		/// <param name="_BRDF">The BRDF to fit into cosine lobes</param>
		/// <param name="_ZHAxes">The double array of resulting ZH axes (i.e. [θ,ϕ] couples)</param>
		/// <param name="_ZHCoefficients">The double array of resulting ZH coefficients (the dimension of the outter array gives the amount of requested ZH lobes while the dimension of the inner arrays should be of size "_Order")</param>
		/// <param name="_HemisphereResolution">The resolution of the hemisphere used to perform initial guess of directions (e.g. using a resolution of 100 will test for 20 * (4*20) possible lobe orientations)</param>
		/// <param name="_BFGSConvergenceTolerance">The convergence tolerance for the BFGS algorithm (the lower the tolerance, the longer it will compute)</param>
		/// <param name="_FunctionSamplingResolution">The resolution of the sphere used to perform function sampling and measuring the error (e.g. using a resolution of 100 will estimate the function with 100 * (2*100) samples)</param>
		/// <param name="_RMS">The resulting array of RMS errors for each ZH lobe</param>
		/// <param name="_Delegate">An optional delegate to pass the method to get feedback about the mapping as it can be a lengthy process (!!)</param>
// 		protected static void		FitBRDF( double[] _BRDF, CosineLobe[] _Lobes, double[][] _ZHCoefficients, int _HemisphereResolution, int _FunctionSamplingResolution, double _BFGSConvergenceTolerance, out double[] _RMS, ZHMappingFeedback _Delegate )
// 		{
// 			int			LobesCount = _Lobes.GetLength( 0 );
// 			Random		RNG = new Random( 1 );
// 
// 			double[]	LobeCoefficients = new double[_Order];
// 			double[]	SHCoefficients = new double[_Order*_Order];
// 			_SHCoefficients.CopyTo( SHCoefficients, 0 );
// 
// 			_RMS = new double[LobesCount];
// 
// 			// Build the local function evaluation context
// 			LocalFitEvaluationContext	ContextLocal = new LocalFitEvaluationContext();
// 			ContextLocal.m_BRDF = new double[_BRDF.Length];
// 			_BRDF.CopyTo( ContextLocal.m_BRDF, 0 );	// Duplicate BRDF as we will modify it for each new lobe
// 
// 			ContextLocal.m_Lobe.C.Set( 0, 0, 0 );
// 			ContextLocal.m_Lobe.N = 0;
// 
// 			// Pre-compute a table of SH coefficients samples
// 			ContextLocal.m_SHSamples.Initialize( _Order, _FunctionSamplingResolution );
// 			ContextLocal.m_Normalizer = 1.0 / ContextLocal.m_SHSamples.SamplesCount;
// //			ContextLocal.m_Normalizer = 4.0 * Math.PI / ContextGlobal.m_SHSamples.SamplesCount;
// //			ContextLocal.m_Normalizer = 1.0;		// Seems to yield better results with this but it's slow!
// 			ContextLocal.m_SHEvaluation = new double[ContextLocal.m_SHSamples.SamplesCount];
// 
// 			// Prepare the hemisphere of random directions for lobe best fit
// 			Vector2D[]	RandomInitialDirections = new Vector2D[_HemisphereResolution*(4*_HemisphereResolution)];
// 			int		DirectionIndex = 0;
// 			for ( int ThetaIndex=0; ThetaIndex < _HemisphereResolution; ThetaIndex++ )
// 				for ( int PhiIndex=0; PhiIndex < 4*_HemisphereResolution; PhiIndex++ )
// 					RandomInitialDirections[DirectionIndex++] = new Vector2D( (float) Math.Acos( Math.Sqrt( 1.0 - (ThetaIndex + RNG.NextDouble()) / _HemisphereResolution) ), (float) (2.0 * Math.PI * (PhiIndex + RNG.NextDouble()) / (4*_HemisphereResolution) ) );
// 
// 			// Prepare the fixed list of coefficients' denominators
// 			double[]	LobeCoefficientsDenominators = new double[_Order];
// 			for ( int CoefficientIndex=0; CoefficientIndex < _Order; CoefficientIndex++ )
// 				LobeCoefficientsDenominators[CoefficientIndex] = 4.0 * Math.PI / (2 * CoefficientIndex + 1);
// 
// 			// Prepare feedback data
// 			float	fCurrentProgress = 0.0f;
// 			float	fProgressDelta = 1.0f / (LobesCount * _HemisphereResolution * (4*_HemisphereResolution));
// 			int		FeedbackCount = 0;
// 			int		FeedbackThreshold = (LobesCount * _HemisphereResolution*(4*_HemisphereResolution)) / 100;
// 
// 			// Compute the best fit for each lobe
// 			int		CrashesCount = 0;
// 			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
// 			{
// 				// 1] Evaluate goal SH coefficients for every sample directions
// 				//	(changes on a per-lobe basis as we'll later subtract the ZH coefficients from the goal SH coefficients)
// 				for ( int SampleIndex=0; SampleIndex < ContextLocal.m_SHSamples.SamplesCount; SampleIndex++ )
// 				{
// 					SHSamplesCollection.SHSample	Sample = ContextLocal.m_SHSamples.Samples[SampleIndex];
// 					ContextLocal.m_SHEvaluation[SampleIndex] = EvaluateSH( SHCoefficients, Sample.m_Theta, Sample.m_Phi, _Order );
// 				}
// 
// 				// 2] Evaluate lobe approximation given a set of directions and keep the best fit
// 				DirectionIndex = 0;
// 				double	MinError = double.MaxValue;
// 				for ( int ThetaIndex=0; ThetaIndex < _HemisphereResolution; ThetaIndex++ )
// 					for ( int PhiIndex=0; PhiIndex < 4*_HemisphereResolution; PhiIndex++ )
// 					{
// 						// Update external feedback on progression
// 						if ( _Delegate != null )
// 						{
// 							fCurrentProgress += fProgressDelta;
// 							FeedbackCount++;
// 							if ( FeedbackCount > FeedbackThreshold )
// 							{	// Send feedback
// 								FeedbackCount = 0;
// 								_Delegate( fCurrentProgress );
// 							}
// 						}
// 
// 						// 2.1] Set the lobe direction
// 						ContextLocal.m_LobeDirection = RandomInitialDirections[DirectionIndex++];
// 
// 						// 2.2] Find the best approximate initial coefficients given the direction
// 						double[]	Coefficients = new double[1+_Order];
// 									Coefficients[1+0] = SHCoefficients[0];	// We use the exact ambient term (rotational invariant)
// 						for ( int l=1; l < _Order; l++ )
// 						{
// 							Coefficients[1+l] = 0.0;
// 							for ( int m=-l; m <= +l; m++ )
// 								Coefficients[1+l] += SHCoefficients[l*(l+1)+m] * ComputeSH( l, m, ContextLocal.m_LobeDirection.x, ContextLocal.m_LobeDirection.y );
// 
// 							Coefficients[1+l] *= LobeCoefficientsDenominators[l];
// 						}
// 
// 						//////////////////////////////////////////////////////////////////////////
// 						// At this point, we have a fixed direction and the best estimated ZH coefficients to map the provided SH in this direction.
// 						//
// 						// We then need to apply BFGS minimization to optimize the ZH coefficients yielding the smallest possible error...
// 						//
// 
// 						// 2.3] Apply BFGS minimization
// 						int		IterationsCount = 0;
// 						double	FunctionMinimum = double.MaxValue;
// 						try
// 						{
// 							dfpmin( Coefficients, _BFGSConvergenceTolerance, out IterationsCount, out FunctionMinimum, new BFGSFunctionEval( ZHMappingLocalFunctionEval ), new BFGSFunctionGradientEval( ZHMappingLocalFunctionGradientEval ), ContextLocal );
// 						}
// 						catch ( Exception )
// 						{
// 							CrashesCount++;
// 							continue;
// 						}
// 
// 						if ( FunctionMinimum >= MinError )
// 							continue;	// Larger error than best candidate so far...
// 
// 						MinError = FunctionMinimum;
// 
// 						// Save that "optimal" lobe data
// 						_ZHAxes[LobeIndex] = ContextLocal.m_LobeDirection;
// 						for ( int l=0; l < _Order; l++ )
// 							_ZHCoefficients[LobeIndex][l] = Coefficients[1+l];
// 
// 						_RMS[LobeIndex] = FunctionMinimum;
// 				}
// 
// 				//////////////////////////////////////////////////////////////////////////
// 				//
// 				// At this point, we have the "best" ZH lobe fit for the given set of spherical harmonics coefficients
// 				//
// 				// We must subtract the ZH influence from the current Spherical Harmonics, which we simply do by subtracting
// 				//	the rotated ZH coefficients from the current SH.
// 				//
// 				// Then, we are ready to start the process all over again with another lobe, hence fitting the original SH
// 				//	better and better with every new lobe
// 				//
// 
// 				// 3] Rotate the ZH toward the fit axis
// 				double[]	RotatedZHCoefficients = new double[_Order*_Order];
// 				ComputeRotatedZHCoefficients( _ZHCoefficients[LobeIndex], SphericalToCartesian( _ZHAxes[LobeIndex].x, _ZHAxes[LobeIndex].y ), RotatedZHCoefficients );
// 
// 				// 4] Subtract the rotated ZH coefficients to the SH coefficients
// 				for ( int CoefficientIndex=0; CoefficientIndex < _Order*_Order; CoefficientIndex++ )
// 					SHCoefficients[CoefficientIndex] -= RotatedZHCoefficients[CoefficientIndex];
// 			}
// 
// 
// 			//////////////////////////////////////////////////////////////////////////
// 			//
// 			// At this point, we have a set of SH lobes that are individual best fits to the goal SH coefficients
// 			//
// 			// We will finally apply a global BFGS minimzation using the total ZH Lobes' axes and coefficients
// 			//
// 
// 			// Build the function evaluation context
// 			ZHMappingGlobalFunctionEvaluationContext	ContextGlobal = new ZHMappingGlobalFunctionEvaluationContext();
// 														ContextGlobal.m_Order = _Order;
// 														ContextGlobal.m_LobesCount = LobesCount;
// 														// Placeholders
// 														ContextGlobal.m_ZHCoefficients = new double[_Order];
// 														ContextGlobal.m_RotatedZHCoefficients = new double[_Order*_Order];
// 														ContextGlobal.m_SumRotatedZHCoefficients = new double[_Order*_Order];
// 
// 			// Build the array of derivatives deltas
// 			ContextGlobal.m_DerivativesDelta = new double[1+LobesCount * (2+_Order)];
// 			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
// 			{
// 				ContextGlobal.m_DerivativesDelta[1 + LobeIndex * (2+_Order) + 0] = Math.PI / _FunctionSamplingResolution;				// Dθ
// 				ContextGlobal.m_DerivativesDelta[1 + LobeIndex * (2+_Order) + 1] = 2.0 * Math.PI / (2.0 * _FunctionSamplingResolution);	// DPhi
// 				for ( int CoefficientIndex = 0; CoefficientIndex < _Order; CoefficientIndex++ )
// 					ContextGlobal.m_DerivativesDelta[1 + LobeIndex * (2+_Order) + 2 + CoefficientIndex] = 1e-3;							// Standard deviation of 1e-3 for ZH coefficients
// 			}
// 
// 			// Pre-compute a table of SH coefficients samples
// 			ContextGlobal.m_SHSamples = ContextLocal.m_SHSamples;
// 			ContextGlobal.m_Normalizer = ContextGlobal.m_SHSamples.SamplesCount;
// //			ContextGlobal.m_Normalizer = 4.0 * Math.PI / ContextGlobal.m_SHSamples.SamplesCount;
// //			ContextGlobal.m_Normalizer = 1.0;		// Seems to yield better results with this but it's slow!
// 			ContextGlobal.m_SHEvaluation = ContextLocal.m_SHEvaluation;
// 
// 			// Compute estimate of the goal SH for every sample direction
// 			for ( int SampleIndex=0; SampleIndex < ContextLocal.m_SHSamples.SamplesCount; SampleIndex++ )
// 			{
// 				SHSamplesCollection.SHSample	Sample = ContextGlobal.m_SHSamples.Samples[SampleIndex];
// 				ContextGlobal.m_SHEvaluation[SampleIndex] = EvaluateSH( _SHCoefficients, Sample.m_Theta, Sample.m_Phi, _Order );
// 			}
// 
// 			// Build the concatenaed set of ZH axes & coefficients
// 			double[]	CoefficientsGlobal = new double[1+LobesCount*(2+_Order)];
// 			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
// 			{
// 				// Set axes
// 				CoefficientsGlobal[1+LobeIndex*(2+_Order)+0] = _ZHAxes[LobeIndex].x;
// 				CoefficientsGlobal[1+LobeIndex*(2+_Order)+1] = _ZHAxes[LobeIndex].y;
// 
// 				// Set ZH coefficients
// 				for ( int CoefficientIndex=0; CoefficientIndex < _Order; CoefficientIndex++ )
// 					CoefficientsGlobal[1+LobeIndex*(2+_Order)+2+CoefficientIndex] = _ZHCoefficients[LobeIndex][CoefficientIndex];
// 			}
// 
// 			// Apply BFGS minimzation on the entire set of coefficients
// 			int		IterationsCountGlobal = 0;
// 			double	FunctionMinimumGlobal = double.MaxValue;
// 			try
// 			{
// 				dfpmin( CoefficientsGlobal, _BFGSConvergenceTolerance, out IterationsCountGlobal, out FunctionMinimumGlobal, new BFGSFunctionEval( ZHMappingGlobalFunctionEval ), new BFGSFunctionGradientEval( ZHMappingGlobalFunctionGradientEval ), ContextGlobal );
// 			}
// 			catch ( Exception )
// 			{
// 			}
// 
// 			// Save the optimized results
// 			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
// 			{
// 				// Set axes
// 				_ZHAxes[LobeIndex].x = (float) CoefficientsGlobal[1+LobeIndex*(2+_Order)+0];
// 				_ZHAxes[LobeIndex].y = (float) CoefficientsGlobal[1+LobeIndex*(2+_Order)+1];
// 
// 				// Set ZH coefficients
// 				for ( int CoefficientIndex=0; CoefficientIndex < _Order; CoefficientIndex++ )
// 					_ZHCoefficients[LobeIndex][CoefficientIndex] = CoefficientsGlobal[1+LobeIndex*(2+_Order)+2+CoefficientIndex];
// 			}
// 
// 			// Give final 100% feedback
// 			if ( _Delegate != null )
// 				_Delegate( 1.0f );
// 		}

		#region BRDF Handling

		//////////////////////////////////////////////////////////////////////////
		// This code is a translation from http://people.csail.mit.edu/wojciech/BRDFDatabase/code/BRDFRead.cpp
		//////////////////////////////////////////////////////////////////////////
		//
		const int		BRDF_SAMPLING_RES_THETA_H = 90;
		const int		BRDF_SAMPLING_RES_THETA_D = 90;
		const int		BRDF_SAMPLING_RES_PHI_D = 360;

		const double	BRDF_SCALE_RED = 1.0 / 1500.0;
		const double	BRDF_SCALE_GREEN = 1.15 / 1500.0;
		const double	BRDF_SCALE_BLUE = 1.66 / 1500.0;

		/// <summary>
		/// Given a pair of incoming/outgoing angles, look up the BRDF.
		/// </summary>
		/// <param name="_BRDF">One of the R,G,B BRDFs</param>
		/// <param name="_ThetaIn"></param>
		/// <param name="_PhiIn"></param>
		/// <param name="_ThetaOut"></param>
		/// <param name="_PhiOut"></param>
		/// <param name="_ComponentScale">Should be BRDF_SCALE_RED, BRDF_SCALE_GREEN, BRDF_SCALE_BLUE depending on the component</param>
		/// <returns></returns>
		static double	LookupBRDF( double[] _BRDF, double _ThetaIn, double _PhiIn, double _ThetaOut, double _PhiOut )//, double _ComponentScale )
		{
			// Convert to half angle / difference angle coordinates
			double ThetaHalf, PhiHalf, ThetaDiff, PhiDiff;
			std_coords_to_half_diff_coords(	_ThetaIn, _PhiIn, _ThetaOut, _PhiOut,
											out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );

			// Find index (note that PhiHalf is ignored, since isotropic BRDFs are assumed)
			int TableIndex = PhiDiff_index( PhiDiff );
				TableIndex += (BRDF_SAMPLING_RES_PHI_D / 2) * ThetaDiff_index( ThetaDiff );
				TableIndex += (BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D / 2) * ThetaHalf_index( ThetaHalf );

			double	Result = _BRDF[TableIndex];
//					Result *= _ComponentScale;
			return Result;
		}

		/// <summary>
		/// Convert standard (Theta,Phi) coordinates to half vector & difference vector coordinates
		/// (from http://graphics.stanford.edu/papers/brdf_change_of_variables/brdf_change_of_variables.pdf)
		/// </summary>
		/// <param name="_ThetaIn"></param>
		/// <param name="_PhiIn"></param>
		/// <param name="_ThetaOut"></param>
		/// <param name="_PhiOut"></param>
		/// <param name="_ThetaHalf"></param>
		/// <param name="_PhiHalf"></param>
		/// <param name="_ThetaDiff"></param>
		/// <param name="_PhiDiff"></param>
		// 
		static private Vector3	In = new Vector3();
		static private Vector3	Out = new Vector3();
		static private Vector3	Half = new Vector3();
		static private Vector3	Diff = new Vector3();
		static private Vector3	Tangent = new Vector3() { x=1.0, y=0.0, z=0.0 };
		static private Vector3	BiTangent = new Vector3() { x=0.0, y=1.0, z=0.0 };
		static private Vector3	Normal = new Vector3() { x=0.0, y=0.0, z=1.0 };
		static private Vector3	Temp = new Vector3();
		static void std_coords_to_half_diff_coords( double _ThetaIn, double _PhiIn, double _ThetaOut, double _PhiOut,
													out double _ThetaHalf, out double _PhiHalf, out double _ThetaDiff, out double _PhiDiff )
		{
			// compute in vector
			double in_vec_z = Math.Cos(_ThetaIn);
			double proj_in_vec = Math.Sin(_ThetaIn);
			double in_vec_x = proj_in_vec*Math.Cos(_PhiIn);
			double in_vec_y = proj_in_vec*Math.Sin(_PhiIn);
			In.Set( in_vec_x, in_vec_y, in_vec_z );

			// compute out vector
			double out_vec_z = Math.Cos(_ThetaOut);
			double proj_out_vec = Math.Sin(_ThetaOut);
			double out_vec_x = proj_out_vec*Math.Cos(_PhiOut);
			double out_vec_y = proj_out_vec*Math.Sin(_PhiOut);
			Out.Set( out_vec_x, out_vec_y, out_vec_z );

			// compute halfway vector
			Half.Set( in_vec_x + out_vec_x, in_vec_y + out_vec_y, in_vec_z + out_vec_z );
			Half.Normalize();

			// compute  _ThetaHalf, _PhiHalf
			_ThetaHalf = Math.Acos( Half.z );
			_PhiHalf = Math.Atan2( Half.y, Half.x );

			// Compute diff vector
			In.Rotate( ref Normal, -_PhiHalf, out Temp );
			Temp.Rotate( ref BiTangent, -_ThetaHalf, out Diff );
	
			// Compute _ThetaDiff, _PhiDiff	
			_ThetaDiff = Math.Acos( Diff.z );
			_PhiDiff = Math.Atan2( Diff.y, Diff.x );
		}

		static void	half_diff_coords_to_std_coords( double _ThetaHalf, double _PhiHalf, double _ThetaDiff, double _PhiDiff,
													out double _ThetaIn, out double _PhiIn, out double _ThetaOut, out double _PhiOut )
		{
			double	SinTheta_half = Math.Sin( _ThetaHalf );
			Vector3	Half = new Vector3() { x=Math.Cos( _PhiHalf ) * SinTheta_half, y=Math.Sin( _PhiHalf ) * SinTheta_half, z=Math.Cos( _ThetaHalf ) };

			// Build the 2 vectors representing the frame in which we can use the diff angles
			Vector3	OrthoX;
			Half.Cross( ref Normal, out OrthoX );
			if ( OrthoX.LengthSq() < 1e-6 )
				OrthoX.Set( 1, 0, 0 );
			else
				OrthoX.Normalize();

			Vector3	OrthoY;
			Half.Cross( ref OrthoX, out OrthoY );

			// Rotate using diff angles to retrieve incoming direction
			Half.Rotate( ref OrthoX, -_ThetaDiff, out Temp );
			Temp.Rotate( ref Half, _PhiDiff, out In );

			// We can get the outgoing vector either by rotating the incoming vector half a circle
// 			Vector3	Out2  = new Vector3();
// 			Temp.Rotate( ref Half, _PhiDiff + Math.PI, out Out2 );

			// ...or by mirroring in "Half tangent space"
			double	MirrorX = -In.Dot( ref OrthoX );
			double	MirrorY = -In.Dot( ref OrthoY );
			double	z = In.Dot( ref Half );
			Vector3	Out2 = new Vector3();
			Out2.Set(
				MirrorX*OrthoX.x + MirrorY*OrthoY.x + z*Half.x,
				MirrorX*OrthoX.y + MirrorY*OrthoY.y + z*Half.y,
				MirrorX*OrthoX.z + MirrorY*OrthoY.z + z*Half.z
			);

			// CHECK
// 			Vector3	CheckHalf = new Vector3() { x = In.x+Out.x, y = In.y+Out.y, z = In.z+Out.z };
// 					CheckHalf.Normalize();	// Is this Half ???
			// CHECK

			// Finally, we can retrieve the angles we came here to look for...
			_ThetaIn = Math.Acos( In.z );
			_PhiIn = Math.Atan2( In.y, In.z );
			_ThetaOut = Math.Acos( Out.z );
			_PhiOut = Math.Atan2( Out.y, Out.z );
		}

		// Lookup _ThetaHalf index
		// This is a non-linear mapping!
		// In:  [0 .. pi/2]
		// Out: [0 .. 89]
		static int ThetaHalf_index( double _ThetaHalf )
		{
			if ( _ThetaHalf <= 0.0 )
				return 0;

			double	ThetaHalf_deg = ((_ThetaHalf / (0.5*Math.PI)) * BRDF_SAMPLING_RES_THETA_H);
			double	temp = ThetaHalf_deg*BRDF_SAMPLING_RES_THETA_H;
					temp = Math.Sqrt( temp );

			int Index = (int) Math.Floor( temp );
				Index = Math.Max( 0, Math.Min( Index, BRDF_SAMPLING_RES_THETA_H-1 ) );
			return Index;
		}

		// Lookup _ThetaDiff index
		// In:  [0 .. pi/2]
		// Out: [0 .. 89]
		static int ThetaDiff_index( double _ThetaDiff )
		{
			int Index = (int) Math.Floor( _ThetaDiff / (Math.PI * 0.5) * BRDF_SAMPLING_RES_THETA_D );
				Index = Math.Max( 0, Math.Min( Index, BRDF_SAMPLING_RES_THETA_D-1 ) );
			return Index;
		}

		// Lookup _PhiDiff index
		static int PhiDiff_index( double _PhiDiff )
		{
			// Because of reciprocity, the BRDF is unchanged under
			// _PhiDiff -> _PhiDiff + PI
			if ( _PhiDiff < 0.0 )
				_PhiDiff += Math.PI;

			// In: _PhiDiff in [0 .. PI]
			// Out: tmp in [0 .. 179]
			int	Index = (int) Math.Floor( 2*_PhiDiff / Math.PI * BRDF_SAMPLING_RES_PHI_D );
				Index = Math.Max( 0, Math.Min( Index, BRDF_SAMPLING_RES_PHI_D/2-1 ) );
			return Index;
		}

		/// <summary>
		/// Loads a MERL BRDF file
		/// </summary>
		/// <param name="_BRDFFile"></param>
		/// <returns></returns>
		protected static double[][]	LoadBRDF( FileInfo _BRDFFile )
		{
			double[][]	Result = null;
			try
			{
				using ( FileStream S = _BRDFFile.OpenRead() )
					using ( BinaryReader Reader = new BinaryReader( S ) )
					{
						// Check coefficients count is the expected value
						int	DimX = Reader.ReadInt32();
						int	DimY = Reader.ReadInt32();
						int	DimZ = Reader.ReadInt32();
						int	CoeffsCount = DimX*DimY*DimZ;
						if ( CoeffsCount != BRDF_SAMPLING_RES_THETA_H*BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D/2 )
							throw new Exception( "The amount of coefficients stored in the file is not the expected value (i.e. " + CoeffsCount + "! (is it a BRDF file?)" );

						// Allocate the R,G,B arrays
						Result = new double[3][];
						Result[0] = new double[CoeffsCount];
						Result[1] = new double[CoeffsCount];
						Result[2] = new double[CoeffsCount];

						// Read content
						for ( int ComponentIndex=0; ComponentIndex < 3; ComponentIndex++ )
						{
							double	Factor = 1.0;
							if ( ComponentIndex == 0 )
								Factor = BRDF_SCALE_RED;
							else if ( ComponentIndex == 0 )
								Factor = BRDF_SCALE_GREEN;
							else 
								Factor = BRDF_SCALE_BLUE;

							double[]	ComponentArray = Result[ComponentIndex];
							for ( int CoeffIndex=0; CoeffIndex < CoeffsCount; CoeffIndex++ )
								ComponentArray[CoeffIndex] = Factor * Reader.ReadDouble();
						}
					}
			}
			catch ( Exception _e )
			{	// Forward...
				throw new Exception( "Failed to load source BRDF file: " + _e.Message );
			}

			return Result;
		}

		#endregion

		#region BFGS Minimization

// 		#region Local Minimization Delegates
// 
// 		protected static double	ZHMappingLocalFunctionEval( double[] _Coefficients, object _Params )
// 		{
// 			ZHMappingLocalFunctionEvaluationContext	ContextLocal = _Params as ZHMappingLocalFunctionEvaluationContext;
// 
// 			// Rotate current ZH coefficients
// 			Array.Copy( _Coefficients, 1, ContextLocal.m_ZHCoefficients, 0, ContextLocal.m_Order );
// 			ComputeRotatedZHCoefficients( ContextLocal.m_ZHCoefficients, SphericalToCartesian( ContextLocal.m_LobeDirection.x, ContextLocal.m_LobeDirection.y ), ContextLocal.m_RotatedZHCoefficients );
// 
// 			// Sum differences between current ZH estimates and current SH goal estimates
// 			double	SumSquareDifference = ComputeSummedDifferences( ContextLocal.m_SHSamples, ContextLocal.m_Normalizer, ContextLocal.m_SHEvaluation, ContextLocal.m_RotatedZHCoefficients, ContextLocal.m_Order );
// 
// 			// Keep the result for gradient eval
// 			ContextLocal.m_SumSquareDifference = SumSquareDifference;
// 
// 			return	SumSquareDifference;
// 		}
// 
// 		protected static void	ZHMappingLocalFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params )
// 		{
// 			ZHMappingLocalFunctionEvaluationContext	ContextLocal = _Params as ZHMappingLocalFunctionEvaluationContext;
// 
// 			// Compute derivatives for each coefficient
// 			_Gradients[0] = 0.0;
// 			for ( int DerivativeIndex=1; DerivativeIndex < _Coefficients.Length; DerivativeIndex++ )
// 			{
// 				// Copy coefficients and add them their delta for current derivative
// 				double[]	Coefficients = new double[_Coefficients.Length];
// 				_Coefficients.CopyTo( Coefficients, 0 );
// 				Coefficients[DerivativeIndex] += 1e-3f;
// 
// 				// Rotate ZH coefficients
// 				Array.Copy( Coefficients, 1, ContextLocal.m_ZHCoefficients, 0, ContextLocal.m_Order );
// 				ComputeRotatedZHCoefficients( ContextLocal.m_ZHCoefficients, SphericalToCartesian( ContextLocal.m_LobeDirection.x, ContextLocal.m_LobeDirection.y ), ContextLocal.m_RotatedZHCoefficients );
// 
// 				// Sum differences between current ZH estimates and current SH goal estimates
// 				double	SumSquareDifference = ComputeSummedDifferences( ContextLocal.m_SHSamples, ContextLocal.m_Normalizer, ContextLocal.m_SHEvaluation, ContextLocal.m_RotatedZHCoefficients, ContextLocal.m_Order );
// 
// 				// Compute delta with fixed square difference
// 				_Gradients[DerivativeIndex] = (SumSquareDifference - ContextLocal.m_SumSquareDifference) / 1e-3;
// 			}
// 		}
// 
// 		#endregion
// 
// 		#region Global Minimization Delegates
// 
// 		protected static double	ZHMappingGlobalFunctionEval( double[] _Coefficients, object _Params )
// 		{
// 			ZHMappingGlobalFunctionEvaluationContext	ContextGlobal = _Params as ZHMappingGlobalFunctionEvaluationContext;
// 
// 			// Rotate current ZH coefficients for each lobe
// 			for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
// 				ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] = 0.0;
// 			for ( int LobeIndex=0; LobeIndex < ContextGlobal.m_LobesCount; LobeIndex++ )
// 			{
// 				Array.Copy( _Coefficients, 1 + LobeIndex * (2+ContextGlobal.m_Order) + 2, ContextGlobal.m_ZHCoefficients, 0, ContextGlobal.m_Order );
// 				ComputeRotatedZHCoefficients( ContextGlobal.m_ZHCoefficients, SphericalToCartesian( _Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 0], _Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 1] ), ContextGlobal.m_RotatedZHCoefficients );
// 
// 				// Accumulate rotated ZH coefficients
// 				for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
// 					ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] += ContextGlobal.m_RotatedZHCoefficients[CoefficientIndex];
// 			}
// 
// 			// Sum differences between current rotated ZH estimate and SH goal
// 			double	SumSquareDifference = ComputeSummedDifferences( ContextGlobal.m_SHSamples, ContextGlobal.m_Normalizer, ContextGlobal.m_SHEvaluation, ContextGlobal.m_SumRotatedZHCoefficients, ContextGlobal.m_Order );
// 
// 			// Keep the result for gradient eval
// 			ContextGlobal.m_SumSquareDifference = SumSquareDifference;
// 
// 			return	SumSquareDifference;
// 		}
// 
// 		protected static void	ZHMappingGlobalFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params )
// 		{
// 			ZHMappingGlobalFunctionEvaluationContext	ContextGlobal = _Params as ZHMappingGlobalFunctionEvaluationContext;
// 
// 			// Compute derivatives for each coefficient
// 			_Gradients[0] = 0.0;
// 			for ( int DerivativeIndex=1; DerivativeIndex < _Coefficients.Length; DerivativeIndex++ )
// 			{
// 				// Copy coefficients and add them their delta
// 				double[]	Coefficients = new double[_Coefficients.Length];
// 				_Coefficients.CopyTo( Coefficients, 0 );
// 				Coefficients[DerivativeIndex] += ContextGlobal.m_DerivativesDelta[DerivativeIndex];
// 
// 				// Rotate current ZH coefficients for each lobe
// 				for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
// 					ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] = 0.0;
// 				for ( int LobeIndex=0; LobeIndex < ContextGlobal.m_LobesCount; LobeIndex++ )
// 				{
// 					Array.Copy( Coefficients, 1 + LobeIndex * (2+ContextGlobal.m_Order) + 2, ContextGlobal.m_ZHCoefficients, 0, ContextGlobal.m_Order );
// 					ComputeRotatedZHCoefficients( ContextGlobal.m_ZHCoefficients, SphericalToCartesian( Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 0], Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 1] ), ContextGlobal.m_RotatedZHCoefficients );
// 
// 					// Accumulate rotated ZH coefficients
// 					for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
// 						ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] += ContextGlobal.m_RotatedZHCoefficients[CoefficientIndex];
// 				}
// 
// 				// Sum differences between current ZH estimates and current SH goal estimates
// 				double	SumSquareDifference = ComputeSummedDifferences( ContextGlobal.m_SHSamples, ContextGlobal.m_Normalizer, ContextGlobal.m_SHEvaluation, ContextGlobal.m_SumRotatedZHCoefficients, ContextGlobal.m_Order );
// 
// 				// Compute difference with fixed square difference
// 				_Gradients[DerivativeIndex] = (SumSquareDifference - ContextGlobal.m_SumSquareDifference) / ContextGlobal.m_DerivativesDelta[DerivativeIndex];
// 			}
// 		}
// 
// 		#endregion

		/// <summary>
		/// Computes the square difference between a current SH estimate and a goal SH function given a set of samples
		/// </summary>
		/// <param name="_SamplesCollection">The collection of samples to use for the computation</param>
		/// <param name="_Normalizer">The normalizer for the final result</param>
		/// <param name="_GoalSHEvaluation">The goal SH function's evaluations for each sample</param>
		/// <param name="_SHEstimate">The estimate SH function's coefficients</param>
		/// <param name="_Order">The SH order</param>
		/// <returns>The square difference between goal and estimate</returns>
// 		protected static double		ComputeSummedDifferences( SHSamplesCollection _SamplesCollection, double _Normalizer, double[] _GoalSHEvaluation, double[] _SHEstimate, int _Order )
// 		{
// 			// Sum differences between current ZH estimates and current SH goal estimates
// 			double	SumSquareDifference = 0.0;
// 			for ( int SampleIndex=0; SampleIndex < _SamplesCollection.SamplesCount; SampleIndex++ )
// 			{
// 				SHSamplesCollection.SHSample	Sample = _SamplesCollection.Samples[SampleIndex];
// 
// 				double		GoalValue = _GoalSHEvaluation[SampleIndex];
// 
// 				// Estimate ZH
// 				double		CurrentValue = 0.0;
// 				for ( int CoefficientIndex=0; CoefficientIndex < _Order * _Order; CoefficientIndex++ )
// 					CurrentValue += _SHEstimate[CoefficientIndex] * Sample.m_SHFactors[CoefficientIndex];
// 
// 				// Sum difference between estimate and goal
// 				SumSquareDifference += (CurrentValue - GoalValue) * (CurrentValue - GoalValue);
// 			}
// 
// 			// Normalize
// 			SumSquareDifference *= _Normalizer;
// 
// 			return	SumSquareDifference;
// 		}

		#region BFGS Algorithm

		protected static readonly int		ITMAX = 200;
		protected static readonly double	EPS = 3.0e-8;
		protected static readonly double	TOLX = 4*EPS;
		protected static readonly double	STPMX = 100.0;			// Scaled maximum step length allowed in line searches.

		/// <summary>
		/// Performs BFGS function minimzation on a quadratic form function evaluated by the provided delegate
		/// </summary>
		/// <param name="_Coefficients">The array of initial coefficients (indexed from 1!!) that will also contain the resulting coefficients when the routine has converged</param>
		/// <param name="_ConvergenceTolerance">The tolerance error to accept as the minimum of the function</param>
		/// <param name="_PerformedIterationsCount">The amount of iterations performed to reach the minimum</param>
		/// <param name="_Minimum">The found minimum</param>
		/// <param name="_FunctionEval">The delegate used to evaluate the function to minimize</param>
		/// <param name="_FunctionGradientEval">The delegate used to evaluate the gradient of the function to minimize</param>
		/// <param name="_Params">Some user params passed to the evaluation functions</param>
		protected static void	dfpmin( double[] _Coefficients, double _ConvergenceTolerance, out int _PerformedIterationsCount, out double _Minimum, BFGSFunctionEval _FunctionEval, BFGSFunctionGradientEval _FunctionGradientEval, object _Params )
		{
			int			n = _Coefficients.Length - 1;

			int			check,i,its,j;
			double		den,fac,fad,fae,fp,stpmax,sum=0.0,sumdg,sumxi,temp,test;

			double[]	dg = new double[1+n];
			double[]	g = new double[1+n];
			double[]	hdg = new double[1+n];
			double[][]	hessin = new double[1+n][];
			for ( i=1; i <= n; i++ )
				hessin[i] = new double[1+n];
			double[]	pnew = new double[1+n];
			double[]	xi = new double[1+n];

			// Initialize values
			fp = _FunctionEval( _Coefficients, _Params );
			_FunctionGradientEval( _Coefficients, g, _Params );

			for ( i=1; i <= n; i++ )
			{
				for ( j=1; j <= n; j++ )
					hessin[i][j]=0.0;

				hessin[i][i] = 1.0;

				xi[i] = -g[i];
				sum += _Coefficients[i]*_Coefficients[i];
			}

			stpmax = STPMX * Math.Max( Math.Sqrt( sum ), n );
			for ( its=1; its <= ITMAX; its++ )
			{
				_PerformedIterationsCount = its;

				// The new function evaluation occurs in lnsrch
				lnsrch( n, _Coefficients, fp, g, xi, pnew, out _Minimum, stpmax, out check, _FunctionEval, _Params );
				fp = _Minimum;

				for ( i=1; i<=n; i++ )
				{
					xi[i] = pnew[i] - _Coefficients[i];	// Update the line direction
					_Coefficients[i] = pnew[i];			// as well as the current point
				}

				// Test for convergence on Delta X
				test = 0.0;
				for ( i=1; i <= n; i++ )
				{
					temp = Math.Abs( xi[i] ) / Math.Max( Math.Abs( _Coefficients[i] ), 1.0 );
					if ( temp > test )
						test = temp;
				}

				if ( test < TOLX )
					return;	// Done!

				// Save the old gradient
				for ( i=1; i <= n; i++ )
					dg[i] = g[i];

				// Get the new one
				_FunctionGradientEval( _Coefficients, g, _Params );

				// Test for convergence on zero gradient
				test = 0.0;
				den = Math.Max( _Minimum, 1.0 );
				for ( i=1; i <= n; i++ )
				{
					temp = Math.Abs( g[i] ) * Math.Max( Math.Abs( _Coefficients[i] ), 1.0 ) / den;
					if ( temp > test )
						test = temp;
				}

				if ( test < _ConvergenceTolerance )
					return;	// Done!

				// Compute difference of gradients
				for ( i=1; i <= n ; i++ )
					dg[i] = g[i]-dg[i];

				// ...and difference times current hessian matrix
				for ( i=1; i <= n; i++ )
				{
					hdg[i]=0.0;
					for ( j=1; j <= n; j++ )
						hdg[i] += hessin[i][j] * dg[j];
				}

				// Calculate dot products for the denominators
				fac = fae = sumdg = sumxi = 0.0;
				for ( i=1; i <= n; i++ )
				{
					fac += dg[i] * xi[i];
					fae += dg[i] * hdg[i];
					sumdg += dg[i] * dg[i];
					sumxi += xi[i] * xi[i];
				}

				if ( fac * fac > EPS * sumdg * sumxi )
				{
					fac = 1.0 / fac;
					fad = 1.0 / fae;

					// The vector that makes BFGS different from DFP
					for ( i=1; i <= n; i++ )
						dg[i] = fac * xi[i] - fad * hdg[i];

					// BFGS Hessian update formula
					for ( i=1; i <= n; i++ )
						for ( j=1; j <= n; j++ )
							hessin[i][j] += fac * xi[i] * xi[j] -fad * hdg[i] * hdg[j] + fae * dg[i] * dg[j];
				}

				// Now, calculate the next direction to go
				for ( i=1; i <= n; i++ )
				{
					xi[i] = 0.0;
					for ( j=1; j <= n; j++ )
						xi[i] -= hessin[i][j] * g[j];
				}
			}

			throw new Exception( "Too many iterations in dfpmin" );
		}

		protected static readonly double	ALF = 1.0e-4;
		protected static readonly double	TOLY = 1.0e-7;

		protected static void	lnsrch( int n, double[] xold, double fold, double[] g, double[] p, double[] x, out double f, double stpmax, out int check, BFGSFunctionEval _FunctionEval, object _Params )
		{
			int i;
			double a,alam,alam2 = 0.0,alamin,b,disc,f2 = 0.0,fold2 = 0.0,rhs1,rhs2,slope,sum,temp,test,tmplam;

			check=0;
			for ( sum=0.0,i=1; i <= n; i++ )
				sum += p[i]*p[i];
			sum = Math.Sqrt( sum );

			if ( sum > stpmax )
				for ( i=1; i <= n; i++ )
					p[i] *= stpmax / sum;

			for ( slope=0.0,i=1; i <= n; i++ )
				slope += g[i] * p[i];

			test = 0.0;
			for ( i=1; i <= n; i++ )
			{
				temp = Math.Abs( p[i] ) / Math.Max( Math.Abs( xold[i] ), 1.0 );
				if ( temp > test )
					test = temp;
			}

			alamin = TOLY / test;
			alam = 1.0;
			for (;;)
			{
				for ( i=1; i <= n; i++ )
					x[i] = xold[i] + alam * p[i];

				f = _FunctionEval( x, _Params );
				if ( alam < alamin )
				{
					for ( i=1; i <= n; i++ )
						x[i] = xold[i];

					check = 1;
					return;
				}
				else if ( f <= fold + ALF * alam * slope )
					return;
				else
				{
					if ( alam == 1.0 )
						tmplam = -slope / (2.0 * (f - fold-slope));
					else
					{
						rhs1 = f-fold-alam*slope;
						rhs2 = f2-fold2-alam2*slope;
						a=(rhs1/(alam*alam)-rhs2/(alam2*alam2))/(alam-alam2);
						b=(-alam2*rhs1/(alam*alam)+alam*rhs2/(alam2*alam2))/(alam-alam2);
						if (a == 0.0) tmplam = -slope/(2.0*b);
						else
						{
							disc = b*b - 3.0 * a * slope;
							if ( disc < 0.0 )
								throw new Exception( "Roundoff problem in lnsrch." );
							else
								tmplam = (-b + Math.Sqrt( disc ) ) / (3.0 * a);
						}
						if ( tmplam > 0.5 * alam )
							tmplam = 0.5 * alam;
					}
				}
				alam2=alam;
				f2 = f;
				fold2=fold;
				alam = Math.Max( tmplam, 0.1*alam );
			}
		}

		#endregion

		#endregion
	}
}
