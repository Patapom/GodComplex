#include "Includes/global.hlsl"
#include "Includes/Photons.hlsl"
#include "Includes/Room.hlsl"
#include "Includes/VolumeDensity.hlsl"

#define	NUM_THREADSX	256
#define	NUM_THREADSY	1
#define	NUM_THREADSZ	1

cbuffer CB_TracePhotons : register(b2) {
	float	_Sigma_t;
};

StructuredBuffer< PhotonInfo_t >	_Buf_PhotonsInfoIn : register(t0);
RWStructuredBuffer< PhotonInfo_t >	_Buf_PhotonsInfoOut : register(u0);

StructuredBuffer< Photon_t >		_Buf_PhotonsIn : register(t1);
RWStructuredBuffer< Photon_t >		_Buf_PhotonsOut : register(u1);

RWTexture3D< uint >					_Tex_PhotonAccumulator : register(u2);


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Initializes the photons
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
void	CS_InitPhotons( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	PhotonIndex = _DispatchThreadID.x;

	// Generate position on sphere
	float	e1 = float(PhotonIndex) / PHOTONS_COUNT;
	float	e2 = ReverseBits( PhotonIndex );

	float	phi = TWOPI * e1;
	float	theta = 2.0 * acos( sqrt( 1.0 - e2 ) );


//theta += 0.1 * sin( 0.37 * _Time + PhotonIndex );
//phi += 0.5 * sin( 4.0 * _Time + 0.001 * PhotonIndex );


	float2	scPhi, scTheta;
	sincos( phi, scPhi.x, scPhi.y );
	sincos( theta, scTheta.x, scTheta.y );

	float3	wsDir = float3( scTheta.x * scPhi.y, scTheta.x * scPhi.x, scTheta.y );

	float	dOmega = FOURPI / PHOTONS_COUNT;	// Thanks to distribution, each photon has the same weight

	PhotonInfo_t	Info;
	Info.wsStartPosition = 0.0;
	Info.wsDirection = wsDir;
	Info.RadiusDivergence = 0.0;

//	Photon_t	P;
//	P.wsPosition = 0.0;
//	P.Radius = 1.0;

	_Buf_PhotonsInfoOut[_DispatchThreadID.x] = Info;
//	_Buf_PhotonsOut[_DispatchThreadID.x] = P;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Traces the photons
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
void	CS_TracePhotons( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint			PhotonIndex = _DispatchThreadID.x;

	PhotonInfo_t	Info = _Buf_PhotonsInfoIn[PhotonIndex];
#if 0
	Photon_t		P = _Buf_PhotonsIn[PhotonIndex];
#else
	Photon_t		P;

	// Generate position on sphere
	float	e1 = float(PhotonIndex) / PHOTONS_COUNT;
	float	e2 = ReverseBits( PhotonIndex );

	float	phi = TWOPI * e1;
	float	theta = 2.0 * acos( sqrt( 1.0 - e2 ) );


//theta += 0.1 * sin( 0.37 * _Time + PhotonIndex );
//phi += 0.5 * sin( 4.0 * _Time + 0.001 * PhotonIndex );


	float2	scPhi, scTheta;
	sincos( phi, scPhi.x, scPhi.y );
	sincos( theta, scTheta.x, scTheta.y );

	float3	wsDir = float3( scTheta.x * scPhi.y, scTheta.x * scPhi.x, scTheta.y );

	float	dOmega = FOURPI / PHOTONS_COUNT;	// Thanks to distribution, each photon has the same weight

#endif

	// Trace, attenuate and accumulate
	float3	wsPos = P.wsPosition;
	float	stepSize = 0.1;
	float3	wsStep = stepSize * Info.wsDirection;

	float	energy = 1.0;	// Initial energy
	for ( uint stepIndex=0; stepIndex < 64; stepIndex++ ) {
		// Update the photon
		P.wsPosition += wsStep;
		P.Radius += Info.RadiusDivergence;

		// Compute extinction based on volume density
		float	density = SampleVolumeDensity( P.wsPosition );
		float	stepExtinction = exp( -_Sigma_t * density * stepSize );
//		energy *= stepExtinction;
		energy *= 0.01;

		// Splat energy
		uint3	cellIndex = World2RoomCellIndex( P.wsPosition );
		uint	value = uint( 65536.0 * energy * Radius2Energy( P.Radius ) );

//value = 10000;

		uint	oldValue;
		InterlockedAdd( _Tex_PhotonAccumulator[cellIndex], value, oldValue );
	}

//	_Buf_PhotonsOut[_DispatchThreadID.x] = P;
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 4, 4, 4 )]
void	CS_ClearAccumulator( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	_Tex_PhotonAccumulator[_DispatchThreadID.xyz] = 0;
}


[numthreads( 4, 4, 4 )]
void	CS_FadeAccumulator( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	currentValue = _Tex_PhotonAccumulator[_DispatchThreadID.xyz];
	currentValue *= 0.99;
	_Tex_PhotonAccumulator[_DispatchThreadID.xyz] = currentValue;
}
