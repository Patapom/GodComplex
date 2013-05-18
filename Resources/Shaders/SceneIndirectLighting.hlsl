//////////////////////////////////////////////////////////////////////////
// This shader applies indirect lighting and finalizes rendering
//
#include "Inc/Global.hlsl"
#include "Inc/LayeredMaterials.hlsl"

cbuffer	cbRender	: register( b10 )
{
	float3		_dUV;
	float3		_MainLightDirection;	// Main light direction
};

Texture2DArray		_TexGBuffer0 : register( t10 );	// 3 First render targets as RGBA16F
Texture2D<uint4>	_TexGBuffer1 : register( t11 );	// [Weight,MatID] target as RGBA16_UINT
Texture2D			_TexDepth : register( t12 );
Texture2D			_TexDepthFrontDownSampled : register( t13 );

Texture2DArray		_TexDiffuseSpecular	: register(t14);	// Diffuse + Specular in 2 slices

Texture2D			_TexEnvMap	: register(t15);	// The spherical projection env map with mips

Texture2DArray		_TexDEBUGMaterial	: register(t16);	// 4 Slices of diffuse+blend masks + normal map + specular map = 6 textures per primitive
Texture2D			_TexDEBUGBackGBuffer: register(t17);


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

WeightMatID	ReadWeightMatID( uint _Packed )
{
	WeightMatID	Out;
	Out.ID = _Packed >> 8;
	Out.Weight = (_Packed & 0xFF) / 255.0f;
	return Out;
}

float3	SampleEnvMap( float3 _Direction, float _MipLevel )
{
	float	EnvMapPhi = 0.0;
	float2	UV = float2( 0.5 * (1.0 + (atan2( _Direction.x, -_Direction.z ) + EnvMapPhi) * INVPI), acos( _Direction.y ) * INVPI );
	return _TexEnvMap.SampleLevel( LinearWrap, UV, _MipLevel ).xyz;
}

#if 0
bool	Intersect( float3 _Position, float3 _Direction, float3 _Normal, out float4 _Intersection, out float2 _UV, out float4 _Debug )
{
	const float	StepsCount = 16;
	const float	MaxRadius = 2.0;	// 2 world units max until we drop the computation

_Debug = 0.0;

	_Intersection = float4( _Position, 0.0 );
	float4	Step = (MaxRadius / StepsCount) * float4( _Direction, 1.0 );
//			Step = float4( Step.xy / (_Z * _CameraData.xy), Step.zw );

//Step /= saturate( 1.0 + Step.z );
//_Debug = saturate( 0.2 + Step.z );

	_Intersection += 0.05 * Step;

	float2	PreviousZ, CurrentZ = _Position.zz;
	float	StepIndex = 0;
	for ( ; StepIndex < StepsCount; StepIndex++ )
	{
//		_Intersection += Step;

		// Retrieve UVs from current camera position
		_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
		_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
// _Debug = float3( _UV, 0 );
// return true;

		// Sample Z buffer at this position
		PreviousZ = CurrentZ;
		CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;
//		CurrentZ = _TexDepthFrontDownSampled.SampleLevel( LinearClamp, _UV, 0.0 ).xy;

		if ( _Intersection.z > CurrentZ.x )
		{	// Frontface Hit! Interpolate to "exact intersection"...
			float	t = (_Intersection.z - CurrentZ.x) / (PreviousZ.x - CurrentZ.x + Step.z);

_Debug.x = _Intersection.z - CurrentZ.x;
_Debug.y = t;

			_Intersection -= t * Step;
//			StepIndex -= t;

			_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
			_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
			return true;
		}

// 		if ( CurrentZ.x > CurrentZ.y && CurrentZ.y > _Intersection.z )
// 		{	// Backface Hit! Interpolate to "exact intersection"...
// 			float	t = (_Intersection.z - CurrentZ.y) / (PreviousZ.y - CurrentZ.y + Step.z);
// 			_Intersection -= t * Step;
// //			StepIndex -= t;
// 			_Debug = 1;
// 
// 			_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
// 			_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
// 			return true;
// 		}

		_Intersection += Step;
	}

	return false;
}

#elif 0

