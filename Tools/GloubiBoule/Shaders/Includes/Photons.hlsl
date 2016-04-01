///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Defines the Photon structures (infos and traced photons)
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef _PHOTONS_INCLUDED
#define _PHOTONS_INCLUDED

static const uint	PHOTONS_COUNT = 128*1024;

struct PhotonInfo_t {
	float3	wsStartPosition;
	float3	wsDirection;
	float	RadiusDivergence;
};

struct Photon_t {
	float3	wsPosition;
	float	Radius;
};

float	Radius2Energy( float _radius ) {
	return 1.0 / _radius;
}

#endif