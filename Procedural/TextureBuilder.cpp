#include "../GodComplex.h"

TextureBuilder::TextureBuilder( int _Width, int _Height )
	: m_ppBufferSpecific( NULL )
	, m_Width( _Width )
	, m_Height( _Height )
	, m_bMipLevelsBuilt( false )
{
	m_MipLevelsCount = Texture2D::ComputeMipLevelsCount( _Width, _Height, 0 );
	m_ppBufferGeneric = new Pixel*[m_MipLevelsCount];
	m_pMipSizes = new int[2*m_MipLevelsCount];
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		m_ppBufferGeneric[MipLevelIndex] = new Pixel[_Width*_Height];
		m_pMipSizes[2*MipLevelIndex+0] = _Width;
		m_pMipSizes[2*MipLevelIndex+1] = _Height;
		Texture2D::NextMipSize( _Width, _Height );
	}
}

TextureBuilder::~TextureBuilder()
{
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		delete[] m_ppBufferGeneric[MipLevelIndex];
	delete[] m_pMipSizes;
	delete[] m_ppBufferGeneric;
	ReleaseSpecificBuffer();
}

const void**	TextureBuilder::GetLastConvertedMips() const
{
	ASSERT( m_ppBufferSpecific != NULL, "Invalid final texture buffers ! Did you forget to call Convert() ?" );
	return (const void**) m_ppBufferSpecific;
}

namespace Fillers
{
	struct __FillerSampleStruct
	{
		const TextureBuilder*	pSource;
		int						W, H;
		int						MipLevel;
	};

// 	void	CopyFillerFast( int _X, int _Y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData )
// 	{
// 		__FillerSampleStruct*	pStruct = (__FillerSampleStruct*) _pData;
// 		pStruct->pSource->SampleClamp( _UV.x * pStruct->W, _UV.y * pStruct->H, 0, _Pixel );
// 	}

	void	CopyFiller( int _X, int _Y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData )
	{
		__FillerSampleStruct*	pStruct = (__FillerSampleStruct*) _pData;
		pStruct->pSource->SampleClamp( _UV.x * pStruct->W, _UV.y * pStruct->H, pStruct->MipLevel, _Pixel );
	}
}

void	TextureBuilder::CopyFromFast( const TextureBuilder& _Source )
{
	Fillers::__FillerSampleStruct	Param;
	Param.pSource = &_Source;
	Param.W = _Source.GetWidth();
	Param.H = _Source.GetHeight();
	Param.MipLevel = 0;
//	Fill( Fillers::CopyFillerFast, (void*) &Param );
	Fill( Fillers::CopyFiller, (void*) &Param );
}

void	TextureBuilder::CopyFrom( const TextureBuilder& _Source )
{
	int	MipLevel = 0;
	if ( _Source.m_Width >= 2*m_Width || _Source.m_Height >= 2*m_Height )
	{	// The source is more than twice our size, we must generate its mips and sample from the immediately superior mip
		if ( !_Source.m_bMipLevelsBuilt )
			_Source.GenerateMips( false );

		float	RatioW = float(_Source.m_Width) / m_Width;
		float	RatioH = float(_Source.m_Height) / m_Height;
		float	Ratio = MAX( RatioW, RatioH );	// We need to use the mip level of the worst ratio...
		float	fMipLevel = log2f( Ratio );		// This is the exact mip we need to sample from
		MipLevel = floorf( fMipLevel );			// In reality, we sample from the nearest lower mip (for example, if the source is 3.5 larger than the destination, fMipLevel=1.8 and we sample from MipLevel=1 which is well suited to accomodate sizes from 2x to 4x)
	}

	Fillers::__FillerSampleStruct	Param;
	Param.pSource = &_Source;
	Param.W = _Source.m_pMipSizes[2*MipLevel+0];
	Param.H = _Source.m_pMipSizes[2*MipLevel+1];
	Param.MipLevel = MipLevel;

	Fill( Fillers::CopyFiller, (void*) &Param );
}

void	TextureBuilder::Clear( const Pixel& _Pixel )
{
	// Clear the mip level 0
	for ( int Y=0; Y < m_Height; Y++ )
	{
		Pixel*	pScanline = m_ppBufferGeneric[0] + m_Width * Y;
		for ( int X=0; X < m_Width; X++, pScanline++ )
			memcpy( pScanline, &_Pixel, sizeof(Pixel) );
	}
	m_bMipLevelsBuilt = false;
}

