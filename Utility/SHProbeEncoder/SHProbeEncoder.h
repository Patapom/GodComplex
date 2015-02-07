//////////////////////////////////////////////////////////////////////////
// SH Probe Encoder
//
// Encodes a cube map containing albedo, normal, distance, material ID, static lighting and emissive material
//
// 
//
//
#pragma once


class	SHProbeEncoder
{
private:	// NESTED TYPES

private:	// FIELDS


public:		// PROPERTIES

public:		// METHODS

	SHProbeEncoder();
	~SHProbeEncoder();

	// Encodes the MRT cube map into basic SH elements that can later be combined at runtime to form a dynamically updatable probe
	void	EncodeProbeCubeMap( Texture2D& _StagingCubeMap );
};