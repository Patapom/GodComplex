using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestMSBSDF
{
	public partial class AutomationForm : Form
	{
		class CanceledException : Exception {}

		TestForm		m_owner;

		bool			m_computing = false;

// 		result structure
// 			=> reflected + refracted!


		public new TestForm		Owner {
			get { return m_owner; }
			set { m_owner = value; }
		}

		TestForm.SURFACE_TYPE	SurfaceType {
			get {
				return radioButtonSurfaceTypeConductor.Checked ? TestForm.SURFACE_TYPE.CONDUCTOR : (radioButtonSurfaceTypeDielectric.Checked ? TestForm.SURFACE_TYPE.DIELECTRIC : TestForm.SURFACE_TYPE.DIFFUSE);
			}
		}

		public AutomationForm()
		{
			InitializeComponent();
		}

		void	MessageBox( string _Text ) {
			MessageBox( _Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon ) {
			System.Windows.Forms.MessageBox.Show( _Text, "MS BSDF Automation", _Buttons, _Icon );
		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			Visible = false;	// Only hide, don't close!
			e.Cancel = true;
			base.OnFormClosing( e );
		}

		private void checkBoxInit_StartSmall_CheckedChanged( object sender, EventArgs e )
		{
//			floatTrackbarControlInit_StartSmallFactor.Enabled = checkBoxInit_StartSmall.Checked;
		}

		private void radioButtonInit_UseCustomRoughness_CheckedChanged( object sender, EventArgs e )
		{
// 			floatTrackbarControlInit_CustomRoughness.Enabled = radioButtonInit_UseCustomRoughness.Checked;
		}

		private void radioButtonSurfaceType_CheckedChanged( object sender, EventArgs e )
		{
			labelParm2.Text = SurfaceType == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0";
		}

		private void buttonCompute_Click( object sender, EventArgs e )
		{
			if ( m_computing )
				throw new CanceledException();

//				MessageBox( "Fitting succeeded after " + m_Fitter.IterationsCount + " iterations.\r\nReached minimum: " + m_Fitter.FunctionMinimum, MessageBoxButtons.OK, MessageBoxIcon.Information );

			try {
			} catch ( Exception _e ) {
//				MessageBox( "An error occurred while performing lobe fitting:\r\n" + _e.Message + "\r\n\r\nLast minimum: " + m_Fitter.FunctionMinimum + " after " + m_Fitter.IterationsCount + " iterations..." );
				
			} finally {
			}
		}
	}
}
