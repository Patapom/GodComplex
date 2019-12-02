//////////////////////////////////////////////////////////////////////////
// This shader computes then combines DOF & rendered scene together
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"

//[
cbuffer	cbObject	: register( b10 )
{
	float3		_dUV;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float2	UV			: UV0;
};

PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = _In.__Position;
	Out.UV = 0.5 * float2( 1.0 + _In.__Position.x, 1.0 - _In.__Position.y );

	return Out;
}


Texture2D<float4>	_TexSourceImage : register( t10 );


Texture2D<float4>	_TexDEBUG0 : register( t64 );
Texture2D<float4>	_TexDEBUG1 : register( t65 );


//////////////////////////////////////////////////////////////////////////
//
//
#define E_LINEAR 	(0) //round diaphragm (average size)
#define E_POISSON	(1) //round diaphragm (with Poisson sampling - not good bokeh)
#define E_DISK		(2) //round diaphragm (with disk sampling)
#define E_DOT		(3) //round diaphragm (small size)
#define E_HEXAGON	(4) //hexagon diaphragm (heavy gpu - for cinematic only)
#define E_PENTAGON	(5) //pentagon diaphragm (not supported)
#define E_CIRCLE	(6) //catadioptric lens (average size)

#define	FILTER_TYPE	E_LINEAR
#define NB_SAMPLES 21
static const float2	Kernel[NB_SAMPLES] =
{
	float2(-1,-2),	float2(-2,-1),	float2(-1,-1),
	float2(-2, 0),	float2(-1, 0),	float2(-2, 1),
	float2(-1, 1),	float2(-1, 2),	float2( 1, 1),
	float2( 2, 1),	float2( 1, 2),	float2( 1,-2),
	float2( 1,-1),	float2( 2,-1),	float2( 1, 0),
	float2( 2, 0),	float2( 0,-1),	float2( 0,-2),
	float2( 0, 2),	float2( 0, 1),	float2( 0, 0)		
};		

float2	GetCOCRadius( float _Depth )
{
	const float scale = $(env/dof/settings/bokeh_size);
			
#if FILTER_TYPE == E_LINEAR
	//x = circle of confusion (default value = 0.025mm)
	//y = focal distance (in millimeters)
	//z = focal length (default value = 35mm)
	//w = aperture f-Stop (default value = f/4)
	//const float coc         = aLensParams.x;
	//const float focalDist   = aLensParams.y;
	//const float focalLength = aLensParams.z;
	//const float fStop       = aLensParams.w;
	//float size = bokehSize( _Depth * 1000, focalDist, focalLength, fStop);
	//return min(size * (1080/35),1.0);
	return _Depth.xx * scale.xx;
 
// #elif FILTER_TYPE == E_POISSON
// 	return float2( lerp( 0.0, 5.0, _Depth ), lerp( 1.0, 2.5, _Depth ) ) * scale.xx;
// #elif FILTER_TYPE == E_DISK
// 	return _Depth.xx * scale.xx;//float2(  lerp( 0.5 , 3.0, _Depth ), lerp( 0.0, 2.5, _Depth ) );
// #elif FILTER_TYPE == E_DOT
// 	return _Depth.xx * scale.xx;
// #elif FILTER_TYPE == E_HEXAGON
// 	return _Depth.xx * scale.xx;
// #elif FILTER_TYPE == E_PENTAGON
// 	return _Depth.xx * scale.xx;			
// #elif FILTER_TYPE == E_CIRCLE
// 	return _Depth.xx * scale.xx;
// #else
// 	return float2(0.0,0.0);			
#endif
}

float2	GetOffsets( int idx, float radius, float nbSteps )
{
	const float anglestep = TWOPI / nbSteps;
	const float angle = (idx * anglestep);									
	float2 offsets = rot2d( float2(radius,0.0), angle );
	return -offsets;					
}
						
