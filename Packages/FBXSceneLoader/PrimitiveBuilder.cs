using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SharpDX;
using FBX.SceneLoader.Objects;
// using FBX.Scene.Nodes;
// using FBX.Scene.Materials;

namespace FBX.SceneLoader
{
	/// <summary>
	/// This class is built from one of our Mesh.Primitives and is able to create a Cirrus primitive from it
	/// </summary>
	public class	PrimitiveBuilder
	{
		#region FIELDS

		protected LoaderTempMesh.Primitive	m_SourcePrimitive = null;
// 		protected Cirrus.DynamicVertexSignature	m_VertexSignature = new Cirrus.DynamicVertexSignature();
// 		protected Dictionary<int,Mesh.Primitive.VertexStream>	m_FieldIndex2Stream = new Dictionary<int,Mesh.Primitive.VertexStream>();

		#endregion

		#region PROPERTIES

//		public Cirrus.IVertexSignature		VertexSignature	{ get { return m_VertexSignature; } }

		#endregion

		#region METHODS

		public PrimitiveBuilder()
		{
		}

		public Scene.Nodes.Mesh.Primitive	CreatePrimitive( LoaderTempMesh.Primitive _SourcePrimitive, Scene.Nodes.Mesh _ParentMesh, string _Name, Scene.Materials.MaterialParameters _MatParams )
		{
			m_SourcePrimitive = _SourcePrimitive;

			Scene.Nodes.Mesh.Primitive	Target = _ParentMesh.AddPrimitive( _Name, _MatParams, _SourcePrimitive.VerticesCount, _SourcePrimitive.FacesCount );

			// Build the primitive's triangles
			for ( int FaceIndex=0; FaceIndex < _SourcePrimitive.FacesCount; FaceIndex++ )
			{
				Target.Faces[FaceIndex].V0 = _SourcePrimitive.Faces[FaceIndex].V0.m_Index;
				Target.Faces[FaceIndex].V1 = _SourcePrimitive.Faces[FaceIndex].V1.m_Index;
				Target.Faces[FaceIndex].V2 = _SourcePrimitive.Faces[FaceIndex].V2.m_Index;
			}

			// Build the primitive's vertex streams
			int[]	StreamIndices = new int[8];
			int		UVStreamIndex = 0;
			foreach ( LoaderTempMesh.Primitive.VertexStream Stream in m_SourcePrimitive.VertexStreams )
			{
				Scene.Nodes.Mesh.Primitive.VertexStream.USAGE		Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.UNKNOWN;
				Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE	FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.UNKNOWN;
				object												Content = null;

				switch ( Stream.StreamType )
				{
					case LoaderTempMesh.VERTEX_INFO_TYPE.POSITION:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT3;
							Vector3[]	T = new Vector3[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertPoint( Stream.Stream[i] as WMath.Point );
							}
						}
 						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.NORMAL:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.NORMAL;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT3;
							Vector3[]	T = new Vector3[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertVector( Stream.Stream[i] as WMath.Vector );
							}
						}
						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.TANGENT:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TANGENT;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT3;
							Vector3[]	T = new Vector3[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertVector( Stream.Stream[i] as WMath.Vector );
							}
						}
						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.BINORMAL:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.BITANGENT;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT3;
							Vector3[]	T = new Vector3[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertVector( Stream.Stream[i] as WMath.Vector );
							}
						}
						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.TEXCOORD1D:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT;
							float[]	T = new float[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = (float) Stream.Stream[i];
							}
						}
						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.TEXCOORD2D:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT2;
							Vector2[]	T = new Vector2[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertVector( Stream.Stream[i] as WMath.Vector2D );
								T[i].Y = 1.0f - T[i].Y;	// Here we must complement the V coordinate as MAX has the bad habit of inverting the Y axis of images!
							}
						}
						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.TEXCOORD3D:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT3;
							Vector3[]	T = new Vector3[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertVector( Stream.Stream[i] as WMath.Vector );
								T[i].Y = 1.0f - T[i].Y;	// Here we must complement the V coordinate as MAX has the bad habit of inverting the Y axis of images!
							}
						}
						break;
					case LoaderTempMesh.VERTEX_INFO_TYPE.COLOR_HDR:
						{
							Usage = Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION;
							FieldType = Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.FLOAT4;
							Vector4[]	T = new Vector4[_SourcePrimitive.VerticesCount];
							Content = T;
							for ( int i=0; i < _SourcePrimitive.VerticesCount; i++ )
							{
								T[i] = SceneLoader.ConvertVector( Stream.Stream[i] as WMath.Vector4D );
							}
						}
 						break;
				}

				if (   Usage == Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.UNKNOWN 
					|| FieldType == Scene.Nodes.Mesh.Primitive.VertexStream.FIELD_TYPE.UNKNOWN )
					continue;	// Unsupported... Should we throw?

				if ( Content == null )
					throw new Exception( "Invalid content!" );

				int	StreamIndexIndex = (int) Usage - 1;	// Skip UNKNOWN usage and use the value as "stream type"
				int	StreamIndex = StreamIndices[StreamIndexIndex];
				StreamIndices[StreamIndexIndex]++;		// Increase stream index

				Target.AddVertexStream( Usage, FieldType, StreamIndex, Content );

				StreamIndex++;
			}

			return Target;
		}

/*
		#region IVertexFieldProvider Members

		public object	GetField( int _VertexIndex, int _FieldIndex )
		{
			if ( !m_FieldIndex2Stream.ContainsKey( _FieldIndex ) )
				throw new Exception( "Requesting unsupported field!" );

			Mesh.Primitive.VertexStream	Stream = m_FieldIndex2Stream[_FieldIndex];

			switch ( Stream.StreamType )
			{
				case LoaderTempMesh.VERTEX_INFO_TYPE.POSITION:
					return ConvertPoint( Stream.Stream[_VertexIndex] as Point );

				case LoaderTempMesh.VERTEX_INFO_TYPE.NORMAL:
				case LoaderTempMesh.VERTEX_INFO_TYPE.TANGENT:
				case LoaderTempMesh.VERTEX_INFO_TYPE.BINORMAL:
					return ConvertVector( Stream.Stream[_VertexIndex] as Vector );

				case LoaderTempMesh.VERTEX_INFO_TYPE.TEXCOORD1D:
					return (float) Stream.Stream[_VertexIndex];
				case LoaderTempMesh.VERTEX_INFO_TYPE.TEXCOORD2D:
					{	// Here we must complement the V coordinate as MAX has the bad habit of inverting the Y axis of images !
						Vector2D	Source = Stream.Stream[_VertexIndex] as Vector2D;
						return ConvertVector( new Vector2D( Source.x, 1.0f - Source.y ) );
					}
				case LoaderTempMesh.VERTEX_INFO_TYPE.TEXCOORD3D:
					return ConvertVector( Stream.Stream[_VertexIndex] as Vector );

				case LoaderTempMesh.VERTEX_INFO_TYPE.COLOR_HDR:
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
*/

		#endregion
	}
}
