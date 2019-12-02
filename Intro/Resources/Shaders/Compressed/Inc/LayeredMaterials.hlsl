////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef _LAYERED_MATERIALS_H_
#define	_LAYERED_MATERIALS_H_

#define	MAX_MATERIALS	64

static const float2	MATERIAL_MIN_LOG_AMPLITUDE = float2( -6.9077552789821370520539743640531, -6.9077552789821370520539743640531 );	// log(0.001), log(0.001)
static const float2	MATERIAL_MAX_LOG_AMPLITUDE = float2( +7.6009024595420823614712064855113, +3.9120230054281460586187507879106 );	// log(2000), log(50)
static const float2	MATERIAL_DELTA_LOG_AMPLITUDE = MATERIAL_MAX_LOG_AMPLITUDE - MATERIAL_MIN_LOG_AMPLITUDE;


struct	MaterialParams
{
	float4	AmplitudeFalloff;	// Here, amplitudes & falloffs are stored in log-space so they interpolate better
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

// This structure embeds 2 layers of layered materials
// Layer0 & 2 are collapsed into a single array of coefficients that will be stored in the XY components of the float4s
// Layer1 & 3 are collapsed into a single array of coefficients that will be stored in the ZW components of the float4s
struct	DualMaterialParams
{
	float4	Amplitude;
	float4	Falloff;
	float4	Exponent;
	float4	Diffuse;
	float2	Offset;
};

// Blends all 4 layers together
MaterialParams	ComputeSingleWeightedMaterialParams( WeightMatID _MatLayers[4] )
{
	MaterialParams	M[4] = {
		_Materials[_MatLayers[0].ID],
		_Materials[_MatLayers[1].ID],
		_Materials[_MatLayers[2].ID],
		_Materials[_MatLayers[3].ID],
	};

	float4	W = float4( _MatLayers[0].Weight, _MatLayers[1].Weight, _MatLayers[2].Weight, _MatLayers[3].Weight );
	float	SumWeights = max( 1.0, dot( W, 1.0 ) );
	W /= SumWeights;	// We normalize weights as we can't exceed one since we're collapsing all 4 material params into a single list of interpolated params!

	MaterialParams	R;
	R.AmplitudeFalloff = W.x * M[0].AmplitudeFalloff + W.y * M[1].AmplitudeFalloff + W.z * M[2].AmplitudeFalloff + W.w * M[3].AmplitudeFalloff;
	R.ExponentDiffuse = W.x * M[0].ExponentDiffuse + W.y * M[1].ExponentDiffuse + W.z * M[2].ExponentDiffuse + W.w * M[3].ExponentDiffuse;
	R.Offset = W.x * M[0].Offset + W.y * M[1].Offset + W.z * M[2].Offset + W.w * M[3].Offset;

	// Amplitude is coded in log space AND brought back in [0,1]
	R.AmplitudeFalloff.xy = MATERIAL_MIN_LOG_AMPLITUDE.xy + MATERIAL_DELTA_LOG_AMPLITUDE.xy * R.AmplitudeFalloff.xy;

	return R;
}

// Blends layers 0 & 2 together into a first set of parameters, and layers 1 & 3 into a second set
DualMaterialParams	ComputeDualWeightedMaterialParams( WeightMatID _MatLayers[4] )
{
	MaterialParams	M[4] = {
		_Materials[_MatLayers[0].ID],
		_Materials[_MatLayers[1].ID],
		_Materials[_MatLayers[2].ID],
		_Materials[_MatLayers[3].ID],
	};

	float4	W = float4( _MatLayers[0].Weight, _MatLayers[1].Weight, _MatLayers[2].Weight, _MatLayers[3].Weight );
	float2	SumWeights = max( 1.0, float2( W.x + W.z, W.y + W.w ) );
	W /= SumWeights.xyxy;	// We normalize weights as we can't exceed one since we're collapsing all 4 material params into two lists of interpolated params!

	DualMaterialParams	R;
	R.Amplitude = float4( W.x * M[0].AmplitudeFalloff.xy + W.z * M[2].AmplitudeFalloff.xy, W.y * M[1].AmplitudeFalloff.xy + W.w * M[3].AmplitudeFalloff.xy );	// Interlace amplitudes
	R.Falloff = float4( W.x * M[0].AmplitudeFalloff.zw + W.z * M[2].AmplitudeFalloff.zw, W.y * M[1].AmplitudeFalloff.zw + W.w * M[3].AmplitudeFalloff.zw );		// Interlace falloffs
	R.Exponent = float4( W.x * M[0].ExponentDiffuse.xy + W.z * M[2].ExponentDiffuse.xy, W.y * M[1].ExponentDiffuse.xy + W.w * M[3].ExponentDiffuse.xy );		// Interlace exponents
	R.Diffuse = float4( W.x * M[0].ExponentDiffuse.zw + W.z * M[2].ExponentDiffuse.zw, W.y * M[1].ExponentDiffuse.zw + W.w * M[3].ExponentDiffuse.zw );			// Interlace diffuses
	R.Offset = float2( W.x * M[0].Offset + W.z * M[2].Offset, W.y * M[1].Offset + W.w * M[3].Offset );															// Interlace offsets

	// Amplitude is coded in log space AND brought back in [0,1]
	R.Amplitude = MATERIAL_MIN_LOG_AMPLITUDE.xyxy + MATERIAL_DELTA_LOG_AMPLITUDE.xyxy * R.Amplitude;

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

// Version with 1 layer
MatReflectance	LayeredMatEval( HalfVectorSpaceParams _ViewParams, MaterialParams _MatParams )
{
	MatReflectance	Result;

	// =================== COMPUTE DIFFUSE ===================
	// I borrowed the diffuse term from §5.3 of http://disney-animation.s3.amazonaws.com/library/s2012_pbs_disney_brdf_notes_v2.pdf
	float	Fd90 = 0.5 + _MatParams.ExponentDiffuse.w * _ViewParams.CosThetaD * _ViewParams.CosThetaD;
	float	a = 1.0 - _ViewParams.TSLight.z;	// 1-cos(ThetaL) = 1-cos(ThetaV)
	float	Cos5 = a * a;
			Cos5 *= Cos5 * a;
	float	Diffuse = 1.0 + (Fd90-1.0)*Cos5;
			Diffuse *= Diffuse;						// Diffuse uses double Fresnel from both ThetaV and ThetaL

	Result.RetroDiffuse = max( 0.0, Diffuse-1.0 );	// Retro-reflection starts above 1
	Result.Diffuse = min( 1.0, Diffuse );			// Clamp diffuse to avoid double-counting retro-reflection...

	Result.Diffuse *= _MatParams.ExponentDiffuse.z * INVPI;
	Result.RetroDiffuse *= _MatParams.ExponentDiffuse.z * INVPI;

	// =================== COMPUTE SPECULAR ===================
//	float2	Cxy = _MatParams.Offset + _MatParams.AmplitudeFalloff.xy * exp( _MatParams.AmplitudeFalloff.zw * pow( _ViewParams.UV, _MatParams.ExponentDiffuse.xy ) );
	float2	Cxy = _MatParams.Offset + exp( _MatParams.AmplitudeFalloff.xy + _MatParams.AmplitudeFalloff.zw * pow( _ViewParams.UV, _MatParams.ExponentDiffuse.xy ) );

	Result.Specular = Cxy.x * Cxy.y - _MatParams.Offset*_MatParams.Offset;	// Specular & Fresnel lovingly modulating each other

	return Result;
}

// Version with 2 layers
MatReflectance	LayeredMatEval( HalfVectorSpaceParams _ViewParams, DualMaterialParams _MatParams )
{
	MatReflectance	Result;

	// =================== COMPUTE DIFFUSE ===================
	// I borrowed the diffuse term from §5.3 of http://disney-animation.s3.amazonaws.com/library/s2012_pbs_disney_brdf_notes_v2.pdf
	float2	Fd90 = 0.5 + _MatParams.Diffuse.yw * _ViewParams.CosThetaD * _ViewParams.CosThetaD;
	float	a = 1.0 - _ViewParams.TSLight.z;	// 1-cos(ThetaL) = 1-cos(ThetaV)
	float	Cos5 = a * a;
			Cos5 *= Cos5 * a;
	float2	Diffuse = 1.0 + (Fd90-1.0)*Cos5;
			Diffuse *= Diffuse;						// Diffuse uses double Fresnel from both ThetaV and ThetaL

	float2	RetroDiffuse = max( 0.0, Diffuse-1.0 );	// Retro-reflection starts above 1
			Diffuse = min( 1.0, Diffuse );			// Clamp diffuse to avoid double-counting retro-reflection...

	Result.Diffuse = INVPI * dot( _MatParams.Diffuse.xz, Diffuse );
	Result.RetroDiffuse = INVPI * dot( _MatParams.Diffuse.xz, RetroDiffuse );

	// =================== COMPUTE SPECULAR ===================
	float4	Cxy = _MatParams.Offset.xxyy + exp( _MatParams.Amplitude + _MatParams.Falloff * pow( _ViewParams.UV.xyxy, _MatParams.Exponent ) );

	float2	Specular = Cxy.xz * Cxy.yw - _MatParams.Offset*_MatParams.Offset;	// Specular & Fresnel lovingly modulating each other
	Result.Specular = Specular.x + Specular.y;

	return Result;
}

#endif	// _LAYERED_MATERIALS_H_
