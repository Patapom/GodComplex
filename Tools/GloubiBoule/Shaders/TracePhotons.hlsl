#include "Includes/global.hlsl"
#include "Includes/Photons.hlsl"
#include "Includes/Room.hlsl"
#include "Includes/Noise.hlsl"

#define	NUM_THREADSX	256
#define	NUM_THREADSY	1
#define	NUM_THREADSZ	1

cbuffer CB_InitPhotons : register(b2) {
};

StructuredBuffer< PhotonInfo_t >	_Buf_PhotonsInfoIn : register(t0);
RWStructuredBuffer< PhotonInfo_t >	_Buf_PhotonsInfoOut : register(u0);

StructuredBuffer< Photon_t >		_Buf_PhotonsIn : register(t1);
RWStructuredBuffer< Photon_t >		_Buf_PhotonsOut : register(u1);

RWTexture3D< uint >					_Tex_PhotonAccumulator : register(u2);


///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
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
phi += 0.5 * sin( 4.0 * _Time + 0.001 * PhotonIndex );


	float2	scPhi, scTheta;
	sincos( phi, scPhi.x, scPhi.y );
	sincos( theta, scTheta.x, scTheta.y );

	float3	wsDir = float3( scTheta.x * scPhi.y, scTheta.x * scPhi.x, scTheta.y );

	float	dOmega = FOURPI / PHOTONS_COUNT;	// Thanks to distribution, each photon has the same weight

	PhotonInfo_t	Info;
	Info.wsStartPosition = 0.0;
	Info.wsDirection = wsDir;
	Info.RadiusDivergence = 0.0;

	Photon_t	P;
	P.wsPosition = 0.0;
	P.Radius = 1.0;

	_Buf_PhotonsInfoOut[_DispatchThreadID.x] = Info;
	_Buf_PhotonsOut[_DispatchThreadID.x] = P;
}

[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
void	CS_TracePhotons( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint			PhotonIndex = _DispatchThreadID.x;

	PhotonInfo_t	Info = _Buf_PhotonsInfoIn[PhotonIndex];
	Photon_t		P = _Buf_PhotonsIn[PhotonIndex];

	// Iterate
	float3	wsStep = 0.1 * Info.wsDirection;
	for ( uint stepIndex=0; stepIndex < 64; stepIndex++ ) {
		// Update the photon
		P.wsPosition += wsStep;
		P.Radius += Info.RadiusDivergence;
		
		// Splat energy
		uint3	cellIndex = World2RoomCellIndex( P.wsPosition );
		uint	value = uint( 65536.0 * Radius2Energy( P.Radius ) );

//value = 10000;

		uint	oldValue;
		InterlockedAdd( _Tex_PhotonAccumulator[cellIndex], value, oldValue );
	}

//	_Buf_PhotonsOut[_DispatchThreadID.x] = P;
}


//[numthreads( NUM_THREADSX, NUM_THREADSY, NUM_THREADSZ )]
//void	CS_SplatPhotons( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
//	Photon_t	P = _Buf_PhotonsIn[_DispatchThreadID.x];
//
//	uint3		cellIndex = World2RoomCellIndex( P.wsPosition );
//	uint		oldValue;
//	InterlockedAdd( _Buf_PhotonAccumulator[cellIndex], value, oldValue );
//}

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
