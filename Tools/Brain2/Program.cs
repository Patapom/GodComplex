using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brain2 {
	static class Program {

		static int	Sum( double alpha, int n, int[] _counters ) {
			Array.Clear( _counters, 0, _counters.Length );

			int	result = 0;
			for ( int k=1; k <= n; k++ ) {
				int	value = (int) Math.Floor( alpha * k );
				result += value;
				_counters[value]++;
			}

			return result;
		}

		static int	SumAnalytical( double alpha, int n, int exactResult ) {
			// Compute integer sum
			int	alphaInt = (int) Math.Floor( alpha );
			int	result = alphaInt * (n * (n+1)) / 2;

			// Compute decimal sum
			alpha -= alphaInt;
			if ( alpha > 0.0 ) {
				int		stepSize = (int) Math.Floor( 1.0 / alpha );
				int		integralSteps = (n+1) / stepSize;
				int		remainder = n - integralSteps * stepSize;

				// Split the sum into integral part
				int		resultIntegralSum = integralSteps * (integralSteps-1) / 2;
						resultIntegralSum *= stepSize;

				// And remainder part
				resultIntegralSum += remainder * integralSteps;

				result += resultIntegralSum;
			}

			return result;
		}

		static void	Test() {

			Random	RNG = new Random( 1 );

			int[]	counters = new int[1000];
			for ( int i=0; i < 10000; i++ ) {
				double	alpha = RNG.NextDouble();
				int		N = RNG.Next( 1000 );

//alpha = 0.46701067987224587;
alpha = (Math.Sqrt( 5 ) - 1) / 2;
N = 1000;

// alpha = 1.0 / 3.0;
// N = 4;

				int		result0 = Sum( alpha, N, counters );
				int		result1 = SumAnalytical( alpha, N, result0 );
				if ( result0 != result1 )
					throw new Exception();
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );

//			Test();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new BrainForm());
		}
	}
}
