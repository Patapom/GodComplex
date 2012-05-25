//--------------------------------------------------------------------------//
// iq . 2003/2008 . code for 64 kb intros by RGBA						   //
//--------------------------------------------------------------------------//
#pragma once

#define KEY_LEFT		0
#define KEY_RIGHT		1
#define KEY_UP			2
#define KEY_DOWN		3
#define KEY_SPACE		4
#define KEY_PGUP		5
#define KEY_PGDOWN		6

#define KEY_0			40
#define KEY_1			41
#define KEY_2			42
#define KEY_3			43
#define KEY_4			44
#define KEY_5			45
#define KEY_6			46
#define KEY_7			47
#define KEY_8			48
#define KEY_9			49

#define KEY_A			65
#define KEY_B			66
#define KEY_C			67
#define KEY_D			68
#define KEY_E			69
#define KEY_F			70
#define KEY_G			71
#define KEY_H			72
#define KEY_I			73
#define KEY_J			74
#define KEY_K			75
#define KEY_L			76
#define KEY_M			77
#define KEY_N			78
#define KEY_O			79
#define KEY_P			80
#define KEY_Q			81
#define KEY_R			82
#define KEY_S			83
#define KEY_T			84
#define KEY_U			85
#define KEY_V			86
#define KEY_W			87
#define KEY_X			88
#define KEY_Y			89
#define KEY_Z			90

#define KEY_LSHIFT		100
#define KEY_RSHIFT		101
#define KEY_LCONTROL	102
#define KEY_RCONTROL	103

typedef struct
{
	int	State[256];
	int Press[256];

} MSYS_INPUT_KEYBORAD;

typedef struct
{
	int	dx, dy;			// Delta
	int	x, y, ox, oy;	// Current/Old position
	int	obuttons[2];	// Previous buttons
	int	buttons[2];		// Current buttons
	int	dbuttons[2];	// Delta buttons
} MSYS_INPUT_MOUSE;

typedef struct
{
	MSYS_INPUT_KEYBORAD	Keyboard;
	MSYS_INPUT_MOUSE	Mouse;
} MSYS_EVENTINFO;
