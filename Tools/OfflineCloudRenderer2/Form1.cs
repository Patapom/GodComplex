#define DEBUG_INFOS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

using WMath;
using RendererManaged;

namespace OfflineCloudRenderer2
{
	public partial class Form1 : Form
	{
		#region CONSTANTS

		private const int		PHOTON_BATCH_SIZE = 1024;		// Must correspond to threads count in compute shader
//		private const int		PHOTONS_COUNT = 10000 * PHOTON_BATCH_SIZE;
		private const int		PHOTONS_COUNT = 1000 * PHOTON_BATCH_SIZE;

		private const int		BOUNCES_COUNT = 1;				// Maximum amount of bounces photons should perform before stopping shooting

		private const float		LOW_PHOTONS_COUNT_RATIO = 0.05f;// If less than 5% photons remaing to be shot, abort shooting...


		private const int		RANDOM_TABLE_SIZE = 4 * 1024 * 1024;

		private const int		LAYERS_COUNT = 8;				// Amount of layers the cloudscape will be split into

		// Density field 3D texture size will be DENSITY_FIELD_SIZE*DENSITY_FIELD_SIZE*DENSITY_FIELD_HEIGHT
		private const int		DENSITY_FIELD_SIZE = 512;
		private const int		DENSITY_FIELD_HEIGHT = 64;

