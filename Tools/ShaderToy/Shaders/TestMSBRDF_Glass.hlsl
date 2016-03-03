#include "Includes/global.hlsl"
#include "Includes/DistanceFieldHelpers.hlsl"

Texture2DArray<float4>	_TexSource : register(t0);

static const float	TAN_HALF_FOV = 0.57735026918962576450914878050196;	// tan( 60° / 2 )

//----------------------------------------------------------------------

float2 map( in float3 pos ) {

	float2 res = float2( INFINITY, -1 );

    res = opU( res, float2( sdEllipsoid( pos-float3( -1.0,0.35,-2.0), float3(0.15, 0.2, 0.05) ), 28 ) );

	res = opU( res, float2( 0.7*sdSphere(    pos-float3(-2.0,0.25,-2.0), 0.2 ) + 
										     0.03*sin(50.0*pos.x)*sin(50.0*pos.y)*sin(50.0*pos.z), 
											39.37 ) );
        
    return res;
}

//----------------------------------------------------------------------
// From Walter 2007 eq. 40
// Expects _incoming pointing AWAY from the surface
// eta = IOR_above / IOR_below
//
float3	Refract( float3 _incoming, float3 _normal, float _eta ) {
	float	c = dot( _incoming, _normal );
	float	b = 1.0 + _eta * (c*c - 1.0);
	if ( b >= 0.0 ) {
		float	k = _eta * c - sign(c) * sqrt( b );
		float3	R = k * _normal - _eta * _incoming;
		return normalize( R );
	} else {
		return 0.0;	// Total internal reflection
	}
}

float	ComputeSphereHitDistance( float3 _wsPosition, float3 _wsView, float3 _wsCenter, float _radius ) {
	float3	D = _wsPosition - _wsCenter;
	float	c = dot( D, D ) - _radius*_radius;
	float	b = dot( D, _wsView );

	float	delta = b*b - c;
	return delta >= 0.0 ? -b - sign(c) * sqrt(delta) : 1e8;
}

