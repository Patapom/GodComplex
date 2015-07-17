using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace GenerateTranslucencyMap
{
	public partial class ViewerForm : Form
	{
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBDisplay {
			public uint		_Width;
			public uint		_Height;
			public float	_Time;
		}

		private GeneratorForm				m_Owner;

		private ConstantBuffer<CBDisplay>	m_CB_Display;
		private Shader						m_PS_Display;

		private DateTime					m_StartTime = DateTime.Now;

		private Device		Device {
			get { return m_Owner.m_Device; }
		}

		public ViewerForm( GeneratorForm _Owner )
		{
			InitializeComponent();
			m_Owner = _Owner;
		}

		public void Init()
		{
			#if DEBUG
				m_PS_Display = new Shader( Device, new ShaderFile( new System.IO.FileInfo( "./Shaders/Display.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			#else
				m_PS_Display = Shader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Display.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
			#endif

			m_CB_Display = new ConstantBuffer<CBDisplay>( Device, 0 );
			m_CB_Display.m._Width = (uint) Width;
			m_CB_Display.m._Height = (uint) Height;

			Application.Idle += new EventHandler( Application_Idle );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) ) {
				m_CB_Display.Dispose();
				m_PS_Display.Dispose();

				components.Dispose();
			}
			base.Dispose( disposing );
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( !Visible )
				return;

			DateTime	CurrentTime = DateTime.Now;
			m_CB_Display.m._Time = (float) (CurrentTime - m_StartTime).TotalSeconds;
			m_CB_Display.UpdateData();

			Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			Device.SetRenderTarget( Device.DefaultTarget, null );

			if ( m_PS_Display.Use() ) {

 				if ( m_Owner.m_TextureSourceThickness != null )
 					m_Owner.m_TextureSourceThickness.SetPS( 0 );
 				if ( m_Owner.m_TextureSourceNormal != null )
 					m_Owner.m_TextureSourceNormal.SetPS( 1 );
 				if ( m_Owner.m_TextureSourceTransmittance != null )
 					m_Owner.m_TextureSourceTransmittance.SetPS( 2 );
 				if ( m_Owner.m_TextureSourceAlbedo != null )
 					m_Owner.m_TextureSourceAlbedo.SetPS( 3 );
 				if ( m_Owner.m_TextureSourceVisibility != null )
 					m_Owner.m_TextureSourceVisibility.SetPS( 4 );

				Device.RenderFullscreenQuad( m_PS_Display );
			}

			Device.Present( false );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			e.Cancel = true;
			Visible = false;	// Only hide...
			base.OnFormClosing( e );
		}
	}
}
