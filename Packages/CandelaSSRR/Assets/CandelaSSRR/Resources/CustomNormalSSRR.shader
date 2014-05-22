// CANDELA-SSRR SCREEN SPACE RAYTRACED REFLECTIONS
// Copyright 2014 Livenda

    Shader "Hidden/CandelaWorldNormal"
    {
      Properties
      {
      	_MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _Shininess ("Shininess", Range (0.03, 1)) = 1
        _SpecTex ("Specular(RGB) Roughness(A)", 2D) = "white" {}
       
      }
      SubShader
      {
        Tags { "RenderType"="Opaque" }
        Pass {
         
          CGPROGRAM
          #pragma vertex vert
          #pragma fragment frag
          #include "UnityCG.cginc"
     
          sampler2D _BumpMap;
          float4 _BumpMap_ST;
     	  half _Shininess;
     	  sampler2D _SpecTex;
     	  sampler2D _MainTex;
     	  float4 _MainTex_ST;

     	  
          struct v2f
          {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 TtoW0 : TEXCOORD1;
            float3 TtoW1 : TEXCOORD2;
            float3 TtoW2 : TEXCOORD3;
            float2 uvs   : TEXCOORD4;
          };
     
          v2f vert (appdata_tan v)
          {
            v2f o;
            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
            o.uv = TRANSFORM_TEX (v.texcoord, _BumpMap);
     		o.uvs = TRANSFORM_TEX (v.texcoord, _MainTex);
			
     		
            TANGENT_SPACE_ROTATION;
              o.TtoW0 = mul(rotation, _Object2World[0].xyz * unity_Scale.w);
              o.TtoW1 = mul(rotation, _Object2World[1].xyz * unity_Scale.w);
              o.TtoW2 = mul(rotation, _Object2World[2].xyz * unity_Scale.w);
     
            return o;
          }
     
          fixed4 frag (v2f i) : COLOR0
          {
            fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
           
            fixed3 normalWS;
            normalWS.x = dot(i.TtoW0, normal);
            normalWS.y = dot(i.TtoW1, normal);
            normalWS.z = dot(i.TtoW2, normal);
           
            fixed4 color;  
            color.xyz = normalWS * 0.5 + 0.5;
            
            float spe = tex2D(_SpecTex, i.uvs).a;
            
            color.a = _Shininess*spe;
            
            return color;
          }
          ENDCG
        }
      }
    }