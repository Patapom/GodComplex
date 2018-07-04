#define FIT_TABLES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using SharpMath;

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

			#if FIT_TABLES
				// Fit
// 				RunForm( new BRDF_GGX(), new FileInfo( "GGX.ltc" ) );					// Fit GGX
// 				RunForm( new BRDF_CookTorrance(), new FileInfo( "CookTorrance.ltc" ) );	// Fit Cook-Torrance
				RunForm( new BRDF_Charlie(), new FileInfo( "CharlieSheen.ltc" ) );		// Fit Charlie Sheen
			#else
				// Export
				Export( new FileInfo( "GGX.ltc" ), "GGX" );
				Export( new FileInfo( "CookTorrance.ltc" ), "CookTorrance" );
				Export( new FileInfo( "CharlieSheen.ltc" ), "Charlie" );
			#endif
		}

		static void	RunForm( IBRDF _BRDF, FileInfo _tableFileName ) {
			FitterForm	form = new FitterForm();

form.RenderBRDF = true;	// Change this to perform fitting without rendering each result (faster)

// Just enter view mode to visualize fitting
//form.DoFitting = false;
form.Paused = true;
//form.ReadOnly = true;

			form.SetupBRDF( _BRDF, 64, _tableFileName );
			Application.Run( form );
		}

		static void		Export( FileInfo _tableFileName, string _BRDFName ) {
			int		validResultsCount;
			LTC[,]	table = FitterForm.LoadTable( _tableFileName, out validResultsCount );

			string	className = "LTCAreaLight_" + _BRDFName;

			string	sourceCode = "";

			// Export LTC matrices
			int	tableSize = table.GetLength(0);
			sourceCode += "namespace LTCAreaLight\r\n"
						+ "{\r\n"
						+ "    public partial class " + className + "\r\n"
						+ "    {\r\n"
						+ "        // Table contains 3x3 matrix coefficients of M^-1 for the fitting of the " + _BRDFName + " BRDF using the LTC technique\r\n"
						+ "        // From \"Real-Time Polygonal-Light Shading with Linearly Transformed Cosines\" 2016 (https://eheitzresearch.wordpress.com/415-2/)\r\n"
						+ "        //\r\n"
						+ "        // The table is accessed via " + className + ".ms_invM[<roughnessIndex>, <thetaIndex>]\r\n"
						+ "        //    • roughness = ( <roughnessIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //    • cosTheta = 1 - ( <thetaIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //\r\n"
						+ "        public static double[,]	ms_invM = new double[" + tableSize + " * " + tableSize +", 3 * 3] {\r\n";

			LTC	defaultLTC = new LTC();
				defaultLTC.amplitude = 0.0;
			for ( int roughnessIndex=0; roughnessIndex < tableSize; roughnessIndex++ ) {
//				string	matrixRowString = "            { ";
				string	matrixRowString = "            ";
				for ( int thetaIndex=0; thetaIndex < tableSize; thetaIndex++ ) {
					LTC		ltc = table[roughnessIndex,thetaIndex];
					if ( ltc == null ) {
						ltc = defaultLTC;
					}

					string	matrixString  = ltc.invM[0,0] + ", " + ltc.invM[0,1] + ", " + ltc.invM[0,2] + ", ";
							matrixString += ltc.invM[1,0] + ", " + ltc.invM[1,1] + ", " + ltc.invM[1,2] + ", ";
							matrixString += ltc.invM[2,0] + ", " + ltc.invM[2,1] + ", " + ltc.invM[2,2];
//					matrixRowString += (thetaIndex == 0 ? "{ " : ", { ") + matrixString + " }";
					matrixRowString += "{ " + matrixString + " }, ";
				}

				// Compute roughness
				float	perceptualRoughness = (float) roughnessIndex / (tableSize-1);
				float	alpha = perceptualRoughness * perceptualRoughness;

//				matrixRowString += " },\r\n";
				matrixRowString += "   // Roughness = " + alpha + "\r\n";
				sourceCode += matrixRowString;
			}

			sourceCode += "        };\r\n";

			// Export LTC amplitude and fresnel
			//
//        public static float[] s_LtcGGXMagnitudeData = new float[k_LtcLUTResolution * k_LtcLUTResolution]
//        public static float[] s_LtcGGXFresnelData = new float[k_LtcLUTResolution * k_LtcLUTResolution]

			sourceCode += "\r\n";
			sourceCode += "        // NOTE: Formerly, we needed to also export and create a table for the BRDF's amplitude factor + fresnel coefficient\r\n";
			sourceCode += "        //    but it turns out these 2 factors are actually already precomputed and available in the FGD table corresponding\r\n";
			sourceCode += "        //    to the " + _BRDFName + " BRDF, therefore they are no longer exported...\r\n";

			// Close class and namespace
			sourceCode += "    }\r\n";
			sourceCode += "}\r\n";

			// Write content
			FileInfo	targetFileName = new FileInfo( Path.Combine( Path.GetDirectoryName( _tableFileName.FullName ), Path.GetFileNameWithoutExtension( _tableFileName.FullName ) + ".cs" ) );
			using ( StreamWriter W = targetFileName.CreateText() )
				W.Write( sourceCode );
		}
	}
}
