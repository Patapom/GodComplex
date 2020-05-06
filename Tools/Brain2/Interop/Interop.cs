using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Brain2 {

	public static class Interop {

		// Windows messages
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int WM_HOTKEY = 0x0312;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYUP = 0x0101;
		public const int WM_SYSKEYDOWN = 0x0104;
		public const int WM_SYSKEYUP = 0x0105;
		public const int WM_INITDIALOG = 0x0110;

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

		// Window styles
		public const int GWL_EXSTYLE = -20;				// GetWindowLong parameter index

			// Extended styles (from https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles)
		public const int WS_EX_ACCEPTFILES			= 0x00000010;	// The window accepts drag-drop files.
		public const int WS_EX_APPWINDOW			= 0x00040000;	// Forces a top-level window onto the taskbar when the window is visible.
		public const int WS_EX_CLIENTEDGE			= 0x00000200;	// The window has a border with a sunken edge.
		public const int WS_EX_COMPOSITED			= 0x02000000;	// Paints all descendants of a window in bottom-to-top painting order using double-buffering. Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects, but only if the descendent window also has the WS_EX_TRANSPARENT bit set. Double-buffering allows the window and its descendents to be painted without flicker. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. Windows 2000: This style is not supported.
		public const int WS_EX_CONTEXTHELP			= 0x00000400;	// The title bar of the window includes a question mark. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window. WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
		public const int WS_EX_CONTROLPARENT		= 0x00010000;	// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
		public const int WS_EX_DLGMODALFRAME		= 0x00000001;	// The window has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
		public const int WS_EX_LAYERED				= 0x00080000;	// The window is a layered window. This style cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. Windows 8: The WS_EX_LAYERED style is supported for top-level windows and child windows. Previous Windows versions support WS_EX_LAYERED only for top-level windows.
		public const int WS_EX_LAYOUTRTL			= 0x00400000;	// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left.
		public const int WS_EX_LEFT					= 0x00000000;	// The window has generic left-aligned properties. This is the default.
		public const int WS_EX_LEFTSCROLLBAR		= 0x00004000;	// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
		public const int WS_EX_LTRREADING			= 0x00000000;	// The window text is displayed using left-to-right reading-order properties. This is the default.
		public const int WS_EX_MDICHILD				= 0x00000040;	// The window is a MDI child window.
		public const int WS_EX_NOACTIVATE			= 0x08000000;	// A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window. The window should not be activated through programmatic access or via keyboard navigation by accessible technology, such as Narrator. To activate the window, use the SetActiveWindow or SetForegroundWindow function. The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
		public const int WS_EX_NOINHERITLAYOUT		= 0x00100000;	// The window does not pass its window layout to its child windows.
		public const int WS_EX_NOPARENTNOTIFY		= 0x00000004;	// The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
		public const int WS_EX_NOREDIRECTIONBITMAP	= 0x00200000;	// The window does not render to a redirection surface. This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
		public const int WS_EX_OVERLAPPEDWINDOW		= (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);					// The window is an overlapped window.
		public const int WS_EX_PALETTEWINDOW		= (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);	// The window is palette window, which is a modeless dialog box that presents an array of commands.
		public const int WS_EX_RIGHT				= 0x00001000;	// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored. Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
		public const int WS_EX_RIGHTSCROLLBAR		= 0x00000000;	// The vertical scroll bar (if present) is to the right of the client area. This is the default.
		public const int WS_EX_RTLREADING			= 0x00002000;	// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
		public const int WS_EX_STATICEDGE			= 0x00020000;	// The window has a three-dimensional border style intended to be used for items that do not accept user input.
		public const int WS_EX_TOOLWINDOW			= 0x00000080;	// The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
		public const int WS_EX_TOPMOST				= 0x00000008;	// The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
		public const int WS_EX_TRANSPARENT			= 0x00000020;	// The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted. To achieve transparency without these restrictions, use the SetWindowRgn function.
		public const int WS_EX_WINDOWEDGE			= 0x00000100;	// The window has a border with a raised edge.

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
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dsNewLong );

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

		public static Rectangle	GetDesktopBounds() {
			Point	min = new Point(  10000,  10000 );
			Point	max = new Point( -10000, -10000 );
			foreach ( Screen screen in Screen.AllScreens ) {
				min.X = Math.Min( min.X, screen.Bounds.X );
				min.Y = Math.Min( min.Y, screen.Bounds.Y );
				max.X = Math.Max( max.X, screen.Bounds.Right );
				max.Y = Math.Max( max.Y, screen.Bounds.Bottom );
			}
			return new Rectangle( min.X, min.Y, max.X - min.X, max.Y - min.Y );
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
