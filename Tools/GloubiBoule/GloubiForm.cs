//#define PRECOMPUTE_BRDF	// Define this to precompute the BRDF, comment to only load it

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
using System.IO;
using UIUtility;

namespace GloubiBoule
{
	public partial class GloubiForm : Form
	{
		#region CONSTANTS

		const int	VOLUME_SIZE = 128;
		const int	HEIGHTMAP_SIZE = 128;
		const int	NOISE_SIZE = 64;
		const int	PHOTONS_COUNT = 128 * 1024;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Global {
			public float4		_ScreenSize;
			public float		_Time;
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

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_GenerateDensity {
			public float3		_wsOffset;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_TracePhotons {
			public float		_Sigma_t;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderRoom {
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderSphere {
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RayMarch {
			public float		_Sigma_t;
			public float		_Sigma_s;
			public float		_Phase_g;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_PostProcess {
		}


		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct SB_PhotonInfo_t {
			public float3	wsStartPosition;
			public float3	wsDirection;
			public float	RadiusDivergence;
		};

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct SB_Photon_t {
			public float3	wsPosition;
			public float	Radius;
		};


		#endregion

		#region FIELDS

		private Device						m_Device = new Device();

		Primitive							m_Prim_Sphere;
		Primitive							m_Prim_Cube;
		Primitive							m_Prim_Quad;

		Texture2D							m_Tex_TempBackBuffer;
		Texture2D							m_Tex_HeightMap;
		Texture2D							m_Tex_Scattering;
		Texture2D							m_Tex_AccumPhotonCube;
		Texture3D							m_Tex_VolumeDensity;
		Texture3D							m_Tex_AccumPhoton3D;
		Texture3D							m_Tex_Noise;
		Texture3D							m_Tex_Noise4D;

		Texture2D							m_tex_IrradianceComplement;
		Texture2D							m_tex_IrradianceAverage;

		ComputeShader						m_Shader_UpdateHeightMap;
		ComputeShader						m_shader_GenerateDensity;
		ComputeShader						m_Shader_ClearAccumulator;
		ComputeShader						m_Shader_InitPhotons;
		ComputeShader						m_Shader_TracePhotons;
		Shader								m_Shader_RenderRoom;
		Shader								m_Shader_RenderSphere;
		Shader								m_Shader_RayMarcher;
		Shader								m_Shader_PostProcess;

		Camera								m_Camera = new Camera();
		CameraManipulator					m_Manipulator = new CameraManipulator();

		ConstantBuffer<CB_Global>			m_CB_Global;
		ConstantBuffer<CB_Camera>			m_CB_Camera;
		ConstantBuffer<CB_GenerateDensity>	m_CB_GenerateDensity;
		ConstantBuffer<CB_TracePhotons>		m_CB_TracePhotons;
		ConstantBuffer<CB_RenderRoom>		m_CB_RenderRoom;
		ConstantBuffer<CB_RenderSphere>		m_CB_RenderSphere;
		ConstantBuffer<CB_RayMarch>			m_CB_RayMarch;
		ConstantBuffer<CB_PostProcess>		m_CB_PostProcess;

		StructuredBuffer< SB_PhotonInfo_t >	m_SB_PhotonInfos;
		StructuredBuffer< SB_Photon_t >[]	m_SB_Photons = new StructuredBuffer< SB_Photon_t >[2];


		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartGameTime = 0;
		public float						m_CurrentGameTime = 0;
		public float						m_StartFPSTime = 0;
		public int							m_SumFrames = 0;
		public float						m_AverageFrameTime = 0.0f;

		#endregion

		public GloubiForm()
		{
			InitializeComponent();

// 			// Build 8 random rotation matrices
// 			string[]	randomRotations = new string[8];
// 			Random	RNG = new Random( 1 );
// 			for ( int i=0; i < 8; i++ ) {
// 				WMath.Matrix3x3	rot = new WMath.Matrix3x3();
// 				rot.FromEuler( new WMath.Vector( (float) RNG.NextDouble(), (float) RNG.NextDouble(), (float) RNG.NextDouble() ) );
// 				randomRotations[i] = rot.ToString();
// 			}

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_Device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			try {
				m_Shader_UpdateHeightMap = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/UpdateHeightMap.hlsl" ), "CS" );
				m_shader_GenerateDensity = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/GenerateDensity.hlsl" ), "CS" );
				m_Shader_ClearAccumulator = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/TracePhotons.hlsl" ), "CS_ClearAccumulator" );
				m_Shader_InitPhotons = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/TracePhotons.hlsl" ), "CS_InitPhotons" );
				m_Shader_TracePhotons = new ComputeShader( m_Device, new System.IO.FileInfo( "Shaders/TracePhotons.hlsl" ), "CS_TracePhotons" );

				m_Shader_RenderRoom = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderRoom.hlsl" ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS" );
				m_Shader_RenderSphere = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderSphere.hlsl" ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS" );
				m_Shader_RayMarcher = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RayMarch.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
				m_Shader_PostProcess = new Shader( m_Device, new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );

//				m_ShaderDownsample = new Shader( m_Device, new System.IO.FileInfo( "Shaders/Downsample.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
			}
			catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			uint	W = (uint) panelOutput.Width;
			uint	H = (uint) panelOutput.Height;

			m_CB_Global = new ConstantBuffer<CB_Global>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_GenerateDensity = new ConstantBuffer<CB_GenerateDensity>( m_Device, 2 );
			m_CB_TracePhotons = new ConstantBuffer<CB_TracePhotons>( m_Device, 2 );
			m_CB_RenderRoom = new ConstantBuffer<CB_RenderRoom>( m_Device, 2 );
			m_CB_RenderSphere = new ConstantBuffer<CB_RenderSphere>( m_Device, 2 );
			m_CB_PostProcess = new ConstantBuffer<CB_PostProcess>( m_Device, 2 );
			m_CB_RayMarch = new ConstantBuffer<CB_RayMarch>( m_Device, 3 );

			m_Tex_TempBackBuffer = new Texture2D( m_Device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA16F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_Tex_HeightMap = new Texture2D( m_Device, HEIGHTMAP_SIZE, HEIGHTMAP_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RG16F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
			m_Tex_Scattering = new Texture2D( m_Device, (uint) panelOutput.Width, (uint) panelOutput.Height, 2, 1, ImageUtility.PIXEL_FORMAT.RGBA16F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_Tex_VolumeDensity = new Texture3D( m_Device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 1, ImageUtility.PIXEL_FORMAT.R8, ImageUtility.COMPONENT_FORMAT.UNORM, false, true, null );
			m_Tex_AccumPhotonCube = new Texture2D( m_Device, 256, 256, -6, 1, ImageUtility.PIXEL_FORMAT.RGBA16F, ImageUtility.COMPONENT_FORMAT.AUTO, false, true, null );
			m_Tex_AccumPhoton3D = new Texture3D( m_Device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 1, ImageUtility.PIXEL_FORMAT.R32, ImageUtility.COMPONENT_FORMAT.UINT, false, true, null );
			BuildNoiseTextures();

//BuildMSBRDF( new DirectoryInfo( @"D:\Workspaces\Patapom.com\Web\blog\patapom\docs\BRDF" ) );
BuildMSBRDF( new DirectoryInfo( @"D:\Workspaces\www.patapom.com\Web\blog\patapom\docs\BRDF" ) );

			// Structured buffer
			m_SB_PhotonInfos = new StructuredBuffer< SB_PhotonInfo_t >( m_Device, PHOTONS_COUNT, false );
			m_SB_Photons[0] = new StructuredBuffer<SB_Photon_t>( m_Device, PHOTONS_COUNT, false );
			m_SB_Photons[1] = new StructuredBuffer<SB_Photon_t>( m_Device, PHOTONS_COUNT, false );

			BuildPrimitives();

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, -2.5f ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartGameTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			m_Prim_Cube.Dispose();
			m_Prim_Sphere.Dispose();
			m_Prim_Quad.Dispose();

			m_SB_Photons[1].Dispose();
			m_SB_Photons[0].Dispose();
			m_SB_PhotonInfos.Dispose();

			m_Tex_Noise4D.Dispose();
			m_Tex_Noise.Dispose();
			m_Tex_AccumPhoton3D.Dispose();
			m_Tex_AccumPhotonCube.Dispose();
			m_Tex_VolumeDensity.Dispose();
			m_Tex_Scattering.Dispose();
			m_Tex_HeightMap.Dispose();
			m_Tex_TempBackBuffer.Dispose();

			m_CB_PostProcess.Dispose();
			m_CB_RayMarch.Dispose();
			m_CB_RenderSphere.Dispose();
			m_CB_RenderRoom.Dispose();
			m_CB_TracePhotons.Dispose();
			m_CB_GenerateDensity.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Global.Dispose();

			m_Shader_PostProcess.Dispose();
			m_Shader_RenderSphere.Dispose();
			m_Shader_RenderRoom.Dispose();
			m_Shader_RayMarcher.Dispose();
			m_Shader_TracePhotons.Dispose();
			m_Shader_InitPhotons.Dispose();
			m_Shader_ClearAccumulator.Dispose();
			m_shader_GenerateDensity.Dispose();
			m_Shader_UpdateHeightMap.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		#region Primitives

		void	BuildPrimitives() {

			{	// Post-process quad
				List<VertexPt4>	Vertices = new List<VertexPt4>();
				Vertices.Add( new VertexPt4() { Pt=new float4( -1, +1, 0, 1 ) } );
				Vertices.Add( new VertexPt4() { Pt=new float4( -1, -1, 0, 1 ) } );
				Vertices.Add( new VertexPt4() { Pt=new float4( +1, +1, 0, 1 ) } );
				Vertices.Add( new VertexPt4() { Pt=new float4( +1, -1, 0, 1 ) } );
				m_Prim_Quad = new Primitive( m_Device, 4, VertexPt4.FromArray( Vertices.ToArray() ), null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
			}

			{	// Sphere Primitive
				List<VertexP3N3G3T2>	Vertices = new List<VertexP3N3G3T2>();
				List<uint>				Indices = new List<uint>();

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
						float3	T = new float3( CosPhi, 0.0f, -SinPhi );
						float2	UV = new float2( (float) X / SUBDIVS_PHI, (float) Y / SUBDIVS_THETA );
						Vertices.Add( new VertexP3N3G3T2() { P=N, N=N, T=T, UV=UV } );
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

				m_Prim_Sphere = new Primitive( m_Device, (uint) Vertices.Count, VertexP3N3G3T2.FromArray( Vertices.ToArray() ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3T2 );
			}

			{	// Build the cube
				float3[]	Normals = new float3[6] {
					-float3.UnitX,
					float3.UnitX,
					-float3.UnitY,
					float3.UnitY,
					-float3.UnitZ,
					float3.UnitZ,
				};

				float3[]	Tangents = new float3[6] {
					float3.UnitZ,
					-float3.UnitZ,
					float3.UnitX,
					-float3.UnitX,
					-float3.UnitX,
					float3.UnitX,
				};

				VertexP3N3G3T2[]	Vertices = new VertexP3N3G3T2[6*4];
				uint[]		Indices = new uint[2*6*3];

				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ ) {
					float3	N = Normals[FaceIndex];
					float3	T = Tangents[FaceIndex];
					float3	B = N.Cross( T );

					Vertices[4*FaceIndex+0] = new VertexP3N3G3T2() {
						P = N - T + B,
						N = N,
						T = T,
//						B = B,
						UV = new float2( 0, 0 )
					};
					Vertices[4*FaceIndex+1] = new VertexP3N3G3T2() {
						P = N - T - B,
						N = N,
						T = T,
//						B = B,
						UV = new float2( 0, 1 )
					};
					Vertices[4*FaceIndex+2] = new VertexP3N3G3T2() {
						P = N + T - B,
						N = N,
						T = T,
//						B = B,
						UV = new float2( 1, 1 )
					};
					Vertices[4*FaceIndex+3] = new VertexP3N3G3T2() {
						P = N + T + B,
						N = N,
						T = T,
//						B = B,
						UV = new float2( 1, 0 )
					};

					Indices[2*3*FaceIndex+0] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+1] = (uint) (4*FaceIndex+1);
					Indices[2*3*FaceIndex+2] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+3] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+4] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+5] = (uint) (4*FaceIndex+3);
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3T2.FromArray( Vertices );

				m_Prim_Cube = new Primitive( m_Device, (uint) Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3T2 );
			}
		}

		#endregion

		#region  Noise Generation

		void	BuildNoiseTextures() {

			PixelsBuffer	Content = new PixelsBuffer( NOISE_SIZE*NOISE_SIZE*NOISE_SIZE*4 );
			PixelsBuffer	Content4D = new PixelsBuffer( NOISE_SIZE*NOISE_SIZE*NOISE_SIZE*16 );

			SimpleRNG.SetSeed( 521288629, 362436069 );

			float4	V = float4.Zero;
			using ( BinaryWriter W = Content.OpenStreamWrite() ) {
				using ( BinaryWriter W2 = Content4D.OpenStreamWrite() ) {
					for ( int Z=0; Z < NOISE_SIZE; Z++ )
						for ( int Y=0; Y < NOISE_SIZE; Y++ )
							for ( int X=0; X < NOISE_SIZE; X++ ) {
								V.Set( (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform() );
								W.Write( V.x );
								W2.Write( V.x );
								W2.Write( V.y );
								W2.Write( V.z );
								W2.Write( V.w );
							}
				}
			}

			m_Tex_Noise = new Texture3D( m_Device, NOISE_SIZE, NOISE_SIZE, NOISE_SIZE, 1, ImageUtility.PIXEL_FORMAT.R8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { Content } );
			m_Tex_Noise4D = new Texture3D( m_Device, NOISE_SIZE, NOISE_SIZE, NOISE_SIZE, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { Content4D } );
		}

		#endregion

		#region Multiple-Scattering BRDF
		
		void	BuildMSBRDF( DirectoryInfo _targetDirectory ) {

			const uint		COS_THETA_SUBDIVS_COUNT = 32;
			const uint		ROUGHNESS_SUBDIVS_COUNT = 32;

			FileInfo	MSBRDFFileName = new FileInfo( Path.Combine( _targetDirectory.FullName, "MSBRDF_E" + COS_THETA_SUBDIVS_COUNT + "x" + ROUGHNESS_SUBDIVS_COUNT + ".float" ) );
			FileInfo	MSBRDFFileName2 = new FileInfo( Path.Combine( _targetDirectory.FullName, "MSBRDF_Eavg" + ROUGHNESS_SUBDIVS_COUNT + ".float" ) );

			#if PRECOMPUTE_BRDF

			const uint		PHI_SUBDIVS_COUNT = 2*512;
			const uint		THETA_SUBDIVS_COUNT = 64;

			const float		dPhi = Mathf.TWOPI / PHI_SUBDIVS_COUNT;
			const float		dTheta = Mathf.HALFPI / THETA_SUBDIVS_COUNT;
			const float		dMu = 1.0f / THETA_SUBDIVS_COUNT;

			float[,]		result = new float[COS_THETA_SUBDIVS_COUNT,ROUGHNESS_SUBDIVS_COUNT];

			string	dumpMathematica = "{";
			for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ ) {
				float	m = (float) Y / (ROUGHNESS_SUBDIVS_COUNT-1);
				float	m2 = Math.Max( 0.01f, m*m );
// 				float	m2 = Math.Max( 0.01f, (float) Y / (ROUGHNESS_SUBDIVS_COUNT-1) );
// 				float	m = Mathf.Sqrt( m2 );

//				dumpMathematica += "{ ";	// Start a new roughness line
				for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ ) {
					float	cosThetaO = (float) X / (COS_THETA_SUBDIVS_COUNT-1);
					float	sinThetaO = Mathf.Sqrt( 1 - cosThetaO*cosThetaO );

					float	NdotV = cosThetaO;

					float	integral = 0.0f;
//					float	integralNDF = 0.0f;
					for ( uint THETA=0; THETA < THETA_SUBDIVS_COUNT; THETA++ ) {
// 						float	thetaI = Mathf.HALFPI * (0.5f+THETA) / THETA_SUBDIVS_COUNT;
// 						float	cosThetaI = Mathf.Cos( thetaI );
// 						float	sinThetaI = Mathf.Sin( thetaI );

						// Use cosine-weighted sampling
						float	sqCosThetaI = (0.5f+THETA) / THETA_SUBDIVS_COUNT;
						float	cosThetaI = Mathf.Sqrt( sqCosThetaI );
						float	sinThetaI = Mathf.Sqrt( 1 - sqCosThetaI );

						float	NdotL = cosThetaI;

						for ( uint PHI=0; PHI < PHI_SUBDIVS_COUNT; PHI++ ) {
							float	phi = Mathf.TWOPI * PHI / PHI_SUBDIVS_COUNT;

							// Compute cos(theta_h) = Omega_h.N where Omega_h = (Omega_i + Omega_o) / ||Omega_i + Omega_o|| is the half vector and N the surface normal
							float	cosThetaH = (cosThetaI + cosThetaO) / Mathf.Sqrt( 2 * (1 + cosThetaO * cosThetaI + sinThetaO * sinThetaI * Mathf.Sin( phi )) );
// 							float3	omega_i = new float3( sinThetaI * Mathf.Cos( phi ), sinThetaI * Mathf.Sin( phi ), cosThetaI );
// 							float3	omega_o = new float3( sinThetaO, 0, cosThetaO );
// 							float3	omega_h = (omega_i + omega_o).Normalized;
// 							float	cosThetaH = omega_h.z;

							// Compute GGX NDF
							float	den = 1 - cosThetaH*cosThetaH * (1 - m2);
							float	NDF = m2 / (Mathf.PI * den*den);

							// Compute Smith shadowing/masking
							float	Smith_i_den = NdotL + Mathf.Sqrt( m2 + (1-m2) * NdotL*NdotL );
							float	Smith_o_den = NdotV + Mathf.Sqrt( m2 + (1-m2) * NdotV*NdotV );

							// Full BRDF is thus...
							float	GGX = NDF / (Smith_i_den * Smith_o_den);

//							integral += GGX * cosThetaI * sinThetaI;
							integral += GGX;
						}

//						integralNDF += Mathf.TWOPI * m2 * cosThetaI * sinThetaI / (Mathf.PI * Mathf.Pow( cosThetaI*cosThetaI * (m2 - 1) + 1, 2.0f ));
					}

					// Finalize
//					integral *= dTheta * dPhi;
					integral *= 0.5f * dMu * dPhi;	// Cosine-weighted sampling has a 0.5 factor!
//					integralNDF *= dTheta;

					result[X,Y] = integral;
					dumpMathematica += "{ " + cosThetaO + ", " + m + ", "  + integral + "}, ";
				}
			}

			dumpMathematica = dumpMathematica.Remove( dumpMathematica.Length-2 );	// Remove last comma
			dumpMathematica += " };";

			// Dump as binary
			using ( FileStream S = MSBRDFFileName.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ )
						for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ )
							W.Write( result[X,Y] );
				}

			//////////////////////////////////////////////////////////////////////////
			// Compute average irradiance based on roughness, re-using the computed results
			const uint		THETA_SUBDIVS_COUNT2 = 512;

			float	dTheta2 = Mathf.HALFPI / THETA_SUBDIVS_COUNT2;

			float[]	resultAvg = new float[ROUGHNESS_SUBDIVS_COUNT];
			for ( uint X=0; X < ROUGHNESS_SUBDIVS_COUNT; X++ ) {

				float	integral = 0.0f;
				for ( uint THETA=0; THETA < THETA_SUBDIVS_COUNT2; THETA++ ) {
					float	thetaO = Mathf.HALFPI * (0.5f+THETA) / THETA_SUBDIVS_COUNT2;
					float	cosThetaO = Mathf.Cos( thetaO );
					float	sinThetaO = Mathf.Sin( thetaO );

					// Sample previously computed table
					float	i = cosThetaO * COS_THETA_SUBDIVS_COUNT;
					uint	i0 = Math.Min( COS_THETA_SUBDIVS_COUNT-1, (uint) Mathf.Floor( i ) );
					uint	i1 = Math.Min( COS_THETA_SUBDIVS_COUNT-1, i0 + 1 );
					float	E = (1-i) * result[i0,X] + i * result[i1,X];

					integral += E * cosThetaO * sinThetaO;
				}

				// Finalize
				integral *= Mathf.TWOPI * dTheta2;

				resultAvg[X] = integral;
			}

			// Dump as binary
			using ( FileStream S = MSBRDFFileName2.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( uint X=0; X < ROUGHNESS_SUBDIVS_COUNT; X++ )
						W.Write( resultAvg[X] );
				}

			#endif

			// Build irradiance complement texture
			using ( PixelsBuffer content = new PixelsBuffer( COS_THETA_SUBDIVS_COUNT * ROUGHNESS_SUBDIVS_COUNT * 4 ) ) {
				using ( FileStream S = MSBRDFFileName.OpenRead() )
					using ( BinaryReader R = new BinaryReader( S ) )
						using ( BinaryWriter W = content.OpenStreamWrite() ) {
							for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ )
								for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ )
									W.Write( R.ReadSingle() );
						}

				m_tex_IrradianceComplement = new Texture2D( m_Device, COS_THETA_SUBDIVS_COUNT, ROUGHNESS_SUBDIVS_COUNT, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
			}

			// Build average irradiance texture
			using ( PixelsBuffer content = new PixelsBuffer( ROUGHNESS_SUBDIVS_COUNT * 4 ) ) {
				using ( FileStream S = MSBRDFFileName2.OpenRead() )
					using ( BinaryReader R = new BinaryReader( S ) )
						using ( BinaryWriter W = content.OpenStreamWrite() ) {
							for ( uint X=0; X < ROUGHNESS_SUBDIVS_COUNT; X++ )
									W.Write( R.ReadSingle() );
						}


//////////////////////////////////////////////////////////////////////////
// Check single-scattering and multiple-scattering BRDFs are actual complements
//
/*
for ( uint Y=0; Y < ROUGHNESS_SUBDIVS_COUNT; Y++ ) {
	float	m = (float) Y / (ROUGHNESS_SUBDIVS_COUNT-1);
	float	m2 = Math.Max( 0.01f, m*m );

	for ( uint X=0; X < COS_THETA_SUBDIVS_COUNT; X++ ) {
		float	cosThetaO = (float) X / (COS_THETA_SUBDIVS_COUNT-1);
		float	sinThetaO = Mathf.Sqrt( 1 - cosThetaO*cosThetaO );

		const uint		THETA_SUBDIVS_COUNT = 512;
		const uint		PHI_SUBDIVS_COUNT = 2*512;

		float	integral = 0.0f;
		for ( uint THETA=0; THETA < THETA_SUBDIVS_COUNT; THETA++ ) {

			// Use cosine-weighted sampling
			float	sqCosThetaI = (0.5f+THETA) / THETA_SUBDIVS_COUNT;
			float	cosThetaI = Mathf.Sqrt( sqCosThetaI );
			float	sinThetaI = Mathf.Sqrt( 1 - sqCosThetaI );

// 			float	NdotL = cosThetaI;

			for ( uint PHI=0; PHI < PHI_SUBDIVS_COUNT; PHI++ ) {
				float	phi = Mathf.TWOPI * PHI / PHI_SUBDIVS_COUNT;

				// Compute cos(theta_h) = Omega_h.N where Omega_h = (Omega_i + Omega_o) / ||Omega_i + Omega_o|| is the half vector and N the surface normal
				float	cosThetaH = (cosThetaI + cosThetaO) / Mathf.Sqrt( 2 * (1 + cosThetaO * cosThetaI + sinThetaO * sinThetaI * Mathf.Sin( phi )) );
// 				float3	omega_i = new float3( sinThetaI * Mathf.Cos( phi ), sinThetaI * Mathf.Sin( phi ), cosThetaI );
// 				float3	omega_o = new float3( sinThetaO, 0, cosThetaO );
// 				float3	omega_h = (omega_i + omega_o).Normalized;
// 				float	cosThetaH = omega_h.z;

				// Compute GGX NDF
				float	den = 1 - cosThetaH*cosThetaH * (1 - m2);
				float	NDF = m2 / (Mathf.PI * den*den);

				// Compute Smith shadowing/masking
				float	Smith_i_den = NdotL + Mathf.Sqrt( m2 + (1-m2) * NdotL*NdotL );
				float	Smith_o_den = NdotV + Mathf.Sqrt( m2 + (1-m2) * NdotV*NdotV );

				// Full BRDF is thus...
				float	GGX = NDF / (Smith_i_den * Smith_o_den);

//				integral += GGX * cosThetaI * sinThetaI;
				integral += GGX;

			}
		}
	}
}
//*/
//////////////////////////////////////////////////////////////////////////


// verify BRDF + BRDFms integration = 1
// cube map + integration

				m_tex_IrradianceAverage = new Texture2D( m_Device, ROUGHNESS_SUBDIVS_COUNT, 1, 1, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
			}
		 }
		
		#endregion

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {

			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null )
				return;

			uint	W = (uint) panelOutput.Width;
			uint	H = (uint) panelOutput.Height;

			// Timer
			float	lastGameTime = m_CurrentGameTime;
			m_CurrentGameTime = GetGameTime();
			
			if ( m_CurrentGameTime - m_StartFPSTime > 1.0f ) {
				m_AverageFrameTime = (m_CurrentGameTime - m_StartFPSTime) / Math.Max( 1, m_SumFrames );
				m_SumFrames = 0;
				m_StartFPSTime = m_CurrentGameTime;
			}
			m_SumFrames++;

			m_CB_Global.m._ScreenSize.Set( W, H, 1.0f / W, 1.0f / H );
			m_CB_Global.m._Time = m_CurrentGameTime;
			m_CB_Global.UpdateData();

			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			m_Tex_Noise.Set( 8 );
			m_Tex_Noise4D.Set( 9 );


			float	sigma_t = floatTrackbarControlExtinction.Value;
			float	sigma_s = floatTrackbarControlExtinction.Value * floatTrackbarControlAlbedo.Value;
			float	phase_g = floatTrackbarControlPhaseAnisotropy.Value;


			//////////////////////////////////////////////////////////////////////////
			// Build Deforming Height Map
			if ( m_Shader_UpdateHeightMap.Use() ) {

				m_Tex_HeightMap.RemoveFromLastAssignedSlots();
				m_Tex_HeightMap.SetCSUAV( 0 );

				m_Shader_UpdateHeightMap.Dispatch( HEIGHTMAP_SIZE >> 4, HEIGHTMAP_SIZE >> 4, 1 );

				m_Tex_HeightMap.RemoveFromLastAssignedSlotUAV();
				m_Tex_HeightMap.Set( 11 );
			}

			//////////////////////////////////////////////////////////////////////////
			// Render volume density
			if ( m_shader_GenerateDensity.Use() ) {
				uint	GroupsCount = VOLUME_SIZE >> 3;

				m_CB_GenerateDensity.m._wsOffset.Set( 0.5f * m_CurrentGameTime, 0, 0 );
				m_CB_GenerateDensity.UpdateData();

				m_Tex_VolumeDensity.RemoveFromLastAssignedSlots();
				m_Tex_VolumeDensity.SetCSUAV( 0 );

				m_shader_GenerateDensity.Dispatch( GroupsCount, GroupsCount, GroupsCount );

				m_Tex_VolumeDensity.RemoveFromLastAssignedSlotUAV();
			}

			//////////////////////////////////////////////////////////////////////////
			// Splat photons
			{
				uint	GroupsCount = PHOTONS_COUNT >> 8;	// 256 threads per group

				m_Tex_AccumPhoton3D.RemoveFromLastAssignedSlots();
				m_Tex_AccumPhoton3D.SetCSUAV( 2 );

				// Clear
				if ( m_Shader_ClearAccumulator.Use() ) {
					m_Shader_ClearAccumulator.Dispatch( VOLUME_SIZE >> 2, VOLUME_SIZE >> 2, VOLUME_SIZE >> 2 );
				}

				// Init
				if ( m_Shader_InitPhotons.Use() ) {
					m_SB_PhotonInfos.RemoveFromLastAssignedSlots();
					m_SB_Photons[0].RemoveFromLastAssignedSlots();

					m_SB_PhotonInfos.SetOutput( 0 );
//					m_SB_Photons[0].SetOutput( 1 );
					m_Shader_InitPhotons.Dispatch( GroupsCount, 1, 1 );
				}

				// Trace
				if ( m_Shader_TracePhotons.Use() ) {

					m_CB_TracePhotons.m._Sigma_t = sigma_t;
					m_CB_TracePhotons.UpdateData();

					m_SB_PhotonInfos.SetInput( 0 );
					m_SB_Photons[0].SetInput( 1 );
					m_Tex_VolumeDensity.Set( 10 );

					m_Shader_TracePhotons.Dispatch( GroupsCount, 1, 1 );
				}

				m_Tex_AccumPhoton3D.RemoveFromLastAssignedSlotUAV();
			}

			//////////////////////////////////////////////////////////////////////////
			// Render room
			if ( m_Shader_RenderRoom.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_FRONT, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_Device.SetRenderTarget( m_Tex_TempBackBuffer, m_Device.DefaultDepthStencil );

				m_CB_RenderRoom.UpdateData();

				m_Prim_Cube.Render( m_Shader_RenderRoom );
			}

			//////////////////////////////////////////////////////////////////////////
			// Render sphere
			if ( m_Shader_RenderSphere.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.NOCHANGE, BLEND_STATE.NOCHANGE );
//				m_Device.SetRenderTarget( m_Tex_TempBackBuffer, m_Device.DefaultDepthStencil );

				m_CB_RenderSphere.UpdateData();

				m_Prim_Sphere.Render( m_Shader_RenderSphere );
			}

			//////////////////////////////////////////////////////////////////////////
			// Ray-March volume
			if ( m_Shader_RayMarcher.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_Device.SetRenderTargets( new IView[] { m_Tex_Scattering.GetView( 0, 1, 0, 1 ), m_Tex_Scattering.GetView( 0, 1, 1, 1 ) }, null );

				m_Tex_AccumPhoton3D.Set( 1 );

				m_CB_RayMarch.m._Sigma_t = sigma_t;
				m_CB_RayMarch.m._Sigma_s = sigma_s;
				m_CB_RayMarch.m._Phase_g = phase_g;
				m_CB_RayMarch.UpdateData();

				m_Prim_Quad.Render( m_Shader_RayMarcher );
			}

			//////////////////////////////////////////////////////////////////////////
			// Post-process
			if ( m_Shader_PostProcess.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );

				m_CB_PostProcess.UpdateData();


m_tex_IrradianceComplement.SetPS( 2 );
m_tex_IrradianceAverage.SetPS( 3 );


m_CB_RayMarch.m._Sigma_t = floatTrackbarControlExtinction.Value;
m_CB_RayMarch.m._Sigma_s = floatTrackbarControlAlbedo.Value;
m_CB_RayMarch.m._Phase_g = floatTrackbarControlPhaseAnisotropy.Value;
m_CB_RayMarch.UpdateData();

				m_Tex_TempBackBuffer.SetPS( 0 );
 				m_Tex_Scattering.SetPS( 1 );

				m_Prim_Quad.Render( m_Shader_PostProcess );

				m_Tex_Scattering.RemoveFromLastAssignedSlots();
			}

			// Show!
			m_Device.Present( false );

			// Update window text
			Text = "GloubiBoule - Avg. Frame Time " + (1000.0f * m_AverageFrameTime).ToString( "G5" ) + " ms (" + (1.0f / m_AverageFrameTime).ToString( "G5" ) + " FPS)";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			m_Device.ReloadModifiedShaders();
		}
	}
}
