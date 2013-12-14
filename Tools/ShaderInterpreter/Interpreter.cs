using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ShaderInterpreter.ShaderMath;

namespace ShaderInterpreter
{
	public class	Interpreter : ShaderSyntaxSupport
	{
// 		cbuffer	cbCubeMapCamera	//: register( b9 )
//		{
			float4x4	_CubeMapWorld2Proj;
// 		};
// 
// 		cbuffer	cbObject	//: register( b10 )
// 		{
			float4x4	_Local2World;
// 		};
// 
// 		cbuffer	cbMaterial	//: register( b11 )
// 		{
			float3		_DiffuseAlbedo;
			bool		_HasDiffuseTexture;
			float3		_SpecularAlbedo;
			bool		_HasSpecularTexture;
			float		_SpecularExponent;
// 		};

		Sampler				LinearWrap;

		Texture2D<float4>	_TexDiffuseAlbedo; //: register( t10 );
		Texture2D<float4>	_TexSpecularAlbedo; //: register( t11 );

		float4x4		_Camera2World;

		struct	VS_IN
		{
			public float3	Position	; // [POSITION]
			public float3	Normal		; // [NORMAL]
			public float3	Tangent		; // [TANGENT]
			public float3	BiTangent	; // [BITANGENT]
			public float3	UV			; // [TEXCOORD0]
		};

		struct	PS_IN
		{
			public float4	__Position	; // [SV_POSITION]
			public float3	Position	; // [POSITION]
			public float3	Normal		; // [NORMAL]
			public float3	Tangent		; // [TANGENT]
			public float3	BiTangent	; // [BITANGENT]
			public float3	UV			; // [TEXCOORD0]
		};

		struct	PS_OUT
		{
			public float3	DiffuseAlbedo	; // : SV_TARGET0;
			public float4	NormalDistance	; // : SV_TARGET1;
		};

		PS_IN	VS( VS_IN _In )
		{
			float4	WorldPosition = mul( new float4( _In.Position, 1.0f ), _Local2World );

			PS_IN	Out;
			Out.__Position = mul( WorldPosition, _CubeMapWorld2Proj );
			Out.Position = WorldPosition.xyz;
			Out.Normal = mul( _float4( _In.Normal, 0.0f ), _Local2World );
			Out.Tangent = mul( _float4( _In.Tangent, 0.0f ), _Local2World );
			Out.BiTangent = mul( _float4( _In.BiTangent, 0.0f ), _Local2World );
			Out.UV = _In.UV;

			return Out;
		}

		PS_OUT	PS( PS_IN _In )
		{
			PS_OUT	Out;
			Out.DiffuseAlbedo = _DiffuseAlbedo;
			if ( _HasDiffuseTexture )
				Out.DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV ).xyz;

		//	Out.NormalDistance = _float4( normalize( _In.Normal ), length( _In.Position - _Camera2World[3].xyz ) );	// Store distance
			Out.NormalDistance = _float4( normalize( _In.Normal ), dot( _In.Position - _Camera2World[3].xyz, _Camera2World[2].xyz ) );	// Store Z
	
			return Out;
		}

	}
}
