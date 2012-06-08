#include "../GodComplex.h"

Camera::Camera( Device& _Device ) : m_Device( _Device )
{
	m_pCB = new CB<CBData>( m_Device );
	m_pCB->m.Camera2World = m_pCB->m.World2Camera = m_pCB->m.Camera2Proj = m_pCB->m.Proj2Camera = NjFloat4x4::Identity;
}
Camera::~Camera()
{
	delete m_pCB;
}

void	Camera::SetPerspective( float _FOV, float _AspectRatio, float _Near, float _Far )
{
	float	H = tanf( 0.5f * _FOV );
	float	W = _AspectRatio * H;

	m_pCB->m.Params.Set( W, H, _Near, _Far );

	m_pCB->m.Camera2Proj.SetRow( 0, NjFloat4( 1.0f / W, 0.0f, 0.0f, 0.0f ) );
	m_pCB->m.Camera2Proj.SetRow( 1, NjFloat4( 0.0f, 1.0f / H, 0.0f, 0.0f ) );
	m_pCB->m.Camera2Proj.SetRow( 2, NjFloat4( 0.0f, 0.0f, _Far / (_Far - _Near), 1.0f ) );
	m_pCB->m.Camera2Proj.SetRow( 3, NjFloat4( 0.0f, 0.0f, _Near*_Far / (_Near - _Far), 0.0f ) );

	m_pCB->m.Proj2Camera = m_pCB->m.Camera2Proj.Inverse();

	UpdateCompositions();
}

void	Camera::LookAt( const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up )
{
	NjFloat3	Z = _Target - _Position;
	Z.Normalize();

	NjFloat3	X = Z ^ _Up;
	X.Normalize();

	NjFloat3	Y = X ^ Z;

	m_pCB->m.Camera2World.SetRow( 0, X, 0.0f );
	m_pCB->m.Camera2World.SetRow( 1, Y, 0.0f );
	m_pCB->m.Camera2World.SetRow( 2, Z, 0.0f );
	m_pCB->m.Camera2World.SetRow( 3, _Position, 1.0f );

	m_pCB->m.World2Camera = m_pCB->m.Camera2World.Inverse();

	UpdateCompositions();
}

void	Camera::Upload( int _SlotIndex )
{
	m_pCB->UpdateData();
	m_pCB->Set( _SlotIndex );
}

void	Camera::UpdateCompositions()
{
	m_pCB->m.World2Proj = m_pCB->m.World2Camera * m_pCB->m.Camera2Proj;
	m_pCB->m.Proj2World = m_pCB->m.World2Proj.Inverse();

// CHECK
//m_pCB->m.Proj2World = m_pCB->m.Proj2Camera * m_pCB->m.Camera2World;
// CHECK
}