bool	Intersect( float3 _Position, float3 _Direction, float3 _Normal, out float4 _Intersection, out float2 _UV, out float4 _Debug )
{
	const float	StepsCount = 8;
	const float	MaxRadius = 2.0;	// 2 world units max until we drop the computation

_Debug = 0.0;

	float4	Step = (MaxRadius / StepsCount) * float4( _Direction, 1.0 );

	_Intersection = float4( _Position, 0.0 );
	_Intersection += Step * 0.01 / saturate( dot( Step.xyz, _Normal ) );	// Offset a little from the surface

	float4	PreviousIntersection;
	float2	PreviousZ2, PreviousZ, CurrentZ = _Position.zz, PreviousUV;
	float	StepIndex = 0;
	for ( ; StepIndex < StepsCount; StepIndex++ )
	{
		// Retrieve UVs from current camera position
		PreviousUV = _UV;
		_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
		_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );

		// Sample Z buffer at this position
		PreviousZ2 = PreviousZ;
		PreviousZ = CurrentZ;
		CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;

		if ( _Intersection.z > CurrentZ.x )
			break;	// High velocity hit! Compute more precise intersection...

		PreviousIntersection = _Intersection;
		_Intersection += Step;
	}

	if ( StepIndex >= StepsCount )
		return false;
// 
// float	t2 = (_Intersection.z - CurrentZ.x) / (PreviousZ.x - CurrentZ.x + Step.z);
// 
// /* 2nd order
// float	a = PreviousZ.x;
// float	b = 0.5 * (CurrentZ.x - PreviousZ2.x);
// float	c = 0.5 * (CurrentZ.x + PreviousZ2.x - 2.0 * a);
// 
// a -= _Intersection.z;
// float	Delta = max( 0.0, b*b - 4*a*c );
// float	t2 = (-b - sqrt(Delta)) / (2*a);
// */
// 
// _Debug.x = _Intersection.z - CurrentZ.x;
// _Debug.y = t2;
// 
// _Intersection -= t2 * Step;
// _UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
// _UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );
// 
// return true;
// 



	// ========= We hit the ZBuffer at high velocity =========
	// We need to reduce speed to find actual intersection...
	float	t = (_Intersection.z - CurrentZ.x) / (PreviousZ.x - CurrentZ.x + Step.z);	// This is a projective estimate of the actual hit within the large step
			t = saturate( t * 1.5 );													// Increase the estimate to account for errors

t = 1.0;

	Step *= t / (StepsCount+1);	// The new step is the old one scaled by the intersection estimate and divided by the amount of steps

	// Go back to before the intersection...
	_Intersection = PreviousIntersection;
	_UV = PreviousUV;
//	CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;
	CurrentZ = PreviousZ;

	_Intersection += Step;

	for ( StepIndex=0; StepIndex < StepsCount; StepIndex++ )
	{
		// Retrieve UVs from current camera position
		_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
		_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );

		// Sample Z buffer at this position
		PreviousZ = CurrentZ;
		CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;

// 		if ( _Intersection.z > CurrentZ.y )
// 			return false;	// 

		if ( _Intersection.z > CurrentZ.x )
		{	// Actual Hit! Compute more precise intersection...
			t = (_Intersection.z - CurrentZ.x) / (PreviousZ.x - CurrentZ.x + Step.z);

_Debug.x = _Intersection.z - CurrentZ.x;
_Debug.y = t;

			_Intersection -= t * Step;
//			StepIndex -= t;
			break;
		}

		_Intersection += Step;
	}

	// Compute final UVs
	_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
	_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );

	return true;
}

#else

