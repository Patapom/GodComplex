
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
Texture2D< float >	_TexGloss : register(t7);
Texture2D< float3 >	_TexNormal : register(t8);

static const float4	AREA_LIGHT_TEX_DIMENSIONS = float4( 256.0, 256.0, 1.0/256.0, 1.0/256.0 );

#include "Ward.hlsl"

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

// Samples the area light given 2 local coordinates
float3	SampleAreaLight( float2 _lsPosMin, float2 _lsPosMax, float2 _AreaLightScale, uint _SliceIndex, const bool _UseAlpha ) {

	// Transform local space coordinates into UV coordinates
	float2	InvScale = 1.0 / _AreaLightScale;
	float2	UV0 = InvScale * float2( _lsPosMin.x, _lsPosMax.y );
			UV0 = 0.5 * float2( 1.0 + UV0.x, 1.0 - UV0.y );
	float2	UV1 = InvScale * float2( _lsPosMax.x, _lsPosMin.y );
			UV1 = 0.5 * float2( 1.0 + UV1.x, 1.0 - UV1.y );

	// Determine mip level
	float2	DeltaUV = 0.5 * (UV1 - UV0);
	float	RadiusPixels = AREA_LIGHT_TEX_DIMENSIONS.x * 0.5 * (DeltaUV.x + DeltaUV.y);
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
	float2	UVCenter = 0.5 * (UV0 + UV1);
	float2	SatUVCenter = saturate( UVCenter );
// 	float	RadiusUV = max( DeltaUV.x, DeltaUV.y );
// 	float	Attenuation = smoothstep( 1.0, 0.0, saturate( length( (UVCenter - SatUVCenter) / (1e-3 + RadiusUV) ) ) );
	float	Attenuation = smoothstep( 1.0, 0.0, saturate( length( (UVCenter - SatUVCenter) / (1e-3 + DeltaUV) ) ) );	// That one accounts for anisotropy

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
//	_lsPosition, the position viewing the rectangular area
//	_lsPosMin, _lsPosMax, the coordinates defining the clipped rectangular area of the light (axis-aligned)
//
float	RectangleSolidAngle( float3 _lsPosition, float2 _lsPosMin, float2 _lsPosMax ) {
#if 1
	// This formula is an approximation using the solid angle subtended by a cone (https://en.wikipedia.org/wiki/Solid_angle#Cone.2C_spherical_cap.2C_hemisphere)
	// The idea is to find theta, the cone's half angle:
	//
	//								*
	//							  /a|lpha
	//							/	|
	//						  /		|
	//						/		|
	//					  /			|
	//		 N  ^	    /			|
	//			|     /			   b|eta
	//			|   /2theta  ------	*
	//			| /  -----
	//	--------*---------------------
	//
	// We can find cos(alpha) and cos(beta) easily since they are the dot product of the vectors pointing toward (for example) the top and bottom of the area light
	// We know that theta = (PI - alpha - beta) / 2
	// So:
	//	cos( theta ) = cos( PI/2 - (alpha + beta) / 2 ) = sin( alpha / 2 + beta / 2 ) = sin( alpha / 2 ) * cos( beta / 2 ) + sin( beta / 2 ) * cos( alpha / 2 )
	//
	// The formula for half angles is:
	//	cos( alpha / 2 ) = sqrt( (1 + cos( alpha )) / 2 )
	//	sin( alpha / 2 ) = sqrt( (1 - cos( alpha )) / 2 )
	//
	// So:
	//	cos( theta ) = sqrt( (1 - cos( alpha )) * (1 + cos( beta )) / 4 ) + sqrt( (1 - cos( beta )) * (1 + cos( alpha )) / 4 )
	//
	// Using Omega = 2PI*(1-cos(theta)) we then find the solid angle subtended by the cone.
	// We compute the average of 2 solid angles (1 along the vertical and one along the horizontal) so we can account for elongated area lights.
	//
	float2	ClippedPos = clamp( _lsPosition.xy, _lsPosMin, _lsPosMax );
	float	CosAlpha_x = normalize( float3( _lsPosMax.x, ClippedPos.y, -1e-2 ) - _lsPosition ).x;
	float	CosBeta_x = normalize( _lsPosition - float3( _lsPosMin.x, ClippedPos.y, -1e-2 ) ).x;
	float	CosAlpha_y = normalize( float3( ClippedPos.x, _lsPosMax.y, -1e-2 ) - _lsPosition ).y;
	float	CosBeta_y = normalize( _lsPosition - float3( ClippedPos.x, _lsPosMin.y, -1e-2 ) ).y;

	float	CosTheta_x = sqrt( 0.25 * (1.0 - CosAlpha_x) * (1.0 + CosBeta_x) )
					   + sqrt( 0.25 * (1.0 + CosAlpha_x) * (1.0 - CosBeta_x) );

	float	CosTheta_y = sqrt( 0.25 * (1.0 - CosAlpha_y) * (1.0 + CosBeta_y) )
					   + sqrt( 0.25 * (1.0 + CosAlpha_y) * (1.0 - CosBeta_y) );

	// Normally...
	// float	SolidAngle_x = 2.0 * PI * (1.0 - CosTheta_x );
	// float	SolidAngle_y = 2.0 * PI * (1.0 - CosTheta_y );
	// return 0.5 * (SolidAngle_x + SolidAngle_y);
	//
	// Which simplifies into...
	return PI * (2.0 - CosTheta_x - CosTheta_y);

#else
	// Here, the solid angle is computed by decomposing the rectangle into 2 triangles and each triangle's solid angle
	//	is then computed via the equation given in http://en.wikipedia.org/wiki/Solid_angle#Tetrahedron
	float3	v0 = float3( _lsPosMin.x, _lsPosMin.y, 0.0 ) - _lsPosition;	// Bottom left
	float3	v1 = float3( _lsPosMax.x, _lsPosMin.y, 0.0 ) - _lsPosition;	// Bottom right
	float3	v2 = float3( _lsPosMax.x, _lsPosMax.y, 0.0 ) - _lsPosition;	// Top right
	float3	v3 = float3( _lsPosMin.x, _lsPosMax.y, 0.0 ) - _lsPosition;	// Top left

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
// 	float	A0 = atan( -MatrixDeterminant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) );
// 	float	A1 = atan( -MatrixDeterminant( v2, v3, v0 ) / (lv2+lv3+lv0 + lv0*dotV2V3 + lv3*dotV2V0 + lv2*dotV3V0) );
// 	float	A0 = atan( abs( MatrixDeterminant( v0, v1, v2 ) ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) );
// 	float	A1 = atan( abs( MatrixDeterminant( v0, v2, v3 ) ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0) );
// 	A0 = A0 < 0.0 ? PI + A0 : A0;
// 	A1 = A1 < 0.0 ? PI + A1 : A1;

	// This one seems to be "working"...
 	float	A0 = PI * frac( atan( abs( MatrixDeterminant( v0, v1, v2 ) ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) ) / PI );
 	float	A1 = PI * frac( atan( abs( MatrixDeterminant( v2, v3, v0 ) ) / (lv2+lv3+lv0 + lv0*dotV2V3 + lv3*dotV2V0 + lv2*dotV3V0) ) / PI );
	return A0 + A1;

	// This one has singularities :(
//  	float	A0 = 0.5*PI * frac( atan( abs( MatrixDeterminant( v0, v1, v2 ) ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) ) * 2.0 / PI );
//  	float	A1 = 0.5*PI * frac( atan( abs( MatrixDeterminant( v2, v3, v0 ) ) / (lv2+lv3+lv0 + lv0*dotV2V3 + lv3*dotV2V0 + lv2*dotV3V0) ) * 2.0 / PI );
// 	return 2.0 * (A0 + A1);

	// But since atan(a)+atan(b) = atan( (a+b) / (1-ab) ) ...
	// DOESN'T WORK WITH LARGE AREA LIGHTS!!! :(
	float	Theta0 = -MatrixDeterminant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0);
	float	Theta1 = -MatrixDeterminant( v0, v2, v3 ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0);
//	return 2.0 * atan( (Theta0 + Theta1) / (1.0 - Theta0*Theta1) );
	float	temp = 2.0 * atan( (Theta0 + Theta1) / (1.0 - Theta0*Theta1) );
	return temp < 0.0 ? 2.0 * PI + temp : temp;
#endif
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
// 	float	IsPositive_d = step( 0.0, d );			// d > 0
// 	float	IsPositive_t = 1.0 - step( t, 0.0 );	// t >= 0
//	float	d_or_t = saturate( IsPositive_d + IsPositive_t );
	float	d_or_t = d > 0.0 || t >= 0.0 ? 1.0 : 0.0;
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
float4	ComputeAreaLightClipping( float3 _lsPosition, float3 _lsNormal, float2 _AreaLightScale ) {

	// Find the coordinates of the farthest point away from the plane
	float	X0 = _AreaLightScale.x * (_lsNormal.x < 0.0 ? -1.0 : 1.0);
	float	Y0 = _AreaLightScale.y * (_lsNormal.y < 0.0 ? -1.0 : 1.0);

	// Build the remaining quad coordinates by mirroring
	float3	P0 = float3( X0, Y0, 0.0 );
	float3	P1 = float3( -X0, Y0, 0.0 );
	float3	P2 = float3( -X0, -Y0, 0.0 );
	float3	P3 = float3( X0, -Y0, 0.0 );

	// Compute clipping of the square by browsing the contour in both CCW and CW
	// We keep the min/max of positions each time
	float2	Min = 100.0;
	float2	Max = -100.0;

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

	// Pack coordinates
	return float4( Min, Max );
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
//	_lsClippedPositions, the min/max positions of the potentially clipped area light
//	_ClippedAreaLightSolidAngle, the solid angle for the visible part of the area light
//
// Returns:
//	_lsPositionMin, _lsPositionMax, the 2 coordinates where to sample the area light
//	_ProjectedSolidAngle, an estimate of the perceived projected solid angle (i.e. cos(IncidentAngle) * dOmega)
//
float	ComputeSolidAngleDiffuse( float3 _lsPosition, float3 _lsNormal, float3 _ProjectionDirection, float4 _lsClippedPositions, float _ClippedAreaLightSolidAngle, out float2 _lsPositionMin, out float2 _lsPositionMax ) {
	const float3	lsPortal[2] = {
		float3( _lsClippedPositions.xy, 0 ),	// Bottom left
		float3( _lsClippedPositions.zw, 0 ),	// Top right
	};

	// Compute the coordinates of the intersection of the virtual light source frustum with the portal's plane
	// The virtual light source is the portal offset by a given vector so it gets away from the portal along the -Z direction
	// This gives us:
	//	_ An almost directional thin brush spanning a few pixels when the virtual light source is far away (almost infinity) from the portal (diffusion = 0)
	//	_ An area that covers the entire portal when the virtual light source is right on the portal (diffusion = 1)
	//
	float2	lsIntersection[2] = { float2( 0.0, 0.0 ), float2( 0.0, 0.0 ) };
	[unroll]
	for ( uint Corner=0; Corner < 2; Corner++ ) {
		float3	lsVirtualPos = lsPortal[Corner] - _ProjectionDirection;	// This is the position of the corner of the virtual source
		float3	Dir = lsVirtualPos - _lsPosition;						// This is the pointing direction, originating from the source position
		float	t = -_lsPosition.z / Dir.z;								// This is the distance at which we hit the physical portal's plane
		lsIntersection[Corner] = (_lsPosition + t * Dir).xy;			// This is the position on the portal's plane
	}

	// Retrieve the intersection positions
	_lsPositionMin = lsIntersection[0];
	_lsPositionMax = lsIntersection[1];

	// Formerly, I simply used the center of the intersection positions to derive an average light vector to compute the L.N
	// The problem with that approach is that it tends to show a bulge of luminance located around the center of the light (this shows a lot on elongated lights!)
//	float3	lsCenter = float3( 0.5 * (_lsPositionMin + _lsPositionMax), 0.0 );
//			lsCenter.xy = clamp( lsCenter.xy, _lsClippedPositions.xy, _lsClippedPositions.zw );
//	float3	lsCenter = float3( clamp( _lsPosition.xy, _lsPositionMin, _lsPositionMax ), 0.0 );

	// Now we're looking for an average light direction to compute the L.N term
	//	• We will find the "average light position" we need along a line that is parallel to the intersection of the surface and light planes.
	//	• This line will pass through an average position between the 4 corners of the min/max rectangle (i.e. _lsPositionMin, _lsPositionMax and their combination)
	//	• The segment of that line will be bound by the min/max positions of the min/max rectangle along the line
	//
	//		 ______
	//		|      | Area light
	//		|----*-| <= Average light position along the "average line" which is parallel to the intersection between the area and surface planes
	//		|______|
	//	   /      . \ <== the dot is _lsPosition on the surface
	//	  /__________\ Surface
	//
	#if 1
		// It turns out using only the min and max corners gives pretty much the same results as the 4 corners version so here we go!
		// Find the line direction of the intersection between the surface and the light planes
		float3	LineOrtho = normalize( float3( _lsNormal.x, _lsNormal.y, 1e-4 ) );	// This is the orthogonal direction to the line direction
		float3	LineDir = float3( -LineOrtho.y, LineOrtho.x, LineOrtho.z );			// This is the direction of the intersection line between surface and light

		// Find the average "vertical" displacement along the direction orthogonal to the line
		float2	dotsY = float2( dot( LineOrtho.xy, _lsPositionMin ), dot( LineOrtho.xy, _lsPositionMax ) );
		float	CenterY = 0.5 * (dotsY.x + dotsY.y);

		// Find the min/max displacements along the direction of the line
		float2	dotsX = float2( dot( LineDir.xy, _lsPositionMin ), dot( LineDir.xy, _lsPositionMax ) );
		float	centerX = clamp( dot( LineDir.xy, _lsPosition.xy ), min( dotsX.x, dotsX.y ), max( dotsX.x, dotsX.y ) );
	#else
		// More expensive "4 corners" version
		// Find the line direction of the intersection between the surface and the light planes
		float3	LineOrtho = normalize( float3( _lsNormal.x, _lsNormal.y, 1e-4 ) );	// This is the orthogonal direction to the line direction
		float3	LineDir = float3( -LineOrtho.y, LineOrtho.x, LineOrtho.z );

		// Find the average "vertical" displacement along the direction orthogonal to the line
		float4	dotsY = float4( dot( LineOrtho.xy, _lsPositionMin ), dot( LineOrtho.xy, _lsPositionMax ), dot( LineOrtho.xy, float2( _lsPositionMin.x, _lsPositionMax.y ) ), dot( LineOrtho.xy, float2( _lsPositionMax.x, _lsPositionMin.y ) ) );

	//dotsY.zw = dotsY.xy;

		float	CenterY = 0.25 * (dotsY.x + dotsY.y + dotsY.z + dotsY.w);

		// Find the min/max displacements along the direction of the line
		float4	dotsX = float4( dot( LineDir.xy, _lsPositionMin ), dot( LineDir.xy, _lsPositionMax ), dot( LineDir.xy, float2( _lsPositionMin.x, _lsPositionMax.y ) ), dot( LineDir.xy, float2( _lsPositionMax.x, _lsPositionMin.y ) ) );

	//dotsX.zw = dotsX.xy;

		float2	MinX2 = min( dotsX.xy, dotsX.zw );
		float	MinX = min( MinX2.x, MinX2.y );
		float2	MaxX2 = max( dotsX.xy, dotsX.zw );
		float	MaxX = max( MinX2.x, MinX2.y );
		float	centerX = clamp( dot( LineDir.xy, _lsPosition.xy ), MinX, MaxX );
	#endif

	// Compute an average center on the line
	float3	lsCenter = centerX * LineDir + CenterY * LineOrtho;

	// Compute the projected solid angle for the entire area light
	float3	lsPosition2Center = lsCenter - _lsPosition;		// Wi, the average incoming light direction
	float	r = length( lsPosition2Center );
			lsPosition2Center *= r > 1e-6 ? 1.0 / r : 0.0;

	// Compute the projected solid angle for the entire area light
//	float	SolidAngle = lerp( 1.0, _ClippedAreaLightSolidAngle / (r * r), _AreaLightDiffusion );
//	float	SolidAngle = lerp( 1.0, _ClippedAreaLightSolidAngle, _AreaLightDiffusion );
	float	SolidAngle = _ClippedAreaLightSolidAngle;

	return saturate( dot( _lsNormal, lsPosition2Center ) ) * SolidAngle;	// (N.Wi) * dWi
}

// Computes the 2 UVs and the solid angle perceived from a single point in world space watching the area light through a cone whose aperture depends on glossiness
//	_lsPosition, the local space position of the surface watching the area light
//	_lsNormal, the local space normal of the surface
//	_lsView, the view direction (usually, the main reflection direction)
//	_Roughness, the surface's roughness factor
//	_lsClippedPositions, the UVs of the potentially clipped area light
//	_ClippedAreaLightSolidAngle, the solid angle for the visible part of the area light
//
// Returns:
//	_lsPositionMin, _lsPositionMax, the 2 coordinates where to sample the area light
//	_ProjectedSolidAngle, an estimate of the perceived projected solid angle (i.e. cos(IncidentAngle) * dOmega)
//
float	ComputeSolidAngleSpecular( float3 _lsPosition, float3 _lsNormal, float3 _lsView, float _Roughness, float4 _lsClippedPositions, float _ClippedAreaLightSolidAngle, inout float2 _lsPositionMin, inout float2 _lsPositionMax ) {

	// Compute the gloss cone's aperture angle
	float	HalfAngle = 0.0003474660443456835 + _Roughness * (1.3331290497744692 - _Roughness * 0.5040552688878546);	// cf. IBL.mrpr to see the link between roughness and aperture angle

	float	SinHalfAngle, CosHalfAngle;
	sincos( HalfAngle, SinHalfAngle, CosHalfAngle );
	float	TanHalfAngle = SinHalfAngle / CosHalfAngle;

// This pisses a fucking warning about uninitialized variables! What can I do else than explicitly assigning them FFS?????????? Fucking compiler!!
// 	if ( _lsView.z > 0.0 ) {
// 		_lsPositionMin = -100.0;
// 		_lsPositionMax = -100.0;
// 		return 0.0;
// 	}

	// Compute the intersection of the view with the area light's plane
	float	t = -_lsPosition.z / _lsView.z;
	float3	I = _lsPosition + t * _lsView;	// Intersection position on the plane

	// Compute the radius of the specular cone at hit distance
	float2	Radius  = TanHalfAngle * t;
//					/ sqrt( 1.0 - _lsView.xy*_lsView.xy );	// This will make the X/Y radii grow depending on view angle

	// Compute the bounding positions encompassing the cone's ellipsoid intersection with the area light plane
	_lsPositionMin = I.xy - Radius;
	_lsPositionMax = I.xy + Radius;


	/////////////////////////////////
	// Compute the specular solid angle
//	float	SolidAngle = 2.0 * PI * (1.0 - CosHalfAngle);		// Solid angle for the specular cone
//			SolidAngle = min( SolidAngle, _ClippedAreaLightSolidAngle );
//			SolidAngle = SmoothMin( SolidAngle, _ClippedAreaLightSolidAngle, 32.0 );

// 	float2	lsClipedPosMin = clamp( _lsPositionMin, _lsClippedPositions.xy, _lsClippedPositions.zw );
// 	float2	lsClipedPosMax = clamp( _lsPositionMax, _lsClippedPositions.xy, _lsClippedPositions.zw );
// 	float	CoveredArea = (lsClipedPosMax.x - lsClipedPosMin.x) * (lsClipedPosMax.y - lsClipedPosMin.y);
// 	float	TotalArea = 1e-6 + (_lsClippedPositions.z - _lsClippedPositions.x) * (_lsClippedPositions.w - _lsClippedPositions.y);
// 	float	SolidAngle = _ClippedAreaLightSolidAngle * CoveredArea / TotalArea;
//	float	SolidAngle = _ClippedAreaLightSolidAngle * smoothstep( 0.0, 1.0, CoveredArea / TotalArea );
	float	SolidAngle = _ClippedAreaLightSolidAngle;

	// Fade based on V.N to remove grazing angles specularities
	float	FresnelGrazing = saturate( 1.0 + _lsView.z );
			FresnelGrazing *= FresnelGrazing;
			FresnelGrazing *= FresnelGrazing;
	SolidAngle *= 1.0 - FresnelGrazing;

	// Finally, we can compute the projected solid angle by dotting with the normal
//	ProjectedSolidAngleSpecular *= 1.0 - pow( saturate( 1.0+_lsView.z ), 2.0 );
	return saturate( dot( _lsNormal, _lsView ) ) * SolidAngle;	// (N.Wi) * dWi
}


///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
// AREA LIGHT COMPUTE LIGHTING
///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////

struct SurfaceContext {
	float3	wsPosition;
	float3	wsNormal;
	float3	wsTangent;
	float3	wsBiTangent;
	float3	wsView;
	float3	diffuseAlbedo;
	float	roughness;
	float3	IOR;
	float	fresnelStrength;
};

struct AreaLightContext {
	float		m_attenuation;			// Combined radial and angular attenuations
	float		m_diffusion;			// Light diffusion factor in [0,1]
	float3		m_color;				// Light "color" (radiance or irradiance)
	float		m_shadow;				// Shadow factor
	float		m_roughness;			// Tweaked roughness to be used for specular term
	float3		m_wsLight;				// Pointing toward the center of the clipped area light patch

	float		m_solidAngle_area;		// The solid angle of the area light perceived by the surface (clipped by the surface's plane)
	float		m_solidAngle_diffuse;	// The solid angle to use for the diffuse part of the BRDF
	float		m_solidAngle_specular;	// The solid angle to use for the specular part of the BRDF

	float3		m_irradianceDiffuse;	// The diffuse irradiance arriving at the surface
	float3		m_irradianceSpecular;	// The specular irradiance arriving at the surface
};

struct ComputeLightingResult {
	float3	accumDiffuse;
	float3	accumSpecular;
};

///////////////////////////////////////////////////////////////////////////////////////////////
// Creates an "advanced" light context for area lights
//
AreaLightContext	CreateAreaLightContext( in SurfaceContext _Surface, uint _SliceIndex, float _Shadow, float2 _RadiusFalloffCutoff, const uint _quality ) {

	AreaLightContext	Result = (AreaLightContext) 0;

	Result.m_color = _AreaLightIntensity;//_Light.m_color;	// Already provided, just forward it...
	Result.m_shadow = _Shadow;//_Light.m_shadow;	// Already provided, just forward it...
	Result.m_roughness = max( 0.001, _Surface.roughness * _Surface.roughness );


	// 1] =========== Reconstruct area light information from fragmented data from the light context ===========
	float3	wsPosition = _Surface.wsPosition;
	float3	wsLightPos = _AreaLightT;//_Light.m_wsLightPos;
	float3	wsLightZ = _AreaLightZ;//_Light.m_SpotDir;
	float3	wsCenter2Position = wsPosition - wsLightPos;
	if ( dot( wsCenter2Position, wsLightZ ) > 0.0 ) {//&& _Shadow > 0.0 ) {	// Are we standing behind the area light?

		float3	wsLightX = _AreaLightX;//_Light.m_AreaLightX;
		float3	wsLightY = cross( wsLightZ, wsLightX );		// No need to normalize

//		float2	AreaLightScale = float2( _Light.m_Decay, _Light.m_originOffset );
		float2	AreaLightScale = float2( _AreaLightScaleX, _AreaLightScaleY );

		Result.m_diffusion = _AreaLightDiffusion;//-_Light.m_ScatterValue;		// Stored as negative on CPU side to avoid scattering computation condition (if m_ScatterValue > 0)

		float3	ProjectionDirection = _ProjectionDirectionDiff;//_Light.m_SpotParms;

		// 1] =========== Compute attenuation with distance ===========
		float	Distance2Light = length( wsCenter2Position );
		Result.m_attenuation = smoothstep( _RadiusFalloffCutoff.y, _RadiusFalloffCutoff.x, Distance2Light );	// Now with forced cutoff smoothed out like this (so we keep the physically correct 1/r² but can nonetheless artificially attenuate early)

		// 2] =========== Transform position & normal into local space ===========
		float3	wsNormal = _Surface.wsNormal;
		float3	wsView = _Surface.wsView;								// Toward camera
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

		// 3] =========== Compute potential area light clipping by the surface's plane ===========
		float4	lsClippedPositions = ComputeAreaLightClipping( lsPosition, lsNormal, AreaLightScale );
		if ( all( lsClippedPositions.zw - lsClippedPositions.xy > 1e-4 ) ) {	// Is the light fully clipped?
			Result.m_solidAngle_area = RectangleSolidAngle( lsPosition, lsClippedPositions.xy, lsClippedPositions.zw );

			// Compute light direction toward the center of the clipped area
			float2	lsCenterPos = 0.5 * (lsClippedPositions.xy + lsClippedPositions.zw);
			Result.m_wsLight = normalize( lsCenterPos.x * wsLightX + lsCenterPos.y * wsLightY - wsCenter2Position );

			// 4] =========== Compute diffuse & specular solid angles ===========
			float2	lsPosMin_diffuse, lsPosMax_diffuse;
			Result.m_solidAngle_diffuse = ComputeSolidAngleDiffuse( lsPosition, lsNormal, ProjectionDirection, lsClippedPositions, Result.m_solidAngle_area, lsPosMin_diffuse, lsPosMax_diffuse );

			float2	lsPosMin_specular = 0.0, lsPosMax_specular = 0.0;
			Result.m_solidAngle_specular = ComputeSolidAngleSpecular( lsPosition, lsNormal, lsView, Result.m_roughness, lsClippedPositions, Result.m_solidAngle_area, lsPosMin_specular, lsPosMax_specular );

			// 5] =========== Compute diffuse & specular irradiance ===========
			Result.m_irradianceDiffuse = SampleAreaLight( lsPosMin_diffuse, lsPosMax_diffuse, AreaLightScale, _SliceIndex, false );
			Result.m_irradianceSpecular = SampleAreaLight( lsPosMin_specular, lsPosMax_specular, AreaLightScale, _SliceIndex, false ).xyz;
		}
	}	// if ( dot( wsCenter2Position, wsLightZ ) > 0.0 && _Light.m_shadow > 0.0 )

	return Result;
}

///////////////////////////////////////////////////////////////////////////////////////////////
// Default lighting computation function, to be called from the BRDF
//
void	ComputeAreaLightLighting( inout ComputeLightingResult _Result, in SurfaceContext _Surface, in AreaLightContext _Light ) {

	float3	ShadowedLightColor = _Light.m_attenuation * _Light.m_shadow * _Light.m_color;
//			ShadowedLightColor *= lerp( 1.0, INVPI, _Light.m_diffusion );	// Assume radiance * 1/PI if the area light is largely diffusing

	float3	wsNormal = _Surface.wsNormal;
	float3	wsView = _Surface.wsView;				// Toward camera
	float3	wsLight = _Light.m_wsLight;				// Toward the light

	float3	H = normalize( wsView + wsLight );
	float	VdotN = saturate( dot( wsView, wsNormal ) );
	float	HdotN = saturate( dot( H, wsNormal ) );

	// 1] ----- Compute Fresnel -----
	float3	FresnelSpecular = FresnelAccurate( _Surface.IOR, HdotN, _Surface.fresnelStrength );
	float3	FresnelDiffuse = 1.0 - FresnelSpecular;	// Simplify a lot! Don't use Disney's Fresnel as it needs a light direction and we can't provide one here...

	// 2] ----- Diffuse term -----
	float3	Irradiance_diffuse = ShadowedLightColor * _Light.m_irradianceDiffuse;

	float3	IntegralBRDF_diffuse = _Surface.diffuseAlbedo * FresnelDiffuse * _Light.m_solidAngle_diffuse;	// = Fresnel * Rho/PI * (N.L) * dw

	_Result.accumDiffuse += Irradiance_diffuse * IntegralBRDF_diffuse;
//_Result.accumDiffuse = _Light.m_solidAngle_diffuse;
//_Result.accumDiffuse = _Light.m_solidAngle_area;

	// 3] ----- Specular Term -----
	float3	Irradiance_specular = ShadowedLightColor * _Light.m_irradianceSpecular;

	#if 1
		// Use the pre-integrated BRDF table to approximate specular BRDF integration over the entire area light
// 		float2	PreIntegratedBRDF = FetchPreIntegratedBRDF( VdotN, _Light.m_roughness );
		float2	PreIntegratedBRDF = _TexBRDFIntegral.SampleLevel( LinearClamp, float2( VdotN, _Light.m_roughness ), 0.0 );
 		float3	F0 = Fresnel_F0FromIOR( _Surface.IOR );
//		float3	IntegralBRDF_specular = F0 * PreIntegratedBRDF.x * _Light.m_solidAngle_specular + PreIntegratedBRDF.y;
 		float3	IntegralBRDF_specular = F0 * PreIntegratedBRDF.x * _Light.m_solidAngle_specular + (1.0-_Light.m_roughness*_Light.m_roughness)*PreIntegratedBRDF.y;	// Remove ambient term when totally rough
	#else
		// Use a basic Ward for specular
		float3	wsReflectedView = reflect( -wsView, wsNormal );			// There is a negative sign because we need the view pointing toward the surface here!
		float3	wsDir = normalize( lerp( wsLight, wsReflectedView, _Light.m_diffusion ) );
		float	WardBRDF_specular = ComputeWard( wsDir, wsView, _Surface.wsNormal, _Surface.wsTangent, _Surface.wsBiTangent, _Light.m_roughness );
		float3	IntegralBRDF_specular = FresnelSpecular * WardBRDF_specular * _Light.m_solidAngle_specular;	// = Fresnel * Ward( L, V, N ) * (N.L) * dw
	#endif

	_Result.accumSpecular += Irradiance_specular * IntegralBRDF_specular;

//_Result.accumSpecular = _Light.m_solidAngle_specular;
}
//
///////////////////////////////////////////////////////////////////////////////////////////////
