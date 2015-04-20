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

namespace SHZHConvolutionViewer
{
	public partial class Form1 : Form
	{
		Device				m_Device = null;

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Camera {
//			public float4x4		m_World2Camera;
			public float4x4		m_World2Proj;
		};

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Mesh {
			public float4		m_SH0;
			public float4		m_SH1;

			public float		m_SH2;
			public float3		m_ZH;

			public float4		m_resultSH0;
			public float4		m_resultSH1;

			public float		m_resultSH2;
			public uint			m_flags;
		};

		ConstantBuffer< CB_Camera >		m_CB_Camera;
		ConstantBuffer< CB_Mesh >		m_CB_Mesh;
		Shader							m_Shader_RenderMesh;
		Primitive						m_Prim_Sphere;

		Camera							m_Camera = new Camera();
		CameraManipulator				m_CameraManipulator = new CameraManipulator();

		public Form1()
		{
			InitializeComponent();

			m_Device = new Device();
			m_Device.Init( panelOutput1.Handle, false, false );

			m_Shader_RenderMesh = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "./Shaders/RenderMesh.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null );

			{
				List<VertexP3>	Vertices = new List<VertexP3>();
				List<uint>		Indices = new List<uint>();

				const int		SUBDIVS_THETA = 80;
				const int		SUBDIVS_PHI = 160;
				for ( int Y=0; Y <= SUBDIVS_THETA; Y++ ) {
					double	Theta = Y * Math.PI / SUBDIVS_THETA;
					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );
					for ( int X=0; X <= SUBDIVS_PHI; X++ ) {
						double	Phi = X * 2.0 * Math.PI / SUBDIVS_PHI;
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	N = new float3( SinTheta*SinPhi, CosTheta, SinTheta*CosPhi );
						Vertices.Add( new VertexP3() { P=N } );
					}
				}

				for ( int Y=0; Y < SUBDIVS_THETA; Y++ ) {
					int	CurrentLineOffset = Y * (SUBDIVS_PHI+1);
					int	NextLineOffset = (Y+1) * (SUBDIVS_PHI+1);
					for ( int X=0; X <= SUBDIVS_PHI; X++ ) {
						Indices.Add( (uint) (CurrentLineOffset + X) );
						Indices.Add( (uint) (NextLineOffset + X) );
					}
					if ( Y < SUBDIVS_THETA-1 ) {
						Indices.Add( (uint) (NextLineOffset - 1) );	// Degenerate triangle to end the line
						Indices.Add( (uint) NextLineOffset );		// Degenerate triangle to start the next line
					}
				}

				m_Prim_Sphere = new Primitive( m_Device, Vertices.Count, VertexP3.FromArray( Vertices.ToArray() ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
			}

			// Setup camera
			m_CB_Camera = new ConstantBuffer< CB_Camera >( m_Device, 0 );
			m_CB_Mesh = new ConstantBuffer< CB_Mesh >( m_Device, 1 );

			m_Camera.CreatePerspectiveCamera( 60.0f * (float) Math.PI / 180.0f, (float) panelOutput1.Width / panelOutput1.Height, 0.01f, 40.0f );
			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			m_CameraManipulator.Attach( panelOutput1, m_Camera );
			m_CameraManipulator.InitializeCamera( 4.0f * float3.UnitZ, float3.Zero, float3.UnitY );

			// Build SH
			{
				const int		SUBDIVS_THETA = 80;
				const int		SUBDIVS_PHI = 160;

				const double	f0 = 0.28209479177387814347403972578039;		// 0.5 / sqrt(PI);
				const double	f1 = 0.48860251190291992158638462283835;		// 0.5 * sqrt(3/PI);
				const double	f2 = 1.09254843059207907054338570580270;		// 0.5 * sqrt(15/PI);
				const double	f3 = 0.31539156525252000603089369029571;		// 0.25 * sqrt(5.PI);

				double[]	SH = new double[9];
				double[]	DirectionSH = new double[9];
				double[]	PreciseDirectionSH = new double[9];
				double[]	SumDirectionSH = new double[9];
				double[]	SumPreciseDirectionSH = new double[9];
				for ( int Y=0; Y < SUBDIVS_THETA; Y++ ) {
 					for ( int X=0; X < SUBDIVS_PHI; X++ ) {
						double	Theta = 2.0 * Math.Acos( Math.Sqrt( 1.0 - (Y+WMath.SimpleRNG.GetUniform()) / SUBDIVS_THETA ) );
						double	Phi = 2.0 * Math.PI * (X+WMath.SimpleRNG.GetUniform()) / SUBDIVS_PHI;

						float	CosTheta = (float) Math.Cos( Theta );
						float	SinTheta = (float) Math.Sin( Theta );
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	Direction = new float3( SinTheta*CosPhi, SinTheta*SinPhi, CosTheta );	// Z up

 						DirectionSH[0] =  f0;																// l=0 m=0
						DirectionSH[1] =  f1 * Direction.y;													// l=1 m=-1
						DirectionSH[2] =  f1 * Direction.z;													// l=1 m=0
						DirectionSH[3] =  f1 * Direction.x;													// l=1 m=1
						DirectionSH[4] =  f2 * Direction.x * Direction.y;									// l=2 m=-2
						DirectionSH[5] =  f2 * Direction.y * Direction.z;									// l=2 m=-1
						DirectionSH[6] =  f3 * (3.0 * Direction.z*Direction.z - 1.0);						// l=2 m=0
						DirectionSH[7] =  f2 * Direction.x * Direction.z;									// l=2 m=1
						DirectionSH[8] =  f2 * 0.5 * (Direction.x*Direction.x - Direction.y*Direction.y);	// l=2 m=2

						for ( int i=0; i < 9; i++ ) {
							int	l = (int) Math.Floor( Math.Sqrt( i ) );
							int	m = i - l*(l+1);
							PreciseDirectionSH[i] = SphericalHarmonics.SHFunctions.ComputeSH( l, m, Theta, Phi );

							SumDirectionSH[i] += DirectionSH[i];
							SumPreciseDirectionSH[i] += PreciseDirectionSH[i];

// 							bool	rha = false;
// 							if ( Math.Abs( DirectionSH[i] - PreciseDirectionSH[i] ) > 1e-3 ) {
// 								//throw new Exception( "BRA!" );
// 								rha = true;
// 							}
						}


						// Encode function
//						double	Function = 0.1 + 0.45 * (1.0 - Math.Cos( 2.0 * Theta ) * Math.Cos( 1.0 * Phi ));
						double	Function = 0.5 * (1.0 + Direction.x);

						for ( int i=0; i < 9; i++ )
 							SH[i] += Function * DirectionSH[i];
					}
				}

				// Normalize and store
				double	Normalizer = 4.0 * Math.PI / (SUBDIVS_THETA * SUBDIVS_PHI);
				for ( int i=0; i < 9; i++ ) {
					SH[i] *= Normalizer;
					SumDirectionSH[i] *= Normalizer;
					SumPreciseDirectionSH[i] *= Normalizer;
				}
						
// 				SphericalHarmonics.SHFunctions.EncodeIntoSH( SH, SUBDIVS_THETA, SUBDIVS_PHI, 1, 3, ( double _Theta, double _Phi ) => { 
// //					double	Function = 0.1 + 0.45 * (1.0 - Math.Cos( 2.0 * _Theta ) * Math.Cos( 1.0 * _Phi ));
// 					double	Function = Math.Cos( _Phi );
// 					return Function;
// 				} );


				m_CB_Mesh.m.m_SH0.x = (float) SH[0];
				m_CB_Mesh.m.m_SH0.y = (float) SH[1];
				m_CB_Mesh.m.m_SH0.z = (float) SH[2];
				m_CB_Mesh.m.m_SH0.w = (float) SH[3];
				m_CB_Mesh.m.m_SH1.x = (float) SH[4];
				m_CB_Mesh.m.m_SH1.y = (float) SH[5];
				m_CB_Mesh.m.m_SH1.z = (float) SH[6];
				m_CB_Mesh.m.m_SH1.w = (float) SH[7];
				m_CB_Mesh.m.m_SH2   = (float) SH[8];
			}

			Application.Idle += new EventHandler( Application_Idle );
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Camera.m.m_World2Proj = m_Camera.World2Camera * m_Camera.Camera2Proj;
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			m_Shader_RenderMesh.Dispose();
			m_Prim_Sphere.Dispose();
			m_CB_Mesh.Dispose();
			m_CB_Camera.Dispose();
			m_Device.Dispose();

			base.OnFormClosing( e );
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Update camera
			m_CB_Camera.UpdateData();

			// Clear
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );
			m_Device.Clear( float4.Zero );

			// Render
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

			if ( m_Shader_RenderMesh.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

				if ( radioButton1.Checked )
					m_CB_Mesh.m.m_flags = 0;
				else if ( radioButton2.Checked )
					m_CB_Mesh.m.m_flags = 1;
				else if ( radioButton3.Checked )
					m_CB_Mesh.m.m_flags = 2;
				else if ( radioButton4.Checked )
					m_CB_Mesh.m.m_flags = 3;
				m_CB_Mesh.UpdateData();

				m_Prim_Sphere.Render( m_Shader_RenderMesh );
			}

			m_Device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}
	}
}
