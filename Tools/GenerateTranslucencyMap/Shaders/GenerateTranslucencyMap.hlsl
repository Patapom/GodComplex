////////////////////////////////////////////////////////////////////////////////
// Directional Translucency Map generator
// This compute shader will generate the directional translucency map and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const uint	DIPOLES_COUNT = 3;	// We're using a maximum of 3 dipoles

static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

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
}

SamplerState LinearWrap		: register( s2 );

Texture2D<float>			_SourceThickness : register( t0 );
Texture2D<float>			_SourceNormal : register( t1 );
Texture2D<float>			_SourceTransmittance : register( t2 );
Texture2D<float>			_SourceAlbedo : register( t3 );
Texture3D<float>			_SourceVisibility : register( t4 );	// This is an interpolable array of 16 principal visiblity directions
																// Each slice in the array gives the cos(horizon angle) to compare against a ray direction

RWTexture2D<float4>			_Target : register( u0 );
// RWTexture2D<float4>			_Target1 : register( u1 );
// RWTexture2D<float4>			_Target2 : register( u2 );

StructuredBuffer<float3>	_Rays : register( t1 );

// Computes the visibility of the ray at the given position
float	ComputeVisibility( float2 _UV, float3 _Direction ) {
	float	phi = atan2( _Direction.y, _Direction.x );	// [-PI,PI]
	float	W = phi / (2.0 * PI);						// [-0.5,+0.5]
	float	cosTheta = _SourceVisibility.SampleLevel( LinearWrap, float3( _UV, W ), 0.0 );
	float	cosThetaMin = 0.9 * cosTheta;
	float	cosThetaMax = 1.1 * cosTheta;
	return smoothstep( cosThetaMax, cosThetaMin, _Direction.z );
}

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelPosition = _DispatchThreadID.xy;
	if ( PixelPosition.x >= _Width || PixelPosition.y >= _Height )
		return;

	float2	pixel2UV = 1.0 / float2( _Width, _Height );
	float2	UV = pixel2UV * float2( 0.5 + PixelPosition );

	uint	RayIndex = _GroupThreadID.x;

	uint	K = 2*_KernelSize;

	// Compute general parameters (TODO: move to constant buffer)
	float	sigma_s = (1.0 - _g) * _Sigma_s;				// Reduced scattering coefficient
	float	sigma_t = _Sigma_a + sigma_s;					// Reduced extinction coefficient
	float	alpha = sigma_s / sigma_t;						// Reduced albedo
	float	l = 1.0 / sigma_t;								// Mean free path
	float	D = l / 3.0;

	float3	Result = 0.0;
	uint2	Pos;
	for ( uint Y=0; Y <= K; Y++ ) {
		Pos.y = PixelPosition.y - _KernelSize + Y;
		if ( Pos.y > _Height )
			continue;

		for ( uint X=0; X <= K; X++ ) {
			Pos.x = PixelPosition.x - _KernelSize + X;
			if ( Pos.x > _Width )
				continue;

			// Fetch local thickness
			float	d = _Thickness_mm * _SourceThickness.Load( int3( Pos, 0 ) ).x;

			// Fetch local diffuse albedo and compute average diffuse reflectance
			float3	albedo = _SourceAlbedo.Load( int3( PixelPosition, 0 ) ).xyz;
			float	rho_d = dot( LUMINANCE, albedo );								// Luminance = average albedo reflectance
			float3	rho_t = 1.0 - albedo;

			// Fetch local normal
			float3	normal = 2.0 * _SourceNormal.SampleLevel( LinearWrap, UV, 0 ).xyz - 1.0;

			// Fetch local transmittance
			float3	transmittance = _SourceTransmittance.SampleLevel( LinearWrap, UV, 0 ).xyz;
			float3	sigma_tr = sqrt( 3.0 * _Sigma_a * sigma_t * transmittance );	// Effective transport coefficient (in mm^-1)

			// Compute local parameters
			float	A = (1.0 + rho_d) / (1.0 - rho_d);								// Change of fluence due to internal reflection at the surface
			float	Zb = 2.0 * A * D;

			// Compute transmittance (accumulate dipoles contribution)
			float3	Tr = 0.0;
			for ( int DipoleIndex=-DIPOLES_COUNT; DipoleIndex <= DIPOLES_COUNT; DipoleIndex++ ) {
				float	Zr = 2.0 * DipoleIndex * (d + 2.0 * Zb) + l;
				float	Zv = Zr - l - 2.0 * Zb;

				float	Dr = length( float3( _TexelSize_mm * Pos, Zr ) );			// in mm
				float	Dv = length( float3( _TexelSize_mm * Pos, Zv ) );			// in mm

				float3	pole_r = (d - Zr) * (1.0 + sigma_tr * Dr * exp( -sigma_tr * Dr )) / (Dr * Dr * Dr);	// Real
				float3	pole_v = (d - Zv) * (1.0 + sigma_tr * Dv * exp( -sigma_tr * Dv )) / (Dv * Dv * Dv);	// Virtual
				Tr += pole_r + pole_v;
			}
			Tr *= alpha / (4.0 * PI);

			// Compute irradiance for our light directions
			float2	CurrentUV = pixel2UV * float2( 0.5 + Pos );
			float	visibility = ComputeVisibility( CurrentUV, _Light );
			float3	E = rho_t * visibility * saturate( normal, _Light );

			// Accumulate
			Result += E * Tr;
		}
	}

	_Target[PixelPosition] = float4( Result, 0 );
}