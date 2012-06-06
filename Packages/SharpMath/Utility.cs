using System;

namespace WMath
{
	/// <summary>
	/// A bunch of utility functions stolen from http://www.johndcook.com
	/// </summary>
	public class UtilityFunctions
	{
		/// <summary>
		/// Error function (http://en.wikipedia.org/wiki/Error_function)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double Erf(double x)
		{
			// constants
			double a1 = 0.254829592;
			double a2 = -0.284496736;
			double a3 = 1.421413741;
			double a4 = -1.453152027;
			double a5 = 1.061405429;
			double p = 0.3275911;

			// Save the sign of x
			int sign = 1;
			if (x < 0)
				sign = -1;
			x = Math.Abs(x);

			// A&S formula 7.1.26
			double t = 1.0 / (1.0 + p*x);
			double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);

			return sign*y;
		}

		/// <summary>
		/// Related to the error function
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double Phi(double x)
		{
			// constants
			double a1 = 0.254829592;
			double a2 = -0.284496736;
			double a3 = 1.421413741;
			double a4 = -1.453152027;
			double a5 = 1.061405429;
			double p = 0.3275911;
        
			// Save the sign of x
			int sign = 1;
			if (x < 0)
				sign = -1;
			x = Math.Abs(x) / Math.Sqrt(2.0);
        
			// A&S formula 7.1.26
			double t = 1.0 / (1.0 + p*x);
			double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t * Math.Exp(-x*x);
        
			return 0.5 * (1.0 + sign*y);
		}

		/// <summary>
		/// Phi-inverse
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static double NormalCDFInverse(double p)
		{
			if (p <= 0.0 || p >= 1.0)
			{
				string msg = String.Format("Invalid input argument: {0}.", p);
				throw new ArgumentOutOfRangeException(msg);
			}

			// See article above for explanation of this section.
			if (p < 0.5)
			{
				// F^-1(p) = - G^-1(p)
				return -RationalApproximation( Math.Sqrt(-2.0*Math.Log(p)) );
			}
			else
			{
				// F^-1(p) = G^-1(1-p)
				return RationalApproximation( Math.Sqrt(-2.0*Math.Log(1.0 - p)) );
			}
		}

		/// <summary>
		/// Compute log(1+x) without losing precision for small values of x
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double LogOnePlusX(double x)
		{
			if (x <= -1.0)
			{
				string msg = String.Format("Invalid input argument: {0}", x);
				throw new ArgumentOutOfRangeException(msg);
			}

			if (Math.Abs(x) > 1e-4)
			{
				// x is large enough that the obvious evaluation is OK
				return Math.Log(1.0 + x);
			}

			// Use Taylor approx. 
			// log(1 + x) = x - x^2/2 with error roughly x^3/3
			// Since |x| < 10^-4, |x|^3 < 10^-12, 
			// relative error less than 10^-8

			return (-0.5*x + 1.0)*x;
		}

		/// <summary>
		/// Computes exp(x)-1 without losing precision for small values of x
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		double expm1(double x)
		{
			if (Math.Abs(x) < 1e-5)
				return x + 0.5*x*x;
			else
				return Math.Exp(x) - 1.0;
		}

		public static double RationalApproximation(double t)
		{
			// Abramowitz and Stegun formula 26.2.23.
			// The absolute value of the error should be less than 4.5 e-4.
			double[] c = {2.515517, 0.802853, 0.010328};
			double[] d = {1.432788, 0.189269, 0.001308};
			return t - ((c[2]*t + c[1])*t + c[0]) / 
						(((d[2]*t + d[1])*t + d[0])*t + 1.0);
		}