//----------------------------------------------------------------------
// Computes the blurred background color and attenuation of this color when a camera ray enters a rough dielectric material of a given thickness
//
//	_wsPosition, the world-space position of the pixel
//	_wsNormalFront, the world-space normal of the pixel on the front-side of the material (coming from the normal map)
//	_wsView, the world-space view vector pointing toward the camera
//	_sceneZ, the Z of the scene behind the pixels
//	_transmittanceColor, target color for the dielectric
//	_surfaceCurvature, the surface's curvature in [-1,+1]. -1 makes a concave lens, +1 for a convex lens, 0 for a plane
//	_thickness, thickness (in meters) of the dielectric material
//	_thicknessFullColor, thickness (in meters) at which the material reaches the specified transmittance color
//	_roughness, roughness of the material
//	_IOR, index of refraction of the material
//
// Returns:
//	the attenuated color of the blurred background
// 
float3	ComputeGlassColor( float3 _wsPosition, float3 _wsNormalFront, float3 _wsView, float _sceneZ, float3 _transmittanceColor, float _surfaceCurvature, float _thickness, float _thicknessFullColor, float _roughness, float _IOR ) {

	// Compute direction refracted against the front surface
	float3	refractedView_inside = Refract( _wsView, _wsNormalFront, 1.0 / _IOR );

	// Compute intersection with a sphere whose radius and center is computed depending on surface's curvature
	const float	MIN_RADIUS = 0.4;
	const float	MAX_RADIUS = 1000.0;
	float	sphereRadius = 1.0 / lerp( (1.0 / MAX_RADIUS), (1.0 / MIN_RADIUS), _surfaceCurvature );

	float3	wsSphereCenter = _wsPosition - (_thickness + sphereRadius) * _wsNormalFront;
	float	thickness = ComputeSphereHitDistance( _wsPosition, refractedView_inside, wsSphereCenter, sphereRadius );
	float3	wsPosition_Back = _wsPosition + thickness * refractedView_inside;	// This is where we'll hit the back of the surface

	// Compute back normal
	float3	wsNormalBack = (wsSphereCenter - wsPosition_Back) / sphereRadius;

	// Refract against back surface
	float3	refractedView = Refract( -refractedView_inside, -wsNormalBack, _IOR );
//	float3	refractedView = refractedView_inside;	// Keep same direction

	// Now, compute the intersection of that final refracted ray with the plane at "scene Z"
	// (here we should intersect with a sphere since we're given the distance, not the Z, but I don't care)
	float3	wsCameraPos = _Camera2World[3].xyz;
	float3	wsCameraZ = _Camera2World[2].xyz;	// "At" vector
	float	orthoDistance = _sceneZ - dot( wsPosition_Back - wsCameraPos, wsCameraZ );
	float	sceneHitDistance = orthoDistance / dot( refractedView, wsCameraZ );
	float3	wsScenePosition = wsPosition_Back + sceneHitDistance * refractedView;

	// Project into camera clip space
	float4	projScenePosition = mul( float4( wsScenePosition, 1.0 ), _World2Proj );
			projScenePosition /= projScenePosition.w;
	float2	UV_back = float2( 0.5 * (1.0 + projScenePosition.x), 0.5 * (1.0 - projScenePosition.y) );

	// Use the formula linking roughness to lobe aperture half-angle to compute the radius
	//	of the disc that represents the intersection of the rough cone with the back plane
	float	lobeApertureHalfAngle = _roughness * (1.3331290497744692 - _roughness * 0.5040552688878546);
	float	discRadius = sceneHitDistance * tan( lobeApertureHalfAngle );

	// Compute the projected radius and retrieve mip level
	float	screenRadius = _sceneZ * TAN_HALF_FOV;				// The "radius" of the screen at the specified Z
	float	discRadius_UV = 0.5 * discRadius / screenRadius;
	float	coveredPixels = iResolution.y * discRadius_UV;		// The amount of pixels covered by the rough cone
	float	mipLevel = log2( 1e-6 + coveredPixels );

	// Sample background at specified position and mip
	UV_back -= discRadius_UV * float2( iResolution.y / iResolution.x, 1.0 );	// Because our blur is centered on top-left pixel
	float3	backColor = _TexSource.SampleLevel( LinearMirror, float3( UV_back, 0.0 ), mipLevel ).xyz;

	// Now that we have the back color properly blurred, we need to compute its transmittance.
	// The back color is light passing through a slab of glass of a specified thickness so it gets influenced by:
	//	• The absorption of the material
	//	• The Fresnel reflectance of the material at both the entry point on the back side and the exit point on the front side

	// Compute Fresnel reflectances
	// We need to compute 2 distinct reflectances here:
// 	float	Fresnel_transmitted_back = 1.0 - FresnelAccurate( _IOR, dot( refractedView, wsNormalBack ) );					//	1) What has NOT been reflected by the back of the surface
// 	float	Fresnel_transmitted_front = 1.0 - FresnelAccurate( 1.0 / _IOR, -dot( refractedView_inside, _wsNormalFront ) );	//	2) What has NOT been reflected by the inside of the front of the surface and transmitted through to the front where it can be viewed by the camera
	float	Fresnel_transmitted_back = 1.0 - FresnelAccurate( _IOR, saturate( dot( refractedView, wsNormalBack ) ) );					//	1) What has NOT been reflected by the back of the surface
	float	Fresnel_transmitted_front = 1.0 - FresnelAccurate( 1.0 / _IOR, saturate( -dot( refractedView_inside, _wsNormalFront ) ) );	//	2) What has NOT been reflected by the inside of the front of the surface and transmitted through to the front where it can be viewed by the camera

	// Compute absorption factor based on refraction indices (eq. 21 in Walter 2007)
	float3	Ht = normalize( -_wsView - _IOR * refractedView_inside );	// Transmitted half vector
	float	absorptionCoeff = abs( dot( _wsView, Ht ) * dot( refractedView_inside, Ht ) ) * _IOR * _IOR
							/ pow2( dot( _wsView, Ht ) + _IOR * dot( refractedView_inside, Ht ) );

			Ht = normalize( _IOR * refractedView_inside - refractedView );	// Transmitted half vector
			absorptionCoeff *= abs( dot( refractedView_inside, Ht ) * dot( refractedView, Ht ) )
							/ pow2( _IOR * dot( refractedView_inside, Ht ) + dot( refractedView, Ht ) );

	absorptionCoeff = lerp( 1.0, absorptionCoeff, saturate( 1.0 * (_IOR-1.0) ) );	// This is to counterbalance the ill-behaved coeff when IOR->1
//absorptionCoeff = 1.0;

	// Compute absorption
	float3	sigma_t = -log( 1e-6 + _transmittanceColor ) / _thicknessFullColor;	// Artists can make the glass more or less colored by varying its thickness
	float3	absorption = exp( -sigma_t * thickness );

	float3	transmittance = Fresnel_transmitted_back * absorptionCoeff * absorption * Fresnel_transmitted_front;

// return isnan( Fresnel_transmitted_back ) ? float3( 1, 0, 0 ) : Fresnel_transmitted_back;
// return saturate( Fresnel_transmitted_front );

	return transmittance * backColor;
}

