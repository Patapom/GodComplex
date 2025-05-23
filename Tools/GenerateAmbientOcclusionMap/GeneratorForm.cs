﻿//////////////////////////////////////////////////////////////////////////
// Builds an AO map from a height maps
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

using SharpMath;

namespace GenerateSelfShadowedBumpMap
{
	public partial class GeneratorForm : Form {
		#region CONSTANTS

		private const int		MAX_THREADS = 1024;			// Maximum threads run by the compute shader

		private const int		BILATERAL_PROGRESS = 50;	// Bilateral filtering is considered as this % of the total task (bilateral is quite long so I decided it was equivalent to 50% of the complete computation task)
		private const int		MAX_LINES = 16;				// Process at most that amount of lines of a 4096x4096 image for a single dispatch

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBInput {
			public uint		_textureDimensionX;
			public uint		_textureDimensionY;
			public uint		_Y0;				// Index of the texture line we're processing
			public uint		_raysCount;			// Amount of rays in the structured buffer

			public uint		_maxStepsCount;		// Maximum amount of steps to take before stopping
			public uint		_tile;				// Tiling flag
			public float	_texelSize_mm;		// Size of a texel (in millimeters)
			public float	_displacement_mm;	// Max displacement value encoded by the height map (in millimeters)
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBFilter {
			public uint		_Y0;				// Index of the texture line we're processing
// 			public float	_radius;			// Radius of the bilateral filter
// 			public float	_tolerance;			// Range tolerance of the bilateral filter
			public float	_sigma_Radius;		// Radius of the bilateral filter
			public float	_sigma_Tolerance;	// Range tolerance of the bilateral filter
			public uint		_tile;				// Tiling flag
		}

		#endregion

		#region FIELDS

		private RegistryKey							m_AppKey;
		private string								m_ApplicationPath;

		private System.IO.FileInfo					m_SourceFileName = null;
		private uint								W, H;
		private ImageUtility.ImageFile				m_imageSourceHeight = null;
		private ImageUtility.ImageFile				m_imageSourceNormal = null;

		internal Renderer.Device					m_device = new Renderer.Device();
		internal Renderer.Texture2D					m_textureSourceHeightMap = null;
		internal Renderer.Texture2D					m_TextureSourceNormal = null;
		internal Renderer.Texture2D					m_textureTarget0 = null;
		internal Renderer.Texture2D					m_textureTarget1 = null;
		internal Renderer.Texture2D					m_textureTarget_CPU = null;

		// SSBump Generation
		private Renderer.ConstantBuffer<CBInput>	m_CB_Input;
		private Renderer.StructuredBuffer<float3>	m_SB_Rays = null;
		private Renderer.ComputeShader				m_CS_GenerateAOMap = null;
		private Renderer.ComputeShader				m_CS_GenerateBentConeMap = null;
		private Renderer.ComputeShader				m_CS_GenerateBentConeMapOpt = null;

		// Bilateral filtering pre-processing
		private Renderer.ConstantBuffer<CBFilter>	m_CB_Filter;
		private Renderer.ComputeShader				m_CS_BilateralFilter = null;

 		private ImageUtility.ColorProfile			m_profilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.ColorProfile			m_profileLinear = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR );
		private ImageUtility.ImageFile				m_imageResult = null;

		#endregion

		#region PROPERTIES

		internal float	TextureHeight_mm {
			get { return 10.0f * floatTrackbarControlHeight.Value; }
		}

		internal float	TextureSize_mm {
			get { return 10.0f * floatTrackbarControlPixelDensity.Value; }
		}

		#endregion

		#region METHODS

