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
		struct CB_ComputeIrradiance {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public float	_texelSize_mm;		// Size of a texel (in millimeters)
			public float	_displacement_mm;	// Max displacement value encoded by the height map (in millimeters)

			public float3	_rho;				// Global surface reflectance (should come from a texture though)
			float			__PAD;

			public float4	_debugValue;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_resolutionX;
			public uint		_resolutionY;
			public uint		_flags;
			public uint		_bouncesCount;

			public float3	_rho;
			public float	_exposure;

			public float4	_debugValue;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_SH {
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

		GeneratorForm	m_owner = null;

		Device			m_device = new Device();

		ConstantBuffer< CB_Main >	m_CB_Main = null;
		ConstantBuffer< CB_SH >		m_CB_SH = null;
		ConstantBuffer< CB_ComputeIrradiance >	m_CB_ComputeIrradiance = null;

		Shader			m_shader_ComputeIndirectIrradiance = null;
		Shader			m_shader_FilterIndirectIrradiance = null;
		Shader			m_shader_Render = null;

		Texture2D		m_tex_BlueNoise = null;
		Texture2D		m_tex_Irradiance0 = null;
		Texture2D		m_tex_Irradiance1 = null;
		Texture2D		m_tex_ComputedBentCone = null;

		public DemoForm( GeneratorForm _owner ) {
			m_owner = _owner;
			InitializeComponent();
			InitD3D();
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || !Visible )
				return;

			if ( m_tex_Height == null
				|| m_tex_Normal == null )
				return;

			//////////////////////////////////////////////////////////////////////////
			// Setup variables
			m_tex_Height.Set( 0 );
			m_tex_Normal.Set( 1 );
			if ( m_tex_AO != null )
				m_tex_AO.Set( 2 );
			if ( m_tex_GroundTruth != null )
				m_tex_GroundTruth.Set( 3 );
			if ( m_tex_BentCone != null )
				m_tex_BentCone.Set( 4 );
			m_tex_BlueNoise.Set( 10 );

			m_CB_Main.m._flags = 0U;
			if ( radioButtonOn.Checked )
				m_CB_Main.m._flags |= 1U;
			else if ( radioButtonBentCone.Checked )
				m_CB_Main.m._flags |= 2U;
			else if ( radioButtonSimul.Checked )
				m_CB_Main.m._flags |= 3U;
			else if ( radioButtonGroundTruth.Checked )
				m_CB_Main.m._flags |= 4U;
			m_CB_Main.m._bouncesCount = (uint) integerTrackbarControlBouncesCount.Value;
			m_CB_Main.m._flags |= checkBoxDiff.Checked ? 0x10U : 0x0U;
			m_CB_Main.m._flags |= checkBoxMix.Checked ? 0x20U : 0x0U;

			m_CB_Main.m._rho = floatTrackbarControlReflectance.Value * new float3( 1.0f, 0.9f, 0.7f );
//m_CB_Main.m._rho = floatTrackbarControlReflectance.Value * float3.One;

			m_CB_Main.m._exposure = floatTrackbarControlExposure.Value;
			m_CB_Main.m._debugValue.Set( floatTrackbarControlDebug0.Value, floatTrackbarControlDebug1.Value, floatTrackbarControlDebug2.Value, floatTrackbarControlDebug3.Value );
			m_CB_Main.UpdateData();

			m_CB_SH.m._SH0.Set( m_rotatedLightSH[0], 0 );
			m_CB_SH.m._SH1.Set( m_rotatedLightSH[1], 0 );
			m_CB_SH.m._SH2.Set( m_rotatedLightSH[2], 0 );
			m_CB_SH.m._SH3.Set( m_rotatedLightSH[3], 0 );
			m_CB_SH.m._SH4.Set( m_rotatedLightSH[4], 0 );
			m_CB_SH.m._SH5.Set( m_rotatedLightSH[5], 0 );
			m_CB_SH.m._SH6.Set( m_rotatedLightSH[6], 0 );
			m_CB_SH.m._SH7.Set( m_rotatedLightSH[7], 0 );
			m_CB_SH.m._SH8.Set( m_rotatedLightSH[8], 0 );
			m_CB_SH.UpdateData();

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			//////////////////////////////////////////////////////////////////////////
			// Compute indirect irradiance & bent cone map
			if ( m_shader_ComputeIndirectIrradiance.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_Irradiance1.GetView(), m_tex_ComputedBentCone.GetView() }, null );
				m_tex_Irradiance0.Set( 5 );

				m_CB_ComputeIrradiance.m._resolutionX = m_tex_Height.Width;
				m_CB_ComputeIrradiance.m._resolutionY = m_tex_Height.Height;
				m_CB_ComputeIrradiance.m._texelSize_mm = m_owner.TextureSize_mm / Math.Max( m_tex_Height.Width, m_tex_Height.Height );
				m_CB_ComputeIrradiance.m._displacement_mm = m_owner.TextureHeight_mm;
				m_CB_ComputeIrradiance.m._rho = m_CB_Main.m._rho;
				m_CB_ComputeIrradiance.m._debugValue.Set( floatTrackbarControlDebug0.Value, floatTrackbarControlDebug1.Value, floatTrackbarControlDebug2.Value, floatTrackbarControlDebug3.Value );
				m_CB_ComputeIrradiance.UpdateData();

				m_device.RenderFullscreenQuad( m_shader_ComputeIndirectIrradiance );

				Texture2D	temp = m_tex_Irradiance0;
				m_tex_Irradiance0 = m_tex_Irradiance1;
				m_tex_Irradiance1 = temp;
			}

			//////////////////////////////////////////////////////////////////////////
			// Render
			if ( m_shader_Render.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );
				m_tex_Irradiance0.Set( 5 );
				m_tex_ComputedBentCone.Set( 6 );

				m_device.RenderFullscreenQuad( m_shader_Render );

				m_tex_Irradiance0.RemoveFromLastAssignedSlots();
				m_tex_ComputedBentCone.RemoveFromLastAssignedSlots();
			}

			m_device.Present( false );
		}

		#region Inputs Setup

		Texture2D	CreateTextureFromImage( ImageFile _image ) {
			if ( _image.PixelFormat == PIXEL_FORMAT.BGR8 ) {
				ImageFile	convertedImage = new ImageFile();
				convertedImage.ConvertFrom( _image, PIXEL_FORMAT.BGRA8 );
				_image = convertedImage;
			}

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

				if ( m_tex_Height != null ) {
					m_tex_Height.Dispose();
					m_tex_Irradiance0.Dispose();
					m_tex_Irradiance1.Dispose();
					m_tex_ComputedBentCone.Dispose();
				}
				m_tex_Height = null;

				m_imageHeight = value;
				if ( m_imageHeight == null )
					return;

				m_tex_Height = CreateTextureFromImage( m_imageHeight );
				m_tex_Irradiance0 = new Texture2D( m_device, m_tex_Height.Width, m_tex_Height.Height, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
				m_tex_Irradiance1 = new Texture2D( m_device, m_tex_Height.Width, m_tex_Height.Height, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
				m_tex_ComputedBentCone = new Texture2D( m_device, m_tex_Height.Width, m_tex_Height.Height, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );

				m_device.Clear( m_tex_Irradiance0, float4.Zero );
				m_device.Clear( m_tex_Irradiance1, float4.Zero );
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

		Texture2D	m_tex_BentCone = null;
		ImageFile	m_imageBentCone = null;
		public ImageFile	ImageBentCone {
			get { return m_imageBentCone; }
			set {
				if ( value == m_imageBentCone )
					return;

				if ( m_tex_BentCone != null )
					m_tex_BentCone.Dispose();
				m_tex_BentCone = null;

				m_imageBentCone = value;
				if ( m_imageBentCone == null )
					return;

				m_tex_BentCone = CreateTextureFromImage( m_imageBentCone );
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
							float	AO = m_AOValues[X,Y].x;
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

// 		Texture2D	m_tex_Illuminance = null;
// 		float[][,]	m_arrayOfIlluminanceValues = null;
// 		public float[][,]	ArrayOfIlluminanceValues {
// 			get { return m_arrayOfIlluminanceValues; }
// 			set {
// 				if ( value == m_arrayOfIlluminanceValues && value != null && m_arrayOfIlluminanceValues != null && value[0] == m_arrayOfIlluminanceValues[0] )
// 					return;
// 
// 				if ( m_tex_Illuminance != null )
// 					m_tex_Illuminance.Dispose();
// 				m_tex_Illuminance = null;
// 
// 				if ( m_arrayOfIlluminanceValues == null )
// 					m_arrayOfIlluminanceValues = new float[value.Length][,];
// 				Array.Copy( value, m_arrayOfIlluminanceValues, value.Length );
// 				if ( m_arrayOfIlluminanceValues == null || m_arrayOfIlluminanceValues[0] == null )
// 					return;
// 
// 				uint	bouncesCount = (uint) m_arrayOfIlluminanceValues.Length;
// 				uint	W = (uint) m_arrayOfIlluminanceValues[0].GetLength( 0 );
// 				uint	H = (uint) m_arrayOfIlluminanceValues[0].GetLength( 1 );
// 				PixelsBuffer[]	arraySlices = new PixelsBuffer[bouncesCount];
// 				for ( uint bounceIndex=0; bounceIndex < bouncesCount; bounceIndex++ ) {
// 					PixelsBuffer	content = new PixelsBuffer( W*H*4 );
// 					arraySlices[bounceIndex] = content;
// 					float[,]		illuminanceValues = m_arrayOfIlluminanceValues[bounceIndex];
// 					using ( System.IO.BinaryWriter Wr = content.OpenStreamWrite() ) {
// 						for ( uint Y=0; Y < H; Y++ )
// 							for ( uint X=0; X < W; X++ ) {
// 								float	E = illuminanceValues[X,Y];
// 								Wr.Write( E );
// 							}
// 					}
// 				}
// 
// 				m_tex_Illuminance = new Texture2D( m_device, W, H, (int) bouncesCount, 1, ImageUtility.PIXEL_FORMAT.R32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, arraySlices );
// 			}
// 		}

		public void	SetImages( ImageFile _imageHeight, ImageFile _imageSourceNormal, float2[,] _AOValues, float[][,] _arrayOfIlluminanceValues ) {
			ImageHeight = _imageHeight;
			ImageNormal = _imageSourceNormal;
			AOValues = _AOValues;
// 			ArrayOfIlluminanceValues = _arrayOfIlluminanceValues;
		}

		#endregion

		#region D3D Management

		void	InitD3D() {
			try {
				m_device.Init( panelOutput.Handle, false, true );

				m_shader_Render = new Shader( m_device, new System.IO.FileInfo( "Shaders/Demo/Render.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
//				m_shader_ComputeIndirectMap = new Shader( m_device, new System.IO.FileInfo( "Shaders/Demo/ComputeIndirectMap.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
				m_shader_ComputeIndirectIrradiance = new Shader( m_device, new System.IO.FileInfo( "Shaders/Demo/ComputeIndirectIrradiance2.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
//				m_shader_FilterIndirectIrradiance = new Shader( m_device, new System.IO.FileInfo( "Shaders/Demo/FilterIndirectIrradiance.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );

				m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
				m_CB_Main.m._resolutionX = (uint) panelOutput.Width;
				m_CB_Main.m._resolutionY = (uint) panelOutput.Height;

				m_CB_SH = new ConstantBuffer<CB_SH>( m_device, 1 );

				m_CB_ComputeIrradiance = new ConstantBuffer<CB_ComputeIrradiance>( m_device, 2 );

				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "BlueNoise64x64.png" ) ) ) {
					using ( ImageFile monoI = new ImageFile() ) {
						monoI.ConvertFrom( I, PIXEL_FORMAT.R8 );
						m_tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] { { monoI } } ), COMPONENT_FORMAT.UNORM );
					}
				}

				InitEnvironmentSH();
				UpdateSH();

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
			if ( m_tex_ComputedBentCone != null )
				m_tex_ComputedBentCone.Dispose();
			if ( m_tex_Irradiance1 != null )
				m_tex_Irradiance1.Dispose();
			if ( m_tex_Irradiance0 != null )
				m_tex_Irradiance0.Dispose();
			if ( m_tex_Normal != null )
				m_tex_Normal.Dispose();
// 			if ( m_tex_Illuminance != null )
// 				m_tex_Illuminance.Dispose();
			if ( m_tex_GroundTruth != null )
				m_tex_GroundTruth.Dispose();
			m_tex_BlueNoise.Dispose();

			m_CB_ComputeIrradiance.Dispose();
			m_CB_SH.Dispose();
			m_CB_Main.Dispose();
//			m_shader_FilterIndirectIrradiance.Dispose();
			m_shader_ComputeIndirectIrradiance.Dispose();
//			m_shader_ComputeIndirectMap.Dispose();
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

		#region Light Manipulation

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
				float	Dy = e.Location.Y - m_buttonDownPosition.Y;
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

		#region Ground Truth Computation

		uint		m_width, m_height, m_raysCount;
		uint[]		m_indirectPixelIndices = null;
		public void		SetIndirectPixelIndices( uint _width, uint _height, uint _raysCount, uint[] _indirectPixelIndices ) {
			if ( m_tex_GroundTruth != null )
				m_tex_GroundTruth.Dispose();
			m_tex_GroundTruth = null;

			m_width = _width;
			m_height = _height;
			m_raysCount = _raysCount;
			m_indirectPixelIndices = _indirectPixelIndices;
		}

		private void radioButtonGroundTruth_CheckedChanged( object sender, EventArgs e ) {
			if ( !radioButtonGroundTruth.Checked )
				return;

//			float3	rho = floatTrackbarControlReflectance.Value * new float3( 1.0f, 0.9f, 0.7f );
			float3	rho = m_CB_Main.m._rho;
			UpdateGroundTruth( rho );
		}

		// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
		void BuildOrthonormalBasis( float3 _normal, ref float3 _tangent, ref float3 _bitangent ) {
			float a = _normal.z > -0.9999999f ? 1.0f / (1.0f + _normal.z) : 0.0f;
			float b = -_normal.x * _normal.y * a;

			_tangent.Set( 1.0f - _normal.x*_normal.x*a, b, -_normal.x );
			_bitangent.Set( b, 1.0f - _normal.y*_normal.y*a, -_normal.y );
		}

		// Evaluates the SH coefficients in the requested direction
		// Analytic method from http://www1.cs.columbia.edu/~ravir/papers/envmap/envmap.pdf eq. 3
		//
		void	EvaluateSHRadiance( ref float3 _direction, ref float3 _radiance ) {
			const float	f0 = 0.28209479177387814347403972578039f;		// 0.5 / sqrt(PI);
			const float	f1 = 0.48860251190291992158638462283835f;		// 0.5 * sqrt(3/PI);
			const float	f2 = 1.0925484305920790705433857058027f;		// 0.5 * sqrt(15/PI);
			const float	f3 = 0.31539156525252000603089369029571f;		// 0.25 * sqrt(5.PI);

			float	SH0 = f0;
			float	SH1 = f1 * _direction.y;
			float	SH2 = f1 * _direction.z;
			float	SH3 = f1 * _direction.x;
			float	SH4 = f2 * _direction.x * _direction.y;
			float	SH5 = f2 * _direction.y * _direction.z;
			float	SH6 = f3 * (3.0f * _direction.z*_direction.z - 1.0f);
			float	SH7 = f2 * _direction.x * _direction.z;
			float	SH8 = f2 * 0.5f * (_direction.x*_direction.x - _direction.y*_direction.y);

			float3[]	_SH = m_rotatedLightSH;

			// Dot the SH together
			_radiance = SH0 * _SH[0]
					  + SH1 * _SH[1]
					  + SH2 * _SH[2]
					  + SH3 * _SH[3]
					  + SH4 * _SH[4]
					  + SH5 * _SH[5]
					  + SH6 * _SH[6]
					  + SH7 * _SH[7]
					  + SH8 * _SH[8];
			_radiance.Max( float3.Zero );
		}

		float3		m_groundTruthLastRho = float3.Zero;
		Texture2D	m_tex_GroundTruth = null;
		float4[][,]	m_lastGroundTruth = null;
		void	UpdateGroundTruth( float3 _rho ) {
			float4[][,]	groundTruth = m_owner.GenerateGroundTruth( _rho, m_rotatedLightSH );
			if ( groundTruth == m_lastGroundTruth )
				return;	// No change
			m_lastGroundTruth = groundTruth;

			if ( m_tex_GroundTruth != null )
				m_tex_GroundTruth.Dispose();

			int				slicesCount = groundTruth.Length;
			uint			W = (uint) groundTruth[0].GetLength( 0 );
			uint			H = (uint) groundTruth[0].GetLength( 1 );
			PixelsBuffer[]	content = new PixelsBuffer[slicesCount];
			for ( int sliceIndex=0; sliceIndex < slicesCount; sliceIndex++ ) {
				float4[,]		sourceContent = groundTruth[sliceIndex];
				PixelsBuffer	sliceContent = new PixelsBuffer( W*H*16 );
				content[sliceIndex] = sliceContent;
				using ( System.IO.BinaryWriter Wr = sliceContent.OpenStreamWrite() ) {
					for ( uint Y=0; Y < H; Y++ )
						for ( uint X=0; X < W; X++ ) {
							Wr.Write( sourceContent[X,Y].x );
							Wr.Write( sourceContent[X,Y].y );
							Wr.Write( sourceContent[X,Y].z );
							Wr.Write( sourceContent[X,Y].w );
						}
				}
			}
			m_tex_GroundTruth = new Texture2D( m_device, W, H, slicesCount, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, content );

/*			if ( m_tex_GroundTruth != null && _rho == m_groundTruthLastRho )
				return;	// Already computed!
			if ( m_indirectPixelIndices == null || m_imageNormal == null )
				return;

			m_groundTruthLastRho = _rho;

			if ( m_tex_GroundTruth != null )
				m_tex_GroundTruth.Dispose();
			m_tex_GroundTruth = null;

			uint		W = m_width;
			uint		H = m_height;
			uint		X, Y, rayIndex, neighborIndex;

			float3[]	rays = GeneratorForm.GenerateRays( (int) m_raysCount, Mathf.ToRad( 179.0f ) );
			float3		lsRayDirection, wsRayDirection;
			float3		radiance = float3.Zero;
			float3		irradiance = float3.Zero;
			float3		T = float3.One, B = float3.One, N = float3.One;

			// IMPORTANT
			_rho /= Mathf.PI;	// We actually need rho/PI to integrate the radiance
			// IMPORTANT


			const int	BOUNCES_COUNT = 20;
			float3[][,]	irradianceBounces = new float3[1+BOUNCES_COUNT][,];


			//////////////////////////////////////////////////////////////////////////
			// 1] Retrieve world normals
			// 
			float3[,]	normals = new float3[W,H];
			float3[,]	tangents = new float3[W,H];
			float3[,]	biTangents = new float3[W,H];
			m_imageNormal.ReadPixels( ( uint _X, uint _Y, ref float4 _color ) => {
				N = new float3( 2.0f * _color.x - 1.0f, 2.0f * _color.y - 1.0f, 2.0f * _color.z - 1.0f );
				BuildOrthonormalBasis( N, ref T, ref B );
				normals[_X,_Y] = N;
				tangents[_X,_Y] = T;
				biTangents[_X,_Y] = B;
			} );


			//////////////////////////////////////////////////////////////////////////
			// 2] Compute irradiance perceived directly
			{
				float3[,]	E0 = new float3[W,H];
				irradianceBounces[0] = E0;

				float	normalizer = 2.0f * Mathf.PI		// This factor is here because are actually integrating over the entire hemisphere of directions
															//	and we only accounted for cosine-weighted distribution along theta, we need to account for phi as well!
								   / m_raysCount;

				neighborIndex = 0;
				for ( Y=0; Y < H; Y++ ) {
					for ( X=0; X < W; X++ ) {
						T = tangents[X,Y];
						B = biTangents[X,Y];
						N = normals[X,Y];

						irradiance = float3.Zero;
						for ( rayIndex=0; rayIndex < m_raysCount; rayIndex++ ) {
							uint	packedNeighborPixelPosition = m_indirectPixelIndices[neighborIndex++];
							if ( packedNeighborPixelPosition != ~0U )
								continue;	// Obstructed

							lsRayDirection = rays[rayIndex];
							wsRayDirection = lsRayDirection.x * T + lsRayDirection.y * B + lsRayDirection.z * N;
							EvaluateSHRadiance( ref wsRayDirection, ref radiance );

							irradiance += radiance * lsRayDirection.z;	// L(x,Wi) * (N.Wi)
						}
						irradiance *= normalizer;
						E0[X,Y] = irradiance;
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			// ]
			PixelsBuffer	content = new PixelsBuffer( W*H*4*4 );
			using ( System.IO.BinaryWriter Wr = content.OpenStreamWrite() ) {
				for ( Y=0; Y < H; Y++ ) {
					for ( X=0; X < W; X++ ) {
						Wr.Write( irradianceBounces[0][X,Y].x );
						Wr.Write( irradianceBounces[0][X,Y].y );
						Wr.Write( irradianceBounces[0][X,Y].z );
						Wr.Write( 1 );
					}
				}
			}
			m_tex_GroundTruth = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
*/		}

		#endregion
	}
}