		/// <summary>
		/// The gamma function
		/// </summary>
		/// <param name="x">We require x > 0</param>
		/// <returns></returns>
		/// <remarks>Note that the functions Gamma and LogGamma are mutually dependent.</remarks>
		public static double Gamma( double x )
		{
			if (x <= 0.0)
			{
				string msg = string.Format("Invalid input argument {0}. Argument must be positive.", x);
				throw new ArgumentOutOfRangeException(msg);
			}

			// Split the function domain into three intervals:
			// (0, 0.001), [0.001, 12), and (12, infinity)

			///////////////////////////////////////////////////////////////////////////
			// First interval: (0, 0.001)
			//
			// For small x, 1/Gamma(x) has power series x + gamma x^2  - ...
			// So in this range, 1/Gamma(x) = x + gamma x^2 with error on the order of x^3.
			// The relative error over this interval is less than 6e-7.

			const double gamma = 0.577215664901532860606512090; // Euler's gamma constant

			if (x < 0.001)
				return 1.0/(x*(1.0 + gamma*x));

			///////////////////////////////////////////////////////////////////////////
			// Second interval: [0.001, 12)
    
			if (x < 12.0)
			{
				// The algorithm directly approximates gamma over (1,2) and uses
				// reduction identities to reduce other arguments to this interval.
		
				double y = x;
				int n = 0;
				bool arg_was_less_than_one = (y < 1.0);

				// Add or subtract integers as necessary to bring y into (1,2)
				// Will correct for this below
				if (arg_was_less_than_one)
				{
					y += 1.0;
				}
				else
				{
					n = (int) (Math.Floor(y)) - 1;  // will use n later
					y -= n;
				}

				// numerator coefficients for approximation over the interval (1,2)
				double[] p =
				{
					-1.71618513886549492533811E+0,
						2.47656508055759199108314E+1,
					-3.79804256470945635097577E+2,
						6.29331155312818442661052E+2,
						8.66966202790413211295064E+2,
					-3.14512729688483675254357E+4,
					-3.61444134186911729807069E+4,
						6.64561438202405440627855E+4
				};

				// denominator coefficients for approximation over the interval (1,2)
				double[] q =
				{
					-3.08402300119738975254353E+1,
						3.15350626979604161529144E+2,
					-1.01515636749021914166146E+3,
					-3.10777167157231109440444E+3,
						2.25381184209801510330112E+4,
						4.75584627752788110767815E+3,
					-1.34659959864969306392456E+5,
					-1.15132259675553483497211E+5
				};

				double num = 0.0;
				double den = 1.0;
				int i;

				double z = y - 1;
				for (i = 0; i < 8; i++)
				{
					num = (num + p[i])*z;
					den = den*z + q[i];
				}
				double result = num/den + 1.0;

				// Apply correction if argument was not initially in (1,2)
				if (arg_was_less_than_one)
				{
					// Use identity gamma(z) = gamma(z+1)/z
					// The variable "result" now holds gamma of the original y + 1
					// Thus we use y-1 to get back the orginal y.
					result /= (y-1.0);
				}
				else
				{
					// Use the identity gamma(z+n) = z*(z+1)* ... *(z+n-1)*gamma(z)
					for (i = 0; i < n; i++)
						result *= y++;
				}

				return result;
			}

			///////////////////////////////////////////////////////////////////////////
			// Third interval: [12, infinity)

			if (x > 171.624)
			{
				// Correct answer too large to display. 
				return double.PositiveInfinity;
			}

			return Math.Exp(LogGamma(x));
		}

		/// <summary>
		/// The log-gamma function
		/// </summary>
		/// <param name="x">We require x > 0</param>
		/// <returns></returns>
		/// <remarks>Note that the functions Gamma and LogGamma are mutually dependent.</remarks>
		public static double LogGamma( double x )
		{
			if (x <= 0.0)
			{
				string msg = string.Format("Invalid input argument {0}. Argument must be positive.", x);
				throw new ArgumentOutOfRangeException(msg);
			}

			if (x < 12.0)
			{
				return Math.Log(Math.Abs(Gamma(x)));
			}

			// Abramowitz and Stegun 6.1.41
			// Asymptotic series should be good to at least 11 or 12 figures
			// For error analysis, see Whittiker and Watson
			// A Course in Modern Analysis (1927), page 252

			double[] c =
			{
				1.0/12.0,
				-1.0/360.0,
				1.0/1260.0,
				-1.0/1680.0,
				1.0/1188.0,
				-691.0/360360.0,
				1.0/156.0,
				-3617.0/122400.0
			};
			double z = 1.0/(x*x);
			double sum = c[7];
			for (int i=6; i >= 0; i--)
			{
				sum *= z;
				sum += c[i];
			}
			double series = sum/x;

			double halfLogTwoPi = 0.91893853320467274178032973640562;
			double logGamma = (x - 0.5)*Math.Log(x) - x + halfLogTwoPi + series;    
			return logGamma;
		}

