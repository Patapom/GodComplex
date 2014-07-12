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
			BUCKETS,
		}

		private double[]	m_Phase = null;
		private double		m_PhaseMin = 0.0f;
		private double		m_PhaseMax = 0.0f;
		public double[]			Phase
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

							EvalDelegate	Eval = ( float _x ) => {
								int		AngleIndex = (int) ((m_Phase.Length-1) * _x);

								double	Num = Math.Log10( m_Phase[AngleIndex] ) - Math.Log10( m_PhaseMin );
								double	Den = Math.Log10( m_PhaseMax ) - Math.Log10( m_PhaseMin );
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

							EvalDelegate	Eval = ( float _x ) => {
								int		AngleIndex = (int) ((m_Phase.Length-1) * _x / Math.PI);

								double	Num = Math.Log10( m_Phase[AngleIndex] ) - Math.Log10( m_PhaseMin );
								double	Den = Math.Log10( m_PhaseMax ) - Math.Log10( m_PhaseMin );
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

// 				FresnelEval	Eval = null;
// 				if ( m_FromData )
// 				{
// 					switch ( m_Type ) 
// 					{
// 						case FRESNEL_TYPE.SCHLICK:	Eval = Fresnel_SchlickData; PrepareData(); break;
// 						case FRESNEL_TYPE.PRECISE:	Eval = Fresnel_PreciseData; PrepareData(); break;
// 					}
// 				}
// 				else
// 				{
// 					switch ( m_Type ) 
// 					{
// 						case FRESNEL_TYPE.SCHLICK:	Eval = Fresnel_Schlick; PrepareSchlick(); break;
// 						case FRESNEL_TYPE.PRECISE:	Eval = Fresnel_Precise; PreparePrecise(); break;
// 					}
// 				}
// 
// 				DrawLine( G, 0, 1, 1, 1, Pens.Gray );
// 
// 				float	x = 0.0f;
// 				float	yr, yg, yb;
// 				Eval( 1.0f, out yr, out yg, out yb );
// 				for ( int X=10; X <= Width; X++ )
// 				{
// 					float	px = x;
// 					float	pyr = yr;
// 					float	pyg = yg;
// 					float	pyb = yb;
// 					x = (float) (X-10.0f) / (Width - 10);
// 
// 					float	CosTheta = (float) Math.Cos( x * 0.5 * Math.PI );	// Cos(theta)
// 
// 					Eval( CosTheta, out yr, out yg, out yb );
// 
// 					DrawLine( G, px, pyr, x, yr, Pens.Red );
// 					DrawLine( G, px, pyg, x, yg, Pens.LimeGreen );
// 					DrawLine( G, px, pyb, x, yb, Pens.Blue );
// 				}
// 
// 				if ( !m_FromData )
// 				{
// 					Eval( 1.0f, out yr, out yg, out yb );
// 					float	F0 = Math.Max( Math.Max( yr, yg ), yb );
// 					G.DrawString( "F0 = " + F0, Font, Brushes.Black, 12.0f, Height - 30 - (Height-20) * F0 );
// 				}
// 				else
// 				{
// 					Eval( 1.0f, out yr, out yg, out yb );
// 					float	F0 = Math.Max( Math.Max( yr, yg ), yb );
// 
// 					float	Offset = Height - 30 - 24 - (Height-20) * F0;
// 					if ( Offset < 40 )
// 						Offset = Height - (Height-20) * Math.Min( Math.Min( yr, yg ), yb );
// 
// 					G.DrawString( "R (n = " + m_IndicesR.n + " k = " + m_IndicesR.k + ") F0 = " + yr, Font, Brushes.Black, 12.0f, Offset );
// 					G.DrawString( "G (n = " + m_IndicesG.n + " k = " + m_IndicesG.k + ") F0 = " + yg, Font, Brushes.Black, 12.0f, Offset + 12 );
// 					G.DrawString( "B (n = " + m_IndicesB.n + " k = " + m_IndicesB.k + ") F0 = " + yb, Font, Brushes.Black, 12.0f, Offset + 24 );
// 				}
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
