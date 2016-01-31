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
			public float		_Albedo;
			public float		_IOR;
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
			public float3		_Color;
			public float		_Radius;
		}
		
		enum SURFACE_TYPE {
			CONDUCTOR,
			DIELECTRIC,
			DIFFUSE,
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
		private ComputeShader		m_Shader_RayTraceSurface_Conductor = null;
		private ComputeShader		m_Shader_RayTraceSurface_Dielectric = null;
		private ComputeShader		m_Shader_AccumulateOutgoingDirections = null;
		private ComputeShader		m_Shader_FinalizeOutgoingDirections = null;
		private Shader				m_Shader_RenderHeightField = null;
		private Shader				m_Shader_RenderLobe = null;
		private Shader				m_Shader_RenderCylinder = null;

		private Texture2D			m_Tex_Random = null;
		private Texture2D			m_Tex_Heightfield = null;
		private Texture2D			m_Tex_OutgoingDirections_Reflected = null;
		private Texture2D			m_Tex_OutgoingDirections_Transmitted = null;
		private Texture2D			m_Tex_LobeHistogram_Reflected_Decimal = null;
		private Texture2D			m_Tex_LobeHistogram_Reflected_Integer = null;
		private Texture2D			m_Tex_LobeHistogram_Transmitted_Decimal = null;
		private Texture2D			m_Tex_LobeHistogram_Transmitted_Integer = null;
		private Texture2D			m_Tex_LobeHistogram_Reflected = null;
		private Texture2D			m_Tex_LobeHistogram_Transmitted = null;
		private Texture2D			m_Tex_LobeHistogram_CPU = null;

		private Primitive			m_Prim_Heightfield = null;
		private Primitive			m_Prim_Lobe = null;
		private Primitive			m_Prim_Cylinder = null;

		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		private float3				m_lastComputedDirection;
		private float				m_lastComputedRoughness;
		private float				m_lastComputedAlbedo;
		private float				m_lastComputedIOR;
		private SURFACE_TYPE		m_lastComputedSurfaceType;
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
				m_Shader_RayTraceSurface_Conductor = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/RayTraceSurface.hlsl" ) ), "CS_Conductor", null );
				m_Shader_RayTraceSurface_Dielectric = new ComputeShader( m_Device, new ShaderFile( new FileInfo( "Shaders/RayTraceSurface.hlsl" ) ), "CS_Dielectric", null );
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
			
			m_Tex_OutgoingDirections_Reflected = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
			m_Tex_OutgoingDirections_Transmitted = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
			m_Tex_LobeHistogram_Reflected_Decimal = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram_Reflected_Integer = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram_Transmitted_Decimal = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram_Transmitted_Integer = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
			m_Tex_LobeHistogram_Reflected = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );
			m_Tex_LobeHistogram_Transmitted = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );
			m_Tex_LobeHistogram_CPU = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER, 1, PIXEL_FORMAT.R32_FLOAT, true, false, null );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 3, 6 ), new float3( 0, 2, 0 ), float3.UnitY );

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
			m_Shader_RayTraceSurface_Dielectric.Dispose();
			m_Shader_RayTraceSurface_Conductor.Dispose();
			m_Shader_ComputeBeckmannSurface.Dispose();

			m_Tex_LobeHistogram_CPU.Dispose();
			m_Tex_LobeHistogram_Transmitted.Dispose();
			m_Tex_LobeHistogram_Reflected.Dispose();
			m_Tex_LobeHistogram_Transmitted_Decimal.Dispose();
			m_Tex_LobeHistogram_Transmitted_Integer.Dispose();
			m_Tex_LobeHistogram_Reflected_Decimal.Dispose();
			m_Tex_LobeHistogram_Reflected_Integer.Dispose();
			m_Tex_OutgoingDirections_Transmitted.Dispose();
			m_Tex_OutgoingDirections_Reflected.Dispose();
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
//					float	theta = 2.0f * (float) Math.Asin( Math.Sqrt( (float) Y / (2*RESOLUTION_THETA-1) ) );
					float	theta = (float) Math.PI * (float) Y / (2*RESOLUTION_THETA-1);
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
			PixelsBuffer[]	Content = new PixelsBuffer[4];

			WMath.SimpleRNG.SetSeed( 561321987, 132194982 );
			for ( int arrayIndex=0; arrayIndex < 4; arrayIndex++ ) {
				Content[arrayIndex] = new PixelsBuffer( HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE*4*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float4)) );
				using ( BinaryWriter W = Content[arrayIndex].OpenStreamWrite() )
					for ( int Y=0; Y < HEIGHTFIELD_SIZE; Y++ ) {
						for ( int X=0; X < HEIGHTFIELD_SIZE; X++ ) {
							W.Write( (float) WMath.SimpleRNG.GetUniform() );
							W.Write( (float) WMath.SimpleRNG.GetUniform() );
							W.Write( (float) WMath.SimpleRNG.GetUniform() );
							W.Write( (float) WMath.SimpleRNG.GetUniform() );
						}
					}
				Content[arrayIndex].CloseStream();
			}

			m_Tex_Random = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 4, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, false, Content );
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
				PixelsBuffer	Content = new PixelsBuffer( HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)) );

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

		// Assuming n1=1 (air) we get:
		//	F0 = ((n2 - n1) / (n2 + n1))²
		//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
		//
		float	Fresnel_IORFromF0( float _F0 ) {
			double	SqrtF0 = Math.Sqrt( _F0 );
			return (float) ((1.0 + SqrtF0) / (1.00000001 - SqrtF0));
		}

		// From Walter 2007
		// Expects _incoming pointing AWAY from the surface
		// eta = IOR_over / IOR_under
		//
		static float3	Refract( float3 _incoming, float3 _normal, float _eta ) {
			float	c = _incoming.Dot( _normal );
			float	k = (float) (_eta * c - Math.Sign(c) * Math.Sqrt( 1.0 + _eta * (c*c - 1.0)));
			return  k * _normal - _eta * _incoming;
		}

		/// <summary>
		/// Performs a ray-tracing of the surface
		/// Outputs resulting directions into a texture then performs a histogram
		/// 
		/// The surface is assigned a beam of photons, one for each texel of the heightfield texture
		/// At each iteration, the whole beam is offset a little using a Hammersley sequence that guarantees
		///  we end up ray-tracing the entire surface finely (hopefully, the full surface can be traced using enough terationsi)
		/// </summary>
		/// <param name="_roughness">Surface roughness</param>
		/// <param name="_albedo">Surface albedo</param>
		/// <param name="_F0">Surface Fresnel reflectance at 0°</param>
		/// <param name="_surfaceType">Type of surface we're simulating</param>
		/// <param name="_theta">Vertical angle of incidence</param>
		/// <param name="_phi">Azimuthal angle of incidence</param>
		/// <param name="_iterationsCount">Amount of iterations of beam tracing</param>
		void	RayTraceSurface( float _roughness, float _albedo, float _F0, SURFACE_TYPE _surfaceType, float _theta, float _phi, int _iterationsCount ) {

			float	sinTheta = (float) Math.Sin( _theta );
			float	cosTheta = (float) Math.Cos( _theta );
			float	sinPhi = (float) Math.Sin( _phi );
			float	cosPhi = (float) Math.Cos( _phi );

			m_lastComputedDirection.Set( -sinTheta * cosPhi, -sinTheta * sinPhi, -cosTheta );	// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)
			m_lastComputedRoughness = _roughness;
			m_lastComputedAlbedo = _albedo;
			m_lastComputedIOR = Fresnel_IORFromF0( _F0 );
			m_lastComputedSurfaceType = _surfaceType;


			m_lastComputedHistogramIterationsCount = _iterationsCount;

			m_CB_RayTrace.m._Direction = m_lastComputedDirection;
			m_CB_RayTrace.m._Roughness = m_lastComputedRoughness;
			m_CB_RayTrace.m._Albedo = m_lastComputedAlbedo;
			m_CB_RayTrace.m._IOR = m_lastComputedIOR;

			m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlots();
			m_Tex_OutgoingDirections_Transmitted.RemoveFromLastAssignedSlots();
			m_Tex_LobeHistogram_Reflected.RemoveFromLastAssignedSlots();
			m_Tex_LobeHistogram_Transmitted.RemoveFromLastAssignedSlots();

			m_Device.Clear( m_Tex_LobeHistogram_Reflected_Decimal, float4.Zero );	// Clear counters
			m_Device.Clear( m_Tex_LobeHistogram_Reflected_Integer, float4.Zero );
			m_Device.Clear( m_Tex_LobeHistogram_Transmitted_Decimal, float4.Zero );	// Clear counters
			m_Device.Clear( m_Tex_LobeHistogram_Transmitted_Integer, float4.Zero );

			WMath.Hammersley	pRNG = new WMath.Hammersley();
			double[,]			sequence = pRNG.BuildSequence( _iterationsCount, 2 );
			for ( int iterationIndex=0; iterationIndex < _iterationsCount; iterationIndex++ ) {
				// 1] Ray-trace surface
				switch ( m_lastComputedSurfaceType ) {
					case SURFACE_TYPE.CONDUCTOR:
						if ( m_Shader_RayTraceSurface_Conductor.Use() ) {
							// Update trace offset
							m_CB_RayTrace.m._Offset.Set( (float) sequence[iterationIndex,0], (float) sequence[iterationIndex,1] );
							m_CB_RayTrace.UpdateData();

							m_Device.Clear( m_Tex_OutgoingDirections_Reflected, float4.Zero );	// Clear target directions and weights

							m_Tex_Heightfield.SetCS( 0 );
							m_Tex_Random.SetCS( 1 );
							m_Tex_OutgoingDirections_Reflected.SetCSUAV( 0 );	// New target buffer where to accumulate

							m_Shader_RayTraceSurface_Conductor.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlotUAV();
						}

						// 2] Accumulate into target histogram
						if ( m_Shader_AccumulateOutgoingDirections.Use() ) {
							m_Tex_OutgoingDirections_Reflected.SetCS( 0 );
							m_Tex_LobeHistogram_Reflected_Decimal.SetCSUAV( 0 );
							m_Tex_LobeHistogram_Reflected_Integer.SetCSUAV( 1 );

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER );

 							m_Tex_LobeHistogram_Reflected_Decimal.RemoveFromLastAssignedSlotUAV();
 							m_Tex_LobeHistogram_Reflected_Integer.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlots();
						}
						break;

					case SURFACE_TYPE.DIELECTRIC:
						if ( m_Shader_RayTraceSurface_Dielectric.Use() ) {
							// Update trace offset
							m_CB_RayTrace.m._Offset.Set( (float) sequence[iterationIndex,0], (float) sequence[iterationIndex,1] );
							m_CB_RayTrace.UpdateData();

							m_Device.Clear( m_Tex_OutgoingDirections_Reflected, float4.Zero );		// Clear target directions and weights
							m_Device.Clear( m_Tex_OutgoingDirections_Transmitted, float4.Zero );	// Clear target directions and weights

							m_Tex_Heightfield.SetCS( 0 );
							m_Tex_Random.SetCS( 1 );
							m_Tex_OutgoingDirections_Reflected.SetCSUAV( 0 );	// New target buffer where to accumulate
							m_Tex_OutgoingDirections_Transmitted.SetCSUAV( 1 );	// New target buffer where to accumulate

							m_Shader_RayTraceSurface_Dielectric.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Transmitted.RemoveFromLastAssignedSlotUAV();
						}

						// 2] Accumulate into target histogram
						if ( m_Shader_AccumulateOutgoingDirections.Use() ) {
							// Accumulated reflections
							m_Tex_OutgoingDirections_Reflected.SetCS( 0 );
							m_Tex_LobeHistogram_Reflected_Decimal.SetCSUAV( 0 );
							m_Tex_LobeHistogram_Reflected_Integer.SetCSUAV( 1 );

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER );

 							m_Tex_LobeHistogram_Reflected_Decimal.RemoveFromLastAssignedSlotUAV();
 							m_Tex_LobeHistogram_Reflected_Integer.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlots();

							// Accumulated transmissions
							m_Tex_OutgoingDirections_Transmitted.SetCS( 0 );
							m_Tex_LobeHistogram_Transmitted_Decimal.SetCSUAV( 0 );
							m_Tex_LobeHistogram_Transmitted_Integer.SetCSUAV( 1 );

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER );

 							m_Tex_LobeHistogram_Transmitted_Decimal.RemoveFromLastAssignedSlotUAV();
 							m_Tex_LobeHistogram_Transmitted_Integer.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Transmitted.RemoveFromLastAssignedSlots();
						}
						break;

					default:
						throw new Exception( "Not implemented!" );
				}
			}

			// 3] Finalize
			if ( m_Shader_FinalizeOutgoingDirections.Use() ) {
				m_Tex_LobeHistogram_Reflected_Decimal.SetCSUAV( 0 );
 				m_Tex_LobeHistogram_Reflected_Integer.SetCSUAV( 1 );
				m_Tex_LobeHistogram_Reflected.SetCSUAV( 2 );

				m_CB_Finalize.m._IterationsCount = (uint) _iterationsCount;
				m_CB_Finalize.UpdateData();

				m_Shader_FinalizeOutgoingDirections.Dispatch( (LOBES_COUNT_PHI + 15) >> 4, (LOBES_COUNT_THETA + 15) >> 4, MAX_SCATTERING_ORDER );

 				m_Tex_LobeHistogram_Reflected_Decimal.RemoveFromLastAssignedSlotUAV();
 				m_Tex_LobeHistogram_Reflected_Integer.RemoveFromLastAssignedSlotUAV();
				m_Tex_LobeHistogram_Reflected.RemoveFromLastAssignedSlotUAV();

				if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
					// Finalize transmitted
					m_Tex_LobeHistogram_Transmitted_Decimal.SetCSUAV( 0 );
 					m_Tex_LobeHistogram_Transmitted_Integer.SetCSUAV( 1 );
					m_Tex_LobeHistogram_Transmitted.SetCSUAV( 2 );

					m_Shader_FinalizeOutgoingDirections.Dispatch( (LOBES_COUNT_PHI + 15) >> 4, (LOBES_COUNT_THETA + 15) >> 4, MAX_SCATTERING_ORDER );

 					m_Tex_LobeHistogram_Transmitted_Decimal.RemoveFromLastAssignedSlotUAV();
 					m_Tex_LobeHistogram_Transmitted_Integer.RemoveFromLastAssignedSlotUAV();
					m_Tex_LobeHistogram_Transmitted.RemoveFromLastAssignedSlotUAV();
				}
			}
		}

		bool	m_pauseRendering = false;
		void	Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null || m_pauseRendering )
				return;

			// Setup global data
			m_CB_Main.UpdateData();

			m_Tex_Heightfield.Set( 0 );
			m_Tex_OutgoingDirections_Reflected.Set( 1 );
			m_Tex_OutgoingDirections_Transmitted.Set( 2 );
			m_Tex_LobeHistogram_Reflected.Set( 3 );
			m_Tex_LobeHistogram_Transmitted.Set( 4 );

			// =========== Render scene ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			//////////////////////////////////////////////////////////////////////////
			// Render heightfield
			if ( !radioButtonHideSurface.Checked && m_Shader_RenderHeightField.Use() ) {
				m_CB_Render.m._Flags = (checkBoxShowNormals.Checked ? 1U : 0U)
//									 | (checkBoxShowOutgoingDirections.Checked ? 2U : 0U)
									 | (checkBoxShowReflectedDirectionsHistogram.Checked ? 4U : 0U)
									 | (checkBoxShowTransmittedDirectionsHistogram.Checked ? 8U : 0U);
				m_CB_Render.m._ScatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value - 1;
				m_CB_Render.m._IterationsCount = (uint) m_lastComputedHistogramIterationsCount;
				m_CB_Render.UpdateData();

				m_Prim_Heightfield.Render( m_Shader_RenderHeightField );
			}


			//////////////////////////////////////////////////////////////////////////
			// Render lobes
			if ( (checkBoxShowLobe.Checked || checkBoxShowAnalyticalLobe.Checked) && m_Shader_RenderLobe.Use() ) {
				// Compute reflected direction to orient the lobe against
				float	phi = (float) Math.PI * floatTrackbarControlPhi.Value / 180.0f;
				float	sinPhi = (float) Math.Sin( phi );
				float	cosPhi = (float) Math.Cos( phi );

				float	theta = (float) Math.PI * floatTrackbarControlAnalyticalLobeTheta.Value / 180.0f;
				float	sinTheta = (float) Math.Sin( theta );
				float	cosTheta = (float) Math.Cos( theta );
				float3	analyticalReflectedDirection = new float3( -sinTheta * cosPhi, -sinTheta * sinPhi, -cosTheta );		// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)
						analyticalReflectedDirection.z = -analyticalReflectedDirection.z;	// Mirror against surface

						theta = (float) Math.PI * floatTrackbarControlAnalyticalLobeTheta_T.Value / 180.0f;
						sinTheta = (float) Math.Sin( theta );
						cosTheta = (float) Math.Cos( theta );
//				float	IOR = Fresnel_IORFromF0( floatTrackbarControlF0.Value );
// 				float3	analyticalTransmittedDirection = new float3( sinTheta * cosPhi, sinTheta * sinPhi, cosTheta );
// 						analyticalTransmittedDirection = Refract( analyticalTransmittedDirection, float3.UnitZ, 1.0f / IOR );
				float3	analyticalTransmittedDirection = -new float3( sinTheta * cosPhi, sinTheta * sinPhi, cosTheta );

				float3	simulatedReflectedDirection = m_lastComputedDirection;
						simulatedReflectedDirection.z = -simulatedReflectedDirection.z;		// Mirror against surface

				float3	simulatedTransmittedDirection = Refract( -m_lastComputedDirection, float3.UnitZ, 1.0f / m_lastComputedIOR );


				m_CB_RenderLobe.m._Direction = m_lastComputedDirection;
				m_CB_RenderLobe.m._LobeIntensity = floatTrackbarControlLobeIntensity.Value;
				m_CB_RenderLobe.m._ScatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value - 1;

				// Flags for analytical lobe rendering
				uint	flags = 0U;
				if ( radioButtonAnalyticalBeckmann.Checked ) flags = 00U;
				else if ( radioButtonAnalyticalGGX.Checked ) flags = 01U;
				else if ( radioButtonAnalyticalPhong.Checked ) flags = 02U;
//					else if ( radioButtonAnalyticalPhong.Checked ) flags = 03U;	// Other
				flags <<= 4;	// First 3 bits are reserved!

				uint	generalDisplayFlags = checkBoxCompensateScatteringFactor.Checked ? 08U : 0U;	// Apply scattering order compensation factor


				//////////////////////////////////////////////////////////////////////////
				// Render lobes first as filled polygons

				// Render analytical lobes
				if ( checkBoxShowAnalyticalLobe.Checked ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
					m_CB_RenderLobe.m._Flags = 2U | generalDisplayFlags | flags;
					m_CB_RenderLobe.m._ReflectedDirection = analyticalReflectedDirection;
					m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness.Value;
					m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR.Value;
					m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT.Value;
					m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB.Value;
					m_CB_RenderLobe.UpdateData();

					m_Prim_Lobe.Render( m_Shader_RenderLobe );

					if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
						// Show transmitted lobe
						m_CB_RenderLobe.m._Flags = 2U | 4U | generalDisplayFlags | flags;
						m_CB_RenderLobe.m._ReflectedDirection = analyticalTransmittedDirection;
						m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness_T.Value;
						m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR_T.Value;
						m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT_T.Value;
						m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB_T.Value;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );
					}
				}

				// Render simulated lobes
				if ( checkBoxShowLobe.Checked ) {
					if ( m_fitting || checkBoxShowXRay.Checked )
						m_Device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.READ_DEPTH_LESS_EQUAL, BLEND_STATE.ALPHA_BLEND );	// Show as transparent during fitting...

					m_CB_RenderLobe.m._Flags = generalDisplayFlags | 0U;
					m_CB_RenderLobe.m._ReflectedDirection = simulatedReflectedDirection;
					m_CB_RenderLobe.UpdateData();

					m_Prim_Lobe.Render( m_Shader_RenderLobe );

					if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
						// Show transmitted lobe
						m_CB_RenderLobe.m._Flags = generalDisplayFlags | 4U;
						m_CB_RenderLobe.m._ReflectedDirection = simulatedTransmittedDirection;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );
					}
				}

				//////////////////////////////////////////////////////////////////////////
				// Render again, in wireframe this time
				//
				if ( !m_fitting && checkBoxShowWireframe.Checked ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.WIREFRAME, DEPTHSTENCIL_STATE.READ_DEPTH_LESS_EQUAL, checkBoxShowXRay.Checked ? BLEND_STATE.ALPHA_BLEND : BLEND_STATE.DISABLED );

					// Render analytical lobes
					if ( checkBoxShowAnalyticalLobe.Checked ) {
						m_CB_RenderLobe.m._Flags = generalDisplayFlags | 1U | 2U | flags;
						m_CB_RenderLobe.m._ReflectedDirection = analyticalReflectedDirection;
						m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness.Value;
						m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR.Value;
						m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT.Value;
						m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB.Value;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );

						if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
							// Show transmitted lobe
							m_CB_RenderLobe.m._Flags = generalDisplayFlags | 1U | 2U | 4U | flags;
							m_CB_RenderLobe.m._ReflectedDirection = analyticalTransmittedDirection;
							m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness_T.Value;
							m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR_T.Value;
							m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT_T.Value;
							m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB_T.Value;
							m_CB_RenderLobe.UpdateData();

							m_Prim_Lobe.Render( m_Shader_RenderLobe );
						}
					}

					// Render simulated lobes
					if ( checkBoxShowLobe.Checked ) {

						m_CB_RenderLobe.m._Flags = generalDisplayFlags | 1U;	// Wireframe mode
						m_CB_RenderLobe.m._ReflectedDirection = simulatedReflectedDirection;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );

						if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
							// Show transmitted lobe
							m_CB_RenderLobe.m._Flags = generalDisplayFlags | 1U | 4U;
							m_CB_RenderLobe.m._ReflectedDirection = simulatedTransmittedDirection;
							m_CB_RenderLobe.UpdateData();

							m_Prim_Lobe.Render( m_Shader_RenderLobe );
						}
					}
				}

				//////////////////////////////////////////////////////////////////////////
				// Render cylinder
				if ( m_Shader_RenderCylinder.Use() ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

					m_CB_RenderCylinder.m._Direction = -m_lastComputedDirection;	// We want the incoming direction point AWAY from the surface
					m_CB_RenderCylinder.m._Length = 10.0f;
					m_CB_RenderCylinder.m._Color = float3.UnitX;
					m_CB_RenderCylinder.m._Radius = 0.025f;
					m_CB_RenderCylinder.UpdateData();

					m_Prim_Cylinder.Render( m_Shader_RenderCylinder );

					if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
						m_CB_RenderCylinder.m._Direction = Refract( -m_lastComputedDirection, float3.UnitZ, 1.0f / m_lastComputedIOR );
						m_CB_RenderCylinder.m._Length = 10.0f;
						m_CB_RenderCylinder.m._Color = float3.UnitZ;
						m_CB_RenderCylinder.m._Radius = 0.0125f;
						m_CB_RenderCylinder.UpdateData();

						m_Prim_Cylinder.Render( m_Shader_RenderCylinder );
					}
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

		private void radioButtonSurfaceTypeChanged( object sender, EventArgs e ) {
			panelDielectric.Enabled = radioButtonDielectric.Checked;
		}

		private void buttonRayTrace_Click( object sender, EventArgs e )
		{
			try {
				m_pauseRendering = true;

				SURFACE_TYPE	surfaceType = SURFACE_TYPE.CONDUCTOR;
				if ( radioButtonDielectric.Checked )
					surfaceType = SURFACE_TYPE.DIELECTRIC;
				else if ( radioButtonDiffuse.Checked )
					surfaceType = SURFACE_TYPE.DIFFUSE;

				RayTraceSurface( floatTrackbarControlBeckmannRoughness.Value, floatTrackbarControlSurfaceAlbedo.Value, floatTrackbarControlF0.Value, surfaceType, (float) Math.PI * floatTrackbarControlTheta.Value / 180.0f, (float) Math.PI * floatTrackbarControlPhi.Value / 180.0f, integerTrackbarControlIterationsCount.Value );
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
			float3		m_centerOfMass;
			double		m_IOR;
			double		m_incomingDirection_CosPhi;
			double		m_incomingDirection_SinPhi;
			double		m_toleranceFactor;
			bool		m_fittingReflectedLobe;
			int			m_lobeType;
			double[,]	m_histogramData;

			int			m_iterationsCount = 0;

			double[]	m_parameters = new double[3];

			public LobeModel( TestForm _owner ) {
				m_owner = _owner;
			}

			public void		Init( float3 _incomingDirection, double _theta, bool _computeInitialThetaUsingCenterOfMass, double _roughness, double _IOR, double _scaleT, double _scaleB, double _scaleN, double _toleranceFactor, Texture2D _texHistogram_CPU, int _scatteringOrder, bool _fittingReflectedLobe ) {
				int	scattMin = _scatteringOrder-1;		// Because scattering order 1 is actually stored in first slice of the texture array
				int	scattMax = scattMin+1;				// To simulate a single scattering order
//				int	scattMax = MAX_SCATTERING_ORDER;	// To simulate all scattering orders accumulated

				// =========================================================================
				// 1] Readback lobe texture data into an array
				int	W = _texHistogram_CPU.Width;
				int	H = _texHistogram_CPU.Height;
				m_histogramData = new double[W,H];

				for ( int scatteringOrder=scattMin; scatteringOrder < scattMax; scatteringOrder++ ) {
					PixelsBuffer	Content = _texHistogram_CPU.Map( 0, scatteringOrder );
					using ( BinaryReader R = Content.OpenStreamRead() )
						for ( int Y=0; Y < H; Y++ )
							for ( int X=0; X < W; X++ )
								m_histogramData[X,Y] += W * H * R.ReadSingle();
					Content.CloseStream();
					_texHistogram_CPU.UnMap( 0, scatteringOrder );
				}


				// =========================================================================
				// 2] Compute center of mass from which we'll measure distances
				m_centerOfMass = float3.Zero;
				float3	wsOutgoingDirection = float3.Zero;
				for ( int Y=0; Y < H; Y++ ) {
					// Y = theta bin index = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) )
					// We need theta:
					double	theta = 2.0 * Math.Asin( Math.Sqrt( 0.5 * Y / LOBES_COUNT_THETA ) );
					double	cosTheta = Math.Cos( theta );
					double	sinTheta = Math.Sin( theta );

					for ( int X=0; X < W; X++ ) {

						// X = phi bin index = LOBES_COUNT_PHI * X / (2PI)
						// We need phi:
						double	phi = 2.0 * Math.PI * X / LOBES_COUNT_PHI;
						double	cosPhi = Math.Cos( phi );
						double	sinPhi = Math.Sin( phi );

						// Build simulated microfacet reflection direction in macro-surface space
						double	outgoingIntensity = m_histogramData[X,Y];
						wsOutgoingDirection.Set( (float) (outgoingIntensity * cosPhi * sinTheta), (float) (outgoingIntensity * sinPhi * sinTheta), (float) (outgoingIntensity * cosTheta) );

						// Accumulate lobe position
						m_centerOfMass += wsOutgoingDirection;
					}
				}
				m_centerOfMass /= W*H;

				// =========================================================================
				// 3] Optionally override theta to use the direction of the center of mass
				// (quite intuitive to start by aligning our lobe along the main simulated lobe direction!)
				if ( _computeInitialThetaUsingCenterOfMass ) {
					float3	towardCenterOfMass = m_centerOfMass.Normalized;
					_theta = Math.Acos( towardCenterOfMass.z );
					_scaleN = 2.0 * m_centerOfMass.Length;		// Also assume we should matche the simulated lobe's length
//					_scaleN *= 0.5;
				}

				// =========================================================================
				// 4] Setup initial parameters
				m_direction = _incomingDirection;
				m_direction.z = -m_direction.z;	// Mirror against surface to obtain reflected direction against which we'll compute masking/shadowing of incoming ray

				m_IOR = _IOR;

				double	incomingPhi = Math.Atan2( m_direction.y, m_direction.x );
				m_incomingDirection_CosPhi = Math.Cos( incomingPhi );
				m_incomingDirection_SinPhi = Math.Sin( incomingPhi );

				m_toleranceFactor = _toleranceFactor;
				m_fittingReflectedLobe = _fittingReflectedLobe;
				m_lobeType = m_owner.radioButtonAnalyticalPhong.Checked ? 0 : (m_owner.radioButtonAnalyticalBeckmann.Checked ? 1 : 2);


				Parameters = new double[] { _theta, _roughness, _scaleN };//, _scaleT, _scaleB };
//Parameters = new double[] { 39.5 * Math.PI / 180, 0.9646, 0.2222, 0.2384, 0.7758 };
//Parameters = new double[] { 0.0 * Math.PI / 180, 0.9444, 0.1666 };	// 0° start for matching a 0.8 roughness

			}

			#region Model Implementation

			public double[]		Parameters {
				get { return m_parameters; }
				set {
					m_parameters = value;

					// Update track bar parameters
					if ( m_fittingReflectedLobe ) {
						m_owner.floatTrackbarControlAnalyticalLobeTheta.Value = (float) (180.0 * m_parameters[0] / Math.PI);
						m_owner.floatTrackbarControlAnalyticalLobeRoughness.Value = (float) m_parameters[1];
						m_owner.floatTrackbarControlLobeScaleR.Value = (float) m_parameters[2];
//						m_owner.floatTrackbarControlLobeScaleT.Value = (float) m_parameters[3];
//						m_owner.floatTrackbarControlLobeScaleB.Value = (float) m_parameters[4];
					} else {
						m_owner.floatTrackbarControlAnalyticalLobeTheta_T.Value = (float) (180.0 * m_parameters[0] / Math.PI);
						m_owner.floatTrackbarControlAnalyticalLobeRoughness_T.Value = (float) m_parameters[1];
						m_owner.floatTrackbarControlLobeScaleR_T.Value = (float) m_parameters[2];
//						m_owner.floatTrackbarControlLobeScaleT_T.Value = (float) m_parameters[3];
//						m_owner.floatTrackbarControlLobeScaleB_T.Value = (float) m_parameters[4];
					}

					// Repaint every N iterations
					m_iterationsCount++;
//					if ( m_iterationsCount == 5 ) {
						m_iterationsCount = 0;
						m_owner.panelOutput.Refresh();
						m_owner.Application_Idle( null, EventArgs.Empty );
						Application.DoEvents();	// Force processing events for refresh
//					}
				}
			}

			public double Eval( double[] _newParameters ) {

				double	lobeTheta = _newParameters[0];
				double	lobeRoughness = _newParameters[1];
				double	lobeScaleN = _newParameters[2];

				double	invLobeScaleN = 1.0 / lobeScaleN;

				// Compute constant masking term due to incoming direction
 				double	maskingIncoming = Masking( m_direction.z, lobeRoughness );		// Masking( incoming )

				// Compute lobe's reflection vector and tangent space using new parameters
				double	cosTheta = Math.Cos( lobeTheta );
				double	sinTheta = Math.Sin( lobeTheta );

				float3	lobe_normal = new float3( (float) (sinTheta * m_incomingDirection_CosPhi), (float) (sinTheta * m_incomingDirection_SinPhi), (float) cosTheta );
// 				if ( !m_fittingReflectedLobe ) {
// 					lobe_normal = Refract( lobe_normal, float3.UnitZ, (float) m_IOR );
// 					lobe_normal.z = -lobe_normal.z;
// 				}

				float3	lobe_tangent = new float3( (float) -m_incomingDirection_SinPhi, (float) m_incomingDirection_CosPhi, 0.0f );	// Always lying in the X^Y plane
				float3	lobe_biTangent = lobe_normal.Cross( lobe_tangent );

				// Compute sum
				double	phi, theta, cosPhi, sinPhi;
				double	outgoingIntensity_Simulated, length;
				double	outgoingIntensity_Analytical, lobeIntensity;
				double	difference;
				float3	wsOutgoingDirection = float3.Zero;
				float3	wsOutgoingDirection2 = float3.Zero;
				float3	lsOutgoingDirection = float3.Zero;
				double	maskingOutGoing = 0.0;

				double	sum = 0.0;
				double	sum_Simulated = 0.0;
				double	sum_Analytical = 0.0;
				double	sqSum_Simulated = 0.0;
				double	sqSum_Analytical = 0.0;
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
						outgoingIntensity_Simulated = m_histogramData[X,Y];
						wsOutgoingDirection.Set( (float) (cosPhi * sinTheta), (float) (sinPhi * sinTheta), (float) cosTheta );

						// Compute maksing term due to outgoing direction
						maskingOutGoing = Masking( wsOutgoingDirection.z, lobeRoughness );		// Masking( outgoing )

						// Compute projection of world space direction onto reflected direction
						float	Vz = wsOutgoingDirection.Dot( lobe_normal );

//Vz = Math.Min( 0.99f, Vz );

						float	cosTheta_M = Math.Max( 1e-6f, Vz );

						// Compute the lobe intensity in local space
						lobeIntensity = NDF( cosTheta_M, lobeRoughness );
 						lobeIntensity *= maskingIncoming * maskingOutGoing;				// * Masking terms

						// Apply additional lobe scaling
						// This scale computed like this:
						//
						// We know that in the shader, local lobe coordinates are transformed into world space by doing:
						//	wsDirection_Scaled = float3( lsDirection.x, lsDirection.y, Scale * lsDirection.z );
						//	float	L = length( wsDirection_Scaled );
						//	wsDirection = wsDirection_Scaled / L;
						//
						// We need to compute L so we write:
						//
						//	lsDirection.x = L * wsDirection.x
						//	lsDirection.y = L * wsDirection.y
						//	lsDirection.z = L * wsDirection.z / Scale
						//
						// We know that:
						//	sqrt( lsDirection.x*lsDirection.x + lsDirection.y*lsDirection.y + lsDirection.z*lsDirection.z ) = 1
						//
						// So:
						//	L * sqrt( wsDirection.x*wsDirection.x + wsDirection.y*wsDirection.y + wsDirection.z*wsDirection.z / (Scale*Scale) ) = 1
						//
						// And:
						//	L = 1 / sqrt( 1 - wsDirection.z*wsDirection.x + wsDirection.z*wsDirection.z / (Scale*Scale) )
						//
						// So finally:
						//	L = 1 / sqrt( 1 + wsDirection.z*wsDirection.x * (1 / (Scale*Scale) - 1) )
						//
						length = 1.0 / Math.Sqrt( 1.0 + Vz*Vz * (invLobeScaleN*invLobeScaleN - 1.0) );

						outgoingIntensity_Analytical = lobeIntensity * length;	// Lobe intensity was estimated in lobe space, account for scaling when converting back in world space

						// Sum the difference between simulated intensity and lobe intensity
						outgoingIntensity_Analytical *= m_toleranceFactor;	// Apply tolerance factor so we're always a bit smaller than the simulated lobe


//						#if true	// Use center of mass?
						if ( m_owner.checkBoxTest.Checked ) {
							double	difference0 = outgoingIntensity_Simulated - outgoingIntensity_Analytical;

							float3	wsLobePosition_Simulated = (float) outgoingIntensity_Simulated * wsOutgoingDirection;
							float3	wsLobePosition_Analytical = (float) outgoingIntensity_Analytical * wsOutgoingDirection;
							// Subtract center of mass
							wsLobePosition_Simulated -= m_centerOfMass;
							wsLobePosition_Analytical -= m_centerOfMass;
							// Compute new intensities, relative to center of mass
							outgoingIntensity_Simulated = wsLobePosition_Simulated.Length;
							outgoingIntensity_Analytical = wsLobePosition_Analytical.Length;

							double	difference1 = outgoingIntensity_Simulated - outgoingIntensity_Analytical;

							difference = 0.5 * difference0 + 0.5 * difference1;

//							difference *= (wsLobePosition_Simulated - wsLobePosition_Analytical).Length;
//							difference += (wsLobePosition_Simulated - wsLobePosition_Analytical).Length;	// We also add the distance between lobe positions so it goes to the best of the 2 minima!

						} else {
							difference = outgoingIntensity_Simulated - outgoingIntensity_Analytical;
//							difference = outgoingIntensity_Simulated / Math.Max( 1e-6, outgoingIntensity_Analytical ) - 1.0;
//							difference = outgoingIntensity_Analytical / Math.Max( 1e-6, outgoingIntensity_Simulated ) - 1.0;
						}
//						#endif


						sum += difference * difference;

						sum_Simulated += outgoingIntensity_Simulated;
						sum_Analytical += outgoingIntensity_Analytical;
						sqSum_Simulated += outgoingIntensity_Simulated*outgoingIntensity_Simulated;
						sqSum_Analytical += outgoingIntensity_Analytical*outgoingIntensity_Analytical;
					}
				}
				sum /= LOBES_COUNT_THETA * LOBES_COUNT_PHI;	// Not very useful since BFGS won't care but I'm doing it anyway to have some sort of normalized sum, better for us humans

				return sum;
			}

			public void Constrain( double[] _Parameters ) {
				_Parameters[0] = Math.Max( 0.0, Math.Min( 0.4999 * Math.PI, _Parameters[0] ) );
				_Parameters[1] = Math.Max( 1e-4, Math.Min( 1.0, _Parameters[1] ) );
				_Parameters[2] = Math.Max( 1e-3, Math.Min( 10.0, _Parameters[2] ) );
//				_Parameters[3] = Math.Max( 1e-6, _Parameters[3] );
//				_Parameters[4] = Math.Max( 1e-6, _Parameters[4] );
			}

			#endregion

			double	Masking( double _cosTheta_V, double _roughness ) {
				switch ( m_lobeType ) {
					case 1:		return BeckmannG1( _cosTheta_V, _roughness );
					case 2:		return GGXG1( _cosTheta_V, _roughness );
					default:	return PhongG1( _cosTheta_V, _roughness );
				}
			}

			double	NDF( double _cosTheta_M, double _roughness ) {
				switch ( m_lobeType ) {
					case 1:		return BeckmannNDF( _cosTheta_M, _roughness );
					case 2:		return GGXNDF( _cosTheta_M, _roughness );
					default:	return PhongNDF( _cosTheta_M, _roughness );
				}
			}

			double	Roughness2PhongExponent( double _roughness ) {
//				return Math.Pow( 2.0, 10.0 * (1.0 - _roughness) + 1.0 );	// From https://seblagarde.wordpress.com/2011/08/17/hello-world/
				return Math.Pow( 2.0, 10.0 * (1.0 - _roughness) + 0.5 );	// Actually, we'd like some fatter rough lobes
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
				double	a = Math.Sqrt( 1.0 + 0.5 * n ) / tanThetaV;
				return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
			}

			// From Walter 2007
			// D(m) = exp( -tan( theta_m )² / a² ) / (PI * a² * cos(theta_m)^4)
			double	BeckmannNDF( double _cosTheta_M, double _roughness ) {
				double	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
				double	a2 = _roughness*_roughness;
				return Math.Exp( -(1.0 - sqCosTheta_M) / (sqCosTheta_M * a2) ) / (Math.PI * a2 * sqCosTheta_M*sqCosTheta_M);
			}

			// Masking G1(v,m) = 2 / (1 + erf( 1/(a * tan(theta_v)) ) + exp(-a²) / (a*sqrt(PI)))
			double	BeckmannG1( double _cosTheta_V, double _roughness ) {
				double	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
				double	tanThetaV = Math.Sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
				double	a = 1.0 / (_roughness * tanThetaV);

				#if true	// Simplified numeric version
					return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
				#else	// Analytical
					return 2.0 / (1.0 + erf( a ) + exp( -a*a ) / (a * SQRTPI));
				#endif
			}

			// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
			// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
			double	GGXNDF( double _cosTheta_M, double _roughness ) {
				double	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
				double	a2 = _roughness*_roughness;
//				return a2 / (Math.PI * sqCosTheta_M*sqCosTheta_M * pow2( a2 + (1.0-sqCosTheta_M)/sqCosTheta_M ));
				return a2 / (Math.PI * Math.Pow( sqCosTheta_M * (a2-1.0) + 1.0, 2.0 ));
			}

			// Masking G1(v,m) = 2 / (1 + sqrt( 1 + a² * tan(theta_v)² ))
			// Simplified into G1(v,m) = 2*cos(theta_v) / (1 + sqrt( cos(theta_v)² * (1-a²) + a² ))
			double	GGXG1( double _cosTheta_V, double _roughness ) {
				double	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
				double	a2 = _roughness*_roughness;
				return 2.0 * _cosTheta_V / (1.0 + Math.Sqrt( sqCosTheta_V * (1.0 - a2) + a2 ));
			}
		}

		LobeModel	m_lobeModel = null;
		WMath.BFGS	m_Fitter = new WMath.BFGS();

		void	PerformLobeFitting( float3 _incomingDirection, float _theta, bool _computeInitialThetaUsingCenterOfMass, float _roughness, float _IOR, float _scaleT, float _scaleB, float _scaleN, float _ToleranceFactor, int _scatteringOrder, bool _reflected ) {

			checkBoxShowAnalyticalLobe.Checked = true;

			// Read back histogram to CPU for fitting
			m_Tex_LobeHistogram_CPU.CopyFrom( _reflected ? m_Tex_LobeHistogram_Reflected : m_Tex_LobeHistogram_Transmitted );

			// Initialize lobe model
			m_lobeModel = new LobeModel( this );
			m_lobeModel.Init( _incomingDirection, _theta, _computeInitialThetaUsingCenterOfMass, _roughness, _IOR, _scaleT, _scaleB, _scaleN, _ToleranceFactor,  m_Tex_LobeHistogram_CPU, _scatteringOrder, _reflected );

// 			if ( !checkBoxTest.Checked ) {
// 				m_Fitter.SuccessTolerance = 1e-4;
// 				m_Fitter.GradientSuccessTolerance = 1e-4;
// 			}

			// Peform fitting
			m_Fitter.Minimize( m_lobeModel );

			panelOutput.Invalidate();
		}

		#endregion

		bool		m_fitting = false;
		private void buttonFit_Click( object sender, EventArgs e )
		{
			if ( m_fitting )
				throw new Exception( "Canceled!" );

			bool	fittingTransmittedLobe = m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC && tabControlAnalyticalLobes.SelectedTab == tabPageTransmittedLobe;
			bool	fittingReflectedLobe = !fittingTransmittedLobe;
			float	theta = (fittingReflectedLobe ? floatTrackbarControlAnalyticalLobeTheta.Value : floatTrackbarControlAnalyticalLobeTheta_T.Value) * (float) Math.PI / 180.0f;
			float	roughness = fittingReflectedLobe ? floatTrackbarControlAnalyticalLobeRoughness.Value : floatTrackbarControlAnalyticalLobeRoughness_T.Value;
			float	scaleT = fittingReflectedLobe ? floatTrackbarControlLobeScaleT.Value : floatTrackbarControlLobeScaleT_T.Value;
			float	scaleB = fittingReflectedLobe ? floatTrackbarControlLobeScaleB.Value : floatTrackbarControlLobeScaleB_T.Value;
			float	scaleR = fittingReflectedLobe ? floatTrackbarControlLobeScaleR.Value : floatTrackbarControlLobeScaleR_T.Value;

			if ( checkBoxInitializeDirectionTowardCenterOfMass.Checked ) {
				roughness = floatTrackbarControlBeckmannRoughness.Value;
			}

			try {
				groupBoxAnalyticalLobe.Enabled = false;
				m_fitting = true;

				buttonFit.Text = "Cancel";

				PerformLobeFitting( m_lastComputedDirection,
									theta,
									checkBoxInitializeDirectionTowardCenterOfMass.Checked,
									roughness,
									m_lastComputedIOR,
									scaleT,
									scaleB,
									scaleR,
									floatTrackbarControlFitTolerance.Value,
									integerTrackbarControlScatteringOrder.Value,
									fittingReflectedLobe
									);

				MessageBox( "Fitting succeeded after " + m_Fitter.IterationsCount + " iterations.\r\nReached minimum: " + m_Fitter.FunctionMinimum, MessageBoxButtons.OK, MessageBoxIcon.Information );

			} catch ( Exception _e ) {
				MessageBox( "An error occurred while performing lobe fitting:\r\n" + _e.Message + "\r\n\r\nLast minimum: " + m_Fitter.FunctionMinimum + " after " + m_Fitter.IterationsCount + " iterations..." );
			} finally {
				buttonFit.Text = "&Fit";

				m_fitting = false;
				groupBoxAnalyticalLobe.Enabled = true;
			}
		}
	}
}
