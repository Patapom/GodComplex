
cbuffer CB_Light : register(b2) {
	float3		_AreaLightX;
	float		_AreaLightScaleX;
	float3		_AreaLightY;
	float		_AreaLightScaleY;
	float3		_AreaLightZ;
	float		_AreaLightDiffusion;
	float3		_AreaLightT;
	float		_AreaLightIntensity;
	float4		_AreaLightTexDimensions;
	float3		_ProjectionDirectionDiff;	// Closer to portal when diffusion increases
};

Texture2D< float4 >	_TexAreaLightSAT : register(t0);
Texture2D< float4 >	_TexAreaLightSATFade : register(t1);
Texture2D< float4 >	_TexAreaLight : register(t4);
Texture2D< float2 >	_TexBRDFIntegral : register(t5);
Texture2D< float4 >	_TexFalseColors : register(t6);

static const float4	AREA_LIGHT_TEX_DIMENSIONS = float4( 256.0, 256.0, 1.0/256.0, 1.0/256.0 );

///////////////////////////////////////////////////////////////////////////////////////////////
// bmayaux (2015-01-19) Area lights support
// 
// Area lights are a big part of next-gen rendering but are extremely difficult to render correctly
//	mainly because the lighting integral requires many samples, both from the BRDF and the area light itself.
// 
// The true lighting equation we need to resolve is:
// 
//	L(x,Wo) = Integral_over_A[ f(Wo, Wi) * L(x,Wi) * (Wi.N) dWi ]
// 
//		x is the surface position we're computing the radiance for
//		Wo is the outgoing view direction
//		Wi is the incoming light direction
//		L(x,Wo) is the outgoing radiance in the view direction
//		L(x,Wi) is the incoming radiance in the light direction
//		A is the area light's surface, clipped by the surface's plane
//		f(Wo, Wi) is the surface's BRDF
//		N is the surface's normal at x
//		dWi is the tiny solid angle covered by the incoming radiance
// 
// To solve this integral we usually numerically integrate by taking many samples on the surface of the area light
//	but this is completely out of question for our purpose so instead, we make drastic simplifcations of this integral.
// 
// As usual, we split the BRDF into 2 parts: diffuse and specular.
// 
//	f_d(Wo, Wi) = Rho_d / PI
//	f_s(Wo, Wi) = Ward( Wo, Wi )
// 
// Then f(Wo,Wi) = F'( Wo, Wi, N ) * f_d(Wo,Wi) + F( Wo, Wi, N ) * f_s(Wo,Wi)
// 
//	F( Wo, Wi, N ) is the specular Fresnel coefficient
//	F'( Wo, Wi, N ) is the diffuse Fresnel coefficient (which is not exactly 1-F( Wo, Wi, N ) but almost)
// 
// The integral becomes:
// 
//	L_d(x,Wo) = Rho_d / PI * Integral_over_A[ F'( Wo, Wi, N ) * L(x,Wi) * (Wi.N) dWi ]		(1) diffuse part
//	L_s(x,Wo) = Integral_over_A[ F( Wo, Wi, N ) * f_s(Wo,Wi) * L(x,Wi) * (Wi.N) dWi ]		(2) specular part
// 
// We can further simplify:
// 
// (1) becomes:	L_d(x,Wo) ~= F'( Wo, Waverage, N ) * Rho_d / PI * (Waverage.N) * Integral_over_A[ L(x,Wi) dWi ]
// 
//		Waverage is the average incoming direction for the whole area light
// 
// (2) becomes:	L_s(x,Wo) ~= Integral_over_A[ F( Wo, Wi, N ) * f_s(Wo,Wi) * (Wi.N) dWi ] * Integral_over_A[ L(x,Wi) dWi ]
// 
// 
// All we need to do to accomplish this is a way to quickly compute:
//	1] The clipping of the area light by the surface's plane
//	2] Compute the integral of the radiance given by the visible part of the clipped area light
// 
// Part 2 is easily done using a Summed Area Table (SAT) whose sampling returns the integral of pixels within a specific
//	sub-rectangle of the texture.
// The texture coordinates of that sub-rectangle will be provided by the clipping routine as well as the aperture of the
//	BRDF's glossiness cone for the specular part (for the diffuse part, the entire hemisphere is used and the 
//	entire area light is considered each time).
// 

