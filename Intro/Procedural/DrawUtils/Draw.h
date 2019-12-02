// Drawing helpers
//
#pragma once

#include "../FatPixel.h"

class	DrawUtils
{
public:		// NESTED TYPES

	struct	DrawInfos
	{
		int			x, y;		// Pixel coordinates in the surface
		int			w, h;		// Size of the surface
		float		Coverage;	// Coverage of the pixel (1 for internal pixels, less than 1 for other pixels
		float2	UV;			// Normalized size of the surface
		float		Distance;	// Normalized distance to the border of the primitive (positive or negative if there is a bias)
		void*		pData;		// User data
	};

	typedef void	(*FillDelegate)( const DrawInfos& _Infos, Pixel& _Pixel );
	typedef void	(*ScratchFillDelegate)( const DrawInfos& _Infos, Pixel& _Pixel, float Distance, float U );

protected:

	struct	DrawContext
	{
		DrawUtils*	pOwner;
		int			X, Y;			// Currently drawn pixel in SURFACE space
		float4	P;				// Current position + UV
		float		Coverage;		// Pixel coverage
		Pixel*		pScanline;		// Current scanline
		virtual void	NewScanline()
		{
			int	WrappedY = Y % pOwner->m_Height;
				WrappedY = WrappedY < 0 ? WrappedY + pOwner->m_Height : WrappedY;	// Ensure always positive !

			pScanline = pOwner->m_pSurface + pOwner->m_Width * WrappedY;
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
	Pixel*		m_pSurface;

	// Transform
	float2	m_X;
	float2	m_Y;
	float2	m_C;

	mutable DrawInfos			m_Infos;
	mutable DrawContextRECT		m_ContextRECT;
	mutable DrawContextLINE		m_ContextLINE;
	mutable DrawContextELLIPSE	m_ContextELLIPSE;

	mutable DrawContextSCRATCH	m_ContextSCRATCH;

public:		// METHODS

	DrawUtils();

	void	SetupSurface( int _Width, int _Height, Pixel* _pSurface );
	void	SetupSurface( TextureBuilder& _TB );
	void	SetupTransform( float _PivotX, float _PivotY, float _Angle );	// Use this to setup your transform context

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
	void	DrawScratch( const float2& _Position, const float2& _Direction, float _Length, float _ThicknessStart, float _ThicknessEnd, float _CurveAngle, float _StepSize, ScratchFillDelegate _Filler, void* _pData ) const;

	void	DrawSplotch( const float2& _Position, const float2& _Size, float _Angle, const Noise& _Noise, float _Perturbation ) const;

protected:
	void	DrawQuad( float4 _pVertices[], DrawContext& _Context ) const;
	void	Transform( const float4& _SourcePosition, float4& _TransformedPosition ) const;
};
