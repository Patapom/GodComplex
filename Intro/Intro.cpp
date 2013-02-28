//////////////////////////////////////////////////////////////////////////
// Main intro code
//
#include "../GodComplex.h"

#include "ConstantBuffers.h"

#include "Effects/EffectTranslucency.h"
#include "Effects/EffectRoom.h"

#include "Effects/Scene/MaterialBank.h"
#include "Effects/Scene/Scene.h"
#include "Effects/Scene/EffectScene.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) return ErrorCode;
#define CHECK_EFFECT( pEffect, ErrorCode )			{ int EffectError = (pEffect)->GetErrorCode(); if ( EffectError != 0 ) return ErrorCode + EffectError; }


static Camera*				gs_pCamera = NULL;
// Video*					gs_pVideo = NULL;

// Main scene
static Scene*				gs_pScene = NULL;

// Textures & Render targets
static Texture2D*			gs_pRTHDR = NULL;

// Primitives
Primitive*					gs_pPrimQuad = NULL;		// Screen quad for post-processes

// Materials
static Material*			gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen

// Constant buffers
static CB<CBGlobal>*		gs_pCB_Global = NULL;
static CB<CBTest>*			gs_pCB_Test = NULL;

// Effects
static EffectTranslucency*	gs_pEffectTranslucency = NULL;
static EffectRoom*			gs_pEffectRoom = NULL;
static EffectScene*			gs_pEffectScene = NULL;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

#include "Build2DTextures.cpp"
#include "Build3DTextures.cpp"

// void	DevicesEnumerator( int _DeviceIndex, const BSTR& _FriendlyName, const BSTR& _DevicePath, IMoniker* _pMoniker, void* _pUserData )
// {
// }

void	PrepareScene();
void	ReleaseScene();

