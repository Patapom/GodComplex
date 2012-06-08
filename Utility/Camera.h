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
		NjFloat4	Params;				// X=AspectRaio*tan(FOV/2)  Y=tan(FOV/2)  Z=Near  W=Far
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
 
// 	// Gets or sets vertical field of view (in radians)
// 	float		GetFOV() const					{ return m_FOV; }
// 	void		SetFOV( float value )			{ m_FOV = value; UpdateProjection(); }
// 
// 	// Gets or sets near & far clip distances
// 	float		GetNear() const					{ return m_Near; }
// 	void		SetNear( float value )			{ m_Near = value; UpdateProjection(); }
// 	float		GetFar() const					{ return m_Far; }
// 	void		SetFar( float value )			{ m_Far = value; UpdateProjection(); }
// 
// 	// Gets or sets screen aspect ratio
// 	float		GetAspectRatio() const			{ return m_FOV; }
// 	void		SetAspectRatio( float value )	{ m_AspectRatio = value; UpdateProjection(); }

// 	// Gets the constant buffer to send to shaders
// 	const ConstantBuffer&	GetCB() const	{ return *m_pCB; }

public:		// METHODS

	Camera( Device& _Device );
	~Camera();

	void	Upload( int _SlotIndex );

	void	SetPerspective( float _FOV, float _AspectRatio, float _Near, float _Far );
	void	LookAt( const NjFloat3& _Position, const NjFloat3& _Target, const NjFloat3& _Up );

private:

	void	UpdateCompositions();
};