bool	Intersect( float3 _Position, float3 _Direction, float3 _Normal, out float4 _Intersection, out float2 _UV, out float4 _Debug )
{
	const float	StepsCount = 16;
	const float	MaxRadius = 3.0;	// 2 world units max until we drop the computation

_Debug = 0.0;

	float4	Step = (MaxRadius / StepsCount) * float4( _Direction, 1.0 );

	_Intersection = float4( _Position, 0.0 );
	_Intersection += Step * 0.01 / saturate( dot( Step.xyz, _Normal ) );	// Offset a little from the surface

	float	StepIndex = 0;
	for ( ; StepIndex < StepsCount; StepIndex++ )
	{
		// Retrieve UVs from current camera position
		_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
		_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );

		// Sample Z buffer at this position
		float2	CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;
		if ( _Intersection.z > CurrentZ.x )
			break;	// High velocity hit! Compute more precise intersection...

		_Intersection += Step;
	}

	if ( StepIndex >= StepsCount )
		return false;

	// ========= We hit the ZBuffer at high velocity =========
	// We'll find the intersection by dichotomy
	_Intersection -= Step;

	Step.xyz *= 0.5;	// Halve the step and reverse direction

	for ( StepIndex=0; StepIndex < 32; StepIndex++ )
	{
		_Intersection += Step;

		// Retrieve UVs from current camera position
		_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
		_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );

		// Sample Z buffer at this position
		float2	CurrentZ = _TexDepth.SampleLevel( LinearClamp, _UV, 0.0 ).xy;
		if ( _Intersection.z > CurrentZ.x )
			Step.xyz *= -0.5;	// We're too far in the ZBuffer, reverse direction
		else
			Step.xyz *= 0.5;	// We're still not far enough, march a little more...
	}

// 	// Compute final UVs
// 	_UV = _Intersection.xy / (_Intersection.z * _CameraData.xy);
// 	_UV = float2( 0.5 * (1.0 + _UV.x), 0.5 * (1.0 - _UV.y) );

	return true;
}

#endif



// Version with 2 layers
MatReflectance	TempLayeredMatEval( HalfVectorSpaceParams _ViewParams, DualMaterialParams _MatParams )
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


float3	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _dUV.xy * _In.__Position.xy;

	float4	Buf0 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	Buf1 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	float4	Buf2 = _TexGBuffer0.SampleLevel( LinearClamp, float3( UV, 2 ), 0.0 );
	uint4	Buf3 = _TexGBuffer1.Load( _In.__Position.xyz );

	// Prepare necessary informations
	float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;

//return 0.1 * _TexDepth.SampleLevel( LinearClamp, UV, 5.0 ).x;;

	float3	DiffuseAlbedo = Buf1.xyz;
	float3	SpecularAlbedo = Buf2.xyz;
	float	Height = Buf2.w;

	WeightMatID		Mats[4] = {
		ReadWeightMatID( Buf3.x ),
		ReadWeightMatID( Buf3.y ),
		ReadWeightMatID( Buf3.z ),
		ReadWeightMatID( Buf3.w ),
	};

	float3	CameraView = float3( (2.0 * UV.x - 1.0) * _CameraData.x, (1.0 - 2.0 * UV.y) * _CameraData.y, 1.0 );
	float3	CameraPosition = Z * CameraView;
			CameraView = normalize( CameraView );

//return 100.0 * abs( Mats[0].Weight - DiffuseAlbedo.x );
//return sqrt(DiffuseAlbedo);
//return Mats[0].Weight;
//return 0.2 * Mats[0].ID;

	// Recompose and unpack tangent
	float3	CameraTangent = 2.0 * float3( Buf0.zw, Buf1.w ) - 1.0;

	// Unpack stereographic normal (from http://aras-p.info/texts/CompactNormalStorage.html#method07stereo)
	// See also http://en.wikipedia.org/wiki/Stereographic_projection
 	Buf0.xy = (1.57 * 2.0) * (Buf0.xy - 0.5);
	float	NormalScale = 2.0 / (1.0 + dot( Buf0.xy, Buf0.xy ) );
	float3	CameraNormal = float3( NormalScale * Buf0.xy, 1.0-NormalScale );

//return float3( 1, 1, -1 ) * CameraNormal;


	float3	CameraBiTangent = normalize( cross( CameraTangent, CameraNormal ) );

	// Compute view reflection
	float3	CameraReflect = reflect( CameraView, CameraNormal );

	// Use camera view for empty pixels
	float	InfinityZ = saturate( 10000.0 * (Z - 50.0) );
	CameraReflect = lerp( CameraReflect, CameraView, InfinityZ );

