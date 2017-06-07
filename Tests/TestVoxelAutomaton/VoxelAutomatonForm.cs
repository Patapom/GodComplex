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
using Renderer;
using ImageUtility;
using Nuaj.Cirrus.Utility;

namespace TestVoxelAutomaton {
	public partial class VoxelAutomatonForm : Form {

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
 		struct CB_Main {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public float	_time;
			public float	_glossRoom;
			public float	_glossSphere;
			public float	_noiseInfluence;
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

		private Device								m_device = new Device();

		private Shader								m_Shader_renderWall = null;
		private Shader								m_Shader_renderGBuffer = null;
		private Shader								m_Shader_renderScene = null;

		private ConstantBuffer< CB_Main >			m_CB_Main;
		private ConstantBuffer<CB_Camera>			m_CB_Camera = null;

		private Texture2D							m_Tex_BlueNoise = null;
		private Texture2D							m_Tex_Wall = null;
		private Texture2D							m_Tex_GBuffer = null;

		private Camera								m_camera = new Camera();
		private CameraManipulator					m_manipulator = new CameraManipulator();

		#endregion

		public VoxelAutomatonForm() {
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput3D.Handle, false, true );

				m_Shader_renderWall = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderWall.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_Shader_renderGBuffer = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderGBuffer.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_Shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				
				m_CB_Main = new ConstantBuffer< CB_Main >( m_device, 0 );
				m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );

				using ( ImageFile blueNoise = new ImageFile( new System.IO.FileInfo( "BlueNoise64x64.png" ) ) ) {
					m_Tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] { { blueNoise } } ), COMPONENT_FORMAT.UNORM );
				}
				m_Tex_Wall = new Texture2D( m_device, 64, 64, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.AUTO, false, false, null );
				m_Tex_GBuffer = new Texture2D( m_device, m_device.DefaultTarget.Width, m_device.DefaultTarget.Height, 2, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Exception: " + _e.Message, "Path Tracing Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			m_startTime = DateTime.Now;

			// Initialize camera
			m_camera.CreatePerspectiveCamera( 60.0f * (float) Math.PI / 180.0f, (float) panelOutput3D.Width / panelOutput3D.Height, 0.01f, 10.0f );
			m_manipulator.Attach( panelOutput3D, m_camera );
			m_manipulator.InitializeCamera( new float3( 0, 0, -0.5f ), float3.Zero, float3.UnitY );
			m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
			m_camera_CameraTransformChanged( null, EventArgs.Empty );

			Application.Idle += Application_Idle;
		}

		protected override void OnClosing( CancelEventArgs e ) {
			base.OnClosing( e );

			Device	D = m_device;
			m_device = null;

			m_Tex_GBuffer.Dispose();
			m_Tex_Wall.Dispose();
			m_Tex_BlueNoise.Dispose();

			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_Shader_renderScene.Dispose();
			m_Shader_renderGBuffer.Dispose();
			m_Shader_renderWall.Dispose();

			D.Dispose();
		}

		void m_camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_camera.Proj2Camera;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;
//			m_CB_Camera.m._Proj2World = m_CB_Camera.m._World2Proj.Inverse;

			m_CB_Camera.UpdateData();
		}

		DateTime	m_startTime;

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			DateTime	currentTime = DateTime.Now;
			m_CB_Main.m._resolutionX = (uint) panelOutput3D.Width;
			m_CB_Main.m._resolutionY = (uint) panelOutput3D.Height;
			m_CB_Main.m._time = (float) (currentTime - m_startTime).TotalSeconds;
			m_CB_Main.m._glossRoom = floatTrackbarControlGlossWall.Value;
			m_CB_Main.m._glossSphere = floatTrackbarControlGlossSphere.Value;
			m_CB_Main.m._noiseInfluence = floatTrackbarControlNoise.Value;
			m_CB_Main.UpdateData();

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			m_Tex_BlueNoise.SetPS( 2 );

			//////////////////////////////////////////////////////////////////////////
			// Render the wall texture
			if ( m_Shader_renderWall.Use() ) {
				m_device.SetRenderTarget( m_Tex_Wall, null );
				m_device.RenderFullscreenQuad( m_Shader_renderWall );
			}

			//////////////////////////////////////////////////////////////////////////
			// Render the G-Buffer
			if ( m_Shader_renderGBuffer.Use() ) {
				m_device.SetRenderTargets( m_Tex_GBuffer.Width, m_Tex_GBuffer.Height, new IView[] { m_Tex_GBuffer.GetView( 0, 1, 0, 1 ), m_Tex_GBuffer.GetView( 0, 1, 1, 1 ) }, null );
				m_device.RenderFullscreenQuad( m_Shader_renderGBuffer );
			}

			//////////////////////////////////////////////////////////////////////////
			// Path trace the scene
			if ( m_Shader_renderScene.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );
				m_Tex_GBuffer.SetPS( 0 );
				m_Tex_Wall.SetPS( 1 );

				m_device.RenderFullscreenQuad( m_Shader_renderScene );

				m_Tex_GBuffer.RemoveFromLastAssignedSlots();
				m_Tex_Wall.RemoveFromLastAssignedSlots();
			}

			m_device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}
	}
}
