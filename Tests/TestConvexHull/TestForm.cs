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

//////////////////////////////////////////////////////////////////////////
// This test starts from a bunch of (position,normal) and attempts to find a convex hull
// It's easier than the standard algorithm that starts from a set of positions only since
//	we already have a bunch of planes that can be used for quick rejection
//////////////////////////////////////////////////////////////////////////
namespace TestConvexHull
{
	public struct Plane {
		public float3	position;
		public float3	normal;
		public float	sortKey;
	}

	public partial class TestForm : Form, IComparer< Plane >
	{
		const int			PLANES_COUNT = 64;

		public Plane[]		m_planes = new Plane[PLANES_COUNT];
		public Plane[]		m_convexHull = null;

		public TestForm()
		{
			InitializeComponent();

			panelOutput.m_Owner = this;

			// Generate random planes
			Random	RNG = new Random( 1 );
			float3	probeProsition = float3.Zero;

			List< Plane >	tempPlanes = new List< Plane >();
			for ( int planeIndex=0; planeIndex < PLANES_COUNT; planeIndex++ ) {
				double	distance = 5.0 + 2.0 * (1.0 + Math.Sin( 8.0 * Math.PI * planeIndex / PLANES_COUNT ));
				double	refAngle = 2.0 * Math.PI * planeIndex / PLANES_COUNT;
				double	deltaAngle = Math.Atan( -2.0 * 8.0 * Math.PI * Math.Cos( 8.0 * Math.PI * planeIndex / PLANES_COUNT ) / PLANES_COUNT );
				double	angle = refAngle + deltaAngle;

				float3	direction = new float3( (float) Math.Cos( refAngle ), (float) Math.Sin( refAngle ), 0.0f );
				tempPlanes.Add( new Plane() { normal = new float3( (float) Math.Cos( angle ), (float) Math.Sin( angle ), 0.0f ), position = probeProsition - (float) distance * direction } );
			}

			// Randomize
			for ( int i=0; i < tempPlanes.Count; i++ ) {
				int	j = RNG.Next( tempPlanes.Count );
				Plane	temp = tempPlanes[i];
				tempPlanes[i] = tempPlanes[j];
				tempPlanes[j] = temp;
			}
			m_planes = tempPlanes.ToArray();

			// Go!
			m_center = float3.Zero;
			float	largestDistance = 0.0f;
			int		largestPlaneIndex = -1;
			for ( int i=0; i < m_planes.Length; i++ ) {
				float	distance = (m_center - m_planes[i].position).Dot( m_planes[i].normal );
				if ( distance > largestDistance ) {
					largestDistance = distance;
					largestPlaneIndex = i;
				}
			}

			m_convexHull = BuildConvexHull( new Plane[] { m_planes[largestPlaneIndex] }, m_planes, (float) Math.PI / 20.0f );

			panelOutput.UpdateBitmap();
		}

