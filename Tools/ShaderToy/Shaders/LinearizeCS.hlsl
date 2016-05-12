//////////////////////////////////////////////////////////////////////////////////////////////
// Depth buffer linearization
//////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Includes/Global.hlsl"

cbuffer CB_Downsample : register(b2) {
	uint2	_depthBufferSize;
};

Texture2D<float4>	_texDepthBuffer				: register(t0);	// SRV, projected depth
RWTexture2D<float>	_texDepthBufferUAV			: register(u0);	// UAV, full-res linear depth in [ZNear, ZFar]


//////////////////////////////////////////////////////////////////////////////////////////////
// Depth linearization
float	ComputeLinearZ( float _projZ ) {
	float	temp = _Proj2Camera[2].w * _projZ + _Proj2Camera[3].w;
	return _projZ / temp;
}

[numthreads( 8, 8, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position = _DispatchThreadID.xy;	// Pixel position
	if ( all( position < _depthBufferSize ) ) {
		float	rawZ = _texDepthBuffer.Load( uint3( position, 0 ) ).w;		// Depth is stored in the W component
		_texDepthBufferUAV[position] = ComputeLinearZ( rawZ );
	}
}
