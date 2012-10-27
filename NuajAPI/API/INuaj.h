#pragma once

#define USE_CELLULAR_NOISE


//////////////////////////////////////////////////////////////////////////
#include "Types.h"
#include "NuajParameters.h"
#include "IEngine.h"
#include "Resources.h"

class INuaj;
class INuajSkyProbe;

//////////////////////////////////////////////////////////////////////////
// Creates the Nuaj' instance
extern INuaj*	CreateNuajInstance( NjIEngine& _Engine, const NuajConfiguration& _Configuration, const NuajQuality& _Quality );

enum NjSKY_PROBE_TYPE
{
	NJ_SKY_PROBE_SKY_ONLY,				// Render the sky without any cloud (used to light the high altitude clouds)
	NJ_SKY_PROBE_HIGH_CLOUDS_ONLY,		// Render only the high altitude sky and clouds (used to light the low altitude clouds)
	NJ_SKY_PROBE_FULL_CLOUDS,			// Render the entire sky with all clouds (used to light the ground objects)
};

//////////////////////////////////////////////////////////////////////////
// Main Nuaj interface (given to you !)
//
class INuaj
{
public: // PROPERTIES

	// Gets the directional color of the Sun (valid only after a call to ComputeSceneLights)
	virtual const NjFloat3&		GetSunColor() const = 0;

	// Gets the ambient color of the Sky (valid only after a call to ComputeSceneLights)
	virtual const NjFloat3&		GetSkyColor() const = 0;

	// Gets the ambient color of the Sky via 4 SH vectors (valid only after a call to ComputeSceneLights)
	virtual const NjFloat3*		GetSkyColorSH() const = 0;

	// Gets the default sky probe internally created by Nuaj' for its own rendering purpose
	virtual INuajSkyProbe&		GetDefaultSkyProbe() = 0;

	// Gets the sky environment map used for reflections
	virtual const NjITexture&	GetSkyEnvironmentMap() const = 0;

	// Gets or sets the 3D noise texture used by Nuaj'
	virtual const NjITexture&	GetNoise3D() const = 0;
	virtual void				SetNoise3D( NjITexture& value ) = 0;

	// Gets the current sky parameters for rendering
	virtual const NuajConfiguration&	GetConfiguration() const = 0;
	virtual const NuajQuality&			GetQuality() const = 0;
	virtual const NuajSkyParameters&	GetParameters() const = 0;


public: // METHODS

	// Update Nuaj' quality settings
	// NOTE: Implies destroying and recreating some textures & geometries (i.e. not to be called every frame !)
	virtual void				UpdateQuality( const NuajQuality& _Quality ) = 0;

	// Update sky & clouds runtime parameters
	virtual void				UpdateParameters( const NuajSkyParameters& _Parameters ) = 0;

	// Begins rendering
	virtual NjErrorID			BeginFrame( const NuajRenderParameters& _Parameters ) = 0;

	// Computes & updates the directional Sun & ambient Sky lights
	// NOTE (for the Sun color) : this routine only accounts for Sun attenuation from the atmosphere and doesn't take clouds into account !
	//  You want to use the Sun color for your directional light and modulate the direct light with the usual shadow mapping algorithms that
	//  will properly include shadowing by the clouds.
	virtual NjErrorID			ComputeSceneLights() = 0;

	// Renders the shadow maps, environment map and ambient sky probes
	virtual NjErrorID			PreRender() = 0;

	// Renders the clouds, the sky and the ambient sky probes
	virtual NjErrorID			RenderSky() = 0;

	// Combines the computed sky with the HDR buffer
	virtual NjErrorID			CombineSky() = 0;

	// Ends rendering, releases resources
	virtual NjErrorID			EndFrame() = 0;


public: // HELPERS

	// Creates an ambient sky probe
	// NOTE: If "_CPUReadBack" is set to false then GetAmbientSH() will return black !
	// NOTE: Setting this to true will imply a locking of the ambient SH render target ! (it's a 1x1x3 render target array)
	virtual INuajSkyProbe&		CreateSkyProbe( NjSKY_PROBE_TYPE _Type, bool _CPUReadBack ) = 0;

	// Provides the atmosphere support to the specified material
	virtual void				QueryAtmosphereSupport( NjIShader& _Material ) = 0;
};


//////////////////////////////////////////////////////////////////////////
// Nuaj sky probe interface
//
class INuajSkyProbe
{
public: // PROPERTIES

	// Gets or sets the probe's position in WORLD space
	virtual const NjFloat3&		GetPosition() const = 0;
	virtual void				SetPosition( const NjFloat3& value ) = 0;

	// Reads back the ambient sky color in the form of 4 SH coefficients
	// NOTE: Returns black if initialized with _CPUReadBack = false !
	virtual const NjFloat3*		GetAmbientSH() const = 0;


public: // METHODS

	// Provides the atmosphere support to the specified material
	virtual void				QueryAmbientSkySupport( NjIShader& _Material ) = 0;
};
