#pragma once

// Return 0 for no error, any other code will ExitProcess() with this code
int		IntroInit( IntroProgressDelegate& _Delegate );
void	IntroExit();

// Return false to end the intro
bool	IntroDo( float _Time, float _DeltaTime );
