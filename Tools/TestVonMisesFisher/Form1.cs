using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SharpMath;
using Renderer;

namespace TestVonMisesFisher {
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
	/// This distribution is used in the 2007 paper to approximate sharp specular lobes in a BRDF but my
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
		struct RandomLobe {
			public float	Phi;
			public float	Theta;
			public float	Concentration;
			public int		RandomPointsCount;
		}

		[System.Diagnostics.DebuggerDisplay( "({Phi_deg},{Theta_deg}) k={Concentration} a={Alpha}" )]
		class FitLobe {
			public double	Alpha;
			public float3	Direction;
			public double	Concentration;

			public float	Phi			{ get { return (float) Math.Atan2( Direction.x, Direction.z ); } }
			public float	Theta		{ get { return (float) Math.Acos( Direction.y ); } }
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

		float3[]		m_RandomDirections = null;
		float[]			m_RandomThetas = null;


// 		public float3x3			MakeRot( float3 _from, float3 _to ) {
// 			float3	v = _from.Cross( _to );
// 			float	c = _from.Dot( _to );
// 			float	k = 1.0f / (1.0f + c);
// 
// 			float3x3	R = new float3x3();
// 			R.m[0, 0] = v.x*v.x*k + c;		R.m[0, 1] = v.y*v.x*k - v.z;	R.m[0, 2] = v.z*v.x*k + v.y;
// 			R.m[1, 0] = v.x*v.y*k + v.z;	R.m[1, 1] = v.y*v.y*k + c;		R.m[1, 2] = v.z*v.y*k - v.x;
// 			R.m[2, 0] = v.x*v.z*k - v.y;	R.m[2, 1] = v.y*v.z*k + v.x;	R.m[2, 2] = v.z*v.z*k + c;
// 
// 			return R;
// 		}

