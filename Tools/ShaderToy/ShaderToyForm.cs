using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;

using RendererManaged;
using Nuaj.Cirrus.Utility;

namespace ShaderToy
{
	public partial class ShaderToyForm : Form
	{
		private Device		m_Device = new Device();

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;

			public float2		_MainPosition;
			public float2		_NeighborPosition0;
			public float2		_NeighborPosition1;
			public float2		_NeighborPosition2;
			public float2		_NeighborPosition3;

			public uint			_IsolatedProbeIndex;
			public float		_WeightMultiplier;
			public uint			_ShowWeights;
			public float		_DebugParm;
			public float2		_MousePosition;
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

		private ConstantBuffer<CB_Main>		m_CB_Main = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private Shader						m_Shader = null;
//		private Texture2D					m_Tex_Christmas = null;
		private Primitive					m_Prim_Quad = null;

		private Camera						m_Camera = new Camera();
		private CameraManipulator			m_Manipulator = new CameraManipulator();
		private bool						m_moveSeparator = false;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartGameTime = 0;
		public float						m_CurrentGameTime = 0;
		public float						m_StartFPSTime = 0;
		public int							m_SumFrames = 0;
		public float						m_AverageFrameTime = 0.0f;

		public ShaderToyForm()
		{
			InitializeComponent();

			Application.Idle += new EventHandler( Application_Idle );

//			BuildSurfaceRadianceIntegrals();
		}

		#region Airlight Integrals

		const double	MAX_U = 5.0;
		const double	HALF_PI = 0.5 * Math.PI;

		/// <summary>
		/// Following the mathematica notebook found in "D:\Docs\Computer Graphics\Volumetric, Clouds, Participating Medium, Light Scattering, Translucency\2005 Sun, Ramamoorthi.nb"
		///  this routine generates the tables representing the integral of function Gn depending on 2 parameters.
		/// Several of these tables are generated for different exponents n, the goal is then to find an approximation to generated these tables from an analytical expression.
		/// </summary>
		void	BuildSurfaceRadianceIntegrals() {

			BuildPreciseF();

//			string	ResultsPath = @"D:\Docs\Computer Graphics\Volumetric, Clouds, Participating Medium, Light Scattering, Translucency";
			string	ResultsPath = @".\";

			const int		TABLE_SIZE = 64;

			const int		IMPORTANCE_SAMPLES_COUNT = 4096;
			double[,]		QRNG = new WMath.Hammersley().BuildSequence( IMPORTANCE_SAMPLES_COUNT, 2 );

			for ( int TableIndex=0; TableIndex < 1; TableIndex++ ) {
				float		Exponent = 1.0f + TableIndex;
				double[,]	Table = new double[TABLE_SIZE,TABLE_SIZE];

				// Compute the integral for each parameter
				for ( int Y=0; Y < TABLE_SIZE; Y++ )
				{
					double	Theta_s = 0.5 * Math.PI * Y / (TABLE_SIZE-1);		// V in [0,PI/2]
					double	CosTheta_s = Math.Sin(Theta_s);
					double	SinTheta_s = Math.Cos(Theta_s);

					for ( int X=0; X < TABLE_SIZE; X++ )
					{
						double	Tsp = MAX_U * X / (TABLE_SIZE-1);				// U in [0,10]

						// Use importance sampling
						double	Sum = 0.0;
						for ( int SampleIndex=0; SampleIndex < IMPORTANCE_SAMPLES_COUNT; SampleIndex++ ) {

							double	X0 = QRNG[SampleIndex,0];
							double	X1 = QRNG[SampleIndex,1];
							double	Phi_i = 2.0 * Math.PI * X0;
							double	Theta_i = Math.Asin( Math.Pow( X1, 1.0 / (1.0 + Exponent) ) );	// Assuming we're integrating a cos^n(theta)

							double	CosGamma = Math.Cos( Theta_i )*Math.Cos( Theta_s ) + Math.Sin( Theta_i )*Math.Sin( Theta_s )*Math.Cos( Phi_i );	// Angle between the incoming ray and the source
							double	SinGamma = Math.Sqrt( 1 - CosGamma*CosGamma );
							double	Gamma = Math.Acos( CosGamma );

// 							double	x0 = Math.Sin(Theta_i) * Math.Sin( Phi_i );
// 							double	y0 = Math.Cos(Theta_i);
// 							double	z0 = Math.Sin(Theta_i) * Math.Cos( Phi_i );
// 							double	x1 = SinTheta_s;
// 							double	y1 = CosTheta_s;
// 							Gamma = Math.Acos( x0*x1 + y0*y1 );
// 
// 							double	CosGamma = Math.Cos( Gamma );
// 							double	SinGamma = Math.Sin( Gamma );

							double	Extinction = Math.Exp( -Tsp * CosGamma );
							double	DeltaF = F_Table( Tsp * SinGamma, HALF_PI ) - F_Table( Tsp * SinGamma, 0.5 * Gamma );
							double	Term = (Extinction / SinGamma) * DeltaF;
							if ( double.IsNaN( Term ) )
								throw new Exception();

							Sum += Term;
						}
						Sum /= IMPORTANCE_SAMPLES_COUNT;


/*						// Use standard numerical integration
						const double	dPhi = 2.0 * Math.PI / 160;
						const double	dTheta = 0.5 * Math.PI / 40;

						double	Sum = 0.0;
						for ( int ThetaIndex=0; ThetaIndex < 40; ThetaIndex++ ) {
							double	Theta_i = 0.5 * Math.PI * (0.5+ThetaIndex) / 40;
							double	SolidAngle = Math.Sin( Theta_i ) * dPhi * dTheta;

							for ( int PhiIndex=0; PhiIndex < 160; PhiIndex++ ) {
								double	Phi_i = Math.PI * PhiIndex / 160.0;

 								double	CosGamma = Math.Cos( Theta_i )*CosTheta_s + Math.Sin( Theta_i )*SinTheta_s*Math.Cos( Phi_i );	// Angle between the incoming ray and the source

// 								double	x0 = Math.Sin(Theta_i)*Math.Cos(Phi_i);
// 								double	y0 = Math.Cos(Theta_i);
// 								double	z0 = Math.Sin(Theta_i)*Math.Sin(Phi_i);
// 								double	x1 = SinTheta_s;
// 								double	y1 = CosTheta_s;
// 								double	CosGamma = x0*x1 + y0*y1;

								double	SinGamma = Math.Sqrt( 1 - CosGamma*CosGamma );
								double	Gamma = Math.Acos( CosGamma );

								double	Extinction = Math.Exp( -Tsp * CosGamma );
								double	DeltaF = F_Table( Tsp * SinGamma, HALF_PI ) - F_Table( Tsp * SinGamma, 0.5 * Gamma );
								double	Term = (Extinction / SinGamma) * DeltaF;
								if ( double.IsNaN( Term ) )
									throw new Exception();

								Sum += Term * Math.Pow( Math.Cos( Theta_i ), Exponent ) * SolidAngle;
							}
						}
*/
						Table[X,Y] = Sum;
					}
				}

				// Write the result
				FileInfo	TargetFile = new FileInfo( Path.Combine( ResultsPath, "TableG"+TableIndex+".double" ) );
				using ( FileStream S = TargetFile.Create() )
					using ( BinaryWriter W = new BinaryWriter( S ) ) {
						for ( int Y=0; Y < TABLE_SIZE; Y++ )
							for ( int X=0; X < TABLE_SIZE; X++ )
								W.Write( Table[X,Y] );
					}
			}
		}


