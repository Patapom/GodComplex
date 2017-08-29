using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BImageViewer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( string[] _args )
		{


//_args = new string[] { @"..\..\..\Arkane\BImages\stainedglass2_area.bimage7" };
//_args = new string[] { @"..\..\..\Arkane\CubeMaps\dust_return\pr_obe_1127_cube_BC6H_UF16.bimage" };
//_args = new string[] { @"D:\Workspaces\Arkane\Dishonored2\Dishonored2\base\editors\arkane\preview\cubemaps\exterior.bimage" };


TestResourcesIndex();


			if ( _args.Length != 1 ) {
				MessageBox.Show( "Missing filename argument! Can't open unspecified file...", "BImage Viewer" );
				return;
			}

			System.IO.FileInfo	imageFileName = new System.IO.FileInfo( _args[0] );
			if ( !imageFileName.Exists ) {
				MessageBox.Show( "Specified image name \"" + _args[1] + "\" not found on disk!", "BImage Viewer" );
				return;
			}

			ArkaneService.BImage	Image = null;
			try {
				using ( System.IO.FileStream S = imageFileName.OpenRead() )
					using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
						Image = new ArkaneService.BImage( R, imageFileName, true );
					}
			} catch ( Exception _e ) {
				MessageBox.Show( "An error occurred while loading bimage \"" + imageFileName.FullName + "\":\r\n\r\n" + _e.Message, "BImage Viewer" );
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new ViewerForm( Image ) );
		}

		static void		TestResourcesIndex() {
			ArkaneService.ResourcesIndex	masterIndex = new ArkaneService.ResourcesIndex( new System.IO.FileInfo( @"D:\Workspaces\Arkane\Dishonored2\dishonored2_GMC2_782384_1.70.0.24\dishonored2_GMC2_782384_MULTI_2016_09_29_01_47_PC\base\master.index" ) );
		}
	}
}
