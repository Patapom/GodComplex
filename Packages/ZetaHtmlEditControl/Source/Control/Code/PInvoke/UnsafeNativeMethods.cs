namespace ZetaHtmlEditControl.Code.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    public sealed class UnsafeNativeMethods
	{
        [DllImport( @"user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
        public static extern int MapWindowPoints( HandleRef hWndFrom, HandleRef hWndTo, [In, Out] NativeMethods.POINT pt, int cPoints );

        [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
			Guid( @"3050f3f0-98b5-11cf-bb82-00aa00bdce0b" )]
		internal interface ICustomDoc
		{
			[PreserveSig]
			void SetUIHandler( IDocHostUIHandler pUIHandler );
		}

        [ComImport, Guid( @"BD3F23C0-D43E-11CF-893B-00AA00BDCE1A" ), ComVisible( true ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
        public interface IDocHostUIHandler
        {
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int ShowContextMenu( [In, MarshalAs( UnmanagedType.U4 )] int dwID, [In] NativeMethods.POINT pt, [In, MarshalAs( UnmanagedType.Interface )] object pcmdtReserved, [In, MarshalAs( UnmanagedType.Interface )] object pdispReserved );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int GetHostInfo( [In, Out] NativeMethods.DOCHOSTUIINFO info );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int ShowUI( [In, MarshalAs( UnmanagedType.I4 )] int dwID, [In] IOleInPlaceActiveObject activeObject, [In] NativeMethods.IOleCommandTarget commandTarget, [In] IOleInPlaceFrame frame, [In] IOleInPlaceUIWindow doc );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int HideUI();
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int UpdateUI();
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int EnableModeless( [In, MarshalAs( UnmanagedType.Bool )] bool fEnable );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int OnDocWindowActivate( [In, MarshalAs( UnmanagedType.Bool )] bool fActivate );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int OnFrameWindowActivate( [In, MarshalAs( UnmanagedType.Bool )] bool fActivate );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int ResizeBorder( [In] NativeMethods.COMRECT rect, [In] IOleInPlaceUIWindow doc, bool fFrameWindow );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int TranslateAccelerator( [In] ref NativeMethods.MSG msg, [In] ref Guid group, [In, MarshalAs( UnmanagedType.I4 )] int nCmdID );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int GetOptionKeyPath( [Out, MarshalAs( UnmanagedType.LPArray )] string[] pbstrKey, [In, MarshalAs( UnmanagedType.U4 )] int dw );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int GetDropTarget( [In, MarshalAs( UnmanagedType.Interface )] IOleDropTarget pDropTarget, [MarshalAs( UnmanagedType.Interface )] out IOleDropTarget ppDropTarget );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int GetExternal( [MarshalAs( UnmanagedType.Interface )] out object ppDispatch );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int TranslateUrl( [In, MarshalAs( UnmanagedType.U4 )] int dwTranslate, [In, MarshalAs( UnmanagedType.LPWStr )] string strURLIn, [MarshalAs( UnmanagedType.LPWStr )] out string pstrURLOut );
            [return: MarshalAs( UnmanagedType.I4 )]
            [PreserveSig]
            int FilterDataObject( IDataObject pDO, out IDataObject ppDORet );
        }

        [ComImport, ComVisible(true)]
        [Guid(@"00000118-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleClientSite
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int SaveObject();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetMoniker(
                [In, MarshalAs(UnmanagedType.U4)]         uint dwAssign,
                [In, MarshalAs(UnmanagedType.U4)]         uint dwWhichMoniker,
                [Out, MarshalAs(UnmanagedType.Interface)] out IMoniker ppmk);

            //[return: MarshalAs(UnmanagedType.I4)]
            //[PreserveSig]
            //int GetContainer(
            //    [Out, MarshalAs(UnmanagedType.Interface)] out IOleContainer ppContainer);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowObject();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnShowWindow([In, MarshalAs(UnmanagedType.Bool)] bool fShow);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int RequestNewObjectLayout();
        }


        /*	[Serializable]
			public enum UnmanagedType
			{
				// Fields
				AnsiBStr = 0x23,
				AsAny = 40,
				Bool = 2,
				BStr = 0x13,
				ByValArray = 30,
				ByValTStr = 0x17,
				Currency = 15,
				CustomMarshaler = 0x2c,
				Error = 0x2d,
				FunctionPtr = 0x26,
				I1 = 3,
				I2 = 5,
				I4 = 7,
				I8 = 9,
				IDispatch = 0x1a,
				Interface = 0x1c,
				IUnknown = 0x19,
				LPArray = 0x2a,
				LPStr = 20,
				LPStruct = 0x2b,
				LPTStr = 0x16,
				LPWStr = 0x15,
				R4 = 11,
				R8 = 12,
				SafeArray = 0x1d,
				Struct = 0x1b,
				SysInt = 0x1f,
				SysUInt = 0x20,
				TBStr = 0x24,
				U1 = 4,
				U2 = 6,
				U4 = 8,
				U8 = 10,
				VariantBool = 0x25,
				VBByRefStr = 0x22
			}
			*/
		[ComImport, Guid( @"00000122-0000-0000-C000-000000000046" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IOleDropTarget
		{
			[PreserveSig]
			int OleDragEnter( [In, MarshalAs( UnmanagedType.Interface )] object pDataObj, [In, MarshalAs( UnmanagedType.U4 )] int grfKeyState, [In, MarshalAs( UnmanagedType.U8 )] long pt, [In, Out] ref int pdwEffect );
			[PreserveSig]
			int OleDragOver( [In, MarshalAs( UnmanagedType.U4 )] int grfKeyState, [In, MarshalAs( UnmanagedType.U8 )] long pt, [In, Out] ref int pdwEffect );
			[PreserveSig]
			int OleDragLeave();
			[PreserveSig]
			int OleDrop( [In, MarshalAs( UnmanagedType.Interface )] object pDataObj, [In, MarshalAs( UnmanagedType.U4 )] int grfKeyState, [In, MarshalAs( UnmanagedType.U8 )] long pt, [In, Out] ref int pdwEffect );
		}

        [ComImport, Guid( @"00000117-0000-0000-C000-000000000046" ), SuppressUnmanagedCodeSecurity, InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
        public interface IOleInPlaceActiveObject
        {
            [PreserveSig]
            int GetWindow( out IntPtr hwnd );
            void ContextSensitiveHelp( int fEnterMode );
            [PreserveSig]
            int TranslateAccelerator( [In] ref NativeMethods.MSG lpmsg );
            void OnFrameWindowActivate( bool fActivate );
            void OnDocWindowActivate( int fActivate );
            void ResizeBorder( [In] NativeMethods.COMRECT prcBorder, [In] IOleInPlaceUIWindow pUIWindow, bool fFrameWindow );
            void EnableModeless( int fEnable );
        }

        [ComImport, Guid( @"00000116-0000-0000-C000-000000000046" ), InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
		public interface IOleInPlaceFrame
		{
			IntPtr GetWindow();
			[PreserveSig]
			int ContextSensitiveHelp( int fEnterMode );
			[PreserveSig]
			int GetBorder( [Out] NativeMethods.COMRECT lprectBorder );
			[PreserveSig]
			int RequestBorderSpace( [In] NativeMethods.COMRECT pborderwidths );
			[PreserveSig]
			int SetBorderSpace( [In] NativeMethods.COMRECT pborderwidths );
			[PreserveSig]
			int SetActiveObject( [In, MarshalAs( UnmanagedType.Interface )] IOleInPlaceActiveObject pActiveObject, [In, MarshalAs( UnmanagedType.LPWStr )] string pszObjName );
			[PreserveSig]
			int InsertMenus( [In] IntPtr hmenuShared, [In, Out] NativeMethods.tagOleMenuGroupWidths lpMenuWidths );
			[PreserveSig]
			int SetMenu( [In] IntPtr hmenuShared, [In] IntPtr holemenu, [In] IntPtr hwndActiveObject );
			[PreserveSig]
			int RemoveMenus( [In] IntPtr hmenuShared );
			[PreserveSig]
			int SetStatusText( [In, MarshalAs( UnmanagedType.LPWStr )] string pszStatusText );
			[PreserveSig]
			int EnableModeless( bool fEnable );
			[PreserveSig]
			int TranslateAccelerator( [In] ref NativeMethods.MSG lpmsg, [In, MarshalAs( UnmanagedType.U2 )] short wID );
		}

        [ComImport, InterfaceType( ComInterfaceType.InterfaceIsIUnknown ), Guid( @"00000115-0000-0000-C000-000000000046" )]
        public interface IOleInPlaceUIWindow
        {
            IntPtr GetWindow();
            [PreserveSig]
            int ContextSensitiveHelp( int fEnterMode );
            [PreserveSig]
            int GetBorder( [Out] NativeMethods.COMRECT lprectBorder );
            [PreserveSig]
            int RequestBorderSpace( [In] NativeMethods.COMRECT pborderwidths );
            [PreserveSig]
            int SetBorderSpace( [In] NativeMethods.COMRECT pborderwidths );
            void SetActiveObject( [In, MarshalAs( UnmanagedType.Interface )] IOleInPlaceActiveObject pActiveObject, [In, MarshalAs( UnmanagedType.LPWStr )] string pszObjName );
        }

        [ComImport, ComVisible(true)]
        [Guid(@"00000112-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleObject
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int SetClientSite(
                [In, MarshalAs(UnmanagedType.Interface)] IOleClientSite pClientSite);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetClientSite(
                [Out, MarshalAs(UnmanagedType.Interface)] out IOleClientSite site);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int SetHostNames(
                [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerApp,
                [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerObj);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Close([In, MarshalAs(UnmanagedType.U4)] uint dwSaveOption);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int SetMoniker(
                [In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker,
                [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmk);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetMoniker(
                [In, MarshalAs(UnmanagedType.U4)] uint dwAssign,
                [In, MarshalAs(UnmanagedType.U4)] uint dwWhichMoniker,
                [Out, MarshalAs(UnmanagedType.Interface)] out IMoniker moniker);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int InitFromData(
                [In, MarshalAs(UnmanagedType.Interface)] IDataObject pDataObject,
                [In, MarshalAs(UnmanagedType.Bool)] bool fCreation,
                [In, MarshalAs(UnmanagedType.U4)] uint dwReserved);

            int GetClipboardData(
                [In, MarshalAs(UnmanagedType.U4)] uint dwReserved,
                [Out, MarshalAs(UnmanagedType.Interface)] out IDataObject data);

            //[return: MarshalAs(UnmanagedType.I4)]
            //[PreserveSig]
            //int DoVerb(
            //    [In, MarshalAs(UnmanagedType.I4)] int iVerb,
            //    [In, MarshalAs(UnmanagedType.Struct)] ref tagMSG lpmsg,
            //    //or [In] IntPtr lpmsg,
            //    [In, MarshalAs(UnmanagedType.Interface)] IOleClientSite pActiveSite,
            //    [In, MarshalAs(UnmanagedType.I4)] int lindex,
            //    [In] IntPtr hwndParent,
            //    [In, MarshalAs(UnmanagedType.Struct)] ref tagRECT lprcPosRect);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int EnumVerbs([Out, MarshalAs(UnmanagedType.Interface)] out Object e);
            //int EnumVerbs(out IEnumOLEVERB e);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OleUpdate();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int IsUpToDate();

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetUserClassID([In, Out] ref Guid pClsid);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetUserType(
                [In, MarshalAs(UnmanagedType.U4)] uint dwFormOfType,
                [Out, MarshalAs(UnmanagedType.LPWStr)] out string userType);

            //[return: MarshalAs(UnmanagedType.I4)]
            //[PreserveSig]
            //int SetExtent(
            //    [In, MarshalAs(UnmanagedType.U4)] uint dwDrawAspect,
            //    [In, MarshalAs(UnmanagedType.Struct)] ref tagSIZEL pSizel);

            //[return: MarshalAs(UnmanagedType.I4)]
            //[PreserveSig]
            //int GetExtent(
            //    [In, MarshalAs(UnmanagedType.U4)] uint dwDrawAspect,
            //    [In, Out, MarshalAs(UnmanagedType.Struct)] ref tagSIZEL pSizel);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Advise(
                [In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink,
                out int cookie);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Unadvise(
                [In, MarshalAs(UnmanagedType.U4)] uint dwConnection);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int EnumAdvise(out IEnumSTATDATA e);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetMiscStatus(
                [In, MarshalAs(UnmanagedType.U4)] uint dwAspect,
                out int misc);

            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int SetColorScheme([In, MarshalAs(UnmanagedType.Struct)] ref object pLogpal);
        }

        [ComImport, ComVisible(true)]
        [Guid(@"6d5140c1-7436-11ce-8034-00aa006009fa")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryService(
                [In] ref Guid guidService,
                [In] ref Guid riid,
                [Out] out IntPtr ppvObject);
            //This does not work i.e.-> ppvObject = (INewWindowManager)this
            //[Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);
        }
	}
}
