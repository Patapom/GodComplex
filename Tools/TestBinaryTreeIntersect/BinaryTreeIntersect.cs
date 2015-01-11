using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestBinaryTreeIntersect
{
	class BinaryTreeIntersect
	{
/*		const float	START_TREE_LEVEL = 3;
		
//		static const float	SPEED = 2.0f;

		float	m_PositionX = 0.0f;
		float	m_OriginY = 0.0f;
		float	m_Slope = 0.25f;	// The gradient of the desired density line

		// Density line
		float LineFunction( float x )
		{
			return m_Slope * (x - m_PositionX) + m_OriginY;
		}
		float InverseLineFunction( float y )
		{
			return (y - m_OriginY) / m_Slope + m_PositionX;
		}

		bool canonicalInt( float i_x0, float i_y0, float SegmentIndex, out float i0, out float i1 )
		{
			float segWidth = 2.0f * i_y0;

			float x0_offset = segWidth * SegmentIndex;

			float x0_seg = i_x0 - x0_offset;
			float x0_prime = i_y0 / 2.0f;
	
			i0 = (m_Slope*x0_seg - 2.0f*x0_prime) / (m_Slope-2.0f) + x0_offset;
			i1 = (m_Slope*x0_seg + 6.0f*x0_prime) / (m_Slope+2.0f) + x0_offset;

			return true;
		}

		// This is a recurrence relation that iteratively computes intersections between the density line and the tree.
		float	FindNextIntersection( float _Distance )
		{
			float	y0 = LineFunction( _Distance );

			// Compute the tree level at which we're standing
			int		TreeLevel = (int) Math.Pow( 2.0, Math.Floor( Math.Log( y0 ) / Math.Log(2.0) ) );

			float	SegmentWidth = 2.0f * TreeLevel;
			int		SegmentIndex = (int) Math.Floor( _Distance / SegmentWidth );

			for( int i = 0; i < 3; i++ )
			{
				float i0, i1;
				canonicalInt( InverseLineFunction( TreeLevel ), TreeLevel, SegmentIndex, out i0, out i1 );

				float di = LineFunction( i0 );
				if ( i0 > _Distance && di >= TreeLevel && di < 2.0f*TreeLevel ) {
					return i0;
				}
				di = LineFunction( i1 );
				if ( i1 > _Distance && di >= TreeLevel && di < 2.0f*TreeLevel ) {
					return i1;
				}
				if ( LineFunction( (SegmentIndex+1.0f) * SegmentWidth ) > 2.0f * TreeLevel ) {
					// move up a level
					TreeLevel *= 2;
					SegmentIndex >>= 1;
				} else {
					// Move to next segment
					SegmentIndex++;
				}
			}
	
			// Shouldn't get here
			return 0.0f;
		}

		// All this really should be on the CPU. this computes the sample distances which do not vary across pixels
		const int	SAMPLE_COUNT = 32;
		float[]		m_Distances = new float[SAMPLE_COUNT+1]; //+1 because we'll do forward differences to calc dt

		void populateDists()
		{
			// generate a naïve view space sampling if the mouse button is down
			bool useNewApproach = true;
// 			if( iMouse.z > 0. )
// 				useNewApproach = false;

			m_PositionX = SPEED*iGlobalTime * (useNewApproach ? 1.0f : 0.0f);

			float	OriginY = (float) Math.Pow( 2.0, -START_TREE_LEVEL );	// 1/8 for tree level 3
			float	OriginX = InverseLineFunction( OriginY );

			m_Distances[0] = OriginX;
			for ( int i = 1; i < SAMPLE_COUNT; i++ ) {
				m_Distances[i] = FindNextIntersection( m_Distances[i-1] );
				m_Distances[i-1] -= OriginX;	// Now that we are done with it, make it camera relative
			}

			// Last 2 elements
			m_Distances[SAMPLE_COUNT-1] -= OriginX;
			m_Distances[SAMPLE_COUNT] = m_Distances[SAMPLE_COUNT-1];
		}
*/
	}
}
