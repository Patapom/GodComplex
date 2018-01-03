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

namespace TestGroundTruthAOFitting
{
	public partial class DemoForm : Form {

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public uint		_flags;
			public float	_reflectance;
			public float4	_SH0;
			public float4	_SH1;
			public float4	_SH2;
			public float4	_SH3;
			public float4	_SH4;
			public float4	_SH5;
			public float4	_SH6;
			public float4	_SH7;
			public float4	_SH8;
		}

		Device		m_device = new Device();

		ConstantBuffer< CB_Main >	m_CB_Main = null;
		Shader		m_shader_Render = null;

		public DemoForm() {
			InitializeComponent();
			InitD3D();
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || !Visible )
				return;

			if ( m_tex_Height == null
				|| m_tex_Normal == null
				|| m_tex_AO == null )
				return;

			if ( !m_shader_Render.Use() )
				return;

			m_tex_Height.Set( 0 );
			m_tex_Normal.Set( 1 );
			m_tex_AO.Set( 2 );

			m_CB_Main.m._flags = (uint) (checkBoxEnableAO.Checked ? 1 : 0);
			m_CB_Main.m._reflectance = floatTrackbarControlReflectance.Value;
			m_CB_Main.m._SH0.Set( m_rotatedLightSH[0], 0 );
			m_CB_Main.m._SH1.Set( m_rotatedLightSH[1], 0 );
			m_CB_Main.m._SH2.Set( m_rotatedLightSH[2], 0 );
			m_CB_Main.m._SH3.Set( m_rotatedLightSH[3], 0 );
			m_CB_Main.m._SH4.Set( m_rotatedLightSH[4], 0 );
			m_CB_Main.m._SH5.Set( m_rotatedLightSH[5], 0 );
			m_CB_Main.m._SH6.Set( m_rotatedLightSH[6], 0 );
			m_CB_Main.m._SH7.Set( m_rotatedLightSH[7], 0 );
			m_CB_Main.m._SH8.Set( m_rotatedLightSH[8], 0 );
			m_CB_Main.UpdateData();

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			m_device.SetRenderTarget( m_device.DefaultTarget, null );

			m_device.RenderFullscreenQuad( m_shader_Render );

