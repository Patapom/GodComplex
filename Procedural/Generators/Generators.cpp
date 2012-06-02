#include "../../GodComplex.h"

//////////////////////////////////////////////////////////////////////////
// AO
// The algorithm here is to find the "horizon" of the height field for each of the samplings directions
// We slice the circle of directions about the considered point then for each direction we march forward
//	and analyze the height of the height field at that position.
// The height and the distance we marched give us the slope of the horizon, which we keep if it exceeds the
//	maximum for that direction.
// In the end, we obtain a portion of horizon that is not occluded by the surrounding heights. Summing these
//	portions we get the AO...
//
struct __AOStruct
{
	const TextureBuilder*	pSource;
	float			HeightFactor;
	int				DirectionsCount;
	int				SamplesCount;
	bool			bWriteOnlyAlpha;
};
void	FillAO( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	__AOStruct&	Params = *((__AOStruct*) _pData);

	float	SumAO = 0.0f;
	for ( int DirectionIndex=0; DirectionIndex < Params.DirectionsCount; DirectionIndex++ )
	{
		float		Angle = TWOPI * DirectionIndex / Params.DirectionsCount;
		NjFloat2	Direction( cosf( Angle ), sinf( Angle ) );

//		NjFloat2	Position( float(_X), float(_Y) );	// For some reason, this doesn't compile !!
		NjFloat2	Position;
		Position.x = float(_X);
		Position.y = float(_Y);

		float	MaxSlope = 0.0f;
		for ( int SampleIndex=0; SampleIndex < Params.SamplesCount; SampleIndex++ )
		{
			Position = Position + Direction;	// March one step

			NjFloat4	C;
			Params.pSource->SampleWrap( Position.x, Position.y, C );
			float		Height = C | LUMINANCE;	// Transform into luminance, which we consider being the height

			float		Slope = Params.HeightFactor * Height / (1.0f + SampleIndex);	// The slope of the horizon
			MaxSlope = MAX( MaxSlope, Slope );
		}

		// Accumulate visibility
		SumAO += HALFPI - atanf( MaxSlope );
	}
	SumAO /= HALFPI * Params.DirectionsCount;	// Normalize

	_Color.w = SumAO;
	if ( Params.bWriteOnlyAlpha )
		return;

	_Color.x = SumAO;
	_Color.y = SumAO;
	_Color.z = SumAO;
}

void Generators::ComputeAO( const TextureBuilder& _Source, TextureBuilder& _Target, float _HeightFactor, int _DirectionsCount, int _SamplesCount, bool _bWriteOnlyAlpha )
{
	__AOStruct	Params;
	Params.pSource = &_Source;
	Params.HeightFactor = _HeightFactor;
	Params.DirectionsCount = _DirectionsCount;
	Params.SamplesCount = _SamplesCount;
	Params.bWriteOnlyAlpha = _bWriteOnlyAlpha;

	_Target.Fill( FillAO, &Params );
}

//////////////////////////////////////////////////////////////////////////
// Dirtyness
struct __DirtynessStruct
{
	TextureBuilder*	pBuilder;
	const Noise*	pNoise;
	float			DirtNoiseFrequency;
	float			DirtAmplitude;
	float			PullBackForce;
	float			AverageIntensity;
};
void	FillDirtyness( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	__DirtynessStruct&	Params = *((__DirtynessStruct*) _pData);

	NjFloat4	C[3];
	Params.pBuilder->SampleWrap( _X-1.0f, _Y-1.0f, C[0] );
	Params.pBuilder->SampleWrap( _X+0.0f, _Y-1.0f, C[1] );
	Params.pBuilder->SampleWrap( _X+1.0f, _Y-1.0f, C[2] );

//	NjFloat4	F = C[0] + C[2] - C[1];	// Some sort of average
//	NjFloat4	F = 0.333f * (C[0] + C[2] + C[1]);
	NjFloat4	F = C[1];
	float		fOffset = Params.DirtAmplitude * Params.pNoise->Perlin( Params.DirtNoiseFrequency * _UV ) + F.w;

	F.x += fOffset;
	F.y += fOffset;
	F.z += fOffset;
	F.w = Params.PullBackForce * (Params.AverageIntensity - (F | LUMINANCE));	// Will yield -1 when F reaches the average intensity so the color is always somewhat brought back to average

	_Color = F;
}

