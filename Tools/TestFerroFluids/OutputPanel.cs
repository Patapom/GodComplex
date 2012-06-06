using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using WMath;

namespace TestSPH
{
	public partial class OutputPanel : Panel
	{
		public Bitmap		m_Bitmap = null;
		public Form1		m_Owner = null;
		public float		m_Time = 0.0f;
		public Point2D		m_Center = new Point2D();

		public OutputPanel()
		{
			InitializeComponent();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );

			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, PixelFormat.Format32bppArgb );
			UpdateBitmap( null );
		}

		public void		UpdateBitmap( Form1.Particle[] _Particles )
		{
			float	SizeNormalizer = 1.0f / Form1.SIMULATION_SPACE_SIZE;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				{	// Draw center
					float	X = Width * 0.5f * (1.0f + m_Center.x * SizeNormalizer);
					float	Y = Height * 0.5f * (1.0f - m_Center.y * SizeNormalizer);
					G.FillEllipse( Brushes.Red, X-2.5f, Y-2.5f, 5, 5 );
				}

				if ( _Particles != null )
				{
					for ( int i=0; i < _Particles.Length; i++ )
					{
						Point2D	P = _Particles[i].P;
						float	Size = Width * _Particles[i].Size * SizeNormalizer;
						float	X = Width * 0.5f * (1.0f + P.x * SizeNormalizer);
						float	Y = Height * 0.5f * (1.0f - P.y * SizeNormalizer);
						G.DrawEllipse( Pens.Black, X - Size, Y - Size, 2.0f * Size, 2.0f * Size );
					}
				}
				G.DrawString( "Simulation Time = " + m_Time, Font, Brushes.Black, 0, 10 );
			}
			Invalidate();
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
		}
	}
}
