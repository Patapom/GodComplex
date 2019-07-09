#include "Global.hlsl"
#include "FGD.hlsl"
#include "LTC.hlsl"
#include "BRDF.hlsl"

struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float3	color : SV_TARGET0;
	float	depth : SV_DEPTH;
};

VS_IN	VS( VS_IN _in ) { return _in; }

// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
void BuildOrthonormalBasis( float3 _normal, out float3 _tangent, out float3 _bitangent ) {
	float a = _normal.z > -0.9999999 ? 1.0 / (1.0 + _normal.z) : 0.0;
	float b = -_normal.x * _normal.y * a;

	_tangent = float3( 1.0 - _normal.x*_normal.x*a, b, -_normal.x );
	_bitangent = float3( b, 1.0 - _normal.y*_normal.y*a, -_normal.y );
}

//
float	DiskIrradiance( float4x3 _tsQuadVertices ) {

	// 1) Extract center position, tangent and bi-tangent axes
	float3	T = 0.5 * (_tsQuadVertices[1] - _tsQuadVertices[2]);
	float3	B = -0.5 * (_tsQuadVertices[3] - _tsQuadVertices[2]);
	float3	P0 = 0.5 * (_tsQuadVertices[0] + _tsQuadVertices[2]);		// Half-way through the diagonal

	float	sqRt = dot( T, T );
	float	sqRb = dot( B, B );

	float3	N = normalize( cross( T, B ) );	// @TODO: Optimize! Do we need to normalize anyway?

	// 2) Build frustum matrices
		// M transform P' = M * P into canonical frustum space
	float3x3	M;
	float		invDepth = 1.0 / dot(P0,N);
	M[0] = (T - dot(P0,T) * invDepth * N) / sqRt;
	M[1] = (B - dot(P0,B) * invDepth * N) / sqRb;
	M[2] = N * invDepth;

		// M^-1 transforms P = M^-1 * P' back into original space
	float3x3	invM;
	invM._m00_m10_m20 = T;
	invM._m01_m11_m21 = B;
	invM._m02_m12_m22 = P0;

		// Compute the determinant of M^-1 that will help us scale the resulting vector
	float	det = determinant( M );
//	float	det = sqrt( sqRt * sqRb ) * dot( P0, N );

	// 3) Compute the exact integral in the canonical space
	// We know the expression of the orthogonal vector at any point on the unit circle as:
	//
	//							| cos(theta)
	//	Tau(theta) = 1/sqrt(2)  | sin(theta)
	//							| 1
	//
	// We move the orientation so we always compute 2 symetrical integrals on a half circle
	//	so we only need to compute the X component integral as the Y components have opposite
	//	sign and simply cancel each other
	//
	// The integral we need to compute is thus:
	//
	//	X = Integral[-maxTheta,maxTheta]{ cos(theta) dtheta }
	//	X = 2 * sin(maxTheta)
	//
	// The Z component is straightforward Z = Integral[-maxTheta,maxTheta]{ dtheta } = 2 maxTheta
	//
	float	cosMaxTheta = -1.0;	// No clipping at the moment
	float	maxTheta = acos( clamp( cosMaxTheta, -1, 1 ) );	// @TODO: Optimize! Use fast acos or something...

	float3	F = (1.0 / (2.0 * PI * sqrt(2))) * float3( 2 * sqrt( 1 - cosMaxTheta*cosMaxTheta ), 0, 2 * maxTheta );

	return length(F) * det;

//	// 4) Transform back into LTC space using M^-1
//	float3	F2 = mul( invM, F );	// @TODO: Optimize => simply return length(F2) = determinant of M^-1
//
//	// 5) Estimate scalar irradiance
////	return -1.0 / det;
////	return abs(F2.z);
//	return length( F2 );
}

