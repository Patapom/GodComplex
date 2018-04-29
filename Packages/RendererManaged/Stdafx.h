// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#pragma unmanaged

#include <memory.h>

// Base lib
#include "..\..\BaseLib\Types.h"

// Renderer lib
#include "../../RendererD3D11/Device.h"
#include "../../RendererD3D11/Components/Texture2D.h"
#include "../../RendererD3D11/Components/Texture3D.h"
#include "../../RendererD3D11/Components/StructuredBuffer.h"
#include "../../RendererD3D11/Components/Shader.h"
#include "../../RendererD3D11/Components/ComputeShader.h"
#include "../../RendererD3D11/Components/ConstantBuffer.h"
#include "../../RendererD3D11/Components/Primitive.h"
#include "../../RendererD3D11/Components/States.h"

#include "../../RendererD3D11/Structures/VertexFormats.h"

#include "../../RendererD3D11/Utility/FileServer.h"
#include "../../RendererD3D11/Utility/ShaderCompiler.h"

#pragma managed
