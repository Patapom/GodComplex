#include "../Types.h"

using namespace BaseLib;

const bfloat2	bfloat2::Zero( 0, 0 );
const bfloat2	bfloat2::One( 1, 1 );
const bfloat2	bfloat2::UnitX( 1, 0 );
const bfloat2	bfloat2::UnitY( 0, 1 );

const bfloat3	bfloat3::Zero( 0, 0, 0 );
const bfloat3	bfloat3::One( 1, 1, 1 );
const bfloat3	bfloat3::MaxFlt( MAX_FLOAT, MAX_FLOAT, MAX_FLOAT );
const bfloat3	bfloat3::UnitX( 1, 0, 0 );
const bfloat3	bfloat3::UnitY( 0, 1, 0 );
const bfloat3	bfloat3::UnitZ( 0, 0, 1 );

const bfloat4	bfloat4::Zero( 0, 0, 0, 0 );
const bfloat4	bfloat4::One( 1, 1, 1, 1 );
const bfloat4	bfloat4::UnitX( 1, 0, 0, 0 );
const bfloat4	bfloat4::UnitY( 0, 1, 0, 0 );
const bfloat4	bfloat4::UnitZ( 0, 0, 1, 0 );
const bfloat4	bfloat4::UnitW( 0, 0, 0, 1 );


// (2016-01-04) Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
// This code doesn't involve any square root!
void bfloat3::OrthogonalBasis( bfloat3& _left, bfloat3& _up ) const {
	if ( z < -0.9999999f ) {
		// Handle the singularity
		_left.Set( 0.0f, -1.0f, 0.0f );
		_up.Set( -1.0f, 0.0f, 0.0f );
		return;
	}

	const float	a = 1.0f / (1.0f + z);
	const float	b = -x*y*a;
	_left.Set( 1.0f - x*x*a, b, -x );
	_up.Set( b, 1.0f - y*y*a, -y );
}

bfloat4	bfloat4::QuatFromAngleAxis( float _angle, const bfloat3& _axis ) {
	bfloat3	NormalizedAxis = _axis;
			NormalizedAxis.Normalize();

	_angle *= 0.5f;

	float	c = cosf(_angle);
	float	s = sinf(_angle);

	return bfloat4( s * NormalizedAxis, c );
}


//////////////////////////////////////////////////////////////////////////
// float3x3
const float3x3	float3x3::Zero( 0, 0, 0, 0, 0, 0, 0, 0, 0 );
const float3x3	float3x3::Identity( 1, 0, 0, 0, 1, 0, 0, 0, 1 );

float	float3x3::Determinant() const {
	return (r[0].x*r[1].y*r[2].z + r[0].y*r[1].z*r[2].x + r[0].z*r[1].x*r[2].y) - (r[2].x*r[1].y*r[0].z + r[2].y*r[1].z*r[0].x + r[2].z*r[1].x*r[0].y);
}

float3x3  float3x3::Inverse() const {
	float	det = Determinant();
	ASSERT( abs(det) > 1e-6f, "Matrix is not inversible!" );
	det = 1.0f / det;

	float3x3	R;
	R.r[0].Set( +(r[1].y * r[2].z - r[2].y * r[1].z) * det,
				-(r[0].y * r[2].z - r[2].y * r[0].z) * det,
				+(r[0].y * r[1].z - r[1].y * r[0].z) * det );
	R.r[1].Set( -(r[1].x * r[2].z - r[2].x * r[1].z) * det,
				+(r[0].x * r[2].z - r[2].x * r[0].z) * det,
				-(r[0].x * r[1].z - r[1].x * r[0].z) * det );
	R.r[2].Set( +(r[1].x * r[2].y - r[2].x * r[1].y) * det,
				-(r[0].x * r[2].y - r[2].x * r[0].y) * det,
				+(r[0].x * r[1].y - r[1].x * r[0].y) * det );

	return R;
}

