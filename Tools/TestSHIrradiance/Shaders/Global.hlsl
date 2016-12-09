////////////////////////////////////////////////////////////////////////////////
// Global Defines
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float	INVPI = 0.31830988618379067153776752674503;
static const float	CAMERA_FOV = 90.0 * PI / 180.0;
static const float	TAN_HALF_FOV = tan( 0.5 * CAMERA_FOV );
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)
static const float	INFINITY = 1e12;
static const float	NO_HIT = 1e6;

cbuffer	CBDisplay : register( b0 ) {
	uint2		_size;
	float		_time;
	uint		_flags;
	float4x4	_world2Proj;
//	float4x4	_proj2World;
	float4x4	_camera2World;
	float		_cosAO;
	float		_luminanceFactor;
	float		_filterWindowSize;
}

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

Texture2D<float4>	_TexHDR : register( t0 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }


// Samples the panormaic environment HDR map
float3	SampleHDREnvironment( float3 _wsDirection ) {
	float	phi = atan2( _wsDirection.y, _wsDirection.x );
	float	theta = acos( _wsDirection.z );
	float2	UV = float2( 0.5 + 0.5 * phi * INVPI, theta * INVPI );
	return _TexHDR.SampleLevel( LinearWrap, UV, 0.0 ).xyz;
}

// Estimates A0, A1 and A2 integrals based on the angle of the AO cone from the normal and cos(PI/2 * AO) defining the AO cone's half-angle aperture
// Fitting was done with mathematica:
//
//	A0 = a + b*x + d*x*y + e*x^2 + f*y^2 + g*x^2*y + i*x^2*y^2;
//	With {a -> 0.86342, b -> 0.127258, c -> 4.9738*10^-14, d -> -0.903477, e -> -0.967484, f -> -0.411706, g -> 0.885699, h -> 0., i -> 0.407098}
//
//	A1 = a + b*x + c*y + d*x*y + e*x^2 + f*y^2 + g*x^2*y + h*x*y^2;
//	With {a -> 0.95672, b -> 0.790566, c -> 0.298642, d -> -2.63968, e -> -1.65043, f -> -0.720222, g -> 2.14987, h -> 0.788641 }
//
//	A2 = a + b*x + c*y + d*x*y + e*x^2 + f*y^2 + g*x^2*y + h*x*y^2 + i*x^2*y^2 + j*x^3 + k*y^3 + l*x^3*y + m*x*y^3 + p*x^3*y^3;
//	With {a -> 0.523407, b -> -0.6694, c -> -0.128209, d -> 5.26746, e -> 3.40837, f -> 0.905606, g -> -12.8261, h -> -10.5428, i -> 9.40113, j -> -3.18758, k -> -1.08565, l -> 7.57317, m -> 5.45239, p -> -4.06299}
//
float3	EstimateLambertReflectanceFactors( float _cosThetaAO, float _coneBendAngle ) {
//	float	cosThetaAO = cos( 0.5 * PI * _AO );

	float	x = _cosThetaAO;
	float	y = _coneBendAngle * 2.0 * INVPI;

	float	x2 = x*x;
	float	x3 = x*x2;
	float	y2 = y*y;
	float	y3 = y*y2;

	const float3	a = float3( 0.86342, 0.95672, 0.523407 );
	const float3	b = float3( 0.127258, 0.790566, -0.6694 );
	const float3	c = float3( 0.0, 0.298642, -0.128209 );
	const float3	d = float3( -0.903477, -2.63968, 5.26746 );
	const float3	e = float3( -0.967484, -1.65043, 3.40837 );
	const float3	f = float3( -0.411706, -0.720222, 0.905606 );
	const float3	g = float3( 0.885699, 2.14987, -12.8261 );
	const float3	h = float3( 0.0, 0.788641, -10.5428 );
	const float3	i = float3( 0.407098, 0.0, 9.40113 );
	const float		j = -3.18758, k = -1.08565, l = 7.57317, m = 5.45239, p = -4.06299;

	float	A0 = a.x + x * (b.x + y * d.x + x * (e.x + y * (g.x + y * i.x))) + f.x * y2;
	float	A1 = a.y + x * (b.y + y * (d.y + h.y * y) + x * (e.y + y * g.y)) + y * (c.y + y * f.y);
	float	A2 = a.z + x * (b.z + y * d.z + x * (e.z + y * (g.z + y * i.z) + x * (j + y * (l + y2 * p))))
					 + y * (c.z + y * (f.z + x * h.z + y * k));

	return float3( A0, A1, A2 );
}

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

// Computes the Ylm coefficients in the requested direction
//
void	Ylm( float3 _direction, out float3 _SH[9] ) {
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

// Evaluates the SH coefficients in the requested direction
// Analytic method from http://www1.cs.columbia.edu/~ravir/papers/envmap/envmap.pdf eq. 3
//
float3	EvaluateSH( float3 _direction, float3 _SH[9] ) {
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
// Analytic method from http://www1.cs.columbia.edu/~ravir/papers/envmap/envmap.pdf eq. 13
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
// Here, _CosThetaAO = cos( PI/2 * AO ) and represents the cosine of the cone half-angle that drives the amount of light a surface is perceiving
//
float3	EvaluateSHIrradiance( float3 _direction, float _CosThetaAO,  float3 _SH[9] ) {
	float		t2 = _CosThetaAO*_CosThetaAO;
	float		t3 = t2*_CosThetaAO;
	float		t4 = t3*_CosThetaAO;
	float		ct2 = 1.0 - t2; 

	float		c0 = 0.88622692545275801364908374167057 * ct2;							// 1/2 * sqrt(PI) * (1-t^2)
	float		c1 = 1.02332670794648848847955162488930 * (1.0-t3);						// sqrt(PI/3) * (1-t^3)
	float		c2 = 0.24770795610037568833406429782001 * (3.0 * (1.0-t4) - 2.0 * ct2);	// 1/16 * sqrt(5*PI) * [3(1-t^4) - 2(1-t^2)]
	const float	sqrt3 = 1.7320508075688772935274463415059;

	float		x = _direction.x;
	float		y = _direction.y;
	float		z = _direction.z;

	return	max( 0.0, c0 * _SH[0]										// c0.L00
			+ c1 * (_SH[1]*y + _SH[2]*z + _SH[3]*x)						// c1.(L1-1.y + L10.z + L11.x)
			+ c2 * (_SH[6]*(3.0*z*z - 1.0)								// c2.L20.(3z²-1)
				+ sqrt3 * (_SH[8]*(x*x - y*y)							// sqrt(3).c2.L22.(x²-y²)
					+ 2.0 * (_SH[4]*x*y + _SH[5]*y*z + _SH[7]*z*x)))	// 2sqrt(3).c2.(L2-2.xy + L2-1.yz + L21.zx)
		);
}

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

float	IntersectSphere( float3 _pos, float3 _dir, float3 _center, float _radius ) {
	float3	D = _pos - _center;
	float	b = dot( D, _dir );
	float	c = dot( D, D ) - _radius*_radius;
	float	delta = b*b - c;
	return delta > 0.0 ? -b - sqrt( delta ) : INFINITY;
}

float	IntersectPlane( float3 _pos, float3 _dir, float3 _planePosition, float3 _normal ) {
	float3	D = _pos - _planePosition;
	return -dot( D, _normal ) / dot( _dir, _normal );
}
