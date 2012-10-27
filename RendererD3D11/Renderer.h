#pragma once

#ifndef RENDERER_H_INCLUDED
#define RENDERER_H_INCLUDED

//#include "targetver.h"
//#define WIN32_LEAN_AND_MEAN			 // Exclude rarely-used stuff from Windows headers

#include "d3d11.h"
#include "dxgi.h"

#define NUAJ_LIB   // Override the NjFloatX types with our math lib
#include "../NuajAPI/API/Types.h"
#include "../NuajAPI/API/Hashtable.h"
#include "../NuajAPI/API/ASMHelpers.h"

#endif