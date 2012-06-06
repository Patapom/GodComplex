using System;
using System.Collections.Generic;
using System.Text;

using WMath;

namespace SphericalHarmonics
{
	/// <summary>
	/// The functions gathered here allow to work with Spherical Harmonics and Zonal Harmonics
	/// 
	/// ===============================================================================
	/// The convention for transformation between spherical and cartesian coordinates is :
	/// 
	///		( sinθ cosϕ, sinθ sinϕ, cosθ ) → (x, y, z)
	/// 
	///	 _ Azimuth ϕ is zero on +X and increases CCW (i.e. PI/2 at +Y, PI at -X and 3PI/2 at -Y)
	///	 _ Elevation θ is zero on +Z and PI on -Z
	/// 
	/// 
	///                   Z θ=0
	///                   |
	///                   |
	///                   |
	/// ϕ=3PI/2 -Y - - - -o------+Y ϕ=PI/2
	///                  /.
	///                 / .
	///               +X  .
	///             ϕ=0   .
	///                  -Z θ=PI
	/// 
	/// So cartesian to polar coordinates is computed this way:
	///		θ = acos( Z );
	///		ϕ = atan2( Y, X );
	/// 
	/// </summary>
	public class SHFunctions
	{
		#region CONSTANTS

		protected static double[]	FACTORIAL = new double[] {	1.0,
																1.0,
																2.0,
																6.0,
																24.0,
																120.0,
																720.0,
																5040.0,
																40320.0,
																362880.0,
																3628800.0,
																39916800.0,
																479001600.0,
																6227020800.0,
																87178291200.0,
																1307674368000.0,
																20922789888000.0,
																355687428096000.0,
																6402373705728000.0,
																1.21645100408832e+17,
																2.43290200817664e+18,
																5.109094217170944e+19,
																1.12400072777760768e+21,
																2.58520167388849766e+22,
																6.20448401733239439e+23,
																1.55112100433309860e+25,
																4.03291461126605636e+26,
																1.08888694504183522e+28,
																3.04888344611713861e+29,
																8.84176199373970195e+30,
																2.65252859812191059e+32,
																8.22283865417792282e+33,
																2.63130836933693530e+35
															};


		protected static double		SQRT2 = Math.Sqrt( 2.0 );

		#endregion

		#region NESTED TYPES

		/// <summary>
		/// The delegate to use to evaluate a function that will be encoded into a SH basis
		/// θ is given in [0,PI] with 0 being the direction of POSITIVE Y while PI is the direction of NEGATIVE Y
		/// ϕ is given in [0,2PI] with 0 being POSITIVE Z, PI/2 being NEGATIVE X, PI being NEGATIVE Z and 3/2PI being POSITIVE X (beware of the CLOCKWISE evolution in the Z^X plane!)
		/// </summary>
		/// <param name="_θ">The polar elevation in [0,PI]</param>
		/// <param name="_ϕ">The azimuth in [0,2PI]</param>
		/// <returns>The function value for the requested angles</returns>
		public delegate double		EvaluateFunctionSH( double _θ, double _ϕ );

		/// <summary>
		/// The delegate to use to evaluate a function that will be encoded into a SH basis
		/// θ is given in [0,PI] with 0 being the direction of POSITIVE Y while PI is the direction of NEGATIVE Y
		/// ϕ is given in [0,2PI] with 0 being POSITIVE Z, PI/2 being NEGATIVE X, PI being NEGATIVE Z and 3/2PI being POSITIVE X (beware of the CLOCKWISE evolution in the Z^X plane!)
		/// </summary>
		/// <param name="_θ">The polar elevation in [0,PI]</param>
		/// <param name="_ϕ">The azimuth in [0,2PI]</param>
		/// <param name="_Value">The function value for the requested angles</param>
		public delegate void		EvaluateFunctionSHVector3( double _θ, double _ϕ, WMath.Vector _Value );

		/// <summary>
		/// The delegate to use to evaluate a function that will be encoded into a ZH basis
		/// The function to evalue is circularly symetric about the Y axis and must only be dependent on θ.
		/// θ is given in [0,PI] with 0 being the direction of positive Y while PI is the direction of negative Y
		/// </summary>
		/// <param name="_θ">The polar elevation in [0,PI]</param>
		/// <returns>The function value for the requested angle</returns>
		public delegate double		EvaluateFunctionZH( double _θ );

		/// <summary>
		/// The delegate to use to receive feedback for the mapping method "MapSHIntoZH()"
		/// </summary>
		/// <param name="_Progress">A value in [0,1] indicating mapping progress</param>
		public delegate void		ZHMappingFeedback( float _Progress );

		/// <summary>
		/// The optional delegate to provide to the Convolve() method to debug Clebsch-Gordan coefficients
		/// </summary>
		/// <param name="_AccumulatedSHCoeffIndex"></param>
		/// <param name="_SHCoeffIndex0"></param>
		/// <param name="_SHCoeffIndex1"></param>
		/// <param name="_Sign"></param>
		/// <param name="_ClebschGordanCoefficient"></param>
		/// <remarks>The operation performed when the delegate is called is :
		/// Result[_AccumulatedSHCoeffIndex] += SHVector0[_SHCoeffIndex0] * SHVector1[_SHCoeffIndex1] * _Sign * _ClebschGordanCoefficient
		/// </remarks>
		public delegate void		ConvolutionDelegate( int _AccumulatedSHCoeffIndex, int _SHCoeffIndex0, int _SHCoeffIndex1, double _Sign, double _ClebschGordanCoefficient );

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

		protected class		ZHMappingLocalFunctionEvaluationContext
		{
			public int					m_Order = 0;						// The SH order
			public Vector2D				m_LobeDirection = null;				// The fixed lobe direction

			// The precomputed table of SH coefficients
			public double				m_Normalizer = 0;
			public SHSamplesCollection	m_SHSamples = new SHSamplesCollection();
			public double[]				m_SHEvaluation = null;				// The list of goal SH evaluation for each sample (changes only on a per-lobe basis)

			// Not to fill in, just a placeholder for computations so we don't re-allocate the room every time
			public double[]				m_ZHCoefficients = null;
			public double[]				m_RotatedZHCoefficients = null;
			public double				m_SumSquareDifference = 0.0;		// The last evaluated square difference between current ZH lobe and SH goal
		};

		protected class		ZHMappingGlobalFunctionEvaluationContext
		{
			public int					m_Order = 0;						// The SH order
			public int					m_LobesCount = 0;					// The amount of encoded lobes
			public double[]				m_DerivativesDelta;					// The array of derivatives deltas

			// The precomputed table of SH coefficients
			public double				m_Normalizer = 0;
			public SHSamplesCollection	m_SHSamples = new SHSamplesCollection();
			public double[]				m_SHEvaluation = null;				// The list of goal SH evaluation for each sample

			// Not to fill in, just a placeholder for computations so we don't re-allocate the room every time
			public double[]				m_ZHCoefficients = null;
			public double[]				m_RotatedZHCoefficients = null;
			public double[]				m_SumRotatedZHCoefficients = null;
			public double				m_SumSquareDifference = 0.0;		// The last evaluated square difference between current ZH lobe and SH goal
		};

		#endregion

		#region METHODS

		#region SH

		// Returns a spot sample of a Spherical Harmonic basis function
		//		l is the band, range [0..N]
		//		m in the range [-l..l]
		//		θ in the range [0..Pi]
		//		ϕ in the range [0..2*Pi]
		//
		public static double	ComputeSH( int l, int m, double _θ, double _ϕ )
		{
			if ( Math.Abs( m ) > l )
				throw new Exception( "m parameter is outside the [-l,+l] range!" );

			if ( m == 0 )
				return	K( l, m ) * P( l, m, Math.Cos( _θ ) );
			else if ( m > 0 )
				return SQRT2 * K( l, m ) * Math.Cos( m * _ϕ ) * P( l, m, Math.Cos( _θ ) );
			else
				return SQRT2 * K( l, -m ) * Math.Sin( -m * _ϕ ) * P( l, -m, Math.Cos( _θ ) );
		}

		// Here, we choose the convention that the vertical axis defining THETA is the Y axis
		//  and the axes defining PHI are X and Z where PHI = 0 when the vector is aligned to the positive Z axis
		//
		// NOTE ==> The '_Direction' vector must be normalized!!
		//
		public static double	ComputeSH( int l, int m, Vector _Direction )
		{
			// Convert from cartesian to polar coords
			double	θ = 0.0;
			double	ϕ = 0.0f;
			CartesianToSpherical( _Direction, out θ, out ϕ );

			return	ComputeSH( l, m, θ, ϕ );
		}

		// Computes a SH windowed with a cardinal sine function
		//
		public static double	ComputeSHWindowedSinc( int l, int m, double _θ, double _ϕ, int _Order )
		{
			return ComputeSigmaFactorSinc( l, _Order ) * ComputeSH( l, m, _θ, _ϕ );
		}

		// Computes a SH windowed with a cosine function
		//
		public static double	ComputeSHWindowedCos( int l, int m, double _θ, double _ϕ, int _Order )
		{
			return ComputeSigmaFactorCos( l, _Order ) * ComputeSH( l, m, _θ, _ϕ );
		}

		/// <summary>
		/// Evaluates the amplitude encoded by the SH coefficients in the provided direction
		/// </summary>
		/// <param name="_Coefficients">The SH coefficients encoding an amplitude in various directions</param>
		/// <param name="_ϕ">The azimuth of the direction of the SH sampling</param>
		/// <param name="_θ">The elevation of the direction of the SH sampling</param>
		/// <param name="_Order">The order of the SH vector</param>
		/// <returns></returns>
		public static double	EvaluateSH( double[] _Coefficients, double _θ, double _ϕ, int _Order )
		{
			double	Result = 0.0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++ )
					Result += ComputeSH( l, m, _θ, _ϕ ) * _Coefficients[l*(l+1)+m];

