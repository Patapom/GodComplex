#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
	float3	_Direction;
	float	_Intensity;
	float3	_ReflectedDirection;
	uint	_ScatteringOrder;
	uint	_Flags;

	// Analytical Beckmann lobe
	float	_Roughness;
	float	_ScaleR;			// Scale factor along reflected ray
	float	_ScaleT;			// Scale factor along tangential axis
	float	_ScaleB;			// Scale factor along bi-tangential axis
	float	_MaskingImportance;	// Importance of the masking function
}

Texture2DArray< float >		_Tex_DirectionsHistogram_Reflected : register( t3 );
Texture2DArray< float >		_Tex_DirectionsHistogram_Transmitted : register( t4 );

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
float	BeckmannG1( float _cosTheta, float _roughness ) {
	float	sqCosTheta_V = _cosTheta * _cosTheta;
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
float	GGXG1( float _cosTheta, float _roughness ) {
	float	sqCosTheta_V = _cosTheta * _cosTheta;
	float	a2 = _roughness*_roughness;
	return 2.0 * _cosTheta / (1.0 + sqrt( sqCosTheta_V * (1.0 - a2) + a2 ));
}

float	Roughness2PhongExponent( float _roughness ) {
//	return exp2( 10.0 * (1.0 - _roughness) + 1.0 );	// From https://seblagarde.wordpress.com/2011/08/17/hello-world/
	return exp2( 10.0 * (1.0 - _roughness) + 0.0 ) - 1.0;	// Actually, we'd like some fatter rough lobes
}

// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
float	PhongNDF( float _cosTheta_M, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	return (n+2)*pow( _cosTheta_M, n ) / PI;
}

// Same as Beckmann but modified a bit
float	PhongG1( float _cosTheta, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	float	sqCosTheta_V = _cosTheta * _cosTheta;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = sqrt( 1.0 + 0.5 * n ) / tanThetaV;
	return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
}

// Finally, our long awaited diffuse lobe for 2nd scattering order
//  
// After fitting each parameter one after another, we noticed that:
// 	\[Bullet] Incident light angle \[Theta] has no effect on fitted lobe, assuming we ignore the backscattering that is visible at highly grazing angles and that would be better fitted using maybe a GGX lobe that features a nice backscatter property.
// 	\[Bullet] Final masking importance m is 0 after all
// 	\[Bullet] There is only a dependency on albedo \[Rho] for the scale factor (that was expected) and it is proportional to \[Rho]^2 which was also expected.
// 	
// Finally, we obtain the following analytical model for 2nd order scattering of a rough diffuse surface:
// 
// 	f(Subscript[\[Omega], o],\[Alpha],\[Rho]) = \[Sigma](\[Rho]) \[Mu]^\[Eta](\[Alpha])
// 	
// 	\[Mu] = Subscript[\[Omega], o]\[CenterDot]Z
// 	\[Sigma](\[Alpha], \[Rho]) = k(\[Rho]) [0.587595 +0.128391 (1-\[Alpha])+0.320232 (1-\[Alpha])^2-1.04001 (1-\[Alpha])^3]	the fitted scale factor with a dependency on albedo and roughness
// 	\[Eta](\[Alpha]) = 0.7782894918463 + 0.1683172467667511 \[Alpha]						the fitted exponent with a dependency on roughness alone
// 	k(\[Rho]) = 1-2(1-\[Rho])+(1-\[Rho])^2										the factor applied to scale depending on \[Rho] and, most importantly, \[Rho]^2, that will give use the expected color saturation
// 	
// The flattening factor along the main lobe direction Z is the most expensive to compute:
// 	a(\[Alpha]) = 0.697462  - 0.479278 (1-\[Alpha])
// 	b(\[Alpha]) = 0.287646  - 0.293594 (1-\[Alpha])
// 	c(\[Alpha]) = 5.69744  + 6.61321 (1-\[Alpha])
// 	Subscript[\[Sigma], n](\[Mu], \[Alpha]) = a(\[Alpha]) + b(\[Alpha]) e^(-c(\[Alpha])  \[Mu])
// 
// An alternate model is possible using a power of 2:
// 	c^\[Prime](\[Alpha]) = 8.21968  + 9.54087 (1-\[Alpha])
// 	Subscript[\[Sigma], n]^\[Prime](\[Mu], \[Alpha]) = a(\[Alpha]) + b(\[Alpha]) 2^(-c^\[Prime](\[Alpha])  \[Mu])
// 	
// So the world-space intensity of the fitted lobe is obtained by multiplying the lobe-space intensity with the scale factor:
// 
// 	Subscript[f, w](Subscript[\[Omega], o],\[Alpha],\[Rho]) = L(\[Mu],Subscript[\[Sigma], n](\[Mu], \[Alpha])) f(Subscript[\[Omega], o],\[Alpha],\[Rho])
// 	
// 	L(\[Mu], Subscript[\[Sigma], n](\[Mu], \[Alpha])) = 1/Sqrt[1+\[Mu]^2 (1/Subscript[\[Sigma], n](\[Mu],\[Alpha])^2-1)]
// 	
//
float	ComputeDiffuseModel( float3 _wsOutgoingDirection, float _roughness, float _albedo ) {
	_albedo = 1.0 - _albedo;
	float	gloss = 1.0 - _roughness;

	float	cosTheta = saturate( _wsOutgoingDirection.z );

	// Compute sigma, the global scale factor
	float	k = 1.0 + _albedo * (-2.0 + _albedo);
	float	sigma = 0.587595 + gloss * (0.128391 + gloss * (0.320232 - 1.04001 * gloss));
			sigma *= k;	// Dependence on albedo²

	// Compute lobe exponent
	float	eta = 0.7782894918463 + 0.1683172467667511 * _roughness;

	// Compute unscaled lobe intensity
	float	intensity = sigma * pow( cosTheta, eta );

	// Compute flattening
	float3	abc = float3(	0.697462 - 0.479278 * gloss,
							0.287646 - 0.293594 * gloss,
							8.219680 + 9.540870 * gloss );
	float	sigma_n = abc.x + abc.y * exp2( -abc.z * cosTheta );
	float	L = rsqrt( 1.0 + cosTheta*cosTheta * (1.0 / pow2( sigma_n ) - 1.0)  );

	return  L * intensity;
}



PS_IN	VS( VS_IN _In ) {

	float3	wsIncomingDirection = -_Direction;			// Actual INCOMING ray direction pointing AWAY from the surface (hence the - sign)
	float3	wsReflectedDirection = _ReflectedDirection;	// Actual REFLECTED ray direction (or REFRACTED when rendering bottom lobes)
	float3	wsTangent, wsBiTangent;
//	BuildOrthonormalBasis( wsReflectedDirection, wsTangent, wsBiTangent );
	wsTangent = normalize( float3( 1e-10 + wsReflectedDirection.y, -wsReflectedDirection.x, 0 ) );	// Always lying in the X^Y plane
	wsBiTangent = cross( wsTangent, wsReflectedDirection );

	float3	lsPosition = float3( _In.Position.x, -_In.Position.z, _In.Position.y );	// Vertex position in Z-up, in local "reflected direction space"

	float	lobeSign = _Flags & 4U ? -1.0 : 1.0;	// -1 for transmitted lobe, +1 for reflected lobe

	float	intensityMultiplier = _Intensity * (_Flags & 8 ? pow( 3.0, _ScatteringOrder ) : 1.0);

	float	lobeIntensity;
	float3	wsPosition;
	if ( _Flags & 2 ) {
		// Show analytical lobe
//		float3	wsScaledDirection = _ScaleT * lsPosition.x * wsTangent + _ScaleB * lsPosition.y * wsBiTangent + _ScaleR * lsPosition.z * wsReflectedDirection;	// World space direction, aligned with reflected ray
		float3	wsScaledDirection = lsPosition.x * wsTangent + lsPosition.y * wsBiTangent + _ScaleR * lsPosition.z * wsReflectedDirection;	// World space direction, aligned with reflected ray
		float3	wsDirection = normalize( wsScaledDirection );

		float	cosTheta_M = saturate( dot( wsDirection, wsReflectedDirection ) );	// Theta_M = angle between reflected direction and the lobe's current direction
																					// (we simply made the lobe BEND toward the reflected direction, as if it was the new surface's normal)

//cosTheta_M = saturate( _ScaleR * lsPosition.z / sqrt( pow2( _ScaleT * lsPosition.x ) + pow2( _ScaleB * lsPosition.y ) + pow2( _ScaleR * lsPosition.z ) ) );
//cosTheta_M = saturate( _ScaleR * lsPosition.z / sqrt( 1.0 + (_ScaleR*_ScaleR - 1) * lsPosition.z*lsPosition.z ) );

		float	maskingShadowing = 1.0;
		switch ( (_Flags >> 4) ) {
		case 2:
			// Phong
			lobeIntensity = PhongNDF( cosTheta_M, _Roughness );					// NDF
			maskingShadowing = PhongG1( wsIncomingDirection.z, _Roughness );	// * Masking( incoming )
			maskingShadowing *= PhongG1( wsDirection.z, _Roughness );			// * Masking( outgoing )
			break;
		case 1:
			// GGX
			lobeIntensity = GGXNDF( cosTheta_M, _Roughness );					// NDF
			maskingShadowing = GGXG1( wsIncomingDirection.z, _Roughness );		// * Masking( incoming )
			maskingShadowing *= GGXG1( wsDirection.z, _Roughness );				// * Masking( outgoing )
			break;
		case 3:
			// Diffuse Lobe Model
			wsDirection = lsPosition;// normalize( lsPosition.x * wsTangent + lsPosition.y * wsBiTangent + lsPosition.z * wsReflectedDirection );	// No scaling for that model
			wsScaledDirection = wsDirection;
			lobeIntensity = ComputeDiffuseModel( wsDirection, _Roughness, _ScaleR );	// _ScaleR is the surface's albedo in this case
			maskingShadowing = 1.0;	// No masking/shadowing
			break;
		default:
			// Beckmann
			lobeIntensity = BeckmannNDF( cosTheta_M, _Roughness );				// NDF
			maskingShadowing = BeckmannG1( wsIncomingDirection.z, _Roughness );	// * Masking( incoming )
			maskingShadowing *= BeckmannG1( wsDirection.z, _Roughness );		// * Masking( outgoing )
			break;
		}

		lobeIntensity *= lerp( 1.0, maskingShadowing, _MaskingImportance );

		lobeIntensity *= _ScaleT;	// Now used as "global scale factor"


//		float	IOR = Fresnel_IORFromF0( 0.04 );
//		lobeIntensity *= FresnelAccurate( IOR, lsDirection.y ).x;

		lobeIntensity *= (lobeSign * wsDirection.z) < 0.0 ? 0.0 : 1.0;		// Nullify all "below the surface" directions

		lobeIntensity *= intensityMultiplier;	// So we match the simulated lobe's intensity scale

		wsDirection = wsScaledDirection;
		wsPosition = lobeIntensity * float3( wsDirection.x, wsDirection.z, -wsDirection.y );	// Vertex position in Y-up

	} else {
		// Show simulated lobe
		float3	wsDirection = lsPosition.x * wsTangent + lsPosition.y * wsBiTangent + lsPosition.z * wsReflectedDirection;	// Direction in world space, aligned with reflected ray

		float	cosTheta_M = lobeSign * wsDirection.z;
		float	theta = acos( clamp( cosTheta_M, -1.0, 1.0 ) );
		float	phi = fmod( 2.0 * PI + atan2( wsDirection.y, wsDirection.x ), 2.0 * PI );

		float	thetaBinIndex = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) );		// Inverse of 2*asin( sqrt( i / (2 * N) ) )
		float2	UV = float2( phi / (2.0 * PI), thetaBinIndex / LOBES_COUNT_THETA );

//		lobeIntensity = _Tex_DirectionsHistogram_Reflected.SampleLevel( PointClamp, float3( UV, _ScatteringOrder ), 0.0 );
		lobeIntensity = (_Flags & 4U) ? _Tex_DirectionsHistogram_Transmitted.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 )
									  : _Tex_DirectionsHistogram_Reflected.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 );

		lobeIntensity *= LOBES_COUNT_THETA * LOBES_COUNT_PHI;	// Re-scale due to lobe's discretization

