#pragma once

#include "Component.h"

class ConstantBuffer : public Component
{
private:	// FIELDS

	int				m_Size;
	int				m_PaddedSize;

	ID3D11Buffer*   m_pBuffer;


public:	 // PROPERTIES

	ID3D11Buffer*	GetBuffer()		{ return m_pBuffer; }


public:	 // METHODS

	ConstantBuffer( Device& _Device, int _Size, void* _pData=NULL );
	~ConstantBuffer();

	void		UpdateData( const void* _pData );

	void		Set( int _SlotIndex );
	void		SetVS( int _SlotIndex );
	void		SetGS( int _SlotIndex );
	void		SetPS( int _SlotIndex );
};

