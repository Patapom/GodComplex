#include "Includes/global.hlsl"
//#include "Includes/ScreenSpaceRayTracing.hlsl"

Texture2DArray<float4>	_TexSource : register(t0);
Texture2D<float>		_TexLinearDepth : register(t1);
Texture2D<float4>		_TexDownsampledDepth : register(t2);

cbuffer CB_PostProcess : register(b2) {
	float2	_UVFactor;
};


/////////////////////////////////////////////////////////////////////////////////////////////////////
// This routine performs a screen-space ray-tracing into a downsampled depth buffer
// It's adaptative as it will change the mip into which it's tracing the ray as long as no potential
//	intersection is found...
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
//static const uint	SSR_MIP_MIN = 1;	// Min mip level is full-resolution
static const uint	SSR_MIP_MAX = 4;	// Max mip level is 1/16th resolution


// bmayaux (2016-05-07) Thickness is now a [1,2] factor varying with scene depth
// The idea is that near objects have a very small thickness and far objects have a larger thickness
float ComputeSceneZThickness( float _sceneZMin, float4 _zThicknessFactors, float _zThicknessPow ) {
	float	t = saturate( (_sceneZMin - _zThicknessFactors.y) / (_zThicknessFactors.w - _zThicknessFactors.y) );
			t = pow( t, _zThicknessPow );
	return lerp( _zThicknessFactors.x, _zThicknessFactors.z, t ) * _sceneZMin;
}

// Samples the scene's depth buffer at appropriate mip level and returns the [ZMin,ZMax] interval
float2	SampleSceneZ( uint2 _pixelPosition, uint _mipLevel, Texture2D<float> _TexFullResDepth, Texture2D<float4> _TexDownsampledDepth, float4 _zThicknessFactors, float _zThicknessPow ) {
	float2	ZMinMax = _mipLevel == 0 ?	_TexFullResDepth[_pixelPosition] :							// Read full resolution Z
										_TexDownsampledDepth.mips[_mipLevel-1U][_pixelPosition].yz;	// Assuming we're receiving Y=Min Z, Z=Max Z


//float2	ZMinMax = _TexDownsampledDepth.mips[3U][_pixelPosition].yz;	// Assuming we're receiving Y=Min Z, Z=Max Z


			ZMinMax.y = ComputeSceneZThickness( ZMinMax.y, _zThicknessFactors, _zThicknessPow );	// Grow the interval with artificial scene thickness

	return ZMinMax;
}

