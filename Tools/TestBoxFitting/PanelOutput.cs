using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using RendererManaged;

namespace TestBoxFitting
{
	public partial class PanelOutput : Panel
	{
		public Form1		m_Owner = null;

		private Bitmap		m_Bitmap = null;

		public PanelOutput()
		{
			InitializeComponent();
		}

		public PanelOutput( IContainer container )
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

			int	W = m_Bitmap.Width;
			int	H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) ) {

				G.FillRectangle( Brushes.White, 0, 0, W, H );

				PointF	P0 = World2Client( SamplePosition( 0.0f ) );
				PointF	P1;
				for ( int i=1; i <= 1000; i++ ) {
					float	angle = (float) (2.0 * Math.PI * i / 1000);
					P1 = World2Client( SamplePosition( angle ) );

					G.DrawLine( Pens.Black, P0, P1 );

					P0 = P1;
				}
			}

			Invalidate();
		}

		float2	SamplePosition( float _angle ) {
			float	pixelAngle = _angle * Form1.PIXELS_COUNT / (float) (2.0 * Math.PI);

			int		i0 = Math.Max( 0, Math.Min( Form1.PIXELS_COUNT-1, (int) Math.Floor( pixelAngle ) ) );
			int		i1 = Math.Min( Form1.PIXELS_COUNT-1, i0+1 );
			float	t = _angle - i0;

			Form1.Pixel	P0 = m_Owner.m_Pixels[i0];
			Form1.Pixel	P1 = m_Owner.m_Pixels[i1];
			float		dist = P0.Distance * (1.0f - t) + P1.Distance * t;

			float2		result = dist * new float2( (float) Math.Cos( _angle ), (float) Math.Sin( _angle ) );
			return result;
		}

		PointF	World2Client( float2 _wsPosition ) {
			const float	MAX_RANGE = 20.0f;	// 20 meters left and right from the center
			float	world2Client = Math.Min( Width, Height ) / (2.0f * MAX_RANGE);

			float	x = 0.5f * Width + _wsPosition.x * world2Client;
			float	y = 0.5f * Height - _wsPosition.y * world2Client;
			return new PointF( x, y );
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