//return float4( SampleEnvMap( CameraReflect.x * _Camera2World[0].xyz + CameraReflect.y * _Camera2World[1].xyz + CameraReflect.z * _Camera2World[2].xyz, 0.0 ), 1 );
//return float4( CameraView, 0 );
//return float4( float3( 1,1,-1 ) * CameraTangent, 0 );
//return float4( float3( 1,1,-1 ) * CameraBiTangent, 0 );
//return float4( float3( 1,1,-1 ) * CameraNormal, 0 );
//return float4( CameraReflect, 0 );


///////////////////////////////////////////////////////////////////////////////////
	float3	Reflection = 0.0;
#if 1
	// Proceed with intersection
	float4	SceneReflection = 0.0;
	float4	Intersection, DEBUG;
	float2	IntersectionUV;
	if ( InfinityZ == 0.0 && Intersect( CameraPosition, CameraReflect, CameraNormal, Intersection, IntersectionUV, DEBUG ) )
	{
//return DEBUG;
	 	SceneReflection.xyz = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( IntersectionUV, 0 ), 0.0 ).xyz;

//Reflection = 0.5 * DEBUG.y;
//Reflection = 0.5 * DEBUG.x;

		SceneReflection.w = saturate( 2.0 * (0.5 - DEBUG.x) );	// Blends with envmap

// Darkens reflection
// SceneReflection.xyz *= saturate( 2.0 * (0.5 - DEBUG.x) );
// SceneReflection.w = 0.0;


float3	PipoNormal = float3( _TexGBuffer0.SampleLevel( LinearClamp, float3( IntersectionUV, 0 ), 0.0 ).xy, 0.0 );
		PipoNormal.xy = (1.57 * 2.0) * (PipoNormal.xy - 0.5);
NormalScale = 2.0 / (1.0 + dot( PipoNormal.xy, PipoNormal.xy ) );
PipoNormal = float3( NormalScale * PipoNormal.xy, 1.0-NormalScale );
//return PipoNormal;


// 	 	Reflection = DEBUG.x == 0
// 			? _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( IntersectionUV, 0 ), 0.0 ).xyz
// 			: _TexDEBUGBackGBuffer.SampleLevel( LinearClamp, IntersectionUV, 0.0 ).xyz;
	}

	float3	EnvMapReflection = lerp( 0.2 * INVPI * SpecularAlbedo, 1.0, InfinityZ ) * SampleEnvMap( CameraReflect.x * _Camera2World[0].xyz + CameraReflect.y * _Camera2World[1].xyz + CameraReflect.z * _Camera2World[2].xyz, 0.0 );

	Reflection = lerp( EnvMapReflection, SceneReflection.xyz, SceneReflection.w );
//Reflection = SceneReflection.xyz;

//return saturate( 2.0 * (0.5 - DEBUG.x) ) * 10.0 * Reflection;
//return saturate( 2.0 * (0.5 - DEBUG.x) );
//return 10.0 * Reflection;

#endif


///////////////////////////////////////////////////////////////////////////////////
// This piece of code is interesting as it generates all the necessary rays for the current view direction
#if 0

	DualMaterialParams		MatParams = ComputeDualWeightedMaterialParams( Mats );
	HalfVectorSpaceParams	ViewParams = (HalfVectorSpaceParams) 0;

//return -0.04 * MatParams.Falloff.w;

	ViewParams.TSView = -float3( dot( CameraView, CameraTangent ), dot( CameraView, CameraBiTangent ), dot( CameraView, CameraNormal ) );
	float3	TSView_Plane = normalize( float3( ViewParams.TSView.xy, 1e-8 ) );
	float3	TSOrtho_Plane = float3( TSView_Plane.y, -TSView_Plane.x, TSView_Plane.z );

	float	CosThetaV = ViewParams.TSView.z;	// This is easy!
	float	SinThetaV = sqrt( 1.0 - CosThetaV*CosThetaV );
	float	ThetaV = acos( CosThetaV );

	float	StepsCountX = INVHALFPI * ThetaV;
			StepsCountX = 1.0 - abs( 2.0 * StepsCountX - 1.0 );	// I noticed the amount of steps was large for grazing angles which we don't care about!
//			StepsCountX = 1 + 2 * lerp( 0, 8, StepsCountX );
StepsCountX = 17;

	float	nStepsCountX = floor( StepsCountX );
//return 0.125 * HalfStepsCountX;

	float	StepsCountY = 1.0 - INVHALFPI * ThetaV;
			StepsCountY = lerp( 1, 16, StepsCountY );
