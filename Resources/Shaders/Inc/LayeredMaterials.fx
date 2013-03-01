////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef _LAYERED_MATERIALS_H_
#define	_LAYERED_MATERIALS_H_

#define	MAX_MATERIALS	64

struct	MaterialParams
{
	float4	AmplitudeFalloff;
	float4	ExponentDiffuse;
	float	Offset;
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
	Result.CosThetaD = clamp( dot( _TSLight, Result.Half ), -1.0, +1.0 );

	Result.ThetaHD = float2( acos( Result.CosThetaH ), acos( Result.CosThetaD ) );
	Result.PhiD = 0.0;	// If needed later... ?

	Result.UV = INVHALFPI * Result.ThetaHD;
	Result.UV.y = 1.0 - Result.UV.y;

	return Result;
}

////////////////////////////////////////////////////////////////////////////////////////
//
struct	WeightMatID
{
	uint	ID;
	float	Weight;
};

MaterialParams	ComputeWeightedMaterialParams( WeightMatID _MatLayers[4] )
{
	MaterialParams	M[4] = {
		_Materials[_MatLayers[0].ID],
		_Materials[_MatLayers[1].ID],
		_Materials[_MatLayers[2].ID],
		_Materials[_MatLayers[3].ID],
	};

	float4	W = float4( _MatLayers[0].Weight, _MatLayers[1].Weight, _MatLayers[2].Weight, _MatLayers[3].Weight );

	MaterialParams	R;
	R.AmplitudeFalloff = W.x * M[0].AmplitudeFalloff + W.y * M[1].AmplitudeFalloff + W.z * M[2].AmplitudeFalloff + W.w * M[3].AmplitudeFalloff;
	R.ExponentDiffuse = W.x * M[0].ExponentDiffuse + W.y * M[1].ExponentDiffuse + W.z * M[2].ExponentDiffuse + W.w * M[3].ExponentDiffuse;
	R.Offset = W.x * M[0].Offset + W.y * M[1].Offset + W.z * M[2].Offset + W.w * M[3].Offset;

	return R;
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
	float	Fd90 = 0.5 + _MatParams.ExponentDiffuse.w * _ViewParams.CosThetaD * _ViewParams.CosThetaD;
	float	a = 1.0 - _ViewParams.TSLight.z;	// 1-cos(ThetaL) = 1-cos(ThetaV)
	float	Cos5 = a * a;
			Cos5 *= Cos5 * a;
	Result.Diffuse = 1.0 + (Fd90-1)*Cos5;
	Result.Diffuse *= Result.Diffuse;						// Diffuse uses double Fresnel from both ThetaV and ThetaL

	Result.RetroDiffuse = max( 0.0, Result.Diffuse-1.0 );	// Retro-reflection starts above 1
	Result.Diffuse = min( 1.0, Result.Diffuse );			// Clamp diffuse to avoid double-counting retro-reflection...

	Result.Diffuse *= _MatParams.ExponentDiffuse.z * INVPI;
	Result.RetroDiffuse *= _MatParams.ExponentDiffuse.z * INVPI;

	// =================== COMPUTE SPECULAR ===================
	float2	Cxy = _MatParams.Offset + _MatParams.AmplitudeFalloff.xy * exp( _MatParams.AmplitudeFalloff.zw * pow( _ViewParams.UV, _MatParams.ExponentDiffuse.xy ) );

	Result.Specular = Cxy.x * Cxy.y - _MatParams.Offset*_MatParams.Offset;	// Specular & Fresnel lovingly modulating each other

	return Result;
}

#endif	// _LAYERED_MATERIALS_H_
