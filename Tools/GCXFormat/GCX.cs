using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using RendererManaged;

namespace GCXFormat
{
	/// <summary>
	/// GodComplex Simple scene format
	/// </summary>
	public class	Scene
	{
		#region NESTED TYPES

		public delegate ushort	MaterialMapperDelegate( FBX.Scene.Materials.MaterialParameters.ParameterTexture2D _Texture );

		public class	Material
		{
			public ushort	m_ID = 0xFFFF;
			public float3	m_DiffuseColor = float3.One;
			public ushort	m_DiffuseTextureID = 0xFFFF;
			public float3	m_SpecularColor = float3.One;
			public ushort	m_SpecularTextureID = 0xFFFF;
			public float3	m_SpecularExponent = float3.One;
			public ushort	m_NormalTextureID = 0xFFFF;
			public float3	m_EmissiveColor = float3.One;

			public Material( BinaryReader _R )
			{
				m_ID = _R.ReadUInt16();
				m_DiffuseColor = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_DiffuseTextureID = _R.ReadUInt16();
				m_SpecularColor = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_SpecularTextureID = _R.ReadUInt16();
				m_SpecularExponent = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_NormalTextureID = _R.ReadUInt16();
				m_EmissiveColor = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );

				if ( _R.ReadUInt16() != 0x1234 )
					throw new Exception( "Failed to find material end marker!" );
			}

			public Material( FBX.Scene.Materials.MaterialParameters _SourceMaterial, MaterialMapperDelegate _Mapper )
			{
				float3	Temp = new float3();

				FBX.Scene.Materials.MaterialParameters.Parameter	P = null;

				m_ID = (ushort) _SourceMaterial.ID;

				// Get diffuse color + texture ID
				P = _SourceMaterial.Find( "DiffuseColor" );
				if ( P != null ) {
					Temp.FromVector3( P.AsFloat3.Value );
					m_DiffuseColor = Temp;
				}

				P = _SourceMaterial.Find( "DiffuseTexture" );
				m_DiffuseTextureID = _Mapper( P != null ? P.AsTexture2D : null );

				// Get specular color + texture ID + exponent
				P = _SourceMaterial.Find( "SpecularColor" );
				if ( P != null ) {
					Temp.FromVector3( P.AsFloat3.Value );
					m_SpecularColor = Temp;
				}

				P = _SourceMaterial.Find( "SpecularTexture" );
				m_SpecularTextureID = _Mapper( P != null ? P.AsTexture2D : null );

				P = _SourceMaterial.Find( "SpecularExponent" );
				if ( P != null ) {
					Temp.FromVector3( P.AsFloat3.Value );
					m_SpecularExponent = Temp;
				}

				// Get noral map ID
				P = _SourceMaterial.Find( "NormalTexture" );
				m_NormalTextureID = _Mapper( P != null ? P.AsTexture2D : null );

				// Get emissive
				P = _SourceMaterial.Find( "EmissiveColor" );
				if ( P != null ) {
					Temp.FromVector3( P.AsFloat3.Value );
					m_EmissiveColor = Temp;
				}
			}

			public void		Save( BinaryWriter _W )
			{
				// Write material ID
				_W.Write( m_ID );

				// Write diffuse color + texture ID
				_W.Write( m_DiffuseColor.x );
				_W.Write( m_DiffuseColor.y );
				_W.Write( m_DiffuseColor.z );

				_W.Write( m_DiffuseTextureID );

				// Write specular color + texture ID + specular exponent
				_W.Write( m_SpecularColor.x );
				_W.Write( m_SpecularColor.y );
				_W.Write( m_SpecularColor.z );

				_W.Write( m_SpecularTextureID );

				_W.Write( m_SpecularExponent.x );
				_W.Write( m_SpecularExponent.y );
				_W.Write( m_SpecularExponent.z );

				// Write normal map ID
				_W.Write( m_NormalTextureID );

				// Write emissive
				_W.Write( m_EmissiveColor.x );
				_W.Write( m_EmissiveColor.y );
				_W.Write( m_EmissiveColor.z );

				// Write end marker
				_W.Write( (ushort) 0x1234 );
			}
		}

		public class	Node
		{
			public enum	 TYPE
			{
				GENERIC = 0,
				MESH,
				LIGHT,
				CAMERA,

				// Special cases
				PROBE,
			}

			public Scene		m_Owner = null;
			public Node			m_Parent = null;
			public TYPE			m_Type = TYPE.GENERIC;
			public float4x4		m_Local2Parent = float4x4.Identity;
			public Node[]		m_Children = new Node[0];

