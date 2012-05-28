//////////////////////////////////////////////////////////////////////////
// Helps to build a texture and its mip levels to provide a valid buffer when constructing a Texture2D
//
#pragma once

class	TextureBuilder
{
protected:	// CONSTANTS

public:		// NESTED TYPES

	typedef void	(*FillDelegate)( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData );

protected:	// FIELDS

	int			m_Width;
	int			m_Height;
	int			m_MipLevelsCount;
	bool		m_bMipLevelsBuilt;

	NjFloat4**	m_ppBufferGeneric;		// Generic buffer consisting of float4
	void**		m_ppBufferSpecific;		// Specific buffer of given pixel format


public:		// PROPERTIES

	int				GetWidth() const	{ return m_Width; }
	int				GetHeight() const	{ return m_Height; }

	NjFloat4**		GetMips()	{ return m_ppBufferGeneric; }
	const void**	GetLastConvertedMips() const;

public:		// METHODS

	TextureBuilder( int _Width, int _Height );
 	~TextureBuilder();

	void	Fill( FillDelegate _Filler, void* _pData );
	void	SampleWrap( float _X, float _Y, NjFloat4& _Color );
	void	SampleClamp( float _X, float _Y, NjFloat4& _Color );
	void	GenerateMips();
	void**	Convert( IPixelFormatDescriptor& _Format );

private:
	void	ReleaseSpecificBuffer();
};
