//////////////////////////////////////////////////////////////////////////
// Main intro code for our code Workshop
//
#include "../GodComplex.h"

#include "ConstantBuffers.h"

#include "Effects/EffectParticles.h"
#include "Effects/EffectDeferred.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) return ErrorCode;
#define CHECK_EFFECT( pEffect, ErrorCode )			{ int EffectError = (pEffect)->GetErrorCode(); if ( EffectError != 0 ) return ErrorCode + EffectError; }

static Camera*			gs_pCamera = NULL;

// Primitives
Primitive*				gs_pPrimQuad = NULL;		// Screen quad for post-processes

// Materials
static Material*		gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen

// Constant buffers
static CB<CBGlobal>*	gs_pCB_Global = NULL;

// Effects
static EffectParticles*		gs_pEffectParticles = NULL;
static EffectDeferred*		gs_pEffectDeferred = NULL;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

#include "Build2DTextures.cpp"
#include "Build3DTextures.cpp"

void	DevicesEnumerator( int _DeviceIndex, const BSTR& _FriendlyName, const BSTR& _DevicePath, IMoniker* _pMoniker, void* _pUserData )
{

}

int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create our camera
	gs_pCamera = new Camera( gs_Device );
	gs_pCamera->SetPerspective( NUAJDEG2RAD( 50.0f ), float(RESX) / RESY, 0.01f, 5000.0f );

	// NOTE: Camera reserves the CB slot #0 for itself !


	//////////////////////////////////////////////////////////////////////////
	// Create render targets & textures
	{
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
	}

	//////////////////////////////////////////////////////////////////////////
	// Create effects
	{
//		CHECK_EFFECT( gs_pEffectParticles = new EffectParticles(), ERR_EFFECT_PARTICLES );
		CHECK_EFFECT( gs_pEffectDeferred = new EffectDeferred(), ERR_EFFECT_DEFERRED );
	}

	return 0;
}

void	IntroExit()
{
	// Release effects
	delete gs_pEffectParticles;
	delete gs_pEffectDeferred;

	// Release constant buffers
	delete gs_pCB_Global;

	// Release materials
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimQuad;

	// Release render targets & textures
	Delete3DTextures();
	Delete2DTextures();

	// Release the camera
	delete gs_pCamera;
}

bool	IntroDo( float _Time, float _DeltaTime )
{
	// Upload global parameters
	gs_pCB_Global->m.Time.Set( _Time, _DeltaTime, 1.0f / _Time, 1.0f / _DeltaTime );
	gs_pCB_Global->UpdateData();


	//////////////////////////////////////////////////////////////////////////
	// Render particles only

	// TODO: Animate camera...
	gs_pCamera->LookAt( NjFloat3( _TV(0.0f), _TV(1.5f), _TV(2.0f) ), NjFloat3( 0.0f, 1.5f, 0.0f ), NjFloat3::UnitY );
	gs_pCamera->Upload( 0 );


	//////////////////////////////////////////////////////////////////////////
	// Render some shit to the HDR buffer
	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );
	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

// 	gs_pEffectParticles->Render( _Time, _DeltaTime );

 	gs_pEffectDeferred->Render( _Time, _DeltaTime );


	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
