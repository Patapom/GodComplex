#include "Includes/global.hlsl"

#define TEST_MSBRDF	1

cbuffer CB_PostProcess : register(b2) {
};


struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

#if !TEST_MSBRDF

Texture2D<float4>		_TexBackground : register(t0);
Texture2DArray<float4>	_TexScattering : register(t1);

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy * _ScreenSize.zw;

//float3	BackgroundColor = float3( UV, 0 );
float3	BackgroundColor = _TexBackground[_In.__Position.xy].xyz;
float3	Scattering = _TexScattering.Sample( LinearWrap, float3( UV, 0.0 ) ).xyz;
float3	Extinction = _TexScattering.Sample( LinearWrap, float3( UV, 1.0 ) ).xyz;

	return BackgroundColor * Extinction + Scattering;
}

#else

// 2018-03-21 Rescucitating dead code!!

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
// 	Subscript[f, 2](Subscript[\[Omega], o],\[Alpha],\[Rho]) = Subscript[\[Sigma], 2](\[Rho]) (2+\[Eta](\[Alpha]))/\[Pi] \[Mu]^\[Eta](\[Alpha])
// 	\[Mu] = Subscript[\[Omega], o]\[CenterDot]Z
// 	
// The exponent \[Eta] is given as a function of surface roughness by:
// 
// 	\[Eta](Subscript[\[Alpha], s]) = 2.58838 \[Alpha]-1.35496 \[Alpha]^2
// 	
// The scale factor \[Sigma] is given by:
// 
// 	\[Sigma](\[Mu],Subscript[\[Alpha], s], \[Rho]) =a(Subscript[\[Alpha], s]) + b(Subscript[\[Alpha], s])\[Mu] + c(Subscript[\[Alpha], s]) \[Mu]^2 + d(Subscript[\[Alpha], s]) (\[Mu]^3) 
// 	Subscript[\[Sigma], 2](\[Mu],Subscript[\[Alpha], s], \[Rho]) =(\[Rho]^2) \[Sigma](\[Mu],Subscript[\[Alpha], s], \[Rho]) 
// 
// 	a(Subscript[\[Alpha], s])= 0.0288133 -0.921537 \[Alpha]+6.63273 \[Alpha]^2-4.5957 \[Alpha]^3
// 	b(Subscript[\[Alpha], s])= -0.0966326+7.21414 \[Alpha]-19.7868 \[Alpha]^2+11.0421 \[Alpha]^3
// 	c(Subscript[\[Alpha], s])= 0.109357 -10.7904 \[Alpha]+28.508 \[Alpha]^2-15.6653 \[Alpha]^3
// 	d(Subscript[\[Alpha], s])=-0.0437643+5.2492 \[Alpha]-13.5827 \[Alpha]^2+7.34841 \[Alpha]^3
// 
// The flattening factor Subscript[\[Sigma], n] along the main lobe direction Z is given by:
// 
// 	Subscript[\[Sigma], n](\[Mu],Subscript[\[Alpha], s]) =a(Subscript[\[Alpha], s]) + b(Subscript[\[Alpha], s])\[Mu] + c(Subscript[\[Alpha], s]) \[Mu]^2 + d(Subscript[\[Alpha], s]) (\[Mu]^3) 
// 	
// 	a(Subscript[\[Alpha], s])= 0.885056 -1.21098 \[Alpha]+0.225698 \[Alpha]^2+0.449826 \[Alpha]^3
// 	b(Subscript[\[Alpha], s])=0.0856807 +0.565903 \[Alpha]
// 	c(Subscript[\[Alpha], s])= -0.0770746-1.38461 \[Alpha]+0.856589 \[Alpha]^2
// 	d(Subscript[\[Alpha], s])=0.0104231 +0.852559 \[Alpha]-0.684474 \[Alpha]^2
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
// 
// 	\!\(TraditionalForm\`a(
// \*SubscriptBox[\(\[Alpha]\), \(s\)]) = \ 
//    0.8850557867448499`  - 1.2109761138443194`\ \[Alpha] + 
//     0.22569832413951335`\ 
// \*SuperscriptBox[\(\[Alpha]\), \(2\)] + 0.4498256199595464`\ 
// \*SuperscriptBox[\(\[Alpha]\), \(3\)]\)
// 	\!\(TraditionalForm\`b(
// \*SubscriptBox[\(\[Alpha]\), \(s\)]) = 
//    0.0856807009397115`  + 0.5659031384072539`\ \[Alpha]\)
// 	\!\(TraditionalForm\`c(
// \*SubscriptBox[\(\[Alpha]\), \(s\)]) = \ \(-0.07707463071513312`\) - 
//     1.384614678037336`\ \[Alpha] + 0.8565888280926491`\ 
// \*SuperscriptBox[\(\[Alpha]\), \(2\)]\)
// 	\!\(TraditionalForm\`d(
// \*SubscriptBox[\(\[Alpha]\), \(s\)]) = 
//    0.010423083821992304`  + 0.8525591060832015`\ \[Alpha] - 
//     0.6844738691665317`\ 
// \*SuperscriptBox[\(\[Alpha]\), \(2\)]\)
// 
// 
// Additional Scaling for 3rd Order Lobes
// 
// Using the same analytical model for 3rd order scattering lobes but letting the \[Sigma] parameter free for new evaluation, we obtain a pretty good fit for a new Subscript[\[Sigma], 3](\[Alpha], \[Rho])
// 
// 	Subscript[\[Sigma], 3](\[Mu],\[Alpha], \[Rho]) = \[Rho]^3 [0.363902 * \[Sigma](\[Mu],\[Alpha])]
// 	
//
float3	ComputeDiffuseModel( float3 _wsIncomingDirection, float3 _wsOutgoingDirection, float _roughness, float3 _albedo ) {

_wsIncomingDirection = float3( _wsIncomingDirection.x, -_wsIncomingDirection.z, _wsIncomingDirection.y );
_wsOutgoingDirection = float3( _wsOutgoingDirection.x, -_wsOutgoingDirection.z, _wsOutgoingDirection.y );


	float	cosTheta = saturate( _wsOutgoingDirection.z );

	// Compute lobe scale, exponent and flattening factor based on incoming direction and roughness
	float	mu = saturate( _wsIncomingDirection.z );
	float	mu2 = mu*mu;
	float	mu3 = mu*mu2;

	float	r = _roughness;
	float	r2 = r*r;
	float	r3 = r*r2;

	float4	abcd = float4(	 0.028813261153483097 - 0.9215374811620882 * r + 6.632726114385572  * r2 - 4.5957022306534    * r3,
							-0.09663259042197028  + 7.214143602200921  * r - 19.786845117100626 * r2 + 11.042058883797509 * r3,
							 0.10935692546815767  - 10.790405157520944 * r + 28.50803667636733  * r2 - 15.665258273262731 * r3,
							-0.04376425480146207  + 5.2491960091879    * r - 13.582707339717146 * r2 + 7.348408854602616  * r3
						);

	float	sigma2 = abcd.x + abcd.y * mu + abcd.z * mu2 + abcd.w * mu3;
	float	sigma3 = 0.363902052363025 * sigma2;

//	if ( (_DebugFlags & 4) == 0 )
//		sigma3 = 0.0;	// Don't use the 3rd order term

	// Compute lobe exponent
	float	eta = 2.588380909161985 * r - 1.3549594389004276 * r2;

	// Compute unscaled lobe intensity
	float	intensity = (eta+2) * pow( cosTheta, eta ) / PI;

	// Compute flattening factor
	abcd = float4(	   0.8850557867448499    - 1.2109761138443194 * r + 0.22569832413951335 * r2 + 0.4498256199595464 * r3,
					   0.0856807009397115    + 0.5659031384072539 * r,
					  -0.07707463071513312   - 1.384614678037336  * r + 0.8565888280926491  * r2,
					   0.010423083821992304  + 0.8525591060832015 * r - 0.6844738691665317  * r2
				);

	float	sigma_n = abcd.x + abcd.y * mu + abcd.z * mu2 + abcd.w * mu3;

	float	L = rsqrt( 1.0 + cosTheta*cosTheta * (1.0 / pow2( sigma_n ) - 1.0)  );

	// Add albedo-dependency
	return  L * intensity * _albedo*_albedo * (sigma2 + _albedo*sigma3);
}

