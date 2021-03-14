#pragma once

#include "Component.h"

// This is the class that is used to pass values to the shader and read back the results
class	StructuredBuffer : public Component {
protected:	// FIELDS

	U32							m_ElementSize;
	U32							m_ElementsCount;
	U32							m_Size;

	ID3D11Buffer*				m_pBuffer;
	ID3D11Buffer*				m_pCPUBuffer;

	ID3D11ShaderResourceView*	m_pShaderView;
	ID3D11UnorderedAccessView*  m_pUnorderedAccessView;

	// Structure to keep track of current inputs/outputs
	mutable int					m_LastAssignedSlots[6];
	mutable int					m_pAssignedToOutputSlot[D3D11_PS_CS_UAV_REGISTER_COUNT];
	static StructuredBuffer*	ms_ppOutputs[D3D11_PS_CS_UAV_REGISTER_COUNT];


public:		// PROPERTIES

	U32			GetElementSize() const		{ return m_ElementSize; }
	U32			GetElementsCount() const	{ return m_ElementsCount; }
	U32			GetSize() const				{ return m_Size; }

	ID3D11ShaderResourceView*	GetShaderView()				{ return m_pShaderView; }
	ID3D11UnorderedAccessView*	GetUnorderedAccessView()	{ return m_pUnorderedAccessView; }

public:		// METHODS

	StructuredBuffer( Device& _device, U32 _elementSize, U32 _elementsCount, bool _writeable, bool _allowRawView );
	~StructuredBuffer();

	// Read/Write for CPU interchange
	void			Read( void* _pData, U32 _elementsCount=~0U ) const;
	void			Write( void* _pData, U32 _elementsCount=~0U );

	// Clear of the unordered access view
	void			Clear( U32 _pValue[4] );
	void			Clear( const bfloat4& _Value );

	// Uploads the buffer to the shader
	void			SetInput( U32 _SlotIndex );
	void			SetOutput( U32 _SlotIndex );

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
	SB( Device& _Device, int _elementsCount, bool _bWriteable ) : m( NULL ), m_pBuffer( NULL ) { Init( _Device, _elementsCount, _bWriteable ); }
	~SB()									{ delete m_pBuffer; delete[] m; }

	void	Init( Device& _Device, int _elementsCount, bool _bWriteable )
	{
		m = new T[_elementsCount];
		m_pBuffer = new StructuredBuffer( _Device, sizeof(T), _elementsCount, _bWriteable );
	}

	void	Read( int _elementsCount=-1 )		{ m_pBuffer->Read( m, _elementsCount ); }
	void	Write( int _elementsCount=-1 )		{ m_pBuffer->Write( m, _elementsCount ); }
	void	Clear( U32 _pValue[4] )				{ m_pBuffer->Clear( _pValue ); }
	void	Clear( const bfloat4& _Value )		{ m_pBuffer->Clear( _Value ); }
	void	SetInput( int _SlotIndex, bool _bIKnowWhatImDoing=false )
	{
		ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );
		m_pBuffer->SetInput( _SlotIndex );
	}
	void	SetOutput( int _SlotIndex )			{ m_pBuffer->SetOutput( _SlotIndex ); }
	void	RemoveFromLastAssignedSlots() const	{ m_pBuffer->RemoveFromLastAssignedSlots(); }
};