StepsCountY = 16;

	float	nStepsCountY = floor( StepsCountY );

	float2	StepX = (ThetaV / nStepsCountX) * float2( 1, -1 );
	float2	StepY = (0.5 * (HALFPI - ThetaV) / nStepsCountY) * float2( 1, 1 );

//StepY *= 0.1;

//	float2	BaseThetaHD = 0.5 * ThetaV - 0.5 * (nStepsCountX-1) * StepX;
	float2	BaseThetaHD = 0.5 * (ThetaV.xx - (nStepsCountX-1) * StepX);
	for ( float StepIndexX=0; StepIndexX < nStepsCountX; StepIndexX++ )
	{
		ViewParams.ThetaHD = BaseThetaHD;

		for ( float StepIndexY=0; StepIndexY < nStepsCountY; StepIndexY++ )
		{
			ViewParams.UV = INVHALFPI * ViewParams.ThetaHD;

			// We know everything!
			float	SinThetaH;
			sincos( ViewParams.ThetaHD.x, SinThetaH, ViewParams.CosThetaH );
			ViewParams.CosThetaD = cos( ViewParams.ThetaHD.y );

			// Compute azimuth between View and Half vectors
			float	CosAlpha = clamp( (ViewParams.CosThetaD - CosThetaV*ViewParams.CosThetaH) / max( 1e-6, SinThetaV*SinThetaH ), -1.0, +1.0 );
			float	SinAlpha = sqrt( 1.0 - CosAlpha*CosAlpha );

			// Rebuild half vector
			float3	TSHalfDirection = CosAlpha * TSView_Plane + SinAlpha * TSOrtho_Plane;
			ViewParams.Half = SinThetaH * TSHalfDirection + ViewParams.CosThetaH * float3( 0, 0, 1 );

			// Finally, rebuild light vector by mirroring view through half vector
			ViewParams.TSLight = 2.0 * dot( ViewParams.TSView, ViewParams.Half ) * ViewParams.Half - ViewParams.TSView;


// float	Theta = acos( sqrt( StepIndexY / StepsCountY ) );
// float	Phi = TWOPI * StepIndexX / StepsCountX;
// ViewParams.TSLight = float3( sin(Theta)*cos(Phi), sin(Theta)*sin(Phi), cos(Theta) );
// 
// ViewParams.Half = normalize( ViewParams.TSLight + ViewParams.TSView );
// ViewParams.ThetaHD = float2( acos( ViewParams.Half.z ), acos( dot( ViewParams.Half, ViewParams.TSLight ) ) );
// ViewParams.UV = INVHALFPI * ViewParams.ThetaHD;


			// Sample environment in that direction
			float3	LightWorld = ViewParams.TSLight.x * _Camera2World[0].xyz + ViewParams.TSLight.y * _Camera2World[1].xyz + ViewParams.TSLight.z * _Camera2World[2].xyz;
			float3	EnvLight = SampleEnvMap( LightWorld, 0.0 );

			// Evaluate POM model
			MatReflectance	Reflectance = TempLayeredMatEval( ViewParams, MatParams );

			Reflection += EnvLight * (Reflectance.Diffuse + Reflectance.RetroDiffuse + Reflectance.Specular);

			ViewParams.ThetaHD += StepY;
		}

//return ViewParams.Half.z;
//return 0.9 * length( TSOrtho_Plane );
//return 0.9 * length( ViewParams.Half );
//return 1.0 * length( ViewParams.TSLight );
//return 0.5 * ViewParams.TSLight.z;
//return ViewParams.Half.z;

		BaseThetaHD += StepX;
	}

return 10.0 * Reflection / (nStepsCountX*nStepsCountY);
return Reflection / nStepsCountX;

#endif


	float3	AccDiffuse = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ).xyz;
	float3	AccSpecular = _TexDiffuseSpecular.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ).xyz;

AccSpecular += Reflection;

//return AccSpecular;
//return AccDiffuse;

//return 0.1 * _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).y;
//return _TexDEBUGBackGBuffer.SampleLevel( LinearClamp, UV, 0.0 );

	return AccDiffuse + AccSpecular;
}