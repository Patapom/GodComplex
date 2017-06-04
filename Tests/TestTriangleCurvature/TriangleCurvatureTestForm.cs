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

namespace TriangleCurvature
{
	public partial class TestTriangleCurvatureForm : Form {

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
 		struct CB_Main {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public float	_time;
			public uint		_flags;
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

		private Shader								m_Shader_renderScene = null;

		private ConstantBuffer< CB_Main >			m_CB_Main;
		private ConstantBuffer< CB_Camera >			m_CB_Camera = null;

		private Primitive							m_Prim_Cube = null;

		private Camera								m_camera = new Camera();
		private CameraManipulator					m_manipulator = new CameraManipulator();

		#endregion

		public TestTriangleCurvatureForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );

//				m_Shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );
				m_Shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene_Subdiv.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );
				
				m_CB_Main = new ConstantBuffer< CB_Main >( m_device, 0 );
				m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );

//				m_Prim_Cube = BuildCube();
				m_Prim_Cube = BuildSubdividedCube();

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Exception: " + _e.Message, "Path Tracing Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			m_startTime = DateTime.Now;

			// Initialize camera
			m_camera.CreatePerspectiveCamera( 60.0f * (float) Math.PI / 180.0f, (float) panelOutput.Width / panelOutput.Height, 0.01f, 10.0f );
			m_manipulator.Attach( panelOutput, m_camera );
			m_manipulator.InitializeCamera( new float3( 0, 0, 4.0f ), float3.Zero, float3.UnitY );
			m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
			m_camera_CameraTransformChanged( null, EventArgs.Empty );

			Application.Idle += Application_Idle;
		}

		protected override void OnClosing( CancelEventArgs e ) {
			base.OnClosing( e );

			Device	D = m_device;
			m_device = null;

			m_Prim_Cube.Dispose();

			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_Shader_renderScene.Dispose();

			D.Dispose();
		}

		#region Primitive Building

		Primitive	BuildCube() {
			// Default example face where B is used to stored the triangle's center and 
			float3	C0 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3( -1.0f, -1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f )) / 3.0f;
			float3	C1 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f ) + new float3(  1.0f,  1.0f, 1.0f )) / 3.0f;

			float	bend = 1.0f;

			VertexP3N3G3B3T2[]	defaultFace = new VertexP3N3G3B3T2[6] {
				// First triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( bend * -1.0f, bend *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3( -1.0f, -1.0f, 1.0f ), N = new float3( bend * -1.0f, bend * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( bend *  1.0f, bend * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 1, 1 ) },

				// 2nd triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( bend * -1.0f, bend *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( bend *  1.0f, bend * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f,  1.0f, 1.0f ), N = new float3( bend *  1.0f, bend *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 0 ) },
			};

			float3[]	faceNormals = new float3[6] {
				-float3.UnitX,
				 float3.UnitX,
				-float3.UnitY,
				 float3.UnitY,
				-float3.UnitZ,
				 float3.UnitZ,
			};
			float3[]	faceTangents = new float3[6] {
				 float3.UnitZ,
				-float3.UnitZ,
				 float3.UnitX,
				 float3.UnitX,
				-float3.UnitX,
				 float3.UnitX,
			};

			Func<float3,float3,float3,float3,float3>	lambdaTransform = ( float3 _P, float3 _T, float3 _B, float3 _N ) => _P.x * _T + _P.y * _B + _P.z * _N;

			VertexP3N3G3B3T2[]	vertices = new VertexP3N3G3B3T2[6*2*3];
			for ( int faceIndex=0; faceIndex < 6; faceIndex++ ) {
				float3	N = faceNormals[faceIndex];
				float3	T = faceTangents[faceIndex];
				float3	B = N.Cross( T );

				for ( int i=0; i < 6; i++ ) {
					VertexP3N3G3B3T2	V = defaultFace[i];
					vertices[6*faceIndex+i].P = lambdaTransform( V.P, T, B, N );
					vertices[6*faceIndex+i].N = lambdaTransform( V.N, T, B, N );
					vertices[6*faceIndex+i].T = lambdaTransform( V.T, T, B, N );
					vertices[6*faceIndex+i].B = lambdaTransform( V.B, T, B, N );
					vertices[6*faceIndex+i].UV = V.UV;
				}
			}
			uint[]				indices = new uint[6 * 2*3] {
// 				0, 1, 2, 0, 2, 3,
// 				4, 5, 6, 4, 6, 7,
// 				8, 9, 10, 8, 10, 11,
// 				12, 13, 14, 12, 14, 15,
// 				16, 17, 18, 16, 18, 19,
// 				20, 21, 22, 20, 22, 23,
				0, 1, 2, 3, 4, 5,
				6, 7, 8, 9, 10, 11,
				12, 13, 14, 15, 16, 17,
				18, 19, 20, 21, 22, 23,
				24, 25, 26, 27, 28, 29,
				30, 31, 32, 33, 34, 35
			};

			return new Primitive( m_device, vertices.Length, VertexP3N3G3B3T2.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
		}

		Primitive	BuildSubdividedCube() {
			// Default example face where B is used to stored the triangle's center and 
			float3	C0 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3( -1.0f, -1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f )) / 3.0f;
			float3	C1 = (new float3( -1.0f,  1.0f, 1.0f ) + new float3(  1.0f, -1.0f, 1.0f ) + new float3(  1.0f,  1.0f, 1.0f )) / 3.0f;

			float	bend = 1.0f;

			VertexP3N3G3B3T2[]	defaultFace = new VertexP3N3G3B3T2[6] {
				// First triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( bend * -1.0f, bend *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3( -1.0f, -1.0f, 1.0f ), N = new float3( bend * -1.0f, bend * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 0, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( bend *  1.0f, bend * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C0, UV = new float2( 1, 1 ) },

				// 2nd triangle
				new VertexP3N3G3B3T2() { P = new float3( -1.0f,  1.0f, 1.0f ), N = new float3( bend * -1.0f, bend *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 0, 0 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f, -1.0f, 1.0f ), N = new float3( bend *  1.0f, bend * -1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 1 ) },
				new VertexP3N3G3B3T2() { P = new float3(  1.0f,  1.0f, 1.0f ), N = new float3( bend *  1.0f, bend *  1.0f, 1.0f ).Normalized, T = new float3( 0, 0, 1 ), B = C1, UV = new float2( 1, 0 ) },
			};

			float3[]	faceNormals = new float3[6] {
				-float3.UnitX,
				 float3.UnitX,
				-float3.UnitY,
				 float3.UnitY,
				-float3.UnitZ,
				 float3.UnitZ,
			};
			float3[]	faceTangents = new float3[6] {
				 float3.UnitZ,
				-float3.UnitZ,
				 float3.UnitX,
				 float3.UnitX,
				-float3.UnitX,
				 float3.UnitX,
			};

			Func<float3,float3,float3,float3,float3>	lambdaTransform = ( float3 _P, float3 _T, float3 _B, float3 _N ) => _P.x * _T + _P.y * _B + _P.z * _N;

			List< VertexP3N3G3B3T2 >	vertices = new List<VertexP3N3G3B3T2>();
			List< uint >				indices = new List<uint>();

			const uint	SUBVIVS_COUNT = 5;

			VertexP3N3G3B3T2[]	temp = new VertexP3N3G3B3T2[3];
