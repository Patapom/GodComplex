using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using SharpMath;
using Renderer;
using ImageUtility;
using Nuaj.Cirrus.Utility;

namespace TestMSBSDF
{
	public partial class TestForm : Form
	{
		#region CONSTANTS

		public const int				HEIGHTFIELD_SIZE = 512;						//  (must match HLSL declaration)
		public const int				MAX_SCATTERING_ORDER = 6;
		public const int				LOBES_COUNT_THETA = 128;					// (must match HLSL declaration)
		public const int				LOBES_COUNT_PHI = 2 * LOBES_COUNT_THETA;

		static readonly double	SQRT2 = Math.Sqrt( 2.0 );

		#endregion

		#region NESTED TYPES

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
			public uint			_iterationsCount;
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
			public uint			_scatteringOrder;
			public uint			_iterationsCount;
			public float		_lobeIntensity;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderLobe {
			public float3		_Direction;
			public float		_LobeIntensity;
			public float3		_ReflectedDirection;
			public uint			_scatteringOrder;
			public uint			_Flags;
			public float		_Roughness;
			public float		_ScaleR;
			public float		_ScaleT;
			public float		_ScaleB;
			public float		_maskingImportance;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_RenderCylinder {
			public float3		_Direction;
			public float		_Length;
			public float3		_Color;
			public float		_Radius;
		}
		
		public enum SURFACE_TYPE {
			CONDUCTOR,
			DIELECTRIC,
			DIFFUSE,
		}

		#endregion

		#region FIELDS

		private Device			m_Device = new Device();

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
		private ComputeShader		m_Shader_RayTraceSurface_Diffuse = null;
		private ComputeShader		m_Shader_AccumulateOutgoingDirections = null;
		private ComputeShader		m_Shader_FinalizeOutgoingDirections = null;
		private Shader				m_Shader_RenderHeightField = null;
		private Shader				m_Shader_RenderLobe = null;
		private Shader				m_Shader_RenderCylinder = null;

		private Texture2D			m_Tex_Random = null;
		private Texture2D			m_Tex_Heightfield_Normal = null;
		private Texture2D			m_Tex_Heightfield_Height = null;
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

		private AutomationForm		m_automation = new AutomationForm();

		bool						m_fitting = false;
		public bool		FittingMode {
			get { return m_fitting; }
			set { m_fitting = value; }
		}

		#endregion

		public TestForm() {
			InitializeComponent();

			m_automation.Owner = this;

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		#region Open/Close

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			integerTrackbarControlScatteringOrder.RangeMax = MAX_SCATTERING_ORDER+1;
			integerTrackbarControlScatteringOrder.VisibleRangeMax = MAX_SCATTERING_ORDER+1;



m_internalChange = true;
integerTrackbarControlScatteringOrder.Value = 0;
integerTrackbarControlIterationsCount.Value = 100;
floatTrackbarControlBeckmannRoughness.Value = 1.0f;
floatTrackbarControlTheta.Value = 0.0f;
checkBoxShowLobe.Checked = false;
checkBoxShowAnalyticalLobe.Checked = false;
checkBoxShowReflectedDirectionsHistogram.Checked = true;
floatTrackbarControlLobeIntensity.Value = 0.2f;
m_internalChange = false;




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
				m_Shader_ComputeBeckmannSurface = new ComputeShader( m_Device, new FileInfo( "Shaders/ComputeBeckmannSurface.hlsl" ), "CS", null );
				m_Shader_RayTraceSurface_Conductor = new ComputeShader( m_Device, new FileInfo( "Shaders/RayTraceSurface.hlsl" ), "CS_Conductor", null );
				m_Shader_RayTraceSurface_Dielectric = new ComputeShader( m_Device, new FileInfo( "Shaders/RayTraceSurface.hlsl" ), "CS_Dielectric", null );
				m_Shader_RayTraceSurface_Diffuse = new ComputeShader( m_Device, new FileInfo( "Shaders/RayTraceSurface.hlsl" ), "CS_Diffuse", null );
				m_Shader_AccumulateOutgoingDirections = new ComputeShader( m_Device, new FileInfo( "Shaders/AccumulateOutgoingDirections.hlsl" ), "CS", null );
				m_Shader_FinalizeOutgoingDirections = new ComputeShader( m_Device, new FileInfo( "Shaders/AccumulateOutgoingDirections.hlsl" ), "CS_Finalize", null );
				m_Shader_RenderHeightField = new Shader( m_Device, new FileInfo( "Shaders/RenderHeightField.hlsl" ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
				m_Shader_RenderLobe = new Shader( m_Device, new FileInfo( "Shaders/RenderLobe.hlsl" ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
				m_Shader_RenderCylinder = new Shader( m_Device, new FileInfo( "Shaders/RenderCylinder.hlsl" ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox( "Shader \"RenderHeightField\" failed to compile!\n\n" + _e.Message );
			}

			BuildPrimHeightfield();
			BuildPrimLobe();
			BuildPrimCylinder();

			m_SB_Beckmann = new StructuredBuffer<SB_Beckmann>( m_Device, 1024, true, false );

			BuildRandomTexture();
			BuildBeckmannSurfaceTexture( floatTrackbarControlBeckmannRoughness.Value );
			
			m_Tex_OutgoingDirections_Reflected = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO,  false, true, null );
			m_Tex_OutgoingDirections_Transmitted = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
			m_Tex_LobeHistogram_Reflected_Decimal = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32, COMPONENT_FORMAT.UINT, false, true, null );
			m_Tex_LobeHistogram_Reflected_Integer = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32, COMPONENT_FORMAT.UINT, false, true, null );
			m_Tex_LobeHistogram_Transmitted_Decimal = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32, COMPONENT_FORMAT.UINT, false, true, null );
			m_Tex_LobeHistogram_Transmitted_Integer = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32, COMPONENT_FORMAT.UINT, false, true, null );
			m_Tex_LobeHistogram_Reflected = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32F, COMPONENT_FORMAT.AUTO, false, true, null );
			m_Tex_LobeHistogram_Transmitted = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32F, COMPONENT_FORMAT.AUTO, false, true, null );
			m_Tex_LobeHistogram_CPU = new Texture2D( m_Device, LOBES_COUNT_PHI, LOBES_COUNT_THETA, MAX_SCATTERING_ORDER+1, 1, PIXEL_FORMAT.R32F, COMPONENT_FORMAT.AUTO, true, false, null );

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

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			e.Cancel = false;
			base.OnFormClosing( e );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			m_Shader_RenderCylinder.Dispose();
			m_Shader_RenderLobe.Dispose();
			m_Shader_RenderHeightField.Dispose();
			m_Shader_FinalizeOutgoingDirections.Dispose();
			m_Shader_AccumulateOutgoingDirections.Dispose();
			m_Shader_RayTraceSurface_Diffuse.Dispose();
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
			m_Tex_Heightfield_Normal.Dispose();
			m_Tex_Heightfield_Height.Dispose();
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

