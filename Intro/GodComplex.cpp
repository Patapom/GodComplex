//////////////////////////////////////////////////////////////////////////
// Entry Point
//////////////////////////////////////////////////////////////////////////
//
#include "GodComplex.h"

const float4	LUMINANCE = float4( 0.2126f, 0.7152f, 0.0722f, 0.0f );	// D65 illuminant

Device		gs_Device;

#ifdef MUSIC
V2MPlayer	gs_Music;
void*		gs_pMusicPlayerWorkMem;
#endif

WININFO		gs_WindowInfos;


// Extern/undefined CRT shit that needs to be defined to avoid linking to actual CRT
// Useful hints found at http://www.benshoof.org/blog/minicrt/ !
#if !defined(_DEBUG) && !defined(DEBUG_SHADER)
extern "C" int _fltused = 0;
#endif
extern "C" int __cdecl	_purecall(void)		{ return 0; }
double __cdecl			ceil( double _X )	{ return ceilf( float(_X) ); }
 
 
static const char*	pMessageError	= "!IntroInit()!\n\n"\
									"	No DirectX?\n"\
									"	No memory?\n"\
									"	No music?\n";
static const char*	pWindowClass	= "GodComplex";
 
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

static bool	gs_bInitKeys = true;
static U8	gs_pKeysPrevious[256];

static LRESULT CALLBACK WndProc( HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam )
{
	if ( gs_bInitKeys )
	{	// Initialize key states to unpressed
		memset( gs_pKeysPrevious, 0, 256*sizeof(U8) );
		memset( gs_WindowInfos.pKeys, 0, 256*sizeof(U8) );
		memset( gs_WindowInfos.pKeysToggle, 0, 256*sizeof(U8) );
		gs_bInitKeys = false;
	}

	// Ignore screen savers
	if ( uMsg == WM_SYSCOMMAND && (wParam == SC_SCREENSAVE || wParam == SC_MONITORPOWER) )
		return 0;

	if ( uMsg == WM_CLOSE || (uMsg == WM_KEYDOWN && wParam == VK_ESCAPE) )
	{	// Quit on close or escape key
		PostQuitMessage(0);
		return 0 ;
	}

	// Handle standard keys
#ifndef NDEBUG
	if ( uMsg == WM_CHAR )
	{
		int conv = 0;
		switch( wParam )
		{
			case VK_LEFT:		conv = KEY_LEFT;		break;
			case VK_RIGHT:		conv = KEY_RIGHT;		break;
			case VK_UP:			conv = KEY_UP;			break;
			case VK_PRIOR:		conv = KEY_PGUP;		break;
			case VK_NEXT:		conv = KEY_PGDOWN;		break;
			case VK_DOWN:		conv = KEY_DOWN;		break;
			case VK_SPACE:		conv = KEY_SPACE;		break;
			case VK_RSHIFT:		conv = KEY_RSHIFT;		break;
			case VK_RCONTROL:	conv = KEY_RCONTROL;	break;
			case VK_LSHIFT:		conv = KEY_LSHIFT;		break;
			case VK_LCONTROL:	conv = KEY_LCONTROL;	break;
		}
		
		for( int i=KEY_A; i <= KEY_Z; i++ )
		{
			if( wParam==(WPARAM)('A'+i-KEY_A) )
				conv = i;
			if( wParam==(WPARAM)('a'+i-KEY_A) )
				conv = i;
		}

		gs_WindowInfos.Events.Keyboard.Press[conv] = 1;
	}
	if ( uMsg == WM_KEYDOWN || uMsg == WM_KEYUP )
	{
		int		KeyIndex = wParam & 0xFF;
		bool	KeyState = uMsg == WM_KEYDOWN;
		if ( !gs_pKeysPrevious[KeyIndex] && KeyState )
			gs_WindowInfos.pKeysToggle[KeyIndex] ^= true;	// Toggle key
		gs_WindowInfos.pKeys[KeyIndex] = KeyState;
	}
// 	if ( uMsg == WM_LBUTTONDOWN )
// 		gs_WindowInfos.MouseButtons |= 1;
// 	if ( uMsg == WM_MBUTTONDOWN )
// 		gs_WindowInfos.MouseButtons |= 2;
// 	if ( uMsg == WM_RBUTTONDOWN )
// 		gs_WindowInfos.MouseButtons |= 4;
// 	if ( uMsg == WM_LBUTTONUP )
// 		gs_WindowInfos.MouseButtons &= ~1;
// 	if ( uMsg == WM_MBUTTONUP )
// 		gs_WindowInfos.MouseButtons &= ~2;
// 	if ( uMsg == WM_RBUTTONUP )
// 		gs_WindowInfos.MouseButtons &= ~4;
// 	if ( uMsg == WM_MOUSEMOVE )
// 		gs_WindowInfos.MouseX
#endif

	return DefWindowProc( hWnd, uMsg, wParam, lParam );
}

