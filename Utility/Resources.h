//////////////////////////////////////////////////////////////////////////
// Resource Loading Helpers
//
#pragma once

// Don't forget to add any new include file pair down here \/
#define REGISTERED_INCLUDE_FILES											\
	{ "Inc/Global.hlsl",			"./Resources/Shaders/Inc/Global.hlsl",				IDR_SHADER_INCLUDE_GLOBAL },			\
	{ "Inc/RayTracing.hlsl",		"./Resources/Shaders/Inc/RayTracing.hlsl",			IDR_SHADER_INCLUDE_RAY_TRACING },		\
	{ "Inc/LayeredMaterials.hlsl",	"./Resources/Shaders/Inc/LayeredMaterials.hlsl",	IDR_SHADER_INCLUDE_LAYERED_MATERIALS },	\
	{ "Inc/Volumetric.hlsl",		"./Resources/Shaders/Inc/Volumetric.hlsl",			IDR_SHADER_INCLUDE_VOLUMETRIC },		\
	{ "Inc/Atmosphere.hlsl",		"./Resources/Shaders/Inc/Atmosphere.hlsl",			IDR_SHADER_INCLUDE_ATMOSPHERE },		\
	{ "Inc/SH.hlsl",				"./Resources/Shaders/Inc/SH.hlsl",					IDR_SHADER_INCLUDE_SH },				\
	{ "Inc/ShadowMap.hlsl",			"./Resources/Shaders/Inc/ShadowMap.hlsl",			IDR_SHADER_INCLUDE_SHADOW_MAP },		\


#include "..\GodComplex.h"
#include "d3dcommon.h"

class IVertexFormatDescriptor;
class Material;
class ComputeShader;


// Loads a binary resource in memory
//	_pResourceType, type of binary resource to load
//	_pResourceSize, an optional pointer to an int that will contain the size of the loaded resource
const U8*		LoadResourceBinary( U16 _ResourceID, const char* _pResourceType, U32* _pResourceSize=NULL );

// Loads a text shader resource in memory
// IMPORTANT NOTE: You MUST destroy the returned pointer once you're done with it !
char*			LoadResourceShader( U16 _ResourceID, U32& _CodeSize );

// Create a full-fledged material given the shader resource ID and the vertex format
// NOTE: The _pFileName is only here for debug purpose and should be provided only if you wish to watch a change on the source file
Material*		CreateMaterial( U16 _ShaderResourceID, const char* _pFileName, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros=NULL );
Material*		CreateMaterial( U16 _ShaderResourceID, const char* _pFileName, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros=NULL );

// Create a full-fledged compute shader given the shader resource ID
// NOTE: The _pFileName is only here for debug purpose and should be provided only if you wish to watch a change on the source file
ComputeShader*	CreateComputeShader( U16 _ShaderResourceID, const char* _pFileName, const char* _pEntryPoint, D3D_SHADER_MACRO* _pMacros=NULL );

// Call this regularly to check for include files modifications that will trigger recompilation of dependent shaders
void			WatchIncludesModifications();

const char*		LoadCSO( const char* _pCSOPath );