//----------------------------------------------------------------------

float4 render( in float2 _pixelPos, in float _sceneDistance, in float3 ro, in float3 rd, bool _useModel, out float _distance ) { 
	float2	res = castRay( ro, rd );
	float	t = res.x;
	float	m = res.y;
	_distance = t;
	if ( m <= -0.5 || t >= _sceneDistance ) {
		return float4( 0, 0, 0, 1 );
	}

	float3 pos = ro + t*rd;
	float3 wsNormal = calcNormal( pos );
	float3 wsView = -rd;
	float3 ref = reflect( rd, wsNormal );
        
	// material        
	float3	albedo = 0.45 + 0.4*cos( float3(0.08,0.05,0.1)*(m-1.0) );
	float	roughness = 0.5 + 0.5 * sin( 0.1 * (m-1.0) );
roughness = _GlassRoughness;
		

roughness *= roughness;	// More linear feel


//	if ( m < 1.5 ) {
//		// Floor
//		float f = abs( fmod( floor(5.0*pos.z) + floor(5.0*pos.x), 2.0 ) );
//		albedo = 0.4 + 0.1*f;
//		roughness = 0.4 + 0.6 * abs( f );
//	}

	// lighting        
	float	occ = calcAO( pos, wsNormal );
	float3	wsLight = normalize( float3(-0.6, 0.7, -0.5) );
	float3	lig2 = normalize(float3(-wsLight.x,0.0,-wsLight.z));	// For backlighting
	float3	lightIntensity = 1.2 * float3(1.0,0.85,0.55);
	float3	lightIntensity2 = 0.3;// * float3(1.0,0.85,0.55);

	float	amb = saturate( 0.5+0.5*wsNormal.y );
	float	LdotN = saturate( dot( wsNormal, wsLight ) );
	float	LdotN2 = saturate( dot( wsNormal, lig2 ) ) * saturate( 1.0-pos.y );
	float	dom = smoothstep( -0.1, 0.1, ref.y );
	float	fre = pow( saturate( 1.0+dot(wsNormal,rd) ), 2.0 );
	float	spe = pow( saturate( dot( ref, wsLight ) ), 16.0 );
        
	float	shadow = softshadow( pos, wsLight, 0.02, 2.5 );
	float	shadow_ref = softshadow( pos, ref, 0.02, 2.5 );
	dom *= shadow_ref;



	// Add rough diffuse model
	float3	dif2 = 0.0;
//	if ( _useModel ) {
//
//		roughness *= _DebugParm;
//		roughness = max( 1e-3, roughness );
//
//		float	shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN-0.2) ) );	// This removes shadowing on back faces
//				shadow2 = 1.0 - shadow2;
//				shadow2 = pow2( shadow2 );	// ^2
//				shadow2 = pow2( shadow2 );	// ^4
//				shadow2 = pow2( shadow2 );	// ^8
//				shadow2 = pow2( shadow2 );	// ^16
//				shadow2 = 1.0 - shadow2;
////				shadow2 *= saturate( 0.2 + 0.8 * LdotN );	// Larger L.N, eating into the backfaces
//
//		dif2 = ComputeDiffuseModel( wsLight, -rd, roughness, albedo ) * lightIntensity * shadow * LdotN;
//
//		// 2nd light
//		shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN2-0.2) ) );	// This removes shadowing on back faces
//		shadow2 = 1.0 - shadow2;
//		shadow2 = pow2( shadow2 );	// ^2
//		shadow2 = pow2( shadow2 );	// ^4
//		shadow2 = pow2( shadow2 );	// ^8
//		shadow2 = pow2( shadow2 );	// ^16
//		shadow2 = 1.0 - shadow2;
////		shadow2 *= saturate( 0.2 + 0.8 * LdotN2 );	// Larger L.N, eating into the backfaces
//
//		dif2 += ComputeDiffuseModel( lig2, -ref, roughness, albedo ) * lightIntensity2 * shadow * LdotN2;
//	}

	float3	wsGeometricNormal = wsNormal;
	float	opacity = _GlassOpacity;	// @TODO: modulation over surface...



//albedo = float3( 1, 0.5, 0.2 );


// 	float3	diffuseAlbedo = albedo * opacity;					// The more transparent it gets, the less diffuse lighting is visible
// 	float3	transmittanceColor = (1.0 - opacity) * albedo;		// The more transparent it gets, the more the diffuse albedo is used as a transmittance color
	float3	diffuseAlbedo = albedo;					// The more transparent it gets, the less diffuse lighting is visible
	float3	transmittanceColor = albedo;		// The more transparent it gets, the more the diffuse albedo is used as a transmittance color

	// NOTE: We could split R, G and B into 3 distinct rays depending on a RGB F0 but we need a single value here
	float	IOR = Fresnel_IORFromF0( _GlassF0 );				// Artists control the glass's IOR by changing the "F0" value which is the "specular reflectance for dielectrics" (a single color for the entire material)

	// Sample scene based on roughness
	float	mipLevel = 4*roughness;

//mipLevel = 4;

	float2	UV = _pixelPos / iResolution.xy;
			UV -= exp2( mipLevel-1 ) / iResolution.y;
	float	backgroundDistance = _TexSource.SampleLevel( LinearClamp, float3( UV, 1 ), mipLevel ).x;

//return float4( 0.1 * backgroundDistance.xxx, 0 );


	// Compute Fresnel reflectance
	float3	H = normalize( wsLight + wsView );
	float	HdotN = saturate( dot( H, wsNormal ) );
	float	Fresnel_specular = FresnelAccurate( IOR, HdotN );
	float	Fresnel_diffuse = 1.0 - Fresnel_specular;	// What is not specularly reflected

	// Compute specular reflectance
	float	exponent = exp2( 10 * (1.0 - roughness) );	// Phong lobe
	float	BRDF_spec = ((2+exponent) / PI) * pow( HdotN, exponent );
	float3	L_specular = Fresnel_specular * lightIntensity * shadow * BRDF_spec * LdotN;

	// Compute diffuse reflectance
	float3	BRDF_diffuse = diffuseAlbedo / PI;
	float3	L_diffuse = Fresnel_diffuse * lightIntensity * shadow * BRDF_diffuse * LdotN;

	// Compute rough transmittance
	float3	wsNormal_Back = wsNormal - 2.0 * dot( wsNormal, wsGeometricNormal ) * wsGeometricNormal;

//	float	MaxColorThickness = lerp( 1.0, _GlassThickness / (1e-4 + _GlassColoring*_GlassColoring), saturate( 2.0 * (IOR-1.0) ) );
	float	MaxColorThickness = _GlassThickness / (1e-4 + _GlassColoring*_GlassColoring);

	float3	L_transmitted = ComputeGlassColor( pos, wsNormal, wsView, backgroundDistance, transmittanceColor, _GlassCurvature, _GlassThickness, MaxColorThickness, roughness, IOR );

//L_transmitted = 0.0;
//transmittance = 1;

//return float4( transmittance, 0.0 );
//return float4( L_transmitted, 0.0 );

	float3	col = lerp( L_transmitted, L_diffuse, opacity );	// Either use diffusely-reflected light, or transmitted light
			col += L_specular;


//col = L_transmitted * transmittance;
//col = Fresnel_specular;
//col = Fresnel_diffuse;
//col = L_diffuse;


/*	float3 lin = float3(0,0,0);
	lin += lightIntensity * shadow * LdotN;
	lin += lightIntensity * shadow * spe * LdotN;
	lin += 0.20*amb*float3(0.50,0.70,1.00)*occ;
	lin += 0.30*dom*float3(0.50,0.70,1.00)*occ;
	lin += lightIntensity2 * LdotN2*float3(0.25,0.25,0.25) * occ;
	lin += 0.40*fre*float3(1.00,1.00,1.00)*occ;

	float3	col = 0;//float3(0.7, 0.9, 1.0) + rd.y*0.8;	// Sky color
	col = albedo * lin;

	col += dif2;
*/


	// Add some fog
	col = lerp( col, float3(0.8,0.9,1.0), 1.0-exp( -0.002*t*t ) );

if ( _DebugFlags & 8 ) {
	col = dif2;
} else if ( _DebugFlags & 1 ) {
	col = roughness;
}

	return float4( col, 0.0 );
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;

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


	float	backgroundDistance = _TexSource[uint3( _In.__Position.xy, 1 )].x;
	float3	backgroundColor = _TexSource[uint3( _In.__Position.xy, 0 )].xyz;

	float	distance;
	float4	Color = render( _In.__Position.xy, backgroundDistance, ro, rd, useModel, distance );

	return backgroundColor.xyz * Color.w + Color.xyz;
}
