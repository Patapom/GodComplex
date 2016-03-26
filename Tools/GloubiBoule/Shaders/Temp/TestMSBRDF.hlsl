
SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

cbuffer CB_Main : register(b0) {
	float3	iResolution;	// viewport resolution (in pixels)
	float	iGlobalTime;	// shader playback time (in seconds)

	float2	_OnSenFout0;
	float2	_OnSenFout1;
	float2	_OnSenFout2;
	float2	_OnSenFout3;
	float2	_OnSenFout4;

	uint	_OnSenFout5;
	float	_DebugPlaneHeight;
	uint	_DebugFlags;
	float	_DebugParm;
	float2	_MousePosition;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Proj2World;
	float4x4	_World2Proj;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;
};

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}


/////////////////////////////////////////////////////////////////////////////////////////////

#define mix	lerp
#define mod	fmod
#define fract frac

#define PI	3.1415926535897932384626433832795
static const float	INFINITY = 1e6;
static const float	TAN_HALF_FOV = 0.57735026918962576450914878050196;	// tan( 60° / 2 )
static const float	SQRT2 = 1.4142135623730950488016887242097;

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float3	Fresnel_IORFromF0( float3 _F0 ) {
	float3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0 );
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float3	FresnelAccurate( float3 _IOR, float _CosTheta )
{
	float	c = _CosTheta;
	float3	g_squared = max( 0.0, _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	float3	g = sqrt( g_squared );

	float3	a = (g - c) / (g + c);
			a *= a;
	float3	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

#include "Includes/DistanceFieldHelpers.hlsl"

float	map( float3 p, out uint _materialID ) {
	float	distance2Plane = plane( p, float3( 0, 1, 0 ), 0.0 );
	float	distance2Sphere = sphere( p - float3( 0, 1, 0 ), 1.0 );

	float	minDistance = distance2Plane;
	_materialID = 0;
	if ( distance2Sphere < minDistance ) {
		minDistance = distance2Sphere;
		_materialID = 1;
	}

	return minDistance;
}

float	map( float3 p ) {
	uint materialID;
	return map( p, materialID );
}

#if 0
float4	ComputeHit( float3 _origin, float3 _direction, float _initialStepSize, float _maxDistance, out uint _materialID, const float _pixelRadius, const uint _maxIterations=64 ) {
	float	omega = 1.2;	// Over-relaxation size in [1,2]
	float4	pos = float4( _origin, 0.0 );
	float4	step = float4( _direction, 1.0 );
			pos += _initialStepSize * step;

	_materialID = ~0U;
	float	candidate_error = INFINITY;
	float4	candidateHit = pos;
	float	previousRadius = 0.0;
	float	stepLength = 0.0;
	float	functionSign = map( pos.xyz ) < 0.0 ? -1.0 : 1.0;	// To correct the fact we may start from inside an object
	for ( uint i=0; i < _maxIterations; ++i ) {
		float	signedRadius = functionSign * map( pos.xyz, _materialID );
		float	radius = abs( signedRadius );
		bool	overRelaxtionFailed = omega > 1.0 && (radius + previousRadius) < stepLength;
		if ( overRelaxtionFailed ) {
			// Failed! Go back to normal sphere tracing with a unit over-relaxation factor...
			stepLength -= omega * stepLength;
			omega = 1.0;
		} else {
			stepLength = signedRadius * omega;	// Use a larger radius than given by the distance field (i.e. over-relaxation)
		}
		previousRadius = radius;

		float error = radius / pos.w;
		if ( !overRelaxtionFailed && error < candidate_error ) {
			// Keep smallest radius for candidate hit
			candidateHit = pos;
			candidate_error = error;
		}

		if ( !overRelaxtionFailed && error < _pixelRadius || pos.w > _maxDistance )
			break;

		pos += stepLength * step;
	}
	if ( (pos.w > _maxDistance || candidate_error > _pixelRadius) )
		return float4( pos.xyz, INFINITY );	// No hit!

	// Finalize hit by computing a proper intersection
#if 0
	for ( uint j=0; j < 5; j++ ) {
		float	signedRadius = functionSign * map( candidateHit.xyz );
		float	err = 0.01 * candidateHit.w * _pixelRadius;
		stepLength = signedRadius - err;
		candidateHit += stepLength * step;
	}
#elif 1
	for ( uint j=0; j < 5; j++ ) {
		float	signedRadius = functionSign * map( candidateHit.xyz );
		float	err = 20.0 * _pixelRadius * candidateHit.w;
		stepLength = signedRadius * pow( 2.0, 1.0 - err * j );
		candidateHit += stepLength * step;
	}
#endif

	return candidateHit;
}
#else
// iQ's version, simpler
float4	ComputeHit( float3 _origin, float3 _direction, float _initialStepSize, float _maxDistance, out uint _materialID, const float _pixelRadius, const uint _maxIterations=64 ) {
	float tmin = 0.1;
	float tmax = _maxDistance;	// 20.0
    
	float3	ro = _origin;
	float3	rd = _direction;

#if 0
	float tp1 = (0.0-ro.y)/rd.y; if( tp1>0.0 ) tmax = min( tmax, tp1 );
	float tp2 = (1.6-ro.y)/rd.y; if( tp2>0.0 ) {
		if( ro.y>1.6 ) tmin = max( tmin, tp2 );
		else           tmax = min( tmax, tp2 );
	}
#endif
    
	float precis = 0.002;
	float t = tmin;
	_materialID = ~0U;
	for ( uint i=0; i < _maxIterations; i++ ) {
		float	res = map( ro + rd*t, _materialID );
		if ( res < precis || t > tmax )
			break;
		t += res;
	}

	if ( t > tmax )
		t = 1e6;

	return float4( ro + t * rd, t );
}
#endif


float	pow2( float a ) { return a*a; }


// From http://graphicrants.blogspot.fr/2013/08/specular-brdf-reference.html
float	Smith_GGX( float _dot, float _alpha2 ) {
	return 2.0 * _dot / (1e-6 + _dot + sqrt( _alpha2 + (1.0 - _alpha2) * _dot*_dot ));
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

#if 1
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

	if ( (_DebugFlags & 4) == 0 )
		sigma3 = 0.0;	// Don't use the 3rd order term

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

#else
	// Formerly, wrong model!
	float	gloss = 1.0 - _roughness;

	float	cosTheta = saturate( _wsOutgoingDirection.z );

	// Compute sigma, the global scale factor
	float3	k2 = _albedo*_albedo;	// Dependence on albedo²
	float3	sigma2 = _roughness * (2.324842464398671 + _roughness * (-2.7502131990906116 + _roughness * 1.012605093077086));
			sigma2 *= k2;

	float3	k3 = _albedo*_albedo*_albedo;	// Dependence on albedo^3
	float3	sigma3 = _roughness * (0.25262765805538445 + _roughness * (0.3902065605355212 - _roughness * 0.3820487315212411));
			sigma3 *= k3;

//sigma2 *= 0;

	if ( (_DebugFlags & 4) == 0 )
		sigma3 = 0;	// Don't use the 3rd order term

	// Compute lobe exponent
	float	eta = 0.7782894918463 + 0.1683172467667511 * _roughness;

	// Compute unscaled lobe intensity
	float3	intensity = (sigma2 + sigma3) * pow( cosTheta, eta );

	// Compute flattening
	float3	abc = float3(	0.697462 - 0.479278 * gloss,
							0.287646 - 0.293594 * gloss,
							8.219680 + 9.540870 * gloss );
	float	sigma_n = abc.x + abc.y * exp2( -abc.z * cosTheta );
	float	L = rsqrt( 1.0 + cosTheta*cosTheta * (1.0 / pow2( sigma_n ) - 1.0)  );

	return  L * intensity;
#endif
}

static const float3	skyColorZenith = float3( 0.5, 0.85, 1.0 );
static const float3	skyColorHorizon = float3( 0.7, 0.9, 1.0 );

//static const float3	planeAlbedo = float3( 0.5, 0.5, 0.5 );
static const float3	planeAlbedo = float3( 0.5, 0.3, 0.1 );
static const float3	planeSpecular = float3( 0.8, 0.78, 0.75 );
static const float3	sphereAlbedo = float3( 0.5, 0.3, 0.6 );

float3	GetSkyColor( float3 _direction ) {
	return lerp( lerp( skyColorHorizon, skyColorZenith, saturate( _direction.y ) ), skyColorZenith * planeAlbedo / PI, saturate( -_direction.y ) );
}

float3	ComputeSky( float3 _pos ) {
	return 1.0 * GetSkyColor( normalize( _pos ) );
}

float ComputeDirectionalShadow( float3 ro, float3 rd, float3 n, float mint=0.5, float tmax=3.5 ) {
	ro += 0.01 * n;
	float res = 1.0;
    float t = mint;
	[loop]
    for( uint i=0; i < 16; i++ ) {
		float	h = map( ro + rd*t );
        res = min( res, 4.0*h/t );
        t += clamp( h, 0.02, 0.10 );
        if ( h < 0.001 || t > tmax )
			break;
    }
    return saturate( res );
}

float3	ComputeLighting( float3 _pos, float3 _normal, float3 _view, out float3 _fresnel, uint _materialID, bool _useModel ) {

	float3	light = normalize( float3( 1, 1, 0 ) );	// Arbitrary light vector
	float3	lightColor = 8.0;
	float	shadow = ComputeDirectionalShadow( _pos, light, _normal );


	float3	H = normalize( light + _view );
	float	LdotN = saturate( dot( _normal, light ) );
	float	VdotN = saturate( dot( _normal, light ) );
	float	HdotN = saturate( dot( H, _normal ) );


	// Build surface info
	float3	albedo, specular;
	float	roughness;
	float	metal = 0.0;
	switch ( _materialID ) {
	case 1:	// Sphere
		albedo = sphereAlbedo;
		specular = 1.0;
		roughness = lerp( 0.01, 1.0, _DebugParm );
		metal = 0.0;
		break;
	default:
		albedo = planeAlbedo;
		specular = planeSpecular;
//		roughness = 0.1;
//		metal = 0.9;
		roughness = lerp( 0.01, 1.0, _DebugParm );
		metal = 0.1;
		break;
	}

	// Use metal to interpolate albedos & specular F0
	albedo = lerp( albedo, 0.0, metal );
	float3	F0 = lerp( 0.01, specular, metal );

	// Compute IOR
	float3	IOR = Fresnel_IORFromF0( F0 );
	_fresnel = FresnelAccurate( IOR, HdotN );

	// Compute specular term (GGX + Smith shadowing)
	float	alpha2 = pow2( roughness );
	float	den = (HdotN * HdotN * (alpha2 - 1.0) + 1.0);
	float	GGX = alpha2 / (PI * den * den);	// TODO: Find out what happens when varying the denominator's exponent (need to find proper normalization!)
	float	Smith = Smith_GGX( LdotN, alpha2 ) * Smith_GGX( VdotN, alpha2 );
	float3	specularBRDF = _fresnel * Smith * GGX / (1e-6 + 4.0 * LdotN * VdotN);
	float3	specularTerm = specularBRDF * LdotN * shadow * lightColor;

	// Compute ambient term
	float3	ambientTerm = GetSkyColor( _normal );
			ambientTerm *= (albedo / PI);
			ambientTerm *= 1.0;
			ambientTerm *= AO( _pos, _normal, 20.0, 0.1 );
//			ambientTerm *= 1.0 - _fresnel;	// Should be diffuse fresnel

	// Compute diffuse term
	float3	diffuseBRDF = (1-_fresnel) * (albedo / PI);
	float3	diffuseTerm = diffuseBRDF * LdotN * shadow * lightColor;

	// Add magic?
	if ( _useModel ) {
		float	shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN-0.2) ) );	// This removes shadowing on back faces
				shadow2 = 1.0 - shadow2;
				shadow2 = pow2( shadow2 );	// ^2
				shadow2 = pow2( shadow2 );	// ^4
				shadow2 = pow2( shadow2 );	// ^8
				shadow2 = pow2( shadow2 );	// ^16
				shadow2 = 1.0 - shadow2;
//				shadow2 *= saturate( 0.2 + 0.8 * dot( light, _normal ) );	// Larger L.N, eating into the backfaces
				shadow2 *= LdotN;

if ( _DebugFlags & 8 ) {
	ambientTerm = 0.0;
	diffuseTerm = 0.0;
	specularTerm = 0.0;
}

		diffuseTerm += (ComputeDiffuseModel( light, _view, roughness, albedo ) / PI) * shadow2 * lightColor;
	}

