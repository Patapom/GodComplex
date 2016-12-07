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

namespace TestSHIrradiance
{
	public partial class Form1 : Form {

		ImageUtility.ImageFile	m_image = new ImageUtility.ImageFile( 800, 550, ImageUtility.ImageFile.PIXEL_FORMAT.RGBA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );

		ImageUtility.ImageFile	m_HDRImage = new ImageUtility.ImageFile();

		float4		m_black = new float4( 0, 0, 0, 1 );
		float4		m_white = new float4( 1, 1, 1, 1 );
		float4		m_red = new float4( 1, 0, 0, 1 );
		float4		m_green = new float4( 0, 1, 0, 1 );
		float4		m_blue = new float4( 0, 0, 1, 1 );

		public Form1() {
			InitializeComponent();
			UpdateGraph();
			LoadHDRImage();

		}

		void	LoadHDRImage() {
			m_HDRImage.Load( new System.IO.FileInfo( @"D:\Docs\Computer Graphics\Image Based Lighting + Colorimetry\HDR Images\grace-new.hdr" ) );
			ImageUtility.ImageFile	tempLDRImage = new ImageUtility.ImageFile();
			tempLDRImage.ToneMapFrom( m_HDRImage, ( float3 _HDR, ref float3 _LDR ) => {
				_LDR = _HDR;
			} );
			graphPanel.Bitmap = tempLDRImage.AsBitmap;
		}

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

		private void floatTrackbarControlThetaMax_SliderDragStop(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue) {
//			UpdateGraph();
		}

		private void floatTrackbarControlThetaMax_ValueChanged(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue) {
			UpdateGraph();
		}
	}
}
