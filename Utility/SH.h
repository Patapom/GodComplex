//////////////////////////////////////////////////////////////////////////
// The basic functions gathered here allow to work with Spherical Harmonics and Zonal Harmonics
// 
// ===============================================================================
// The convention for transformation between spherical and cartesian coordinates is:
// 
//		( sinθ cosϕ, sinθ sinϕ, cosθ ) → (x, y, z)
// 
//	 _ Azimuth ϕ is zero on +X and increases CCW (i.e. PI/2 at +Y, PI at -X and 3PI/2 at -Y)
//	 _ Elevation θ is zero on +Z and PI on -Z
// 
// 
//                   Z θ=0
//                   |
//                   |
//                   |
// ϕ=3PI/2 -Y - - - -o------+Y ϕ=PI/2
//                  /.
//                 / .
//               +X  .
//             ϕ=0   .
//                  -Z θ=PI
// 
// So cartesian to polar coordinates is computed this way:
//		θ = acos( Z );
//		ϕ = atan2( Y, X );
//
#pragma once

class	SH
{
private:
	static double		FACTORIAL[];
	static double		SQRT2;

public:
	static double		ComputeSHCoeff( int l, int m, double _θ, double _ϕ );
	static double		ComputeSHCoeff( int l, int m, const NjFloat3& _Direction );

	// SH Coeffs with windowing
	static double		ComputeSHWindowedSinc( int l, int m, double _θ, double _ϕ, int _Order );
	static double		ComputeSHWindowedCos( int l, int m, double _θ, double _ϕ, int _Order );

	static void			BuildSHCoeffs( const NjFloat3& _Direction, double _Coeffs[9] );
	static void			BuildSHCosineLobe( const NjFloat3& _Direction, double _Coeffs[9] );

	// Advanced
	static void			ZHRotate( const NjFloat3& _Direction, const NjFloat3& _ZHCoeffs, double _Coeffs[9] );
	static void			Product3( double a[9], double b[9], double r[9] );

	// Helpers
	static void			CartesianToSpherical( const NjFloat3& _Direction, double& _θ, double& _ϕ );
	static NjFloat3		SphericalToCartesian( double _θ, double _ϕ );
	static void			SphericalToCartesian( double _θ, double _ϕ, NjFloat3& _Direction );
	static NjFloat3		Yup2Zup( const NjFloat3& _Yup );

private:
	static double		Factorial( int _Value );
	static double		K( int l, int m );
	static double		P( int l, int m, double x );
	static double		ComputeSigmaFactorSinc( int l, int _Order );
	static double		ComputeSigmaFactorCos( int l, int _Order );
	static double		ComputeSigmaFactorCos( int l, double h );
};
