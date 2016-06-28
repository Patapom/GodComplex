using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

//////////////////////////////////////////////////////////////////////////
//
//	AXIOMS
//	------
//	Diffuse =>	Texture alpha can contain gloss iff !$isMasking && !$isAlpha
//				In any case, the FetchDiffuseAlbedo should always replace alpha as a regular alpha channel when exiting the function...
//
//	Masks	=>	Masks cannot be merged if they don't have the same UV tiling/bias
//				Layer 0 mask is only used to apply coloring to layer 0 diffuse albedo
//
//////////////////////////////////////////////////////////////////////////
//
//	OPTIONS
//	-------
//
//		$alphaTest, $isMasking		=> Diffuse  with alpha, nothing to do for those materials: SKIP!
//		$extraLayer					=> 0, 1, 2 allows up to 3 layers
//
//	Layer 0
//		$use_Layer0_ColorConstant	=> Multiply diffuse by constant color
//		$Layer0_MaskMode			=> 0 = Vertex color, 1 = Map, 2 = Map * Vertex Color[channel]
//
//	Layer 1
//		$Layer1_diffuseReuseLayer	=> Re-use diffuse albedo from layer0, certainly with difference scaling
//		$use_Layer1_ColorConstant	=> Multiply diffuse by constant color
//		
//	Layer 2
//		$Layer2_diffuseReuseLayer	=> 0 = disabled, 1 = re-use layer 0 diffuse, 2 = re-use layer 1 diffuse
//		$use_Layer2_ColorConstant	=> Multiply diffuse by constant color
//		
//		
//		
//	renderparm Layer0_UVset					{ option 0 range [0, 1] }	// 0 first uvset // 1 second uvset
// 	renderparm Layer0_Mask_UVset			{ option 0 range [0, 1] }	// 0 first uvset // 1 second uvset
// 	renderparm Layer0_VtxColorMaskChannel	{ option 3 range [0, 3] }	// 0 red // 1 green // 2 blue // 3 alpha
// 	renderparm Layer0_MaskMode				{ option 0 range [0, 2] }	// 0 vertex color channel // 1 mask texture // 2 mask texture * vertex color channel
// 	renderparm Layer0_InvertMask			{ option 0 range [0, 1] }
// 	renderparm use_Layer0_ColorConstant		{ option 0 range [0, 1] }
// 	renderParm Layer0_ScaleBias				{ Uniform	float4	{ 1.0, 1.0, 0.0, 0.0 } range [{ -20.0, -20.0, 0.0, 0.0 }, { 20.0, 20.0, 1.0, 1.0 }] }
// 	renderParm Layer0_MaskScaleBias			{ Uniform	float4	{ 1.0, 1.0, 0.0, 0.0 } range [{ -20.0, -20.0, 0.0, 0.0 }, { 20.0, 20.0, 1.0, 1.0 }] }
// 	renderParm Layer0_Maskmap				{ Texture2D	float	_default }
// 	renderparm Layer0_ColorConstant			{ Uniform float4 { 1.0, 1.0, 1.0, 1.0 } range [{0,0,0,0}, {1,1,1,1}] }
// 	renderParm Layer0_RescaleValues			{ Uniform float2 { 0.4, 0.6 } range [{0,0}, {1,1}] }
// 
// 	renderparm Layer1_UVset					{ option 0 range [0, 1] }	// 0 first uvset // 1 second uvset
// 	renderparm Layer1_Mask_UVset			{ option 0 range [0, 1] }	// 0 first uvset // 1 second uvset
// 	renderparm Layer1_VtxColorMaskChannel	{ option 3 range [0, 3] }	// 0 red // 1 green // 2 blue // 3 alpha
// 	renderparm Layer1_MaskMode				{ option 0 range [0, 2] }	// 0 vertex color channel // 1 mask texture // 2 mask texture * vertex color channel
// 	renderparm Layer1_InvertMask			{ option 0 range [0, 1] }
// 	renderparm use_Layer1_ColorConstant		{ option 0 range [0, 1] }
// 	renderParm Layer1_diffuseReuseLayer		{ option 0 range [0, 1] }	// 0 = No reuse // 1 = re-use layer 0
// 	renderParm Layer1_bumpReuseLayer		{ option 0 range [0, 1] }	// 0 = No reuse // 1 = re-use layer 0
// 	renderParm Layer1_specularReuseLayer	{ option 0 range [0, 1] }	// 0 = No reuse // 1 = re-use layer 0
// 	renderParm Layer1_glossReuseLayer		{ option 0 range [0, 1] }	// 0 = No reuse // 1 = re-use layer 0
// 	renderParm Layer1_metallicReuseLayer	{ option 0 range [0, 1] }	// 0 = No reuse // 1 = re-use layer 0
// 	renderParm Layer1_maskReuseLayer		{ option 0 range [0, 1] }	// 0 = No reuse // 1 = re-use layer 0

