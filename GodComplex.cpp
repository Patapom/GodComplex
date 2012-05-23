//////////////////////////////////////////////////////////////////////////
// Entry Point
//////////////////////////////////////////////////////////////////////////
//
#include "GodComplex.h"

// Extern/undefined CRT shit that needs to be defined to avoid linking to actual CRT
// Useful hints found at http://www.benshoof.org/blog/minicrt/ !
#ifndef _DEBUG
extern "C" int _fltused = 0;
#endif
extern "C" int __cdecl _purecall(void) { return 0; }
double __cdecl ceil( double _X )	{ return ASM_ceilf( float(_X) ); }


static const char*	pMessageError	= "intro_init()!\n\n"\
									"  no memory?\n"\
									"  no music?\n"\
									"  no shaders?";
static const char*	pWindowClass	= "god_complex";

static DEVMODE	ScreenSettings =
{	{0},
#if _MSC_VER < 1400
	0,0,148,0,0x001c0000,{0},0,0,0,0,0,0,0,0,0,{0},0,32,RESX,RESY,0,0,	// Visual C++ 6.0
#else
	0,0,156,0,0x001c0000,{0},0,0,0,0,0,{0},0,32,RESX,RESY,{0}, 0,		// Visual Studio 2005
#endif

#if(WINVER >= 0x0400)
	0,0,0,0,0,0,
#if (WINVER >= 0x0500) || (_WIN32_WINNT >= 0x0400)
	0,0
#endif
#endif
};


static LRESULT CALLBACK WndProc( HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam )
{
// 	// Ignore screen savers
// 	if ( uMsg == WM_SYSCOMMAND && (wParam == SC_SCREENSAVE || wParam == SC_MONITORPOWER) )
// 		return 0;
// 
// 	if ( uMsg == WM_CLOSE || (uMsg == WM_KEYDOWN && wParam == VK_ESCAPE) )
// 	{	// Quit on close or escape key
// 		PostQuitMessage(0);
// 		return 0 ;
// 	}
// 
// 	// Handle standard keys
// #ifdef _DEBUG
// 	if ( uMsg == WM_CHAR )
// 	{
// 		int conv = 0;
// 		switch( wParam )
// 		{
// 			case VK_LEFT:		conv = KEY_LEFT;		break;
// 			case VK_RIGHT:		conv = KEY_RIGHT;		break;
// 			case VK_UP:			conv = KEY_UP;			break;
// 			case VK_PRIOR:		conv = KEY_PGUP;		break;
// 			case VK_NEXT:		conv = KEY_PGDOWN;		break;
// 			case VK_DOWN:		conv = KEY_DOWN;		break;
// 			case VK_SPACE:		conv = KEY_SPACE;		break;
// 			case VK_RSHIFT:		conv = KEY_RSHIFT;		break;
// 			case VK_RCONTROL:	conv = KEY_RCONTROL;	break;
// 			case VK_LSHIFT:		conv = KEY_LSHIFT;		break;
// 			case VK_LCONTROL:	conv = KEY_LCONTROL;	break;
// 		}
// 		
// 		for( int i=KEY_A; i <= KEY_Z; i++ )
// 		{
// 			if( wParam==(WPARAM)('A'+i-KEY_A) )
// 				conv = i;
// 			if( wParam==(WPARAM)('a'+i-KEY_A) )
// 				conv = i;
// 		}
// 
// 		WindowInfos.Events.Keyboard.Press[conv] = 1;
// 	}
// #endif

	return DefWindowProc( hWnd, uMsg, wParam, lParam );
}

void	WindowExit()
{
	// Kill the music
	dsClose();
	gs_Music.Close();

	// Kill the DirectX device
	gs_Device.Exit();

	// Destroy the Windows contexts
	if ( WindowInfos.hDC != NULL )	ReleaseDC( WindowInfos.hWnd, WindowInfos.hDC );
	if ( WindowInfos.hWnd != NULL )	DestroyWindow( WindowInfos.hWnd );

	UnregisterClass( pWindowClass, WindowInfos.hInstance );

	if ( WindowInfos.bFullscreen )
	{
		ChangeDisplaySettings( 0, 0 );
		ShowCursor( 1 ); 
	}
}