int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Attempt to create the video capture object
// 	gs_pVideo = new Video( gs_Device, gs_WindowInfos.hWnd );
// 	gs_pVideo->EnumerateDevices( DevicesEnumerator, NULL );
// 	gs_pVideo->Init( 0 );	// Use first device
// 	gs_pVideo->Play();		// GO!


	//////////////////////////////////////////////////////////////////////////
	// Create our camera
	gs_pCamera = new Camera( gs_Device );	// NOTE: Camera reserves the CB slot #0 for itself !
	gs_pCamera->SetPerspective( NUAJDEG2RAD( 80.0f ), float(RESX) / RESY, 0.01f, 5000.0f );


	//////////////////////////////////////////////////////////////////////////
	// Create & initialize our scene
	gs_pScene = new Scene( gs_Device );
	PrepareScene();


	//////////////////////////////////////////////////////////////////////////
	// Create render targets & textures
	{
		gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

		Build2DTextures( _Delegate );
		Build3DTextures( _Delegate );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create primitives
	{
		NjFloat4	pVertices[4] =
		{
			NjFloat4( -1.0f, +1.0f, 0.0f, 1.0f ),
			NjFloat4( -1.0f, -1.0f, 0.0f, 1.0f ),
			NjFloat4( +1.0f, +1.0f, 0.0f, 1.0f ),
			NjFloat4( +1.0f, -1.0f, 0.0f, 1.0f ),
		};
		gs_pPrimQuad = new Primitive( gs_Device, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, VertexFormatPt4::DESCRIPTOR );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create materials
	{
		CHECK_MATERIAL( gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), ERR_EFFECT_INTRO+1 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create constant buffers
	{
		gs_pCB_Global = new CB<CBGlobal>( gs_Device, 1, true );	// Global params go to slot #1
		gs_pCB_Test = new CB<CBTest>( gs_Device, 10 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create effects
	{
// 		CHECK_EFFECT( gs_pEffectRoom = new EffectRoom( *gs_pRTHDR ), ERR_EFFECT_ROOM );
// 		gs_pEffectRoom->m_pTexVoronoi = gs_pEffectParticles->m_pTexVoronoi;
// 
//		CHECK_EFFECT( gs_pEffectTranslucency = new EffectTranslucency( *gs_pRTHDR ), ERR_EFFECT_TRANSLUCENCY );

		CHECK_EFFECT( gs_pEffectScene = new EffectScene( gs_Device, *gs_pScene, *gs_pPrimQuad ), ERR_EFFECT_SCENE );
	}

	return 0;
}

void	IntroExit()
{
	// Release effects
	delete gs_pEffectTranslucency;
	delete gs_pEffectRoom;
	delete gs_pEffectScene;

	// Release constant buffers
	delete gs_pCB_Test;
	delete gs_pCB_Global;

	// Release materials
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimQuad;

	// Release render targets & textures
	Delete3DTextures();
	Delete2DTextures();
	delete gs_pRTHDR;

	// Release the scene
	ReleaseScene();
	delete gs_pScene;

	// Release the camera
	delete gs_pCamera;

	// Release the video capture object
// 	if ( gs_pVideo != NULL )
// 	{
// 		gs_pVideo->Exit();
// 	 	delete gs_pVideo;
// 	}
}

namespace
{
	Primitive*	gs_pPrimSphere0;
	Primitive*	gs_pPrimPlane0;
	Texture2D*	gs_pSceneTexture0;
}

bool	IntroDo( float _Time, float _DeltaTime )
{
	// Upload global parameters
	gs_pCB_Global->m.Time.Set( _Time, _DeltaTime, 1.0f / _Time, 1.0f / _DeltaTime );
	gs_pCB_Global->UpdateData();

#if 0	// TEST ROOM

	//////////////////////////////////////////////////////////////////////////
	// Update the camera settings and upload its data to the shaders

	// TODO: Animate camera...
	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(1.5f), _TV(1.4f) ), NjFloat3( 0.0f, 1.7f, 0.0f ), NjFloat3::UnitY );

	gs_pCamera->Upload( 0 );


	//////////////////////////////////////////////////////////////////////////
	// Render some shit to the HDR buffer
	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );
	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );
//	gs_Device.SetRenderTarget( *gs_pRTHDR, &gs_Device.DefaultDepthStencil() );

	gs_pEffectRoom->Render( _Time, _DeltaTime );

#elif 0	// TEST TRANSLUCENCY

	//////////////////////////////////////////////////////////////////////////
	// Update the camera settings and upload its data to the shaders

	// TODO: Animate camera...
	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(0.8f), _TV(1.4f) ), NjFloat3( 0.0f, 0.8f, 0.0f ), NjFloat3::UnitY );

	gs_pCamera->Upload( 0 );

	//////////////////////////////////////////////////////////////////////////
	// Render the effects
//	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );
	gs_Device.ClearRenderTarget( *gs_pRTHDR, NjFloat4( 0.5f, 0.25f, 0.125f, 0.0f ) );
	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

	gs_pEffectTranslucency->Render( _Time, _DeltaTime );

	// Setup default states
	gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );

		gs_pCB_Test->m.LOD = 10.0f * (1.0f - fabs( sinf( _TV(1.0f) * _Time ) ));
		gs_pCB_Test->m.BackLight = 1.0f - gs_pEffectTranslucency->m_EmissivePower;
//gs_CBTest.LOD = 0.0f;
		gs_pCB_Test->UpdateData();

//		gs_pTexTestNoise->SetPS( 10 );
//		gs_pEffectTranslucency->GetZBuffer()->SetPS( 10 );
		gs_pEffectTranslucency->GetIrradiance()->SetPS( 10 );
		gs_pRTHDR->SetPS( 11 );

		gs_pPrimQuad->Render( M );

	USING_MATERIAL_END

#elif 1	// TEST FULL SCENE

	//////////////////////////////////////////////////////////////////////////
	// Animate camera
