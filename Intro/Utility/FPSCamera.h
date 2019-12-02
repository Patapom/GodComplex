#ifdef _DEBUG

class	FPSCamera
{
public:

	Camera&		m_Camera;
	float3	m_Position;
	float3	m_Target;
	float3	m_Up;

	// Button down state
	int			m_ButtonDownMouseX;
	int			m_ButtonDownMouseY;
	float3	m_ButtonDownPosition;
	float3	m_ButtonDownTarget;

public:

	FPSCamera( Camera& _Camera, const float3& _Position, const float3& _Target, const float3& _Up=float3::UnitY );

	void	Init( const float3& _Position, const float3& _Target, const float3& _Up=float3::UnitY );

	// Updates the camera transform
	//	_TranslationSpeed, the speed at which we travel with the keys (in world units per second)
	//	_RotationSpeed, the amount of turns we do when the mouse travels through the entire window
	void	Update( float _DeltaTime, float _TranslationSpeed=1.0f, float _RotationSpeed=1.0f, float _SpeedBoostWithShift=4.0f );
};

#endif