float3x3&	float3x3::BuildFromQuat( const bfloat4& _Quat ) {
	bfloat4		q = _Quat;
	q.Normalize();

	float	xs = 2.0f * q.x;
	float	ys = 2.0f * q.y;
	float	zs = 2.0f * q.z;

	float	wx, wy, wz, xx, xy, xz, yy, yz, zz;
	wx = q.w * xs;	wy = q.w * ys;	wz = q.w * zs;
	xx = q.x * xs;	xy = q.x * ys;	xz = q.x * zs;
	yy = q.y * ys;	yz = q.y * zs;	zz = q.z * zs;

	r[0].x = 1.0f - yy - zz;
	r[0].y =        xy + wz;
	r[0].z =        xz - wy;

	r[1].x =        xy - wz;
	r[1].y = 1.0f - xx - zz;
	r[1].z =        yz + wx;

	r[2].x =        xz + wy;
	r[2].y =        yz - wx;
	r[2].z = 1.0f - xx - yy;

	return *this;
}

float3x3&	float3x3::BuildRot( const bfloat3& _Source, const bfloat3& _Target ) {
	bfloat3	Ortho = _Source.Cross( _Target );
	float	Length = Ortho.Length();
	if ( Length > 1e-6f )
		Ortho = Ortho / Length;
	else
		Ortho.Set( 1, 0, 0 );

	float	Angle = asinf( Length );
	return BuildFromAngleAxis( Angle, Ortho );
}

// (2016-01-06) Builds the remaining 2 orthogonal vectors from a given vector (very fast! no normalization or square root involved!)
// Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
float3x3& float3x3::BuildRot( const bfloat3& _vector ) {
	r[0] = _vector;
	_vector.OrthogonalBasis( r[1], r[2] );
	return *this;
}

float3x3&	float3x3::BuildRotX( float _Angle ) {
	*this = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	r[1].y = C;		r[1].z = S;
	r[2].y = -S;	r[2].z = C;

	return *this;
}
float3x3&	float3x3::BuildRotY( float _Angle ) {
	*this = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	r[0].x = C;	r[0].z = -S;
	r[2].x = S;	r[2].z = C;

	return *this;
}
float3x3&	float3x3::BuildRotZ( float _Angle ) {
	*this = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	r[0].x = C;		r[0].y = S;
	r[1].x = -S;	r[1].y = C;

	return *this;
}

float3x3&	float3x3::BuildPYR( float _Pitch, float _Yaw, float _Roll ) {
	float3x3	Pitch, Yaw, Roll;
	Pitch.BuildRotX( _Pitch );
	Yaw.BuildRotY( _Yaw );
	Roll.BuildRotZ( _Roll );

	*this = Pitch * Yaw * Roll;
	return *this;
}


bfloat3   operator*( const bfloat3& a, const float3x3& b ) {
	bfloat3	R;
	R.x = a.x * b.r[0].x + a.y * b.r[1].x + a.z * b.r[2].x;
	R.y = a.x * b.r[0].y + a.y * b.r[1].y + a.z * b.r[2].y;
	R.z = a.x * b.r[0].z + a.y * b.r[1].z + a.z * b.r[2].z;

	return R;
}

bfloat3   operator*( const float3x3& b, const bfloat3& a ) {
	bfloat3	R;
	R.x = a.Dot( b.r[0] );
	R.y = a.Dot( b.r[1] );
	R.z = a.Dot( b.r[2] );
	return R;
}

float3x3	float3x3::operator*( float a ) const {
	float3x3	R;
	R.r[0].Set( a*r[0].x, a*r[0].y, a*r[0].z );
	R.r[1].Set( a*r[1].x, a*r[1].y, a*r[1].z );
	R.r[2].Set( a*r[2].x, a*r[2].y, a*r[2].z );
	return R;
}
float3x3	operator*( const float3x3& b, float a ) {
	float3x3	R;
	R.r[0].Set( a*b.r[0].x, a*b.r[0].y, a*b.r[0].z );
	R.r[1].Set( a*b.r[1].x, a*b.r[1].y, a*b.r[1].z );
	R.r[2].Set( a*b.r[2].x, a*b.r[2].y, a*b.r[2].z );
	return R;
}

