//////////////////////////////////////////////////////////////////////////
// Random
// SimpleRNG is a simple random number generator based on George Marsaglia's MWC (multiply with carry) generator.
// Although it is very simple, it passes Marsaglia's DIEHARD series of random number generator tests.
// 
// Written by John D. Cook 
// http://www.johndcook.com
//
#pragma once

void	_srand( U32 u, U32 v );
float	_rand();
float	_randGauss();
