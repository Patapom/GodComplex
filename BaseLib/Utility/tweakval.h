#pragma once

//=========================================================
// tweakval.h
//
// Author: Joel Davis (joeld42@yahoo.com)
// 
// Based on a discussion on this thread:
// https://mollyrocket.com/forums/viewtopic.php?t=556
// See that thread for details.
//
// USAGE: Anywhere you're using a constant value that 
// you want to be tweakable, wrap it in a _TV() macro. 
// In a release build (with -DNDEBUG defined) this 
// will simply compile out to the constant, but in a 
// debug build this will scan the source file for 
// a modified value, providing a "live update" in 
// the game.
//
// Simply edit the value in the source file, and save the file
// while the game is running, and you'll see the change.
//
//  EXAMPLES:
//     // Move at constant (tweakable) speed
//     float newPos = oldPos + _TV(5.2f) * dt;
//
//     // Clear to a solid color
//     glClearColor( _TV( 0.5 ), _TV( 0.2 ), _TV( 0.9 ), 1.0 );
//     glClear( GL_COLOR_BUFFER_BIT );
//
//     // initial monster health
//     Monster *m = new Monster( _TV( 10 ) );
//
//   WARNING: 
//   This is currently in a very rough state, it doesn't
//   handle errors, c-style comments, and adding and removing or
//   rearranging TV's will probably confuse it.
//   Also, putting _TV's in header files is not a good idea.
//=========================================================

#define MATERIAL_REFRESH_CHANGES_INTERVAL	500		// Time in ms between checks for changes


// Do only in debug builds
#ifdef _DEBUG

	#define _TV(Val) _TweakValue( __FILE__, __COUNTER__, Val )

	float	_TweakValue( const char *file, size_t counter, float origVal );
	int		_TweakValue( const char *file, size_t counter, int origVal );
	void	ReloadChangedTweakableValues();

#else

	#define _TV(Val) Val
	#define ReloadChangedTweakableValues()

#endif
