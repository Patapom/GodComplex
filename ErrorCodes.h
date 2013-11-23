//////////////////////////////////////////////////////////////////////////
// Contains the error codes returned by the Windows/Intro init
// Those codes are returned by the GodComplex.exe on exit so you can match them with this file and see what happened wrong...
//
#pragma once

// Window creation
#define	ERR_REGISTER_CLASS				-1		// Failed to register window class
#define	ERR_CHANGE_DISPLAY_SETTINGS		-2		// Failed to change display settings
#define	ERR_CREATE_WINDOW				-3		// Failed to create the window
#define	ERR_RETRIEVE_DC					-4		// Failed to retrieve a valid device context for GDI calls

// DirectX Device initialization
#define	ERR_DX_INIT_DEVICE				-10		// Failed to initialize DirectX device

// Music initialization
#define	ERR_MUSIC_RESOURCE_NOT_FOUND	-20		// Failed to retrieve the music binary resource
#define	ERR_MUSIC_INIT					-21		// Failed to open the music binary with V2


//////////////////////////////////////////////////////////////////////////
// Effects base error codes
// Error codes for effects should increase in thousands like 2000, 3000, 4000, etc.
// Any error code with these base numbers belong to the effect (e.g. ErrorCode #2002 means an error in the 2nd material of the Translucency effect)
//
// NOTE: These error codes will actually be NEGATED when returned by the executable so you'll get -1001, -2003, etc.
//
#define ERR_EFFECT_INTRO				1000	// The main intro program
#define ERR_EFFECT_TRANSLUCENCY			2000
#define ERR_EFFECT_ROOM					3000
#define ERR_EFFECT_SCENE				4000
#define ERR_EFFECT_VOLUMETRIC			5000
#define ERR_EFFECT_GLOBALILLUM			6000


// Code Workshop
#define ERR_EFFECT_PARTICLES			2000
#define ERR_EFFECT_DEFERRED				3000