		Plane[]	BuildConvexHull( Plane[] _ExistingPlanes, Plane[] _PlanesForHull, float _MinAngleBetweenPlane ) {
			float	cosMinAngle = (float) Math.Cos( _MinAngleBetweenPlane );

			List< Plane >	planes = new List< Plane >( _PlanesForHull );	// List of candidate planes for hull
			List< Plane >	results = new List< Plane >( _ExistingPlanes );	// List of planes already used for hull

			// 1st débile test
// 			planes.Sort( this );
// 			foreach ( Plane P0 in planes ) {
// 				bool	isValid = true;
// 				foreach ( Plane P1 in results ) {
// 					float	cosAngle = P1.normal.Dot( P0.normal );
// 					if ( cosAngle > cosMinAngle ) {
// 						isValid = false;	// Too close to an existing plane!
// 						break;
// 					}
// 				}
// 
// 				if ( isValid ) {
// 					results.Add( P0 );	// Now part of the convex hull!
// 				}
// 			}

			// 2nd débile test
// 			while ( planes.Count > 0 ) {
// 
// 				float	bestCandidateDistance = 0.0f;
// 				int		bestCandidateIndex = -1;
// 				for ( int candidateIndex=0; candidateIndex < planes.Count; candidateIndex++ ) {
// 					Plane	candidate = planes[candidateIndex];
// 
// 					// Check if the candidate's normal is far enough from existing hull's normals
// 					bool	isValid = true;
// 					foreach ( Plane P in results ) {
// 						float	cosAngle = P.normal.Dot( candidate.normal );
// 						if ( cosAngle > cosMinAngle ) {
// 							isValid = false;	// Too close to an existing plane!
// 							break;
// 						}
// 					}
// 					if ( !isValid ) {
// 						// Remove the plane from the list of candidates
// 						planes.RemoveAt( candidateIndex );
// 						candidateIndex--;
// 						continue;
// 					}
// 
// 					// Keep the best plane with the largest distance from the center
// 					float	candidateDistance = (m_center - candidate.position).Dot( candidate.normal );
// 					if ( candidateDistance > bestCandidateDistance ) {
// 						bestCandidateDistance = candidateDistance;
// 						bestCandidateIndex = candidateIndex;
// 					}
// 				}
// 
// 				if ( bestCandidateIndex >= 0 )
// 					results.Add( planes[bestCandidateIndex] );
// 			}

// 			// 3rd test seems to be the correct one:
// 			// 
// 			while ( true ) {
// 
// 				float	bestPlaneScore = 0.0f;
// 				int		bestPlaneIndex = -1;
// 				for ( int candidateIndex=0; candidateIndex < planes.Count; candidateIndex++ ) {
// 					Plane	candidate = planes[candidateIndex];
// 
// 					// Check if the candidate's normal is far enough from existing hull's normals
// 					// This parameter allows to specify the precision of the hull (e.g. one plane every 10°)
// 					float	maxCosAngle = 0.0f;
// 					bool	isValid = true;
// 					foreach ( Plane P in results ) {
// 						float	cosAngle = P.normal.Dot( candidate.normal );
// 						maxCosAngle = Math.Max( maxCosAngle, cosAngle );
// 						if ( cosAngle > cosMinAngle ) {
// 							isValid = false;	// Too close to an existing plane!
// 							break;
// 						}
// 					}
// 					if ( !isValid ) {
// 						continue;	// Invalid for now but maybe not later when the hull is more closed
// 					}
// 
// 					// Check if the candidate plane is not in front of any other plane
// 					// This test allows to reject planes that can be "shadowed" by other, further planes
// 
// 					// Start by checking the planes already in the hull
// 					for ( int i=0; i < results.Count; i++ ) {
// 						Plane	existingPlane = results[i];
// 						if ( (existingPlane.position - candidate.position).Dot( candidate.normal ) < 0.0f ) {
// 							isValid = false;	// Candidate hides this plane's position
// 							break;
// 						}
// 					}
// 					// Then check all remaining planes
// 					for ( int otherCandidateIndex=0; otherCandidateIndex < planes.Count; otherCandidateIndex++ ) {
// 						if ( otherCandidateIndex != candidateIndex ) {
// 							Plane	otherCandidate = planes[otherCandidateIndex];
// 							if ( (otherCandidate.position - candidate.position).Dot( candidate.normal ) < 0.0f ) {
// 								isValid = false;	// Candidate hides this plane's position
// 								break;
// 							}
// 						}
// 					}
// 					if ( !isValid ) {
// 						// Remove the plane from the list of candidates because getting shadowed by other planes
// 						//	is definitely not good, whatever the completion of the hull...
// 						planes.RemoveAt( candidateIndex );
// 						candidateIndex--;
// 						continue;
// 					}
// 
// 					// Keep the best plane with the largest distance from the center
// 					float	candidateDistance = (m_center - candidate.position).Dot( candidate.normal );
// 							candidateDistance *= 0.5f + 0.5f * maxCosAngle;	// Will multiply by 1 if angle is close to one of the hull planes, by 0 if it's far away
// 																			// So basically the best planes are the ones that are the farthest from the probe and the nearest (in term of angular proximity)
// 																			// to the existing hull planes.
// 
// 					if ( candidateDistance > bestPlaneScore ) {
// 						bestPlaneScore = candidateDistance;
// 						bestPlaneIndex = candidateIndex;
// 					}
// 				}
// 
// 				if ( bestPlaneIndex == -1 )
// 					break;
// 
// 				results.Add( planes[bestPlaneIndex] );
// 			}


			// 4th test is the same as 3rd except simpler:
			//	• We first discard planes that are shadowed by any other plane (this was done at every iteration in the 3rd test whereas it's a definite
			//		test that should only be performed once)
			//	• Build the hull progressively from the list of valid planes
			//
			List< Plane >	validPlanes = new List< Plane >();
			for ( int i=0; i < planes.Count; i++ ) {
				Plane	candidate = planes[i];
				bool	isValid = true;

				// Check shadowing by existing planes
				for ( int j=0; j < results.Count; j++ ) {
					Plane	existingPlane = results[j];
					if ( (existingPlane.position - candidate.position).Dot( candidate.normal ) < 0.0f ) {
						isValid = false;		// Existing plane hides this candidate's position
						break;
					}
				}
				if ( !isValid )
					continue;

				// Check shadowing by all other planes
				for ( int j=0; j < planes.Count; j++ )
					if ( i != j ) {
						Plane	otherCandidate = planes[j];
						if ( (otherCandidate.position - candidate.position).Dot( candidate.normal ) < 0.0f ) {
							isValid = false;	// Other candidate hides this candidate's position
							break;
						}
					}

				if ( isValid )
					validPlanes.Add( candidate );
			}

			while ( true ) {

				float	bestPlaneScore = 0.0f;
				int		bestPlaneIndex = -1;
				for ( int candidateIndex=0; candidateIndex < validPlanes.Count; candidateIndex++ ) {
					Plane	candidate = validPlanes[candidateIndex];

					// Check if the candidate's normal is far enough from existing hull's normals
					// This parameter allows to specify the precision of the hull (e.g. one plane every 10°)
					float	maxCosAngle = 0.0f;
					bool	isValid = true;
					foreach ( Plane P in results ) {
						float	cosAngle = P.normal.Dot( candidate.normal );
						maxCosAngle = Math.Max( maxCosAngle, cosAngle );
						if ( cosAngle > cosMinAngle ) {
							isValid = false;	// Too close to an existing plane!
							break;
						}
					}
					if ( !isValid ) {
						continue;	// Invalid for now but maybe not later when the hull is more closed
					}

					// Keep the best plane with the largest distance from the center
					float	candidateDistance = (m_center - candidate.position).Dot( candidate.normal );
							candidateDistance *= 0.5f + 0.5f * maxCosAngle;	// Will multiply by 1 if angle is close to one of the hull planes, by 0 if it's far away
																			// So basically the best planes are the ones that are the farthest from the probe and the nearest (in term of angular proximity)
																			// to the existing hull planes.

					if ( candidateDistance > bestPlaneScore ) {
						bestPlaneScore = candidateDistance;
						bestPlaneIndex = candidateIndex;
					}
				}

				if ( bestPlaneIndex == -1 )
					break;

				results.Add( validPlanes[bestPlaneIndex] );
				validPlanes.RemoveAt( bestPlaneIndex );
			}

			return results.ToArray();
		}

		#region IComparer<Plane> Members

		float3	m_center;
		public int Compare( Plane x, Plane y ) {
 			return Comparer<float>.Default.Compare( y.sortKey, x.sortKey );

// 			float	Dx = (m_center - x.position).Dot( x.normal );
// 			float	Dy = (m_center - y.position).Dot( y.normal );
// 			return Comparer<float>.Default.Compare( Dy, Dx );
		}

		#endregion
	}
}
