using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WMath;

namespace TestVonMisesFisher
{
	/// <summary>
	/// The purpose of this test is to try the Spherical Expectation Maximization (EM) algorithm
	///  from the paper "Frequency Domain Normal Map Filtering", Han (2007)
	///  (http://www.cs.columbia.edu/cg/normalmap/normalmap.pdf)
	/// 
	/// The idea is to categorize a number of random points placed on a sphere into a finite set of gaussian lobes
	///  for which the pdf is f(x) = k / (4PI sinh( k )) * exp( k * dot( µ, x ) ) for any x on the sphere
	/// 
	///		µ is the "mean direction" of the lobe
	///		k (kappa) is the "concentration parameter" or variance of the lobe
	/// 
	/// The pdf is called the "von Mises–Fisher distribution"
	/// (more details: http://en.wikipedia.org/wiki/Von_Mises%E2%80%93Fisher_distribution)
	/// (also see the Kent distribution for elliptical distribution: http://en.wikipedia.org/wiki/Kent_distribution)
	/// 
	/// -------------------------------------------------------------------------------------------------
	/// 
	/// This distribution is used in the 2007 paper to appoximate sharp specular lobes in a BRDF but my
	///  intention is to simulate distant, high-frequency specular light sources from a set of sampling
	///  points in the environment surrounding a location.
	/// 
	/// The EM algorithm is better described in "Clustering on the Unit Hypersphere using von Mises-Fisher Distributions"
	/// Banerjee (2005) found here: http://www.cs.utexas.edu/users/inderjit/public_papers/movmf_jmlr.pdf
	/// 
	/// The algorithm works as a loop split into two parts:
	/// 
	///  Expectation:
	///   for all N points x in the set of directions,
	///		for all considered lobes with parameters (a,µ,k) (a is the weight),
	///			compute f(x) = k / (4PI sinh( k )) * exp( k * dot( µ, x ) )
	///     
	///		for all considered lobes,
	///		    compute p(x) = a * f(x) / sum( a * f(x) )	<-- expected likelihood that x is in the considered lobe >
	/// 
	///	 Maximization:
	///	  for all considered lobes,
	///			compute a = 1/N * sum( p(x_i) )
	///			compute µ = 1/N * sum( x_i * p(x_i) )	<-- not normalized >
	///			compute r = |µ| / (N * a)
	///			compute µ = µ / |µ|						<-- now normalized >
	///			compute k = (3*r - r^3) / (1 - r^2)
	/// 
	/// Both parts are repeated until convergence.
	/// There's no clear indication how convergence is measured so I simply computed a relative progress
	///  from one iteration to the next and stopped iterating when the progress became insignificant.
	/// 
	/// </summary>
	public partial class FittingForm : Form
	{
		const int		FITTING_LOBES_COUNT = 4;

		[System.Diagnostics.DebuggerDisplay( "({Phi},{Theta}) k={Concentration}" )]
		struct RandomLobe
		{
			public float	Phi;
			public float	Theta;
			public float	Concentration;
			public int		RandomPointsCount;
		}

		[System.Diagnostics.DebuggerDisplay( "({Phi_deg},{Theta_deg}) k={Concentration} a={Alpha}" )]
		class FitLobe {
			public double	Alpha;
			public Vector	Direction;
			public double	Concentration;

			public float	Phi		{ get { return (float) Math.Atan2( Direction.x, Direction.z ); } }
			public float	Theta	{ get { return (float) Math.Acos( Direction.y ); } }
			public float	Phi_deg		{ get { return (float) (180.0 * Math.Atan2( Direction.x, Direction.z ) / Math.PI); } }
			public float	Theta_deg	{ get { return (float) (180.0 * Math.Acos( Direction.y ) / Math.PI); } }
		}

