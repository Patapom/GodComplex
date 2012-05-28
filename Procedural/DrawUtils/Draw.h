// Drawing helpers
//
#pragma once

class	DrawUtils
{
public:		// NESTED TYPES

	struct	DrawInfos
	{
		int			x, y;		// Pixel coordinates in the surface
		int			w, h;		// Size of the surface
		float		Coverage;	// Coverage of the pixel (1 for internal pixels, less than 1 
		NjFloat2	UV;			// Normalized size of the surface
		float		Distance;	// Normalized distance to the border of the primitive
	};

	struct	Pixel
	{
		NjFloat4	RGBA;		// For the moment we only draw RGBA textures

		// Blends source with current value using provided alpha
		//	RGBA = _Source * (1-t) + RGBA * t
		void		Blend( const NjFloat4& _Source, float t );
	};

	typedef void	(*FillDelegate)( const DrawInfos& _Infos, Pixel& _Pixel );

protected:

	struct	DrawContext
	{
		DrawUtils*	pOwner;
		int			X, Y;		// Currently drawn pixel
		NjFloat4	P;			// Current position + UV
		float		Coverage;	// Pixel coverage
		Pixel*		pScanline;	// Current scanline
		virtual void	NewScanline()
		{
			pScanline = (Pixel*) pOwner->m_pSurface + pOwner->m_Width * Y;
			pOwner->m_Infos.y = Y;
		}
		virtual void	DrawPixel() = 0;
	};

	struct	DrawContextRECT : public DrawContext
	{
		virtual void	DrawPixel();

		float			w, h;			// Rectangle width/height
		float			x0, y0, x1, y1;	// Borders
		float			InvBorderSize;	// 1/border size
		FillDelegate	pFiller;		// Filler delegate
	};

	struct	DrawContextLINE : public DrawContext
	{
		virtual void	DrawPixel();

		float			dU;				// Small portion of UV space along U that offsets to the line's start
		FillDelegate	pFiller;		// Filler delegate
	};

	struct	DrawContextELLIPSE : public DrawContext
	{
		virtual void	DrawPixel();

		float			w, h;			// Rectangle width/height
		float			x0, y0, x1, y1;	// Borders
		float			DistanceBias;
		float			InvDu, InvDv;	// 1/border size for U & V
		FillDelegate	pFiller;		// Filler delegate
	};

protected:	// FIELDS

	int			m_Width;
	int			m_Height;
	NjFloat4*	m_pSurface;

	// Transform
	NjFloat2	m_X;
	NjFloat2	m_Y;
	NjFloat2	m_C;

	DrawInfos	m_Infos;
	DrawContextRECT	m_ContextRECT;
	DrawContextLINE	m_ContextLINE;
	DrawContextELLIPSE	m_ContextELLIPSE;

public:		// METHODS

	DrawUtils();

	void	SetupSurface( int _Width, int _Height, NjFloat4* _pSurface );
	void	SetupContext( float _PivotX, float _PivotY, float _Angle );

	// Draws a rectangle
	//	border = thickness of the border
	//	bias = bias in the border computation [0,1]. 1 shifts the border toward the outside of the rectangle.
	void	DrawRectangle( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler );

	// Draws an ellipse
	//	border = thickness of the border
	//	bias = bias in the border computation [0,1]. 1 shifts the border toward the outside of the rectangle.
	void	DrawEllipse( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler );

	// Draws a line
	void	DrawLine( float x0, float y0, float x1, float y1, float thickness, FillDelegate _Filler );


protected:
	void	DrawQuad( NjFloat2 _pVertices[], DrawContext& _Context );
	void	Transform( const NjFloat2& _SourcePosition, NjFloat4& _TransformedPosition ) const;
};
