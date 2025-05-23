////////////////////////////////////////////////////////////////////////////////
// Describes the various photon structures used for shooting and splatting
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"
#include "../Noise.hlsl"

struct Photon
{
	float2	Position;				// Position on the layer
	uint	Direction;				// Packed Phi, Theta on 2 bytes each
	uint	RGBE;					// RGBE-encoded color
						// TODO: add stats like scattering events count & marched length?
#ifdef DEBUG
	float4	Infos;
#endif
};	// Total size: 16 bytes

RWStructuredBuffer<Photon>	_Photons : register( u0 );						// Contains the photons to shoot through layers

RWStructuredBuffer<uint>	_PhotonLayerIndices : register( u1 );			// Index of the layer each photon is standing on, most significant bit is set to indicate direction (1 is bottom to top, is top to bottom)
RWStructuredBuffer<uint>	_ProcessedPhotonsCounter : register( u2 );		// Counter of process photon reset to 0 for each shooting and incremented every time a photon is processed


////////////////////////////////////////////////////////////////////////////////
// Photon helper
struct PhotonUnpacked
{
	float2	Position;
	float3	Direction;
	float3	Color;
#ifdef DEBUG
	float4	Infos;
#endif
};

void	UnPackPhoton( in Photon _In, out PhotonUnpacked _Out )
{
	_Out.Position = _In.Position;

	// Unpack direction & layer index
	float	Phi = TWOPI * (_In.Direction & 0xFFFF) / 65535.0 - PI;
	float	Theta = PI * ((_In.Direction >> 16) & 0xFFFF) / 65535.0f;

	float2	SCTheta, SCPhi;
	sincos( Theta, SCTheta.x, SCTheta.y );
	sincos( Phi, SCPhi.x, SCPhi.y );
	_Out.Direction = float3( SCTheta.x * SCPhi.x, SCTheta.y, SCTheta.x * SCPhi.y );

	// Unpack color
	_Out.Color = float3( (_In.RGBE & 0xFF) / 255.0, ((_In.RGBE >> 8) & 0xFF) / 255.0, ((_In.RGBE >> 16) & 0xFF) / 255.0 );
	int		nExponent = int((_In.RGBE >> 24) & 0xFF);
	float	Exponent = exp2( nExponent - 128 );
	_Out.Color *= Exponent;

#ifdef DEBUG
	_Out.Infos = _In.Infos;
#endif
}

void	PackPhoton( in PhotonUnpacked _In, out Photon _Out )
{
	_Out.Position = _In.Position;

	// Pack direction & layer index
	uint	Phi = uint( 65535 * saturate( (PI + atan2( _In.Direction.x, _In.Direction.z )) / TWOPI ) );
	uint	Theta = uint( 65535 * saturate( acos( _In.Direction.y ) * INVPI ) );
	_Out.Direction = Phi | (Theta << 16);

	// Pack color
	float	Max = max( max( _In.Color.x, _In.Color.y ), _In.Color.z );
	float	Exponent = ceil( log2( Max ) );
	float3	Temp = _In.Color * exp2( -Exponent );
	uint	R = uint( clamp( 255 * Temp.x, 0, 255 ) );
	uint	G = uint( clamp( 255 * Temp.y, 0, 255 ) );
	uint	B = uint( clamp( 255 * Temp.z, 0, 255 ) );
	uint	E = uint( clamp( Exponent + 128, 0, 255 ) );
	_Out.RGBE = R | ((G | ((B | (E << 8)) << 8)) << 8);

#ifdef DEBUG
	_Out.Infos = _In.Infos;
#endif
}


////////////////////////////////////////////////////////////////////////////////
// Scattering helpers
//
StructuredBuffer<float4>	_Random : register( t0 );						// A large table of random numbers, more efficient and random than our analytical noise function
StructuredBuffer<float>		_PhaseQuantiles : register( t1 );				// Phase function in the form of quantiles.
																			// _ The first 65536 entries give a random scattering angle from the peak of the phase function
																			//		that represents ~99% of the scattering probability
																			// _ The next 65536 entries give a random scattering angle from the rest of the phase function
																			// It's used by drawing a random number in [0,1[ and scaling it to fetch the table that returns the new scattering angle


#define USE_RANDOM_TABLE	4 * 1024 * 1024									// Define this to use the random table of float4 instead of generating pseudo random numbers on the fly