//	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(2.0f), _TV(2.0f) ), NjFloat3( 0.0f, 1.0f, 0.0f ), NjFloat3::UnitY );
//	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), 2.0f + sinf( 1.0f * _Time ), _TV(2.0f) ), NjFloat3( 0.0f, 1.0f, 0.0f ), NjFloat3::UnitY );
	gs_pCamera->LookAt( NjFloat3( 2.0f * sinf( 0.2f * _Time ), 2.0f + sinf( 1.0f * _Time ), 2.0f * cosf( 0.2f * _Time ) ), NjFloat3( 0.0f, 1.0f, 0.0f ), NjFloat3::UnitY );
	gs_pCamera->Upload( 0 );


	//////////////////////////////////////////////////////////////////////////
	// Animate the scene
// 	gs_pScene->GetObjectAt( 0 ).SetPRS( NjFloat3::UnitY, NjFloat4::QuatFromAngleAxis( 0.5f * _Time, NjFloat3::UnitY ) );
// 	gs_pScene->GetObjectAt( 1 ).SetPRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( -0.1f * _Time, NjFloat3::UnitY ) );


	//////////////////////////////////////////////////////////////////////////
	// Render the scene
	gs_pEffectScene->Render( _Time, _DeltaTime, gs_pSceneTexture0 );


#endif

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}

//////////////////////////////////////////////////////////////////////////
// Build the scene with all the objects & primitives

void	PrepareScene()
{
	//////////////////////////////////////////////////////////////////////////
	// Build our material bank
	{
		MaterialBank&	Bank = gs_pScene->GetMaterialBank();

		int	MaterialsCount = 4;

		const char*		ppMatNames[] = {
			"Empty",
			"Wood",
			"Metal",
			"Phenolic",
		};

		MaterialBank::Material::StaticParameters	pMatParamsStatic[] = {
			{0.001,0.001,0.001,0.001,0,0.001,0,0,0},										// Empty
			{3.9810717055349722,6.309573444801933,0.471,0.521,0.87,1.041,0.04,1.3,0.02},	// Wood
			{1047.1285480508996,3.2359365692962827,0.281,1.001,0.47,0.561,0.01,0.33,0.01},	// Metal
			{1230.268770812381,7.413102413009175,0.321,0.501,0.35,0.941,0.15,0.02,0.01},	// Phenolic

// 			{ 0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9},										// Empty
// 			{ 0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9},										// Empty
// 			{ 0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9},										// Empty
// 			{ 0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9},										// Empty

		};

		// Thickness		// Material thickness in millimeters
		// Opacity			// Opacity in [0,1]
		// IOR				// Index of Refraction
		// Frosting			// A frosting coefficient in [0,1]
		//
		MaterialBank::Material::DynamicParameters	pMatParamsDynamic[] = {
			{ 0.0f, 0.0f, 1.0f, 0.0f, 0 },		// Empty material lets light completely through
			{ 5.0f, 1, MAX_FLOAT, 0, 0 },
			{ 5.0f, 1, MAX_FLOAT, 0, 1 },		// Metal has no diffuse hence the diffuse texture is used to color the specular
			{ 1.0f, 0.1f, 1.2f, 0.01f, 0 },		// Phenolic is used as transparent coating with slight refraction and frosting
		};

		Bank.AllocateMaterials( MaterialsCount );
		for ( int MaterialIndex=0; MaterialIndex < MaterialsCount; MaterialIndex++ )
		{
			Bank.GetMaterialAt( MaterialIndex ).SetStaticParameters( Bank, ppMatNames[MaterialIndex], pMatParamsStatic[MaterialIndex] );
			Bank.GetMaterialAt( MaterialIndex ).SetDynamicParameters( pMatParamsDynamic[MaterialIndex] );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Create our complex material texture
	{
		TextureBuilder	TBLayer0( 512, 512 );			TBLayer0.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Layer0.raw" );
		TextureBuilder	TBLayer1( 512, 512 );			TBLayer1.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Layer1.raw" );
		TextureBuilder	TBLayer2( 512, 512 );			TBLayer2.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Layer2.raw" );
		TextureBuilder	TBLayer3( 512, 512 );			TBLayer3.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Layer3.raw" );
		TextureBuilder	TBLayerSpecular( 512, 512 );	TBLayerSpecular.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Specular.raw" );
		TextureBuilder	TBLayerHeight( 512, 512 );		TBLayerHeight.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Height.raw", true );

		// Convert all layers
		int		pArraySizes[6];
		void**	pppContents[6];
		pppContents[0] = TBLayer0.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[0] );
		pppContents[1] = TBLayer1.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[1] );
		pppContents[2] = TBLayer2.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[2] );
		pppContents[3] = TBLayer3.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[3] );
		pppContents[4] = TBLayerSpecular.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[4] );
		pppContents[5] = TBLayerHeight.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_NxNyNzH, pArraySizes[5], 4.0f );

		// Generate the final texture array
		gs_pSceneTexture0 = TBLayer0.Concat( 6, pppContents, pArraySizes, PixelFormatRGBA8::DESCRIPTOR );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create primitives
	{
		GeometryBuilder::MapperSpherical	MapperSphere( 4.0f, 2.0f );
		gs_pPrimSphere0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 60, 30, *gs_pPrimSphere0, MapperSphere );

		GeometryBuilder::MapperPlanar		MapperPlane( 1.0f, 1.0f );
		gs_pPrimPlane0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildPlane( 1, 1, 10.0f * NjFloat3::UnitX, -10.0f * NjFloat3::UnitZ, *gs_pPrimPlane0, MapperPlane );
	}

	gs_pScene->AllocateObjects( 2 );

	// Create our sphere object
	{
		Scene::Object&	Sphere0 = gs_pScene->CreateObjectAt( 0, "Sphere0" );
						Sphere0.SetPRS( NjFloat3::UnitY, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ) );

		Sphere0.AllocatePrimitives( 1 );
		Sphere0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimSphere0 );
		Sphere0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture0, 1, 2, 3, 0 );	// Wood + Metal + Phenolic coating + Empty
	}

	// Create our plane object
	{
		Scene::Object&	Plane0 = gs_pScene->CreateObjectAt( 1, "Plane0" );
						Plane0.SetPRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ) );

		Plane0.AllocatePrimitives( 1 );
		Plane0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimPlane0 );
		Plane0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture0, 1, 2, 3, 0 );	// Wood + Metal + Phenolic coating + Empty
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the lights
	gs_pScene->AllocateLights( 1, 1, 1 );

 	gs_pScene->GetDirectionalLightAt( 0 ).SetDirectional( 50.0f * NjFloat3::One, NjFloat3( 4, 4, 4 ), -NjFloat3( 1, 1, 1 ), 2.0f, 3.0f, 16.0f );
