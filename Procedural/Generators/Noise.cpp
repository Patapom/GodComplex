#include "../../GodComplex.h"

// Used to generate a normalized random vector of variable size
#define GENERATE_AND_NORMALIZE( pNoise, Shift, Count )	\
{	float	SumSq = 0.0; \
	for ( int j=0; j < Count; j++ )	\
	{	float	v = pNoise[(i<<Shift)+j] = 2.0f * _frand() - 1.0f;	\
		SumSq += v*v;	\
	}\
	SumSq = 1.0f / sqrtf( SumSq );	\
	for ( int j=0; j < Count; j++ )	\
		pNoise[(i<<Shift)+j] *= SumSq;\
}

const float	Noise::BIAS_U = 0.1316519815f;
const float	Noise::BIAS_V = 0.1984632145f;
const float	Noise::BIAS_W = 0.1621987463f;
const float	Noise::BIAS_R = 0.7685431298f;
const float	Noise::BIAS_S = 0.4646579661f;
const float	Noise::BIAS_T = 0.9887465321f;

Noise::Noise( int _Seed )
	: m_pWavelet2D( NULL )
{
	_randpushseed();
	_srand( _Seed, RAND_DEFAULT_SEED_V );

	// Allocate tables
	m_pNoise1 = new float[NOISE_SIZE];
	m_pNoise2 = new float[2*NOISE_SIZE];
	m_pNoise3 = new float[4*NOISE_SIZE];
	m_pNoise4 = new float[4*NOISE_SIZE];
	m_pNoise5 = new float[8*NOISE_SIZE];
	m_pNoise6 = new float[8*NOISE_SIZE];

	m_pPermutation = new U32[2*NOISE_SIZE];

	// Fill the table of random numbers & permutations
	for ( int i=0; i < NOISE_SIZE; i++ )
	{
		m_pNoise1[i] = _frand();
		GENERATE_AND_NORMALIZE( m_pNoise2, 1, 2 );
		GENERATE_AND_NORMALIZE( m_pNoise3, 2, 3 );
		GENERATE_AND_NORMALIZE( m_pNoise4, 2, 4 );
		GENERATE_AND_NORMALIZE( m_pNoise5, 3, 5 );
		GENERATE_AND_NORMALIZE( m_pNoise6, 3, 6 );

		m_pPermutation[i] = i;
	}

	// Perform permutations
	for ( int i=0; i < NOISE_SIZE; i++ )
	{
		U32	j = _rand( NOISE_SIZE );
		U32	Temp = m_pPermutation[i];
		m_pPermutation[i] = m_pPermutation[j];
		m_pPermutation[j] = Temp;
	}
	for ( int i=0; i < NOISE_SIZE; i++ )
		m_pPermutation[NOISE_SIZE+i] = m_pPermutation[i];

	_randpopseed();

	// Arbitrary default wrapping init
	SetWrappingParameters( 0.001f, 1 );
	SetCellularWrappingParameters( 16, 16, 16 );
}

Noise::~Noise()
{
	delete[] m_pNoise1;
	delete[] m_pNoise2;
	delete[] m_pNoise3;
	delete[] m_pNoise4;
	delete[] m_pNoise5;
	delete[] m_pNoise6;
	delete[] m_pPermutation;

	if ( m_pWavelet2D != NULL )
		delete[] m_pWavelet2D;
}

// This should generate a code like this:
//
// 	float	fX0 = (BIAS_U+u) * NOISE_SIZE;
// 	int		X0_ = floorf( fX0 );
// 	float	t0 = fX0 - X0_;	float	r0 = t0 - 1.0f;
// 			X0_ = X0_ & NOISE_MASK;
// 	int		X0 = (X0_ + 1) & NOISE_MASK;
//
#define	NOISE_INDICES( Bias, Var, Index )	\
	float	fX##Index = (Bias+Var) * NOISE_SIZE;	\
 	int		X##Index##_ = floorf( fX##Index );	\
 	float	t##Index = fX##Index - X##Index##_;	float	r##Index = t##Index - 1.0f;	\
 			X##Index##_ = X##Index##_ & NOISE_MASK;	\
 	int		X##Index = (X##Index##_ + 1) & NOISE_MASK;

float	Noise::Perlin( float u ) const
{
	NOISE_INDICES( BIAS_U, u, 0 )

	float	N0 = Dot( m_pPermutation[X0_], t0 );
	float	N1 = Dot( m_pPermutation[X0], r0 );

	t0 = SCurve( t0 );

	return Lerp( N0, N1, t0 );
}

float	Noise::Perlin( const NjFloat2& uv ) const
{
	NOISE_INDICES( BIAS_U, uv.x, 0 )
	NOISE_INDICES( BIAS_V, uv.y, 1 )

	float	N00 = Dot( m_pPermutation[m_pPermutation[X0_]+X1_], t0, t1 );
	float	N01 = Dot( m_pPermutation[m_pPermutation[X0 ]+X1_], r0, t1 );
	float	N10 = Dot( m_pPermutation[m_pPermutation[X0_]+X1 ], t0, r1 );
	float	N11 = Dot( m_pPermutation[m_pPermutation[X0 ]+X1 ], r0, r1 );

	t0 = SCurve( t0 );
	t1 = SCurve( t1 );

	return BiLerp( N00, N01, N11, N10, t0, t1 );
}

float	Noise::Perlin( const NjFloat3& uvw ) const
{
	NOISE_INDICES( BIAS_U, uvw.x, 0 )
	NOISE_INDICES( BIAS_V, uvw.y, 1 )
	NOISE_INDICES( BIAS_W, uvw.z, 2 )

	float	N000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_], t0, t1, t2 );
	float	N001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_], r0, t1, t2 );
	float	N010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_], t0, r1, t2 );
	float	N011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_], r0, r1, t2 );
	float	N100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ], t0, t1, r2 );
	float	N101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ], r0, t1, r2 );
	float	N110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ], t0, r1, r2 );
	float	N111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ], r0, r1, r2 );

	t0 = SCurve( t0 );
	t1 = SCurve( t1 );
	t2 = SCurve( t2 );

	return TriLerp( N000, N001, N011, N010, N100, N101, N111, N110, t0, t1, t2 );
}

float	Noise::Perlin( const NjFloat4& uvwr ) const
{
	NOISE_INDICES( BIAS_U, uvwr.x, 0 )
	NOISE_INDICES( BIAS_V, uvwr.y, 1 )
	NOISE_INDICES( BIAS_W, uvwr.z, 2 )
	NOISE_INDICES( BIAS_R, uvwr.w, 3 )

	float	N0000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_], t0, t1, t2, t3 );
	float	N0001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_], r0, t1, t2, t3 );
	float	N0010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_], t0, r1, t2, t3 );
	float	N0011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_], r0, r1, t2, t3 );
	float	N0100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_], t0, t1, r2, t3 );
	float	N0101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_], r0, t1, r2, t3 );
	float	N0110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_], t0, r1, r2, t3 );
	float	N0111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_], r0, r1, r2, t3 );
	float	N1000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ], t0, t1, t2, r3 );
	float	N1001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ], r0, t1, t2, r3 );
	float	N1010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ], t0, r1, t2, r3 );
	float	N1011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ], r0, r1, t2, r3 );
	float	N1100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ], t0, t1, r2, r3 );
	float	N1101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ], r0, t1, r2, r3 );
	float	N1110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ], t0, r1, r2, r3 );
	float	N1111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ], r0, r1, r2, r3 );

	t0 = SCurve( t0 );
	t1 = SCurve( t1 );
	t2 = SCurve( t2 );
	t3 = SCurve( t3 );

	float	N0 = TriLerp( N0000, N0001, N0011, N0010, N0100, N0101, N0111, N0110, t0, t1, t2 );
	float	N1 = TriLerp( N1000, N1001, N1011, N1010, N1100, N1101, N1111, N1110, t0, t1, t2 );

	return Lerp( N0, N1, t3 );
}

