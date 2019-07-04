/////////////////////////////////////////////////////////////////////////////////////////////////////
// Pre-Integrated BRDF irradiance Support
//
Texture2DArray< float >	_tex_Eo : register( t2 );
Texture2D< float >		_tex_Eavg : register( t3 );

#define FGD_BRDFS_COUNT	2	// Only 2 BRDF types are supported at the moment

// Specular BRDFs
#define FGD_BRDF_INDEX_GGX				0

// Diffuse BRDFs
#define FGD_BRDF_INDEX_OREN_NAYAR		1


/////////////////////////////////////////////////////////////////////////////////////////////////////
// 
float	SampleIrradiance( float _cosTheta, float _alpha, uint _BRDFIndex ) {
	return _tex_Eo.SampleLevel( LinearClamp, float3( _cosTheta, _alpha, _BRDFIndex ), 0.0 );
}

float	SampleAlbedo( float _alpha, uint _BRDFIndex ) {
	return _tex_Eavg.SampleLevel( LinearClamp, float2( _alpha, (0.5 + _BRDFIndex) / FGD_BRDFS_COUNT ), 0.0 );
}