bool	WindowInit()
{
	WindowInfos.hInstance = GetModuleHandle( 0 );

	//////////////////////////////////////////////////////////////////////////
	// Register the new window class
	WNDCLASSA	wc;
	ASM_memset( &wc, 0, sizeof(WNDCLASSA) );
	wc.style		 = CS_OWNDC;
	wc.lpfnWndProc   = WndProc;
	wc.hInstance	 = WindowInfos.hInstance;
	wc.lpszClassName = pWindowClass;

	if( !RegisterClass( (WNDCLASSA*) &wc ) )
		return false;	// Failed !

	//////////////////////////////////////////////////////////////////////////
	// Create the window
	U32	dwExStyle, dwStyle;
	if ( WindowInfos.bFullscreen )
	{
		if ( ChangeDisplaySettings( &ScreenSettings, CDS_FULLSCREEN ) != DISP_CHANGE_SUCCESSFUL )
			return false;

		dwExStyle = WS_EX_APPWINDOW;
		dwStyle   = WS_VISIBLE | WS_POPUP | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
		ShowCursor( 0 );
	}
	else
	{
		dwExStyle = WS_EX_APPWINDOW;// | WS_EX_WINDOWEDGE;
		dwStyle   = WS_VISIBLE | WS_CAPTION | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | WS_SYSMENU;
	}

	RECT	WindowRect;
	WindowRect.left		= 0;
	WindowRect.top		= 0;
	WindowRect.right	= RESX;
	WindowRect.bottom	= RESY;

#ifdef ALLOW_WINDOWED
	AdjustWindowRect( &WindowRect, dwStyle, 0 );
	WindowInfos.hWnd = CreateWindowEx( dwExStyle, wc.lpszClassName, wc.lpszClassName, dwStyle,
							   (GetSystemMetrics(SM_CXSCREEN)-WindowRect.right+WindowRect.left)>>1,
							   (GetSystemMetrics(SM_CYSCREEN)-WindowRect.bottom+WindowRect.top)>>1,
							   WindowRect.right-WindowRect.left, WindowRect.bottom-WindowRect.top, 0, 0, WindowInfos.hInstance, 0 );
#else
	WindowInfos.hWnd = CreateWindowEx( dwExStyle, wc.lpszClassName, wc.lpszClassName, dwStyle, 0, 0, 
								 WindowRect.right-WindowRect.left, WindowRect.bottom-WindowRect.top, 0, 0, WindowInfos.hInstance, 0 );
#endif
	if( WindowInfos.hWnd == NULL )
		return false;	// Failed OldTime create the window !

	if( (WindowInfos.hDC = GetDC( WindowInfos.hWnd )) == NULL )
		return false;	// Failed OldTime retrieve a valid device context for GDI calls
	
	SetForegroundWindow( WindowInfos.hWnd );
	SetFocus( WindowInfos.hWnd );

	//////////////////////////////////////////////////////////////////////////
	// Initialize DirectX Device
	gs_Device.Init( RESX, RESY, WindowInfos.hWnd, WindowInfos.bFullscreen, true );

	//////////////////////////////////////////////////////////////////////////
	// Initialize sound player
	gs_Music.Init();

	U32			TuneSize = 0;
	const U8*	pTheTune = LoadResourceBinary( IDR_MUSIC, "MUSIC", &TuneSize );
	gs_Music.Open( pTheTune );

	dsInit( gs_Music.RenderProxy, &gs_Music, WindowInfos.hWnd );

	return true;
}

