using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using FBX.Scene.Materials;

namespace FBX.Scene.Nodes
{
	/// <summary>
	/// The mesh class node hosts a collection of primitives
	/// </summary>
	public class	Mesh : Node
	{
		#region NESTED TYPES

		/// <summary>
		/// A primitive wraps a basic Nuaj primitive and contains additional informations like material parameters to setup to render the primitive
		/// Primitives should be created via a ITechniqueSupportsObjects render technique using the CreatePrimitive() method
		/// </summary>
		public class Primitive : IDisposable
		{
			#region NESTED TPES

			public class	VertexStream
			{
				#region NESTED TYPES

				public enum	USAGE
				{
					UNKNOWN,
					POSITION,
					NORMAL,
					TANGENT,
					BITANGENT,
					TEXCOORDS,
					COLOR,
					COLOR_HDR,
					GENERIC,
				}

				public enum FIELD_TYPE
				{
					UNKNOWN,
					FLOAT,
					FLOAT2,
					FLOAT3,
					FLOAT4,
					UINT32,
				}

				#endregion

				#region FIELDS

				protected USAGE				m_Usage = USAGE.UNKNOWN;				// Stream purpose
				protected FIELD_TYPE		m_FieldType = FIELD_TYPE.UNKNOWN;		// Type of fields in the stream
				protected int				m_Index = 0;							// Index of the stream to differentiate identical usage streams (e.g. several UV sets)
				protected object			m_Content = null;

				#endregion

				#region PROPERTIES

				public USAGE				Usage		{ get { return m_Usage; } }
				public FIELD_TYPE			FieldType	{ get { return m_FieldType; } }
				public int					Index		{ get { return m_Index; } }
				public object				Content		{ get { return m_Content; } }

				#endregion

				#region METHODS

				public VertexStream( USAGE _Usage, FIELD_TYPE _FieldType, int _Index, object _Content )
				{
					m_Usage = _Usage;
					m_FieldType = _FieldType;
					m_Index = _Index;
					m_Content = _Content;
				}

				public VertexStream( System.IO.BinaryReader _Reader )
				{
					Load( _Reader );
				}

				public void		Load( System.IO.BinaryReader _Reader )
				{
					m_Usage = (USAGE) _Reader.ReadInt32();
					m_FieldType = (FIELD_TYPE) _Reader.ReadInt32();
					m_Index = _Reader.ReadInt32();

					int	ArraySize = _Reader.ReadInt32();
					switch ( m_FieldType )
					{
						case FIELD_TYPE.FLOAT:
							{
								float[]	T = new float[ArraySize];
								m_Content = T;
								for ( int i=0; i < ArraySize; i++ )
								{
									T[i] = _Reader.ReadSingle();
								}
							}
							break;
						case FIELD_TYPE.FLOAT2:
							{
								Vector2[]	T = new Vector2[ArraySize];
								m_Content = T;
								for ( int i=0; i < ArraySize; i++ )
								{
									T[i].X = _Reader.ReadSingle();
									T[i].Y = _Reader.ReadSingle();
								}
							}
							break;
						case FIELD_TYPE.FLOAT3:
							{
								Vector3[]	T = new Vector3[ArraySize];
								m_Content = T;
								for ( int i=0; i < ArraySize; i++ )
								{
									T[i].X = _Reader.ReadSingle();
									T[i].Y = _Reader.ReadSingle();
									T[i].Z = _Reader.ReadSingle();
								}
							}
							break;
						case FIELD_TYPE.FLOAT4:
							{
								Vector4[]	T = new Vector4[ArraySize];
								m_Content = T;
								for ( int i=0; i < ArraySize; i++ )
								{
									T[i].X = _Reader.ReadSingle();
									T[i].Y = _Reader.ReadSingle();
									T[i].Z = _Reader.ReadSingle();
									T[i].W = _Reader.ReadSingle();
								}
							}
							break;
						case FIELD_TYPE.UINT32:
							{
								UInt32[]	T = new UInt32[ArraySize];
								m_Content = T;
								for ( int i=0; i < ArraySize; i++ )
								{
									T[i] = _Reader.ReadUInt32();
								}
							}
							break;
					}
				}

