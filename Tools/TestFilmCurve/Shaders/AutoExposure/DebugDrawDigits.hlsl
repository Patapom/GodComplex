/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Displays a number using shader code :)
// Based on the nice ascii shader by movAX13h
//
static const float	DEBUG_DRAW_DIGIT_HEIGHT = 6.0;										// A single digit is 6 pixels high
static const float	DEBUG_DRAW_NUMBER_ASPECT_RATIO = 34.0 / DEBUG_DRAW_DIGIT_HEIGHT;	// Text is max 34 pixels wide and 6 pixels high
static const float	DEBUG_DRAW_DIGIT_ASPECT_RATIO = 4.0 / DEBUG_DRAW_DIGIT_HEIGHT;		// A single digit is 4 pixels wide

float	drawNumber( float num, float2 pos, float2 pixel_coords );	// Don't use that! It's only a pre-declaration! Use the function below...

// Usage:
// Call MyColor += drawNumber( NumberToDisplay, UV, BottomLeft, TopRight )
//	
//	_UV, the current screen UV
//	_TopLeft, is the UV coordinates of the top left corner of the string rectangle
//	_BottomRight, is the UV coordinates of the bottom right corner of the string rectangle
//
float	drawNumber( float _Number, float2 _UV, float2 _TopLeft, float2 _BottomRight )	// Use that!
{
	float2	BottomLeft = float2( _TopLeft.x, _BottomRight.y );
	float2	TopRight = float2( _BottomRight.x, _TopLeft.y );
	float2	PixelCoord = float2( 34.0, 6.0 ) * (_UV - BottomLeft) / (TopRight - BottomLeft);
	return drawNumber( _Number, 0.0, PixelCoord );
}

//-------------------------------------------------------------------------------------------------------------------------------
// Pre-declaration
float drawDecPt( float2 center, float2 pixel_coords );
float drawMinus( float2 center, float2 pixel_coords );
float drawDigit( float dig, float2 pos, float2 pixel_coords );

// Measures the amount of pixels that will be used to draw the number
uint	measureStringWidth( float _Number )
{
	uint	width = 0;
	bool	on = false;

	// minus sign
	if( _Number < 0.0 )
	{
		width += 4;
		_Number = -_Number;
	}
	// thousands
	float d = floor(fmod(_Number/1000.,10.));
	if( on || d > 1e-3 )
	{
		width += 4;
		on = true;
	}
	// hundreds
	d = floor(fmod(_Number/100.,10.));
	if( on || d > 1e-3 )
	{
		width += 4;
		on = true;
	}
	// tens
	d = floor(fmod(_Number/10.,10.));
	if( on || d > 1e-3 )
	{
		width += 4;
		on = true;
	}
	// ones
	d = floor(fmod(10.0*_Number,10.));
	width += 4;
	// dec pt
	width += 2;
	// tenths
	width += 4;
	// hundredths
	d = floor(.5+fmod(100.0*_Number,10.));
	if( d > 1e-3 )
		width += 4;
	// thousandths
	d = floor(.5+fmod(1000.0*_Number,10.));
	if( d > 1e-3 )
		width += 4;

	return width;
}

// max num width is 34px (minus, 4 nums, dec pt, 3 nums)
// max height is 6px
float	drawNumber( float _Number, float2 pos, float2 pixel_coords )
{
	float result = 0.;
	bool on = false;
	float d;
	
	// minus sign
	if( _Number < 0.0 )
	{
		result += drawMinus( pos, pixel_coords );
		pos.x += 4.0;
		_Number = -_Number;
	}
	// thousands
	d = floor(fmod(_Number/1000.,10.));
	if( on || d > 1e-3 )
	{
		result += drawDigit( d, pos, pixel_coords );
		pos.x += 4.0;
		on = true;
	}
	// hundreds
	d = floor(fmod(_Number/100.,10.));
	if( on || d > 1e-3 )
	{
		result += drawDigit( d, pos, pixel_coords );
		pos.x += 4.0;
		on = true;
	}
	// tens
	d = floor(fmod(_Number/10.,10.));
	if( on || d > 1e-3 )
	{
		result += drawDigit( d, pos, pixel_coords );
		pos.x += 4.0;
		on = true;
	}
	// ones
	d = floor(fmod(_Number,10.));
	result += drawDigit( d, pos, pixel_coords );
	pos.x += 4.0;
	// dec pt
	result += drawDecPt( pos, pixel_coords );
	pos.x += 2.0;
	// tenths
	d = floor(fmod(10.0*_Number,10.));
	if( true )
	{
		result += drawDigit( d, pos, pixel_coords );
		pos.x += 4.0;
	}
	// hundredths
	d = floor(.5+fmod(100.0*_Number,10.));
	if( d > 1e-3 )
	{
		result += drawDigit( d, pos, pixel_coords );
		pos.x += 4.0;
	}
	// thousandths
	d = floor(.5+fmod(1000.0*_Number,10.));
	if( d > 1e-3 )
	{
		result += drawDigit( d, pos, pixel_coords );
		pos.x += 4.0;
	}
	
	return saturate( result );
}

float drawDig( float2 pos, float2 pixel_coords, uint bitfield )
{
	// offset relative to 
	float2 ic = pixel_coords - pos ;
	ic = floor(ic);
	// test if overlap letter
	if( clamp(ic.x, 0., 2.) == ic.x && clamp(ic.y, 0., 4.) == ic.y )
	{
		// compute 1d bitindex from 2d pos
		uint bitIndex = uint( ic.y*3.0+ic.x );
		// isolate the bit
//				return floor( fmod( bitfield / exp2( floor(bitIndex) ), 2. ) );
		return (bitfield >> bitIndex) & 1;
	}
	return 0.;
}
// decimal point
float drawDecPt( float2 center, float2 pixel_coords )
{
	return drawDig( center, pixel_coords, 1 );
}
// minus sign
float drawMinus( float2 center, float2 pixel_coords )
{
	return drawDig( center, pixel_coords, 448 );
}
// digits 0 to 9
float drawDigit( float dig, float2 pos, float2 pixel_coords )
{
	if( dig == 1. )
		return drawDig( pos, pixel_coords, 18724 );
	if( dig == 2. )
		return drawDig( pos, pixel_coords, 31183 );
	if( dig == 3. )
		return drawDig( pos, pixel_coords, 31207 );
	if( dig == 4. )
		return drawDig( pos, pixel_coords, 23524 );
	if( dig == 5. )
		return drawDig( pos, pixel_coords, 29671 );
	if( dig == 6. )
		return drawDig( pos, pixel_coords, 29679 );
	if( dig == 7. )
		return drawDig( pos, pixel_coords, 31012 );
	if( dig == 8. )
		return drawDig( pos, pixel_coords, 31727 );
	if( dig == 9. )
		return drawDig( pos, pixel_coords, 31719 );
	// 0
	return drawDig( pos, pixel_coords, 31599 );
}