float3x3  float3x3::operator*( const float3x3& b ) const {
	float3x3  R;
	R.r[0].Set( r[0].x*b.r[0].x + r[0].y*b.r[1].x + r[0].z*b.r[2].x, /**/ r[0].x*b.r[0].y + r[0].y*b.r[1].y + r[0].z*b.r[2].y, /**/ r[0].x*b.r[0].z + r[0].y*b.r[1].z + r[0].z*b.r[2].z );
	R.r[1].Set( r[1].x*b.r[0].x + r[1].y*b.r[1].x + r[1].z*b.r[2].x, /**/ r[1].x*b.r[0].y + r[1].y*b.r[1].y + r[1].z*b.r[2].y, /**/ r[1].x*b.r[0].z + r[1].y*b.r[1].z + r[1].z*b.r[2].z );
	R.r[2].Set( r[2].x*b.r[0].x + r[2].y*b.r[1].x + r[2].z*b.r[2].x, /**/ r[2].x*b.r[0].y + r[2].y*b.r[1].y + r[2].z*b.r[2].y, /**/ r[2].x*b.r[0].z + r[2].y*b.r[1].z + r[2].z*b.r[2].z );

	return R;
}

float&	float3x3::operator()( int _row, int _column ) {
	bfloat3&	row = r[_row%3];
	switch ( _column%3 ) {
		case 0: return row.x;
		case 1: return row.y;
		case 2: return row.z;
	}
	return *((float*) 0);
}


//////////////////////////////////////////////////////////////////////////
// float4x4
const float4x4	float4x4::Zero( 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 );
const float4x4	float4x4::Identity( 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 );

float4x4 float4x4::Inverse() const {
	float	det = Determinant();
	ASSERT( abs(det) > 1e-6f, "Matrix is not inversible!" );
	det = 1.0f / det;

	float4x4	R;
	R.r[0].Set( CoFactor( 0, 0 ) * det, CoFactor( 1, 0 ) * det, CoFactor( 2, 0 ) * det, CoFactor( 3, 0 ) * det );
	R.r[1].Set( CoFactor( 0, 1 ) * det, CoFactor( 1, 1 ) * det, CoFactor( 2, 1 ) * det, CoFactor( 3, 1 ) * det );
	R.r[2].Set( CoFactor( 0, 2 ) * det, CoFactor( 1, 2 ) * det, CoFactor( 2, 2 ) * det, CoFactor( 3, 2 ) * det );
	R.r[3].Set( CoFactor( 0, 3 ) * det, CoFactor( 1, 3 ) * det, CoFactor( 2, 3 ) * det, CoFactor( 3, 3 ) * det );

	return R;
}

float	float4x4::Determinant() const {
	return r[0].x * CoFactor( 0, 0 ) + r[0].y * CoFactor( 0, 1 ) + r[0].z * CoFactor( 0, 2 ) + r[0].w * CoFactor( 0, 3 ); 
}

float	float4x4::CoFactor( int _row, int _col ) const {
	int		row1 = (_row+1) & 3;
	int		row2 = (_row+2) & 3;
	int		row3 = (_row+3) & 3;
	float	sign = float( (((_row + _col) & 1) << 1) - 1 );
	return	((	r[row1][_col+1] * r[row2][_col+2] * r[row3][_col+3] +
				r[row1][_col+2] * r[row2][_col+3] * r[row3][_col+1] +
				r[row1][_col+3] * r[row2][_col+1] * r[row3][_col+2] )

			-(	r[row3][_col+1] * r[row2][_col+2] * r[row1][_col+3] +
				r[row3][_col+2] * r[row2][_col+3] * r[row1][_col+1] +
				r[row3][_col+3] * r[row2][_col+1] * r[row1][_col+2] ))
			* sign;
}

float4x4&	float4x4::Normalize() {
	((bfloat3&) r[0]).Normalize();
	((bfloat3&) r[1]).Normalize();
	((bfloat3&) r[2]).Normalize();
	return *this;
}

float4x4&	float4x4::Scale( const bfloat3& _scale ) {
	r[0] *= _scale.x;
	r[1] *= _scale.y;
	r[2] *= _scale.z;
	return *this;
}

