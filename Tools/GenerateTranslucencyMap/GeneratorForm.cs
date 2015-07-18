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

namespace GenerateTranslucencyMap
{
	public partial class GeneratorForm : Form
	{
		#region CONSTANTS

//		private const int		MAX_THREADS = 1024;			// Maximum threads run by the compute shader

		private const int		BILATERAL_PROGRESS = 25;	// Bilateral filtering is considered as this % of the total task (bilateral is quite long so I decided it was equivalent to 50% of the complete computation task)
		private const int		VISIBILITY_PROGRESS = 50;	// Visibility computation is considered as this % of the total task
		private const int		MAX_LINES = 16;				// Process at most that amount of lines of a 4096x4096 image for a single dispatch

		private const int		VISIBILITY_SLICES = 32;		// Maximum amount of horizon lines collected in the visibility texture

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBInput {
			public uint						_Width;
			public uint						_Height;
			public float					_TexelSize_mm;		// Texel size in millimeters
			public float					_Thickness_mm;		// Thickness map max encoded displacement in millimeters
			public uint						_KernelSize;		// Size of the convolution kernel
			public float					_Sigma_a;			// Absorption coefficient (mm^-1)
			public float					_Sigma_s;			// Scattering coefficient (mm^-1)
			public float					_g;					// Scattering anisotropy (mean cosine of scattering phase function)
			public RendererManaged.float3	_Light;				// Light direction (in tangent space)
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBVisibility {
			public uint						_Width;
			public uint						_Height;
			public uint						_Y0;
			public float					_TexelSize_mm;		// Texel size in millimeters
			public float					_Thickness_mm;		// Thickness map max encoded displacement in millimeters
			public uint						_DiscRadius;		// Radius of the disc where the horizon will get sampled
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBFilter {
			public UInt32	Y0;					// Index of the texture line we're processing
			public float	Radius;				// Radius of the bilateral filter
			public float	Tolerance;			// Range tolerance of the bilateral filter
			public UInt32	Tile;				// Tiling flag
		}

		#endregion

		#region FIELDS

		private RegistryKey						m_AppKey;
		private string							m_ApplicationPath;

		private ViewerForm						m_viewerForm;

		private System.IO.FileInfo				m_SourceFileName = null;
		private int								W, H;
		private ImageUtility.Bitmap				m_BitmapSourceThickness = null;
		private ImageUtility.Bitmap				m_BitmapSourceNormal = null;
		private ImageUtility.Bitmap				m_BitmapSourceAlbedo = null;
		private ImageUtility.Bitmap				m_BitmapSourceTransmittance = null;

		internal RendererManaged.Device			m_Device = new RendererManaged.Device();
		internal RendererManaged.Texture2D		m_TextureSourceThickness = null;
		internal RendererManaged.Texture3D		m_TextureSourceVisibility = null;	// Procedurally generated from thickness
		internal RendererManaged.Texture2D		m_TextureSourceNormal = null;
		internal RendererManaged.Texture2D		m_TextureSourceAlbedo = null;
		internal RendererManaged.Texture2D		m_TextureSourceTransmittance = null;
		internal RendererManaged.Texture2D		m_TextureFilteredThickness = null;

		internal RendererManaged.Texture2D[][]	m_TextureTargets = new RendererManaged.Texture2D[3][] {
			new RendererManaged.Texture2D[2],
			new RendererManaged.Texture2D[2],
			new RendererManaged.Texture2D[2],
		};

		internal RendererManaged.Texture2D		m_TextureTarget_CPU = null;

		// Directional Translucency Map Generation
		private RendererManaged.ConstantBuffer<CBInput>						m_CB_Input;
		private RendererManaged.StructuredBuffer<RendererManaged.float3>	m_SB_Rays = null;
		private RendererManaged.ComputeShader								m_CS_GenerateTranslucencyMap = null;

		// Visibility map generation
		private RendererManaged.ConstantBuffer<CBVisibility>	m_CB_Visibility;
		private RendererManaged.ComputeShader					m_CS_GenerateVisibilityMap = null;

		// Bilateral filtering pre-processing
		private RendererManaged.ConstantBuffer<CBFilter>	m_CB_Filter;
		private RendererManaged.ComputeShader				m_CS_BilateralFilter = null;

		private ImageUtility.ColorProfile		m_LinearProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR );
		private ImageUtility.ColorProfile		m_sRGBProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.Bitmap[]			m_BitmapResults = new ImageUtility.Bitmap[3];
		private ImageUtility.Bitmap				m_BitmapResultCombined = null;

		#endregion

		#region PROPERTIES

		internal float	Thickness_mm {
			get { return floatTrackbarControlThickness.Value; }
		}

		internal float	TextureSize_mm {
			get { return 10.0f * floatTrackbarControlPixelDensity.Value; }
		}

		#endregion

		#region METHODS

		public GeneratorForm()
		{
			InitializeComponent();

			m_viewerForm = new ViewerForm( this );

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\TranslucencyMapGenerator" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

			// Show dominant hue
			floatTrackbarControlDominantHue_ValueChanged( floatTrackbarControlDominantHue, 0.0f );
		}

		protected override void  OnLoad(EventArgs e)
		{
 			base.OnLoad(e);

			try {
				m_Device.Init( m_viewerForm.Handle, false, true );

				m_viewerForm.Init();

				// Create our compute shaders
				#if DEBUG && !BISOU
					m_CS_BilateralFilter = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ) ), "CS", null );
					m_CS_GenerateVisibilityMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateVisibilityMap.hlsl" ) ), "CS", null );
					m_CS_GenerateTranslucencyMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateTranslucencyMap.hlsl" ) ), "CS", null );
				#else
					m_CS_BilateralFilter = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BilateralFiltering.fxbin" ), "CS" );
					m_CS_GenerateVisibilityMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateVisibilityMap.fxbin" ), "CS" );
					m_CS_GenerateTranslucencyMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateTranslucencyMap.fxbin" ), "CS" );
				#endif

