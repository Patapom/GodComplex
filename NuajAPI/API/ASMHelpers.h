//--------------------------------------------------------------------------//
// iq . 2003/2008 . code for 64 kb intros by RGBA                           //
//--------------------------------------------------------------------------//
#pragma once

//#ifdef GODCOMPLEX
#if 1

// Override some functions with our own implementations
#define memset( dst, val, amount )	ASM_memset( dst, val, amount )
#define memcpy( dst, src, amount )	ASM_memcpy( dst, src, amount )

static float ASM_log2f( float x )
{
    float res;

    _asm fld    dword ptr [x]
    _asm fld1
    _asm fxch   st(1)
    _asm fyl2x
    _asm fstp   dword ptr [res]

    return res;
}

static float ASM_expf( float x )
{
    float res;

    _asm fld     dword ptr [x]
    _asm fldl2e
    _asm fmulp   st(1), st(0)
    _asm fld1
    _asm fld     st(1)
    _asm fprem
    _asm f2xm1
    _asm faddp   st(1), st(0)
    _asm fscale
    _asm fxch    st(1)
    _asm fstp    st(0)
    _asm fstp    dword ptr [res]

    return res;
}

static float ASM_fmodf( float x, float y )
{
    float res;

    _asm fld     dword ptr [y]
    _asm fld     dword ptr [x]
    _asm fprem
    _asm fxch    st(1)
    _asm fstp    st(0)
    _asm fstp    dword ptr [res]

    return res;
}

static float ASM_powf( float x, float y )
{
    float res;

    _asm fld     dword ptr [y]
    _asm fld     dword ptr [x]
    _asm fyl2x
    _asm fld1
    _asm fld     st(1)
    _asm fprem
    _asm f2xm1
    _asm faddp   st(1), st(0)
    _asm fscale
    _asm fxch
    _asm fstp    st(0)
    _asm fstp    dword ptr [res];

    return res;
}

// Fast acos() using cubic polynomial approximation
// http://stackoverflow.com/questions/3380628/fast-arc-cos-algorithm
static float FAST_acosf( float x )
{
   return (-0.69813170079773212f * x * x - 0.87266462599716477f) * x + 1.5707963267948966f;
}

// From http://www.jbox.dk/sanos/source/lib/math/acos.asm.html
static float ASM_acosf( float x )
{
    float res;

	_asm fld     dword ptr [x]           // Load real from stack
	_asm fld     st(0)                   // Load x
	_asm fld     st(0)                   // Load x
	_asm fmul                            // x²
	_asm fld1                            // Load 1
	_asm fsubr                           // 1 - x²
	_asm fsqrt                           // sqrt( 1 - x² )
	_asm fxch                            // Exchange st, st(1)
	_asm fpatan                          // atan( sqrt( 1 - x*x ) / x ) = This gives the arc cosine !
	_asm fstp    dword ptr [res];

	return res;
}

static float ASM_asinf( float x )
{
    float res;

	_asm fld     dword ptr [x]           // Load real from stack
	_asm fld     st(0)                   // Load x
	_asm fld     st(0)                   // Load x
	_asm fmul                            // x²
	_asm fld1                            // Load 1
	_asm fsubr                           // 1 - x²
	_asm fsqrt                           // sqrt( 1 - x² )
	_asm fpatan                          // atan( x / sqrt( 1 - x*x ) ) = This gives the arc sine !
	_asm fstp    dword ptr [res];

	return res;
}


static U16	opc1 = 0x043f;     // floor
static U16	opc2 = 0x083f;     // ceil

static int ASM_floorf( float x )
{
    int res;
    short tmp;

    _asm fstcw   word  ptr [tmp]
    _asm fld     dword ptr [x]
    _asm fldcw   word  ptr [opc1]
    _asm fistp   dword ptr [res]
    _asm fldcw   word  ptr [tmp]

    return res;
}

static int ASM_ceilf( float x )
{
	int res;
	short tmp;

	_asm fstcw   word  ptr [tmp]
	_asm fld     dword ptr [x]
	_asm fldcw   word  ptr [opc2]
	_asm fistp   dword ptr [res]
	_asm fldcw   word  ptr [tmp]

	return res;
}

static void ASM_memset( void *dst, int val, int amount )
{
    _asm mov edi, dst
    _asm mov eax, val
    _asm mov ecx, amount
    _asm rep stosb
}

static void ASM_memcpy( void *dst, const void *src, int amount )
{
    _asm mov edi, dst
    _asm mov esi, src
    _asm mov ecx, amount
    _asm rep movsb
}

static int ASM_strlen( const char *src )
{
    int res;

    _asm mov esi, src
    _asm xor ecx, ecx
    _asm myloop:
    _asm    mov al, [esi]
    _asm    inc esi
    _asm    inc ecx
    _asm    test al, al
    _asm    jnz myloop
    _asm dec ecx
    _asm mov [res], ecx

    return res;    
}

#endif