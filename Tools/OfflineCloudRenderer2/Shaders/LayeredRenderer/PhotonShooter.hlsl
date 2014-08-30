////////////////////////////////////////////////////////////////////////////////
// Photon Shooter
////////////////////////////////////////////////////////////////////////////////
//
// The goal of this compute shader is to shoot an important amount of photons from one side of a flat layer
// Cloud landscape is composed of a large amount of layers that will represent the entire landscape up to several kilometers in altitude
// 
// Photons will initially start from the top layer (assuming the Sun is above the clouds) and will be fired through each successive layer
//	from top to bottom.
// A "top" bucket of remaining photons is initially filled with indices of photons still in the race for shooting top to bottom, while another
//	"bottom" bucket (initially empty) will progressively fill up to indicate photons that came back up to the original top side of the layer.
// 
// When we reach the bottom layer, we start the process again using the bottom bucket and starting from the bottom layer up to the top layer.
// This time, it's the "top" bucket that fills up with photons bouncing back to the bottom of each layer.
// 
// The process is repeated until all photons have either escaped through the top or the bottom layer. Practice will tell us how many bounces
//	are actually needed to obtain a correct lighting for the entire cloudscape.
// 
//	---------------------------
//
// Every time a batch of photons is shot through between 2 layers, the photons that reached the bottom layer are splat to a 2D texture array
//	(as many array slices as layers)
//
// This texture 2D array is then used, with interpolation, to determine the lighting at a particular 3D position within the cloudscape.
//
#include "../Global.hlsl"
#include "PhotonStructures.hlsl"

static const uint	MAX_THREADS = 1024;

static const float	INITIAL_START_EPSILON = 1e-4;

cbuffer	CBInput : register( b8 )
{
	uint	_LayerIndex;			// Index of the layer we start shooting photons from, most significant bit is set to indicate direction (1 is bottom to top, is top to bottom)
	uint	_LayersCount;			// Total amount of layers
	float	_LayerThickness;		// Thickness of each individual layer (in meters)
	float	_SigmaScattering;		// Scattering coefficient (in m^-1)

	float3	_CloudScapeSize;		// Size of the cloud scape covered by the 3D texture of densities
	uint	_MaxScattering;			// Maximum scattering events before discarding the photon

	uint	_BatchIndex;			// Photon batch index
}

Texture3D<float>	_TexDensity : register( t2 );	// 3D noise density texture covering the entire cloudscape

// Samples the cloud density given a world space position within the cloud scape
float	SampleDensity( float3 _Position )
{
	float3	TopCorner = float3( 0, 0, 0 ) + float3( -0.5 * _CloudScapeSize.x, _CloudScapeSize.y, -0.5 * _CloudScapeSize.z );
	float3	BottomCorner = float3( 0, 0, 0 ) + float3( 0.5 * _CloudScapeSize.x, 0.0, 0.5 * _CloudScapeSize.z );
	float3	UVW = (_Position - TopCorner) / (BottomCorner - TopCorner);
	return _TexDensity.SampleLevel( LinearClamp, UVW, 0.0 ).x;
}

void	ShootPhoton( uint _PhotonIndex )
{
	uint	LayerIndex = _PhotonLayerIndices[_PhotonIndex];
	if ( LayerIndex != _LayerIndex )
		return;	// Photon is not concerned

	Photon	Pp = _Photons[_PhotonIndex];
	PhotonUnpacked	P;
	UnPackPhoton( Pp, P );

	float3	Position = float3( P.Position.x, (_LayersCount - _LayerIndex) * _LayerThickness, P.Position.y );

	// Prepare marching through the layer
	float	MarchedLength = 0.0;
	float	OpticalDepth = 0.0;
	uint	ScatteringEventsCount = 0;

	float	StepSize = _LayerThickness / 64.0;	// Arbitrary! Parameter!
	uint	StepsCount = 0;

	float	LayerTopAltitude = (_LayersCount-_LayerIndex) * _LayerThickness;
	float	LayerBottomAltitude = LayerTopAltitude - _LayerThickness;

	[allow_uav_condition]
	[loop]
	while ( ScatteringEventsCount < _MaxScattering )
	{
		// March one step
		Position += StepSize * P.Direction;

		if ( Position.y > LayerTopAltitude )
		{	// Exited through top of layer, compute exact intersection
			float	t = (LayerTopAltitude - Position.y) / (StepSize * P.Direction.y);
			Position += t * StepSize * P.Direction;
			LayerIndex |= 0x80000000U;	// Change of direction => now going up
			break;
		}
		else if ( Position.y < LayerBottomAltitude )
		{	// Exited through bottom of layer, compute exact intersection
			float	t = (LayerBottomAltitude - Position.y) / (StepSize * P.Direction.y);
			Position += t * StepSize * P.Direction;
			LayerIndex++;	// Passed through the layer
			break;
		}

		// Sample density
		float	Density = SampleDensity( Position );
		OpticalDepth += Density * _SigmaScattering;

		// Draw random number and check if we should be scattered
#if USE_RANDOM_TABLE
		float	Random = _Random[uint(-0.17138198 * (_MaxScattering * _PhotonIndex + StepsCount)) & (USE_RANDOM_TABLE-1)].x;
#else
		float	Random = Hash( -0.01718198 * (_MaxScattering * (0.27891+_PhotonIndex) + StepsCount) );
#endif

		if ( Random > exp( -OpticalDepth * StepSize ) )
		{	// We must scatter!
			P.Direction = Scatter( _PhotonIndex, ScatteringEventsCount++, _MaxScattering, P.Direction );
//			OpticalDepth = 0.0;				// Reset optical depth for next scattering event
			OpticalDepth += log( Random );	// Reset optical depth for next scattering event
		}

		StepsCount++;
	}

	if ( ScatteringEventsCount >= _MaxScattering )
		return;	// Too many events, discard photons...

	// Write resulting photon
	P.Position = Position.xz;


// Test direction change
//P.Direction = float3( 1, 0, 0 );

// Test position change
//P.Position += float2( 100.0, 50.0 );

// Test color change
//P.Color *= float3( 1.8, 0.6, 0.4 );

	PackPhoton( P, Pp );
	_Photons[_PhotonIndex] = Pp;

	// Update layer index
	_PhotonLayerIndices[_PhotonIndex] = LayerIndex;

	// Increase processed photons count
	InterlockedAdd( _ProcessedPhotonsCounter[0], 1U );
}

[numthreads( MAX_THREADS, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
//	uint	PhotonIndex = MAX_THREADS * _GroupID.x + _GroupThreadID.x;
	uint	PhotonIndex = MAX_THREADS * _BatchIndex + _GroupThreadID.x;
	ShootPhoton( PhotonIndex );
}