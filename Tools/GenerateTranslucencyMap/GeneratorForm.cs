//#define BISOU

//////////////////////////////////////////////////////////////////////////
// Implements the Directional Translucency Map from Habel "Physically Based Real-Time Translucency for Leaves" (2007)
// Source: https://www.cg.tuwien.ac.at/research/publications/2007/Habel_2007_RTT/
// 
// The idea is to precompute 3 translucency values for 3 principal directions encoded by the HL2 basis.
// 
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

using Renderer;
using SharpMath;

namespace GenerateTranslucencyMap
{
	public partial class GeneratorForm : Form {
		#region CONSTANTS

//		private const int		MAX_THREADS = 1024;			// Maximum threads run by the compute shader

		private const int		BILATERAL_PROGRESS = 25;	// Bilateral filtering is considered as this % of the total task (bilateral is quite long so I decided it was equivalent to 50% of the complete computation task)
		private const int		VISIBILITY_PROGRESS = 50;	// Visibility computation is considered as this % of the total task
		private const int		MAX_LINES = 16;				// Process at most that amount of lines of a 4096x4096 image for a single dispatch

		private const int		VISIBILITY_SLICES = 32;		// Maximum amount of horizon lines collected in the visibility texture

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBGenerate {
			public uint		_Width;
			public uint		_Height;
			public float	_TexelSize_mm;		// Texel size in millimeters
			public float	_Thickness_mm;		// Thickness map max encoded displacement in millimeters
			public uint		_KernelSize;		// Size of the convolution kernel
			public float	_Sigma_a;			// Absorption coefficient (mm^-1)
			public float	_Sigma_s;			// Scattering coefficient (mm^-1)
			public float	_g;					// Scattering anisotropy (mean cosine of scattering phase function)
			public float3	_Light;				// Light direction (in tangent space)
			public float	_F0;				// Fresnel reflection coefficient at normal angle
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBVisibility {
			public uint		_Width;
			public uint		_Height;
			public uint		_Y0;
			public float	_TexelSize_mm;		// Texel size in millimeters
			public float	_Thickness_mm;		// Thickness map max encoded displacement in millimeters
			public uint		_DiscRadius;		// Radius of the disc where the horizon will get sampled
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBFilter {
			public UInt32	_Y0;				// Index of the texture line we're processing
			public float	_Radius;			// Radius of the bilateral filter
			public float	_Tolerance;			// Range tolerance of the bilateral filter
			public UInt32	_Tile;				// Tiling flag
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBHelper {
			public uint		_Width;
			public uint		_Height;
			float			__PAD0;
			float			__PAD1;
			public float3	_Parms;
		}

		#endregion

		#region FIELDS

		private RegistryKey						m_AppKey;
		private string							m_ApplicationPath;

		private ViewerForm						m_viewerForm;

		private System.IO.FileInfo				m_SourceFileName = null;
		private uint							W, H;
		private ImageUtility.ImageFile			m_ImageSourceThickness = null;
		private ImageUtility.ImageFile			m_imageSourceNormal = null;
		private ImageUtility.ImageFile			m_imageSourceAlbedo = null;
		private ImageUtility.ImageFile			m_imageSourceTransmittance = null;

		internal Device							m_Device = new Device();
		internal Texture2D						m_TextureSourceThickness = null;
		internal Texture3D						m_TextureSourceVisibility = null;	// Procedurally generated from thickness
		internal Texture2D						m_TextureSourceNormal = null;
		internal Texture2D						m_TextureSourceAlbedo = null;
		internal Texture2D						m_TextureSourceTransmittance = null;
		internal Texture2D						m_TextureFilteredThickness = null;

		internal Texture2D[][]					m_TextureTargets = new Texture2D[3][] {
			new Texture2D[2],
			new Texture2D[2],
			new Texture2D[2],
		};
		internal Texture2D						m_TextureTargetCombined = null;

		internal Texture2D						m_TextureTarget_CPU = null;

		// The bunch of rays to shoot
		private float3[][]						m_rays = new float3[3][];

		// Directional Translucency Map Generation
		private ConstantBuffer<CBGenerate>		m_CB_Generate;
		private ComputeShader					m_CS_GenerateTranslucencyMap = null;

		// Visibility map generation
		private ConstantBuffer<CBVisibility>	m_CB_Visibility;
		private ComputeShader					m_CS_GenerateVisibilityMap = null;

		// Bilateral filtering pre-processing
		private ConstantBuffer<CBFilter>		m_CB_Filter;
		private ComputeShader					m_CS_BilateralFilter = null;

		// Helpers
		private ConstantBuffer<CBHelper>		m_CB_Helper;
		private ComputeShader					m_CS_Helper_Normalize = null;
		private ComputeShader					m_CS_Helper_Mix = null;

		private ImageUtility.ColorProfile		m_LinearProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR );
		private ImageUtility.ColorProfile		m_sRGBProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.ImageFile[]		m_imageResults = new ImageUtility.ImageFile[3];
		private ImageUtility.ImageFile			m_imageResultCombined = null;

		#endregion

		#region PROPERTIES

		internal float	Thickness_mm {
			get { return floatTrackbarControlThickness.Value; }
		}

		internal float	TextureSize_mm {
			get { return 10.0f * floatTrackbarControlPixelDensity.Value; }
		}

		internal float3	MixColor {
			get { return new float3( panelDominantHue.BackColor.R / 255.0f, panelDominantHue.BackColor.G / 255.0f, panelDominantHue.BackColor.B / 255.0f ); }
		}

		internal float	IOR {
			get { return floatTrackbarControlRefractionIndex.Value; }
		}

		// Cheap extinction coeff for rendering
		internal float	Sigma_t {
			get { return floatTrackbarControlScatteringCoefficient.Value + floatTrackbarControlAbsorptionCoefficient.Value; }
		}

		#endregion

		#region METHODS

		public GeneratorForm() {
			InitializeComponent();

			m_viewerForm = new ViewerForm( this );

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\TranslucencyMapGenerator" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

			// Show dominant hue
			floatTrackbarControlDominantHue_ValueChanged( floatTrackbarControlDominantHue, 0.0f );

			#if DEBUG
			integerTrackbarControlKernelSize.Visible = true;
			label5.Visible = true;
			buttonReload.Visible = true;
			#endif
		}

		protected override void  OnLoad(EventArgs e) {
 			base.OnLoad(e);

			try {
				m_Device.Init( m_viewerForm.Handle, false, true );

				m_viewerForm.Init();

				// Create our compute shaders
				#if !DEBUG || BISOU
					using ( ScopedForceShadersLoadFromBinary scope = new ScopedForceShadersLoadFromBinary() )
				#endif
				{
					m_CS_BilateralFilter = new ComputeShader( m_Device, new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ), "CS", null );
					m_CS_GenerateVisibilityMap = new ComputeShader( m_Device, new System.IO.FileInfo( "./Shaders/GenerateVisibilityMap.hlsl" ), "CS", null );
					m_CS_GenerateTranslucencyMap = new ComputeShader( m_Device, new System.IO.FileInfo( "./Shaders/GenerateTranslucencyMap.hlsl" ), "CS", null );
					m_CS_Helper_Normalize = new ComputeShader( m_Device, new System.IO.FileInfo( "./Shaders/Helpers.hlsl" ), "CS_Finalize", null );
					m_CS_Helper_Mix = new ComputeShader( m_Device, new System.IO.FileInfo( "./Shaders/Helpers.hlsl" ), "CS_Mix", null );
				}
// 				m_CS_BilateralFilter = new ComputeShader( m_Device, new ShaderBinaryFile( new System.IO.FileInfo( "./Shaders/Binary/BilateralFiltering.fxbin" ) ), "CS" );
// 				m_CS_GenerateVisibilityMap = new ComputeShader( m_Device, new ShaderBinaryFile( new System.IO.FileInfo( "./Shaders/Binary/GenerateVisibilityMap.fxbin" ) ), "CS" );
// 				m_CS_GenerateTranslucencyMap = new ComputeShader( m_Device, new ShaderBinaryFile( new System.IO.FileInfo( "./Shaders/Binary/GenerateTranslucencyMap.fxbin" ) ), "CS" );
// 				m_CS_Helper_Normalize = new ComputeShader( m_Device, new ShaderBinaryFile( new System.IO.FileInfo( "./Shaders/Helpers.hlsl" ) ), "CS_Finalize" );
// 				m_CS_Helper_Mix = new ComputeShader( m_Device, new ShaderBinaryFile( new System.IO.FileInfo( "./Shaders/Helpers.hlsl" ) ), "CS_Mix" );

				// Create our constant buffers
				m_CB_Generate = new ConstantBuffer<CBGenerate>( m_Device, 0 );
				m_CB_Visibility = new ConstantBuffer<CBVisibility>( m_Device, 0 );
				m_CB_Filter = new ConstantBuffer<CBFilter>( m_Device, 0 );
				m_CB_Helper = new ConstantBuffer<CBHelper>( m_Device, 0 );

				// Create the rays
				integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

			} catch ( Exception _e ) {
				MessageBox( "Failed to create DX11 device and default shaders:\r\n", _e );
				Close();
			}


// LoadThicknessMap( new System.IO.FileInfo( "Leaf_thickness.tga" ) );
// LoadNormalMap( new System.IO.FileInfo( "Leaf_normal.tga" ) );
// LoadAlbedoMap( new System.IO.FileInfo( "Leaf_albedo.tga" ) );
// LoadTransmittanceMap( new System.IO.FileInfo( "Leaf_transmittance.tga" ) );

//LoadResults( new System.IO.FileInfo( "Leaf_thickness.tga" ) );

		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			try {
				m_viewerForm.Exit();
				m_viewerForm.Dispose();

				m_CS_Helper_Normalize.Dispose();
				m_CS_Helper_Mix.Dispose();
				m_CS_GenerateTranslucencyMap.Dispose();
				m_CS_GenerateVisibilityMap.Dispose();
				m_CS_BilateralFilter.Dispose();

				m_CB_Helper.Dispose();
				m_CB_Filter.Dispose();
				m_CB_Visibility.Dispose();
				m_CB_Generate.Dispose();

				if ( m_TextureTarget_CPU != null ) {
					m_TextureTarget_CPU.Dispose();
				}
				if ( m_TextureTargets[0][0] != null ) {
					m_TextureTargets[0][0].Dispose();
					m_TextureTargets[0][1].Dispose();
					m_TextureTargets[1][0].Dispose();
					m_TextureTargets[1][1].Dispose();
					m_TextureTargets[2][0].Dispose();
					m_TextureTargets[2][1].Dispose();
				}
				if ( m_TextureTargetCombined != null )
					m_TextureTargetCombined.Dispose();
				if ( m_TextureFilteredThickness != null )
					m_TextureFilteredThickness.Dispose();
				if ( m_TextureSourceThickness != null )
					m_TextureSourceThickness.Dispose();
				if ( m_TextureSourceNormal != null )
					m_TextureSourceNormal.Dispose();
				if ( m_TextureSourceTransmittance != null )
					m_TextureSourceTransmittance.Dispose();
				if ( m_TextureSourceAlbedo != null )
					m_TextureSourceAlbedo.Dispose();
				if ( m_TextureSourceVisibility != null )
					m_TextureSourceVisibility.Dispose();

				m_Device.Dispose();
			} catch ( Exception ) {
			}

			e.Cancel = false;
			base.OnFormClosing( e );
		}

		#region Images Loading

		private void	LoadThicknessMap( System.IO.FileInfo _FileName ) {
			try {
				groupBoxOptions.Enabled = false;

				// Dispose of existing resources
				if ( m_ImageSourceThickness != null )
					m_ImageSourceThickness.Dispose();
				m_ImageSourceThickness = null;
				if ( m_TextureSourceThickness != null )
					m_TextureSourceThickness.Dispose();
				m_TextureSourceThickness = null;

				if ( m_TextureSourceVisibility != null )
					m_TextureSourceVisibility.Dispose();
				m_TextureSourceVisibility = null;

				if ( m_TextureTarget_CPU != null ) {
					m_TextureTarget_CPU.Dispose();
				}
				m_TextureTarget_CPU = null;

				if ( m_TextureFilteredThickness != null )
					m_TextureFilteredThickness.Dispose();
				m_TextureFilteredThickness = null;

				if ( m_TextureTargets[0][0] != null ) {
					m_TextureTargets[0][0].Dispose();
					m_TextureTargets[0][1].Dispose();
					m_TextureTargets[1][0].Dispose();
					m_TextureTargets[1][1].Dispose();
					m_TextureTargets[2][0].Dispose();
					m_TextureTargets[2][1].Dispose();
				}
				m_TextureTargets[0][0] = null;
				m_TextureTargets[0][1] = null;
				m_TextureTargets[1][0] = null;
				m_TextureTargets[1][1] = null;
				m_TextureTargets[2][0] = null;
				m_TextureTargets[2][1] = null;

				if ( m_TextureTargetCombined != null )
					m_TextureTargetCombined.Dispose();
				m_TextureTargetCombined = null;

				// Load the source image
				m_SourceFileName = _FileName;
				m_ImageSourceThickness = new ImageUtility.ImageFile( _FileName );
				imagePanelThicknessMap.Image = m_ImageSourceThickness;

				W = m_ImageSourceThickness.Width;
				H = m_ImageSourceThickness.Height;

				// Build the source texture assuming the image is in linear space
				float4[]	scanline = new float4[W];

				PixelsBuffer	sourceHeightMap = new PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = sourceHeightMap.OpenStreamWrite() )
					for ( uint Y=0; Y < H; Y++ ) {
						m_ImageSourceThickness.ReadScanline( Y, scanline );
						for ( uint X=0; X < W; X++ ) {
							Wr.Write( scanline[X].x );
						}
					}

				m_TextureSourceThickness = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { sourceHeightMap } );

				// Build the 3D visibility texture
				m_TextureSourceVisibility = new Texture3D( m_Device, W, H, VISIBILITY_SLICES, 1, ImageUtility.PIXEL_FORMAT.R16F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );

				// Build the target UAV & staging texture for readback
				m_TextureFilteredThickness = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );

