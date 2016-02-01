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
		TestForm		m_owner;

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

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			Visible = false;	// Only hide, don't close!
			e.Cancel = true;
			base.OnFormClosing( e );
		}

		private void checkBoxInit_StartSmall_CheckedChanged( object sender, EventArgs e )
		{
			floatTrackbarControlInit_StartSmallFactor.Enabled = checkBoxInit_StartSmall.Checked;
		}

		private void radioButtonInit_UseCustomRoughness_CheckedChanged( object sender, EventArgs e )
		{
			floatTrackbarControlInit_CustomRoughness.Enabled = radioButtonInit_UseCustomRoughness.Checked;
		}

		private void radioButtonSurfaceType_CheckedChanged( object sender, EventArgs e )
		{
			labelParm2.Text = SurfaceType == TestForm.SURFACE_TYPE.DIFFUSE ? "Albedo" : "F0";
		}


	}
}
