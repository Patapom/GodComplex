#pragma once

#define CODE_WORKSHOP	// Pour Flure et 'lrik

#define NOISE3D_SIZE	32
#define NOISE3D_SHIFT	5

extern Primitive*	gs_pPrimQuad;		// Screen quad for post-processes
extern Texture3D*	gs_pTexNoise3D;		// General purpose 3D noise texture (32x32x32)

extern Video*		gs_pVideo;			// Global video capture from the webcam


// Return 0 for no error, any other code will ExitProcess() with this code
int		IntroInit( IntroProgressDelegate& _Delegate );
void	IntroExit();

// Return false to end the intro
bool	IntroDo( float _Time, float _DeltaTime );
