////////////////////////////////////////////////////////////////////////////////
// Visibility Map generator
// This compute shader will generate a 3D texture based containing horizon angles
//	for a limited amount of directions, each direction is stored in a separate slice.
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

static const uint	SLICES_COUNT = 16;		// Amount of azimutal slices
static const uint	RAYS_COUNT = 64;		// Rays count per slice

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
	float2	scPhi;
	sincos( phi, scPhi.x, scPhi.y );

	///////////////////////////////////////////////////////////////////
	// Sample heights along the ray and keep largest tangent
	float2	pixel2UV = 1.0 / float2( _Width, _Height );
	float2	UV = pixel2UV * float2( 0.5 + PixelPosition );
	float2	dUV = scPhi * pixel2UV;

	float	maxTanAngle = 0.0;
	float	radius = 0.0;
	for ( uint i=0; i < _DiscRadius; i++ ) {
		UV += dUV;
		radius += _TexelSize_mm;

		// Compute the tangent to the horizon
		float	H = _Thickness_mm * _SourceThickness.SampleLevel( LinearClamp, UV, 0 ).x;
		float	tanAngle = (H-H0) / radius;

		maxTanAngle = max( maxTanAngle, tanAngle );
	}
	gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex] = atan( maxTanAngle );

// 	[allow_uav_condition]
// 	[loop]

	GroupMemoryBarrierWithGroupSync();

	///////////////////////////////////////////////////////////////////
	// Average angles
	if ( rayIndex < 32 && RAYS_COUNT > 32 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+32];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 16 && RAYS_COUNT > 16 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+16];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 8 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+8];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < 4 ) {
		gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+0] += gs_MaximumAngle[RAYS_COUNT*sliceIndex+rayIndex+4];
	}

	GroupMemoryBarrierWithGroupSync();

	///////////////////////////////////////////////////////////////////
	// Finalize
	if ( rayIndex == 0 ) {
		float	averageAngle = (gs_MaximumAngle[RAYS_COUNT*sliceIndex+0] + gs_MaximumAngle[RAYS_COUNT*sliceIndex+1] + gs_MaximumAngle[RAYS_COUNT*sliceIndex+2] + gs_MaximumAngle[RAYS_COUNT*sliceIndex+3]) / RAYS_COUNT;
		float	cosAverageAngle = cos( averageAngle );
		_Target[uint3( PixelPosition, sliceIndex )] = cosAverageAngle;
	}
}