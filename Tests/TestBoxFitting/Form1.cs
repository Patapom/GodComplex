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
		double[,]	probabilities;
		private void	PerformExpectationMaximization( float3[] _Directions, FitLobe[] _Lobes, int _MaxIterations, double _ConvergenceThreshold ) {
			int			n = _Directions.Length;
			double		invN = 1.0 / n;
			int			k = _Lobes.Length;

			probabilities = new double[n,k];

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
							r = Math.Min( 1.0-1e-12, r );	// Avoid invalid, negative concentrations

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

		[System.Diagnostics.DebuggerDisplay( "Normal=[{m_Normal.x}, {m_Normal.y}] - D = {m_OrthoDistance} - Dimissed={m_Dismissed}" )]
		public struct Plane {
			public float	m_OrthoDistance;
			public float2	m_Position;
			public float2	m_Normal;
			public float	m_Weight;
			public bool		m_Dismissed;
			public string	m_DismissalReason;
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

			BuildRoom();
		}

		[System.Diagnostics.DebuggerDisplay( "weight={weight} - Index={planeIndex}" )]
		struct SortedPlane : IComparable<SortedPlane> {
			public float	weight;
			public int		planeIndex;

			#region IComparable<SortedPlane> Members

			public int CompareTo( SortedPlane other ) {
				return -Comparer<float>.Default.Compare( weight, other.weight );
			}

			#endregion
		}

		#region Room Building

		// Build a random polygonal room
		void	BuildRoom() {

			const float		MAX_DISTANCE = 20.0f;

			Random	RNG = new Random( integerTrackbarControlRandomSeed.Value );

//			int			planesCount = (int) (4 + 2 * RNG.NextDouble());
			int			planesCount = integerTrackbarControlRoomPlanesCount.Value;

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
					m_Pixels[i].Distance = 40.0f;
					continue;
				}
				minHitDistance = Math.Min( 40.0f, minHitDistance );

				float	noiseDistance = 0.2f * (2.0f * (float) RNG.NextDouble() - 1.0f);
				m_Pixels[i].Distance = minHitDistance + noiseDistance;
				m_Pixels[i].Position = m_boxCenter + m_Pixels[i].Distance * V;

				float2	normal = planeNormals[hitPlaneIndex];
				float	normalAngle = (float) Math.Atan2( normal.y, normal.x );
				float	noiseAngle = 0.1f * (2.0f * (float) RNG.NextDouble() - 1.0f);
				normal = new float2( (float) Math.Cos( normalAngle + noiseAngle ), (float) Math.Sin( normalAngle + noiseAngle ) );

				m_Pixels[i].Normal = normal;
			}

			// Add some obstacles
			int	OBSTACLES_COUNT = integerTrackbarControlObstacles.Value;
			const float	MAX_OBSTACLE_DISTANCE = 15.0f;

			m_ObstaclesRound.Clear();
			m_ObstaclesSquare.Clear();
			for ( int i=0; i < OBSTACLES_COUNT; i++ ) {
//				float2	pos = m_boxCenter + MAX_OBSTACLE_DISTANCE * new float2( (float) (2.0 * RNG.NextDouble() - 1.0), (float) (2.0 * RNG.NextDouble() - 1.0) );

				float	d = MAX_OBSTACLE_DISTANCE * (float) (0.3 + 0.7 * RNG.NextDouble());
				float	a = (float) (2.0 * Math.PI * RNG.NextDouble());
				float2	pos = m_boxCenter + d * new float2( (float) Math.Cos( a ), (float) Math.Sin( a ) );
				float2	dir = new float2( (float) (2.0 * RNG.NextDouble() - 1.0), (float) (2.0 * RNG.NextDouble() - 1.0) ).Normalized;
				float2	radius = 1.0f * new float2( (float) (0.1 + 0.9 * RNG.NextDouble()), (float) (0.1 + 0.9 * RNG.NextDouble()) );

				Obstacle	O = new Obstacle() {
					m_Position = pos,
					m_Orientation = dir,
					m_Scale = radius
				};

				double	k = RNG.NextDouble();
				if ( k < 0.7 ) {
					AddObstacleRound( pos, dir, radius ); m_ObstaclesRound.Add( O );
				} else {
					AddObstacleSquare( pos, dir, radius ); m_ObstaclesSquare.Add( O );
				}
			}

			SolveRoom();
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

				float2	I = C + t * V;	// Hit in local space, also the normal
				float2	wsN = (I.x * _radius.x * _radius.x * X + I.y * _radius.y * _radius.y * Y).Normalized;

				m_Pixels[i].Distance = t;
				m_Pixels[i].Position = m_boxCenter + t * wsV;
				m_Pixels[i].Normal = wsN;
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
				float	t0 = (C.x + Math.Sign( V.x )) / -V.x;
				float2	I0 = C + t0 * V;
				float2	N0 = new float2( -Math.Sign( V.x ), 0.0f );
				if ( Math.Abs( I0.y ) > 1.0f )
					t0 = float.MaxValue;

				float	t1 = (C.y + Math.Sign( V.y )) / -V.y;
				float2	I1 = C + t1 * V;
				float2	N1 = new float2( 0.0f, -Math.Sign( V.y ) );
				if ( Math.Abs( I1.x ) > 1.0f )
					t1 = float.MaxValue;

				float	t;	// Math.Min( t0, t1 )
				float2	N;
				if ( t0 < t1 ) {
					t = t0;
					N = N0;
				} else {
					t = t1;
					N = N1;
				}
				if ( t < 0.0f || t > m_Pixels[i].Distance )
					continue;

				float2	wsN = (N.x * _radius.x * _radius.x * X + N.y * _radius.y * _radius.y * Y).Normalized;

				m_Pixels[i].Distance = t;
				m_Pixels[i].Position = m_boxCenter + t * wsV;
				m_Pixels[i].Normal = wsN;
			}
		}

		#endregion

		// Solves the best planes for the room
		void SolveRoom() {

			//////////////////////////////////////////////////////////////////////////
			// Use EM to obtain principal directions
			List<float3>	directions = new List<float3>( PIXELS_COUNT );
			for ( int i=0; i < PIXELS_COUNT; i++ ) {
//				if ( m_Pixels[i].Distance < 1e3f )
					directions.Add( new float3( m_Pixels[i].Normal.x, m_Pixels[i].Normal.y, 0.0f ) );
			}

			int	planesCount = integerTrackbarControlResultPlanesCount.Value;
			m_Planes = new Plane[planesCount];
			m_Lobes = new FitLobe[planesCount];
			for ( int i=0; i < planesCount; i++ )
				m_Lobes[i] = new FitLobe();
			PerformExpectationMaximization( directions.ToArray(), m_Lobes, 1000, 1e-6 );

			for ( int i=0; i < planesCount; i++ )
				m_Planes[i].m_Weight = (float) m_Lobes[i].Alpha;


			//////////////////////////////////////////////////////////////////////////
			// Remove similar planes
			for ( int i=0; i < planesCount-1; i++ ) {
				FitLobe	P0 = m_Lobes[i];
				if ( m_Planes[i].m_Dismissed )
					continue;	// Already dismissed...

				float3	averageDirection = (float) P0.Alpha * P0.Direction;
				double	maxKappa = P0.Concentration;
				for ( int j=i+1; j < planesCount; j++ ) {
					FitLobe	P1 = m_Lobes[j];
					if ( m_Planes[j].m_Dismissed )
						continue;	// Already dismissed...

					float	dot = P0.Direction.Dot( P1.Direction );
					if ( dot < floatTrackbarControlSimilarPlanes.Value )
						continue;

					averageDirection += (float) P1.Alpha * P1.Direction;
					maxKappa = Math.Max( maxKappa, P1.Concentration );

					m_Planes[j].m_Dismissed = true;	// Dismiss
					m_Planes[j].m_DismissalReason += " SIMILAR" + i;
				}
				P0.Direction = averageDirection.Normalized;
				P0.Concentration = maxKappa;
			}


			//////////////////////////////////////////////////////////////////////////
			// Place planes at the best positions
			float	maxOrthoDistance = 0.0f;
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				float2	Normal = new float2( m_Lobes[planeIndex].Direction.x, m_Lobes[planeIndex].Direction.y );
				m_Planes[planeIndex].m_Normal = Normal;

				float	sumOrthoDistances0 = 0.0f;
				float	sumWeights0 = 0.0f;
				float	sumOrthoDistances1 = 0.0f;
				float	sumWeights1 = 0.0f;
				for ( int i=0; i < PIXELS_COUNT; i++ ) {
					float	orthoDistance = -(m_Pixels[i].Position - m_boxCenter).Dot( Normal );

					// Use computed probabilities
					float	weight = (float) probabilities[i,planeIndex];
					sumOrthoDistances0 += weight * orthoDistance;
					sumWeights0 += weight;

					// Use weighted sum
					float	dot = Math.Max( 0.0f, Normal.Dot( m_Pixels[i].Normal ) );
							dot = (float) Math.Pow( dot, floatTrackbarControlWeightExponent.Value );
					weight = dot;
					sumOrthoDistances1 += weight * orthoDistance;
					sumWeights1 += weight;
				}
				float	averageOrthoDistance0 = sumOrthoDistances0 / sumWeights0;
				float	averageOrthoDistance1 = sumOrthoDistances1 / sumWeights1;

				// Choose whichever ortho distance is best
				float	finalOrthoDistance;
				if ( radioButtonBest.Checked )
					finalOrthoDistance = Math.Max( averageOrthoDistance0, averageOrthoDistance1 );
				else 
					finalOrthoDistance = radioButtonProbabilities.Checked ? averageOrthoDistance0 : averageOrthoDistance1;

				maxOrthoDistance = Math.Max( maxOrthoDistance, finalOrthoDistance );

				m_Planes[planeIndex].m_OrthoDistance = finalOrthoDistance;
				m_Planes[planeIndex].m_Position = m_boxCenter - finalOrthoDistance * Normal;

				if ( finalOrthoDistance <= 0.0f ) {
					m_Planes[planeIndex].m_Dismissed = true;
					m_Planes[planeIndex].m_DismissalReason += " BEHIND";
				}
			}


			//////////////////////////////////////////////////////////////////////////
			// Reconstruct weight
			if ( radioButtonNormalAffinity.Checked ) {
				// Reconstruct weights by normal affinity
				// We do the same operation as the normal weighting to compute ortho distances: dot each pixel with each plane normal and use that as a weight
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					float2	Normal = new float2( m_Lobes[planeIndex].Direction.x, m_Lobes[planeIndex].Direction.y );

					// Use weighted sum
					float	sumWeights = 0.0f;
					for ( int i=0; i < PIXELS_COUNT; i++ ) {
						float	weight = Math.Max( 0.0f, Normal.Dot( m_Pixels[i].Normal ) );
								weight = (float) Math.Pow( weight, floatTrackbarControlWeightExponent.Value );
						sumWeights += weight;
					}
					float	finalWeight = sumWeights / PIXELS_COUNT;
					m_Lobes[planeIndex].Alpha = finalWeight;
					m_Planes[planeIndex].m_Weight = finalWeight;
				}
			} else if ( radioButtonLargestD.Checked ) {
				// Choose largest ortho distances
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					float	weight = Math.Max( 0.0f, m_Planes[planeIndex].m_OrthoDistance / maxOrthoDistance );
					m_Lobes[planeIndex].Alpha = weight;
					m_Planes[planeIndex].m_Weight = weight;
				}
			} else if ( radioButtonWeightHybrid.Checked ) {
				// Hybrid method combining ortho distance and normal affinity
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					float2	Normal = new float2( m_Lobes[planeIndex].Direction.x, m_Lobes[planeIndex].Direction.y );

					// Use weighted sum
					float	sumWeights = 0.0f;
					for ( int i=0; i < PIXELS_COUNT; i++ ) {
						float	weight = Math.Max( 0.0f, Normal.Dot( m_Pixels[i].Normal ) );
								weight = (float) Math.Pow( weight, floatTrackbarControlWeightExponent.Value );
						sumWeights += weight;
					}
					float	finalWeight = sumWeights / PIXELS_COUNT;
							finalWeight *= Math.Max( 0.0f, m_Planes[planeIndex].m_OrthoDistance / maxOrthoDistance );

					m_Lobes[planeIndex].Alpha = finalWeight;
					m_Planes[planeIndex].m_Weight = finalWeight;
				}
			}


			//////////////////////////////////////////////////////////////////////////
			// Dismiss unimportant planes
			float	averageWeight = 0.0f, harmonicAverageWeight = 0.0f, averageConcentration = 0.0f, harmonicAverageConcentration = 0.0f;
			float	maxWeight = 0.0f, maxConcentration = 0.0f;
			float	minWeight = float.MaxValue, minConcentration = float.MaxValue;
			int		validPlanesCount = 0;

			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				if ( m_Planes[planeIndex].m_Dismissed )
					continue;

				float	weight = (float) m_Lobes[planeIndex].Alpha;
				minWeight = Math.Min( minWeight, weight );
				maxWeight = Math.Max( maxWeight, weight );
				averageWeight += weight;
				harmonicAverageWeight += 1.0f / Math.Max( 1e-4f, weight );

				float	concentration = (float) m_Lobes[planeIndex].Concentration;
				minConcentration = Math.Min( minConcentration, concentration );
				maxConcentration = Math.Max( maxConcentration, concentration );
				averageConcentration += concentration;
				harmonicAverageConcentration += 1.0f / Math.Max( 1e-4f, concentration );

				validPlanesCount++;
			}

			validPlanesCount = Math.Max( 1, validPlanesCount );
			averageWeight /= validPlanesCount;
			harmonicAverageWeight = validPlanesCount / harmonicAverageWeight;
			averageConcentration /= validPlanesCount;
			harmonicAverageConcentration = validPlanesCount / harmonicAverageConcentration;