void	TextureBuilder::Fill( FillDelegate _Filler, void* _pData )
{
	// Fill the mip level 0
	NjFloat2	UV;
	for ( int Y=0; Y < m_Height; Y++ )
	{
		Pixel*	pScanline = m_ppBufferGeneric[0] + m_Width * Y;
		UV.y = float(Y) / m_Height;
		for ( int X=0; X < m_Width; X++, pScanline++ )
		{
			UV.x = float(X) / m_Width;
			(*_Filler)( X, Y, UV, *pScanline, _pData );
		}
	}
	m_bMipLevelsBuilt = false;
}

void	TextureBuilder::Get( int _X, int _Y, int _MipLevel, Pixel& _Color ) const
{
	ASSERT( _MipLevel == 0 || m_bMipLevelsBuilt, "You must call GenerateMips() prior getting a pixel from a mip level different than 0!" );

	int		W = m_pMipSizes[(_MipLevel<<1)+0];
	int		H = m_pMipSizes[(_MipLevel<<1)+1];

	ASSERT( _X >= 0 && _X < W, "X out of range !" );
	ASSERT( _Y >= 0 && _Y < H, "Y out of range !" );

	_Color = m_ppBufferGeneric[_MipLevel][W*_Y+_X];
}

void	TextureBuilder::SampleWrap( float _X, float _Y, int _MipLevel, Pixel& _Pixel ) const
{
	ASSERT( _MipLevel == 0 || m_bMipLevelsBuilt, "You must call GenerateMips() prior sampling from a mip level different than 0!" );

	int		W = m_pMipSizes[(_MipLevel<<1)+0];
	int		H = m_pMipSizes[(_MipLevel<<1)+1];

	int		X0 = floorf( _X );
	float	x = _X - X0;
	float	rx = 1.0f - x;
	int		X1 = (100*W+X0+1) % W;
			X0 = (100*W+X0) % W;

	int		Y0 = floorf( _Y );
	float	y = _Y - Y0;
	float	ry = 1.0f - y;
	int		Y1 = (100*H+Y0+1) % H;
			Y0 = (100*H+Y0) % H;

	ASSERT( X0 >= 0 && X0 < W && X1 >= 0 && X1 < W, "X out of range !" );	// Should never happen
	ASSERT( Y0 >= 0 && Y0 < H && Y1 >= 0 && Y1 < H, "Y out of range !" );	// Should never happen
	Pixel&	V00 = m_ppBufferGeneric[_MipLevel][W*Y0+X0];
	Pixel&	V01 = m_ppBufferGeneric[_MipLevel][W*Y0+X1];
	Pixel&	V10 = m_ppBufferGeneric[_MipLevel][W*Y1+X0];
	Pixel&	V11 = m_ppBufferGeneric[_MipLevel][W*Y1+X1];

	NjFloat4	V0 = rx * V00.RGBA + x * V01.RGBA;
	NjFloat4	V1 = rx * V10.RGBA + x * V11.RGBA;
	float		H0 = rx * V00.Height + x * V01.Height;
	float		H1 = rx * V10.Height + x * V11.Height;
	float		R0 = rx * V00.Roughness + x * V01.Roughness;
	float		R1 = rx * V10.Roughness + x * V11.Roughness;

	_Pixel.RGBA.x = ry * V0.x + y * V1.x;
	_Pixel.RGBA.y = ry * V0.y + y * V1.y;
	_Pixel.RGBA.z = ry * V0.z + y * V1.z;
	_Pixel.RGBA.w = ry * V0.w + y * V1.w;
	_Pixel.Height = ry * H0 + y * H1;
	_Pixel.Roughness = ry * R0 + y * R1;
	_Pixel.MatID = V00.MatID;	// Arbitrary!
}

