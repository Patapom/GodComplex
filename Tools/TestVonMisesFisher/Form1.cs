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


		public FittingForm()
		{
			InitializeComponent();

			// Create the random points
			List< Vector >	RandomDirections = new List< Vector >();
			for ( int LobeIndex=0; LobeIndex < m_RandomLobes.Length; LobeIndex++ )
			{
				float	MainPhi = m_RandomLobes[LobeIndex].Phi;
				float	MainTheta = m_RandomLobes[LobeIndex].Theta;
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
					float	SinTheta = (float) Math.Sqrt( 1 - CosTheta*CosTheta );
					float	Phi = (float) (WMath.SimpleRNG.GetUniform() * 2.0 * Math.PI);

					Vector	RandomDirection = new Vector(
						(float) (SinTheta * Math.Sin( MainPhi )),
						CosTheta,
						(float) (SinTheta * Math.Cos( MainPhi ))
						);

					Vector	FinalDirection = RandomDirection * Rot;

					RandomDirections.Add( FinalDirection );
				}
			}

			m_RandomDirections = RandomDirections.ToArray();
		}

		private void panelOutput_BitmapUpdating( int W, int H, Graphics G )
		{
			G.FillRectangle( Brushes.White, 0, 0, W, H );


		}
	}
}
