#include "Global.hlsl"

#define	USE_PHONG	1
//#define	USE_GGX		1

cbuffer CB_Render : register(b10) {
	float3	_Direction;
	float	_Intensity;
	float3	_ReflectedDirection;
	uint	_ScatteringOrder;
	uint	_Flags;

	// Analytical Beckmann lobe
	float	_Roughness;
	float	_ScaleR;		// Scale factor along reflected ray
	float	_ScaleT;		// Scale factor along tangential axis
	float	_ScaleB;		// Scale factor along bi-tangential axis
}

Texture2DArray< float >		_Tex_DirectionsHistogram : register( t2 );

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : TEXCOORDS0;
};

// From Walter 2007
// D(m) = exp( -tan( theta_m )² / a² ) / (PI * a² * cos(theta_m)^4)
float	BeckmannNDF( float _cosTheta_M, float _roughness ) {
	float	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
	float	a2 = _roughness*_roughness;
	return exp( -(1.0 - sqCosTheta_M) / (sqCosTheta_M * a2) ) / (PI * a2 * sqCosTheta_M*sqCosTheta_M);
}

// Masking G1(v,m) = 2 / (1 + erf( 1/(a * tan(theta_v)) ) + exp(-a²) / (a*sqrt(PI)))
float	BeckmannG1( float _cosTheta_V, float _roughness ) {
	float	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = 1.0 / (_roughness * tanThetaV);

	#if 1	// Simplified numeric version
		return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
	#else	// Analytical
		return 2.0 / (1.0 + erf( a ) + exp( -a*a ) / (a * SQRTPI));
	#endif
}

// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
float	GGXNDF( float _cosTheta_M, float _roughness ) {
	float	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
	float	a2 = _roughness*_roughness;
//	return a2 / (PI * sqCosTheta_M*sqCosTheta_M * pow2( a2 + (1.0-sqCosTheta_M)/sqCosTheta_M ));
	return a2 / (PI * pow2( sqCosTheta_M * (a2-1.0) + 1.0 ));
}

// Masking G1(v,m) = 2 / (1 + sqrt( 1 + a² * tan(theta_v)² ))
// Simplified into G1(v,m) = 2*cos(theta_v) / (1 + sqrt( cos(theta_v)² * (1-a²) + a² ))
float	GGXG1( float _cosTheta_V, float _roughness ) {
	float	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
	float	a2 = _roughness*_roughness;
	return 2.0 * _cosTheta_V / (1.0 + sqrt( sqCosTheta_V * (1.0 - a2) + a2 ));
}

float	Roughness2PhongExponent( float _roughness ) {
	return exp2( 10.0 * (1.0 - _roughness) + 1.0 );	// From https://seblagarde.wordpress.com/2011/08/17/hello-world/
}

// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
float	PhongNDF( float _cosTheta_M, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	return (n+2)*pow( _cosTheta_M, n ) / PI;
}

// Same as Beckmann but with a modified a bit
float	PhongG1( float _cosTheta_V, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	float	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = sqrt( 1.0 + 0.5 * n) / tanThetaV;
	return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
}


