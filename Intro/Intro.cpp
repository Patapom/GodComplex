#include "../GodComplex.h"
#include "ConstantBuffers.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )	if ( (pMaterial)->HasErrors() ) return ErrorCode;

static Camera*			gs_pCamera = NULL;

// Textures & Render targets
static Texture2D*		gs_pRTHDR = NULL;
//static Texture2D*		gs_pTexTestNoise = NULL;

// Primitives
static Primitive*		gs_pPrimQuad = NULL;		// Screen quad for post-processes
static Primitive*		gs_pPrimSphereInternal;
static Primitive*		gs_pPrimSphereExternal;

// Materials
static Material*		gs_pMatPostFinal = NULL;	// Final post-process rendering to the screen
static Material*		gs_pMatTestDisplay = NULL;	// Some test material for primitive display

// Constant buffers
static CB<CBTest>*		gs_pCB_Test = NULL;
static CB<CBObject>*	gs_pCB_Object = NULL;


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

#include "Build2DTextures.cpp"

void	TweakSphere( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, NjFloat2& _UV, void* _pUserData )
{
	Noise&	N = *((Noise*) _pUserData);

	_Position = _Position + _Normal * 0.2f * N.Perlin( 0.01f * _Position );	// Add perlin
}

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


		{	// Build some spheres
			GeometryBuilder::MapperSpherical	Mapper( 4.0f, 2.0f );

			Noise	N( 1 );

			gs_pPrimSphereInternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
			GeometryBuilder::BuildSphere( 20, 10, *gs_pPrimSphereInternal, Mapper, TweakSphere, &N );

			gs_pPrimSphereExternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
			GeometryBuilder::BuildSphere( 20, 10, *gs_pPrimSphereExternal, Mapper );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Create materials
	{
		CHECK_MATERIAL( gs_pMatPostFinal = CreateMaterial( IDR_SHADER_POST_FINAL, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1001 );
		CHECK_MATERIAL( gs_pMatTestDisplay = CreateMaterial( IDR_SHADER_TEST_DISPLAY, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1002 );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create constant buffers
	{
		gs_pCB_Test = new CB<CBTest>( gs_Device );
		gs_pCB_Object = new CB<CBObject>( gs_Device );
	}

	return 0;
}

void	IntroExit()
{
	// Release constant buffers
	delete gs_pCB_Object;
	delete gs_pCB_Test;

	// Release materials
 	delete gs_pMatTestDisplay;
 	delete gs_pMatPostFinal;

	// Release primitives
	delete gs_pPrimSphereInternal;
	delete gs_pPrimSphereExternal;
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
	USING_MATERIAL_START( *gs_pMatTestDisplay )

		gs_Device.SetStates( *gs_Device.m_pRS_CullBack, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled );

		static float	ObjectAngle = 0.0f;
		ObjectAngle += _TV(0.5f) * _DeltaTime;

		gs_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( ObjectAngle, NjFloat3::UnitY ), NjFloat3::One );
		gs_pCB_Object->UpdateData();
		gs_pCB_Object->Set( 1 );

		gs_pPrimSphereInternal->Render( *gs_pMatTestDisplay );

	USING_MATERIAL_END

	// Setup default states
	gs_Device.SetStates( *gs_Device.m_pRS_CullNone, *gs_Device.m_pDS_Disabled, *gs_Device.m_pBS_Disabled );

	// Render to screen
	USING_MATERIAL_START( *gs_pMatPostFinal )
		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );

		gs_pCB_Test->m.LOD = 10.0f * (1.0f - fabs( sinf( _TV(1.0f) * _Time ) ));
//gs_CBTest.LOD = 0.0f;

		gs_pCB_Test->UpdateData();
		gs_pCB_Test->SetPS( 1 );

		gs_pTexTestNoise->SetPS( 0 );
		gs_pRTHDR->SetPS( 1 );

		gs_pPrimQuad->Render( M );

	USING_MATERIAL_END

	// Present !
	gs_Device.DXSwapChain().Present( 0, 0 );

	return true;	// True means continue !
}