		// Cloudscape coverage in meters
		private const float		CLOUDSCAPE_SIZE = 1000.0f;
//		private const float		CLOUDSCAPE_HEIGHT = 100.0f;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_Camera
		{
			public float4		CameraData;		// X=tan(FOV_H/2) Y=tan(FOV_V/2) Z=Near W=Far
			public float4x4		Camera2World;
			public float4x4		World2Camera;
			public float4x4		Camera2Proj;
			public float4x4		Proj2Camera;
			public float4x4		World2Proj;
			public float4x4		Proj2World;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_Render
		{
			public float3		CloudScapeSize;			// Size of the cloud scape covered by the 3D texture of densities
			public uint			LayersCount;			// Amount of layers

			public uint			StartLayerIndex;		// Start layer for display
			public float		IntensityFactor;		// Multiplier for display intensity
			public uint			DisplayType;			// 0=Flux, 1=Directions, bit #2 for normalization by weight
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_PhotonShooterInput
		{
			public uint			LayerIndex;				// Index of the layer we start shooting photons from, most significant bit is set to indicate direction (1 is bottom to top, is top to bottom)
			public uint			LayersCount;			// Total amount of layers
			public float		LayerThickness;			// Thickness of each individual layer (in meters)
			public float		SigmaScattering;		// Scattering coefficient (in m^-1)

			public float3		CloudScapeSize;			// Size of the cloud scape covered by the 3D texture of densities
			public uint			MaxScattering;			// Maximum scattering events before discarding the photon

			public uint			BatchIndex;				// Photon batch index
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_SplatPhoton
		{
			public float3		CloudScapeSize;			// Size of the cloud scape covered by the 3D texture of densities
 			public float		SplatSize;				// Splat size in NDC space

 			public float		SplatIntensity;			// Global intensity multiplier
			public uint			LayerIndex;				// Index of the layer we splat photons to, most significant bit is set to indicate direction (1 is bottom to top, is top to bottom)
		}

		// Structured buffers
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		[System.Diagnostics.DebuggerDisplay( "P=({Position.x}, {Position.y}) D={Direction} RGBE={RGBE}" )]
		public struct	SB_Photon
		{
			public float2		Position;				// Position on the layer
			public uint			Direction;				// Packed Phi, Theta each on 2 UINT16
			public uint			RGBE;					// RGBE-encoded color
#if DEBUG_INFOS
			public float4		Infos;
#endif
		}

		#endregion

		#region FIELDS

		private RegistryKey					m_AppKey;
		private string						m_ApplicationPath;

		private Device						m_Device = new Device();

		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Render>	m_CB_Render = null;

		// Photon Shooter
		private ComputeShader							m_CS_PhotonShooter = null;
		private ConstantBuffer<CB_PhotonShooterInput>	m_CB_PhotonShooterInput = null;

		private StructuredBuffer<float>		m_SB_PhaseQuantile = null;
		private StructuredBuffer<float4>	m_SB_Random = null;

		private StructuredBuffer<SB_Photon>	m_SB_Photons = null;

		private StructuredBuffer<uint>		m_SB_PhotonLayerIndices = null;
		private StructuredBuffer<uint>		m_SB_ProcessedPhotonsCounter = null;

		private Texture3D					m_Tex_DensityField = null;

		private float3						m_CloudScapeSize;

		// Photons Splatter
		private Shader						m_PS_PhotonSplatter	= null;
		private ConstantBuffer<CB_SplatPhoton>	m_CB_SplatPhoton;
		private Primitive					m_Prim_Point = null;

		private Texture3D					m_Tex_PhotonLayers_Flux = null;
		private Texture3D					m_Tex_PhotonLayers_Direction = null;

		// Photons Renderer
		private Shader						m_PS_RenderLayer = null;
		private Primitive					m_Prim_Quad = null;
		private Primitive					m_Prim_Cube = null;

// 		// Vectors Renderer
// 		private Shader						m_PS_RenderPhotonVectors;
// 		private ConstantBuffer<CB_RenderPhotonVector>	m_CB_RenderPhotonVector;
// 		private Primitive					m_Prim_Line;

		private Shader						m_PS_RenderWorldCube = null;

		private List<IDisposable>			m_Disposables = new List<IDisposable>();

		private CameraManipulator			m_Manipulator = new CameraManipulator();

		#endregion

		#region METHODS

		public Form1()
		{
			InitializeComponent();
			viewportPanel.Device = m_Device;

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\OfflineCloudsRenderer" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			m_Device.Init( viewportPanel.Handle, false, true );
			m_Device.Clear( m_Device.DefaultTarget, new RendererManaged.float4( Color.SkyBlue, 1 ) );

			Reg( m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 0 ) );
			Reg( m_CB_Render = new ConstantBuffer<CB_Render>( m_Device, 8 ) );

			//////////////////////////////////////////////////////////////////////////
			// Photon Shooter
#if DEBUG_INFOS
			ShaderMacro[]	Macros = new ShaderMacro[] {
				new ShaderMacro( "DEBUG", "" )
			};
#else
			ShaderMacro[]	Macros = null;
#endif


			Reg( m_CS_PhotonShooter = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/LayeredRenderer/PhotonShooter.hlsl" ) ), "CS", Macros ) );
			Reg( m_CB_PhotonShooterInput = new ConstantBuffer<CB_PhotonShooterInput>( m_Device, 8 ) );

			BuildPhaseQuantileBuffer( new System.IO.FileInfo( @"Mie65536x2.float" ) );
			BuildRandomBuffer();

			Reg( m_SB_Photons = new StructuredBuffer<SB_Photon>( m_Device, PHOTONS_COUNT, true ) );

			Reg( m_SB_PhotonLayerIndices = new StructuredBuffer<uint>( m_Device, PHOTONS_COUNT, true ) );
			Reg( m_SB_ProcessedPhotonsCounter = new StructuredBuffer<uint>( m_Device, 1, true ) );

			Build3DDensityField();


			//////////////////////////////////////////////////////////////////////////
			// Photons Splatter
			Reg( m_PS_PhotonSplatter = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/LayeredRenderer/SplatPhoton.hlsl" ) ), VERTEX_FORMAT.P3, "VS", "GS", "PS", Macros ) );

			Reg( m_CB_SplatPhoton = new ConstantBuffer<CB_SplatPhoton>( m_Device, 8 ) );

			Reg( m_Tex_PhotonLayers_Flux = new Texture3D( m_Device, 512, 512, LAYERS_COUNT+1, 1, PIXEL_FORMAT.RGBA16_FLOAT, false, true, null ) );
			Reg( m_Tex_PhotonLayers_Direction = new Texture3D( m_Device, 512, 512, LAYERS_COUNT+1, 1, PIXEL_FORMAT.RGBA16_FLOAT, false, true, null ) );

			// Build a single point that will be instanced as many times as there are photons
			{
				ByteBuffer	Point = new ByteBuffer( 3*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)) );
				Reg( m_Prim_Point = new Primitive( m_Device, 1, Point, null, Primitive.TOPOLOGY.POINT_LIST, VERTEX_FORMAT.P3 ) );
			}


			//////////////////////////////////////////////////////////////////////////
			// Photons Renderer
			Reg( m_PS_RenderLayer = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/LayeredRenderer/DisplayPhotonLayer.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null ) );
			Reg( m_PS_RenderWorldCube = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/DisplayWorldCube.hlsl" ) ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null ) );

			BuildQuad();
			BuildCube();


// 			//////////////////////////////////////////////////////////////////////////
// 			// Photon Vectors Renderer
// 			Reg( m_PS_RenderPhotonVectors = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/CanonicalCubeRenderer/DisplayPhotonVector.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null ) );
// 			Reg( m_CB_RenderPhotonVector = new ConstantBuffer<CB_RenderPhotonVector>( m_Device, 8 ) );
// 			{
// 				ByteBuffer	Line = VertexP3.FromArray( new VertexP3[] { new VertexP3() { P = new float3( 0, 0, 0 ) }, new VertexP3() { P = new float3( 1, 0, 0 ) } } );
// 				Reg( m_Prim_Line = new Primitive( m_Device, 2, Line, null, Primitive.TOPOLOGY.LINE_LIST, VERTEX_FORMAT.P3 ) );
// 			}

			// Create the camera manipulator
			m_CB_Camera.m.Camera2World = float4x4.Identity;
			UpdateCameraProjection( 60.0f * (float) Math.PI / 180.0f, (float) viewportPanel.Width / viewportPanel.Height, 0.1f, 100.0f );

			m_Manipulator.Attach( viewportPanel );
			m_Manipulator.CameraTransformChanged += new CameraManipulator.UpdateCameraTransformEventHandler( Manipulator_CameraTransformChanged );
			m_Manipulator.InitializeCamera( new float3( 0.0f, 0.0f, 4.0f ), new float3( 0, 0, 0 ), new float3( 0, 1, 0 ) );

	
			integerTrackbarControlLayerDisplayStart.RangeMax = LAYERS_COUNT;
			integerTrackbarControlLayerDisplayStart.VisibleRangeMax = LAYERS_COUNT;
			integerTrackbarControlLayerDisplayEnd.RangeMax = LAYERS_COUNT+1;
			integerTrackbarControlLayerDisplayEnd.VisibleRangeMax = LAYERS_COUNT+1;
			integerTrackbarControlLayerDisplayEnd.Value = LAYERS_COUNT+1;
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			foreach ( IDisposable D in m_Disposables )
				D.Dispose();

			m_Device.Exit();
//			m_Device = null;

			base.OnClosing( e );
		}

		private void	Build3DDensityField()
		{
			PixelsBuffer	DensityField = new PixelsBuffer( DENSITY_FIELD_SIZE*DENSITY_FIELD_SIZE*DENSITY_FIELD_HEIGHT );
			byte	D;

			float3	P;
			float3	C = new float3( 0.5f, 0.5f, 0.5f );
			using ( System.IO.BinaryWriter W = DensityField.OpenStreamWrite() )
				for ( int X=0; X < DENSITY_FIELD_SIZE; X++ )
				{
					P.x = (0.5f+X) / DENSITY_FIELD_SIZE;
					for ( int Y=0; Y < DENSITY_FIELD_HEIGHT; Y++ )
					{
						P.y = (0.5f+Y) / DENSITY_FIELD_HEIGHT;
						for ( int Z=0; Z < DENSITY_FIELD_SIZE; Z++ )
						{
							P.z = (0.5f+Z) / DENSITY_FIELD_SIZE;

//							D = 0;	// Empty for now: photons should go straight through!

							D = (byte) ((P - C).LengthSquared < 0.125f ? 255 : 0);

							W.Write( D );
						}
					}
				}

			Reg( m_Tex_DensityField = new Texture3D( m_Device, DENSITY_FIELD_SIZE, DENSITY_FIELD_HEIGHT, DENSITY_FIELD_SIZE, 1, PIXEL_FORMAT.R8_UNORM, false, false, new PixelsBuffer[] { DensityField } ) );

			DensityField.Dispose();
		}

		private void	BuildPhaseQuantileBuffer( System.IO.FileInfo _PhaseQuantileFileName )
		{
			const int	QUANTILES_COUNT = 65536;

			Reg( m_SB_PhaseQuantile = new StructuredBuffer<float>( m_Device, 2*QUANTILES_COUNT, true ) );
			using ( System.IO.FileStream S = _PhaseQuantileFileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) )
				{
					for ( int i=0; i < m_SB_PhaseQuantile.m.Length; i++ )
						m_SB_PhaseQuantile.m[i] = R.ReadSingle();
				}
			m_SB_PhaseQuantile.Write();
		}

		private void	BuildRandomBuffer()
		{
			Reg( m_SB_Random = new StructuredBuffer<float4>( m_Device, RANDOM_TABLE_SIZE, true ) );
			for ( int i=0; i < RANDOM_TABLE_SIZE; i++ )
//				m_SB_Random.m[i] = new float4( (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform() );
				m_SB_Random.m[i] = new float4( (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), (float) SimpleRNG.GetUniform(), -(float) Math.Log( 1e-3 + (1.0-1e-3) * SimpleRNG.GetUniform() ) );
			m_SB_Random.Write();
		}

		private void	BuildCube()
		{
			VertexP3N3[]	Vertices = new VertexP3N3[4*6];
			Vertices[4*0+0] = new VertexP3N3() { P = new float3( +1, +1, +1 ), N = new float3( 0, 0, 0 ) };	// +X
			Vertices[4*0+1] = new VertexP3N3() { P = new float3( +1, -1, +1 ), N = new float3( 0, 1, 0 ) };
			Vertices[4*0+2] = new VertexP3N3() { P = new float3( +1, -1, -1 ), N = new float3( 1, 1, 0 ) };
			Vertices[4*0+3] = new VertexP3N3() { P = new float3( +1, +1, -1 ), N = new float3( 1, 0, 0 ) };

			Vertices[4*1+0] = new VertexP3N3() { P = new float3( -1, +1, -1 ), N = new float3( 0, 0, 1 ) };	// -X
			Vertices[4*1+1] = new VertexP3N3() { P = new float3( -1, -1, -1 ), N = new float3( 0, 1, 1 ) };
			Vertices[4*1+2] = new VertexP3N3() { P = new float3( -1, -1, +1 ), N = new float3( 1, 1, 1 ) };
			Vertices[4*1+3] = new VertexP3N3() { P = new float3( -1, +1, +1 ), N = new float3( 1, 0, 1 ) };

			Vertices[4*2+0] = new VertexP3N3() { P = new float3( -1, +1, -1 ), N = new float3( 0, 0, 2 ) };	// +Y
			Vertices[4*2+1] = new VertexP3N3() { P = new float3( -1, +1, +1 ), N = new float3( 0, 1, 2 ) };
			Vertices[4*2+2] = new VertexP3N3() { P = new float3( +1, +1, +1 ), N = new float3( 1, 1, 2 ) };
			Vertices[4*2+3] = new VertexP3N3() { P = new float3( +1, +1, -1 ), N = new float3( 1, 0, 2 ) };

			Vertices[4*3+0] = new VertexP3N3() { P = new float3( -1, -1, +1 ), N = new float3( 0, 0, 3 ) };	// -Y
			Vertices[4*3+1] = new VertexP3N3() { P = new float3( -1, -1, -1 ), N = new float3( 0, 1, 3 ) };
			Vertices[4*3+2] = new VertexP3N3() { P = new float3( +1, -1, -1 ), N = new float3( 1, 1, 3 ) };
			Vertices[4*3+3] = new VertexP3N3() { P = new float3( +1, -1, +1 ), N = new float3( 1, 0, 3 ) };

			Vertices[4*4+0] = new VertexP3N3() { P = new float3( -1, +1, +1 ), N = new float3( 0, 0, 4 ) };	// +Z
			Vertices[4*4+1] = new VertexP3N3() { P = new float3( -1, -1, +1 ), N = new float3( 0, 1, 4 ) };
			Vertices[4*4+2] = new VertexP3N3() { P = new float3( +1, -1, +1 ), N = new float3( 1, 1, 4 ) };
			Vertices[4*4+3] = new VertexP3N3() { P = new float3( +1, +1, +1 ), N = new float3( 1, 0, 4 ) };

			Vertices[4*5+0] = new VertexP3N3() { P = new float3( +1, +1, -1 ), N = new float3( 0, 0, 5 ) };	// -Z
			Vertices[4*5+1] = new VertexP3N3() { P = new float3( +1, -1, -1 ), N = new float3( 0, 1, 5 ) };
			Vertices[4*5+2] = new VertexP3N3() { P = new float3( -1, -1, -1 ), N = new float3( 1, 1, 5 ) };
			Vertices[4*5+3] = new VertexP3N3() { P = new float3( -1, +1, -1 ), N = new float3( 1, 0, 5 ) };

			UInt32[]	Indices = new UInt32[6*6];
			for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
			{
				Indices[6*FaceIndex+0] = (uint) (4*FaceIndex+0);
				Indices[6*FaceIndex+1] = (uint) (4*FaceIndex+1);
				Indices[6*FaceIndex+2] = (uint) (4*FaceIndex+2);

				Indices[6*FaceIndex+3] = (uint) (4*FaceIndex+0);
				Indices[6*FaceIndex+4] = (uint) (4*FaceIndex+2);
				Indices[6*FaceIndex+5] = (uint) (4*FaceIndex+3);
			}

			ByteBuffer	VerticesBuffer = VertexP3N3.FromArray( Vertices );

			Reg( m_Prim_Cube = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3 ) );
		}

