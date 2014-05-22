// CANDELA-SSRR SCREEN SPACE RAYTRACED REFLECTIONS
// Copyright 2014 Livenda

Shader "Hidden/CustomDepthSSRR" {
SubShader {
//ZTest Always Cull Off ZWrite Off Fog { Mode Off }
    Tags { "RenderType"="Opaque" }
    Pass {
        Fog { Mode Off }
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

struct v2f {
    float4 pos : SV_POSITION;
    float2 mypos : TEXCOORD1;
};

v2f vert (appdata_base v) {
    v2f o;
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.mypos = o.pos.zw;
    return o;
}

half4 frag(v2f i) : COLOR {
    return  i.mypos.x/i.mypos.y;
}
ENDCG
    }
}
}