			public object		m_Tag = null;	// User tag

			public Node( Scene _Owner, Node _Parent, BinaryReader _R )
			{
				m_Owner = _Owner;
				m_Owner.m_Nodes.Add( this );

				m_Parent = _Parent;

				// =============================
				// Load standard infos
//				m_Type = (TYPE) _R.ReadByte();	// Already consumed by the guy who called this constructor!

				// Load Local2Parent matrix
				m_Local2Parent.r0.x = _R.ReadSingle();	m_Local2Parent.r0.y = _R.ReadSingle();	m_Local2Parent.r0.z = _R.ReadSingle();	m_Local2Parent.r0.w = _R.ReadSingle();
				m_Local2Parent.r1.x = _R.ReadSingle();	m_Local2Parent.r1.y = _R.ReadSingle();	m_Local2Parent.r1.z = _R.ReadSingle();	m_Local2Parent.r1.w = _R.ReadSingle();
				m_Local2Parent.r2.x = _R.ReadSingle();	m_Local2Parent.r2.y = _R.ReadSingle();	m_Local2Parent.r2.z = _R.ReadSingle();	m_Local2Parent.r2.w = _R.ReadSingle();
				m_Local2Parent.r3.x = _R.ReadSingle();	m_Local2Parent.r3.y = _R.ReadSingle();	m_Local2Parent.r3.z = _R.ReadSingle();	m_Local2Parent.r3.w = _R.ReadSingle();


				// =============================
				// Write specialized infos
				LoadSpecialized( _R );


				// =============================
				// Write end marker
				if ( _R.ReadUInt16() != 0xABCD )
					throw new Exception( "Failed to reach end node marker!" );


				// =============================
				// Recurse through children
				m_Children = new Node[_R.ReadUInt16()];
				for ( int ChildIndex=0; ChildIndex < m_Children.Length; ChildIndex++ )
				{
					TYPE	ChildType = (TYPE) _R.ReadByte();
					Node	Child = null;
					switch ( ChildType )
					{
						case TYPE.GENERIC:
						case TYPE.PROBE:
							Child = new Node( _Owner, this, _R );
							break;

						case TYPE.LIGHT:	Child = new Light( _Owner, this, _R ); break;
						case TYPE.CAMERA:	Child = new Camera( _Owner, this, _R ); break;
						case TYPE.MESH:		Child = new Mesh( _Owner, this, _R ); break;
						default: throw new Exception( "Unsupported node type!" );
					}
					m_Children[ChildIndex] = Child;
				}
			}

			public Node( Scene _Owner, FBX.Scene.Nodes.Node _Node )
			{
				m_Owner = _Owner;
				m_Owner.m_Nodes.Add( this );

				if ( _Node is FBX.Scene.Nodes.Mesh )
					m_Type = TYPE.MESH;
				else if ( _Node is FBX.Scene.Nodes.Camera )
					m_Type = TYPE.CAMERA;
				else if ( _Node is FBX.Scene.Nodes.Light )
					m_Type = TYPE.LIGHT;
				else
				{
					// Isolate locators as probes
					if ( _Node.Name.ToLower().IndexOf( "locator" ) != -1 )
						m_Type = TYPE.PROBE;
				}

				m_Local2Parent.FromMatrix4( _Node.Local2Parent );

				// Build children
				FBX.Scene.Nodes.Node[]	Children = _Node.Children;
				m_Children = new Node[Children.Length];
				for ( int ChildIndex=0; ChildIndex < Children.Length; ChildIndex++ )
				{
					FBX.Scene.Nodes.Node	SourceChild = Children[ChildIndex];
					Node	Child = null;
					switch ( SourceChild.NodeType )
					{
						case FBX.Scene.Nodes.Node.NODE_TYPE.NODE:	Child = new Node( _Owner, SourceChild ); break;
						case FBX.Scene.Nodes.Node.NODE_TYPE.LIGHT:	Child = new Light( _Owner, SourceChild ); break;
						case FBX.Scene.Nodes.Node.NODE_TYPE.CAMERA:	Child = new Camera( _Owner, SourceChild ); break;
						case FBX.Scene.Nodes.Node.NODE_TYPE.MESH:	Child = new Mesh( _Owner, SourceChild ); break;
					}
					m_Children[ChildIndex] = Child;
				}
			}