// Computes the diffuse and specular luminance of the area light that is reflected from the surface
void	ComputeLTCAreaLightLuminance( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _alphaD, float _alphaS, out float3 _diffuse, out float3 _specular ) {
	// Build tangent space
	#if 1
		float3	wsTangent, wsBiTangent;
		BuildOrthonormalBasis( _wsNormal, wsTangent, wsBiTangent );
	#else
		// Construct a right-handed view-dependent orthogonal basis around the normal
		float3		wsTangent = normalize( _wsView - _wsNormal * dot( _wsView, _wsNormal ) );
		float3		wsBiTangent = cross( _wsNormal, wsTangent );
	#endif

	float3x3	world2TangentSpace = transpose( float3x3( wsTangent, wsBiTangent, _wsNormal ) );
//	float3		tsView = mul( _wsView, world2TangentSpace );

	float		VdotN = saturate( dot( _wsView, _wsNormal ) );

	float		perceptualAlphaD = sqrt( _alphaD );
	float		perceptualAlphaS = sqrt( _alphaS );

	float3x3	LTC_diffuse = LTCSampleMatrix( VdotN, perceptualAlphaD, LTC_BRDF_INDEX_OREN_NAYAR );
	float3x3	LTC_specular = LTCSampleMatrix( VdotN, perceptualAlphaS, LTC_BRDF_INDEX_GGX );
	float		magnitude_diffuse = SampleIrradiance( VdotN, _alphaD, FGD_BRDF_INDEX_OREN_NAYAR );
	float3		magnitude_specular = SampleIrradiance( VdotN, _alphaS, FGD_BRDF_INDEX_GGX );

	// Build rectangular area light corners in local space
	float3		lsAreaLightPosition = _wsLight2World[3].xyz - _wsPosition;
	float4x3    lsLightCorners;
				lsLightCorners[0] = lsAreaLightPosition + _wsLight2World[0].w * _wsLight2World[0].xyz + _wsLight2World[1].w * _wsLight2World[1].xyz;
				lsLightCorners[1] = lsAreaLightPosition + _wsLight2World[0].w * _wsLight2World[0].xyz - _wsLight2World[1].w * _wsLight2World[1].xyz;
				lsLightCorners[2] = lsAreaLightPosition - _wsLight2World[0].w * _wsLight2World[0].xyz - _wsLight2World[1].w * _wsLight2World[1].xyz;
				lsLightCorners[3] = lsAreaLightPosition - _wsLight2World[0].w * _wsLight2World[0].xyz + _wsLight2World[1].w * _wsLight2World[1].xyz;

	float4x3    tsLightCorners = mul( lsLightCorners, world2TangentSpace );		// Transform them into tangent-space

	float3		Li = _diskLuminance;

	#if 1
		// Optimized disk area light
//		_diffuse = Li * magnitude_diffuse * DiskIrradiance( mul( tsLightCorners, LTC_diffuse ) );	// Diffuse LTC is already multiplied by 1/PI
_diffuse = Li * magnitude_diffuse * DiskIrradiance( tsLightCorners );	// Diffuse LTC is already multiplied by 1/PI
		_specular = Li * magnitude_specular * DiskIrradiance( mul( tsLightCorners, LTC_specular ) );
	#else
		// Rectangular area light
		_diffuse = Li * magnitude_diffuse * PolygonIrradiance( mul( tsLightCorners, LTC_diffuse ) );	// Diffuse LTC is already multiplied by 1/PI
		_specular = Li * magnitude_specular * PolygonIrradiance( mul( tsLightCorners, LTC_specular ) );
	#endif
}

PS_OUT	PS( VS_IN _In ) {
	float3	csView = GenerateCameraRay( _In.__Position.xy );
	float3	wsView = mul( float4( csView, 0 ), _camera2World ).xyz;
	float3	wsCamPos = _camera2World[3].xyz;

	float	t = -wsCamPos.y / wsView.y;
	clip( t );

	float3	wsPos = wsCamPos + t * wsView;

	float3	wsNormal = float3( 0, 1, 0 );	// Simple plane
	float3	diffuseAlbedo = 0.5;			// Assume a regular 50% diffuse reflectance
	float3	specularF0 = 0.04;				// 1.5 IOR
	float	roughnessDiffuse = 0.0;			// Lambert
	float	roughnessSpecular = 0.2;

	// Compute reference diffuse lighting
	float3	diffuse, specular;
	ComputeLTCAreaLightLuminance( wsPos, wsNormal, -wsView, roughnessDiffuse, roughnessSpecular, diffuse, specular );
	diffuse *= diffuseAlbedo;

	float4	ndcPos = mul( float4( wsPos, 1 ), _world2Proj );

	PS_OUT	result;
	result.color = diffuse;
	result.depth = ndcPos.z / ndcPos.w;

	return result;
}