			return	Result;
		}

		/// <summary>
		/// Evaluates the amplitude encoded by the SH coefficients in the provided direction
		/// </summary>
		/// <param name="_Coefficients">The SH coefficients encoding an amplitude in various directions</param>
		/// <param name="_Direction">The direction where to evalute the SH</param>
		/// <param name="_Order">The order of the SH vector</param>
		/// <returns>The SH evaluation in the provided direction</returns>
		public static double	EvaluateSH( double[] _Coefficients, Vector _Direction, int _Order )
		{
			// Convert from cartesian to polar coords
			double	θ = 0.0;
			double	ϕ = 0.0f;
			CartesianToSpherical( _Direction, out θ, out ϕ );

			return	EvaluateSH( _Coefficients, θ, ϕ, _Order );
		}

		/// <summary>
		/// Encodes a function evaluated by the provided delegate into a series of ZH coefficients
		/// </summary>
		/// <param name="_Coefficients">The coefficients supposed to best fit the provided function (the length of the vector must be _Order²)</param>
		/// <param name="_SamplesCount">The amount of samples to use to fit the function</param>
		/// <param name="_RandomSeed">The random seed to initialize the RNG with when drawing random sample position</param>
		/// <param name="_Order">The order of the SH vector to encode</param>
		/// <param name="_Delegate">The delegate that will be called to evaluate the function to encode</param>
		public static void		EncodeIntoSH( double[] _Coefficients, int _SamplesCountTheta, int _SamplesCountPhi, int _RandomSeed, int _Order, EvaluateFunctionSH _Delegate )
		{
			Random	RNG = new Random( _RandomSeed );

			// Reset coefficients
			for ( int CoefficientIndex=0; CoefficientIndex < _Coefficients.Length; CoefficientIndex++ )
				_Coefficients[CoefficientIndex] = 0.0;

			for ( int PhiIndex=0; PhiIndex < _SamplesCountPhi; PhiIndex++ )
				for ( int ThetaIndex=0; ThetaIndex < _SamplesCountTheta; ThetaIndex++ )
				{
					// Draw uniformly sampled θ and ϕ angles
					double	θ = 2.0 * Math.Acos( Math.Sqrt( 1.0 - (ThetaIndex + RNG.NextDouble()) / _SamplesCountTheta ) );
					double	ϕ = 2.0 * Math.PI * (PhiIndex + RNG.NextDouble()) / _SamplesCountPhi;

					// Accumulate coefficients
					double	Value = _Delegate( θ, ϕ );
					for ( int l=0; l < _Order; l++ )
						for ( int m=-l; m <= +l; m++ )
							_Coefficients[l*(l+1)+m] += Value * ComputeSH( l, m, θ, ϕ );
				}

			// Final normalizing
			double	Normalizer = 4.0 * Math.PI / (_SamplesCountTheta * _SamplesCountPhi);
			for ( int CoefficientIndex=0; CoefficientIndex < _Order*_Order; CoefficientIndex++ )
				_Coefficients[CoefficientIndex] *= Normalizer;
		}

		/// <summary>
		/// Encodes a function evaluated by the provided delegate into a series of ZH coefficients
		/// </summary>
		/// <param name="_Coefficients">The coefficients supposed to best fit the provided function (the length of the vector must be _Order²)</param>
		/// <param name="_SamplesCount">The amount of samples to use to fit the function</param>
		/// <param name="_RandomSeed">The random seed to initialize the RNG with when drawing random sample position</param>
		/// <param name="_Order">The order of the SH vector to encode</param>
		/// <param name="_Delegate">The delegate that will be called to evaluate the function to encode</param>
		public static void		EncodeIntoSH( WMath.Vector[] _Coefficients, int _SamplesCountTheta, int _SamplesCountPhi, int _RandomSeed, int _Order, EvaluateFunctionSHVector3 _Delegate )
		{
			Random	RNG = new Random( _RandomSeed );

			// Reset coefficients
			for ( int CoefficientIndex=0; CoefficientIndex < _Coefficients.Length; CoefficientIndex++ )
			{
				if ( _Coefficients[CoefficientIndex] == null )
					_Coefficients[CoefficientIndex] = new Vector();
				_Coefficients[CoefficientIndex].Zero();
			}

			WMath.Vector	Value = new WMath.Vector();
			for ( int PhiIndex=0; PhiIndex < _SamplesCountPhi; PhiIndex++ )
				for ( int ThetaIndex=0; ThetaIndex < _SamplesCountTheta; ThetaIndex++ )
				{
					// Draw uniformly sampled θ and ϕ angles
					double	θ = 2.0 * Math.Acos( Math.Sqrt( 1.0 - (ThetaIndex + RNG.NextDouble()) / _SamplesCountTheta ) );
					double	ϕ = 2.0 * Math.PI * (PhiIndex + RNG.NextDouble()) / _SamplesCountPhi;

					// Accumulate coefficients
					_Delegate( θ, ϕ, Value );
					for ( int l=0; l < _Order; l++ )
						for ( int m=-l; m <= +l; m++ )
							_Coefficients[l*(l+1)+m] += (float) ComputeSH( l, m, θ, ϕ ) * Value;
				}

			// Final normalizing
			float	Normalizer = 4.0f * (float) Math.PI / (_SamplesCountTheta * _SamplesCountPhi);
			for ( int CoefficientIndex=0; CoefficientIndex < _Order*_Order; CoefficientIndex++ )
				_Coefficients[CoefficientIndex] *= Normalizer;
		}

		#endregion

		#region ZH

		// Returns a spot sample of a Zonal Harmonic basis function aligned to the Y axis
		//		l is the band, range [0..N]
		//		θ in the range [0..Pi]
		//
		public static double	ComputeZH( int l, double _θ )
		{
			return	K( l, 0 ) * P( l, 0, Math.Cos( _θ ) );
		}

		// Here, we choose the convention that the vertical axis defining THETA is the Y axis
		//  and the axes defining PHI are X and Z where PHI = 0 when the vector is aligned to the positive Z axis
		//
		// NOTE ==> The '_Direction' vector must be normalized!!
		//
		public static double	ComputeZH( int l, Vector _Direction )
		{
			// Convert from cartesian to polar coords
			double	θ = 0.0;
			double	ϕ = 0.0f;
			CartesianToSpherical( _Direction, out θ, out ϕ );

			return	ComputeZH( l, θ );
		}

		/// <summary>
		/// Evaluates the amplitude encoded by the Y-aligned ZH coefficients in the provided direction
		/// </summary>
		/// <param name="_Coefficients">The SH coefficients encoding an amplitude in various directions</param>
		/// <param name="_θ">The elevation of the direction of the ZH sampling</param>
		/// <returns>The ZH evaluation in the provided direction</returns>
		public static double	EvaluateZH( double[] _Coefficients, double _θ )
		{
			double	Result = 0.0;
			for ( int l=0; l < _Coefficients.Length; l++ )
				Result += ComputeSH( l, 0, _θ, 0.0 ) * _Coefficients[l];

			return	Result;
		}

		/// <summary>
		/// Evaluates the amplitude encoded by the Y-aligned ZH coefficients in the provided direction
		/// </summary>
		/// <param name="_Coefficients">The SH coefficients encoding an amplitude in various directions</param>
		/// <param name="_Direction">The direction where to evalute the ZH</param>
		/// <returns>The ZH evaluation in the provided direction</returns>
		public static double	EvaluateZH( double[] _Coefficients, Vector _Direction )
		{
			// Convert from cartesian to polar coords
			double	θ = 0.0;
			double	ϕ = 0.0f;
			CartesianToSpherical( _Direction, out θ, out ϕ );

			return	EvaluateZH( _Coefficients, θ );
		}

		/// <summary>
		/// Attempts to encode a function evaluated by the provided delegate into a series of ZH coefficients
		/// </summary>
		/// <param name="_Coefficients">The coefficients supposed to best fit the provided function (the length of the vector will also provide the ZH/SH order)</param>
		/// <param name="_SamplesCount">The amount of samples to use to fit the function</param>
		/// <param name="_RandomSeed">The random seed to initialize the RNG with when drawing random sample position</param>
		/// <param name="_Delegate">The delegate that will be called to evaluate the function to encode</param>
		public static void		EncodeIntoZH( double[] _Coefficients, int _SamplesCount, int _RandomSeed, EvaluateFunctionZH _Delegate )
		{
			Random	RNG = new Random( _RandomSeed );

			// Reset coefficients
			for ( int CoefficientIndex=0; CoefficientIndex < _Coefficients.Length; CoefficientIndex++ )
				_Coefficients[CoefficientIndex] = 0.0;

			for ( int SampleIndex=0; SampleIndex < _SamplesCount; SampleIndex++ )
			{
				// Sort a uniformly sampled θ angle
				double	θ = 2.0 * Math.Acos( Math.Sqrt( 1.0 - (SampleIndex + RNG.NextDouble()) / _SamplesCount ) );

				// Accumulate coefficients
				for ( int CoefficientIndex=0; CoefficientIndex < _Coefficients.Length; CoefficientIndex++ )
					_Coefficients[CoefficientIndex] += _Delegate( θ ) * ComputeZH( CoefficientIndex, θ );
			}

			// Final normalizing
			double	Normalizer = Math.PI / _SamplesCount;
			for ( int CoefficientIndex=0; CoefficientIndex < _Coefficients.Length; CoefficientIndex++ )
				_Coefficients[CoefficientIndex] *= Normalizer;
		}