				public void		Save( System.IO.BinaryWriter _Writer )
				{
					_Writer.Write( (int) m_Usage );
					_Writer.Write( (int) m_FieldType );
					_Writer.Write( m_Index );

					int	ArraySize = (m_Content as Array).Length;
					_Writer.Write( ArraySize );

					switch ( m_FieldType )
					{
						case FIELD_TYPE.FLOAT:
							{
								float[]	T = m_Content as float[];
								for ( int i=0; i < ArraySize; i++ )
								{
									_Writer.Write( T[i] );
								}
							}
							break;
						case FIELD_TYPE.FLOAT2:
							{
								Vector2[]	T = m_Content as Vector2[];
								for ( int i=0; i < ArraySize; i++ )
								{
									_Writer.Write( T[i].X );
									_Writer.Write( T[i].Y );
								}
							}
							break;
						case FIELD_TYPE.FLOAT3:
							{
								Vector3[]	T = m_Content as Vector3[];
								for ( int i=0; i < ArraySize; i++ )
								{
									_Writer.Write( T[i].X );
									_Writer.Write( T[i].Y );
									_Writer.Write( T[i].Z );
								}
							}
							break;
						case FIELD_TYPE.FLOAT4:
							{
								Vector4[]	T = m_Content as Vector4[];
								for ( int i=0; i < ArraySize; i++ )
								{
									_Writer.Write( T[i].X );
									_Writer.Write( T[i].Y );
									_Writer.Write( T[i].Z );
									_Writer.Write( T[i].W );
								}
							}
							break;
						case FIELD_TYPE.UINT32:
							{
								UInt32[]	T = m_Content as UInt32[];
								for ( int i=0; i < ArraySize; i++ )
								{
									_Writer.Write( T[i] );
								}
							}
							break;
					}
				}

				#endregion
			}

			public struct	Face
			{
				#region FIELDS

				public int			V0, V1, V2;

				#endregion

				#region METHODS

				public	Face( int _V0, int _V1, int _V2 )
				{
					V0 = _V0;
					V1 = _V1;
					V2 = _V2;
				}

				public void		Load( System.IO.BinaryReader _Reader )
				{
					V0 = _Reader.ReadInt32();
					V1 = _Reader.ReadInt32();
					V2 = _Reader.ReadInt32();
				}

				public void		Save( System.IO.BinaryWriter _Writer )
				{
					_Writer.Write( V0 );
					_Writer.Write( V1 );
					_Writer.Write( V2 );
				}

				#endregion
			}

			#endregion

			#region FIELDS

			protected Mesh					m_Parent = null;
			protected bool					m_bVisible = true;
			protected bool					m_bCastShadow = true;
			protected bool					m_bReceiveShadow = true;
			protected MaterialParameters	m_Parameters = null;
			protected int					m_FrameToken = -1;			// The token for the last rendered frame

			protected int					m_VerticesCount = 0;
			protected List<VertexStream>	m_Streams = new List<VertexStream>();
			protected Face[]				m_Faces = new Face[0];

			#endregion

			#region PROPERTIES

			/// <summary>
			/// Gets the parent mesh
			/// </summary>
			public Mesh					ParentMesh			{ get { return m_Parent; } }

			/// <summary>
			/// Gets or sets the visible state of that primitive, an invisible primitive won't be rendered (obviously)
			/// </summary>
			public bool					Visible				{ get { return m_bVisible; } set { m_bVisible = value; } }

			/// <summary>
			/// Gets or sets the "cast shadow" state of that primitive
			/// </summary>
			public bool					CastShadow			{ get { return m_bCastShadow; } set { m_bCastShadow = value; } }

			/// <summary>
			/// Gets or sets the "receive shadow" state of that primitive
			/// </summary>
			public bool					ReceiveShadow		{ get { return m_bReceiveShadow; } set { m_bReceiveShadow = value; } }

			/// <summary>
			/// Gets the parameters to render the primitive
			/// </summary>
			public MaterialParameters	Parameters			{ get { return m_Parameters; } }

			/// <summary>
			/// Gets the amount of vertices in the primitive
			/// </summary>
			public int					VerticesCount		{ get { return m_VerticesCount; } }

			/// <summary>
			/// Gets the amount of vertex streams in the primitive
			/// </summary>
			public int					VertexStreamsCount	{ get { return m_Streams.Count; } }

			/// <summary>
			/// Gets the vertex streams for the primitive
			/// </summary>
			public VertexStream[]		VertexStreams		{ get { return m_Streams.ToArray(); } }

			/// <summary>
			/// Gets the amount of face triangles in the primitive
			/// </summary>
			public int					FacesCount			{ get { return m_Faces.Length; } }

			/// <summary>
			/// Gets the faces of the primitive
			/// </summary>
			public Face[]				Faces				{ get { return m_Faces; } }

			#endregion

			#region METHODS

			/// <summary>
			/// Creates a primitive with parameters
			/// </summary>
			/// <param name="_Parent">The parent mesh for that primitive</param>
			/// <param name="_Parameters"></param>
			internal	Primitive( Mesh _Parent, MaterialParameters _Parameters, int _VerticesCount, int _FacesCount )
			{
				m_Parent = _Parent;
				m_Parameters = _Parameters;
				m_VerticesCount = _VerticesCount;
				m_Faces = new Face[_FacesCount];
			}

