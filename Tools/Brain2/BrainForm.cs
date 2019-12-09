// Sources:
//	https://dotnetbrowser.support.teamdev.com/support/solutions/articles/9000109708-saving-web-page-to-png-image
//	https://stackoverflow.com/questions/2715385/convert-webpage-to-image-from-asp-net
//	https://www.codeproject.com/Articles/12629/WYSIWYG-HTML-Editor	<== Old COM issues
//	https://github.com/UweKeim/ZetaHtmlEditControl	<== Currently used
//	https://www.telerik.com/support/kb/winforms/details/how-to-embed-chrome-browser-in-a-winforms-application	<== Chrome in Control
//	https://mkunc.com/2012/02/18/automating-chrome-browser-from-csharp/ <== Driving Chrome Dev Tools
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ImageUtility;
using Renderer;

namespace Brain2 {
	public partial class BrainForm : Form {
		#region FIELDS

		Device		m_device = new Device();

		#endregion

		#region METHODS

		public BrainForm() {
			InitializeComponent();

			try {
				// Attempt to retrieve containing monitor
				IntPtr	hMonitor;
				Screen	screen;
				GetMonitorFromPosition( Control.MousePosition, out screen, out hMonitor );

				// Rescale window to fullscreen overlay
				this.SetDesktopBounds( screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height );

				// Create fullscreen windowed device
				m_device.Init( this.Handle, false, true );

			} catch ( Exception _e ) {
				MessageBox.Show( "Error when creating D3D device!" + _e.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error );
				Application.Exit();
			}
		}

		protected override void Dispose(bool disposing) {
			if( disposing && components != null ) {
				components.Dispose();
			}
			
			m_device.Dispose();

			base.Dispose(disposing);
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if ( e.KeyCode == Keys.Escape )
				Close();
		}

		#region Monitors Management

		// Code from https://stackoverflow.com/questions/6279076/how-to-use-win32-getmonitorinfo-in-net-c

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct POINTSTRUCT { 
			public int x;
			public int y;
			public POINTSTRUCT(int x, int y) {
				this.x = x; 
				this.y = y;
			} 
		} 

		const int	MONITOR_DEFAULTTONULL       = 0x00000000;
		const int	MONITOR_DEFAULTTOPRIMARY    = 0x00000001;
		const int	MONITOR_DEFAULTTONEAREST    = 0x00000002;

		[System.Runtime.InteropServices.DllImport("User32.dll", ExactSpelling=true)]
		static extern IntPtr MonitorFromPoint( POINTSTRUCT pt, int flags );

// 		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,CharSet=CharSet.Auto, Pack=4)]
// 		public class MONITORINFOEX { 
// 			public int     cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MONITORINFOEX));
// 			public RECT    rcMonitor = new RECT(); 
// 			public RECT    rcWork = new RECT(); 
// 			public int     dwFlags = 0;
// 			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=32)] 
// 			public char[]  szDevice = new char[32];
// 		}

		void	GetMonitorFromPosition( Point _position, out Screen _containingScreen, out IntPtr _hMonitor ) {
			_containingScreen = null;
			_hMonitor = IntPtr.Zero;
			foreach ( Screen screen in Screen.AllScreens ) {
				if ( screen.Bounds.Contains( _position ) ) {
					_containingScreen = screen;
					break;
				}
			}
			if ( _containingScreen == null )
				throw new Exception( "Provided position is not contained by any monitor!" );

			// Also retrieve the HMONITOR value
			POINTSTRUCT	pt = new POINTSTRUCT( _position.X, _position.Y );
			_hMonitor = MonitorFromPoint( pt, MONITOR_DEFAULTTONEAREST );
		}

		#endregion

		#endregion
	}
}
