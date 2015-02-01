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

namespace ShaderToy
{
	public partial class ShaderToyForm : Form
	{
		private Device		m_Device = new Device();


		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;

			public float		_Beta;
		}

		private ConstantBuffer<CB_Main>	m_CB_Main = null;
		private Shader		m_Shader_Christmas = null;
		private Texture2D	m_Tex_Christmas = null;
		private Primitive	m_Prim_Quad = null;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartGameTime = 0;
		public float						m_CurrentGameTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		public ShaderToyForm()
		{
			InitializeComponent();

			Application.Idle += new EventHandler( Application_Idle );


			BuildSurfaceRadianceIntegrals();
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
			m_Tex_Christmas = Image2Texture( new System.IO.FileInfo( "christmas.jpg" ) );

			try
			{
//				m_Shader_Christmas = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Christmas.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
				m_Shader_Christmas = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Airlight.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;

				m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_Christmas = null;
			}

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartGameTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( m_Shader_Christmas != null ) {
				m_CB_Main.Dispose();
				m_Shader_Christmas.Dispose();
			}
			m_Prim_Quad.Dispose();
			m_Tex_Christmas.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime()
		{
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Post render command
			if ( m_Shader_Christmas != null ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
				m_CB_Main.m.iGlobalTime = GetGameTime() - m_StartGameTime;
				m_CB_Main.m._Beta = floatTrackbarControlBeta.Value;
				m_CB_Main.UpdateData();

				m_Tex_Christmas.SetPS( 0 );

				m_Shader_Christmas.Use();
				m_Prim_Quad.Render( m_Shader_Christmas );
			} else {
				m_Device.Clear( new float4( 1.0f, 0, 0, 0 ) );
			}

			// Show!
			m_Device.Present( false );


			// Update window text
//			Text = "Zombizous Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}
	}
}
