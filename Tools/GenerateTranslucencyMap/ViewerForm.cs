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
			public float	_Time;
		}

		private Device						m_Device;
		private ConstantBuffer<CBDisplay>	m_CB_Display;
		private Shader						m_PS_Display;

		private DateTime					m_StartTime = DateTime.Now;

		public ViewerForm()
		{
			InitializeComponent();
		}

		public void Init( Device _Device )
		{
			m_Device = _Device;

			#if DEBUG
				m_PS_Display = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "./Shaders/Display.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			#else
				m_PS_Display = Shader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Display.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
			#endif

			m_CB_Display = new ConstantBuffer<CBDisplay>( m_Device, 0 );

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

			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			m_Device.SetRenderTarget( m_Device.DefaultTarget, null );

			if ( m_PS_Display.Use() ) {



				m_Device.RenderFullscreenQuad( m_PS_Display );
			}

			m_Device.Present( false );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			e.Cancel = true;
			Visible = false;	// Only hide...
			base.OnFormClosing( e );
		}
	}
}
