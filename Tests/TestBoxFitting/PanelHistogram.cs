using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using SharpMath;
using Renderer;

namespace TestBoxFitting
{
	public partial class PanelHistogram : Panel
	{
		public Form1		m_Owner = null;

		private Bitmap		m_Bitmap = null;

		public PanelHistogram()
		{
			InitializeComponent();
		}

		public PanelHistogram( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) ) {
				components.Dispose();
				if ( m_Bitmap != null )
					m_Bitmap.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}

		#endregion

		protected override void OnResize( EventArgs eventargs )
		{
			base.OnResize( eventargs );

			if ( m_Bitmap != null )
				m_Bitmap.Dispose();
			m_Bitmap = new Bitmap( Width, Height, PixelFormat.Format32bppArgb );

			UpdateBitmap();
		}

		public unsafe void		UpdateBitmap() {
			if ( m_Owner == null ) {
				Invalidate();
				return;
			}

			// Build histogram
			const int	BUCKETS_COUNT = 100;
			int[]	histo = new int[BUCKETS_COUNT];
			for ( int i=0; i < Form1.PIXELS_COUNT; i++ ) {
				Form1.Pixel	P = m_Owner.m_Pixels[i];
				float2	N = P.Normal;
				if ( N.x < 0.0f )
					N = -N;

				float	angle = (float) Math.Atan2( N.y, N.x );
				float	normAngle = 0.5f + angle / (float) Math.PI;
				int		bucketIndex = Math.Max( 0, Math.Min( BUCKETS_COUNT-1, (int) (BUCKETS_COUNT * normAngle) ) );
				histo[bucketIndex]++;
			}

			int		maxBucket = 0;
			for ( int i=0; i < BUCKETS_COUNT; i++ )
				maxBucket = Math.Max( maxBucket, histo[i] );

			// Render
			int	W = m_Bitmap.Width;
			int	H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) ) {

				G.FillRectangle( Brushes.White, 0, 0, W, H );

				for ( int X=0; X < W; X++ ) {
					int		bucketIndex = X * BUCKETS_COUNT / W;
					float	count = (float) histo[bucketIndex] / maxBucket;
					G.DrawLine( Pens.Black, X, H, X, H * (1.0f - 0.9f * count) );
				}
			}

			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );	// Don't!
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
			else
				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
		}
	}
}
