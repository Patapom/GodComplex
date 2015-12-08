
cbuffer CB_Camera : register( b0 ) {
	float4x4	_World2Proj;
	uint		_ScreenWidth;
	uint		_ScreenHeight;

	uint		_ImageWidth;
	uint		_ImageHeight;
	uint		_ImageDepth;
	uint		_ImageType;

	float		_Time;
	float		_MipLevel;
	float		_Exposure;
};

Texture2DArray<float4>		_Tex2D : register(t0);
TextureCubeArray<float4>	_TexCube : register(t1);
Texture3D<float4>			_Tex3D : register(t2);

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};


PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.__Position = _In.__Position;
	Out.UV = float2( 0.5 * (1.0 + _In.__Position.x), 0.5 * (1.0 - _In.__Position.y ) );

	return Out;
}

float3	Show2D( PS_IN _In ) {
	uint	SliceIndex = 0;
	return _Tex2D.SampleLevel( PointClamp, float3( _In.UV, SliceIndex ), _MipLevel ).xyz;
}

float3	ShowCube( PS_IN _In ) {
	float2	UV = 2.05 * (_In.UV - 0.5) * float2( float(_ScreenWidth) / _ScreenHeight, -1.0 );
	float	sqDistance = dot( UV, UV );
	if ( sqDistance >= 1.0 )
		return float3( 0,0, 0 );

	float	Z = sqrt( 1.0 - sqDistance );
	float3	Direction = float3( UV, Z ).zxy;

	float	angle = 0.4f * _Time;
	float2	scRot;
	sincos( angle, scRot.x, scRot.y );
	float3x3	Rot = float3x3(	 scRot.y, scRot.x, 0.0,
								-scRot.x, scRot.y, 0.0,
								0, 0, 1 );

	Direction = mul( Direction, Rot );

//	return Direction;
	return _TexCube.SampleLevel( LinearWrap, float4( Direction, 0 ), _MipLevel ).xyz;
}

float3	Show3D( PS_IN _In ) {
	return 0.0;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float3	Color = float3( _In.UV, 0 );
	switch ( _ImageType ) {
		case 0: Color = Show2D( _In ); break;
		case 1: Color = ShowCube( _In ); break;
		case 2: Color = Show3D( _In ); break;
	}

	Color *= _Exposure;

	return float4( Color, 1 );
}