		#region Textures Generation

		double	GenerateNormalDistributionHeight() {
			double	U = SimpleRNG.GetUniform();	// Uniform distribution in ]0,1[
			double	errfinv = Functions.erfinv( 2.0 * U - 1.0 );
			double	h = SQRT2 * errfinv;
			return h;
		}

		/// <summary>
		/// Builds many random values into a texture
		/// </summary>
		void	BuildRandomTexture() {
			PixelsBuffer[]	Content = new PixelsBuffer[4];

			SimpleRNG.SetSeed( 561321987, 132194982 );
			for ( int arrayIndex=0; arrayIndex < 4; arrayIndex++ ) {
				Content[arrayIndex] = new PixelsBuffer( (uint) (HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE*4*System.Runtime.InteropServices.Marshal.SizeOf(typeof(float4))) );
				using ( BinaryWriter W = Content[arrayIndex].OpenStreamWrite() )
					for ( int Y=0; Y < HEIGHTFIELD_SIZE; Y++ ) {
						for ( int X=0; X < HEIGHTFIELD_SIZE; X++ ) {
							W.Write( (float) SimpleRNG.GetUniform() );
							W.Write( (float) SimpleRNG.GetUniform() );
							W.Write( (float) SimpleRNG.GetUniform() );
							W.Write( (float) SimpleRNG.GetUniform() );
						}
					}
				Content[arrayIndex].CloseStream();
			}

			m_Tex_Random = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 4, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, Content );
		}

		/// <summary>
		/// Builds a heightfield whose heights are distributed according to the following probability (a.k.a. the normal distribution with sigma=1 and µ=0):
		///		p(height) = exp( -0.5*height^2 ) / sqrt(2PI)
		///	
		///	From "2015 Heitz - Generating Procedural Beckmann Surfaces"
		/// </summary>
		/// <param name="_roughness"></param>
		/// <remarks>Only isotropic roughness is supported</remarks>
		public void	BuildBeckmannSurfaceTexture( float _roughness ) {
			m_internalChange = true;	// Shouldn't happen but modifying the slider value may trigger a call to this function again, this flag prevents it

			// Mirror current roughness
			floatTrackbarControlBeckmannRoughness.Value = _roughness;

			// Precompute stuff that resemble a lot to the Box-Muller algorithm to generate normal distribution random values
			SimpleRNG.SetSeed( 521288629, 362436069 );
			for ( int i=0; i < m_SB_Beckmann.m.Length; i++ ) {
				double	U0 = SimpleRNG.GetUniform();
				double	U1 = SimpleRNG.GetUniform();
				double	U2 = SimpleRNG.GetUniform();

				m_SB_Beckmann.m[i].m_phase = (float) (2.0 * Math.PI * U0);	// Phase

				double	theta = 2.0 * Math.PI * U1;
				double	radius = Math.Sqrt( -Math.Log( U2 ) );
				m_SB_Beckmann.m[i].m_frequencyX = (float) (radius * Math.Cos( theta ) * _roughness);	// Frequency in X direction
				m_SB_Beckmann.m[i].m_frequencyY = (float) (radius * Math.Sin( theta ) * _roughness);	// Frequency in Y direction
			}

			m_SB_Beckmann.Write();

			#if true
				if ( m_Tex_Heightfield_Height == null ) {
					m_Tex_Heightfield_Height = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 1, 1, PIXEL_FORMAT.R32F, COMPONENT_FORMAT.AUTO, false, true, null );
					m_Tex_Heightfield_Normal = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
				}

				// Run the CS
				if ( m_Shader_ComputeBeckmannSurface.Use() ) {

					m_Tex_Heightfield_Height.RemoveFromLastAssignedSlots();
					m_Tex_Heightfield_Normal.RemoveFromLastAssignedSlots();

					m_SB_Beckmann.SetInput( 0 );
					m_Tex_Heightfield_Height.SetCSUAV( 0 );
					m_Tex_Heightfield_Normal.SetCSUAV( 1 );

					float	size = floatTrackbarControlBeckmannSizeFactor.Value * HEIGHTFIELD_SIZE;
//					m_CB_ComputeBeckmann.m._Position_Size.Set( -128.0f, -128.0f, 256.0f, 256.0f );
					m_CB_ComputeBeckmann.m._Position_Size.Set( -0.5f * size, -0.5f * size, size, size );
					m_CB_ComputeBeckmann.m._HeightFieldResolution = HEIGHTFIELD_SIZE;
					m_CB_ComputeBeckmann.m._SamplesCount = (uint) m_SB_Beckmann.m.Length;
					m_CB_ComputeBeckmann.UpdateData();

					m_Shader_ComputeBeckmannSurface.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

					m_Tex_Heightfield_Normal.RemoveFromLastAssignedSlotUAV();
					m_Tex_Heightfield_Height.RemoveFromLastAssignedSlotUAV();
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

			#if false
				Texture2D	Tex_Heightfield_CPU = new Texture2D( m_Device, HEIGHTFIELD_SIZE, HEIGHTFIELD_SIZE, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, true, false, null );
				Tex_Heightfield_CPU.CopyFrom( m_Tex_Heightfield );
				float4[,]	normalHeight = new float4[HEIGHTFIELD_SIZE,HEIGHTFIELD_SIZE];
				float		minHeight = float.MaxValue, maxHeight = -float.MaxValue;
				Tex_Heightfield_CPU.ReadPixels( 0, 0, ( uint X, uint Y, BinaryReader R ) => {
					normalHeight[X,Y].x = R.ReadSingle();
					normalHeight[X,Y].y = R.ReadSingle();
					normalHeight[X,Y].z = R.ReadSingle();

					float	H = R.ReadSingle();
					normalHeight[X,Y].w = H;
					minHeight = Mathf.Min( minHeight, H );
					maxHeight = Mathf.Max( maxHeight, H );
				} );
				Tex_Heightfield_CPU.Dispose();
			#endif

			m_internalChange = false;
		}

