/////////////////////////////////////////////////////////////////////////////////////////////////////
// Pre-Integrated BRDF irradiance Support
//
Texture2DArray< float >	_tex_Eo : register( t2 );
Texture2D< float >		_tex_Eavg : register( t3 );

#define FDG_BRDFS_COUNT	6	// Only 6 BRDF types are supported at the moment

// Specular BRDFs
#define FDG_BRDF_INDEX_GGX				0
#define FDG_BRDF_INDEX_COOK_TORRANCE	1	// Not available => GGX is stored in that slot
#define FDG_BRDF_INDEX_WARD				2	// Not available => GGX is stored in that slot

// Diffuse BRDFs
#define FDG_BRDF_INDEX_OREN_NAYAR		3
#define FDG_BRDF_INDEX_CHARLIE			4	// Not available => Oren-Nayar is stored in that slot
#define FDG_BRDF_INDEX_DISNEY			5	// Not available => Oren-Nayar is stored in that slot


/////////////////////////////////////////////////////////////////////////////////////////////////////
// 
float	SampleIrradiance( float _cosTheta, float _alpha, uint _BRDFIndex ) {
	return _tex_Eo.SampleLevel( LinearClamp, float3( _cosTheta, _alpha, _BRDFIndex ), 0.0 );
}

float	SampleAlbedo( float _alpha, uint _BRDFIndex ) {
	return _tex_Eavg.SampleLevel( LinearClamp, float2( _alpha, (0.5 + _BRDFIndex) / FDG_BRDFS_COUNT ), 0.0 );
}