float	Noise::Perlin( const NjFloat4& uvwr, float s ) const
{
	NOISE_INDICES( BIAS_U, uvwr.x, 0 )
	NOISE_INDICES( BIAS_V, uvwr.y, 1 )
	NOISE_INDICES( BIAS_W, uvwr.z, 2 )
	NOISE_INDICES( BIAS_R, uvwr.w, 3 )
	NOISE_INDICES( BIAS_S, s, 4 )

	float	N00000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_]+X4_], t0, t1, t2, t3, t4 );
	float	N00001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_]+X4_], r0, t1, t2, t3, t4 );
	float	N00010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_]+X4_], t0, r1, t2, t3, t4 );
	float	N00011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_]+X4_], r0, r1, t2, t3, t4 );
	float	N00100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_]+X4_], t0, t1, r2, t3, t4 );
	float	N00101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_]+X4_], r0, t1, r2, t3, t4 );
	float	N00110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_]+X4_], t0, r1, r2, t3, t4 );
	float	N00111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_]+X4_], r0, r1, r2, t3, t4 );
	float	N01000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ]+X4_], t0, t1, t2, r3, t4 );
	float	N01001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ]+X4_], r0, t1, t2, r3, t4 );
	float	N01010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ]+X4_], t0, r1, t2, r3, t4 );
	float	N01011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ]+X4_], r0, r1, t2, r3, t4 );
	float	N01100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ]+X4_], t0, t1, r2, r3, t4 );
	float	N01101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ]+X4_], r0, t1, r2, r3, t4 );
	float	N01110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ]+X4_], t0, r1, r2, r3, t4 );
	float	N01111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ]+X4_], r0, r1, r2, r3, t4 );

	float	N10000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_]+X4 ], t0, t1, t2, t3, r4 );
	float	N10001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_]+X4 ], r0, t1, t2, t3, r4 );
	float	N10010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_]+X4 ], t0, r1, t2, t3, r4 );
	float	N10011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_]+X4 ], r0, r1, t2, t3, r4 );
	float	N10100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_]+X4 ], t0, t1, r2, t3, r4 );
	float	N10101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_]+X4 ], r0, t1, r2, t3, r4 );
	float	N10110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_]+X4 ], t0, r1, r2, t3, r4 );
	float	N10111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_]+X4 ], r0, r1, r2, t3, r4 );
	float	N11000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ]+X4 ], t0, t1, t2, r3, r4 );
	float	N11001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ]+X4 ], r0, t1, t2, r3, r4 );
	float	N11010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ]+X4 ], t0, r1, t2, r3, r4 );
	float	N11011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ]+X4 ], r0, r1, t2, r3, r4 );
	float	N11100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ]+X4 ], t0, t1, r2, r3, r4 );
	float	N11101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ]+X4 ], r0, t1, r2, r3, r4 );
	float	N11110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ]+X4 ], t0, r1, r2, r3, r4 );
	float	N11111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ]+X4 ], r0, r1, r2, r3, r4 );

	t0 = SCurve( t0 );
	t1 = SCurve( t1 );
	t2 = SCurve( t2 );
	t3 = SCurve( t3 );
	t4 = SCurve( t4 );

	float	N00 = TriLerp( N00000, N00001, N00011, N00010, N00100, N00101, N00111, N00110, t0, t1, t2 );
	float	N01 = TriLerp( N01000, N01001, N01011, N01010, N01100, N01101, N01111, N01110, t0, t1, t2 );
	float	N10 = TriLerp( N10000, N10001, N10011, N10010, N10100, N10101, N10111, N10110, t0, t1, t2 );
	float	N11 = TriLerp( N11000, N11001, N11011, N11010, N11100, N11101, N11111, N11110, t0, t1, t2 );

	return BiLerp( N00, N01, N11, N10, t3, t4 );
}