		/// <summary>
		/// Builds the surface texture from an actual image file
		/// </summary>
		/// <param name="_textureFileName"></param>
		/// <param name="_pixelSize">Size of a pixel, assuming the maximum height is 1</param>
		public unsafe void	BuildSurfaceFromTexture( string _textureFileName, float _pixelSize ) {

			if ( m_Tex_Heightfield_Height != null ) {
				m_Tex_Heightfield_Height.Dispose();	// We will create a new one so dispose of the old one...
				m_Tex_Heightfield_Normal.Dispose();
			}

			// Read the bitmap
			int			W, H;
			float4[,]	Content = null;
// 			using ( Bitmap BM = Bitmap.FromFile( _textureFileName ) as Bitmap ) {
// 				W = BM.Width;
// 				H = BM.Height;
// 				Content = new float4[W,H];
// 
// 				BitmapData	LockedBitmap = BM.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
// 
// 				byte	R, G, B, A;
// 				for ( int Y=0; Y < H; Y++ ) {
// 					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y*LockedBitmap.Stride;
// 					for ( int X=0; X < W; X++ ) {
// 
// 						// Read in shitty order
// 						B = *pScanline++;
// 						G = *pScanline++;
// 						R = *pScanline++;
// 						A = *pScanline++;
// 
// // Use this if you really need RGBA data
// //						Content[X,Y].Set( R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f );
// 
// // But assuming it's a height field, we only store one component into alpha
// 						Content[X,Y].Set( 0, 0, 0, R / 255.0f );	// Use Red as height
// 					}
// 				}
// 				BM.UnlockBits( LockedBitmap );
// 			}
			using ( ImageFile I = new ImageFile( new FileInfo( _textureFileName ) ) ) {
				W = (int) I.Width;
				H = (int) I.Height;
				Content = new float4[W,H];
				I.ReadPixels( ( uint X, uint Y, ref float4 _color ) => { Content[X,Y] = _color; } );
			}

			// Build normal (shitty version)
			float	Hx0, Hx1, Hy0, Hy1;
			float3	dNx = new float3( 2.0f * _pixelSize, 0, 0 );
			float3	dNy = new float3( 0, 2.0f * _pixelSize, 0 );
			float3	N;
			for ( int Y=0; Y < H; Y++ ) {
				int	pY = (Y+H-1) % H;
				int	nY = (Y+1) % H;
				for ( int X=0; X < W; X++ ) {
					int	pX = (X+W-1) % W;
					int	nX = (X+1) % W;

					// Use a single row/column
// 					Hx0 = Content[pX,Y].w;
// 					Hx1 = Content[nX,Y].w;
// 					Hy0 = Content[X,pY].w;
// 					Hy1 = Content[X,nY].w;

					// Use 3 rows/columns
					Hx0 = (Content[pX,pY].w + Content[pX,Y].w + Content[pX,nY].w) / 3.0f;
					Hx1 = (Content[nX,pY].w + Content[nX,Y].w + Content[nX,nY].w) / 3.0f;
					Hy0 = (Content[pX,pY].w + Content[X,pY].w + Content[nX,pY].w) / 3.0f;
					Hy1 = (Content[pX,nY].w + Content[X,nY].w + Content[nX,nY].w) / 3.0f;

					dNx.z = Hx1 - Hx0;
					dNy.z = Hy0 - Hy1;	// Assuming +Y is upward

					N = dNx.Cross( dNy );
					N = N.Normalized;

					Content[X,Y].x = N.x;
					Content[X,Y].y = N.y;
					Content[X,Y].z = N.z;
				}
			}

			// Build the texture from the array
			PixelsBuffer	BufHeight = new PixelsBuffer( (uint) (W*H*4) );
			using ( BinaryWriter Writer = BufHeight.OpenStreamWrite() )
				for ( int Y=0; Y < H; Y++ )
					for ( int X=0; X < W; X++ ) {
						float4	pixel = Content[X,Y];
						Writer.Write( pixel.w );
					}
			BufHeight.CloseStream();

			PixelsBuffer	BufNormal = new PixelsBuffer( (uint) (W*H*16) );
			using ( BinaryWriter Writer = BufNormal.OpenStreamWrite() )
				for ( int Y=0; Y < H; Y++ )
					for ( int X=0; X < W; X++ ) {
						float4	pixel = Content[X,Y];
						Writer.Write( pixel.x );
						Writer.Write( pixel.y );
						Writer.Write( pixel.z );
						Writer.Write( pixel.w );
					}
			BufNormal.CloseStream();

			m_Tex_Heightfield_Height = new Texture2D( m_Device, (uint) W, (uint) H, 1, 1, PIXEL_FORMAT.R32F, COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { BufHeight } );
			m_Tex_Heightfield_Normal = new Texture2D( m_Device, (uint) W, (uint) H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { BufNormal } );
		}

		#endregion

		#region Ray-Tracing Simulation

		// Assuming n1=1 (air) we get:
		//	F0 = ((n2 - n1) / (n2 + n1))²
		//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
		//
		public static float	Fresnel_IORFromF0( float _F0 ) {
			double	SqrtF0 = Math.Sqrt( _F0 );
			return (float) ((1.0 + SqrtF0) / (1.00000001 - SqrtF0));
		}

		// From Walter 2007 eq. 40
		// Expects _incoming pointing AWAY from the surface
		// eta = IOR_above / IOR_below
		//
		public static float3	Refract( float3 _incoming, float3 _normal, float _eta ) {
			float	c = _incoming.Dot( _normal );
			float	k = (float) (_eta * c - Math.Sign(c) * Math.Sqrt( 1.0 + _eta * (c*c - 1.0) ));
			float3	R = k * _normal - _eta * _incoming;
			return R.Normalized;
		}

