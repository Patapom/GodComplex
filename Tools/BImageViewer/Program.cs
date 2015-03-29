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
			if ( _args.Length != 1 ) {
				MessageBox.Show( "Missing filename argument! Can't open unspecified file...", "BImage Viewer" );
				return;
			}

			System.IO.FileInfo	ImageFileName = new System.IO.FileInfo( _args[0] );
			if ( !ImageFileName.Exists ) {
				MessageBox.Show( "Specified image name \"" + _args[1] + "\" not found on disk!", "BImage Viewer" );
				return;
			}

			BImage	Image = null;
			try {
				Image = new BImage( ImageFileName );
			} catch ( Exception _e ) {
				MessageBox.Show( "An error occurred while loading bimage \"" + ImageFileName.FullName + "\":\r\n" + _e.Message, "BImage Viewer" );
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new ViewerForm( Image ) );
		}
	}
}
