using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WMath;

namespace FBX.Scene.Nodes
{
	/// <summary>
	/// The camera class node hosts parameters defining a camera
	/// </summary>
	public class	Camera : Node
	{
		#region NESTED TYPES

		public enum PROJECTION_TYPE
		{
			PERSPECTIVE = 0,
			ORTHOGRAPHIC = 1,
		}

		#endregion

		#region FIELDS

		protected PROJECTION_TYPE		m_Type = PROJECTION_TYPE.PERSPECTIVE;
		protected Vector				m_Target = Vector.Zero;
		protected float					m_FOV = 0.0f;
		protected float					m_AspectRatio = 0.0f;
		protected float					m_ClipNear = 0.0f;
		protected float					m_ClipFar = 0.0f;
		protected float					m_Roll = 0.0f;

		protected Node					m_TargetNode = null;
		protected int					m_TempTargetNodeID = -1;	// De-serialized target node ID waiting for rebinding as reference

		#endregion

		#region PROPERTIES

		public override NODE_TYPE	NodeType	{ get { return NODE_TYPE.CAMERA; } }

		/// <summary>
		/// Gets or sets the camera projection type
		/// </summary>
		public PROJECTION_TYPE		Type
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		/// <summary>
		/// Gets or sets the camera target
		/// </summary>
		public Vector				Target
		{
			get { return m_Target; }
			set { m_Target = value; }
		}

		/// <summary>
		/// Gets or sets the camera FOV
		/// </summary>
		public float				FOV
		{
			get { return m_FOV; }
			set { m_FOV = value;  }
		}

		/// <summary>
		/// Gets or sets the camera aspect ratio
		/// </summary>
		public float				AspectRatio
		{
			get { return m_AspectRatio; }
			set { m_AspectRatio = value; }
		}

		/// <summary>
		/// Gets or sets the camera near clip distance
		/// </summary>
		public float				ClipNear
		{
			get { return m_ClipNear; }
			set { m_ClipNear = value; }
		}

		/// <summary>
		/// Gets or sets the camera far clip distance
		/// </summary>
		public float				ClipFar
		{
			get { return m_ClipFar; }
			set { m_ClipFar = value; }
		}

		/// <summary>
		/// Gets or sets the camera roll
		/// </summary>
		public float				Roll
		{
			get { return m_Roll; }
			set { m_Roll = value; }
		}

		/// <summary>
		/// Gets or sets the optional camera target node
		/// </summary>
		public Node					TargetNode
		{
			get { return m_TargetNode; }
			set	{ m_TargetNode = value; }
		}

		#endregion

		#region METHODS

		internal Camera( Scene _Owner, int _ID, string _Name, Node _Parent, Matrix4x4 _Local2Parent ) : base( _Owner, _ID, _Name, _Parent, _Local2Parent )
		{
		}

		internal Camera( Scene _Owner, Node _Parent, System.IO.BinaryReader _Reader ) : base( _Owner, _Parent, _Reader )
		{
		}

		internal override void RestoreReferences()
		{
			base.RestoreReferences();

			TargetNode = m_Owner.FindNode( m_TempTargetNodeID );
		}

		protected override void		SaveSpecific( System.IO.BinaryWriter _Writer )
		{
			_Writer.Write( (int) m_Type );
			_Writer.Write( m_Target.X );
			_Writer.Write( m_Target.Y );
			_Writer.Write( m_Target.Z );
			_Writer.Write( m_FOV );
			_Writer.Write( m_AspectRatio );
			_Writer.Write( m_ClipNear );
			_Writer.Write( m_ClipFar );
			_Writer.Write( m_Roll );
			_Writer.Write( m_TargetNode != null ? m_TargetNode.ID : -1 );
		}

		protected override void		LoadSpecific( System.IO.BinaryReader _Reader )
		{
			m_Type = (PROJECTION_TYPE) _Reader.ReadInt32();
			m_Target.X = _Reader.ReadSingle();
			m_Target.Y = _Reader.ReadSingle();
			m_Target.Z = _Reader.ReadSingle();
			m_FOV = _Reader.ReadSingle();
			m_AspectRatio = _Reader.ReadSingle();
			m_ClipNear = _Reader.ReadSingle();
			m_ClipFar = _Reader.ReadSingle();
			m_Roll = _Reader.ReadSingle();
			m_TempTargetNodeID = _Reader.ReadInt32();
		}

		#endregion
	}
}
