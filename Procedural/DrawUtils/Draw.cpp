#include "../../GodComplex.h"

#define SETINFOS( _x, _y, coverage )\
{\
	Infos.x = _x; Infos.y = _y;	\
	Infos.UV.x = (_x+0.5f) / m_Width; Infos.UV.y = (_y+0.5f) / m_Height;	\
	Infos.Coverage = coverage;	\
}

#define SAFEPIXEL( pPixel )\
	if ( X >= 0 && X < m_Width && Y >= 0 && Y < m_Height ) _Filler( Infos, pPixel )

DrawUtils::DrawUtils()
	: m_pSurface( NULL )
{
}

void	DrawUtils::SetupContext( int _Width, int _Height, NjFloat4* _pSurface )
{
	m_Width = _Width;
	m_Height = _Height;
	m_pSurface = _pSurface;
}

void	DrawUtils::DrawRectangle( float x, float y, float w, float h, float border, FillDelegate _Filler )
{
	ASSERT( m_pSurface != NULL, "Did you forget to call SetupContext() ?" );

	int		X0 = ASM_ceilf( x );
	float	CoverageStartX = X0-x;
	int		Y0 = ASM_ceilf( y );
	float	CoverageStartY = Y0-y;

	float	x2 = x + w;
	int		X1 = ASM_floorf( x2 );
	float	CoverageEndX = x2-X1;
	float	y2 = y + h;
	int		Y1 = ASM_floorf( y2 );
	float	CoverageEndY = y2-Y1;

	DrawInfos	Infos;
	Infos.w = m_Width;
	Infos.h = m_Height;

	// Draw top border
	int	Y = Y0-1;
	{
		Infos.Distance = 0.0f;

		int		X = X0-1;

		Pixel*	pPixel = (Pixel*) m_pSurface + m_Width * Y;

		// Top-left pixel
		SETINFOS( X, Y, MIN( CoverageStartX, CoverageStartY ) );
		SAFEPIXEL( pPixel[X] );
		X++;

		// Top pixels
		for ( ; X <= X1; X++ )
		{
			SETINFOS( X, Y, CoverageStartY );
			SAFEPIXEL( pPixel[X] );
		}

		// Top-right pixel
		SETINFOS( X, Y, MIN( CoverageEndX, CoverageStartY ) );
		SAFEPIXEL( pPixel[X] );
	}
	Y++;

	// Standard filling
	for ( ; Y <= Y1; Y++ )
	{
		int		X = X0-1;

		Pixel*	pPixel = (Pixel*) m_pSurface + m_Width * Y;

		// Left pixel
		Infos.Distance = 0.0f;
		SETINFOS( X, Y, CoverageStartX );
		SAFEPIXEL( pPixel[X] );
		X++;

		// Main pixels
		for ( ; X <= X1; X++ )
		{
			float	Dx0 = (X+0.5f) - x;
			float	Dy0 = (Y+0.5f) - y;
			float	Dx1 = x2 - (X+0.5f);
			float	Dy1 = y2 - (Y+0.5f);
			float	D = MIN( MIN( MIN( Dx0, Dy0 ), Dx1 ), Dy1 );
					D /= border;
			Infos.Distance = D;

			SETINFOS( X, Y, 1.0f );
			SAFEPIXEL( pPixel[X] );
		}

		// Right pixel
		Infos.Distance = 0.0f;
		SETINFOS( X, Y, CoverageEndX );
		SAFEPIXEL( pPixel[X] );
	}

	// Bottom border
	{
		Infos.Distance = 0.0f;

		int		X = X0-1;

		Pixel*	pPixel = (Pixel*) m_pSurface + m_Width * Y;

		// Bottom-left pixel
		SETINFOS( X, Y, MIN( CoverageStartX, CoverageEndY ) );
		SAFEPIXEL( pPixel[X] );
		X++;

		// Bottom pixels
		for ( ; X <= X1; X++ )
		{
			SETINFOS( X, Y, CoverageEndY );
			SAFEPIXEL( pPixel[X] );
		}

		// Bottom-right pixel
		SETINFOS( X, Y, MIN( CoverageEndX, CoverageEndY ) );
		SAFEPIXEL( pPixel[X] );
	}
}

void	DrawUtils::Pixel::Blend( NjFloat4& _Source, float t )
{
	float	r = 1.0f - t;
	RGBA = RGBA * r + _Source * t;
}
