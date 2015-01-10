

// Check cette ligne dans ward.mrpr, changer ce facteur hardcodé pour voir si ça limite pas un peu nos pics de spec
// anisotropicRoughness = max( 0.01, anisotropicRoughness );	// Make sure we don't go below 0.01 otherwise specularity is unnatural for our poor lights (only IBL with many samples would solve that!)


#include "Global.hlsl"

cbuffer CB_Object : register(b2) {
	float4x4	_Local2World;
	float4x4	_World2Local;
};

cbuffer CB_Material : register(b3) {
	float4x4	_AreaLight2World;
	float4x4	_World2AreaLight;
	float3		_ProjectionDirectionDiff;	// Closer to portal when diffusion increases
	float		_Area;
	float3		_ProjectionDirectionSpec;	// Closer to portal when diffusion decreases
	float		_LightIntensity;
	float		_Gloss;
	float		_Metal;
};

Texture2D< float4 >	_TexAreaLightSAT : register(t0);

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;

	return Out;
}

static const uint2	TEX_SIZE = uint2( 465, 626 );
static const float3	dUV = float3( 1.0 / TEX_SIZE, 0.0 );

float4	SampleSATSinglePixel( float2 _UV ) {

	float2	PixelIndex = _UV * TEX_SIZE;
	float2	NextPixelIndex = PixelIndex + 1;
	float2	UV2 = NextPixelIndex / TEX_SIZE;

	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xz );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.zy );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xy );

	return C11 - C10 - C01 + C00;
}

float4	SampleSAT( float2 _UV0, float2 _UV1 ) {
	// Sample SAT
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV0 );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV1.x, _UV0.y ) );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV0.x, _UV1.y ) );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV1 );
	float4	C = C11 - C10 - C01 + C00;

	// Compute normalization factor
	float2	DeltaUV = _UV1 - _UV0;
	float	PixelsCount = (DeltaUV.x * TEX_SIZE.x) * (DeltaUV.y * TEX_SIZE.y);

// PixelsCount = 1.0;

	return C / (1e-6 + PixelsCount);
}

// Determinant of the 3x3 row-major matrix
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

	float3	v0 = normalize( float3( 2.0 * _UV0.x - 1.0, 1.0 - 2.0 * _UV0.y, 0.0 ) - _lsPosition );
	float3	v1 = normalize( float3( 2.0 * _UV0.x - 1.0, 1.0 - 2.0 * _UV1.y, 0.0 ) - _lsPosition );
	float3	v2 = normalize( float3( 2.0 * _UV1.x - 1.0, 1.0 - 2.0 * _UV1.y, 0.0 ) - _lsPosition );
	float3	v3 = normalize( float3( 2.0 * _UV1.x - 1.0, 1.0 - 2.0 * _UV0.y, 0.0 ) - _lsPosition );

	float	dotV0V1 = dot( v0, v1 );
	float	dotV1V2 = dot( v1, v2 );
	float	dotV2V3 = dot( v2, v3 );
	float	dotV3V0 = dot( v3, v0 );
	float	dotV2V0 = dot( v2, v0 );

	float	A0 = atan( -Determinant( v0, v1, v2 ) / (1.0 + dotV0V1 + dotV1V2 + dotV2V0) );
	float	A1 = atan( -Determinant( v0, v2, v3 ) / (1.0 + dotV2V0 + dotV2V3 + dotV3V0) );
	return 2.0 * (A0 + A1);
}

// Computes the 2 UVs and the solid angle perceived from a single point in world space
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

