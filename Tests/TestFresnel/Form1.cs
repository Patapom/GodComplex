﻿//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// This is an experiment about Fresnel reflection and complex IOR
//	• It uses Schlick's approximation or the exact formulation from Walter 2007 for dielectrics
//	• I tried fiddling with metals and complex IORs but I don't remember arriving at anything good
//		=> Instead, I implemented the "edge tint" specular color from the paper "Artist Friendly Metallic Fresnel" (2014) by Ole Gulbrandsen (http://jcgt.org/published/0003/04/03/paper.pdf).
//		http://jcgt.org/published/0003/04/03/paper.pdf 
//
// In the bottom panel:
//	• I also compute the Fresnel diffuse reflectance which is the "total Fresnel reflectance" that
//		gets the ratio of reflected agains refracted rays for the entire hemisphere of directions
//		=> It's super useful to known how much *diffuse* indirect lighting must be reflected
//			(actually, it should be what "has not been specularly reflected", so the complement of
//			 the integral of all Fresnel reflectance coefficients for the entire hemisphere)
//
//	• You can also choose to use the pre-computed BRDF table that also accounts for surface
//		roughness (although it's still not clear why it makes a difference in specularly reflected
//		light?)
//
//////////////////////////////////////////////////////////////////////////
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

namespace TestFresnel
{
	public partial class Form1 : Form {
		enum READING_STATE {
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


		public Form1() {
			InitializeComponent();

			using ( System.IO.FileStream S = new System.IO.FileInfo( "../TestAreaLight/BRDF0_64x64.table" ).OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) )
					for ( int i=0; i < 64; i++ ) {
						m_IntegralSpecularReflection[i,0] = R.ReadSingle();
						m_IntegralSpecularReflection[i,1] = R.ReadSingle();
					}
			outputPanelDiffuseFresnelReflectance.m_Table = m_IntegralSpecularReflection;

			radioButtonPrecise_CheckedChanged( null, EventArgs.Empty );
		}

		private void radioButtonSchlick_CheckedChanged( object sender, EventArgs e ) {
			outputPanelFresnelGraph.FresnelType = PanelFresnelReflectance.FRESNEL_TYPE.SCHLICK;
			outputPanelDiffuseFresnelReflectance.FresnelType = PanelDiffuseFresnelReflectance.FRESNEL_TYPE.SCHLICK;
		}

		private void radioButtonPrecise_CheckedChanged( object sender, EventArgs e ) {
			outputPanelFresnelGraph.FresnelType = PanelFresnelReflectance.FRESNEL_TYPE.PRECISE;
			outputPanelDiffuseFresnelReflectance.FresnelType = PanelDiffuseFresnelReflectance.FRESNEL_TYPE.PRECISE;
		}

		private void radioButtonIOR_CheckedChanged( object sender, EventArgs e ) {
			outputPanelFresnelGraph.IORSource = PanelFresnelReflectance.IOR_SOURCE.IOR;
//			outputPanelDiffuseFresnelReflectance.IORSource = PanelFresnelReflectance.IOR_SOURCE.IOR;
			floatTrackbarControlIOR.Enabled = true;
			panelSpecularTintNormal.Enabled = false;
			panelUseEdgeTint.Enabled = false;
		}

		private void radioButtonSpecularTint_CheckedChanged( object sender, EventArgs e ) {
			outputPanelFresnelGraph.IORSource = PanelFresnelReflectance.IOR_SOURCE.F0;
//			outputPanelDiffuseFresnelReflectance.IORSource = PanelFresnelReflectance.IOR_SOURCE.F0;
			floatTrackbarControlIOR.Enabled = false;
			panelSpecularTintNormal.Enabled = !checkBoxData.Checked;
			panelUseEdgeTint.Enabled = !checkBoxData.Checked;
		}

		private void checkBoxData_CheckedChanged( object sender, EventArgs e ) {
			floatTrackbarControlIOR.Enabled = !checkBoxData.Checked;
			panelSpecularTintNormal.Enabled = !checkBoxData.Checked;
			panelUseEdgeTint.Enabled = !checkBoxData.Checked;
			outputPanelFresnelGraph.FromData = checkBoxData.Checked;
		}

		private void checkBoxUseEdgeTint_CheckedChanged( object sender, EventArgs e ) {
			outputPanelFresnelGraph.UseEdgeTint = checkBoxUseEdgeTint.Checked;
			panelSpecularTintEdge.Enabled = checkBoxUseEdgeTint.Checked;
		}

		Color	IOR_to_Color( float _IOR_red, float _IOR_green, float _IOR_blue ) {
			float	F0_red = IOR_to_F0( _IOR_red );
			float	F0_green = IOR_to_F0( _IOR_green );
			float	F0_blue = IOR_to_F0( _IOR_blue );

			Color	result = Color.FromArgb( (int) (255 * F0_red), (int) (255 * F0_green), (int) (255 * F0_blue) );
			return result;
		}

		private void floatTrackbarControl1_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {

			outputPanelDiffuseFresnelReflectance.MaxIOR = floatTrackbarControlIOR.VisibleRangeMax;	// So we match the visible range

			if ( !floatTrackbarControlIOR.Enabled )
				return;	// Changed externally from specular tint modification, don't change panels' values!

			outputPanelFresnelGraph.IOR_red = _Sender.Value;
			outputPanelFresnelGraph.IOR_green = _Sender.Value;
			outputPanelFresnelGraph.IOR_blue = _Sender.Value;

			outputPanelDiffuseFresnelReflectance.IOR = _Sender.Value;

			panelSpecularTintNormal.BackColor = IOR_to_Color( _Sender.Value, _Sender.Value, _Sender.Value );
		}

