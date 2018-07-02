using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LTCTableGenerator
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

//			FitterForm	debug = null;				// Blind fitting
			FitterForm	debug = new FitterForm();	// Visual fitting


// Just enter view mode to visualize fitting
//debug.Paused = true;
//debug.DoFitting = false;
//debug.ReadOnly = true;



// 			{	// Fit GGX
// 				LTCFitter	fitter = new LTCFitter( debug );
// 				BRDF_GGX	BRDF = new BRDF_GGX();
// 
// fitter.CheckBRDFNormalization( BRDF );
// 
// 				fitter.Fit( BRDF, 64, new FileInfo( "GGX.ltc" ) );
// //				fitter.Fit( BRDF, 64, null );
// 			}

// 			{	// Fit Cook-Torrance
// 				LTCFitter	fitter = new LTCFitter( debug );
// 				BRDF_CookTorrance	BRDF = new BRDF_CookTorrance();
// 
// fitter.CheckBRDFNormalization( BRDF );
// 
// 				fitter.Fit( BRDF, 64, new FileInfo( "CookTorrance.ltc" ) );
// //				fitter.Fit( BRDF, 64, null );
// 			}

 			{	// Fit Charlie Sheen
				LTCFitter		fitter = new LTCFitter( debug );
				BRDF_Charlie	BRDF = new BRDF_Charlie();

fitter.CheckBRDFNormalization( BRDF );

				fitter.Fit( BRDF, 64, new FileInfo( "CharlieSheen.ltc" ) );
//				fitter.Fit( BRDF, 64, null );
			}
		}
	}
}