				// Create our constant buffers
				m_CB_Filter = new RendererManaged.ConstantBuffer<CBFilter>( m_Device, 0 );
				m_CB_Visibility = new RendererManaged.ConstantBuffer<CBVisibility>( m_Device, 0 );
				m_CB_Input = new RendererManaged.ConstantBuffer<CBInput>( m_Device, 0 );

				// Create our structured buffer containing the rays
//				m_SB_Rays = new RendererManaged.StructuredBuffer<RendererManaged.float3>( m_Device, 3 * MAX_THREADS, true );
				integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

//				LoadHeightMap( new System.IO.FileInfo( "eye_generic_01_disp.png" ) );
//				LoadHeightMap( new System.IO.FileInfo( "10 - Smooth.jpg" ) );

			} catch ( Exception _e ) {
				MessageBox( "Failed to create DX11 device and default shaders:\r\n", _e );
				Close();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			try {
				m_viewerForm.Dispose();

				m_CS_GenerateTranslucencyMap.Dispose();
				m_CS_GenerateVisibilityMap.Dispose();
				m_CS_BilateralFilter.Dispose();

				m_SB_Rays.Dispose();
				m_CB_Filter.Dispose();
				m_CB_Visibility.Dispose();
				m_CB_Input.Dispose();

				if ( m_TextureTarget_CPU != null ) {
					m_TextureTarget_CPU.Dispose();
				}
				if ( m_TextureTargets[0] != null ) {
					m_TextureTargets[0][0].Dispose();
					m_TextureTargets[0][1].Dispose();
					m_TextureTargets[1][0].Dispose();
					m_TextureTargets[1][1].Dispose();
					m_TextureTargets[2][0].Dispose();
					m_TextureTargets[2][1].Dispose();
				}
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

				m_viewerForm.Dispose();

				m_Device.Dispose();
			} catch ( Exception ) {
			}

			base.OnClosing( e );
		}

