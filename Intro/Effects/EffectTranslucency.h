#pragma once

template<typename> class CB;

class EffectTranslucency
{
public:		// NESTED TYPES

	struct CBObject
	{
		NjFloat4x4	Local2World;	// Local=>World transform to rotate the object
	};


private:	// FIELDS

	int					m_ErrorCode;

	Material*			m_pMatDisplay;	// Some test material for primitive display

	Primitive*			m_pPrimSphereInternal;
	Primitive*			m_pPrimSphereExternal;

	CB<CBObject>*		m_pCB_Object;


public:		// PROPERTIES

	int			GetErrorCode() const	{ return m_ErrorCode; }

public:		// METHODS

	EffectTranslucency();
	~EffectTranslucency();

	void	Render( float _Time, float _DeltaTime );

};