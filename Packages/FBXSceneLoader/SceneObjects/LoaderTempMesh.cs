using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;

namespace FBX.SceneLoader.Objects
{
	public class	LoaderTempMesh : LoaderTempSceneObject
	{
		#region NESTED TYPES

		/// <summary>
		/// Defines the type of data associated to a vertex
		/// </summary>
		public enum		VERTEX_INFO_TYPE
		{
			UNKNOWN,
			POSITION,	// Point
			NORMAL,		// Vector
			TANGENT,	// Vector
			BINORMAL,	// Vector
			TEXCOORD1D,	// float
			TEXCOORD2D,	// Vector2D
			TEXCOORD3D,	// Vector
			COLOR,		// Vector4D => to be cast into UInt32
			COLOR_HDR,	// Vector4D
			SMOOTHING,	// int
		}

		[Flags]
		public enum	TANGENT_SPACE_AVAILABILITY
		{
			NOTHING = 0,
			UVs = 1,
			NORMAL = 2,
			TANGENT = 4,
			BINORMAL = 8,
			FULL = NORMAL | TANGENT | BINORMAL,
			TANGENT_SPACE_ONLY = TANGENT | BINORMAL,
		}

		/// <summary>
		/// Internal class used to consolidate meshes
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "Index={Index} SMG={SmoothingGroups} V0={VertexIndex0} V1={VertexIndex1} V2={VertexIndex2}" )]
		public class		ConsolidatedFace
		{
			public int					Index = -1;

			// Original data
			public int					VertexIndex0 = -1;
			public int					VertexIndex1 = -1;
			public int					VertexIndex2 = -1;

			// Generated data (WARNING! => valid only after mesh consolidation)
			public ConsolidatedVertex	V0 = null;
			public ConsolidatedVertex	V1 = null;
			public ConsolidatedVertex	V2 = null;

			// Optional, for consolidation
			public int					SmoothingGroups = 1;
			public FBXImporter.Material	Material = null;

			// Optional, for TS generation
			public Vector				Normal = null;
			public Vector				Tangent = null;
			public Vector				BiNormal = null;
		};

		/// <summary>
		/// Internal class used to consolidate meshes
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "Index={m_Index} Position={m_PositionInfo.m_Value} SMG={m_SmoothingGroups}" )]
		public class		ConsolidatedVertex
		{
			#region NESTED TYPES

			/// <summary>
			/// Stores an information about the vertex
			/// </summary>
			[System.Diagnostics.DebuggerDisplay( "Type={m_Type} Value={m_Value}" )]
			public class	VertexInfo
			{
				public LoaderTempMesh			m_Owner = null;					// The mesh that owns the info
				public FBXImporter.LayerElement	m_SourceLayerElement = null;	// The layer element source for that info

				public VERTEX_INFO_TYPE	m_Type = VERTEX_INFO_TYPE.UNKNOWN;		// The type of info
				public int				m_Index = 0;							// The index of this info
				public object			m_Value = null;							// The value of the info

				// Comparison flags...
				public static bool		ms_CompareSmoothingGroups = false;		// Tells if we should differentiate vertices by their smoothing groups
				public static bool		ms_CompareUVs = false;					// Tells if we should differentiate vertices by their UVs
				public static bool		ms_CompareColors = false;				// Tells if we should differentiate vertices by their colors
				public static bool		ms_CompareTangentSpace = false;			// Tells if we should differentiate vertices by their tangent space

				/// <summary>
				/// Compares with another vertex info of the same type
				/// The comparison strategy is to declare the infos as "equal" if they're not of the same type
				///  or don't have the same index.
				/// The only case when these infos are not equal is when they differ by value.
				/// Thus, it allows us to compare all infos of a vertex against all the infos of another vertex
				///  and to know if the vertices are actually equal to each other because they have the exact same values.
				/// </summary>
				/// <param name="_o"></param>
				/// <returns></returns>
				public bool  Compare( VertexInfo _Info )
				{
					if ( _Info.m_Type != m_Type )
						return	true;	// Not a vertex info of the same type...
					if ( _Info.m_Index != m_Index )
						return	true;

					switch ( m_Type )
					{
						case VERTEX_INFO_TYPE.POSITION:
							return (_Info.m_Value as Point) == (m_Value as Point);

						case VERTEX_INFO_TYPE.NORMAL:
						case VERTEX_INFO_TYPE.TANGENT:
						case VERTEX_INFO_TYPE.BINORMAL:
							return	!ms_CompareTangentSpace || (_Info.m_Value as Vector) == (m_Value as Vector);

						case VERTEX_INFO_TYPE.TEXCOORD3D:
							return	!ms_CompareUVs || (_Info.m_Value as Vector) == (m_Value as Vector);

						case VERTEX_INFO_TYPE.TEXCOORD2D:
							return	!ms_CompareUVs || (_Info.m_Value as Vector2D) == (m_Value as Vector2D);

						case VERTEX_INFO_TYPE.TEXCOORD1D:
							return	!ms_CompareUVs || Math.Abs( (float) _Info.m_Value - (float) m_Value ) < 1e-6f;

						case VERTEX_INFO_TYPE.COLOR:
							return	!ms_CompareColors || _Info.m_Value.Equals( m_Value );

						case VERTEX_INFO_TYPE.COLOR_HDR:
							return	!ms_CompareColors || (_Info.m_Value as Vector4D) == (m_Value as Vector4D);
					}

					return	true;
				}
			};

			#endregion

			// The vertex index
			public int				m_Index = -1;

			// The owner face's smoothing groups
			public int				m_SmoothingGroups = 1;

			// The additional infos associated to the vertex
			public List<VertexInfo>	m_Infos = new List<VertexInfo>();

			// Special infos for tangent space generation
			public VertexInfo		m_PositionInfo = null;
			public VertexInfo		m_NormalInfo = null;
			public VertexInfo		m_TangentInfo = null;
			public VertexInfo		m_BinormalInfo = null;

			// Comparison flags...
			public static bool		ms_CompareSmoothingGroups = false;		// Tells if we should differentiate vertices by their smoothing groups

			/// <summary>
			/// Compares with another vertex
			/// </summary>
			/// <param name="_o"></param>
			/// <returns></returns>
			public bool Compare( ConsolidatedVertex _V )
			{
				// Compare smoothing groups
				if ( ms_CompareSmoothingGroups && (_V.m_SmoothingGroups & m_SmoothingGroups) == 0 )
					return	false;

				if ( m_Infos.Count != _V.m_Infos.Count )
					throw new Exception( "2 vertices from the same mesh have a different count of infos!" );

				for ( int InfoIndex=0; InfoIndex < m_Infos.Count; InfoIndex++ )
				{
					VertexInfo	V0 = m_Infos[InfoIndex];
					VertexInfo	V1 = _V.m_Infos[InfoIndex];
					if ( V0.m_Type != V1.m_Type )
						throw new Exception( "2 vertices from the same mesh have infos at the same index but with different types!" );

					if ( !V0.Compare( V1 ) )
						return	false;
				}

				return	true;
			}
		};

		public class		Primitive : LoaderTempSceneObject
		{
			#region NESTED TYPES

			[System.Diagnostics.DebuggerDisplay( "Name={SourceLayerElement.Name} Type={StreamType} Index={Index} Length={m_Stream.Length}" )]
			public class		VertexStream
			{
				#region FIELDS

				protected FBXImporter.LayerElement	m_SourceLayerElement = null;// The source layer element that yielded this vertex stream
				protected VERTEX_INFO_TYPE	m_Type = VERTEX_INFO_TYPE.UNKNOWN;	// The vertex stream type
				protected int				m_Index = 0;						// The stream index
				protected object[]			m_Stream = null;					// The stream data

				#endregion

				#region PROPERTIES

				/// <summary>
				/// Gets the source layer element that yielded this vertex stream
				/// </summary>
				public FBXImporter.LayerElement	SourceLayerElement	{ get { return m_SourceLayerElement; } }

				/// <summary>
				/// Gets the type of data encoded by the stream
				/// </summary>
				public VERTEX_INFO_TYPE		StreamType	{ get { return m_Type; } }

				/// <summary>
				/// Gets the index of the stream (useful if you have several UV sets for example)
				/// </summary>
				public int					Index		{ get { return m_Index; } }

				/// <summary>
				/// Gets the array of data stored by the stream
				/// </summary>
				public object[]				Stream		{ get { return m_Stream; } }

				#endregion

				#region METHODS

				public	VertexStream( FBXImporter.LayerElement _Source, VERTEX_INFO_TYPE _Type, int _Index, int _StreamLength )
				{
					m_SourceLayerElement = _Source;
					m_Type = _Type;
					m_Index = _Index;
					m_Stream = new object[_StreamLength];
				}

				#endregion
			};

			#endregion

			#region FIELDS

			protected LoaderTempMesh			m_OwnerMesh = null;

			protected FBXImporter.Material		m_Material = null;
			protected Material					m_OverrideMaterial = null;
			protected List<ConsolidatedFace>	m_Faces = new List<ConsolidatedFace>();
			protected List<ConsolidatedVertex>	m_Vertices = new List<ConsolidatedVertex>();

			protected VertexStream[]			m_Streams = null;

			#endregion

			#region PROPERTIES

			public FBXImporter.Material	Material
			{
				get { return m_Material; }
			}

			public Material				OverrideMaterial
			{
				get { return m_OverrideMaterial; }
				set { m_OverrideMaterial = value; }
			}

			public int					VerticesCount
			{
				get { return m_Vertices.Count; }
			}

			public int					FacesCount
			{
				get { return m_Faces.Count; }
			}

			public ConsolidatedFace[]	Faces
			{
				get { return m_Faces.ToArray(); }
			}

			public VertexStream[]		VertexStreams
			{
				get
				{
					if ( m_Streams != null )
						return	m_Streams;

					// We build every stream based on the first vertex's infos (assuming all vertices have the same infos in the same order, if not, that's a mistake anyway)
					ConsolidatedVertex V0 = m_Vertices[0];

					// Build the vertex streams
					List<VertexStream>	Streams = new List<VertexStream>();
					foreach ( ConsolidatedVertex.VertexInfo Info in V0.m_Infos )
						Streams.Add( new VertexStream( Info.m_SourceLayerElement, Info.m_Type, Info.m_Index, m_Vertices.Count ) );

					// Fill up the streams
					for ( int VertexIndex=0; VertexIndex < m_Vertices.Count; VertexIndex++ )
					{
						ConsolidatedVertex	V = m_Vertices[VertexIndex];
						for ( int InfoIndex=0; InfoIndex < V.m_Infos.Count; InfoIndex++ )
						{
							ConsolidatedVertex.VertexInfo	Info = V.m_Infos[InfoIndex];
							Streams[InfoIndex].Stream[VertexIndex] = Info.m_Value;
						}
					}

					// Cache the result
					m_Streams = Streams.ToArray();

					return	m_Streams;
				}
			}

			#endregion

			#region METHODS

			public Primitive( LoaderTempMesh _OwnerMesh, SceneLoader _Owner, string _Name, FBXImporter.Material _Material ) : base( _Owner, _Name )
			{
				m_OwnerMesh = _OwnerMesh;
				m_Material = _Material;
// 				if ( m_Material == null )
// 					throw new Exception( "Invalid material for primitive \"" + _Name + "\"!" );
			}

			public void		AddFace( ConsolidatedFace _Face )
			{
				m_Faces.Add( _Face );
			}

			#region Mesh Consolidation

			/// <summary>
			/// Consolidates the mesh defined by the primitive's array of faces
			/// This will actually merge vertices that are considered equal, thus reducing their number
			/// Consolidated faces will be re-ordered to map the new consolidated vertices
			/// </summary>
			public void		Consolidate()
			{
				//////////////////////////////////////////////////////////////////////////
				// Build a list of vertices that have the same characteristics, and faces that reference them
				//

				// This is the map that maps a vertex index from the table of POSITION vertices into a list of consolidated vertices
				// Through this list, we can choose which existing consolidated vertex is equivalent to a given vertex.
				// If none can be found, then a new consolidated vertex is created
				Dictionary<int,List<ConsolidatedVertex>>	OriginalVertexIndex2ConsolidatedVertices = new Dictionary<int,List<ConsolidatedVertex>>();

				foreach ( ConsolidatedFace F in m_Faces )
				{
					// -------------------------------------------------------------------------------
					// Build a new temporary consolidated vertex for every face vertex and insert it into the list
					F.V0 = InsertConsolidatedVertex( m_Vertices, OriginalVertexIndex2ConsolidatedVertices, F.VertexIndex0, m_OwnerMesh.BuildConsolidatedVertex( F, 0, F.VertexIndex0 ) );
					F.V1 = InsertConsolidatedVertex( m_Vertices, OriginalVertexIndex2ConsolidatedVertices, F.VertexIndex1, m_OwnerMesh.BuildConsolidatedVertex( F, 1, F.VertexIndex1 ) );
					F.V2 = InsertConsolidatedVertex( m_Vertices, OriginalVertexIndex2ConsolidatedVertices, F.VertexIndex2, m_OwnerMesh.BuildConsolidatedVertex( F, 2, F.VertexIndex2 ) );
				}
			}

			/// <summary>
			/// Inserts the provided consolidated vertex into the list of vertices
			/// If there already exists a matching vertex in the list of consolidated vertices, then this vertex is returned instead
			/// </summary>
			/// <param name="_ConsolidatedVertices">The list where to insert the vertex in case it does not already exist</param>
			/// <param name="_Dictionary">The dictionary yielding the list of consolidated vertices associated to each original position vertex (as the only forever common data of all vertices --consolidated or not-- is their position)</param>
			/// <param name="_OriginalVertexIndex">The index of the original position vertex</param>
			/// <param name="_Vertex">The consolidated vertex to insert</param>
			/// <returns>The inserted consolidated vertex</returns>
			protected ConsolidatedVertex	InsertConsolidatedVertex( List<ConsolidatedVertex> _ConsolidatedVertices, Dictionary<int,List<ConsolidatedVertex>> _Dictionary, int _OriginalVertexIndex, ConsolidatedVertex _Vertex )
			{
				// Check there already is a list of vertices
				if ( !_Dictionary.ContainsKey( _OriginalVertexIndex ) )
					_Dictionary[_OriginalVertexIndex] = new List<ConsolidatedVertex>();

				List<ConsolidatedVertex>	ExistingVertices = _Dictionary[_OriginalVertexIndex];

				if ( !m_OwnerMesh.m_Owner.ConsolidateMeshes )
				{	// Only check if there already is a vertex at this index
					if ( ExistingVertices.Count > 0 )
						return	ExistingVertices[0];	// Return the only vertex there will ever be at this index
				}
				else
				{	// Check all existing vertices for a match
					foreach ( ConsolidatedVertex ExistingVertex in ExistingVertices )
						if ( ExistingVertex.Compare( _Vertex ) )
							return	ExistingVertex;	// There is a match! Use this vertex instead
				}

				// There was no match, so we insert the provided vertex
				_Vertex.m_Index = _ConsolidatedVertices.Count;
				_ConsolidatedVertices.Add( _Vertex );
				ExistingVertices.Add( _Vertex );

				return	_Vertex;
			}

			#endregion

			#endregion
		};

		/// <summary>
		/// Temporary structure in which we store a face and its influence on a given vertex
		/// This structure is meant to be used in an array of values attached to a vertex
		/// </summary>
		[System.Diagnostics.DebuggerDisplay( "Weight={m_Weight} Index={m_Face.Index}" )]
		private class		SharedFace
		{
			public ConsolidatedFace	m_Face = null;		// The face referencing this vertex
			public float			m_Weight = 0.0f;	// The weight of this face (in our case, the angle of the face at the given vertex)

			public	SharedFace( ConsolidatedFace _Face, Point _V0, Point _V1, Point _V2 )
			{
				m_Face = _Face;

				// Retrieve the angle formed by the 2 vectors and use it as weight for this face's influence
				Vector	D0 = (_V1 - _V0).Normalize();
				Vector	D1 = (_V2 - _V0).Normalize();
				float	fDot = D0 | D1;
				m_Weight = (float) Math.Acos( fDot );
				if ( float.IsNaN( m_Weight ) )
					m_Weight = 0.0f;	// Occurs when D0 & D1 are very very small or really close to each other (usually, degenerate faces so it's okay if we skip them anyway)
			}
		};

		/// <summary>
		/// A small structure that stores the owner mesh of an external layer element
		/// External layer elements are layer elements that belong to another mesh (i.e. a slave mesh).
		/// The other mesh is a slave to our mesh and adds the layer lement to our own layer elements so
		///		we take it into account when consolidating this mesh.
		/// Doing this allows slave meshes to keep a common structure (our mesh) (i.e. the instance) + 
		///		adding their own information, which is coherent with the master structure (meaning for
		///		example that a slave mesh can add its own vertex colors to our mesh and use the whole
		///		thing to display an instance of the master mesh + vertex colors)
		/// </summary>
		public class	ExternalLayerElement
		{
			public LoaderTempMesh					m_Owner;
			public FBXImporter.LayerElement			m_LayerElement = null;
		};

		/// <summary>
		/// A small structure that stores the owner mesh of a reference layer element
		/// Reference layer elements are layer elements that belong to another mesh (i.e. a master mesh).
		/// The other mesh is our master mesh and we're using a reference to one of its layer elements
		///		in order to factorize the layer element's data.
		/// </summary>
		public class	ReferenceLayerElement
		{
			public LoaderTempMesh					m_Owner;
			public FBXImporter.LayerElement			m_LayerElement = null;
		};

		#endregion

		#region FIELDS

		protected LoaderTempMesh					m_MasterMesh = null;	// Non-null if this mesh is an instance mesh

		protected Point[]							m_Vertices = null;
		protected FBXImporter.NodeMesh.Triangle[]	m_Faces = null;
		protected List<FBXImporter.LayerElement>	m_LayerElements = new List<FBXImporter.LayerElement>();
		protected List<ExternalLayerElement>		m_LayerElementsExternal = new List<ExternalLayerElement>();
		protected List<ReferenceLayerElement>		m_LayerElementsReference = new List<ReferenceLayerElement>();

		// The dummy layer elements for position & tangent space
		protected FBXImporter.LayerElement			m_LayerElementPosition = null;
		protected FBXImporter.LayerElement			m_LayerElementNormal = null;
		protected FBXImporter.LayerElement			m_LayerElementTangent = null;
		protected FBXImporter.LayerElement			m_LayerElementBiNormal = null;

		// The list of meshes slave of this mesh
		protected List<LoaderTempMesh>				m_SlaveMeshes = new List<LoaderTempMesh>();
		protected Dictionary<LoaderTempMesh,bool>	m_SlaveMesh2Registered = new Dictionary<LoaderTempMesh,bool>();	// The table that indicates if a mesh is already slave of this mesh

		// These are the 2 lists of collapsed layer elements (ours + external ones from other slave mesh instances)
		// These lists are built in the PerformConsolidation() method
		protected FBXImporter.LayerElement[]		m_CollapsedLayerElements = null;
		protected LoaderTempMesh[]					m_CollapsedLayerElementMeshes = null;

		protected Material							m_OverrideMaterial = null;

		protected BoundingBox						m_BBox = null;
		protected Matrix4x4							m_Pivot = null;
		protected int								m_UVSetsCount = 0;

		// Generated data
		protected List<Primitive>					m_Primitives = new List<Primitive>();

		#endregion

		#region PROPERTIES

		public bool								IsMaster
		{
			get { return m_MasterMesh == null; }
		}

		public LoaderTempMesh					MasterMesh
		{
				get { return m_MasterMesh; }
		}

		public Point[]							Vertices
		{
			get { return m_Vertices; }
		}

		public FBXImporter.NodeMesh.Triangle[]	Faces
		{
			get { return m_Faces; }
		}

		public FBXImporter.LayerElement[]		LayerElements
		{
			get
			{
				// The layer elements of a mesh are its own + the referenced ones from the master mesh
				// (NOTE: Not the external ones as they're not part of this mesh)
				List<FBXImporter.LayerElement>	Result = new List<FBXImporter.LayerElement>();
				foreach ( ReferenceLayerElement RLE in m_LayerElementsReference )
					Result.Add( RLE.m_LayerElement );
				Result.AddRange( m_LayerElements );

				return Result.ToArray();
			}
		}

		public Primitive[]			ConsolidatedPrimitives
		{
			get { return m_Primitives.ToArray(); }
		}

		/// <summary>
		/// Gets or sets the mesh's bounding box
		/// </summary>
		public BoundingBox	BoundingBox
		{
			get { return m_BBox; }
			set { m_BBox = value; }
		}

		/// <summary>
		/// Gets or sets the mesh's pivot
		/// </summary>
		/// <remarks>If the pivot is set, the vertices are transformed by this matrix (a.k.a. the "Reset X-Form" operation)</remarks>
		public Matrix4x4	Pivot
		{
			get { return m_Pivot; }
			set { m_Pivot = value; }
		}

		#endregion

		#region METHODS

		public LoaderTempMesh( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
		{
		}

		/// <summary>
		/// Sets the mesh's array of vertices
		/// </summary>
		/// <param name="_Vertices"></param>
		public void	SetVertices( Point[] _Vertices )
		{
			m_Vertices = _Vertices;
		}

		/// <summary>
		/// Sets the mesh's array of faces
		/// </summary>
		/// <param name="_Faces"></param>
		public void	SetFaces( FBXImporter.NodeMesh.Triangle[] _Faces )
		{
			m_Faces = _Faces;
		}

		/// <summary>
		/// Adds a layer element to the mesh, hence adding a new entry to the vertex buffer
		/// </summary>
		/// <param name="_LayerElement"></param>
		public void		AddLayerElement( FBXImporter.LayerElement _LayerElement )
		{
			if ( _LayerElement == null )
				throw new Exception( "Invalid layer element!" );

			// Compact identical UV sets
			if ( m_Owner.CompactUVs && _LayerElement.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.UV )
			{
				m_UVSetsCount++;

				// DEBUG
// 					if ( m_Name == "Body" )
// 						m_Pivot = null;
				// DEBUG

				// Compare the new layer element with any existing UV element
				if ( m_UVSetsCount > m_Owner.MinUVsCount )
					foreach ( FBXImporter.LayerElement Element in m_LayerElements )
						if ( Element.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.UV )
						{
							object[]	ExistingUV = Element.ToArray();
							object[]	NewUV = _LayerElement.ToArray();	// This array is cached, so the cost of evaluation is only one

							if ( ExistingUV.Length != NewUV.Length )
								continue;	// They already differ by length...

							// Compare each entry of the arrays
							bool	bEqual = true;
							for ( int i=0; i < ExistingUV.Length; i++ )
							{
								Vector2D	UV0 = ExistingUV[i] as Vector2D;
								Vector2D	UV1 = NewUV[i] as Vector2D;
								if ( UV0 != UV1 )
								{	// They differ!
									bEqual = false;
									break;
								}
							}

							if ( bEqual )
								return;	// Both UV sets are equal, so we don't add the new one...
						}
			}

			// Add this as a new entry...
			m_LayerElements.Add( _LayerElement );
		}

		/// <summary>
		/// Adds a layer element from another (slave) mesh
		/// </summary>
		/// <param name="_OwnerMesh">The mesh owning the layer element to add</param>
		/// <param name="_LayerElement">The external layer element</param>
		public void		AddExternalLayerElement( LoaderTempMesh _OwnerMesh, FBXImporter.LayerElement _LayerElement )
		{
			ExternalLayerElement	ELE = new ExternalLayerElement();
									ELE.m_Owner = _OwnerMesh;
									ELE.m_LayerElement = _LayerElement;

			m_LayerElementsExternal.Add( ELE );

			// Add this mesh to our list of slave meshes
			if ( m_SlaveMesh2Registered.ContainsKey( _OwnerMesh ) )
				return;	// Already registered!

			m_SlaveMeshes.Add( _OwnerMesh );
			m_SlaveMesh2Registered[_OwnerMesh] = true;
		}

		/// <summary>
		/// Replaces a layer element from this mesh by a reference to another element from another mesh
		/// </summary>
		/// <param name="_LayerElementSource">The source layer element to replace</param>
		/// <param name="_OwnerMesh">The mesh that owns the referenced layer element</param>
		/// <param name="_LayerElementReference">The layer element to reference in place of our own layer element</param>
		public void		ReplaceLayerElementByAReference( FBXImporter.LayerElement _LayerElementSource, LoaderTempMesh _OwnerMesh, FBXImporter.LayerElement _LayerElementReference )
		{
			m_LayerElements.Remove( _LayerElementSource );

			ReferenceLayerElement	RLE = new ReferenceLayerElement();
									RLE.m_Owner = _OwnerMesh;
									RLE.m_LayerElement = _LayerElementReference;

			m_LayerElementsReference.Add( RLE );
		}

		#region Procedural Creation

		/// <summary>
		/// Creates a box mesh
		/// </summary>
		/// <param name="_BBox">The mesh's box in local space</param>
		/// <param name="_Material">The material to use for the box</param>
		public void		CreateBox( BoundingBox _BBox, Material _Material )
		{
			m_OverrideMaterial = _Material;

			// Build vertices
			Point[]	Vertices = new Point[8];

			Vertices[0] = new Point( _BBox.m_Min.x + 0 * _BBox.DimX, _BBox.m_Min.y + 0 * _BBox.DimY, _BBox.m_Min.z + 0 * _BBox.DimZ );
			Vertices[1] = new Point( _BBox.m_Min.x + 1 * _BBox.DimX, _BBox.m_Min.y + 0 * _BBox.DimY, _BBox.m_Min.z + 0 * _BBox.DimZ );
			Vertices[2] = new Point( _BBox.m_Min.x + 1 * _BBox.DimX, _BBox.m_Min.y + 1 * _BBox.DimY, _BBox.m_Min.z + 0 * _BBox.DimZ );
			Vertices[3] = new Point( _BBox.m_Min.x + 0 * _BBox.DimX, _BBox.m_Min.y + 1 * _BBox.DimY, _BBox.m_Min.z + 0 * _BBox.DimZ );
			Vertices[4] = new Point( _BBox.m_Min.x + 0 * _BBox.DimX, _BBox.m_Min.y + 0 * _BBox.DimY, _BBox.m_Min.z + 1 * _BBox.DimZ );
			Vertices[5] = new Point( _BBox.m_Min.x + 1 * _BBox.DimX, _BBox.m_Min.y + 0 * _BBox.DimY, _BBox.m_Min.z + 1 * _BBox.DimZ );
			Vertices[6] = new Point( _BBox.m_Min.x + 1 * _BBox.DimX, _BBox.m_Min.y + 1 * _BBox.DimY, _BBox.m_Min.z + 1 * _BBox.DimZ );
			Vertices[7] = new Point( _BBox.m_Min.x + 0 * _BBox.DimX, _BBox.m_Min.y + 1 * _BBox.DimY, _BBox.m_Min.z + 1 * _BBox.DimZ );

			SetVertices( Vertices );

			// Build faces
			FBXImporter.NodeMesh.Triangle[]	Faces = new FBXImporter.NodeMesh.Triangle[2*6];

			Faces[0] = new FBXImporter.NodeMesh.Triangle( 7, 4, 5, 0 );		// Front
			Faces[1] = new FBXImporter.NodeMesh.Triangle( 7, 5, 6, 1 );
			Faces[2] = new FBXImporter.NodeMesh.Triangle( 6, 5, 1, 2 );		// Right
			Faces[3] = new FBXImporter.NodeMesh.Triangle( 6, 1, 2, 3 );
			Faces[4] = new FBXImporter.NodeMesh.Triangle( 3, 7, 6, 4 );		// Top
			Faces[5] = new FBXImporter.NodeMesh.Triangle( 3, 6, 2, 5 );
			Faces[6] = new FBXImporter.NodeMesh.Triangle( 3, 0, 4, 6 );		// Left
			Faces[7] = new FBXImporter.NodeMesh.Triangle( 3, 4, 7, 7 );
			Faces[8] = new FBXImporter.NodeMesh.Triangle( 2, 1, 0, 8 );		// Back
			Faces[9] = new FBXImporter.NodeMesh.Triangle( 2, 0, 3, 9 );
			Faces[10] = new FBXImporter.NodeMesh.Triangle( 4, 0, 1, 10 );	// Bottom
			Faces[11] = new FBXImporter.NodeMesh.Triangle( 4, 1, 5, 11 );

			SetFaces( Faces );

			// Build smoothing groups
			object[]	SmoothingGroups = new object[]	{	1, 1,
															2, 2,
															4, 4,
															8, 8,
															16, 16,
															32, 32,
															64, 64 };

			FBXImporter.LayerElement	Element = new FBXImporter.LayerElement( "Smg", FBXImporter.LayerElement.ELEMENT_TYPE.SMOOTHING, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE, 0 );
										Element.SetArrayOfData( SmoothingGroups );

			AddLayerElement( Element );

			// Build UV set (compulsory otherwise TS can't be generated and an exception may occur)
			object[]	UVs = new object[]
			{
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 0.0f, 1.0f ), new Vector2D( 1.0f, 1.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 1.0f, 1.0f ), new Vector2D( 1.0f, 0.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 0.0f, 1.0f ), new Vector2D( 1.0f, 1.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 1.0f, 1.0f ), new Vector2D( 1.0f, 0.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 0.0f, 1.0f ), new Vector2D( 1.0f, 1.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 1.0f, 1.0f ), new Vector2D( 1.0f, 0.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 0.0f, 1.0f ), new Vector2D( 1.0f, 1.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 1.0f, 1.0f ), new Vector2D( 1.0f, 0.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 0.0f, 1.0f ), new Vector2D( 1.0f, 1.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 1.0f, 1.0f ), new Vector2D( 1.0f, 0.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 0.0f, 1.0f ), new Vector2D( 1.0f, 1.0f ),
				new Vector2D( 0.0f, 0.0f ), new Vector2D( 1.0f, 1.0f ), new Vector2D( 1.0f, 0.0f ),
			};

			Element = new FBXImporter.LayerElement( "UVs", FBXImporter.LayerElement.ELEMENT_TYPE.UV, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX, 0 );
			Element.SetArrayOfData( UVs );

			AddLayerElement( Element );

			// That's all, we don't build anything else as it will be done by the mesh consolidation...
		}

		#endregion

		/// <summary>
		/// This builds the mesh primitives that we'll be able to use at runtime
		/// </summary>
		public void		BuildPrimitives()
		{
// 			if ( m_Owner.GenerateTriangleStrips )
// 				throw new Exception( "Triangle Strips are not supported yet!" );

			// Setup the comparison flags used for consolidation
			ConsolidatedVertex.ms_CompareSmoothingGroups = m_Owner.ConsolidateSplitBySMG;
			ConsolidatedVertex.VertexInfo.ms_CompareUVs = m_Owner.ConsolidateSplitByUV;
			ConsolidatedVertex.VertexInfo.ms_CompareColors = m_Owner.ConsolidateSplitByColor;

			//////////////////////////////////////////////////////////////////////////
			// Reset X-Form
			if ( m_Pivot != null )
			{
				Point[]	NewVertices = new Point[m_Vertices.Length];
				for ( int VertexIndex=0; VertexIndex < m_Vertices.Length; VertexIndex++ )
					NewVertices[VertexIndex] = m_Vertices[VertexIndex] * m_Pivot;

				m_Vertices = NewVertices;
			}

			//////////////////////////////////////////////////////////////////////////
			// Build the original list of consolidated faces
			List<ConsolidatedFace>	Faces = new List<ConsolidatedFace>();
			foreach ( FBXImporter.NodeMesh.Triangle T in m_Faces )
			{
				ConsolidatedFace	NewFace = new ConsolidatedFace();
									NewFace.Index = Faces.Count;
									NewFace.VertexIndex0 = T.Vertex0;
									NewFace.VertexIndex1 = T.Vertex1;
									NewFace.VertexIndex2 = T.Vertex2;

				Faces.Add( NewFace );
			}

			//////////////////////////////////////////////////////////////////////////
			// Attempt to retrieve smoothing group & material data
			foreach ( FBXImporter.LayerElement Element in m_LayerElements )
			{
				if ( Element.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.MATERIAL )
				{
					if ( m_OverrideMaterial != null )
						continue;	// Ignore specific material if we have an override...

					// Retrieve the array of data
					object[]	Data = Element.ToArray();
					switch ( Element.MappingType )
					{
						case FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE:
							for ( int FaceIndex=0; FaceIndex < Faces.Count; FaceIndex++ )
								Faces[FaceIndex].Material = (FBXImporter.Material) Data[FaceIndex];
							break;

						case FBXImporter.LayerElement.MAPPING_TYPE.ALL_SAME:
							{
								FBXImporter.Material	Mat = (FBXImporter.Material) Data[0];
								foreach ( ConsolidatedFace F in Faces )
									F.Material = Mat;
								break;
							}

						default:
							throw new Exception( "Found a layer element of type \"MATERIAL\" with unsupported \"" + Element.MappingType + "\" mapping type!\r\n(Only BY_POLYGON & ALL_SAME mapping modes are supported!)" );
					}
				}
				else if ( Element.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.SMOOTHING )
				{
					// Retrieve the array of data
					object[]	Data = Element.ToArray();
					switch ( Element.MappingType )
					{
						case FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE:
							for ( int FaceIndex=0; FaceIndex < Faces.Count; FaceIndex++ )
								Faces[FaceIndex].SmoothingGroups = (int) Data[FaceIndex];
							break;

						case FBXImporter.LayerElement.MAPPING_TYPE.ALL_SAME:
							{
								int	SMG = (int) Data[0];
								foreach ( ConsolidatedFace F in Faces )
									F.SmoothingGroups = SMG;
								break;
							}

						case FBXImporter.LayerElement.MAPPING_TYPE.BY_EDGE:
							{
								break;
							}

						default:
							throw new Exception( "Found a layer element of type \"SMOOTHING\" with unsupported \"" + Element.MappingType + "\" mapping type!\r\n(Only BY_POLYGON & ALL_SAME mapping modes are supported!)" );
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Check if we have tangent space information
			TANGENT_SPACE_AVAILABILITY	TSAvailability = TANGENT_SPACE_AVAILABILITY.NOTHING;

			foreach ( FBXImporter.LayerElement Element in m_LayerElements )
				switch ( Element.ElementType )
				{
					case FBXImporter.LayerElement.ELEMENT_TYPE.UV:
						TSAvailability |= TANGENT_SPACE_AVAILABILITY.UVs;
						break;

					case FBXImporter.LayerElement.ELEMENT_TYPE.NORMAL:
						TSAvailability |= TANGENT_SPACE_AVAILABILITY.NORMAL;
						break;

					case FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT:
						TSAvailability |= TANGENT_SPACE_AVAILABILITY.TANGENT;
						break;

					case FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL:
						TSAvailability |= TANGENT_SPACE_AVAILABILITY.BINORMAL;
						break;
				}

			if ( TSAvailability == TANGENT_SPACE_AVAILABILITY.NOTHING )
			{	// Can't generate !
				switch ( m_Owner.NoTangentSpaceAction )
				{
					case SceneLoader.NO_TANGENT_SPACE_ACTION.THROW:
						throw new Exception( "Can't generate Tangent Space because there is no texture coordinates!" );

					case SceneLoader.NO_TANGENT_SPACE_ACTION.SKIP:
						return;
				}
			}


			//////////////////////////////////////////////////////////////////////////
			// Build dummy layer elements for position, normal, tangent & binormal streams of data
			//
			m_LayerElementPosition = new FBXImporter.LayerElement( "Position", FBXImporter.LayerElement.ELEMENT_TYPE.POSITION, FBXImporter.LayerElement.MAPPING_TYPE.BY_CONTROL_POINT, 0 );
			m_LayerElementPosition.SetArrayOfData( m_Vertices );
			m_LayerElements.Insert( 0, m_LayerElementPosition );	// Make it first layer!

			m_LayerElementNormal = null;
			m_LayerElementTangent = null;
			m_LayerElementBiNormal = null;
			foreach ( FBXImporter.LayerElement LE in m_LayerElements )
			{
				if ( m_LayerElementNormal == null && LE.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.NORMAL )
				{	// Re-use the normals element
					m_LayerElementNormal = LE;
					m_LayerElementNormal.MappingType = FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX;
					m_LayerElementNormal.ReferenceType = FBXImporter.LayerElement.REFERENCE_TYPE.DIRECT;
				}
				else if ( m_LayerElementTangent == null && LE.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT )
				{	// Re-use the tangents element
					m_LayerElementTangent = LE;
					m_LayerElementTangent.MappingType = FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX;
					m_LayerElementTangent.ReferenceType = FBXImporter.LayerElement.REFERENCE_TYPE.DIRECT;
				}
				else if ( m_LayerElementBiNormal == null && LE.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL )
				{	// Re-use the binormals element
					m_LayerElementBiNormal = LE;
					m_LayerElementBiNormal.MappingType = FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX;
					m_LayerElementBiNormal.ReferenceType = FBXImporter.LayerElement.REFERENCE_TYPE.DIRECT;
				}
			}

			if ( m_LayerElementNormal == null )
			{	// Create a new normals element that we'll need to generate
				m_LayerElementNormal = new FBXImporter.LayerElement( "Normal", FBXImporter.LayerElement.ELEMENT_TYPE.NORMAL, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX, 0 );
				m_LayerElements.Add( m_LayerElementNormal );
			}
			if ( m_Owner.GenerateTangentSpace && m_LayerElementTangent == null )
			{	// Create a new tangents element that we'll need to generate
				m_LayerElementTangent = new FBXImporter.LayerElement( "Tangent", FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX, 0 );
				m_LayerElements.Add( m_LayerElementTangent );
			}
			if ( m_Owner.GenerateTangentSpace && m_LayerElementBiNormal == null )
			{	// Create a new binormals element that we'll need to generate
				m_LayerElementBiNormal = new FBXImporter.LayerElement( "BiNormal", FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX, 0 );
				m_LayerElements.Add( m_LayerElementBiNormal );
			}


			//////////////////////////////////////////////////////////////////////////
			// Generate missing data
			BuildTangentSpace( Faces, TSAvailability, m_Owner.GenerateTangentSpace );


			//////////////////////////////////////////////////////////////////////////
			// Build primitives based on referenced materials
			m_Primitives.Clear();

			Dictionary<FBXImporter.Material,Primitive>	Material2Primitive = new Dictionary<FBXImporter.Material,Primitive>();
			Primitive	DefaultPrimitive = null;

			for ( int FaceIndex=0; FaceIndex < Faces.Count; FaceIndex++ )
			{
				ConsolidatedFace	F = Faces[FaceIndex];
				Primitive			P = null;
				if ( F.Material == null )
				{	// Default material
					if ( DefaultPrimitive == null )
					{	// Create the default primitive
						DefaultPrimitive = new Primitive( this, m_Owner, this.m_Name + "_Primitive" + m_Primitives.Count, null );
						DefaultPrimitive.OverrideMaterial = m_OverrideMaterial;	// Setup the optional override material
						m_Primitives.Add( DefaultPrimitive );
					}

					P = DefaultPrimitive;
				}
				else if ( !Material2Primitive.ContainsKey( F.Material ) )
				{	// New primitive!
					P = new Primitive( this, m_Owner, this.m_Name + "_Primitive" + m_Primitives.Count, F.Material );
					m_Primitives.Add( P );
					Material2Primitive[F.Material] = P;
				}
				else
					P = Material2Primitive[F.Material];

				P.AddFace( F );
			}
		}

		/// <summary>
		/// Generates the tangent space informations at face level (called by Commit())
		/// </summary>
		/// <param name="_Faces">The list of faces to build tangent space for</param>
		/// <param name="_TSAvailability">A combination of availability flags for tangent space reconstruction</param>
		/// <param name="_bGenerateMissingTangentSpace">Generates the missing tangent space data</param>
		protected void	BuildTangentSpace( List<ConsolidatedFace> _Faces, TANGENT_SPACE_AVAILABILITY _TSAvailability, bool _bGenerateMissingTangentSpace )
		{
			bool	bHasUVs = (_TSAvailability & TANGENT_SPACE_AVAILABILITY.UVs) != 0;

			//////////////////////////////////////////////////////////////////////////
			// Build face normals
			//
			foreach ( ConsolidatedFace F in _Faces )
			{
				Point	V0 = m_Vertices[F.VertexIndex0];
				Point	V1 = m_Vertices[F.VertexIndex1];
				Point	V2 = m_Vertices[F.VertexIndex2];

				F.Normal = ((V2 - V1) ^ (V0 - V1));
				float	fLength = F.Normal.Magnitude();
				if ( fLength > 1e-6f )
					F.Normal /= fLength;
				else
					F.Normal = new Vector( 1, 0, 0 );
			}

			//////////////////////////////////////////////////////////////////////////
			// Here we handle the case where we're missing some tangent space data and have a UV set to generate one from scratch
			//
			bool	bTangentsBinormalsAvailable = false;
			if ( _bGenerateMissingTangentSpace && bHasUVs )
			{
				// Retrieve the UV layer element that will help us to compute the tangent space
				FBXImporter.LayerElement	UVLE = null;
				foreach ( FBXImporter.LayerElement LE in m_LayerElements )
					if ( LE.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.UV && (UVLE == null || LE.Index == 0) )
						UVLE = LE;	// Found a valid UV layer element !

				// Rebuild tangent space from UVs
				foreach ( ConsolidatedFace F in _Faces )
				{
					FBXImporter.NodeMesh.Triangle	T = m_Faces[F.Index];

					Point		V0 = m_Vertices[F.VertexIndex0];
					Point		V1 = m_Vertices[F.VertexIndex1];
					Point		V2 = m_Vertices[F.VertexIndex2];
					Vector2D	UV0 = UVLE.GetElementByTriangleVertex( F.Index, 0 ) as Vector2D;
					Vector2D	UV1 = UVLE.GetElementByTriangleVertex( F.Index, 1 ) as Vector2D;
					Vector2D	UV2 = UVLE.GetElementByTriangleVertex( F.Index, 2 ) as Vector2D;

					// Compute tangent using U gradient
					Vector		dV0 = V1 - V0;
					Vector		dV1 = V2 - V0;
					Vector2D 	dUV0 = UV1 - UV0;
					Vector2D 	dUV1 = UV2 - UV0;

					float	fDet = (dUV0.x * dUV1.y - dUV0.y * dUV1.x);
					if ( Math.Abs( fDet ) > 1e-6f )
					{
						F.Tangent = new Vector(		dUV1.y * dV0.x - dUV0.y * dV1.x,
													dUV1.y * dV0.y - dUV0.y * dV1.y,
													dUV1.y * dV0.z - dUV0.y * dV1.z ).Normalize();
						F.BiNormal = -new Vector(	dUV1.x * dV0.x - dUV0.x * dV1.x,
													dUV1.x * dV0.y - dUV0.x * dV1.y,
													dUV1.x * dV0.z - dUV0.x * dV1.z ).Normalize();
					}
					else
					{	// Singularity...
						F.Tangent = new Vector( 1, 0, 0 );
						F.BiNormal = new Vector( 0, 1, 0 );
					}
				}

				bTangentsBinormalsAvailable = true;
			}

			//////////////////////////////////////////////////////////////////////////
			// Build the list of faces that share a common vertex
			//
			Dictionary<int,List<SharedFace>>	Vertex2SharedFace = new Dictionary<int,List<SharedFace>>();
			for ( int VertexIndex=0; VertexIndex < m_Vertices.Length; VertexIndex++ )
				Vertex2SharedFace[VertexIndex] = new List<SharedFace>();

			Vector[]	Normals = new Vector[3*_Faces.Count];
			Vector[]	Tangents = new Vector[3*_Faces.Count];
			Vector[]	BiNormals = new Vector[3*_Faces.Count];

			foreach ( ConsolidatedFace F in _Faces )
			{
				Point		V0 = m_Vertices[F.VertexIndex0];
				Point		V1 = m_Vertices[F.VertexIndex1];
				Point		V2 = m_Vertices[F.VertexIndex2];

				SharedFace	SF0 = new SharedFace( F, V0, V1, V2 );
				SharedFace	SF1 = new SharedFace( F, V1, V2, V0 );
				SharedFace	SF2 = new SharedFace( F, V2, V0, V1 );

				Vertex2SharedFace[F.VertexIndex0].Add( SF0 );
				Vertex2SharedFace[F.VertexIndex1].Add( SF1 );
				Vertex2SharedFace[F.VertexIndex2].Add( SF2 );

				// Initialize normal, tangent & binormal for the 3 vertices of that face
				Normals[3*F.Index+0] = new Vector( 0, 0, 0 );
				Normals[3*F.Index+1] = new Vector( 0, 0, 0 );
				Normals[3*F.Index+2] = new Vector( 0, 0, 0 );

				Tangents[3*F.Index+0] = new Vector( 0, 0, 0 );
				Tangents[3*F.Index+1] = new Vector( 0, 0, 0 );
				Tangents[3*F.Index+2] = new Vector( 0, 0, 0 );

				BiNormals[3*F.Index+0] = new Vector( 0, 0, 0 );
				BiNormals[3*F.Index+1] = new Vector( 0, 0, 0 );
				BiNormals[3*F.Index+2] = new Vector( 0, 0, 0 );
			}

			//////////////////////////////////////////////////////////////////////////
			// Accumulate normals, tangents & binormals for each vertex according to their smoothing group
			//
			foreach ( ConsolidatedFace F in _Faces )
			{
				// Accumulate for vertex 0
				foreach ( SharedFace SF in Vertex2SharedFace[F.VertexIndex0] )
					if ( SF.m_Face == F || (SF.m_Face.SmoothingGroups & F.SmoothingGroups) != 0 )
					{	// Another face shares our smoothing groups!
						Normals[3*F.Index+0] += SF.m_Weight * SF.m_Face.Normal;
						if ( bTangentsBinormalsAvailable )
						{
							Tangents[3*F.Index+0] += SF.m_Weight * SF.m_Face.Tangent;
							BiNormals[3*F.Index+0] += SF.m_Weight * SF.m_Face.BiNormal;
						}
					}

				// Accumulate for vertex 1
				foreach ( SharedFace SF in Vertex2SharedFace[F.VertexIndex1] )
					if ( SF.m_Face == F || (SF.m_Face.SmoothingGroups & F.SmoothingGroups) != 0 )
					{	// Another face shares our smoothing groups!
						Normals[3*F.Index+1] += SF.m_Weight * SF.m_Face.Normal;
						if ( bTangentsBinormalsAvailable )
						{
							Tangents[3*F.Index+1] += SF.m_Weight * SF.m_Face.Tangent;
							BiNormals[3*F.Index+1] += SF.m_Weight * SF.m_Face.BiNormal;
						}
					}

				// Accumulate for vertex 2
				foreach ( SharedFace SF in Vertex2SharedFace[F.VertexIndex2] )
					if ( SF.m_Face == F || (SF.m_Face.SmoothingGroups & F.SmoothingGroups) != 0 )
					{	// Another face shares our smoothing groups!
						Normals[3*F.Index+2] += SF.m_Weight * SF.m_Face.Normal;
						if ( bTangentsBinormalsAvailable )
						{
							Tangents[3*F.Index+2] += SF.m_Weight * SF.m_Face.Tangent;
							BiNormals[3*F.Index+2] += SF.m_Weight * SF.m_Face.BiNormal;
						}
					}
			}

			//////////////////////////////////////////////////////////////////////////
			// Finally, normalize the normals, tangents & binormals
			//
			for ( int i=0; i < 3 * _Faces.Count; i++ )
			{
				Normals[i].Normalize();
				if ( bTangentsBinormalsAvailable )
				{
					Tangents[i].Normalize();
					BiNormals[i].Normalize();
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Set the data in the layer elements
			//
			m_LayerElementNormal.SetArrayOfData( Normals );
			if ( _bGenerateMissingTangentSpace )
			{
				m_LayerElementTangent.SetArrayOfData( Tangents );
				m_LayerElementBiNormal.SetArrayOfData( BiNormals );
			}
		}

		/// <summary>
		/// Performs mesh consolidation and builds any missing tangent space information
		/// </summary>
		public void		PerformConsolidation()
		{
			if ( !IsMaster )
				return;	// The master mesh holds the consolidation data, we only need to retrieve them later...

			//////////////////////////////////////////////////////////////////////////
			// Build the list of layer elements, ours and external ones
			List<FBXImporter.LayerElement>	CollapsedLayerElements = new List<FBXImporter.LayerElement>();
			List<LoaderTempMesh>			CollapsedLayerElementMeshes = new List<LoaderTempMesh>();
			foreach ( FBXImporter.LayerElement Element in m_LayerElements )
			{
				CollapsedLayerElements.Add( Element );
				CollapsedLayerElementMeshes.Add( this );
			}
			foreach ( ExternalLayerElement Element in m_LayerElementsExternal )
			{
				CollapsedLayerElements.Add( Element.m_LayerElement );
				CollapsedLayerElementMeshes.Add( Element.m_Owner );
			}
			m_CollapsedLayerElements = CollapsedLayerElements.ToArray();
			m_CollapsedLayerElementMeshes = CollapsedLayerElementMeshes.ToArray();

			//////////////////////////////////////////////////////////////////////////
			// Consolidate each primitive
			foreach ( Primitive P in m_Primitives )
				P.Consolidate();
		}
 
		/// <summary>
		/// Builds a consolidated vertex
		/// </summary>
		/// <param name="_Face">The face referencing this vertex</param>
		/// <param name="_FaceVertexIndex">The index of the vertex in that face</param>
		/// <param name="_VertexIndex">The index of the vertex to build</param>
		/// <returns></returns>
		protected ConsolidatedVertex	BuildConsolidatedVertex( ConsolidatedFace _Face, int _FaceVertexIndex, int _VertexIndex )
		{
			ConsolidatedVertex	Result = new ConsolidatedVertex();

			// Setup its smoothing group
			Result.m_SmoothingGroups = _Face.SmoothingGroups;

			// Setup informations
			for ( int LayerElementIndex=0; LayerElementIndex < m_CollapsedLayerElements.Length; LayerElementIndex++ )
			{
				FBXImporter.LayerElement	Element = m_CollapsedLayerElements[LayerElementIndex];
				LoaderTempMesh				OwnerMesh = m_CollapsedLayerElementMeshes[LayerElementIndex];

				if ( Element.MappingType != FBXImporter.LayerElement.MAPPING_TYPE.BY_EDGE )
				{
					// Translate information
					VERTEX_INFO_TYPE	InfoType = VERTEX_INFO_TYPE.UNKNOWN;
					switch ( Element.ElementType )
					{
						case FBXImporter.LayerElement.ELEMENT_TYPE.POSITION:
							InfoType = VERTEX_INFO_TYPE.POSITION;
							break;
						case FBXImporter.LayerElement.ELEMENT_TYPE.NORMAL:
							InfoType = VERTEX_INFO_TYPE.NORMAL;
							break;
						case FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL:
							InfoType = VERTEX_INFO_TYPE.BINORMAL;
							break;
						case FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT:
							InfoType = VERTEX_INFO_TYPE.TANGENT;
							break;
						case FBXImporter.LayerElement.ELEMENT_TYPE.UV:
							InfoType = VERTEX_INFO_TYPE.TEXCOORD2D;
							break;
						case FBXImporter.LayerElement.ELEMENT_TYPE.VERTEX_COLOR:
							InfoType = VERTEX_INFO_TYPE.COLOR_HDR;
							break;
					}

					if ( InfoType == VERTEX_INFO_TYPE.UNKNOWN )
						continue;	// Not supported...

					// Fill the info
					ConsolidatedVertex.VertexInfo	Info = new ConsolidatedVertex.VertexInfo();
													Info.m_Owner = OwnerMesh;
													Info.m_SourceLayerElement = Element;
													Info.m_Type = InfoType;
													Info.m_Index = Element.Index;

					object[]	Data = Element.ToArray();
					switch ( Element.MappingType )
					{
						case	FBXImporter.LayerElement.MAPPING_TYPE.BY_CONTROL_POINT:
							Info.m_Value = Data[_VertexIndex];
							break;

						case	FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE:
							Info.m_Value = Data[_Face.Index];
							break;

						case	FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX:
							Info.m_Value = Data[3*_Face.Index + _FaceVertexIndex];
							break;

						case	FBXImporter.LayerElement.MAPPING_TYPE.ALL_SAME:
							Info.m_Value = Data[0];
							break;
					}

					Result.m_Infos.Add( Info );

					// Special treatment for position, normal, tangent & binormal...
					switch ( InfoType )
					{
						case	VERTEX_INFO_TYPE.POSITION:
							Result.m_PositionInfo = Info;
							break;
						case	VERTEX_INFO_TYPE.NORMAL:
							Result.m_NormalInfo = Info;
							break;
						case	VERTEX_INFO_TYPE.TANGENT:
							Result.m_TangentInfo = Info;
							break;
						case	VERTEX_INFO_TYPE.BINORMAL:
							Result.m_BinormalInfo = Info;
							break;
					}
				}
			}

			return	Result;
		}

		/// <summary>
		/// Attempts to merge this mesh with the provided master mesh
		/// If the provided mesh can be used as a master for this mesh then the identical layer elements are "shared by reference"
		///  and the layer elements that differ are kept in this mesh and added as external elements to the master mesh.
		/// 
		/// In the end, only the master meshes will be optimized, and this mesh's data along with them so all is left is to retrieve
		///  the optimized referenced data from the master mesh and make them our own.
		/// </summary>
		/// <returns>True if the merge was successful</returns>
		public bool					MergeWithMasterMesh( LoaderTempMesh _Master )
		{
			if ( !_Master.IsMaster )
				return	false;	// Not a master mesh...

			// 1] Compare vertex, faces & primitives counts (easy comparisons first)
			if ( m_Vertices.Length != _Master.m_Vertices.Length )
				return	false;	// Not identical !
			if ( m_Faces.Length != _Master.m_Faces.Length )
				return	false;	// Not identical !
			if ( m_Primitives.Count != _Master.m_Primitives.Count )
				return	false;	// Not identical !

			// 2] Compare each primitive's vertex & faces count
			for ( int PrimitiveIndex=0; PrimitiveIndex < m_Primitives.Count; PrimitiveIndex++ )
			{
				Primitive	P0 = m_Primitives[PrimitiveIndex];
				Primitive	P1 = _Master.m_Primitives[PrimitiveIndex];

				if ( P0.FacesCount != P1.FacesCount )
					return	false;	// Not identical !
			}

			// 3] Compare the vertices one by one
			for ( int VertexIndex=0; VertexIndex < m_Vertices.Length; VertexIndex++ )
			{
				Point	V0 = m_Vertices[VertexIndex];
				Point	V1 = _Master.m_Vertices[VertexIndex];
				if ( V0 != V1 )
					return	false;
			}

			// 4] Compare the faces one by one
			for ( int FaceIndex=0; FaceIndex < m_Faces.Length; FaceIndex++ )
			{
				FBXImporter.NodeMesh.Triangle	F0 = m_Faces[FaceIndex];
				FBXImporter.NodeMesh.Triangle	F1 = _Master.m_Faces[FaceIndex];
				if ( F0.Vertex0 != F1.Vertex0 || F0.Vertex1 != F1.Vertex1 || F0.Vertex2 != F1.Vertex2 )
					return	false;
			}

			//////////////////////////////////////////////////////////////////////////
			// At this point, the 2 meshes are deemed identical (up to the point of Vertices and Faces at least)
			//////////////////////////////////////////////////////////////////////////

			// Make this mesh a slave
			m_MasterMesh = _Master;

			// 5] Compare each of our Layer Elements to the master's and merge them
			//	_ Layer Elements that are identical to the master will be replaced by references to the master's
			//	_ Layer Elements that are different will be kept and will be added as external elements to the master
			//
			FBXImporter.LayerElement[]	LayerElements = m_LayerElements.ToArray();
			foreach ( FBXImporter.LayerElement LE0 in LayerElements )
			{
				FBXImporter.LayerElement	LE1 = null;
				foreach ( FBXImporter.LayerElement MasterLE in _Master.m_LayerElements )
					if ( LE0.Compare( MasterLE ) )
					{	// Found a match !
						LE1 = MasterLE;
						break;
					}

				if ( LE1 != null )
				{	// We found a matching layer element in the master mesh!
					// Now, we simply replace our own element by a reference to the master's element
					ReplaceLayerElementByAReference( LE0, _Master, LE1 );
				}
				else
				{	// We couldn't find a matching layer element!
					// That means this layer element is unique to our instance so we add it as an external element in the master mesh
					// When the master mesh will be consolidated, it will take our elements into account as well...
					_Master.AddExternalLayerElement( this, LE0 );
				}
			}

			return	true;
		}

		#endregion
	};
}