		public static double LogFactorial( int n )
		{
			if (n < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			else if (n > 254)
			{
				double x = n + 1;
				return (x - 0.5)*Math.Log(x) - x + 0.5*Math.Log(2*Math.PI) + 1.0/(12.0*x);
			}
			else
			{
				double[] lf = 
				{
					0.000000000000000,
					0.000000000000000,
					0.693147180559945,
					1.791759469228055,
					3.178053830347946,
					4.787491742782046,
					6.579251212010101,
					8.525161361065415,
					10.604602902745251,
					12.801827480081469,
					15.104412573075516,
					17.502307845873887,
					19.987214495661885,
					22.552163853123421,
					25.191221182738683,
					27.899271383840894,
					30.671860106080675,
					33.505073450136891,
					36.395445208033053,
					39.339884187199495,
					42.335616460753485,
					45.380138898476908,
					48.471181351835227,
					51.606675567764377,
					54.784729398112319,
					58.003605222980518,
					61.261701761002001,
					64.557538627006323,
					67.889743137181526,
					71.257038967168000,
					74.658236348830158,
					78.092223553315307,
					81.557959456115029,
					85.054467017581516,
					88.580827542197682,
					92.136175603687079,
					95.719694542143202,
					99.330612454787428,
					102.968198614513810,
					106.631760260643450,
					110.320639714757390,
					114.034211781461690,
					117.771881399745060,
					121.533081515438640,
					125.317271149356880,
					129.123933639127240,
					132.952575035616290,
					136.802722637326350,
					140.673923648234250,
					144.565743946344900,
					148.477766951773020,
					152.409592584497350,
					156.360836303078800,
					160.331128216630930,
					164.320112263195170,
					168.327445448427650,
					172.352797139162820,
					176.395848406997370,
					180.456291417543780,
					184.533828861449510,
					188.628173423671600,
					192.739047287844900,
					196.866181672889980,
					201.009316399281570,
					205.168199482641200,
					209.342586752536820,
					213.532241494563270,
					217.736934113954250,
					221.956441819130360,
					226.190548323727570,
					230.439043565776930,
					234.701723442818260,
					238.978389561834350,
					243.268849002982730,
					247.572914096186910,
					251.890402209723190,
					256.221135550009480,
					260.564940971863220,
					264.921649798552780,
					269.291097651019810,
					273.673124285693690,
					278.067573440366120,
					282.474292687630400,
					286.893133295426990,
					291.323950094270290,
					295.766601350760600,
					300.220948647014100,
					304.686856765668720,
					309.164193580146900,
					313.652829949878990,
					318.152639620209300,
					322.663499126726210,
					327.185287703775200,
					331.717887196928470,
					336.261181979198450,
					340.815058870798960,
					345.379407062266860,
					349.954118040770250,
					354.539085519440790,
					359.134205369575340,
					363.739375555563470,
					368.354496072404690,
					372.979468885689020,
					377.614197873918670,
					382.258588773060010,
					386.912549123217560,
					391.575988217329610,
					396.248817051791490,
					400.930948278915760,
					405.622296161144900,
					410.322776526937280,
					415.032306728249580,
					419.750805599544780,
					424.478193418257090,
					429.214391866651570,
					433.959323995014870,
					438.712914186121170,
					443.475088120918940,
					448.245772745384610,
					453.024896238496130,
					457.812387981278110,
					462.608178526874890,
					467.412199571608080,
					472.224383926980520,
					477.044665492585580,
					481.872979229887900,
					486.709261136839360,
					491.553448223298010,
					496.405478487217580,
					501.265290891579240,
					506.132825342034830,
					511.008022665236070,
					515.890824587822520,
					520.781173716044240,
					525.679013515995050,
					530.584288294433580,
					535.496943180169520,
					540.416924105997740,
					545.344177791154950,
					550.278651724285620,
					555.220294146894960,
					560.169054037273100,
					565.124881094874350,
					570.087725725134190,
					575.057539024710200,
					580.034272767130800,
					585.017879388839220,
					590.008311975617860,
					595.005524249382010,
					600.009470555327430,
					605.020105849423770,
					610.037385686238740,
					615.061266207084940,
					620.091704128477430,
					625.128656730891070,
					630.172081847810200,
					635.221937855059760,
					640.278183660408100,
					645.340778693435030,
					650.409682895655240,
					655.484856710889060,
					660.566261075873510,
					665.653857411105950,
					670.747607611912710,
					675.847474039736880,
					680.953419513637530,
					686.065407301994010,
					691.183401114410800,
					696.307365093814040,
					701.437263808737160,
					706.573062245787470,
					711.714725802289990,
					716.862220279103440,
					722.015511873601330,
					727.174567172815840,
					732.339353146739310,
					737.509837141777440,
					742.685986874351220,
					747.867770424643370,
					753.055156230484160,
					758.248113081374300,
					763.446610112640200,
					768.650616799717000,
					773.860102952558460,
					779.075038710167410,
					784.295394535245690,
					789.521141208958970,
					794.752249825813460,
					799.988691788643450,
					805.230438803703120,
					810.477462875863580,
					815.729736303910160,
					820.987231675937890,
					826.249921864842800,
					831.517780023906310,
					836.790779582469900,
					842.068894241700490,
					847.352097970438420,
					852.640365001133090,
					857.933669825857460,
					863.231987192405430,
					868.535292100464630,
					873.843559797865740,
					879.156765776907600,
					884.474885770751830,
					889.797895749890240,
					895.125771918679900,
					900.458490711945270,
					905.796028791646340,
					911.138363043611210,
					916.485470574328820,
					921.837328707804890,
					927.193914982476710,
					932.555207148186240,
					937.921183163208070,
					943.291821191335660,
					948.667099599019820,
					954.046996952560450,
					959.431492015349480,
					964.820563745165940,
					970.214191291518320,
					975.612353993036210,
					981.015031374908400,
					986.422203146368590,
					991.833849198223450,
					997.249949600427840,
					1002.670484599700300,
					1008.095434617181700,
					1013.524780246136200,
					1018.958502249690200,
					1024.396581558613400,
					1029.838999269135500,
					1035.285736640801600,
					1040.736775094367400,
					1046.192096209724900,
					1051.651681723869200,
					1057.115513528895000,
					1062.583573670030100,
					1068.055844343701400,
					1073.532307895632800,
					1079.012946818975000,
					1084.497743752465600,
					1089.986681478622400,
					1095.479742921962700,
					1100.976911147256000,
					1106.478169357800900,
					1111.983500893733000,
					1117.492889230361000,
					1123.006317976526100,
					1128.523770872990800,
					1134.045231790853000,
					1139.570684729984800,
					1145.100113817496100,
					1150.633503306223700,
					1156.170837573242400,
				};
				return lf[n];
			}
		}
	}
}
