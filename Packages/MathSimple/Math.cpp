#include "Math.h"

using namespace SharpMath;

float	Mathf::Lerp( cli::array<float>^ a, float t ) {
	int		W = a->GetLength(0);

	float	X = t * W;
	int		X0 = (int) Mathf::Floor( X );
	float	x = X - X0;
	int		X1 = Mathf::Clamp( X0+1, 0, W-1 );
			X0 = Mathf::Clamp( X0, 0, W-1 );

	float	V0 = a[X0];
	float	V1 = a[X1];

	float	V = (1-x) * V0 + x * V1;
	return V;
}

float	Mathf::BiLerp( cli::array<float,2>^ a, float u, float v ) {
	int		W = a->GetLength(0);
	int		H = a->GetLength(1);

	float	X = u * W;
	int		X0 = (int) Mathf::Floor( X );
	float	x = X - X0;
	int		X1 = Mathf::Clamp( X0+1, 0, W-1 );
			X0 = Mathf::Clamp( X0, 0, W-1 );

	float	Y = v * H;
	int		Y0 = (int) Mathf::Floor( Y );
	float	y = Y - Y0;
	int		Y1 = Mathf::Clamp( Y0+1, 0, H-1 );
			Y0 = Mathf::Clamp( Y0, 0, H-1 );

	float	V00 = a[X0,Y0];				// Top-left
	float	V10 = a[X1,Y0];				// Top-right
	float	V01 = a[X0,Y1];				// Bottom-left
	float	V11 = a[X1,Y1];				// Bottom-right

	float	V0 = (1-x) * V00 + x * V10;	// Top
	float	V1 = (1-x) * V01 + x * V11;	// Bottom

	float	V = (1-y) * V0 + y * V1;
	return V;
}