void	Generators::Dirtyness( TextureBuilder& _Builder, const Noise& _Noise, float _InitialIntensity, float _AverageIntensity, float _DirtNoiseFrequency, float _DirtAmplitude, float _PullBackForce )
{
	__DirtynessStruct	Params;
	Params.pBuilder = &_Builder;
	Params.pNoise = &_Noise;
	Params.DirtNoiseFrequency = _DirtNoiseFrequency;
	Params.DirtAmplitude = _DirtAmplitude;
	Params.PullBackForce = _PullBackForce;
	Params.AverageIntensity = _AverageIntensity;

	// Setup last line used as initial seed
	for ( int X=0; X < _Builder.GetWidth(); X++ )
	{
		NjFloat4&	Pixel = _Builder.GetMips()[0][_Builder.GetWidth() * (_Builder.GetHeight()-1) + X];
//		float		InitialValue = _AverageIntensity + abs( _Noise.Perlin( NjFloat2( _InitNoiseFrequency * float(X) / _Builder.GetWidth(), 0.0f ) ) );
		float		InitialValue = _InitialIntensity;
		Pixel.x = Pixel.y = Pixel.z = InitialValue;
		Pixel.w = 0.0f;
	}

	_Builder.Fill( FillDirtyness, &Params );
}

//////////////////////////////////////////////////////////////////////////
// Secret marble recipe
// struct __MarbleStruct
// {
// 	U32		Width;
// 	S16		Min, Max;
// 	S16*	pBuffer;
// };
// void	FillMarble( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
// {
// 	__MarbleStruct&	Params = *((__MarbleStruct*) _pData);
// 
// 	S16		iValue = Params.pBuffer[Params.Width*_Y+_X];
// 	float	Value = float(iValue - Params.Min) / (Params.Max - Params.Min);
// 	_Color.x = _Color.y = _Color.z = Value;
// }
// 
// void	Generators::Marble( TextureBuilder& _Builder, int _BootSize )
// {
// 	int			W = _Builder.GetWidth();
// 	int			H = _Builder.GetHeight();
// 	H += _BootSize;		// Skip the first lines are sometimes ugly 
// 
// Original version using S16 words
//
// 	S16*	pBuffer = new S16[W*H];
// 
// 	// Build marble
// 	static U16	RNG0 = 0;
// 	static U16	RNG1 = 0;
// 	static U16	RNG2 = 0;
// 	static S16	RandOffset;
// 
// 	S16		Min = 32767;
// 	S16		Max = -32768;
// 	S16*	pScanline = pBuffer + 2*W;
// 	for ( int Y=0; Y < H-2; Y++ )
// 		for ( int X=0; X < W; X++ )
// 		{
// 			// Do your magic trick
// 			_asm	mov     ax, RNG0
// 			_asm	mov     cx, RNG1
// 			_asm	add     ax, cx
// 			_asm	rol     ax, cl
// 			_asm	add     cx, 0x01234
// 			_asm	ror     cx, 1
// 			_asm	mov     RNG0, ax
// 			_asm	mov     RNG1, cx
// //			_asm	add     ax, RNG2
// //			_asm	dec     RNG2
// 
// // 			_asm	sar     ax,0ch
// // 			_asm	inc     ax
// 			_asm	mov		RandOffset, ax
// 
// 
// //			RandOffset = (RandOffset % 5) - 2;	// Intéressant !
// 			RandOffset = 1 + RandOffset / 10000;
// 
// 
// 			// Average previous line's colors
// //			S16	Average = pScanline[-W];	// Sad lines
// 			S16	Average = (pScanline[-W] + pScanline[-W+1]) >> 1;					// Original recipe !
// //			S16	Average = (pScanline[-W-1] + pScanline[-W] + pScanline[-W+1]) / 3;	// Sympa !!
// 			Average += RandOffset;
// 
// 			*pScanline++ = Average;
// 
// 			Min = MIN( Min, Average );
// 			Max = MAX( Max, Average );
// 		}
// 
// 	// Renormalize
// 	__MarbleStruct	Params;
// 	Params.Width = W;
// 	Params.Min = Min;
// 	Params.Max = Max;
// 	Params.pBuffer = pBuffer + W * _BootSize;
// 	_Builder.Fill( FillMarble, &Params );
// 
// 	delete[] pBuffer;
// }

struct __MarbleRandomStruct
{
	U16	C0, C1, C2;
};
float	CalcColor( __MarbleRandomStruct& _Marble )
{
	U32	A = _Marble.C0;
	U32	C = _Marble.C1;
	A += C;
	A <<= C & 0xFF;	A |= A >> 16;	// Random ROL
	C += 0x1234;					// Add magic value
	C = (C >> 1) | (C << 15);		// ROR 1
	_Marble.C0 = U16(A);
	_Marble.C1 = U16(C);

	A += _Marble.C2--;

	return A / 65535.0f;
}
U32	LCGRandom( U32& _LastValue )
{
	return _LastValue = U32( (1103515245u * _LastValue + 12345u) );
}