//			float	dismissWeight = 0.5f / validPlanesCount;
			float	dismissWeight = floatTrackbarControlDismissFactor.Value * harmonicAverageWeight;
			float	dismissConcentration = floatTrackbarControlDismissFactor.Value * harmonicAverageConcentration;

			// Select the N best planes/Dismiss others
			if ( !radioButtonUseBest.Checked )
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					bool	dismissed = radioButtonDismissKappa.Checked ? m_Lobes[planeIndex].Concentration < dismissConcentration : m_Lobes[planeIndex].Alpha < dismissWeight;
					if ( dismissed ) {
						m_Planes[planeIndex].m_Dismissed = true;
						m_Planes[planeIndex].m_DismissalReason += " REJECTED";
					}
				}

			// Dismiss planes that are totally clipped by others
			DismissClippedPlanes();

			// Dismiss planes above target count
			List<SortedPlane>	SortedPlanes = new List<SortedPlane>( planesCount );
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				SortedPlanes.Add( new SortedPlane() { planeIndex = planeIndex, weight = m_Planes[planeIndex].m_Dismissed ? 0.0f : m_Planes[planeIndex].m_Weight } );
			}

			SortedPlanes.Sort();
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				SortedPlane	SP = SortedPlanes[planeIndex];
				if ( !m_Planes[SP.planeIndex].m_Dismissed && planeIndex >= integerTrackbarControlKeepBestPlanesCount.Value ) {
					m_Planes[SP.planeIndex].m_Dismissed = true;
					m_Planes[SP.planeIndex].m_DismissalReason += " LIMIT";
				}
			}


			//////////////////////////////////////////////////////////////////////////
			// Display info
			string	text = "";
			text += "Min, Max, Avg, HAvg Weight = " + minWeight.ToString( "G4" ) + ", " + maxWeight.ToString( "G4" ) + ", " + averageWeight.ToString( "G4" ) + ", " + harmonicAverageWeight.ToString( "G4" ) + " > Dismiss weight = " + dismissWeight + "\r\n";
			text += "Min, Max, Avg, HAvg Kappa = " + minConcentration.ToString( "G4" ) + ", " + maxConcentration.ToString( "G4" ) + ", " + averageConcentration.ToString( "G4" ) + ", " + harmonicAverageConcentration.ToString( "G4" ) + " > Dismiss kappa = " + dismissConcentration + "\r\n\r\n";

			text += planesCount + " Planes:\r\n";
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				text += "#" + planeIndex + " => { " + m_Lobes[planeIndex].Direction.x.ToString( "G4" ) + ", " + m_Lobes[planeIndex].Direction.y.ToString( "G4" ) + "} D = " + m_Planes[planeIndex].m_OrthoDistance.ToString( "G4" ) + "  -  k=" + m_Lobes[planeIndex].Concentration.ToString( "G4" ) + "  -  weight = " + m_Lobes[planeIndex].Alpha.ToString( "G4" ) + m_Planes[planeIndex].m_DismissalReason + " " + (m_Planes[planeIndex].m_Dismissed ? "(DIS)" : "") + "\r\n";
			}

			textBoxPlanes.Text = text;

			// Refresh
			panelOutput.UpdateBitmap();
			panelHistogram.UpdateBitmap();
		}

		#region Planes Clipping

		class Polygon {

			public float2		m_Position;
			public float2		m_Direction;
			public float		m_Min;
			public float		m_Max;

			public bool		IsClipped {
				get { return m_Max - m_Min < 1e-4f; }
			}

			// Initializes the original interval that will get reduced by clipping
			public void		Init( Plane _P ) {
				m_Position = _P.m_Position;
				m_Direction = new float2( -_P.m_Normal.y, _P.m_Normal.x );
				m_Min = -40.0f;
				m_Max = 40.0f;
			}

			// Cuts the interval with another plane
			public void		Cut( Plane _P ) {
				float	ViewDist = m_Direction.Dot( _P.m_Normal );
				float2	D = m_Position - _P.m_Position;
				float	Dist = D.Dot( _P.m_Normal );
				float	t = -Dist / ViewDist;

				if ( ViewDist > 0.0f )
					m_Min = Math.Max( m_Min, t );
				else
					m_Max = Math.Min( m_Max, t );
			}
		}

		// Computes clipped areas for each plane clipped by every other plane
		// Dismisses the planes that are totally clipped
		void	DismissClippedPlanes() {

			Polygon	Poly = new Polygon();
			for ( int i=0; i < m_Planes.Length; i++ ) {
				if ( m_Planes[i].m_Dismissed )
					continue;

				Poly.Init( m_Planes[i] );
				for ( int j=0; j < m_Planes.Length; j++ ) {
					if ( i != j && !m_Planes[j].m_Dismissed )
						Poly.Cut( m_Planes[j] );
				}

				if ( !Poly.IsClipped )
					continue;

				m_Planes[i].m_Dismissed = true;	// Fully clipped!
				m_Planes[i].m_DismissalReason += " CLIPPED";
			}
		}

		#endregion


		private void integerTrackbarControlRoomPlanesCount_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