void	TextureBuilder::SampleClamp( float _X, float _Y, int _MipLevel, Pixel& _Pixel ) const
{
	ASSERT( _MipLevel == 0 || m_bMipLevelsBuilt, "You must call GenerateMips() prior sampling from a mip level different than 0!" );

	int		W = m_pMipSizes[(_MipLevel<<1)+0];
	int		H = m_pMipSizes[(_MipLevel<<1)+1];

	int		X0 = floorf( _X );
	float	x = _X - X0;
	float	rx = 1.0f - x;
	int		X1 = CLAMP( (X0+1), 0, W-1 );
			X0 = CLAMP( X0, 0, W-1 );

	int		Y0 = floorf( _Y );
	float	y = _Y - Y0;
	float	ry = 1.0f - y;
	int		Y1 = CLAMP( (Y0+1), 0, H-1 );
			Y0 = CLAMP( Y0, 0, H-1 );

	Pixel&	V00 = m_ppBufferGeneric[_MipLevel][W*Y0+X0];
	Pixel&	V01 = m_ppBufferGeneric[_MipLevel][W*Y0+X1];
	Pixel&	V10 = m_ppBufferGeneric[_MipLevel][W*Y1+X0];
	Pixel&	V11 = m_ppBufferGeneric[_MipLevel][W*Y1+X1];

	NjFloat4	V0 = rx * V00.RGBA + x * V01.RGBA;
	NjFloat4	V1 = rx * V10.RGBA + x * V11.RGBA;
	float		H0 = rx * V00.Height + x * V01.Height;
	float		H1 = rx * V10.Height + x * V11.Height;
	float		R0 = rx * V00.Roughness + x * V01.Roughness;
	float		R1 = rx * V10.Roughness + x * V11.Roughness;

	_Pixel.RGBA.x = ry * V0.x + y * V1.x;
	_Pixel.RGBA.y = ry * V0.y + y * V1.y;
	_Pixel.RGBA.z = ry * V0.z + y * V1.z;
	_Pixel.RGBA.w = ry * V0.w + y * V1.w;
	_Pixel.Height = ry * H0 + y * H1;
	_Pixel.Roughness = ry * R0 + y * R1;
	_Pixel.MatID = V00.MatID;	// Arbitrary!
}

void	TextureBuilder::GenerateMips( bool _bTreatRGBAsNormal, bool _bNormalizeNormals ) const
{
	// Build remaining mip levels
	int	Width = m_Width;
	int	Height = m_Height;
	for ( int MipLevelIndex=1; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		int		SourceWidth = Width;
		int		SourceHeight = Height;
		Texture2D::NextMipSize( Width, Height );

		Pixel*	pSource = m_ppBufferGeneric[MipLevelIndex-1];
		Pixel*	pTarget = m_ppBufferGeneric[MipLevelIndex];
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

			Pixel*	pScanline = pTarget + Width * Y;
			for ( int X=0; X < Width; X++, pScanline++ )
			{
				int	X0 = (X << 1) + 0;
#if 0
				int	X1 = (X0+1) % Width;	// TODO: Handle WRAP/CLAMP
#else
				int	X1 = (X0+1) % SourceWidth;	// TODO: Handle WRAP/CLAMP
#endif

				Pixel&	V00 = pSource[SourceWidth*Y0+X0];
				Pixel&	V01 = pSource[SourceWidth*Y0+X1];
				Pixel&	V10 = pSource[SourceWidth*Y1+X0];
				Pixel&	V11 = pSource[SourceWidth*Y1+X1];

				if ( _bTreatRGBAsNormal )
				{
					NjFloat3	N00 = NjFloat3(V00.RGBA);
					NjFloat3	N01 = NjFloat3(V01.RGBA);
					NjFloat3	N10 = NjFloat3(V10.RGBA);
					NjFloat3	N11 = NjFloat3(V11.RGBA);

					NjFloat3	N = 0.25f * (N00 + N01 + N10 + N11);
					if ( _bNormalizeNormals )
						N.Normalize();
					pScanline->RGBA.x = N.x;
					pScanline->RGBA.y = N.y;
					pScanline->RGBA.z = N.z;
					pScanline->RGBA.w = 0.25f * (V00.RGBA.w + V01.RGBA.w + V10.RGBA.w + V11.RGBA.w);
				}
				else
					pScanline->RGBA = 0.25f * (V00.RGBA + V01.RGBA + V10.RGBA + V11.RGBA);
				pScanline->Height = 0.25f * (V00.Height + V01.Height + V10.Height + V11.Height);
				pScanline->Roughness = 0.25f * (V00.Roughness + V01.Roughness + V10.Roughness + V11.Roughness);
				pScanline->MatID = V00.MatID;	// Arbitrary! We really should choose the material shared by most of the pixels... Need to create a mini hashtable... Pain... See later...
			}
		}
	}

	m_bMipLevelsBuilt = true;
}

TextureBuilder::ConversionParams	TextureBuilder::CONV_RGBA =
{
	0, 1, 2, 3, false,	// RGBA + bLinearize

			// Position of the height & roughness fields
	-1,		// int		PosHeight;
	-1,		// int		PosRoughness;

			// Position of the Material ID
	-1,		// int		PosMatID;

			// Position of the normal fields
//	1.0f,	// float	NormalFactor;	// Factor to apply to the height to generate the normals
	true,	// bool		bPackNormal;
	-1, -1, -1,	// Normal fields

			// Position of the AO field
//	1.0f,	// float	AOFactor;		// Factor to apply to the height to generate the AO
	-1,		// int		PosAO;
};

