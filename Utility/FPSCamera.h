#ifdef _DEBUG

class	FPSCamera
{
public:

	Camera&		m_Camera;
	NjFloat3	m_Position;
	NjFloat3	m_Target;
	NjFloat3	m_Up;

	// Button down state
	int			m_ButtonDownMouseX;
	int			m_ButtonDownMouseY;
	NjFloat3	m_ButtonDownPosition;
	NjFloat3	m_ButtonDownTarget;

public:

	FPSCamera( Camera& _Camera, const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up=NjFloat3::UnitY );

	void	Init( const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up=NjFloat3::UnitY );

	// Updates the camera transform
	//	_TranslationSpeed, the speed at which we travel with the keys (in world units per second)
	//	_RotationSpeed, the amount of turns we do when the mouse travels through the entire window
	void	Update( float _DeltaTime, float _TranslationSpeed=1.0f, float _RotationSpeed=1.0f, float _SpeedBoostWithShift=4.0f );
};

#endif