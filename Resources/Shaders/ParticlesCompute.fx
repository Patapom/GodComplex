//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.fx"

Texture2D	_TexParticlesPositions0	: register(t10);
Texture2D	_TexParticlesPositions1	: register(t11);
Texture2D	_TexParticlesNormals	: register(t12);
Texture2D	_TexParticlesTangents	: register(t13);

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
	float4	Normal		: SV_TARGET1;
	float4	Tangent		: SV_TARGET2;
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

	// Read back positions from frame-2 and frame-1
	float4	Position1 = _TexParticlesPositions1.SampleLevel( PointClamp, UV, 0.0 );
	float	Life = Position1.w + _Time.y;	// Increase life
	float4	Position2 = _TexParticlesPositions0.SampleLevel( PointClamp, UV, 0.0 );
	float3	Pt_1 = Position1.xyz;
	float3	Pt_2 = Position2.xyz;
			Pt_2 = Pt_1 - (Pt_1 - Pt_2) * _DeltaTime.y;	// Time-correction

	// Life is invalid if negative
	float	InvalidLife = saturate( 10000.0 * Life );

	// Read back normal & tangent
	float4	NormalSize = _TexParticlesNormals.SampleLevel( PointClamp, UV, 0.0 );
	float3	Normal = NormalSize.xyz;
	float	Size = NormalSize.w;
	float4	TangentBehavior = _TexParticlesTangents.SampleLevel( PointClamp, UV, 0.0 );
	float3	Tangent = TangentBehavior.xyz;

	float3	Acceleration = 0.0;
	float3	Velocity = 0.0;

	///////////////////////////////////////////
	// Manage positions
	if ( TangentBehavior.w > 0.0 )
	{	// The particle should get uplifted by buoyancy
		Acceleration += float3( 0, 0.0005, 0 );	// Add buoyancy...

		// Add noise to make it move
		float3	UVW = 1.0 * Pt_1;
				UVW += 400.0 * _Time.x;

		Acceleration += 0.01 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 ).xyz;
//		Velocity += 0.0002 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 ).xyz;
	}
	else
	{	// The particle should fall with gravity
		Acceleration -= float3( 0, 0.0005, 0 );	// Add gravity...

		// Add a tiny noise but quickly moving
		float3	UVW = 10.0 * Pt_1;
				UVW += 400.0 * _Time.x;
		Acceleration += 0.025 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 ).xyz;
	}

	// Nullify velocity and acceleration if life is negative
	Velocity *= InvalidLife;
	Acceleration *= InvalidLife;

	float3	NewPosition = 2.0 * Pt_1 - Pt_2 + _DeltaTime.x * (Velocity + _DeltaTime.x * Acceleration);
//	float3	NewPosition = Pt_1  + ( Pt_1 - Pt_2) * (_DeltaTime.x / _PreviousDeltaTime) + Acceleration * _DeltaTime.x * _DeltaTime.x;

//NewPosition = Pt_1;

	///////////////////////////////////////////
	// Manage rotations
	float3	UVW = 0.5 * Pt_1;
	float4	AxisAngle = 1.0 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );
			AxisAngle.xyz = normalize( AxisAngle.xyz );
			AxisAngle.w *= _DeltaTime.x;

	// Nullify rotation if life is negative
	AxisAngle.w *= InvalidLife;

	float3	NewNormal = normalize( RotateVector( Normal, AxisAngle.xyz, AxisAngle.w ) );
	float3	NewTangent = normalize( RotateVector( Tangent, AxisAngle.xyz, AxisAngle.w ) );

//NewNormal = Normal;
//NewNormal = float3( 0, 1, 0 );
//NewTangent = Tangent;
//NewTangent = float3( 1, 0, 0 );

	///////////////////////////////////////////
	// Manage size
	float	dSize = -0.2 * _Time.y;
			dSize *= InvalidLife;
	Size = max( 0.0, Size + dSize );	// Decrease size with time...

	PS_OUT	Out;
	Out.Position = float4( NewPosition, Life );
	Out.Normal = float4( NewNormal, Size );
	Out.Tangent = float4( NewTangent, TangentBehavior.w);

	return Out;
}
