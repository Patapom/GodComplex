#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
	float3	_Direction;
	float	_Intensity;
	float3	_ReflectedDirection;
	uint	_ScatteringOrder;
	uint	_Flags;

	// Analytical Beckmann lobe
	float	_Roughness;
	float	_Flattening;		// Scale factor along main lobe direction for isotropic lobe model, or along orthogonal directions for anisotropic lobe model
	float	_Scale;		// Global scale factor for the entire lobe
	float	_ScaleB;			// Not used
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

// D(m) = (n+2)/(2*PI) * cos(theta_m)^n
float	PhongNDF( float _cosTheta_M, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	return (n+2)*pow( _cosTheta_M, n ) / (2.0*PI);
}

// Same as Beckmann but modified a bit
float	PhongG1( float _cosTheta, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	float	sqCosTheta_V = _cosTheta * _cosTheta;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = sqrt( 1.0 + 0.5 * n ) / tanThetaV;
	return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
}

// Finally, our long awaited diffuse lobe for 2nd and 3rd scattering orders!
//  
// Resulting Model
//	
// After fitting each parameter one after another, we noticed that:
// 	\[Bullet] Incident light angle \[Theta] has no effect on fitted lobe, assuming we ignore the backscattering that is visible at highly grazing angles and that would be better fitted using maybe a GGX lobe that features a nice backscatter property.
// 	\[Bullet] Final masking importance m is 0 after all
// 	\[Bullet] There is only a dependency on albedo \[Rho] for the scale factor (that was expected) and it is proportional to \[Rho]^2 which was also expected.
// 	
// Finally, we obtain the following analytical model for 2nd order scattering of a rough diffuse surface:
// 
// 	Subscript[f, 2](Subscript[\[Omega], o],\[Alpha],\[Rho]) = Subscript[\[Sigma], 2](\[Rho]) \[Mu]^\[Eta](\[Alpha])
// 	\[Mu] = Subscript[\[Omega], o]\[CenterDot]Z
// 	
// The exponent \[Eta] is given as a function of surface roughness by:
// 
// 	\[Eta](Subscript[\[Alpha], s]) = 2.5958 \[Alpha]-1.32697 \[Alpha]^2
// 	
// The scale factor \[Sigma] is given by:
// 
// 	\[Sigma](\[Mu],Subscript[\[Alpha], s], \[Rho]) =a(Subscript[\[Alpha], s]) + b(Subscript[\[Alpha], s])\[Mu] + c(Subscript[\[Alpha], s]) \[Mu]^2 + d(Subscript[\[Alpha], s]) (\[Mu]^3) 
// 	Subscript[\[Sigma], 2](\[Mu],Subscript[\[Alpha], s], \[Rho]) =(\[Rho]^2) \[Sigma](\[Mu],Subscript[\[Alpha], s], \[Rho]) 
// 
// 	a(Subscript[\[Alpha], s])= 0.0576265 -1.84307 \[Alpha]+13.2655 \[Alpha]^2-9.1914 \[Alpha]^3
// 	b(Subscript[\[Alpha], s])= -0.193265+14.4283 \[Alpha]-39.5737 \[Alpha]^2+22.0841 \[Alpha]^3
// 	c(Subscript[\[Alpha], s])= 0.218714 -21.5808 \[Alpha]+57.0161 \[Alpha]^2-31.3305 \[Alpha]^3
// 	d(Subscript[\[Alpha], s])=-0.0875285+10.4984 \[Alpha]-27.1654 \[Alpha]^2+14.6968 \[Alpha]^3
// 
// The flattening factor Subscript[\[Sigma], n] along the main lobe direction Z is given by:
// 
// 	Subscript[\[Sigma], n](\[Mu],Subscript[\[Alpha], s]) =a(Subscript[\[Alpha], s]) + b(Subscript[\[Alpha], s])\[Mu] + c(Subscript[\[Alpha], s]) \[Mu]^2 + d(Subscript[\[Alpha], s]) (\[Mu]^3) 
// 	
// 	a(\[Alpha])= 0.913643 -1.65548 \[Alpha]+1.39617 \[Alpha]^2-0.320331 \[Alpha]^3
// 	b(\[Alpha])= 0.0447239 +0.62474 \[Alpha]
// 	c(\[Alpha])= -0.118844-0.973213 \[Alpha]+0.36902 \[Alpha]^2
// 	d(\[Alpha])=0.132577 +0.16975 \[Alpha]
// 	
// So the world-space intensity of the fitted lobe is obtained by multiplying the lobe-space intensity with the scale factor:
// 
// 	Subscript[f, w](Subscript[\[Omega], o],\[Alpha],\[Rho]) = L(\[Mu],Subscript[\[Sigma], n](\[Mu], \[Alpha])) Subscript[f, 2](Subscript[\[Omega], o],\[Alpha],\[Rho])
// 	
// 	L(\[Mu], Subscript[\[Sigma], n](\[Mu], \[Alpha])) = 1/Sqrt[1+\[Mu]^2 (1/Subscript[\[Sigma], n](\[Mu],\[Alpha])^2-1)]
// 
// Additionally, the fitted lobe roughness \[Alpha] as a function of surface roughness Subscript[\[Alpha], s] is given by:
// 
// 	 \[Alpha](Subscript[\[Alpha], s])= 1-0.2687 \[Alpha]+0.153596 \[Alpha]^2
//
// We also fit order 3 using the order 2 scale curve and we obtained the following expression:
//	fScale3[ \[Mu]_, \[Alpha]_, \[Rho]_] = \[Rho]^3 * (0.363902052363025`* fScale[\[Mu], \[Alpha]]);
//
float3	ComputeDiffuseModel( float3 _wsIncomingDirection, float3 _wsOutgoingDirection, float _roughness, float3 _albedo ) {
#if 1

	float	cosTheta = saturate( _wsOutgoingDirection.z );

	float	mu = saturate( _wsIncomingDirection.z );
	float	mu2 = mu*mu;
	float	mu3 = mu*mu2;

	float	r = _roughness;
	float	r2 = r*r;
	float	r3 = r*r2;

	float4	abcd = float4(	 0.057626522306966195 - 1.8430749623241764 * r + 13.265452228771144 * r2 - 9.1914044613068    * r3,
							-0.19326518084394056  + 14.428287204401842 * r - 39.57369023420125  * r2 + 22.084117767595018 * r3,
							 0.21871385093630663  - 21.580810315041887 * r + 57.01607335273474  * r2 - 31.33051654652553  * r3,
							-0.08752850960291476  + 10.498392018375801 * r - 27.165414679434377 * r2 + 14.696817709205297 * r3
						);

	float3	sigma2 = abcd.x + abcd.y * mu + abcd.z * mu2 + abcd.w * mu3;
	float3	sigma3 = 0.363902052363025 * sigma2;
			sigma2 *= _albedo*_albedo;			// Dependence on albedo²
			sigma3 *= _albedo*_albedo*_albedo;	// Dependence on albedo^3


sigma2 *= _ScatteringOrder == 1 ? 1 : 0;
sigma3 *= _ScatteringOrder == 2 ? 1 : 0;


	// Compute lobe exponent
	float	eta = 2.588380909161985 * r - 1.3549594389004276 * r2;
//eta = Roughness2PhongExponent( 1 - 0.2687 * r + 0.153596 * r2 );

	// Compute unscaled lobe intensity
	float3	intensity = (sigma2 + sigma3) * (eta+2) * pow( cosTheta, eta ) / (2.0*PI);

	// Compute flattening factor
	abcd = float4(	   0.8850557867448499    - 1.2109761138443194 * r + 0.22569832413951335 * r2 + 0.4498256199595464 * r3,
					   0.0856807009397115    + 0.5659031384072539 * r,
					  -0.07707463071513312   - 1.384614678037336  * r + 0.8565888280926491  * r2,
					   0.010423083821992304  + 0.8525591060832015 * r - 0.6844738691665317  * r2
				);

	float	sigma_n = abcd.x + abcd.y * mu + abcd.z * mu2 + abcd.w * mu3;

	float	L = rsqrt( 1.0 + cosTheta*cosTheta * (1.0 / pow2( sigma_n ) - 1.0)  );

	return  L * intensity;

#else
	// Formerly, when we had bad histogram bins, we got this:
	float	gloss = 1.0 - _roughness;

	float	cosTheta = saturate( _wsOutgoingDirection.z );

	// Compute sigma, the global scale factor
	float3	sigma2 = _roughness * (2.324842464398671 + _roughness * (-2.7502131990906116 + _roughness * 1.012605093077086));
			sigma2 *= _albedo*_albedo;	// Dependence on albedo²

	float3	sigma3 = _roughness * (0.25262765805538445 + _roughness * (0.3902065605355212 - _roughness * 0.3820487315212411));
			sigma3 *= _albedo*_albedo*_albedo;	// Dependence on albedo^3

sigma2 *= _ScatteringOrder == 1 ? 1 : 0;
sigma3 *= _ScatteringOrder == 2 ? 1 : 0;

	// Compute lobe exponent
	float	eta = 0.7782894918463 + 0.1683172467667511 * _roughness;

	// Compute unscaled lobe intensity
	float3	intensity = (sigma2 + sigma3) * pow( cosTheta, eta );

	// Compute flattening
	float4	abcd = float4(	0.697462 - 0.479278 * gloss,
							0.287646 - 0.293594 * gloss,
							8.219680 + 9.540870 * gloss );
	float	sigma_n = abc.x + abc.y * exp2( -abc.z * cosTheta );
	float	L = rsqrt( 1.0 + cosTheta*cosTheta * (1.0 / pow2( sigma_n ) - 1.0)  );

	return  L * intensity;
#endif
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

	float	intensityMultiplier = _Intensity * (_Flags & 8U ? pow( 3.0, _ScatteringOrder ) : 1.0);

	float	lobeIntensity;
	float3	wsPosition;
	if ( _Flags & 2 ) {
		// Show analytical lobe
		float	scaleT = 1.0;
		float	scaleB = 1.0;
		float	scaleR = _Flattening;	// Isotropic with flattening along normal

		uint	lobeModel = _Flags >> 4;
		if ( lobeModel == 3U ) {
			// Anisotropic lobe model
			float	s = exp2( 4.0 * (_Flattening - 1.0) );	// From 2e-4 to 2e4
			scaleT = s;
			scaleB = 1.0 / s;
			scaleR = 1.0;
		}

		float3	wsScaledDirection = scaleT * lsPosition.x * wsTangent + scaleB * lsPosition.y * wsBiTangent + scaleR * lsPosition.z * wsReflectedDirection;	// World space direction, aligned with reflected ray

		float3	wsDirection = normalize( wsScaledDirection );

		float	cosTheta_M = saturate( dot( wsDirection, wsReflectedDirection ) );	// Theta_M = angle between reflected direction and the lobe's current direction
																					// (we simply made the lobe BEND toward the reflected direction, as if it was the new surface's normal)

		float	maskingShadowing = 1.0;
		switch ( lobeModel ) {
		case 1:
			// GGX
			lobeIntensity = GGXNDF( cosTheta_M, _Roughness );					// NDF
			maskingShadowing = GGXG1( wsIncomingDirection.z, _Roughness );		// * Masking( incoming )
			maskingShadowing *= GGXG1( wsDirection.z, _Roughness );				// * Masking( outgoing )
			break;
		case 2:
		case 3:
			// Phong
			lobeIntensity = PhongNDF( cosTheta_M, _Roughness );					// NDF
			maskingShadowing = PhongG1( wsIncomingDirection.z, _Roughness );	// * Masking( incoming )
			maskingShadowing *= PhongG1( wsDirection.z, _Roughness );			// * Masking( outgoing )
			break;

		case 4:
			// Diffuse Lobe Model
			wsDirection = lsPosition;// normalize( lsPosition.x * wsTangent + lsPosition.y * wsBiTangent + lsPosition.z * wsReflectedDirection );	// No scaling for that model
			wsScaledDirection = wsDirection;
			lobeIntensity = ComputeDiffuseModel( wsIncomingDirection, wsDirection, _Roughness, _Flattening ).x;	// _Flattening is the surface's albedo in this case
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

		lobeIntensity *= _Scale;

		lobeIntensity *= (lobeSign * wsDirection.z) < 0.0 ? 0.0 : 1.0;			// Nullify all "below the surface" directions

		lobeIntensity *= intensityMultiplier;									// So we match the simulated lobe's intensity scale

		wsDirection = wsScaledDirection;
		wsPosition = lobeIntensity * float3( wsDirection.x, wsDirection.z, -wsDirection.y );	// Vertex position in Y-up

	} else {
		// Show simulated lobe

//wsTangent = float3( 1, 0, 0 );
//wsBiTangent = float3( 0, 1, 0 );
//wsReflectedDirection = float3( 0, 0, 1 );

		float3	wsDirection = lsPosition.x * wsTangent + lsPosition.y * wsBiTangent + lsPosition.z * wsReflectedDirection;	// Direction in world space, aligned with reflected ray

		float	cosTheta_M = lobeSign * wsDirection.z;
		float	phi = fmod( 2.0 * PI + atan2( wsDirection.y, wsDirection.x ), 2.0 * PI );

//		float	theta = acos( clamp( cosTheta_M, -1.0, 1.0 ) );
//		float	thetaBinIndex = 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) );	// Inverse of theta = 2*asin( sqrt( i / (2 * N) ) )
		float	thetaBinIndex = LOBES_COUNT_THETA * (1.0 - cosTheta_M);					// Inverse of theta = acos( 1 - i / N )

		float2	UV = float2( phi / (2.0 * PI), thetaBinIndex / LOBES_COUNT_THETA );

		lobeIntensity = (_Flags & 4U) ? _Tex_DirectionsHistogram_Transmitted.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 )
									  : _Tex_DirectionsHistogram_Reflected.SampleLevel( LinearClamp, float3( UV, _ScatteringOrder ), 0.0 );

		lobeIntensity *= LOBES_COUNT_THETA * LOBES_COUNT_PHI;			// Re-scale due to lobe's discretization

		lobeIntensity *= intensityMultiplier;							// Manual intensity scale
		lobeIntensity = max( 0.01, lobeIntensity );						// So we always at least see a little something

		lobeIntensity *= (lobeSign * wsDirection.z) < 0.0 ? 0.0 : 1.0;	// Nullify all "below the surface" directions

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
	float4	analyticalLobeColor = isTransmittedLobe ? float4( _In.Color * float3( 1.0, 0.5, 1.0 ), solidAlpha )
													: float4( _In.Color * float3( 0.5, 1.0, 0.5 ), solidAlpha );
	float4	simulatedLobeWireColor = isTransmittedLobe	? float4( 0, 0, 0.1, wireframeAlpha )
														: float4( 0.1, 0, 0, wireframeAlpha );
	float4	analyticalLobeWireColor = isTransmittedLobe	? float4( 0.05, 0, 0.1, wireframeAlpha )
														: float4( 0, 0.1, 0, wireframeAlpha );

	bool	isDiffuseModel = (_Flags >> 4) == 4U;
	if ( isDiffuseModel ) {
		analyticalLobeColor = float4( _In.Color * float3( 1.0, 1.0, 0.5 ), 0.9 );
		analyticalLobeWireColor = float4( 0.1, 0.1, 0, wireframeAlpha );
	}

	if ( isAnalytical )
		return isWireframe ? analyticalLobeWireColor : analyticalLobeColor;
	else
		return isWireframe ? simulatedLobeWireColor : simulatedLobeColor;
}