		RandomLobe[]	m_RandomLobes = new RandomLobe[] {
//			new RandomLobe() { Phi = 30.0f, Theta = 45.0f, Concentration = 100.0f, RandomPointsCount = 4000 },

			new RandomLobe() { Phi = 0.0f, Theta = 0.0f, Concentration = 0.1f, RandomPointsCount = 2000 },
 			new RandomLobe() { Phi = 90.0f, Theta = 45.0f, Concentration = 4.0f, RandomPointsCount = 4000 },
 			new RandomLobe() { Phi = 160.0f, Theta = 120.0f, Concentration = 20.0f, RandomPointsCount = 500 },
			new RandomLobe() { Phi = -60.0f, Theta = 70.0f, Concentration = 100.0f, RandomPointsCount = 1000 },

// 			new RandomLobe() { Phi = 0.0f, Theta = 0.0f, Concentration = 0.1f, RandomPointsCount = 2000 },
//   			new RandomLobe() { Phi = 90.0f, Theta = 45.0f, Concentration = 4.0f, RandomPointsCount = 2000 },
//   			new RandomLobe() { Phi = 160.0f, Theta = 120.0f, Concentration = 20.0f, RandomPointsCount = 2000 },
//  			new RandomLobe() { Phi = -60.0f, Theta = 70.0f, Concentration = 100.0f, RandomPointsCount = 2000 },
		};

		Vector[]		m_RandomDirections = null;
		float[]			m_RandomThetas = null;


		public FittingForm()
		{
			InitializeComponent();

			// Create the random points
			List< Vector >	RandomDirections = new List< Vector >();
			List< float >	RandomThetas = new List< float >();
			for ( int LobeIndex=0; LobeIndex < m_RandomLobes.Length; LobeIndex++ )
			{
				float	MainPhi = (float) (m_RandomLobes[LobeIndex].Phi * Math.PI / 180.0f);
				float	MainTheta = (float) (m_RandomLobes[LobeIndex].Theta * Math.PI / 180.0f);
				float	Concentration = m_RandomLobes[LobeIndex].Concentration;
				int		PointsCount = m_RandomLobes[LobeIndex].RandomPointsCount;

				// Build the main direction for the target lobe
				Vector		MainDirection = new Vector(
					(float) (Math.Sin( MainTheta ) * Math.Sin( MainPhi )),
					(float) (Math.Cos( MainTheta )),
					(float) (Math.Sin( MainTheta ) * Math.Cos( MainPhi ))
					);

				// Build the transform to bring Y-aligned points to the main direction
				Matrix3x3	Rot = new Matrix3x3();
				Rot.MakeRot( Vector.UnitY, MainDirection );

				BuildDistributionMapping( Concentration, 0.0 );

				// Draw random points in the Y-aligned hemisphere and transform them into the main direction
				for ( int PointIndex=0; PointIndex < PointsCount; PointIndex++ )
				{
					double	Theta = GetTheta();
					float	CosTheta = (float) Math.Cos( Theta );
//					float	SinTheta = (float) Math.Sqrt( 1.0f - CosTheta*CosTheta );
					float	SinTheta = (float) Math.Sin( Theta );
					float	Phi = (float) (WMath.SimpleRNG.GetUniform() * Math.PI);

					Vector	RandomDirection = new Vector(
						(float) (SinTheta * Math.Sin( Phi )),
						CosTheta,
						(float) (SinTheta * Math.Cos( Phi ))
						);

					Vector	FinalDirection = RandomDirection * Rot;

					RandomDirections.Add( FinalDirection );

					RandomThetas.Add( CosTheta );
				}
			}

			m_RandomDirections = RandomDirections.ToArray();
			m_RandomThetas = RandomThetas.ToArray();

			panelOutput.UpdateBitmap();
			panelOutputNormalDistribution.UpdateBitmap();

			// Do it!
			FitLobe[]	Result = new FitLobe[FITTING_LOBES_COUNT];
			for ( int h=0; h < Result.Length; h++ )
				Result[h] = new FitLobe();
			PerformExpectationMaximization( m_RandomDirections, Result );
		}

		#region Bessel

		private static double[]	FACTORIAL = new double[] {	1.0,
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
															2.63130836933693530e+35		// 32!
														};

		/// <summary>
		/// Computes I0 (from http://mathworld.wolfram.com/ModifiedBesselFunctionoftheFirstKind.html)
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		private double	ModifiedBesselI0( double z ) {
			double	result = 1.0;
			for ( int k=1; k < 32; k++ ) {
				double	term = Math.Pow( 0.25 * z * z, k ) / Math.Pow( FACTORIAL[k], 2.0 );
				result += term;
			}
			return result;
		}

