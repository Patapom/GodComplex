using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImprovedNormalMapDistribution
{
	public partial class OutputPanel : Panel
	{
		private Bitmap	m_Bitmap = null;

		private double	m_Phi = 0;
		private double	m_Theta = 0;

		private bool	m_Splat = false;
		public bool		Splat
		{
			get { return m_Splat; }
			set
			{
				m_Splat = value;
				UpdateBitmap();
			}
		}

		public void		SetAngles( double _Phi, double _Theta )
		{
			m_Phi = _Phi;
			m_Theta = _Theta;

			if ( !m_Splat )
				UpdateBitmap();
		}

		private int		m_IntersectionsCount = 0;
		private int		m_SumIterationsCount = 0;

		public OutputPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			UpdateBitmap();
		}

		public unsafe void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, W, H );

				// Draw grid
				const int	SUBDIVS = 50;

				for ( int Y=0; Y <= SUBDIVS; Y++ )
				{
					float	y = -1.0f + 2.0f * Y / SUBDIVS;
					for ( int X=0; X < SUBDIVS; X++ )
					{
						float	x0 = -1.0f + 2.0f * X / SUBDIVS;
						float	x1 = -1.0f + 2.0f * (X+1) / SUBDIVS;

						PointF	P0, P1;
						Transform( x0, y, out P0 );
						Transform( x1, y, out P1 );
						G.DrawLine( Pens.Black, P0, P1 );
					}
				}

				for ( int X=0; X <= SUBDIVS; X++ )
				{
					float	x = -1.0f + 2.0f * X / SUBDIVS;
					for ( int Y=0; Y < SUBDIVS; Y++ )
					{
						float	y0 = -1.0f + 2.0f * Y / SUBDIVS;
						float	y1 = -1.0f + 2.0f * (Y+1) / SUBDIVS;

						PointF	P0, P1;
						Transform( x, y0, out P0 );
						Transform( x, y1, out P1 );
						G.DrawLine( Pens.Black, P0, P1 );
					}
				}

				// Compute normal intersection
				if ( m_Splat )
				{
					for ( int Y=-256; Y < 256; Y++ )
					{
						double	Phi = Math.PI * Y / 256.0;
						for ( int X=0; X <= 128; X++ )
						{
							double	Theta = Math.PI * X / 256.0;

							float	Ix, Iy;
							ComputeIntersectionExact( Phi, Theta, out Ix, out Iy );

							PointF	N;
							Transform( Ix, Iy, out N );
							G.FillEllipse( Brushes.Red, N.X - 0.5f, N.Y - 0.5f, 1.0f, 1.0f );
						}
					}
				}
				else
				{
					float	Nx, Ny;
//					ComputeIntersectionNewton( out Nx, out Ny );
					ComputeIntersectionExact( out Nx, out Ny );
					PointF	N;
					Transform( Nx, Ny, out N );
					G.FillEllipse( Brushes.Red, N.X - 4, N.Y - 4, 8, 8 );
				}

				G.DrawString( "Average Iterations = " + ((float) m_SumIterationsCount / m_IntersectionsCount).ToString( "G3"), Font, Brushes.Black, 10, 20 );
			}

			Invalidate();
		}

		private void	Transform( float x, float y, out PointF _P )
		{
			float	h = (1.0f - x*x) * (1.0f - y*y);

			x = 0.5f * (1.0f + x);
			y = 0.5f * (1.0f + y);

			float	CornerMinX = 0.1f * Width;
			float	CornerMaxX = 0.9f * Width;
			float	CornerMinY = 0.7f * Height;
			float	W = CornerMaxX - CornerMinX;
			float	H = 0.2f * Height;

			float	Ratio = 0.4f;

			float	X = CornerMinX + (y * Ratio + x * (1.0f - Ratio)) * W;
			float	Y = CornerMinY + (y - x) * H;

			Y -= 0.3f * Height * h;

			_P = new PointF( X, Y );
		}
		
		/// <summary>
		/// Computes the intersection of the normal vector with the curve Z=(1-x²)(1-y²) using Newton-Raphson
		/// 
		/// We can parametrize the Normal path as:
		///		x = Nx.t
		///		y = Ny.t
		///		z = Nz.t
		///		
		/// Injecting this in f(x,y) = (1-x²)(1-y²) we get:
		/// 
		///		f(t) = (1-Nx².t²)(1-Ny².t²)
		///		
		/// To find the intersection with the normal we subtract Nz.t:
		/// 
		///		f(t) = (1-Nx².t²)(1-Ny².t²) - Nz.t
		///		
		/// Expanding into:
		///		
		///		f(t) = Nx².Ny².t^4 - (Nx²+Ny²).t² - Nz.t + 1
		/// 
		/// And try to find the root f(t) = 0, we need the derivative of f(t):
		///		
		///		f'(t) = 4.Nx².Ny².t^3 - 2.(Nx²+Ny²).t - Nz.t
		///		
		///	We perform several iterations of the Newton-Raphson algorithm (http://en.wikipedia.org/wiki/Newton's_method):
		///		
		///	Starting from t0 = 0;
		///	
		///		t1 = t0 - f(t0) / f'(t0)
		///		t2 = t1 - f(t1) / f'(t1)
		///		etc.
		///		
		///	Until f(t) < epsilon ...
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
// 		void	ComputeIntersectionNewton( out float x, out float y )
// 		{
// 			double	a = m_NormalX*m_NormalX * m_NormalY*m_NormalY;
// 			double	b = 0.0;
// 			double	c = -m_NormalX*m_NormalX - m_NormalY*m_NormalY;
// 			double	d = -m_NormalZ;
// 			double	e = 1.0;
// 
// 			const double	epsilon = 1e-6;
// 			const int		MAX_ITERATIONS = 10;
// 
// 			double	t = 0.0;
// 			int		Iteration = 0;
// 			while ( Iteration++ < MAX_ITERATIONS )
// 			{
// 				double	f = e + t*(d + t*(c + t*(b + t*a)));
// 				if ( Math.Abs( f ) < epsilon )
// 					break;
// 
// 				double	f_prime = d + t*(2*c + t*(3*b + t*4*a));
// 
// 				double	t1 = t - f / Math.Sign( f_prime ) * Math.Max( 1e-6, Math.Abs( f_prime ) );
// 				t = t1;
// 			}
// 
// 			x = (float) (t * m_NormalX);
// 			y = (float) (t * m_NormalY);
// 
// 			m_IntersectionsCount++;
// 			m_SumIterationsCount += Iteration;
// 		}

		void	ComputeIntersectionExact( out float x, out float y )
		{
			ComputeIntersectionExact( m_Phi, m_Theta, out x, out y );
		}

		void	ComputeIntersectionExact( double _Phi, double _Theta, out float x, out float y )
		{
			double	CosPhi = Math.Cos( _Phi );
			double	SinPhi = Math.Sin( _Phi );
			double	CosTheta = Math.Cos( _Theta );
			double	SinTheta = Math.Sin( _Theta );

			double	t = 0.0;
//			if ( m_NormalZ < 1.0 )
			{
				double	e = SinTheta*SinTheta*SinTheta*SinTheta * CosPhi*CosPhi * SinPhi*SinPhi;	// pow( sin(theta), 4 )
				double	d = 0.0;
				double	c = -SinTheta*SinTheta;
				double	b = -CosTheta;
				double	a = 1.0;

				double[]	roots = Polynomial.solvePolynomial( a, b, c, d, e );

				t = Math.Sqrt( 2.0 );
				for ( int i=0; i < roots.Length; i++ )
					if ( !double.IsNaN( roots[i] ) && roots[i] >= 0.0 )
						t = Math.Min( t, roots[i] );
			}

			x = (float) (t * CosPhi * SinTheta);
			y = (float) (t * SinPhi * SinTheta);

			m_IntersectionsCount = 1;
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
//				e.Graphics.DrawImage( m_Bitmap, new Rectangle( 0, 0, Width, Height ), 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel );
		}
	}
}
