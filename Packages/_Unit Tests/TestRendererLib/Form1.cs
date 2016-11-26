using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Renderer;
using SharpMath;

namespace Renderer.UnitTests
{
	public partial class Form1 : Form {

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CBCamera {
			public float4x4	_world2Proj;
		}

		Device						m_device = new Device( );
		Primitive					m_prim_cube;
		Shader						m_shader_renderCube;
		ConstantBuffer< CBCamera >	m_CB_Camera;

		public Form1() {
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			// Initialize the device
			m_device.Init( panelOutput1.Handle, false, true );

			// Build cube primitive
			{
				VertexP3N3G3T2[]	vertices = new VertexP3N3G3T2[6*4] {
					// +X
					new VertexP3N3G3T2() { P = new float3(  1,  1,  1 ), N = new float3(  1, 0, 0 ), UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1,  1 ), N = new float3(  1, 0, 0 ), UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1, -1 ), N = new float3(  1, 0, 0 ), UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1, -1 ), N = new float3(  1, 0, 0 ), UV = new float2( 1, 0 ) },
					// -X
					new VertexP3N3G3T2() { P = new float3( -1,  1, -1 ), N = new float3( -1, 0, 0 ), UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1, -1 ), N = new float3( -1, 0, 0 ), UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1,  1 ), N = new float3( -1, 0, 0 ), UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1,  1,  1 ), N = new float3( -1, 0, 0 ), UV = new float2( 1, 0 ) },
					// +Y
					new VertexP3N3G3T2() { P = new float3( -1,  1, -1 ), N = new float3( 0,  1, 0 ), UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1,  1,  1 ), N = new float3( 0,  1, 0 ), UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1,  1 ), N = new float3( 0,  1, 0 ), UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1, -1 ), N = new float3( 0,  1, 0 ), UV = new float2( 1, 0 ) },
					// -Y
					new VertexP3N3G3T2() { P = new float3( -1, -1,  1 ), N = new float3( 0, -1, 0 ), UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1, -1 ), N = new float3( 0, -1, 0 ), UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1, -1 ), N = new float3( 0, -1, 0 ), UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1,  1 ), N = new float3( 0, -1, 0 ), UV = new float2( 1, 0 ) },
					// +Z
					new VertexP3N3G3T2() { P = new float3( -1,  1,  1 ), N = new float3( 0, 0,  1 ), UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1,  1 ), N = new float3( 0, 0,  1 ), UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1,  1 ), N = new float3( 0, 0,  1 ), UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1,  1 ), N = new float3( 0, 0,  1 ), UV = new float2( 1, 0 ) },
					// -Z
					new VertexP3N3G3T2() { P = new float3(  1,  1, -1 ), N = new float3( 0, 0, -1 ), UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1, -1 ), N = new float3( 0, 0, -1 ), UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1, -1 ), N = new float3( 0, 0, -1 ), UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1,  1, -1 ), N = new float3( 0, 0, -1 ), UV = new float2( 1, 0 ) },
				};
				uint[]	indices = new uint[3*2*6] {
					4*0+0, 4*0+1, 4*0+2, 4*0+0, 4*0+2, 4*0+3,
					4*1+0, 4*1+1, 4*1+2, 4*1+0, 4*1+2, 4*1+3,
					4*2+0, 4*2+1, 4*2+2, 4*2+0, 4*2+2, 4*2+3,
					4*3+0, 4*3+1, 4*3+2, 4*3+0, 4*3+2, 4*3+3,
					4*4+0, 4*4+1, 4*4+2, 4*4+0, 4*4+2, 4*4+3,
					4*5+0, 4*5+1, 4*5+2, 4*5+0, 4*5+2, 4*5+3,
				};

				m_prim_cube = new Primitive( m_device, 6*4, VertexP3N3G3T2.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3T2 );
			}

			// Build the shader to render the cube
			m_shader_renderCube = new Shader( m_device, new ShaderFile( new System.IO.FileInfo( @".\Shaders\RenderCube.hlsl" ) ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS", null );

			// Build constant buffer to provide camera transform
			m_CB_Camera = new ConstantBuffer<CBCamera>( m_device, 0 );

			Application.Idle += Application_Idle;
		}

		protected override void OnFormClosing(FormClosingEventArgs e) {
			e.Cancel = false;
			base.OnFormClosing(e);

			m_CB_Camera.Dispose();
			m_shader_renderCube.Dispose();
			m_prim_cube.Dispose();

			Device	temp = m_device;
			m_device = null;
			temp.Dispose();
		}

		DateTime	m_startTime = DateTime.Now;
		float4x4	ComputeCameraProjection() {
			float	t = (float) (DateTime.Now - m_startTime).TotalSeconds;

			// Make a camera orbiting around the cube
			float	T = t;
			float3	cameraPosition = 3.0f * new float3( (float) Math.Cos( T ), (float) Math.Cos( 3 * T ), (float) Math.Sin( T ) );
			float3	cameraTarget = new float3( 0, 0, 0 );

			float4x4	camera2World = new float4x4();
			camera2World.MakeLookAt( cameraPosition, cameraTarget, float3.UnitY );

			// Build the perspective projection matrix
			float4x4	camera2Proj = new float4x4();
			camera2Proj.MakeProjectionPerspective( 80.0f * (float) Math.PI / 180.0f, (float) Width / Height, 0.01f, 100.0f );

			// Compose the 2 matrices together to obtain the final matrix that transforms world coordinates into projected 2D coordinates
			return camera2World.Inverse * camera2Proj;
		}
		void Application_Idle(object sender, EventArgs e) {
			if ( m_device == null )
				return;

			// Setup default target and depth stencil
			m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

			// Setup default render states
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

			// Clear target & depth
			m_device.Clear( new float4( 0.2f, 0.2f, 0.2f, 1.0f ) );
			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

			// Render the cube
			if ( m_shader_renderCube.Use() ) {
				m_CB_Camera.m._world2Proj = ComputeCameraProjection();
				m_CB_Camera.UpdateData();

				m_prim_cube.Render( m_shader_renderCube );
			}

			// Present
			m_device.Present( false );
		}

// 		void timer1_Tick(object sender, System.EventArgs e) {
// 			throw new System.NotImplementedException();
// 		}
	}
}