		/// <summary>
		/// Computes In (from http://mathworld.wolfram.com/ModifiedBesselFunctionoftheFirstKind.html)
		/// using integral form (5)</summary>
		/// <param name="z"></param>
		/// <returns></returns>
		private double ModifiedBesselI( int n, double z ) {
			const int		COUNT = 100;
			const double	dTheta = Math.PI / COUNT;
			double	result = 0.0;
			double	theta = 0.0;
			double	px = Math.Exp( z );
			for ( int i=1; i <= COUNT; i++ ) {
				theta += dTheta;
				double	x = Math.Exp( z * Math.Cos( theta ) ) * Math.Cos( n * theta );
				result += 0.5f * (x + px);
				px = x;
			}
			result *= dTheta / Math.PI;
			return result;
		}

		#endregion

		#region CDF

		private double	Phi( double x, double kappa, double mu ) {
			double	result = x;
			double	I0 = 2.0 / ModifiedBesselI0( kappa );
			for ( int j=1; j < 10; j++ ) {
				double	term = ModifiedBesselI( j, kappa ) * Math.Sin( j * (x - mu) ) / j;
				result += term;
			}

			result /= 2.0 * Math.PI;
			return result;
		}

		private double	CDF( double x, double kappa ) {
			double	Phi0 = Phi( -Math.PI, kappa, 0.0 );
			double	PhiX = Phi( x, kappa, 0.0 );
			return PhiX - Phi0;
		}

		#endregion

		#region Uniform -> von Mises Mapping

		private double[]	m_UniformRandom2Angle = new double[2048];
		private void	BuildDistributionMapping( double kappa, double mu ) {
			int		N = m_UniformRandom2Angle.Length;
			double	dTheta = 2.0 * Math.PI / N;
			double	sum = 0.0;
			double	theta = -Math.PI;

			// Standard von Mises distribution
//			double	I0 = ModifiedBesselI0( kappa );
//			double	Norm = dTheta / (2.0 * Math.PI * I0);

			// von Mises - Fisher distribution
			double	I0_5 = kappa / (2.0 * Math.PI * (Math.Exp( kappa ) - Math.Exp( -kappa )));
			double	Norm = dTheta * I0_5;
					Norm *= Math.PI;		// Since theta € [-PI,+PI] and covers half, dPhi = PI

			for ( int i=0; i < N; i++ ) {
				double	pdf = Math.Exp( kappa * Math.Cos( theta - mu ) );

pdf *= Math.Abs( Math.Sin( theta ) );	// Spherical coordinates integrand is sin(theta).dTheta.dPhi

				double	prevSum = sum;
				sum += pdf * Norm;
				theta += dTheta;

				int	x0 = (int) Math.Floor( prevSum * N );
				int	x1 = Math.Min( N-1, (int) Math.Ceiling( sum * N ) );
				for ( int x=x0; x < x1; x++ ) {
					m_UniformRandom2Angle[x] = theta;
				}
			}
		}

		private double	GetTheta() {
			double	x = WMath.SimpleRNG.GetUniform();
			int		index = Math.Min( m_UniformRandom2Angle.Length-1, (int) Math.Floor( x * m_UniformRandom2Angle.Length ) );
			double	value = m_UniformRandom2Angle[index];
			return value;
		}

		#endregion