// 	renderParm Layer1_ScaleBias				{ Uniform	float4	{ 1.0, 1.0, 0.0, 0.0 } range [{ -20.0, -20.0, 0.0, 0.0 }, { 20.0, 20.0, 1.0, 1.0 }] }
// 	renderParm Layer1_MaskScaleBias			{ Uniform	float4	{ 1.0, 1.0, 0.0, 0.0 } range [{ -20.0, -20.0, 0.0, 0.0 }, { 20.0, 20.0, 1.0, 1.0 }] }
// 	renderParm Layer1_Maskmap				{ Texture2D	float	_default }
// 	renderParm Layer1_diffuseMap			{ Texture2D	float4	_default }
// 	renderParm Layer1_bumpMap				{ Texture2D	float4	ipr_constantColor(0.5,0.5,0,0) }
// 	renderParm Layer1_specularMap			{ Texture2D	float4	ipr_constantColor(0,0,0,0) }
// 	renderParm Layer1_glossMap				{ Texture2D	float	ipr_constantColor(0,0,0,0) }
// 	renderParm Layer1_metallicMap			{ Texture2D	float	ipr_constantColor(0,0,0,0) }
// 	renderparm Layer1_ColorConstant			{ Uniform float4 { 1.0, 1.0, 1.0, 1.0 } range [{0,0,0,0}, {1,1,1,1}] }
// 	renderParm Layer1_RescaleValues			{ Uniform float2 { 0.4, 0.6 } range [{0,0}, {1,1}] }
// 
// 	renderparm Layer2_UVset					{ option 0 range [0, 1] }	// 0 first uvset // 1 second uvset
// 	renderparm Layer2_Mask_UVset			{ option 0 range [0, 1] }	// 0 first uvset // 1 second uvset
// 	renderparm Layer2_VtxColorMaskChannel	{ option 3 range [0, 3] }	// 0 red // 1 green // 2 blue // 3 alpha
// 	renderparm Layer2_MaskMode				{ option 0 range [0, 2] }	// 0 vertex color channel // 1 mask texture // 2 mask texture * vertex color channel
// 	renderparm Layer2_InvertMask			{ option 0 range [0, 1] }
// 	renderparm use_Layer2_ColorConstant		{ option 0 range [0, 1] }
// 	renderParm Layer2_diffuseReuseLayer		{ option 0 range [0, 2] }	// 0 = No re-use // 1 = re-user layer 0 // 2 = re-use layer 1
// 	renderParm Layer2_bumpReuseLayer		{ option 0 range [0, 2] }	// 0 = No re-use // 1 = re-user layer 0 // 2 = re-use layer 1
// 	renderParm Layer2_specularReuseLayer	{ option 0 range [0, 2] }	// 0 = No re-use // 1 = re-user layer 0 // 2 = re-use layer 1
// 	renderParm Layer2_glossReuseLayer		{ option 0 range [0, 2] }	// 0 = No re-use // 1 = re-user layer 0 // 2 = re-use layer 1
// 	renderParm Layer2_metallicReuseLayer	{ option 0 range [0, 2] }	// 0 = No re-use // 1 = re-user layer 0 // 2 = re-use layer 1
// 	renderParm Layer2_maskReuseLayer		{ option 0 range [0, 2] }	// 0 = No re-use // 1 = re-user layer 0 // 2 = re-use layer 1