		public unsafe GeneratorForm() {
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\AOMapGenerator" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );

// N.Silvagni test with Adobe RGB
//			ImageUtility.Bitmap	Test = new ImageUtility.Bitmap( 1, 1, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
//			Test.ContentXYZ[0,0] = Test.Profile.RGB2XYZ( new ImageUtility.float4( 0, 1, 0, 1 ) );
//			ImageUtility.ColorProfile	AdobeProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.ADOBE_RGB_D65 );
//			ImageUtility.ColorProfile	AdobeProfile2 = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.ADOBE_RGB_D50 );
//
//
//			ImageUtility.float4	Glou0 = AdobeProfile.XYZ2RGB( Test.ContentXYZ[0,0] );
//			ImageUtility.float4	Glou1 = AdobeProfile2.XYZ2RGB( Test.ContentXYZ[0,0] );
//			ImageUtility.float4	Glou2 = Test.Profile.XYZ2RGB( Test.ContentXYZ[0,0] );

// Check numerical integration is right
// 			float	phi = 0.0f;
// 			float	thetaBack = -0.5f * Mathf.PI;
// 			float	thetaFront = 0.5f * Mathf.PI;
// 			float2	ssDirection = new float2( Mathf.Cos( phi ), Mathf.Sin( phi ) );
// 			float3	ssBentNormal = float3.Zero;
// 			for ( uint i=0; i < 256; i++ ) {
// 				float	theta = Mathf.Lerp( thetaBack, thetaFront, (i+0.5f) / 256 );
// 				float	sinTheta = Mathf.Sin( theta );
// 				float	cosTheta = Mathf.Cos( theta );
// 				float3	ssUnOccludedDirection = new float3( sinTheta * ssDirection, cosTheta );
// 				ssBentNormal += Math.Abs( sinTheta ) * ssUnOccludedDirection;
// 			}

// Check integral is right
// 		float	phi = 0.0f;
// 		float	theta0 = 0.75f * Mathf.PI;
// 		float	theta1 = 0.25f * Mathf.PI;
// 
// 		float2	ssDirection = new float2( Mathf.Cos( phi ), Mathf.Sin( phi ) );
// 		float3	N = new float3( 1, 2, 1 ).Normalized;
// 
// 		float	cosAlpha = N.Dot( float3.UnitZ );
// 		float	sinAlpha = Mathf.Sqrt( 1.0f - cosAlpha*cosAlpha );
// 		float	cosPhi = N.xy.Normalized.Dot( ssDirection );
// 
// 		float	cosTheta0 = Mathf.Cos( theta0 );
// 		float	sinTheta0 = Mathf.Sin( theta0 );
// 		float	cosTheta1 = Mathf.Cos( theta1 );
// 		float	sinTheta1 = Mathf.Sin( theta1 );
// 		float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
// 		float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
// 		float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
// 		float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;
// 
// // 		float	X = cosAlpha * (sinTheta0_3 - sinTheta1_3) + sinAlpha * cosPhi * (cosTheta0_3 - 3*cosTheta0 + cosTheta1_3 - 3*cosTheta1 + 4) + sinAlpha * (cosTheta1_3 - 3*cosTheta1);
// // 		float	Y = cosAlpha * (cosTheta1_3 - cosTheta0_3) + sinAlpha * cosPhi * (sinTheta0_3 + sinTheta1_3) + sinAlpha * sinTheta1_3;
// 
// 		float	X = cosAlpha * (sinTheta0_3 - sinTheta1_3) + sinAlpha * cosPhi * (cosTheta0_3 - 3*cosTheta0 + cosTheta1_3 - 3*cosTheta1 + 4) - sinAlpha * (cosTheta1_3 - 3*cosTheta1 + 2);
// 		float	Y = cosAlpha * (2 - cosTheta1_3 - cosTheta0_3) + sinAlpha * cosPhi * (sinTheta0_3 - sinTheta1_3) + sinAlpha * sinTheta1_3;

			#if DEBUG
				buttonReload.Visible = true;
				checkBoxBruteForce.Visible = true;
			#endif
		}

		protected override void OnLoad(EventArgs e) {
 			base.OnLoad(e);

			try {
				m_device.Init( viewportPanelResult.Handle, false, true );

				// Create our compute shaders
				#if !DEBUG
					using ( Renderer.ScopedForceShadersLoadFromBinary scope = new Renderer.ScopedForceShadersLoadFromBinary() )
				#endif
				{
					m_CS_BilateralFilter = new Renderer.ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ), "CS" );
					m_CS_GenerateAOMap = new Renderer.ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/GenerateAOMap.hlsl" ), "CS" );
					m_CS_GenerateBentConeMap = new Renderer.ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/GenerateBentConeMap.hlsl" ), "CS" );
					m_CS_GenerateBentConeMapOpt = new Renderer.ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/GenerateBentConeMapOpt3.hlsl" ), "CS" );
				}

				// Create our constant buffers
				m_CB_Input = new Renderer.ConstantBuffer<CBInput>( m_device, 0 );
				m_CB_Filter = new Renderer.ConstantBuffer<CBFilter>( m_device, 0 );

				// Create our structured buffer containing the rays
				m_SB_Rays = new Renderer.StructuredBuffer<float3>( m_device, MAX_THREADS, true, false );
				integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

