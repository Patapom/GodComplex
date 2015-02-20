using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RendererManaged;

namespace idTech5Map
{
	/// <summary>
	/// Contains the surfaces & materials to render a 3D model
	/// </summary>
	public class Model {

		/// <summary>
		/// Describes a unique 3D triangle list primitive
		/// </summary>
		public class	Surface {

			#region NESTED TYPES

			[System.Diagnostics.DebuggerDisplay( "{m_Info}" )]
			public struct VertexElement {
				public uint		m_format;
				public byte		m_usage;
				public byte		m_usageIndex;
				public byte		m_offset;
				public byte		m_stream;
				string			m_Info;

				delegate float4	Reader( BinaryReader _R );
				Reader			m_Reader;
				delegate void	Writer( Vertex _Vertex, float4 _Value );
				Writer			m_Writer;

				public void		Read( BinaryReader _R ) {
					m_format = _R.ReadUInt32();
					m_usage = _R.ReadByte();
					m_usageIndex = _R.ReadByte();
					m_offset = _R.ReadByte();
					m_stream = _R.ReadByte();

					// Decode format
					switch ( m_format ) {
						case 0x09070100: m_Reader = ReaderRGB32F; break;
						case 0x05070100: m_Reader = ReaderRG16F; break;
						case 0x0A040100: m_Reader = ReaderRGBA8_SNORM; break;
						case 0x0A050100: m_Reader = ReaderRGBA8_UNORM; break;
						default: throw new Exception( "Unrecognized format!" );
					}

					// Decode usage 
					switch ( m_usage ) {
						case 0:	// VU_POSTION
							m_Writer = WriterPosition;
							m_Info = "POSITION";
							break;
						case 1:	// VU_NORMAL,
							m_Writer = WriterNormal;
							m_Info = "NORMAL";
							break;
						case 2:	// VU_TANGENT,
							m_Writer = WriterTangent;
							m_Info = "TANGENT";
							break;
						case 3:	// VU_BITANGENT,
							m_Writer = WriterBiTangent;
							m_Info = "BITANGENT";
							break;
						case 4:	// VU_TEXCOORD,
							if ( m_usageIndex == 0 ) {
								m_Writer = WriterUV0;
								m_Info = "UV0";
							} else if ( m_usageIndex == 1 ) {
								m_Writer = WriterUV1;
								m_Info = "UV1";
							}
							else if ( m_usageIndex == 3 ) {
								m_Writer = WriterBarycentric;
								m_Info = "BARYCENTRIC";
							} else
								throw new Exception( "Unsupported UV stream index!" );
							break;
						case 5:	// VU_COLOR,
							m_Writer = WriterColor;
							m_Info = "COLOR";
							break;
// 						case 6:	// VU_SKINWEIGHTS,
// //							m_Writer = WriterPosition;
// 							break;
// 						case 7:	// VU_SKININDICES,
// //							m_Writer = WriterPosition;
// 							break;
						default:
							throw new Exception( "Unsupported usage!" );
					}
				}

				public void		ReadElement( BinaryReader _R, Vertex _Vertex ) {
					float4	Value = m_Reader( _R );
					m_Writer( _Vertex, Value );
				}

				#region Readers/Writers

				float4	ReaderRGBA8_UNORM( BinaryReader _R ) {
					byte	R = _R.ReadByte();
					byte	G = _R.ReadByte();
					byte	B = _R.ReadByte();
					byte	A = _R.ReadByte();
					float4	Value = new float4( R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f );
					return Value;
				}

				float4	ReaderRGBA8_SNORM( BinaryReader _R ) {
					sbyte	R = _R.ReadSByte();
					sbyte	G = _R.ReadSByte();
					sbyte	B = _R.ReadSByte();
					sbyte	A = _R.ReadSByte();
					float4	Value = new float4( R / 127.0f, G / 127.0f, B / 127.0f, A / 127.0f );
					return Value;
				}

				float4	ReaderRG16F( BinaryReader _R ) {
					half	R, G;
					R.raw = _R.ReadUInt16();
					G.raw = _R.ReadUInt16();
					float4	Value = new float4( (float) R, (float) G, 0, 0 );
					return Value;
				}

				float4	ReaderRGB32F( BinaryReader _R ) {
					float4	Value = new float4( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle(), 0 );
					return Value;
				}

				//////////////////////////////////////////////////////////////////////////

				void	WriterPosition( Vertex _Vertex, float4 _Value ) {
					_Vertex.Position = (float3) _Value;
				}

				void	WriterNormal( Vertex _Vertex, float4 _Value ) {
					_Vertex.Normal = (float3) _Value;
				}

				void	WriterTangent( Vertex _Vertex, float4 _Value ) {
					_Vertex.Tangent = (float3) _Value;
					_Vertex.BiTangent = _Value.w * _Vertex.Normal.Cross( _Vertex.Tangent );
				}

				void	WriterBiTangent( Vertex _Vertex, float4 _Value ) {
					_Vertex.BiTangent = (float3) _Value;
				}

