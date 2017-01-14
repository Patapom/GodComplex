//////////////////////////////////////////////////////////////////////////
// This is a simple tool intented for artists that still have difficulties computing
//	and understanding color values in linear or sRGB color profiles...
// You just enter any value in any form from one profile or the other and click the 
//	arrows to make the conversion into the opposite profile
//////////////////////////////////////////////////////////////////////////
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sRGBLinearConverter
{
	public partial class ConverterForm : Form
	{
		Color		m_color_sRGB = Color.White;
		Color		m_color_Linear = Color.White;

		bool		m_lastChangeIsInteger = false;

		public ConverterForm()
		{
			InitializeComponent();

			UpdateColorAndText_sRGB();
			UpdateColorAndText_Linear();
		}

		private void buttonsRGB2Linear_Click( object sender, EventArgs e )
		{
			int	R = (int) (255.0f * sRGB2Linear( m_color_sRGB.R / 255.0f ));
			int	G = (int) (255.0f * sRGB2Linear( m_color_sRGB.G / 255.0f ));
			int	B = (int) (255.0f * sRGB2Linear( m_color_sRGB.B / 255.0f ));
			m_color_Linear = Color.FromArgb( R, G, B );
			UpdateColorAndText_Linear();

			if ( m_lastChangeIsInteger ) {
				floatTrackbarControlNormalized_Linear.Value = sRGB2Linear( integerTrackbarControl255_sRGB.Value / 255.0f );
				integerTrackbarControl255_Linear.Value = (int) (255.0f * sRGB2Linear( integerTrackbarControl255_sRGB.Value / 255.0f ));
			} else {
				floatTrackbarControlNormalized_Linear.Value = sRGB2Linear( floatTrackbarControlNormalized_sRGB.Value );
				integerTrackbarControl255_Linear.Value = (int) (255.0f * sRGB2Linear( floatTrackbarControlNormalized_sRGB.Value ));
			}
		}

		private void buttonLinear2sRGB_Click( object sender, EventArgs e )
		{
			int	R = (int) (255.0f * Linear2sRGB( m_color_Linear.R / 255.0f ));
			int	G = (int) (255.0f * Linear2sRGB( m_color_Linear.G / 255.0f ));
			int	B = (int) (255.0f * Linear2sRGB( m_color_Linear.B / 255.0f ));
			m_color_sRGB = Color.FromArgb( R, G, B );
			UpdateColorAndText_sRGB();

			if ( m_lastChangeIsInteger ) {
				floatTrackbarControlNormalized_sRGB.Value = Linear2sRGB( integerTrackbarControl255_Linear.Value / 255.0f );
				integerTrackbarControl255_sRGB.Value = (int) (255.0f * Linear2sRGB( integerTrackbarControl255_Linear.Value / 255.0f ));
			} else {
				floatTrackbarControlNormalized_sRGB.Value = Linear2sRGB( floatTrackbarControlNormalized_Linear.Value );
				integerTrackbarControl255_sRGB.Value = (int) (255.0f * Linear2sRGB( floatTrackbarControlNormalized_Linear.Value ));
			}
		}

		private void UpdateColorAndText_sRGB() {
			panelColorsRGB.BackColor = m_color_sRGB;

			float	R = m_color_sRGB.R / 255.0f;
			float	G = m_color_sRGB.G / 255.0f;
			float	B = m_color_sRGB.B / 255.0f;
			textBoxColorNormalized_sRGB.Text = R.ToString( "G2" ) + "; " + G.ToString( "G2" ) + "; " + B.ToString( "G2" );
			textBoxColor255_sRGB.Text = m_color_sRGB.R.ToString() + " " + m_color_sRGB.G.ToString() + " " + m_color_sRGB.B.ToString();
			textBoxColorWeb_sRGB.Text = m_color_sRGB.ToArgb().ToString( "X8" ).Substring( 2 );
		}

		private void UpdateColorAndText_Linear() {
			panelColorLinear.BackColor = m_color_Linear;

			float	R = m_color_Linear.R / 255.0f;
			float	G = m_color_Linear.G / 255.0f;
			float	B = m_color_Linear.B / 255.0f;
			textBoxColorNormalized_Linear.Text = R.ToString( "G2" ) + "; " + G.ToString( "G2" ) + "; " + B.ToString( "G2" );
			textBoxColor255_Linear.Text = m_color_Linear.R.ToString() + " " + m_color_Linear.G.ToString() + " " + m_color_Linear.B.ToString();
			textBoxColorWeb_Linear.Text = m_color_Linear.ToArgb().ToString( "X8" ).Substring( 2 );
		}


		float	sRGB2Linear( float _value ) {
			float	result = Math.Min( 1.0f, _value < 0.04045f ? _value / 12.92f : (float) Math.Pow( (_value + 0.055f) / 1.055f, 2.4f ) );
			return result;
		}

		float	Linear2sRGB( float _value ) {
			float	result = Math.Min( 1.0f, _value > 0.0031308f ? 1.055f * (float) Math.Pow( _value, 1.0f / 2.4f ) - 0.054f : 12.92f * _value );
			return result;
		}

		private void panelColorsRGB_Click( object sender, EventArgs e )
		{
			colorDialog.Color = m_color_sRGB;
			if ( colorDialog.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}

			m_color_sRGB = colorDialog.Color;
			UpdateColorAndText_sRGB();
		}

		private void panelColorLinear_Click( object sender, EventArgs e )
		{
			colorDialog.Color = m_color_Linear;
			if ( colorDialog.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}

			m_color_Linear = colorDialog.Color;
			UpdateColorAndText_Linear();
		}

		private void textBoxColorNormalized_sRGB_DoubleClick( object sender, EventArgs e )
		{
			Clipboard.SetText( (sender as TextBox).Text );
		}

		private void textBoxColor255_sRGB_DoubleClick( object sender, EventArgs e )
		{
			Clipboard.SetText( (sender as TextBox).Text );
		}

		private void textBoxColorWeb_sRGB_DoubleClick( object sender, EventArgs e )
		{
			Clipboard.SetText( (sender as TextBox).Text );
		}

		private void textBoxColorNormalized_Linear_DoubleClick( object sender, EventArgs e )
		{
			Clipboard.SetText( (sender as TextBox).Text );
		}

		private void textBoxColor255_Linear_DoubleClick( object sender, EventArgs e )
		{
			Clipboard.SetText( (sender as TextBox).Text );
		}

		private void textBoxColorWeb_Linear_DoubleClick( object sender, EventArgs e )
		{
			Clipboard.SetText( (sender as TextBox).Text );
		}

		private void floatTrackbarControlNormalized_Linear_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_lastChangeIsInteger = false;
		}

		private void integerTrackbarControl255_Linear_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_lastChangeIsInteger = true;
		}

		private void floatTrackbarControlNormalized_sRGB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			m_lastChangeIsInteger = false;
		}

		private void integerTrackbarControl255_sRGB_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			m_lastChangeIsInteger = true;
		}
	}
}
