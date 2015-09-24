using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using RendererManaged;

namespace TestConvexHull
{
	public partial class PanelOutput : Panel
	{
		public TestForm		m_Owner = null;

		private Bitmap		m_Bitmap = null;

		float2				m_center = float2.Zero;

		MouseButtons		m_buttonsDown = MouseButtons.None;
		float2				m_buttonDownPos;
		float2				m_buttonDownCenter;


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
			this.SuspendLayout();
			// 
			// PanelOutput
			// 
			this.BackColor = System.Drawing.Color.LightCoral;
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ResumeLayout(false);

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

				// Draw planes
				using ( Pen planesPen = new Pen( Color.FromArgb( 200, 200, 200 ) ) ) {
					foreach ( Plane P in m_Owner.m_planes ) {
						float3	Dir = new float3( -P.normal.y, P.normal.x, 0.0f );
						PointF	P0 = World2Client( m_center + (float2) (P.position - 2.0f * Dir) );
						PointF	P1 = World2Client( m_center + (float2) (P.position + 2.0f * Dir) );
						G.DrawLine( planesPen, P0, P1 );

						P0 = World2Client( m_center + (float2) P.position );
						P1 = World2Client( m_center + (float2) (P.position + 0.5f * P.normal) );
						G.DrawLine( planesPen, P0, P1 );
					}
				}

				// Draw convex hull planes
				foreach ( Plane P in m_Owner.m_convexHull ) {
					float3	Dir = new float3( -P.normal.y, P.normal.x, 0.0f );
					PointF	P0 = World2Client( m_center + (float2) (P.position - 8.0f * Dir) );
					PointF	P1 = World2Client( m_center + (float2) (P.position + 8.0f * Dir) );
					G.DrawLine( Pens.DarkGreen, P0, P1 );
				}
			}

			Invalidate();
		}
 
// 		float2	SamplePosition( float _angle ) {
// 			float	pixelAngle = _angle * Form1.PIXELS_COUNT / (float) (2.0 * Math.PI);
// 
// 			int		i0 = Math.Max( 0, Math.Min( Form1.PIXELS_COUNT-1, (int) Math.Floor( pixelAngle ) ) );
// 			int		i1 = Math.Min( Form1.PIXELS_COUNT-1, i0+1 );
// 			float	t = pixelAngle - i0;
// 
// 			Form1.Pixel	P0 = m_Owner.m_Pixels[i0];
// 			Form1.Pixel	P1 = m_Owner.m_Pixels[i1];
// 			float		dist = P0.Distance * (1.0f - t) + P1.Distance * t;
// 
// 			dist = Math.Min( 1000.0f, dist );
// 
// 			float2		result = dist * new float2( (float) Math.Cos( _angle ), (float) Math.Sin( _angle ) );
// 			return result;
// 		}

		PointF	World2Client( float2 _wsPosition ) {
			float	world2Client = Math.Min( Width, Height ) / (2.0f * MAX_RANGE);

			float	x = 0.5f * Width + _wsPosition.x * world2Client;
			float	y = 0.5f * Height - _wsPosition.y * world2Client;
			return new PointF( x, y );
		}

		float2	Client2World( PointF _clPosition ) {
			float	world2Client = Math.Min( Width, Height ) / (2.0f * MAX_RANGE);

			float	x = (_clPosition.X - 0.5f * Width) / world2Client;
			float	y = -(_clPosition.Y - 0.5f * Height) / world2Client;
			return new float2( x, y );
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

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );
			m_buttonsDown |= e.Button;
			m_buttonDownPos = Client2World( e.Location );
			m_buttonDownCenter = m_center;
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );
			m_buttonsDown &= ~e.Button;
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );
			if ( m_buttonsDown != MouseButtons.Left )
				return;

 			float2	wsCurrent = Client2World( e.Location );
 			m_center = m_buttonDownCenter + (wsCurrent - m_buttonDownPos);
			UpdateBitmap();
		}
	}
}
