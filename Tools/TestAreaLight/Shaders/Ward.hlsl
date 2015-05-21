// Ward specular reflection model
// From "A New Ward BRDF with Bounded Albedo" by Dür et al.
// (isotropic version, I know it's stupid since Ward is essentially interesting for its anisotropic characteristics but in the engine we don't even use it...)
//
float	ComputeWard( float3 _Light, float3 _View, float3 _Normal, float3 _Tangent, float3 _BiTangent, float _Roughness ) {

	float3	H_unorm = _Light + _View;
	float	invRoughness = 1.0 / _Roughness;
	float	invSqRoughness = invRoughness * invRoughness;

#if 1
	float	HdotH = dot( H_unorm, H_unorm );
	float	HdotN = dot( H_unorm, _Normal );
	float	HdotT = dot( H_unorm, _Tangent );
	float	HdotB = dot( H_unorm, _BiTangent );
	float	sqHdotN = HdotN * HdotN;
	float	invSqHdotN = 1.0 / sqHdotN;
	float	sqTanHdotN = (HdotT*HdotT + HdotB*HdotB) * invSqHdotN;
	return INVPI * invSqRoughness * (exp( -invSqRoughness * sqTanHdotN ) * invSqHdotN * invSqHdotN) * HdotH;
#else
	float3	H = normalize( H_unorm );
	float	NdotH = dot( _Normal, H );
	float	LdotH = dot( _Light, H );
	float	cosTheta = NdotH;
	float	sqCosTheta = cosTheta * cosTheta;
	float	sqSinTheta = 1.0 - sqCosTheta;
	float	sqTanTheta = sqSinTheta / sqCosTheta;
	return INVPI * invSqRoughness * exp( -invSqRoughness * sqTanTheta ) / (4 * NdotH*NdotH*NdotH*NdotH * LdotH*LdotH);
#endif
}