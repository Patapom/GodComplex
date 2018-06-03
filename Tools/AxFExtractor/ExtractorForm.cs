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
				buttonDumpMaterial.Focus();
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

		enum TEXTURE_TYPE {
			UNKNOWN = -1,
			FLAG_sRGB = 0x10000,
			FLAG_NORMAL = 0x20000,
			FLAG_IOR = 0x40000,

			// SVBRDF
			DIFFUSE_COLOR		= 0 | FLAG_sRGB,
			SPECULAR_COLOR		= 1 | FLAG_sRGB,
			NORMAL				= 2 | FLAG_NORMAL,
			FRESNEL				= 3 | FLAG_sRGB,
			SPECULAR_LOBE		= 4,
			ANISOTROPY_ANGLE	= 5,
			HEIGHT				= 6,
			OPACITY				= 7 | FLAG_sRGB,
			CLEARCOAT_COLOR		= 8 | FLAG_sRGB,
			CLEARCOAT_NORMAL	= 9 | FLAG_NORMAL,
			CLEARCOAT_IOR		= 10 | FLAG_IOR,
		}

		void	DumpMaterial( AxFService.AxFFile.Material _material, System.IO.DirectoryInfo _targetDirectory ) {

			System.IO.DirectoryInfo	fullTargetDirectory = new System.IO.DirectoryInfo( System.IO.Path.Combine( _targetDirectory.FullName, _material.Name ) );
			if ( !fullTargetDirectory.Exists ) {
				fullTargetDirectory.Create();
			}

			AxFService.AxFFile.Material.Texture[]	textures = _material.Textures;
			TEXTURE_TYPE[]							textureTypes = new TEXTURE_TYPE[textures.Length];
			string[]								GUIDs = new string[textures.Length];
			bool									allGUIDsValid = true;

			for ( int textureIndex=0; textureIndex < textures.Length; textureIndex++ ) {
				AxFService.AxFFile.Material.Texture texture = textures[textureIndex];

				TEXTURE_TYPE	textureType = TEXTURE_TYPE.UNKNOWN;
				switch ( texture.Name.ToLower() ) {
					case "diffusecolor":	textureType = TEXTURE_TYPE.DIFFUSE_COLOR; break;
					case "specularcolor":	textureType = TEXTURE_TYPE.SPECULAR_COLOR; break;
					case "normal":			textureType = TEXTURE_TYPE.NORMAL; break;
					case "fresnel":			textureType = TEXTURE_TYPE.FRESNEL; break;
					case "specularlobe":	textureType = TEXTURE_TYPE.SPECULAR_LOBE; break;
					case "anisorotation":	textureType = TEXTURE_TYPE.ANISOTROPY_ANGLE; break;
					case "height":			textureType = TEXTURE_TYPE.HEIGHT; break;
					case "opacity":			textureType = TEXTURE_TYPE.OPACITY; break;
					case "clearcoatcolor":	textureType = TEXTURE_TYPE.CLEARCOAT_COLOR; break;
					case "clearcoatnormal":	textureType = TEXTURE_TYPE.CLEARCOAT_NORMAL; break;
					case "clearcoatior":	textureType = TEXTURE_TYPE.CLEARCOAT_IOR; break;

					default:
						throw new Exception( "Unsupported texture type \"" + texture.Name + "\"!" );
				}

				textureTypes[textureIndex] = textureType;

				bool	sRGB = ((int) textureType & (int) TEXTURE_TYPE.FLAG_sRGB) != 0;
				bool	isNormalMap = ((int) textureType & (int) TEXTURE_TYPE.FLAG_NORMAL) != 0;
				bool	isIOR = ((int) textureType & (int) TEXTURE_TYPE.FLAG_IOR) != 0;

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

				if ( sRGB ) {
					source.ReadWritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						_color.x = Mathf.Pow( _color.x, 1.0f / 2.2f );
						_color.y = Mathf.Pow( _color.y, 1.0f / 2.2f );
						_color.z = Mathf.Pow( _color.z, 1.0f / 2.2f );
//						_color.w = 1.0f;
					} );
					sRGB = true;
				}

				if ( isNormalMap ) {
					source.ReadWritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						_color.x = 0.5f * (1.0f + _color.x);
						_color.y = 0.5f * (1.0f + _color.y);
						_color.z = 0.5f * (1.0f + _color.z);
					} );
				}

				if ( isIOR ) {
					// Transform into F0
					source.ReadWritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						if ( float.IsNaN( _color.x ) )
							_color.x = 1.2f;
						if ( float.IsNaN( _color.y ) )
							_color.y = 1.2f;
						if ( float.IsNaN( _color.z ) )
							_color.z = 1.2f;

						_color.x = (_color.x - 1.0f) / (_color.x + 1.0f);	// We apply the square below, during the sRGB conversion
						_color.y = (_color.y - 1.0f) / (_color.y + 1.0f);
						_color.z = (_color.z - 1.0f) / (_color.z + 1.0f);
						_color.x = Mathf.Pow( _color.x, 2.0f / 2.2f );		// <= Notice the 2/2.2 here!
						_color.y = Mathf.Pow( _color.y, 2.0f / 2.2f );
						_color.z = Mathf.Pow( _color.z, 2.0f / 2.2f );
					} );
					sRGB = true;	// Also encoded as sRGB now
				}

				ImageUtility.ImageFile	temp = new ImageUtility.ImageFile();