static void DrawTime( float t )
{
	static int		frame=0;
	static float	OldTime=0.0;
	static int		fps=0;
	char			str[64];
	int				s, m, c;

	if ( t < 0.0f )
		return;

	if ( WindowInfos.bFullscreen )
		return;

	frame++;
	if ( (t-OldTime) > 1.0f )
	{
		fps = frame;
		OldTime = t;
		frame = 0;
	}

	if ( !(frame&3) )
	{
		m = ASM_floorf( t / 60.0f );
		s = ASM_floorf( t - 60.0f * m );
		c = ASM_floorf( t * 100.0f ) % 100;
		sprintf( str, "%02d:%02d:%02d  [%d fps]", m, s, c, fps );
		SetWindowText( WindowInfos.hWnd, str );
	}
}

#ifdef _DEBUG
void	HandleEvents()
{
	WindowInfos.Events.Keyboard.State[KEY_LEFT]     = GetAsyncKeyState( VK_LEFT );
	WindowInfos.Events.Keyboard.State[KEY_RIGHT]    = GetAsyncKeyState( VK_RIGHT );
	WindowInfos.Events.Keyboard.State[KEY_UP]       = GetAsyncKeyState( VK_UP );
    WindowInfos.Events.Keyboard.State[KEY_PGUP]     = GetAsyncKeyState( VK_PRIOR );
    WindowInfos.Events.Keyboard.State[KEY_PGDOWN]   = GetAsyncKeyState( VK_NEXT );
	WindowInfos.Events.Keyboard.State[KEY_DOWN]     = GetAsyncKeyState( VK_DOWN );
	WindowInfos.Events.Keyboard.State[KEY_SPACE]    = GetAsyncKeyState( VK_SPACE );
	WindowInfos.Events.Keyboard.State[KEY_RSHIFT]   = GetAsyncKeyState( VK_RSHIFT );
	WindowInfos.Events.Keyboard.State[KEY_RCONTROL] = GetAsyncKeyState( VK_RCONTROL );
	WindowInfos.Events.Keyboard.State[KEY_LSHIFT]   = GetAsyncKeyState( VK_LSHIFT );
	WindowInfos.Events.Keyboard.State[KEY_LCONTROL] = GetAsyncKeyState( VK_LCONTROL );
	WindowInfos.Events.Keyboard.State[KEY_1]        = GetAsyncKeyState( '1' );
	WindowInfos.Events.Keyboard.State[KEY_2]        = GetAsyncKeyState( '2' );
	WindowInfos.Events.Keyboard.State[KEY_3]        = GetAsyncKeyState( '3' );
	WindowInfos.Events.Keyboard.State[KEY_4]        = GetAsyncKeyState( '4' );
	WindowInfos.Events.Keyboard.State[KEY_5]        = GetAsyncKeyState( '5' );
	WindowInfos.Events.Keyboard.State[KEY_6]        = GetAsyncKeyState( '6' );
	WindowInfos.Events.Keyboard.State[KEY_7]        = GetAsyncKeyState( '7' );
	WindowInfos.Events.Keyboard.State[KEY_8]        = GetAsyncKeyState( '8' );
	WindowInfos.Events.Keyboard.State[KEY_9]        = GetAsyncKeyState( '9' );
	WindowInfos.Events.Keyboard.State[KEY_0]        = GetAsyncKeyState( '0' );
    for( int i=KEY_A; i<=KEY_Z; i++ )
	    WindowInfos.Events.Keyboard.State[i] = GetAsyncKeyState( 'A'+i-KEY_A );

	// Handle mouse events
	POINT	p;
	GetCursorPos( &p );

	WindowInfos.Events.Mouse.ox = WindowInfos.Events.Mouse.x;
	WindowInfos.Events.Mouse.oy = WindowInfos.Events.Mouse.y;
	WindowInfos.Events.Mouse.x  = p.x;
	WindowInfos.Events.Mouse.y  = p.y;
	WindowInfos.Events.Mouse.dx = WindowInfos.Events.Mouse.x - WindowInfos.Events.Mouse.ox;
	WindowInfos.Events.Mouse.dy = WindowInfos.Events.Mouse.y - WindowInfos.Events.Mouse.oy;

	WindowInfos.Events.Mouse.obuttons[0] = WindowInfos.Events.Mouse.buttons[0];
	WindowInfos.Events.Mouse.obuttons[1] = WindowInfos.Events.Mouse.buttons[1];
	WindowInfos.Events.Mouse.buttons[0] = GetAsyncKeyState(VK_LBUTTON);
	WindowInfos.Events.Mouse.buttons[1] = GetAsyncKeyState(VK_RBUTTON);

	WindowInfos.Events.Mouse.dbuttons[0] = WindowInfos.Events.Mouse.buttons[0] - WindowInfos.Events.Mouse.obuttons[0];
	WindowInfos.Events.Mouse.dbuttons[1] = WindowInfos.Events.Mouse.buttons[1] - WindowInfos.Events.Mouse.obuttons[1];
}
#endif


