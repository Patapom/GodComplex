using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

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


		public Form1()
		{
			InitializeComponent();

			LoadScene( new FileInfo( @"kiosk.fbx" ) );
		}

		public void	LoadScene( FileInfo _File )
		{
			FBX.SceneLoader.SceneLoader	Loader = new FBX.SceneLoader.SceneLoader();

			FBX.Scene.Scene	Scene = new FBX.Scene.Scene();
			Loader.Load( _File, Scene );

			// Start writing
			FileInfo	Target = new FileInfo( Path.GetFileNameWithoutExtension( _File.FullName ) + ".gcx" );
			using ( FileStream S = Target.OpenWrite() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
					SaveGCX( W, Scene );
		}

		private void	SaveGCX( BinaryWriter _W, FBX.Scene.Scene _Scene )
		{
			_W.Write( (UInt32) 0x47435831L );	// "GCX1"

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
			_W.Write( (byte) NodeType );

				// Write Local2Parent matrix
			SharpDX.Matrix	Local2Parent = _Node.Local2Parent;
			_W.Write( Local2Parent.M11 );	_W.Write( Local2Parent.M12 );	_W.Write( Local2Parent.M13 );	_W.Write( Local2Parent.M14 );
			_W.Write( Local2Parent.M21 );	_W.Write( Local2Parent.M22 );	_W.Write( Local2Parent.M23 );	_W.Write( Local2Parent.M24 );
			_W.Write( Local2Parent.M31 );	_W.Write( Local2Parent.M32 );	_W.Write( Local2Parent.M33 );	_W.Write( Local2Parent.M34 );
			_W.Write( Local2Parent.M41 );	_W.Write( Local2Parent.M42 );	_W.Write( Local2Parent.M43 );	_W.Write( Local2Parent.M44 );


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
			// Write stuff
			_W.Write( (ushort) _Material.ID );

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
				_W.Write( (ushort) Primitive.Parameters.ID );

				// Write faces
				_W.Write( (UInt32) Primitive.FacesCount );
				foreach ( FBX.Scene.Nodes.Mesh.Primitive.Face Face in Primitive.Faces )
				{
					_W.Write( (UInt32) Face.V0 );
					_W.Write( (UInt32) Face.V1 );
					_W.Write( (UInt32) Face.V2 );
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
				UInt32	VerticesCount = (UInt32) Primitive.VerticesCount;
				_W.Write( VerticesCount );
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
									SharpDX.Vector3[]	Stream = Streams[UsageIndex][0].Content as SharpDX.Vector3[];
									SharpDX.Vector3		Vertex = Stream[VertexIndex];
									_W.Write( Vertex.X );
									_W.Write( Vertex.Y );
									_W.Write( Vertex.Z );
								}
								break;

							case FBX.Scene.Nodes.Mesh.Primitive.VertexStream.USAGE.TEXCOORDS:
								{
									SharpDX.Vector2[]	Stream = Streams[UsageIndex][0].Content as SharpDX.Vector2[];
									SharpDX.Vector2		Vertex = Stream[VertexIndex];
									_W.Write( Vertex.X );
									_W.Write( Vertex.Y );
								}
								break;
						}
					}
				}
			}
		}
	}
}
