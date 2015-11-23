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

		public Form1()
		{
			InitializeComponent();
		}

		private void radioButtonSchlick_CheckedChanged( object sender, EventArgs e )
		{
			floatTrackbarControl1.Enabled = true;
			panelColor.Enabled = true;
			outputPanel1.FresnelType = OutputPanel.FRESNEL_TYPE.SCHLICK;
			outputPanel2.FresnelType = OutputPanel2.FRESNEL_TYPE.SCHLICK;
		}

		private void radioButtonPrecise_CheckedChanged( object sender, EventArgs e )
		{
			floatTrackbarControl1.Enabled = true;
			panelColor.Enabled = true;
			outputPanel1.FresnelType = OutputPanel.FRESNEL_TYPE.PRECISE;
			outputPanel2.FresnelType = OutputPanel2.FRESNEL_TYPE.PRECISE;
		}

		private void checkBoxData_CheckedChanged( object sender, EventArgs e )
		{
			floatTrackbarControl1.Enabled = !checkBoxData.Checked;
			panelColor.Enabled = !checkBoxData.Checked;
			outputPanel1.FromData = checkBoxData.Checked;
		}

		private void floatTrackbarControl1_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanel1.IOR = _Sender.Value;
			outputPanel2.IOR = _Sender.Value;
		}

		private void panelColor_Click( object sender, EventArgs e )
		{
			if ( colorDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			outputPanel1.SpecularTint = colorDialog1.Color;
			outputPanel2.SpecularTint = colorDialog1.Color;
			panelColor.BackColor = colorDialog1.Color;
		}

		private void buttonLoadData_Click( object sender, EventArgs e )
		{
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
	}
}
