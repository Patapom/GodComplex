// Ward specular reflection model
// From "A New Ward BRDF with Bounded Albedo" by Dür et al.
// (isotropic version, I know it's stupid since Ward is essentially interesting for its anisotropic characteristics but in the engine we don't even use it...)
//
float	ComputeWard( float3 _Light, float3 _View, float3 _Normal, float3 _Tangent, float3 _BiTangent, float _Roughness ) {

	float	invRoughness = 1.0 / (1e-3 + _Roughness);
	float	invSqRoughness = invRoughness * invRoughness;

	float3	H_unorm = _Light + _View;
	float	HdotH = dot( H_unorm, H_unorm );
	float	HdotN = dot( H_unorm, _Normal );
	float	HdotT = dot( H_unorm, _Tangent );
	float	HdotB = dot( H_unorm, _BiTangent );
			HdotN = max( 1e-4, HdotN );

// 	float	sqHdotN = HdotN * HdotN;
// 	float	invSqHdotN = 1.0 / sqHdotN;

	float	invHdotN_2 = 1.0 / (HdotN * HdotN);
	float	invHdotN_4 = invHdotN_2 * invHdotN_2;

	float	sqTanHdotN = invHdotN_2 * (HdotT*HdotT + HdotB*HdotB);
	return INVPI * invSqRoughness * (exp( -invSqRoughness * sqTanHdotN ) * HdotH * invHdotN_4);
}