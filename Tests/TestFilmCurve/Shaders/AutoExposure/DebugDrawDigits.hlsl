/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Displays a number using shader code :)
// Based on the nice ascii shader by movAX13h
//
static const float	DEBUG_DRAW_DIGIT_HEIGHT = 6.0;										// A single digit is 6 pixels high
static const float	DEBUG_DRAW_NUMBER_ASPECT_RATIO = 38.0 / DEBUG_DRAW_DIGIT_HEIGHT;	// Text is max 38 pixels wide and 6 pixels high
static const float	DEBUG_DRAW_DIGIT_ASPECT_RATIO = 4.0 / DEBUG_DRAW_DIGIT_HEIGHT;		// A single digit is 4 pixels wide

// Usage:
// Call MyColor += drawNumber( NumberToDisplay, PixelPosition, BottomLeft )
//	
//	_Position, the current screen position (in pixels)
//	_BottomLeft, is the coordinates of the bottom left corner of the string rectangle (in pixels)
//
float	drawNumber( float _Number, float2 _Position, float2 _BottomLeft );	// Don't use that! It's only a pre-declaration! Use the function below...

// Measures the amount of pixels that will be used to draw the number
uint	measureStringWidth( float _Number ) {
	uint	width = 0;
	bool	on = false;

	// minus sign
	if( _Number < 0.0 ) {
		width += 4;
		_Number = -_Number;
	}
	// tens of thousands
	float d = floor(fmod(_Number/10000.,10.));
	if( on || d > 1e-3 ) {
		width += 4;
		on = true;
	}
	// thousands
	d = floor(fmod(_Number/1000.,10.));
	if( on || d > 1e-3 ) {
		width += 4;
		on = true;
	}
	// hundreds
	d = floor(fmod(_Number/100.,10.));
	if( on || d > 1e-3 ) {
		width += 4;
		on = true;
	}
	// tens
	d = floor(fmod(_Number/10.,10.));
	if( on || d > 1e-3 ) {
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

//-------------------------------------------------------------------------------------------------------------------------------
// Pre-declaration
float drawDecPt( float2 center, float2 _BottomLeft );
float drawMinus( float2 center, float2 _BottomLeft );
float drawDigit( float dig, float2 _Position, float2 _BottomLeft );

// max num width is 38px (minus, 5 nums, dec pt, 3 nums)
// max height is 6px
float	drawNumber( float _Number, float2 _Position, float2 _BottomLeft ) {
	float	result = 0.;
	bool	on = false;
	float	d;
	
	// minus sign
	if( _Number < 0.0 ) {
		result += drawMinus( _Position, _BottomLeft );
		_Position.x -= 4.0;
		_Number = -_Number;
	}
	// tens of thousands
	d = floor(fmod(_Number/10000.,10.));
	if( on || d > 1e-3 ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
		on = true;
	}
	// thousands
	d = floor(fmod(_Number/1000.,10.));
	if( on || d > 1e-3 ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
		on = true;
	}
	// hundreds
	d = floor(fmod(_Number/100.,10.));
	if( on || d > 1e-3 ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
		on = true;
	}
	// tens
	d = floor(fmod(_Number/10.,10.));
	if( on || d > 1e-3 ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
		on = true;
	}
	// ones
	d = floor(fmod(_Number,10.));
	result += drawDigit( d, _Position, _BottomLeft );
	_Position.x -= 4.0;
	// dec pt
	result += drawDecPt( _Position, _BottomLeft );
	_Position.x -= 2.0;
	// tenths
	d = floor(fmod(10.0*_Number,10.));
	if( true ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
	}
	// hundredths
	d = floor(.5+fmod(100.0*_Number,10.));
	if( d > 1e-3 ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
	}
	// thousandths
	d = floor(.5+fmod(1000.0*_Number,10.));
	if( d > 1e-3 ) {
		result += drawDigit( d, _Position, _BottomLeft );
		_Position.x -= 4.0;
	}
	
	return saturate( result );
}

float drawDig( float2 _Position, float2 _BottomLeft, uint bitfield ) {
	// offset relative to 
	float2	ic = _BottomLeft - _Position;
			ic.y -= 1.0;
			ic = floor(ic);

	// test if we're currently standing in the digit's rectangle
	if ( clamp(ic.x, 0.0, 2.0) == ic.x && clamp(ic.y, 0.0, 5.0) == ic.y ) {
		// compute 1d bitindex from 2d _Position
		uint	bitIndex = uint( ic.y*3.0 + (2.0-ic.x) );
		// isolate the bit
//		return floor( fmod( bitfield / exp2( floor(bitIndex) ), 2. ) );
		return (bitfield >> bitIndex) & 1;
	}
	return 0.0;
}
// decimal point
float drawDecPt( float2 center, float2 _BottomLeft ) {
	return drawDig( center, _BottomLeft, 1U );
}
// minus sign
float drawMinus( float2 center, float2 _BottomLeft ) {
	return drawDig( center, _BottomLeft, 448U );
}
// digits 0 to 9
float drawDigit( float dig, float2 _Position, float2 _BottomLeft ) {
	if( dig == 1.0 ) return drawDig( _Position, _BottomLeft, 18724U );
	if( dig == 2.0 ) return drawDig( _Position, _BottomLeft, 31183U );
	if( dig == 3.0 ) return drawDig( _Position, _BottomLeft, 31207U );
	if( dig == 4.0 ) return drawDig( _Position, _BottomLeft, 23524U );
	if( dig == 5.0 ) return drawDig( _Position, _BottomLeft, 29671U );
	if( dig == 6.0 ) return drawDig( _Position, _BottomLeft, 29679U );
	if( dig == 7.0 ) return drawDig( _Position, _BottomLeft, 31012U );
	if( dig == 8.0 ) return drawDig( _Position, _BottomLeft, 31727U );
	if( dig == 9.0 ) return drawDig( _Position, _BottomLeft, 31719U );
	// 0
	return drawDig( _Position, _BottomLeft, 31599U );
}
