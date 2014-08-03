////////////////////////////////////////////////////////////////////////////////
// Displays the photons as vectors
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"
#include "../Noise.hlsl"

cbuffer	cbRender : register(b8)
{
	uint		_VectorsPerFace;
	float		_VectorSizeMultiplier;
	float		_ClipAboveValue;
}

Texture2DArray<float4>	_TexPhotons : register(t0);

struct VS_IN
{
	float3	Position : POSITION;
	uint	PhotonIndex : SV_INSTANCEID;
};

struct PS_IN
{
	float4	__Position : SV_POSITION;
};

PS_IN	VS( VS_IN _In )
{
	uint	FaceIndex = _In.PhotonIndex / _VectorsPerFace;
	uint	PhotonIndex = _In.PhotonIndex % _VectorsPerFace;
	float	PhotonsPerSide = sqrt( PhotonIndex );

	float2	UV = float2( fmod( PhotonIndex, PhotonsPerSide ) / PhotonsPerSide, float( PhotonIndex ) / _VectorsPerFace );

	float3	PhotonPosition = _TexPhotons.SampleLevel( PointClamp, float3( UV, 6*0 + FaceIndex ), 0.0 ).xyz;
	float3	PhotonDirection = _TexPhotons.SampleLevel( PointClamp, float3( UV, 6*1 + FaceIndex ), 0.0 ).xyz;
	float	PhotonIntensity = _TexPhotons.SampleLevel( LinearClamp, float3( UV, 6*3 + FaceIndex ), 0.0 ).x;

	float	VectorLength = 0.0;
	if ( PhotonIntensity < _ClipAboveValue )
		VectorLength = 10.0 * _VectorSizeMultiplier * PhotonIntensity;

	PS_IN	Out;
	Out.__Position = mul( float4( PhotonPosition + VectorLength * _In.Position.x * PhotonDirection, 1.0 ), _World2Proj );

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return float4( 1, 1, 0, 0 );
}