//return HdotN;
//return 1-FresnelAccurate( IOR, HdotN );
//return dot( H, _normal );
//return _view;

_fresnel *= pow2( 1-roughness );

	return ambientTerm + diffuseTerm + specularTerm;
}

float3	Shader( float2 _UV, bool _useModel ) {
	float	AspectRatio = iResolution.x / iResolution.y;
	float	pixelRadius = 2.0 * SQRT2 * TAN_HALF_FOV / iResolution.y;
	float3	csView = normalize( float3( AspectRatio * TAN_HALF_FOV * (2.0 * _UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * _UV.y), 1.0 ) );
	float3	wsPos = _Camera2World[3].xyz;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	float3	color = 0.0;

	bool	useModel = _useModel && _UV.x > _MousePosition.x;

	float3	fresnel = 0.0;
	uint	materialID;
	float4	hitPosition = ComputeHit( wsPos, wsView, 0.1, 40.0, materialID, pixelRadius );

	float3	N = normal( hitPosition.xyz );
	float3	Lighting0 = hitPosition.w < 1e3	? ComputeLighting( hitPosition.xyz, N, -wsView, fresnel, materialID, useModel )
											: ComputeSky( hitPosition.xyz );

	float3	secondFresnel;
	float3	wsReflect = reflect( wsView, N );
	float4	secondHitPosition = ComputeHit( hitPosition.xyz, wsReflect, 0.01, 40.0, materialID, pixelRadius );
	float3	N2 = normal( secondHitPosition.xyz );
	float3	Lighting1 = secondHitPosition.w < 1e3	? ComputeLighting( secondHitPosition.xyz, N2, -wsReflect, secondFresnel, materialID, useModel )
													: ComputeSky( secondHitPosition.xyz );

	float3	Lighting = lerp( Lighting0, Lighting1, fresnel );


	if ( abs( iResolution.x * (_UV.x - _MousePosition.x) ) < 2 )
		return 0.0;

color = Lighting;
//color = fresnel;
//color = Lighting0;

	if ( _DebugFlags & 1 )
		return DebugPlane( color, wsPos, wsView, hitPosition.w );
	else
		return color;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;
	float3	Color = Shader( UV, (_DebugFlags & 2) );

/*if ( (_DebugFlags & 4) ) {
	float3	colorWithout = Shader( UV, false );
//	Color -= colorWithout;
//	Color = (Color - colorWithout) / max( 1e-6, colorWithout );
	Color = (Color - colorWithout) / 8.0;
}*/


//if ( any(Color < 0.0) ) Color = float3( 1, 0, 1 );
	
	return Color;
}
