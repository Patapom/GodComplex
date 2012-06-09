#include "../GodComplex.h"

#include "ConstantBuffers.h"

#include "Effects/EffectTranslucency.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) return ErrorCode;
#define CHECK_EFFECT( pEffect, ErrorCode )			{ int EffectError = (pEffect)->GetErrorCode(); if ( EffectError != 0 ) return ErrorCode + EffectError; }


static Camera*			gs_pCamera = NULL;

// Textures & Render targets
static Texture2D*		gs_pRTHDR = NULL;
//static Texture2D*		gs_pTexTestNoise = NULL;

// Primitives
static Primitive*		gs_pPrimQuad = NULL;		// Screen quad for post-processes

// Materials
static Material*		gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen

// Constant buffers
static CB<CBTest>*		gs_pCB_Test = NULL;

// Effects
static EffectTranslucency*	gs_pEffectTranslucency = NULL;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

#include "Build2DTextures.cpp"

int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create our camera
	gs_pCamera = new Camera( gs_Device );
	gs_pCamera->SetPerspective( HALFPI, float(RESX) / RESY, 0.01f, 5000.0f );


	//////////////////////////////////////////////////////////////////////////
	// Create render targets & textures
	{
		gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

		Build2DTextures( _Delegate );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create primitives
	{
		NjFloat4	pVertices[4] =
		{
			NjFloat4( -1.0f, +1.0f, 0.0f, 0.0f ),
			NjFloat4( -1.0f, -1.0f, 0.0f, 0.0f ),
			NjFloat4( +1.0f, +1.0f, 0.0f, 0.0f ),
			NjFloat4( +1.0f, -1.0f, 0.0f, 0.0f ),
		};
		gs_pPrimQuad = new Primitive( gs_Device, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, VertexFormatPt4::DESCRIPTOR );

	}

	//////////////////////////////////////////////////////////////////////////
	// Create materials
	{
		CHECK_MATERIAL( gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1001 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create constant buffers
	{
		gs_pCB_Test = new CB<CBTest>( gs_Device );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create effects
	{
		CHECK_EFFECT( gs_pEffectTranslucency = new EffectTranslucency(), 2000 );	// Error codes should increase in hundreds like 2000, 2100, 2200, etc.
	}

	return 0;
}

void	IntroExit()
{
	// Release effects
	delete gs_pEffectTranslucency;

	// Release constant buffers
	delete gs_pCB_Test;

	// Release materials
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimQuad;

	// Release render targets & textures
	Delete2DTextures();
	delete gs_pRTHDR;

	// Release the camera
	delete gs_pCamera;
}

bool	IntroDo( float _Time, float _DeltaTime )
{
//	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );
	gs_Device.ClearRenderTarget( *gs_pRTHDR, NjFloat4( 0.5f, 0.25f, 0.125f, 0.0f ) );
	gs_Device.ClearDepthStencil( gs_Device.DefaultDepthStencil(), 1.0f, 0 );

	//////////////////////////////////////////////////////////////////////////
	// Update the camera settings and upload its data to the shaders

	// TODO: Animate camera...
	gs_pCamera->LookAt( NjFloat3( 0.0f, 0.0f, 2.0f ), NjFloat3( 0.0f, 0.0f, 0.0f ), NjFloat3::UnitY );

	gs_pCamera->Upload( 0 );


	//////////////////////////////////////////////////////////////////////////
	// Render some shit to the HDR buffer
	gs_Device.SetRenderTarget( *gs_pRTHDR, &gs_Device.DefaultDepthStencil() );

	gs_pEffectTranslucency->Render( _Time, _DeltaTime );

	// Setup default states
	gs_Device.SetStates( *gs_Device.m_pRS_CullNone, *gs_Device.m_pDS_Disabled, *gs_Device.m_pBS_Disabled );

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );

		gs_pCB_Test->m.LOD = 10.0f * (1.0f - fabs( sinf( _TV(1.0f) * _Time ) ));
//gs_CBTest.LOD = 0.0f;

		gs_pCB_Test->UpdateData();
		gs_pCB_Test->SetPS( 1 );

//		gs_pTexTestNoise->SetPS( 0 );
		gs_pEffectTranslucency->GetZBuffer()->SetPS( 0 );
		gs_pRTHDR->SetPS( 1 );

		gs_pPrimQuad->Render( M );

	USING_MATERIAL_END

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