// 	renderParm Layer2_ScaleBias				{ Uniform	float4	{ 1.0, 1.0, 0.0, 0.0 } range [{ -20.0, -20.0, 0.0, 0.0 }, { 20.0, 20.0, 1.0, 1.0 }] }
// 	renderParm Layer2_MaskScaleBias			{ Uniform	float4	{ 1.0, 1.0, 0.0, 0.0 } range [{ -20.0, -20.0, 0.0, 0.0 }, { 20.0, 20.0, 1.0, 1.0 }] }
// 	renderParm Layer2_Maskmap				{ Texture2D	float	_default }
// 	renderParm Layer2_diffuseMap			{ Texture2D	float4	_default }
// 	renderParm Layer2_bumpMap				{ Texture2D	float4	ipr_constantColor(0.5,0.5,0,0) }
// 	renderParm Layer2_specularMap			{ Texture2D	float4	ipr_constantColor(0,0,0,0) }
// 	renderParm Layer2_glossMap				{ Texture2D	float	ipr_constantColor(0,0,0,0) }
// 	renderParm Layer2_metallicMap			{ Texture2D	float	ipr_constantColor(0,0,0,0) }
// 	renderparm Layer2_ColorConstant			{ Uniform float4 { 1.0, 1.0, 1.0, 1.0 } range [{0,0,0,0}, {1,1,1,1}] }
// 	renderParm Layer2_RescaleValues			{ Uniform float2 { 0.4, 0.6 } range [{0,0}, {1,1}] }
//
//
//////////////////////////////////////////////////////////////////////////
//	Layering & Masking
//	------------------
// 
// 	void initLayeredTexcoord() {
// 		#if $Layer0_UVset == 0
// 			float2 layer0_uv = m_texCoords.xy;
// 		#else
// 			float2 layer0_uv = m_texCoords.zw;
// 		#endif
//
//		m_layerTexCoords[0] = layer0_uv*$Layer0_ScaleBias.xy + $Layer0_ScaleBias.zw;
//		m_layerTexCoords[1] = m_layerTexCoords[0];
//		m_layerTexCoords[2] = m_layerTexCoords[0];
// 		#if( $extraLayer > 0 )
// 			#if $Layer1_UVset == 0
// 				float2 layer1_uv = m_texCoords.xy;
// 			#else
// 				float2 layer1_uv = m_texCoords.zw;
// 			#endif
// 			m_layerTexCoords[1] = layer1_uv*$Layer1_ScaleBias.xy + $Layer1_ScaleBias.zw;
// 			#if( $extraLayer > 1 )
// 				#if $Layer2_UVset == 0
// 					float2 layer2_uv = m_texCoords.xy;
// 				#else
// 					float2 layer2_uv = m_texCoords.zw;
// 				#endif
// 				m_layerTexCoords[2] = layer2_uv*$Layer2_ScaleBias.xy + $Layer2_ScaleBias.zw;
// 			#endif
// 		#endif
//
// 		#if $Layer0_Mask_UVset == 0
// 			float2 layer0_mask_uv = m_texCoords.xy;
// 		#else
// 			float2 layer0_mask_uv = m_texCoords.zw;
// 		#endif
//
// 		m_maskTexCoords[0] = layer0_mask_uv*$Layer0_MaskScaleBias.xy + $Layer0_MaskScaleBias.zw;
// 		m_maskTexCoords[1] = m_maskTexCoords[0];
// 		m_maskTexCoords[2] = m_maskTexCoords[0];
// 		#if( $extraLayer > 0 )
// 			#if $Layer1_Mask_UVset == 0
// 				float2 layer1_mask_uv = m_texCoords.xy;
// 			#else
// 				float2 layer1_mask_uv = m_texCoords.zw;
// 			#endif
// 			m_maskTexCoords[1] = layer1_mask_uv*$Layer1_MaskScaleBias.xy + $Layer1_MaskScaleBias.zw;
// 			#if( $extraLayer > 1 )
// 				#if $Layer2_Mask_UVset == 0
// 					float2 layer2_mask_uv = m_texCoords.xy;
// 				#else
// 					float2 layer2_mask_uv = m_texCoords.zw;
// 				#endif
// 				m_maskTexCoords[2] = layer2_mask_uv*$Layer2_MaskScaleBias.xy + $Layer2_MaskScaleBias.zw;
// 			#endif
// 		#endif
// 	}
//
// 	void HQ_FetchMasks( inout arkPixelContext_t _pixelCtx ) {
// 		float layer0_mask = 0.0;
// 		#if $use_Layer0_ColorConstant
// 			#if $Layer0_MaskMode == 0		
// 				_pixelCtx.m_masks[0] = _pixelCtx.m_vertexColor[$Layer0_VtxColorMaskChannel];
// 			#elif $Layer0_MaskMode == 1
// 				layer0_mask = $Layer0_Maskmap.Sample($anisotropicSampler, _pixelCtx.m_maskTexCoords[0]);
// 				_pixelCtx.m_masks[0] = layer0_mask;
// 			#else
// 				layer0_mask = $Layer0_Maskmap.Sample($anisotropicSampler, _pixelCtx.m_maskTexCoords[0]).r;
// 				float L0_M1 = layer0_mask;
// 				float L0_M2 = _pixelCtx.m_vertexColor[$Layer0_VtxColorMaskChannel];
// 				float L0_M = L0_M1 * L0_M2;
// 				_pixelCtx.m_masks[0] = LinearStep( $Layer0_RescaleValues.x, $Layer0_RescaleValues.y, L0_M );
// 			#endif
// 		#else
// 			_pixelCtx.m_masks[0] = 0;
// 		#endif
// 
// 		#if $Layer0_InvertMask
// 			_pixelCtx.m_masks[0] = 1-_pixelCtx.m_masks[0];
// 		#endif
// 			
// 		#if( $extraLayer > 0 )
// 			float layer1_mask = 0.0;
// 
// 			#if $Layer1_MaskMode == 0		
// 				_pixelCtx.m_masks[1] = _pixelCtx.m_vertexColor[$Layer1_VtxColorMaskChannel];
// 			#elif $Layer1_MaskMode == 1
// 				#if $Layer1_maskReuseLayer
// 					layer1_mask = layer0_mask;
// 				#else
// 					layer1_mask = $Layer1_Maskmap.Sample($anisotropicSampler, _pixelCtx.m_maskTexCoords[1]).r;
// 				#endif
// 				_pixelCtx.m_masks[1] = layer1_mask;
// 			#else
// 				#if $Layer1_maskReuseLayer
// 					layer1_mask = layer0_mask;
// 				#else
// 					layer1_mask = $Layer1_Maskmap.Sample($anisotropicSampler, _pixelCtx.m_maskTexCoords[1]).r;
// 				#endif
// 				float L1_M1 = layer1_mask;
// 				float L1_M2 = _pixelCtx.m_vertexColor[$Layer1_VtxColorMaskChannel];
// 				float L1_M = L1_M1 * L1_M2;
// 				_pixelCtx.m_masks[1] = LinearStep( $Layer1_RescaleValues.x, $Layer1_RescaleValues.y, L1_M );
// 			#endif
// 			#if $Layer1_InvertMask
// 				_pixelCtx.m_masks[1] = 1-_pixelCtx.m_masks[1];
// 			#endif
// 
// 			#if( $extraLayer > 1 )
// 				float layer2_mask = 0.0;
// 				
// 				#if $Layer2_MaskMode == 0		
// 					_pixelCtx.m_masks[2] = _pixelCtx.m_vertexColor[$Layer2_VtxColorMaskChannel];
// 				#elif $Layer2_MaskMode == 1
// 					#if $Layer2_maskReuseLayer == 0
// 						layer2_mask = $Layer2_Maskmap.Sample($anisotropicSampler, _pixelCtx.m_maskTexCoords[2]).r;
// 					#elif $Layer2_maskReuseLayer == 1
// 						layer2_mask = layer0_mask;
// 					#else
// 						layer2_mask = layer1_mask;
// 					#endif
// 					_pixelCtx.m_masks[2] = layer2_mask;
// 				#else
// 					#if $Layer2_maskReuseLayer == 0
// 						layer2_mask = $Layer2_Maskmap.Sample($anisotropicSampler, _pixelCtx.m_maskTexCoords[2]).r;
// 					#elif $Layer2_maskReuseLayer == 1
// 						layer2_mask = layer0_mask;
// 					#else
// 						layer2_mask = layer1_mask;
// 					#endif
// 					float L2_M1 = layer2_mask;
// 					float L2_M2 = _pixelCtx.m_vertexColor[$Layer2_VtxColorMaskChannel];
// 					float L2_M = L2_M1 * L2_M2;
// 					_pixelCtx.m_masks[2] = LinearStep( $Layer2_RescaleValues.x, $Layer2_RescaleValues.y, L2_M );
// 				#endif
// 				#if $Layer2_InvertMask
// 					_pixelCtx.m_masks[2] = 1-_pixelCtx.m_masks[2];
// 				#endif
// 					
// 			#endif
// 		#endif
// 	}
// 
//
//	void HQ_FetchDiffuseAlbedo( inout arkPixelContext_t _pixelCtx ) {
// 		// ==== Sample diffuse albedo ====
// 		#if $alphaTest
// 			float4 Layer0_DiffuseTexture = $diffusemap.Sample( $anisotropicSampler, _pixelCtx.m_layerTexCoords[0] );
// 		#else
// 			float4 Layer0_DiffuseTexture = $diffusemap.Sample( $anisotropicSamplerHQ, _pixelCtx.m_layerTexCoords[0] );
// 		#endif
// 		_pixelCtx.m_diffuseAlbedo = Layer0_DiffuseTexture;
// 						
// 		#if $alphaTest && !$(tool/unlit)
// 			clip( _pixelCtx.m_diffuseAlbedo.w - $DefaultAlphaTest );
// 		#endif
// 
// 		#if $use_Layer0_ColorConstant
// 			float4 layer0_ColorConstant = lerp( float4(1,1,1,1), $Layer0_ColorConstant, _pixelCtx.m_masks[0] );
// 			_pixelCtx.m_diffuseAlbedo *= layer0_ColorConstant;
// 		#endif
// 			
// 		#if( $extraLayer > 0 )
// 			#if( $Layer1_diffuseReuseLayer )
// 				float4 Layer1_DiffuseTexture = Layer0_DiffuseTexture;
// 			#else
// 				float4 Layer1_DiffuseTexture = $Layer1_diffuseMap.Sample( $anisotropicSampler, _pixelCtx.m_layerTexCoords[1] );
// 			#endif
// 			float4 Layer1_Diffuse = Layer1_DiffuseTexture;
// 			#if $use_Layer1_ColorConstant
// 				Layer1_Diffuse *= $Layer1_ColorConstant;
// 			#endif
// 				
// 			_pixelCtx.m_diffuseAlbedo = lerp( _pixelCtx.m_diffuseAlbedo, Layer1_Diffuse, _pixelCtx.m_masks[1] );
// 
// 			#if( $extraLayer > 1 )
// 				#if( $Layer2_diffuseReuseLayer == 2 )
// 					float4 Layer2_Diffuse = Layer1_DiffuseTexture;
// 				#elif( $Layer2_diffuseReuseLayer == 1 )
// 					float4 Layer2_Diffuse = Layer0_DiffuseTexture;
// 				#else
// 					float4 Layer2_Diffuse = $Layer2_diffuseMap.Sample( $anisotropicSampler, _pixelCtx.m_layerTexCoords[2] );
// 				#endif
// 
// 				#if $use_Layer2_ColorConstant
// 					Layer2_Diffuse *= $Layer2_ColorConstant;
// 				#endif
// 
// 				_pixelCtx.m_diffuseAlbedo = lerp( _pixelCtx.m_diffuseAlbedo, Layer2_Diffuse, _pixelCtx.m_masks[2] );
// 			#endif
// 		#endif
// 
// 		#if $VtxColorMultiply || ($useParticleColorInstancing && !$isShadow && !$isZPrePass)
// 			_pixelCtx.m_diffuseAlbedo *= _pixelCtx.m_vertexColor;
// 		#endif
// 	}
//
//
//////////////////////////////////////////////////////////////////////////
//
namespace MaterialsOptimizer
{
	public partial class Form1 : Form {

