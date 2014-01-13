using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestGradientPNG
{
	public partial class OutputPanelHammersley : Panel
	{
		protected Bitmap	m_Bitmap = null;

		protected const int			SAMPLES_COUNT = 16;
		protected WMath.Vector2D[]	m_Samples = new WMath.Vector2D[SAMPLES_COUNT];

		public OutputPanelHammersley( IContainer container )
		{
			container.Add( this );

			InitializeComponent();

			WMath.Hammersley	QRNG = new WMath.Hammersley();
			double[,]		Sequence = QRNG.BuildSequence( SAMPLES_COUNT, 2 );
			for ( int SampleIndex=0; SampleIndex < SAMPLES_COUNT; SampleIndex++ )
			{
				float	Angle = 2.0f * (float) Math.PI * (float) Sequence[SampleIndex,0];
				float	Radius = (float) Math.Sqrt( Sequence[SampleIndex,1] );	// Uniform
//				float	Radius = (float) Sequence[SampleIndex,1];

				m_Samples[SampleIndex] = new WMath.Vector2D( Radius * (float) Math.Cos( Angle ), Radius * (float) Math.Sin( Angle ) );
			}

			OnSizeChanged( EventArgs.Empty );
		}

		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				int		Xz = Width / 2;
				int		Yz = Height / 2;

				G.DrawLine( Pens.Black, 0, Yz, Width, Yz );
				G.DrawLine( Pens.Black, Xz, 0, Xz, Height );
				
				for ( int SampleIndex=0; SampleIndex < SAMPLES_COUNT; SampleIndex++ )
				{
					float	X = Xz + 0.5f * Width * m_Samples[SampleIndex].x;
					float	Y = Yz + 0.5f * Height * m_Samples[SampleIndex].y;
					G.FillEllipse( Brushes.Black, X-2, Y-2, 4, 4 );
				}
			}

			Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			UpdateBitmap();

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
		}
	}
}