bfloat4   operator*( const bfloat4& a, const float4x4& b ) {
	bfloat4	R;
	R.x = a.x * b.r[0].x + a.y * b.r[1].x + a.z * b.r[2].x + a.w * b.r[3].x;
	R.y = a.x * b.r[0].y + a.y * b.r[1].y + a.z * b.r[2].y + a.w * b.r[3].y;
	R.z = a.x * b.r[0].z + a.y * b.r[1].z + a.z * b.r[2].z + a.w * b.r[3].z;
	R.w = a.x * b.r[0].w + a.y * b.r[1].w + a.z * b.r[2].w + a.w * b.r[3].w;

	return R;
}

bfloat4   operator*( const float4x4& b, const bfloat4& a ) {
	bfloat4	R;
	R.x = a.Dot( b.r[0] );
	R.y = a.Dot( b.r[1] );
	R.z = a.Dot( b.r[2] );
	R.w = a.Dot( b.r[3] );
	return R;
}

float4x4&	float4x4::BuildFromQuat( const bfloat4& _Quat ) {
	bfloat4	q = _Quat;
	q.Normalize();

	float	xs = 2.0f * q.x;
	float	ys = 2.0f * q.y;
	float	zs = 2.0f * q.z;

	float	wx, wy, wz, xx, xy, xz, yy, yz, zz;
	wx = q.w * xs;	wy = q.w * ys;	wz = q.w * zs;
	xx = q.x * xs;	xy = q.x * ys;	xz = q.x * zs;
	yy = q.y * ys;	yz = q.y * zs;	zz = q.z * zs;

	r[0].x = 1.0f - yy - zz;
	r[0].y =        xy + wz;
	r[0].z =        xz - wy;
	r[0].w = 0.0f;

	r[1].x =        xy - wz;
	r[1].y = 1.0f - xx - zz;
	r[1].z =        yz + wx;
	r[1].w = 0.0f;

	r[2].x =        xz + wy;
	r[2].y =        yz - wx;
	r[2].z = 1.0f - xx - yy;
	r[2].w = 0.0f;

	r[3].x = 0.0f;
	r[3].y = 0.0f;
	r[3].z = 0.0f;
	r[3].w = 1.0f;

	return *this;
}

float4x4&	float4x4::BuildProjectionPerspective( float _FOVY, float _aspectRatio, float _near, float _far ) {
	float	H = tanf( 0.5f * _FOVY );
	float	W = _aspectRatio * H;
	float	Q =  _far / (_far - _near);

	r[0].Set( 1.0f / W, 0.0f, 0.0f, 0.0f );
	r[1].Set( 0.0f, 1.0f / H, 0.0f, 0.0f );
	r[2].Set( 0.0f, 0.0f, Q, 1.0f );
	r[3].Set( 0.0f, 0.0f, -_near * Q, 0.0f );

	return *this;
}

float4x4&	float4x4::BuildPRS( const bfloat3& P, const bfloat4& R, const bfloat3& S ) {
	BuildFromQuat( R );

	r[0].x *= S.x;
	r[0].y *= S.x;
	r[0].z *= S.x;
	r[0].w *= S.x;
	r[1].x *= S.y;
	r[1].y *= S.y;
	r[1].z *= S.y;
	r[1].w *= S.y;
	r[2].x *= S.z;
	r[2].y *= S.z;
	r[2].z *= S.z;
	r[2].w *= S.z;
	r[3].x = P.x;
	r[3].y = P.y;
	r[3].z = P.z;

	return *this;
}

float4x4&	float4x4::BuildRot( const bfloat3& _Source, const bfloat3& _Target ) {
	bfloat3	Ortho = _Source.Cross( _Target );
	float	Length = Ortho.Length();
	if ( Length > 1e-6f )
		Ortho = Ortho / Length;
	else
		Ortho.Set( 1, 0, 0 );

	float	Angle = asinf( Length );
	return BuildFromAngleAxis( Angle, Ortho );
}

// (2016-01-06) Builds the remaining 2 orthogonal vectors from a given vector (very fast! no normalization or square root involved!)
// Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
float4x4& float4x4::BuildRot( const bfloat3& _vector ) {
	r[0].Set( _vector, 0.0f );
	_vector.OrthogonalBasis( (bfloat3&) r[1], (bfloat3&) r[2] );
	r[1].w = 0.0f;
	r[2].w = 0.0f;
	r[3].Set( 0, 0, 0, 1 );
	return *this;
}

