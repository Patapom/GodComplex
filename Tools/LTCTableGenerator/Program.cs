#define FIT_TABLES
//#define EXPORT_FOR_UNITY
//#define EXPORT_TEXTURE

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
				bool	usePreviousRoughness = false;

				switch ( BRDFIndex ) {
					// Fit specular
					case 0:
						RunForm( new BRDF_GGX(), new FileInfo( "GGX.ltc" ), usePreviousRoughness );						// Fit GGX
						break;
					case 1:
						RunForm( new BRDF_CookTorrance(), new FileInfo( "CookTorrance.ltc" ), usePreviousRoughness );	// Fit Cook-Torrance
						break;
					case 2:
						RunForm( new BRDF_Ward(), new FileInfo( "Ward.ltc" ), usePreviousRoughness );					// Fit Ward
						break;

					// Fit diffuse
					case 10:
						RunForm( new BRDF_OrenNayar(), new FileInfo( "OrenNayar.ltc" ), usePreviousRoughness );			// Fit Oren-Nayar diffuse
						break;
					case 11:
						RunForm( new BRDF_Charlie(), new FileInfo( "CharlieSheen.ltc" ), usePreviousRoughness );		// Fit Charlie Sheen diffuse
						break;
					case 12:
						RunForm( new BRDF_Disney(), new FileInfo( "Disney.ltc" ), usePreviousRoughness );				// Fit Disney diffuse
						break;

					default:
						MessageBox.Show( "Unsupported BRDF index: " + BRDFIndex, "Invalid Argument!", MessageBoxButtons.OK, MessageBoxIcon.Error );
						return;
				}
			#endif

			// Export
			#if EXPORT_FOR_UNITY
//				string	targetDir = @"D:\Workspaces\Unity Labs\AxF Unity Project\SRP\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\";
				string	targetDir = @"D:\Workspaces\Unity Labs\SRP-AreaLights\com.unity.render-pipelines.high-definition\HDRP\Material\LTCAreaLight\";

				Export( new FileInfo( "GGX.ltc" ), new FileInfo( targetDir + "LtcData.GGX2.cs" ), "GGX" );
 				Export( new FileInfo( "CookTorrance.ltc" ), new FileInfo( targetDir + "LtcData.CookTorrance.cs" ), "CookTorrance" );
 				Export( new FileInfo( "Ward.ltc" ), new FileInfo( targetDir + "LtcData.Ward.cs" ), "Ward" );
 
 				Export( new FileInfo( "OrenNayar.ltc" ), new FileInfo( targetDir + "LtcData.OrenNayar.cs" ), "OrenNayar" );
 				Export( new FileInfo( "CharlieSheen.ltc" ), new FileInfo( targetDir + "LtcData.CharlieSheen.cs" ), "Charlie" );
 				Export( new FileInfo( "Disney.ltc" ), new FileInfo( targetDir + "LtcData.DisneyDiffuse2.cs" ), "Disney" );
			#endif

			#if EXPORT_TEXTURE
// 				Export( new FileInfo( "GGX.ltc" ), new FileInfo( "GGX.cs" ), "GGX" );
// 				Export( new FileInfo( "CookTorrance.ltc" ), new FileInfo( "CookTorrance.cs" ), "CookTorrance" );
// 				Export( new FileInfo( "CharlieSheen.ltc" ), new FileInfo( "CharlieSheen.cs" ), "Charlie" );

				ExportTexture( new FileInfo[] {
									// Specular
									new FileInfo( "GGX.ltc" ),
									new FileInfo( "CookTorrance.ltc" ),
									new FileInfo( "Ward.ltc" ),

									// Diffuse
									new FileInfo( "OrenNayar.ltc" ),
									new FileInfo( "CharlieSheen.ltc" ),
									new FileInfo( "Disney.ltc" ),
								}, 
					new FileInfo( "LTC.dds" ),
					ImageUtility.PIXEL_FORMAT.RGBA32F
				);

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
//form.RoughnessIndex = 23;
//form.ThetaIndex = 57;

