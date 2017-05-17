using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestPullPush
{
	public partial class PanelOutput : Panel {
		public Bitmap	m_bitmap = null;

		public PanelOutput() {
			InitializeComponent();
		}

		public PanelOutput( IContainer container ) {
			container.Add( this );
			InitializeComponent();
		}

		protected void		UpdateBitmap() {
			if ( m_bitmap == null )
				return;

// 			using ( Graphics G = Graphics.FromImage( m_bitmap ) )
// 			{
// 				G.FillRectangle( Brushes.White, 0, 0, Width, Height );
// 
// 				G.DrawLine( Pens.Black, 10, 0, 10, Height );
// 				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );
// 
// 				FresnelEval	Eval = null;
// 				if ( m_FromData ) {
// 					switch ( m_Type )  {
// 						case FRESNEL_TYPE.SCHLICK:	Eval = Fresnel_SchlickData; PrepareData(); break;
// 						case FRESNEL_TYPE.PRECISE:	Eval = Fresnel_PreciseData; PrepareData(); break;
// 						default: Eval = Fresnel_SchlickData; PrepareData(); break;
// 					}
// 				} else {
// 					if ( m_IORSource == IOR_SOURCE.F0 && m_useEdgeTint ) {
// 						PrepareArtistFriendly();
// 						Eval = Fresnel_ArtistFriendly;
// 					} else {
// 						switch ( m_Type ) {
// 							case FRESNEL_TYPE.SCHLICK:	Eval = Fresnel_Schlick; PrepareSchlick(); break;
// 							case FRESNEL_TYPE.PRECISE:	Eval = Fresnel_Precise; PreparePrecise(); break;
// 						}
// 					}
// 				}
// 
// 				DrawLine( G, 0, 1, 1, 1, Pens.Gray );
// 
// 				float	x = 0.0f;
// 				float	yr, yg, yb;
// 				Eval( 1.0f, out yr, out yg, out yb );
// 				for ( int X=10; X <= Width; X++ ) {
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
// 				if ( !m_FromData ) {
// 					Eval( 1.0f, out yr, out yg, out yb );
// 					float	F0 = Math.Max( Math.Max( yr, yg ), yb );
// 					G.DrawString( "F0 = " + F0, Font, Brushes.Black, 12.0f, Height - 30 - (Height-20) * F0 );
// 
// 				} else {
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
// 			}

			Invalidate();
		}

// 		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1 )
// 		{
// 			DrawLine( G, x0, y0, x1, y1, Pens.Black );
// 		}
// 		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1, Pen _Pen )
// 		{
// 			float	X0 = 10 + (Width-20) * x0;
// 			float	Y0 = Height - 10 - (Height-20) * y0;
// 			float	X1 = 10 + (Width-20) * x1;
// 			float	Y1 = Height - 10 - (Height-20) * y1;
// 			G.DrawLine( _Pen, X0, Y0, X1, Y1 );
// 		}

		protected override void OnSizeChanged( EventArgs e ) {
// 			if ( m_bitmap != null )
// 				m_bitmap.Dispose();
// 
// 			m_bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
// 			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
			e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );
			if ( m_bitmap != null )
//				e.Graphics.DrawImage( m_bitmap, 0, 0 );
				e.Graphics.DrawImage( m_bitmap, 0, 0, Width, Height );
		}
	}
}
