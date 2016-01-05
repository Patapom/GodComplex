using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;

using RendererManaged;
using Nuaj.Cirrus.Utility;

namespace TestFilmicCurve
{
	public partial class Form1 : Form
	{
		private RegistryKey	m_AppKey;

		private Device		m_Device = new Device();

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		_Resolution;
 			public float		_GlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_AutoExposure {
			public float		_delta_time;
			public float		_white_level;				// (1.0) White level for tone mapping
			public float		_clip_shadows;				// (0.0) Shadow cropping in histogram (first buckets will be ignored, leading to brighter image)
			public float		_clip_highlights;			// (1.0) Highlights cropping in histogram (last buckets will be ignored, leading to darker image)
			public float		_EV;						// (0.0) Your typical EV setting
			public float		_fstop_bias;				// (0.0) F-stop number bias to override automatic computation (NOTE: This will NOT change exposure, only the F number)
			public float		_reference_camera_fps;		// (30.0) Default camera at 30 FPS
			public float		_adapt_min_luminance;		// (0.03) Prevents the auto-exposure to adapt to luminances lower than this
			public float		_adapt_max_luminance;		// (2000.0) Prevents the auto-exposure to adapt to luminances higher than this
			public float		_adapt_speed_up;			// (0.99) Adaptation speed from low to high luminances
			public float		_adapt_speed_down;			// (0.99) Adaptation speed from high to low luminances
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_ToneMapping {
			public float		_Exposure;
			public uint			_Flags;
			public float		_A;
			public float		_B;
			public float		_C;
			public float		_D;
			public float		_E;
			public float		_F;
			public float		_WhitePoint;

			public float		_SaturationFactor;
			public float		_DarkenFactor;
			public float		_DebugLuminanceLevel;
			public float		_MouseU;
			public float		_MouseV;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct autoExposure_t {
			public float	EngineLuminanceFactor;		// The actual factor to apply to values stored to the HDR render target (it's simply LuminanceFactor * WORLD_TO_BISOU_LUMINANCE so it's a division by about 100)
			public float	TargetLuminance;			// The target luminance to apply to the HDR luminance to bring it to the LDR luminance (warning: still in world units, you must multiply by WORLD_TO_BISOU_LUMINANCE for a valid engine factor)
			public float	MinLuminanceLDR;			// Minimum luminance (cd/m²) the screen will display as the value sRGB 1
			public float	MaxLuminanceLDR;			// Maximum luminance (cd/m²) the screen will display as the value sRGB 255
			public float	MiddleGreyLuminanceLDR;		// "Reference EV" luminance (cd/m²) the screen will display as the value sRGB 128 (55 linear)
			public float	EV;							// Absolute Exposure Value of middle grey (sRGB 128) from a reference luminance of 0.15 cd/m² (see above for an explanation on that magic value)
			public float	Fstop;						// The estimate F-stop number (overridden with env/autoexp/fstop_bias)
			public uint		PeakHistogramValue;			// The maximum value found in the browsed histogram (values at start and end of histogram are not accounted for based on start & end bucket indices
		}

		private ConstantBuffer<CB_Main>				m_CB_Main = null;
		private ConstantBuffer<CB_Camera>			m_CB_Camera = null;
		private ConstantBuffer<CB_AutoExposure>		m_CB_AutoExposure = null;
		private ConstantBuffer<CB_ToneMapping>		m_CB_ToneMapping = null;

		private Shader								m_Shader_RenderHDR = null;
		private ComputeShader						m_Shader_ComputeTallHistogram = null;
		private ComputeShader						m_Shader_FinalizeHistogram = null;
		private ComputeShader						m_Shader_ComputeAutoExposure = null;
		private Shader								m_Shader_ToneMapping = null;

		private Texture2D							m_Tex_TallHistogram = null;
		private Texture2D							m_Tex_Histogram = null;
		private StructuredBuffer<autoExposure_t>	m_Buffer_AutoExposureSource = null;
		private StructuredBuffer<autoExposure_t>	m_Buffer_AutoExposureTarget = null;

		private Texture2D							m_Tex_HDR = null;
		private Texture2D							m_Tex_CubeMap = null;

		private Camera								m_Camera = new Camera();
		private CameraManipulator					m_Manipulator = new CameraManipulator();

		private DateTime							m_startTime = DateTime.Now;
		private DateTime							m_lastTime = DateTime.Now;

		public unsafe Form1()
		{
			InitializeComponent();

			m_AppKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\Patapom\ToneMapping" );

			panelGraph_Hable.ScaleX = floatTrackbarControlScaleX.Value;
			panelGraph_Hable.ScaleY = floatTrackbarControlScaleY.Value;

			SaveResetValues();

			outputPanelFilmic_Insomniac.ScaleX = floatTrackbarControlScaleX.Value;
			outputPanelFilmic_Insomniac.ScaleY = floatTrackbarControlScaleY.Value;
			outputPanelFilmic_Insomniac.BlackPoint = floatTrackbarControlIG_BlackPoint.Value;
			outputPanelFilmic_Insomniac.WhitePoint = floatTrackbarControlIG_WhitePoint.Value;
			outputPanelFilmic_Insomniac.JunctionPoint = floatTrackbarControlIG_JunctionPoint.Value;
			outputPanelFilmic_Insomniac.ToeStrength = ComputeToeStrength();
			outputPanelFilmic_Insomniac.ShoulderStrength = ComputeShoulderStrength();

// 			using ( Bitmap B = new Bitmap( 512, 512, PixelFormat.Format32bppArgb ) )
// 			{
// 				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, 512, 512 ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
// 
// 				for ( int Y=0; Y < 512; Y++ )
// 				{
// 					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
// 					for ( int X=0; X < 512; X++ )
// 					{
// 						double	Angle = Math.Atan2( Y - 255, X - 255 );
// //						byte	C = (byte) (255.0 * ((Angle + Math.PI) % (0.5 * Math.PI)) / (0.5 * Math.PI));
// 						byte	C = (byte) (255.0 * ((Angle + Math.PI) % Math.PI) / Math.PI);
// 						*pScanline++ = C;
// 						*pScanline++ = C;
// 						*pScanline++ = C;
// 						*pScanline++ = (byte) ((X-255)*(X-255) + (Y-255)*(Y-255) < 255*255 ? 255 : 0);
// 					}
// 				}
// 
// 				B.UnlockBits( LockedBitmap );
// 
// 				B.Save( "\\Anisotropy.png", ImageFormat.Png ); 
// 			}

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try {
				m_Device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Filmic Tone Mapping Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_AutoExposure = new ConstantBuffer<CB_AutoExposure>( m_Device, 10 );
			m_CB_ToneMapping = new ConstantBuffer<CB_ToneMapping>( m_Device, 10 );

			try {
				m_Shader_RenderHDR = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderCubeMap.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_Shader_ComputeTallHistogram = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/AutoExposure/ComputeTallHistogram.hlsl" ) ), "CS", null );
				m_Shader_FinalizeHistogram = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/AutoExposure/FinalizeHistogram.hlsl" ) ), "CS", null );
				m_Shader_ComputeAutoExposure = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/AutoExposure/ComputeAutoExposure.hlsl" ) ), "CS", null );
				m_Shader_ToneMapping = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/ToneMapping.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "Filmic Tone Mapping Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderHDR = null;
				m_Shader_ComputeTallHistogram = null;
				m_Shader_FinalizeHistogram = null;
				m_Shader_ComputeAutoExposure = null;
				m_Shader_ToneMapping = null;
			}

			// Create the HDR buffer
			m_Tex_HDR = new Texture2D( m_Device, panelOutput.Width, panelOutput.Height, 1, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, false, null );

			// Create the histogram & auto-exposure buffers
			int	tallHistogramHeight = (panelOutput.Height + 3) >> 2;
			m_Tex_TallHistogram = new Texture2D( m_Device, 128, tallHistogramHeight, 1, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_Histogram = new Texture2D( m_Device, 128, 1, 1, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Buffer_AutoExposureSource = new StructuredBuffer<autoExposure_t>( m_Device, 1, true );
			m_Buffer_AutoExposureSource.m[0].EngineLuminanceFactor = 1.0f;
			m_Buffer_AutoExposureSource.m[0].TargetLuminance = 1.0f;
			m_Buffer_AutoExposureSource.m[0].MinLuminanceLDR = 0.0f;
			m_Buffer_AutoExposureSource.m[0].MaxLuminanceLDR = 1.0f;
			m_Buffer_AutoExposureSource.m[0].MiddleGreyLuminanceLDR = 1.0f;
			m_Buffer_AutoExposureSource.m[0].EV = 0.0f;
			m_Buffer_AutoExposureSource.m[0].Fstop = 0.0f;
			m_Buffer_AutoExposureSource.m[0].PeakHistogramValue = 0;
			m_Buffer_AutoExposureSource.Write();
			m_Buffer_AutoExposureTarget = new StructuredBuffer<autoExposure_t>( m_Device, 1, true );

			// Load cube map
			try {
				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( "garage4_hd.dds" ) );
//				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\hdrcube6.dds" ) );
//				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_28_cube_BC6H_UF16.bimage" ) );		// Tunnel
// 				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_89_cube_BC6H_UF16.bimage" ) );		// Large sky
// 				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_115_cube_BC6H_UF16.bimage" ) );	// Indoor
// 				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_123_cube_BC6H_UF16.bimage" ) );	// Under the arch
// 				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_189_cube_BC6H_UF16.bimage" ) );	// Indoor viewing out (vista)
// 				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_246_cube_BC6H_UF16.bimage" ) );	// Nice! Statue's feet
// 				m_Tex_CubeMap = LoadCubeMap( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_248_cube_BC6H_UF16.bimage" ) );	// Nice! In a corner with lot of sky

			} catch ( Exception ) {
			}

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (90.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 0, 1 ), new float3( 0, 0, 0 ), float3.UnitY );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			if ( m_Shader_ToneMapping != null ) {
				m_Shader_ToneMapping.Dispose();
			}
			if ( m_Shader_ComputeAutoExposure != null ) {
				m_Shader_ComputeAutoExposure.Dispose();
			}
			if ( m_Shader_FinalizeHistogram != null ) {
				m_Shader_FinalizeHistogram.Dispose();
			}
			if ( m_Shader_ComputeTallHistogram != null ) {
				m_Shader_ComputeTallHistogram.Dispose();
			}
			if ( m_Shader_RenderHDR != null ) {
				m_Shader_RenderHDR.Dispose();
			}

			m_Buffer_AutoExposureTarget.Dispose();
			m_Buffer_AutoExposureSource.Dispose();
			m_Tex_Histogram.Dispose();
			m_Tex_TallHistogram.Dispose();
			m_Tex_HDR.Dispose();
			if ( m_Tex_CubeMap != null )
				m_Tex_CubeMap.Dispose();

			m_CB_AutoExposure.Dispose();
			m_CB_ToneMapping.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;
//			m_CB_Camera.m._Proj2World = m_CB_Camera.m._World2Proj.Inverse;

			m_CB_Camera.UpdateData();
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null )
				return;

			m_CB_Main.m._Resolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			DateTime	Now = DateTime.Now;
			float		DeltaTime = (float) (Now - m_lastTime).TotalSeconds;
			m_lastTime = Now;
			m_CB_Main.m._GlobalTime = (float) (Now - m_startTime).TotalSeconds;
			m_CB_Main.UpdateData();

			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			//////////////////////////////////////////////////////////////////////////
			// 1] Render to the HDR buffer
			if ( m_Shader_RenderHDR.Use() ) {
				m_Device.SetRenderTarget( m_Tex_HDR, null );
				if ( m_Tex_CubeMap != null )
					m_Tex_CubeMap.SetPS( 0 );
				m_Device.RenderFullscreenQuad( m_Shader_RenderHDR );
			}

			//////////////////////////////////////////////////////////////////////////
			// 2] Compute auto-exposure
			if ( m_Shader_ComputeTallHistogram.Use() ) {
				// Build a 128xH "tall histogram"
				m_Device.RemoveRenderTargets();
				m_Tex_HDR.SetCS( 0 );
				m_Tex_TallHistogram.SetCSUAV( 0 );
				m_Shader_ComputeTallHistogram.Dispatch( 1, m_Tex_TallHistogram.Height, 1 );
			}
			if ( m_Shader_FinalizeHistogram.Use() ) {
				// Build the 128x1 standard histogram
				m_Tex_Histogram.SetCSUAV( 0 );
				m_Tex_TallHistogram.SetCS( 0 );
				m_Shader_FinalizeHistogram.Dispatch( 128, 1, 1 );
			}
			if ( m_Shader_ComputeAutoExposure.Use() ) {
				// Compute auto-exposure from histogram and last value
				m_Buffer_AutoExposureSource.SetInput( 0 );
				m_Buffer_AutoExposureTarget.SetOutput( 0 );
				m_Tex_Histogram.SetCS( 1 );

				float	EV = floatTrackbarControlExposure.Value;

				m_CB_AutoExposure.m._delta_time = Math.Max( 0.01f, Math.Min( 1.0f, DeltaTime ) );
				if ( checkBoxEnable.Checked ) {
					m_CB_AutoExposure.m._white_level = tabControlToneMappingTypes.SelectedIndex == 0 ? floatTrackbarControlIG_WhitePoint.Value : floatTrackbarControlWhitePoint.Value;
				} else
					m_CB_AutoExposure.m._white_level = 1.0f;

				m_CB_AutoExposure.m._clip_shadows = 0.0f;				// (0.0) Shadow cropping in histogram (first buckets will be ignored, leading to brighter image)
				m_CB_AutoExposure.m._clip_highlights = 1.0f;			// (1.0) Highlights cropping in histogram (last buckets will be ignored, leading to darker image)
				m_CB_AutoExposure.m._EV = EV;							// (0.0) Your typical EV setting
				m_CB_AutoExposure.m._fstop_bias = 0.0f;					// (0.0) F-stop number bias to override automatic computation (NOTE: This will NOT change exposure, only the F number)
				m_CB_AutoExposure.m._reference_camera_fps = 30.0f;		// (30.0) Default camera at 30 FPS
				m_CB_AutoExposure.m._adapt_min_luminance = 0.1f;		// (0.03) Prevents the auto-exposure to adapt to luminances lower than this
				m_CB_AutoExposure.m._adapt_max_luminance = 3000.0f;		// (2000.0) Prevents the auto-exposure to adapt to luminances higher than this
				m_CB_AutoExposure.m._adapt_speed_up = 0.99f;			// (0.99) Adaptation speed from low to high luminances
				m_CB_AutoExposure.m._adapt_speed_down = 0.99f;			// (0.99) Adaptation speed from high to low luminances
				m_CB_AutoExposure.UpdateData();

				m_Shader_ComputeAutoExposure.Dispatch( 1, 1, 1 );

				// Swap source & target for next frame
				StructuredBuffer<autoExposure_t>	temp = m_Buffer_AutoExposureSource;
				m_Buffer_AutoExposureSource = m_Buffer_AutoExposureTarget;
				m_Buffer_AutoExposureTarget = temp;
			}

			//////////////////////////////////////////////////////////////////////////
			// 3] Apply tone mapping
			if ( m_Shader_ToneMapping.Use() ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );

				float	mouseU = 1.0f;
				float	mouseV = 0.0f;
				Point	clientMousePos = panelOutput.PointToClient( Control.MousePosition );
				if (	clientMousePos.X >= 0 && clientMousePos.X < panelOutput.Width
					&&  clientMousePos.Y >= 0 && clientMousePos.Y < panelOutput.Height ) {
					mouseU = (float) clientMousePos.X / panelOutput.Width;
					mouseV = (float) clientMousePos.Y / panelOutput.Height;
				}

				m_CB_ToneMapping.m._Exposure = 1.0f;//(float) Math.Pow( 2, floatTrackbarControlExposure.Value );
				m_CB_ToneMapping.m._Flags = (checkBoxEnable.Checked ? 1U : 0U)
										  | (checkBoxDebugLuminanceLevel.Checked ? 2U : 0U)
										  | (checkBoxShowHistogram.Checked ? 4U : 0U)
										  | (tabControlToneMappingTypes.SelectedIndex == 0 ? 0U : 8U)
										  | (checkBoxLuminanceOnly.Checked ? 16U : 0U);
				if ( tabControlToneMappingTypes.SelectedIndex == 0 ) {
					m_CB_ToneMapping.m._WhitePoint = floatTrackbarControlIG_WhitePoint.Value;
					m_CB_ToneMapping.m._A = floatTrackbarControlIG_BlackPoint.Value;
					m_CB_ToneMapping.m._B = Math.Min( m_CB_ToneMapping.m._WhitePoint-1e-3f, floatTrackbarControlIG_JunctionPoint.Value );
					m_CB_ToneMapping.m._C = ComputeToeStrength();// floatTrackbarControlIG_ToeStrength.Value;
					m_CB_ToneMapping.m._D = ComputeShoulderStrength();// floatTrackbarControlIG_ShoulderStrength.Value;

					// Compute junction factor
					float	b = m_CB_ToneMapping.m._A;
					float	w = m_CB_ToneMapping.m._WhitePoint;
					float	t = m_CB_ToneMapping.m._C;
					float	s = m_CB_ToneMapping.m._D;
					float	c = m_CB_ToneMapping.m._B;
					m_CB_ToneMapping.m._E = (1.0f - t) * (c - b) / ((1.0f - s) * (w - c) + (1.0f - t) * (c - b));
				} else {
					// Hable Filmic
					m_CB_ToneMapping.m._WhitePoint = floatTrackbarControlWhitePoint.Value;
					m_CB_ToneMapping.m._A = floatTrackbarControlA.Value;
					m_CB_ToneMapping.m._B = floatTrackbarControlB.Value;
					m_CB_ToneMapping.m._C = floatTrackbarControlC.Value;
					m_CB_ToneMapping.m._D = floatTrackbarControlD.Value;
					m_CB_ToneMapping.m._E = floatTrackbarControlE.Value;
					m_CB_ToneMapping.m._F = floatTrackbarControlF.Value;
				}
				m_CB_ToneMapping.m._DebugLuminanceLevel = floatTrackbarControlDebugLuminanceLevel.Value;
				m_CB_ToneMapping.m._MouseU = mouseU;
				m_CB_ToneMapping.m._MouseV = mouseV;
				m_CB_ToneMapping.UpdateData();

				m_Buffer_AutoExposureSource.SetInput( 0 );
				m_Tex_Histogram.SetPS( 1 );
				m_Tex_HDR.SetPS( 2 );
m_Tex_TallHistogram.SetPS( 3 );

				m_Device.RenderFullscreenQuad( m_Shader_ToneMapping );

m_Tex_TallHistogram.RemoveFromLastAssignedSlots();
				m_Tex_Histogram.RemoveFromLastAssignedSlots();
				m_Tex_HDR.RemoveFromLastAssignedSlots();
			}

			// Show!
			m_Device.Present( false );
		}

		#region EVENT HANDLERS

		private void floatTrackbarControlScaleX_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.ScaleX = _Sender.Value;
			outputPanelFilmic_Insomniac.ScaleX = _Sender.Value;
		}

