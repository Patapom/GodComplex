#include "../GodComplex.h"

double	SH::FACTORIAL[] = {	1.0,
							1.0,
							2.0,
							6.0,
							24.0,
							120.0,
							720.0,
							5040.0,
							40320.0,
							362880.0,
							3628800.0,
							39916800.0,
							479001600.0,
							6227020800.0,
							87178291200.0,
							1307674368000.0,
							20922789888000.0,
							355687428096000.0,
							6402373705728000.0,
							1.21645100408832e+17,
							2.43290200817664e+18,
							5.109094217170944e+19,
							1.12400072777760768e+21,
							2.58520167388849766e+22,
							6.20448401733239439e+23,
							1.55112100433309860e+25,
							4.03291461126605636e+26,
							1.08888694504183522e+28,
							3.04888344611713861e+29,
							8.84176199373970195e+30,
							2.65252859812191059e+32,
							8.22283865417792282e+33,
							2.63130836933693530e+35
						};

double		SH::SQRT2 = sqrt( 2.0 );

// Returns a spot sample of a Spherical Harmonic basis function
//		l is the band, range [0..N]
//		m in the range [-l..l]
//		θ in the range [0..Pi]
//		ϕ in the range [0..2*Pi]
//
double	SH::ComputeSHCoeff( int l, int m, double _θ, double _ϕ )
{
	ASSERT( abs( m ) <= l, "m parameter is outside the [-l,+l] range!" );

	if ( m == 0 )
		return	K( l, m ) * P( l, m, cos( _θ ) );
	else if ( m > 0 )
		return SQRT2 * K( l, m ) * cos( m * _ϕ ) * P( l, m, cos( _θ ) );
	else
		return SQRT2 * K( l, -m ) * sin( -m * _ϕ ) * P( l, -m, cos( _θ ) );
}

// Computes a SH windowed with a cardinal sine function
//
double	SH::ComputeSHWindowedSinc( int l, int m, double _θ, double _ϕ, int _Order )
{
	return ComputeSigmaFactorSinc( l, _Order ) * ComputeSHCoeff( l, m, _θ, _ϕ );
}

// Computes a SH windowed with a cosine function
//
double	SH::ComputeSHWindowedCos( int l, int m, double _θ, double _ϕ, int _Order )
{
	return ComputeSigmaFactorCos( l, _Order ) * ComputeSHCoeff( l, m, _θ, _ϕ );
}

// Here, we choose the convention that the vertical axis defining THETA is the Y axis
//  and the axes defining PHI are X and Z where PHI = 0 when the vector is aligned to the positive Z axis
//
// NOTE ==> The '_Direction' vector must be normalized!!
//
double	SH::ComputeSHCoeff( int l, int m, const NjFloat3& _Direction )
{
	// Convert from cartesian to polar coords
	double	θ = 0.0;
	double	ϕ = 0.0f;
	CartesianToSpherical( _Direction, θ, ϕ );

	return	ComputeSHCoeff( l, m, θ, ϕ );
}

void	SH::BuildSHCoeffs( const NjFloat3& _Direction, double _Coeffs[9] )
{
	// Convert from cartesian to polar coords
	double	θ, ϕ;
	CartesianToSpherical( _Direction, θ, ϕ );

	int	CoeffIndex = 0;
	for ( int l=0; l < 3; l++ )
		for ( int m=-l; m <= l; m++ )
			_Coeffs[CoeffIndex++] = ComputeSHCoeff( l, m, θ, ϕ );
}