		class Error {
			public FileInfo		m_fileName;
			public Exception	m_error;

			public override string ToString() {
				return "ERROR! " + m_fileName.FullName + " > " + FullMessage( m_error );
			}

			public Error( FileInfo _fileName, Exception _error ) {
				m_fileName = _fileName;
				m_error = _error;
			}
			public Error( BinaryReader R ) {
				Read( R );
			}

			public void		Write( BinaryWriter W ) {
				W.Write( m_fileName.FullName );
				W.Write( FullMessage( m_error ) );
			}

			public void		Read( BinaryReader R ) {
				m_fileName = new FileInfo( R.ReadString() );
				m_error = new Exception( R.ReadString() );
			}

			public string	FullMessage( Exception _e ) {
				return _e.Message + (_e.InnerException != null ? "\r\n" + FullMessage( _e.InnerException ) : "");
			}
		}

		private RegistryKey			m_AppKey;
		private string				m_ApplicationPath;



		// Materials database
		private List< Material >		m_materials = new List< Material >();
		private List< Error >			m_materialErrors = new List< Error >();
		private FileInfo				m_materialsDatabaseFileName = new FileInfo( "materials.database" );
		private int						m_materialsSortColumn = 0;
		private int						m_materialsSortOrder = 1;

		// Textures database
		private List< TextureFileInfo >	m_textures = new List< TextureFileInfo >();
		private List< Error >			m_textureErrors = new List< Error >();
		private FileInfo				m_texturesDatabaseFileName = new FileInfo( "textures.database" );
		private int						m_texturesSortColumn = 0;
		private int						m_texturesSortOrder = 1;

		public Form1()
		{
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\Arkane\MaterialsOptimizer" );
			m_ApplicationPath = Path.GetDirectoryName( Application.ExecutablePath );

			Material.Layer.Texture.ms_TexturesBasePath = new DirectoryInfo( textBoxTexturesBasePath.Text );

			// Reload textures database
			if ( m_materialsDatabaseFileName.Exists )
				LoadMaterialsDatabase( m_materialsDatabaseFileName );
			if ( m_texturesDatabaseFileName.Exists )
				LoadTexturesDatabase( m_texturesDatabaseFileName );
			AnalyzeMaterialsDatabase( false );

//ParseFile( new FileInfo( @"V:\Dishonored2\Dishonored2\base\decls\m2\models\environment\buildings\rich_large_ext_partitions_01.m2" ) );
//			RecurseParseMaterials( new DirectoryInfo( @"V:\Dishonored2\Dishonored2\base\decls\m2" ) );
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

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
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
			return System.Windows.Forms.MessageBox.Show( this, _Text, "Materials Optimizer", _Buttons, _Icon );
		}

		void	Log( string _text ) {
			textBoxLog.AppendText( _text );
		}
		void	LogLine( string _text ) {
			Log( _text + "\n" );
		}

		#endregion

		#region Materials Parsing

		void	RecurseParseMaterials( DirectoryInfo _directory ) {

			FileInfo[]	materialFileNames = _directory.GetFiles( "*.m2", SearchOption.AllDirectories );
			foreach ( FileInfo materialFileName in materialFileNames ) {

				try {
					ParseMaterialFile( materialFileName );
				} catch ( Exception _e ) {
					Error	Err = new Error( materialFileName, _e );
					m_materialErrors.Add( Err );
					LogLine( Err.ToString() );
				}
			}
		}

