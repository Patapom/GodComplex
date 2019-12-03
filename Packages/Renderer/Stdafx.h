// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#include <vcclr.h>

#pragma unmanaged

#include <d3d11.h>

#include <memory.h>

// Free Image lib
#include "FreeImage.h"

// Base libs
#include "..\BaseLib\Types.h"

// Image Utility lib
#include "..\ImageUtilityLib\Bitmap.h"
#include "..\ImageUtilityLib\ImagesMatrix.h"

// Renderer lib
#include "../RendererLib/Device.h"
#include "../RendererLib/Components/Texture2D.h"
#include "../RendererLib/Components/Texture3D.h"
#include "../RendererLib/Components/StructuredBuffer.h"
#include "../RendererLib/Components/Shader.h"
#include "../RendererLib/Components/ComputeShader.h"
#include "../RendererLib/Components/ConstantBuffer.h"
#include "../RendererLib/Components/Primitive.h"
#include "../RendererLib/Components/States.h"

#include "../RendererLib/Structures/VertexFormats.h"

#include "../RendererLib/Utility/FileServer.h"
#include "../RendererLib/Utility/ShaderCompiler.h"

#pragma managed
