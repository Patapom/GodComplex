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
	static double		ComputeSHCoeff( int l, int m, const float3& _Direction );

	// SH Coeffs with windowing
	static double		ComputeSHWindowedSinc( int l, int m, double _θ, double _ϕ, int _Order );
	static double		ComputeSHWindowedCos( int l, int m, double _θ, double _ϕ, int _Order );

	static void			BuildSHCoeffs( const float3& _Direction, double _Coeffs[9] );

	// Advanced
	static void			Product3( const double a[9], const double b[9], double r[9] );
	static void			Product3( const float a[9], const float b[9], float r[9] );
	static void			Product3( const float3 a[9], const float b[9], float3 r[9] );
	static void			Product3( const float3 a[9], const float3 b[9], float3 r[9] );

	// Helpers
	static void			CartesianToSpherical( const float3& _Direction, double& _θ, double& _ϕ );
	static float3		SphericalToCartesian( double _θ, double _ϕ );
	static void			SphericalToCartesian( double _θ, double _ϕ, float3& _Direction );
	static float3		Yup2Zup( const float3& _Yup );

	// Filters
	static void			FilterHanning( float3 _SH[9], float w );	// Applies a Hanning window of width w (usually the SH order).
	static void			FilterLanczos( float3 _SH[9], float w );	// Applies a Lanczos window of width w (usually the SH order).
	static void			FilterGaussian( float3 _SH[9], float w );	// Applies a Gaussian window of width w (usually the SH order).


	// Y-up helpers
	// These functions are the same as the SH.hlsl shader helpers and are all ordered using our familiar Y-up reference frame:
	// 
	//	   Y
	//	   |
	//	   |
	//	   |
	//	   o------X
	//	  /
	//	 /
	//	Z
	//
	static void			BuildSHCoeffs_YUp( const float3& _Direction, double _Coeffs[9] );
	static void			BuildSHCosineLobe_YUp( const float3& _Direction, double _Coeffs[9] );
	static void			BuildSHCone_YUp( const float3& _Direction, float _HalfAngle, double _Coeffs[9] );
	static void			BuildSHSmoothCone_YUp( const float3& _Direction, float _HalfAngle, double _Coeffs[9] );
	static void			ZHRotate_YUp( const float3& _Direction, const float3& _ZHCoeffs, double _Coeffs[9] );


private:
	static double		Factorial( int _Value );
	static double		K( int l, int m );
	static double		P( int l, int m, double x );
	static double		ComputeSigmaFactorSinc( int l, int _Order );
	static double		ComputeSigmaFactorCos( int l, int _Order );
	static double		ComputeSigmaFactorCos( int l, double h );
	static void			Filter( float3 _SH[9], int l, float a );	// Modulate all coefficients of degree l by scalar a.
};
