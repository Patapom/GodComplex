#include "../GodComplex.h"

Camera::Camera( Device& _Device ) : m_Device( _Device )
{
	m_pCB = new CB<CBData>( m_Device, 0 );
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

	float	Q =  _Far / (_Far - _Near);

	m_pCB->m.Camera2Proj.SetRow( 0, NjFloat4( 1.0f / W, 0.0f, 0.0f, 0.0f ) );
	m_pCB->m.Camera2Proj.SetRow( 1, NjFloat4( 0.0f, 1.0f / H, 0.0f, 0.0f ) );
	m_pCB->m.Camera2Proj.SetRow( 2, NjFloat4( 0.0f, 0.0f, Q, 1.0f ) );
	m_pCB->m.Camera2Proj.SetRow( 3, NjFloat4( 0.0f, 0.0f, -_Near * Q, 0.0f ) );

	m_pCB->m.Proj2Camera = m_pCB->m.Camera2Proj.Inverse();

// IDENTITY CHECK
//NjFloat4x4	I = m_pCB->m.Proj2Camera * m_pCB->m.Camera2Proj;
// IDENTITY CHECK

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

// IDENTITY CHECK
//NjFloat4x4	I = m_pCB->m.World2Camera * m_pCB->m.Camera2World;
// IDENTITY CHECK

	UpdateCompositions();
}

void	Camera::Upload( int _SlotIndex )
{
	m_pCB->UpdateData();
}

void	Camera::UpdateCompositions()
{
	m_pCB->m.World2Proj = m_pCB->m.World2Camera * m_pCB->m.Camera2Proj;
	m_pCB->m.Proj2World = m_pCB->m.World2Proj.Inverse();

// CHECKS
//m_pCB->m.Proj2World = m_pCB->m.Proj2Camera * m_pCB->m.Camera2World;
// 
//NjFloat4	T2 = NjFloat4( 0, 0, 0, 1 ) * m_pCB->m.World2Camera;
// 
//NjFloat4	T0 = NjFloat4( 0, 0, 9.95f, 1 ) * m_pCB->m.World2Proj;
// T0 = T0 / T0.w;
// 
//NjFloat4	T1 = NjFloat4( 5, 5, 0, 1 ) * m_pCB->m.World2Proj;
// T1 = T1 / T1.w;
// CHECKS
}