#include "Global.hlsl"

static const uint	TABLE_SIZE = 64;

RWTexture2D< float2 >	_TexBRDFIntegral : register(u0);

float	Pow2( float a ) { return a * a; }
float	Pow4( float a ) { return a * a * a * a; }

#if 1//IMPORTANCE_SAMPLING

static const uint	SAMPLES_COUNT = 2048;

// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
float ReverseBits( uint bits ) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelIndex = _DispatchThreadID.xy;
	float	CosThetaV = float(PixelIndex.x) / TABLE_SIZE;
	float	SinThetaV = sqrt( 1.0 - CosThetaV*CosThetaV );
	float	Roughness = max( 0.01, float(PixelIndex.y) / TABLE_SIZE );

	float3	View = float3( sqrt( 1.0 - CosThetaV*CosThetaV ), CosThetaV, 0.0 );
	float	PhiV = 0.0;	// Aligned on X^Y plane

	float2	Sum = 0.0;

	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {

		float	X0 = float(i) / SAMPLES_COUNT;
		float	X1 = ReverseBits( i );

// Debug Hammersley
// 		uint	x = uint(X0 * TABLE_SIZE);
// 		uint	y = uint(X1 * TABLE_SIZE);
// 		if ( x==PixelIndex.x && y==PixelIndex.y )
// 			Sum = 1.0;
//		continue;


		float	PhiH = 2.0 * PI * X0;

		// Ward importance sampling
		float	ThetaH = atan( -Roughness * Roughness * log( 1.0 - X1 ) );
		float	CosThetaH = cos( ThetaH );
		float	SinThetaH = sin( ThetaH );

		float	CosPhiH = cos( PhiH );
		float	SinPhiH = sin( PhiH );

		float3	Half = float3(	SinPhiH * SinThetaH,
								CosThetaH,
								CosPhiH * SinThetaH );

 		float3	Light = 2.0 * dot( View, Half ) * Half - View;	// Light is on the other size of the Half vector...

		if ( Light.y <= 0.0 )
			continue;

		float	HoN = Half.y;
		float	HoN2 = HoN*HoN;
		float	HoV = dot( Half, View );
//		float	HoV = Half.x * View.x + Half.y * View.y;	// We know that Z=0 here...
//		float	HoL = dot( Half, Light );
// 		float	NoL = Light.y;
// 		float	NoV = View.y;

 		// Apply sampling weight for correct distribution
 		float	SampleWeight = 2.0 / (1.0 + View.y / Light.y);
// 		float	BRDF = SampleWeight * Light.y;
 		float	BRDF = SampleWeight;		// Don't N.L since it's already accounted for in the pre-integrated irradiance!

		// Compute Fresnel terms
		float	Schlick = 1.0 - HoV;
		float	Schlick5 = Schlick * Schlick;
				Schlick5 *= Schlick5 * Schlick;

		float2	Fresnel = float2( 1.0f - Schlick5, Schlick5 );
//		float2	Fresnel = float2( 1.0f, Schlick5 );

		Sum += Fresnel * BRDF;
	}

	Sum /= SAMPLES_COUNT;
	_TexBRDFIntegral[PixelIndex] = Sum;
}

#else

static const uint	SAMPLES_THETA = 256;					// Integrating from [0,PI/2]
static const uint	SAMPLES_PHI = 2*SAMPLES_THETA;	// Integrating from [0,PI] (only an hemisphere since it's symmetric)

static const float	dPhi = 2.0 * PI / SAMPLES_PHI;
static const float	dTheta = 0.5 * PI / SAMPLES_THETA;

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelIndex = _DispatchThreadID.xy;
	float	CosThetaV = float(PixelIndex.x) / TABLE_SIZE;
	float	SinThetaV = sqrt( 1.0 - CosThetaV*CosThetaV );
	float	Roughness = max( 0.01, float(PixelIndex.y) / TABLE_SIZE );

	float3	View = float3( sqrt( 1.0 - CosThetaV*CosThetaV ), CosThetaV, 0.0 );
	float	PhiV = 0.0;	// Aligned on X^Y plane

	float2	Sum = 0.0;