		private void floatTrackbarControlScaleY_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.ScaleY = _Sender.Value;
			outputPanelFilmic_Insomniac.ScaleY = _Sender.Value;
		}

		private void tabControlToneMappingTypes_SelectedIndexChanged( object sender, EventArgs e )
		{
			outputPanelFilmic_Insomniac.Visible = tabControlToneMappingTypes.SelectedIndex == 0;
			panelGraph_Hable.Visible = tabControlToneMappingTypes.SelectedIndex == 1;
		}

		private void checkBoxLogLuminance_CheckedChanged( object sender, EventArgs e )
		{
			outputPanelFilmic_Insomniac.ShowLogLuminance = checkBoxLogLuminance.Checked;
			panelGraph_Hable.ShowLogLuminance = checkBoxLogLuminance.Checked;
		}

		#region Hable

		private void floatTrackbarControlWhitePoint_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlA_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.A = _Sender.Value;
		}

		private void floatTrackbarControlB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.B = _Sender.Value;
		}

		private void floatTrackbarControlC_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.C = _Sender.Value;
		}

		private void floatTrackbarControlD_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.D = _Sender.Value;
		}

		private void floatTrackbarControlE_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.E = _Sender.Value;
		}

		private void floatTrackbarControlF_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.F = _Sender.Value;
		}

		#endregion

		#region Insomniac Games

		private void floatTrackbarControlIG_BlackPoint_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanelFilmic_Insomniac.BlackPoint = _Sender.Value;
		}

		private void floatTrackbarControlIG_WhitePoint_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanelFilmic_Insomniac.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlIG_JunctionPoint_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanelFilmic_Insomniac.JunctionPoint = _Sender.Value;
		}

		float	ComputeToeStrength() {
//			return (float) Math.Pow( floatTrackbarControlIG_ToeStrength.Value, floatTrackbarControlTest.Value );
			return Math.Min( 0.999f, (float) Math.Pow( floatTrackbarControlIG_ToeStrength.Value, 2.0 ) );	// Empirical
		}

		private void floatTrackbarControlIG_ToeStrength_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanelFilmic_Insomniac.ToeStrength = ComputeToeStrength();
		}

		float	ComputeShoulderStrength() {
//			return (float) Math.Pow( floatTrackbarControlIG_ShoulderStrength.Value, floatTrackbarControlTest.Value );
			return Math.Min( 0.999f, (float) Math.Pow( floatTrackbarControlIG_ShoulderStrength.Value, 0.2 ) );	// Empirical
		}
		private void floatTrackbarControlIG_ShoulderStrength_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanelFilmic_Insomniac.ShoulderStrength = ComputeShoulderStrength();
		}

		private void floatTrackbarControlTest_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanelFilmic_Insomniac.ToeStrength = ComputeToeStrength();
		}

		#endregion

		private void checkBoxDebugLuminanceLevel_CheckedChanged( object sender, EventArgs e )
		{
			panelGraph_Hable.ShowDebugLuminance = checkBoxDebugLuminanceLevel.Checked;
		}

		private void floatTrackbarControlDebugLuminanceLevel_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph_Hable.DebugLuminance = _Sender.Value;
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}

		float	m_resetValue_IG_b;
		float	m_resetValue_IG_w;
		float	m_resetValue_IG_c;
		float	m_resetValue_IG_t;
		float	m_resetValue_IG_s;

		float	m_resetValue_Hable_A;
		float	m_resetValue_Hable_B;
		float	m_resetValue_Hable_C;
		float	m_resetValue_Hable_D;
		float	m_resetValue_Hable_E;
		float	m_resetValue_Hable_F;
		float	m_resetValue_Hable_W;
		void	SaveResetValues() {
			m_resetValue_IG_b = floatTrackbarControlIG_BlackPoint.Value;
			m_resetValue_IG_w = floatTrackbarControlIG_WhitePoint.Value;
			m_resetValue_IG_c = floatTrackbarControlIG_JunctionPoint.Value;
			m_resetValue_IG_t = floatTrackbarControlIG_ToeStrength.Value;
			m_resetValue_IG_s = floatTrackbarControlIG_ShoulderStrength.Value;

			m_resetValue_Hable_A = floatTrackbarControlA.Value;
			m_resetValue_Hable_B = floatTrackbarControlB.Value;
			m_resetValue_Hable_C = floatTrackbarControlC.Value;
			m_resetValue_Hable_D = floatTrackbarControlD.Value;
			m_resetValue_Hable_E = floatTrackbarControlE.Value;
			m_resetValue_Hable_F = floatTrackbarControlF.Value;
			m_resetValue_Hable_W = floatTrackbarControlWhitePoint.Value;
		}

		private void buttonReset_Click( object sender, EventArgs e )
		{
			if ( tabControlToneMappingTypes.SelectedIndex == 0 ) {
				floatTrackbarControlIG_BlackPoint.Value = m_resetValue_IG_b;
				 floatTrackbarControlIG_WhitePoint.Value = m_resetValue_IG_w;
				 floatTrackbarControlIG_JunctionPoint.Value = m_resetValue_IG_c;
				 floatTrackbarControlIG_ToeStrength.Value = m_resetValue_IG_t;
				 floatTrackbarControlIG_ShoulderStrength.Value = m_resetValue_IG_s;
			} else {
				floatTrackbarControlA.Value = m_resetValue_Hable_A;
				floatTrackbarControlB.Value = m_resetValue_Hable_B;
				floatTrackbarControlC.Value = m_resetValue_Hable_C;
				floatTrackbarControlD.Value = m_resetValue_Hable_D;
				floatTrackbarControlE.Value = m_resetValue_Hable_E;
				floatTrackbarControlF.Value = m_resetValue_Hable_F;
				floatTrackbarControlWhitePoint.Value = m_resetValue_Hable_W;
			}
		}

		#endregion

		private void panelOutput_DoubleClick( object sender, EventArgs e )
		{
			string	CubeMapName = m_AppKey.GetValue( "LastSelectedCubeMap", new System.IO.FileInfo( "garage4_hd.dds" ).FullName ) as string;
			openFileDialog.FileName = System.IO.Path.GetFileName( CubeMapName );
			openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName( CubeMapName );
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			System.IO.FileInfo	SelectedMapFile = new System.IO.FileInfo( openFileDialog.FileName );
			try
			{
				Texture2D	Tex_CubeMap = LoadCubeMap( SelectedMapFile );
				if ( Tex_CubeMap == null )
					throw new Exception( "Failed to create cube map!" );

				if ( m_Tex_CubeMap != null )
					m_Tex_CubeMap.Dispose();
				m_Tex_CubeMap = Tex_CubeMap;

				m_AppKey.SetValue( "LastSelectedCubeMap", SelectedMapFile.FullName );
			} catch ( Exception _e ) {
				MessageBox.Show( this, "An error occurred while opening cube map file \"" + SelectedMapFile.FullName + "\":\r\n\r\n" + _e.Message, "Film Tone Mapping Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		Texture2D LoadCubeMap( System.IO.FileInfo _FileName ) {
			if ( !_FileName.Exists )
				throw new Exception( "File not found!" );
			
			string	Ext = _FileName.Extension.ToLower();
			switch ( Ext ) {
				case ".dds":
					return DirectXTexManaged.TextureCreator.CreateTexture2DFromDDSFile( m_Device, _FileName.FullName );
				case ".bimage": {
					BImage I = new BImage( _FileName );
					return I.CreateTextureCube( m_Device );
				}
				default:
					throw new Exception( "Unsupported image extension!" );
			}
		}
	}
}
