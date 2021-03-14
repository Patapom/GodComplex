using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using Renderer;
using SharpMath;
using UIUtility;

namespace AreaLightTest {
	public partial class AreaLightForm : Form {

		#region CONSTANTS

		const float			FOV_DEGREES = 60.0f;
		readonly float3		PLANE_NORMAL = float3.UnitY;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		iResolution;
			public float		tanHalfFOV;
			public float		iGlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_camera2World;
			public float4x4		_world2Camera;
			public float4x4		_proj2World;
			public float4x4		_world2Proj;
			public float4x4		_camera2Proj;
			public float4x4		_proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Light {
			public float4x4		_wsLight2World;
			public float		_luminance;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_TestQuad {
			public float4x4		_wsLight2World;
			public float4		_invM_transposed_r0;
			public float4		_invM_transposed_r1;
			public float4		_invM_transposed_r2;
		}

		#endregion

		#region FIELDS

		private Device				m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_Light>		m_CB_Light = null;
		private ConstantBuffer<CB_TestQuad>		m_CB_TestQuad = null;

		private Shader				m_shader_RenderLight = null;
		private Shader				m_shader_RenderScene = null;
		private Shader				m_shader_RenderScene_Reference = null;
		private Shader				m_shader_RenderTestQuad = null;
		private Shader				m_shader_RenderDiff = null;

		private Primitive			m_prim_disk = null;

		private Texture2D			m_RT_temp0 = null;
		private Texture2D			m_RT_temp1 = null;
		private Texture2D			m_tex_FalseColors = null;

		// Pre-integrated BRDF textures
		Texture2D					m_tex_MSBRDF_E;
		Texture2D					m_tex_MSBRDF_Eavg;

			// LTC texture
		Texture2D					m_tex_LTC;
		Texture2D					m_tex_MS_LTC;

		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		private float3				m_wsLightTargetPosition = float3.Zero;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double				m_ticks2Seconds;
		public float				m_startTime = 0;
		public float				m_CurrentTime = 0;
		public float				m_DeltaTime = 0;		// Delta time used for the current frame

		#endregion

		#region METHODS

		public AreaLightForm() {
			InitializeComponent();

			CheckMatrix();
		}

		#region Open/Close

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
			m_CB_Light = new ConstantBuffer<CB_Light>( m_device, 2 );
			m_CB_TestQuad = new ConstantBuffer<CB_TestQuad>( m_device, 3 );

			try {
				m_shader_RenderLight = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderLight.hlsl" ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null );;
				m_shader_RenderScene = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
				m_shader_RenderScene_Reference = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene_Reference.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
				m_shader_RenderDiff = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderDiff.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
				m_shader_RenderTestQuad = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderTestQuad.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			BuildPrimitives();

			// Allocate temp targets & false colors spectrum
			m_RT_temp0 = new Texture2D( m_device, m_device.DefaultTarget.Width, m_device.DefaultTarget.Height, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_RT_temp1 = new Texture2D( m_device, m_device.DefaultTarget.Width, m_device.DefaultTarget.Height, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_FalseColors = Image2Texture( new System.IO.FileInfo( "FalseColorsSpectrum.png" ), ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );

			// Setup camera
			m_camera.CreatePerspectiveCamera( (float) (FOV_DEGREES * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_manipulator.Attach( panelOutput, m_camera );
			m_manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
			m_manipulator.EnableMouseAction += m_manipulator_EnableMouseAction;

			// Pre-integrated BRDF tables
			LoadMSBRDF( new uint[] { 128, 32 },
						new System.IO.FileInfo[] {
							new System.IO.FileInfo( "./Tables/MSBRDF_GGX_G2_E128x128.float" ),
							new System.IO.FileInfo( "./Tables/MSBRDF_OrenNayar_E32x32.float" ),
						},
						new System.IO.FileInfo[] {
							new System.IO.FileInfo( "./Tables/MSBRDF_GGX_G2_Eavg128.float" ),
							new System.IO.FileInfo( "./Tables/MSBRDF_OrenNayar_Eavg32.float" ),
						},
						out m_tex_MSBRDF_E, out m_tex_MSBRDF_Eavg
			);

			// LTC Tables
			m_tex_LTC = LoadLTC( new System.IO.FileInfo( @".\Tables\LTC.dds" ) );
			m_tex_MS_LTC = LoadMSLTC( new System.IO.FileInfo( @".\Tables\MS_LTC.dds" ) );


			// Start game time
			m_ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_stopWatch.Start();
			m_startTime = GetGameTime();

			m_camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_device == null )
				return;

			m_tex_MS_LTC.Dispose();
			m_tex_LTC.Dispose();

			m_tex_MSBRDF_Eavg.Dispose();
			m_tex_MSBRDF_E.Dispose();

			m_RT_temp0.Dispose();
			m_RT_temp1.Dispose();
			m_tex_FalseColors.Dispose();

			m_shader_RenderLight.Dispose();
			m_shader_RenderScene.Dispose();
			m_shader_RenderScene_Reference.Dispose();
			m_shader_RenderDiff.Dispose();
			m_shader_RenderTestQuad.Dispose();

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Light.Dispose();
			m_CB_TestQuad.Dispose();

			m_prim_disk.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		const int		VERTICES_COUNT = 1024;

		void		BuildPrimitives() {
			VertexP3N3[]	vertices = new VertexP3N3[2*VERTICES_COUNT];
			uint[]			indices = new uint[3*VERTICES_COUNT];
			for ( int i=0; i < VERTICES_COUNT; i++ ) {
				float	a = Mathf.TWOPI * i / VERTICES_COUNT;
				float3	P = new float3( Mathf.Cos( a ), Mathf.Sin( a ), 0 );

				vertices[2*i+0].P = P;
				vertices[2*i+0].N = new float3( (float) i / VERTICES_COUNT, 0, 0 );
				vertices[2*i+1].P = P;
				vertices[2*i+1].N = new float3( (float) i / VERTICES_COUNT, 1, 0 );

				indices[3*i+0] = (uint) (2*i+0);
				indices[3*i+1] = (uint) (2*i+1);
				indices[3*i+2] = (uint) (2*((i+1) % VERTICES_COUNT) + 1);
			}

			m_prim_disk = new Primitive( m_device, 2*VERTICES_COUNT, VertexP3N3.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3 );
		}

		#region LTC Area Lights

		Texture2D	LoadLTC( System.IO.FileInfo _LTCFileName ) {
			using ( ImageUtility.ImagesMatrix M = new ImageUtility.ImagesMatrix() ) {
				M.DDSLoadFile( _LTCFileName );
				Texture2D	T = new Texture2D( m_device, M, ImageUtility.COMPONENT_FORMAT.AUTO );
				return T;
			}
		}

		Texture2D	LoadMSLTC( System.IO.FileInfo _MSLTCFileName ) {
			using ( ImageUtility.ImagesMatrix M = new ImageUtility.ImagesMatrix() ) {
				M.DDSLoadFile( _MSLTCFileName );
				Texture2D	T = new Texture2D( m_device, M, ImageUtility.COMPONENT_FORMAT.AUTO );
				return T;
			}
		}

		#endregion

		#region Pre-Integrated BRDF Tables

		void	LoadMSBRDF( uint[] _sizes, System.IO.FileInfo[] _irradianceTablesNames, System.IO.FileInfo[] _albedoTablesNames, out Texture2D _irradianceTexture, out Texture2D _albedoTexture ) {

			uint	BRDFSCount = (uint) _sizes.Length;
			if ( _irradianceTablesNames.Length != BRDFSCount || _albedoTablesNames.Length != BRDFSCount )
				throw new Exception( "Irradiance and albedo textures count must match the amount of BRDFs computed from the size of the _sizes array!" );

			// Determine max texture size
			uint	textureSize = 0;
			foreach ( uint size in _sizes )
				textureSize = Math.Max( textureSize, size );

			// Create placeholders
			ImageUtility.ImagesMatrix	texE = new ImageUtility.ImagesMatrix();
			texE.InitTexture2DArray( textureSize, textureSize, BRDFSCount, 1 );
			texE.AllocateImageFiles( ImageUtility.PIXEL_FORMAT.R32F, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR ) );

			ImageUtility.ImagesMatrix	texEavg = new ImageUtility.ImagesMatrix();
			texEavg.InitTexture2DArray( textureSize, BRDFSCount, 1, 1 );
			texEavg.AllocateImageFiles( ImageUtility.PIXEL_FORMAT.R32F, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR ) );

			float[][]	albedoTables = new float[BRDFSCount][];
			for ( uint BRDFIndex=0; BRDFIndex < BRDFSCount; BRDFIndex++ ) {
				uint	size = _sizes[BRDFIndex];

				// Read irradiance table
				float[,]	irradianceTable = new float[size,size];
				using ( System.IO.FileStream S = _irradianceTablesNames[BRDFIndex].OpenRead() )
					using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
						for ( int Y=0; Y < size; Y++ ) {
							for ( int X=0; X < size; X++ ) {
								irradianceTable[X,Y] = R.ReadSingle();
							}
						}
					}

				// Write content
				if ( size == textureSize ) {
					// One-one correspondance
					texE[BRDFIndex][0][0].WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						_color.x = irradianceTable[_X,_Y];
					} );
				} else {
					// Needs scaling
					texE[BRDFIndex][0][0].WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
						_color.x = Mathf.BiLerp( irradianceTable, (float) _X / textureSize, (float) _Y / textureSize );
					} );
				}

