using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace TestVoxelAutomaton
{
	public class VoxelGrid {

		uint		m_POT;
		uint		m_size;
		uint		m_mask;
		byte[,,]	m_grid;

		public	VoxelGrid( uint _POT ) {
			m_POT = _POT;
			m_size = 1U << (int) _POT;
			m_mask = (uint) m_size - 1;
			m_grid = new byte[m_size,m_size,m_size];
		}

		/// <summary>
		/// Randomly fills bottom states
		/// </summary>
		public void		Init() {
			for ( int X=0; X < m_size; X++ )
				for ( int Z=0; Z < m_size; Z++ ) {
					m_grid[X,m_size-1,Z] = (byte) (1 + (SimpleRNG.GetUint() & 1));
				}
		}

		public void		Eval() {

		}

		// fill the center of a cube
		void	EvalCube( uint X, uint Y, uint Z, uint w ) {
			if ( (X+w >= m_size) || (Y+w >= m_size) || (Z+w >= m_size))
				return;
			int idx1 = (m_grid[X,Y,Z]==1?1:0) + (m_grid[X+w,Y,Z]==1?1:0) + (m_grid[X,Y+w,Z]==1?1:0) + (m_grid[X+w,Y+w,Z]==1?1:0) +
						(m_grid[X,Y,Z+w]==1?1:0) + (m_grid[X+w,Y,Z+w]==1?1:0) + (m_grid[X,Y+w,Z+w]==1?1:0) + (m_grid[X+w,Y+w,Z+w]==1?1:0);
			int idx2 = (m_grid[X,Y,Z]==2?1:0) + (m_grid[X+w,Y,Z]==2?1:0) + (m_grid[X,Y+w,Z]==2?1:0) + (m_grid[X+w,Y+w,Z]==2?1:0) +
						(m_grid[X,Y,Z+w]==2?1:0) + (m_grid[X+w,Y,Z+w]==2?1:0) + (m_grid[X,Y+w,Z+w]==2?1:0) + (m_grid[X+w,Y+w,Z+w]==2?1:0);

			m_grid[X+w/2,Y+w/2,Z+w/2] = cubeRule[idx1,idx2];

			if ((random(1.0) < flipP) && (m_grid[X+w/2,Y+w/2,Z+w/2] != 0)) {
				m_grid[X+w/2,Y+w/2,Z+w/2] = 3 - m_grid[X+w/2,Y+w/2,Z+w/2];
			}
		}

/*

// compute the full m_grid of the universe, propegating in from the boundary
// also build it into textures for efficient drawing
void evalState() {
  print("Computing...");
  // do everything on all scales in order
  for (int w = m_size-1; w >= 2; w /= 2) {

    for (int i = 0; i < m_size-1; i+=w) {
      for (int j = 0; j < m_size-1; j+=w) {
        for (int k = 0; k < m_size-1; k+=w) {
          evalCube(i,j,k,w);
        }
      }
    }
    for (int i = 0; i < m_size-1; i+=w) {
      for (int j = 0; j < m_size-1; j+=w) {
        for (int k = 0; k < m_size-1; k+=w) {
          evalFaces(i,j,k,w);
        }
      }
    }
    for (int i = 0; i < m_size-1; i+=w) {
      for (int j = 0; j < m_size-1; j+=w) {
        for (int k = 0; k < m_size-1; k+=w) {
          evalEdges(i,j,k,w);
        }
      }
    }
  }
  // draw the dots to the PShape for efficiency
  print("Lighting...");
  uniDirGI();
  addAmbient();
  print("Building Textures...");
  makeTexes();
  println("Done.");
}
	
*/
	}
}