// Return 0 for no error
int	WindowInit()
{
	gs_WindowInfos.hInstance = GetModuleHandle( NULL );

	#ifndef _WIN64
		//////////////////////////////////////////////////////////////////////////
		// Initialize rounding mode once since we disabled fucking _ftol2 link error by adding a deprecated compile flag named /QIfist
		// (always following the advice from http://www.benshoof.org/blog/minicrt/)
		static U16	CW;
		__asm
		{
			fstcw	CW							// store fpu control word  
			mov		dx, word ptr [CW]  
			or		dx, 0x0C00                  // rounding: truncate (default)
			mov		CW, dx 
			fldcw	CW							// load modfied control word  
		}  
	#endif

	//////////////////////////////////////////////////////////////////////////
	// Register the new window class
	WNDCLASSA	wc;
	memset( &wc, 0, sizeof(WNDCLASSA) );
	wc.style		 = CS_OWNDC;
	wc.lpfnWndProc   = WndProc;
	wc.hInstance	 = gs_WindowInfos.hInstance;
	wc.lpszClassName = pWindowClass;

	if( !RegisterClass( (WNDCLASSA*) &wc ) )
		return ERR_REGISTER_CLASS;

	//////////////////////////////////////////////////////////////////////////
	// Create the window
	U32	dwExStyle, dwStyle;
	if ( gs_WindowInfos.bFullscreen )
	{
		if ( ChangeDisplaySettings( &ScreenSettings, CDS_FULLSCREEN ) != DISP_CHANGE_SUCCESSFUL )
			return ERR_CHANGE_DISPLAY_SETTINGS;

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
	AdjustWindowRect( &WindowRect, dwStyle, false );
	gs_WindowInfos.hWnd = CreateWindowEx( dwExStyle, wc.lpszClassName, wc.lpszClassName, dwStyle,
							   (GetSystemMetrics(SM_CXSCREEN)-WindowRect.right+WindowRect.left)>>1,
							   (GetSystemMetrics(SM_CYSCREEN)-WindowRect.bottom+WindowRect.top)>>1,
							   WindowRect.right-WindowRect.left, WindowRect.bottom-WindowRect.top, 0, 0, gs_WindowInfos.hInstance, 0 );
#else
	gs_WindowInfos.hWnd = CreateWindowEx( dwExStyle, wc.lpszClassName, wc.lpszClassName, dwStyle, 0, 0, 
								 WindowRect.right-WindowRect.left, WindowRect.bottom-WindowRect.top, 0, 0, gs_WindowInfos.hInstance, 0 );
#endif
	if( gs_WindowInfos.hWnd == NULL )
		return ERR_CREATE_WINDOW;

	if( (gs_WindowInfos.hDC = GetDC( gs_WindowInfos.hWnd )) == NULL )
		return ERR_RETRIEVE_DC;
	
	SetForegroundWindow( gs_WindowInfos.hWnd );
	SetFocus( gs_WindowInfos.hWnd );


	//////////////////////////////////////////////////////////////////////////
	// Initialize DirectX Device
// 
// 	Video	Test( *((Device*) NULL), gs_WindowInfos.hWnd );
// 
	gs_Device.Init( RESX, RESY, gs_WindowInfos.hWnd, gs_WindowInfos.bFullscreen, true );
	if ( !gs_Device.IsInitialized() )
		return ERR_DX_INIT_DEVICE;	// Oopsy daisy shit fuck hell !


	//////////////////////////////////////////////////////////////////////////
	// Initialize sound player
#ifdef MUSIC
	gs_Music.Init();

	int	WorkMemSize = synthGetSize();
	gs_pMusicPlayerWorkMem = new U8[WorkMemSize];

	U32			TuneSize = 0;
	const U8*	pTheTune = LoadResourceBinary( IDR_MUSIC, "MUSIC", &TuneSize );
	if ( pTheTune == NULL )
		return ERR_MUSIC_RESOURCE_NOT_FOUND;
	if ( !gs_Music.Open( pTheTune ) )
		return ERR_MUSIC_INIT;

	dsInit( gs_Music.RenderProxy, &gs_Music, gs_WindowInfos.hWnd );

// Readback positions
// sS32*	pPositions = NULL;
// U32		PositionsCount = gs_Music.CalcPositions( &pPositions );
// delete[] pPositions;

#endif

	//////////////////////////////////////////////////////////////////////////
	// Initialize other static fields

	return 0;
}

void	WindowExit()
{
	// Kill the music
#ifdef MUSIC
	dsClose();

	delete[] gs_pMusicPlayerWorkMem;
	gs_Music.Close();
#endif

	// Kill the DirectX device
	int	RemainingComponents = gs_Device.ComponentsCount();
	ASSERT( RemainingComponents == 0, "Some DirectX components remain on exit !	Did you forget some deletes ???" );	// This means you forgot to clean up some components ! It's okay since the device is going to clean them up for you, but it's better yet if you know what your doing and take care of your own garbage...
	gs_Device.Exit();

	// Destroy the Windows contexts
	if ( gs_WindowInfos.hDC != NULL )	ReleaseDC( gs_WindowInfos.hWnd, gs_WindowInfos.hDC );
	if ( gs_WindowInfos.hWnd != NULL )	DestroyWindow( gs_WindowInfos.hWnd );

	UnregisterClass( pWindowClass, gs_WindowInfos.hInstance );

	if ( gs_WindowInfos.bFullscreen )
	{
		ChangeDisplaySettings( 0, 0 );
		ShowCursor( 1 ); 
	}
}

#ifndef NDEBUG
void	DrawTime( float t )
{
	static int		frame=0;
	static float	OldTime=0.0;
	static int		fps=0;
	char			str[128];
	int				s, m, c;

	if ( t < 0.0f )
		return;

	if ( gs_WindowInfos.bFullscreen )
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
		m = int( floorf( t / 60.0f ) );
		s = int( floorf( t - 60.0f * m ) );
		c = int( floorf( t * 100.0f ) ) % 100;
		float	ms = 1000.0f / fps;

		sprintf_s( str, 128, "%s %02d:%02d:%02d  [%d fps] [%4.4f ms] (!DEBUG WIP VERSION!)", pWindowClass, m, s, c, fps, ms );
		SetWindowText( gs_WindowInfos.hWnd, str );
	}
}

