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

namespace OfflineCloudRenderer
{
	public partial class Form1 : Form
	{
		#region CONSTANTS

		private const int		PHOTON_BATCH_SIZE = 1024;	// Must correspond to threads count in compute shader
		private const int		PHOTONS_COUNT = 10000 * PHOTON_BATCH_SIZE;

		private const int		RANDOM_TABLE_SIZE = 4 * 1024 * 1024;

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
			public float4		TargetDimensions;	// XY=Target dimensions, ZW=1/XY
			public float4		Debug;
			public float		FluxMultiplier;
 			public uint			SplatType;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_PhotonShooterInput
		{
			public float2		InitialPosition;		// Initial beam position in [-1,+1] on the top side of the cube (Y=+1)
			public float2		InitialIncidence;		// Initial beam angular incidence (Phi,Theta)
			public float		CubeSize;				// Size of the canonical cube in meters
			public float		SigmaScattering;		// Scattering coefficient (in m^-1)
			public uint			MaxScattering;			// Maximum scattering events before exiting the cube (default is 30)
			public uint			BatchIndex;				// Photons batch index
			public uint			FullSurface;			// Full surface random location
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_SplatPhoton
		{
 			public float		SplatSize;
 			public float		SplatIntensity;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	SB_PhotonOut
		{
			public float3		ExitPosition;			// Photon exit position in [-1,+1]
			public float3		ExitDirection;			// Photon exit direction
			public float		MarchedLength;			// Length the photon had to march before exiting (in canonical [-1,+1] units, multiply by 0.5*CubeSize to get length in meters)
			public UInt32		ScatteringStepsCount;	// Amount of scattering events before exiting
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_RenderPhotonVector
		{
 			public UInt32		VectorsPerFace;
 			public float		VectorMultiplier;
 			public float		ClipAboveValue;
		}

		#endregion

		#region FIELDS

		private RegistryKey					m_AppKey;
		private string						m_ApplicationPath;

		private Device						m_Device = new Device();

		private ComputeShader				m_CS = null;
		private Shader						m_PS = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Render>	m_CB_Render = null;

		// Photon Shooter
		private ComputeShader							m_CS_PhotonShooter = null;
		private ConstantBuffer<CB_PhotonShooterInput>	m_CB_PhotonShooterInput = null;
		private StructuredBuffer<float>					m_SB_PhaseQuantile = null;
		private StructuredBuffer<SB_PhotonOut>			m_SB_PhotonOut = null;
		private StructuredBuffer<float4>				m_SB_Random = null;

		// Photons Splatter
		private Shader							m_PS_PhotonSplatter	= null;
		private Shader							m_PS_PhotonSplatter_Intensity	= null;
		private ConstantBuffer<CB_SplatPhoton>	m_CB_SplatPhoton;
		private Texture2D						m_Tex_Photons = null;
		private Primitive						m_Prim_Point = null;

		// Photons Renderer
		private Shader						m_PS_RenderCube = null;
		private Primitive					m_Prim_Cube = null;

		// Vectors Renderer
		private Shader									m_PS_RenderPhotonVectors;
		private ConstantBuffer<CB_RenderPhotonVector>	m_CB_RenderPhotonVector;
		private Primitive								m_Prim_Line;

//		private Texture3D					m_Noise3D = null;

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

			Reg( m_CS = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/Test/TestCompute.hlsl" ) ), "CS", null ) );
			Reg( m_PS = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/DisplayDistanceField.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null ) );
			Reg( m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 0 ) );
			Reg( m_CB_Render = new ConstantBuffer<CB_Render>( m_Device, 8 ) );

			Build3DNoise();

			//////////////////////////////////////////////////////////////////////////
			// Photon Shooter
			Reg( m_CS_PhotonShooter = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/CanonicalCubeRenderer/PhotonShooter.hlsl" ) ), "CS", null ) );
			Reg( m_CB_PhotonShooterInput = new ConstantBuffer<CB_PhotonShooterInput>( m_Device, 8 ) );
			Reg( m_SB_PhotonOut = new StructuredBuffer<SB_PhotonOut>( m_Device, PHOTONS_COUNT, true ) );
			BuildPhaseQuantileBuffer( new System.IO.FileInfo( @"Mie65536x2.float" ) );
			BuildRandomBuffer();

			//////////////////////////////////////////////////////////////////////////
			// Photons Splatter
			Reg( m_PS_PhotonSplatter = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/CanonicalCubeRenderer/SplatPhoton.hlsl" ) ), VERTEX_FORMAT.P3, "VS", "GS", "PS", null ) );
			Reg( m_PS_PhotonSplatter_Intensity = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/CanonicalCubeRenderer/SplatPhoton.hlsl" ) ), VERTEX_FORMAT.P3, "VS", "GS", "PS_Intensity", null ) );

			Reg( m_CB_SplatPhoton = new ConstantBuffer<CB_SplatPhoton>( m_Device, 8 ) );
			Reg( m_Tex_Photons = new Texture2D( m_Device, 512, 512, 6*3, 1, PIXEL_FORMAT.RGBA16_FLOAT, false, true, null ) );

			// Build a single point that will be instanced as many times as there are photons
			{
				ByteBuffer	Point = new ByteBuffer( 3*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)) );
				Reg( m_Prim_Point = new Primitive( m_Device, 1, Point, null, Primitive.TOPOLOGY.POINT_LIST, VERTEX_FORMAT.P3 ) );
			}

			//////////////////////////////////////////////////////////////////////////
			// Photons Renderer
			Reg( m_PS_RenderCube = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/CanonicalCubeRenderer/DisplayPhotonCube.hlsl" ) ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null ) );
			BuildCube();
			Reg( m_PS_RenderWorldCube = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/DisplayWorldCube.hlsl" ) ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null ) );


			//////////////////////////////////////////////////////////////////////////
			// Photon Vectors Renderer
			Reg( m_PS_RenderPhotonVectors = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/CanonicalCubeRenderer/DisplayPhotonVector.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null ) );
			Reg( m_CB_RenderPhotonVector = new ConstantBuffer<CB_RenderPhotonVector>( m_Device, 8 ) );
			{
				ByteBuffer	Line = VertexP3.FromArray( new VertexP3[] { new VertexP3() { P = new float3( 0, 0, 0 ) }, new VertexP3() { P = new float3( 1, 0, 0 ) } } );
				Reg( m_Prim_Line = new Primitive( m_Device, 2, Line, null, Primitive.TOPOLOGY.LINE_LIST, VERTEX_FORMAT.P3 ) );
			}

			// Create the camera manipulator
			m_CB_Camera.m.Camera2World = float4x4.Identity;
			UpdateCameraProjection( 60.0f * (float) Math.PI / 180.0f, (float) viewportPanel.Width / viewportPanel.Height, 0.1f, 100.0f );

			m_Manipulator.Attach( viewportPanel );
			m_Manipulator.CameraTransformChanged += new CameraManipulator.UpdateCameraTransformEventHandler( Manipulator_CameraTransformChanged );
			m_Manipulator.InitializeCamera( new float3( 0.0f, 0.0f, 4.0f ), new float3( 0, 0, 0 ), new float3( 0, 1, 0 ) );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			foreach ( IDisposable D in m_Disposables )
				D.Dispose();

			m_Device.Exit();
//			m_Device = null;

			base.OnClosing( e );
		}

		private void	Build3DNoise()
		{
//			Reg( m_Noise3D = new Texture3D( m_Device, ) );

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

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct Vertex
		{
			public float3	P;
			public float3	UVW;
		}
		private void	BuildCube()
		{
			Vertex[]	Vertices = new Vertex[4*6];
			Vertices[4*0+0] = new Vertex() { P = new float3( +1, +1, +1 ), UVW = new float3( 0, 0, 0 ) };	// +X
			Vertices[4*0+1] = new Vertex() { P = new float3( +1, -1, +1 ), UVW = new float3( 0, 1, 0 ) };
			Vertices[4*0+2] = new Vertex() { P = new float3( +1, -1, -1 ), UVW = new float3( 1, 1, 0 ) };
			Vertices[4*0+3] = new Vertex() { P = new float3( +1, +1, -1 ), UVW = new float3( 1, 0, 0 ) };

			Vertices[4*1+0] = new Vertex() { P = new float3( -1, +1, -1 ), UVW = new float3( 0, 0, 1 ) };	// -X
			Vertices[4*1+1] = new Vertex() { P = new float3( -1, -1, -1 ), UVW = new float3( 0, 1, 1 ) };
			Vertices[4*1+2] = new Vertex() { P = new float3( -1, -1, +1 ), UVW = new float3( 1, 1, 1 ) };
			Vertices[4*1+3] = new Vertex() { P = new float3( -1, +1, +1 ), UVW = new float3( 1, 0, 1 ) };

			Vertices[4*2+0] = new Vertex() { P = new float3( -1, +1, -1 ), UVW = new float3( 0, 0, 2 ) };	// +Y
			Vertices[4*2+1] = new Vertex() { P = new float3( -1, +1, +1 ), UVW = new float3( 0, 1, 2 ) };
			Vertices[4*2+2] = new Vertex() { P = new float3( +1, +1, +1 ), UVW = new float3( 1, 1, 2 ) };
			Vertices[4*2+3] = new Vertex() { P = new float3( +1, +1, -1 ), UVW = new float3( 1, 0, 2 ) };

			Vertices[4*3+0] = new Vertex() { P = new float3( -1, -1, +1 ), UVW = new float3( 0, 0, 3 ) };	// -Y
			Vertices[4*3+1] = new Vertex() { P = new float3( -1, -1, -1 ), UVW = new float3( 0, 1, 3 ) };
			Vertices[4*3+2] = new Vertex() { P = new float3( +1, -1, -1 ), UVW = new float3( 1, 1, 3 ) };
			Vertices[4*3+3] = new Vertex() { P = new float3( +1, -1, +1 ), UVW = new float3( 1, 0, 3 ) };

			Vertices[4*4+0] = new Vertex() { P = new float3( -1, +1, +1 ), UVW = new float3( 0, 0, 4 ) };	// +Z
			Vertices[4*4+1] = new Vertex() { P = new float3( -1, -1, +1 ), UVW = new float3( 0, 1, 4 ) };
			Vertices[4*4+2] = new Vertex() { P = new float3( +1, -1, +1 ), UVW = new float3( 1, 1, 4 ) };
			Vertices[4*4+3] = new Vertex() { P = new float3( +1, +1, +1 ), UVW = new float3( 1, 0, 4 ) };

			Vertices[4*5+0] = new Vertex() { P = new float3( +1, +1, -1 ), UVW = new float3( 0, 0, 5 ) };	// -Z
			Vertices[4*5+1] = new Vertex() { P = new float3( +1, -1, -1 ), UVW = new float3( 0, 1, 5 ) };
			Vertices[4*5+2] = new Vertex() { P = new float3( -1, -1, -1 ), UVW = new float3( 1, 1, 5 ) };
			Vertices[4*5+3] = new Vertex() { P = new float3( -1, +1, -1 ), UVW = new float3( 1, 0, 5 ) };

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

			ByteBuffer	VerticesBuffer = new ByteBuffer( 4*6* (6*4) );
			using ( System.IO.BinaryWriter W = VerticesBuffer.OpenStreamWrite() )
				for ( int VertexIndex=0; VertexIndex < Vertices.Length; VertexIndex++ )
				{
					W.Write( Vertices[VertexIndex].P.x );
					W.Write( Vertices[VertexIndex].P.y );
					W.Write( Vertices[VertexIndex].P.z );
					W.Write( Vertices[VertexIndex].UVW.x );
					W.Write( Vertices[VertexIndex].UVW.y );
					W.Write( Vertices[VertexIndex].UVW.z );
				}

			Reg( m_Prim_Cube = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3 ) );
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
			// Setup default render target as UAV & render using the compute shader
			m_CB_Render.m.TargetDimensions = new float4( viewportPanel.Width, viewportPanel.Height, 1.0f / viewportPanel.Width, 1.0f / viewportPanel.Height );
			m_CB_Render.m.Debug = new float4( floatTrackbarControlDebug0.Value, floatTrackbarControlDebug1.Value, floatTrackbarControlDebug2.Value, floatTrackbarControlDebug3.Value );
			m_CB_Render.m.FluxMultiplier = floatTrackbarControlFluxMultiplier.Value;

			uint	Modifier = (uint) (radioButtonPos.Checked ? 0 : radioButtonNeg.Checked ? 1 : 2) << 4;
			if ( radioButtonExitPosition.Checked )
				m_CB_Render.m.SplatType = 0 | Modifier;
			if ( radioButtonExitDirection.Checked )
				m_CB_Render.m.SplatType = 1 | Modifier;
			if ( radioButtonScatteringEventIndex.Checked )
				m_CB_Render.m.SplatType = 2;
			if ( radioButtonMarchedLength.Checked )
				m_CB_Render.m.SplatType = 3;
			if ( radioButtonAccumFlux.Checked )
				m_CB_Render.m.SplatType = 4;

			m_CB_Render.UpdateData();

 			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

// 			// Render a fullscreen quad
// 			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
// 			m_Device.RenderFullscreenQuad( m_PS );


			// Render the photons cube
 			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );
			m_Device.Clear( m_Device.DefaultTarget, 0.5f * new float4( Color.SkyBlue, 1 ) );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1, 0, true, false );

			m_Tex_Photons.SetPS( 0 );

			m_PS_RenderCube.Use();
			m_Prim_Cube.Render( m_PS_RenderCube );

			// Render the photon vectors
			if ( checkBoxRenderVectors.Checked )
			{
				const int		PHOTON_VECTORS_COUNT_PER_FACE = 10000;
				m_CB_RenderPhotonVector.m.VectorsPerFace = PHOTON_VECTORS_COUNT_PER_FACE;
				m_CB_RenderPhotonVector.m.VectorMultiplier = floatTrackbarControlVectorSize.Value;
				m_CB_RenderPhotonVector.m.ClipAboveValue = checkBoxClipAboveValue.Checked ? 0.01f * floatTrackbarControlClipAbove.Value : 1e6f;
				m_CB_RenderPhotonVector.UpdateData();
				m_PS_RenderPhotonVectors.Use();
				m_Prim_Line.RenderInstanced( m_PS_RenderPhotonVectors, 6*PHOTON_VECTORS_COUNT_PER_FACE );
			}


			// Render the world cube
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
//			viewportPanel.Refresh();
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

		private void buttonShootPhotons_Click( object sender, EventArgs e )
		{
			//////////////////////////////////////////////////////////////////////////
			// 1] Shoot the photons and store the result into the structured buffer
			m_CS_PhotonShooter.Use();

			m_SB_Random.SetInput( 0 );
			m_SB_PhaseQuantile.SetInput( 1 );
			m_SB_PhotonOut.SetOutput( 0 );

			m_CB_PhotonShooterInput.m.MaxScattering = 100;
			m_CB_PhotonShooterInput.m.InitialPosition = new float2( floatTrackbarControlPositionX.Value, floatTrackbarControlPositionZ.Value );	// Center of the cube side
			m_CB_PhotonShooterInput.m.InitialIncidence = new float2( floatTrackbarControlOrientationPhi.Value * (float) Math.PI / 180.0f, floatTrackbarControlOrientationTheta.Value * (float) Math.PI / 180.0f );			// Vertical incidence
			m_CB_PhotonShooterInput.m.CubeSize = floatTrackbarControlCubeSize.Value;	// Try a 100m thick cube
//			m_CB_PhotonShooterInput.m.SigmaScattering = 0.5f;
			m_CB_PhotonShooterInput.m.SigmaScattering = 0.04523893421169302263386206471922f;	// re=6µm Gamma=2 N0=4e8   Sigma_t = N0 * PI * re²
			m_CB_PhotonShooterInput.m.FullSurface = (uint)(checkBoxFullSurface.Checked ? 1 : 0);
													//	mean free path = 22.1048m

			int	BatchesCount = PHOTONS_COUNT / PHOTON_BATCH_SIZE;
			for ( int BatchIndex=0; BatchIndex < BatchesCount; BatchIndex++ )
			{
				m_CB_PhotonShooterInput.m.BatchIndex = (uint) BatchIndex;
				m_CB_PhotonShooterInput.UpdateData();

				m_CS_PhotonShooter.Dispatch( 1, 1, 1 );

				m_Device.Present( true );
				progressBar1.Value = progressBar1.Maximum * (1+BatchIndex) / BatchesCount;
				Application.DoEvents();
			}

			m_SB_PhotonOut.RemoveFromLastAssignedSlots();

			//////////////////////////////////////////////////////////////////////////
			// Splat photons
			m_Tex_Photons.RemoveFromLastAssignedSlots();
			m_Device.Clear( m_Tex_Photons, new float4( 0, 0, 0, 0 ) );

			m_SB_PhotonOut.SetInput( 0 );

			// Splat data
			m_CB_SplatPhoton.m.SplatSize = 2.0f * (2.0f / m_Tex_Photons.Width);
			m_CB_SplatPhoton.UpdateData();

			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			View2D[]	Views = new View2D[] {
				m_Tex_Photons.GetView( 0, 0, 6*0, 6 ),
				m_Tex_Photons.GetView( 0, 0, 6*1, 6 ),
			};
			m_Device.SetRenderTargets( Views, null );

			m_PS_PhotonSplatter.Use();
			m_Prim_Point.RenderInstanced( m_PS_PhotonSplatter, PHOTONS_COUNT );

			// Splat additive intensity
			m_CB_SplatPhoton.m.SplatSize = 8.0f * (2.0f / m_Tex_Photons.Width);
 			m_CB_SplatPhoton.m.SplatIntensity = 1000.0f / PHOTONS_COUNT;
			m_CB_SplatPhoton.UpdateData();

			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ADDITIVE );

			Views = new View2D[] { m_Tex_Photons.GetView( 0, 0, 6*2, 6 ) };
			m_Device.SetRenderTargets( Views, null );

			m_PS_PhotonSplatter_Intensity.Use();
			m_Prim_Point.RenderInstanced( m_PS_PhotonSplatter_Intensity, PHOTONS_COUNT );

			m_Tex_Photons.RemoveFromLastAssignedSlots();

			Render();
		}

		private void floatTrackbarControlFluxMultiplier_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Render();
		}

		private void radioButtonExitPosition_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked )
				Render();
		}

		private void checkBoxRenderVectors_CheckedChanged( object sender, EventArgs e )
		{
			Render();
		}

		private void floatTrackbarControlVectorSize_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Render();
		}

		private void checkBoxClipAboveValue_CheckedChanged( object sender, EventArgs e )
		{
			Render();
		}

		private void floatTrackbarControlClipAbove_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Render();
		}

		private void checkBoxFullSurface_CheckedChanged( object sender, EventArgs e )
		{
			floatTrackbarControlPositionX.Enabled = !checkBoxFullSurface.Checked;
			floatTrackbarControlPositionZ.Enabled = !checkBoxFullSurface.Checked;
		}

		#endregion
	}
}
