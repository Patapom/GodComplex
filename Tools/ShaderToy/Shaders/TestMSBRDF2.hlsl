// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
//
// A list of useful distance function to simple primitives, and an example on how to 
// do some interesting boolean operations, repetition and displacement.
//
// More info here: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
//
#include "Includes/global.hlsl"
#include "Includes/DistanceFieldHelpers.hlsl"

static const float	TAN_HALF_FOV = 0.57735026918962576450914878050196;	// tan( 60° / 2 )


//----------------------------------------------------------------------

float2 map( in float3 pos )
{
    float2 res = opU( float2( sdPlane(     pos), 1.0 ),
	                float2( sdSphere(    pos-float3( 0.0,0.25, 0.0), 0.25 ), 46.9 ) );
    res = opU( res, float2( sdBox(       pos-float3( 1.0,0.25, 0.0), 0.25 ), 3.0 ) );
    res = opU( res, float2( udRoundBox(  pos-float3( 1.0,0.25, 1.0), 0.15, 0.1 ), 41.0 ) );
	res = opU( res, float2( sdTorus(     pos-float3( 0.0,0.25, 1.0), float2(0.20,0.05) ), 25.0 ) );
    res = opU( res, float2( sdCapsule(   pos,float3(-1.3,0.10,-0.1), float3(-0.8,0.50,0.2), 0.1  ), 31.9 ) );
	res = opU( res, float2( sdTriPrism(  pos-float3(-1.0,0.25,-1.0), float2(0.25,0.05) ),43.5 ) );
	res = opU( res, float2( sdCylinder(  pos-float3( 1.0,0.30,-1.0), float2(0.1,0.2) ), 8.0 ) );
	res = opU( res, float2( sdCone(      pos-float3( 0.0,0.50,-1.0), float3(0.8,0.6,0.3) ), 55.0 ) );
	res = opU( res, float2( sdTorus82(   pos-float3( 0.0,0.25, 2.0), float2(0.20,0.05) ),50.0 ) );
	res = opU( res, float2( sdTorus88(   pos-float3(-1.0,0.25, 2.0), float2(0.20,0.05) ),43.0 ) );
	res = opU( res, float2( sdCylinder6( pos-float3( 1.0,0.30, 2.0), float2(0.1,0.2) ), 12.0 ) );
	res = opU( res, float2( sdHexPrism(  pos-float3(-1.0,0.20, 1.0), float2(0.25,0.05) ),17.0 ) );

    res = opU( res, float2( opS(
		             udRoundBox(  pos-float3(-2.0,0.2, 1.0), 0.15,0.05),
	                 sdSphere(    pos-float3(-2.0,0.2, 1.0), 0.25)), 13.0 ) );

    res = opU( res, float2( opS(
		             sdTorus82(	pos-float3(-2.0,0.2, 0.0), float2(0.20,0.1)),
	                 sdCylinder(  opRep( float3(
											(3.14 + atan2(pos.x+2.0,pos.z))/6.2831,
											pos.y,
											0.02+0.5*length(pos-float3(-2.0,0.2, 0.0))
											),
								  float3(0.05,1.0,0.05)), float2(0.02,0.6))), 51.0 ) );

	res = opU( res, float2( 0.7*sdSphere(    pos-float3(-2.0,0.25,-1.0), 0.2 ) + 
										     0.03*sin(50.0*pos.x)*sin(50.0*pos.y)*sin(50.0*pos.z), 
											65.0 ) );

	res = opU( res, float2( 0.5*sdTorus( opTwist(pos-float3(-2.0,0.25, 2.0)),float2(0.20,0.05)), 46.7 ) );

    res = opU( res, float2( sdConeSection( pos-float3( 0.0,0.35,-2.0), 0.15, 0.2, 0.1 ), 13.67 ) );

    res = opU( res, float2( sdEllipsoid( pos-float3( 1.0,0.35,-2.0), float3(0.15, 0.2, 0.05) ), 43.17 ) );
        
    return res;
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
}

float3 render( in float3 ro, in float3 rd, bool _useModel, out float3 _normal, out float _distance ) { 
	float3	col = float3(0.7, 0.9, 1.0) + rd.y*0.8;	// Sky color
	float2	res = castRay(ro,rd);
	float	t = res.x;
	float	m = res.y;
	_normal = float3( 0, 1, 0 );
	_distance = t;
	if( m <= -0.5 )
		return saturate( col );

	float3 pos = ro + t*rd;
	_normal = calcNormal( pos );
	float3 ref = reflect( rd, _normal );
        
	// material        
	float3	albedo = 0.45 + 0.3*sin( float3(0.05,0.08,0.10)*(m-1.0) );
	float	roughness = 0.5 + 0.5 * sin( 0.1 * (m-1.0) );
		
	if ( m < 1.5 ) {
		// Floor
		float f = abs( fmod( floor(5.0*pos.z) + floor(5.0*pos.x), 2.0 ) );
		albedo = 0.4 + 0.1*f;
		roughness = 0.4 + 0.6 * abs( f );
	}

	// lighting        
	float	occ = calcAO( pos, _normal );
	float3	lig = normalize( float3(-0.6, 0.7, -0.5) );
	float3	lig2 = normalize(float3(-lig.x,0.0,-lig.z));	// For backlighting
	float3	lightIntensity = 1.2 * float3(1.0,0.85,0.55);
	float3	lightIntensity2 = 0.3;// * float3(1.0,0.85,0.55);

	float	amb = saturate( 0.5+0.5*_normal.y );
	float	LdotN = saturate( dot( _normal, lig ) );
	float	LdotN2 = saturate( dot( _normal, lig2 ) ) * saturate( 1.0-pos.y );
	float	dom = smoothstep( -0.1, 0.1, ref.y );
	float	fre = pow( saturate( 1.0+dot(_normal,rd) ), 2.0 );
	float	spe = pow( saturate( dot( ref, lig ) ), 16.0 );
        
	float	shadow = softshadow( pos, lig, 0.02, 2.5 );
	float	shadow_ref = softshadow( pos, ref, 0.02, 2.5 );
	dom *= shadow_ref;

	// Add rough diffuse model
	float3	dif2 = 0.0;
	if ( _useModel ) {

		roughness *= _DebugParm;
		roughness = max( 1e-3, roughness );

		float	shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN-0.2) ) );	// This removes shadowing on back faces
				shadow2 = 1.0 - shadow2;
				shadow2 = pow2( shadow2 );	// ^2
				shadow2 = pow2( shadow2 );	// ^4
				shadow2 = pow2( shadow2 );	// ^8
				shadow2 = pow2( shadow2 );	// ^16
				shadow2 = 1.0 - shadow2;
