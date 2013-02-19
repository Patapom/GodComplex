//////////////////////////////////////////////////////////////////////////
// Main intro code
//
#include "../GodComplex.h"

#include "ConstantBuffers.h"

#include "Effects/EffectTranslucency.h"
#include "Effects/EffectRoom.h"

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
	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(0.8f), _TV(2.0f) ), NjFloat3( 0.0f, 0.8f, 0.0f ), NjFloat3::UnitY );
	gs_pCamera->Upload( 0 );

	//////////////////////////////////////////////////////////////////////////
	// Prepare scene
	gs_pEffectScene->Render( _Time, _DeltaTime );


#endif

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}

//////////////////////////////////////////////////////////////////////////
// Build the scene with all the objects & primitives
namespace
{
	Primitive*	gs_pPrimSphere0;
}

void	PrepareScene()
{
	// Create a sphere primitive
	{
		GeometryBuilder::MapperSpherical	Mapper;
		gs_pPrimSphere0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 60, 30, *gs_pPrimSphere0, Mapper );
	}

	gs_pScene->AllocateObjects( 1 );

	// Create our sphere object
	{
		Scene::Object&	Sphere0 = gs_pScene->CreateObjectAt( 0, "Sphere0" );
		Sphere0.AllocatePrimitives( 1 );
		Sphere0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimSphere0 );

		PrimitiveMaterial	Mat =
		{
			NULL,
			{ 0, 1, 2, 3 },
			NjFloat3( 0, 0, 0 ),
		};

//		Sphere0.GetPrimitiveAt( 0 ).SetMaterial( Mat );
	}
}

void	ReleaseScene()
{
	delete gs_pPrimSphere0;
}