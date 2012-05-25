#pragma once
#include "../Device.h"

class Component
{
protected:  // FIELDS

	Device&		m_Device;
	Component*	m_pPrevious;
	Component*	m_pNext;

public:		// METHODS

	Component( Device& _Device );
	virtual ~Component();

	void		Check( HRESULT _Result ) const;

	friend class Device;
};