		void	ParseMaterialFile( FileInfo _fileName ) {
			string	fileContent = null;
			using ( StreamReader R = _fileName.OpenText() )
				fileContent = R.ReadToEnd();

			Parser	P = new Parser( fileContent );
			while ( P.OK ) {
				string	token = P.ReadString();
				if ( token == null )
					return;
				if ( token.StartsWith( "//" ) ) {
					P.ReadToEOL();
					continue;
				}
				if ( token.StartsWith( "/*" ) ) {
					P.SkipComment();
					continue;
				}

				switch ( token.ToLower() ) {
					case "material":
						string		materialName = P.ReadString();
						if ( materialName.EndsWith( "{" ) ) {
							materialName = materialName.Substring( 0, materialName.Length-1 );
							P.m_Index--;
						}

						P.SkipSpaces();
						if ( P[0] == '/' && P[1] == '*' ) {
							P.SkipComment();	// YES! Someone did do that!
						}

						string		materialContent;
						try {
							materialContent = P.ReadBlock();
						} catch ( Exception _e ) {
							throw new Exception( "Failed parsing material content block for material \"" + materialName + "\"! Check strange comment markers and matching closing brackets", _e );
						}
						try {
							Material	M = new Material( _fileName, materialName, materialContent );
							m_materials.Add( M );
						} catch ( Exception _e ) {
							throw new Exception( "Failed parsing material!", _e );
						}
						break;
				}
			}
		}

		void		AnalyzeMaterialsDatabase( bool _yell ) {
			if ( m_materials.Count == 0 ) {
				if ( _yell )
					MessageBox( "Can't analyze materials database since there are no materials available!\r\nTry and parse materials to enable analysis...", MessageBoxButtons.OK );
				return;
			}
			if ( m_textures.Count == 0 ) {
				if ( _yell )
					MessageBox( "Can't analyze materials database since there are no textures available!\r\nTry and collect textures to enable analysis...", MessageBoxButtons.OK );
				return;
			}


		}

		void		SaveMaterialsDatabase( FileInfo _fileName ) {
			using ( FileStream S = _fileName.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					W.Write( m_materials.Count );
					foreach ( Material M in m_materials )
						M.Write( W );

					W.Write( m_materialErrors.Count );
					foreach ( Error E in m_materialErrors )
						E.Write( W );
				}
		}

		void		LoadMaterialsDatabase( FileInfo _fileName ) {
			m_materials.Clear();
			m_materialErrors.Clear();

			using ( FileStream S = _fileName.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {
					int	materialsCount = R.ReadInt32();
					for ( int errorIndex=0; errorIndex < materialsCount; errorIndex++ )
						m_materials.Add( new Material( R ) );

					int	errorsCount = R.ReadInt32();
					for ( int errorIndex=0; errorIndex < errorsCount; errorIndex++ )
						m_materialErrors.Add( new Error( R ) );
				}

			RebuildMaterialsListView();
		}

		void		RebuildMaterialsListView() {

			// Filter materials
			List< Material >	filteredMaterials = new List< Material >();
			bool	skipDefault = !checkBoxShowArkDefault.Checked ^ checkBoxInvertMaterialFilters.Checked;
			bool	skipSkin = !checkBoxShowSkin.Checked ^ checkBoxInvertMaterialFilters.Checked;
			bool	skipEye = !checkBoxShowEye.Checked ^ checkBoxInvertMaterialFilters.Checked;
			bool	skipHair = !checkBoxShowHair.Checked ^ checkBoxInvertMaterialFilters.Checked;
			bool	skipVegetation = !checkBoxShowVegetation.Checked ^ checkBoxInvertMaterialFilters.Checked;
			bool	skipVista = !checkBoxShowVista.Checked ^ checkBoxInvertMaterialFilters.Checked;
			bool	skipOther = !checkBoxShowOtherMaterialTypes.Checked ^ checkBoxInvertMaterialFilters.Checked;
			int		layersCountMin = integerTrackbarControlLayerMin.Value;
			int		layersCountMax = integerTrackbarControlLayerMax.Value;
			foreach ( Material M in m_materials ) {
// 				// Filter by type
// 				if ( TFI.m_fileType == TextureFileInfo.FILE_TYPE.TGA ) {
// 					texCountTGA++;
// 					if ( !checkBoxShowTGA.Checked )
// 						continue;
// 				} else if ( TFI.m_fileType == TextureFileInfo.FILE_TYPE.PNG ) {
// 					texCountPNG++;
// 					if ( !checkBoxShowPNG.Checked )
// 						continue;
// 				} else {
// 					texCountOther++;
// 					if ( !checkBoxShowOtherFormats.Checked )
// 						continue;
// 				}

				// Filter by layers count
				if ( M.m_layers.Count < layersCountMin || M.m_layers.Count > layersCountMax )
					continue;

				// Filter by program type
				bool	skip = false;
				switch ( M.m_programs.m_type ) {
					case Material.Programs.KNOWN_TYPES.DEFAULT: skip = skipDefault; break;
					case Material.Programs.KNOWN_TYPES.SKIN: skip = skipSkin; break;
					case Material.Programs.KNOWN_TYPES.EYE: skip = skipEye; break;
					case Material.Programs.KNOWN_TYPES.HAIR: skip = skipHair; break;
					case Material.Programs.KNOWN_TYPES.VEGETATION: skip = skipVegetation; break;
					case Material.Programs.KNOWN_TYPES.VISTA: skip = skipVista; break;
					default: skip = skipOther; break;
				}

				if ( !skip )
					filteredMaterials.Add( M );
			}

// 			checkBoxShowTGA.Text = "Show " + texCountTGA + " TGA";
// 			checkBoxShowPNG.Text = "Show " + texCountPNG + " PNG";
// 			checkBoxShowOtherFormats.Text = "Show " + texCountOther + " misc. formats";
			labelTotalMaterials.Text = "Total Materials:\n" + filteredMaterials.Count;

			// Sort
			if ( m_materialsSortOrder == 1 ) {
				switch ( m_materialsSortColumn ) {
					case 0: filteredMaterials.Sort( new MatCompareNames_Ascending() ); break;
					case 1: filteredMaterials.Sort( new MatCompareTypes_Ascending() ); break;
					case 2: filteredMaterials.Sort( new MatCompareLayers_Ascending() ); break;
					case 6: filteredMaterials.Sort( new MatCompareFileNames_Ascending() ); break;
				}
			} else {
				switch ( m_materialsSortColumn ) {
					case 0: filteredMaterials.Sort( new MatCompareNames_Descending() ); break;
					case 1: filteredMaterials.Sort( new MatCompareTypes_Descending() ); break;
					case 2: filteredMaterials.Sort( new MatCompareLayers_Descending() ); break;
					case 6: filteredMaterials.Sort( new MatCompareFileNames_Descending() ); break;
				}
			}

			// Rebuild list view
			listViewMaterials.BeginUpdate();
			listViewMaterials.Items.Clear();
			foreach ( Material M in filteredMaterials ) {

				ListViewItem	item = new ListViewItem( M.m_name );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, M.m_programs.m_type.ToString() ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, M.m_layers.Count.ToString() ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, M.m_options.IsAlpha ? "Yes" : "" ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, M.m_isCandidateForOptmization ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, M.m_errors ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, M.m_sourceFileName.FullName ) );

				if ( M.m_errors != null ) {
					item.BackColor = Color.Salmon;
				} else if ( M.m_isCandidateForOptmization != null ) {
					item.BackColor = Color.ForestGreen;
				}

				listViewMaterials.Items.Add( item );
			}

