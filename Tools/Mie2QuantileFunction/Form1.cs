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
	public partial class Form1 : Form, IComparer<KeyValuePair<double,int>>, IComparer<float>, IComparer<int>
	{
		private const int		QUANTILE_TEXTURE_SIZE = 65536;

		public double[][]		m_PhaseRGB = new double[3][];
		public double[]			m_Phase = null;	// Monochromatic

		public float[]			m_PhaseAnglesQuantilePeak = new float[QUANTILE_TEXTURE_SIZE];
		public float[]			m_PhaseAnglesQuantileOffPeak = new float[QUANTILE_TEXTURE_SIZE];

		public Form1()
		{
			InitializeComponent();

		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			panelOutputScattering.DisplayType = OutputPanel.DISPLAY_TYPE.POLAR;

			Rebuild( floatTrackbarControlPeakAngle.Value );
		}

		void	Rebuild( float _PeakAngle )
		{
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

			int		PeakOffStartAngleIndex = (int) (AnglesCount * _PeakAngle / 180);	// Angle where to start counting from to cut the strong forward peak

			double	SumPhaseTotal = 0.0;
			double	SumPhasePeak = 0.0;
			double	SumPhaseOffPeak = 0.0;

			double	SumPhaseTotalLog = 0.0;
			double	SumPhasePeakLog = 0.0;
			double	SumPhaseOffPeakLog = 0.0;

			m_Phase = new double[AnglesCount];
			for ( AngleIndex=0; AngleIndex < AnglesCount; AngleIndex++ )
			{
				double	PhaseMono = 0.2126f * m_PhaseRGB[0][AngleIndex] + 0.7152f * m_PhaseRGB[0][AngleIndex] + 0.0722f * m_PhaseRGB[0][AngleIndex];
				m_Phase[AngleIndex] = PhaseMono;

				SumPhaseTotal += PhaseMono;
				if ( AngleIndex < PeakOffStartAngleIndex )
					SumPhasePeak += PhaseMono;
				else
					SumPhaseOffPeak += PhaseMono;

				PhaseMono = Math.Log10( PhaseMono );
				SumPhaseTotalLog += PhaseMono;
				if ( AngleIndex < PeakOffStartAngleIndex )
					SumPhasePeakLog += PhaseMono;
				else
					SumPhaseOffPeakLog += PhaseMono;
			}

			double	PeakRatio = SumPhasePeak / SumPhaseTotal;
			double	OffPeakRatio = SumPhaseOffPeak / SumPhaseTotal;

			double	PeakRatioLog = SumPhasePeakLog / SumPhaseTotalLog;
			double	OffPeakRatioLog = SumPhaseOffPeakLog / SumPhaseTotalLog;

			//////////////////////////////////////////////////////////////////////////
			// Compute quantile function

			// 1] Build a list of pairs (normalized phase, angle)
			List<KeyValuePair<double,int>>	BisouOffPeak = new List<KeyValuePair<double,int>>();
			List<KeyValuePair<double,int>>	BisouPeak = new List<KeyValuePair<double,int>>();
			for ( AngleIndex=0; AngleIndex < AnglesCount; AngleIndex++ )
			{
				if ( AngleIndex < PeakOffStartAngleIndex )
					BisouPeak.Add( new KeyValuePair<double,int>( m_Phase[AngleIndex] / SumPhasePeak, AngleIndex ) );
				else
					BisouOffPeak.Add( new KeyValuePair<double,int>( m_Phase[AngleIndex] / SumPhaseOffPeak, AngleIndex ) );
			}

			// 2] Sort the list from most important phase to least important
			BisouPeak.Sort( this );
			BisouOffPeak.Sort( this );

			// 3] Fill peak quantiles
//			List<float>	QuantilesPeak = new List<float>();
			List<int>	QuantilesPeak = new List<int>();
			int QuantileIndex=0;
			for ( AngleIndex=0; AngleIndex < BisouPeak.Count; AngleIndex++ )
			{
				KeyValuePair<double,int>	Entry = BisouPeak[AngleIndex];

				// Fill up the quantile texture up to the amount required to represent the phase importance of that angle
//				int	QuantilesCount = Math.Max( 1, (int) Math.Floor( QUANTILE_TEXTURE_SIZE * Entry.Key ));
				int	QuantilesCount = (int) Math.Floor( QUANTILE_TEXTURE_SIZE * Entry.Key );

//				int	QuantileEnd = Math.Min( QUANTILE_TEXTURE_SIZE, QuantileIndex + QuantilesCount );
				int	QuantileEnd = QuantileIndex + QuantilesCount;
				for ( ; QuantileIndex < QuantileEnd; QuantileIndex++ )
//					m_PhaseAnglesQuantilePeak[QuantileIndex] = (float) (Entry.Value * Math.PI / AnglesCount);
//					QuantilesPeak.Add( (float) (Entry.Value * Math.PI / AnglesCount) );
					QuantilesPeak.Add( Entry.Value );
			}

			QuantilesPeak.Sort( this );
//			m_PhaseAnglesQuantilePeak = QuantilesPeak.ToArray();

			// 4] Fill off-peak quantiles
//			List<float>	QuantilesOffPeak = new List<float>();
			List<int>	QuantilesOffPeak = new List<int>();
			QuantileIndex=0;
			for ( AngleIndex=0; AngleIndex < BisouOffPeak.Count; AngleIndex++ )
			{
				KeyValuePair<double,int>	Entry = BisouOffPeak[AngleIndex];

				// Fill up the quantile texture up to the amount required to represent the phase importance of that angle
//				int	QuantilesCount = Math.Max( 1, (int) Math.Floor( QUANTILE_TEXTURE_SIZE * Entry.Key ));
				int	QuantilesCount = (int) Math.Floor( QUANTILE_TEXTURE_SIZE * Entry.Key );

//				int	QuantileEnd = Math.Min( QUANTILE_TEXTURE_SIZE, QuantileIndex + QuantilesCount );
				int	QuantileEnd = QuantileIndex + QuantilesCount;
				for ( ; QuantileIndex < QuantileEnd; QuantileIndex++ )
//					m_PhaseAnglesQuantileOffPeak[QuantileIndex] = (float) (Entry.Value * Math.PI / AnglesCount);
//					QuantilesOffPeak.Add( (float) (Entry.Value * Math.PI / AnglesCount) );
					QuantilesOffPeak.Add( Entry.Value );
			}

			QuantilesOffPeak.Sort( this );
//			m_PhaseAnglesQuantileOffPeak = QuantilesOffPeak.ToArray();


			//////////////////////////////////////////////////////////////////////////
			// Rebuild a smooth complete array
			m_PhaseAnglesQuantilePeak = new float[QUANTILE_TEXTURE_SIZE];
			for ( int i=0; i < QuantilesPeak.Count-1; )
			{
				int	AngleAtI = QuantilesPeak[i];

				// Find where the angle value changes
				int	j = i;
				for ( ; j < QuantilesPeak.Count-1; j++ )
					if ( QuantilesPeak[j] != AngleAtI )
						break;	// Found a change!
				int	AngleAtJ = QuantilesPeak[Math.Min( QuantilesPeak.Count-1, j )];

				// Fill the array with a linear interpolation of angles
				long	TargetIndex0 = (long)i * (long)(QUANTILE_TEXTURE_SIZE-1) / (long)(QuantilesPeak.Count-1);
				long	TargetIndex1 = (long)j * (long)(QUANTILE_TEXTURE_SIZE-1) / (long)(QuantilesPeak.Count-1);
				float	StartAngle = (float) (AngleAtI * Math.PI / AnglesCount);
				float	EndAngle = (float) (AngleAtJ * Math.PI / AnglesCount);

				for ( long k=TargetIndex0; k <= TargetIndex1; k++ )
					m_PhaseAnglesQuantilePeak[k] = StartAngle + (EndAngle - StartAngle) * (k - TargetIndex0) / (TargetIndex1 - TargetIndex0);

				i = j;
			}

			// Do the same for off peak array
			m_PhaseAnglesQuantileOffPeak = new float[QUANTILE_TEXTURE_SIZE];
			for ( int i=0; i < QuantilesOffPeak.Count-1; )
			{
				int	AngleAtI = QuantilesOffPeak[i];

				// Find where the angle value changes
				int	j = i;
				for ( ; j < QuantilesOffPeak.Count-1; j++ )
					if ( QuantilesOffPeak[j] != AngleAtI )
						break;	// Found a change!
				int	AngleAtJ = QuantilesOffPeak[Math.Min( QuantilesOffPeak.Count-1, j )];

				// Fill the array with a linear interpolation of angles
				long	TargetIndex0 = (long)i * (long)(QUANTILE_TEXTURE_SIZE-1) / (long)(QuantilesOffPeak.Count-1);
				long	TargetIndex1 = (long)j * (long)(QUANTILE_TEXTURE_SIZE-1) / (long)(QuantilesOffPeak.Count-1);
				float	StartAngle = (float) (AngleAtI * Math.PI / AnglesCount);
				float	EndAngle = (float) (AngleAtJ * Math.PI / AnglesCount);

				for ( long k=TargetIndex0; k <= TargetIndex1; k++ )
					m_PhaseAnglesQuantileOffPeak[k] = StartAngle + (EndAngle - StartAngle) * (k - TargetIndex0) / (TargetIndex1 - TargetIndex0);

				i = j;
			}

			// Write result to file
			using ( FileStream S = new FileInfo( "Mie" + QUANTILE_TEXTURE_SIZE +"x2.float" ).Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) )
				{
					for ( int i=0; i < QUANTILE_TEXTURE_SIZE; i++ )
						W.Write( m_PhaseAnglesQuantilePeak[i] );
					for ( int i=0; i < QUANTILE_TEXTURE_SIZE; i++ )
						W.Write( m_PhaseAnglesQuantileOffPeak[i] );
				}


			// Update panel
			panelOutputScattering.Phase = m_Phase;
			panelOutputScattering.PhaseQuantilesPeak = m_PhaseAnglesQuantilePeak;
			panelOutputScattering.PhaseQuantilesOffPeak = m_PhaseAnglesQuantileOffPeak;
			panelOutputScattering.SetQuantileRanges( true, 0, _PeakAngle, (float) SumPhasePeak, (float) SumPhaseTotal );
			panelOutputScattering.SetQuantileRanges( false, _PeakAngle, 180, (float) SumPhaseOffPeak, (float) SumPhaseTotal );

			panelOutputScattering.DisplayType = panelOutputScattering.DisplayType;
		}

		private void radioButtonPolar_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonPolar.Checked )
				panelOutputScattering.DisplayType = OutputPanel.DISPLAY_TYPE.POLAR;
		}

		private void radioButtonLog_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonLog.Checked )
				panelOutputScattering.DisplayType = OutputPanel.DISPLAY_TYPE.LOG;
		}

		private void radioButtonBuckets_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonQuantilesPeak.Checked )
				panelOutputScattering.DisplayType = OutputPanel.DISPLAY_TYPE.QUANTILES_PEAK;
		}

		private void radioButtonQuantilesOffPeak_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonQuantilesOffPeak.Checked )
				panelOutputScattering.DisplayType = OutputPanel.DISPLAY_TYPE.QUANTILES_OFF_PEAK;
		}

		private void radioButtonScattering_CheckedChanged( object sender, EventArgs e )
		{
			if ( radioButtonScattering.Checked )
				panelOutputScattering.DisplayType = OutputPanel.DISPLAY_TYPE.SCATTERING_SIMULATION;
		}

		private void buttonReShoot_Click( object sender, EventArgs e )
		{
			panelOutputScattering.ScatteringReShoot();
		}

		private void floatTrackbarControlPeakAngle_SliderDragStop( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue )
		{
			Rebuild( floatTrackbarControlPeakAngle.Value );
		}

		#region IComparer<KeyValuePair<double,int>> Members

		public int Compare( KeyValuePair<double, int> x, KeyValuePair<double, int> y )
		{
			if ( x.Key < y.Key )
				return 1;
			else if ( x.Key > y.Key )
				return -1;
			else
				return 0;
		}

		#endregion

		#region IComparer<float> Members

		public int Compare( float x, float y )
		{
			if ( x < y )
				return -1;
			else if ( x > y )
				return 1;
			else
				return 0;
		}

		#endregion

		#region IComparer<int> Members

		public int Compare( int x, int y )
		{
			if ( x < y )
				return -1;
			else if ( x > y )
				return 1;
			else
				return 0;
		}

		#endregion
	}
}
