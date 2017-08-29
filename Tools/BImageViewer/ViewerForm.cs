using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Renderer;
using SharpMath;

namespace BImageViewer
{
	public partial class ViewerForm : Form {
		Device		m_device = null;

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

			public float		m_Time;
			public float		m_MipLevel;
			public float		m_Exposure;
		};

		ConstantBuffer< CB_Global >		m_CB_Global;

		Shader							m_shader_Render2D;

		Texture2D						m_Tex2D = null;
		Texture2D						m_TexCube = null;
		Texture3D						m_Tex3D = null;

		DateTime						m_startTime = DateTime.Now;
//		Primitive						m_Prim_Cube;

		public ViewerForm( ArkaneService.BImage _Image ) {
			InitializeComponent();

//TransparencyKey = SystemColors.Control;

			// Setup device
			m_device = new Device();
			m_device.Init( Handle, false, true );

			m_CB_Global = new ConstantBuffer< CB_Global >( m_device, 0 );

			// Create shaders
			m_shader_Render2D = new Shader( m_device, new System.IO.FileInfo( @"./Shaders/Render2D.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

			// Create the texture
			try {
				if ( _Image.m_opts.m_type == ArkaneService.BImage.ImageOptions.TYPE.TT_2D ) {
					m_Tex2D = _Image.CreateTexture2D( m_device );

					m_CB_Global.m.m_ImageWidth = (uint) m_Tex2D.Width;
					m_CB_Global.m.m_ImageHeight = (uint) m_Tex2D.Height;
					m_CB_Global.m.m_ImageDepth = (uint) m_Tex2D.ArraySize;
					m_CB_Global.m.m_ImageType = 0;

					integerTrackbarControlMipLevel.RangeMax = (int) m_Tex2D.MipLevelsCount;
					integerTrackbarControlMipLevel.VisibleRangeMax = (int) m_Tex2D.MipLevelsCount;

				} else if ( _Image.m_opts.m_type == ArkaneService.BImage.ImageOptions.TYPE.TT_CUBIC ) {
					m_TexCube = _Image.CreateTextureCube( m_device );

					m_CB_Global.m.m_ImageWidth = (uint) m_TexCube.Width;
					m_CB_Global.m.m_ImageHeight = (uint) m_TexCube.Height;
					m_CB_Global.m.m_ImageDepth = (uint) m_TexCube.ArraySize;
					m_CB_Global.m.m_ImageType = 1;

					integerTrackbarControlMipLevel.RangeMax = (int) m_TexCube.MipLevelsCount;
					integerTrackbarControlMipLevel.VisibleRangeMax = (int) m_TexCube.MipLevelsCount;

				} else if ( _Image.m_opts.m_type == ArkaneService.BImage.ImageOptions.TYPE.TT_3D ) {
					m_Tex3D = _Image.CreateTexture3D( m_device );
				}

				// Enable EV manipulation for HDR images
				bool	showExposure = _Image.m_opts.m_format.m_type == ArkaneService.BImage.PixelFormat.Type.FLOAT;
				labelEV.Visible = showExposure;
				floatTrackbarControlEV.Visible = showExposure;

			} catch ( Exception _e ) {
				MessageBox.Show( this, "Failed to create a valid texture from the image:\r\n\r\n" + _e.Message, "BImage Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			if ( m_Tex2D != null )
				m_Tex2D.Dispose();
			if ( m_TexCube != null )
				m_TexCube.Dispose();
			if ( m_Tex3D != null )
				m_Tex3D.Dispose();

			m_shader_Render2D.Dispose();
			m_CB_Global.Dispose();
			m_device.Dispose();

			base.OnFormClosing( e );
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			// Update camera
			m_CB_Global.m.m_ScreenWidth = (uint) Width;
			m_CB_Global.m.m_ScreenHeight = (uint) Height;
			m_CB_Global.m.m_Time = (float) (DateTime.Now - m_startTime).TotalSeconds;
			m_CB_Global.m.m_MipLevel = (float) integerTrackbarControlMipLevel.Value;
			m_CB_Global.m.m_Exposure = (float) Math.Pow( 2.0, floatTrackbarControlEV.Visible ? (float) floatTrackbarControlEV.Value : 0.0f );
			m_CB_Global.UpdateData();

			// Clear
			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );
			m_device.Clear( float4.Zero );

			// Render
			m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_Render2D.Use() ) {

				if ( m_Tex2D != null )
					m_Tex2D.SetPS( 0 );
				if ( m_TexCube != null )
					m_TexCube.SetPS( 1 );
				if ( m_Tex3D != null )
					m_Tex3D.SetPS( 2 );

				m_device.RenderFullscreenQuad( m_shader_Render2D );
			}

			m_device.Present( false );
		}

		private void ViewerForm_KeyDown( object sender, KeyEventArgs e ) {
			if ( e.KeyCode == Keys.Escape )
				Close();
		}
	}
}
