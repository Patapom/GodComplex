#include "../GodComplex.h"

// These values are not magical, just the default values Marsaglia used.
// Any pair of unsigned integers should be fine.
static U32	m_w = 521288629;
static U32	m_z = 362436069;


// This is the heart of the generator.
// It uses George Marsaglia's MWC algorithm to produce an unsigned integer.
// See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
U32	GetUint()
{
	m_z = 36969 * (m_z & 0xFFFF) + (m_z >> 16);
	m_w = 18000 * (m_w & 0xFFFF) + (m_w >> 16);
	return (m_z << 16) + m_w;
}

// The random generator seed can be set three ways:
// 1) specifying two non-zero unsigned integers
// 2) specifying one non-zero unsigned integer and taking a default value for the second
// 3) setting the seed from the system time
void	_srand( U32 u, U32 v )
{
	if (u != 0) m_w = u; 
	if (v != 0) m_z = v;
}

// Produce a uniform random sample from the open interval ]0, 1[.
// The method will not return either end point.
float	_rand()
{
	// 0 <= u < 2^32
	U32 u = GetUint();

	// The magic number below is 1/(2^32 + 2).
	// The result is strictly between 0 and 1.
	return float( (u + 1.0) * 2.328306435454494e-10 );
}

// Get normal (Gaussian) random sample with mean 0 and standard deviation 1
float	_randGauss()
{
	// Use Box-Muller algorithm
	float	u1 = _rand();
	float	u2 = _rand();
	float	r = sqrtf( -2.0f * logf(u1) );
	float	theta = 2.0f * PI * u2;
	return r * sinf(theta);
}

