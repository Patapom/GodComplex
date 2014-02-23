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
		public Form1()
		{
			InitializeComponent();
		}

		private void radioButtonSchlick_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.FresnelType = OutputPanel.FRESNEL_TYPE.SCHLICK;
		}

		private void radioButtonPrecise_CheckedChanged( object sender, EventArgs e )
		{
			outputPanel1.FresnelType = OutputPanel.FRESNEL_TYPE.PRECISE;
		}

		private void floatTrackbarControl1_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			outputPanel1.IOR = _Sender.Value;
		}

		private void panelColor_Click( object sender, EventArgs e )
		{
			if ( colorDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			outputPanel1.SpecularTint = colorDialog1.Color;
			panelColor.BackColor = colorDialog1.Color;
		}
	}
}
