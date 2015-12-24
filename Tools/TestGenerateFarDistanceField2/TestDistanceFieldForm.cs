//#define BISOU	// Define this to avoid compiling the heavy shaders, even in DEBUG mode

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

namespace TestGenerateFarDistanceField
{
	public partial class TestDistanceFieldForm : Form
	{
		private Device				m_Device = new Device();

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;

			public float4x4		_OldCamera2NewCamera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Object {
			public float4x4		_Local2World;
			public float4x4		_World2Local;
			public float3		_DiffuseAlbedo;
			public float		_Gloss;
			public float3		_SpecularTint;
			public float		_Metal;
// 			public UInt32		_UseTexture;
// 			public UInt32		_FalseColors;
// 			public float		_FalseColorsMaxRange;
		}

		private ConstantBuffer<CB_Main>		m_CB_Main = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Object>	m_CB_Object = null;

		private Shader				m_Shader_RenderScene = null;
		private ComputeShader		m_Shader_ClearAccumulator = null;
		private ComputeShader		m_Shader_SplatDepthStencil = null;
		private ComputeShader		m_Shader_Reproject = null;
		private ComputeShader		m_Shader_FinalizeSplat = null;
		private ComputeShader[]		m_Shader_BuildDistanceField = new ComputeShader[3];
		private Shader				m_Shader_PostProcess = null;

		private Texture2D			m_Tex_TempTarget = null;
		private Texture3D			m_Tex_TempDepthAccumulatorRG = null;		// Accumulates positions splat from depth buffer
		private Texture3D			m_Tex_TempDepthAccumulatorBA = null;		// Accumulates positions splat from depth buffer
		private Texture3D[]			m_Tex_SplatDepthStencil = new Texture3D[2];	// Final depth buffer average splat
		private Texture3D[]			m_Tex_DistanceField = new Texture3D[2];		// Double-buffered distance field

		private Primitive			m_Prim_Quad = null;
		private Primitive			m_Prim_Rectangle = null;
		private Primitive			m_Prim_Sphere = null;
		private Primitive			m_Prim_Cube = null;

		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		private DateTime			m_TimeSTart = DateTime.Now;

		void	TAS( int[] _array, int a, int b ) {
			if ( _array[a] > _array[b] ) {
				int temp = _array[a];
				_array[a] = _array[b];
				_array[b] = temp;
			}
		}
		public TestDistanceFieldForm()
		{
			InitializeComponent();

//			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try {
				m_Device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			try {
				m_Shader_RenderScene = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ) ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"RenderScene\" failed to compile!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderScene = null;
			}

			try {
				m_Shader_PostProcess = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"PostProcess\" failed to compile!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_PostProcess = null;
			}

