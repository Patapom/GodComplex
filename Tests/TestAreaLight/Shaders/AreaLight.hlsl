
#define USE_SAT	1

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


// Samples the SAT
float4	SampleAreaLight( float2 _UV0, float2 _UV1 ) {
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV0 );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV1.x, _UV0.y ) );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV0.x, _UV1.y ) );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV1 );
	float4	C = C11 - C10 - C01 + C00;

	// Compute normalization factor
	float2	DeltaUV = _UV1 - _UV0;

	float	PixelsCount = (DeltaUV.x * _AreaLightTexDimensions.x) * (DeltaUV.y * _AreaLightTexDimensions.y);

// 	uint2	TexSize;
// 	_TexAreaLightSAT.GetDimensions( TexSize.x, TexSize.y );
// 	float	PixelsCount = (DeltaUV.x * TexSize.x) * (DeltaUV.y * TexSize.y);

	return C * (PixelsCount > 1e-3 ? 1.0 / PixelsCount : 0.0);
}

// Another version that uses mip mapping
// float4	SampleMip( Texture2D< float4 > _Tex, float2 _UV0, float2 _UV1 ) {
// 
// 	float2	UV = 0.5 * (_UV0 + _UV1);
// 	float2	RadiusUV = 0.5 * (_UV1 - _UV0);
// 	float2	RadiusPixel = _AreaLightTexDimensions.xy * RadiusUV;
// 	float	MipLevel = log2( max( RadiusPixel.x, RadiusPixel.y ) );
// 
// 	return _Tex.SampleLevel( LinearBorder, UV, MipLevel );
// }

// Determinant of a 3x3 row-major matrix
float	Determinant( float3 a, float3 b, float3 c ) {
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
//	float	A0 = atan( -Determinant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) );
//	float	A1 = atan( -Determinant( v0, v2, v3 ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0) );
// 	return 2.0 * (A0 + A1);

	// But since atan(a)+atan(b) = atan( (a+b) / (1-ab) ) ...
 	float	Theta0 = -Determinant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0);
 	float	Theta1 = -Determinant( v0, v2, v3 ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0);
	return 2.0 * atan( (Theta0 + Theta1) / (1.0 - Theta0*Theta1) );

// 	// Try and average with a second rectangle split along (v1,v3)
// 	float	Omega0 = 2.0 * atan( (Theta0 + Theta1) / (1.0 - Theta0*Theta1) );
// 	float	dotV1V3 = dot( v1, v3 );
// 
//  			Theta0 = -Determinant( v0, v1, v3 ) / (lv0+lv1+lv3 + lv3*dotV0V1 + lv0*dotV1V3 + lv1*dotV3V0);
//  			Theta1 = -Determinant( v1, v2, v3 ) / (lv1+lv2+lv3 + lv3*dotV1V2 + lv1*dotV2V3 + lv2*dotV1V3);
// 	float	Omega1 = 2.0 * atan( (Theta0 + Theta1) / (1.0 - Theta0*Theta1) );
// 
// 	return 0.5 * (Omega0 + Omega1);
}


