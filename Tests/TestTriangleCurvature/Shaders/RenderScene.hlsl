//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

static const float3	LIGHT_POSITION = float3( 0, 4, 2 );
static const float3	LIGHT_COLOR = 50.0;

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	wsPosition : POSITION;
	float3	wsNormal : NORMAL;

	float	sphereRadius : RADIUS;
	float3	wsFaceNormal : FACE_NORMAL;
	float3	wsFaceCenter: FACE_CENTER;

	float2	UV : TEXCOORD0;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;	// Assume already in world space
	Out.wsNormal = _In.Normal;
	Out.wsFaceNormal = _In.Tangent;
	Out.wsFaceCenter = _In.BiTangent;
	Out.UV = _In.UV;

	// Compute sphere radius
	float	NdotNt = dot( Out.wsNormal, Out.wsFaceNormal );
	float	cosTheta = sqrt( 1.0 - NdotNt * NdotNt );
	Out.sphereRadius = length( Out.wsPosition - Out.wsFaceCenter ) / max( 1e-6, cosTheta );
//Out.sphereRadius = length( Out.wsPosition - Out.wsFaceCenter );
//Out.sphereRadius = 0.5;

	return Out;
}

float	NDF_GGX( float _cosTheta, float _alpha ) {
	float	a2 = _alpha * _alpha;
	float	c2 = _cosTheta * _cosTheta;
	float	k  = 1 + c2 * (a2 - 1.0);
	return a2 / (PI * k * k);
}
float	SmithG( float _cosTheta, float _alpha ) {
	float	a2 = _alpha * _alpha;
	float	c = saturate( _cosTheta );
	float	c2 = c * c;
	return 2.0 * c / (c + sqrt( c2 + a2 * (1 - c2) ));
}


