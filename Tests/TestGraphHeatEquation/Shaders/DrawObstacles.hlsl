#include "Global.hlsl"

Texture2D< float4 >	_texObstacles : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	P = _In.__Position.xy - 0.5;
	float4	obstacle = _texObstacles[P];

	obstacle.y = 0.0;	// Always clear sources

	if ( all(uint2(mousePosition) == P) ) {
		// Use right mouse button to set or clear obstacles
		if ( mouseButtons & 4 )
			obstacle.x = 1;	// Right = set
		if ( (mouseButtons & 12) == 12 )
			obstacle.x = 0;	// Right + Shift = clear

		// Use left mouse button to set source
		if ( mouseButtons & 1 )
			obstacle.y = 1.0;
	}

	return obstacle;
}
