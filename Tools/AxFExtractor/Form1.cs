using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

using SharpMath;

namespace AxFExtractor
{
	public partial class AxFDumpForm : Form
	{
		private RegistryKey		m_AppKey;
		private string			m_applicationPath;

		AxFService.AxFFile		m_file = null;

		AxFService.AxFFile.Material	m_selectedMaterial = null;
		AxFService.AxFFile.Material	SelectedMaterial {
			get { return m_selectedMaterial; }
			set {
				if ( value == m_selectedMaterial )
					return;	// No change

				m_selectedMaterial = value;
				if ( m_selectedMaterial == null ) {
					buttonDumpMaterial.Enabled = false;
					textBoxMatInfo.Text = "";
					return;
				}
				
				textBoxMatInfo.Text = null;
				textBoxMatInfo.Text += "Type: " + m_selectedMaterial.Type + "\r\n";
				textBoxMatInfo.Text += "• Diffuse Type: " + m_selectedMaterial.DiffuseType + "\r\n";
				textBoxMatInfo.Text += "• Specular Type: " + m_selectedMaterial.SpecularType + "\r\n";
				textBoxMatInfo.Text += "• Fresnel Variant: " + m_selectedMaterial.FresnelVariant + "\r\n";
				textBoxMatInfo.Text += "• Specular Variant: " + m_selectedMaterial.SpecularVariant + "\r\n";
				textBoxMatInfo.Text += "• Anisotropic: " + m_selectedMaterial.IsAnisotropic + "\r\n";

				textBoxMatInfo.Text += "\r\n";
				AxFService.AxFFile.Material.Texture[]	textures = m_selectedMaterial.Textures;
				textBoxMatInfo.Text += "► Textures Count: " + textures.Length + "\r\n";
				foreach ( AxFService.AxFFile.Material.Texture texture in textures ) {
					textBoxMatInfo.Text += "	Texture: " + texture.Name + "\r\n";
				}

				textBoxMatInfo.Text += "\r\n";
				AxFService.AxFFile.Material.Property[]	properties = m_selectedMaterial.Properties;
				textBoxMatInfo.Text += "► Properties Count: " + properties.Length + "\r\n";
				foreach ( AxFService.AxFFile.Material.Property prop in properties ) {
					textBoxMatInfo.Text += "	" + prop.m_name + " = " + prop.m_value + "\r\n";
				}

				buttonDumpMaterial.Enabled = true;
			}
		}

		public AxFDumpForm() {
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\AxFExtractor" );
			m_applicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

// 			System.IO.FileInfo	sourceMaterialFileName = new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF Sample Files\Material Samples\AxFSvbrdf_1_1_Dir\X-Rite_14-LTH_Red_GoatLeather_4405_2479.axf" );
// 			string				targetMaterialDirectory = @"D:\Workspaces\Unity Labs\AxF Unity Project\Assets\AxF Materials\";
// 
// 			AxFService.AxFFile	testFile = new AxFService.AxFFile( sourceMaterialFileName );
// 
// 			AxFService.AxFFile.Material[]	materials = new AxFService.AxFFile.Material[testFile.MaterialsCount];
// 			for ( int i=0; i < materials.Length; i++ ) {
// 				materials[i] = testFile[(uint)i];
// 			}
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
		}
		private DialogResult	MessageBox( string _Text, Exception _e )
		{
			return MessageBox( _Text + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			return MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			return MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
// 			if ( m_silentMode )
// 				throw new Exception( _Text );

			return System.Windows.Forms.MessageBox.Show( this, _Text, "AxF Extractor", _Buttons, _Icon );
		}

		#endregion

		void	DumpMaterial( AxFService.AxFFile.Material _material, System.IO.DirectoryInfo _targetDirectory ) {

			System.IO.DirectoryInfo	fullTargetDirectory = new System.IO.DirectoryInfo( System.IO.Path.Combine( _targetDirectory.FullName, _material.Name ) );
			if ( !fullTargetDirectory.Exists ) {
				fullTargetDirectory.Create();
			}

			AxFService.AxFFile.Material.Texture[]	textures = _material.Textures;
			foreach ( AxFService.AxFFile.Material.Texture texture in textures ) {

//				// Dump as DDS
//				texture.Images.DDSSaveFile( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Shader\Assets\AxF Materials\X-Rite_14-LTH_Red_GoatLeather_4405_2479\" + texture.Name + ".dds" ), texture.ComponentFormat );

				// Individual dump as RGBA8 files
// 				ImageUtility.ImageFile	source = texture.Images[0][0][0];
// 				ImageUtility.ImageFile	temp = new ImageUtility.ImageFile();
// 				//temp.ConvertFrom( source, ImageUtility.PIXEL_FORMAT.BGRA8 );
// 				temp.ToneMapFrom( source, ( float3 _HDR, ref float3 _LDR ) => { _LDR =_HDR; } );
// 				temp.Save( new System.IO.FileInfo( @"D:\Workspaces\Unity Labs\AxF\AxF Shader\Assets\AxF Materials\X-Rite_14-LTH_Red_GoatLeather_4405_2479\" + texture.Name + ".png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG );

				// Individual dump as RGBA16 files
				ImageUtility.ImageFile	source = texture.Images[0][0][0];

				if ( texture.Name.ToLower() == "diffusecolor" || texture.Name.ToLower() == "specularcolor" || texture.Name.ToLower() == "clearcoatcolor" ) {
					source.ReadWritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						_color.x = Mathf.Pow( _color.x, 1.0f / 2.2f );
						_color.y = Mathf.Pow( _color.y, 1.0f / 2.2f );
						_color.z = Mathf.Pow( _color.z, 1.0f / 2.2f );
					} );
				}


				if ( texture.Name.ToLower() == "normal" || texture.Name.ToLower() == "clearcoatnormal" ) {
					source.ReadWritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						_color.x = 0.5f * (1.0f + _color.x);
						_color.y = 0.5f * (1.0f + _color.y);
						_color.z = 0.5f * (1.0f + _color.z);
					} );
				}

