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

namespace TestWaveletATrousFiltering
{
	public partial class Form1 : Form {

		const float	CAMERA_FOV = (float) (90.0 * Math.PI / 180.0);
		const uint	FILTER_ITERATIONS_COUNT = 5;

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Global {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public float	_time;
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
		private struct CB_Filtering {
			public float		_stride;
			public float		_sigma_Color;
			public float		_sigma_Normal;
			public float		_sigma_Position;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_PostProcess {
			public uint			_filterLevel;
		}

		#endregion

		Device						m_device = new Device();

		ConstantBuffer< CB_Global >			m_CB_global;
		ConstantBuffer< CB_Camera >			m_CB_camera = null;
		ConstantBuffer< CB_RenderScene >	m_CB_renderScene = null;
		ConstantBuffer< CB_Filtering >		m_CB_filtering = null;
		ConstantBuffer< CB_PostProcess >	m_CB_postProcess = null;

		Shader						m_shader_renderGBuffer;
		Shader						m_shader_renderScene;
		Shader						m_shader_filter;
		Shader						m_shader_postProcess;

		Texture2D					m_tex_GBuffer;
		Texture2D					m_tex_sceneRadiance;
		Texture2D					m_Tex_BlueNoise;

		DateTime					m_startTime;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();


		public Form1() {
			InitializeComponent();

// 			BuildKernelWeights();

			try {
				m_device.Init( panelOutput.Handle, false, true );

				m_shader_renderGBuffer = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderGBuffer.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_renderScene = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_filter = new Shader( m_device, new System.IO.FileInfo( "./Shaders/ATrousFilter.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_postProcess = new Shader( m_device, new System.IO.FileInfo( "./Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

				m_tex_GBuffer = new Texture2D( m_device, (uint) panelOutput.Width, (uint) panelOutput.Height, 3, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.UNORM, false, false, null );
				m_tex_sceneRadiance = new Texture2D( m_device, (uint) panelOutput.Width, (uint) panelOutput.Height, 1+(int)FILTER_ITERATIONS_COUNT, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.UNORM, false, false, null );

				using ( ImageFile blueNoise = new ImageFile( new System.IO.FileInfo( "BlueNoise64x64.png" ) ) ) {
					m_Tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] { { blueNoise } } ), COMPONENT_FORMAT.UNORM );
				}

				m_CB_global = new ConstantBuffer< CB_Global >( m_device, 0 );
				m_CB_camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
				m_CB_renderScene = new ConstantBuffer< CB_RenderScene >( m_device, 10 );
				m_CB_filtering = new ConstantBuffer< CB_Filtering >( m_device, 10 );
				m_CB_postProcess = new ConstantBuffer< CB_PostProcess >( m_device, 10 );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Error", "An exception occurred while creating DX structures:\r\n" + _e.Message );
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

		void	BuildKernelWeights() {
			double[]	B3Spline = new double[5] { 1/16.0, 1/4.0, 3/8.0, 1/4.0, 1/16.0 };
			double		sum = 0.0;
			double[,]	kernel = new double[5,5];
			for ( int Y=-2; Y <=2; Y++ )
				for ( int X=-2; X <=2; X++ ) {
					double	weight = B3Spline[2+Y] * B3Spline[2+X];
					kernel[2+X,2+Y] = weight;
					sum += weight;
				}

			// Normalize
			double	norm = 1.0 / sum;
			for ( int Y=-2; Y <=2; Y++ )
				for ( int X=-2; X <=2; X++ )
					kernel[2+X,2+Y] *= norm;
		}

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

			//////////////////////////////////////////////////////////////////////////
			// Render the G-Buffer (albedo + gloss + normal + distance)
			if ( m_shader_renderGBuffer.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTargets( new IView[] { m_tex_GBuffer.GetView( 0, 1, 0, 1 ), m_tex_GBuffer.GetView( 0, 1, 1, 1 ), m_tex_GBuffer.GetView( 0, 1, 2, 1 ) }, null );
				m_device.RenderFullscreenQuad( m_shader_renderGBuffer );
			}

			//////////////////////////////////////////////////////////////////////////
			// Render the GI scene
			if ( m_shader_renderScene.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTargets( new IView[] { m_tex_sceneRadiance.GetView( 0, 1, 0, 1 ) }, null );

				m_tex_GBuffer.Set( 0 );
				m_Tex_BlueNoise.Set( 1 );

				m_CB_renderScene.m._lightSize = floatTrackbarControlLightSize.Value;
				m_CB_renderScene.UpdateData();

				m_device.RenderFullscreenQuad( m_shader_renderScene );
			}

			//////////////////////////////////////////////////////////////////////////
			// Apply à-trous filtering
			if ( m_shader_filter.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				float	SIGMA_COLOR = floatTrackbarControlSigmaColor.Value;
				float	SIGMA_NORMAL = floatTrackbarControlSigmaNormal.Value;
				float	SIGMA_POSITION = floatTrackbarControlSigmaPosition.Value;

				m_tex_GBuffer.Set( 0 );

				for ( uint levelIndex=0; levelIndex < FILTER_ITERATIONS_COUNT; levelIndex++ ) {
					m_device.SetRenderTargets( new IView[] { m_tex_sceneRadiance.GetView( 0, 1, levelIndex+1, 1 ) }, null );
					m_tex_sceneRadiance.GetView( 0, 1, levelIndex, 1 ).Set( 1 );

					float	POT = (float) Math.Pow( 2.0, levelIndex );
					float	sigma_Color = SIGMA_COLOR / POT;	// Here, increase range for larger filter sizes
//  Thus the edge-stopping function depends at the first level on wn and wx only.
// ===> if level index == 0 => wrt = 1 ?
// 					if ( levelIndex == 0 )
// 						sigma_Color = 1.0f;

					m_CB_filtering.m._stride = POT;
					m_CB_filtering.m._sigma_Color = -1.0f / (sigma_Color * sigma_Color);
					m_CB_filtering.m._sigma_Normal = -1.0f / (SIGMA_NORMAL * SIGMA_NORMAL);
					m_CB_filtering.m._sigma_Position = -1.0f / (SIGMA_POSITION * SIGMA_POSITION);
					m_CB_filtering.UpdateData();

					m_device.RenderFullscreenQuad( m_shader_filter );
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// Render the filtered result
			if ( m_shader_postProcess.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_GBuffer.Set( 0 );
				m_tex_sceneRadiance.Set( 1 );

				m_CB_postProcess.m._filterLevel = (uint) (checkBoxToggleFilter.Checked ? integerTrackbarControlFilterLevel.Value : 0);
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
	}
}
