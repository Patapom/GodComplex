#include "Math.h"

SharpMath::float3	SharpMath::float2::Cross( float2 b ) {
	return float3( 0.0f, 0.0f, x * b.y - y * b.x );
}
