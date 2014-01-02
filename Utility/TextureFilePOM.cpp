#include "../GodComplex.h"

TextureFilePOM::TextureFilePOM()
	: m_Width( 0 )
	, m_Height( 0 )
	, m_ArraySizeOrDepth( 0 )
	, m_MipsCount( 0 )
	, m_pPixelFormat( NULL )
	, m_ppContent( NULL )
	, m_pMipsDescriptors( NULL )
{
}
TextureFilePOM::TextureFilePOM( const char* _pFileName )
	: m_Width( 0 )
	, m_Height( 0 )
	, m_ArraySizeOrDepth( 0 )
	, m_MipsCount( 0 )
	, m_pPixelFormat( NULL )
	, m_ppContent( NULL )
	, m_pMipsDescriptors( NULL )
{
	Load( _pFileName );
}

TextureFilePOM::~TextureFilePOM()
{
	ReleasContent();
}

void	TextureFilePOM::Load( const char* _pFileName )
{
	ReleasContent();

	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "rb" );
	ASSERT( pFile != NULL, "Can't load POM file!" );

	// Read the type and format
	U8		Format;
	fread_s( &m_Type, sizeof(U8), sizeof(U8), 1, pFile );
	fread_s( &Format, sizeof(U8), sizeof(U8), 1, pFile );
	DXGI_FORMAT	PixelFormat = DXGI_FORMAT( Format );

	switch ( PixelFormat )
	{
	case DXGI_FORMAT_R8_UNORM:				m_pPixelFormat = &PixelFormatR8::DESCRIPTOR; break;
	case DXGI_FORMAT_R8G8B8A8_UNORM:		m_pPixelFormat = &PixelFormatRGBA8::DESCRIPTOR; break;
	case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:	m_pPixelFormat = &PixelFormatRGBA8_sRGB::DESCRIPTOR; break;
	case DXGI_FORMAT_R16_FLOAT:				m_pPixelFormat = &PixelFormatR16F::DESCRIPTOR; break;
	case DXGI_FORMAT_R16_UNORM:				m_pPixelFormat = &PixelFormatR16_UNORM::DESCRIPTOR; break;
	case DXGI_FORMAT_R16G16_FLOAT:			m_pPixelFormat = &PixelFormatRG16F::DESCRIPTOR; break;
	case DXGI_FORMAT_R16G16B16A16_UINT:		m_pPixelFormat = &PixelFormatRGBA16_UINT::DESCRIPTOR; break;
	case DXGI_FORMAT_R16G16B16A16_UNORM:	m_pPixelFormat = &PixelFormatRGBA16_UNORM::DESCRIPTOR; break;
	case DXGI_FORMAT_R16G16B16A16_FLOAT:	m_pPixelFormat = &PixelFormatRGBA16F::DESCRIPTOR; break;
	case DXGI_FORMAT_R32_FLOAT:				m_pPixelFormat = &PixelFormatR32F::DESCRIPTOR; break;
	case DXGI_FORMAT_R32G32_FLOAT:			m_pPixelFormat = &PixelFormatRG32F::DESCRIPTOR; break;
	case DXGI_FORMAT_R32G32B32A32_FLOAT:	m_pPixelFormat = &PixelFormatRGBA32F::DESCRIPTOR; break;
	}
	ASSERT( m_pPixelFormat != NULL, "Unsupported pixel format!" );

	// Read the dimensions
	fread_s( &m_Width, sizeof(U32), sizeof(U32), 1, pFile );
	fread_s( &m_Height, sizeof(U32), sizeof(U32), 1, pFile );
	fread_s( &m_ArraySizeOrDepth, sizeof(U32), sizeof(U32), 1, pFile );
	fread_s( &m_MipsCount, sizeof(U32), sizeof(U32), 1, pFile );

	int	ContentBuffersCount = m_Type == TEX_3D ? m_MipsCount : m_MipsCount*m_ArraySizeOrDepth;
	m_ppContent = new void*[ContentBuffersCount];
	m_pMipsDescriptors = new MipDescriptor[m_MipsCount];

	// Read each mip
	int	Depth = m_ArraySizeOrDepth;
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
	{
		fread_s( &m_pMipsDescriptors[MipLevelIndex].RowPitch, sizeof(U32), sizeof(U32), 1, pFile );
		fread_s( &m_pMipsDescriptors[MipLevelIndex].DepthPitch, sizeof(U32), sizeof(U32), 1, pFile );

		if ( m_Type != TEX_3D )
		{
			for ( int SliceIndex=0; SliceIndex < m_ArraySizeOrDepth; SliceIndex++ )
			{
				m_ppContent[MipLevelIndex+m_MipsCount*SliceIndex] = new void*[m_pMipsDescriptors[MipLevelIndex].DepthPitch];
				fread_s( m_ppContent[MipLevelIndex+m_MipsCount*SliceIndex], m_pMipsDescriptors[MipLevelIndex].DepthPitch, m_pMipsDescriptors[MipLevelIndex].DepthPitch, 1, pFile );
			}
		}
		else
		{
			m_ppContent[MipLevelIndex] = new void*[Depth * m_pMipsDescriptors[MipLevelIndex].DepthPitch];
			fread_s( m_ppContent[MipLevelIndex], Depth * m_pMipsDescriptors[MipLevelIndex].DepthPitch, Depth * m_pMipsDescriptors[MipLevelIndex].DepthPitch, 1, pFile );
		}

		Depth = MAX( 1, Depth >> 1 );
	}

	// We're done!
	fclose( pFile );
}