			// 
			public Node( Scene _Owner, idTech5Map.Map _Map ) {
				m_Owner = _Owner;
				m_Owner.m_Nodes.Add( this );

				List< Node >	Children = new List< Node >();
				foreach ( idTech5Map.Map.Entity E in _Map.m_Entities ) {
					switch ( E.m_Type ) {
						case idTech5Map.Map.Entity.TYPE.MODEL:
							m_Type = TYPE.MESH;
							Children.Add( new Mesh( _Owner, E ) );
							break;
						case idTech5Map.Map.Entity.TYPE.LIGHT:
							m_Type = TYPE.LIGHT;
							Children.Add( new Light( _Owner, E ) );
							break;
						case idTech5Map.Map.Entity.TYPE.PLAYER_START:
							m_Type = TYPE.CAMERA;
							Children.Add( new Camera( _Owner, E ) );
							break;
						case idTech5Map.Map.Entity.TYPE.PROBE:
							m_Type = TYPE.PROBE;
							Children.Add( new Node( _Owner, E ) );
							break;
						case idTech5Map.Map.Entity.TYPE.UNKNOWN:
						case idTech5Map.Map.Entity.TYPE.REF_MAP:
							// Don't care...
							break;
					}
				}
				m_Children = Children.ToArray();
			}
			public Node( Scene _Owner, idTech5Map.Map.Entity _Entity ) {
				m_Owner = _Owner;

				float3	X = new float3( _Entity.m_Local2World.r0.x, _Entity.m_Local2World.r1.x, _Entity.m_Local2World.r2.x );
				float3	Y = new float3( _Entity.m_Local2World.r0.y, _Entity.m_Local2World.r1.y, _Entity.m_Local2World.r2.y );
				float3	Z = new float3( _Entity.m_Local2World.r0.z, _Entity.m_Local2World.r1.z, _Entity.m_Local2World.r2.z );
				float3	P = new float3( _Entity.m_Local2World.r0.w, _Entity.m_Local2World.r1.w, _Entity.m_Local2World.r2.w );

				X = ConvTech5( X );
				Y = ConvTech5( Y );
				Z = ConvTech5( Z );
				P = ConvTech5( P );

				m_Local2Parent.r0.Set( X, 0 );
				m_Local2Parent.r1.Set( Y, 0 );
				m_Local2Parent.r2.Set( Z, 0 );
				m_Local2Parent.r3.Set( P, 1 );
			}
			protected static float3	ConvTech5( float3 V ) {
				return new float3( V.x, V.z, -V.y );
			}


			public void	Save( BinaryWriter _W )
			{
				// =============================
				// Write standard infos
				_W.Write( (byte) m_Type );

					// Write Local2Parent matrix
				_W.Write( m_Local2Parent.r0.x );	_W.Write( m_Local2Parent.r0.y );	_W.Write( m_Local2Parent.r0.z );	_W.Write( m_Local2Parent.r0.w );
				_W.Write( m_Local2Parent.r1.x );	_W.Write( m_Local2Parent.r1.y );	_W.Write( m_Local2Parent.r1.z );	_W.Write( m_Local2Parent.r1.w );
				_W.Write( m_Local2Parent.r2.x );	_W.Write( m_Local2Parent.r2.y );	_W.Write( m_Local2Parent.r2.z );	_W.Write( m_Local2Parent.r2.w );
				_W.Write( m_Local2Parent.r3.x );	_W.Write( m_Local2Parent.r3.y );	_W.Write( m_Local2Parent.r3.z );	_W.Write( m_Local2Parent.r3.w );


				// =============================
				// Write specialized infos
				SaveSpecialized( _W );


				// =============================
				// Write end marker
				_W.Write( (ushort) 0xABCD );


				// =============================
				// Recurse through children
				_W.Write( (ushort) m_Children.Length );
				foreach ( Node Child in m_Children )
					Child.Save( _W );
			}

			protected virtual void	LoadSpecialized( BinaryReader _R )	{}
			protected virtual void	SaveSpecialized( BinaryWriter _W )	{}
		}

		public class	Light : Node
		{
			public enum	LIGHT_TYPE
			{
				POINT = 0,
				DIRECTIONAL = 1,
				SPOT = 2,
			}

			public LIGHT_TYPE	m_LightType;
			public float3		m_Color = float3.One;
			public float		m_Intensity = 0.0f;
			public float		m_HotSpot = 0.0f;
			public float		m_ConeAngle = 0.0f;

			public Light( Scene _Owner, Node _Parent, BinaryReader _R ) : base( _Owner, _Parent, _R )
			{
			}

