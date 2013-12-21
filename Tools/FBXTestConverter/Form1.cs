using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using WMath;

namespace FBXTestConverter
{
	public partial class Form1 : Form
	{
		private enum	NODE_TYPE
		{
			GENERIC = 0,
			MESH,
			LIGHT,
			CAMERA,

			// Special cases
			PROBE,
		}

		private enum	LIGHT_TYPE
		{
			POINT = 0,
			DIRECTIONAL = 1,
			SPOT = 2,
		}

		private enum	VERTEX_FORMAT
		{
			P3N3G3B3T2 = 0,
		}

		private delegate ushort	MapMaterialDelegate( FBX.Scene.Materials.MaterialParameters.ParameterTexture2D _Texture );

		public Form1()
		{
			InitializeComponent();

//			LoadScene( new FileInfo( @"..\..\Resources\Scenes\GITest1.fbx" ) );
			LoadScene( new FileInfo( @"..\..\Resources\Scenes\GITest1_10Probes.fbx" ) );
//			LoadScene( new FileInfo( @"..\..\Resources\Scenes\CubeTest.fbx" ) );
		}

		public void	LoadScene( FileInfo _File )
		{
			FBX.SceneLoader.SceneLoader	Loader = new FBX.SceneLoader.SceneLoader();

			FBX.Scene.Scene	Scene = new FBX.Scene.Scene();
			Loader.Load( _File, Scene );

			// Start writing
			FileInfo	Target = new FileInfo( Path.Combine( Path.GetDirectoryName( _File.FullName ), Path.GetFileNameWithoutExtension( _File.FullName ) + ".gcx" ) );
			using ( FileStream S = Target.OpenWrite() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
					SaveGCX( W, Scene );

			// Write infos
			List<string>	Infos = new List<string>();
			Infos.Add( "Textures:" );
			foreach ( FBX.Scene.Materials.Texture2D Texture in Scene.Textures )
				Infos.Add( "ID #" + Texture.ID + " URL=" + Texture.URL );
			Infos.Add( "" );

			Infos.Add( "=============================" );
			Infos.Add( "Materials:" );
			foreach ( FBX.Scene.Materials.MaterialParameters Mat in Scene.MaterialParameters )
				Infos.Add( "ID #" + Mat.ID + " => " + Mat.Name + " (shader=" + Mat.ShaderURL + ")" );
			Infos.Add( "" );

			Infos.Add( "=============================" );
			Infos.Add( "Meshes:" );
			foreach ( FBX.Scene.Nodes.Mesh Mesh in Scene.Meshes )
				Infos.Add( "ID #" + Mesh.ID + " => " + Mesh.Name + " (primsCount=" + Mesh.PrimitivesCount + ")" );
			Infos.Add( "" );

			Infos.Add( "=============================" );
			Infos.Add( "Lights:" );
			foreach ( FBX.Scene.Nodes.Light Light in Scene.Lights )
				Infos.Add( "ID #" + Light.ID + " => " + Light.Name + " (type=" + Light.Type + ")" );
			Infos.Add( "" );

			Infos.Add( "=============================" );
			Infos.Add( "Cameras:" );
			foreach ( FBX.Scene.Nodes.Camera Camera in Scene.Cameras )
				Infos.Add( "ID #" + Camera.ID + " => " + Camera.Name + " (FOV=" + (Camera.FOV * 180.0f / Math.PI) + ")" );
			Infos.Add( "" );

			listBoxInfos.Items.AddRange( Infos.ToArray() );
		}

		private void	SaveGCX( BinaryWriter _W, FBX.Scene.Scene _Scene )
		{
			_W.Write( (UInt32) 0x31584347L );	// "GCX1"

			//////////////////////////////////////////////////////////////////////////
			// Write materials
			//
			FBX.Scene.Materials.MaterialParameters[]	Materials = _Scene.MaterialParameters;
			_W.Write( (ushort) Materials.Length );
			foreach ( FBX.Scene.Materials.MaterialParameters Material in Materials )
				SaveMaterial( _W, Material );

			//////////////////////////////////////////////////////////////////////////
			// Write nodes
			SaveNode( _W, _Scene.RootNode );
		}

		private void	SaveNode( BinaryWriter _W, FBX.Scene.Nodes.Node _Node )
		{
			// =============================
			// Write standard infos
			NODE_TYPE	NodeType = NODE_TYPE.GENERIC;
			if ( _Node is FBX.Scene.Nodes.Mesh )
				NodeType = NODE_TYPE.MESH;
			else if ( _Node is FBX.Scene.Nodes.Camera )
				NodeType = NODE_TYPE.CAMERA;
			else if ( _Node is FBX.Scene.Nodes.Light )
				NodeType = NODE_TYPE.LIGHT;
			else
			{
				// Isolate locators as probes
				if ( _Node.Name.ToLower().IndexOf( "locator" ) != -1 )
					NodeType = NODE_TYPE.PROBE;
			}
			_W.Write( (byte) NodeType );

				// Write Local2Parent matrix
			Matrix4x4	Local2Parent = _Node.Local2Parent;
			_W.Write( Local2Parent.m[0,0] );	_W.Write( Local2Parent.m[0,1] );	_W.Write( Local2Parent.m[0,2] );	_W.Write( Local2Parent.m[0,3] );
			_W.Write( Local2Parent.m[1,0] );	_W.Write( Local2Parent.m[1,1] );	_W.Write( Local2Parent.m[1,2] );	_W.Write( Local2Parent.m[1,3] );
			_W.Write( Local2Parent.m[2,0] );	_W.Write( Local2Parent.m[2,1] );	_W.Write( Local2Parent.m[2,2] );	_W.Write( Local2Parent.m[2,3] );
			_W.Write( Local2Parent.m[3,0] );	_W.Write( Local2Parent.m[3,1] );	_W.Write( Local2Parent.m[3,2] );	_W.Write( Local2Parent.m[3,3] );


			// =============================
			// Write specialized infos
			switch ( NodeType )
			{
				case NODE_TYPE.GENERIC:
					break;
				case NODE_TYPE.MESH:
					SaveMesh( _W, _Node as FBX.Scene.Nodes.Mesh );
					break;
				case NODE_TYPE.CAMERA:
					SaveCamera( _W, _Node as FBX.Scene.Nodes.Camera );
					break;
				case NODE_TYPE.LIGHT:
					SaveLight( _W, _Node as FBX.Scene.Nodes.Light );
					break;
			}


			// =============================
			// Write end marker
			_W.Write( (ushort) 0xABCD );


			// =============================
			// Recurse through children
			FBX.Scene.Nodes.Node[]	Children = _Node.Children;
			_W.Write( (ushort) Children.Length );

			foreach ( FBX.Scene.Nodes.Node Child in Children )
				SaveNode( _W, Child );
		}

		private void	SaveMaterial( BinaryWriter _W, FBX.Scene.Materials.MaterialParameters _Material )
		{
			// Write material ID
			_W.Write( (ushort) _Material.ID );

			// Write diffuse color + texture ID
			FBX.Scene.Materials.MaterialParameters.Parameter	P = null;
			P = _Material.Find( "DiffuseColor" );
			_W.Write( P.AsFloat3.Value.X );
			_W.Write( P.AsFloat3.Value.Y );
			_W.Write( P.AsFloat3.Value.Z );

			P = _Material.Find( "DiffuseTexture" );
			_W.Write( (ushort) MapMaterial( P != null ? P.AsTexture2D : null ) );

			// Write specular color + texture ID
			P = _Material.Find( "SpecularColor" );
			_W.Write( P != null ? P.AsFloat3.Value.X : 0.0f );
			_W.Write( P != null ? P.AsFloat3.Value.Y : 0.0f );
			_W.Write( P != null ? P.AsFloat3.Value.Z : 0.0f );

			P = _Material.Find( "SpecularTexture" );
			_W.Write( (ushort) MapMaterial( P != null ? P.AsTexture2D : null ) );

			// Write specular exponent
			P = _Material.Find( "SpecularExponent" );
			_W.Write( P != null ? P.AsFloat3.Value.X : 0.0f );
			_W.Write( P != null ? P.AsFloat3.Value.Y : 0.0f );
			_W.Write( P != null ? P.AsFloat3.Value.Z : 0.0f );

			// Write end marker
			_W.Write( (ushort) 0x1234 );
		}

		private void	SaveCamera( BinaryWriter _W, FBX.Scene.Nodes.Camera _Camera )
		{
			_W.Write( _Camera.FOV );
		}

		private void	SaveLight( BinaryWriter _W, FBX.Scene.Nodes.Light _Light )
		{
			_W.Write( (byte) _Light.Type );
			_W.Write( _Light.Color.X );
			_W.Write( _Light.Color.Y );
			_W.Write( _Light.Color.Z );
			_W.Write( _Light.Intensity );
			_W.Write( _Light.HotSpot );
			_W.Write( _Light.ConeAngle );
		}

		private void	SaveMesh( BinaryWriter _W, FBX.Scene.Nodes.Mesh _Mesh )
		{
			// Write primitives
			_W.Write( (ushort) _Mesh.PrimitivesCount );
			foreach ( FBX.Scene.Nodes.Mesh.Primitive Primitive in _Mesh.Primitives )
			{
				// Write material ID
				_W.Write( (ushort) Primitive.MaterialParms.ID );

				_W.Write( (UInt32) Primitive.FacesCount );
				UInt32	VerticesCount = (UInt32) Primitive.VerticesCount;
				_W.Write( VerticesCount );

				// Write faces
				if ( Primitive.VerticesCount <= 65536 )
				{
					foreach ( FBX.Scene.Nodes.Mesh.Primitive.Face Face in Primitive.Faces )
					{
						_W.Write( (UInt16) Face.V0 );
						_W.Write( (UInt16) Face.V1 );
						_W.Write( (UInt16) Face.V2 );
					}
				}
				else
				{
					foreach ( FBX.Scene.Nodes.Mesh.Primitive.Face Face in Primitive.Faces )
					{
						_W.Write( (UInt32) Face.V0 );
						_W.Write( (UInt32) Face.V1 );
						_W.Write( (UInt32) Face.V2 );
					}
				}

				// Retrieve & write streams
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
					Streams[UsageIndex] = Primitive.FindStreamsByUsage( Usages[UsageIndex] );
					if ( Streams[UsageIndex].Length == 0 )
						throw new Exception( "No stream for usage " + Usages[UsageIndex] + "! Can't complete target vertex format!" );
				}

				_W.Write( (byte) VERTEX_FORMAT.P3N3G3B3T2 );
				for ( int VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++ )
				{
					for ( int UsageIndex=0; UsageIndex < Usages.Length; UsageIndex++ )
					{
						FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE	Usage = Usages[UsageIndex];
						switch ( Usage )
						{
							case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.POSITION:
							case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.NORMAL:
							case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TANGENT:
							case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.BITANGENT:
								{
									Vector[]	Stream = Streams[UsageIndex][0].Content as Vector[];
									Vector		Vertex = Stream[VertexIndex];
									_W.Write( Vertex.X );
									_W.Write( Vertex.Y );
									_W.Write( Vertex.Z );
								}
								break;

							case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TEXCOORDS:
								{
									Vector2D[]	Stream = Streams[UsageIndex][0].Content as Vector2D[];
									Vector2D	Vertex = Stream[VertexIndex];
									_W.Write( Vertex.X );
									_W.Write( Vertex.Y );
								}
								break;
						}
					}
				}
			}
		}

		private ushort	MapMaterial( FBX.Scene.Materials.MaterialParameters.ParameterTexture2D _Texture )
		{
			if ( _Texture == null )
				return (ushort) 0xFFFF;

// 			if ( _Texture.Value.URL.IndexOf( "pata_diff_colo.tga" ) != -1 )
// 				return 0;

			return (ushort) _Texture.Value.ID;
		}
	}
}
