#pragma once
#include "../Device.h"

class Component
{
protected:  // FIELDS

	Device&		m_device;
	Component*	m_previous;
	Component*	m_next;

public:

	void*		m_tag;		// User tag

public:		// METHODS

	Component( Device& _Device );
	virtual ~Component();

	Device&		GetDevice()	{ return m_device; }
	void		Check( HRESULT _Result ) const;

	friend class Device;
};
