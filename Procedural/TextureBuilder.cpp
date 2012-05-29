#include "../GodComplex.h"

TextureBuilder::TextureBuilder( int _Width, int _Height )
	: m_ppBufferSpecific( NULL )
	, m_Width( _Width )
	, m_Height( _Height )
	, m_bMipLevelsBuilt( false )
{
	m_MipLevelsCount = Texture2D::ComputeMipLevelsCount( _Width, _Height, 0 );
	m_ppBufferGeneric = new NjFloat4*[m_MipLevelsCount];
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		m_ppBufferGeneric[MipLevelIndex] = new NjFloat4[_Width*_Height];
		Texture2D::NextMipSize( _Width, _Height );
	}
}

TextureBuilder::~TextureBuilder()
{
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		delete[] m_ppBufferGeneric[MipLevelIndex];
	delete[] m_ppBufferGeneric;
	ReleaseSpecificBuffer();
}

const void**	TextureBuilder::GetLastConvertedMips() const
{
	ASSERT( m_ppBufferSpecific != NULL, "Invalid final texture buffers ! Did you forget to call Convert() ?" );
	return (const void**) m_ppBufferSpecific;
}

void	CopyFiller( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	const TextureBuilder&	Source = *((const TextureBuilder*) _pData);
	Source.SampleClamp( _UV.x * Source.GetWidth(), _UV.y * Source.GetHeight(), _Color );
}

void	TextureBuilder::CopyFrom( const TextureBuilder& _Source )
{
	Fill( CopyFiller, (void*) &_Source );
}

void	TextureBuilder::Fill( FillDelegate _Filler, void* _pData )
{
	// Fill the mip level 0
	NjFloat2	UV;
	for ( int Y=0; Y < m_Height; Y++ )
	{
		NjFloat4*	pScanline = m_ppBufferGeneric[0] + m_Width * Y;
		UV.y = float(Y) / m_Height;
		for ( int X=0; X < m_Width; X++, pScanline++ )
		{
			UV.x = float(X) / m_Width;
			(*_Filler)( X, Y, UV, *pScanline, _pData );
		}
	}
	m_bMipLevelsBuilt = false;
}

void	TextureBuilder::SampleWrap( float _X, float _Y, NjFloat4& _Color ) const
{
	int		X0 = ASM_floorf( _X );
	float	x = _X - X0;
	float	rx = 1.0f - x;
	int		X1 = (100*m_Width+X0+1) % m_Width;
			X0 = (100*m_Width+X0) % m_Width;

	int		Y0 = ASM_floorf( _Y );
	float	y = _Y - Y0;
	float	ry = 1.0f - y;
	int		Y1 = (100*m_Height+Y0+1) % m_Height;
			Y0 = (100*m_Height+Y0) % m_Height;

	ASSERT( X0 >= 0 && X0 < m_Width && X1 >= 0 && X1 < m_Width, "X out of range !" );
	ASSERT( Y0 >= 0 && Y0 < m_Height && Y1 >= 0 && Y1 < m_Height, "Y out of range !" );
	NjFloat4&	V00 = m_ppBufferGeneric[0][m_Width*Y0+X0];
	NjFloat4&	V01 = m_ppBufferGeneric[0][m_Width*Y0+X1];
	NjFloat4&	V10 = m_ppBufferGeneric[0][m_Width*Y1+X0];
	NjFloat4&	V11 = m_ppBufferGeneric[0][m_Width*Y1+X1];

	NjFloat4	V0 = rx * V00 + x * V01;
	NjFloat4	V1 = rx * V10 + x * V11;

	_Color.x = ry * V0.x + y * V1.x;
	_Color.y = ry * V0.y + y * V1.y;
	_Color.z = ry * V0.z + y * V1.z;
	_Color.w = ry * V0.w + y * V1.w;
}

void	TextureBuilder::SampleClamp( float _X, float _Y, NjFloat4& _Color ) const
{
	int		X0 = ASM_floorf( _X );
	float	x = _X - X0;
	float	rx = 1.0f - x;
	int		X1 = CLAMP( (X0+1), 0, m_Width-1 );
			X0 = CLAMP( X0, 0, m_Width-1 );

	int		Y0 = ASM_floorf( _Y );
	float	y = _Y - Y0;
	float	ry = 1.0f - y;
	int		Y1 = CLAMP( (Y0+1), 0, m_Height-1 );
			Y0 = CLAMP( Y0, 0, m_Height-1 );

	NjFloat4&	V00 = m_ppBufferGeneric[0][m_Width*Y0+X0];
	NjFloat4&	V01 = m_ppBufferGeneric[0][m_Width*Y0+X1];
	NjFloat4&	V10 = m_ppBufferGeneric[0][m_Width*Y1+X0];
	NjFloat4&	V11 = m_ppBufferGeneric[0][m_Width*Y1+X1];

	NjFloat4	V0 = rx * V00 + x * V01;
	NjFloat4	V1 = rx * V10 + x * V11;

	_Color.x = ry * V0.x + y * V1.x;
	_Color.y = ry * V0.y + y * V1.y;
	_Color.z = ry * V0.z + y * V1.z;
	_Color.w = ry * V0.w + y * V1.w;
}

