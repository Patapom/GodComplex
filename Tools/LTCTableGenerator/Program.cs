 #define FIT_TABLES
#define EXPORT_FOR_UNITY

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
				// Fit specular
// 				RunForm( new BRDF_GGX(), new FileInfo( "GGX.ltc" ), true );						// Fit GGX
 				RunForm( new BRDF_CookTorrance(), new FileInfo( "CookTorrance.ltc" ), true );	// Fit Cook-Torrance
// 				RunForm( new BRDF_Ward(), new FileInfo( "Ward.ltc" ), true );					// Fit Ward

				// Fit diffuse
//				RunForm( new BRDF_Charlie(), new FileInfo( "CharlieSheen.ltc" ), true );		// Fit Charlie Sheen
// 				RunForm( new BRDF_Disney(), new FileInfo( "Disney.ltc" ), true );				// Fit Disney diffuse
//				RunForm( new BRDF_OrenNayar(), new FileInfo( "OrenNayar.ltc" ), true );			// Fit Oren-Nayar diffuse
			#else
				// Export
				#if EXPORT_FOR_UNITY
					Export( new FileInfo( "GGX.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.GGX2.cs" ), "GGX" );
					Export( new FileInfo( "CookTorrance.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.CookTorrance.cs" ), "CookTorrance" );
					Export( new FileInfo( "CharlieSheen.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.CharlieSheen.cs" ), "Charlie" );
				#else
					Export( new FileInfo( "GGX.ltc" ), new FileInfo( "GGX.cs" ), "GGX" );
					Export( new FileInfo( "CookTorrance.ltc" ), new FileInfo( "CookTorrance.cs" ), "CookTorrance" );
					Export( new FileInfo( "CharlieSheen.ltc" ), new FileInfo( "CharlieSheen.cs" ), "Charlie" );
				#endif
			#endif
		}

		static void	RunForm( IBRDF _BRDF, FileInfo _tableFileName, bool _usePreviousRoughnessForFitting ) {
			FitterForm	form = new FitterForm();

form.RenderBRDF = true;	// Change this to perform fitting without rendering each result (faster)

// Just enter view mode to visualize fitting
form.UsePreviousRoughness = _usePreviousRoughnessForFitting;
//form.DoFitting = false;
//form.Paused = true;
//form.ReadOnly = true;

form.UseAdaptiveFit = true;


			form.SetupBRDF( _BRDF, 64, _tableFileName );

			Application.Run( form );

// 			ApplicationContext	ctxt = new ApplicationContext( form );
// 			ctxt.ThreadExit += ctxt_ThreadExit;
// 			Application.Run( ctxt );
//			ctxt.Dispose();
		}

// 		static void ctxt_ThreadExit( object sender, EventArgs e ) {
// 
// 		}

		static void		Export( FileInfo _tableFileName, FileInfo _targetFileName, string _BRDFName ) {
			int		validResultsCount;
			LTC[,]	table = FitterForm.LoadTable( _tableFileName, out validResultsCount );

			string	tableName = "s_LtcMatrixData_" + _BRDFName;

			string	sourceCode = "";

			// Export LTC matrices
			int	tableSize = table.GetLength(0);
			sourceCode += "using UnityEngine;\r\n"
						+ "using System;\r\n"
						+ "\r\n"
						+ "namespace UnityEngine.Experimental.Rendering.HDPipeline\r\n"
						+ "{\r\n"
						+ "    public partial class LTCAreaLight\r\n"
						+ "    {\r\n"
						+ "        // Table contains 3x3 matrix coefficients of M^-1 for the fitting of the " + _BRDFName + " BRDF using the LTC technique\r\n"
						+ "        // From \"Real-Time Polygonal-Light Shading with Linearly Transformed Cosines\" 2016 (https://eheitzresearch.wordpress.com/415-2/)\r\n"
						+ "        //\r\n"
						+ "        // The table is accessed via LTCAreaLight." + tableName + "[<roughnessIndex> + 64 * <thetaIndex>]    // Theta values are on Y, Roughness values are on X axis\r\n"
						+ "        //    • roughness = ( <roughnessIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //    • cosTheta = 1 - ( <thetaIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //\r\n"
						+ "        public static double[,]	" + tableName + " = new double[" + tableSize + " * " + tableSize +", 3 * 3] {\r\n";

			LTC	defaultLTC = new LTC();
				defaultLTC.amplitude = 0.0;

		#if EXPORT_FOR_UNITY
			for ( int thetaIndex=tableSize-1; thetaIndex >= 0; thetaIndex-- ) {		// Export for Unity with its OpenGL reversed V textures! :'(
		#else
			for ( int thetaIndex=0; thetaIndex < tableSize; thetaIndex++ ) {		// Export for DirectX regular V textures...
		#endif
//				string	matrixRowString = "            { ";
				string	matrixRowString = "            ";
				for ( int roughnessIndex=0; roughnessIndex < tableSize; roughnessIndex++ ) {
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

// 				// Compute roughness
// 				float	perceptualRoughness = (float) roughnessIndex / (tableSize-1);
// 				float	alpha = perceptualRoughness * perceptualRoughness;
// 
// //				matrixRowString += " },\r\n";
// 				matrixRowString += "   // Roughness = " + alpha + "\r\n";

throw new Exception( "Ta mère!" );
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
//			FileInfo	targetFileName = new FileInfo( Path.Combine( Path.GetDirectoryName( _tableFileName.FullName ), Path.GetFileNameWithoutExtension( _tableFileName.FullName ) + ".cs" ) );
			using ( StreamWriter W = _targetFileName.CreateText() )
				W.Write( sourceCode );
		}
	}
}
