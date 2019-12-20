using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Brain2 {

	public static class Interop {

		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int WM_HOTKEY = 0x0312;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;
		public const int WM_SYSKEYDOWN = 0x0104;
		public const int WM_SYSKEYUP = 0x0105;

		// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest
		public const int HTCAPTION = 0x2;
		public const int HTLEFT = 10;
		public const int HTRIGHT = 11;
		public const int HTTOP = 12;
		public const int HTTOPLEFT = 13;
		public const int HTTOPRIGHT = 14;
		public const int HTBOTTOM = 15;
		public const int HTBOTTOMLEFT = 16;
		public const int HTBOTTOMRIGHT = 17;

		/// <summary>
		/// The enumeration of possible modifiers.
		/// </summary>
		[Flags]
		public enum NativeModifierKeys : uint {
			None = 0,
			Alt = 1,
			Control = 2,
			Shift = 4,
			Win = 8,
		}
		// Registers a hot key with Windows.
		[DllImport("User32.dll", ExactSpelling=true)]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		// Unregisters the hot key with Windows.
		[DllImport("User32.dll", ExactSpelling=true)]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		// Code from https://stackoverflow.com/questions/6279076/how-to-use-win32-getmonitorinfo-in-net-c

		[StructLayout(LayoutKind.Sequential)]
		public struct POINTSTRUCT { 
			public int x;
			public int y;
			public POINTSTRUCT(int x, int y) {
				this.x = x; 
				this.y = y;
			} 
		} 

		#region Monitors Management

		const int	MONITOR_DEFAULTTONULL       = 0x00000000;
		const int	MONITOR_DEFAULTTOPRIMARY    = 0x00000001;
		const int	MONITOR_DEFAULTTONEAREST    = 0x00000002;

		[DllImport("User32.dll", ExactSpelling=true)]
		static extern IntPtr MonitorFromPoint( POINTSTRUCT pt, int flags );

// 		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto, Pack=4)]
// 		public class MONITORINFOEX { 
// 			public int     cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
// 			public RECT    rcMonitor = new RECT(); 
// 			public RECT    rcWork = new RECT(); 
// 			public int     dwFlags = 0;
// 			[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] 
// 			public char[]  szDevice = new char[32];
// 		}

		public static void	GetMonitorFromPosition( Point _position, out Screen _containingScreen, out IntPtr _hMonitor ) {
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

		#region Hot Key Management

		/// <summary>
		/// Registers a hot key in the system.
		/// </summary>
		/// <param name="modifier">The modifiers that are associated with the hot key.</param>
		/// <param name="key">The key itself that is associated with the hot key.</param>
		public static void RegisterHotKey( Control _owner, Interop.NativeModifierKeys modifier, Keys key ) {
			RegisterHotKey( _owner, 0, modifier, key );
		}
		public static void RegisterHotKey( Control _owner, int _ID, Interop.NativeModifierKeys modifier, Keys key ) {
			if ( !Interop.RegisterHotKey( _owner.Handle, 0, (uint) modifier, (uint) key ) )
				throw new InvalidOperationException( "Couldn’t register the hot key." );
		}

		#endregion

		#region Window Handles Collecting

		// Code from https://pinvoke.net/default.aspx/user32/EnumWindows.html
		// and from https://www.experts-exchange.com/questions/24331722/Getting-the-list-of-Open-Window-Handles-in-C.html
		// and a condescending bit from http://csharphelper.com/blog/2016/08/list-desktop-windows-in-c/

		public delegate bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumWindows(EnumWindowsCallback lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
		ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsCallback lpEnumCallbackFunction, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);
 
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
 
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		public static IntPtr[]	GetWindowHandles() {    
			List<IntPtr>	windowHandles = new List<IntPtr>();
//			EnumWindows( ( IntPtr hWnd, IntPtr lParam ) => { windowHandles.Add( hWnd ); return true; }, IntPtr.Zero );
			EnumDesktopWindows( IntPtr.Zero, ( IntPtr hWnd, IntPtr lParam ) => { windowHandles.Add( hWnd ); return true; }, IntPtr.Zero );
			
			return windowHandles.ToArray();
		}

		#endregion
	}
}