			listViewMaterials.EndUpdate();
		}

		class	MatCompareNames_Ascending : IComparer< Material > {
			public int Compare(Material x, Material y) {
				return StringComparer.CurrentCultureIgnoreCase.Compare( x.m_name, y.m_name );
			}
		}
		class	MatCompareNames_Descending : IComparer< Material > {
			public int Compare(Material x, Material y) {
				return -StringComparer.CurrentCultureIgnoreCase.Compare( x.m_name, y.m_name );
			}
		}

		class	MatCompareTypes_Ascending : IComparer< Material > {
			public int Compare(Material x, Material y) {
//				return StringComparer.CurrentCultureIgnoreCase.Compare( x.m_programs.m_type.ToString(), y.m_programs.m_type.ToString() );
				return (int) x.m_programs.m_type < (int) y.m_programs.m_type ? -1 : ((int) x.m_programs.m_type > (int) y.m_programs.m_type ? 1 : 0);
			}
		}
		class	MatCompareTypes_Descending : IComparer< Material > {
			public int Compare(Material x, Material y) {
//				return -StringComparer.CurrentCultureIgnoreCase.Compare( x.m_programs.m_type.ToString(), y.m_programs.m_type.ToString() );
				return (int) x.m_programs.m_type < (int) y.m_programs.m_type ? 1 : ((int) x.m_programs.m_type > (int) y.m_programs.m_type ? -1 : 0);
			}
		}

		class	MatCompareLayers_Ascending : IComparer< Material > {
			public int Compare(Material x, Material y) {
				return x.m_layers.Count < y.m_layers.Count ? -1 : (x.m_layers.Count > y.m_layers.Count ? 1 : 0);
			}
		}
		class	MatCompareLayers_Descending : IComparer< Material > {
			public int Compare(Material x, Material y) {
				return x.m_layers.Count < y.m_layers.Count ? 1 : (x.m_layers.Count > y.m_layers.Count ? -1 : 0);
			}
		}

		class	MatCompareFileNames_Ascending : IComparer< Material > {
			public int Compare(Material x, Material y) {
				return StringComparer.CurrentCultureIgnoreCase.Compare( x.m_sourceFileName.FullName, y.m_sourceFileName.FullName );
			}
		}
		class	MatCompareFileNames_Descending : IComparer< Material > {
			public int Compare(Material x, Material y) {
				return -StringComparer.CurrentCultureIgnoreCase.Compare( x.m_sourceFileName.FullName, y.m_sourceFileName.FullName );
			}
		}

		#endregion

		#region Textures Parsing

		void	CollectTextures( DirectoryInfo _directory ) {

			ImageUtility.Bitmap.ReadContent = false;

			m_textures.Clear();
			m_textureErrors.Clear();

			string[]	supportedExtensions = new string[] {
				 ".jpg",
				 ".png",
				 ".tga",
				 ".tiff",
//				 ".dds",
			};

			foreach ( string supportedExtension in supportedExtensions ) {
				FileInfo[]	textureFileNames = _directory.GetFiles( "*" + supportedExtension, SearchOption.AllDirectories );

				LogLine( "Parsing " + textureFileNames.Length + " " + supportedExtension + " image files" );
				DateTime	startTime = DateTime.Now;

				int			textureIndex = 0;
				foreach ( FileInfo textureFileName in textureFileNames ) {

// 					bool	supported = false;
// 					bool	isAnImage = true;
// 					string	extension = Path.GetExtension( textureFileName.Name ).ToLower();
// 					switch ( extension ) {
// 						case ".tga": supported = true; break;
// 						case ".png": supported = true; break;
// 						case ".jpg": supported = true; break;
// 						case ".tiff": supported = true; break;
// 						case ".dds": supported = true; break;	// But can't read it at the moment...
// 
// 						default: isAnImage = false; break;
// 					}
// 
// 					if ( !supported ) {
// 						continue;	// Unknown file type
// 					}

					try {
						TextureFileInfo	T = new TextureFileInfo( textureFileName );
						m_textures.Add( T );
					} catch ( Exception _e ) {
						Error	Err = new Error( textureFileName, _e );
						m_textureErrors.Add( Err );
						LogLine( Err.ToString() );
					}

					textureIndex++;
				}

				TimeSpan	totalTime = DateTime.Now - startTime;
				LogLine( "Finished parsing " + supportedExtension + " image files. Total time: " + totalTime.ToString( @"mm\:ss" ) );
			}

			ImageUtility.Bitmap.ReadContent = true;
		}

