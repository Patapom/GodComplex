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
// TestFloat3x3();
// TestFloat4x4();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			// Initialize the device
			m_device.Init( panelOutput1.Handle, false, true );

			// Build cube primitive
			{
				float3	colorXp = new float3( 1, 0, 0 );
				float3	colorXn = new float3( 1, 1, 0 );
				float3	colorYp = new float3( 0, 1, 0 );
				float3	colorYn = new float3( 0, 1, 1 );
				float3	colorZp = new float3( 0, 0, 1 );
				float3	colorZn = new float3( 1, 0, 1 );

				VertexP3N3G3T2[]	vertices = new VertexP3N3G3T2[6*4] {
					// +X
					new VertexP3N3G3T2() { P = new float3(  1,  1,  1 ), N = new float3(  1, 0, 0 ), T = colorXp, UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1,  1 ), N = new float3(  1, 0, 0 ), T = colorXp, UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1, -1 ), N = new float3(  1, 0, 0 ), T = colorXp, UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1, -1 ), N = new float3(  1, 0, 0 ), T = colorXp, UV = new float2( 1, 0 ) },
					// -X
					new VertexP3N3G3T2() { P = new float3( -1,  1, -1 ), N = new float3( -1, 0, 0 ), T = colorXn, UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1, -1 ), N = new float3( -1, 0, 0 ), T = colorXn, UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1,  1 ), N = new float3( -1, 0, 0 ), T = colorXn, UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1,  1,  1 ), N = new float3( -1, 0, 0 ), T = colorXn, UV = new float2( 1, 0 ) },
					// +Y
					new VertexP3N3G3T2() { P = new float3( -1,  1, -1 ), N = new float3( 0,  1, 0 ), T = colorYp, UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1,  1,  1 ), N = new float3( 0,  1, 0 ), T = colorYp, UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1,  1 ), N = new float3( 0,  1, 0 ), T = colorYp, UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1, -1 ), N = new float3( 0,  1, 0 ), T = colorYp, UV = new float2( 1, 0 ) },
					// -Y
					new VertexP3N3G3T2() { P = new float3( -1, -1,  1 ), N = new float3( 0, -1, 0 ), T = colorYn, UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1, -1 ), N = new float3( 0, -1, 0 ), T = colorYn, UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1, -1 ), N = new float3( 0, -1, 0 ), T = colorYn, UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1,  1 ), N = new float3( 0, -1, 0 ), T = colorYn, UV = new float2( 1, 0 ) },
					// +Z
					new VertexP3N3G3T2() { P = new float3( -1,  1,  1 ), N = new float3( 0, 0,  1 ), T = colorZp, UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1,  1 ), N = new float3( 0, 0,  1 ), T = colorZp, UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1,  1 ), N = new float3( 0, 0,  1 ), T = colorZp, UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3(  1,  1,  1 ), N = new float3( 0, 0,  1 ), T = colorZp, UV = new float2( 1, 0 ) },
					// -Z
					new VertexP3N3G3T2() { P = new float3(  1,  1, -1 ), N = new float3( 0, 0, -1 ), T = colorZn, UV = new float2( 0, 0 ) },
					new VertexP3N3G3T2() { P = new float3(  1, -1, -1 ), N = new float3( 0, 0, -1 ), T = colorZn, UV = new float2( 0, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1, -1, -1 ), N = new float3( 0, 0, -1 ), T = colorZn, UV = new float2( 1, 1 ) },
					new VertexP3N3G3T2() { P = new float3( -1,  1, -1 ), N = new float3( 0, 0, -1 ), T = colorZn, UV = new float2( 1, 0 ) },
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
			m_shader_renderCube = new Shader( m_device, new System.IO.FileInfo( @".\Shaders\RenderCube.hlsl" ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS" );

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
			camera2World.BuildRotLeftHanded( cameraPosition, cameraTarget, float3.UnitY );

			// Build the perspective projection matrix
			float4x4	camera2Proj = new float4x4();
			camera2Proj.BuildProjectionPerspective( 80.0f * (float) Math.PI / 180.0f, (float) Width / Height, 0.01f, 100.0f );

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


#if DEBUG

//////////////////////////////////////////////////////////////////////////
// Test methods compile
static void	TestFloat3x3() {
	if ( System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3x3)) != 36 )
		throw new Exception( "Not the appropriate size!" );

	float3x3	test1 = new float3x3( new float[9] { 9, 8, 7, 6, 5, 4, 3, 2, 1 } );
	float3x3	test2 = new float3x3( new float3( 1, 0, 0 ), new float3( 0, 1, 0 ), new float3( 0, 0, 1 ) );
	float3x3	test3 = new float3x3( 0, 1, 2, 3, 4, 5, 6, 7, 8 );
	float3x3	test0;
				test0 = test3;
	test2.Scale( new float3( 2, 2, 2 ) );
	float3x3	mul0 = test1 * test2;
	float3x3	mul1 = 3.0f * test2;
	float3		mul2 = new float3( 1, 1, 1 ) * test3;
	float3		access0 = test3[2];
	test3[1] = new float3( 12, 13, 14 );
	float		access1 = test3[1,2];
	test3[1,2] = 18;
//	float		coFactor = test3.CoFactor( 0, 2 );
	float		det = test3.Determinant;
	float3x3	inv = test2.Inverse;
	float3x3	id = float3x3.Identity;

	test3.BuildRotationX( 0.5f );
	test3.BuildRotationY( 0.5f );
	test3.BuildRotationZ( 0.5f );
	test3.BuildFromAngleAxis( 0.5f, float3.UnitY );
}

static void	TestFloat4x4() {
	if ( System.Runtime.InteropServices.Marshal.SizeOf(typeof(float4x4)) != 64 )
		throw new Exception( "Not the appropriate size!" );

	float4x4	test1 = new float4x4( new float[16] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 } );
	float4x4	test2 = new float4x4( new float4( 1, 0, 0, 0 ), new float4( 0, 1, 0, 0 ), new float4( 0, 0, 1, 0 ), new float4( 0, 0, 0, 1 ) );
	float4x4	test3 = new float4x4( 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 );
	float4x4	test0;
				test0 = test3;
	test2.Scale( new float3( 2, 2, 2 ) );
	float3x3	cast = (float3x3) test3;
	float4x4	mul0 = test1 * test2;
	float4x4	mul1 = 3.0f * test2;
	float4		mul2 = new float4( 1, 1, 1, 1 ) * test3;
	float4		access0 = test3[2];
	test3[1] = new float4( 12, 13, 14, 15 );
	float		access1 = test3[1,2];
	test3[1,2] = 18;
	float		coFactor = test3.CoFactor( 0, 2 );
	float		det = test3.Determinant;
	float4x4	inv = test2.Inverse;
	float4x4	id = float4x4.Identity;

	test3.BuildRotLeftHanded( float3.UnitZ, float3.Zero, float3.UnitY );
	test3.BuildRotRightHanded( float3.UnitZ, float3.Zero, float3.UnitY );
	test3.BuildProjectionPerspective( 1.2f, 2.0f, 0.01f, 10.0f );
	test3.BuildRotationX( 0.5f );
	test3.BuildRotationY( 0.5f );
	test3.BuildRotationZ( 0.5f );
	test3.BuildFromAngleAxis( 0.5f, float3.UnitY );
}

#endif


	}
}
