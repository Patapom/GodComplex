#include "../../GodComplex.h"
#include "EffectGlobalIllum.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectGlobalIllum::EffectGlobalIllum( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_ErrorCode( 0 ), m_Device( _Device ), m_RTTarget( _RTHDR )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_ROOM_DISPLAY, "./Resources/Shaders/RoomDisplay.hlsl", VertexFormatP3N3G3T3T3::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatDisplayEmissive = CreateMaterial( IDR_SHADER_ROOM_DISPLAY, "./Resources/Shaders/RoomDisplay.hlsl", VertexFormatP3N3G3T3T3::DESCRIPTOR, "VS", NULL, "PS_Emissive" ), 2 );

}

EffectGlobalIllum::~EffectGlobalIllum()
{
	m_Scene.Render();

 	delete m_pCB_Object;

 	delete m_pMatDisplay;
 	delete m_pMatDisplayEmissive;
}


void	EffectGlobalIllum::Render( float _Time, float _DeltaTime )
{
	m_Scene.Render();
}

void	EffectGlobalIllum::InitScene()
{
	m_Scene.Load( IDR_SCENE_GI, *this );
}

void*	EffectGlobalIllum::TagMaterial( const Scene::Material& _Material ) const
{
	return NULL;
}
void*	EffectGlobalIllum::TagNode( const Scene::Node& _Node ) const
{
	return NULL;
}
void*	EffectGlobalIllum::TagPrimitive( const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) const
{
	return NULL;
}
