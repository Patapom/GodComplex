using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace OfflineCloudRenderer2
{
	/// <summary>
	/// This class holds the various distance field primitives inside an AABBTree
	/// Each primitive is encompassed in an AABB, primitive groups are formed
	///  and stored in a larger encompassing AABB until all groups have been
	///  stored in the root AABB
	/// 
	/// Implementation was shamelessly stolen from box2D since I don't want to
	///  lose time with this awful structure...
	/// </summary>
	public class AABBTree
	{
		#region	CONSTANTS

		#endregion

		#region NESTED TYPES

		public struct	AABB
		{
			public float3	m_Min;
			public float3	m_Max;

			// Get the perimeter length
			public float	Perimeter	{ get { return 2.0f * (m_Max.x + m_Max.y + m_Max.z - m_Min.x - m_Min.y - m_Min.z); } }

			public void		Combine( AABB _Other )
			{
				m_Min.Min( _Other.m_Min );
				m_Max.Max( _Other.m_Max );
			}

			public void		Combine( AABB a, AABB b )
			{
				m_Min = a.m_Min;
				m_Min.Min( b.m_Min );
				m_Max = a.m_Max;
				m_Max.Max( b.m_Max );
			}
		}

		public class	Node
		{
			public AABB		m_AABB;
			public int		m_Height = 0;
			public Node		m_Parent = null;
			public Node[]	m_Children = new Node[2];
			public IDistanceFieldPrimitive	m_Primitive = null;

			public Node		Child0	{ get { return m_Children[0]; } set { m_Children[0] = value; } }
			public Node		Child1	{ get { return m_Children[1]; } set { m_Children[1] = value; } }
			public bool		IsLeaf	{ get { return m_Children[1] == null; } }
		}

		#endregion

		#region FIELDS

		public Node		m_Root = null;
		public List<IDistanceFieldPrimitive>	m_Primitives = new List<IDistanceFieldPrimitive>();

		#endregion

		#region PROPERTIES

		#endregion

		#region METHODS

		public	AABBTree()
		{
		}

		public void	InsertLeaf( IDistanceFieldPrimitive _Primitive )
		{
			AABB	PrimAABB = new AABB() {
				m_Min = _Primitive.Min,
				m_Max = _Primitive.Max
			};
			Node	Leaf = new Node() {
				m_AABB = PrimAABB,
				m_Primitive = _Primitive
			};
			m_Primitives.Add( _Primitive );

			if ( m_Root == null )
			{	// This is our new root
				m_Root = Leaf;
				return;
			}

			// Find the best Sibling for this node
			AABB	LeafAABB = Leaf.m_AABB;
			Node	Current = m_Root;
			while ( !Leaf.IsLeaf )
			{
				Node	Child0 = Leaf.Child0;
				Node	Child1 = Leaf.Child1;

				float	Area = Current.m_AABB.Perimeter;

				AABB	CombinedAABB = Current.m_AABB;
						CombinedAABB.Combine( LeafAABB );
				float	CombinedArea = CombinedAABB.Perimeter;

				// Cost of creating a new parent for this node and the new Leaf
				float	Cost = 2.0f * CombinedArea;

				// Minimum Cost of pushing the Leaf further down the tree
				float	InheritanceCost = 2.0f * (CombinedArea - Area);

				// Cost of descending into Child0
				AABB	aabb = LeafAABB;
						aabb.Combine( Child0.m_AABB );
				float	Cost0;
				if ( Child0.IsLeaf )
					Cost0 = aabb.Perimeter + InheritanceCost;
				else
				{
					float	OldArea = Child0.m_AABB.Perimeter;
					float	NewArea = aabb.Perimeter;
					Cost0 = (NewArea - OldArea) + InheritanceCost;
				}

				// Cost of descending into Child1
				aabb = LeafAABB;
				aabb.Combine( Child1.m_AABB );
				float	Cost1;
				if ( Child1.IsLeaf )
				{
					Cost1 = aabb.Perimeter + InheritanceCost;
				}
				else
				{
					float	OldArea = Child1.m_AABB.Perimeter;
					float	NewArea = aabb.Perimeter;
					Cost1 = NewArea - OldArea + InheritanceCost;
				}

				// Descend according to the minimum Cost.
				if ( Cost < Cost0 && Cost < Cost1 )
					break;


				// Descend
				if ( Cost0 < Cost1 )
					Current = Child0;
				else
					Current = Child1;
			}

			Node	Sibling = Current;

			// Create a new parent.
			Node	OldParent = Sibling.m_Parent;
			Node	NewParent = new Node() {
				m_Parent = OldParent,
				m_Height = Sibling.m_Height + 1
			};
			NewParent.m_AABB = LeafAABB;
			NewParent.m_AABB.Combine( Sibling.m_AABB );
			if ( OldParent != null )
			{	// The Sibling was not the root.
				if ( OldParent.Child0 == Sibling )
					OldParent.Child0 = NewParent;
				else
					OldParent.Child1 = NewParent;

				NewParent.Child0 = Sibling;
				NewParent.Child1 = Leaf;
				Sibling.m_Parent = NewParent;
				Leaf.m_Parent = NewParent;
			}
			else
			{	// The Sibling was the root.
				NewParent.Child0 = Sibling;
				NewParent.Child1 = Leaf;
				Sibling.m_Parent = NewParent;
				Leaf.m_Parent = NewParent;
				m_Root = NewParent;
			}

			// Walk back up the tree fixing heights and AABBs
			Current = Leaf.m_Parent;
			while ( Current != null )
			{
				Current = Balance( Current );

				Node	Child0 = Current.Child0;
				if ( Child0 == null )
					throw new Exception( "Unexpected null child!" );
				Node	Child1 = Current.Child1;
				if ( Child1 == null )
					throw new Exception( "Unexpected null child!" );

				Current.m_Height = 1 + Math.Max( Child0.m_Height, Child1.m_Height );
				Current.m_AABB = Child0.m_AABB;
				Current.m_AABB.Combine( Child1.m_AABB );

				Current = Current.m_Parent;
			}
		}

		/// <summary>
		/// Evaluates the distance to the closest distance field primitive in the field
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		public float	EvalDistance( float3 _Position )
		{
			float	Distance = float.MaxValue;
			foreach ( IDistanceFieldPrimitive P in m_Primitives )
				Distance = Math.Min( Distance, P.Eval( _Position ) );
			return Distance;
		}

		// Perform a left or right rotation if node A is imbalanced.
		// Returns the new root index.
		private Node		Balance( Node A )
		{
			if ( A.IsLeaf || A.m_Height < 2 )
				return A;

			Node	B = A.Child0;
			Node	C = A.Child1;
			int		Balance = C.m_Height - B.m_Height;

			// Rotate C up
			if ( Balance > 1 )
			{
				Node	F = C.Child0;
				Node	G = C.Child1;

				// Swap A and C
				C.Child0 = A;
				C.m_Parent = A.m_Parent;
				A.m_Parent = C;

				// A's old parent should point to C
				if ( C.m_Parent != null )
				{
					if ( C.m_Parent.Child0 == A )
						C.m_Parent.Child0 = C;
					else
					{
						if ( C.m_Parent.Child1 != A )
							throw new Exception( "Unexpected child value!" );
						C.m_Parent.Child1 = C;
					}
				}
				else
				{
					m_Root = C;
				}

				// Rotate
				if ( F.m_Height > G.m_Height )
				{
					C.Child1 = F;
					A.Child1 = G;
					G.m_Parent = A;
					A.m_AABB.Combine( B.m_AABB, G.m_AABB );
					C.m_AABB.Combine( A.m_AABB, F.m_AABB );

					A.m_Height = 1 + Math.Max( B.m_Height, G.m_Height );
					C.m_Height = 1 + Math.Max( A.m_Height, F.m_Height );
				}
				else
				{
					C.Child1 = G;
					A.Child1 = F;
					F.m_Parent = A;
					A.m_AABB.Combine( B.m_AABB, F.m_AABB );
					C.m_AABB.Combine( A.m_AABB, G.m_AABB );

					A.m_Height = 1 + Math.Max( B.m_Height, F.m_Height );
					C.m_Height = 1 + Math.Max( A.m_Height, G.m_Height );
				}

				return C;
			}
	
			// Rotate B up
			if ( Balance < -1 )
			{
				Node	D = B.Child0;
				Node	E = B.Child1;

				// Swap A and B
				B.Child0 = A;
				B.m_Parent = A.m_Parent;
				A.m_Parent = B;

				// A's old parent should point to B
				if ( B.m_Parent != null )
				{
					if ( B.m_Parent.Child0 == A )
						B.m_Parent.Child0 = B;
					else
					{
						if ( B.m_Parent.Child1 != A )
							throw new Exception( "Unexpected child value!" );
						B.m_Parent.Child1 = B;
					}
				}
				else
				{
					m_Root = B;
				}

				// Rotate
				if ( D.m_Height > E.m_Height )
				{
					B.Child1 = D;
					A.Child0 = E;
					E.m_Parent = A;
					A.m_AABB.Combine( C.m_AABB, E.m_AABB );
					B.m_AABB.Combine( A.m_AABB, D.m_AABB );

					A.m_Height = 1 + Math.Max( C.m_Height, E.m_Height );
					B.m_Height = 1 + Math.Max( A.m_Height, D.m_Height );
				}
				else
				{
					B.Child1 = E;
					A.Child0 = D;
					D.m_Parent = A;
					A.m_AABB.Combine( C.m_AABB, D.m_AABB );
					B.m_AABB.Combine( A.m_AABB, E.m_AABB );

					A.m_Height = 1 + Math.Max( C.m_Height, D.m_Height );
					B.m_Height = 1 + Math.Max( A.m_Height, E.m_Height );
				}

				return B;
			}

			return A;
		}

		#endregion
	}
}
