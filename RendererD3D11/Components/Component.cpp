#include "stdafx.h"

#include "Component.h"

Component::Component( Device& _Device ) : m_device( _Device ), m_previous( NULL ), m_next( NULL ), m_tag( NULL ) {
	m_device.RegisterComponent( *this );
}

Component::~Component() {
	m_device.UnRegisterComponent( *this );
}

void	Component::Check( HRESULT _Result ) const {
	m_device.Check( _Result );
}
