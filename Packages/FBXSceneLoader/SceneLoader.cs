using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Nuaj;
using WMath;

namespace Nuaj.Cirrus.FBX
{
	/// <summary>
	/// This class is able to convert a FBX scene into a Cirrus scene
	/// 
	/// Load a scene likes this :
	/// 1) Create an empty scene
	/// 2) Create the scene loader
	/// 3) Ask the loader to load a FBX scene providing :
	///  . The filename of the scene to load
	///  . A material map that is able to convert FBX materials into Cirrus render techniques that render these materials
	///  . A texture provider that loads textures for the scene materials
	/// 
	/// Once the FBX scene is loaded, you can save the scene into a proprietary scene format and shouldn't need the FBX format anymore
	/// (i.e. FBX is only a DEBUG mode format that should NOT be used at runtime as proprietary scenes are much more lightweight).
	/// </summary>
	/// <remarks>
	///	_ ZUp fix from Max orientation is done in the RecurseProcessNode() method at root node level
	///	_ V coordinate complement to 1 is done in PrimitiveFeeder.GetField() method for the TEX_COORD2D case
	/// </remarks>
	public class	SceneLoader
	{
		#region NESTED TYPES

		// The list of suported texture formats
		//
		public enum		TEXTURE_CONVERSION_TYPES
		{
			NONE,	// No conversion, leave texture as is
			JPG,
			PNG,
			DDS,	// NOT SUPPORTED YET
			DXT1,	// NOT SUPPORTED YET
			DXT2,	// NOT SUPPORTED YET
			DXT3,	// NOT SUPPORTED YET
			DXT5,	// NOT SUPPORTED YET
		}

		// The list of actions to perform if no tangent space data is available
		//
		public enum		NO_TANGENT_SPACE_ACTION
		{
			THROW,
			SKIP,
			VALIDATE
		}

		[System.Diagnostics.DebuggerDisplay( "Name={Name}" )]
		public class	SceneObject
		{
			#region FIELDS

			protected SceneLoader				m_Owner = null;
			protected string					m_Name = null;

			protected SceneObject				m_Parent = null;

			protected Dictionary<string,string>	m_Properties = new Dictionary<string,string>();
			protected Dictionary<string,string>	m_Params = new Dictionary<string,string>();
			protected Dictionary<string,string>	m_Custom = new Dictionary<string,string>();

			#endregion

			#region PROPERTIES

			/// <summary>
			/// Gets the object's name
			/// </summary>
			public string		Name
			{
				get { return m_Name; }
			}

			/// <summary>
			/// Gets the owner serializer
			/// </summary>
			public SceneLoader	Owner
			{
				get { return m_Owner; }
			}

			/// <summary>
			/// Gets the object's parent
			/// </summary>
			public SceneObject	Parent
			{
				get { return m_Parent; }
			}

			/// <summary>
			/// Gets the dictionary of properties
			/// This will be serialized as "properties" in the JSON file
			/// </summary>
			public Dictionary<string,string>	Properties
			{
				get { return m_Properties; }
			}

			/// <summary>
			/// Gets the dictionary of params
			/// This will be serialized as "params" in the JSON file
			/// </summary>
			public Dictionary<string,string>	Params
			{
				get { return m_Params; }
			}

			/// <summary>
			/// Gets the dictionary of custom data
			/// This will be serialized as "custom" in the JSON file
			/// </summary>
			public Dictionary<string,string>	Custom
			{
				get { return m_Custom; }
			}

			#endregion

			#region METHODS

			public SceneObject( SceneLoader _Owner, string _Name )
			{
				if ( _Name == null )
					throw new Exception( "Invalid name for object ! You cannot provide null as a name for an object!" );

				m_Owner = _Owner;
				m_Name = _Name;
			}

			/// <summary>
			/// Sets the object's parent
			/// </summary>
			/// <param name="_Parent">The object's parent</param>
			public void		SetParent( SceneObject _Parent )
			{
				m_Parent = _Parent;
			}

			/// <summary>
			/// Sets a property
			/// </summary>
			/// <param name="_Name">The name of the property to set</param>
			/// <param name="_Value">The value of the property (null clears the property)</param>
			public void		SetProperty( string _Name, string _Value )
			{
				if ( _Value == null && m_Properties.ContainsKey( _Name ) )
					m_Properties.Remove( _Name );
				else
					m_Properties[_Name] = _Value;
			}

			/// <summary>
			/// Sets a param
			/// </summary>
			/// <param name="_Name">The name of the param to set</param>
			/// <param name="_Value">The value of the param (null clears the param)</param>
			public void		SetParam( string _Name, string _Value )
			{
				if ( _Value == null && m_Params.ContainsKey( _Name ) )
					m_Params.Remove( _Name );
				else
					m_Params[_Name] = _Value;
			}

			/// <summary>
			/// Sets a custom property
			/// </summary>
			/// <param name="_Name">The name of the property to set</param>
			/// <param name="_Value">The value of the proeprty (null clears the property)</param>
			public void		SetCustomProperty( string _Name, string _Value )
			{
				if ( _Value == null && m_Custom.ContainsKey( _Name ) )
					m_Custom.Remove( _Name );
				else
					m_Custom[_Name] = _Value;
			}

			#endregion
		};

		public class	Transform : SceneObject
		{
			#region FIELDS

			// Pivot setup. The actual transform's matrix is the composition of the pivot with either the static matrix or the dynamic animation source matrix
			protected Matrix4x4			m_Pivot = null;

			// Static transform setup
			protected Matrix4x4			m_Matrix = null;
			protected Point				m_Position = null;
			protected Matrix3x3			m_Rotation = null;
			protected Quat				m_QuatRotation = null;
			protected Vector			m_Scale = null;

			// Dynamic transform setup
			protected bool							m_bAnimated = false;
			protected Matrix4x4						m_AnimationSourceMatrix = null;
			protected FBXImporter.AnimationTrack[]	m_AnimP = null;
			protected FBXImporter.AnimationTrack[]	m_AnimR = null;
			protected FBXImporter.AnimationTrack[]	m_AnimS = null;

			protected List<Mesh>		m_Meshes = new List<Mesh>();

			#endregion

			#region PROPERTIES

			public Matrix4x4	Pivot
			{
				get { return m_Pivot != null ? m_Pivot : new Matrix4x4().MakeIdentity(); }
				set { m_Pivot = value; }
			}

			/// <summary>
			/// The staic local transform
			/// </summary>
			public Matrix4x4	Matrix
			{
				get
				{
					if ( m_Matrix != null )
						return	Pivot * m_Matrix;	// Simple...

					// Otherwise, recompose matrix
					Matrix4x4	Result = new Matrix4x4();
								Result.MakeIdentity();

					// Setup the rotation part
					if ( m_Rotation != null )
						Result.SetRotation( m_Rotation );
					else if ( (m_QuatRotation as object) != null )
						Result = (Matrix4x4) m_QuatRotation;

					// Setup the scale part
					if ( m_Scale != null )
						Result.Scale( m_Scale );

					// Setup the translation part
					if ( m_Position != null )
						Result.SetTrans( m_Position );

					return	Pivot * Result;
				}
			}

			/// <summary>
			/// Tells if the transform is animated
			/// </summary>
			public bool			IsAnimated
			{
				get { return m_bAnimated; }
			}

			public FBXImporter.AnimationTrack[]	AnimationTrackPositions		{ get { return m_AnimP; } }
			public FBXImporter.AnimationTrack[]	AnimationTrackRotations		{ get { return m_AnimR; } }
			public FBXImporter.AnimationTrack[]	AnimationTrackScales		{ get { return m_AnimS; } }
			public Matrix4x4					AnimationSourceMatrix		{ get { return Pivot * m_AnimationSourceMatrix; } }

			/// <summary>
			/// Gets the list of meshes attached to this transform
			/// </summary>
			public Mesh[]		Meshes
			{
				get { return m_Meshes.ToArray(); }
			}

			#endregion

			#region METHODS

