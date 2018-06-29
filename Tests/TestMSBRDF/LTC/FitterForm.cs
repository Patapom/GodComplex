//#define RELATIVE_ERROR

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
		LTCFitter		m_owner = null;

		uint			m_width;
		float4[]		m_falseSpectrum;

		ImageFile		m_imageSource;
		ImageFile		m_imageTarget;
		ImageFile		m_imageDifference;

		float			m_theta;
		float			m_roughness;
		IBRDF			m_BRDF;
		LTCFitter.LTC	m_LTC;
		float3			m_tsView;
		float3			m_tsLight;

		int				m_statsCounter;
		int				m_statsNormalizationCounter;
		double			m_lastError;
		int				m_lastIterationsCount;
		double			m_lastNormalization;

		double			m_statsSumError;
		double			m_statsSumErrorWithoutHighValues;
		double			m_statsSumNormalization;
		int				m_statsSumIterations;

		public bool		Paused {
			get { return checkBoxPause.Checked; }
			set {
				checkBoxPause.Checked = value;
				checkBoxPause.Text = checkBoxPause.Checked ? "PLAY" : "PAUSE";
				integerTrackbarControlRoughnessIndex.Enabled = checkBoxPause.Checked;
				integerTrackbarControlThetaIndex.Enabled = checkBoxPause.Checked;
			}
		}

		public bool		AutoRun { get { return checkBoxAutoRun.Checked; } set { checkBoxAutoRun.Checked = value; } }
		public bool		DoFitting { get { return checkBoxDoFitting.Checked; } set { checkBoxDoFitting.Checked = value; } }

		public int		RoughnessIndex { get { return integerTrackbarControlRoughnessIndex.Value; } set { integerTrackbarControlRoughnessIndex.Value = value; } }
		public int		ThetaIndex { get { return integerTrackbarControlThetaIndex.Value; } set { integerTrackbarControlThetaIndex.Value = value; } }

		public delegate void	TrackbarChangedDelegate();
		public event TrackbarChangedDelegate	TrackbarValueChanged;

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

			#if RELATIVE_ERROR
				label7.Text = "1e4";
				label3.Text = "Relative Error";
			#else
				label7.Text = "1e0";
				label3.Text = "Absolute Error";
			#endif
		}

		public void	AccumulateStatistics( LTCFitter.LTC _LTC, bool _fullRefresh ) {
			m_lastError = _LTC.error;
			m_lastIterationsCount = _LTC.iterationsCount;
			if ( _fullRefresh ) {
				m_lastNormalization = _LTC.TestNormalization();
				m_statsSumNormalization += m_lastNormalization;
				m_statsNormalizationCounter++;
			}
			m_statsSumError += m_lastError;
			m_statsSumErrorWithoutHighValues += Math.Min( 1, m_lastError );
			m_statsSumIterations += m_lastIterationsCount;
			m_statsCounter++;
		}

		public void	ShowBRDF( float _progress, float _theta, float _roughness, IBRDF _BRDF, LTCFitter.LTC _LTC, bool _fullRefresh ) {
			m_theta = _theta;
			m_roughness = _roughness;
			m_BRDF = _BRDF;
			m_LTC = _LTC;

			this.Text = "Fitter Debugger - Theta = " + Mathf.ToDeg(_theta).ToString( "G3" ) + "° - Roughness = " + _roughness.ToString( "G3" ) + " - Error = " + (_LTC != null ? _LTC.error.ToString( "G4" ) : "not computed") + " - Progress = " + (100.0f * _progress).ToString( "G3" ) + "%";

			// Build fixed view vector
			m_tsView.x = Mathf.Sin( m_theta );
			m_tsView.y = 0;
			m_tsView.z = Mathf.Cos( m_theta );

			// Recompute images
			if ( _fullRefresh ) {
				m_imageSource.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateBRDF( ref _tsView, ref _tsLight ); } ); } );

				if ( m_LTC != null ) {
					m_imageTarget.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, +4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => { return EstimateLTC( ref _tsView, ref _tsLight ); } ); } );
				} else {
					m_imageTarget.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { _color = (((_X >> 2) ^ (_Y >> 2)) & 1) == 0 ? new float4( 1, 0, 0, 1 ) : float4.UnitW; } );
				}

				if ( m_LTC != null ) {
					#if RELATIVE_ERROR
						m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, 4, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => {
							float	V0 = EstimateBRDF( ref _tsView, ref _tsLight );
							float	V1 = EstimateLTC( ref _tsView, ref _tsLight );
							return (V0 > V1 ? V0 / Math.Max( 1e-6f, V1 ) : V1 / Math.Max( 1e-6f, V0 )) - 1.0f;
						} ); } );
					#else
						m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { RenderSphere( _X, _Y, -4, 0, ref _color, ( ref float3 _tsView, ref float3 _tsLight ) => {
//							utiliser error!
							return Mathf.Abs( EstimateBRDF( ref _tsView, ref _tsLight ) - EstimateLTC( ref _tsView, ref _tsLight ) );
						} ); } );
					#endif
				} else {
					m_imageDifference.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { _color = (((_X >> 2) ^ (_Y >> 2)) & 1) == 0 ? new float4( 1, 0, 0, 1 ) : float4.UnitW; } );
				}

				// Assign bitmaps
				panelOutputSourceBRDF.PanelBitmap = m_imageSource.AsBitmap;
				panelOutputTargetBRDF.PanelBitmap = m_imageTarget.AsBitmap;
				panelOutputDifference.PanelBitmap = m_imageDifference.AsBitmap;

				panelOutputSourceBRDF.Refresh();
				panelOutputTargetBRDF.Refresh();
				panelOutputDifference.Refresh();
			}

			// Update text
			if ( m_LTC == null ) {
				textBoxFitting.Text = "<NOT COMPUTED>";
				return;
			}

			double[]	runtimeParms = _LTC.RuntimeParameters;

			textBoxFitting.Text = "m11 = " + _LTC.m11 + "\r\n"
								+ "m22 = " + _LTC.m22 + "\r\n"
								+ "m13 = " + _LTC.m13 + "\r\n"
								+ "m31 = " + _LTC.m31 + "\r\n"
								+ "\r\n"
								+ "Amplitude = " + runtimeParms[4] + "\r\n"
								+ "Fresnel = " + runtimeParms[5] + "\r\n"
								+ "\r\n"
								+ "invM = \r\n"
								+ "r0 = " + WriteRow( 0, _LTC.invM ) + "\r\n"
								+ "r1 = " + WriteRow( 1, _LTC.invM ) + "\r\n"
								+ "r2 = " + WriteRow( 2, _LTC.invM ) + "\r\n"
								+ "\r\n"
								+ "► Error:\r\n"
								+ "Avg. = " + (m_statsSumError / m_statsCounter).ToString( "G4" ) + "\r\n"
								+ "Avg. (Clipped) = " + (m_statsSumErrorWithoutHighValues / m_statsCounter).ToString( "G4" ) + "\r\n"
								+ "\r\n"
								+ "► Normalization:\r\n"
								+ "Value = " + m_lastNormalization.ToString( "G4" ) + "\r\n"
								+ "Avg. = " + (m_statsSumNormalization / m_statsNormalizationCounter).ToString( "G4" ) + "\r\n"
								+ "\r\n"
								+ "► Iterations:\r\n"
								+ "Value = " + m_lastIterationsCount + "\r\n"
								+ "Avg. = " + (m_statsSumIterations / m_statsCounter).ToString( "G4" ) + "\r\n";

			// Redraw
			textBoxFitting.Refresh();
			Refresh();

			// Let windows get updated
			Application.DoEvents();

			while ( Paused )
				Application.DoEvents();
		}

		string		WriteRow( int _rowIndex, double[,] _M ) {
			return _M[_rowIndex,0].ToString( "G4" ) + ", " + _M[_rowIndex,1].ToString( "G4" ) + ", " + _M[_rowIndex,2].ToString( "G4" );
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
			double	pdf;
			float	V = (float) m_BRDF.Eval( ref _tsView, ref _tsLight, m_roughness, out pdf );
			return V;
		}

//		BRDF_GGX_NoView	COMPARE = new BRDF_GGX_NoView();
		float	EstimateLTC( ref float3 _tsView, ref float3 _tsLight ) {
// float	pdf;
// return COMPARE.Eval( ref _tsView, ref _tsLight, m_roughness, out pdf ) * 1 / (1 + COMPARE.Lambda( _tsView.z, m_roughness )) / (4 * _tsView.z);	// Apply view-dependent part later

			float	V = (float) m_LTC.Eval( ref _tsLight );
			return V;
		}

		private void checkBoxPause_CheckedChanged( object sender, EventArgs e ) {
			Paused = checkBoxPause.Checked;
		}

		private void integerTrackbarControlRoughnessIndex_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			TrackbarValueChanged();
		}

		private void integerTrackbarControlThetaIndex_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue ) {
			TrackbarValueChanged();
		}
	}
}
