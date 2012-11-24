//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.fx"

Texture2D	_TexParticlesPosition0	: register(t10);
Texture2D	_TexParticlesPosition1	: register(t11);
Texture2D	_TexParticlesRotation0	: register(t12);
Texture2D	_TexParticlesRotation1	: register(t13);

//[
cbuffer	cbRender	: register( b10 )
{
	float3	_dUV;		// XY=1/BufferSize Z=0
	float2	_DeltaTime;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_OUT
{
	float4	Position	: SV_TARGET0;
	float3	Orientation	: SV_TARGET1;
};

float3	RotateVector( float3 _Vector, float3 _Axis, float _Angle )
{
	float2	SinCos;
	sincos( _Angle, SinCos.x, SinCos.y );

	float3	Result = _Vector * SinCos.y;
	float	temp = dot( _Vector, _Axis );
			temp *= 1.0 - SinCos.y;

	Result += _Axis * temp;

	float3	Ortho = cross( _Axis, _Vector );

	Result += Ortho * SinCos.x;

	return Result;
}

VS_IN	VS( VS_IN _In )	{ return _In; }

PS_OUT	PS( VS_IN _In )
{
	float2	UV = _In.__Position.xy * _dUV.xy;

	float4	Position2 = _TexParticlesPosition0.SampleLevel( PointClamp, UV, 0.0 );
	float4	Position1 = _TexParticlesPosition1.SampleLevel( PointClamp, UV, 0.0 );
	float3	Pt_2 = Position2.xyz;
	float3	Pt_1 = Position1.xyz;

	float3	Dir_2 = _TexParticlesRotation0.SampleLevel( PointClamp, UV, 0.0 ).xyz;
	float3	Dir_1 = _TexParticlesRotation1.SampleLevel( PointClamp, UV, 0.0 ).xyz;

	Pt_2 = Pt_1 - (Pt_1 - Pt_2) * _DeltaTime.y;
	Dir_2 = Dir_1 - (Dir_1 - Dir_2) * _DeltaTime.y;

	float3	Acceleration = 0.0;
	float3	Velocity = 0.0;

	///////////////////////////////////////////
	// Manage positions
	float3	UVW = 1.0 * Pt_1;
			UVW += 10.0 * _Time.x;
 	Acceleration += 0.01 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 ).xyz;
//	Velocity += 0.0002 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 ).xyz;

	Acceleration += float3( 0, 0.001, 0 );

	float3	NewPosition = 2.0 * Pt_1 - Pt_2 + _DeltaTime.x * (Velocity + _DeltaTime.x * Acceleration);
//	float3	NewPosition = Pt_1  + ( Pt_1 - Pt_2) * (_DeltaTime.x / _PreviousDeltaTime) + Acceleration * _DeltaTime.x * _DeltaTime.x;

	///////////////////////////////////////////
	// Manage rotations
	UVW = 0.5 * Pt_1;
	float4	AxisAngle = 1.0 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );
			AxisAngle.xyz = normalize( AxisAngle.xyz );
			AxisAngle.w *= _DeltaTime.x;

	float3	NewRotation = normalize( RotateVector( Dir_1, AxisAngle.xyz, AxisAngle.w ) );

//NewRotation = normalize( float3( 1, 1, 1 ) );	// ###
//NewRotation = Dir_1;

	PS_OUT	Out;
	Out.Position = float4( NewPosition, 0 );
	Out.Orientation = NewRotation;

	return Out;
}
