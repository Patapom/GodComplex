////////////////////////////////////////////////////////////////////////////////
// Spherical Harmonics Helpers
////////////////////////////////////////////////////////////////////////////////
//

////////////////////////////////////////////////////////////////////////////////
// Environments encoded as SH

// Grace probe
//static const float3	EnvironmentSH[9] = {
//	float3( 0.933358105849532, 0.605499186927096, 0.450999072970855 ), 
//	float3( 0.0542981143130068, 0.0409598475963159, 0.0355377036564806 ), 
//	float3( 0.914255336642483, 0.651103534810611, 0.518065694132826 ), 
//	float3( 0.238207071886099, 0.14912965904707, 0.0912559191766972 ), 
//	float3( 0.0321476755042544, 0.0258939812282057, 0.0324159089991572 ), 
//	float3( 0.104707893908821, 0.0756648975030993, 0.0749934936107284 ), 
//	float3( 1.27654512826622, 0.85613828921136, 0.618241442250845 ), 
//	float3( 0.473237767573493, 0.304160108872238, 0.193304867770535 ), 
//	float3( 0.143726445535245, 0.0847402441253633, 0.0587779174281925 ), 
//};

// Ennis house
static const float3	EnvironmentSH[9] = {
					float3( 4.52989505453915, 4.30646452463535, 4.51721251492342 ), 
					float3( 0.387870406203612, 0.384965748870704, 0.395325521894004 ), 
					float3( 1.05692530696077, 1.33538156449369, 1.82393006020369 ), 
					float3( 6.18680912868925, 6.19927929741711, 6.6904772608617 ), 
					float3( 0.756169905467733, 0.681053631625203, 0.677636982521888 ), 
					float3( 0.170950637080382, 0.1709443393056, 0.200437519088333 ), 
					float3( -3.59338856195816, -3.37861193089806, -3.30850268192343 ), 
					float3( 2.65318898618603, 2.97074561577712, 3.82264536047523 ), 
					float3( 6.07079134655854, 6.05819330192308, 6.50325529149908 ), 
};

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Generic SH coefficients (works up to order 17, after that I believe it's lacking precision)
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

static const float	FACT[41] = {	1.0,							//  0!	Order 0
									1.0,							//  1!
									2.0,							//  2!	Order 1
									6.0,							//  3!
									24.0,							//  4!	Order 2
									120.0,							//  5!
									720.0,							//  6!	Order 3
									5040.0,							//  7!
									40320.0,						//  8!	Order 4
									362880.0,						//  9!
									3628800.0,						// 10!	Order 5
									39916800.0,						// 11!
									479001600.0,					// 12!	Order 6
									6227020800.0,					// 13!
									87178291200.0,					// 14!	Order 7
									1307674368000.0,				// 15!
									20922789888000.0,				// 16!	Order 8
									355687428096000.0,				// 17!
									6402373705728000.0,				// 18!	Order 9
									1216451004088320000.0,									// 19!
									24329020081766400000.0,									// 20!	Order 10
									510909421717094400000.0,								// 21!
									11240007277776076800000.0,								// 22!	Order 11
									258520167388849766400000.0,								// 23!
									6204484017332394393600000.0,							// 24!	Order 12
									155112100433309859840000000.0,							// 25!
									4032914611266056355840000000.0,							// 26!	Order 13
									108888694504183521607680000000.0,						// 27!
									3048883446117138605015040000000.0,						// 28!	Order 14
									88417619937397019545436160000000.0,						// 29!
									2652528598121910586363084800000000.0,					// 30!	Order 15
									82228386541779228177255628800000000.0,					// 31!
									263130836933693530167218012160000000.0,					// 32!	Order 16
									8683317618811886495518194401280000000.0,				// 33!
									295232799039604140847618609643520000000.0,				// 34!	Order 17
									10333147966386144929666651337523200000000.0,			// 35!
									371993326789901217467999448150835200000000.0,			// 36!	Order 18
									13763753091226345046315979581580902400000000.0,			// 37!
									523022617466601111760007224100074291200000000.0,		// 38!	Order 19
									20397882081197443358640281739902897356800000000.0,		// 39!
									815915283247897734345611269596115894272000000000.0,		// 40!	Order 20
};

// Factors to multiply ZH coefficients by, as described by eq. 26 in "On the relationship between radiance and irradiance" by Ramamoorthi
static const float3	ZH_FACTORS = float3( 3.5449077018110320545963349666823, 2.0466534158929769769591032497785, 1.5853309190424044053380115060481 );	// sqrt( 4 * PI / (2*l+1) ) for the first 3 bands

// Renormalisation constant for SH functions
//           .------------------------
// K(l,m) =  |   (2*l+1)*(l-|m|)!
//           | --------------------
//          \|    4*PI * (l+|m|)!
//
float	General_K( int l, int m ) {
	return	sqrt( ((2.0 * l + 1.0 ) * FACT[l - abs(m)]) / (4.0 * PI * FACT[l + abs(m)]) );
}

// Calculates an Associated Legendre Polynomial P(l,m,x) using stable recurrence relations
// From Numerical Recipes in C
// x = cos(theta)
float	General_P( int l, int m, float x ) {
	x = clamp( x, -1.0, 1.0 );

	float	pmm = 1.0;
	if ( m > 0 ) {
		// pmm = (-1) ^ m * Factorial( 2 * m - 1 ) * ( (1 - x) * (1 + x) ) ^ (m/2);
		float	somx2 = sqrt( (1.0-x) * (1.0+x) );
//		float	somx2 = sqrt( max( 0.0, (1.0-x) * (1.0+x) ) );
		float	fact = 1.0;
		[loop]
		for ( int i=1; i <= m; i++ ) {
			pmm *= -fact * somx2;
			fact += 2;
		}
	}
	if ( l == m )
		return	pmm;

	float	pmmp1 = x * (2.0 * m + 1.0) * pmm;
	if ( l == m+1 )
		return	pmmp1;

	float	pll = 0.0;
	[loop]
	for ( int ll=m+2; ll <= l; ++ll ) {
		pll = ( (2.0*ll-1.0) * x * pmmp1 - (ll+m-1.0) * pmm ) / (ll-m);
		pmm = pmmp1;
		pmmp1 = pll;
	}

	return	pll;
}

float	General_Ylm( int l, int m, float _cosTheta, float _phi ) {
	float	factor = General_K( l, m );
	float	P = General_P( l, abs(m), _cosTheta );
	if ( m == 0 )
		return	factor * P;

	factor *= (m & 1 ? -1 : 1) * SQRT2;	// (2015-04-13) According to wikipedia (http://en.wikipedia.org/wiki/Spherical_harmonics#Real_form) we should multiply coeffs by (-1)^m ...
//	return  factor * P * (m > 0 ? cos( m * _phi ) : sin( -m * _phi ));
	return  factor * P * sin( (m >= 0 ? 0.5*PI : 0.0) + abs(m) * _phi );
}

////////////////////////////////////////////////////////////////////////////////
// Specialized SH coefficients
// Computes the Ylm coefficients in the requested direction
//
void	Ylm( float3 _direction, out float _SH[9] ) {
	const float	c0 = 0.28209479177387814347403972578039;	// 1/2 sqrt(1/pi)
	const float	c1 = 0.48860251190291992158638462283835;	// 1/2 sqrt(3/pi)
	const float	c2 = 1.09254843059207907054338570580270;	// 1/2 sqrt(15/pi)
	const float	c3 = 0.31539156525252000603089369029571;	// 1/4 sqrt(5/pi)

	float	x = _direction.x;
	float	y = _direction.y;
	float	z = _direction.z;

	_SH[0] = c0;
	_SH[1] = c1*y;
	_SH[2] = c1*z;
	_SH[3] = c1*x;
	_SH[4] = c2*x*y;
	_SH[5] = c2*y*z;
	_SH[6] = c3*(3.0*z*z - 1.0);
	_SH[7] = c2*x*z;
	_SH[8] = 0.5*c2*(x*x - y*y);
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Generic radiance estimate up to order 19
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//

float3	EvaluateSHRadiance( float3 _direction, uint _maxOrder, Texture2D< float3 > _texSH, float _filterWindowSize=1000.0 ) {

//	_direction = float3( _direction.z, _direction.x, _direction.y );	// Transform direction from Y-up to Z-up

	_maxOrder++;
	uint	coeffsCount = _maxOrder * _maxOrder;
	float	rcpWindow = 1.0 / _filterWindowSize;

	float	cosTheta = _direction.z;
//	float	phi = atan2( _direction.y, _direction.x );	// Produces NaNs if atan2( 0, 0 )!!
	float	phi = abs(_direction.x) > 1e-6 ? atan2( _direction.y, _direction.x ) : 0.0;

	// Use generic Ylm for all orders
	float3	radiance = 0.0;
	[loop]
	for ( uint i=0; i < coeffsCount; i++ ) {
		int	l = int( floor( sqrt( i ) ) );
		int	m = i - l*(l+1);

		float	filter = 0.5 * (1.0 + cos( l * PI * rcpWindow ));

		float	Ylm = filter * General_Ylm( l, m, cosTheta, phi );
		radiance += Ylm * _texSH[uint2(i,0)];
	}

	return radiance;
}

/*float3	General_EvaluateSHRadiance( float3 _direction, int _maxOrder, float _filterWindowSize, float _A[20] ) {
	float	cosTheta = _direction.z;
//	float	phi = atan2( _direction.y, _direction.x );	// Produces NaNs if atan2( 0, 0 )!!
	float	phi = abs(_direction.x) > 1e-6 ? atan2( _direction.y, _direction.x ) : 0.0;
	float	rcpWindow = 1.0 / _filterWindowSize;

	// Use hardcoded Ylm for first 3 orders
	float	firstSH[9];
	Ylm( _direction, firstSH );

	float2	filters = float2( _A[1] * 0.5 * (1.0 + cos( PI * rcpWindow )), _A[2] * 0.5 * (1.0 + cos( 2.0 * PI * rcpWindow )) );
	firstSH[0] *= _A[0];
	firstSH[1] *= filters.x;
	firstSH[2] *= filters.x;
	firstSH[3] *= filters.x;
	firstSH[4] *= filters.y;
	firstSH[5] *= filters.y;
	firstSH[6] *= filters.y;
	firstSH[7] *= filters.y;
	firstSH[8] *= filters.y;

	float3	radiance = 0.0;

#if 0
	uint	imax = min( 9, _maxOrder*_maxOrder );
	for ( uint i=0; i < imax; i++ ) {
		float	Ylm = firstSH[i];
		radiance += Ylm * _environmentSH[i].xyz;
	}

	// Use generic Ylm for remaining orders
	[loop]
	for ( int l=3; l < _maxOrder; l++ ) {
		float	filter = 0.5 * (1.0 + cos( l * PI * rcpWindow ));
				filter *= _A[l];	// Incorporate Al term
		[loop]
		for ( int m=-l; m <= l; m++ ) {
			uint	i = l*(l+1)+m;
			float	Ylm = filter * General_Ylm( l, m, cosTheta, phi );
			radiance += Ylm * _environmentSH[i].xyz;
		}
	}
#else
	// Use generic Ylm for all orders
	int	coeffsCount = _maxOrder * _maxOrder;
	[loop]
	for ( int i=0; i < coeffsCount; i++ ) {
		int	l = int( floor( sqrt( i ) ) );
		int	m = i - l*(l+1);

		float	filter = 0.5 * (1.0 + cos( l * PI * rcpWindow ));
				filter *= _A[l];	// Incorporate Al term

		float	Ylm = filter * General_Ylm( l, m, cosTheta, phi );
		radiance += Ylm * _environmentSH[i].xyz;
	}
#endif

	return radiance;
}
*/
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Radiance/Irradiance Evaluators
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// Evaluates the SH coefficients in the requested direction
// Analytic method from https://cseweb.ucsd.edu/~ravir/papers/envmap/envmap.pdf eq. 3
//
float3	EvaluateSHRadiance( float3 _direction, float3 _SH[9] ) {
	const float	f0 = 0.28209479177387814347403972578039;		// 0.5 / sqrt(PI);
	const float	f1 = 0.48860251190291992158638462283835;		// 0.5 * sqrt(3/PI);
	const float	f2 = 1.0925484305920790705433857058027;			// 0.5 * sqrt(15/PI);
	const float	f3 = 0.31539156525252000603089369029571;		// 0.25 * sqrt(5.PI);

	float	EvalSH0 = f0;
	float4	EvalSH1234, EvalSH5678;
	EvalSH1234.x = f1 * _direction.y;
	EvalSH1234.y = f1 * _direction.z;
	EvalSH1234.z = f1 * _direction.x;
	EvalSH1234.w = f2 * _direction.x * _direction.y;
	EvalSH5678.x = f2 * _direction.y * _direction.z;
	EvalSH5678.y = f3 * (3.0 * _direction.z*_direction.z - 1.0);
	EvalSH5678.z = f2 * _direction.x * _direction.z;
	EvalSH5678.w = f2 * 0.5 * (_direction.x*_direction.x - _direction.y*_direction.y);

	// Dot the SH together
	return max( 0.0,
			EvalSH0		* _SH[0]
			+ EvalSH1234.x * _SH[1]
			+ EvalSH1234.y * _SH[2]
			+ EvalSH1234.z * _SH[3]
			+ EvalSH1234.w * _SH[4]
			+ EvalSH5678.x * _SH[5]
			+ EvalSH5678.y * _SH[6]
			+ EvalSH5678.z * _SH[7]
			+ EvalSH5678.w * _SH[8] );
}

// Evaluates the irradiance perceived in the provided direction
// Analytic method from https://cseweb.ucsd.edu/~ravir/papers/envmap/envmap.pdf eq. 13
//
float3	EvaluateSHIrradiance( float3 _direction, float3 _SH[9] ) {
	const float	c1 = 0.42904276540489171563379376569857;	// 4 * Â2.Y22 = 1/4 * sqrt(15.PI)
	const float	c2 = 0.51166335397324424423977581244463;	// 0.5 * Â1.Y10 = 1/2 * sqrt(PI/3)
	const float	c3 = 0.24770795610037568833406429782001;	// Â2.Y20 = 1/16 * sqrt(5.PI)
	const float	c4 = 0.88622692545275801364908374167057;	// Â0.Y00 = 1/2 * sqrt(PI)

	float	x = _direction.x;
	float	y = _direction.y;
	float	z = _direction.z;

	return	max( 0.0,
			(c1*(x*x - y*y)) * _SH[8]						// c1.L22.(x²-y²)
			+ (c3*(3.0*z*z - 1)) * _SH[6]					// c3.L20.(3.z² - 1)
			+ c4 * _SH[0]									// c4.L00 
			+ 2.0*c1*(_SH[4]*x*y + _SH[7]*x*z + _SH[5]*y*z)	// 2.c1.(L2-2.xy + L21.xz + L2-1.yz)
			+ 2.0*c2*(_SH[3]*x + _SH[1]*y + _SH[2]*z) );	// 2.c2.(L11.x + L1-1.y + L10.z)
}

// Evaluates the irradiance perceived in the provided direction, also accounting for Ambient Occlusion
// Details can be found at http://wiki.nuaj.net/index.php?title=SphericalHarmonicsPortal
// Here, _cosThetaAO = cos( PI/2 * AO ) and represents the cosine of the cone half-angle that drives the amount of light a surface is perceiving
//
float3	EvaluateSHIrradiance( float3 _direction, float _cosThetaAO, float3 _SH[9] ) {
	float		t2 = _cosThetaAO*_cosThetaAO;
	float		t3 = t2*_cosThetaAO;
	float		t4 = t3*_cosThetaAO;
	float		ct2 = 1.0 - t2; 

	float		c0 = 0.88622692545275801364908374167057 * ct2;							// 1/2 * sqrt(PI) * (1-t^2)
	float		c1 = 1.02332670794648848847955162488930 * (1.0-t3);						// sqrt(PI/3) * (1-t^3)
	float		c2 = 0.24770795610037568833406429782001 * (3.0 * (1.0-t4) - 2.0 * ct2);	// 1/16 * sqrt(5*PI) * [3(1-t^4) - 2(1-t^2)]
	const float	sqrt3 = 1.7320508075688772935274463415059;

	float		x = _direction.x;
	float		y = _direction.y;
	float		z = _direction.z;

	return	max( 0.0, c0 * _SH[0]													// c0.L00
			+ c1 * (_SH[1]*y + _SH[2]*z + _SH[3]*x)									// c1.(L1-1.y + L10.z + L11.x)
			+ c2 * (_SH[6]*(3.0*z*z - 1.0)											// c2.L20.(3z²-1)
					+ sqrt3 * (_SH[8]*(x*x - y*y)									// sqrt(3).c2.L22.(x²-y²)
								+ 2.0 * (_SH[4]*x*y + _SH[5]*y*z + _SH[7]*z*x)		// 2sqrt(3).c2.(L2-2.xy + L2-1.yz + L21.zx)
							  )
				   )
		);
}

// Rotate ZH cosine lobe into specific direction
void	RotateZH( float3 _A, float3 _wsDirection, out float _SH[9] ) {
	_A *= ZH_FACTORS;	// Multiply by sqrt( 4 PI / (2l+1) ) as by eq. 26 in "On the relationship between radiance and irradiance" by Ramamoorthi

	Ylm( _wsDirection, _SH );
	_SH[0] *= _A.x;
	_SH[1] *= _A.y;
	_SH[2] *= _A.y;
	_SH[3] *= _A.y;
	_SH[4] *= _A.z;
	_SH[5] *= _A.z;
	_SH[6] *= _A.z;
	_SH[7] *= _A.z;
	_SH[8] *= _A.z;
}

// Generates SH for a regular cosine lobe
void	CosineLobe( float3 _direction, out float _SH[9] ) {
	float3	A = float3(	PI,
						2.0 * PI / 3.0,
						PI / 4.0 );
	RotateZH( A, _direction, _SH );
}

// Generates SH for a "clamped cosine lobe" aligned in the specific direction
void	ClampedCosineLobe( float3 _direction, float _cosConeAngle, out float _SH[9] ) {
	float	t = _cosConeAngle;
	float	t2 = t * t;
	float	t3 = t * t2;
	float	ct2 = 1.0 - t2;
	float3	A = float3(	PI * ct2,
						2.0 * PI / 3.0 * (1.0 - t3),
						PI / 4.0 * (3.0 * (1.0 - t*t3) - 2.0 * ct2) );
	RotateZH( A, _direction, _SH );
}

// Generates SH for a "clamped cone" aligned in the specific direction
void	ClampedCone( float3 _direction, float _cosConeAngle, out float _SH[9] ) {
	float	t = _cosConeAngle;
	float	A1 = PI * (1.0 - t*t);
	float3	A = float3(	2.0 * PI * (1.0 - t),
						A1,
						t * A1 );
	RotateZH( A, _direction, _SH );
}


////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Filtering
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// Applies Hanning filter for given window size
void FilterHanning( float3 _inSH[9], out float3 _outSH[9], float _WindowSize ) {

	float	rcpWindow = 1.0 / _WindowSize;
	float2	Factors = float2( 0.5 * (1.0 + cos( PI * rcpWindow )), 0.5 * (1.0 + cos( 2.0 * PI * rcpWindow )) );
	_outSH[0] = _inSH[0];
	_outSH[1] = Factors.x * _inSH[1];
	_outSH[2] = Factors.x * _inSH[2];
	_outSH[3] = Factors.x * _inSH[3];
	_outSH[4] = Factors.y * _inSH[4];
	_outSH[5] = Factors.y * _inSH[5];
	_outSH[6] = Factors.y * _inSH[6];
	_outSH[7] = Factors.y * _inSH[7];
	_outSH[8] = Factors.y * _inSH[8];
}

// Applies Lanczos filter for given window size
void FilterLanczos( float3 _inSH[9], out float3 _outSH[9], float _WindowSize ) {

	float	rcpWindow = 1.0 / _WindowSize;
	float2	Factors = float2( sin( PI * rcpWindow ) / (PI * rcpWindow), sin( 2.0 * PI * rcpWindow ) / (2.0 * PI * rcpWindow) );
	_outSH[0] = _inSH[0];
	_outSH[1] = Factors.x * _inSH[1];
	_outSH[2] = Factors.x * _inSH[2];
	_outSH[3] = Factors.x * _inSH[3];
	_outSH[4] = Factors.y * _inSH[4];
	_outSH[5] = Factors.y * _inSH[5];
	_outSH[6] = Factors.y * _inSH[6];
	_outSH[7] = Factors.y * _inSH[7];
	_outSH[8] = Factors.y * _inSH[8];
}

// Applies gaussian filter for given window size
void FilterGaussian( float3 _inSH[9], out float3 _outSH[9], float _WindowSize ) {

	float	rcpWindow = 1.0 / _WindowSize;
	float2	Factors = float2( exp( -0.5 * (PI * rcpWindow) * (PI * rcpWindow) ), exp( -0.5 * (2.0 * PI * rcpWindow) * (2.0 * PI * rcpWindow) ) );
	_outSH[0] = _inSH[0];
	_outSH[1] = Factors.x * _inSH[1];
	_outSH[2] = Factors.x * _inSH[2];
	_outSH[3] = Factors.x * _inSH[3];
	_outSH[4] = Factors.y * _inSH[4];
	_outSH[5] = Factors.y * _inSH[5];
	_outSH[6] = Factors.y * _inSH[6];
	_outSH[7] = Factors.y * _inSH[7];
	_outSH[8] = Factors.y * _inSH[8];
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// Performs the SH triple product r = a * b
// From John Snyder (appendix A8)
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
void SHProduct( const in float a[9], const in float b[9], out float r[9] ) {
	float	ta, tb, t;

	const float	C0 = 0.282094792935999980;
	const float	C1 = -0.126156626101000010;
	const float	C2 = 0.218509686119999990;
	const float	C3 = 0.252313259986999990;
	const float	C4 = 0.180223751576000010;
	const float	C5 = 0.156078347226000000;
	const float	C6 = 0.090111875786499998;

	// [0,0]: 0,
	r[0] = C0*a[0]*b[0];

	// [1,1]: 0,6,8,
	ta = C0*a[0]+C1*a[6]-C2*a[8];
	tb = C0*b[0]+C1*b[6]-C2*b[8];
	r[1] = ta*b[1]+tb*a[1];
	t = a[1]*b[1];
	r[0] += C0*t;
	r[6] = C1*t;
	r[8] = -C2*t;

	// [1,2]: 5,
	ta = C2*a[5];
	tb = C2*b[5];
	r[1] += ta*b[2]+tb*a[2];
	r[2] = ta*b[1]+tb*a[1];
	t = a[1]*b[2]+a[2]*b[1];
	r[5] = C2*t;

	// [1,3]: 4,
	ta = C2*a[4];
	tb = C2*b[4];
	r[1] += ta*b[3]+tb*a[3];
	r[3] = ta*b[1]+tb*a[1];
	t = a[1]*b[3]+a[3]*b[1];
	r[4] = C2*t;

	// [2,2]: 0,6,
	ta = C0*a[0]+C3*a[6];
	tb = C0*b[0]+C3*b[6];
	r[2] += ta*b[2]+tb*a[2];
	t = a[2]*b[2];
	r[0] += C0*t;
	r[6] += C3*t;

	// [2,3]: 7,
	ta = C2*a[7];
	tb = C2*b[7];
	r[2] += ta*b[3]+tb*a[3];
	r[3] += ta*b[2]+tb*a[2];
	t = a[2]*b[3]+a[3]*b[2];
	r[7] = C2*t;

	// [3,3]: 0,6,8,
	ta = C0*a[0]+C1*a[6]+C2*a[8];
	tb = C0*b[0]+C1*b[6]+C2*b[8];
	r[3] += ta*b[3]+tb*a[3];
	t = a[3]*b[3];
	r[0] += C0*t;
	r[6] += C1*t;
	r[8] += C2*t;

	// [4,4]: 0,6,
	ta = C0*a[0]-C4*a[6];
	tb = C0*b[0]-C4*b[6];
	r[4] += ta*b[4]+tb*a[4];
	t = a[4]*b[4];
	r[0] += C0*t;
	r[6] -= C4*t;

	// [4,5]: 7,
	ta = C5*a[7];
	tb = C5*b[7];
	r[4] += ta*b[5]+tb*a[5];
	r[5] += ta*b[4]+tb*a[4];
	t = a[4]*b[5]+a[5]*b[4];
	r[7] += C5*t;

	// [5,5]: 0,6,8,
	ta = C0*a[0]+C6*a[6]-C5*a[8];
	tb = C0*b[0]+C6*b[6]-C5*b[8];
	r[5] += ta*b[5]+tb*a[5];
	t = a[5]*b[5];
	r[0] += C0*t;
	r[6] += C6*t;
	r[8] -= C5*t;

	// [6,6]: 0,6,
	ta = C0*a[0];
	tb = C0*b[0];
	r[6] += ta*b[6]+tb*a[6];
	t = a[6]*b[6];
	r[0] += C0*t;
	r[6] += C4*t;

	// [7,7]: 0,6,8,
	ta = C0*a[0]+C6*a[6]+C5*a[8];
	tb = C0*b[0]+C6*b[6]+C5*b[8];
	r[7] += ta*b[7]+tb*a[7];
	t = a[7]*b[7];
	r[0] += C0*t;
	r[6] += C6*t;
	r[8] += C5*t;

	// [8,8]: 0,6,
	ta = C0*a[0]-C4*a[6];
	tb = C0*b[0]-C4*b[6];
	r[8] += ta*b[8]+tb*a[8];
	t = a[8]*b[8];
	r[0] += C0*t;
	r[6] -= C4*t;
	// entry count=13
	// multiplications count=120
	// addition count=74
}
void SHProduct( const in float a[9], const in float3 b[9], out float3 r[9] ) {
	float3	ta, tb, t;

	const float	C0 = 0.282094792935999980;
	const float	C1 = -0.126156626101000010;
	const float	C2 = 0.218509686119999990;
	const float	C3 = 0.252313259986999990;
	const float	C4 = 0.180223751576000010;
	const float	C5 = 0.156078347226000000;
	const float	C6 = 0.090111875786499998;

	// [0,0]: 0,
	r[0] = C0*a[0]*b[0];

	// [1,1]: 0,6,8,
	ta = C0*a[0]+C1*a[6]-C2*a[8];
	tb = C0*b[0]+C1*b[6]-C2*b[8];
	r[1] = ta*b[1]+tb*a[1];
	t = a[1]*b[1];
	r[0] += C0*t;
	r[6] = C1*t;
	r[8] = -C2*t;

	// [1,2]: 5,
	ta = C2*a[5];
	tb = C2*b[5];
	r[1] += ta*b[2]+tb*a[2];
	r[2] = ta*b[1]+tb*a[1];
	t = a[1]*b[2]+a[2]*b[1];
	r[5] = C2*t;

	// [1,3]: 4,
	ta = C2*a[4];
	tb = C2*b[4];
	r[1] += ta*b[3]+tb*a[3];
	r[3] = ta*b[1]+tb*a[1];
	t = a[1]*b[3]+a[3]*b[1];
	r[4] = C2*t;

	// [2,2]: 0,6,
	ta = C0*a[0]+C3*a[6];
	tb = C0*b[0]+C3*b[6];
	r[2] += ta*b[2]+tb*a[2];
	t = a[2]*b[2];
	r[0] += C0*t;
	r[6] += C3*t;

	// [2,3]: 7,
	ta = C2*a[7];
	tb = C2*b[7];
	r[2] += ta*b[3]+tb*a[3];
	r[3] += ta*b[2]+tb*a[2];
	t = a[2]*b[3]+a[3]*b[2];
	r[7] = C2*t;

	// [3,3]: 0,6,8,
	ta = C0*a[0]+C1*a[6]+C2*a[8];
	tb = C0*b[0]+C1*b[6]+C2*b[8];
	r[3] += ta*b[3]+tb*a[3];
	t = a[3]*b[3];
	r[0] += C0*t;
	r[6] += C1*t;
	r[8] += C2*t;

	// [4,4]: 0,6,
	ta = C0*a[0]-C4*a[6];
	tb = C0*b[0]-C4*b[6];
	r[4] += ta*b[4]+tb*a[4];
	t = a[4]*b[4];
	r[0] += C0*t;
	r[6] -= C4*t;

	// [4,5]: 7,
	ta = C5*a[7];
	tb = C5*b[7];
	r[4] += ta*b[5]+tb*a[5];
	r[5] += ta*b[4]+tb*a[4];
	t = a[4]*b[5]+a[5]*b[4];
	r[7] += C5*t;

	// [5,5]: 0,6,8,
	ta = C0*a[0]+C6*a[6]-C5*a[8];
	tb = C0*b[0]+C6*b[6]-C5*b[8];
	r[5] += ta*b[5]+tb*a[5];
	t = a[5]*b[5];
	r[0] += C0*t;
	r[6] += C6*t;
	r[8] -= C5*t;

	// [6,6]: 0,6,
	ta = C0*a[0];
	tb = C0*b[0];
	r[6] += ta*b[6]+tb*a[6];
	t = a[6]*b[6];
	r[0] += C0*t;
	r[6] += C4*t;

	// [7,7]: 0,6,8,
	ta = C0*a[0]+C6*a[6]+C5*a[8];
	tb = C0*b[0]+C6*b[6]+C5*b[8];
	r[7] += ta*b[7]+tb*a[7];
	t = a[7]*b[7];
	r[0] += C0*t;
	r[6] += C6*t;
	r[8] += C5*t;

	// [8,8]: 0,6,
	ta = C0*a[0]-C4*a[6];
	tb = C0*b[0]-C4*b[6];
	r[8] += ta*b[8]+tb*a[8];
	t = a[8]*b[8];
	r[0] += C0*t;
	r[6] -= C4*t;
	// entry count=13
	// multiplications count=120
	// addition count=74
}