float	Noise::Perlin( const NjFloat4& uvwr, const NjFloat2& st ) const
{
	NOISE_INDICES( BIAS_U, uvwr.x, 0 )
	NOISE_INDICES( BIAS_V, uvwr.y, 1 )
	NOISE_INDICES( BIAS_W, uvwr.z, 2 )
	NOISE_INDICES( BIAS_R, uvwr.w, 3 )
	NOISE_INDICES( BIAS_S, st.x, 4 )
	NOISE_INDICES( BIAS_T, st.y, 5 )

	float	N000000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_]+X4_]+X5_], t0, t1, t2, t3, t4, t5 );
	float	N000001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_]+X4_]+X5_], r0, t1, t2, t3, t4, t5 );
	float	N000010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_]+X4_]+X5_], t0, r1, t2, t3, t4, t5 );
	float	N000011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_]+X4_]+X5_], r0, r1, t2, t3, t4, t5 );
	float	N000100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_]+X4_]+X5_], t0, t1, r2, t3, t4, t5 );
	float	N000101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_]+X4_]+X5_], r0, t1, r2, t3, t4, t5 );
	float	N000110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_]+X4_]+X5_], t0, r1, r2, t3, t4, t5 );
	float	N000111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_]+X4_]+X5_], r0, r1, r2, t3, t4, t5 );
	float	N001000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ]+X4_]+X5_], t0, t1, t2, r3, t4, t5 );
	float	N001001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ]+X4_]+X5_], r0, t1, t2, r3, t4, t5 );
	float	N001010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ]+X4_]+X5_], t0, r1, t2, r3, t4, t5 );
	float	N001011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ]+X4_]+X5_], r0, r1, t2, r3, t4, t5 );
	float	N001100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ]+X4_]+X5_], t0, t1, r2, r3, t4, t5 );
	float	N001101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ]+X4_]+X5_], r0, t1, r2, r3, t4, t5 );
	float	N001110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ]+X4_]+X5_], t0, r1, r2, r3, t4, t5 );
	float	N001111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ]+X4_]+X5_], r0, r1, r2, r3, t4, t5 );
	float	N010000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_]+X4 ]+X5_], t0, t1, t2, t3, r4, t5 );
	float	N010001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_]+X4 ]+X5_], r0, t1, t2, t3, r4, t5 );
	float	N010010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_]+X4 ]+X5_], t0, r1, t2, t3, r4, t5 );
	float	N010011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_]+X4 ]+X5_], r0, r1, t2, t3, r4, t5 );
	float	N010100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_]+X4 ]+X5_], t0, t1, r2, t3, r4, t5 );
	float	N010101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_]+X4 ]+X5_], r0, t1, r2, t3, r4, t5 );
	float	N010110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_]+X4 ]+X5_], t0, r1, r2, t3, r4, t5 );
	float	N010111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_]+X4 ]+X5_], r0, r1, r2, t3, r4, t5 );
	float	N011000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ]+X4 ]+X5_], t0, t1, t2, r3, r4, t5 );
	float	N011001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ]+X4 ]+X5_], r0, t1, t2, r3, r4, t5 );
	float	N011010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ]+X4 ]+X5_], t0, r1, t2, r3, r4, t5 );
	float	N011011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ]+X4 ]+X5_], r0, r1, t2, r3, r4, t5 );
	float	N011100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ]+X4 ]+X5_], t0, t1, r2, r3, r4, t5 );
	float	N011101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ]+X4 ]+X5_], r0, t1, r2, r3, r4, t5 );
	float	N011110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ]+X4 ]+X5_], t0, r1, r2, r3, r4, t5 );
	float	N011111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ]+X4 ]+X5_], r0, r1, r2, r3, r4, t5 );

	float	N100000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_]+X4_]+X5 ], t0, t1, t2, t3, t4, r5 );
	float	N100001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_]+X4_]+X5 ], r0, t1, t2, t3, t4, r5 );
	float	N100010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_]+X4_]+X5 ], t0, r1, t2, t3, t4, r5 );
	float	N100011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_]+X4_]+X5 ], r0, r1, t2, t3, t4, r5 );
	float	N100100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_]+X4_]+X5 ], t0, t1, r2, t3, t4, r5 );
	float	N100101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_]+X4_]+X5 ], r0, t1, r2, t3, t4, r5 );
	float	N100110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_]+X4_]+X5 ], t0, r1, r2, t3, t4, r5 );
	float	N100111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_]+X4_]+X5 ], r0, r1, r2, t3, t4, r5 );
	float	N101000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ]+X4_]+X5 ], t0, t1, t2, r3, t4, r5 );
	float	N101001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ]+X4_]+X5 ], r0, t1, t2, r3, t4, r5 );
	float	N101010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ]+X4_]+X5 ], t0, r1, t2, r3, t4, r5 );
	float	N101011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ]+X4_]+X5 ], r0, r1, t2, r3, t4, r5 );
	float	N101100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ]+X4_]+X5 ], t0, t1, r2, r3, t4, r5 );
	float	N101101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ]+X4_]+X5 ], r0, t1, r2, r3, t4, r5 );
	float	N101110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ]+X4_]+X5 ], t0, r1, r2, r3, t4, r5 );
	float	N101111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ]+X4_]+X5 ], r0, r1, r2, r3, t4, r5 );
	float	N110000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3_]+X4 ]+X5 ], t0, t1, t2, t3, r4, r5 );
	float	N110001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3_]+X4 ]+X5 ], r0, t1, t2, t3, r4, r5 );
	float	N110010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3_]+X4 ]+X5 ], t0, r1, t2, t3, r4, r5 );
	float	N110011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3_]+X4 ]+X5 ], r0, r1, t2, t3, r4, r5 );
	float	N110100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3_]+X4 ]+X5 ], t0, t1, r2, t3, r4, r5 );
	float	N110101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3_]+X4 ]+X5 ], r0, t1, r2, t3, r4, r5 );
	float	N110110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3_]+X4 ]+X5 ], t0, r1, r2, t3, r4, r5 );
	float	N110111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3_]+X4 ]+X5 ], r0, r1, r2, t3, r4, r5 );
	float	N111000 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2_]+X3 ]+X4 ]+X5 ], t0, t1, t2, r3, r4, r5 );
	float	N111001 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2_]+X3 ]+X4 ]+X5 ], r0, t1, t2, r3, r4, r5 );
	float	N111010 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2_]+X3 ]+X4 ]+X5 ], t0, r1, t2, r3, r4, r5 );
	float	N111011 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2_]+X3 ]+X4 ]+X5 ], r0, r1, t2, r3, r4, r5 );
	float	N111100 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1_]+X2 ]+X3 ]+X4 ]+X5 ], t0, t1, r2, r3, r4, r5 );
	float	N111101 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1_]+X2 ]+X3 ]+X4 ]+X5 ], r0, t1, r2, r3, r4, r5 );
	float	N111110 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0_]+X1 ]+X2 ]+X3 ]+X4 ]+X5 ], t0, r1, r2, r3, r4, r5 );
	float	N111111 = Dot( m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[m_pPermutation[X0 ]+X1 ]+X2 ]+X3 ]+X4 ]+X5 ], r0, r1, r2, r3, r4, r5 );

	t0 = SCurve( t0 );
	t1 = SCurve( t1 );
	t2 = SCurve( t2 );
	t3 = SCurve( t3 );
	t4 = SCurve( t4 );
	t5 = SCurve( t5 );

	float	N000 = TriLerp( N000000, N000001, N000011, N000010, N000100, N000101, N000111, N000110, t0, t1, t2 );
	float	N001 = TriLerp( N001000, N001001, N001011, N001010, N001100, N001101, N001111, N001110, t0, t1, t2 );
	float	N010 = TriLerp( N010000, N010001, N010011, N010010, N010100, N010101, N010111, N010110, t0, t1, t2 );
	float	N011 = TriLerp( N011000, N011001, N011011, N011010, N011100, N011101, N011111, N011110, t0, t1, t2 );

	float	N100 = TriLerp( N100000, N100001, N100011, N100010, N100100, N100101, N100111, N100110, t0, t1, t2 );
	float	N101 = TriLerp( N101000, N101001, N101011, N101010, N101100, N101101, N101111, N101110, t0, t1, t2 );
	float	N110 = TriLerp( N110000, N110001, N110011, N110010, N110100, N110101, N110111, N110110, t0, t1, t2 );
	float	N111 = TriLerp( N111000, N111001, N111011, N111010, N111100, N111101, N111111, N111110, t0, t1, t2 );

	return TriLerp( N000, N001, N011, N010, N100, N101, N111, N110, t3, t4, t5 );
}

NjFloat2	Noise::PerlinVector( const NjFloat2& _uv ) const
{
	NjFloat2	uv = _uv;
	NjFloat2	Result;
	Result.x = Perlin( uv );	uv.x += BIAS_U; uv.y += BIAS_V;
	Result.y = Perlin( uv );
	return Result;
}

NjFloat3	Noise::PerlinVector( const NjFloat3& _uvw ) const
{
	NjFloat3	uvw = _uvw;
	NjFloat3	Result;
	Result.x = Perlin( uvw );	uvw.x += BIAS_U; uvw.y += BIAS_V; uvw.z += BIAS_W;
	Result.y = Perlin( uvw );	uvw.x += BIAS_R; uvw.y += BIAS_S; uvw.z += BIAS_T;
	Result.z = Perlin( uvw );
	return Result;
}

