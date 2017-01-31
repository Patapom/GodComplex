using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestFilmicCurve
{
	public partial class AdvancedParmsForm : Form
	{
		Form1	m_parent;



		public AdvancedParmsForm( Form1 _parent )
		{
			m_parent = _parent;
			InitializeComponent();
		}

		protected override void OnClosing( CancelEventArgs e ) {
			e.Cancel = true;
			Visible = false;
			base.OnClosing( e );
		}

		private void buttonReset_Click( object sender, EventArgs e )
		{
			floatTrackbarControlMinLuminance.Value = 0.1f;
			floatTrackbarControlMaxLuminance.Value = 2000.0f;
			floatTrackbarControlAdaptationSpeedBright.Value = 0.99f;
			floatTrackbarControlAdaptationSpeedDark.Value = 0.99f;
		}
	}
}