//////////////////////////////////////////////////////////////////////////
// Progress Bar Display
//	_Progress = [0..100]
static void	ShowProgress( WININFO* _pInfos, int _Progress )
{
    const int	xo = (( 28*RESX)>>8);
    const int	y1 = ((200*RESY)>>8);
    const int	yo = y1-8;

    // Draw background
    SelectObject( _pInfos->hDC, CreateSolidBrush(0x0045302c) );
    Rectangle( _pInfos->hDC, 0, 0, RESX, RESY );

    // Draw text
    SetBkMode( _pInfos->hDC, TRANSPARENT );
    SetTextColor( _pInfos->hDC, 0x00ffffff );
    SelectObject( _pInfos->hDC, CreateFont( 44,0,0,0,0,0,0,0,0,0,0,ANTIALIASED_QUALITY,0,"arial") );
    TextOut( _pInfos->hDC, (RESX-318)>>1, (RESY-38)>>1, "wait while loading...", 21 );

    // Draw bar
    SelectObject( _pInfos->hDC, CreateSolidBrush(0x00705030) );
    Rectangle( _pInfos->hDC, xo, yo, (228*RESX) >> 8, y1 );
    SelectObject( _pInfos->hDC, CreateSolidBrush(0x00f0d0b0) );
    Rectangle( _pInfos->hDC, xo, yo, ((28 + (_Progress<<1))*RESX) >> 8, y1 );
}


//////////////////////////////////////////////////////////////////////////
// Entry point
#ifdef _DEBUG
int WINAPI WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
#else
void WINAPI	EntryPoint()	
#endif
{
#ifdef ALLOW_WINDOWED
//	WindowInfos.bFullscreen = MessageBox( 0, "Fullscreen?", pWindowClass, MB_YESNO | MB_ICONQUESTION ) == IDYES;
	WindowInfos.bFullscreen = false;
#endif

	if ( !WindowInit() )
	{
		WindowExit();
		MessageBox( 0, pMessageError, 0, MB_OK | MB_ICONEXCLAMATION );
		ExitProcess( -1 );
	}

	IntroProgressDelegate	Progress = { &WindowInfos, ShowProgress };
	if ( !IntroInit( Progress ) )
	{
		WindowExit();
		MessageBox( 0, pMessageError, 0, MB_OK | MB_ICONEXCLAMATION );
		ExitProcess( -2 );
	}

	// Start the music
	gs_Music.Play();

	//////////////////////////////////////////////////////////////////////////
	// Run the message loop !
	bool	bFinished = false;
	while ( !bFinished )
	{
		// Process Windows messages
		MSG		msg;
		while ( PeekMessage( &msg, 0, 0, 0, PM_REMOVE ) )
		{
			if ( msg.message == WM_QUIT )
			{
				bFinished = true;
				break;
			}
			DispatchMessage( &msg );
		}

#ifdef _DEBUG
		HandleEvents();

		// Show FPS
        static long	OldTime = 0; if( !OldTime ) OldTime=timeGetTime(); 
        float	DeltaTime = 0.001f * (float) (timeGetTime() - OldTime);
		DrawTime( DeltaTime );
#endif

		// Run the intro
		bFinished |= IntroDo();

		SwapBuffers( WindowInfos.hDC );
	}

	// Stop the music
	gs_Music.Stop();

	IntroExit();

	WindowExit();

	// Clean exit...
	ExitProcess( 0 );
}

