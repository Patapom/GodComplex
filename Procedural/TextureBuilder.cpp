#include "../GodComplex.h"

TextureBuilder::TextureBuilder( int _Width, int _Height )
	: m_ppBufferSpecific( NULL )
	, m_Width( _Width )
	, m_Height( _Height )
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

const void**	TextureBuilder::GetMips() const
{
	ASSERT( m_ppBufferSpecific != NULL, "Invalid final texture buffers ! Did you forget to call GenerateMips() ?" );
	return (const void**) m_ppBufferSpecific;
}

void	TextureBuilder::Fill( FillDelegate _Filler )
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
			(*_Filler)( X, Y, UV, *pScanline );
		}
	}

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
}

void*	TextureBuilder::GenerateMips( IPixelFormatDescriptor& _Format )
{
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