				// Create the default, planar normal map
				clearNormalToolStripMenuItem_Click( null, EventArgs.Empty );

			} catch ( Exception _e ) {
				MessageBox( "Failed to create DX11 device and default shaders:\r\n", _e );
				Close();
			}
		}

		protected override void OnClosing( CancelEventArgs e ) {
			try {
				m_CS_GenerateBentConeMapOpt.Dispose();
				m_CS_GenerateBentConeMap.Dispose();
				m_CS_GenerateAOMap.Dispose();
				m_CS_BilateralFilter.Dispose();

				m_SB_Rays.Dispose();
				m_CB_Filter.Dispose();
				m_CB_Input.Dispose();

				if ( m_textureTarget_CPU != null )
					m_textureTarget_CPU.Dispose();
				if ( m_textureTarget1 != null )
					m_textureTarget1.Dispose();
				if ( m_textureTarget0 != null )
					m_textureTarget0.Dispose();
				if ( m_TextureSourceNormal != null )
					m_TextureSourceNormal.Dispose();
				if ( m_textureSourceHeightMap != null )
					m_textureSourceHeightMap.Dispose();

				m_device.Dispose();
			} catch ( Exception ) {
			}

			e.Cancel = false;
			base.OnClosing( e );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Arguments-driven generation
		/// </summary>
		/// 
		public class	BuildArguments {
			public string	heightMapFileName = null;
			public string	normalMapFileName = null;
			public string	AOMapFileName = null;
			public float	textureSize_cm = 100.0f;
			public float	displacementSize_cm = 45.0f;
			public int		raysCount = 1024;
			public int		searchRange = 200;
			public float	coneAngle = 160.0f;
			public bool		tile = true;
			public float	bilateralRadius = 1.0f;
			public float	bilateralTolerance = 0.2f;
		}

		bool	m_silentMode = false;
		public void		Build( BuildArguments _args ) {

			// Setup arguments
			floatTrackbarControlHeight.Value = _args.displacementSize_cm;
			floatTrackbarControlPixelDensity.Value = _args.textureSize_cm;

			integerTrackbarControlRaysCount.Value = _args.raysCount;
			integerTrackbarControlMaxStepsCount.Value = _args.searchRange;
			floatTrackbarControlMaxConeAngle.Value = _args.coneAngle;

			floatTrackbarControlBilateralRadius.Value = _args.bilateralRadius;
			floatTrackbarControlBilateralTolerance.Value = _args.bilateralTolerance;

			checkBoxWrap.Checked = _args.tile;

			m_silentMode = true;

			// Create device, shaders and structures
			OnLoad( EventArgs.Empty );

			// Load height map
			System.IO.FileInfo	HeightMapFileName = new System.IO.FileInfo( _args.heightMapFileName );
			LoadHeightMap( HeightMapFileName );

			// Load normal map
			if ( _args.normalMapFileName != null ) {
				System.IO.FileInfo	NormalMapFileName = new System.IO.FileInfo( _args.normalMapFileName );
				LoadNormalMap( NormalMapFileName );
			}

			// Generate
 			GenerateAO();

			// Save results
			m_imageResult.Save( new System.IO.FileInfo( _args.AOMapFileName ) );

			// Dispose
			CancelEventArgs	onsenfout = new CancelEventArgs();
			OnClosing( (CancelEventArgs) onsenfout );
		}

		private void	LoadHeightMap( System.IO.FileInfo _FileName ) {
			try {
				panelParameters.Enabled = false;

				// Dispose of existing resources
				if ( m_imageSourceHeight != null )
					m_imageSourceHeight.Dispose();
				m_imageSourceHeight = null;

				if ( m_textureTarget_CPU != null )
					m_textureTarget_CPU.Dispose();
				m_textureTarget_CPU = null;
				if ( m_textureTarget0 != null )
					m_textureTarget0.Dispose();
				m_textureTarget0 = null;
				if ( m_textureTarget1 != null )
					m_textureTarget1.Dispose();
				m_textureTarget1 = null;
				if ( m_textureSourceHeightMap != null )
					m_textureSourceHeightMap.Dispose();
				m_textureSourceHeightMap = null;

				// Load the source image
				// Assume it's in linear space
				m_SourceFileName = _FileName;
				m_imageSourceHeight = new ImageUtility.ImageFile( _FileName );
				outputPanelInputHeightMap.Bitmap = m_imageSourceHeight.AsBitmap;

				W = m_imageSourceHeight.Width;
				H = m_imageSourceHeight.Height;

				// Build the source texture
				float4[]	scanline = new float4[W];

				Renderer.PixelsBuffer	SourceHeightMap = new Renderer.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceHeightMap.OpenStreamWrite() )
					for ( uint Y=0; Y < H; Y++ ) {
						m_imageSourceHeight.ReadScanline( Y, scanline );
						for ( uint X=0; X < W; X++ ) {
							Wr.Write( scanline[X].y );
						}
					}

				m_textureSourceHeightMap = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceHeightMap } );

				// Build the target UAV & staging texture for readback
				m_textureTarget0 = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
				m_textureTarget1 = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
				m_textureTarget_CPU = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, false, null );

				panelParameters.Enabled = true;
				buttonGenerate.Focus();

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the image:\n\n", _e );
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
				// Assume it's in linear space (all normal maps should be in linear space, with the default value being (0.5, 0.5, 1))
				m_imageSourceNormal = new ImageUtility.ImageFile( _FileName );
				imagePanelNormalMap.Bitmap = m_imageSourceNormal.AsBitmap;

				uint	W = m_imageSourceNormal.Width;
				uint	H = m_imageSourceNormal.Height;

				// Build the source texture
				float4[]	scanline = new float4[W];

				Renderer.PixelsBuffer	SourceNormalMap = new Renderer.PixelsBuffer( W*H*4*4 );
				using ( System.IO.BinaryWriter Wr = SourceNormalMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ ) {
						m_imageSourceNormal.ReadScanline( (uint) Y, scanline );
						for ( int X=0; X < W; X++ ) {
							float	Nx = 2.0f * scanline[X].x - 1.0f;
							float	Ny = 1.0f - 2.0f * scanline[X].y;
							float	Nz = 2.0f * scanline[X].z - 1.0f;
							Wr.Write( Nx );
							Wr.Write( Ny );
							Wr.Write( Nz );
							Wr.Write( 1.0f );
						}
					}

				m_TextureSourceNormal = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceNormalMap } );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void	GenerateAO() {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_textureSourceHeightMap, m_textureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, checkBoxWrap.Checked, BILATERAL_PROGRESS );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute directional occlusion
				m_textureTarget1.RemoveFromLastAssignedSlots();

				// Prepare computation parameters
				m_textureTarget0.SetCS( 0 );
				m_textureTarget1.SetCSUAV( 0 );
				m_SB_Rays.SetInput( 1 );
				m_TextureSourceNormal.SetCS( 2 );

				m_CB_Input.m._textureDimensionX = W;
				m_CB_Input.m._textureDimensionY = H;
				m_CB_Input.m._raysCount = (uint) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
				m_CB_Input.m._maxStepsCount = (uint) integerTrackbarControlMaxStepsCount.Value;
				m_CB_Input.m._tile = (uint) (checkBoxWrap.Checked ? 1 : 0);
				m_CB_Input.m._texelSize_mm = TextureSize_mm / Math.Max( W, H );
				m_CB_Input.m._displacement_mm = TextureHeight_mm;

				// Start
				if ( !m_CS_GenerateAOMap.Use() )
					throw new Exception( "Can't generate self-shadowed bump map as compute shader failed to compile!" );

				uint	h = Math.Max( 1, MAX_LINES*1024 / W );
				uint	CallsCount = (uint) Math.Ceiling( (float) H / h );
				for ( int i=0; i < CallsCount; i++ ) {
					m_CB_Input.m._Y0 = (uint) (i * h);
					m_CB_Input.UpdateData();

					m_CS_GenerateAOMap.Dispatch( W, h, 1 );

					m_device.Present( true );

					progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (100-BILATERAL_PROGRESS) * (i+1) / (CallsCount)) * progressBar.Maximum);
