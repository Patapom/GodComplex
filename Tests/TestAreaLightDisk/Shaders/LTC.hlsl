/////////////////////////////////////////////////////////////////////////////////////////////////////
// LTC Area Light Support
//
Texture2DArray< float4 >	_tex_LTC : register(t4);
Texture2D< float2 >			_tex_MS_LTC : register(t5);

#define LTC_LUT_SIZE	64	// LTC LUTs are 64x64
#define LTC_BRDFS_COUNT	6	// Only 6 BRDF types are supported at the moment

// Specular BRDFs
#define LTC_BRDF_INDEX_GGX				0
#define LTC_BRDF_INDEX_COOK_TORRANCE	1
#define LTC_BRDF_INDEX_WARD				2

// Diffuse BRDFs
#define LTC_BRDF_INDEX_OREN_NAYAR		3
#define LTC_BRDF_INDEX_CHARLIE			4
#define LTC_BRDF_INDEX_DISNEY			5


/////////////////////////////////////////////////////////////////////////////////////////////////////
// LTC Table Sampling Helpers
// Expects _NdotV clamped in [0,1]
// 
// U = Perceptual roughness
// V = sqrt( 1 - N.V )      <== Cheaper
//
float2	LTCGetSamplingUV( float _NdotV, float _perceptualRoughness )
{
	float2  xy;
	xy.x = _perceptualRoughness;
	xy.y = sqrt( 1 - _NdotV );	// Now, we use V = sqrt( 1 - cos(theta) ) which is kind of linear and only requires a single sqrt() instead of an expensive acos()

	xy *= (LTC_LUT_SIZE-1);		// 0 is pixel 0, 1 = last pixel in the table
	xy += 0.5;					// Perfect pixel sampling starts at the center
	return xy / LTC_LUT_SIZE;	// Finally, return UVs in [0,1]
}

// Fetches the transposed M^-1 matrix need for runtime LTC estimate
// Texture contains XYZW = { m00, m20, m02, m22 } coefficients of the M^-1 matrix. All other coefficients except m11=1 are assumed to be 0
// Note we load the matrix transposed (to avoid having to transpose it in shader) so we must use it as mul( point, invM )
//
float3x3	LTCSampleMatrix( float2 _UV, uint _BRDFIndex ) {
	float3x3	invM_transposed = 0.0;
				invM_transposed._m11 = 1.0;
				invM_transposed._m00_m20_m02_m22 = _tex_LTC.SampleLevel( LinearClamp, float3( _UV, _BRDFIndex ), 0 );

	return invM_transposed;
}

float3x3	LTCSampleMatrix( float _NdotV, float _perceptualRoughness, uint _BRDFIndex ) {
	return LTCSampleMatrix( LTCGetSamplingUV( _NdotV, _perceptualRoughness ), _BRDFIndex );
}

// Multiple-Scattering Table Fetch
// Texture contains XY = { m22, magnitude }, a single coefficient of the isotropic M^-1 matrix + the BRDF's magnitude
float3x3	MSLTCSampleMatrix( float _perceptualRoughness, uint _BRDFIndex, out float _magnitude ) {
	float2		UV = float2( _perceptualRoughness, (0.5 + _BRDFIndex) / LTC_BRDFS_COUNT );
	float2		coeffs = _tex_MS_LTC.SampleLevel( LinearClamp, UV, 0.0 );
	_magnitude = coeffs.y;

	float3x3	invM = 0.0;
				invM._m00_m11 = 1.0;
				invM._m22 = coeffs.x;

	return invM;
}