// Computes the potential UV clipping by the surface's normal
// We simplify *a lot* by assuming either a vertical or horizontal normal that clearly cuts the square along one of its main axes
//
// The problem with this routine is that it yields very noticeable discontinuities every 90° offset by 45° as the principal directions
//	get switched and the clipping is not continuous when the switch is occurring...
//
float4	ComputeClipping_OLD( float3 _lsPosition, float3 _lsNormal, out float4 _Debug ) {

_Debug = 0;

	float	IsVertical = step( abs(_lsNormal.x), abs(_lsNormal.y) );	// 1 if normal is vertical, 0 otherwise

	// Compute normal and position reduced to a 2D plane (either Y^Z for a vertical normal, or Z^X for an horizontal normal)
 	float2	AlignedNormal = lerp( float2( _lsNormal.z, -_lsNormal.x ), _lsNormal.zy, IsVertical );
 	float2	AlignedPosition = lerp( float2( _lsPosition.z, -_lsPosition.x ), _lsPosition.zy, IsVertical );

	// Make sure we keep the sign and always treat the positive case
	float	IsPositive = step( 0.0, AlignedNormal.y );
	AlignedNormal *= 2.0 * IsPositive - 1.0;	// Mirror normal so we always treat the positive case shown below

	// Once we're reduced to a 2D problem, the computation is easy:
	//
	//	                      * Top (0,1)
	//	                      |
	//	                      |
	//	                      |  --
	//	                    I +-
	//	                   -- |
	//	     \ N        --    |
	//	      \      --       |
	//		   \  --          |
	//			*             |<= Square area light seen from the side
	//			P             |
	//			              * Bottom (0,-1)
	//
	// We need to compute I, the intersection of segment (Top, Bottom) with the surface's plane of normal N
	// We simply pose:
	//	D = Top - P = (0,1) - P
	//	V = Bottom - Top = (0,-1) - (0,1) = (0,-2)
	//
	// Then, if we write the parametric position along the segment as I(t) = Top + V * t
	//	and (I(t) - P).N = 0 to satisfy the condition of a point belonging to the plane
	// We get:
	//
	//	((Top - P) + V * t).N = (D + V * t).N = D.N + V.N * t = 0
	//
	// Solving for t:
	//
	//	t = -D.N / V.N
	//
	// Knowing V.N = (0,-2).N = -2*Ny we simplify:
	//
	//	t = D.N / (2*Ny)
	//
	float2	D = float2( 0, 1 ) - AlignedPosition;			// (0,1) represents the top of the area light square in 2D
	float	DdotN = dot( D, AlignedNormal );
	float	t = saturate( DdotN / (2.0 * AlignedNormal.y ) );

	float	Top = lerp( t, 0.0, IsPositive );
	float	Bottom = lerp( 1.0, t, IsPositive );

	float2	UV0 = float2( 0.0, Top );
	float2	UV1 = float2( 1.0, Bottom );

	// And finally swap U and V depending on verticality
	return lerp( float4( UV0.yx, UV1.yx ), float4( UV0, UV1 ), IsVertical );
}

