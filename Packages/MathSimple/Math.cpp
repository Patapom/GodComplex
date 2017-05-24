#include "Math.h"

using namespace SharpMath;

float3	float2::Cross( float2 b ) {
	return float3( 0.0f, 0.0f, x * b.y - y * b.x );
}

bool	float2::TryParse( String^ _stringValue, float2% _value ) {
	_stringValue = _stringValue->Trim();
	if ( !_stringValue->StartsWith( "{" )
		|| !_stringValue->EndsWith( "}" ) ) {
		return false;
	}
	array<String^>^	components = _stringValue->Substring( 1, _stringValue->Length-2 )->Split( ',' );
	if ( components->Length != 2 )
		return false;

	if ( !float::TryParse( components[0], _value.x ) )
		return false;
	if ( !float::TryParse( components[1], _value.y ) )
		return false;

	return true;
}

bool	float3::TryParse( String^ _stringValue, float3% _value ) {
	_stringValue = _stringValue->Trim();
	if ( !_stringValue->StartsWith( "{" )
		|| !_stringValue->EndsWith( "}" ) ) {
		return false;
	}
	array<String^>^	components = _stringValue->Substring( 1, _stringValue->Length-2 )->Split( ',' );
	if ( components->Length != 3 )
		return false;

	if ( !float::TryParse( components[0], _value.x ) )
		return false;
	if ( !float::TryParse( components[1], _value.y ) )
		return false;
	if ( !float::TryParse( components[2], _value.z ) )
		return false;

	return true;
}

bool	float4::TryParse( String^ _stringValue, float4% _value ) {
	_stringValue = _stringValue->Trim();
	if ( !_stringValue->StartsWith( "{" )
		|| !_stringValue->EndsWith( "}" ) ) {
		return false;
	}
	array<String^>^	components = _stringValue->Substring( 1, _stringValue->Length-2 )->Split( ',' );
	if ( components->Length != 4 )
		return false;

	if ( !float::TryParse( components[0], _value.x ) )
		return false;
	if ( !float::TryParse( components[1], _value.y ) )
		return false;
	if ( !float::TryParse( components[2], _value.z ) )
		return false;
	if ( !float::TryParse( components[3], _value.w ) )
		return false;

	return true;
}


#ifdef _DEBUG

//////////////////////////////////////////////////////////////////////////
// Test methods compile
void	TestFloat4x4() {

	float4x4	test0;
	float4x4	test1( gcnew array<float>( 16 ) { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 } );
	float4x4	test2( float4( 1, 0, 0, 0 ), float4( 0, 1, 0, 0 ), float4( 0, 0, 1, 0 ), float4( 0, 0, 0, 1 ) );
	float4x4	test3( 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 );
	test2.Scale( float3( 2, 2, 2 ) );
	float3x3	cast = (float3x3) test3;
	float4x4	mul0 = test1 * test2;
	float4x4	mul1 = 3.0f * test2;
	float4		mul2 = float4( 1, 1, 1, 1 ) * test3;
	float4^		access0 = test3[2];
	float		access1 = test3[1,2];
	float		coFactor = test3.CoFactor( 0, 2 );
	float		det = test3.Determinant;
	float4x4	inv = test2.Inverse;
	float4x4	id = float4x4::Identity;

	test3.BuildRotLeftHanded( float3::UnitZ, float3::Zero, float3::UnitY );
	test3.BuildRotRightHanded( float3::UnitZ, float3::Zero, float3::UnitY );
	test3.BuildProjectionPerspective( 1.2f, 2.0f, 0.01f, 10.0f );
	test3.BuildRotationX( 0.5f );
	test3.BuildRotationY( 0.5f );
	test3.BuildRotationZ( 0.5f );
	test3.BuildFromAngleAxis( 0.5f, float3::UnitY );

}

#endif