			public Transform( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			#region Static Transform Setup

			public void		SetMatrix( Matrix4x4 _Matrix )
			{
				m_Matrix = _Matrix;
			}

			public void		SetPosition( float _x, float _y, float _z )
			{
				m_Position = new Point( _x, _y, _z );
			}

			public void		SetRotationFromMatrix( float[] _Row0, float[] _Row1, float[] _Row2 )
			{
				if ( _Row0 == null )
					throw new Exception( "Invalid row #0!" );
				if ( _Row1 == null )
					throw new Exception( "Invalid row #1!" );
				if ( _Row2 == null )
					throw new Exception( "Invalid row #2!" );
				if ( _Row0.Length != 3 || _Row1.Length != 3 || _Row2.Length != 3 )
					throw new Exception( "Rows must be of length 3!" );

				float[,]	Mat = new float[3,3];
							Mat[0,0] = _Row0[0];	Mat[0,1] = _Row0[1];	Mat[0,2] = _Row0[2];
							Mat[1,0] = _Row1[0];	Mat[1,1] = _Row1[1];	Mat[1,2] = _Row1[2];
							Mat[2,0] = _Row2[0];	Mat[2,1] = _Row2[1];	Mat[2,2] = _Row2[2];

				m_Rotation = new Matrix3x3( Mat );
			}

			public void		SetRotationFromQuat( float _x, float _y, float _z, float _s )
			{
				m_QuatRotation = new Quat( _s, _x, _y, _z );
			}

			public void		SetScale( float _x, float _y, float _z )
			{
				m_Scale = new Vector( _x, _y, _z );
			}

			#endregion

			#region Dynamic Transform Setup

			public void		SetAnimationTrackPositions( FBXImporter.AnimationTrack[] _Tracks )
			{
				m_AnimP = _Tracks;
				m_bAnimated = true;
			}

			public void		SetAnimationTrackRotations( FBXImporter.AnimationTrack[] _Tracks )
			{
				m_AnimR = _Tracks;
				m_bAnimated = true;
			}

			public void		SetAnimationTrackScales( FBXImporter.AnimationTrack[] _Tracks )
			{
				m_AnimS = _Tracks;
				m_bAnimated = true;
			}

			public void		SetAnimationSourceMatrix( Matrix4x4 _SourceMatrix )
			{
				m_AnimationSourceMatrix = _SourceMatrix;
			}

			#endregion

			/// <summary>
			/// Adds a mesh to this transform
			/// </summary>
			/// <param name="_Mesh">The mesh to add</param>
			/// <remarks>This will transform into referenced shapes in the JSON file</remarks>
			public void		AddMesh( Mesh _Mesh )
			{
				m_Meshes.Add( _Mesh );
			}

			#endregion
		};

		public class	Mesh : SceneObject
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
			private class		ConsolidatedFace
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
			private class		ConsolidatedVertex
			{
				#region NESTED TYPES

				/// <summary>
				/// Stores an information about the vertex
				/// </summary>
				[System.Diagnostics.DebuggerDisplay( "Type={m_Type} Value={m_Value}" )]
				public class	VertexInfo
				{
					public Mesh				m_OwnerMesh = null;						// The mesh that owns the info
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

			public class		Primitive : SceneObject
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

				protected Mesh						m_OwnerMesh = null;

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

				public Primitive( Mesh _OwnerMesh, SceneLoader _Owner, string _Name, FBXImporter.Material _Material ) : base( _Owner, _Name )
				{
					m_OwnerMesh = _OwnerMesh;
					m_Material = _Material;
// 					if ( m_Material == null )
// 						throw new Exception( "Invalid material for primitive \"" + _Name + "\"!" );
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

					if ( !m_OwnerMesh.m_Owner.m_bConsolidateMeshes )
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
				public Mesh								m_OwnerMesh;
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
				public Mesh								m_OwnerMesh;
				public FBXImporter.LayerElement			m_LayerElement = null;
			};

			#endregion

			#region FIELDS

			protected Mesh								m_MasterMesh = null;	// Non-null if this mesh is an instance mesh

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
			protected List<Mesh>						m_SlaveMeshes = new List<Mesh>();
			protected Dictionary<Mesh,bool>				m_SlaveMesh2Registered = new Dictionary<Mesh,bool>();	// The table that indicates if a mesh is already slave of this mesh

			// These are the 2 lists of collapsed layer elements (ours + external ones from other slave mesh instances)
			// These lists are built in the PerformConsolidation() method
			protected FBXImporter.LayerElement[]		m_CollapsedLayerElements = null;
			protected Mesh[]							m_CollapsedLayerElementMeshes = null;

			protected Material							m_OverrideMaterial = null;

			protected BoundingBox						m_BBox = null;
			protected Matrix4x4							m_Pivot = null;
			protected int								m_UVSetsCount = 0;

			// Generated data
			protected List<Primitive>					m_Primitives = new List<Primitive>();

			#endregion

			#region PROPERTIES

			public bool			IsMaster
			{
				get { return m_MasterMesh == null; }
			}

			public Mesh			MasterMesh
			{
				 get { return m_MasterMesh; }
			}

			public Point[]		Vertices
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

			public Primitive[]	ConsolidatedPrimitives
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

