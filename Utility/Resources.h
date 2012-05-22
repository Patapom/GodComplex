//////////////////////////////////////////////////////////////////////////
// Resource Loading Helpers
//
#pragma once

// Loads a binary resource in memory
//	_pResourceType, type of binary resource to load
//	_pResourceSize, an optional pointer to an int that will contain the size of the loaded resource
const U8*	LoadResourceBinary( U16 _ResourceID, const char* _pResourceType, U32* _pResourceSize=NULL );

// Loads a text shader resource in memory
// IMPORTANT NOTE: You MUST destroy the returned pointed once you're done with it !
char*	LoadResourceShader( U16 _ResourceID );
