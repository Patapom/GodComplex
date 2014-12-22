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
		const int		LOBES_COUNT = 3;

		struct RandomLobe
		{
			public float	Phi;
			public float	Theta;
			public float	Variance;
			public int		RandomPointsCount;
		}

		RandomLobe[]	m_RandomLobes = new RandomLobe[] {
			new RandomLobe() { Phi = 0.0f, Theta = 0.0f, Variance = 0.1f, RandomPointsCount = 200 },
			new RandomLobe() { Phi = 90.0f, Theta = 30.0f, Variance = 4.0f, RandomPointsCount = 400 },
			new RandomLobe() { Phi = 160.0f, Theta = 60.0f, Variance = 50.0f, RandomPointsCount = 50 },
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
				float	Variance = m_RandomLobes[LobeIndex].Variance;
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

				// Draw random points in the Y-aligned hemisphere and transform them into the main direction
				for ( int PointIndex=0; PointIndex < PointsCount; PointIndex++ )
				{
					float	CosTheta = (float) WMath.SimpleRNG.GetNormal( 0.0f, Variance );
					float	SinTheta = (float) Math.Sqrt( 1.0f - CosTheta*CosTheta );
					float	Phi = (float) (WMath.SimpleRNG.GetUniform() * 2.0 * Math.PI);

					Vector	RandomDirection = new Vector(
						(float) (SinTheta * Math.Sin( MainPhi )),
						CosTheta,
						(float) (SinTheta * Math.Cos( MainPhi ))
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


			// Test CDF
			Color[]	colors = new Color[] {
				Color.FromArgb( 0, 0, 0 ),
				Color.FromArgb( 255, 0, 0 ),
				Color.FromArgb( 255, 128, 0 ),
				Color.FromArgb( 128, 255, 0 ),
				Color.FromArgb( 0, 0, 255 ),
			};

			for ( int i=0; i < colors.Length; i++ ) {
				using ( Pen P = new Pen( colors[i], 1.0f ) ) {
					double	kappa = i > 0 ? Math.Pow( 2.0, -2.0 + i ) : 0.0;
					double	pz = 0.0;
					double	py = CDF( -Math.PI, kappa );
					for ( int x=1; x < W; x++ ) {
						double	z = Math.PI * (2.0 * x - 1.0) / W;
						double	y = CDF( z, kappa );

						G.DrawLine( P, x-1, (H-1) * (float) (1.0-py), x, (H-1) * (float) (1.0-y) );

						pz = z;
						py = y;
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
