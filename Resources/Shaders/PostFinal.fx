#include "TestInclude.fx"

struct	VS_IN
{
	float4	Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	return OUTPUT_COLOR;
}