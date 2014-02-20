//////////////////////////////////////////////////////////////////////////
// Main intro code
//
#include "../GodComplex.h"

#include "ConstantBuffers.h"

// #include "Effects/EffectTranslucency.h"
// #include "Effects/EffectRoom.h"
// #include "Effects/EffectVolumetric.h"
//#include "Effects/EffectGlobalIllum2.h"
#include "Effects/EffectDOF.h"

// #include "Effects/Scene/MaterialBank.h"
// #include "Effects/Scene/Scene.h"
// #include "Effects/Scene/EffectScene.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) return ErrorCode;
#define CHECK_EFFECT( pEffect, ErrorCode )			{ int EffectError = (pEffect)->GetErrorCode(); if ( EffectError != 0 ) return ErrorCode + EffectError; }


static Camera*				gs_pCamera = NULL;
static FPSCamera*			gs_pCameraManipulator = NULL;
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
//static EffectTranslucency*	gs_pEffectTranslucency = NULL;
//static EffectRoom*			gs_pEffectRoom = NULL;
//static EffectScene*		gs_pEffectScene = NULL;
//static EffectVolumetric*	gs_pEffectVolumetric = NULL;
//static EffectGlobalIllum2*	gs_pEffectGI = NULL;
static EffectDOF*			gs_pEffectDOF = NULL;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

#include "Build2DTextures.cpp"
#include "Build3DTextures.cpp"

// void	DevicesEnumerator( int _DeviceIndex, const BSTR& _FriendlyName, const BSTR& _DevicePath, IMoniker* _pMoniker, void* _pUserData )
// {
// }

//#define TEST_SCENE

void	PrepareScene();
void	ReleaseScene();

