#include "../GodComplex.h"

const float	Noise::BIAS_U = 0.1316519815f;
const float	Noise::BIAS_V = 0.1984632145f;
const float	Noise::BIAS_W = 0.1621987463f;
const float	Noise::BIAS_R = 0.7685431298f;
const float	Noise::BIAS_S = 0.4646579661f;
const float	Noise::BIAS_T = 0.9887465321f;

Noise::Noise()
{
}

void	Noise::Init( int _Seed )
{
	_randpushseed();
	_srand( _Seed, RAND_DEFAULT_SEED_V );

	// Fill the table of random numbers & permutations
	for ( int i=0; i < NOISE_SIZE; i++ )
	{
		for ( int j=0; j < 8; j++ )
			m_pNoise[(i<<3)+j] = _frand();	// Generate 8 noise values (although we'll use only 6 of them for the 6D noise)
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
}

void	Noise::Exit()
{
}

// This should generate a code like this:
//
// 	float	fX0 = (BIAS_U+u) * NOISE_SIZE;
// 	int		X0_ = ASM_floorf( fX0 );
// 	float	t0 = fX0 - X0_;	float	r0 = 1.0f - t0;
// 			X0_ = X0_ & NOISE_MASK;
// 	int		X0 = (X0_ + 1) & NOISE_MASK;
//
#define	NOISE_INDICES( Bias, Var, Index )	\
	float	fX##Index = (Bias+Var) * NOISE_SIZE;	\
 	int		X##Index##_ = ASM_floorf( fX##Index );	\
 	float	t##Index = fX##Index - X##Index##_;	float	r##Index = 1.0f - t##Index;	\
 			X##Index##_ = X##Index##_ & NOISE_MASK;	\
 	int		X##Index = (X##Index##_ + 1) & NOISE_MASK;

float	Noise::Noise1D( float u )
{
	NOISE_INDICES( BIAS_U, u, 0 )

	float	N0 = Dot( m_pPermutation[X0_], t0 );
	float	N1 = Dot( m_pPermutation[X0], r0 );

	t0 = SCurve( t0 );

	return Lerp( N0, N1, t0 );
}

float	Noise::Noise2D( const NjFloat2& uv )
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

float	Noise::Noise3D( const NjFloat3& uvw )
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

float	Noise::Noise4D( const NjFloat4& uvwr )
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

float	Noise::Noise5D( const NjFloat4& uvwr, float s )
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

float	Noise::Noise6D( const NjFloat4& uvwr, const NjFloat2& st )
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

void	Noise::SetWrappingParameters( float _Frequency, U32 _Seed )
{
	m_WrapRadius = _Frequency * 0.5f * NOISE_SIZE;

	_randpushseed();
	_srand( _Seed, RAND_DEFAULT_SEED_V );

	m_WrapCenter0 = NjFloat2( _frand() * NOISE_SIZE, _frand() * NOISE_SIZE );
	m_WrapCenter1 = NjFloat2( _frand() * NOISE_SIZE, _frand() * NOISE_SIZE );
	m_WrapCenter2 = NjFloat2( _frand() * NOISE_SIZE, _frand() * NOISE_SIZE );

	_randpopseed();
}

float	Noise::WrapNoise1D( float u )
{
	float		Angle = TWOPI * u;
	NjFloat2	Pos( m_WrapCenter0.x + cosf( Angle ), m_WrapCenter0.y + sinf( Angle ) );
	return Noise2D( Pos );
}

float	Noise::WrapNoise2D( const NjFloat2& uv )
{
	float		Angle0 = TWOPI * uv.x;
	float		Angle1 = TWOPI * uv.y;
	NjFloat4	Pos( m_WrapCenter0.x + cosf( Angle0 ), m_WrapCenter0.y + sinf( Angle0 ), m_WrapCenter1.x + cosf( Angle1 ), m_WrapCenter1.y + sinf( Angle1 ) );
	return Noise4D( Pos );
}

float	Noise::WrapNoise3D( const NjFloat3& uvw )
{
	float		Angle0 = TWOPI * uvw.x;
	float		Angle1 = TWOPI * uvw.y;
	float		Angle2 = TWOPI * uvw.z;
	NjFloat4	Pos0( m_WrapCenter0.x + cosf( Angle0 ), m_WrapCenter0.y + sinf( Angle0 ), m_WrapCenter1.x + cosf( Angle1 ), m_WrapCenter1.y + sinf( Angle1 ) );
	NjFloat2	Pos1( m_WrapCenter2.x + cosf( Angle1 ), m_WrapCenter2.y + sinf( Angle1 ) );
	return Noise6D( Pos0, Pos1 );
}