			#if DEBUG && !BISOU
				try {
					m_Shader_ClearAccumulator = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/SplatDepthStencil_Accumulate.hlsl" ) ), "CS_Clear", null );
					m_Shader_SplatDepthStencil = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/SplatDepthStencil_Accumulate.hlsl" ) ), "CS_Accumulate", null );
					m_Shader_Reproject = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/SplatDepthStencil_Reproject.hlsl" ) ), "CS", null );
					m_Shader_FinalizeSplat = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/SplatDepthStencil_Finalize.hlsl" ) ), "CS", null );
				} catch ( Exception _e ) {
					MessageBox.Show( "Shader \"SplatDepthStencil\" failed to compile!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
					m_Shader_ClearAccumulator = null;
					m_Shader_SplatDepthStencil = null;
					m_Shader_FinalizeSplat = null;
				}

				try {
					m_Shader_BuildDistanceField[0] = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/BuildDistanceField_X.hlsl" ) ), "CS", null );
					m_Shader_BuildDistanceField[1] = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/BuildDistanceField_Y.hlsl" ) ), "CS", null );
					m_Shader_BuildDistanceField[2] = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/BuildDistanceField_Z.hlsl" ) ), "CS", null );
				} catch ( Exception _e ) {
					MessageBox.Show( "Shader \"BuildDistanceField\" failed to compile!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
					m_Shader_BuildDistanceField[0] = null;
					m_Shader_BuildDistanceField[1] = null;
					m_Shader_BuildDistanceField[2] = null;
				}
			#else
				try {
					m_Shader_ClearAccumulator = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "Shaders/SplatDepthStencil_Accumulate.hlsl" ), "CS_Clear" );
					m_Shader_SplatDepthStencil = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "Shaders/SplatDepthStencil_Accumulate.hlsl" ), "CS_Accumulate" );
					m_Shader_Reproject = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "Shaders/SplatDepthStencil_Reproject.hlsl" ), "CS" );
					m_Shader_FinalizeSplat = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "Shaders/SplatDepthStencil_Finalize.hlsl" ), "CS" );
				} catch ( Exception _e ) {
					MessageBox.Show( "Shader \"SplatDepthStencil\" failed to compile!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
					m_Shader_ClearAccumulator = null;
					m_Shader_SplatDepthStencil = null;
					m_Shader_Reproject = null;
					m_Shader_FinalizeSplat = null;
				}

				try {
					m_Shader_BuildDistanceField[0] = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BuildDistanceField_X.fxbin" ), "CS" );
					m_Shader_BuildDistanceField[1] = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BuildDistanceField_Y.fxbin" ), "CS" );
					m_Shader_BuildDistanceField[2] = ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BuildDistanceField_Z.fxbin" ), "CS" );
				} catch ( Exception _e ) {
					MessageBox.Show( "Shader \"BuildDistanceField\" failed to compile!\n\n" + _e.Message, "Distance Field Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
					m_Shader_BuildDistanceField[0] = null;
					m_Shader_BuildDistanceField[1] = null;
					m_Shader_BuildDistanceField[2] = null;
				}
			#endif

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Object = new ConstantBuffer<CB_Object>( m_Device, 2 );

			BuildPrimitives();

			// Allocate texture
			m_Tex_TempTarget = new Texture2D( m_Device, panelOutput.Width, panelOutput.Height, 1, 1, PIXEL_FORMAT.RGBA8_UNORM_sRGB, false, false, null );

			// Allocate several 3D textures for depth-stencil reduction
			m_Tex_TempDepthAccumulatorRG = new Texture3D( m_Device, 64, 64, 64, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_TempDepthAccumulatorBA = new Texture3D( m_Device, 64, 64, 64, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_SplatDepthStencil[0] = new Texture3D( m_Device, 64, 64, 64, 1, PIXEL_FORMAT.RGBA8_UNORM, false, true, null );
			m_Tex_SplatDepthStencil[1] = new Texture3D( m_Device, 64, 64, 64, 1, PIXEL_FORMAT.RGBA8_UNORM, false, true, null );
			m_Tex_DistanceField[0] = new Texture3D( m_Device, 64, 64, 64, 6, PIXEL_FORMAT.R16_FLOAT, false, true, null );
			m_Tex_DistanceField[1] = new Texture3D( m_Device, 64, 64, 64, 6, PIXEL_FORMAT.R16_FLOAT, false, true, null );

			m_Device.Clear( m_Tex_SplatDepthStencil[0], float4.Zero );
			m_Device.Clear( m_Tex_SplatDepthStencil[1], float4.Zero );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
		}

		protected override void OnClosed( EventArgs e ) {
			base.OnClosed( e );

			m_Tex_DistanceField[1].Dispose();
			m_Tex_DistanceField[0].Dispose();
			m_Tex_SplatDepthStencil[0].Dispose();
			m_Tex_SplatDepthStencil[1].Dispose();
			m_Tex_TempDepthAccumulatorBA.Dispose();
			m_Tex_TempDepthAccumulatorRG.Dispose();
			m_Tex_TempTarget.Dispose();

			m_Prim_Cube.Dispose();
			m_Prim_Sphere.Dispose();
			m_Prim_Rectangle.Dispose();
			m_Prim_Quad.Dispose();

			m_CB_Object.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_Shader_BuildDistanceField[2].Dispose();
			m_Shader_BuildDistanceField[1].Dispose();
			m_Shader_BuildDistanceField[0].Dispose();
			m_Shader_PostProcess.Dispose();
			m_Shader_FinalizeSplat.Dispose();
			m_Shader_SplatDepthStencil.Dispose();
			m_Shader_ClearAccumulator.Dispose();
			m_Shader_RenderScene.Dispose();

			m_Device.Dispose();
			m_Device = null;
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null )
				return;

			m_CB_Main.m.iGlobalTime = (float) (DateTime.Now - m_TimeSTart).TotalSeconds;
			m_CB_Main.m.iResolution.Set( panelOutput.Width, panelOutput.Height, 1 );
			m_CB_Main.UpdateData();


			// Force call each frame
			Camera_CameraTransformChanged( null, EventArgs.Empty );


			//////////////////////////////////////////////////////////////////////////
			// Render scene
			if ( m_Shader_RenderScene != null && m_Shader_RenderScene.Use() ) {

				m_Device.SetRenderTarget( m_Tex_TempTarget, m_Device.DefaultDepthStencil );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

				m_Device.Clear( m_Tex_TempTarget, float4.Zero );
				m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

				// Render a floor plane
				m_CB_Object.m._Local2World.MakeLookAt( float3.Zero, float3.UnitY, float3.UnitX );
				m_CB_Object.m._Local2World.Scale( new float3( 16.0f, 16.0f, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
//				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;// 0.8f;
				m_CB_Object.m._Gloss = 0.5f;
				m_CB_Object.m._Metal = 0.0f;
				m_CB_Object.UpdateData();

				m_Prim_Rectangle.Render( m_Shader_RenderScene );

				// Render the sphere
				m_CB_Object.m._Local2World.MakeLookAt( new float3( 0, 0.5f, 1.0f ), new float3( 0, 0.5f, 2 ), float3.UnitY );
//				m_CB_Object.m._Local2World.MakeLookAt( new float3( 0, 0.3f, 1.0f ), new float3( 0, 0.3f, 2 ), float3.UnitY );
				m_CB_Object.m._Local2World.Scale( new float3( 0.5f, 0.5f, 0.5f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 0.5f, 0.2f );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
// 				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;//0.2f;
 				m_CB_Object.m._Gloss = 0.2f;
 				m_CB_Object.m._Metal = 0.2f;
				m_CB_Object.UpdateData();

				m_Prim_Sphere.Render( m_Shader_RenderScene );

				// Render the cube
				m_CB_Object.m._Local2World.MakeLookAt( new float3( -2.0f, 1.0f, 0.0f ), new float3( -1.0f, 1.0f, 1 ), float3.UnitY );
				m_CB_Object.m._Local2World.Scale( new float3( 1.0f, 1.0f, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = new float3( 0.5f, 0.8f, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.92f );
// 				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;//0.9f;
 				m_CB_Object.m._Gloss = 0.9f;
 				m_CB_Object.m._Metal = 0.0f;
				m_CB_Object.UpdateData();

				m_Prim_Cube.Render( m_Shader_RenderScene );
			}


			//////////////////////////////////////////////////////////////////////////
			// Splat depth-stencil into 3D map
			if ( m_Shader_ClearAccumulator != null && m_Shader_ClearAccumulator.Use() ) {
				// First, clear accumulators
				m_Tex_TempDepthAccumulatorRG.SetCSUAV( 0 );
				m_Tex_TempDepthAccumulatorBA.SetCSUAV( 1 );

				int	groupsCount = m_Tex_TempDepthAccumulatorRG.Width / 4;
				m_Shader_ClearAccumulator.Dispatch( groupsCount, groupsCount, groupsCount );
			}

			if ( m_Shader_SplatDepthStencil != null && m_Shader_SplatDepthStencil.Use() ) {
				// Splat and accumulate
				m_Device.RemoveRenderTargets();	// So we can use the depth stencil as input
				m_Device.DefaultDepthStencil.SetCS( 0 );

				int	groupsCountX = (m_Tex_TempTarget.Width + 7) >> 3;
				int	groupsCountY = (m_Tex_TempTarget.Height + 7) >> 3;
				m_Shader_SplatDepthStencil.Dispatch( groupsCountX, groupsCountY, 1 );

				m_Device.DefaultDepthStencil.RemoveFromLastAssignedSlots();		// So we can use it again next time!
			}

// 			if ( m_Shader_Reproject != null && m_Shader_Reproject.Use() ) {
// 				// Reproject previous frame's result
// 				m_Tex_SplatDepthStencil[0].SetCS( 0 );
// 
// 				int	groupsCount = m_Tex_SplatDepthStencil[0].Width / 4;
// 				m_Shader_Reproject.Dispatch( groupsCount, groupsCount, groupsCount );
// 
// 			}
			m_Tex_TempDepthAccumulatorRG.RemoveFromLastAssignedSlotUAV();	// So we can use it as input
			m_Tex_TempDepthAccumulatorBA.RemoveFromLastAssignedSlotUAV();	// So we can use it as input

			if ( m_Shader_FinalizeSplat != null && m_Shader_FinalizeSplat.Use() ) {
				// Finalize splat
				m_Tex_TempDepthAccumulatorRG.SetCS( 0 );
				m_Tex_TempDepthAccumulatorBA.SetCS( 1 );
				m_Tex_SplatDepthStencil[1].SetCSUAV( 0 );

				int	groupsCount = m_Tex_SplatDepthStencil[1].Width / 4;
				m_Shader_FinalizeSplat.Dispatch( groupsCount, groupsCount, groupsCount );

				m_Tex_TempDepthAccumulatorRG.RemoveFromLastAssignedSlots();
				m_Tex_TempDepthAccumulatorBA.RemoveFromLastAssignedSlots();
				m_Tex_SplatDepthStencil[1].RemoveFromLastAssignedSlotUAV();		// So we can use it as input
			}

			// Swap 3D textures
			Texture3D	Temp = m_Tex_SplatDepthStencil[0];
			m_Tex_SplatDepthStencil[0] = m_Tex_SplatDepthStencil[1];
			m_Tex_SplatDepthStencil[1] = Temp;


			//////////////////////////////////////////////////////////////////////////
			// Build the distance field
			if ( m_Shader_BuildDistanceField[0] != null && m_Shader_BuildDistanceField[0].Use() ) {
				// =========== Build along X ===========

				m_Tex_SplatDepthStencil[0].SetCS( 0 );	// Source texture with positions within voxels
				m_Tex_DistanceField[0].SetCSUAV( 0 );	// Target distance field

				int	groupsCountX = m_Tex_DistanceField[0].Width;		// Horizontal sweep
				int	groupsCountY = m_Tex_DistanceField[0].Height >> 3;
				int	groupsCountZ = m_Tex_DistanceField[0].Depth >> 3;

				m_Shader_BuildDistanceField[0].Dispatch( groupsCountX, groupsCountY, groupsCountZ );

				m_Tex_SplatDepthStencil[0].RemoveFromLastAssignedSlots();
				m_Tex_DistanceField[0].RemoveFromLastAssignedSlotUAV();		// So we can use it as input
			}

			if ( m_Shader_BuildDistanceField[1] != null && m_Shader_BuildDistanceField[1].Use() ) {
				// =========== Build along Y ===========

				m_Tex_DistanceField[0].SetCS( 0 );
				m_Tex_DistanceField[1].SetCSUAV( 0 );

				int	groupsCountX = m_Tex_DistanceField[1].Width >> 3;
				int	groupsCountY = m_Tex_DistanceField[1].Height;		// Vertical sweep
				int	groupsCountZ = m_Tex_DistanceField[1].Depth >> 3;

				m_Shader_BuildDistanceField[1].Dispatch( groupsCountX, groupsCountY, groupsCountZ );

				m_Tex_DistanceField[0].RemoveFromLastAssignedSlots();
				m_Tex_DistanceField[1].RemoveFromLastAssignedSlotUAV();		// So we can use it as input
			}

			if ( m_Shader_BuildDistanceField[2] != null && m_Shader_BuildDistanceField[2].Use() ) {
				// =========== Build along Z ===========

				m_Tex_DistanceField[1].SetCS( 0 );
				m_Tex_DistanceField[0].SetCSUAV( 0 );

				int	groupsCountX = m_Tex_DistanceField[1].Width >> 3;
				int	groupsCountY = m_Tex_DistanceField[1].Height >> 3;
				int	groupsCountZ = m_Tex_DistanceField[1].Depth;		// Depth sweep

				m_Shader_BuildDistanceField[2].Dispatch( groupsCountX, groupsCountY, groupsCountZ );

				m_Tex_DistanceField[1].RemoveFromLastAssignedSlots();
				m_Tex_DistanceField[0].RemoveFromLastAssignedSlotUAV();		// So we can use it as input
			}


			//////////////////////////////////////////////////////////////////////////
			// Display rendered scene + distance field stuff
			if ( m_Shader_PostProcess != null && m_Shader_PostProcess.Use() ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				m_Tex_TempTarget.SetPS( 0 );
				m_Tex_SplatDepthStencil[0].SetPS( 1 );
				m_Tex_DistanceField[0].SetPS( 2 );
				m_Tex_DistanceField[1].SetPS( 3 );

				m_Prim_Quad.Render( m_Shader_PostProcess );

				m_Tex_TempTarget.RemoveFromLastAssignedSlots();
				m_Tex_SplatDepthStencil[0].RemoveFromLastAssignedSlots();
				m_Tex_DistanceField[0].RemoveFromLastAssignedSlots();
				m_Tex_DistanceField[1].RemoveFromLastAssignedSlots();
			}

			m_Device.Present( false );
		}

		#region Primitives

		private void	BuildPrimitives()
		{
			{
				VertexPt4[]	Vertices = new VertexPt4[4];
				Vertices[0] = new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) };	// Top-Left
				Vertices[1] = new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) };	// Top-Right
				Vertices[3] = new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

				m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
			}

			{
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[4];
				Vertices[0] = new VertexP3N3G3B3T2() { P = new float3( -1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 0 ) };	// Top-Left
				Vertices[1] = new VertexP3N3G3B3T2() { P = new float3( -1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexP3N3G3B3T2() { P = new float3( +1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 0 ) };	// Top-Right
				Vertices[3] = new VertexP3N3G3B3T2() { P = new float3( +1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Rectangle = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the sphere
				const int	W = 41;
				const int	H = 22;
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[W*H];
				for ( int Y=0; Y < H; Y++ ) {
					double	Theta = Math.PI * Y / (H-1);
					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );
					for ( int X=0; X < W; X++ ) {
						double	Phi = 2.0 * Math.PI * X / (W-1);
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	N = new float3( SinTheta * SinPhi, CosTheta, SinTheta * CosPhi );
						float3	T = new float3( CosPhi, 0.0f, -SinPhi );
						float3	B = N.Cross( T );

						Vertices[W*Y+X] = new VertexP3N3G3B3T2() {
							P = N,
							N = N,
							T = T,
							B = B,
							UV = new float2( 2.0f * X / W, 1.0f * Y / H )
						};
					}
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				uint[]		Indices = new uint[(H-1) * (2*W+2)-2];
				int			IndexCount = 0;
				for ( int Y=0; Y < H-1; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						Indices[IndexCount++] = (uint) ((Y+0) * W + X);
						Indices[IndexCount++] = (uint) ((Y+1) * W + X);
					}
					if ( Y < H-2 ) {
						Indices[IndexCount++] = (uint) ((Y+1) * W - 1);
						Indices[IndexCount++] = (uint) ((Y+1) * W + 0);
					}
				}

				m_Prim_Sphere = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
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

				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[6*4];
				uint[]		Indices = new uint[2*6*3];

				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ ) {
					float3	N = Normals[FaceIndex];
					float3	T = Tangents[FaceIndex];
					float3	B = N.Cross( T );

					Vertices[4*FaceIndex+0] = new VertexP3N3G3B3T2() {
						P = N - T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 0 )
					};
					Vertices[4*FaceIndex+1] = new VertexP3N3G3B3T2() {
						P = N - T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 1 )
					};
					Vertices[4*FaceIndex+2] = new VertexP3N3G3B3T2() {
						P = N + T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 1 )
					};
					Vertices[4*FaceIndex+3] = new VertexP3N3G3B3T2() {
						P = N + T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 0 )
					};

					Indices[2*3*FaceIndex+0] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+1] = (uint) (4*FaceIndex+1);
					Indices[2*3*FaceIndex+2] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+3] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+4] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+5] = (uint) (4*FaceIndex+3);
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Cube = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
			}
		}

		#endregion

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {

			float4x4	OldCamera2World = m_CB_Camera.m._Camera2World;

			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			// Allows transformation from old to new camera space (for reprojection)
			m_CB_Camera.m._OldCamera2NewCamera = OldCamera2World * m_CB_Camera.m._World2Camera;

			m_CB_Camera.UpdateData();
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_Device.ReloadModifiedShaders();
		}
	}
}