				void	WriterUV0( Vertex _Vertex, float4 _Value ) {
					_Vertex.UVs[0] = (float2) _Value;
				}

				void	WriterUV1( Vertex _Vertex, float4 _Value ) {
					_Vertex.UVs[1] = (float2) _Value;
				}

				void	WriterColor( Vertex _Vertex, float4 _Value ) {
					_Vertex.Color = _Value;
				}

				void	WriterBarycentric( Vertex _Vertex, float4 _Value ) {
					_Vertex.Barycentric = (float3) _Value;
				}

				#endregion
			}

			public class Vertex {
				public float3	Position;
				public float3	Normal;
				public float3	Tangent;
				public float3	BiTangent;
				public float2[]	UVs = new float2[2];
				public float4	Color;
				public float3	Barycentric;
			};

			#endregion

			public Model			m_Owner;
			public VertexElement[]	m_VertexElements = null;
			public Vertex[]			m_Vertices = null;
			public ushort[]			m_Indices = null;
			public Material			m_Material = null;
			public float3			m_BoundsMin = float3.Zero;
			public float3			m_BoundsMax = float3.Zero;

			public Surface( Model _Owner, BinaryReader _R ) {
				m_Owner = _Owner;

				m_Material = new Material( m_Owner.ReadString( _R ) );
				m_Material.m_MaterialIndex = (int) m_Owner.ReadBig32( _R );

				// Prepare triangles
				uint	VerticesCount = _R.ReadUInt32();
				uint	IndicesCount = _R.ReadUInt32();
				m_Vertices = new Vertex[VerticesCount];
				m_Indices = new ushort[IndicesCount];

				// Read vertex declaration
				int	VertexElementsCount = (int) _R.ReadByte();
				m_VertexElements = new VertexElement[VertexElementsCount];
				for ( int elementIndex=0; elementIndex < VertexElementsCount; elementIndex++ ) {
					m_VertexElements[elementIndex].Read( _R );
				}

				// Read vertex scale & bias
				float3	XYZScale = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				float3	XYZBias = new float3( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				float2	UVScale = new float2( _R.ReadSingle(), _R.ReadSingle() );
				float2	UVBias = new float2( _R.ReadSingle(), _R.ReadSingle() );

				// Read vertices
				for ( int i=0; i < m_Vertices.Length; i++ ) {
					Vertex	V = new Vertex();
					m_Vertices[i] = V;
					foreach ( VertexElement E in m_VertexElements )
						E.ReadElement( _R, V );
				}

				// Read indices
				for ( int i=0; i < m_Indices.Length; i++ ) {
					m_Indices[i] = _R.ReadUInt16();
					if ( m_Indices[i] >= m_Vertices.Length ) throw new Exception( "Vertex index out of range!" );
				}

				// Read bounds & stuff
				m_BoundsMin.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_BoundsMax.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );

				int	detailOffset = _R.ReadInt32();

				// End
				uint	TrailingMagic = m_Owner.ReadBig32( _R );
				if ( (TrailingMagic & 0x00FFFFFFU) != 0x004C4D42 )
					throw new Exception( "Bad trailing magic!" );
			}
		}
		public Surface[]	m_Surfaces = null;
		public string		m_SourceFileName = null;

		public	Model( string _ModelFileName ) {

			using ( FileStream S = new FileInfo( _ModelFileName ).OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {

					uint	Magic = ReadBig32( R );
					if ( (Magic & 0x00FFFFFFU) != 0x004C4D42 )	// BML
						throw new Exception( "Unsupported BMODEL format!" );
					uint	Version = Magic >> 24;
					if ( Version != 32 )
						throw new Exception( "Unsupported BMODEL version!" );

					uint	TimeStamp = ReadBig32( R );

					m_SourceFileName = ReadString( R );

					int	SurfacesCount = (int) ReadBig32( R );
					m_Surfaces = new Surface[SurfacesCount];
					for ( int SurfaceIndex=0; SurfaceIndex < SurfacesCount; SurfaceIndex++ )
						m_Surfaces[SurfaceIndex] = new Surface( this, R );
				}
		}

		#region Binary Reading Helpers

		string	ReadString( BinaryReader _R ) {
			int	Length = (int) _R.ReadUInt32();
			StringBuilder	Result = new StringBuilder( Length );
			for ( int i=0; i < Length; i++ ) {
				byte	Cb = _R.ReadByte();
				char	Cc = (char) Cb;
				Result.Append( Cc );
			}
			return Result.ToString();
		}

		uint	ReadBig32( BinaryReader _R ) {
			uint	v = _R.ReadUInt32();
			v = Flip( v );
			return v;
		}
		uint	Flip( uint v ) {
			uint	r	= ((v & 0x000000FFU) << 24)
						| ((v & 0x0000FF00U) << 8)
						| ((v & 0x00FF0000U) >> 8)
						| ((v & 0xFF000000U) >> 24);
			return r;
		}

		#endregion
	}
}
