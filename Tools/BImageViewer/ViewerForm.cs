using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace BImageViewer
{
	public partial class ViewerForm : Form
	{
		Device		m_Device = null;

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Camera {
//			public float4x4		m_World2Camera;
			public float4x4		m_World2Proj;
		};

		ConstantBuffer< CB_Camera >		m_CB_Camera;

		Shader							m_Shader_Render2D;

		Primitive						m_Prim_Cube;

		public ViewerForm( BImage _Image )
		{
			InitializeComponent();

			// Setup device
			m_Device = new Device();
			m_Device.Init( Handle, false, true );

			m_CB_Camera = new ConstantBuffer< CB_Camera >( m_Device, 0 );

			// Create shaders
			m_Shader_Render2D = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"./Shaders/Render2D.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			m_Shader_Render2D.Dispose();
			m_CB_Camera.Dispose();
			m_Device.Dispose();

			base.OnFormClosing( e );
		}


		bool	m_bHasRendered = false;
		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Update camera
			m_CB_Camera.UpdateData();

			// Clear
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );
			m_Device.Clear( float4.Zero );

			// Render
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_Shader_Render2D.Use() ) {
				m_Device.RenderFullscreenQuad( m_Shader_Render2D );
			}

			m_Device.Present( false );
			m_bHasRendered = true;
		}
	}
}
