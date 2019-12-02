//////////////////////////////////////////////////////////////////////////
// Main include for the framework
//
// NOTE: Many routines were "borrowed" from iQ's 64K framework
//
#pragma once

// WARNING! If you ever change these, also reflect the changes in Resources/Shaders/Inc/Global.hlsl !
#define RESX	1280	// 720p 16:9
#define RESY	720

// #define RESX	512	// 720p 16:9
// #define RESY	288

#define ALLOW_WINDOWED

//#define MUSIC			// Enable music

#ifdef A64BITS
#pragma pack(8)			// VERY important, so WNDCLASS gets the correct padding and we don't crash the system
#pragma error caillou!
#endif

#include "BaseLib/Types.h"

#define WIN32_LEAN_AND_MEAN
#define WIN32_EXTRA_LEAN

#include <windows.h>
#include <mmsystem.h>
#include <string.h>
#include <stdio.h>
#include "resource.h"

#include "ErrorCodes.h"
#ifdef _DEBUG
#include "Utility/Events.h"
#endif
#include "Utility/Memory.h"
#include "Utility/Resources.h"
#include "Utility/Camera.h"
#include "Utility/MemoryMappedFile.h"
#include "Utility/Profiling.h"
#include "Utility/FPSCamera.h"
#include "Utility/Video.h"
#include "Utility/TextureFilePOM.h"
#include "Utility/Octree.h"

// DirectX Renderer
#include "RendererD3D11/Device.h"
#include "RendererD3D11/Components/Texture2D.h"
#include "RendererD3D11/Components/Texture3D.h"
#include "RendererD3D11/Components/StructuredBuffer.h"
#include "RendererD3D11/Components/Shader.h"
#include "RendererD3D11/Components/ComputeShader.h"
#include "RendererD3D11/Components/ConstantBuffer.h"
#include "RendererD3D11/Components/Primitive.h"
#include "RendererD3D11/Components/States.h"

// V2 Sound Player
#include "Sound/v2mplayer.h"
#include "Sound/libv2.h"

// 2D Procedural
#include "Procedural/TextureBuilder.h"
#include "Procedural/Generators/Noise.h"
#include "Procedural/Generators/Generators.h"
#include "Procedural/Filters/Filters.h"
#include "Procedural/DrawUtils/Draw.h"

// 3D Procedural
#include "Procedural/GeometryBuilder.h"
#include "Procedural/RayTracer.h"

// Scene loading
#include "Scene/Scene.h"

// Indirect Lighting
#include "Utility/SHProbeEncoder/SHProbeNetwork.h"
#include "Utility/SHProbeEncoder/SHProbeEncoder.h"


extern const float4	LUMINANCE;	// D65 Illuminant with observer at 2�

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
	U8				pKeys[256];
	U8				pKeysToggle[256];
#endif

};
extern WININFO		gs_WindowInfos;

//////////////////////////////////////////////////////////////////////////
// The DirectX device
extern Device		gs_Device;

//////////////////////////////////////////////////////////////////////////
// The sound player
extern V2MPlayer	gs_Music;
extern void*		gs_pMusicPlayerWorkMem;

//////////////////////////////////////////////////////////////////////////
// Progress callback
//
struct IntroProgressDelegate
{
    void*	pInfos;
    void (*func)( WININFO* _pInfos, int _Progress );
};

//////////////////////////////////////////////////////////////////////////
// Helpers
void	print( const char* _pText, ... );	// Works only in DEBUG mode !


//////////////////////////////////////////////////////////////////////////
// Main intro functions
#include "Intro/Intro.h"