// Condition-less clipping routine that returns the result of the intersection of [P0,P1] with the given plane:
//	_ Return the intersection point if the segment cuts the plane
//	_ Return P0 if both P0 and P1 stand below the plane
//	_ Returns P1 if both P0 and P1 stand above the plane
//
float3	ClipSegment( float3 _P0, float3 _P1, float3 _PlanePosition, float3 _PlaneNormal, out float4 _Debug ) {

	float3	V = _P1 - _P0;
	float3	D = _PlanePosition - _P0;
	float	d = dot( D, _PlaneNormal );
	float	t = d / dot( V, _PlaneNormal );

_Debug = t;

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

// Computes the potential UV clipping by the surface's normal
// The algorithm is "quite cheap" as it doesn't use any condition and proceeds like:
//		1) Depending on the orientation of the normal, we choose P0 as the farthest point away from the plane
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
float4	ComputeClipping( float3 _lsPosition, float3 _lsNormal, out float4 _Debug ) {

_Debug = 0;

float4	Debug;

	// Reverse Y so Positions are in the same orientation as UVs
	_lsPosition.y = -_lsPosition.y;
	_lsNormal.y = -_lsNormal.y;

	float	X0 = 1.0 - 2.0 * step( _lsNormal.x, 0.0 );
	float	Y0 = 1.0 - 2.0 * step( _lsNormal.y, 0.0 );

	float3	P0 = float3( X0, Y0, 0.0 );
	float3	P1 = float3( -X0, Y0, 0.0 );
	float3	P2 = float3( -X0, -Y0, 0.0 );
	float3	P3 = float3( X0, -Y0, 0.0 );

// _Debug = float4( 0.5*(1.0+P3.xy), 0, 0 );
// return 0.0;

	// Compute clipping of the square by browsing the contour in both CCW and CW
	// We keep the min/max of UVs each time
	float4	MinMax = float4( 1.0, 1.0, -1.0, -1.0 );

	// CCW
	float3	P = ClipSegment( P0, P1, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

//_Debug = float4( 0.5 * (1.0 + float2( P.x, -P.y )), 0, 0 );
_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );
// _Debug = Debug;

	P = ClipSegment( P, P2, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );
//_Debug = float4( _lsNormal.xy, 0, 0 );
//_Debug = Debug;

	P = ClipSegment( P, P3, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegment( P, P0, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	// CW
	P = ClipSegment( P0, P3, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegment( P, P2, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegment( P, P1, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	P = ClipSegment( P, P0, _lsPosition, _lsNormal, Debug );
	MinMax = float4( min( MinMax.xy, P.xy ), max( MinMax.zw, P.xy ) );

_Debug = float4( 0.5 * (1.0 + P.xy), 0, 0 );

	// Finalize UVs
//	MinMax.yw = -MinMax.yw;
	return 0.5 * (1.0 + MinMax);
}

// Computes the 2 UVs and the solid angle perceived from a single point in world space (used for diffuse reflection)
// The area light's unit square is first clipped agains the surface's plane and the remaining bounding rectangle is used as the area to sample for irradiance.
// 
bool	ComputeSolidAngleDiffuse( float3 _wsPosition, float3 _wsNormal, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle, out float4 _Debug ) {
	_UV0 = _UV1 = 0.0;
	_ProjectedSolidAngle = 0.0;
	_Debug = 0.0;

	float3	wsCenter2Position = _wsPosition - _AreaLightT;
	float3	lsPosition = float3(dot( wsCenter2Position, _AreaLightX ),	// Transform world position into local area light space
								dot( wsCenter2Position, _AreaLightY ),
								dot( wsCenter2Position, _AreaLightZ ) );
	if ( lsPosition.z <= 0.0 ) {
		// Position is behind area light...
		return false;
	}

	lsPosition.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );		// Account for scale

	float3	lsNormal = float3(	dot( _wsNormal, _AreaLightX ),			// Transform world normal into local area light space
								dot( _wsNormal, _AreaLightY ),
								dot( _wsNormal, _AreaLightZ ) );

	// In local area light space, the position is in front of a canonical square:
	//
	//	(-1,+1)					(+1,+1)
	//			o-------------o
	//			|             |
	//			|             |
	//			|             |
	//			|      o      |
	//			|             |
	//			|             |
	//			|             |
	//			o-------------o
	//	(-1,-1)					(+1,-1)
	//
	//
	float3	lsPortal[2] = {
		float3( -1, +1, 0 ),		// Top left
		float3( +1, -1, 0 ),		// Bottom right
	};

	// Compute the UV coordinates of the intersection of the virtual light source frustum with the portal's plane
	// The virtual light source is the portal offset by a given vector so it gets away from the portal along the -Z direction
	// This gives us:
	//	_ An almost directional thin brush spanning a few pixels when the virtual light source is far away (almost infinity) from the portal (diffusion = 0)
	//	_ An area that covers the entire portal when the virtual light source is right on the portal (diffusion = 1)
	//
	float2	lsIntersection[2] = { 0.0.xx, 0.0.xx };
	[unroll]
	for ( uint Corner=0; Corner < 2; Corner++ ) {
		float3	lsVirtualPos = lsPortal[Corner] - _ProjectionDirectionDiff;	// This is the position of the corner of the virtual source
		float3	Dir = lsVirtualPos - lsPosition;							// This is the pointing direction, originating from the source _wsPosition
		float	t = -lsPosition.z / Dir.z;									// This is the distance at which we hit the physical portal's plane
		lsIntersection[Corner] = (lsPosition + t * Dir).xy;					// This is the position on the portal's plane
	}

	// Retrieve the UVs
	_UV0 = 0.5 * (1.0 + float2( lsIntersection[0].x, -lsIntersection[0].y));
	_UV1 = 0.5 * (1.0 + float2( lsIntersection[1].x, -lsIntersection[1].y));
	_UV1 = max( _UV1, _UV0 + _AreaLightTexDimensions.zw );	// Make sure the UVs are at least separated by a single texel before clamping

	// Compute potential clipping by the surface's plane
	float4	ClippedUVs = ComputeClipping( lsPosition, lsNormal, _Debug );
	_UV0 = clamp( _UV0, ClippedUVs.xy, ClippedUVs.zw );
	_UV1 = clamp( _UV1, ClippedUVs.xy, ClippedUVs.zw );

	// Compute the solid angle
//	float	SolidAngle = RectangleSolidAngle( lsPosition, _UV0, _UV1 );
	float	SolidAngle = RectangleSolidAngle( lsPosition, ClippedUVs.xy, ClippedUVs.zw );

	// Now, we can compute the projected solid angle by dotting with the normal
	float3	lsCenter = float3( _UV1.x + _UV0.x - 1.0, 1.0 - _UV1.y - _UV0.y, 0.0 );
	float3	lsPosition2Center = normalize( lsCenter - lsPosition );						// Wi, the average incoming light direction
	_ProjectedSolidAngle = saturate( dot( lsNormal, lsPosition2Center ) ) * SolidAngle;	// (N.Wi) * dWi

_Debug = _ProjectedSolidAngle;
_Debug = SolidAngle;

	return true;
}

// Computes the 2 UVs and the solid angle perceived from a single point in world space watching the area light through a cone (used for specular reflection)
//	_wsPosition, the world space position of the surface watching the area light
//	_wsNormal, the world space normal of the surface
//	_wsView, the view direction (usually, the main reflection direction)
//	_TanHalfAngle, the tangent of the half-angle of the cone watching
//
// Returns:
//	_UV0, _UV1, the 2 UVs coordinates where to sample the SAT
//	_ProjectedSolidAngle, an estimate of the perceived projected solid angle (i.e. cos(IncidentAngle) * dOmega)
//
bool	ComputeSolidAngleSpecular( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _Gloss, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle, out float4 _Debug ) {
	_UV0 = _UV1 = 0.0;
	_ProjectedSolidAngle = 0.0;
	_Debug = 0.0;

_Gloss = pow( _Gloss, 0.125 );

	float3	wsCenter2Position = _wsPosition - _AreaLightT;
	float3	lsPosition = float3(dot( wsCenter2Position, _AreaLightX ),	// Transform world position into local area light space
								dot( wsCenter2Position, _AreaLightY ),
								dot( wsCenter2Position, _AreaLightZ ) );

	float3	lsView = float3(	dot( _wsView, _AreaLightX ),			// Transform world direction into local area light space
								dot( _wsView, _AreaLightY ),
								dot( _wsView, _AreaLightZ ) );
	if ( lsPosition.z <= 0.0 ) {
		// Position is behind area light...
		return false;
	}

	lsView.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );			// Account for scale
	lsPosition.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );		// Account for scale

	float3	lsNormal = float3(	dot( _wsNormal, _AreaLightX ),			// Transform world normal into local area light space
								dot( _wsNormal, _AreaLightY ),
								dot( _wsNormal, _AreaLightZ ) );

	// Tweak the view to point toward the center of the area light depending on glossiness
//	lsView = normalize( lerp( -lsPosition, lsView, _Gloss ) );

	// In local area light space, the position is in front of a canonical square:
	//
	//	(-1,+1)					(+1,+1)
	//			o-------------o
	//			|             |
	//			|             |
	//			|             |
	//			|      o      |
	//			|             |
	//			|             |
	//			|             |
	//			o-------------o
	//	(-1,-1)					(+1,-1)
	//
	//
	const float	DIFFUSION_FACTOR = 1;//0.25;

	float	Roughness = 1.0 * (1.0 - _Gloss);
	float	HalfAngle = 0.0003474660443456835 + Roughness * (1.3331290497744692 - Roughness * 0.5040552688878546);	// cf. HDRCubeMapConvolver to see the link between roughness and aperture angle
	float	SinHalfAngle, CosHalfAngle;
	sincos( HalfAngle, SinHalfAngle, CosHalfAngle );
	float	TanHalfAngle = SinHalfAngle / CosHalfAngle;

#if 1
	// Compute the intersection of the view with the light plane
	float	t = lsPosition.z / max(1e-3, -lsView.z );
	float3	I = lsPosition + t * lsView;	// Intersection position on the plane

	// Compute the radius of the specular cone at hit distance
	float	Radius = TanHalfAngle * t;		// Correct radius at hit distance
//			Radius /= -lsView.z;			// Account for radius increase due to grazing angle
			Radius *= DIFFUSION_FACTOR;		// Arbitrary radius attenuation to avoid too much diffusion (an esthetic factor really)

	float	RadiusUV = 0.5 * Radius;
//			RadiusUV = saturate( Radius );	// Useless to have a larger radius than the entire area
			RadiusUV = lerp( RadiusUV, 1.0, smoothstep( 0.0, 1.0, RadiusUV ) );

	float2	UVcenter = float2( 0.5 * (1.0 + I.x), 0.5 * (1.0 - I.y) );

//UVcenter = 0.5;

	_UV0 = UVcenter - RadiusUV;
	_UV1 = UVcenter + RadiusUV;
	_UV1 = max( _UV1, _UV0 + _AreaLightTexDimensions.zw );	// Make sure the UVs are at least separated by a single texel before clamping

#else

	float3	Y = normalize( cross( float3( 1, 0, 0 ), lsView ) );
	float3	X = cross( lsView, Y );
	float3	lsTopLeft = TanHalfAngle * (-X + Y) + lsView;
	float3	lsBottomRight = TanHalfAngle * (X - Y) + lsView;

	float3	I0 = lsPosition + (lsPosition.z / max( 1e-3, -lsTopLeft.z)) * lsTopLeft;
	float3	I1 = lsPosition + (lsPosition.z / max( 1e-3, -lsBottomRight.z)) * lsBottomRight;
	_UV0 = float2( 0.5 * (1.0 + I0.x), 0.5 * (1.0 - I0.y) );
	_UV1 = float2( 0.5 * (1.0 + I1.x), 0.5 * (1.0 - I1.y) );
	_UV1 = max( _UV1, _UV0 + _AreaLightTexDimensions.zw );	// Make sure the UVs are at least separated by a single texel before clamping

	float2	DeltaUV = _UV1 - _UV0;
//	float	RadiusUV = 0.5 * sqrt( DeltaUV.x*DeltaUV.x + DeltaUV.y*DeltaUV.y );
	float	RadiusUV = 0.5 * min( DeltaUV.x, DeltaUV.y );
	float2	UVcenter = 0.5 * (_UV0 + _UV1);

#endif

	// Compute potential clipping by the surface's plane
	float4	ClippedUVs = ComputeClipping( lsPosition, lsNormal, _Debug );

_Debug = float4( _UV0, 0, 0 );

// 	_UV0 = clamp( _UV0, ClippedUVs.xy, ClippedUVs.zw );
// 	_UV1 = clamp( _UV1, ClippedUVs.xy, ClippedUVs.zw );

	float2	SatUVcenter = clamp( UVcenter, ClippedUVs.xy, ClippedUVs.zw );


	/////////////////////////////////
	// Compute the specular solid angle
	float	SolidAngleSpecular = 2.0 * PI * (1.0 - CosHalfAngle);				// Specular solid angle for a cone
//			SolidAngleSpecular *= 0.0;

	// Compute an approximation of the clipping of the projected disc and the area light:
	//
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
	SolidAngleSpecular *= 1.0 - saturate( length( UVcenter - SatUVcenter ) / (1e-3 + RadiusUV) );
//	SolidAngleSpecular = max( 1e-1, SolidAngleSpecular );

	// Now, multiply by PI/4 to account for the fact we traced a square pyramid instead of a cone
	// (area of the base of the pyramid is 2x2 while area of the circle is PI)
	SolidAngleSpecular *= 0.25 * PI;

	// Finally, we can compute the projected solid angle by dotting with the normal
	float	ProjectedSolidAngleSpecular = saturate( dot( lsNormal, lsView ) ) * saturate( -lsView.z ) * SolidAngleSpecular;		// (N.Wi) * dWi


//ProjectedSolidAngleSpecular *= 0.001/31.831;


	/////////////////////////////////
	// Compute the diffuse solid angle
//	float	SolidAngleDiffuse = RectangleSolidAngle( lsPosition, _UV0, _UV1 );	// Diffuse solid angle always intersects the clipped square
	float	SolidAngleDiffuse = RectangleSolidAngle( lsPosition, ClippedUVs.xy, ClippedUVs.zw );
	
//	float3	lsCenter = float3( _UV1.x + _UV0.x - 1.0, 1.0 - _UV1.y - _UV0.y, 0.0 );
	float3	lsCenter = float3( ClippedUVs.z + ClippedUVs.x - 1.0, 1.0 - ClippedUVs.y - ClippedUVs.w, 0.0 );
	float3	lsPosition2Center = normalize( lsCenter - lsPosition );						// Wi, the average incoming light direction
	float	ProjectedSolidAngleDiffuse = saturate( dot( lsNormal, lsPosition2Center ) ) * SolidAngleDiffuse;	// (N.Wi) * dWi
//ProjectedSolidAngleDiffuse = SolidAngleDiffuse;


//ProjectedSolidAngleDiffuse *= 0.5;
ProjectedSolidAngleDiffuse *= 1.0;


	/////////////////////////////////
	// Build the result solid angle
//	_ProjectedSolidAngle = min( ProjectedSolidAngleCone, ProjectedSolidAngleDiffuse );
//	_ProjectedSolidAngle = ProjectedSolidAngleSpecular;
	_ProjectedSolidAngle = ProjectedSolidAngleDiffuse;
//	_ProjectedSolidAngle = lerp( ProjectedSolidAngleDiffuse, ProjectedSolidAngleSpecular, _Gloss );



	_UV0 = lerp( ClippedUVs.xy, _UV0, _Gloss );
	_UV1 = lerp( ClippedUVs.zw, _UV1, _Gloss );
	_UV1 = max( _UV0, _UV1 );



//_Debug = float4( I0, 0 );
_Debug = float4( UVcenter, 0, 0 );
_Debug = RadiusUV;
_Debug = SolidAngleSpecular;
_Debug = 1-length( UVcenter - SatUVcenter ) / (1e-3 + RadiusUV);
_Debug = _ProjectedSolidAngle;

	return true;
}
