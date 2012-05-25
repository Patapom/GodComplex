#pragma once

bool	IntroInit( IntroProgressDelegate& _Delegate );
void	IntroExit();

// Return false to end the intro
bool	IntroDo();
