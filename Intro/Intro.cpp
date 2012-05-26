#include "../GodComplex.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )	if ( pMaterial->HasErrors() ) return ErrorCode;


// Textures & Render targets
static Texture2D*	gs_pRTHDR = NULL;

// Primitives
static Primitive*	gs_pPrimQuad = NULL;		// Screen quad for post-processes

// Render states
static RasterizerState*		gs_pRS_CullNone = NULL;
static DepthStencilState*	gs_pDS_Disabled = NULL;
static BlendState*			gs_pBS_Disabled = NULL;

// Materials
static Material*	gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen

int	IntroInit( IntroProgressDelegate& _Delegate )
{
	//////////////////////////////////////////////////////////////////////////
	// Create render targets
	gs_pRTHDR = new Texture2D( gs_Device, RESX, RESY, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

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
	// Create render states
	{
		D3D11_RASTERIZER_DESC	Desc;
		ASM_memset( &Desc, 0, sizeof(Desc) );
		Desc.FillMode = D3D11_FILL_SOLID;
        Desc.CullMode = D3D11_CULL_NONE;
        Desc.FrontCounterClockwise = TRUE;
        Desc.DepthBias = D3D11_DEFAULT_DEPTH_BIAS;
        Desc.DepthBiasClamp = D3D11_DEFAULT_DEPTH_BIAS_CLAMP;
        Desc.SlopeScaledDepthBias = D3D11_DEFAULT_SLOPE_SCALED_DEPTH_BIAS;
        Desc.DepthClipEnable = TRUE;
        Desc.ScissorEnable = FALSE;
        Desc.MultisampleEnable = FALSE;
        Desc.AntialiasedLineEnable = FALSE;

		gs_pRS_CullNone = new RasterizerState( gs_Device, Desc );
	}
	{
		D3D11_DEPTH_STENCIL_DESC	Desc;
		ASM_memset( &Desc, 0, sizeof(Desc) );
		Desc.DepthEnable = false;
		Desc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
		Desc.DepthFunc = D3D11_COMPARISON_LESS;
		Desc.StencilEnable = false;
		Desc.StencilReadMask = 0;
		Desc.StencilWriteMask = 0;

		gs_pDS_Disabled = new DepthStencilState( gs_Device, Desc );
	}
	{
		D3D11_BLEND_DESC	Desc;
		ASM_memset( &Desc, 0, sizeof(Desc) );
		Desc.AlphaToCoverageEnable = false;
		Desc.IndependentBlendEnable = false;
		Desc.RenderTarget[0].BlendEnable = false;
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_COLOR;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_DEST_COLOR;
		Desc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_SRC_ALPHA;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_DEST_ALPHA;
		Desc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;
		Desc.RenderTarget[0].RenderTargetWriteMask = 0x0F;		// Seems to crash on my card when setting more than 4 bits of write mask ! (limited to 4 MRTs I suppose ?)

		gs_pBS_Disabled = new BlendState( gs_Device, Desc );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create materials
	{
		gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" );
		CHECK_MATERIAL( gs_pMatPostFinal, 1001 );
	}

	return 0;
}

void	IntroExit()
{
	// Release materials
 	delete gs_pMatPostFinal;

	// Release states
	delete gs_pRS_CullNone;
	delete gs_pDS_Disabled;
	delete gs_pBS_Disabled;

	// Release primitives
	delete gs_pPrimQuad;

	// Release render targets & textures
	delete gs_pRTHDR;
}

bool	IntroDo()
{
	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4( 0.5f, 0.5f, 0.5f, 1.0f ) );

	// Setup default states
	gs_Device.SetStates( *gs_pRS_CullNone, *gs_pDS_Disabled, *gs_pBS_Disabled );

// 	{	// Render some shit to the HDR buffer
// 		gs_Device.SetRenderTarget( *gs_pRTHDR );
// 
// 	}

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
//		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );
		gs_pPrimQuad->Render( M );
	USING_MATERIAL_END

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