//					for ( int a=0; a < 10; a++ )
						Application.DoEvents();
				}

				m_textureTarget1.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage

				progressBar.Value = progressBar.Maximum;

				// Compute in a single shot (this is madness!)
// 				m_CB_Input.m.y = 0;
// 				m_CB_Input.UpdateData();
// 				m_CS_GenerateSSBumpMap.Dispatch( W, H, 1 );


				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				m_textureTarget_CPU.CopyFrom( m_textureTarget1 );

/* Annoyingly complicated and useless code
//				ImageUtility.ColorProfile	profile = m_profilesRGB;	// AO maps are sRGB! (although strange, that's certainly to have more range in dark values)
				ImageUtility.ColorProfile	profile = m_profileLinear;

				float3	whitePoint_xyY = new float3( profile.Chromas.White, 0 );
				float3	whitePoint_XYZ = new float3();

				ImageUtility.Bitmap		tempBitmap = new ImageUtility.Bitmap( W, H );
				Renderer.PixelsBuffer	Pixels = m_textureTarget_CPU.MapRead( 0, 0 );
				using ( System.IO.BinaryReader R = Pixels.OpenStreamRead() )
					for ( uint Y=0; Y < H; Y++ ) {
						R.BaseStream.Position = Y * Pixels.RowPitch;
						for ( uint X=0; X < W; X++ ) {
							whitePoint_xyY.z = R.ReadSingle();		// Linear value
							ImageUtility.ColorProfile.xyY2XYZ( whitePoint_xyY, ref whitePoint_XYZ );
							tempBitmap[X,Y] = new float4( whitePoint_XYZ, 1 );
						}
					}

				m_textureTarget_CPU.UnMap( Pixels );

				// Convert to RGB
				ImageUtility.ImageFile	temmpImageRGBA32F = new ImageUtility.ImageFile();
				tempBitmap.ToImageFile( temmpImageRGBA32F, profile );

				if ( m_imageResult == null )
					m_imageResult = new ImageUtility.ImageFile();
				m_imageResult.ToneMapFrom( temmpImageRGBA32F, ( float3 _HDR, ref float3 _LDR ) => {
					_LDR = _HDR;	// Return as-is..
				} );
*/

				float[,]	AOMap = new float[W,H];
				m_textureTarget_CPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => { AOMap[_X,_Y] = _R.ReadSingle(); } );

				if ( m_imageResult != null )
					m_imageResult.Dispose();
				m_imageResult = new ImageUtility.ImageFile( W, H, ImageUtility.PIXEL_FORMAT.BGRA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
				m_imageResult.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
					float	AO = AOMap[_X,_Y];
					_color.Set( AO, AO, AO, 1.0f );
				} );

				// Assign result
				viewportPanelResult.Bitmap = m_imageResult.AsBitmap;

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		private void	GenerateBentCone() {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_textureSourceHeightMap, m_textureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, checkBoxWrap.Checked, BILATERAL_PROGRESS );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute bent normal map
				Renderer.Texture2D	textureBentNormal = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );

				m_textureTarget1.RemoveFromLastAssignedSlots();

				// Prepare computation parameters
				m_textureTarget0.SetCS( 0 );

//m_textureSourceHeightMap.SetCS( 0 );

				textureBentNormal.SetCSUAV( 0 );
				m_SB_Rays.SetInput( 1 );
				m_TextureSourceNormal.SetCS( 2 );

				m_CB_Input.m._textureDimensionX = W;
				m_CB_Input.m._textureDimensionY = H;