			public Light( Scene _Owner, FBX.Scene.Nodes.Node _Node ) : base( _Owner, _Node )
			{
				FBX.Scene.Nodes.Light _Light = _Node as FBX.Scene.Nodes.Light;

				m_LightType = (LIGHT_TYPE) _Light.Type;
				m_Color.FromVector3( _Light.Color );
				m_Intensity = _Light.Intensity;
				m_HotSpot = _Light.HotSpot;
				m_ConeAngle = _Light.ConeAngle;
			}

			public Light( Scene _Owner, idTech5Map.Map.Entity _Entity ) : base( _Owner, _Entity ) {
				m_LightType = LIGHT_TYPE.POINT;
// TODO
// 				m_LightType = (LIGHT_TYPE) _Light.Type;
// 				m_Color.FromVector3( _Light.Color );
// 				m_Intensity = _Light.Intensity;
// 				m_HotSpot = _Light.HotSpot;
// 				m_ConeAngle = _Light.ConeAngle;
			}

			protected override void	SaveSpecialized( BinaryWriter _W )
			{
				_W.Write( (byte) m_LightType );
				_W.Write( m_Color.x );
				_W.Write( m_Color.y );
				_W.Write( m_Color.z );
				_W.Write( m_Intensity );
				_W.Write( m_HotSpot );
				_W.Write( m_ConeAngle );
			}

			protected override void LoadSpecialized( BinaryReader _R )
			{
				m_LightType = (LIGHT_TYPE) _R.ReadByte();
				m_Color = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_Intensity = _R.ReadSingle();
				m_HotSpot = _R.ReadSingle();
				m_ConeAngle = _R.ReadSingle();
			}
		}

		public class	Camera : Node
		{
			public float	m_FOV = 0.0f;

			public Camera( Scene _Owner, Node _Parent, BinaryReader _R ) : base( _Owner, _Parent, _R )
			{
			}

			public Camera( Scene _Owner, FBX.Scene.Nodes.Node _Node ) : base( _Owner, _Node )
			{
				FBX.Scene.Nodes.Camera _Camera = _Node as FBX.Scene.Nodes.Camera;
				m_FOV = _Camera.FOV;
			}

			public Camera( Scene _Owner, idTech5Map.Map.Entity _Entity ) : base( _Owner, _Entity ) {
				m_FOV = (float) (60.0 * Math.PI / 180.0);
			}

			protected override void	SaveSpecialized( BinaryWriter _W )
			{
				_W.Write( m_FOV );
			}

			protected override void LoadSpecialized( BinaryReader _R )
			{
				m_FOV = _R.ReadSingle();
			}
		}

		public class	Mesh : Node
		{
			public enum	VERTEX_FORMAT
			{
				P3N3G3B3T2 = 0,
			}

			public class	Primitive
			{
				[System.Diagnostics.DebuggerDisplay( "[{V0}, {V1}, {V2]]" )]
				public struct	Face
				{
					public int		V0, V1, V2;
				}

				[System.Diagnostics.DebuggerDisplay( "P={P} N={N} G={G] B={B} T={T}" )]
				public struct	Vertex
				{
					public float3	P;	// Position
					public float3	N;	// Normal
					public float3	G;	// Tangent
					public float3	B;	// BiTangent
					public float2	T;	// Texcoords

					public void	Load( BinaryReader _R )
					{
						P = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						N = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						G = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						B = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						T = new float2( _R.ReadSingle(), _R.ReadSingle() );
					}

					public void	Save( BinaryWriter _W )
					{
						_W.Write( P.x );
						_W.Write( P.y );
						_W.Write( P.z );
						_W.Write( N.x );
						_W.Write( N.y );
						_W.Write( N.z );
						_W.Write( G.x );
						_W.Write( G.y );
						_W.Write( G.z );
						_W.Write( B.x );
						_W.Write( B.y );
						_W.Write( B.z );
						_W.Write( T.x );
						_W.Write( T.y );
					}
				}

				public Mesh			m_Owner = null;

				public ushort		m_MaterialID = 0xFFFF;
				public float3		m_BBoxMin = float3.Zero;
				public float3		m_BBoxMax = float3.Zero;
				public Face[]		m_Faces = null;
				public Vertex[]		m_Vertices = null;

				// Temporary data, not saved
				public int			m_FaceOffset = 0;	// Absolute offset to add to this primitive's faces to obtain an absolute unique face index
				public int			m_VertexOffset = 0;	// Absolute offset to add to this primitive's vertices to obtain an absolute unique vertex index

