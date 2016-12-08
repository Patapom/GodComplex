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
	uint2		_Size;
	float		_Time;
	uint		_Flags;
	float4x4	_world2Proj;
//	float4x4	_proj2World;
	float4x4	_camera2World;
	float		_cosAO;
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
	float	theta =acos( _wsDirection.z );
	float2	UV = float2( 0.5 * phi * INVPI, theta * INVPI );
	return _TexHDR.SampleLevel( LinearWrap, UV, 0.0 ).xyz;
}


// Grace probe
static const float3	EnvironmentSH[9] = {
	float3( 0.933358105849532, 0.605499186927096, 0.450999072970855 ), 
	float3( 0.0542981143130068, 0.0409598475963159, 0.0355377036564806 ), 
	float3( 0.914255336642483, 0.651103534810611, 0.518065694132826 ), 
	float3( 0.238207071886099, 0.14912965904707, 0.0912559191766972 ), 
	float3( 0.0321476755042544, 0.0258939812282057, 0.0324159089991572 ), 
	float3( 0.104707893908821, 0.0756648975030993, 0.0749934936107284 ), 
	float3( 1.27654512826622, 0.85613828921136, 0.618241442250845 ), 
	float3( 0.473237767573493, 0.304160108872238, 0.193304867770535 ), 
	float3( 0.143726445535245, 0.0847402441253633, 0.0587779174281925 ), 
};

// Evaluates the SH coefficients in the requested direction
// Analytic method from http://www1.cs.columbia.edu/~ravir/papers/envmap/envmap.pdf eq. 3
//
float3	EvaluateSH( float3 _Direction, float3 _SH[9] ) {
	const float	f0 = 0.28209479177387814347403972578039;		// 0.5 / sqrt(PI);
	const float	f1 = 0.48860251190291992158638462283835;		// 0.5 * sqrt(3/PI);
	const float	f2 = 1.0925484305920790705433857058027;			// 0.5 * sqrt(15/PI);
	const float	f3 = 0.31539156525252000603089369029571;		// 0.25 * sqrt(5.PI);

	float	EvalSH0 = f0;
	float4	EvalSH1234, EvalSH5678;
	EvalSH1234.x = f1 * _Direction.y;
	EvalSH1234.y = f1 * _Direction.z;
	EvalSH1234.z = f1 * _Direction.x;
	EvalSH1234.w = f2 * _Direction.x * _Direction.y;
	EvalSH5678.x = f2 * _Direction.y * _Direction.z;
	EvalSH5678.y = f3 * (3.0 * _Direction.z*_Direction.z - 1.0);
	EvalSH5678.z = f2 * _Direction.x * _Direction.z;
	EvalSH5678.w = f2 * 0.5 * (_Direction.x*_Direction.x - _Direction.y*_Direction.y);

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
float3	EvaluateSHIrradiance( float3 _Direction, float3 _SH[9] ) {
	const float	c1 = 0.42904276540489171563379376569857;	// 4 * Â2.Y22 = 1/4 * sqrt(15.PI)
	const float	c2 = 0.51166335397324424423977581244463;	// 0.5 * Â1.Y10 = 1/2 * sqrt(PI/3)
	const float	c3 = 0.24770795610037568833406429782001;	// Â2.Y20 = 1/16 * sqrt(5.PI)
	const float	c4 = 0.88622692545275801364908374167057;	// Â0.Y00 = 1/2 * sqrt(PI)

	float	x = _Direction.x;
	float	y = _Direction.y;
	float	z = _Direction.z;

	return	max( 0.0,
			(c1*(x*x - y*y)) * _SH[8]			// c1.L22.(x²-y²)
			+ (c3*(3.0*z*z - 1)) * _SH[6]			// c3.L20.(3.z² - 1)
			+ c4 * _SH[0]					// c4.L00 
			+ 2.0*c1*(_SH[4]*x*y + _SH[7]*x*z + _SH[5]*y*z)	// 2.c1.(L2-2.xy + L21.xz + L2-1.yz)
			+ 2.0*c2*(_SH[3]*x + _SH[1]*y + _SH[2]*z) );	// 2.c2.(L11.x + L1-1.y + L10.z)
}

// Evaluates the irradiance perceived in the provided direction, also accounting for Ambient Occlusion
// Details can be found at http://wiki.nuaj.net/index.php?title=SphericalHarmonicsPortal
// Here, _CosThetaAO = cos( PI/2 * AO ) and represents the cosine of the cone half-angle that drives the amount of light a surface is perceiving
//
float3	EvaluateSHIrradiance( float3 _Direction, float _CosThetaAO,  float3 _SH[9] ) {
	float		t2 = _CosThetaAO*_CosThetaAO;
	float		t3 = t2*_CosThetaAO;
	float		t4 = t3*_CosThetaAO;
	float		ct2 = 1.0 - t2; 

	float		c0 = 0.88622692545275801364908374167057 * ct2;							// 1/2 * sqrt(PI) * (1-t^2)
	float		c1 = 1.02332670794648848847955162488930 * (1.0-t3);						// sqrt(PI/3) * (1-t^3)
	float		c2 = 0.24770795610037568833406429782001 * (3.0 * (1.0-t4) - 2.0 * ct2);	// 1/16 * sqrt(5*PI) * [3(1-t^4) - 2(1-t^2)]
	const float	sqrt3 = 1.7320508075688772935274463415059;

	float		x = _Direction.x;
	float		y = _Direction.y;
	float		z = _Direction.z;

	return	max( 0.0, c0 * _SH[0]										// c0.L00
			+ c1 * (_SH[1]*y + _SH[2]*z + _SH[3]*x)						// c1.(L1-1.y + L10.z + L11.x)
			+ c2 * (_SH[6]*(3.0*z*z - 1.0)								// c2.L20.(3z²-1)
				+ sqrt3 * (_SH[8]*(x*x - y*y)							// sqrt(3).c2.L22.(x²-y²)
					+ 2.0 * (_SH[4]*x*y + _SH[5]*y*z + _SH[7]*z*x)))	// 2sqrt(3).c2.(L2-2.xy + L2-1.yz + L21.zx)
		);
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
