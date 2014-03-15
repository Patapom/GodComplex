//////////////////////////////////////////////////////////////////////////
// This project converts FBX scenes to simple GCX (GodComplex format) scenes
//
// The GCX format only supports a minimal set of objects and properties but it's
//	easy to customize it and augment it with custom data, shader parameters, etc.
//
// The GCX scene can then be loaded using the Scene class in the GodComplex project.
//
//////////////////////////////////////////////////////////////////////////
//
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
		public Form1()
		{
			InitializeComponent();

			LoadScene( new FileInfo( @"..\..\..\Arkane\City.fbx" ) );

//			LoadScene( new FileInfo( @"..\..\Resources\Scenes\GITest1.fbx" ) );
//			LoadScene( new FileInfo( @"..\..\Resources\Scenes\GITest1_10Probes.fbx" ) );
//			LoadScene( new FileInfo( @"..\..\Resources\Scenes\CubeTest.fbx" ) );
		}

		/// <summary>
		/// Loads a FBX scene and converts it (in the same folder) into its GCX equivalent
		/// </summary>
		/// <param name="_File"></param>
		public void	LoadScene( FileInfo _File )
		{
			FBX.SceneLoader.SceneLoader	Loader = new FBX.SceneLoader.SceneLoader();

			FBX.SceneLoader.MaterialsDatabase	Materials = new FBX.SceneLoader.MaterialsDatabase();
			Materials.BuildFromM2( new DirectoryInfo( @"D:\Workspaces\Arkane\m2" ) );

			FBX.Scene.Scene	Scene = new FBX.Scene.Scene();
			Loader.Load( _File, Scene, 1.0f, Materials );

			// Start writing
			FileInfo	Target = new FileInfo( Path.Combine( Path.GetDirectoryName( _File.FullName ), Path.GetFileNameWithoutExtension( _File.FullName ) + ".gcx" ) );
			using ( FileStream S = Target.OpenWrite() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
				{
					GCXFormat.Scene	GCX = new GCXFormat.Scene( Scene );
					GCX.Save( W );
				}

			// Write infos
			List<string>	Infos = new List<string>();
			Infos.Add( "Textures:" );
			foreach ( FBX.Scene.Materials.Texture2D Texture in Scene.Textures )
				Infos.Add( "ID #" + Texture.ID.ToString( "D3" ) + " URL=" + Texture.URL );
			Infos.Add( "" );

			// Here, I write an array mapping texture IDs to names, assuming textures have been converted to POM format using PNG2POM (or HDR2POM)
			// This way I can simply copy this "code friendly" list and paste it in my C++ code (e.g. const char* ppID2TextureName[] = { PasteHere }; )
			Infos.Add( "Texture Flat Names:" );
			Infos.Add( "" );
			foreach ( FBX.Scene.Materials.Texture2D Texture in Scene.Textures )
//				Infos.Add( "ID #" + Texture.ID.ToString( "D3" ) + " URL=" + Path.GetFileNameWithoutExtension( Texture.URL ) );
				Infos.Add( "\"" + @"..\\Arkane\\TexturesPOM\\" + Path.GetFileNameWithoutExtension( Texture.URL ) + ".pom\"," );
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

			if ( Materials != null )
			{
				FBX.SceneLoader.MaterialsDatabase.Material[]	QueriedMaterials = Materials.QueriedMaterials;

				List<string>	QueriedTextures = new List<string>();

				// Here I'm generating the XCOPY batch commands to copy original textures from a complex directory structure
				//	into a flattened directory where they'll further be converted into POM files using PNG2POM
				Infos.Add( "=============================" );
				Infos.Add( "Queried database materials:" );
				foreach ( FBX.SceneLoader.MaterialsDatabase.Material M in QueriedMaterials )
				{
					Infos.Add( M.Name );

					if ( M.TextureDiffuse != null )
						QueriedTextures.Add( "xcopy \"" + M.TextureDiffuse.Replace( '/', '\\' ) + "\" \"..\\POMTextures\\\" /Y/U/S" );
					if ( M.TextureNormal != null )
						QueriedTextures.Add( "xcopy \"" + M.TextureNormal.Replace( '/', '\\' ) + "\" \"..\\POMTextures\\\" /Y/U/S" );
					if ( M.TextureSpecular != null )
						QueriedTextures.Add( "xcopy \"" + M.TextureSpecular.Replace( '/', '\\' ) + "\" \"..\\POMTextures\\\" /Y/U/S" );
				}
				Infos.Add( "" );

				Infos.Add( "=============================" );
				Infos.Add( "Queried textures:" );
				Infos.AddRange( QueriedTextures );

			}

			textBoxReport.Lines = Infos.ToArray();
		}
	}
}
