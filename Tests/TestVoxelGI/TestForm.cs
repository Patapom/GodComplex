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
using UIUtility;

namespace TestVoxelGI
{
	public partial class TestForm : Form {

		const float	CAMERA_FOV = (float) (90.0 * Math.PI / 180.0);

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_ComputeIndirectLighting {
			public uint			_mipLevel;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Global {
			public uint			_resolutionX;
			public uint			_resolutionY;
			public float		_time;
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
		private struct CB_RenderScene {
			public float		_lightSize;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderVoxels {
			public float		_voxelSizeX;
			public float		_voxelSizeY;
			public float		_voxelSizeZ;
			public float		_PAD0;
			public uint			_voxelPOTX;
			public uint			_voxelPOTY;
			public uint			_voxelPOTZ;
			public uint			_mipLevel;
			public uint			_voxelMasksX;
			public uint			_voxelMasksY;
			public uint			_voxelMasksZ;
			public uint			_flags;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Filtering {
			public float		_stride;
			public float		_sigma_Color;
			public float		_sigma_Normal;
			public float		_sigma_Position;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_PostProcess {
			public uint			_flags;
			public uint			_filterLevel;
		}

		#endregion

		Device						m_device = new Device();

		ConstantBuffer< CB_ComputeIndirectLighting >	m_CB_computeIndirect;
		ConstantBuffer< CB_Global >			m_CB_global;
		ConstantBuffer< CB_Camera >			m_CB_camera = null;
		ConstantBuffer< CB_RenderScene >	m_CB_renderScene = null;
		ConstantBuffer< CB_RenderVoxels >	m_CB_renderVoxels = null;
		ConstantBuffer< CB_Filtering >		m_CB_filtering = null;
		ConstantBuffer< CB_PostProcess >	m_CB_postProcess = null;

		ComputeShader				m_shader_voxelizeScene;
		ComputeShader				m_shader_buildVoxelMips;
		ComputeShader				m_shader_buildSingleVoxelMips;
		ComputeShader				m_shader_computeIndirectLighting;
		ComputeShader				m_shader_accumulateVoxelLighting;
		Shader						m_shader_renderGBuffer;
		Shader						m_shader_renderScene;
		Shader						m_shader_renderVoxels;
		Shader						m_shader_postProcess;

		Texture2D					m_tex_GBuffer;
		Texture2D					m_tex_sceneRadiance;
		Texture2D					m_Tex_BlueNoise;

		Primitive					m_prim_Cube;

		DateTime					m_startTime;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();


		public TestForm() {
			InitializeComponent();
			ComputeConeDirections();

			try {
				m_device.Init( panelOutput.Handle, false, true );

				m_shader_voxelizeScene = new ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/VoxelizeScene.hlsl" ), "CS" );
				m_shader_buildVoxelMips = new ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/VoxelizeScene.hlsl" ), "CS_Mip" );
				m_shader_buildSingleVoxelMips = new ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/VoxelizeScene.hlsl" ), "CS_SingleMip" );
				m_shader_accumulateVoxelLighting = new ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/VoxelizeScene.hlsl" ), "CS_Accumulate" );
				m_shader_computeIndirectLighting = new ComputeShader( m_device, new System.IO.FileInfo( "./Shaders/ComputeIndirectLighting.hlsl" ), "CS" );
				m_shader_renderGBuffer = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderGBuffer.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
				m_shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
				m_shader_renderVoxels = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderVoxels.hlsl" ), VERTEX_FORMAT.P3, "VS", null, "PS" );
				m_shader_postProcess = new Shader( m_device, new System.IO.FileInfo( "./Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );

				m_tex_GBuffer = new Texture2D( m_device, (uint) panelOutput.Width, (uint) panelOutput.Height, 4, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.UNORM, false, false, null );
				m_tex_sceneRadiance = new Texture2D( m_device, (uint) panelOutput.Width, (uint) panelOutput.Height, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.UNORM, false, false, null );

				using ( ImageFile blueNoise = new ImageFile( new System.IO.FileInfo( "BlueNoise64x64.png" ) ) ) {
					m_Tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] { { blueNoise } } ), COMPONENT_FORMAT.UNORM );
				}

				m_CB_computeIndirect = new ConstantBuffer< CB_ComputeIndirectLighting >( m_device, 10 );
				m_CB_global = new ConstantBuffer< CB_Global >( m_device, 0 );
				m_CB_camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
				m_CB_renderScene = new ConstantBuffer< CB_RenderScene >( m_device, 10 );
				m_CB_renderVoxels = new ConstantBuffer< CB_RenderVoxels >( m_device, 10 );
				m_CB_filtering = new ConstantBuffer< CB_Filtering >( m_device, 10 );
				m_CB_postProcess = new ConstantBuffer< CB_PostProcess >( m_device, 10 );

				// Build the cube primitive used to render voxels
				{
					VertexP3[]	vertices = new VertexP3[] {
						new VertexP3() { P = new float3( -1, -1, -1 ) },
						new VertexP3() { P = new float3(  1, -1, -1 ) },
						new VertexP3() { P = new float3(  1,  1, -1 ) },
						new VertexP3() { P = new float3( -1,  1, -1 ) },
						new VertexP3() { P = new float3( -1, -1, +1 ) },
						new VertexP3() { P = new float3(  1, -1, +1 ) },
						new VertexP3() { P = new float3(  1,  1, +1 ) },
						new VertexP3() { P = new float3( -1,  1, +1 ) },
					};
					uint[]		indices = new uint[] {
						3, 0, 4, 3, 4, 7,
						6, 5, 1, 6, 1, 2,
						3, 7, 6, 3, 6, 2,
						4, 0, 1, 4, 1, 5,
						2, 1, 0, 2, 0, 3,
						7,4, 5, 7, 5, 6,
					};
					m_prim_Cube = new Primitive( m_device, (uint) vertices.Length, VertexP3.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3 );
				}

				// Voxelize the scene & compute indirect lighting
				Voxelize();
				ComputeIndirectLighting( (uint) integerTrackbarControlBouncesCount.Value );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "An exception occurred while creating DX structures:\r\n" + _e.Message, "Error" );
			}

			// Initialize camera
			m_camera.CreatePerspectiveCamera( CAMERA_FOV, (float) panelOutput.Width / panelOutput.Height, 0.1f, 100.0f );
			m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
			m_cameraManipulator.Attach( panelOutput, m_camera );
//			m_cameraManipulator.InitializeCamera( new float3( 0, 1.5f, 4.0f ), new float3( 0, 1.5f, 0.0f ), float3.UnitY );
			m_cameraManipulator.InitializeCamera( new float3( 0.0f, 2.73f, -6.0f ), new float3( 0.0f, 2.73f, 0.0f ), float3.UnitY );

			m_startTime = DateTime.Now;
			Application.Idle += Application_Idle;
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			Device	D = m_device;
			m_device = null;
			D.Dispose();
		}

		#region Scene Voxelization

		const uint	VOLUME_SIZE = 128;
		Texture3D	m_Tex_VoxelScene_Albedo = null;
		Texture3D	m_Tex_VoxelScene_Normal = null;
		Texture3D	m_Tex_VoxelScene_Lighting = null;

		void	Voxelize() {
			if ( !m_shader_voxelizeScene.Use() )
				throw new Exception( "Can't use voxelization shader! Failed to compile?" );

			m_Tex_VoxelScene_Albedo = new Texture3D( m_device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 8, PIXEL_FORMAT.RGBA8 , COMPONENT_FORMAT.UNORM, false, true, null ); m_Tex_VoxelScene_Albedo.Tag = "Albedo";
			m_Tex_VoxelScene_Normal = new Texture3D( m_device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 8, PIXEL_FORMAT.RGBA8 , COMPONENT_FORMAT.SNORM, false, true, null ); m_Tex_VoxelScene_Normal.Tag = "Normal";
			m_Tex_VoxelScene_Lighting = new Texture3D( m_device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 8, PIXEL_FORMAT.RGBA16F , COMPONENT_FORMAT.AUTO, false, true, null ); m_Tex_VoxelScene_Lighting.Tag = "Direct Lighting";

			m_Tex_VoxelScene_Albedo.SetCSUAV( 0 );
			m_Tex_VoxelScene_Normal.SetCSUAV( 1 );
			m_Tex_VoxelScene_Lighting.SetCSUAV( 2 );

			uint	volumeSize = VOLUME_SIZE;
			m_shader_voxelizeScene.Dispatch( volumeSize >> 4, volumeSize >> 4, volumeSize );

			m_Tex_VoxelScene_Albedo.RemoveFromLastAssignedSlotUAV();
			m_Tex_VoxelScene_Normal.RemoveFromLastAssignedSlotUAV();
			m_Tex_VoxelScene_Lighting.RemoveFromLastAssignedSlotUAV();

			// Build the mips
			if ( !m_shader_buildVoxelMips.Use() )
				throw new Exception( "Can't use mip building shader!" );

			for ( uint mipLevelIndex=1; mipLevelIndex < m_Tex_VoxelScene_Albedo.MipLevelsCount; mipLevelIndex++ ) {
				volumeSize >>= 1;

				m_Tex_VoxelScene_Albedo.GetView( mipLevelIndex-1, 1, 0, 0 ).SetCS( 0 );
				m_Tex_VoxelScene_Normal.GetView( mipLevelIndex-1, 1, 0, 0 ).SetCS( 1 );
				m_Tex_VoxelScene_Lighting.GetView( mipLevelIndex-1, 1, 0, 0 ).SetCS( 2 );

				m_Tex_VoxelScene_Albedo.GetView( mipLevelIndex, 1, 0, 0 ).SetCSUAV( 0 );
				m_Tex_VoxelScene_Normal.GetView( mipLevelIndex, 1, 0, 0 ).SetCSUAV( 1 );
				m_Tex_VoxelScene_Lighting.GetView( mipLevelIndex, 1, 0, 0 ).SetCSUAV( 2 );

				uint	groupsCountXY = Math.Max( 1, volumeSize >> 4 );
				m_shader_buildVoxelMips.Dispatch( groupsCountXY, groupsCountXY, volumeSize );

				m_Tex_VoxelScene_Albedo.RemoveFromLastAssignedSlotUAV();
				m_Tex_VoxelScene_Normal.RemoveFromLastAssignedSlotUAV();
				m_Tex_VoxelScene_Lighting.RemoveFromLastAssignedSlotUAV();
			}
		}

		#endregion

		#region Indirect Lighting Computation

		Texture3D	m_Tex_VoxelScene_IndirectLighting0 = null;
		Texture3D	m_Tex_VoxelScene_IndirectLighting1 = null;
		Texture3D	m_Tex_VoxelScene_IndirectLighting2 = null;

		void	ComputeIndirectLighting( uint _bouncesCount ) {
			if ( !m_shader_computeIndirectLighting.Use() )
				throw new Exception( "Can't use voxelization shader! Failed to compile?" );

			if ( m_Tex_VoxelScene_IndirectLighting0 == null ) {
				m_Tex_VoxelScene_IndirectLighting0 = new Texture3D( m_device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 8, PIXEL_FORMAT.RGBA16F , COMPONENT_FORMAT.AUTO, false, true, null ); m_Tex_VoxelScene_IndirectLighting0.Tag = "IL0";
				m_Tex_VoxelScene_IndirectLighting1 = new Texture3D( m_device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 8, PIXEL_FORMAT.RGBA16F , COMPONENT_FORMAT.AUTO, false, true, null ); m_Tex_VoxelScene_IndirectLighting1.Tag = "IL1";
				m_Tex_VoxelScene_IndirectLighting2 = new Texture3D( m_device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 8, PIXEL_FORMAT.RGBA16F , COMPONENT_FORMAT.AUTO, false, true, null ); m_Tex_VoxelScene_IndirectLighting2.Tag = "IL2";
			}
			m_Tex_VoxelScene_IndirectLighting0.RemoveFromLastAssignedSlots();
			m_Tex_VoxelScene_IndirectLighting1.RemoveFromLastAssignedSlots();
			m_Tex_VoxelScene_IndirectLighting2.RemoveFromLastAssignedSlots();
			m_device.Clear( m_Tex_VoxelScene_IndirectLighting2, float4.Zero );

			m_Tex_VoxelScene_Albedo.SetCS( 0 );
			m_Tex_VoxelScene_Normal.SetCS( 1 );
			m_Tex_VoxelScene_Lighting.SetCS( 2 );	// Start with direct lighting as our source

//			const uint	BOUNCES_COUNT = 2;
			for ( uint bounceIndex=0; bounceIndex < _bouncesCount; bounceIndex++ ) {

				/////////////////////////////////////////////////////////////////
				// 1] Computes indirect lighting from existing lighting
				if ( m_shader_computeIndirectLighting.Use() ) {
					// Source lighting to bounce one more time is supposed to be in m_Tex_VoxelScene_IndirectLighting0 at this point (and assigned to input slot t2)
					m_Tex_VoxelScene_IndirectLighting1.SetCSUAV( 0 );

m_CB_computeIndirect.m._mipLevel = bounceIndex;
m_CB_computeIndirect.UpdateData();

					m_shader_computeIndirectLighting.Dispatch( VOLUME_SIZE >> 4, VOLUME_SIZE >> 4, VOLUME_SIZE );

					m_Tex_VoxelScene_IndirectLighting1.RemoveFromLastAssignedSlotUAV();
 					m_Tex_VoxelScene_IndirectLighting0.RemoveFromLastAssignedSlots();
				}

				/////////////////////////////////////////////////////////////////
				// 2] Build mip maps
				if ( m_shader_buildSingleVoxelMips.Use() ) {
					uint	volumeSize = VOLUME_SIZE;
					for ( uint mipLevelIndex=1; mipLevelIndex < m_Tex_VoxelScene_IndirectLighting1.MipLevelsCount; mipLevelIndex++ ) {
						volumeSize >>= 1;

						m_Tex_VoxelScene_IndirectLighting1.GetView( mipLevelIndex, 1, 0, 0 ).SetCSUAV( 2 );
						m_Tex_VoxelScene_IndirectLighting1.GetView( mipLevelIndex-1, 1, 0, 0 ).SetCS( 2 );

						uint	groupsCountXY = Math.Max( 1, volumeSize >> 4 );
						m_shader_buildSingleVoxelMips.Dispatch( groupsCountXY, groupsCountXY, volumeSize );

						m_Tex_VoxelScene_IndirectLighting1.RemoveFromLastAssignedSlotUAV();
					}
				}

				/////////////////////////////////////////////////////////////////
				// 3] Scroll indirect lighting buffers for next bounce
				// At this point:
				//	• m_Tex_VoxelScene_IndirectLighting0 was the previous source bounce, it's not used anymore and can be re-used for something else
				//	• m_Tex_VoxelScene_IndirectLighting1 was just rendered as the new bounce
				//	• m_Tex_VoxelScene_IndirectLighting2 contains the current total accumulated bounces
				//
				Texture3D	temp = m_Tex_VoxelScene_IndirectLighting0;
				m_Tex_VoxelScene_IndirectLighting0 = m_Tex_VoxelScene_IndirectLighting1;
				m_Tex_VoxelScene_IndirectLighting1 = m_Tex_VoxelScene_IndirectLighting2;
				m_Tex_VoxelScene_IndirectLighting2 = temp;

				// Now at this point:
				//	• m_Tex_VoxelScene_IndirectLighting0 was just rendered as the new bounce and will now be used as source for next bounce
				//	• m_Tex_VoxelScene_IndirectLighting1 contains the current total accumulated bounces 
				//	• m_Tex_VoxelScene_IndirectLighting2 was the previous source bounce and will be used as our new total accumulated bounce computed at stage 4]

				/////////////////////////////////////////////////////////////////
				// 4] Accumulate to existing lighting
				if ( m_shader_accumulateVoxelLighting.Use() ) {
					m_Tex_VoxelScene_IndirectLighting0.SetCS( 2 );		// Contains the just rendered bounce
					m_Tex_VoxelScene_IndirectLighting1.SetCS( 3 );		// Contains previous total accumulated bounce
					m_Tex_VoxelScene_IndirectLighting2.SetCSUAV( 2 );

					m_shader_accumulateVoxelLighting.Dispatch( VOLUME_SIZE >> 4, VOLUME_SIZE >> 4, VOLUME_SIZE );

					m_Tex_VoxelScene_IndirectLighting0.RemoveFromLastAssignedSlots();
					m_Tex_VoxelScene_IndirectLighting1.RemoveFromLastAssignedSlots();
					m_Tex_VoxelScene_IndirectLighting2.RemoveFromLastAssignedSlotUAV();
				}

				m_Tex_VoxelScene_IndirectLighting0.SetCS( 2 );	// Becomes source lighting for next bounce
			}

			m_Tex_VoxelScene_IndirectLighting0.RemoveFromLastAssignedSlots();
			m_Tex_VoxelScene_Albedo.RemoveFromLastAssignedSlots();
			m_Tex_VoxelScene_Normal.RemoveFromLastAssignedSlots();

			/////////////////////////////////////////////////////////////////
			// Build final mip maps on the accumulated lighting
			if ( m_shader_buildSingleVoxelMips.Use() ) {
				uint	volumeSize = VOLUME_SIZE;
				for ( uint mipLevelIndex=1; mipLevelIndex < m_Tex_VoxelScene_IndirectLighting2.MipLevelsCount; mipLevelIndex++ ) {
					volumeSize >>= 1;

					m_Tex_VoxelScene_IndirectLighting2.GetView( mipLevelIndex-1, 1, 0, 0 ).SetCS( 2 );
					m_Tex_VoxelScene_IndirectLighting2.GetView( mipLevelIndex, 1, 0, 0 ).SetCSUAV( 2 );

					uint	groupsCountXY = Math.Max( 1, volumeSize >> 4 );
					m_shader_buildSingleVoxelMips.Dispatch( groupsCountXY, groupsCountXY, volumeSize );

					m_Tex_VoxelScene_IndirectLighting2.RemoveFromLastAssignedSlotUAV();
				}
			}
		}

		/// <summary>
		/// Uses the same "repulsion" algorithm as the Voronoi Visualizer tool
		/// </summary>
		void	ComputeConeDirections() {
			uint	CONES_COUNT = 7;
			float	CONE_APERTURE = (float) (60.0f * Math.PI / 180.0f);
//			float	FORCE = 1.0f;

			float	COS_HALF_ANGLE = (float) Math.Cos( 0.5 * CONE_APERTURE );

			float3[]	positions = new float3[CONES_COUNT];
			positions[0] = float3.UnitZ ;
			for ( int i=0; i < 6; i++ ) {
				float	phi = 2.0f * (float) Math.PI * i / 6;
				float	theta = 60.0f * (float) Math.PI / 180.0f;

				float	sinTheta = (float) Math.Sin( theta );
				float	cosTheta = (float) Math.Cos( theta );

				positions[1+i] = new float3( sinTheta * (float) Math.Cos( phi ), sinTheta * (float) Math.Sin( phi ), cosTheta );
			}

/*			float3[]	forces = new float3[CONES_COUNT];
			for ( int iterationIndex=0; iterationIndex < 1000; iterationIndex++ ) {

				// Compute forces to apply to each cone
				for ( int coneIndex=0; coneIndex < CONES_COUNT-1; coneIndex++ ) {
					float3	D0 = positions[coneIndex];
					for ( int neighborConeIndex=coneIndex+1; neighborConeIndex < CONES_COUNT; neighborConeIndex++ ) {
						float3	D1 = positions[neighborConeIndex];

						float3	Dir = (D1 - D0).Normalized;

						float	dot = D0.Dot( D1 ) - 1.0f;			// in [0,-2]
						float	force = FORCE * (float) Math.Exp( dot );
						forces[coneIndex] -= force * Dir;			// Pushes 0 away from 1
						forces[neighborConeIndex] = + force * Dir;	// Pushes 1 away from 0
					}

					// Apply force to prevent cone to go below z=0 plane
					float	planeForce = Math.Max( 0.0f, COS_HALF_ANGLE / D0.z - 1.0f );
					forces[coneIndex] += planeForce * float3.UnitZ;
				}

				// Apply force (except first one that is fixed along normal)
				for ( int i=1; i < CONES_COUNT; i++ ) {
					positions[i] = (positions[i] + forces[i]).Normalized;
				}
			}
*/


			// Transform into text
			string	code = "{\r\n";
			for ( int i=0; i < CONES_COUNT; i++ ) {
				code += "	float3( " + positions[i].x + ", " + positions[i].y + ", " + positions[i].z + " ),\r\n" ;
			}
			code += "};\r\n" ;
		}

		#endregion

		void m_camera_CameraTransformChanged(object sender, EventArgs e) {
			m_CB_camera.m._Camera2World = m_camera.Camera2World;
			m_CB_camera.m._World2Camera = m_camera.World2Camera;
			m_CB_camera.m._Proj2World = m_camera.Proj2World;
			m_CB_camera.m._World2Proj = m_camera.World2Proj;
			m_CB_camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_camera.m._Proj2Camera = m_camera.Proj2Camera;
			m_CB_camera.UpdateData();
		}

		void Application_Idle(object sender, EventArgs e) {
			if ( m_device == null )
				return;

			DateTime	currentTime = DateTime.Now;
			float		totalTime = (float) (currentTime - m_startTime).TotalSeconds;
			m_CB_global.m._resolutionX = (uint) panelOutput.Width;
			m_CB_global.m._resolutionY = (uint) panelOutput.Height;
			m_CB_global.m._time = totalTime;
			m_CB_global.UpdateData();

			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

			//////////////////////////////////////////////////////////////////////////
			// Render the G-Buffer (albedo + gloss + normal + distance)
			if ( m_shader_renderGBuffer.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTargets( new IView[] { m_tex_GBuffer.GetView( 0, 1, 0, 1 ), m_tex_GBuffer.GetView( 0, 1, 1, 1 ), m_tex_GBuffer.GetView( 0, 1, 2, 1 ), m_tex_GBuffer.GetView( 0, 1, 3, 1 ) }, null );
				m_device.RenderFullscreenQuad( m_shader_renderGBuffer );
			}

// 			//////////////////////////////////////////////////////////////////////////
// 			// Render the GI scene
// 			if ( m_shader_renderScene.Use() ) {
// 				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
// 				m_device.SetRenderTarget( m_tex_sceneRadiance, null );
// 
// 				m_tex_GBuffer.Set( 0 );
// 				m_Tex_BlueNoise.Set( 1 );
// 
// //				m_CB_renderScene.m._lightSize = floatTrackbarControlLightSize.Value;
// 				m_CB_renderScene.UpdateData();
// 
// 				m_device.RenderFullscreenQuad( m_shader_renderScene );
// 			}
m_device.Clear( m_tex_sceneRadiance, float4.Zero );

			//////////////////////////////////////////////////////////////////////////
			// Render many instanced voxel cubes
			if ( m_shader_renderVoxels.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_tex_sceneRadiance, m_device.DefaultDepthStencil );

				m_Tex_VoxelScene_Albedo.SetVS( 0 );
				m_Tex_VoxelScene_Normal.SetVS( 1 );
				m_Tex_VoxelScene_Lighting.SetVS( 2 );
				m_Tex_VoxelScene_IndirectLighting0.SetVS( 3 );
				m_Tex_VoxelScene_IndirectLighting1.SetVS( 4 );
				m_Tex_VoxelScene_IndirectLighting2.SetVS( 5 );

				uint	POT = (uint) (7 - integerTrackbarControlVoxelMipIndex.Value);
				uint	count = (uint) (1 << (int) POT);
				uint	mask = (uint) (0x7F >> integerTrackbarControlVoxelMipIndex.Value);
				float	size = 0.05f * (1 << integerTrackbarControlVoxelMipIndex.Value);
				m_CB_renderVoxels.m._voxelSizeX = size;
				m_CB_renderVoxels.m._voxelSizeY = size;
				m_CB_renderVoxels.m._voxelSizeZ = size;
				m_CB_renderVoxels.m._voxelPOTX = POT;
				m_CB_renderVoxels.m._voxelPOTY = POT;
				m_CB_renderVoxels.m._voxelPOTZ = POT;
				m_CB_renderVoxels.m._mipLevel = (uint) integerTrackbarControlVoxelMipIndex.Value;
				m_CB_renderVoxels.m._voxelMasksX = mask;
				m_CB_renderVoxels.m._voxelMasksY = mask;
				m_CB_renderVoxels.m._voxelMasksZ = mask;
				m_CB_renderVoxels.m._flags = checkBoxEnableIndirect.Checked ? 2U : 0U;
				m_CB_renderVoxels.UpdateData();

				m_prim_Cube.RenderInstanced( m_shader_renderVoxels, count * count * count );
			}

			//////////////////////////////////////////////////////////////////////////
			// Render the filtered result
			if ( m_shader_postProcess.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_GBuffer.Set( 0 );
				m_tex_sceneRadiance.Set( 1 );

m_Tex_VoxelScene_Lighting.Set( 2 );
m_Tex_VoxelScene_IndirectLighting0.Set( 3 );
m_Tex_VoxelScene_IndirectLighting1.Set( 4 );
m_Tex_VoxelScene_IndirectLighting2.Set( 5 );

				m_CB_postProcess.m._flags = (checkBoxRenderAsVoxels.Checked ? 1U : 0U) | (checkBoxEnableIndirect.Checked ? 2U : 0U);
				m_CB_postProcess.m._filterLevel = (uint) integerTrackbarControlVoxelMipIndex.Value;
				m_CB_postProcess.UpdateData();

				m_device.RenderFullscreenQuad( m_shader_postProcess );

				m_tex_GBuffer.RemoveFromLastAssignedSlots();
				m_tex_sceneRadiance.RemoveFromLastAssignedSlots();
			}

			m_device.Present( false );
		}

		private void buttonReload_Click(object sender, EventArgs e) {
			m_device.ReloadModifiedShaders();
		}

		private void buttonComputeIndirect_Click(object sender, EventArgs e)
		{
			ComputeIndirectLighting( (uint) integerTrackbarControlBouncesCount.Value );
		}
	}
}
