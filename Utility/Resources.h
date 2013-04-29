//////////////////////////////////////////////////////////////////////////
// Resource Loading Helpers
//
#pragma once


// Don't forget to add any new include file pair down here \/
#define REGISTERED_INCLUDE_FILES	\
	{ "Inc/Global.hlsl", IDR_SHADER_INCLUDE_GLOBAL },	\
	{ "Inc/RayTracing.hlsl", IDR_SHADER_INCLUDE_RAY_TRACING },	\
	{ "Inc/LayeredMaterials.hlsl", IDR_SHADER_INCLUDE_LAYERED_MATERIALS },	\
	{ "Inc/Volumetric.hlsl", IDR_SHADER_INCLUDE_VOLUMETRIC },	\
	{ "Inc/Atmosphere.hlsl", IDR_SHADER_INCLUDE_ATMOSPHERE },	\


// Add any shader files that need to be watched for automatic reloading (works only in DEBUG mode !)
#define REGISTERED_SHADER_FILES		\
	{ IDR_SHADER_POST_FINAL,					"./Resources/Shaders/PostFinal.hlsl" },	\
	{ IDR_SHADER_TEST_DISPLAY,					NULL },	\
	{ IDR_SHADER_TRANSLUCENCY_BUILD_ZBUFFER,	"./Resources/Shaders/TranslucencyBuildZBuffer.hlsl" },	\
	{ IDR_SHADER_TRANSLUCENCY_DIFFUSION,		"./Resources/Shaders/TranslucencyDiffusion.hlsl" },	\
	{ IDR_SHADER_TRANSLUCENCY_DISPLAY,			"./Resources/Shaders/TranslucencyDisplay.hlsl" },	\
	{ IDR_SHADER_ROOM_DISPLAY,					"./Resources/Shaders/RoomDisplay.hlsl" },	\
	{ IDR_SHADER_ROOM_TESSELATION,				"./Resources/Shaders/RoomTesselation.hlsl" },	\
	{ IDR_SHADER_ROOM_TEST_COMPUTE,				"./Resources/Shaders/RoomTestCompute.hlsl" },	\
	{ IDR_SHADER_ROOM_BUILD_LIGHTMAP,			"./Resources/Shaders/RoomBuildLightMap.hlsl" },	\
	{ IDR_SHADER_PARTICLES_COMPUTE,				"./Resources/Shaders/ParticlesCompute.hlsl" },	\
/* Deferred Rendered Scene */ \
	{ IDR_SHADER_SCENE_DEPTH_PASS,				"./Resources/Shaders/SceneDepthPass.hlsl" },	\
	{ IDR_SHADER_SCENE_BUILD_LINEARZ,			"./Resources/Shaders/SceneBuildLinearZ.hlsl" },	\
	{ IDR_SHADER_SCENE_FILL_GBUFFER,			"./Resources/Shaders/SceneFillGBuffer.hlsl" },	\
	{ IDR_SHADER_SCENE_DOWNSAMPLE,				"./Resources/Shaders/SceneDownSample.hlsl" },	\
	{ IDR_SHADER_SCENE_SHADING_STENCIL,			"./Resources/Shaders/SceneShadingStencil.hlsl" },	\
	{ IDR_SHADER_SCENE_SHADING,					"./Resources/Shaders/SceneShading.hlsl" },	\
	{ IDR_SHADER_SCENE_INDIRECT_LIGHTING,		"./Resources/Shaders/SceneIndirectLighting.hlsl" },	\
/* Volumetric Effect */ \
	{ IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE,"./Resources/Shaders/VolumetricPreComputeAtmosphere.hlsl" },	\
	{ IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE,"./Resources/Shaders/VolumetricComputeTransmittance.hlsl" },	\
	{ IDR_SHADER_VOLUMETRIC_DEPTH_WRITE,		"./Resources/Shaders/VolumetricDepthWrite.hlsl" },	\
	{ IDR_SHADER_VOLUMETRIC_DISPLAY,			"./Resources/Shaders/VolumetricDisplay.hlsl" },	\
	{ IDR_SHADER_VOLUMETRIC_COMBINE,			"./Resources/Shaders/VolumetricCombine.hlsl" },	\
	{ IDR_SHADER_VOLUMETRIC_TERRAIN,			"./Resources/Shaders/VolumetricTerrain.hlsl" },	\
\
\
/* ============= Workshop ============= */ \
	{ IDR_SHADER_PARTICLES_DISPLAY,				"./Resources/Shaders/ParticlesDisplay.hlsl" }, 	\
	{ IDR_SHADER_DEFERRED_DEPTH_PASS,			"./Resources/Shaders/DeferredDepthPass.hlsl" },	\
	{ IDR_SHADER_DEFERRED_FILL_GBUFFER,			"./Resources/Shaders/DeferredFillGBuffer.hlsl" },	\
	{ IDR_SHADER_DEFERRED_SHADING_STENCIL,		"./Resources/Shaders/DeferredShadingStencil.hlsl" }, \
	{ IDR_SHADER_DEFERRED_SHADING,				"./Resources/Shaders/DeferredShading.hlsl" }, \
\
\
/* ============= Includes ============= */ \
	{ IDR_SHADER_INCLUDE_GLOBAL,				NULL },	\
	{ IDR_SHADER_INCLUDE_RAY_TRACING,			NULL },	\
	{ IDR_SHADER_INCLUDE_LAYERED_MATERIALS,		"./Resources/Shaders/Inc/LayeredMaterials.hlsl" },	\
	{ IDR_SHADER_INCLUDE_VOLUMETRIC,			"./Resources/Shaders/Inc/Volumetric.hlsl" },	\
	{ IDR_SHADER_INCLUDE_ATMOSPHERE,			"./Resources/Shaders/Inc/Atmosphere.hlsl" },	\


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
// IMPORTANT NOTE: You MUST destroy the returned pointed once you're done with it !
char*			LoadResourceShader( U16 _ResourceID, U32& _CodeSize );

// Create a full-fledged material given the shader resource ID and the vertex format
// NOTE: The ShaderFileName is only here for debug purpose
Material*		CreateMaterial( U16 _ShaderResourceID, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros=NULL );
Material*		CreateMaterial( U16 _ShaderResourceID, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros=NULL );

// Create a full-fledged compute shader given the shader resource ID
// NOTE: The ShaderFileName is only here for debug purpose
ComputeShader*	CreateComputeShader( U16 _ShaderResourceID, const char* _pEntryPoint, D3D_SHADER_MACRO* _pMacros=NULL );

const char*		LoadCSO( const char* _pCSOPath );