int	IntroInit( IntroProgressDelegate& _Delegate )
{

/*	NjHalf		Test( 1.0f / 65535 );
	Test.raw = 0x0001;	// This gives wrong values when exponent is 0!
	Test.raw = 0x0400;	// Smallest exponent, no mantissa
	Test.raw = 0x0401;	// Smallest exponent, smallest non null mantissa		(6.1094761e-005)
	Test.raw = 0x0402;	// Smallest exponent, larger mantissa without LSbit		(6.1154366e-005)
	Test.raw = 0x0404;	// Smallest exponent, larger mantissa without 2 LSbits	(6.1273575e-005)
	float		Smallest = Test;

	int	i;
	for ( i=0; i < 100; i++ )
	{
	//	1/65535.0 = 1.5259021896696422e-005;

		float	f = _frand();
//		f = MAX( f, 1.5259021896696422e-005f );
//		f = MAX( f, 2*3.0518043793392843518730449378195e-5f );
//		U32		Bisou = U32( f * 65535.0f );
//		if ( Bisou & 1 )
//			break;

		U32	Bisou = U32( f * 65535 );
			Bisou &= ~0xF;
		float	f2 = Bisou / 65535.0f;
		U32	Bisou2 = U32( f2 * 65535 );
		if ( Bisou2 & 1 )
			break;
	}
*/

	//////////////////////////////////////////////////////////////////////////
	// Attempt to create the video capture object
// 	gs_pVideo = new Video( gs_Device, gs_WindowInfos.hWnd );
// 	gs_pVideo->EnumerateDevices( DevicesEnumerator, NULL );
// 	gs_pVideo->Init( 0 );	// Use first device
// 	gs_pVideo->Play();		// GO!


	//////////////////////////////////////////////////////////////////////////
	// Create our camera
	gs_pCamera = new Camera( gs_Device );	// NOTE: Camera reserves the CB slot #0 for itself !
	gs_pCamera->SetPerspective( NUAJDEG2RAD( 50.0f ), float(RESX) / RESY, 0.01f, 1000.0f );
	gs_pCamera->Upload( 0 );

//	gs_pCameraManipulator = new FPSCamera( *gs_pCamera, NjFloat3::Zero, NjFloat3::UnitZ );

	// Global illum test
	gs_pCameraManipulator = new FPSCamera( *gs_pCamera, NjFloat3( 0, 1, 6 ), -NjFloat3::UnitZ );


	//////////////////////////////////////////////////////////////////////////
	// Create our scene
//	gs_pScene = new Scene( gs_Device );


	//////////////////////////////////////////////////////////////////////////
	// Create render targets & textures
	{
		gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 3, NULL );

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
		CHECK_MATERIAL( gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, "./Resources/Shaders/PostFinal.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), ERR_EFFECT_INTRO+1 );
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
 
//		CHECK_EFFECT( gs_pEffectTranslucency = new EffectTranslucency( *gs_pRTHDR ), ERR_EFFECT_TRANSLUCENCY );

//		CHECK_EFFECT( gs_pEffectScene = new EffectScene( gs_Device, *gs_pScene, *gs_pPrimQuad ), ERR_EFFECT_SCENE );

//		CHECK_EFFECT( gs_pEffectVolumetric = new EffectVolumetric( gs_Device, *gs_pRTHDR, *gs_pPrimQuad, *gs_pCamera ), ERR_EFFECT_VOLUMETRIC );

//		CHECK_EFFECT( gs_pEffectGI = new EffectGlobalIllum2( gs_Device, *gs_pRTHDR, *gs_pPrimQuad, *gs_pCamera ), ERR_EFFECT_GLOBALILLUM );

		CHECK_EFFECT( gs_pEffectDOF = new EffectDOF( gs_Device, *gs_pRTHDR, *gs_pPrimQuad, *gs_pCamera ), ERR_EFFECT_DOF );
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize the scene last so it gives us the opportunity to fix shader errors first instead of waiting for the scene to be ready!
#ifdef TEST_SCENE
	PrepareScene();
#endif


	return 0;
}

void	IntroExit()
{
	// Release effects
	delete gs_pEffectDOF;
// 	delete gs_pEffectGI;
// 	delete gs_pEffectVolumetric;
//	delete gs_pEffectScene;
// 	delete gs_pEffectTranslucency;
// 	delete gs_pEffectRoom;

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
#ifdef TEST_SCENE
	ReleaseScene();
#endif
	delete gs_pScene;

	// Release the camera
	delete gs_pCameraManipulator;
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
	Primitive*	gs_pPrimTorus0;
	Primitive*	gs_pPrimCube0;
	Texture2D*	gs_pSceneTexture0;
	Texture2D*	gs_pSceneTexture1;
	Texture2D*	gs_pSceneTexture2;
	Texture2D*	gs_pSceneTexture3;
	Texture2D*	gs_pTexEnvMap;
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

#elif 0	// TEST FULL SCENE

	//////////////////////////////////////////////////////////////////////////
	// Animate camera
//	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(2.0f), _TV(2.0f) ), NjFloat3( 0.0f, 1.0f, 0.0f ), NjFloat3::UnitY );
//	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), 2.0f + sinf( 1.0f * _Time ), _TV(2.0f) ), NjFloat3( 0.0f, 1.0f, 0.0f ), NjFloat3::UnitY );

// _Time = 11.4f;	// Black negative normals ?
// _Time = 11.55f;	// Black specks

	float	t = 1.0f * _Time;

//	t = 31.415926535897932384626433832795f;
//	t = _TV( 41.2f );
//	t = 32 + 11.0f/60;
	t = 1.71f;

// 	float	Radius = 4.0f;
// 	gs_pCamera->LookAt( NjFloat3( Radius * sinf( 0.2f * t ), 2.0f + sinf( 1.0f * t ), Radius * cosf( 0.2f * t ) ), NjFloat3( 0.0f, 1.0f, 0.0f ), NjFloat3::UnitY );

	// Use the manipulator
	gs_pCameraManipulator->Update( _DeltaTime, 1.0f, 1.0f );

	gs_pCamera->Upload( 0 );

	//////////////////////////////////////////////////////////////////////////
	// Animate the scene
//	gs_pScene->GetObjectAt( 0 ).SetPRS( NjFloat3::UnitY, NjFloat4::QuatFromAngleAxis( 0.5f * _Time, NjFloat3::UnitY ) );
// 	gs_pScene->GetObjectAt( 1 ).SetPRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( -0.1f * _Time, NjFloat3::UnitY ) );
//	gs_pScene->GetObjectAt( 0 ).SetPRS( NjFloat3::UnitY, NjFloat4::QuatFromAngleAxis( 0.5f * _Time, NjFloat3( 1, 1, 1 ) ) );


	//////////////////////////////////////////////////////////////////////////
	// Render the scene
	gs_pEffectScene->Render( _Time, _DeltaTime, *gs_pRTHDR );

#elif 0	// TEST VOLUMETRIC

	//////////////////////////////////////////////////////////////////////////
	// Update the camera settings and upload its data to the shaders
#if 0
	// Auto animate
	float	t = 0.25f * _Time;
	float	R = 6.0f;
//t = 0;

// 	float	H = _TV(6.5f);
	float	H = 4.0f * cosf(2.0f*t);

// TODO: Animate camera...
//	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(0.2f), _TV(4.0f) ), NjFloat3( 0.0f, 1.5f, 0.0f ), NjFloat3::UnitY );
//	gs_pCamera->LookAt( NjFloat3( R*sinf(t), H, R*cosf(t) ), NjFloat3( 0.0f, 2.0f, 0.0f ), NjFloat3::UnitY );
//	gs_pCamera->LookAt( NjFloat3( 0, 1, 6 ), NjFloat3( 0.0f, 1.0f, -10.0f ), NjFloat3::UnitY );		// Inside clouds
//	gs_pCamera->LookAt( NjFloat3( 0, -10, 6 ), NjFloat3( 0.0f, -2.0f, -10.0f ), NjFloat3::UnitY );	// Below
//	gs_pCamera->LookAt( NjFloat3( 0, -10, 6 ), NjFloat3( 0.0f, -2.0f, 6.1f ), NjFloat3::UnitY );	// Below looking up

//	float	CameraHeight = 6.0f;	// Elevated
//	float	CameraHeight = 3.0f;	// Slightly elevated
// 	float	CameraHeight = 1.5f;	// Ground level

 	float	CameraHeight = LERP( 1.5f, 3.0f, 0.5f + 0.5f * sinf(t) );	// Ground level

	CameraHeight *= 1.0f;

//	NjFloat3	Target( 0.0f, 3.0f, -10.0f );		// Fixed target
	NjFloat3	Target( 10.0f * sinf(t), 3.0f, -10.0f );		// Fixed target

//	gs_pCamera->LookAt( NjFloat3( 0, CameraHeight, 6 ), NjFloat3( 0.0f, CameraHeight + 6.0f, -10.0f ), NjFloat3::UnitY );		// looking up
//	gs_pCamera->LookAt( NjFloat3( 0, CameraHeight, 6 ), NjFloat3( 0.0f, CameraHeight + 1.5f, -10.0f ), NjFloat3::UnitY );		// slightly looking up
//	gs_pCamera->LookAt( NjFloat3( 0, CameraHeight, 6 ), NjFloat3( 0.0f, CameraHeight + 1.0f, -10.0f ), NjFloat3::UnitY );		// looking forward
	gs_pCamera->LookAt( NjFloat3( 0, CameraHeight, 6 ), Target, NjFloat3::UnitY );

// 	NjFloat3	Center = NjFloat3( 0, CameraHeight, 6 );
// 	float		ViewAngle = 0.5f * t;
// 	gs_pCamera->LookAt( Center, Center + NjFloat3( sinf(ViewAngle), +0.1f, -cosf(ViewAngle) ), NjFloat3::UnitY );

#else
	// Use the manipulator
	gs_pCameraManipulator->Update( _DeltaTime, 5.0f, 1.0f );

#endif


	gs_pCamera->Upload( 0 );

	//////////////////////////////////////////////////////////////////////////
	// Render the effects
 	gs_Device.ClearRenderTarget( *gs_pRTHDR, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );
 	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

	gs_pEffectVolumetric->Render( _Time, _DeltaTime );


#elif 0	// TEST GLOBAL ILLUM

	gs_pCameraManipulator->Update( _DeltaTime, 1.0f, 1.0f );
	gs_pCamera->Upload( 0 );

 	gs_Device.ClearRenderTarget( *gs_pRTHDR, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );
 	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

	gs_pEffectGI->Render( _Time, _DeltaTime );


#elif 1	// TEST DEPTH OF FIELD

	gs_pCameraManipulator->Update( _DeltaTime, 1.0f, 1.0f );
	gs_pCamera->Upload( 0 );

 	gs_Device.ClearRenderTarget( *gs_pRTHDR, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );
 	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

	gs_pEffectDOF->Render( _Time, _DeltaTime );

#endif

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}

//////////////////////////////////////////////////////////////////////////
// Build the scene with all the objects & primitives
#ifdef TEST_SCENE
void	PrepareScene()
{
	//////////////////////////////////////////////////////////////////////////
	// Build our material bank
	{
		MaterialBank&	Bank = gs_pScene->GetMaterialBank();

		const int	MaterialsCount = 9;

		const char*		ppMatNames[MaterialsCount] = {
			"Empty",
			"Wood",
			"Metal",
			"Phenolic",
			"Rubber",
			"Marble",
			"Moss",
			"Fabric",
			"Aluminium",
		};

		MaterialBank::Material::StaticParameters	pMatParamsStatic[MaterialsCount] = {
			{0.001,0.001,0.001,0.001,0,0.001,0,0,0},										// #0 Empty
			{3.9810717055349722,6.309573444801933,0.471,0.521,0.87,1.041,0.04,1.3,0.02},	// #1 Wood
			{1047.1285480508996,3.2359365692962827,0.281,1.001,0.47,0.561,0.15,0.33,0.01},	// #2 Metal
			{1230.268770812381,7.413102413009175,0.321,0.501,0.35,0.941,0.15,0.02,0.01},	// #3 Phenolic
			{2.6302679918953817,15.488166189124811,0.541,0.481,0.94,0.891,0.13,0.81,0.01},	// #4 Rubber
			{812.8305161640995,4.36515832240166,0.191,0.521,0.49,1.031,0.23,0.14,0.01},		// #5 Marble
			{0.02754228703338166,21.87761623949553,0.651,1.001,5.11,0.941,0.32,1.77,0},		// #6 Moss (based on fabric preset)
			{0.14454397707459277,2.5703957827688635,1.001,0.641,2.65,1.111,0.03,2.54,0.03},	// #7 Fabric (based on blue-fabric)
			{1584.893192461114,0.6918309709189365,0.301,0.441,0.48,1.421,0.05,0.35,0.03},	// #8 Brushed Aluminium (based on aluminium + added a little diffuse)
		};

		// Thickness		// Material thickness in millimeters
		// Opacity			// Opacity in [0,1]
		// IOR				// Index of Refraction (for non opaque materials only)
		// Frosting			// A frosting coefficient in [0,1] (for non opaque materials only)
		//
		MaterialBank::Material::DynamicParameters	pMatParamsDynamic[MaterialsCount] = {
			{ 0.0f, 0.0f, 1.0f, 0.0f, 0 },		// Empty material lets light completely through
			{ 5.0f, 1, MAX_FLOAT, 0, 0 },		// Wood is simply opaque
			{ 5.0f, 1, MAX_FLOAT, 0, 1 },		// Metal has no diffuse hence the diffuse texture is used to color the specular
			{ 1.0f, 0.1f, 1.2f, 0.01f, 0 },		// Phenolic is used as transparent coating with slight refraction and frosting
			{ 1.0f, 1, MAX_FLOAT, 0, 0 },		// Rubber is simply opaque
			{ 1.0f, 1, MAX_FLOAT, 0, 0 },		// Marble is simply opaque
			{ 1.0f, 1, MAX_FLOAT, 0, 0 },		// Moss is simply opaque
			{ 1.0f, 1, MAX_FLOAT, 0, 0 },		// Fabric is simply opaque
			{ 1.0f, 1, MAX_FLOAT, 0, 0 },		// Aluminium is simply opaque
		};

		Bank.AllocateMaterials( MaterialsCount );
		for ( int MaterialIndex=0; MaterialIndex < MaterialsCount; MaterialIndex++ )
		{
			Bank.GetMaterialAt( MaterialIndex ).SetStaticParameters( Bank, ppMatNames[MaterialIndex], pMatParamsStatic[MaterialIndex] );
			Bank.GetMaterialAt( MaterialIndex ).SetDynamicParameters( pMatParamsDynamic[MaterialIndex] );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Create our complex material textures
#if 0
	{
		const int	LayeredTexturesCount = 4;

		const char*	ppNames[LayeredTexturesCount*6] = {
				// Wood + metal
				"./Resources/Images/LayeredMaterial0-Layer0.raw",
				"./Resources/Images/LayeredMaterial0-Layer1.raw",
				"./Resources/Images/LayeredMaterial0-Layer2.raw",
				"./Resources/Images/LayeredMaterial0-Layer3.raw",
				"./Resources/Images/LayeredMaterial0-Specular.raw",
				"./Resources/Images/LayeredMaterial0-Height.raw",

				// Blue rubber
				"./Resources/Images/LayeredMaterial1-Layer0.raw",
				"./Resources/Images/LayeredMaterial1-Layer1.raw",
				"./Resources/Images/LayeredMaterial1-Layer2.raw",
				"./Resources/Images/LayeredMaterial1-Layer3.raw",
				"./Resources/Images/LayeredMaterial1-Specular.raw",
				"./Resources/Images/LayeredMaterial1-Height.raw",

				// White marble + moss
				"./Resources/Images/LayeredMaterial2-Layer0.raw",
				"./Resources/Images/LayeredMaterial2-Layer1.raw",
				"./Resources/Images/LayeredMaterial2-Layer2.raw",
				"./Resources/Images/LayeredMaterial2-Layer3.raw",
				"./Resources/Images/LayeredMaterial2-Specular.raw",
				"./Resources/Images/LayeredMaterial2-Height.raw",

				// Brushed Aluminium + water puddles
				"./Resources/Images/LayeredMaterial3-Layer0.raw",
				"./Resources/Images/LayeredMaterial3-Layer1.raw",
				"./Resources/Images/LayeredMaterial3-Layer2.raw",
				"./Resources/Images/LayeredMaterial3-Layer3.raw",
				"./Resources/Images/LayeredMaterial3-Specular.raw",
				"./Resources/Images/LayeredMaterial3-Height.raw",
		};
		Texture2D**	pppTargetTextures[LayeredTexturesCount] = {
			&gs_pSceneTexture0,
			&gs_pSceneTexture1,
			&gs_pSceneTexture2,
			&gs_pSceneTexture3,
		};

		TextureBuilder	TBLayer0( 512, 512 );
		TextureBuilder	TBLayer1( 512, 512 );
		TextureBuilder	TBLayer2( 512, 512 );
		TextureBuilder	TBLayer3( 512, 512 );
		TextureBuilder	TBLayerSpecular( 512, 512 );
		TextureBuilder	TBLayerHeight( 512, 512 );

		int		pArraySizes[6];
		void**	pppContents[6];

		for ( int TextureIndex=0; TextureIndex < LayeredTexturesCount; TextureIndex++ )
		{
			const char**	ppTexNames = &ppNames[6*TextureIndex];
			Texture2D**		ppTargetTexture = pppTargetTextures[TextureIndex];

			TBLayer0.LoadFromRAWFile( ppTexNames[0] );
			TBLayer1.LoadFromRAWFile( ppTexNames[1] );
			TBLayer2.LoadFromRAWFile( ppTexNames[2] );
			TBLayer3.LoadFromRAWFile( ppTexNames[3] );
			TBLayerSpecular.LoadFromRAWFile( ppTexNames[4] );
			TBLayerHeight.LoadFromRAWFile( ppTexNames[5], true );

			// Convert all layers into a single texture
			pppContents[0] = TBLayer0.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[0] );
			pppContents[1] = TBLayer1.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[1] );
			pppContents[2] = TBLayer2.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[2] );
			pppContents[3] = TBLayer3.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[3] );
			pppContents[4] = TBLayerSpecular.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[4] );
			pppContents[5] = TBLayerHeight.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_NxNyNzH, pArraySizes[5], 4.0f );

			// Generate the final texture array
			*ppTargetTexture = TBLayer0.Concat( 6, pppContents, pArraySizes, PixelFormatRGBA8::DESCRIPTOR );
		}
	}
