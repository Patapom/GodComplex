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
		void		Blend( NjFloat4& _Source, float t );
	};

	typedef void	(*FillDelegate)( const DrawInfos& _Infos, Pixel& _Pixel );


protected:	// FIELDS

	int			m_Width;
	int			m_Height;
	NjFloat4*	m_pSurface;

public:		// METHODS

	DrawUtils();

	void	SetupContext( int _Width, int _Height, NjFloat4* _pSurface );

	// Draws a rectangle
	//	border = thickness of the border
	void	DrawRectangle( float x, float y, float w, float h, float border, FillDelegate _Filler );
};