//				if ( texture.Name.ToLower() == "diffusecolor" )
//					temp.ToneMapFrom( source, ( float3 _HDR, ref float3 _LDR ) => { _LDR =_HDR; } );	// 8-bits for diffuse otherwise unity doesn't like it... :'(
//				else
					temp.ConvertFrom( source, ImageUtility.PIXEL_FORMAT.RGBA16 );

				System.IO.FileInfo	targetTextureFileName = new System.IO.FileInfo( System.IO.Path.Combine( fullTargetDirectory.FullName, texture.Name + ".png" ) );
				temp.Save( targetTextureFileName, ImageUtility.ImageFile.FILE_FORMAT.PNG );
// 				System.IO.FileInfo	targetTextureFileName = new System.IO.FileInfo( System.IO.Path.Combine( fullTargetDirectory.FullName, texture.Name + ".tif" ) );
// 				temp.Save( targetTextureFileName, ImageUtility.ImageFile.FILE_FORMAT.TIFF );

				// Generate or read meta file
				string	GUID = GenerateMeta( targetTextureFileName, checkBoxGenerateMeta.Checked, sRGB, isNormalMap, isIOR );
				GUIDs[textureIndex] = GUID;
				allGUIDsValid &= GUID != null;
			}

			if ( !checkBoxGenerateMat.Checked )
				return;
			if ( !allGUIDsValid )
				throw new Exception( "Not all texture GUIDs are valid! Can't generate material file!" );

			GenerateMaterial( _material, _targetDirectory, textureTypes, GUIDs );
		}

		string	GenerateMeta( System.IO.FileInfo _textureFileName, bool _createMeta, bool _sRGB, bool _isNormal, bool _isIOR ) {
			System.IO.FileInfo	metaFileName = new System.IO.FileInfo( _textureFileName.FullName + ".meta" );
			string	metaContent = null;
			string	stringGUID = null;
			if ( metaFileName.Exists ) {
				// Read back existing file
				using ( System.IO.StreamReader S = metaFileName.OpenText() )
					 metaContent = S.ReadToEnd();
			} else if ( _createMeta ) {
				Guid	GUID = Guid.NewGuid();
				stringGUID = GUID.ToString( "N" );

				metaContent = Properties.Resources.TemplateMeta;
				metaContent = metaContent.Replace( "<GUID>", stringGUID );
				metaContent = metaContent.Replace( "<sRGB>", _sRGB ? "1" : "0" );

				using ( System.IO.StreamWriter S = metaFileName.CreateText() )
					S.Write( metaContent );
			} else {
				return null;
			}

			// Retrieve GUID from string
			int	indexOfGUID = metaContent.IndexOf( "guid:" );
			if ( indexOfGUID != -1 ) {
				indexOfGUID += 5;
				int	indexOfEOL = metaContent.IndexOf( '\n', indexOfGUID );
				stringGUID = metaContent.Substring( indexOfGUID, indexOfEOL-indexOfGUID );
				stringGUID = stringGUID.Trim();
			}

			return stringGUID;
		}
		void	GenerateMaterial( AxFService.AxFFile.Material _material, System.IO.DirectoryInfo _targetDirectory, TEXTURE_TYPE[] _textureTypes, string[] _textureGUIDs ) {
			string	templateTexture =	"    - <TEX_VARIABLE_NAME>:\n" +
										"        m_Texture: {fileID: <FILE ID>, guid: <GUID>, type: 3}\n" +
										"        m_Scale: {x: 1, y: 1}\n" +
										"        m_Offset: {x: 0, y: 0}\n";

			string	materialContent = Properties.Resources.TemplateMaterial;

			//////////////////////////////////////////////////////////////////////////
			// Generate textures array
			bool	hasClearCoat = false;
			bool	hasHeightMap = false;
			string	texturesArray = "";
			for ( int textureIndex=0; textureIndex < _textureTypes.Length; textureIndex++ ) {
//				int		fileID = 2800000 + textureIndex;
				int		fileID = 2800000;
				string	GUID = _textureGUIDs[textureIndex];
				string	variableName = null;
				switch ( _textureTypes[textureIndex] ) {
					case TEXTURE_TYPE.ANISOTROPY_ANGLE:		variableName = "_SVBRDF_AnisotropicRotationAngleMap"; break;
					case TEXTURE_TYPE.CLEARCOAT_COLOR:		variableName = "_SVBRDF_ClearCoatColorMap_sRGB"; hasClearCoat = true; break;
					case TEXTURE_TYPE.CLEARCOAT_IOR:		variableName = "_SVBRDF_ClearCoatIORMap_sRGB"; hasClearCoat = true; break;
					case TEXTURE_TYPE.CLEARCOAT_NORMAL:		variableName = "_SVBRDF_ClearCoatNormalMap"; hasClearCoat = true; break;
					case TEXTURE_TYPE.DIFFUSE_COLOR:		variableName = "_SVBRDF_DiffuseColorMap_sRGB"; break;
					case TEXTURE_TYPE.FRESNEL:				variableName = "_SVBRDF_FresnelMap_sRGB"; break;
					case TEXTURE_TYPE.HEIGHT:				variableName = "_SVBRDF_HeightMap"; hasHeightMap = true; break;
					case TEXTURE_TYPE.NORMAL:				variableName = "_SVBRDF_NormalMap"; break;
					case TEXTURE_TYPE.OPACITY:				variableName = "_SVBRDF_OpacityMap"; break;
					case TEXTURE_TYPE.SPECULAR_COLOR:		variableName = "_SVBRDF_SpecularColorMap_sRGB"; break;
					case TEXTURE_TYPE.SPECULAR_LOBE:		variableName = "_SVBRDF_SpecularLobeMap"; break;
					default:
						throw new Exception( "Unsupported texture type! Can't match to variable name..." );
				}
				string	textureEntry = templateTexture.Replace( "<FILE ID>", fileID.ToString() );
						textureEntry = textureEntry.Replace( "<GUID>", GUID );
						textureEntry = textureEntry.Replace( "<TEX_VARIABLE_NAME>", variableName );
				texturesArray += textureEntry;
			}
			materialContent = materialContent.Replace( "<TEXTURES ARRAY>", texturesArray );

			//////////////////////////////////////////////////////////////////////////
			// Generate uniforms array
			string	uniformsArray = "";

			uniformsArray += "    - _materialSizeU_mm: 0\n";
			uniformsArray += "    - _materialSizeV_mm: 0\n";

			switch ( _material.Type ) {
				case AxFService.AxFFile.Material.TYPE.SVBRDF:	uniformsArray += "    - _AxF_BRDFType: 0\n"; break;
				case AxFService.AxFFile.Material.TYPE.CARPAINT:	uniformsArray += "    - _AxF_BRDFType: 1\n"; break;
				case AxFService.AxFFile.Material.TYPE.BTF:		uniformsArray += "    - _AxF_BRDFType: 2\n"; break;
			}

			switch ( _material.Type ) {
				case AxFService.AxFFile.Material.TYPE.SVBRDF: {
					// Setup flags
					uint	flags = 0;
							flags |= _material.IsAnisotropic ? 1U : 0;
							flags |= hasClearCoat ? 2U : 0;
							flags |= _material.GetPropertyBool( "cc_no_refraction", false ) ? 0 : 4U;	// Explicitly use no refraction
							flags |= hasHeightMap ? 8U : 0;

					uniformsArray += "    - _SVBRDF_flags: " + flags + "\n";

					// Setup SVBRDF diffuse & specular types
					uint	BRDFType = 0;
							BRDFType |= (uint) _material.DiffuseType;
							BRDFType |= ((uint) _material.SpecularType) << 1;

					uniformsArray += "    - _SVBRDF_BRDFType: " + BRDFType + "\n";

					// Setup SVBRDF fresnel and specular variants
					uint	BRDFVariants = 0;
							BRDFVariants |= ((uint) _material.FresnelVariant & 3);
					switch ( _material.SpecularVariant ) {
						// Ward variants
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.GEISLERMORODER:	BRDFVariants |= 0U << 2; break;
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.DUER:				BRDFVariants |= 1U << 2; break;
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.WARD:				BRDFVariants |= 2U << 2; break;

						// Blinn variants
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.ASHIKHMIN_SHIRLEY:	BRDFVariants |= 0U << 4; break;
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.BLINN:				BRDFVariants |= 1U << 4; break;
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.VRAY:				BRDFVariants |= 2U << 4; break;
						case AxFService.AxFFile.Material.SVBRDF_SPECULAR_VARIANT.LEWIS:				BRDFVariants |= 3U << 4; break;
					}

					uniformsArray += "    - _SVBRDF_BRDFVariants: " + BRDFVariants + "\n";
					break;
				}

				default:
					throw new Exception( "TODO! Support feeding variables to other BRDF types!" );
			}

			float	heightMapSize_mm = 0.0f;	// @TODO!
			uniformsArray += "    - _SVBRDF_heightMapMax_mm: " + heightMapSize_mm + "\n";

			materialContent = materialContent.Replace( "<UNIFORMS ARRAY>", uniformsArray );

			// Write target file
			System.IO.FileInfo materialFileName = new System.IO.FileInfo( System.IO.Path.Combine( _targetDirectory.FullName, _material.Name, "material.mat" ) );
			using ( System.IO.StreamWriter S = materialFileName.CreateText() )
				S.Write( materialContent );
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

			try {
				DumpMaterial( SelectedMaterial, new System.IO.DirectoryInfo( folderBrowserDialog1.SelectedPath ) );
				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while dumping material:", _e );
			}
		}

		private void buttonDumpAllMaterials_Click( object sender, EventArgs e ) {

			string	oldPath = GetRegKey( "AxFDumpFolder", System.IO.Path.GetDirectoryName( openFileDialog1.FileName ) );
			folderBrowserDialog1.SelectedPath = oldPath;
			if ( folderBrowserDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "AxFDumpFolder", folderBrowserDialog1.SelectedPath );

			try {
				System.IO.DirectoryInfo	targetFolder = new System.IO.DirectoryInfo( folderBrowserDialog1.SelectedPath );
				foreach ( AxFService.AxFFile.Material material in m_file.Materials ) {
					DumpMaterial( material, targetFolder );
				}
				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while dumping material:", _e );
			}
		}

		private void listBox1_SelectedIndexChanged( object sender, EventArgs e ) {
			SelectedMaterial = listBoxMaterials.SelectedItem as AxFService.AxFFile.Material;
		}

		private void checkBoxGenerateMeta_CheckedChanged( object sender, EventArgs e ) {
			checkBoxGenerateMat.Enabled = checkBoxGenerateMeta.Checked;	// Can't generate mat file if meta is disabled (because meta contains texture GUID and mat file needs the GUIDs to reference textures)
		}
	}
}
