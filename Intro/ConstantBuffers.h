//////////////////////////////////////////////////////////////////////////
// Definition of all the constant buffers used throughout the intro
//
#pragma once

struct CBGlobal 
{
	float4	Time;	// X=Time Y=DeltaTime ZW = 1/XY
};

struct CBTest
{
	float	LOD;		// Animated texture LOD
	float	BackLight;	// 1 to enable backlighting
};
