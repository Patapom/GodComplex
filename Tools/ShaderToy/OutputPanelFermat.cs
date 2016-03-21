using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ShaderToy
{
	public partial class OutputPanelFermat : Panel
	{
		protected Bitmap	m_Bitmap = null;

		protected const float	PHI = 2.39996322972865332f;	// Golden angle (137.5077640500378546463487°)

		public OutputPanelFermat( IContainer container )
		{
			container.Add( this );

			InitializeComponent();

			OnSizeChanged( EventArgs.Empty );
		}

		/// <summary>
		/// Draws a special case of the Fermat's spiral r=+/- sqrt(theta) itself a special case of archimedean spiral
		/// The location of "florets" is given by a single parameter "n", the floret index:
		///		r = C sqrt( n )
		///		theta = n * 137.5077640500378546463487°		<-- Golden Angle
		/// </summary>
		PointF	m_mousePosition;
		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				int		Xc = Width / 2;
				int		Yc = Height / 2;

				G.DrawLine( Pens.Black, 0, Yc, Width, Yc );
				G.DrawLine( Pens.Black, Xc, 0, Xc, Height );
				
				#if false
					const float		C = 1.0f / (float) (2.0 * Math.PI);	// Growth of the spiral, in pixels per turn...
					const float		MAX_RADIUS = 40.0f;
					const float		THETA_MAX = MAX_RADIUS * 2 * (float) Math.PI;

					for ( int floretIndex=0; floretIndex < 1024; floretIndex++ ) {

						float	theta = floretIndex * THETA_MAX / 1024;
//						float	r = C * (float) Math.Sqrt( theta );
						float	r = C * theta;
						float	Dx = r * (float) Math.Cos( theta );
						float	Dy = r * (float) Math.Sin( theta );

						float	X = Xc + Dx;
						float	Y = Yc - Dy;
						G.FillEllipse( Brushes.Black, X-2, Y-2, 4, 4 );
//						G.FillEllipse( Brushes.Black, X-1, Y-1, 2, 2 );
					}

					// Draw special floret index based on mouse position
					{
						float	Dx = m_mousePosition.X - Xc;
						float	Dy = Yc - m_mousePosition.Y;

 						float	r = (float) Math.Sqrt( Dx*Dx + Dy*Dy ) / C;
 						float	dTheta = (float) ((Math.Atan2( Dy, Dx ) + 2.0 * Math.PI) % (2.0 * Math.PI));

						float	turnIndex = (float) Math.Floor( r / (2.0 * Math.PI) );	// Amount of integer turns
						float	theta = (float) (2.0 * Math.PI  * turnIndex + dTheta);
						float	index = (float) Math.Floor( 1024 * theta / THETA_MAX );
								theta = index * THETA_MAX / 1024;
						r = C * theta;
//*/
						Dx = r * (float) Math.Cos( theta );
						Dy = r * (float) Math.Sin( theta );

						float	X = Xc + Dx;
						float	Y = Yc - Dy;
						G.FillEllipse( Brushes.Red, X-2, Y-2, 4, 4 );

//						G.DrawString( "turn0 " + turnIndex0.ToString( "G3" ) + " - turn1 " + turnIndex1.ToString( "G3" ), Font, Brushes.Red, 0, 0 );
//						G.DrawString( "dturn " + dTurn.ToString( "G3" ), Font, Brushes.Red, 0, Height-12 );
					}

				#elif false
					const float		C = 2.0f;	// Growth of the spiral, in pixels...

					for ( int floretIndex=0; floretIndex < 1024; floretIndex++ ) {

						float	theta = floretIndex * PHI;
						float	r = C * (float) Math.Sqrt( theta );
						float	Dx = r * (float) Math.Cos( theta );
						float	Dy = r * (float) Math.Sin( theta );

						float	X = Xc + Dx;
						float	Y = Yc - Dy;
						G.FillEllipse( Brushes.Black, X-2, Y-2, 4, 4 );
//						G.FillEllipse( Brushes.Black, X-1, Y-1, 2, 2 );
					}
					// Draw special floret index based on mouse position
					{
						float	Dx = m_mousePosition.X - Xc;
						float	Dy = Yc - m_mousePosition.Y;

 						float	r = (float) Math.Sqrt( Dx*Dx + Dy*Dy ) / C;
 						float	dTheta = (float) ((Math.Atan2( Dy, Dx ) + 2.0 * Math.PI) % (2.0 * Math.PI));

/*
// 						float	dTheta = (float) Math.Atan2( Dy, Dx );

//  						float	r0 = (float) Math.Floor( r );
//  						float	r1 = r0 + 1;
// 						float	turnIndex0 = (float) Math.Floor( r0*r0 / (2.0 * Math.PI) );
// 						float	turnIndex1 = (float) Math.Floor( r1*r1 / (2.0 * Math.PI) );
//  						float	dTurn = (float) Math.Floor( (r - r0) * (turnIndex1 - turnIndex0) );
//  						float	turnIndex = turnIndex0 + dTurn;

						float	turnIndex = (float) Math.Floor( (r*r) / (2.0 * Math.PI) );		// Best yet

						float	theta = (float) (turnIndex * 2.0 * Math.PI) + dTheta;

						int	selectedFloretIndex = (int) Math.Floor( theta / PHI );

*/
						// Find the "fibonacci index" where we're standing
						int[]	Fibo = new int[18] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597 };
						float	turnsCount = r * r / PHI;
						int	i = 0;
						for ( ; i < 17; i++ ) {
							if ( turnsCount < Fibo[i+1] )
								break;	// Found fibo index!
						}

//						int	selectedFloretIndex = Fibo[i];

						float	theta0 = Fibo[i] * PHI;
						float	r0 = (float) Math.Sqrt( theta0 );
						
// 						float	theta = (float) Math.Sqrt( r*r - r0*r0 + theta0*theta0 );
// 								theta += dTheta;
// 
// 						// Now count the amount of turns we made to reach mouse position
// 						int	selectedFloretIndex = (int) Math.Floor( theta / PHI );

						float	dR = dTheta / PHI;
//						r += dR;

 						int	selectedFloretIndex = (int) Math.Floor( (r * r - theta0) / PHI );


						// F.Maestre
						float	angle = (float) Math.Atan2( Dy, Dx );
						r = (float) (Dx / Math.Cos( angle ));
						float	theta = r * r;
						selectedFloretIndex = (int) Math.Round( theta / PHI );


//						r = C * (float) Math.Sqrt( 2.0 * Math.PI * turnIndex0 );
//*

//						float	floret_Min = (float) Math.Floor( thetaTurn * PHI );
// 						float	floret_Max = (R_int+1.0f) * (R_int+1.0f);
// 						float	floretIndex = floret_Min + (floret_Max - floret_Min) * (float) ((theta + 2.0 * Math.PI) % (2.0 * Math.PI));

//						int	selectedFloretIndex = (int) floretIndex;

//*/

						theta = selectedFloretIndex * PHI;
						r = C * (float) Math.Sqrt( theta );
						Dx = r * (float) Math.Cos( theta );
						Dy = r * (float) Math.Sin( theta );

						float	X = Xc + Dx;
						float	Y = Yc - Dy;
						G.FillEllipse( Brushes.Red, X-2, Y-2, 4, 4 );

//						G.DrawString( "turn0 " + turnIndex0.ToString( "G3" ) + " - turn1 " + turnIndex1.ToString( "G3" ), Font, Brushes.Red, 0, 0 );
//						G.DrawString( "dturn " + dTurn.ToString( "G3" ), Font, Brushes.Red, 0, Height-12 );
					}

			#else
					const float		C = 2.0f;	// Growth of the spiral, in pixels...

					for ( int floretIndex=0; floretIndex < 1024; floretIndex++ ) {

						float	theta = floretIndex * PHI;
// 						float	r = C * (float) Math.Sqrt( floretIndex );
						float	r = C * (float) Math.Sqrt( theta );
						float	Dx = r * (float) Math.Cos( theta );
						float	Dy = r * (float) Math.Sin( theta );

						float	X = Xc + Dx;
						float	Y = Yc - Dy;
						G.FillEllipse( Brushes.Black, X-2, Y-2, 4, 4 );
//						G.FillEllipse( Brushes.Black, X-1, Y-1, 2, 2 );
					}

					// Draw special floret index based on mouse position
					{
m_bisou += 0.01f;
const float	radius = 80.0f;
m_mousePosition = new PointF( Xc + radius * (float) (Math.Cos( 0.37 * m_bisou ) * Math.Sin( -0.78 * m_bisou )),
							  Yc + radius * (float) (Math.Cos( 0.56 * m_bisou ) * Math.Sin( 0.94 * m_bisou )) );


						float	Dx = m_mousePosition.X - Xc;
						float	Dy = Yc - m_mousePosition.Y;

 						float	r = (float) Math.Sqrt( Dx*Dx + Dy*Dy ) / C;
 						float	dTheta = (float) ((Math.Atan2( Dy, Dx ) + 2.0 * Math.PI) % (2.0 * Math.PI));

#if true
						float	turnIndex = (float) Math.Floor( r * r / (2.0 * Math.PI) );	// Amount of integer turns
						float	theta = (float) (2.0 * Math.PI * turnIndex + dTheta);

theta = r*r + dTheta;

						int		selectedFloretIndex = (int) Math.Floor( theta / PHI );

// 						// F.Maestre
// 						float	angle = (float) Math.Atan2( Dy, Dx );
// 						r = (float) (Dx / Math.Cos( angle ));
// 						float	theta = r * r;
// 						int		selectedFloretIndex = (int) Math.Round( theta / PHI );

						theta = selectedFloretIndex * PHI;
						r = C * (float) Math.Sqrt( theta );

#else
						float	R_int = (float) Math.Floor( Math.Sqrt( Dx*Dx + Dy*Dy ) / C );
//						float	floret_Min = R_int * R_int;
						float	floret_Min = (float) Math.Floor( ( Dx*Dx + Dy*Dy ) / (C * C) );
						float	floret_Max = (R_int+1.0f) * (R_int+1.0f);
						float	floretIndex = floret_Min + (floret_Max - floret_Min) * (float) ((theta + 2.0 * Math.PI) % (2.0 * Math.PI));


						const float	PHI = 1.6180339887498948482045868f;
						float	phiN = (float) Math.Pow( PHI, R_int );
						float	fibonacci = (phiN + ((float) Math.Pow( -1.0, R_int )) / phiN) / ((float) Math.Sqrt( 5.0 ));
						floret_Min = (float) Math.Round( fibonacci );

						float	r;

//						int	selectedFloretIndex = (int) floretIndex;
						int	selectedFloretIndex = (int) floret_Min;

						r = C * (float) Math.Sqrt( selectedFloretIndex );
						theta = selectedFloretIndex * PHI;	// Golden angle (137.5077640500378546463487°)
#endif

						Dx = r * (float) Math.Cos( theta );
						Dy = r * (float) Math.Sin( theta );

						float	X = Xc + Dx;
						float	Y = Yc - Dy;
						G.FillEllipse( Brushes.Red, X-2, Y-2, 4, 4 );

X = m_mousePosition.X;
Y = m_mousePosition.Y;
G.FillEllipse( Brushes.Green, X-2, Y-2, 4, 4 );
					}
				#endif
			}

			Invalidate();
		}
 float	m_bisou = 0.0f;

		protected override void OnSizeChanged( EventArgs e ) {
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);

			m_mousePosition = e.Location;
			UpdateBitmap();
		}
	}
}