#else
	{
		TextureBuilder	TBLayer( 512, 512 );
		TBLayer.LoadFromRAWFile( "./Resources/Images/LayeredMaterial0-Layer0.raw" );

		int		pArraySizes[6];
		void**	pppContents[6];
		pppContents[0] = TBLayer.Convert( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA_sRGB, pArraySizes[0] );
		pppContents[1] = pppContents[0];	pArraySizes[1] = pArraySizes[0];
		pppContents[2] = pppContents[0];	pArraySizes[2] = pArraySizes[0];
		pppContents[3] = pppContents[0];	pArraySizes[3] = pArraySizes[0];
		pppContents[4] = pppContents[0];	pArraySizes[4] = pArraySizes[0];
		pppContents[5] = pppContents[0];	pArraySizes[5] = pArraySizes[0];
				
		Texture2D*	pTexPipo = TBLayer.Concat( 6, pppContents, pArraySizes, PixelFormatRGBA8::DESCRIPTOR );
		gs_pSceneTexture0 = pTexPipo;
		gs_pSceneTexture1 = pTexPipo;
		gs_pSceneTexture2 = pTexPipo;
		gs_pSceneTexture3 = pTexPipo;
	}
#endif

	//////////////////////////////////////////////////////////////////////////
	// Create the env map
	{
		TextureBuilder	TBEnvMap( 1024, 512 );		TBEnvMap.LoadFromFloatFile( "./Resources/Images/uffizi-large_1024x512.float" );
		gs_pTexEnvMap = TBEnvMap.CreateTexture( PixelFormatRGBA16F::DESCRIPTOR, TextureBuilder::CONV_RGBA );

		gs_pScene->SetEnvMap( *gs_pTexEnvMap );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create primitives
	{
		GeometryBuilder::MapperSpherical	MapperSphere( 4.0f, 2.0f );
		gs_pPrimSphere0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
//		GeometryBuilder::BuildSphere( 60, 30, *gs_pPrimSphere0, &MapperSphere );
		GeometryBuilder::BuildSphere( 60, 30, *gs_pPrimSphere0 );

		GeometryBuilder::MapperPlanar		MapperPlane( 0.125f, 0.125f );
		gs_pPrimPlane0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildPlane( 1, 1, 10.0f * NjFloat3::UnitX, -10.0f * NjFloat3::UnitZ, *gs_pPrimPlane0, &MapperPlane );

		gs_pPrimTorus0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildTorus( 60, 30, 1.0f, 0.3f, *gs_pPrimTorus0, NULL );

		gs_pPrimCube0 = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
//		GeometryBuilder::MapperCube		MapperCube( 2.0f, 2.0f );
		GeometryBuilder::BuildCube( 1, 1, 1, *gs_pPrimCube0, NULL );
	}

	gs_pScene->AllocateObjects( 5 );

	// Create our plane object
	{
		Scene::Object&	Plane0 = gs_pScene->CreateObjectAt( 0, "Plane0" );
						Plane0.SetPRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ) );

		Plane0.AllocatePrimitives( 1 );
		Plane0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimPlane0 );
		Plane0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture3, 8, 3, 0, 0 );		// Brushed aluminium + Water + Empty
	}

	// Create our sphere object
	{
		Scene::Object&	Sphere0 = gs_pScene->CreateObjectAt( 1, "Sphere0" );
						Sphere0.SetPRS( NjFloat3( 0, 0.8f, 0 ), NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), 0.8f * NjFloat3::One );

		Sphere0.AllocatePrimitives( 1 );
		Sphere0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimSphere0 );
		Sphere0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture2, 5, 6, 0, 0 );	// Marble + Moss + Empty
	}

	// Create our torus object
	{
		Scene::Object&	Torus0 = gs_pScene->CreateObjectAt( 2, "Torus" );
						Torus0.SetPRS( NjFloat3( 2.0f, 0.3f, 1.5f ), NjFloat4::QuatFromAngleAxis( 0.5f * PI, NjFloat3::UnitX ) );

		Torus0.AllocatePrimitives( 1 );
		Torus0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimTorus0 );
