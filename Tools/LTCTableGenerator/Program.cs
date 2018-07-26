//#define FIT_TABLES
//#define EXPORT_FOR_UNITY

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
		static void Main( string[] _args )
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			int	BRDFIndex = 0;
			if ( _args.Length > 0 )
				BRDFIndex = int.Parse( _args[0] );

			#if FIT_TABLES
				switch ( BRDFIndex ) {
					// Fit specular
					case 0:
						RunForm( new BRDF_GGX(), new FileInfo( "GGX.ltc" ), true );						// Fit GGX
						break;
					case 1:
						RunForm( new BRDF_CookTorrance(), new FileInfo( "CookTorrance.ltc" ), true );	// Fit Cook-Torrance
						break;
					case 2:
						RunForm( new BRDF_Ward(), new FileInfo( "Ward.ltc" ), true );					// Fit Ward
						break;

					// Fit diffuse
					case 10:
						RunForm( new BRDF_OrenNayar(), new FileInfo( "OrenNayar.ltc" ), true );			// Fit Oren-Nayar diffuse
						break;
					case 11:
						RunForm( new BRDF_Charlie(), new FileInfo( "CharlieSheen.ltc" ), true );		// Fit Charlie Sheen diffuse
						break;
					case 12:
						RunForm( new BRDF_Disney(), new FileInfo( "Disney.ltc" ), true );				// Fit Disney diffuse
						break;

					default:
						MessageBox.Show( "Unsupported BRDF index: " + BRDFIndex, "Invalid Argument!", MessageBoxButtons.OK, MessageBoxIcon.Error );
						return;
				}
			#else
				// Export
				#if EXPORT_FOR_UNITY
					Export( new FileInfo( "GGX.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.GGX2.cs" ), "GGX" );
					Export( new FileInfo( "CookTorrance.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.CookTorrance.cs" ), "CookTorrance" );
					Export( new FileInfo( "Ward.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.Ward.cs" ), "Ward" );

					Export( new FileInfo( "OrenNayar.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.OrenNayar.cs" ), "OrenNayar" );
					Export( new FileInfo( "CharlieSheen.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.CharlieSheen.cs" ), "Charlie" );
					Export( new FileInfo( "Disney.ltc" ), new FileInfo( @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\LtcData.DisneyDiffuse2.cs" ), "Disney" );
				#else
// 					Export( new FileInfo( "GGX.ltc" ), new FileInfo( "GGX.cs" ), "GGX" );
// 					Export( new FileInfo( "CookTorrance.ltc" ), new FileInfo( "CookTorrance.cs" ), "CookTorrance" );
// 					Export( new FileInfo( "CharlieSheen.ltc" ), new FileInfo( "CharlieSheen.cs" ), "Charlie" );

					ExportTexture( new FileInfo[] {
							new FileInfo( "GGX.ltc" ), new FileInfo( "CookTorrance.ltc" ), new FileInfo( "Ward.ltc" ),
							new FileInfo( "OrenNayar.ltc" ), new FileInfo( "CharlieSheen.ltc" ), new FileInfo( "Disney.ltc" ),
						}, 
						new FileInfo( "LTC.dds" ),
						ImageUtility.PIXEL_FORMAT.RGBA32F
						);

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

// Debug a specific case
//form.RoughnessIndex = 19;
//form.ThetaIndex = 40;

form.UseAdaptiveFit = true;


			form.SetupBRDF( _BRDF, 64, _tableFileName );

			Application.Run( form );

// 			ApplicationContext	ctxt = new ApplicationContext( form );
// 			ctxt.ThreadExit += ctxt_ThreadExit;
// 			Application.Run( ctxt );
//			ctxt.Dispose();
		}

		static void	Export( FileInfo _tableFileName, FileInfo _targetFileName, string _BRDFName ) {
			int		validResultsCount;
			LTC[,]	table = FitterForm.LoadTable( _tableFileName, out validResultsCount );

			string	sourceCode = "";

			// Export LTC matrices
			int	tableSize = table.GetLength(0);
			LTC	defaultLTC = new LTC();
				defaultLTC.amplitude = 0.0;

		#if EXPORT_FOR_UNITY
			string	tableName = "s_LtcMatrixData_" + _BRDFName;

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
						+ "        // The table is accessed via LTCAreaLight." + tableName + "[<roughnessIndex> + 64 * <thetaIndex>]    // Theta values are along the Y axis, Roughness values are along the X axis\r\n"
						+ "        //    • roughness = ( <roughnessIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //    • cosTheta = 1 - ( <thetaIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //\r\n"
						+ "        public static double[,]	" + tableName + " = new double[k_LtcLUTResolution * k_LtcLUTResolution, k_LtcLUTMatrixDim * k_LtcLUTMatrixDim] {\r\n";

			for ( int thetaIndex=0; thetaIndex < tableSize; thetaIndex++ ) {
				string	matrixRowString = "            ";
				for ( int roughnessIndex=0; roughnessIndex < tableSize; roughnessIndex++ ) {
					LTC		ltc = table[roughnessIndex,thetaIndex];
					if ( ltc == null ) {
						ltc = defaultLTC;
					}

					string	matrixString  = ltc.invM[0,0] + ", " + ltc.invM[0,1] + ", " + ltc.invM[0,2] + ", ";
							matrixString += ltc.invM[1,0] + ", " + ltc.invM[1,1] + ", " + ltc.invM[1,2] + ", ";
							matrixString += ltc.invM[2,0] + ", " + ltc.invM[2,1] + ", " + ltc.invM[2,2];
					matrixRowString += "{ " + matrixString + " }, ";
				}

				// Compute theta
				float	y = (float) thetaIndex / (tableSize-1);
				float	cosTheta = 1 - y * y;

				matrixRowString += "   // Cos(theta) = " + cosTheta + "\r\n";

 				sourceCode += matrixRowString;
			}
		#else
			string	className = "LTCData_" + _BRDFName;

			sourceCode += "using System;\r\n"
						+ "\r\n"
						+ "namespace LTCAreaLight\r\n"
						+ "{\r\n"
						+ "    public partial class " + className + "\r\n"
						+ "    {\r\n"
						+ "        // Table contains 3x3 matrix coefficients of M^-1 for the fitting of the " + _BRDFName + " BRDF using the LTC technique\r\n"
						+ "        // From \"Real-Time Polygonal-Light Shading with Linearly Transformed Cosines\" 2016 (https://eheitzresearch.wordpress.com/415-2/)\r\n"
						+ "        //\r\n"
						+ "        // The table is accessed via LTCAreaLight." + className + "[64 * <roughnessIndex> + <thetaIndex>]    // Theta values are on X axis, Roughness values are on Y axis\r\n"
						+ "        //    • roughness = ( <roughnessIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //    • cosTheta = 1 - ( <thetaIndex> / " + (tableSize-1) + " )^2\r\n"
						+ "        //\r\n"
						+ "        public static double[,]	s_invM = new double[" + tableSize + " * " + tableSize +", 3 * 3] {\r\n";

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

// 				// Compute roughness
// 				float	perceptualRoughness = (float) roughnessIndex / (tableSize-1);
// 				float	alpha = perceptualRoughness * perceptualRoughness;
// 
// //				matrixRowString += " },\r\n";
// 				matrixRowString += "   // Roughness = " + alpha + "\r\n";

throw new Exception( "Ta mère!" );
 				sourceCode += matrixRowString;

			}
		#endif

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

		/// <summary>
		/// Concatenates multiple tables into one single texture 2D array
		/// </summary>
		/// <param name="_tablesFileNames"></param>
		/// <param name="_targetFileName"></param>
		/// <param name="_foramt"></param>
		static void ExportTexture( FileInfo[] _tablesFileNames, FileInfo _targetFileName, ImageUtility.PIXEL_FORMAT _format ) {
			// Load tables
			LTC[][,]	tables = new LTC[_tablesFileNames.Length][,];
			for ( int i=0; i < _tablesFileNames.Length; i++ ) {
				int		validResultsCount;
				LTC[,]	table = FitterForm.LoadTable( _tablesFileNames[i], out validResultsCount );
				if ( validResultsCount != table.Length )
					throw new Exception( "Not all table results are valid!" );

				tables[i] = table;
				if ( i != 0 && (table.GetLength(0) != tables[0].GetLength(0) || table.GetLength(1) != tables[0].GetLength(1)) )
					throw new Exception( "Table dimensions mismatch!" );
			}

			// Create the Texture2DArray
			uint	W = (uint) tables[0].GetLength(0);
			uint	H = (uint) tables[0].GetLength(1);

			ImageUtility.ImagesMatrix	M = new ImageUtility.ImagesMatrix();
			M.InitTexture2DArray( W, H, (uint) tables.Length, 1 );
			M.AllocateImageFiles( _format, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR ) );

			for ( int i=0; i < tables.Length; i++ ) {
				LTC[,]	table = tables[i];
// 				ImageUtility.ImageFile	I = new ImageUtility.ImageFile( W, H, _format, profile );
// 				M[(uint) i][0][0] = I;
				ImageUtility.ImageFile	I = M[(uint) i][0][0];
				I.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
					LTC	ltc = table[_X,_Y];
					_color.x = (float) ltc.invM[0,0];
					_color.y = (float) ltc.invM[0,2];
					_color.z = (float) ltc.invM[1,1];
					_color.w = (float) ltc.invM[2,0];
				} );
			}
			M.DDSSaveFile( _targetFileName, ImageUtility.COMPONENT_FORMAT.AUTO );
		}
	}
}