//lobeIntensity = 100000 * _Tex_DirectionsHistogram_Transmitted.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 );

//lobeIntensity = 1.0;

		lobeIntensity *= intensityMultiplier;				// Manual intensity scale
		lobeIntensity = max( 0.01, lobeIntensity );			// So we always at least see something

		lobeIntensity *= (lobeSign * wsDirection.z) < 0.0 ? 0.0 : 1.0;		// Nullify all "below the surface" directions

		wsPosition = lobeIntensity * float3( wsDirection.x, wsDirection.z, -wsDirection.y );	// Vertex position in Y-up
	}

	PS_IN	Out;
	Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );
	Out.Color = 0.1 * lobeIntensity;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float	solidAlpha = 0.5;
	float	wireframeAlpha = 0.1;
	bool	isWireframe = _Flags & 1U;
	bool	isAnalytical = _Flags & 2U;
	bool	isTransmittedLobe = _Flags & 4U;

	float4	simulatedLobeColor = float4( _In.Color, solidAlpha );
	float4	analyticalLobeColor = isTransmittedLobe ? float4( _In.Color * float3( 0.5, 0.5, 1.0 ), solidAlpha )
													: float4( _In.Color * float3( 0.5, 1.0, 0.5 ), solidAlpha );
	float4	simulatedLobeWireColor = isTransmittedLobe	? float4( 0, 0, 0.1, wireframeAlpha )
														: float4( 0.1, 0, 0, wireframeAlpha );
	float4	analyticalLobeWireColor = isTransmittedLobe	? float4( 0, 0, 0.1, wireframeAlpha )
														: float4( 0, 0.1, 0, wireframeAlpha );

	bool	isDiffuseModel = (_Flags >> 4) == 3U;
	if ( isDiffuseModel ) {
		analyticalLobeColor = float4( _In.Color * float3( 1.0, 1.0, 0.5 ), 0.9 );
		analyticalLobeWireColor = float4( 0.1, 0.1, 0, wireframeAlpha );
	}

	if ( isAnalytical )
		return isWireframe ? analyticalLobeWireColor : analyticalLobeColor;
	else
		return isWireframe ? simulatedLobeWireColor : simulatedLobeColor;
}