//		Torus0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture0, 1, 2, 3, 0 );	// Wood + Metal + Phenolic coating + Empty
		Torus0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture1, 4, 0, 0, 0 );		// Blue rubber

		Scene::Object&	Torus1 = gs_pScene->CreateObjectAt( 3, "Torus" );
//						Torus1.SetPRS( NjFloat3( 2.0f, 0.9f, 1.5f ), NjFloat4::QuatFromAngleAxis( 0.5f * PI, NjFloat3::UnitX ) );
						Torus1.SetPRS( NjFloat3( -2.0f, 0.3f, 1.6f ), NjFloat4::QuatFromAngleAxis( 0.5f * PI, NjFloat3::UnitX ) );

		Torus1.AllocatePrimitives( 1 );
		Torus1.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimTorus0 );
		Torus1.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture1, 7, 0, 0, 0 );		// Blue fabric
	}

	// Create our cube object
	{
		Scene::Object&	Cube0 = gs_pScene->CreateObjectAt( 4, "Sphere0" );
						Cube0.SetPRS( NjFloat3( -1.5f, 0.5f, -1.0f ), NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), 0.5f * NjFloat3::One );

		Cube0.AllocatePrimitives( 1 );
		Cube0.GetPrimitiveAt( 0 ).SetRenderPrimitive( *gs_pPrimCube0 );
		Cube0.GetPrimitiveAt( 0 ).SetLayerMaterials( *gs_pSceneTexture0, 1, 2, 3, 0 );	// Wood + Metal + Phenolic coating + Empty
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the lights
	gs_pScene->AllocateLights( 1, 1, 1 );