// Samples the area light given 2 UV coordinates
float3	SampleAreaLight( float2 _UV0, float2 _UV1, uint _SliceIndex, const bool _UseAlpha ) {

	float2	DeltaUV = 0.5 * (_UV1 - _UV0);
	float	RadiusUV = max( DeltaUV.x, DeltaUV.y );
	float	RadiusPixels = AREA_LIGHT_TEX_DIMENSIONS.x * RadiusUV;
	float	MipLevel = log2( 1e-4 + RadiusPixels );

	// Compute an approximation of the clipping of the projected disc and the area light:
	//	__
	//	  |  ...
	//	  |.
	//	 .|
	//	. |   
	//	.I+---+ C
	//	. |
	//	 .|
	//	  |.
	//	  |  ...
	//	__|
	//
	// We simply need to compute the distance between C, the center of projection
	//	and I, the closest position to C within the square area light and normalize
	//	it by the radius of the circle of confusion to obtain a nice, round fading
	//	zone when the circle exits the area light...
	float2	UVCenter = 0.5 * (_UV0 + _UV1);
	float2	SatUVCenter = saturate( UVCenter );

	float	Attenuation = saturate( length( UVCenter - SatUVCenter ) / (1e-3 + RadiusUV) );
			Attenuation = smoothstep( 1.0, 0.0, Attenuation );

	float4	Color = _SliceIndex != ~0U ? _TexAreaLight.SampleLevel( LinearWrap, SatUVCenter, MipLevel ) : 1.0;
	if ( _UseAlpha ) {
		Color.xyz *= Color.w;	// Diffuse uses alpha to allow smoothing out of the texture's borders...
	}

	return Attenuation * Color.xyz;
}

// Determinant of a 3x3 row-major matrix
float	MatrixDeterminant( float3 a, float3 b, float3 c ) {
	return	(a.x * b.y * c.z + a.y * b.z * c.x + a.z * b.x * c.y)
		-	(a.x * b.z * c.y + a.y * b.x * c.z + a.z * b.y * c.x);
}

// Compute the solid angle of a rectangular area perceived by a point
// The solid angle is computed by decomposing the rectangle into 2 triangles and each triangle's solid angle
//	is then computed via the equation given in http://en.wikipedia.org/wiki/Solid_angle#Tetrahedron
//
//	_lsPosition, the position viewing the rectangular area
//	_UV0, _UV1, the 2 UV coordinates defining the rectangular area of a canonical square in [-1,+1] in both x and y
//
float	RectangleSolidAngle( float3 _lsPosition, float2 _UV0, float2 _UV1 ) {
	float3	v0 = float3( 2.0 * _UV0.x - 1.0, 1.0 - 2.0 * _UV0.y, 0.0 ) - _lsPosition;
	float3	v1 = float3( 2.0 * _UV0.x - 1.0, 1.0 - 2.0 * _UV1.y, 0.0 ) - _lsPosition;
	float3	v2 = float3( 2.0 * _UV1.x - 1.0, 1.0 - 2.0 * _UV1.y, 0.0 ) - _lsPosition;
	float3	v3 = float3( 2.0 * _UV1.x - 1.0, 1.0 - 2.0 * _UV0.y, 0.0 ) - _lsPosition;

	float	lv0 = length( v0 );
	float	lv1 = length( v1 );
	float	lv2 = length( v2 );
	float	lv3 = length( v3 );

	float	dotV0V1 = dot( v0, v1 );
	float	dotV1V2 = dot( v1, v2 );
	float	dotV2V3 = dot( v2, v3 );
	float	dotV3V0 = dot( v3, v0 );
	float	dotV2V0 = dot( v2, v0 );

// Naïve formula with 2 atans
//	float	A0 = atan( -MatrixDeterminant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) );
//	float	A1 = atan( -MatrixDeterminant( v0, v2, v3 ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0) );
// 	return 2.0 * (A0 + A1);

	// But since atan(a)+atan(b) = atan( (a+b) / (1-ab) ) ...
 	float	Theta0 = -MatrixDeterminant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0);
 	float	Theta1 = -MatrixDeterminant( v0, v2, v3 ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0);
	return 2.0 * atan( (Theta0 + Theta1) / (1.0 - Theta0*Theta1) );
}