static const uint	QUANTILES_COUNT = 65536;								// _PhaseQuantiles table is actually twice that big
static const float	QUANTILE_PEAK_RATIO = 0.98757544366683103837778075954578;
static const float	QUANTILE_OFFPEAK_RATIO = 1.0-QUANTILE_PEAK_RATIO;		// = 0.01242455633316896162221924045422;

float	GetRandomPhase( float _Random )
{
	uint	PhaseIndex;
	if ( _Random < QUANTILE_PEAK_RATIO )
		PhaseIndex = uint( QUANTILES_COUNT * _Random * (1.0 / QUANTILE_PEAK_RATIO) );							// Draw from peak
	else
		PhaseIndex = QUANTILES_COUNT + uint( (_Random-QUANTILE_PEAK_RATIO) * (1.0 / QUANTILE_OFFPEAK_RATIO) );	// Draw from off-peak

	return _PhaseQuantiles[PhaseIndex];
}

// Changes the photon direction and returns the length of the path before next scattering event
float3	Scatter( float3 _OriginalDirection, float4 _Random )
{
	// Draw a random orthogonal vector to current direction
#if 0

// #if USE_RANDOM_TABLE
// 	float2	SinCosPhi, SinCosTheta;
// 	sincos( TWOPI * _Random.x, SinCosPhi.x, SinCosPhi.y );
// 	sincos( PI * _Random.y, SinCosTheta.x, SinCosTheta.y );
// //	float3	RandomVector = normalize( 2.0 * _Random.xyz - 1.0 );
// #else
// 	float2	SinCosPhi, SinCosTheta;
// 	sincos( 132.378 * _Random.x, SinCosPhi.x, SinCosPhi.y );
// 	sincos( PI * _Random.y, SinCosTheta.x, SinCosTheta.y );
// #endif
// 	float3	RandomVector = float3( SinCosPhi.y * SinCosTheta.x, SinCosTheta.y, SinCosPhi.x * SinCosTheta.x );
// 	float3	Ortho = normalize( cross( RandomVector, _OriginalDirection ) );

#else

	float3	Y = normalize( cross( float3( 1, 0, 1 ), _OriginalDirection ) );	// First vector of the plane orthogonal to our original vector
	float3	X = cross( _OriginalDirection, Y );									// Second vector of the plane

	float2	SCPhi;
	sincos( TWOPI * _Random.x, SCPhi.x, SCPhi.y );
	float3	Ortho = SCPhi.y * X + SCPhi.x * Y;									// Random vector in a disc lying on the orthogonal plane of our original vector

#endif
 
	// Rotate current direction about that vector by the scattering angle
	float	ScatteringAngle = GetRandomPhase( _Random.y );

// ScatteringAngle = 0.01;

	return RotateVector( _OriginalDirection, Ortho, ScatteringAngle );
}

//	_PhotonIndex, the index of the photon we're scattering
//	_ScatteringEventIndex, the index of the scattering event
//	_MaxScattering, the maximum amount of tolerated scattering events
//	_OriginalDirection, the current direction of the photon
// Returns the new, scattered photon direction
//
float3	Scatter( float3 _OriginalDirection, uint _PhotonIndex, uint _ScatteringEventIndex, uint _MaxScattering )
{
#if USE_RANDOM_TABLE
// 	float	TempRandom = Hash( 0.3718198 * (_MaxScattering * (0.37891+_PhotonIndex) + _ScatteringEventIndex) );
//	float4	Random = _Random[uint( TempRandom * USE_RANDOM_TABLE) & (USE_RANDOM_TABLE-1)];
	float4	Random = _Random[uint(0.37138198 * (_MaxScattering * _PhotonIndex + _ScatteringEventIndex)) & (USE_RANDOM_TABLE-1)];
#else
	float4	Random = 0.0;
#endif

	// Add analytical random to increase randomness even more
	Random += float4(	Hash( 0.0003718198 * (_MaxScattering * (0.37918+_PhotonIndex) + 0.3879 * _ScatteringEventIndex) ),
						Hash( 0.0007594813 * (_MaxScattering * (0.37189+_PhotonIndex) + 0.5637 * _ScatteringEventIndex) ),
						Hash( 0.0005984763 * (_MaxScattering * (0.37918+_PhotonIndex) + 0.7355 * _ScatteringEventIndex) ),
						Hash( 0.0013984763 * (_MaxScattering * (0.38917+_PhotonIndex) + 0.1234 * _ScatteringEventIndex) )
					);
	Random = frac( Random );

	return Scatter( _OriginalDirection, Random );
}