void	HandleEvents()
{
	BOOL	bHasFocus = GetFocus() == gs_WindowInfos.hWnd;

	gs_WindowInfos.Events.Keyboard.State[KEY_LEFT]     = bHasFocus && GetAsyncKeyState( VK_LEFT );
	gs_WindowInfos.Events.Keyboard.State[KEY_RIGHT]    = bHasFocus && GetAsyncKeyState( VK_RIGHT );
	gs_WindowInfos.Events.Keyboard.State[KEY_UP]       = bHasFocus && GetAsyncKeyState( VK_UP );
	gs_WindowInfos.Events.Keyboard.State[KEY_PGUP]     = bHasFocus && GetAsyncKeyState( VK_PRIOR );
	gs_WindowInfos.Events.Keyboard.State[KEY_PGDOWN]   = bHasFocus && GetAsyncKeyState( VK_NEXT );
	gs_WindowInfos.Events.Keyboard.State[KEY_DOWN]     = bHasFocus && GetAsyncKeyState( VK_DOWN );
	gs_WindowInfos.Events.Keyboard.State[KEY_SPACE]    = bHasFocus && GetAsyncKeyState( VK_SPACE );
	gs_WindowInfos.Events.Keyboard.State[KEY_RSHIFT]   = bHasFocus && GetAsyncKeyState( VK_RSHIFT );
	gs_WindowInfos.Events.Keyboard.State[KEY_RCONTROL] = bHasFocus && GetAsyncKeyState( VK_RCONTROL );
	gs_WindowInfos.Events.Keyboard.State[KEY_LSHIFT]   = bHasFocus && GetAsyncKeyState( VK_LSHIFT );
	gs_WindowInfos.Events.Keyboard.State[KEY_LCONTROL] = bHasFocus && GetAsyncKeyState( VK_LCONTROL );
	gs_WindowInfos.Events.Keyboard.State[KEY_1]        = bHasFocus && GetAsyncKeyState( '1' );
	gs_WindowInfos.Events.Keyboard.State[KEY_2]        = bHasFocus && GetAsyncKeyState( '2' );
	gs_WindowInfos.Events.Keyboard.State[KEY_3]        = bHasFocus && GetAsyncKeyState( '3' );
	gs_WindowInfos.Events.Keyboard.State[KEY_4]        = bHasFocus && GetAsyncKeyState( '4' );
	gs_WindowInfos.Events.Keyboard.State[KEY_5]        = bHasFocus && GetAsyncKeyState( '5' );
	gs_WindowInfos.Events.Keyboard.State[KEY_6]        = bHasFocus && GetAsyncKeyState( '6' );
	gs_WindowInfos.Events.Keyboard.State[KEY_7]        = bHasFocus && GetAsyncKeyState( '7' );
	gs_WindowInfos.Events.Keyboard.State[KEY_8]        = bHasFocus && GetAsyncKeyState( '8' );
	gs_WindowInfos.Events.Keyboard.State[KEY_9]        = bHasFocus && GetAsyncKeyState( '9' );
	gs_WindowInfos.Events.Keyboard.State[KEY_0]        = bHasFocus && GetAsyncKeyState( '0' );
	for( int i=KEY_A; i<=KEY_Z; i++ )
		gs_WindowInfos.Events.Keyboard.State[i] = bHasFocus && GetAsyncKeyState( 'A'+i-KEY_A );

	// Handle mouse events
	POINT	p;
	GetCursorPos( &p );

	gs_WindowInfos.Events.Mouse.ox = gs_WindowInfos.Events.Mouse.x;
	gs_WindowInfos.Events.Mouse.oy = gs_WindowInfos.Events.Mouse.y;
	gs_WindowInfos.Events.Mouse.x  = p.x;
	gs_WindowInfos.Events.Mouse.y  = p.y;
	gs_WindowInfos.Events.Mouse.dx = gs_WindowInfos.Events.Mouse.x - gs_WindowInfos.Events.Mouse.ox;
	gs_WindowInfos.Events.Mouse.dy = gs_WindowInfos.Events.Mouse.y - gs_WindowInfos.Events.Mouse.oy;

	gs_WindowInfos.Events.Mouse.obuttons[0] = gs_WindowInfos.Events.Mouse.buttons[0];
	gs_WindowInfos.Events.Mouse.obuttons[1] = gs_WindowInfos.Events.Mouse.buttons[1];
	gs_WindowInfos.Events.Mouse.obuttons[2] = gs_WindowInfos.Events.Mouse.buttons[2];
	gs_WindowInfos.Events.Mouse.buttons[0] = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0 && bHasFocus;
	gs_WindowInfos.Events.Mouse.buttons[1] = (GetAsyncKeyState(VK_MBUTTON) & 0x8000) != 0 && bHasFocus;
	gs_WindowInfos.Events.Mouse.buttons[2] = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0 && bHasFocus;

	gs_WindowInfos.Events.Mouse.dbuttons[0] = gs_WindowInfos.Events.Mouse.buttons[0] - gs_WindowInfos.Events.Mouse.obuttons[0];
	gs_WindowInfos.Events.Mouse.dbuttons[1] = gs_WindowInfos.Events.Mouse.buttons[1] - gs_WindowInfos.Events.Mouse.obuttons[1];
	gs_WindowInfos.Events.Mouse.dbuttons[2] = gs_WindowInfos.Events.Mouse.buttons[2] - gs_WindowInfos.Events.Mouse.obuttons[2];
}

