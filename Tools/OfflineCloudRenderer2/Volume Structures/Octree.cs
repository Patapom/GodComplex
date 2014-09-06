using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace OfflineCloudRenderer2
{
	/// <summary>
	/// This class builds an octree from an AABB tree of distance field primitives and holds the methods to query boundary octree cells
	/// </summary>
	public class Octree
	{
		#region NESTED TYPES

		public class	Cell
		{
			public float3		m_Min;		// Back lower left corner
			public float		m_Size;

			public Cell			m_Parent = null;
			public Cell[,,]		m_Children = new Cell[2,2,2];
			public float[,,]	m_Corners = new float[2,2,2];		// Corners of this cell

			public static float	MIN_CELL_SIZE = 1.0f;

			/// <summary>
			/// Inserts an AABB in the tree, ensuring leaf-containing tree cells have at most the minimum dimension of the AABB
			/// </summary>
			/// <param name="_Min">The minimum corner of the AABB</param>
			/// <param name="_Max">The maximum corner of the AABB</param>
			/// <returns></returns>
			internal void		InsertAABB( float3 _Min, float3 _Max )
			{
				float3	Dim = _Max - _Min;
				float	MinSize = Dim.Min();
				if ( m_Size < MinSize || m_Size <= MIN_CELL_SIZE )
					return;	// This cell is already too small

				// Recurse through children
				float	ChildSize = 0.5f * m_Size;
				float3	P;
				for ( int Z=0; Z < 2; Z++ )
				{
					P.z = m_Min.z + Z * ChildSize;
					float	nz = P.z + ChildSize;
					if ( _Min.z >= nz || _Max.z < P.z )
						continue;	// Easy early out

					for ( int Y=0; Y < 2; Y++ )
					{
						P.y = m_Min.y + Y * ChildSize;
						float	ny = P.y + ChildSize;
						if ( _Min.y >= ny || _Max.y < P.y )
							continue;	// Easy early out

						for ( int X=0; X < 2; X++ )
						{
							P.x = m_Min.x + X * ChildSize;
							float	nx = P.x + ChildSize;
							if ( _Min.x >= nx || _Max.x < P.x )
								continue;	// Easy early out

							if ( m_Children[X,Y,Z] == null )
							{	// Create the child
								Cell	Child = new Cell() {
									m_Parent = this,
									m_Min = P,
									m_Size = ChildSize,
								};
								m_Children[X,Y,Z] = Child;
							}

							// Recurse
							m_Children[X,Y,Z].InsertAABB( _Min, _Max );
						}
					}
				}
			}

			/// <summary>
			/// Builds the final tree cells using the pre-built cells and the distance field
			/// This method expects the current cell's corner distances to be already computed
			/// </summary>
			/// <param name="_Tree"></param>
			internal void	BuildDistanceFieldCells( AABBTree _Tree )
			{
				float	ChildSize = 0.5f * m_Size;
				if ( ChildSize <= MIN_CELL_SIZE )
					return;	// This cell's children will be too small...

				// Build corner distances for all children
				float[,,]	ChildCorners = new float[3,3,3];
				ChildCorners[0,0,0] = m_Corners[0,0,0];
				ChildCorners[2,0,0] = m_Corners[1,0,0];
				ChildCorners[0,2,0] = m_Corners[0,1,0];
				ChildCorners[2,2,0] = m_Corners[1,1,0];
				ChildCorners[0,0,2] = m_Corners[0,0,1];
				ChildCorners[2,0,2] = m_Corners[1,0,1];
				ChildCorners[0,2,2] = m_Corners[0,1,1];
				ChildCorners[2,2,2] = m_Corners[1,1,1];
				float3	P;
				for ( int Z=0; Z < 3; Z++ )
				{
					P.z = m_Min.z + Z * ChildSize;
					for ( int Y=0; Y < 3; Y++ )
					{
						P.y = m_Min.y + Y * ChildSize;
						for ( int X=0; X < 3; X++ )
						{
							P.x = m_Min.x + X * ChildSize;
							if ( ChildCorners[X,Y,Z] == 0.0f )
								ChildCorners[X,Y,Z] = _Tree.EvalDistance( P );
						}
					}
				}

				// Recurse through children that are either:
				//	_ non null (i.e. already created by the first pass)
				//	_ have corners with non-consistent signs (i.e. boundary child)
				for ( int Z=0; Z < 2; Z++ )
				{
					P.z = m_Min.z + Z * ChildSize;
					float	nz = P.z + ChildSize;

					for ( int Y=0; Y < 2; Y++ )
					{
						P.y = m_Min.y + Y * ChildSize;
						float	ny = P.y + ChildSize;

						for ( int X=0; X < 2; X++ )
						{
							P.x = m_Min.x + X * ChildSize;
							float	nx = P.x + ChildSize;

							// Grab the child node
							bool	ForceRecurse = false;
							Cell	Child = m_Children[X,Y,Z];
							if ( Child == null )
							{	// Create missing child
								Child = new Cell() {
									m_Parent = this,
									m_Min = P,
									m_Size = ChildSize
								};
								m_Children[X,Y,Z] = Child;
							}
							else
								ForceRecurse = true;	// Was already created by first pass, we need to recurse through it...

							// Assign child's corner distances
							int	PositiveCornersCount = 0;
							int	NegativeCornersCount = 0;
							for ( int CZ=0; CZ < 2; CZ++ )
								for ( int CY=0; CY < 2; CY++ )
									for ( int CX=0; CX < 2; CX++ )
									{
										Child.m_Corners[CX,CY,CZ] = ChildCorners[X+CX,Y+CY,Z+CZ];
										if ( Child.m_Corners[CX,CY,CZ] > 0.0f )
											PositiveCornersCount++;
										else
											NegativeCornersCount++;
									}

							// Recurse through child if corners' signs mismatch
							if ( ForceRecurse || (PositiveCornersCount > 0 && NegativeCornersCount > 0) )
								BuildDistanceFieldCells( _Tree );
						}
					}
				}
			}
		}

		#endregion

		#region FIELDS

		public Cell		m_Root = null;

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public	Octree()
		{
		}

		/// <summary>
		/// Builds the octree from the AABB tree of distance field primitives
		/// </summary>
		/// <param name="_Tree"></param>
		/// <param name="_MinCellSize"></param>
		public void		Build( AABBTree _Tree, float _MinCellSize )
		{
			if ( _Tree.m_Root == null )
				throw new Exception( "Invalid AABB tree to build octree from!" );

			Cell.MIN_CELL_SIZE = _MinCellSize;

			// Compute maximum extent for our octree from root AABB
			AABBTree.AABB	RootAABB = _Tree.m_Root.m_AABB;
			float			MaxDimension = (RootAABB.m_Max - RootAABB.m_Min).Max();
			MaxDimension *= 1.1f;	// Always a bit larger

			// Build center & min corner
			float3			Center = 0.5f * (RootAABB.m_Max - RootAABB.m_Min);
			float3			Min = Center - 0.5f * MaxDimension * float3.One;

			// Build root cell
			m_Root = new Cell();
			m_Root.m_Min = Min;
			m_Root.m_Size = MaxDimension;

			// Insert each primitive in turn to pre-build important cells
			foreach ( IDistanceFieldPrimitive Prim in _Tree.m_Primitives )
				m_Root.InsertAABB( Prim.Min, Prim.Max );

			// Now, build actual cells by querying the distance field
			float3	P;// = new float3();
			for ( int Z=0; Z < 1; Z++ )
			{
				P.z = Min.z + Z * MaxDimension;
				for ( int Y=0; Y < 1; Y++ )
				{
					P.y = Min.y + Y * MaxDimension;
					for ( int X=0; X < 1; X++ )
					{
						P.x = Min.x + X * MaxDimension;
						m_Root.m_Corners[X,Y,Z] = _Tree.EvalDistance( P );
					}
				}
			}
			m_Root.BuildDistanceFieldCells( _Tree );
		}

		#endregion
	}
}
