#include "StructuredBuffer.h"


//////////////////////////////////////////////////////////////////////////
// The structured buffer class
//
StructuredBuffer*	StructuredBuffer::ms_ppOutputs[D3D11_PS_CS_UAV_REGISTER_COUNT] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

StructuredBuffer::StructuredBuffer( Device& _Device, int _ElementSize, int _ElementsCount, bool _bWriteable ) : Component( _Device )
{
	ASSERT( (_ElementSize&3)==0, "Element size must be a multiple of 4!" );
	for ( int i=0; i < D3D11_PS_CS_UAV_REGISTER_COUNT; i++ )
		m_pAssignedToOutputSlot[i] = -1;
	m_ElementSize = _ElementSize;
	m_ElementsCount = _ElementsCount;
	m_Size = _ElementSize * _ElementsCount;

	// Create the buffer
	D3D11_BUFFER_DESC   Desc;
	Desc.ByteWidth = m_Size;
	Desc.Usage = D3D11_USAGE_DEFAULT;
	Desc.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS;
	Desc.CPUAccessFlags = 0;
	Desc.MiscFlags = D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
	Desc.StructureByteStride = _ElementSize;

	Check( m_Device.DXDevice().CreateBuffer( &Desc, NULL, &m_pBuffer ) );

	// Create the CPU accessible version of the buffer
	Desc.Usage = D3D11_USAGE_STAGING;
	Desc.BindFlags = 0;
	Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | (_bWriteable ? D3D11_CPU_ACCESS_WRITE : 0);
	
	Check( m_Device.DXDevice().CreateBuffer( &Desc, NULL, &m_pCPUBuffer ) );


	//////////////////////////////////////////////////////////////////////////
	// Create the Shader Resource View for the reading
	D3D11_SHADER_RESOURCE_VIEW_DESC	ViewDesc;
	ViewDesc.Format = DXGI_FORMAT_UNKNOWN;
	ViewDesc.ViewDimension = D3D11_SRV_DIMENSION_BUFFER;
	ViewDesc.Buffer.FirstElement = 0;
	ViewDesc.Buffer.NumElements = _ElementsCount;

	Check( m_Device.DXDevice().CreateShaderResourceView( m_pBuffer, &ViewDesc, &m_pShaderView ) );

 
	// Create the Unordered Access View for the Buffers
	// This is used for writing the buffer during the sort and transpose
	D3D11_UNORDERED_ACCESS_VIEW_DESC	UnorderedViewDesc;
	UnorderedViewDesc.Format = DXGI_FORMAT_UNKNOWN;
	UnorderedViewDesc.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
	UnorderedViewDesc.Buffer.FirstElement = 0;
	UnorderedViewDesc.Buffer.NumElements = _ElementsCount;
	UnorderedViewDesc.Buffer.Flags = 0;

	Check( m_Device.DXDevice().CreateUnorderedAccessView( m_pBuffer, &UnorderedViewDesc, &m_pUnorderedAccessView ) );
}

StructuredBuffer::~StructuredBuffer()
{
	m_pUnorderedAccessView->Release();
	m_pShaderView->Release();
	m_pCPUBuffer->Release();
	m_pBuffer->Release();
}

void	StructuredBuffer::Read( void* _pData, int _ElementsCount ) const
{
	int	Size = m_ElementSize * (_ElementsCount < 0 ? m_ElementsCount : _ElementsCount);

	// Copy from actual buffer
	m_Device.DXContext().CopyResource( m_pCPUBuffer, m_pBuffer );

	// Read from staging resource
	D3D11_MAPPED_SUBRESOURCE	SubResource;
	Check( m_Device.DXContext().Map( m_pCPUBuffer, 0, D3D11_MAP_READ, 0, &SubResource ) );
	ASSERT( SubResource.pData != NULL, "Failed to Map resource for reading !" );

	memcpy( _pData, SubResource.pData, Size );

	m_Device.DXContext().Unmap( m_pCPUBuffer, 0 );
}

void	StructuredBuffer::Write( void* _pData, int _ElementsCount )
{
	int	Size = m_ElementSize * (_ElementsCount < 0 ? m_ElementsCount : _ElementsCount);

	// Write to staging resource
	D3D11_MAPPED_SUBRESOURCE	SubResource;
	Check( m_Device.DXContext().Map( m_pCPUBuffer, 0, D3D11_MAP_WRITE, 0, &SubResource ) );
	ASSERT( SubResource.pData != NULL, "Failed to Map resource for writing !" );

	memcpy( SubResource.pData, _pData, Size );

	m_Device.DXContext().Unmap( m_pCPUBuffer, 0 );

	// Copy to actual buffer
	m_Device.DXContext().CopyResource( m_pBuffer, m_pCPUBuffer );
}

void	StructuredBuffer::Clear( U32 _pValue[4] )
{
	m_Device.DXContext().ClearUnorderedAccessViewUint( m_pUnorderedAccessView, _pValue );
}

void	StructuredBuffer::Clear( const NjFloat4& _Value )
{
	m_Device.DXContext().ClearUnorderedAccessViewFloat( m_pUnorderedAccessView, &_Value.x );
}

void	StructuredBuffer::SetInput( int _SlotIndex )
{
	// Unassign this buffer to any output it was previously bound to
	// NOTE: This mechanism may seem a bit heavy but it's really necessary to avoid scratching one's head too often
	//	when a buffer seems to be empty in the compute shader, whereas it has silently been NOT ASSIGNED AS INPUT
	//	for the only reason that it's still assigned as output somewhere...
	//
	{
		U32							UAVInitialCount = -1;
		ID3D11UnorderedAccessView*	pView = NULL;
		for ( int OutputSlotIndex=0; OutputSlotIndex < D3D11_PS_CS_UAV_REGISTER_COUNT; OutputSlotIndex++ )
			if ( m_pAssignedToOutputSlot[OutputSlotIndex] != -1 )
			{	// We're still assigned to an output...
				m_Device.DXContext().CSSetUnorderedAccessViews( OutputSlotIndex, 1, &pView, &UAVInitialCount );
				m_pAssignedToOutputSlot[OutputSlotIndex] = -1;
				ms_ppOutputs[OutputSlotIndex] = NULL;
			}
	}

	// We can now safely assign it as an input
	ID3D11ShaderResourceView*	pView = GetShaderView();
	m_Device.DXContext().CSSetShaderResources( _SlotIndex, 1, &pView );
}

void	StructuredBuffer::SetOutput( int _SlotIndex )
{
#ifdef _DEBUG
	ASSERT( ms_ppOutputs[_SlotIndex] != this, "StructureBuffer already assigned to this output slot! It's only a warning, you can ignore this but you should consider removing this redundant SetOutput() from your code..." );
#endif

	ID3D11UnorderedAccessView*	pView = GetUnorderedAccessView();
	U32							UAVInitialCount = -1;
	m_Device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &pView, &UAVInitialCount );

	// Remove any previous output buffer
	if ( ms_ppOutputs[_SlotIndex] != NULL )
		ms_ppOutputs[_SlotIndex]->m_pAssignedToOutputSlot[_SlotIndex] = -1;	// Not an output anymore!

	// Store ourselves as the new current output
	ms_ppOutputs[_SlotIndex] = this;					// We are the new output!
	m_pAssignedToOutputSlot[_SlotIndex] = _SlotIndex;	// And we're assigned to that slot
}