			public Mesh( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
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
				if ( m_Owner.m_bCompactUVs && _LayerElement.ElementType == FBXImporter.LayerElement.ELEMENT_TYPE.UV )
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
			public void		AddExternalLayerElement( Mesh _OwnerMesh, FBXImporter.LayerElement _LayerElement )
			{
				ExternalLayerElement	ELE = new ExternalLayerElement();
										ELE.m_OwnerMesh = _OwnerMesh;
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
			public void		ReplaceLayerElementByAReference( FBXImporter.LayerElement _LayerElementSource, Mesh _OwnerMesh, FBXImporter.LayerElement _LayerElementReference )
			{
				m_LayerElements.Remove( _LayerElementSource );

				ReferenceLayerElement	RLE = new ReferenceLayerElement();
										RLE.m_OwnerMesh = _OwnerMesh;
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
				if ( m_Owner.m_bGenerateTriangleStrips )
					throw new Exception( "Triangle Strips are not supported yet!" );

				// Setup the comparison flags used for consolidation
				ConsolidatedVertex.ms_CompareSmoothingGroups = m_Owner.m_bConsolidateSplitBySMG;
				ConsolidatedVertex.VertexInfo.ms_CompareUVs = m_Owner.m_bConsolidateSplitByUV;
				ConsolidatedVertex.VertexInfo.ms_CompareColors = m_Owner.m_bConsolidateSplitByColor;

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
					switch ( m_Owner.m_NoTangentSpaceAction )
					{
						case NO_TANGENT_SPACE_ACTION.THROW:
							throw new Exception( "Can't generate Tangent Space because there is no texture coordinates!" );

						case NO_TANGENT_SPACE_ACTION.SKIP:
							return;
					}
				}


				//////////////////////////////////////////////////////////////////////////
				// Build dummy layer elements for position, normal, tangent & binormal streams of data
				//
				m_LayerElementPosition = new FBXImporter.LayerElement( "Position", FBXImporter.LayerElement.ELEMENT_TYPE.POSITION, FBXImporter.LayerElement.MAPPING_TYPE.BY_CONTROL_POINT, 0 );
				m_LayerElementPosition.SetArrayOfData( m_Vertices );
				m_LayerElements.Add( m_LayerElementPosition );

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
				if ( m_Owner.m_bGenerateTangentSpace && m_LayerElementTangent == null )
				{	// Create a new tangents element that we'll need to generate
					m_LayerElementTangent = new FBXImporter.LayerElement( "Tangent", FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX, 0 );
					m_LayerElements.Add( m_LayerElementTangent );
				}
				if ( m_Owner.m_bGenerateTangentSpace && m_LayerElementBiNormal == null )
				{	// Create a new binormals element that we'll need to generate
					m_LayerElementBiNormal = new FBXImporter.LayerElement( "BiNormal", FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL, FBXImporter.LayerElement.MAPPING_TYPE.BY_TRIANGLE_VERTEX, 0 );
					m_LayerElements.Add( m_LayerElementBiNormal );
				}


				//////////////////////////////////////////////////////////////////////////
				// Generate missing data
				BuildTangentSpace( Faces, TSAvailability, m_Owner.m_bGenerateTangentSpace );


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
					return;

				//////////////////////////////////////////////////////////////////////////
				// Build the list of layer elements, ours and external ones
				List<FBXImporter.LayerElement>	CollapsedLayerElements = new List<FBXImporter.LayerElement>();
				List<Mesh>						CollapsedLayerElementMeshes = new List<Mesh>();
				foreach ( FBXImporter.LayerElement Element in m_LayerElements )
				{
					CollapsedLayerElements.Add( Element );
					CollapsedLayerElementMeshes.Add( this );
				}
				foreach ( ExternalLayerElement Element in m_LayerElementsExternal )
				{
					CollapsedLayerElements.Add( Element.m_LayerElement );
					CollapsedLayerElementMeshes.Add( Element.m_OwnerMesh );
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
					Mesh						OwnerMesh = m_CollapsedLayerElementMeshes[LayerElementIndex];

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
														Info.m_OwnerMesh = OwnerMesh;
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
			public bool					MergeWithMasterMesh( Mesh _Master )
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

		public class	Material : SceneObject
		{
			#region FIELDS

			protected Dictionary<string,Texture>	m_TexturesDiffuse = new Dictionary<string,Texture>();
			protected Dictionary<string,Texture>	m_TexturesNormal = new Dictionary<string,Texture>();
			protected Dictionary<string,Texture>	m_TexturesRegular = new Dictionary<string,Texture>();

			#endregion

			#region PROPERTIES

			public Texture[]	DiffuseTextures
			{
				get
				{
					Texture[]	Result = new Texture[m_TexturesDiffuse.Count];
					m_TexturesDiffuse.Values.CopyTo( Result, 0 );

					return	Result;
				}
			}

			public Texture[]	NormalTextures
			{
				get
				{
					Texture[]	Result = new Texture[m_TexturesNormal.Count];
					m_TexturesNormal.Values.CopyTo( Result, 0 );

					return	Result;
				}
			}

			public Texture[]	RegularTextures
			{
				get
				{
					Texture[]	Result = new Texture[m_TexturesRegular.Count];
					m_TexturesRegular.Values.CopyTo( Result, 0 );

					return	Result;
				}
			}

			#endregion

			#region METHODS

			public Material( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			/// <summary>
			/// Adds a diffuse texture to the material
			/// </summary>
			/// <param name="_TextureName">The name of the texture to add</param>
			/// <param name="_Texture">The texture to add</param>
			public void		AddTextureDiffuse( Texture _Texture )
			{
				if ( m_TexturesDiffuse.ContainsKey( _Texture.Name ) )
					throw new Exception( "Material \"" + m_Name + "\" already contains a DIFFUSE texture named \"" + _Texture.Name + "\"!" );

				_Texture.GenerateMipMaps = m_Owner.GenerateMipMapsDiffuse;
				m_TexturesDiffuse[_Texture.Name] = _Texture;
			}

			/// <summary>
			/// Adds a normal texture to the material
			/// </summary>
			/// <param name="_TextureName">The name of the texture to add</param>
			/// <param name="_Texture">The texture to add</param>
			public void		AddTextureNormal( Texture _Texture )
			{
				if ( m_TexturesNormal.ContainsKey( _Texture.Name ) )
					throw new Exception( "Material \"" + m_Name + "\" already contains a NORMAL texture named \"" + _Texture.Name + "\"!" );

				_Texture.GenerateMipMaps = m_Owner.GenerateMipMapsNormal;
				m_TexturesNormal[_Texture.Name] = _Texture;
			}

			/// <summary>
			/// Adds a generic texture to the material
			/// </summary>
			/// <param name="_TextureName">The name of the texture to add</param>
			/// <param name="_Texture">The texture to add</param>
			public void		AddTexture( Texture _Texture )
			{
				if ( m_TexturesRegular.ContainsKey( _Texture.Name ) )
					throw new Exception( "Material \"" + m_Name + "\" already contains a GENERIC texture named \"" + _Texture.Name + "\"!" );

				_Texture.GenerateMipMaps = m_Owner.GenerateMipMapsRegular;
				m_TexturesRegular[_Texture.Name] = _Texture;
			}

			#endregion
		};

		public class	Texture : SceneObject
		{
			#region NESTED TYPES

			protected enum	WRAP_MODE
			{
				WRAP,
				MIRROR,
				CLAMP,
				BORDER,
			}

			protected enum	FILTER_TYPE
			{
				NONE,
				POINT,
				LINEAR,
				ANISOTROPIC,
			}

			#endregion

			#region FIELDS

			protected string						m_SamplerName = null;
			protected FBXImporter.Texture			m_SourceTexture = null;
			protected bool							m_bEmbed = false;
			protected bool							m_bGenerateMipMaps = false;

			protected Dictionary<string,string>		m_SamplerParams = new Dictionary<string,string>();

			#endregion

			#region PROPERTIES

			/// <summary>
			/// Gets or sets the "Embed" flag telling if the texture should be embedded in the scene archive
			/// </summary>
			public bool		Embed
			{
				get { return m_bEmbed; }
				set { m_bEmbed = value; }
			}

			/// <summary>
			/// Gets or sets the "GenerateMipMaps" flag telling if the texture should generate multiple mip levels
			/// </summary>
			public bool		GenerateMipMaps
			{
				get { return m_bGenerateMipMaps; }
				set { m_bGenerateMipMaps = value; }
			}

			/// <summary>
			/// Gets or sets the name of the sampler that will reference that texture
			/// </summary>
			/// <remarks>
			/// If unspecified, the name of the sampler will be the name of the texture with "Sampler" appended
			/// </remarks>
			public string	SamplerName
			{
				get { return m_SamplerName != null ? m_SamplerName : (m_SourceTexture.Name + "Sampler"); }
				set { m_SamplerName = value; }
			}

			/// <summary>
			/// Gets the absolute name of the texture file
			/// </summary>
			public string	TextureFileName	{ get { return m_SourceTexture.AbsoluteFileName; } }

			/// <summary>
			/// Gets the dictionary of sampler params
			/// This will be serialized as "params" for a TextureSampler object in the JSON file
			/// </summary>
			public Dictionary<string,string>	SamplerParams
			{
				get { return m_SamplerParams; }
			}

			#endregion

			#region METHODS

			public Texture( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			/// <summary>
			/// Sets a sampler param
			/// </summary>
			/// <param name="_Name">The name of the param to set</param>
			/// <param name="_Value">The value of the param (null clears the param)</param>
			public void		SetSamplerParam( string _Name, string _Value )
			{
				if ( _Value == null && m_SamplerParams.ContainsKey( _Name ) )
					m_SamplerParams.Remove( _Name );
				else
					m_SamplerParams[_Name] = _Value;
			}

			/// <summary>
			/// Sets the source FBX texture to get the parameters from
			/// </summary>
			/// <remarks>You can override individual parameters using the "SetSamplerParam()" function,
			///  if a parameter exists in the parameters table, it will be used instead
			///  of the one that would be generated from the texture
			/// </remarks>
			/// <param name="_Texture"></param>
			public void		SetSourceFBXTexture( FBXImporter.Texture _Texture )
			{
				m_SourceTexture = _Texture;

// 				// Build default sampler params
// 				SetSamplerParam( "o3d.addressModeU", Helpers.FormatParamObject( ConvertWrapMode( _Texture.WrapModeU ) ) );
// 				SetSamplerParam( "o3d.addressModeV", Helpers.FormatParamObject( ConvertWrapMode( _Texture.WrapModeV ) ) );
// 				SetSamplerParam( "o3d.borderColor", Helpers.FormatParamObject( new Vector4D( 0, 0, 0, 0 ) ) );
// 				SetSamplerParam( "o3d.magFilter", Helpers.FormatParamObject( (int) FILTER_TYPE.LINEAR ) );
// 				SetSamplerParam( "o3d.minFilter", Helpers.FormatParamObject( (int) FILTER_TYPE.LINEAR ) );
// 				SetSamplerParam( "o3d.mipFilter", Helpers.FormatParamObject( (int) FILTER_TYPE.LINEAR ) );
// 				SetSamplerParam( "o3d.maxAnisotropy", Helpers.FormatParamObject( 1 ) );
			}


			/// <summary>
			/// Converts an FBX wrap mode into an O3D wrap mode
			/// </summary>
			/// <param name="_WrapMode"></param>
			/// <returns></returns>
			protected int	ConvertWrapMode( FBXImporter.Texture.WRAP_MODE _WrapMode )
			{
				switch ( _WrapMode )
				{
					case FBXImporter.Texture.WRAP_MODE.CLAMP:
						return	(int) WRAP_MODE.CLAMP;

					case FBXImporter.Texture.WRAP_MODE.REPEAT:
						return	(int) WRAP_MODE.WRAP;
				}

				return	(int) WRAP_MODE.WRAP;
			}

			#endregion
		};

		public class	TextureCube : Texture
		{
			#region METHODS

			public TextureCube( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			#endregion
		}

		/// <summary>
		/// This class is built from one of our Mesh.Primitives and is able to create a Cirrus primitive from it
		/// </summary>
		protected class	PrimitiveFeeder : IVertexFieldProvider, IIndexProvider
		{
			#region FIELDS

			protected Mesh.Primitive				m_SourcePrimitive = null;
			protected Cirrus.DynamicVertexSignature	m_VertexSignature = new Cirrus.DynamicVertexSignature();
			protected Dictionary<int,Mesh.Primitive.VertexStream>	m_FieldIndex2Stream = new Dictionary<int,Mesh.Primitive.VertexStream>();

			#endregion

			#region PROPERTIES

			public Cirrus.IVertexSignature		VertexSignature	{ get { return m_VertexSignature; } }

			#endregion

			#region METHODS

			public PrimitiveFeeder( Mesh.Primitive _SourcePrimitive )
			{
				m_SourcePrimitive = _SourcePrimitive;

				// Build the custom vertex signature
				int StreamIndex = 0;
				int UVStreamIndex = 0;
				foreach ( Mesh.Primitive.VertexStream Stream in m_SourcePrimitive.VertexStreams )
				{
					bool	bSupported = true;
					switch ( Stream.StreamType )
					{
						case Mesh.VERTEX_INFO_TYPE.POSITION:
							m_VertexSignature.AddField( "Position", Cirrus.VERTEX_FIELD_USAGE.POSITION, Cirrus.VERTEX_FIELD_TYPE.FLOAT3, 0 );
							break;
						case Mesh.VERTEX_INFO_TYPE.NORMAL:
							m_VertexSignature.AddField( "Normal", Cirrus.VERTEX_FIELD_USAGE.NORMAL, Cirrus.VERTEX_FIELD_TYPE.FLOAT3, 0 );
							break;
						case Mesh.VERTEX_INFO_TYPE.TANGENT:
							m_VertexSignature.AddField( "Tangent", Cirrus.VERTEX_FIELD_USAGE.TANGENT, Cirrus.VERTEX_FIELD_TYPE.FLOAT3, 0 );
							break;
						case Mesh.VERTEX_INFO_TYPE.BINORMAL:
							m_VertexSignature.AddField( "BiTangent", Cirrus.VERTEX_FIELD_USAGE.BITANGENT, Cirrus.VERTEX_FIELD_TYPE.FLOAT3, 0 );
							break;
						case Mesh.VERTEX_INFO_TYPE.TEXCOORD1D:
							m_VertexSignature.AddField( "UV", Cirrus.VERTEX_FIELD_USAGE.TEX_COORD2D, Cirrus.VERTEX_FIELD_TYPE.FLOAT2, UVStreamIndex++ );
							break;
						case Mesh.VERTEX_INFO_TYPE.TEXCOORD2D:
							m_VertexSignature.AddField( "UV", Cirrus.VERTEX_FIELD_USAGE.TEX_COORD2D, Cirrus.VERTEX_FIELD_TYPE.FLOAT2, UVStreamIndex++ );
							break;
						case Mesh.VERTEX_INFO_TYPE.TEXCOORD3D:
							m_VertexSignature.AddField( "UV", Cirrus.VERTEX_FIELD_USAGE.TEX_COORD2D, Cirrus.VERTEX_FIELD_TYPE.FLOAT2, UVStreamIndex++ );
							break;
						case Mesh.VERTEX_INFO_TYPE.COLOR_HDR:
							m_VertexSignature.AddField( "Color", Cirrus.VERTEX_FIELD_USAGE.TEX_COORD4D, Cirrus.VERTEX_FIELD_TYPE.FLOAT4, UVStreamIndex++ );
							break;

						default:
							bSupported = false;
							break;
					}

					if ( bSupported )
						m_FieldIndex2Stream[StreamIndex] = Stream;
					StreamIndex++;
				}
			}

			public Scene.Mesh.Primitive	CreatePrimitive( ITechniqueSupportsObjects _RenderTechnique, Scene.Mesh _ParentMesh, string _Name, Scene.MaterialParameters _MatParams )
			{
				return _RenderTechnique.CreatePrimitive( _ParentMesh, _Name, VertexSignature, m_SourcePrimitive.VerticesCount, this, 3*m_SourcePrimitive.FacesCount, this, _MatParams );
			}

			#region IVertexFieldProvider Members

			public object	GetField( int _VertexIndex, int _FieldIndex )
			{
				if ( !m_FieldIndex2Stream.ContainsKey( _FieldIndex ) )
					throw new Exception( "Requesting unsupported field!" );

				Mesh.Primitive.VertexStream	Stream = m_FieldIndex2Stream[_FieldIndex];

				switch ( Stream.StreamType )
				{
					case Mesh.VERTEX_INFO_TYPE.POSITION:
						return ConvertPoint( Stream.Stream[_VertexIndex] as Point );

					case Mesh.VERTEX_INFO_TYPE.NORMAL:
					case Mesh.VERTEX_INFO_TYPE.TANGENT:
					case Mesh.VERTEX_INFO_TYPE.BINORMAL:
						return ConvertVector( Stream.Stream[_VertexIndex] as Vector );

					case Mesh.VERTEX_INFO_TYPE.TEXCOORD1D:
						return (float) Stream.Stream[_VertexIndex];
					case Mesh.VERTEX_INFO_TYPE.TEXCOORD2D:
						{	// Here we must complement the V coordinate as MAX has the bad habit of inverting the Y axis of images !
							Vector2D	Source = Stream.Stream[_VertexIndex] as Vector2D;
							return ConvertVector( new Vector2D( Source.x, 1.0f - Source.y ) );
						}
					case Mesh.VERTEX_INFO_TYPE.TEXCOORD3D:
						return ConvertVector( Stream.Stream[_VertexIndex] as Vector );

					case Mesh.VERTEX_INFO_TYPE.COLOR_HDR:
						return ConvertVector( Stream.Stream[_VertexIndex] as Vector4D );
				}

				return null;
			}

			#endregion

			#region IIndexProvider Members

			public int	GetIndex( int _TriangleIndex, int _TriangleVertexIndex )
			{
				if ( _TriangleVertexIndex == 0 )
					return m_SourcePrimitive.Faces[_TriangleIndex].V0.m_Index;
				else if ( _TriangleVertexIndex == 1 )
					return m_SourcePrimitive.Faces[_TriangleIndex].V1.m_Index;
				else
					return m_SourcePrimitive.Faces[_TriangleIndex].V2.m_Index;
			}

			#endregion

			#endregion
		}

		#endregion

		#region FIELDS

		protected Cirrus.Scene					m_Scene = null;
		protected Cirrus.MaterialMap			m_MaterialMap = null;
		protected Cirrus.Scene.ITextureProvider	m_TextureProvider = null;
		protected float							m_ScaleFactor = 1.0f;

		// The list of material mapping from FBX materials to Cirrus render technique able to handle the materials
		protected Dictionary<FBXImporter.Material,Cirrus.ITechniqueSupportsObjects>	m_Material2Technique = new Dictionary<FBXImporter.Material,Cirrus.ITechniqueSupportsObjects>();

		// The list of material mapping from FBX materials to Cirrus material parameters
		protected Dictionary<FBXImporter.Material,Cirrus.Scene.MaterialParameters>	m_Material2Parameters = new Dictionary<FBXImporter.Material,Cirrus.Scene.MaterialParameters>();

		// The table of meshes
		protected Dictionary<Mesh,Cirrus.Scene.Mesh>	m_Mesh2CirrusMesh = new Dictionary<Mesh,Cirrus.Scene.Mesh>();

		// The list of texture files
		protected List<FileInfo>			m_Textures = new List<FileInfo>();


		// =================== Internal options ===================

		// Mesh options
		protected bool						m_bGenerateTangentSpace = true;
		protected NO_TANGENT_SPACE_ACTION	m_NoTangentSpaceAction = NO_TANGENT_SPACE_ACTION.THROW;

		protected bool						m_bStoreHDRVertexColors = true;
		protected bool						m_bGenerateBoundingBoxes = true;
		protected bool						m_bResetXForm = false;
		protected bool						m_bGenerateTriangleStrips = false;
		protected bool						m_bCompactUVs = true;
		protected bool						m_bExportAnimations = false;

			// Consolidation options
		protected bool						m_bConsolidateMeshes = true;
		protected bool						m_bConsolidateSplitBySMG = true;
		protected bool						m_bConsolidateSplitByUV = true;
		protected bool						m_bConsolidateSplitByColor = true;

			// UV Compacting
		protected bool						m_bCompactIdenticalMeshes = false;
		protected int						m_MinUVsCount = 1;

		// Textures
		protected DirectoryInfo				m_TargetTexturesBaseDirectory = null;

		protected TEXTURE_CONVERSION_TYPES	m_ConvertDiffuse = TEXTURE_CONVERSION_TYPES.NONE;
		protected bool						m_bGenerateMipMapsDiffuse = true;

		protected TEXTURE_CONVERSION_TYPES	m_ConvertNormal = TEXTURE_CONVERSION_TYPES.NONE;
		protected bool						m_bGenerateMipMapsNormal = true;

		protected TEXTURE_CONVERSION_TYPES	m_ConvertRegular = TEXTURE_CONVERSION_TYPES.NONE;
		protected bool						m_bGenerateMipMapsRegular = true;

		protected int						m_JPEGQuality = 80;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// True to generate missing tangent space informations from meshes
		/// (they must have at least either UV coordinates or a normal and 
		/// tangent/binormal set of data to be eligible to tangent space
		/// construction, otherwise the NoTangentSpaceAction is applied)
		/// </summary>
		public bool				GenerateTangentSpace
		{
			get { return m_bGenerateTangentSpace; }
			set { m_bGenerateTangentSpace = value; }
		}

		/// <summary>
		/// Defines the action to perform when no tangent space can be built on a mesh
		/// </summary>
		public NO_TANGENT_SPACE_ACTION	NoTangentSpaceAction
		{
			get { return m_NoTangentSpaceAction; }
			set { m_NoTangentSpaceAction = value; }
		}

		/// <summary>
		/// True to store HDR (i.e. Vector4) colors instead of LDR (i.e. UInt32) colors
		/// </summary>
		public bool				StoreHDRVertexColors
		{
			get { return m_bStoreHDRVertexColors; }
			set { m_bStoreHDRVertexColors = value; }
		}

		/// <summary>
		/// True to compute local mesh bounding-boxes
		/// </summary>
		public bool				GenerateBoundingBoxes
		{
			get { return m_bGenerateBoundingBoxes; }
			set { m_bGenerateBoundingBoxes = value; }
		}

		/// <summary>
		/// True to reset the transform of meshes to an identity matrix (discards animations)
		/// </summary>
		public bool				ResetXForm
		{
			get { return m_bResetXForm; }
			set { m_bResetXForm = value; }
		}

// Not supported yet !
// 		/// <summary>
// 		/// True to generate triangle strips instead of default triangls lists
// 		/// </summary>
// 		public bool				GenerateTriangleStrips
// 		{
// 			get { return m_bGenerateTriangleStrips; }
// 			set { m_bGenerateTriangleStrips = value; }
// 		}

		/// <summary>
		/// Compacts identical UV layers into a single layer
		/// </summary>
		public bool				CompactUVs
		{
			get { return m_bCompactUVs; }
			set { m_bCompactUVs = value; }
		}

		/// <summary>
		/// Gets or sets the minimum amount of UV sets to keep when compacting UVs
		/// </summary>
		public int				MinUVsCount
		{
			get { return m_MinUVsCount; }
			set { m_MinUVsCount = value; }
		}

		/// <summary>
		/// True to consolidate meshes and split primitives by difference in vertex infos
		/// (cf. consolidation options below)
		/// </summary>
		public bool				ConsolidateMeshes
		{
			get { return m_bConsolidateMeshes; }
			set { m_bConsolidateMeshes = value; }
		}

		/// <summary>
		/// True to split a vertex if it's shared by faces whose SMG differ
		/// </summary>
		public bool				ConsolidateSplitBySMG
		{
			get { return m_bConsolidateSplitBySMG; }
			set { m_bConsolidateSplitBySMG = value; }
		}

		/// <summary>
		/// True to split a vertex if it's shared by faces whose UVs differ
		/// </summary>
		public bool				ConsolidateSplitByUV
		{
			get { return m_bConsolidateSplitByUV; }
			set { m_bConsolidateSplitByUV = value; }
		}

		/// <summary>
		/// True to split a vertex if it's shared by faces whose colors differ
		/// </summary>
		public bool				ConsolidateSplitByColor
		{
			get { return m_bConsolidateSplitByColor; }
			set { m_bConsolidateSplitByColor = value; }
		}

// Not supported in Cirrus yet
// 		/// <summary>
// 		/// Compatcs all identical meshes into mesh instances, keeping only the difference between meshes
// 		/// </summary>
// 		public bool				CompactIdenticalMeshes
// 		{
// 			get { return m_bCompactIdenticalMeshes; }
// 			set { m_bCompactIdenticalMeshes = value; }
// 		}

		public DirectoryInfo	TargetTexturesBaseDirectory
		{
			get { return m_TargetTexturesBaseDirectory; }
			set { m_TargetTexturesBaseDirectory = value; }
		}

		public TEXTURE_CONVERSION_TYPES	ConvertDiffuse
		{
			get { return m_ConvertDiffuse; }
			set { m_ConvertDiffuse = value; }
		}
		public bool						GenerateMipMapsDiffuse
		{
			get { return m_bGenerateMipMapsDiffuse; }
			set { m_bGenerateMipMapsDiffuse = value; }
		}

		public TEXTURE_CONVERSION_TYPES	ConvertNormal
		{
			get { return m_ConvertNormal; }
			set { m_ConvertNormal = value; }
		}
		public bool						GenerateMipMapsNormal
		{
			get { return m_bGenerateMipMapsNormal; }
			set { m_bGenerateMipMapsNormal = value; }
		}

		public TEXTURE_CONVERSION_TYPES	ConvertRegular
		{
			get { return m_ConvertRegular; }
			set { m_ConvertRegular = value; }
		}
		public bool						GenerateMipMapsRegular
		{
			get { return m_bGenerateMipMapsRegular; }
			set { m_bGenerateMipMapsRegular = value; }
		}

		public int						JPEGQuality
		{
			get { return m_JPEGQuality; }
			set { m_JPEGQuality = value; }
		}

		#endregion

		#region METHODS

		public	SceneLoader( Device _Device, string _Name ) : base( _Device, _Name )
		{
		}

		/// <summary>
		/// Loads a FBX file into a Cirrus scene (which may already contain meshes and materials, don't care)
		/// </summary>
		/// <param name="_FileName">The name of the FBX file to load</param>
		/// <param name="_Scene">The cirrus scene into which we should store the data</param>
		/// <param name="_MaterialMap">The material map for FBX materials conversion into Nuaj materials</param>
		/// <param name="_TextureProvider">The texture provider that is capable of feeding our textures</param>
		public void	Load( FileInfo _FileName, Cirrus.Scene _Scene, Cirrus.MaterialMap _MaterialMap, Cirrus.Scene.ITextureProvider _TextureProvider )
		{
			Load( _FileName, _Scene, _MaterialMap, _TextureProvider, 1.0f );
		}

		/// <summary>
		/// Loads a FBX file into a Cirrus scene (which may already contain meshes and materials, don't care)
		/// </summary>
		/// <param name="_FileName">The name of the FBX file to load</param>
		/// <param name="_Scene">The cirrus scene into which we should store the data</param>
		/// <param name="_MaterialMap">The material map for FBX materials conversion into Nuaj materials</param>
		/// <param name="_TextureProvider">The texture provider that is capable of feeding our textures</param>
		/// <param name="_ScaleFactor">The scale factor to apply to the entire scene
		/// By default, internal MAX units can be considered as centimeters so if you create a scene whose dimensions of a one meter box are 100x100x100, you
		/// will want to use a scale factor of 0.01.
		/// FBX offers the possibility of scaling but does a shitty job at it as it doesn't even rescale other dimensions like near/far clips or ranges for lights
		///  and camera, which plain sucks.</param>
		public void	Load( FileInfo _FileName, Cirrus.Scene _Scene, Cirrus.MaterialMap _MaterialMap, Cirrus.Scene.ITextureProvider _TextureProvider, float _ScaleFactor )
		{
			if ( _FileName == null )
				throw new Exception( "Invalid file name!" );
			if ( !_FileName.Exists )
				throw new Exception( "Scene file \"" + _FileName + "\" does not exist!" );
			if ( _Scene == null )
				throw new Exception( "Invalid Cirrus Scene to load into!" );
			if ( _MaterialMap == null )
				throw new Exception( "Invalid material map for material conversion!" );

			m_Scene = _Scene;
			m_MaterialMap = _MaterialMap;
			m_Mesh2CirrusMesh.Clear();
			m_TextureProvider = _TextureProvider;
			m_ScaleFactor = _ScaleFactor;

			FBXImporter.Scene	FBXScene = null;
			try
			{
				FBXScene = new FBXImporter.Scene();
				FBXScene.Load( _FileName.FullName );

				// Process materials
				ProcessMaterials( FBXScene.Materials );

				// Process the scene nodes
				RecurseProcessNode( FBXScene.RootNode, null );

				// Attach camera & light targets
				PostProcessNodes( m_Scene.RootNode );

				// Build actual optimized and consolidated meshes
				BuildCirrusMeshes();

				// Propagate state once so Local2World matrices are up to date
				m_Scene.RootNode.PropagateState();
			}
			catch ( Exception _e )
			{
				throw new Exception( "An error occurred while importing the FBX file \"" + _FileName + "\"!", _e );
			}
			finally
			{
				FBXScene.Dispose();
			}
		}

		/// <summary>
		/// Attempts to map FBX materials to render techniques
		/// </summary>
		/// <param name="_Materials">The list of materials to process</param>
		protected void	ProcessMaterials( FBXImporter.Material[] _Materials )
		{
			m_Material2Technique.Clear();
			m_Material2Parameters.Clear();

			foreach ( FBXImporter.Material Material in _Materials )
			{
				Cirrus.Scene.MaterialParameters	MatParams = null;

// DEBUG
// if ( Material.Name == "sp_00_svod" )
// 	MatParams = null;
// DEBUG

				if ( Material is FBXImporter.MaterialHardwareShader )
				{
					FBXImporter.MaterialHardwareShader	SpecificMaterial = Material as FBXImporter.MaterialHardwareShader;

					MatParams = m_Scene.CreateMaterialParameters( Material.Name, SpecificMaterial.RelativeURL );

					foreach ( FBXImporter.MaterialHardwareShader.TableEntry Entry in SpecificMaterial.ShaderEntries )
					{
						switch ( Entry.TypeName )
						{
							case "Boolean":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = (bool) Entry.Value;
								break;
							case "Integer":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.INT ).AsInt.Value = (int) Entry.Value;
								break;
							case "Float":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = (float) Entry.Value;
								break;
							case "Float2":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT2 ).AsFloat2.Value = ConvertVector( Entry.Value as Vector2D );
								break;
							case "Float3":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertVector( Entry.Value as Vector );
								break;
							case "Float4":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT4 ).AsFloat4.Value = ConvertVector( Entry.Value as Vector4D );
								break;
							case "Matrix":
								MatParams.CreateParameter( Entry.Name, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.MATRIX4 ).AsMatrix4.Value = ConvertMatrix( Entry.Value as Matrix4x4 );
								break;
							case "Texture":
								CreateTextureParameter( Material, Entry.Name, MatParams, Entry.Name );
								break;
						}
					}
				}
				else if ( Material is FBXImporter.MaterialPhong )
				{
					FBXImporter.MaterialPhong	SpecificMaterial = Material as FBXImporter.MaterialPhong;

					MatParams = m_Scene.CreateMaterialParameters( Material.Name, "Phong" );

					// Lambert parameters
					MatParams.CreateParameter( "AmbientColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.AmbientColor );
					CreateTextureParameter( Material, "AmbientColor", MatParams, "AmbientTexture" );
					MatParams.CreateParameter( "AmbientFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.AmbientFactor;

					MatParams.CreateParameter( "DiffuseColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.DiffuseColor );
					MatParams.CreateParameter( "DiffuseFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.DiffuseFactor;
					bool	bHasDiffuseTexture = CreateTextureParameter( Material, "DiffuseColor", MatParams, "DiffuseTexture", "TransparentColor" );
					MatParams.CreateParameter( "HasDiffuseTexture", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasDiffuseTexture;

					MatParams.CreateParameter( "EmissiveColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.EmissiveColor );
					MatParams.CreateParameter( "EmissiveFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.EmissiveFactor;
					CreateTextureParameter( Material, "EmissiveColor", MatParams, "EmissiveTexture" );

					float	fOpacity = (float) (double) SpecificMaterial.FindProperty( "Opacity" ).Value;
					MatParams.CreateParameter( "Opacity", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = fOpacity;
					MatParams.IsOpaque = fOpacity >= 1.0f;

					bool	bHasNormalTexture = CreateTextureParameter( Material, "Bump", MatParams, "NormalTexture" );
					MatParams.CreateParameter( "HasNormalTexture", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasNormalTexture;

					// Phong parameters
					MatParams.CreateParameter( "ReflectionColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.ReflectionColor );
					CreateTextureParameter( Material, "ReflectionColor", MatParams, "ReflectionTexture" );
					MatParams.CreateParameter( "ReflectionFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.ReflectionFactor;

					MatParams.CreateParameter( "Shininess", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.Shininess;
					MatParams.CreateParameter( "SpecularColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.SpecularColor );
					MatParams.CreateParameter( "SpecularFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.SpecularFactor;
					bool	bHasSpecularTexture = CreateTextureParameter( Material, "SpecularFactor", MatParams, "SpecularTexture" );
					MatParams.CreateParameter( "HasSpecularTexture", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasSpecularTexture;
				}
				else if ( Material is FBXImporter.MaterialLambert )
				{
					FBXImporter.MaterialLambert	SpecificMaterial = Material as FBXImporter.MaterialLambert;

					MatParams = m_Scene.CreateMaterialParameters( Material.Name, "Lambert" );

					// Lambert parameters
					MatParams.CreateParameter( "AmbientColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.AmbientColor );
					CreateTextureParameter( Material, "AmbientColor", MatParams, "AmbientTexture" );
					MatParams.CreateParameter( "AmbientFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.AmbientFactor;

					MatParams.CreateParameter( "DiffuseColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.DiffuseColor );
					MatParams.CreateParameter( "DiffuseFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.DiffuseFactor;
					bool	bHasDiffuseTexture = CreateTextureParameter( Material, "DiffuseColor", MatParams, "DiffuseTexture", "TransparentColor" );
					MatParams.CreateParameter( "HasDiffuseTexture", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasDiffuseTexture;

					MatParams.CreateParameter( "EmissiveColor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = ConvertPoint( SpecificMaterial.EmissiveColor );
					MatParams.CreateParameter( "EmissiveFactor", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.EmissiveFactor;
					CreateTextureParameter( Material, "EmissiveColor", MatParams, "EmissiveTexture" );

					float	fOpacity = (float) (double) SpecificMaterial.FindProperty( "Opacity" ).Value;
					MatParams.CreateParameter( "Opacity", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = fOpacity;
					MatParams.IsOpaque = fOpacity >= 1.0f;

					bool	bHasNormalTexture = CreateTextureParameter( Material, "Bump", MatParams, "NormalTexture" );
					MatParams.CreateParameter( "HasNormalTexture", Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasNormalTexture;
				}
				else
					continue;	// Unrecognized hence unsupported material type...

				Cirrus.ITechniqueSupportsObjects	RT = m_MaterialMap.MapToTechnique( MatParams );
				if ( RT == null )
					continue;	// Failed to find a suitable technique for that material...

				// Register that material mapping
				m_Material2Technique[Material] = RT;
				m_Material2Parameters[Material] = MatParams;
			}
		}

		/// <summary>
		/// Processes a FBX node and converts it into a Cirrus object
		/// </summary>
		/// <param name="_FBXNode"></param>
		/// <param name="_ParentNode"></param>
		protected void	RecurseProcessNode( FBXImporter.Node _FBXNode, Cirrus.Scene.Node _ParentNode )
		{
			Cirrus.Scene.Node	NewNode = _ParentNode;
			if ( _FBXNode is FBXImporter.NodeMesh )
				NewNode = CreateMesh( _FBXNode as FBXImporter.NodeMesh, _ParentNode );	// Create a new mesh
			else if ( _FBXNode is FBXImporter.NodeCamera )
				NewNode = CreateCamera( _FBXNode as FBXImporter.NodeCamera, _ParentNode );
			else if ( _FBXNode is FBXImporter.NodeLight )
				NewNode = CreateLight( _FBXNode as FBXImporter.NodeLight, _ParentNode );
			else
			{	// Create a default node that is just here to respect the hierarchy for PRS evaluation
				Matrix4x4	LocalTransform = _FBXNode.LocalTransform;
				if ( _ParentNode == null )
				{	// Tweak the root node's transform to match DirectX representation
					LocalTransform.SetRow0( new Vector( 1.0f, 0.0f, 0.0f ) );
					LocalTransform.SetRow1( new Vector( 0.0f, 0.0f, -1.0f ) );
					LocalTransform.SetRow2( new Vector( 0.0f, 1.0f, 0.0f ) );
				}

				NewNode = m_Scene.CreateNode( _FBXNode.Name, _ParentNode, ConvertMatrix( LocalTransform, m_ScaleFactor ) );
			}

			// Recurse through children
			foreach ( FBXImporter.Node Child in _FBXNode.Children )
				RecurseProcessNode( Child, NewNode );
		}

		/// <summary>
		/// Post-processes nodes, attaches light and camera targets, that kind of stuff
		/// </summary>
		/// <param name="_Node"></param>
		protected void	PostProcessNodes( Cirrus.Scene.Node _Node )
		{
			if ( _Node is Cirrus.Scene.Camera )
				(_Node as Cirrus.Scene.Camera).TargetNode = m_Scene.FindNode( _Node.Name + ".Target", true );
			else if ( _Node is Cirrus.Scene.Light )
				(_Node as Cirrus.Scene.Light).TargetNode = m_Scene.FindNode( _Node.Name + ".Target", true );

			foreach ( Cirrus.Scene.Node Child in _Node.Children )
				PostProcessNodes( Child );
		}

		/// <summary>
		/// Creates a Cirrus mesh node
		/// </summary>
		/// <param name="_Scene"></param>
		/// <param name="_FBXMesh"></param>
		/// <returns></returns>
		protected Cirrus.Scene.Mesh	CreateMesh( FBXImporter.NodeMesh _FBXMesh, Cirrus.Scene.Node _Parent )
		{
			// Create a temporary mesh that will be optimized later, when all meshes have been loaded
			Mesh	TempMesh = new Mesh( this, _FBXMesh.Name );

			if ( m_bGenerateBoundingBoxes )
				TempMesh.BoundingBox = new BoundingBox( m_ScaleFactor * _FBXMesh.BoundingBox.m_Min, m_ScaleFactor * _FBXMesh.BoundingBox.m_Max );

			// Handle pivot & reset X-Form
			if ( m_bResetXForm )
				TempMesh.Pivot = _FBXMesh.Pivot;	// Storing the mesh's pivot will have the effect of composing vertices with that matrix, actually performing the "reset X-Form" operation
// 			else
// 				Transform.Pivot = _FBXNode.Pivot;	// Storing the mesh's pivot here will simply compose the mesh's transform with its pivot

			// Setup compulsory vertices and triangles
			Point[]	SourceVertices = _FBXMesh.Vertices;
			Point[]	ScaledVertices = new Point[SourceVertices.Length];
			for ( int VertexIndex=0; VertexIndex < _FBXMesh.VerticesCount; VertexIndex++ )
				ScaledVertices[VertexIndex] = m_ScaleFactor * SourceVertices[VertexIndex];

			TempMesh.SetVertices( ScaledVertices );
			TempMesh.SetFaces( _FBXMesh.Triangles );

			// Setup all the possible recognized layers
			foreach ( FBXImporter.Layer Layer in _FBXMesh.Layers )
				foreach ( FBXImporter.LayerElement LE in Layer.Elements )
				{
					switch ( LE.ElementType )
					{
						case FBXImporter.LayerElement.ELEMENT_TYPE.MATERIAL:
						case FBXImporter.LayerElement.ELEMENT_TYPE.UV:
						case FBXImporter.LayerElement.ELEMENT_TYPE.NORMAL:
						case FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT:
						case FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL:
						case FBXImporter.LayerElement.ELEMENT_TYPE.VERTEX_COLOR:
						case FBXImporter.LayerElement.ELEMENT_TYPE.SMOOTHING:
							TempMesh.AddLayerElement( LE );
							break;

						default:
							break;	// Other types are not supported (or irrelevant)...
					}
				}

			// Build un-optimized primitives
			TempMesh.BuildPrimitives();

			// Create the cirrus mesh and tie it to our temporary mesh
			Cirrus.Scene.Mesh	Mesh = m_Scene.CreateMesh( _FBXMesh.Name, _Parent, ConvertMatrix( _FBXMesh.LocalTransform, m_ScaleFactor ) );
			m_Mesh2CirrusMesh[TempMesh] = Mesh;

			// Add some properties
			Mesh.Visible = _FBXMesh.Visible;

// 			FBXImporter.ObjectProperty	PropertyCastShadow = _FBXMesh.FindUserProperty( "CastShadow" );
// 			Mesh.CastShadow = PropertyCastShadow != null ? (bool) PropertyCastShadow.Value : true;
			Mesh.CastShadow = true;
			Mesh.ReceiveShadow = true;

			return Mesh;
		}

		/// <summary>
		/// Creates a Cirrus camera node
		/// </summary>
		/// <param name="_FBXMesh"></param>
		/// <param name="_Parent"></param>
		/// <returns></returns>
		protected Cirrus.Scene.Camera	CreateCamera( FBXImporter.NodeCamera _FBXCamera, Cirrus.Scene.Node _Parent )
		{
			// Create the cirrus mesh and tie it to our temporary mesh
			Cirrus.Scene.Camera	Camera = m_Scene.CreateCamera( _FBXCamera.Name, _Parent, ConvertMatrix( _FBXCamera.LocalTransform, m_ScaleFactor ) );

			// Add some properties
			Camera.Visible = _FBXCamera.Visible;
			Camera.Type = (Cirrus.Scene.Camera.PROJECTION_TYPE) _FBXCamera.ProjectionType;
			Camera.Target = m_ScaleFactor * new SharpDX.Vector3( _FBXCamera.Target.x, _FBXCamera.Target.y, _FBXCamera.Target.z );
			Camera.FOV = _FBXCamera.FOVY;
			Camera.AspectRatio = (float) (Math.Tan( 0.5f * _FBXCamera.FOVX ) / Math.Tan( 0.5f * _FBXCamera.FOVY ));
			Camera.ClipNear = _FBXCamera.NearClipPlane * m_ScaleFactor;
			Camera.ClipFar = _FBXCamera.FarClipPlane * m_ScaleFactor;
			Camera.Roll = _FBXCamera.Roll;

			return Camera;
		}

		/// <summary>
		/// Creates a Cirrus light node
		/// </summary>
		/// <param name="_FBXMesh"></param>
		/// <param name="_Parent"></param>
		/// <returns></returns>
		protected Cirrus.Scene.Light	CreateLight( FBXImporter.NodeLight _FBXLight, Cirrus.Scene.Node _Parent )
		{
			// Create the cirrus mesh and tie it to our temporary mesh
			Cirrus.Scene.Light	Light = m_Scene.CreateLight( _FBXLight.Name, _Parent, ConvertMatrix( _FBXLight.LocalTransform, m_ScaleFactor ) );

			// Add some properties
			Light.Visible = _FBXLight.Visible;
			Light.Type = (Cirrus.Scene.Light.LIGHT_TYPE) _FBXLight.LightType;
			Light.CastShadow = _FBXLight.CastShadows;
			Light.Color = new SharpDX.Vector3( _FBXLight.Color.x, _FBXLight.Color.y, _FBXLight.Color.z );
			Light.Intensity = _FBXLight.Intensity;
			Light.EnableNearAttenuation = _FBXLight.EnableNearAttenuation;
			Light.NearAttenuationStart = _FBXLight.NearAttenuationStart * m_ScaleFactor;
			Light.NearAttenuationEnd = _FBXLight.NearAttenuationEnd * m_ScaleFactor;
			Light.EnableFarAttenuation = _FBXLight.EnableFarAttenuation;
			Light.FarAttenuationStart = _FBXLight.FarAttenuationStart * m_ScaleFactor;
			Light.FarAttenuationEnd = _FBXLight.FarAttenuationEnd * m_ScaleFactor;
			Light.HotSpot = _FBXLight.HotSpot;
			Light.ConeAngle = _FBXLight.ConeAngle;
			Light.DecayType = (Cirrus.Scene.Light.DECAY_TYPE) _FBXLight.DecayType;
			Light.DecayStart = _FBXLight.DecayStart * m_ScaleFactor;

			return Light;
		}

		/// <summary>
		/// Optimizes the existing meshes and build the primitives necessary for runtime display
		/// This will attempt to compact identical meshes and also consolidate mesh primitives
		/// </summary>
		protected void	BuildCirrusMeshes()
		{
			// 1] Retrieve all existing meshes and compact identical instances
			List<Mesh>	CompactedMeshes = new List<Mesh>();
			foreach ( Mesh M in m_Mesh2CirrusMesh.Keys )
			{
				// Check the existing meshes to see if they might be a master to this mesh
				if ( m_bCompactIdenticalMeshes )
					foreach ( Mesh MasterMesh in CompactedMeshes )
						if ( M.MergeWithMasterMesh( MasterMesh ) )
							break;	// We found this mesh's master !

				CompactedMeshes.Add( M );
			}

			// 2] Consolidate master meshes
//			WMath.Global.PushEpsilon( 1e-3f );	// Use this new epsilon for float comparisons in the Math library...

			foreach ( Mesh M in CompactedMeshes )
				M.PerformConsolidation();

//			WMath.Global.PopEpsilon();

			// 3] Convert the mesh into a clean Cirrus mesh
			foreach ( Mesh M in CompactedMeshes )
			{
				Cirrus.Scene.Mesh	TargetMesh = m_Mesh2CirrusMesh[M];

				// Setup basic mesh infos
                TargetMesh.BBox = new SharpDX.BoundingBox(new SharpDX.Vector3(M.BoundingBox.m_Min.x, M.BoundingBox.m_Min.y, M.BoundingBox.m_Min.z),
                                                          new SharpDX.Vector3(M.BoundingBox.m_Max.x, M.BoundingBox.m_Max.y, M.BoundingBox.m_Max.z));

				// I know it's a bit of a lousy approximation for the b-sphere but we can always refine it later...
                TargetMesh.BSphere = new SharpDX.BoundingSphere(new SharpDX.Vector3(M.BoundingBox.Center.x, M.BoundingBox.Center.y, M.BoundingBox.Center.z),
																0.5f * M.BoundingBox.Dim.Magnitude() );

				// Build primitives
				int	PrimitiveIndex = 0;
				foreach ( Mesh.Primitive P in M.ConsolidatedPrimitives )
				{
					if ( P.Material == null )
						throw new Exception( "Primitive \"" + P.Name + "\" has no assigned material!" );

					if ( !m_Material2Technique.ContainsKey( P.Material ) )
						continue;	// Un-supported...

					// Create the temporary feeder that will handle the primitive conversion
					PrimitiveFeeder	Feeder = new PrimitiveFeeder( P );

					Cirrus.ITechniqueSupportsObjects	RT = m_Material2Technique[P.Material];
					Cirrus.Scene.MaterialParameters		MatParams = m_Material2Parameters[P.Material];

					// Final check to see if signatures match
					if ( RT.RecognizedSignature.CheckMatch( Feeder.VertexSignature ) )
						TargetMesh.AddPrimitive( Feeder.CreatePrimitive( RT, TargetMesh, M.Name + "." + PrimitiveIndex++, MatParams ) );
				}
			}
		}

		#region Conversion Helpers

		protected static SharpDX.Vector3	ConvertPoint( Point _Value )
		{
            return new SharpDX.Vector3(_Value.x, _Value.y, _Value.z);
		}

        protected static SharpDX.Vector2 ConvertVector(Vector2D _Value)
		{
            return new SharpDX.Vector2(_Value.x, _Value.y);
		}

        protected static SharpDX.Vector3 ConvertVector(Vector _Value)
		{
            return new SharpDX.Vector3(_Value.x, _Value.y, _Value.z);
		}

        protected static SharpDX.Vector4 ConvertVector(Vector4D _Value)
		{
            return new SharpDX.Vector4(_Value.x, _Value.y, _Value.z, _Value.w);
		}

        protected static SharpDX.Matrix		ConvertMatrix( Matrix4x4 _Value )
		{
			return ConvertMatrix( _Value, 1.0f );
		}

        protected static SharpDX.Matrix		ConvertMatrix( Matrix4x4 _Value, float _PositionScaleFactor )
		{
            SharpDX.Matrix Result = new SharpDX.Matrix();
			Vector4D	Row = _Value.GetRow0();
			Result.Row1 = new SharpDX.Vector4( Row.x, Row.y, Row.z, Row.w );
			Row = _Value.GetRow1();
			Result.Row2 = new SharpDX.Vector4( Row.x, Row.y, Row.z, Row.w );
			Row = _Value.GetRow2();
			Result.Row3 = new SharpDX.Vector4( Row.x, Row.y, Row.z, Row.w );
			Point4D	Trans = _Value.GetTrans();
			Result.Row4 = new SharpDX.Vector4( _PositionScaleFactor * Trans.x, _PositionScaleFactor * Trans.y, _PositionScaleFactor * Trans.z, Trans.w );

			return Result;
		}

		/// <summary>
		/// Creates a texture parameter from a material property that contains a texture (e.g. DiffuseColor, SpecularColor, etc.)
		/// </summary>
		/// <param name="_Material"></param>
		/// <param name="_PropertyName"></param>
		/// <param name="_MaterialParameters"></param>
		/// <param name="_ParameterName"></param>
		/// <returns>True if a texture is available</returns>
		protected bool	CreateTextureParameter( FBXImporter.Material _Material, string _PropertyName, Cirrus.Scene.MaterialParameters _MaterialParameters, string _ParameterName )
		{
			return CreateTextureParameter( _Material, _PropertyName, _MaterialParameters, _ParameterName, null );
		}

		protected bool	CreateTextureParameter( FBXImporter.Material _Material, string _PropertyName, Cirrus.Scene.MaterialParameters _MaterialParameters, string _ParameterName, string _OpacityPropertyName )
		{
			FBXImporter.ObjectProperty	Property = _Material.FindProperty( _PropertyName );
			if ( Property == null )
				return false;	// No such property...
			if ( Property.Textures.Length == 0 )
				return false;	// That property has no texture...

			// Check for opacity
			string	OpacityTextureRelativeFileName = null;
			if ( _OpacityPropertyName != null )
			{
				FBXImporter.ObjectProperty	OpacityProperty = _Material.FindProperty( _OpacityPropertyName );
				if ( OpacityProperty != null && OpacityProperty.Textures.Length != 0 )
					OpacityTextureRelativeFileName = OpacityProperty.Textures[0].RelativeFileName;
			}

			// Create the parameter with that texture
			Cirrus.Scene.Texture2D	Tex = m_Scene.CreateTexture( Property.Textures[0].RelativeFileName, OpacityTextureRelativeFileName, Property.Textures[0].UseMipMap, m_TextureProvider );
			_MaterialParameters.CreateParameter( _ParameterName, Cirrus.Scene.MaterialParameters.PARAMETER_TYPE.TEXTURE2D ).AsTexture2D.Value = Tex;

			return true;
		}

		#endregion

		#endregion
	}
}
