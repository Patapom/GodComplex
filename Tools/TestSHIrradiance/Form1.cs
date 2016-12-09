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
			public uint		_Flags;
			public float4x4	_world2Proj;
//			public float4x4	_proj2World;
			public float4x4	_camera2World;
			public float	_cosAO;
			public float	_luminanceFactor;
			public float	_filteringWindowSize;
		}

		Device						m_device = null;
		Shader						m_shader_RenderSphere = null;
		Shader						m_shader_RenderScene = null;
		ConstantBuffer< CB_Main >	m_CB_Render;
		Texture2D					m_Tex_HDR;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();


		public Form1() {
			InitializeComponent();
			UpdateGraph();
		}

		#region HDR Image Encoding

//Check direction d'encodage daubée +Y / -Y!
//FILTERING !!! (Hanning, tout ça)

		void	LoadHDRImage() {
//			m_HDRImage.Load( new System.IO.FileInfo( @".\Images\grace-new.hdr" ) );
			m_HDRImage.Load( new System.IO.FileInfo( @".\Images\ennis_1024x512.hdr" ) );
			
// 			ImageUtility.ImageFile	tempLDRImage = new ImageUtility.ImageFile();
// 			tempLDRImage.ToneMapFrom( m_HDRImage, ( float3 _HDR, ref float3 _LDR ) => {
// 				_LDR = _HDR;
// 			} );
// 			graphPanel.Bitmap = tempLDRImage.AsBitmap;

			// Integrate SH
//			EncodeSH();

			// Test numerical integration
//			NumericalIntegration();

			// Build texture
			ImagesMatrix	images = new Renderer.ImagesMatrix( new ImageUtility.ImageFile[] { m_HDRImage }, 1 );
			m_Tex_HDR = Texture2D.CreateTexture2D( m_device, images );
		}

		void	NumericalIntegration() {
			// Generate a bunch of rays with equal probability on the hemisphere
			const int		THETA_SAMPLES = 100;
			const int		SAMPLES_COUNT = 4*THETA_SAMPLES*THETA_SAMPLES;
			const double	dPhi = 2.0 * Math.PI / (4 * THETA_SAMPLES);
			float3[]		directions = new float3[SAMPLES_COUNT];
			for ( int Y=0; Y < THETA_SAMPLES; Y++ ) {
				for ( int X=0; X < 4*THETA_SAMPLES; X++ ) {
					double	phi = dPhi * (X+SimpleRNG.GetUniform());
					double	theta = 2.0 * Math.Acos( Math.Sqrt( 1.0 - 0.5 * (Y+SimpleRNG.GetUniform()) / THETA_SAMPLES ) );		// Uniform sampling on theta
					directions[4*THETA_SAMPLES*Y+X].Set( (float) (Math.Sin( theta ) * Math.Cos( phi )), (float) (Math.Sin( theta ) * Math.Sin( phi )), (float) Math.Cos( theta ) );
				}
			}

			// Compute numerical integration for various sets of angles
			const int	TABLE_SIZE = 100;

			float3		coneDirection = float3.Zero;
			float3[,]	integratedSHCoeffs = new float3[TABLE_SIZE,TABLE_SIZE];
			for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ ) {
//				float	cosTheta = 1.0f - (float) thetaIndex / TABLE_SIZE;
float	cosTheta = (float) Math.Cos( 0.5 * Math.PI * thetaIndex / TABLE_SIZE );
				coneDirection.x = (float) Math.Sqrt( 1.0f - cosTheta*cosTheta );
				coneDirection.z = cosTheta;

				for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
//					float	AO = 1.0f - (float) AOIndex / TABLE_SIZE;
//					float	coneHalfAngle = 0.5f * (float) Math.PI * AO;			// Cone half angle varies in [0,PI/2]
//					float	cosConeHalfAngle = (float) Math.Cos( coneHalfAngle );
float	cosConeHalfAngle = (float) AOIndex / TABLE_SIZE;

					double	A0 = 0.0;
					double	A1 = 0.0;
					double	A2 = 0.0;
					for ( int sampleIndex=0; sampleIndex < SAMPLES_COUNT; sampleIndex++ ) {
						float3	direction = directions[sampleIndex];
						if ( direction.Dot( coneDirection ) < cosConeHalfAngle )
							continue;	// Sample is outside cone

						float	u = direction.z;	// cos(theta_sample)
						float	u2 = u * u;
						float	u3 = u * u2;
						A0 += u;
//A0 += 1.0;
						A1 += u2;
						A2 += 0.5 * (3 * u3 - u);
					}

					A0 *= 2.0 * Math.PI / SAMPLES_COUNT;
					A1 *= 2.0 * Math.PI / SAMPLES_COUNT;
					A2 *= 2.0 * Math.PI / SAMPLES_COUNT;
					A0 *= Math.Sqrt( 1.0 / (4.0 * Math.PI) );
					A1 *= Math.Sqrt( 3.0 / (4.0 * Math.PI) );
					A2 *= Math.Sqrt( 5.0 / (4.0 * Math.PI) );

					integratedSHCoeffs[thetaIndex,AOIndex].Set( (float) A0, (float) A1, (float) A2 );
				}
			}

			using ( System.IO.FileStream S = new System.IO.FileInfo( @"ConeTable_cosAO.float3" ).Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
				for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ )
						for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
							W.Write( integratedSHCoeffs[thetaIndex,AOIndex].x );
							W.Write( integratedSHCoeffs[thetaIndex,AOIndex].y );
							W.Write( integratedSHCoeffs[thetaIndex,AOIndex].z );
						}
				}
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

			double[]	SH0 = new double[9];
			double[]	SH1 = new double[9];

			double[,]	coeffs = new double[9,3];
			for ( uint Y=0; Y < H; Y++ ) {
				double	theta = (0.5+Y) * dTheta;
//				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );

				for ( uint X=0; X < W; X++ ) {
					double	phi = X * dPhi - Math.PI;
// 					double	cosPhi = Math.Cos( phi );
// 					double	sinPhi = Math.Sin( phi );

					float4	HDRColor = m_HDRImage[X,Y];
//HDRColor.Set( 1, 1, 1, 1 );

					// Accumulate weighted SH
					double	omega = sinTheta * dPhi * dTheta;


// Compare our 2 ways of generating Ylm
// SHFunctions.Ylm( new float3( (float) (sinTheta * cosPhi), (float) (sinTheta * sinPhi), (float) cosTheta ), SH0 );
// for ( int l=0; l < 3; l++ ) {
// 	for ( int m=-l; m <= l; m++ ) {
// 		int		i = l*(l+1)+m;
// 		SH1[i] = SHFunctions.Ylm( l, m, theta, phi );
// 	}
// }
// for ( int i=0; i < 9; i++ )
// 	if ( Math.Abs( SH0[i] - SH1[i] ) > 1e-4 )
// 		throw new Exception( "ARGH!" );
			


					for ( int l=0; l < 3; l++ ) {
						for ( int m=-l; m <= l; m++ ) {
							double	Ylm = SHFunctions.Ylm( l, m, theta, phi );
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
			graphPanel.EnablePaint = true;
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
				try {
					m_shader_RenderSphere = new Shader( m_device, new ShaderFile( new System.IO.FileInfo( @"./Shaders/RenderSphere.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
					m_shader_RenderScene = new Shader( m_device, new ShaderFile( new System.IO.FileInfo( @"./Shaders/RenderScene.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				} catch ( Exception _e ) {
					throw new Exception( "Failed to compile shader! " + _e.Message );
				}

				// Create CB
				m_CB_Render = new ConstantBuffer< CB_Main >( m_device, 0 );

				// Create textures
				LoadHDRImage();

				// Create camera + manipulator
				m_camera.CreatePerspectiveCamera( 0.5f * (float) Math.PI, (float) graphPanel.Width / graphPanel.Height, 0.01f, 100.0f );
				m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
				m_cameraManipulator.Attach( graphPanel, m_camera );
				m_cameraManipulator.InitializeCamera( -2.0f * float3.UnitZ, float3.Zero, float3.UnitY );
				m_camera_CameraTransformChanged( null, EventArgs.Empty );

				// Start rendering
				Application.Idle += Application_Idle;

			} catch ( Exception _e ) {
				MessageBox.Show( "Failed to initialize D3D renderer!\r\nReason: " + _e.Message );
			}
		}

		void m_camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Render.m._world2Proj = m_camera.World2Proj;
//			m_CB_Render.m._proj2World =  m_camera.Proj2World;
			m_CB_Render.m._camera2World =  m_camera.Camera2World;
		}

		DateTime	m_startTime = DateTime.Now;

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || radioButtonCoeffs.Checked )
				return;	// No 3D render

			m_device.SetRenderTarget( m_device.DefaultTarget, null );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			// Update CB
			m_CB_Render.m._SizeX = (uint) graphPanel.Width;
			m_CB_Render.m._SizeY = (uint) graphPanel.Height;
			m_CB_Render.m._Time = (float) (DateTime.Now - m_startTime).TotalSeconds;
			m_CB_Render.m._cosAO = (float) Math.Cos( floatTrackbarControlThetaMax.Value * Math.PI / 180.0 );
			m_CB_Render.m._luminanceFactor = floatTrackbarControlLuminanceFactor.Value;
			m_CB_Render.m._filteringWindowSize = floatTrackbarControlFilterWindowSize.Value;
			m_CB_Render.m._Flags = 0;
			if ( radioButtonSideBySide.Checked )
				m_CB_Render.m._Flags = 1;
			m_CB_Render.m._Flags |= checkBoxAO.Checked ? 0x8U : 0U;
			m_CB_Render.m._Flags |= checkBoxShowAO.Checked ? 0x10U : 0U;
			m_CB_Render.m._Flags |= checkBoxShowBentNormal.Checked ? 0x20U : 0U;
			m_CB_Render.m._Flags |= checkBoxEnvironmentSH.Checked ? 0x100U : 0U;

			m_CB_Render.UpdateData();

			m_Tex_HDR.SetPS( 0 );

			// Render
			Shader	S = radioButtonSimpleScene.Checked ? m_shader_RenderScene : m_shader_RenderSphere;
			if ( S.Use() ) {
				m_device.RenderFullscreenQuad( S );
			}

			m_device.Present( false );
		}

		#endregion

		private void floatTrackbarControlThetaMax_ValueChanged(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue) {
			if ( radioButtonCoeffs.Checked )
				UpdateGraph();
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		private void radioButtonCoeffs_CheckedChanged( object sender, EventArgs e ) {
			graphPanel.Refresh();
			graphPanel.EnablePaint = radioButtonCoeffs.Checked;

			checkBoxAO.Visible = radioButtonSingleSphere.Checked | radioButtonSimpleScene.Checked;

			checkBoxShowAO.Visible = radioButtonSimpleScene.Checked;
			checkBoxShowBentNormal.Visible = radioButtonSimpleScene.Checked;
		}
	}
}