//	gs_pScene->AllocateLights( 1, 0, 0 );

 	gs_pScene->GetDirectionalLightAt( 0 ).SetDirectional( 10.0f * NjFloat3::One, NjFloat3( 4, 4, 4 ), -NjFloat3( 1, 1, 1 ), 10.0f, 12.0f, 16.0f );
//	gs_pScene->GetDirectionalLightAt( 0 ).SetDirectional( 50.0f * NjFloat3::One, NjFloat3( 0, 4, 0 ), -NjFloat3( 0, 1, 0 ), 1.0f, 1.5f, 8.0f );

//	gs_pScene->GetPointLightAt( 0 ).SetPoint( 20.0f * NjFloat3( 1, 1, 1 ), NjFloat3( -3.0f, 1.5f, -1.5f ), 8.0f );
	gs_pScene->GetPointLightAt( 0 ).SetPoint( 100.0f * NjFloat3( 1, 1, 1 ), NjFloat3( -3.0f, 5.5f, -1.5f ), 40.0f );
//	gs_pScene->GetPointLightAt( 0 ).SetPoint( NjFloat3( 0, 1, 0 ), NjFloat3( 0, 1, 0 ), 1.0f );

 	gs_pScene->GetSpotLightAt( 0 ).SetSpot( 50.0f * NjFloat3( 1, 1, 1 ), NjFloat3( -4, 3, 3 ), NjFloat3( 1, -2, -1 ), NUAJDEG2RAD(30.0f), NUAJDEG2RAD(40.0f), 16.0f );
//	gs_pScene->GetSpotLightAt( 0 ).SetSpot( NjFloat3( 0, 0, 4 ), NjFloat3( 0, 4, 0 ), -NjFloat3( 0, 1, 0 ), NUAJDEG2RAD(30.0f), NUAJDEG2RAD(40.0f), 8.0f );
}

void	ReleaseScene()
{
	delete gs_pPrimCube0;
	delete gs_pPrimPlane0;
	delete gs_pPrimTorus0;
	delete gs_pPrimSphere0;

// 	delete gs_pSceneTexture3;
// 	delete gs_pSceneTexture2;
// 	delete gs_pSceneTexture1;
	delete gs_pSceneTexture0;
	delete gs_pTexEnvMap;
}
#endif