float	OffsetPosition( inout float3 _wsPosition, float3 _wsNormal, float _sphereRadius, float3 _wsFaceCenter, float3 _wsFaceNormal ) {
	float3	P = _wsPosition;
	float3	N = _wsNormal;
	float3	Pt = _wsFaceCenter;
	float3	Nt = _wsFaceNormal;

	// First, we need to find the center of the sphere along the line (Pt, Nt) (i.e. the line passing through the center of the triangle and perpendicular to it)
	// We do that by expressing the distance between the lines (P,N) and (Pt,Nt) and finding the minimum
	// We write:
	//	P(t) = P + N * t
	//	P'(s) = Pt + Nt * s
	//
	// We can also write some representation of s as a function of t by projecting P(t) onto the (Pt,Nt) line:
	//	s(t) = [(P + N * t) - Pt].Nt = (P-Pt).Nt + (N.Nt) * t = (N.Nt) * t    (because (P-Pt).Nt = 0 since P and Pt both are on the triangle)
	//
	// Rewriting
	//	P'(s(t)) = Pt + (N.Nt) * Nt * t
	//
	// We write distance d(t)² = [P(t) - P'(s(t))]²
	//
	//	d(t)² = [(P + N * t) - (Pt + [(N.Nt) * Nt] * t)]²
	//		  = [(P-Pt) + [N - (N.Nt) * Nt] * t]²
	//
	// Let A = [N - (N.Nt) * Nt]
	// Let D = P - Pt
	//
	//	d(t)² = [D + A * t]²
	//		  = D.D + 2 * D.A * t + A.A * t²
	//
	// We see that the distance is varying as a quadratic function and we're looking for the minimum:
	//
	//	f(t)   = d(t)² = c + b*t + a*t²
	//	f'(t)  = 2at + b	<== Minimum occurs at f'(t) = 0 which always occurs unless a = 0 that happens when N = Nt
	//	f''(t) = 2a			<== Also, this must always be positive since we're expecting a downward parabola: distance is decreasing to reach minimum then increasing again!
	//
	float3	D = _wsPosition - _wsFaceCenter;
	float	NdotNt = dot( N, Nt );

//return dot( D, Nt ) == 0.0 ? 1.0 : 0.0;
//	float3	A = N - NdotNt * Nt;
//	float	a = dot( A, A );	// = [N - (N.Nt) * Nt].[N - (N.Nt) * Nt] = N.N - 2 * (N.Nt)² + (N.Nt)² * (Nt.Nt) = 1 - (N.Nt)²  (assuming N and Nt are normalized)
//	float	b = dot( A, D );	// = 2 * [N - (N.Nt) * Nt].D = 2 * N.D  (since we showed that D.Nt = 0)

	float	a = 1.0 - NdotNt*NdotNt;
	float	b = dot( N, D );
//return step( 0.0, a );	// Make sure always positive!
//	float	c = dot( D, D );	// Not used here anyway

	float	t_min = -b / a;	// Here we see that t can go to infinity when a = 1 - (N.Nt)² --> 0 which occurs when normals are equal (infinite curvature of the planar triangle!)
	float3	Ct = P + t_min * N;
//return abs(t_min);
//return -t_min;

	// Now we can compute C, the center of the sphere along the (Pt,Nt) line:
//	float	s_min = dot( P + t_min * N - Pt, Nt );	// We retrieve s(t_min) by projecting P(t_min) onto the (Pt,Nt) line... Note that s(t_min) must always be <= 0
	float	s_min = t_min * NdotNt;
//s_min = -1.0;
	float3	C = Pt + s_min * Nt;
//return length( Ct - C );
//return -s_min;

	// Finally, with our sphere center located, we then choose one of 2 solutions:
	#if 0
		//	1) Project P onto the sphere, simple and cheap but will move the position along the triangle's tangent space which is not ideal
		//
		float3	M = P - C;					// Move radially
		float	d = length( M );			// Distance from the sphere's center
				M /= d;						// Normalized displacement vector				
		float	offset = _sphereRadius - d;	// How much do we need to move to reach the sphere?

	#else
		//	2) Project P orthogonally onto the sphere following the flat triangle's normal Nt, which is more complicated but keeps the tangential position at the same place
		//
		// To do this, we need to find the intersection of a line (P,Nt) parallel to the (Pt,Nt) line that intersects the sphere (C,R).
		// Once again we write P(t) = P + t * Nt
		// And:
		//	[P(t) - C]² = [(P-C) + Nt * t]² = [D + Nt * t]² = R²
		//
		// Expanding the expression:
		//	D.D + 2*D.Nt*t + Nt.Nt*t² = R²
		//
		// Letting:
		//	a = Nt.Nt = 1
		//	b = D.Nt
		//	c = D.D - R²
		//
		// We get the classical a + 2 b t + c t² = 0
		//
		float3	M = Nt;						// Move orthogonally to the triangle
		D = P - C;
		b = dot( D, Nt );
//return step( 0.0, b );
		float	c = dot( D, D ) - _sphereRadius * _sphereRadius;
//return step( c, 0.0 );
		float	delta = b*b - c;
		float	offset = -b + sqrt( delta );
//offset = 0.0;
	#endif

	_wsPosition += offset * M;

	return offset;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {

	float3	wsPosition = _In.wsPosition;
	float3	wsNormal = normalize( _In.wsNormal );

//wsNormal = _In.wsFaceNormal;

	float	offset = OffsetPosition( wsPosition, wsNormal, _In.sphereRadius, _In.wsFaceCenter, _In.wsFaceNormal );
//return offset;
//return 0.25 * _In.sphereRadius;


	float3	wsView = normalize( _Camera2World[3].xyz - wsPosition );
	float3	wsLight = LIGHT_POSITION - wsPosition;
	float	distance2Light = length( wsLight );
			wsLight /= distance2Light;

	float3	H = normalize( wsLight + wsView );
	float	NdotH = saturate( dot( wsNormal, H ) );
	float	NdotL = dot( wsNormal, wsLight );
	float	NdotV = dot( wsNormal, wsView );

	const float3	F0 = 0.04;
	const float3	RhoD = float3( 0.05, 0.2, 0.6 );
	const float		roughness = 0.15;

	float3	F = FresnelAccurate( Fresnel_IORFromF0( F0 ), NdotH );
	float	G = SmithG( NdotL, roughness ) * SmithG( NdotV, roughness );
	float	D = NDF_GGX( NdotH, roughness );

	float3	Lin = LIGHT_COLOR / (distance2Light * distance2Light);
	float3	specularBRDF = F * G * D / (4.0 * NdotL * NdotV);
	float3	diffuseBRDF = (1.0 - F) * (INVPI * RhoD);
	float3	indirectDiffuse = 0.3 * (INVPI * RhoD);	// Should be coming from a pre-filtered cube map or something
	float3	indirectSpecular = 0.0;					// Should be coming from a pre-filtered cube map or something
	float3	Lout = (diffuseBRDF + specularBRDF) * saturate( NdotL ) * Lin
				 + indirectDiffuse
				 + indirectSpecular;

	return Lout;
}
