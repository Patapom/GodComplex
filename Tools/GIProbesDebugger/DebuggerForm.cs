using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

using RendererManaged;

namespace GIProbesDebugger
{
	public partial class DebuggerForm : Form
	{
		private Device		m_Device = new Device();

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float4x4		_Camera2World;
			public float4		_TargetSize;
			public uint			_Type;
			public uint			_Flags;
		}

		private ConstantBuffer<CB_Main>	m_CB_Main = null;

		private Shader					m_Shader_Render = null;

		private Texture2D				m_Tex_CubeMap = null;

		private Camera					m_Camera = new Camera();
		private CameraManipulator		m_Manipulator = new CameraManipulator();

		public DebuggerForm()
		{
			InitializeComponent();

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		#region Probe Pixels Loader

		struct Pixel {
			public uint		ParentSurfaceID;
			public float3	Position;
			public float3	Normal;
			public float3	Albedo;
			public float3	F0;
			public float3	StaticLitColor;
			public float3	SmoothedStaticLitColor;
			public uint		FaceIndex;
			public uint		EmissiveMatID;
			public uint		NeighborProbeID;
			public float	NeighborProbeDistance;
			public double	Importance;
			public float	Distance;
			public float	SmoothedDistance;
			public bool		Infinity;
			public float	SmoothedInfinity;
			public int		Distance2Border;
			public int		ParentSurfaceSampleIndex;
		}

		void	LoadProbePixels( FileInfo _FileName ) {

			if ( m_Tex_CubeMap != null ) {
				m_Tex_CubeMap.Dispose();
				m_Tex_CubeMap = null;
			}

			Pixel[][,]	CubeMapFaces = new Pixel[6][,];
			using ( FileStream S = _FileName.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {
					int	CubeMapSize = R.ReadInt32();
					for ( int CubeMapFaceIndex=0; CubeMapFaceIndex < 6; CubeMapFaceIndex++ ) {
						Pixel[,]	Face = new Pixel[CubeMapSize,CubeMapSize];
						CubeMapFaces[CubeMapFaceIndex] = Face;

						for ( int Y=0; Y < CubeMapSize; Y++ )
							for ( int X=0; X < CubeMapSize; X++ ) {
								Face[X,Y].ParentSurfaceID = R.ReadUInt32();
								Face[X,Y].Position.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Face[X,Y].Normal.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Face[X,Y].Albedo.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Face[X,Y].F0.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Face[X,Y].StaticLitColor.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Face[X,Y].SmoothedStaticLitColor.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
								Face[X,Y].FaceIndex = R.ReadUInt32();
								Face[X,Y].EmissiveMatID = R.ReadUInt32();
								Face[X,Y].NeighborProbeID = R.ReadUInt32();
								Face[X,Y].NeighborProbeDistance = R.ReadSingle();
								Face[X,Y].Importance = R.ReadDouble();
								Face[X,Y].Distance = R.ReadSingle();
								Face[X,Y].SmoothedDistance = R.ReadSingle();
								Face[X,Y].Infinity = R.ReadByte() != 0;
								Face[X,Y].SmoothedInfinity = R.ReadSingle();
								Face[X,Y].Distance2Border = R.ReadInt32();
								Face[X,Y].ParentSurfaceSampleIndex = R.ReadInt32();
							}

					}

 					List<PixelsBuffer>	Content = new List<PixelsBuffer>();
					float4			Value = new float4();

					for ( int CubeIndex=0; CubeIndex < 8; CubeIndex++ ) {
						for ( int CubeMapFaceIndex=0; CubeMapFaceIndex < 6; CubeMapFaceIndex++ ) {
							PixelsBuffer	Buff = new PixelsBuffer( CubeMapSize*CubeMapSize * 16 );
							Content.Add( Buff );

							using ( BinaryWriter W = Buff.OpenStreamWrite() ) {
								for ( int Y=0; Y < CubeMapSize; Y++ )
									for ( int X=0; X < CubeMapSize; X++ ) {
										Pixel	P = CubeMapFaces[CubeMapFaceIndex][X,Y];
										switch ( CubeIndex ) {
											case 0:
												Value.Set( P.Position, P.Distance );
												break;
											case 1:
												Value.Set( P.Normal, (float) P.Importance );
												break;
											case 2:
												Value.Set( P.Albedo, P.SmoothedInfinity );
												break;
											case 3:
												Value.Set( P.StaticLitColor, (float) P.ParentSurfaceID );
												break;
											case 4:
												Value.Set( P.SmoothedStaticLitColor, P.SmoothedDistance );
												break;
											case 5:
												Value.Set( (float) P.Distance2Border, (float) P.ParentSurfaceSampleIndex, P.Infinity ? 1 : 0, (float) P.FaceIndex );
												break;
											case 6:
												Value.Set( P.F0, (float) P.NeighborProbeID );
												break;
											case 7:
												Value.Set( P.NeighborProbeDistance, 0, 0, 0 );
												break;
										}
										W.Write( Value.x );
										W.Write( Value.y );
										W.Write( Value.z );
										W.Write( Value.w );
									}
							}
						}
					}
 
 					m_Tex_CubeMap = new Texture2D( m_Device, CubeMapSize, CubeMapSize, -6*8, 1, PIXEL_FORMAT.RGBA32_FLOAT, false, false, Content.ToArray() );
				}
		}

		#endregion

		#region Open/Close

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_Device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e )
			{
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "GI Probes Debugger", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );

			try
			{
				m_Shader_Render = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Render.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "GI Probes Debugger", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_Render = null;
			}

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( m_Shader_Render != null ) {
				m_Shader_Render.Dispose();
			}

			m_CB_Main.Dispose();

			if ( m_Tex_CubeMap != null )
				m_Tex_CubeMap.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Main.m._Camera2World = m_Camera.Camera2World;
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Setup global data
			m_CB_Main.m._TargetSize = new float4( Width, Height, 1.0f / Width, 1.0f / Height );
			m_CB_Main.m._Type = (uint) integerTrackbarControlDisplayType.Value;
			m_CB_Main.m._Flags = (uint) ((checkBoxShowCubeMapFaces.Checked ? 1 : 0) | (checkBoxShowDistance.Checked ? 2 : 0) | (checkBoxShowWSPosition.Checked ? 4 : 0));
			m_CB_Main.UpdateData();


			// Render the scene
			m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_Tex_CubeMap != null )
				m_Tex_CubeMap.SetPS( 0 );

			if ( m_Shader_Render != null && m_Shader_Render.Use() ) {
				m_Device.RenderFullscreenQuad( m_Shader_Render );
			} else {
				m_Device.Clear( new float4( 1, 1, 0, 0 ) );
			}

			// Show!
			m_Device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}

		private void buttonLoadProbe_Click( object sender, EventArgs e )
		{
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			LoadProbePixels( new FileInfo( openFileDialog.FileName ) );
		}
	}
}
