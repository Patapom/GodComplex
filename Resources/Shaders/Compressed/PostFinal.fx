#include"Inc/Global.fx"
struct VS_IN{float4 __Position:SV_POSITION;};VS_IN VS(VS_IN V){return V;}float4 PS(VS_IN V):SV_TARGET0{return float4(V.__Position.xy*INV_SCREEN_SIZE,0,0);}