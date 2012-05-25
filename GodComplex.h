//////////////////////////////////////////////////////////////////////////
// Main include for the framework
//
// NOTE: Many routines were "borrowed" (rather violently stolen) from iQ's 64K framework
//
#pragma once

#define RESX	1280	// 720p 16:9
#define RESY	720
#define ALLOW_WINDOWED

#define MUSIC			// Enable music

#ifdef A64BITS
#pragma pack(8)			// VERY important, so WNDCLASS gets the correct padding and we don't crash the system
#endif

#define WIN32_LEAN_AND_MEAN
#define WIN32_EXTRA_LEAN

#include <windows.h>
#include <mmsystem.h>
//#include <string.h>
#include <stdio.h>
#include <math.h>
#include "resource.h"

#include "NuajAPI/API/Types.h"
#include "NuajAPI/Math/Math.h"
#include "Utility/ASMHelpers.h"
#include "Utility/Events.h"
#include "Utility/Memory.h"
#include "Utility/Random.h"
#include "Utility/Resources.h"

// DirectX Renderer
#include "RendererD3D11/Device.h"
#include "RendererD3D11/Components/Texture2D.h"
#include "RendererD3D11/Components/Texture3D.h"
#include "RendererD3D11/Components/Material.h"
#include "RendererD3D11/Components/ConstantBuffer.h"
#include "RendererD3D11/Components/Primitive.h"
#include "RendererD3D11/Components/States.h"

// V2 Sound Player
#include "Sound/v2mplayer.h"
#include "Sound/libv2.h"


//////////////////////////////////////////////////////////////////////////
// Main info about the system
//
struct WININFO
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

};
extern WININFO	WindowInfos;

//////////////////////////////////////////////////////////////////////////
// The DirectX device
extern Device		gs_Device;

//////////////////////////////////////////////////////////////////////////
// The sound player
extern V2MPlayer	gs_Music;

//////////////////////////////////////////////////////////////////////////
// Progress callback
//
struct IntroProgressDelegate
{
    void*	pInfos;
    void (*func)( WININFO* _pInfos, int _Progress );
};


//////////////////////////////////////////////////////////////////////////
// Main intro functions
#include "Intro/Intro.h"