void	print( const char* _pText, ... )
{
	va_list	argptr;
	va_start( argptr, _pText );
	char	pTemp[4096];
	sprintf_s( pTemp, 4096, _pText, argptr );
	va_end( argptr );

	OutputDebugString( pTemp );
}
#else
void	print( const char* _pText, ... )	{}	// Empty in release mode
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
	TextOut( _pInfos->hDC, (RESX-318)>>1, (RESY-38)>>1, "Precomputing...", 21 );

	// Draw bar
	SelectObject( _pInfos->hDC, CreateSolidBrush(0x00705030) );
	Rectangle( _pInfos->hDC, xo, yo, (228*RESX) >> 8, y1 );
	SelectObject( _pInfos->hDC, CreateSolidBrush(0x00f0d0b0) );
	Rectangle( _pInfos->hDC, xo, yo, ((28 + (_Progress<<1))*RESX) >> 8, y1 );
}


//////////////////////////////////////////////////////////////////////////
// Entry point
#if defined(_DEBUG) || defined(DEBUG_SHADER)
int WINAPI	WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow )
#else
void WINAPI	EntryPoint()	
#endif
{

#ifdef ALLOW_WINDOWED
//	gs_WindowInfos.bFullscreen = MessageBox( 0, "Fullscreen?", pWindowClass, MB_YESNO | MB_ICONQUESTION ) == IDYES;
	gs_WindowInfos.bFullscreen = false;
#endif

	int	ErrorCode = 0;
	if ( (ErrorCode = WindowInit()) )
	{
		WindowExit();
		MessageBox( 0, pMessageError, 0, MB_OK | MB_ICONEXCLAMATION );
		ExitProcess( ErrorCode );
	}

	IntroProgressDelegate	Progress = { &gs_WindowInfos, ShowProgress };
	if ( (ErrorCode = IntroInit( Progress )) )
	{
		WindowExit();
		MessageBox( 0, pMessageError, 0, MB_OK | MB_ICONEXCLAMATION );
		ExitProcess( -ErrorCode );
	}

	// Start the music
#ifdef MUSIC
	gs_Music.Play();
#endif

	//////////////////////////////////////////////////////////////////////////
	// Run the message loop !
	bool	bFinished = false;
    float	StartTime = 0.001f * timeGetTime(); 
	float	LastTime = 0.0f;

	while ( !bFinished )
	{
		float	Time = 0.001f * timeGetTime() - StartTime;
	    float	DeltaTime = Time - LastTime;
		LastTime = Time;

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

#ifndef NDEBUG
		// Show FPS
		DrawTime( Time );
		HandleEvents();
#endif

#ifdef SURE_DEBUG
		// Check for hash collisions => We must never have too many of them !
		ASSERT( DictionaryU32::ms_MaxCollisionsCount < 2, "Too many collisions in hash tables! Either increase size or use different hashing scheme!" );

		// Reload in-file constants
		ReloadChangedTweakableValues();

		// Reload modified shaders
		WatchIncludesModifications();
		Shader::WatchShadersModifications();
		ComputeShader::WatchShadersModifications();
#endif

		// Run the intro
		bFinished |= !IntroDo( Time, DeltaTime );

// This was in iQ's framework, I don't know what it's for. I believe it's useful when using OpenGL but with DirectX it makes everything slow as hell (attempts to load/unload DLLs every frame) !
// I left it here so everyone knows it must NOT be called...
//		SwapBuffers( gs_WindowInfos.hDC );
	}
	//
	//////////////////////////////////////////////////////////////////////////

	// Stop the music
#ifdef MUSIC
	gs_Music.Stop();
#endif

 	IntroExit();

	WindowExit();

	// Clean exit...
	ExitProcess( 0 );
}

