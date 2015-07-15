#pragma once

#include "Component.h"

// This is the class that is used to pass values to the shader and read back the results
class	StructuredBuffer : public Component
{
protected:	// FIELDS

	int							m_ElementSize;
	int							m_ElementsCount;
	int							m_Size;

	ID3D11Buffer*				m_pBuffer;
	ID3D11Buffer*				m_pCPUBuffer;

	ID3D11ShaderResourceView*	m_pShaderView;
	ID3D11UnorderedAccessView*  m_pUnorderedAccessView;

	// Structure to keep track of current inputs/outputs
	mutable int					m_LastAssignedSlots[6];
	int							m_pAssignedToOutputSlot[D3D11_PS_CS_UAV_REGISTER_COUNT];
	static StructuredBuffer*	ms_ppOutputs[D3D11_PS_CS_UAV_REGISTER_COUNT];


public:		// PROPERTIES

	int				GetElementSize() const		{ return m_ElementSize; }
	int				GetElementsCount() const	{ return m_ElementsCount; }
	int				GetSize() const				{ return m_Size; }

	ID3D11ShaderResourceView*	GetShaderView()				{ return m_pShaderView; }
	ID3D11UnorderedAccessView*	GetUnorderedAccessView()	{ return m_pUnorderedAccessView; }

public:		// METHODS

	StructuredBuffer( Device& _Device, int _ElementSize, int _ElementsCount, bool _bWriteable );
	~StructuredBuffer();

	// Read/Write for CPU interchange
	void			Read( void* _pData, int _ElementsCount=-1 ) const;
	void			Write( void* _pData, int _ElementsCount=-1 );

	// Clear of the unordered access view
	void			Clear( U32 _pValue[4] );
	void			Clear( const float4& _Value );

	// Uploads the buffer to the shader
	void			SetInput( int _SlotIndex );
	void			SetOutput( int _SlotIndex );

	// Removes the structured buffer from any last assigned SRV slots
	void			RemoveFromLastAssignedSlots() const;
	void			RemoveFromLastAssignedSlotUAV() const;
};


//////////////////////////////////////////////////////////////////////////
// Helper class to easily manipulate structured buffers
//
template<typename T> class	SB
{
public:		// FIELDS

	T*					m;

protected:

	StructuredBuffer*	m_pBuffer;

public:		// PROPERTIES

	int							GetElementSize() const		{ return sizeof(T); }
	int							GetElementsCount() const	{ return m_pBuffer->GetElementsCount(); }
	int							GetSize() const				{ return m_pBuffer->GetSize(); }
	ID3D11ShaderResourceView*	GetShaderView()				{ return m_pBuffer->GetShaderView(); }
	ID3D11UnorderedAccessView*	GetUnorderedAccessView()	{ return m_pBuffer->GetUnorderedAccessView(); }

public:		// METHODS

	SB() : m( NULL ), m_pBuffer( NULL )		{}
	SB( Device& _Device, int _ElementsCount, bool _bWriteable ) : m( NULL ), m_pBuffer( NULL ) { Init( _Device, _ElementsCount, _bWriteable ); }
	~SB()									{ delete m_pBuffer; delete[] m; }

	void	Init( Device& _Device, int _ElementsCount, bool _bWriteable )
	{
		m = new T[_ElementsCount];
		m_pBuffer = new StructuredBuffer( _Device, sizeof(T), _ElementsCount, _bWriteable );
	}

	void	Read( int _ElementsCount=-1 )		{ m_pBuffer->Read( m, _ElementsCount ); }
	void	Write( int _ElementsCount=-1 )		{ m_pBuffer->Write( m, _ElementsCount ); }
	void	Clear( U32 _pValue[4] )				{ m_pBuffer->Clear( _pValue ); }
	void	Clear( const float4& _Value )		{ m_pBuffer->Clear( _Value ); }
	void	SetInput( int _SlotIndex, bool _bIKnowWhatImDoing=false )
	{
		ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );
		m_pBuffer->SetInput( _SlotIndex );
	}
	void	SetOutput( int _SlotIndex )			{ m_pBuffer->SetOutput( _SlotIndex ); }
	void	RemoveFromLastAssignedSlots() const	{ m_pBuffer->RemoveFromLastAssignedSlots(); }
};
