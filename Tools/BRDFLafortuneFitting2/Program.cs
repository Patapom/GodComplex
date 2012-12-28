//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// Difference with first fitting project is that this one is using Levenberg-Marquardt optimization
//////////////////////////////////////////////////////////////////////////
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
// This is the goal of this little application that will attempt to fit complex 4D BRDFs from the MERL database (each file is 33MB!) into several cosine lobes.
// After failing at using BFGS, I'm now using Levenberg-Marquardt as advised by Lafortune.
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

		const int		SAMPLES_COUNT_THETA = 90;				// Use 90 to cover the entire BRDF (1 458 000 samples).
		const double	BFGS_CONVERGENCE_TOLERANCE = 1e-4;		// Don't exit unless we reach below this threshold...
		const double	DERIVATIVE_OFFSET = 5e-3;

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

		[System.Diagnostics.DebuggerDisplay( "Axis=({C.x}, {C.y}, {C.z}) N={N}" )]
		struct	CosineLobe
		{
			public Vector3	C;		// Cx, Cy, Cz coefficients of the generalized cosine lobe model
			public double	N;		// Exponent

			// DEBUG => Keep track of the error with this lobe
			public double	Error;

			public	CosineLobe( Vector3 _C, double _N )
			{
				C = _C;
				N = _N;
				Error = double.MaxValue;
			}

			public void		CopyFrom( CosineLobe _Source )
			{
				C.x = _Source.C.x;
				C.y = _Source.C.y;
				C.z = _Source.C.z;
				N = _Source.N;
				Error = _Source.Error;
			}
		}

		class	BRDFSample
		{
			public int			m_BRDFIndex = 0;				// The flattened index in the BRDF table
			public double		m_CosThetaIn = 0.0;			// Optional divide by cos theta to alleviate deviations at grazing angles
			public Vector3		m_DotProduct = new Vector3();	// The 3 coefficients of the dot product between the incoming/outgoing directions corresponding to the BRDF index
		}

		class	BRDFFitEvaluationContext
		{
			public double[]		m_BRDF = null;					// The temporary BRDF coefficients
			public CosineLobe[]	m_Lobes = null;					// The current cosine lobe coefficients
			public double		m_SumSquareDifference = 0;
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

		/// <summary>
		/// The delegate to use to receive feedback for the mapping method "MapSHIntoZH()"
		/// </summary>
		/// <param name="_Progress">A value in [0,1] indicating mapping progress</param>
		protected delegate void		BRDFMappingFeedback( double _Progress );

		#endregion

		#region FIELDS

		static BRDFSample[]			ms_BRDFSamples = null;
		static LevenbergMarquardt	ms_Solver = new LevenbergMarquardt();

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

				// Attempt to create the target file first, just to ensure we don't bump into a problem after the lengthy minimization process...
				try
				{
					using ( TargetFile.Create() ) {}
				}
				catch ( Exception _e )
				{
					throw new Exception( "Failed to create the target coefficients file: " + _e.Message );
				}

				// Load the BRDF
				double[][]	BRDF = LoadBRDF( SourceBRDF );

// 				// DEBUG CHECK
// 				{	// Generate a bunch of incoming/outgoing directions, get their Half/Diff angles then regenerate back incoming/outgoing directions from these angles and check the relative incoming/outgoing directions are conserved
// 					// This is important to ensure we sample only the relevant (i.e. changing) parts of the BRDF in our minimization scheme
// 					// (I want to actually sample the BRDF using the half/diff angles and generate incoming/outgoing vectors from these, rather than sample all the possible 4D space)
// 					//
// 					Random	TempRNG = new Random( 1 );
// 					Vector3	TempIn = new Vector3(), TempOut = new Vector3();
// 					double	MinThetaHalf = double.MaxValue, MaxThetaHalf = -double.MaxValue;
// 					double	MinPhiHalf = double.MaxValue, MaxPhiHalf = -double.MaxValue;
// 					double	MinThetaDiff = double.MaxValue, MaxThetaDiff = -double.MaxValue;
// 					double	MinPhiDiff = double.MaxValue, MaxPhiDiff = -double.MaxValue;
// 					for ( int i=0; i < 10000; i++ )
// 					{
// 						double	Phi_i = 2.0 * Math.PI * (TempRNG.NextDouble() - 0.5);
// 						double	Theta_i = 0.5 * Math.PI * TempRNG.NextDouble();
// 						double	Phi_r = 2.0 * Math.PI * (TempRNG.NextDouble() - 0.5);
// 						double	Theta_r = 0.5 * Math.PI * TempRNG.NextDouble();
// 
// 						double	Theta_half, Phi_half, Theta_diff, Phi_diff;
// 						std_coords_to_half_diff_coords( Theta_i, Phi_i, Theta_r, Phi_r, out Theta_half, out Phi_half, out Theta_diff, out Phi_diff );
// 
// 
// // 						MinThetaHalf = Math.Min( MinThetaHalf, Theta_half );
// // 						MaxThetaHalf = Math.Max( MaxThetaHalf, Theta_half );
// // 						MinThetaDiff = Math.Min( MinThetaDiff, Theta_diff );
// // 						MaxThetaDiff = Math.Max( MaxThetaDiff, Theta_diff );
// // 						MinPhiHalf = Math.Min( MinPhiHalf, Phi_half );
// // 						MaxPhiHalf = Math.Max( MaxPhiHalf, Phi_half );
// // 						MinPhiDiff = Math.Min( MinPhiDiff, Phi_diff );
// // 						MaxPhiDiff = Math.Max( MaxPhiDiff, Phi_diff );
// 
// // 						if ( Theta_half > MaxThetaHalf )
// // 						{
// // 							MaxThetaHalf = Theta_half;
// // 							MaxPhiHalf = Phi_half;
// // 							MaxThetaDiff = Theta_diff;
// // 							MaxPhiDiff = Phi_diff;
// // 						}
// 
// 						// Convert back...
// 						double	NewTheta_i, NewPhi_i, NewTheta_r, NewPhi_r;
// 						half_diff_coords_to_std_coords( Theta_half, Phi_half, Theta_diff, Phi_diff, out NewTheta_i, out NewPhi_i, out NewTheta_r, out NewPhi_r );
// 
// 						// Convert back into directions
// 						half_diff_coords_to_std_coords( Theta_half, Phi_half, Theta_diff, Phi_diff, ref TempIn, ref TempOut );
// 
// 						// Check
// 						const double Tol = 1e-4;
// 						if ( Math.Abs( NewTheta_i - Theta_i ) > Tol
// 							|| Math.Abs( NewTheta_r - Theta_r ) > Tol )
// 							throw new Exception( "ARGH THETA!" );
// 						if ( Math.Abs( NewPhi_i - Phi_i ) > Tol
// 							|| Math.Abs( NewPhi_r - Phi_r ) > Tol )
// 							throw new Exception( "ARGH PHI!" );
// 
// 						if ( NewTheta_i > 0.5 * Math.PI )
// 							throw new Exception( "Incoming direction below surface!" );
// 						if ( NewTheta_r > 0.5 * Math.PI )
// 							throw new Exception( "Outgoing direction below surface!" );
// 						if ( TempIn.z < 0.0 )
// 							throw new Exception( "VECTOR Incoming direction below surface!" );
// 						if ( TempOut.z < 0.0 )
// 							throw new Exception( "VECTOR Outgoing direction below surface!" );
// 					}
// 				}
// 				// DEBUG CHECK

// DEBUG CHECK
// I can't understand the purpose of certain "wrong angles" in the BRDF table
// When Theta_half is near PI/2, and difference angles make the incoming or outgoing directions go BELOW the fucking surface, what does it mean????
// {
// 	double	ThetaHalf = 0.5*Math.PI;
// 	double	ThetaDiff = 0.1*Math.PI;	// This should make the incoming direction go BELOW the surface...
// 	double	PhiDiff = 0.0;
// 
// 	// Check it's below the surface
// 	Vector3	TempIn = new Vector3(), TempOut = new Vector3();
// 	half_diff_coords_to_std_coords( ThetaHalf, 0, ThetaDiff, PhiDiff, ref TempIn, ref TempOut );
// 
// 	// Get BRDF index
// 	int	TableIndex = PhiDiff_index( PhiDiff );
// 		TableIndex += (BRDF_SAMPLING_RES_PHI_D / 2) * ThetaDiff_index( ThetaDiff );
// 		TableIndex += (BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D / 2) * ThetaHalf_index( ThetaHalf );
// 
// 	// What the fuck is there??
// 	double	Value = BRDF[0][TableIndex];
// 
// 	// A tiny negative value...
// }
// DEBUG CHECK



// DEBUG Clear out BRDF values to make sure we only read positive samples. If we find a negative sample then there is an error!
// for ( int PhiDiffIndex=0; PhiDiffIndex < BRDF_SAMPLING_RES_PHI_D/2; PhiDiffIndex++ )
// 	for ( int ThetaDiffIndex=0; ThetaDiffIndex < BRDF_SAMPLING_RES_THETA_D; ThetaDiffIndex++ )
// 		for ( int ThetaHalfIndex=0; ThetaHalfIndex < BRDF_SAMPLING_RES_THETA_H; ThetaHalfIndex++ )
// 		{
// 			int	TableIndex = PhiDiffIndex;
// 				TableIndex += (BRDF_SAMPLING_RES_PHI_D / 2) * ThetaDiffIndex;
// 				TableIndex += (BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D / 2) * ThetaHalfIndex;
// 
// 			BRDF[0][TableIndex] = -1.0f;
// 		}
// DEBUG


				// Generate the sampling base
				// => We generate and store as many samples as possible in the 90*90*360/2 source array
				// => We generate the associated incoming/outgoing direction
				// => We store the 3 dot product coefficients for the sample, which we will use to evaluate the cosine lobe
				//
				Random	RNG = new Random( 1 );
				double	dPhi = Math.PI / (2*SAMPLES_COUNT_THETA);
				double	dTheta = 0.5*Math.PI / SAMPLES_COUNT_THETA;

				Vector3	MinValues = new Vector3() { x=+double.MaxValue, y=+double.MaxValue, z=+double.MaxValue };
				Vector3	MaxValues = new Vector3() { x=-double.MaxValue, y=-double.MaxValue, z=-double.MaxValue };

				Vector3	In = new Vector3();
				Vector3	Out = new Vector3();
				List<BRDFSample>	Samples = new List<BRDFSample>();
				for ( int PhiDiffIndex=0; PhiDiffIndex < 2*SAMPLES_COUNT_THETA; PhiDiffIndex++ )
				{
					for ( int ThetaDiffIndex=0; ThetaDiffIndex < SAMPLES_COUNT_THETA; ThetaDiffIndex++ )
					{
						for ( int ThetaHalfIndex=0; ThetaHalfIndex < SAMPLES_COUNT_THETA; ThetaHalfIndex++ )
						{
							// Generate random stratified samples
							double	PhiDiff = dPhi * (PhiDiffIndex + RNG.NextDouble());
							double	ThetaDiff = dTheta * (ThetaDiffIndex + RNG.NextDouble());
							double	ThetaHalf = dTheta * (ThetaHalfIndex + RNG.NextDouble());
// 							double	PhiDiff = dPhi * PhiDiffIndex;
// 							double	ThetaDiff = dTheta * ThetaDiffIndex;
// 							double	ThetaHalf = dTheta * ThetaHalfIndex;

							// Retrieve incoming/outgoing vectors
							half_diff_coords_to_std_coords( ThetaHalf, 0.0, ThetaDiff, PhiDiff, ref In, ref Out );

							// Build the general BRDF index
							int	TableIndex = PhiDiff_index( PhiDiff );
								TableIndex += (BRDF_SAMPLING_RES_PHI_D / 2) * ThetaDiff_index( ThetaDiff );
								TableIndex += (BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D / 2) * ThetaHalf_index( ThetaHalf );

							// Check the in & out directions are valid (i.e. ABOVE the surface)
							const double	Z_TOL = 0.001;
							if ( In.z <= Z_TOL || Out.z <= Z_TOL )
								continue;	// Invalid sample...

							//////////////////////////////////////////////////////////////////////////
							// Replace any invalid value
							double	R = BRDF[0][TableIndex];
							double	G = BRDF[1][TableIndex];
							double	B = BRDF[2][TableIndex];

							double	SumValidValues = 0.0;
							int		ValidValuesCount = 0;
							if ( IsValid( R ) )
							{	// Red is valid
								SumValidValues += R;
								ValidValuesCount++;
							}
							if ( IsValid( G ) )
							{	// Green is valid
								SumValidValues += G;
								ValidValuesCount++;
							}
							if ( IsValid( B ) )
							{	// Blue is valid
								SumValidValues += B;
								ValidValuesCount++;
							}
							if ( ValidValuesCount != 3 )
							{
								SumValidValues /= Math.Max( 1, ValidValuesCount );	// Get the average of valid values
								if ( !IsValid( R ) )
									R = SumValidValues;	// Replace Red by average...
								if ( !IsValid( G ) )
									G = SumValidValues;	// Replace Green by average...
								if ( !IsValid( B ) )
									B = SumValidValues;	// Replace Blue by average...

								BRDF[0][TableIndex] = R;
								BRDF[1][TableIndex] = G;
								BRDF[2][TableIndex] = B;
							}


							//////////////////////////////////////////////////////////////////////////
							BRDFSample	Sample = new BRDFSample();
							Samples.Add( Sample );

							Sample.m_BRDFIndex = TableIndex;

							// Store the cos(ThetaIn)
							Sample.m_CosThetaIn = In.z;

							// Build the dot product coefficients used by the Lafortune model
							Sample.m_DotProduct.Set(
								In.x*Out.x,
								In.y*Out.y,
								In.z*Out.z
								);


// DEBUG Keep min/max values to have an idea of what we're manipulating here...
MinValues.x = Math.Min( MinValues.x, R );
MaxValues.x = Math.Max( MaxValues.x, R );
MinValues.y = Math.Min( MinValues.y, G );
MaxValues.y = Math.Max( MaxValues.y, G );
MinValues.z = Math.Min( MinValues.z, B );
MaxValues.z = Math.Max( MaxValues.z, B );
// DEBUG

// DEBUG
// Patch the BRDF to make it look like an obvious cosine lobe from a standard Phong reflection
// {
// 	Vector3	Reflect = new Vector3() { x=-In.x, y=-In.y, z=In.z };
// 	double	Dot = Reflect.Dot( ref Out );
// 
// // 	Vector3	Half = new Vector3() { x=In.x+Out.x, y=In.y+Out.y, z=In.z+Out.z };
// // 			Half.Normalize();
// // 	double	Dot = Half.z;
// 			Dot = Math.Max( 0, Dot );
// 			Dot = Math.Pow( Dot, 17.2 );	// The exponent is very particular
// //			Dot /= In.z;					// / cos(ThetaIn)
// 
// 	BRDF[0][TableIndex] = Dot;
// }
// DEBUG
						}
					}
				}

				// We got our samples!
				ms_BRDFSamples = Samples.ToArray();

				double	PercentageOfTableUsed = (double) ms_BRDFSamples.Length / (BRDF_SAMPLING_RES_THETA_H*BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D / 2);


				// Show modeless progress form
				ms_ProgressForm = new ProgressForm();
				ms_ProgressForm.Show();
				ms_ProgressForm.BRDFComponentIndex = 0;
				ms_ProgressForm.Progress = 0.0;


				//////////////////////////////////////////////////////////////////////////
				// Setup the solver
				{
					ms_Solver.Init( ms_BRDFSamples.Length, 4*LobesCount, 45 );

					ms_Solver.AbsoluteTolerance = 0.005;
					ms_Solver.RelativeTolerance = 0.01;
     
					for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
					{
						ms_Solver.VectorConstraints.SetConstraint( 4*LobeIndex+0, -2.0, +2.0 );	// Cx
						ms_Solver.VectorConstraints.SetConstraint( 4*LobeIndex+1, -2.0, +2.0 );	// Cy
						ms_Solver.VectorConstraints.SetConstraint( 4*LobeIndex+2,  0.0, +1.5 );	// Cz
						ms_Solver.VectorConstraints.SetConstraint( 4*LobeIndex+3,  1.0, +128 );	// Exponent
					}
				}


				// Build a list of initial guesses
				CosineLobe[]	InitialGuesses = new CosineLobe[10];
				InitialGuesses[0] = new CosineLobe(
					new Vector3() { x=-1, y=-1, z=1 },	// Standard Phong reflection
// 					new Vector3() { x=0, y=0, z=1 },	// Dumb test
// 					17.2 );
 					1.0 );

				for ( int i=1; i < InitialGuesses.Length; i++ )
				{
					Vector3	Direction = new Vector3() { x=2.0*RNG.NextDouble()-1.0, y=2.0*RNG.NextDouble()-1.0, z=RNG.NextDouble() };
					InitialGuesses[i] = new CosineLobe( Direction, 1.0 );
				}

				// Perform local minimization for each R,G,B component
				CosineLobe[][]	CosineLobes = new CosineLobe[3][];
				double[][]		RMSErrors = new double[3][];
				try
				{
					for ( int ComponentIndex=0; ComponentIndex < 3; ComponentIndex++ )
					{
						CosineLobes[ComponentIndex] = new CosineLobe[LobesCount];
						RMSErrors[ComponentIndex] = new double[LobesCount];

						ms_ProgressForm.BRDFComponentIndex = ComponentIndex;

						FitBRDF( BRDF[ComponentIndex], CosineLobes[ComponentIndex], InitialGuesses, BFGS_CONVERGENCE_TOLERANCE, RMSErrors[ComponentIndex], ShowProgress );

// 						// CHECK We get the same result starting from another initial guess
// 						CosineLobe[]	TempLobes = new CosineLobe[LobesCount]; 
// 						InitialGuesses[0] = new CosineLobe( new Vector3() { x=0, y=0, z=1 }, 1 );
// 						FitBRDF( BRDF[ComponentIndex], TempLobes, InitialGuesses, BFGS_CONVERGENCE_TOLERANCE, RMSErrors[ComponentIndex], ShowProgress );
// 						// CHECK
					}
				}
				catch ( Exception _e )
				{
					throw new Exception( "BRDF Fitting failed: " + _e.Message );
				}

				ms_ProgressForm.Dispose();

				// Save the result
				try
				{
					using ( FileStream Stream = TargetFile.Create() )
						using ( BinaryWriter Writer = new BinaryWriter( Stream ) )
						{
							// Write the amount of lobes
							Writer.Write( LobesCount );

							// Write the coefficients
							for ( int ComponentIndex=0; ComponentIndex < 3; ComponentIndex++ )
							{
								for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
								{
									CosineLobe	Lobe = CosineLobes[ComponentIndex][LobeIndex];
									Writer.Write( Lobe.C.x );
									Writer.Write( Lobe.C.y );
									Writer.Write( Lobe.C.z );
									Writer.Write( Lobe.N );
								}
							}
						}
				}
				catch ( Exception _e )
				{
					throw new Exception( "Error writing the result cosine lobe coefficients: " + _e.Message );
				}
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "An error occurred!\r\n\r\n" + _e.Message + "\r\n\r\n" + _e.StackTrace, "BRDF Fitting", MessageBoxButtons.OK, MessageBoxIcon.Warning );
			}
		}

		private static bool		IsValid( double _BRDFValue )
		{
			if ( _BRDFValue < 0.0 )
				return false;
			if ( _BRDFValue > 10000.0 )
				return false;

			if ( double.IsNaN( _BRDFValue ) )
				return false;

			return true;
		}

		private static ProgressForm	ms_ProgressForm = null;
		private static void		ShowProgress( double _Progress )
		{
			ms_ProgressForm.Progress = _Progress;
		}

		/// <summary>
		/// Performs mapping of a BRDF into N sets of cosine lobes coefficients
		/// WARNING: Takes hell of a time to compute !
		/// </summary>
		/// <param name="_BRDF">The BRDF to fit into cosine lobes</param>
		/// <param name="_Lobes">The array of resulting cosine lobes</param>
		/// <param name="_InitialGuesses">An array of initial lobe coefficients</param>
		/// <param name="_BFGSConvergenceTolerance">The convergence tolerance for the BFGS algorithm (the lower the tolerance, the longer it will compute)</param>
		/// <param name="_RMS">The resulting array of RMS errors for each cosine lobe</param>
		/// <param name="_Delegate">An optional delegate to pass the method to get feedback about the mapping as it can be a lengthy process (!!)</param>
		/// <returns>The global minimum for all the cosine lobes put together</returns>
		private static double		FitBRDF( double[] _BRDF, CosineLobe[] _Lobes, CosineLobe[] _InitialGuesses, double _BFGSConvergenceTolerance, double[] _RMS, BRDFMappingFeedback _Delegate )
		{
			int			LobesCount = _Lobes.GetLength( 0 );

			// Build the local function evaluation context
			BRDFFitEvaluationContext	Context = new BRDFFitEvaluationContext();
			Context.m_Lobes = new CosineLobe[1] { new CosineLobe() };
			Context.m_BRDF = new double[_BRDF.Length];
			_BRDF.CopyTo( Context.m_BRDF, 0 );	// Duplicate BRDF as we will modify it for each new lobe

			// Prepare feedback data
			float	fCurrentProgress = 0.0f;
			float	fProgressDelta = 1.0f / (LobesCount * _InitialGuesses.Length);
			int		FeedbackCount = 0;
			int		FeedbackThreshold = (LobesCount * _InitialGuesses.Length) / 100;	// Notify every percent

			//////////////////////////////////////////////////////////////////////////
			// 1] Compute the best fit for each lobe
			int			CrashesCount = 0;
			double[]	LocalLobeCoefficients = new double[1+4];	// Don't forget the BFGS function annoyingly uses indices starting from 1!
			List<CosineLobe>	BestFits = new List<CosineLobe>( _InitialGuesses.Length );
			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
			{
				BestFits.Clear();

				// 1.1] Perform minification using several attempts with different initial coefficients and keep the best fit
				double	MinError = double.MaxValue;
				for ( int AttemptIndex = 0; AttemptIndex < _InitialGuesses.Length; AttemptIndex++ )
				{
					// Update external feedback on progression
					if ( _Delegate != null )
					{
						fCurrentProgress += fProgressDelta;
						FeedbackCount++;
						if ( FeedbackCount > FeedbackThreshold )
						{	// Send feedback
							FeedbackCount = 0;
							_Delegate( fCurrentProgress );
						}
					}

					// 1.1.1] Set the initial lobe coefficients
					Context.m_Lobes[0].CopyFrom( _InitialGuesses[AttemptIndex] );

					// 1.1.2] Copy coefficients into working array
					LocalLobeCoefficients[1+0] = Context.m_Lobes[0].C.x;
					LocalLobeCoefficients[1+1] = Context.m_Lobes[0].C.y;
					LocalLobeCoefficients[1+2] = Context.m_Lobes[0].C.z;
					LocalLobeCoefficients[1+3] = Context.m_Lobes[0].N;

					//////////////////////////////////////////////////////////////////////////
					// At this point, we have a fixed direction and the best estimated ZH coefficients to map the provided SH in this direction.
					//
					// We then need to apply BFGS minimization to optimize the ZH coefficients yielding the smallest possible error...
					//

					// 1.1.3] Apply BFGS minimization
					int		IterationsCount = 0;
					double	FunctionMinimum = 0;
					try
					{
						FunctionMinimum = dfpmin( LocalLobeCoefficients, _BFGSConvergenceTolerance, out IterationsCount, new BFGSFunctionEval( BRDFMappingLocalFunctionEval ), new BFGSFunctionGradientEval( BRDFMappingLocalFunctionGradientEval ), Context );
					}
					catch ( Exception )
					{
						CrashesCount++;
						continue;
					}

					if ( FunctionMinimum >= MinError )
						continue;	// Larger error than best candidate so far...

					MinError = FunctionMinimum;

					// Save that "optimal" lobe data
					_Lobes[LobeIndex].C.Set( Context.m_Lobes[0].C.x, Context.m_Lobes[0].C.y, Context.m_Lobes[0].C.z );
					_Lobes[LobeIndex].N = Context.m_Lobes[0].N;
					_Lobes[LobeIndex].Error = FunctionMinimum;

					// Keep in the list of best fits
					BestFits.Insert( 0, _Lobes[LobeIndex] );

					_RMS[LobeIndex] = FunctionMinimum;
				}

				//////////////////////////////////////////////////////////////////////////
				// 1.2] At this point, we have the "best" cosine lobe fit for the given BRDF
				// We must subtract the influence of that lobe from the current BRDF and restart fitting with a new lobe...
				//
				double		OldMaxValue = -double.MaxValue;
				double		NewMaxValue = -double.MaxValue;
				CosineLobe	LobeToSubtract = _Lobes[LobeIndex];
				for ( int SampleIndex=0; SampleIndex < ms_BRDFSamples.Length; SampleIndex++ )
				{
					BRDFSample	Sample = ms_BRDFSamples[SampleIndex];

					double		LobeInfluence = Sample.m_DotProduct.x*LobeToSubtract.C.x + Sample.m_DotProduct.y*LobeToSubtract.C.y + Sample.m_DotProduct.z*LobeToSubtract.C.z;
								LobeInfluence = Math.Max( 0.0, LobeInfluence );
								LobeInfluence = Math.Pow( LobeInfluence, LobeToSubtract.N );

					double		CurrentBRDFValue = Context.m_BRDF[Sample.m_BRDFIndex];
								CurrentBRDFValue *= Sample.m_CosThetaIn;

// 					if ( CurrentBRDFValue > 100.0 )
// 						return 1;

					OldMaxValue = Math.Max( OldMaxValue, CurrentBRDFValue );

								CurrentBRDFValue -= LobeInfluence;
								CurrentBRDFValue = Math.Max( 0, CurrentBRDFValue );	// Constrain to positive values only

					NewMaxValue = Math.Max( NewMaxValue, CurrentBRDFValue );

					Context.m_BRDF[Sample.m_BRDFIndex] = CurrentBRDFValue;
				}
			}


			//////////////////////////////////////////////////////////////////////////
			// 2] At this point, we have a set of SH lobes that are individual best fits to the goal SH coefficients
			// We will finally apply a global BFGS minimzation using all of the total cosine lobes
			//
			double[]	GlobalLobeCoefficients = new double[1+4*_Lobes.Length];	// Don't forget the BFGS function annoyingly uses indices starting from 1!
			ms_TempCoefficientsGlobal = new double[1+4*_Lobes.Length];

			// 2.1] Re-assign the original BRDF to which we compare to
			Context.m_BRDF = _BRDF;

			// 2.2] Re-assign the best lobes as initial best guess
			Context.m_Lobes = _Lobes;
			for ( int LobeIndex=0; LobeIndex < _Lobes.Length; LobeIndex++ )
			{
				CosineLobe	SourceLobe = _Lobes[LobeIndex];
				GlobalLobeCoefficients[1+4*LobeIndex+0] = SourceLobe.C.x;
				GlobalLobeCoefficients[1+4*LobeIndex+1] = SourceLobe.C.y;
				GlobalLobeCoefficients[1+4*LobeIndex+2] = SourceLobe.C.z;
				GlobalLobeCoefficients[1+4*LobeIndex+3] = SourceLobe.N;
			}

			// 2.3] Apply BFGS minimzation to the entire set of coefficients
			int		IterationsCountGlobal = 0;
			double	FunctionMinimumGlobal = double.MaxValue;
			try
			{
				FunctionMinimumGlobal = dfpmin( GlobalLobeCoefficients, _BFGSConvergenceTolerance, out IterationsCountGlobal, new BFGSFunctionEval( BRDFMappingGlobalFunctionEval ), new BFGSFunctionGradientEval( BRDFMappingGlobalFunctionGradientEval ), Context );
			}
			catch ( Exception )
			{
				CrashesCount++;
			}

			// 2.4] Save the final optimized results
			for ( int LobeIndex=0; LobeIndex < _Lobes.Length; LobeIndex++ )
			{
				CosineLobe	TargetLobe = _Lobes[LobeIndex];
				TargetLobe.C.Set( GlobalLobeCoefficients[1+4*LobeIndex+0], GlobalLobeCoefficients[1+4*LobeIndex+1], GlobalLobeCoefficients[1+4*LobeIndex+2] );
				TargetLobe.N = GlobalLobeCoefficients[1+4*LobeIndex+3];
			}

			// Give final 100% feedback
			if ( _Delegate != null )
				_Delegate( 1.0f );

			return FunctionMinimumGlobal;
		}

		#region BFGS Minimization

		#region Local Minimization Delegates

		protected static double	BRDFMappingLocalFunctionEval( double[] _Coefficients, object _Params )
		{
			BRDFFitEvaluationContext	Context = _Params as BRDFFitEvaluationContext;

			// PATCH: Make sure the coefficient is strictly positive
			if ( _Coefficients[4] < EPS )
				_Coefficients[4] = EPS;

			// Copy current coefficients into the current cosine lobe
			Context.m_Lobes[0].C.Set( _Coefficients[1], _Coefficients[2], _Coefficients[3] );	// Remember those stupid coefficients are indexed from 1!
			Context.m_Lobes[0].N = _Coefficients[4];

			// Sum differences between current lobe estimates and current goal BRDF
			double	Normalizer = 1.0 / ms_BRDFSamples.Length;
			double	SumSquareDifference = ComputeSummedDifferences( ms_BRDFSamples, Normalizer, Context.m_BRDF, Context.m_Lobes );

			// Keep the result for gradient eval
			Context.m_SumSquareDifference = SumSquareDifference;

			return	SumSquareDifference;
		}

		static double[]	ms_TempCoefficientsLocal = new double[5];
		protected static void	BRDFMappingLocalFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params )
		{
			BRDFFitEvaluationContext	Context = _Params as BRDFFitEvaluationContext;

			if ( _Coefficients[4] < EPS )
				throw new Exception( "Invalid negative exponent!" );

			double	Normalizer = 1.0 / ms_BRDFSamples.Length;

			// Compute derivatives for each coefficient
			_Gradients[0] = 0.0;
			for ( int DerivativeIndex=1; DerivativeIndex < _Coefficients.Length; DerivativeIndex++ )
			{
				// Copy coefficients and add a tiny delta for derivative estimate
				_Coefficients.CopyTo( ms_TempCoefficientsLocal, 0 );
				ms_TempCoefficientsLocal[DerivativeIndex] += DERIVATIVE_OFFSET;

				// Copy current coefficients into the current cosine lobe
				Context.m_Lobes[0].C.Set( ms_TempCoefficientsLocal[1], ms_TempCoefficientsLocal[2], ms_TempCoefficientsLocal[3] );	// Remember those stupid coefficients are indexed from 1!
				Context.m_Lobes[0].N = Math.Max( EPS, ms_TempCoefficientsLocal[4] );	// Exponents are constrained to be strictly positive!

				// Sum differences between current ZH estimates and current SH goal estimates
				double	SumSquareDifference = ComputeSummedDifferences( ms_BRDFSamples, Normalizer, Context.m_BRDF, Context.m_Lobes );

				// Compute delta with fixed central square difference
				_Gradients[DerivativeIndex] = (SumSquareDifference - Context.m_SumSquareDifference) / DERIVATIVE_OFFSET;
			}

//			_Gradients[4] = Math.Log( _Gradients[4] );	// Use log of gradient instead since we're dealing with an exponent here...
//			_Gradients[4] = Math.Sign( _Gradients[4] ) * Math.Log( 1+Math.Abs(_Gradients[4]) );	// Use log of gradient instead since we're dealing with an exponent here...
		}

		#endregion

		#region Global Minimization Delegates

		protected static double	BRDFMappingGlobalFunctionEval( double[] _Coefficients, object _Params )
		{
			BRDFFitEvaluationContext	Context = _Params as BRDFFitEvaluationContext;

			// Copy current coefficients into the current cosine lobes
			for ( int LobeIndex=0; LobeIndex < Context.m_Lobes.Length; LobeIndex++ )
			{
				CosineLobe	Lobe = Context.m_Lobes[LobeIndex];
				int			CoeffOffset = 1+4*LobeIndex;	// Remember those stupid coefficients are indexed from 1!
				Lobe.C.Set( _Coefficients[CoeffOffset+0], _Coefficients[CoeffOffset+1], _Coefficients[CoeffOffset+2] );
				Lobe.N = _Coefficients[CoeffOffset+3];
			}

			// Sum differences between current lobe estimates and current goal BRDF
			double	Normalizer = 1.0 / ms_BRDFSamples.Length;
			double	SumSquareDifference = ComputeSummedDifferences( ms_BRDFSamples, Normalizer, Context.m_BRDF, Context.m_Lobes );

			// Keep the result for gradient eval
			Context.m_SumSquareDifference = SumSquareDifference;

			return	SumSquareDifference;
		}

		static double[]	ms_TempCoefficientsGlobal = null;
		protected static void	BRDFMappingGlobalFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params )
		{
			BRDFFitEvaluationContext	Context = _Params as BRDFFitEvaluationContext;

			double	Normalizer = 1.0 / ms_BRDFSamples.Length;

			// Compute derivatives for each coefficient
			_Gradients[0] = 0.0;
			for ( int DerivativeIndex=1; DerivativeIndex < _Coefficients.Length; DerivativeIndex++ )
			{
				// Copy coefficients and add a tiny delta for derivative estimate
				_Coefficients.CopyTo( ms_TempCoefficientsGlobal, 0 );
				ms_TempCoefficientsGlobal[DerivativeIndex] += DERIVATIVE_OFFSET;	// Add a tiny delta for derivative estimate

				// Copy current coefficients into the current cosine lobes
				for ( int LobeIndex=0; LobeIndex < Context.m_Lobes.Length; LobeIndex++ )
				{
					CosineLobe	Lobe = Context.m_Lobes[LobeIndex];
					int			CoeffOffset = 1+4*LobeIndex;	// Remember those stupid coefficients are indexed from 1!
					Lobe.C.Set( ms_TempCoefficientsGlobal[CoeffOffset+0], ms_TempCoefficientsGlobal[CoeffOffset+1], ms_TempCoefficientsGlobal[CoeffOffset+2] );
					Lobe.N = Math.Max( EPS, ms_TempCoefficientsGlobal[CoeffOffset+3] );	// Exponents are constrained to be strictly positive!
				}

				// Sum differences between current ZH estimates and current SH goal estimates
				double	SumSquareDifference = ComputeSummedDifferences( ms_BRDFSamples, Normalizer, Context.m_BRDF, Context.m_Lobes );

				// Compute delta with fixed central square difference
				_Gradients[DerivativeIndex] = (SumSquareDifference - Context.m_SumSquareDifference) / DERIVATIVE_OFFSET;
			}
		}

		#endregion

		/// <summary>
		/// Computes the square difference between a current cosine lobe estimate and a goal BRDF given a set of samples
		/// </summary>
		/// <param name="_SamplesCollection">The collection of samples to use for the computation</param>
		/// <param name="_Normalizer">The normalizer for the final result</param>
		/// <param name="_GoalBDRF">The goal BRDF function to compute square difference from</param>
		/// <param name="_LobeEstimates">The cosine lobes matching the BRDF</param>
		/// <returns>The square difference between goal and estimate</returns>
		private static double		ComputeSummedDifferences( BRDFSample[] _Samples, double _Normalizer, double[] _GoalBDRF, CosineLobe[] _LobeEstimates )
		{
			// Sum differences between current ZH estimates and current SH goal estimates
			double	SumSquareDifference = 0.0;
			double	GoalValue, CurrentValue, TempLobeDot;

			int		SamplesCount = _Samples.Length;
			int		LobesCount = _LobeEstimates.Length;

			for ( int SampleIndex=0; SampleIndex < SamplesCount; SampleIndex++ )
			{
				BRDFSample	Sample = _Samples[SampleIndex];

				// Get BRDF value for that sample
				GoalValue = _GoalBDRF[Sample.m_BRDFIndex];
// 				if ( GoalValue < 0.0 )
// 					throw new Exception( "Unexpected negative value!" );
				if ( double.IsNaN( GoalValue ) )
					throw new Exception( "Unexpected NaN!" );

				// DEBUG => Multiply by cos(ThetaIn) to test if it gets us a better conditioning
 				GoalValue *= Sample.m_CosThetaIn;

				// Estimate cosine lobe value in that direction
				CurrentValue = 0.0;
				for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
				{
					CosineLobe	Lobe = _LobeEstimates[LobeIndex];
					TempLobeDot = Lobe.C.x * Sample.m_DotProduct.x + Lobe.C.y * Sample.m_DotProduct.y + Lobe.C.z * Sample.m_DotProduct.z;
					TempLobeDot = Math.Max( 0.0, TempLobeDot );
					TempLobeDot = Math.Pow( TempLobeDot, Lobe.N );
					CurrentValue += TempLobeDot;
				}

				// Sum difference between estimate and goal
				SumSquareDifference += (CurrentValue - GoalValue) * (CurrentValue - GoalValue);
//				SumSquareDifference += Math.Abs( CurrentValue - GoalValue );	// Just to avoid super large numbers!
			}

			// Normalize
			SumSquareDifference *= _Normalizer;

			return	SumSquareDifference;
		}

		#region BFGS Algorithm

		protected static readonly int		ITMAX = 200;
		protected static readonly double	EPS = 3.0e-8;
		protected static readonly double	TOLX = 4*EPS;
		protected static readonly double	STPMX = 100.0;			// Scaled maximum step length allowed in line searches.

		protected static void		ENSUREVALID( double[] _Coefficients )
		{
			for ( int i=1; i < _Coefficients.Length; i++ )
			{
				double	v = _Coefficients[i];
				if ( double.IsInfinity( v ) )
					throw new Exception( "Infinity!" );
				if ( double.IsNaN( v ) )
					throw new Exception( "NaN!" );
			}
		}

		/// <summary>
		/// Performs BFGS function minimzation on a quadratic form function evaluated by the provided delegate
		/// </summary>
		/// <param name="_Coefficients">The array of initial coefficients (indexed from 1!!) that will also contain the resulting coefficients when the routine has converged</param>
		/// <param name="_ConvergenceTolerance">The tolerance error to accept as the minimum of the function</param>
		/// <param name="_PerformedIterationsCount">The amount of iterations performed to reach the minimum</param>
		/// <param name="_FunctionEval">The delegate used to evaluate the function to minimize</param>
		/// <param name="_FunctionGradientEval">The delegate used to evaluate the gradient of the function to minimize</param>
		/// <param name="_Params">Some user params passed to the evaluation functions</param>
		/// <returns>The found minimum</returns>
		protected static double	dfpmin( double[] _Coefficients, double _ConvergenceTolerance, out int _PerformedIterationsCount, BFGSFunctionEval _FunctionEval, BFGSFunctionGradientEval _FunctionGradientEval, object _Params )
		{
			double		Minimum = double.MaxValue;
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
				lnsrch( n, _Coefficients, fp, g, xi, pnew, out Minimum, stpmax, out check, _FunctionEval, _Params );
				ENSUREVALID( _Coefficients );
				fp = Minimum;

				for ( i=1; i<=n; i++ )
				{
					xi[i] = pnew[i] - _Coefficients[i];	// Update the line direction
					_Coefficients[i] = pnew[i];			// as well as the current point
				}
				ENSUREVALID( _Coefficients );

				// Test for convergence on Delta X
				test = 0.0;
				for ( i=1; i <= n; i++ )
				{
					temp = Math.Abs( xi[i] ) / Math.Max( Math.Abs( _Coefficients[i] ), 1.0 );
					if ( temp > test )
						test = temp;
				}

				if ( test < TOLX )
					return Minimum;	// Done!

				// Save the old gradient
				for ( i=1; i <= n; i++ )
					dg[i] = g[i];

				// Get the new one
				_FunctionGradientEval( _Coefficients, g, _Params );

				// Test for convergence on zero gradient
				test = 0.0;
				den = Math.Max( Minimum, 1.0 );
				for ( i=1; i <= n; i++ )
				{
					temp = Math.Abs( g[i] ) * Math.Max( Math.Abs( _Coefficients[i] ), 1.0 ) / den;
					if ( temp > test )
						test = temp;
				}

				if ( test < _ConvergenceTolerance )
					return Minimum;	// Done!

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
			int	IterationsCount = 0;
			while ( true )
			{
				IterationsCount++;

				for ( i=1; i <= n; i++ )
					x[i] = xold[i] + alam * p[i];

				f = _FunctionEval( x, _Params );
				if ( double.IsInfinity( f ) )
					throw new Exception( "Infinity!" );
				ENSUREVALID( x );
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
			Half.Set( Math.Cos( _PhiHalf ) * SinTheta_half, Math.Sin( _PhiHalf ) * SinTheta_half, Math.Cos( _ThetaHalf ) );

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
// 			Temp.Rotate( ref Half, _PhiDiff + Math.PI, out Out );

			// ...or by mirroring in "Half tangent space"
			double	MirrorX = -In.Dot( ref OrthoX );
			double	MirrorY = -In.Dot( ref OrthoY );
			double	z = In.Dot( ref Half );
			Out.Set(
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
			_PhiIn = Math.Atan2( In.y, In.x );
			_ThetaOut = Math.Acos( Out.z );
			_PhiOut = Math.Atan2( Out.y, Out.x );
		}

		static void	half_diff_coords_to_std_coords( double _ThetaHalf, double _PhiHalf, double _ThetaDiff, double _PhiDiff,
													ref Vector3 _In, ref Vector3 _Out )
		{
			double	SinTheta_half = Math.Sin( _ThetaHalf );
			Half.Set( Math.Cos( _PhiHalf ) * SinTheta_half, Math.Sin( _PhiHalf ) * SinTheta_half, Math.Cos( _ThetaHalf ) );

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
			Temp.Rotate( ref Half, _PhiDiff, out _In );

			// ...or by mirroring in "Half tangent space"
			double	MirrorX = -_In.Dot( ref OrthoX );
			double	MirrorY = -_In.Dot( ref OrthoY );
			double	z = _In.Dot( ref Half );
			_Out.Set(
				MirrorX*OrthoX.x + MirrorY*OrthoY.x + z*Half.x,
				MirrorX*OrthoX.y + MirrorY*OrthoY.y + z*Half.y,
				MirrorX*OrthoX.z + MirrorY*OrthoY.z + z*Half.z
			);

// 			if ( _In.z < -0.5 || _Out.z < -0.5 )
// 				throw new Exception( "RHA MAIS MERDE!" );
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
						int[]		NegativeValuesCount = new int[3] { 0, 0, 0 };
						double[]	MinValues = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
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
							{
								double	Value = Factor * Reader.ReadDouble();
								ComponentArray[CoeffIndex] = Value;
								if ( Value < 0.0 )
									NegativeValuesCount[ComponentIndex]++;
								MinValues[ComponentIndex] = Math.Min( MinValues[ComponentIndex], Value );
							}
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
	}
}