///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
// AREA LIGHT CLIPPING BY A PLANE
///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////

// Condition-less clipping routine that returns the result of the intersection of [P0,P1] with the given plane:
//	_ Return the intersection point if the segment cuts the plane
//	_ Return P0 if both P0 and P1 stand below the plane
//	_ Returns P1 if both P0 and P1 stand above the plane
//
float3	ClipSegment( float3 _P0, float3 _P1, float3 _PlanePosition, float3 _PlaneNormal ) {

	float3	V = _P1 - _P0;
	float3	D = _PlanePosition - _P0;
	float	d = dot( D, _PlaneNormal );
	float	t = d / dot( V, _PlaneNormal );

	// The actual code we want to execute is:
	// 	if ( d > 0.0 || t >= 0.0 )
	// 		return _P0 + saturate( t - 1e-3 ) * V;	// There's an intersection
	// 	else
	// 		return _P1;								// Both points are above the plane, go straight to end point

	// But we can replace them with this condition-less code
	float	IsPositive_d = step( 0.0, d );			// d > 0
	float	IsPositive_t = 1.0 - step( t, 0.0 );	// t >= 0
	float	d_or_t = saturate( IsPositive_d + IsPositive_t );
	return lerp( _P1, _P0 + saturate( t - 1e-3 ) * V, d_or_t );
}

