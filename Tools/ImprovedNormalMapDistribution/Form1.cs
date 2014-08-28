using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImprovedNormalMapDistribution
{
	/// <summary>
	/// Computes an improved normal distribution following http://rgba32.blogspot.fr/2011/02/improved-normal-map-distributions.html
	/// I'm attempting to use the more advanced quartic function f(x,y)=(1-x²)(1-y²) for which there is a more involved intersection computation.
	/// 
	/// I'm first trying with Newton-Raphson then I'll use the quartic intersection that can obviously have only a single intersection...
	/// </summary>
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void floatTrackbarControlPhi_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			UpdateNormal();
		}

		private void floatTrackbarControlTheta_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			UpdateNormal();
		}

		void	UpdateNormal()
		{
			double	Phi = floatTrackbarControlPhi.Value * Math.PI / 180;
			double	Theta = floatTrackbarControlTheta.Value * Math.PI / 180;

			double	X = Math.Cos( Phi ) * Math.Sin( Theta );
			double	Y = Math.Sin( Phi ) * Math.Sin( Theta );
			double	Z = Math.Cos( Theta );

			if ( Z < 1e-12 )
				Z = 0.0;

			outputPanel1.SetNormal( X, Y, Z );
			outputPanel21.Theta = Theta;
		}
	}
}