		/// <summary>
		/// Here I'm using the analytical expression to find F(u,v) that I first derived from the first part of the document
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		double	F_analytical( double _u, double _v ) {
			double	a = 0.00118554 + _v * (0.599188 - 0.012787 * _v);
			double	b = 0.977767 + _v * (-0.748114 + _v* (0.555383 - _v * 0.175846));
			return _v * Math.Exp( a * Math.Pow( _u, b ) );
		}


		// ======= TABLE VERSION ======= 
		const int	TABLE_F_SIZE = 256;
		double[,]	TableF = new double[TABLE_F_SIZE,TABLE_F_SIZE];
		void	BuildPreciseF() {

			for ( int Y=0; Y < TABLE_F_SIZE; Y++ ) {
				double	v = HALF_PI * Y / (TABLE_F_SIZE-1);
				for ( int X=0; X < TABLE_F_SIZE; X++ ) {
					double	u = MAX_U * X / (TABLE_F_SIZE-1);
					TableF[X,Y] = F_NIntegrate( u, v );
				}
			}

			// Write the result
			FileInfo	TargetFile = new FileInfo( "TableF.double" );
			using ( FileStream S = TargetFile.Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( int Y=0; Y < TABLE_F_SIZE; Y++ )
						for ( int X=0; X < TABLE_F_SIZE; X++ )
							W.Write( TableF[X,Y] );
				}
		}

		double	F_Table( double _u, double _v ) {
			_u *= TABLE_F_SIZE / MAX_U;
			int		U0 = (int) Math.Floor( _u );
					U0 = Math.Max( 0, Math.Min( TABLE_F_SIZE-1, U0 ) );
			int		U1 = Math.Min( TABLE_F_SIZE-1, U0+1 );
			float	u = (float) (_u - U0);
			_v *= TABLE_F_SIZE / HALF_PI;
			int		V0 = (int) Math.Floor( _v );
					V0 = Math.Max( 0, Math.Min( TABLE_F_SIZE-1, V0 ) );
			int		V1 = Math.Min( TABLE_F_SIZE-1, V0+1 );
			float	v = (float) (_v - V0);

			double	F00 = TableF[U0,V0];
			double	F01 = TableF[U1,V0];
			double	F11 = TableF[U0,V1];
			double	F10 = TableF[U1,V1];

			double	F0 = (1.0-u) * F00 + u * F01;
			double	F1 = (1.0-u) * F10 + u * F11;
			double	F = (1.0-v) * F0 + v * F1;
			return F;
		}

