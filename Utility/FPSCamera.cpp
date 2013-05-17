#ifdef _DEBUG

#include "../GodComplex.h"

FPSCamera::FPSCamera( Camera& _Camera, const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up )
	: m_Camera( _Camera )
{
	Init( _Position, _Target, _Up );
}

void	FPSCamera::Init( const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up )
{
	m_Position = _Position;
	m_Target = _Target;
	m_Up = _Up;

	m_Camera.LookAt( m_Position, m_Target, m_Up );
}

void	FPSCamera::Update( float _DeltaTime, float _TranslationSpeed, float _RotationSpeed, float _SpeedBoostWithShift )
{

	//////////////////////////////////////////////////////////////////////////
	// Handle mouse manipulation
	//
	if ( gs_WindowInfos.Events.Mouse.dbuttons[0] > 0 )
	{	// Button down
		SetCapture( gs_WindowInfos.hWnd );

		m_ButtonDownMouseX = gs_WindowInfos.Events.Mouse.x;
		m_ButtonDownMouseY = gs_WindowInfos.Events.Mouse.y;
		m_ButtonDownPosition = m_Position;
		m_ButtonDownTarget = m_Target;
	}
	else if ( gs_WindowInfos.Events.Mouse.dbuttons[0] < 0 )
	{	// Button up
		ReleaseCapture();
	}

	if ( gs_WindowInfos.Events.Mouse.buttons[0] != 0 )
	{
		int		MouseDx = gs_WindowInfos.Events.Mouse.x - m_ButtonDownMouseX;
		int		MouseDy = gs_WindowInfos.Events.Mouse.y - m_ButtonDownMouseY;
		float	DAngleX = (_RotationSpeed * TWOPI) * MouseDx / RESX;
		float	DAngleY = (_RotationSpeed * PI) * MouseDy / RESY;

		NjFloat3	At = m_ButtonDownTarget - m_ButtonDownPosition;
		float		Distance2Target = At.Length();
		At = At / Distance2Target;

		float	Theta = asinf( At.y );
		float	Phi = atan2f( At.x, At.z );

		Theta = CLAMP( Theta - DAngleY, -0.99f * HALFPI, +0.99f * HALFPI );	// Never completly up or down to avoid gimbal lock
		Phi -= DAngleX;

		NjFloat3	NewAt( sinf(Phi)*cosf(Theta), sinf(Theta), cosf(Phi)*cos(Theta) );

		m_Target = m_Position + Distance2Target * NewAt;

// 		Vector3	Euler = GetEuler( m_ButtonDownTransform );
// 		Matrix	CamRotYMatrix = Matrix.RotationY( fAngleY + Euler.Y );
// 		Matrix	CamRotXMatrix = Matrix.RotationX( fAngleX + Euler.X );
// 		Matrix	CamRotZMatrix = Matrix.RotationZ( Euler.Z );
// 
// 		Matrix	RotateMatrix = CamRotXMatrix * CamRotYMatrix * CamRotZMatrix;
	}

	NjFloat3	At = (m_Target - m_Position).Normalize();
	NjFloat3	Right = (At ^ m_Up).Normalize();
	NjFloat3	Up = Right ^ At;

	//////////////////////////////////////////////////////////////////////////
	// Handle keyboard manipulation
	//
	float		Speed = _DeltaTime * _TranslationSpeed;
	if ( gs_WindowInfos.Events.Keyboard.State[KEY_LSHIFT] )
		Speed *= _SpeedBoostWithShift;

	NjFloat3	Delta = NjFloat3::Zero;
	if ( gs_WindowInfos.pKeys['Q'] )
	{	// Strafe left
		Delta = -Speed * Right;
	}
	if ( gs_WindowInfos.pKeys['D'] )
	{	// Strafe right
		Delta = +Speed * Right;
	}
	if ( gs_WindowInfos.pKeys['Z'] )
	{	// Forward
		Delta = +Speed * At;
	}
	if ( gs_WindowInfos.pKeys['S'] )
	{	// Backward
		Delta = -Speed * At;
	}
	if ( gs_WindowInfos.pKeys[' '] )
	{	// Up
		Delta = +Speed * Up;
	}
	if ( gs_WindowInfos.Events.Keyboard.State[KEY_LCONTROL] )
	{	// Down
		Delta = -Speed * Up;
	}

	m_Position = m_Position + Delta;
	m_Target = m_Target + Delta;

	//////////////////////////////////////////////////////////////////////////
	// Rebuild camera matrix
	m_Camera.LookAt( m_Position, m_Target, m_Up );
}

#endif