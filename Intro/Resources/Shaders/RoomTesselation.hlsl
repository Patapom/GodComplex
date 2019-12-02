//////////////////////////////////////////////////////////////////////////
// This shader shows a tesselated quad using Hull+Domain shaders
// (Woooooot! My first HS/DS!!!)
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbRender	: register( b10 )
{
	float3	_dUV;
	float2	_TesselationFactors;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
	float2	UV			: TEXCOORD0;
};

// Output patch control point & constant data
struct HS_PATCH_OUT
{
	float	Edges[4]		: SV_TESSFACTOR;
	float	Inside[2]		: SV_INSIDETESSFACTOR;

// Additional per-patch infos
	float2	UV[4]			: TEXCOORD;
//	float3	vTangent[4]		: TANGENT;
//	float3 vTanUCorner[4]	: TANUCORNER;
//	float3 vTanVCorner[4]	: TANVCORNER;
//	float4 vCWts			: TANWEIGHTS;
};

struct	HS_OUT
{
	float3	Position	: POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float2	UV			: TEXCOORD0;
	float	Color		: COLOR;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

///////////////////////////////////////////////////////////////////
// Constant per-patch data generation
//	This must AT LEAST output the tesselation factor for edges and triangles
//
//[
HS_PATCH_OUT	PatchHS( InputPatch<VS_IN, 4> _In, uint _PatchID : SV_PRIMITIVEID )
{	
    HS_PATCH_OUT	Out;

	Out.Edges[0] = _TesselationFactors.x;
	Out.Edges[1] = _TesselationFactors.x;
	Out.Edges[2] = _TesselationFactors.x;
	Out.Edges[3] = _TesselationFactors.x;
    
	Out.Inside[0] = _TesselationFactors.y;
	Out.Inside[1] = _TesselationFactors.y;

	// Copy UVs
	Out.UV[0] = _In[0].UV;
	Out.UV[1] = _In[1].UV;
	Out.UV[2] = _In[2].UV;
	Out.UV[3] = _In[3].UV;

    return Out;
}
//]

///////////////////////////////////////////////////////////////////
// Per-control point Hull Shader
//
// Type of Partitioning | Range
// -----------------------------
// Fractional_odd		 [1..63]
// Fractional_even		 [2..64]
// Integer				 [1..64]
// Pow2					 [1..64]
//
//[
[domain( "quad" )]						// Either "tri", "quad" or "isoline"
[partitioning( "fractional_odd" )]	// Either "integer", "fractional_even", "fractional_odd", or "pow2"
//[partitioning( "fractional_even" )]
//[partitioning( "integer" )]
//[partitioning( "pow2" )]
[outputtopology( "triangle_ccw" )]		// Either "line", "triangle_cw", or "triangle_ccw"
[outputcontrolpoints( 4 )]
[patchconstantfunc( "PatchHS" )]
HS_OUT	HS( InputPatch<VS_IN, 4> _In, uint _ControlPointID : SV_OUTPUTCONTROLPOINTID, uint _PatchID : SV_PRIMITIVEID )
{
    HS_OUT	Out;
			Out.Position = _In[_ControlPointID].Position;	// Simply output the un-altered control point's position...

    return Out;
}
//]


///////////////////////////////////////////////////////////////////
// Called for each generated vertex
//
//[
[domain( "quad" )]
PS_IN	DS( HS_PATCH_OUT _PatchIn, const OutputPatch<HS_OUT, 4> _ControlPointsIn, float2 _UV : SV_DOMAINLOCATION )
{
	// Interpolate data within the patch
	float2	UV = BILERP( _PatchIn.UV[0], _PatchIn.UV[1], _PatchIn.UV[2], _PatchIn.UV[3], _UV );
	float3	Position = BILERP( _ControlPointsIn[0].Position, _ControlPointsIn[1].Position, _ControlPointsIn[2].Position, _ControlPointsIn[3].Position, _UV );

	float	Noise = _TexNoise3D.SampleLevel( LinearWrap, float3( UV, 0.0 ), 0.0 ).x;

	// Offset position with noise
	Position.y += 0.2 + Noise;

	PS_IN	Out;
			Out.__Position = mul( float4( Position, 1.0 ), _World2Proj );
			Out.UV = UV;
			Out.Color = 4.0 * (0.1+Noise);

	return Out;
}
//]

///////////////////////////////////////////////////////////////////
float4	PS( PS_IN _In ) : SV_TARGET0
{
//	return float4( _In.UV, 0, 1.0 );
	return _In.Color;
}