cbuffer CB_RayMarch : register(b3) {
	float	_Sigma_t;
	float	_Sigma_s;
	float	_Phase_g;
};

float	RayTraceSphere( float3 _wsPos, float3 _wsDir, float3 _wsCenter, float _radius, out float3 _wsClosestPosition ) {
	float3	D = _wsPos - _wsCenter;
	_wsClosestPosition = _wsPos - dot( D, _wsDir ) * _wsDir;

	float	b = dot( D, _wsDir );
	float	c = dot( D, D ) - _radius*_radius;
	float	delta = b*b - c;
	if ( delta < 0.0 )
		return INFINITY;

	return -b - sqrt( delta );
}

float	RayTracePlane( float3 _wsPos, float3 _wsDir ) {
	float	t = -_wsPos.z / _wsDir.z;
	return t > 0.0 ? t : INFINITY;
}

static const float3	SPHERE_CENTER = float3( 0, 0, 1 );
static const float	SPHERE_RADIUS = 1;

float2	RayTraceScene( float3 _wsPos, float3 _wsDir, out float3 _wsNormal, out float3 _wsClosestPosition ) {
	_wsNormal = float3( 0, 0, 1 );

	float	t = RayTraceSphere( _wsPos, _wsDir, SPHERE_CENTER, SPHERE_RADIUS, _wsClosestPosition );
	if ( t < 1e4 ) {
		_wsNormal = normalize( _wsPos + t.x * _wsDir - SPHERE_CENTER );
		return float2( t, 0 );
	}

	t = RayTracePlane( _wsPos, _wsDir );
	if ( t < 1e4 )
		return float2( t, 1 );

	return float2( INFINITY, -1 );
}

