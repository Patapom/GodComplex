namespace ZetaHtmlEditControl.Code.PInvoke
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class NativeMethods
	{
        public enum ContextMenuKind
        {
            #region Enum members.

            CONTEXT_MENU_DEFAULT = 0,
            CONTEXT_MENU_IMAGE = 1,
            CONTEXT_MENU_CONTROL = 2,
            CONTEXT_MENU_TABLE = 3,

            CONTEXT_MENU_TEXTSELECT = 4,
            CONTEXT_MENU_ANCHOR = 5,
            CONTEXT_MENU_UNKNOWN = 6

            #endregion
        }

        [Flags]
        public enum DOCHOSTUIFLAG
        {
            #region Enum members.

            DOCHOSTUIFLAG_DIALOG = 0x00000001,
            DOCHOSTUIFLAG_DISABLE_HELP_MENU = 0x00000002,
            DOCHOSTUIFLAG_NO3DBORDER = 0x00000004,
            DOCHOSTUIFLAG_SCROLL_NO = 0x00000008,
            DOCHOSTUIFLAG_DISABLE_SCRIPT_INACTIVE = 0x00000010,
            DOCHOSTUIFLAG_OPENNEWWIN = 0x00000020,
            DOCHOSTUIFLAG_DISABLE_OFFSCREEN = 0x00000040,
            DOCHOSTUIFLAG_FLAT_SCROLLBAR = 0x00000080,
            DOCHOSTUIFLAG_DIV_BLOCKDEFAULT = 0x00000100,
            DOCHOSTUIFLAG_ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
            DOCHOSTUIFLAG_OVERRIDEBEHAVIORFACTORY = 0x00000400,
            DOCHOSTUIFLAG_CODEPAGELINKEDFONTS = 0x00000800,
            DOCHOSTUIFLAG_URL_ENCODING_DISABLE_UTF8 = 0x00001000,
            DOCHOSTUIFLAG_URL_ENCODING_ENABLE_UTF8 = 0x00002000,
            DOCHOSTUIFLAG_ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
            DOCHOSTUIFLAG_ENABLE_INPLACE_NAVIGATION = 0x00010000,
            DOCHOSTUIFLAG_IME_ENABLE_RECONVERSION = 0x00020000,
            DOCHOSTUIFLAG_THEME = 0x00040000,
            DOCHOSTUIFLAG_NOTHEME = 0x00080000,
            DOCHOSTUIFLAG_NOPICS = 0x00100000,
            DOCHOSTUIFLAG_NO3DOUTERBORDER = 0x00200000,
            DOCHOSTUIFLAG_DISABLE_EDIT_NS_FIXUP = 0x00400000,
            DOCHOSTUIFLAG_LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
            DOCHOSTUIFLAG_DISABLE_UNTRUSTEDPROTOCOL = 0x01000000

            #endregion
        }

        public const int WmKeydown = 0x100;
        public const int WmSyskeydown = 0x104;
        public const int IdmPrint = 27;
        public const int IdmPrintpreview = 2003;

        public static readonly int BOOL_FALSE = 0;
		public static readonly int BOOL_TRUE = 1;

        [StructLayout( LayoutKind.Sequential )]
        public class COMRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            //public COMRECT();
            //public COMRECT(Rectangle r);
            //public COMRECT(int left, int top, int right, int bottom);
            //public static NativeMethods.COMRECT FromXYWH(int x, int y, int width, int height);
            //public override string ToString();
            public COMRECT()
            {
            }

            public COMRECT( Rectangle r )
            {
                left = r.X;
                top = r.Y;
                right = r.Right;
                bottom = r.Bottom;
            }

            public COMRECT( int left, int top, int right, int bottom )
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public static COMRECT FromXYWH( int x, int y, int width, int height )
            {
                return new COMRECT( x, y, x + width, y + height );
            }

            public override string ToString()
            {
                return String.Concat( new object[] 
                { "Left = ", left, " Top ", top, " Right = ", right, " Bottom = ", bottom } );
            }
        }

        [StructLayout( LayoutKind.Sequential ), ComVisible( true )]
        public class DOCHOSTUIINFO
        {
            [MarshalAs( UnmanagedType.U4 )]
            public int cbSize;
            [MarshalAs( UnmanagedType.I4 )]
            public int dwFlags;
            [MarshalAs( UnmanagedType.I4 )]
            public int dwDoubleClick;
            [MarshalAs( UnmanagedType.I4 )]
            public int dwReserved1;
            [MarshalAs( UnmanagedType.I4 )]
            public int dwReserved2;
            //public DOCHOSTUIINFO();
            public DOCHOSTUIINFO()
            {
                cbSize = Marshal.SizeOf( typeof( DOCHOSTUIINFO ) );
            }
        }

        [Guid("00020400-0000-0000-C000-000000000046")]
		[TypeLibType(512)]
		[ComImport]
		public interface IDispatch
		{
		}

        [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), Guid( @"B722BCCB-4E68-101B-A2BC-00AA00404770" ), ComVisible( true )]
		public interface IOleCommandTarget
		{
			[return: MarshalAs( UnmanagedType.I4 )]
			[PreserveSig]
			int QueryStatus( ref Guid pguidCmdGroup, int cCmds, [In, Out] OLECMD prgCmds, [In, Out] IntPtr pCmdText );
			[return: MarshalAs( UnmanagedType.I4 )]
			[PreserveSig]
			int Exec( ref Guid pguidCmdGroup, int nCmdID, int nCmdexecopt, 
                 [In, MarshalAs(UnmanagedType.LPArray)] object[] pvaIn,
                 [Out, MarshalAs(UnmanagedType.LPArray)] object[] pvaOut);
		}

        [Guid(@"00000000-0000-0000-C000-000000000046")]
        [InterfaceType(1)]
        [TypeLibType(16)]
        [ComImport]
        public interface IUnknown
        {
        }

        [Serializable, StructLayout( LayoutKind.Sequential )]
		public struct MSG
		{
			public IntPtr hwnd;
			public int message;
			public IntPtr wParam;
			public IntPtr lParam;
			public int time;
			public int pt_x;
			public int pt_y;
		}

        [StructLayout( LayoutKind.Sequential )]
        public class OLECMD
        {
            [MarshalAs( UnmanagedType.U4 )]
            public int cmdID;
            [MarshalAs( UnmanagedType.U4 )]
            public int cmdf;
            //public OLECMD();
        }

        [StructLayout( LayoutKind.Sequential )]
        public class POINT
        {
            public int x;
            public int y;
            public POINT()
            {
            }

            public POINT( int x, int y )
            {
                this.x = x;
                this.y = y;
            }
        }

        public static class SRESULTS
        {
            #region Public properties.

            public static readonly int S_OK = 0;
            public static readonly int S_FALSE = 1;

            #endregion
        }

        [StructLayout( LayoutKind.Sequential )]
		public sealed class tagOleMenuGroupWidths
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 6 )]
			public int[] widths;
			//public tagOleMenuGroupWidths();
			public tagOleMenuGroupWidths()
			{
				widths = new int[6];
			}
		}
	}
}