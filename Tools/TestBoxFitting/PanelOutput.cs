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

		const float	MAX_RANGE = 20.0f;	// 20 meters left and right from the center

		public unsafe void		UpdateBitmap() {
			if ( m_Owner == null ) {
				Invalidate();
				return;
			}

			int	W = m_Bitmap.Width;
			int	H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) ) {

				G.FillRectangle( Brushes.White, 0, 0, W, H );

				// Draw obstacles
				float	world2Client = Math.Min( Width, Height ) / (2.0f * MAX_RANGE);
				foreach ( Form1.Obstacle O in m_Owner.m_ObstaclesRound ) {
					float	angle = (float) Math.Atan2( -O.m_Orientation.y, O.m_Orientation.x );
					PointF	P = World2Client( O.m_Position );
					PointF	S = World2Client( O.m_Scale );
							S.X -= 0.5f * W;
							S.Y -= 0.5f * H;

// 					S.X = 2;
// 					S.Y = 2;

					G.ResetTransform();
					G.TranslateTransform( P.X, P.Y );
//					G.ScaleTransform( world2Client * O.m_Scale.x, world2Client * O.m_Scale.y );
					G.RotateTransform( (float) (angle * 180.0f / Math.PI) );
					G.DrawEllipse( Pens.Red, -S.X, -S.Y, 2.0f * S.X, 2.0f * S.Y );
				}
				G.ResetTransform();

				foreach ( Form1.Obstacle O in m_Owner.m_ObstaclesSquare ) {
					float2	Y = new float2( -O.m_Orientation.y, O.m_Orientation.x );
					PointF	P0 = World2Client( O.m_Position + O.m_Scale.x * O.m_Orientation + O.m_Scale.y * Y );
					PointF	P1 = World2Client( O.m_Position - O.m_Scale.x * O.m_Orientation + O.m_Scale.y * Y );
					PointF	P2 = World2Client( O.m_Position - O.m_Scale.x * O.m_Orientation - O.m_Scale.y * Y );
					PointF	P3 = World2Client( O.m_Position + O.m_Scale.x * O.m_Orientation - O.m_Scale.y * Y );

					G.DrawLine( Pens.Blue, P0, P1 );
					G.DrawLine( Pens.Blue, P1, P2 );
					G.DrawLine( Pens.Blue, P2, P3 );
					G.DrawLine( Pens.Blue, P3, P0 );
				}

				// Draw "depth buffer"
				PointF	Z0 = World2Client( SamplePosition( 0.0f ) );
				PointF	Z1;
				for ( int i=1; i <= 1000; i++ ) {
					float	angle = (float) (2.0 * Math.PI * i / 1000);
					Z1 = World2Client( SamplePosition( angle ) );

					G.DrawLine( Pens.Black, Z0, Z1 );

					Z0 = Z1;
				}

// 				PointF	Center = World2Client( float2.Zero );
// 				G.FillEllipse( Brushes.Red, Center.X - 2, Center.Y-2, 5, 5 );

				PointF	Center = World2Client( m_Owner.m_boxCenter );
				G.FillEllipse( Brushes.Red, Center.X - 2, Center.Y-2, 5, 5 );

				// Draw main directions
				for ( int lobeIndex=0; lobeIndex < m_Owner.m_Lobes.Length; lobeIndex++ ) {
//					float2	mainNormal = (float2) m_Owner.m_Lobes[lobeIndex].Direction;
					float2	mainNormal = m_Owner.m_Planes[lobeIndex].m_Normal;
					float2	mainDirection = new float2( -mainNormal.y, mainNormal.x );

					float2	mainPosition = m_Owner.m_Planes[lobeIndex].m_Position;

					PointF	D0 = World2Client( mainPosition - 20.0f * mainDirection );
					PointF	D1 = World2Client( mainPosition + 20.0f * mainDirection );
					G.DrawLine( Pens.DarkGreen, D0, D1 );
				}
			}

			Invalidate();
		}

		float2	SamplePosition( float _angle ) {
			float	pixelAngle = _angle * Form1.PIXELS_COUNT / (float) (2.0 * Math.PI);

			int		i0 = Math.Max( 0, Math.Min( Form1.PIXELS_COUNT-1, (int) Math.Floor( pixelAngle ) ) );
			int		i1 = Math.Min( Form1.PIXELS_COUNT-1, i0+1 );
			float	t = pixelAngle - i0;

			Form1.Pixel	P0 = m_Owner.m_Pixels[i0];
			Form1.Pixel	P1 = m_Owner.m_Pixels[i1];
			float		dist = P0.Distance * (1.0f - t) + P1.Distance * t;

			dist = Math.Min( 1000.0f, dist );

			float2		result = dist * new float2( (float) Math.Cos( _angle ), (float) Math.Sin( _angle ) );
			return result;
		}

		PointF	World2Client( float2 _wsPosition ) {
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
