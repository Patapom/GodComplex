#include "stdafx.h"

#include "StructuredBuffer.h"


//////////////////////////////////////////////////////////////////////////
// The structured buffer class
//
StructuredBuffer*	StructuredBuffer::ms_ppOutputs[D3D11_PS_CS_UAV_REGISTER_COUNT] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

StructuredBuffer::StructuredBuffer( Device& _Device, U32 _ElementSize, U32 _ElementsCount, bool _bWriteable, bool _allowRawView )
	: Component( _Device )
{
	ASSERT( _ElementSize > 0, "Buffer must have at least one element!" );
	ASSERT( (_ElementSize&3)==0, "Element size must be a multiple of 4!" );

	for ( int shaderStageIndex=0; shaderStageIndex < 6; shaderStageIndex++ )
		m_LastAssignedSlots[shaderStageIndex] = -1;
	for ( int i=0; i < D3D11_PS_CS_UAV_REGISTER_COUNT; i++ )
		m_pAssignedToOutputSlot[i] = -1;
	m_ElementSize = _ElementSize;
	m_ElementsCount = _ElementsCount;
	m_Size = _ElementSize * _ElementsCount;

	// Create the buffer
	D3D11_BUFFER_DESC   desc;
	desc.ByteWidth = m_Size;
	desc.Usage = D3D11_USAGE_DEFAULT;
	desc.CPUAccessFlags = 0;
	desc.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS;
	desc.MiscFlags = (_allowRawView ? 0 : D3D11_RESOURCE_MISC_BUFFER_STRUCTURED)
					| (_allowRawView ? D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS : 0);
	desc.StructureByteStride = _ElementSize;

	Check( m_device.DXDevice().CreateBuffer( &desc, NULL, &m_pBuffer ) );

	// Create the CPU accessible version of the buffer
	desc.Usage = D3D11_USAGE_STAGING;
	desc.BindFlags = 0;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | (_bWriteable ? D3D11_CPU_ACCESS_WRITE : 0);
	desc.MiscFlags = 0;
	
	Check( m_device.DXDevice().CreateBuffer( &desc, NULL, &m_pCPUBuffer ) );


	//////////////////////////////////////////////////////////////////////////
	// Create the Shader Resource View for the reading
	D3D11_SHADER_RESOURCE_VIEW_DESC	ViewDesc;
	ViewDesc.Format = _allowRawView ? DXGI_FORMAT_R32_TYPELESS : DXGI_FORMAT_UNKNOWN;
	ViewDesc.ViewDimension = D3D11_SRV_DIMENSION_BUFFEREX;
	ViewDesc.BufferEx.FirstElement = 0;
	ViewDesc.BufferEx.NumElements = _ElementsCount;
	ViewDesc.BufferEx.Flags = _allowRawView ? D3D11_BUFFEREX_SRV_FLAG_RAW : 0;

	Check( m_device.DXDevice().CreateShaderResourceView( m_pBuffer, &ViewDesc, &m_pShaderView ) );

 
	// Create the Unordered Access View for the Buffers
	// This is used for writing the buffer during the sort and transpose
	D3D11_UNORDERED_ACCESS_VIEW_DESC	UnorderedViewDesc;
	UnorderedViewDesc.Format = _allowRawView ? DXGI_FORMAT_R32_TYPELESS : DXGI_FORMAT_UNKNOWN;
	UnorderedViewDesc.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
	UnorderedViewDesc.Buffer.FirstElement = 0;
	UnorderedViewDesc.Buffer.NumElements = _ElementsCount;
	UnorderedViewDesc.Buffer.Flags = _allowRawView ? D3D11_BUFFER_UAV_FLAG_RAW : 0;

	Check( m_device.DXDevice().CreateUnorderedAccessView( m_pBuffer, &UnorderedViewDesc, &m_pUnorderedAccessView ) );
}

StructuredBuffer::~StructuredBuffer() {
	m_pUnorderedAccessView->Release();
	m_pShaderView->Release();
	m_pCPUBuffer->Release();
	m_pBuffer->Release();
}

void	StructuredBuffer::Read( void* _pData, U32 _ElementsCount ) const {
	U32	Size = m_ElementSize * (_ElementsCount == ~0U ? m_ElementsCount : _ElementsCount);

	// Copy from actual buffer
	m_device.DXContext().CopyResource( m_pCPUBuffer, m_pBuffer );

	// Read from staging resource
	D3D11_MAPPED_SUBRESOURCE	SubResource;
	Check( m_device.DXContext().Map( m_pCPUBuffer, 0, D3D11_MAP_READ, 0, &SubResource ) );
	ASSERT( SubResource.pData != NULL, "Failed to Map resource for reading!" );

	memcpy( _pData, SubResource.pData, Size );

	m_device.DXContext().Unmap( m_pCPUBuffer, 0 );
}