// Performs screen-space ray-tracing using a downsampled depth buffer
//	_csPosition, the initial camera-space ray position
//	_csDirection, the camera-space ray direction
//	_RayLength, the maximum ray length to trace
//	_TexFullResDepth, the full resolution depth buffer
//	_TexDownsampledDepth, the downsampled depth buffer
//	_TexSize, the size of lowest mip of the _TexDownsampledDepth
//	_Camera2Proj, the CAMERA=>PROJECTION transform
//	_MaxStepsCount, the maximum allowed amount of steps
//	_HitDistance, the distance of the hit, in camera space units
//	_UV, the normalized UV coordinates where the hit occurred
// Returns a blend factor in [0,1] to blend the reflection with default sky reflection
//
float	ScreenSpaceRayTrace( float3 _csPosition, float3 _csDirection, float _RayLength, Texture2D<float> _TexFullResDepth, Texture2D<float4> _TexDownsampledDepth, uint2 _TexSize, float4x4 _Camera2Proj, uint _MaxStepsCount, out float _HitDistance, out float2 _UV, out float3 _DEBUG ) {

	_HitDistance = 0.0;
	_UV = 0.0;
	_DEBUG = 0.0;


const float4	ZThicknessFactors = float4( 1.02, 1.0, 1.1, 10.0 );	// Apply factor 1.02 at 10 meters, and 1.3 at 100 meters
const float		ZThicknessPow = 1.0;

	float3	csEndPos = _csPosition + _RayLength * _csDirection;

	const float	FADE_START_DISTANCE = 0.5;// * _RayLength;
	float	Fade = smoothstep( 0.0, FADE_START_DISTANCE, csEndPos.z );	// Fade based on end point's Z
//	float	Fade = smoothstep( 0.0, 0.1, _csDirection.z );				// Fade based on view direction is too drastic
	if ( Fade == 0.0 )
		return 0.0;	// Don't trace rays that come our way

	float4	H0 = mul( float4( _csPosition, 1.0 ), _Camera2Proj );
	float4	H1 = mul( float4( csEndPos, 1.0 ), _Camera2Proj );

	float	k0 = 1.0 / H0.w;	// k0 = 1/Zstart
	float	k1 = 1.0 / H1.w;	// k1 = 1/Zend

	H0.xyz *= k0;
	H1.xyz *= k1;

	H0.xy = float2( 0.5 * (1.0 + H0.x), 0.5 * (1.0 - H0.y) );
	H1.xy = float2( 0.5 * (1.0 + H1.x), 0.5 * (1.0 - H1.y) );

//H0.xy = saturate( H0.xy );
//H1.xy = saturate( H1.xy );

	H0.xy *= _TexSize;	// Now in full-resolution pixels
	H1.xy *= _TexSize;

	float2	Delta = H1.xy - H0.xy;
	float	PixelsCount = max( abs( Delta.x ), abs( Delta.y ) );

	float3	P0 = float3( H0.xy, 1.0 / _csPosition.z );	// We compose our screen-interpolable "P" as { XY=Pixel index, Z=1/Z }
	float3	P1 = float3( H1.xy, 1.0 / csEndPos.z );

	float3	Slope = (P1 - P0) / PixelsCount;			// Slope, with at least one of the 2 XY components equal to +1 or -1 (so adding the slope makes us advance an entire pixel)

	const float	BORDER_EPSILON = 1e-3;														// Always add a little epsilon so we always fully cross the border
	float2	BorderOffset = float2(	Slope.x > 0.0 ? 1.0+BORDER_EPSILON : -BORDER_EPSILON,	// If we're tracing to the right, then the next pixel border is on the right, otherwise it's on the left
									Slope.y > 0.0 ? 1.0+BORDER_EPSILON : -BORDER_EPSILON );	// If we're tracing to the bottom, then the next pixel border is below, otherwise it's above


// _DEBUG = float3( 1.0*abs(Slope.xy), 0 );
// _DEBUG = 0.5*abs(Slope.y);
// _DEBUG = 1.0*abs(Slope.x);
//_DEBUG = float3( BorderOffset, 0 );
//_DEBUG = float3( -0.001 * Delta.xy, 0 );
//_DEBUG = float3( H0.xy / _TexSize, 0 );
//_DEBUG = float3( H1.xy / _TexSize, 0 );
// _DEBUG = 0.01 / P1.z;
// _DEBUG = P0.y / _TexSize.y;
// return Fade;


	float2	InvSlope = abs( Slope.xy );
			InvSlope = sign( Slope.xy ) / max( 1e-6, InvSlope );

//_DEBUG = float3( -0.5 * InvSlope, 0 );
//return Fade;

//	uint	MipLevel = SSR_MIP_MAX;				// Start at coarsest mip
uint	MipLevel = 4U;
	float	PixelSize = 1U << MipLevel;			// That's the size of a pixel at this mip
//	float	ScaleFactor = 1.0 / PixelSize;		// That's the scale factor to apply to a mip 0 pixel to obtain its position in the current mip's pixel

	// Initial scale of pixel and slope
//	P0.xy *= ScaleFactor;
	P0.xy /= PixelSize;
	Slope.z *= PixelSize;

	uint	StepsCount = min( ceil( PixelsCount ), _MaxStepsCount );
	uint	StepIndex = 0U;
	bool	InInterval = false;

	// Sample at current position & current mip
	uint2	PixelPos = uint2( floor( P0.xy ) );
	float2	ZMinMax = SampleSceneZ( PixelPos, MipLevel, _TexFullResDepth, _TexDownsampledDepth, ZThicknessFactors, ZThicknessPow );
	float	Z = _csPosition.z;

	// Main loop
	[loop]
	while ( StepIndex < StepsCount ) {

		// Compute next position
		uint2	PixelPos = uint2( floor( P0.xy ) );
		float2	PixelBorder = PixelPos + BorderOffset;
		float2	Distance2Border = PixelBorder - P0.xy;
		float2	T = Distance2Border * InvSlope;	// "Time" to intersect, on both X and Y, to reach either next left/right pixel, or top/bottom pixel
		float	t = min( T.x, T.y );			// Choose the closest intersection (i.e. horizontal or vertical border)
		float3	NextP = P0 + t * Slope;
		float	NextZ = 1.0 / NextP.z;

		// Sample Zs at new mip and position
		uint2	NextPixelPos = uint2( floor( NextP.xy ) );
		float2	NextZMinMax = SampleSceneZ( NextPixelPos, MipLevel, _TexFullResDepth, _TexDownsampledDepth, ZThicknessFactors, ZThicknessPow );

		StepIndex++;			// Count actual texture taps
//		StepIndex += PixelSize;	// Or count actual pixels on screen

		// Check if we're in the thickness interval
InInterval = false;
#if 1
Ici ça résoud la barbe, mais on rate des intersections
InInterval = InInterval || (NextZ >= NextZMinMax.x && NextZ <= NextZMinMax.y);	// Either we're currently in the interval
//InInterval = InInterval || (Z < ZMinMax.x && NextZ > ZMinMax.x);					// Or we passed through the front
InInterval = InInterval || (Z < NextZMinMax.y && NextZ > NextZMinMax.y);			// Or we passed through the back
#else
Ici on a les intersections mais c'est la barbe!
InInterval = InInterval || (NextZ >= NextZMinMax.x && NextZ <= NextZMinMax.y);	// Either we're currently in the interval
InInterval = InInterval || (Z < ZMinMax.x && NextZ > ZMinMax.x);					// Or we passed through the front
InInterval = InInterval || (Z < NextZMinMax.y && NextZ > NextZMinMax.y);			// Or we passed through the back
#endif

		[branch]
		if ( InInterval ) {
			if ( MipLevel == 0U )
			{
				break;	// We have an intersection at finest mip!
			}

//*			// Refine mip to check if we're still intersecting at finer resolution (smaller pixels, slower steps)
			MipLevel--;
// 			PixelSize *= 0.5;	// Smaller pixels
// 			ScaleFactor *= 2.0;	// Larger scale
			P0.xy *= 2.0;		// Smaller pixels
			Slope.z *= 0.5;		// Smaller steps

			// We can't march yet as we don't know if we're intersecting at finer mip!
			continue;
//*/
		}

		// No intersection!
		// It means we can safely march forward a full pixel!
		P0 = NextP;
		Z = NextZ;
		ZMinMax = NextZMinMax;

/*		// Attempt to trace at coarser resolution (larger pixels, faster steps)
		if ( MipLevel < SSR_MIP_MAX ) {
			MipLevel++;
// 			PixelSize *= 2.0;	// Larger pixels
// 			ScaleFactor *= 0.5;	// Smaller scale
			P0.xy *= 0.5;		// Larger pixels
			Slope.z *= 2.0;		// Larger steps
		}
//*/
	}

	// Recompute hit distance and UVs
//	PixelSize = 1U << MipLevel;
	_UV = P0.xy / _TexSize;

// 	// Check full res depth
// 	ZMinMax = _TexFullResDepth.SampleLevel( LinearClamp, _UV, 0.0 );
// 	ZMinMax.y = ComputeSceneZThickness( ZMinMax.y, ZThicknessFactors, ZThicknessPow );	// Grow the interval with artificial scene thickness
// 	InInterval = Z >= ZMinMax.x && Z <= ZMinMax.y;

//_DEBUG = MipLevel;
//_DEBUG = 0.5 * PixelSize;
_DEBUG = float3( _UV, 0 );
_DEBUG = StepIndex / 128.0;
//InInterval = 1.0;

	return InInterval ? Fade : 0.0;
}