//			BuildRoom();
		}

		private void integerTrackbarControlObstacles_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
//			BuildRoom();
		}

		private void integerTrackbarControlResultPlanesCount_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
//			BuildRoom();
		}

		private void integerTrackbarControlObstacles_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			BuildRoom();
		}

		private void integerTrackbarControlRoomPlanesCount_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			BuildRoom();
		}

		private void integerTrackbarControlResultPlanesCount_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			BuildRoom();
		}

		private void floatTrackbarControlWeightExponent_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			BuildRoom();
		}

		private void radioButtonNormalWeight_CheckedChanged( object sender, EventArgs e )
		{
			BuildRoom();
		}

		private void checkBoxDismissKappa_CheckedChanged( object sender, EventArgs e )
		{
			BuildRoom();
		}

		private void radioButtonUseBest_CheckedChanged( object sender, EventArgs e )
		{
 			panelDismissFactor.Visible = !radioButtonUseBest.Checked;
// 			panelKeepBestPlanes.Visible = radioButtonUseBest.Checked;
			BuildRoom();
		}

		private void checkBoxShowDismissedPlanes_CheckedChanged( object sender, EventArgs e )
		{
			panelOutput.ShowDismissedPlanes = checkBoxShowDismissedPlanes.Checked;
		}

		private void checkBoxReconstructWeights_CheckedChanged( object sender, EventArgs e )
		{
			BuildRoom();
		}

		private void radioButtonDefaultEMWeight_CheckedChanged( object sender, EventArgs e )
		{
			BuildRoom();
		}
	}
}
