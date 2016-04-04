///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 3D Noise sampling
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef _NOISE_INCLUDED
#define _NOISE_INCLUDED

static const uint	NOISE_LATTICE_SIZE = 64;
static const float	NOISE_LATTICE_INV_SIZE = 1.0 / NOISE_LATTICE_SIZE;

Texture3D< float >		_TexNoise : register(t8);
Texture3D< float4 >		_TexNoise4D : register(t9);


// Low quality noise sampling
float NLQ( float3 _UVW, float4 _dot ) {
	return dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, _UVW, 0.0 ) );
}

// Medium quality noise sampling
float NMQ( float3 _UVW, float4 _dot ) {
	// Smooth the input coord
	float3	t = frac( _UVW * NOISE_LATTICE_SIZE + 0.5 );
	float3	t2 = (3.0 - 2.0*t)*t*t;
	float3	UVW2 = _UVW + NOISE_LATTICE_INV_SIZE * (t2-t);

	// Fetch
	return NLQ( UVW2, _dot );
}

// High quality noise sampling
float NHQ( float3 _UVW, float4 _dot, const float _smooth=1.0 ) {
	float3	FloorUVW = floor(_UVW * NOISE_LATTICE_SIZE) * NOISE_LATTICE_INV_SIZE;
	float3	t = (_UVW - FloorUVW) * NOISE_LATTICE_SIZE;
			t = lerp( t, t*t*(3.0 - 2.0*t), _smooth );
 
	float2	d = float2( NOISE_LATTICE_INV_SIZE, 0 );

#if 1
	// the 8-lookup version... (SLOW)
	float4	f1 = float4( dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.xxx, 0.0 ) ), 
						 dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.yxx, 0.0 ) ), 
						 dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.xyx, 0.0 ) ), 
						 dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.yyx, 0.0 ) ) );
	float4	f2 = float4( dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.xxy, 0.0 ) ), 
						 dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.yxy, 0.0 ) ), 
						 dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.xyy, 0.0 ) ), 
						 dot( _dot, _TexNoise4D.SampleLevel( LinearWrap, FloorUVW + d.yyy, 0.0 ) ) );

	float4	f3 = lerp( f2, f1, t.z );
	float2	f4 = lerp( f3.zw, f3.xy, t.y );
	float	f5 = lerp( f4.y, f4.x, t.x );
#else
	// THE TWO-SAMPLE VERSION: much faster!
	// NOTE: requires that three YZ-neighbor texels' original .x values are packed into .yzw values of each texel.
	float4	f1 = _Noise.SampleLevel(NearestRepeat, FloorUVW        , 0);	// <+0, +y,  +z,  +yz>
	float4	f2 = _Noise.SampleLevel(NearestRepeat, FloorUVW + d.xyy, 0);	// <+x, +xy, +xz, +xyz>
	float4	f3 = lerp(f1, f2, t.xxxx);										// <+0, +y,  +z,  +yz> (X interpolation)
	float2	f4 = lerp(f3.xz, f3.yw, t.yy);									// <+0, +z> (Y interpolation)
	float	f5 = lerp(f4.x, f4.y, t.z);										// Z interpolation
#endif
  
	return f5;
}

float	fbm( float3 _noisePosition ) {
	float	V0 = _TexNoise.SampleLevel( LinearWrap, 0.05 * _noisePosition, 0.0 );
	float	V1 = _TexNoise.SampleLevel( LinearWrap, 0.10 * _noisePosition, 0.0 );
	float	V2 = _TexNoise.SampleLevel( LinearWrap, 0.20 * _noisePosition, 0.0 );
	return (V2 + 2.0 * V1 + 4.0 * V0) / 7.0;
}

float3	fbm3D( float3 _noisePosition ) {
	float3	V0 = _TexNoise4D.SampleLevel( LinearWrap, 0.05 * _noisePosition, 0.0 ).xyz;
	float3	V1 = _TexNoise4D.SampleLevel( LinearWrap, 0.10 * _noisePosition, 0.0 ).xyz;
	float3	V2 = _TexNoise4D.SampleLevel( LinearWrap, 0.20 * _noisePosition, 0.0 ).xyz;
	return (V2 + 2.0 * V1 + 4.0 * V0) / 7.0;
}

float3	NoiseVector( float3 _noisePosition ) {
	return 2.0 * fbm3D( _noisePosition ) - 1.0;
}

#endif