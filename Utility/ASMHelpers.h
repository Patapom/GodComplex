//--------------------------------------------------------------------------//
// iq . 2003/2008 . code for 64 kb intros by RGBA                           //
//--------------------------------------------------------------------------//
#pragma once

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

static short opc1 = 0x043f;     // floor

static int ASM_ifloorf( float x )
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

static void ASM_memset( void *dst, int val, int amount )
{
    _asm mov edi, dst
    _asm mov eax, val
    _asm mov ecx, amount
    _asm rep stosb
}

static void ASM_memcpy( void *dst, const void *ori, int amount )
{
    _asm mov edi, dst
    _asm mov esi, ori
    _asm mov ecx, amount
    _asm rep movsb
}

static int ASM_strlen( const char *ori )
{
    int res;

    _asm mov esi, ori
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