void	Noise::SetWrappingParameters( float _Frequency, U32 _Seed )
{
	m_WrapRadius = _Frequency * 0.5f;

	_randpushseed();
	_srand( _Seed, RAND_DEFAULT_SEED_V );

	m_WrapCenter0 = NjFloat2( _frand(), _frand() );
	m_WrapCenter1 = NjFloat2( _frand(), _frand() );
	m_WrapCenter2 = NjFloat2( _frand(), _frand() );

	_randpopseed();
}

float	Noise::WrapPerlin( float u ) const
{
	float		Angle = TWOPI * u;
	NjFloat2	Pos( m_WrapCenter0.x + cosf( Angle ), m_WrapCenter0.y + sinf( Angle ) );
	return Perlin( Pos );
}

float	Noise::WrapPerlin( const NjFloat2& uv ) const
{
	float		Angle0 = TWOPI * uv.x;
	float		Angle1 = TWOPI * uv.y;
	NjFloat4	Pos( m_WrapCenter0.x + m_WrapRadius * cosf( Angle0 ), m_WrapCenter0.y + m_WrapRadius * sinf( Angle0 ), m_WrapCenter1.x + m_WrapRadius * cosf( Angle1 ), m_WrapCenter1.y + m_WrapRadius * sinf( Angle1 ) );
	return Perlin( Pos );
}

float	Noise::WrapPerlin( const NjFloat3& uvw ) const
{
	float		Angle0 = TWOPI * uvw.x;
	float		Angle1 = TWOPI * uvw.y;
	float		Angle2 = TWOPI * uvw.z;
	NjFloat4	Pos0( m_WrapCenter0.x + m_WrapRadius * cosf( Angle0 ), m_WrapCenter0.y + m_WrapRadius * sinf( Angle0 ), m_WrapCenter1.x + m_WrapRadius * cosf( Angle1 ), m_WrapCenter1.y + m_WrapRadius * sinf( Angle1 ) );
	NjFloat2	Pos1( m_WrapCenter2.x + m_WrapRadius * cosf( Angle2 ), m_WrapCenter2.y + m_WrapRadius * sinf( Angle2 ) );
	return Perlin( Pos0, Pos1 );
}

//////////////////////////////////////////////////////////////////////////
// Cellular noise
void	Noise::SetCellularWrappingParameters( int _SizeX, int _SizeY, int _SizeZ )
{
	m_SizeX = _SizeX;
	m_SizeY = _SizeY;
	m_SizeZ = _SizeZ;
}

float	Noise::Cellular( const NjFloat2& _UV, CombineDistancesDelegate _Combine, void* _pData, bool _bWrap ) const
{
	int	CellX = floorf( _UV.x );
	int	CellY = floorf( _UV.y );

	// Read center spot offset for all 9 cells and choose closest distance
	float	pSqDistances[3] = { FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX };
	int		pCellX[3] = { -1, -1, -1 };
	int		pCellY[3] = { -1, -1, -1 };

	for ( int Y=CellY-1; Y <= CellY+1; Y++ )
		for ( int X=CellX-1; X <= CellX+1; X++ )
		{
			U32	Hx = _bWrap ? (X + 100*m_SizeX) % m_SizeX : X;
			U32	Hy = _bWrap ? (Y + 100*m_SizeY) % m_SizeY : Y;

			// Hash two integers into a single integer using FNV hash (http://isthe.com/chongo/tech/comp/fnv/#FNV-source)
			U32	Hash = U32( (((OFFSET_BASIS ^ Hx) * FNV_PRIME) ^ Hy) * FNV_PRIME );

			NjFloat2	CellCenter;
			CellCenter.x = X + LCGRandom( Hash ) * 2.3283064370807973754314699618685e-10f;
			CellCenter.y = Y + LCGRandom( Hash ) * 2.3283064370807973754314699618685e-10f;

			// Check if the distance to that point is the closest
			float	SqDistance = (_UV - CellCenter).LengthSq();
			if ( SqDistance < pSqDistances[0] )
			{
				pSqDistances[2] = pSqDistances[1];
				pSqDistances[1] = pSqDistances[0];
				pSqDistances[0] = SqDistance;

				pCellX[2] = pCellX[1];
				pCellX[1] = pCellX[0];
				pCellX[0] = Hx;

				pCellY[2] = pCellY[1];
				pCellY[1] = pCellY[0];
				pCellY[0] = Hy;
			}
			else if ( SqDistance < pSqDistances[1] )
			{
				pSqDistances[2] = pSqDistances[1];
				pSqDistances[1] = SqDistance;

				pCellX[2] = pCellX[1];
				pCellX[1] = Hx;

				pCellY[2] = pCellY[1];
				pCellY[1] = Hy;
			}
			else if ( SqDistance < pSqDistances[2] )
			{
				pSqDistances[2] = SqDistance;

				pCellX[2] = Hx;
				pCellY[2] = Hy;
			}
		}

	return _Combine( pSqDistances, pCellX, pCellY, NULL, _pData );
}

float	Noise::Cellular( const NjFloat3& _UVW, CombineDistancesDelegate _Combine, void* _pData, bool _bWrap ) const
{
	int	CellX = floorf( _UVW.x );
	int	CellY = floorf( _UVW.y );
	int	CellZ = floorf( _UVW.z );

	float		pSqDistances[3] = { FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX };	// Only keep the 3 closest distances
	int			pCellX[3] = { -1, -1, -1 };
	int			pCellY[3] = { -1, -1, -1 };
	int			pCellZ[3] = { -1, -1, -1 };

	// Read center spot offset for all 9 cells and choose closest distance
	float	MinSqDistance = FLOAT32_MAX;
	for ( int Z=CellZ-1; Z <= CellZ+1; Z++ )
		for ( int Y=CellY-1; Y <= CellY+1; Y++ )
			for ( int X=CellX-1; X <= CellX+1; X++ )
			{
				U32	Hx = _bWrap ? (X + 100*m_SizeX) % m_SizeX : X;
				U32	Hy = _bWrap ? (Y + 100*m_SizeY) % m_SizeY : Y;
				U32	Hz = _bWrap ? (Z + 100*m_SizeZ) % m_SizeZ : Z;

				// Hash three integers into a single integer using FNV hash (http://isthe.com/chongo/tech/comp/fnv/#FNV-source)
				U32	Hash = U32( (((((OFFSET_BASIS ^ Hx) * FNV_PRIME) ^ Hy) * FNV_PRIME) ^ Hz) * FNV_PRIME );

				NjFloat3	CellCenter;
				CellCenter.x = X + LCGRandom( Hash ) * 2.3283064370807973754314699618685e-10f;
				CellCenter.y = Y + LCGRandom( Hash ) * 2.3283064370807973754314699618685e-10f;
				CellCenter.z = Z + LCGRandom( Hash ) * 2.3283064370807973754314699618685e-10f;

				// Check if the distance to that point is the closest
				float	SqDistance = (_UVW - CellCenter).LengthSq();
				if ( SqDistance < pSqDistances[0] )
				{
					pSqDistances[2] = pSqDistances[1];
					pSqDistances[1] = pSqDistances[0];
					pSqDistances[0] = SqDistance;

					pCellX[2] = pCellX[1];
					pCellX[1] = pCellX[0];
					pCellX[0] = Hx;

					pCellY[2] = pCellY[1];
					pCellY[1] = pCellY[0];
					pCellY[0] = Hy;

					pCellZ[2] = pCellZ[1];
					pCellZ[1] = pCellZ[0];
					pCellZ[0] = Hz;
				}
				else if ( SqDistance < pSqDistances[1] )
				{
					pSqDistances[2] = pSqDistances[1];
					pSqDistances[1] = SqDistance;

					pCellX[2] = pCellX[1];
					pCellX[1] = Hx;

					pCellY[2] = pCellY[1];
					pCellY[1] = Hy;

					pCellZ[2] = pCellZ[1];
					pCellZ[1] = Hz;
				}
				else if ( SqDistance < pSqDistances[2] )
				{
					pSqDistances[2] = SqDistance;

					pCellX[2] = Hx;
					pCellY[2] = Hy;
					pCellZ[2] = Hz;
				}
			}

	return _Combine( pSqDistances, pCellX, pCellY, pCellZ, _pData );
}

