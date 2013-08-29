#include "../GodComplex.h"

// Textures
Texture3D*	gs_pTexNoise3D = NULL;

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

int	Build3DTextures( IntroProgressDelegate& _Delegate )
{
	NjHalf4*	ppNoise[NOISE3D_SHIFT+1];

	// Build wrapping 3D noise for mip level 0
	Noise	N( 1 );
	_randpushseed();
	_srand( RAND_DEFAULT_SEED_U, RAND_DEFAULT_SEED_V );

	ppNoise[0] = new NjHalf4[NOISE3D_SIZE*NOISE3D_SIZE*NOISE3D_SIZE];

	NjFloat3	Position;
	NjFloat3	Offset0( _frand(), _frand(), _frand() );
	NjFloat3	Offset1( _frand(), _frand(), _frand() );
	NjFloat3	Offset2( _frand(), _frand(), _frand() );
	for ( int Z=0; Z < NOISE3D_SIZE; Z++ )
	{
		Position.z = float(Z) / NOISE3D_SIZE;
		for ( int Y=0; Y < NOISE3D_SIZE; Y++ )
		{
			Position.y = float(Y) / NOISE3D_SIZE;
			for ( int X=0; X < NOISE3D_SIZE; X++ )
			{
				Position.x = float(X) / NOISE3D_SIZE;

				int	Index = X+((Y+(Z<<NOISE3D_SHIFT)) << NOISE3D_SHIFT);

				ppNoise[0][Index].x = N.WrapPerlin( Position );
				ppNoise[0][Index].y = N.WrapPerlin( Position + Offset0 );
				ppNoise[0][Index].z = N.WrapPerlin( Position + Offset1 );
				ppNoise[0][Index].w = N.WrapPerlin( Position + Offset2 );
			}
		}
	}
	_randpopseed();

	// Build mipmaps
	int	Size = NOISE3D_SIZE;
	for ( int MipLevel=1; MipLevel <= NOISE3D_SHIFT; MipLevel++ )
	{
		int	PreviousSize = Size;
		Size >>= 1;

		ppNoise[MipLevel] = new NjHalf4[Size*Size*Size];
		for ( int Z=0; Z < Size; Z++ )
		{
			NjHalf4*	pSlice0 = ppNoise[MipLevel-1] + PreviousSize*PreviousSize*(2*Z);
			NjHalf4*	pSlice1 = ppNoise[MipLevel-1] + PreviousSize*PreviousSize*(2*Z+1);

			NjHalf4*	pTargetSlice = ppNoise[MipLevel] + Size*Size*Z;

			for ( int Y=0; Y < Size; Y++ )
			{
				NjHalf4*	pScanline00 = pSlice0 + PreviousSize*(2*Y);
				NjHalf4*	pScanline01 = pSlice0 + PreviousSize*(2*Y+1);
				NjHalf4*	pScanline10 = pSlice1 + PreviousSize*(2*Y);
				NjHalf4*	pScanline11 = pSlice1 + PreviousSize*(2*Y+1);

				NjHalf4*	pTargetScanline = pTargetSlice + Size*Y;

				for ( int X=0; X < Size; X++ )
				{
					int			Px = 2*X;
					int			Nx = 2*X+1;

					NjFloat4	V000 = pScanline00[Px];
					NjFloat4	V001 = pScanline00[Nx];
					NjFloat4	V010 = pScanline01[Px];
					NjFloat4	V011 = pScanline01[Nx];
					NjFloat4	V100 = pScanline10[Px];
					NjFloat4	V101 = pScanline10[Nx];
					NjFloat4	V110 = pScanline11[Px];
					NjFloat4	V111 = pScanline11[Nx];

					NjFloat4	V = 0.125f * (V000 + V001 + V010 + V011
											+ V100 + V101 + V110 + V111);

					*pTargetSlice++ = V;
				}
			}
		}
	}

	// Generate texture
	gs_pTexNoise3D = new Texture3D( gs_Device, NOISE3D_SIZE, NOISE3D_SIZE, NOISE3D_SIZE, PixelFormatRGBA16F::DESCRIPTOR, 0, (void**) ppNoise );

	gs_pTexNoise3D->Set( 0, true );	// This is a global texture !

	for ( int MipLevel=0; MipLevel <= NOISE3D_SHIFT; MipLevel++ )
		delete[] ppNoise[MipLevel];

#if 1
	// Save as POM format
	Texture3D*	pStagingNoise = new Texture3D( gs_Device, NOISE3D_SIZE, NOISE3D_SIZE, NOISE3D_SIZE, PixelFormatRGBA16F::DESCRIPTOR, 0, NULL, true );
	pStagingNoise->CopyFrom( *gs_pTexNoise3D );
	pStagingNoise->Save( "./Noise32x32x32.pom" );
	delete pStagingNoise;
#endif

	return 0;
}

void	Delete3DTextures()
{
	delete gs_pTexNoise3D;
}