TextureBuilder::ConversionParams	TextureBuilder::CONV_RGBA_sRGB =
{
	0, 1, 2, 3, true,	// RGBA + bLinearize

			// Position of the height & roughness fields
	-1,		// int		PosHeight;
	-1,		// int		PosRoughness;

			// Position of the Material ID
	-1,		// int		PosMatID;

			// Position of the normal fields
//	1.0f,	// float	NormalFactor;	// Factor to apply to the height to generate the normals
	true,	// bool		bPackNormal;
	-1, -1, -1,	// Normal fields

			// Position of the AO field
//	1.0f,	// float	AOFactor;		// Factor to apply to the height to generate the AO
	-1,		// int		PosAO;
};

TextureBuilder::ConversionParams	TextureBuilder::CONV_RGBA_NxNyHR_M =
{
	0, 1, 2, 3, false,	// RGBA + bLinearize

			// Position of the height & roughness fields
	6,		// int		PosHeight;
	7,		// int		PosRoughness;

			// Position of the Material ID
	8,		// int		PosMatID;

			// Position of the normal fields
//	1.0f,	// float	NormalFactor;	// Factor to apply to the height to generate the normals
	true,	// bool		bPackNormal;
	4, 5, -1,	// Normal fields

			// Position of the AO field
//	1.0f,	// float	AOFactor;		// Factor to apply to the height to generate the AO
	-1,		// int		PosAO;
};

TextureBuilder::ConversionParams	TextureBuilder::CONV_NxNyNzH =
{
	-1,	-1, -1, -1, false,	// RGBA + bLinearize

			// Position of the height & roughness fields
	3,		// int		PosHeight;
	-1,		// int		PosRoughness;

			// Position of the Material ID
	-1,		// int		PosMatID;

			// Position of the normal fields
//	1.0f,	// float	NormalFactor;	// Factor to apply to the height to generate the normals
	true,	// bool		bPackNormal;
	0, 1, 2,	// Normal fields

			// Position of the AO field
//	1.0f,	// float	AOFactor;		// Factor to apply to the height to generate the AO
	-1,		// int		PosAO;
};

