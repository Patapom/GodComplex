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
// 			public float		iGlobalTime;
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
		}

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_ToneMapping>	m_CB_ToneMapping = null;

		private Shader				m_Shader_ToneMapping = null;
		private Texture2D			m_Tex_CubeMap = null;

		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

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
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_ToneMapping = new ConstantBuffer<CB_ToneMapping>( m_Device, 10 );

			try {
				m_Shader_ToneMapping = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/ToneMapping.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"ToneMapping\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_ToneMapping = null;
			}

			// Load cube map
			m_Tex_CubeMap = DirectXTexManaged.TextureCreator.CreateTexture2DFromDDSFile( m_Device, "garage4_hd.dds" );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (90.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 0, 1 ), new float3( 0, 0, 0 ), float3.UnitY );
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( m_Shader_ToneMapping != null ) {
				m_Shader_ToneMapping.Dispose();
			}

			m_Tex_CubeMap.Dispose();

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
			m_CB_Main.UpdateData();

			m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_Shader_ToneMapping.Use() ) {

				m_Tex_CubeMap.SetPS( 0 );

				m_CB_ToneMapping.m._Exposure = (float) Math.Pow( 2, floatTrackbarControlExposure.Value );
				m_CB_ToneMapping.m._Flags = checkBoxEnable.Checked ? 1U : 0U;
				m_CB_ToneMapping.m._A = floatTrackbarControlA.Value;
				m_CB_ToneMapping.m._B = floatTrackbarControlB.Value;
				m_CB_ToneMapping.m._C = floatTrackbarControlC.Value;
				m_CB_ToneMapping.m._D = floatTrackbarControlD.Value;
				m_CB_ToneMapping.m._E = floatTrackbarControlE.Value;
				m_CB_ToneMapping.m._F = floatTrackbarControlF.Value;
				m_CB_ToneMapping.m._WhitePoint = floatTrackbarControlWhitePoint.Value;
				m_CB_ToneMapping.UpdateData();

				m_Device.RenderFullscreenQuad( m_Shader_ToneMapping );
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
