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
			ConvertMap( @"..\Arkane\SimpleMapWithManyProbes\test_probes_p - Fixed.map", @"..\Arkane\SimpleMapWithManyProbes\scene.gcx" );
		}

		private void	ConvertMap( string _SourceFileName, string _TargetFileName ) {

			idTech5Map.Map	Map = new idTech5Map.Map( _SourceFileName );

			// Convert
			GCXFormat.Scene	Scene = new GCXFormat.Scene( Map );

			// Save
			using ( FileStream S = new FileInfo( _TargetFileName ).Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
					Scene.Save( W );
		}
	}
}