void**	TextureBuilder::Convert( const IPixelFormatDescriptor& _Format, const ConversionParams& _Params, int& _ArraySize, float _NormalFactor, bool _bNormalizeNormals, float _AOFactor ) const
{
	if ( !m_bMipLevelsBuilt )
		GenerateMips();

	ReleaseSpecificBuffer();

	//////////////////////////////////////////////////////////////////////////
	// Generate normal
	TextureBuilder	TBNormal( m_Width, m_Height );
	if ( _Params.PosNormalX != -1 )
	{
		ASSERT( _Params.PosNormalY != -1, "You must specify a position for the Y component of the normal if PosNormalX is not -1!" );
		bool	bNormalize = _bNormalizeNormals || _Params.PosNormalZ == -1;
		Generators::ComputeNormal( *this, TBNormal, _NormalFactor, bNormalize );
		TBNormal.GenerateMips( true, true );
	}

	//////////////////////////////////////////////////////////////////////////
	// Generate AO
	TextureBuilder	TBAO( m_Width, m_Height );
	if ( _Params.PosAO != -1 )
	{
		Generators::ComputeAO( *this, TBNormal, _AOFactor );
		TBAO.GenerateMips();
	}

	//////////////////////////////////////////////////////////////////////////
	// Compute the amount of textures to create in the array
	int	MaxPosition = -1;
	MaxPosition = MAX( MaxPosition, _Params.PosR );
	MaxPosition = MAX( MaxPosition, _Params.PosG );
	MaxPosition = MAX( MaxPosition, _Params.PosB );
	MaxPosition = MAX( MaxPosition, _Params.PosA );
	MaxPosition = MAX( MaxPosition, _Params.PosNormalX );
	MaxPosition = MAX( MaxPosition, _Params.PosNormalY );
	MaxPosition = MAX( MaxPosition, _Params.PosNormalZ );
	MaxPosition = MAX( MaxPosition, _Params.PosHeight );
	MaxPosition = MAX( MaxPosition, _Params.PosRoughness );
	MaxPosition = MAX( MaxPosition, _Params.PosMatID );
	MaxPosition = MAX( MaxPosition, _Params.PosAO );

	_ArraySize = (MaxPosition+4) >> 2;

	//////////////////////////////////////////////////////////////////////////
	// Allocate buffers
	m_ppBufferSpecific = new void*[m_MipLevelsCount*_ArraySize];

	int	PixelSize = _Format.Size();
	for ( int ArrayIndex=0; ArrayIndex < _ArraySize; ArrayIndex++ )
	{
		int	Width = m_Width;
		int	Height = m_Height;
		int	ComponentsOffset = ArrayIndex << 2;

		for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		{
			Pixel*	pSource0 = m_ppBufferGeneric[MipLevelIndex];
			Pixel*	pSource1 = TBNormal.GetMips()[MipLevelIndex];
			Pixel*	pSource2 = TBAO.GetMips()[MipLevelIndex];

			U8*		pDest = new U8[Width*Height*PixelSize];
			m_ppBufferSpecific[m_MipLevelsCount*ArrayIndex+MipLevelIndex] = (void*) pDest;

			// Copy
			for ( int Y=0; Y < Height; Y++ )
			{
				NjFloat4	Temp;
				Pixel*		pScanlineSource0 = &pSource0[Width*Y];
				Pixel*		pScanlineSource1 = &pSource1[Width*Y];
				Pixel*		pScanlineSource2 = &pSource2[Width*Y];
				U8*			pScanlineDest = &pDest[PixelSize*Width*Y];
				for ( int X=0; X < Width; X++, pScanlineDest+=PixelSize, pScanlineSource0++, pScanlineSource1++, pScanlineSource2++ )
				{
					Temp.x = BuildComponent( ComponentsOffset+0, _Params, *pScanlineSource0, *pScanlineSource1, *pScanlineSource2 );
					Temp.y = BuildComponent( ComponentsOffset+1, _Params, *pScanlineSource0, *pScanlineSource1, *pScanlineSource2 );
					Temp.z = BuildComponent( ComponentsOffset+2, _Params, *pScanlineSource0, *pScanlineSource1, *pScanlineSource2 );
					Temp.w = BuildComponent( ComponentsOffset+3, _Params, *pScanlineSource0, *pScanlineSource1, *pScanlineSource2 );

					_Format.Write( pScanlineDest, Temp );
				}
			}

			// Downsample
			Texture2D::NextMipSize( Width, Height );
		}
	}

	return m_ppBufferSpecific;
}

Texture2D*	TextureBuilder::CreateTexture( const IPixelFormatDescriptor& _Format, const ConversionParams& _Params, bool _bStaging, bool _bWriteable ) const
{
	int			ArraySize;
	void**		ppContent = Convert( _Format, _Params, ArraySize );
	Texture2D*	pResult = new Texture2D( gs_Device, m_Width, m_Height, ArraySize, _Format, m_MipLevelsCount, ppContent, _bStaging, _bWriteable );
	return pResult;
}

Texture2D*	TextureBuilder::Concat( int _SourcesCount, void** _pppArrays[], int _ArraySizes[], const IPixelFormatDescriptor& _Format, bool _bStaging, bool _bWriteable ) const
{
	int	TotalArraySize = 0;
	for ( int SourceIndex=0; SourceIndex < _SourcesCount; SourceIndex++ )
		TotalArraySize += _ArraySizes[SourceIndex];

	// Concatenate all arrays into one giant array
	void**	ppFinalArray = new void*[m_MipLevelsCount*TotalArraySize];
	TotalArraySize = 0;
	for ( int SourceIndex=0; SourceIndex < _SourcesCount; SourceIndex++ )
		for ( int ArrayIndex=0; ArrayIndex < _ArraySizes[SourceIndex]; ArrayIndex++ )
		{
			for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
				ppFinalArray[m_MipLevelsCount*(TotalArraySize+ArrayIndex)+MipLevelIndex] = _pppArrays[SourceIndex][m_MipLevelsCount*ArrayIndex+MipLevelIndex];

			TotalArraySize += _ArraySizes[SourceIndex];
		}

	Texture2D*	pResult = new Texture2D( gs_Device, m_Width, m_Height, TotalArraySize, _Format, m_MipLevelsCount, ppFinalArray, _bStaging, _bWriteable );

	delete[] ppFinalArray;

	return pResult;
}

