using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using SharpMath;
using ImageUtility;
using Renderer;
using Nuaj.Cirrus.Utility;
using Nuaj.Cirrus;

namespace TestHBIL {
	public partial class TestHBILForm : Form {

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		_resolution;
			public float		_time;
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

		#endregion

		#region FIELDS

		private Device				m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;

		private Shader				m_shader_RenderScene_DepthPass = null;

		private Shader				m_shader_PostProcess = null;

		// G-Buffer
		private Texture2D			m_tex_albedo = null;
		private Texture2D			m_tex_normal = null;
		private Texture2D			m_tex_motionVectors = null;

		private Texture2D			m_tex_BlueNoise = null;

		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double						m_ticks2Seconds;
		public float						m_startTime = 0;
		public float						m_currentTime = 0;
		public float						m_deltaTime = 0;		// Delta time used for the current frame

		#endregion

		#region METHODS

		public TestHBILForm() {
			InitializeComponent();
		}

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _fileName, COMPONENT_FORMAT _componentFormat ) {
			ImagesMatrix	images = null;
			if ( _fileName.Extension.ToLower() == ".dds" ) {
				images = ImageFile.DDSLoadFile( _fileName );
			} else {
				ImageFile	image = new ImageFile( _fileName );
				if ( image.PixelFormat != PIXEL_FORMAT.BGRA8 ) {
					ImageFile	badImage = image;
					image = new ImageFile();
					image.ConvertFrom( badImage, PIXEL_FORMAT.BGRA8 );
					badImage.Dispose();
				}
				images = new ImagesMatrix( new ImageFile[1,1] { { image } } );
			}
			return new Texture2D( m_device, images, _componentFormat );
		}

		#endregion

		#region Init/Exit

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			uint	W = (uint) panelOutput.Width;
			uint	H = (uint) panelOutput.Height;

			try {
				m_device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Main.m._resolution.Set( W, H );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );

			try {
				m_shader_RenderScene_DepthPass = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_Depth", null );
				m_shader_PostProcess = new Shader( m_device, new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			// Create buffers
			m_tex_albedo = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_normal = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_motionVectors = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RG8, COMPONENT_FORMAT.SNORM, false, false, null );

			// Create textures
			using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/BlueNoise64x64.png" ) ) )
				using ( ImageFile Imono = new ImageFile() ) {
					Imono.ConvertFrom( I, PIXEL_FORMAT.R8 );
					m_tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );
				}

			// Setup camera
			m_camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_manipulator.Attach( panelOutput, m_camera );
//			m_manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
			m_manipulator.InitializeCamera( new float3( 0, -4.5f, 5.0f ), new float3( 0, 0, 0 ), float3.UnitY );

			// Start game time
			m_ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_stopWatch.Start();
			m_startTime = GetGameTime();

			m_camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_device == null )
				return;

			m_tex_BlueNoise.Dispose();
			m_tex_motionVectors.Dispose();
			m_tex_normal.Dispose();
			m_tex_albedo.Dispose();

			m_shader_PostProcess.Dispose();
			m_shader_RenderScene_DepthPass.Dispose();

			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			// Setup global data
			if ( checkBoxAnimate.Checked )
				m_CB_Main.m._time = GetGameTime() - m_startTime;
			m_CB_Main.UpdateData();

			//////////////////////////////////////////////////////////////////////////
			// =========== Render Depth Pass ===========
//			m_device.DefaultDepthStencil
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.WRITE_ALWAYS, BLEND_STATE.DISABLED );

			if ( m_shader_RenderScene_DepthPass.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_albedo.GetView( 0, 1, 0, 1 ), m_tex_normal.GetView( 0, 1, 0, 1 ), m_tex_motionVectors.GetView( 0, 1, 0, 1 ) }, m_device.DefaultDepthStencil );
				m_device.RenderFullscreenQuad( m_shader_RenderScene_DepthPass );
			}

			//////////////////////////////////////////////////////////////////////////
			// =========== Post-Process ===========
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_PostProcess.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_albedo.SetPS( 0 );
				m_tex_normal.SetPS( 1 );
				m_tex_motionVectors.SetPS( 2 );
				m_device.DefaultDepthStencil.SetPS( 3 );

				m_device.RenderFullscreenQuad( m_shader_PostProcess );

				m_device.DefaultDepthStencil.RemoveFromLastAssignedSlots();
				m_tex_motionVectors.RemoveFromLastAssignedSlots();
				m_tex_normal.RemoveFromLastAssignedSlots();
				m_tex_albedo.RemoveFromLastAssignedSlots();
			}

			// Show!
			m_device.Present( false );


			// Update window text
//			Text = "Test HBIL Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_stopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_ticks2Seconds);
			return Time;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