// Computes the potential UV clipping of the canonical area light by the surface's normal
// The algorithm is "quite cheap" as it doesn't use any condition and proceeds like this:
//		1) Depending on the orientation of the plane's normal, we choose P0 as the farthest point away from the plane
//		2) We build P1, P2 and P3 from P0
//			=> the P0,P1,P2,P3 quad will either be CW or CCW but that does not matter
//		3) We browse the quad in the P0->P1->P2->P3 orientation and keep the min/max of the clipped segments each time
//		4) We browse the quad in the P3->P2->P1->P0 orientation and keep the min/max of the clipped segments each time
//		5) We transform the min/max back into UV0 and UV1
//
// This is a crude approximation of the clipped UV area but it's okay for our needs
// If you watch the solid angle of the clipped square as perceived by a surface, you will notice some
//	discontinuities in the sense of "area bounces" because of the min/max but these discontinuites
//	become less noticeable as they concur with the N.L becoming 0 at these locations
//
float4	ComputeAreaLightClipping( float3 _lsPosition, float3 _lsNormal ) {

	// Reverse Y so Positions are in the same orientation as UVs
	_lsPosition.y = -_lsPosition.y;
	_lsNormal.y = -_lsNormal.y;

	// Find the coordinates of the farthest point away from the plane
	float	X0 = 1.0 - 2.0 * step( _lsNormal.x, 0.0 );
	float	Y0 = 1.0 - 2.0 * step( _lsNormal.y, 0.0 );

	// Build the remaining quad coordinates by mirroring
	float3	P0 = float3( X0, Y0, 0.0 );
	float3	P1 = float3( -X0, Y0, 0.0 );
	float3	P2 = float3( -X0, -Y0, 0.0 );
	float3	P3 = float3( X0, -Y0, 0.0 );

	// Compute clipping of the square by browsing the contour in both CCW and CW
	// We keep the min/max of positions each time
	float2	Min = 1.0;
	float2	Max = -1.0;

	// CCW
	float3	P = ClipSegment( P0, P1, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	P = ClipSegment( P, P2, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	P = ClipSegment( P, P3, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	P = ClipSegment( P, P0, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	// CW
	P = ClipSegment( P0, P3, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	P = ClipSegment( P, P2, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	P = ClipSegment( P, P1, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	P = ClipSegment( P, P0, _lsPosition, _lsNormal );
	Min = min( Min, P.xy );	Max = max( Max, P.xy );

	// Finalize UVs
	return 0.5 * (1.0 + float4( Min, Max ));
}


///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
// AREA LIGHT SOLID ANGLE COMPUTATION
///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////

// Computes the 2 UVs and the solid angle perceived from a single point in world space (used for diffuse reflection)
// The area light is using the "diffusion" parameter to make the area light either behave as a directional light or
//	a window to the outside.
// 
// With this diffusion parameter, you can either obtain the clear projective pattern of a stained glass lit by the Sun,
//	or the soft appearance of a window letting the sky light enter.
// 
//	_lsPosition, the local space position of the surface watching the area light
//	_lsNormal, the local space normal of the surface
//	_ProjectionDirection, the projection vector, casting the "virtual light source" either very far from the area light (diffuse=0), or completely stuck to it (diffuse=1)
//	_ClippedUVs, the UVs of the potentially clipped area light
//	_ClippedAreaLightSolidAngle, the solid angle for the visible part of the area light
//
// Returns:
//	_UV0, _UV1, the 2 UVs coordinates where to sample the SAT
//	_ProjectedSolidAngle, an estimate of the perceived projected solid angle (i.e. cos(IncidentAngle) * dOmega)
//
float	ComputeSolidAngleDiffuse( float3 _lsPosition, float3 _lsNormal, float3 _ProjectionDirection, float4 _ClippedUVs, float _ClippedAreaLightSolidAngle, out float2 _UV0, out float2 _UV1 ) {

// 	const float3	lsPortal[2] = {
// 		float3( -1, +1, 0 ),		// Top left
// 		float3( +1, -1, 0 ),		// Bottom right
// 	};

	const float3	lsPortal[2] = {
		float3( 2.0 * _ClippedUVs.x - 1.0, 1.0 - 2.0 * _ClippedUVs.y, 0 ),		// Top left
		float3( 2.0 * _ClippedUVs.z - 1.0, 1.0 - 2.0 * _ClippedUVs.w, 0 ),		// Bottom right
	};

	// Compute the UV coordinates of the intersection of the virtual light source frustum with the portal's plane
	// The virtual light source is the portal offset by a given vector so it gets away from the portal along the -Z direction
	// This gives us:
	//	_ An almost directional thin brush spanning a few pixels when the virtual light source is far away (almost infinity) from the portal (diffusion = 0)
	//	_ An area that covers the entire portal when the virtual light source is right on the portal (diffusion = 1)
	//
	float2	lsIntersection[2] = { float2( 0.0, 0.0 ), float2( 0.0, 0.0 ) };
	[unroll]
	for ( uint Corner=0; Corner < 2; Corner++ ) {
		float3	lsVirtualPos = lsPortal[Corner] - _ProjectionDirection;	// This is the position of the corner of the virtual source
		float3	Dir = lsVirtualPos - _lsPosition;						// This is the pointing direction, originating from the source _wsPosition
		float	t = -_lsPosition.z / Dir.z;								// This is the distance at which we hit the physical portal's plane
		lsIntersection[Corner] = (_lsPosition + t * Dir).xy;			// This is the position on the portal's plane
	}

	// Retrieve the UVs
	_UV0 = 0.5 * (1.0 + float2( lsIntersection[0].x, -lsIntersection[0].y));
	_UV1 = 0.5 * (1.0 + float2( lsIntersection[1].x, -lsIntersection[1].y));

	// Compute the projected solid angle for the entire area light
//	float	SolidAngle = _ClippedAreaLightSolidAngle;

//	float2	ClippedUV0 = clamp( _UV0, _ClippedUVs.xy, _ClippedUVs.zw );
//	float2	ClippedUV1 = clamp( _UV1, _ClippedUVs.xy, _ClippedUVs.zw );
//	float	SolidAngle = lerp( normalize( _lsPosition ).z, _ClippedAreaLightSolidAngle, saturate( RectangleSolidAngle( _lsPosition, ClippedUV0, ClippedUV1 ) ) );
//	float	SolidAngle = RectangleSolidAngle( _lsPosition, _ClippedUVs.xy, _ClippedUVs.zw );

	float3	lsCenter = float3( _UV1.x + _UV0.x - 1.0, 1.0 - _UV1.y - _UV0.y, 0.0 );
	float3	lsPosition2Center = lsCenter - _lsPosition;		// Wi, the average incoming light direction
	float	r = length( lsPosition2Center );
			lsPosition2Center /= lsPosition2Center;


	// Compute the projected solid angle for the entire area light
	float	SolidAngle = lerp( 1.0, _ClippedAreaLightSolidAngle / (r * r), _AreaLightDiffusion );
//	float	SolidAngle = _ClippedAreaLightSolidAngle;


	return saturate( dot( _lsNormal, lsPosition2Center ) ) * SolidAngle;	// (N.Wi) * dWi
}

// Computes the 2 UVs and the solid angle perceived from a single point in world space watching the area light through a cone whose aperture depends on glossiness
//	_lsPosition, the local space position of the surface watching the area light
//	_lsNormal, the local space normal of the surface
//	_lsView, the view direction (usually, the main reflection direction)
//	_Gloss, the surface's gloss factor
//	_ClippedUVs, the UVs of the potentially clipped area light
//	_ClippedAreaLightSolidAngle, the solid angle for the visible part of the area light
//
// Returns:
//	_UV0, _UV1, the 2 UVs coordinates where to sample the SAT
//	_ProjectedSolidAngle, an estimate of the perceived projected solid angle (i.e. cos(IncidentAngle) * dOmega)
//
float	ComputeSolidAngleSpecular( float3 _lsPosition, float3 _lsNormal, float3 _lsView, float _Gloss, float4 _ClippedUVs, float _ClippedAreaLightSolidAngle, out float2 _UV0, out float2 _UV1 ) {

	// Bend view toward center of area light depending on glossiness
//	_lsView = normalize( lerp( -_lsPosition, _lsView, _Gloss ) );

	// Compute the gloss cone's aperture angle
	float	Roughness = 1.0 - _Gloss;
	float	HalfAngle = 0.0003474660443456835 + Roughness * (1.3331290497744692 - Roughness * 0.5040552688878546);	// cf. IBL.mrpr to see the link between roughness and aperture angle
	float	SinHalfAngle, CosHalfAngle;
	sincos( HalfAngle, SinHalfAngle, CosHalfAngle );
	float	TanHalfAngle = SinHalfAngle / CosHalfAngle;

	_UV0 = _UV1 = -1.0;
	if ( _lsView.z > 0.0 ) {
		return 0.0;
	}

	// Compute the intersection of the view with the are light's plane
	float	t = -_lsPosition.z / _lsView.z;
	float3	I = _lsPosition + t * _lsView;	// Intersection position on the plane

	// Compute the radius of the specular cone at hit distance
	float	Radius = TanHalfAngle * t;		// Correct radius at hit distance

	float	RadiusUV = 0.5 * Radius;
//			RadiusUV = lerp( RadiusUV, 1.0, smoothstep( 0.0, 1.0, RadiusUV ) );
//			RadiusUV = lerp( 1.0, RadiusUV, _Gloss );

	// Compute the UVs encompassing the cone's ellipsoid intersection with the area light plane
	float2	UVcenter = float2( 0.5 * (1.0 + I.x), 0.5 * (1.0 - I.y) );

	_UV0 = UVcenter - RadiusUV;
	_UV1 = UVcenter + RadiusUV;


	/////////////////////////////////
	// Compute the specular solid angle
	float	SolidAngle = 2.0 * PI * (1.0 - CosHalfAngle);		// Solid angle for the specular cone


	// Now, multiply by PI/4 to account for the fact we traced a square pyramid instead of a cone
	// (area of the base of the pyramid is 2x2 while area of the circle is PI)
//	SolidAngle *= 0.25 * PI;

//	float2	ClippedUV0 = clamp( _UV0, _ClippedUVs.xy, _ClippedUVs.zw );
//	float2	ClippedUV1 = clamp( _UV1, _ClippedUVs.xy, _ClippedUVs.zw );

	// Interpolate between a "directional light source" unit solid angle and the fully diffuse solid angle, clipped by the area light
//	SolidAngle = lerp( normalize( _lsPosition ).z, _ClippedAreaLightSolidAngle, SolidAngle / _ClippedAreaLightSolidAngle );
//	SolidAngle = lerp( 1.0, _ClippedAreaLightSolidAngle, saturate(SolidAngle / _ClippedAreaLightSolidAngle) );
	SolidAngle = _ClippedAreaLightSolidAngle;


	// Finally, we can compute the projected solid angle by dotting with the normal
	float	ProjectedSolidAngleSpecular = saturate( dot( _lsNormal, _lsView ) ) * SolidAngle;	// (N.Wi) * dWi
//			ProjectedSolidAngleSpecular *= saturate( -_lsView.z );								// Also fade based on V.N to remove grazing angles

	float	FresnelGrazing = 1.0 + _lsView.z;
			FresnelGrazing *= FresnelGrazing;
			FresnelGrazing *= FresnelGrazing;
			ProjectedSolidAngleSpecular *= 1.0 - FresnelGrazing;								// Also fade based on V.N to remove grazing angles

			// Assume radiance * 1/PI if the area light is largely diffusing
			ProjectedSolidAngleSpecular *= lerp( 1.0, INVPI, _AreaLightDiffusion );

// Difference with ComputeSolidAngleDiffuse() is that we take the center of the clipped UVs as main direction for dotting with normal, instead of center of target UVs (potentially directional)
// 			/////////////////////////////////
// 			// Compute the diffuse solid angle
// 			float	SolidAngleDiffuse = _ClippedAreaLightSolidAngle;
// 	
// 			float3	lsCenter = float3( _ClippedUVs.z + _ClippedUVs.x - 1.0, 1.0 - _ClippedUVs.y - _ClippedUVs.w, 0.0 );
// 			float3	lsPosition2Center = normalize( lsCenter - _lsPosition );											// Wi, the average incoming light direction
// 			float	ProjectedSolidAngleDiffuse = saturate( dot( _lsNormal, lsPosition2Center ) ) * SolidAngleDiffuse;	// (N.Wi) * dWi


// 	/////////////////////////////////
// 	// Lamely interpolate from "specular UVs" to "diffuse UVs" depending on gloss
// 	_UV0 = lerp( _ClippedUVs.xy, _UV0, _Gloss );
// 	_UV1 = lerp( _ClippedUVs.zw, _UV1, _Gloss );
// 	_UV1 = max( _UV0, _UV1 );




//ProjectedSolidAngleSpecular *= lerp( step( abs( I.x ), 1.0 ) * step( abs( I.y ), 1.0 ), 1.0, _AreaLightDiffusion );




	/////////////////////////////////
	// Build the result solid angle
	return ProjectedSolidAngleSpecular;
}


///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
// AREA LIGHT COMPUTE LIGHTING
///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////

struct SurfaceContext {
	float3	wsPosition;
	float3	wsNormal;
	float3	wsView;
	float3	diffuseAlbedo;
	float	roughness;
	float3	IOR;
	float	fresnelStrength;
};

void	ComputeAreaLightLighting( in SurfaceContext _Surface, uint _SliceIndex, float _Shadow, float2 _RadiusFalloffCutoff, out float3 _RadianceDiffuse, out float3 _RadianceSpecular ) {

	_RadianceDiffuse = _RadianceSpecular = 0.0;

	// 1] =========== Reconstruct area light information from fragmented data from the light context ===========
	float3	wsPosition = _Surface.wsPosition;

	float3	wsLightPos = _AreaLightT;
	float3	wsLightZ = _AreaLightZ;
	float3	wsCenter2Position = wsPosition - wsLightPos;
	if ( dot( wsCenter2Position, wsLightZ ) <= 0.0 || _Shadow < 1e-6 ) {
		return;	// We're standing behind the area light...
	}

	float3	wsLightX = _AreaLightX;
	float3	wsLightY = cross( wsLightZ, wsLightX );	// No need to normalize

	float	SizeX = _AreaLightScaleX;
	float	SizeY = _AreaLightScaleY;

	float	Diffusion = _AreaLightDiffusion;

	float3	ProjectionDirection = _ProjectionDirectionDiff;

	// 2] =========== Transform position & normal into local space ===========
	float3	wsNormal = _Surface.wsNormal;
	float3	wsView = _Surface.wsView;
	float3	wsReflectedView = reflect( -wsView, wsNormal );			// There is a negative sign because we need the view pointing toward the surface here!

	float3	lsPosition = float3(dot( wsCenter2Position, wsLightX ),	// Transform world position into local area light space
								dot( wsCenter2Position, wsLightY ),
								dot( wsCenter2Position, wsLightZ ) );

	float3	lsNormal = float3(	dot( wsNormal, wsLightX ),			// Transform world normal into local area light space
								dot( wsNormal, wsLightY ),
								dot( wsNormal, wsLightZ ) );

	float3	lsView = float3(	dot( wsReflectedView, wsLightX ),	// Transform reflected view direction into local area light space
								dot( wsReflectedView, wsLightY ),
								dot( wsReflectedView, wsLightZ ) );

	// Account for scaling
	float2	InvScale = 1.0 / float2( SizeX, SizeY );
	lsView.xy *= InvScale;
	lsPosition.xy *= InvScale;

	// Once we get there, lsPosition, lsView and lsNormal all are in local space, in front of the canonical area light square:
	//
	//	(-1,+1)			 Y			(+1,+1)
	//			o--------^--------o
	//			|        |        |
	//			|        |        |
	//			|        |        |
	//			|        |        |
	//			|        o -------> X
	//			|       Z         |
	//			|                 |
	//			|                 |
	//			|                 |
	//			o-----------------o
	//	(-1,-1)						(+1,-1)
	//
	//

	// 3] =========== Compute potential area light clipping by the surface's plane ===========
	float4	ClippedUVs = ComputeAreaLightClipping( lsPosition, lsNormal );
	float2	DeltaUVs = ClippedUVs.zw - ClippedUVs.xy;
	if ( any( DeltaUVs < 1e-4 ) ) {
		return;	// The light is fully clipped so there's no use going any further...
	}
	float	SolidAngle = RectangleSolidAngle( lsPosition, ClippedUVs.xy, ClippedUVs.zw );

	// 4] =========== Compute diffuse & specular solid angles ===========
	float2	UV0_diffuse, UV1_diffuse;
	float	SolidAngle_diffuse = ComputeSolidAngleDiffuse( lsPosition, lsNormal, ProjectionDirection, ClippedUVs, SolidAngle, UV0_diffuse, UV1_diffuse );

 	float	Gloss = pow( 1.0 - _Surface.roughness, 0.5 );	// More linear appearance... To be defined (I prefer that one, too bad it costs more :( !)
//	float	Gloss = 1.0 - pow( _Surface.roughness, 2.0 );	// More linear appearance... To be defined

	float2	UV0_specular, UV1_specular;
	float	SolidAngle_specular = ComputeSolidAngleSpecular( lsPosition, lsNormal, lsView, Gloss, ClippedUVs, SolidAngle, UV0_specular, UV1_specular );

	// 5] =========== Compute the integration ===========
//	float	AreaLightSliceIndex = _Light.m_AreaLightSlice;
//	float3	ShadowedLightColor = _Light.m_shadow * _Light.m_color;

	float3	ShadowedLightColor = _Shadow * _AreaLightIntensity;

		// 5.1] ----- Compute Fresnel -----
	float	VdotN = saturate( dot( wsView, wsNormal ) );
	float3	FresnelSpecular = FresnelDielectric( _Surface.IOR, VdotN, _Surface.fresnelStrength );
	float3	FresnelDiffuse = 1.0 - FresnelSpecular;	// Simplify a lot! Don't use Disney's Fresnel as it needs a light direction and we can't provide one here...

// _RadianceDiffuse = FresnelDiffuse;
// return;

		// 5.2] ----- Diffuse -----
	float3	Irradiance_diffuse = SampleAreaLight( UV0_diffuse, UV1_diffuse, _SliceIndex, false );
			Irradiance_diffuse *= ShadowedLightColor;

	float3	IntegralBRDF_diffuse = _Surface.diffuseAlbedo * FresnelDiffuse * SolidAngle_diffuse;

	_RadianceDiffuse = Irradiance_diffuse * IntegralBRDF_diffuse;

		// 5.3] ----- Specular -----
	float3	Irradiance_specular = SampleAreaLight( UV0_specular, UV1_specular, _SliceIndex, false );
			Irradiance_specular *= ShadowedLightColor;

// 	// Here, I must admit I'm taking a HUUUGE shortcut since I don't have a proper solution for the BRDF's integral yet and don't have time to find one...
// 	// What I'm planning to do in a near future (or, in video-games industry's terms: never) is to write a bunch of integral values for various angles and roughnesses in a table
// 	//	and approximate that table using a simplified analytical formula, a bit like what they did in http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
// 	//	which was later simplified by Krzysztof Narkowicz (https://knarkowicz.wordpress.com/2014/12/27/analytical-dfg-term-for-ibl/).
// 	//
//	float3	IntegralBRDF_specular = INVPI * FresnelSpecular * SolidAngle_specular;	// Woohoo! Why not?! :)
//
// 	_RadianceSpecular = Irradiance_specular * IntegralBRDF_specular;

	// Okay so I did it: I computed the pre-integrated BRDF table and I'm using a texture (for now) to scale/bias the irradiance...
	// Still need to approximate that texture using polynomials or something...
	float2	PreIntegratedBRDF = _TexBRDFIntegral.SampleLevel( LinearClamp, float2( VdotN, _Surface.roughness ), 0.0 );

	float3	F0 = Fresnel_F0FromIOR( _Surface.IOR );

//float3	IntegralBRDF_specular = F0 * PreIntegratedBRDF.x * SolidAngle_specular + PreIntegratedBRDF.y;
float3	IntegralBRDF_specular = F0 * PreIntegratedBRDF.x * SolidAngle_specular + Gloss*PreIntegratedBRDF.y;	// Remove ambient term when totally rough



/////////////
/////////////
/////////////
	// Dull out specular in directional mode
	float3	wsLightDir = normalize( -_ProjectionDirectionDiff );	// Toward light
	float3	wsHalf = normalize( wsLightDir + wsView );
	float	NdotH = pow( saturate( dot( wsNormal, wsHalf ) ), 1.0-Diffusion );
//	float	NdotH = pow( saturate( dot( wsNormal, wsHalf ) ), 1e-6 + 1000.0 * (1.0 - Diffusion) );
//	float	NdotH = saturate( dot( wsNormal, wsHalf ) );


/////////////
/////////////
/////////////




	_RadianceSpecular = Irradiance_specular * IntegralBRDF_specular;

//_RadianceSpecular = NdotH;

//_RadianceSpecular = lsView;

	// 6] =========== Attenuate with distance ===========
	float	Distance2Light = length( wsCenter2Position );
// 	float	Attenuation = 1.0 / (Distance2Light*Distance2Light)
// 						* smoothstep( _RadiusFalloffCutoff.y, _RadiusFalloffCutoff.x, Distance2Light );	// Now with forced cutoff smoothed out like this (so we keep the physically correct 1/r² but can nonetheless artificially attenuate early)
	float	Attenuation = smoothstep( _RadiusFalloffCutoff.y, _RadiusFalloffCutoff.x, Distance2Light );	// Now with forced cutoff smoothed out like this (so we keep the physically correct 1/r² but can nonetheless artificially attenuate early)

	_RadianceDiffuse *= Attenuation;
	_RadianceSpecular *= Attenuation;
}