//////////////////////////////////////////////////////////////////////////
// Worley Noise (from https://github.com/freethenation/CellNoiseDemo)
// I though about implementing the optimization suggested in Worley's paper to skip inelegible neighbor cubes that are too far away but I remembered I am writing a 64K intro... Less code the better !
float	Noise::Worley( const NjFloat2& _UV, CombineDistancesDelegate _Combine, void* _pData, bool _bWrap ) const
{
	int	CellX = floorf( _UV.x );
	int CellY = floorf( _UV.y );

	NjFloat2	Point;

	float		pSqDistances[3] = { FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX };	// Only keep the 3 closest distances
	int			pCellX[3] = { -1, -1, -1 };
	int			pCellY[3] = { -1, -1, -1 };

	for ( int Y=CellY-1; Y <= CellY+1; Y++ )
		for ( int X=CellX-1; X <= CellX+1; X++ )
		{
			U32	Hx = _bWrap ? (X + 100*m_SizeX) % m_SizeX : X;
			U32	Hy = _bWrap ? (Y + 100*m_SizeY) % m_SizeY : Y;

			// Hash two integers into a single integer using FNV hash (http://isthe.com/chongo/tech/comp/fnv/#FNV-source)
			U32	Hash = U32( (((OFFSET_BASIS ^ Hx) * FNV_PRIME) ^ Hy) * FNV_PRIME );

			// Determine how many feature points are in the square
			int	PointsCount = PoissonPointsCount( Hash );

			// Randomly place the feature points in the square & find the closest distance
			for ( int PointIndex=0; PointIndex < PointsCount; PointIndex++ )
			{
				Hash = LCGRandom(Hash);
				Point.x = X + Hash * 2.3283064370807973754314699618685e-10f;

				Hash = LCGRandom(Hash);
				Point.y = Y + Hash * 2.3283064370807973754314699618685e-10f;

				// Check if the distance to that point is the closest
				float	SqDistance = (Point - _UV).LengthSq();
				if ( SqDistance < pSqDistances[0] )
				{
					pSqDistances[2] = pSqDistances[1];
					pSqDistances[1] = pSqDistances[0];
					pSqDistances[0] = SqDistance;

					pCellX[2] = pCellX[1];
					pCellX[1] = pCellX[0];
					pCellX[0] = Hx;

					pCellY[2] = pCellY[1];
					pCellY[1] = pCellY[0];
					pCellY[0] = Hy;
				}
				else if ( SqDistance < pSqDistances[1] )
				{
					pSqDistances[2] = pSqDistances[1];
					pSqDistances[1] = SqDistance;

					pCellX[2] = pCellX[1];
					pCellX[1] = Hx;

					pCellY[2] = pCellY[1];
					pCellY[1] = Hy;
				}
				else if ( SqDistance < pSqDistances[2] )
				{
					pSqDistances[2] = SqDistance;

					pCellX[2] = Hx;
					pCellY[2] = Hy;
				}
			}
		}

	return _Combine( pSqDistances, pCellX, pCellY, NULL, _pData );
}

float	Noise::Worley( const NjFloat3& _UVW, CombineDistancesDelegate _Combine, void* _pData, bool _bWrap ) const
{
	int	CellX = floorf( _UVW.x );
	int CellY = floorf( _UVW.y );
	int CellZ = floorf( _UVW.z );

	NjFloat3	Point;
	float		pSqDistances[3] = { FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX };	// Only keep the 3 closest distances
	int			pCellX[3] = { -1, -1, -1 };
	int			pCellY[3] = { -1, -1, -1 };
	int			pCellZ[3] = { -1, -1, -1 };

	for ( int Z=CellZ-1; Z <= CellZ+1; Z++ )
		for ( int Y=CellY-1; Y <= CellY+1; Y++ )
			for ( int X=CellX-1; X <= CellX+1; X++ )
			{
				U32	Hx = _bWrap ? (X + 100*m_SizeX) % m_SizeX : X;
				U32	Hy = _bWrap ? (Y + 100*m_SizeY) % m_SizeY : Y;
				U32	Hz = _bWrap ? (Z + 100*m_SizeZ) % m_SizeZ : Z;

				// Hash three integers into a single integer using FNV hash (http://isthe.com/chongo/tech/comp/fnv/#FNV-source)
				U32	Hash = U32( (((((OFFSET_BASIS ^ Hx) * FNV_PRIME) ^ Hy) * FNV_PRIME) ^ Hz) * FNV_PRIME );

				// Determine how many feature points are in the cube
				int	PointsCount = PoissonPointsCount( Hash );

				// Randomly place the feature points in the cube & find the closest distance
				for ( int PointIndex=0; PointIndex < PointsCount; PointIndex++ )
				{
					Hash = LCGRandom(Hash);
					Point.x = X + Hash * 2.3283064370807973754314699618685e-10f;

					Hash = LCGRandom(Hash);
					Point.y = Y + Hash * 2.3283064370807973754314699618685e-10f;

					Hash = LCGRandom(Hash);
					Point.z = Z + Hash * 2.3283064370807973754314699618685e-10f;

					// Check if the distance to that point is the closest
					float	SqDistance = (Point - _UVW).LengthSq();
					if ( SqDistance < pSqDistances[0] )
					{
						pSqDistances[2] = pSqDistances[1];
						pSqDistances[1] = pSqDistances[0];
						pSqDistances[0] = SqDistance;

						pCellX[2] = pCellX[1];
						pCellX[1] = pCellX[0];
						pCellX[0] = Hx;

						pCellY[2] = pCellY[1];
						pCellY[1] = pCellY[0];
						pCellY[0] = Hy;

						pCellZ[2] = pCellZ[1];
						pCellZ[1] = pCellZ[0];
						pCellZ[0] = Hz;
					}
					else if ( SqDistance < pSqDistances[1] )
					{
						pSqDistances[2] = pSqDistances[1];
						pSqDistances[1] = SqDistance;

						pCellX[2] = pCellX[1];
						pCellX[1] = Hx;

						pCellY[2] = pCellY[1];
						pCellY[1] = Hy;

						pCellZ[2] = pCellZ[1];
						pCellZ[1] = Hz;
					}
					else if ( SqDistance < pSqDistances[2] )
					{
						pSqDistances[2] = SqDistance;

						pCellX[2] = Hx;
						pCellY[2] = Hy;
						pCellZ[2] = Hz;
					}
				}
			}

	return _Combine( pSqDistances, pCellX, pCellY, pCellZ, _pData );
}

