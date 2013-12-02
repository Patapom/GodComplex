using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WMath;

namespace FBX.Scene.Nodes
{
	/// <summary>
	/// The base scene node class that holds a Local->Parent transform
	/// </summary>
	public class	Node : IDisposable
	{
		#region NESTED TYPES

		/// <summary>
		/// Supported node types in a scene
		/// </summary>
		public enum		NODE_TYPE
		{
			NODE,	// Basic node with only PRS informations
			MESH,
			CAMERA,
			LIGHT,
		}

		#endregion

		#region FIELDS

		protected Scene				m_Owner = null;
		protected int				m_ID = -1;
		protected Node				m_Parent = null;
		protected string			m_Name = null;
		protected Matrix4x4			m_Local2Parent = Matrix4x4.Identity;
		protected bool				m_bVisible = true;

		protected List<Node>		m_Children = new List<Node>();

		// Cached state
		protected bool				m_bStateDirty = true;	// Needs propagation of state ?
		protected bool				m_bPropagatedVisibility = true;

		protected bool				m_bFirstLocal2WorldAssignment = true;
		protected Matrix4x4			m_Local2World = Matrix4x4.Identity;
		protected Matrix4x4			m_PreviousLocal2World = Matrix4x4.Identity;	// The object transform matrix from the previous frame

		protected bool				m_bDeltaMotionDirty = true;
		protected Vector			m_DeltaPosition = Vector.Zero;
		protected Quat				m_DeltaRotation = Quat.Identity;
		protected Vector			m_DeltaPivot = Vector.Zero;

		#endregion

		#region PROPERTIES

		public Scene				Owner			{ get { return m_Owner; } }
		public virtual NODE_TYPE	NodeType		{ get { return NODE_TYPE.NODE; } }
		public int					ID				{ get { return m_ID; } }
		public virtual Node			Parent			{ get { return m_Parent; } }
		public virtual string		Name			{ get { return m_Name; } }
		public virtual Matrix4x4	Local2Parent	{ get { return m_Local2Parent; } set { m_Local2Parent = value; PropagateDirtyState(); } }
		public virtual Matrix4x4	Local2World		{ get { return m_Local2World; } set { m_Local2World = value; } }
		public virtual bool			Visible			{ get { return m_bVisible && m_bPropagatedVisibility; } set { m_bVisible = value; PropagateDirtyState(); } }
		public virtual Node[]		Children		{ get { return m_Children.ToArray(); } }

		public virtual Matrix4x4	PreviousLocal2World	{ get { return m_PreviousLocal2World; } set { m_PreviousLocal2World = value; } }

		/// <summary>
		/// Internal event one can subscribe to to be notified a node was updated
		/// </summary>
		internal event EventHandler	StatePropagated;

		#endregion

		#region METHODS

		/// <summary>
		/// Creates a new scene node
		/// </summary>
		/// <param name="_Owner"></param>
		/// <param name="_ID"></param>
		/// <param name="_Name"></param>
		/// <param name="_Parent"></param>
		/// <param name="_Local2Parent"></param>
		internal Node( Scene _Owner, int _ID, string _Name, Node _Parent, Matrix4x4 _Local2Parent )
		{
			m_Owner = _Owner;
			m_ID = _ID;
			m_Name = _Name;
			m_Parent = _Parent;
			m_Local2Parent = _Local2Parent;

			// Append the node to its parent
			if ( _Parent != null )
				_Parent.AddChild( this );
		}

		/// <summary>
		/// Creates a scene node from a stream
		/// </summary>
		/// <param name="_Owner"></param>
		/// <param name="_Parent"></param>
		/// <param name="_Reader"></param>
		internal Node( Scene _Owner, Node _Parent, System.IO.BinaryReader _Reader )
		{
			m_Owner = _Owner;

//			m_NodeType = _Reader.ReadInt32();	// Don't read back the node type as it has already been consumed by the parent
			m_ID = _Reader.ReadInt32();
			m_Owner.RegisterNodeID( this );
			m_Name = _Reader.ReadString();

			m_Parent = _Parent;
			if ( _Parent != null )
				m_Parent.AddChild( this );

			// Read the matrix
			m_Local2Parent.m[0,0] = _Reader.ReadSingle();
			m_Local2Parent.m[0,1] = _Reader.ReadSingle();
			m_Local2Parent.m[0,2] = _Reader.ReadSingle();
			m_Local2Parent.m[0,3] = _Reader.ReadSingle();
			m_Local2Parent.m[1,0] = _Reader.ReadSingle();
			m_Local2Parent.m[1,1] = _Reader.ReadSingle();
			m_Local2Parent.m[1,2] = _Reader.ReadSingle();
			m_Local2Parent.m[1,3] = _Reader.ReadSingle();
			m_Local2Parent.m[2,0] = _Reader.ReadSingle();
			m_Local2Parent.m[2,1] = _Reader.ReadSingle();
			m_Local2Parent.m[2,2] = _Reader.ReadSingle();
			m_Local2Parent.m[2,3] = _Reader.ReadSingle();
			m_Local2Parent.m[3,0] = _Reader.ReadSingle();
			m_Local2Parent.m[3,1] = _Reader.ReadSingle();
			m_Local2Parent.m[3,2] = _Reader.ReadSingle();
			m_Local2Parent.m[3,3] = _Reader.ReadSingle();

			// Read specific data
			LoadSpecific( _Reader );

			// Read children
			int	ChildrenCount = _Reader.ReadInt32();
			for ( int ChildIndex=0; ChildIndex < ChildrenCount; ChildIndex++ )
			{
				NODE_TYPE	ChildType = (NODE_TYPE) _Reader.ReadByte();
				switch ( ChildType )
				{
				case NODE_TYPE.NODE:
					new Node( _Owner, this, _Reader );
					break;

				case NODE_TYPE.MESH:
					new Mesh( _Owner, this, _Reader );
					break;

				case NODE_TYPE.LIGHT:
					new Light( _Owner, this, _Reader );
					break;

				case NODE_TYPE.CAMERA:
					new Camera( _Owner, this, _Reader );
					break;
				}
			}
		}

