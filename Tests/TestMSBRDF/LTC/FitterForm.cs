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

namespace TestMSBRDF.LTC
{
	public partial class FitterForm : Form
	{
		LTCFitter	m_owner = null;

		uint		m_width;
		float4[]	m_falseSpectrum;

		ImageFile	m_imageSource;
		ImageFile	m_imageTarget;
		ImageFile	m_imageDifference;

		public FitterForm( LTCFitter _owner ) {
			InitializeComponent();

			m_owner = _owner;
			m_width = (uint) panelOutputSourceBRDF.Width;

			ColorProfile	profile = new ColorProfile( ColorProfile.STANDARD_PROFILE.LINEAR );

			m_imageSource = new ImageFile( (uint) panelOutputSourceBRDF.Width, (uint) panelOutputSourceBRDF.Height, PIXEL_FORMAT.BGRA8, profile );
			m_imageTarget = new ImageFile( (uint) panelOutputTargetBRDF.Width, (uint) panelOutputTargetBRDF.Height, PIXEL_FORMAT.BGRA8, profile );
			m_imageDifference = new ImageFile( (uint) panelOutputDifference.Width, (uint) panelOutputDifference.Height, PIXEL_FORMAT.BGRA8, profile );

			// Read false spectrum colors
			ImageFile	I = new ImageFile( Properties.Resources.FalseColorsSpectrum, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_falseSpectrum = new float4[I.Width];
			I.ReadScanline( 0, m_falseSpectrum );
		}

		float			m_theta;
		float			m_roughness;
		IBRDF			m_BRDF;
		LTCFitter.LTC	m_LTC;
		float3			m_tsView;
		float3			m_tsLight;

		int				m_statsCounter;
		double			m_lastError;
		double			m_lastNormalization;
		double			m_statsSumError;
		double			m_statsSumErrorWithoutHighValues;
		double			m_statsSumNormalization;

		public bool		Paused { get { return checkBoxPause.Checked; } set { checkBoxPause.Checked = value; } }

		public void	AccumulateStatistics( LTCFitter.LTC _LTC ) {
			m_lastError = _LTC.error;
			m_lastNormalization = _LTC.TestNormalization();
			m_statsSumError += m_lastError;
			m_statsSumErrorWithoutHighValues += Math.Min( 1, m_lastError );
			m_statsSumNormalization += m_lastNormalization;
			m_statsCounter++;
		}

		public void	ShowBRDF( float _progress, float _theta, float _roughness, IBRDF _BRDF, LTCFitter.LTC _LTC ) {
			m_theta = _theta;
			m_roughness = _roughness;
			m_BRDF = _BRDF;
			m_LTC = _LTC;

			this.Text = "Fitter Debugger - Theta = " + Mathf.ToDeg(_theta).ToString( "G3" ) + "° - Roughness = " + _roughness.ToString( "G3" ) + " - Error = " + _LTC.error.ToString( "G4" ) + " - Progress = " + (100.0f * _progress).ToString( "G3" ) + "%";

			// Build fixed view vector
			m_tsView.x = Mathf.Sin( m_theta );
			m_tsView.y = 0;
			m_tsView.z = Mathf.Cos( m_theta );

			// Recompute images
			m_imageSource.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateBRDF( ref _tsView, ref _tsLight ); } ); } );
			m_imageTarget.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateLTC( ref _tsView, ref _tsLight ); } ); } );
			m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, 0, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return Mathf.Abs( EstimateBRDF( ref _tsView, ref _tsLight ) - EstimateLTC( ref _tsView, ref _tsLight ) ); } ); } );

			// Assign bitmaps
			panelOutputSourceBRDF.PanelBitmap = m_imageSource.AsBitmap;
			panelOutputTargetBRDF.PanelBitmap = m_imageTarget.AsBitmap;
			panelOutputDifference.PanelBitmap = m_imageDifference.AsBitmap;

			// Update text
			float[]		runtimeParms = _LTC.RuntimeParameters;

			textBoxFitting.Text = "m11 = " + runtimeParms[0] + "\r\n"
								+ "m22 = " + runtimeParms[1] + "\r\n"
								+ "m13 = " + runtimeParms[2] + "\r\n"
								+ "m31 = " + runtimeParms[3] + "\r\n"
								+ "\r\n"
								+ "Amplitude = " + runtimeParms[4] + "\r\n"
								+ "Fresnel = " + runtimeParms[5] + "\r\n"
								+ "\r\n"
								+ "invM = \r\n"
								+ "r0 = " + _LTC.invM.r0 + "\r\n"
								+ "r1 = " + _LTC.invM.r1 + "\r\n"
								+ "r2 = " + _LTC.invM.r2 + "\r\n"
								+ "\r\n"
								+ "► Error:\r\n"
								+ "Avg. = " + (m_statsSumError / m_statsCounter).ToString( "G4" ) + "\r\n"
								+ "Avg. (Clipped) = " + (m_statsSumErrorWithoutHighValues / m_statsCounter).ToString( "G4" ) + "\r\n"
								+ "\r\n"
								+ "► Normalization:\r\n"
								+ "Value = " + m_lastNormalization.ToString( "G4" ) + "\r\n"
								+ "Avg. = " + (m_statsSumNormalization / m_statsCounter).ToString( "G4" ) + "\r\n";

			// Redraw
			panelOutputSourceBRDF.Refresh();
			panelOutputTargetBRDF.Refresh();
			panelOutputDifference.Refresh();
			textBoxFitting.Refresh();
			Refresh();

			// Let windows get updated
			Application.DoEvents();

			while ( Paused )
				Application.DoEvents();
		}

		delegate float	RadialAmplitudeDelegate( ref float3 _tsView, ref float3 _tsLight );

		void	RenderSphere( uint _X, uint _Y, float _log10Min, float _log10Max, ref float4 _color, RadialAmplitudeDelegate _radialAmplitude ) {
			m_tsLight.x = 2.0f * _X / m_width - 1.0f;
			m_tsLight.y = 1.0f - 2.0f * _Y / m_width;
			m_tsLight.z = 1 - m_tsLight.x*m_tsLight.x - m_tsLight.y*m_tsLight.y;
			if ( m_tsLight.z <= 0.0f ) {
				_color.Set( 0, 0, 0, 1 );
				return;
			}

			m_tsLight.z = Mathf.Sqrt( m_tsLight.z );

			// Estimate radial amplitude
			float	V = _radialAmplitude( ref m_tsView, ref m_tsLight );

			// Transform into false spectrum color
			float	logV = Mathf.Clamp( Mathf.Log10( Mathf.Max( 1e-8f, V ) ), _log10Min, _log10Max );
			float	t = (logV - _log10Min) / (_log10Max - _log10Min);
//t = (float) _X / m_width;
			int		it = Mathf.Clamp( (int) (t * m_falseSpectrum.Length), 0, m_falseSpectrum.Length-1 );
			_color = m_falseSpectrum[it];
//_color = new float4( 1, 1, 0, 1 );
		}

		float	EstimateBRDF( ref float3 _tsView, ref float3 _tsLight ) {
			float	pdf;
			float	V = m_BRDF.Eval( ref _tsView, ref _tsLight, m_roughness, out pdf );
			return V;
		}

		float	EstimateLTC( ref float3 _tsView, ref float3 _tsLight ) {
			float	V = m_LTC.Eval( ref _tsLight );
			return V;
		}
	}
}
