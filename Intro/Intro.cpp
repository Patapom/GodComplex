#include "../GodComplex.h"

// Textures & Render targets
static Texture2D*	gs_pRTHDR = NULL;

// Primitives
static Primitive*	gs_pPrimQuad = NULL;	// Screen quad for post-processes

// Materials
static Material*	gs_MatPostFinal = NULL;	// Final post-process rendering to the screen

bool	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create render targets
	gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	//////////////////////////////////////////////////////////////////////////
	// Create primitives
	{
		NjFloat4	pVertices[4] =
		{
			NjFloat4( -1.0f, +1.0f, 0.0f, 0.5f ),
			NjFloat4( -1.0f, -1.0f, 0.0f, 0.5f ),
			NjFloat4( +1.0f, +1.0f, 0.0f, 0.5f ),
			NjFloat4( +1.0f, -1.0f, 0.0f, 0.5f ),
		};
		gs_pPrimQuad = new Primitive( gs_Device, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, VertexFormatPt4::DESCRIPTOR );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	{
		gs_MatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" );
	}

	return true;
}

void	IntroExit()
{
	delete gs_pPrimQuad;

	delete gs_pRTHDR;
}

bool	IntroDo()
{
	{	// Render some shit to the HDR buffer
		gs_Device.SetRenderTarget( *gs_pRTHDR );

	}

	{	// Render to screen
//		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );
	}

	return false;
}
