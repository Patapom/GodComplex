#define BISOU

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;
using Renderer;
using Nuaj.Cirrus.Utility;

namespace TestWaveletATrousFiltering
{
	public partial class Form1 : Form {

		const float	CAMERA_FOV = (float) (90.0 * Math.PI / 180.0);

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Global {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public float	_time;
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
		private struct CB_PostProcess {
			public float		_lightSize;
		}

		#endregion

		Device						m_device = new Device();

		ConstantBuffer< CB_Global >	m_CB_global;
		ConstantBuffer< CB_Camera >	m_CB_camera = null;
		ConstantBuffer< CB_PostProcess >	m_CB_postProcess = null;

		Shader						m_shader_renderGBuffer;
		Shader						m_shader_postProcess;

		Texture2D					m_tex_GBuffer;
		Texture2D					m_Tex_BlueNoise;

		DateTime					m_startTime;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();


		public Form1() {
			InitializeComponent();

			try {
				m_device.Init( panelOutput.Handle, false, true );

				m_shader_renderGBuffer = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderGBuffer.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_postProcess = new Shader( m_device, new System.IO.FileInfo( "./Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

				m_tex_GBuffer = new Texture2D( m_device, (uint) panelOutput.Width, (uint) panelOutput.Height, 2, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.UNORM, false, false, null );

				using ( ImageFile blueNoise = new ImageFile( new System.IO.FileInfo( "BlueNoise64x64.png" ) ) ) {
					m_Tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] { { blueNoise } } ), COMPONENT_FORMAT.UNORM );
				}

				m_CB_global = new ConstantBuffer< CB_Global >( m_device, 0 );
				m_CB_camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
				m_CB_postProcess = new ConstantBuffer< CB_PostProcess >( m_device, 10 );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Error", "An exception occurred while creating DX structures:\r\n" + _e.Message );
			}

			// Initialize camera
			m_camera.CreatePerspectiveCamera( CAMERA_FOV, (float) panelOutput.Width / panelOutput.Height, 0.1f, 100.0f );
			m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
			m_cameraManipulator.Attach( panelOutput, m_camera );
//			m_cameraManipulator.InitializeCamera( new float3( 0, 1.5f, 4.0f ), new float3( 0, 1.5f, 0.0f ), float3.UnitY );
			m_cameraManipulator.InitializeCamera( new float3( 0.0f, 2.73f, -6.0f ), new float3( 0.0f, 2.73f, 0.0f ), float3.UnitY );

			m_startTime = DateTime.Now;
			Application.Idle += Application_Idle;
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			Device	D = m_device;
			m_device = null;
			D.Dispose();
		}

		void m_camera_CameraTransformChanged(object sender, EventArgs e) {
			m_CB_camera.m._Camera2World = m_camera.Camera2World;
			m_CB_camera.m._World2Camera = m_camera.World2Camera;
			m_CB_camera.m._Proj2World = m_camera.Proj2World;
			m_CB_camera.m._World2Proj = m_camera.World2Proj;
			m_CB_camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_camera.m._Proj2Camera = m_camera.Proj2Camera;
			m_CB_camera.UpdateData();
		}

		void Application_Idle(object sender, EventArgs e) {
			if ( m_device == null )
				return;

			DateTime	currentTime = DateTime.Now;
			float		totalTime = (float) (currentTime - m_startTime).TotalSeconds;
			m_CB_global.m._resolutionX = (uint) panelOutput.Width;
			m_CB_global.m._resolutionY = (uint) panelOutput.Height;
			m_CB_global.m._time = totalTime;
			m_CB_global.UpdateData();

			m_CB_postProcess.m._lightSize = floatTrackbarControlLightSize.Value;
			m_CB_postProcess.UpdateData();

			//////////////////////////////////////////////////////////////////////////
			// Render the G-Buffer (albedo + gloss + normal + distance)
			if ( m_shader_renderGBuffer.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTargets( new IView[] { m_tex_GBuffer.GetView( 0, 1, 0, 1 ), m_tex_GBuffer.GetView( 0, 1, 1, 1 ) }, null );
				m_device.RenderFullscreenQuad( m_shader_renderGBuffer );
			}

			//////////////////////////////////////////////////////////////////////////
			//
			if ( m_shader_postProcess.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_GBuffer.Set( 0 );
				m_Tex_BlueNoise.Set( 1 );

				m_device.RenderFullscreenQuad( m_shader_postProcess );

				m_tex_GBuffer.RemoveFromLastAssignedSlots();
			}

			m_device.Present( false );
		}

		private void buttonReload_Click(object sender, EventArgs e) {
			m_device.ReloadModifiedShaders();
		}
	}
}