form.UseAdaptiveFit = false;


			form.SetupBRDF( _BRDF, 64, _tableFileName );

			Application.Run( form );
		}

		static void	Export( FileInfo _tableFileName, FileInfo _targetFileName, string _BRDFName ) {
			int		validResultsCount;
			LTC[,]	table = FitterForm.LoadTable( _tableFileName, out validResultsCount );

			string	sourceCode = "";

			// Export LTC matrices
			int	tableSize = table.GetLength(0);
			LTC	defaultLTC = new LTC();
				defaultLTC.magnitude = 0.0;

		#if EXPORT_FOR_UNITY
			string	tableName = "s_LtcMatrixData_" + _BRDFName;

			sourceCode += "using UnityEngine;\n"
						+ "using System;\n"
						+ "\n"
						+ "namespace UnityEngine.Experimental.Rendering.HDPipeline\n"
						+ "{\n"
						+ "    public partial class LTCAreaLight\n"
						+ "    {\n"
						+ "        // Table contains 3x3 matrix coefficients of M^-1 for the fitting of the " + _BRDFName + " BRDF using the LTC technique\n"
						+ "        // From \"Real-Time Polygonal-Light Shading with Linearly Transformed Cosines\" 2016 (https://eheitzresearch.wordpress.com/415-2/)\n"
						+ "        //\n"
						+ "        // The table is accessed via LTCAreaLight." + tableName + "[<roughnessIndex> + 64 * <thetaIndex>]    // Theta values are along the Y axis, Roughness values are along the X axis\n"
						+ "        //    • roughness = ( <roughnessIndex> / " + (tableSize-1) + " )^2  (the table is indexed by perceptual roughness)\n"
						+ "        //    • cosTheta = 1 - ( <thetaIndex> / " + (tableSize-1) + " )^2\n"
						+ "        //\n"
						+ "        public static double[,]	" + tableName + " = new double[k_LtcLUTResolution * k_LtcLUTResolution, k_LtcLUTMatrixDim * k_LtcLUTMatrixDim] {";

			string	lotsOfSpaces = "                                                                                                                            ";

			float	alpha, cosTheta;
			for ( int thetaIndex=0; thetaIndex < tableSize; thetaIndex++ ) {
				#if true
					FitterForm.GetRoughnessAndAngle( 0, thetaIndex, tableSize, tableSize, out alpha, out cosTheta );
	 				sourceCode += "\n";
	 				sourceCode += "            // Cos(theta) = " + cosTheta + "\n";

					for ( int roughnessIndex=0; roughnessIndex < tableSize; roughnessIndex++ ) {
						LTC		ltc = table[roughnessIndex,thetaIndex];
						if ( ltc == null ) {
							ltc = defaultLTC;
						}
						FitterForm.GetRoughnessAndAngle( roughnessIndex, thetaIndex, tableSize, tableSize, out alpha, out cosTheta );

						// Export the matrix as a list of 3x3 doubles, columns first
//						double	factor = 1.0 / ltc.invM[2,2];
						double	factor = 1.0 / ltc.invM[1,1];

						string	matrixString  = (factor * ltc.invM[0,0]) + ", " + (factor * ltc.invM[1,0]) + ", " + (factor * ltc.invM[2,0]) + ", ";
								matrixString += (factor * ltc.invM[0,1]) + ", " + (factor * ltc.invM[1,1]) + ", " + (factor * ltc.invM[2,1]) + ", ";
								matrixString += (factor * ltc.invM[0,2]) + ", " + (factor * ltc.invM[1,2]) + ", " + (factor * ltc.invM[2,2]);

						string	line = "            { " + matrixString + " },";
	 					sourceCode += line;
						if ( line.Length < 132 )
							sourceCode += lotsOfSpaces.Substring( lotsOfSpaces.Length - (132 - line.Length) );	// Pad with spaces
						sourceCode += "// alpha = " + alpha + "\n";
					}
				#else
					string	matrixRowString = "            ";
					for ( int roughnessIndex=0; roughnessIndex < tableSize; roughnessIndex++ ) {
						LTC		ltc = table[roughnessIndex,thetaIndex];
						if ( ltc == null ) {
							ltc = defaultLTC;
						}

						// Export the matrix as a list of 3x3 doubles, columns first
						string	matrixString  = ltc.invM[0,0] + ", " + ltc.invM[1,0] + ", " + ltc.invM[2,0] + ", ";
								matrixString += ltc.invM[0,1] + ", " + ltc.invM[1,1] + ", " + ltc.invM[2,1] + ", ";
								matrixString += ltc.invM[0,2] + ", " + ltc.invM[1,2] + ", " + ltc.invM[2,2];

						matrixRowString += "{ " + matrixString + " }, ";
					}

					// Compute theta
					float	alpha, cosTheta;
					FitterForm.GetRoughnessAndAngle( 0, thetaIndex, tableSize, tableSize, out alpha, out cosTheta );

					matrixRowString += "   // Cos(theta) = " + cosTheta + "\n";
 					sourceCode += matrixRowString;
				#endif
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

			sourceCode += "        };\n";

			// Export LTC amplitude and fresnel
			//
//        public static float[] s_LtcGGXMagnitudeData = new float[k_LtcLUTResolution * k_LtcLUTResolution]
//        public static float[] s_LtcGGXFresnelData = new float[k_LtcLUTResolution * k_LtcLUTResolution]

			sourceCode += "\n";
			sourceCode += "        // NOTE: Formerly, we needed to also export and create a table for the BRDF's amplitude factor + fresnel coefficient\n";
			sourceCode += "        //    but it turns out these 2 factors are actually already precomputed and available in the FGD table corresponding\n";
			sourceCode += "        //    to the " + _BRDFName + " BRDF, therefore they are no longer exported...\n";

			// Close class and namespace
			sourceCode += "    }\n";
			sourceCode += "}\n";

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

				double	largest = 0;	// Keep track of largest error from 1
				ImageUtility.ImageFile	I = M[(uint) i][0][0];
				I.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => {
					LTC	ltc = table[_X,_Y];

					const double	tol = 1e-6;
// 					if ( Mathf.Abs( ltc.invM[2,2] - 1 ) > tol )
// 						throw new Exception( "Not one!" );
					if ( Mathf.Abs( ltc.invM[0,1] ) > tol || Mathf.Abs( ltc.invM[1,0] ) > tol || Mathf.Abs( ltc.invM[1,2] ) > tol || Mathf.Abs( ltc.invM[2,1] ) > tol )
						throw new Exception( "Not zero!" );

					// Former code used to normalize by m22 term but according to Hill, this leads to poorly interpolatble tables (cf.  page 81 of https://blog.selfshadow.com/publications/s2016-advances/s2016_ltc_rnd.pdf)
// 					largest = Math.Max( largest, Math.Abs( ltc.invM[2,2] - 1 ) );
// 					double	factor = 1.0 / ltc.invM[2,2];
// 
// 					_color.x = (float) (factor * ltc.invM[0,0]);
// 					_color.y = (float) (factor * ltc.invM[0,2]);
// 					_color.z = (float) (factor * ltc.invM[1,1]);
// 					_color.w = (float) (factor * ltc.invM[2,0]);

					// Instead, normalize by m11!
 					largest = Math.Max( largest, Math.Abs( ltc.invM[1,1] - 1 ) );
					double	factor = 1.0 / ltc.invM[1,1];

					_color.x = (float) (factor * ltc.invM[0,0]);
					_color.y = (float) (factor * ltc.invM[0,2]);
					_color.z = (float) (factor * ltc.invM[2,0]);
					_color.w = (float) (factor * ltc.invM[2,2]);
				} );
			}
			M.DDSSaveFile( _targetFileName, ImageUtility.COMPONENT_FORMAT.AUTO );
		}
	}
}
