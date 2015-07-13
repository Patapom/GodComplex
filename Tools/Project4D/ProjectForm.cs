using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;
using Nuaj.Cirrus.Utility;

namespace Project4D
{
	// 4D Projection is done using information from http://steve.hollasch.net/thesis/chapter4.html
	public partial class ProjectForm : Form
	{
		const int	POINTS_COUNT = 256 * 100;

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
		private struct CB_Camera4D {
			public float4		_CameraPos;
			public float4		_CameraX;
			public float4		_CameraY;
			public float4		_CameraZ;
			public float4		_CameraW;
			public float		_CameraTanHalfFOV;
		}

// 		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
// 		private struct SB_Point {
// 			public float3		__Position;	// Projected position + Z
// 			public float		_Size;		// Point size
// 		}

		private Device				m_Device = new Device();

		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Camera4D>	m_CB_Camera4D = null;

		private StructuredBuffer<float4>	m_SB_Points4D = null;
		private StructuredBuffer<float4>	m_SB_Points2D = null;

		private Primitive			m_PrimQuad = null;

		private ComputeShader		m_CS_Project4D = null;
		private Shader				m_PS_Display = null;

		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		// 4D Camera
		private float4				m_CameraPosition4D = float4.Zero;
		private float4				m_CameraTarget4D = float4.Zero;
		private float4				m_CameraUp4D = float4.UnitY;
		private float4				m_CameraOver4D = float4.UnitZ;

		private DateTime			m_StartTime = DateTime.Now;

