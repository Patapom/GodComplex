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
			public uint			_SampleIndex;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct SB_Sample {
			public uint			ID;
			public float3		Position;
			public float3		Normal;
			public float3		Tangent;
			public float3		BiTangent;
			public float		Radius;
			public float3		Albedo;
			public float3		F0;
			public uint			PixelsCount;
			public float		SHFactor;		// Ratio of sample pixels compared to total pixels count
			public float3		SH0, SH1, SH2;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct SB_EmissiveSurface {
			public uint			ID;
			public float3		SH0, SH1, SH2;
		}

		private ConstantBuffer<CB_Main>	m_CB_Main = null;

		private Shader					m_Shader_Render = null;

		private Texture2D								m_Tex_CubeMap = null;
		private StructuredBuffer<SB_Sample>				m_SB_Samples = null;
		private StructuredBuffer<SB_EmissiveSurface>	m_SB_EmissiveSurfaces = null;

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
			public uint		ParentSampleIndex;
			public bool		UsedForSampling;
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
			public uint		VoronoiProbeID;
			public double	Importance;
			public float	Distance;
			public float	SmoothedDistance;
			public bool		Infinity;
			public float	SmoothedInfinity;
		}

		void	LoadProbePixels( FileInfo _FileName ) {

			if ( m_Tex_CubeMap != null ) {
				m_Tex_CubeMap.Dispose();
				m_Tex_CubeMap = null;
			}
			if ( m_SB_Samples != null ) {
				m_SB_Samples.Dispose();
				m_SB_Samples = null;
			}
			if ( m_SB_EmissiveSurfaces != null ) {
				m_SB_EmissiveSurfaces.Dispose();
				m_SB_EmissiveSurfaces = null;
			}

			using ( FileStream S = _FileName.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {

					//////////////////////////////////////////////////////////////////
					// Read pixels
					Pixel[][,]	CubeMapFaces = new Pixel[6][,];

					int	CubeMapSize = R.ReadInt32();
					for ( int CubeMapFaceIndex=0; CubeMapFaceIndex < 6; CubeMapFaceIndex++ ) {
						Pixel[,]	Face = new Pixel[CubeMapSize,CubeMapSize];
						CubeMapFaces[CubeMapFaceIndex] = Face;

						for ( int Y=0; Y < CubeMapSize; Y++ )
							for ( int X=0; X < CubeMapSize; X++ ) {
								Face[X,Y].ParentSampleIndex = R.ReadUInt32();
								Face[X,Y].UsedForSampling = R.ReadBoolean();
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
								Face[X,Y].VoronoiProbeID = R.ReadUInt32();
								Face[X,Y].Importance = R.ReadDouble();
								Face[X,Y].Distance = R.ReadSingle();
								Face[X,Y].SmoothedDistance = R.ReadSingle();
								Face[X,Y].Infinity = R.ReadByte() != 0;
								Face[X,Y].SmoothedInfinity = R.ReadSingle();
							}

					}

 					List<PixelsBuffer>	Content = new List<PixelsBuffer>();
					float4				Value = new float4();

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
												Value.Set( P.Normal, P.SmoothedDistance );
												break;
											case 2:
												Value.Set( P.Albedo, P.SmoothedInfinity );
												break;
											case 3:
												Value.Set( P.StaticLitColor, (float) P.ParentSampleIndex );
												break;
											case 4:
												Value.Set( P.SmoothedStaticLitColor, (float) P.Importance );
												break;
											case 5:
												Value.Set( P.UsedForSampling ? 1 : 0, P.Infinity ? 1 : 0, (float) P.FaceIndex, (float) P.VoronoiProbeID );
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

					//////////////////////////////////////////////////////////////////
					// Read samples
					int	SamplesCount = (int) R.ReadUInt32();
					m_SB_Samples = new StructuredBuffer<SB_Sample>( m_Device, SamplesCount, true );

					for ( int SampleIndex=0; SampleIndex < SamplesCount; SampleIndex++ ) {
						m_SB_Samples.m[SampleIndex].ID = (uint) SampleIndex;
						m_SB_Samples.m[SampleIndex].Position.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_SB_Samples.m[SampleIndex].Normal.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_SB_Samples.m[SampleIndex].Tangent.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_SB_Samples.m[SampleIndex].BiTangent.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_SB_Samples.m[SampleIndex].Radius = R.ReadSingle();
						m_SB_Samples.m[SampleIndex].Albedo.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_SB_Samples.m[SampleIndex].F0.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						m_SB_Samples.m[SampleIndex].PixelsCount = R.ReadUInt32();
						m_SB_Samples.m[SampleIndex].SHFactor = R.ReadSingle();

						m_SB_Samples.m[SampleIndex].SH0.Set( (float) R.ReadDouble(), (float) R.ReadDouble(), (float) R.ReadDouble() );
						m_SB_Samples.m[SampleIndex].SH1.Set( (float) R.ReadDouble(), (float) R.ReadDouble(), (float) R.ReadDouble() );
						m_SB_Samples.m[SampleIndex].SH2.Set( (float) R.ReadDouble(), (float) R.ReadDouble(), (float) R.ReadDouble() );
					}
					m_SB_Samples.Write();

					//////////////////////////////////////////////////////////////////
					// Read emissive surfaces
					int	EmissiveSurfacesCount = (int) R.ReadUInt32();
					if ( EmissiveSurfacesCount > 0 ) {
						m_SB_EmissiveSurfaces = new StructuredBuffer<SB_EmissiveSurface>( m_Device, EmissiveSurfacesCount, true );

						for ( int SurfaceIndex=0; SurfaceIndex < EmissiveSurfacesCount; SurfaceIndex++ ) {
							m_SB_EmissiveSurfaces.m[SurfaceIndex].ID = R.ReadUInt32();
							m_SB_EmissiveSurfaces.m[SurfaceIndex].SH0.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
							m_SB_EmissiveSurfaces.m[SurfaceIndex].SH1.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
							m_SB_EmissiveSurfaces.m[SurfaceIndex].SH2.Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
						}
						m_SB_EmissiveSurfaces.Write();
					}
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
			if ( m_SB_Samples != null )
				m_SB_Samples.Dispose();
			if ( m_SB_EmissiveSurfaces != null )
				m_SB_EmissiveSurfaces.Dispose();

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
			m_CB_Main.m._Flags = (uint) ((checkBoxShowCubeMapFaces.Checked ? 1 : 0) | (checkBoxShowDistance.Checked ? 2 : 0) | (checkBoxShowWSPosition.Checked ? 4 : 0) | (checkBoxShowSamples.Checked ? 8 : 0) | (checkBoxShowNeighbors.Checked ? 16 : 0));
			m_CB_Main.m._Type = (uint) integerTrackbarControlDisplayType.Value;
			if ( checkBoxShowSamples.Checked ) {
				m_CB_Main.m._Type = (uint) (radioButtonSampleAll.Checked ? 1 : 0) | ((uint) (radioButtonSampleColor.Checked ? 0 : radioButtonSampleAlbedo.Checked ? 1 : radioButtonSampleNormal.Checked ? 2 : 4) << 1);
			}
			if ( checkBoxShowNeighbors.Checked ) {
				m_CB_Main.m._Type = (uint) (radioButtonNeighbors.Checked ? 0 : 1);
			}
			m_CB_Main.m._SampleIndex = (uint) integerTrackbarControlSampleIndex.Value;
			m_CB_Main.UpdateData();


			// Render the scene
			m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_Tex_CubeMap != null )
				m_Tex_CubeMap.SetPS( 0 );
			if ( m_SB_Samples != null )
				m_SB_Samples.SetInput( 1 );
			if ( m_SB_EmissiveSurfaces != null )
				m_SB_EmissiveSurfaces.SetInput( 2 );

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
