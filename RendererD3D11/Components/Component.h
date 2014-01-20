#pragma once
#include "../Device.h"

class Component
{
protected:  // FIELDS

	Device&		m_Device;
	Component*	m_pPrevious;
	Component*	m_pNext;

public:

	void*		m_pTag;		// User tag

public:		// METHODS

	Component( Device& _Device );
	virtual ~Component();

	Device&		GetDevice()	{ return m_Device; }
	void		Check( HRESULT _Result ) const;

	friend class Device;
};
