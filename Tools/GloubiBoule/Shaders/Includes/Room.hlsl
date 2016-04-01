///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Room Definition
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
static const float3		ROOM_CENTER = 0.0;
static const float3		ROOM_HALF_SIZE = 4.0;
static const float3		ROOM_SIZE = 2 * ROOM_HALF_SIZE;
static const float3		ROOM_INV_SIZE = 1.0 / ROOM_SIZE;
static const float3		ROOM_MIN = ROOM_CENTER - ROOM_HALF_SIZE;
static const uint3		ROOM_VOLUME_SIZE = 128;
static const uint3		ROOM_INV_VOLUME_SIZE = 1.0 / ROOM_VOLUME_SIZE;


uint3	World2RoomCellIndex( float3 _wsPosition ) {
	float3	wsDelta = _wsPosition - ROOM_MIN;
	return (ROOM_VOLUME_SIZE * ROOM_INV_SIZE) * wsDelta;
}

float3	World2RoomUVW( float3 _wsPosition ) {
	float3	wsDelta = _wsPosition - ROOM_MIN;
	return wsDelta * ROOM_INV_SIZE;
}

// Converts a cell index into a world space position
// The returned position is in the middle of the cell
float3	RoomCellIndex2World( uint3 _cellIndex ) {
	return ROOM_MIN + (_cellIndex + 0.5) * (ROOM_SIZE * ROOM_INV_VOLUME_SIZE);
}