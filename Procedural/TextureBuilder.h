//////////////////////////////////////////////////////////////////////////
// Helps to build a texture and its mip levels to provide a valid buffer when constructing a Texture2D
// Note that the resulting texture is always a Texture2DArray where the various fields are populated as dictated by the ConversionParams structure
//
#pragma once

struct Pixel;

class	TextureBuilder
{
protected:	// CONSTANTS

public:		// NESTED TYPES

	typedef void	(*FillDelegate)( int _X, int _Y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData );

	// The complex structure that is guiding the texture conversion
	// Use -1 in field positions to avoid storing the field
	// * If you use only [1,4] fields, a single texture will be generated
	// * If you use only [5,8] fields, 2 textures will be generated
	// * If you use only [9,12] fields, 3 textures will be generated
	//	etc.
	//
	// Check the existing presets for typical cases.
	struct	ConversionParams
	{
		// Positions of the color fields
		int		PosR;
		int		PosG;
		int		PosB;
		int		PosA;

		// Position of the height & roughness fields
		int		PosHeight;
		int		PosRoughness;

		// Position of the normal fields
		bool	GenerateNormal;	// If true, the normal will be generated
		bool	PackNormalXY;	// If true, only the XY components of the normal will be stored. Z will then be extracted by sqrt(1-X²-Y²)
		float	NormalFactor;	// Factor to apply to the height to generate the normals
		int		PosNormalX;
		int		PosNormalY;
		int		PosNormalZ;

		// Position of the AO field
		bool	GenerateAO;
		float	AOFactor;		// Factor to apply to the height to generate the AO
		int		PosAO;

		// TODO: Curvature? Dirt accumulation? Gradient?
	};

	static ConversionParams		CONV_RGBA_NxNyHR;	// Generates an array of 2 textures: 1st is RGBA, 2nd is Normal(X+Y), Height, Roughness


protected:	// FIELDS

	int				m_Width;
	int				m_Height;
	int				m_MipLevelsCount;
	mutable bool	m_bMipLevelsBuilt;

	Pixel**			m_ppBufferGeneric;		// Generic buffer consisting of meta-pixels
	mutable void**	m_ppBufferSpecific;		// Specific buffer of given pixel format


public:		// PROPERTIES

	int				GetWidth() const	{ return m_Width; }
	int				GetHeight() const	{ return m_Height; }

	Pixel**			GetMips()			{ return m_ppBufferGeneric; }
	const void**	GetLastConvertedMips() const;

public:		// METHODS

	TextureBuilder( int _Width, int _Height );
 	~TextureBuilder();

	void			CopyFrom( const TextureBuilder& _Source );
	void			Fill( FillDelegate _Filler, void* _pData );
	void			Get( int _X, int _Y, Pixel& _Color ) const;
	void			SampleWrap( float _X, float _Y, Pixel& _Pixel ) const;
	void			SampleClamp( float _X, float _Y, Pixel& _Pixel ) const;
	void			GenerateMips( bool _bTreatRGBAsNormal=false ) const;

	void**			Convert( const IPixelFormatDescriptor& _Format, const ConversionParams& _Params ) const;

private:
	void			ReleaseSpecificBuffer() const;
	float			BuildComponent( int _ComponentIndex, const ConversionParams& _Params, Pixel& _Pixel0, Pixel& _Pixel1, Pixel& _Pixel2 ) const;
};
