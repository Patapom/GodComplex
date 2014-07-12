using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace Mie2QuantileFunction
{
	public partial class Form1 : Form
	{
		private const int		QUANTILE_TEXTURE_SIZE = 2048;

		public double[][]		m_PhaseRGB = new double[3][];
		public double[]			m_Phase = null;	// Monochromatic

		public int[]			m_Buckets = new int[QUANTILE_TEXTURE_SIZE];

		public Form1()
		{
			InitializeComponent();

			//////////////////////////////////////////////////////////////////////////
			// Load phase data from file
			string	Content = null;
			using ( StreamReader R = new FileInfo( "Phase - 7µm - Gamma=2 - N0=4x10^8 - VALUES ONLY.txt" ).OpenText() )
				Content = R.ReadToEnd();

			string[]	Lines = Content.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
			int			AnglesCount = Lines.Length / 3;

			m_PhaseRGB[0] = new double[AnglesCount];
			m_PhaseRGB[1] = new double[AnglesCount];
			m_PhaseRGB[2] = new double[AnglesCount];
			int		AngleIndex = 0;
			for ( int LineIndex=0; LineIndex < Lines.Length; LineIndex++ )
			{
				string[]	Components = Lines[LineIndex].Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );

				double	Angle = double.Parse( Components[0] );
				double	Wavelength = double.Parse( Components[1] );
				double	Parallel = double.Parse( Components[2] );
				double	Perpendicular = double.Parse( Components[3] );

				// Retrieve RGB wavelength index
				int		RGB = -1;
				if ( Math.Abs( Wavelength - 0.65 ) < 1e-3 )
					RGB = 0;
				else if ( Math.Abs( Wavelength - 0.51 ) < 1e-3 )
					RGB = 1;
				else if ( Math.Abs( Wavelength - 0.44 ) < 1e-3 )
					RGB = 2;

				// Retrieve integer angle index
				m_PhaseRGB[RGB][AngleIndex] = 0.5f * (Parallel + Perpendicular);

				if ( RGB == 2 )
					AngleIndex++;
			}

			m_Phase = new double[AnglesCount];
			for ( AngleIndex=0; AngleIndex < AnglesCount; AngleIndex++ )
				m_Phase[AngleIndex] = 0.2126f * m_PhaseRGB[0][AngleIndex] + 0.7152f * m_PhaseRGB[0][AngleIndex] + 0.0722f * m_PhaseRGB[0][AngleIndex];

			//////////////////////////////////////////////////////////////////////////
			// Compute quantile function

			// 1] Fill buckets
			for ( AngleIndex=0; AngleIndex < AnglesCount; AngleIndex++ )
			{

			}


			// Update panel
			panelOutput1.Phase = m_Phase;
			panelOutput1.DisplayType = OutputPanel.DISPLAY_TYPE.POLAR;
		}

		private void radioButtonPolar_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonPolar.Checked )
				panelOutput1.DisplayType = OutputPanel.DISPLAY_TYPE.POLAR;
		}

		private void radioButtonLog_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonLog.Checked )
				panelOutput1.DisplayType = OutputPanel.DISPLAY_TYPE.LOG;
		}

		private void radioButtonBuckets_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonBuckets.Checked )
				panelOutput1.DisplayType = OutputPanel.DISPLAY_TYPE.BUCKETS;
		}
	}
}
