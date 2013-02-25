////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef _LAYERED_MATERIALS_H_
#define	_LAYERED_MATERIALS_H_

#define	MAX_MATERIALS	64

struct	MaterialParams
{
	// Specular (X) & Fresnel (Y) parameters
	float2	Amplitude;
	float2	Falloff;
	float2	Exponent;
	float	Offset;

	// Diffuse parameters
	float	DiffuseReflectance;
	float	DiffuseRoughness;
};

tbuffer	tbMaterials : register( t8 )
{
    MaterialParams	_Materials[MAX_MATERIALS];
};

////////////////////////////////////////////////////////////////////////////////////////
// Transform from tangent space to half-vector space
struct	HalfVectorSpaceParams
{
	float3	TSView, TSLight;
	float3	Half;
	float	CosThetaH, CosThetaD;
	float2	ThetaHD;
	float	PhiD;
	float2	UV;		// Material slice UVs
};

HalfVectorSpaceParams	Tangent2HalfVector( float3 _TSView, float3 _TSLight )
{
	HalfVectorSpaceParams	Result;
	Result.TSView = _TSView;
	Result.TSLight = _TSLight;
	Result.Half = normalize( _TSLight + _TSView );
	Result.CosThetaH = Result.Half.z;
	Result.CosThetaD = dot( _TSLight, Result.Half );

	Result.ThetaHD = float2( acos( Result.CosThetaH ), acos( Result.CosThetaD ) );
	Result.PhiD = 0.0;	// If needed later... ?

	Result.UV = INVHALFPI * Result.ThetaHD;
	Result.UV.y = 1.0 - Result.UV.y;

	return Result;
}

////////////////////////////////////////////////////////////////////////////////////////
// Material reflectance evaluation
struct	MatReflectance
{
	float	Specular;
	float	Diffuse;
	float	RetroDiffuse;
	// Total reflectance is the sum of all these values...
};

MatReflectance	LayeredMatEval( HalfVectorSpaceParams _ViewParams, MaterialParams _MatParams )
{
	MatReflectance	Result;

	// =================== COMPUTE DIFFUSE ===================
	// I borrowed the diffuse term from §5.3 of http://disney-animation.s3.amazonaws.com/library/s2012_pbs_disney_brdf_notes_v2.pdf
	float	Fd90 = 0.5 + _MatParams.DiffuseRoughness * _ViewParams.CosThetaD * _ViewParams.CosThetaD;
	float	a = 1.0 - _ViewParams.TSLight.z;	// 1-cos(ThetaL) = 1-cos(ThetaV)
	float	Cos5 = a * a;
			Cos5 *= Cos5 * a;
	Result.Diffuse = 1.0 + (Fd90-1)*Cos5;
	Result.Diffuse *= Result.Diffuse;					// Diffuse uses double Fresnel from both ThetaV and ThetaL

	Result.RetroDiffuse = max( 0, Result.Diffuse-1 );	// Retro-reflection starts above 1
	Result.Diffuse = min( 1, Result.Diffuse );			// Clamp diffuse to avoid double-counting retro-reflection...

	Result.Diffuse *= INVPI;
	Result.RetroDiffuse *= INVPI;

	// =================== COMPUTE SPECULAR ===================
	float2	Cxy = _MatParams.Offset + _MatParams.Amplitude * exp( _MatParams.Falloff * pow( _ViewParams.UV, _MatParams.Exponent ) );

	Result.Specular = Cxy.x * Cxy.y - _MatParams.Offset*_MatParams.Offset;	// Specular & Fresnel lovingly modulating each other

	return Result;
}

#endif	// _LAYERED_MATERIALS_H_