//	gs_pScene->GetDirectionalLightAt( 0 ).SetDirectional( NjFloat3( 1, 0, 0 ), NjFloat3( 0, 4, 0 ), -NjFloat3( 0, 1, 0 ), 0.5f, 1.0f, 8.0f );

	gs_pScene->GetPointLightAt( 0 ).SetPoint( 20.0f * NjFloat3( 1, 1, 1 ), NjFloat3( -3.0f, 1.5f, -1.5f ), 8.0f );
//	gs_pScene->GetPointLightAt( 0 ).SetPoint( NjFloat3( 0, 1, 0 ), NjFloat3( 0, 1, 0 ), 1.0f );

 	gs_pScene->GetSpotLightAt( 0 ).SetSpot( 50.0f * NjFloat3( 1, 1, 1 ), NjFloat3( -4, 3, 3 ), NjFloat3( 1, -2, -1 ), NUAJDEG2RAD(30.0f), NUAJDEG2RAD(40.0f), 16.0f );
//	gs_pScene->GetSpotLightAt( 0 ).SetSpot( NjFloat3( 0, 0, 4 ), NjFloat3( 0, 4, 0 ), -NjFloat3( 0, 1, 0 ), NUAJDEG2RAD(30.0f), NUAJDEG2RAD(40.0f), 8.0f );
}

void	ReleaseScene()
{
	delete gs_pPrimSphere0;
	delete gs_pPrimPlane0;
	delete gs_pSceneTexture0;
}