		/// <summary>
		/// Performs a ray-tracing of the surface
		/// Outputs resulting directions into a texture then performs a histogram
		/// 
		/// The surface is assigned a beam of photons, one for each texel of the heightfield texture
		/// At each iteration, the whole beam is offset a little using a Hammersley sequence that guarantees
		///  we end up ray-tracing the entire surface finely (hopefully, the full surface can be traced using enough iterations)
		/// </summary>
		/// <param name="_roughness">Surface roughness</param>
		/// <param name="_albedo">Surface albedo for diffuse or F0 for dielectrics</param>
		/// <param name="_surfaceType">Type of surface we're simulating</param>
		/// <param name="_theta">Vertical angle of incidence</param>
		/// <param name="_phi">Azimuthal angle of incidence</param>
		/// <param name="_iterationsCount">Amount of iterations of beam tracing</param>
		public void	RayTraceSurface( float _roughness, float _albedoF0, SURFACE_TYPE _surfaceType, float _theta, float _phi, int _iterationsCount ) {

			float	sinTheta = (float) Math.Sin( _theta );
			float	cosTheta = (float) Math.Cos( _theta );
			float	sinPhi = (float) Math.Sin( _phi );
			float	cosPhi = (float) Math.Cos( _phi );

			m_lastComputedDirection.Set( -sinTheta * cosPhi, -sinTheta * sinPhi, -cosTheta );	// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)
			m_lastComputedRoughness = _roughness;
			m_lastComputedAlbedo = _albedoF0;
			m_lastComputedIOR = Fresnel_IORFromF0( _albedoF0 );
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

			Hammersley	pRNG = new Hammersley();
			double[,]	sequence = pRNG.BuildSequence( _iterationsCount, 2 );
			for ( int iterationIndex=0; iterationIndex < _iterationsCount; iterationIndex++ ) {
				// 1] Ray-trace surface
				switch ( m_lastComputedSurfaceType ) {
					case SURFACE_TYPE.CONDUCTOR:
						if ( m_Shader_RayTraceSurface_Conductor.Use() ) {
							// Update trace offset
							m_CB_RayTrace.m._Offset.Set( (float) sequence[iterationIndex,0], (float) sequence[iterationIndex,1] );
							m_CB_RayTrace.UpdateData();

							m_Device.Clear( m_Tex_OutgoingDirections_Reflected, float4.Zero );	// Clear target directions and weights

							m_Tex_Heightfield_Height.SetCS( 0 );
							m_Tex_Heightfield_Normal.SetCS( 1 );
							m_Tex_Random.SetCS( 2 );
							m_Tex_OutgoingDirections_Reflected.SetCSUAV( 0 );	// New target buffer where to accumulate

							m_Shader_RayTraceSurface_Conductor.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlotUAV();
						}

						// 2] Accumulate into target histogram
						if ( m_Shader_AccumulateOutgoingDirections.Use() ) {
							m_Tex_OutgoingDirections_Reflected.SetCS( 0 );
							m_Tex_LobeHistogram_Reflected_Decimal.SetCSUAV( 0 );
							m_Tex_LobeHistogram_Reflected_Integer.SetCSUAV( 1 );

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER+1 );

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

							m_Tex_Heightfield_Height.SetCS( 0 );
							m_Tex_Heightfield_Normal.SetCS( 1 );
							m_Tex_Random.SetCS( 2 );
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

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER+1 );

 							m_Tex_LobeHistogram_Reflected_Decimal.RemoveFromLastAssignedSlotUAV();
 							m_Tex_LobeHistogram_Reflected_Integer.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlots();

							// Accumulated transmissions
							m_Tex_OutgoingDirections_Transmitted.SetCS( 0 );
							m_Tex_LobeHistogram_Transmitted_Decimal.SetCSUAV( 0 );
							m_Tex_LobeHistogram_Transmitted_Integer.SetCSUAV( 1 );

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER+1 );

 							m_Tex_LobeHistogram_Transmitted_Decimal.RemoveFromLastAssignedSlotUAV();
 							m_Tex_LobeHistogram_Transmitted_Integer.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Transmitted.RemoveFromLastAssignedSlots();
						}
						break;

