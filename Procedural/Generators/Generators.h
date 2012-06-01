//////////////////////////////////////////////////////////////////////////
// Generator algorithms
//
#pragma once

class	Generators
{
public:		// METHODS

	// Computes the ambient occlusion from a source texture's luminance level considered a height field
	static void ComputeAO( const TextureBuilder& _Source, TextureBuilder& _Target, float _HeightFactor=1.0f, int _DirectionsCount=8, int _SamplesCount=8, bool _bWriteOnlyAlpha=false );

	// Fills a texture with dirtyness/moss/mouldiness leaking from the top of the texture
	//	_InitialIntensity, intensity for initialization
	//	_AverageIntensity, the average intensity of dirtyness
	//	_DirtNoiseFrequency, frequency of noise for dirt stains
	//	_DirtAmplitude, amplitude of variation for the dirt stains
	//	_PullBackForce, the force with which any too bright dirt spots are pulled back toward the average intensity
	static void	Dirtyness( TextureBuilder& _Builder, const Noise& _Noise, float _InitialIntensity=1.0f, float _AverageIntensity=0.0f, float _DirtNoiseFrequency=0.1f, float _DirtAmplitude=0.1f, float _PullBackForce=0.01f );

	// Generates a "marble" texture (courtezy of Pierre Terdiman a.k.a. Zappy, thanks to him for digging up that old routine !)
	static void	Marble( TextureBuilder& _Builder, int _BootSize=30 );
};