#if 1
	[loop]
	[fastopt]
	for ( uint ThetaIndex=0; ThetaIndex < SAMPLES_THETA; ThetaIndex++ ) {
		float	ThetaL = 0.5*PI * (0.5+ThetaIndex) / SAMPLES_THETA;
		float2	SCThetaL;
		sincos( ThetaL, SCThetaL.x, SCThetaL.y );

//		float	SolidAngle = SCThetaL.y * SCThetaL.x * dTheta * dPhi;	// (N.L) sin(Theta) dTheta dPhi
		float	SolidAngle = SCThetaL.x * dTheta * dPhi;	// sin(Theta) dTheta dPhi   (don't N.L since it's already accounted for in the pre-integrated irradiance!)

		[loop]
		[fastopt]
		for ( uint PhiIndex=0; PhiIndex < SAMPLES_PHI; PhiIndex++ ) {
			float	PhiL = PI * PhiIndex / SAMPLES_PHI;

			// Build light and half vectors
			float3	Light = float3(	cos(PhiL) * SCThetaL.x,
									SCThetaL.y,
									sin(PhiL) * SCThetaL.x );

			float3	Half = normalize( View + Light );

			// Expensive Ward
			float	tanDelta = tan( acos( Half.y ) );
			float	BRDF = exp( -Pow2( tanDelta / Roughness ) ) * 2.0 * (1.0 + SCThetaL.y*CosThetaV + SCThetaL.x*SinThetaV*cos( PhiV - PhiL )) / Pow4( SCThetaL.y + CosThetaV );

			// Compute Fresnel terms
			float	VoH = View.x * Half.x + View.y * Half.y;
			float	Schlick = 1.0 - VoH;
			float	Schlick5 = Schlick * Schlick;
					Schlick5 *= Schlick5 * Schlick;

			float2	Fresnel = float2( 1.0 - Schlick5, Schlick5 );
//			float2	Fresnel = float2( 1.0, Schlick5 );

			Sum += Fresnel * BRDF * SolidAngle;
		}
	}

	Sum /= PI * Pow2( Roughness );

#else

// This version integrates the "true" BRDF
// The formula comes from eq. (15) from http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.169.9908&rep=rep1&type=pdf
//
	float2	UV = float2( PixelIndex ) / TABLE_SIZE;

	float3	ToView = float3( SinThetaV, 0, CosThetaV );

	const int	THETA_COUNT = 64; // * $alwaysOne; // warning X4008: floating point division by zero
	const float	dTheta = 0.5 * PI / THETA_COUNT;
	const float	dPhi = 2.0 * PI / THETA_COUNT;
	for ( int i=0; i < THETA_COUNT; i++ )
	{
		float	ThetaL = 0.5*PI * (0.5 + i) / THETA_COUNT;
		float	CosThetaL = cos( ThetaL );
		float	SinThetaL = sin( ThetaL );

		float	SolidAngle = CosThetaL * SinThetaL * dTheta * dPhi;	// (N.L) sin(Theta) dTheta dPhi

		for ( int j=0; j < THETA_COUNT; j++ )
		{
			float	PhiL = PI * j / THETA_COUNT;

			float3	ToLight = float3( SinThetaL * cos( PhiL ), SinThetaL * sin( PhiL ), CosThetaL );

			float3	Half = normalize( ToLight + ToView );

			// "True" and expensive evaluation of the Ward BRDF
			float	CosDelta = Half.z;	// dot( Half, _wsNormal );
			float	delta = acos( CosDelta );

			float	BRDF = exp( -Pow2( tan( delta ) / Roughness ) ) * 2.0 * (1.0 + CosThetaL*CosThetaV + SinThetaL*SinThetaV*cos( PhiV - PhiL )) / Pow4( CosThetaL + CosThetaV );

			// Compute Fresnel terms
			float	VoH = dot( ToView, Half );
			float	Schlick = 1.0 - VoH;
			float	Schlick5 = Schlick * Schlick;
					Schlick5 *= Schlick5 * Schlick;

			float2	Fresnel = float2( 1.0 - Schlick5, Schlick5 );

			Sum += Fresnel * BRDF * SolidAngle;//CosThetaL * SinThetaL * dTheta * dPhi;
		}
	}

	Sum /= PI * Pow2(Roughness);	// Since we forgot that in the main loop

#endif


//Sum.y *= 4.0;


	_TexBRDFIntegral[PixelIndex] = Sum;
}

#endif	// IMPORTANCE_SAMPLING