//				m_CB_Input.m._raysCount = (uint) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
				m_CB_Input.m._raysCount = (uint) Math.Min( 256, integerTrackbarControlRaysCount.Value );	// Useless to go above and we need to make sure we've less than 256 rays so we can store a counter on 8 bits
				m_CB_Input.m._maxStepsCount = (uint) integerTrackbarControlMaxStepsCount.Value;
				m_CB_Input.m._tile = (uint) (checkBoxWrap.Checked ? 1 : 0);
				m_CB_Input.m._texelSize_mm = TextureSize_mm / Math.Max( W, H );
				m_CB_Input.m._displacement_mm = TextureHeight_mm;

				// Start
				Renderer.ComputeShader	shaderGenerateBentCone = checkBoxBruteForce.Checked ? m_CS_GenerateBentConeMap : m_CS_GenerateBentConeMapOpt;
				if ( !shaderGenerateBentCone.Use() )
					throw new Exception( "Can't generate self-shadowed bump map as compute shader failed to compile!" );

				uint	h = Math.Max( 1, MAX_LINES*1024 / W );
				uint	callsCount = (uint) Math.Ceiling( (float) H / h );
				for ( int i=0; i < callsCount; i++ ) {
					m_CB_Input.m._Y0 = (uint) (i * h);
					m_CB_Input.UpdateData();

					shaderGenerateBentCone.Dispatch( W, h, 1 );

					m_device.Present( true );

					progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (100-BILATERAL_PROGRESS) * (i+1) / (callsCount)) * progressBar.Maximum);
					Application.DoEvents();
				}

