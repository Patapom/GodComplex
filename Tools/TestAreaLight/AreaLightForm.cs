using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using RendererManaged;

namespace AreaLightTest
{
	public partial class AreaLightForm : Form
	{
		private Device		m_Device = new Device();


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
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Object {
			public float4x4		_Local2World;
			public float4x4		_World2Local;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Material {
			public float4x4		_AreaLight2World;
			public float4x4		_World2AreaLight;
			public float3		_ProjectionDirection;
			public float		_LightIntensity;
			public float		_Gloss;
			public float		_Metal;
		}

		private ConstantBuffer<CB_Main>		m_CB_Main = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Object>	m_CB_Object = null;
		private ConstantBuffer<CB_Material>	m_CB_Material = null;

		private Shader		m_Shader_RenderAreaLight = null;
		private Shader		m_Shader_RenderScene = null;
		private Texture2D	m_Tex_AreaLight = null;
		private Texture2D	m_Tex_AreaLightSAT = null;

		private Primitive	m_Prim_Quad = null;
		private Primitive	m_Prim_Rectangle = null;
		private Primitive	m_Prim_Sphere = null;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartTime = 0;
		public float						m_CurrentTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		public AreaLightForm()
		{
			InitializeComponent();


// ComputeSAT( new System.IO.FileInfo( "Dummy.png" ), new System.IO.FileInfo( "DummySAT.dds" ) );
// ComputeSAT( new System.IO.FileInfo( "StainedGlass.png" ), new System.IO.FileInfo( "AreaLightSAT.dds" ) );


			Application.Idle += new EventHandler( Application_Idle );
		}

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

				return Image2Texture( _Width, _Height, PIXEL_FORMAT.RGBA8_UNORM_sRGB, Buff );
			}
		}
		public Texture2D	PipoImage2Texture( System.IO.FileInfo _FileName ) {
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {

					int	W, H;
					W = R.ReadInt32();
					H = R.ReadInt32();

					PixelsBuffer	Buff = new PixelsBuffer( 4 * W * H * 4 );
					using ( System.IO.BinaryWriter Wr = Buff.OpenStreamWrite() )
					{
						WMath.Vector4D	C = new WMath.Vector4D();
						for ( int Y=0; Y < H; Y++ ) {
							for ( int X=0; X < W; X++ ) {
								C.x = R.ReadSingle();
								C.y = R.ReadSingle();
								C.z = R.ReadSingle();
								C.w = R.ReadSingle();

								Wr.Write( C.x );
								Wr.Write( C.y );
								Wr.Write( C.z );
								Wr.Write( C.w );
							}
						}
					}

					return Image2Texture( W, H, PIXEL_FORMAT.RGBA32_FLOAT, Buff );
				}
		}
		public Texture2D	Image2Texture( int _Width, int _Height, PIXEL_FORMAT _Format, PixelsBuffer _Content )
		{
			return new Texture2D( m_Device, _Width, _Height, 1, 1, _Format, false, false, new PixelsBuffer[] { _Content } );
		}

		/// <summary>
		/// Builds the SAT
		/// </summary>
		/// <param name="_FileName"></param>
		public unsafe void	ComputeSAT( System.IO.FileInfo _FileName, System.IO.FileInfo _TargetFileName )
		{
			int		W, H;
			byte[]	Content = null;
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				using ( Bitmap B = Bitmap.FromStream( S ) as Bitmap )
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

			// Build the float4 image
			WMath.Vector4D[,]	Image = new WMath.Vector4D[W,H];
			for ( int Y=0; Y < H; Y++ ) {
				for ( int X=0; X < W; X++ ) {
					Image[X,Y] = new WMath.Vector4D( Content[4*(W*Y+X)+0] / 255.0f, Content[4*(W*Y+X)+1] / 255.0f, Content[4*(W*Y+X)+2] / 255.0f, 0.0f );

					// Linearize from gamma space
					Image[X,Y].x = (float) Math.Pow( Image[X,Y].x, 2.2 );
					Image[X,Y].y = (float) Math.Pow( Image[X,Y].y, 2.2 );
					Image[X,Y].z = (float) Math.Pow( Image[X,Y].z, 2.2 );
				}
			}

			// Perform the accumulation
			for ( int Y=0; Y < H; Y++ ) {
				for ( int X=1; X < W; X++ ) {
					Image[X,Y] += Image[X-1,Y];		// Build first scanline
				}
			}

			for ( int X=0; X < W; X++ ) {
				for ( int Y=1; Y < H; Y++ ) {
					Image[X,Y] += Image[X,Y-1];
				}
			}

			DirectXTexManaged.TextureCreator.CreateRGBA16FFile( _TargetFileName.FullName, Image );

			// Save as a simple format
			string	Pipo = _TargetFileName.FullName;
			Pipo = System.IO.Path.GetFileNameWithoutExtension( Pipo ) + ".pipo";
			System.IO.FileInfo	SimpleTargetFileName = new System.IO.FileInfo(  Pipo );
			using ( System.IO.FileStream S = SimpleTargetFileName.OpenWrite() )
				using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {

					Wr.Write( W );
					Wr.Write( H );
					for ( int Y=0; Y < H; Y++ ) {
						for ( int X=0; X < W; X++ ) {
							Wr.Write( Image[X,Y].x );
							Wr.Write( Image[X,Y].y );
							Wr.Write( Image[X,Y].z );
							Wr.Write( Image[X,Y].w );
						}
					}
				}
		} 