float4x4&	float4x4::BuildRotX( float _Angle ) {
	*this = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	r[1].y = C;		r[1].z = S;
	r[2].y = -S;	r[2].z = C;

	return *this;
}
float4x4&	float4x4::BuildRotY( float _Angle ) {
	*this = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	r[0].x = C;	r[0].z = -S;
	r[2].x = S;	r[2].z = C;

	return *this;
}
float4x4&	float4x4::BuildRotZ( float _Angle ) {
	*this = Identity;

	float C = cosf( _Angle );
	float S = sinf( _Angle );
	r[0].x = C;		r[0].y = S;
	r[1].x = -S;	r[1].y = C;

	return *this;
}
float4x4&	float4x4::BuildPYR( float _Pitch, float _Yaw, float _Roll ) {
	float4x4	Pitch, Yaw, Roll;
	Pitch.BuildRotX( _Pitch );
	Yaw.BuildRotY( _Yaw );
	Roll.BuildRotZ( _Roll );

	*this = Pitch * Yaw * Roll;
	return *this;
}

float4x4	float4x4::operator*( float a ) const {
	float4x4	R;
	R.r[0].Set( a*r[0].x, a*r[0].y, a*r[0].z, a*r[0].w );
	R.r[1].Set( a*r[1].x, a*r[1].y, a*r[1].z, a*r[1].w );
	R.r[2].Set( a*r[2].x, a*r[2].y, a*r[2].z, a*r[2].w );
	R.r[3].Set( a*r[3].x, a*r[3].y, a*r[3].z, a*r[3].w );
	return R;
}

float4x4	operator*( float a, const float4x4& b ) {
	float4x4	R;
	R.r[0].Set( a*b.r[0].x, a*b.r[0].y, a*b.r[0].z, a*b.r[0].w );
	R.r[1].Set( a*b.r[1].x, a*b.r[1].y, a*b.r[1].z, a*b.r[1].w );
	R.r[2].Set( a*b.r[2].x, a*b.r[2].y, a*b.r[2].z, a*b.r[2].w );
	R.r[3].Set( a*b.r[3].x, a*b.r[3].y, a*b.r[3].z, a*b.r[3].w );
	return R;
}

float4x4  float4x4::operator*( const float4x4& b ) const {
	float4x4  R;

	R.r[0].x = r[0].x * b.r[0].x + r[0].y * b.r[1].x + r[0].z * b.r[2].x + r[0].w * b.r[3].x;
	R.r[0].y = r[0].x * b.r[0].y + r[0].y * b.r[1].y + r[0].z * b.r[2].y + r[0].w * b.r[3].y;
	R.r[0].z = r[0].x * b.r[0].z + r[0].y * b.r[1].z + r[0].z * b.r[2].z + r[0].w * b.r[3].z;
	R.r[0].w = r[0].x * b.r[0].w + r[0].y * b.r[1].w + r[0].z * b.r[2].w + r[0].w * b.r[3].w;

	R.r[1].x = r[1].x * b.r[0].x + r[1].y * b.r[1].x + r[1].z * b.r[2].x + r[1].w * b.r[3].x;
	R.r[1].y = r[1].x * b.r[0].y + r[1].y * b.r[1].y + r[1].z * b.r[2].y + r[1].w * b.r[3].y;
	R.r[1].z = r[1].x * b.r[0].z + r[1].y * b.r[1].z + r[1].z * b.r[2].z + r[1].w * b.r[3].z;
	R.r[1].w = r[1].x * b.r[0].w + r[1].y * b.r[1].w + r[1].z * b.r[2].w + r[1].w * b.r[3].w;

	R.r[2].x = r[2].x * b.r[0].x + r[2].y * b.r[1].x + r[2].z * b.r[2].x + r[2].w * b.r[3].x;
	R.r[2].y = r[2].x * b.r[0].y + r[2].y * b.r[1].y + r[2].z * b.r[2].y + r[2].w * b.r[3].y;
	R.r[2].z = r[2].x * b.r[0].z + r[2].y * b.r[1].z + r[2].z * b.r[2].z + r[2].w * b.r[3].z;
	R.r[2].w = r[2].x * b.r[0].w + r[2].y * b.r[1].w + r[2].z * b.r[2].w + r[2].w * b.r[3].w;

	R.r[3].x = r[3].x * b.r[0].x + r[3].y * b.r[1].x + r[3].z * b.r[2].x + r[3].w * b.r[3].x;
	R.r[3].y = r[3].x * b.r[0].y + r[3].y * b.r[1].y + r[3].z * b.r[2].y + r[3].w * b.r[3].y;
	R.r[3].z = r[3].x * b.r[0].z + r[3].y * b.r[1].z + r[3].z * b.r[2].z + r[3].w * b.r[3].z;
	R.r[3].w = r[3].x * b.r[0].w + r[3].y * b.r[1].w + r[3].z * b.r[2].w + r[3].w * b.r[3].w;

	return R;
}

