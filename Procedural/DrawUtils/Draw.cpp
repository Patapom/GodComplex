#include "../../GodComplex.h"

DrawUtils::DrawUtils()
	: m_pSurface( NULL )
{
	m_ContextRECT.pOwner = this;
	m_ContextLINE.pOwner = this;
	m_ContextELLIPSE.pOwner = this;
	m_ContextSCRATCH.pOwner = this;

	m_X = NjFloat2::UnitX;
	m_Y = NjFloat2::UnitY;
	m_C = NjFloat2::Zero;
}

void	DrawUtils::SetupSurface( int _Width, int _Height, Pixel* _pSurface )
{
	m_Infos.w = m_Width = _Width;
	m_Infos.h = m_Height = _Height;
	m_pSurface = _pSurface;
}

void	DrawUtils::SetupSurface( TextureBuilder& _TB )
{
	SetupSurface( _TB.GetWidth(), _TB.GetHeight(), _TB.GetMips()[0] );
}

void	DrawUtils::SetupTransform( float _PivotX, float _PivotY, float _Angle )
{
	_Angle = NUAJDEG2RAD( _Angle );
	float	c = cosf(_Angle), s =sinf(_Angle);

	m_C.Set( _PivotX, _PivotY );
	m_X.Set(  c, s );
	m_Y.Set( -s, c );
}