		void		SaveTexturesDatabase( FileInfo _fileName ) {
			using ( FileStream S = _fileName.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					W.Write( m_textures.Count );
					foreach ( TextureFileInfo TFI in m_textures )
						TFI.Write( W );

					W.Write( m_textureErrors.Count );
					foreach ( Error E in m_textureErrors )
						E.Write( W );
				}
		}

		void		LoadTexturesDatabase( FileInfo _fileName ) {
			m_textures.Clear();
			m_textureErrors.Clear();

			using ( FileStream S = _fileName.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {
					int	texturesCount = R.ReadInt32();
					for ( int errorIndex=0; errorIndex < texturesCount; errorIndex++ )
						m_textures.Add( new TextureFileInfo( R ) );

					int	errorsCount = R.ReadInt32();
					for ( int errorIndex=0; errorIndex < errorsCount; errorIndex++ )
						m_textureErrors.Add( new Error( R ) );
				}

			RebuildTexturesListView();
		}

		void		RebuildTexturesListView() {

			// Filter textures
			List< TextureFileInfo >	filteredTextures = new List< TextureFileInfo >();
			int		texCountPNG = 0;
			int		texCountTGA = 0;
			int		texCountOther = 0;
			bool	skipDiffuse = !checkBoxShowDiffuse.Checked ^ checkBoxInvertFilters.Checked;
			bool	skipNormal = !checkBoxShowNormal.Checked ^ checkBoxInvertFilters.Checked;
			bool	skipGloss = !checkBoxShowGloss.Checked ^ checkBoxInvertFilters.Checked;
			bool	skipMetal = !checkBoxShowMetal.Checked ^ checkBoxInvertFilters.Checked;
			bool	skipEmissive = !checkBoxShowEmissive.Checked ^ checkBoxInvertFilters.Checked;
			bool	skipOther = !checkBoxShowOther.Checked ^ checkBoxInvertFilters.Checked;
			foreach ( TextureFileInfo TFI in m_textures ) {
				// Filter by type
				if ( TFI.m_fileType == TextureFileInfo.FILE_TYPE.TGA ) {
					texCountTGA++;
					if ( !checkBoxShowTGA.Checked )
						continue;
				} else if ( TFI.m_fileType == TextureFileInfo.FILE_TYPE.PNG ) {
					texCountPNG++;
					if ( !checkBoxShowPNG.Checked )
						continue;
				} else {
					texCountOther++;
					if ( !checkBoxShowOtherFormats.Checked )
						continue;
				}

				// Filter by usage
				bool	skip = false;
				switch ( TFI.m_usage ) {
					case TextureFileInfo.USAGE.DIFFUSE: skip = skipDiffuse; break;
					case TextureFileInfo.USAGE.NORMAL: skip = skipNormal; break;
					case TextureFileInfo.USAGE.GLOSS: skip = skipGloss; break;
					case TextureFileInfo.USAGE.METAL: skip = skipMetal; break;
					case TextureFileInfo.USAGE.EMISSIVE: skip = skipEmissive; break;
					default: skip = skipOther; break;
				}

				if ( !skip )
					filteredTextures.Add( TFI );
			}

			checkBoxShowTGA.Text = "Show " + texCountTGA + " TGA";
			checkBoxShowPNG.Text = "Show " + texCountPNG + " PNG";
			checkBoxShowOtherFormats.Text = "Show " + texCountOther + " misc. formats";
			labelTotalTextures.Text = "Total Textures:\n" + filteredTextures.Count;

			// Sort
//			filteredTextures.Sort( this );
			if ( m_texturesSortOrder == 1 ) {
				switch ( m_texturesSortColumn ) {
					case 0: filteredTextures.Sort( new CompareNames_Ascending() ); break;
					case 1: filteredTextures.Sort( new CompareSizes_Ascending() ); break;
					case 2: filteredTextures.Sort( new CompareUsages_Ascending() ); break;
					case 3: filteredTextures.Sort( new CompareRefCounts_Ascending() ); break;
				}
			} else {
				switch ( m_texturesSortColumn ) {
					case 0: filteredTextures.Sort( new CompareNames_Descending() ); break;
					case 1: filteredTextures.Sort( new CompareSizes_Descending() ); break;
					case 2: filteredTextures.Sort( new CompareUsages_Descending() ); break;
					case 3: filteredTextures.Sort( new CompareRefCounts_Descending() ); break;
				}
			}


			// Rebuild list view
			listViewTextures.BeginUpdate();
			listViewTextures.Items.Clear();
			foreach ( TextureFileInfo TFI in filteredTextures ) {

				ListViewItem	item = new ListViewItem( TFI.m_fileName.FullName );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, TFI.m_width.ToString() + "x" + TFI.m_height.ToString() ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, TFI.m_usage.ToString() ) );
				item.SubItems.Add( new ListViewItem.ListViewSubItem( item, TFI.m_refCount.ToString() ) );

				int	area = TFI.m_width * TFI.m_height;
				if ( area > 2048*2048 ) {
					item.BackColor = Color.Salmon;
				} else if ( area > 1024*1024 ) {
					item.BackColor = Color.Gold;
				}

				listViewTextures.Items.Add( item );
			}

			listViewTextures.EndUpdate();
		}

		class	CompareNames_Ascending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
				return StringComparer.CurrentCultureIgnoreCase.Compare( x.m_fileName.FullName, y.m_fileName.FullName );
			}
		}
		class	CompareNames_Descending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
				return -StringComparer.CurrentCultureIgnoreCase.Compare( x.m_fileName.FullName, y.m_fileName.FullName );
			}
		}

		class	CompareSizes_Ascending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
				int	area0 = x.m_width * x.m_height;
				int	area1 = y.m_width * y.m_height;
				return area0 < area1 ? -1 : (area0 > area1 ? 1 : 0);
			}
		}
		class	CompareSizes_Descending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
				int	area0 = x.m_width * x.m_height;
				int	area1 = y.m_width * y.m_height;
				return area0 < area1 ? 1 : (area0 > area1 ? -1 : 0);
			}
		}

		class	CompareUsages_Ascending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