// Computes the 2 UVss and the solid angle perceived from a single point in world space (used for diffuse reflection)
// The area light's unit square is first clipped agains the surface's plane and the remaining bounding rectangle is used as the area to sample for irradiance.
// 
bool	ComputeSolidAngleDiffuse( float3 _wsPosition, float3 _wsNormal, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle, out float4 _Debug ) {
	_UV0 = _UV1 = 0.0;
	_ProjectedSolidAngle = 0.0;
	_Debug = 0.0;

	float3	lsPosition = mul( float4( _wsPosition, 1.0 ), _World2AreaLight ).xyz;		// Transform world position in local area light space
	float3	lsNormal = mul( float4( _wsNormal, 0.0 ), _World2AreaLight ).xyz;			// Transform world normal in local area light space
	if ( lsPosition.z <= 0.0 ) {
		// Position is behind area light...
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

	// Compute potential clipping by the surface's plane
	// We simplify *a lot* by assuming either a vertical or horizontal normal that cuts the square along one of its main axes
	if ( abs(lsNormal.y) > abs(lsNormal.x) ) {
		// Check for a vertical cut
		float2	AlignedNormal = lsNormal.zy;
		float2	Delta = float2( 0, 1 ) - lsPosition.zy;
		float	D = dot( Delta, AlignedNormal );
		float	t = saturate( D / (2.0 * AlignedNormal.y ) );

// _Debug = float4( Delta, 0, 0 );
// //_Debug = float4( lsPosition.zy, 0, 0 );
// _Debug = D;	// = 2
// _Debug = 0.5 * t;

		if ( AlignedNormal.y >= 0.0 ) {
			_UV0 = 0.0;
			_UV1 = float2( 1, t );
		} else {
			_UV1 = 1.0;
			_UV0 = float2( 0, 1-t );
		}
	} else {
		// Check for a horizontal cut
		float2	AlignedNormal = lsNormal.zx;
		float2	Delta = float2( 0, 1 ) - lsPosition.zx;
		float	D = dot( Delta, AlignedNormal );
		float	t = saturate( D / (2.0 * AlignedNormal.y ) );

//_Debug = float4( -Delta, 0, 0 );

		if ( AlignedNormal.y >= 0.0 ) {
			_UV0 = 0.0;
			_UV1 = float2( t, 1 );
		} else {
			_UV1 = 1.0;
			_UV0 = float2( 1-t, 0 );
		}
	}

	// Compute the solid angle
	float	SolidAngle = RectangleSolidAngle( lsPosition, _UV0, _UV1 );

	// Now, we can compute the projected solid angle by dotting with the normal
	float3	lsCenter = float3( (_UV1 + _UV0) - 1.0, 0.0 );
	float3	lsPosition2Center = normalize( lsCenter - lsPosition );
	_ProjectedSolidAngle = saturate( dot( lsNormal, lsPosition2Center ) ) * SolidAngle;	// (N.Wi) * dWi

// _Debug = _ProjectedSolidAngle;

//_ProjectedSolidAngle = 1;//UVArea;


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
bool	ComputeSolidAngleSpecular( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _TanHalfAngle, out float2 _UV0, out float2 _UV1, out float _ProjectedSolidAngle, out float4 _Debug ) {

_Debug = 0.0;

	float3	lsPosition = mul( float4( _wsPosition, 1.0 ), _World2AreaLight ).xyz;	// Transform world position into local area light space
	float3	lsView = mul( float4( _wsView, 0.0 ), _World2AreaLight ).xyz;			// Transform world direction into local area light space
	if ( lsPosition.z <= 0.0 || lsView.z >= 0.0 ) {
		// Position is behind area light or watching away from it...
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
	float	Distance2VirtualSource = lsPosition.z + _ProjectionDirectionSpec.z;
	float3	lsVirtualSourceCenter = -_ProjectionDirectionSpec;					// Center of the virtual source

	float2	HitMin = 1e6;
	float2	HitMax = -1e6;
	for ( uint Corner=0; Corner < 4; Corner++ ) {

		float3	vsDirection = float3( _TanHalfAngle * Dirs[Corner], 1.0 );						// Ray direction in view space
		float3	lsDirection = vsDirection.x * X + vsDirection.y * Y + vsDirection.z * lsView;	// Ray direction in local space

		float	t = -Distance2VirtualSource / lsDirection.z;									// Distance at which the ray hits the virtual source's plane
		float3	lsIntersection = lsPosition + t * lsDirection - lsVirtualSourceCenter;			// Hit position on the virtual source's plane, relative to its center

		// Keep min and max hit positions
		HitMin = min( HitMin, lsIntersection.xy );
		HitMax = max( HitMax, lsIntersection.xy );
	}

	// Compute the UV's from the hit positions
	_UV0 = 0.5 * (1.0 + float2( HitMin.x, -HitMin.y));
	_UV1 = max( _UV0 + dUV.xy, 0.5 * (1.0 + float2( HitMax.x, -HitMax.y)) );	// Make sure the UVs are at least separated by a single texel before clamping

//_Debug = float4( _UV1, 0, 0 );

	// Clamp to [0,1]
	_UV0 = saturate( _UV0 );
	_UV1 = saturate( _UV1 );

	// Compute the solid angle
	float2	DeltaUV = _UV1 - _UV0;
	float	UVArea = DeltaUV.x * DeltaUV.y;	// This is the perceived area in UV space
	float	wsArea = UVArea * _Area;		// This is the perceived area in world space

	float	SolidAngle = wsArea * -dot( _wsView, _AreaLight2World[2].xyz );	// dWi = Area * cos( theta )

	// Now, we can compute the projected solid angle by dotting with the normal
	_ProjectedSolidAngle = saturate( dot( _wsNormal, _wsView ) ) * SolidAngle;	// (N.Wi) * dWi


_ProjectedSolidAngle = 1;//UVArea;


// _UV0 = lsPosition.xy;
// _UV1 = lsPosition.z;
// _UV0 = _UV1;
// _UV1 = 0;

	return true;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
// 	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
//	float4	StainedGlass = SampleSATSinglePixel( _In.UV );

	float3	wsPosition = _In.Position;
	float3	wsNormal = normalize( _In.Normal );
	float3	wsView = normalize( wsPosition - _Camera2World[3].xyz );

	const float3	RhoD = 0.5;	// 50% diffuse albedo
	const float3	F0 = 0.04;							// DIELECTRIC
//	const float3	F0 = float3( 0.95, 0.94, 0.93 );	// METAL

 	float2	UV0, UV1;
 	float	SolidAngle;
 	float4	Debug;

 	// Compute diffuse lighting
 	float3	Ld = 0.0;
	if ( ComputeSolidAngleDiffuse( wsPosition, wsNormal, UV0, UV1, SolidAngle, Debug ) ) {

//return Debug;
//return float4( 100.0 * (UV1 - UV0), 0, 1 );
// float4	Test = float4( UV0, UV1 );
// return Test;

//SolidAngle = 1;
//return _LightIntensity * SolidAngle;

		float3	Irradiance = _LightIntensity * SampleSAT( UV0, UV1 ).xyz;
		Ld = RhoD / PI * Irradiance * SolidAngle;
		return float4( Ld, 1 );
	}

Ld = float3( 1, 1, 0 );

	// Compute specular lighting

//Calculer l'intersection avec le frustum et le portal, puis utiliser le facteur de diffusion pour grossir les UVs!! On va pas s'faire chier hein!


	float3	Ls = 0.0;
// 	float3	wsReflectedView = reflect( wsView, wsNormal );
// 	float	TanHalfAngle = tan( (1.0 - _Gloss) * 0.5 * PI );
//  	if ( ComputeSolidAngleSpecular( wsPosition, wsNormal, wsReflectedView, TanHalfAngle, UV0, UV1, SolidAngle, Debug ) ) {
// 
// // return Debug;
// 
// 		float3	Irradiance = _LightIntensity * SampleSAT( UV0, UV1 ).xyz;
// 		Ls = RhoS * Irradiance * SolidAngle;
// 	}

	// Compute Fresnel
	float	VdotN = saturate( dot( wsView, wsNormal ) );
	float3	IOR = Fresnel_IORFromF0( F0 );
	float3	FresnelSpecular = FresnelAccurate( IOR, VdotN );
	float3	FresnelDiffuse = 1.0 - FresnelSpecular;

//	return float4( FresnelDiffuse * Ld + FresnelSpecular * Ls, 1 );
	return float4( Ld + Ls, 1 );
}