		/// <summary>
		/// Numerical integration of F
		/// </summary>
		/// <param name="_u"></param>
		/// <param name="_v"></param>
		/// <returns></returns>
		double	F_NIntegrate( double _u, double _v ) {
			const int	SAMPLES_COUNT = 100;

			double	dTheta = _v / SAMPLES_COUNT;

			double	Theta = 0.0, Term;
			double	Sum = 0.0;
			for ( int i=0; i < SAMPLES_COUNT; i++, Theta+=dTheta ) {
				Term = Math.Exp( -_u * Math.Tan( Theta ) );
				Sum += Term;
			}
			Sum *= dTheta;
			return Sum;
		}

		#endregion

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _FileName )
		{
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				return Image2Texture( S );
		}
		public unsafe Texture2D	Image2Texture( System.IO.Stream _Stream )
		{
			int		W, H;
			byte[]	Content = null;
			using ( Bitmap B = Bitmap.FromStream( _Stream ) as Bitmap )
			{
				W = B.Width;
				H = B.Height;
				Content = new byte[W*H*4];

				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;
					int		Offset = 4*W*Y;
					for ( int X=0; X < W; X++, Offset+=4 )
					{
						Content[Offset+2] = *pScanline++;	// B
						Content[Offset+1] = *pScanline++;	// G
						Content[Offset+0] = *pScanline++;	// R
						Content[Offset+3] = *pScanline++;	// A
					}
				}
				B.UnlockBits( LockedBitmap );
			}
			return Image2Texture( W, H, Content );
		}
		public Texture2D	Image2Texture( int _Width, int _Height, byte[] _Content )
		{
			using ( PixelsBuffer Buff = new PixelsBuffer( _Content.Length ) )
			{
				using ( System.IO.BinaryWriter W = Buff.OpenStreamWrite() )
					W.Write( _Content );

				return Image2Texture( _Width, _Height, Buff );
			}
		}
		public Texture2D	Image2Texture( int _Width, int _Height, PixelsBuffer _Content )
		{
			return new Texture2D( m_Device, _Width, _Height, 1, 1, PIXEL_FORMAT.RGBA8_UNORM_sRGB, false, false, new PixelsBuffer[] { _Content } );
		}

		#endregion

		private void	BuildQuad()
		{
			VertexPt4[]	Vertices = new VertexPt4[4];
			Vertices[0] = new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) };	// Top-Left
			Vertices[1] = new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) };	// Bottom-Left
			Vertices[2] = new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) };	// Top-Right
			Vertices[3] = new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) };	// Bottom-Right

			ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

			m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_Device.Init( panelOutput.Handle, false, false );
			}
			catch ( Exception _e )
			{
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			BuildQuad();
//			m_Tex_Christmas = Image2Texture( new System.IO.FileInfo( "christmas.jpg" ) );

			try
			{
#if DEBUG
//				m_Shader = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Christmas.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
//				m_Shader = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Airlight.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
//				m_Shader = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/VoronoiInterpolation.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
//				m_Shader = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Room.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_Shader = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/TestMSBRDF.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
#else
				m_Shader = Shader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "Shaders/TestMSBRDF.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
#endif
			}
			catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader = null;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );

			// Initialize Voronoï neighbor positions
			m_CB_Main.m._MainPosition = new float2( 0.0f, 0.0f );
			m_CB_Main.m._NeighborPosition0 = new float2( -0.4f, -0.6f );
			m_CB_Main.m._NeighborPosition1 = new float2( 0.6f, -0.4f );
			m_CB_Main.m._NeighborPosition2 = new float2( -0.2f, 0.8f );
			m_CB_Main.m._NeighborPosition3 = new float2( -0.6f, 0.14f );
			m_CB_Main.m._MousePosition.Set( 0.5f, 0.5f );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 2.5f ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartGameTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			if ( m_Shader != null ) {
				m_Shader.Dispose();
			}
			m_Prim_Quad.Dispose();
//			m_Tex_Christmas.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {

//			float4x4	OldCamera2World = m_CB_Camera.m._Camera2World;

			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			// Allows transformation from old to new camera space (for reprojection)
