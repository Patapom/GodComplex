#line 1 ""


#line 4


#line 1 "Inc/Global.hlsl"


#line 5



#line 9
static const float RESX = 1280.0 ; 
static const float RESY = 720.0 ; 
static const float ASPECT_RATIO = RESX / RESY ; 
static const float INV_ASPECT_RATIO = RESY / RESX ; 
static const float2 SCREEN_SIZE = float2 ( RESX , RESY ) ; 
static const float2 INV_SCREEN_SIZE = float2 ( 1.0 / RESX , 1.0 / RESY ) ; 

static const float PI = 3.1415926535897932384626433832795 ; 
static const float TWOPI = 6.283185307179586476925286766559 ; 
static const float HALFPI = 1.5707963267948966192313216916398 ; 
static const float INVPI = 0.31830988618379067153776752674503 ; 
static const float INVHALFPI = 0.63661977236758134307553505349006 ; 
static const float INVTWOPI = 0.15915494309189533576888376337251 ; 

static const float3 LUMINANCE = float3 ( 0.2126 , 0.7152 , 0.0722 ) ; 

static const float INFINITY = 1e6 ; 

#line 28


#line 31


#line 37
SamplerState LinearClamp : register ( s0 ) ; 
SamplerState PointClamp : register ( s1 ) ; 
SamplerState LinearWrap : register ( s2 ) ; 
SamplerState PointWrap : register ( s3 ) ; 
SamplerState LinearMirror : register ( s4 ) ; 
SamplerState PointMirror : register ( s5 ) ; 

#line 49
cbuffer cbCamera : register ( b0 ) 
{ 
    float4 _CameraData ; 
    float4x4 _Camera2World ; 
    float4x4 _World2Camera ; 
    float4x4 _Camera2Proj ; 
    float4x4 _Proj2Camera ; 
    float4x4 _World2Proj ; 
    float4x4 _Proj2World ; 
} ; 

cbuffer cbGlobal : register ( b1 ) 
{ 
    float4 _Time ; 
} ; 

#line 67
Texture3D _TexNoise3D : register ( t0 ) ; 

#line 78
float3 Distort ( float3 _Position , float3 _Normal , float4 _NoiseOffset ) 
{ 
    return _Position + _NoiseOffset . w * _TexNoise3D . SampleLevel ( LinearWrap , 0.2 * ( _Position + _NoiseOffset . xyz ) , 0.0 ) . xyz ; 
} 

#line 95


#line 109
float3 RotateVector ( float3 _Vector , float3 _Axis , float _Angle ) 
{ 
    float2 SinCos ; 
    sincos ( _Angle , SinCos . x , SinCos . y ) ; 
    
    float3 Result = _Vector * SinCos . y ; 
    float temp = dot ( _Vector , _Axis ) ; 
    temp *= 1.0 - SinCos . y ; 
    
    Result += _Axis * temp ; 
    
    float3 Ortho = cross ( _Axis , _Vector ) ; 
    
    Result += Ortho * SinCos . x ; 
    
    return Result ; 
} 




#line 7 ""
cbuffer cbObject : register ( b10 ) 
{ 
    float4x4 _Local2View ; 
    float4x4 _View2Proj ; 
    float2 _dUV ; 
} ; 

cbuffer cbShadow : register ( b11 ) 
{ 
    float4x4 _World2Shadow ; 
    float4x4 _Shadow2World ; 
    float _ShadowZMax ; 
} ; 

#line 22
struct VS_IN 
{ 
    float3 Position : POSITION ; 
} ; 

struct PS_IN 
{ 
    float4 __Position : SV_POSITION ; 
    float Z : DEPTH ; 
} ; 

PS_IN VS ( VS_IN _In ) 
{ 
    float4 ViewPosition = mul ( float4 ( _In . Position , 1.0 ) , _Local2View ) ; 
    float Z = ViewPosition . z ; 
    float4 ProjPosition = mul ( ViewPosition , _View2Proj ) ; 
    
    PS_IN Out ; 
    Out . __Position = ProjPosition ; 
    Out . Z = Z ; 
    
    return Out ; 
} 

float4 PS ( PS_IN _In ) : SV_TARGET0 
{ 
    
    return _In . Z ; 
} 
