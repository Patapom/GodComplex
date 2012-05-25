#include"Inc/TestInclude.fx"
struct VS_IN{float4 Position:SV_POSITION;};VS_IN VS(VS_IN P){return P;}float4 PS(VS_IN P):SV_TARGET0{return OUTPUT_COLOR;}