using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// Use this namespace to use the ACTIVE version of the monitor that can trigger measurements (this needs the version 1 of the tank monitor Arduino code!)
// This first version was okay as it allowed to send measurement commands whenever we liked but it made the monitor station keep listening for commands
//	which is a bad idea regarding power consumption, especially in winter times!
//
//namespace WaterTankMonitor {

// Use this namespace to use the PASSIVE version of the monitor that can only receive measurements (this needs the version 2 of the tank monitor Arduino code!)
// This version is preferred as the monitoring device placed in the field is allowed to go into deep sleep mode and save energy, only to wake up every 10 minutes
//	or so and send a bunch of 16 measurements at the same time. The first measurement is the last one that was made, the 16th one is the oldest that can go up to
//	16 * 10 minutes = 2 hours and 40 minutes in the past (if nothing happened and flow was steady, otherwise measurements can occur more frequently)
//
namespace WaterTankMonitorPassive {

	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo( "en-US" );

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new WaterTankMonitorForm() );
		}
	}
}
