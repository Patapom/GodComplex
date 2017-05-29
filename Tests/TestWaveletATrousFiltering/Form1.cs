#define BISOU

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;
using Renderer;

namespace TestWaveletATrousFiltering
{
	public partial class Form1 : Form {

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Global {
			public uint		resolutionX;
			public uint		resolutionY;
			public float	time;
		}

		#endregion

		Device			m_device = new Device();

		ConstantBuffer< CB_Global >		m_CB_Global;

		Shader			m_shader_postProcess;

		DateTime		m_startTime;


		public Form1() {
			InitializeComponent();

			try {
				m_device.Init( panelOutput.Handle, false, true );

				m_shader_postProcess = new Shader( m_device, new System.IO.FileInfo( "./Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

				m_CB_Global = new ConstantBuffer< CB_Global >( m_device, 0 );

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Error", "An exception occurred while creating DX structures:\r\n" + _e.Message );
			}

			m_startTime = DateTime.Now;
			Application.Idle += Application_Idle;
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			Device	D = m_device;
			m_device = null;
			D.Dispose();
		}

		void Application_Idle(object sender, EventArgs e) {
			if ( m_device == null )
				return;

			DateTime	currentTime = DateTime.Now;
			float		totalTime = (float) (currentTime - m_startTime).TotalSeconds;
			m_CB_Global.m.resolutionX = (uint) panelOutput.Width;
			m_CB_Global.m.resolutionY = (uint) panelOutput.Height;
			m_CB_Global.m.time = totalTime;
			m_CB_Global.UpdateData();

			if ( m_shader_postProcess.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );
				m_device.RenderFullscreenQuad( m_shader_postProcess );
			}

			m_device.Present( false );
		}
	}
}
