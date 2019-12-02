//////////////////////////////////////////////////////////////////////////
// This shader performs light diffusion through the volume
//
#include "Inc/Global.hlsl"

static const float2	dUV = 1.0 / TARGET_SIZE;	// TARGET_SIZE is passed as a macro and represents the size of the render target

//[
cbuffer	cbDiffusion	: register( b10 )
{
	float	_BBoxSize;					// Size of the bbox (in meters)
	float	_SliceThickness;			// Thickness of each slice we take (in meters)
	float	_TexelSize;					// Size of a texel (in meters)
	float3	_ExtinctionCoeff;			// Extinction through the material
	float3	_Albedo;					// Material albedo (to determine scattering)
	float3	_Phase0, _Phase1, _Phase2;	// The phase weights at 0, 1 and 2 pixels away from sampling position

	float3	_ExternalLight;				// The external light color
	float3	_InternalEmissive;			// The emissive color of the internal object (normally 0)
};

cbuffer	cbPass	: register( b11 )
{
	float	_PassIndex;					// Index of the pass
	float	_CurrentZ;					// Current pass' Z in [0,2]
	float	_NextZ;						// Next pass' Z in [0,2]
};
//]

Texture2D	_TexZBuffer		: register(t10);
Texture2D	_TexIrradiance	: register(t11);


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }


// #define SAMPLE( x, y, i )	I += SampleIrradiance( UV, float2( x, y ), _Phase##i );
float3	SampleIrradiance( float2 _UV, float2 _dXY, float3 _Phase )
{
	float3	NeighborIrradiance = TEXLOD( _TexIrradiance, LinearMirror, _UV + _dXY * dUV, 0.0 ).xyz;
	float3	Extinction = 1.0;//exp( -_ExtinctionCoeff * length( _dXY ) );
	return _Phase * Extinction * NeighborIrradiance;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * dUV;

	float4	ObjZ = TEXLOD( _TexZBuffer, LinearClamp, UV, 0.0 );	// Objects' Z in [0,2]

	clip( _NextZ - ObjZ.x );	// Don't do anything if we're standing BEFORE the object's closest Z (i.e. we've not entered the object yet !)
	clip( ObjZ.y - _CurrentZ );	// Don't do anything if we're standing AFTER the object's farthest Z (i.e. we've already exited the object !)

// return float4( UV, 0, 0 );
//return 0.5 * ObjZ.x;
 
 	// Determine if we should initialize irradiance rather than diffuse existing one
	if ( _CurrentZ <= ObjZ.x )
	{	// Because of clip( _NextZ - ObjZ.x ) earlier, we have realized the condition that
		//	CurrentZ < Zin < NextZ  with Zin the Z at which we enter the object
		// This means that we're penetrating the object's input boundary where it's supposed to be lit by the light source
		// This is the time we initialize the irradiance !
		//
		float3	Position = float3( 2.0 * UV - 1.0, _CurrentZ );
		float3	Normal = float3( Position.xy, 1.0 - Position.z );

		float3	LightPosition = float3( 0.0, 0.0, 4.0 );

		float3	ToLight = normalize( LightPosition - Position );
		float	Diffuse = saturate(dot( Normal, ToLight ));

//		return 100.0;
		return float4( Diffuse * _ExternalLight, 0.0 );
	}

	// Determine if we're inside the internal object
	if ( _CurrentZ > ObjZ.z && _CurrentZ < ObjZ.w )
		return float4( _InternalEmissive, 0.0f );	// Simply store the internal object's emissive color. By default it's black so it's simply a blocker of irradiance...

	// We're definitely completely inside the object and outside of the internal blocker so we should perform diffusion...
	float3	SliceExtinction = exp( -_ExtinctionCoeff * _SliceThickness );

// float3	I = TEXLOD( _TexIrradiance, LinearClamp, UV, 0.0 ).xyz;
// return float4( SliceExtinction * I, 0.0 );

	// Try with 9 samples first
	float3	I = 0.0;

	I += SampleIrradiance( UV, float2( -1, -1 ), _Phase1 );	I += SampleIrradiance( UV, float2(  0, -1 ), _Phase1 );	I += SampleIrradiance( UV, float2( +1, -1 ), _Phase1 );
	I += SampleIrradiance( UV, float2( -1,  0 ), _Phase1 );	I += SampleIrradiance( UV, float2(  0,  0 ), _Phase0 );	I += SampleIrradiance( UV, float2( +1,  0 ), _Phase1 );
	I += SampleIrradiance( UV, float2( -1, +1 ), _Phase1 );	I += SampleIrradiance( UV, float2(  0, +1 ), _Phase1 );	I += SampleIrradiance( UV, float2( +1, +1 ), _Phase1 );

	// Then add 12 more samples
	I += SampleIrradiance( UV, float2( -1, -2 ), _Phase2 );	I += SampleIrradiance( UV, float2(  0, -2 ), _Phase2 );	I += SampleIrradiance( UV, float2( +1, -2 ), _Phase2 );
	I += SampleIrradiance( UV, float2( -2, -1 ), _Phase2 );															I += SampleIrradiance( UV, float2( +2, -1 ), _Phase2 );
	I += SampleIrradiance( UV, float2( -2,  0 ), _Phase2 );															I += SampleIrradiance( UV, float2( +2,  0 ), _Phase2 );
	I += SampleIrradiance( UV, float2( -2, +1 ), _Phase2 );															I += SampleIrradiance( UV, float2( +2, +1 ), _Phase2 );
	I += SampleIrradiance( UV, float2( -1, +2 ), _Phase2 );	I += SampleIrradiance( UV, float2(  0, +2 ), _Phase2 );	I += SampleIrradiance( UV, float2( +1, +2 ), _Phase2 );

	return float4( SliceExtinction * I, 0.0 );
}