float	TextureBuilder::BuildComponent( int _ComponentIndex, const ConversionParams& _Params, Pixel& _Pixel0, Pixel& _Pixel1, Pixel& _Pixel2 ) const
{
	// Check if it's the color
	if ( _Params.bLinearizeColors )
	{
		if ( _ComponentIndex == _Params.PosR )
			return sRGB2Linear( _Pixel0.RGBA.x );
		if ( _ComponentIndex == _Params.PosG )
			return sRGB2Linear( _Pixel0.RGBA.y );
		if ( _ComponentIndex == _Params.PosB )
			return sRGB2Linear( _Pixel0.RGBA.z );
	}
	else
	{
		if ( _ComponentIndex == _Params.PosR )
			return _Pixel0.RGBA.x;
		if ( _ComponentIndex == _Params.PosG )
			return _Pixel0.RGBA.y;
		if ( _ComponentIndex == _Params.PosB )
			return _Pixel0.RGBA.z;
	}
	if ( _ComponentIndex == _Params.PosA )
		return _Pixel0.RGBA.w;

	// Check if it's the height or roughness
	if ( _ComponentIndex == _Params.PosHeight )
		return _Pixel0.Height;
	if ( _ComponentIndex == _Params.PosRoughness )
		return _Pixel0.Roughness;

	// Check if it's the material ID
	if ( _ComponentIndex == _Params.PosMatID )
		return float(_Pixel0.MatID);

	// Check if it's the normal
	if ( _ComponentIndex == _Params.PosNormalX )
		return _Params.bPackNormal ? 0.5f * (1.0f + _Pixel1.RGBA.x) : _Pixel1.RGBA.x;
	if ( _ComponentIndex == _Params.PosNormalY )
		return _Params.bPackNormal ? 0.5f * (1.0f + _Pixel1.RGBA.y) : _Pixel1.RGBA.y;
	if ( _ComponentIndex == _Params.PosNormalZ )
		return _Params.bPackNormal ? 0.5f * (1.0f + _Pixel1.RGBA.z) : _Pixel1.RGBA.z;

	// Check if it's the ambient occlusion
	if ( _ComponentIndex == _Params.PosAO )
		return _Pixel2.RGBA.x;

	// Empty component => WASTE !!!!
	return 0.0f;
}

float	TextureBuilder::sRGB2Linear( float _sRGB )
{
	return _sRGB > 0.04045f ? powf( (_sRGB + 0.055f) / 1.055f, 2.4f ) : _sRGB / 12.92f;
}

float	TextureBuilder::Linear2sRGB( float _Linear )
{
	return _Linear > 0.0031308f ? 1.055f * powf( _Linear, 1.0f / 2.4f ) - 0.055f : 12.92f * _Linear;
}

void	TextureBuilder::ReleaseSpecificBuffer() const
{
	if ( m_ppBufferSpecific == NULL )
		return;

	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		delete[] m_ppBufferSpecific[MipLevelIndex];
	delete[] m_ppBufferSpecific;
}


#ifdef _DEBUG
#include <stdio.h>

// Loads a RAW image from disk (RAW images can be created by the Tool/PNG2RAW project)
// Warning: There is absolutely NO check on the size of the file. You must know what you're doing here!
void	TextureBuilder::LoadFromRAWFile( const char* _pPath, bool _bAsHeight )
{
	int		Size = 4*m_Width*m_Height;

	U8*		pRAW = new U8[Size];
	FILE*	pFile = fopen( _pPath, "rb" );
	ASSERT( pFile != NULL, "Invalid file!" );
	fread_s( pRAW, Size, 1, Size, pFile );
	fclose( pFile );

	for ( int Y=0; Y < m_Height; Y++ )
	{
		U8*		pScanlineSource = &pRAW[4*m_Width*Y];
		Pixel*	pScanlineTarget = &m_ppBufferGeneric[0][m_Width*Y];
		for ( int X=0; X < m_Width; X++, pScanlineTarget++ )
		{
			float	R = *pScanlineSource++ / 255.0f;
			float	G = *pScanlineSource++ / 255.0f;
			float	B = *pScanlineSource++ / 255.0f;
			float	A = *pScanlineSource++ / 255.0f;
			if ( _bAsHeight )
			{
				pScanlineTarget->Height = 0.2126f * R + 0.7152f * G + 0.0722f * B;
				pScanlineTarget->RGBA.Set( 0, 0, 0, 0 );
			}
			else
			{
				pScanlineTarget->RGBA.Set( R, G, B, A );
				pScanlineTarget->Height = 0.0f;
			}
			pScanlineTarget->Roughness = 0.0f;
		}
	}

	delete[] pRAW;
}

#endif