//				m_textureTarget1.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage

				progressBar.Value = progressBar.Maximum;


				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				Renderer.Texture2D	textureBentNormal_CPU = new Renderer.Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, false, null );
				textureBentNormal_CPU.CopyFrom( textureBentNormal );
				textureBentNormal.Dispose();

				float4[,]	bentNormal = new float4[W,H];
				textureBentNormal_CPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
					bentNormal[_X,_Y].x = _R.ReadSingle();
					bentNormal[_X,_Y].y = _R.ReadSingle();
					bentNormal[_X,_Y].z = _R.ReadSingle();
					bentNormal[_X,_Y].w = _R.ReadSingle();
				} );

				textureBentNormal_CPU.Dispose();

				if ( m_imageResult != null )
					m_imageResult.Dispose();
				m_imageResult = new ImageUtility.ImageFile( W, H, ImageUtility.PIXEL_FORMAT.BGRA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
				m_imageResult.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
					_color = bentNormal[_X,_Y];
// 					_color.Set( bentNormal[_X,_Y].xyz, 1.0f );
// 					_color.Set( bentNormal[_X,_Y].w, bentNormal[_X,_Y].w, bentNormal[_X,_Y].w, 1.0f );
				} );

				// Assign result
				viewportPanelResult.Bitmap = m_imageResult.AsBitmap;

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		#region CPU Computation (DEBUG PURPOSE)

		float[,]	m_tempHeights = null;
		float3[,]	m_tempNormals = null;

		// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
		void BuildOrthonormalBasis( ref float3 _normal, ref float3 _tangent, ref float3 _bitangent ) {
			float a = _normal.z > -0.9999999f ? 1.0f / (1.0f + _normal.z) : 0.0f;
			float b = -_normal.x * _normal.y * a;

			_tangent = new float3( 1.0f - _normal.x*_normal.x*a, b, -_normal.x );
			_bitangent = new float3( b, 1.0f - _normal.y*_normal.y*a, -_normal.y );
		}

		float	SampleHeight( ref float3 _ssPosition ) {
			int		X0 = (int) Mathf.Floor( _ssPosition.x );
			float	x = _ssPosition.x - X0;
					X0 = X0 & ((int)W-1);
			int		X1 = (X0+1) & ((int)W-1);

			int		Y0 = (int) Mathf.Floor( _ssPosition.x );
			float	y = _ssPosition.x - X0;
					Y0 = Y0 & ((int)H-1);
			int		Y1 = (Y0+1) & ((int)H-1);

			float	H00 = m_tempHeights[X0,Y0];
			float	H10 = m_tempHeights[X1,Y0];
			float	H01 = m_tempHeights[X0,Y1];
			float	H11 = m_tempHeights[X1,Y1];
			float	H0 = (1.0f-x) * H00 + x * H10;
			float	H1 = (1.0f-x) * H01 + x * H11;
			float	Hfinal = (1.0f-y) * H0 + y * H1;
			return Hfinal;
		}

		private void	GenerateBentCone_CPU() {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// Read back textures
				float	pixelSize_mm = TextureSize_mm / Math.Max( W, H );
				float	factor = TextureHeight_mm / pixelSize_mm;
				m_tempHeights = new float[W,H];
				m_imageSourceHeight.ReadPixels( ( uint X, uint Y, ref float4 _color ) => {
					m_tempHeights[X,Y] = _color.y * factor;
				} );

				m_tempNormals = new float3[W,H];
				m_imageSourceNormal.ReadPixels( (uint X, uint Y, ref float4 _color ) => {
					m_tempNormals[X,Y].Set( 2.0f * _color.x - 1.0f, 1.0f - 2.0f * _color.y, 2.0f * _color.z - 1.0f );
				} );

				//////////////////////////////////////////////////////////////////////////
				// Process
				// Test: X = 238, Y = 178
				//
				const uint	ANGLE_STEPS = 16;
				float		H0;
				float3		T = float3.Zero, B = float3.Zero, N;
				float		phi;
				float2		tsDirection = float2.Zero, ssDirection = float2.Zero;
				float3		ssPosition_Front = float3.Zero, ssPosition_Back = float3.Zero;
				float3		ssDeltaPosition_Front = float3.Zero, ssDeltaPosition_Back = float3.Zero;
				float3		tsDeltaPosition_Front = float3.Zero, tsDeltaPosition_Back = float3.Zero;
				float		maxAngle_Front, minAngle_Back;
				float3		tsBentNormal, ssBentNormal;
				float		sumWeights;
				float3		tsTempDirection = float3.Zero;
				float		angle_Front, angle_Back;
				float		weight_Front, weight_Back;
				float4[,]	resultBentCone = new float4[W,H];

				for ( uint Y=178; Y < H; Y++ ) {
					for ( uint X=238; X < W; X++ ) {
						N = m_tempNormals[X,Y];
						BuildOrthonormalBasis( ref N, ref T, ref B );
						H0 = m_tempHeights[X,Y];

						float4	tsAccumulator = float4.Zero;
						for ( uint angleIndex=0; angleIndex < ANGLE_STEPS; angleIndex++ ) {
							phi = Mathf.PI * angleIndex / ANGLE_STEPS;
							tsDirection.x = Mathf.Cos( phi );
							tsDirection.y = Mathf.Sin( phi );

							ssDirection = tsDirection.x * T.xy + ssDirection.y * B.xy;
							ssDirection.Normalize();

							ssPosition_Front.Set( X, Y, H0 );
							ssPosition_Back.Set( X, Y, H0 );

							// Gather min/max horizon angles
							maxAngle_Front = 0.0f;
							minAngle_Back = Mathf.PI;
							for ( uint radius=1; radius < 100; radius++ ) {
								ssPosition_Front.x += ssDirection.x;
								ssPosition_Front.y += ssDirection.y;
								ssPosition_Front.z = SampleHeight( ref ssPosition_Front );
								ssDeltaPosition_Front.Set( ssPosition_Front.x - X, ssPosition_Front.y - Y, ssPosition_Front.z - H0 );
								tsDeltaPosition_Front.Set( ssDeltaPosition_Front.Dot( T ), ssDeltaPosition_Front.Dot( B ), ssDeltaPosition_Front.Dot( N ) );
//								maxAngle_Front = Mathf.Max( maxAngle_Front, Mathf.Atan2( tsDeltaPosition_Front.z, tsDeltaPosition_Front.xy.Dot( tsDirection ) ) );
float	tsDistance = tsDeltaPosition_Front.xy.Dot( tsDirection );
float	tsAngle = Mathf.Atan2( tsDeltaPosition_Front.z, tsDistance );
maxAngle_Front = Mathf.Max( maxAngle_Front, tsAngle );

								ssPosition_Back.x -= ssDirection.x;
								ssPosition_Back.y -= ssDirection.y;
								ssPosition_Back.z = SampleHeight( ref ssPosition_Back );
								ssDeltaPosition_Back.Set( ssPosition_Back.x - X, ssPosition_Back.y - Y, ssPosition_Back.z - H0 );
								tsDeltaPosition_Back.Set( ssDeltaPosition_Back.Dot( T ), ssDeltaPosition_Back.Dot( B ), ssDeltaPosition_Back.Dot( N ) );
//								minAngle_Back = Mathf.Min( minAngle_Back, Mathf.Atan2( tsDeltaPosition_Back.z, tsDeltaPosition_Back.xy.Dot( tsDirection ) ) );
tsDistance = tsDeltaPosition_Back.xy.Dot( tsDirection );
tsAngle = Mathf.Atan2( tsDeltaPosition_Back.z, tsDistance );
minAngle_Back = Mathf.Min( minAngle_Back, tsAngle );
							}

							// Half brute force where we perform the integration numerically as a sum...
							tsBentNormal = float3.Zero;
							sumWeights = 0.0f;
							for ( uint i=0; i < 32; i++ ) {
								float	cosTheta = (i+0.5f) / 32.0f;
								float	sinTheta = Mathf.Sqrt( 1.0f - cosTheta*cosTheta );

								// Accumulate front
								tsTempDirection.Set( sinTheta * tsDirection.x, sinTheta * tsDirection.y, cosTheta );
								angle_Front = 0.5f*Mathf.PI - Mathf.Acos(cosTheta);
								weight_Front = (angle_Front > maxAngle_Front && angle_Front < minAngle_Back) ? cosTheta : 0.0f;
								tsBentNormal += weight_Front * tsTempDirection;
								sumWeights += weight_Front;

								// Accumulate back
								tsTempDirection.Set( -sinTheta * tsDirection.x, -sinTheta * tsDirection.y, cosTheta );
								angle_Back = 0.5f*Mathf.PI + Mathf.Acos(cosTheta);
								weight_Back = (angle_Back > maxAngle_Front && angle_Back < minAngle_Back) ? cosTheta : 0.0f;
								tsBentNormal += weight_Back * tsTempDirection;
								sumWeights += weight_Back;
							}

							tsAccumulator.x += tsBentNormal.x;
							tsAccumulator.y += tsBentNormal.y;
							tsAccumulator.z += tsBentNormal.z;
							tsAccumulator.w += sumWeights;
						}

						// Store result
						tsBentNormal = tsAccumulator.xyz;
						tsBentNormal.Normalize();

						ssBentNormal = tsBentNormal.x * T + tsBentNormal.y * B + tsBentNormal.z * N;
						ssBentNormal.y *= -1.0f;

						resultBentCone[X,Y].Set( ssBentNormal, 1.0f );
					}

					progressBar.Value = (int) (Y * progressBar.Maximum / H);
					Application.DoEvents();
				}
				progressBar.Value = progressBar.Maximum;


				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				if ( m_imageResult != null )
					m_imageResult.Dispose();
				m_imageResult = new ImageUtility.ImageFile( W, H, ImageUtility.PIXEL_FORMAT.BGRA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
				m_imageResult.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
					_color = resultBentCone[_X,_Y];
// 					_color.Set( bentNormal[_X,_Y].xyz, 1.0f );
// 					_color.Set( bentNormal[_X,_Y].w, bentNormal[_X,_Y].w, bentNormal[_X,_Y].w, 1.0f );
				} );

				// Assign result
				viewportPanelResult.Bitmap = m_imageResult.AsBitmap;

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		#endregion

		private void	ApplyBilateralFiltering( Renderer.Texture2D _Source, Renderer.Texture2D _Target, float _BilateralRadius, float _BilateralTolerance, bool _Wrap, int _ProgressBarMax ) {
			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

// 			m_CB_Filter.m._radius = _BilateralRadius;
// 			m_CB_Filter.m._tolerance = _BilateralTolerance;
			m_CB_Filter.m._sigma_Radius = (float) (-0.5 * Math.Pow( _BilateralRadius / 3.0f, -2.0 ));
			m_CB_Filter.m._sigma_Tolerance = _BilateralTolerance > 0.0f ? (float) (-0.5 * Math.Pow( _BilateralTolerance, -2.0 )) : -1e6f;
			m_CB_Filter.m._tile = (uint) (_Wrap ? 1 : 0);

			m_CS_BilateralFilter.Use();

			uint	h = Math.Max( 1, MAX_LINES*1024 / W );
			uint	CallsCount = (uint) Math.Ceiling( (float) H / h );
			for ( uint i=0; i < CallsCount; i++ ) {
				m_CB_Filter.m._Y0 = (uint) (i * h);
				m_CB_Filter.UpdateData();

				m_CS_BilateralFilter.Dispatch( W, h, 1 );

				m_device.Present( true );

				progressBar.Value = (int) (0.01f * (0 + _ProgressBarMax * (i+1) / CallsCount) * progressBar.Maximum);
//				for ( int a=0; a < 10; a++ )
					Application.DoEvents();
			}

			// Single gulp (crashes the driver on large images!)
//			m_CS_BilateralFilter.Dispatch( W, H, 1 );

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	GenerateRays( int _RaysCount, float _MaxConeAngle, Renderer.StructuredBuffer<float3> _Target ) {
			_RaysCount = Math.Min( MAX_THREADS, _RaysCount );

			Hammersley	hammersley = new Hammersley();
			double[,]	sequence = hammersley.BuildSequence( _RaysCount, 2 );
			float3[]	rays = hammersley.MapSequenceToSphere( sequence, 0.5f * _MaxConeAngle );
			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++ ) {
				float3	ray = rays[RayIndex];

// 				// Scale the ray so we ensure to always walk at least a texel in the texture
// 				float	SinTheta = (float) Math.Sqrt( 1.0 - ray.y * ray.y );
// 				float	LengthFactor = 1.0f / SinTheta;
// 				ray *= LengthFactor;

				_Target.m[RayIndex].Set( ray.x, -ray.z, ray.y );
			}

			_Target.Write();
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
			if ( m_silentMode )
				throw new Exception( _Text );

			return System.Windows.Forms.MessageBox.Show( this, _Text, "Ambient Occlusion Map Generator", _Buttons, _Icon );
		}

		#endregion 

		#endregion

		#region EVENT HANDLERS

 		private unsafe void buttonGenerate_Click( object sender, EventArgs e ) {
 			GenerateAO();
//			Generate_CPU( integerTrackbarControlRaysCount.Value );
		}

		private void buttonGenerateBentCone_Click( object sender, EventArgs e ) {
 			GenerateBentCone();
// 			GenerateBentCone_CPU();
		}

		private void buttonTestBilateral_Click( object sender, EventArgs e ) {
			try {
				panelParameters.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_textureSourceHeightMap, m_textureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, checkBoxWrap.Checked, 100 );

				progressBar.Value = progressBar.Maximum;

				//////////////////////////////////////////////////////////////////////////
				// 2] Copy target to staging for CPU readback and update the resulting bitmap
				m_textureTarget_CPU.CopyFrom( m_textureTarget0 );

				ImageUtility.Bitmap		tempBitmap = new ImageUtility.Bitmap( W, H );
				Renderer.PixelsBuffer	pixels = m_textureTarget_CPU.MapRead( 0, 0 );
				using ( System.IO.BinaryReader R = pixels.OpenStreamRead() )
					for ( uint Y=0; Y < H; Y++ ) {
						R.BaseStream.Position = Y * pixels.RowPitch;
						for ( uint X=0; X < W; X++ ) {
							float	AO = R.ReadSingle();
							tempBitmap[X,Y] = new float4( AO, AO, AO, 1 );
						}
					}

				m_textureTarget_CPU.UnMap( pixels );

				// Convert to RGB
//				ImageUtility.ColorProfile	Profile = m_ProfilesRGB;	// AO maps are sRGB! (although strange, that's certainly to have more range in dark values)
				ImageUtility.ColorProfile	Profile = m_profilesRGB;	// AO maps are sRGB! (although strange, that's certainly to have more range in dark values)

				if ( m_imageResult != null )
					m_imageResult.Dispose();
				m_imageResult = new ImageUtility.ImageFile();
				tempBitmap.ToImageFile( m_imageResult, Profile );

				// Assign result
				viewportPanelResult.Bitmap = m_imageResult.AsCustomBitmap( ( ref float4 _color ) => {} );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				panelParameters.Enabled = true;
			}
		}

		private void integerTrackbarControlRaysCount_SliderDragStop( UIUtility.IntegerTrackbarControl _Sender, int _StartValue ) {
			GenerateRays( _Sender.Value, floatTrackbarControlMaxConeAngle.Value * (float) (Math.PI / 180.0), m_SB_Rays );
		}

		private void floatTrackbarControlMaxConeAngle_SliderDragStop( UIUtility.FloatTrackbarControl _Sender, float _fStartValue ) {
			GenerateRays( integerTrackbarControlRaysCount.Value, floatTrackbarControlMaxConeAngle.Value * (float) (Math.PI / 180.0), m_SB_Rays );
		}

		private void checkBoxViewsRGB_CheckedChanged( object sender, EventArgs e ) {
			viewportPanelResult.ViewLinear = !checkBoxViewsRGB.Checked;
		}

		private void floatTrackbarControlBrightness_SliderDragStop( UIUtility.FloatTrackbarControl _Sender, float _fStartValue ) {
			viewportPanelResult.Brightness = _Sender.Value;
		}

		private void floatTrackbarControlContrast_SliderDragStop( UIUtility.FloatTrackbarControl _Sender, float _fStartValue ) {
			viewportPanelResult.Contrast = _Sender.Value;
		}

		private void floatTrackbarControlGamma_SliderDragStop( UIUtility.FloatTrackbarControl _Sender, float _fStartValue ) {
			viewportPanelResult.Gamma = _Sender.Value;
		}

		private unsafe void viewportPanelResult_Click( object sender, EventArgs e ) {
			if ( m_imageResult == null ) {
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_ao.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try {
				m_imageResult.Save( new System.IO.FileInfo( saveFileDialogImage.FileName ), ImageUtility.ImageFile.FILE_FORMAT.PNG );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		#region Height Map

		private void outputPanelInputHeightMap_Click( object sender, EventArgs e ) {
			string	OldFileName = GetRegKey( "HeightMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "HeightMapFileName", openFileDialogImage.FileName );

			LoadHeightMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private string	m_DraggedFileName = null;
		private void outputPanelInputHeightMap_DragEnter( object sender, DragEventArgs e ) {
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			if ( ImageUtility.ImageFile.GetFileTypeFromFileNameOnly( DraggedFileName ) != ImageUtility.ImageFile.FILE_FORMAT.UNKNOWN ) {
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void outputPanelInputHeightMap_DragDrop( object sender, DragEventArgs e ) {
			if ( m_DraggedFileName != null )
				LoadHeightMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Normal Map

		private void outputPanelInputNormalMap_Click( object sender, EventArgs e ) {
			string	OldFileName = GetRegKey( "NormalMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetDirectoryName( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "NormalMapFileName", openFileDialogImage.FileName );

			LoadNormalMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private void outputPanelInputNormalMap_DragEnter( object sender, DragEventArgs e ) {
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			if ( ImageUtility.ImageFile.GetFileTypeFromFileNameOnly( DraggedFileName ) != ImageUtility.ImageFile.FILE_FORMAT.UNKNOWN ) {
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void outputPanelInputNormalMap_DragDrop( object sender, DragEventArgs e ) {
			if ( m_DraggedFileName != null )
				LoadNormalMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		private void clearNormalToolStripMenuItem_Click( object sender, EventArgs e ) {
			if ( m_TextureSourceNormal != null )
				m_TextureSourceNormal.Dispose();
			m_TextureSourceNormal = null;
			imagePanelNormalMap.Bitmap = null;

			// Create the default, planar normal map
			Renderer.PixelsBuffer	SourceNormalMap = new Renderer.PixelsBuffer( 4*4 );
			using ( System.IO.BinaryWriter Wr = SourceNormalMap.OpenStreamWrite() ) {
				Wr.Write( 0.0f );
				Wr.Write( 0.0f );
				Wr.Write( 1.0f );
				Wr.Write( 1.0f );
			}

			m_TextureSourceNormal = new Renderer.Texture2D( m_device, 1, 1, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceNormalMap } );
		}

		#endregion

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
