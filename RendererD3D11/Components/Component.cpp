#include "Component.h"

Component::Component( Device& _Device ) : m_Device( _Device ), m_pPrevious( NULL ), m_pNext( NULL ), m_pTag( NULL )
{
	m_Device.RegisterComponent( *this );
}

Component::~Component()
{
	m_Device.UnRegisterComponent( *this );
}

void	Component::Check( HRESULT _Result ) const
{
	m_Device.Check( _Result );
}
