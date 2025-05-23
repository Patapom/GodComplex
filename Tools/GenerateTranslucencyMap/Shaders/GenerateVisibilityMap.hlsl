////////////////////////////////////////////////////////////////////////////////
// Visibility Map generator
// This compute shader will generate a 3D texture based containing horizon angles
//	for a limited amount of directions, each direction is stored in a separate slice.
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2 degree observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

static const uint	SLICES_COUNT = 32;		// Amount of azimutal slices
static const uint	RAYS_COUNT = 32;		// Rays count per slice

cbuffer	CBInput : register( b0 )
{
	uint	_Width;
	uint	_Height;
	uint	_Y0;				// Start scanline for this group
	float	_TexelSize_mm;		// Texel size in millimeters
	float	_Thickness_mm;		// Thickness map max encoded displacement in millimeters
	uint	_DiscRadius;		// Radius of the disc where the horizon will get sampled
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture2D<float>			_SourceThickness : register( t0 );
RWTexture3D<float>			_Target : register( u0 );

groupshared float			gs_MaximumAngle[SLICES_COUNT*RAYS_COUNT];	// Maximum collected angle along each slice for each ray

[numthreads( SLICES_COUNT, RAYS_COUNT, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID )
{
	uint2	PixelPosition = uint2( _GroupID.x, _Y0 + _GroupID.y );
	if ( PixelPosition.x >= _Width || PixelPosition.y >= _Height )
		return;

	uint	sliceIndex = _GroupThreadID.x;
	uint	rayIndex = _GroupThreadID.y;

	// Sample current height
	float	H0 = _Thickness_mm * _SourceThickness.Load( uint3( PixelPosition, 0 ) ).x;

	// Determine azimuth
	float	phi = 2.0 * PI * (float(sliceIndex) + float(rayIndex) / RAYS_COUNT) / SLICES_COUNT;
// float	phi = 0.0 * PI;
// phi += float(rayIndex) / RAYS_COUNT;

	float2	scPhi;
	sincos( phi, scPhi.y, scPhi.x );

	///////////////////////////////////////////////////////////////////
	// Sample heights along the ray and keep largest tangent
	float2	pixel2UV = 1.0 / float2( _Width, _Height );
	float2	UV = pixel2UV * float2( 0.5 + PixelPosition );
	float2	dUV = scPhi * pixel2UV;
			dUV.y = -dUV.y;	// Y axis points to the top of the image

	float	maxTanAngle = 0.0;
	float	radius = 0.0;

// 	[allow_uav_condition]
// 	[loop]
	for ( uint i=0; i < _DiscRadius; i++ ) {
		UV += dUV;
		radius += _TexelSize_mm;

		// Compute the tangent to the horizon
		float	H = _Thickness_mm * _SourceThickness.SampleLevel( LinearClamp, UV, 0 ).x;
		float	tanAngle = (H-H0) / radius;

		maxTanAngle = max( maxTanAngle, tanAngle );
	}
	gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex] = atan( maxTanAngle );

	GroupMemoryBarrierWithGroupSync();

	///////////////////////////////////////////////////////////////////
	// Average angles
	if ( rayIndex < 64 && RAYS_COUNT > 64 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+64];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 32 && RAYS_COUNT > 32 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+32];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 16 && RAYS_COUNT > 16 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+16];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 8 && RAYS_COUNT > 4 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+8];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 4 && RAYS_COUNT > 4 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+4];
	}

	GroupMemoryBarrierWithGroupSync();

	///////////////////////////////////////////////////////////////////
	// Finalize
	if ( rayIndex == 0 ) {
		float	averageAngle = (gs_MaximumAngle[RAYS_COUNT*sliceIndex+0] + gs_MaximumAngle[RAYS_COUNT*sliceIndex+1] + gs_MaximumAngle[RAYS_COUNT*sliceIndex+2] + gs_MaximumAngle[RAYS_COUNT*sliceIndex+3]) / RAYS_COUNT;
		float	sinAverageAngle = sin( averageAngle );
		_Target[uint3( PixelPosition, sliceIndex )] = sinAverageAngle;	// We need the angle from the vertical axis! so we need cos(PI/2-averageAngle) = sin(averageAngle)...
//_Target[uint3( PixelPosition, sliceIndex )] = 1.0;
//_Target[uint3( PixelPosition, sliceIndex )] = 1.0 / sliceIndex;
	}
}