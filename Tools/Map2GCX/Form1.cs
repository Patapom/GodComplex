//////////////////////////////////////////////////////////////////////////
// Converts idTech5 map, bmodel and m2 formats into a readable GCX scene
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace Map2GCX
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			ConvertMap( @"..\Arkane\GIScenes\SimpleMapWithManyProbes\test_probes_p - Fixed.map", @"..\Arkane\GIScenes\SimpleMapWithManyProbes\scene.gcx" );
		}

		private void	ConvertMap( string _SourceFileName, string _TargetFileName ) {

			idTech5Map.Map	Map = new idTech5Map.Map( _SourceFileName );

			// Convert
			GCXFormat.Scene	Scene = new GCXFormat.Scene( Map );

			// Save
			using ( FileStream S = new FileInfo( _TargetFileName ).Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
					Scene.Save( W );

			// Update log
			int	MeshesCount = 0;
			int	LightsCount = 0;
			int	CamerasCount = 0;
			int	ProbesCount = 0;
			int	GenericsCount = 0;
			foreach ( GCXFormat.Scene.Node Node in Scene.m_Nodes )
			{
				switch ( Node.m_Type ) {
					case GCXFormat.Scene.Node.TYPE.MESH: MeshesCount++; break;
					case GCXFormat.Scene.Node.TYPE.LIGHT: LightsCount++; break;
					case GCXFormat.Scene.Node.TYPE.CAMERA: CamerasCount++; break;
					case GCXFormat.Scene.Node.TYPE.PROBE: ProbesCount++; break;
					case GCXFormat.Scene.Node.TYPE.GENERIC: GenericsCount++; break;
				}
			}

			string		TextTextures = "";
			int			TexturesCount = 0;
// 			foreach ( GCXFormat.Scene.Material Material in Scene.m_Materials ) {
// 				if ( Material.m_DiffuseTextureID )
// 			}

			string	Text = "Nodes count: " + Scene.m_Nodes.Count + "\r\n"
						+ "	> Meshes count: " + MeshesCount + "\r\n"
						+ "	> Lights count: " + LightsCount + "\r\n"
						+ "	> Cameras count: " + CamerasCount + "\r\n"
						+ "	> Probes count: " + ProbesCount + "\r\n"
						+ "	> Generics count: " + GenericsCount + "\r\n"
						+ "\r\n"
						+ "Total vertices: " + Scene.m_TotalVerticesCount + " - Total faces: " + Scene.m_TotalFacesCount + "\r\n"
						+ "\r\n"
						+ "Materials count: " + Scene.m_Materials.Count + "\r\n"
						+ "Textures count: " + TexturesCount + "\r\n"
						+ TextTextures;
			textBoxLog.Text = Text;
		}
	}
}