float2 GetOffsets(int idx)
{
//#if FILTER_TYPE == E_LINEAR
	return Kernel[idx];
// #elif FILTER_TYPE == E_POISSON
// 	return Kernel[idx];
// #elif FILTER_TYPE == E_DISK
// 	if(idx >= 12)				
// 		return GetOffsets(idx-12, 3.0, 9);
// 	else if(idx >= 5)
// 		return GetOffsets(idx-5, 2.0, 7);
// 	else if(idx >= 1)
// 		return GetOffsets(idx-1, 1.0, 4);
// 	else
// 		return float2(0.0,0.0);
// #elif FILTER_TYPE == E_DOT
// 	return Kernel[idx];	
// #elif FILTER_TYPE == E_HEXAGON
// 	return Kernel[idx];	
// #elif FILTER_TYPE == E_PENTAGON
// 	/*
// 	const float POLYGON_NUM = 5.0;
// 	const float basedAngle = 360.0 / POLYGON_NUM;
// 				
// 	int sampleCycle=0;
// 	int sampleCycleCounter=0;
// 	int sampleCounterInCycle=0;
// 	float2 currentVertex = 0;
// 	float2 nextVertex = 0;				
// 		//------------------------------------
// 
// 	{
// 	if(sampleCounterInCycle % sampleCycle == 0) 
// 		sampleCounterInCycle=0;
// 		sampleCycleCounter++;
// 		
// 		sampleCycle+=POLYGON_NUM;
// 					
// 		currentVertex.xy=float2(1.0 , 0.0);
// 		sincos( DegreesToRadians(basedAngle), nextVertex.y, nextVertex.x);	
// 	}
// 				
// 		
// 	sampleCounterInCycle++;
// 		
// 	float sampleAngle=basedAngle / float(sampleCycleCounter) * sampleCounterInCycle;
// 	float remainAngle=frac(sampleAngle / basedAngle) * basedAngle;
// 		
// 		
// 	if(remainAngle == 0.0)
// 	{
// 		currentVertex=nextVertex;
// 		sincos( DegreesToRadians(sampleAngle +  basedAngle), nextVertex.y, nextVertex.x);
// 	}
// 
// 	float2 sampleOffset=lerp(currentVertex.xy, nextVertex.xy, remainAngle / basedAngle);
// 	sampleOffset*=sampleCycleCounter / float(NB_SAMPLES);
// 				
// 	return sampleOffset;
// 	*/
// 				
// 	return float2(0.0,0.0);
// 				
// #elif FILTER_TYPE == E_CIRCLE
// 	return GetOffsets(idx, 3.0, NB_SAMPLES);
// #else
// 	return float2(0.0,0.0);
// #endif
}

//	_Fringe, bokeh chromatic aberration/fringing
//	_Threshold, highlight threshold
//	_Gain, highlight gain
//
float3	Fringing( Texture2D _Color, float2 _UV, float2 _Texel, float _Blur, float _Fringe, float _Threshold, float _Gain )
{
	float	R = _Color.SampleLevel( LinearClamp, _UV + float2(0.0,1.0)    *_Texel*_Fringe*_Blur, 0.0 ).r;
	float	G = _Color.SampleLevel( LinearClamp, _UV + float2(-0.866,-0.5)*_Texel*_Fringe*_Blur, 0.0 ).g;
	float	B = _Color.SampleLevel( LinearClamp, _UV + float2(0.866,-0.5) *_Texel*_Fringe*_Blur, 0.0 ).b;
	float3	Color = float3( R, G, B );
			
	float	Luminance = dot( normalize( Color ), LUMINANCE );
	float	Threshold = saturate( (Luminance-Threshold)*_Gain*_Blur );

	return lerp( Color, Color*Threshold, Threshold );
}

float3	ComputeDOFColor( Texture2D dofColorMap, float2 texCoord, float2 invTexSize, float2 weight )
{
	float3 sample00, sample10, sample01, sample11;
	sample00 = dofColorMap.SampleLevel( LinearBorderClamp, texCoord+invTexSize*float2(-0.5,-0.5), 0.0).rgb;
	sample10 = dofColorMap.SampleLevel( LinearBorderClamp, texCoord+invTexSize*float2(+0.5,-0.5), 0.0).rgb;
	sample01 = dofColorMap.SampleLevel( LinearBorderClamp, texCoord+invTexSize*float2(-0.5,+0.5), 0.0).rgb;
	sample11 = dofColorMap.SampleLevel( LinearBorderClamp, texCoord+invTexSize*float2(+0.5,+0.5), 0.0).rgb;

	sample00 = lerp(sample00,sample10, weight.x );
	sample01 = lerp(sample01,sample11, weight.x );
	return lerp(sample00, sample01, weight.y );
}

float	ComputeFuzziness( float depth, float dof, const float4 aDofRangeNear, const float4 aDofRangeFar )
{
	float nearFuzzy = aDofRangeNear.x;
	float nearSharp = aDofRangeNear.y;
	const float fuzzinessStrength = aDofRangeNear.w;
	float farFuzzy = aDofRangeFar.x;
	float farSharp = aDofRangeFar.y;
	const float fuzzinessBias = aDofRangeFar.z;
	float fDofFar = aDofRangeFar.w;

	float	linearDepth = depth * $projectionMatrixZ.w / min( -0.00001, depth + $projectionMatrixZ.z);
			linearDepth = LinearStep(0, fDofFar, linearDepth);
			
	// compute fuzziness Near - range [-1,1]
	//float k0 = ( linearDepth - nearSharp ) / (nearSharp - nearFuzzy);
	//float fuzzinessNear = (linearDepth >= nearSharp) ? 0.0 : clamp(k0,-1,0);
	float fuzzinessNear = dof;

	// compute fuzziness Far - range [-1,1]
	float k1 = ( linearDepth - farSharp ) / (farFuzzy - farSharp);
	float fuzzinessFar = (linearDepth > farSharp) * k1;

	float fuzziness = saturate( fuzzinessNear + fuzzinessFar + fuzzinessBias );			
	return fuzziness * fuzzinessStrength;
}


