////////////////////////////////////////////////////////////////////////////////
// Result display
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2 degree observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)
static const float	LIGHT_INTENSITY = 1.0;
static const float	AIRLIGHT_BETA = 0.01;
static const float	CAMERA_FOV = 60.0 * PI / 180.0;

cbuffer	CBDisplay : register( b0 ) {
	uint2	_Size;
	float	_Time;
	uint	_Flags;
	float3	_Light;			// Light position
	float	_Thickness_mm;	// Texture thickness
	float3	_CameraPos;
	float	_Size_mm;		// Texture size
	float3	_CameraTarget;
	float	_IOR;			// IOR
	float3	_CameraUp;
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture2D<float>			_TexThickness : register( t0 );
Texture2D<float3>			_TexNormal : register( t1 );
Texture2D<float4>			_TexTransmittance : register( t2 );
Texture2D<float4>			_TexAlbedo : register( t3 );
Texture3D<float>			_TexVisibility : register( t4 );	// This is an interpolable array of 16 principal visiblity directions

Texture2D<float3>			_TexResult0 : register( t5 );
Texture2D<float3>			_TexResult1 : register( t6 );
Texture2D<float3>			_TexResult2 : register( t7 );
Texture2D<float3>			_TexResultRGB : register( t8 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

// This is my analytical solution to the airlight integral as explained in "A Practical Analytic Single Scattering Model for Real Time Rendering" (http://www.cs.columbia.edu/~bosun/sig05.htm)
// u€[0,10], v€[0,PI/2]
float	F( float _u, float _v ) {
	float	a = 0.00118554 + _v * (0.599188 - 0.012787 * _v);
	float	b = 0.977767 + _v * (-0.748114 + _v* (0.555383 - _v * 0.175846));
	return _v * exp( a * pow( _u, b ) );
}

float	Airlight( float3 _cameraPosition, float3 _cameraView, float3 _lightPosition, float3 _hitPosition, float _beta ) {
	float3	V2S = _lightPosition - _cameraPosition;
	float	Dsv = length( V2S );
			V2S /= Dsv;
	float	Tsv = _beta * Dsv;	// Optical thickness from light source to camera

	float3	V2P = _hitPosition - _cameraPosition;
	float	Dvp = length( V2P );
			V2P /= Dvp;
	float	Tvp = _beta * Dvp;	// Optical thickness from hit position to camera

	float	CosGamma = dot( V2P, V2S );
	float	SinGamma = sqrt( 1.0 - CosGamma*CosGamma );
	float	Gamma = acos( CosGamma );
	float	A1 = Tsv * SinGamma;
	float	A0 = _beta*_beta * exp( -Tsv * CosGamma ) / (2.0 * PI * A1);

	// Estimate integral
	float	v0 = 0.5 * Gamma;
	float	F0 = F( A1, v0 );

	float	v1 = 0.25 * PI + atan( (Tvp - Tsv * CosGamma) / A1 );
//	float	F1 = F( A1, v1 );
	float	F1 = saturate( dot( V2S, _cameraView )) * F( A1, v1 );

	return A0 * saturate( F1 - F0 );
}

float3	ShowVisibilityMap( float2 _UV ) {

	float	phi = fmod( _Time + PI, 2.0 * PI ) - PI;

//phi = 0*0.5 * PI;

	if ( all(_UV < 0.1) ) {
		_UV /= 0.1;
		float2	xy = 2.0 * _UV - 1.0;
				xy.y *= -1.0;
		if ( length( xy ) < 1.0 ) {
			float	pixelPhi = atan2( xy.y, xy.x );
			return	abs(pixelPhi-phi) < 0.05 ? 1.0 : 0.0;
		}
	}

//	return _TexThickness.SampleLevel( LinearWrap, _UV, 0.0 );
//	return _TexAlbedo.SampleLevel( LinearWrap, _UV, 0.0 );
// 	return _TexTransmittance.SampleLevel( LinearWrap, _UV, 0.0 );
//	return _TexNormal.SampleLevel( LinearWrap, _UV, 0.0 );
	return _TexVisibility.SampleLevel( LinearWrap, float3( _UV, phi / (2.0 * PI) ), 0.0 );
	return float3( sin( _Time ) * _UV, 0 );
	return float3( sin( _Time ), 0, 0 );
}

// Schlick's approximation to Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float	FresnelSchlick( float _F0, float _CosTheta, float _FresnelStrength=1.0 )
{
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

// Ward specular reflection model
// From "A New Ward BRDF with Bounded Albedo" by Dür et al.
// (isotropic version, I know it's stupid since Ward is essentially interesting for its anisotropic characteristics but in the engine we don't even use it...)
//
float	ComputeWard( float3 _Light, float3 _View, float3 _Normal, float3 _Tangent, float3 _BiTangent, float _Roughness ) {

	float3	H_unorm = _Light + _View;
	float	invRoughness = 1.0 / _Roughness;
	float	invSqRoughness = invRoughness * invRoughness;

#if 1
	float	HdotH = dot( H_unorm, H_unorm );
	float	HdotN = dot( H_unorm, _Normal );
	float	HdotT = dot( H_unorm, _Tangent );
	float	HdotB = dot( H_unorm, _BiTangent );
	float	sqHdotN = HdotN * HdotN;
	float	invSqHdotN = 1.0 / sqHdotN;
	float	sqTanHdotN = (HdotT*HdotT + HdotB*HdotB) * invSqHdotN;
	return (1.0/PI) * invSqRoughness * (exp( -invSqRoughness * sqTanHdotN ) * invSqHdotN * invSqHdotN) * HdotH;
#else
	float3	H = normalize( H_unorm );
	float	NdotH = dot( _Normal, H );
	float	LdotH = dot( _Light, H );
	float	cosTheta = NdotH;
	float	sqCosTheta = cosTheta * cosTheta;
	float	sqSinTheta = 1.0 - sqCosTheta;
	float	sqTanTheta = sqSinTheta / sqCosTheta;
	return (1.0/PI) * invSqRoughness * exp( -invSqRoughness * sqTanTheta ) / (4 * NdotH*NdotH*NdotH*NdotH * LdotH*LdotH);
#endif
}
float4	ComputeTranslucency( float3 _wsPosition, float3 _wsView ) {
	float2	UV = _wsPosition.xz / (0.001 * _Size_mm) - 0.5;

	// Get albedo
	float4	rawAlbedo = _TexAlbedo.SampleLevel( LinearWrap, UV, 0.0 );
	if ( rawAlbedo.w < 0.5 )
		return 0.0;
	float3	Rho_d = rawAlbedo.xyz / PI;

	// Get normal & compute tangent space
	float3	tsNormal = 2.0 * _TexNormal.SampleLevel( LinearWrap, UV, 0.0 ) - 1.0;
	float3	wsNormal = float3( tsNormal.x, tsNormal.z, tsNormal.y ); 

	float3	wsTangent = normalize( cross( wsNormal, float3( 0, 0, 1 ) ) );
	float3	wsBiTangent = cross( wsTangent, wsNormal );

	// Compute light intensity & direction
	float3	L = LIGHT_INTENSITY;
	float3	Light = _Light - _wsPosition;	// Light direction
	float	d = length( Light );
			Light /= d;

	L /= max( 1e-3, d * d );	// 1/r² attenuation

	// Build special vectors & dots
	float3	H = normalize( Light + _wsView );
	float	NdotL = dot( wsNormal, Light );
	float	NdotH = dot( wsNormal, H );
	float	NdotV = dot( wsNormal, _wsView );

	// Compute Fresnel reflectance
	float	F0 = (_IOR - 1.0f) / (_IOR + 1.0f);
	float	Fs = FresnelSchlick( F0, NdotL );

	if ( _wsView.y < 0.0 ) {
		// Standard front lighting
		float	Rho_s = ComputeWard( Light, -_wsView, wsNormal, wsTangent, wsBiTangent, 0.2 );	// Standard roughness for a leaf... Could be a parm...
		return float4( L * lerp( Rho_d, Rho_s, Fs) * NdotL, 0.0 );
	}

	// Back lighting
	const float3	HL2_R = float3( sqrt( 2.0 / 3.0 ), 0.0, sqrt( 1.0 / 3.0 ) );
	const float3	HL2_G = float3( -sqrt( 1.0 / 6.0 ), sqrt( 1.0 / 2.0 ), sqrt( 1.0 / 3.0 ) );
	const float3	HL2_B = float3( -sqrt( 1.0 / 6.0 ), -sqrt( 1.0 / 2.0 ), sqrt( 1.0 / 3.0 ) );

	float3	HL2Light = float3( -Light.x, Light.z, Light.y );	// Translucency was computed for Z-up

	float3	Translucency = 0.0;
	if ( (_Flags & 2) != 0 ) {
		float3	Tr = _TexResultRGB.SampleLevel( LinearWrap, UV, 0.0 );
		Translucency = Tr.x * saturate( dot( HL2_R, HL2Light ) )
					 + Tr.y * saturate( dot( HL2_G, HL2Light ) )
					 + Tr.z * saturate( dot( HL2_B, HL2Light ) );

		Translucency *= _TexTransmittance.SampleLevel( LinearWrap, UV, 0.0 ).xyz;
	} else {
		float3	Tr0 = _TexResult0.SampleLevel( LinearWrap, UV, 0.0 );
		float3	Tr1 = _TexResult0.SampleLevel( LinearWrap, UV, 0.0 );
		float3	Tr2 = _TexResult0.SampleLevel( LinearWrap, UV, 0.0 );
		Translucency = Tr0 * saturate( dot( HL2_R, HL2Light ) )
					 + Tr1 * saturate( dot( HL2_G, HL2Light ) )
					 + Tr2 * saturate( dot( HL2_B, HL2Light ) );
	}

Translucency *= 1.0;

	float3	Rho_t = 1.0 - (1.0-Fs) * Rho_d * PI;

	return float4( L * (Rho_t / PI) * Translucency, 1.0 );
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Size;

//return _TexResultRGB.SampleLevel( LinearWrap, UV, 0.0 );

	if ( (_Flags & 4) != 0 )
		return ShowVisibilityMap( UV );

	if ( (_Flags & 1) == 0 )
		return float3( sin( _Time ) * UV, 0 );

	// Build the camera ray
	float3	At = normalize( _CameraTarget - _CameraPos );
	float3	Right = normalize( cross( At, _CameraUp ) );
	float3	Up = cross( Right, At );

	float	TanHalfFOV = tan( 0.5 * CAMERA_FOV );
	float3	csView = normalize( float3( _Size.x / _Size.y * TanHalfFOV * (2.0 * UV.x - 1.0), TanHalfFOV * (1.0 - 2.0 * UV.y ), 1.0 ) );
	float3	wsView = csView.x * Right + csView.y * Up + csView.z * At;

	// Intersect with ground plane
	float4	Color = 0.0;
	float	t = -_CameraPos.y / wsView.y;
	if ( t > 0.0 )
		Color = ComputeTranslucency( _CameraPos + t * wsView, wsView );
	if ( t <= 0.0 || (_CameraPos.y < 0.0 && all( Color < 1e-5 )) )
		t = 1000.0;	// Infinity

	// Add light scattering
//	Color += LIGHT_INTENSITY * Airlight( _CameraPos, wsView, _Light, _CameraPos + t * wsView, AIRLIGHT_BETA );
	Color += (1.0 - Color.w) * LIGHT_INTENSITY * pow( saturate( dot( normalize( _Light - _CameraPos ), wsView ) ), 1000.0 );

	return Color.xyz;
}