			m_device.Present( false );
		}

		#region Inputs Setup

		Texture2D	CreateTextureFromImage( ImageFile _image ) {
			if ( _image.PixelFormat == PIXEL_FORMAT.BGR8 ) {
//				ImageFile	convertedImage = new ImageFile( _image.Width, _image.Height, PIXEL_FORMAT.RG);
				ImageFile	convertedImage = new ImageFile();
				convertedImage.ConvertFrom( _image, PIXEL_FORMAT.BGRA8 );
				_image = convertedImage;
			}

//			m_tex_Height = new Texture2D( m_device, m_imageHeight.Width, m_imageHeight.Height, 1, 1, ImageUtility.PIXEL_FORMAT.R8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { content } );
			ImagesMatrix	matrix = new ImagesMatrix( new ImageFile[,] { { _image } } );
			Texture2D		result = new Texture2D( m_device, matrix, ImageUtility.COMPONENT_FORMAT.UNORM );
			return result;
		}

		Texture2D	m_tex_Height = null;
		ImageFile	m_imageHeight = null;
		public ImageFile	ImageHeight {
			get { return m_imageHeight; }
			set {
				if ( value == m_imageHeight )
					return;

				if ( m_tex_Height != null )
					m_tex_Height.Dispose();
				m_tex_Height = null;

				m_imageHeight = value;
				if ( m_imageHeight == null )
					return;

				m_tex_Height = CreateTextureFromImage( m_imageHeight );
			}
		}
		Texture2D	m_tex_Normal = null;
		ImageFile	m_imageNormal = null;
		public ImageFile	ImageNormal {
			get { return m_imageNormal; }
			set {
				if ( value == m_imageNormal )
					return;

				if ( m_tex_Normal != null )
					m_tex_Normal.Dispose();
				m_tex_Normal = null;

				m_imageNormal = value;
				if ( m_imageNormal == null )
					return;

				m_tex_Normal = CreateTextureFromImage( m_imageNormal );
			}
		}
		Texture2D	m_tex_AO = null;
		float2[,]	m_AOValues = null;
		public float2[,]	AOValues {
			get { return m_AOValues; }
			set {
				if ( value == m_AOValues )
					return;

				if ( m_tex_AO != null )
					m_tex_AO.Dispose();
				m_tex_AO = null;

				m_AOValues = value;
				if ( m_AOValues == null )
					return;

				uint	W = (uint) m_AOValues.GetLength( 0 );
				uint	H = (uint) m_AOValues.GetLength( 1 );
				PixelsBuffer	content = new PixelsBuffer( W*H*2*4 );
				float	maxAO = 0.0f;
				using ( System.IO.BinaryWriter Wr = content.OpenStreamWrite() ) {
					for ( uint Y=0; Y < H; Y++ )
						for ( uint X=0; X < W; X++ ) {
							float	AO = m_AOValues[X,Y].x / Mathf.TWOPI;
							float	E0 = m_AOValues[X,Y].y;
							maxAO = Mathf.Max( maxAO, AO );
//							byte	V = (byte) (255.0f * Mathf.Saturate( AO ));
							Wr.Write( AO );
							Wr.Write( E0 );
						}
				}

//				m_tex_AO = new Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.R8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, new PixelsBuffer[] { content } );
				m_tex_AO = new Texture2D( m_device, W, H, 1, 1, ImageUtility.PIXEL_FORMAT.RG32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
			}
		}
// 		float[][,]	m_arrayOfIlluminanceValues = null;
// 		float[][,]	arrayOfIlluminanceValues {
// 			get { return m_arrayOfIlluminanceValues; }
// 			set {
// 				if ( value == m_arrayOfIlluminanceValues )
// 					return;
// 			}
// 		}

		public void	SetImages( ImageFile _imageHeight, ImageFile _imageSourceNormal, float2[,] _AOValues ) {
			ImageHeight = _imageHeight;
			ImageNormal = _imageSourceNormal;
			AOValues = _AOValues;
		}

		#endregion

		#region D3D Management

		void	InitD3D() {
			try {
				m_device.Init( panelOutput.Handle, false, true );

				m_shader_Render = new Shader( m_device, new System.IO.FileInfo( "Shaders/Render.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

				m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
				m_CB_Main.m._resolutionX = (uint) panelOutput.Width;
				m_CB_Main.m._resolutionY = (uint) panelOutput.Height;

				InitEnvironmentSH();

				Application.Idle += Application_Idle;
			} catch ( Exception ) {
				m_device = null;
			}
		}

		void	DisposeD3D() {
			if ( m_tex_AO != null )
				m_tex_AO.Dispose();
			if ( m_tex_Height != null )
				m_tex_Height.Dispose();
			if ( m_tex_Normal != null )
				m_tex_Normal.Dispose();

			m_CB_Main.Dispose();
			m_shader_Render.Dispose();
			m_device.Dispose();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) ) {
				components.Dispose();
			}

			base.Dispose( disposing );
		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			base.OnFormClosing( e );
			if ( e.CloseReason == CloseReason.UserClosing ) {
				// Hide instead
				e.Cancel = true;
				Visible = false;
				return;
			}

			DisposeD3D();
		}

		#endregion

		#region Manipulation

		Quat			m_lightQuat = new Quat( new AngleAxis( 0.0f, float3.UnitZ ) );
		float3x3		m_lightRotation = float3x3.Identity;
// 		float3x3		m_lightRotation = (float3x3) m_lightQuat;

		float3[]		m_lightSH = new float3[9];
		double[,]		m_SHRotation = new double[9,9];
		float3[]		m_rotatedLightSH = new float3[9];

		MouseButtons	m_buttonDown = MouseButtons.None;
		Point			m_buttonDownPosition;
		Quat			m_buttonDownLightQuat;

		private void panelOutput_MouseDown( object sender, MouseEventArgs e ) {
			m_buttonDownPosition = e.Location;
			m_buttonDownLightQuat = new Quat( m_lightQuat );
			m_buttonDown |= e.Button;
//			Capture = true;
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs e ) {
			m_buttonDown &= ~e.Button;
			if ( m_buttonDown == MouseButtons.None )
				Capture = false;
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs e ) {
			if ( m_buttonDown == MouseButtons.None )
				return;

			if ( (m_buttonDown & System.Windows.Forms.MouseButtons.Left) != 0 ) {
				float	Dx = e.Location.X - m_buttonDownPosition.X;
				float	Dy = m_buttonDownPosition.Y - e.Location.Y;
				Quat	rotY = new Quat( new AngleAxis( 0.01f * Dx, float3.UnitY ) );
				Quat	rotX = new Quat( new AngleAxis( 0.01f * Dy, float3.UnitX ) );
				m_lightQuat = m_buttonDownLightQuat * rotX * rotY;
				m_lightRotation = (float3x3) m_lightQuat;
				UpdateSH();
			}
		}

		void	UpdateSH() {
			SphericalHarmonics.SHFunctions.BuildRotationMatrix( m_lightRotation, m_SHRotation, 3 );
			SphericalHarmonics.SHFunctions.Rotate( m_lightSH, m_SHRotation, m_rotatedLightSH, 3 );
		}

		/// <summary>
		/// Initializes the SH coeffs for the environment
		/// </summary>
		void	InitEnvironmentSH() {
/* Cosine lobe
			float3	dir = float3.UnitZ;

			const float CosineA0 = Mathf.PI;
			const float CosineA1 = (2.0f * Mathf.PI) / 3.0f;
			const float CosineA2 = Mathf.PI * 0.25f;

			// Band 0
			m_lightSH[0] = 0.282095f * CosineA0 * float3.One;

			// Band 1
			m_lightSH[1] = 0.488603f * dir.y * CosineA1 * float3.One;
			m_lightSH[2] = 0.488603f * dir.z * CosineA1 * float3.One;
			m_lightSH[3] = 0.488603f * dir.x * CosineA1 * float3.One;

			// Band 2
			m_lightSH[4] = 1.092548f * dir.x * dir.y * CosineA2 * float3.One;
			m_lightSH[5] = 1.092548f * dir.y * dir.z * CosineA2 * float3.One;
			m_lightSH[6] = 0.315392f * (3.0f * dir.z * dir.z - 1.0f) * CosineA2 * float3.One;
			m_lightSH[7] = 1.092548f * dir.x * dir.z * CosineA2 * float3.One;
			m_lightSH[8] = 0.546274f * (dir.x * dir.x - dir.y * dir.y) * CosineA2 * float3.One;
*/

			// Ennis House
			m_lightSH[0] = new float3( 4.52989505453915f, 4.30646452463535f, 4.51721251492342f );
			m_lightSH[1] = new float3( 0.387870406203612f, 0.384965748870704f, 0.395325521894004f );
			m_lightSH[2] = new float3( 1.05692530696077f, 1.33538156449369f, 1.82393006020369f );
			m_lightSH[3] = new float3( 6.18680912868925f, 6.19927929741711f, 6.6904772608617f );
			m_lightSH[4] = new float3( 0.756169905467733f, 0.681053631625203f, 0.677636982521888f );
			m_lightSH[5] = new float3( 0.170950637080382f, 0.1709443393056f, 0.200437519088333f );
			m_lightSH[6] = new float3( -3.59338856195816f, -3.37861193089806f, -3.30850268192343f );
			m_lightSH[7] = new float3( 2.65318898618603f, 2.97074561577712f, 3.82264536047523f );
			m_lightSH[8] = new float3( 6.07079134655854f, 6.05819330192308f, 6.50325529149908f );

 			// Grace Cathedral
// 			m_lightSH[0] = new float3( 0.933358105849532f, 0.605499186927096f, 0.450999072970855f );
// 			m_lightSH[1] = new float3( 0.0542981143130068f, 0.0409598475963159f, 0.0355377036564806f );
// 			m_lightSH[2] = new float3( 0.914255336642483f, 0.651103534810611f, 0.518065694132826f );
// 			m_lightSH[3] = new float3( 0.238207071886099f, 0.14912965904707f, 0.0912559191766972f );
// 			m_lightSH[4] = new float3( 0.0321476755042544f, 0.0258939812282057f, 0.0324159089991572f );
// 			m_lightSH[5] = new float3( 0.104707893908821f, 0.0756648975030993f, 0.0749934936107284f );
// 			m_lightSH[6] = new float3( 1.27654512826622f, 0.85613828921136f, 0.618241442250845f );
// 			m_lightSH[7] = new float3( 0.473237767573493f, 0.304160108872238f, 0.193304867770535f );
// 			m_lightSH[8] = new float3( 0.143726445535245f, 0.0847402441253633f, 0.0587779174281925f );

			SphericalHarmonics.SHFunctions.FilterHanning( m_lightSH, 1.8f );
		}

		#endregion
	}
}
