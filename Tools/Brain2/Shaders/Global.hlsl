////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Common code
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

#define PI		3.1415926535897932384626433832795
#define INVPI	0.31830988618379067153776752674503
#define SQRT2	1.4142135623730950488016887242097

cbuffer CB_Main : register(b0) {
	float4		_resolution;	// XY=Viewport resolution (in pixels), ZW=Reciprocal of viewport resolution
	float4		_mouseUVs;		// XY=Current Mouse UV, ZW=Unused
//	float		_time;
//	float		_deltaTime;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_camera2World;
	float4x4	_world2Camera;
	float4x4	_proj2World;
	float4x4	_world2Proj;
	float4x4	_camera2Proj;
	float4x4	_proj2Camera;

//	float4		_ZNearFar_Q_Z;			// XY=Near/Far Clip, Z=Q=Zf/(Zf-Zn), W=0
//	float4		_cameraSubPixelOffset;	// XY=Un-jitter vector, ZW=sub-pixel jitter offset
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
