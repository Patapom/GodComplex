//////////////////////////////////////////////////////////////////////////
// This shader computes pixels' fuzziness for near & far fields
//
#include "Inc/Global.hlsl"


cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
	float4		_Offsets[4];
	float4		_Weights[4];
};

Texture2D<float4>	_TexSourceImage : register( t10 );

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float4	UV0			: UV0;
	float4	UV1			: UV1;
	float4	UV2			: UV2;
	float4	UV3			: UV3;
};


VS_IN	VS( VS_IN _In )	{ return _In; }

float2	PS_Near( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position * _dUV.xy;

	float	FarDOF = $(POSTFX/DOF/CommonParams).x;
	float	NearFuzzy = $(POSTFX/DOF/NearParams).x;
	float	NearSharp = $(POSTFX/DOF/NearParams).y;
		
// 	//------------------------------------------------------------------------
// 	// Compute Alpha Fuzziness
// 	//------------------------------------------------------------------------
// 	float FarSharp   = $(PostFX/DOF/AlphaParams).x;
// 	float FarFuzzy   = $(PostFX/DOF/AlphaParams).y;
// 	const float Bias = $(PostFX/DOF/AlphaParams).z;
// 			
// 	`if $(PostFX/DOF/AUTOFOCUS)
// 		FarSharp = (FarSharp + focusDist) / FarDOF;
// 		FarFuzzy = (FarFuzzy + focusDist) / FarDOF;
// 	`endif
// 			
// 	float DepthMax = $(PostFX/DOF/ZbufferDownsampled).SampleLevel( $pointClamp, UV, 4.0 ).y; //zmin	
// 	float DepthMaxLinear = LinearStep(0, FarDOF, DepthMax);			
// 			
// 	float depthAlpha = $(PostFX/DOF/ZbufferAlpha).SampleLevel( LinearClamp, $legacy_texcoord.zw, 0.0 ).x; //zmax
// 	depthAlpha = depthAlpha * $projectionMatrixZ.w / min( -0.00001, depthAlpha + $projectionMatrixZ.z);
// 	float depthAlphaLinear = LinearStep(0, FarDOF, depthAlpha);			
// 			
// 	const float epsilon = 0.001; //(epsilon is captain age driven)
// 	//float alpha_mask = abs(depthAlphaLinear/DepthMaxLinear) < 1-epsilon;
// 	float alpha_mask = abs(depthAlphaLinear-DepthMaxLinear) > epsilon;
// 	float alpha_k1 = ( DepthMaxLinear - FarSharp ) / (FarFuzzy - FarSharp);	
// 	float alpha_Fuzziness = saturate((DepthMaxLinear > FarSharp) * alpha_k1 + Bias);
// 	Result = float2( alpha_Fuzziness, alpha_mask );
// 			
	//------------------------------------------------------------------------
	// Fetch depth
	//------------------------------------------------------------------------
	//float DepthMin = $(PostFX/DOF/ZbufferDownsampled).SampleLevel( $pointClamp, UV, 0.0 ).y; //zmin
	//float DepthMinLinear = LinearStep(0,FarDOF,DepthMin); //clamp to FarDof
	float	DepthMin = $(PostFX/DOF/ZbufferAlpha).SampleLevel( LinearClamp, UV, 0.0 ).x; //zmax	- should be z-max with alpha
	float	DepthMinLinear = DepthMin * $projectionMatrixZ.w / min( -0.00001, DepthMin + $projectionMatrixZ.z);			
	DepthMinLinear = LinearStep( 0, FarDOF, DepthMinLinear );			
				
	//------------------------------------------------------------------------
	// Compute Near Fuzziness
	//------------------------------------------------------------------------			
	float	k0 = (DepthMinLinear - NearSharp) / (NearSharp - NearFuzzy);
	float	Fuzziness = (DepthMinLinear >= NearSharp) ? 0.0 : -k0;
	
	return float2( Fuzziness, 0.0 );
}

float2	PS_Far( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position * _dUV.xy;
	
	float	FarDOF = $(POSTFX/DOF/CommonParams).x;
	float	FarFuzzy = $(POSTFX/DOF/FarParams).x;
	float	FarSharp = $(POSTFX/DOF/FarParams).y;
	const float Bias = $(POSTFX/DOF/FarParams).z;

	//------------------------------------------------------------------------
	// Fetch depth
	//------------------------------------------------------------------------
	float	DepthMax = $(PostFX/DOF/ZbufferAlpha).SampleLevel( LinearClamp, UV, 0.0 ).x; //zmax - should be z-max with alpha
	float	DepthMaxLinear = DepthMax * $projectionMatrixZ.w / min( -0.00001, DepthMax + $projectionMatrixZ.z);			
			DepthMaxLinear = LinearStep( 0, FarDOF, DepthMaxLinear);
			
	float	DepthMin = $(PostFX/DOF/ZbufferDownsampled).SampleLevel( $pointClamp, HACK_texCoords, 0.0 ).y; //zmin
	float	DepthMinLinear = LinearStep( 0, FarDOF, DepthMin ); //clamp to FarDof	
			
	//------------------------------------------------------------------------
	// Compute Far Fuzziness
	//------------------------------------------------------------------------			
	float	FuzzyCoef = saturate( abs( DepthMaxLinear - DepthMinLinear ) / FarFuzzy ); //better if DepthMin have alpha depth
	float	NearFuzziness = _TexSourceImage.SampleLevel( LinearClamp, UV, 0.0 ).x;
	float	k1 = (DepthMaxLinear - FarSharp) / (FarFuzzy - FarSharp);
	float	FarFuzziness = ((DepthMaxLinear > FarSharp) * k1) + Bias;// + FuzzyCoef;
								
	return saturate( float2( NearFuzziness, FarFuzziness ) );
}

// Separable blur
PS_IN	VS_Blur( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = _In.__Position;

	float2	UV = 0.5 * float2( 1.0 + _In.__Position.x, 1.0 - _In.__Position.y );

	Out.UV0 = UV.xyxy + _Offsets[0];
	Out.UV1 = UV.xyxy + _Offsets[1];
	Out.UV2 = UV.xyxy + _Offsets[2];
	Out.UV3 = UV.xyxy + _Offsets[3];

	return Out;
}

float	PS_Blur( VS_IN _In ) : SV_TARGET0
{
	float Result;
	Result  = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.xy, 0.0 ).x * _Weights[0].x;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.zw, 0.0 ).x * _Weights[0].z;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.xy, 0.0 ).x * _Weights[1].x;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.zw, 0.0 ).x * _Weights[1].z;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV2.xy, 0.0 ).x * _Weights[2].x;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV2.zw, 0.0 ).x * _Weights[2].z;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV3.xy, 0.0 ).x * _Weights[3].x;
	Result += _TexSourceImage.SampleLevel( LinearClamp, _In.UV3.zw, 0.0 ).x * _Weights[3].z;

	return Result;
}
