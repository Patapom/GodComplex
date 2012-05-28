#include "../../GodComplex.h"

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

void	DrawUtils::DrawRectangle( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler )
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

	m_Infos.w = m_Width;
	m_Infos.h = m_Height;

	// Add border bias
	m_ContextRECT.x0 = x + bias * border;
	m_ContextRECT.y0 = y + bias * border;
	m_ContextRECT.x1 = x2 - bias * border;
	m_ContextRECT.y1 = y2 - bias * border;
	m_ContextRECT.InvBorderSize = border != 0.0f ? 1.0f / border : 0.0f;
	m_ContextRECT.pFiller = _Filler;

	// Draw top border
	m_ContextRECT.Y = Y0-1;
	{
		m_ContextRECT.X = X0-1;
		m_ContextRECT.pScanline = (Pixel*) m_pSurface + m_Width * m_ContextRECT.Y;

		// Top-left pixel
		SetInfosRECT( MIN( CoverageStartX, CoverageStartY ) );
		DrawSafePixel();
		m_ContextRECT.X++;

		// Top pixels
		for ( ; m_ContextRECT.X <= X1; m_ContextRECT.X++ )
		{
			SetInfosRECT( CoverageStartY );
			DrawSafePixel();
		}

		// Top-right pixel
		SetInfosRECT( MIN( CoverageEndX, CoverageStartY ) );
		DrawSafePixel();
	}
	m_ContextRECT.Y++;

	// Standard filling
	for ( ; m_ContextRECT.Y <= Y1; m_ContextRECT.Y++ )
	{
		m_ContextRECT.X = X0-1;
		m_ContextRECT.pScanline = (Pixel*) m_pSurface + m_Width * m_ContextRECT.Y;

		// Left pixel
		SetInfosRECT( CoverageStartX );
		DrawSafePixel();
		m_ContextRECT.X++;

		// Main pixels
		for ( ; m_ContextRECT.X <= X1; m_ContextRECT.X++ )
		{
			SetInfosRECT( 1.0f );
			DrawSafePixel();
		}

		// Right pixel
		SetInfosRECT( CoverageEndX );
		DrawSafePixel();
	}

	// Bottom border
	{
		m_ContextRECT.X = X0-1;
		m_ContextRECT.pScanline = (Pixel*) m_pSurface + m_Width * m_ContextRECT.Y;

		// Bottom-left pixel
		SetInfosRECT( MIN( CoverageStartX, CoverageEndY ) );
		DrawSafePixel();
		m_ContextRECT.X++;

		// Bottom pixels
		for ( ; m_ContextRECT.X <= X1; m_ContextRECT.X++ )
		{
			SetInfosRECT( CoverageEndY );
			DrawSafePixel();
		}

		// Bottom-right pixel
		SetInfosRECT( MIN( CoverageEndX, CoverageEndY ) );
		DrawSafePixel();
	}
}

void	DrawUtils::SetInfosRECT( float _Coverage )
{
	float	fX = 0.5f + m_ContextRECT.X;
	float	fY = 0.5f + m_ContextRECT.Y;

	m_Infos.x = m_ContextRECT.X;
	m_Infos.y = m_ContextRECT.Y;
	m_Infos.UV.x = fX / m_Width;
	m_Infos.UV.y = fY / m_Height;
	m_Infos.Coverage = _Coverage;

	// Compute signed distance to border
	float	Dx0 = fX - m_ContextRECT.x0;
	float	Dy0 = fY - m_ContextRECT.y0;
	float	Dx1 = m_ContextRECT.x1 - fX;
	float	Dy1 = m_ContextRECT.y1 - fY;

	bool	bOutside = Dx0 < 0.0f || Dy0 < 0.0f || Dx1 < 0.0f || Dy1 < 0.0f;
	float	D;
//* Comment this part to use manhattan distance instead of cartesian
	if ( bOutside )
	{
		float	Bx = fX < m_ContextRECT.x0 ? m_ContextRECT.x0 : (fX > m_ContextRECT.x1 ? m_ContextRECT.x1 : fX);
		float	By = fY < m_ContextRECT.y0 ? m_ContextRECT.y0 : (fY > m_ContextRECT.y1 ? m_ContextRECT.y1 : fY);
		float	Dx = fX - Bx;
		float	Dy = fY - By;
		D = sqrtf( Dx*Dx + Dy*Dy );
	}
	else
//*/
	{
		D = MIN( MIN( MIN( Dx0, Dy0 ), Dx1 ), Dy1 );
	}

	// Normalize and re-sign
			D *= (bOutside ? -1.0f : +1.0f) * m_ContextRECT.InvBorderSize;
	m_Infos.Distance = D;
}

void	DrawUtils::DrawSafePixel()
{
	int	X = m_ContextRECT.X;
	if ( X >= 0 && X < m_Width && m_ContextRECT.Y >= 0 && m_ContextRECT.Y < m_Height )
		m_ContextRECT.pFiller( m_Infos, m_ContextRECT.pScanline[X] );
}

void	DrawUtils::Pixel::Blend( NjFloat4& _Source, float t )
{
	float	r = 1.0f - t;
	RGBA = RGBA * r + _Source * t;
}
