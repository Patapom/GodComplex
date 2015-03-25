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
		struct CB_Global {
//			public float4x4		m_World2Camera;
			public float4x4		m_World2Proj;
			public uint			m_ScreenWidth;
			public uint			m_ScreenHeight;

			public uint			m_ImageWidth;
			public uint			m_ImageHeight;
			public uint			m_ImageDepth;
			public uint			m_ImageType;

			public float		m_MipLevel;
		};

		ConstantBuffer< CB_Global >		m_CB_Global;

		Shader							m_Shader_Render2D;

		Texture2D						m_Tex2D = null;
		Texture2D						m_TexCube = null;
		Texture3D						m_Tex3D = null;

//		Primitive						m_Prim_Cube;

		public ViewerForm( BImage _Image )
		{
			InitializeComponent();

//TransparencyKey = SystemColors.Control;

			// Setup device
			m_Device = new Device();
			m_Device.Init( Handle, false, true );

			m_CB_Global = new ConstantBuffer< CB_Global >( m_Device, 0 );

			// Create shaders
			m_Shader_Render2D = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"./Shaders/Render2D.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

			// Create the texture
			try
			{
				if ( _Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_2D ) {
					m_Tex2D = _Image.CreateTexture2D( m_Device );

					m_CB_Global.m.m_ImageWidth = (uint) m_Tex2D.Width;
					m_CB_Global.m.m_ImageHeight = (uint) m_Tex2D.Height;
					m_CB_Global.m.m_ImageDepth = (uint) m_Tex2D.ArraySize;
					m_CB_Global.m.m_ImageType = 0;

					integerTrackbarControlMipLevel.RangeMax = m_Tex2D.MipLevelsCount;
					integerTrackbarControlMipLevel.VisibleRangeMax = m_Tex2D.MipLevelsCount;

				}
				else if ( _Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_CUBIC ) {
					m_TexCube = _Image.CreateTextureCube( m_Device );

					m_CB_Global.m.m_ImageWidth = (uint) m_Tex2D.Width;
					m_CB_Global.m.m_ImageHeight = (uint) m_Tex2D.Height;
					m_CB_Global.m.m_ImageDepth = (uint) m_Tex2D.ArraySize;
					m_CB_Global.m.m_ImageType = 1;
				}
				else if ( _Image.m_Opts.m_type == BImage.ImageOptions.TYPE.TT_3D ) {
					m_Tex3D = _Image.CreateTexture3D( m_Device );
				}
			}
			catch ( Exception _e )
			{
				MessageBox.Show( this, "Failed to create a valid texture from the image:\r\n\r\n" + _e.Message, "BImage Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			if ( m_Tex2D != null )
				m_Tex2D.Dispose();
			if ( m_TexCube != null )
				m_TexCube.Dispose();
			if ( m_Tex3D != null )
				m_Tex3D.Dispose();

			m_Shader_Render2D.Dispose();
			m_CB_Global.Dispose();
			m_Device.Dispose();

			base.OnFormClosing( e );
		}


		bool	m_bHasRendered = false;
		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Update camera
			m_CB_Global.m.m_ScreenWidth = (uint) Width;
			m_CB_Global.m.m_ScreenHeight = (uint) Height;
			m_CB_Global.m.m_MipLevel = (float) integerTrackbarControlMipLevel.Value;
			m_CB_Global.UpdateData();

			// Clear
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );
			m_Device.Clear( float4.Zero );

			// Render
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_Shader_Render2D.Use() ) {

				if ( m_Tex2D != null )
					m_Tex2D.SetPS( 0 );
				if ( m_TexCube != null )
					m_TexCube.SetPS( 1 );
				if ( m_Tex3D != null )
					m_Tex3D.SetPS( 2 );

				m_Device.RenderFullscreenQuad( m_Shader_Render2D );
			}

			m_Device.Present( false );
			m_bHasRendered = true;
		}

		private void ViewerForm_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode == Keys.Escape )
				Close();
		}
	}
}