				// Read albedo table
				float[]	albedoTable = new float[size];
				albedoTables[BRDFIndex] = albedoTable;
				using ( System.IO.FileStream S = _albedoTablesNames[BRDFIndex].OpenRead() )
					using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
						for ( int Y=0; Y < size; Y++ ) {
							albedoTable[Y] = R.ReadSingle();
						}
					}
			}

			// Build the entire albedo tables
			texEavg[0][0][0].WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
				_color.x = Mathf.Lerp( albedoTables[_Y], (float) _X / textureSize );
			} );


			// Create textures
			_irradianceTexture = new Texture2D( m_device, texE, ImageUtility.COMPONENT_FORMAT.AUTO );
			_albedoTexture = new Texture2D( m_device, texEavg, ImageUtility.COMPONENT_FORMAT.AUTO );
		}

		#endregion

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _fileName, ImageUtility.COMPONENT_FORMAT _componentFormat ) {
			ImageUtility.ImagesMatrix	images = null;
			if ( _fileName.Extension.ToLower() == ".dds" ) {
				images = new ImageUtility.ImagesMatrix();
				images.DDSLoadFile( _fileName );
			} else {
				ImageUtility.ImageFile	image = new ImageUtility.ImageFile( _fileName );
				if ( image.PixelFormat != ImageUtility.PIXEL_FORMAT.BGRA8 ) {
					ImageUtility.ImageFile	badImage = image;
					image = new ImageUtility.ImageFile();
					image.ConvertFrom( badImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
					badImage.Dispose();
				}
				images = new ImageUtility.ImagesMatrix( new ImageUtility.ImageFile[1,1] { { image } } );
			}
			return new Texture2D( m_device, images, _componentFormat );
		}

		#endregion

		#endregion

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_stopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_ticks2Seconds);
			return Time;
		}

		#region Manipulation

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._camera2World = m_camera.Camera2World;
			m_CB_Camera.m._world2Camera = m_camera.World2Camera;

			m_CB_Camera.m._camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._proj2Camera = m_CB_Camera.m._camera2Proj.Inverse;

			m_CB_Camera.m._world2Proj = m_CB_Camera.m._world2Camera * m_CB_Camera.m._camera2Proj;
			m_CB_Camera.m._proj2World = m_CB_Camera.m._proj2Camera * m_CB_Camera.m._camera2World;

			m_CB_Camera.UpdateData();
		}

		float3	ComputeCameraRay( PointF _clienPosition ) {
			float3	pos;
			float	f;
			return ComputeCameraRay( _clienPosition, out pos, out f );
		}
		float3	ComputeCameraRay( PointF _clienPosition, out float3 _csView, out float _Z2Length ) {
			_csView = new float3( m_CB_Main.m.tanHalfFOV * (2.0f * new float2( _clienPosition.X, _clienPosition.Y ) / m_CB_Main.m.iResolution - float2.One), 1.0f );
			_csView.x *= m_CB_Main.m.iResolution.x / m_CB_Main.m.iResolution.y;
			_csView.y = -_csView.y;
			
			_Z2Length = _csView.Length;
			_csView /= _Z2Length;

			float3	wsView =  (new float4( _csView, 0 ) * m_CB_Camera.m._camera2World ).xyz;
			return wsView;
		}

		private bool m_manipulator_EnableMouseAction( MouseEventArgs _e ) {
			return (Control.ModifierKeys & Keys.Control) == Keys.None;
		}

		bool	m_manipulatingLight = false;
		Point	m_buttonDownPosition;
		private void panelOutput_MouseDown( object sender, MouseEventArgs _e ) {
			if ( _e.Button == MouseButtons.Right ) {
				float3	wsCamPos = m_CB_Camera.m._camera2World.r3.xyz;
				float3	wsView = ComputeCameraRay( _e.Location );
				float	t = -wsCamPos.y / wsView.y;
				if ( t > 0.0f )
					ComputeResult( wsCamPos + t * wsView );
			}

			if ( (Control.ModifierKeys & Keys.Control) == Keys.None )
				return;

//			if ( (_e.Button & MouseButtons.Left) == MouseButtons.Left )
			if ( (_e.Button & MouseButtons.Left) != 0 )
				m_manipulatingLight = true;
			m_buttonDownPosition = _e.Location;
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs _e ) {
			if ( (_e.Button & MouseButtons.Left) != 0 ) {
				m_manipulatingLight = false;
				m_buttonDownPosition = _e.Location;
			}
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs _e ) {
			if ( !m_manipulatingLight )
				return;

			float3	wsCamPos = m_CB_Camera.m._camera2World.r3.xyz;
			float3	wsView = ComputeCameraRay( _e.Location );
			float	t = -wsCamPos.y / wsView.y;
			if ( t <= 0.0f )
				return;	// No intersection with the plane...

			m_wsLightTargetPosition = wsCamPos + t * wsView;
		}

		#endregion

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float2( panelOutput.Width, panelOutput.Height );
			m_CB_Main.m.tanHalfFOV = Mathf.Tan( 0.5f * Mathf.ToRad( FOV_DEGREES ) );
			m_CB_Main.m.iGlobalTime = GetGameTime() - m_startTime;
			m_CB_Main.UpdateData();

			// Setup light data
			float3	wsLightPosition = new float3( floatTrackbarControlLightPosX.Value, floatTrackbarControlLightPosY.Value, floatTrackbarControlLightPosZ.Value );
			float3	at = (m_wsLightTargetPosition - wsLightPosition).Normalized;
			if ( radioButtonNegativeFreeTarget.Checked )
				at = -at;
			else if ( radioButtonHorizontalTarget.Checked ) {
				at.y = 0;
				at.Normalize();
			}

			float	roll = Mathf.ToRad( floatTrackbarControlLightRoll.Value );
			float3	left, up;
			at.OrthogonalBasis( out left, out up );

			float3	axisX =  Mathf.Cos( roll ) * left + Mathf.Sin( roll ) * up;
			float3	axisY = -Mathf.Sin( roll ) * left + Mathf.Cos( roll ) * up;

			float	radiusX = floatTrackbarControlLightScaleX.Value;
			float	radiusY = floatTrackbarControlLightScaleY.Value;

			m_CB_Light.m._luminance = floatTrackbarControlLuminance.Value;
			m_CB_Light.m._wsLight2World.r0.Set( axisX, radiusX );
			m_CB_Light.m._wsLight2World.r1.Set( axisY, radiusY );
			m_CB_Light.m._wsLight2World.r2.Set( at, Mathf.PI * radiusX * radiusY );	// Disk area in W
			m_CB_Light.m._wsLight2World.r3.Set( wsLightPosition, 1 );
			m_CB_Light.UpdateData();

			// Upload FGD & LTC tables
			m_tex_MSBRDF_E.SetPS( 2 );
			m_tex_MSBRDF_Eavg.SetPS( 3 );
			m_tex_LTC.SetPS( 4 );
			m_tex_MS_LTC.SetPS( 5 );

			// =========== Render scene ===========
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

			if ( !checkBoxShowDiff.Checked ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );
 				m_device.Clear( m_device.DefaultTarget, float4.Zero );
 				m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

				if ( checkBoxShowReference.Checked ) {
					// Use expensive reference
					if ( m_shader_RenderScene_Reference.Use() ) {
						m_device.RenderFullscreenQuad( m_shader_RenderScene_Reference );
					}
				} else {
					if ( m_shader_RenderScene.Use() ) {
						m_device.RenderFullscreenQuad( m_shader_RenderScene );
					}
				}
			} else {
				// Render reference in RT0
				m_device.SetRenderTarget( m_RT_temp0, m_device.DefaultDepthStencil );
 				m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );
				if ( m_shader_RenderScene_Reference.Use() ) {
					m_device.RenderFullscreenQuad( m_shader_RenderScene_Reference );
				}

				// Render LTC in RT1
				m_device.SetRenderTarget( m_RT_temp1, m_device.DefaultDepthStencil );
	 			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );
				if ( m_shader_RenderScene.Use() ) {
					m_device.RenderFullscreenQuad( m_shader_RenderScene );
				}

				// Render difference
				m_device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );
				if ( m_shader_RenderDiff.Use() ) {

					m_RT_temp0.SetPS( 0 );
					m_RT_temp1.SetPS( 1 );
					m_tex_FalseColors.SetPS( 2 );

					m_device.RenderFullscreenQuad( m_shader_RenderDiff );
				}
			}

			// =========== Render Light Disk ===========
			m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );
			if ( !checkBoxDebugMatrix.Checked ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

				if ( m_shader_RenderLight.Use() ) {
					m_prim_disk.Render( m_shader_RenderLight );
				}
			} else {
				if ( m_shader_RenderTestQuad.Use() ) {
					m_CB_TestQuad.m._wsLight2World.r0.Set( radiusX * axisX, 0 );
					m_CB_TestQuad.m._wsLight2World.r1.Set( radiusY * axisY, 0 );
					m_CB_TestQuad.m._wsLight2World.r2.Set( at, 0 );
					m_CB_TestQuad.m._wsLight2World.r3.Set( wsLightPosition, 1 );

					// Upload the full matrix, although we only really need the 4 non trivial coefficients at indices m11, m13, m31 and m33...
					int			roughnessIndex = (int) Mathf.Floor( 63.99f * Mathf.Sqrt( floatTrackbarControlRoughness.Value ) );
					int			thetaIndex = (int) Mathf.Floor( 63.99f * Mathf.Sqrt( 1.0f - Mathf.Cos( Mathf.ToRad( floatTrackbarControlViewAngle.Value ) ) ) );
					int			matrixIndex = roughnessIndex + 64 * thetaIndex;

					double[,]	LTC = radioButtonGGX.Checked ? LTCAreaLight.s_LtcMatrixData_GGX : LTCAreaLight.s_LtcMatrixData_OrenNayar;

					// NOTE: The LTC inverse matrices stored in the tables are transposed: columns are stored first
					// So in order to use them in the shaders, we need to compute P * M^-1^T instead of the paper's formulation M^-1 * P
					//
					m_CB_TestQuad.m._invM_transposed_r0.Set( (float) LTC[matrixIndex,0], (float) LTC[matrixIndex,1], (float) LTC[matrixIndex,2], 0 );
					m_CB_TestQuad.m._invM_transposed_r1.Set( (float) LTC[matrixIndex,3], (float) LTC[matrixIndex,4], (float) LTC[matrixIndex,5], 0 );
					m_CB_TestQuad.m._invM_transposed_r2.Set( (float) LTC[matrixIndex,6], (float) LTC[matrixIndex,7], (float) LTC[matrixIndex,8], 0 );

					m_CB_TestQuad.UpdateData();

					m_device.RenderFullscreenQuad( m_shader_RenderTestQuad );
				}
			}

			// Show!
			m_device.Present( false );
		}

		/// <summary>
		/// Computes the solid angle covered by the light as seen by provided position
		/// </summary>
		/// <param name="_wsPosition"></param>
		void	ComputeResult( float3 _wsPosition ) {
			
			float3	lsLightPos = m_CB_Light.m._wsLight2World[3].xyz - _wsPosition;
			float3	lsLightX = m_CB_Light.m._wsLight2World[0].w * m_CB_Light.m._wsLight2World[0].xyz;
			float3	lsLightY = m_CB_Light.m._wsLight2World[1].w * m_CB_Light.m._wsLight2World[1].xyz;

			float3	n = float3.UnitY;	// Plane normal is straight up

			float	dA = Mathf.TWOPI / 1024.0f;

			float	c0, s0, c1, s1;
			float	FdotN, dot;

			double	dTheta;
			float3	v0, v1, u0, u1, cross, dF;

			double	sum0 = 0;
			double	sum1 = 0;
			float3	sumF0 = float3.Zero;
			float3	sumF1 = float3.Zero;
			for ( int i=0; i < 1024; i++ ) {
				c0 = Mathf.Cos( i * dA );
				s0 = Mathf.Sin( i * dA );
				c1 = Mathf.Cos( i * dA + dA );
				s1 = Mathf.Sin( i * dA + dA );

				v0 = lsLightPos + c0 * lsLightX + s0 * lsLightY;
				v1 = lsLightPos + c1 * lsLightX + s1 * lsLightY;

				u0 = v0.NormalizedSafe;
				u1 = v1.NormalizedSafe;
				dot = u0.Dot( u1 );
				dTheta = Math.Acos( Mathf.Clamp( dot, -1, 1 ) );

				//////////////////////////////////////////////////////////////////////////
				// Regular way
				cross = v0.Cross( v1 );
				cross.NormalizeSafe();

				// Directly accumulate F
				dF = (float) dTheta * cross;
				sumF0 += dF;

				// Accumulate F.n each step of the way
//				FdotN = dF.Dot( n );
				FdotN = dF.y;
				sum0 += FdotN;


				//////////////////////////////////////////////////////////////////////////
				// Bent way
//				FdotN = Mathf.Sqrt( 1 - Mathf.Pow2( u0.Dot( n ) ) );	// sin( alpha )
				FdotN = Mathf.Sqrt( 1 - Mathf.Pow2( u0.y ) );			// sin( alpha )
				dF = (float) dTheta * FdotN * u0;
				sumF1 += dF;
				sum1 += (float) dTheta * FdotN;
			}

			textBoxResults.Text = "Target = " + _wsPosition + "\r\n"
								+ "\r\n"
								+ "Regular F = " + sumF0 + "\r\n"
								+ "Regular sum = " + sum0 + "\r\n"
								+ "Regular F.N = " + sumF0.Dot( n ) + "\r\n"
								+ "\r\n"
								+ "Ortho F = " + sumF1 + "\r\n"
								+ "Ortho sum = " + sum1 + "\r\n"
								+ "Ortho |F| = " + sumF1.Length + "  <== \r\n";
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
		}

		#region Frustum Matrix Transform Checks

		/// <summary>
		/// This code tests the determinant of the transform matrices that rectify a 
		/// </summary>
		void	CheckMatrix() {

			const int	COUNT_RADIUS = 10;
			const int	COUNT_X = 10;
			const int	COUNT_Y = 10;
			const int	COUNT_Z = 10;

			const float	Xmax = 10.0f;
			const float	Ymax = 10.0f;
			const float	Zmax = 4.0f;
			const float	Rb = 2.0f;

			float3	P0 = float3.Zero;
			float3	T = new float3( 1, 0, 0 );
			float3	B = new float3( 0, 1, 0 );
			float3	N = new float3( 0, 0, 1 );

//			float3	Nt, Nb, Tn, Bn, Dtn, Dbn;

//			float4		P  = float4.UnitW, Pt;
//			float4x4	M = new float4x4();

			float3		P, Pt;
			float3x3	M = new float3x3();
			float3x3	M2 = new float3x3();
			float3x3	invM = new float3x3();
			float3x3	invM2 = new float3x3();

			float	det, illuminance;
//			float	depth;

			List<float[,,]>	determinantss = new List<float[, , ]>( COUNT_RADIUS);

//			for ( int radiusIndex=0; radiusIndex < COUNT_RADIUS; radiusIndex++ )
int	radiusIndex = 10;
			{
				float[,,]	determinants = new float[COUNT_Z,1+2*COUNT_Y,1+2*COUNT_X];
				determinantss.Add( determinants );

				float	radiusFactor = Mathf.Max( 0.01f, 2.0f * radiusIndex / 100 );
				float	Rt = radiusFactor * Rb;

				T.Set( Rt, 0, 0 );
				B.Set( 0, Rb, 0 );

				for ( int Z=COUNT_Z; Z > 0; Z-- ) {
					P0.z = Zmax * Z / COUNT_Z;
					for ( int Y=-COUNT_Y; Y <= COUNT_Y; Y++ ) {
						P0.y = Ymax * Y / COUNT_Y;
						for ( int X=-COUNT_X; X <= COUNT_X; X++ ) {
							P0.x = Xmax * X / COUNT_X;

#if true
							// Matrix is a simple slanted parallelogram
							M.r0 = (T - (P0.Dot(T) / P0.Dot(N)) * N) / (Rt * Rt);
							M.r1 = (B - (P0.Dot(B) / P0.Dot(N)) * N) / (Rb * Rb);
							M.r2 = N / P0.Dot(N);

							invM = M.Inverse;

							det = M.Determinant;

							// Construct inverse directly
							invM2.r0.Set( T.x, B.x, P0.x );
							invM2.r1.Set( T.y, B.y, P0.y );
							invM2.r2.Set( T.z, B.z, P0.z );

							M2 = invM2.Inverse;

							// Test matrix is working
							P = P0;
							Pt = M * P;
							P = T;
							Pt = M * P;
							P = -T;
							Pt = M * P;
							P = B;
							Pt = M * P;
							P = -B;
							Pt = M * P;
							P = float3.Lerp( 0.5f * T - 0.25f * B, 0.5f * T - 0.25f * B + P0, 0.666f );
							Pt = M * P;

// 							invM *= M;
// 							invM2 *= M2;

							// Test lighting computation

							// Build rectangular area light corners in local space
							float3		lsAreaLightPosition = P0;
							float3[]	lsLightCorners = new float3[4];
										lsLightCorners[0] = lsAreaLightPosition + T + B;
										lsLightCorners[1] = lsAreaLightPosition + T - B;
										lsLightCorners[2] = lsAreaLightPosition - T - B;
										lsLightCorners[3] = lsAreaLightPosition - T + B;

							float3x3	world2TangentSpace = float3x3.Identity;	// Assume we're already in tangent space

							// Transform them into tangent-space
							float3[]	tsLightCorners = new float3[4];
							tsLightCorners[0] = lsLightCorners[0] * world2TangentSpace;
							tsLightCorners[1] = lsLightCorners[1] * world2TangentSpace;
							tsLightCorners[2] = lsLightCorners[2] * world2TangentSpace;
							tsLightCorners[3] = lsLightCorners[3] * world2TangentSpace;

							// Compute diffuse disk
							illuminance = DiskIrradiance( tsLightCorners );
#else
// Stupid version where I still thought we needed a perspective projection matrix!
							depth = P0.Dot(N);	// Altitude from plane
							Nt = P0.Dot(T) * N;
							Nb = P0.Dot(B) * N;
							Tn = P0.Dot(N) * T;	// = depth * T
							Bn = P0.Dot(N) * B;	// = depth * B

#if true
							M.r0.Set( (Tn - Nt) / Rt, 0 );
							M.r1.Set( (Bn - Nb) / Rb, 0 );
							M.r2.Set( 0, 0, 1, 0 );
							M.r3.Set( -N, depth );
#else
							Dtn = (Nt - Tn) / Rt;
							Dbn = (Nb - Bn) / Rb;

							M.r0.Set( Dtn, -P0.Dot( Dtn ) );
							M.r1.Set( Dbn, -P0.Dot( Dbn ) );
							M.r2.Set( N / depth, 0.0f );		// Will normalize Z to 1 if P is at same altitude as P0
							M.r3.Set( N, -P0.Dot( N ) );		// Here, use "depth"
#endif
							det = M.Determinant;

							determinants[Z-1,COUNT_Y+Y,COUNT_X+X] = det;

							// Test matrix is working
							P.Set( P0, 1 );
							Pt = M * P;
//							Pt /= Pt.w;	// This will NaN because W=0 in this particular case
							P.Set( Rt * T, 1 );
							Pt = M * P;
							Pt /= Pt.w;
							P.Set( -Rt * T, 1 );
							Pt = M * P;
							Pt /= Pt.w;
							P.Set( Rb * B, 1 );
							Pt = M * P;
							Pt /= Pt.w;
							P.Set( -Rb * B, 1 );
							Pt = M * P;
							Pt /= Pt.w;
							P.Set( float3.Lerp( 0.5f * Rt * T - 0.25f * Rb * B, P0, 0.666f ), 1 );
							Pt = M * P;
							Pt /= Pt.w;
#endif
						}
					}
				}


			}

		}


		//
		float	DiskIrradiance( float3[] _tsQuadVertices ) {

			// 1) Extract center position, tangent and bi-tangent axes
			float3	T = 0.5f * (_tsQuadVertices[1] - _tsQuadVertices[2]);
			float3	B = -0.5f * (_tsQuadVertices[3] - _tsQuadVertices[2]);
			float3	P0 = 0.5f * (_tsQuadVertices[0] + _tsQuadVertices[2]);		// Half-way through the diagonal

			float	sqRt = T.Dot( T );
			float	sqRb = B.Dot( B );

			float3	N = T.Cross( B ).Normalized;	// @TODO: Optimize! Do we need to normalize anyway?

			// 2) Build frustum matrices
				// M transform P' = M * P into canonical frustum space
			float3x3	M;
			float		invDepth = 1.0f / P0.Dot(N);
			M.r0 = (T - P0.Dot(T) * invDepth * N) / sqRt;
			M.r1 = (B - P0.Dot(B) * invDepth * N) / sqRb;
			M.r2 = N * invDepth;

				// M^-1 transforms P = M^-1 * P' back into original space
			float3x3	invM = new float3x3();
			invM.r0.Set( T.x, B.x, P0.x );
			invM.r1.Set( T.y, B.y, P0.y );
			invM.r2.Set( T.z, B.z, P0.z );

				// Compute the determinant of M^-1 that will help us scale the resulting vector
			float	det = invM.Determinant;
					det = Mathf.Sqrt( sqRt * sqRb ) * P0.Dot( N );

			// 3) Compute the exact integral in the canonical space
			// We know the expression of the orthogonal vector at any point on the unit circle as:
			//
			//							| cos(theta)
			//	Tau(theta) = 1/sqrt(2)  | sin(theta)
			//							| 1
			//
			// We move the orientation so we always compute 2 symetrical integrals on a half circle
			//	then we only need to compute the X component integral as the Y components have opposite
			//	sign and simply cancel each other
			//
			// The integral we need to compute is thus:
			//
			//	X = Integral[-maxTheta,maxTheta]{ cos(theta) dtheta }
			//	X = 2 * sin(maxTheta)
			//
			// The Z component is straightforward Z = Integral[-maxTheta,maxTheta]{ dtheta } = 2 maxTheta
			//
			float	cosMaxTheta = -1.0f;	// No clipping at the moment
			float	maxTheta = Mathf.Acos( Mathf.Clamp( cosMaxTheta, -1, 1 ) );	// @TODO: Optimize! Use fast acos or something...

			float3	F = new float3( 2 * Mathf.Sqrt( 1 - cosMaxTheta*cosMaxTheta ), 0, 2 * maxTheta ) / (Mathf.Sqrt( 2 ) * Mathf.TWOPI);

			float3	F2 = F / -det;
			return F2.Length;

//			// 4) Transform back into LTC space using M^-1
//			float3	F2 = invM * F;	// @TODO: Optimize => simply return length(F2) = determinant of M^-1
//
//			// 5) Estimate scalar irradiance
////			return -1.0f / det;
////			return Mathf.Abs(F2.z);
//			return F2.Length;
		}


		#endregion

		private void checkBoxDebugMatrix_CheckedChanged( object sender, EventArgs e ) {
			panelVisualizeLTCTransform.Enabled = checkBoxDebugMatrix.Checked;
		}

		#endregion
	}
}