				public Primitive( Mesh _Owner, BinaryReader _R )
				{
					m_Owner = _Owner;

					m_MaterialID = _R.ReadUInt16();

					m_Faces = new Face[_R.ReadUInt32()];
					m_Vertices = new Vertex[_R.ReadUInt32()];

					// Read local bounding box
					m_BBoxMin.x = _R.ReadSingle();
					m_BBoxMin.y = _R.ReadSingle();
					m_BBoxMin.z = _R.ReadSingle();
					m_BBoxMax.x = _R.ReadSingle();
					m_BBoxMax.y = _R.ReadSingle();
					m_BBoxMax.z = _R.ReadSingle();

					// Read faces
					if ( m_Vertices.Length <= 65536 )
					{
						for ( int FaceIndex=0; FaceIndex < m_Faces.Length; FaceIndex++ )
						{
							m_Faces[FaceIndex].V0 = (int) _R.ReadUInt16();
							m_Faces[FaceIndex].V1 = (int) _R.ReadUInt16();
							m_Faces[FaceIndex].V2 = (int) _R.ReadUInt16();
							if ( m_Faces[FaceIndex].V0 >= m_Vertices.Length ) throw new Exception( "Vertex index out of range for face #" + FaceIndex + "!" );
							if ( m_Faces[FaceIndex].V1 >= m_Vertices.Length ) throw new Exception( "Vertex index out of range for face #" + FaceIndex + "!" );
							if ( m_Faces[FaceIndex].V2 >= m_Vertices.Length ) throw new Exception( "Vertex index out of range for face #" + FaceIndex + "!" );
						}
					}
					else
					{
						for ( int FaceIndex=0; FaceIndex < m_Faces.Length; FaceIndex++ )
						{
							m_Faces[FaceIndex].V0 = (int) _R.ReadUInt32();
							m_Faces[FaceIndex].V1 = (int) _R.ReadUInt32();
							m_Faces[FaceIndex].V2 = (int) _R.ReadUInt32();
							if ( m_Faces[FaceIndex].V0 >= m_Vertices.Length ) throw new Exception( "Vertex index out of range for face #" + FaceIndex + "!" );
							if ( m_Faces[FaceIndex].V1 >= m_Vertices.Length ) throw new Exception( "Vertex index out of range for face #" + FaceIndex + "!" );
							if ( m_Faces[FaceIndex].V2 >= m_Vertices.Length ) throw new Exception( "Vertex index out of range for face #" + FaceIndex + "!" );
						}
					}

					// Read vertex format and ensure we can handle it
					VERTEX_FORMAT	Format = (VERTEX_FORMAT) _R.ReadByte();
					if ( Format != VERTEX_FORMAT.P3N3G3B3T2 )
						throw new Exception( "Unsupported vertex format!" );

					// Read vertices
					foreach ( Vertex V in m_Vertices )
						V.Load( _R );

					// Store absolute vertex offset
					m_VertexOffset = _Owner.m_Owner.m_TotalVerticesCount;
					_Owner.m_Owner.m_TotalVerticesCount += m_Vertices.Length;	// Increase global vertices counter

					m_FaceOffset = _Owner.m_Owner.m_TotalFacesCount;
					_Owner.m_Owner.m_TotalFacesCount += m_Faces.Length;		// Increase global faces counter
				}