					case SURFACE_TYPE.DIFFUSE:
						if ( m_Shader_RayTraceSurface_Diffuse.Use() ) {
							// Update trace offset
							m_CB_RayTrace.m._Offset.Set( (float) sequence[iterationIndex,0], (float) sequence[iterationIndex,1] );
							m_CB_RayTrace.UpdateData();

							m_Device.Clear( m_Tex_OutgoingDirections_Reflected, float4.Zero );	// Clear target directions and weights

							m_Tex_Heightfield_Height.SetCS( 0 );
							m_Tex_Heightfield_Normal.SetCS( 1 );
							m_Tex_Random.SetCS( 2 );
							m_Tex_OutgoingDirections_Reflected.SetCSUAV( 0 );	// New target buffer where to accumulate

							m_Shader_RayTraceSurface_Diffuse.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, 1 );

							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlotUAV();
						}

						// 2] Accumulate into target histogram
						if ( m_Shader_AccumulateOutgoingDirections.Use() ) {
							m_Tex_OutgoingDirections_Reflected.SetCS( 0 );
							m_Tex_LobeHistogram_Reflected_Decimal.SetCSUAV( 0 );
							m_Tex_LobeHistogram_Reflected_Integer.SetCSUAV( 1 );

							m_Shader_AccumulateOutgoingDirections.Dispatch( HEIGHTFIELD_SIZE >> 4, HEIGHTFIELD_SIZE >> 4, MAX_SCATTERING_ORDER+1 );

 							m_Tex_LobeHistogram_Reflected_Decimal.RemoveFromLastAssignedSlotUAV();
 							m_Tex_LobeHistogram_Reflected_Integer.RemoveFromLastAssignedSlotUAV();
							m_Tex_OutgoingDirections_Reflected.RemoveFromLastAssignedSlots();
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

				m_CB_Finalize.m._iterationsCount = (uint) _iterationsCount;
				m_CB_Finalize.UpdateData();

				m_Shader_FinalizeOutgoingDirections.Dispatch( (LOBES_COUNT_PHI + 15) >> 4, (LOBES_COUNT_THETA + 15) >> 4, MAX_SCATTERING_ORDER+1 );

 				m_Tex_LobeHistogram_Reflected_Decimal.RemoveFromLastAssignedSlotUAV();
 				m_Tex_LobeHistogram_Reflected_Integer.RemoveFromLastAssignedSlotUAV();
				m_Tex_LobeHistogram_Reflected.RemoveFromLastAssignedSlotUAV();

				if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
					// Finalize transmitted
					m_Tex_LobeHistogram_Transmitted_Decimal.SetCSUAV( 0 );
 					m_Tex_LobeHistogram_Transmitted_Integer.SetCSUAV( 1 );
					m_Tex_LobeHistogram_Transmitted.SetCSUAV( 2 );

					m_Shader_FinalizeOutgoingDirections.Dispatch( (LOBES_COUNT_PHI + 15) >> 4, (LOBES_COUNT_THETA + 15) >> 4, MAX_SCATTERING_ORDER+1 );

 					m_Tex_LobeHistogram_Transmitted_Decimal.RemoveFromLastAssignedSlotUAV();
 					m_Tex_LobeHistogram_Transmitted_Integer.RemoveFromLastAssignedSlotUAV();
					m_Tex_LobeHistogram_Transmitted.RemoveFromLastAssignedSlotUAV();
				} else {
					m_Device.Clear( m_Tex_LobeHistogram_Transmitted, float4.Zero );
				}
			}
		}

		/// <summary>
		/// Reads back a lobe texture histogram into an array
		/// </summary>
		/// <param name="_reflectedLobe"></param>
		/// <param name="_scatteringOrder"></param>
		/// <returns></returns>
		public double[,]	GetSimulationHistogram( bool _reflectedLobe, uint _scatteringOrder ) {
			uint	scattMin = _scatteringOrder-1;		// Because scattering order 1 is actually stored in first slice of the texture array
			uint	scattMax = scattMin+1;				// To simulate a single scattering order
//			uint	scattMax = MAX_SCATTERING_ORDER;	// To simulate all scattering orders accumulated

			m_Tex_LobeHistogram_CPU.CopyFrom( _reflectedLobe ? m_Tex_LobeHistogram_Reflected : m_Tex_LobeHistogram_Transmitted );

			uint		W = m_Tex_LobeHistogram_CPU.Width;
			uint		H = m_Tex_LobeHistogram_CPU.Height;
			double[,]	histogramData = new double[W,H];

			for ( uint scatteringOrder=scattMin; scatteringOrder < scattMax; scatteringOrder++ ) {
// 				PixelsBuffer	Content = _texHistogram_CPU.MapRead( 0, scatteringOrder );
// 				using ( BinaryReader R = Content.OpenStreamRead() )
// 					for ( int Y=0; Y < H; Y++ )
// 						for ( int X=0; X < W; X++ )
// 							histogramData[X,Y] += W * H * R.ReadSingle();
// 				Content.CloseStream();
// 				_texHistogram_CPU.UnMap( Content );

				m_Tex_LobeHistogram_CPU.ReadPixels( 0, scatteringOrder, ( uint X, uint Y, BinaryReader R ) => {
					histogramData[X,Y] += R.ReadSingle();
				} );
			}

			return histogramData;
		}

		#endregion

		public void	SetCurrentScatteringOrder( int _scatteringOrder ) {
			integerTrackbarControlScatteringOrder.Value = _scatteringOrder;
		}

		public void	UpdateLobeParameters( double[] _parameters, bool _isReflectedLobe ) {
			if ( InvokeRequired ) {
				BeginInvoke( (Action) (() => {
					UpdateLobeParameters( _parameters, _isReflectedLobe );
				}) );
				return;
			}

			checkBoxShowAnalyticalLobe.Checked = true;

			// Update track bar parameters
			if ( _isReflectedLobe ) {
				floatTrackbarControlAnalyticalLobeTheta.Value = (float) (180.0 * _parameters[0] / Math.PI);
				floatTrackbarControlAnalyticalLobeRoughness.Value = (float) _parameters[1];
				floatTrackbarControlLobeScaleT.Value = (float) _parameters[2];
				floatTrackbarControlLobeScaleR.Value = (float) _parameters[3];
//				floatTrackbarControlLobeScaleB.Value = (float) m_parameters[4];
				floatTrackbarControlLobeMaskingImportance.Value = (float) _parameters[4];
			} else {
				floatTrackbarControlAnalyticalLobeTheta_T.Value = (float) (180.0 * _parameters[0] / Math.PI);
				floatTrackbarControlAnalyticalLobeRoughness_T.Value = (float) _parameters[1];
				floatTrackbarControlLobeScaleT_T.Value = (float) _parameters[2];
				floatTrackbarControlLobeScaleR_T.Value = (float) _parameters[3];
//				floatTrackbarControlLobeScaleB_T.Value = (float) m_parameters[4];
				floatTrackbarControlLobeMaskingImportance_T.Value = (float) _parameters[4];
			}

			// Repaint
			UpdateApplication();
		}

		public void	 UpdateSurfaceParameters( int _scatteringOrder, float3 _incomingDirection, float _roughness, float _albedoF0, bool _rebuildBeckmannSurface ) {
			m_internalChange = !_rebuildBeckmannSurface;	// So the Beckmann surface is not recomputed again!

			integerTrackbarControlScatteringOrder.Value = _scatteringOrder;
			float	theta = (float) (180.0 * Math.Acos( -_incomingDirection.z ) / Math.PI);
			floatTrackbarControlTheta.Value = theta;
			floatTrackbarControlBeckmannRoughness.Value = _roughness;
			floatTrackbarControlSurfaceAlbedo.Value = _albedoF0;

			m_internalChange = false;
		}

		/// <summary>
		/// Called by the automation form to change the surface type
		/// </summary>
		/// <param name="_type"></param>
		public void	SetSurfaceType( SURFACE_TYPE _type ) {
			switch ( _type ) {
				case SURFACE_TYPE.CONDUCTOR: radioButtonConductor.Checked = true; break;
				case SURFACE_TYPE.DIELECTRIC: radioButtonDielectric.Checked = true; break;
				case SURFACE_TYPE.DIFFUSE: radioButtonDiffuse.Checked = true; break;
			}
		}

		/// <summary>
		/// Called by the automation form to change the lobe type
		/// </summary>
		/// <param name="_type"></param>
		public void SetLobeType( LobeModel.LOBE_TYPE _type ) {
			switch ( _type ) {
				case LobeModel.LOBE_TYPE.MODIFIED_PHONG: radioButtonAnalyticalPhong.Checked = true; break;
				case LobeModel.LOBE_TYPE.MODIFIED_PHONG_ANISOTROPIC: radioButtonAnalyticalPhongAnisotropic.Checked = true; break;
				case LobeModel.LOBE_TYPE.BECKMANN: radioButtonAnalyticalBeckmann.Checked = true; break;
				case LobeModel.LOBE_TYPE.GGX: radioButtonAnalyticalGGX.Checked = true; break;
			}
		}

		#region Rendering

		public void	UpdateApplication() {
			panelOutput.Refresh();
			Application_Idle( null, EventArgs.Empty );
			Application.DoEvents();	// Give a chance to the app to process messages!
		}

		bool	m_pauseRendering = false;
		void	Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null || m_pauseRendering )
				return;

			// Setup global data
			m_CB_Main.UpdateData();

			m_Tex_Heightfield_Height.Set( 0 );
			m_Tex_Heightfield_Normal.Set( 1 );
			m_Tex_OutgoingDirections_Reflected.Set( 2 );
			m_Tex_OutgoingDirections_Transmitted.Set( 3 );
			m_Tex_LobeHistogram_Reflected.Set( 4 );
			m_Tex_LobeHistogram_Transmitted.Set( 5 );

			// =========== Render scene ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

			m_Device.Clear( m_Device.DefaultTarget, new float4( 0.02f, 0.03f, 0.04f, 0 ) );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			//////////////////////////////////////////////////////////////////////////
			// Render heightfield
			if ( !radioButtonHideSurface.Checked && m_Shader_RenderHeightField.Use() ) {
				m_CB_Render.m._Flags = (checkBoxShowNormals.Checked ? 0x1U : 0U)
//									 | (checkBoxShowOutgoingDirections.Checked ? 2U : 0U)
									 | (checkBoxShowReflectedDirectionsHistogram.Checked ? 0x4U : 0U)
									 | (checkBoxShowTransmittedDirectionsHistogram.Checked ? 0x8U : 0U)
									 | (checkBoxCompensateScatteringFactor.Checked ? 0x10U : 0U);
				m_CB_Render.m._scatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value - 1;
				m_CB_Render.m._iterationsCount = (uint) m_lastComputedHistogramIterationsCount;
				m_CB_Render.m._lobeIntensity = floatTrackbarControlLobeIntensity.Value;
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

				float	theta = (float) Math.PI * floatTrackbarControlTheta.Value / 180.0f;
				float	sinTheta = (float) Math.Sin( theta );
				float	cosTheta = (float) Math.Cos( theta );
				float3	currentDirection = -new float3( sinTheta * cosPhi, sinTheta * sinPhi, cosTheta );		// Minus sign because we need the direction pointing TOWARD the surface (i.e. z < 0)

						theta = (float) Math.PI * floatTrackbarControlAnalyticalLobeTheta.Value / 180.0f;
						sinTheta = (float) Math.Sin( theta );
						cosTheta = (float) Math.Cos( theta );
				float3	analyticalReflectedDirection = -new float3( sinTheta * cosPhi, sinTheta * sinPhi, cosTheta );
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


				m_CB_RenderLobe.m._LobeIntensity = floatTrackbarControlLobeIntensity.Value;
				m_CB_RenderLobe.m._scatteringOrder = (uint) integerTrackbarControlScatteringOrder.Value - 1;

				// Flags for analytical lobe rendering
				uint	flags = 0U;
				if ( radioButtonAnalyticalBeckmann.Checked ) flags = 0U;
				else if ( radioButtonAnalyticalGGX.Checked ) flags = 1U;
				else if ( radioButtonAnalyticalPhong.Checked ) flags = 2U;
				else if ( radioButtonAnalyticalPhongAnisotropic.Checked ) flags = 3U;
				flags <<= 4;	// First 4 bits are reserved!

				uint	generalDisplayFlags = checkBoxCompensateScatteringFactor.Checked ? 08U : 0U;	// Apply scattering order compensation factor


				//////////////////////////////////////////////////////////////////////////
				// Render lobes first as filled polygons

				// Render analytical lobes
				if ( checkBoxShowAnalyticalLobe.Checked ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
					m_CB_RenderLobe.m._Flags = 2U | generalDisplayFlags | flags;
					m_CB_RenderLobe.m._Direction = currentDirection;
					m_CB_RenderLobe.m._ReflectedDirection = analyticalReflectedDirection;
					m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness.Value;
					m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR.Value;
					m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT.Value;
					m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB.Value;
					m_CB_RenderLobe.m._maskingImportance = floatTrackbarControlLobeMaskingImportance.Value;
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
						m_CB_RenderLobe.m._maskingImportance = floatTrackbarControlLobeMaskingImportance_T.Value;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );
					}

					if ( checkBoxShowDiffuseModel.Checked ) {
						// Show analytical diffuse model lobe
						m_Device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.NOCHANGE, checkBoxShowXRay.Checked ? BLEND_STATE.ALPHA_BLEND : BLEND_STATE.DISABLED );

						m_CB_RenderLobe.m._Flags = 2U | generalDisplayFlags | (4U << 4);
						m_CB_RenderLobe.m._ReflectedDirection = analyticalReflectedDirection;
						m_CB_RenderLobe.m._Roughness = floatTrackbarControlBeckmannRoughness.Value;
						m_CB_RenderLobe.m._ScaleR = floatTrackbarControlSurfaceAlbedo.Value;
						m_CB_RenderLobe.m._ScaleT = 1.0f;
						m_CB_RenderLobe.m._ScaleB = 0.0f;
						m_CB_RenderLobe.m._maskingImportance = floatTrackbarControlLobeMaskingImportance_T.Value;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );
					}
				}

				// Render simulated lobes
				if ( checkBoxShowLobe.Checked ) {
					if ( m_fitting || checkBoxShowXRay.Checked )
						m_Device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.READ_DEPTH_LESS_EQUAL, BLEND_STATE.ALPHA_BLEND );	// Show as transparent during fitting...

					m_CB_RenderLobe.m._Flags = generalDisplayFlags | 0U;
					m_CB_RenderLobe.m._Direction = m_lastComputedDirection;
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
						m_CB_RenderLobe.m._Flags = 1U | 2U | generalDisplayFlags | flags;
						m_CB_RenderLobe.m._Direction = currentDirection;
						m_CB_RenderLobe.m._ReflectedDirection = analyticalReflectedDirection;
						m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness.Value;
						m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR.Value;
						m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT.Value;
						m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB.Value;
						m_CB_RenderLobe.m._maskingImportance = floatTrackbarControlLobeMaskingImportance.Value;
						m_CB_RenderLobe.UpdateData();

						m_Prim_Lobe.Render( m_Shader_RenderLobe );

						if ( m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC ) {
							// Show transmitted lobe
							m_CB_RenderLobe.m._Flags = 1U | 2U | 4U | generalDisplayFlags | flags;
							m_CB_RenderLobe.m._ReflectedDirection = analyticalTransmittedDirection;
							m_CB_RenderLobe.m._Roughness = floatTrackbarControlAnalyticalLobeRoughness_T.Value;
							m_CB_RenderLobe.m._ScaleR = floatTrackbarControlLobeScaleR_T.Value;
							m_CB_RenderLobe.m._ScaleT = floatTrackbarControlLobeScaleT_T.Value;
							m_CB_RenderLobe.m._ScaleB = floatTrackbarControlLobeScaleB_T.Value;
							m_CB_RenderLobe.m._maskingImportance = floatTrackbarControlLobeMaskingImportance_T.Value;
							m_CB_RenderLobe.UpdateData();

							m_Prim_Lobe.Render( m_Shader_RenderLobe );
						}

						if ( checkBoxShowDiffuseModel.Checked ) {
							// Show analytical diffuse model lobe
							m_CB_RenderLobe.m._Flags = 1U | 2U | generalDisplayFlags | (4U << 4);
							m_CB_RenderLobe.m._ReflectedDirection = analyticalReflectedDirection;
							m_CB_RenderLobe.m._Roughness = floatTrackbarControlBeckmannRoughness.Value;
							m_CB_RenderLobe.m._ScaleR = floatTrackbarControlSurfaceAlbedo.Value;
							m_CB_RenderLobe.m._ScaleT = 1.0f;
							m_CB_RenderLobe.m._ScaleB = 0.0f;
							m_CB_RenderLobe.m._maskingImportance = floatTrackbarControlLobeMaskingImportance_T.Value;
							m_CB_RenderLobe.UpdateData();

							m_Prim_Lobe.Render( m_Shader_RenderLobe );
						}
					}

					// Render simulated lobes
					if ( checkBoxShowLobe.Checked ) {

						m_CB_RenderLobe.m._Flags = generalDisplayFlags | 1U;	// Wireframe mode
						m_CB_RenderLobe.m._Direction = m_lastComputedDirection;
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

		#endregion

		#region Lobe Fitting

		LobeModel	m_lobeModel = null;
		BFGS		m_Fitter = new BFGS();

		void	PerformLobeFitting( float3 _incomingDirection, float _theta, bool _computeInitialThetaUsingCenterOfMass, float _roughness, float _IOR, float _scale, float _flatteningFactor, float _maskingImportance, float _OversizeFactor, int _scatteringOrder, bool _reflected ) {

			checkBoxShowAnalyticalLobe.Checked = true;

			// Read back histogram to CPU for fitting
			double[,]	histogramData = GetSimulationHistogram( _reflected, (uint) _scatteringOrder );

			// Initialize lobe model
			m_lobeModel = new LobeModel();
			m_lobeModel.ParametersChanged += ( double[] _parameters ) => {
				UpdateLobeParameters( _parameters, _reflected );
			};
			m_lobeModel.InitTargetData( histogramData );

			LobeModel.LOBE_TYPE	lobeType =	radioButtonAnalyticalPhong.Checked ? LobeModel.LOBE_TYPE.MODIFIED_PHONG :
											(radioButtonAnalyticalPhongAnisotropic.Checked ? LobeModel.LOBE_TYPE.MODIFIED_PHONG_ANISOTROPIC :
											(radioButtonAnalyticalBeckmann.Checked ? LobeModel.LOBE_TYPE.BECKMANN : LobeModel.LOBE_TYPE.GGX));

			if ( _computeInitialThetaUsingCenterOfMass ) {
				// Optionally override theta to use the direction of the center of mass
				// (quite intuitive to start by aligning our lobe along the main simulated lobe direction!)
				float3	towardCenterOfMass = m_lobeModel.CenterOfMass.Normalized;
				_theta = (float) Math.Acos( towardCenterOfMass.z );
//				_scale = 2.0 * m_centerOfMass.Length;				// Also assume we should match the simulated lobe's length
				_flatteningFactor = lobeType == LobeModel.LOBE_TYPE.MODIFIED_PHONG ? 0.5f : 1.0f;	// Start from a semi-flattened shape so it can choose either direction...
				_scale = 0.01f * m_lobeModel.CenterOfMass.Length;	// In fact, I realized the algorithm converged much faster starting from a very small lobe!! (~20 iterations compared to 200 otherwise, because the gradient leads the algorithm in the wrong direction too fast and it takes hell of a time to get back on tracks afterwards if we start from too large a lobe!)
			}

			m_lobeModel.InitLobeData( lobeType, _incomingDirection, _theta, _roughness, _scale, _flatteningFactor, _maskingImportance, _OversizeFactor, checkBoxUseCenterOfMassForBetterFitting.Checked );

// 			if ( !checkBoxUseCenterOfMassForBetterFitting.Checked ) {
// 				m_Fitter.SuccessTolerance = 1e-4;
// 				m_Fitter.GradientSuccessTolerance = 1e-4;
// 			}

			// Peform fitting
			m_Fitter.Minimize( m_lobeModel );

			panelOutput.Invalidate();
		}

		#endregion

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

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_Device.ReloadModifiedShaders();
		}

		bool	m_internalChange = false;
		private void floatTrackbarControlBeckmannRoughness_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			if ( m_internalChange )
				return;

			try {
				m_pauseRendering = true;
				BuildBeckmannSurfaceTexture( floatTrackbarControlBeckmannRoughness.Value );
			} catch ( Exception ) {
			} finally {
				m_pauseRendering = false;
			}
		}

		private void buttonRayTrace_Click( object sender, EventArgs e ) {
			try {
				m_pauseRendering = true;

				SURFACE_TYPE	surfaceType = SURFACE_TYPE.CONDUCTOR;
				if ( radioButtonDielectric.Checked )
					surfaceType = SURFACE_TYPE.DIELECTRIC;
				else if ( radioButtonDiffuse.Checked )
					surfaceType = SURFACE_TYPE.DIFFUSE;

				RayTraceSurface( floatTrackbarControlBeckmannRoughness.Value, floatTrackbarControlSurfaceAlbedo.Value, surfaceType, (float) Math.PI * floatTrackbarControlTheta.Value / 180.0f, (float) Math.PI * floatTrackbarControlPhi.Value / 180.0f, integerTrackbarControlIterationsCount.Value );

				// Display some results
				try {
					string	decimalPlaces = "G5";
					string	lines = "";
					for ( int i=0; i < (surfaceType == SURFACE_TYPE.DIELECTRIC ? 2 : 1); i++ ) {
						bool	reflectedLobe = i == 0;
						double	totalSum = 0.0;

						// Compute sum for each order
						for ( int scatteringOrder=0; scatteringOrder <= MAX_SCATTERING_ORDER; scatteringOrder++ ) {
							double[,]	histogram = GetSimulationHistogram( reflectedLobe, (uint) (1+scatteringOrder) );
							double		sum = 0.0;
							for ( int Y=0; Y < LOBES_COUNT_THETA; Y++ )
								for ( int X=0; X < LOBES_COUNT_PHI; X++ )
									sum += histogram[X,Y];

							totalSum += sum;
							lines += (scatteringOrder > 0 ? " - " : "") + (scatteringOrder < MAX_SCATTERING_ORDER ? "Order " + (1+scatteringOrder) : "Other " ) + " = " + sum.ToString( decimalPlaces );
						}
						lines += " - Total = " + totalSum.ToString( decimalPlaces ) + "\r\n";
					}

					textBoxResults.Text = lines;
					
				} catch ( Exception ) {
					
				}

				m_pauseRendering = false;
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while ray-tracing the surface using " + integerTrackbarControlIterationsCount.Value + " iterations:\r\n" + _e.Message + "\r\n\r\nDisabling device..." );
				m_Device.Exit();
				m_Device = null;
			}
		}

		private void buttonFit_Click( object sender, EventArgs e ) {
			if ( m_fitting )
				throw new AutomationForm.CanceledException();

			bool	fittingTransmittedLobe = m_lastComputedSurfaceType == SURFACE_TYPE.DIELECTRIC && tabControlAnalyticalLobes.SelectedTab == tabPageTransmittedLobe;
			bool	fittingReflectedLobe = !fittingTransmittedLobe;
			float	theta = (fittingReflectedLobe ? floatTrackbarControlAnalyticalLobeTheta.Value : floatTrackbarControlAnalyticalLobeTheta_T.Value) * (float) Math.PI / 180.0f;
			float	roughness = fittingReflectedLobe ? floatTrackbarControlAnalyticalLobeRoughness.Value : floatTrackbarControlAnalyticalLobeRoughness_T.Value;
			float	scaleT = fittingReflectedLobe ? floatTrackbarControlLobeScaleT.Value : floatTrackbarControlLobeScaleT_T.Value;
			float	scaleB = fittingReflectedLobe ? floatTrackbarControlLobeScaleB.Value : floatTrackbarControlLobeScaleB_T.Value;
			float	scaleR = fittingReflectedLobe ? floatTrackbarControlLobeScaleR.Value : floatTrackbarControlLobeScaleR_T.Value;
			float	maskingImportance = fittingReflectedLobe ? floatTrackbarControlLobeMaskingImportance.Value : floatTrackbarControlLobeMaskingImportance_T.Value;

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
									scaleR,
									scaleT,
									maskingImportance,
									floatTrackbarControlFitOversize.Value,
									integerTrackbarControlScatteringOrder.Value,
									fittingReflectedLobe
								);

				MessageBox( "Fitting succeeded after " + m_Fitter.IterationsCount + " iterations.\r\nReached minimum: " + m_Fitter.FunctionMinimum, MessageBoxButtons.OK, MessageBoxIcon.Information );
			} catch ( AutomationForm.CanceledException ) {
				// Simply cancel...
				MessageBox( "Lobe fitting was canceled.\r\n\r\nLast minimum: " + m_Fitter.FunctionMinimum + " after " + m_Fitter.IterationsCount + " iterations..." );
			} catch ( Exception _e ) {
				MessageBox( "An error occurred while performing lobe fitting:\r\n" + _e.Message + "\r\n\r\nLast minimum: " + m_Fitter.FunctionMinimum + " after " + m_Fitter.IterationsCount + " iterations..." );
			} finally {
				buttonFit.Text = "&Fit";

				m_fitting = false;
				groupBoxAnalyticalLobe.Enabled = true;
			}
		}

		private void buttonAutomation_Click( object sender, EventArgs e ) {
			// Simply toggle show/hide, the form is always there, never disposed except when we are...
			if ( !m_automation.Visible )
				m_automation.Show( this );
			else
				m_automation.Visible = false;
		}

		private void buttonTestImage_Click( object sender, EventArgs e ) {
			BuildSurfaceFromTexture( "TestSurface.png", 1.0f );
		}

		#endregion
	}
}
