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

namespace VoxelConeTracing
{
	public partial class VoxelForm : Form {

		#region CONSTANTS

		const float		CAMERA_FOV = 60.0f * (float) Math.PI / 180.0f;

		const int		VOLUME_VOXELS_COUNT = 512;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public uint			_resolutionX;
			public uint			_resolutionY;
			public float2		_tanHalfFOV;
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

		Device						m_device = new Device();

		ConstantBuffer< CB_Main >	m_CB_Main = null;
		ConstantBuffer< CB_Camera >	m_CB_camera = null;

		Shader						m_shader_RenderDistanceField = null;

		OctreeBuilder				m_octree = null;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();

		#endregion

		#region METHODS

		public VoxelForm() {
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			try {
				m_device.Init( outputPanel.Handle, false, true );

				m_shader_RenderDistanceField = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderDistanceField.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

				m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
				m_CB_camera = new ConstantBuffer<CB_Camera>( m_device, 1 );

				// Initialize camera
				m_camera.CreatePerspectiveCamera( CAMERA_FOV, (float) outputPanel.Width / outputPanel.Height, 0.1f, 100.0f );
				m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
				m_cameraManipulator.Attach( outputPanel, m_camera );
//				m_cameraManipulator.InitializeCamera( new float3( 0, 1.5f, 4.0f ), new float3( 0, 1.5f, 0.0f ), float3.UnitY );
				m_cameraManipulator.InitializeCamera( new float3( 0.0f, 2.73f, -8.0f ), new float3( 0.0f, 2.73f, 0.0f ), float3.UnitY );

				// Build static voxel
				m_octree = new OctreeBuilder( m_device, 5.6f * new float3( -0.5f, 0.0f, -0.5f ), 5.6f, 8 );

				m_startTime = DateTime.Now;
				Application.Idle += Application_Idle;

			} catch ( Exception _e ) {
				MessageBox.Show( this, "An error occurred while initializing objects:\r\n\r\n" + _e.Message );
			}
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

		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);

			Device	device = m_device;
			m_device = null;

			m_octree.Dispose();

			m_CB_Main.Dispose();
			m_CB_camera.Dispose();

			m_shader_RenderDistanceField.Dispose();

			device.Dispose();
		}

		DateTime	m_startTime;
		void Application_Idle(object sender, EventArgs e) {
			if ( m_device == null )
				return;

			// Update main CB
			m_CB_Main.m._resolutionX = (uint) outputPanel.Width;
			m_CB_Main.m._resolutionY = (uint) outputPanel.Height;
			m_CB_Main.m._tanHalfFOV.Set( m_camera.AspectRatio * (float) Math.Tan( 0.5 * CAMERA_FOV ), (float) Math.Tan( 0.5 * CAMERA_FOV ) );
			m_CB_Main.m._time = (float) (DateTime.Now - m_startTime).TotalSeconds;
			m_CB_Main.UpdateData();

			// Render pipo
			if ( m_shader_RenderDistanceField.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_device.RenderFullscreenQuad( m_shader_RenderDistanceField );
			}

			m_device.Present( false );
		}

		#endregion

		#region EVENT HANDLERS

		private void buttonReload_Click(object sender, EventArgs e) {
			m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