/// <summary>
/// Computes the product of 2 SH vectors of order 3
/// (code from John Snyder "Code Generation and Factoring for Fast Evaluation of Low-order Spherical Harmonic Products and Squares")
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
/// <returns>c = a * b</returns>
void	SH::Product3( double a[9], double b[9], double c[9] )
{
	double		ta, tb, t;

	const double	C0 = 0.282094792935999980;
	const double	C1 = -0.126156626101000010;
	const double	C2 = 0.218509686119999990;
	const double	C3 = 0.252313259986999990;
	const double	C4 = 0.180223751576000010;
	const double	C5 = 0.156078347226000000;
	const double	C6 = 0.090111875786499998;

	// [0,0]: 0,
	c[0] = C0*a[0]*b[0];

	// [1,1]: 0,6,8,
	ta = C0*a[0]+C1*a[6]-C2*a[8];
	tb = C0*b[0]+C1*b[6]-C2*b[8];
	c[1] = ta*b[1]+tb*a[1];
	t = a[1]*b[1];
	c[0] += C0*t;
	c[6] = C1*t;
	c[8] = -C2*t;

	// [1,2]: 5,
	ta = C2*a[5];
	tb = C2*b[5];
	c[1] += ta*b[2]+tb*a[2];
	c[2] = ta*b[1]+tb*a[1];
	t = a[1]*b[2]+a[2]*b[1];
	c[5] = C2*t;

	// [1,3]: 4,
	ta = C2*a[4];
	tb = C2*b[4];
	c[1] += ta*b[3]+tb*a[3];
	c[3] = ta*b[1]+tb*a[1];
	t = a[1]*b[3]+a[3]*b[1];
	c[4] = C2*t;

	// [2,2]: 0,6,
	ta = C0*a[0]+C3*a[6];
	tb = C0*b[0]+C3*b[6];
	c[2] += ta*b[2]+tb*a[2];
	t = a[2]*b[2];
	c[0] += C0*t;
	c[6] += C3*t;

	// [2,3]: 7,
	ta = C2*a[7];
	tb = C2*b[7];
	c[2] += ta*b[3]+tb*a[3];
	c[3] += ta*b[2]+tb*a[2];
	t = a[2]*b[3]+a[3]*b[2];
	c[7] = C2*t;

	// [3,3]: 0,6,8,
	ta = C0*a[0]+C1*a[6]+C2*a[8];
	tb = C0*b[0]+C1*b[6]+C2*b[8];
	c[3] += ta*b[3]+tb*a[3];
	t = a[3]*b[3];
	c[0] += C0*t;
	c[6] += C1*t;
	c[8] += C2*t;

	// [4,4]: 0,6,
	ta = C0*a[0]-C4*a[6];
	tb = C0*b[0]-C4*b[6];
	c[4] += ta*b[4]+tb*a[4];
	t = a[4]*b[4];
	c[0] += C0*t;
	c[6] -= C4*t;

	// [4,5]: 7,
	ta = C5*a[7];
	tb = C5*b[7];
	c[4] += ta*b[5]+tb*a[5];
	c[5] += ta*b[4]+tb*a[4];
	t = a[4]*b[5]+a[5]*b[4];
	c[7] += C5*t;

	// [5,5]: 0,6,8,
	ta = C0*a[0]+C6*a[6]-C5*a[8];
	tb = C0*b[0]+C6*b[6]-C5*b[8];
	c[5] += ta*b[5]+tb*a[5];
	t = a[5]*b[5];
	c[0] += C0*t;
	c[6] += C6*t;
	c[8] -= C5*t;

	// [6,6]: 0,6,
	ta = C0*a[0];
	tb = C0*b[0];
	c[6] += ta*b[6]+tb*a[6];
	t = a[6]*b[6];
	c[0] += C0*t;
	c[6] += C4*t;

	// [7,7]: 0,6,8,
	ta = C0*a[0]+C6*a[6]+C5*a[8];
	tb = C0*b[0]+C6*b[6]+C5*b[8];
	c[7] += ta*b[7]+tb*a[7];
	t = a[7]*b[7];
	c[0] += C0*t;
	c[6] += C6*t;
	c[8] += C5*t;

	// [8,8]: 0,6,
	ta = C0*a[0]-C4*a[6];
	tb = C0*b[0]-C4*b[6];
	c[8] += ta*b[8]+tb*a[8];
	t = a[8]*b[8];
	c[0] += C0*t;
	c[6] -= C4*t;
	// entry count=13
	// multiply count=120
	// addition count=74
}

/// <summary>
/// Converts a cartesian UNIT vector into spherical coordinates (θ,ϕ)
/// </summary>
/// <param name="_Direction">The cartesian unit vector to convert</param>
/// <param name="_θ">The polar elevation</param>
/// <param name="_ϕ">The azimuth</param>
void		SH::CartesianToSpherical( const NjFloat3& _Direction, double& _θ, double& _ϕ )
{
	_θ = acos( MAX( -1.0f, MIN( +1.0f, _Direction.z ) ) );
	_ϕ = atan2( _Direction.y, _Direction.x );
}

/// <summary>
/// Converts spherical coordinates (θ,ϕ) into a cartesian UNIT vector
/// </summary>
/// <param name="_θ">The polar elevation</param>
/// <param name="_ϕ">The azimuth</param>
/// <returns>The unit vector in cartesian coordinates</returns>
NjFloat3	SH::SphericalToCartesian( double _θ, double _ϕ )
{
	NjFloat3	Result;
	SphericalToCartesian( _θ, _ϕ, Result );
	return	Result;
}

