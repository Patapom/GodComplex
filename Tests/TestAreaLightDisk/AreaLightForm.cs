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

		const float			FOV_DEGREES = 60.0f;
		readonly float3		PLANE_NORMAL = float3.UnitY;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		iResolution;
			public float		tanHalfFOV;
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
			public float		_luminance;
		}

		#endregion

		#region FIELDS

		private Device				m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_Light>		m_CB_Light = null;

		private Shader				m_shader_RenderLight = null;
		private Shader				m_shader_RenderScene = null;
		private Shader				m_shader_RenderScene_Reference = null;

		private Primitive			m_prim_disk = null;

//		private Texture2D			m_tex_FalseColors = null;


		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		private float3				m_wsLightTargetPosition = float3.Zero;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double				m_ticks2Seconds;
		public float				m_startTime = 0;
		public float				m_CurrentTime = 0;
		public float				m_DeltaTime = 0;		// Delta time used for the current frame

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

//			m_tex_FalseColors = Image2Texture( new System.IO.FileInfo( "FalseColorsSpectrum.png" ), ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
			m_CB_Light = new ConstantBuffer<CB_Light>( m_device, 2 );

			try {
				m_shader_RenderLight = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderLight.hlsl" ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null );;
				m_shader_RenderScene = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
				m_shader_RenderScene_Reference = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene_Reference.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			BuildPrimitives();

			// Setup camera
			m_camera.CreatePerspectiveCamera( (float) (FOV_DEGREES * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
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

			m_shader_RenderLight.Dispose();
			m_shader_RenderScene.Dispose();
			m_shader_RenderScene_Reference.Dispose();

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Light.Dispose();

			m_prim_disk.Dispose();

//			m_tex_FalseColors.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		const int		VERTICES_COUNT = 1024;

		void		BuildPrimitives() {
			VertexP3N3[]	vertices = new VertexP3N3[2*VERTICES_COUNT];
			uint[]			indices = new uint[3*VERTICES_COUNT];
			for ( int i=0; i < VERTICES_COUNT; i++ ) {
				float	a = Mathf.TWOPI * i / VERTICES_COUNT;
				float3	P = new float3( Mathf.Cos( a ), Mathf.Sin( a ), 0 );

				vertices[2*i+0].P = P;
				vertices[2*i+0].N = new float3( (float) i / VERTICES_COUNT, 0, 0 );
				vertices[2*i+1].P = P;
				vertices[2*i+1].N = new float3( (float) i / VERTICES_COUNT, 1, 0 );

				indices[3*i+0] = (uint) (2*i+0);
				indices[3*i+1] = (uint) (2*i+1);
				indices[3*i+2] = (uint) (2*((i+1) % VERTICES_COUNT) + 1);
			}

			m_prim_disk = new Primitive( m_device, 2*VERTICES_COUNT, VertexP3N3.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3 );
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

		float3	ComputeCameraRay( PointF _clienPosition ) {
			float3	pos;
			float	f;
			return ComputeCameraRay( _clienPosition, out pos, out f );
		}
		float3	ComputeCameraRay( PointF _clienPosition, out float3 _csView, out float _Z2Length ) {
			_csView = new float3( m_CB_Main.m.tanHalfFOV * (2.0f * new float2( _clienPosition.X, _clienPosition.Y ) / m_CB_Main.m.iResolution - float2.One), 1.0f );
			_csView.x *= m_CB_Main.m.iResolution.x / m_CB_Main.m.iResolution.y;
			_csView.y = -_csView.y;
			
			_Z2Length = _csView.Length;
			_csView /= _Z2Length;

			float3	wsView =  (new float4( _csView, 0 ) * m_CB_Camera.m._camera2World ).xyz;
			return wsView;
		}

		private bool m_manipulator_EnableMouseAction( MouseEventArgs _e ) {
			return (Control.ModifierKeys & Keys.Control) == Keys.None;
		}

		bool	m_manipulatingLight = false;
		Point	m_buttonDownPosition;
		private void panelOutput_MouseDown( object sender, MouseEventArgs _e ) {
			if ( _e.Button == MouseButtons.Right ) {
				float3	wsCamPos = m_CB_Camera.m._camera2World.r3.xyz;
				float3	wsView = ComputeCameraRay( _e.Location );
				float	t = -wsCamPos.y / wsView.y;
				if ( t > 0.0f )
					ComputeResult( wsCamPos + t * wsView );
			}

			if ( (Control.ModifierKeys & Keys.Control) == Keys.None )
				return;

//			if ( (_e.Button & MouseButtons.Left) == MouseButtons.Left )
			if ( (_e.Button & MouseButtons.Left) != 0 )
				m_manipulatingLight = true;
			m_buttonDownPosition = _e.Location;
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs _e ) {
			if ( (_e.Button & MouseButtons.Left) != 0 ) {
				m_manipulatingLight = false;
				m_buttonDownPosition = _e.Location;
			}
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs _e ) {
			if ( !m_manipulatingLight )
				return;

			float3	wsCamPos = m_CB_Camera.m._camera2World.r3.xyz;
			float3	wsView = ComputeCameraRay( _e.Location );
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
			m_CB_Main.m.iResolution = new float2( panelOutput.Width, panelOutput.Height );
			m_CB_Main.m.tanHalfFOV = Mathf.Tan( 0.5f * Mathf.ToRad( FOV_DEGREES ) );
			m_CB_Main.m.iGlobalTime = GetGameTime() - m_startTime;
			m_CB_Main.UpdateData();

			// Setup light data
			float3	wsLightPosition = new float3( floatTrackbarControlLightPosX.Value, floatTrackbarControlLightPosY.Value, floatTrackbarControlLightPosZ.Value );
			float3	at = (m_wsLightTargetPosition - wsLightPosition).Normalized;
			if ( radioButtonNegativeFreeTarget.Checked )
				at = -at;
			else if ( radioButtonHorizontalTarget.Checked ) {
				at.y = 0;
				at.Normalize();
			}

			float	roll = Mathf.ToRad( floatTrackbarControlLightRoll.Value );
			float3	left, up;
			at.OrthogonalBasis( out left, out up );

			float3	axisX =  Mathf.Cos( roll ) * left + Mathf.Sin( roll ) * up;
			float3	axisY = -Mathf.Sin( roll ) * left + Mathf.Cos( roll ) * up;

			float	radiusX = floatTrackbarControlLightScaleX.Value;
			float	radiusY = floatTrackbarControlLightScaleY.Value;

			m_CB_Light.m._luminance = floatTrackbarControlLuminance.Value;
			m_CB_Light.m._wsLight2World.r0.Set( axisX, radiusX );
			m_CB_Light.m._wsLight2World.r1.Set( axisY, radiusY );
			m_CB_Light.m._wsLight2World.r2.Set( at, Mathf.PI * radiusX * radiusY );	// Disk area in W
			m_CB_Light.m._wsLight2World.r3.Set( wsLightPosition, 1 );
			m_CB_Light.UpdateData();


 			m_device.Clear( m_device.DefaultTarget, float4.Zero );
 			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

			// =========== Render scene ===========
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
			m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

			if ( checkBoxShowReference.Checked ) {
				// Use expensive reference
				if ( m_shader_RenderScene_Reference.Use() ) {
					m_device.RenderFullscreenQuad( m_shader_RenderScene_Reference );
				}
			} else {
				if ( m_shader_RenderScene.Use() ) {
					m_device.RenderFullscreenQuad( m_shader_RenderScene );
				}
			}

			// =========== Render Disk ===========
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

			if ( m_shader_RenderLight.Use() ) {
				m_prim_disk.Render( m_shader_RenderLight );
			}

			// Show!
			m_device.Present( false );
		}

		/// <summary>
		/// Computes the solid angle covered by the light as seen by provided position
		/// </summary>
		/// <param name="_wsPosition"></param>
		void	ComputeResult( float3 _wsPosition ) {
			
			float3	lsLightPos = m_CB_Light.m._wsLight2World[3].xyz - _wsPosition;
			float3	lsLightX = m_CB_Light.m._wsLight2World[0].w * m_CB_Light.m._wsLight2World[0].xyz;
			float3	lsLightY = m_CB_Light.m._wsLight2World[1].w * m_CB_Light.m._wsLight2World[1].xyz;

			float3	n = float3.UnitY;	// Plane normal is straight up

			float	dA = Mathf.TWOPI / 1024.0f;

			float	c0, s0, c1, s1;
			float	FdotN, dot;

			double	dTheta;
			float3	v0, v1, u0, u1, cross, dF;

			double	sum0 = 0;
			double	sum1 = 0;
			float3	sumF0 = float3.Zero;
			float3	sumF1 = float3.Zero;
			for ( int i=0; i < 1024; i++ ) {
				c0 = Mathf.Cos( i * dA );
				s0 = Mathf.Sin( i * dA );
				c1 = Mathf.Cos( i * dA + dA );
				s1 = Mathf.Sin( i * dA + dA );

				v0 = lsLightPos + c0 * lsLightX + s0 * lsLightY;
				v1 = lsLightPos + c1 * lsLightX + s1 * lsLightY;

				u0 = v0.NormalizedSafe;
				u1 = v1.NormalizedSafe;
				dot = u0.Dot( u1 );
				dTheta = Math.Acos( Mathf.Clamp( dot, -1, 1 ) );

				//////////////////////////////////////////////////////////////////////////
				// Regular way
				cross = v0.Cross( v1 );
				cross.NormalizeSafe();

				// Directly accumulate F
				dF = (float) dTheta * cross;
				sumF0 += dF;

				// Accumulate F.n each step of the way
//				FdotN = dF.Dot( n );
				FdotN = dF.y;
				sum0 += FdotN;


				//////////////////////////////////////////////////////////////////////////
				// Bent way
//				FdotN = Mathf.Sqrt( 1 - Mathf.Pow2( u0.Dot( n ) ) );	// sin( alpha )
				FdotN = Mathf.Sqrt( 1 - Mathf.Pow2( u0.y ) );			// sin( alpha )
				dF = (float) dTheta * FdotN * u0;
				sumF1 += dF;
				sum1 += (float) dTheta * FdotN;
			}

			textBoxResults.Text = "Target = " + _wsPosition + "\r\n"
								+ "\r\n"
								+ "Regular F = " + sumF0 + "\r\n"
								+ "Regular sum = " + sum0 + "\r\n"
								+ "Regular F.N = " + sumF0.Dot( n ) + "\r\n"
								+ "\r\n"
								+ "Ortho F = " + sumF1 + "\r\n"
								+ "Ortho sum = " + sum1 + "\r\n"
								+ "Ortho |F| = " + sumF1.Length + "  <== \r\n";
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
