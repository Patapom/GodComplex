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
		void*		pData;		// User data
	};

	struct	Pixel
	{
		NjFloat4	RGBA;		// For the moment we only draw RGBA textures

		// Blends source with current value using provided alpha
		//	RGBA = _Source * (1-t) + RGBA * t
		void		Blend( const NjFloat4& _Source, float t );
	};

	typedef void	(*FillDelegate)( const DrawInfos& _Infos, Pixel& _Pixel );
	typedef void	(*ScratchFillDelegate)( const DrawInfos& _Infos, Pixel& _Pixel, float Distance, float U );

protected:

	struct	DrawContext
	{
		DrawUtils*	pOwner;
		int			X, Y;			// Currently drawn pixel in SURFACE space
		NjFloat4	P;				// Current position + UV
		float		Coverage;		// Pixel coverage
		Pixel*		pScanline;		// Current scanline
		virtual void	NewScanline()
		{
			int	WrappedY = (Y + pOwner->m_Height) % pOwner->m_Height;
			pScanline = (Pixel*) pOwner->m_pSurface + pOwner->m_Width * WrappedY;
			pOwner->m_Infos.y = WrappedY;
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

	struct	DrawContextSCRATCH : public DrawContext
	{
		virtual void	DrawPixel();

		float		Distance;
		float		StepDistance;
		float		U;
		float		StepU;

		ScratchFillDelegate	pFiller;	// Filler delegate
	};

protected:	// FIELDS

	int			m_Width;
	int			m_Height;
	NjFloat4*	m_pSurface;

	// Transform
	NjFloat2	m_X;
	NjFloat2	m_Y;
	NjFloat2	m_C;

	mutable DrawInfos			m_Infos;
	mutable DrawContextRECT		m_ContextRECT;
	mutable DrawContextLINE		m_ContextLINE;
	mutable DrawContextELLIPSE	m_ContextELLIPSE;

	mutable DrawContextSCRATCH	m_ContextSCRATCH;

public:		// METHODS

	DrawUtils();

	void	SetupSurface( int _Width, int _Height, NjFloat4* _pSurface );
	void	SetupContext( float _PivotX, float _PivotY, float _Angle );

	// Draws a rectangle
	//	border = thickness of the border
	//	bias = bias in the border computation [0,1]. 1 shifts the border toward the outside of the rectangle.
	void	DrawRectangle( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler, void* _pData ) const;

	// Draws an ellipse
	//	border = thickness of the border
	//	bias = bias in the border computation [0,1]. 1 shifts the border toward the outside of the rectangle.
	void	DrawEllipse( float x, float y, float w, float h, float border, float bias, FillDelegate _Filler, void* _pData ) const;

	// Draws a line
	void	DrawLine( float x0, float y0, float x1, float y1, float thickness, FillDelegate _Filler, void* _pData ) const;


	// =================== Compound Drawing ===================
	// Draws a scratch mark
	//	_Length, length of the scratch
	//	_ThicknessStart/End, thickness of the scratch
	//	_CurveAngle, angle to add to the scratch direction for every unit step
	//	_StepSize, size of each subdivision
	void	DrawScratch( const NjFloat2& _Position, const NjFloat2& _Direction, float _Length, float _ThicknessStart, float _ThicknessEnd, float _CurveAngle, float _StepSize, ScratchFillDelegate _Filler, void* _pData ) const;


protected:
	void	DrawQuad( NjFloat4 _pVertices[], DrawContext& _Context ) const;
	void	Transform( const NjFloat4& _SourcePosition, NjFloat4& _TransformedPosition ) const;
};
