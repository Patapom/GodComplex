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
			public float		_sizeX0;
			public float3		_wsNormal;
			public float		_sizeX1;
			public float3		_wsTangent;
			public float		_sizeY;
			public float4		_color;
			public uint			_flags;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Line {
			public float3		_wsPosition0;
			public float		_thickness;
			public float3		_wsPosition1;
			float				__PAD;
			public float3		_wsOrtho;
			float				__PAD2;
			public float4		_color;
		}

		public class	cell_t {
			public class	plane_t {
				public class	line_t {
					public class	vertex_t {
						public float	m_linePosition;
						public float	m_lastCutDot;
						public uint		m_planeIndex = ~0U;

						public void		Read( BinaryReader _R ) {
							m_linePosition = _R.ReadSingle();
							m_lastCutDot = _R.ReadSingle();
							m_planeIndex = _R.ReadUInt32();
						}
					}

					plane_t				m_owner;
					public float3		m_wsPosition;
					public float3		m_wsDirection;
					public float3		m_wsOrthoDirection;

					public vertex_t[]	m_vertices = new vertex_t[2] {
						new vertex_t(),
						new vertex_t(),
					};

					public uint[]		m_planeIndices = new uint[2];

					public uint			m_clipperPlaneIndex;

					public line_t( plane_t _owner ) {
						m_owner = _owner;
					}

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

					public void		Draw( DebuggerForm _owner, bool _isRemovedLine, bool _isSelected ) {
						if ( _owner.checkBoxHideNonPlaneLines.Checked && m_owner.m_index != _owner.integerTrackbarControlPlane.Value )
							return;	// Hide lines that are not part of the currently selected plane

						float	offset = _owner.floatTrackbarControlLinesOffset.Value;

						float4	color = _isRemovedLine ? new float4( 1, 0, 0, 0.25f ) : (m_clipperPlaneIndex == ~0U ? new float4( 1, 1, 0, 1 ) : new float4( 0.1f, 0.5f, 0, 1 ));
						if ( _isSelected && _owner.checkBoxDebugLine.Checked )
							color = _owner.SelectedLineColor( color );
						if ( !_isRemovedLine ) {
							if ( !m_owner.m_owner.m_planes[(int)m_planeIndices[1]].m_isValid )
								color = _owner.FlashError( color );	// It's an error for a line to reference an invalid plane and still be part of this plane!
						}

						float3	wsPosition = m_wsPosition + offset * m_wsOrthoDirection;
						float	length0 = Math.Max( -1000.0f, m_vertices[0].m_linePosition ) + offset;
						float	length1 = Math.Min(  1000.0f, m_vertices[1].m_linePosition ) - offset;

						float3	wsStart = wsPosition + length0 * m_wsDirection;
						float3	wsEnd = wsPosition + length1 * m_wsDirection;

						_owner.DrawLine( wsStart, wsEnd, m_wsOrthoDirection, -1.0f, color );

						// Draw ortho arrow pointing inward
						const float	arrowSize = 0.2f;
						float3	wsCenter = 0.5f * (wsStart + wsEnd);
						if ( _owner.checkBoxShowLineNormals.Checked )
							_owner.DrawArrow( wsCenter, wsCenter + arrowSize * m_wsOrthoDirection, m_owner.m_wsNormal, color );
						if ( _owner.checkBoxShowLineDirections.Checked )
							_owner.DrawArrow( wsCenter, wsCenter + m_wsDirection, m_wsOrthoDirection, color );	// Draw line direction

						if ( _isRemovedLine )
							return;

						// Draw vertices
						bool	isInvalidStart = false;
						float4	startVertexColor = new float4( 0, 1, 0, 0.5f );
						if ( m_vertices[0].m_planeIndex == ~0U )
							startVertexColor = new float4( 1, 1, 1, 0.2f );
						else {
							plane_t	cuttingPlane = m_owner.m_owner.m_planes[(int) m_vertices[0].m_planeIndex];
							if ( !cuttingPlane.m_isValid ) {
								startVertexColor = _owner.FlashError( startVertexColor );	// It's an error for a vertex to reference an invalid plane and still be part of this plane!
								isInvalidStart = true;
							}
						}
						if ( _isSelected && _owner.checkBoxDebugVertex.Checked && _owner.integerTrackbarControlVertex.Value == 0 )
							startVertexColor = _owner.SelectedVertexColor( startVertexColor );

						bool	isInvalidEnd = false;
						float4	endVertexColor = new float4( 0, 1, 0, 0.5f );
						if ( m_vertices[1].m_planeIndex == ~0U )
							endVertexColor = new float4( 1, 1, 1, 0.2f );
						else {
							plane_t	cuttingPlane = m_owner.m_owner.m_planes[(int) m_vertices[1].m_planeIndex];
							if ( !cuttingPlane.m_isValid ) {
								endVertexColor = _owner.FlashError( endVertexColor );	// It's an error for a vertex to reference an invalid plane and still be part of this plane!
								isInvalidEnd = true;
							}
						}
						if ( _isSelected && _owner.checkBoxDebugVertex.Checked && _owner.integerTrackbarControlVertex.Value == 1 )
							endVertexColor = _owner.SelectedVertexColor( endVertexColor );

						_owner.DrawPoint( wsStart, isInvalidStart ? -8.0f : -4.0f, startVertexColor );
						_owner.DrawPoint( wsEnd, isInvalidEnd ? -8.0f : -4.0f, endVertexColor );
					}
				};

				cell_t			m_owner;
				public int		m_index;
				public bool		m_isValid;
				public bool		m_isNatural;
				public bool		m_isClosing;
				public float3	m_wsPosition;
				public float3	m_wsNormal;
				public float3	m_wsCenter;

				public List< line_t >	m_lines = new List< line_t >();
				public List< line_t >	m_removedLines = new List< line_t >();

				public plane_t( cell_t _owner ) {
					m_owner = _owner;
				}

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
						line_t	L = new line_t( this );
						m_lines.Add( L );
						L.Read( _R );
					}

					int	removedLinesCount = _R.ReadInt32();
					for ( int removedLinesIndex=0; removedLinesIndex < removedLinesCount; removedLinesIndex++ ) {
						line_t	L = new line_t( this );
						m_removedLines.Add( L );
						L.Read( _R );
					}
				}

				public void		Draw( DebuggerForm _owner, bool _isSelected ) {

					// Render the plane
					float4	color = m_isValid ? m_isClosing ? new float4( 1, 0.5f, 0.2f, 0.2f ) : new float4( 1, 1, 1, 0.2f ) : new float4( 1, 0, 0, 0.2f );
					if ( _isSelected && _owner.checkBoxDebugPlane.Checked )
						color = _owner.SelectedPlaneColor( color );

					if ( !_owner.checkBoxHidePlanes.Checked || _isSelected )
						_owner.DrawPlane( m_wsPosition, m_wsNormal, ComputeTangent(), 10.0f, 10.0f, 10.0f, color, false );

					// Render the lines
					for ( int lineIndex=0; lineIndex < m_lines.Count; lineIndex++ ) {
						m_lines[lineIndex].Draw( _owner, !m_isValid, _isSelected && lineIndex == _owner.integerTrackbarControlLine.Value );
					}

					if ( !_owner.checkBoxHideRemovedLines.Checked || (_isSelected && _owner.checkBoxDebugPlane.Checked) )
						for ( int lineIndex=0; lineIndex < m_removedLines.Count; lineIndex++ ) {
							m_removedLines[lineIndex].Draw( _owner, true, false );
						}
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
			public float3			m_wsPosition;
			public List< plane_t >	m_planes = new List< plane_t >();

			public void		Read( BinaryReader _R ) {
				m_wsPosition.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );

				m_planes.Clear();
				int		planesCount = _R.ReadInt32();
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					plane_t	P = new plane_t( this );
					m_planes.Add( P );
					P.Read( _R );
				}
			}

			public void		Draw( DebuggerForm _owner, bool _isSelected ) {
				float4	cellColor = new float4( 1, 0, 0, 1 );
				if ( _isSelected && _owner.checkBoxDebugCell.Checked )
					cellColor = _owner.SelectedCellColor( cellColor );
				_owner.DrawPoint( m_wsPosition, -10.0f, cellColor );

				for ( int planeIndex=0; planeIndex < m_planes.Count; planeIndex++ )
					m_planes[planeIndex].Draw( _owner, _isSelected && planeIndex == _owner.integerTrackbarControlPlane.Value );
			}
		}

		#endregion

		private Device						m_Device = new Device();

		private ConstantBuffer<CB_Main>		m_CB_Main = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Plane>	m_CB_Plane = null;
		private ConstantBuffer<CB_Line>		m_CB_Line = null;
		private Shader						m_Shader_Plane = null;
		private Shader						m_Shader_Line = null;
		private Primitive					m_Prim_Quad = null;

		private Camera						m_Camera = new Camera();
		private CameraManipulator			m_Manipulator = new CameraManipulator();

		protected MemoryMappedFile			m_MMF = null;
		protected MemoryMappedViewAccessor	m_View = null;
		protected List< cell_t >			m_cells = new List< cell_t >();

		public DebuggerForm() {
			InitializeComponent();

			m_MMF = MemoryMappedFile.CreateOrOpen( @"VoronoiDebugger", 1 << 20, MemoryMappedFileAccess.ReadWrite );
			m_View = m_MMF.CreateViewAccessor( 0, 1 << 20, MemoryMappedFileAccess.ReadWrite );	// Open in R/W even though we won't write into it, just because we need to have the same access privileges as the writer otherwise we make the writer crash when it attempts to open the file in R/W mode!

			integerTrackbarControlCell_ValueChanged( null, 0 );
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
					while ( m_cells.Count <= cellIndex ) {
						C = new cell_t();
						C.m_index = m_cells.Count;
						C.m_wsPosition = new float3( -10000, -10000, -10000 );
						m_cells.Add( C );

						integerTrackbarControlCell.Value = C.m_index;	// Always track last cell
					}
					m_cells[cellIndex].Read( R );

					m_lastReadVersion = version;
				}

			// Update text that is now maybe valid?
			UpdatePlaneInfos();
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
// 			ulong	remainder = 0;
// 			while ( _length > 0 ) {
// 				int	shift = 8 * (7-(int)_length);
// 				remainder |= ((ulong) _buffer[_offset]) << shift;
// 				_offset++;
// 				_length--;
// 			}
// 			checkSum += remainder;

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
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Voronoi Debugger", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			BuildQuad();

			try {
				m_Shader_Plane = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderPlane.hlsl" ) ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS", null );
				m_Shader_Line = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderLine.hlsl" ) ), VERTEX_FORMAT.P3N3G3T2, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "Voronoi Debugger", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_Plane = null;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Plane = new ConstantBuffer<CB_Plane>( m_Device, 2 );
			m_CB_Line = new ConstantBuffer<CB_Line>( m_Device, 2 );

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 1000.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, -2.5f ), new float3( 0, 1, 0 ), float3.UnitY );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

			m_CB_Line.Dispose();
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

		DateTime	m_startTime = DateTime.Now;
		float		m_cycle, m_cycle2, m_cycle3, m_cycleError;
		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			TimeSpan	totalTime = DateTime.Now - m_startTime;
			m_cycle = 0.5f * (1.0f + (float) Math.Sin( 6.0 * totalTime.TotalSeconds ));
			m_cycle2 = 0.5f * (1.0f + (float) Math.Sin( 1*Math.PI/3 + 6.0 * totalTime.TotalSeconds ));
			m_cycle3 = 0.5f * (1.0f + (float) Math.Sin( 2*Math.PI/3 + 6.0 * totalTime.TotalSeconds ));
			m_cycleError = 0.5f * (1.0f + (float) Math.Sin( 20.0 * totalTime.TotalSeconds ));	// Super fast!

			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			m_Device.Clear( new float4( 0, 0, 0, 0 ) );

			// Render planes
			if ( m_Shader_Plane.Use() ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ALPHA_BLEND );

				m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
				m_CB_Main.UpdateData();

				if ( checkBoxDebugCell.Checked && integerTrackbarControlCell.Value < m_cells.Count )
					m_cells[integerTrackbarControlCell.Value].Draw( this, true );
				else {
					for ( int cellIndex=0; cellIndex < m_cells.Count; cellIndex++ ) {
						m_cells[cellIndex].Draw( this, cellIndex == integerTrackbarControlCell.Value );
					}
				}
			}

			// Show!
			m_Device.Present( false );

			// Update structure
			UpdatePlanes();
		}

		cell_t	SelectedCell {
			get { return integerTrackbarControlCell.Value < m_cells.Count ? m_cells[integerTrackbarControlCell.Value] : null; }
		}
		cell_t.plane_t	SelectedPlane {
			get {
				cell_t	C = SelectedCell;
				return C != null && integerTrackbarControlPlane.Value < C.m_planes.Count ? C.m_planes[integerTrackbarControlPlane.Value] : null;
			}
		}
		cell_t.plane_t.line_t	SelectedLine {
			get {
				cell_t.plane_t	P = SelectedPlane;
				return P != null && integerTrackbarControlLine.Value < P.m_lines.Count ? P.m_lines[integerTrackbarControlLine.Value] : null;
			}
		}
		string	PrintFloat3( float3 v ) {
			return "{ " + v.x + ", " + v.y + ", " + v.z + " }";
		}
		void	UpdatePlaneInfos() {
			cell_t.plane_t	P = SelectedPlane;
			labelPlaneInfo.Text = "Plane Info";
			textBoxPlaneInfos.Text = "";
			if ( P != null ) {
				labelPlaneInfo.Text += " for plane " + P.m_index + "/" + SelectedCell.m_planes.Count;
				textBoxPlaneInfos.Text = "Plane #" + P.m_index + "\r\n";
				textBoxPlaneInfos.Text += " isNatural = " + P.m_isNatural + " isValid = " + P.m_isValid + " isClosing = " + P.m_isClosing + "\r\n";
				textBoxPlaneInfos.Text += " Lines " + P.m_lines.Count + " - Removed Lines " + P.m_removedLines.Count + "\r\n";
				textBoxPlaneInfos.Text += " Pos = " + PrintFloat3( P.m_wsPosition ) + "\r\n";
				textBoxPlaneInfos.Text += " Normal = " + PrintFloat3( P.m_wsNormal ) + "\r\n";
			}
			UpdateLineInfos();
		}
		void	UpdateLineInfos() {
			cell_t.plane_t.line_t	L = SelectedLine;
			textBoxLineInfos.Text = "";
			if ( L != null ) {
				textBoxLineInfos.Text = "Plane Indices = { " + L.m_planeIndices[0] + ", " + L.m_planeIndices[1] + " }\r\n";
				textBoxLineInfos.Text += " Clipper Index = " + L.m_clipperPlaneIndex + "\r\n";
				textBoxLineInfos.Text += " Pos = " + PrintFloat3( L.m_wsPosition ) + "\r\n";
				textBoxLineInfos.Text += " Dir = " + PrintFloat3( L.m_wsDirection ) + "\r\n";
				textBoxLineInfos.Text += " Ortho = " + PrintFloat3( L.m_wsOrthoDirection ) + "\r\n";
			}
			UpdateVertexInfos();
		}
		void	UpdateVertexInfos() {
			cell_t.plane_t.line_t	L = SelectedLine;
			if ( L == null ) {
				textBoxLineInfos.Text = "";
				return;
			}
			cell_t.plane_t.line_t.vertex_t	V = integerTrackbarControlVertex.Value == 0 ? L.m_vertices[0] : L.m_vertices[1];

			textBoxVertexInfos.Text = "Vertex #" + integerTrackbarControlVertex.Value + "\r\n";
			textBoxVertexInfos.Text += " Line Position = " + V.m_linePosition + "\r\n";
			textBoxVertexInfos.Text += " Last Cut Dot = " + V.m_lastCutDot + "\r\n";
			textBoxVertexInfos.Text += " Plane Index = " + V.m_planeIndex + "\r\n";
		}

		private void buttonReload_Click(object sender, EventArgs e)
		{
			m_Device.ReloadModifiedShaders();
		}

		void	DrawLine( float3 _start, float3 _end, float3 _ortho, float _thickness, float4 _color ) {
			if ( _thickness <= 0.0f ) {
				// Compute the world size of a pixel for the farthest extremity of the line
				float3	wsCameraPos = (float3) m_Camera.World2Camera.GetRow( 3 );
				float3	wsDir = _end - _start;
				float	t = -wsDir.Dot( _start - wsCameraPos ) / wsDir.Dot( wsDir );
						t = Math.Max( 0.0f, Math.Min( 1.0f, t ) );
				float3	wsNearest = _start + t * wsDir;
				float4	csNearest = new float4( wsNearest, 1 ) * m_Camera.World2Camera;
				float	worldSize = 2.0f * (float) Math.Tan( 0.5f * m_Camera.PerspectiveFOV ) * csNearest.z;
				_thickness *= -worldSize / panelOutput.Height;
			}

			m_CB_Line.m._wsPosition0 = _start;
			m_CB_Line.m._thickness = _thickness;
			m_CB_Line.m._wsPosition1 = _end;
			m_CB_Line.m._wsOrtho = _ortho;
			m_CB_Line.m._color = _color;
			m_CB_Line.UpdateData();

			m_Shader_Line.Use();
			m_Prim_Quad.Render( m_Shader_Line );
		}

		void	DrawPlane( float3 _position, float3 _normal, float3 _tangent, float _sizeTop, float _sizeBottom, float _sizeY, float4 _color, bool _circle ) {
			m_CB_Plane.m._wsPosition = _position;
			m_CB_Plane.m._wsNormal = _normal;
			m_CB_Plane.m._wsTangent = _tangent;
			m_CB_Plane.m._sizeX0 = _sizeTop;
			m_CB_Plane.m._sizeX1 = _sizeBottom;
			m_CB_Plane.m._sizeY = _sizeY;
			m_CB_Plane.m._color = _color;
			m_CB_Plane.m._flags = (uint) (_circle ? 1 : 0);
			m_CB_Plane.UpdateData();

			m_Shader_Plane.Use();
			m_Prim_Quad.Render( m_Shader_Plane );
		}

		void	DrawPoint( float3 _position, float _size, float4 _color ) {
			if ( _size <= 0.0f ) {
				// Compute the world size of a pixel for the given position
				float4	csPos = new float4( _position, 1 ) * m_Camera.World2Camera;
				float	worldSize = 2.0f * (float) Math.Tan( 0.5f * m_Camera.PerspectiveFOV ) * csPos.z;
				_size *= -worldSize / panelOutput.Height;
			}

			float3	wsNormal = -(float3) m_Camera.Camera2World.GetRow( 2 );
			float3	wsTangent = (float3) m_Camera.Camera2World.GetRow( 0 );

			DrawPlane( _position, wsNormal, wsTangent, _size, _size, _size, _color, true );
		}

		void	DrawArrow( float3 _start, float3 _end, float3 _ortho, float4 _color ) {
			DrawLine( _start, _end, _ortho, -1.0f, _color );

			float3	dir = _end - _start;
			float	size = dir.Length;
					dir /= size;

			size *= 0.1f;

			DrawPlane( _end - 0.5f * size * dir, _ortho, dir.Cross( _ortho ), 0.0f, 0.5f * size, size, _color, false );
		}

		float4	Lerp( float4 a, float4 b, float t ) {
			return a + t * (b-a);
		}
		float4	SelectedCellColor( float4 _in ) {
			return Lerp( _in, new float4( 1, 1, 0, 1 ), m_cycle );
		}
		float4	SelectedPlaneColor( float4 _in ) {
			return Lerp( _in, new float4( 0, 1, 0, 1 ), m_cycle );
		}
		float4	SelectedLineColor( float4 _in ) {
			return Lerp( _in, new float4( 0, 1, 1, 1 ), m_cycle2 );
		}
		float4	SelectedVertexColor( float4 _in ) {
			return Lerp( _in, new float4( 1, 0, 1, 1 ), m_cycle3 );
		}
		float4	FlashError( float4 _in ) {
			return Lerp( _in, new float4( 10, 0, 0, 1 ), m_cycleError );
		}

		private void buttonClean_Click(object sender, EventArgs e) {
			m_cells.Clear();
			m_lastReadVersion = 0;
			UpdatePlanes();
		}

		private void integerTrackbarControlCell_ValueChanged(IntegerTrackbarControl _Sender, int _FormerValue)
		{
			UpdatePlaneInfos();
		}

		private void integerTrackbarControlPlane_ValueChanged(IntegerTrackbarControl _Sender, int _FormerValue)
		{
			UpdatePlaneInfos();
		}

		private void integerTrackbarControlLine_ValueChanged(IntegerTrackbarControl _Sender, int _FormerValue)
		{
			UpdateLineInfos();
		}

		private void integerTrackbarControlVertex_ValueChanged(IntegerTrackbarControl _Sender, int _FormerValue)
		{
			UpdateVertexInfos();
		}
	}
}