//////////////////////////////////////////////////////////////////////////
void	DrawUtils::DrawRectangle( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler, void* _pData ) const
{
	m_Infos.pData = _pData;

	// Setup rectangle-specific parameters
	m_ContextRECT.pFiller = _Filler;
	m_ContextRECT.x0 = bias*border;
	m_ContextRECT.y0 = bias*border;
	m_ContextRECT.x1 = w - bias*border;
	m_ContextRECT.y1 = h - bias*border;
	m_ContextRECT.w = w;
	m_ContextRECT.h = h;
	m_ContextRECT.InvBorderSize = border != 0.0f ? 1.0f / border : 0.0f;

	// Build quad vertices
	NjFloat4	pVertices[4];
	pVertices[0].Set( x, y, 0, 0 );
	pVertices[1].Set( x, y + h, 0, 1 );
	pVertices[2].Set( x + w, y + h, 1, 1 );
	pVertices[3].Set( x + w, y, 1, 0 );

	DrawQuad( pVertices, m_ContextRECT );
}
void	DrawUtils::DrawContextRECT::DrawPixel()
{
	int	WrappedX = X % pOwner->m_Width;
		WrappedX = WrappedX < 0 ? WrappedX + pOwner->m_Width : WrappedX;

	pOwner->m_Infos.x = X;
	pOwner->m_Infos.UV.Set( P.z, P.w );
	pOwner->m_Infos.Coverage = Coverage;

	// Compute signed distance to border
	float	fX = P.z * w;	// U * w = Pixel X in LOCAL rectangle space
	float	fY = P.w * h;	// V * h = Pixel Y in LOCAL rectangle space

	float	Dx0 = fX - x0;
	float	Dy0 = fY - y0;
	float	Dx1 = x1 - fX;
	float	Dy1 = y1 - fY;

	bool	bOutside = Dx0 < 0.0f || Dy0 < 0.0f || Dx1 < 0.0f || Dy1 < 0.0f;
	float	D;
//* Comment this part to use manhattan distance instead of cartesian
	if ( bOutside )
	{
		float	Bx = fX < x0 ? x0 : (fX > x1 ? x1 : fX);
		float	By = fY < y0 ? y0 : (fY > y1 ? y1 : fY);
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
	D *= (bOutside ? -1.0f : +1.0f) * InvBorderSize;
	pOwner->m_Infos.Distance = D;

	// Invoke pixel drawing
	pFiller( pOwner->m_Infos, pScanline[WrappedX] );
}

//////////////////////////////////////////////////////////////////////////
void	DrawUtils::DrawLine( float x0, float y0, float x1, float y1, float thickness, FillDelegate _Filler, void* _pData ) const
{
	m_Infos.pData = _pData;

	// Setup line-specific parameters
	m_ContextLINE.pFiller = _Filler;

	// Build quad vertices
	NjFloat2	P0( x0, y0 );
	NjFloat2	P1( x1, y1 );
	NjFloat2	U = P1 - P0;
	float		L = U.Length();
	if ( L == 0.0f )
		return;

	m_ContextLINE.dU = thickness / (2.0f * thickness + L);	// This is the U offset to reach the start/end points of the line

	U = U / L;
	NjFloat2	V( -U.y, U.x );

	NjFloat4	pVertices[4];
	pVertices[0] = NjFloat4( P0 + thickness * (V - U), 0, 1 );
	pVertices[1] = NjFloat4( P1 + thickness * (V + U), 1, 1 );
	pVertices[2] = NjFloat4( P1 + thickness * (U - V), 1, 0 );
	pVertices[3] = NjFloat4( P0 - thickness * (U + V), 0, 0 );

	DrawQuad( pVertices, m_ContextLINE );
}
void	DrawUtils::DrawContextLINE::DrawPixel()
{
	int	WrappedX = X % pOwner->m_Width;
		WrappedX = WrappedX < 0 ? WrappedX + pOwner->m_Width : WrappedX;

	pOwner->m_Infos.x = X;
	pOwner->m_Infos.UV.Set( P.z, P.w );
	pOwner->m_Infos.Coverage = Coverage;

	// Compute distance to the line
	float	U = CLAMP( pOwner->m_Infos.UV.x, dU, 1.0f - dU );	// Nearest U coordinate on the segment
	float	Du = (pOwner->m_Infos.UV.x - U) / dU;		// Normalize U distance
	float	Dv = 2.0f * (pOwner->m_Infos.UV.y - 0.5f);	// For V, the line is simply in the middle at V=0.5
	pOwner->m_Infos.Distance = sqrtf( Du*Du + Dv*Dv );

	// Invoke pixel drawing
	pFiller( pOwner->m_Infos, pScanline[WrappedX] );
}

//////////////////////////////////////////////////////////////////////////
void	DrawUtils::DrawEllipse( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler, void* _pData ) const
{
	m_Infos.pData = _pData;

	// Setup rectangle-specific parameters
	m_ContextELLIPSE.pFiller = _Filler;
	m_ContextELLIPSE.w = w;
	m_ContextELLIPSE.h = h;
	m_ContextELLIPSE.InvDu = (border != 0.0f ? w / border : 0.0f);
	m_ContextELLIPSE.DistanceBias = -bias * border / w;

	// Build quad vertices
	NjFloat4	pVertices[4];
	pVertices[0].Set( x, y, 0, 0 );
	pVertices[1].Set( x, y + h, 0, 1 );
	pVertices[2].Set( x + w, y + h, 1, 1 );
	pVertices[3].Set( x + w, y, 1, 0 );

	DrawQuad( pVertices, m_ContextELLIPSE );
}
void	DrawUtils::DrawContextELLIPSE::DrawPixel()
{
	int	WrappedX = X % pOwner->m_Width;
		WrappedX = WrappedX < 0 ? WrappedX + pOwner->m_Width : WrappedX;

	pOwner->m_Infos.x = X;
	pOwner->m_Infos.UV.Set( P.z, P.w );
	pOwner->m_Infos.Coverage = Coverage;

	float	Du = 2.0f * (P.z - 0.5f);
	float	Dv = 2.0f * (P.w - 0.5f);
	float	D = 1.0f - sqrtf( Du*Du + Dv*Dv );
	if ( D < 0.0f )
		return;	// Skip negative distances (i.e. out of the ellipse)

	D += DistanceBias;	// Apply bias

	pOwner->m_Infos.Distance = D * InvDu;

	// Invoke pixel drawing
	pFiller( pOwner->m_Infos, pScanline[WrappedX] );
}


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// Compound drawing
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////
// Scratches
void	DrawUtils::DrawScratch( const NjFloat2& _Position, const NjFloat2& _Direction, float _Length, float _ThicknessStart, float _ThicknessEnd, float _CurveAngle, float _StepSize, ScratchFillDelegate _Filler, void* _pData ) const
{
	m_Infos.pData = _pData;

	int	StepsCount = ceilf( _Length / _StepSize );

	m_ContextSCRATCH.pFiller = _Filler;
	m_ContextSCRATCH.Distance = 0.0f;
	m_ContextSCRATCH.StepDistance = _StepSize;
	m_ContextSCRATCH.U = 0.0f;
	m_ContextSCRATCH.StepU = _Length / (StepsCount * StepsCount * _StepSize);

	float		StepAngle = NUAJDEG2RAD( _CurveAngle ) * _StepSize;
	NjFloat2	RotX( cosf( StepAngle ), -sinf( StepAngle ) );
	NjFloat2	RotY( -RotX.y, RotX.x );

	NjFloat2	Position = _Position;
	NjFloat2	Direction = _Direction;
				Direction.Normalize();
	NjFloat2	Normal( -Direction.y, Direction.x );
	float		Thickness = _ThicknessStart;

	float		StepThickness = (_ThicknessEnd - _ThicknessStart) * _StepSize / _Length;

	NjFloat4	pVertices[4];
	pVertices[3] = NjFloat4( Position - Thickness * Normal, 0, 0 );
	pVertices[2] = NjFloat4( Position + Thickness * Normal, 0, 0 );

	for ( int StepIndex=0; StepIndex < StepsCount; StepIndex++ )
	{
		pVertices[0] = pVertices[3];	pVertices[0].z = 0.0;	pVertices[0].w = 0.0f;
		pVertices[1] = pVertices[2];	pVertices[1].z = 0.0f;	pVertices[1].w = 1.0f;

		// March along the scratch and build new vertices
		Thickness += StepThickness;
		Position = Position + _StepSize * Direction;
		Direction = NjFloat2( Direction | RotX, Direction | RotY );
		pVertices[3] = NjFloat4( Position - Thickness * Normal, 1, 0 );
		pVertices[2] = NjFloat4( Position + Thickness * Normal, 1, 1 );

		DrawQuad( pVertices, m_ContextSCRATCH );

		m_ContextSCRATCH.Distance += m_ContextSCRATCH.StepDistance;
		m_ContextSCRATCH.U += m_ContextSCRATCH.StepU;
	}
}

void	DrawUtils::DrawContextSCRATCH::DrawPixel()
{
	int	WrappedX = X % pOwner->m_Width;
		WrappedX = WrappedX < 0 ? WrappedX + pOwner->m_Width : WrappedX;

	pOwner->m_Infos.x = X;
	pOwner->m_Infos.UV.Set( P.z, P.w );
	pOwner->m_Infos.Coverage = Coverage;

	// Compute distance to the center of the scratch
	pOwner->m_Infos.Distance = 2.0f * (pOwner->m_Infos.UV.y - 0.5f);

	// Invoke pixel drawing
	pFiller( pOwner->m_Infos, pScanline[WrappedX], Distance + P.z * StepDistance, U + P.z * StepU );
}


//////////////////////////////////////////////////////////////////////////
void	DrawUtils::Transform( const NjFloat4& _SourcePosition, NjFloat4& _TransformedPosition ) const
{
	_TransformedPosition.x = m_C.x + m_X.x * _SourcePosition.x + m_Y.x * _SourcePosition.y;
	_TransformedPosition.y = m_C.y + m_X.y * _SourcePosition.x + m_Y.y * _SourcePosition.y;
	_TransformedPosition.z = _SourcePosition.z;
	_TransformedPosition.w = _SourcePosition.w;
}

// Here, we're assuming (x,y) couples are CCW and form a convex quadrilateral
void	DrawUtils::DrawQuad( NjFloat4 _pVertices[], DrawContext& _Context ) const
{
	// Build doubled list of vertices with UVs
	NjFloat4	pVertices[8];
	Transform( _pVertices[0], pVertices[0] );
	Transform( _pVertices[1], pVertices[1] );
	Transform( _pVertices[2], pVertices[2] );
	Transform( _pVertices[3], pVertices[3] );
	for ( int i=0; i < 4; i++ )
		pVertices[4+i] = pVertices[i];

	// Find top vertex
	float	Min = FLOAT32_MAX;
	int		Top = -1;
	for ( int i=0; i < 4; i++ )
		if ( pVertices[i].y < Min )
		{
			Min = pVertices[i].y;
			Top = i;
		}

	// Start drawing
	_Context.Y = floorf( pVertices[Top].y );
	int			LDy = 0, RDy = 0;			// Amount of pixels to trace for the left & right segments until the next segment (or end of the quad)
	int			L = Top, R = 4+Top;			// Left & Right indices: Left will increase, Right will decrease
	NjFloat4	LPos, RPos;					// Left & Right position & UV
	NjFloat4	LSlope, RSlope;				// Left & Right slope
//	while ( _Context.Y < m_Height )
	while ( true )
	{
		while ( LDy <= 0.0f && L <= R )
		{	// Rebuild left slope
			NjFloat4&	Current = pVertices[L];
			NjFloat4&	Next = pVertices[++L];
			LSlope = Next - Current;
			int		EndY = floorf( Next.y+0.5f );
			LDy = EndY - _Context.Y;
// 			if ( LDy <= 0 )
// 				continue;	// Too low a slope !

			LSlope = LSlope / LSlope.y;
			LPos = Current + (_Context.Y+0.5f - Current.y) * LSlope;
		}
		while ( RDy <= 0.0f && L <= R )
		{	// Rebuild right slope
			NjFloat4&	Current = pVertices[R];
			NjFloat4&	Next = pVertices[--R];
			RSlope = Next - Current;
			int		EndY = floorf( Next.y+0.5f );
			RDy = EndY - _Context.Y;
// 			if ( RDy <= 0 )
// 				continue;	// Too low a slope !

			RSlope = RSlope / RSlope.y;
			RPos = Current + (_Context.Y+0.5f - Current.y) * RSlope;
		}
		if ( L > R )
			break;	// The quad is over !

		// Draw the scanline
		// The comments left here are the original lines you need to draw in CLAMP mode
//		if ( _Context.Y > 0 )
		{
			_Context.X = floorf( LPos.x );
//			bool		bClipLeft = _Context.X < 0;
			bool		bClipLeft = false;	// Don't ever clip since we wrap !
//			_Context.X = MAX( 0, _Context.X );

			int			RX = floorf( RPos.x );
//			bool		bClipRight = RX >= m_Width;
			bool		bClipRight = false;	// Don't ever clip since we wrap !
//						RX = MIN( m_Width-1, RX );

			// Compute slope & start position
			NjFloat4	Slope = RPos - LPos;
			if ( Slope.x != 0.0f )
				Slope = Slope / Slope.x;

			_Context.P = LPos + (_Context.X+0.5f - LPos.x) * Slope;
			_Context.NewScanline();

			// Draw left pixel
			if ( !bClipLeft )
			{
				_Context.Coverage = ceilf( _Context.P.x ) - LPos.x;
				_Context.DrawPixel();
				_Context.X++;
				_Context.P = _Context.P + Slope;
			}

			// Draw full coverage pixels
			_Context.Coverage = 1.0f;
			for ( ; _Context.X < RX; _Context.X++ )
			{
				_Context.DrawPixel();
				_Context.P = _Context.P + Slope;
			}

			// Draw right pixel
			if ( !bClipRight )
			{
				_Context.Coverage = RPos.x - floorf( _Context.P.x );
				_Context.DrawPixel();
			}
		}

		// Increment
		_Context.Y++;
		LDy--; RDy--;
		LPos = LPos + LSlope;
		RPos = RPos + RSlope;
	}
}
