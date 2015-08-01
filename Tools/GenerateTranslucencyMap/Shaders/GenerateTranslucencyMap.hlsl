////////////////////////////////////////////////////////////////////////////////
// Directional Translucency Map generator
// This compute shader will generate the directional translucency map and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const int	DIPOLES_COUNT = 3;	// We're using a maximum of 3 dipoles

static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2 degree observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

cbuffer	CBInput : register( b0 ) {
	uint	_Width;
	uint	_Height;
	float	_TexelSize_mm;		// Texel size in millimeters
	float	_Thickness_mm;		// Thickness map max encoded displacement in millimeters

	uint	_KernelSize;		// Size of the convolution kernel
	float	_Sigma_a;			// Absorption coefficient (mm^-1)
	float	_Sigma_s;			// Scattering coefficient (mm^-1)
	float	_g;					// Scattering anisotropy (mean cosine of scattering phase function)

	float3	_Light;				// Light direction (in tangent space)
	float	_F0;				// Fresnel reflection coefficient at normal incidence
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture2D<float>			_SourceThickness : register( t0 );
Texture2D<float3>			_SourceNormal : register( t1 );
Texture2D<float3>			_SourceTransmittance : register( t2 );
Texture2D<float4>			_SourceAlbedo : register( t3 );
Texture3D<float>			_SourceVisibility : register( t4 );	// This is an interpolable array of 16 principal visiblity directions
																// Each slice in the array gives the cos(horizon angle) to compare against a ray direction

Texture2D<float4>			_Previous : register( t5 );
RWTexture2D<float4>			_Target : register( u0 );

StructuredBuffer<float3>	_Rays : register( t1 );

// Computes the visibility of the ray at the given position using the horizon map
float	ComputeVisibility( float2 _UV, float3 _Direction ) {
	float	phi = atan2( _Direction.y, _Direction.x );	// [-PI,PI]
	float	W = phi / (2.0 * PI);						// [-0.5,+0.5]
	float	cosTheta = _SourceVisibility.SampleLevel( LinearWrap, float3( _UV, W ), 0.0 );

//return _Direction.z > cosTheta ? 1 : 0;

	float	cosThetaMin = 0.9 * cosTheta;
	float	cosThetaMax = 1.1 * cosTheta;
	return smoothstep( cosThetaMin, cosThetaMax, _Direction.z );
}

// Schlick's approximation to Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float	FresnelSchlick( float _F0, float _CosTheta, float _FresnelStrength=1.0 )
{
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelPosition = _DispatchThreadID.xy;
	if ( PixelPosition.x >= _Width || PixelPosition.y >= _Height )
		return;

	float2	pixel2UV = 1.0 / float2( _Width, _Height );
	float2	UV = pixel2UV * float2( 0.5 + PixelPosition );

	uint	K = 2*_KernelSize;

	// Compute general parameters (TODO: move to constant buffer)
	float	sigma_s = (1.0 - _g) * _Sigma_s;				// Reduced scattering coefficient
	float	sigma_t = _Sigma_a + sigma_s;					// Reduced extinction coefficient
	float	alpha = sigma_s / sigma_t;						// Reduced albedo
	float	l = 1.0 / sigma_t;								// Mean free path
	float	D = l / 3.0;

	float3	result = 0.0;
	uint2	localPos;
	for ( uint Y=0; Y <= K; Y++ ) {
		localPos.y = PixelPosition.y - _KernelSize + Y;
		if ( localPos.y > _Height )
			continue;

		for ( uint X=0; X <= K; X++ ) {
			localPos.x = PixelPosition.x - _KernelSize + X;
			if ( localPos.x > _Width )
				continue;

			float2	LocalUV = pixel2UV * float2( 0.5 + localPos );

			// Fetch local thickness
			float	d = _Thickness_mm * _SourceThickness.Load( uint3( localPos, 0 ) ).x;

			// Fetch local diffuse albedo and compute average diffuse reflectance
			float4	albedo = _SourceAlbedo.SampleLevel( LinearClamp, LocalUV, 0.0 );
			float	rho_d = dot( LUMINANCE, albedo.xyz );							// Luminance = average albedo reflectance

			d *= albedo.w;

			// Fetch local normal
			float3	normal = 2.0 * _SourceNormal.SampleLevel( LinearClamp, LocalUV, 0.0 ) - 1.0;

			// Compute diffuse fresnel transmission
			float	Fs = FresnelSchlick( _F0, saturate( dot( _Light, normal ) ) );
			float	Fd = 1.0 - Fs;	// I know it's a bit more complicated than this...

			// Fetch local transmittance
			float3	transmittance = max( 0.0, _SourceTransmittance.SampleLevel( LinearClamp, LocalUV, 0.0 ) );
			float3	sigma_tr = sqrt( 3.0 * _Sigma_a * sigma_t * transmittance * transmittance );	// Effective transport coefficient (in mm^-1)

			// Compute local parameters
			float	A = (1.0 + rho_d) / (1.0 - rho_d);								// Change of fluence due to internal reflection at the surface
			float	Zb = 2.0 * A * D;

			// Compute transmittance (accumulate dipoles contribution)
			float2	relativePos = _TexelSize_mm * (float2( X, Y ) - _KernelSize);
			float3	Tr = 0.0;
			for ( int DipoleIndex=-DIPOLES_COUNT; DipoleIndex <= DIPOLES_COUNT; DipoleIndex++ ) {
				float	Zr = 2.0 * DipoleIndex * (d + 2.0 * Zb) + l;
				float	Zv = Zr - 2.0 * (l + Zb);

				float	Dr = length( float3( relativePos, Zr ) );		// in mm
				float	Dv = length( float3( relativePos, Zv ) );		// in mm

				float3	pole_r = (d - Zr) * (1.0 + sigma_tr * Dr) * exp( -sigma_tr * Dr ) / (Dr * Dr * Dr);	// Real
				float3	pole_v = (d - Zv) * (1.0 + sigma_tr * Dv) * exp( -sigma_tr * Dv ) / (Dv * Dv * Dv);	// Virtual
// 				float3	pole_r = Zr * (1.0 + sigma_tr * Dr) * exp( -sigma_tr * Dr ) / (Dr * Dr * Dr);	// Real
// 				float3	pole_v = Zv * (1.0 + sigma_tr * Dv) * exp( -sigma_tr * Dv ) / (Dv * Dv * Dv);	// Virtual
//				Tr += pole_r - pole_v;
//				Tr += abs( pole_r - pole_v );	// Another problem here: I have to use the absolute value otherwise I have negative results! And taking the abs() at the end gives shitty results...
				Tr += max( 0.0, pole_r - pole_v );	// Another problem here: I have to use the absolute value otherwise I have negative results! And taking the abs() at the end gives shitty results...
			}
//			Tr *= alpha / (4.0 * PI);

			// Compute irradiance for our light directions
//			float3	rho_t = 1.0 - Fd * albedo.xyz;	// This completely reverses the colors! I can't understand why since I'm doing exactly what's said in the paper... :(
			float3	rho_t = Fd * transmittance;		// So instead I'm using the transmittance directly...

			float	visibility = ComputeVisibility( LocalUV, _Light );
			float3	E = rho_t * visibility * saturate( dot( normal, _Light ) );

			// Accumulate
			result += E * Tr;
		}
	}

	result *= alpha / (4.0 * PI);	// Transmittance constant weighting is only done at the end


//	result *= _TexelSize_mm*_TexelSize_mm / (K*K);
	result *= _TexelSize_mm*_TexelSize_mm;


	// TODO: Single scattering term?
	// TODO: Single scattering term?
	// TODO: Single scattering term?
	// TODO: Single scattering term?
	// TODO: Single scattering term?
	// TODO: Single scattering term?

//result *= 0.1;

	float4	Current = _Previous[PixelPosition];
	_Target[PixelPosition] = Current + float4( result, 1 );
}