				for ( int i=0; i < 3; i++ ) {
					m_TextureTargets[i][0] = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
					m_TextureTargets[i][1] = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
				}
				m_TextureTargetCombined = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
				m_TextureTarget_CPU = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, false, null );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the thickness map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadNormalMap( System.IO.FileInfo _FileName ) {
			try {
				// Dispose of existing resources
				if ( m_imageSourceNormal != null )
					m_imageSourceNormal.Dispose();
				m_imageSourceNormal = null;
				if ( m_TextureSourceNormal != null )
					m_TextureSourceNormal.Dispose();
				m_TextureSourceNormal = null;

				// Load the source image
				m_imageSourceNormal = new ImageUtility.ImageFile( _FileName );
				imagePanelNormalMap.Image = m_imageSourceNormal;

				uint	W = m_imageSourceNormal.Width;
				uint	H = m_imageSourceNormal.Height;

				// Build the source texture assuming the image is in linear space
				float4[]	scanline = new float4[W];

				PixelsBuffer	sourceNormalMap = new PixelsBuffer( W*H*16 );
				using ( System.IO.BinaryWriter Wr = sourceNormalMap.OpenStreamWrite() )
					for ( uint Y=0; Y < H; Y++ ) {
						m_imageSourceNormal.ReadScanline( Y, scanline );
						for ( uint X=0; X < W; X++ ) {
							Wr.Write( scanline[X].x );
							Wr.Write( scanline[X].y );
							Wr.Write( scanline[X].z );
							Wr.Write( 1.0f );
						}
					}

				m_TextureSourceNormal = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { sourceNormalMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the normal map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadTransmittanceMap( System.IO.FileInfo _FileName ) {
			try {
				// Dispose of existing resources
				if ( m_imageSourceTransmittance != null )
					m_imageSourceTransmittance.Dispose();
				m_imageSourceTransmittance = null;
				if ( m_TextureSourceTransmittance != null )
					m_TextureSourceTransmittance.Dispose();
				m_TextureSourceTransmittance = null;

				// Load the source image
				m_imageSourceTransmittance = new ImageUtility.ImageFile( _FileName );
				imagePanelTransmittanceMap.Image = m_imageSourceTransmittance;

				uint	W = m_imageSourceTransmittance.Width;
				uint	H = m_imageSourceTransmittance.Height;

				// Build the source texture assuming the image's color profile
				float4[]	scanline = new float4[W];
				float4		linearRGB = float4.Zero;
				ImageUtility.ColorProfile	imageProfile = m_imageSourceTransmittance.ColorProfile;
//				ImageUtility.ColorProfile	imageProfile = m_sRGBProfile;

// 				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
// 				m_LinearProfile.XYZ2RGB( m_imageSourceTransmittance.ContentXYZ, ContentRGB );

				PixelsBuffer	SourceMap = new PixelsBuffer( W*H*16 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( uint Y=0; Y < H; Y++ ) {
						m_imageSourceTransmittance.ReadScanline( Y, scanline );
						for ( int X=0; X < W; X++ ) {
							imageProfile.GammaRGB2LinearRGB( scanline[X], ref linearRGB );
							Wr.Write( linearRGB.x );
							Wr.Write( linearRGB.y );
							Wr.Write( linearRGB.z );
							Wr.Write( linearRGB.w );
						}
					}

				m_TextureSourceTransmittance = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { SourceMap } );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the transmittance map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadAlbedoMap( System.IO.FileInfo _FileName ) {
			try {
				// Dispose of existing resources
				if ( m_imageSourceAlbedo != null )
					m_imageSourceAlbedo.Dispose();
				m_imageSourceAlbedo = null;
				if ( m_TextureSourceAlbedo != null )
					m_TextureSourceAlbedo.Dispose();
				m_TextureSourceAlbedo = null;

				// Load the source image
				m_imageSourceAlbedo = new ImageUtility.ImageFile( _FileName );
				imagePanelAlbedoMap.Image = m_imageSourceAlbedo;

				uint	W = m_imageSourceAlbedo.Width;
				uint	H = m_imageSourceAlbedo.Height;

				// Build the source texture assuming the image's color profile
				float4[]	scanline = new float4[W];
				float4		linearRGB = float4.Zero;
				ImageUtility.ColorProfile	imageProfile = m_imageSourceAlbedo.ColorProfile;
//				ImageUtility.ColorProfile	imageProfile = m_sRGBProfile;

// 				float4[,]	ContentRGB = new float4[W,H];
// 				m_LinearProfile.XYZ2RGB( m_imageSourceAlbedo.ContentXYZ, ContentRGB );

				PixelsBuffer	SourceMap = new PixelsBuffer( W*H*16 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( uint Y=0; Y < H; Y++ ) {
						m_imageSourceAlbedo.ReadScanline( Y, scanline );
						for ( uint X=0; X < W; X++ ) {
							imageProfile.GammaRGB2LinearRGB( scanline[X], ref linearRGB );
							Wr.Write( linearRGB.x );
							Wr.Write( linearRGB.y );
							Wr.Write( linearRGB.z );
							Wr.Write( linearRGB.w );
						}
					}

				m_TextureSourceAlbedo = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the albedo map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadResults( System.IO.FileInfo _FileName ) {
			try {
				groupBoxOptions.Enabled = false;

				// Dispose of existing resources
				if ( m_TextureTargets[0][0] != null ) {
					m_TextureTargets[0][0].Dispose();
					m_TextureTargets[0][1].Dispose();
					m_TextureTargets[1][0].Dispose();
					m_TextureTargets[1][1].Dispose();
					m_TextureTargets[2][0].Dispose();
					m_TextureTargets[2][1].Dispose();
					m_TextureTargetCombined.Dispose();
				}
				m_TextureTargets[0][0] = null;
				m_TextureTargets[0][1] = null;
				m_TextureTargets[1][0] = null;
				m_TextureTargets[1][1] = null;
				m_TextureTargets[2][0] = null;
				m_TextureTargets[2][1] = null;
				m_TextureTargetCombined = null;

				if ( m_imageResults[0] != null )
					m_imageResults[0].Dispose();
				if ( m_imageResults[1] != null )
					m_imageResults[1].Dispose();
				if ( m_imageResults[2] != null )
					m_imageResults[2].Dispose();
				if ( m_imageResultCombined != null )
					m_imageResultCombined.Dispose();
//				m_imageResults[0] = new ImageUtility.ImageFile( W, H, ImageUtility.ImageFile.PIXEL_FORMAT.R8, m_sRGBProfile );
//				m_imageResults[1] = new ImageUtility.ImageFile( W, H, ImageUtility.ImageFile.PIXEL_FORMAT.R8, m_sRGBProfile );
//				m_imageResults[2] = new ImageUtility.ImageFile( W, H, ImageUtility.ImageFile.PIXEL_FORMAT.R8, m_sRGBProfile );
//				m_imageResultCombined = new ImageUtility.ImageFile( W, H, ImageUtility.ImageFile.PIXEL_FORMAT.RGBA8, m_sRGBProfile );
				m_imageResults[0] = new ImageUtility.ImageFile();
				m_imageResults[1] = new ImageUtility.ImageFile();
				m_imageResults[2] = new ImageUtility.ImageFile();
				m_imageResultCombined = new ImageUtility.ImageFile();

				// Load the result images assuming it's in sRGB space
				string[]		FileNames = new string[4] {
					System.IO.Path.Combine( System.IO.Path.GetDirectoryName( _FileName.FullName ), System.IO.Path.GetFileNameWithoutExtension( _FileName.FullName ) + "_translucency0.png" ),
					System.IO.Path.Combine( System.IO.Path.GetDirectoryName( _FileName.FullName ), System.IO.Path.GetFileNameWithoutExtension( _FileName.FullName ) + "_translucency1.png" ),
					System.IO.Path.Combine( System.IO.Path.GetDirectoryName( _FileName.FullName ), System.IO.Path.GetFileNameWithoutExtension( _FileName.FullName ) + "_translucency2.png" ),
					System.IO.Path.Combine( System.IO.Path.GetDirectoryName( _FileName.FullName ), System.IO.Path.GetFileNameWithoutExtension( _FileName.FullName ) + "_translucency.png" ),
				};
				ImageUtility.ImageFile[]	images = new ImageUtility.ImageFile[] {
					m_imageResults[0],
					m_imageResults[1],
					m_imageResults[2],
					m_imageResultCombined
				};
				Texture2D[]		Results = new Texture2D[] {
					m_TextureTargets[0][0],
					m_TextureTargets[1][0],
					m_TextureTargets[2][0],
					m_TextureTargetCombined,
				};
				ImagePanel[]	Panels = new ImagePanel[] {
					imagePanelResult0,
					imagePanelResult1,
					imagePanelResult2,
					imagePanelResult3,
				};

				for ( int i=0; i < 4; i++ ) {
					ImageUtility.ImageFile	B = images[i];
					B.Load( new System.IO.FileInfo( FileNames[i] ) );

					Panels[i].Image = B;

					// Build the texture assuming sRGB sources
					float4[]	scanline = new float4[B.Width];
					float4		linearRGB = float4.Zero;

					PixelsBuffer	sourceMap = new PixelsBuffer( B.Width*B.Height*16 );
					using ( System.IO.BinaryWriter Wr = sourceMap.OpenStreamWrite() )
						for ( uint Y=0; Y < B.Height; Y++ ) {
							B.ReadScanline( Y, scanline );
							for ( uint X=0; X < B.Width; X++ ) {
								m_sRGBProfile.GammaRGB2LinearRGB( scanline[X], ref linearRGB );
								Wr.Write( linearRGB.x );
								Wr.Write( linearRGB.y );
								Wr.Write( linearRGB.z );
								Wr.Write( linearRGB.w );
							}
						}

					Results[i] = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, new PixelsBuffer[] { sourceMap } );
				}

				m_TextureTargets[0][0] = Results[0];
				m_TextureTargets[0][1] = Results[0];
				m_TextureTargets[1][0] = Results[1];
				m_TextureTargets[1][1] = Results[1];
				m_TextureTargets[2][0] = Results[2];
				m_TextureTargets[2][1] = Results[2];
				m_TextureTargetCombined = Results[3];
			}
			catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the result maps \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		#endregion

		private void	Generate() {
			try {
				groupBoxOptions.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 0] Assign empty textures
				if ( m_imageSourceNormal == null )
					LoadNormalMap( new System.IO.FileInfo( "default_normal.png" ) );
				if ( m_imageSourceTransmittance == null )
					LoadTransmittanceMap( new System.IO.FileInfo( "default_transmittance.png" ) );
				if ( m_imageSourceAlbedo == null )
					LoadAlbedoMap( new System.IO.FileInfo( "default_albedo.png" ) );

				m_TextureTargets[0][0].RemoveFromLastAssignedSlots();
				m_TextureTargets[0][1].RemoveFromLastAssignedSlots();
				m_TextureTargets[1][0].RemoveFromLastAssignedSlots();
				m_TextureTargets[1][1].RemoveFromLastAssignedSlots();
				m_TextureTargets[2][0].RemoveFromLastAssignedSlots();
				m_TextureTargets[2][1].RemoveFromLastAssignedSlots();


				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_TextureSourceThickness, m_TextureFilteredThickness, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, false );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute visibility texture
				BuildVisibilityMap( m_TextureFilteredThickness, m_TextureSourceVisibility );
//BuildVisibilityMap( m_TextureSourceThickness, m_TextureSourceVisibility );	// While we're not using bilateral filtering...


//*				//////////////////////////////////////////////////////////////////////////
				// 3] Compute directional occlusion
				if ( !m_CS_GenerateTranslucencyMap.Use() )
					throw new Exception( "Can't generate translucency map as compute shader failed to compile!" );

				// Prepare computation parameters
				m_CB_Generate.m._Width = (uint) W;
				m_CB_Generate.m._Height = (uint) H;
				m_CB_Generate.m._TexelSize_mm = TextureSize_mm / Math.Max( W, H );
				m_CB_Generate.m._Thickness_mm = Thickness_mm;
				m_CB_Generate.m._KernelSize = (uint) integerTrackbarControlKernelSize.Value;
				m_CB_Generate.m._Sigma_a = floatTrackbarControlAbsorptionCoefficient.Value;
				m_CB_Generate.m._Sigma_s = floatTrackbarControlScatteringCoefficient.Value;
				m_CB_Generate.m._g = floatTrackbarControlScatteringAnisotropy.Value;

				float	IOR = floatTrackbarControlRefractionIndex.Value;
				m_CB_Generate.m._F0 = (IOR - 1.0f) / (IOR + 1.0f);
				m_CB_Generate.m._F0 *= m_CB_Generate.m._F0;

				// Assign inputs
				m_TextureFilteredThickness.SetCS( 0 );
//m_TextureSourceThickness.SetCS( 0 );	// While we're not using bilateral filtering...
				m_TextureSourceNormal.SetCS( 1 );
				m_TextureSourceTransmittance.SetCS( 2 );
				m_TextureSourceAlbedo.SetCS( 3 );
				m_TextureSourceVisibility.SetCS( 4 );

				uint	groupsCountX = (W + 15) >> 4;
				uint	groupsCountY = (H + 15) >> 4;

				uint	raysCount = (uint) integerTrackbarControlRaysCount.Value;
				uint	updateCountMax = Math.Max( 1, raysCount / 100 );
				uint	updateCount = 0;

				// For each HL2 basis direction
				for ( int i=0; i < 3; i++ ) {

// 					switch ( i ) {
// 						case 0: m_CB_Generate.m._Light = new float3( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ); break;
// 						case 1: m_CB_Generate.m._Light = new float3( (float) -Math.Sqrt( 1.0 / 6.0 ), (float)  Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ); break;
// 						case 2: m_CB_Generate.m._Light = new float3( (float) -Math.Sqrt( 1.0 / 6.0 ), (float) -Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ); break;
// 					}

					// Clear initial target
					m_Device.Clear( m_TextureTargets[i][0], float4.Zero );

					// Start
					for ( uint rayIndex=0; rayIndex < raysCount; rayIndex++ ) {

						m_CB_Generate.m._Light = m_rays[i][rayIndex];
//m_CB_Generate.m._Light = float3.UnitZ;
						m_CB_Generate.UpdateData();

						m_TextureTargets[i][0].SetCS( 5 );
						m_TextureTargets[i][1].SetCSUAV( 0 );

	 					m_CS_GenerateTranslucencyMap.Dispatch( groupsCountX, groupsCountY, 1 );

						m_TextureTargets[i][0].RemoveFromLastAssignedSlots();
						m_TextureTargets[i][1].RemoveFromLastAssignedSlotUAV();

						// Swap targets
						Texture2D	Temp = m_TextureTargets[i][0];
						m_TextureTargets[i][0] = m_TextureTargets[i][1];
						m_TextureTargets[i][1] = Temp;

						// Progress
						if ( updateCount++ >= updateCountMax ) {
							updateCount = 0;

							m_Device.Present( true );

							float	progress = (float) (raysCount*i+1) / (3*raysCount);
							progressBar.Value = (int) (0.01f * (VISIBILITY_PROGRESS + (100-VISIBILITY_PROGRESS) * progress) * progressBar.Maximum);
							Application.DoEvents();
						}
					}
				}
 
 				progressBar.Value = progressBar.Maximum;

				//////////////////////////////////////////////////////////////////////////
				// 3] Normalize results
				if ( !m_CS_Helper_Normalize.Use() )
					throw new Exception( "Can't normalize translucency map as normalization compute shader failed to compile!" );

				for ( int i=0; i < 3; i++ ) {

					m_CB_Helper.m._Width = (uint) W;
					m_CB_Helper.m._Height = (uint) H;
					m_CB_Helper.m._Parms = (1.0f / raysCount) * float3.One;
					m_CB_Helper.UpdateData();

					m_TextureTargets[i][0].SetCS( 0 );
					m_TextureTargets[i][1].SetCSUAV( 0 );

					m_CS_Helper_Normalize.Dispatch( groupsCountX, groupsCountY, 1 );

					m_TextureTargets[i][0].RemoveFromLastAssignedSlots();
					m_TextureTargets[i][1].RemoveFromLastAssignedSlotUAV();

					// Swap targets
					Texture2D	Temp = m_TextureTargets[i][0];
					m_TextureTargets[i][0] = m_TextureTargets[i][1];
					m_TextureTargets[i][1] = Temp;
				}
//*/

				//////////////////////////////////////////////////////////////////////////
				// 4] Copy target to staging for CPU readback and update the resulting bitmaps
				ImagePanel[]	ImagePanels = new ImagePanel[3] {
					imagePanelResult0,
					imagePanelResult1,
					imagePanelResult2,
				};
				for ( int i=0; i < 3; i++ ) {
					if ( m_imageResults[i] != null )
						m_imageResults[i].Dispose();
//					m_BitmapResults[i] = new ImageUtility.Bitmap( W, H, m_LinearProfile );
					m_imageResults[i] = new ImageUtility.ImageFile( W, H, ImageUtility.PIXEL_FORMAT.R8, m_sRGBProfile );

					// Copy from GPU to CPU
					m_TextureTarget_CPU.CopyFrom( m_TextureTargets[i][0] );

					float4[]		scanline = new float4[W];
					float4			gammaRGB = float4.Zero;
					PixelsBuffer	pixels = m_TextureTarget_CPU.MapRead( 0, 0 );
					using ( System.IO.BinaryReader R = pixels.OpenStreamRead() )
						for ( uint Y=0; Y < H; Y++ ) {
							R.BaseStream.Position = Y * pixels.RowPitch;
							for ( uint X=0; X < W; X++ ) {
								gammaRGB.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								m_sRGBProfile.LinearRGB2GammaRGB( gammaRGB, ref scanline[X] );
							}
							m_imageResults[i].WriteScanline( Y, scanline );
						}

					m_TextureTarget_CPU.UnMap( pixels );

					// Assign result
					ImagePanels[i].Image = m_imageResults[i];
				}

				//////////////////////////////////////////////////////////////////////////
				// 5] Mix results
				MixResults( MixColor );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				groupBoxOptions.Enabled = true;
			}
		}

		private void	BuildVisibilityMap( Texture2D _Source, Texture3D _Target ) {
			if ( !m_CS_GenerateVisibilityMap.Use() )
				throw new Exception( "Can't generate translucency map as visibility map compute shader failed to compile!" );

			_Target.RemoveFromLastAssignedSlots();

			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

			m_CB_Visibility.m._Width = (uint) W;
			m_CB_Visibility.m._Height = (uint) H;
			m_CB_Visibility.m._Thickness_mm = Thickness_mm;
			m_CB_Visibility.m._TexelSize_mm = TextureSize_mm / Math.Max( W, H );
			m_CB_Visibility.m._DiscRadius = 32;

			uint	h = Math.Max( 1, MAX_LINES*1024 / W );
			uint	callsCount = (uint) Math.Ceiling( (float) H / h );
			for ( uint i=0; i < callsCount; i++ ) {
				m_CB_Visibility.m._Y0 = i * h;
				m_CB_Visibility.UpdateData();

				m_CS_GenerateVisibilityMap.Dispatch( W, h, 1 );

				m_Device.Present( true );

				progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (VISIBILITY_PROGRESS - BILATERAL_PROGRESS) * (i+1) / callsCount) * progressBar.Maximum);
				Application.DoEvents();
			}

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	ApplyBilateralFiltering( Texture2D _Source, Texture2D _Target, float _BilateralRadius, float _BilateralTolerance, bool _Wrap ) {
			if ( !m_CS_BilateralFilter.Use() )
				throw new Exception( "Can't generate translucency map as bilateral filter compute shader failed to compile!" );

			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

			m_CB_Filter.m._Radius = _BilateralRadius;
			m_CB_Filter.m._Tolerance = _BilateralTolerance;
			m_CB_Filter.m._Tile = (uint) (_Wrap ? 1 : 0);

			uint	h = Math.Max( 1, MAX_LINES*1024 / W );
			uint	callsCount = (uint) Math.Ceiling( (float) H / h );
			for ( uint i=0; i < callsCount; i++ ) {
				m_CB_Filter.m._Y0 = i * h;
				m_CB_Filter.UpdateData();

				m_CS_BilateralFilter.Dispatch( W, h, 1 );

				m_Device.Present( true );

				progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS * (i+1) / callsCount) * progressBar.Maximum);
				Application.DoEvents();
			}

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	MixResults( float3 _MixColor ) {
			if ( m_TextureTargets[0][0] == null )
				return;	// Nothing to mix yet...

			if ( !m_CS_Helper_Mix.Use() )
				throw new Exception( "Can't mix translucency maps as mixer compute shader failed to compile!" );

			//////////////////////////////////////////////////////////////////////////
			// 1] Combine
			m_CB_Helper.m._Width = (uint) W;
			m_CB_Helper.m._Height = (uint) H;
			m_CB_Helper.m._Parms = _MixColor;
			m_CB_Helper.UpdateData();

			m_TextureTargets[0][0].SetCS( 0 );
			m_TextureTargets[1][0].SetCS( 1 );
			m_TextureTargets[2][0].SetCS( 2 );
			m_TextureTargetCombined.RemoveFromLastAssignedSlots();
			m_TextureTargetCombined.SetCSUAV( 0 );

			uint	groupsCountX = (W + 15) >> 4;
			uint	groupsCountY = (H + 15) >> 4;

			m_CS_Helper_Mix.Dispatch( groupsCountX, groupsCountY, 1 );

			m_TextureTargetCombined.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage

			//////////////////////////////////////////////////////////////////////////
			// 2] Copy target to staging for CPU readback and update the resulting bitmaps
			if ( m_imageResultCombined != null )
				m_imageResultCombined.Dispose();
			m_imageResultCombined = new ImageUtility.ImageFile( W, H, ImageUtility.PIXEL_FORMAT.RGBA8, m_sRGBProfile );

			// Copy from GPU to CPU
			m_TextureTarget_CPU.CopyFrom( m_TextureTargetCombined );

			PixelsBuffer	pixels = m_TextureTarget_CPU.MapRead( 0, 0 );
			float4[]		scanline = new float4[W];
			float4			gammaRGB = float4.Zero;

			using ( System.IO.BinaryReader R = pixels.OpenStreamRead() )
				for ( uint Y=0; Y < H; Y++ ) {
					R.BaseStream.Position = Y * pixels.RowPitch;
					for ( uint X=0; X < W; X++ ) {
						gammaRGB.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_sRGBProfile.LinearRGB2GammaRGB( gammaRGB, ref scanline[X] );
					}
					m_imageResultCombined.WriteScanline( Y, scanline );
				}

			m_TextureTarget_CPU.UnMap( pixels );

			// Assign result
			imagePanelResult3.Image = m_imageResultCombined;
		}

		private void	GenerateRays( int _raysCount ) {

			m_rays[0] = new float3[_raysCount];
			m_rays[1] = new float3[_raysCount];
			m_rays[2] = new float3[_raysCount];

			// Half-Life 2 basis
			float3[]	HL2Basis = new float3[] {
				new float3( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ),
				new float3( -(float) Math.Sqrt( 1.0 / 6.0 ), (float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ),
				new float3( -(float) Math.Sqrt( 1.0 / 6.0 ), -(float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) )
			};
			float3[]	X = new float3[] {
				HL2Basis[0].Cross( float3.UnitZ ).Normalized,
				HL2Basis[1].Cross( float3.UnitZ ).Normalized,
				HL2Basis[2].Cross( float3.UnitZ ).Normalized,
			};
			float3[]	Y = new float3[] {
				X[0].Cross( HL2Basis[0] ),
				X[1].Cross( HL2Basis[1] ),
				X[2].Cross( HL2Basis[2] ),
			};

			double	randomTheta = 0.0;	// So first draw is always the HL2 direction itself
			for ( int rayIndex=0; rayIndex < _raysCount; rayIndex++ ) {
				// Stratified version
				double	phi = (Math.PI / 3.0) * (2.0 * SimpleRNG.GetUniform() - 1.0);
				double	theta = 2.0 * Math.Asin( Math.Sqrt( 0.5 * (rayIndex + randomTheta) / _raysCount ) );

				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );

				float3	dir = new float3( (float) (Math.Cos( phi ) * sinTheta),
										  (float) (Math.Sin( phi ) * sinTheta),
										  (float) cosTheta );

				m_rays[0][rayIndex] = dir.x * X[0] + dir.y * Y[0] + dir.z * HL2Basis[0];
				m_rays[1][rayIndex] = dir.x * X[1] + dir.y * Y[1] + dir.z * HL2Basis[1];
				m_rays[2][rayIndex] = dir.x * X[2] + dir.y * Y[2] + dir.z * HL2Basis[2];

				randomTheta = SimpleRNG.GetUniform();
			}
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
			return System.Windows.Forms.MessageBox.Show( this, _Text, "SSBumpMap Generator", _Buttons, _Icon );
		}

		#endregion 

		#endregion

		#region EVENT HANDLERS

 		private void buttonGenerate_Click( object sender, EventArgs e )
 		{
 			Generate();
		}

		private void integerTrackbarControlRaysCount_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
			GenerateRays( _Sender.Value );
		}

		private void imagePanelResult0_Click(object sender, EventArgs e)
		{
			if ( m_imageResults[0] == null ) {
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_translucency0.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_imageResults[0].Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
//				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult1_Click(object sender, EventArgs e)
		{
			if ( m_imageResults[1] == null ) {
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_translucency1.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_imageResults[1].Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
//				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult2_Click(object sender, EventArgs e)
		{
			if ( m_imageResults[2] == null ) {
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_translucency2.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_imageResults[2].Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
//				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult3_Click(object sender, EventArgs e)
		{
			if ( m_imageResultCombined == null ) {
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_translucency.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_imageResultCombined.Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
//				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		#region Thickness Map (Input)

		private void imagePanelInputThicknessMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "ThicknessMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "ThicknessMapFileName", openFileDialogImage.FileName );

			LoadThicknessMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private string	m_DraggedFileName = null;
		private void imagePanelInputThicknessMap_DragEnter( object sender, DragEventArgs e )
		{
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			string	Extension = System.IO.Path.GetExtension( DraggedFileName ).ToLower();
			if (	Extension == ".jpg"
				||	Extension == ".jpeg"
				||	Extension == ".png"
				||	Extension == ".tga"
				||	Extension == ".bmp"
				||	Extension == ".tif"
				||	Extension == ".tiff"
				||	Extension == ".hdr"
				||	Extension == ".crw"
				||	Extension == ".dng"
				)
			{
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void imagePanelInputThicknessMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadThicknessMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Normal Map (Input)

		private void imagePanelNormalMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "NormalMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "NormalMapFileName", openFileDialogImage.FileName );

			LoadNormalMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private void imagePanelNormalMap_DragEnter( object sender, DragEventArgs e )
		{
			imagePanelInputThicknessMap_DragEnter( sender, e );
		}

		private void imagePanelNormalMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadNormalMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Albedo Map (Input)

		private void imagePanelAlbedoMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "AlbedoMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "AlbedoMapFileName", openFileDialogImage.FileName );

			try
			{
				LoadAlbedoMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void imagePanelAlbedoMap_DragEnter( object sender, DragEventArgs e )
		{
			imagePanelInputThicknessMap_DragEnter( sender, e );
		}

		private void imagePanelAlbedoMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadAlbedoMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Transmittance Map (Input)

		private void imagePanelTransmittanceMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "TransmittanceMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "TransmittanceMapFileName", openFileDialogImage.FileName );

			try
			{
				LoadTransmittanceMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void imagePanelTransmittanceMap_DragEnter( object sender, DragEventArgs e )
		{
			imagePanelInputThicknessMap_DragEnter( sender, e );
		}

		private void imagePanelTransmittanceMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadTransmittanceMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		private void buttonShowViewer_Click( object sender, EventArgs e )
		{
			if ( m_viewerForm.Visible )
				m_viewerForm.Hide();
			else
				m_viewerForm.Show( this );
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			m_Device.ReloadModifiedShaders();
		}

		private void floatTrackbarControlDominantHue_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue )
		{
			try {
				MixResults( MixColor );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred results mixig generation!\r\n\r\nDetails: ", _e );
			}
		}

		private void floatTrackbarControlDominantHue_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	RGB = HSL2RGB( new float3( _Sender.Value / 360.0f, 1.0f, 0.5f ) );
			panelDominantHue.BackColor = Color.FromArgb( (int) (255 * RGB.x), (int) (255 * RGB.y), (int) (255 * RGB.z) );
		}

		float3	HSL2RGB( float3 _HSL ) {
			float	H = _HSL.x;
			float	S = _HSL.y;
			float	L = _HSL.z;

			if ( S == 0.0f )
			   return L * float3.One;

			float	var_2 = _HSL.z < 0.5f ? L * ( 1 + S ) :( L + S ) - ( S * L );
			float	var_1 = 2 * L - var_2;

			float	R = Hue_2_RGB( var_1, var_2, H + 1 / 3.0f ) ;
			float	G = Hue_2_RGB( var_1, var_2, H );
			float	B = Hue_2_RGB( var_1, var_2, H - 1 / 3.0f );

			return new float3( R, G, B );
		}

		float	Hue_2_RGB( float v1, float v2, float vH ) {
			if ( vH < 0 ) vH += 1;
			if ( vH > 1 ) vH -= 1;
			if ( ( 6 * vH ) < 1 ) return ( v1 + ( v2 - v1 ) * 6 * vH );
			if ( ( 2 * vH ) < 1 ) return ( v2 );
			if ( ( 3 * vH ) < 2 ) return ( v1 + ( v2 - v1 ) * ( ( 2.0f / 3 ) - vH ) * 6 );
			return v1;
		}

		#endregion
	}
}