		private void	BuildQuad()
		{
			VertexPt4[]	Vertices = new VertexPt4[] {
				new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) },
				new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) },
				new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) },
				new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) },
			};

			ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

			Reg( m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 ) );
		}

		private void	UpdateCameraTransform( float3 _Position, float3 _Target, float3 _Up )
		{
			m_CB_Camera.m.Camera2World.MakeLookAt( _Position, _Target, _Up );
		}

		private void	UpdateCameraTransform( float4x4 _Camera2World )
		{
			m_CB_Camera.m.Camera2World = _Camera2World;
			m_CB_Camera.m.World2Camera = m_CB_Camera.m.Camera2World.Inverse;

			UpdateCameraCompositions();
		}

		private void	UpdateCameraProjection( float _FOV, float _AspectRatio, float _Near, float _Far )
		{
			float	TanHalfFOV = (float) Math.Tan( 0.5 * _FOV );
			m_CB_Camera.m.CameraData = new float4( _AspectRatio * TanHalfFOV, TanHalfFOV, _Near, _Far );

			m_CB_Camera.m.Camera2Proj.MakeProjectionPerspective( _FOV, _AspectRatio, _Near, _Far );
			m_CB_Camera.m.Proj2Camera = m_CB_Camera.m.Camera2Proj.Inverse;

			UpdateCameraCompositions();
		}

		private void	UpdateCameraCompositions()
		{
			m_CB_Camera.m.World2Proj = m_CB_Camera.m.World2Camera * m_CB_Camera.m.Camera2Proj;
			m_CB_Camera.m.Proj2World = m_CB_Camera.m.Proj2Camera * m_CB_Camera.m.Camera2World;
			m_CB_Camera.UpdateData();
		}

		private void	Render()
		{
			m_Device.Clear( m_Device.DefaultTarget, 0.5f * new float4( Color.SkyBlue, 1 ) );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1, 0, true, false );

 
