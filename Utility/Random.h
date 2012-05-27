//////////////////////////////////////////////////////////////////////////
// Random
// SimpleRNG is a simple random number generator based on George Marsaglia's MWC (multiply with carry) generator.
// Although it is very simple, it passes Marsaglia's DIEHARD series of random number generator tests.
// 
// Written by John D. Cook 
// http://www.johndcook.com
//
#pragma once

#define RAND_DEFAULT_SEED_U	521288629
#define RAND_DEFAULT_SEED_V	362436069

void	_srand( U32 u, U32 v );
void	_randpushseed();
void	_randpopseed();
float	_frand();					// [0,1]
float	_frandStrict();				// ]0,1[
U32		_rand();					// [0,2^32[
U32		_rand( U32 min, U32 max );	// [min,max]
U32		_rand( U32 size );			// [0,size[
float	_randGauss();