PS_IN	VS( VS_IN _In ) {

	float3	wsIncomingDirection = -_Direction;			// Actual INCOMING ray direction pointing AWAY from the surface (hence the - sign)
	float3	wsReflectedDirection = _ReflectedDirection;	// Actual REFLECTED ray direction
	float3	wsTangent, wsBiTangent;
//	BuildOrthonormalBasis( wsReflectedDirection, wsTangent, wsBiTangent );
	wsTangent = normalize( float3( 1e-10 + wsReflectedDirection.y, -wsReflectedDirection.x, 0 ) );	// Always lying in the X^Y plane
	wsBiTangent = cross( wsReflectedDirection, wsTangent );

	float3	lsPosition = float3( _In.Position.x, -_In.Position.z, _In.Position.y );	// Vertex position in Z-up, in local "reflected direction space"

	float	lobeIntensity;
	float3	wsPosition;
	if ( _Flags & 2 ) {
		// Show analytical lobe
		float3	wsScaledDirection = _ScaleT * lsPosition.x * wsTangent + _ScaleB * lsPosition.y * wsBiTangent + _ScaleR * lsPosition.z * wsReflectedDirection;	// World space direction, aligned with reflected ray
		float3	wsDirection = normalize( wsScaledDirection );

		float	cosTheta_M = saturate( dot( wsDirection, wsReflectedDirection ) );	// Theta_M = angle between reflected direction and the lobe's current direction
																					// (we simply made the lobe BEND toward the reflected direction, as if it was the new surface's normal)

		switch ( (_Flags >> 2) ) {
		case 2:
			// Phong
			lobeIntensity = PhongNDF( cosTheta_M, _Roughness );					// NDF
			lobeIntensity *= PhongG1( wsIncomingDirection.z, _Roughness );		// * Masking( incoming )
			lobeIntensity *= PhongG1( wsDirection.z, _Roughness );				// * Masking( outgoing )
			break;
		case 1:
			// GGX
			lobeIntensity = GGXNDF( cosTheta_M, _Roughness );					// NDF
			lobeIntensity *= GGXG1( wsIncomingDirection.z, _Roughness );		// * Masking( incoming )
			lobeIntensity *= GGXG1( wsDirection.z, _Roughness );				// * Masking( outgoing )
			break;
		default:
			// Beckmann
			lobeIntensity = BeckmannNDF( cosTheta_M, _Roughness );				// NDF
			lobeIntensity *= BeckmannG1( wsIncomingDirection.z, _Roughness );	// * Masking( incoming )
			lobeIntensity *= BeckmannG1( wsDirection.z, _Roughness );			// * Masking( outgoing )
			break;
		}

//		float	IOR = Fresnel_IORFromF0( 0.04 );
//		lobeIntensity *= FresnelAccurate( IOR, lsDirection.y ).x;

		lobeIntensity *= wsDirection.z < 0.0 ? 0.0 : 1.0;		// Nullify all "below the surface" directions


lobeIntensity *= _Intensity;	// So we match the simulated lobe's intensity scale


		wsDirection = wsScaledDirection;
		wsPosition = lobeIntensity * float3( wsDirection.x, wsDirection.z, -wsDirection.y );	// Vertex position in Y-up

	} else {
		// Show simulated lobe
		float3	wsDirection = lsPosition.x * wsTangent + lsPosition.y * wsBiTangent + lsPosition.z * wsReflectedDirection;	// Direction in world space, aligned with reflected ray

		float	cosTheta_M = wsDirection.z;
		float	theta = acos( clamp( cosTheta_M, -1.0, 1.0 ) );
		float	phi = fmod( 2.0 * PI + atan2( wsDirection.y, wsDirection.x ), 2.0 * PI );

		float	thetaBinIndex = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) );		// Inverse of 2*asin( sqrt( i / (2 * N) ) )
		float2	UV = float2( phi / (2.0 * PI), thetaBinIndex / LOBES_COUNT_THETA );
//		float	lobeIntensity = _Tex_DirectionsHistogram.SampleLevel( PointClamp, float3( UV, _ScatteringOrder ), 0.0 );
		lobeIntensity = _Tex_DirectionsHistogram.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 );
		lobeIntensity *= LOBES_COUNT_THETA * LOBES_COUNT_PHI;	// Re-scale due to lobe's discretization
		lobeIntensity *= _Intensity;							// Manual intensity scale

		lobeIntensity = max( 0.01, lobeIntensity );				// So we always at least see something
		lobeIntensity *= wsDirection.z < 0.0 ? 0.0 : 1.0;		// Nullify all "below the surface" directions

		wsPosition = lobeIntensity * float3( wsDirection.x, wsDirection.z, -wsDirection.y );	// Vertex position in Y-up
	}

	PS_IN	Out;
	Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );
	Out.Color = 0.1 * lobeIntensity;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	if ( _Flags & 2 )
		return _Flags & 1 ? float3( 0, 0.1, 0 ) : _In.Color * float3( 0.5, 1.0, 0.5 );
	else
		return _Flags & 1 ? float3( 0.1, 0, 0 ) : _In.Color;
}