U32	Noise::LCGRandom( U32& _LastValue )
{
	return _LastValue = U32( (1103515245u * _LastValue + 12345u) );
}

#if 0
// Generated using mathmatica with "AccountingForm[N[Table[CDF[PoissonDistribution[4], i], {i, 1, 9}], 20]*2^32]"
// Follows Poisson distribution: http://en.wikipedia.org/wiki/Poisson_distribution
int	Noise::PoissonPointsCount( U32 _Random ) const
{
	if ( _Random < 393325350 ) return 1;
	else if ( _Random < 1022645910 ) return 2;
	else if ( _Random < 1861739990 ) return 3;
	else if ( _Random < 2700834071 ) return 4;
	else if ( _Random < 3372109335 ) return 5;
	else if ( _Random < 3819626178 ) return 6;
	else if ( _Random < 4075350088 ) return 7;
	else if ( _Random < 4203212043 ) return 8;

	return 9;
}
#else
// Same but with less points
int	Noise::PoissonPointsCount( U32 _Random ) const
{
// Value		Normalized							Complemented
//  393325350	0.09157819442720576059706643237664	0.90842180557279423940293356762336
// 1022645910	0.23810330551073497755237272417927	0.76189669448926502244762727582073
// 1861739990	0.43347012028877393349278111324943	0.56652987971122606650721888675057
// 2700834071	0.62883693529964353314126923986274	0.37116306470035646685873076013726
// 3372109335	0.78513038712207469789359595111888	0.21486961287792530210640404888112
// 3819626178	0.88932602174797235563117367113735	0.11067397825202764436882632886265
// 4075350088	0.948866384324819404707481014707	0.051133615675180595292518985293
// 4203212043	0.97863656561324292924563468649183	0.02136343438675707075436531350817

// 	{	// This is used to GENERATE the table of propability distributions
// 		// (from http://en.wikipedia.org/wiki/Poisson_distribution)
// 		//
// 		// Poisson distribution is Pr( X = k ) = lambda^k . e^-lambda / k!
// 		// X = amount of points in the cube
// 		// k = average points count
// 		//
// 		const int		AVERAGE_POINTS = 2;		// k = the average number of points we need per cell
// 		const int		FACT_K = 1*2;			// k!
// 		const int		MAX_POINTS = 8;
// 
// 		float	pProbabilities[MAX_POINTS+1];
// 		float	pProbabilityOffsets[MAX_POINTS+1];
// 		for ( int i=0; i <= MAX_POINTS; i++ )
// 		{
// 			int	PointsCount = i+1;	// Amount of points we expect
// 			pProbabilities[i] = powf( float(PointsCount), float(AVERAGE_POINTS) ) * expf( -float(PointsCount) ) / FACT_K;
// 			pProbabilityOffsets[i] = (i > 0 ? pProbabilityOffsets[i-1] : 0) + pProbabilities[i];
// 		}
// 
// 		U32	pNumbers[MAX_POINTS];
// 		for ( int i=0; i < MAX_POINTS; i++ )
// 		{
// //			float	fNormalizedProbability = pProbabilityOffsets[i] / pProbabilityOffsets[MAX_POINTS];
// 			float	fNormalizedProbability = pProbabilityOffsets[i];
// 			pNumbers[i] = U32( 4294967296.0f * fNormalizedProbability );
// 		}
// 
// 		pNumbers[MAX_POINTS] = 0;
// 	}

	if ( _Random < 	 790015040 ) return 1;
	if ( _Random < 	1952536192 ) return 2;
	if ( _Random < 	2914788352 ) return 3;
	if ( _Random < 	3544108800 ) return 4;
// 	if ( _Random < 	3905849344 ) return 5;
// 	if ( _Random < 	4097480192 ) return 6;
	return 5;
}
#endif