float	ComputeShadow( float3 _wsPos, float3 _wsLight ) {
	float3	wsClosestPosition;
	float	t = RayTraceSphere( _wsPos, _wsLight, SPHERE_CENTER, SPHERE_RADIUS, wsClosestPosition );
	if ( t < 1e4 )
		return 0;

	float	r = length( wsClosestPosition - SPHERE_CENTER ) / SPHERE_RADIUS;
	return smoothstep( 1.0, 2, r );
}

float3	PS( VS_IN _In ) : SV_TARGET0 {

	float2	UV = float2( _ScreenSize.x / _ScreenSize.y * (2.0 * _In.__Position.x / _ScreenSize.x - 1.0), 1.0 - 2.0 * _In.__Position.y / _ScreenSize.y );

	const float3	CAMERA_POS = float3( 0, 1.5, 2 );
	const float3	CAMERA_TARGET = float3( -0.4, 0, 0.4 );
	const float3	LIGHT_COLOR = 40.0;
	const float3	ALBEDO = float3( 0.9, 0.5, 0.1 );	// Nicely saturated yellow
	const float3	AMBIENT = 0.02 * float3( 0.5, 0.8, 0.9 );

	const float		theta = _Phase_g * PI / 2;
	const float3	wsLight = normalize( float3( sin(theta), 0, cos(theta) ) );

	const float3	albedo = _Sigma_s * ALBEDO;
	const float		roughness = _Sigma_t;

	float3	csView = normalize( float3( UV, 1 ) );
	float3	wsAt = normalize( CAMERA_TARGET - CAMERA_POS );
	float3	wsRight = normalize( cross( wsAt, float3( 0, 0, 1 ) ) );
	float3	wsUp = cross( wsRight, wsAt );
	float3	wsView = csView.x * wsRight + csView.y * wsUp + csView.z * wsAt;

	float3	wsClosestPosition;
	float3	wsNormal;
	float2	hit = RayTraceScene( CAMERA_POS, wsView, wsNormal, wsClosestPosition );

	if ( hit.x > 1e4 )
		return 0.0;	// No hit

	float3	wsPosition = CAMERA_POS + hit.x * wsView;

	// Compute simple lighting
	float	LdotN = dot( wsLight, wsNormal );
	float	shadow = hit.y != 0 ? ComputeShadow( wsPosition, wsLight ) : 1;

	float3	diffuseTerm = (albedo / PI) * saturate( LdotN ) * shadow * LIGHT_COLOR;

	// Add magic?
	if ( roughness > 0 ) {
		float	shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN-0.2) ) );	// This removes shadowing on back faces
				shadow2 = 1.0 - pow( saturate( 1.0 - shadow2 ), 16.0 );
//				shadow2 *= saturate( 0.2 + 0.8 * dot( light, _normal ) );	// Larger L.N, eating into the backfaces
				shadow2 *= LdotN;

//shadow2 = shadow;

		diffuseTerm += saturate(ComputeDiffuseModel( wsLight, -wsView, roughness, albedo ) / PI) * shadow2 * LIGHT_COLOR;

//diffuseTerm = shadow * LdotN;
	}

	return AMBIENT + diffuseTerm;
}

#endif