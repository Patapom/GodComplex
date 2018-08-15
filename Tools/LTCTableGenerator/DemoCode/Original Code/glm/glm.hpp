#pragma once

#include <math.h>

namespace glm
{
	struct vec3 {
		float	x, y, z;
		vec3() {}
		vec3( float _x, float _y, float _z ) : x(_x), y(_y), z(_z) {}
		float&	operator[]( int i ) {
			switch ( i%3 ) {
			case 0: return x;
			case 1: return y;
			case 2: return z;
			}
			return x;
		}
		float	operator[]( int i ) const {
			switch ( i%3 ) {
			case 0: return x;
			case 1: return y;
			case 2: return z;
			}
		}
	};

	vec3	operator+( const vec3& a, const vec3& b ) {
		return vec3( a.x+b.x, a.y+b.y, a.z+b.z );
	}
	vec3	operator-( const vec3& a, const vec3& b ) {
		return vec3( a.x-b.x, a.y-b.y, a.z-b.z );
	}
	vec3	operator*( float a, const vec3& b ) {
		return vec3( a*b.x, a*b.y, a*b.z );
	}

	float	dot( const vec3& a, const vec3& b ) {
		return a.x*b.x + a.y*b.y + a.z*b.z;
	}
	float	length( const vec3& v ) {
		return sqrtf( v.x*v.x + v.y*v.y + v.z*v.z );
	}
	vec3	normalize( const vec3& v ) {
		vec3	r = v;
		float	f = 1.0f / length(v);
		r.x *= f;
		r.y *= f;
		r.z *= f;
		return r;
	}

	struct mat3 {
		vec3	value[3];

		mat3() {
			value[0] = vec3( 1, 0, 0 );
			value[1] = vec3( 0, 1, 0 );
			value[2] = vec3( 0, 0, 1 );
		}
		mat3(
			float x0, float y0, float z0,
			float x1, float y1, float z1,
			float x2, float y2, float z2) {
			value[0] = vec3(x0, y0, z0);
			value[1] = vec3(x1, y1, z1);
			value[2] = vec3(x2, y2, z2);
		}
		mat3(
			vec3 const& v0,
			vec3 const& v1,
			vec3 const& v2) {
			value[0] = v0;
			value[1] = v1;
			value[2] = v2;
		}
		vec3&	operator[]( int i ) { return value[i%3]; }
		vec3	operator[]( int i ) const { return value[i%3]; }
	};

	vec3 operator*( vec3 const& v, mat3 const& m ) {
		return vec3(
			m[0][0] * v.x + m[0][1] * v.y + m[0][2] * v.z,
			m[1][0] * v.x + m[1][1] * v.y + m[1][2] * v.z,
			m[2][0] * v.x + m[2][1] * v.y + m[2][2] * v.z );
	}
	vec3 operator*( mat3 const& m, vec3 const& v ) {
		return vec3(
			m[0][0] * v.x + m[1][0] * v.y + m[2][0] * v.z,
			m[0][1] * v.x + m[1][1] * v.y + m[2][1] * v.z,
			m[0][2] * v.x + m[1][2] * v.y + m[2][2] * v.z );
	}
	mat3 operator*( mat3 const& m1, mat3 const& m2 ) {
		float const SrcA00 = m1[0][0];
		float const SrcA01 = m1[0][1];
		float const SrcA02 = m1[0][2];
		float const SrcA10 = m1[1][0];
		float const SrcA11 = m1[1][1];
		float const SrcA12 = m1[1][2];
		float const SrcA20 = m1[2][0];
		float const SrcA21 = m1[2][1];
		float const SrcA22 = m1[2][2];

		float const SrcB00 = m2[0][0];
		float const SrcB01 = m2[0][1];
		float const SrcB02 = m2[0][2];
		float const SrcB10 = m2[1][0];
		float const SrcB11 = m2[1][1];
		float const SrcB12 = m2[1][2];
		float const SrcB20 = m2[2][0];
		float const SrcB21 = m2[2][1];
		float const SrcB22 = m2[2][2];

		mat3	Result;
		Result[0][0] = SrcA00 * SrcB00 + SrcA10 * SrcB01 + SrcA20 * SrcB02;
		Result[0][1] = SrcA01 * SrcB00 + SrcA11 * SrcB01 + SrcA21 * SrcB02;
		Result[0][2] = SrcA02 * SrcB00 + SrcA12 * SrcB01 + SrcA22 * SrcB02;
		Result[1][0] = SrcA00 * SrcB10 + SrcA10 * SrcB11 + SrcA20 * SrcB12;
		Result[1][1] = SrcA01 * SrcB10 + SrcA11 * SrcB11 + SrcA21 * SrcB12;
		Result[1][2] = SrcA02 * SrcB10 + SrcA12 * SrcB11 + SrcA22 * SrcB12;
		Result[2][0] = SrcA00 * SrcB20 + SrcA10 * SrcB21 + SrcA20 * SrcB22;
		Result[2][1] = SrcA01 * SrcB20 + SrcA11 * SrcB21 + SrcA21 * SrcB22;
		Result[2][2] = SrcA02 * SrcB20 + SrcA12 * SrcB21 + SrcA22 * SrcB22;
		return Result;
	}

	static float	determinant( mat3 const& m ) {
		return
			+ m[0][0] * (m[1][1] * m[2][2] - m[2][1] * m[1][2])
			- m[1][0] * (m[0][1] * m[2][2] - m[2][1] * m[0][2])
			+ m[2][0] * (m[0][1] * m[1][2] - m[1][1] * m[0][2]);
	}

	static mat3	inverse( mat3 const& m ) {
		float OneOverDeterminant = 1 / determinant( m );

		mat3	Inverse;
		Inverse[0][0] = + (m[1][1] * m[2][2] - m[2][1] * m[1][2]) * OneOverDeterminant;
		Inverse[1][0] = - (m[1][0] * m[2][2] - m[2][0] * m[1][2]) * OneOverDeterminant;
		Inverse[2][0] = + (m[1][0] * m[2][1] - m[2][0] * m[1][1]) * OneOverDeterminant;
		Inverse[0][1] = - (m[0][1] * m[2][2] - m[2][1] * m[0][2]) * OneOverDeterminant;
		Inverse[1][1] = + (m[0][0] * m[2][2] - m[2][0] * m[0][2]) * OneOverDeterminant;
		Inverse[2][1] = - (m[0][0] * m[2][1] - m[2][0] * m[0][1]) * OneOverDeterminant;
		Inverse[0][2] = + (m[0][1] * m[1][2] - m[1][1] * m[0][2]) * OneOverDeterminant;
		Inverse[1][2] = - (m[0][0] * m[1][2] - m[1][0] * m[0][2]) * OneOverDeterminant;
		Inverse[2][2] = + (m[0][0] * m[1][1] - m[1][0] * m[0][1]) * OneOverDeterminant;

		return Inverse;
	}
}
