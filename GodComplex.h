//////////////////////////////////////////////////////////////////////////
// Main include for the framework
//
// NOTE: Many routines were "borrowed" (rather violently stolen) from iQ's framework
//
#pragma once

#define RESX	1280	// 720p 16:9
#define RESY	720
#define ALLOW_WINDOWED


#ifdef A64BITS
#pragma pack(8) // VERY important, so WNDCLASS gets the correct padding and we don't crash the system
#endif


#define WIN32_LEAN_AND_MEAN
#define WIN32_EXTRA_LEAN

#include <windows.h>
#include <mmsystem.h>
//#include <string.h>
#include <stdio.h>
#include <math.h>

#include "NuajAPI/API/Types.h"
#include "NuajAPI/Math/Math.h"

#include "Utility/Events.h"
#include "Utility/Memory.h"
#include "Utility/Random.h"
#include "Utility/ASMHelpers.h"

#include "RendererD3D11/Device.h"


//////////////////////////////////////////////////////////////////////////
// Main info about the system
//
static struct WININFO
{
	//---------------
	HINSTANCE	hInstance;
	HDC			hDC;
	HWND		hWnd;
	//---------------
	bool		bFullscreen;

#ifdef _DEBUG
	//---------------
	MSYS_EVENTINFO	Events;
#endif

} WindowInfos;


//////////////////////////////////////////////////////////////////////////
// The DirectX device
static Device*	gs_pDevice;


//////////////////////////////////////////////////////////////////////////
// Progress callback
//
typedef struct
{
    void*	pInfos;
    void (*func)( WININFO* _pInfos, int _Progress );
} IntroProgressDelegate;


//////////////////////////////////////////////////////////////////////////
// Main intro functions
#include "Intro/Intro.h"