float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;

	float4	Color = _TexSource[uint3( _In.__Position.xy, 0)];
	float	sceneDistance = Color.w;
	float4	Depth = _TexSource[uint3( _In.__Position.xy, 1)];
//Depth = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 1.0 );


	// Build reflection on a plane
	const float	TAN_HALF_FOV_Y = 0.57735026918962576450914878050196;	// Assuming a 60° FOV
	const float	ASPECT_RATIO = iResolution.x / iResolution.y;
	const float	TAN_HALF_FOV_X = ASPECT_RATIO * TAN_HALF_FOV_Y;

	float3	csDir = normalize( float3( TAN_HALF_FOV_X * (2.0 * UV.x - 1.0), TAN_HALF_FOV_Y * (1.0 - 2.0 * UV.y), 1.0 ) );
	float3	wsDir = mul( float4( csDir, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	const float	PLANE_HEIGHT = 0.1;
	float	hitDistance = -(wsPos.y - PLANE_HEIGHT) / wsDir.y;
	if ( hitDistance > 0.0 && hitDistance < sceneDistance ) {
		float3	wsHit = wsPos + hitDistance * wsDir;
		float3	wsNormal = float3( 0, 1, 0 );	// Let's make it wavy later...
		float3	wsReflect = reflect( wsDir, wsNormal );

		const uint	MAX_STEPS = 256;

		float3	csHit = mul( float4( wsHit, 1.0 ), _World2Camera ).xyz;
		float3	csReflect = mul( float4( wsReflect, 0.0 ), _World2Camera ).xyz;

		uint2	Dims;
//		_TexDownsampledDepth.GetDimensions( Dims.x, Dims.y );
//		Dims = uint2( _UVFactor * Dims );	// So we account for padding
		_TexLinearDepth.GetDimensions( Dims.x, Dims.y );

//uint prout;
//_TexSource.GetDimensions( Dims.x, Dims.y, prout );
//Dims >>= 1U;

		// Compute screen space reflection and blend with background color
		// Actually, at the moment we simply replace the background color but we should use Fresnel equations to blend correctly...
		float2	hitUV;
		float	hitDistance;
		float3	DEBUG;
		float	blend = ScreenSpaceRayTrace( csHit, csReflect, 100.0, _TexLinearDepth, _TexDownsampledDepth, Dims, _Camera2Proj, MAX_STEPS, hitDistance, hitUV, DEBUG );
		float3	SKY_COLOR = float3( 1, 0, 0 );
		Color.xyz = lerp( SKY_COLOR, _TexSource.SampleLevel( LinearClamp, float3( hitUV, 0.0 ), 0.0 ).xyz, blend );

//return blend;
//return csReflect;
return lerp( float3( 0.8, 0, 0.8 ), DEBUG, blend );
	}


	Color.xyz = pow( max( 0.0, Color.xyz ), 1.0 / 2.2 );	// Gamma-correct

//return 0.1 * hitDistance;
//return 0.1 * sceneDistance;


//return 0.1 * _TexSource.SampleLevel( LinearClamp, float3( UV, 0.0 ), 0.0 ).w;	// Show distance
//return 0.1 * _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).w;	// Show projZ

//float	projZ = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).w;
//float	temp = _Proj2Camera[2].w * projZ + _Proj2Camera[3].w;
//return 0.1 * projZ / temp;	// Show linear Z

//return 0.1 * _TexLinearDepth.SampleLevel( LinearClamp, UV, 0.0 );
//return 0.1 * _TexDownsampledDepth.SampleLevel( LinearClamp, UV, 1.0 ).xyz;
//return 0.1 * _TexDownsampledDepth.SampleLevel( LinearClamp, UV, 1.0 ).xyz;



//uint	mipLevel = 4;
//Color = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 0)].xyz;
//Depth = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 1)];

//return 0.1 * Depth.x;


/*
uint	mipLevel = 2;
uint2	pixelPos = _In.__Position.xy;
		pixelPos >>= mipLevel;
float3	V = mipLevel > 0 ? _TexDownsampledDepth.mips[mipLevel-1][pixelPos].xyz : _TexLinearDepth.SampleLevel( LinearClamp, UV, 0.0 );
//return 1.0 * (V.z - V.y);
return 0.1 * V;
*/
	return Color.xyz;
}