		public ProjectForm()
		{
			InitializeComponent();

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_Device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e )
			{
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Project 4D Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CS_Project4D = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "./Shaders/Project4D.hlsl" ) ), "CS", null );
			m_PS_Display = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "./Shaders/Display.hlsl" ) ), VERTEX_FORMAT.T2, "VS", null, "PS", null );

			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Camera4D = new ConstantBuffer<CB_Camera4D>( m_Device, 2 );

			{
				VertexT2[]	Vertices = new VertexT2[4] {
					new VertexT2() { UV = new float2( -1.0f, 1.0f ) },
					new VertexT2() { UV = new float2( -1.0f, -1.0f ) },
					new VertexT2() { UV = new float2(  1.0f, 1.0f ) },
					new VertexT2() { UV = new float2(  1.0f, -1.0f ) },
				};
				m_PrimQuad = new Primitive( m_Device, 4, VertexT2.FromArray( Vertices ), null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.T2 );
			}

			/////////////////////////////////////////////////////////////////////////////////////
			// Create and fill the structured buffers with initial points lying on the surface of a hypersphere
			m_SB_Points4D = new StructuredBuffer<float4>( m_Device, POINTS_COUNT, true );
			m_SB_Points2D = new StructuredBuffer<float4>( m_Device, POINTS_COUNT, false );

			Random	RNG = new Random( 1 );
			float4	temp = new float4();
			for ( int i=0; i < POINTS_COUNT; i++ ) {
				temp.x = (float) (2.0 * RNG.NextDouble() - 1.0);
				temp.y = (float) (2.0 * RNG.NextDouble() - 1.0);
				temp.z = (float) (2.0 * RNG.NextDouble() - 1.0);
				temp.w = (float) (2.0 * RNG.NextDouble() - 1.0);
				temp = temp.Normalized;	// Makes the point lie on a hypersphere of radius 1
				m_SB_Points4D.m[i] = temp;
			}
			m_SB_Points4D.Write();	// Upload

			/////////////////////////////////////////////////////////////////////////////////////
			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 10.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 0, 2 ), new float3( 0, 0, 0 ), float3.UnitY );
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			m_SB_Points2D.Dispose();
			m_SB_Points4D.Dispose();

			m_CB_Camera4D.Dispose();
			m_CB_Camera.Dispose();

			m_PrimQuad.Dispose();

			m_PS_Display.Dispose();
			m_CS_Project4D.Dispose();

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

			m_CB_Camera.UpdateData();
		}

		// Returns a 4D vector orthogonal to the 3 vectors a, b and c (source http://steve.hollasch.net/thesis/chapter2.html)
		//   | i  j  k  l  |   | ay*(bz*cw - cz*bw) - az*(by*cw - cy*bw) + aw*(by*cz - cy*bz) |
		//   | ax ay az aw |   |-ax*(bz*cw - cz*bw) + az*(bx*cw - cx*bw) - aw*(bx*cz - cx*bz) |
		//   | bx by bz bw | = | ax*(by*cw - cy*bw) - ay*(bx*cw - cx*bw) + aw*(bx*cy - cx*by) |
		//   | cx cy cz cw |   |-ax*(by*cz - cy*bz) + ay*(bx*cz - cx*bz) - az*(bx*cy - cx*by) |
		// 
		float4	Cross4D( float4 a, float4 b, float4 c ) {
			// Calculate intermediate values.
			float	A = (b.x * c.y) - (b.y * c.x);
			float	B = (b.x * c.z) - (b.z * c.x);
			float	C = (b.x * c.w) - (b.w * c.x);
			float	D = (b.y * c.z) - (b.z * c.y);
			float	E = (b.y * c.w) - (b.w * c.y);
			float	F = (b.z * c.w) - (b.w * c.z);

			return new float4(
					  (a.y * F) - (a.z * E) + (a.w * D),
					- (a.x * F) + (a.z * C) - (a.w * B),
					  (a.x * E) - (a.y * C) + (a.w * A),
					- (a.x * D) + (a.y * B) - (a.z * A)
				);
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			DateTime	currentTime = DateTime.Now;
			double		time = (float) (currentTime - m_StartTime).TotalSeconds;


			/////////////////////////////////////////////////////////////////////////////////////
			// Build 4D Camera vectors
			const float	CAMERA_FOV_4D = 60.0f;
			const float	CAMERA_FAR_CLIP = 8.0f;

			if ( checkBoxAuto.Checked ) {
				const float	RADIUS = 1.0f;
				double		alpha = 0.5 * time * Math.PI;
				double		beta = 0.3 * time * Math.PI;
				double		gamma = 0.7 * time * Math.PI;

				m_CameraPosition4D = new float4( RADIUS * (float) (Math.Cos( alpha ) * Math.Cos( gamma )), RADIUS * (float) (Math.Cos( beta ) * Math.Sin( alpha )), RADIUS * (float) (Math.Cos( beta ) * Math.Sin( gamma )), RADIUS * (float) Math.Sin( alpha ) );
			} else {
				m_CameraPosition4D = new float4( floatTrackbarControlX.Value, floatTrackbarControlY.Value, floatTrackbarControlZ.Value, floatTrackbarControlW.Value );
			}

			float4	At = (m_CameraTarget4D - m_CameraPosition4D).Normalized;
			m_CB_Camera4D.m._CameraPos = m_CameraPosition4D;
			m_CB_Camera4D.m._CameraTanHalfFOV = (float) Math.Tan( 0.5f * Math.PI / 180.0 * CAMERA_FOV_4D );
			m_CB_Camera4D.m._CameraW = At;
			m_CB_Camera4D.m._CameraX = Cross4D( m_CameraUp4D, m_CameraOver4D, At ).Normalized;
			m_CB_Camera4D.m._CameraY = Cross4D( m_CameraOver4D, At, m_CB_Camera4D.m._CameraX ).Normalized;
			m_CB_Camera4D.m._CameraZ = Cross4D( At, m_CB_Camera4D.m._CameraX, m_CB_Camera4D.m._CameraY );

			m_CB_Camera4D.m._CameraX /= CAMERA_FAR_CLIP;
			m_CB_Camera4D.m._CameraY /= CAMERA_FAR_CLIP;
			m_CB_Camera4D.m._CameraZ /= CAMERA_FAR_CLIP;

			m_CB_Camera4D.UpdateData();

			/////////////////////////////////////////////////////////////////////////////////////
			// Transform points from 4D to 2D using compute shader
			if ( m_CS_Project4D.Use() ) {
				m_SB_Points4D.SetInput( 0 );
				m_SB_Points2D.SetOutput( 0 );

				m_SB_Points2D.RemoveFromLastAssignedSlots();

				int	groupsCount = POINTS_COUNT >> 8;
				m_CS_Project4D.Dispatch( groupsCount, 1, 1 );
			}

			/////////////////////////////////////////////////////////////////////////////////////
			// Render points
			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			if ( m_PS_Display.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

				m_SB_Points2D.SetInput( 0 );

				m_PrimQuad.RenderInstanced( m_PS_Display, POINTS_COUNT );
			}

			// Show!
			m_Device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}
	}
}
