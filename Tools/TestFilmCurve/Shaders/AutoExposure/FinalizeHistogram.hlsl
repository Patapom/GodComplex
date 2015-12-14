/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Finalizes the histogram by adding all (ImageHeight / 4) scanlines together into a single histogram
//	• The shader is operating vertically on (ImageHeight / 4) scanlines, summing for an entire column on each dispatch
//	• Thus, 128 calls on X must be dispatched to cover the entire histogram
//
#include "../Global.hlsl"
#include "Common.hlsl"

#define NUMTHREADX	1
#define NUMTHREADY	512	// NOTE: This exceeds the advised amount of threads but I don't want to split this shader into multiple passes again
#define NUMTHREADZ	1	// 512 vertical threads ensures a maximum vertical image size of 2048 so we're pretty safe on that side for now...

Texture2D<uint>		_texTallHistogram : register(t0);	// The histogram from last pass used as a RO texture!
RWTexture2D<uint>	_texFinalHistogram : register(u0);	// The final histogram

groupshared uint	Rows[NUMTHREADY];	// Here we're going to read many rows from the multi-rows histogram

[numthreads( NUMTHREADX, NUMTHREADY, NUMTHREADZ )]
void CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	Dimensions;
	_texTallHistogram.GetDimensions( Dimensions.x, Dimensions.y );

	// Initialize source histogram rows
	uint	RowIndex = NUMTHREADY * _GroupID.y + _GroupThreadID.y;
	Rows[_GroupThreadID.y] = RowIndex < Dimensions.y ? _texTallHistogram[uint2( _GroupID.x, RowIndex )] : 0;
	GroupMemoryBarrierWithGroupSync();

	// Add the 64 last rows to their corresponding 64 first rows
	if ( _GroupThreadID.y < 64 ) {
		Rows[_GroupThreadID.y] += Rows[_GroupThreadID.y+64];
	}
	GroupMemoryBarrierWithGroupSync();

	// Add the 32 last rows to their corresponding 32 first rows
	if ( _GroupThreadID.y < 32 ) {
		Rows[_GroupThreadID.y] += Rows[_GroupThreadID.y+32];
	}
	GroupMemoryBarrierWithGroupSync();

	// Add the 16 last rows to their corresponding 16 first rows
	if ( _GroupThreadID.y < 16 ) {
		Rows[_GroupThreadID.y] += Rows[_GroupThreadID.y+16];
	}
	GroupMemoryBarrierWithGroupSync();

	// Add the 8 last rows to their corresponding 8 first rows
	if ( _GroupThreadID.y < 8 ) {
		Rows[_GroupThreadID.y] += Rows[_GroupThreadID.y+8];
	}
	GroupMemoryBarrierWithGroupSync();

	// Add the 4 last rows to their corresponding 4 first rows
	if ( _GroupThreadID.y < 4 ) {
		Rows[_GroupThreadID.y] += Rows[_GroupThreadID.y+4];
	}
	GroupMemoryBarrierWithGroupSync();

// 	// Add the 2 last rows to their corresponding 2 first rows
// 	if ( _GroupThreadID.y < 2 ) {
// 		Rows[_GroupThreadID.y] += Rows[_GroupThreadID.y+2];
// 	}
// 	GroupMemoryBarrierWithGroupSync();

	// Write the sum of the 4 remaining rows
	if ( _GroupThreadID.y == 0 ) {
//		_texFinalHistogram[uint2( _GroupID.x, 0 )] = Rows[0] + Rows[1];
		_texFinalHistogram[uint2( _GroupID.x, 0 )] = Rows[0] + Rows[1] + Rows[2] + Rows[3];
//_texFinalHistogram[uint2( _GroupID.x, 0 )] = uint( floor( 1000 * abs(sin( _GroupID.x * 3.14 / 128 )) ) );
	}
}