			/// <summary>
			/// Creates a primitive with parameters
			/// </summary>
			/// <param name="_Parent">The parent mesh for that primitive</param>
			/// <param name="_Parameters"></param>
			internal	Primitive( Mesh _Parent, System.IO.BinaryReader _Reader )
			{
				m_Parent = _Parent;
				Load( _Reader );
			}

			#region IDisposable Members

			public void Dispose()
			{
				throw new NotImplementedException();
			}

			#endregion

			public void			ClearVertexStreams()
			{
				m_Streams.Clear();
			}

			public VertexStream	AddVertexStream( VertexStream.USAGE _Usage, VertexStream.FIELD_TYPE _FieldType, int _StreamIndex, object _Content )
			{
				VertexStream	Result = new VertexStream( _Usage, _FieldType, _StreamIndex, _Content );
				m_Streams.Add( Result );

				return Result;
			}

			public void			Load( System.IO.BinaryReader _Reader )
			{
				// Retrieve the material parameters
				int	MaterialID = _Reader.ReadInt32();
				m_Parameters = m_Parent.m_Owner.FindMaterialParameters( MaterialID );

				// Read faces & vertices count
				int	FacesCount = _Reader.ReadInt32();
				m_VerticesCount = _Reader.ReadInt32();

				// Read faces
				m_Faces = new Face[FacesCount];
				for ( int FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ )
					m_Faces[FaceIndex].Load( _Reader );

				// Read vertex streams
				ClearVertexStreams();
				int	StreamsCount = _Reader.ReadInt32();
				for ( int StreamIndex=0; StreamIndex < StreamsCount; StreamIndex++ )
				{
					VertexStream	Stream = new VertexStream( _Reader );
					m_Streams.Add( Stream );
				}
			}

			public void			Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Parameters.ID );
				_Writer.Write( m_VerticesCount );

				// Write faces & vertices count
				_Writer.Write( m_Faces.Length );
				_Writer.Write( m_VerticesCount );

				// Write faces
				for ( int FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ )
					m_Faces[FaceIndex].Save( _Writer );

				// Write streams
				_Writer.Write( m_Streams.Count );
				foreach ( VertexStream Stream in m_Streams )
					Stream.Save( _Writer );
			}

