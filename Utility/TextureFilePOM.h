//////////////////////////////////////////////////////////////////////////
// Loads & saves the POM format
//
#pragma once

#define POM_FORMAT_SUPPORT

class Device;
class IPixelFormatDescriptor;
class Texture2D;
class Texture3D;

class	TextureFilePOM
{
public:		// NESTED TYPES

	enum	TEXTURE_TYPE
	{
		TEX_2D = 0,		// 2D
		TEX_CUBE = 1,	// CUBE
		TEX_3D = 2,		// 3D
	};

	struct	MipDescriptor
	{
		int					RowPitch;
		int					DepthPitch;
	};

public:		// FIELDS

	TEXTURE_TYPE			m_Type;
	int						m_Width;
	int						m_Height;
	int						m_ArraySizeOrDepth;
	int						m_MipsCount;
	const IPixelFormatDescriptor*	m_pPixelFormat;
	void**					m_ppContent;
	MipDescriptor*			m_pMipsDescriptors;

public:		// PROPERTIES
 

public:		// METHODS

	TextureFilePOM();
	TextureFilePOM( const char* _pFileName );
	~TextureFilePOM();

	void	Load( const char* _pFileName );
	void	Save( const char* _pFileName );

	// Used by Texture2D/Texture3D to store their mapped content
	void	AllocateContent( Texture2D& _Texture );
	void	AllocateContent( Texture3D& _Texture );

private:
	void	ReleasContent();
};