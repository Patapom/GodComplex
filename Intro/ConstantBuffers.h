//////////////////////////////////////////////////////////////////////////
// Definition of all the constant buffers used throughout the intro
//
#pragma once

struct CBTest
{
	float	LOD;	// Animated texture LOD
};

struct CBObject
{
	NjFloat4x4	Local2World;	// Local=>World transform to rotate the object
};
