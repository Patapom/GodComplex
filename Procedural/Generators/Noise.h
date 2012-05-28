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

protected:	// FIELDS

	float*		m_pNoise1;
	float*		m_pNoise2;
	float*		m_pNoise3;
	float*		m_pNoise4;
	float*		m_pNoise5;
	float*		m_pNoise6;
	U32*		m_pPermutation;

	float		m_WrapRadius;
	NjFloat2	m_WrapCenter0;
	NjFloat2	m_WrapCenter1;
	NjFloat2	m_WrapCenter2;

public:		// METHODS

	Noise();
// 	~Noise();	// Can't have destructors on static instances !

	void	Init( int _Seed );
	void	Exit();

	float	Noise1D( float u );
	float	Noise2D( const NjFloat2& uv );
	float	Noise3D( const NjFloat3& uvw );
	float	Noise4D( const NjFloat4& uvwr );
	float	Noise5D( const NjFloat4& uvwr, float s );
	float	Noise6D( const NjFloat4& uvwr, const NjFloat2& st );

	// Noises that wrap !
	void	SetWrappingParameters( float _Frequency, U32 _Seed );
	float	WrapNoise1D( float u );
	float	WrapNoise2D( const NjFloat2& uv );
	float	WrapNoise3D( const NjFloat3& uvw );

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
	float	TriLerp( float _p0, float _p1, float _p2, float _p3, float _p4, float _p5, float _p6, float _p7, float _x, float _y, float _z )
	{
		return	Lerp( BiLerp( _p0, _p1, _p2, _p3, _x, _y ), BiLerp( _p4, _p5, _p6, _p7, _x, _y ), _z );
	}

	float	BiLerp( float _p0, float _p1, float _p2, float _p3, float _x, float _y )
	{
		return	Lerp( Lerp( _p0, _p1, _x ), Lerp( _p3, _p2, _x ), _y );
	}

	float	Lerp( float _p0, float _p1, float _x )
	{
		return	_p0 + (_p1 - _p0) * _x;
	}

#if 1
	// 6 t^5 - 15 t^4 + 10 t^3  ==> Gives some sort of S-Shaped curve with 0 first and second derivatives at t=0 & t=1
	inline float	SCurve( float _t )
	{
		return	_t * _t * _t * (10.0f + _t * (-15.0f + _t *  6.0f));
	}
#else
	// 3 t^2 - 2 t^3  ==> Gives some sort of S-Shaped curve with 0 first derivatives at t=0 & t=1
	inline float	SCurve( float _t )
	{
		return	_t * _t * (3.0f - 2.0f * _t);
	}
#endif

	inline float	Dot( U32 _Permutation, float u )												{ return m_pNoise1[_Permutation] * u; }
	inline float	Dot( U32 _Permutation, float u, float v )										{ float* V = &m_pNoise2[_Permutation<<1]; return V[0] * u + V[1] * v; }
	inline float	Dot( U32 _Permutation, float u, float v, float w )								{ float* V = &m_pNoise3[_Permutation<<2]; return V[0] * u + V[1] * v + V[2] * w; }
	inline float	Dot( U32 _Permutation, float u, float v, float w, float r )						{ float* V = &m_pNoise4[_Permutation<<2]; return V[0] * u + V[1] * v + V[2] * w + V[3] * r; }
	inline float	Dot( U32 _Permutation, float u, float v, float w, float r, float s )			{ float* V = &m_pNoise5[_Permutation<<3]; return V[0] * u + V[1] * v + V[2] * w + V[3] * r + V[4] * s; }
	inline float	Dot( U32 _Permutation, float u, float v, float w, float r, float s, float t )	{ float* V = &m_pNoise6[_Permutation<<3]; return V[0] * u + V[1] * v + V[2] * w + V[3] * r + V[4] * s + V[5] * t; }
};
