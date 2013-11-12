using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;

namespace FBX.Scene.Nodes
{
	/// <summary>
	/// The light class node hosts parameters defining a light
	/// </summary>
	public class	Light : Node
	{
		#region NESTED TYPES

		public enum LIGHT_TYPE
		{
			POINT = 0,
			DIRECTIONAL = 1,
			SPOT = 2,
		}

		public enum DECAY_TYPE
		{
			LINEAR = 0,
			QUADRATIC = 1,
			CUBIC = 2,
		}

		#endregion

		#region FIELDS

		protected LIGHT_TYPE			m_Type = LIGHT_TYPE.POINT;
		protected Vector3				m_Color = Vector3.Zero;
		protected float					m_Intensity = 0.0f;
		protected bool					m_bCastShadow = false;
		protected bool					m_bEnableNearAttenuation = false;
		protected float					m_NearAttenuationStart = 0.0f;
		protected float					m_NearAttenuationEnd = 0.0f;
		protected bool					m_bEnableFarAttenuation = false;
		protected float					m_FarAttenuationStart = 0.0f;
		protected float					m_FarAttenuationEnd = 0.0f;
		protected float					m_HotSpot = 0.0f;	// (aka inner cone angle)
		protected float					m_ConeAngle = 0.0f;	// Valid for spotlights only
		protected DECAY_TYPE			m_DecayType = DECAY_TYPE.QUADRATIC;
		protected float					m_DecayStart = 0.0f;

		protected Node					m_TargetNode = null;
		protected int					m_TempTargetNodeID = -1;	// De-serialized target node ID waiting for rebinding as reference

		protected Vector3				m_CachedDirection = Vector3.UnitY;

		protected static float			ms_GlobalIntensityMultiplier = 1.0f;

		#endregion

		#region PROPERTIES

		public override NODE_TYPE	NodeType	{ get { return NODE_TYPE.LIGHT; } }

		/// <summary>
		/// Gets or sets the light type
		/// </summary>
		public LIGHT_TYPE			Type
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		/// <summary>
		/// Gets the light direction
		/// </summary>
		public Vector3				Direction
		{
			get { return m_CachedDirection; }
		}

		/// <summary>
		/// Gets or sets the light color
		/// </summary>
		public Vector3				Color
		{
			get { return m_Color; }
			set { m_Color = value; }
		}

		/// <summary>
		/// Gets or sets the light intensity
		/// </summary>
		public float				Intensity
		{
			get { return m_Intensity; }
			set { m_Intensity = value; }
		}

		/// <summary>
		/// Gets or sets the "Cast Shadow" state of that light
		/// </summary>
		public bool					CastShadow
		{
			get { return m_bCastShadow; }
			set { m_bCastShadow = value; }
		}

		/// <summary>
		/// Gets or sets the "Enable Near Attenuation" state
		/// </summary>
		public bool					EnableNearAttenuation
		{
			get { return m_bEnableNearAttenuation; }
			set { m_bEnableNearAttenuation = value; }
		}

		/// <summary>
		/// Gets or sets the light near attenuation start distance
		/// </summary>
		public float				NearAttenuationStart
		{
			get { return m_NearAttenuationStart; }
			set { m_NearAttenuationStart = value; }
		}

		/// <summary>
		/// Gets or sets the light near attenuation end distance
		/// </summary>
		public float				NearAttenuationEnd
		{
			get { return m_NearAttenuationEnd; }
			set { m_NearAttenuationEnd = value; }
		}

		/// <summary>
		/// Gets or sets the "Enable Far Attenuation" state
		/// </summary>
		public bool					EnableFarAttenuation
		{
			get { return m_bEnableFarAttenuation; }
			set { m_bEnableFarAttenuation = value; }
		}

		/// <summary>
		/// Gets or sets the light far attenuation start distance
		/// </summary>
		public float				FarAttenuationStart
		{
			get { return m_FarAttenuationStart; }
			set { m_FarAttenuationStart = value; }
		}

