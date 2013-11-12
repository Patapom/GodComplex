using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;

namespace FBX.SceneLoader.Objects
{
	[System.Diagnostics.DebuggerDisplay( "Name={Name}" )]
	public class	LoaderTempSceneObject
	{
		#region FIELDS

		protected SceneLoader				m_Owner = null;
		protected string					m_Name = null;

		protected LoaderTempSceneObject		m_Parent = null;

		protected Dictionary<string,string>	m_Properties = new Dictionary<string,string>();
		protected Dictionary<string,string>	m_Params = new Dictionary<string,string>();
		protected Dictionary<string,string>	m_Custom = new Dictionary<string,string>();

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets the object's name
		/// </summary>
		public string						Name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Gets the owner serializer
		/// </summary>
		public SceneLoader					Owner
		{
			get { return m_Owner; }
		}

		/// <summary>
		/// Gets the object's parent
		/// </summary>
		public LoaderTempSceneObject		Parent
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

		public LoaderTempSceneObject( SceneLoader _Owner, string _Name )
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
		public void		SetParent( LoaderTempSceneObject _Parent )
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

/*
	/// <summary>
	/// This class is built from one of our Mesh.Primitives and is able to create a Cirrus primitive from it
	/// </summary>
	public class	PrimitiveFeeder : IVertexFieldProvider, IIndexProvider
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
*/
}