		public override string ToString()
		{
			return m_Name;
		}

		#region IDisposable Members

		public void Dispose()
		{
			DisposeSpecific();

			// Dispose of children
			foreach ( Node Child in m_Children )
				Child.Dispose();
			m_Children.Clear();
		}

		#endregion

		public void		AddChild( Node _Child )
		{
			m_Children.Add( _Child );
			m_bStateDirty = true;
		}

		public void		RemoveChild( Node _Child )
		{
			m_Children.Remove( _Child );
		}

		/// <summary>
		/// Propagates this node's state to its children (e.g. visibility, Local2World transform, etc.)
		/// This should be done only once per frame and is usually automatically taken care of by the renderer
		/// </summary>
		/// <returns>True if this node's state or the state of one of its children was modified</returns>
		public virtual bool		PropagateState()
		{
			bool	bModified = false;
			if ( m_bStateDirty )
			{
				m_PreviousLocal2World = m_Local2World;	// Current becomes previous...
				m_bDeltaMotionDirty = true;				// Delta values become dirty...

				if ( m_Parent == null )
				{
					m_bPropagatedVisibility = m_bVisible;						// Use our own visibility
					m_Local2World = m_Local2Parent;								// Use our own transform
				}
				else
				{
					m_bPropagatedVisibility = m_Parent.m_bVisible;				// Use parent's visibility
					m_Local2World = m_Local2Parent * m_Parent.m_Local2World;	// Compose with parent...
				}

				if ( m_bFirstLocal2WorldAssignment )
					m_PreviousLocal2World = m_Local2World;	// For first assignment, previous & current matrices are the same !

				m_bStateDirty = false;	// We're good !
				m_bFirstLocal2WorldAssignment = false;
				bModified = true;
			}

			// Propagate to children
			foreach ( Node Child in m_Children )
				bModified |= Child.PropagateState();

			// Notify of propagation
			if ( bModified && StatePropagated != null )
				StatePropagated( this, EventArgs.Empty );

			return bModified;
		}

		/// <summary>
		/// This is a helper to compute relative motion between current and last frame, usually used for motion blur
		/// </summary>
		/// <param name="_DeltaPosition">Returns the difference in position from last frame</param>
		/// <param name="_DeltaRotation">Returns the difference in rotation from last frame</param>
		/// <param name="_Pivot">Returns the pivot position the object rotated about</param>
		public void		ComputeDeltaPositionRotation( out Vector _DeltaPosition, out Quat _DeltaRotation, out Vector _Pivot )
		{
			if ( m_bDeltaMotionDirty )
				Scene.ComputeObjectDeltaPositionRotation( ref m_PreviousLocal2World, ref m_Local2World, out m_DeltaPosition, out m_DeltaRotation, out m_DeltaPivot );

			_DeltaPosition = m_DeltaPosition;
			_DeltaRotation = m_DeltaRotation;
			_Pivot = m_DeltaPivot;

			m_bDeltaMotionDirty = false;
		}

		internal void	Save( System.IO.BinaryWriter _Writer )
		{
			_Writer.Write( (byte) NodeType );
			_Writer.Write( m_ID );
			_Writer.Write( m_Name );

			// Write the matrix
			_Writer.Write( m_Local2Parent.m[0,0] );
			_Writer.Write( m_Local2Parent.m[0,1] );
			_Writer.Write( m_Local2Parent.m[0,2] );
			_Writer.Write( m_Local2Parent.m[0,3] );
			_Writer.Write( m_Local2Parent.m[1,0] );
			_Writer.Write( m_Local2Parent.m[1,1] );
			_Writer.Write( m_Local2Parent.m[1,2] );
			_Writer.Write( m_Local2Parent.m[1,3] );
			_Writer.Write( m_Local2Parent.m[2,0] );
			_Writer.Write( m_Local2Parent.m[2,1] );
			_Writer.Write( m_Local2Parent.m[2,2] );
			_Writer.Write( m_Local2Parent.m[2,3] );
			_Writer.Write( m_Local2Parent.m[3,0] );
			_Writer.Write( m_Local2Parent.m[3,1] );
			_Writer.Write( m_Local2Parent.m[3,2] );
			_Writer.Write( m_Local2Parent.m[3,3] );

			// Write specific data
			SaveSpecific( _Writer );

			// Write children
			_Writer.Write( m_Children.Count );
			foreach ( Node Child in m_Children )
				Child.Save( _Writer );
		}

		/// <summary>
		/// Override this to restore internal references once the scene has loaded
		/// </summary>
		internal virtual void	RestoreReferences()
		{
		}

		protected virtual void	LoadSpecific( System.IO.BinaryReader _Reader )
		{
		}

		protected virtual void	SaveSpecific( System.IO.BinaryWriter _Writer )
		{
		}

		protected virtual void	DisposeSpecific()
		{
		}

		/// <summary>
		/// Mark this node and children as dirty
		/// </summary>
		protected virtual void PropagateDirtyState()
		{
			m_bStateDirty = true;
			foreach ( Node Child in m_Children )
				Child.PropagateDirtyState();
		}

		#endregion
	}
}
