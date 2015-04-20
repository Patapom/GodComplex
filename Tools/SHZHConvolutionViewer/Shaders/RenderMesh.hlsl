
cbuffer CB_Camera : register( b0 ) {
	float4x4	_World2Proj;
};

cbuffer CB_Mesh : register( b1 ) {
	float4		_SH0;
	float4		_SH1;

	float		_SH2;
	float3		_ZH;

	float4		_resultSH0;
	float4		_resultSH1;

	float		_resultSH2;
	uint		_flags;
};

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Normal : NORMAL;
};

float3	EvaluateSH( float3 _Direction, float _SH[9] )
{
	const float	f0 = 0.28209479177387814347403972578039;		// 0.5 / sqrt(PI);
	const float	f1 = 0.48860251190291992158638462283835;		// 0.5 * sqrt(3/PI);
	const float	f2 = 1.0925484305920790705433857058027;			// 0.5 * sqrt(15/PI);
	const float	f3 = 0.31539156525252000603089369029571;		// 0.25 * sqrt(5.PI);

	float	EvalSH0 = f0;
	float4	EvalSH1234, EvalSH5678;
	EvalSH1234.x = f1 * _Direction.y;
	EvalSH1234.y = f1 * _Direction.z;
	EvalSH1234.z = f1 * _Direction.x;
	EvalSH1234.w = f2 * _Direction.x * _Direction.y;
	EvalSH5678.x = f2 * _Direction.y * _Direction.z;
	EvalSH5678.y = f3 * (3.0 * _Direction.z*_Direction.z - 1.0);
	EvalSH5678.z = f2 * _Direction.x * _Direction.z;
	EvalSH5678.w = f2 * 0.5 * (_Direction.x*_Direction.x - _Direction.y*_Direction.y);

	// Dot the SH together
	return max( 0.0,
			  EvalSH0	   * _SH[0]
			+ EvalSH1234.x * _SH[1]
			+ EvalSH1234.y * _SH[2]
			+ EvalSH1234.z * _SH[3]
			+ EvalSH1234.w * _SH[4]
			+ EvalSH5678.x * _SH[5]
			+ EvalSH5678.y * _SH[6]
			+ EvalSH5678.z * _SH[7]
			+ EvalSH5678.w * _SH[8] );
}

// Rotates ZH coefficients in the specified direction (from "Stupid SH Tricks")
// Rotating ZH comes to evaluating scaled SH in the given direction.
// The scaling factors for each band are equal to the ZH coefficients multiplied by sqrt( 4PI / (2l+1) )
//
void ZHRotate( const in float3 _Direction, const in float3 _ZHCoeffs, out float _Coeffs[9] )
{
	float	cl0 = 3.5449077018110320545963349666823 * _ZHCoeffs.x;	// sqrt(4PI)
	float	cl1 = 2.0466534158929769769591032497785 * _ZHCoeffs.y;	// sqrt(4PI/3)
	float	cl2 = 1.5853309190424044053380115060481 * _ZHCoeffs.z;	// sqrt(4PI/5)

	float	f0 = cl0 * 0.28209479177387814347403972578039;	// 0.5 / sqrt(PI);
	float	f1 = cl1 * 0.48860251190291992158638462283835;	// 0.5 * sqrt(3/PI);
	float	f2 = cl2 * 1.0925484305920790705433857058027;	// 0.5 * sqrt(15/PI);
	float	f3 = cl2 * 0.31539156525252000603089369029571;	// 0.25 * sqrt(5.PI);

	_Coeffs[0] = f0;
	_Coeffs[1] = f1 * _Direction.y;
	_Coeffs[2] = f1 * _Direction.z;
	_Coeffs[3] = f1 * _Direction.x;
	_Coeffs[4] = f2 * _Direction.x * _Direction.y;
	_Coeffs[5] = f2 * _Direction.y * _Direction.z;
	_Coeffs[6] = f3 * (3.0 * _Direction.z*_Direction.z - 1.0);
	_Coeffs[7] = f2 * _Direction.x * _Direction.z;
	_Coeffs[8] = f2 * 0.5 * (_Direction.x*_Direction.x - _Direction.y*_Direction.y);
}

float	EvaluateDistance( float3 _Direction ) {
	float	SH[9] = { _SH0.x, _SH0.y, _SH0.z, _SH0.w, _SH1.x, _SH1.y, _SH1.z, _SH1.w, _SH2 };
	float	resultSH[9] = { _resultSH0.x, _resultSH0.y, _resultSH0.z, _resultSH0.w, _resultSH1.x, _resultSH1.y, _resultSH1.z, _resultSH1.w, _resultSH2 };

	float	Theta = acos( _Direction.z );
	float	Phi = atan2( _Direction.y, _Direction.x );

	float	Distance = 1.0;
	switch ( _flags ) {
	case 0:
//		Distance = 0.1 + 0.45 * (1.0 - cos( 2.0 * Theta ) * cos( 1.0 * Phi ));
		Distance = 0.5 * (1.0 + _Direction.x);
		break;
	case 1:
		Distance = EvaluateSH( _Direction, SH );
		break;
	}

	return Distance;
}

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	float3	Direction = float3( _In.Position.x, -_In.Position.z, _In.Position.y );	// Z up!
	float3	Position = EvaluateDistance( Direction ) * _In.Position;

	float4	wsPosition = float4( Position, 1.0 );
	Out.__Position = mul( wsPosition, _World2Proj );
	Out.Normal = Direction;

	return Out;
}
float3	PS( PS_IN _In ) : SV_TARGET0 {
	float	Distance = EvaluateDistance( _In.Normal );

	return Distance;
	return abs( _In.Normal );
}