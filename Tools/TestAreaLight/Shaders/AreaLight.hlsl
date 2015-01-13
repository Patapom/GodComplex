
cbuffer CB_Light : register(b2) {
	float3		_AreaLightX;
	float		_AreaLightScaleX;
	float3		_AreaLightY;
	float		_AreaLightScaleY;
	float3		_AreaLightZ;
	float		_AreaLightDiffusion;
	float3		_AreaLightT;
	float		_AreaLightIntensity;
	float3		_ProjectionDirectionDiff;	// Closer to portal when diffusion increases
	float3		_ProjectionDirectionSpec;	// Closer to portal when diffusion decreases
};

Texture2D< float4 >	_TexAreaLightSAT : register(t0);


static const uint2	TEX_SIZE = uint2( 465, 626 );
static const float3	dUV = float3( 1.0 / TEX_SIZE, 0.0 );

// Samples the SAT
float4	SampleSAT( float2 _UV0, float2 _UV1 ) {
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV0 );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV1.x, _UV0.y ) );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV0.x, _UV1.y ) );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV1 );
	float4	C = C11 - C10 - C01 + C00;

	// Compute normalization factor
	float2	DeltaUV = _UV1 - _UV0;

//	float	PixelsCount = (DeltaUV.x * TEX_SIZE.x) * (DeltaUV.y * TEX_SIZE.y);

	uint2	TexSize;
	_TexAreaLightSAT.GetDimensions( TexSize.x, TexSize.y );
	float	PixelsCount = (DeltaUV.x * TexSize.x) * (DeltaUV.y * TexSize.y);

	return C * (PixelsCount > 1e-3 ? 1.0 / PixelsCount : 0.0);
}

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

// World space version
// float	RectangleSolidAngleWS( float3 _wsPosition, float2 _UV0, float2 _UV1 ) {
// 
// 	float3	D = _AreaLightT - _wsPosition;
// 	float3	v0 = D + _AreaLightX * (2.0 * _UV0.x - 1.0) + _AreaLightY * (1.0 - 2.0 * _UV0.y);
// 	float3	v1 = D + _AreaLightX * (2.0 * _UV0.x - 1.0) + _AreaLightY * (1.0 - 2.0 * _UV1.y);
// 	float3	v2 = D + _AreaLightX * (2.0 * _UV1.x - 1.0) + _AreaLightY * (1.0 - 2.0 * _UV1.y);
// 	float3	v3 = D + _AreaLightX * (2.0 * _UV1.x - 1.0) + _AreaLightY * (1.0 - 2.0 * _UV0.y);
// 
// 	float	lv0 = length( v0 );
// 	float	lv1 = length( v1 );
// 	float	lv2 = length( v2 );
// 	float	lv3 = length( v3 );
// 
// 	float	dotV0V1 = dot( v0, v1 );
// 	float	dotV1V2 = dot( v1, v2 );
// 	float	dotV2V3 = dot( v2, v3 );
// 	float	dotV3V0 = dot( v3, v0 );
// 	float	dotV2V0 = dot( v2, v0 );
// 
//  	float	A0 = atan( -Determinant( v0, v1, v2 ) / (lv0+lv1+lv2 + lv2*dotV0V1 + lv0*dotV1V2 + lv1*dotV2V0) );
//  	float	A1 = atan( -Determinant( v0, v2, v3 ) / (lv0+lv2+lv3 + lv3*dotV2V0 + lv0*dotV2V3 + lv2*dotV3V0) );
// 	return 2.0 * (A0 + A1);
// }


