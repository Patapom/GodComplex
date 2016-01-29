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
		const int				HEIGHTFIELD_SIZE = 512;						//  (must match HLSL declaration)
		const int				MAX_SCATTERING_ORDER = 4;
		const int				LOBES_COUNT_THETA = 128;					// (must match HLSL declaration)
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

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderLobe {
			public float3		_Direction;
			public float		_LobeIntensity;
			public float3		_ReflectedDirection;
			public uint			_ScatteringOrder;
			public uint			_Flags;
			public float		_Roughness;
			public float		_ScaleR;
			public float		_ScaleT;
			public float		_ScaleB;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderCylinder {
			public float3		_Direction;
			public float		_Length;
			public float		_Radius;
		}

		private ConstantBuffer<CB_Main>				m_CB_Main = null;
		private ConstantBuffer<CB_Camera>			m_CB_Camera = null;
		private ConstantBuffer<CB_ComputeBeckmann>	m_CB_ComputeBeckmann = null;
		private ConstantBuffer<CB_RayTrace>			m_CB_RayTrace = null;
		private ConstantBuffer<CB_Finalize>			m_CB_Finalize = null;
		private ConstantBuffer<CB_Render>			m_CB_Render = null;
		private ConstantBuffer<CB_RenderLobe>		m_CB_RenderLobe = null;
		private ConstantBuffer<CB_RenderCylinder>	m_CB_RenderCylinder = null;

		private StructuredBuffer<SB_Beckmann>		m_SB_Beckmann;

		private ComputeShader		m_Shader_ComputeBeckmannSurface = null;
		private ComputeShader		m_Shader_RayTraceSurface = null;
		private ComputeShader		m_Shader_AccumulateOutgoingDirections = null;
		private ComputeShader		m_Shader_FinalizeOutgoingDirections = null;
		private Shader				m_Shader_RenderHeightField = null;
		private Shader				m_Shader_RenderLobe = null;
		private Shader				m_Shader_RenderCylinder = null;

		private Texture2D			m_Tex_Random = null;
		private Texture2D			m_Tex_Heightfield = null;
		private Texture2D			m_Tex_OutgoingDirections = null;
		private Texture2D			m_Tex_LobeHistogram_Decimal = null;
		private Texture2D			m_Tex_LobeHistogram_Integer = null;
		private Texture2D			m_Tex_LobeHistogram = null;
		private Texture2D			m_Tex_LobeHistogram_CPU = null;

		private Primitive			m_Prim_Heightfield = null;
		private Primitive			m_Prim_Lobe = null;
		private Primitive			m_Prim_Cylinder = null;

		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		private float3				m_lastComputedDirection;
		private float				m_lastComputedRoughness;
		private int					m_lastComputedHistogramIterationsCount = 1;

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
			m_CB_RenderLobe = new ConstantBuffer<CB_RenderLobe>( m_Device, 10 );
			m_CB_RenderCylinder = new ConstantBuffer<CB_RenderCylinder>( m_Device, 10 );

			try {
				m_Shader_ComputeBeckmannSurface = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/ComputeBeckmannSurface.hlsl" ) ), "CS", null );
				m_Shader_RayTraceSurface = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/RayTraceSurface.hlsl" ) ), "CS", null );
				m_Shader_AccumulateOutgoingDirections = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/AccumulateOutgoingDirections.hlsl" ) ), "CS", null );
				m_Shader_FinalizeOutgoingDirections = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/AccumulateOutgoingDirections.hlsl" ) ), "CS_Finalize", null );
				m_Shader_RenderHeightField = new Shader( m_Device, new ShaderFile( new FileInfo( "Shaders/RenderHeightField.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
				m_Shader_RenderLobe = new Shader( m_Device, new ShaderFile( new FileInfo( "Shaders/RenderLobe.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
				m_Shader_RenderCylinder = new Shader( m_Device, new ShaderFile( new FileInfo( "Shaders/RenderCylinder.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox( "Shader \"RenderHeightField\" failed to compile!\n\n" + _e.Message );
			}

			BuildPrimHeightfield();
			BuildPrimLobe();
			BuildPrimCylinder();

			m_SB_Beckmann = new StructuredBuffer<SB_Beckmann>( m_Device, 1024, true );

			BuildRandomTexture();
			BuildBeckmannSurfaceTexture( floatTrackbarControlBeckmannRoughness.Value );
			
			m_Tex_OutgoingDirections = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
			m_Tex_LobeHistogram_Decimal = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram_Integer = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );
			m_Tex_LobeHistogram_CPU = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_FLOAT, true, false, null );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 0, 0 ), float3.UnitY );

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

			m_Shader_RenderCylinder.Dispose();
			m_Shader_RenderLobe.Dispose();
			m_Shader_RenderHeightField.Dispose();
			m_Shader_FinalizeOutgoingDirections.Dispose();
			m_Shader_AccumulateOutgoingDirections.Dispose();
			m_Shader_RayTraceSurface.Dispose();
			m_Shader_ComputeBeckmannSurface.Dispose();

			m_Tex_LobeHistogram_CPU.Dispose();
			m_Tex_LobeHistogram.Dispose();
			m_Tex_LobeHistogram_Decimal.Dispose();
			m_Tex_LobeHistogram_Integer.Dispose();
			m_Tex_OutgoingDirections.Dispose();
			m_Tex_Heightfield.Dispose();
			m_Tex_Random.Dispose();

			m_Prim_Lobe.Dispose();
			m_Prim_Heightfield.Dispose();

			m_SB_Beckmann.Dispose();

			m_CB_RenderCylinder.Dispose();
			m_CB_RenderLobe.Dispose();
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
					Vertices[HEIGHTFIELD_SIZE*Y+X].P.Set( x, y, 0.0f );
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
			const int	RESOLUTION_THETA = 4 * LOBES_COUNT_THETA;
			const int	RESOLUTION_PHI = 2 * RESOLUTION_THETA;

			#if true	// FULL SPHERE
				VertexP3[]	Vertices = new VertexP3[RESOLUTION_PHI*2*RESOLUTION_THETA];
				for ( uint Y=0; Y < 2*RESOLUTION_THETA; Y++ ) {
					float	theta = 2.0f * (float) Math.Asin( Math.Sqrt( (float) Y / (2*RESOLUTION_THETA-1) ) );
					for ( uint X=0; X < RESOLUTION_PHI; X++ ) {
						float	phi = 2.0f * (float) Math.PI * X / RESOLUTION_PHI;
						Vertices[RESOLUTION_PHI*Y+X].P.Set( (float) (Math.Sin( theta ) * Math.Cos( phi )), (float) Math.Cos( theta ), -(float) (Math.Sin( theta ) * Math.Sin( phi )) );	// Phi=0 => +X, Phi=PI/2 => -Z in our Y-up frame
					}
				}

				List<uint>	Indices = new List<uint>();
				for ( uint Y=0; Y < 2*RESOLUTION_THETA-1; Y++ ) {
					uint	IndexStart0 = RESOLUTION_PHI*Y;		// Start index of top band
					uint	IndexStart1 = RESOLUTION_PHI*(Y+1);	// Start index of bottom band
					for ( uint X=0; X < RESOLUTION_PHI; X++ ) {
						Indices.Add( IndexStart0++ );
						Indices.Add( IndexStart1++ );
					}
					Indices.Add( IndexStart0-RESOLUTION_PHI );	// Loop
					Indices.Add( IndexStart1-RESOLUTION_PHI );
					if ( Y != 2*RESOLUTION_THETA-1 ) {
						Indices.Add( IndexStart1-1 );			// Double current band's last index (first degenerate triangle => finish current band)
						Indices.Add( IndexStart0 );				// Double next band's first index (second degenerate triangle => start new band)
					}
				}
				m_Prim_Lobe = new Primitive( m_Device, RESOLUTION_PHI*2*RESOLUTION_THETA, VertexP3.FromArray( Vertices ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
			#else	// HEMISPHERE
				VertexP3[]	Vertices = new VertexP3[RESOLUTION_PHI*RESOLUTION_THETA];
				for ( uint Y=0; Y < RESOLUTION_THETA; Y++ ) {
//					float	theta = 2.0f * (float) Math.Asin( Math.Sqrt( 0.5 * Y / (RESOLUTION_THETA-1) ) );
					float	theta = 0.5f * (float) Math.PI * Y / (RESOLUTION_THETA-1);
					for ( uint X=0; X < RESOLUTION_PHI; X++ ) {
						float	phi = 2.0f * (float) Math.PI * X / RESOLUTION_PHI;

						Vertices[RESOLUTION_PHI*Y+X].P.Set( (float) (Math.Sin( theta ) * Math.Cos( phi )), (float) Math.Cos( theta ), -(float) (Math.Sin( theta ) * Math.Sin( phi )) );	// Phi=0 => +X, Phi=PI/2 => -Z in our Y-up frame
					}
				}

				List<uint>	Indices = new List<uint>();
				for ( uint Y=0; Y < RESOLUTION_THETA-1; Y++ ) {
					uint	IndexStart0 = RESOLUTION_PHI*Y;		// Start index of top band
					uint	IndexStart1 = RESOLUTION_PHI*(Y+1);	// Start index of bottom band
					for ( uint X=0; X < RESOLUTION_PHI; X++ ) {
						Indices.Add( IndexStart0++ );
						Indices.Add( IndexStart1++ );
					}
					Indices.Add( IndexStart0-RESOLUTION_PHI );	// Loop
					Indices.Add( IndexStart1-RESOLUTION_PHI );
					if ( Y != RESOLUTION_THETA-1 ) {
						Indices.Add( IndexStart1-1 );			// Double current band's last index (first degenerate triangle => finish current band)
						Indices.Add( IndexStart0 );				// Double next band's first index (second degenerate triangle => start new band)
					}
				}
				m_Prim_Lobe = new Primitive( m_Device, RESOLUTION_PHI*RESOLUTION_THETA, VertexP3.FromArray( Vertices ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
			#endif
		}

		void	BuildPrimCylinder() {
			const int	COUNT = 20;

			// Build 2 circles at Y=0 and Y=1
			VertexP3[]	Vertices = new VertexP3[2*COUNT];
			for ( uint Y=0; Y < 2; Y++ ) {
				for ( uint X=0; X < COUNT; X++ ) {
					float	phi = 2.0f * (float) Math.PI * X / COUNT;
					Vertices[COUNT*Y+X].P.Set( (float) Math.Cos( phi ), Y, (float) Math.Sin( phi ) );
				}
			}

			// Link the 2 circles with quads
			List<uint>	Indices = new List<uint>();
			for ( uint X=0; X < COUNT; X++ ) {
				uint	NX = (X+1) % COUNT;
				Indices.Add( X );
				Indices.Add( COUNT+X );
				Indices.Add( COUNT+NX );

				Indices.Add( X );
				Indices.Add( COUNT+NX );
				Indices.Add( NX );
			}

			// Cap the circles
			for ( uint X=1; X < COUNT-1; X++ ) {
				Indices.Add( 0 );
				Indices.Add( X );
				Indices.Add( X+1 );

				Indices.Add( COUNT+0 );
				Indices.Add( COUNT+X+1 );	// Indices reversed for bottom triangles, although we render in CULL_NONE so we don't really care...
				Indices.Add( COUNT+X );
			}

			m_Prim_Cylinder = new Primitive( m_Device, 2*COUNT, VertexP3.FromArray( Vertices ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3 );
		}

		#endregion

		double	GenerateNormalDistributionHeight() {
			double	U = WMath.SimpleRNG.GetUniform();	// Uniform distribution in ]0,1[
			double	errfinv = WMath.Functions.erfinv( 2.0 * U - 1.0 );
			double	h = SQRT2 * errfinv;
			return h;
		}

		/// <summary>
		/// Builds many random values into a texture
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
					float	size = floatTrackbarControlBeckmannSizeFactor.Value * HEIGHTFIELD_SIZE;
//					m_CB_ComputeBeckmann.m._Position_Size.Set( -128.0f, -128.0f, 256.0f, 256.0f );
					m_CB_ComputeBeckmann.m._Position_Size.Set( -0.5f * size, -0.5f * size, size, size );
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

			m_lastComputedDirection.Set( -sinTheta * cosPhi, -sinTheta * sinPhi, -cosTheta );	// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)
			m_lastComputedRoughness = _roughness;

			m_lastComputedHistogramIterationsCount = _iterationsCount;

			m_CB_RayTrace.m._Direction = m_lastComputedDirection;
			m_CB_RayTrace.m._Roughness = m_lastComputedRoughness;

			m_Tex_OutgoingDirections.RemoveFromLastAssignedSlots();
			m_Tex_LobeHistogram.RemoveFromLastAssignedSlots();

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

			// 4] Read back to CPU for fitting
			m_Tex_LobeHistogram_CPU.CopyFrom( m_Tex_LobeHistogram );
		}

		bool	m_pauseRendering = false;
		void	Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null || m_pauseRendering )
				return;

			// Setup global data
			m_CB_Main.UpdateData();

			m_Tex_Heightfield.Set( 0 );
			m_Tex_OutgoingDirections.SetPS( 1 );
			m_Tex_LobeHistogram.Set( 2 );

			// =========== Render scene ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			// Render heightfield
			if ( !radioButtonHideSurface.Checked && m_Shader_RenderHeightField.Use() ) {
				m_CB_Render.m._Flags = (checkBoxShowNormals.Checked ? 1U : 0U) | (checkBoxShowOutgoingDirections.Checked ? 2U : 0U) | (checkBoxShowOutgoingDirectionsHistogram.Checked ? 4U : 0U);
				m_CB_Render.m._ScatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value - 1;
				m_CB_Render.m._IterationsCount = (uint) m_lastComputedHistogramIterationsCount;
				m_CB_Render.UpdateData();

				m_Prim_Heightfield.Render( m_Shader_RenderHeightField );
			}

			// Render lobes
			if ( checkBoxShowLobe.Checked || checkBoxShowAnalyticalLobe.Checked ) {
				// Compute reflected direction to orient the lobe against
//				float3	reflectedDirection = m_lastComputedDirection;

				float	theta = (float) Math.PI * floatTrackbarControlAnalyticalLobeTheta.Value / 180.0f;
				float	phi = (float) Math.PI * floatTrackbarControlPhi.Value / 180.0f;
				float	sinTheta = (float) Math.Sin( theta );
				float	cosTheta = (float) Math.Cos( theta );
				float	sinPhi = (float) Math.Sin( phi );
				float	cosPhi = (float) Math.Cos( phi );

				float3	reflectedDirection = new float3( -sinTheta * cosPhi, -sinTheta * sinPhi, -cosTheta );	// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)

						reflectedDirection.z = -reflectedDirection.z;	// Mirror against surface

				m_CB_RenderLobe.m._Direction = m_lastComputedDirection;
				m_CB_RenderLobe.m._LobeIntensity = floatTrackbarControlLobeIntensity.Value;
				m_CB_RenderLobe.m._ReflectedDirection = reflectedDirection;
				m_CB_RenderLobe.m._ScatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value - 1;
				m_CB_RenderLobe.m._Flags = checkBoxShowAnalyticalLobe.Checked ? 2U : 0U;
				m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness.Value;
				m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR.Value;
				m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT.Value;
				m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB.Value;

				if ( m_Shader_RenderLobe.Use() ) {
					// Flags for analytical lobe rendering
					uint	flags = 0U;
					if ( radioButtonAnalyticalBeckmann.Checked ) flags = 00U;
					else if ( radioButtonAnalyticalGGX.Checked ) flags = 01U;
					else if ( radioButtonAnalyticalPhong.Checked ) flags = 02U;
//					else if ( radioButtonAnalyticalPhong.Checked ) flags = 03U;	// Other

					if ( checkBoxShowLobe.Checked ) {
						// Show simulated lobe
						m_CB_RenderLobe.m._Flags = 0U;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );
					}

					if ( checkBoxShowAnalyticalLobe.Checked ) {
						// Show analytical lobe
						m_CB_RenderLobe.m._Flags = 2U | (flags << 2);	// Analytical
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );
					}

					if ( checkBoxShowWireframe.Checked ) {
						m_Device.SetRenderStates( RASTERIZER_STATE.WIREFRAME, DEPTHSTENCIL_STATE.READ_DEPTH_LESS_EQUAL, BLEND_STATE.NOCHANGE );

						if ( checkBoxShowLobe.Checked ) {
							m_CB_RenderLobe.m._Flags = 1U;	// Wireframe mode
							m_CB_RenderLobe.UpdateData();

							m_Prim_Lobe.Render( m_Shader_RenderLobe );
						}
						if ( checkBoxShowAnalyticalLobe.Checked ) {
							m_CB_RenderLobe.m._Flags = 1U | 2U | (flags << 2);	// Analytical
							m_CB_RenderLobe.UpdateData();

							m_Prim_Lobe.Render( m_Shader_RenderLobe );
						}
					}
				}

				// Render cylinder
				if ( m_Shader_RenderCylinder.Use() ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.NOCHANGE );

					m_CB_RenderCylinder.m._Direction = m_lastComputedDirection;
					m_CB_RenderCylinder.m._Length = 10.0f;
					m_CB_RenderCylinder.m._Radius = 0.025f;
					m_CB_RenderCylinder.UpdateData();

					m_Prim_Cylinder.Render( m_Shader_RenderCylinder );
				}
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

			panelOutput.Invalidate();
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

		#region Lobe Fitting

		class	LobeModel : WMath.BFGS.Model {

			TestForm	m_owner = null;
			float3		m_direction;
			double		m_cosPhi;
			double		m_sinPhi;
			double[,]	m_histogramData;

			int			m_iterationsCount = 0;

			public LobeModel( TestForm _owner ) {
				m_owner = _owner;
			}

			public void		Init( float3 _direction, double _theta, double _roughness, double _scaleT, double _scaleB, double _scaleN, Texture2D _texHistogram_CPU, int _scatteringOrder ) {
				_scatteringOrder--;	// Because scattering order 1 is actually stored in first slice of the texture array

				// 1] Readback lobe texture data into an array
				int	W = _texHistogram_CPU.Width;
				int	H = _texHistogram_CPU.Height;
				m_histogramData = new double[W,H];
				PixelsBuffer	Content = _texHistogram_CPU.Map( 0, _scatteringOrder );
				using ( BinaryReader R = Content.OpenStreamRead() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							m_histogramData[X,Y] = W * H * R.ReadSingle();
				Content.CloseStream();
				_texHistogram_CPU.UnMap( 0, _scatteringOrder );

				// 2] Setup initial parameters
				m_direction = _direction;
				m_direction.z = -m_direction.z;	// Mirror against surface to obtain reflected direction

				double	phi = Math.Atan2( m_direction.y, m_direction.x );
				m_cosPhi = Math.Cos( phi );
				m_sinPhi = Math.Sin( phi );

				Parameters = new double[] { _theta, _roughness, _scaleN, _scaleT, _scaleB };
//Parameters = new double[] { 39.5 * Math.PI / 180, 0.9646, 0.2222, 0.2384, 0.7758 };
			}

			#region Model Implementation

			double[]		m_parameters = new double[5];

			public override double[]		Parameters {
				get { return m_parameters; }
				set {
					m_parameters = value;

					// Update track bar parameters
					m_owner.floatTrackbarControlAnalyticalLobeTheta.Value = (float) (180.0 * m_parameters[0] / Math.PI);
					m_owner.floatTrackbarControlAnalyticalLobeRoughness.Value = (float) m_parameters[1];
					m_owner.floatTrackbarControlLobeScaleR.Value = (float) m_parameters[2];
					m_owner.floatTrackbarControlLobeScaleT.Value = (float) m_parameters[3];
					m_owner.floatTrackbarControlLobeScaleB.Value = (float) m_parameters[4];

					// Repaint every N iterations
					m_iterationsCount++;
//					if ( m_iterationsCount == 5 ) {
						m_iterationsCount = 0;
						m_owner.panelOutput.Refresh();
//					}
				}
			}

			public override double Eval( double[] _newParameters ) {

				double	lobeTheta = Math.Max( 0.0, Math.Min( 0.4999 * Math.PI, _newParameters[0] ) );
				double	lobeRoughness = Math.Max( 0.0, Math.Min( 1.0, _newParameters[1] ) );
				double	lobeScaleN = Math.Max( 1e-6, _newParameters[2] );
				double	lobeScaleT = Math.Max( 1e-6, _newParameters[3] );
				double	lobeScaleB = Math.Max( 1e-6, _newParameters[4] );

				double	invLobeScaleN = 1.0 / lobeScaleN;
				double	invLobeScaleT = 1.0 / lobeScaleT;
				double	invLobeScaleB = 1.0 / lobeScaleB;

				// Compute constant masking term due to incoming direction
 				double	maskingIncoming = PhongG1( m_direction.z, lobeRoughness );		// Masking( incoming )

				// Compute lobe's reflection vector and tangent space using new parameters
				double	cosTheta = Math.Cos( lobeTheta );
				double	sinTheta = Math.Sin( lobeTheta );

				float3	lobe_normal = new float3( (float) (sinTheta * m_cosPhi), (float) (sinTheta * m_sinPhi), (float) cosTheta );
// 				float3	lobe_tangent = lobe_normal.z < 0.9999f ? lobe_normal.Cross( float3.UnitZ ).Normalized : float3.UnitX;
// 				float3	lobe_biTangent = lobe_normal.Cross( lobe_tangent );
				float3	lobe_tangent = new float3( (float) -m_sinPhi, (float) m_cosPhi, 0.0f );	// Always lying in the X^Y plane
				float3	lobe_biTangent = lobe_normal.Cross( lobe_tangent );

				// Compute sum
				double	phi, theta, cosPhi, sinPhi;
				double	simulatedIntensity, simulatedIntensity_scaled, invSimulatedIntensity_scaled, lobeIntensity;
				float3	microfacet_direction = float3.Zero;
				float3	lobe_direction = float3.Zero;
				double	lobeT, lobeB, lobeN;
				double	maskingOutGoing = 0.0;

				double	sum = 0.0;
				for ( int Y=0; Y < LOBES_COUNT_THETA; Y++ ) {

					// Y = theta bin index = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) )
					// We need theta:
					theta = 2.0 * Math.Asin( Math.Sqrt( 0.5 * Y / LOBES_COUNT_THETA ) );
					cosTheta = Math.Cos( theta );
					sinTheta = Math.Sin( theta );

					for ( int X=0; X < LOBES_COUNT_PHI; X++ ) {

						// X = phi bin index = LOBES_COUNT_PHI * X / (2PI)
						// We need phi:
						phi = 2.0 * Math.PI * X / LOBES_COUNT_PHI;
						cosPhi = Math.Cos( phi );
						sinPhi = Math.Sin( phi );

						// Build simulated microfacet reflection direction in macro-surface space
						simulatedIntensity = m_histogramData[X,Y];
						microfacet_direction.Set( (float) (cosPhi * sinTheta), (float) (sinPhi * sinTheta), (float) cosTheta );

						// Compute maksing term due to outgoing direction
						maskingOutGoing = PhongG1( microfacet_direction.z, lobeRoughness );		// Masking( outgoing )

						// Transform direction into local lobe space
						// In the pixel shader that displays our lobes, the lobe's vertex position/direction is computed like this:
						//
						//	float3	worldLobeDirection = _ScaleT * localLobeDirection.x * tangent + _ScaleB * localLobeDirection.y * biTangent + _ScaleR * localLobeDirection.z * wsReflectedDirection;
						//
						// Here, we have the microfacet direction that we need to express into local lobe reference frame so we apply:
						//
						lobeT = simulatedIntensity * invLobeScaleT * microfacet_direction.Dot( lobe_tangent );
						lobeB = simulatedIntensity * invLobeScaleB * microfacet_direction.Dot( lobe_biTangent );
						lobeN = simulatedIntensity * invLobeScaleN * microfacet_direction.Dot( lobe_normal );

						simulatedIntensity_scaled = Math.Sqrt( lobeT*lobeT + lobeB*lobeB + lobeN*lobeN );
						invSimulatedIntensity_scaled = simulatedIntensity_scaled > 1e-12 ? 1.0 / simulatedIntensity_scaled : 0.0;
						lobe_direction = new float3( (float) (invSimulatedIntensity_scaled * lobeT), (float) (invSimulatedIntensity_scaled * lobeB), (float) (invSimulatedIntensity_scaled * lobeN) );

						// Compute the lobe intensity in local space
						float	cosTheta_M = Math.Max( 0.0f, lobe_direction.z );// Angle with pseudo normal (actually, the bent reflection direction)
 						lobeIntensity = PhongNDF( cosTheta_M, lobeRoughness );	// NDF
 						lobeIntensity *= maskingIncoming * maskingOutGoing;		// * Masking terms

						// Sum the difference between scaled simulated intensity and local lobe intensity
						sum += (simulatedIntensity_scaled - lobeIntensity) * (simulatedIntensity_scaled - lobeIntensity);
					}
				}
				sum /= LOBES_COUNT_THETA * LOBES_COUNT_PHI;

				return sum;
			}

			public override void Constrain( double[] _Parameters ) {
				_Parameters[0] = Math.Max( 0.0, Math.Min( 0.4999 * Math.PI, _Parameters[0] ) );
				_Parameters[1] = Math.Max( 0.0, Math.Min( 1.0, _Parameters[1] ) );
				_Parameters[2] = Math.Max( 1e-6, _Parameters[2] );
				_Parameters[3] = Math.Max( 1e-6, _Parameters[3] );
				_Parameters[4] = Math.Max( 1e-6, _Parameters[4] );
			}

			#endregion


			double	Roughness2PhongExponent( double _roughness ) {
				return Math.Pow( 2.0, 10.0 * (1.0 - _roughness) + 1.0 );	// From https://seblagarde.wordpress.com/2011/08/17/hello-world/
			}

			// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
			// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
			double	PhongNDF( double _cosTheta_M, double _roughness ) {
				double	n = Roughness2PhongExponent( _roughness );
				return (n+2)*Math.Pow( _cosTheta_M, n ) / Math.PI;
			}

			// Same as Beckmann but with a modified a bit
			double	PhongG1( double _cosTheta_V, double _roughness ) {
				double	n = Roughness2PhongExponent( _roughness );
				double	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
				double	tanThetaV = Math.Sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
				double	a = Math.Sqrt( 1.0 + 0.5 * n) / tanThetaV;
				return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
			}
		}

		LobeModel	m_lobeModel = null;
		WMath.BFGS	m_Fitter = new WMath.BFGS();

		void	PerformLobeFitting( float3 _direction, float _theta, float _roughness, float _scaleT, float _scaleB, float _scaleN, int _scatteringOrder ) {

			checkBoxShowAnalyticalLobe.Checked = true;

			m_lobeModel = new LobeModel( this );
			m_lobeModel.Init( _direction, _roughness, _theta, _scaleT, _scaleB, _scaleN,  m_Tex_LobeHistogram_CPU, _scatteringOrder );

			m_Fitter.Minimize( m_lobeModel );

			panelOutput.Invalidate();
		}

		#endregion

		private void buttonFit_Click( object sender, EventArgs e )
		{
			try {
				PerformLobeFitting( m_lastComputedDirection, floatTrackbarControlAnalyticalLobeTheta.Value * (float) Math.PI / 180.0f, floatTrackbarControlAnalyticalLobeRoughness.Value, floatTrackbarControlLobeScaleT.Value, floatTrackbarControlLobeScaleB.Value, floatTrackbarControlLobeScaleR.Value, integerTrackbarControlScatteringOrder.Value );

				MessageBox( "Fitting succeeded after " + m_Fitter.IterationsCount + " iterations.\r\nReached minimum: " + m_Fitter.FunctionMinimum, MessageBoxButtons.OK, MessageBoxIcon.Information );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while performing lobe fitting:\r\n" + _e.Message );
			} finally {
			}
		}
	}
}