struct __MarbleStruct
{
	U32		Width;
	float	Min, Factor;
	float*	pBuffer;
};
void	FillMarble( int _X, int _Y, const NjFloat2& _UV, NjFloat4& _Color, void* _pData )
{
	__MarbleStruct&	Params = *((__MarbleStruct*) _pData);

	float	Value = Params.pBuffer[Params.Width*_Y+_X];
	Value = Params.Factor * (Value - Params.Min);
	_Color.x = _Color.y = _Color.z = Value;
}


void	Generators::Marble( TextureBuilder& _Builder, int _BootSize )
{
	int			W = _Builder.GetWidth();
	int			H = _Builder.GetHeight();
	H += _BootSize;		// Skip the first lines are sometimes ugly 

	float*		pBuffer = new float[W*H];

	__MarbleRandomStruct	RNG;
	RNG.C0 = RNG.C1 = RNG.C2 = 0;

	// Fill up the first N lines
	U32		RandomSeed = 1;
	float	Min = FLOAT32_MAX, Max = -FLOAT32_MAX;
	float*	pTarget = pBuffer + 2*W;
	for ( int Y=2; Y < H; Y++ )
	{
		float*	pSource = &pBuffer[W*(Y-1)];
		for ( int X=0; X < W; X++ )
		{
//			float		RandomColor = CalcColor( RNG );
			LCGRandom( RandomSeed );
			float		RandomColor = RandomSeed / 2147483648.0f - 1.0f;

			float	C0 = pSource[(X+W-1) % W];
			float	C1 = pSource[X];
			float	C2 = pSource[(X+1) % W];

			float	C = 0.5f * (C1 + C2);
//			float	C = (C0 + C1 + C2) / 3.0f;
					C += 0.2f * RandomColor;

			C = fmodf( C, 1.0f );
			Min = MIN( Min, C );
			Max = MAX( Max, C );

			*pTarget++ = C;
		}
	}

	// Renormalize
	__MarbleStruct	Params;
	Params.Width = W;
	Params.Min = Min;
	Params.Factor = 1.0f / (Max - Min);
	Params.pBuffer = pBuffer + W * _BootSize;
	_Builder.Fill( FillMarble, &Params );

	delete[] pBuffer;
}

/*

                                                                     
                                                                     
                                                                     
                                             
static	sword	gw10ce;
static	sword	gw10d0;
static	sword	gw10d2;
static	sword	gw10d3;

static void ComputeMarble(Picture& picture)
{
	picture.Clear();
	const sdword Width = picture.GetWidth();
	const sdword Height = picture.GetHeight();

	gw10ce = 0;
	gw10d0 = 0;
	gw10d2 = 0;

	udword NbToGo = Width*(Height-2);
	sword* Buffer = (sword*)ICE_ALLOC_TMP(sizeof(sword)*Width*Height);
	ZeroMemory(Buffer, sizeof(sword)*Width*Height);

	sword* Dest_EDI = Buffer + Width*2;
	sword Min = MAX_SWORD;
	sword Max = MIN_SWORD;
	while(NbToGo--)
	{
		_asm	mov     ax, gw10ce
		_asm	mov     cx, gw10d0
		_asm	add     ax, cx
		_asm	rol     ax, cl
		_asm	add     cx, 0x01234
		_asm	ror     cx, 1
		_asm	mov     gw10ce, ax
		_asm	mov     gw10d0, cx
		_asm	add     ax, gw10d2
		_asm	dec     gw10d2

		_asm	sar     ax,0ch
		_asm	inc     ax
		_asm	mov		gw10d3, ax
		gw10d3 += (Dest_EDI[-Width] + Dest_EDI[-Width+1])/2;

		if(gw10d3<Min)
			Min = gw10d3;
		if(gw10d3>Max)
			Max = gw10d3;

		*Dest_EDI++ = gw10d3;
	}

	RGBAPixel* Dest = picture.GetPixels();
	const float Coeff = 255.0f/float(Max - Min);
	for(sdword i=0;i<Width*Height;i++)
	{
		sword Value = Buffer[i];
		Value -= Min;
		Value = sword(float(Value)*Coeff);
		Dest->R = Dest->G = Dest->B = ubyte(Value);
		Dest->A = PIXEL_OPAQUE;
		Dest++;
	}

	ICE_FREE(Buffer);
}
*/



