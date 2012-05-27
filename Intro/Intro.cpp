#include "../GodComplex.h"
#include "ConstantBuffers.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )	if ( pMaterial->HasErrors() ) return ErrorCode;

// Textures & Render targets
static Texture2D*	gs_pRTHDR = NULL;
static Texture2D*	gs_pTexTestNoise = NULL;

// Primitives
static Primitive*	gs_pPrimQuad = NULL;		// Screen quad for post-processes

// Materials
static Material*	gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen

// Constant buffers
static CBTest			gs_CBTest;
static ConstantBuffer*	gs_pCB_Test = NULL;

void	FillNoise( int x, int y, const NjFloat2& _UV, NjFloat4& _Color )
{
//	float	C = abs( gs_Noise.Noise2D( 0.005f * _UV ) );	// Simple test with gradient noise that doesn't loop
	float	C = abs( gs_Noise.WrapNoise2D( _UV ) );			// Advanced test with gradient noise that loops !

	_Color.x = _Color.y = _Color.z = C;
	_Color.w = 1.0f;
}

int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create render targets
	gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	{
		TextureBuilder	TB( 512, 512 );
		TB.Fill( FillNoise );
		TB.GenerateMips( PixelFormatRGBA16F::DESCRIPTOR );
		gs_pTexTestNoise = new Texture2D( gs_Device, 512, 512, 1, PixelFormatRGBA16F::DESCRIPTOR, 0, TB.GetMips() );
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
		gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" );
		CHECK_MATERIAL( gs_pMatPostFinal, 1001 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create constant buffers
	{
		gs_pCB_Test = new ConstantBuffer( gs_Device, sizeof(gs_CBTest) );
	}

	return 0;
}

void	IntroExit()
{
	// Release constant buffers
	delete gs_pCB_Test;

	// Release materials
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimQuad;

	// Release render targets & textures
	delete gs_pTexTestNoise;
	delete gs_pRTHDR;
}

bool	IntroDo( float _Time, float _DeltaTime )
{
	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );

	// Setup default states
	gs_Device.SetStates( *gs_Device.m_pRS_CullNone, *gs_Device.m_pDS_Disabled, *gs_Device.m_pBS_Disabled );

// 	{	// Render some shit to the HDR buffer
// 		gs_Device.SetRenderTarget( *gs_pRTHDR );
// 
// 	}

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
//		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );

		gs_CBTest.LOD = 10.0f * (1.0f - fabs( sinf( _Time ) ));
		gs_pCB_Test->UpdateData( &gs_CBTest );
		gs_pCB_Test->SetPS( 0 );

		gs_pTexTestNoise->SetPS( 0 );

		gs_pPrimQuad->Render( M );
	USING_MATERIAL_END

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
