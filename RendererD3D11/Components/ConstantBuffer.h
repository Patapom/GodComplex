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
	void		SetHS( int _SlotIndex );
	void		SetDS( int _SlotIndex );
	void		SetGS( int _SlotIndex );
	void		SetPS( int _SlotIndex );
	void		SetCS( int _SlotIndex );
};

template<typename T> class	CB : public ConstantBuffer
{
protected:	// FIELDS

	int		m_SlotIndex;

public:
	T		m;

public:		// METHODS

	CB( Device& _Device, int _SlotIndex, bool _bIKnowWhatImDoing=false ) : ConstantBuffer( _Device, sizeof(T) ), m_SlotIndex( _SlotIndex )	{ ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved constant buffer slot ! (i.e. all slots [0,9] are reserved for global constants)" ); }
	void		UpdateData()	{ ConstantBuffer::UpdateData( &m ); Set( m_SlotIndex ); }
};