		#region Images Loading

		private void	LoadThicknessMap( System.IO.FileInfo _FileName ) {
			try
			{
				groupBoxOptions.Enabled = false;

				// Dispose of existing resources
				if ( m_BitmapSourceThickness != null )
					m_BitmapSourceThickness.Dispose();
				m_BitmapSourceThickness = null;
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

				// Load the source image assuming it's in linear space
				m_SourceFileName = _FileName;
				m_BitmapSourceThickness = new ImageUtility.Bitmap( _FileName, m_LinearProfile );
				imagePanelThicknessMap.Image = m_BitmapSourceThickness;

				W = m_BitmapSourceThickness.Width;
				H = m_BitmapSourceThickness.Height;

				// Build the source texture
				RendererManaged.PixelsBuffer	SourceHeightMap = new RendererManaged.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceHeightMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							Wr.Write( m_BitmapSourceThickness.ContentXYZ[X,Y].y );

				m_TextureSourceThickness = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceHeightMap } );

				// Build the 3D visibility texture
				m_TextureSourceVisibility = new RendererManaged.Texture3D( m_Device, W, H, VISIBILITY_SLICES, 1, RendererManaged.PIXEL_FORMAT.R16_FLOAT, false, true, null );

				// Build the target UAV & staging texture for readback
				m_TextureFilteredThickness = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, true, null );

				for ( int i=0; i < 3; i++ ) {
					m_TextureTargets[i][0] = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
					m_TextureTargets[i][1] = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
				}
				m_TextureTarget_CPU = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, true, false, null );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the thickness map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadNormalMap( System.IO.FileInfo _FileName ) {
			try
			{
				// Dispose of existing resources
				if ( m_BitmapSourceNormal != null )
					m_BitmapSourceNormal.Dispose();
				m_BitmapSourceNormal = null;
				if ( m_TextureSourceNormal != null )
					m_TextureSourceNormal.Dispose();
				m_TextureSourceNormal = null;

				// Load the source image assuming it's in linear space
				m_BitmapSourceNormal = new ImageUtility.Bitmap( _FileName, m_LinearProfile );
				imagePanelNormalMap.Image = m_BitmapSourceNormal;

				int	W = m_BitmapSourceNormal.Width;
				int	H = m_BitmapSourceNormal.Height;

				// Build the source texture
				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
				m_LinearProfile.XYZ2RGB( m_BitmapSourceNormal.ContentXYZ, ContentRGB );

				RendererManaged.PixelsBuffer	SourceMap = new RendererManaged.PixelsBuffer( W*H*16 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ ) {
							Wr.Write( ContentRGB[X,Y].x );
							Wr.Write( ContentRGB[X,Y].y );
							Wr.Write( ContentRGB[X,Y].z );
							Wr.Write( 1.0f );
						}

				m_TextureSourceNormal = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the normal map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadTransmittanceMap( System.IO.FileInfo _FileName ) {
			try
			{
				// Dispose of existing resources
				if ( m_BitmapSourceTransmittance != null )
					m_BitmapSourceTransmittance.Dispose();
				m_BitmapSourceTransmittance = null;
				if ( m_TextureSourceTransmittance != null )
					m_TextureSourceTransmittance.Dispose();
				m_TextureSourceTransmittance = null;

				// Load the source image assuming it's in linear space
				m_BitmapSourceTransmittance = new ImageUtility.Bitmap( _FileName, m_sRGBProfile );
				imagePanelTransmittanceMap.Image = m_BitmapSourceTransmittance;

				int	W = m_BitmapSourceTransmittance.Width;
				int	H = m_BitmapSourceTransmittance.Height;

				// Build the source texture
				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
				m_LinearProfile.XYZ2RGB( m_BitmapSourceTransmittance.ContentXYZ, ContentRGB );

				RendererManaged.PixelsBuffer	SourceMap = new RendererManaged.PixelsBuffer( W*H*16 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ ) {
							Wr.Write( ContentRGB[X,Y].x );
							Wr.Write( ContentRGB[X,Y].y );
							Wr.Write( ContentRGB[X,Y].z );
							Wr.Write( ContentRGB[X,Y].w );
						}

				m_TextureSourceTransmittance = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the transmittance map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		private void	LoadAlbedoMap( System.IO.FileInfo _FileName ) {
			try
			{
				// Dispose of existing resources
				if ( m_BitmapSourceAlbedo != null )
					m_BitmapSourceAlbedo.Dispose();
				m_BitmapSourceAlbedo = null;
				if ( m_TextureSourceAlbedo != null )
					m_TextureSourceAlbedo.Dispose();
				m_TextureSourceAlbedo = null;

				// Load the source image assuming it's in linear space
				m_BitmapSourceAlbedo = new ImageUtility.Bitmap( _FileName, m_sRGBProfile );
				imagePanelAlbedoMap.Image = m_BitmapSourceAlbedo;

				int	W = m_BitmapSourceAlbedo.Width;
				int	H = m_BitmapSourceAlbedo.Height;

				// Build the source texture
				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
				m_LinearProfile.XYZ2RGB( m_BitmapSourceAlbedo.ContentXYZ, ContentRGB );

				RendererManaged.PixelsBuffer	SourceMap = new RendererManaged.PixelsBuffer( W*H*16 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ ) {
							Wr.Write( ContentRGB[X,Y].x );
							Wr.Write( ContentRGB[X,Y].y );
							Wr.Write( ContentRGB[X,Y].z );
							Wr.Write( ContentRGB[X,Y].w );
						}

				m_TextureSourceAlbedo = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the albedo map \"" + _FileName.FullName + "\":\n\n", _e );
			}
		}

		#endregion

		private void	Generate() {
			try {
				groupBoxOptions.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 0] Assign empty textures
				if ( m_BitmapSourceNormal == null )
					LoadNormalMap( new System.IO.FileInfo( "default_normal.png" ) );
				if ( m_BitmapSourceTransmittance == null )
					LoadTransmittanceMap( new System.IO.FileInfo( "default_transmittance.png" ) );
				if ( m_BitmapSourceAlbedo == null )
					LoadAlbedoMap( new System.IO.FileInfo( "default_albedo.png" ) );


				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_TextureSourceThickness, m_TextureFilteredThickness, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, false );
				m_TextureFilteredThickness.SetCS( 0 );
//m_TextureSourceThickness.SetCS( 0 );	// While we're not using bilateral filtering...

				m_TextureSourceNormal.SetCS( 1 );
				m_TextureSourceTransmittance.SetCS( 2 );
				m_TextureSourceAlbedo.SetCS( 3 );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute visibility texture
				BuildVisibilityMap( m_TextureFilteredThickness, m_TextureSourceVisibility );
//BuildVisibilityMap( m_TextureSourceThickness, m_TextureSourceVisibility );	// While we're not using bilateral filtering...

				m_TextureSourceVisibility.SetCS( 4 );


//*				//////////////////////////////////////////////////////////////////////////
				// 3] Compute directional occlusion
				if ( !m_CS_GenerateTranslucencyMap.Use() )
					throw new Exception( "Can't generate translucency map as compute shader failed to compile!" );

				// Prepare computation parameters
//				m_SB_Rays.SetInput( 1 );

				m_CB_Input.m._Width = (uint) W;
				m_CB_Input.m._Height = (uint) H;
				m_CB_Input.m._TexelSize_mm = TextureSize_mm / Math.Max( W, H );
				m_CB_Input.m._Thickness_mm = Thickness_mm;
				m_CB_Input.m._KernelSize = 8;
				m_CB_Input.m._Sigma_a = floatTrackbarControlAbsorptionCoefficient.Value;
				m_CB_Input.m._Sigma_s = floatTrackbarControlScatteringCoefficient.Value;
				m_CB_Input.m._g = floatTrackbarControlScatteringAnisotropy.Value;

				int	groupsCountX = (W + 15) >> 4;
				int	groupsCountY = (H + 15) >> 4;

				int	RaysCount = 1;//integerTrackbarControlRaysCount.Value;

				// For each HL2 basis direction
				for ( int i=0; i < 3; i++ ) {

					switch ( i ) {
						case 0: m_CB_Input.m._Light = new RendererManaged.float3( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ); break;
						case 1: m_CB_Input.m._Light = new RendererManaged.float3( (float) -Math.Sqrt( 1.0 / 6.0 ), (float)  Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ); break;
						case 2: m_CB_Input.m._Light = new RendererManaged.float3( (float) -Math.Sqrt( 1.0 / 6.0 ), (float) -Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ); break;
					}
					m_CB_Input.UpdateData();

					// Clear initial target
					m_Device.Clear( m_TextureTargets[i][0], RendererManaged.float4.Zero );

					// Start
					for ( int RayIndex=0; RayIndex < RaysCount; RayIndex++ ) {

						m_TextureTargets[i][0].SetCS( 5 );
						m_TextureTargets[i][1].SetCSUAV( 0 );

	 					m_CS_GenerateTranslucencyMap.Dispatch( groupsCountX, groupsCountY, 1 );

						// Swap targets
						RendererManaged.Texture2D	Temp = m_TextureTargets[i][0];
						m_TextureTargets[i][0] = m_TextureTargets[i][1];
						m_TextureTargets[i][1] = Temp;
					}

					m_TextureTargets[i][0].RemoveFromLastAssignedSlotUAV();

					m_Device.Present( true );

					float	progress = (float) (RaysCount*i+1) / (3*RaysCount);

					progressBar.Value = (int) (0.01f * (VISIBILITY_PROGRESS + (100-VISIBILITY_PROGRESS) * progress) * progressBar.Maximum);
					Application.DoEvents();
				}

// 				int	CallsCount = (int) Math.Ceiling( (float) H / h );
// 				for ( int i=0; i < CallsCount; i++ )
// 				{
// 
// 					m_CS_GenerateTranslucencyMap.Dispatch( W, h, 1 );
// 
// 					m_Device.Present( true );
// 
// 					progressBar.Value = (int) (0.01f * (VISIBILITY_PROGRESS + (100-VISIBILITY_PROGRESS) * (i+1) / (CallsCount)) * progressBar.Maximum);
// //					for ( int a=0; a < 10; a++ )
// 						Application.DoEvents();
// 				}
 
 				progressBar.Value = progressBar.Maximum;

//*/

				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				ImagePanel[]	ImagePanels = new ImagePanel[3] {
					imagePanelResult0,
					imagePanelResult1,
					imagePanelResult2,
				};
				for ( int i=0; i < 3; i++ ) {
					if ( m_BitmapResults[i] != null )
						m_BitmapResults[i].Dispose();
					m_BitmapResults[i] = new ImageUtility.Bitmap( W, H, m_LinearProfile );
					m_BitmapResults[i].HasAlpha = true;

					// Copy from GPU to CPU
					m_TextureTarget_CPU.CopyFrom( m_TextureTargets[i][0] );

					RendererManaged.PixelsBuffer	Pixels = m_TextureTarget_CPU.Map( 0, 0 );
					using ( System.IO.BinaryReader R = Pixels.OpenStreamRead() )
						for ( int Y=0; Y < H; Y++ )
						{
							R.BaseStream.Position = Y * Pixels.RowPitch;
							for ( int X=0; X < W; X++ )
							{
								ImageUtility.float4	Color = new ImageUtility.float4( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Color = m_LinearProfile.RGB2XYZ( Color );
								m_BitmapResults[i].ContentXYZ[X,Y] = Color;
							}
						}

					Pixels.Dispose();
					m_TextureTarget_CPU.UnMap( 0, 0 );

					// Assign result
					ImagePanels[i].Image = m_BitmapResults[i];
				}

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				groupBoxOptions.Enabled = true;
			}
		}

		private void	BuildVisibilityMap( RendererManaged.Texture2D _Source, RendererManaged.Texture3D _Target ) {
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

			int	h = Math.Max( 1, MAX_LINES*1024 / W );
			int	CallsCount = (int) Math.Ceiling( (float) H / h );
			for ( int i=0; i < CallsCount; i++ )
			{
				m_CB_Visibility.m._Y0 = (UInt32) (i * h);
				m_CB_Visibility.UpdateData();

				m_CS_GenerateVisibilityMap.Dispatch( W, h, 1 );

				m_Device.Present( true );

				progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (VISIBILITY_PROGRESS - BILATERAL_PROGRESS) * (i+1) / CallsCount) * progressBar.Maximum);
//				for ( int a=0; a < 10; a++ )
					Application.DoEvents();
			}

			// Single gulp (crashes the driver on large images!)
//			m_CS_GenerateVisibilityMap.Dispatch( W, H, 1 );

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	ApplyBilateralFiltering( RendererManaged.Texture2D _Source, RendererManaged.Texture2D _Target, float _BilateralRadius, float _BilateralTolerance, bool _Wrap ) {
			if ( !m_CS_BilateralFilter.Use() )
				throw new Exception( "Can't generate translucency map as bilateral filter compute shader failed to compile!" );

			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

			m_CB_Filter.m.Radius = _BilateralRadius;
			m_CB_Filter.m.Tolerance = _BilateralTolerance;
			m_CB_Filter.m.Tile = (uint) (_Wrap ? 1 : 0);

			int	h = Math.Max( 1, MAX_LINES*1024 / W );
			int	CallsCount = (int) Math.Ceiling( (float) H / h );
			for ( int i=0; i < CallsCount; i++ )
			{
				m_CB_Filter.m.Y0 = (UInt32) (i * h);
				m_CB_Filter.UpdateData();

				m_CS_BilateralFilter.Dispatch( W, h, 1 );

				m_Device.Present( true );

				progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS * (i+1) / CallsCount) * progressBar.Maximum);
//				for ( int a=0; a < 10; a++ )
					Application.DoEvents();
			}

			// Single gulp (crashes the driver on large images!)