				public Primitive( Mesh _Owner, FBX.Scene.Nodes.Mesh.Primitive _Primitive )
				{
					m_Owner = _Owner;

					m_MaterialID = (ushort) _Primitive.MaterialParms.ID;

					m_Faces = new Face[_Primitive.FacesCount];
					m_Vertices = new Vertex[_Primitive.VerticesCount];

					// Retrieve streams
					FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE[]	Usages = {
						FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION,
						FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.NORMAL,
						FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TANGENT,
						FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.BITANGENT,
						FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TEXCOORDS,
					};
					FBX.Scene.Nodes.Mesh.Primitive.VertexStream[][]		Streams = new FBX.Scene.Nodes.Mesh.Primitive.VertexStream[Usages.Length][];
					for ( int UsageIndex=0; UsageIndex < Usages.Length; UsageIndex++ )
					{
						Streams[UsageIndex] = _Primitive.FindStreamsByUsage( Usages[UsageIndex] );
						if ( Streams[UsageIndex].Length == 0 )
							throw new Exception( "No stream for usage " + Usages[UsageIndex] + "! Can't complete target vertex format!" );
					}

					// Build local space bounding box
					float3	Temp = new float3();
					WMath.Vector[]	VertexPositions = Streams[0][0].Content as WMath.Vector[];
					foreach ( WMath.Vector VertexPosition in VertexPositions ) {
						Temp.FromVector3( VertexPosition );
						m_BBoxMin.Min( Temp );
						m_BBoxMax.Max( Temp );
					}

					// Build faces
					int	FaceIndex = 0;
					foreach ( FBX.Scene.Nodes.Mesh.Primitive.Face F in _Primitive.Faces )
					{
						m_Faces[FaceIndex].V0 = F.V0;
						m_Faces[FaceIndex].V1 = F.V1;
						m_Faces[FaceIndex].V2 = F.V2;
						FaceIndex++;
					}

					// Build vertices
					for ( int VertexIndex=0; VertexIndex < m_Vertices.Length; VertexIndex++ )
					{
						for ( int UsageIndex=0; UsageIndex < Usages.Length; UsageIndex++ )
						{
							FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE	Usage = Usages[UsageIndex];
							switch ( Usage )
							{
								case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION:
								{
									float3[]	Stream = Streams[UsageIndex][0].Content as float3[];
									m_Vertices[VertexIndex].P = Stream[VertexIndex];
									break;
								}
								case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.NORMAL:
								{
									float3[]	Stream = Streams[UsageIndex][0].Content as float3[];
									m_Vertices[VertexIndex].N = Stream[VertexIndex];
									break;
								}
								case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TANGENT:
								{
									float3[]	Stream = Streams[UsageIndex][0].Content as float3[];
									m_Vertices[VertexIndex].G = Stream[VertexIndex];
									break;
								}
								case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.BITANGENT:
								{
									float3[]	Stream = Streams[UsageIndex][0].Content as float3[];
									m_Vertices[VertexIndex].B = Stream[VertexIndex];
									break;
								}
								case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TEXCOORDS:
								{
									float2[]	Stream = Streams[UsageIndex][0].Content as float2[];
									m_Vertices[VertexIndex].T = Stream[VertexIndex];
									break;
								}
							}
						}
					}
				}

				public Primitive( Mesh _Owner, idTech5Map.Model.Surface _Surface )
				{
					m_Owner = _Owner;

					m_MaterialID = (ushort) _Surface.m_Material.m_MaterialIndex;

					int	FacesCount = _Surface.m_Indices.Length / 3;

					m_Faces = new Face[FacesCount];
					m_Vertices = new Vertex[_Surface.m_Vertices.Length];

					m_BBoxMin = ConvTech5( _Surface.m_BoundsMin );
					m_BBoxMax = ConvTech5( _Surface.m_BoundsMax );

					// Build faces
					int	i = 0;
					for ( int FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ ) {
						m_Faces[FaceIndex].V0 = _Surface.m_Indices[i++];
						m_Faces[FaceIndex].V1 = _Surface.m_Indices[i++];
						m_Faces[FaceIndex].V2 = _Surface.m_Indices[i++];
					}

					// Build vertices
					for ( int VertexIndex=0; VertexIndex < m_Vertices.Length; VertexIndex++ ) {
						idTech5Map.Model.Surface.Vertex	V = _Surface.m_Vertices[VertexIndex];
						m_Vertices[VertexIndex].P = ConvTech5( V.Position );
						m_Vertices[VertexIndex].N = ConvTech5( V.Normal );
						m_Vertices[VertexIndex].G = ConvTech5( V.Tangent );
						m_Vertices[VertexIndex].B = ConvTech5( V.BiTangent );
						m_Vertices[VertexIndex].T = V.UVs[0];
					}
				}

				public void	Save( BinaryWriter _W )
				{
					// Write material ID
					_W.Write( m_MaterialID );

					_W.Write( (UInt32) m_Faces.Length );
					_W.Write( (UInt32) m_Vertices.Length );

					// Write local space bounding box
					_W.Write( m_BBoxMin.x );
					_W.Write( m_BBoxMin.y );
					_W.Write( m_BBoxMin.z );
					_W.Write( m_BBoxMax.x );
					_W.Write( m_BBoxMax.y );
					_W.Write( m_BBoxMax.z );

					// Write faces
					if ( m_Vertices.Length <= 65536 )
					{
						foreach ( Face F in m_Faces )
						{
							_W.Write( (UInt16) F.V0 );
							_W.Write( (UInt16) F.V1 );
							_W.Write( (UInt16) F.V2 );
						}
					}
					else
					{
						foreach ( Face F in m_Faces )
						{
							_W.Write( (UInt32) F.V0 );
							_W.Write( (UInt32) F.V1 );
							_W.Write( (UInt32) F.V2 );
						}
					}

					// Wwrite streams
					_W.Write( (byte) VERTEX_FORMAT.P3N3G3B3T2 );
					foreach ( Vertex V in m_Vertices )
						V.Save( _W );
				}

