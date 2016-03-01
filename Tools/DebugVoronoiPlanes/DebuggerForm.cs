using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RendererManaged;
using Nuaj.Cirrus.Utility;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace DebugVoronoiPlanes
{
	public partial class DebuggerForm : Form
	{
		#region TYPES

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

			public float4x4		_OldCamera2NewCamera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Plane {
			public float3		_wsPosition;
			public float		_PlaneSize;
			public float3		_wsNormal;
			float				__PAD;
			public float3		_wsTangent;
		}

		public class	cell_t {
			public class	plane_t {
				public class	line_t {
					public class	vertex_t {
						public float	m_linePosition;
						public uint[]	m_planeIndices = new uint[3];

						public void		Read( BinaryReader _R ) {
							m_linePosition = _R.ReadSingle();
							m_planeIndices[0] = _R.ReadUInt32();
							m_planeIndices[1] = _R.ReadUInt32();
							m_planeIndices[2] = _R.ReadUInt32();
						}
					}

					public float3		m_wsPosition;
					public float3		m_wsDirection;
					public float3		m_wsOrthoDirection;

					public vertex_t[]	m_vertices = new vertex_t[2] {
						new vertex_t(),
						new vertex_t(),
					};

					public uint[]		m_planeIndices = new uint[2];

					public uint			m_clipperPlaneIndex;

					public void		Read( BinaryReader _R ) {
						m_wsPosition.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						m_wsDirection.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						m_wsOrthoDirection.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );

						m_vertices[0].Read( _R );
						m_vertices[1].Read( _R );

						m_planeIndices[0] = _R.ReadUInt32();
						m_planeIndices[1] = _R.ReadUInt32();

						m_clipperPlaneIndex = _R.ReadUInt32();
					}
				};

				public int		m_index;
				public bool		m_isValid;
				public bool		m_isNatural;
				public bool		m_isClosing;
				public float3	m_wsPosition;
				public float3	m_wsNormal;
				public float3	m_wsCenter;

				public List< line_t >	m_lines = new List< line_t >();
				public List< line_t >	m_removedLines = new List< line_t >();

				public void		Read( BinaryReader _R ) {
					m_index = _R.ReadInt32();
					m_isValid = _R.ReadBoolean();
					m_isNatural = _R.ReadBoolean();
					m_isClosing = _R.ReadBoolean();
					m_wsPosition.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
					m_wsNormal.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
					m_wsCenter.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );

					m_lines.Clear();
					m_removedLines.Clear();

					int	linesCount = _R.ReadInt32();
					for ( int lineIndex=0; lineIndex < linesCount; lineIndex++ ) {
						line_t	L = new line_t();
						m_lines.Add( L );
						L.Read( _R );
					}

					int	removedLinesCount = _R.ReadInt32();
					for ( int removedLinesIndex=0; removedLinesIndex < removedLinesCount; removedLinesIndex++ ) {
						line_t	L = new line_t();
						m_removedLines.Add( L );
						L.Read( _R );
					}
				}

				public void		Draw( DebuggerForm _owner ) {

					_owner.m_CB_Plane.m._wsPosition = m_wsPosition;
					_owner.m_CB_Plane.m._wsNormal = m_wsNormal;
					_owner.m_CB_Plane.m._wsTangent = ComputeTangent();
					_owner.m_CB_Plane.m._PlaneSize = 10.0f;
					_owner.m_CB_Plane.UpdateData();

					_owner.m_Prim_Quad.Render( _owner.m_Shader_Plane );
				}

				float3	ComputeTangent() {
					float3	result = float3.UnitZ.Cross( m_wsNormal );
					float	sqLength = result.LengthSquared;
					if ( sqLength < 1e-6f ) {
						return float3.UnitX;
					}
					result /= (float) Math.Sqrt( sqLength );
					return result;
				}
			}

			public int				m_index;
			public List< plane_t >	m_planes = new List< plane_t >();

			public void		Read( BinaryReader _R ) {
				m_planes.Clear();
				int		planesCount = _R.ReadInt32();
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					plane_t	P = new plane_t();
					m_planes.Add( P );
					P.Read( _R );
				}
			}

			public void		Draw( DebuggerForm _owner ) {
				for ( int planeIndex=0; planeIndex < m_planes.Count; planeIndex++ )
					m_planes[planeIndex].Draw( _owner );
			}
		}

		#endregion

		private Device						m_Device = new Device();

		private ConstantBuffer<CB_Main>		m_CB_Main = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Plane>	m_CB_Plane = null;
		private Shader						m_Shader_Plane = null;
		private Primitive					m_Prim_Quad = null;

		private Camera						m_Camera = new Camera();
		private CameraManipulator			m_Manipulator = new CameraManipulator();

		protected MemoryMappedFile			m_MMF = null;
		protected MemoryMappedViewAccessor	m_View = null;
		protected List< cell_t >			m_cells = new List< cell_t >();

		public DebuggerForm() {
			InitializeComponent();

			m_MMF = MemoryMappedFile.CreateOrOpen( @"VoronoiDebugger", 1 << 20, MemoryMappedFileAccess.Read );
			m_View = m_MMF.CreateViewAccessor( 0, 1 << 20, MemoryMappedFileAccess.Read );
		}

		private void	BuildQuad()
		{
			VertexP3N3G3T2[]	Vertices = new VertexP3N3G3T2[4];
			Vertices[0] = new VertexP3N3G3T2() { P = new float3( -1, +1, 0 ), N = new float3( 0, 0, 1 ), UV = new float2( 0, 0 ) };	// Top-Left
			Vertices[1] = new VertexP3N3G3T2() { P = new float3( -1, -1, 0 ), N = new float3( 0, 0, 1 ), UV = new float2( 0, 1 ) };	// Bottom-Left
			Vertices[2] = new VertexP3N3G3T2() { P = new float3( +1, +1, 0 ), N = new float3( 0, 0, 1 ), UV = new float2( 1, 0 ) };	// Top-Right
			Vertices[3] = new VertexP3N3G3T2() { P = new float3( +1, -1, 0 ), N = new float3( 0, 0, 1 ), UV = new float2( 1, 1 ) };	// Bottom-Right

			ByteBuffer	VerticesBuffer = VertexP3N3G3T2.FromArray( Vertices );

			m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3T2 );
		}

		#region MMF Planes Update

		byte[]	m_buffer = new byte[1 << 20];
		ulong	m_lastReadVersion = 0U;
		void	UpdatePlanes() {
			ulong	currentVersion = m_View.ReadUInt64( 0 );
			if ( currentVersion == m_lastReadVersion ) {
				return;	// Up to date!
			}

			// Read the entire buffer
			m_View.ReadArray( 0, m_buffer, 0, 1 << 20 );

			using ( MemoryStream S = new MemoryStream( m_buffer ) )
				using ( BinaryReader R = new BinaryReader( S ) ) {

					ulong	version = R.ReadUInt64();
					ulong	blockLength = R.ReadUInt64();
					ulong	checkSum = R.ReadUInt64();

					ulong	obtainedCheckSum = ComputeCheckSum( m_buffer, 24, blockLength );
					if ( obtainedCheckSum != checkSum ) {
						return;	// Failed! Retry next time...
					}

					int	cellIndex = R.ReadInt32();

					cell_t	C = null;
					if ( cellIndex < m_cells.Count )
						C = m_cells[cellIndex];
					else {
						C = new cell_t();
						C.m_index = cellIndex;
						m_cells.Add( C );
					}
					C.Read( R );
				}
		}

		ulong	ComputeCheckSum( byte[] _buffer, int _offset, ulong _length ) {
			ulong	checkSum = 0;

			int		blocksCount = (int) (_length >> 3);	// Amount of integer 64 bits blocks
			for ( int i=0; i < blocksCount; i++ ) {
				ulong	block =	   _buffer[_offset+0]
								| ((ulong) _buffer[_offset+1] << 8)
								| ((ulong) _buffer[_offset+2] << 16)
								| ((ulong) _buffer[_offset+3] << 24)
								| ((ulong) _buffer[_offset+4] << 32)
								| ((ulong) _buffer[_offset+5] << 40)
								| ((ulong) _buffer[_offset+6] << 48)
								| ((ulong) _buffer[_offset+7] << 56);
				_offset += 8;
				_length -= 8;
				checkSum += block;
			}
			ulong	remainder = 0;
			while ( _length > 0 ) {
				remainder <<= 8;
				remainder |= (ulong) _buffer[_offset];
				_offset++;
				_length--;
			}
			checkSum += remainder;

			return checkSum;
		}

		#endregion

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

			try {
				m_Shader_Plane = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderPlane.hlsl" ) ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_Plane = null;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Plane = new ConstantBuffer<CB_Plane>( m_Device, 2 );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, -2.5f ), new float3( 0, 1, 0 ), float3.UnitY );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			m_CB_Plane.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			if ( m_Shader_Plane != null ) {
				m_Shader_Plane.Dispose();
			}
			m_Prim_Quad.Dispose();

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

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			m_Device.Clear( new float4( 0, 0, 0, 0 ) );

			// Render planes
			if ( m_Shader_Plane.Use() ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ALPHA_BLEND );

				m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
				m_CB_Main.UpdateData();

				for ( int cellIndex=0; cellIndex < m_cells.Count; cellIndex++ ) {
					m_cells[cellIndex].Draw( this );
				}
			}

			// Show!
			m_Device.Present( false );

			// Update structure
			UpdatePlanes();
		}

		private void buttonReload_Click(object sender, EventArgs e)
		{
			m_Device.ReloadModifiedShaders();
		}
	}
}