// Computes the potential UV clipping by the surface's normal
// We simplify *a lot* by assuming either a vertical or horizontal normal that clearly cuts the square along one of its main axes
//
float4	ComputeClipping( float3 _lsPosition, float3 _lsNormal, out float4 _Debug ) {

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

/*// Computes the 2 UVs and the solid angle perceived from a single point in world space
bool	ComputeSolidAngleFromPoint____OLD( float3 _wsPosition, float3 _wsNormal, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle ) {

	float3	lsPosition = mul( float4( _wsPosition, 1.0 ), _World2AreaLight ).xyz;		// Transform world position in local area light space
	if ( lsPosition.z <= 0.0 ) {
		// Position is behind area light...
		_UV0 = _UV1 = 0.0;
		_ProjectedSolidAngle = 0.0;
		return false;
	}

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

	// Compute the UV coordinates of the intersection of the frustum with the portal's plane
	float2	lsIntersection[2] = { 0.0.xx, 0.0.xx };
	for ( uint Corner=0; Corner < 2; Corner++ ) {
		float3	lsVirtualPos = lsPortal[Corner] - _ProjectionDirectionDiff;	// This is the position of the corner of the virtual source
		float3	Dir = lsVirtualPos - lsPosition;							// This is the pointing direction, originating from the source _wsPosition
		float	t = -lsPosition.z / Dir.z;									// This is the distance at which we hit the physical portal's plane
		lsIntersection[Corner] = (lsPosition + t * Dir).xy;					// This is the position on the portal's plane
	}

	// Retrieve the UVs
	_UV0 = 0.5 * (1.0 + float2( lsIntersection[0].x, -lsIntersection[0].y));
	_UV1 = max( _UV0 + dUV.xy, 0.5 * (1.0 + float2( lsIntersection[1].x, -lsIntersection[1].y)) );	// Make sure the UVs are at least separated by a single texel before clamping

	// Clamp to [0,1]
	_UV0 = saturate( _UV0 );
	_UV1 = saturate( _UV1 );

	// Compute the solid angle
	float2	DeltaUV = _UV1 - _UV0;
	float	UVArea = DeltaUV.x * DeltaUV.y;	// This is the perceived area in UV space
	float	wsArea = UVArea * _Area;		// This is the perceived area in world space

	float2	lsCenter = 0.5 * (lsIntersection[0] + lsIntersection[1]);
	float3	wsCenter = _AreaLight2World[3].xyz + lsCenter.x * _AreaLight2World[0].xyz + lsCenter.y * _AreaLight2World[1].xyz;	// World space center
	float3	wsPos2Center = normalize( wsCenter - _wsPosition );

	float	SolidAngle = wsArea * -dot( wsPos2Center, _AreaLight2World[2].xyz );	// dWi = Area * cos( theta )

	// Now, we can compute the projected solid angle by dotting with the normal
	_ProjectedSolidAngle = saturate( dot( _wsNormal, wsPos2Center ) ) * SolidAngle;	// (N.Wi) * dWi


_ProjectedSolidAngle = 1;//UVArea;


// _UV0 = lsPosition.xy;
// _UV1 = lsPosition.z;
// _UV0 = _UV1;
// _UV1 = 0;

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
bool	ComputeSolidAngleSpecular___OLD( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _TanHalfAngle, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle, out float4 _Debug ) {
	_UV0 = _UV1 = 0.0;
	_ProjectedSolidAngle = 0.0;
	_Debug = 0.0;

	float3	wsCenter2Position = _wsPosition - _AreaLightT;
	float3	lsPosition = float3(dot( wsCenter2Position, _AreaLightX ),	// Transform world position into local area light space
								dot( wsCenter2Position, _AreaLightY ),
								dot( wsCenter2Position, _AreaLightZ ) );

	float3	lsView = float3(	dot( _wsView, _AreaLightX ),				// Transform world direction into local area light space
								dot( _wsView, _AreaLightY ),
								dot( _wsView, _AreaLightZ ) );
	if ( lsPosition.z <= 0.0 || lsView.z >= 0.0 ) {
		// Position is behind area light or watching away from it...
		return false;
	}

	lsView.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );			// Account for scale
	lsPosition.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );			// Account for scale

	float3	lsNormal = float3(	dot( _wsNormal, _AreaLightX ),				// Transform world normal in local area light space
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

	// Build a reference frame for the view direction
	float3	Y = normalize( float3( 0.0, -lsView.z, lsView.y ) );	// = normalize( cross( PlaneTangent, lsView );  where PlaneTangent = (1,0,0)
	float3	X = cross( lsView, Y );

	// Generate the 4 rays encompassing the cone aperture
	float2	Dirs[4] = {
		float2( -1, +1 ),
		float2( +1, +1 ),
		float2( -1, -1 ),
		float2( +1, -1 ),
	};

	// Compute the intersection of the frustum with the virtual source's plane
	float	Distance2VirtualSource = lsPosition.z;// + _ProjectionDirectionSpec.z;
	float3	lsVirtualSourceCenter = 0.0;//-_ProjectionDirectionSpec;					// Center of the virtual source

	float2	HitMin = 1e6;
	float2	HitMax = -1e6;
	for ( uint Corner=0; Corner < 4; Corner++ ) {
		float3	vsDirection = float3( _TanHalfAngle * Dirs[Corner], 1.0 );						// Ray direction in view space
		float3	lsDirection = vsDirection.x * X + vsDirection.y * Y + vsDirection.z * lsView;	// Ray direction in local space

		float	t = -Distance2VirtualSource / lsDirection.z;									// Distance at which the ray hits the virtual source's plane
		float3	lsIntersection = lsPosition + t * lsDirection - lsVirtualSourceCenter;			// Hit position on the virtual source's plane, relative to its center

// _Debug = float4( lsIntersection, 0 );
// _Debug = float4( lsDirection, 0 );
// return true;

		// Keep min and max hit positions
		HitMin = min( HitMin, lsIntersection.xy );
		HitMax = max( HitMax, lsIntersection.xy );
	}

	// Compute the UV's from the hit positions
	_UV0 = 0.5 * (1.0 + float2( HitMin.x, -HitMin.y));
	_UV1 = 0.5 * (1.0 + float2( HitMax.x, -HitMax.y));
	_UV1 = max( _UV0 + dUV.xy, _UV1 );	// Make sure the UVs are at least separated by a single texel before clamping

//_Debug = float4( _UV1, 0, 0 );

	// Compute potential clipping by the surface's plane
	float4	ClippedUVs = ComputeClipping( lsPosition, lsNormal, _Debug );
	_UV0 = clamp( _UV0, ClippedUVs.xy, ClippedUVs.zw );
	_UV1 = clamp( _UV1, ClippedUVs.xy, ClippedUVs.zw );

	// Compute the solid angle
	float	SolidAngle = RectangleSolidAngle( lsPosition, _UV0, _UV1 );

	// Now, we can compute the projected solid angle by dotting with the normal
	_ProjectedSolidAngle = saturate( dot( _wsNormal, _wsView ) ) * SolidAngle;	// (N.Wi) * dWi

	// Finally, multiply by PI/4 to account for the fact we traced a square pyramid instead of a cone
	// (area of the base of the pyramid is 2x2 while area of the circle is PI)
	_ProjectedSolidAngle *= 0.25 * PI;

	return true;
}

*/

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
	_UV1 = max( _UV1, _UV0 + dUV.xy );	// Make sure the UVs are at least separated by a single texel before clamping

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

// _Debug = _ProjectedSolidAngle;

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
bool	ComputeSolidAngleSpecular( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _CosHalfAngle, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle, out float4 _Debug ) {
	_UV0 = _UV1 = 0.0;
	_ProjectedSolidAngle = 0.0;
	_Debug = 0.0;

	float3	wsCenter2Position = _wsPosition - _AreaLightT;
	float3	lsPosition = float3(dot( wsCenter2Position, _AreaLightX ),	// Transform world position into local area light space
								dot( wsCenter2Position, _AreaLightY ),
								dot( wsCenter2Position, _AreaLightZ ) );

	float3	lsView = float3(	dot( _wsView, _AreaLightX ),			// Transform world direction into local area light space
								dot( _wsView, _AreaLightY ),
								dot( _wsView, _AreaLightZ ) );
	if ( lsPosition.z <= 0.0 || lsView.z >= 0.0 ) {
		// Position is behind area light or watching away from it...
		return false;
	}

	lsView.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );			// Account for scale
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
	float	t = -lsPosition.z / lsView.z;
	float3	I = lsPosition + t * lsView;

// I.xy = clamp( I.xy, -2.0, 2.0 );
// 
// float3	Delta = I - lsPosition;
// 		Delta.xy /= float2( _AreaLightScaleX, _AreaLightScaleY );
// 		t = length( Delta );

	const float	DIFFUSION_FACTOR = 1;//0.25;

	float	TanHalfAngle = sqrt( 1.0 - _CosHalfAngle*_CosHalfAngle ) / _CosHalfAngle;
	float	Radius = TanHalfAngle * t;		// Correct radius at hit distance
			Radius /= -lsView.z;			// Account for radius increase due to grazing angle
			Radius *= DIFFUSION_FACTOR;		// Arbitrary radius attenuation to avoid too much diffusion (an esthetic factor really)

	float	RadiusUV = 0.5 * Radius;
//			RadiusUV = saturate( Radius );	// Useless to have a larger radius than the entire area
//			RadiusUV = lerp( RadiusUV, 1.0, smoothstep( 0.0, 1.0, RadiusUV ) );

	float2	UVcenter = float2( 0.5 * (1.0 + I.x), 0.5 * (1.0 - I.y) );
	_UV0 = UVcenter - RadiusUV;
	_UV1 = UVcenter + RadiusUV + dUV.xy;

	float2	SatUVcenter = saturate( UVcenter );
// 	float2	UVMin = 0.5 * RadiusUV;
// 	float2	UVMax = 1.0 - 0.5 * RadiusUV;
// 	float2	SatUVcenter = clamp( UVcenter, UVMin, UVMax );

//	float2	SatUV0 = saturate( _UV0 );
//	float2	SatUV1 = saturate( _UV1 );
//	float2	DeltaUV = _UV1 - _UV0;

	
/*	// Build a reference frame for the view direction
	float3	Y = normalize( float3( 0.0, -lsView.z, lsView.y ) );	// = normalize( cross( PlaneTangent, lsView );  where PlaneTangent = (1,0,0)
	float3	X = cross( lsView, Y );

	// Generate the 4 rays encompassing the cone aperture
	float2	Dirs[4] = {
		float2( -1, +1 ),
		float2( +1, +1 ),
		float2( -1, -1 ),
		float2( +1, -1 ),
	};

	// Compute the intersection of the frustum with the virtual source's plane
	float	Distance2VirtualSource = lsPosition.z;// + _ProjectionDirectionSpec.z;
	float3	lsVirtualSourceCenter = 0.0;//-_ProjectionDirectionSpec;					// Center of the virtual source

	float2	HitMin = 1e6;
	float2	HitMax = -1e6;
	for ( uint Corner=0; Corner < 4; Corner++ ) {
		float3	vsDirection = float3( _TanHalfAngle * Dirs[Corner], 1.0 );						// Ray direction in view space
		float3	lsDirection = vsDirection.x * X + vsDirection.y * Y + vsDirection.z * lsView;	// Ray direction in local space

		float	t = -Distance2VirtualSource / lsDirection.z;									// Distance at which the ray hits the virtual source's plane
		float3	lsIntersection = lsPosition + t * lsDirection - lsVirtualSourceCenter;			// Hit position on the virtual source's plane, relative to its center

// _Debug = float4( lsIntersection, 0 );
// _Debug = float4( lsDirection, 0 );
// return true;

		// Keep min and max hit positions
		HitMin = min( HitMin, lsIntersection.xy );
		HitMax = max( HitMax, lsIntersection.xy );
	}

	// Compute the UV's from the hit positions
	_UV0 = 0.5 * (1.0 + float2( HitMin.x, -HitMin.y));
	_UV1 = 0.5 * (1.0 + float2( HitMax.x, -HitMax.y));
	_UV1 = max( _UV0 + dUV.xy, _UV1 );	// Make sure the UVs are at least separated by a single texel before clamping

//_Debug = float4( _UV1, 0, 0 );
*/

	// Compute potential clipping by the surface's plane
	float4	ClippedUVs = ComputeClipping( lsPosition, lsNormal, _Debug );
	_UV0 = clamp( _UV0, ClippedUVs.xy, ClippedUVs.zw );
	_UV1 = clamp( _UV1, ClippedUVs.xy, ClippedUVs.zw );

	// Compute the solid angle
//	float	SolidAngleRect = RectangleSolidAngle( lsPosition, _UV0, _UV1 );
	float	SolidAngleRect = RectangleSolidAngle( lsPosition, ClippedUVs.xy, ClippedUVs.zw );
	float	SolidAngleCone = 2.0 * PI * (1.0 - _CosHalfAngle);	// Solid angle of a cone

//	float	SolidAngle = min( SolidAngleCone, SolidAngleRect );
//	float	SolidAngle = lerp( 0.25 * SolidAngleCone, SolidAngleRect, saturate( RadiusUV ) );
	float	SolidAngle = 0.25 * SolidAngleCone;


// float4	UVs = float4( _UV0, _UV1 );
// 		UVs = clamp( UVs, ClippedUVs.xyxy, ClippedUVs.zwzw );
// float	SolidAngle = RectangleSolidAngle( lsPosition, UVs.xy, UVs.zw );

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
	SolidAngle *= 1.0 - saturate( length( UVcenter - SatUVcenter ) / RadiusUV );

// _Debug = (DeltaSatUV.x * DeltaSatUV.y) / (DeltaUV.x * DeltaUV.y);
_Debug = 1.0 - saturate( length( UVcenter - SatUVcenter ) / RadiusUV );
// //_Debug = float4( 0.25 * SatUV1, 0, 0 );
//_Debug = SolidAngle;
//_Debug = float4( I, 0 );
// _Debug = float4( _UV1, 0, 0 );
// _Debug = RadiusUV;

	// Now, multiply by PI/4 to account for the fact we traced a square pyramid instead of a cone
	// (area of the base of the pyramid is 2x2 while area of the circle is PI)
//	SolidAngle *= 0.25 * PI;

	// Finally, we can compute the projected solid angle by dotting with the normal
	_ProjectedSolidAngle = saturate( dot( _wsNormal, _wsView ) ) * SolidAngle;	// (N.Wi) * dWi
//_ProjectedSolidAngle = SolidAngle;

	// Arbitrary factor
//	_ProjectedSolidAngle *= 0.25;

	return true;
}