				#region HELPERS

				[System.Diagnostics.DebuggerDisplay( "[{V0}, {V1}] L={LeftFace} R={RightFace}" )]
				public class	Edge
				{
					public int	V0, V1;
					public int	LeftFace, RightFace;

					public override int GetHashCode()
					{
						return V0 ^ V1;
					}

					// An edge is equal regardless of its orientation
					public override bool Equals( object obj )
					{
						Edge	other = obj as Edge;
						if ( other == null )
							return false;

						return (other.V0 == V0 && other.V1 == V1) || (other.V0 == V1 && other.V1 == V0);
					}

					public int	GetOtherFaceIndex( int _FaceIndex )
					{
						if ( _FaceIndex == LeftFace )
							return	RightFace;
						else if ( _FaceIndex == RightFace )
							return LeftFace;
						else
							throw new Exception( "The edge doesn't belong to the provided face!" );
					}
				}

				public class	WingedEdgeTriangle
				{
					public int		m_Index;
					public Edge[]	m_Edges;
					public object	m_Tag;
				}

				public WingedEdgeTriangle[]	m_WingedEdgeFaces = new WingedEdgeTriangle[0];

				/// <summary>
				/// Builds a winged-edges mesh from the primitive
				/// </summary>
				public void		BuildWingedEdgesMesh()
				{
					m_WingedEdgeFaces = new WingedEdgeTriangle[m_Faces.Length];

					Dictionary<Edge,Edge>	Hash2Edge = new Dictionary<Edge,Edge>();
					for ( int FaceIndex=0; FaceIndex < m_Faces.Length; FaceIndex++ )
					{
						Face	F = m_Faces[FaceIndex];
						Edge	E0 = new Edge() { V0=F.V0, V1=F.V1, LeftFace=FaceIndex, RightFace=-1 };
						Edge	E1 = new Edge() { V0=F.V1, V1=F.V2, LeftFace=FaceIndex, RightFace=-1 };
						Edge	E2 = new Edge() { V0=F.V2, V1=F.V0, LeftFace=FaceIndex, RightFace=-1 };
						
						if ( !Hash2Edge.ContainsKey( E0 ) )
							Hash2Edge.Add( E0, E0 );
						else
						{	// Re-use the edge from an existing face
							E0 = Hash2Edge[E0];
							if ( E0.V0 == F.V0 )
								throw new Exception( "Existing edge has the same winding as this face! We should be its adjacent face and it should be winded the other way!" );
							if ( E0.RightFace != -1 )
								throw new Exception( "Existing edge already has a right face! We should be the one to be on the right, that means this edge is shared by more than 2 faces?!" );
							E0.RightFace = FaceIndex;
						}

						if ( !Hash2Edge.ContainsKey( E1 ) )
							Hash2Edge.Add( E1, E1 );
						else
						{	// Re-use the edge from an existing face
							E1 = Hash2Edge[E1];
							if ( E1.V0 == F.V1 )
								throw new Exception( "Existing edge has the same winding as this face! We should be its adjacent face and it should be winded the other way!" );
							if ( E1.RightFace != -1 )
								throw new Exception( "Existing edge already has a right face! We should be the one to be on the right, that means this edge is shared by more than 2 faces?!" );
							E1.RightFace = FaceIndex;
						}

						if ( !Hash2Edge.ContainsKey( E2 ) )
							Hash2Edge.Add( E2, E2 );
						else
						{	// Re-use the edge from an existing face
							E2 = Hash2Edge[E2];
							if ( E2.V0 == F.V2 )
								throw new Exception( "Existing edge has the same winding as this face! We should be its adjacent face and it should be winded the other way!" );
							if ( E2.RightFace != -1 )
								throw new Exception( "Existing edge already has a right face! We should be the one to be on the right, that means this edge is shared by more than 2 faces?!" );
							E2.RightFace = FaceIndex;
						}

						// We finally have our list of edges
						WingedEdgeTriangle	Triangle = new WingedEdgeTriangle() {
							m_Index = FaceIndex,
							m_Edges  = new Edge[3] { E0, E1, E2 }
						};
						m_WingedEdgeFaces[FaceIndex] = Triangle;
					}
				}

				#endregion
			}

			public Primitive[]	m_Primitives = new Primitive[0];