float&	float4x4::operator()( int _row, int _column ) {
	bfloat4&	row = r[_row&3];
	switch ( _column&3 ) {
		case 0: return row.x;
		case 1: return row.y;
		case 2: return row.z;
		case 3: return row.w;
	}
	return *((float*) 0);
}

float4x4::operator float3x3() const {
	float3x3	R;
	R.r[0].Set( r[0].x, r[0].y, r[0].z );
	R.r[1].Set( r[1].x, r[1].y, r[1].z );
	R.r[2].Set( r[2].x, r[2].y, r[2].z );
	return R;
}


//////////////////////////////////////////////////////////////////////////
// Half floats encoding
const float	half::SMALLEST = 6.1035156e-005f;	// The smallest encodable half float

#define F16_EXPONENT_BITS 0x1F
#define F16_EXPONENT_SHIFT 10
#define F16_EXPONENT_BIAS 15
#define F16_MANTISSA_BITS 0x03ff
#define F16_MANTISSA_SHIFT (23 - F16_EXPONENT_SHIFT)
#define F16_MAX_EXPONENT (F16_EXPONENT_BITS << F16_EXPONENT_SHIFT)

half::half( float value ) {
	U32 f32 = *((U32*) &value);
	raw = 0;

	// Decode IEEE 754 little-endian 32-bit floating-point value
	int sign = (f32 >> 16) & 0x8000;
	// Map exponent to the range [-127,128]
	int exponent = ((f32 >> 23) & 0xff) - 127;
	int mantissa = f32 & 0x007fffff;
	if ( exponent == 128 ) {
		// Infinity or NaN
		raw = U16( sign | F16_MAX_EXPONENT );
		if ( mantissa != 0 ) raw |= (mantissa & F16_MANTISSA_BITS);
	} else if ( exponent > 15 ) {
		// Overflow - flush to Infinity
		raw = U16( sign | F16_MAX_EXPONENT );
	} else if ( exponent > -15 ) {
		// Representable value
		exponent += F16_EXPONENT_BIAS;
		mantissa >>= F16_MANTISSA_SHIFT;
		raw = U16( sign | exponent << F16_EXPONENT_SHIFT | mantissa );
	} else {
		raw = U16(sign);
	}
}

half::operator float() const {
	union {
		float   f;
		U32	 ui;
	} f32;

	int sign = (raw & 0x8000) << 15;
	int exponent = (raw & 0x7c00) >> 10;
	int mantissa = (raw & 0x03ff);

	f32.f = 0.0f;
	if ( exponent == 0 ) {
		if ( mantissa != 0 ) 
			f32.f = mantissa / float(1 << 24);
	} else if ( exponent == 31 ) {
		f32.ui = sign | 0x7f800000 | mantissa;
	} else {
		float scale, decimal;
		exponent -= 15;
		if ( exponent < 0 ) {
			scale = float( 1.0 / (1 << -exponent) );
		} else {
			scale = float( 1 << exponent );
		}
		decimal = 1.0f + (float) mantissa / (1 << 10);
		f32.f = scale * decimal;
	}
	
	if ( sign != 0 )
		f32.f = -f32.f;

	return f32.f;
}
