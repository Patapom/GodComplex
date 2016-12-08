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
using SphericalHarmonics;
using Renderer;
using Nuaj.Cirrus.Utility;

namespace TestSHIrradiance
{
	public partial class Form1 : Form {

		ImageUtility.ImageFile	m_image = new ImageUtility.ImageFile( 800, 550, ImageUtility.ImageFile.PIXEL_FORMAT.RGBA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
		ImageUtility.ImageFile	m_HDRImage = new ImageUtility.ImageFile();

		float4					m_black = new float4( 0, 0, 0, 1 );
		float4					m_white = new float4( 1, 1, 1, 1 );
		float4					m_red = new float4( 1, 0, 0, 1 );
		float4					m_green = new float4( 0, 1, 0, 1 );
		float4					m_blue = new float4( 0, 0, 1, 1 );

		// D3D Stuff
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_SizeX;
			public uint		_SizeY;
			public float	_Time;
			public float	_cosAO;
			public float4x4	_world2Proj;
		}

		Device						m_device = null;
		Shader						m_shader_RenderSphere = null;
		Shader						m_shader_RenderScene = null;
		ConstantBuffer< CB_Main >	m_CB_Render;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();


		public Form1() {
			InitializeComponent();
			UpdateGraph();
		}

		#region HDR Image Encoding

		void	LoadHDRImage() {
//			m_HDRImage.Load( new System.IO.FileInfo( @"D:\Docs\Computer Graphics\Image Based Lighting + Colorimetry\HDR Images\grace-new.hdr" ) );
			m_HDRImage.Load( new System.IO.FileInfo( @".\Images\grace-new.hdr" ) );
// 			ImageUtility.ImageFile	tempLDRImage = new ImageUtility.ImageFile();
// 			tempLDRImage.ToneMapFrom( m_HDRImage, ( float3 _HDR, ref float3 _LDR ) => {
// 				_LDR = _HDR;
// 			} );
// 			graphPanel.Bitmap = tempLDRImage.AsBitmap;

			// Integrate SH
//			EncodeSH();

// Grace probe
// float3	SH[9] = {
// 					float3( 0.933358105849532, 0.605499186927096, 0.450999072970855 ), 
// 					float3( 0.0542981143130068, 0.0409598475963159, 0.0355377036564806 ), 
// 					float3( 0.914255336642483, 0.651103534810611, 0.518065694132826 ), 
// 					float3( 0.238207071886099, 0.14912965904707, 0.0912559191766972 ), 
// 					float3( 0.0321476755042544, 0.0258939812282057, 0.0324159089991572 ), 
// 					float3( 0.104707893908821, 0.0756648975030993, 0.0749934936107284 ), 
// 					float3( 1.27654512826622, 0.85613828921136, 0.618241442250845 ), 
// 					float3( 0.473237767573493, 0.304160108872238, 0.193304867770535 ), 
// 					float3( 0.143726445535245, 0.0847402441253633, 0.0587779174281925 ), 
// };


			// Build texture

		}

		/// <summary>
		/// Encodes HDR radiance image into SH
		/// We're assuming the input image is encoded as panoramic format described here: http://gl.ict.usc.edu/Data/HighResProbes/
		/// </summary>
		/// <returns></returns>
		string	EncodeSH() {
			// Thus, if we consider the images to have a rectangular image domain of u=[0,2], v=[0,1], we have theta= pi*(u-1), phi=pi*v.
			// The unit vector pointing in the corresponding direction is obtained by (Dx,Dy,Dz) = (sin(phi)*sin(theta), cos(phi), -sin(phi)*cos(theta)).
			// For the reverse mapping from the direction vector in the world (Dx, Dy, Dz), the corresponding (u,v) coordinate in the light probe image is ( 1 + atan2(Dx,-Dz) / pi, arccos(Dy) / pi).
			//
			// This mapping is convenient but does not have equal area.
			// Thus to find the average pixel value one must first multiply by the vertical cosine falloff function cos(phi).
			//
			// NOTE: Apparently, they consider Y-up axis and we're using Z-up
			//
			uint	W = m_HDRImage.Width;
			uint	H = m_HDRImage.Height;
			double	dPhi = 2.0 * Math.PI / W;
			double	dTheta = Math.PI / H;

			double[,]	coeffs = new double[9,3];
			for ( uint Y=0; Y < H; Y++ ) {
				double	theta = (0.5+Y) * dTheta;
				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );

				for ( uint X=0; X < W; X++ ) {
					double	phi = X * dPhi - Math.PI;
					double	cosPhi = Math.Cos( phi );
					double	sinPhi = Math.Sin( phi );

					float4	HDRColor = m_HDRImage[X,Y];

					// Accumulate weighted SH
					double	omega = sinTheta * dPhi * dTheta;
					for ( int l=0; l < 3; l++ ) {
						for ( int m=-l; m <= l; m++ ) {
							double	Ylm = SHFunctions.ComputeSH( l, m, theta, phi );
							int		i = l*(l+1)+m;
							coeffs[i,0] += HDRColor.x * omega * Ylm;
							coeffs[i,1] += HDRColor.y * omega * Ylm;
							coeffs[i,2] += HDRColor.z * omega * Ylm;
						}
					}
				}
			}