			#endregion
		}

		#endregion

		#region FIELDS

		protected BoundingBox			m_BoundingBox = new BoundingBox();
		protected BoundingSphere		m_BoundingSphere = new BoundingSphere();
		protected List<Primitive>		m_Primitives = new List<Primitive>();
		protected bool					m_bCastShadow = true;
		protected bool					m_bReceiveShadow = true;

		// Cached world BBox
		protected bool					m_bWorldBBoxDirty = true;
		protected BoundingBox			m_WorldBBox;

		#endregion

		#region PROPERTIES

		public override NODE_TYPE	NodeType	{ get { return NODE_TYPE.MESH; } }

		public override bool		Visible
		{
			get { return m_bVisible; }
			set
			{
				base.Visible = value;

				// Also forward the visible state to each of our primitives
				foreach ( Primitive P in m_Primitives )
					P.Visible = value;
			}
		}

		/// <summary>
		/// Gets or sets the "Cast Shadow" state of that mesh and all its primitives
		/// </summary>
		public bool					CastShadow
		{
			get { return m_bCastShadow; }
			set
			{
				m_bCastShadow = value;

				// Also forward the visible state to each of our primitives
				foreach ( Primitive P in m_Primitives )
					P.CastShadow = value;
			}
		}

		/// <summary>
		/// Gets or sets the "Receive Shadow" state of that mesh and all its primitives
		/// </summary>
		public bool					ReceiveShadow
		{
			get { return m_bReceiveShadow; }
			set
			{
				m_bReceiveShadow = value;

				// Also forward the visible state to each of our primitives
				foreach ( Primitive P in m_Primitives )
					P.CastShadow = value;
			}
		}

		public BoundingBox			BBox		{ get { return m_BoundingBox; } set { m_BoundingBox = value; } }
		public BoundingBox			WorldBBox
		{
			get
			{
				if ( m_bWorldBBoxDirty )
				{	// Update world BBox
					m_WorldBBox = new BoundingBox( +float.MaxValue * Vector3.One, -float.MaxValue * Vector3.One );

					foreach ( Vector3 Corner in m_BoundingBox.GetCorners() )
					{
						Vector3	WorldCorner = Vector3.TransformCoordinate( Corner, m_Local2World );
						m_WorldBBox.Minimum = Vector3.Min( m_WorldBBox.Minimum, WorldCorner );
						m_WorldBBox.Maximum = Vector3.Max( m_WorldBBox.Maximum, WorldCorner );
					}

					m_bWorldBBoxDirty = false;
				}

				return m_WorldBBox;
			}
		}

		public BoundingSphere		BSphere		{ get { return m_BoundingSphere; } set { m_BoundingSphere = value; } }

		/// <summary>
		/// Gets the amount of primitives composing this mesh
		/// </summary>
		public int					PrimitivesCount		{ get { return m_Primitives.Count; } }

		/// <summary>
		/// Gets the array of primitives
		/// </summary>
		public Primitive[]			Primitives	{ get { return m_Primitives.ToArray(); } }

		#endregion

		#region METHODS

		internal Mesh( Scene _Owner, int _ID, string _Name, Node _Parent, Matrix _Local2Parent ) : base( _Owner, _ID, _Name, _Parent, _Local2Parent )
		{
		}

		internal Mesh( Scene _Owner, Node _Parent, System.IO.BinaryReader _Reader ) : base( _Owner, _Parent, _Reader )
		{
		}

		public void		ClearPrimitives()
		{
			foreach ( Primitive P in m_Primitives )
				P.Dispose();
			m_Primitives.Clear();
		}

		public Primitive	AddPrimitive( string _Name, MaterialParameters _Material, int _VerticesCount, int _FacesCount )
		{
			Primitive	Result = new Primitive( this, _Material, _VerticesCount, _FacesCount );
			m_Primitives.Add( Result );
			Result.Visible = m_bVisible;

			return Result;
		}

		public void		RemovePrimitive( Primitive _Primitive )
		{
			if ( !m_Primitives.Contains( _Primitive ) )
				return;

			_Primitive.Dispose();
			m_Primitives.Remove( _Primitive );
		}

		protected override void		LoadSpecific( System.IO.BinaryReader _Reader )
		{
			// Read bounding-box
			m_BoundingBox.Minimum.X = _Reader.ReadSingle();
			m_BoundingBox.Minimum.Y = _Reader.ReadSingle();
			m_BoundingBox.Minimum.Z = _Reader.ReadSingle();
			m_BoundingBox.Maximum.X = _Reader.ReadSingle();
			m_BoundingBox.Maximum.Y = _Reader.ReadSingle();
			m_BoundingBox.Maximum.Z = _Reader.ReadSingle();

			// Read bounding-sphere
			m_BoundingSphere.Center.X = _Reader.ReadSingle();
			m_BoundingSphere.Center.Y = _Reader.ReadSingle();
			m_BoundingSphere.Center.Z = _Reader.ReadSingle();
			m_BoundingSphere.Radius = _Reader.ReadSingle();

			// Write shadow states
			m_bCastShadow = _Reader.ReadBoolean();
			m_bReceiveShadow = _Reader.ReadBoolean();

			// Read primitives
			m_Primitives.Clear();
			int	PrimitivesCount = _Reader.ReadInt32();
			for ( int PrimitiveIndex=0; PrimitiveIndex < PrimitivesCount; PrimitiveIndex++ )
			{
				Primitive	P = new Primitive( this, _Reader );
				m_Primitives.Add( P );
			}
		}

		protected override void		SaveSpecific( System.IO.BinaryWriter _Writer )
		{
			// Write bounding-box
			_Writer.Write( m_BoundingBox.Minimum.X );
			_Writer.Write( m_BoundingBox.Minimum.Y );
			_Writer.Write( m_BoundingBox.Minimum.Z );
			_Writer.Write( m_BoundingBox.Maximum.X );
			_Writer.Write( m_BoundingBox.Maximum.Y );
			_Writer.Write( m_BoundingBox.Maximum.Z );

			// Write bounding-sphere
			_Writer.Write( m_BoundingSphere.Center.X );
			_Writer.Write( m_BoundingSphere.Center.Y );
			_Writer.Write( m_BoundingSphere.Center.Z );
			_Writer.Write( m_BoundingSphere.Radius );

			// Write shadow states
			_Writer.Write( m_bCastShadow );
			_Writer.Write( m_bReceiveShadow );

			// Write primitives
			_Writer.Write( m_Primitives.Count );
			foreach ( Primitive P in m_Primitives )
				P.Save( _Writer );
		}

		protected override void DisposeSpecific()
		{
			base.DisposeSpecific();
			ClearPrimitives();
		}

		protected override void PropagateDirtyState()
		{
			base.PropagateDirtyState();
			m_bWorldBBoxDirty = true;
		}

		#endregion
	}
}