// 			// Render the photon vectors
// 			if ( checkBoxRenderVectors.Checked )
// 			{
// 				const int		PHOTON_VECTORS_COUNT_PER_FACE = 10000;
// 				m_CB_RenderPhotonVector.m.VectorsPerFace = PHOTON_VECTORS_COUNT_PER_FACE;
// 				m_CB_RenderPhotonVector.m.VectorMultiplier = floatTrackbarControlVectorSize.Value;
// 				m_CB_RenderPhotonVector.m.ClipAboveValue = checkBoxClipAboveValue.Checked ? 0.01f * floatTrackbarControlClipAbove.Value : 1e6f;
// 				m_CB_RenderPhotonVector.UpdateData();
// 				m_PS_RenderPhotonVectors.Use();
// 				m_Prim_Line.RenderInstanced( m_PS_RenderPhotonVectors, 6*PHOTON_VECTORS_COUNT_PER_FACE );
// 			}


			// Render photon layers
 			m_CB_Render.m.CloudScapeSize = m_CloudScapeSize;
			m_CB_Render.m.LayersCount = LAYERS_COUNT;
			m_CB_Render.m.StartLayerIndex = (uint) integerTrackbarControlLayerDisplayStart.Value;
			m_CB_Render.m.IntensityFactor = floatTrackbarControlDisplayIntensity.Value;
			m_CB_Render.m.DisplayType = (uint) ((radioButtonShowDirection.Checked ? 1 : (radioButtonShowDensityField.Checked ? 2 : 0))
									  | (checkBoxShowNormalized.Checked ? 4 : 0));
			m_CB_Render.UpdateData();

			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );

			m_Tex_PhotonLayers_Flux.SetPS( 0 );
			m_Tex_PhotonLayers_Direction.SetPS( 1 );
			m_Tex_DensityField.SetPS( 2 );

			m_PS_RenderLayer.Use();

			int	instancesCount = Math.Max( 1, integerTrackbarControlLayerDisplayEnd.Value - integerTrackbarControlLayerDisplayStart.Value );
			m_Prim_Quad.RenderInstanced( m_PS_RenderLayer, instancesCount );

			// Render the world cube
 			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );
			m_PS_RenderWorldCube.Use();
			m_Prim_Cube.Render( m_PS_RenderWorldCube );

			// Refresh
			viewportPanel.Invalidate();
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
		}
		private DialogResult	MessageBox( string _Text, Exception _e )
		{
			return MessageBox( _Text + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			return MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			return MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			return System.Windows.Forms.MessageBox.Show( this, _Text, "Cloud Renderer", _Buttons, _Icon );
		}

		/// <summary>
		/// Registers a disposable that will get disposed on form closing
		/// </summary>
		/// <param name="_Disposable"></param>
		private void	Reg( IDisposable _Disposable )
		{
			m_Disposables.Add( _Disposable );
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		void	Manipulator_CameraTransformChanged( float4x4 _Camera2World )
		{
			UpdateCameraTransform( _Camera2World );
			Render();
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			m_Device.ReloadModifiedShaders();
			Render();
		}

		private void floatTrackbarControlDebug3_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Render();
		}

		private void floatTrackbarControlDisplayIntensity_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Render();
		}

		private void radioButtonShowFlux_CheckedChanged( object sender, EventArgs e )
		{
			Render();
		}

		private void radioButtonShowDirection_CheckedChanged( object sender, EventArgs e )
		{
			Render();
		}

		private void checkBoxShowNormalized_CheckedChanged( object sender, EventArgs e )
		{
			Render();
		}

		private void integerTrackbarControlLayerDisplayStart_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			Render();
		}

		private void integerTrackbarControlLayerDisplayEnd_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			Render();
		}

		/// <summary>
		/// Packs a direction and layer index into a single uint expected in the "Data" field of the Photon structure
		/// </summary>
		/// <param name="_Direction"></param>
		/// <param name="_LayerIndex"></param>
		/// <returns></returns>
		private uint	PackPhotonDirection( float3 _Direction )
		{
			double	Phi = Math.PI + Math.Atan2( _Direction.x, _Direction.z );
			ushort	nPhi = (ushort) (65535.0f * Phi / (2.0 * Math.PI));

			double	Theta = Math.Acos( _Direction.y );
			ushort	nTheta = (ushort) (65535.0f * Theta / Math.PI);

			uint	Result = (uint) (nPhi | (nTheta << 16));
			return Result;
		}

		private uint	EncodeRGBE( float3 _Color )
		{
			float	Max = Math.Max( Math.Max( _Color.x, _Color.y ), _Color.z );
			float	Exponent = (float) Math.Ceiling( Math.Log( Max ) / Math.Log( 2.0 ) );
			_Color *= (float) Math.Pow( 2.0f, -Exponent );	// Normalize components

			byte	R = (byte) (255.0f * _Color.x);
			byte	G = (byte) (255.0f * _Color.y);
			byte	B = (byte) (255.0f * _Color.z);
			byte	E = (byte) (Exponent + 128);
			uint	RGBE = (uint) (R | ((G | ((B | (E << 8)) << 8 )) << 8));
			return RGBE;
		}

		private void buttonShootPhotons_Click( object sender, EventArgs e )
		{
			//////////////////////////////////////////////////////////////////////////
			// 1] Build initial photon positions and directions
			float3	SunDirection = new float3( 0, 1, 0 ).Normalized;

			uint	InitialDirection = PackPhotonDirection( -SunDirection );
			uint	InitialColor = EncodeRGBE( new float3( 1.0f, 1.0f, 1.0f ) );

			int		PhotonsPerSize = (int) Math.Floor( Math.Sqrt( PHOTONS_COUNT ) );
			float	PhotonCoverageSize = CLOUDSCAPE_SIZE / PhotonsPerSize;

			for ( int PhotonIndex=0; PhotonIndex < PHOTONS_COUNT; PhotonIndex++ )
			{
				int	Z = PhotonIndex / PhotonsPerSize;
				int	X = PhotonIndex - Z * PhotonsPerSize;

				float	x = ((X + (float) SimpleRNG.GetUniform()) / PhotonsPerSize - 0.5f) * CLOUDSCAPE_SIZE;
				float	z = ((Z + (float) SimpleRNG.GetUniform()) / PhotonsPerSize - 0.5f) * CLOUDSCAPE_SIZE;

				m_SB_Photons.m[PhotonIndex].Position.Set( x, z );
				m_SB_Photons.m[PhotonIndex].Direction = InitialDirection;
				m_SB_Photons.m[PhotonIndex].RGBE = InitialColor;

#if DEBUG_INFOS
				m_SB_Photons.m[PhotonIndex].Infos.Set( 0, 0, 0, 0 );	// Will store scattering events counter, marched length, steps count, etc.
#endif
			}
			m_SB_Photons.Write();


			//////////////////////////////////////////////////////////////////////////
			// 2] Initialize layers & textures

			// 2.1) Fill source bucket with all photons
			for ( int PhotonIndex=0; PhotonIndex < PHOTONS_COUNT; PhotonIndex++ )
				m_SB_PhotonLayerIndices.m[PhotonIndex] = 0U;	// Starting from top layer, direction is down
			m_SB_PhotonLayerIndices.Write();

			// 2.2) Clear photon splatting texture
			m_Device.Clear( m_Tex_PhotonLayers_Flux, new float4( 0, 0, 0, 0 ) );
			m_Device.Clear( m_Tex_PhotonLayers_Direction, new float4( 0, 0, 0, 0 ) );


			//////////////////////////////////////////////////////////////////////////
			// 3] Prepare buffers & states

			m_CloudScapeSize.Set( CLOUDSCAPE_SIZE, floatTrackbarControlCloudscapeThickness.Value, CLOUDSCAPE_SIZE );

			// 3.1) Prepare density field
			m_Tex_DensityField.SetCS( 2 );

			// 3.2) Constant buffer for photon shooting
			m_CB_PhotonShooterInput.m.LayersCount = LAYERS_COUNT;
			m_CB_PhotonShooterInput.m.MaxScattering = 30;
			m_CB_PhotonShooterInput.m.LayerThickness = m_CloudScapeSize.y / LAYERS_COUNT;
			m_CB_PhotonShooterInput.m.SigmaScattering = floatTrackbarControlSigmaScattering.Value;	// 0.04523893421169302263386206471922f;	// re=6µm Gamma=2 N0=4e8   Sigma_t = N0 * PI * re²
			m_CB_PhotonShooterInput.m.CloudScapeSize = m_CloudScapeSize;

			// 3.3) Prepare photon splatting buffer & states
			m_CB_SplatPhoton.m.CloudScapeSize = m_CloudScapeSize;
			m_CB_SplatPhoton.m.SplatSize = 1.0f * (2.0f / m_Tex_PhotonLayers_Flux.Width);
 			m_CB_SplatPhoton.m.SplatIntensity = 1.0f;// 1000.0f / PHOTONS_COUNT;

			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ADDITIVE );	// Splatting is additive
			m_Tex_PhotonLayers_Flux.RemoveFromLastAssignedSlots();
			m_Tex_PhotonLayers_Direction.RemoveFromLastAssignedSlots();


			//////////////////////////////////////////////////////////////////////////
			// 4] Splat initial photons to the top layer
			m_PS_PhotonSplatter.Use();

			m_SB_Photons.SetInput( 0 );				// RO version for splatting
			m_SB_PhotonLayerIndices.SetInput( 1 );	// RO version for splatting

			m_CB_SplatPhoton.m.LayerIndex = 0U;
			m_CB_SplatPhoton.UpdateData();

			m_Device.SetRenderTargets( new View3D[] {
				m_Tex_PhotonLayers_Flux.GetView( 0, 0, 0, 1 ),
				m_Tex_PhotonLayers_Direction.GetView( 0, 0, 0, 1 )
			}, null );
			m_Prim_Point.RenderInstanced( m_PS_PhotonSplatter, PHOTONS_COUNT );


			//////////////////////////////////////////////////////////////////////////
			// 5] Render loop
			int	BatchesCount = PHOTONS_COUNT / PHOTON_BATCH_SIZE;

			m_SB_ProcessedPhotonsCounter.SetOutput( 2 );

 			for ( int BounceIndex=0; BounceIndex < BOUNCES_COUNT; BounceIndex++ )
			{
				// 5.1] Process every layers from top to bottom
				m_SB_ProcessedPhotonsCounter.m[0] = 0;
				m_SB_ProcessedPhotonsCounter.Write();	// Reset processed photons counter

				for ( int LayerIndex=0; LayerIndex < LAYERS_COUNT; LayerIndex++ )
				{
					// 5.1.1) Shoot a bunch of photons from layer "LayerIndex" to layer "LayerIndex+1"
					m_CS_PhotonShooter.Use();

					m_CB_PhotonShooterInput.m.LayerIndex = (uint) LayerIndex;

					m_SB_Photons.RemoveFromLastAssignedSlots();
					m_SB_PhotonLayerIndices.RemoveFromLastAssignedSlots();

					m_SB_Photons.SetOutput( 0 );
					m_SB_PhotonLayerIndices.SetOutput( 1 );

					m_SB_Random.SetInput( 0 );
					m_SB_PhaseQuantile.SetInput( 1 );

					for ( int BatchIndex=0; BatchIndex < BatchesCount; BatchIndex++ )
					{
						m_CB_PhotonShooterInput.m.BatchIndex = (uint) BatchIndex;
						m_CB_PhotonShooterInput.UpdateData();

						m_CS_PhotonShooter.Dispatch( 1, 1, 1 );

						m_Device.Present( true );

						// Notify of progress
						progressBar1.Value = progressBar1.Maximum * (1+BatchIndex+BatchesCount*(LayerIndex+LAYERS_COUNT*BounceIndex)) / (BOUNCES_COUNT*LAYERS_COUNT*BatchesCount);
						Application.DoEvents();
					}

#if DEBUG_INFOS
//DEBUG Read back photons buffer
m_SB_Photons.Read();
// m_SB_PhotonLayerIndices.Read();
// Verify photons have the same energy and were indeed transported to the next layer unaffected (this test is only valid if the density field is filled with 0s)
// for ( int PhotonIndex=0; PhotonIndex < PHOTONS_COUNT; PhotonIndex++ )
// {
// 	if ( m_SB_Photons.m[PhotonIndex].RGBE != 0x80FFFFFF )
// 		throw new Exception( "Intensity changed!" );
// 	if ( m_SB_PhotonLayerIndices.m[PhotonIndex] != LayerIndex+1 )
// 		throw new Exception( "Unexpected layer index!" );
// }
//DEBUG
#endif

					// 5.1.2) Splat the photons that got through to the 2D texture array
					m_PS_PhotonSplatter.Use();

					m_SB_Photons.SetInput( 0 );				// RO version for splatting
					m_SB_PhotonLayerIndices.SetInput( 1 );	// RO version for splatting

					m_CB_SplatPhoton.m.LayerIndex = (uint) (LayerIndex+1);
					m_CB_SplatPhoton.UpdateData();

					m_Device.SetRenderTargets( new View3D[] {
						m_Tex_PhotonLayers_Flux.GetView( 0, 0, LayerIndex+1, 1 ),
						m_Tex_PhotonLayers_Direction.GetView( 0, 0, LayerIndex+1, 1 )
					}, null );
					m_Prim_Point.RenderInstanced( m_PS_PhotonSplatter, PHOTONS_COUNT );
				}

				m_SB_ProcessedPhotonsCounter.Read();
				if ( m_SB_ProcessedPhotonsCounter.m[0] < LOW_PHOTONS_COUNT_RATIO * PHOTONS_COUNT )
					break;	// We didn't shoot a significant number of photons to go on...

				// ================================================================================
				// 5.2] Process every layers from bottom to top
				BounceIndex++;
				if ( BounceIndex >= BOUNCES_COUNT )
					break;

				m_SB_ProcessedPhotonsCounter.m[0] = 0;
				m_SB_ProcessedPhotonsCounter.Write();	// Reset processed photons counter

				for ( int LayerIndex=LAYERS_COUNT; LayerIndex > 0; LayerIndex-- )
				{
					// 5.2.1) Shoot a bunch of photons from layer "LayerIndex" to layer "LayerIndex-1"
					m_CS_PhotonShooter.Use();

					m_CB_PhotonShooterInput.m.LayerIndex = (uint) LayerIndex | 0x80000000U;	// <= MSB indicates photons are going up

					m_SB_Photons.RemoveFromLastAssignedSlots();
					m_SB_PhotonLayerIndices.RemoveFromLastAssignedSlots();

					m_SB_Photons.SetOutput( 0 );
					m_SB_PhotonLayerIndices.SetOutput( 1 );

					m_SB_Random.SetInput( 0 );
					m_SB_PhaseQuantile.SetInput( 1 );

					for ( int BatchIndex=0; BatchIndex < BatchesCount; BatchIndex++ )
					{
						m_CB_PhotonShooterInput.m.BatchIndex = (uint) BatchIndex;
						m_CB_PhotonShooterInput.UpdateData();

						m_CS_PhotonShooter.Dispatch( 1, 1, 1 );

						m_Device.Present( true );

						// Notify of progress
						progressBar1.Value = progressBar1.Maximum * (1+BatchIndex+BatchesCount*(LayerIndex+LAYERS_COUNT*BounceIndex)) / (BOUNCES_COUNT*LAYERS_COUNT*BatchesCount);
						Application.DoEvents();
					}

					// 5.2.2) Splat the photons that got through to the 2D texture array
					m_PS_PhotonSplatter.Use();

					m_SB_Photons.SetInput( 0 );				// RO version for splatting
					m_SB_PhotonLayerIndices.SetInput( 1 );	// RO version for splatting

					m_CB_SplatPhoton.m.LayerIndex = (uint) (LayerIndex-1) | 0x80000000U;	// <= MSB indicates photons are going up
					m_CB_SplatPhoton.UpdateData();

					m_Device.SetRenderTargets( new View3D[] {
						m_Tex_PhotonLayers_Flux.GetView( 0, 0, LayerIndex-1, 0 ),
						m_Tex_PhotonLayers_Direction.GetView( 0, 0, LayerIndex-1, 0 )
					}, null );
					m_Prim_Point.RenderInstanced( m_PS_PhotonSplatter, PHOTONS_COUNT );
				}

				m_SB_ProcessedPhotonsCounter.Read();
				if ( m_SB_ProcessedPhotonsCounter.m[0] < LOW_PHOTONS_COUNT_RATIO * PHOTONS_COUNT )
					break;	// We didn't shoot a significant number of photons to go on...
			}

			m_Tex_PhotonLayers_Flux.RemoveFromLastAssignedSlots();
			m_Tex_PhotonLayers_Direction.RemoveFromLastAssignedSlots();

			Render();
		}

		#endregion
	}
}
