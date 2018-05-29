using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxFExtractor
{
	public partial class Form1 : Form
	{
		public Form1() {
			InitializeComponent();

			AxFService.AxFFile	testFile = new AxFService.AxFFile( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Sample Files\Material Samples\AxFSvbrdf_1_1_Dir\X-Rite_14-LTH_Red_GoatLeather_4405_2479.axf" ) );

			AxFService.AxFFile.Material[]	materials = new AxFService.AxFFile.Material[testFile.MaterialsCount];
			for ( int i=0; i < materials.Length; i++ )
				materials[i] = testFile[(uint)i];
		}
	}
}