void	TextureFilePOM::Save( const char* _pFileName )
{
	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "wb" );
	ASSERT( pFile != NULL, "Can't create file!" );

	// Write the type and format
	U8		Format = U32(m_pPixelFormat->DirectXFormat()) & 0xFF;
	fwrite( &m_Type, sizeof(U8), 1, pFile );
	fwrite( &Format, sizeof(U8), 1, pFile );

	// Write the dimensions
	fwrite( &m_Width, sizeof(U32), 1, pFile );
	fwrite( &m_Height, sizeof(U32), 1, pFile );
	fwrite( &m_ArraySizeOrDepth, sizeof(U32), 1, pFile );
	fwrite( &m_MipsCount, sizeof(U32), 1, pFile );

	// Write each mip
	int	Depth = m_ArraySizeOrDepth;
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
	{
		fwrite( &m_pMipsDescriptors[MipLevelIndex].RowPitch, sizeof(U32), 1, pFile );
		fwrite( &m_pMipsDescriptors[MipLevelIndex].DepthPitch, sizeof(U32), 1, pFile );

		if ( m_Type != TEX_3D )
		{
			for ( int SliceIndex=0; SliceIndex < m_ArraySizeOrDepth; SliceIndex++ )
				fwrite( m_ppContent[MipLevelIndex+m_MipsCount*SliceIndex], m_pMipsDescriptors[MipLevelIndex].DepthPitch, 1, pFile );
		}
		else
			fwrite( m_ppContent[MipLevelIndex], Depth * m_pMipsDescriptors[MipLevelIndex].DepthPitch, 1, pFile );

		Depth = MAX( 1, Depth >> 1 );
	}

	// We're done!
	fclose( pFile );
}

void	TextureFilePOM::AllocateContent( Texture2D& _Texture )
{
	ReleasContent();

	m_Width = _Texture.GetWidth();
	m_Height = _Texture.GetHeight();
	m_ArraySizeOrDepth = _Texture.GetArraySize();
	m_MipsCount = _Texture.GetMipLevelsCount();
	m_Type = _Texture.IsCubeMap() ? TEX_CUBE : TEX_2D;
	m_pPixelFormat = (const IPixelFormatDescriptor*) &_Texture.GetFormatDescriptor();

	m_pMipsDescriptors = new MipDescriptor[m_MipsCount];
	m_ppContent = new void*[m_ArraySizeOrDepth*m_MipsCount];
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
		for ( int SliceIndex=0; SliceIndex < m_ArraySizeOrDepth; SliceIndex++ )
			m_ppContent[MipLevelIndex+m_MipsCount*SliceIndex] = NULL;	// We can't allocate anything there since we don't know the row/depth pitch of the texture unless we map it!
}

void	TextureFilePOM::AllocateContent( Texture3D& _Texture )
{
	ReleasContent();

	m_Width = _Texture.GetWidth();
	m_Height = _Texture.GetHeight();
	m_ArraySizeOrDepth = _Texture.GetDepth();
	m_MipsCount = _Texture.GetMipLevelsCount();
	m_Type = TEX_3D;
	m_pPixelFormat = (const IPixelFormatDescriptor*) &_Texture.GetFormatDescriptor();

	m_pMipsDescriptors = new MipDescriptor[m_MipsCount];
	m_ppContent = new void*[m_MipsCount];
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
		m_ppContent[MipLevelIndex] = NULL;	// We can't allocate anything there since we don't know the row/depth pitch of the texture unless we map it!
}

void	TextureFilePOM::ReleasContent()
{
	if ( m_Type != TEX_3D )
	{	// Release each slice in each mip
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
			for ( int SliceIndex=0; SliceIndex < m_ArraySizeOrDepth; SliceIndex++ )
				delete[] m_ppContent[MipLevelIndex+m_MipsCount*SliceIndex];
	}
	else
	{	// Release each mip
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipsCount; MipLevelIndex++ )
			delete[] m_ppContent[MipLevelIndex];
	}
	delete[] m_ppContent;
	m_ppContent = NULL;

	delete[] m_pMipsDescriptors;
	m_pMipsDescriptors = NULL;
}
