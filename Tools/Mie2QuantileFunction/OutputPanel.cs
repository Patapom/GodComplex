using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Mie2QuantileFunction
{
	public partial class OutputPanel : Panel
	{
		protected Bitmap	m_Bitmap = null;

		public enum		DISPLAY_TYPE
		{
			LOG,
			POLAR,
			QUANTILES_PEAK,
			QUANTILES_OFF_PEAK,
			SCATTERING_SIMULATION,
		}

		private double[]	m_Phase = null;
		private double		m_PhaseMin = 0.0f;
		private double		m_PhaseMax = 0.0f;
		public double[]		Phase
		{
			get { return m_Phase; }
			set
			{
				if ( value == null )
					return;

				m_Phase = value;
				m_PhaseMin = float.PositiveInfinity;
				m_PhaseMax = 0.0f;
				for ( int AngleIndex=0; AngleIndex < m_Phase.Length; AngleIndex++ )
				{
					m_PhaseMin = Math.Min( m_PhaseMin, m_Phase[AngleIndex] );
					m_PhaseMax = Math.Max( m_PhaseMax, m_Phase[AngleIndex] );
				}
			}
		}

		private struct QuantileInfos
		{
			public float	m_AngleStart;
			public float	m_AngleEnd;
			public float	m_SumPhase;
			public float	m_SumPhaseTotal;
		}

		private QuantileInfos	m_PhaseInfosPeak = new QuantileInfos();
		private float[]	m_PhaseQuantilesPeak = null;
		public float[]	PhaseQuantilesPeak
		{
			get { return m_PhaseQuantilesPeak; }
			set { m_PhaseQuantilesPeak = value; }
		}

		private QuantileInfos	m_PhaseInfosOffPeak = new QuantileInfos();
		private float[]	m_PhaseQuantilesOffPeak = null;
		public float[]	PhaseQuantilesOffPeak
		{
			get { return m_PhaseQuantilesOffPeak; }
			set { m_PhaseQuantilesOffPeak = value; }
		}

		private DISPLAY_TYPE	m_Type = DISPLAY_TYPE.LOG;
		public  DISPLAY_TYPE	DisplayType
		{
			get { return m_Type; }
			set
			{
				m_Type = value;
				UpdateBitmap();
			}
		}

		public OutputPanel()
		{
			InitializeComponent();
		}

		public OutputPanel( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		public void		ScatteringReShoot()
		{
			UpdateBitmap();
		}

		public void		SetQuantileRanges( bool _Peak, float _AngleStart, float _AngleEnd, float _SumPhase, float _TotalPhase )
		{
			QuantileInfos	Infos = new QuantileInfos() {
				m_AngleStart = _AngleStart,
				m_AngleEnd = _AngleEnd,
				m_SumPhase = _SumPhase,
				m_SumPhaseTotal = _TotalPhase
			};
			if ( _Peak )
				m_PhaseInfosPeak = Infos;
			else
				m_PhaseInfosOffPeak = Infos;
		}

		delegate float	EvalDelegate( float x );
		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				if ( m_Phase == null )
					return;

				switch ( m_Type )
				{
					case DISPLAY_TYPE.LOG:
						{
							G.DrawLine( Pens.Black, 10, 0, 10, Height );
							G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

							double	LogPhaseMin = Math.Floor( Math.Log10( m_PhaseMin ) );
							double	LogPhaseMax = Math.Ceiling( Math.Log10( m_PhaseMax ) );

							EvalDelegate	Eval = ( float _x ) => {
								int		AngleIndex = (int) ((m_Phase.Length-1) * _x);

								double	Num = Math.Log10( m_Phase[AngleIndex] ) - LogPhaseMin;
								double	Den = LogPhaseMax - LogPhaseMin;
								double	P = Num / Den;
								return (float) P;
							};

							float	px = 0.0f;
							float	py = Eval( 0.0f );
							for ( int X=10; X <= Width; X++ )
							{
			 					float	x = (float) (X-10.0f) / (Width - 10);

								float	y = Eval( x );
								DrawLine( G, px, py, x, y, Pens.Black );

								px = x;
								py = y;
							}
						}
						break;

					case DISPLAY_TYPE.POLAR:
						{
							G.DrawLine( Pens.Black, 0, Height/2, Width, Height/2 );
							G.DrawLine( Pens.Black, Width/2, 0, Width/2, Height );

							double	LogPhaseMin = Math.Floor( Math.Log10( m_PhaseMin ) );
							double	LogPhaseMax = Math.Ceiling( Math.Log10( m_PhaseMax ) );

							EvalDelegate	Eval = ( float _x ) => {
								int		AngleIndex = (int) ((m_Phase.Length-1) * _x / Math.PI);

								double	Num = Math.Log10( m_Phase[AngleIndex] ) - LogPhaseMin;
								double	Den = LogPhaseMax - LogPhaseMin;
								double	P = Num / Den;
								return (float) P;
							};

							for ( int i=0; i < 1800; i++ )
							{
								float	Angle0 = (float) (Math.PI * i / 1800.0f);
								float	Angle1 = (float) (Math.PI * (i+1) / 1800.0f);
								float	P0 = 0.5f * Width * Eval( Angle0 );
								float	P1 = 0.5f * Width * Eval( Angle1 );

								float	C0 = (float) Math.Cos( Angle0 );
								float	C1 = (float) Math.Cos( Angle1 );
								float	S0 = (float) Math.Sin( Angle0 );
								float	S1 = (float) Math.Sin( Angle1 );

								G.DrawLine( Pens.Black, 0.5f * Width + P0 * C0, 0.5f * Height + P0 * S0, 0.5f * Width + P1 * C1, 0.5f * Height + P1 * S1 );
								G.DrawLine( Pens.Black, 0.5f * Width + P0 * C0, 0.5f * Height - P0 * S0, 0.5f * Width + P1 * C1, 0.5f * Height - P1 * S1 );
							}
						}
						break;

					case DISPLAY_TYPE.QUANTILES_PEAK:
					case DISPLAY_TYPE.QUANTILES_OFF_PEAK:
						{
							G.DrawLine( Pens.Black, 10, 0, 10, Height );
							G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

							QuantileInfos	Infos;
							EvalDelegate	Eval = null;
							if ( m_Type == DISPLAY_TYPE.QUANTILES_PEAK )
							{
								Eval = ( float _x ) => {
									int		QuantileIndex = (int) ((m_PhaseQuantilesPeak.Length-1) * _x);

									float	R = m_PhaseQuantilesPeak[QuantileIndex] / (m_PhaseInfosPeak.m_AngleEnd * (float) Math.PI / 180.0f);
									return R;
								};
								Infos = m_PhaseInfosPeak;
							}
							else
							{
								Eval = ( float _x ) => {
									int		QuantileIndex = (int) ((m_PhaseQuantilesOffPeak.Length-1) * _x);

									float	R = m_PhaseQuantilesOffPeak[QuantileIndex] / (float) Math.PI;
									return R;
								};
								Infos = m_PhaseInfosOffPeak;
							}

							float	px = 0.0f;
							float	py = Eval( 0.0f );
							for ( int X=10; X <= Width; X++ )
							{
			 					float	x = (float) (X-10.0f) / (Width - 10);

								float	y = Eval( x );
								DrawLine( G, px, py, x, y, Pens.Black );

								px = x;
								py = y;
							}

							// Draw some info
							G.DrawString( "Angle Start = " + Infos.m_AngleStart + "°", Font, Brushes.Black, 20, 20*1 );
							G.DrawString( "Angle End = " + Infos.m_AngleEnd + "°", Font, Brushes.Black, 20, 20*2 );
							G.DrawString( "Total Phase = " + Infos.m_SumPhaseTotal, Font, Brushes.Black, 20, 20*3 );
							G.DrawString( "Sum Phase = " + Infos.m_SumPhase + " (" + (100.0 * Infos.m_SumPhase / Infos.m_SumPhaseTotal).ToString( "G6" ) + "%)", Font, Brushes.Black, 20, 20*4 );
						}
						break;

					case DISPLAY_TYPE.SCATTERING_SIMULATION:
						{
							G.DrawLine( Pens.Black, 0, Height/2, Width, Height/2 );
							G.DrawLine( Pens.Black, Width/2, 0, Width/2, Height );

							// Shoot random numbers and stack them according to phase tables
							int[]		Accum = new int[1080];
							Random		RNG = new Random();
							float		PeakLimit = m_PhaseInfosPeak.m_SumPhase / m_PhaseInfosPeak.m_SumPhaseTotal;
							float		NormPeak = 1.0f / PeakLimit;
							float		NormOffPeak = 1.0f / (1.0f - PeakLimit);
							int			PeakArraySize = m_PhaseQuantilesPeak.Length;
							int			OffPeakArraySize = m_PhaseQuantilesOffPeak.Length;

							int			MinAccum = 100000000;
							int			MaxAccum = 0;
							for ( int i=0; i < 10000000; i++ )
							{
								float	R = (float) RNG.NextDouble();
								float	Angle;
								if ( R < PeakLimit )
								{	// Peak draw
									int	QuantileIndex = (int) Math.Floor( R * NormPeak * PeakArraySize );
									Angle = m_PhaseQuantilesPeak[Math.Min( PeakArraySize-1, QuantileIndex )];
								}
								else
								{	// Off-peak draw
									int	QuantileIndex = (int) Math.Floor( (R-PeakLimit) * NormOffPeak * OffPeakArraySize );
									Angle = m_PhaseQuantilesOffPeak[Math.Min( OffPeakArraySize-1, QuantileIndex )];
								}

								int		nAngle = (int) Math.Floor( (Angle * 1080) / Math.PI );
								Accum[nAngle]++;
								MinAccum = Math.Min( MinAccum, Accum[nAngle] );
								MaxAccum = Math.Max( MaxAccum, Accum[nAngle] );
							}

							float	Normalizer = 1.0f / MaxAccum;

							double	LogPhaseMin = Math.Floor( Math.Log10( Math.Max( 1, MinAccum ) ) );
							double	LogPhaseMax = Math.Ceiling( Math.Log10( MaxAccum ) );

							EvalDelegate	Eval = ( float _x ) => {
								int		AngleIndex = (int) Math.Min( 1079, 1080 * _x / Math.PI );
//								float	P = Accum[AngleIndex] * Normalizer;
//								float	P = (float) (Math.Log10( Math.Max( 1, Accum[AngleIndex] ) ) / Math.Log10( MaxAccum ));
//								return P;

								double	Num = Math.Log10( Math.Max( 1, Accum[AngleIndex] ) ) - LogPhaseMin;
								double	Den = LogPhaseMax - LogPhaseMin;
								double	P = Num / Den;
								return (float) P;
							};

							for ( int i=0; i < 1800; i++ )
							{
								float	Angle0 = (float) (Math.PI * i / 1800.0f);
								float	Angle1 = (float) (Math.PI * (i+1) / 1800.0f);
								float	P0 = 0.5f * Width * Eval( Angle0 );
								float	P1 = 0.5f * Width * Eval( Angle1 );

								float	C0 = (float) Math.Cos( Angle0 );
								float	C1 = (float) Math.Cos( Angle1 );
								float	S0 = (float) Math.Sin( Angle0 );
								float	S1 = (float) Math.Sin( Angle1 );

								G.DrawLine( Pens.Black, 0.5f * Width + P0 * C0, 0.5f * Height + P0 * S0, 0.5f * Width + P1 * C1, 0.5f * Height + P1 * S1 );
								G.DrawLine( Pens.Black, 0.5f * Width + P0 * C0, 0.5f * Height - P0 * S0, 0.5f * Width + P1 * C1, 0.5f * Height - P1 * S1 );
							}
						}
						break;
				}
			}

			Invalidate();
		}

		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1 )
		{
			DrawLine( G, x0, y0, x1, y1, Pens.Black );
		}
		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1, Pen _Pen )
		{
			float	X0 = 10 + (Width-20) * x0;
			float	Y0 = Height - 10 - (Height-20) * y0;
			float	X1 = 10 + (Width-20) * x1;
			float	Y1 = Height - 10 - (Height-20) * y1;
			G.DrawLine( _Pen, X0, Y0, X1, Y1 );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			UpdateBitmap();
			Invalidate();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
			else
				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
		}
	}
}
