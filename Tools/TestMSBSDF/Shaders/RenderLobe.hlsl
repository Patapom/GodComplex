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

// Same as Beckmann but with modified a bit
float	PhongG1( float _cosTheta_V, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	float	sqCosTheta_V = _cosTheta_V * _cosTheta_V;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = sqrt( 1.0 + 0.5 * n) / tanThetaV;
	return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
}


PS_IN	VS( VS_IN _In ) {

	float3	wsIncomingDirection = -float3( _Direction.x, _Direction.z, -_Direction.y );								// Actual INCOMING ray direction in Y-up pointing AWAY from the surface (hence the - sign)
	float3	wsReflectedDirection = float3( _ReflectedDirection.x, _ReflectedDirection.z, -_ReflectedDirection.y );	// Actual REFLECTED ray direction in Y-up
	float3	tangent, biTangent;
	BuildOrthonormalBasis( wsReflectedDirection, tangent, biTangent );

//	float3	lsDirection = _In.Position;												// Direction in our local object's Y-up space (which is also our world space BTW)
	float3	lsDirection = _In.Position.x * tangent + _In.Position.y * wsReflectedDirection + _In.Position.z * biTangent;	// Direction, aligned with reflected ray

	float3	mfDirection = float3( lsDirection.x, -lsDirection.z, lsDirection.y );	// Direction in µ-facet Z-up space

	float	theta = acos( clamp( mfDirection.z, -1.0, 1.0 ) );
	float	phi = fmod( 2.0 * PI + atan2( mfDirection.y, mfDirection.x ), 2.0 * PI );

	float	lobeIntensity;
	float3	lsPosition;
	if ( _Flags & 2 ) {
		// Show analytical Beckmann lobe
		float	cosTheta_M = mfDirection.z;
		cosTheta_M = saturate( dot( lsDirection, wsReflectedDirection ) );	// Theta_M = angle between reflected direction and the lobe's vertex direction (we simply made the lobe BEND toward the reflected direction, as if it was the new surface's normal)

		switch ( (_Flags >> 2) ) {
		case 2:
			// Phong
			lobeIntensity = PhongNDF( cosTheta_M, _Roughness );					// NDF
			lobeIntensity *= PhongG1( wsIncomingDirection.y, _Roughness );		// * Masking( incoming )
			lobeIntensity *= PhongG1( lsDirection.y, _Roughness );				// * Masking( outgoing )
			break;
		case 1:
			// GGX
			lobeIntensity = GGXNDF( cosTheta_M, _Roughness );					// NDF
			lobeIntensity *= GGXG1( wsIncomingDirection.y, _Roughness );		// * Masking( incoming )
			lobeIntensity *= GGXG1( lsDirection.y, _Roughness );				// * Masking( outgoing )
			break;
		default:
			// Beckmann
			lobeIntensity = BeckmannNDF( cosTheta_M, _Roughness );				// NDF
			lobeIntensity *= BeckmannG1( wsIncomingDirection.y, _Roughness );	// * Masking( incoming )
			lobeIntensity *= BeckmannG1( lsDirection.y, _Roughness );			// * Masking( outgoing )
			break;
		}

		float	IOR = Fresnel_IORFromF0( 0.04 );
		lobeIntensity *= FresnelAccurate( IOR, lsDirection.y ).x;

//lobeIntensity *= cosTheta_M;

		lobeIntensity *= 10.0 * _ScaleR;
		lobeIntensity *= lsDirection.y < 0.0 ? 0.0 : 1.0;		// Nullify all "below the surface" directions

		lsPosition = lobeIntensity * lsDirection;

		// Tangential scale
		float3	lsTangent = wsReflectedDirection.y < 0.9999 ? normalize( cross( wsReflectedDirection, float3( 0, 1, 0 ) ) ) : float3( 1, 0, 0 );
		float	L = dot( lsPosition, lsTangent );
		lsPosition = lsPosition + L * (_ScaleT - 1.0) * lsTangent;

	} else {
		// Show simulated lobe
		float	thetaBinIndex = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) );		// Inverse of 2*asin( sqrt( i / (2 * N) ) )
		float2	UV = float2( phi / (2.0 * PI), thetaBinIndex / LOBES_COUNT_THETA );
//		float	lobeIntensity = _Tex_DirectionsHistogram.SampleLevel( PointClamp, float3( UV, _ScatteringOrder ), 0.0 );
		lobeIntensity = _Tex_DirectionsHistogram.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 );
		lobeIntensity *= LOBES_COUNT_THETA * LOBES_COUNT_PHI;	// Re-scale due to lobe's discretization
		lobeIntensity *= _Intensity;							// Manual intensity scale

		lobeIntensity = max( 0.01, lobeIntensity );				// So we always at least see something
		lobeIntensity *= lsDirection.y < 0.0 ? 0.0 : 1.0;		// Nullify all "below the surface" directions

		lsPosition = lobeIntensity * lsDirection;
	}

	PS_IN	Out;
	Out.__Position = mul( float4( lsPosition, 1.0 ), _World2Proj );
	Out.Color = 0.1 * lobeIntensity;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	if ( _Flags & 2 )
		return _Flags & 1 ? float3( 0, 0.1, 0 ) : _In.Color * float3( 0.5, 1.0, 0.5 );
	else
		return _Flags & 1 ? float3( 0.1, 0, 0 ) : _In.Color;
}