				ImageUtility.ImageFile	temp = new ImageUtility.ImageFile();
				temp.ConvertFrom( source, ImageUtility.PIXEL_FORMAT.RGBA16 );
				//temp.ToneMapFrom( source, ( float3 _HDR, ref float3 _LDR ) => { _LDR =_HDR; } );

				System.IO.FileInfo	targetTextureFileName = new System.IO.FileInfo( System.IO.Path.Combine( fullTargetDirectory.FullName, texture.Name + ".png" ) );
				temp.Save( targetTextureFileName, ImageUtility.ImageFile.FILE_FORMAT.PNG );
			}
		}

		private void buttonLoad_Click( object sender, EventArgs e ) {

			string	oldFileName = GetRegKey( "AxFFileName", System.IO.Path.Combine( m_applicationPath, "Example.axf" ) );
			openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName( oldFileName );
			openFileDialog1.FileName = System.IO.Path.GetFileName( oldFileName );
			if ( openFileDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "AxFFileName", openFileDialog1.FileName );

			//////////////////////////////////////////////////////////////////////////
			buttonDumpAllMaterials.Enabled = false;
			buttonDumpMaterial.Enabled = false;
			SelectedMaterial = null;

			try {
				m_file = new AxFService.AxFFile( new System.IO.FileInfo( openFileDialog1.FileName ) );

				// Populate list
				listBoxMaterials.BeginUpdate();
				listBoxMaterials.Items.Clear();
				labelMaterialsCount.Text = m_file.MaterialsCount + " Materials";

				foreach ( AxFService.AxFFile.Material material in m_file.Materials ) {
					listBoxMaterials.Items.Add( material );
				}
				if ( listBoxMaterials.Items.Count > 0 )
					listBoxMaterials.SelectedItem = listBoxMaterials.Items[0];
				listBoxMaterials.EndUpdate();

				buttonDumpAllMaterials.Enabled = true;

			} catch ( Exception _e ) {
				m_file = null;
				MessageBox( "An error occurred while opening AxF file:", _e );
			}
		}

		private void buttonDumpMaterial_Click( object sender, EventArgs e ) {

			string	oldPath = GetRegKey( "AxFDumpFolder", System.IO.Path.GetDirectoryName( openFileDialog1.FileName )  );
			folderBrowserDialog1.SelectedPath = oldPath;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "AxFDumpFolder", folderBrowserDialog1.SelectedPath );

			DumpMaterial( SelectedMaterial, new System.IO.DirectoryInfo( folderBrowserDialog1.SelectedPath ) );
		}

		private void buttonDumpAllMaterials_Click( object sender, EventArgs e ) {

			string	oldPath = GetRegKey( "AxFDumpFolder", System.IO.Path.GetDirectoryName( openFileDialog1.FileName ) );
			folderBrowserDialog1.SelectedPath = oldPath;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "AxFDumpFolder", folderBrowserDialog1.SelectedPath );

			System.IO.DirectoryInfo	targetFolder = new System.IO.DirectoryInfo( folderBrowserDialog1.SelectedPath );
			foreach ( AxFService.AxFFile.Material material in m_file.Materials ) {
				DumpMaterial( material, targetFolder );
			}
		}

		private void listBox1_SelectedIndexChanged( object sender, EventArgs e ) {
			SelectedMaterial = listBoxMaterials.SelectedItem as AxFService.AxFFile.Material;
		}
	}
}