/* StartY = 30

		bool	bFirstPixel = true;
		U8*		pTarget = _pTarget;
        U8*		pTargetEND = _pTarget + 320 * StartY

        int		PixelsCount = 320*(200 + StartY-2)
next_dot:
        U16		RandomColor = CalcColor();
        RandomColor >>= 12		// 16 colors
        RandomColor++			// Except black ?

        U8		C0 = pTarget[-320]	// Color[X,Y-1]
        U8		C1 = pTarget[-319]	// Color[X+1,Y-1]
        U16		C = 0.5 * (C0 + C1)	// Average
		        C += RandomColor	// Add random
		*pTarget++ = C;				// Store as new color

		// cmp	bFirstTime,0 jnz     go_ahead_mb
		if ( !bFirstPixel )
			goto go_ahead_mb;

		// cmp	pTarget, pTargetEND  jnz     go_ahead_mb
		if ( pTarget != pTargetEND )
			goto go_ahead_mb;

        pSource = pTarget-640;
        pTarget = _pTarget;
		for ( int i=0; i < 640; i++ )
			pTarget[i] = pSource[i];

		bFirstPixel = false;

        pTarget = _pTarget + 640;

go_ahead_mb:
        dec     PixelsCount
        jne     next_dot

*/
/*


StartY          =       30

;nb_colors       =       3
;0=256 couleurs
;1=128 couleurs
;2=64 couleurs
;3=32 couleurs
;4=16 couleurs

;flag_trame      =       0
;0=on exécute le tramage (2 fois moins de couleur)
;1=Que dalle

        ;edi=adresse destination
        ;eax=couleur de départ
        ;ebx=flag_trame
        ;ecx=nb_colors
aff_marbre:
    bFirstTime = 1
	pScanline = pScreen;
    pScanlineEnd = pScreen + 320 * StartY
        
	PixelsCount = 320*(200+StartY-2)	// On remplit 200 + 30 lignes ??? -2 okay, on s'en branle, on remplit plus que 200 lignes j'veux dire ???
	do
	{
        U16		Random = calc_color();
		Random = (Random >> 12) + 1;

		U8		C0 = pScanline[-320];
		U8		C1 = pScanline[-319];
		U16		Color = (C0 + C1) >> 1;
				Color += Random
		*pScanline++ = U8(Color);

// ;        cmp     bFirstTime,0       jnz     go_ahead_mb		// PUTAIN DE DOUBLE NEGATION !! C'est imbitable sérieux !! Êtes-vous non-séronégatif ?? Attention à la réponse !
// 		if ( !bFirstTime )
// 			goto go_ahead_mb;
// 
// ;        cmp     edi,value_mb        jnz     go_ahead_mb
// 		if ( pScanline != pScanlineEnd )
// 			goto go_ahead_mb;
// 
// 		if ( !bFirstTime || pScanline != pScanlineEnd )
// 			continue;	// WTF ??? Sérieux ?? Ca sera jamais exécuté ce code !
// 
// 		mov     esi,edi
// 		sub     esi,640
// 		mov     edi,pScreen
// 		mov     ecx,640/4
// 		rep     movsd
// 
// 		bFirstTime = 0
// 
// 		mov     edi,pScreen
// 		add     edi,640
	}
    while ( PixelsCount-- > 0 );


	; This part only limits colors to a determined set
	; Shouldn't be done only AFTER halftone ??
	for ( int i=0; i < 320*200; i++ )
		pScreen[i] >>= Colors Count;


	// Halftone
	if ( flag_trame )
	{
        U8*	pScanline = pScreen;
		for ( int Y=0; Y < 200; Y++ )
			for ( int X=0; X < 320; X++, pScanline++ )
			{
				mov	al,b ds:[pScanline]

				if ( al & 1 != 0 )
				{	// Odd
					mov     edi, pScanline
					add     edi, Y&1
					if ( edi & 1 == 0 )
					{
						inc     al
						sar     al,1
						mov     ds:[pScanline],al
					}
					else
					{
						dec     al
						sar     al,1
						mov     ds:[pScanline],al
					}
				}
				else
				{	// Even
					sar     al,1
					mov     ds:[pScanline],al
				}
				pScanline++;
			}
	}

	// Offset
	for ( int i=0; i < 320*200; i++ )
		pScreen[i] += ColorOffset;

	return


w10ce			dw      0000
w10d0			dw      0000
w10d2			dw      0000

value_init      dd      0
*/