//				shadow2 *= saturate( 0.2 + 0.8 * LdotN );	// Larger L.N, eating into the backfaces

		dif2 = ComputeDiffuseModel( lig, -rd, roughness, albedo ) * lightIntensity * shadow * LdotN;

		// 2nd light
		shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN2-0.2) ) );	// This removes shadowing on back faces
		shadow2 = 1.0 - shadow2;
		shadow2 = pow2( shadow2 );	// ^2
		shadow2 = pow2( shadow2 );	// ^4
		shadow2 = pow2( shadow2 );	// ^8
		shadow2 = pow2( shadow2 );	// ^16
		shadow2 = 1.0 - shadow2;
//		shadow2 *= saturate( 0.2 + 0.8 * LdotN2 );	// Larger L.N, eating into the backfaces

		dif2 += ComputeDiffuseModel( lig2, -ref, roughness, albedo ) * lightIntensity2 * shadow * LdotN2;
	}

	float3 lin = float3(0,0,0);
	lin += lightIntensity * shadow * LdotN;
	lin += lightIntensity * shadow * spe * LdotN;
	lin += 0.20*amb*float3(0.50,0.70,1.00)*occ;
	lin += 0.30*dom*float3(0.50,0.70,1.00)*occ;
	lin += lightIntensity2 * LdotN2*float3(0.25,0.25,0.25)*occ;
	lin += 0.40*fre*float3(1.00,1.00,1.00)*occ;

	col = albedo * lin;

	col += dif2;


//col = 0.1 * pos;



	// Add some fog
	col = lerp( col, float3(0.8,0.9,1.0), 1.0-exp( -0.002*t*t ) );

if ( _DebugFlags & 8 ) {
	col = dif2;
} else if ( _DebugFlags & 1 ) {
	col = roughness;
}

	return saturate( col );
}

float3x3 setCamera( in float3 ro, in float3 ta, float cr )
{
	float3 cw = normalize(ta-ro);
	float3 cp = float3(sin(cr), cos(cr),0.0);
	float3 cu = normalize( cross(cw,cp) );
	float3 cv = normalize( cross(cu,cw) );
    return float3x3( cu, cv, cw );
}

struct PS_OUT {
	float4	Color : SV_TARGET0;
	float4	NormalDepth : SV_TARGET1;
};

PS_OUT PS( VS_IN _In ) {

	float2	fragCoord = _In.__Position.xy;
	float2	UV = fragCoord.xy / iResolution.xy;

	#if 1
		float	AspectRatio = iResolution.x / iResolution.y;
		float	pixelRadius = 2.0 * SQRT2 * TAN_HALF_FOV / iResolution.y;
		float3	csView = normalize( float3( AspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 ) );
		float3	ro = _Camera2World[3].xyz;
		float3	rd = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	#else
		float2	p = -1.0+2.0*UV;
				p.x *= iResolution.x / iResolution.y;

		float2 mo = 0;//iMouse.xy / iResolution.xy;
		 
		float time = 15.0 + iGlobalTime;

		// camera	
		float3 ro = float3( -0.5+3.5*cos(0.1*time + 6.0*mo.x), 1.0 + 2.0*mo.y, 0.5 + 3.5*sin(0.1*time + 6.0*mo.x) );
		float3 ta = float3( -0.5, -0.4, 0.5 );
	
		// camera-to-world transformation
		float3x3 ca = setCamera( ro, ta, 0.0 );

		// ray direction
		float3	rd = mul( normalize( float3(p.x, -p.y, 2.0) ), ca );
	#endif

	bool	useModel = _DebugFlags & 2;// && UV.x > _MousePosition.x;

	// Render
	PS_OUT	Out;
	float3	normal;
	float	distance;
	Out.Color = float4( render( ro, rd, useModel, normal, distance ), distance );
//	Out.NormalDepth = float4( normal, _In.__Position.z );	// SV_Position.z = post-perspective Z/W  |  SV_Position.w = 1/W

	float4	projPosition = mul( float4( ro + distance * rd, 1.0 ), _World2Proj );
	Out.NormalDepth = float4( normal, projPosition.z / projPosition.w );

//	float4	csPosition = mul( float4( ro + distance * rd, 1.0 ), _World2Camera );
//	Out.NormalDepth = float4( normal, csPosition.z );


//Out.Color.xyz = normal;

	return Out;
}