//////////////////////////////////////////////////////////////////////////
//
float4	PS_Near( PS_IN _In ) : SV_TARGET0
{
	float3	CenterColor = $(PostFX/DOF/diffuseMap).SampleLevel( LinearClamp, _In.UV, 0.0 ).xyz;
	float	CenterDepth = $(PostFX/DOF/dofDataMap).SampleLevel( LinearClamp, _In.UV, 0.0 ).x;

	const float2	COCRadius = GetCOCRadius( CenterDepth );

//	return Dof_Near( _In.UV, $(PostFX/DOF/targetSize).zw, CenterColor, CenterDepth );
//float4 Dof_Near ( float2 texcoord, float2 pixelSize, float3 centerColor, float CenterDepth )

	// Start with center sample
	float3	accumulator = 0.0;
	float	fuzzAccumulator = 1.0;
		
	// Sample points
	for ( int i=0; i < NB_SAMPLES; ++i )
	{
		//float2 UV = texcoord + pixelSize * GetOffsets(i) * COCRadius;
		float2	HACK_coords = float2(texcoord.x, 1.0 - texcoord.y) + pixelSize * GetOffsets(i) * COCRadius; //HACK_texCoords

		float3	Color = Fringing( $(PostFX/DOF/diffuseMap), HACK_coords, pixelSize, CenterDepth, $(PostFX/DOF/_FringeParams).x, $(PostFX/DOF/_FringeParams).y, $(PostFX/DOF/_FringeParams).z );

		float	contribution = CenterDepth;
		accumulator += Color * contribution;
		fuzzAccumulator += contribution;
	}

	// normalize kernel result
	accumulator /= fuzzAccumulator;
	Output = accumulator;

	float alpha = saturate( fuzzAccumulator / NB_SAMPLES );
	return float4( Output, alpha );
}

float4	PS_Far( PS_IN _In ) : SV_TARGET0
{
	float3	CenterColor = $(PostFX/DOF/diffuseMap).SampleLevel( LinearClamp, _In.UV, 0.0 ).xyz;
	float	CenterDepth = $(PostFX/DOF/dofDataMap).SampleLevel( LinearClamp, _In.UV, 0.0 ).y;

	const float2	COCRadius = GetCOCRadius( CenterDepth );

//	return Dof_Far( _In.UV, _In.UV, $(PostFX/DOF/targetSize), CenterColor, CenterDepth );					
//float4 Dof_Far(	float2 coords_high, float2 coords_low, float4 fTargetSize, float3 centerColor, float centerDepth )

	const float2 pixelSizeHigh = fTargetSize.xy;
	const float2 pixelSizeLow  = fTargetSize.zw;

	const float discRadiusHigh = cocRadius.x;
	const float discRadiusLow  = cocRadius.y;

	// Start with center sample
	float3	accumulator =  0;
	float	totalContribution = 0.0;

//`if($(PostFX/DOF/USE_CATEYE))
//	float cateye = vignette(coords_high, $(PostFX/DOF/center).xy, $(PostFX/DOF/vignParams));
//			
//	float2 uv = coords_high * 2.0 - 1.0;
//	float angle = atan2(uv.y, uv.x);
//	float cosAngle;
//	float sinAngle;
//	sincos(angle,sinAngle,cosAngle);								
//	float2x2 rotMat = { cosAngle, -sinAngle, sinAngle, cosAngle };
//`endif

	// Sample points
	for ( int i=0; i < NB_SAMPLES; ++i )
	{
//`if($(PostFX/DOF/USE_CATEYE))
//		float catEyeRadius = (i < 8) ? cateye : 1.0;
//		float2x2 scaleMat = { catEyeRadius, 0.0, 0.0, 1.0 };	
//		float2x2 scaleRotMat = mul( rotMat, scaleMat );
//		float2	coordHigh = coords_high + pixelSizeHigh * discRadiusHigh * GetOffsets(i);
//		float2	coordLow  = coords_low + pixelSizeLow  * discRadiusLow  * mul( scaleRotMat, GetOffsets(i) ) ;
//`else
		float2	coordHigh = coords_high + pixelSizeHigh * discRadiusHigh * GetOffsets(i);
		float2	coordLow  = coords_low + pixelSizeLow  * discRadiusLow * GetOffsets(i);
//`endif

		// Fetch blurriness
		float	BlurLow  = $(PostFX/DOF/dofBlurMap).SampleLevel( LinearClamp, coordLow, 0.0 ).g; //HACK_texCoords
		float	BlurHigh = $(PostFX/DOF/dofDataMap).SampleLevel( LinearClamp, float2(coordHigh.x, 1.0 - coordHigh[i].y), 0.0 ).g; //HACK_texCoords

		// Fetch color
		float3	tapHigh = $(PostFX/DOF/diffuseMap).SampleLevel( LinearClamp, coordHigh, 0.0).rgb;
		float3	tapLow  = Fringing( $(PostFX/DOF/blurredMap), float2( coordLow.x, 1.0 - coordLow.y ), pixelSizeLow, blurLow, $(PostFX/DOF/fringParams) ); //HACK_texCoords

		//Compute blur coef and leaking				
		bool	tapBlur = blurLow > centerDepth;
		float	blur = blurLow;
		//float leak = pow(saturate(blurLow * blurHigh), 8.0/centerDepth );	//average anti-leaking, too fast blur					
		//float leak = blurLow * saturate(blurHigh-centerDepth);			//strong anti-leaking, bad artifact on far fuzziness			
		//float leak = centerDepth;											//no anti-leaking
		float	leak = lerp( blurHigh, blurLow, centerDepth );

		//Get Coef and Color
		float coef = tapBlur ? 1.0 : leak;
				
#if 0//$(PostFX/DOF/USE_CATEYE)
		//weight is not homogeneous for cat-eyes
		coef *= (i < 8) ? 1.0/8.0 : 1.0;
#endif

		blur = lerp( blurHigh, leak, blurHigh );
		float3 tap = lerp( tapHigh, tapLow, blur );
					
		//Accumulate			
		accumulator += tap * coef;
		totalContribution += coef;
	}

	accumulator = totalContribution <= 1 ? centerColor : accumulator / totalContribution;

	// output
	return float4( accumulator, 1.0 );
}

