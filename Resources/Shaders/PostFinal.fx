#include "Inc/Global.fx"

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	return float4( _In.__Position.xy * INV_SCREEN_SIZE, 0, 0 );
}