		private void	PerformExpectationMaximization( Vector[] _Directions, FitLobe[] _Lobes ) {
			int			n = _Directions.Length;
			double		invN = 1.0 / n;
			int			k = _Lobes.Length;
			double[,]	probabilities = new double[n,k];

			// 1] Initialize lobes
			for ( int h=0; h < k; h++ ) {
				_Lobes[h].Direction = _Directions[(int) ((h+0.5f) * n / k)];
				_Lobes[h].Concentration = 0.5;
				_Lobes[h].Alpha = 1.0 / k;
			}

			// 2] Iterate
			int	iterationsCount = 0;
			while ( ++iterationsCount < 1000 ) {
				// 2.1] Compute Expectation (the E step of the algorithm)
				for ( int i=0; i < n; i++ ) {
					Vector	dir = _Directions[i];

					// 2.1.1) Compute weighted probability for each direction to belong to each lobe
					double	weightedSumProbabilities = 0.0;
					for ( int h=0; h < k; h++ ) {
						FitLobe	Lobe = _Lobes[h];

						double	kappa = Lobe.Concentration;
						double	dot = dir.Dot( Lobe.Direction );
						double	f = kappa / (2.0 * Math.PI * (Math.Exp( kappa ) - Math.Exp( -kappa ))) * Math.Exp( kappa * dot );
								f *= Lobe.Alpha;
						probabilities[i,h] = f;
						weightedSumProbabilities += f;
					}
					// 2.1.2) Normalize
					double	normalizer = weightedSumProbabilities > 1e-12 ? 1.0 / weightedSumProbabilities : 0.0;
					for ( int h=0; h < k; h++ ) {
						probabilities[i,h] *= normalizer;
					}
				}

				// 2.2] Compute Maximization (the M step of the algorithm)
				double	sqConvergenceRate = 0.0;
				for ( int h=0; h < k; h++ ) {
					FitLobe	Lobe = _Lobes[h];

					// Accumulate new alpha and average direction
					Lobe.Alpha = 0.0;
					double	mu_x = 0.0;
					double	mu_y = 0.0;
					double	mu_z = 0.0;
					for ( int i=0; i < n; i++ ) {
						Vector	dir = _Directions[i];
						double	p = probabilities[i,h];
						double	p_over_N = invN * p;

						Lobe.Alpha += p_over_N;

						mu_x += p_over_N * dir.x;
						mu_y += p_over_N * dir.y;
						mu_z += p_over_N * dir.z;
					}

					// Compute new direction
					double	mu_length = Math.Sqrt( mu_x*mu_x + mu_y*mu_y + mu_z*mu_z );
					double	r = Lobe.Alpha > 1e-12 ? mu_length / Lobe.Alpha : 0.0;

					// Normalize direction
					mu_length = mu_length > 1e-12 ? 1.0 / mu_length : 0.0;
					Lobe.Direction.x = (float) (mu_length * mu_x);
					Lobe.Direction.y = (float) (mu_length * mu_y);
					Lobe.Direction.z = (float) (mu_length * mu_z);

					// Compute new concentration
					double	oldConcentration = Lobe.Concentration;
					double	newConcentration = (3.0 * r - r*r*r) / (1.0 - r*r);
					Lobe.Concentration = newConcentration;

					sqConvergenceRate += (newConcentration - oldConcentration) * (newConcentration - oldConcentration);
				}
				sqConvergenceRate /= k * k;

				if ( sqConvergenceRate < 1e-6 )
					break;
			}
		}

