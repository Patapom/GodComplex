// Cornell Box Dimensions
//
#ifndef CORNELL_BOX_INCLUDED
#define CORNELL_BOX_INCLUDED

static const float3	CORNELL_SIZE = float3( 5.528f, 5.488f, 5.592f );
static const float3	CORNELL_POS = 0.0;
static const float	CORNELL_THICKNESS = 0.1f;

// Small box setup
static const float3	CORNELL_SMALL_BOX_SIZE = float3( 1.65, 1.65, 1.65 );	// It's a cube
static const float3	CORNELL_SMALL_BOX_POS = float3( 1.855, 0.5 * CORNELL_SMALL_BOX_SIZE.y, 1.69 ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float	CORNELL_SMALL_BOX_ANGLE = 0.29145679447786709199560462143289;	// ~16°

// Large box setup
static const float3	CORNELL_LARGE_BOX_SIZE = float3( 1.65, 3.3, 1.65 );
static const float3	CORNELL_LARGE_BOX_POS = float3( 3.685, 0.5 * CORNELL_LARGE_BOX_SIZE.y, 3.6125 ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float	CORNELL_LARGE_BOX_ANGLE = -0.30072115015043337195437489062082;	// ~17°

// Light setup
static const float3	CORNELL_LIGHT_SIZE = float3( 1.3, 0.0, 1.05 );
static const float3	CORNELL_LIGHT_POS = float3( 2.78, 5.2, 2.795 ) - 0.0 * float3( CORNELL_LIGHT_SIZE.x, 0.0, CORNELL_LIGHT_SIZE.z ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float3	LIGHT_ILLUMINANCE = 50.0;
static const float3	LIGHT_SIZE = float3( 1.0, 0.0, 1.0 );

#endif
