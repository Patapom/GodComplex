#include "Global.hlsl"

Texture2D< float >		_TexDepthStencil : register(t0);
RWTexture3D< float4 >	_TexDistance : register(u0);

[numthread( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {


}
