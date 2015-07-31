using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace TestBoxFitting
{
	public partial class Form1 : Form
	{
		public const int		PIXELS_COUNT = 4*128;

		[System.Diagnostics.DebuggerDisplay( "d={Distance} N=[{Normal.x}, {Normal.y}]" )]
		public struct Pixel {
			public float	Distance;
			public float2	Position;
			public float2	Normal;
		}

		public Pixel[]		m_Pixels = new Pixel[PIXELS_COUNT];
		public float2		m_boxCenter;

		#region von Mises Fisher (EM Algorithm)

		[System.Diagnostics.DebuggerDisplay( "({Phi_deg},{Theta_deg}) k={Concentration} a={Alpha}" )]
		public class FitLobe {
			public double	Alpha;
			public float3	Direction;
			public double	Concentration;

			public float	Phi			{ get { return (float) Math.Atan2( Direction.x, Direction.z ); } }
			public float	Theta		{ get { return (float) Math.Acos( Direction.y ); } }
			public float	Phi_deg		{ get { return (float) (180.0 * Math.Atan2( Direction.x, Direction.z ) / Math.PI); } }
			public float	Theta_deg	{ get { return (float) (180.0 * Math.Acos( Direction.y ) / Math.PI); } }
		}

		/// <summary>
		/// Given a set of directions and an array of resulting lobes, the algorithm performs multiple Expectation/Maximization iterations
		///  to arrive at the most significant lobe distributions
		/// </summary>
		/// <param name="_Directions"></param>
		/// <param name="_Lobes"></param>
		/// <param name="_MaxIterations"></param>
		/// <param name="_ConvergenceThreshold">Default should be 1e-6</param>
		private void	PerformExpectationMaximization( float3[] _Directions, FitLobe[] _Lobes, int _MaxIterations, double _ConvergenceThreshold ) {
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
			while ( ++iterationsCount < _MaxIterations ) {
				// 2.1] Compute Expectation (the E step of the algorithm)
				for ( int i=0; i < n; i++ ) {
					float3	dir = _Directions[i];

					// 2.1.1) Compute weighted probability for each direction to belong to each lobe
					double	weightedSumProbabilities = 0.0;
					for ( int h=0; h < k; h++ ) {
						FitLobe	Lobe = _Lobes[h];

						double	kappa = Math.Min( Lobe.Concentration, 700.0 );	// Larger than 700 and we exceed the precision limit of doubles

						double	dot = dir.Dot( Lobe.Direction );
						double	f = kappa / (2.0 * Math.PI * (Math.Exp( kappa ) - Math.Exp( -kappa ))) * Math.Exp( kappa * dot );
								f *= Lobe.Alpha;
						probabilities[i,h] = f;
						if ( double.IsNaN( probabilities[i,h] ) )
							throw new Exception( "Rha!" );
						weightedSumProbabilities += f;
					}
					// 2.1.2) Normalize
					double	normalizer = weightedSumProbabilities > 1e-12 ? 1.0 / weightedSumProbabilities : 0.0;
					for ( int h=0; h < k; h++ ) {
						probabilities[i,h] *= normalizer;
						if ( double.IsNaN( probabilities[i,h] ) )
							throw new Exception( "Rha!" );
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

				if ( sqConvergenceRate < _ConvergenceThreshold )
					break;
			}
		}

		#endregion

		public FitLobe[]	m_Lobes = null;

		public struct Plane {
			public float2	m_Position;
			public float2	m_Normal;
		}
		public Plane[]		m_Planes = null;

		public struct Obstacle {
			public float2	m_Position;
			public float2	m_Orientation;
			public float2	m_Scale;
		}
		public List< Obstacle >	m_ObstaclesRound = new List< Obstacle >();
		public List< Obstacle >	m_ObstaclesSquare = new List< Obstacle >();

		public Form1()
		{
			InitializeComponent();
			panelOutput.m_Owner = this;
			panelHistogram.m_Owner = this;

			// Build a random polygonal room
			Random	RNG = new Random( 3 );

			const float		MAX_DISTANCE = 10.0f;

			int			planesCount = (int) (4 + 2 * RNG.NextDouble());

planesCount = 5;

			float2[]	planePositions = new float2[planesCount];
			float2[]	planeNormals = new float2[planesCount];

			m_boxCenter = float2.Zero;//new float2( (float) (-10.0 + 20.0 * RNG.NextDouble()), (float) (-10.0 + 20.0 * RNG.NextDouble()) );
			float		baseAngle = (float) (RNG.NextDouble() * 2.0 * Math.PI);
			float		averageAngle = (float) (2.0 * Math.PI / planesCount);
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {

				float2	normal = new float2( (float) Math.Cos( baseAngle ), (float) Math.Sin( baseAngle ) );
				planeNormals[planeIndex] = normal;

				float2	toCenter = m_boxCenter;
				float	distance2Center = toCenter.Dot( normal );
				float	planeDistance = distance2Center > 0.0f ? distance2Center + 0.1f + (float) (Math.Max( 0.0f, MAX_DISTANCE-distance2Center ) * RNG.NextDouble()) : (float) (MAX_DISTANCE * RNG.NextDouble());

				float2	position = m_boxCenter - planeDistance * normal;
				planePositions[planeIndex] = position;

				baseAngle += (float) (averageAngle * (0.9 + 0.2 * RNG.NextDouble()));
			}

			// Build the original room pixels
			for ( int i=0; i < PIXELS_COUNT; i++ ) {
				float2	C = m_boxCenter;
				float	angle = (float) (2.0 * Math.PI * i / PIXELS_COUNT);
				float2	V = new float2( (float) Math.Cos( angle ), (float) Math.Sin( angle ) );

				// Compute intersection with the planes
				float	minHitDistance = float.MaxValue;
				int		hitPlaneIndex = -1;
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					float2	D = C - planePositions[planeIndex];
					float	hitDistance = -D.Dot( planeNormals[planeIndex] ) / V.Dot( planeNormals[planeIndex] );
					if ( hitDistance > 0.0f && hitDistance < minHitDistance ) {
						minHitDistance = hitDistance;
						hitPlaneIndex = planeIndex;
					}
				}
				if ( hitPlaneIndex == -1 ) {
					m_Pixels[i].Distance = float.MaxValue;
					continue;
				}

				float	noiseDistance = 0.2f * (2.0f * (float) RNG.NextDouble() - 1.0f);
				m_Pixels[i].Distance = minHitDistance + noiseDistance;
				m_Pixels[i].Position = m_boxCenter + m_Pixels[i].Distance * V;

				float2	normal = planeNormals[hitPlaneIndex];
				float	normalAngle = (float) Math.Atan2( normal.y, normal.x );
				float	noiseAngle = 0.1f * (2.0f * (float) RNG.NextDouble() - 1.0f);
				normal = new float2( (float) Math.Cos( normalAngle + noiseAngle ), (float) Math.Sin( normalAngle + noiseAngle ) );

				m_Pixels[i].Normal = normal;
			}

// 			// Add some obstacles
// 			const int	OBSTACLES_COUNT = 40;
// 			const float	MAX_OBSTACLE_DISTANCE = 10.0f;
// 
// 			for ( int i=0; i < OBSTACLES_COUNT; i++ ) {
// //				float2	pos = m_boxCenter + MAX_OBSTACLE_DISTANCE * new float2( (float) (2.0 * RNG.NextDouble() - 1.0), (float) (2.0 * RNG.NextDouble() - 1.0) );
// 
// 				float	d = MAX_OBSTACLE_DISTANCE * (float) (0.3 + 0.7 * RNG.NextDouble());
// 				float	a = (float) (2.0 * Math.PI * RNG.NextDouble());
// 				float2	pos = m_boxCenter + d * new float2( (float) Math.Cos( a ), (float) Math.Sin( a ) );
// 				float2	dir = new float2( (float) (2.0 * RNG.NextDouble() - 1.0), (float) (2.0 * RNG.NextDouble() - 1.0) ).Normalized;
// 				float2	radius = 1.0f * new float2( (float) (0.1 + 0.9 * RNG.NextDouble()), (float) (0.1 + 0.9 * RNG.NextDouble()) );
// 
// 				Obstacle	O = new Obstacle() {
// 					m_Position = pos,
// 					m_Orientation = dir,
// 					m_Scale = radius
// 				};
// 
// 				switch ( RNG.Next( 2 ) ) {
// 					case 0: AddObstacleRound( pos, dir, radius ); m_ObstaclesRound.Add( O ); break;
// 					case 1: AddObstacleSquare( pos, dir, radius ); m_ObstaclesSquare.Add( O ); break;
// 				}
// 			}
// 

			//////////////////////////////////////////////////////////////////////////
			// Use EM to obtain principal directions
			List<float3>	directions = new List<float3>( PIXELS_COUNT );
			for ( int i=0; i < PIXELS_COUNT; i++ ) {
				if ( m_Pixels[i].Distance < 1e3f )
					directions.Add( new float3( m_Pixels[i].Normal.x, m_Pixels[i].Normal.y, 0.0f ) );
			}

			planesCount = 4;

			m_Lobes = new FitLobe[planesCount];
			for ( int i=0; i < planesCount; i++ )
				m_Lobes[i] = new FitLobe();
			PerformExpectationMaximization( directions.ToArray(), m_Lobes, 1000, 1e-6 );


			//////////////////////////////////////////////////////////////////////////
			// Place planes at the best positions
			string	text = planesCount + " Planes:\r\n";

			m_Planes = new Plane[planesCount];
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				Plane	P = new Plane();
				P.m_Normal = new float2( m_Lobes[planeIndex].Direction.x, m_Lobes[planeIndex].Direction.y );

				float2	sumPositions = float2.Zero;
				float	sumWeights = 0.0f;
				for ( int i=0; i < PIXELS_COUNT; i++ ) {
					float	dot = Math.Max( 0.0f, P.m_Normal.Dot( m_Pixels[i].Normal ) );
							dot = (float) Math.Pow( dot, 10.0 );
					float	weight = dot;
					sumPositions += weight * m_Pixels[i].Position;
					sumWeights += weight;
				}

				P.m_Position = sumPositions / sumWeights;
				m_Planes[planeIndex] = P;

// 				// Harmonic mean
// 				float	sumWeights = 0.0f;
// 				for ( int i=0; i < PIXELS_COUNT; i++ ) {
// 					float	dot = Math.Max( 0.0f, P.m_Normal.Dot( m_Pixels[i].Normal ) );
// //							dot = (float) Math.Pow( dot, 10.0 );
// 					float	weight = dot;
// 					sumWeights += 1.0f / Math.Max( 1e-4f, weight * m_Pixels[i].Distance );
// 				}
// 				float	averageDistance = PIXELS_COUNT / sumWeights;
// 
// 				P.m_Position = m_boxCenter - averageDistance * P.m_Normal;
// 				m_Planes[planeIndex] = P;

				text += "{ " + m_Lobes[planeIndex].Direction.x.ToString( "G4" ) + ", " + m_Lobes[planeIndex].Direction.y.ToString( "G4" ) + ", " + m_Lobes[planeIndex].Direction.z.ToString( "G4" ) + "}  -  k=" + m_Lobes[planeIndex].Concentration.ToString( "G4" ) + "  -  weight = " + m_Lobes[planeIndex].Alpha.ToString( "G4" ) + "\r\n";
			}

			textBoxPlanes.Text = text;


			// Refresh
			panelOutput.UpdateBitmap();
			panelHistogram.UpdateBitmap();
		}

		void	AddObstacleRound( float2 _P, float2 _D, float2 _radius ) {
			float2	X = new float2( _D.x / _radius.x, _D.y / _radius.x );
			float2	Y = new float2( -_D.y / _radius.y, _D.x / _radius.y );

			float2	C = m_boxCenter - _P;
			C = new float2( C.Dot( X ), C.Dot( Y ) );

			for ( int i=0; i < PIXELS_COUNT; i++ ) {
				float	angle = (float) (2.0 * Math.PI * i / PIXELS_COUNT);
				float2	wsV = new float2( (float) Math.Cos( angle ), (float) Math.Sin( angle ) );
				float2	V = new float2( wsV.Dot( X ), wsV.Dot( Y ) );

				// Compute the intersection with the unit circle centered in 0
				float	c = C.Dot( C ) - 1.0f;
				float	b = C.Dot( V );
				float	a = V.Dot( V );
				float	Delta = b*b - a*c;
				if ( Delta < 0.0f )
					continue;

				Delta = (float) Math.Sqrt( Delta );
				float	t0 = (-b - Delta) / a;
				float	t1 = (-b + Delta) / a;
				float	t = Math.Min( t0, t1 );
				if ( t < 0.0f || t > m_Pixels[i].Distance )
					continue;

				m_Pixels[i].Distance = t;
				m_Pixels[i].Position = m_boxCenter + t * wsV;
			}
		}

		void	AddObstacleSquare( float2 _P, float2 _D, float2 _radius ) {
			float2	X = new float2( _D.x / _radius.x, _D.y / _radius.x );
			float2	Y = new float2( -_D.y / _radius.y, _D.x / _radius.y );

			float2	C = m_boxCenter - _P;
			C = new float2( C.Dot( X ), C.Dot( Y ) );

			for ( int i=0; i < PIXELS_COUNT; i++ ) {
				float	angle = (float) (2.0 * Math.PI * i / PIXELS_COUNT);
				float2	wsV = new float2( (float) Math.Cos( angle ), (float) Math.Sin( angle ) );
				float2	V = new float2( wsV.Dot( X ), wsV.Dot( Y ) );

				// Compute the intersection with the unit box centered in 0
				float	c = C.Dot( C ) - 1.0f;
				float	b = C.Dot( V );
				float	a = V.Dot( V );
				float	Delta = b*b - a*c;
				if ( Delta < 0.0f )
					continue;

				Delta = (float) Math.Sqrt( Delta );
				float	t0 = (-b - Delta) / a;
				float	t1 = (-b + Delta) / a;
				float	t = Math.Min( t0, t1 );
				if ( t < 0.0f || t > m_Pixels[i].Distance )
					continue;

				m_Pixels[i].Distance = t;
				m_Pixels[i].Position = m_boxCenter + t * wsV;
			}
		}
	}
}