//////////////////////////////////////////////////////////////////////////
// Wavelet Noise
// Taken from http://graphics.pixar.com/library/WaveletNoise/paper.pdf
//
#define GETNOISE( pNoise, x, y )	pNoise[((y)<<_POT)+(x)]
void	Noise::Create2DWaveletNoiseTile( int _POT )
{
	if ( m_pWavelet2D != NULL )
		delete[] m_pWavelet2D;

	int	Size = 1 << _POT;
	int	HalfSize = Size >> 1;
	int	Mask = Size - 1;
	int	HalfMask = Mask >> 1;

	m_WaveletPOT = _POT;
	m_WaveletSize = Size;
	m_WaveletMask = Mask;
	m_pWavelet2D = new float[Size*Size];
	float*	ppTemp[2] =
	{
		new float[Size*Size],
		new float[Size*Size]
	};

	// Fill with gaussian noise
	for ( int Y=0; Y < Size; Y++ )
		for ( int X=0; X < Size; X++ )
		{
			// FFS! Ain't the gaussian RNG supposed to yield values in [-1,1] ???
// 			float	Rand = _randGauss();
// 			Rand = CLAMP( Rand, -1.0f, 1.0f );
			float	Rand = 0.25f * _randGauss();

			GETNOISE( m_pWavelet2D, X, Y) = GETNOISE( ppTemp[0], X, Y ) = Rand;
		}

	// Wavelet analysis coefficients for quadratic B-splines (cf. Chapter 3.3 of the paper)
	float	pWeights_[2*16] =
	{
		0.000334,-0.001528, 0.000410, 0.003545,-0.000938,-0.008233, 0.002172, 0.019120,
		-0.005040,-0.044412, 0.011655, 0.103311,-0.025936,-0.243780, 0.033979, 0.655340,
		0.655340, 0.033979,-0.243780,-0.025936, 0.103311, 0.011655,-0.044412,-0.005040,
		0.019120, 0.002172,-0.008233,-0.000938, 0.003546, 0.000410,-0.001528, 0.000334
	};
	float*	pWeights = &pWeights_[16];

	//////////////////////////////////////////////////////////////////////////
	// Horizontal processing
	for ( int y=0; y < Size; y++ )
	{
		// Downsample
		for ( int x=0; x < HalfSize; x++ )
		{
			GETNOISE( ppTemp[1], x, y ) = 0.0f;
			for ( int k=-16; k < 16; k++ )
				GETNOISE( ppTemp[1], x, y ) += pWeights[k] * GETNOISE( ppTemp[0], (x+k) & Mask, y );
		}
		
		// Upsample again by reconstructing lower frequency quadratic B-spline
		for ( int x=0; x < Size; x++ )
		{
			GETNOISE( ppTemp[0], x, y )  = 0.25f * (GETNOISE( ppTemp[1], (x-1)&HalfMask, y ) + GETNOISE( ppTemp[1], (x+2)&HalfMask, y ));
			GETNOISE( ppTemp[0], x, y ) += 0.75f * (GETNOISE( ppTemp[1], x, y ) + GETNOISE( ppTemp[1], (x+1)&HalfMask, y ));
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Vertical processing
	for ( int x=0; x < Size; x++ )
	{
		// Downsample
		for ( int y=0; y < HalfSize; y++ )
		{
			GETNOISE( ppTemp[1], x, y ) = 0.0f;
			for ( int k=-16; k < 16; k++ )
				GETNOISE( ppTemp[1], x, y ) += pWeights[k] * GETNOISE( ppTemp[0], x, (y+k) & Mask );
		}
		
		// Upsample again by reconstructing lower frequency quadratic B-spline
		for ( int y=0; y < Size; y++ )
		{
			GETNOISE( ppTemp[0], x, y )  = 0.25f * (GETNOISE( ppTemp[1], x, (y-1)&HalfMask ) + GETNOISE( ppTemp[1], x, (y+2)&HalfMask ));
			GETNOISE( ppTemp[0], x, y ) += 0.75f * (GETNOISE( ppTemp[1], x, y ) + GETNOISE( ppTemp[1], x, (y+1)&HalfMask ));
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Subtract low-frequency version of the noise to the original noise, leaving only high frequency residual (i.e. wavelet space)
	for ( int y=0; y < Size; y++ )
	{
		float*	pSourceScanline = &ppTemp[0][y<<_POT];
		float*	pTargetScanline = &m_pWavelet2D[y<<_POT];
		for ( int x=0; x < Size; x++ )
			*pTargetScanline++ -= *pSourceScanline++;
	}

	//////////////////////////////////////////////////////////////////////////
	// Avoid even/odd variance difference by adding odd-offset version of noise to itself.
	int Offset = HalfSize + 1;
	for ( int y=0; y < Size; y++ )
	{
		float*	pTargetScanline = &ppTemp[0][y<<_POT];
		for ( int x=0; x < Size; x++ )
			*pTargetScanline++ = GETNOISE( m_pWavelet2D, (Offset+y) & Mask, (Offset+x) & Mask );
	}

	for ( int y=0; y < Size; y++ )
	{
		float*	pSourceScanline = &ppTemp[0][y<<_POT];
		float*	pTargetScanline = &m_pWavelet2D[y<<_POT];
		for ( int x=0; x < Size; x++ )
			*pTargetScanline++ += *pSourceScanline++;
	}

	delete[] ppTemp[0];
	delete[] ppTemp[1];
}

float	Noise::Wavelet( const NjFloat2& _UV ) const
{
	ASSERT( m_pWavelet2D != NULL, "Did you forget to call Create2DWaveletNoiseTile() ?" );

	float	pPixelPosition[2] =	{ _UV.x * m_WaveletSize, _UV.y * m_WaveletSize };

//return m_pWavelet2D[floorf(pPixelPosition[0]) + (floorf(pPixelPosition[1]) << m_WaveletPOT)];

	// Evaluate quadratic B-spline basis functions
	int		pPixelCenter[2];
	float	ppWeights[2][3];
	for ( int i=0; i < 2; i++ )
	{
		float	fPositionOffset = pPixelPosition[i] - 0.5f;
		pPixelCenter[i] = ceilf( fPositionOffset );
		float	t = pPixelCenter[i] - fPositionOffset;
		float	r = 1.0f - t;

		// Weights for 3D query
		ppWeights[i][0] = 0.5f * t*t;
		ppWeights[i][2] = 0.5f * r*r;
		ppWeights[i][1] = 1.0f - ppWeights[i][0] - ppWeights[i][2];
	}

	// Evaluate noise by weighting noise coefficients by basis function values
	float	Result = 0.0f;
	int		f[2];
	int		c[2];
	for ( f[1]=-1; f[1] <= 1; f[1]++ )
	{
		c[1] = (pPixelCenter[1] + f[1]) & m_WaveletMask;
		for ( f[0]=-1; f[0] <= 1; f[0]++ )
		{
			c[0] = (pPixelCenter[0] + f[0]) & m_WaveletMask;

			float Weight = 1.0f;
			for ( int i=0; i < 2; i++ )
				Weight *= ppWeights[i][f[i]+1];

			Result += Weight * m_pWavelet2D[(c[1] << m_WaveletPOT)+c[0]];
		}
	}

	return Result;
}

//////////////////////////////////////////////////////////////////////////
// Algorithms
float	Noise::FractionalBrownianMotion( GetNoise2DDelegate _GetNoise, void* _pData, const NjFloat2& _UV, float _FrequencyFactor, float _AmplitudeFactor, int _OctavesCount ) const
{
	NjFloat2	UV = _UV;

	float		Result = 0.0f;
	float		Frequency = 1.0f;
	float		Amplitude = 1.0f;
	float		SumAmplitudes = 0.0f;
	for ( int Octave=0; Octave < _OctavesCount; Octave++ )
	{
		float	NoiseValue = _GetNoise( UV, _pData );
		Result += Amplitude * NoiseValue;

		SumAmplitudes += Amplitude;
		Amplitude *= _AmplitudeFactor;
		UV.x *= _FrequencyFactor;
		UV.y *= _FrequencyFactor;
	}

	Result /= SumAmplitudes;
	return Result;
}

float	Noise::RidgedMultiFractal( GetNoise2DDelegate _GetNoise, void* _pData, const NjFloat2& _UV, float _FrequencyFactor, float _AmplitudeFactor, int _OctavesCount ) const
{
	NjFloat2	UV = _UV;

	float		Result = 0.0f;
	float		Frequency = 1.0f;
	float		Amplitude = 1.0f;
	float		PreviousNoise = 1.0f;
	float		SumAmplitudes = 0.0f;
	for ( int Octave=0; Octave < _OctavesCount; Octave++ )
	{
		float	NoiseValue = _GetNoise( UV, _pData );
//				NoiseValue *= NoiseValue;	// Try with an EXP !
				NoiseValue = expf( -NoiseValue*NoiseValue );
		Result += Amplitude * PreviousNoise * NoiseValue;
		PreviousNoise = NoiseValue;

		SumAmplitudes += Amplitude;
		Amplitude *= _AmplitudeFactor;
		UV.x *= _FrequencyFactor;
		UV.y *= _FrequencyFactor;
	}

	Result = Result / SumAmplitudes;
	return Result;
}

/*
//////////////////////////////////////////////////////////////////////////
// Wavelet Noise tile generation
// Note: this code is designed for brevity, not efficiency; many operations can be hoisted,
* precomputed, or vectorized. Some of the straightforward details, such as tile meshing,
* decorrelating bands and fading out the last band, are omitted in the interest of space.

static float*	noiseTileData;
static int		noiseTileSize;

int Mod(int x, int n) { int m=x%n; return (m<0) ? m+n : m; }

#define ARAD 16
void Downsample( float* from, float* to, int n, int stride )
{
	float* a, aCoeffs[2*ARAD] =
	{
		0.000334,-0.001528, 0.000410, 0.003545,-0.000938,-0.008233, 0.002172, 0.019120,
		-0.005040,-0.044412, 0.011655, 0.103311,-0.025936,-0.243780, 0.033979, 0.655340,
		0.655340, 0.033979,-0.243780,-0.025936, 0.103311, 0.011655,-0.044412,-0.005040,
		0.019120, 0.002172,-0.008233,-0.000938, 0.003546, 0.000410,-0.001528, 0.000334
	};
	a = &aCoeffs[ARAD];

	for ( int i=0; i<n/2; i++ )
	{
		to[i*stride] = 0;
		for ( int k=-ARAD; k<=ARAD; k++ )
			to[i*stride] += a[k] * from[Mod( 2*i+k, n ) * stride];
	}
}

void Upsample( float *from, float *to, int n, int stride)
{
	float	pCoeffs[4] = { 0.25, 0.75, 0.75, 0.25 };
	float*	p = &pCoeffs[2];
	for ( int i=0; i<n; i++ )
	{
		to[i*stride] = 0;
// 		for (int k=i/2; k<=i/2+1; k++)
// 			to[i*stride] += p[i-2*k] * from[Mod(i/2 + k,n/2)*stride];

		to[i*stride] += p[i-2*k] * from[Mod(i/2 + k,n/2)*stride];
	}
}

void GenerateNoiseTile( int n, int olap)
{
	if (n%2) n++; // tile size must be even 

	int sz=n*n*n*sizeof(float);
	float*	temp1 = malloc(sz);
	float*	temp2 = malloc(sz);
	float*	noise= malloc(sz);

	// Step 1. Fill the tile with random numbers in the range -1 to 1. 
	for (i=0; i<n*n*n; i++)
		noise[i] = gaussianNoise();

	// Steps 2 and 3. Downsample and upsample the tile 
	for (int iy=0; iy<n; iy++)
		for (int iz=0; iz<n; iz++)
		{ // each x row 
			i = iy*n + iz*n*n;
			Downsample( &noise[i], &temp1[i], n, 1 );
			Upsample( &temp1[i], &temp2[i], n, 1 );
		}

	for (int ix=0; ix<n; ix++)
		for (int iz=0; iz<n; iz++)
		{ // each y row 
			i = ix + iz*n*n;
			Downsample( &temp2[i], &temp1[i], n, n );
			Upsample( &temp1[i], &temp2[i], n, n );
		}

	for (int ix=0; ix<n; ix++)
		for (int iy=0; iy<n; iy++)
		{ // each z row 
			i = ix + iy*n;
			Downsample( &temp2[i], &temp1[i], n, n*n );
			Upsample( &temp1[i], &temp2[i], n, n*n );
		}

	// Step 4. Subtract out the coarse-scale contribution 
	for (i=0; i<n*n*n; i++)
		noise[i] -= temp2[i];

	// Avoid even/odd variance difference by adding odd-offset version of noise to itself.
	int offset=n/2;
	if ( offset%2 == 0 )
		offset++;

	for ( i=0,ix=0; ix<n; ix++ )
		for ( iy=0; iy<n; iy++ )
			for ( iz=0; iz<n; iz++ )
				temp1[i++] = noise[ Mod(ix+offset,n) + Mod(iy+offset,n)*n + Mod(iz+offset,n)*n*n ];

	for (i=0; i<n*n*n; i++ )
		noise[i] += temp1[i];

	free(temp1);
	free(temp2);
	noiseTileData = noise; noiseTileSize = n;
}


//////////////////////////////////////////////////////////////////////////
// Noise evaluation
float WNoise( float p[3])
{ // Non-projected 3D noise
	int i, f[3], c[3], mid[3], n=noiseTileSize; // f, c = filter, noise coeff indices
	float w[3][3], t, result =0;
	// Evaluate quadratic B-spline basis functions
	for (i=0; i<3; i++)
	{
		mid[i]=ceil(p[i]-0.5); t=mid[i]-(p[i]-0.5);
		w[i][0]=t*t/2; w[i][2]=(1-t)*(1-t)/2; w[i][1]=1-w[i][0]-w[i][2];
	}
	// Evaluate noise by weighting noise coefficients by basis function values
	for(f[2]=-1;f[2]<=1;f[2]++) for(f[1]=-1;f[1]<=1;f[1]++) for(f[0]=-1;f[0]<=1;f[0]++)
	{
		float weight=1;
		for (i=0; i<3; i++) {c[i]=Mod(mid[i]+f[i],n); weight*=w[i][f[i]+1];}
		result += weight * noiseTileData[c[2]*n*n+c[1]*n+c[0]];
	}

	return result;
}


// 3D noise projected onto 2D
float WProjectedNoise( float p[3], float normal[3] )
{
	int i, c[3], min[3], max[3], n=noiseTileSize; // c = noise coeff location
	float support, result=0;

	// Bound the support of the basis functions for this projection direction
	for (i=0; i<3; i++)
	{
		support = 3*abs(normal[i]) + 3*sqrt((1-normal[i]*normal[i])/2);
		min[i] = ceil( p[i] - (3*abs(normal[i]) + 3*sqrt((1-normal[i]*normal[i])/2)) );
		max[i] = floor( p[i] + (3*abs(normal[i]) + 3*sqrt((1-normal[i]*normal[i])/2)) );
	}

	// Loop over the noise coefficients within the bound.
	for(c[2]=min[2];c[2]<=max[2];c[2]++)
	{
		for(c[1]=min[1];c[1]<=max[1];c[1]++)
		{
			for(c[0]=min[0];c[0]<=max[0];c[0]++)
			{
				// Dot the normal with the vector from c to p
				float	dot = 0.0f;
				for (i=0; i<3; i++)
					dot+=normal[i]*(p[i]-c[i]);

				// Evaluate the basis function at c moved halfway to p along the normal.
				float weight=1;
				for (i=0; i<3; i++)
				{
					float	t = (c[i]+normal[i]*dot/2)-(p[i]-1.5);
					float	t1=t-1;
					float	t2=2-t;
					float	t3=3-t;
					weight *= (t<=0||t>=3) ? 0 : (t<1) ? t*t/2 : (t<2)? 1-(t1*t1+t2*t2)/2 : t3*t3/2;
				}

				// Evaluate noise by weighting noise coefficients by basis function values.
				result += weight * noiseTileData[ Mod(c[2],n)*n*n + Mod(c[1],n)*n + Mod(c[0],n)];
			}
		}
	}
	return result;
}


float WMultibandNoise( float p[3],float s,float *normal,int firstBand,int nbands,float *w)
{
	float q[3], result=0, variance=0; int i, b;
	for (b=0; b<nbands && s+firstBand+b<0; b++)
	{
		for (i=0; i<=2; i++) {q[i]=2*p[i]*pow(2,firstBand+b);}
		result += (normal) ? w[b] * WProjectedNoise(q,normal) : w[b] * WNoise(q);
	}
	for (b=0; b<nbands; b++) {variance+=w[b]*w[b];}

	// Adjust the noise so it has a variance of 1.
	if (variance) result /= sqrt(variance * ((normal) ? 0.296 : 0.210));

	return result;
}
*/