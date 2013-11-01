using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace TestGradientPNG
{
	public partial class Form1 : Form
	{
		public unsafe Form1()
		{
			InitializeComponent();

			
		}

		private void floatTrackbarControlScaleX_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleX = _Sender.Value;
		}

		private void floatTrackbarControlScaleY_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleY = _Sender.Value;
		}

		private void floatTrackbarControlWhitePoint_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlA_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.A = _Sender.Value;
		}

		private void floatTrackbarControlB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.B = _Sender.Value;
		}

		private void floatTrackbarControlC_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.C = _Sender.Value;
		}

		private void floatTrackbarControlD_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.D = _Sender.Value;
		}

		private void floatTrackbarControlE_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.E = _Sender.Value;
		}

		private void floatTrackbarControlF_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.F = _Sender.Value;
		}
	}
}