//////////////////////////////////////////////////////////////////////////
//
float4	PS_Combine( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;

	//gather renderParms
	const float4 aDofRangeNear = float4($(PostFX/DOF/NearParams).x,		//near fuzzy
										$(PostFX/DOF/NearParams).y,		//far fuzzy
										0.0,							//motion strength
										1.0 );							//fuzziness strength
	const float4 aDofRangeFar = float4(	$(PostFX/DOF/FarParams).x,		//far fuzzy
										$(PostFX/DOF/FarParams).y,		//far sharp
										$(PostFX/DOF/FarParams).z,		//fuzziness bias
										$(POSTFX/DOF/CommonParams).x);	//far DOF

	const float2 invTexSize = $(PostFX/DOF/targetSize).xy;
	const float2 texSize = 1.0/invTexSize;
	const float2 weight = frac(texCoords * texSize - float2(0.5,0.5));
	const float2 center = $(PostFX/DOF/center).xy;

	float3	texColor = $(PostFX/DOF/frameBufferMap).SampleLevel( LinearClamp, texCoords, 0.0 ).rgb;

	// Compute Dof Mask
	float	dofNear = $(PostFX/DOF/dofDataMap).SampleLevel( LinearClamp, texCoords, 0.0 ).x;
	float	depth = $(PostFX/DOF/depthMap).SampleLevel( LinearClamp, texCoords, 0.0 ).x;
	float	dofMaskSharp = ComputeFuzziness( depth, dofNear, aDofRangeNear, aDofRangeFar );
	float	motionMask = 0.0;
	float3	dofColor = ComputeDOFColor( $(PostFX/DOF/dofMap), texCoords, invTexSize, weight );

	const float threshold = 0.05; //this value is captain-age driven
	if ( dofMaskSharp+motionMask > threshold )
	{
		// Blend Motion Blur			
		texColor = lerp( texColor, dofColor.xyz, motionMask );		
			
		// Blend Depth of field			
		//- linear DOF
		//texColor = lerp( texColor, dofColor.xyz, dofMaskSharp );
			
		//- non-linear mask (due to low-res DOF resolution)
		dofMaskSharp = smoothstep( 0.0, 0.25, dofMaskSharp );			
		texColor = lerp( texColor, dofColor.xyz, dofMaskSharp );
	}

	return texColor;

 	return _TexDEBUG0.SampleLevel( LinearClamp, UV, 0.0 );
 	return _TexSourceImage.SampleLevel( LinearClamp, UV, 0.0 );
	return float4( _In.__Position.xy * _dUV.xy, 0, 0 );
}