		private void panelColor_Click( object sender, EventArgs e ) {
// 			colorDialog1.Color = IOR_to_Color( outputPanelFresnelGraph.IOR_red, outputPanelFresnelGraph.IOR_green, outputPanelFresnelGraph.IOR_blue );
			UIUtility.ColorPickerForm	colorPicker = new UIUtility.ColorPickerForm( panelSpecularTintNormal.BackColor );
			colorPicker.ColorChanged += colorPicker_ColorChanged;
			colorPicker.ShowDialog( this );
		}

		void colorPicker_ColorChanged( UIUtility.ColorPickerForm _sender ) {
			Color	pickedColor = _sender.ColorLDR;

			panelSpecularTintNormal.BackColor = pickedColor;

// 			floatTrackbarControlIOR.Value = Math.Max( Math.Max( IOR_red, IOR_green ), IOR_blue );
// 			floatTrackbarControlIOR.VisibleRangeMax = Math.Max( 10.0f, 2.0f * floatTrackbarControlIOR.Value );
// 
			outputPanelFresnelGraph.SpecularTintNormal = pickedColor;

			float	F0_red = pickedColor.R / 255.0f;
			float	F0_green = pickedColor.G / 255.0f;
			float	F0_blue = pickedColor.B / 255.0f;

			float	IOR_red = F0_to_IOR( F0_red );
			float	IOR_green = F0_to_IOR( F0_green );
			float	IOR_blue = F0_to_IOR( F0_blue );
			outputPanelFresnelGraph.IOR_red = IOR_red;
			outputPanelFresnelGraph.IOR_green = IOR_green;
			outputPanelFresnelGraph.IOR_blue = IOR_blue;
			outputPanelDiffuseFresnelReflectance.IOR = Math.Max( Math.Max( IOR_red, IOR_green ), IOR_blue );

			outputPanelFresnelGraph.Refresh();
		}

		private void panelEdgeTint_Click( object sender, EventArgs e ) {
// 			colorDialog1.Color = IOR_to_Color( outputPanelFresnelGraph.IOR_red, outputPanelFresnelGraph.IOR_green, outputPanelFresnelGraph.IOR_blue );
			UIUtility.ColorPickerForm	colorPicker = new UIUtility.ColorPickerForm( panelSpecularTintEdge.BackColor );
			colorPicker.ColorChanged += colorPicker_ColorChanged_EdgeTint;
			colorPicker.ShowDialog( this );
		}

		void colorPicker_ColorChanged_EdgeTint( UIUtility.ColorPickerForm _sender ) {
			Color	pickedColor = _sender.ColorLDR;

			panelSpecularTintEdge.BackColor = pickedColor;
			outputPanelFresnelGraph.SpecularTintEdge = pickedColor;

			outputPanelFresnelGraph.Refresh();
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

				List<PanelFresnelReflectance.RefractionData>	Data = new List<PanelFresnelReflectance.RefractionData>();
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

					PanelFresnelReflectance.RefractionData	D = null;
					if ( InsertExisting )
					{	// Find existing slot in list
						foreach ( PanelFresnelReflectance.RefractionData ExistingD in Data )
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
						D = new PanelFresnelReflectance.RefractionData() { Wavelength = wl };
						Data.Add( D );
					}

					if ( State == READING_STATE.N )
						D.n = v;
					else
						D.k = v;
				}

				outputPanelFresnelGraph.Data = Data.ToArray();
				checkBoxData.Checked = true;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( this, "Failed to load data file:" + _e.Message, "Argh!", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void floatTrackbarControlVerticalScale_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			outputPanelDiffuseFresnelReflectance.VerticalScale = floatTrackbarControlVerticalScale.Value;
		}

		private void checkBoxusePreComputedtable_CheckedChanged( object sender, EventArgs e ) {
			if ( checkBoxusePreComputedTable.Checked )
				outputPanelDiffuseFresnelReflectance.FresnelType = PanelDiffuseFresnelReflectance.FRESNEL_TYPE.TABLE;
			else
				outputPanelDiffuseFresnelReflectance.FresnelType = radioButtonPrecise.Checked ? PanelDiffuseFresnelReflectance.FRESNEL_TYPE.PRECISE : PanelDiffuseFresnelReflectance.FRESNEL_TYPE.SCHLICK;
			floatTrackbarControlRoughness.Enabled = checkBoxusePreComputedTable.Checked;
			floatTrackbarControlPeakFactor.Enabled = checkBoxusePreComputedTable.Checked;
		}

		private void floatTrackbarControlRoughness_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			outputPanelDiffuseFresnelReflectance.Roughness = floatTrackbarControlRoughness.Value;
		}

		private void checkBoxPlotAgainstF0_CheckedChanged( object sender, EventArgs e ) {
			outputPanelDiffuseFresnelReflectance.PlotAgainstF0 = checkBoxPlotAgainstF0.Checked;
		}

		private void floatTrackbarControlPeakFactor_ValueChanged( UIUtility.FloatTrackbarControl _Sender, float _fFormerValue ) {
			outputPanelDiffuseFresnelReflectance.PeakFactor = floatTrackbarControlPeakFactor.Value;
		}

		private void floatTrackbarControlIOR_SliderDragStop( UIUtility.FloatTrackbarControl _Sender, float _fStartValue ) {
			if ( _Sender.Value < 10.0f )
				_Sender.VisibleRangeMax = 10.0f;	// Restore visibility range
		}
	}
}
