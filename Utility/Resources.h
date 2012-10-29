//////////////////////////////////////////////////////////////////////////
// Resource Loading Helpers
//
#pragma once


// Don't forget to add any new include file pair down here \/
#define REGISTERED_INCLUDE_FILES	\
	{ "Inc/Global.fx", IDR_SHADER_INCLUDE_GLOBAL },	\

// Add any shader files that need to be watched for automatic reloading (works only in DEBUG mode !)
#define REGISTERED_SHADER_FILES		\
	{ IDR_SHADER_POST_FINAL,					"./Resources/Shaders/PostFinal.fx" },	\
	{ IDR_SHADER_INCLUDE_GLOBAL,				NULL },	\
	{ IDR_SHADER_TEST_DISPLAY,					NULL },	\
	{ IDR_SHADER_TRANSLUCENCY_BUILD_ZBUFFER,	"./Resources/Shaders/TranslucencyBuildZBuffer" },	\
	{ IDR_SHADER_TRANSLUCENCY_DIFFUSION,		"./Resources/Shaders/TranslucencyDiffusion" },	\
	{ IDR_SHADER_TRANSLUCENCY_DISPLAY,			"./Resources/Shaders/TranslucencyDisplay" },	\
	{ IDR_SHADER_ROOM_DISPLAY,					"./Resources/Shaders/RoomDisplay.fx" },	\
	{ IDR_SHADER_ROOM_TESSELATION,				"./Resources/Shaders/RoomTesselation.fx" },	\
	{ IDR_SHADER_ROOM_TEST_COMPUTE,				"./Resources/Shaders/RoomTestCompute.fx" },	\


#include "..\GodComplex.h"
#include "d3dcommon.h"

class IVertexFormatDescriptor;
class Material;
class ComputeShader;


// Loads a binary resource in memory
//	_pResourceType, type of binary resource to load
//	_pResourceSize, an optional pointer to an int that will contain the size of the loaded resource
const U8*	LoadResourceBinary( U16 _ResourceID, const char* _pResourceType, U32* _pResourceSize=NULL );

// Loads a text shader resource in memory
// IMPORTANT NOTE: You MUST destroy the returned pointed once you're done with it !
char*		LoadResourceShader( U16 _ResourceID, U32& _CodeSize );

// Create a full-fledged material given the shader resource ID and the vertex format
// NOTE: The ShaderFileName is only here for debug purpose
Material*	CreateMaterial( U16 _ShaderResourceID, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros=NULL );
Material*	CreateMaterial( U16 _ShaderResourceID, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros=NULL );

// Create a full-fledged compute shader given the shader resource ID
// NOTE: The ShaderFileName is only here for debug purpose
ComputeShader*	CreateComputeShader( U16 _ShaderResourceID, const char* _pEntryPoint, D3D_SHADER_MACRO* _pMacros=NULL );
