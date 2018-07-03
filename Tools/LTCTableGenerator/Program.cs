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

// 			RunForm( new BRDF_GGX(), new FileInfo( "GGX.ltc" ) );					// Fit GGX
// 			RunForm( new BRDF_CookTorrance(), new FileInfo( "CookTorrance.ltc" ) );	// Fit Cook-Torrance
			RunForm( new BRDF_Charlie(), new FileInfo( "CharlieSheen.ltc" ) );		// Fit Charlie Sheen
		}

		static void	RunForm( IBRDF _BRDF, FileInfo _tableFileName ) {
			FitterForm	form = new FitterForm();

form.RenderBRDF = true;	// Change this to perform fitting without rendering each result (faster)

// Just enter view mode to visualize fitting
//form.DoFitting = false;
form.Paused = true;
form.ReadOnly = true;

			form.SetupBRDF( _BRDF, 64, _tableFileName );
			Application.Run( form );
		}
	}
}
