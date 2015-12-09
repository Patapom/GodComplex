/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Provides helpers to debug the exposure histogram
// Allows to highlight a particular luminance by hovering the mouse over a specific pixel
//
static const float	LUM_HISTOGRAM_INSET_WIDTH = 0.4;	// Size of the histogram inset in UV space (1 means the entire screen!)
static const float	LUM_HISTOGRAM_INSET_HEIGHT = 0.25;	// Size of the histogram inset in UV space (1 means the entire screen!)
		
float	DrawGraduationText( float2 _UV, float _Value, int _GraduationIndex );

// Displays the luminance histogram as computed by the auto-exposure algorithm
// You can hover the mouse cursor on the histogram to highlight a specific range of luminance on screen
//	and also see an "out of bounds" checker board whenever the luminance is below or above the LDR range...
//
void	DEBUG_DisplayLuminanceHistogram( float2 _UV, float2 _mouseUV, float _debugLuminanceLevel, float2 _screenSize, float _time, inout float3 _Color, float3 _OriginalColor ) {
	autoExposure_t	Adaptation = ReadAutoExposureParameters();

	// Debug on screen luminances that are over- or under-exposed
	if ( _mouseUV.x < LUM_HISTOGRAM_INSET_WIDTH && _mouseUV.y > 1.0-LUM_HISTOGRAM_INSET_HEIGHT ) {
		float	ScreenWorldLuminance = BISOU_TO_WORLD_LUMINANCE * dot( LUMINANCE, _OriginalColor );

		#if 1
			// Show an ugly scrolling checker board
			if ( ScreenWorldLuminance < Adaptation.MinLuminanceLDR || ScreenWorldLuminance > Adaptation.MaxLuminanceLDR ) {
				uint2	PixelPos = uint2( floor( _UV * _screenSize + 20.0 * _time ) );
						PixelPos >>= 2;
				_Color = lerp( float3( 0, 0, 0 ), float3( 1, 1, 0 ), (PixelPos.x ^ PixelPos.y) & 1 );
			}
		#else
			// Show blue/red zones
			if ( ScreenWorldLuminance < Adaptation.MinLuminanceLDR )
//				_Color.yz += 0.05 * float2( 1, 2 );		// Blue-ize under-exposed areas
				_Color = float3( 0, 0.0, 0.4 );
			if ( ScreenWorldLuminance > Adaptation.MaxLuminanceLDR )
//				_Color.yz -= 0.8 * float2( 0.8, 1.0 );	// Red-ize over-exposed areas
				_Color = float3( 0.9, 0, 0 );
		#endif
	}

	// Debug on screen luminances by pointing the histogram with the mouse
	if ( _mouseUV.x < LUM_HISTOGRAM_INSET_WIDTH && _mouseUV.y > 1.0-LUM_HISTOGRAM_INSET_HEIGHT ) {
		float2	MouseUV = float2( _mouseUV.x / LUM_HISTOGRAM_INSET_WIDTH, (_mouseUV.y - 1.0 + LUM_HISTOGRAM_INSET_HEIGHT) / LUM_HISTOGRAM_INSET_HEIGHT );
		float	fHistoBucketIndex = ceil( HISTOGRAM_SIZE * MouseUV.x );
		float	HistoLuminance = MIN_ADAPTABLE_SCENE_LUMINANCE * dB2Luminance( fHistoBucketIndex * HISTOGRAM_BUCKET_RANGE_DB );
		float	ArkaneLuminance = WORLD_TO_BISOU_LUMINANCE * HistoLuminance;

		float	ScreenLuminance = dot( LUMINANCE, _OriginalColor );
		_Color = lerp( float3( 1, 0, 0 ), _Color, saturate( 10.0 * abs( ScreenLuminance - ArkaneLuminance ) / ArkaneLuminance ) );
	}

	if ( _UV.x < LUM_HISTOGRAM_INSET_WIDTH && _UV.y > 1.0-LUM_HISTOGRAM_INSET_HEIGHT ) {
		// Renormalize UVs
		_UV.x /= LUM_HISTOGRAM_INSET_WIDTH;
		_UV.y = (_UV.y - 1.0+LUM_HISTOGRAM_INSET_HEIGHT) / LUM_HISTOGRAM_INSET_HEIGHT;

		// Determine histogram factors from screen's resolution
		uint	Width = _screenSize.x;
		uint	Height = _screenSize.y;
		uint	TotalPixels = Width * Height;								// Highest possible histogram peak
		float	AveragePixelsCount = float(TotalPixels) / HISTOGRAM_SIZE;	// Average peak size if all pixels were distributed over the entire histogram
		float	HistoFactor = 1.0 / AveragePixelsCount;						// Multiplied by 1.0 for good measure

		// Display the histogram
		float	fHistoBucketIndex = HISTOGRAM_SIZE * _UV.x;
		uint2	HistoBucketIndex = uint2( floor( fHistoBucketIndex ), 0 );

// We could lerp the histogram but I suspect we'll miss some infos...
// 		 		float	HistoValue0 = saturate( HistoFactor * _texHistogram[HistoBucketIndex].x );
// 		 		float	HistoValue1 = saturate( HistoFactor * _texHistogram[HistoBucketIndex+uint2(1,0)].x );
// 				float	HistoValue = lerp( HistoValue0, HistoValue1, smoothstep( 0, 1, fHistoBucketIndex - HistoBucketIndex.x ) );

// So instead we use the square value...
		float	HistoValue = saturate( HistoFactor * _texHistogram[HistoBucketIndex].x );

		// Check if we're within adapted range
		// Retrieve the luminance represented by the current histogram column
		// We know that dB = 20.log10( Lum )
		float	HistoLuminance = MIN_ADAPTABLE_SCENE_LUMINANCE * dB2Luminance( fHistoBucketIndex * HISTOGRAM_BUCKET_RANGE_DB );
		float	InRange = HistoLuminance > Adaptation.MinLuminanceLDR && HistoLuminance < Adaptation.MaxLuminanceLDR ? 1.0 : 0.0;

		// Compute histogram background color with graduations
		float	DebugLuminanceHistoBucketIndex = Luminance2dB( _debugLuminanceLevel / MIN_ADAPTABLE_SCENE_LUMINANCE ) / HISTOGRAM_BUCKET_RANGE_DB;

		float3	BackgroundColor = lerp( 0.2, 0.7, _UV.x );						// Nice gradient background
				BackgroundColor = lerp( BackgroundColor, 0.8 * BackgroundColor, InRange );
		float	IsGraduation = saturate( fHistoBucketIndex % (HISTOGRAM_SIZE / 6) );	// The division by 6 is there because we're visualizing 6 orders of magnitude in luminance (from 10^-2 to 10^4)
		float	IsRef = floor( fHistoBucketIndex / (HISTOGRAM_SIZE / 6) ) == 4;			// 4th graduation is 10^2, our reference luminance for 100W lightbulb
		float	IsDebug = smoothstep( 0.5, 0.0, abs( fHistoBucketIndex - DebugLuminanceHistoBucketIndex ) );
		float3	GraduationColor = lerp( float3( 0.5, 0.0, 0.0 ), float3( 0.0, 0.5, 0 ), IsRef );	// Paint ref graduation in green, others in red
				GraduationColor = lerp( GraduationColor, BackgroundColor, _UV.y );					// Make a nice gradient to avoid painting dull graduations

		#if 1	// ==== Paint the integral ====
			float	CurrentFrameTargetBucket = ComputeStickyIntegralTargetBucket( Adaptation );
			float	Integral = 0.0;
			for ( int BucketIndex=0; BucketIndex < TARGET_MONITOR_BUCKETS_COUNT; BucketIndex++ ) {
				uint	CurrentHistoValue = _texHistogram[uint2( HistoBucketIndex.x+BucketIndex, 0)].x;
				float	Weight = ComputeStickyIntegralWeight( BucketIndex - CurrentFrameTargetBucket );

				Integral += Weight * CurrentHistoValue;
			}
			Integral *= 8.0 / (AveragePixelsCount * TARGET_MONITOR_BUCKETS_COUNT);

//Integral = 0.2 * ComputeStickyIntegralWeight( fHistoBucketIndex - CurrentFrameTargetBucket );

			BackgroundColor = lerp( _UV.y < Integral ? float3( 0.5, 0.3, 0.0 ) : BackgroundColor, BackgroundColor, 0.5 );
		#endif

		#if 1	// ==== Paint graduation values ====
			BackgroundColor = DrawGraduationText( _UV, 0.1, 1 ) > 0.5 ? 0 : BackgroundColor;
			BackgroundColor = DrawGraduationText( _UV, 1.0, 2 ) > 0.5 ? 0 : BackgroundColor;
			BackgroundColor = DrawGraduationText( _UV, 10.0, 3 ) > 0.5 ? 0 : BackgroundColor;
			BackgroundColor = DrawGraduationText( _UV, 100.0, 4 ) > 0.5 ? 0 : BackgroundColor;
			BackgroundColor = DrawGraduationText( _UV, 1000.0, 5 ) > 0.5 ? 0 : BackgroundColor;
		#endif

		BackgroundColor = lerp( GraduationColor, BackgroundColor, IsGraduation );
		BackgroundColor = lerp( BackgroundColor, float3( 1.0, 0.8, 0.2 ), IsDebug );		// Paint orange line where our debug luminance is

		float3	HistoColor = lerp( float3( 0, 0, 0 ), float3( 0.8, 0, 0 ), InRange );
		// Paint the peak
		_Color = _UV.y < 1.0 - 0.002 - HistoValue ? BackgroundColor : HistoColor;
	}
}

float	DrawGraduationText( float2 _UV, float _Value, int _GraduationIndex ) {
	const float	TEXT_SIZE_Y = 0.04;									// 6 pixels high will span this UV verticaly
	const float	PIXEL_SIZE = TEXT_SIZE_Y / DEBUG_DRAW_DIGIT_HEIGHT;	// Pixel size in UV space
	float		TextSizeX = measureStringWidth( _Value ) * PIXEL_SIZE;

	float2	Pos = float2( -0.5 * TextSizeX + _GraduationIndex * 1.0 / 6.0, 0.01 );
	return drawNumber( _Value, _UV, Pos, Pos + float2( TextSizeX, TEXT_SIZE_Y ) );
}