//			for ( int faceIndex=3; faceIndex < 4; faceIndex++ ) {
			for ( int faceIndex=0; faceIndex < 6; faceIndex++ ) {
				float3	N = faceNormals[faceIndex];
				float3	T = faceTangents[faceIndex];
				float3	B = N.Cross( T );

				for ( uint triIndex=0; triIndex < 2; triIndex++ ) {
					for ( int i=0; i < 3; i++ ) {
						VertexP3N3G3B3T2	V = defaultFace[3*triIndex+i];
						temp[i].P = lambdaTransform( V.P, T, B, N );
						temp[i].N = lambdaTransform( V.N, T, B, N );
						temp[i].T = lambdaTransform( V.T, T, B, N );
						temp[i].B = lambdaTransform( V.B, T, B, N );
						temp[i].UV = V.UV;
					}

					SubdivTriangle( temp, SUBVIVS_COUNT, vertices, indices );
				}
			}

			return new Primitive( m_device, vertices.Count, VertexP3N3G3B3T2.FromArray( vertices.ToArray() ), indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
		}

		void	SubdivTriangle( VertexP3N3G3B3T2[] _triangle, uint _count, List< VertexP3N3G3B3T2 > _vertices, List< uint > _indices ) {
			if ( _count == 0 ) {
				// Push triangle
// _triangle[0].B.x = (float) _vertices.Count;
// _triangle[1].B.x = (float) _vertices.Count+1;
// _triangle[2].B.x = (float) _vertices.Count+2;
				_indices.Add( (uint) _vertices.Count );
				_vertices.Add( _triangle[0] );
				_indices.Add( (uint) _vertices.Count );
				_vertices.Add( _triangle[1] );
				_indices.Add( (uint) _vertices.Count );
				_vertices.Add( _triangle[2] );
				return;
			}

			VertexP3N3G3B3T2[]	insideTriangle = new VertexP3N3G3B3T2[3] {
				new VertexP3N3G3B3T2() { P = 0.5f * (_triangle[0].P + _triangle[1].P), N = 0.5f * (_triangle[0].N + _triangle[1].N), UV = 0.5f * (_triangle[0].UV + _triangle[1].UV), T = 0.5f * (_triangle[0].T + _triangle[1].T), B = 0.5f * (_triangle[0].B + _triangle[1].B) },
				new VertexP3N3G3B3T2() { P = 0.5f * (_triangle[1].P + _triangle[2].P), N = 0.5f * (_triangle[1].N + _triangle[2].N), UV = 0.5f * (_triangle[1].UV + _triangle[2].UV), T = 0.5f * (_triangle[1].T + _triangle[2].T), B = 0.5f * (_triangle[1].B + _triangle[2].B) },
				new VertexP3N3G3B3T2() { P = 0.5f * (_triangle[2].P + _triangle[0].P), N = 0.5f * (_triangle[2].N + _triangle[0].N), UV = 0.5f * (_triangle[2].UV + _triangle[0].UV), T = 0.5f * (_triangle[2].T + _triangle[0].T), B = 0.5f * (_triangle[2].B + _triangle[0].B) },
			};

			VertexP3N3G3B3T2[]	temp = new VertexP3N3G3B3T2[3];
			temp[0] = _triangle[0];
			temp[1] = insideTriangle[0];
			temp[2] = insideTriangle[2];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
			temp[0] = insideTriangle[0];
			temp[1] = _triangle[1];
			temp[2] = insideTriangle[1];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
			temp[0] = insideTriangle[0];
			temp[1] = insideTriangle[1];
			temp[2] = insideTriangle[2];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
			temp[0] = insideTriangle[2];
			temp[1] = insideTriangle[1];
			temp[2] = _triangle[2];
			SubdivTriangle( temp, _count-1, _vertices, _indices );
		}

		#endregion

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
			m_CB_Main.m._resolutionX = (uint) panelOutput.Width;
			m_CB_Main.m._resolutionY = (uint) panelOutput.Height;
			m_CB_Main.m._time = (float) (currentTime - m_startTime).TotalSeconds;
			m_CB_Main.m._flags = (uint) (checkBox1.Checked ? 1U : 0U);
			m_CB_Main.UpdateData();

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
			m_device.Clear( float4.Zero );
			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, (byte) 0U, true, false );

			if ( m_Shader_renderScene.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

				m_Prim_Cube.Render( m_Shader_renderScene );
			}

			m_device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}
	}
}