		public FittingForm() {
// 			Random		RNG = new Random();
// 			float3		From = new float3( 2.0f * (float) RNG.NextDouble() - 1.0f, 2.0f * (float) RNG.NextDouble() - 1.0f, 2.0f * (float) RNG.NextDouble() - 1.0f ).Normalized;
// 			float3		To = new float3( 2.0f * (float) RNG.NextDouble() - 1.0f, 2.0f * (float) RNG.NextDouble() - 1.0f, 2.0f * (float) RNG.NextDouble() - 1.0f ).Normalized;
// 			float3x3	Pipo = MakeRot( From, To );
// 			float3		Test = Pipo * From;

//TestChromaRanges();
//TestSHRGBEEncoding();
//TestSquareFilling();

			InitializeComponent();

			// Create the random points
			List< float3 >	RandomDirections = new List< float3 >();
			List< float >	RandomThetas = new List< float >();
			for ( int LobeIndex=0; LobeIndex < m_RandomLobes.Length; LobeIndex++ ) {
				float	MainPhi = (float) (m_RandomLobes[LobeIndex].Phi * Math.PI / 180.0f);
				float	MainTheta = (float) (m_RandomLobes[LobeIndex].Theta * Math.PI / 180.0f);
				float	Concentration = m_RandomLobes[LobeIndex].Concentration;
				int		PointsCount = m_RandomLobes[LobeIndex].RandomPointsCount;

				// Build the main direction for the target lobe
				float3		MainDirection = new float3(
					(float) (Math.Sin( MainTheta ) * Math.Sin( MainPhi )),
					(float) (Math.Cos( MainTheta )),
					(float) (Math.Sin( MainTheta ) * Math.Cos( MainPhi ))
					);

				// Build the transform to bring Y-aligned points to the main direction
				float3x3	Rot = new float3x3();
				Rot.BuildRot( float3.UnitY, MainDirection );

				BuildDistributionMapping( Concentration, 0.0 );

				// Draw random points in the Y-aligned hemisphere and transform them into the main direction
				for ( int PointIndex=0; PointIndex < PointsCount; PointIndex++ )
				{
					double	Theta = GetTheta();
					float	CosTheta = (float) Math.Cos( Theta );
//					float	SinTheta = (float) Math.Sqrt( 1.0f - CosTheta*CosTheta );
					float	SinTheta = (float) Math.Sin( Theta );
					float	Phi = (float) (SimpleRNG.GetUniform() * Math.PI);

					float3	RandomDirection = new float3(
						(float) (SinTheta * Math.Sin( Phi )),
						CosTheta,
						(float) (SinTheta * Math.Cos( Phi ))
						);

					float3	FinalDirection = RandomDirection * Rot;

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
			double	x = SimpleRNG.GetUniform();
			int		index = Math.Min( m_UniformRandom2Angle.Length-1, (int) Math.Floor( x * m_UniformRandom2Angle.Length ) );
			double	value = m_UniformRandom2Angle[index];
			return value;
		}

		#endregion

		private void	PerformExpectationMaximization( float3[] _Directions, FitLobe[] _Lobes ) {
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
					float3	dir = _Directions[i];

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
						float3	dir = _Directions[i];
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

		private void panelOutput_BitmapUpdating( int W, int H, Graphics G ) {
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
			for ( int LobeIndex=0; LobeIndex < m_RandomLobes.Length; LobeIndex++ ) {
				float	MainPhi = (float) (m_RandomLobes[LobeIndex].Phi * Math.PI / 180.0f);
				float	MainTheta = (float) (m_RandomLobes[LobeIndex].Theta * Math.PI / 180.0f);
				float	Concentration = m_RandomLobes[LobeIndex].Concentration;
				int		PointsCount = m_RandomLobes[LobeIndex].RandomPointsCount;

				using ( Brush B = new SolidBrush( colors[LobeIndex] ) ) {
//					using ( Brush B2 = new SolidBrush( Color.FromArgb( colors[LobeIndex].R >> 1, colors[LobeIndex].G >> 1, colors[LobeIndex].B >> 1 ) ) ) {
					using ( Brush B2 = new SolidBrush( Color.FromArgb( colors[LobeIndex].R * 2 / 3, colors[LobeIndex].G * 2 / 3, colors[LobeIndex].B * 2 / 3 ) ) ) {

						for ( int i=0; i < PointsCount; i++, PointIndex++ ) {
							float3	Direction = m_RandomDirections[PointIndex];

							float	Px = 0.5f * (1.0f + Direction.x) * W;
							float	Py = 0.5f * (1.0f - Direction.y) * H;
							G.FillRectangle( Direction.z >= 0 ? B : B2, Px-1.0f, Py-1.0f, 2.0f, 2.0f );
						}
					}
				}
			}
		}

		private void panelOutputNormalDistribution_BitmapUpdating( int W, int H, Graphics G ) {
			G.FillRectangle( Brushes.White, 0, 0, W, H );

			int[]	Buckets = new int[128];
			int		Peak = 0;
			for ( int i=0; i < 10000; i++ ) {
				float	Random = (float) SimpleRNG.GetNormal( 0.0, 4.0 );

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

		#region Quick Tests for N.Silvagni (nothing to do with von Mises–Fisher)

		class ranges_t {
			public float	Ygo_min = float.MaxValue, Ygo_max = -float.MaxValue;
			public float	Cg_min = float.MaxValue, Cg_max = -float.MaxValue;
			public float	Co_min = float.MaxValue, Co_max = -float.MaxValue;

			public float	x_min = float.MaxValue, x_max = -float.MaxValue;
			public float	y_min = float.MaxValue, y_max = -float.MaxValue;
			public float	Y_min = float.MaxValue, Y_max = -float.MaxValue;
		}

		void	RGB2YCoCg( float _R, float _G, float _B, out float _Y, out float _Co, out float _Cg ) {
			_Co = _R - _B;
			float	t = _B + _Co * 0.5f;
			_Cg = _G - t;
			_Y = t + _Cg * 0.5f;
		}
		void	YCoCg2RGB( float _Y, float _Co, float _Cg, out float _R, out float _G, out float _B ) {
			float	t = _Y - _Cg * 0.5f;
			_G = _Cg + t;
			_B = t - _Co * 0.5f;
			_R = _Co + _B;
		}

		void	TestChromaRanges() {
			ImageUtility.ColorProfile	profile = new ImageUtility.ColorProfile(ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );

			float3	tempFloat3 = new float3( 0, 0, 0 );
			float4	tempFloat4 = new float4( 0, 0, 0, 1 );

			float	Ygo, Cg, Co;

			ranges_t[]	ranges = new ranges_t[4];
			for ( int lumaIndex=0; lumaIndex < ranges.Length; lumaIndex++ ) {

				ranges_t	range = new ranges_t();
				ranges[lumaIndex] = range;

				float	L = (1+lumaIndex) / 255.0f;

				for ( int R=0; R < 256; R++ ) {
					for ( int G=0; G < 256; G++ ) {
						for ( int B=0; B < 256; B++ ) {

							tempFloat4.x = L * R;
							tempFloat4.y = L * G;
							tempFloat4.z = L * B;

							// Convert to YCoCg
// 							Ygo = 0.25f * tempFloat4.x + 0.5f * tempFloat4.y + 0.25f * tempFloat4.z;
// 							Cg = -0.25f * tempFloat4.x + 0.5f * tempFloat4.y - 0.25f * tempFloat4.z;
// 							Co =  0.50f * tempFloat4.x + 0.0f * tempFloat4.y - 0.50f * tempFloat4.z;

							RGB2YCoCg( tempFloat4.x, tempFloat4.y, tempFloat4.z, out Ygo, out Co, out Cg );
							YCoCg2RGB( Ygo, Co, Cg, out tempFloat3.x, out tempFloat3.y, out tempFloat3.z );
							if ( Math.Abs( tempFloat3.x - tempFloat4.x ) > 1e-6 ) throw new Exception( "RHA!" );
							if ( Math.Abs( tempFloat3.y - tempFloat4.y ) > 1e-6 ) throw new Exception( "RHA!" );
							if ( Math.Abs( tempFloat3.z - tempFloat4.z ) > 1e-6 ) throw new Exception( "RHA!" );

							// Convert to xyY
							float4	XYZ = float4.Zero;
							profile.RGB2XYZ( tempFloat4, ref XYZ );
							tempFloat3.x = XYZ.x;
							tempFloat3.y = XYZ.y;
							tempFloat3.z = XYZ.z;
							float3	xyY = float3.Zero;
							ImageUtility.ColorProfile.XYZ2xyY( tempFloat3, ref xyY );

							// Update ranges
							range.Ygo_min = Math.Min( range.Ygo_min, Ygo );
							range.Ygo_max = Math.Max( range.Ygo_max, Ygo );
							range.Cg_min = Math.Min( range.Cg_min, Cg );
							range.Cg_max = Math.Max( range.Cg_max, Cg );
							range.Co_min = Math.Min( range.Co_min, Co );
							range.Co_max = Math.Max( range.Co_max, Co );

							range.Y_min = Math.Min( range.Y_min, xyY.z );
							range.Y_max = Math.Max( range.Y_max, xyY.z );
							range.x_min = Math.Min( range.x_min, xyY.x );
							range.x_max = Math.Max( range.x_max, xyY.x );
							range.y_min = Math.Min( range.y_min, xyY.y );
							range.y_max = Math.Max( range.y_max, xyY.y );
						}
					}
				}

			}
		}

		#endregion

		#region Quick Color Encoding/Decoding Tests (nothing to do with von Mises–Fisher)

// 		uint packR8G8B8A8( float4 value ) {
// 			value = saturate( value );
// 			return ( ( ( uint( value.x * 255.0 ) ) << 24 ) | ( ( uint( value.y * 255.0 ) ) << 16 ) | ( ( uint( value.z * 255.0 ) ) << 8 ) | ( uint( value.w * 255.0 ) ) );
// 		}
// 
// 		float4 unpackR8G8B8A8( uint value ) {
// 			return float4( ( value >> 24 ) & 0xFF, ( value >> 16 ) & 0xFF, ( value >> 8 ) & 0xFF, value & 0xFF ) / 255.0;
// 		}
// 
// 		// RGBE (ward 1984)
// 		uint packRGBE( float3 value ) {  
// 			const float sharedExp = ceil( ApproxLog2( max( max( value.r, value.g ), value.b ) ) );
// 			return packR8G8B8A8( saturate( float4( value / ApproxExp2( sharedExp ), ( sharedExp + 128.0f ) / 255.0f ) ) );
// 		}
// 
// 		float3 unpackRGBE( uint value ) {
// 			const float4 rgbe = unpackR8G8B8A8( value );
// 			return rgbe.rgb * ApproxExp2( rgbe.a * 255.0f - 128.0f );
// 		}

		uint	EncodeRGBE( float3 _RGB ) {
			float3	Sign = new float3( Math.Sign( _RGB.x ), Math.Sign( _RGB.y ), Math.Sign( _RGB.z ) );
			_RGB *= Sign;

			float	maxRGB = _RGB.Max();
			int		sharedExp = (int) Math.Ceiling( Math.Log( maxRGB ) / Math.Log(2) );

sharedExp = Math.Max( 0, Math.Min( 31, sharedExp + EXPONENT_BIAS ) ) - EXPONENT_BIAS;


// 			float	pow2 = (float) Math.Pow( 2.0f, sharedExp );
// 			float3	reducedRGB = _RGB / pow2;
			float	invPow2 = (float) Math.Pow( 2.0f, -sharedExp );
			float3	reducedRGB = _RGB * invPow2;
					reducedRGB.x = Math.Max( 0.0f, Math.Min( 1.0f, reducedRGB.x ) );
					reducedRGB.y = Math.Max( 0.0f, Math.Min( 1.0f, reducedRGB.y ) );
					reducedRGB.z = Math.Max( 0.0f, Math.Min( 1.0f, reducedRGB.z ) );

			byte	R = (byte) (255.0f * reducedRGB.x);
			byte	G = (byte) (255.0f * reducedRGB.y);
			byte	B = (byte) (255.0f * reducedRGB.z);
			byte	exp = (byte) (EXPONENT_BIAS + sharedExp);
					exp |= (byte) ((Sign.x < 0.0f ? 128 : 0) | (Sign.y < 0.0f ? 64 : 0) | (Sign.z < 0.0f ? 32 : 0));

			uint	RGBE = (uint) ((exp << 24) | (R << 16) | (G << 8) | B);
			return RGBE;
		}

		float3	DecodeRGBE( uint _RGBE ) {
			byte	exp = (byte) (_RGBE >> 24);
			byte	R = (byte) ((_RGBE >> 16) & 0xFF);
			byte	G = (byte) ((_RGBE >> 8) & 0xFF);
			byte	B = (byte) (_RGBE & 0xFF);

			float3	Sign = new float3( (exp & 128) != 0 ? -1.0f : 1.0f, (exp & 64) != 0 ? -1.0f : 1.0f, (exp & 32) != 0 ? -1.0f : 1.0f );
			exp &= 31;
			float	pow2 = (float) Math.Pow( 2.0f, (int) exp - EXPONENT_BIAS );

			float3	resultRGB = new float3( 
				Sign.x * pow2 * R / 255.0f,
				Sign.y * pow2 * G / 255.0f,
				Sign.z * pow2 * B / 255.0f
				);
			return resultRGB;
		}

		void TestSHRGBEEncoding() {
			float3[]	coeffs = null;

			System.IO.FileInfo	coeffsFileName = new System.IO.FileInfo( "SHCoeffs.sh3" );
			using ( System.IO.FileStream S = coeffsFileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
					uint	coeffsCount = R.ReadUInt32();
					coeffs = new float3[coeffsCount * 9];
					for ( int i=0; i < 9*coeffsCount; i++ ) {
						coeffs[i] = new float3( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );

// The exponent bias allows us to support up to 512 in luminance!
//coeffs[i] *= 5.0f;

					}
				}

			uint	test1_packed = EncodeRGBE( new float3( 1, 0, 1.5f ) );
			float3	test1_unpacked = DecodeRGBE( test1_packed );

//			float3	coeffMin = new float3( float.MaxValue, float.MaxValue, float.MaxValue );
			float3	coeffMax = new float3( -float.MaxValue, -float.MaxValue, -float.MaxValue );
			float3	coeffMinAbs = new float3( float.MaxValue, float.MaxValue, float.MaxValue );
			int		coeffsWithDifferentSignsInRGBCount = 0;
			for ( int i=0; i < coeffs.Length; i++ ) {
				float3	coeff = coeffs[i];
				float3	absCoeff = new float3( Math.Abs( coeff.x ), Math.Abs( coeff.y ), Math.Abs( coeff.z ) );

				if ( coeff.x * coeff.y < 0.0f || coeff.x * coeff.z < 0.0f || coeff.y * coeff.z < 0.0f )
					coeffsWithDifferentSignsInRGBCount++;

//				coeffMin.Min( coeff );
				coeffMax.Max( absCoeff );
				if ( absCoeff.x > 0.0f ) coeffMinAbs.x = Math.Min( coeffMinAbs.x, absCoeff.x );
				if ( absCoeff.y > 0.0f ) coeffMinAbs.y = Math.Min( coeffMinAbs.y, absCoeff.y );
				if ( absCoeff.z > 0.0f ) coeffMinAbs.z = Math.Min( coeffMinAbs.z, absCoeff.z );
			}

			double	expMin = Math.Min( Math.Min( Math.Log( coeffMinAbs.x ) / Math.Log(2), Math.Log( coeffMinAbs.y ) / Math.Log(2) ), Math.Log( coeffMinAbs.z ) / Math.Log(2) );
			double	expMax = Math.Max( Math.Max( Math.Log( coeffMax.x ) / Math.Log(2), Math.Log( coeffMax.y ) / Math.Log(2) ), Math.Log( coeffMax.z ) / Math.Log(2) );

			// Measure discrepancies after RGBE encoding
// 			float3	errorAbsMin = new float3( +float.MaxValue, +float.MaxValue, +float.MaxValue );
// 			float3	errorAbsMax = new float3( -float.MaxValue, -float.MaxValue, -float.MaxValue );
			float3	errorRelMin = new float3( +float.MaxValue, +float.MaxValue, +float.MaxValue );
			float3	errorRelMax = new float3( -float.MaxValue, -float.MaxValue, -float.MaxValue );
			int		minExponent = +int.MaxValue, maxExponent = -int.MaxValue;
			int		largeRelativeErrorsCount = 0;
			for ( int i=0; i < coeffs.Length; i++ ) {
				float3	originalRGB = coeffs[i];
				uint	RGBE = EncodeRGBE( originalRGB );
				float3	decodedRGB = DecodeRGBE( RGBE );

				// Compute absolute error
// 				float3	delta = decodedRGB - originalRGB;
// 				float3	distanceFromOriginal = new float3( Math.Abs( delta.x ), Math.Abs( delta.y ), Math.Abs( delta.z ) );
// 				errorAbsMin.Min( distanceFromOriginal );
// 				errorAbsMax.Max( distanceFromOriginal );

				// Compute relative error
				float3	errorRel = new float3( Math.Abs( originalRGB.x ) > 0.0f ? Math.Abs( decodedRGB.x / originalRGB.x - 1.0f ) : 0.0f, Math.Abs( originalRGB.y ) > 0.0f ? Math.Abs( decodedRGB.y / originalRGB.y - 1.0f ) : 0.0f, Math.Abs( originalRGB.z ) > 0.0f ? Math.Abs( decodedRGB.z / originalRGB.z - 1.0f ) : 0.0f );

				// Scale the relative error by the magnitude of each component as compared to the maximum component
				// This way, if we happen to have a "large" relative error on a component that is super small compared to the component with maximum amplitude then we can safely drop that small component (it's insignificant compared to the largest contribution)
				float	maxComponent = Math.Max( Math.Max( Math.Abs( originalRGB.x ), Math.Abs( originalRGB.y ) ), Math.Abs( originalRGB.z ) );
				float3	magnitudeScale = maxComponent > 0.0f ? new float3( Math.Abs( originalRGB.x ) / maxComponent, Math.Abs( originalRGB.y ) / maxComponent, Math.Abs( originalRGB.z ) / maxComponent ) : float3.Zero;
				errorRel *= magnitudeScale;

				// Don't account for dernomalization
// 				if ( decodedRGB.x == 0.0 && originalRGB.x != 0.0f ) errorRel.x = 0.0f;
// 				if ( decodedRGB.y == 0.0 && originalRGB.y != 0.0f ) errorRel.y = 0.0f;
// 				if ( decodedRGB.z == 0.0 && originalRGB.z != 0.0f ) errorRel.z = 0.0f;

				const float	errorThreshold = 0.2f;
				if ( Math.Abs( errorRel.x ) > errorThreshold || Math.Abs( errorRel.y ) > errorThreshold || Math.Abs( errorRel.z ) > errorThreshold )
					largeRelativeErrorsCount++;
				errorRelMin.Min( errorRel );
				errorRelMax.Max( errorRel );

				int		exp = (int) ((RGBE >> 24) & 31) - EXPONENT_BIAS;
				minExponent = Math.Min( minExponent, exp );
				maxExponent = Math.Max( maxExponent, exp );
			}
		}

		const int	EXPONENT_BIAS = 22;

		#endregion

		#region Quick Test Mapping a 1D Counter into a 2D position along a Square Spiral (nothing to do with von Mises–Fisher either)

		void	TestSquareFilling() {

			for ( int N=0; N < 6; N++ ) {

				Console.WriteLine( "STARTING NEW LANE FOR N=" + N );

				int	L = Math.Max( 1, 8 * N );
				int	maxSize = 1+2*N;

				for ( int i=0; i < L; i++ ) {
					int	iX = (i + N) % L;
					int	iY = (i + L-N) % L;

					int	X = Math.Max( 0, Math.Min( maxSize-1, 3*N - Math.Abs( 4*N - iX ) ) ) - N;
					int	Y = Math.Max( 0, Math.Min( maxSize-1, 3*N - Math.Abs( 4*N - iY ) ) ) - N;

					Console.WriteLine( "i=" + i + " X=" + X + " Y=" + Y );
				}
			}
//					Console.WriteLine( "i=" + i + " X=" + X + " Y=" + Y + "  - Xc=" + Math.Max( 0, Math.Min( maxSize-1, X ) ) + "  - Yc=" + Math.Max( 0, Math.Min( maxSize-1, Y ) ) );

// 			int	HalfSize = whatever;
// 			int	S = 1 + 2 * HalfSize;		// <= Square size
// 			int	Count = S * S;
// 			for ( int i=0; i < Count; i++ ) {
// 				int	N = (int) Math.Floor( (Math.Sqrt( i ) + 1) / 2 );	// Lane index
// 				int	L = Math.Max( 1, 8 * N );							// Amount of pixels in the lane
// 				int	LaneSize = 1+2*N;									// Size of the lane
// 				int	LaneStartPixelIndex = N*N;
// 				int	j = i - LaneStartPixelIndex;						// Pixel index relative to the lane
// 				int	iX = (j + N) % L;
// 				int	iY = (j + L-N) % L;
// 				int	X = Math.Max( 0, Math.Min( LaneSize-1, 3*N - Math.Abs( 4*N - iX ) ) ) - N;
// 				int	Y = Math.Max( 0, Math.Min( LaneSize-1, 3*N - Math.Abs( 4*N - iY ) ) ) - N;
// 				// Plot pixel at (X,Y)
// 			}
		}

		#endregion
	}
}