			public Mesh( Scene _Owner, Node _Parent, BinaryReader _R ) : base( _Owner, _Parent, _R )
			{
			}

			public Mesh( Scene _Owner, FBX.Scene.Nodes.Node _Node ) : base( _Owner, _Node )
			{
				FBX.Scene.Nodes.Mesh _Mesh = _Node as FBX.Scene.Nodes.Mesh;
				m_Primitives = new Primitive[_Mesh.PrimitivesCount];
				int	PrimitiveIndex = 0;
				foreach ( FBX.Scene.Nodes.Mesh.Primitive SourcePrimitive in _Mesh.Primitives )
					m_Primitives[PrimitiveIndex++] = new Primitive( this, SourcePrimitive );
			}

			public Mesh( Scene _Owner, idTech5Map.Map.Entity _Entity ) : base( _Owner, _Entity ) {
				m_Primitives = new Primitive[_Entity.m_Model.m_Surfaces.Length];
				int	PrimitiveIndex = 0;
				foreach ( idTech5Map.Model.Surface S in _Entity.m_Model.m_Surfaces )
					m_Primitives[PrimitiveIndex++] = new Primitive( this, S );
			}

			protected override void	SaveSpecialized( BinaryWriter _W )
			{
				// Write primitives
				_W.Write( (ushort) m_Primitives.Length );
				foreach ( Primitive P in m_Primitives )
					P.Save( _W );
			}

			protected override void LoadSpecialized( BinaryReader _R )
			{
				m_Primitives = new Primitive[_R.ReadUInt16()];
				for ( int PrimitiveIndex=0; PrimitiveIndex < m_Primitives.Length; PrimitiveIndex++ )
					m_Primitives[PrimitiveIndex] = new Primitive( this, _R );
			}
		}

		#endregion

		#region FIELDS

		public List<Material>	m_Materials = new List<Material>();

		// Nodes hierarchy
		public Node				m_RootNode = null;
		public List<Node>		m_Nodes = new List<Node>();	// Collapsed list

		// Statistics
		public int				m_TotalVerticesCount = 0;
		public int				m_TotalFacesCount = 0;

		#endregion

		#region METHODS

		public Scene( BinaryReader _R )
		{
			if ( _R.ReadUInt32() != 0x31584347L )
				throw new Exception( "Unexpected header signature: unsupported format!" );

			// Read materials
			m_Materials = new List<Material>();
			int	MaterialsCount = (int) _R.ReadUInt16();
			for ( int MaterialIndex=0; MaterialIndex < MaterialsCount; MaterialIndex++ )
				m_Materials.Add( new Material( _R ) );

			// Read nodes
			Node.TYPE	RootNodeType = (Node.TYPE) _R.ReadByte();	// Consume type, but we know it's a simple generic node...

			m_RootNode = new Node( this, null, _R );
		}

		/// <summary>
		/// Convert from FBX
		/// </summary>
		/// <param name="_Scene"></param>
		public Scene( FBX.Scene.Scene _Scene )
		{
			// Create materials
			FBX.Scene.Materials.MaterialParameters[]	SourceMaterials = _Scene.MaterialParameters;
			foreach ( FBX.Scene.Materials.MaterialParameters SourceMaterial in SourceMaterials )
				m_Materials.Add( new Material( SourceMaterial, MapMaterial ) );

			// Create nodes
			if ( _Scene.RootNode != null )
				m_RootNode = new Node( this, _Scene.RootNode );
		}

		/// <summary>
		/// Convert from idTech5 map
		/// </summary>
		/// <param name="_Map"></param>
		public Scene( idTech5Map.Map _Map ) {

			// Create nodes
			m_RootNode = new Node( this, _Map );

			// Create materials

		}

		public void	Save( BinaryWriter _W )
		{
			_W.Write( (UInt32) 0x31584347L );	// "GCX1"

			//////////////////////////////////////////////////////////////////////////
			// Write materials
			//
			_W.Write( (ushort) m_Materials.Count );
			foreach ( Material M in m_Materials )
				M.Save( _W );

			//////////////////////////////////////////////////////////////////////////
			// Write nodes
			m_RootNode.Save( _W );
		}

		private ushort	MapMaterial( FBX.Scene.Materials.MaterialParameters.ParameterTexture2D _Texture )
		{
			if ( _Texture == null )
				return (ushort) 0xFFFF;

// 			if ( _Texture.Value.URL.IndexOf( "pata_diff_colo.tga" ) != -1 )
// 				return 0;

			return (ushort) _Texture.Value.ID;
		}

		#endregion
	}
}