//			m_CS_BilateralFilter.Dispatch( W, H, 1 );

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	GenerateRays( int _RaysCount, RendererManaged.StructuredBuffer<RendererManaged.float3> _Target ) {
// 			_RaysCount = Math.Min( MAX_THREADS, _RaysCount );
// 
// 			// Half-Life 2 basis
// 			RendererManaged.float3[]	HL2Basis = new RendererManaged.float3[] {
// 				new RendererManaged.float3( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ),
// 				new RendererManaged.float3( -(float) Math.Sqrt( 1.0 / 6.0 ), (float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ),
// 				new RendererManaged.float3( -(float) Math.Sqrt( 1.0 / 6.0 ), -(float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) )
// 			};
// 
// 			float	CenterTheta = (float) Math.Acos( HL2Basis[0].z );
// 			float[]	CenterPhi = new float[] {
// 				(float) Math.Atan2( HL2Basis[0].y, HL2Basis[0].x ),
// 				(float) Math.Atan2( HL2Basis[1].y, HL2Basis[1].x ),
// 				(float) Math.Atan2( HL2Basis[2].y, HL2Basis[2].x ),
// 			};
// 
// 			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++ ) {
// 				double	Phi = (Math.PI / 3.0) * (2.0 * WMath.SimpleRNG.GetUniform() - 1.0);
// 
// 				// Stratified version
// 				double	Theta = (Math.Acos( Math.Sqrt( (RayIndex + WMath.SimpleRNG.GetUniform()) / _RaysCount ) ));
// 
// // 				// Don't give a shit version (a.k.a. melonhead version)
// // //				double	Theta = Math.Acos( Math.Sqrt(WMath.SimpleRNG.GetUniform() ) );
// // 				double	Theta = 0.5 * Math.PI * WMath.SimpleRNG.GetUniform();
// 
// 				Theta = Math.Min( 0.499f * Math.PI, Theta );
// 
// 
// 				double	CosTheta = Math.Cos( Theta );
// 				double	SinTheta = Math.Sin( Theta );
// 
// 				double	LengthFactor = 1.0 / SinTheta;	// The ray is scaled so we ensure we always walk at least a texel in the texture
// 				CosTheta *= LengthFactor;
// 				SinTheta *= LengthFactor;	// Yeah, yields 1... :)
// 
// 				_Target.m[0*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[0] + Phi ) * SinTheta),
// 														(float) (Math.Sin( CenterPhi[0] + Phi ) * SinTheta),
// 														(float) CosTheta );
// 				_Target.m[1*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[1] + Phi ) * SinTheta),
// 														(float) (Math.Sin( CenterPhi[1] + Phi ) * SinTheta),
// 														(float) CosTheta );
// 				_Target.m[2*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[2] + Phi ) * SinTheta),
// 														(float) (Math.Sin( CenterPhi[2] + Phi ) * SinTheta),
// 														(float) CosTheta );
// 			}
// 
// 			_Target.Write();
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
			GenerateRays( _Sender.Value, m_SB_Rays );
		}

		private void radioButtonShowDirOccRGB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.RGB;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.RGB;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.RGB;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.RGB;
			}
		}

		private void radioButtonDirOccRGBtimeAO_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
			}
		}

		private void radioButtonDirOccR_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.R;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.R;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.R;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.R;
			}
		}

		private void radioButtonDirOccG_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.G;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.G;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.G;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.G;
			}
		}

		private void radioButtonDirOccB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.B;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.B;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.B;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.B;
			}
		}

		private void radioButton1_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.AO;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.AO;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.AO;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.AO;
			}
		}

		private void radioButtonAOfromRGB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
			}
		}

		private void checkBoxShowsRGB_CheckedChanged( object sender, EventArgs e )
		{
			imagePanelThicknessMap.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult0.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult1.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult2.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult3.ViewLinear = !checkBoxShowsRGB.Checked;
		}

		private void imagePanelResult0_Click(object sender, EventArgs e)
		{
			if ( m_BitmapResults[0] == null ) {
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
				m_BitmapResults[0].Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult1_Click(object sender, EventArgs e)
		{
			if ( m_BitmapResults[1] == null ) {
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
				m_BitmapResults[1].Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult2_Click(object sender, EventArgs e)
		{
			if ( m_BitmapResults[2] == null ) {
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
				m_BitmapResults[2].Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult3_Click(object sender, EventArgs e)
		{
			if ( m_BitmapResultCombined == null ) {
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
				m_BitmapResultCombined.Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );
				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
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

		}

		private void floatTrackbarControlDominantHue_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			RendererManaged.float3	RGB = HSL2RGB( new RendererManaged.float3( _Sender.Value / 360.0f, 1.0f, 0.5f ) );
			panelDominantHue.BackColor = Color.FromArgb( (int) (255 * RGB.x), (int) (255 * RGB.y), (int) (255 * RGB.z) );
		}

		RendererManaged.float3	HSL2RGB( RendererManaged.float3 _HSL ) {
			float	H = _HSL.x;
			float	S = _HSL.y;
			float	L = _HSL.z;

			if ( S == 0.0f )
			   return L * RendererManaged.float3.One;

			float	var_2 = _HSL.z < 0.5f ? L * ( 1 + S ) :( L + S ) - ( S * L );
			float	var_1 = 2 * L - var_2;

			float	R = Hue_2_RGB( var_1, var_2, H + 1 / 3.0f ) ;
			float	G = Hue_2_RGB( var_1, var_2, H );
			float	B = Hue_2_RGB( var_1, var_2, H - 1 / 3.0f );

			return new RendererManaged.float3( R, G, B );
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