		#endregion

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

			BuildPrimitives();
			m_Tex_AreaLight = Image2Texture( new System.IO.FileInfo( "StainedGlass.png" ) );
			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT.pipo" ) );

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Object = new ConstantBuffer<CB_Object>( m_Device, 2 );
			m_CB_Material = new ConstantBuffer<CB_Material>( m_Device, 3 );

			try
			{
				m_Shader_RenderAreaLight = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderAreaLight.hlsl" ) ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderAreaLight = null;
			}

			try
			{
				m_Shader_RenderScene = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ) ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderScene = null;
			}

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( m_Shader_RenderAreaLight != null ) {
				m_Shader_RenderAreaLight.Dispose();
			}
			if ( m_Shader_RenderScene != null ) {
				m_Shader_RenderScene.Dispose();
			}

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Object.Dispose();
			m_CB_Material.Dispose();

			m_Prim_Quad.Dispose();
			m_Prim_Rectangle.Dispose();
//			m_Prim_Sphere.Dispose();

			m_Tex_AreaLight.Dispose();
			m_Tex_AreaLightSAT.Dispose();

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

		public void	UpdateCamera() {

			float3	Position = new float3( 0, 1, 4 );
			float3	Target = new float3( 0, 1, 0 );

			m_CB_Camera.m._Camera2World.MakeLookAtCamera( Position, Target, float3.UnitY );
			m_CB_Camera.m._World2Camera = m_CB_Camera.m._Camera2World.Inverse;

			m_CB_Camera.m._Camera2Proj.MakeProjectionPerspective( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Camera2World * m_CB_Camera.m._Proj2Camera;

			m_CB_Camera.UpdateData();
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			m_CB_Main.m.iGlobalTime = GetGameTime() - m_StartTime;
			m_CB_Main.UpdateData();

			// Setup camera data
			UpdateCamera();

			// =========== Render ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );

			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			m_Tex_AreaLightSAT.SetPS( 0 );

			float4x4	AreaLight2World = new float4x4(); 
						AreaLight2World.MakeLookAt( float3.UnitY, float3.UnitY + float3.UnitZ, float3.UnitY );
						AreaLight2World.Scale( new float3( 0.5f, 1.0f, 1.0f ) );

			float4x4	World2AreaLight = AreaLight2World.Inverse;

			// Render the area light itself
			if ( m_Shader_RenderAreaLight != null && m_Shader_RenderAreaLight.Use() ) {

				m_CB_Object.m._Local2World = AreaLight2World;
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.UpdateData();

				m_Tex_AreaLight.SetPS( 1 );

				m_Prim_Rectangle.Render( m_Shader_RenderAreaLight );
			} else {
				m_Device.Clear( new float4( 1, 0, 0, 0 ) );
			}


			// Render the scene
			if ( m_Shader_RenderScene != null && m_Shader_RenderScene.Use() ) {

				// Create a floor plane
				m_CB_Object.m._Local2World.MakeLookAt( float3.Zero, float3.UnitY, float3.UnitX );
				m_CB_Object.m._Local2World.Scale( new float3( 2.0f, 2.0f, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.UpdateData();

				double	Phi = Math.PI * floatTrackbarControlProjectionPhi.Value / 180.0;
				double	Theta = Math.PI * floatTrackbarControlProjectionTheta.Value / 180.0;
				float3	Direction = new float3( (float) (Math.Sin(Theta) * Math.Sin(Phi)), (float) (Math.Sin(Theta) * Math.Cos(Phi)), (float) Math.Cos( Theta ) );

				const float	DiffusionMin = 1e-2f;
				const float	DiffusionMax = 1000.0f;
				float	Diffusion = DiffusionMin / (DiffusionMin / DiffusionMax + floatTrackbarControlProjectionDiffusion.Value);
						Direction *= Diffusion;

				float3	LocalDirection = (new float4( Direction, 0 ) * World2AreaLight).AsVec3;

				m_CB_Material.m._AreaLight2World = AreaLight2World;
				m_CB_Material.m._World2AreaLight = World2AreaLight;
				m_CB_Material.m._ProjectionDirection = LocalDirection;
				m_CB_Material.m._LightIntensity = floatTrackbarControlLightIntensity.Value;
				m_CB_Material.m._Gloss = floatTrackbarControlGloss.Value;
				m_CB_Material.m._Metal = floatTrackbarControlMetal.Value;
				m_CB_Material.UpdateData();

				m_Prim_Rectangle.Render( m_Shader_RenderScene );

			} else {
				m_Device.Clear( new float4( 1, 1, 0, 0 ) );
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