		private void panelOutput_BitmapUpdating( int W, int H, Graphics G )
		{
			G.FillRectangle( Brushes.White, 0, 0, W, H );

// Test Bessel functions
// 			float	s = 0.05f;
// 
// 			double	pz = 0.0;
// 			double	py = ModifiedBesselI0( 0.0 );
// 			for ( int x=1; x < W; x++ ) {
// 				double	z = x * 6.0 / W;
// 				double	y = ModifiedBesselI0( z );
// 
// 				G.DrawLine( Pens.Black, x-1, (H-1) * (float) (1.0-s*py), x, (H-1) * (float) (1.0-s*y) );
// 
// 				pz = z;
// 				py = y;
// 			}
// 
// 			Color[]	colors = new Color[] {
// 				Color.FromArgb( 255, 0, 0 ),
// 				Color.FromArgb( 255, 128, 0 ),
// 				Color.FromArgb( 128, 255, 0 ),
// 				Color.FromArgb( 0, 0, 255 ),
// 			};
// 
// 			for ( int i=0; i < 4; i++ ) {
// 				using ( Pen P = new Pen( colors[i], 1.0f ) ) {
// 					pz = 0.0;
// 					py = ModifiedBesselI( i, 0.0 );
// 					for ( int x=1; x < W; x++ ) {
// 						double	z = x * 6.0 / W;
// 						double	y = ModifiedBesselI( i, z );
// 
// 						G.DrawLine( P, x-1, (H-1) * (float) (1.0-s*py), x, (H-1) * (float) (1.0-s*y) );
// 
// 						pz = z;
// 						py = y;
// 					}
// 				}
// 			}


// 			// Test mapping
// 			Color[]	colors = new Color[] {
// 				Color.FromArgb( 0, 0, 0 ),
// 				Color.FromArgb( 255, 0, 0 ),
// 				Color.FromArgb( 255, 128, 0 ),
// 				Color.FromArgb( 128, 255, 0 ),
// 				Color.FromArgb( 0, 0, 255 ),
// 			};
// 
// 			for ( int i=0; i < colors.Length; i++ ) {
// 				using ( Pen P = new Pen( colors[i], 1.0f ) ) {
// 					double	kappa = i > 0 ? Math.Pow( 2.0, -2.0 + i ) : 0.0;
// 
// 					BuildDistributionMapping( kappa, 0.0 );
// 
// 					double	py = 0.5 * (1.0 + m_UniformRandom2Angle[0] / Math.PI);
// 					for ( int x=1; x < W; x++ ) {
// 						double	y = 0.5 * (1.0 + m_UniformRandom2Angle[m_UniformRandom2Angle.Length*x/W] / Math.PI);
// 
// 						G.DrawLine( P, x-1, (H-1) * (float) (1.0-py), x, (H-1) * (float) (1.0-y) );
// 
// 						py = y;
// 					}
// 				}
// 			}


			// Test distribution
			Color[]	colors = new Color[] {
				Color.FromArgb( 255, 0, 0 ),
				Color.FromArgb( 255, 128, 0 ),
				Color.FromArgb( 128, 255, 0 ),
				Color.FromArgb( 0, 0, 255 ),
			};

			G.DrawEllipse( Pens.Black, 0, 0, W, H );

			int	PointIndex = 0;
			for ( int LobeIndex=0; LobeIndex < m_RandomLobes.Length; LobeIndex++ )
			{
				float	MainPhi = (float) (m_RandomLobes[LobeIndex].Phi * Math.PI / 180.0f);
				float	MainTheta = (float) (m_RandomLobes[LobeIndex].Theta * Math.PI / 180.0f);
				float	Concentration = m_RandomLobes[LobeIndex].Concentration;
				int		PointsCount = m_RandomLobes[LobeIndex].RandomPointsCount;

				using ( Brush B = new SolidBrush( colors[LobeIndex] ) ) {
//					using ( Brush B2 = new SolidBrush( Color.FromArgb( colors[LobeIndex].R >> 1, colors[LobeIndex].G >> 1, colors[LobeIndex].B >> 1 ) ) ) {
					using ( Brush B2 = new SolidBrush( Color.FromArgb( colors[LobeIndex].R * 2 / 3, colors[LobeIndex].G * 2 / 3, colors[LobeIndex].B * 2 / 3 ) ) ) {

						for ( int i=0; i < PointsCount; i++, PointIndex++ ) {
							Vector	Direction = m_RandomDirections[PointIndex];

							float	Px = 0.5f * (1.0f + Direction.x) * W;
							float	Py = 0.5f * (1.0f - Direction.y) * H;
							G.FillRectangle( Direction.z >= 0 ? B : B2, Px-1.0f, Py-1.0f, 2.0f, 2.0f );
						}
					}
				}
			}
		}

		private void panelOutputNormalDistribution_BitmapUpdating( int W, int H, Graphics G )
		{
			G.FillRectangle( Brushes.White, 0, 0, W, H );

			int[]	Buckets = new int[128];
			int		Peak = 0;
			for ( int i=0; i < 10000; i++ ) {

				float	Random = (float) WMath.SimpleRNG.GetNormal( 0.0, 4.0 );

				int		BucketIndex = (int) (64 * (1.0f + 0.05f * Random));
				BucketIndex = Math.Max( 0, Math.Min( 127, BucketIndex ) );
				Buckets[BucketIndex]++;
				Peak = Math.Max( Peak, Buckets[BucketIndex] );
			}

			for ( int i=0; i < 128; i++ ) {
				float	h = H * Buckets[i] / (1.1f * Peak);
				G.FillRectangle( Brushes.Black, W * (i+0) / 128.0f, H - h - 1, W/128.0f, h );
			}
		}
	}
}