		#endregion

		#region SH Helpers

		/// <summary>
		/// Initializes the provided vector with the SH coefficients corresponding to the specified direction
		/// </summary>
		/// <param name="_Order">The order of the SH vector</param>
		/// <param name="_Direction">The direction of the SH sampling</param>
		/// <param name="_Coefficients">The array of coefficients to fill up</param>
		public static void	InitializeSHCoefficients( int _Order, Vector _Direction, double[] _Coefficients )
		{
			int	Index = 0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++ )
					_Coefficients[Index++] = ComputeSH( l, m, _Direction );
		}

		/// <summary>
		/// Initializes the provided vector with the SH coefficients corresponding to the specified direction
		/// </summary>
		/// <param name="_Order">The order of the SH vector</param>
		/// <param name="_θ">The elevation of the direction of the SH sampling</param>
		/// <param name="_ϕ">The azimuth of the direction of the SH sampling</param>
		/// <param name="_Coefficients">The array of coefficients to fill up</param>
		public static void	InitializeSHCoefficients( int _Order, double _θ, double _ϕ, double[] _Coefficients )
		{
			int	Index = 0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++ )
					_Coefficients[Index++] = ComputeSH( l, m, _θ, _ϕ );
		}

		/// <summary>
		/// Initializes the provided vector with the SH coefficients corresponding to the specified direction
		/// </summary>
		/// <param name="_Order">The order of the SH vector</param>
		/// <param name="_θ">The elevation of the direction of the SH sampling</param>
		/// <param name="_ϕ">The azimuth of the direction of the SH sampling</param>
		/// <param name="_Coefficients">The array of coefficients to fill up</param>
		public static void	InitializeSHCoefficients( int _Order, double _θ, double _ϕ, Vector[] _Coefficients )
		{
			int	Index = 0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++ )
				{
					float	fCoeff = (float) ComputeSH( l, m, _θ, _ϕ );
					_Coefficients[Index++].Set( fCoeff, fCoeff, fCoeff );
				}
		}

		/// <summary>
		/// Initializes the provided vector with the SH coefficients corresponding to the specified direction
		/// </summary>
		/// <param name="_Order">The order of the SH vector</param>
		/// <param name="_Direction">The direction of the SH sampling</param>
		/// <param name="_Coefficients">The array of coefficients to fill up</param>
		public static void	InitializeSHCoefficients( int _Order, Vector _Direction, Vector[] _Coefficients )
		{
			int	Index = 0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++ )
				{
					float	fCoeff = (float) ComputeSH( l, m, _Direction );
					_Coefficients[Index++].Set( fCoeff, fCoeff, fCoeff );
				}
		}

		/// <summary>
		/// Applies a simple mirroring on the Y axis
		/// </summary>
		/// <param name="_Vector">The vector to mirror</param>
		/// <param name="_MirroredVector">The mirrored vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		public static void			MirrorY( double[] _Vector, double[] _MirroredVector, int _Order )
		{
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= l; m++ )
				{
					int		k = l*(l+1) + m;
					_MirroredVector[k] = (((l+m) & 1) == 0 ? +1 : -1) * _Vector[k];
				}
		}

		/// <summary>
		/// Applies a simple mirroring on the Y axis
		/// </summary>
		/// <param name="_Vector">The vector to mirror</param>
		/// <param name="_MirroredVector">The mirrored vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		public static void			MirrorY( WMath.Vector[] _Vector, WMath.Vector[] _MirroredVector, int _Order )
		{
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= l; m++ )
				{
					int		k = l*(l+1) + m;
					_MirroredVector[k] = (((l+m) & 1) == 0 ? +1 : -1) * _Vector[k];
				}
		}

		/// <summary>
		/// Applies a simple fast rotation about the Y axis
		/// </summary>
		/// <param name="_Vector">The vector to rotate</param>
		/// <param name="_ϕ">The rotation angle</param>
		/// <param name="_RotatedVector">The rotated vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		public static void			RotateY( double[] _Vector, double _ϕ, double[] _RotatedVector, int _Order )
		{
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= l; m++ )
					if ( m != 0 )
					{
						double	fCos = Math.Cos( Math.Abs( m ) * _ϕ );
						double	fSin = Math.Sin( Math.Abs( m ) * _ϕ );

						int		CoeffIndex0 = l*(l+1) + m;
						int		CoeffIndex1 = l*(l+1) - m;

						_RotatedVector[CoeffIndex0] = _Vector[CoeffIndex0] * fCos - Math.Sign( m ) * _Vector[CoeffIndex1] * fSin;
					}
					else
						_RotatedVector[l*(l+1)] = _Vector[l*(l+1)];	// <= Don't rotate zonal harmonics
		}

		/// <summary>
		/// Applies a simple fast rotation about the Y axis
		/// </summary>
		/// <param name="_Vector">The vector to rotate</param>
		/// <param name="_ϕ">The rotation angle</param>
		/// <param name="_RotatedVector">The rotated vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		public static void			RotateY( WMath.Vector[] _Vector, double _ϕ, WMath.Vector[] _RotatedVector, int _Order )
		{
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= l; m++ )
					if ( m != 0 )
					{
						float	fCos = (float) Math.Cos( Math.Abs( m ) * _ϕ );
						float	fSin = (float) Math.Sin( Math.Abs( m ) * _ϕ );

						int		CoeffIndex0 = l*(l+1) + m;
						int		CoeffIndex1 = l*(l+1) - m;

						_RotatedVector[CoeffIndex0] = _Vector[CoeffIndex0] * fCos - Math.Sign( m ) * _Vector[CoeffIndex1] * fSin;
					}
					else
						_RotatedVector[l*(l+1)] = _Vector[l*(l+1)];	// <= Don't rotate zonal harmonics
		}

		/// <summary>
		/// Builds the matrix to apply the SH coefficients to make them rotate by the specified matrix
		/// </summary>
		/// <param name="_Rotation">The 3x3 rotation matrix to infer the SH rotation matrix from</param>
		/// <param name="_Matrix">The resulting _Order² x _Order² rotation matrix</param>
		/// <param name="_Order">The order of the SH vectors that will be rotated using the matrix</param>
		public static void	BuildRotationMatrix( Matrix3x3 _Rotation, double[,]	_Matrix, int _Order )
		{
			// This method recursively computes the SH transformation matrix based on a regular 3x3 transform
			//
			// The transformation matrix is block diagonal-sparse in the following form:
			//
			//	   1   |   0   |   0   |   0   |   0   | ...
			//	---------------------------------------------
			//	   0   |  m00  |  m01  |  m02  |   0   | ...
			//	---------------------------------------------
			//	   0   |  m10  |  m11  |  m12  |   0   | ...
			//	---------------------------------------------
			//	   0   |  m20  |  m21  |  m22  |   0   | ...
			//	---------------------------------------------
			//	   0   |   0   |   0   |   0   |  xxx  | ...
			//	---------------------------------------------
			//	  ...  |   ... |  ...  |  ...  |  ...  | ...
			//
			// The blocks from order > 1 are infered by reccurence from the block at order 1, which is a regular 3x3 matrix.
			//

			// Clear coefficients
			for ( int DimIndex0=0; DimIndex0 < _Matrix.GetLength( 0 ); DimIndex0++ )
				for ( int DimIndex1=0; DimIndex1 < _Matrix.GetLength( 1 ); DimIndex1++ )
					_Matrix[DimIndex0,DimIndex0] = 0.0;

			// Order 0 is the ambient term (constant)
			_Matrix[0,0] = 1.0f;

			// Order 1 is a regular 3x3 rotation matrix
			_Matrix[1,1] = _Rotation[0,0];	_Matrix[1,2] = _Rotation[0,1];	_Matrix[1,3] = _Rotation[0,2];
			_Matrix[2,1] = _Rotation[1,0];	_Matrix[2,2] = _Rotation[1,1];	_Matrix[2,3] = _Rotation[1,2];
			_Matrix[3,1] = _Rotation[2,0];	_Matrix[3,2] = _Rotation[2,1];	_Matrix[3,3] = _Rotation[2,2];

			// The remaining bands are built recursively
			for ( int l=2; l < _Order; l++ )
			{
				int		BandOffset = l * (l + 1);
				for ( int m=-l; m <= +l; m++ )
				{
					for ( int n=-l; n <= +l; n++ )
					{
						// Compute (u, v, w)
						double	u, v, w;
						double	m0 = m == 0 ? 1.0 : 0.0;
						double	m1 = m == 1 ? 1.0 : 0.0;
						double	mm1 = m == -1 ? 1.0 : 0.0;
						if ( Math.Abs(n) < l )
						{
							u = Math.Sqrt( (l + m) * (l - m) / ((l + n) * (l - n)) );
							v = +.5 * Math.Sqrt( (1.0 + m0) * (l + Math.Abs( m ) - 1) * (l + Math.Abs( m )) / ((l + n) * (l - n)) ) * (1.0 - 2.0 * m0);
							w = -.5 * Math.Sqrt( (l - Math.Abs( m ) - 1) * (l - Math.Abs( m )) / ((l + n) * (l - n)) ) * (1.0 - m0);
						}
						else
						{
							u = Math.Sqrt( (l + m) * (l - m) / (2 * l * (2 * l - 1)) );
							v = +.5 * Math.Sqrt( (1.0 + m0) * (l + Math.Abs( m ) - 1) * (l + Math.Abs( m )) / (2 * l * (2 * l - 1)) ) * (1.0 - 2.0 * m0);
							w = -.5 * Math.Sqrt( (l - Math.Abs( m ) - 1) * (l - Math.Abs( m )) / (2 * l * (2 * l - 1)) ) * (1.0 - m0);
						}

						// Compute (U, V, W)
						double	U, V, W;
						if ( m > 0 )
						{
							U = ComputeP( m, n, 0, l, _Matrix );
							V = ComputeP( m - 1, n, 1, l, _Matrix ) * Math.Sqrt( 1.0 + m1 ) - ComputeP( 1 - m, n, -1, l, _Matrix ) * (1.0 - m1);
							W = ComputeP( m + 1, n, 1, l, _Matrix ) + ComputeP( -m - 1, n, -1, l, _Matrix );
						}
						else if ( m < 0 )
						{
							U = ComputeP( m, n, 0, l, _Matrix );
							V = ComputeP( m + 1, n, 1, l, _Matrix ) * (1.0 + mm1) + ComputeP( -m - 1, n, -1, l, _Matrix ) * Math.Sqrt( 1.0 - mm1 );
							W = ComputeP( m - 1, n, 1, l, _Matrix ) - ComputeP( -m + 1, n, -1, l, _Matrix );
						}
						else
						{
							U = ComputeP( 0, n, 0, l, _Matrix );
							V = ComputeP( 1, n, 1, l, _Matrix ) + ComputeP( -1, n, -1, l, _Matrix );
							W = 0.0;
						}

						// Compute final coefficient
						_Matrix[BandOffset + m,BandOffset + n] = u * U + v * V + w * W;
					}
				}
			}
		}

		/// <summary>
		/// Applies rotation to the specified SH vector using the specified rotation matrix yielded by the above "BuildRotationMatrix()" method
		/// </summary>
		/// <param name="_Vector">The vector to rotate</param>
		/// <param name="_RotationMatrix">The SH rotation matrix</param>
		/// <param name="_RotatedVector">The rotated vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		public static void		Rotate( double[] _Vector, double[,] _RotationMatrix, double[] _RotatedVector, int _Order )
		{
			int	MatrixSize = 1;
			int	SourceCoefficientIndex = 0;
			int	DestCoefficientIndex = 0;
			for ( int l=0; l < _Order; l++, MatrixSize+=2 )
			{
				int	BandOffset = l * l;
				for ( int n=0; n < MatrixSize; n++ )
				{
					_RotatedVector[DestCoefficientIndex] = 0.0f;

					for ( int m=0; m < MatrixSize; m++ )
						_RotatedVector[DestCoefficientIndex] += _Vector[SourceCoefficientIndex+m] * _RotationMatrix[BandOffset + m,BandOffset + n];

					DestCoefficientIndex++;
				}
				SourceCoefficientIndex += MatrixSize;
			}
		}

		/// <summary>
		/// Applies rotation to the specified 3-vector using the specified rotation matrix returned by the "BuildRotationMatrix()" method
		/// </summary>
		/// <param name="_Vector">The 3-vector to rotate</param>
		/// <param name="_RotationMatrix">The SH rotation matrix</param>
		/// <param name="_RotatedVector">The rotated vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		public static void		Rotate( Vector[] _Vector, double[,] _RotationMatrix, Vector[] _RotatedVector, int _Order )
		{
			int	MatrixSize = 1;
			int	SourceCoefficientIndex = 0;
			int	DestCoefficientIndex = 0;
			for ( int l=0; l < _Order; l++, MatrixSize+=2 )
			{
				int	BandOffset = l * l;
				for ( int n=0; n < MatrixSize; n++ )
				{
					_RotatedVector[DestCoefficientIndex] = new Vector( 0.0f, 0.0f, 0.0f );

					for ( int m=0; m < MatrixSize; m++ )
						_RotatedVector[DestCoefficientIndex] += _Vector[SourceCoefficientIndex+m] * (float) _RotationMatrix[BandOffset + m,BandOffset + n];

					DestCoefficientIndex++;
				}
				SourceCoefficientIndex += MatrixSize;
			}
		}

		/// <summary>
		/// Computes the product of 2 SH vectors of order 3
		/// (code from John Snyder "Code Generation and Factoring for Fast Evaluation of Low-order Spherical Harmonic Products and Squares")
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>c = a * b</returns>
		public static double[]	Product3( double[] a, double[] b )
		{
			double[]	c = new double[9];
			double		ta, tb, t;

			const double	C0 = 0.282094792935999980;
			const double	C1 = -0.126156626101000010;
			const double	C2 = 0.218509686119999990;
			const double	C3 = 0.252313259986999990;
			const double	C4 = 0.180223751576000010;
			const double	C5 = 0.156078347226000000;
			const double	C6 = 0.090111875786499998;

			// [0,0]: 0,
			c[0] = C0*a[0]*b[0];

			// [1,1]: 0,6,8,
			ta = C0*a[0]+C1*a[6]-C2*a[8];
			tb = C0*b[0]+C1*b[6]-C2*b[8];
			c[1] = ta*b[1]+tb*a[1];
			t = a[1]*b[1];
			c[0] += C0*t;
			c[6] = C1*t;
			c[8] = -C2*t;

			// [1,2]: 5,
			ta = C2*a[5];
			tb = C2*b[5];
			c[1] += ta*b[2]+tb*a[2];
			c[2] = ta*b[1]+tb*a[1];
			t = a[1]*b[2]+a[2]*b[1];
			c[5] = C2*t;

			// [1,3]: 4,
			ta = C2*a[4];
			tb = C2*b[4];
			c[1] += ta*b[3]+tb*a[3];
			c[3] = ta*b[1]+tb*a[1];
			t = a[1]*b[3]+a[3]*b[1];
			c[4] = C2*t;

			// [2,2]: 0,6,
			ta = C0*a[0]+C3*a[6];
			tb = C0*b[0]+C3*b[6];
			c[2] += ta*b[2]+tb*a[2];
			t = a[2]*b[2];
			c[0] += C0*t;
			c[6] += C3*t;

			// [2,3]: 7,
			ta = C2*a[7];
			tb = C2*b[7];
			c[2] += ta*b[3]+tb*a[3];
			c[3] += ta*b[2]+tb*a[2];
			t = a[2]*b[3]+a[3]*b[2];
			c[7] = C2*t;

			// [3,3]: 0,6,8,
			ta = C0*a[0]+C1*a[6]+C2*a[8];
			tb = C0*b[0]+C1*b[6]+C2*b[8];
			c[3] += ta*b[3]+tb*a[3];
			t = a[3]*b[3];
			c[0] += C0*t;
			c[6] += C1*t;
			c[8] += C2*t;

			// [4,4]: 0,6,
			ta = C0*a[0]-C4*a[6];
			tb = C0*b[0]-C4*b[6];
			c[4] += ta*b[4]+tb*a[4];
			t = a[4]*b[4];
			c[0] += C0*t;
			c[6] -= C4*t;

			// [4,5]: 7,
			ta = C5*a[7];
			tb = C5*b[7];
			c[4] += ta*b[5]+tb*a[5];
			c[5] += ta*b[4]+tb*a[4];
			t = a[4]*b[5]+a[5]*b[4];
			c[7] += C5*t;

			// [5,5]: 0,6,8,
			ta = C0*a[0]+C6*a[6]-C5*a[8];
			tb = C0*b[0]+C6*b[6]-C5*b[8];
			c[5] += ta*b[5]+tb*a[5];
			t = a[5]*b[5];
			c[0] += C0*t;
			c[6] += C6*t;
			c[8] -= C5*t;

			// [6,6]: 0,6,
			ta = C0*a[0];
			tb = C0*b[0];
			c[6] += ta*b[6]+tb*a[6];
			t = a[6]*b[6];
			c[0] += C0*t;
			c[6] += C4*t;

			// [7,7]: 0,6,8,
			ta = C0*a[0]+C6*a[6]+C5*a[8];
			tb = C0*b[0]+C6*b[6]+C5*b[8];
			c[7] += ta*b[7]+tb*a[7];
			t = a[7]*b[7];
			c[0] += C0*t;
			c[6] += C6*t;
			c[8] += C5*t;

			// [8,8]: 0,6,
			ta = C0*a[0]-C4*a[6];
			tb = C0*b[0]-C4*b[6];
			c[8] += ta*b[8]+tb*a[8];
			t = a[8]*b[8];
			c[0] += C0*t;
			c[6] -= C4*t;
			// entry count=13
			// multiply count=120
			// addition count=74

			return c;
		}

		/// <summary>
		/// Computes the convolution of 2 vectors of SH coefficients of the specified order using Clebsch-Gordan coefficients
		/// </summary>
		/// <param name="_Vector0">First vector</param>
		/// <param name="_Vector1">Second vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		/// <returns>The convolution of the 2 vectors</returns>
		/// <remarks>This method is quite time-consuming as the convolution is computed using 5 loops but, as an optimisation, we can notice that most Clebsh-Gordan are 0
		/// and a vector of non-null coefficients could be precomputed as only the SH vectors' coefficients change</remarks>
		public static double[]		Convolve( double[] _Vector0, double[] _Vector1, int _Order, ConvolutionDelegate _Delegate )
		{
			if ( _Vector0 == null || _Vector1 == null )
				throw new Exception( "Invalid coefficients!" );
			if ( _Vector0.Length != _Vector1.Length )
				throw new Exception( "Coefficient vectors length mismatch!" );
			if ( _Order * _Order != _Vector0.Length )
				throw new Exception( "Coefficient vectors are not of the specified order!" );

			double[]	ConvolvedCoeffs = new double[_Vector0.Length];

			// Compute convolution
			int	TotalCoeffIndex = 0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++, TotalCoeffIndex++ )
				{
					ConvolvedCoeffs[TotalCoeffIndex] = 0.0;

					int	InnerTotalCoeffIndex = 0;
					for ( int l1=0; l1 < _Order; l1++ )
						for ( int m1=-l1; m1 <= +l1; m1++, InnerTotalCoeffIndex++ )
							for ( int l2=0; l2 < _Order; l2++ )
							{
								int	Bl2m1mIndex = l2*(l2+1) + m1 - m;
								if ( Bl2m1mIndex < 0 || Bl2m1mIndex >= _Order * _Order )
									continue;

								double	Sign = ((m1 - m) & 1) == 0 ? +1 : -1;
								double	Sqrt = Math.Sqrt( (2.0 * l1 + 1) * (2.0 * l2 + 1) / (4.0 * Math.PI * (2.0 * l + 1)) );
								double	CGC0 = ComputeClebschGordan( l1, l2, 0, 0, l, 0 );
								if ( Math.Abs( CGC0 ) < 1e-4 )
									continue;
								double	CGC1 = ComputeClebschGordan( l1, l2, m1, m - m1, l, m );
								if ( Math.Abs( CGC1 ) < 1e-4 )
									continue;

								double	A = _Vector0[InnerTotalCoeffIndex];
								double	B = _Vector1[Bl2m1mIndex];

								double	FinalCoeff = Sqrt * CGC0 * CGC1;

								ConvolvedCoeffs[TotalCoeffIndex] += Sign * A * B * FinalCoeff;

								if ( _Delegate != null )
									_Delegate( TotalCoeffIndex, InnerTotalCoeffIndex, Bl2m1mIndex, Sign * FinalCoeff < 0.0 ? -1.0 : +1.0, Math.Abs( FinalCoeff ) );
							}
				}

			return	ConvolvedCoeffs;
		}

		/// <summary>
		/// Computes the convolution of 2 3-vectors of SH coefficients of the specified order using Clebsch-Gordan coefficients
		/// </summary>
		/// <param name="_Vector0">First 3-vector</param>
		/// <param name="_Vector1">Second 3-vector</param>
		/// <param name="_Order">The order of the vectors (i.e. vectors must have a length of Order²)</param>
		/// <returns>The convolution of the 2 vectors</returns>
		/// <remarks>This method is quite time-consuming as the convolution is computed using 5 loops but, as an optimisation, we can notice that most Clebsh-Gordan are 0
		/// and a vector of non-null coefficients could be precomputed as only the vectors' coefficients change</remarks>
		public static Vector[]		Convolve( Vector[] _Vector0, Vector[] _Vector1, int _Order )
		{
			if ( _Vector0 == null || _Vector1 == null )
				throw new Exception( "Invalid coefficients!" );
			if ( _Vector0.Length != _Vector1.Length )
				throw new Exception( "Coefficient vectors length mismatch!" );
			if ( _Order * _Order != _Vector0.Length )
				throw new Exception( "Coefficient vectors are not of the specified order!" );

			Vector[]	ConvolvedCoeffs = new Vector[_Vector0.Length];

			// Compute convolution
			int	TotalCoeffIndex = 0;
			for ( int l=0; l < _Order; l++ )
				for ( int m=-l; m <= +l; m++, TotalCoeffIndex++ )
				{
					ConvolvedCoeffs[TotalCoeffIndex] = new Vector( 0.0f, 0.0f, 0.0f );

					int	InnerTotalCoeffIndex = 0;
					for ( int l1=0; l1 < _Order; l1++ )
						for ( int m1=-l1; m1 <= +l1; m1++, InnerTotalCoeffIndex++ )
							for ( int l2=0; l2 < _Order; l2++ )
							{
								int	Bl2m1mIndex = l2*(l2+1) + m1 - m;
								if ( Bl2m1mIndex < 0 || Bl2m1mIndex >= _Order * _Order )
									continue;

								double	Sign = ((m1 - m) & 1) == 0 ? +1 : -1;
								double	Sqrt = Math.Sqrt( (2.0 * l1 + 1) * (2.0 * l2 + 1) / (4.0 * Math.PI * (2.0 * l + 1)) );
								double	CGC0 = ComputeClebschGordan( l1, l2, 0, 0, l, 0 );
								if ( Math.Abs( CGC0 ) < 1e-4 )
									continue;
								double	CGC1 = ComputeClebschGordan( l1, l2, m1, m - m1, l, m );
								if ( Math.Abs( CGC1 ) < 1e-4 )
									continue;

								Vector	A = _Vector0[InnerTotalCoeffIndex];
								Vector	B = _Vector1[Bl2m1mIndex];

								double	FinalCoeff = Sqrt * CGC0 * CGC1;

								ConvolvedCoeffs[TotalCoeffIndex] += (float) (Sign * FinalCoeff) * A * B;
							}
				}

			return	ConvolvedCoeffs;
		}

		#endregion

		#region ZH Helpers

		/// <summary>
		/// Computes the SH coefficients of originaly Y-aligned Zonal Harmonics coefficients rotated to match the provided axis
		/// </summary>
		/// <param name="_ZHCoefficients">The ZH coefficients to rotate</param>
		/// <param name="_TargetAxis">The target axis to match (the original axis being positive Y)</param>
		/// <param name="_RotatedSHCoefficients">The rotated SH coefficients</param>
		/// <remarks>Be careful the returned coefficients are SH coefficients, hence the _RotatedSHCoefficients vector should be N² if rotating an order N Zonal Harmonics vector</remarks>
		public static void		ComputeRotatedZHCoefficients( double[] _ZHCoefficients, Vector _TargetAxis, double[] _RotatedSHCoefficients )
		{
			// Compute the convolution coefficients from input ZH coeffs
			double[]	ConvolutionCoefficients = new double[_ZHCoefficients.Length];
			for ( int ConvolutionCoefficientIndex=0; ConvolutionCoefficientIndex < _ZHCoefficients.Length; ConvolutionCoefficientIndex++ )
				ConvolutionCoefficients[ConvolutionCoefficientIndex] = Math.Sqrt( 4.0 * Math.PI / (2 * ConvolutionCoefficientIndex + 1) ) * _ZHCoefficients[ConvolutionCoefficientIndex];

			// Perform rotation
			for ( int l=0; l < _ZHCoefficients.Length; l++ )
				for ( int m=-l; m <= +l; m++ )
					_RotatedSHCoefficients[l*(l+1)+m] = ComputeSH( l, m, _TargetAxis ) * ConvolutionCoefficients[l];
		}

		/// <summary>
		/// Performs mapping of a set of SH coefficients into N sets of ZH coefficients
		/// WARNING: Takes hell of a time to compute !
		/// </summary>
		/// <param name="_SHCoefficients">The SH coefficients to map into ZH</param>
		/// <param name="_Order">The order of the SH coefficients vector</param>
		/// <param name="_ZHAxes">The double array of resulting ZH axes (i.e. [θ,ϕ] couples)</param>
		/// <param name="_ZHCoefficients">The double array of resulting ZH coefficients (the dimension of the outter array gives the amount of requested ZH lobes while the dimension of the inner arrays should be of size "_Order")</param>
		/// <param name="_HemisphereResolution">The resolution of the hemisphere used to perform initial guess of directions (e.g. using a resolution of 100 will test for 20 * (4*20) possible lobe orientations)</param>
		/// <param name="_BFGSConvergenceTolerance">The convergence tolerance for the BFGS algorithm (the lower the tolerance, the longer it will compute)</param>
		/// <param name="_FunctionSamplingResolution">The resolution of the sphere used to perform function sampling and measuring the error (e.g. using a resolution of 100 will estimate the function with 100 * (2*100) samples)</param>
		/// <param name="_RMS">The resulting array of RMS errors for each ZH lobe</param>
		/// <param name="_Delegate">An optional delegate to pass the method to get feedback about the mapping as it can be a lengthy process (!!)</param>
		public static void		MapSHIntoZH( double[] _SHCoefficients, int _Order, Vector2D[] _ZHAxes, double[][] _ZHCoefficients, int _HemisphereResolution, int _FunctionSamplingResolution, double _BFGSConvergenceTolerance, out double[] _RMS, ZHMappingFeedback _Delegate )
		{
			int			LobesCount = _ZHCoefficients.GetLength( 0 );
			Random		RNG = new Random( 1 );
			double[]	LobeCoefficients = new double[_Order];
			double[]	SHCoefficients = new double[_Order*_Order];
			_SHCoefficients.CopyTo( SHCoefficients, 0 );

			_RMS = new double[LobesCount];

			// Build the local function evaluation context
			ZHMappingLocalFunctionEvaluationContext	ContextLocal = new ZHMappingLocalFunctionEvaluationContext();
													ContextLocal.m_Order = _Order;
													// Placeholders
													ContextLocal.m_ZHCoefficients = new double[_Order];
													ContextLocal.m_RotatedZHCoefficients = new double[_Order*_Order];

			// Pre-compute a table of SH coefficients samples
			ContextLocal.m_SHSamples.Initialize( _Order, _FunctionSamplingResolution );
			ContextLocal.m_Normalizer = 1.0 / ContextLocal.m_SHSamples.SamplesCount;
//			ContextLocal.m_Normalizer = 4.0 * Math.PI / ContextGlobal.m_SHSamples.SamplesCount;
//			ContextLocal.m_Normalizer = 1.0;		// Seems to yield better results with this but it's slow!
			ContextLocal.m_SHEvaluation = new double[ContextLocal.m_SHSamples.SamplesCount];

			// Prepare the hemisphere of random directions for lobe best fit
			Vector2D[]	RandomInitialDirections = new Vector2D[_HemisphereResolution*(4*_HemisphereResolution)];
			int		DirectionIndex = 0;
			for ( int ThetaIndex=0; ThetaIndex < _HemisphereResolution; ThetaIndex++ )
				for ( int PhiIndex=0; PhiIndex < 4*_HemisphereResolution; PhiIndex++ )
					RandomInitialDirections[DirectionIndex++] = new Vector2D( (float) Math.Acos( Math.Sqrt( 1.0 - (ThetaIndex + RNG.NextDouble()) / _HemisphereResolution) ), (float) (2.0 * Math.PI * (PhiIndex + RNG.NextDouble()) / (4*_HemisphereResolution) ) );

			// Prepare the fixed list of coefficients' denominators
			double[]	LobeCoefficientsDenominators = new double[_Order];
			for ( int CoefficientIndex=0; CoefficientIndex < _Order; CoefficientIndex++ )
				LobeCoefficientsDenominators[CoefficientIndex] = 4.0 * Math.PI / (2 * CoefficientIndex + 1);

			// Prepare feedback data
			float	fCurrentProgress = 0.0f;
			float	fProgressDelta = 1.0f / (LobesCount * _HemisphereResolution * (4*_HemisphereResolution));
			int		FeedbackCount = 0;
			int		FeedbackThreshold = (LobesCount * _HemisphereResolution*(4*_HemisphereResolution)) / 100;

			// Compute the best fit for each lobe
			int		CrashesCount = 0;
			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
			{
				// 1] Evaluate goal SH coefficients for every sample directions
				//	(changes on a per-lobe basis as we'll later subtract the ZH coefficients from the goal SH coefficients)
				for ( int SampleIndex=0; SampleIndex < ContextLocal.m_SHSamples.SamplesCount; SampleIndex++ )
				{
					SHSamplesCollection.SHSample	Sample = ContextLocal.m_SHSamples.Samples[SampleIndex];
					ContextLocal.m_SHEvaluation[SampleIndex] = EvaluateSH( SHCoefficients, Sample.m_Theta, Sample.m_Phi, _Order );
				}

				// 2] Evaluate lobe approximation given a set of directions and keep the best fit
				DirectionIndex = 0;
				double	MinError = double.MaxValue;
				for ( int ThetaIndex=0; ThetaIndex < _HemisphereResolution; ThetaIndex++ )
					for ( int PhiIndex=0; PhiIndex < 4*_HemisphereResolution; PhiIndex++ )
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

						// 2.1] Set the lobe direction
						ContextLocal.m_LobeDirection = RandomInitialDirections[DirectionIndex++];

						// 2.2] Find the best approximate initial coefficients given the direction
						double[]	Coefficients = new double[1+_Order];
									Coefficients[1+0] = SHCoefficients[0];	// We use the exact ambient term (rotational invariant)
						for ( int l=1; l < _Order; l++ )
						{
							Coefficients[1+l] = 0.0;
							for ( int m=-l; m <= +l; m++ )
								Coefficients[1+l] += SHCoefficients[l*(l+1)+m] * ComputeSH( l, m, ContextLocal.m_LobeDirection.x, ContextLocal.m_LobeDirection.y );

							Coefficients[1+l] *= LobeCoefficientsDenominators[l];
						}

						//////////////////////////////////////////////////////////////////////////
						// At this point, we have a fixed direction and the best estimated ZH coefficients to map the provided SH in this direction.
						//
						// We then need to apply BFGS minimization to optimize the ZH coefficients yielding the smallest possible error...
						//

						// 2.3] Apply BFGS minimization
						int		IterationsCount = 0;
						double	FunctionMinimum = double.MaxValue;
						try
						{
							dfpmin( Coefficients, _BFGSConvergenceTolerance, out IterationsCount, out FunctionMinimum, new BFGSFunctionEval( ZHMappingLocalFunctionEval ), new BFGSFunctionGradientEval( ZHMappingLocalFunctionGradientEval ), ContextLocal );
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
						_ZHAxes[LobeIndex] = ContextLocal.m_LobeDirection;
						for ( int l=0; l < _Order; l++ )
							_ZHCoefficients[LobeIndex][l] = Coefficients[1+l];

						_RMS[LobeIndex] = FunctionMinimum;
				}

				//////////////////////////////////////////////////////////////////////////
				//
				// At this point, we have the "best" ZH lobe fit for the given set of spherical harmonics coefficients
				//
				// We must subtract the ZH influence from the current Spherical Harmonics, which we simply do by subtracting
				//	the rotated ZH coefficients from the current SH.
				//
				// Then, we are ready to start the process all over again with another lobe, hence fitting the original SH
				//	better and better with every new lobe
				//

				// 3] Rotate the ZH toward the fit axis
				double[]	RotatedZHCoefficients = new double[_Order*_Order];
				ComputeRotatedZHCoefficients( _ZHCoefficients[LobeIndex], SphericalToCartesian( _ZHAxes[LobeIndex].x, _ZHAxes[LobeIndex].y ), RotatedZHCoefficients );

				// 4] Subtract the rotated ZH coefficients to the SH coefficients
				for ( int CoefficientIndex=0; CoefficientIndex < _Order*_Order; CoefficientIndex++ )
					SHCoefficients[CoefficientIndex] -= RotatedZHCoefficients[CoefficientIndex];
			}


			//////////////////////////////////////////////////////////////////////////
			//
			// At this point, we have a set of SH lobes that are individual best fits to the goal SH coefficients
			//
			// We will finally apply a global BFGS minimzation using the total ZH Lobes' axes and coefficients
			//

			// Build the function evaluation context
			ZHMappingGlobalFunctionEvaluationContext	ContextGlobal = new ZHMappingGlobalFunctionEvaluationContext();
														ContextGlobal.m_Order = _Order;
														ContextGlobal.m_LobesCount = LobesCount;
														// Placeholders
														ContextGlobal.m_ZHCoefficients = new double[_Order];
														ContextGlobal.m_RotatedZHCoefficients = new double[_Order*_Order];
														ContextGlobal.m_SumRotatedZHCoefficients = new double[_Order*_Order];

			// Build the array of derivatives deltas
			ContextGlobal.m_DerivativesDelta = new double[1+LobesCount * (2+_Order)];
			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
			{
				ContextGlobal.m_DerivativesDelta[1 + LobeIndex * (2+_Order) + 0] = Math.PI / _FunctionSamplingResolution;				// Dθ
				ContextGlobal.m_DerivativesDelta[1 + LobeIndex * (2+_Order) + 1] = 2.0 * Math.PI / (2.0 * _FunctionSamplingResolution);	// DPhi
				for ( int CoefficientIndex = 0; CoefficientIndex < _Order; CoefficientIndex++ )
					ContextGlobal.m_DerivativesDelta[1 + LobeIndex * (2+_Order) + 2 + CoefficientIndex] = 1e-3;							// Standard deviation of 1e-3 for ZH coefficients
			}

			// Pre-compute a table of SH coefficients samples
			ContextGlobal.m_SHSamples = ContextLocal.m_SHSamples;
			ContextGlobal.m_Normalizer = ContextGlobal.m_SHSamples.SamplesCount;
//			ContextGlobal.m_Normalizer = 4.0 * Math.PI / ContextGlobal.m_SHSamples.SamplesCount;
//			ContextGlobal.m_Normalizer = 1.0;		// Seems to yield better results with this but it's slow!
			ContextGlobal.m_SHEvaluation = ContextLocal.m_SHEvaluation;

			// Compute estimate of the goal SH for every sample direction
			for ( int SampleIndex=0; SampleIndex < ContextLocal.m_SHSamples.SamplesCount; SampleIndex++ )
			{
				SHSamplesCollection.SHSample	Sample = ContextGlobal.m_SHSamples.Samples[SampleIndex];
				ContextGlobal.m_SHEvaluation[SampleIndex] = EvaluateSH( _SHCoefficients, Sample.m_Theta, Sample.m_Phi, _Order );
			}

			// Build the concatenaed set of ZH axes & coefficients
			double[]	CoefficientsGlobal = new double[1+LobesCount*(2+_Order)];
			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
			{
				// Set axes
				CoefficientsGlobal[1+LobeIndex*(2+_Order)+0] = _ZHAxes[LobeIndex].x;
				CoefficientsGlobal[1+LobeIndex*(2+_Order)+1] = _ZHAxes[LobeIndex].y;

				// Set ZH coefficients
				for ( int CoefficientIndex=0; CoefficientIndex < _Order; CoefficientIndex++ )
					CoefficientsGlobal[1+LobeIndex*(2+_Order)+2+CoefficientIndex] = _ZHCoefficients[LobeIndex][CoefficientIndex];
			}

			// Apply BFGS minimzation on the entire set of coefficients
			int		IterationsCountGlobal = 0;
			double	FunctionMinimumGlobal = double.MaxValue;
			try
			{
				dfpmin( CoefficientsGlobal, _BFGSConvergenceTolerance, out IterationsCountGlobal, out FunctionMinimumGlobal, new BFGSFunctionEval( ZHMappingGlobalFunctionEval ), new BFGSFunctionGradientEval( ZHMappingGlobalFunctionGradientEval ), ContextGlobal );
			}
			catch ( Exception )
			{
			}

			// Save the optimized results
			for ( int LobeIndex=0; LobeIndex < LobesCount; LobeIndex++ )
			{
				// Set axes
				_ZHAxes[LobeIndex].x = (float) CoefficientsGlobal[1+LobeIndex*(2+_Order)+0];
				_ZHAxes[LobeIndex].y = (float) CoefficientsGlobal[1+LobeIndex*(2+_Order)+1];

				// Set ZH coefficients
				for ( int CoefficientIndex=0; CoefficientIndex < _Order; CoefficientIndex++ )
					_ZHCoefficients[LobeIndex][CoefficientIndex] = CoefficientsGlobal[1+LobeIndex*(2+_Order)+2+CoefficientIndex];
			}

			// Give final 100% feedback
			if ( _Delegate != null )
				_Delegate( 1.0f );
		}

		#endregion

		#region SH Coefficients & Legendre Polynomials & Windowing Sigma Factors

		// Renormalisation constant for SH functions
		//           .------------------------
		// K(l,m) =  |   (2*l+1)*(l-|m|)!
		//           | --------------------
		//          \| 4*Math.PI * (l+|m|)!
		//
		public static double	K( int l, int m )
		{
			return	Math.Sqrt( ((2.0 * l + 1.0 ) * Factorial( l - Math.Abs(m) )) / (4.0 * Math.PI * Factorial( l + Math.Abs(m) )) );
		}

		// Calculates an Associated Legendre Polynomial P(l,m,x) using stable recurrence relations
		// From Numerical Recipes in C
		//
		public static double	P( int l, int m, double x )
		{
			double	pmm = 1.0;
			if ( m > 0 )
			{	// pmm = (-1) ^ m * Factorial( 2 * m - 1 ) * ( (1 - x) * (1 + x) ) ^ (m/2);
				double	somx2 = Math.Sqrt( (1.0-x) * (1.0+x) );
				double	fact = 1.0;
				for ( int i=1; i <= m; i++ )
				{
					pmm *= -fact * somx2;
					fact += 2;
				}
			}
			if ( l == m )
				return	pmm;

			double	pmmp1 = x * (2.0 * m + 1.0) * pmm;
			if ( l == m+1 )
				return	pmmp1;

			double	pll = 0.0;
			for ( int ll=m+2; ll <= l; ++ll )
			{
				pll = ( (2.0*ll-1.0) * x * pmmp1 - (ll+m-1.0) * pmm ) / (ll-m);
				pmm = pmmp1;
				pmmp1 = pll;
			}

			return	pll;
		}

		/// <summary>
		/// Factored code used to compute U,V,W coefficients needed for rotation matrix inference
		/// </summary>
		/// <param name="_a"></param>
		/// <param name="_b"></param>
		/// <param name="_i"></param>
		/// <param name="_l"></param>
		/// <param name="_Matrix">The source rotation matrix</param>
		/// <returns></returns>
		protected static double	ComputeP( int _a, int _b, int _i, int _l, double[,] _Matrix )
		{
			int	PrevBandOffset = _l * (_l - 1);

			if ( PrevBandOffset + _a < 0 )
				return	0.0;		// Bound excess prevention...

			if ( _b == -_l )
			{
				return	_Matrix[2 + _i,2 + 1] * _Matrix[PrevBandOffset + _a,PrevBandOffset - _l + 1]
					  + _Matrix[2 + _i,2 - 1] * _Matrix[PrevBandOffset + _a,PrevBandOffset + _l - 1];
			}
			else if ( _b == +_l )
			{
				return	_Matrix[2 + _i,2 + 1] * _Matrix[PrevBandOffset + _a,PrevBandOffset + _l - 1]
					  - _Matrix[2 + _i,2 - 1] * _Matrix[PrevBandOffset + _a,PrevBandOffset - _l + 1];
			}
			else
			{
				return	_Matrix[2 + _i,2 + 0] * _Matrix[PrevBandOffset + _a,PrevBandOffset + _b];
			}
		}

		/// <summary>
		/// Sigma factor using a cardinal sine for windowing
		/// From "A Quick Rendering Method Using Basis Functions for Interactive Lighting Design" by Dobashi et al. (1995)
		/// 
		/// Therefore, you should no longer use ComputeSH( l, m ) when reconstructing SH but ComputeSigmaFactorSinc( l ) * ComputeSH( l, m )
		/// </summary>
		/// <param name="l">Current band</param>
		/// <param name="_Order">Max used SH order</param>
		/// <returns>The sigma factor to apply to the SH coefficient to avoid ringing (aka Gibbs phenomenon)</returns>
		public static double	ComputeSigmaFactorSinc( int l, int _Order )
		{
			double	Angle = Math.PI * l / (_Order+1);
			return l > 0 ? Math.Sin( Angle ) / Angle : 1.0;
		}

		/// <summary>
		/// Sigma factor using a cardinal sine for windowing
		/// From "Real-time Soft Shadows in Dynamic Scenes using Spherical Harmonic Exponentiation" by Ren et al. (2006)
		/// 
		/// Therefore, you should no longer use ComputeSH( l, m ) when reconstructing SH but ComputeSigmaFactorCos( l ) * ComputeSH( l, m )
		/// </summary>
		/// <param name="l">Current band</param>
		/// <param name="h">Window size. By default a value of 2*Order works well but some HDR lighting may require greater windowing (i.e. smaller h)</param>
		/// <param name="_Order">Max used SH order</param>
		/// <returns>The sigma factor to apply to the SH coefficient to avoid ringing (aka Gibbs phenomenon)</returns>
		public static double	ComputeSigmaFactorCos( int l, int _Order )	{ return ComputeSigmaFactorCos( l, 2.0 * _Order ); }
		public static double	ComputeSigmaFactorCos( int l, double h )
		{
			return Math.Cos( 0.5 * Math.PI * l / h );
		}

		#endregion

		#region Clebsch-Gordan

		/// From "Addition of Angular Momenta, Clebsch-Gordan Coefficients and the Wigner-Eckart Theorem" (http://string.howard.edu/~tristan/QM2/QM2WE.pdf)
		///
		//  1.18a)	c(j,m, j1,j2,m1,m2) = delta(m,m1+m2) * rho(j, j1,j2) * sigma * tau
		//  1.18b)	delta(m,m1+m2) =  (m==m1+m2) ? 1 : 0
		//  1.18c)	rho = sqrt( (j1+j2-j)! * (j+j1-j2)! * (j2+j-j1)! * (2*j+1) / (j1+j2+j+1)! )
		//  1.18d)	sigma = sqrt( (j+m)!*(j-m)!*(j1+m1)!*(j1-m1)!*(j2+m2)!*(j2-m2)! )
		//  1.18e)	tau = SUM( -1^r / ( (j1-m1-r)! (j2+m2-r)! (j-j2+m1+r)! (j-j1-m2+r)! (j1+j2-j-r)! r! ) )
		// note: 0! = 1; (-n)! = Gamma(1-n) = infinity for n = 1,2,...
		//
		public static double	ComputeClebschGordan( int j1, int j2, int m1, int m2, int j, int m )
		{
			if ( m != m1 + m2 )
				return	0.0;	// No use to bother...
			if ( Math.Abs( m2 ) > j2 )
				return	0.0;	// No use to bother...

			if ( j1 + j2 - j < 0 || j1 + j - j2 < 0 || j2 + j - j1 < 0 )
				return	0.0;	// Doesn't satisfy triangle relation!

			return	ComputeRho( j1, j2, j ) * ComputeSigma( j1, j2, j, m1, m2, m ) * ComputeTau( j1, j2, j, m1, m2 );
		}

		protected static double	ComputeRho( int j1, int j2, int j )
		{
			double	fNum = Factorial( j1+j2-j ) * Factorial( j+j1-j2 ) * Factorial( j2+j-j1 ) * ( 2*j+1 );
			double	fDen = Factorial( j1+j2+j+1 );

			return	Math.Sqrt( fNum / fDen );
		}

		protected static double	ComputeSigma( int j1, int j2, int j, int m1, int m2, int m )
		{
			return	Math.Sqrt( Factorial( j + m ) * Factorial( j - m ) * Factorial( j1 + m1 ) * Factorial( j1 - m1 ) * Factorial( j2 + m2 ) * Factorial( j2 - m2 ) );
		}

		protected static double	ComputeTau( int j1, int j2, int j, int m1, int m2 )
		{
			double	Tau = 0.0;

			// Here, we sum over all values for which none of the factors engaged in 1.18e vanish (that is, factors which are positive)
			int	RMax = 2 * ( Math.Abs( j1 ) + Math.Abs( j1 ) + Math.Abs( j2 ) + Math.Abs( m1 ) + Math.Abs( m2 ) );
			for ( int r=0; r <= RMax; r++ )
			{
				int[]	Factorials = new int[] {	j1 - m1 - r,
													j2 + m2 - r,
													j - j2 + m1 + r,
													j - j1 - m2 + r,
													j1 + j2 - j - r,
													r
											   };

				double	Num = (r & 1) == 0 ? +1 : -1;
				double	Den = 1.0f;

				for ( int FactorialIndex=0; FactorialIndex < Factorials.Length; FactorialIndex++ )
				{
					int	NumberToFactorialize = Factorials[FactorialIndex];
					if ( NumberToFactorialize < 0 )
					{	// Fact( -n ) = +oo so nullify numerator and set denominator to 1 so this factor doesn't get accounted for in the sum...
						Num = 0.0;
						Den = 1.0;
						break;
					}

					Den *= Factorial( NumberToFactorialize );
				}

				Tau += Num / Den;
			}

			return	Tau;
		}

		#endregion

		#region BFGS Minimization

		#region Local Minimization Delegates

		protected static double	ZHMappingLocalFunctionEval( double[] _Coefficients, object _Params )
		{
			ZHMappingLocalFunctionEvaluationContext	ContextLocal = _Params as ZHMappingLocalFunctionEvaluationContext;

			// Rotate current ZH coefficients
			Array.Copy( _Coefficients, 1, ContextLocal.m_ZHCoefficients, 0, ContextLocal.m_Order );
			ComputeRotatedZHCoefficients( ContextLocal.m_ZHCoefficients, SphericalToCartesian( ContextLocal.m_LobeDirection.x, ContextLocal.m_LobeDirection.y ), ContextLocal.m_RotatedZHCoefficients );

			// Sum differences between current ZH estimates and current SH goal estimates
			double	SumSquareDifference = ComputeSummedDifferences( ContextLocal.m_SHSamples, ContextLocal.m_Normalizer, ContextLocal.m_SHEvaluation, ContextLocal.m_RotatedZHCoefficients, ContextLocal.m_Order );

			// Keep the result for gradient eval
			ContextLocal.m_SumSquareDifference = SumSquareDifference;

			return	SumSquareDifference;
		}

		protected static void	ZHMappingLocalFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params )
		{
			ZHMappingLocalFunctionEvaluationContext	ContextLocal = _Params as ZHMappingLocalFunctionEvaluationContext;

			// Compute derivatives for each coefficient
			_Gradients[0] = 0.0;
			for ( int DerivativeIndex=1; DerivativeIndex < _Coefficients.Length; DerivativeIndex++ )
			{
				// Copy coefficients and add them their delta for current derivative
				double[]	Coefficients = new double[_Coefficients.Length];
				_Coefficients.CopyTo( Coefficients, 0 );
				Coefficients[DerivativeIndex] += 1e-3f;

				// Rotate ZH coefficients
				Array.Copy( Coefficients, 1, ContextLocal.m_ZHCoefficients, 0, ContextLocal.m_Order );
				ComputeRotatedZHCoefficients( ContextLocal.m_ZHCoefficients, SphericalToCartesian( ContextLocal.m_LobeDirection.x, ContextLocal.m_LobeDirection.y ), ContextLocal.m_RotatedZHCoefficients );

				// Sum differences between current ZH estimates and current SH goal estimates
				double	SumSquareDifference = ComputeSummedDifferences( ContextLocal.m_SHSamples, ContextLocal.m_Normalizer, ContextLocal.m_SHEvaluation, ContextLocal.m_RotatedZHCoefficients, ContextLocal.m_Order );

				// Compute delta with fixed square difference
				_Gradients[DerivativeIndex] = (SumSquareDifference - ContextLocal.m_SumSquareDifference) / 1e-3;
			}
		}

		#endregion

		#region Global Minimization Delegates

		protected static double	ZHMappingGlobalFunctionEval( double[] _Coefficients, object _Params )
		{
			ZHMappingGlobalFunctionEvaluationContext	ContextGlobal = _Params as ZHMappingGlobalFunctionEvaluationContext;

			// Rotate current ZH coefficients for each lobe
			for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
				ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] = 0.0;
			for ( int LobeIndex=0; LobeIndex < ContextGlobal.m_LobesCount; LobeIndex++ )
			{
				Array.Copy( _Coefficients, 1 + LobeIndex * (2+ContextGlobal.m_Order) + 2, ContextGlobal.m_ZHCoefficients, 0, ContextGlobal.m_Order );
				ComputeRotatedZHCoefficients( ContextGlobal.m_ZHCoefficients, SphericalToCartesian( _Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 0], _Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 1] ), ContextGlobal.m_RotatedZHCoefficients );

				// Accumulate rotated ZH coefficients
				for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
					ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] += ContextGlobal.m_RotatedZHCoefficients[CoefficientIndex];
			}

			// Sum differences between current rotated ZH estimate and SH goal
			double	SumSquareDifference = ComputeSummedDifferences( ContextGlobal.m_SHSamples, ContextGlobal.m_Normalizer, ContextGlobal.m_SHEvaluation, ContextGlobal.m_SumRotatedZHCoefficients, ContextGlobal.m_Order );

			// Keep the result for gradient eval
			ContextGlobal.m_SumSquareDifference = SumSquareDifference;

			return	SumSquareDifference;
		}

		protected static void	ZHMappingGlobalFunctionGradientEval( double[] _Coefficients, double[] _Gradients, object _Params )
		{
			ZHMappingGlobalFunctionEvaluationContext	ContextGlobal = _Params as ZHMappingGlobalFunctionEvaluationContext;

			// Compute derivatives for each coefficient
			_Gradients[0] = 0.0;
			for ( int DerivativeIndex=1; DerivativeIndex < _Coefficients.Length; DerivativeIndex++ )
			{
				// Copy coefficients and add them their delta
				double[]	Coefficients = new double[_Coefficients.Length];
				_Coefficients.CopyTo( Coefficients, 0 );
				Coefficients[DerivativeIndex] += ContextGlobal.m_DerivativesDelta[DerivativeIndex];

				// Rotate current ZH coefficients for each lobe
				for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
					ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] = 0.0;
				for ( int LobeIndex=0; LobeIndex < ContextGlobal.m_LobesCount; LobeIndex++ )
				{
					Array.Copy( Coefficients, 1 + LobeIndex * (2+ContextGlobal.m_Order) + 2, ContextGlobal.m_ZHCoefficients, 0, ContextGlobal.m_Order );
					ComputeRotatedZHCoefficients( ContextGlobal.m_ZHCoefficients, SphericalToCartesian( Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 0], Coefficients[1 + LobeIndex * (2+ContextGlobal.m_Order) + 1] ), ContextGlobal.m_RotatedZHCoefficients );

					// Accumulate rotated ZH coefficients
					for ( int CoefficientIndex=0; CoefficientIndex < ContextGlobal.m_Order * ContextGlobal.m_Order; CoefficientIndex++ )
						ContextGlobal.m_SumRotatedZHCoefficients[CoefficientIndex] += ContextGlobal.m_RotatedZHCoefficients[CoefficientIndex];
				}

				// Sum differences between current ZH estimates and current SH goal estimates
				double	SumSquareDifference = ComputeSummedDifferences( ContextGlobal.m_SHSamples, ContextGlobal.m_Normalizer, ContextGlobal.m_SHEvaluation, ContextGlobal.m_SumRotatedZHCoefficients, ContextGlobal.m_Order );

				// Compute difference with fixed square difference
				_Gradients[DerivativeIndex] = (SumSquareDifference - ContextGlobal.m_SumSquareDifference) / ContextGlobal.m_DerivativesDelta[DerivativeIndex];
			}
		}

		#endregion

		/// <summary>
		/// Computes the square difference between a current SH estimate and a goal SH function given a set of samples
		/// </summary>
		/// <param name="_SamplesCollection">The collection of samples to use for the computation</param>
		/// <param name="_Normalizer">The normalizer for the final result</param>
		/// <param name="_GoalSHEvaluation">The goal SH function's evaluations for each sample</param>
		/// <param name="_SHEstimate">The estimate SH function's coefficients</param>
		/// <param name="_Order">The SH order</param>
		/// <returns>The square difference between goal and estimate</returns>
		protected static double		ComputeSummedDifferences( SHSamplesCollection _SamplesCollection, double _Normalizer, double[] _GoalSHEvaluation, double[] _SHEstimate, int _Order )
		{
			// Sum differences between current ZH estimates and current SH goal estimates
			double	SumSquareDifference = 0.0;
			for ( int SampleIndex=0; SampleIndex < _SamplesCollection.SamplesCount; SampleIndex++ )
			{
				SHSamplesCollection.SHSample	Sample = _SamplesCollection.Samples[SampleIndex];

				double		GoalValue = _GoalSHEvaluation[SampleIndex];

				// Estimate ZH
				double		CurrentValue = 0.0;
				for ( int CoefficientIndex=0; CoefficientIndex < _Order * _Order; CoefficientIndex++ )
					CurrentValue += _SHEstimate[CoefficientIndex] * Sample.m_SHFactors[CoefficientIndex];

				// Sum difference between estimate and goal
				SumSquareDifference += (CurrentValue - GoalValue) * (CurrentValue - GoalValue);
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

		/// <summary>
		/// Converts a cartesian UNIT vector into spherical coordinates (θ,ϕ)
		/// </summary>
		/// <param name="_Direction">The cartesian unit vector to convert</param>
		/// <param name="_θ">The polar elevation</param>
		/// <param name="_ϕ">The azimuth</param>
		public static void		CartesianToSpherical( Vector _Direction, out double _θ, out double _ϕ )
		{
			_θ = Math.Acos( Math.Max( -1.0f, Math.Min( +1.0f, _Direction.z ) ) );
			_ϕ = Math.Atan2( _Direction.y, _Direction.x );
		}

		/// <summary>
		/// Converts spherical coordinates (θ,ϕ) into a cartesian UNIT vector
		/// </summary>
		/// <param name="_θ">The polar elevation</param>
		/// <param name="_ϕ">The azimuth</param>
		/// <returns>The unit vector in cartesian coordinates</returns>
		public static Vector	SphericalToCartesian( double _θ, double _ϕ )
		{
			Vector	Result = new Vector();
			SphericalToCartesian( _θ, _ϕ, Result );
			return	Result;
		}

		/// <summary>
		/// Converts spherical coordinates (θ,ϕ) into a cartesian UNIT vector
		/// </summary>
		/// <param name="_θ">The polar elevation</param>
		/// <param name="_ϕ">The azimuth</param>
		/// <param name="_Direction">The unit vector in cartesian coordinates</param>
		public static void		SphericalToCartesian( double _θ, double _ϕ, WMath.Vector _Direction )
		{
			_Direction.x = (float) (Math.Sin( _θ ) * Math.Cos( _ϕ ));
			_Direction.z = (float) Math.Cos( _θ );
			_Direction.y = (float) (Math.Sin( _θ ) * Math.Sin( _ϕ ));
		}

		/// <summary>
		/// Computes the factorial of the given integer value
		/// </summary>
		/// <param name="_Value">The value to compute the factorial of</param>
		/// <returns>The factorial of the input value</returns>
		protected static double		Factorial( int _Value )
		{
			return	FACTORIAL[_Value];
		}

		#endregion
	}
}