//				return StringComparer.CurrentCultureIgnoreCase.Compare( x.m_usage.ToString(), y.m_usage.ToString() );
				return (int) x.m_usage < (int) y.m_usage ? -1 : ((int) x.m_usage > (int) y.m_usage ? 1 : 0);
			}
		}
		class	CompareUsages_Descending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
//				return -StringComparer.CurrentCultureIgnoreCase.Compare( x.m_usage.ToString(), y.m_usage.ToString() );
				return (int) x.m_usage < (int) y.m_usage ? 1 : ((int) x.m_usage > (int) y.m_usage ? -1 : 0);
			}
		}

		class	CompareRefCounts_Ascending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
				return x.m_refCount < y.m_refCount ? -1 : (x.m_refCount > y.m_refCount ? 1 : 0);
			}
		}
		class	CompareRefCounts_Descending : IComparer< TextureFileInfo > {
			public int Compare(TextureFileInfo x, TextureFileInfo y) {
				return x.m_refCount < y.m_refCount ? 1 : (x.m_refCount > y.m_refCount ? -1 : 0);
			}
		}

// 		public int Compare( TextureFileInfo x, TextureFileInfo y ) {
// 			int	value = 0;
// 			switch ( m_texturesSortColumn ) {
// 				case 0: value = StringComparer.CurrentCultureIgnoreCase.Compare( x.m_fileName.FullName, y.m_fileName.FullName ); break;	// Sort by name
// 
// 				case 1:	// Sort by area
// 					int	area0 = x.m_width * x.m_height;
// 					int	area1 = y.m_width * y.m_height;
// 					value = area0 < area1 ? -1 : (area0 > area1 ? 1 : 0);
// 					break;
// 
// 				case 2: value = StringComparer.CurrentCultureIgnoreCase.Compare( x.m_usage.ToString(), y.m_usage.ToString() ); break;	// Sort by usage
// 
// 				case 3: value = x.m_refCount < y.m_refCount ? -1 : (x.m_refCount > y.m_refCount ? 1 : 0); break;		// Sort by ref count
// 
// 				default: throw new Exception( "Unhandled case" );
// 			}
// 
// 			return m_texturesSortOrder * value;
// 		}

		#endregion

		private void buttonSetMaterialsBasePath_Click(object sender, EventArgs e) {
			folderBrowserDialog.SelectedPath = GetRegKey( "MaterialsBasePath", textBoxMaterialsBasePath.Text );
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}

			textBoxMaterialsBasePath.Text = folderBrowserDialog.SelectedPath;
			SetRegKey( "MaterialsBasePath", textBoxMaterialsBasePath.Text );
		}

		private void buttonSetTexturesBasePath_Click(object sender, EventArgs e) {
			folderBrowserDialog.SelectedPath = GetRegKey( "TexturesBasePath", textBoxTexturesBasePath.Text );
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}

			textBoxTexturesBasePath.Text = folderBrowserDialog.SelectedPath;
			SetRegKey( "TexturesBasePath", textBoxTexturesBasePath.Text );

			Material.Layer.Texture.ms_TexturesBasePath = new DirectoryInfo( textBoxTexturesBasePath.Text );
		}

		private void buttonParseMaterials_Click(object sender, EventArgs e)
		{
			try {
				RecurseParseMaterials( new DirectoryInfo( textBoxMaterialsBasePath.Text ) );
				SaveMaterialsDatabase( m_materialsDatabaseFileName );
				AnalyzeMaterialsDatabase( true );
				RebuildMaterialsListView();
				RebuildTexturesListView();
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while parsing materials:\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void buttonCollectTextures_Click(object sender, EventArgs e) {
			if ( MessageBox( "Collecting textures can take some serious time as they're read to determine their sizes!\r\nAre you sure you wish to proceed?", MessageBoxButtons.YesNo, MessageBoxIcon.Question ) != DialogResult.Yes ) {
				return;
			}

			try {
				CollectTextures( new DirectoryInfo( textBoxTexturesBasePath.Text ) );
				SaveTexturesDatabase( m_texturesDatabaseFileName );
				AnalyzeMaterialsDatabase( true );
				RebuildTexturesListView();
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while collecting textures:\r\n" + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void checkBoxShowOtherFormats_CheckedChanged(object sender, EventArgs e)
		{
			RebuildTexturesListView();
		}

		private void checkBoxShowPNG_CheckedChanged(object sender, EventArgs e)
		{
			RebuildTexturesListView();
		}

		private void checkBoxShowTGA_CheckedChanged(object sender, EventArgs e)
		{
			RebuildTexturesListView();
		}

		private void listViewTextures_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if ( e.Column == m_texturesSortColumn )
				m_texturesSortOrder *= -1;
			else
				m_texturesSortOrder = 1;
			m_texturesSortColumn = e.Column;

			RebuildTexturesListView();
		}

		private void checkBoxShowDiffuse_CheckedChanged(object sender, EventArgs e)
		{
			RebuildTexturesListView();
		}

		private void checkBoxShowArkDefault_CheckedChanged(object sender, EventArgs e)
		{
			RebuildMaterialsListView();
		}

		private void listViewMaterials_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if ( e.Column == m_materialsSortColumn )
				m_materialsSortOrder *= -1;
			else
				m_materialsSortOrder = 1;
			m_materialsSortColumn = e.Column;

			RebuildMaterialsListView();
		}

		private void integerTrackbarControlLayerMin_ValueChanged(Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue)
		{
			RebuildMaterialsListView();
		}

		private void integerTrackbarControlLayerMax_ValueChanged(Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue)
		{
			RebuildMaterialsListView();
		}
	}
}
