#include "../../../GodComplex.h"
#include "Scene.h"

//////////////////////////////////////////////////////////////////////////
// Scene
Scene::Scene( Device& _Device ) : m_Device( _Device )
{
}

Scene::~Scene()
{
	DestroyObjects();
}

void	Scene::Update( float _Time, float _DeltaTime )
{
	for ( int ObjectIndex=0; ObjectIndex < m_ObjectsCount; ObjectIndex++ )
		if ( m_ppObjects[ObjectIndex] != NULL )
			m_ppObjects[ObjectIndex]->Update( _Time, _DeltaTime );
}

void	Scene::Render( Material& _Material, bool _bDepthPass ) const
{
	for ( int ObjectIndex=0; ObjectIndex < m_ObjectsCount; ObjectIndex++ )
		if ( m_ppObjects[ObjectIndex] != NULL )
			m_ppObjects[ObjectIndex]->Render( _Material, _bDepthPass );
}

void	Scene::AllocateObjects( int _ObjectsCount )
{
	DestroyObjects();

	m_ObjectsCount = _ObjectsCount;
	m_ppObjects = new Object*[m_ObjectsCount];
	for ( int ObjectIndex=0; ObjectIndex < m_ObjectsCount; ObjectIndex++ )
		m_ppObjects[ObjectIndex] = NULL;
}

void	Scene::DestroyObjects()
{
	if ( m_ppObjects == NULL )
		return;

	for ( int ObjectIndex=0; ObjectIndex < m_ObjectsCount; ObjectIndex++ )
		if ( m_ppObjects[ObjectIndex] != NULL )
			delete m_ppObjects[ObjectIndex];

	delete[] m_ppObjects;
	m_ppObjects = NULL;
	m_ObjectsCount = 0;
}

Scene::Object&	Scene::CreateObjectAt( int _ObjectIndex, const char* _pName )
{
	ASSERT( _ObjectIndex < m_ObjectsCount, "Object index out of range!" );

	// Delete any existing object first
	if ( m_ppObjects[_ObjectIndex] != NULL )
		delete m_ppObjects[_ObjectIndex];

	// Let's create our new object
	Object*	pNewObject = new Object( *this, _pName );
	m_ppObjects[_ObjectIndex] = pNewObject;

	return *pNewObject;
}


//////////////////////////////////////////////////////////////////////////
// Object
Scene::Object::Object( Scene& _Owner, const char* _pName )
	: m_Owner( _Owner )
	, m_pName( _pName )
	, m_PrimitivesCount( 0 )
	, m_ppPrimitives( NULL )
	, m_Position( NjFloat3::Zero )
	, m_Rotation( NjFloat3::UnitY, 0.0f )
	, m_Scale( NjFloat3::One )
	, m_bPRSDirty( true )
{
	m_Rotation = NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY );
	m_pCB_Object = new CB<CBObject>( m_Owner.m_Device, 10 );
}
Scene::Object::~Object()
{
	delete m_pCB_Object;
	DestroyPrimitives();
}

void	Scene::Object::SetPRS( const NjFloat3& _Position, const NjFloat4& _Rotation, const NjFloat3& _Scale )
{
	m_Position = _Position;
	m_Rotation = _Rotation;
	m_Scale = _Scale;
	m_bPRSDirty = true;
}

void	Scene::Object::Update( float _Time, float _DeltaTime )
{
	if ( !m_bPRSDirty )
		return;	// Already up to date!

	// Rebuild transform from PRS
	m_pCB_Object->m.Local2World.PRS( m_Position, m_Rotation, m_Scale );
	m_bPRSDirty = false;
}

void	Scene::Object::Render( Material& _Material, bool _bDepthPass ) const
{
	// Update our transform
	m_pCB_Object->UpdateData();

	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ )
		m_ppPrimitives[PrimitiveIndex]->Render( _Material, _bDepthPass );
}

void	Scene::Object::AllocatePrimitives( int _PrimitivesCount )
{
	DestroyPrimitives();

	m_PrimitivesCount = _PrimitivesCount;
	m_ppPrimitives = new Primitive*[_PrimitivesCount];
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ )
		m_ppPrimitives[PrimitiveIndex] = new Primitive( *this );
}

void	Scene::Object::DestroyPrimitives()
{
	if ( m_ppPrimitives == NULL )
		return;

	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ )
		delete m_ppPrimitives[PrimitiveIndex];

	delete[] m_ppPrimitives;
	m_PrimitivesCount = 0;
	m_ppPrimitives = NULL;
}

Scene::Object::Primitive&	Scene::Object::GetPrimitiveAt( int _PrimitiveIndex )
{
	ASSERT( _PrimitiveIndex < m_PrimitivesCount, "Primitive index out of range!" );
	return *m_ppPrimitives[_PrimitiveIndex];
}


//////////////////////////////////////////////////////////////////////////
// Primitive
Scene::Object::Primitive::Primitive( Object& _Owner ) : m_Owner( _Owner )
{
	m_pCB_Primitive = new CB<CBPrimitive>( m_Owner.m_Owner.m_Device, 11 );
}
Scene::Object::Primitive::~Primitive()
{
	delete m_pCB_Primitive;
}

void	Scene::Object::Primitive::Render( Material& _Material, bool _bDepthPass ) const
{
	ASSERT( m_pPrimitive != NULL, "Primitive was not set!" );

	if ( !_bDepthPass )
	{	// Send our primitive infos & textures
		ASSERT( m_pTextures != NULL, "Textures were not set!" );

		m_pCB_Primitive->UpdateData();
		m_pTextures->SetPS( 10 );
	}

	m_pPrimitive->Render( _Material );
}

void	Scene::Object::Primitive::SetRenderPrimitive( ::Primitive& _Primitive )
{
	m_pPrimitive = &_Primitive;
}

void	Scene::Object::Primitive::SetMaterial( MaterialParameters& _Material )
{
	m_pCB_Primitive->m.MatIDs[0] = _Material.MatIDs[0];
	m_pCB_Primitive->m.MatIDs[1] = _Material.MatIDs[1];
	m_pCB_Primitive->m.MatIDs[2] = _Material.MatIDs[2];
	m_pCB_Primitive->m.MatIDs[3] = _Material.MatIDs[3];
	m_pCB_Primitive->m.Thickness = _Material.Thickness;
	m_pCB_Primitive->m.Extinction = _Material.Extinction;	// TODO: Translate extinctions into values depending on thickness
	m_pCB_Primitive->m.IOR = _Material.IOR;

	ASSERT( _Material.pTextures != NULL, "Invalid textures for primitive material!" );
	m_pTextures = _Material.pTextures;
}
