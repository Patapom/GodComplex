#pragma once

//#include "targetver.h"
//#define WIN32_LEAN_AND_MEAN			 // Exclude rarely-used stuff from Windows headers

// #pragma pack( push, 4 )		// MEGA IMPORTANT LINE OR MIS-ALIGNED DIRECTX STRUCTURES WILL COME AND BITE YOUR ASS!
// #include "d3d11.h"
// #include "dxgi.h"
#include <d3d11.h>

#ifdef _DEBUG
//#include "d3d9.h"
#endif

//#pragma pack( pop )

// Define this to enable shader reflection and be able to list input textures & uniforms
// WARNING: I didn't manage to solve a link problem about a missing IID_ID3D11ShaderReflection implementation so the RendererManaged library won't work with that option!
//#define ENABLE_SHADER_REFLECTION

// Define this if you're debugging the app using Nvidia Nsight
//#define NSIGHT

// Define this if you're debugging the app using Crytek's RenderDoc
//#define RENDERDOC
