using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using Renderer;
using SharpMath;
using Nuaj.Cirrus.Utility;
using Nuaj.Cirrus;

namespace AreaLightTest {
	public partial class AreaLightForm : Form {

		#region CONSTANTS

		readonly float3		PLANE_NORMAL = float3.UnitY;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_camera2World;
			public float4x4		_world2Camera;
			public float4x4		_proj2World;
			public float4x4		_world2Proj;
			public float4x4		_camera2Proj;
			public float4x4		_proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Light {
			public float4x4		_wsLight2World;
			public float		_illuminance;
		}

		#endregion

		#region FIELDS

		private Device				m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_Light>		m_CB_Light = null;

		private Shader				m_shader_RenderScene = null;

//		private Texture2D			m_tex_FalseColors = null;


		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		private float3				m_wsLightTargetPosition = float3.Zero;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double						m_ticks2Seconds;
		public float						m_startTime = 0;
		public float						m_CurrentTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		#endregion

		#region METHODS

		public AreaLightForm() {
			InitializeComponent();
		}

		#region Open/Close

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			buttonRebuildBRDF_Click( null, EventArgs.Empty );

//			m_tex_FalseColors = Image2Texture( new System.IO.FileInfo( "FalseColorsSpectrum.png" ), ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
			m_CB_Light = new ConstantBuffer<CB_Light>( m_device, 2 );

			try {
				m_shader_RenderScene = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			// Setup camera
			m_camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_manipulator.Attach( panelOutput, m_camera );
			m_manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
			m_manipulator.EnableMouseAction += m_manipulator_EnableMouseAction;

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

			if ( m_shader_RenderScene != null ) {
				m_shader_RenderScene.Dispose();
			}

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Light.Dispose();

//			m_tex_FalseColors.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_stopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_ticks2Seconds);
			return Time;
		}

		#region Manipulation

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._camera2World = m_camera.Camera2World;
			m_CB_Camera.m._world2Camera = m_camera.World2Camera;

			m_CB_Camera.m._camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._proj2Camera = m_CB_Camera.m._camera2Proj.Inverse;

			m_CB_Camera.m._world2Proj = m_CB_Camera.m._world2Camera * m_CB_Camera.m._camera2Proj;
			m_CB_Camera.m._proj2World = m_CB_Camera.m._proj2Camera * m_CB_Camera.m._camera2World;

			m_CB_Camera.UpdateData();
		}

		private bool m_manipulator_EnableMouseAction( MouseEventArgs _e ) {
			return (Control.ModifierKeys & Keys.LControlKey) == Keys.None;
		}

		bool	m_manipulatingLight = false;
		Point	m_buttonDownPosition;
		private void panelOutput_MouseDown( object sender, MouseEventArgs _e ) {
			if ( (Control.ModifierKeys & Keys.LControlKey) == Keys.None )
				return;

//			if ( (_e.Button & MouseButtons.Left) == MouseButtons.Left )
			if ( (_e.Button & MouseButtons.Left) != 0 )
				m_manipulatingLight = true;
			m_buttonDownPosition = _e.Location;
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs _e ) {
			if ( (_e.Button & MouseButtons.Left) == 0 ) {
				m_manipulatingLight = false;
				m_buttonDownPosition = _e.Location;
			}
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs _e ) {
			if ( !m_manipulatingLight )
				return;

			float3	csView = new float3( 2.0f * new float2( _e.X, _e.Y ) / m_CB_Main.m.iResolution.y - float2.One, 1.0f );
					csView.y = -csView.y;
			
			float	Z2Length = csView.Length;
					csView /= Z2Length;

			float3	wsView =  (new float4( csView, 0 ) * m_CB_Camera.m._camera2World ).xyz;
			float3	wsCamPos = m_CB_Camera.m._camera2World.r3.xyz;

			float	t = -wsCamPos.y / wsView.y;
			if ( t <= 0.0f )
				return;	// No intersection with the plane...

			m_wsLightTargetPosition = wsCamPos + t * wsView;
		}

		#endregion

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			m_CB_Main.m.iGlobalTime = GetGameTime() - m_startTime;
			m_CB_Main.UpdateData();

			// Setup light data
			float3	wsLightPosition = new float3( floatTrackbarControlLightPosX.Value, floatTrackbarControlLightPosY.Value, floatTrackbarControlLightPosZ.Value );
			float3	at = (m_wsLightTargetPosition - wsLightPosition).Normalized;
			float	roll = Mathf.ToRad( floatTrackbarControlLightRoll.Value );
			float3	left, up;
			at.OrthogonalBasis( out left, out up );

			float3	axisX =  Mathf.Cos( roll ) * left + Mathf.Sin( roll ) * up;
			float3	axisY = -Mathf.Sin( roll ) * left + Mathf.Cos( roll ) * up;

			float	radiusX = floatTrackbarControlLightScaleX.Value;
			float	radiusY = floatTrackbarControlLightScaleY.Value;

			m_CB_Light.m._illuminance = floatTrackbarControlIlluminance.Value;
			m_CB_Light.m._wsLight2World.r0.Set( axisX, radiusX );
			m_CB_Light.m._wsLight2World.r1.Set( axisY, radiusY );
			m_CB_Light.m._wsLight2World.r2.Set( at, Mathf.PI * radiusX * radiusY );	// Disk area in W
			m_CB_Light.m._wsLight2World.r3.Set( wsLightPosition, 1 );
			m_CB_Light.UpdateData();


			// =========== Render scene ===========
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

// 			m_device.Clear( m_device.DefaultTarget, float4.Zero );
// 			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

			if ( m_shader_RenderScene != null && m_shader_RenderScene.Use() ) {

				m_device.RenderFullscreenQuad( m_shader_RenderScene );
			} else {
				m_device.Clear( new float4( 1, 0, 0, 0 ) );
			}

			// Show!
			m_device.Present( false );

			// Update window text
//			Text = "Zombizous Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
		}

		private void buttonRebuildBRDF_Click( object sender, EventArgs e ) {
		}

		private void checkBoxUseTexture_CheckedChanged( object sender, EventArgs e ) {
		}

		#endregion
	}
}
