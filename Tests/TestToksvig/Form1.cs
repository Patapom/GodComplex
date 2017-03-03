//////////////////////////////////////////////////////////////////////////
// "Mipmapping Normal Maps" by Michael Toksvig (2005)
//
// Toksvig claims that a "gaussian distribution" of normals will never produce a short average normal.
// Except he doesn't say how to produce that gaussian distribution of normals so I assumed he was
//	talking about the "wrapped normal distribution" (https://en.wikipedia.org/wiki/Wrapped_normal_distribution)
// (anyway, he doesn't talk about the von Mises distribution, which is simpler (https://en.wikipedia.org/wiki/Von_Mises_distribution)
//
// The Wrapped Normal Distribution "simply" is the normal distribution wrapped on the unit circle.
// The PDF is:
// 
//		pdf(theta,mu,sigma) = 1/(sigma * sqrt(2PI)) * Sum[-oo,+oo]( e^(-(theta + mu + 2PI*k)^2 / (2*sigma^2)) )
// 
// Where mu and sigma are the mean and standard deviation of the distribution respectively. Theta is then in [-pi,+pi].
// 
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace TestToksvig
{
	public partial class Form1 : Form
	{
		static float	GRAPH_WIDTH = 5;
		static float	GRAPH_HEIGHT = 1;

		List< float2 >	m_distribution = new List< float2 >();

		public Form1() {
			InitializeComponent();

			for ( int x=0; x <= 100; x++ ) {
				float	sigma = Math.Max( 1e-4f, GRAPH_WIDTH * x / 100 );
				double	avgNormal = 0.0f;
				for ( int i=0; i < 10000; i++ ) {
					double	rnd = SimpleRNG.GetNormal( 0, sigma );

					rnd = rnd % Math.PI;				// Simple wrapping (https://en.wikipedia.org/wiki/Wrapped_distribution)
//					rnd = rnd % (2.0*Math.PI);			// Simple wrapping (https://en.wikipedia.org/wiki/Wrapped_distribution)
					avgNormal += Math.Cos( rnd );

				}
				avgNormal /= 10000;
				m_distribution.Add( new float2( sigma, (float) avgNormal ) );
			}
			panelOutput.UpdateBitmap();
		}

		#region Wrapped Normal Distribution

		// Code stolen from: https://github.com/cran/CircStats/blob/master/R/circular.R
		// 
		// ###############################################################
		// # Modified December 31, 2002
		// # aggiunto parametro sd
		// # controllo sul valore di rho
		// 
		// rwrpnorm <- function(n, mu, rho, sd=1) {
		//         if (missing(rho)) {
		//             rho <- exp(-sd^2/2)
		//         }
		//         if (rho < 0 | rho > 1)
		//             stop("rho must be between 0 and 1")        
		// 	if (rho == 0)
		// 		result <- runif(n, 0, 2 * pi)
		// 	else if (rho == 1)
		// 		result <- rep(mu, n)
		// 	else {
		// 		sd <- sqrt(-2 * log(rho))
		// 		result <- rnorm(n, mu, sd) %% (2 * pi)
		// 	}
		// 	result
		// }
		//
// 		double	WrappedNormalDistribution( double _sigma ) {
// //			double	rho = Math.Exp( -0.5 * _sigma*_sigma );
// 			
// 		}

		#endregion

		private void panelOutput_BitmapUpdating( int W, int H, Graphics G ) {
			for ( int i=0; i < m_distribution.Count-1; i++ )
				DrawLine( G, Pens.Black, m_distribution[i], m_distribution[i+1] );
		}

		float2	Graph2Client( float2 P ) {
			return new float2( P.x * panelOutput.Width / GRAPH_WIDTH, (1.0f - P.y) * panelOutput.Height / GRAPH_HEIGHT );
		}
		void	DrawLine( Graphics _G, Pen _pen, float2 _P0, float2 _P1 ) {
			float2	P0 = Graph2Client( _P0 );
			float2	P1 = Graph2Client( _P1 );
			_G.DrawLine( _pen, P0.x, P0.y, P1.x, P1.y );
		}
	}
}