//			m_CB_Camera.m._OldCamera2NewCamera = OldCamera2World * m_CB_Camera.m._World2Camera;

			m_CB_Camera.UpdateData();
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			float	lastGameTime = m_CurrentGameTime;
			m_CurrentGameTime = GetGameTime();
			
			if ( m_CurrentGameTime - m_StartFPSTime > 1.0f ) {
				m_AverageFrameTime = (m_CurrentGameTime - m_StartFPSTime) / Math.Max( 1, m_SumFrames );
				m_SumFrames = 0;
				m_StartFPSTime = m_CurrentGameTime;
			}
			m_SumFrames++;

			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			// Post render command
			if ( m_Shader != null ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
				m_CB_Main.m.iGlobalTime = m_CurrentGameTime - m_StartGameTime;
				m_CB_Main.m._WeightMultiplier = floatTrackbarControlWeightMultiplier.Value;
				m_CB_Main.m._ShowWeights = (uint) ((checkBoxShowWeights.Checked ? 1 : 0) | (checkBoxSmoothStep.Checked ? 2 : 0) | (checkBoxShowOrder3.Checked ? 4 : 0) | (checkBoxShowOnlyMS.Checked ? 8 : 0));
				m_CB_Main.m._DebugParm = floatTrackbarControlParm.Value;
				m_CB_Main.UpdateData();

//				m_Tex_Christmas.SetPS( 0 );

				m_Shader.Use();
				m_Prim_Quad.Render( m_Shader );
			} else {
				m_Device.Clear( new float4( 1.0f, 0, 0, 0 ) );
			}

			// Show!
			m_Device.Present( false );

			// Update window text
			Text = "ShaderToy - Avg. Frame Time " + (1000.0f * m_AverageFrameTime).ToString( "G5" ) + " ms (" + (1.0f / m_AverageFrameTime).ToString( "G5" ) + " FPS)";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}
		
		float2	Client2UV( Point _ClientPos ) {
			float2	UV = 2.0f * new float2( (float) _ClientPos.X / panelOutput.Width, (float) _ClientPos.Y / panelOutput.Height ) - float2.One;
					UV.x *= (float) panelOutput.Width / panelOutput.Height;

			return UV;
		}

		bool	m_MouseDown = false;
		float2	m_ButtonDownMouseUV;
		int		m_ButtonDownCellIndex = 0;
		float2	m_ButtonDownCellUV;
		private void panelOutput_MouseDown( object sender, MouseEventArgs e )
		{
			if ( e.Button != MouseButtons.Left)
				return;

			m_MouseDown = true;
			m_ButtonDownMouseUV = Client2UV( e.Location );

			// Determine which cell the user clicked
			float2[]	CellPositions = new float2[] {
				m_CB_Main.m._MainPosition,
				m_CB_Main.m._NeighborPosition0,
				m_CB_Main.m._NeighborPosition1,
				m_CB_Main.m._NeighborPosition2,
				m_CB_Main.m._NeighborPosition3,
			};

			float	BestCellSqDistance = float.MaxValue;
			for ( int CellIndex=0; CellIndex < CellPositions.Length; CellIndex++ ) {
				float	CellSqDistance = (m_ButtonDownMouseUV - CellPositions[CellIndex]).LengthSquared;
				if ( CellSqDistance < BestCellSqDistance ) {
					BestCellSqDistance = CellSqDistance;
					m_ButtonDownCellIndex = CellIndex;
				}
			}
			m_ButtonDownCellUV = CellPositions[m_ButtonDownCellIndex];
			m_CB_Main.m._IsolatedProbeIndex = (uint) m_ButtonDownCellIndex;
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs e )
		{
			if ( m_moveSeparator )
				m_CB_Main.m._MousePosition.Set( (float) e.X / panelOutput.Width, (float) e.Y / panelOutput.Height );

			if ( !m_MouseDown )
				return;

			float2	MouseUV = Client2UV( e.Location );

			float2[]	CellPositions = new float2[] {
				m_CB_Main.m._MainPosition,
				m_CB_Main.m._NeighborPosition0,
				m_CB_Main.m._NeighborPosition1,
				m_CB_Main.m._NeighborPosition2,
				m_CB_Main.m._NeighborPosition3,
			};

			// Make it move
			CellPositions[m_ButtonDownCellIndex] = m_ButtonDownCellUV + MouseUV - m_ButtonDownMouseUV;

			// Update positions
			m_CB_Main.m._MainPosition = CellPositions[0];
			m_CB_Main.m._NeighborPosition0 = CellPositions[1];
			m_CB_Main.m._NeighborPosition1 = CellPositions[2];
			m_CB_Main.m._NeighborPosition2 = CellPositions[3];
			m_CB_Main.m._NeighborPosition3 = CellPositions[4];
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs e )
		{
			m_MouseDown = false;
		}

		private void panelOutput_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if ( e.KeyCode == Keys.Space )
				m_moveSeparator = !m_moveSeparator;
		}
	}
}
