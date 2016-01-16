using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using RendererManaged;
using Nuaj.Cirrus.Utility;

namespace TestMSBSDF
{
	public partial class TestForm : Form
	{
		const int				HEIGHTFIELD_SIZE = 256;
		const int				MAX_SCATTERING_ORDER = 4;
		const int				LOBES_COUNT_THETA = 64;
		const int				LOBES_COUNT_PHI = 2 * LOBES_COUNT_THETA;

		static readonly double	SQRT2 = Math.Sqrt( 2.0 );

		private Device		m_Device = new Device();

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
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
		private struct CB_ComputeBeckmann {
			public float4		_Position_Size;
			public uint			_HeightFieldResolution;
			public uint			_SamplesCount;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RayTrace {
			public float3		_Direction;
			public float		_Roughness;
			public float2		_Offset;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Finalize {
			public uint			_IterationsCount;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct SB_Beckmann {
			public float		m_phase;
			public float		m_frequencyX;
			public float		m_frequencyY;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Render {
			public uint			_Flags;
			public uint			_ScatteringOrder;
			public uint			_IterationsCount;
		}

		private ConstantBuffer<CB_Main>				m_CB_Main = null;
		private ConstantBuffer<CB_Camera>			m_CB_Camera = null;
		private ConstantBuffer<CB_ComputeBeckmann>	m_CB_ComputeBeckmann = null;
		private ConstantBuffer<CB_RayTrace>			m_CB_RayTrace = null;
		private ConstantBuffer<CB_Finalize>			m_CB_Finalize = null;
		private ConstantBuffer<CB_Render>			m_CB_Render = null;

		private StructuredBuffer<SB_Beckmann>		m_SB_Beckmann;

		private ComputeShader		m_Shader_ComputeBeckmannSurface = null;
		private ComputeShader		m_Shader_RayTraceSurface = null;
		private ComputeShader		m_Shader_AccumulateOutgoingDirections = null;
		private ComputeShader		m_Shader_FinalizeOutgoingDirections = null;
		private Shader				m_Shader_RenderHeightField = null;

		private Texture2D			m_Tex_Random = null;
		private Texture2D			m_Tex_Heightfield = null;
		private Texture2D			m_Tex_OutgoingDirections = null;
		private Texture2D			m_Tex_LobeHistogram_Decimal = null;
		private Texture2D			m_Tex_LobeHistogram_Integer = null;
		private Texture2D			m_Tex_LobeHistogram = null;
		private int					m_lastComputedHistogramIterationsCount = 1;

		private Primitive			m_Prim_Heightfield = null;
		private Primitive			m_Prim_Lobe = null;

		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		public TestForm() {
			InitializeComponent();

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		#region Open/Close

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try {
				m_Device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_Device = null;
				MessageBox( "Failed to initialize DX device!\n\n" + _e.Message );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_ComputeBeckmann = new ConstantBuffer<CB_ComputeBeckmann>( m_Device, 10 );
			m_CB_RayTrace = new ConstantBuffer<CB_RayTrace>( m_Device, 10 );
			m_CB_Finalize = new ConstantBuffer<CB_Finalize>( m_Device, 10 );
			m_CB_Render = new ConstantBuffer<CB_Render>( m_Device, 10 );

			try {
				m_Shader_ComputeBeckmannSurface = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/ComputeBeckmannSurface.hlsl" ) ), "CS", null );
				m_Shader_RayTraceSurface = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/RayTraceSurface.hlsl" ) ), "CS", null );
				m_Shader_AccumulateOutgoingDirections = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/AccumulateOutgoingDirections.hlsl" ) ), "CS", null );
				m_Shader_FinalizeOutgoingDirections = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/AccumulateOutgoingDirections.hlsl" ) ), "CS_Finalize", null );
				m_Shader_RenderHeightField = new Shader( m_Device, new ShaderFile( new FileInfo( "Shaders/RenderHeightField.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox( "Shader \"RenderHeightField\" failed to compile!\n\n" + _e.Message );
				m_Shader_RenderHeightField = null;
			}

			BuildPrimHeightfield();
			BuildPrimLobe();

			m_SB_Beckmann = new StructuredBuffer<SB_Beckmann>( m_Device, 1024, true );

			BuildRandomTexture();
			BuildBeckmannSurfaceTexture( floatTrackbarControlBeckmannRoughness.Value );
			
			m_Tex_OutgoingDirections = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
			m_Tex_LobeHistogram_Decimal = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram_Integer = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 2 ), new float3( 0, 0, 0 ), float3.UnitY );

			// Perform a simple initial trace
			try {
				buttonRayTrace_Click( buttonRayTrace, EventArgs.Empty );
			} catch ( Exception ) {
				m_Device = null;
			}
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			m_Shader_RenderHeightField.Dispose();
			m_Shader_FinalizeOutgoingDirections.Dispose();
			m_Shader_AccumulateOutgoingDirections.Dispose();
			m_Shader_RayTraceSurface.Dispose();
			m_Shader_ComputeBeckmannSurface.Dispose();

			m_Tex_LobeHistogram_Decimal.Dispose();
			m_Tex_LobeHistogram_Integer.Dispose();
			m_Tex_OutgoingDirections.Dispose();
			m_Tex_Heightfield.Dispose();
			m_Tex_Random.Dispose();

			m_Prim_Lobe.Dispose();
			m_Prim_Heightfield.Dispose();

			m_SB_Beckmann.Dispose();

			m_CB_Render.Dispose();
			m_CB_Finalize.Dispose();
			m_CB_RayTrace.Dispose();
			m_CB_ComputeBeckmann.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		void	MessageBox( string _Text ) {
			MessageBox( _Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon ) {
			System.Windows.Forms.MessageBox.Show( _Text, "MS BSDF Test", _Buttons, _Icon );
		}

		#endregion

		#region Primitives

		void	BuildPrimHeightfield() {
			VertexP3[]	Vertices = new VertexP3[HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE];
			for ( uint Y=0; Y < HEIGHTFIELD_SIZE; Y++ ) {
				float	y = -1.0f + 2.0f * Y / (HEIGHTFIELD_SIZE-1);
				for ( uint X=0; X < HEIGHTFIELD_SIZE; X++ ) {
					float	x = -1.0f + 2.0f * X / (HEIGHTFIELD_SIZE-1);
					Vertices[256*Y+X].P.Set( x, y, 0.0f );
				}
			}

			List< uint >	Indices = new List< uint >();
			for ( uint Y=0; Y < HEIGHTFIELD_SIZE-1; Y++ ) {
				uint	IndexStart0 = HEIGHTFIELD_SIZE*Y;		// Start index of top band
				uint	IndexStart1 = HEIGHTFIELD_SIZE*(Y+1);	// Start index of bottom band
				for ( uint X=0; X < HEIGHTFIELD_SIZE; X++ ) {
					Indices.Add( IndexStart0++ );
					Indices.Add( IndexStart1++ );
				}
				if ( Y != HEIGHTFIELD_SIZE-1 ) {
					Indices.Add( IndexStart1-1 );				// Double current band's last index (first degenerate triangle => finish current band)
					Indices.Add( IndexStart0 );					// Double next band's first index (second degenerate triangle => start new band)
				}
			}


			m_Prim_Heightfield = new Primitive( m_Device, HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE, VertexP3.FromArray( Vertices ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
		}

		void	BuildPrimLobe() {
			VertexP3[]	Vertices = new VertexP3[LOBES_COUNT_PHI*LOBES_COUNT_THETA];
			for ( uint Y=0; Y < LOBES_COUNT_THETA; Y++ ) {
				float	theta = 2.0f * (float) Math.Asin( Math.Sqrt( 0.5 * Y / LOBES_COUNT_THETA ) );
				for ( uint X=0; X < LOBES_COUNT_PHI; X++ ) {
					float	phi = 2.0f * (float) Math.PI * X / LOBES_COUNT_PHI;

					Vertices[LOBES_COUNT_PHI*Y+X].P.Set( (float) (Math.Sin( theta ) * Math.Cos( phi )), (float) Math.Cos( theta ), -(float) (Math.Sin( theta ) * Math.Sin( phi )) );	// Phi=0 => +X, Phi=PI/2 => -Z in our Y-up frame
				}
			}

			List<uint>	Indices = new List<uint>();
			for ( uint Y=0; Y < LOBES_COUNT_THETA-1; Y++ ) {
				uint	IndexStart0 = LOBES_COUNT_PHI*Y;		// Start index of top band
				uint	IndexStart1 = LOBES_COUNT_PHI*(Y+1);	// Start index of bottom band
				for ( uint X=0; X < LOBES_COUNT_PHI; X++ ) {
					Indices.Add( IndexStart0++ );
					Indices.Add( IndexStart1++ );
				}
				if ( Y != LOBES_COUNT_THETA-1 ) {
					Indices.Add( IndexStart1-1 );			// Double current band's last index (first degenerate triangle => finish current band)
					Indices.Add( IndexStart0 );				// Double next band's first index (second degenerate triangle => start new band)
				}
			}

			m_Prim_Lobe = new Primitive( m_Device, HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE, VertexP3.FromArray( Vertices ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
		}

		#endregion

		double	GenerateNormalDistributionHeight() {
			double	U = WMath.SimpleRNG.GetUniform();	// Uniform distribution in ]0,1[
			double	errfinv = WMath.Functions.erfinv( 2.0 * U - 1.0 );
			double	h = SQRT2 * errfinv;
			return h;
		}

		/// <summary>
		/// Builds many random values
		/// </summary>
		void	BuildRandomTexture() {
			PixelsBuffer	Content = new PixelsBuffer( HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float4)) );

			WMath.SimpleRNG.SetSeed( 561321987, 132194982 );
			using ( BinaryWriter W = Content.OpenStreamWrite() )
				for ( int Y=0; Y < HEIGHTFIELD_SIZE; Y++ ) {
					for ( int X=0; X < HEIGHTFIELD_SIZE; X++ ) {
						W.Write( (float) WMath.SimpleRNG.GetUniform() );
						W.Write( (float) WMath.SimpleRNG.GetUniform() );
						W.Write( (float) WMath.SimpleRNG.GetUniform() );
						W.Write( (float) WMath.SimpleRNG.GetUniform() );
					}
				}
			Content.CloseStream();

			m_Tex_Random = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 1, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, false, new PixelsBuffer[] { Content } );
		}

		/// <summary>
		/// Builds a heightfield whose heights are distributed according to the following probability (a.k.a. the normal distribution with sigma=1 and µ=0):
		///		p(height) = exp( -0.5*height^2 ) / sqrt(2PI)
		///	
		///	From "2015 Heitz - Generating Procedural Beckmann Surfaces"
		/// </summary>
		/// <remarks>Only isotropic roughness is supported</remarks>
		void	BuildBeckmannSurfaceTexture( float _roughness ) {

			// Precompute stuff that resemble a lot to the Box-Muller algorithm to generate normal distribution random values
			WMath.SimpleRNG.SetSeed( 521288629, 362436069 );
			for ( int i=0; i < m_SB_Beckmann.m.Length; i++ ) {
				double	U0 = WMath.SimpleRNG.GetUniform();
				double	U1 = WMath.SimpleRNG.GetUniform();
				double	U2 = WMath.SimpleRNG.GetUniform();

				m_SB_Beckmann.m[i].m_phase = (float) (2.0 * Math.PI * U0);	// Phase

				double	theta = 2.0 * Math.PI * U1;
				double	radius = Math.Sqrt( -Math.Log( U2 ) );
				m_SB_Beckmann.m[i].m_frequencyX = (float) (radius * Math.Cos( theta ) * _roughness);	// Frequency in X direction
				m_SB_Beckmann.m[i].m_frequencyY = (float) (radius * Math.Sin( theta ) * _roughness);	// Frequency in Y direction
			}

			m_SB_Beckmann.Write();
			m_SB_Beckmann.SetInput( 0 );

			#if true
				if ( m_Tex_Heightfield == null )
					m_Tex_Heightfield = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 1, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );

				// Run the CS
				if ( m_Shader_ComputeBeckmannSurface.Use() ) {
					m_Tex_Heightfield.SetCSUAV( 0 );
					m_CB_ComputeBeckmann.m._Position_Size.Set( -128.0f, -128.0f, 256.0f, 256.0f );
					m_CB_ComputeBeckmann.m._HeightFieldResolution = HEIGHTFIELD_SIZE;
					m_CB_ComputeBeckmann.m._SamplesCount = (uint) m_SB_Beckmann.m.Length;
					m_CB_ComputeBeckmann.UpdateData();

					m_Shader_ComputeBeckmannSurface.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

					m_Tex_Heightfield.RemoveFromLastAssignedSlotUAV();
				}
			#else	// CPU version
				PixelsBuffer	Content = new PixelsBuffer( HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float4)) );

				double	scale = Math.Sqrt( 2.0 / N );

				// Generate heights
				float	range = 128.0f;
				float2	pos;
				float	height;
				float	minHeight = float.MaxValue, maxHeight = -float.MaxValue;
				double	accum;
				using ( BinaryWriter W = Content.OpenStreamWrite() ) {
					for ( int Y=0; Y < HEIGHTFIELD_SIZE; Y++ ) {
						pos.y = range * (2.0f * Y / (HEIGHTFIELD_SIZE-1) - 1.0f);
						for ( int X=0; X < HEIGHTFIELD_SIZE; X++ ) {
							pos.x = range * (2.0f * X / (HEIGHTFIELD_SIZE-1) - 1.0f);

	//						height = (float) WMath.SimpleRNG.GetNormal();
	//						height = (float) GenerateNormalDistributionHeight();

							accum = 0.0;
							for ( int i=0; i < N; i++ ) {
								accum += Math.Cos( m_phi[i] + pos.x * m_fx[i] + pos.y * m_fy[i] );
							}
							height = (float) (scale * accum);

							minHeight = Math.Min( minHeight, height );
							maxHeight = Math.Max( maxHeight, height );

							W.Write( height );
						}
					}
				}
				Content.CloseStream();

				m_Tex_Heightfield = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 1, 1, PIXEL_FORMAT.R32_FLOAT, false, false, new PixelsBuffer[] { Content } );
			#endif
		}

		/// <summary>
		/// Performs a ray-tracing of the surface
		/// Outputs resulting directions into a texture then performs a histogram
		/// </summary>
		/// <param name="_Roughness"></param>
		void	RayTraceSurface( float _roughness, float _theta, float _phi, int _iterationsCount ) {

			float	sinTheta = (float) Math.Sin( _theta );
			float	cosTheta = (float) Math.Cos( _theta );
			float	sinPhi = (float) Math.Sin( _phi );
			float	cosPhi = (float) Math.Cos( _phi );

			m_CB_RayTrace.m._Direction.Set( -sinTheta * cosPhi, -sinTheta * sinPhi, -cosTheta );	// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)
			m_CB_RayTrace.m._Roughness = _roughness;

			m_Tex_OutgoingDirections.RemoveFromLastAssignedSlots();
			m_Tex_LobeHistogram.RemoveFromLastAssignedSlots();
			m_lastComputedHistogramIterationsCount = _iterationsCount;

			m_Device.Clear( m_Tex_LobeHistogram_Decimal, float4.Zero );	// Clear counters
			m_Device.Clear( m_Tex_LobeHistogram_Integer, float4.Zero );

			WMath.Hammersley	pRNG = new WMath.Hammersley();
			double[,]			sequence = pRNG.BuildSequence( _iterationsCount, 2 );
			for ( int iterationIndex=0; iterationIndex < _iterationsCount; iterationIndex++ ) {
				// 1] Ray-trace surface
				if ( m_Shader_RayTraceSurface.Use() ) {
					// Update trace offset
					m_CB_RayTrace.m._Offset.Set( (float) sequence[iterationIndex,0], (float) sequence[iterationIndex,1] );
					m_CB_RayTrace.UpdateData();

					m_Device.Clear( m_Tex_OutgoingDirections, float4.Zero );	// Clear target directions and weights

					m_Tex_Heightfield.SetCS( 0 );
					m_Tex_Random.SetCS( 1 );
					m_Tex_OutgoingDirections.SetCSUAV( 0 );	// New target buffer where to accumulate

					m_Shader_RayTraceSurface.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

					m_Tex_OutgoingDirections.RemoveFromLastAssignedSlotUAV();
				}

				// 2] Accumulate into target histogram
				if ( m_Shader_AccumulateOutgoingDirections.Use() ) {
					m_Tex_OutgoingDirections.SetCS( 0 );
					m_Tex_LobeHistogram_Decimal.SetCSUAV( 0 );
					m_Tex_LobeHistogram_Integer.SetCSUAV( 1 );

					m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER );

 					m_Tex_LobeHistogram_Decimal.RemoveFromLastAssignedSlotUAV();
 					m_Tex_LobeHistogram_Integer.RemoveFromLastAssignedSlotUAV();
					m_Tex_OutgoingDirections.RemoveFromLastAssignedSlots();
				}
			}

			// 3] Finalize
			if ( m_Shader_FinalizeOutgoingDirections.Use() ) {
 				m_Tex_LobeHistogram_Decimal.SetCSUAV( 0 );
 				m_Tex_LobeHistogram_Integer.SetCSUAV( 1 );
				m_Tex_LobeHistogram.SetCSUAV( 2 );

				m_CB_Finalize.m._IterationsCount = (uint) _iterationsCount;
				m_CB_Finalize.UpdateData();

				m_Shader_FinalizeOutgoingDirections.Dispatch( (LOBES_COUNT_PHI + 15) >> 4, (LOBES_COUNT_THETA + 15) >> 4, MAX_SCATTERING_ORDER );

 				m_Tex_LobeHistogram_Decimal.RemoveFromLastAssignedSlotUAV();
 				m_Tex_LobeHistogram_Integer.RemoveFromLastAssignedSlotUAV();
				m_Tex_LobeHistogram.RemoveFromLastAssignedSlotUAV();
			}
		}

		bool	m_pauseRendering = false;
		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null || m_pauseRendering )
				return;

			// Setup global data
			m_CB_Main.UpdateData();

			m_Tex_Heightfield.Set( 0 );
			m_Tex_OutgoingDirections.SetPS( 1 );

			m_Tex_LobeHistogram.SetPS( 2 );

			// =========== Render scene ===========
			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			// Render heightfield
			if ( m_Shader_RenderHeightField != null && m_Shader_RenderHeightField.Use() ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

				m_CB_Render.m._Flags = (checkBoxShowNormals.Checked ? 1U : 0U) | (checkBoxShowOutgoingDirections.Checked ? 2U : 0U) | (checkBoxShowOutgoingDirectionsHistogram.Checked ? 4U : 0U);
				m_CB_Render.m._ScatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value;
				m_CB_Render.m._IterationsCount = (uint) m_lastComputedHistogramIterationsCount;
				m_CB_Render.UpdateData();

				m_Prim_Heightfield.Render( m_Shader_RenderHeightField );
			}

			// Show!
			m_Device.Present( false );
		}

		#region EVENT HANDLERS

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			m_Device.ReloadModifiedShaders();
		}

		private void floatTrackbarControlBeckmannRoughness_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue )
		{
			try {
				m_pauseRendering = true;
				BuildBeckmannSurfaceTexture( floatTrackbarControlBeckmannRoughness.Value );
			} catch ( Exception ) {
			} finally {
				m_pauseRendering = false;
			}
		}

		private void buttonRayTrace_Click( object sender, EventArgs e )
		{
			try {
				m_pauseRendering = true;
				RayTraceSurface( floatTrackbarControlBeckmannRoughness.Value, (float) Math.PI * floatTrackbarControlTheta.Value / 180.0f, (float) Math.PI * floatTrackbarControlPhi.Value / 180.0f, integerTrackbarControlIterationsCount.Value );
				m_pauseRendering = false;
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while ray-tracing the surface using " + integerTrackbarControlIterationsCount.Value + " iterations:\r\n" + _e.Message + "\r\n\r\nDisabling device..." );
				m_Device.Exit();
				m_Device = null;
			}
		}

		#endregion
	}
}
