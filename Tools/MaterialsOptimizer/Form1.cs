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
			public FileInfo		m_materialFileName;
			public Exception	m_error;

			public override string ToString() {
				return "ERROR! " + m_materialFileName.FullName + " > " + m_error.Message;
			}
		}

		List< Material >	m_materials = new List< Material >();
		List< Error >		m_errors = new List< Error >();

		public Form1()
		{
			InitializeComponent();


			Material.Layer.Texture.ms_TexturesBasePath = new DirectoryInfo( @"V:\Dishonored2\Dishonored2\base\" );

ParseFile( new FileInfo( @"V:\Dishonored2\Dishonored2\base\decls\m2\models\environment\buildings\rich_large_ext_partitions_01.m2" ) );

			RecurseParseMaterials( new DirectoryInfo( @"V:\Dishonored2\Dishonored2\base\decls\m2" ) );
		}

		void	RecurseParseMaterials( DirectoryInfo _directory ) {

			FileInfo[]	materialFileNames = _directory.GetFiles( "*.m2", SearchOption.AllDirectories );
			foreach ( FileInfo materialFileName in materialFileNames ) {

				try {
					ParseFile( materialFileName );
				} catch ( Exception _e ) {
					Error	Err = new Error() { m_materialFileName = materialFileName, m_error = _e };
					m_errors.Add( Err );
					textBoxLog.AppendText( Err + "\n" );
				}
			}
		}

		void	ParseFile( FileInfo _fileName ) {
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

						string		materialContent = P.ReadBlock();
						Material	M = new Material( _fileName, materialName, materialContent );
						m_materials.Add( M );
						break;
				}
			}
		}
	}
}