void	StructuredBuffer::Write( void* _pData, U32 _ElementsCount ) {
	U32	Size = m_ElementSize * (_ElementsCount == ~0U ? m_ElementsCount : _ElementsCount);

	// Write to staging resource
	D3D11_MAPPED_SUBRESOURCE	SubResource;
	Check( m_device.DXContext().Map( m_pCPUBuffer, 0, D3D11_MAP_WRITE, 0, &SubResource ) );
	ASSERT( SubResource.pData != NULL, "Failed to Map resource for writing!" );

	memcpy( SubResource.pData, _pData, Size );

	m_device.DXContext().Unmap( m_pCPUBuffer, 0 );

	// Copy to actual buffer
	m_device.DXContext().CopyResource( m_pBuffer, m_pCPUBuffer );
}

void	StructuredBuffer::Clear( U32 _pValue[4] ) {
	m_device.DXContext().ClearUnorderedAccessViewUint( m_pUnorderedAccessView, _pValue );
}

void	StructuredBuffer::Clear( const bfloat4& _Value ) {
	m_device.DXContext().ClearUnorderedAccessViewFloat( m_pUnorderedAccessView, &_Value.x );
}

void	StructuredBuffer::SetInput( int _SlotIndex ) {
	// Unassign this buffer to any output it was previously bound to
	// NOTE: This mechanism may seem a bit heavy but it's really necessary to avoid scratching one's head too often
	//	when a buffer seems to be empty in the compute shader, whereas it has silently been NOT ASSIGNED AS INPUT
	//	for the only reason that it's still assigned as output somewhere...
	//
	RemoveFromLastAssignedSlotUAV();

	// We can now safely assign it as an input
	ID3D11ShaderResourceView*	pView = GetShaderView();
	m_device.DXContext().VSSetShaderResources( _SlotIndex, 1, &pView );
	m_device.DXContext().HSSetShaderResources( _SlotIndex, 1, &pView );
	m_device.DXContext().DSSetShaderResources( _SlotIndex, 1, &pView );
	m_device.DXContext().GSSetShaderResources( _SlotIndex, 1, &pView );
	m_device.DXContext().PSSetShaderResources( _SlotIndex, 1, &pView );
	m_device.DXContext().CSSetShaderResources( _SlotIndex, 1, &pView );

	m_LastAssignedSlots[0] = _SlotIndex;
	m_LastAssignedSlots[1] = _SlotIndex;
	m_LastAssignedSlots[2] = _SlotIndex;
	m_LastAssignedSlots[3] = _SlotIndex;
	m_LastAssignedSlots[4] = _SlotIndex;
	m_LastAssignedSlots[5] = _SlotIndex;
}

void	StructuredBuffer::SetOutput( int _SlotIndex ) {
#ifdef _DEBUG
	ASSERT( ms_ppOutputs[_SlotIndex] != this, "StructureBuffer already assigned to this output slot! It's only a warning, you can ignore this but you should consider removing this redundant SetOutput() from your code..." );
#endif

	ID3D11UnorderedAccessView*	pView = GetUnorderedAccessView();
	U32							UAVInitialCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &pView, &UAVInitialCount );

	// Remove any previous output buffer
	if ( ms_ppOutputs[_SlotIndex] != NULL )
		ms_ppOutputs[_SlotIndex]->m_pAssignedToOutputSlot[_SlotIndex] = -1;	// Not an output anymore!

	// Store ourselves as the new current output
	ms_ppOutputs[_SlotIndex] = this;					// We are the new output!
	m_pAssignedToOutputSlot[_SlotIndex] = _SlotIndex;	// And we're assigned to that slot
}

void	StructuredBuffer::RemoveFromLastAssignedSlots() const
{
	Device::SHADER_STAGE_FLAGS	pStageFlags[] = {
		Device::SSF_VERTEX_SHADER,
		Device::SSF_HULL_SHADER,
		Device::SSF_DOMAIN_SHADER,
		Device::SSF_GEOMETRY_SHADER,
		Device::SSF_PIXEL_SHADER,
		Device::SSF_COMPUTE_SHADER,
	};
	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		if ( m_LastAssignedSlots[ShaderStageIndex] != -1 ) {
			m_device.RemoveShaderResources( m_LastAssignedSlots[ShaderStageIndex], 1, pStageFlags[ShaderStageIndex] );
			m_LastAssignedSlots[ShaderStageIndex] = -1;
		}
}

void	StructuredBuffer::RemoveFromLastAssignedSlotUAV() const {
	U32							UAVInitialCount = -1;
	ID3D11UnorderedAccessView*	pView = NULL;
	for ( int OutputSlotIndex=0; OutputSlotIndex < D3D11_PS_CS_UAV_REGISTER_COUNT; OutputSlotIndex++ )
		if ( m_pAssignedToOutputSlot[OutputSlotIndex] != -1 )
		{	// We're still assigned to an output...
			m_device.DXContext().CSSetUnorderedAccessViews( OutputSlotIndex, 1, &pView, &UAVInitialCount );
			m_pAssignedToOutputSlot[OutputSlotIndex] = -1;
			ms_ppOutputs[OutputSlotIndex] = NULL;
		}
}