		/// <summary>
		/// Gets or sets the light far attenuation end distance
		/// </summary>
		public float				FarAttenuationEnd
		{
			get { return m_FarAttenuationEnd; }
			set { m_FarAttenuationEnd = value; }
		}

		/// <summary>
		/// Gets or sets the light hotspot distance (i.e. inner cone angle)
		/// </summary>
		public float				HotSpot
		{
			get { return m_HotSpot; }
			set { m_HotSpot = value; }
		}

		/// <summary>
		/// Gets or sets the light cone angle (i.e. outer cone angle)
		/// </summary>
		public float				ConeAngle
		{
			get { return m_ConeAngle; }
			set { m_ConeAngle = value; }
		}

		/// <summary>
		/// Gets or sets the light decay type
		/// </summary>
		public DECAY_TYPE			DecayType
		{
			get { return m_DecayType; }
			set { m_DecayType = value; }
		}

		/// <summary>
		/// Gets or sets the light decay start distance
		/// </summary>
		public float				DecayStart
		{
			get { return m_DecayStart; }
			set { m_DecayStart = value; }
		}

		/// <summary>
		/// Gets or sets the optional camera target node
		/// </summary>
		public Node					TargetNode
		{
			get { return m_TargetNode; }
			set
			{
				if ( value == m_TargetNode )
					return;

				m_TargetNode = value;
			}
		}

		/// <summary>
		/// Gets or sets the global intensity multiplier for all lights
		/// </summary>
		public static float			GlobalIntensityMultiplier
		{
			get { return ms_GlobalIntensityMultiplier; }
			set { ms_GlobalIntensityMultiplier = value; }
		}

		#endregion

		#region METHODS

		internal Light( Scene _Owner, int _ID, string _Name, Node _Parent, Matrix _Local2Parent ) : base( _Owner, _ID, _Name, _Parent, _Local2Parent )
		{
		}

		internal Light( Scene _Owner, Node _Parent, System.IO.BinaryReader _Reader ) : base( _Owner, _Parent, _Reader )
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
			_Writer.Write( m_Color.X );
			_Writer.Write( m_Color.Y );
			_Writer.Write( m_Color.Z );
			_Writer.Write( m_Intensity );
			_Writer.Write( m_bCastShadow );
			_Writer.Write( m_bEnableNearAttenuation );
			_Writer.Write( m_NearAttenuationStart );
			_Writer.Write( m_NearAttenuationEnd );
			_Writer.Write( m_bEnableFarAttenuation );
			_Writer.Write( m_FarAttenuationStart );
			_Writer.Write( m_FarAttenuationEnd );
			_Writer.Write( m_HotSpot );
			_Writer.Write( m_ConeAngle );
			_Writer.Write( (int) m_DecayType );
			_Writer.Write( m_DecayStart );
			_Writer.Write( m_TargetNode != null ? m_TargetNode.ID : -1 );
		}

		protected override void		LoadSpecific( System.IO.BinaryReader _Reader )
		{
			m_Type = (LIGHT_TYPE) _Reader.ReadInt32();
			m_Color.X = _Reader.ReadSingle();
			m_Color.Y = _Reader.ReadSingle();
			m_Color.Z = _Reader.ReadSingle();
			m_Intensity = _Reader.ReadSingle();
			m_bCastShadow = _Reader.ReadBoolean();
			m_bEnableNearAttenuation = _Reader.ReadBoolean();
			m_NearAttenuationStart = _Reader.ReadSingle();
			m_NearAttenuationEnd = _Reader.ReadSingle();
			m_bEnableFarAttenuation = _Reader.ReadBoolean();
			m_FarAttenuationStart = _Reader.ReadSingle();
			m_FarAttenuationEnd = _Reader.ReadSingle();
			m_HotSpot = _Reader.ReadSingle();
			m_ConeAngle = _Reader.ReadSingle();
			m_DecayType = (DECAY_TYPE) _Reader.ReadInt32();
			m_DecayStart = _Reader.ReadSingle();
			m_TempTargetNodeID = _Reader.ReadInt32();
		}

		#endregion
	}
}