/// <summary>
/// Converts spherical coordinates (θ,ϕ) into a cartesian UNIT vector
/// </summary>
/// <param name="_θ">The polar elevation</param>
/// <param name="_ϕ">The azimuth</param>
/// <param name="_Direction">The unit vector in cartesian coordinates</param>
void		SH::SphericalToCartesian( double _θ, double _ϕ, NjFloat3& _Direction )
{
	_Direction.x = (float) (sin( _θ ) * cos( _ϕ ));
	_Direction.z = (float) cos( _θ );
	_Direction.y = (float) (sin( _θ ) * sin( _ϕ ));
}

// Converts a GodComplex Y-up vector into a vector usable by the SH library
NjFloat3	SH::Yup2Zup( const NjFloat3& _Yup )
{
	NjFloat3	Result;
	Result.x = _Yup.z;
	Result.y = _Yup.x;
	Result.z = _Yup.y;
	return Result;
}


//////////////////////////////////////////////////////////////////////////
// SH Coefficients & Legendre Polynomials & Windowing Sigma Factors
//

// Renormalisation constant for SH functions
//           .------------------------
// K(l,m) =  |   (2*l+1)*(l-|m|)!
//           | --------------------
//          \| 4*Math.PI * (l+|m|)!
//
double	SH::K( int l, int m )
{
	return	sqrt( ((2.0 * l + 1.0 ) * Factorial( l - abs(m) )) / (4.0 * PI * Factorial( l + abs(m) )) );
}

// Calculates an Associated Legendre Polynomial P(l,m,x) using stable recurrence relations
// From Numerical Recipes in C
//
double	SH::P( int l, int m, double x )
{
	double	pmm = 1.0;
	if ( m > 0 )
	{	// pmm = (-1) ^ m * Factorial( 2 * m - 1 ) * ( (1 - x) * (1 + x) ) ^ (m/2);
		double	somx2 = sqrt( (1.0-x) * (1.0+x) );
		double	fact = 1.0;
		for ( int i=1; i <= m; i++ )
		{
			pmm *= -fact * somx2;
			fact += 2;
		}
	}
	if ( l == m )
		return	pmm;

	double	pmmp1 = x * (2.0 * m + 1.0) * pmm;
	if ( l == m+1 )
		return	pmmp1;

	double	pll = 0.0;
	for ( int ll=m+2; ll <= l; ++ll )
	{
		pll = ( (2.0*ll-1.0) * x * pmmp1 - (ll+m-1.0) * pmm ) / (ll-m);
		pmm = pmmp1;
		pmmp1 = pll;
	}

	return	pll;
}

/// <summary>
/// Sigma factor using a cardinal sine for windowing
/// From "A Quick Rendering Method Using Basis Functions for Interactive Lighting Design" by Dobashi et al. (1995)
/// 
/// Therefore, you should no longer use ComputeSH( l, m ) when reconstructing SH but ComputeSigmaFactorSinc( l ) * ComputeSH( l, m )
/// </summary>
/// <param name="l">Current band</param>
/// <param name="_Order">Max used SH order</param>
/// <returns>The sigma factor to apply to the SH coefficient to avoid ringing (aka Gibbs phenomenon)</returns>
double	SH::ComputeSigmaFactorSinc( int l, int _Order )
{
	double	Angle = PI * l / (_Order+1);
	return l > 0 ? sin( Angle ) / Angle : 1.0;
}

/// <summary>
/// Sigma factor using a cardinal sine for windowing
/// From "Real-time Soft Shadows in Dynamic Scenes using Spherical Harmonic Exponentiation" by Ren et al. (2006)
/// 
/// Therefore, you should no longer use ComputeSH( l, m ) when reconstructing SH but ComputeSigmaFactorCos( l ) * ComputeSH( l, m )
/// </summary>
/// <param name="l">Current band</param>
/// <param name="h">Window size. By default a value of 2*Order works well but some HDR lighting may require greater windowing (i.e. smaller h)</param>
/// <param name="_Order">Max used SH order</param>
/// <returns>The sigma factor to apply to the SH coefficient to avoid ringing (aka Gibbs phenomenon)</returns>
double	SH::ComputeSigmaFactorCos( int l, int _Order )	{ return ComputeSigmaFactorCos( l, 2.0 * _Order ); }
double	SH::ComputeSigmaFactorCos( int l, double h )
{
	return cos( 0.5 * PI * l / h );
}

/// <summary>
/// Computes the factorial of the given integer value
/// </summary>
/// <param name="_Value">The value to compute the factorial of</param>
/// <returns>The factorial of the input value</returns>
double		SH::Factorial( int _Value )
{
	return	FACTORIAL[_Value];
}
