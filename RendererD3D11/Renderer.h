#pragma once

//#include "targetver.h"
//#define WIN32_LEAN_AND_MEAN			 // Exclude rarely-used stuff from Windows headers

#pragma pack(4)		// MEGA IMPORTANT LINE OR MIS-ALIGNED DIRECTX STRUCTURES WILL COME AND BITE YOUR ASS!
#include "d3d11.h"
#include "dxgi.h"

#ifdef _DEBUG
#include "d3d9.h"
#endif

#pragma pack()

#define NUAJ_LIB   // Override the NjFloatX types with our math lib
#include "../NuajAPI/API/Types.h"
#include "../NuajAPI/API/Hashtable.h"
#include "../NuajAPI/API/List.h"
#include "../NuajAPI/API/ASMHelpers.h"