void	TextureBuilder::GenerateMips()
{
	// Build remaining mip levels
	int	Width = m_Width;
	int	Height = m_Height;
	for ( int MipLevelIndex=1; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		int		SourceWidth = Width;
		int		SourceHeight = Height;
		Texture2D::NextMipSize( Width, Height );

		NjFloat4*	pSource = m_ppBufferGeneric[MipLevelIndex-1];
		NjFloat4*	pTarget = m_ppBufferGeneric[MipLevelIndex];
		for ( int Y=0; Y < Height; Y++ )
		{
			int	Y0 = (Y << 1) + 0;

// Phénomène curieux:
// Dans mon programme de test avec le LOD de texture de noise, j'avais un bug où la texture rebouclait bizarrement dans les niveaux de mips supérieurs.
// C'était dû à ce mauvais modulo ci-dessous (idem pour X plus bas).
// Sauf qu'avec ce bug, je tournais à 2000 FPS
// Après l'avoir corrigé, je suis tombé à 700 FPS !!
// Hormis une sorte de "cache de cohérence du contenu des mips", j'ai aucune idée qui me vient à l'esprit pour expliquer cette chute !
//
// On parle ici de framerate qui change à cause du CONTENU d'une texture quand même ! C'est pas rien ! Depuis quand les cartes sont dépendantes du contenu des textures ???
//
#if 0
			int	Y1 = (Y0+1) % Height;	// TODO: Handle WRAP/CLAMP
#else
			int	Y1 = (Y0+1) % SourceHeight;	// TODO: Handle WRAP/CLAMP
#endif

			NjFloat4*	pScanline = pTarget + Width * Y;
			for ( int X=0; X < Width; X++ )
			{
				int	X0 = (X << 1) + 0;
#if 0
				int	X1 = (X0+1) % Width;	// TODO: Handle WRAP/CLAMP
#else
				int	X1 = (X0+1) % SourceWidth;	// TODO: Handle WRAP/CLAMP
#endif

				NjFloat4&	V00 = pSource[SourceWidth*Y0+X0];
				NjFloat4&	V01 = pSource[SourceWidth*Y0+X1];
				NjFloat4&	V10 = pSource[SourceWidth*Y1+X0];
				NjFloat4&	V11 = pSource[SourceWidth*Y1+X1];

				NjFloat4	V = 0.25f * (V00 + V01 + V10 + V11);

				*pScanline++ = V;
			}
		}
	}

	m_bMipLevelsBuilt = true;
}

void**	TextureBuilder::Convert( IPixelFormatDescriptor& _Format )
{
	if ( !m_bMipLevelsBuilt )
		GenerateMips();

	ReleaseSpecificBuffer();

	// Allocate buffers
	m_ppBufferSpecific = new void*[m_MipLevelsCount];
	int	Width = m_Width;
	int	Height = m_Height;
	int	PixelSize = _Format.Size();
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		NjFloat4*	pSource = m_ppBufferGeneric[MipLevelIndex];
		U8*			pTarget = new U8[Width*Height*PixelSize];
		m_ppBufferSpecific[MipLevelIndex] = (void*) pTarget;

		// Copy
		for ( int Y=0; Y < Height; Y++ )
		{
			PixelFormat*	pScanline = (PixelFormat*) &pTarget[PixelSize*Width*Y];
			for ( int X=0; X < Width; X++, pScanline+=PixelSize )
				_Format.Write( *pScanline, *pSource++ );
		}

		// Downscale
		Texture2D::NextMipSize( Width, Height );
	}

	return m_ppBufferSpecific;
}

void	TextureBuilder::ReleaseSpecificBuffer()
{
	if ( m_ppBufferSpecific == NULL )
		return;

	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		delete[] m_ppBufferSpecific[MipLevelIndex];
	delete[] m_ppBufferSpecific;
}
