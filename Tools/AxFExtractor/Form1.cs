using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace AxFExtractor
{
	public partial class Form1 : Form
	{
		public Form1() {
			InitializeComponent();

			AxFService.AxFFile	testFile = new AxFService.AxFFile( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Sample Files\Material Samples\AxFSvbrdf_1_1_Dir\X-Rite_14-LTH_Red_GoatLeather_4405_2479.axf" ) );

			AxFService.AxFFile.Material[]	materials = new AxFService.AxFFile.Material[testFile.MaterialsCount];
			for ( int i=0; i < materials.Length; i++ ) {
				materials[i] = testFile[(uint)i];

				AxFService.AxFFile.Material.Texture[]	textures = materials[i].Textures;
				foreach ( AxFService.AxFFile.Material.Texture texture in textures ) {

// 					// Dump as DDS
// 					texture.Images.DDSSaveFile( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Shader\Assets\AxF Materials\X-Rite_14-LTH_Red_GoatLeather_4405_2479\" + texture.Name + ".dds" ), texture.ComponentFormat );

// Individual dump as RGBA8 files
// ImageUtility.ImageFile	source = texture.Images[0][0][0];
// ImageUtility.ImageFile	temp = new ImageUtility.ImageFile();
// //temp.ConvertFrom( source, ImageUtility.PIXEL_FORMAT.BGRA8 );
// temp.ToneMapFrom( source, ( float3 _HDR, ref float3 _LDR ) => { _LDR =_HDR; } );
// temp.Save( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Shader\Assets\AxF Materials\X-Rite_14-LTH_Red_GoatLeather_4405_2479\" + texture.Name + ".png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG );

// Individual dump as RGBA16 files
ImageUtility.ImageFile	source = texture.Images[0][0][0];

if ( texture.Name.ToLower() == "normal" ) {
	source.ReadWritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
		_color.x = 0.5f * (1.0f + _color.x);
		_color.y = 0.5f * (1.0f + _color.y);
		_color.z = 0.5f * (1.0f + _color.z);
	} );
}

ImageUtility.ImageFile	temp = new ImageUtility.ImageFile();
temp.ConvertFrom( source, ImageUtility.PIXEL_FORMAT.RGBA16 );
//temp.ToneMapFrom( source, ( float3 _HDR, ref float3 _LDR ) => { _LDR =_HDR; } );
temp.Save( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Shader\Assets\AxF Materials\X-Rite_14-LTH_Red_GoatLeather_4405_2479\" + texture.Name + ".png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG );
				}
			}
		}
	}
}
