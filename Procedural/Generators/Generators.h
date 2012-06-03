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
	//
	static void	Dirtyness( TextureBuilder& _Builder, const Noise& _Noise, float _InitialIntensity=1.0f, float _AverageIntensity=0.0f, float _DirtNoiseFrequency=0.1f, float _DirtAmplitude=0.1f, float _PullBackForce=0.01f );

	// Generates a "marble" texture (courtezy of Pierre Terdiman a.k.a. Zappy, thanks to him for digging up that old routine !)
	//	_BootSize, size of the boot zone that is used to initialize the texture. We skip the first lines as they are sometimes ugly
	//	_NoiseAmplitude, amplitude of the random offset added each line
	//	_WeightX, the weights for the [Y-1,X-1], [Y-1,X] & [Y-1,X+1] pixels respectively
	//	_WeightsNormalizer, the normalizing factor for the sum of pixels
	//
	static void	Marble( TextureBuilder& _Builder, int _BootSize=30, float _NoiseAmplitude=0.2f, float _Weight0=0.0f, float _Weight1=1.0f, float _Weight2=1.0f, float _WeightsNormalizer=0.5f );

// Doesn't work, slow, bad idea
//	static void	Marble3D( TextureBuilder& _Builder, int _IterationsCount=16, float _NoiseAmplitude=0.2f );
};
