using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestFresnel
{
	public partial class Form1 : Form
	{
		enum READING_STATE
		{
			UNKNOWN,
			N,
			K,
		}

		// F0 = ((n2 - n1) / (n2 + n1))²
		// Assuming n1=1 (air)
		public static float	IOR_to_F0( float _IOR ) {
			float	F0 = (_IOR-1.0f) / (_IOR+1.0f);
					F0 *= F0;
			return F0;
		}

		// We look for n2 so:
		//	n2 = (1+a)/(1-a) with a = sqrt(F0)
		public static float	F0_to_IOR( float _F0 ) {
			float	a = (float) Math.Sqrt( _F0 );
			float	IOR = (1.0f + a) / (1.001f - a);
			return IOR;
		}

		public float[,]	m_IntegralSpecularReflection = new float[64,2];

		public Form1()
		{
			InitializeComponent();

			using ( System.IO.FileStream S = new System.IO.FileInfo( "../TestAreaLight/BRDF0_64x64.table" ).OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) )
					for ( int i=0; i < 64; i++ ) {
						m_IntegralSpecularReflection[i,0] = R.ReadSingle();
						m_IntegralSpecularReflection[i,1] = R.ReadSingle();
					}
			outputPanel2.m_Table = m_IntegralSpecularReflection;
		}

		private void radioButtonSchlick_CheckedChanged( object sender, EventArgs e ) {
			outputPanel1.FresnelType = OutputPanel.FRESNEL_TYPE.SCHLICK;
			outputPanel2.FresnelType = OutputPanel2.FRESNEL_TYPE.SCHLICK;
		}

		private void radioButtonPrecise_CheckedChanged( object sender, EventArgs e ) {
			outputPanel1.FresnelType = OutputPanel.FRESNEL_TYPE.PRECISE;
			outputPanel2.FresnelType = OutputPanel2.FRESNEL_TYPE.PRECISE;
		}

		private void radioButtonIOR_CheckedChanged( object sender, EventArgs e ) {
			floatTrackbarControlIOR.Enabled = true;
			panelColor.Enabled = false;
		}

		private void radioButtonSpecularTint_CheckedChanged( object sender, EventArgs e ) {
			floatTrackbarControlIOR.Enabled = false;
			panelColor.Enabled = true;
		}

		private void checkBoxData_CheckedChanged( object sender, EventArgs e ) {
			floatTrackbarControlIOR.Enabled = !checkBoxData.Checked;
			panelColor.Enabled = !checkBoxData.Checked;
			outputPanel1.FromData = checkBoxData.Checked;
		}

		Color	IOR_to_Color( float _IOR_red, float _IOR_green, float _IOR_blue ) {
			float	F0_red = IOR_to_F0( _IOR_red );
			float	F0_green = IOR_to_F0( _IOR_green );
			float	F0_blue = IOR_to_F0( _IOR_blue );

			Color	result = Color.FromArgb( (int) (255 * F0_red), (int) (255 * F0_green), (int) (255 * F0_blue) );
			return result;
		}

		private void floatTrackbarControl1_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {

			outputPanel2.MaxIOR = floatTrackbarControlIOR.VisibleRangeMax;	// So we match the visible range

			if ( !floatTrackbarControlIOR.Enabled )
				return;	// Changed externally from specular tint modification, don't change panels' values!

			outputPanel1.IOR_red = _Sender.Value;
			outputPanel1.IOR_green = _Sender.Value;
			outputPanel1.IOR_blue = _Sender.Value;

			outputPanel2.IOR = _Sender.Value;

			panelColor.BackColor = IOR_to_Color( _Sender.Value, _Sender.Value, _Sender.Value );
		}

		private void panelColor_Click( object sender, EventArgs e ) {
			colorDialog1.Color = IOR_to_Color( outputPanel1.IOR_red, outputPanel1.IOR_green, outputPanel1.IOR_blue );
			if ( colorDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			panelColor.BackColor = colorDialog1.Color;

			float	F0_red = colorDialog1.Color.R / 255.0f;
			float	F0_green = colorDialog1.Color.G / 255.0f;
			float	F0_blue = colorDialog1.Color.B / 255.0f;

			float	IOR_red = F0_to_IOR( F0_red );
			float	IOR_green = F0_to_IOR( F0_green );
			float	IOR_blue = F0_to_IOR( F0_blue );

			floatTrackbarControlIOR.Value = Math.Max( Math.Max( IOR_red, IOR_green ), IOR_blue );
			floatTrackbarControlIOR.VisibleRangeMax = Math.Max( 10.0f, 2.0f * floatTrackbarControlIOR.Value );

			outputPanel1.IOR_red = IOR_red;
			outputPanel1.IOR_green = IOR_green;
			outputPanel1.IOR_blue = IOR_blue;

			outputPanel2.IOR = Math.Max( Math.Max( IOR_red, IOR_green ), IOR_blue );
		}

		private void buttonLoadData_Click( object sender, EventArgs e ) {
			if ( openFileDialogRefract.ShowDialog( this ) != DialogResult.OK )
				return;

			try
			{
				string	Content = "";
				using ( System.IO.StreamReader S = new System.IO.FileInfo( openFileDialogRefract.FileName ).OpenText() )
					Content = S.ReadToEnd();

				Content = Content.Replace( "\r", "" );
				string[]	Lines = Content.Split( '\n' );

				List<OutputPanel.RefractionData>	Data = new List<OutputPanel.RefractionData>();
				READING_STATE	State = READING_STATE.UNKNOWN;
				bool			InsertExisting = false;
				for ( int LineIndex=0; LineIndex < Lines.Length; LineIndex++ )
				{
					string		Line = Lines[LineIndex];
					string[]	Values = Line.Split( ' ', '\t' );
					if ( Line == "" || Values.Length == 0 )
						continue;	// Skip empty lines
					if ( Values.Length != 2 )
						throw new Exception( "Unexpected line " + LineIndex + " does not contain exactly 2 values! (" + Line + ")" );

					if ( Values[0].ToLower() == "wl" )
					{
						if ( Values[1].ToLower() == "n" )
							State = READING_STATE.N;
						else if ( Values[1].ToLower() == "k" )
							State = READING_STATE.K;
						else
							throw new Exception( "Unexpected data type \"" + Values[1] + "\" at line " + LineIndex + ". Expecting either n or k." );
						InsertExisting = Data.Count > 0;	// Populate list or insert in existing one?
						continue;	// Skip this descriptor line
					}

					float	wl;
					if ( !float.TryParse( Values[0], out wl ) )
						throw new Exception( "Failed to parse wavelength at line " + LineIndex );
					float	v;
					if ( !float.TryParse( Values[1], out v ) )
						throw new Exception( "Failed to parse " + (State == READING_STATE.N ? "n" : "k") + " at line " + LineIndex );

					OutputPanel.RefractionData	D = null;
					if ( InsertExisting )
					{	// Find existing slot in list
						foreach ( OutputPanel.RefractionData ExistingD in Data )
							if ( Math.Abs( ExistingD.Wavelength - wl ) < 1e-6f )
							{	// Found it!
								D = ExistingD;
								break;
							}
						if ( D == null )
							throw new Exception( "Failed to retrieve wavelength " + wl + " in existing array of values populated by " + (State == READING_STATE.N ? "k" : "n") + " values at line " + LineIndex );
					}
					else
					{	// Simply append
						D = new OutputPanel.RefractionData() { Wavelength = wl };
						Data.Add( D );
					}

					if ( State == READING_STATE.N )
						D.n = v;
					else
						D.k = v;
				}

				outputPanel1.Data = Data.ToArray();
				checkBoxData.Checked = true;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( this, "Failed to load data file:" + _e.Message, "Argh!", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void floatTrackbarControlVerticalScale_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			outputPanel2.VerticalScale = floatTrackbarControlVerticalScale.Value;
		}

		private void checkBoxusePreComputedtable_CheckedChanged( object sender, EventArgs e ) {
			if ( checkBoxusePreComputedTable.Checked )
				outputPanel2.FresnelType = OutputPanel2.FRESNEL_TYPE.TABLE;
			else
				outputPanel2.FresnelType = radioButtonPrecise.Checked ? OutputPanel2.FRESNEL_TYPE.PRECISE : OutputPanel2.FRESNEL_TYPE.SCHLICK;
			floatTrackbarControlRoughness.Enabled = checkBoxusePreComputedTable.Checked;
			floatTrackbarControlPeakFactor.Enabled = checkBoxusePreComputedTable.Checked;
		}

		private void floatTrackbarControlRoughness_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			outputPanel2.Roughness = floatTrackbarControlRoughness.Value;
		}

		private void checkBoxPlotAgainstF0_CheckedChanged( object sender, EventArgs e ) {
			outputPanel2.PlotAgainstF0 = checkBoxPlotAgainstF0.Checked;
		}

		private void floatTrackbarControlPeakFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			outputPanel2.PeakFactor = floatTrackbarControlPeakFactor.Value;
		}

		private void floatTrackbarControlIOR_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue ) {
			if ( _Sender.Value < 10.0f )
				_Sender.VisibleRangeMax = 10.0f;	// Restore visibility range
		}
	}
}