// Only returns the diagonal since it's the only factors that matter
float3	MSLTCSampleMatrixDiagonal( float _perceptualRoughness, uint _BRDFIndex, out float _magnitude ) {
	float2		UV = float2( _perceptualRoughness, (0.5 + _BRDFIndex) / LTC_BRDFS_COUNT );
	float2		coeffs = _tex_MS_LTC.SampleLevel( LinearClamp, UV, 0.0 );
	_magnitude = coeffs.y;

	return float3( 1, 1, coeffs.x );
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// LTC Estimators
// Code from Unity's Scriptable HD Render Pipeline

// Not normalized by the factor of 1/TWO_PI.
float3	ComputeEdgeFactor( float3 V1, float3 V2 ) {
	float	V1oV2 = dot(V1, V2);
	float3	V1xV2 = cross(V1, V2);

	// Approximate: { y = rsqrt(1.0 - V1oV2 * V1oV2) * acos(V1oV2) } on [0, 1].
	// Fit: HornerForm[MiniMaxApproximation[ArcCos[x]/Sqrt[1 - x^2], {x, {0, 1 - $MachineEpsilon}, 6, 0}][[2, 1]]].
	// Maximum relative error: 2.6855360216340534 * 10^-6. Intensities up to 1000 are artifact-free.
	float x = abs(V1oV2);
	float y = 1.5707921083647782 + x * (-0.9995697178013095 + x * (0.778026455830408 + x * (-0.6173111361273548 + x * (0.4202724111150622 + x * (-0.19452783598217288 + x * 0.04232040013661036)))));

	if ( V1oV2 < 0 ) {
		y = PI * rsqrt(saturate(1 - V1oV2 * V1oV2)) - y;	// Undo range reduction.
	}

	return V1xV2 * y;
}

// Not normalized by the factor of 1/TWO_PI.
// Ref: Improving radiosity solutions through the use of analytically determined form-factors.
// 'V1' and 'V2' are represented in a coordinate system with N = (0, 0, 1).
float IntegrateEdge( float3 V1, float3 V2 ) {
	return ComputeEdgeFactor( V1, V2 ).z;
}

// Expects non-normalized vertex positions.
float PolygonIrradiance( float4x3 L ) {

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Super expensive clipping
	// Unity doesn't do that: they replace the rectangular area light's form factor by the one from an equivalent sphere
	//	and then clip the sphere instead (there are some exact computations for a plane/sphere clipping=
	//
	// 1. ClipQuadToHorizon

	// detect clipping config
	uint config = 0;
	if (L[0].z > 0) config += 1;
	if (L[1].z > 0) config += 2;
	if (L[2].z > 0) config += 4;
	if (L[3].z > 0) config += 8;

	// The fifth vertex for cases when clipping cuts off one corner.
	// Due to a compiler bug, copying L into a vector array with 5 rows
	// messes something up, so we need to stick with the matrix + the L4 vertex.
	float3 L4 = L[3];

	// This switch is surprisingly fast. Tried replacing it with a lookup array of vertices.
	// Even though that replaced the switch with just some indexing and no branches, it became
	// way, way slower - mem fetch stalls?

	// clip
	uint n = 0;
	switch (config)
	{
	case 0: // clip all
		break;

	case 1: // V1 clip V2 V3 V4
		n = 3;
		L[1] = -L[1].z * L[0] + L[0].z * L[1];
		L[2] = -L[3].z * L[0] + L[0].z * L[3];
		break;

	case 2: // V2 clip V1 V3 V4
		n = 3;
		L[0] = -L[0].z * L[1] + L[1].z * L[0];
		L[2] = -L[2].z * L[1] + L[1].z * L[2];
		break;

	case 3: // V1 V2 clip V3 V4
		n = 4;
		L[2] = -L[2].z * L[1] + L[1].z * L[2];
		L[3] = -L[3].z * L[0] + L[0].z * L[3];
		break;

	case 4: // V3 clip V1 V2 V4
		n = 3;
		L[0] = -L[3].z * L[2] + L[2].z * L[3];
		L[1] = -L[1].z * L[2] + L[2].z * L[1];
		break;

	case 5: // V1 V3 clip V2 V4: impossible
		break;

	case 6: // V2 V3 clip V1 V4
		n = 4;
		L[0] = -L[0].z * L[1] + L[1].z * L[0];
		L[3] = -L[3].z * L[2] + L[2].z * L[3];
		break;

	case 7: // V1 V2 V3 clip V4
		n = 5;
		L4 = -L[3].z * L[0] + L[0].z * L[3];
		L[3] = -L[3].z * L[2] + L[2].z * L[3];
		break;

	case 8: // V4 clip V1 V2 V3
		n = 3;
		L[0] = -L[0].z * L[3] + L[3].z * L[0];
		L[1] = -L[2].z * L[3] + L[3].z * L[2];
		L[2] = L[3];
		break;

	case 9: // V1 V4 clip V2 V3
		n = 4;
		L[1] = -L[1].z * L[0] + L[0].z * L[1];
		L[2] = -L[2].z * L[3] + L[3].z * L[2];
		break;

	case 10: // V2 V4 clip V1 V3: impossible
		break;

	case 11: // V1 V2 V4 clip V3
		n = 5;
		L[3] = -L[2].z * L[3] + L[3].z * L[2];
		L[2] = -L[2].z * L[1] + L[1].z * L[2];
		break;

	case 12: // V3 V4 clip V1 V2
		n = 4;
		L[1] = -L[1].z * L[2] + L[2].z * L[1];
		L[0] = -L[0].z * L[3] + L[3].z * L[0];
		break;

	case 13: // V1 V3 V4 clip V2
		n = 5;
		L[3] = L[2];
		L[2] = -L[1].z * L[2] + L[2].z * L[1];
		L[1] = -L[1].z * L[0] + L[0].z * L[1];
		break;

	case 14: // V2 V3 V4 clip V1
		n = 5;
		L4 = -L[0].z * L[3] + L[3].z * L[0];
		L[0] = -L[0].z * L[1] + L[1].z * L[0];
		break;

	case 15: // V1 V2 V3 V4
		n = 4;
		break;
	}

	if ( n == 0 )
		return 0;

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// 2. Project onto sphere
	L[0] = normalize(L[0]);
	L[1] = normalize(L[1]);
	L[2] = normalize(L[2]);

	switch (n) {
		case 3:
			L[3] = L[0];
			break;
		case 4:
			L[3] = normalize(L[3]);
			L4   = L[0];
			break;
		case 5:
			L[3] = normalize(L[3]);
			L4   = normalize(L4);
			break;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// 3. Integrate
	float sum = 0;
	sum += IntegrateEdge(L[0], L[1]);
	sum += IntegrateEdge(L[1], L[2]);
	sum += IntegrateEdge(L[2], L[3]);
	if (n >= 4)
		sum += IntegrateEdge(L[3], L4);
	if (n == 5)
		sum += IntegrateEdge(L4, L[0]);

	sum *= 0.5*INVPI;	// Normalization

	sum = max(sum, 0.0);

	return isfinite(sum) ? sum : 0.0;
}
