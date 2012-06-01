//////////////////////////////////////////////////////////////////////////
// Perlin Noise
//
#pragma once

#define NOISE_POT	12
#define NOISE_SIZE	(1 << NOISE_POT)
#define NOISE_MASK	((1 << NOISE_POT) - 1)

class	Noise
{
protected:	// CONSTANTS

	static const float	BIAS_U;
	static const float	BIAS_V;
	static const float	BIAS_W;
	static const float	BIAS_R;
	static const float	BIAS_S;
	static const float	BIAS_T;

public:
	// Constants used in FNV hash function (http://isthe.com/chongo/tech/comp/fnv/#FNV-source)
	static const U32	OFFSET_BASIS = 2166136261;
	static const U32	FNV_PRIME = 16777619;

public:		// NESTED TYPES

	// WARNING: Notice you get an array of three SQUARED distances !
	typedef float	(*CombineDistancesDelegate)( float _pSqDistances[] );

	typedef float	(*GetNoise2DDelegate)( const NjFloat2& _UV, void* _pData );


protected:	// FIELDS

	float*		m_pNoise1;
	float*		m_pNoise2;
	float*		m_pNoise3;
	float*		m_pNoise4;
	float*		m_pNoise5;
	float*		m_pNoise6;
	U32*		m_pPermutation;

	// Wrapping parameters for Perlin noise
	float		m_WrapRadius;
	NjFloat2	m_WrapCenter0;
	NjFloat2	m_WrapCenter1;
	NjFloat2	m_WrapCenter2;

	// Wrapping parameters for cellular/Worley noise
	int			m_SizeX;
	int			m_SizeY;
	int			m_SizeZ;

	// Wavelet noise tile
	int			m_WaveletPOT;
	int			m_WaveletSize;
	int			m_WaveletMask;
	float*		m_pWavelet2D;

public:		// METHODS

	Noise( int _Seed );
 	~Noise();

	// --------- PERLIN ---------
	float	Perlin( float u ) const;
	float	Perlin( const NjFloat2& uv ) const;
	float	Perlin( const NjFloat3& uvw ) const;
	float	Perlin( const NjFloat4& uvwr ) const;
	float	Perlin( const NjFloat4& uvwr, float s ) const;
	float	Perlin( const NjFloat4& uvwr, const NjFloat2& st ) const;

	// Noises that wrap !
	void	SetWrappingParameters( float _Frequency, U32 _Seed );
	float	WrapPerlin( float u ) const;
	float	WrapPerlin( const NjFloat2& uv ) const;
	float	WrapPerlin( const NjFloat3& uvw ) const;

	// --------- CELLULAR ---------
	void	SetCellularWrappingParameters( int _SizeX, int _SizeY, int _SizeZ );
	float	Cellular( const NjFloat2& uv, CombineDistancesDelegate _Combine, bool _bWrap=false ) const;
	float	Cellular( const NjFloat3& uvw, CombineDistancesDelegate _Combine, bool _bWrap=false ) const;
	float	Worley( const NjFloat2& uv, CombineDistancesDelegate _Combine, bool _bWrap=false ) const;
	float	Worley( const NjFloat3& uvw, CombineDistancesDelegate _Combine, bool _bWrap=false ) const;

	// --------- WAVELET ---------
	void	Create2DWaveletNoiseTile( int _POT );
	float	Wavelet( const NjFloat2& uv ) const;

	// --------- ALGORITHMS ---------
	float	FractionalBrownianMotion( GetNoise2DDelegate _GetNoise, void* _pData, const NjFloat2& uv, float _FrequencyFactor=2.0f, float _AmplitudeFactor=0.5f, int _OctavesCount=4 ) const;
	float	RidgedMultiFractal( GetNoise2DDelegate _GetNoise, void* _pData, const NjFloat2& _UV, float _FrequencyFactor=2.0f, float _AmplitudeFactor=0.5f, int _OctavesCount=4 ) const;

private:

	// Linear, Bilinear and Trilinear interpolation functions.
	// The cube is designed like this:
	//                                                                  
	//        p3         p2                                             
	//         o--------o                                               
	//        /:       /|          Y
	//     p7/ :    p6/ |          |                                    
	//      o--------o  |          |                                    
	//      |  :p0   |  |p1        |                                    
	//      |  o.....|..o          o------X
	//      | '      | /          /                                     
	//      |'       |/          /                                      
	//      o--------o          Z
	//     p4        p5                                                 
	//                                                                  
	float	TriLerp( float _p0, float _p1, float _p2, float _p3, float _p4, float _p5, float _p6, float _p7, float _x, float _y, float _z ) const
	{
		return	Lerp( BiLerp( _p0, _p1, _p2, _p3, _x, _y ), BiLerp( _p4, _p5, _p6, _p7, _x, _y ), _z );
	}

	float	BiLerp( float _p0, float _p1, float _p2, float _p3, float _x, float _y ) const
	{
		return	Lerp( Lerp( _p0, _p1, _x ), Lerp( _p3, _p2, _x ), _y );
	}

	float	Lerp( float _p0, float _p1, float _x ) const
	{
		return	_p0 + (_p1 - _p0) * _x;
	}

#if 1
	// 6 t^5 - 15 t^4 + 10 t^3  ==> Gives some sort of S-Shaped curve with 0 first and second derivatives at t=0 & t=1
	float	SCurve( float _t ) const
	{
		return	_t * _t * _t * (10.0f + _t * (-15.0f + _t *  6.0f));
	}
#else
	// 3 t^2 - 2 t^3  ==> Gives some sort of S-Shaped curve with 0 first derivatives at t=0 & t=1
	float	SCurve( float _t ) const
	{
		return	_t * _t * (3.0f - 2.0f * _t);
	}
#endif

	float	Dot( U32 _Permutation, float u ) const												{ return m_pNoise1[_Permutation] * u; }
	float	Dot( U32 _Permutation, float u, float v ) const										{ float* V = &m_pNoise2[_Permutation<<1]; return V[0] * u + V[1] * v; }
	float	Dot( U32 _Permutation, float u, float v, float w ) const							{ float* V = &m_pNoise3[_Permutation<<2]; return V[0] * u + V[1] * v + V[2] * w; }
	float	Dot( U32 _Permutation, float u, float v, float w, float r ) const					{ float* V = &m_pNoise4[_Permutation<<2]; return V[0] * u + V[1] * v + V[2] * w + V[3] * r; }
	float	Dot( U32 _Permutation, float u, float v, float w, float r, float s ) const			{ float* V = &m_pNoise5[_Permutation<<3]; return V[0] * u + V[1] * v + V[2] * w + V[3] * r + V[4] * s; }
	float	Dot( U32 _Permutation, float u, float v, float w, float r, float s, float t ) const	{ float* V = &m_pNoise6[_Permutation<<3]; return V[0] * u + V[1] * v + V[2] * w + V[3] * r + V[4] * s + V[5] * t; }

	int		PoissonPointsCount( U32 _Random ) const;

	void	WaveletDownsampleUpsample( float* _pSource, float* _pTarget, int _X, int _Y, int _Size, int _Stride ) const;

public:
	static U32		LCGRandom( U32& _LastValue );
};
