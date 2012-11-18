//////////////////////////////////////////////////////////////////////////
// Camera class helper
// Supports only perspective cameras because nobody gives a shit about orthographic projections anyway
//
// A typical camera matrix looks like this :
// 
//     Y (Up)
//     ^
//     |    Z (At)
//     |   /
//     |  /
//     | /
//     |/
//     o---------> X (Right)
//
//
// Notice that the Z vector points AT the target and is in opposite direction compared to normal object matrices !
//
#pragma once

class Device;
class ConstantBuffer;
template<typename> class CB;

class	Camera
{
private:	// NESTED TYPES

	struct	CBData
	{
		NjFloat4	Params;				// X=AspectRatio*tan(FOV/2)  Y=tan(FOV/2)  Z=Near  W=Far
		NjFloat4x4	Camera2World;
		NjFloat4x4	World2Camera;
		NjFloat4x4	Camera2Proj;
		NjFloat4x4	Proj2Camera;
		NjFloat4x4	World2Proj;
		NjFloat4x4	Proj2World;
	};

private:	// FIELDS

	Device&			m_Device;
	CB<CBData>*		m_pCB;

	NjFloat3		m_Position;
	NjFloat3		m_Target;

public:		// PROPERTIES
 
	// Gets the constant buffer to send to shaders
	CBData&	GetCB();

public:		// METHODS

	Camera( Device& _Device );
	~Camera();

	void	Upload( int _SlotIndex );

	void	SetPerspective( float _FOV, float _AspectRatio, float _Near, float _Far );
	void	LookAt( const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up );
	void	UpdateCompositions();
};