			string	SHText = "float3	SH[9] = {\r\n";
			for ( int l=0; l < 3; l++ ) {
				for ( int m=-l; m <= l; m++ ) {
					int		i = l*(l+1)+m;
					SHText += "					float3( " + coeffs[i,0] + ", " + coeffs[i,1] + ", " + coeffs[i,2] + " ), \r\n";
				}
			}
			SHText += "};\r\n";
			return SHText;
		}

		#endregion

		#region Graph Plotting

		double	EstimateSHCoeff( int l, double _thetaMax ) {
			const int		STEPS_COUNT = 100;

			double	normalizationFactor = SphericalHarmonics.SHFunctions.K( l, 0 );

			double	dTheta = _thetaMax / STEPS_COUNT;
			double	sum = 0.0;
			for ( int i=0; i < STEPS_COUNT; i++ ) {
				double	theta = (0.5+i) * dTheta;
				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );
				double	Pl0 = SphericalHarmonics.SHFunctions.P( l, 0, cosTheta );
				sum += Pl0 * cosTheta * sinTheta * dTheta;
			}
			sum *= 2.0 * Math.PI * normalizationFactor;

			return sum;
		}

		const int	MAX_ORDER = 20;
		float2	rangeX = new float2( 0, MAX_ORDER );
		float2	rangeY = new float2( -0.2f, 1.1f );
		float[]	coeffs = new float[1+MAX_ORDER];

		void	PlotSquare( float4 _color, float2 _rangeX, float2 _rangeY, float2 _rangedPosition ) {
			float2	imagePosition = m_image.RangedCoordinates2ImageCoordinates( _rangeX, _rangeY, _rangedPosition );
			int		size = 4;
			int		X0 = Math.Max( 0, (int) Math.Floor( imagePosition.x - size ) );
			int		Y0 = Math.Max( 0, (int) Math.Floor( imagePosition.y - size ) );
			int		X1 = Math.Min( (int) m_image.Width-1, (int) Math.Floor( imagePosition.x + size ) );
			int		Y1 = Math.Min( (int) m_image.Height-1, (int) Math.Floor( imagePosition.y + size ) );
			for ( int Y=Y0; Y <= Y1; Y++ )
				for ( int X=X0; X <= X1; X++ )
					m_image[(uint)X,(uint)Y] = _color;
		}

		void	UpdateGraph() {
			string	text = "";

			// Estimate coeffs
			double	thetaMax = floatTrackbarControlThetaMax.Value * 0.5 * Math.PI / 90.0;
			for ( int l=0; l <= MAX_ORDER; l++ ) {
				coeffs[l] = (float) EstimateSHCoeff( l, thetaMax );
				text += "SH #" + l + " = " + coeffs[l] + "\r\n";
			}

			m_image.Clear( m_white );
//			m_image.PlotGraphAutoRangeY( m_black, rangeX, ref rangeY, ( float x ) => {
			m_image.PlotGraph( m_black, rangeX, rangeY, ( float x ) => {
				int		l0 = Math.Min( MAX_ORDER, (int) Math.Floor( x ) );
				float	A0 = coeffs[l0];
				int		l1 = Math.Min( MAX_ORDER, l0+1 );
				float	A1 = coeffs[l1];
				x = x - l0;
				return A0 + (A1 - A0) * x;
			} );

			// Plot A0, A1 and A2 terms
			double	C = Math.Cos( thetaMax );
// 			m_image.PlotGraph( m_red, rangeX, rangeY, ( float x ) => { return (float) (Math.Sqrt( Math.PI ) * (1.0 - C*C) / 2.0); } );
// 			m_image.PlotGraph( m_green, rangeX, rangeY, ( float x ) => { return (float) (Math.Sqrt( 3.0 * Math.PI ) * (1.0 - C*C*C) / 3.0); } );
// 			m_image.PlotGraph( m_blue, rangeX, rangeY, ( float x ) => { return (float) (Math.Sqrt( 5.0 * Math.PI / 4.0 ) * (3.0/4.0 * (1.0 - C*C*C*C) - 1.0 / 2.0 * (1 - C*C))); } );
			PlotSquare( m_red, rangeX, rangeY, new float2( 0, (float) (Math.Sqrt( Math.PI ) * (1.0 - C*C) / 2.0) ) );
			PlotSquare( m_green, rangeX, rangeY, new float2( 1, (float) (Math.Sqrt( Math.PI / 3.0 ) * (1.0 - C*C*C)) ) );
			PlotSquare( m_blue, rangeX, rangeY, new float2( 2, (float) (Math.Sqrt( 5.0 * Math.PI / 4.0 ) * (3.0/4.0 * (1.0 - C*C*C*C) - 1.0 / 2.0 * (1 - C*C))) ) );

			m_image.PlotAxes( m_black, rangeX, rangeY, 1, 0.1f );

			text += "\r\nRange Y = [" + rangeY.x + ", " + rangeY.y + "]\r\n";
			textBoxResults.Text = text;

			graphPanel.Bitmap = m_image.AsBitmap;
			graphPanel.Refresh();
			floatTrackbarControlThetaMax.Refresh();
			textBoxResults.Refresh();
		}

		#endregion

		#region 3D Rendering

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				// Initialize the device
				m_device = new Device();
				m_device.Init( graphPanel.Handle, false, true );

				// Create the render shaders

				// Create CB
				m_CB_Render = new ConstantBuffer< CB_Main >( m_device, 0 );

				// Create textures
				LoadHDRImage();

				// Create camera + manipulator
				m_camera.CreatePerspectiveCamera( 0.5f * (float) Math.PI, (float) graphPanel.Width / graphPanel.Height, 0.01f, 100.0f );
				m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
				m_cameraManipulator.Attach( graphPanel, m_camera );

				// Start rendering
				Application.Idle += Application_Idle;

			} catch ( Exception _e ) {
				MessageBox.Show( "Failed to initialize D3D renderer!\r\nReason: " + _e.Message );
			}
		}

		void m_camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Render.m._world2Proj = m_camera.World2Camera * m_camera.Camera2Proj;
		}

		DateTime	m_startTime = DateTime.Now;

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || radioButtonCoeffs.Checked )
				return;	// No 3D render

			m_device.SetRenderTarget( m_device.DefaultTarget, null );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			// Update CB
			m_CB_Render.m._SizeX = graphPanel.Width;
			m_CB_Render.m._SizeY = graphPanel.Height;
			m_CB_Render.m._Time = (float) (DateTime.Now - m_startTime).TotalSeconds;
			m_CB_Render.m._cosAO = (float) Math.Cos( floatTrackbarControlThetaMax.Value * Math.PI / 180.0 );
			m_CB_Render.UpdateData();

			// Render
			if ( m_shader_RenderSphere.Use() ) {
				 m_device.RenderFullscreenQuad( m_shader_RenderSphere );
			}

			m_device.Present( false );
		}

		#endregion

		private void floatTrackbarControlThetaMax_ValueChanged(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue) {
			if ( radioButtonCoeffs.Checked )
				UpdateGraph();
		}
	}
}
