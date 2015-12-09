using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using RendererManaged;
using Nuaj.Cirrus.Utility;

namespace TestFilmicCurve
{
	public partial class Form1 : Form
	{
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
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct autoExposure_t {
			public float	EngineLuminanceFactor;		// The actual factor to apply to values stored to the HDR render target (it's simply LuminanceFactor * WORLD_TO_BISOU_LUMINANCE so it's a division by about 100)
			public float	LuminanceFactor;			// The factor to apply to the HDR luminance to bring it to the LDR luminance (warning: still in world units, you must multiply by WORLD_TO_BISOU_LUMINANCE for a valid engine factor)
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

// 		private StructuredBuffer<UInt32>			m_Buffer_TallHistogram = null;
// 		private StructuredBuffer<UInt32>			m_Buffer_Histogram = null;
		private Texture2D							m_Tex_TallHistogram = null;
		private Texture2D							m_Tex_Histogram = null;
		private StructuredBuffer<autoExposure_t>	m_Buffer_AutoExposureSource = null;
		private StructuredBuffer<autoExposure_t>	m_Buffer_AutoExposureTarget = null;

		private Texture2D							m_Tex_HDR = null;
		private Texture2D							m_Tex_CubeMap = null;

		private Camera								m_Camera = new Camera();
		private CameraManipulator					m_Manipulator = new CameraManipulator();

		private DateTime							m_startTime = DateTime.Now;

		public unsafe Form1()
		{
			InitializeComponent();

			panelGraph.ScaleX = floatTrackbarControlScaleX.Value;
			panelGraph.ScaleY = floatTrackbarControlScaleY.Value;
			
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
// 			m_Buffer_TallHistogram = new StructuredBuffer<uint>( m_Device, 128, true );
// 			m_Buffer_Histogram = new StructuredBuffer<uint>( m_Device, 128, true );
			m_Tex_TallHistogram = new Texture2D( m_Device, 128, tallHistogramHeight, 1, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_Histogram = new Texture2D( m_Device, 128, 1, 1, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Buffer_AutoExposureSource = new StructuredBuffer<autoExposure_t>( m_Device, 1, true );
			m_Buffer_AutoExposureTarget = new StructuredBuffer<autoExposure_t>( m_Device, 1, true );

			// Load cube map
//			m_Tex_CubeMap = DirectXTexManaged.TextureCreator.CreateTexture2DFromDDSFile( m_Device, "garage4_hd.dds" );
			{
				BImage I = new BImage( new System.IO.FileInfo( @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_1127_cube_BC6H_UF16.bimage" ) );
				m_Tex_CubeMap = I.CreateTextureCube( m_Device );
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
// 			m_Buffer_Histogram.Dispose();
// 			m_Buffer_TallHistogram.Dispose();
			m_Tex_Histogram.Dispose();
			m_Tex_TallHistogram.Dispose();
			m_Tex_HDR.Dispose();
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

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			m_CB_Main.m._Resolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			m_CB_Main.m._GlobalTime = (float) (DateTime.Now - m_startTime).TotalSeconds;
			m_CB_Main.UpdateData();

			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			//////////////////////////////////////////////////////////////////////////
			// 1] Render to the HDR buffer
			if ( m_Shader_RenderHDR.Use() ) {
				m_Device.SetRenderTarget( m_Tex_HDR, null );
				m_Tex_CubeMap.SetPS( 0 );
				m_Device.RenderFullscreenQuad( m_Shader_RenderHDR );
			}

			//////////////////////////////////////////////////////////////////////////
			// 2] Compute auto-exposure
			if ( m_Shader_ComputeTallHistogram.Use() ) {
				m_Device.RemoveRenderTargets();
				m_Tex_HDR.SetCS( 0 );
				m_Tex_TallHistogram.SetCSUAV( 0 );
				m_Shader_ComputeTallHistogram.Dispatch( 1, m_Tex_TallHistogram.Height, 1 );
			}
			if ( m_Shader_FinalizeHistogram.Use() ) {
				m_Tex_Histogram.SetCSUAV( 0 );
				m_Tex_TallHistogram.SetCS( 0 );
				m_Shader_FinalizeHistogram.Dispatch( 128, 1, 1 );
			}
			if ( m_Shader_ComputeAutoExposure.Use() ) {
				m_Buffer_AutoExposureSource.SetInput( 0 );
				m_Buffer_AutoExposureTarget.SetOutput( 0 );
				m_Tex_Histogram.SetCS( 1 );

				float	EV = floatTrackbarControlExposure.Value;

				m_CB_AutoExposure.m._clip_shadows = 0.0f;				// (0.0) Shadow cropping in histogram (first buckets will be ignored, leading to brighter image)
				m_CB_AutoExposure.m._clip_highlights = 1.0f;			// (1.0) Highlights cropping in histogram (last buckets will be ignored, leading to darker image)
				m_CB_AutoExposure.m._EV = EV;							// (0.0) Your typical EV setting
				m_CB_AutoExposure.m._fstop_bias = 0.0f;					// (0.0) F-stop number bias to override automatic computation (NOTE: This will NOT change exposure, only the F number)
				m_CB_AutoExposure.m._reference_camera_fps = 30.0f;		// (30.0) Default camera at 30 FPS
				m_CB_AutoExposure.m._adapt_min_luminance = 0.1f;		// (0.03) Prevents the auto-exposure to adapt to luminances lower than this
				m_CB_AutoExposure.m._adapt_max_luminance = 2000.0f;		// (2000.0) Prevents the auto-exposure to adapt to luminances higher than this
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

				m_CB_ToneMapping.m._Exposure = (float) Math.Pow( 2, floatTrackbarControlExposure.Value );
				m_CB_ToneMapping.m._Flags = (checkBoxEnable.Checked ? 1U : 0U) | (checkBoxDebugLuminanceLevel.Checked ? 2U : 0U);
				m_CB_ToneMapping.m._A = floatTrackbarControlA.Value;
				m_CB_ToneMapping.m._B = floatTrackbarControlB.Value;
				m_CB_ToneMapping.m._C = floatTrackbarControlC.Value;
				m_CB_ToneMapping.m._D = floatTrackbarControlD.Value;
				m_CB_ToneMapping.m._E = floatTrackbarControlE.Value;
				m_CB_ToneMapping.m._F = floatTrackbarControlF.Value;
				m_CB_ToneMapping.m._WhitePoint = floatTrackbarControlWhitePoint.Value;
				m_CB_ToneMapping.m._DebugLuminanceLevel = floatTrackbarControlDebugLuminanceLevel.Value;
				m_CB_ToneMapping.UpdateData();

				m_Buffer_AutoExposureSource.SetInput( 0 );
				m_Tex_Histogram.SetCS( 1 );
				m_Tex_HDR.SetPS( 2 );

				m_Device.RenderFullscreenQuad( m_Shader_ToneMapping );

				m_Tex_Histogram.RemoveFromLastAssignedSlots();
				m_Tex_HDR.RemoveFromLastAssignedSlots();
			}

			// Show!
			m_Device.Present( false );
		}

		#region EVENT HANDLERS

		private void floatTrackbarControlScaleX_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleX = _Sender.Value;
		}

		private void floatTrackbarControlScaleY_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleY = _Sender.Value;
		}

		private void floatTrackbarControlWhitePoint_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlA_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.A = _Sender.Value;
		}

		private void floatTrackbarControlB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.B = _Sender.Value;
		}

		private void floatTrackbarControlC_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.C = _Sender.Value;
		}

		private void floatTrackbarControlD_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.D = _Sender.Value;
		}

		private void floatTrackbarControlE_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.E = _Sender.Value;
		}

		private void floatTrackbarControlF_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.F = _Sender.Value;
		}

		private void checkBoxDebugLuminanceLevel_CheckedChanged( object sender, EventArgs e )
		{
			panelGraph.ShowDebugLuminance = checkBoxDebugLuminanceLevel.Checked;
		}

		private void floatTrackbarControlDebugLuminanceLevel_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.DebugLuminance = _Sender.Value;
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}

		private void buttonReset_Click( object sender, EventArgs e )
		{
			floatTrackbarControlA.Value = 0.15f;
			floatTrackbarControlB.Value = 0.5f;
			floatTrackbarControlC.Value = 0.1f;
			floatTrackbarControlD.Value = 0.2f;
			floatTrackbarControlE.Value = 0.02f;
			floatTrackbarControlF.Value = 0.3f;
			floatTrackbarControlWhitePoint.Value = 10f;
		}

		#endregion
	}
}
