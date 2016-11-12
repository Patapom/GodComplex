#include "../Types.h"

// These values are not magical, just the default values Marsaglia used.
// Any pair of unsigned integers should be fine.
static U32	gs_w = RAND_DEFAULT_SEED_U;
static U32	gs_z = RAND_DEFAULT_SEED_V;

static U32	gs_SeedPushW = 0;
static U32	gs_SeedPushZ = 0;

void	_randpushseed()
{
	gs_SeedPushW = gs_w;
	gs_SeedPushZ = gs_z;
}
void	_randpopseed()
{
	gs_w = gs_SeedPushW;
	gs_z = gs_SeedPushZ;
}

// This is the heart of the generator.
// It uses George Marsaglia's MWC algorithm to produce an unsigned integer.
// See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
U32	_rand()
{
	gs_z = 36969 * (gs_z & 0xFFFF) + (gs_z >> 16);
	gs_w = 18000 * (gs_w & 0xFFFF) + (gs_w >> 16);
	return (gs_z << 16) + gs_w;
}

U32	_rand( U32 min, U32 max )
{
	U32	D = 1+max-min;
	U32	v = _rand() % D;
	return min + v;
}

U32	_rand( U32 size )
{
	return _rand() % size;
}

// The random generator seed can be set three ways:
// 1) specifying two non-zero unsigned integers
// 2) specifying one non-zero unsigned integer and taking a default value for the second
// 3) setting the seed from the system time
void	_srand( U32 u, U32 v )
{
	if (u != 0) gs_w = u;
	if (v != 0) gs_z = v;
}

float	_frand()
{
	U32 u = _rand();
	return u / (4294967295.0f);	// Denominator = 2^32-1
}

float	_frand( float min, float max )
{
	return min + (max-min) * _frand();
}

// Produce a uniform random sample from the open interval ]0, 1[.
// The method will not return either end point.
float	_frandStrict()
{
	// 0 <= u < 2^32
	U32 u = _rand();

	// The magic number below is 1/(2^32 + 2).
	// The result is strictly between 0 and 1.
	return float( (u + 1.0) * 2.328306435454494e-10 );
}

// Get normal (Gaussian) random sample with mean 0 and standard deviation 1
float	_randGauss()
{
	// Use Box-Muller algorithm
	float	u1 = _frand();
	float	u2 = _frand();
	float	r = sqrtf( -2.0f * logf(u1) );
	float	theta = 2.0f * PI * u2;
	return r * sinf(theta);
}
